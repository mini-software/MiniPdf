package io.github.minisoftware.minipdf.internal.xlsx;

import io.github.minisoftware.minipdf.MiniPdfException;
import io.github.minisoftware.minipdf.internal.SecureXml;
import org.apache.poi.hemf.usermodel.HemfPicture;

import javax.imageio.ImageIO;
import javax.xml.stream.XMLStreamConstants;
import javax.xml.stream.XMLStreamException;
import javax.xml.stream.XMLStreamReader;
import java.awt.Color;
import java.awt.Graphics2D;
import java.awt.RenderingHints;
import java.awt.geom.Dimension2D;
import java.awt.geom.Rectangle2D;
import java.awt.image.BufferedImage;
import java.io.ByteArrayInputStream;
import java.io.ByteArrayOutputStream;
import java.io.IOException;
import java.nio.file.Path;
import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.Locale;
import java.util.Map;
import java.util.zip.ZipEntry;
import java.util.zip.ZipInputStream;

final class LegacyVmlPictureReader {
    private static final float EMF_RASTER_DPI = 300.0f;

    private LegacyVmlPictureReader() {
    }

    static Map<String, List<LegacyPicture>> read(byte[] input) throws MiniPdfException {
        Map<String, byte[]> entries = zipEntries(input);
        Map<String, List<LegacyPicture>> pictures = new HashMap<>();
        for (Map.Entry<String, byte[]> entry : entries.entrySet()) {
            String relationshipPath = entry.getKey();
            if (!relationshipPath.startsWith("xl/worksheets/_rels/")
                    || !relationshipPath.endsWith(".xml.rels")) {
                continue;
            }
            String sheetPath = "xl/worksheets/"
                    + relationshipPath.substring("xl/worksheets/_rels/".length(), relationshipPath.length() - 5);
            for (Relationship relationship : relationships(entry.getValue())) {
                if (!relationship.type().endsWith("/vmlDrawing")) {
                    continue;
                }
                String vmlPath = resolve(sheetPath, relationship.target());
                byte[] vml = entries.get(vmlPath);
                if (vml == null) {
                    continue;
                }
                Map<String, String> imageTargets = new HashMap<>();
                byte[] vmlRelationships = entries.get(relationshipPath(vmlPath));
                if (vmlRelationships != null) {
                    for (Relationship imageRelationship : relationships(vmlRelationships)) {
                        if (imageRelationship.type().endsWith("/image")) {
                            imageTargets.put(
                                    imageRelationship.id(),
                                    resolve(vmlPath, imageRelationship.target()));
                        }
                    }
                }
                List<LegacyPicture> sheetPictures = parseVml(vml, imageTargets, entries);
                if (!sheetPictures.isEmpty()) {
                    pictures.computeIfAbsent(sheetPath, ignored -> new ArrayList<>()).addAll(sheetPictures);
                }
            }
        }
        return pictures;
    }

    private static Map<String, byte[]> zipEntries(byte[] input) throws MiniPdfException {
        Map<String, byte[]> entries = new HashMap<>();
        try (ZipInputStream archive = new ZipInputStream(new ByteArrayInputStream(input))) {
            ZipEntry entry;
            while ((entry = archive.getNextEntry()) != null) {
                if (!entry.isDirectory()) {
                    entries.put(entry.getName(), archive.readAllBytes());
                }
            }
            return entries;
        } catch (IOException exception) {
            throw new MiniPdfException(
                    MiniPdfException.Kind.ZIP_PACKAGE,
                    "unable to read XLSX package: " + exception.getMessage(),
                    exception);
        }
    }

    private static List<Relationship> relationships(byte[] xml) throws MiniPdfException {
        List<Relationship> relationships = new ArrayList<>();
        try {
            XMLStreamReader reader = SecureXml.reader(xml);
            while (reader.hasNext()) {
                if (reader.next() == XMLStreamConstants.START_ELEMENT
                        && reader.getLocalName().equals("Relationship")) {
                    relationships.add(new Relationship(
                            attribute(reader, "Id"),
                            attribute(reader, "Type"),
                            attribute(reader, "Target")));
                }
            }
            reader.close();
            return relationships;
        } catch (XMLStreamException exception) {
            throw SecureXml.parseError(exception);
        }
    }

