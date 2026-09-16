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
import org.apache.poi.xwpf.usermodel.IBodyElement;
import org.apache.poi.xwpf.usermodel.ParagraphAlignment;
import org.apache.poi.xwpf.usermodel.XWPFDocument;
import org.apache.poi.xwpf.usermodel.XWPFFooter;
import org.apache.poi.xwpf.usermodel.XWPFParagraph;
import org.apache.poi.xwpf.usermodel.XWPFPicture;
import org.apache.poi.xwpf.usermodel.XWPFRun;
import org.apache.poi.xwpf.usermodel.XWPFTable;
import org.apache.poi.xwpf.usermodel.XWPFTableCell;
import org.apache.poi.xwpf.usermodel.XWPFTableRow;
import org.openxmlformats.schemas.wordprocessingml.x2006.main.CTBody;
import org.openxmlformats.schemas.wordprocessingml.x2006.main.CTFonts;
import org.openxmlformats.schemas.wordprocessingml.x2006.main.CTPageMar;
import org.openxmlformats.schemas.wordprocessingml.x2006.main.CTPPr;
import org.openxmlformats.schemas.wordprocessingml.x2006.main.CTRPr;
import org.openxmlformats.schemas.wordprocessingml.x2006.main.CTSectPr;
import org.openxmlformats.schemas.wordprocessingml.x2006.main.CTStyles;
import org.openxmlformats.schemas.wordprocessingml.x2006.main.CTTblBorders;
import org.openxmlformats.schemas.wordprocessingml.x2006.main.CTTblGridCol;
import org.openxmlformats.schemas.wordprocessingml.x2006.main.CTTcBorders;

import java.io.ByteArrayInputStream;
import java.io.ByteArrayOutputStream;
import java.io.IOException;
import java.util.ArrayList;
import java.util.Collections;
import java.util.HashMap;
import java.util.List;
import java.util.Map;
import java.util.regex.Matcher;
import java.util.regex.Pattern;
import java.util.stream.Collectors;

final class PoiDocxRenderer {
    private static final float DEFAULT_MARGIN = 54.0f;
    private static final float DEFAULT_FONT_SIZE = 11.0f;
    private static final float DEFAULT_TABLE_FONT_SIZE = 10.5f;
    private static final float CELL_HORIZONTAL_PADDING = 5.4f;
    private static final float CELL_VERTICAL_PADDING = 2.0f;
    private static final float EMUS_PER_POINT = 12_700.0f;
    private static final Pattern PAGE_FIELD = Pattern.compile("(?i)^PAGE(?:\\s|$)");
    private static final Pattern CACHED_PAGE_NUMBER = Pattern.compile("^(.*?)(\\d+)(.*)$");
    private static final AutoSpacing DEFAULT_AUTO_SPACING = new AutoSpacing(true, true);

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
            List<List<String>> text = Collections.singletonList(source.getBodyElements().stream()
                    .map(element -> element instanceof XWPFTable
                        ? ((XWPFTable) element).getText()
                        : ((XWPFParagraph) element).getText())
                    .map(value -> value.replace('\n', ' ').replace('\t', ' '))
                    .collect(Collectors.toList()));
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
                List<List<String>> paragraphText = Collections.singletonList(source.getParagraphs().stream()
                    .map(XWPFParagraph::getText)
                    .collect(Collectors.toList()));
            PDFont paragraphFont = SimplePdfTextRenderer.loadSystemFont(
                    output,
                    paragraphText,
                    "simhei.ttf",
                    "msyh.ttc",
                    "NotoSansCJK-Regular.ttc");
            if (paragraphFont == null) {
                paragraphFont = font;
            }
                List<List<String>> latinText = Collections.singletonList(paragraphText.get(0).stream()
                    .map(PoiDocxRenderer::latinText)
                    .collect(Collectors.toList()));
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

