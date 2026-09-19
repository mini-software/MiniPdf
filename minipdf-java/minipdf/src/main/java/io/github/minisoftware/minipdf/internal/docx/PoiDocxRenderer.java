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
import org.apache.pdfbox.pdmodel.font.PDType0Font;
import org.apache.pdfbox.pdmodel.font.PDType1Font;
import org.apache.pdfbox.pdmodel.font.Standard14Fonts;
import org.apache.pdfbox.pdmodel.graphics.image.PDImageXObject;
import org.apache.poi.xwpf.usermodel.IBodyElement;
import org.apache.poi.xwpf.usermodel.LineSpacingRule;
import org.apache.poi.xwpf.usermodel.ParagraphAlignment;
import org.apache.poi.xwpf.usermodel.XWPFDocument;
import org.apache.poi.xwpf.usermodel.XWPFFooter;
import org.apache.poi.xwpf.usermodel.XWPFFootnote;
import org.apache.poi.xwpf.usermodel.XWPFNumbering;
import org.apache.poi.xwpf.usermodel.XWPFAbstractNum;
import org.apache.poi.xwpf.usermodel.XWPFParagraph;
import org.apache.poi.xwpf.usermodel.XWPFPicture;
import org.apache.poi.xwpf.usermodel.XWPFRun;
import org.apache.poi.xwpf.usermodel.XWPFStyle;
import org.apache.poi.xwpf.usermodel.XWPFStyles;
import org.apache.poi.xwpf.usermodel.XWPFTable;
import org.apache.poi.xwpf.usermodel.XWPFTableCell;
import org.apache.poi.xwpf.usermodel.XWPFTableRow;
import org.apache.poi.openxml4j.opc.PackagePart;
import org.apache.xmlbeans.XmlCursor;
import org.apache.xmlbeans.XmlObject;
import org.openxmlformats.schemas.wordprocessingml.x2006.main.CTBody;
import org.openxmlformats.schemas.wordprocessingml.x2006.main.CTBorder;
import org.openxmlformats.schemas.wordprocessingml.x2006.main.CTOnOff;
import org.openxmlformats.schemas.wordprocessingml.x2006.main.CTParaRPr;
import org.openxmlformats.schemas.wordprocessingml.x2006.main.CTFonts;
import org.openxmlformats.schemas.wordprocessingml.x2006.main.CTPageMar;
import org.openxmlformats.schemas.wordprocessingml.x2006.main.CTPPr;
import org.openxmlformats.schemas.wordprocessingml.x2006.main.CTRPr;
import org.openxmlformats.schemas.wordprocessingml.x2006.main.CTSectPr;
import org.openxmlformats.schemas.wordprocessingml.x2006.main.CTStyles;
import org.openxmlformats.schemas.wordprocessingml.x2006.main.CTSpacing;
import org.openxmlformats.schemas.wordprocessingml.x2006.main.CTSym;
import org.openxmlformats.schemas.wordprocessingml.x2006.main.CTP;
import org.openxmlformats.schemas.wordprocessingml.x2006.main.STLineSpacingRule;
import org.openxmlformats.schemas.wordprocessingml.x2006.main.STDocGrid;
import org.openxmlformats.schemas.wordprocessingml.x2006.main.CTStyle;
import org.openxmlformats.schemas.wordprocessingml.x2006.main.STBorder;
import org.openxmlformats.schemas.wordprocessingml.x2006.main.CTTblBorders;
import org.openxmlformats.schemas.wordprocessingml.x2006.main.CTTblGridCol;
import org.openxmlformats.schemas.wordprocessingml.x2006.main.CTTcBorders;
import org.openxmlformats.schemas.wordprocessingml.x2006.main.CTAbstractNum;
import org.openxmlformats.schemas.wordprocessingml.x2006.main.CTLvl;
import org.w3c.dom.Document;
import org.w3c.dom.NamedNodeMap;
import org.w3c.dom.Node;
import org.w3c.dom.NodeList;

