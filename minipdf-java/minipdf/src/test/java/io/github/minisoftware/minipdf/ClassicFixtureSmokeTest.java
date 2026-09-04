package io.github.minisoftware.minipdf;

import org.junit.jupiter.api.Test;

import java.nio.charset.StandardCharsets;
import java.nio.file.Files;
import java.nio.file.Path;

import static org.junit.jupiter.api.Assertions.assertTrue;

class ClassicFixtureSmokeTest {
    private static final Path REPOSITORY_ROOT = Path.of("..", "..").toAbsolutePath().normalize();

    @Test
    void convertsTrackedXlsxFixture() throws Exception {
        Path fixture = REPOSITORY_ROOT.resolve("tests/Issue_Files/xlsx/Simple invoice1.xlsx");

        String pdf = pdfText(MiniPdf.convertToPdfBytes(fixture));

        assertTrue(pdf.contains("INVOICE"));
        assertTrue(pdf.contains("Wedding florals"));
        assertTrue(pdf.endsWith("%%EOF\n"));
    }

    @Test
    void convertsTrackedDocxFixture() throws Exception {
        Path fixture = REPOSITORY_ROOT.resolve("tests/Issue_Files/docx/Invoice.docx");

        String pdf = pdfText(MiniPdf.convertToPdfBytes(fixture));

        assertTrue(pdf.contains("Invoice"));
        assertTrue(pdf.contains("ABC12345"));
        assertTrue(pdf.endsWith("%%EOF\n"));
    }

    @Test
    void convertsIssuePptx() throws Exception {
        Path fixture = REPOSITORY_ROOT.resolve("tests/Issue_Files/pptx/Asian Pacific.pptx");

        String pdf = pdfText(MiniPdf.convertToPdfBytes(fixture));

        assertTrue(pdf.startsWith("%PDF-1.4"));
        assertTrue(pdf.contains("/Type /Pages"));
        assertTrue(pdf.endsWith("%%EOF\n"));
    }

    private static String pdfText(byte[] pdf) {
        return new String(pdf, StandardCharsets.ISO_8859_1);
    }
}