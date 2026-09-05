package io.github.minisoftware.minipdf;

import org.junit.jupiter.api.Test;

import java.nio.charset.StandardCharsets;
import java.util.regex.Matcher;
import java.util.regex.Pattern;

import static org.junit.jupiter.api.Assertions.assertEquals;
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
    void rejectsInvalidGeometryAndColor() {
        PdfDocument document = new PdfDocument();
        assertThrows(IllegalArgumentException.class, () -> document.addPage(0.0f, 100.0f));
        assertThrows(IllegalArgumentException.class, () -> new PdfColor(1.1f, 0.0f, 0.0f));
    }
}