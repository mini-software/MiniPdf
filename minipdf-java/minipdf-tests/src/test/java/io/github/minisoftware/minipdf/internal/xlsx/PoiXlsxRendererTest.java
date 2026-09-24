package io.github.minisoftware.minipdf.internal.xlsx;

import io.github.minisoftware.minipdf.MiniPdf;
import org.apache.pdfbox.Loader;
import org.apache.pdfbox.contentstream.operator.Operator;
import org.apache.pdfbox.cos.COSArray;
import org.apache.pdfbox.cos.COSNumber;
import org.apache.pdfbox.pdfparser.PDFStreamParser;
import org.apache.pdfbox.pdmodel.PDDocument;
import org.apache.pdfbox.text.PDFTextStripper;
import org.apache.poi.ss.usermodel.BorderStyle;
import org.apache.poi.ss.usermodel.Cell;
import org.apache.poi.ss.usermodel.CellStyle;
import org.apache.poi.ss.usermodel.Row;
import org.apache.poi.xssf.usermodel.XSSFClientAnchor;
import org.apache.poi.xssf.usermodel.XSSFDrawing;
import org.apache.poi.xssf.usermodel.XSSFSheet;
import org.apache.poi.xssf.usermodel.XSSFWorkbook;
import org.junit.jupiter.api.AfterEach;
import org.junit.jupiter.api.Assumptions;
import org.junit.jupiter.api.Test;
import org.openxmlformats.schemas.drawingml.x2006.main.CTShapeProperties;
import org.openxmlformats.schemas.drawingml.x2006.main.STShapeType;

import java.io.ByteArrayOutputStream;
import java.util.List;

import static org.junit.jupiter.api.Assertions.assertFalse;
import static org.junit.jupiter.api.Assertions.assertTrue;

class PoiXlsxRendererTest {

    @AfterEach
    void clearRegisteredFonts() {
        MiniPdf.clearRegisteredFonts();
    }

    @Test
    void rendersBallotBoxWithoutQuestionMarkFallback() throws Exception {
        Assumptions.assumeTrue(System.getenv("WINDIR") != null, "requires Windows system fonts");
        try (XSSFWorkbook workbook = new XSSFWorkbook(); ByteArrayOutputStream output = new ByteArrayOutputStream()) {
            XSSFSheet sheet = workbook.createSheet("Sheet1");
            Row row = sheet.createRow(0);
            Cell cell = row.createCell(0);
            cell.setCellValue("\u2610");
            workbook.write(output);
            byte[] pdf = MiniPdf.convertBytesToPdf(output.toByteArray());
            try (PDDocument document = Loader.loadPDF(pdf)) {
                String text = new PDFTextStripper().getText(document);
                assertTrue(text.contains("\u2610"), "expected ballot box, got: " + text);
                assertFalse(text.contains("?"), "unexpected glyph fallback: " + text);
            }
        }
    }

    @Test
    void rendersStandaloneRectangleShapeWithFill() throws Exception {
        try (XSSFWorkbook workbook = new XSSFWorkbook(); ByteArrayOutputStream output = new ByteArrayOutputStream()) {
            XSSFSheet sheet = workbook.createSheet("Sheet1");
            sheet.createRow(0).createCell(0).setCellValue("Cell");
            XSSFDrawing drawing = sheet.createDrawingPatriarch();
            XSSFClientAnchor anchor = new XSSFClientAnchor(0, 0, 0, 0, 0, 0, 1, 1);
            CTShapeProperties properties = drawing.createSimpleShape(anchor).getCTShape().getSpPr();
            if (!properties.isSetPrstGeom()) {
                properties.addNewPrstGeom();
            }
            properties.getPrstGeom().setPrst(STShapeType.RECT);
            properties.addNewSolidFill().addNewSrgbClr().setVal(new byte[]{(byte) 255, (byte) 153, (byte) 153});
            workbook.write(output);
            byte[] pdf = MiniPdf.convertBytesToPdf(output.toByteArray());
            try (PDDocument document = Loader.loadPDF(pdf)) {
                assertTrue(containsFillRect(document, 1.0f, 153.0f / 255.0f, 153.0f / 255.0f),
                        "expected a filled rectangle with the shape fill color");
            }
        }
    }

