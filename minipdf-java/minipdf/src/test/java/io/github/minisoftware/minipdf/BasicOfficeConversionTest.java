package io.github.minisoftware.minipdf;

import org.junit.jupiter.api.Test;
import org.junit.jupiter.api.io.TempDir;

import java.io.ByteArrayOutputStream;
import java.nio.charset.StandardCharsets;
import java.nio.file.Files;
import java.nio.file.Path;
import java.util.LinkedHashMap;
import java.util.Map;
import java.util.zip.ZipEntry;
import java.util.zip.ZipOutputStream;

import static org.junit.jupiter.api.Assertions.assertTrue;
import static org.junit.jupiter.api.Assertions.assertSame;
import static org.junit.jupiter.api.Assertions.assertThrows;

class BasicOfficeConversionTest {
    @TempDir
    Path temporaryDirectory;

    @Test
    void convertsBasicDocxText() throws Exception {
        byte[] docx = packageWith(Map.of(
                "word/document.xml",
                "<w:document xmlns:w=\"urn:w\"><w:body><w:p><w:r><w:t>Hello DOCX</w:t></w:r>"
                        + "</w:p></w:body></w:document>"));

        String pdf = pdfText(MiniPdf.convertBytesToPdf(docx));

        assertTrue(pdf.startsWith("%PDF-1.4"));
        assertTrue(pdf.contains("(Hello DOCX) Tj"));
    }

    @Test
    void convertsBasicXlsxSharedStringsAndNumbers() throws Exception {
        Map<String, String> entries = new LinkedHashMap<>();
        entries.put("xl/sharedStrings.xml",
                "<sst><si><t>Name</t></si><si><t>Alice</t></si></sst>");
        entries.put("xl/worksheets/sheet1.xml",
                "<worksheet><sheetData><row><c t=\"s\"><v>0</v></c><c><v>30</v></c></row>"
                        + "<row><c t=\"s\"><v>1</v></c></row></sheetData></worksheet>");

        String pdf = pdfText(MiniPdf.convertBytesToPdf(packageWith(entries)));

        assertTrue(pdf.startsWith("%PDF-1.4"));
        assertTrue(pdf.contains("(Name    30) Tj"));
        assertTrue(pdf.contains("(Alice) Tj"));
    }

    @Test
    void honorsPageSizeOverride() throws Exception {
        byte[] docx = packageWith(Map.of(
                "word/document.xml",
                "<w:document xmlns:w=\"urn:w\"><w:body><w:p><w:r><w:t>Size</w:t></w:r>"
                        + "</w:p></w:body></w:document>"));

        byte[] pdf = MiniPdf.convertBytesToPdf(
                docx,
                ConversionOptions.withPageSize(PageSize.of(400.0f, 500.0f)));

        assertTrue(pdfText(pdf).contains("/MediaBox [0 0 400 500]"));
    }

    @Test
    void convertsPptxSlidesToSeparatePages() throws Exception {
    Map<String, String> entries = new LinkedHashMap<>();
    entries.put("ppt/presentation.xml",
        "<p:presentation xmlns:p=\"urn:p\"><p:sldSz cx=\"9144000\" cy=\"6858000\"/>"
            + "</p:presentation>");
    entries.put("ppt/slides/slide2.xml",
        "<p:sld xmlns:p=\"urn:p\" xmlns:a=\"urn:a\"><a:p><a:r><a:t>Slide Two</a:t>"
            + "</a:r></a:p></p:sld>");
    entries.put("ppt/slides/slide1.xml",
        "<p:sld xmlns:p=\"urn:p\" xmlns:a=\"urn:a\"><a:p><a:r><a:t>Slide One</a:t>"
            + "</a:r></a:p></p:sld>");
    entries.put("ppt/slides/slide10.xml",
        "<p:sld xmlns:p=\"urn:p\" xmlns:a=\"urn:a\"><a:p><a:r><a:t>Slide Ten</a:t>"
            + "</a:r></a:p></p:sld>");

    String pdf = pdfText(MiniPdf.convertBytesToPdf(packageWith(entries)));

    assertTrue(pdf.startsWith("%PDF-1.4"));
    assertTrue(pdf.contains("/MediaBox [0 0 720 540]"));
    assertTrue(pdf.contains("/Count 3"));
    assertTrue(pdf.indexOf("(Slide One) Tj") < pdf.indexOf("(Slide Two) Tj"));
    assertTrue(pdf.indexOf("(Slide Two) Tj") < pdf.indexOf("(Slide Ten) Tj"));
    }