                boolean landscape = pageSize.width() > pageSize.height();
                PageMargins margins = pageMargins(source, DEFAULT_MARGIN, landscape);
            float linePitch = documentLinePitch(source);
            PageNumberFooter pageNumberFooter = pageNumberFooter(source);
            PageContext context = new PageContext(
                    output,
                    pageSize,
                    margins.left(),
                    margins.top(),
                    margins.bottom(),
                    linePitch,
                    margins.firstPageTopOffset(),
                    pageNumberFooter == null ? null : timesFont,
                    pageNumberFooter,
                    pageFooterDistance(source, margins.bottom()));
                    for (IBodyElement element : source.getBodyElements()) {
                if (element instanceof XWPFParagraph) {
                        XWPFParagraph paragraph = (XWPFParagraph) element;
                        renderParagraph(context, paragraph, paragraphFonts);
                } else if (element instanceof XWPFTable) {
                    XWPFTable table = (XWPFTable) element;
                    renderTable(context, table, paragraphFonts, boldFont);
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
        AutoSpacing autoSpacing = paragraphAutoSpacing(paragraph);
        List<RunSegment> segments = paragraphSegments(paragraph, fonts, fontSize);
        float segmentWidth = 0.0f;
        for (RunSegment segment : segments) {
            segmentWidth += segment.leadingSpacing()
                + textWidth(segment.font(), segment.text(), segment.fontSize(), autoSpacing);
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
                showText(
                        context.content,
                        segment.font(),
                        segment.fontSize(),
                        segment.text(),
                        x,
                        baseline,
                        autoSpacing);
                x += textWidth(segment.font(), segment.text(), segment.fontSize(), autoSpacing);
            }
            context.y -= lineHeight;
            context.moveDown(twipsToPoints(paragraph.getSpacingAfter()));
            if (startsNewSection(paragraph)) {
                context.newPage();
            }
            return;
        }
        PDFont font = wrappingFont(fonts, text);
        List<String> lines = wrap(font, text, fontSize, availableWidth, autoSpacing);
        float lineHeight = gridLineHeight(fontSize * 1.2f, context.linePitch);
        float firstBaseline = 0.0f;
        for (int index = 0; index < lines.size(); index++) {
            context.ensureSpace(lineHeight);
            float baseline = context.y - fontSize;
            if (index == 0) {
                firstBaseline = baseline;
            }
            String line = lines.get(index);
            float width = textWidth(font, line, fontSize, autoSpacing);
            float x = context.margin + leftIndent;
            if (paragraph.getAlignment() == ParagraphAlignment.CENTER) {
                x = centeredTextX(context.pageSize.width(), leftIndent, rightIndent, width);
            } else if (paragraph.getAlignment() == ParagraphAlignment.RIGHT) {
                x = context.pageSize.width() - context.margin - rightIndent - width;
            }
            showText(context.content, font, fontSize, line, x, baseline, autoSpacing);
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
        CTSectPr section = paragraph.getCTP().getPPr().getSectPr();
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
                } catch (IOException | IllegalArgumentException exception) {
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
            ParagraphFonts fonts,
            PDFont boldFont) throws IOException {
        PDFont font = fonts.simSun();
        float tableWidth = twipsToPoints(table.getWidth());
        if (tableWidth <= 0.0f) {
            tableWidth = context.pageSize.width() - context.margin * 2.0f;
        }
        float x = (context.pageSize.width() - tableWidth) / 2.0f;
        boolean compact = tableWidth < 200.0f;
        boolean landscape = context.pageSize.width() > context.pageSize.height();
        List<Float> columnWidths = columnWidths(table, tableWidth);
        float rowHeightPadding = compact ? 0.0f : columnWidths.size() > 2
            ? landscape ? 1.6f : 2.3f
            : 1.5f;
        List<Float> rowHeights = new ArrayList<>();
        for (XWPFTableRow row : table.getRows()) {
            rowHeights.add(rowHeight(
                    row, fonts, boldFont, columnWidths, compact, rowHeightPadding, context.linePitch));
        }
        float tableHeight = sum(rowHeights, rowHeights.size());
        float spacingBefore = compact ? 4.0f : table.getRows().size() == 1
            ? 6.0f
            : landscape ? 0.0f : 3.0f;
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
                            fonts,
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
        CTTcBorders borders = cell.getCTTc().getTcPr().getTcBorders();
        CTTblBorders tableBorders = table.getCTTbl().getTblPr().isSetTblBorders()
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
            ParagraphFonts fonts,
            PDFont boldFont,
            float x,
            float top,
            float width,
            float height,
            boolean compact) throws IOException {
        PDFont cellFont = resolvedCellFont(cell, fonts, boldFont);
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
            for (CTTblGridCol column : table.getCTTbl().getTblGrid().getGridColList()) {
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
                widths = row.getTableCells().stream()
                    .map(cell -> twipsToPoints(cell.getWidth()))
                    .collect(Collectors.toList());
            }
        }
        float total = sum(widths, widths.size());
        if (total <= 0.0f) {
            return Collections.singletonList(tableWidth);
        }
        float scale = tableWidth / total;
        return widths.stream().map(width -> width * scale).collect(Collectors.toList());
    }

