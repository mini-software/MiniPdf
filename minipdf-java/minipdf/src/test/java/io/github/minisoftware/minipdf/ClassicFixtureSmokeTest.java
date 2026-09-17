package io.github.minisoftware.minipdf;

import org.apache.pdfbox.Loader;
import org.apache.pdfbox.cos.COSName;
import org.apache.pdfbox.contentstream.operator.Operator;
import org.apache.pdfbox.pdfparser.PDFStreamParser;
import org.apache.pdfbox.pdmodel.PDDocument;
import org.apache.pdfbox.pdmodel.graphics.PDXObject;
import org.apache.pdfbox.pdmodel.graphics.form.PDFormXObject;
import org.apache.pdfbox.pdmodel.graphics.image.PDImageXObject;
import org.apache.pdfbox.rendering.PDFRenderer;
import org.apache.pdfbox.text.PDFTextStripper;
import org.apache.poi.xssf.usermodel.XSSFWorkbook;
import org.apache.poi.xssf.usermodel.XSSFSheet;
import org.junit.jupiter.api.Test;

import java.io.ByteArrayOutputStream;
import java.awt.image.BufferedImage;
import java.nio.charset.StandardCharsets;
import java.nio.file.Files;
import java.nio.file.Path;
import java.nio.file.Paths;
import java.util.Arrays;
import java.util.stream.Stream;
import java.util.regex.Matcher;
import java.util.regex.Pattern;

import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.assertFalse;
import static org.junit.jupiter.api.Assertions.assertTrue;

class ClassicFixtureSmokeTest {
    private static final Path REPOSITORY_ROOT = Paths.get("..", "..").toAbsolutePath().normalize();

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
            BufferedImage page = new PDFRenderer(document).renderImageWithDPI(0, 150.0f);
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
    void rendersScaledTablesAndGroupedArtwork() throws Exception {
        Path fixture = REPOSITORY_ROOT.resolve("tests/Issue_Files/xlsx/Business expenses budget2.xlsx");

        try (PDDocument document = Loader.loadPDF(MiniPdf.convertToPdfBytes(fixture))) {
            assertEquals(4, document.getNumberOfPages());
            PDFTextStripper stripper = new PDFTextStripper();
            stripper.setStartPage(1);
            stripper.setEndPage(1);
            String firstPage = stripper.getText(document);
            assertEquals(1, firstPage.split("EMPLOYEE COSTS", -1).length - 1, firstPage);

            long imageCount = 0;
            for (COSName name : document.getPage(0).getResources().getXObjectNames()) {
                if (document.getPage(0).getResources().getXObject(name) instanceof PDImageXObject) {
                    imageCount++;
                }
            }
            assertTrue(imageCount >= 2, "imageCount=" + imageCount);

            BufferedImage page = new PDFRenderer(document).renderImageWithDPI(0, 150.0f);
            long stripedPixels = 0;
            long translucentShapePixels = 0;
            for (int y = 0; y < page.getHeight(); y++) {
                for (int x = 0; x < page.getWidth(); x++) {
                    int rgb = page.getRGB(x, y);
                    int red = (rgb >> 16) & 0xff;
                    int green = (rgb >> 8) & 0xff;
                    int blue = rgb & 0xff;
                    if (red >= 210 && red <= 220 && green >= 210 && green <= 220 && blue >= 210 && blue <= 220) {
                        stripedPixels++;
                    }
                    if (red >= 110 && red <= 125 && green >= 78 && green <= 92 && blue >= 70 && blue <= 84) {
                        translucentShapePixels++;
                    }
                }
            }
            assertTrue(stripedPixels > 100_000, "stripedPixels=" + stripedPixels);
            assertTrue(translucentShapePixels > 5_000, "translucentShapePixels=" + translucentShapePixels);
        }
    }