    @Test
    void rejectsPptxWithoutSlides() throws Exception {
    byte[] pptx = packageWith(Map.of(
        "ppt/presentation.xml",
        "<p:presentation xmlns:p=\"urn:p\"/>"));

    MiniPdfException exception = assertThrows(
        MiniPdfException.class,
        () -> MiniPdf.convertBytesToPdf(pptx));

    assertSame(MiniPdfException.Kind.INVALID_INPUT, exception.kind());
    }

    @Test
    void classifiesMalformedPptxSlideAsXmlError() throws Exception {
        Map<String, String> entries = new LinkedHashMap<>();
        entries.put("ppt/presentation.xml", "<p:presentation xmlns:p=\"urn:p\"/>");
        entries.put("ppt/slides/slide1.xml", "<p:sld xmlns:p=\"urn:p\"><p:broken></p:sld>");

        MiniPdfException exception = assertThrows(
                MiniPdfException.class,
                () -> MiniPdf.convertBytesToPdf(packageWith(entries)));

        assertSame(MiniPdfException.Kind.XML_PARSE, exception.kind());
    }

    @Test
    void classifiesMalformedSharedStringsAsXmlError() throws Exception {
        Map<String, String> entries = new LinkedHashMap<>();
        entries.put("xl/sharedStrings.xml", "<sst><si><t>broken</si></sst>");
        entries.put("xl/worksheets/sheet1.xml", "<worksheet><sheetData/></worksheet>");

        MiniPdfException exception = assertThrows(
                MiniPdfException.class,
                () -> MiniPdf.convertBytesToPdf(packageWith(entries)));

        assertSame(MiniPdfException.Kind.XML_PARSE, exception.kind());
    }

    @Test
    void convertsPathToRequestedOutput() throws Exception {
        Path input = temporaryDirectory.resolve("input.docx");
        Path output = temporaryDirectory.resolve("output.pdf");
        Files.write(input, packageWith(Map.of(
                "word/document.xml",
                "<w:document xmlns:w=\"urn:w\"><w:body><w:p><w:r><w:t>Path API</w:t></w:r>"
                        + "</w:p></w:body></w:document>")));

        MiniPdf.convertToPdf(input, output);

        assertTrue(Files.isRegularFile(output));
        assertTrue(Files.readString(output, StandardCharsets.ISO_8859_1).contains("(Path API) Tj"));
    }

    @Test
    void classifiesMissingInputAsIoError() {
        Path missing = temporaryDirectory.resolve("missing.docx");

        MiniPdfException exception = assertThrows(
                MiniPdfException.class,
                () -> MiniPdf.convertToPdfBytes(missing));

        assertSame(MiniPdfException.Kind.IO, exception.kind());
    }

    private static byte[] packageWith(Map<String, String> entries) throws Exception {
        ByteArrayOutputStream bytes = new ByteArrayOutputStream();
        try (ZipOutputStream archive = new ZipOutputStream(bytes)) {
            for (Map.Entry<String, String> entry : entries.entrySet()) {
                archive.putNextEntry(new ZipEntry(entry.getKey()));
                archive.write(entry.getValue().getBytes(StandardCharsets.UTF_8));
                archive.closeEntry();
            }
        }
        return bytes.toByteArray();
    }

    private static String pdfText(byte[] pdf) {
        return new String(pdf, StandardCharsets.ISO_8859_1);
    }
}