    private static List<LegacyPicture> parseVml(
            byte[] xml,
            Map<String, String> imageTargets,
            Map<String, byte[]> entries) throws MiniPdfException {
        List<LegacyPicture> pictures = new ArrayList<>();
        String relationId = null;
        float cropTop = 0.0f;
        float cropBottom = 0.0f;
        float cropLeft = 0.0f;
        float cropRight = 0.0f;
        try {
            XMLStreamReader reader = SecureXml.reader(xml);
            while (reader.hasNext()) {
                int event = reader.next();
                if (event == XMLStreamConstants.START_ELEMENT && reader.getLocalName().equals("shape")) {
                    relationId = null;
                    cropTop = cropBottom = cropLeft = cropRight = 0.0f;
                } else if (event == XMLStreamConstants.START_ELEMENT
                        && reader.getLocalName().equals("imagedata")) {
                    relationId = attribute(reader, "relid");
                    cropTop = crop(attribute(reader, "croptop"));
                    cropBottom = crop(attribute(reader, "cropbottom"));
                    cropLeft = crop(attribute(reader, "cropleft"));
                    cropRight = crop(attribute(reader, "cropright"));
                } else if (event == XMLStreamConstants.START_ELEMENT
                        && reader.getLocalName().equals("Anchor") && relationId != null) {
                    int[] anchor = parseAnchor(reader.getElementText());
                    String imagePath = imageTargets.get(relationId);
                    byte[] image = imagePath == null ? null : entries.get(imagePath);
                    if (anchor != null && image != null) {
                        pictures.add(new LegacyPicture(
                                image,
                                imagePath,
                                anchor,
                                cropTop,
                                cropBottom,
                                cropLeft,
                                cropRight));
                    }
                }
            }
            reader.close();
            return pictures;
        } catch (XMLStreamException exception) {
            throw SecureXml.parseError(exception);
        }
    }

    private static String attribute(XMLStreamReader reader, String localName) {
        for (int index = 0; index < reader.getAttributeCount(); index++) {
            if (reader.getAttributeLocalName(index).equals(localName)) {
                return reader.getAttributeValue(index);
            }
        }
        return "";
    }

    private static int[] parseAnchor(String value) {
        String[] values = value.split(",");
        if (values.length != 8) {
            return null;
        }
        int[] anchor = new int[8];
        try {
            for (int index = 0; index < values.length; index++) {
                anchor[index] = Integer.parseInt(values[index].trim());
            }
            return anchor;
        } catch (NumberFormatException ignored) {
            return null;
        }
    }

    private static float crop(String value) {
        if (value == null || value.isBlank()) {
            return 0.0f;
        }
        String normalized = value.toLowerCase(Locale.ROOT).endsWith("f")
                ? value.substring(0, value.length() - 1)
                : value;
        try {
            return Math.max(0.0f, Math.min(1.0f, Integer.parseInt(normalized) / 65_536.0f));
        } catch (NumberFormatException ignored) {
            return 0.0f;
        }
    }

    private static String resolve(String source, String target) {
        Path parent = Path.of(source).getParent();
        return parent.resolve(target.replace('/', java.io.File.separatorChar))
                .normalize()
                .toString()
                .replace('\\', '/');
    }

    private static String relationshipPath(String partPath) {
        Path path = Path.of(partPath);
        return path.getParent().resolve("_rels").resolve(path.getFileName() + ".rels")
                .toString()
                .replace('\\', '/');
    }

    record LegacyPicture(
            byte[] data,
            String path,
            int[] anchor,
            float cropTop,
            float cropBottom,
            float cropLeft,
            float cropRight) {
        byte[] png() throws IOException {
            if (!path.toLowerCase(Locale.ROOT).endsWith(".emf")) {
                return data;
            }
            HemfPicture picture = new HemfPicture(new ByteArrayInputStream(data));
            Dimension2D size = picture.getSize();
            int width = Math.max(1, (int) Math.ceil(size.getWidth() * EMF_RASTER_DPI / 72.0f));
            int height = Math.max(1, (int) Math.ceil(size.getHeight() * EMF_RASTER_DPI / 72.0f));
            BufferedImage image = new BufferedImage(width, height, BufferedImage.TYPE_INT_ARGB);
            Graphics2D graphics = image.createGraphics();
            try {
                graphics.setColor(Color.WHITE);
                graphics.fillRect(0, 0, width, height);
                graphics.setRenderingHint(RenderingHints.KEY_ANTIALIASING, RenderingHints.VALUE_ANTIALIAS_ON);
                picture.draw(graphics, new Rectangle2D.Double(0, 0, width, height));
            } finally {
                graphics.dispose();
            }
            ByteArrayOutputStream output = new ByteArrayOutputStream();
            ImageIO.write(image, "png", output);
            return output.toByteArray();
        }
    }

    private record Relationship(String id, String type, String target) {
    }
}