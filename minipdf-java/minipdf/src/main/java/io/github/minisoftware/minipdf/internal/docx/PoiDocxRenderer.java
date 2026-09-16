package io.github.minisoftware.minipdf.internal.docx;

import io.github.minisoftware.minipdf.ConversionOptions;
import io.github.minisoftware.minipdf.MiniPdfException;
import io.github.minisoftware.minipdf.PageSize;
import io.github.minisoftware.minipdf.internal.SimplePdfTextRenderer;
import org.apache.pdfbox.pdmodel.PDDocument;
import org.apache.pdfbox.pdmodel.PDPage;
import org.apache.pdfbox.pdmodel.PDPageContentStream;
import org.apache.pdfbox.pdmodel.common.PDRectangle;
import org.apache.pdfbox.pdmodel.font.PDFont;
import org.apache.pdfbox.pdmodel.font.PDType1Font;
import org.apache.pdfbox.pdmodel.font.Standard14Fonts;
import org.apache.pdfbox.pdmodel.graphics.image.PDImageXObject;
import org.apache.poi.xwpf.usermodel.ParagraphAlignment;
import org.apache.poi.xwpf.usermodel.XWPFDocument;
import org.apache.poi.xwpf.usermodel.XWPFParagraph;
import org.apache.poi.xwpf.usermodel.XWPFPicture;
import org.apache.poi.xwpf.usermodel.XWPFRun;
import org.apache.poi.xwpf.usermodel.XWPFTable;
import org.apache.poi.xwpf.usermodel.XWPFTableCell;
import org.apache.poi.xwpf.usermodel.XWPFTableRow;

import java.io.ByteArrayInputStream;
import java.io.ByteArrayOutputStream;
import java.io.IOException;
import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.Map;
import java.util.regex.Matcher;
import java.util.regex.Pattern;

final class PoiDocxRenderer {
    private static final float DEFAULT_MARGIN = 54.0f;
    private static final float DEFAULT_FONT_SIZE = 11.0f;
    private static final float DEFAULT_TABLE_FONT_SIZE = 10.5f;
    private static final float CELL_HORIZONTAL_PADDING = 5.4f;
    private static final float CELL_VERTICAL_PADDING = 2.0f;
    private static final float EMUS_PER_POINT = 12_700.0f;

    private PoiDocxRenderer() {
    }

    static byte[] render(byte[] input, ConversionOptions options, PageSize documentPageSize)
            throws MiniPdfException {
        PageSize pageSize = options.pageSize().orElse(documentPageSize);
        try (XWPFDocument source = new XWPFDocument(new ByteArrayInputStream(input));
                PDDocument output = new PDDocument()) {
            if (source.getBodyElements().stream()
                    .anyMatch(element -> !(element instanceof XWPFParagraph)
                            && !(element instanceof XWPFTable))) {
                return null;
            }
            List<List<String>> text = List.of(source.getBodyElements().stream()
                    .map(element -> element instanceof XWPFTable table
                        ? table.getText()
                        : ((XWPFParagraph) element).getText())
                    .map(value -> value.replace('\n', ' ').replace('\t', ' '))
                    .toList());
            PDFont font = SimplePdfTextRenderer.loadFont(output, text);
            if (font == null && text.stream().flatMap(List::stream)
                    .flatMapToInt(String::codePoints).allMatch(codePoint -> codePoint <= 255)) {
                font = new PDType1Font(Standard14Fonts.FontName.HELVETICA);
            }
            if (font == null) {
                return null;
            }
            PDFont boldFont = SimplePdfTextRenderer.loadSystemFont(
                    output,
                    text,
                    "simsunb.ttf",
                    "msyhbd.ttc",
                    "NotoSansCJK-Bold.ttc");
            if (boldFont == null) {
                boldFont = font;
            }
            List<List<String>> paragraphText = List.of(source.getParagraphs().stream()
                    .map(XWPFParagraph::getText)
                    .toList());
            PDFont paragraphFont = SimplePdfTextRenderer.loadSystemFont(
                    output,
                    paragraphText,
                    "simhei.ttf",
                    "msyh.ttc",
                    "NotoSansCJK-Regular.ttc");
            if (paragraphFont == null) {
                paragraphFont = font;
            }
            List<List<String>> latinText = List.of(paragraphText.get(0).stream()
                    .map(PoiDocxRenderer::latinText)
                    .toList());
            PDFont timesFont = SimplePdfTextRenderer.loadSystemFont(
                    output,
                    latinText,
                    "times.ttf");
            if (timesFont == null) {
                timesFont = font;
            }
            PDFont arialFont = SimplePdfTextRenderer.loadSystemFont(
                    output,
                    latinText,
                    "arial.ttf");
            if (arialFont == null) {
                arialFont = timesFont;
            }
            PDFont kaiFont = SimplePdfTextRenderer.loadSystemFont(
                    output,
                    paragraphText,
                    "simkai.ttf");
            PDFont fangSongFont = SimplePdfTextRenderer.loadSystemFont(
                    output,
                    paragraphText,
                    "simfang.ttf");
            ParagraphFonts paragraphFonts = new ParagraphFonts(
                    paragraphFont,
                    font,
                    paragraphFont,
                    kaiFont == null ? paragraphFont : kaiFont,
                    fangSongFont == null ? paragraphFont : fangSongFont,
                    timesFont,
                    arialFont,
                    defaultFontFamily(source, XWPFRun.FontCharRange.ascii),
                    defaultFontFamily(source, XWPFRun.FontCharRange.eastAsia));

            float margin = pageMargin(source, DEFAULT_MARGIN);
            float linePitch = documentLinePitch(source);
                    PageContext context = new PageContext(output, pageSize, margin, linePitch, 10.0f);
                    for (var element : source.getBodyElements()) {
                if (element instanceof XWPFParagraph paragraph) {
                        renderParagraph(context, paragraph, paragraphFonts);
                } else if (element instanceof XWPFTable table) {
                    renderTable(context, table, font, boldFont);
                }
            }
            context.close();
            ByteArrayOutputStream bytes = new ByteArrayOutputStream();
            output.save(bytes);
            return bytes.toByteArray();
        } catch (IOException | RuntimeException exception) {
            throw new MiniPdfException(
                    MiniPdfException.Kind.IO,
                    "failed to render structured DOCX: " + exception.getMessage(),
                    exception);
        }
    }

