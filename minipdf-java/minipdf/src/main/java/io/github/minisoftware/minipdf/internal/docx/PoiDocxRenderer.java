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

            float margin = pageMargin(source, DEFAULT_MARGIN);
            PageContext context = new PageContext(output, pageSize, margin, 10.0f);
            for (var element : source.getBodyElements()) {
                if (element instanceof XWPFParagraph paragraph) {
                    renderParagraph(context, paragraph, paragraphFont);
                } else if (element instanceof XWPFTable table) {
                    renderTable(context, table, font, boldFont);
                }
            }
            context.close();
            if (context.pageCount > 1) {
                return null;
            }
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

    private static void renderParagraph(PageContext context, XWPFParagraph paragraph, PDFont font)
            throws IOException {
        float fontSize = paragraphFontSize(paragraph, DEFAULT_FONT_SIZE);
        context.moveDown(twipsToPoints(paragraph.getSpacingBefore()));
        context.ensureSpace(fontSize * 1.2f);
        float baseline = context.y - fontSize;
        String text = paragraph.getText();
        if (!text.isEmpty()) {
            float width = textWidth(font, text, fontSize);
            float x = context.margin + twipsToPoints(paragraph.getIndentationLeft());
            if (paragraph.getAlignment() == ParagraphAlignment.CENTER) {
                x = (context.pageSize.width() - width) / 2.0f;
            } else if (paragraph.getAlignment() == ParagraphAlignment.RIGHT) {
                x = context.pageSize.width() - context.margin - width;
            }
            showText(context.content, font, fontSize, text, x, baseline);
        }
        renderPictures(context, paragraph, baseline);
        context.y = baseline - fontSize * 0.2f - twipsToPoints(paragraph.getSpacingAfter());
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
                PDImageXObject image = PDImageXObject.createFromByteArray(
                        context.document,
                        picture.getPictureData().getData(),
                        picture.getDescription());
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
            rowHeights.add(rowHeight(row, font, columnWidths, compact, verticalPadding));
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
                drawCellBorder(context, cell, cellX, rowTop, width, rowHeight);
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
                            isMergeRestart(cell),
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
        if (borders.isSetTop() && isVisibleBorder(borders.getTop().getVal().toString())) {
            drawBorder(context, x, top, x + width, top, borders.getTop().getVal().toString());
        }
        if (borders.isSetLeft() && isVisibleBorder(borders.getLeft().getVal().toString())) {
            drawBorder(context, x, top, x, top - height, borders.getLeft().getVal().toString());
        }
        if (borders.isSetBottom() && isVisibleBorder(borders.getBottom().getVal().toString())) {
            drawBorder(context, x, top - height, x + width, top - height,
                    borders.getBottom().getVal().toString());
        }
        if (borders.isSetRight() && isVisibleBorder(borders.getRight().getVal().toString())) {
            drawBorder(context, x + width, top, x + width, top - height,
                    borders.getRight().getVal().toString());
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
        return !"nil".equalsIgnoreCase(style) && !"none".equalsIgnoreCase(style);
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
            boolean merged,
            boolean compact) throws IOException {
        PDFont cellFont = isCellBold(cell) ? boldFont : font;
        float fontSize = cellFontSize(cell, 9.0f);
        float horizontalPadding = compact ? 0.0f : CELL_HORIZONTAL_PADDING;
        float verticalPadding = compact ? 0.0f : CELL_VERTICAL_PADDING;
        List<String> lines = cellLines(cell, cellFont, fontSize, width, horizontalPadding);
        float lineHeight = fontSize * 1.35f;
        float baseline = top - verticalPadding - paragraphSpacingBefore(cell) - fontSize;
        if (merged && cell.getVerticalAlignment() == XWPFTableCell.XWPFVertAlign.CENTER) {
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
            float verticalPadding) throws IOException {
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
            float fontSize = cellFontSize(cell, 9.0f);
            float horizontalPadding = compact ? 0.0f : CELL_HORIZONTAL_PADDING;
            int lines = cellLines(cell, font, fontSize, width, horizontalPadding).size();
                height = Math.max(
                    height,
                    paragraphSpacingBefore(cell)
                        + lines * fontSize * 1.35f
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
        String text = cell.getText().replace('\t', ' ').trim();
        if (cell.getCTTc().isSetTcPr() && cell.getCTTc().getTcPr().isSetNoWrap()) {
            return List.of(text);
        }
        return wrap(font, text, fontSize, width - padding * 2.0f);
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

    private static List<String> wrap(PDFont font, String text, float fontSize, float width) throws IOException {
        if (text.isEmpty()) {
            return List.of("");
        }
        List<String> lines = new ArrayList<>();
        StringBuilder line = new StringBuilder();
        for (int offset = 0; offset < text.length();) {
            int codePoint = text.codePointAt(offset);
            offset += Character.charCount(codePoint);
            String next = line + Character.toString(codePoint);
            if (!line.isEmpty() && textWidth(font, next, fontSize) > width) {
                if (isClosingPunctuation(codePoint) && line.codePointCount(0, line.length()) > 1) {
                    int lastOffset = line.offsetByCodePoints(line.length(), -1);
                    String last = line.substring(lastOffset);
                    line.setLength(lastOffset);
                    lines.add(line.toString());
                    line.setLength(0);
                    line.append(last);
                } else if (endsWithOpeningPunctuation(line)
                        && line.codePointCount(0, line.length()) > 1) {
                    int lastOffset = line.offsetByCodePoints(line.length(), -1);
                    String opening = line.substring(lastOffset);
                    line.setLength(lastOffset);
                    lines.add(line.toString());
                    line.setLength(0);
                    line.append(opening);
                } else {
                    lines.add(line.toString());
                    line.setLength(0);
                }
            }
            line.appendCodePoint(codePoint);
        }
        lines.add(line.toString());
        return lines;
    }

    private static boolean endsWithOpeningPunctuation(StringBuilder text) {
        int codePoint = text.codePointBefore(text.length());
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

    private static float textWidth(PDFont font, String text, float fontSize) throws IOException {
        return font.getStringWidth(text) / 1000.0f * fontSize;
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
        content.showText(text);
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

    private static final class PageContext implements AutoCloseable {
        private final PDDocument document;
        private final PageSize pageSize;
        private final float margin;
        private PDPageContentStream content;
        private float y;
        private int pageCount;

        private PageContext(PDDocument document, PageSize pageSize, float margin, float topOffset)
                throws IOException {
            this.document = document;
            this.pageSize = pageSize;
            this.margin = margin;
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