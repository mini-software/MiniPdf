package io.github.minisoftware.minipdf.internal.docx;

import io.github.minisoftware.minipdf.ConversionOptions;
import io.github.minisoftware.minipdf.MiniPdfException;
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
        return SimplePdfTextRenderer.render(readParagraphs(documentXml), options);
    }

    private static List<String> readParagraphs(byte[] documentXml) throws MiniPdfException {
        List<String> paragraphs = new ArrayList<>();
        StringBuilder paragraph = null;
        try {
            XMLStreamReader reader = SecureXml.reader(documentXml);
            while (reader.hasNext()) {
                int event = reader.next();
                if (event == XMLStreamConstants.START_ELEMENT && reader.getLocalName().equals("p")) {
                    paragraph = new StringBuilder();
                } else if (event == XMLStreamConstants.START_ELEMENT
                        && reader.getLocalName().equals("t") && paragraph != null) {
                    paragraph.append(reader.getElementText());
                } else if (event == XMLStreamConstants.START_ELEMENT
                        && reader.getLocalName().equals("tab") && paragraph != null) {
                    paragraph.append('\t');
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
}