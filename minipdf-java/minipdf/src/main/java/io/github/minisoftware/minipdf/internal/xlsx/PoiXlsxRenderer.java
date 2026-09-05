package io.github.minisoftware.minipdf.internal.xlsx;

import de.rototor.pdfbox.graphics2d.PdfBoxGraphics2D;
import io.github.minisoftware.minipdf.ConversionOptions;
import io.github.minisoftware.minipdf.MiniPdf;
import io.github.minisoftware.minipdf.MiniPdfException;
import io.github.minisoftware.minipdf.PageSize;
import io.github.minisoftware.minipdf.RegisteredFont;
import org.apache.pdfbox.pdmodel.PDDocument;
import org.apache.pdfbox.pdmodel.PDPage;
import org.apache.pdfbox.pdmodel.PDPageContentStream;
import org.apache.pdfbox.pdmodel.common.PDRectangle;
import org.apache.pdfbox.pdmodel.font.PDFont;
import org.apache.pdfbox.pdmodel.font.PDType0Font;
import org.apache.pdfbox.pdmodel.font.PDType1Font;
import org.apache.pdfbox.pdmodel.graphics.form.PDFormXObject;
import org.apache.pdfbox.pdmodel.font.Standard14Fonts;
import org.apache.pdfbox.pdmodel.graphics.image.PDImageXObject;
import org.apache.pdfbox.util.Matrix;
import org.apache.poi.hemf.usermodel.HemfPicture;
import org.apache.pdfbox.pdmodel.graphics.state.RenderingMode;
import org.apache.pdfbox.pdfwriter.compress.CompressParameters;
import org.apache.poi.ss.SpreadsheetVersion;
import org.apache.poi.ss.usermodel.BorderStyle;
import org.apache.poi.ss.usermodel.Cell;
import org.apache.poi.ss.usermodel.CellStyle;
import org.apache.poi.ss.usermodel.DataFormatter;
import org.apache.poi.ss.usermodel.FillPatternType;
import org.apache.poi.ss.usermodel.FormulaEvaluator;
import org.apache.poi.ss.usermodel.HorizontalAlignment;
import org.apache.poi.ss.usermodel.Name;
import org.apache.poi.ss.usermodel.PageMargin;
import org.apache.poi.ss.usermodel.PrintSetup;
import org.apache.poi.ss.usermodel.Row;
import org.apache.poi.ss.usermodel.VerticalAlignment;
import org.apache.poi.ss.util.AreaReference;
import org.apache.poi.ss.util.CellRangeAddress;
import org.apache.poi.ss.util.CellReference;
import org.apache.poi.xssf.usermodel.XSSFCellStyle;
import org.apache.poi.xssf.usermodel.XSSFClientAnchor;
import org.apache.poi.xssf.usermodel.XSSFColor;
import org.apache.poi.xssf.usermodel.XSSFDrawing;
import org.apache.poi.xssf.usermodel.XSSFFont;
import org.apache.poi.xssf.usermodel.XSSFPicture;
import org.apache.poi.xssf.usermodel.XSSFPrintSetup;
import org.apache.poi.xssf.usermodel.XSSFShape;
import org.apache.poi.xssf.usermodel.XSSFSheet;
import org.apache.poi.xssf.usermodel.XSSFWorkbook;
import org.openxmlformats.schemas.spreadsheetml.x2006.main.CTDefinedName;

import java.awt.Color;
import java.awt.geom.Rectangle2D;
import java.io.ByteArrayInputStream;
import java.io.ByteArrayOutputStream;
import java.io.IOException;
import java.nio.file.Files;
import java.nio.file.Path;
import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.Locale;
import java.util.Map;

import static io.github.minisoftware.minipdf.internal.xlsx.LegacyVmlPictureReader.LegacyPicture;

final class PoiXlsxRenderer {
    private static final float POINTS_PER_PIXEL = 72.0f / 96.0f;
    private static final float POINTS_PER_INCH = 72.0f;
    private static final float EMU_PER_POINT = 12_700.0f;
    private static final float CELL_PADDING = 2.0f;
    private static final float CENTERED_VML_HORIZONTAL_GEOMETRY_SCALE = 1.0777f;
    private static final float CENTERED_VML_BORDER_SCALE = 1.8f;

    private PoiXlsxRenderer() {
    }

    static byte[] render(byte[] input, ConversionOptions options) throws MiniPdfException {
        try (XSSFWorkbook workbook = new XSSFWorkbook(new ByteArrayInputStream(input));
             PDDocument document = new PDDocument()) {
            document.setVersion(1.4f);
            FontSet fonts = FontSet.load(document);
            Map<String, List<LegacyPicture>> legacyPictures = LegacyVmlPictureReader.read(input);
            DataFormatter formatter = new DataFormatter(Locale.ROOT, true);
            FormulaEvaluator evaluator = workbook.getCreationHelper().createFormulaEvaluator();
            for (int sheetIndex = 0; sheetIndex < workbook.getNumberOfSheets(); sheetIndex++) {
                XSSFSheet sheet = workbook.getSheetAt(sheetIndex);
                String sheetPath = sheet.getPackagePart().getPartName().getName().replaceFirst("^/", "");
                renderSheet(
                    document,
                    fonts,
                    workbook,
                    sheet,
                    sheetIndex,
                    formatter,
                    evaluator,
                    options,
                    legacyPictures.getOrDefault(sheetPath, List.of()));
            }
            ByteArrayOutputStream output = new ByteArrayOutputStream();
            document.save(output, CompressParameters.NO_COMPRESSION);
            return output.toByteArray();
        } catch (IOException | RuntimeException exception) {
            throw new MiniPdfException(
                    MiniPdfException.Kind.INVALID_INPUT,
                    "unable to render XLSX: " + exception.getMessage(),
                    exception);
        }
    }