    private static void renderParagraph(
            PageContext context,
            XWPFParagraph paragraph,
            ParagraphFonts fonts)
            throws IOException {
        float fontSize = paragraphFontSize(paragraph, DEFAULT_FONT_SIZE);
        context.moveDown(twipsToPoints(paragraph.getSpacingBefore()));
        String text = renderableText(paragraph.getText());
        float leftIndent = indentationToPoints(paragraph.getIndentationLeft());
        float rightIndent = indentationToPoints(paragraph.getIndentationRight());
        float availableWidth = context.pageSize.width() - context.margin * 2.0f - leftIndent - rightIndent;
        List<RunSegment> segments = paragraphSegments(paragraph, fonts, fontSize);
        float segmentWidth = 0.0f;
        for (RunSegment segment : segments) {
            segmentWidth += segment.leadingSpacing()
                    + textWidth(segment.font(), segment.text(), segment.fontSize());
        }
        boolean hasPictures = paragraph.getRuns().stream()
                .anyMatch(run -> !run.getEmbeddedPictures().isEmpty());
        if (!segments.isEmpty()
                && !hasPictures
                && segments.stream().map(RunSegment::text).reduce("", String::concat).equals(text)
                && segmentWidth <= availableWidth) {
            float maxFontSize = segments.stream()
                    .map(RunSegment::fontSize)
                    .max(Float::compare)
                    .orElse(fontSize);
                float naturalLineHeight = maxFontSize * 1.2f;
                float lineHeight = gridLineHeight(naturalLineHeight, context.linePitch);
            context.ensureSpace(lineHeight);
                float leading = Math.max(0.0f, lineHeight - naturalLineHeight) / 2.0f;
                float baseline = context.y - leading - maxFontSize;
            float x = context.margin + leftIndent;
            if (paragraph.getAlignment() == ParagraphAlignment.CENTER) {
                    x = centeredTextX(
                            context.pageSize.width(), leftIndent, rightIndent, segmentWidth);
            } else if (paragraph.getAlignment() == ParagraphAlignment.RIGHT) {
                x = context.pageSize.width() - context.margin - rightIndent - segmentWidth;
            }
            for (RunSegment segment : segments) {
                x += segment.leadingSpacing();
                showText(context.content, segment.font(), segment.fontSize(), segment.text(), x, baseline);
                x += textWidth(segment.font(), segment.text(), segment.fontSize());
            }
            context.y -= lineHeight;
            context.moveDown(twipsToPoints(paragraph.getSpacingAfter()));
            if (startsNewSection(paragraph)) {
                context.newPage();
            }
            return;
        }
        PDFont font = fonts.fallback();
        List<String> lines = wrap(font, text, fontSize, availableWidth);
        float lineHeight = gridLineHeight(fontSize * 1.2f, context.linePitch);
        float firstBaseline = 0.0f;
        for (int index = 0; index < lines.size(); index++) {
            context.ensureSpace(lineHeight);
            float baseline = context.y - fontSize;
            if (index == 0) {
                firstBaseline = baseline;
            }
            String line = lines.get(index);
            float width = textWidth(font, line, fontSize);
            float x = context.margin + leftIndent;
            if (paragraph.getAlignment() == ParagraphAlignment.CENTER) {
                x = centeredTextX(context.pageSize.width(), leftIndent, rightIndent, width);
            } else if (paragraph.getAlignment() == ParagraphAlignment.RIGHT) {
                x = context.pageSize.width() - context.margin - rightIndent - width;
            }
            showText(context.content, font, fontSize, line, x, baseline);
            context.y -= lineHeight;
        }
        renderPictures(context, paragraph, firstBaseline);
        context.moveDown(twipsToPoints(paragraph.getSpacingAfter()));
        if (startsNewSection(paragraph)) {
            context.newPage();
        }
    }

