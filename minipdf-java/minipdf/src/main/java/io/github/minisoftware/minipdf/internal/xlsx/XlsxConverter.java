package io.github.minisoftware.minipdf.internal.xlsx;

import io.github.minisoftware.minipdf.ConversionOptions;
import io.github.minisoftware.minipdf.MiniPdfException;
import io.github.minisoftware.minipdf.internal.OoxmlPackage;
import io.github.minisoftware.minipdf.internal.SecureXml;
import io.github.minisoftware.minipdf.internal.SimplePdfTextRenderer;

import javax.xml.stream.XMLStreamConstants;
import javax.xml.stream.XMLStreamException;
import javax.xml.stream.XMLStreamReader;
import java.util.ArrayList;
import java.util.Comparator;
import java.util.List;

public final class XlsxConverter {
    private XlsxConverter() {
    }

    public static byte[] convert(byte[] input, ConversionOptions options) throws MiniPdfException {
        OoxmlPackage workbook = OoxmlPackage.open(input);
        byte[] sharedStringsXml = workbook.entry("xl/sharedStrings.xml").orElse(null);
        List<String> sharedStrings = sharedStringsXml == null
            ? List.of()
            : readSharedStrings(sharedStringsXml);
        List<String> worksheetNames = workbook.entryNames().stream()
                .filter(name -> name.startsWith("xl/worksheets/") && name.endsWith(".xml"))
                .sorted(Comparator.naturalOrder())
                .toList();
        if (worksheetNames.isEmpty()) {
            throw new MiniPdfException(
                    MiniPdfException.Kind.INVALID_INPUT,
                    "XLSX package does not contain a worksheet");
        }

        List<String> lines = new ArrayList<>();
        for (String worksheetName : worksheetNames) {
            byte[] worksheet = workbook.entry(worksheetName).orElseThrow();
            lines.addAll(readWorksheet(worksheet, sharedStrings));
        }
        return SimplePdfTextRenderer.render(lines, options);
    }

    private static List<String> readSharedStrings(byte[] xml) throws MiniPdfException {
        List<String> strings = new ArrayList<>();
        StringBuilder value = null;
        try {
            XMLStreamReader reader = SecureXml.reader(xml);
            while (reader.hasNext()) {
                int event = reader.next();
                if (event == XMLStreamConstants.START_ELEMENT && reader.getLocalName().equals("si")) {
                    value = new StringBuilder();
                } else if (event == XMLStreamConstants.START_ELEMENT
                        && reader.getLocalName().equals("t") && value != null) {
                    value.append(reader.getElementText());
                } else if (event == XMLStreamConstants.END_ELEMENT
                        && reader.getLocalName().equals("si") && value != null) {
                    strings.add(value.toString());
                    value = null;
                }
            }
            reader.close();
            return strings;
        } catch (XMLStreamException exception) {
            throw SecureXml.parseError(exception);
        }
    }

    private static List<String> readWorksheet(byte[] xml, List<String> sharedStrings) throws MiniPdfException {
        List<String> lines = new ArrayList<>();
        List<String> row = null;
        String cellType = null;
        String cellValue = null;
        try {
            XMLStreamReader reader = SecureXml.reader(xml);
            while (reader.hasNext()) {
                int event = reader.next();
                if (event == XMLStreamConstants.START_ELEMENT && reader.getLocalName().equals("row")) {
                    row = new ArrayList<>();
                } else if (event == XMLStreamConstants.START_ELEMENT && reader.getLocalName().equals("c")) {
                    cellType = reader.getAttributeValue(null, "t");
                    cellValue = "";
                } else if (event == XMLStreamConstants.START_ELEMENT
                        && (reader.getLocalName().equals("v") || reader.getLocalName().equals("t"))
                        && row != null) {
                    cellValue = reader.getElementText();
                } else if (event == XMLStreamConstants.END_ELEMENT
                        && reader.getLocalName().equals("c") && row != null) {
                    row.add(resolveCellValue(cellType, cellValue, sharedStrings));
                } else if (event == XMLStreamConstants.END_ELEMENT
                        && reader.getLocalName().equals("row") && row != null) {
                    lines.add(String.join("    ", row));
                    row = null;
                }
            }
            reader.close();
            return lines;
        } catch (XMLStreamException exception) {
            throw SecureXml.parseError(exception);
        }
    }

    private static String resolveCellValue(String type, String value, List<String> sharedStrings) {
        if ("s".equals(type)) {
            try {
                int index = Integer.parseInt(value);
                return index >= 0 && index < sharedStrings.size() ? sharedStrings.get(index) : value;
            } catch (NumberFormatException ignored) {
                return value;
            }
        }
        if ("b".equals(type)) {
            return "1".equals(value) ? "TRUE" : "FALSE";
        }
        return value == null ? "" : value;
    }

}