    @Test
    void fitsWidthWhenFitHeightIsUnlimited() throws Exception {
        Path fixture = REPOSITORY_ROOT.resolve("tests/Issue_Files/xlsx/Expense report basic1.xlsx");

        try (PDDocument document = Loader.loadPDF(MiniPdf.convertToPdfBytes(fixture))) {
            assertEquals(1, document.getNumberOfPages());
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
            for (COSName name : document.getPage(0).getResources().getXObjectNames()) {
                PDXObject object = document.getPage(0).getResources().getXObject(name);
                if (object instanceof PDImageXObject) {
                    PDImageXObject image = (PDImageXObject) object;
                    hasImage = true;
                    widestImage = Math.max(widestImage, image.getWidth());
                } else if (object instanceof PDFormXObject) {
                    hasVectorForm = true;
                }
            }
            assertTrue(hasImage || hasVectorForm);
            assertTrue(hasVectorForm || widestImage >= 2000, "widestImage=" + widestImage);
            BufferedImage pageOne = new PDFRenderer(document).renderImageWithDPI(0, 150.0f);
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
            XSSFSheet sheet = workbook.createSheet("Multilingual");
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

        byte[] pdf = MiniPdf.convertToPdfBytes(fixture);
        String text = extractedText(pdf);

        assertTrue(text.contains("Invoice"), text);
        assertTrue(text.contains("ABC12345"), text);
        assertTrue(pdfText(pdf).endsWith("%%EOF\n"));
    }

    @Test
    void preservesVietnameseTextAndPaginationInIssue91() throws Exception {
        String windows = System.getenv("WINDIR");
        org.junit.jupiter.api.Assumptions.assumeTrue(
                windows != null && Files.isRegularFile(Paths.get(windows, "Fonts", "times.ttf")),
                "requires Times New Roman");
        Path fixture = REPOSITORY_ROOT.resolve("tests/Issue_Files/docx/TestIssue91.docx");
        byte[] pdf = MiniPdf.convertToPdfBytes(fixture);

        try (PDDocument document = Loader.loadPDF(pdf)) {
            String text = new PDFTextStripper().getText(document);
            assertEquals(3, document.getNumberOfPages());
            assertTrue(text.contains("PH\u1ee4 L\u1ee4C - KHO\u1ea2N VAY V\u1ed0N \u0110\u1ea6U T\u01af"), text);
            assertTrue(text.contains("Bên B chấp nhận chịu, chỉ được gia hạn"), text);
        }
        try (PDDocument document = Loader.loadPDF(pdf)) {
            PDFTextStripper secondPageStripper = new PDFTextStripper();
            secondPageStripper.setStartPage(2);
            secondPageStripper.setEndPage(2);
            String secondPage = secondPageStripper.getText(document).trim();
            assertTrue(secondPage.startsWith("Trong Phụ lục này"), secondPage);
            assertTrue(
                    secondPage.replace("\r\n", "\n").contains(
                            "[INTEREST_RATE] %/năm.\nTrong trường hợp"),
                    secondPage);
            assertTrue(lines(secondPage).anyMatch(line -> line.trim().equals("2")), secondPage);
        }
    }

    @Test
    void rendersIssue93AsSinglePageForm() throws Exception {
        String windows = System.getenv("WINDIR");
        org.junit.jupiter.api.Assumptions.assumeTrue(
                windows != null && Files.isRegularFile(Paths.get(windows, "Fonts", "msyh.ttc")),
                "requires Microsoft YaHei");
        Path fixture = REPOSITORY_ROOT.resolve("tests/Issue_Files/docx/TestIssue93.docx");

        try (PDDocument document = Loader.loadPDF(MiniPdf.convertToPdfBytes(fixture))) {
            String text = new PDFTextStripper().getText(document);
            assertEquals(1, document.getNumberOfPages());
            assertTrue(text.contains("\u57fa\u672c\u4fe1\u606f"), text);
            assertTrue(text.contains("A-1_1"), text);
        }
    }

    @Test
    void rendersIssue66AsThreePageForm() throws Exception {
        String windows = System.getenv("WINDIR");
        org.junit.jupiter.api.Assumptions.assumeTrue(
                windows != null && Files.isRegularFile(Paths.get(windows, "Fonts", "msyh.ttc")),
                "requires Microsoft YaHei");
        Path fixture = REPOSITORY_ROOT.resolve("tests/Issue_Files/docx/issues66.docx");

        try (PDDocument document = Loader.loadPDF(MiniPdf.convertToPdfBytes(fixture))) {
            String text = new PDFTextStripper().getText(document);
            assertEquals(3, document.getNumberOfPages());
            assertTrue(text.contains("\u5fae\u7eb3\u52a0\u5de5\u5e73\u53f0\u5de5\u827a\u7533\u8bf7\u8868"), text);
            assertTrue(text.contains("985 \u9ad8\u6821"), text);
            PDFTextStripper firstPageStripper = new PDFTextStripper();
            firstPageStripper.setStartPage(1);
            firstPageStripper.setEndPage(1);
            String firstPage = firstPageStripper.getText(document);
            assertFalse(firstPage.contains("\u4f7f\u7528\u6d89\u53ca\u771f\u7a7a\u7684\u8bbe\u5907"), firstPage);
            assertTrue(lines(firstPage).anyMatch(line -> line.trim().equals("1")), firstPage);
            PDFTextStripper secondPageStripper = new PDFTextStripper();
            secondPageStripper.setStartPage(2);
            secondPageStripper.setEndPage(2);
            assertTrue(lines(secondPageStripper.getText(document))
                    .anyMatch(line -> line.trim().equals("2")));
            PDFTextStripper thirdPageStripper = new PDFTextStripper();
            thirdPageStripper.setStartPage(3);
            thirdPageStripper.setEndPage(3);
            assertTrue(lines(thirdPageStripper.getText(document))
                    .anyMatch(line -> line.trim().equals("3")));
        }
    }

    @Test
    void skipsUnsupportedDocxPictures() throws Exception {
        Path fixture = REPOSITORY_ROOT.resolve("tests/Issue_Files/docx/OSCAR WARD.docx");

        try (PDDocument document = Loader.loadPDF(MiniPdf.convertToPdfBytes(fixture))) {
            assertTrue(document.getNumberOfPages() > 0);
        }
    }

    @Test
    void preservesLiteralTextAroundPageNumberField() throws Exception {
        String windows = System.getenv("WINDIR");
        org.junit.jupiter.api.Assumptions.assumeTrue(
                windows != null && Files.isRegularFile(Paths.get(windows, "Fonts", "msyh.ttc")),
                "requires Microsoft YaHei");
        Path fixture = REPOSITORY_ROOT.resolve("tests/Issue_Files/docx/issue202605.docx");

        try (PDDocument document = Loader.loadPDF(MiniPdf.convertToPdfBytes(fixture))) {
            assertEquals(3, document.getNumberOfPages());
            for (int page = 1; page <= document.getNumberOfPages(); page++) {
                PDFTextStripper stripper = new PDFTextStripper();
                stripper.setStartPage(page);
                stripper.setEndPage(page);
                String expected = page + " \u9801";
                assertTrue(lines(stripper.getText(document))
                        .anyMatch(line -> line.trim().equals(expected)), "missing " + expected);
            }
        }
    }

    @Test
    void convertsIssuePptx() throws Exception {
        Path fixture = REPOSITORY_ROOT.resolve("tests/Issue_Files/pptx/Asian Pacific.pptx");

        byte[] pdf = MiniPdf.convertToPdfBytes(fixture);

        try (PDDocument document = Loader.loadPDF(pdf)) {
            assertTrue(document.getNumberOfPages() > 0);
        }
        assertTrue(pdfText(pdf).endsWith("%%EOF\n"));
    }

    private static String pdfText(byte[] pdf) {
        return new String(pdf, StandardCharsets.ISO_8859_1);
    }

    private static String extractedText(byte[] pdf) throws Exception {
        try (PDDocument document = Loader.loadPDF(pdf)) {
            return new PDFTextStripper().getText(document);
        }
    }

    private static Stream<String> lines(String value) {
        return Arrays.stream(value.split("\\R"));
    }
}