    private static boolean startsNewSection(XWPFParagraph paragraph) {
        if (!paragraph.getCTP().isSetPPr() || !paragraph.getCTP().getPPr().isSetSectPr()) {
            return false;
        }
        var section = paragraph.getCTP().getPPr().getSectPr();
        return !section.isSetType()
                || section.getType().getVal() == null
                || !"continuous".equals(section.getType().getVal().toString());
    }

    private static void renderPictures(PageContext context, XWPFParagraph paragraph, float baseline)
            throws IOException {
        for (XWPFRun run : paragraph.getRuns()) {
            for (XWPFPicture picture : run.getEmbeddedPictures()) {
                if (picture.getPictureData() == null) {
                    continue;
                }
                float width = (float) (picture.getWidth() / EMUS_PER_POINT);
                float height = (float) (picture.getDepth() / EMUS_PER_POINT);
                if (width < 1.0f || height < 1.0f) {
                    width = 52.0f;
                    height = 32.0f;
                }
                PDImageXObject image;
                try {
                    image = PDImageXObject.createFromByteArray(
                            context.document,
                            picture.getPictureData().getData(),
                            picture.getDescription());
                } catch (IOException exception) {
                    continue;
                }
                float marginLeft = stylePoints(run.getCTR().xmlText(), "margin-left");
                float x = Float.isNaN(marginLeft)
                        ? context.pageSize.width() - context.margin - width
                        : context.margin + marginLeft + 7.0f;
                context.content.drawImage(image, x, baseline - height * 0.15f - 7.0f, width, height);
            }
        }
    }

        private static void renderTable(
            PageContext context,
            XWPFTable table,
            PDFont font,
            PDFont boldFont) throws IOException {
        float tableWidth = twipsToPoints(table.getWidth());
        if (tableWidth <= 0.0f) {
            tableWidth = context.pageSize.width() - context.margin * 2.0f;
        }
        float x = (context.pageSize.width() - tableWidth) / 2.0f;
        boolean compact = tableWidth < 200.0f;
        List<Float> columnWidths = columnWidths(table, tableWidth);
        float verticalPadding = compact ? 0.0f : columnWidths.size() > 2 ? 2.3f : 1.5f;
        List<Float> rowHeights = new ArrayList<>();
        for (XWPFTableRow row : table.getRows()) {
            rowHeights.add(rowHeight(row, font, columnWidths, compact, verticalPadding, context.linePitch));
        }
        float tableHeight = sum(rowHeights, rowHeights.size());
        float spacingBefore = compact ? 4.0f : table.getRows().size() == 1 ? 6.0f : 3.0f;
        context.moveDown(spacingBefore);
        context.ensureSpace(tableHeight);

        float rowTop = context.y;
        for (int rowIndex = 0; rowIndex < table.getRows().size(); rowIndex++) {
            XWPFTableRow row = table.getRows().get(rowIndex);
            float rowHeight = rowHeights.get(rowIndex);
            float cellX = x;
            int column = 0;
            for (XWPFTableCell cell : row.getTableCells()) {
                float width = twipsToPoints(cell.getWidth());
                if (width <= 0.0f) {
                    width = column < columnWidths.size() ? columnWidths.get(column) : tableWidth;
                }
                drawCellBorder(context, table, cell, cellX, rowTop, width, rowHeight);
                cellX += width;
                while (column < columnWidths.size() && cellX > x + sum(columnWidths, column + 1) - 0.5f) {
                    column++;
                }
            }
            rowTop -= rowHeight;
        }

        rowTop = context.y;
        Map<Integer, MergeLine> mergeLines = new HashMap<>();
        for (int rowIndex = 0; rowIndex < table.getRows().size(); rowIndex++) {
            XWPFTableRow row = table.getRows().get(rowIndex);
            float cellX = x;
            int column = 0;
            for (int cellIndex = 0; cellIndex < row.getTableCells().size(); cellIndex++) {
                XWPFTableCell cell = row.getTableCells().get(cellIndex);
                float width = twipsToPoints(cell.getWidth());
                if (width <= 0.0f) {
                    width = column < columnWidths.size() ? columnWidths.get(column) : tableWidth;
                }
                if (compact && isVerticallyMerged(cell)) {
                    MergeLine mergeLine;
                    if (isMergeRestart(cell)) {
                        mergeLine = new MergeLine(cell, 0);
                    } else {
                        MergeLine previous = mergeLines.get(cellIndex);
                        mergeLine = previous == null
                                ? new MergeLine(cell, 0)
                                : new MergeLine(previous.cell(), previous.index() + 1);
                    }
                    mergeLines.put(cellIndex, mergeLine);
                    float mergeWidth = twipsToPoints(mergeLine.cell().getWidth());
                    drawCompactMergeLine(
                            context,
                            mergeLine,
                            font,
                            boldFont,
                            cellX,
                            rowTop,
                            mergeWidth > 0.0f ? mergeWidth : width);
                } else if (!isMergeContinuation(cell)) {
                    mergeLines.remove(cellIndex);
                    float height = isMergeRestart(cell)
                            ? mergedHeight(table, rowHeights, rowIndex, cellIndex)
                            : rowHeights.get(rowIndex);
                    drawCellText(
                            context,
                            cell,
                            font,
                            boldFont,
                            cellX,
                            rowTop,
                            width,
                            height,
                            compact);
                }
                cellX += width;
                while (column < columnWidths.size() && cellX > x + sum(columnWidths, column + 1) - 0.5f) {
                    column++;
                }
            }
            rowTop -= rowHeights.get(rowIndex);
        }
        context.y -= tableHeight;
        if (compact) {
            context.moveDown(4.0f);
        } else if (table.getRows().size() == 1) {
            context.moveDown(2.0f);
        }
    }

