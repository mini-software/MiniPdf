package io.github.minisoftware.minipdf;

import org.apache.pdfbox.Loader;
import org.apache.pdfbox.cos.COSName;
import org.apache.pdfbox.pdmodel.PDDocument;
import org.apache.pdfbox.text.PDFTextStripper;
import org.junit.jupiter.api.Test;

import java.io.ByteArrayOutputStream;
import java.io.IOException;
import java.io.InputStream;
import java.nio.charset.StandardCharsets;
import java.util.regex.Matcher;
import java.util.regex.Pattern;

import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.assertNotNull;
import static org.junit.jupiter.api.Assertions.assertThrows;
import static org.junit.jupiter.api.Assertions.assertTrue;

class PdfDocumentTest {
    @Test
    void writesBasicPdfDocument() {
        PdfDocument document = new PdfDocument();
        PdfPage page = document.addPage(PageSize.A4.width(), PageSize.A4.height());
        page.addText("Hello from Java MiniPdf", 72.0f, 760.0f, 14.0f, PdfColor.BLACK, false);

        byte[] pdf = document.toBytes();
        String text = new String(pdf, StandardCharsets.ISO_8859_1);

        assertTrue(text.startsWith("%PDF-1.4\n"));
        assertTrue(text.contains("(Hello from Java MiniPdf) Tj"));
        assertTrue(text.endsWith("%%EOF\n"));
    }

    @Test
    void declaresStreamLengthsExactly() {
        PdfDocument document = new PdfDocument();
        document.addPage(612.0f, 792.0f)
                .addText("Hello", 72.0f, 700.0f, 12.0f, PdfColor.BLACK, true);
        byte[] pdf = document.toBytes();
        String text = new String(pdf, StandardCharsets.ISO_8859_1);
        Matcher matcher = Pattern.compile("/Length (\\d+) >>\\nstream\\n").matcher(text);

        int streams = 0;
        while (matcher.find()) {
            int streamStart = matcher.end();
            int streamEnd = text.indexOf("\nendstream", streamStart);
            assertEquals(Integer.parseInt(matcher.group(1)), streamEnd - streamStart);
            streams++;
        }
        assertTrue(streams > 0);
    }

    @Test
    void escapesPdfLiteralText() {
        PdfDocument document = new PdfDocument();
        document.addPage(100.0f, 100.0f)
                .addText("a(b)\\c", 10.0f, 10.0f, 10.0f, PdfColor.BLACK, false);

        String pdf = new String(document.toBytes(), StandardCharsets.ISO_8859_1);

        assertTrue(pdf.contains("(a\\(b\\)\\\\c) Tj"));
    }

    @Test
    void writesXrefAtDeclaredOffset() {
        PdfDocument document = new PdfDocument();
        document.addPage(100.0f, 100.0f);
        byte[] pdf = document.toBytes();
        String text = new String(pdf, StandardCharsets.ISO_8859_1);
        Matcher matcher = Pattern.compile("startxref\\n(\\d+)\\n%%EOF").matcher(text);

        assertTrue(matcher.find());
        assertTrue(text.startsWith("xref\n", Integer.parseInt(matcher.group(1))));
    }

    @Test
    void writesUnicodeWithRegisteredTrueTypeFont() throws Exception {
        byte[] fontData = resourceBytes("/org/apache/pdfbox/resources/ttf/LiberationSans-Regular.ttf");
        MiniPdf.registerFont("Liberation Sans", fontData);
        try {
            PdfDocument document = new PdfDocument();
            document.addPage(300.0f, 200.0f)
                    .addText("Hello Привет", 20.0f, 150.0f, 14.0f, PdfColor.BLACK, false);

            byte[] pdf = document.toBytes();
            String raw = new String(pdf, StandardCharsets.ISO_8859_1);
            assertTrue(raw.contains("/Subtype /Type0"));
            assertTrue(raw.contains("/FontFile2"));
            assertTrue(raw.contains("/CIDToGIDMap"));
            assertTrue(raw.contains("/ToUnicode"));

            try (PDDocument parsed = Loader.loadPDF(pdf)) {
                assertEquals("Hello Привет", new PDFTextStripper().getText(parsed).trim());
                boolean embedded = false;
                for (COSName fontName : parsed.getPage(0).getResources().getFontNames()) {
                    if (parsed.getPage(0).getResources().getFont(fontName).isEmbedded()) {
                        embedded = true;
                    }
                }
                assertTrue(embedded);
            }
        } finally {
            MiniPdf.clearRegisteredFonts();
        }
    }

