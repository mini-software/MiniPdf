package io.github.minisoftware.minipdf;

import org.apache.pdfbox.Loader;
import org.apache.pdfbox.contentstream.operator.Operator;
import org.apache.pdfbox.pdfparser.PDFStreamParser;
import org.apache.pdfbox.pdmodel.PDDocument;
import org.apache.pdfbox.pdmodel.graphics.form.PDFormXObject;
import org.apache.pdfbox.pdmodel.graphics.image.PDImageXObject;
import org.apache.pdfbox.rendering.PDFRenderer;
import org.apache.pdfbox.text.PDFTextStripper;
import org.apache.poi.xssf.usermodel.XSSFWorkbook;
import org.junit.jupiter.api.Test;

import java.io.ByteArrayOutputStream;
import java.nio.charset.StandardCharsets;
import java.nio.file.Files;
import java.nio.file.Path;
import java.util.regex.Matcher;
import java.util.regex.Pattern;

import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.assertTrue;

class ClassicFixtureSmokeTest {
    private static final Path REPOSITORY_ROOT = Path.of("..", "..").toAbsolutePath().normalize();

    @Test
    void convertsTrackedXlsxFixture() throws Exception {
        Path fixture = REPOSITORY_ROOT.resolve("tests/Issue_Files/xlsx/Simple invoice1.xlsx");

        String pdf = extractedText(MiniPdf.convertToPdfBytes(fixture));

        assertTrue(pdf.contains("INVOICE"));
        assertTrue(pdf.contains("Wedding florals"));
    }

    @Test
    void defaultsUnspecifiedXlsxPaperSizeToA4() throws Exception {
        Path fixture = REPOSITORY_ROOT.resolve("tests/Issue_Files/xlsx/XlsxIssue77_Template1.xlsx");

        try (PDDocument document = Loader.loadPDF(MiniPdf.convertToPdfBytes(fixture))) {
            assertEquals(PageSize.A4.width(), document.getPage(0).getMediaBox().getWidth(), 0.1f);
            assertEquals(PageSize.A4.height(), document.getPage(0).getMediaBox().getHeight(), 0.1f);
            assertEquals(6, document.getNumberOfPages());
            PDFTextStripper stripper = new PDFTextStripper();
            stripper.setStartPage(2);
            stripper.setEndPage(2);
            String secondPage = stripper.getText(document);
            assertTrue(secondPage.contains("RMA AUTHORIZATION"), secondPage);
        }
    }

    @Test
    void preservesMergedCellColumnWidths() throws Exception {
        Path fixture = REPOSITORY_ROOT.resolve("tests/Issue_Files/xlsx/XlsxIssue77_MergedCellAlignment.xlsx");

        try (PDDocument document = Loader.loadPDF(MiniPdf.convertToPdfBytes(fixture))) {
            String text = new PDFTextStripper().getText(document);
            assertTrue(text.contains("Horizontal Left + Vertical Bottom"), text);
            var page = new PDFRenderer(document).renderImageWithDPI(0, 150.0f);
            int rightmostDarkPixel = 0;
            int visibleOverflowPixels = 0;
            for (int y = 0; y < page.getHeight(); y++) {
                for (int x = 0; x < page.getWidth(); x++) {
                    int rgb = page.getRGB(x, y);
                    if (((rgb >> 16) & 0xff) < 80
                            && ((rgb >> 8) & 0xff) < 80
                            && (rgb & 0xff) < 80) {
                        rightmostDarkPixel = Math.max(rightmostDarkPixel, x);
                        if (x >= 160 && x < 370 && y >= 150 && y < 185) {
                            visibleOverflowPixels++;
                        }
                    }
                }
            }
            assertTrue(rightmostDarkPixel > 1150, "rightmostDarkPixel=" + rightmostDarkPixel);
            assertTrue(visibleOverflowPixels > 100, "visibleOverflowPixels=" + visibleOverflowPixels);
            long mergedRightBorderPixels = java.util.stream.IntStream.range(250, 280)
                .mapToLong(x -> java.util.stream.IntStream.range(360, 445)
                    .filter(y -> {
                        int rgb = page.getRGB(x, y);
                        return ((rgb >> 16) & 0xff) < 80
                            && ((rgb >> 8) & 0xff) < 80
                            && (rgb & 0xff) < 80;
                    })
                    .count())
                .max()
                .orElse(0);
            assertTrue(mergedRightBorderPixels > 40, "mergedRightBorderPixels=" + mergedRightBorderPixels);
        }
    }