    private static void drawCompactMergeLine(
            PageContext context,
            MergeLine mergeLine,
            PDFont font,
            PDFont boldFont,
            float x,
            float top,
            float width) throws IOException {
        XWPFTableCell cell = mergeLine.cell();
            PDFont cellFont = isCellBold(cell) ? boldFont : font;
        float fontSize = cellFontSize(cell, 8.0f);
        List<String> lines = cellLines(cell, cellFont, fontSize, width, 0.0f);
        if (mergeLine.index() >= lines.size()) {
            return;
        }
        showText(context.content, cellFont, fontSize, lines.get(mergeLine.index()), x, top - fontSize);
    }

    private static void drawCellBorder(
            PageContext context,
            XWPFTable table,
            XWPFTableCell cell,
            float x,
            float top,
            float width,
            float height) throws IOException {
        context.content.setStrokingColor(0, 0, 0);
        context.content.setLineWidth(0.5f);
        if (!cell.getCTTc().isSetTcPr() || !cell.getCTTc().getTcPr().isSetTcBorders()) {
            context.content.addRect(x, top - height, width, height);
            context.content.stroke();
            return;
        }
        var borders = cell.getCTTc().getTcPr().getTcBorders();
        var tableBorders = table.getCTTbl().getTblPr().isSetTblBorders()
                ? table.getCTTbl().getTblPr().getTblBorders()
                : null;
        String topStyle = borders.isSetTop()
                ? borders.getTop().getVal().toString()
                : tableBorders != null && tableBorders.isSetInsideH()
                        ? tableBorders.getInsideH().getVal().toString()
                        : null;
        String leftStyle = borders.isSetLeft()
                ? borders.getLeft().getVal().toString()
                : tableBorders != null && tableBorders.isSetInsideV()
                        ? tableBorders.getInsideV().getVal().toString()
                        : null;
        String bottomStyle = borders.isSetBottom()
                ? borders.getBottom().getVal().toString()
                : tableBorders != null && tableBorders.isSetInsideH()
                        ? tableBorders.getInsideH().getVal().toString()
                        : null;
        String rightStyle = borders.isSetRight()
                ? borders.getRight().getVal().toString()
                : tableBorders != null && tableBorders.isSetInsideV()
                        ? tableBorders.getInsideV().getVal().toString()
                        : null;
        if (isVisibleBorder(topStyle)) {
            drawBorder(context, x, top, x + width, top, topStyle);
        }
        if (isVisibleBorder(leftStyle)) {
            drawBorder(context, x, top, x, top - height, leftStyle);
        }
        if (isVisibleBorder(bottomStyle)) {
            drawBorder(context, x, top - height, x + width, top - height, bottomStyle);
        }
        if (isVisibleBorder(rightStyle)) {
            drawBorder(context, x + width, top, x + width, top - height, rightStyle);
        }
    }

    private static void drawBorder(
            PageContext context,
            float startX,
            float startY,
            float endX,
            float endY,
            String style) throws IOException {
        if (style.toLowerCase().contains("dash") || style.toLowerCase().contains("dot")) {
            context.content.setLineDashPattern(new float[] {1.0f, 1.0f}, 0.0f);
        }
        context.content.moveTo(startX, startY);
        context.content.lineTo(endX, endY);
        context.content.stroke();
        context.content.setLineDashPattern(new float[0], 0.0f);
    }

    private static boolean isVisibleBorder(String style) {
        return style != null && !"nil".equalsIgnoreCase(style) && !"none".equalsIgnoreCase(style);
    }