    private static void renderSheet(
            PDDocument document,
            FontSet fonts,
            XSSFWorkbook workbook,
            XSSFSheet sheet,
            int sheetIndex,
            DataFormatter formatter,
            FormulaEvaluator evaluator,
            ConversionOptions options,
            List<LegacyPicture> legacyPictures) throws IOException {
        CellRangeAddress area = printArea(workbook, sheet, sheetIndex);
        PageGeometry geometry = pageGeometry(sheet, options);
        XSSFPrintSetup setup = sheet.getPrintSetup();
        List<CellRangeAddress> merged = sheet.getMergedRegions();
        boolean hasPrintArea = printAreaValue(workbook, sheet, sheetIndex) != null;
        float verticalScaleLimit = explicitScale(sheet) && sheet.getVerticallyCenter()
            ? Math.min(1.0f, geometry.usableHeight() / rowsBefore(
                sheet, area.getFirstRow(), area.getLastRow() + 1))
            : 1.0f;
        for (ColumnGroup group : columnGroups(
            sheet, area, geometry, setup, hasPrintArea, verticalScaleLimit)) {
            CellRangeAddress groupArea = new CellRangeAddress(
                area.getFirstRow(),
                area.getLastRow(),
                group.firstColumn(),
                group.lastColumn());
            int startRow = area.getFirstRow();
            while (startRow <= area.getLastRow()) {
            int endRow = pageEndRow(
                sheet,
                startRow,
                area.getLastRow(),
                geometry.usableHeight(),
                group.scale());
            renderPage(
                document,
                fonts,
                workbook,
                sheet,
                groupArea,
                merged,
                group.widths(),
                startRow,
                endRow,
                group.scale(),
                geometry,
                formatter,
                        evaluator,
                        legacyPictures);
            startRow = endRow + 1;
            }
        }
    }

    private static void renderPage(
            PDDocument document,
            FontSet fonts,
            XSSFWorkbook workbook,
            XSSFSheet sheet,
            CellRangeAddress area,
            List<CellRangeAddress> merged,
            float[] columnWidths,
            int startRow,
            int endRow,
            float scale,
            PageGeometry geometry,
            DataFormatter formatter,
            FormulaEvaluator evaluator,
            List<LegacyPicture> legacyPictures) throws IOException {
        PDPage page = new PDPage(new PDRectangle(geometry.width(), geometry.height()));
        document.addPage(page);
        float unscaledWidth = sum(columnWidths, 0, columnWidths.length);
        float horizontalScale = scale;
        boolean centeredLegacy = !legacyPictures.isEmpty()
            && sheet.getHorizontallyCenter()
            && sheet.getVerticallyCenter();
        if (centeredLegacy) {
            horizontalScale = Math.min(
                scale * CENTERED_VML_HORIZONTAL_GEOMETRY_SCALE,
                geometry.usableWidth() / unscaledWidth);
        }
        float printedWidth = unscaledWidth * horizontalScale;
        float horizontalOffset = sheet.getHorizontallyCenter()
            ? Math.max(0.0f, (geometry.usableWidth() - printedWidth) / 2.0f)
            : 0.0f;
        float[] columnX = columnPositions(
            columnWidths,
            geometry.marginLeft() + horizontalOffset,
            horizontalScale);
        float usedHeight = rowsBefore(sheet, startRow, endRow + 1) * scale;
        float centerOffset = sheet.getVerticallyCenter()
            ? Math.max(0.0f, (geometry.usableHeight() - usedHeight) / 2.0f)
            : 0.0f;
        float[] rowTop = rowPositions(
            sheet,
            startRow,
            endRow,
            geometry.height() - geometry.marginTop() - centerOffset,
            scale);
        try (PDPageContentStream content = new PDPageContentStream(document, page)) {
            for (int rowIndex = startRow; rowIndex <= endRow; rowIndex++) {
                Row row = sheet.getRow(rowIndex);
                float cellTop = rowTop[rowIndex - startRow];
                float cellHeight = rowHeight(sheet, rowIndex) * scale;
                for (int column = area.getFirstColumn(); column <= area.getLastColumn(); column++) {
                    int localColumn = column - area.getFirstColumn();
                    float x = columnX[localColumn];
                    float width = columnWidths[localColumn] * horizontalScale;
                    Cell cell = row == null ? null : row.getCell(column, Row.MissingCellPolicy.RETURN_NULL_AND_BLANK);
                    XSSFCellStyle style = cell == null ? null : (XSSFCellStyle) cell.getCellStyle();
                    CellRangeAddress merge = mergedAt(merged, rowIndex, column);
                        boolean mergeOrigin = merge != null
                            && rowIndex == merge.getFirstRow()
                            && column == merge.getFirstColumn();
                        float textWidth = merge == null
                            ? width
                            : mergedWidth(columnWidths, area.getFirstColumn(), merge, horizontalScale);
                        float textHeight = merge == null
                            ? cellHeight
                            : mergedHeight(sheet, merge, startRow, endRow, scale);
                        if (merge == null || mergeOrigin) {
                        drawCell(
                                content,
                                style,
                                x,
                                cellTop - textHeight,
                                textWidth,
                                textHeight,
                                centeredLegacy ? CENTERED_VML_BORDER_SCALE : 1.0f);
                        }
                    if (cell == null || (merge != null
                            && !mergeOrigin)) {
                        continue;
                    }
                    String value = formatter.formatCellValue(cell, evaluator);
                    drawText(
                            content,
                            fonts,
                            workbook,
                            cell,
                            style,
                            value,
                            x,
                            cellTop - textHeight,
                            textWidth,
                            textHeight,
                            scale,
                            centeredLegacy ? 0.75f : CELL_PADDING);
                }
            }
                drawPictures(
                    document,
                    content,
                    sheet,
                    area,
                    columnWidths,
                    startRow,
                    endRow,
                    scale,
                    horizontalScale,
                    geometry,
                    horizontalOffset,
                    centerOffset,
                    legacyPictures);
        }
    }

