import java.awt.image.BufferedImage;
import java.io.BufferedReader;
import java.io.ByteArrayOutputStream;
import java.io.DataOutputStream;
import java.io.File;
import java.io.IOException;
import java.io.InputStreamReader;
import java.nio.charset.StandardCharsets;
import java.util.ArrayList;
import java.util.Base64;
import java.util.List;

import org.apache.pdfbox.Loader;
import org.apache.pdfbox.pdmodel.PDDocument;
import org.apache.pdfbox.rendering.ImageType;
import org.apache.pdfbox.rendering.PDFRenderer;
import org.apache.pdfbox.text.PDFTextStripper;
import org.apache.pdfbox.text.TextPosition;

public final class BenchmarkPdfBox {
    private static void reply(DataOutputStream output, int status, byte[] payload) throws IOException {
        output.writeByte(status);
        output.writeInt(payload.length);
        output.write(payload);
        output.flush();
    }

    private static byte[] text(PDDocument document, int page) throws IOException {
        List<byte[]> spans = new ArrayList<>();
        PDFTextStripper stripper = new PDFTextStripper() {
            @Override
            protected void writeString(String value, List<TextPosition> positions) throws IOException {
                if (positions.isEmpty() || value.trim().isEmpty()) {
                    return;
                }
                TextPosition first = positions.get(0);
                ByteArrayOutputStream buffer = new ByteArrayOutputStream();
                try (DataOutputStream span = new DataOutputStream(buffer)) {
                    span.writeFloat(first.getYDirAdj());
                    span.writeFloat(first.getXDirAdj());
                    byte[] utf8 = value.getBytes(StandardCharsets.UTF_8);
                    span.writeInt(utf8.length);
                    span.write(utf8);
                }
                spans.add(buffer.toByteArray());
            }
        };
        stripper.setSortByPosition(true);
        stripper.setStartPage(page + 1);
        stripper.setEndPage(page + 1);
        stripper.getText(document);
        ByteArrayOutputStream buffer = new ByteArrayOutputStream();
        try (DataOutputStream data = new DataOutputStream(buffer)) {
            data.writeInt(spans.size());
            for (byte[] span : spans) {
                data.write(span);
            }
        }
        return buffer.toByteArray();
    }

    private static byte[] render(PDFRenderer renderer, int page, int dpi) throws IOException {
        if (dpi <= 0) {
            throw new IllegalArgumentException("DPI must be positive");
        }
        BufferedImage image = renderer.renderImageWithDPI(page, dpi, ImageType.RGB);
        ByteArrayOutputStream buffer = new ByteArrayOutputStream();
        try (DataOutputStream data = new DataOutputStream(buffer)) {
            data.writeInt(image.getWidth());
            data.writeInt(image.getHeight());
            int[] row = new int[image.getWidth()];
            for (int y = 0; y < image.getHeight(); y++) {
                image.getRGB(0, y, image.getWidth(), 1, row, 0, image.getWidth());
                for (int pixel : row) {
                    data.writeByte(pixel >> 16);
                    data.writeByte(pixel >> 8);
                    data.writeByte(pixel);
                }
            }
        }
        return buffer.toByteArray();
    }

    public static void main(String[] args) throws IOException {
        DataOutputStream output = new DataOutputStream(System.out);
        String path = new String(Base64.getDecoder().decode(args[0]), StandardCharsets.UTF_8);
        try (PDDocument document = Loader.loadPDF(new File(path))) {
            reply(output, 0, Integer.toString(document.getNumberOfPages()).getBytes(StandardCharsets.US_ASCII));
            PDFRenderer renderer = new PDFRenderer(document);
            BufferedReader input = new BufferedReader(new InputStreamReader(System.in, StandardCharsets.US_ASCII));
            String command;
            while ((command = input.readLine()) != null && !command.equals("close")) {
                String[] parts = command.split(" ");
                int page = Integer.parseInt(parts[1]);
                if (page < 0 || page >= document.getNumberOfPages()) {
                    throw new IllegalArgumentException("Page index out of range: " + page);
                }
                if (parts[0].equals("text")) {
                    reply(output, 0, text(document, page));
                } else if (parts[0].equals("render")) {
                    reply(output, 0, render(renderer, page, Integer.parseInt(parts[2])));
                } else {
                    throw new IllegalArgumentException("Unknown command: " + command);
                }
            }
        } catch (IOException | RuntimeException error) {
            error.printStackTrace(System.err);
            reply(output, 1, error.toString().getBytes(StandardCharsets.UTF_8));
            System.exit(1);
        }
    }
}