    private static void drawCellText(
            PageContext context,
            XWPFTableCell cell,
            PDFont font,
            PDFont boldFont,
            float x,
            float top,
            float width,
            float height,
            boolean compact) throws IOException {
        PDFont cellFont = isCellBold(cell) ? boldFont : font;
        float fontSize = cellFontSize(cell, DEFAULT_TABLE_FONT_SIZE);
        float horizontalPadding = compact ? 0.0f : CELL_HORIZONTAL_PADDING;
        float verticalPadding = compact ? 0.0f : CELL_VERTICAL_PADDING;
        List<String> lines = cellLines(cell, cellFont, fontSize, width, horizontalPadding);
        float lineHeight = gridLineHeight(fontSize * 1.35f, context.linePitch);
        float baseline = top - verticalPadding - paragraphSpacingBefore(cell) - fontSize;
        if (cell.getVerticalAlignment() == XWPFTableCell.XWPFVertAlign.CENTER) {
            float textHeight = fontSize + Math.max(0, lines.size() - 1) * lineHeight;
            baseline = top - (height - textHeight) / 2.0f - fontSize;
        }
        ParagraphAlignment alignment = cell.getParagraphs().isEmpty()
                ? ParagraphAlignment.LEFT
                : cell.getParagraphs().get(0).getAlignment();
        boolean highlighted = cell.getParagraphs().stream()
                .flatMap(paragraph -> paragraph.getRuns().stream())
                .anyMatch(run -> run.getTextHighlightColor() != null
                        && "yellow".equalsIgnoreCase(run.getTextHighlightColor().toString()));
        for (String line : lines) {
            float lineWidth = textWidth(cellFont, line, fontSize);
            float lineX = x + horizontalPadding;
            if (alignment == ParagraphAlignment.CENTER) {
                lineX = x + (width - lineWidth) / 2.0f;
            } else if (alignment == ParagraphAlignment.RIGHT) {
                lineX = x + width - horizontalPadding - lineWidth;
            }
            if (highlighted && !line.isEmpty()) {
                context.content.setNonStrokingColor(1.0f, 1.0f, 0.0f);
                context.content.addRect(lineX, baseline - 1.0f, lineWidth, fontSize + 2.0f);
                context.content.fill();
                context.content.setNonStrokingColor(0.0f, 0.0f, 0.0f);
            }
            showText(context.content, cellFont, fontSize, line, lineX, baseline);
            baseline -= lineHeight;
        }
    }

    private static List<Float> columnWidths(XWPFTable table, float tableWidth) {
        List<Float> widths = new ArrayList<>();
        if (table.getCTTbl().getTblGrid() != null) {
            for (var column : table.getCTTbl().getTblGrid().getGridColList()) {
                if (!column.isSetW()) {
                    widths.clear();
                    break;
                }
                try {
                    widths.add(Float.parseFloat(column.getW().toString()) / 20.0f);
                } catch (NumberFormatException ignored) {
                    widths.clear();
                    break;
                }
            }
        }
        if (widths.isEmpty()) {
            for (XWPFTableRow row : table.getRows()) {
                if (row.getTableCells().size() <= widths.size()) {
                    continue;
                }
                widths = row.getTableCells().stream().map(cell -> twipsToPoints(cell.getWidth())).toList();
            }
        }
        float total = sum(widths, widths.size());
        if (total <= 0.0f) {
            return List.of(tableWidth);
        }
        float scale = tableWidth / total;
        return widths.stream().map(width -> width * scale).toList();
    }

    private static float rowHeight(
            XWPFTableRow row,
            PDFont font,
            List<Float> widths,
            boolean compact,
            float verticalPadding,
            float linePitch) throws IOException {
        float height = twipsToPoints(row.getHeight());
        int column = 0;
        for (XWPFTableCell cell : row.getTableCells()) {
            if (isVerticallyMerged(cell)) {
                column++;
                continue;
            }
            float width = twipsToPoints(cell.getWidth());
            if (width <= 0.0f && column < widths.size()) {
                width = widths.get(column);
            }
            float fontSize = cellFontSize(cell, DEFAULT_TABLE_FONT_SIZE);
            float horizontalPadding = compact ? 0.0f : CELL_HORIZONTAL_PADDING;
            int lines = cellLines(cell, font, fontSize, width, horizontalPadding).size();
            float lineHeight = gridLineHeight(fontSize * 1.35f, linePitch);
                height = Math.max(
                    height,
                    paragraphSpacingBefore(cell)
                        + lines * lineHeight
                        + paragraphSpacingAfter(cell)
                        + verticalPadding);
            column++;
        }
        return height;
    }

            private static float paragraphSpacingBefore(XWPFTableCell cell) {
            return cell.getParagraphs().stream()
                .mapToInt(XWPFParagraph::getSpacingBefore)
                .max()
                .orElse(0) / 20.0f;
            }

            private static float paragraphSpacingAfter(XWPFTableCell cell) {
            return cell.getParagraphs().stream()
                .mapToInt(XWPFParagraph::getSpacingAfter)
                .max()
                .orElse(0) / 20.0f;
            }

            private static boolean isCellBold(XWPFTableCell cell) {
                return cell.getParagraphs().stream()
                        .flatMap(paragraph -> paragraph.getRuns().stream())
                        .anyMatch(XWPFRun::isBold);
            }

    private static List<String> cellLines(
            XWPFTableCell cell,
            PDFont font,
            float fontSize,
            float width,
            float padding)
            throws IOException {
        List<String> lines = new ArrayList<>();
        for (XWPFParagraph paragraph : cell.getParagraphs()) {
            String text = renderableText(paragraph.getText()).trim();
            if (cell.getCTTc().isSetTcPr() && cell.getCTTc().getTcPr().isSetNoWrap()) {
                lines.add(text);
            } else {
                lines.addAll(wrap(font, text, fontSize, cellContentWidth(width, padding)));
            }
        }
        return lines.isEmpty() ? List.of("") : lines;
    }

    static float cellContentWidth(float width, float padding) {
        return width - padding;
    }