    private static void drawCell(
            PDPageContentStream content,
            XSSFCellStyle style,
            float x,
            float y,
            float width,
            float height,
            float borderScale) throws IOException {
        if (style != null && style.getFillPattern() == FillPatternType.SOLID_FOREGROUND) {
            Color fill = color(style.getFillForegroundXSSFColor(), null);
            if (fill != null) {
                content.setNonStrokingColor(fill);
                content.addRect(x, y, width, height);
                content.fill();
            }
        }
        if (style == null) {
            return;
        }
        drawBorder(content, style.getBorderTop(), style.getTopBorderXSSFColor(), x, y + height, x + width, y + height, borderScale);
        drawBorder(content, style.getBorderBottom(), style.getBottomBorderXSSFColor(), x, y, x + width, y, borderScale);
        drawBorder(content, style.getBorderLeft(), style.getLeftBorderXSSFColor(), x, y, x, y + height, borderScale);
        drawBorder(content, style.getBorderRight(), style.getRightBorderXSSFColor(), x + width, y, x + width, y + height, borderScale);
    }

    private static void drawBorder(
            PDPageContentStream content,
            BorderStyle border,
            XSSFColor borderColor,
            float x1,
            float y1,
            float x2,
            float y2,
            float borderScale) throws IOException {
        if (border == null || border == BorderStyle.NONE) {
            return;
        }
        content.setStrokingColor(color(borderColor, Color.BLACK));
        content.setLineWidth(borderWidth(border) * borderScale);
        content.moveTo(x1, y1);
        content.lineTo(x2, y2);
        content.stroke();
    }

    private static void drawText(
            PDPageContentStream content,
            FontSet fonts,
            XSSFWorkbook workbook,
            Cell cell,
            XSSFCellStyle style,
            String value,
            float x,
            float y,
            float width,
            float height,
            float scale,
            float padding) throws IOException {
        if (value == null || value.isEmpty() || width <= padding * 2.0f || height <= 1.0f) {
            return;
        }
        XSSFFont cellFont = workbook.getFontAt(style == null ? 0 : style.getFontIndex());
        float fontSize = Math.max(4.0f, cellFont.getFontHeightInPoints() * scale);
        PDFont font = fonts.resolve(cellFont.getFontName(), value, cellFont.getBold());
        String safeValue = fonts.sanitize(font, value.replace('\t', ' ').replace('\r', '\n'));
        List<String> lines = style != null && style.getRotation() == 255
            ? safeValue.codePoints()
                .filter(codePoint -> codePoint != '\n')
                .mapToObj(Character::toString)
                .toList()
                : wrap(font, safeValue, fontSize, width - padding * 2.0f, style != null && style.getWrapText());
        float lineHeight = fontSize * 1.18f;
        float verticalPadding = padding;
        int maxLines = Math.max(1, (int) ((height - verticalPadding * 2.0f) / lineHeight));
        if (lines.size() > maxLines) {
            lines = lines.subList(0, maxLines);
        }
        float blockHeight = lines.size() * lineHeight;
        VerticalAlignment vertical = style == null ? VerticalAlignment.BOTTOM : style.getVerticalAlignment();
        float baseline = switch (vertical) {
            case TOP -> y + height - verticalPadding - fontSize;
            case CENTER -> y + (height + blockHeight) / 2.0f - fontSize;
            default -> y + verticalPadding + blockHeight - fontSize;
        };
        Color textColor = color(cellFont.getXSSFColor(), Color.BLACK);
        content.saveGraphicsState();
        content.addRect(x + 0.2f, y + 0.2f, Math.max(0.1f, width - 0.4f), Math.max(0.1f, height - 0.4f));
        content.clip();
        content.setNonStrokingColor(textColor);
        if (cellFont.getBold()) {
            content.setStrokingColor(textColor);
            content.setLineWidth(Math.max(0.15f, fontSize * 0.025f));
            content.setRenderingMode(RenderingMode.FILL_STROKE);
        }
        for (String line : lines) {
            float lineWidth = textWidth(font, line, fontSize);
            HorizontalAlignment alignment = style == null ? HorizontalAlignment.GENERAL : style.getAlignment();
            float textX = switch (alignment) {
                case CENTER, CENTER_SELECTION -> x + Math.max(padding, (width - lineWidth) / 2.0f);
                case RIGHT -> x + Math.max(padding, width - padding - lineWidth);
                default -> x + padding;
            };
            content.beginText();
            content.setFont(font, fontSize);
            content.newLineAtOffset(textX, baseline);
            content.showText(line);
            content.endText();
            baseline -= lineHeight;
        }
        content.restoreGraphicsState();
    }

