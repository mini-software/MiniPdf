package io.github.minisoftware.minipdf;

import org.apache.pdfbox.Loader;
import org.apache.pdfbox.contentstream.operator.Operator;
import org.apache.pdfbox.pdfparser.PDFStreamParser;
import org.apache.pdfbox.pdmodel.PDDocument;
import org.apache.pdfbox.pdmodel.graphics.form.PDFormXObject;
import org.apache.pdfbox.pdmodel.graphics.image.PDImageXObject;
import org.apache.pdfbox.rendering.PDFRenderer;
import org.apache.pdfbox.text.PDFTextStripper;
import org.junit.jupiter.api.Test;

import java.nio.charset.StandardCharsets;
import java.nio.file.Files;
import java.nio.file.Path;
import java.util.regex.Matcher;
import java.util.regex.Pattern;

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
        Path fixture = REPOSITORY_ROOT.resolve(
                "tests/MiniPdf.Scripts/output/classic151_multilingual_greetings.xlsx");

        try (PDDocument document = Loader.loadPDF(MiniPdf.convertToPdfBytes(fixture))) {
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