    private static float mergedHeight(
            XWPFTable table,
            List<Float> rowHeights,
            int rowIndex,
            int cellIndex) {
        float height = rowHeights.get(rowIndex);
        for (int nextRow = rowIndex + 1; nextRow < table.getRows().size(); nextRow++) {
            XWPFTableRow row = table.getRows().get(nextRow);
            if (cellIndex >= row.getTableCells().size() || !isMergeContinuation(row.getCell(cellIndex))) {
                break;
            }
            height += rowHeights.get(nextRow);
        }
        return height;
    }

    private static boolean isVerticallyMerged(XWPFTableCell cell) {
        return cell.getCTTc().isSetTcPr() && cell.getCTTc().getTcPr().isSetVMerge();
    }

    private static boolean isMergeRestart(XWPFTableCell cell) {
        if (!isVerticallyMerged(cell) || cell.getCTTc().getTcPr().getVMerge().getVal() == null) {
            return false;
        }
        return "restart".equals(cell.getCTTc().getTcPr().getVMerge().getVal().toString());
    }

    private static boolean isMergeContinuation(XWPFTableCell cell) {
        return isVerticallyMerged(cell) && !isMergeRestart(cell);
    }

    static List<String> wrap(PDFont font, String text, float fontSize, float width) throws IOException {
        if (text.isEmpty()) {
            return List.of("");
        }
        List<String> lines = new ArrayList<>();
        StringBuilder line = new StringBuilder();
        for (int offset = 0; offset < text.length();) {
            int codePoint = text.codePointAt(offset);
            offset += Character.charCount(codePoint);
            line.appendCodePoint(codePoint);
            while (line.codePointCount(0, line.length()) > 1
                    && textWidth(font, line.toString(), fontSize) > width) {
                int breakOffset = lastWrapBoundary(line);
                if (breakOffset <= 0) {
                    breakOffset = line.offsetByCodePoints(line.length(), -1);
                }
                String completed = line.substring(0, breakOffset).stripTrailing();
                String remainder = line.substring(breakOffset).stripLeading();
                if (!completed.isEmpty()) {
                    lines.add(completed);
                }
                line.setLength(0);
                line.append(remainder);
            }
        }
        lines.add(line.toString());
        return lines;
    }

    private static int lastWrapBoundary(StringBuilder text) {
        int rightOffset = text.length();
        int right = text.codePointBefore(rightOffset);
        while (rightOffset > 0) {
            int leftOffset = text.offsetByCodePoints(rightOffset, -1);
            if (leftOffset == 0) {
                return -1;
            }
            int left = text.codePointBefore(leftOffset);
            if (isWrapBoundary(left, right)) {
                return leftOffset;
            }
            rightOffset = leftOffset;
            right = left;
        }
        return -1;
    }

    private static boolean isWrapBoundary(int left, int right) {
        if (Character.isWhitespace(left) || Character.isWhitespace(right) || left == '-') {
            return true;
        }
        return (isEastAsian(left) || isEastAsian(right))
                && !isOpeningPunctuation(left)
                && !isClosingPunctuation(right);
    }

    private static boolean isOpeningPunctuation(int codePoint) {
        return codePoint == '(' || codePoint == '[' || codePoint == '{'
                || codePoint == '\u3008' || codePoint == '\u300a' || codePoint == '\u300c'
                || codePoint == '\u300e' || codePoint == '\u3010' || codePoint == '\uff08';
    }

    private static boolean isClosingPunctuation(int codePoint) {
        return codePoint == ')' || codePoint == ']' || codePoint == '}'
                || codePoint == '\u3001' || codePoint == '\u3002' || codePoint == '\u3009'
                || codePoint == '\u300b' || codePoint == '\u300d' || codePoint == '\u300f'
                || codePoint == '\u3011' || codePoint == '\uff09';
    }

    private record MergeLine(XWPFTableCell cell, int index) {
    }

    record RunSegment(String text, PDFont font, float fontSize, float leadingSpacing) {
    }

    record ParagraphFonts(
            PDFont fallback,
            PDFont simSun,
            PDFont simHei,
            PDFont kai,
            PDFont fangSong,
            PDFont times,
            PDFont arial,
            String defaultAsciiFamily,
            String defaultEastAsiaFamily) {
        private PDFont resolve(XWPFRun run, int codePoint) {
            boolean eastAsian = usesEastAsianFontSlot(codePoint);
            XWPFRun.FontCharRange range = eastAsian
                    ? XWPFRun.FontCharRange.eastAsia
                    : XWPFRun.FontCharRange.ascii;
            String family = run.getFontFamily(range);
            if (family == null || family.isBlank()) {
                family = eastAsian ? defaultEastAsiaFamily : defaultAsciiFamily;
            }
            String normalized = family == null ? "" : family.toLowerCase();
            if (normalized.contains("\u5b8b\u4f53") || normalized.contains("simsun")) {
                return simSun;
            }
            if (normalized.contains("\u9ed1\u4f53") || normalized.contains("simhei")) {
                return simHei;
            }
            if (normalized.contains("\u6977\u4f53") || normalized.contains("simkai")) {
                return kai;
            }
            if (normalized.contains("\u4eff\u5b8b") || normalized.contains("simfang")) {
                return fangSong;
            }
            if (normalized.contains("arial")) {
                return arial;
            }
            if (normalized.contains("times")) {
                return times;
            }
            return eastAsian ? simSun : fallback;
        }
    }