    private static float rowHeight(
            XWPFTableRow row,
            ParagraphFonts fonts,
            PDFont boldFont,
            List<Float> widths,
            boolean compact,
            float rowHeightPadding,
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
                PDFont cellFont = compact ? fonts.simSun() : resolvedCellFont(cell, fonts, boldFont);
                int lines = cellLines(cell, cellFont, fontSize, width, horizontalPadding).size();
            float lineHeight = gridLineHeight(fontSize * 1.35f, linePitch);
                height = Math.max(
                    height,
                    paragraphSpacingBefore(cell)
                        + lines * lineHeight
                        + paragraphSpacingAfter(cell)
                        + rowHeightPadding);
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
        return lines.isEmpty() ? Collections.singletonList("") : lines;
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
        return wrap(font, text, fontSize, width, DEFAULT_AUTO_SPACING);
    }

    private static List<String> wrap(
            PDFont font,
            String text,
            float fontSize,
            float width,
            AutoSpacing autoSpacing) throws IOException {
        if (text.isEmpty()) {
            return Collections.singletonList("");
        }
        List<String> lines = new ArrayList<>();
        StringBuilder line = new StringBuilder();
        for (int offset = 0; offset < text.length();) {
            int codePoint = text.codePointAt(offset);
            offset += Character.charCount(codePoint);
            line.appendCodePoint(codePoint);
            while (line.codePointCount(0, line.length()) > 1
                    && textWidth(font, line.toString(), fontSize, autoSpacing) > width) {
                int breakOffset = lastWrapBoundary(line);
                if (breakOffset <= 0) {
                    breakOffset = line.offsetByCodePoints(line.length(), -1);
                }
                String completed = stripTrailing(line.substring(0, breakOffset));
                String remainder = stripLeading(line.substring(breakOffset));
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

    private static String stripLeading(String value) {
        int index = 0;
        while (index < value.length() && Character.isWhitespace(value.charAt(index))) {
            index++;
        }
        return value.substring(index);
    }

    private static String stripTrailing(String value) {
        int index = value.length();
        while (index > 0 && Character.isWhitespace(value.charAt(index - 1))) {
            index--;
        }
        return value.substring(0, index);
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

    private static final class MergeLine {
        private final XWPFTableCell cell;
        private final int index;

        private MergeLine(XWPFTableCell cell, int index) {
            this.cell = cell;
            this.index = index;
        }

        private XWPFTableCell cell() {
            return cell;
        }

        private int index() {
            return index;
        }
    }

    private static final class AutoSpacing {
        private final boolean latin;
        private final boolean digit;

        private AutoSpacing(boolean latin, boolean digit) {
            this.latin = latin;
            this.digit = digit;
        }

        private boolean latin() {
            return latin;
        }

        private boolean digit() {
            return digit;
        }
    }

    private static final class PageNumberFooter {
        private final String prefix;
        private final String suffix;

        private PageNumberFooter(String prefix, String suffix) {
            this.prefix = prefix;
            this.suffix = suffix;
        }

        private String prefix() {
            return prefix;
        }

        private String suffix() {
            return suffix;
        }

        @Override
        public boolean equals(Object value) {
            if (this == value) {
                return true;
            }
            if (!(value instanceof PageNumberFooter)) {
                return false;
            }
            PageNumberFooter other = (PageNumberFooter) value;
            return prefix.equals(other.prefix) && suffix.equals(other.suffix);
        }

        @Override
        public int hashCode() {
            return 31 * prefix.hashCode() + suffix.hashCode();
        }
    }

    private static final class PageMargins {
        private final float left;
        private final float top;
        private final float bottom;
        private final float firstPageTopOffset;

        private PageMargins(float left, float top, float bottom, float firstPageTopOffset) {
            this.left = left;
            this.top = top;
            this.bottom = bottom;
            this.firstPageTopOffset = firstPageTopOffset;
        }

        private float left() {
            return left;
        }

        private float top() {
            return top;
        }

        private float bottom() {
            return bottom;
        }

        private float firstPageTopOffset() {
            return firstPageTopOffset;
        }
    }

    static final class RunSegment {
        private final String text;
        private final PDFont font;
        private final float fontSize;
        private final float leadingSpacing;

        RunSegment(String text, PDFont font, float fontSize, float leadingSpacing) {
            this.text = text;
            this.font = font;
            this.fontSize = fontSize;
            this.leadingSpacing = leadingSpacing;
        }

        String text() {
            return text;
        }

        PDFont font() {
            return font;
        }

        float fontSize() {
            return fontSize;
        }

        float leadingSpacing() {
            return leadingSpacing;
        }
    }

    static final class ParagraphFonts {
        private final PDFont fallback;
        private final PDFont simSun;
        private final PDFont simHei;
        private final PDFont kai;
        private final PDFont fangSong;
        private final PDFont times;
        private final PDFont arial;
        private final String defaultAsciiFamily;
        private final String defaultEastAsiaFamily;

        ParagraphFonts(
                PDFont fallback,
                PDFont simSun,
                PDFont simHei,
                PDFont kai,
                PDFont fangSong,
                PDFont times,
                PDFont arial,
                String defaultAsciiFamily,
                String defaultEastAsiaFamily) {
            this.fallback = fallback;
            this.simSun = simSun;
            this.simHei = simHei;
            this.kai = kai;
            this.fangSong = fangSong;
            this.times = times;
            this.arial = arial;
            this.defaultAsciiFamily = defaultAsciiFamily;
            this.defaultEastAsiaFamily = defaultEastAsiaFamily;
        }

        PDFont fallback() {
            return fallback;
        }

        PDFont simSun() {
            return simSun;
        }

        private PDFont resolve(XWPFRun run, int codePoint) {
            boolean eastAsian = usesEastAsianFontSlot(codePoint);
            XWPFRun.FontCharRange range = eastAsian
                    ? XWPFRun.FontCharRange.eastAsia
                    : XWPFRun.FontCharRange.ascii;
            String family = run.getFontFamily(range);
            if (family == null || family.trim().isEmpty()) {
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

    static PDFont wrappingFont(ParagraphFonts fonts, String text) {
        return text.codePoints().anyMatch(PoiDocxRenderer::usesEastAsianFontSlot)
                ? fonts.simSun()
                : fonts.fallback();
    }

    static List<RunSegment> paragraphSegments(
            XWPFParagraph paragraph,
            ParagraphFonts fonts,
            float fallbackFontSize) {
        List<RunSegment> segments = new ArrayList<>();
        AutoSpacing autoSpacing = paragraphAutoSpacing(paragraph);
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
                    float spacing = isEastAsianLatinBoundary(previous, codePoint, autoSpacing)
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
            if (segmentText.length() > 0) {
                segments.add(new RunSegment(
                        segmentText.toString(), segmentFont, fontSize, segmentLeadingSpacing));
            }
        }
        return segments;
    }

    static PDFont resolvedCellFont(XWPFTableCell cell, ParagraphFonts fonts, PDFont boldFont) {
        PDFont resolved = null;
        for (XWPFParagraph paragraph : cell.getParagraphs()) {
            for (XWPFRun run : paragraph.getRuns()) {
                String text = renderableText(run.text());
                for (int offset = 0; offset < text.length();) {
                    int codePoint = text.codePointAt(offset);
                    offset += Character.charCount(codePoint);
                    PDFont candidate = run.isBold() ? boldFont : fonts.resolve(run, codePoint);
                    if (resolved != null && resolved != candidate) {
                        return isCellBold(cell) ? boldFont : fonts.simSun();
                    }
                    resolved = candidate;
                }
            }
        }
        return resolved == null
                ? isCellBold(cell) ? boldFont : fonts.simSun()
                : resolved;
    }

    private static AutoSpacing paragraphAutoSpacing(XWPFParagraph paragraph) {
        if (!paragraph.getCTP().isSetPPr()) {
            return DEFAULT_AUTO_SPACING;
        }
        CTPPr properties = paragraph.getCTP().getPPr();
        boolean latin = !properties.isSetAutoSpaceDE()
                || onOffValue(properties.getAutoSpaceDE().getVal());
        boolean digit = !properties.isSetAutoSpaceDN()
                || onOffValue(properties.getAutoSpaceDN().getVal());
        return new AutoSpacing(latin, digit);
    }

    private static boolean onOffValue(Object value) {
        return value == null
                || !("0".equals(value.toString()) || "false".equalsIgnoreCase(value.toString())
                || "off".equalsIgnoreCase(value.toString()));
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
        CTStyles styles = document.getStyles().getCtStyles();
        if (!styles.isSetDocDefaults()
                || !styles.getDocDefaults().isSetRPrDefault()
                || !styles.getDocDefaults().getRPrDefault().isSetRPr()) {
            return null;
        }
        CTRPr properties = styles.getDocDefaults().getRPrDefault().getRPr();
        if (properties.sizeOfRFontsArray() == 0) {
            return null;
        }
        CTFonts fonts = properties.getRFontsArray(0);
        switch (range) {
            case ascii:
                return fonts.isSetAscii() ? fonts.getAscii() : fonts.getHAnsi();
            case eastAsia:
                return fonts.isSetEastAsia() ? fonts.getEastAsia() : null;
            default:
                return null;
        }
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

    private static PageMargins pageMargins(XWPFDocument document, float fallback, boolean landscape) {
        CTBody body = document.getDocument().getBody();
        if (!body.isSetSectPr() || !body.getSectPr().isSetPgMar()) {
            return new PageMargins(fallback, fallback, fallback, 10.0f);
        }
        CTPageMar margins = body.getSectPr().getPgMar();
        float left = twipsValue(margins.getLeft(), fallback);
        if (!landscape) {
            return new PageMargins(left, left, left, 10.0f);
        }
        float top = twipsValue(margins.getTop(), left);
        float bottom = twipsValue(margins.getBottom(), left);
        return new PageMargins(left, top, bottom, 0.0f);
    }

    private static float twipsValue(Object value, float fallback) {
        if (value == null) {
            return fallback;
        }
        try {
            return Float.parseFloat(value.toString()) / 20.0f;
        } catch (NumberFormatException exception) {
            return fallback;
        }
    }

    private static float documentLinePitch(XWPFDocument document) {
        CTBody body = document.getDocument().getBody();
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

    private static PageNumberFooter pageNumberFooter(XWPFDocument document) {
        if (document.getFooterList().isEmpty()) {
            return null;
        }
        PageNumberFooter result = null;
        for (XWPFFooter footer : document.getFooterList()) {
            boolean hasPageField = footer.getParagraphs().stream()
                    .flatMap(paragraph -> paragraph.getRuns().stream())
                    .flatMap(run -> run.getCTR().getInstrTextList().stream())
                    .map(instruction -> instruction.getStringValue().trim())
                    .anyMatch(instruction -> PAGE_FIELD.matcher(instruction).find());
            if (!hasPageField) {
                return null;
            }
            Matcher cachedPage = CACHED_PAGE_NUMBER.matcher(footer.getText().trim());
            if (!cachedPage.matches()) {
                continue;
            }
            PageNumberFooter candidate = new PageNumberFooter(cachedPage.group(1), cachedPage.group(3));
            if (result != null && !result.equals(candidate)) {
                return null;
            }
            result = candidate;
        }
        return result == null ? new PageNumberFooter("", "") : result;
    }

    private static float pageFooterDistance(XWPFDocument document, float fallback) {
        CTBody body = document.getDocument().getBody();
        if (!body.isSetSectPr() || !body.getSectPr().isSetPgMar()
                || body.getSectPr().getPgMar().getFooter() == null) {
            return fallback;
        }
        try {
            return Float.parseFloat(body.getSectPr().getPgMar().getFooter().toString()) / 20.0f;
        } catch (NumberFormatException exception) {
            return fallback;
        }
    }

    private static float textWidth(PDFont font, String text, float fontSize) throws IOException {
        return textWidth(font, text, fontSize, DEFAULT_AUTO_SPACING);
    }

    private static float textWidth(
            PDFont font,
            String text,
            float fontSize,
            AutoSpacing autoSpacing) throws IOException {
        return font.getStringWidth(text) / 1000.0f * fontSize
                + eastAsianBoundaryCount(text, autoSpacing) * fontSize * 0.25f;
    }

    static int eastAsianBoundaryCount(String text) {
        return eastAsianBoundaryCount(text, DEFAULT_AUTO_SPACING);
    }

    private static int eastAsianBoundaryCount(String text, AutoSpacing autoSpacing) {
        int boundaries = 0;
        int previous = -1;
        for (int offset = 0; offset < text.length();) {
            int codePoint = text.codePointAt(offset);
            offset += Character.charCount(codePoint);
            if (previous >= 0 && isEastAsianLatinBoundary(previous, codePoint, autoSpacing)) {
                boundaries++;
            }
            previous = codePoint;
        }
        return boundaries;
    }

    private static boolean isEastAsianLatinBoundary(int left, int right, AutoSpacing autoSpacing) {
        if (isEastAsian(left)) {
            return isSpacedLatinOrDigit(right, autoSpacing);
        }
        return isSpacedLatinOrDigit(left, autoSpacing) && isEastAsian(right);
    }

    private static boolean isSpacedLatinOrDigit(int codePoint, AutoSpacing autoSpacing) {
        return Character.isDigit(codePoint)
                ? autoSpacing.digit()
                : autoSpacing.latin()
                        && Character.UnicodeScript.of(codePoint) == Character.UnicodeScript.LATIN;
    }

    private static boolean isEastAsian(int codePoint) {
        Character.UnicodeScript script = Character.UnicodeScript.of(codePoint);
        return script == Character.UnicodeScript.HAN
                || script == Character.UnicodeScript.HIRAGANA
                || script == Character.UnicodeScript.KATAKANA
                || script == Character.UnicodeScript.HANGUL;
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
        showText(content, font, fontSize, text, x, y, DEFAULT_AUTO_SPACING);
    }

    private static void showText(
            PDPageContentStream content,
            PDFont font,
            float fontSize,
            String text,
            float x,
            float y,
            AutoSpacing autoSpacing) throws IOException {
        if (text.isEmpty()) {
            return;
        }
        content.beginText();
        content.setFont(font, fontSize);
        content.newLineAtOffset(x, y);
        if (eastAsianBoundaryCount(text, autoSpacing) == 0) {
            content.showText(text);
        } else {
            List<Object> positioned = new ArrayList<>();
            int start = 0;
            int previous = -1;
            for (int offset = 0; offset < text.length();) {
                int codePoint = text.codePointAt(offset);
                if (previous >= 0 && isEastAsianLatinBoundary(previous, codePoint, autoSpacing)) {
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
        private final float topMargin;
        private final float bottomMargin;
        private final float linePitch;
        private final PDFont pageNumberFont;
        private final PageNumberFooter pageNumberFooter;
        private final float pageFooterDistance;
        private PDPageContentStream content;
        private float y;
        private int pageCount;

        private PageContext(
                PDDocument document,
                PageSize pageSize,
                float margin,
                float topMargin,
                float bottomMargin,
                float linePitch,
                float topOffset,
                PDFont pageNumberFont,
                PageNumberFooter pageNumberFooter,
                float pageFooterDistance)
                throws IOException {
            this.document = document;
            this.pageSize = pageSize;
            this.margin = margin;
            this.topMargin = topMargin;
            this.bottomMargin = bottomMargin;
            this.linePitch = linePitch;
            this.pageNumberFont = pageNumberFont;
            this.pageNumberFooter = pageNumberFooter;
            this.pageFooterDistance = pageFooterDistance;
            newPage(topOffset);
        }

        private void ensureSpace(float height) throws IOException {
            if (y - height < bottomMargin) {
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
                renderPageNumber();
                content.close();
            }
            PDPage page = new PDPage(new PDRectangle(pageSize.width(), pageSize.height()));
            document.addPage(page);
            pageCount++;
            content = new PDPageContentStream(document, page);
            y = pageSize.height() - topMargin - topOffset;
        }

        private void renderPageNumber() throws IOException {
            if (pageNumberFont == null) {
                return;
            }
            String pageNumber = pageNumberFooter.prefix() + pageCount + pageNumberFooter.suffix();
            float fontSize = 9.0f;
            float x = (pageSize.width() - textWidth(pageNumberFont, pageNumber, fontSize)) / 2.0f;
            showText(content, pageNumberFont, fontSize, pageNumber, x, pageFooterDistance);
        }

        @Override
        public void close() throws IOException {
            if (content != null) {
                renderPageNumber();
                content.close();
                content = null;
            }
        }
    }
}