    private static void drawPictures(
            PDDocument document,
            PDPageContentStream content,
            XSSFSheet sheet,
            CellRangeAddress area,
            float[] columnWidths,
            int startRow,
            int endRow,
            float scale,
            float horizontalScale,
            PageGeometry geometry,
            float horizontalOffset,
            float centerOffset,
            List<LegacyPicture> legacyPictures) throws IOException {
        XSSFDrawing drawing = sheet.getDrawingPatriarch();
        if (drawing != null) {
            for (XSSFShape shape : drawing.getShapes()) {
                if (!(shape instanceof XSSFPicture picture)) {
                    continue;
                }
                XSSFClientAnchor anchor = picture.getClientAnchor();
                if (anchor == null || anchor.getRow1() < startRow || anchor.getRow1() > endRow) {
                    continue;
                }
                try {
                    PDImageXObject image = PDImageXObject.createFromByteArray(
                            document,
                            picture.getPictureData().getData(),
                            picture.getShapeName());
                        float x = geometry.marginLeft() + horizontalOffset
                            + columnsBefore(columnWidths, area.getFirstColumn(), anchor.getCol1()) * horizontalScale
                            + anchor.getDx1() / EMU_PER_POINT * horizontalScale;
                        float top = geometry.height() - geometry.marginTop() - centerOffset
                            - rowsBefore(sheet, startRow, anchor.getRow1()) * scale
                            - anchor.getDy1() / EMU_PER_POINT * scale;
                    float width = (columnsBefore(columnWidths, area.getFirstColumn(), anchor.getCol2())
                            - columnsBefore(columnWidths, area.getFirstColumn(), anchor.getCol1())) * horizontalScale
                            + (anchor.getDx2() - anchor.getDx1()) / EMU_PER_POINT * horizontalScale;
                    float height = rowsBefore(sheet, anchor.getRow1(), anchor.getRow2()) * scale
                            + (anchor.getDy2() - anchor.getDy1()) / EMU_PER_POINT * scale;
                    if (width > 0.0f && height > 0.0f) {
                        content.drawImage(image, x, top - height, width, height);
                    }
                } catch (IllegalArgumentException ignored) {
                }
            }
        }
            for (LegacyPicture picture : legacyPictures) {
                drawLegacyPicture(
                    document,
                    content,
                    sheet,
                    area,
                    columnWidths,
                    startRow,
                    endRow,
                    scale,
                    horizontalScale,
                    geometry,
                    horizontalOffset,
                    centerOffset,
                    picture);
            }
    }

            private static void drawLegacyPicture(
                PDDocument document,
                PDPageContentStream content,
                XSSFSheet sheet,
                CellRangeAddress area,
                float[] columnWidths,
                int startRow,
                int endRow,
                float scale,
                float horizontalScale,
                PageGeometry geometry,
                float horizontalOffset,
                float centerOffset,
                LegacyPicture picture) throws IOException {
            int[] anchor = picture.anchor();
            int column1 = anchor[0];
            int row1 = anchor[2];
            int column2 = anchor[4];
            int row2 = anchor[6];
            if (row1 < startRow || row1 > endRow
                || column2 < area.getFirstColumn() || column1 > area.getLastColumn()) {
                return;
            }
            float x = geometry.marginLeft() + horizontalOffset
                + columnsBefore(columnWidths, area.getFirstColumn(), column1) * horizontalScale
                + columnWidth(sheet, column1) * anchor[1] / 1024.0f * horizontalScale;
            float top = geometry.height() - geometry.marginTop() - centerOffset
                - rowsBefore(sheet, startRow, row1) * scale
                - rowHeight(sheet, row1) * anchor[3] / 256.0f * scale;
            float width = (columnsBefore(columnWidths, area.getFirstColumn(), column2)
                - columnsBefore(columnWidths, area.getFirstColumn(), column1)) * horizontalScale
                + (columnWidth(sheet, column2) * anchor[5] / 1024.0f
                - columnWidth(sheet, column1) * anchor[1] / 1024.0f) * horizontalScale;
            float height = rowsBefore(sheet, row1, row2) * scale
                + (rowHeight(sheet, row2) * anchor[7] / 256.0f
                - rowHeight(sheet, row1) * anchor[3] / 256.0f) * scale;
            if (width <= 0.0f || height <= 0.0f) {
                return;
            }
            if (!drawVectorEmf(document, content, picture, x, top - height, width, height)) {
                PDImageXObject image = PDImageXObject.createFromByteArray(document, picture.png(), picture.path());
                content.drawImage(image, x, top - height, width, height);
            }
    }

