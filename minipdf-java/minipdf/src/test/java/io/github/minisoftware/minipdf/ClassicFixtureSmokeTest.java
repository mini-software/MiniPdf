package io.github.minisoftware.minipdf;

import org.junit.jupiter.api.Test;

import java.nio.charset.StandardCharsets;
import java.nio.file.Files;
import java.nio.file.Path;

import static org.junit.jupiter.api.Assertions.assertTrue;

class ClassicFixtureSmokeTest {
    private static final Path REPOSITORY_ROOT = Path.of("..", "..").toAbsolutePath().normalize();

    @Test
    void convertsClassic01Xlsx() throws Exception {
        Path fixture = REPOSITORY_ROOT.resolve(
                "tests/MiniPdf.Scripts/output/classic01_basic_table_with_headers.xlsx");

        String pdf = pdfText(MiniPdf.convertToPdfBytes(fixture));

        assertTrue(pdf.contains("(Name    Age    City) Tj"));
        assertTrue(pdf.contains("(Alice    30    New York) Tj"));
        assertTrue(pdf.endsWith("%%EOF\n"));
    }

    @Test
    void convertsClassic01Docx() throws Exception {
        Path fixture = REPOSITORY_ROOT.resolve(
                "tests/MiniPdf.Scripts/output_docx/docx_classic01_single_paragraph.docx");

        String pdf = pdfText(MiniPdf.convertToPdfBytes(fixture));

        assertTrue(pdf.contains("Hello, World!"));
        assertTrue(pdf.contains("benchmarking"));
        assertTrue(pdf.contains("MiniPdf DOCX-to-PDF conversion."));
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