import javax.xml.parsers.DocumentBuilderFactory;
import java.io.ByteArrayInputStream;
import java.io.ByteArrayOutputStream;
import java.io.IOException;
import java.io.InputStream;
import java.math.BigInteger;
import java.text.Normalizer;
import java.util.ArrayList;
import java.util.Collections;
import java.util.Comparator;
import java.util.HashMap;
import java.util.HashSet;
import java.util.LinkedHashMap;
import java.util.List;
import java.util.Map;
import java.util.Set;
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
    // Per-render flags mirroring DocxToPdfConverter state: whether the
    // document's default Latin font (Normal style) is Calibri-like, and
    // whether it is a serif family.
    private static final ThreadLocal<Boolean> CALIBRI_DEFAULT = new ThreadLocal<>();
    private static final ThreadLocal<Boolean> SERIF_DEFAULT = new ThreadLocal<>();

    // Times-Roman (Times New Roman Regular) character widths for ASCII 32..126,
    // sourced from Adobe Type 1 Times-Roman AFM (matches Times New Roman TTF
    // advances within ~1 unit). Mirrors DocxToPdfConverter.TimesRomanWidths.
    private static final int[] TIMES_ROMAN_WIDTHS = {
        250, 333, 408, 500, 500, 833, 778, 180, 333, 333, // ' ' to )
        500, 564, 250, 333, 250, 278,                     // * to /
        500, 500, 500, 500, 500, 500, 500, 500, 500, 500, // 0-9
        278, 278, 564, 564, 564, 444, 921,                // : to @
        722, 667, 667, 722, 611, 556, 722, 722, 333,      // A-I
        389, 722, 611, 889, 722, 722, 556, 722, 667, 556, // J-S
        611, 722, 722, 944, 722, 722, 611,                // T-Z
        333, 278, 333, 469, 500, 333,                     // [ to `
        444, 500, 444, 500, 444, 333, 500, 500, 278,      // a-i
        278, 500, 278, 778, 500, 500, 500, 500, 333, 389, // j-s
        278, 500, 500, 722, 500, 500, 444,                // t-z
        480, 200, 480, 541,                               // { to ~
    };

    // Helvetica (Arial) character widths for ASCII 32..126 (per 1000 em).
    // Mirrors DocxToPdfConverter.HelveticaWidths: the raw-width path for
    // documents whose default font is not Calibri.
    private static final int[] HELVETICA_WIDTHS = {
        278, 278, 355, 556, 556, 889, 667, 191, 333, 333, // ' ' to )
        389, 584, 278, 333, 278, 278,                     // * to /
        556, 556, 556, 556, 556, 556, 556, 556, 556, 556, // 0-9
        278, 278, 584, 584, 584, 556, 1015,               // : to @
        667, 667, 722, 722, 667, 611, 778, 722, 278,      // A-I
        500, 667, 556, 833, 722, 778, 667, 778, 722, 667, // J-S
        611, 722, 667, 944, 667, 667, 611,                // T-Z
        278, 278, 278, 469, 556, 333,                     // [ to `
        556, 556, 500, 556, 556, 278, 556, 556, 222,      // a-i
        222, 500, 222, 833, 556, 556, 556, 556, 333, 500, // j-s
        278, 556, 500, 722, 500, 500, 500,                // t-z
        334, 260, 334, 584,                               // { to ~
    };

    // Calibri Regular character widths for ASCII 32..126 (UPM=2048 scaled to
    // 1000). Mirrors DocxToPdfConverter.CalibrWidths.
    private static final int[] CALIBRI_WIDTHS = {
        226, 326, 401, 498, 507, 715, 682, 221, 303, 303, // ' ' to )
        498, 498, 250, 306, 252, 386,                     // * to /
        507, 507, 507, 507, 507, 507, 507, 507, 507, 507, // 0-9
        268, 268, 498, 498, 498, 463, 894,                // : to @
        579, 544, 533, 615, 488, 459, 631, 623, 252,      // A-I
        319, 520, 420, 855, 646, 662, 517, 673, 543, 459, // J-S
        487, 642, 567, 890, 519, 487, 468,                // T-Z
        307, 386, 307, 498, 498, 291,                     // [ to `
        479, 525, 423, 525, 498, 305, 471, 525, 229,      // a-i
        239, 455, 229, 799, 525, 527, 525, 525, 349, 391, // j-s
        335, 525, 452, 715, 433, 453, 395,                // t-z
        314, 460, 314, 498,                               // { to ~
    };

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
                        : paragraphText((XWPFParagraph) element))
                    .map(value -> value.replace('\n', ' ').replace('\t', ' '))
                    .collect(Collectors.toList()));
            // Mirror DocxReader.cs: choose the layout path from the document text,
            // not from which font happens to load. CJK fonts (SimSun etc.) render
            // Latin glyphs too, so a font-based check would misroute Latin documents
            // into the CJK grid/margin path.
            boolean requiresCjkFont = text.stream()
                    .flatMap(List::stream)
                    .flatMapToInt(String::codePoints)
                    .anyMatch(codePoint -> codePoint > 255);
            String defaultAsciiFamily = defaultFontFamily(source, XWPFRun.FontCharRange.ascii);
            String defaultFamily = defaultAsciiFamily == null ? "" : defaultAsciiFamily.toLowerCase();
            CALIBRI_DEFAULT.set(defaultFamily.contains("calibri"));
            SERIF_DEFAULT.set(serifFamily(defaultFamily));
            PDFont font = requiresCjkFont ? SimplePdfTextRenderer.loadFont(output, text) : null;
            boolean usesDocumentLatinFont = false;
            if (font == null) {
                font = loadDocumentLatinFont(output, text, defaultAsciiFamily);
                usesDocumentLatinFont = font != null;
            }
            if (font == null && text.stream().flatMap(List::stream)
                    .flatMapToInt(String::codePoints).allMatch(codePoint -> codePoint <= 255)) {
                font = new PDType1Font(Standard14Fonts.FontName.HELVETICA);
            }
            if (font == null) {
                return null;
            }
            // Load a bold face matching the document script: CJK documents use
            // SimSun/Microsoft YaHei Bold, Latin documents use Times/Arial Bold.
            // Loading SimSun Bold first misroutes Latin bold runs (Times) into a
            // CJK face, whose half-width Latin glyphs distort wrap widths.
            PDFont boldFont = requiresCjkFont
                    ? SimplePdfTextRenderer.loadSystemFont(
                            output, text, "simsunb.ttf", "msyhbd.ttc", "NotoSansCJK-Bold.ttc")
                    : SimplePdfTextRenderer.loadSystemFont(
                            output, text, "timesbd.ttf", "arialbd.ttf", "simsunb.ttf", "msyhbd.ttc");
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
                    defaultAsciiFamily,
                    defaultFontFamily(source, XWPFRun.FontCharRange.eastAsia));

                boolean landscape = pageSize.width() > pageSize.height();
                PageMargins margins = pageMargins(
                    source, DEFAULT_MARGIN, landscape, usesDocumentLatinFont);
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
                    pageFooterDistance(source, margins.bottom()),
                    footnotes(source, timesFont),
                    themeColors(source));
            // Pre-process contextualSpacing the way Word does: between two
            // consecutive paragraphs of the same style, when either has
            // contextualSpacing, the first paragraph's spacing-after collapses.
            Set<XWPFParagraph> suppressContextualAfter = new HashSet<>();
            List<IBodyElement> bodyElements = source.getBodyElements();
            for (int index = 0; index < bodyElements.size() - 1; index++) {
                if (!(bodyElements.get(index) instanceof XWPFParagraph)
                        || !(bodyElements.get(index + 1) instanceof XWPFParagraph)) {
                    continue;
                }
                XWPFParagraph current = (XWPFParagraph) bodyElements.get(index);
                XWPFParagraph next = (XWPFParagraph) bodyElements.get(index + 1);
                String currentStyle = current.getStyle();
                if (currentStyle != null
                        && currentStyle.equals(next.getStyle())
                        && (paragraphContextualSpacing(current) || paragraphContextualSpacing(next))
                        && effectiveSpacing(current).after() > 0.0) {
                    suppressContextualAfter.add(current);
                }
            }
            for (int index = 0; index < bodyElements.size(); index++) {
                IBodyElement element = bodyElements.get(index);
                if (element instanceof XWPFParagraph) {
                    XWPFParagraph paragraph = (XWPFParagraph) element;
                    IBodyElement next = index + 1 < bodyElements.size()
                            ? bodyElements.get(index + 1)
                            : null;
                    if (next instanceof XWPFParagraph
                            && usesDocumentLatinFont
                            && paragraphKeepNext(paragraph)) {
                        // keepNext: prevent an orphaned heading at the bottom
                        // of a page. Word keeps the heading together with the
                        // following paragraph's first lines (widow/orphan
                        // control), so reserve the heading plus up to three
                        // follow lines.
                        XWPFParagraph follow = (XWPFParagraph) next;
                        float headingFontSize = paragraphFontSize(paragraph, DEFAULT_FONT_SIZE);
                        float headingLineHeight = paragraphLineHeight(
                                paragraph, headingFontSize, headingFontSize * 1.2f, linePitch,
                                usesDocumentLatinFont);
                        float headingSpacingBefore = usesDocumentLatinFont
                                ? twipsToPoints(effectiveSpacing(paragraph).before())
                                : twipsToPoints(paragraph.getSpacingBefore());
                        float followFontSize = paragraphFontSize(follow, DEFAULT_FONT_SIZE);
                        float followLineHeight = paragraphLineHeight(
                                follow, followFontSize, followFontSize * 1.2f, linePitch,
                                usesDocumentLatinFont);
                        String followText = paragraphText(follow);
                        PDFont followFont = wrappingFont(paragraphFonts, followText);
                        int followLines = wrap(
                                followFont,
                                followText.replace('\t', ' '),
                                followFontSize,
                                context.pageSize.width() - context.margin * 2.0f).size();
                        float needed = headingSpacingBefore + headingLineHeight
                                + Math.min(followLines, 3) * followLineHeight;
                        context.keepNextBeforeParagraph(needed);
                    }
                    renderParagraph(
                            context,
                            paragraph,
                            paragraphFonts,
                            usesDocumentLatinFont ? boldFont : null,
                            usesDocumentLatinFont,
                            suppressContextualAfter.contains(paragraph));
                } else if (element instanceof XWPFTable) {
                    XWPFTable table = (XWPFTable) element;
                    renderTable(context, table, paragraphFonts, boldFont, source);
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
        } finally {
            CALIBRI_DEFAULT.remove();
            SERIF_DEFAULT.remove();
        }
    }

    private static void renderParagraph(
            PageContext context,
            XWPFParagraph paragraph,
            ParagraphFonts fonts,
            PDFont boldFont,
            boolean useWordParagraphLayout,
            boolean suppressContextualAfter)
            throws IOException {
        context.registerFootnotes(paragraph);
        boolean alignCheckboxLabels = context.consumeCheckboxLabelAlignment();
        boolean topOfPage = context.consumeTopOfPage();
        // Paragraphs that only host behindDoc anchored shapes are positioned
        // absolutely and consume no flow space (mirrors the .NET
        // isShapeOnlyParagraph behavior — Word keeps such paragraph marks
        // from pushing following content down).
        if (isShapeOnlyParagraph(paragraph)) {
            renderAnchoredBehindDocShapes(context, paragraph);
            return;
        }
        // An empty paragraph that ends a section only carries an invisible
        // paragraph mark: Word places the break after it without pushing
        // content onto a fresh page first, so don't consume a line.
        if (startsNewSection(paragraph)
                && paragraph.getRuns().stream().allMatch(run -> run.text().isEmpty())
                && floatingCheckboxes(paragraph).isEmpty()) {
            context.newPage();
            return;
        }
        float fontSize = paragraphFontSize(paragraph, DEFAULT_FONT_SIZE);
        float spacingBefore = useWordParagraphLayout
                ? twipsToPoints(effectiveSpacing(paragraph).before())
                : twipsToPoints(paragraph.getSpacingBefore());
        if (useWordParagraphLayout) {
            // Word suppresses spacing-before at the top of a page.
            if (!topOfPage) {
                context.moveToParagraph(spacingBefore);
            }
        } else {
            if (!topOfPage) {
                context.moveDown(spacingBefore);
            }
        }
        float paragraphTop = context.y;
        String bullet = paragraphBullet(paragraph);
        String text = alignCheckboxLabels
            ? paragraphText(paragraph)
            : renderableText(paragraphText(paragraph));
        if (bullet != null) {
            text = bullet + " " + text;
        }
        float leftIndent = indentationToPoints(paragraph.getIndentationLeft());
        float rightIndent = indentationToPoints(paragraph.getIndentationRight());
        float firstLineIndent = useWordParagraphLayout
            ? indentationToPoints(paragraph.getIndentationFirstLine())
                - indentationToPoints(paragraph.getIndentationHanging())
            : 0.0f;
        float availableWidth = context.pageSize.width() - context.margin * 2.0f - leftIndent - rightIndent;
        float firstLineWidth = availableWidth - firstLineIndent;
        AutoSpacing autoSpacing = paragraphAutoSpacing(paragraph);
        List<RunSegment> segments = paragraphSegments(
            paragraph, fonts, fontSize, boldFont, alignCheckboxLabels);
        if (bullet != null && !segments.isEmpty()) {
            for (int index = 0; index < segments.size(); index++) {
                RunSegment first = segments.get(index);
                if (!first.tab()) {
                    segments.set(index, first.prepend(bullet + " "));
                    break;
                }
            }
        }
        float segmentWidth = 0.0f;
        for (RunSegment segment : segments) {
            if (segment.tab()) {
                segmentWidth = nextDefaultTabStop(segmentWidth);
                continue;
            }
            segmentWidth += segment.leadingSpacing()
                + textWidth(segment.font(), segment.text(), segment.fontSize(), autoSpacing, segment.bold());
        }
        // Mirror DocxToPdfConverter: center/right alignment positions text with
        // the actual font advances (measured width), not the wrap estimate.
        float actualSegmentWidth = 0.0f;
        for (RunSegment segment : segments) {
            if (segment.tab()) {
                actualSegmentWidth = nextDefaultTabStop(actualSegmentWidth);
                continue;
            }
            actualSegmentWidth += segment.leadingSpacing()
                + segment.font().getStringWidth(segment.text()) / 1000.0f * segment.fontSize();
        }
        boolean hasPictures = paragraph.getRuns().stream()
                .anyMatch(run -> !run.getEmbeddedPictures().isEmpty());
        if (!segments.isEmpty()
                && !hasPictures
                && segments.stream().map(RunSegment::text).reduce("", String::concat).equals(text)
                && segmentWidth <= firstLineWidth) {
            float maxFontSize = segments.stream()
                    .map(RunSegment::fontSize)
                    .max(Float::compare)
                    .orElse(fontSize);
            float naturalLineHeight = maxFontSize * 1.2f;
                float lineHeight = paragraphLineHeight(
                    paragraph, maxFontSize, naturalLineHeight, context.linePitch,
                    useWordParagraphLayout, bullet != null);
            context.ensureSpace(lineHeight);
            float leading = Math.max(0.0f, lineHeight - naturalLineHeight) / 2.0f;
            float firstBaselineOffset = maxFontSize;
            if (topOfPage) {
                RunSegment tallest = segments.stream()
                        .max(Comparator.comparing(RunSegment::fontSize))
                        .orElse(null);
                if (tallest != null) {
                    firstBaselineOffset = maxFontSize * fontAscentRatio(tallest.font());
                }
            }
            float baseline = context.y - leading - firstBaselineOffset;
            float x = context.margin + leftIndent + firstLineIndent;
            if (paragraph.getAlignment() == ParagraphAlignment.CENTER) {
                x = centeredTextX(
                        context.pageSize.width(), leftIndent + firstLineIndent, rightIndent, actualSegmentWidth);
            } else if (paragraph.getAlignment() == ParagraphAlignment.RIGHT) {
                x = context.pageSize.width() - context.margin - rightIndent - actualSegmentWidth;
            }
            for (RunSegment segment : segments) {
                if (segment.tab()) {
                    x = context.margin + leftIndent + firstLineIndent
                            + nextDefaultTabStop(x - context.margin - leftIndent - firstLineIndent);
                    continue;
                }
                x += segment.leadingSpacing();
                drawRunHighlight(
                    context.content,
                    segment.highlight(),
                    x,
                    baseline,
                    textWidth(segment.font(), segment.text(), segment.fontSize(), autoSpacing),
                    segment.fontSize());
                applyTextColor(context.content, segment.color());
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
            context.content.setNonStrokingColor(0.0f, 0.0f, 0.0f);
            renderFloatingCheckboxes(context, paragraph, paragraphTop);
            context.y -= lineHeight;
            finishParagraph(context, paragraph, useWordParagraphLayout, suppressContextualAfter);
            if (startsNewSection(paragraph)) {
                context.newPage();
            }
            return;
        }
        PDFont font = wrappingFont(fonts, text);
        // Mirrors DocxReader: a wrapped paragraph renders with the first
        // explicit run color, falling back to the paragraph style color
        // (e.g. the Heading2 4F81BD blue).
        String paragraphColor = paragraphStyleColor(paragraph);
        if (paragraphColor == null) {
            for (XWPFRun run : paragraph.getRuns()) {
                paragraphColor = runColor(run);
                if (paragraphColor != null) {
                    break;
                }
            }
        }
        // Calibrated wrap widths already match Word/LibreOffice metrics, so no
        // extra per-line tolerance is needed. The old fontSize tolerance let
        // bold headings like "A Novel Approach to Document Conversion" fit on a
        // single line and collapse pagination.
        float wrapTolerance = 0.0f;
        boolean paragraphBold = paragraph.getRuns().stream()
                .anyMatch(run -> !run.text().isEmpty() && run.isBold());
        List<String> lines = wrap(
            font,
            text.replace('\t', ' '),
            fontSize,
            firstLineWidth + wrapTolerance,
            availableWidth + wrapTolerance,
            autoSpacing,
            paragraphBold);
        float lineHeight = paragraphLineHeight(
            paragraph, fontSize, fontSize * 1.2f, context.linePitch, useWordParagraphLayout,
            bullet != null);
        if (useWordParagraphLayout) {
            context.avoidWidowOrphanSplit(lineHeight, lines.size());
        }
        float firstBaseline = 0.0f;
        applyTextColor(context.content, paragraphColor);
        for (int index = 0; index < lines.size(); index++) {
            context.ensureSpace(lineHeight);
            if (index == 0) {
                // behindDoc anchored drawing shapes (e.g. a full-page sidebar or
                // decorative freeforms) render at absolute page positions on the
                // anchor paragraph's page, before the following content.
                renderAnchoredBehindDocShapes(context, paragraph);
            }
            float baseline = context.y - (topOfPage && index == 0
                    ? fontSize * fontAscentRatio(font)
                    : fontSize);
            if (index == 0) {
                firstBaseline = baseline;
            }
            String line = lines.get(index);
            float estimatedWidth = textWidth(font, line, fontSize, autoSpacing);
            float width = paragraph.getAlignment() == ParagraphAlignment.CENTER
                    || paragraph.getAlignment() == ParagraphAlignment.RIGHT
                    ? font.getStringWidth(line) / 1000.0f * fontSize
                    : estimatedWidth;
            float lineLeftIndent = index == 0 ? leftIndent + firstLineIndent : leftIndent;
            float lineWidth = index == 0 ? firstLineWidth : availableWidth;
            float x = context.margin + lineLeftIndent;
            if (paragraph.getAlignment() == ParagraphAlignment.CENTER) {
                x = centeredTextX(context.pageSize.width(), lineLeftIndent, rightIndent, width);
            } else if (paragraph.getAlignment() == ParagraphAlignment.RIGHT) {
                x = context.pageSize.width() - context.margin - rightIndent - width;
            }
            float wordSpacing = paragraph.getAlignment() == ParagraphAlignment.BOTH
                    && useWordParagraphLayout
                    && index < lines.size() - 1
                    ? justifiedWordSpacing(line, estimatedWidth, lineWidth)
                    : 0.0f;
            showText(context.content, font, fontSize, line, x, baseline, autoSpacing, wordSpacing);
            context.y -= lineHeight;
        }
        context.content.setNonStrokingColor(0.0f, 0.0f, 0.0f);
        renderPictures(context, paragraph, firstBaseline);
        renderFloatingCheckboxes(context, paragraph, paragraphTop);
        finishParagraph(context, paragraph, useWordParagraphLayout, suppressContextualAfter);
        if (startsNewSection(paragraph)) {
            context.newPage();
        }
    }

    private static void finishParagraph(
            PageContext context,
            XWPFParagraph paragraph,
            boolean useWordParagraphLayout,
            boolean suppressContextualAfter) throws IOException {
        float spacingAfter = suppressContextualAfter
                ? 0.0f
                : useWordParagraphLayout
                    ? twipsToPoints(effectiveSpacing(paragraph).after())
                    : twipsToPoints(paragraph.getSpacingAfter());
        if (useWordParagraphLayout) {
            context.finishParagraph(spacingAfter);
        } else {
            context.moveDown(spacingAfter);
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
                // XWPFPicture#getWidth/getDepth already return points.
                float width = (float) picture.getWidth();
                float height = (float) picture.getDepth();
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

    private static void renderFloatingCheckboxes(
            PageContext context,
            XWPFParagraph paragraph,
            float paragraphTop) throws IOException {
        List<FloatingCheckbox> checkboxes = floatingCheckboxes(paragraph);
        for (FloatingCheckbox checkbox : checkboxes) {
            float x = "column".equals(checkbox.horizontalRelative)
                    ? context.margin + checkbox.offsetX
                    : checkbox.offsetX;
                float top = paragraphTop - checkbox.offsetY;
                float bottom = top - checkbox.height;
            float right = x + checkbox.width;
            context.content.setStrokingColor(0, 0, 0);
            context.content.setLineWidth(checkbox.lineWidth);
            context.content.addRect(x, bottom, checkbox.width, checkbox.height);
            context.content.stroke();
            if (checkbox.checked) {
                context.content.setLineWidth(Math.max(0.8f, checkbox.lineWidth * 1.6f));
                context.content.moveTo(x + checkbox.width * 0.20f, bottom + checkbox.height * 0.52f);
                context.content.lineTo(x + checkbox.width * 0.40f, bottom + checkbox.height * 0.25f);
                context.content.lineTo(right - checkbox.width * 0.16f, top - checkbox.height * 0.18f);
                context.content.stroke();
            }
        }
        if (!checkboxes.isEmpty()) {
            context.expectCheckboxLabelAlignment();
        }
    }

    static List<FloatingCheckbox> floatingCheckboxes(XWPFParagraph paragraph) {
        List<FloatingCheckbox> checkboxes = new ArrayList<>();
        collectFloatingCheckboxes(paragraph.getCTP().getDomNode(), checkboxes);
        return checkboxes;
    }

    private static void collectFloatingCheckboxes(Node node, List<FloatingCheckbox> checkboxes) {
        if ("anchor".equals(node.getLocalName()) && firstDescendant(node, "txbxContent") != null) {
            Node positionH = directChild(node, "positionH");
            Node positionV = directChild(node, "positionV");
            Node extent = directChild(node, "extent");
            if (positionH != null && extent != null) {
                Node offset = directChild(positionH, "posOffset");
                float offsetX = emuToPoints(nodeText(offset));
                Node verticalOffset = directChild(positionV, "posOffset");
                float offsetY = emuToPoints(nodeText(verticalOffset));
                float width = emuToPoints(attribute(extent, "cx"));
                float height = emuToPoints(attribute(extent, "cy"));
                Node line = firstDescendant(node, "ln");
                float lineWidth = Math.max(0.5f, emuToPoints(attribute(line, "w")));
                if (width > 0.0f && height > 0.0f) {
                    checkboxes.add(new FloatingCheckbox(
                            offsetX,
                            offsetY,
                            width,
                            height,
                            lineWidth,
                            attribute(positionH, "relativeFrom"),
                            containsWingdingsCheck(firstDescendant(node, "txbxContent"))));
                }
            }
            return;
        }
        NodeList children = node.getChildNodes();
        for (int index = 0; index < children.getLength(); index++) {
            collectFloatingCheckboxes(children.item(index), checkboxes);
        }
    }

    private static boolean containsWingdingsCheck(Node node) {
        if (node == null) {
            return false;
        }
        if ("sym".equals(node.getLocalName())
                && "Wingdings".equalsIgnoreCase(attribute(node, "font"))
                && "F0FC".equalsIgnoreCase(attribute(node, "char"))) {
            return true;
        }
        NodeList children = node.getChildNodes();
        for (int index = 0; index < children.getLength(); index++) {
            if (containsWingdingsCheck(children.item(index))) {
                return true;
            }
        }
        return false;
    }

    private static Node directChild(Node node, String localName) {
        if (node == null) {
            return null;
        }
        NodeList children = node.getChildNodes();
        for (int index = 0; index < children.getLength(); index++) {
            Node child = children.item(index);
            if (localName.equals(child.getLocalName())) {
                return child;
            }
        }
        return null;
    }

    private static Node firstDescendant(Node node, String localName) {
        if (node == null) {
            return null;
        }
        NodeList children = node.getChildNodes();
        for (int index = 0; index < children.getLength(); index++) {
            Node child = children.item(index);
            if (localName.equals(child.getLocalName())) {
                return child;
            }
            Node descendant = firstDescendant(child, localName);
            if (descendant != null) {
                return descendant;
            }
        }
        return null;
    }

    private static String nodeText(Node node) {
        if (node == null) {
            return null;
        }
        StringBuilder text = new StringBuilder();
        NodeList children = node.getChildNodes();
        for (int index = 0; index < children.getLength(); index++) {
            Node child = children.item(index);
            if (child.getNodeType() == Node.TEXT_NODE || child.getNodeType() == Node.CDATA_SECTION_NODE) {
                text.append(child.getNodeValue());
            } else {
                String childText = nodeText(child);
                if (childText != null) {
                    text.append(childText);
                }
            }
        }
        return text.toString();
    }

    private static String attribute(Node node, String localName) {
        if (node == null || node.getAttributes() == null) {
            return null;
        }
        NamedNodeMap attributes = node.getAttributes();
        for (int index = 0; index < attributes.getLength(); index++) {
            Node attribute = attributes.item(index);
            if (localName.equals(attribute.getLocalName()) || localName.equals(attribute.getNodeName())) {
                return attribute.getNodeValue();
            }
        }
        return null;
    }

    private static float emuToPoints(String value) {
        if (value == null || value.isEmpty()) {
            return 0.0f;
        }
        try {
            return Long.parseLong(value) / EMUS_PER_POINT;
        } catch (NumberFormatException ignored) {
            return 0.0f;
        }
    }

    /**
     * Reads the theme color scheme (dk1/lt1/dk2/lt2/accent1..6) from the
     * document's theme part, mirroring DocxReader's themeColors map.
     */
    private static Map<String, String> themeColors(XWPFDocument source) {
        Map<String, String> colors = new LinkedHashMap<>();
        try {
            for (PackagePart part : source.getPackage().getParts()) {
                if (!part.getPartName().getName().contains("/theme/")) {
                    continue;
                }
                try (InputStream stream = part.getInputStream()) {
                    DocumentBuilderFactory factory = DocumentBuilderFactory.newInstance();
                    // localName-based lookups below require namespace processing.
                    factory.setNamespaceAware(true);
                    Document theme = factory.newDocumentBuilder().parse(stream);
                    Node scheme = firstDescendant(theme.getDocumentElement(), "clrScheme");
                    if (scheme == null) {
                        continue;
                    }
                    NodeList entries = scheme.getChildNodes();
                    for (int index = 0; index < entries.getLength(); index++) {
                        Node entry = entries.item(index);
                        if (entry.getNodeType() != Node.ELEMENT_NODE) {
                            continue;
                        }
                        Node srgb = firstDescendant(entry, "srgbClr");
                        Node sys = firstDescendant(entry, "sysClr");
                        String value = attribute(srgb, "val");
                        if (value == null && sys != null) {
                            value = attribute(sys, "lastClr");
                            if (value == null) {
                                value = attribute(sys, "val");
                            }
                        }
                        if (value != null && !value.isEmpty()) {
                            colors.put(entry.getLocalName(), value);
                        }
                    }
                }
                break;
            }
        } catch (Exception ignored) {
            // A missing or malformed theme part must not abort conversion.
        }
        return colors;
    }

    /**
     * True when a paragraph's only visual content is behindDoc anchored
     * drawing shapes (no runs, no pictures).
     */
    private static boolean isShapeOnlyParagraph(XWPFParagraph paragraph) {
        // Runs that only host w:drawing content carry no text and must not
        // make the paragraph consume flow space.
        for (XWPFRun run : paragraph.getRuns()) {
            if (!run.text().isEmpty() || !run.getEmbeddedPictures().isEmpty()) {
                return false;
            }
        }
        List<Node> drawings = new ArrayList<>();
        collectDescendants(paragraph.getCTP().getDomNode(), "drawing", drawings);
        for (Node drawing : drawings) {
            Node anchor = directChild(drawing, "anchor");
            if (anchor != null && "1".equals(attribute(anchor, "behindDoc"))) {
                return true;
            }
        }
        return false;
    }

    /**
     * Renders behindDoc anchored DrawingML group shapes (sidebars, decorative
     * freeforms) at absolute page positions. Mirrors DocxReader.ReadAnchorShapes
     * and DocxToPdfConverter.RenderShape.
     */
    private static void renderAnchoredBehindDocShapes(PageContext context, XWPFParagraph paragraph)
            throws IOException {
        List<Node> drawings = new ArrayList<>();
        collectDescendants(paragraph.getCTP().getDomNode(), "drawing", drawings);
        for (Node drawing : drawings) {
            Node anchor = directChild(drawing, "anchor");
            if (anchor == null || !"1".equals(attribute(anchor, "behindDoc"))) {
                continue;
            }
            Node group = firstDescendant(anchor, "wgp");
            if (group != null) {
                long offsetX = emuValue(directChild(directChild(anchor, "positionH"), "posOffset"));
                long offsetY = emuValue(directChild(directChild(anchor, "positionV"), "posOffset"));
                Node grpSpPr = directChild(group, "grpSpPr");
                Node xfrm = grpSpPr == null ? null : directChild(grpSpPr, "xfrm");
                Node extNode = directChild(xfrm, "ext");
                long extCx = attributeEmu(extNode, "cx");
                long extCy = attributeEmu(extNode, "cy");
                long chOffX = attributeEmu(directChild(xfrm, "chOff"), "x");
                long chOffY = attributeEmu(directChild(xfrm, "chOff"), "y");
                long chExtCx = attributeEmu(directChild(xfrm, "chExt"), "cx");
                long chExtCy = attributeEmu(directChild(xfrm, "chExt"), "cy");
                if (extCx <= 0) {
                    extCx = 1;
                }
                if (extCy <= 0) {
                    extCy = 1;
                }
                if (chExtCx <= 0) {
                    chExtCx = 1;
                }
                if (chExtCy <= 0) {
                    chExtCy = 1;
                }
                float[] groupFill = fillOf(grpSpPr, context.themeColors, null);
                renderGroupChildren(
                        context,
                        group,
                        groupFill,
                        offsetX,
                        offsetY,
                        chOffX,
                        chOffY,
                        extCx,
                        extCy,
                        chExtCx,
                        chExtCy);
            }
        }
    }

    private static void renderGroupChildren(
            PageContext context,
            Node groupElement,
            float[] groupFill,
            long anchorOffsetX,
            long anchorOffsetY,
            long chOffX,
            long chOffY,
            long groupExtCx,
            long groupExtCy,
            long chExtCx,
            long chExtCy) throws IOException {
        NodeList children = groupElement.getChildNodes();
        for (int index = 0; index < children.getLength(); index++) {
            Node child = children.item(index);
            if (child.getNodeType() != Node.ELEMENT_NODE) {
                continue;
            }
            if ("wsp".equals(child.getLocalName())) {
                renderWspShape(
                        context,
                        child,
                        groupFill,
                        anchorOffsetX,
                        anchorOffsetY,
                        chOffX,
                        chOffY,
                        groupExtCx,
                        groupExtCy,
                        chExtCx,
                        chExtCy);
            } else if ("grpSp".equals(child.getLocalName())) {
                Node subSpPr = directChild(child, "grpSpPr");
                Node subXfrm = subSpPr == null ? null : directChild(subSpPr, "xfrm");
                long subOffX = attributeEmu(directChild(subXfrm, "off"), "x");
                long subOffY = attributeEmu(directChild(subXfrm, "off"), "y");
                long subCx = attributeEmu(directChild(subXfrm, "ext"), "cx");
                long subCy = attributeEmu(directChild(subXfrm, "ext"), "cy");
                long subChOffX = attributeEmu(directChild(subXfrm, "chOff"), "x");
                long subChOffY = attributeEmu(directChild(subXfrm, "chOff"), "y");
                long subChCx = attributeEmu(directChild(subXfrm, "chExt"), "cx");
                long subChCy = attributeEmu(directChild(subXfrm, "chExt"), "cy");
                if (subCx <= 0) {
                    subCx = 1;
                }
                if (subCy <= 0) {
                    subCy = 1;
                }
                if (subChCx <= 0) {
                    subChCx = 1;
                }
                if (subChCy <= 0) {
                    subChCy = 1;
                }
                float[] subFill = fillOf(subSpPr, context.themeColors, groupFill);
                long mappedAnchorX = anchorOffsetX + (subOffX - chOffX) * groupExtCx / chExtCx;
                long mappedAnchorY = anchorOffsetY + (subOffY - chOffY) * groupExtCy / chExtCy;
                long mappedExtCx = subCx * groupExtCx / chExtCx;
                long mappedExtCy = subCy * groupExtCy / chExtCy;
                renderGroupChildren(
                        context,
                        child,
                        subFill,
                        mappedAnchorX,
                        mappedAnchorY,
                        subChOffX,
                        subChOffY,
                        mappedExtCx,
                        mappedExtCy,
                        subChCx,
                        subChCy);
            }
        }
    }

    private static void renderWspShape(
            PageContext context,
            Node wsp,
            float[] groupFill,
            long anchorOffsetX,
            long anchorOffsetY,
            long chOffX,
            long chOffY,
            long groupExtCx,
            long groupExtCy,
            long chExtCx,
            long chExtCy) throws IOException {
        Node spPr = directChild(wsp, "spPr");
        if (spPr == null) {
            return;
        }
        float[] fill = fillOf(spPr, context.themeColors, groupFill);
        if (fill == null) {
            return;
        }
        Node xfrm = directChild(spPr, "xfrm");
        if (xfrm == null) {
            return;
        }
        long childOffX = attributeEmu(directChild(xfrm, "off"), "x");
        long childOffY = attributeEmu(directChild(xfrm, "off"), "y");
        long childCx = attributeEmu(directChild(xfrm, "ext"), "cx");
        long childCy = attributeEmu(directChild(xfrm, "ext"), "cy");
        long pageX = anchorOffsetX + (childOffX - chOffX) * groupExtCx / chExtCx;
        long pageY = anchorOffsetY + (childOffY - chOffY) * groupExtCy / chExtCy;
        long pageW = childCx * groupExtCx / chExtCx;
        long pageH = childCy * groupExtCy / chExtCy;
        if (pageW <= 0 || pageH <= 0) {
            return;
        }
        float x = context.margin + pageX / EMUS_PER_POINT;
        float y = context.pageSize.height() - context.topMargin - pageY / EMUS_PER_POINT
                - pageH / EMUS_PER_POINT;
        float width = pageW / EMUS_PER_POINT;
        float height = pageH / EMUS_PER_POINT;
        // Alpha-blend the fill over white, mirroring DocxToPdfConverter.RenderShape.
        float alpha = fill.length > 3 ? fill[3] : 1.0f;
        context.content.setNonStrokingColor(
                1.0f + (fill[0] - 1.0f) * alpha,
                1.0f + (fill[1] - 1.0f) * alpha,
                1.0f + (fill[2] - 1.0f) * alpha);
        List<List<float[]>> subpaths = customGeometryPaths(spPr, childCx, childCy);
        if (subpaths != null && !subpaths.isEmpty()) {
            for (List<float[]> subpath : subpaths) {
                if (subpath.size() < 3) {
                    continue;
                }
                float[] first = subpath.get(0);
                context.content.moveTo(
                        x + first[0] * width, y + height - first[1] * height);
                for (int index = 1; index < subpath.size(); index++) {
                    float[] point = subpath.get(index);
                    context.content.lineTo(
                            x + point[0] * width, y + height - point[1] * height);
                }
                context.content.closePath();
            }
            // Mirror DocxToPdfConverter: custom geometry paths use even-odd fill.
            context.content.fillEvenOdd();
        } else {
            context.content.addRect(x, y, width, height);
            context.content.fill();
        }
        context.content.setNonStrokingColor(0.0f, 0.0f, 0.0f);
    }

    /**
     * Resolves a shape's solidFill to RGB floats + alpha, honoring grpFill
     * inheritance. Returns null when the shape has no fill.
     */
    private static float[] fillOf(Node spPr, Map<String, String> themeColors, float[] groupFill) {
        if (spPr == null || directChild(spPr, "noFill") != null) {
            return null;
        }
        Node solidFill = directChild(spPr, "solidFill");
        if (solidFill != null) {
            return resolveSolidFill(solidFill, themeColors);
        }
        if (directChild(spPr, "grpFill") != null) {
            return groupFill;
        }
        return null;
    }

    private static float[] resolveSolidFill(Node solidFill, Map<String, String> themeColors) {
        Node srgb = directChild(solidFill, "srgbClr");
        String hex = attribute(srgb, "val");
        if (hex == null) {
            Node scheme = directChild(solidFill, "schemeClr");
            hex = themeColors == null ? null : themeColors.get(attribute(scheme, "val"));
        }
        if (hex == null || hex.length() < 6) {
            return null;
        }
        try {
            int rgb = Integer.parseInt(hex.substring(0, 6), 16);
            float[] fill = new float[] {
                    ((rgb >> 16) & 0xff) / 255.0f,
                    ((rgb >> 8) & 0xff) / 255.0f,
                    (rgb & 0xff) / 255.0f,
                    1.0f,
            };
            if (srgb != null) {
                Node alpha = directChild(srgb, "alpha");
                long alphaValue = attributeEmu(alpha, "val");
                if (alphaValue > 0) {
                    fill[3] = Math.min(1.0f, alphaValue / 100000.0f);
                }
            }
            return fill;
        } catch (NumberFormatException ignored) {
            return null;
        }
    }

    /**
     * Parses custGeom freeform paths into normalized point subpaths, mirroring
     * DocxReader.ParseCustomGeometryPaths.
     */
    private static List<List<float[]>> customGeometryPaths(Node spPr, long widthEmu, long heightEmu) {
        Node custGeom = firstDescendant(spPr, "custGeom");
        if (custGeom == null) {
            return null;
        }
        Node pathLst = directChild(custGeom, "pathLst");
        if (pathLst == null) {
            return null;
        }
        List<List<float[]>> subpaths = new ArrayList<>();
        NodeList paths = pathLst.getChildNodes();
        for (int pathIndex = 0; pathIndex < paths.getLength(); pathIndex++) {
            Node path = paths.item(pathIndex);
            if (path.getNodeType() != Node.ELEMENT_NODE || !"path".equals(path.getLocalName())) {
                continue;
            }
            long pathW = widthEmu > 0 ? widthEmu : 1;
            long pathH = heightEmu > 0 ? heightEmu : 1;
            long parsedW = attributeEmu(path, "w");
            long parsedH = attributeEmu(path, "h");
            if (parsedW > 0) {
                pathW = parsedW;
            }
            if (parsedH > 0) {
                pathH = parsedH;
            }
            Map<String, Double> vars = new HashMap<>();
            vars.put("w", (double) pathW);
            vars.put("h", (double) pathH);
            Node gdLst = directChild(custGeom, "gdLst");
            if (gdLst != null) {
                NodeList guides = gdLst.getChildNodes();
                for (int guideIndex = 0; guideIndex < guides.getLength(); guideIndex++) {
                    Node guide = guides.item(guideIndex);
                    if (guide.getNodeType() != Node.ELEMENT_NODE || !"gd".equals(guide.getLocalName())) {
                        continue;
                    }
                    String name = attribute(guide, "name");
                    String formula = attribute(guide, "fmla");
                    if (name != null && formula != null) {
                        vars.put(name, evaluateGuideFormula(formula, vars));
                    }
                }
            }
            List<float[]> current = new ArrayList<>();
            NodeList commands = path.getChildNodes();
            for (int commandIndex = 0; commandIndex < commands.getLength(); commandIndex++) {
                Node command = commands.item(commandIndex);
                if (command.getNodeType() != Node.ELEMENT_NODE) {
                    continue;
                }
                String commandName = command.getLocalName();
                if ("moveTo".equals(commandName)) {
                    if (current.size() >= 3) {
                        subpaths.add(current);
                    }
                    current = new ArrayList<>();
                    float[] point = pathPoint(directChild(command, "pt"), vars, pathW, pathH);
                    if (point != null) {
                        current.add(point);
                    }
                } else if ("lnTo".equals(commandName)) {
                    float[] point = pathPoint(directChild(command, "pt"), vars, pathW, pathH);
                    if (point != null) {
                        current.add(point);
                    }
                } else if ("close".equals(commandName)) {
                    if (current.size() >= 3) {
                        subpaths.add(current);
                        current = new ArrayList<>();
                    }
                }
            }
            if (current.size() >= 3) {
                subpaths.add(current);
            }
        }
        return subpaths.isEmpty() ? null : subpaths;
    }

    private static float[] pathPoint(Node point, Map<String, Double> vars, long pathW, long pathH) {
        if (point == null) {
            return null;
        }
        String xToken = attribute(point, "x");
        String yToken = attribute(point, "y");
        if (xToken == null || yToken == null) {
            return null;
        }
        double x = resolveGuideToken(xToken, vars);
        double y = resolveGuideToken(yToken, vars);
        if (pathW <= 0 || pathH <= 0) {
            return null;
        }
        return new float[] {
                (float) Math.max(-0.25, Math.min(1.25, x / pathW)),
                (float) Math.max(-0.25, Math.min(1.25, y / pathH)),
        };
    }

    private static double evaluateGuideFormula(String formula, Map<String, Double> vars) {
        String[] tokens = formula.trim().split("\\s+");
        if (tokens.length == 0) {
            return 0.0;
        }
        if ("val".equals(tokens[0]) && tokens.length >= 2) {
            return resolveGuideToken(tokens[1], vars);
        }
        if ("*/".equals(tokens[0]) && tokens.length >= 4) {
            double a = resolveGuideToken(tokens[1], vars);
            double b = resolveGuideToken(tokens[2], vars);
            double c = resolveGuideToken(tokens[3], vars);
            return Math.abs(c) < 0.0001 ? 0.0 : a * b / c;
        }
        if ("+-".equals(tokens[0]) && tokens.length >= 4) {
            return resolveGuideToken(tokens[1], vars)
                    + resolveGuideToken(tokens[2], vars)
                    - resolveGuideToken(tokens[3], vars);
        }
        return tokens.length == 1 ? resolveGuideToken(tokens[0], vars) : 0.0;
    }

    private static double resolveGuideToken(String token, Map<String, Double> vars) {
        try {
            return Double.parseDouble(token);
        } catch (NumberFormatException ignored) {
            Double value = vars.get(token);
            return value == null ? 0.0 : value;
        }
    }

    private static void collectDescendants(Node node, String localName, List<Node> result) {
        if (node == null) {
            return;
        }
        NodeList children = node.getChildNodes();
        for (int index = 0; index < children.getLength(); index++) {
            Node child = children.item(index);
            if (localName.equals(child.getLocalName())) {
                result.add(child);
            }
            collectDescendants(child, localName, result);
        }
    }

    private static long emuValue(Node node) {
        if (node == null) {
            return 0L;
        }
        String text = nodeText(node);
        if (text == null) {
            return 0L;
        }
        try {
            return Long.parseLong(text.trim());
        } catch (NumberFormatException ignored) {
            return 0L;
        }
    }

    private static long attributeEmu(Node node, String localName) {
        String value = attribute(node, localName);
        if (value == null || value.isEmpty()) {
            return 0L;
        }
        try {
            return Long.parseLong(value.trim());
        } catch (NumberFormatException ignored) {
            return 0L;
        }
    }

        private static void renderTable(
            PageContext context,
            XWPFTable table,
            ParagraphFonts fonts,
            PDFont boldFont,
            XWPFDocument document) throws IOException {
        PDFont font = fonts.simSun();
        float tableWidth = twipsToPoints(table.getWidth());
        boolean autoWidth = tableWidth <= 0.0f;
        if (autoWidth) {
            tableWidth = context.pageSize.width() - context.margin * 2.0f;
        }
        // Word applies a default table indent of -108 twips (one cell margin)
        // to auto-width tables so the left cell margin lands on the text
        // margin. Verified against Word and LibreOffice: the thesis table
        // borders start 5.4pt left of the 90pt margin.
        float x = autoWidth
                ? context.margin - CELL_HORIZONTAL_PADDING
                : (context.pageSize.width() - tableWidth) / 2.0f;
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
            : landscape ? 0.0f : 1.9f;
        context.moveDown(spacingBefore);
        context.ensureSpace(tableHeight);

        float rowTop = context.y;
        for (int rowIndex = 0; rowIndex < table.getRows().size(); rowIndex++) {
            XWPFTableRow row = table.getRows().get(rowIndex);
            float rowHeight = rowHeights.get(rowIndex);
            boolean firstRow = rowIndex == 0;
            boolean lastRow = rowIndex == table.getRows().size() - 1;
            float cellX = x;
            int column = 0;
            int cellCount = row.getTableCells().size();
            for (int cellIndex = 0; cellIndex < cellCount; cellIndex++) {
                XWPFTableCell cell = row.getTableCells().get(cellIndex);
                float width = twipsToPoints(cell.getWidth());
                if (width <= 0.0f) {
                    width = column < columnWidths.size() ? columnWidths.get(column) : tableWidth;
                }
                drawCellBorder(
                    context,
                    table,
                    cell,
                    cellX,
                    rowTop,
                    width,
                    rowHeight,
                    firstRow,
                    lastRow,
                    cellIndex == 0,
                    cellIndex == cellCount - 1,
                    document);
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
        // A table interrupts the paragraph chain: the next paragraph's
        // spacing-before must not collapse with the spacing-after of the
        // paragraph preceding the table (Word does not collapse across tables).
        context.resetParagraphSpacingAfter();
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
        List<CellLine> lines = cellLines(cell, cellFont, width, 0.0f);
        if (mergeLine.index() >= lines.size()) {
            return;
        }
        CellLine line = lines.get(mergeLine.index());
        showText(context.content, cellFont, line.fontSize, line.text, x, top - line.fontSize);
    }

    private static void drawCellBorder(
            PageContext context,
            XWPFTable table,
            XWPFTableCell cell,
            float x,
            float top,
            float width,
            float height,
            boolean firstRow,
            boolean lastRow,
            boolean firstColumn,
            boolean lastColumn,
            XWPFDocument document) throws IOException {
        context.content.setStrokingColor(0, 0, 0);
        context.content.setLineWidth(0.5f);
        CTTcBorders borders = cell.getCTTc().isSetTcPr()
            && cell.getCTTc().getTcPr().isSetTcBorders()
            ? cell.getCTTc().getTcPr().getTcBorders()
            : null;
        CTTblBorders tableBorders = resolvedTableBorders(table, document);
        if (borders == null && isBorderlessLayoutTable(table, tableBorders)) {
            return;
        }
        String topStyle = resolveBorderStyle(
            borders != null && borders.isSetTop() ? borders.getTop() : null,
            tableBorders == null ? null : firstRow ? tableBorders.getTop() : tableBorders.getInsideH());
        String leftStyle = resolveBorderStyle(
            borders != null && borders.isSetLeft() ? borders.getLeft() : null,
            tableBorders == null ? null : firstColumn ? tableBorders.getLeft() : tableBorders.getInsideV());
        String bottomStyle = resolveBorderStyle(
            borders != null && borders.isSetBottom() ? borders.getBottom() : null,
            tableBorders == null ? null : lastRow ? tableBorders.getBottom() : tableBorders.getInsideH());
        String rightStyle = resolveBorderStyle(
            borders != null && borders.isSetRight() ? borders.getRight() : null,
            tableBorders == null ? null : lastColumn ? tableBorders.getRight() : tableBorders.getInsideV());
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

    /**
     * Resolves the table's effective borders the way Word does: explicit
     * w:tblBorders wins, then the table style's tblBorders, then a built-in
     * "Grid"-style heuristic for styles that are referenced but not embedded.
     */
    private static CTTblBorders resolvedTableBorders(XWPFTable table, XWPFDocument document) {
        CTTblBorders direct = table.getCTTbl().getTblPr().isSetTblBorders()
                ? table.getCTTbl().getTblPr().getTblBorders()
                : null;
        if (direct != null) {
            return direct;
        }
        CTTblBorders style = tableStyleBorders(table, document);
        if (style != null) {
            return style;
        }
        String styleId = table.getStyleID();
        if (styleId != null && styleId.toLowerCase().contains("grid")) {
            return syntheticGridBorders();
        }
        return null;
    }

    private static CTTblBorders tableStyleBorders(XWPFTable table, XWPFDocument document) {
        String styleId = table.getStyleID();
        if (styleId == null || styleId.isEmpty()) {
            return null;
        }
        CTStyles styles = document.getStyles().getCtStyles();
        if (styles == null) {
            return null;
        }
        for (CTStyle style : styles.getStyleList()) {
            if (!styleId.equals(style.getStyleId())) {
                continue;
            }
            // Read the style's w:tblPr/w:tblBorders through the DOM instead of
            // CTStyle#getTblPr(), which materializes CTTblPrBase — a schema type
            // only present in poi-ooxml-full, not poi-ooxml-lite.
            Node tblPr = directChild(style.getDomNode(), "tblPr");
            Node tblBorders = tblPr == null ? null : directChild(tblPr, "tblBorders");
            if (tblBorders == null) {
                return null;
            }
            CTTblBorders borders = CTTblBorders.Factory.newInstance();
            copyStyleBorderSide(borders, tblBorders, "top");
            copyStyleBorderSide(borders, tblBorders, "left");
            copyStyleBorderSide(borders, tblBorders, "bottom");
            copyStyleBorderSide(borders, tblBorders, "right");
            copyStyleBorderSide(borders, tblBorders, "insideH");
            copyStyleBorderSide(borders, tblBorders, "insideV");
            return borders;
        }
        return null;
    }

    private static void copyStyleBorderSide(CTTblBorders borders, Node tblBorders, String side) {
        Node sideNode = directChild(tblBorders, side);
        if (sideNode == null) {
            return;
        }
        String value = attribute(sideNode, "val");
        if (value == null || value.isEmpty()) {
            return;
        }
        STBorder.Enum borderValue;
        try {
            borderValue = STBorder.Enum.forString(value);
        } catch (IllegalArgumentException ignored) {
            return;
        }
        CTBorder border;
        switch (side) {
            case "top":
                border = borders.addNewTop();
                break;
            case "left":
                border = borders.addNewLeft();
                break;
            case "bottom":
                border = borders.addNewBottom();
                break;
            case "right":
                border = borders.addNewRight();
                break;
            case "insideH":
                border = borders.addNewInsideH();
                break;
            case "insideV":
                border = borders.addNewInsideV();
                break;
            default:
                return;
        }
        border.setVal(borderValue);
    }

    private static CTTblBorders syntheticGridBorders() {
        CTTblBorders borders = CTTblBorders.Factory.newInstance();
        for (CTBorder side : new CTBorder[] {
                borders.addNewTop(),
                borders.addNewLeft(),
                borders.addNewBottom(),
                borders.addNewRight(),
                borders.addNewInsideH(),
                borders.addNewInsideV()}) {
            side.setVal(STBorder.SINGLE);
        }
        return borders;
    }

    static String resolveBorderStyle(CTBorder cellBorder, CTBorder tableBorder) {
        CTBorder border = cellBorder == null ? tableBorder : cellBorder;
        return border == null || border.getVal() == null ? null : border.getVal().toString();
    }

    private static boolean explicitlyBorderless(CTTblBorders borders) {
        return borders != null
                && !isVisibleBorder(resolveBorderStyle(borders.getTop(), null))
                && !isVisibleBorder(resolveBorderStyle(borders.getLeft(), null))
                && !isVisibleBorder(resolveBorderStyle(borders.getBottom(), null))
                && !isVisibleBorder(resolveBorderStyle(borders.getRight(), null))
                && !isVisibleBorder(resolveBorderStyle(borders.getInsideH(), null))
                && !isVisibleBorder(resolveBorderStyle(borders.getInsideV(), null));
    }

    private static boolean isBorderlessLayoutTable(XWPFTable table, CTTblBorders borders) {
        return table.getRows().size() == 1
                && table.getRows().get(0).getTableCells().size() == 1
                && explicitlyBorderless(borders);
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
        float horizontalPadding = compact ? 0.0f : CELL_HORIZONTAL_PADDING;
        // Word's default table cell margin is 0 top/bottom (only left/right
        // default to 108 twips), so no vertical padding is added.
        float verticalPadding = 0.0f;
        List<XWPFParagraph> paragraphs = cellParagraphs(cell);
        List<CellItem> items = cellItems(cell, cellFont, width, horizontalPadding, context.linePitch);
        float contentHeight = 0.0f;
        for (CellItem item : items) {
            contentHeight += item.line() != null
                    ? item.line().advance(context.linePitch)
                    : item.pictureHeight();
        }
        // Cursor tracks the top of the current content line (or image).
        float cursor = top - verticalPadding - paragraphSpacingBefore(cell);
        if (cell.getVerticalAlignment() == XWPFTableCell.XWPFVertAlign.CENTER) {
            cursor -= (height - contentHeight) / 2.0f;
        } else if (cell.getVerticalAlignment() == XWPFTableCell.XWPFVertAlign.BOTTOM) {
            cursor -= height - contentHeight;
        }
        ParagraphAlignment alignment = paragraphs.isEmpty()
                ? ParagraphAlignment.LEFT
                : paragraphs.get(0).getAlignment();
        boolean highlighted = paragraphs.stream()
                .flatMap(paragraph -> paragraph.getRuns().stream())
                .anyMatch(run -> run.getTextHighlightColor() != null
                        && "yellow".equalsIgnoreCase(run.getTextHighlightColor().toString()));
        for (CellItem item : items) {
            if (item.line() == null) {
                XWPFPicture picture = item.picture();
                // XWPFPicture#getWidth/getDepth already return points.
                float imageWidth = (float) picture.getWidth();
                float imageHeight = (float) picture.getDepth();
                float imageX = x + horizontalPadding;
                if (alignment == ParagraphAlignment.CENTER) {
                    imageX = x + (width - imageWidth) / 2.0f;
                } else if (alignment == ParagraphAlignment.RIGHT) {
                    imageX = x + width - horizontalPadding - imageWidth;
                }
                try {
                    PDImageXObject image = PDImageXObject.createFromByteArray(
                            context.document,
                            picture.getPictureData().getData(),
                            picture.getDescription());
                    context.content.drawImage(image, imageX, cursor - imageHeight, imageWidth, imageHeight);
                } catch (IOException | IllegalArgumentException ignored) {
                    // Unsupported image data must not abort cell rendering.
                }
                cursor -= imageHeight;
                continue;
            }
            CellLine line = item.line();
            float lineWidth = textWidth(cellFont, line.text, line.fontSize);
            float lineX = x + horizontalPadding;
            if (alignment == ParagraphAlignment.CENTER) {
                lineX = x + (width - lineWidth) / 2.0f;
            } else if (alignment == ParagraphAlignment.RIGHT) {
                lineX = x + width - horizontalPadding - lineWidth;
            }
            if (highlighted && !line.text.isEmpty()) {
                context.content.setNonStrokingColor(1.0f, 1.0f, 0.0f);
                context.content.addRect(lineX, cursor - line.fontSize - 1.0f, lineWidth, line.fontSize + 2.0f);
                context.content.fill();
            }
            applyTextColor(context.content, line.color);
            showText(context.content, cellFont, line.fontSize, line.text, lineX, cursor - line.fontSize);
            cursor -= line.advance(context.linePitch);
        }
        context.content.setNonStrokingColor(0.0f, 0.0f, 0.0f);
    }

    /**
     * Returns the paragraphs that make up a cell's content: direct w:p
     * children plus paragraphs inside block-level w:sdt content controls, in
     * document order. POI's XWPFTableCell#getParagraphs() only surfaces direct
     * w:p children, so SDT-wrapped body text (e.g. Word newsletter templates)
     * would otherwise disappear.
     */
    static List<XWPFParagraph> cellParagraphs(XWPFTableCell cell) {
        List<XWPFParagraph> paragraphs = new ArrayList<>();
        XmlCursor cursor = cell.getCTTc().newCursor();
        try {
            if (cursor.toFirstChild()) {
                do {
                    if (cursor.isStart()) {
                        collectCellParagraphs(cursor.getObject(), cell, paragraphs);
                    }
                } while (cursor.toNextSibling());
            }
        } finally {
            cursor.dispose();
        }
        if (paragraphs.isEmpty()) {
            paragraphs.addAll(cell.getParagraphs());
        }
        return paragraphs;
    }

    private static void collectCellParagraphs(XmlObject object, XWPFTableCell cell, List<XWPFParagraph> paragraphs) {
        if (object instanceof CTP) {
            // Copy so the wrapped paragraph is independent of the document's
            // live typed tree (copy preserves the child element types).
            paragraphs.add(new XWPFParagraph((CTP) object.copy(), cell));
            return;
        }
        // Block-level SDTs wrap their paragraphs in w:sdtContent.
        XmlCursor cursor = object.newCursor();
        try {
            if (cursor.toFirstChild()) {
                do {
                    if (!cursor.isStart()) {
                        continue;
                    }
                    String name = cursor.getName() == null ? null : cursor.getName().getLocalPart();
                    if ("sdtContent".equals(name)) {
                        XmlCursor inner = cursor.getObject().newCursor();
                        try {
                            if (inner.toFirstChild()) {
                                do {
                                    if (inner.isStart()) {
                                        collectCellParagraphs(inner.getObject(), cell, paragraphs);
                                    }
                                } while (inner.toNextSibling());
                            }
                        } finally {
                            inner.dispose();
                        }
                    }
                } while (cursor.toNextSibling());
            }
        } finally {
            cursor.dispose();
        }
    }

    private static final class CellItem {
        private final XWPFPicture picture;
        private final CellLine line;

        private CellItem(XWPFPicture picture, CellLine line) {
            this.picture = picture;
            this.line = line;
        }

        static CellItem image(XWPFPicture picture) {
            return new CellItem(picture, null);
        }

        static CellItem text(CellLine line) {
            return new CellItem(null, line);
        }

        XWPFPicture picture() {
            return picture;
        }

        CellLine line() {
            return line;
        }

        float pictureHeight() {
            // XWPFPicture#getDepth already returns points.
            return picture == null ? 0.0f : Math.max(1.0f, (float) picture.getDepth());
        }
    }

    private static List<CellItem> cellItems(
            XWPFTableCell cell,
            PDFont font,
            float width,
            float padding,
            float linePitch) throws IOException {
        List<CellItem> items = new ArrayList<>();
        boolean noWrap = cell.getCTTc().isSetTcPr() && cell.getCTTc().getTcPr().isSetNoWrap();
        for (XWPFParagraph paragraph : cellParagraphs(cell)) {
            boolean hasPictures = false;
            for (XWPFRun run : paragraph.getRuns()) {
                for (XWPFPicture picture : run.getEmbeddedPictures()) {
                    if (picture.getPictureData() == null) {
                        continue;
                    }
                    items.add(CellItem.image(picture));
                    hasPictures = true;
                }
            }
            String text = paragraphText(paragraph).trim();
            float fontSize = paragraphFontSize(paragraph, DEFAULT_TABLE_FONT_SIZE);
            // A picture-only paragraph's image replaces its paragraph-mark line.
            if (text.isEmpty() && hasPictures) {
                continue;
            }
            // Mirrors the body paragraph color chain: an explicit run color
            // wins, otherwise the paragraph style's color applies (e.g. the
            // green MastheadGREEN title).
            String color = paragraphStyleColor(paragraph);
            for (XWPFRun run : paragraph.getRuns()) {
                String runColor = runColor(run);
                if (runColor != null) {
                    color = runColor;
                    break;
                }
            }
            // Each paragraph advances by its own effective line height (a
            // uniform per-cell height wrongly inflates mixed-size cells like
            // a 24pt heading followed by 11pt body text).
            float advance = paragraphCellLineHeight(paragraph, fontSize, linePitch);
            if (noWrap) {
                items.add(CellItem.text(new CellLine(text, fontSize, advance, color)));
            } else {
                for (String line : wrap(font, text, fontSize, cellContentWidth(width, padding))) {
                    items.add(CellItem.text(new CellLine(line, fontSize, advance, color)));
                }
            }
        }
        if (items.isEmpty()) {
            items.add(CellItem.text(new CellLine("", DEFAULT_TABLE_FONT_SIZE)));
        }
        return items;
    }

    /**
     * Effective line height for a cell paragraph: explicit line rule when
     * present, otherwise the natural height. Honors each paragraph's own
     * spacing rule instead of taking the tallest paragraph of the cell.
     */
    private static float paragraphCellLineHeight(XWPFParagraph paragraph, float fontSize, float linePitch) {
        float natural = fontSize * 1.2f;
        double spacing = paragraph.getSpacingBetween();
        if (spacing < 0.0) {
            return gridLineHeight(natural, linePitch);
        }
        LineSpacingRule rule = paragraph.getSpacingLineRule();
        if (rule == LineSpacingRule.EXACT) {
            return Math.max(1.0f, (float) spacing);
        }
        if (rule == LineSpacingRule.AT_LEAST) {
            return Math.max(natural, (float) spacing);
        }
        return natural * (float) spacing;
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

    private static boolean rowHeightExact(XWPFTableRow row) {
        Node trPr = directChild(row.getCtRow().getDomNode(), "trPr");
        Node trHeight = trPr == null ? null : directChild(trPr, "trHeight");
        return trHeight != null && "exact".equalsIgnoreCase(attribute(trHeight, "hRule"));
    }

    private static float rowHeight(
            XWPFTableRow row,
            ParagraphFonts fonts,
            PDFont boldFont,
            List<Float> widths,
            boolean compact,
            float rowHeightPadding,
            float linePitch) throws IOException {
        // Mirror DocxReader.cs: w:trHeight with hRule="exact" is a hard cap —
        // Word clips cell content to that height instead of growing the row.
        // Without this, a fixed-height business-card/label table expands every
        // row to its wrapped content height and overflows the page.
        if (rowHeightExact(row)) {
            return twipsToPoints(row.getHeight());
        }
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
            List<CellItem> items = cellItems(cell, cellFont, width, horizontalPadding, linePitch);
            float contentHeight = 0.0f;
            for (CellItem item : items) {
                if (item.line() != null) {
                    contentHeight += item.line().advance(linePitch);
                } else {
                    // Inline pictures advance the cell content flow like a
                    // line of text (Word grows the row to fit the image).
                    contentHeight += item.pictureHeight();
                }
            }
                height = Math.max(
                    height,
                    paragraphSpacingBefore(cell)
                        + contentHeight
                        + paragraphSpacingAfter(cell)
                        + (linePitch > 0.0f ? rowHeightPadding : 0.0f));
            column++;
        }
        return height;
    }

            private static float paragraphSpacingBefore(XWPFTableCell cell) {
            return cellParagraphs(cell).stream()
                .mapToInt(XWPFParagraph::getSpacingBefore)
                .max()
                .orElse(0) / 20.0f;
            }

            private static float paragraphSpacingAfter(XWPFTableCell cell) {
            return cellParagraphs(cell).stream()
                .mapToInt(XWPFParagraph::getSpacingAfter)
                .max()
                .orElse(0) / 20.0f;
            }

            private static boolean isCellBold(XWPFTableCell cell) {
                return cellParagraphs(cell).stream()
                        .flatMap(paragraph -> paragraph.getRuns().stream())
                        .anyMatch(XWPFRun::isBold);
            }

    private static final class CellLine {
        private final String text;
        private final float fontSize;
        private final float advance;
        private final String color;

        private CellLine(String text, float fontSize) {
            this(text, fontSize, 0.0f, null);
        }

        private CellLine(String text, float fontSize, float advance, String color) {
            this.text = text;
            this.fontSize = fontSize;
            this.advance = advance;
            this.color = color;
        }

        private float advance(float linePitch) {
            return advance > 0.0f ? advance : gridLineHeight(fontSize * 1.35f, linePitch);
        }
    }

    private static List<CellLine> cellLines(
            XWPFTableCell cell,
            PDFont font,
            float width,
            float padding)
            throws IOException {
        List<CellLine> lines = new ArrayList<>();
        for (XWPFParagraph paragraph : cellParagraphs(cell)) {
            String text = paragraphText(paragraph).trim();
            float fontSize = paragraphFontSize(paragraph, DEFAULT_TABLE_FONT_SIZE);
            if (cell.getCTTc().isSetTcPr() && cell.getCTTc().getTcPr().isSetNoWrap()) {
                lines.add(new CellLine(text, fontSize));
            } else {
                for (String line : wrap(font, text, fontSize, cellContentWidth(width, padding))) {
                    lines.add(new CellLine(line, fontSize));
                }
            }
        }
        return lines.isEmpty() ? Collections.singletonList(new CellLine("", DEFAULT_TABLE_FONT_SIZE)) : lines;
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
        return wrap(font, text, fontSize, width, width, autoSpacing);
    }

    private static List<String> wrap(
            PDFont font,
            String text,
            float fontSize,
            float firstLineWidth,
            float width,
            AutoSpacing autoSpacing) throws IOException {
        return wrap(font, text, fontSize, firstLineWidth, width, autoSpacing, false);
    }

    private static List<String> wrap(
            PDFont font,
            String text,
            float fontSize,
            float firstLineWidth,
            float width,
            AutoSpacing autoSpacing,
            boolean bold) throws IOException {
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
                    && textWidth(font, line.toString(), fontSize, autoSpacing, bold)
                        > (lines.isEmpty() ? firstLineWidth : width)) {
                int breakOffset = lastWrapBoundary(line);
                if (breakOffset <= 0) {
                    // An unbreakable token wider than the line (e.g. an email
                    // address): Word/LibreOffice let it overflow instead of
                    // splitting it mid-word.
                    break;
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
        private final float fontSize;

        private PageNumberFooter(String prefix, String suffix, float fontSize) {
            this.prefix = prefix;
            this.suffix = suffix;
            this.fontSize = fontSize;
        }

        private String prefix() {
            return prefix;
        }

        private String suffix() {
            return suffix;
        }

        private float fontSize() {
            return fontSize;
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
                return prefix.equals(other.prefix)
                    && suffix.equals(other.suffix)
                    && Float.compare(fontSize, other.fontSize) == 0;
        }

        @Override
        public int hashCode() {
            return 31 * (31 * prefix.hashCode() + suffix.hashCode()) + Float.floatToIntBits(fontSize);
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
        private final HighlightColor highlight;
        private final boolean tab;
        private final boolean bold;
        private final String color;

        RunSegment(
                String text,
                PDFont font,
                float fontSize,
                float leadingSpacing,
                HighlightColor highlight,
                boolean tab) {
            this(text, font, fontSize, leadingSpacing, highlight, tab, false, null);
        }

        RunSegment(
                String text,
                PDFont font,
                float fontSize,
                float leadingSpacing,
                HighlightColor highlight,
                boolean tab,
                boolean bold) {
            this(text, font, fontSize, leadingSpacing, highlight, tab, bold, null);
        }

        RunSegment(
                String text,
                PDFont font,
                float fontSize,
                float leadingSpacing,
                HighlightColor highlight,
                boolean tab,
                boolean bold,
                String color) {
            this.text = text;
            this.font = font;
            this.fontSize = fontSize;
            this.leadingSpacing = leadingSpacing;
            this.highlight = highlight;
            this.tab = tab;
            this.bold = bold;
            this.color = color;
        }

        String text() {
            return text;
        }

        RunSegment prepend(String prefix) {
            return new RunSegment(
                    prefix + text, font, fontSize, leadingSpacing, highlight, tab, bold, color);
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

        HighlightColor highlight() {
            return highlight;
        }

        boolean tab() {
            return tab;
        }

        boolean bold() {
            return bold;
        }

        String color() {
            return color;
        }
    }

    static final class HighlightColor {
        private final float red;
        private final float green;
        private final float blue;

        HighlightColor(float red, float green, float blue) {
            this.red = red;
            this.green = green;
            this.blue = blue;
        }
    }

    private static final class FootnoteData {
        private final String number;
        private final String text;
        private final PDFont font;
        private final float fontSize;

        private FootnoteData(String number, String text, PDFont font, float fontSize) {
            this.number = number;
            this.text = text;
            this.font = font;
            this.fontSize = fontSize;
        }

        private float lineHeight() {
            return fontSize * 1.2f;
        }

        private String displayText() {
            return number + " " + text;
        }
    }

    static final class FloatingCheckbox {
        private final float offsetX;
        private final float offsetY;
        private final float width;
        private final float height;
        private final float lineWidth;
        private final String horizontalRelative;
        private final boolean checked;

        private FloatingCheckbox(
                float offsetX,
            float offsetY,
                float width,
                float height,
                float lineWidth,
                String horizontalRelative,
                boolean checked) {
            this.offsetX = offsetX;
            this.offsetY = offsetY;
            this.width = width;
            this.height = height;
            this.lineWidth = lineWidth;
            this.horizontalRelative = horizontalRelative;
            this.checked = checked;
        }

        float offsetX() {
            return offsetX;
        }

        float offsetY() {
            return offsetY;
        }

        boolean checked() {
            return checked;
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

        PDFont latin() {
            String family = defaultAsciiFamily == null ? "" : defaultAsciiFamily.toLowerCase();
            if (family.contains("times") || family.contains("georgia")) {
                return times;
            }
            return arial;
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
        if (text.codePoints().anyMatch(PoiDocxRenderer::usesEastAsianFontSlot)) {
            return fonts.simSun();
        }
        // Mirror DocxReader.cs: Latin text must wrap with the document's Latin
        // font metrics (Times/Arial), not the CJK fallback (SimHei renders Latin
        // glyphs at half-width 500/1000). Wrapping with half-width glyphs makes
        // every line ~10% narrower than Word, collapsing pagination.
        return fonts.latin();
    }

    static List<RunSegment> paragraphSegments(
            XWPFParagraph paragraph,
            ParagraphFonts fonts,
            float fallbackFontSize) {
        return paragraphSegments(paragraph, fonts, fallbackFontSize, null);
    }

    static List<RunSegment> paragraphSegments(
            XWPFParagraph paragraph,
            ParagraphFonts fonts,
            float fallbackFontSize,
            PDFont boldFont) {
        return paragraphSegments(paragraph, fonts, fallbackFontSize, boldFont, false);
        }

        private static List<RunSegment> paragraphSegments(
            XWPFParagraph paragraph,
            ParagraphFonts fonts,
            float fallbackFontSize,
            PDFont boldFont,
            boolean preserveTabs) {
        List<RunSegment> segments = new ArrayList<>();
        AutoSpacing autoSpacing = paragraphAutoSpacing(paragraph);
        boolean styleBold = paragraphStyleBold(paragraph);
        String styleColor = paragraphStyleColor(paragraph);
        for (XWPFRun run : paragraph.getRuns()) {
            String text = preserveTabs ? runText(run) : renderableText(runText(run));
            if (text.isEmpty()) {
                continue;
            }
            float fontSize = run.getFontSizeAsDouble() == null || run.getFontSizeAsDouble() <= 0.0
                    ? fallbackFontSize
                    : run.getFontSizeAsDouble().floatValue();
            StringBuilder segmentText = new StringBuilder();
            PDFont segmentFont = null;
            float segmentLeadingSpacing = 0.0f;
            boolean runBold = (run.isBold() || styleBold) && boldFont != null;
            String color = runColor(run);
            if (color == null) {
                color = styleColor;
            }
            int previous = -1;
            for (int offset = 0; offset < text.length();) {
                int codePoint = text.codePointAt(offset);
                offset += Character.charCount(codePoint);
                if (codePoint == '\t') {
                    if (segmentText.length() > 0) {
                        segments.add(new RunSegment(
                                segmentText.toString(), segmentFont, fontSize, segmentLeadingSpacing,
                                runHighlight(run), false, runBold, color));
                        segmentText.setLength(0);
                    }
                    segments.add(new RunSegment("\t", null, fontSize, 0.0f, null, true));
                    segmentFont = null;
                    segmentLeadingSpacing = 0.0f;
                    previous = -1;
                    continue;
                }
                PDFont codePointFont = runBold
                        ? boldFont
                        : fonts.resolve(run, codePoint);
                if (segmentFont != null && codePointFont != segmentFont) {
                    float spacing = isEastAsianLatinBoundary(previous, codePoint, autoSpacing)
                            ? fontSize * 0.25f
                            : 0.0f;
                    segments.add(new RunSegment(
                            segmentText.toString(), segmentFont, fontSize, segmentLeadingSpacing,
                            runHighlight(run), false, runBold, color));
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
                        segmentText.toString(), segmentFont, fontSize, segmentLeadingSpacing,
                    runHighlight(run), false, runBold, color));
            }
        }
        return segments;
    }

    static String paragraphText(XWPFParagraph paragraph) {
        boolean needsSemanticRuns = paragraph.getRuns().stream()
                .anyMatch(run -> run.getCTR().sizeOfFootnoteReferenceArray() > 0
                        || run.getCTR().sizeOfSymArray() > 0);
        if (!needsSemanticRuns) {
            return normalizeRunText(paragraph.getText());
        }
        return paragraph.getRuns().stream()
                .map(PoiDocxRenderer::runText)
                .collect(Collectors.joining());
    }

    static String runText(XWPFRun run) {
        if (run.getCTR().sizeOfFootnoteReferenceArray() > 0) {
            return run.getCTR().getFootnoteReferenceList().stream()
                    .map(reference -> reference.getId().toString())
                    .collect(Collectors.joining());
        }
        if (run.getCTR().sizeOfSymArray() > 0) {
            return run.getCTR().getSymList().stream()
                    .map(PoiDocxRenderer::symbolText)
                    .collect(Collectors.joining());
        }
        return normalizeRunText(run.text());
    }

    private static String normalizeRunText(String text) {
        return Normalizer.normalize(text.replace('\r', ' ').replace('\n', ' '), Normalizer.Form.NFC);
    }

    static float nextDefaultTabStop(float position) {
        return ((float) Math.floor(position / 36.0f) + 1.0f) * 36.0f;
    }

    private static String symbolText(CTSym symbol) {
        byte[] value = symbol.getChar();
        if (value == null || value.length == 0) {
            return "";
        }
        int code = 0;
        for (byte part : value) {
            code = code << 8 | part & 0xff;
        }
        if ("Wingdings".equalsIgnoreCase(symbol.getFont()) && code == 0xf0fc) {
            return "\u2713";
        }
        return Character.toString((char) code);
    }

    private static Map<String, FootnoteData> footnotes(XWPFDocument source, PDFont font) {
        Map<String, FootnoteData> footnotes = new LinkedHashMap<>();
        for (XWPFFootnote footnote : source.getFootnotes()) {
            BigInteger id = footnote.getId();
            if (id == null || id.signum() < 0) {
                continue;
            }
            String text = footnote.getParagraphs().stream()
                    .flatMap(paragraph -> paragraph.getRuns().stream())
                    .map(PoiDocxRenderer::footnoteRunText)
                    .collect(Collectors.joining())
                    .trim();
            float fontSize = footnote.getParagraphs().isEmpty()
                    ? 10.0f
                    : paragraphFontSize(footnote.getParagraphs().get(0), 10.0f);
            footnotes.put(id.toString(), new FootnoteData(id.toString(), text, font, fontSize));
        }
        return footnotes;
    }

    private static String footnoteRunText(XWPFRun run) {
        if (run.getCTR().sizeOfFootnoteRefArray() == 0) {
            return runText(run);
        }
        return java.util.Arrays.stream(run.getCTR().getTArray())
                .map(text -> text.getStringValue())
                .collect(Collectors.joining());
    }

    static HighlightColor runHighlight(XWPFRun run) {
        if (run.getTextHighlightColor() == null) {
            return null;
        }
        String color = run.getTextHighlightColor().toString().toLowerCase();
        if ("yellow".equals(color)) {
            return new HighlightColor(1.0f, 1.0f, 0.0f);
        }
        if ("green".equals(color)) {
            return new HighlightColor(0.0f, 1.0f, 0.0f);
        }
        if ("cyan".equals(color)) {
            return new HighlightColor(0.0f, 1.0f, 1.0f);
        }
        if ("magenta".equals(color)) {
            return new HighlightColor(1.0f, 0.0f, 1.0f);
        }
        if ("red".equals(color)) {
            return new HighlightColor(1.0f, 0.0f, 0.0f);
        }
        if ("blue".equals(color)) {
            return new HighlightColor(0.0f, 0.0f, 1.0f);
        }
        return null;
    }

    static PDFont resolvedCellFont(XWPFTableCell cell, ParagraphFonts fonts, PDFont boldFont) {
        PDFont resolved = null;
        for (XWPFParagraph paragraph : cellParagraphs(cell)) {
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

    private static PDFont loadDocumentLatinFont(
            PDDocument document,
            List<List<String>> text,
            String family) {
        String normalized = family == null ? "" : family.toLowerCase();
        if (normalized.contains("times") || normalized.contains("georgia")) {
            return SimplePdfTextRenderer.loadSystemFont(
                    document, text, "times.ttf", "NotoSerif-Regular.ttf");
        }
        if (normalized.contains("arial")
                || normalized.contains("calibri")
                || normalized.contains("cambria")
                || normalized.isEmpty()) {
            // Theme defaults (minorHAnsi, e.g. Calibri) and unknown western
            // families use Arial metrics — mirrors the .NET fallback so that
            // Latin documents take the Word-paragraph layout path instead of
            // the CJK grid/margin path.
            return SimplePdfTextRenderer.loadSystemFont(
                    document, text, "arial.ttf", "NotoSans-Regular.ttf");
        }
        return null;
    }

    private static String defaultFontFamily(XWPFDocument document, XWPFRun.FontCharRange range) {
        if (document.getStyles() == null || document.getStyles().getCtStyles() == null) {
            return null;
        }
        XWPFStyle normalStyle = document.getStyles().getStyle("Normal");
        if (normalStyle != null && normalStyle.getCTStyle().isSetRPr()) {
            String family = fontFamily(normalStyle.getCTStyle().getRPr(), range);
            if (family != null) {
                return family;
            }
        }
        CTStyles styles = document.getStyles().getCtStyles();
        if (!styles.isSetDocDefaults()
                || !styles.getDocDefaults().isSetRPrDefault()
                || !styles.getDocDefaults().getRPrDefault().isSetRPr()) {
            return null;
        }
        return fontFamily(styles.getDocDefaults().getRPrDefault().getRPr(), range);
    }

    private static String fontFamily(CTRPr properties, XWPFRun.FontCharRange range) {
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
        // Mirror the .NET effective-size chain: direct run -> paragraph mark ->
        // paragraph style -> Normal style -> docDefaults. Without this, a body
        // paragraph that inherits its size from Normal (e.g. 12pt) falls back
        // to the 11pt default, making every line ~0.5pt shorter and collapsing
        // pagination.
        XWPFStyles styles = paragraph.getDocument().getStyles();
        if (styles != null) {
            String styleId = paragraph.getStyle();
            if (styleId != null) {
                Float styleSize = styleFontSize(styles.getStyle(styleId));
                if (styleSize != null) {
                    return styleSize;
                }
            }
            Float normalSize = styleFontSize(styles.getStyle("Normal"));
            if (normalSize != null) {
                return normalSize;
            }
            Float docDefaultSize = docDefaultsFontSize(styles.getCtStyles());
            if (docDefaultSize != null) {
                return docDefaultSize;
            }
        }
        return fallback;
    }

    private static Float styleFontSize(XWPFStyle style) {
        if (style == null || style.getCTStyle() == null || !style.getCTStyle().isSetRPr()) {
            return null;
        }
        CTRPr properties = style.getCTStyle().getRPr();
        if (properties.sizeOfSzArray() == 0) {
            return null;
        }
        try {
            return Float.parseFloat(properties.getSzArray(0).getVal().toString()) / 2.0f;
        } catch (NumberFormatException ignored) {
            return null;
        }
    }

    private static Float docDefaultsFontSize(CTStyles styles) {
        if (styles == null || !styles.isSetDocDefaults()
                || !styles.getDocDefaults().isSetRPrDefault()
                || !styles.getDocDefaults().getRPrDefault().isSetRPr()) {
            return null;
        }
        CTRPr properties = styles.getDocDefaults().getRPrDefault().getRPr();
        if (properties.sizeOfSzArray() == 0) {
            return null;
        }
        try {
            return Float.parseFloat(properties.getSzArray(0).getVal().toString()) / 2.0f;
        } catch (NumberFormatException ignored) {
            return null;
        }
    }

    /**
     * Mirrors Word's effective bold: a run is bold if its own rPr says so, or
     * if the paragraph mark or paragraph style (e.g. Heading2) turns bold on.
     * POI's XWPFRun#isBold only reads the direct run rPr, so heading styles
     * would otherwise render non-bold.
     */
    private static boolean paragraphStyleBold(XWPFParagraph paragraph) {
        if (paragraph.getCTP().isSetPPr() && paragraph.getCTP().getPPr().isSetRPr()) {
            CTParaRPr mark = paragraph.getCTP().getPPr().getRPr();
            if (isBoldOn(mark.sizeOfBArray() == 0 ? null : mark.getBArray(0))) {
                return true;
            }
        }
        if (paragraph.getDocument().getStyles() == null) {
            return false;
        }
        String styleId = paragraph.getStyle();
        if (styleId != null) {
            XWPFStyle style = paragraph.getDocument().getStyles().getStyle(styleId);
            if (style != null && style.getCTStyle() != null && style.getCTStyle().isSetRPr()) {
                CTRPr properties = style.getCTStyle().getRPr();
                if (isBoldOn(properties.sizeOfBArray() == 0 ? null : properties.getBArray(0))) {
                    return true;
                }
            }
        }
        return false;
    }

    private static boolean isBoldOn(CTOnOff bold) {
        if (bold == null) {
            return false;
        }
        Object value = bold.getVal();
        return value == null
                || !"0".equals(value.toString()) && !"false".equalsIgnoreCase(value.toString());
    }

    /**
     * Mirrors Word's contextualSpacing flag: set on the paragraph itself or on
     * its style (e.g. ListBullet). Between consecutive same-style paragraphs,
     * Word suppresses the spacing-after of the first one.
     */
    private static boolean paragraphContextualSpacing(XWPFParagraph paragraph) {
        Node pPr = directChild(paragraph.getCTP().getDomNode(), "pPr");
        if (directChild(pPr, "contextualSpacing") != null) {
            return true;
        }
        if (paragraph.getDocument().getStyles() == null) {
            return false;
        }
        String styleId = paragraph.getStyle();
        if (styleId == null) {
            return false;
        }
        XWPFStyle style = paragraph.getDocument().getStyles().getStyle(styleId);
        if (style == null || style.getCTStyle() == null) {
            return false;
        }
        Node stylePPr = directChild(style.getCTStyle().getDomNode(), "pPr");
        return directChild(stylePPr, "contextualSpacing") != null;
    }

    /**
     * Mirrors Word's keepNext: set on the paragraph pPr or its style pPr
     * (e.g. Heading2/Heading3). Prevents the paragraph from being orphaned at
     * the bottom of a page.
     */
    private static boolean paragraphKeepNext(XWPFParagraph paragraph) {
        Node pPr = directChild(paragraph.getCTP().getDomNode(), "pPr");
        if (keepNextOn(directChild(pPr, "keepNext"))) {
            return true;
        }
        if (paragraph.getDocument().getStyles() == null) {
            return false;
        }
        String styleId = paragraph.getStyle();
        if (styleId == null) {
            return false;
        }
        XWPFStyle style = paragraph.getDocument().getStyles().getStyle(styleId);
        if (style == null || style.getCTStyle() == null) {
            return false;
        }
        Node stylePPr = directChild(style.getCTStyle().getDomNode(), "pPr");
        return keepNextOn(directChild(stylePPr, "keepNext"));
    }

    private static boolean keepNextOn(Node node) {
        if (node == null) {
            return false;
        }
        String value = attribute(node, "val");
        return value == null || (!"0".equals(value) && !"false".equalsIgnoreCase(value));
    }

    /**
     * Returns the run's explicit w:color/w:val hex value, or null when the run
     * has no color (or "auto", which resets to the paragraph default). Read via
     * DOM to avoid materializing CTColor from the lite schema.
     */
    private static String runColor(XWPFRun run) {
        Node rPr = directChild(run.getCTR().getDomNode(), "rPr");
        Node color = directChild(rPr, "color");
        String value = attribute(color, "val");
        if (value == null || value.isEmpty() || value.equalsIgnoreCase("auto")) {
            return null;
        }
        return value;
    }

    /**
     * Mirrors Word's paragraph-level color: runs without an explicit color
     * inherit the paragraph style's rPr/w:color (e.g. Heading2 = 4F81BD blue).
     * Resolved via DOM so no theme mapping is needed when w:val is present.
     */
    private static String paragraphStyleColor(XWPFParagraph paragraph) {
        if (paragraph.getDocument().getStyles() == null) {
            return null;
        }
        String styleId = paragraph.getStyle();
        if (styleId == null) {
            return null;
        }
        XWPFStyle style = paragraph.getDocument().getStyles().getStyle(styleId);
        if (style == null || style.getCTStyle() == null) {
            return null;
        }
        Node rPr = directChild(style.getCTStyle().getDomNode(), "rPr");
        Node color = directChild(rPr, "color");
        String value = attribute(color, "val");
        if (value == null || value.isEmpty() || value.equalsIgnoreCase("auto")) {
            return null;
        }
        return value;
    }

    private static void applyTextColor(PDPageContentStream content, String hex) throws IOException {
        if (hex == null || hex.isEmpty()) {
            content.setNonStrokingColor(0.0f, 0.0f, 0.0f);
            return;
        }
        try {
            int value = Integer.parseInt(hex, 16);
            content.setNonStrokingColor(
                    ((value >> 16) & 0xFF) / 255.0f,
                    ((value >> 8) & 0xFF) / 255.0f,
                    (value & 0xFF) / 255.0f);
        } catch (NumberFormatException ignored) {
            content.setNonStrokingColor(0.0f, 0.0f, 0.0f);
        }
    }

    /**
     * Resolves a paragraph's list bullet marker, or null when the paragraph is
     * not part of a bulleted list. Returns a renderable bullet (U+2022) that
     * the Latin fonts embed, replacing Word's private-use Wingdings/Symbol
     * bullet codepoints (e.g. \uF0B7) that standard text fonts cannot draw.
     */
    private static String paragraphBullet(XWPFParagraph paragraph) {
        BigInteger numId = paragraphNumberingId(paragraph);
        if (numId == null) {
            return null;
        }
        XWPFNumbering numbering = paragraph.getDocument().getNumbering();
        if (numbering == null) {
            return null;
        }
        BigInteger abstractId = numbering.getAbstractNumID(numId);
        if (abstractId == null) {
            return null;
        }
        XWPFAbstractNum abstractNum = numbering.getAbstractNum(abstractId);
        if (abstractNum == null || abstractNum.getCTAbstractNum() == null) {
            return null;
        }
        CTAbstractNum definition = abstractNum.getCTAbstractNum();
        if (definition.sizeOfLvlArray() == 0 || definition.getLvlArray(0).getNumFmt() == null
                || definition.getLvlArray(0).getLvlText() == null) {
            return null;
        }
        CTLvl level = definition.getLvlArray(0);
        String format = level.getNumFmt().getVal() == null
                ? null
                : level.getNumFmt().getVal().toString();
        if (!"bullet".equalsIgnoreCase(format)) {
            return null;
        }
        String text = level.getLvlText().getVal();
        if (text == null || text.isEmpty()) {
            return null;
        }
        return text.chars().anyMatch(cp -> cp >= 0xF000 && cp <= 0xF8FF)
                ? "\u2022"
                : text;
    }

    private static BigInteger paragraphNumberingId(XWPFParagraph paragraph) {
        if (paragraph.getCTP().isSetPPr() && paragraph.getCTP().getPPr().isSetNumPr()
                && paragraph.getCTP().getPPr().getNumPr().isSetNumId()) {
            return paragraph.getCTP().getPPr().getNumPr().getNumId().getVal();
        }
        if (paragraph.getDocument().getStyles() == null) {
            return null;
        }
        String styleId = paragraph.getStyle();
        if (styleId == null) {
            return null;
        }
        XWPFStyle style = paragraph.getDocument().getStyles().getStyle(styleId);
        if (style != null && style.getCTStyle() != null && style.getCTStyle().isSetPPr()
                && style.getCTStyle().getPPr().isSetNumPr()
                && style.getCTStyle().getPPr().getNumPr().isSetNumId()) {
            return style.getCTStyle().getPPr().getNumPr().getNumId().getVal();
        }
        return null;
    }

    private static float cellFontSize(XWPFTableCell cell, float fallback) {
        return cellParagraphs(cell).stream()
                .map(paragraph -> paragraphFontSize(paragraph, fallback))
                .max(Float::compare)
                .orElse(fallback);
    }

    private static PageMargins pageMargins(
            XWPFDocument document,
            float fallback,
            boolean landscape,
            boolean useWordParagraphLayout) {
        CTBody body = document.getDocument().getBody();
        if (!body.isSetSectPr() || !body.getSectPr().isSetPgMar()) {
            return new PageMargins(
                    fallback, fallback, fallback, useWordParagraphLayout ? 0.0f : 10.0f);
        }
        CTPageMar margins = body.getSectPr().getPgMar();
        float left = twipsValue(margins.getLeft(), fallback);
        if (!landscape && !useWordParagraphLayout) {
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
        // Mirror DocxReader.cs: only CJK line-grid types activate line snapping.
        // A plain w:docGrid (type omitted/default) must not inflate line heights.
        STDocGrid.Enum gridType = body.getSectPr().getDocGrid().getType();
        if (gridType == null
                || (!gridType.equals(STDocGrid.LINES)
                    && !gridType.equals(STDocGrid.LINES_AND_CHARS)
                    && !gridType.equals(STDocGrid.SNAP_TO_CHARS))) {
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
        float fontSize = footerFontSize(document);
        for (XWPFFooter footer : document.getFooterList()) {
            boolean hasPageField = descendantElementText(footer, "instrText").stream()
                    .map(String::trim)
                    .anyMatch(instruction -> PAGE_FIELD.matcher(instruction).find());
            if (!hasPageField) {
                return null;
            }
            String cachedText = footer.getText().trim();
            if (cachedText.isEmpty()) {
                cachedText = String.join("", descendantElementText(footer, "t")).trim();
            }
            Matcher cachedPage = CACHED_PAGE_NUMBER.matcher(cachedText);
            if (!cachedPage.matches()) {
                continue;
            }
                PageNumberFooter candidate = new PageNumberFooter(
                    cachedPage.group(1), cachedPage.group(3), fontSize);
            if (result != null && !result.equals(candidate)) {
                return null;
            }
            result = candidate;
        }
        return result == null ? new PageNumberFooter("", "", fontSize) : result;
    }

    private static float footerFontSize(XWPFDocument document) {
        if (document.getStyles() != null) {
            for (String styleId : new String[] {"FooterChar", "Footer", "Normal"}) {
                XWPFStyle style = document.getStyles().getStyle(styleId);
                if (style == null || !style.getCTStyle().isSetRPr()) {
                    continue;
                }
                CTRPr properties = style.getCTStyle().getRPr();
                if (properties.sizeOfSzArray() > 0) {
                    try {
                        return Float.parseFloat(properties.getSzArray(0).getVal().toString()) / 2.0f;
                    } catch (NumberFormatException ignored) {
                        // Try the next inherited style.
                    }
                }
            }
        }
        return 9.0f;
    }

    private static List<String> descendantElementText(XWPFFooter footer, String localName) {
        List<String> values = new ArrayList<>();
        try (XmlCursor cursor = footer._getHdrFtr().newCursor()) {
            while (cursor.toNextToken() != XmlCursor.TokenType.NONE) {
                if (cursor.isStart()
                        && cursor.getName() != null
                        && localName.equals(cursor.getName().getLocalPart())) {
                    values.add(cursor.getTextValue());
                }
            }
        }
        return values;
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
        return textWidth(font, text, fontSize, autoSpacing, false);
    }

    private static float textWidth(
            PDFont font,
            String text,
            float fontSize,
            AutoSpacing autoSpacing,
            boolean bold) throws IOException {
        // Mirrors DocxToPdfConverter.EstimateWrapTextWidth: wrap decisions use
        // calibrated per-character width tables, not the actual embedded font's
        // advances. Actual TTF advances drift from Word's metrics by a few
        // percent and collapse pagination over many lines.
        //
        // Path selection mirrors the .NET state flags:
        // - Calibri-default document: Calibri table x0.977, except serif runs
        //   which use the Times table with -8.8 units/letter kerning
        //   (s_serifRunInCalibri).
        // - Other defaults (e.g. Times New Roman): raw Helvetica table scaled
        //   down per the .NET latin-fraction reduction (serif default 10%,
        //   sans default 8%, bold 5%, CJK 27%).
        boolean calibriDefault = Boolean.TRUE.equals(CALIBRI_DEFAULT.get());
        if (calibriDefault) {
            return calibratedTextWidth(font, text, fontSize, autoSpacing, bold);
        }
        return helveticaTextWidth(font, text, fontSize, autoSpacing, bold);
    }

    private static float calibratedTextWidth(
            PDFont font,
            String text,
            float fontSize,
            AutoSpacing autoSpacing,
            boolean bold) throws IOException {
        boolean useTimesWidths = serifFont(font);
        int[] table = useTimesWidths ? TIMES_ROMAN_WIDTHS : CALIBRI_WIDTHS;
        float totalUnits = 0.0f;
        int kernable = 0;
        for (int offset = 0; offset < text.length();) {
            int codePoint = text.codePointAt(offset);
            offset += Character.charCount(codePoint);
            int width = calibratedCharWidth(codePoint, table);
            totalUnits += width;
            if (useTimesWidths && width != 1000 && isKernable(codePoint)) {
                kernable++;
            }
        }
        if (useTimesWidths) {
            // Approximate kerning/hinting: ~8.8 units per kernable letter/digit.
            totalUnits -= kernable * 8.8f;
        } else {
            totalUnits *= 0.977f;
        }
        if (bold) {
            totalUnits *= useTimesWidths ? 1.06f : 1.03f;
        }
        return totalUnits / 1000.0f * fontSize
                + eastAsianBoundaryCount(text, autoSpacing) * fontSize * 0.25f;
    }

    private static float helveticaTextWidth(
            PDFont font,
            String text,
            float fontSize,
            AutoSpacing autoSpacing,
            boolean bold) throws IOException {
        if (font instanceof PDType0Font) {
            // Word lays out with the same TTF advances as the embedded font
            // (Times New Roman etc.), so for real document fonts use the
            // actual metrics. The Helvetica table stays as a fallback for
            // Standard14 fonts (unit tests) where getStringWidth is still
            // usable but historically mismatched Word's Times metrics.
            float width = font.getStringWidth(text) / 1000.0f * fontSize;
            if (bold) {
                width *= serifFont(font) ? 1.06f : 1.03f;
            }
            return width + eastAsianBoundaryCount(text, autoSpacing) * fontSize * 0.25f;
        }
        boolean hasCjk = text.codePoints().anyMatch(PoiDocxRenderer::usesEastAsianFontSlot);
        float latinUnits = 0.0f;
        float totalUnits = 0.0f;
        for (int offset = 0; offset < text.length();) {
            int codePoint = text.codePointAt(offset);
            offset += Character.charCount(codePoint);
            int width = calibratedCharWidth(codePoint, HELVETICA_WIDTHS);
            float actual = hasCjk && codePoint == ' ' ? 500.0f : width;
            totalUnits += actual;
            if (!(width == 1000 && usesEastAsianFontSlot(codePoint)) && codePoint != '\u2009') {
                latinUnits += actual;
            }
        }
        if (latinUnits > 0.0f && totalUnits > 0.0f) {
            float latinFraction = latinUnits / totalUnits;
            float reduction = hasCjk
                    ? 0.27f
                    : bold ? 0.05f : Boolean.TRUE.equals(SERIF_DEFAULT.get()) ? 0.10f : 0.08f;
            totalUnits *= 1.0f - latinFraction * reduction;
        }
        return totalUnits / 1000.0f * fontSize
                + eastAsianBoundaryCount(text, autoSpacing) * fontSize * 0.25f;
    }

    private static boolean serifFamily(String family) {
        return family.contains("times")
                || family.contains("georgia")
                || family.contains("cambria")
                || family.contains("palatino")
                || family.contains("garamond");
    }

    private static int calibratedCharWidth(int codePoint, int[] table) {
        if (codePoint >= ' ' && codePoint <= '~') {
            return table[codePoint - ' '];
        }
        if (usesEastAsianFontSlot(codePoint)) {
            return 1000;
        }
        return 500;
    }

    private static boolean isKernable(int codePoint) {
        return codePoint >= 'A' && codePoint <= 'Z'
                || codePoint >= 'a' && codePoint <= 'z'
                || codePoint >= '0' && codePoint <= '9';
    }

    private static boolean serifFont(PDFont font) {
        String name = font.getName() == null ? "" : font.getName().toLowerCase();
        return name.contains("times") || name.contains("georgia") || name.contains("serif");
    }

    /**
     * Font ascent as a fraction of the font size. Word's first line on a page
     * sits on baseline = topMargin + fontSize * ascent (Times = 0.891), not
     * topMargin + fontSize. Falls back to 0.8 for fonts without a descriptor
     * (Standard14 unit-test fonts).
     */
    private static float fontAscentRatio(PDFont font) throws IOException {
        if (font.getFontDescriptor() != null) {
            return font.getFontDescriptor().getAscent() / 1000.0f;
        }
        return serifFont(font) ? 0.891f : 0.8f;
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

    static String renderableText(String text) {
        return Normalizer.normalize(
                text.replace('\r', ' ').replace('\n', ' ').replace('\t', ' '),
                Normalizer.Form.NFC);
    }

    private static float gridLineHeight(float naturalHeight, float linePitch) {
        if (linePitch <= 0.0f) {
            return naturalHeight;
        }
        return (float) Math.ceil(naturalHeight / linePitch) * linePitch;
    }

    static float paragraphLineHeight(
            XWPFParagraph paragraph,
            float fontSize,
            float naturalHeight,
            float linePitch,
            boolean useWordParagraphLayout) {
        return paragraphLineHeight(
                paragraph, fontSize, naturalHeight, linePitch, useWordParagraphLayout, false);
    }

    static float paragraphLineHeight(
            XWPFParagraph paragraph,
            float fontSize,
            float naturalHeight,
            float linePitch,
            boolean useWordParagraphLayout,
            boolean symbolBullet) {
        double spacing = useWordParagraphLayout
                ? effectiveSpacing(paragraph).line()
                : paragraph.getSpacingBetween();
        LineSpacingRule rule = useWordParagraphLayout
                ? effectiveSpacing(paragraph).rule()
                : paragraph.getSpacingLineRule();
        if (useWordParagraphLayout && spacing >= 0.0) {
            if (rule == LineSpacingRule.EXACT) {
                return (float) spacing;
            }
            if (rule == LineSpacingRule.AT_LEAST) {
                return Math.max(naturalHeight, (float) spacing);
            }
            // Word computes line height from the tallest font in the line.
            // Bullet list labels render in Symbol whose metrics are taller
            // than Times (verified against Word and LibreOffice: 12pt bullets
            // advance 16.7pt vs 15.9pt for plain body lines).
            float metricsFactor = symbolBullet ? 1.21f : 1.151f;
            return fontSize * metricsFactor * (float) spacing;
        }
        return gridLineHeight(naturalHeight, linePitch);
    }

    /**
     * Resolves a paragraph's effective spacing properties the way Word does:
     * direct pPr wins, then the paragraph style, then Normal, then docDefaults.
     * Each property (line / lineRule / before / after) falls back independently.
     */
    private static ParagraphSpacing effectiveSpacing(XWPFParagraph paragraph) {
        CTSpacing direct = paragraph.getCTP().isSetPPr()
                && paragraph.getCTP().getPPr().isSetSpacing()
                ? paragraph.getCTP().getPPr().getSpacing()
                : null;
        CTSpacing styleSpacing = null;
        CTSpacing normalSpacing = null;
        CTSpacing defaultSpacing = null;
        XWPFStyles documentStyles = paragraph.getDocument().getStyles();
        if (documentStyles != null) {
            String styleId = paragraph.getStyle();
            XWPFStyle style = styleId == null ? null : documentStyles.getStyle(styleId);
            if (style != null && style.getCTStyle() != null
                    && style.getCTStyle().isSetPPr()
                    && style.getCTStyle().getPPr().isSetSpacing()) {
                styleSpacing = style.getCTStyle().getPPr().getSpacing();
            }
            XWPFStyle normal = documentStyles.getStyle("Normal");
            if (normal != null && normal.getCTStyle() != null
                    && normal.getCTStyle().isSetPPr()
                    && normal.getCTStyle().getPPr().isSetSpacing()) {
                normalSpacing = normal.getCTStyle().getPPr().getSpacing();
            }
            CTStyles ctStyles = documentStyles.getCtStyles();
            if (ctStyles != null && ctStyles.isSetDocDefaults()
                    && ctStyles.getDocDefaults().isSetPPrDefault()
                    && ctStyles.getDocDefaults().getPPrDefault().isSetPPr()
                    && ctStyles.getDocDefaults().getPPrDefault().getPPr().isSetSpacing()) {
                defaultSpacing = ctStyles.getDocDefaults().getPPrDefault().getPPr().getSpacing();
            }
        }
        CTSpacing[] chain = {direct, styleSpacing, normalSpacing, defaultSpacing};
        return new ParagraphSpacing(
                resolveLine(chain),
                resolveLineRule(chain),
                resolveBefore(chain),
                resolveAfter(chain));
    }

    private static double resolveLine(CTSpacing[] chain) {
        for (CTSpacing spacing : chain) {
            if (spacing == null || !spacing.isSetLine()) {
                continue;
            }
            double value = Double.parseDouble(String.valueOf(spacing.getLine()));
            if (spacing.isSetLineRule() && spacing.getLineRule() == STLineSpacingRule.AUTO) {
                return value / 240.0;
            }
            return value / 20.0;
        }
        return -1.0;
    }

    private static LineSpacingRule resolveLineRule(CTSpacing[] chain) {
        for (CTSpacing spacing : chain) {
            if (spacing == null || !spacing.isSetLineRule()) {
                continue;
            }
            if (spacing.getLineRule() == STLineSpacingRule.EXACT) {
                return LineSpacingRule.EXACT;
            }
            if (spacing.getLineRule() == STLineSpacingRule.AT_LEAST) {
                return LineSpacingRule.AT_LEAST;
            }
            return LineSpacingRule.AUTO;
        }
        return null;
    }

    private static int resolveBefore(CTSpacing[] chain) {
        for (CTSpacing spacing : chain) {
            if (spacing != null && spacing.isSetBefore()) {
                return Integer.parseInt(String.valueOf(spacing.getBefore()));
            }
        }
        return -1;
    }

    private static int resolveAfter(CTSpacing[] chain) {
        for (CTSpacing spacing : chain) {
            if (spacing != null && spacing.isSetAfter()) {
                return Integer.parseInt(String.valueOf(spacing.getAfter()));
            }
        }
        return -1;
    }

    private static final class ParagraphSpacing {
        private final double line;
        private final LineSpacingRule rule;
        private final int before;
        private final int after;

        private ParagraphSpacing(double line, LineSpacingRule rule, int before, int after) {
            this.line = line;
            this.rule = rule;
            this.before = before;
            this.after = after;
        }

        private double line() {
            return line;
        }

        private LineSpacingRule rule() {
            return rule;
        }

        private int before() {
            return before;
        }

        private int after() {
            return after;
        }
    }

    static float centeredTextX(float pageWidth, float leftIndent, float rightIndent, float textWidth) {
        return (pageWidth + leftIndent - rightIndent - textWidth) / 2.0f;
    }

    private static float stylePoints(String xml, String property) {
        Matcher matcher = Pattern.compile(property + ":(-?[0-9.]+)pt").matcher(xml);
        return matcher.find() ? Float.parseFloat(matcher.group(1)) : Float.NaN;
    }

    private static void drawRunHighlight(
            PDPageContentStream content,
            HighlightColor highlight,
            float x,
            float baseline,
            float width,
            float fontSize) throws IOException {
        if (highlight == null || width <= 0.0f) {
            return;
        }
        float horizontalPadding = Math.max(0.7f, fontSize * 0.08f);
        content.setNonStrokingColor(highlight.red, highlight.green, highlight.blue);
        content.addRect(
                x - horizontalPadding,
                baseline - fontSize * 0.24f,
                width + horizontalPadding * 2.0f,
                fontSize * 1.18f);
        content.fill();
        content.setNonStrokingColor(0.0f, 0.0f, 0.0f);
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
        showText(content, font, fontSize, text, x, y, autoSpacing, 0.0f);
    }

    private static void showText(
            PDPageContentStream content,
            PDFont font,
            float fontSize,
            String text,
            float x,
            float y,
            AutoSpacing autoSpacing,
            float wordSpacing) throws IOException {
        if (text.isEmpty()) {
            return;
        }
        content.beginText();
        content.setFont(font, fontSize);
        if (wordSpacing > 0.0f) {
            content.setWordSpacing(wordSpacing);
        }
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

    static float justifiedWordSpacing(String text, float textWidth, float availableWidth) {
        long spaces = text.codePoints().filter(codePoint -> codePoint == ' ').count();
        return spaces == 0 ? 0.0f : Math.max(0.0f, availableWidth - textWidth) / spaces;
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
        private final Map<String, FootnoteData> footnotes;
        private final Map<String, String> themeColors;
        private final List<String> currentPageFootnoteIds = new ArrayList<>();
        private PDPageContentStream content;
        private float y;
        private int pageCount;
        private float previousParagraphSpacingAfter;
        private float footnoteReservedHeight;
        private boolean alignNextParagraphToCheckboxes;
        private boolean topOfPage;

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
                float pageFooterDistance,
                Map<String, FootnoteData> footnotes,
                Map<String, String> themeColors)
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
            this.footnotes = footnotes;
            this.themeColors = themeColors;
            newPage(topOffset);
        }

        private void ensureSpace(float height) throws IOException {
            if (y - height < bottomMargin + footnoteReservedHeight) {
                newPage();
            }
        }

        private void registerFootnotes(XWPFParagraph paragraph) throws IOException {
            for (XWPFRun run : paragraph.getRuns()) {
                run.getCTR().getFootnoteReferenceList().forEach(reference -> {
                    String id = reference.getId().toString();
                    if (footnotes.containsKey(id) && !currentPageFootnoteIds.contains(id)) {
                        currentPageFootnoteIds.add(id);
                        FootnoteData footnote = footnotes.get(id);
                        if (footnoteReservedHeight == 0.0f) {
                            footnoteReservedHeight = 17.0f;
                        }
                        footnoteReservedHeight += footnote.lineHeight() + 8.0f;
                    }
                });
            }
        }

        private void expectCheckboxLabelAlignment() {
            alignNextParagraphToCheckboxes = true;
        }

        private boolean consumeCheckboxLabelAlignment() {
            boolean align = alignNextParagraphToCheckboxes;
            alignNextParagraphToCheckboxes = false;
            return align;
        }

        private void moveDown(float amount) throws IOException {
            ensureSpace(amount);
            y -= amount;
            topOfPage = false;
        }

        private boolean consumeTopOfPage() {
            boolean value = topOfPage;
            topOfPage = false;
            return value;
        }

        private void avoidWidowOrphanSplit(float lineHeight, int lineCount) throws IOException {
            if (lineCount < 2 || lineHeight <= 0.0f) {
                return;
            }
            int linesThatFit = (int) Math.floor((y - bottomMargin) / lineHeight);
            if (linesThatFit > 0
                    && linesThatFit < lineCount
                    && (linesThatFit < 2 || lineCount - linesThatFit < 2)) {
                newPage();
            }
        }

        private void moveToParagraph(float spacingBefore) throws IOException {
            moveDown(Math.max(0.0f, spacingBefore - previousParagraphSpacingAfter));
            previousParagraphSpacingAfter = 0.0f;
        }

        private void finishParagraph(float spacingAfter) throws IOException {
            moveDown(spacingAfter);
            previousParagraphSpacingAfter = spacingAfter;
        }

        private void resetParagraphSpacingAfter() {
            previousParagraphSpacingAfter = 0.0f;
        }

        private void keepNextBeforeParagraph(float needed) throws IOException {
            if (y < pageSize.height() - topMargin && y - needed < bottomMargin) {
                newPage();
            }
        }

        private void newPage() throws IOException {
            newPage(0.0f);
        }

        private void newPage(float topOffset) throws IOException {
            if (content != null) {
                renderFootnotes();
                renderPageNumber();
                content.close();
            }
            currentPageFootnoteIds.clear();
            footnoteReservedHeight = 0.0f;
            previousParagraphSpacingAfter = 0.0f;
            PDPage page = new PDPage(new PDRectangle(pageSize.width(), pageSize.height()));
            document.addPage(page);
            pageCount++;
            content = new PDPageContentStream(document, page);
            y = pageSize.height() - topMargin - topOffset;
            topOfPage = true;
        }

        private void renderFootnotes() throws IOException {
            if (currentPageFootnoteIds.isEmpty()) {
                return;
            }
            float textHeight = currentPageFootnoteIds.stream()
                    .map(footnotes::get)
                    .map(FootnoteData::lineHeight)
                    .reduce(0.0f, Float::sum);
            float footnoteY = bottomMargin + textHeight + 5.0f;
            content.setStrokingColor(0, 0, 0);
            content.setLineWidth(0.5f);
            content.moveTo(margin, footnoteY);
            content.lineTo(margin + Math.min(144.0f, (pageSize.width() - margin * 2.0f) * 0.33f), footnoteY);
            content.stroke();
            footnoteY -= 2.0f;
            for (String id : currentPageFootnoteIds) {
                FootnoteData footnote = footnotes.get(id);
                footnoteY -= footnote.lineHeight();
                showText(
                        content,
                        footnote.font,
                        footnote.fontSize,
                        footnote.displayText(),
                        margin,
                        footnoteY);
            }
        }

        private void renderPageNumber() throws IOException {
            if (pageNumberFont == null) {
                return;
            }
            String pageNumber = pageNumberFooter.prefix() + pageCount + pageNumberFooter.suffix();
                float fontSize = pageNumberFooter.fontSize();
            float x = (pageSize.width() - textWidth(pageNumberFont, pageNumber, fontSize)) / 2.0f;
                float descent = pageNumberFont.getFontDescriptor() == null
                    ? 0.0f
                    : pageNumberFont.getFontDescriptor().getDescent();
                float baseline = pageNumberBaseline(pageFooterDistance, fontSize, descent);
                showText(content, pageNumberFont, fontSize, pageNumber, x, baseline);
        }

        @Override
        public void close() throws IOException {
            if (content != null) {
                renderFootnotes();
                renderPageNumber();
                content.close();
                content = null;
            }
        }
    }

    static float pageNumberBaseline(float footerDistance, float fontSize, float fontDescent) {
        return footerDistance + fontSize * 1.151f + Math.abs(fontDescent) / 1000.0f * fontSize;
    }
}