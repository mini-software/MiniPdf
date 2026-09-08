package io.github.minisoftware.minipdf;

import org.apache.pdfbox.Loader;
import org.apache.pdfbox.cos.COSName;
import org.apache.pdfbox.pdmodel.PDDocument;
import org.apache.pdfbox.pdmodel.PDPage;
import org.apache.pdfbox.pdmodel.font.PDFont;
import org.apache.pdfbox.text.PDFTextStripper;
import org.junit.jupiter.api.AfterEach;
import org.junit.jupiter.api.Test;
import org.junit.jupiter.api.io.TempDir;

import java.io.ByteArrayOutputStream;
import java.io.InputStream;
import java.nio.charset.StandardCharsets;
import java.nio.file.Files;
import java.nio.file.Path;
import java.util.LinkedHashMap;
import java.util.Map;
import java.util.zip.ZipEntry;
import java.util.zip.ZipOutputStream;

import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.assertNotNull;
import static org.junit.jupiter.api.Assertions.assertTrue;
import static org.junit.jupiter.api.Assertions.assertSame;
import static org.junit.jupiter.api.Assertions.assertThrows;

class BasicOfficeConversionTest {
    @TempDir
    Path temporaryDirectory;

    @AfterEach
    void clearRegisteredFonts() {
        MiniPdf.clearRegisteredFonts();
    }

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
    void usesRegisteredFontForDocxUnicodeText() throws Exception {
        String text = "\u041f\u0440\u0438\u0432\u0435\u0442 DOCX";
        registerTestFont();
        byte[] docx = packageWith(Map.of(
                "word/document.xml",
                "<w:document xmlns:w=\"urn:w\"><w:body><w:p><w:r><w:t>" + text
                        + "</w:t></w:r></w:p></w:body></w:document>"));

        assertUnicodeTextUsesEmbeddedFont(MiniPdf.convertBytesToPdf(docx), text);
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
            "<p:presentation xmlns:p=\"urn:p\" xmlns:r=\"urn:r\"><p:sldIdLst>"
                + "<p:sldId r:id=\"rId2\"/><p:sldId r:id=\"rId1\"/></p:sldIdLst>"
                + "<p:sldSz cx=\"9144000\" cy=\"6858000\"/></p:presentation>");
        entries.put("ppt/_rels/presentation.xml.rels",
            "<Relationships><Relationship Id=\"rId1\" Type=\"urn/slide\" "
                + "Target=\"slides/slide1.xml\"/><Relationship Id=\"rId2\" "
                + "Type=\"urn/slide\" Target=\"slides/slide2.xml\"/></Relationships>");
        entries.put("ppt/slides/slide1.xml",
            "<p:sld xmlns:p=\"urn:p\" xmlns:a=\"urn:a\"><a:p><a:r><a:t>Second</a:t>"
                + "</a:r></a:p></p:sld>");
        entries.put("ppt/slides/slide2.xml",
            "<p:sld xmlns:p=\"urn:p\" xmlns:a=\"urn:a\"><a:p><a:r><a:t>First</a:t>"
                + "</a:r></a:p></p:sld>");
        entries.put("ppt/slides/slide10.xml",
            "<p:sld xmlns:p=\"urn:p\" xmlns:a=\"urn:a\"><a:p><a:r><a:t>Orphan</a:t>"
                + "</a:r></a:p></p:sld>");

        String pdf = pdfText(MiniPdf.convertBytesToPdf(packageWith(entries)));

        assertTrue(pdf.startsWith("%PDF-1.4"));
        assertTrue(pdf.contains("/MediaBox [0 0 720 540]"));
        assertTrue(pdf.contains("/Count 2"));
        assertTrue(pdf.indexOf("(First) Tj") < pdf.indexOf("(Second) Tj"));
        assertTrue(!pdf.contains("(Orphan) Tj"));
    }

    @Test
    void usesRegisteredFontForPptxUnicodeText() throws Exception {
        String text = "\u041f\u0440\u0438\u0432\u0435\u0442 PPTX";
        registerTestFont();
        Map<String, String> entries = new LinkedHashMap<>();
        entries.put("ppt/presentation.xml",
                "<p:presentation xmlns:p=\"urn:p\" xmlns:r=\"urn:r\"><p:sldIdLst>"
                        + "<p:sldId r:id=\"rId1\"/></p:sldIdLst></p:presentation>");
        entries.put("ppt/_rels/presentation.xml.rels",
                "<Relationships><Relationship Id=\"rId1\" Type=\"urn/slide\" "
                        + "Target=\"slides/slide1.xml\"/></Relationships>");
        entries.put("ppt/slides/slide1.xml",
                "<p:sld xmlns:p=\"urn:p\" xmlns:a=\"urn:a\"><a:p><a:r><a:t>" + text
                        + "</a:t></a:r></a:p></p:sld>");

        assertUnicodeTextUsesEmbeddedFont(MiniPdf.convertBytesToPdf(packageWith(entries)), text);
    }

    private static void registerTestFont() throws Exception {
        try (InputStream input = PDFont.class.getResourceAsStream(
                "/org/apache/pdfbox/resources/ttf/LiberationSans-Regular.ttf")) {
            assertNotNull(input);
            MiniPdf.registerFont("Liberation Sans", input.readAllBytes());
        }
    }

    private static void assertUnicodeTextUsesEmbeddedFont(byte[] pdf, String expected) throws Exception {
        try (PDDocument document = Loader.loadPDF(pdf)) {
            assertEquals(expected, new PDFTextStripper().getText(document).trim());
            boolean hasEmbeddedFont = false;
            for (PDPage page : document.getPages()) {
                for (COSName fontName : page.getResources().getFontNames()) {
                    if (page.getResources().getFont(fontName).isEmbedded()) {
                        hasEmbeddedFont = true;
                    }
                }
            }
            assertTrue(hasEmbeddedFont);
        }
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
        entries.put("ppt/presentation.xml",
            "<p:presentation xmlns:p=\"urn:p\" xmlns:r=\"urn:r\"><p:sldIdLst>"
                + "<p:sldId r:id=\"rId1\"/></p:sldIdLst></p:presentation>");
        entries.put("ppt/_rels/presentation.xml.rels",
            "<Relationships><Relationship Id=\"rId1\" Type=\"urn/slide\" "
                + "Target=\"slides/slide1.xml\"/></Relationships>");
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