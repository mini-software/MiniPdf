package io.github.minisoftware.minipdf.internal.pptx;

import io.github.minisoftware.minipdf.ConversionOptions;
import io.github.minisoftware.minipdf.MiniPdfException;
import io.github.minisoftware.minipdf.PageSize;
import io.github.minisoftware.minipdf.internal.OoxmlPackage;
import io.github.minisoftware.minipdf.internal.SecureXml;
import io.github.minisoftware.minipdf.internal.SimplePdfTextRenderer;

import javax.xml.stream.XMLStreamConstants;
import javax.xml.stream.XMLStreamException;
import javax.xml.stream.XMLStreamReader;
import java.util.ArrayDeque;
import java.util.ArrayList;
import java.util.LinkedHashMap;
import java.util.List;
import java.util.Map;

public final class PptxConverter {
    private static final float EMUS_PER_POINT = 12_700.0f;
    private static final float DEFAULT_SLIDE_WIDTH = 720.0f;
    private static final float DEFAULT_SLIDE_HEIGHT = 540.0f;

    private PptxConverter() {
    }

    public static byte[] convert(byte[] input, ConversionOptions options) throws MiniPdfException {
        OoxmlPackage presentation = OoxmlPackage.open(input);
        byte[] presentationXml = presentation.entry("ppt/presentation.xml")
                .orElseThrow(() -> new MiniPdfException(
                        MiniPdfException.Kind.INVALID_INPUT,
                        "PPTX package does not contain ppt/presentation.xml"));
        PageSize slideSize = readSlideSize(presentationXml);
                List<String> slideNames = readSlideNames(presentation, presentationXml);
        if (slideNames.isEmpty()) {
            throw new MiniPdfException(
                    MiniPdfException.Kind.INVALID_INPUT,
                    "PPTX package does not contain a slide");
        }

        List<List<String>> slides = new ArrayList<>();
        for (String slideName : slideNames) {
            byte[] slideXml = presentation.entry(slideName)
                    .orElseThrow(() -> new MiniPdfException(
                            MiniPdfException.Kind.INVALID_INPUT,
                            "PPTX slide part is missing: " + slideName));
            slides.add(readSlide(slideXml));
        }
        return SimplePdfTextRenderer.renderPages(slides, options, slideSize);
    }

    private static PageSize readSlideSize(byte[] xml) throws MiniPdfException {
        XMLStreamReader reader = null;
        try {
            reader = SecureXml.reader(xml);
            while (reader.hasNext()) {
                int event = reader.next();
                if (event == XMLStreamConstants.START_ELEMENT && reader.getLocalName().equals("sldSz")) {
                    float width = emusToPoints(reader.getAttributeValue(null, "cx"), DEFAULT_SLIDE_WIDTH);
                    float height = emusToPoints(reader.getAttributeValue(null, "cy"), DEFAULT_SLIDE_HEIGHT);
                    return PageSize.of(width, height);
                }
            }
            return PageSize.of(DEFAULT_SLIDE_WIDTH, DEFAULT_SLIDE_HEIGHT);
        } catch (XMLStreamException exception) {
            throw SecureXml.parseError(exception);
        } finally {
            closeReader(reader);
        }
    }

    private static List<String> readSlide(byte[] xml) throws MiniPdfException {
        List<String> paragraphs = new ArrayList<>();
        StringBuilder paragraph = null;
        XMLStreamReader reader = null;
        try {
            reader = SecureXml.reader(xml);
            while (reader.hasNext()) {
                int event = reader.next();
                if (event == XMLStreamConstants.START_ELEMENT && reader.getLocalName().equals("p")) {
                    paragraph = new StringBuilder();
                } else if (event == XMLStreamConstants.START_ELEMENT
                        && reader.getLocalName().equals("t") && paragraph != null) {
                    paragraph.append(reader.getElementText());
                } else if (event == XMLStreamConstants.START_ELEMENT
                        && reader.getLocalName().equals("br") && paragraph != null) {
                    paragraph.append(' ');
                } else if (event == XMLStreamConstants.END_ELEMENT
                        && reader.getLocalName().equals("p") && paragraph != null) {
                    paragraphs.add(paragraph.toString());
                    paragraph = null;
                }
            }
            return paragraphs;
        } catch (XMLStreamException exception) {
            throw SecureXml.parseError(exception);
        } finally {
            closeReader(reader);
        }
    }