    @Test
    void convertsIssueXlsxWithCjkText() throws Exception {
        Path fixture = REPOSITORY_ROOT.resolve("tests/Issue_Files/xlsx/Issue202609031340.xlsx");
        byte[] pdf = MiniPdf.convertToPdfBytes(fixture);

        try (PDDocument document = Loader.loadPDF(pdf)) {
            String text = new PDFTextStripper().getText(document).replaceAll("\\s+", "");
            assertTrue(text.contains("注塑制程检验标准书"), text);
            assertTrue(text.contains("产品型号"), text);
            assertTrue(text.contains("包装"), text);
            assertTrue(text.contains("项目"), text);
            assertTrue(text.contains("频率"), text);
            assertTrue(text.contains("首件"), text);
            assertTrue(document.getNumberOfPages() == 4, "pages=" + document.getNumberOfPages());
            boolean hasImage = false;
            boolean hasVectorForm = false;
            int widestImage = 0;
            for (var name : document.getPage(0).getResources().getXObjectNames()) {
                var object = document.getPage(0).getResources().getXObject(name);
                if (object instanceof PDImageXObject image) {
                    hasImage = true;
                    widestImage = Math.max(widestImage, image.getWidth());
                } else if (object instanceof PDFormXObject) {
                    hasVectorForm = true;
                }
            }
            assertTrue(hasImage || hasVectorForm);
            assertTrue(hasVectorForm || widestImage >= 2000, "widestImage=" + widestImage);
            var pageOne = new PDFRenderer(document).renderImageWithDPI(0, 150.0f);
            int longestDarkRow = 0;
            for (int y = 0; y < pageOne.getHeight(); y++) {
                int darkPixels = 0;
                for (int x = 0; x < pageOne.getWidth(); x++) {
                    int rgb = pageOne.getRGB(x, y);
                    if (((rgb >> 16) & 0xff) < 80
                            && ((rgb >> 8) & 0xff) < 80
                            && (rgb & 0xff) < 80) {
                        darkPixels++;
                    }
                }
                longestDarkRow = Math.max(longestDarkRow, darkPixels);
            }
            assertTrue(longestDarkRow > 1100, "longestDarkRow=" + longestDarkRow);
            long pageThreeStrokes = new PDFStreamParser(document.getPage(2)).parse().stream()
                    .filter(Operator.class::isInstance)
                    .map(Operator.class::cast)
                    .filter(operator -> "S".equals(operator.getName()))
                    .count();
            assertTrue(pageThreeStrokes > 100, "pageThreeStrokes=" + pageThreeStrokes);
        }
        String rawPdf = new String(pdf, StandardCharsets.ISO_8859_1);
        Matcher startXref = Pattern.compile("startxref\\s+(\\d+)").matcher(rawPdf);
        assertTrue(startXref.find());
        assertTrue(rawPdf.startsWith("xref", Integer.parseInt(startXref.group(1))));
    }

    @Test
    void convertsMultilingualXlsxWhenTheSelectedFontLacksGlyphs() throws Exception {
        byte[] workbookBytes;
        try (XSSFWorkbook workbook = new XSSFWorkbook();
             ByteArrayOutputStream output = new ByteArrayOutputStream()) {
            var sheet = workbook.createSheet("Multilingual");
            sheet.createRow(0).createCell(0).setCellValue("Hello");
            sheet.createRow(1).createCell(0).setCellValue("안녕하세요 مرحبا 😀");
            workbook.write(output);
            workbookBytes = output.toByteArray();
        }

        try (PDDocument document = Loader.loadPDF(MiniPdf.convertBytesToPdf(workbookBytes))) {
            String text = new PDFTextStripper().getText(document);
            assertTrue(text.contains("Hello"), text);
            assertTrue(document.getNumberOfPages() > 0);
        }
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

    private static String extractedText(byte[] pdf) throws Exception {
        try (PDDocument document = Loader.loadPDF(pdf)) {
            return new PDFTextStripper().getText(document);
        }
    }
}