    private static boolean drawVectorEmf(
            PDDocument document,
            PDPageContentStream content,
            LegacyPicture picture,
            float x,
            float y,
            float width,
            float height) throws IOException {
        if (!picture.path().toLowerCase(Locale.ROOT).endsWith(".emf")) {
            return false;
        }
        PdfBoxGraphics2D graphics = new PdfBoxGraphics2D(document, width, height);
        try {
            HemfPicture emf = new HemfPicture(new ByteArrayInputStream(picture.data()));
            emf.draw(graphics, new Rectangle2D.Float(0.0f, 0.0f, width, height));
        } catch (RuntimeException exception) {
            return false;
        } finally {
            graphics.dispose();
        }
        PDFormXObject form = graphics.getXFormObject();
        content.saveGraphicsState();
        try {
            content.transform(Matrix.getTranslateInstance(x, y));
            content.drawForm(form);
        } finally {
            content.restoreGraphicsState();
        }
        return true;
    }

    private static CellRangeAddress printArea(XSSFWorkbook workbook, XSSFSheet sheet, int sheetIndex) {
        String value = printAreaValue(workbook, sheet, sheetIndex);
        if (value != null && !value.isBlank()) {
            AreaReference reference = new AreaReference(value, SpreadsheetVersion.EXCEL2007);
            CellReference first = reference.getFirstCell();
            CellReference last = reference.getLastCell();
            return new CellRangeAddress(first.getRow(), last.getRow(), first.getCol(), last.getCol());
        }
        int firstRow = Math.max(0, sheet.getFirstRowNum());
        int lastRow = Math.max(firstRow, sheet.getLastRowNum());
        int lastColumn = 0;
        for (Row row : sheet) {
            lastColumn = Math.max(lastColumn, Math.max(0, row.getLastCellNum() - 1));
        }
        for (CellRangeAddress merge : sheet.getMergedRegions()) {
            lastColumn = Math.max(lastColumn, merge.getLastColumn());
        }
        return new CellRangeAddress(firstRow, lastRow, 0, lastColumn);
    }

    private static String printAreaValue(XSSFWorkbook workbook, XSSFSheet sheet, int sheetIndex) {
        String direct = workbook.getPrintArea(sheetIndex);
        if (referencesSheet(direct, sheet.getSheetName())) {
            return direct;
        }
        for (Name name : workbook.getAllNames()) {
            if ("_xlnm.Print_Area".equals(name.getNameName())
                    && referencesSheet(name.getRefersToFormula(), sheet.getSheetName())) {
                return name.getRefersToFormula();
            }
        }
        if (workbook.getCTWorkbook().isSetDefinedNames()) {
            for (CTDefinedName name : workbook.getCTWorkbook().getDefinedNames().getDefinedNameList()) {
                if ("_xlnm.Print_Area".equals(name.getName())
                        && referencesSheet(name.getStringValue(), sheet.getSheetName())) {
                    return name.getStringValue();
                }
            }
        }
        return null;
    }

    private static boolean referencesSheet(String formula, String sheetName) {
        if (formula == null || formula.isBlank()) {
            return false;
        }
        int separator = formula.indexOf('!');
        if (separator < 0) {
            return true;
        }
        String referenceSheet = formula.substring(0, separator);
        if (referenceSheet.startsWith("'") && referenceSheet.endsWith("'")) {
            referenceSheet = referenceSheet.substring(1, referenceSheet.length() - 1).replace("''", "'");
        }
        return referenceSheet.equals(sheetName);
    }

    private static PageGeometry pageGeometry(XSSFSheet sheet, ConversionOptions options) {
        PageSize configured = options.pageSize().orElse(null);
        float width;
        float height;
        if (configured != null) {
            width = configured.width();
            height = configured.height();
        } else {
            PrintSetup setup = sheet.getPrintSetup();
            boolean letter = setup.getPaperSize() == PrintSetup.LETTER_PAPERSIZE;
            width = letter ? PageSize.LETTER.width() : PageSize.A4.width();
            height = letter ? PageSize.LETTER.height() : PageSize.A4.height();
            if (setup.getLandscape()) {
                float swap = width;
                width = height;
                height = swap;
            }
        }
        float left = margin(sheet.getMargin(PageMargin.LEFT), 0.25f);
        float right = margin(sheet.getMargin(PageMargin.RIGHT), 0.25f);
        float top = margin(sheet.getMargin(PageMargin.TOP), 0.3f);
        float bottom = margin(sheet.getMargin(PageMargin.BOTTOM), 0.3f);
        return new PageGeometry(width, height, left, right, top, bottom);
    }

    private static float margin(double inches, float fallback) {
        return (float) ((inches > 0.0 ? inches : fallback) * POINTS_PER_INCH);
    }