    private static List<String> readSlideNames(OoxmlPackage presentation, byte[] presentationXml)
            throws MiniPdfException {
        List<String> relationshipIds = readSlideRelationshipIds(presentationXml);
        if (relationshipIds.isEmpty()) {
            return List.of();
        }
        byte[] relationshipsXml = presentation.entry("ppt/_rels/presentation.xml.rels")
                .orElseThrow(() -> new MiniPdfException(
                        MiniPdfException.Kind.INVALID_INPUT,
                        "PPTX package does not contain presentation relationships"));
        Map<String, String> relationships = readSlideRelationships(relationshipsXml);
        List<String> slideNames = new ArrayList<>();
        for (String relationshipId : relationshipIds) {
            String target = relationships.get(relationshipId);
            if (target == null) {
                throw new MiniPdfException(
                        MiniPdfException.Kind.INVALID_INPUT,
                        "PPTX slide relationship is missing: " + relationshipId);
            }
            slideNames.add(resolvePartName("ppt/presentation.xml", target));
        }
        return slideNames;
    }

    private static List<String> readSlideRelationshipIds(byte[] xml) throws MiniPdfException {
        List<String> relationshipIds = new ArrayList<>();
        XMLStreamReader reader = null;
        try {
            reader = SecureXml.reader(xml);
            while (reader.hasNext()) {
                int event = reader.next();
                if (event == XMLStreamConstants.START_ELEMENT && reader.getLocalName().equals("sldId")) {
                    String relationshipId = namespacedAttribute(reader, "id");
                    if (relationshipId != null) {
                        relationshipIds.add(relationshipId);
                    }
                }
            }
            return relationshipIds;
        } catch (XMLStreamException exception) {
            throw SecureXml.parseError(exception);
        } finally {
            closeReader(reader);
        }
    }

    private static Map<String, String> readSlideRelationships(byte[] xml) throws MiniPdfException {
        Map<String, String> relationships = new LinkedHashMap<>();
        XMLStreamReader reader = null;
        try {
            reader = SecureXml.reader(xml);
            while (reader.hasNext()) {
                int event = reader.next();
                if (event != XMLStreamConstants.START_ELEMENT
                        || !reader.getLocalName().equals("Relationship")) {
                    continue;
                }
                String type = attribute(reader, "Type");
                String targetMode = attribute(reader, "TargetMode");
                if (type == null || !type.endsWith("/slide")
                        || "external".equalsIgnoreCase(targetMode)) {
                    continue;
                }
                String relationshipId = attribute(reader, "Id");
                String target = attribute(reader, "Target");
                if (relationshipId != null && target != null) {
                    relationships.put(relationshipId, target);
                }
            }
            return relationships;
        } catch (XMLStreamException exception) {
            throw SecureXml.parseError(exception);
        } finally {
            closeReader(reader);
        }
    }

    private static float emusToPoints(String value, float defaultValue) {
        if (value == null) {
            return defaultValue;
        }
        try {
            long emus = Long.parseLong(value);
            return emus > 0 ? emus / EMUS_PER_POINT : defaultValue;
        } catch (NumberFormatException ignored) {
            return defaultValue;
        }
    }

    private static String attribute(XMLStreamReader reader, String localName) {
        for (int index = 0; index < reader.getAttributeCount(); index++) {
            if (reader.getAttributeLocalName(index).equals(localName)) {
                return reader.getAttributeValue(index);
            }
        }
        return null;
    }

    private static String namespacedAttribute(XMLStreamReader reader, String localName) {
        for (int index = 0; index < reader.getAttributeCount(); index++) {
            String namespace = reader.getAttributeNamespace(index);
            if (namespace != null && !namespace.isEmpty()
                    && reader.getAttributeLocalName(index).equals(localName)) {
                return reader.getAttributeValue(index);
            }
        }
        return null;
    }

    private static String resolvePartName(String sourcePart, String target) throws MiniPdfException {
        String normalizedTarget = target.replace('\\', '/');
        String sourceDirectory = sourcePart.substring(0, sourcePart.lastIndexOf('/') + 1);
        String combined = normalizedTarget.startsWith("/")
                ? normalizedTarget.substring(1)
                : sourceDirectory + normalizedTarget;
        ArrayDeque<String> segments = new ArrayDeque<>();
        for (String segment : combined.split("/")) {
            if (segment.isEmpty() || segment.equals(".")) {
                continue;
            }
            if (segment.equals("..")) {
                if (segments.isEmpty()) {
                    throw new MiniPdfException(
                            MiniPdfException.Kind.INVALID_INPUT,
                            "PPTX relationship target escapes the package: " + target);
                }
                segments.removeLast();
            } else {
                segments.addLast(segment);
            }
        }
        return String.join("/", segments);
    }

    private static void closeReader(XMLStreamReader reader) {
        if (reader == null) {
            return;
        }
        try {
            reader.close();
        } catch (XMLStreamException ignored) {
        }
    }
}