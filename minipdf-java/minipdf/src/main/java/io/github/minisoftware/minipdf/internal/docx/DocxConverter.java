package io.github.minisoftware.minipdf.internal.docx;

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
import java.util.List;

public final class DocxConverter {
    private DocxConverter() {
    }

    public static byte[] convert(byte[] input, ConversionOptions options) throws MiniPdfException {
        OoxmlPackage document = OoxmlPackage.open(input);
        byte[] documentXml = document.entry("word/document.xml")
                .orElseThrow(() -> new MiniPdfException(
                        MiniPdfException.Kind.INVALID_INPUT,
                        "DOCX package does not contain word/document.xml"));
        DocxContent content = readDocument(documentXml);
        if (content.tableCount() > 0) {
            byte[] structured = PoiDocxRenderer.render(input, options, content.pageSize());
            if (structured != null) {
                return structured;
            }
        }
        return SimplePdfTextRenderer.renderPages(content.pages(), options, content.pageSize());
    }

    private static DocxContent readDocument(byte[] documentXml) throws MiniPdfException {
        List<List<String>> pages = new ArrayList<>();
        List<String> page = new ArrayList<>();
        StringBuilder paragraph = null;
        float pageWidth = 0.0f;
        float pageHeight = 0.0f;
        boolean inParagraphSectionProperties = false;
        boolean sectionBreakAfterParagraph = false;
        boolean paragraphSectionChangesOrientation = false;
        int tableCount = 0;
        int drawingCount = 0;
        try {
            XMLStreamReader reader = SecureXml.reader(documentXml);
            while (reader.hasNext()) {
                int event = reader.next();
                if (event == XMLStreamConstants.START_ELEMENT && reader.getLocalName().equals("pgSz")) {
                    boolean landscape = "landscape".equals(attribute(reader, "orient"));
                    if (inParagraphSectionProperties && landscape) {
                        paragraphSectionChangesOrientation = true;
                    }
                    pageWidth = floatAttribute(reader, "w") / 20.0f;
                    pageHeight = floatAttribute(reader, "h") / 20.0f;
                } else if (event == XMLStreamConstants.START_ELEMENT && reader.getLocalName().equals("p")) {
                    paragraph = new StringBuilder();
                    sectionBreakAfterParagraph = false;
                    paragraphSectionChangesOrientation = false;
                } else if (event == XMLStreamConstants.START_ELEMENT && reader.getLocalName().equals("tbl")) {
                    tableCount++;
                } else if (event == XMLStreamConstants.START_ELEMENT && reader.getLocalName().equals("drawing")) {
                    drawingCount++;
                } else if (event == XMLStreamConstants.START_ELEMENT
                        && reader.getLocalName().equals("sectPr") && paragraph != null) {
                    inParagraphSectionProperties = true;
                    sectionBreakAfterParagraph = true;
                } else if (event == XMLStreamConstants.START_ELEMENT
                        && reader.getLocalName().equals("type") && inParagraphSectionProperties) {
                    sectionBreakAfterParagraph = !"continuous".equals(attribute(reader, "val"));
                } else if (event == XMLStreamConstants.START_ELEMENT
                        && reader.getLocalName().equals("t") && paragraph != null) {
                    paragraph.append(reader.getElementText());
                } else if (event == XMLStreamConstants.START_ELEMENT
                        && reader.getLocalName().equals("tab") && paragraph != null) {
                    paragraph.append("    ");
                } else if (event == XMLStreamConstants.START_ELEMENT
                        && (reader.getLocalName().equals("br") || reader.getLocalName().equals("cr"))
                        && paragraph != null) {
                    if (isPageBreak(reader)) {
                        page.add(paragraph.toString());
                        pages.add(page);
                        page = new ArrayList<>();
                        paragraph = new StringBuilder();
                    } else if (isLineBreak(reader)) {
                        page.add(paragraph.toString());
                        paragraph = new StringBuilder();
                    } else {
                        paragraph.append(' ');
                    }
                } else if (event == XMLStreamConstants.END_ELEMENT
                        && reader.getLocalName().equals("sectPr") && inParagraphSectionProperties) {
                    inParagraphSectionProperties = false;
                } else if (event == XMLStreamConstants.END_ELEMENT
                        && reader.getLocalName().equals("p") && paragraph != null) {
                    page.add(paragraph.toString());
                    paragraph = null;
                    if (sectionBreakAfterParagraph && !paragraphSectionChangesOrientation && !page.isEmpty()) {
                        pages.add(page);
                        page = new ArrayList<>();
                    }
                }
            }
            reader.close();
            if (!page.isEmpty() || pages.isEmpty()) {
                pages.add(page);
            }
            PageSize pageSize = pageWidth > 0.0f && pageHeight > 0.0f
                    ? PageSize.of(pageWidth, pageHeight)
                    : PageSize.A4;
            return new DocxContent(pages, pageSize, tableCount, drawingCount);
        } catch (XMLStreamException exception) {
            throw SecureXml.parseError(exception);
        }
    }

    private static float floatAttribute(XMLStreamReader reader, String localName) {
        String value = attribute(reader, localName);
        if (value == null) {
            return 0.0f;
        }
        try {
            return Float.parseFloat(value);
        } catch (NumberFormatException ignored) {
            return 0.0f;
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

    private static boolean isPageBreak(XMLStreamReader reader) {
        for (int index = 0; index < reader.getAttributeCount(); index++) {
            if (reader.getAttributeLocalName(index).equals("type")
                    && reader.getAttributeValue(index).equals("page")) {
                return true;
            }
        }
        return false;
    }

    private static boolean isLineBreak(XMLStreamReader reader) {
        if (reader.getLocalName().equals("cr")) {
            return true;
        }
        String type = attribute(reader, "type");
        return type == null || type.equals("textWrapping");
    }

    private static final class DocxContent {
        private final List<List<String>> pages;
        private final PageSize pageSize;
        private final int tableCount;
        private final int drawingCount;

        private DocxContent(List<List<String>> pages, PageSize pageSize, int tableCount, int drawingCount) {
            this.pages = pages;
            this.pageSize = pageSize;
            this.tableCount = tableCount;
            this.drawingCount = drawingCount;
        }

        private List<List<String>> pages() {
            return pages;
        }

        private PageSize pageSize() {
            return pageSize;
        }

        private int tableCount() {
            return tableCount;
        }

        private int drawingCount() {
            return drawingCount;
        }
    }
}