    private static float[] columnWidths(XSSFSheet sheet, CellRangeAddress area) {
        float[] widths = new float[area.getLastColumn() - area.getFirstColumn() + 1];
        for (int column = area.getFirstColumn(); column <= area.getLastColumn(); column++) {
                widths[column - area.getFirstColumn()] = columnWidth(sheet, column);
        }
        return widths;
    }

            private static float columnWidth(XSSFSheet sheet, int column) {
            return sheet.isColumnHidden(column)
                ? 0.0f
                : Math.max(1.0f, sheet.getColumnWidthInPixels(column) * POINTS_PER_PIXEL);
            }

    private static List<ColumnGroup> columnGroups(
            XSSFSheet sheet,
            CellRangeAddress area,
            PageGeometry geometry,
            XSSFPrintSetup setup,
            boolean hasPrintArea,
            float verticalScaleLimit) {
        float[] allWidths = columnWidths(sheet, area);
        float naturalWidth = sum(allWidths, 0, allWidths.length);
        boolean fitOnePageWide = hasPrintArea || (sheet.getFitToPage() && setup.getFitWidth() == 1);
        if (fitOnePageWide || naturalWidth <= geometry.usableWidth()) {
            float scale = naturalWidth > 0.0f
                    ? Math.min(1.0f, geometry.usableWidth() / naturalWidth)
                    : 1.0f;
            scale = Math.min(scale, verticalScaleLimit);
            return List.of(new ColumnGroup(area.getFirstColumn(), area.getLastColumn(), allWidths, scale));
        }

        float scale = setup.getScale() > 0 ? setup.getScale() / 100.0f : 1.0f;
        scale = Math.min(scale, verticalScaleLimit);
        List<ColumnGroup> groups = new ArrayList<>();
        int groupStart = 0;
        float groupWidth = 0.0f;
        for (int index = 0; index < allWidths.length; index++) {
            float next = allWidths[index] * scale;
            if (index > groupStart && groupWidth + next > geometry.usableWidth()) {
                groups.add(columnGroup(area, allWidths, groupStart, index, scale));
                groupStart = index;
                groupWidth = 0.0f;
            }
            groupWidth += next;
        }
        groups.add(columnGroup(area, allWidths, groupStart, allWidths.length, scale));
        return groups;
    }

    private static boolean explicitScale(XSSFSheet sheet) {
        return sheet.getCTWorksheet().isSetPageSetup()
                && sheet.getCTWorksheet().getPageSetup().isSetScale();
    }

    private static ColumnGroup columnGroup(
            CellRangeAddress area,
            float[] allWidths,
            int start,
            int end,
            float scale) {
        float[] widths = new float[end - start];
        System.arraycopy(allWidths, start, widths, 0, widths.length);
        return new ColumnGroup(
                area.getFirstColumn() + start,
                area.getFirstColumn() + end - 1,
                widths,
                scale);
    }

    private static float[] columnPositions(float[] widths, float start, float scale) {
        float[] positions = new float[widths.length];
        float x = start;
        for (int index = 0; index < widths.length; index++) {
            positions[index] = x;
            x += widths[index] * scale;
        }
        return positions;
    }

    private static float[] rowPositions(XSSFSheet sheet, int startRow, int endRow, float start, float scale) {
        float[] positions = new float[endRow - startRow + 1];
        float y = start;
        for (int row = startRow; row <= endRow; row++) {
            positions[row - startRow] = y;
            y -= rowHeight(sheet, row) * scale;
        }
        return positions;
    }

    private static int pageEndRow(XSSFSheet sheet, int startRow, int lastRow, float available, float scale) {
        float used = 0.0f;
        int row = startRow;
        while (row <= lastRow) {
            float next = rowHeight(sheet, row) * scale;
            if (row > startRow && (used + next > available + 0.5f || sheet.isRowBroken(row - 1))) {
                break;
            }
            used += next;
            row++;
        }
        return Math.max(startRow, row - 1);
    }

    private static float rowHeight(XSSFSheet sheet, int rowIndex) {
        Row row = sheet.getRow(rowIndex);
        return row == null || row.getZeroHeight() ? sheet.getDefaultRowHeightInPoints() : row.getHeightInPoints();
    }

    private static CellRangeAddress mergedAt(List<CellRangeAddress> merged, int row, int column) {
        for (CellRangeAddress range : merged) {
            if (range.isInRange(row, column)) {
                return range;
            }
        }
        return null;
    }

    private static float mergedWidth(
            float[] columnWidths,
            int areaFirstColumn,
            CellRangeAddress merge,
            float scale) {
        int first = Math.max(0, merge.getFirstColumn() - areaFirstColumn);
        int end = Math.min(columnWidths.length, merge.getLastColumn() - areaFirstColumn + 1);
        return sum(columnWidths, first, end) * scale;
    }

    private static float mergedHeight(
            XSSFSheet sheet,
            CellRangeAddress merge,
            int pageStart,
            int pageEnd,
            float scale) {
        float height = 0.0f;
        for (int row = Math.max(pageStart, merge.getFirstRow()); row <= Math.min(pageEnd, merge.getLastRow()); row++) {
            height += rowHeight(sheet, row) * scale;
        }
        return height;
    }

