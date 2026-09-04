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
import java.util.ArrayList;
import java.util.Comparator;
import java.util.List;

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
        List<String> slideNames = presentation.entryNames().stream()
                .filter(PptxConverter::isSlide)
                .sorted(Comparator.comparingInt(PptxConverter::slideNumber))
                .toList();
        if (slideNames.isEmpty()) {
            throw new MiniPdfException(
                    MiniPdfException.Kind.INVALID_INPUT,
                    "PPTX package does not contain a slide");
        }

        List<List<String>> slides = new ArrayList<>();
        for (String slideName : slideNames) {
            slides.add(readSlide(presentation.entry(slideName).orElseThrow()));
        }
        return SimplePdfTextRenderer.renderPages(slides, options, slideSize);
    }

    private static PageSize readSlideSize(byte[] xml) throws MiniPdfException {
        try {
            XMLStreamReader reader = SecureXml.reader(xml);
            while (reader.hasNext()) {
                int event = reader.next();
                if (event == XMLStreamConstants.START_ELEMENT && reader.getLocalName().equals("sldSz")) {
                    float width = emusToPoints(reader.getAttributeValue(null, "cx"), DEFAULT_SLIDE_WIDTH);
                    float height = emusToPoints(reader.getAttributeValue(null, "cy"), DEFAULT_SLIDE_HEIGHT);
                    reader.close();
                    return PageSize.of(width, height);
                }
            }
            reader.close();
            return PageSize.of(DEFAULT_SLIDE_WIDTH, DEFAULT_SLIDE_HEIGHT);
        } catch (XMLStreamException exception) {
            throw SecureXml.parseError(exception);
        }
    }

    private static List<String> readSlide(byte[] xml) throws MiniPdfException {
        List<String> paragraphs = new ArrayList<>();
        StringBuilder paragraph = null;
        try {
            XMLStreamReader reader = SecureXml.reader(xml);
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
            reader.close();
            return paragraphs;
        } catch (XMLStreamException exception) {
            throw SecureXml.parseError(exception);
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

    private static boolean isSlide(String name) {
        if (!name.startsWith("ppt/slides/slide") || !name.endsWith(".xml")) {
            return false;
        }
        String number = name.substring("ppt/slides/slide".length(), name.length() - ".xml".length());
        return !number.isEmpty() && number.chars().allMatch(Character::isDigit);
    }

    private static int slideNumber(String name) {
        String number = name.substring("ppt/slides/slide".length(), name.length() - ".xml".length());
        try {
            return Integer.parseInt(number);
        } catch (NumberFormatException ignored) {
            return Integer.MAX_VALUE;
        }
    }
}