    private static boolean usesEastAsianFontSlot(int codePoint) {
        Character.UnicodeBlock block = Character.UnicodeBlock.of(codePoint);
        return isEastAsian(codePoint)
                || block == Character.UnicodeBlock.CJK_SYMBOLS_AND_PUNCTUATION
                || block == Character.UnicodeBlock.HALFWIDTH_AND_FULLWIDTH_FORMS;
    }

    static List<RunSegment> paragraphSegments(
            XWPFParagraph paragraph,
            ParagraphFonts fonts,
            float fallbackFontSize) {
        List<RunSegment> segments = new ArrayList<>();
        for (XWPFRun run : paragraph.getRuns()) {
            String text = renderableText(run.text());
            if (text.isEmpty()) {
                continue;
            }
            float fontSize = run.getFontSizeAsDouble() == null || run.getFontSizeAsDouble() <= 0.0
                    ? fallbackFontSize
                    : run.getFontSizeAsDouble().floatValue();
            StringBuilder segmentText = new StringBuilder();
            PDFont segmentFont = null;
            float segmentLeadingSpacing = 0.0f;
            int previous = -1;
            for (int offset = 0; offset < text.length();) {
                int codePoint = text.codePointAt(offset);
                offset += Character.charCount(codePoint);
                PDFont codePointFont = fonts.resolve(run, codePoint);
                if (segmentFont != null && codePointFont != segmentFont) {
                    float spacing = isEastAsianLatinBoundary(previous, codePoint)
                            ? fontSize * 0.25f
                            : 0.0f;
                    segments.add(new RunSegment(
                            segmentText.toString(), segmentFont, fontSize, segmentLeadingSpacing));
                    segmentText.setLength(0);
                    segmentFont = codePointFont;
                    segmentLeadingSpacing = spacing;
                    segmentText.appendCodePoint(codePoint);
                } else {
                    segmentFont = codePointFont;
                    segmentText.appendCodePoint(codePoint);
                }
                previous = codePoint;
            }
            if (!segmentText.isEmpty()) {
                segments.add(new RunSegment(
                        segmentText.toString(), segmentFont, fontSize, segmentLeadingSpacing));
            }
        }
        return segments;
    }

    private static String latinText(String text) {
        return text.codePoints()
                .filter(codePoint -> codePoint <= 0xff)
                .collect(StringBuilder::new, StringBuilder::appendCodePoint, StringBuilder::append)
                .toString();
    }

    private static String defaultFontFamily(XWPFDocument document, XWPFRun.FontCharRange range) {
        if (document.getStyles() == null || document.getStyles().getCtStyles() == null) {
            return null;
        }
        var styles = document.getStyles().getCtStyles();
        if (!styles.isSetDocDefaults()
                || !styles.getDocDefaults().isSetRPrDefault()
                || !styles.getDocDefaults().getRPrDefault().isSetRPr()) {
            return null;
        }
        var properties = styles.getDocDefaults().getRPrDefault().getRPr();
        if (properties.sizeOfRFontsArray() == 0) {
            return null;
        }
        var fonts = properties.getRFontsArray(0);
        return switch (range) {
            case ascii -> fonts.isSetAscii() ? fonts.getAscii() : fonts.getHAnsi();
            case eastAsia -> fonts.isSetEastAsia() ? fonts.getEastAsia() : null;
            default -> null;
        };
    }

    private static float paragraphFontSize(XWPFParagraph paragraph, float fallback) {
        Float runSize = paragraph.getRuns().stream()
                .map(XWPFRun::getFontSizeAsDouble)
                .filter(size -> size != null && size > 0.0)
                .map(Double::floatValue)
                .max(Float::compare)
                .orElse(null);
        if (runSize != null) {
            return runSize;
        }
        if (paragraph.getCTP().isSetPPr()
                && paragraph.getCTP().getPPr().isSetRPr()
                && paragraph.getCTP().getPPr().getRPr().sizeOfSzArray() > 0) {
            try {
                return Float.parseFloat(
                        paragraph.getCTP().getPPr().getRPr().getSzArray(0).getVal().toString()) / 2.0f;
            } catch (NumberFormatException ignored) {
                return fallback;
            }
        }
        return fallback;
    }

    private static float cellFontSize(XWPFTableCell cell, float fallback) {
        return cell.getParagraphs().stream()
                .map(paragraph -> paragraphFontSize(paragraph, fallback))
                .max(Float::compare)
                .orElse(fallback);
    }

    private static float pageMargin(XWPFDocument document, float fallback) {
        var body = document.getDocument().getBody();
        if (!body.isSetSectPr() || !body.getSectPr().isSetPgMar()) {
            return fallback;
        }
        try {
            return Float.parseFloat(body.getSectPr().getPgMar().getLeft().toString()) / 20.0f;
        } catch (NumberFormatException exception) {
            return fallback;
        }
    }