    private static float columnsBefore(float[] widths, int areaFirstColumn, int column) {
        return sum(widths, 0, Math.max(0, Math.min(widths.length, column - areaFirstColumn)));
    }

    private static float rowsBefore(XSSFSheet sheet, int startRow, int endRow) {
        float height = 0.0f;
        for (int row = startRow; row < endRow; row++) {
            height += rowHeight(sheet, row);
        }
        return height;
    }

    private static float sum(float[] values, int start, int end) {
        float total = 0.0f;
        for (int index = start; index < end; index++) {
            total += values[index];
        }
        return total;
    }

    private static List<String> wrap(PDFont font, String value, float size, float width, boolean enabled)
            throws IOException {
        List<String> lines = new ArrayList<>();
        for (String paragraph : value.split("\\R", -1)) {
            if (!enabled || textWidth(font, paragraph, size) <= width) {
                lines.add(paragraph);
                continue;
            }
            StringBuilder line = new StringBuilder();
            for (int offset = 0; offset < paragraph.length();) {
                int codePoint = paragraph.codePointAt(offset);
                String character = Character.toString(codePoint);
                if (!line.isEmpty() && textWidth(font, line + character, size) > width) {
                    lines.add(line.toString());
                    line.setLength(0);
                }
                line.append(character);
                offset += Character.charCount(codePoint);
            }
            lines.add(line.toString());
        }
        return lines;
    }

    private static float textWidth(PDFont font, String value, float size) throws IOException {
        return font.getStringWidth(value) / 1000.0f * size;
    }

    private static float borderWidth(BorderStyle style) {
        return switch (style) {
            case HAIR -> 0.1f;
            case MEDIUM, MEDIUM_DASHED, MEDIUM_DASH_DOT, MEDIUM_DASH_DOT_DOT -> 1.0f;
            case THICK, DOUBLE -> 1.5f;
            default -> 0.35f;
        };
    }

    private static Color color(XSSFColor source, Color fallback) {
        if (source == null) {
            return fallback;
        }
        byte[] rgb = source.getRGB();
        if (rgb == null || rgb.length < 3) {
            byte[] argb = source.getARGB();
            if (argb == null || argb.length < 4) {
                return fallback;
            }
            rgb = new byte[]{argb[1], argb[2], argb[3]};
        }
        return new Color(Byte.toUnsignedInt(rgb[0]), Byte.toUnsignedInt(rgb[1]), Byte.toUnsignedInt(rgb[2]));
    }

    private record PageGeometry(
            float width,
            float height,
            float marginLeft,
            float marginRight,
            float marginTop,
            float marginBottom) {
        float usableWidth() {
            return width - marginLeft - marginRight;
        }

        float usableHeight() {
            return height - marginTop - marginBottom;
        }
    }

    private record ColumnGroup(int firstColumn, int lastColumn, float[] widths, float scale) {
    }

    private static final class FontSet {
        private final PDFont latin;
        private final PDFont latinBold;
        private final PDFont calibri;
        private final PDFont calibriBold;
        private final PDFont times;
        private final PDFont timesBold;
        private final PDFont cjk;
        private final PDFont cjkBold;
        private final PDFont simsun;
        private final PDFont mingliu;
        private final PDFont kaiti;

        private FontSet(
                PDFont latin,
                PDFont latinBold,
                PDFont calibri,
                PDFont calibriBold,
                PDFont times,
                PDFont timesBold,
                PDFont cjk,
                PDFont cjkBold,
                PDFont simsun,
                PDFont mingliu,
                PDFont kaiti) {
            this.latin = latin;
            this.latinBold = latinBold;
            this.calibri = calibri;
            this.calibriBold = calibriBold;
            this.times = times;
            this.timesBold = timesBold;
            this.cjk = cjk;
            this.cjkBold = cjkBold;
            this.simsun = simsun;
            this.mingliu = mingliu;
            this.kaiti = kaiti;
        }