    @Test
    void wrapsAtWordBoundariesWhenWhitespaceIsNearTheLineEnd() throws Exception {
        try (XSSFWorkbook workbook = new XSSFWorkbook(); ByteArrayOutputStream output = new ByteArrayOutputStream()) {
            XSSFSheet sheet = workbook.createSheet("Sheet1");
            sheet.setColumnWidth(0, 9 * 256);
            Row row = sheet.createRow(0);
            row.setHeightInPoints(36.0f);
            Cell cell = row.createCell(0);
            cell.setCellValue("one two three four");
            CellStyle style = workbook.createCellStyle();
            style.setWrapText(true);
            cell.setCellStyle(style);
            workbook.write(output);

            byte[] pdf = MiniPdf.convertBytesToPdf(output.toByteArray());
            try (PDDocument document = Loader.loadPDF(pdf)) {
                String text = new PDFTextStripper().getText(document);
                String normalized = text.replace("\r\n", "\n").replace('\r', '\n');
                assertTrue(normalized.contains("one two\nthree four"), "expected whole-word wrapping, got: " + text);
            }
        }
    }

    @Test
    void rendersDottedCellBorderWithPdfDashPattern() throws Exception {
        try (XSSFWorkbook workbook = new XSSFWorkbook(); ByteArrayOutputStream output = new ByteArrayOutputStream()) {
            XSSFSheet sheet = workbook.createSheet("Sheet1");
            Cell cell = sheet.createRow(0).createCell(0);
            cell.setCellValue("Cell");
            CellStyle style = workbook.createCellStyle();
            style.setBorderBottom(BorderStyle.DOTTED);
            cell.setCellStyle(style);
            workbook.write(output);

            byte[] pdf = MiniPdf.convertBytesToPdf(output.toByteArray());
            try (PDDocument document = Loader.loadPDF(pdf)) {
                assertTrue(containsDashPattern(document, 0.2f, 0.8f), "expected the dotted PDF dash pattern");
            }
        }
    }

    private static boolean containsFillRect(PDDocument document, float red, float green, float blue)
            throws Exception {
        PDFStreamParser parser = new PDFStreamParser(document.getPage(0));
        List<Object> tokens = parser.parse();
        for (int index = 0; index + 4 < tokens.size(); index++) {
            if (tokens.get(index) instanceof COSNumber
                    && tokens.get(index + 1) instanceof COSNumber
                    && tokens.get(index + 2) instanceof COSNumber
                    && near(((COSNumber) tokens.get(index)).floatValue(), red)
                    && near(((COSNumber) tokens.get(index + 1)).floatValue(), green)
                    && near(((COSNumber) tokens.get(index + 2)).floatValue(), blue)
                    && tokens.get(index + 3) instanceof Operator
                    && ("rg".equals(((Operator) tokens.get(index + 3)).getName())
                        || "sc".equals(((Operator) tokens.get(index + 3)).getName()))) {
                for (int cursor = index + 4; cursor + 1 < tokens.size(); cursor++) {
                    if (tokens.get(cursor) instanceof Operator
                            && "re".equals(((Operator) tokens.get(cursor)).getName())
                            && tokens.get(cursor + 1) instanceof Operator
                            && "f".equals(((Operator) tokens.get(cursor + 1)).getName())) {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    private static boolean containsDashPattern(PDDocument document, float dash, float gap) throws Exception {
        PDFStreamParser parser = new PDFStreamParser(document.getPage(0));
        List<Object> tokens = parser.parse();
        for (int index = 0; index + 2 < tokens.size(); index++) {
            if (!(tokens.get(index) instanceof COSArray)
                    || !(tokens.get(index + 1) instanceof COSNumber)
                    || !(tokens.get(index + 2) instanceof Operator)
                    || !"d".equals(((Operator) tokens.get(index + 2)).getName())) {
                continue;
            }
            COSArray pattern = (COSArray) tokens.get(index);
            if (pattern.size() == 2
                    && pattern.getObject(0) instanceof COSNumber
                    && pattern.getObject(1) instanceof COSNumber
                    && near(((COSNumber) pattern.getObject(0)).floatValue(), dash)
                    && near(((COSNumber) pattern.getObject(1)).floatValue(), gap)) {
                return true;
            }
        }
        return false;
    }

    private static boolean near(float actual, float expected) {
        return Math.abs(actual - expected) < 0.01f;
    }
}