    private static float documentLinePitch(XWPFDocument document) {
        var body = document.getDocument().getBody();
        if (!body.isSetSectPr() || !body.getSectPr().isSetDocGrid()
                || body.getSectPr().getDocGrid().getLinePitch() == null) {
            return 0.0f;
        }
        try {
            return Float.parseFloat(body.getSectPr().getDocGrid().getLinePitch().toString()) / 20.0f;
        } catch (NumberFormatException exception) {
            return 0.0f;
        }
    }

    private static float textWidth(PDFont font, String text, float fontSize) throws IOException {
        return font.getStringWidth(text) / 1000.0f * fontSize
                + eastAsianBoundaryCount(text) * fontSize * 0.25f;
    }

    static int eastAsianBoundaryCount(String text) {
        int boundaries = 0;
        int previous = -1;
        for (int offset = 0; offset < text.length();) {
            int codePoint = text.codePointAt(offset);
            offset += Character.charCount(codePoint);
            if (previous >= 0 && isEastAsianLatinBoundary(previous, codePoint)) {
                boundaries++;
            }
            previous = codePoint;
        }
        return boundaries;
    }

    private static boolean isEastAsianLatinBoundary(int left, int right) {
        return isEastAsian(left) && isLatinOrDigit(right)
                || isLatinOrDigit(left) && isEastAsian(right);
    }

    private static boolean isEastAsian(int codePoint) {
        Character.UnicodeScript script = Character.UnicodeScript.of(codePoint);
        return script == Character.UnicodeScript.HAN
                || script == Character.UnicodeScript.HIRAGANA
                || script == Character.UnicodeScript.KATAKANA
                || script == Character.UnicodeScript.HANGUL;
    }

    private static boolean isLatinOrDigit(int codePoint) {
        return Character.isDigit(codePoint)
                || Character.UnicodeScript.of(codePoint) == Character.UnicodeScript.LATIN;
    }

    private static String renderableText(String text) {
        return text.replace('\r', ' ').replace('\n', ' ').replace('\t', ' ');
    }

    private static float gridLineHeight(float naturalHeight, float linePitch) {
        if (linePitch <= 0.0f) {
            return naturalHeight;
        }
        return (float) Math.ceil(naturalHeight / linePitch) * linePitch;
    }

    static float centeredTextX(float pageWidth, float leftIndent, float rightIndent, float textWidth) {
        return (pageWidth + leftIndent - rightIndent - textWidth) / 2.0f;
    }

    private static float stylePoints(String xml, String property) {
        Matcher matcher = Pattern.compile(property + ":(-?[0-9.]+)pt").matcher(xml);
        return matcher.find() ? Float.parseFloat(matcher.group(1)) : Float.NaN;
    }

    private static void showText(
            PDPageContentStream content,
            PDFont font,
            float fontSize,
            String text,
            float x,
            float y) throws IOException {
        if (text.isEmpty()) {
            return;
        }
        content.beginText();
        content.setFont(font, fontSize);
        content.newLineAtOffset(x, y);
        if (eastAsianBoundaryCount(text) == 0) {
            content.showText(text);
        } else {
            List<Object> positioned = new ArrayList<>();
            int start = 0;
            int previous = -1;
            for (int offset = 0; offset < text.length();) {
                int codePoint = text.codePointAt(offset);
                if (previous >= 0 && isEastAsianLatinBoundary(previous, codePoint)) {
                    positioned.add(text.substring(start, offset));
                    positioned.add(-250.0f);
                    start = offset;
                }
                offset += Character.charCount(codePoint);
                previous = codePoint;
            }
            positioned.add(text.substring(start));
            content.showTextWithPositioning(positioned.toArray());
        }
        content.endText();
    }

    private static float sum(List<Float> values, int count) {
        float sum = 0.0f;
        for (int index = 0; index < count && index < values.size(); index++) {
            sum += values.get(index);
        }
        return sum;
    }

    private static float twipsToPoints(int twips) {
        return Math.max(0, twips) / 20.0f;
    }

    static float indentationToPoints(int twips) {
        return twips == -1 ? 0.0f : twips / 20.0f;
    }

    private static final class PageContext implements AutoCloseable {
        private final PDDocument document;
        private final PageSize pageSize;
        private final float margin;
        private final float linePitch;
        private PDPageContentStream content;
        private float y;
        private int pageCount;

        private PageContext(
                PDDocument document,
                PageSize pageSize,
                float margin,
                float linePitch,
                float topOffset)
                throws IOException {
            this.document = document;
            this.pageSize = pageSize;
            this.margin = margin;
            this.linePitch = linePitch;
            newPage(topOffset);
        }

        private void ensureSpace(float height) throws IOException {
            if (y - height < margin) {
                newPage();
            }
        }

        private void moveDown(float amount) throws IOException {
            ensureSpace(amount);
            y -= amount;
        }

        private void newPage() throws IOException {
            newPage(0.0f);
        }

        private void newPage(float topOffset) throws IOException {
            if (content != null) {
                content.close();
            }
            PDPage page = new PDPage(new PDRectangle(pageSize.width(), pageSize.height()));
            document.addPage(page);
            pageCount++;
            content = new PDPageContentStream(document, page);
            y = pageSize.height() - margin - topOffset;
        }

        @Override
        public void close() throws IOException {
            if (content != null) {
                content.close();
                content = null;
            }
        }
    }
}