    @Test
    void writesUnicodeWithRegisteredTrueTypeCollection() throws Exception {
        byte[] fontData = resourceBytes("/org/apache/pdfbox/resources/ttf/LiberationSans-Regular.ttf");
        MiniPdf.registerFont("Liberation Sans TTC", trueTypeCollection(fontData));
        try {
            PdfDocument document = new PdfDocument();
            document.addPage(300.0f, 200.0f)
                    .addText("Hello Привет", 20.0f, 150.0f, 14.0f, PdfColor.BLACK, false);

            byte[] pdf = document.toBytes();
            try (PDDocument parsed = Loader.loadPDF(pdf)) {
                assertEquals("Hello Привет", new PDFTextStripper().getText(parsed).trim());
            }
            assertTrue(new String(pdf, StandardCharsets.ISO_8859_1).contains("/Subtype /Type0"));
        } finally {
            MiniPdf.clearRegisteredFonts();
        }
    }

    @Test
    void unsupportedSupplementaryTextDoesNotDisableSupportedFont() throws Exception {
        byte[] fontData = resourceBytes("/org/apache/pdfbox/resources/ttf/LiberationSans-Regular.ttf");
        MiniPdf.registerFont("Liberation Sans", fontData);
        try {
            PdfDocument document = new PdfDocument();
            PdfPage page = document.addPage(300.0f, 200.0f);
            page.addText("Привет", 20.0f, 150.0f, 14.0f, PdfColor.BLACK, false);
            page.addText("😀", 20.0f, 120.0f, 14.0f, PdfColor.BLACK, false);

            try (PDDocument parsed = Loader.loadPDF(document.toBytes())) {
                assertTrue(new PDFTextStripper().getText(parsed).contains("Привет"));
            }
        } finally {
            MiniPdf.clearRegisteredFonts();
        }
    }

    @Test
    void rejectsTrueTypeReadsOutsideDeclaredTable() throws Exception {
        byte[] fontData = resourceBytes("/org/apache/pdfbox/resources/ttf/LiberationSans-Regular.ttf");
        int headRecord = tableRecord(fontData, "head");
        fontData[headRecord + 12] = 0;
        fontData[headRecord + 13] = 0;
        fontData[headRecord + 14] = 0;
        fontData[headRecord + 15] = 1;

        assertThrows(
                IllegalArgumentException.class,
                () -> TrueTypeFontData.parse("invalid", fontData));
    }

    @Test
    void rejectsInvalidGeometryAndColor() {
        PdfDocument document = new PdfDocument();
        assertThrows(IllegalArgumentException.class, () -> document.addPage(0.0f, 100.0f));
        assertThrows(IllegalArgumentException.class, () -> new PdfColor(1.1f, 0.0f, 0.0f));
    }

    private static byte[] resourceBytes(String name) throws IOException {
        try (InputStream input = PdfDocumentTest.class.getResourceAsStream(name)) {
            assertNotNull(input);
            ByteArrayOutputStream output = new ByteArrayOutputStream();
            byte[] buffer = new byte[8192];
            int read;
            while ((read = input.read(buffer)) != -1) {
                output.write(buffer, 0, read);
            }
            return output.toByteArray();
        }
    }

    private static int tableRecord(byte[] fontData, String tableName) {
        int tableCount = ((fontData[4] & 0xFF) << 8) | (fontData[5] & 0xFF);
        for (int index = 0; index < tableCount; index++) {
            int record = 12 + index * 16;
            String name = new String(fontData, record, 4, StandardCharsets.ISO_8859_1);
            if (tableName.equals(name)) {
                return record;
            }
        }
        throw new AssertionError("missing font table: " + tableName);
    }

    private static byte[] trueTypeCollection(byte[] fontData) {
        int collectionHeaderLength = 16;
        byte[] collection = new byte[collectionHeaderLength + fontData.length];
        collection[0] = 't';
        collection[1] = 't';
        collection[2] = 'c';
        collection[3] = 'f';
        writeU32(collection, 4, 0x00010000L);
        writeU32(collection, 8, 1);
        writeU32(collection, 12, collectionHeaderLength);
        System.arraycopy(fontData, 0, collection, collectionHeaderLength, fontData.length);

        int tableCount = ((fontData[4] & 0xFF) << 8) | (fontData[5] & 0xFF);
        for (int index = 0; index < tableCount; index++) {
            int record = collectionHeaderLength + 12 + index * 16;
            long tableOffset = readU32(collection, record + 8);
            writeU32(collection, record + 8, tableOffset + collectionHeaderLength);
        }
        return collection;
    }

    private static long readU32(byte[] data, int offset) {
        return ((long) (data[offset] & 0xFF) << 24)
                | ((long) (data[offset + 1] & 0xFF) << 16)
                | ((long) (data[offset + 2] & 0xFF) << 8)
                | (data[offset + 3] & 0xFF);
    }

    private static void writeU32(byte[] data, int offset, long value) {
        data[offset] = (byte) (value >> 24);
        data[offset + 1] = (byte) (value >> 16);
        data[offset + 2] = (byte) (value >> 8);
        data[offset + 3] = (byte) value;
    }
}