        static FontSet load(PDDocument document) throws IOException {
            Map<String, byte[]> registered = new HashMap<>();
            for (RegisteredFont font : MiniPdf.registeredFonts()) {
                registered.put(font.name().toLowerCase(Locale.ROOT), font.data());
            }
            PDFont latin = load(document, registered, List.of("arial"), systemFonts("arial.ttf"));
            PDFont latinBold = load(document, registered, List.of("arialbd"), systemFonts("arialbd.ttf"));
            PDFont calibri = load(document, registered, List.of("calibri"), systemFonts("calibri.ttf"));
            PDFont calibriBold = load(
                    document,
                    registered,
                    List.of("calibrib"),
                    systemFonts("calibrib.ttf"));
            PDFont times = load(document, registered, List.of("times"), systemFonts("times.ttf"));
            PDFont timesBold = load(document, registered, List.of("timesbd"), systemFonts("timesbd.ttf"));
            PDFont cjk = load(
                    document,
                    registered,
                    List.of("notosanssc", "simhei", "simsun"),
                    systemFonts("NotoSansSC-VF.ttf", "simhei.ttf", "simsun.ttc"));
            PDFont simsun = load(document, registered, List.of("simsun"), systemFonts("simsun.ttc"));
            PDFont mingliu = load(document, registered, List.of("mingliu"), systemFonts("mingliu.ttc"));
            List<Path> kaitiPaths = officeCloudFonts("STKaiti");
            kaitiPaths.addAll(systemFonts("simkai.ttf"));
            PDFont kaiti = load(document, registered, List.of("stkaiti", "simkai"), kaitiPaths);
            if (latin == null) {
                latin = new PDType1Font(Standard14Fonts.FontName.HELVETICA);
            }
            if (latinBold == null) {
                latinBold = new PDType1Font(Standard14Fonts.FontName.HELVETICA_BOLD);
            }
            if (calibri == null) {
                calibri = latin;
            }
            if (calibriBold == null) {
                calibriBold = latinBold;
            }
            if (times == null) {
                times = latin;
            }
            if (timesBold == null) {
                timesBold = latinBold;
            }
            if (cjk == null) {
                cjk = latin;
            }
            PDFont cjkBold = cjk;
            if (simsun == null) {
                simsun = cjk;
            }
            if (mingliu == null) {
                mingliu = simsun;
            }
            if (kaiti == null) {
                kaiti = cjk;
            }
            return new FontSet(
                    latin,
                    latinBold,
                    calibri,
                    calibriBold,
                    times,
                    timesBold,
                    cjk,
                    cjkBold,
                    simsun,
                    mingliu,
                    kaiti);
        }

        PDFont resolve(String requested, String text, boolean bold) {
            String name = requested == null ? "" : requested.toLowerCase(Locale.ROOT);
            if (name.contains("kaiti") || name.contains("kai") || name.contains("楷")) {
                return kaiti;
            }
            if (name.contains("simsun") || name.contains("宋体")) {
                return simsun;
            }
            if (name.contains("mingliu") || name.contains("細明體") || name.contains("细明体")) {
                return mingliu;
            }
            if (text.codePoints().anyMatch(codePoint -> codePoint > 255)) {
                return bold ? cjkBold : cjk;
            }
            if (name.contains("calibri")) {
                return bold ? calibriBold : calibri;
            }
            if (name.contains("times")) {
                return bold ? timesBold : times;
            }
            return bold ? latinBold : latin;
        }

        String sanitize(PDFont font, String value) throws IOException {
            if (!(font instanceof PDType0Font)) {
                return value.chars()
                        .map(character -> character <= 255 ? character : '?')
                        .collect(StringBuilder::new, StringBuilder::appendCodePoint, StringBuilder::append)
                        .toString();
            }
            StringBuilder safe = new StringBuilder();
            for (int offset = 0; offset < value.length();) {
                int codePoint = value.codePointAt(offset);
                offset += Character.charCount(codePoint);
                if (codePoint == '\n') {
                    safe.append('\n');
                    continue;
                }
                if (Character.isISOControl(codePoint)) {
                    safe.append(' ');
                    continue;
                }
                String character = Character.toString(codePoint);
                try {
                    font.getStringWidth(character);
                    safe.append(character);
                } catch (IllegalArgumentException exception) {
                    safe.append('?');
                }
            }
            return safe.toString();
        }

        private static PDFont load(
                PDDocument document,
                Map<String, byte[]> registered,
                List<String> names,
                List<Path> paths) throws IOException {
            for (Map.Entry<String, byte[]> font : registered.entrySet()) {
                if (names.stream().anyMatch(font.getKey()::contains)) {
                    return PDType0Font.load(document, new ByteArrayInputStream(font.getValue()), true);
                }
            }
            for (Path path : paths) {
                if (Files.isRegularFile(path)) {
                    try {
                        return PDType0Font.load(document, Files.newInputStream(path), true);
                    } catch (IOException ignored) {
                    }
                }
            }
            return null;
        }

        private static List<Path> systemFonts(String... names) {
            List<Path> paths = new ArrayList<>();
            String windows = System.getenv("WINDIR");
            if (windows != null) {
                for (String name : names) {
                    paths.add(Path.of(windows, "Fonts", name));
                }
            }
            for (String name : names) {
                paths.add(Path.of("/usr/share/fonts/truetype/noto", name));
                paths.add(Path.of("/usr/share/fonts/opentype/noto", name));
                paths.add(Path.of("/System/Library/Fonts", name));
                paths.add(Path.of("/System/Library/Fonts/Supplemental", name));
            }
            return paths;
        }

        private static List<Path> officeCloudFonts(String family) {
            List<Path> paths = new ArrayList<>();
            String localAppData = System.getenv("LOCALAPPDATA");
            if (localAppData == null) {
                return paths;
            }
            Path cacheRoot = Path.of(localAppData, "Microsoft", "FontCache");
            if (!Files.isDirectory(cacheRoot)) {
                return paths;
            }
            try (var candidates = Files.walk(cacheRoot, 4)) {
                candidates
                        .filter(Files::isRegularFile)
                        .filter(path -> path.getFileName().toString().toLowerCase(Locale.ROOT).endsWith(".ttf"))
                        .filter(path -> path.getParent() != null
                                && path.getParent().getFileName().toString().equalsIgnoreCase(family))
                        .sorted()
                        .forEach(paths::add);
            } catch (IOException ignored) {
            }
            return paths;
        }
    }
}