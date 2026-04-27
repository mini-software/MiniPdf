namespace MiniSoftware;

/// <summary>
/// Converts Word (.docx) files to PDF documents.
/// Renders paragraphs, tables, and images using the built-in Helvetica font.
/// </summary>
internal static class DocxToPdfConverter
{
    // Single-spaced line height ≈ fontSize × this factor (ascent + descent for typical fonts)
    private const float FontMetricsFactor = 1.17f;
    // Times New Roman / Georgia metric factor: these serif fonts have smaller ascent/descent
    // ratios than Calibri, so the standard factor over-estimates line height.
    // TNR: (usWinAscent+usWinDescent)/unitsPerEm ≈ 1.107
    private const float FontMetricsFactorTimesNewRoman = 1.098f;
    // CJK font metric factor: East Asian fonts (SimSun, PMingLiU, MS Gothic, etc.)
    // have taller ascent+descent.  Measured from LibreOffice reference:
    // 12pt SimSun single-spaced ≈ 15.5pt line height → 1.29 × fontSize.
    // Only applied when paragraph has NO explicit w:line (auto spacing fallback).
    private const float FontMetricsFactorCJK = 1.29f;
    // Helvetica ascent ratio: visual top of text is baseline + fontSize × AscentRatio
    private const float AscentRatio = 1.075f;
    // Calibri-to-Helvetica width ratio: most DOCX documents use Calibri (default since Word 2007).
    // Used only as a fallback; per-character Calibri widths are used when available.
    private const float CalibriWidthScale = 0.87f;

    // Per-conversion flag: true when the document's default font is a serif font
    // (Times New Roman, Georgia, etc.), which has narrower Latin glyphs than Helvetica.
    // Used to apply a stronger width reduction in EstimateWrapTextWidth.
    [ThreadStatic] private static bool s_serifFont;

    // Per-paragraph/cell flag: set when the run font is a serif font (Times New Roman,
    // Georgia, etc.) inside a document whose default font is Calibri. Calibri Latin
    // glyphs are noticeably wider than serif glyphs (e.g. Calibri 'a'=507, 'e'=515 vs
    // Times 'a'=444, 'e'=444), so the Calibri-width estimate is ~5% too wide for serif
    // runs. Without this correction WordWrap wraps trailing words (e.g. "publication.")
    // to a new line that Word fits comfortably on the previous line.
    [ThreadStatic] private static bool s_serifRunInCalibri;

    // Per-paragraph override: when set, WordWrap uses these character widths instead of
    // Helvetica/Calibri metrics. Used for fonts with significantly different glyph widths
    // (e.g. Century Gothic) where Helvetica is a poor proxy.
    [ThreadStatic] private static int[]? s_overrideWidths;

    // Per-paragraph flag: set when the run font is a wide sans-serif (Franklin Gothic,
    // etc.) whose Latin glyphs are close to Helvetica width. In that case the default
    // 8% Latin reduction in EstimateWrapTextWidth produces too-narrow estimates,
    // causing WordWrap to over-pack lines that then trigger Tz compression at render.
    [ThreadStatic] private static bool s_wideSansSerifFont;

    /// <summary>
    /// Options for controlling DOCX-to-PDF conversion.
    /// </summary>
    internal sealed class ConversionOptions
    {
        /// <summary>Default font size in points (default: 11).</summary>
        public float FontSize { get; set; } = 11;

        /// <summary>Page left margin in points (default: 72 = 1 inch).</summary>
        public float MarginLeft { get; set; } = 72;

        /// <summary>Page top margin in points (default: 72 = 1 inch).</summary>
        public float MarginTop { get; set; } = 72;

        /// <summary>Page right margin in points (default: 72 = 1 inch).</summary>
        public float MarginRight { get; set; } = 72;

        /// <summary>Page bottom margin in points (default: 72 = 1 inch).</summary>
        public float MarginBottom { get; set; } = 72;

        /// <summary>Line spacing multiplier (default: 1.15).</summary>
        public float LineSpacing { get; set; } = 1.15f;

        /// <summary>Page width in points (default: 612 = US Letter).</summary>
        public float PageWidth { get; set; } = 612;

        /// <summary>Page height in points (default: 792 = US Letter).</summary>
        public float PageHeight { get; set; } = 792;

        /// <summary>Document grid line pitch in points (0 = no grid).</summary>
        public float GridLinePitch { get; set; }

        /// <summary>Header margin in points (distance from page top to header).</summary>
        public float HeaderMargin { get; set; } = 36;

        /// <summary>Footer margin in points (distance from page bottom to footer).</summary>
        public float FooterMargin { get; set; } = 36;

        /// <summary>Use Calibri widths for word-wrap layout (true for Calibri-based docs, false for Arial/Helvetica-based).</summary>
        public bool UseCalibriWidths { get; set; } = true;
    }

    /// <summary>
    /// Converts a DOCX file to a PDF document.
    /// </summary>
    internal static PdfDocument Convert(string docxPath, ConversionOptions? options = null)
    {
        using var stream = File.OpenRead(docxPath);
        return Convert(stream, options);
    }

    /// <summary>
    /// Converts a DOCX stream to a PDF document.
    /// </summary>
    internal static PdfDocument Convert(Stream docxStream, ConversionOptions? options = null)
    {
        options ??= new ConversionOptions();
        var docxDoc = DocxReader.Read(docxStream);

        // Apply page layout from DOCX if available
        if (docxDoc.PageLayout is { } layout)
        {
            options.PageWidth = layout.PageWidth;
            options.PageHeight = layout.PageHeight;
            options.MarginTop = layout.MarginTop;
            options.MarginBottom = layout.MarginBottom;
            options.MarginLeft = layout.MarginLeft;
            options.MarginRight = layout.MarginRight;
            options.GridLinePitch = layout.GridLinePitch;
            options.HeaderMargin = layout.HeaderMargin;
            options.FooterMargin = layout.FooterMargin;
        }

        // Apply document default line spacing from styles.xml docDefaults
        if (docxDoc.DefaultLineSpacing > 0 && !docxDoc.DefaultLineSpacingAbsolute)
            options.LineSpacing = docxDoc.DefaultLineSpacing;

        // Choose wrap metrics from the effective document default font.
        // Many Word docs use theme fonts (e.g., minorHAnsi -> Cambria), so always using
        // Calibri widths can under-wrap and reduce page count.
        if (!string.IsNullOrWhiteSpace(docxDoc.DefaultFontName))
        {
            options.UseCalibriWidths = docxDoc.DefaultFontName.Contains("Calibri", StringComparison.OrdinalIgnoreCase);
            s_serifFont = IsSerifFont(docxDoc.DefaultFontName);
        }
        else
        {
            s_serifFont = false;
        }

        var pdfDoc = new PdfDocument();

        // Pass preferred CJK font from DOCX to PDF writer for correct font selection
        if (!string.IsNullOrWhiteSpace(docxDoc.DefaultEastAsiaFontName))
            pdfDoc.PreferredCjkFontName = docxDoc.DefaultEastAsiaFontName;

        // Pre-scan section breaks to build correct section layout mapping.
        // In OOXML, sectPr in a paragraph defines the layout of the section ENDING at that paragraph.
        // We collect layouts in order: [section1_layout, section2_layout, ...body_layout]
        // Then section N uses sectionLayouts[N] and when we hit break N, we switch to sectionLayouts[N+1].
        var sectionLayouts = new List<DocxPageLayout>();
        foreach (var element in docxDoc.Elements)
        {
            if (element is DocxParagraph p && p.SectionBreak is { } sb)
                sectionLayouts.Add(sb);
        }
        if (docxDoc.PageLayout is { } bodyLayout2)
            sectionLayouts.Add(bodyLayout2);

        // Apply first section's layout (or body layout as fallback)
        if (sectionLayouts.Count > 0)
        {
            var firstLayout = sectionLayouts[0];
            options.PageWidth = firstLayout.PageWidth;
            options.PageHeight = firstLayout.PageHeight;
            options.MarginTop = firstLayout.MarginTop;
            options.MarginBottom = firstLayout.MarginBottom;
            options.MarginLeft = firstLayout.MarginLeft;
            options.MarginRight = firstLayout.MarginRight;
            options.GridLinePitch = firstLayout.GridLinePitch;
            options.HeaderMargin = firstLayout.HeaderMargin;
            options.FooterMargin = firstLayout.FooterMargin;
        }

        // Pre-process: apply contextualSpacing rules — suppress spacing between
        // same-style consecutive paragraphs when either has contextualSpacing set.
        var processedElements = new List<DocxElement>(docxDoc.Elements.Count);
        for (int i = 0; i < docxDoc.Elements.Count; i++)
        {
            var element = docxDoc.Elements[i];
            if (element is DocxParagraph currPara
                && i + 1 < docxDoc.Elements.Count
                && docxDoc.Elements[i + 1] is DocxParagraph nextPara
                && (currPara.ContextualSpacing || nextPara.ContextualSpacing)
                && !string.IsNullOrEmpty(currPara.StyleId)
                && currPara.StyleId == nextPara.StyleId
                && currPara.SpacingAfter > 0)
            {
                element = currPara with { SpacingAfter = 0 };
            }
            processedElements.Add(element);
        }

        var state = new RenderState(pdfDoc, options);

        // Adjust top margin when header content is taller than the default header area
        if (docxDoc.HeaderElements is { Count: > 0 })
        {
            var headerContentHeight = EstimateElementsHeight(TrimTrailingEmptyParagraphs(docxDoc.HeaderElements), options);
            var headerAreaHeight = options.MarginTop - options.HeaderMargin;
            if (headerAreaHeight < 0) headerAreaHeight = 0; // header starts below marginTop
            if (headerContentHeight > headerAreaHeight)
            {
                // Preserve a reasonable gap between header area and body content.
                // When headerMargin > marginTop (header below nominal body start),
                // use half the original top margin as a gap, which approximates Word behavior.
                var gap = headerAreaHeight > 0 ? headerAreaHeight : Math.Max(6f, options.MarginTop / 2f);
                options.MarginTop = options.HeaderMargin + headerContentHeight + gap;
            }
        }
        // Adjust bottom margin when footer content is taller than the default footer area
        if (docxDoc.FooterElements is { Count: > 0 })
        {
            var footerContentHeight = EstimateElementsHeight(TrimTrailingEmptyParagraphs(docxDoc.FooterElements), options);
            var footerTopFromBottom = options.FooterMargin + footerContentHeight;
            if (footerTopFromBottom > options.MarginBottom)
                options.MarginBottom = footerTopFromBottom;
        }

        state.EnsurePage();
        // Record section 0 start page
        state.SectionStartPages.Add(0);

        // Initialize footnote support
        state.Footnotes = docxDoc.Footnotes;
        state.BaseMarginBottom = options.MarginBottom;

        var sectionIndex = 0;
        for (int elemIdx = 0; elemIdx < processedElements.Count; elemIdx++)
        {
            var element = processedElements[elemIdx];
            switch (element)
            {
                case DocxParagraph paragraph:
                    // keepNext: prevent orphaned heading at bottom of page.
                    // If this paragraph has keepNext and there is a following paragraph,
                    // ensure there is room for the heading + at least 2 body lines.
                    if (paragraph.KeepNext && elemIdx + 1 < processedElements.Count
                        && processedElements[elemIdx + 1] is DocxParagraph followPara
                        && state.CurrentPage != null)
                    {
                        var thisH = EstimateElementsHeight(new System.Collections.Generic.List<DocxElement> { paragraph }, state.Options);
                        var spacingBeforeH = paragraph.SpacingBefore > 0 ? paragraph.SpacingBefore : 0f;
                        var followFontSz = followPara.FontSize > 0 ? followPara.FontSize : state.Options.FontSize;
                        var followLineH = followFontSz * GetFontMetricsFactor(followPara.ParagraphFontName) *
                            (followPara.LineSpacing > 0 ? followPara.LineSpacing : state.Options.LineSpacing);
                        // Require space for heading + 2 follow lines (widow/orphan control)
                        var needed = spacingBeforeH + thisH + 2f * followLineH;
                        if (state.CurrentY - needed < state.Options.MarginBottom)
                            state.ForceNewPage();
                    }

                    RenderParagraph(state, paragraph);
                    if (paragraph.FloatingTextBoxes is { Count: > 0 })
                        RenderFloatingTextBoxes(state, paragraph.FloatingTextBoxes, state.LastParagraphStartY);
                    if (paragraph.ConnectorLines is { Count: > 0 })
                        RenderConnectorLines(state, paragraph.ConnectorLines, state.LastParagraphStartY);
                    if (paragraph.SectionBreak != null)
                    {
                        sectionIndex++;
                        if (sectionIndex < sectionLayouts.Count)
                        {
                            var nextLayout = sectionLayouts[sectionIndex];

                            // Exit multi-column mode before applying new layout
                            if (state.ColumnCount > 1)
                                state.ExitMultiColumnSection();

                            state.Options.PageWidth = nextLayout.PageWidth;
                            state.Options.PageHeight = nextLayout.PageHeight;
                            state.Options.MarginTop = nextLayout.MarginTop;
                            state.Options.MarginBottom = nextLayout.MarginBottom;
                            state.Options.MarginLeft = nextLayout.MarginLeft;
                            state.Options.MarginRight = nextLayout.MarginRight;
                            state.Options.GridLinePitch = nextLayout.GridLinePitch;
                            state.Options.HeaderMargin = nextLayout.HeaderMargin;
                            state.Options.FooterMargin = nextLayout.FooterMargin;

                            // Continuous sections don't force a new page
                            if (nextLayout.SectionType == "continuous")
                            {
                                // Enter multi-column if applicable
                                if (nextLayout.ColumnCount > 1)
                                    state.EnterMultiColumnSection(nextLayout.ColumnCount, nextLayout.ColumnSpacing, nextLayout.ColumnWidths, nextLayout.ColumnGaps);
                            }
                            else
                            {
                                // At hard section breaks, clear any accumulated PendingVerticalSpace
                                // from the closing section so it does not shift the next section's
                                // first content downward on the new page.
                                state.PendingVerticalSpace = 0;
                                state.ForceNewPage();
                                if (nextLayout.ColumnCount > 1)
                                    state.EnterMultiColumnSection(nextLayout.ColumnCount, nextLayout.ColumnSpacing, nextLayout.ColumnWidths, nextLayout.ColumnGaps);
                            }
                        }
                        else
                        {
                            if (state.ColumnCount > 1)
                                state.ExitMultiColumnSection();
                            state.PendingVerticalSpace = 0;
                            state.ForceNewPage();
                        }
                        // Record new section's start page
                        state.SectionStartPages.Add(pdfDoc.Pages.Count - 1);
                        state.CurrentSectionIndex = sectionIndex;
                    }
                    break;
                case DocxTable table:
                    RenderTable(state, table);
                    break;
            }
        }

        // Ensure at least one page exists
        if (pdfDoc.Pages.Count == 0)
            pdfDoc.AddPage(options.PageWidth, options.PageHeight);

        // Render behindDoc images.
        // Page-relative behindDoc images act as watermarks and repeat on all pages.
        // Other behindDoc images render only on their anchor page.
        if (state.BehindDocImagesPerPage.Count > 0)
        {
            const float emuPerPt = 914400f / 72f;
            foreach (var (_, entries) in state.BehindDocImagesPerPage)
            {
                foreach (var (img, anchorY) in entries)
                {
                    var fmt = img.Extension;
                    if (fmt != "jpg" && fmt != "png") continue;
                    var w = img.WidthEmu > 0 ? img.WidthEmu / emuPerPt : 200f;
                    var h = img.HeightEmu > 0 ? img.HeightEmu / emuPerPt : 150f;
                    var ax = img.RelativeFromH == "page"
                        ? img.OffsetXEmu / emuPerPt
                        : options.MarginLeft + img.OffsetXEmu / emuPerPt;
                    float ay;
                    if (img.RelativeFromV == "page")
                        ay = options.PageHeight - img.OffsetYEmu / emuPerPt;
                    else if (img.RelativeFromV == "paragraph")
                        ay = anchorY - img.OffsetYEmu / emuPerPt;
                    else
                        ay = options.PageHeight - options.MarginTop - img.OffsetYEmu / emuPerPt;
                    var isWatermark = img.RelativeFromH == "page" && img.RelativeFromV == "page";
                    if (isWatermark)
                    {
                        // Render on all pages as a repeating watermark
                        for (int pi = 0; pi < pdfDoc.Pages.Count; pi++)
                            pdfDoc.Pages[pi].AddImage(img.Data, fmt, ax, ay - h, w, h, img.Alpha);
                    }
                    else
                    {
                        // Render only on the anchor page — find it from the dictionary key
                        foreach (var (pageIdx2, entries2) in state.BehindDocImagesPerPage)
                        {
                            if (entries2.Any(e => e.Image == img) && pageIdx2 >= 0 && pageIdx2 < pdfDoc.Pages.Count)
                            {
                                pdfDoc.Pages[pageIdx2].AddImage(img.Data, fmt, ax, ay - h, w, h, img.Alpha);
                                break;
                            }
                        }
                    }
                }
            }
        }

        // Render header/footer background shapes on all pages.
        if (docxDoc.HeaderShapes is { Count: > 0 } || docxDoc.FooterShapes is { Count: > 0 }
            || docxDoc.HeaderFooterImages is { Count: > 0 })
        {
            var totalPages = pdfDoc.Pages.Count;
            for (int pi = 0; pi < totalPages; pi++)
            {
                var page = pdfDoc.Pages[pi];

                if (docxDoc.HeaderShapes is { Count: > 0 })
                {
                    foreach (var shape in docxDoc.HeaderShapes)
                        RenderHeaderFooterShape(page, options, shape);
                }

                if (docxDoc.FooterShapes is { Count: > 0 })
                {
                    foreach (var shape in docxDoc.FooterShapes)
                        RenderHeaderFooterShape(page, options, shape);
                }

                if (docxDoc.HeaderFooterImages is { Count: > 0 })
                {
                    foreach (var img in docxDoc.HeaderFooterImages)
                        RenderHeaderFooterImage(page, options, img);
                }
            }
        }

        // Render footnotes on the last page
        state.RenderPageFootnotes();

        // Render headers and footers text on all pages
        // When full elements are available, use them; otherwise fall back to text-only rendering.
        // Footer/header elements that are all empty (e.g., content is in text boxes) are treated as absent.
        var hasHeaderElements = docxDoc.HeaderElements is { Count: > 0 }
            && docxDoc.HeaderElements.Any(e => e is DocxTable
                || (e is DocxParagraph p && (p.Runs.Any(r => !string.IsNullOrWhiteSpace(r.Text)) || p.Images.Count > 0)));
        var hasFooterElements = docxDoc.FooterElements is { Count: > 0 }
            && docxDoc.FooterElements.Any(e => e is DocxTable
                || (e is DocxParagraph p && (p.Runs.Any(r => !string.IsNullOrWhiteSpace(r.Text)) || p.Images.Count > 0)));

        // Check for per-section footer elements
        var hasSectionFooters = docxDoc.SectionFooterElements is { Count: > 0 }
            && docxDoc.SectionFooterElements.Any(f => f is { Count: > 0 });

        if (hasHeaderElements || hasFooterElements || hasSectionFooters)
        {
            var totalPages = pdfDoc.Pages.Count;
            for (int pi = 0; pi < totalPages; pi++)
            {
                var page = pdfDoc.Pages[pi];
                if (hasHeaderElements)
                {
                    var headerStartY = page.Height - options.HeaderMargin;
                    RenderHeaderFooterElementsOnPage(page, options, docxDoc.HeaderElements!, headerStartY, pi, totalPages);
                }

                // Determine footer elements for this page: prefer per-section, fall back to global
                List<DocxElement>? pageFooterElements = null;
                int sectionPageNum = -1;
                if (hasSectionFooters)
                {
                    // Find which section this page belongs to
                    int pageSectionIdx = 0;
                    for (int si = state.SectionStartPages.Count - 1; si >= 0; si--)
                    {
                        if (pi >= state.SectionStartPages[si])
                        {
                            pageSectionIdx = si;
                            break;
                        }
                    }
                    if (pageSectionIdx < docxDoc.SectionFooterElements!.Count)
                        pageFooterElements = docxDoc.SectionFooterElements[pageSectionIdx];

                    // Calculate section-relative page number using PageNumStart
                    // Walk backward to find nearest ancestor section with PageNumStart
                    int ancestorIdx = pageSectionIdx;
                    while (ancestorIdx > 0 && (ancestorIdx >= sectionLayouts.Count || sectionLayouts[ancestorIdx].PageNumStart < 0))
                        ancestorIdx--;
                    if (ancestorIdx >= 0 && ancestorIdx < sectionLayouts.Count && sectionLayouts[ancestorIdx].PageNumStart >= 0)
                    {
                        int pagesFromAncestor = pi - state.SectionStartPages[ancestorIdx];
                        sectionPageNum = sectionLayouts[ancestorIdx].PageNumStart + pagesFromAncestor;
                    }
                }
                // Fall back to global footer if no section-specific footer
                if (pageFooterElements == null && hasFooterElements)
                    pageFooterElements = docxDoc.FooterElements;

                if (pageFooterElements is { Count: > 0 }
                    && pageFooterElements.Any(e => e is DocxTable
                        || (e is DocxParagraph pp && (pp.Runs.Any(r => !string.IsNullOrWhiteSpace(r.Text)) || pp.Images.Count > 0))))
                {
                    var trimmedFooter = TrimTrailingEmptyParagraphs(pageFooterElements);
                    var footerContentHeight = EstimateElementsHeight(trimmedFooter, options);
                    var footerStartY = options.FooterMargin + footerContentHeight;
                    RenderHeaderFooterElementsOnPage(page, options, trimmedFooter, footerStartY, pi, totalPages, sectionPageNum);
                }
            }
        }
        if ((!hasHeaderElements && docxDoc.HeaderText != null) || (!hasFooterElements && docxDoc.FooterText != null))
        {
            const float headerFooterFontSize = 9f;
            var headerColor = PdfColor.FromRgb(128, 128, 128);
            var totalPages = pdfDoc.Pages.Count;
            for (int pi = 0; pi < totalPages; pi++)
            {
                var page = pdfDoc.Pages[pi];
                var usableW = page.Width - options.MarginLeft - options.MarginRight;
                if (!hasHeaderElements && docxDoc.HeaderText != null)
                {
                    RenderHeaderFooterRuns(page, options, docxDoc.HeaderRuns, docxDoc.HeaderText,
                        pi, totalPages, headerFooterFontSize, headerColor, usableW,
                        page.Height - options.HeaderMargin);
                }
                if (!hasFooterElements && docxDoc.FooterText != null)
                {
                    RenderHeaderFooterRuns(page, options, docxDoc.FooterRuns, docxDoc.FooterText,
                        pi, totalPages, headerFooterFontSize, headerColor, usableW,
                        options.FooterMargin);
                }
            }
        }

        return pdfDoc;
    }

    /// <summary>
    /// Renders header/footer text with per-run bold formatting.
    /// Falls back to plain text if runs are not available.
    /// </summary>
    private static void RenderHeaderFooterRuns(PdfPage page, ConversionOptions options,
        List<DocxRun>? runs, string plainText,
        int pageIndex, int totalPages, float fontSize, PdfColor color, float usableW, float y)
    {
        if (runs != null && runs.Count > 0)
        {
            // Resolve placeholders in each run and compute total width
            var resolvedRuns = new List<(string Text, bool Bold, bool Italic, string? FontName)>();
            foreach (var run in runs)
            {
                var text = ResolvePagePlaceholders(run.Text, pageIndex + 1, totalPages);
                if (text == "\n") continue; // skip paragraph breaks for width calc
                resolvedRuns.Add((text, run.Bold, run.Italic, run.FontName));
            }

            float totalWidth = 0;
            foreach (var (text, _, _, _) in resolvedRuns)
                totalWidth += EstimateTextWidth(text, fontSize);

            var startX = options.MarginLeft + (usableW - totalWidth) / 2;
            var currentX = startX;
            foreach (var (text, bold, italic, runFontName) in resolvedRuns)
            {
                page.AddText(text, currentX, y, fontSize, color, bold: bold, italic: italic, preferredFontName: runFontName);
                currentX += EstimateTextWidth(text, fontSize);
            }
        }
        else
        {
            // Fallback: plain text without formatting
            var resolved = plainText
                .Replace("{PAGE}", (pageIndex + 1).ToString())
                .Replace("{NUMPAGES}", totalPages.ToString());
            var textWidth = EstimateTextWidth(resolved, fontSize);
            var x = options.MarginLeft + (usableW - textWidth) / 2;
            page.AddText(resolved, x, y, fontSize, color);
        }
    }

    /// <summary>
    /// Converts a DOCX file directly to a PDF file.
    /// </summary>
    internal static void ConvertToFile(string docxPath, string pdfPath, ConversionOptions? options = null)
    {
        var doc = Convert(docxPath, options);
        doc.Save(pdfPath);
    }

    // ── Render state ────────────────────────────────────────────────────

    private sealed class RenderState
    {
        public PdfDocument Doc { get; }
        public ConversionOptions Options { get; }
        public PdfPage? CurrentPage { get; set; }
        public float CurrentY { get; set; }
        public bool IsTopOfPage { get; set; } = true;
        public float LastParagraphStartY { get; set; }
        public float LastLineHeight { get; set; }
        public bool LastParagraphWasEmpty { get; set; }
        /// <summary>
        /// Accumulated vertical space from empty paragraphs that overflowed past
        /// the bottom margin.  Applied at the top of the next page so that spacing
        /// from empty paragraphs is preserved across page breaks.
        /// </summary>
        public float PendingVerticalSpace { get; set; }
        /// <summary>
        /// The spacingAfter value applied by the most recently rendered paragraph.
        /// Used for paragraph spacing collapsing: the space between adjacent
        /// paragraphs is max(spacingAfter_prev, spacingBefore_current) rather
        /// than the sum (matching Word/LibreOffice behavior).
        /// </summary>
        public float LastSpacingAfter { get; set; }
        /// <summary>
        /// BehindDoc anchor images collected during paragraph rendering.
        /// These are rendered on every page after layout is complete (matching Word behavior).
        /// </summary>
        public List<DocxImage> BehindDocImages { get; } = new();
        public Dictionary<int, List<(DocxImage Image, float AnchorY)>> BehindDocImagesPerPage { get; } = new();

        // Per-section tracking: maps sectionIndex -> first page index (0-based)
        public List<int> SectionStartPages { get; } = new();
        public int CurrentSectionIndex { get; set; } = 0;

        // Footnote tracking
        public Dictionary<string, DocxFootnote>? Footnotes { get; set; }
        public List<string> CurrentPageFootnoteIds { get; } = new();
        public float FootnoteReservedHeight { get; set; }
        public float BaseMarginBottom { get; set; }

        // Multi-column state
        public int ColumnCount { get; set; } = 1;
        public int CurrentColumn { get; set; } = 0;
        public float ColumnWidth { get; set; }
        public float ColumnSpacing { get; set; }
        public float ColumnTopY { get; set; }
        public float SavedMarginLeft { get; set; }
        public float SavedMarginRight { get; set; }
        public float[]? ColumnWidths { get; set; }
        public float[]? ColumnGaps { get; set; }

        /// <summary>
        /// When grid snapping is active, the ascent offset applied at the top of a
        /// paragraph's first rendered line.  Stored so that the table renderer can
        /// compensate for the extra descent space included in the final AdvanceY of
        /// the preceding paragraph (Word's grid model places the next element at
        /// baseline + descent, not baseline + lineHeight).
        /// </summary>
        public float LastGridAscentExcess { get; set; }

        public float UsableWidth => Options.PageWidth - Options.MarginLeft - Options.MarginRight;

        public RenderState(PdfDocument doc, ConversionOptions options)
        {
            Doc = doc;
            Options = options;
        }

        public void EnterMultiColumnSection(int colCount, float colSpacing, float[]? colWidths = null, float[]? colGaps = null)
        {
            ColumnCount = colCount;
            CurrentColumn = 0;
            ColumnSpacing = colSpacing;
            SavedMarginLeft = Options.MarginLeft;
            SavedMarginRight = Options.MarginRight;
            ColumnWidths = colWidths;
            ColumnGaps = colGaps;

            if (colWidths != null && colWidths.Length > 0)
            {
                // Unequal column widths
                ColumnWidth = colWidths[0];
            }
            else
            {
                var totalUsable = Options.PageWidth - SavedMarginLeft - SavedMarginRight;
                ColumnWidth = (totalUsable - (colCount - 1) * colSpacing) / colCount;
            }
            ColumnTopY = CurrentY;
            // Set margins for first column
            Options.MarginRight = Options.PageWidth - SavedMarginLeft - ColumnWidth;
        }

        public void ExitMultiColumnSection()
        {
            Options.MarginLeft = SavedMarginLeft;
            Options.MarginRight = SavedMarginRight;
            ColumnCount = 1;
            CurrentColumn = 0;
        }

        public bool AdvanceToNextColumn()
        {
            if (CurrentColumn + 1 >= ColumnCount)
                return false;
            CurrentColumn++;

            if (ColumnWidths != null && CurrentColumn < ColumnWidths.Length)
            {
                // Unequal columns: calculate left margin from accumulated widths + gaps
                var left = SavedMarginLeft;
                for (int i = 0; i < CurrentColumn; i++)
                {
                    left += ColumnWidths[i];
                    if (ColumnGaps != null && i < ColumnGaps.Length)
                        left += ColumnGaps[i];
                }
                Options.MarginLeft = left;
                ColumnWidth = ColumnWidths[CurrentColumn];
                Options.MarginRight = Options.PageWidth - Options.MarginLeft - ColumnWidth;
            }
            else
            {
                Options.MarginLeft = SavedMarginLeft + CurrentColumn * (ColumnWidth + ColumnSpacing);
                Options.MarginRight = Options.PageWidth - Options.MarginLeft - ColumnWidth;
            }

            CurrentY = ColumnTopY;
            IsTopOfPage = true;
            LastLineHeight = 0;
            LastSpacingAfter = 0;
            LastGridAscentExcess = 0;
            return true;
        }

        public void EnsurePage()
        {
            if (CurrentPage == null || CurrentY < Options.MarginBottom)
            {
                // Try advancing to next column before creating a new page
                if (ColumnCount > 1 && CurrentPage != null && AdvanceToNextColumn())
                    return;

                // Render footnotes on the current page before moving to a new one
                if (CurrentPage != null)
                {
                    RenderPageFootnotes();
                    CurrentPageFootnoteIds.Clear();
                    FootnoteReservedHeight = 0;
                    Options.MarginBottom = BaseMarginBottom;
                }

                CurrentPage = Doc.AddPage(Options.PageWidth, Options.PageHeight);
                CurrentY = Options.PageHeight - Options.MarginTop;
                // Apply accumulated vertical space from empty paragraphs that
                // overflowed the previous page.
                if (PendingVerticalSpace > 0)
                {
                    CurrentY -= PendingVerticalSpace;
                    PendingVerticalSpace = 0;
                }
                IsTopOfPage = true;
                LastLineHeight = 0;
                LastSpacingAfter = 0;

                // Reset to first column on new page
                if (ColumnCount > 1)
                {
                    CurrentColumn = 0;
                    ColumnTopY = CurrentY;
                    Options.MarginLeft = SavedMarginLeft;
                    Options.MarginRight = Options.PageWidth - SavedMarginLeft - ColumnWidth;
                }
            }
        }

        public void AdvanceY(float amount)
        {
            CurrentY -= amount;
            IsTopOfPage = false;
        }

        public void ForceNewPage()
        {
            // Render footnotes before leaving the current page
            RenderPageFootnotes();

            // Restore margins if in multi-column mode
            if (ColumnCount > 1)
            {
                CurrentColumn = 0;
                Options.MarginLeft = SavedMarginLeft;
                Options.MarginRight = Options.PageWidth - SavedMarginLeft - ColumnWidth;
            }

            CurrentPage = Doc.AddPage(Options.PageWidth, Options.PageHeight);
            CurrentY = Options.PageHeight - Options.MarginTop;
            if (PendingVerticalSpace > 0)
            {
                CurrentY -= PendingVerticalSpace;
                PendingVerticalSpace = 0;
            }
            IsTopOfPage = true;
            LastLineHeight = 0;
            LastSpacingAfter = 0;
            LastGridAscentExcess = 0;

            // Reset footnote state for new page
            CurrentPageFootnoteIds.Clear();
            FootnoteReservedHeight = 0;
            Options.MarginBottom = BaseMarginBottom;

            if (ColumnCount > 1)
                ColumnTopY = CurrentY;
        }

        public void AddFootnoteReference(string fnId)
        {
            if (Footnotes == null || !Footnotes.TryGetValue(fnId, out var fn)) return;
            if (CurrentPageFootnoteIds.Contains(fnId)) return;
            CurrentPageFootnoteIds.Add(fnId);

            // Reserve space: separator line (1pt) + spacing (4pt) + one line per footnote
            var lineH = fn.FontSize * 1.2f;
            if (FootnoteReservedHeight == 0)
                FootnoteReservedHeight = 5f; // separator + gap
            FootnoteReservedHeight += lineH;
            Options.MarginBottom = BaseMarginBottom + FootnoteReservedHeight;
        }

        public void RenderPageFootnotes()
        {
            if (CurrentPage == null || Footnotes == null || CurrentPageFootnoteIds.Count == 0) return;

            var x = Options.MarginLeft;
            var separatorW = UsableWidth * 0.33f;
            var y = BaseMarginBottom + FootnoteReservedHeight;

            // Draw separator line
            CurrentPage.AddLine(x, y, x + separatorW, y, new PdfColor(0, 0, 0), 0.5f);
            y -= 4f;

            // Render each footnote
            foreach (var fnId in CurrentPageFootnoteIds)
            {
                if (!Footnotes.TryGetValue(fnId, out var fn)) continue;
                var lineH = fn.FontSize * 1.2f;
                y -= lineH;
                var curX = x;
                foreach (var run in fn.Runs)
                {
                    var runFs = run.FontSize > 0 ? run.FontSize : fn.FontSize;
                    var drawY = y + (run.VerticalPosition > 0 ? run.VerticalPosition : 0);
                    CurrentPage.AddText(run.Text, curX, drawY, runFs, preferredFontName: "Helvetica");
                    curX += run.Text.Length * runFs * 0.5f; // approximate width
                }
            }
        }
    }

    // ── Paragraph rendering ─────────────────────────────────────────────

    private static void RenderParagraph(RenderState state, DocxParagraph paragraph)
    {
        // Handle page break before
        if (paragraph.HasPageBreakBefore)
            state.ForceNewPage();

        var options = state.Options;
        var fontSize = paragraph.FontSize > 0 ? paragraph.FontSize : options.FontSize;
        // Font metrics factor: single-spaced line height ≈ fontSize × FontMetricsFactor
        // Use font-specific factor based on the dominant run font, falling back to
        // the paragraph mark font (pPr/rPr/rFonts) for empty paragraphs.
        var paraFontName = paragraph.Runs.FirstOrDefault(r => !string.IsNullOrEmpty(r.FontName))?.FontName
                        ?? paragraph.ParagraphFontName;
        var metricsFactor = GetFontMetricsFactor(paraFontName);
        // CJK fonts have taller ascent+descent ratios.  Apply the CJK-specific
        // factor for TOC paragraphs (TOC1-9) using auto line spacing so that the
        // table of contents page-break position matches the reference.  Limited
        // to TOC styles to avoid cumulative height growth in body text pages.
        if (paragraph.LineSpacing <= 0 && paraFontName != null && IsCjkFont(paraFontName)
            && IsTocStyle(paragraph.StyleId))
            metricsFactor = FontMetricsFactorCJK;
        float lineHeight;
        if (paragraph.LineSpacingAbsolute && paragraph.LineSpacing > 0)
            lineHeight = paragraph.LineSpacing; // exact/atLeast: absolute points
        else
        {
            var lineSpacingMul = paragraph.LineSpacing > 0 ? paragraph.LineSpacing : options.LineSpacing;
            // Use maximum run font size for line height when runs specify larger sizes
            // than the paragraph default (e.g. title text with run-level sz=48 but no
            // paragraph-level font size). Prevents text overlap on hard line breaks.
            var effectiveFs = fontSize;
            foreach (var run in paragraph.Runs)
            {
                var runFs = run.FontSize > 0 ? run.FontSize : fontSize;
                if (runFs > effectiveFs) effectiveFs = runFs;
            }
            lineHeight = effectiveFs * metricsFactor * lineSpacingMul;
        }

        // Snap to document grid when active (CJK line grid)
        if (options.GridLinePitch > 0 && paragraph.SnapToGrid)
        {
            var gridPitch = options.GridLinePitch;
            if (paragraph.LineSpacing == 0)
            {
                // Auto-spaced: for fonts that fit in a single grid cell, snap
                // the line height to the grid pitch.  For larger fonts spanning
                // multiple cells, keep the raw auto-spacing value (fontSize ×
                // metricsFactor) — Word advances by the natural line height,
                // not the full n×gridPitch allocation, so subsequent text
                // resumes closer to the heading.
                var maxFs = fontSize;
                foreach (var run in paragraph.Runs)
                {
                    var runFs = run.FontSize > 0 ? run.FontSize : fontSize;
                    if (runFs > maxFs) maxFs = runFs;
                }
                if (Compat.Ceiling(maxFs / gridPitch) > 1)
                    lineHeight = Math.Max(gridPitch, lineHeight);
                else
                    lineHeight = gridPitch;
            }
            else if (paragraph.LineSpacingAbsolute && !paragraph.LineSpacingExact)
            {
                // Snap atLeast line spacing to the nearest grid line.
                // Exact line spacing (lineRule="exact") is honoured as-is.
                lineHeight = Math.Max(gridPitch, Compat.Ceiling(lineHeight / gridPitch) * gridPitch);
            }
        }

        // Apply spacing before with collapsing: the space between adjacent
        // paragraphs is max(spacingAfter_prev, spacingBefore_current), matching
        // Word/LibreOffice behavior.  Since spacingAfter was already applied by
        // the previous paragraph, only add the excess (if any).
        var spacingBefore = paragraph.SpacingBefore > 0 ? paragraph.SpacingBefore : 0;
        if (spacingBefore > 0 && (!state.IsTopOfPage || paragraph.ForceSpacingBefore))
        {
            var extraBefore = spacingBefore - state.LastSpacingAfter;
            if (extraBefore > 0)
            {
                state.AdvanceY(extraBefore);
            }
        }

        // Force a page break when the paragraph carries Word's lastRenderedPageBreak
        // hint, indicating that Word placed a break before this paragraph's content.
        // This must precede the empty-paragraph block so it fires for non-empty
        // paragraphs that would render visible text.
        if (paragraph.HasLastRenderedPageBreak && state.CurrentPage != null && !state.IsTopOfPage)
        {
            state.ForceNewPage();
        }

        // Detect "line-spacer" host paragraphs: empty paragraphs whose only visual
        // content is a small wrapNone floating textbox that visually fits within a
        // single line of the paragraph and is anchored at ~0 vertical offset.  In
        // Word these paragraphs DO consume their line height (the textbox occupies
        // the empty line).  Larger floating overlays are handled by the
        // isFloatingAnchorOnlyParagraph branch below, where the line height is
        // suppressed because the overlay is positioned absolutely.
        var isLineSpacerAnchorHost =
            paragraph.Runs.Count == 0
            && paragraph.Images.Count == 0
            && paragraph.Shading == null
            && (paragraph.Shapes == null || paragraph.Shapes.Count == 0)
            && (paragraph.ConnectorLines == null || paragraph.ConnectorLines.Count == 0)
            && paragraph.FloatingTextBoxes is { Count: > 0 }
            && paragraph.FloatingTextBoxes.All(b =>
                Math.Abs(b.YPt) <= lineHeight
                && b.HeightPt <= lineHeight * 1.5f);

        // Handle empty paragraphs before EnsurePage — they don't produce visible content
        // and should not force a new page (avoids spurious trailing pages).
        if (paragraph.Runs.Count == 0 && paragraph.Images.Count == 0 && paragraph.Shading == null
            && (paragraph.Shapes == null || paragraph.Shapes.Count == 0)
            && ((paragraph.FloatingTextBoxes == null || paragraph.FloatingTextBoxes.Count == 0) || isLineSpacerAnchorHost)
            && (paragraph.ConnectorLines == null || paragraph.ConnectorLines.Count == 0))
        {

            // Record start position so floating textboxes anchored to this
            // empty paragraph use the correct Y (before advancing by lineHeight).
            // Word's vRelativeFrom="paragraph" measures from the line-box top,
            // while state.CurrentY is the next baseline. Offset upward by
            // (lineHeight - fontSize) to approximate the paragraph top.
            var baselineToTopOffset = lineHeight - fontSize;
            if (baselineToTopOffset < 0) baselineToTopOffset = 0;
            state.LastParagraphStartY = state.CurrentY + baselineToTopOffset;

            var totalEmptyAdvance = lineHeight;
            var spacingAfterEmpty = paragraph.SpacingAfter >= 0 ? paragraph.SpacingAfter : 0f;
            totalEmptyAdvance += spacingAfterEmpty;

            if (state.CurrentPage != null)
            {
                // At top of page, add an ascent offset for the paragraph mark so
                // that empty paragraphs consume the same initial space as non-empty
                // ones.  Without this, a leading empty paragraph's height starts
                // directly at the margin instead of being offset by the mark's
                // ascent, causing subsequent content to sit too high.
                if (state.IsTopOfPage)
                {
                    var emptyAscentOffset = options.GridLinePitch > 0 && paragraph.SnapToGrid
                        ? (lineHeight + fontSize) / 2f
                        : fontSize * AscentRatio;
                    state.AdvanceY(emptyAscentOffset);
                }
                state.AdvanceY(totalEmptyAdvance);
                // If the empty paragraph pushed past the bottom margin, accumulate
                // the overflow as pending vertical space for the next page so that
                // spacing from empty paragraphs is preserved across page breaks.
                if (state.CurrentY < state.Options.MarginBottom)
                {
                    var overflow = state.Options.MarginBottom - state.CurrentY;
                    state.PendingVerticalSpace += overflow;
                    state.CurrentY = state.Options.MarginBottom;
                }
            }
            else
            {
                // No page yet — accumulate as pending space
                state.PendingVerticalSpace += totalEmptyAdvance;
            }
            state.LastLineHeight = lineHeight;
            state.LastSpacingAfter = spacingAfterEmpty;
            state.LastParagraphWasEmpty = true;
            if (options.GridLinePitch > 0 && paragraph.SnapToGrid && !(paragraph.LineSpacingAbsolute && paragraph.LineSpacingExact))
                state.LastGridAscentExcess = (lineHeight + fontSize) / 2f;
            else
                state.LastGridAscentExcess = 0;
            if (paragraph.HasPageBreakAfter)
                state.ForceNewPage();
            return;
        }

        // Ghost paragraph: wrapNone textbox flow paragraph that advances Y for layout
        // but doesn't render visible text (the floating overlay handles visual rendering).
        if (paragraph.IsTextBoxFlow)
        {
            state.EnsurePage();
            if (state.IsTopOfPage)
            {
                var ghostAscentOffset = options.GridLinePitch > 0 && paragraph.SnapToGrid
                    ? (lineHeight + fontSize) / 2f
                    : fontSize * AscentRatio;
                state.AdvanceY(ghostAscentOffset);
            }
            state.LastParagraphStartY = state.CurrentY;

            // Compute how many lines the text would occupy and advance Y accordingly
            var fullText = string.Concat(paragraph.Runs.Select(r => r.Text));
            if (!string.IsNullOrEmpty(fullText))
            {
                var ghostAvailableWidth = paragraph.TextBoxWidth > 0 ? paragraph.TextBoxWidth : state.UsableWidth;
                var lines = WordWrap(fullText, ghostAvailableWidth, ghostAvailableWidth, fontSize,
                    paragraph.TabStops, useCalibriWidths: options.UseCalibriWidths);
                state.AdvanceY(lines.Count * lineHeight);
            }
            else
            {
                state.AdvanceY(lineHeight);
            }
            var spacingAfterGhost = paragraph.SpacingAfter >= 0 ? paragraph.SpacingAfter : 0f;
            state.AdvanceY(spacingAfterGhost);
            state.LastSpacingAfter = spacingAfterGhost;
            return;
        }

        state.EnsurePage();

        // Paragraphs that host only behindDoc images render the images for
        // deferred drawing but still consume vertical space based on the
        // paragraph mark's font size (matching Word's behaviour).
        if (paragraph.Runs.Count == 0
            && paragraph.Images.Count > 0 && paragraph.Images.All(img => img.IsBehindDoc)
            && (paragraph.Shapes == null || paragraph.Shapes.Count == 0))
        {
            // Capture paragraph-top Y before the ascent offset so behindDoc
            // images anchored at positionV=0 relativeFrom="paragraph" align with
            // the actual top of the paragraph (i.e., the top margin when this is
            // the first paragraph on the page), not the text baseline.
            var paragraphTopY = state.CurrentY;
            // At top of page, apply the ascent offset so the first line's visual
            // top aligns with the top margin (matching the empty-paragraph branch
            // above).  Without this the paragraph mark "line" would straddle the
            // margin and following paragraphs would sit too high on the page.
            if (state.IsTopOfPage)
            {
                var ascentOffset = options.GridLinePitch > 0 && paragraph.SnapToGrid
                    ? (lineHeight + fontSize) / 2f
                    : fontSize * AscentRatio;
                state.AdvanceY(ascentOffset);
            }
            var baselineToTopOffset = lineHeight - fontSize;
            if (baselineToTopOffset < 0) baselineToTopOffset = 0;
            state.LastParagraphStartY = paragraphTopY + baselineToTopOffset;
            var savedY = state.CurrentY;
            state.CurrentY = paragraphTopY;
            foreach (var image in paragraph.Images)
                RenderImage(state, image, paragraph.Alignment);
            state.CurrentY = savedY;
            var sa = paragraph.SpacingAfter >= 0 ? paragraph.SpacingAfter : 0f;
            state.AdvanceY(lineHeight + sa);
            state.LastLineHeight = lineHeight;
            state.LastSpacingAfter = sa;
            state.LastParagraphWasEmpty = true;
            if (paragraph.HasPageBreakAfter)
                state.ForceNewPage();
            return;
        }

        // At top of page, offset by font ascent so text visual top aligns with margin.
        // When a document grid is active, center the text within the first grid cell
        // so the baseline position matches LibreOffice's CJK line grid placement.
        var wasTopOfPage = state.IsTopOfPage;
        if (state.IsTopOfPage)
        {
            var ascentOffset = options.GridLinePitch > 0 && paragraph.SnapToGrid
                ? (lineHeight + fontSize) / 2f
                : fontSize * AscentRatio;
            state.AdvanceY(ascentOffset);
        }

        // When a paragraph's auto line height exceeds the previous paragraph's line
        // height, the baseline distance must match the larger value to prevent text
        // overlap (e.g., 36pt text following a 16pt paragraph).
        // Skip this compensation when the previous paragraph was empty, as empty
        // paragraphs have no visible content that could overlap.
        if (!wasTopOfPage && !paragraph.LineSpacingAbsolute && state.LastLineHeight > 0
            && lineHeight > state.LastLineHeight && !state.LastParagraphWasEmpty)
        {
            state.AdvanceY(lineHeight - state.LastLineHeight);
        }

        // Track paragraph start position for borders and floating textboxes
        var paragraphStartY = state.CurrentY;
        state.LastParagraphStartY = paragraphStartY;

        // Calculate available width considering indentation
        var indentLeft = paragraph.IndentLeft;
        var indentRight = paragraph.IndentRight;
        var availableWidth = state.UsableWidth - indentLeft - indentRight;
        var x = options.MarginLeft + indentLeft;

        // Prevent orphaned list labels: if the first text line won't fit on
        // this page, force a new page before rendering the label so the
        // number/bullet stays together with its content.
        if ((paragraph.IsBulletList || paragraph.IsNumberedList) && paragraph.ListText != null
            && state.CurrentPage != null && !state.IsTopOfPage
            && state.CurrentY - lineHeight < state.Options.MarginBottom)
        {
            state.ForceNewPage();
            state.EnsurePage();
            var ascentOffset = options.GridLinePitch > 0 && paragraph.SnapToGrid
                ? (lineHeight + fontSize) / 2f
                : fontSize * AscentRatio;
            state.AdvanceY(ascentOffset);
            paragraphStartY = state.CurrentY;
            state.LastParagraphStartY = paragraphStartY;
        }

        // Render list bullet/number
        if ((paragraph.IsBulletList || paragraph.IsNumberedList) && paragraph.ListText != null)
        {
            // Use the paragraph's first run font so the list label shares the same
            // embedded font slot as the body text, keeping identical ascent metrics
            // and preventing text-extraction Y-offset splits.
            var listFont = paragraph.Runs.FirstOrDefault(r => !string.IsNullOrEmpty(r.FontName))?.FontName;

            if (paragraph.IndentFirstLine < 0)
            {
                // Hanging indent from DOCX: number at outdented position, body text at indentLeft
                var numberX = options.MarginLeft + paragraph.IndentLeft + paragraph.IndentFirstLine;
                state.CurrentPage!.AddText(paragraph.ListText, numberX, state.CurrentY, fontSize, preferredFontName: listFont, bold: paragraph.ListTextBold);
                // x and availableWidth remain unchanged (body text wraps at indentLeft)
            }
            else
            {
                // Fallback: hardcoded list indent when no hanging indent is specified
                if (paragraph.IndentLeft > 0)
                {
                    // Style already provides left indentation (e.g. ListParagraph)
                    // Place the label to the left of the text body without reducing available width
                    var numberX = options.MarginLeft + Math.Max(0, paragraph.IndentLeft - 18f);
                    state.CurrentPage!.AddText(paragraph.ListText, numberX, state.CurrentY, fontSize, preferredFontName: listFont, bold: paragraph.ListTextBold);
                    // x and availableWidth remain unchanged (indentLeft already accounts for list)
                }
                else
                {
                    var listIndent = 18f * (paragraph.ListLevel + 1);
                    state.CurrentPage!.AddText(paragraph.ListText, x + listIndent - 12f, state.CurrentY, fontSize, preferredFontName: listFont, bold: paragraph.ListTextBold);
                    x += listIndent;
                    availableWidth -= listIndent;
                }
            }
        }

        // First line indent
        var firstLineX = x + Math.Max(0, paragraph.IndentFirstLine);
        var firstLineWidth = availableWidth - Math.Max(0, paragraph.IndentFirstLine);

        // For justified text (jc="both"), use the natural Calibri width for wrapping.
        // The per-character Calibri width table provides sufficient accuracy for
        // line-break matching without additional width reduction.
        // Add small CJK tolerance: CJK fonts may render fullwidth characters
        // slightly narrower than the 1000-unit estimate, preventing unnecessary wraps.
        var wrapFirstLineWidth = firstLineWidth + 0.5f;
        var wrapAvailableWidth = availableWidth + 0.5f;

        // Numbered justified paragraphs in DOCX references tend to wrap slightly
        // earlier than our baseline estimation. Apply a tiny, localized reduction
        // so long lines (like affidavit items 1/2/3) break closer to reference.
        if (paragraph.Alignment == "both" && paragraph.IsNumberedList)
        {
            var numberedBothWrapScale = GetDynamicNumberedWrapScale(paragraph);
            wrapFirstLineWidth *= numberedBothWrapScale;
            wrapAvailableWidth *= numberedBothWrapScale;
        }


        // Render images first (inline images), defer wrapTopAndBottom to after text
        List<DocxImage>? deferredWrapTBImages = null;
        float deferredImagesTotalHeight = 0;
        foreach (var image in paragraph.Images)
        {
            if (image.IsWrapTopBottom)
            {
                deferredWrapTBImages ??= new List<DocxImage>();
                deferredWrapTBImages.Add(image);
                const float emuPt = 914400f / 72f;
                var imgH = image.HeightEmu > 0 ? image.HeightEmu / emuPt : 150f;
                var imgW = image.WidthEmu > 0 ? image.WidthEmu / emuPt : 200f;
                if (imgW > state.UsableWidth) imgH *= state.UsableWidth / imgW;
                deferredImagesTotalHeight += imgH + 1f;
                continue;
            }
            RenderImage(state, image, paragraph.Alignment);
        }

        // Proactive page break: if text + deferred wrapTopAndBottom images
        // won't fit on the current page, break before rendering anything.
        if (deferredWrapTBImages != null && state.CurrentPage != null && !state.IsTopOfPage)
        {
            var neededHeight = lineHeight + deferredImagesTotalHeight;
            if (state.CurrentY - neededHeight < state.Options.MarginBottom)
                state.ForceNewPage();
        }

        // Render anchor shapes (filled rectangles behind text)
        if (paragraph.Shapes is { Count: > 0 })
        {
            foreach (var shape in paragraph.Shapes)
            {
                RenderShape(state, shape);
            }
        }

        // Render paragraph background shading
        if (paragraph.Shading != null)
        {
            var shadingHeight = lineHeight;
            if (paragraph.Runs.Count > 0)
            {
                var fullText = string.Concat(paragraph.Runs.Select(r => r.Text));
                if (!string.IsNullOrEmpty(fullText))
                {
                    var shadingLines = WordWrap(fullText, wrapFirstLineWidth, wrapAvailableWidth, fontSize, paragraph.TabStops, useCalibriWidths: options.UseCalibriWidths);
                    shadingHeight = shadingLines.Count * lineHeight;
                }
            }
            state.CurrentPage!.AddRectangle(options.MarginLeft, state.CurrentY - shadingHeight, state.UsableWidth, shadingHeight, paragraph.Shading);
        }

        // If paragraph has no text runs, still account for spacing
        if (paragraph.Runs.Count == 0)
        {
            // Only add line height when no inline images were rendered
            // (inline images advance Y themselves via RenderImage).
            // Skip line height for paragraphs that exist solely to host anchor shapes
            // (e.g. behind-doc drawing groups); they are positioned absolutely and
            // should not push subsequent content down.
            // Also skip for paragraphs that only contain behindDoc images — these are
            // rendered behind the document at absolute positions and should not push
            // subsequent content down.
            var isShapeOnlyParagraph = paragraph.Shapes is { Count: > 0 } && paragraph.Images.Count == 0;
            var isBehindDocOnlyParagraph = paragraph.Images.Count > 0 && paragraph.Images.All(img => img.IsBehindDoc);
            // Paragraphs whose only visual content is wrapNone floating textboxes and/or
            // connector lines (anchored shapes that are absolutely positioned overlays)
            // should not consume a line height in the main flow, matching Word's behaviour.
            var isFloatingAnchorOnlyParagraph =
                paragraph.Runs.Count == 0
                && paragraph.Images.Count == 0
                && (paragraph.Shapes is null || paragraph.Shapes.Count == 0)
                && ((paragraph.FloatingTextBoxes is { Count: > 0 })
                    || (paragraph.ConnectorLines is { Count: > 0 }));
            if (!isShapeOnlyParagraph && !isBehindDocOnlyParagraph && !isFloatingAnchorOnlyParagraph && (paragraph.Images.Count == 0 || !paragraph.Images.Any(img => !img.IsAnchor || img.IsWrapTopBottom)))
                state.AdvanceY(lineHeight);

            // Render wrapTopAndBottom images even for empty paragraphs
            foreach (var image in paragraph.Images)
            {
                if (image.IsWrapTopBottom)
                    RenderImage(state, image, paragraph.Alignment);
            }

            // Apply spacing after
            var spacingAfterEmpty = paragraph.SpacingAfter >= 0 ? paragraph.SpacingAfter : 0f;
            state.AdvanceY(spacingAfterEmpty);
            state.LastSpacingAfter = spacingAfterEmpty;
            state.LastParagraphWasEmpty = true;

            // Handle page break after (even for empty paragraphs)
            if (paragraph.HasPageBreakAfter)
                state.ForceNewPage();
            return;
        }

        // Check if runs have varying formatting
        // Merge consecutive runs with identical formatting to reduce text extraction artifacts
        var mergedRuns = MergeConsecutiveRuns(paragraph.Runs, fontSize);

        // Detect column breaks in runs
        var hasColumnBreak = mergedRuns.Any(r => r.IsColumnBreak);

        // If the first run is a column break, advance to next column BEFORE
        // rendering the paragraph text so the content appears in the correct column.
        var columnBreakHandledAtStart = false;
        if (hasColumnBreak && state.ColumnCount > 1
            && mergedRuns.Count > 0 && mergedRuns[0].IsColumnBreak)
        {
            // Remove the leading column-break run so it isn't processed again
            mergedRuns = mergedRuns.Skip(1).ToList();
            if (!state.AdvanceToNextColumn())
                state.ForceNewPage();
            columnBreakHandledAtStart = true;

            // Recalculate layout variables since margins changed after column advance
            availableWidth = state.UsableWidth - indentLeft - indentRight;
            x = options.MarginLeft + indentLeft;
            firstLineX = x + Math.Max(0, paragraph.IndentFirstLine);
            firstLineWidth = availableWidth - Math.Max(0, paragraph.IndentFirstLine);
            wrapFirstLineWidth = firstLineWidth;
            wrapAvailableWidth = availableWidth;
        }

        // Suppress underline on trailing whitespace-only runs when the paragraph
        // already has underline context (paragraph mark underline or any preceding
        // run with explicit <w:u> declaration). In such cases the underlined spaces
        // are paragraph mark formatting artifacts, not intentional form-fill lines.
        if (mergedRuns.Count >= 2)
        {
            var lastRun = mergedRuns[^1];
            if (lastRun.Underline && !string.IsNullOrEmpty(lastRun.Text) && string.IsNullOrWhiteSpace(lastRun.Text))
            {
                var hasUnderlineContext = paragraph.ParagraphMarkUnderline
                    || mergedRuns.Take(mergedRuns.Count - 1).Any(r =>
                        !string.IsNullOrWhiteSpace(r.Text) && r.HasExplicitUnderlineDecl);
                if (hasUnderlineContext)
                {
                    mergedRuns[^1] = lastRun with { Underline = false };
                }
            }
        }

        var hasVaryingFormat = false;
        if (mergedRuns.Count > 1)
        {
            var firstRun = mergedRuns[0];
            var firstRunFontSize = firstRun.FontSize > 0 ? firstRun.FontSize : fontSize;
            var firstRunColor = firstRun.Color;
            var firstRunBold = firstRun.Bold;
            var firstRunItalic = firstRun.Italic;
            var firstRunUnderline = firstRun.Underline;
            var firstRunCharSpacing = firstRun.CharSpacing;
            var firstRunFontName = firstRun.FontName;
            var firstRunVertPos = firstRun.VerticalPosition;

            hasVaryingFormat = mergedRuns.Any(r =>
            {
                var rFontSize = r.FontSize > 0 ? r.FontSize : fontSize;
                return Math.Abs(rFontSize - firstRunFontSize) > 0.01f
                    || r.Color != firstRunColor
                    || r.Bold != firstRunBold
                    || r.Italic != firstRunItalic
                    || r.Underline != firstRunUnderline
                    || Math.Abs(r.CharSpacing - firstRunCharSpacing) > 0.01f
                    || !string.Equals(r.FontName, firstRunFontName, StringComparison.OrdinalIgnoreCase)
                    || Math.Abs(r.VerticalPosition - firstRunVertPos) > 0.01f;
            });
        }

        if (hasVaryingFormat)
        {
            // Render each run individually on the same line
            RenderMultiFormatRuns(state, new DocxParagraph(mergedRuns, paragraph.Images, paragraph.Alignment,
                paragraph.SpacingBefore, paragraph.SpacingAfter, paragraph.LineSpacing, paragraph.LineSpacingAbsolute,
                paragraph.LineSpacingExact, paragraph.IndentLeft, paragraph.IndentRight, paragraph.IndentFirstLine,
                paragraph.IsBulletList, paragraph.IsNumberedList, paragraph.ListLevel, paragraph.ListText,
                paragraph.ListTextBold, paragraph.StyleId, paragraph.Bold, paragraph.Italic, paragraph.FontSize, paragraph.Color,
                paragraph.HasPageBreakBefore, paragraph.HasPageBreakAfter, paragraph.Shading, paragraph.TabStops,
                paragraph.SectionBreak, paragraph.Borders),
                x, firstLineX, wrapAvailableWidth, wrapFirstLineWidth, fontSize, lineHeight);

        }
        else
        {
            // Simple path: all runs share the same formatting
            var fullText = AddInterScriptSpacing(string.Concat(mergedRuns.Select(r => r.Text)), paragraph.AutoSpaceDE, paragraph.AutoSpaceDN);
            var dominantRun = mergedRuns.FirstOrDefault(r => !string.IsNullOrEmpty(r.Text));
            var runFontSize = dominantRun?.FontSize > 0 ? dominantRun.FontSize : fontSize;
            var runColor = dominantRun?.Color ?? paragraph.Color;
            var runBold = dominantRun?.Bold ?? false;
            var runItalic = dominantRun?.Italic ?? false;
            var runUnderline = dominantRun?.Underline ?? false;
            var runCharSpacing = dominantRun?.CharSpacing ?? 0f;
            var runFontName = dominantRun?.FontName;

            // Use Calibri widths only when the run font is Calibri-like;
            // wide sans-serif fonts (e.g. Montserrat) need Helvetica-based estimation.
            var paraUseCalibri = options.UseCalibriWidths
                && !IsWideSansSerifFont(runFontName);
            s_overrideWidths = GetFontOverrideWidths(runFontName);
            s_wideSansSerifFont = IsWideSansSerifFont(runFontName) && s_overrideWidths == null;
            s_serifRunInCalibri = paraUseCalibri && !s_serifFont && IsSerifFont(runFontName);
            var lines = WordWrap(fullText, wrapFirstLineWidth, wrapAvailableWidth, runFontSize, paragraph.TabStops, runBold, runCharSpacing, paraUseCalibri);
            // Keep s_overrideWidths set during the rendering loop so that
            // EstimateWrapTextWidth uses the same font metrics as WordWrap,
            // ensuring accurate justified text spacing and Tz compression.

            // If paragraph has leader tab stops, apply maxWidth so the Tz operator
            // compresses the extra-dot text to fit the intended tab position.
            float? tabLeaderMaxWidth = null;
            if (paragraph.TabStops?.Any(ts => ts.Leader is "dot" or "hyphen" or "underscore") == true)
            {
                tabLeaderMaxWidth = paragraph.TabStops.Max(ts => ts.Position);
            }

            for (var i = 0; i < lines.Count; i++)
            {
                // Proactive page break: break when the text's descenders would
                // clip below the bottom margin.
                var pageBreakThreshold = runFontSize * (GetFontMetricsFactor(runFontName) - 1f);
                if (state.CurrentPage != null && !state.IsTopOfPage
                    && state.CurrentY - pageBreakThreshold < state.Options.MarginBottom)
                {
                    state.ForceNewPage();
                }
                state.EnsurePage();
                if (state.IsTopOfPage)
                {
                    var lineAscentOffset = options.GridLinePitch > 0 && paragraph.SnapToGrid
                        ? (lineHeight + runFontSize) / 2f
                        : runFontSize * AscentRatio;
                    state.AdvanceY(lineAscentOffset);
                }

                var line = lines[i];
                var lineX = i == 0 ? firstLineX : x;
                var lineW = i == 0 ? firstLineWidth : availableWidth;

                // Use Tz compression to fit Helvetica text into Calibri-width lines\n only when text actually exceeds available width
                var renderMaxWidth = tabLeaderMaxWidth;
                // For center/right alignment, use Helvetica widths for positioning
                // so the rendered text aligns correctly with the margin edge.
                var isCenterRight = paragraph.Alignment is "center" or "right";
                var alignUseCalibri = isCenterRight ? false : paraUseCalibri;
                var textWidth = EstimateWrapTextWidth(line, runFontSize, runBold, runCharSpacing, alignUseCalibri);
                // Only apply Tz for non-tab-leader lines when text significantly overflows
                if (renderMaxWidth == null && textWidth > lineW)
                    renderMaxWidth = lineW;
                var effectiveWidth = renderMaxWidth.HasValue ? Math.Min(textWidth, renderMaxWidth.Value) : textWidth;
                var renderX = paragraph.Alignment switch
                {
                    "center" => lineX + (lineW - effectiveWidth) / 2,
                    "right" => lineX + lineW - effectiveWidth,
                    _ => lineX
                };

                // Calculate word spacing for justified text (alignment="both")
                // Apply to all lines except the last line of the paragraph
                float wordSpacing = 0;
                if (paragraph.Alignment == "both" && i < lines.Count - 1)
                {
                    var spaceCount = line.Count(c => c == ' ');
                    if (spaceCount > 0)
                    {
                        // When the wrap estimate undercounts true rendered width
                        // (e.g. serif runs in a Calibri-default doc had latinUnits
                        // scaled by 0.95 in EstimateCalibrTextWidth), invert that
                        // scaling so wordSpacing reflects the true text width.
                        // Otherwise justified word spacing pushes the rendered
                        // line past the right margin.
                        var realTextWidth = s_serifRunInCalibri ? textWidth / 0.95f : textWidth;
                        var extraSpace = lineW - realTextWidth;
                        if (extraSpace > 0)
                            wordSpacing = extraSpace / spaceCount;
                    }
                }
                // For justified paragraphs, always cap rendered width to lineW via
                // Tz so any wrap-time estimate error compresses instead of
                // overflowing the right margin (applies to last line too — in case
                // the algorithm packed one extra word due to undercounting).
                if (paragraph.Alignment == "both" && renderMaxWidth == null)
                    renderMaxWidth = lineW;

                state.CurrentPage!.AddText(line, renderX, state.CurrentY, runFontSize, runColor, maxWidth: renderMaxWidth, bold: runBold, italic: runItalic, underline: runUnderline, charSpacing: runCharSpacing, wordSpacing: wordSpacing, preferredFontName: runFontName);
                state.AdvanceY(lineHeight);
            }
            s_overrideWidths = null;
            s_wideSansSerifFont = false;
            s_serifRunInCalibri = false;
        }

        // For exact line spacing, cap paragraph height to lineHeight so
        // imperfect width estimation doesn't cause multi-line wrapping that
        // inflates equation/overlay paragraphs beyond their intended height.
        if (paragraph.LineSpacingExact && paragraphStartY > 0)
        {
            var expectedBottomY = paragraphStartY - lineHeight;
            if (state.CurrentY < expectedBottomY)
                state.CurrentY = expectedBottomY;
        }
        // Render paragraph borders
        if (paragraph.Borders != null && state.CurrentPage != null)
        {
            var bdr = paragraph.Borders;
            var paraLeft = options.MarginLeft + paragraph.IndentLeft;
            var paraRight = options.MarginLeft + state.UsableWidth - paragraph.IndentRight;
            var paraTop = paragraphStartY;
            var paraBottom = state.CurrentY;

            if (bdr.Top != null)
                state.CurrentPage.AddLine(paraLeft, paraTop, paraRight, paraTop, bdr.Top.Color, bdr.Top.Width);
            if (bdr.Bottom != null)
                state.CurrentPage.AddLine(paraLeft, paraBottom, paraRight, paraBottom, bdr.Bottom.Color, bdr.Bottom.Width);
            if (bdr.Left != null)
                state.CurrentPage.AddLine(paraLeft, paraTop, paraLeft, paraBottom, bdr.Left.Color, bdr.Left.Width);
            if (bdr.Right != null)
                state.CurrentPage.AddLine(paraRight, paraTop, paraRight, paraBottom, bdr.Right.Color, bdr.Right.Width);
        }

        // Render text box border (outline rectangle around text box content)
        if (paragraph.TextBoxBorder is { } tb && state.CurrentPage != null)
        {
            var tbLeft = tb.BoxXPt;
            var tbRight = tbLeft + tb.BoxWidthPt;
            // The anchor positionV offset is relative to the paragraph's position (top margin for first paragraph)
            var tbTop = options.PageHeight - options.MarginTop - tb.VerticalOffsetPt;
            var tbBottom = tbTop - tb.BoxHeightPt;
            state.CurrentPage.AddLine(tbLeft, tbTop, tbRight, tbTop, tb.Color, tb.LineWidth);
            state.CurrentPage.AddLine(tbLeft, tbBottom, tbRight, tbBottom, tb.Color, tb.LineWidth);
            state.CurrentPage.AddLine(tbLeft, tbTop, tbLeft, tbBottom, tb.Color, tb.LineWidth);
            state.CurrentPage.AddLine(tbRight, tbTop, tbRight, tbBottom, tb.Color, tb.LineWidth);
        }

        // Render deferred wrapTopAndBottom images (after text, before spacing)
        if (deferredWrapTBImages != null)
        {
            foreach (var image in deferredWrapTBImages)
                RenderImage(state, image, paragraph.Alignment);
        }

        // Apply spacing after
        var spacingAfter = paragraph.SpacingAfter >= 0 ? paragraph.SpacingAfter : 0f;
        state.AdvanceY(spacingAfter);
        state.LastSpacingAfter = spacingAfter;

        state.LastLineHeight = lineHeight;
        state.LastParagraphWasEmpty = false;

        // When grid snapping is active, the AdvanceY(lineHeight) after the last
        // text line includes both the descent of the current line and the ascent
        // of the *next* line. Tables do not consume that ascent, so record the
        // excess so RenderTable can compensate.
        if (options.GridLinePitch > 0 && paragraph.SnapToGrid && !(paragraph.LineSpacingAbsolute && paragraph.LineSpacingExact))
            state.LastGridAscentExcess = (lineHeight + fontSize) / 2f;
        else
            state.LastGridAscentExcess = 0;

        // Handle page break after
        if (paragraph.HasPageBreakAfter)
            state.ForceNewPage();

        // Handle column break at end: advance to next column
        // (only when column break was NOT already handled at the start)
        if (hasColumnBreak && !columnBreakHandledAtStart && state.ColumnCount > 1)
        {
            if (!state.AdvanceToNextColumn())
                state.ForceNewPage();
        }


    }

    /// <summary>
    /// Renders floating text boxes (wrapNone) at their absolute page positions.
    /// These text boxes do not affect the normal document flow.
    /// </summary>
    private static void RenderFloatingTextBoxes(RenderState state, List<DocxFloatingTextBox> boxes, float paragraphY)
    {
        var page = state.CurrentPage;
        if (page == null) return;
        var options = state.Options;
        var hostPageIdx = -1;
        for (int i = 0; i < state.Doc.Pages.Count; i++)
        {
            if (state.Doc.Pages[i] == page) { hostPageIdx = i; break; }
        }
        if (hostPageIdx < 0) return;

        foreach (var box in boxes)
        {
            // Convert DOCX coordinates (origin top-left) to PDF (origin bottom-left)
            var usableW = options.PageWidth - options.MarginLeft - options.MarginRight;

            // Horizontal positioning with alignment support
            float boxLeft;
            if (box.HAlign != null)
            {
                var refWidth = box.HRelativeFrom == "page" ? options.PageWidth : usableW;
                var refLeft = box.HRelativeFrom == "page" ? 0f : options.MarginLeft;
                boxLeft = box.HAlign switch
                {
                    "center" => refLeft + (refWidth - box.WidthPt) / 2,
                    "right" => refLeft + refWidth - box.WidthPt,
                    _ => refLeft // left
                };
            }
            else
            {
                boxLeft = box.HRelativeFrom == "page"
                    ? box.XPt
                    : options.MarginLeft + box.XPt;
            }

            // Vertical positioning with cross-page support
            var targetPage = page;
            float boxTop;
            if (box.VAlign != null)
            {
                var refHeight = box.VRelativeFrom == "page" ? options.PageHeight
                    : options.PageHeight - options.MarginTop - options.MarginBottom;
                var refTop = box.VRelativeFrom == "page" ? options.PageHeight
                    : options.PageHeight - options.MarginTop;
                boxTop = box.VAlign switch
                {
                    "center" => refTop - (refHeight - box.HeightPt) / 2,
                    "bottom" => refTop - refHeight + box.HeightPt,
                    _ => refTop // top
                };
            }
            else if (box.VRelativeFrom is "paragraph" or "line")
            {
                // Compute position in continuous document space to handle cross-page textboxes
                var contentHeight = options.PageHeight - options.MarginTop - options.MarginBottom;
                var contentTop = options.PageHeight - options.MarginTop;
                var hostDistFromTop = contentTop - paragraphY;
                var continuousY = hostPageIdx * contentHeight + hostDistFromTop + box.YPt;
                if (continuousY < 0) continuousY = 0;
                var targetIdx = (int)(continuousY / contentHeight);
                if (targetIdx >= state.Doc.Pages.Count)
                    targetIdx = state.Doc.Pages.Count - 1;
                var offsetInPage = continuousY - targetIdx * contentHeight;
                boxTop = contentTop - offsetInPage;
                targetPage = state.Doc.Pages[targetIdx];
            }
            else if (box.VRelativeFrom == "page")
            {
                boxTop = options.PageHeight - box.YPt;
            }
            else // margin
            {
                boxTop = options.PageHeight - options.MarginTop - box.YPt;
            }

            // Render fill background if present
            if (box.FillColor is { } fill)
            {
                targetPage.AddRectangle(boxLeft, boxTop - box.HeightPt, box.WidthPt, box.HeightPt, fill);
            }

            // Render border if present
            if (box.Border is { } tb)
            {
                var tbLeft = boxLeft;
                var tbRight = tbLeft + box.WidthPt;
                var tbTop = boxTop;
                var tbBottom = tbTop - box.HeightPt;
                targetPage.AddLine(tbLeft, tbTop, tbRight, tbTop, tb.Color, tb.LineWidth);
                targetPage.AddLine(tbLeft, tbBottom, tbRight, tbBottom, tb.Color, tb.LineWidth);
                targetPage.AddLine(tbLeft, tbTop, tbLeft, tbBottom, tb.Color, tb.LineWidth);
                targetPage.AddLine(tbRight, tbTop, tbRight, tbBottom, tb.Color, tb.LineWidth);
            }

            // Render text content at absolute position.
            // Offset baseline down from boxTop by the first line's font size + top inset
            // so text sits inside the textbox boundary, not on its edge.
            var firstFontSize = box.Paragraphs.FirstOrDefault()?.FontSize ?? 0;
            if (box.Paragraphs.Count > 0)
            {
                var firstRunsMax = box.Paragraphs[0].Runs.Where(r => r.FontSize > 0).Select(r => r.FontSize).DefaultIfEmpty(0f).Max();
                if (firstRunsMax > firstFontSize) firstFontSize = firstRunsMax;
            }
            if (firstFontSize <= 0) firstFontSize = options.FontSize;
            var topInset = box.TopInsetPt;
            var currentY = boxTop - firstFontSize - topInset;
            foreach (var para in box.Paragraphs)
            {
                // Use paragraph font size, but prefer run-level size when larger (runs may override inherited style size)
                var fontSize = para.FontSize > 0 ? para.FontSize : options.FontSize;
                if (para.Runs.Count > 0)
                {
                    var maxRunFs = para.Runs.Where(r => r.FontSize > 0).Select(r => r.FontSize).DefaultIfEmpty(0f).Max();
                    if (maxRunFs > fontSize) fontSize = maxRunFs;
                }
                var paraRunFont = para.Runs.FirstOrDefault(r => !string.IsNullOrEmpty(r.FontName))?.FontName;
                float lineHeight;
                if (para.LineSpacingAbsolute && para.LineSpacing > 0)
                    lineHeight = para.LineSpacing;
                else
                {
                    var lineSpacingMul = para.LineSpacing > 0 ? para.LineSpacing : options.LineSpacing;
                    lineHeight = fontSize * GetFontMetricsFactor(paraRunFont) * lineSpacingMul;
                }

                if (para.Runs.Count == 0)
                {
                    currentY -= lineHeight;
                    continue;
                }

                // Simple text rendering at absolute position
                var fullText = string.Concat(para.Runs.Select(r => r.Text));
                if (string.IsNullOrWhiteSpace(fullText))
                {
                    currentY -= lineHeight;
                    continue;
                }

                // Use the first run's formatting as representative
                var color = para.Color ?? para.Runs.FirstOrDefault()?.Color;
                var bold = para.Bold || (para.Runs.FirstOrDefault()?.Bold ?? false);
                var italic = para.Italic || (para.Runs.FirstOrDefault()?.Italic ?? false);

                // Wrap text within the box width (minus left/right insets)
                var leftInset = box.LeftInsetPt;
                var maxWidth = (box.WidthPt > 0 ? box.WidthPt : state.UsableWidth) - leftInset * 2;
                if (maxWidth < 10) maxWidth = box.WidthPt > 0 ? box.WidthPt : state.UsableWidth;
                // Use Calibri widths only when the text box paragraph actually uses Calibri;
                // other fonts (e.g. Century Gothic) have different glyph widths.
                var boxUseCalibri = options.UseCalibriWidths
                    && (string.IsNullOrEmpty(paraRunFont) || paraRunFont.Contains("Calibri", StringComparison.OrdinalIgnoreCase));
                s_overrideWidths = GetFontOverrideWidths(paraRunFont);
                var lines = WordWrap(fullText, maxWidth, maxWidth, fontSize,
                    para.TabStops, useCalibriWidths: boxUseCalibri);
                s_overrideWidths = null;

                foreach (var line in lines)
                {
                    var x = boxLeft + leftInset;
                    if (para.Alignment == "center")
                    {
                        // Use Helvetica widths for positioning: PDF renders with Helvetica,
                        // so Calibri estimates can misalign center/right text.
                        var textWidth = EstimateWrapTextWidth(line, fontSize, bold, 0, useCalibriWidths: false);
                        x = boxLeft + leftInset + (maxWidth - textWidth) / 2;
                    }
                    else if (para.Alignment == "right")
                    {
                        var textWidth = EstimateWrapTextWidth(line, fontSize, bold, 0, useCalibriWidths: false);
                        x = boxLeft + leftInset + maxWidth - textWidth;
                    }
                    targetPage.AddText(line, x, currentY, fontSize, color, bold: bold, italic: italic);
                    currentY -= lineHeight;
                }
            }
        }
    }

    /// <summary>
    /// Renders connector lines (straightConnector1, line shapes) as absolute-positioned lines.
    /// </summary>
    private static void RenderConnectorLines(RenderState state, List<DocxConnectorLine> lines, float paragraphY)
    {
        var page = state.CurrentPage;
        if (page == null) return;
        var options = state.Options;
        var hostPageIdx = -1;
        for (int i = 0; i < state.Doc.Pages.Count; i++)
        {
            if (state.Doc.Pages[i] == page) { hostPageIdx = i; break; }
        }
        if (hostPageIdx < 0) return;

        var contentHeight = options.PageHeight - options.MarginTop - options.MarginBottom;
        var contentTop = options.PageHeight - options.MarginTop;

        foreach (var conn in lines)
        {
            // Convert DOCX coords to PDF coords
            float pdfX1, pdfY1, pdfX2, pdfY2;

            if (conn.HRelativeFrom == "page")
            {
                pdfX1 = conn.X1Pt;
                pdfX2 = conn.X2Pt;
            }
            else // column or margin
            {
                pdfX1 = options.MarginLeft + conn.X1Pt;
                pdfX2 = options.MarginLeft + conn.X2Pt;
            }

            var targetPage = page;
            if (conn.VRelativeFrom is "paragraph" or "line")
            {
                var hostDistFromTop = contentTop - paragraphY;
                var cy1 = hostPageIdx * contentHeight + hostDistFromTop + conn.Y1Pt;
                var cy2 = hostPageIdx * contentHeight + hostDistFromTop + conn.Y2Pt;
                var targetIdx1 = (int)(cy1 / contentHeight);
                if (targetIdx1 >= state.Doc.Pages.Count) targetIdx1 = state.Doc.Pages.Count - 1;
                if (targetIdx1 < 0) targetIdx1 = 0;
                pdfY1 = contentTop - (cy1 - targetIdx1 * contentHeight);
                pdfY2 = contentTop - (cy2 - targetIdx1 * contentHeight);
                targetPage = state.Doc.Pages[targetIdx1];
            }
            else if (conn.VRelativeFrom == "page")
            {
                pdfY1 = options.PageHeight - conn.Y1Pt;
                pdfY2 = options.PageHeight - conn.Y2Pt;
            }
            else // margin
            {
                pdfY1 = contentTop - conn.Y1Pt;
                pdfY2 = contentTop - conn.Y2Pt;
            }

            targetPage.AddLine(pdfX1, pdfY1, pdfX2, pdfY2, conn.Color, conn.LineWidthPt, conn.DashPattern);

            // Render arrow heads as small filled triangles
            if (conn.HasTailArrow)
            {
                RenderArrowHead(targetPage, pdfX1, pdfY1, pdfX2, pdfY2, conn.LineWidthPt, conn.Color);
            }
            if (conn.HasHeadArrow)
            {
                RenderArrowHead(targetPage, pdfX2, pdfY2, pdfX1, pdfY1, conn.LineWidthPt, conn.Color);
            }
        }
    }

    /// <summary>Renders a filled triangle arrow head at (toX, toY) pointing from (fromX, fromY).</summary>
    private static void RenderArrowHead(PdfPage page, float fromX, float fromY, float toX, float toY, float lineWidth, PdfColor color)
    {
        var arrowLen = Math.Max(lineWidth * 4, 6f);
        var arrowHalfW = arrowLen * 0.4f;
        var dx = toX - fromX;
        var dy = toY - fromY;
        var len = (float)Math.Sqrt(dx * dx + dy * dy);
        if (len < 0.01f) return;
        var ux = dx / len;
        var uy = dy / len;
        // Perpendicular
        var px = -uy;
        var py = ux;
        // Triangle points: tip at (toX,toY), base offset back along the line
        var bx = toX - ux * arrowLen;
        var by = toY - uy * arrowLen;
        var points = new List<PdfPoint>
        {
            new PdfPoint(toX, toY),
            new PdfPoint(bx + px * arrowHalfW, by + py * arrowHalfW),
            new PdfPoint(bx - px * arrowHalfW, by - py * arrowHalfW)
        };
        page.AddPolygon(points, color);
    }

    /// <summary>
    /// Merges consecutive runs that have identical formatting (font size, color, bold, underline)
    /// to reduce separate AddText calls and improve text extraction quality.
    /// </summary>
    private static List<DocxRun> MergeConsecutiveRuns(List<DocxRun> runs, float defaultFontSize)
    {
        if (runs.Count <= 1) return runs;
        var result = new List<DocxRun>(runs.Count);
        var current = runs[0];
        for (var i = 1; i < runs.Count; i++)
        {
            var next = runs[i];
            var curFs = current.FontSize > 0 ? current.FontSize : defaultFontSize;
            var nextFs = next.FontSize > 0 ? next.FontSize : defaultFontSize;
            // Whitespace-only runs are format-agnostic (invisible characters have no visible color/bold)
            // EXCEPT for underline: underlined spaces create a visible line and must be preserved.
            // EXCEPT for tabs: tab characters need their own font for leader dot rendering.
            var isWhitespaceOnly = string.IsNullOrWhiteSpace(next.Text) && !next.Text.Contains('\t');
            var isCurWhitespace = string.IsNullOrWhiteSpace(current.Text) && !current.Text.Contains('\t');
            var colorMatch = current.Color == next.Color || isWhitespaceOnly || isCurWhitespace;
            var boldMatch = current.Bold == next.Bold || isWhitespaceOnly || isCurWhitespace;
            var italicMatch = current.Italic == next.Italic || isWhitespaceOnly || isCurWhitespace;
            var underlineMatch = current.Underline == next.Underline
                || (isWhitespaceOnly && !next.Underline)
                || (isCurWhitespace && !current.Underline);
            var charSpacingMatch = Math.Abs(current.CharSpacing - next.CharSpacing) < 0.01f || isWhitespaceOnly || isCurWhitespace;
            var fontNameMatch = string.Equals(current.FontName, next.FontName, StringComparison.OrdinalIgnoreCase) || isWhitespaceOnly || isCurWhitespace;
            var vertPosMatch = Math.Abs(current.VerticalPosition - next.VerticalPosition) < 0.01f;
            if (Math.Abs(curFs - nextFs) < 0.01f && colorMatch && boldMatch && italicMatch && underlineMatch && charSpacingMatch && fontNameMatch && vertPosMatch
                && !current.IsPageBreak && !next.IsPageBreak
                && !current.IsColumnBreak && !next.IsColumnBreak
                && current.FootnoteId == null && next.FootnoteId == null)
            {
                // When the current run is whitespace-only, adopt the next run's
                // visual formatting so that following non-whitespace text retains
                // its bold/color/font (whitespace is format-agnostic).
                var useBold = isCurWhitespace ? next.Bold : current.Bold;
                var useItalic = isCurWhitespace ? (current.Italic || next.Italic) : (current.Italic || next.Italic);
                var useColor = isCurWhitespace ? next.Color : current.Color;
                var useUnderline = isCurWhitespace ? next.Underline : current.Underline;
                var useCharSpacing = isCurWhitespace ? next.CharSpacing : current.CharSpacing;
                var useFontName = isCurWhitespace ? next.FontName : current.FontName;
                current = new DocxRun(current.Text + next.Text, useBold, useItalic,
                    current.FontSize, useColor, false, useUnderline, useCharSpacing, useFontName,
                    VerticalPosition: current.VerticalPosition);
            }
            else
            {
                result.Add(current);
                current = next;
            }
        }
        result.Add(current);
        return result;
    }

    /// <summary>
    /// Renders runs with varying font sizes/colors on the same line(s).
    /// </summary>
    private static void RenderMultiFormatRuns(RenderState state, DocxParagraph paragraph,
        float baseX, float firstLineX, float availableWidth, float firstLineWidth,
        float defaultFontSize, float lineHeight)
    {
        // Proactive page break: use text descent to check margin fit
        var mfPageBreakDescent = defaultFontSize * (GetFontMetricsFactor(
            paragraph.Runs.FirstOrDefault(r => !string.IsNullOrEmpty(r.FontName))?.FontName) - 1f);
        if (state.CurrentPage != null && !state.IsTopOfPage
            && state.CurrentY - mfPageBreakDescent < state.Options.MarginBottom)
        {
            state.ForceNewPage();
        }
        state.EnsurePage();
        if (state.IsTopOfPage)
        {
            var mfAscentOffset = state.Options.GridLinePitch > 0 && paragraph.SnapToGrid
                ? (lineHeight + defaultFontSize) / 2f
                : defaultFontSize * AscentRatio;
            state.AdvanceY(mfAscentOffset);
        }
        var currentX = firstLineX;
        var isFirstLine = true;
        var rightEdge = state.Options.MarginLeft + state.UsableWidth - paragraph.IndentRight;

        // For center/right alignment, pre-calculate total line width of all runs
        // and offset the starting X position.
        // Use Helvetica widths (useCalibri=false) because the multi-format path
        // renders each run individually without Tz compression, so positioning
        // must match the actual rendered (Helvetica) glyph widths.
        var needsPerLineAlignment = false;
        var isJustified = paragraph.Alignment == "both";
        if (isJustified)
        {
            needsPerLineAlignment = true;
        }
        else if (paragraph.Alignment is "center" or "right")
        {
            var totalWidth = 0f;
            string? prevRunTextPre = null;
            foreach (var r in paragraph.Runs)
            {
                if (string.IsNullOrEmpty(r.Text)) continue;
                var rFs = r.FontSize > 0 ? r.FontSize : defaultFontSize;
                // Remove hard line breaks for width estimation (only first line matters for single-line case)
                var rText = r.Text.Replace("\n", "");
                rText = ExpandTabs(rText, rFs, paragraph.TabStops, false, currentXOffset: totalWidth);
                rText = AddInterScriptSpacing(rText, paragraph.AutoSpaceDE, paragraph.AutoSpaceDN);
                // Add inter-script gap at run boundaries (Latin↔CJK)
                if (prevRunTextPre != null && rText.Length > 0
                    && NeedInterRunScriptGap(prevRunTextPre, rText))
                {
                    totalWidth += rFs * 500f / 1000f;
                }
                totalWidth += EstimateTextWidth(rText, rFs, r.CharSpacing);
                prevRunTextPre = rText;
            }
            var lineW = isFirstLine ? firstLineWidth : availableWidth;
            if (totalWidth <= lineW)
            {
                // Single-line: apply one-time alignment offset
                var offset = paragraph.Alignment == "center"
                    ? (lineW - totalWidth) / 2
                    : lineW - totalWidth;
                currentX += offset;
            }
            else
            {
                // Multi-line: defer alignment to per-line flush
                needsPerLineAlignment = true;
            }
        }

        // Line buffer for per-line alignment when center/right text wraps across lines.
        // Each entry stores the AddText parameters; entries are flushed with an
        // alignment shift when a line break (word-wrap, CJK break, or hard break) occurs.
        var lineEntries = needsPerLineAlignment
            ? new List<(string Text, float X, float Y, float FontSize, PdfColor? Color, bool Bold, bool Italic, bool Underline, float CharSpacing, string? FontName, float? MaxWidth, float? UlWidth)>()
            : null;

        void BufferOrEmit(string text, float bx, float by, float bfs, PdfColor? bcolor,
            bool bbold, bool bitalic, bool bunderline, float bcs, string? bfontName, float? bmaxW, float? bulW)
        {
            if (lineEntries != null)
                lineEntries.Add((text, bx, by, bfs, bcolor, bbold, bitalic, bunderline, bcs, bfontName, bmaxW, bulW));
            else
                state.CurrentPage!.AddText(text, bx, by, bfs, bcolor,
                    bold: bbold, italic: bitalic, underline: bunderline, charSpacing: bcs,
                    preferredFontName: bfontName, maxWidth: bmaxW, underlineWidth: bulW);
        }

        void FlushLineEntries(bool isLastLine = false)
        {
            if (lineEntries == null || lineEntries.Count == 0) return;
            var lw = isFirstLine ? firstLineWidth : availableWidth;

            if (isJustified)
            {
                // Compute word spacing from Calibri-estimated widths (same as wrapping).
                // Each entry gets maxWidth so PdfWriter can Tz-scale each run to
                // exactly its allocated width using the actual embedded font metrics,
                // eliminating gaps at run boundaries (e.g. regular→bold transitions).
                static float WrapEntryWidth((string Text, float X, float Y, float FontSize, PdfColor? Color, bool Bold, bool Italic, bool Underline, float CharSpacing, string? FontName, float? MaxWidth, float? UlWidth) e, bool useCalibri)
                {
                    var w = EstimateWrapTextWidth(e.Text, e.FontSize, e.Bold, e.CharSpacing, useCalibri);
                    if (useCalibri && e.FontName != null
                        && !e.FontName.Contains("Calibri", StringComparison.OrdinalIgnoreCase))
                    {
                        w *= e.Bold ? 1.06f : 1.00f;
                    }
                    return w;
                }

                var useCalibriJustify = state.Options.UseCalibriWidths;
                float totalTextWidth = 0;
                int totalSpaces = 0;
                foreach (var e in lineEntries)
                {
                    totalTextWidth += WrapEntryWidth(e, useCalibriJustify);
                    totalSpaces += e.Text.Count(c => c == ' ');
                }
                float justifyWordSpacing = 0;
                if (!isLastLine && totalSpaces > 0)
                {
                    var extraSpace = lw - totalTextWidth;
                    if (extraSpace > 0)
                        justifyWordSpacing = extraSpace / totalSpaces;
                }

                float entryX = lineEntries[0].X;
                foreach (var e in lineEntries)
                {
                    var estW = WrapEntryWidth(e, useCalibriJustify);
                    state.CurrentPage!.AddText(e.Text, entryX, e.Y, e.FontSize, e.Color,
                        bold: e.Bold, italic: e.Italic, underline: e.Underline, charSpacing: e.CharSpacing,
                        wordSpacing: justifyWordSpacing,
                        preferredFontName: e.FontName, maxWidth: estW, underlineWidth: e.UlWidth);
                    entryX += estW
                        + justifyWordSpacing * e.Text.Count(c => c == ' ');
                }
            }
            else
            {
                // For center/right alignment, use original positioning logic
                float minX = float.MaxValue, maxEndX = 0;
                foreach (var e in lineEntries)
                {
                    if (e.X < minX) minX = e.X;
                    var w = EstimateWrapTextWidth(e.Text, e.FontSize, e.Bold, e.CharSpacing, false);
                    var endX = e.X + w;
                    if (endX > maxEndX) maxEndX = endX;
                }
                var lineTextWidth = maxEndX - minX;
                var shift = paragraph.Alignment == "center"
                    ? Math.Max(0, (lw - lineTextWidth) / 2f)
                    : Math.Max(0, lw - lineTextWidth);
                foreach (var e in lineEntries)
                {
                    state.CurrentPage!.AddText(e.Text, e.X + shift, e.Y, e.FontSize, e.Color,
                        bold: e.Bold, italic: e.Italic, underline: e.Underline, charSpacing: e.CharSpacing,
                        wordSpacing: 0,
                        preferredFontName: e.FontName, maxWidth: e.MaxWidth, underlineWidth: e.UlWidth);
                }
            }
            lineEntries.Clear();
        }

        // Detect CJK context: if any run contains CJK text, whitespace-only
        // underlined runs should use CJK half-width (500/1000) for space metrics
        // to match Word/LibreOffice rendering of form-fill underline lines.
        var hasCjkContext = paragraph.Runs.Any(r =>
            !string.IsNullOrEmpty(r.Text) && r.Text.Any(c => c >= '\u2E80' && !char.IsHighSurrogate(c) && !char.IsLowSurrogate(c)));

        // Extend rightEdge for tab-leader paragraphs so dot-filled text doesn't wrap
        if (paragraph.TabStops is { Count: > 0 }
            && paragraph.TabStops.Any(ts => ts.Leader is "dot" or "hyphen" or "underscore"))
        {
            var maxTabPos = paragraph.TabStops.Max(ts => ts.Position);
            var expandedRight = state.Options.MarginLeft + maxTabPos / 0.725f;
            if (expandedRight > rightEdge)
                rightEdge = expandedRight;
        }

        string? prevRenderedSegment = null;
        foreach (var run in paragraph.Runs)
        {
            if (string.IsNullOrEmpty(run.Text)) continue;

            // Register footnote reference for bottom-of-page rendering
            if (!string.IsNullOrEmpty(run.FootnoteId))
                state.AddFootnoteReference(run.FootnoteId);

            var runFs = run.FontSize > 0 ? run.FontSize : defaultFontSize;
            var runColor = run.Color ?? paragraph.Color;

            // Split run text by hard line breaks first (from <w:br/>)
            var hardLines = run.Text.Split('\n');
            for (var hi = 0; hi < hardLines.Length; hi++)
            {
                // Force line break for each \n (except before the first segment)
                if (hi > 0)
                {
                    // The line ending at this <w:br/> is the last visual line
                    // of the preceding segment, so it must not be justified.
                    FlushLineEntries(isLastLine: true);
                    if (!state.IsTopOfPage && state.CurrentY - runFs * (GetFontMetricsFactor(run.FontName) - 1f) < state.Options.MarginBottom)
                        state.ForceNewPage();
                    else
                        state.AdvanceY(lineHeight);
                    state.EnsurePage();
                    if (state.IsTopOfPage)
                    {
                        var hardBrAscentOffset = state.Options.GridLinePitch > 0 && paragraph.SnapToGrid
                            ? (lineHeight + runFs) / 2f
                            : runFs * AscentRatio;
                        state.AdvanceY(hardBrAscentOffset);
                    }
                    currentX = baseX;
                    isFirstLine = false;
                }

                // For centered/right text, use Helvetica widths for positioning
                // (matches actual rendering; multi-format runs have no Tz compression).
                // For left-aligned text, keep Calibri widths to preserve layout matching.
                var isCenterRight = paragraph.Alignment is "center" or "right";
                var useCalibri = isCenterRight ? false : state.Options.UseCalibriWidths;
                // When the run font is a serif (Times New Roman etc.) inside a Calibri-default
                // document, Calibri Latin glyphs are ~5% wider than the serif glyphs Word
                // actually renders. Apply the s_serifRunInCalibri flag so downstream
                // EstimateWrapTextWidth/EstimateCalibrTextWidth calls scale the estimate down,
                // matching Word's wrap decisions for serif runs in Calibri-default docs.
                s_serifRunInCalibri = useCalibri && !s_serifFont && IsSerifFont(run.FontName);

                // Handle tab stops directly: advance to tab position with leader fill
                if (hardLines[hi] == "\t" && paragraph.TabStops is { Count: > 0 })
                {
                    var relX = currentX - state.Options.MarginLeft;
                    DocxTabStop? matchedStop = null;
                    foreach (var ts in paragraph.TabStops)
                    {
                        if (ts.Position > relX + 1)
                        {
                            matchedStop = ts;
                            break;
                        }
                    }
                    if (matchedStop != null)
                    {
                        var leaderChar = matchedStop.Leader switch
                        {
                            "dot" => '.',
                            "hyphen" => '-',
                            "underscore" => '_',
                            _ => (char?)null
                        };
                        if (leaderChar != null)
                        {
                            var leaderCharWidth = runFs * GetHelveticaCharWidth(leaderChar.Value) / 1000f * 0.725f;
                            var gapWidth = matchedStop.Position - relX;
                            var fillCount = Math.Max(1, (int)(gapWidth / leaderCharWidth));
                            var leaderText = new string(leaderChar.Value, fillCount);
                            state.EnsurePage();
                            state.CurrentPage!.AddText(leaderText, currentX, state.CurrentY + run.VerticalPosition, runFs, runColor,
                                bold: run.Bold, charSpacing: run.CharSpacing, preferredFontName: run.FontName,
                                maxWidth: gapWidth > 0 ? gapWidth : (float?)null);
                        }
                        currentX = state.Options.MarginLeft + matchedStop.Position;
                        continue;
                    }
                }

                var segment = ExpandTabs(hardLines[hi], runFs, paragraph.TabStops, useCalibri, currentXOffset: currentX - state.Options.MarginLeft);
                segment = AddInterScriptSpacing(segment, paragraph.AutoSpaceDE, paragraph.AutoSpaceDN);
                if (string.IsNullOrEmpty(segment)) continue;

                // Add inter-script gap at run boundaries (Latin↔CJK) for centered/right text
                if (isCenterRight && prevRenderedSegment != null && NeedInterRunScriptGap(prevRenderedSegment, segment))
                {
                    currentX += runFs * 500f / 1000f;
                }
                prevRenderedSegment = segment;

                // For whitespace-only underlined runs in CJK context, use CJK half-width
                // space metric (500/1000) for layout and underline width calculation.
                var isWhitespaceUnderline = run.Underline && string.IsNullOrWhiteSpace(segment) && hasCjkContext;
                var cjkSpaceWidth = isWhitespaceUnderline ? runFs * 500f / 1000f : 0f;

                // When segment contains CJK characters, the PDF renderer uses CJK fonts
                // for the entire text block, so ASCII spaces use CJK half-width (500/1000).
                var segmentHasCjk = hasCjkContext && segment.Any(c => c >= '\u2E80' && !char.IsHighSurrogate(c) && !char.IsLowSurrogate(c));
                var effectiveSpaceWidth = (isWhitespaceUnderline || segmentHasCjk)
                    ? runFs * 500f / 1000f
                    : runFs * GetWrapCharWidth(' ', useCalibri) / 1000f;

                // Split segment by spaces for word wrapping, but accumulate text
                // per line to produce fewer AddText calls (improves text extraction).
                var words = segment.Split(' ');
                var pendingText = "";
                var pendingX = currentX;

                // When wrapping uses Calibri widths but the run's actual font is
                // non-Calibri (e.g. Times New Roman), glyph widths differ —
                // especially uppercase letters which are substantially wider in
                // serif fonts. Apply a correction factor so wrapping decisions
                // better match the actual font width.
                var nonCalibriWidthFactor = 1f;
                if (useCalibri && run.FontName != null
                    && !run.FontName.Contains("Calibri", StringComparison.OrdinalIgnoreCase))
                    nonCalibriWidthFactor = run.Bold ? 1.06f : 1.00f;

                // When a CJK paragraph has pure-Latin runs (e.g. "PECVD"),
                // EstimateWrapTextWidth sees no CJK in the word and applies only
                // 8% Latin reduction instead of the 22% CJK-context reduction.
                // Pre-compute correction factor to match CJK font Latin glyph widths.
                var cjkLatinCorrection = 1f;
                if (!useCalibri && hasCjkContext)
                {
                    var nonCjkKeep = run.Bold ? 0.95f : (s_serifFont ? 0.90f : 0.92f);
                    cjkLatinCorrection = 0.78f / nonCjkKeep;
                }

                for (var wi = 0; wi < words.Length; wi++)
                {
                    var word = words[wi];
                    var wordWidth = EstimateWrapTextWidth(word, runFs, run.Bold, run.CharSpacing, useCalibri) * nonCalibriWidthFactor;
                    // Apply CJK Latin correction for pure-Latin words in CJK paragraph
                    if (cjkLatinCorrection < 1f && word.Length > 0)
                    {
                        bool wordHasCjk = false;
                        foreach (var wc in word)
                            if (wc >= '\u2E80' && !char.IsHighSurrogate(wc) && !char.IsLowSurrogate(wc))
                            { wordHasCjk = true; break; }
                        if (!wordHasCjk)
                            wordWidth *= cjkLatinCorrection;
                    }
                    var spaceWidth = wi > 0
                        ? effectiveSpaceWidth + run.CharSpacing
                        : 0;

                    // Check if word fits on current line (skip wrapping for exact line spacing)
                    if (!paragraph.LineSpacingExact && currentX + spaceWidth + wordWidth > rightEdge && (pendingText.Length > 0 || currentX > baseX + 1))
                    {
                        // When pendingText is empty and the word contains CJK characters,
                        // skip wrapping to fill remaining space on the current line.
                        // The CJK break loop below will split the text at character boundaries.
                        var remainingWidth = rightEdge - currentX - spaceWidth;
                        var canCjkBreak = pendingText.Length == 0
                            && word.Length > 1
                            && remainingWidth >= runFs * 0.9f
                            && word.Any(c => c >= '\u2E80' && !char.IsHighSurrogate(c) && !char.IsLowSurrogate(c));

                        if (!canCjkBreak)
                        {
                            // Flush pending text before wrapping
                            if (pendingText.Length > 0)
                            {
                                var flushMaxW = rightEdge - pendingX;
                                BufferOrEmit(pendingText, pendingX, state.CurrentY + run.VerticalPosition, runFs, runColor, run.Bold, run.Italic, run.Underline, run.CharSpacing, run.FontName, flushMaxW > 0 ? flushMaxW : (float?)null, null);
                                pendingText = "";
                            }
                            FlushLineEntries();
                            // Wrap to next line
                            if (!state.IsTopOfPage && state.CurrentY - runFs * (GetFontMetricsFactor(run.FontName) - 1f) < state.Options.MarginBottom)
                                state.ForceNewPage();
                            else
                                state.AdvanceY(lineHeight);
                            state.EnsurePage();
                            if (state.IsTopOfPage)
                            {
                                var wrapAscentOffset = state.Options.GridLinePitch > 0 && paragraph.SnapToGrid
                                    ? (lineHeight + runFs) / 2f
                                    : runFs * AscentRatio;
                                state.AdvanceY(wrapAscentOffset);
                            }
                            currentX = baseX;
                            pendingX = currentX;
                            isFirstLine = false;
                            spaceWidth = 0;
                        }
                    }

                    if (wi > 0)
                    {
                        pendingText += " ";
                        currentX += spaceWidth;
                    }

                    pendingText += word;
                    currentX += wordWidth;

                    // Break oversized CJK words at character boundaries (kinsoku)
                    while (!paragraph.LineSpacingExact && currentX > rightEdge && pendingText.Length > 1)
                    {
                        var breakAt = -1;
                        float accWidth = 0;
                        for (var ci = 0; ci < pendingText.Length; ci++)
                        {
                            var charW = runFs * GetWrapCharWidth(pendingText[ci], useCalibri) / 1000f;
                            // In CJK paragraph context, Latin chars render with CJK font's
                            // narrower Latin glyphs. Apply 22% reduction for non-fullwidth chars.
                            if (!useCalibri && hasCjkContext
                                && GetWrapCharWidth(pendingText[ci], useCalibri) != 1000
                                && pendingText[ci] >= '!' && pendingText[ci] <= '~')
                            {
                                charW *= 0.78f;
                            }
                            accWidth += charW;
                            // Update break point before overflow check so the last
                            // fitting character is included on the current line.
                            if (ci > 0 && (GetWrapCharWidth(pendingText[ci], useCalibri) == 1000 || GetWrapCharWidth(pendingText[ci - 1], useCalibri) == 1000))
                            {
                                if (!IsNoStartChar(pendingText[ci]))
                                    breakAt = ci;
                            }
                            if (pendingX + accWidth > rightEdge && breakAt >= 0)
                                break;
                        }
                        if (breakAt <= 0) break;
                        var cjkBrkMaxW = rightEdge - pendingX;
                        BufferOrEmit(pendingText[..breakAt], pendingX, state.CurrentY + run.VerticalPosition, runFs, runColor, run.Bold, run.Italic, run.Underline, run.CharSpacing, run.FontName, cjkBrkMaxW > 0 ? cjkBrkMaxW : (float?)null, null);
                        FlushLineEntries();
                        pendingText = pendingText[breakAt..];
                        if (!state.IsTopOfPage && state.CurrentY - runFs * (GetFontMetricsFactor(run.FontName) - 1f) < state.Options.MarginBottom)
                            state.ForceNewPage();
                        else
                            state.AdvanceY(lineHeight);
                        state.EnsurePage();
                        if (state.IsTopOfPage)
                        {
                            var cjkBrkAscentOffset = state.Options.GridLinePitch > 0 && paragraph.SnapToGrid
                                ? (lineHeight + runFs) / 2f
                                : runFs * AscentRatio;
                            state.AdvanceY(cjkBrkAscentOffset);
                        }
                        currentX = baseX + EstimateWrapTextWidth(pendingText, runFs, run.Bold, run.CharSpacing, useCalibri) * nonCalibriWidthFactor;
                        pendingX = baseX;
                        isFirstLine = false;
                    }
                }

                // Flush remaining text for this segment
                if (pendingText.Length > 0)
                {
                    // For whitespace-only underlined runs in CJK context, pass explicit
                    // underline width based on CJK space metric so the form-fill line
                    // matches Word/LibreOffice rendering width.
                    float? ulWidth = isWhitespaceUnderline
                        ? pendingText.Length * cjkSpaceWidth
                        : null;
                    // Constrain each segment's maxWidth to its estimated width so Tz
                    // compression prevents overlap when the actual font renders wider
                    // than the Calibri-estimated layout width.
                    var segEstWidth = currentX - pendingX;
                    var segMaxW = segEstWidth > 0 ? segEstWidth : rightEdge - pendingX;
                    BufferOrEmit(pendingText, pendingX, state.CurrentY + run.VerticalPosition, runFs, runColor, run.Bold, run.Italic, run.Underline, run.CharSpacing, run.FontName, segMaxW > 0 ? segMaxW : (float?)null, ulWidth);
                }

            }
        }

        FlushLineEntries(isLastLine: true);
        state.AdvanceY(lineHeight);
        s_serifRunInCalibri = false;
    }

    // ── Shape rendering ─────────────────────────────────────────────────

    private static void RenderShape(RenderState state, DocxShape shape)
    {
        const float emuPerPoint = 914400f / 72f;

        var width = shape.WidthEmu / emuPerPoint;
        var height = shape.HeightEmu / emuPerPoint;
        var x = state.Options.MarginLeft + shape.OffsetXEmu / emuPerPoint;
        var y = (state.Options.PageHeight - state.Options.MarginTop) - shape.OffsetYEmu / emuPerPoint - height;

        // Alpha-blend fill color over white background
        var fc = shape.FillColor;
        var a = shape.Alpha;
        var blended = new PdfColor(
            1f + (fc.R - 1f) * a,
            1f + (fc.G - 1f) * a,
            1f + (fc.B - 1f) * a);

        RenderShapeGeometry(state.CurrentPage!, x, y, width, height, blended, shape);

        // Render stroke/outline if present
        if (shape.StrokeColor != null)
        {
            var strokeWidth = shape.StrokeWidthEmu > 0 ? shape.StrokeWidthEmu / emuPerPoint : 0.75f;
            var sc = shape.StrokeColor.Value;
            state.CurrentPage!.AddLine(x, y, x + width, y, sc, strokeWidth);                // bottom
            state.CurrentPage!.AddLine(x, y + height, x + width, y + height, sc, strokeWidth); // top
            state.CurrentPage!.AddLine(x, y, x, y + height, sc, strokeWidth);                // left
            state.CurrentPage!.AddLine(x + width, y, x + width, y + height, sc, strokeWidth); // right
        }
    }

    private static void RenderHeaderFooterShape(PdfPage page, ConversionOptions options, DocxShape shape)
    {
        const float emuPerPoint = 914400f / 72f;

        var width = shape.WidthEmu / emuPerPoint;
        var height = shape.HeightEmu / emuPerPoint;
        var x = options.MarginLeft + shape.OffsetXEmu / emuPerPoint;
        // Header/footer anchors are typically page-relative; don't subtract page top margin.
        var y = options.PageHeight - shape.OffsetYEmu / emuPerPoint - height;

        var fc = shape.FillColor;
        var a = shape.Alpha;
        var blended = new PdfColor(
            1f + (fc.R - 1f) * a,
            1f + (fc.G - 1f) * a,
            1f + (fc.B - 1f) * a);

        RenderShapeGeometry(page, x, y, width, height, blended, shape);

        if (shape.StrokeColor != null)
        {
            var strokeWidth = shape.StrokeWidthEmu > 0 ? shape.StrokeWidthEmu / emuPerPoint : 0.75f;
            var sc = shape.StrokeColor.Value;
            page.AddLine(x, y, x + width, y, sc, strokeWidth);
            page.AddLine(x, y + height, x + width, y + height, sc, strokeWidth);
            page.AddLine(x, y, x, y + height, sc, strokeWidth);
            page.AddLine(x + width, y, x + width, y + height, sc, strokeWidth);
        }
    }

    private static void RenderHeaderFooterImage(PdfPage page, ConversionOptions options, DocxImage img)
    {
        const float emuPerPoint = 914400f / 72f;
        var width = img.WidthEmu / emuPerPoint;
        var height = img.HeightEmu / emuPerPoint;

        // Footer/header anchor images have offsets relative to the footer paragraph position.
        // The footer paragraph is at approximately (pageHeight - footerMargin) in top-down DOCX coords.
        // Convert to PDF bottom-up: y = footerMargin - offsetYPt - height
        var offsetXPt = img.OffsetXEmu / emuPerPoint;
        var offsetYPt = img.OffsetYEmu / emuPerPoint;
        var x = options.MarginLeft + offsetXPt;
        var y = options.FooterMargin - offsetYPt - height;

        page.AddImage(img.Data, img.Extension, x, y, width, height, img.Alpha);
    }

    /// <summary>
    /// Renders shape geometry. For "frame" shapes, draws only the border area.
    /// Supports frame, ellipse and custom polygon paths; defaults to rectangle.
    /// </summary>
    private static void RenderShapeGeometry(PdfPage page, float x, float y, float width, float height,
        PdfColor color, DocxShape shape)
    {
        // If shape has no fill (stroke-only), skip fill rendering
        if (!shape.FillOnly && shape.StrokeColor != null &&
            shape.FillColor is { R: >= 0.999f, G: >= 0.999f, B: >= 0.999f })
            return; // stroke-only shape, fill handled separately

        if (shape.PresetGeometry == "frame")
        {
            // Frame shape: draw 4 border rectangles, leaving center empty
            var t = shape.FrameThicknessRatio * Math.Min(width, height);
            // Top border
            page.AddRectangle(x, y + height - t, width, t, color);
            // Bottom border
            page.AddRectangle(x, y, width, t, color);
            // Left border (between top and bottom)
            page.AddRectangle(x, y + t, t, height - 2 * t, color);
            // Right border (between top and bottom)
            page.AddRectangle(x + width - t, y + t, t, height - 2 * t, color);
        }
        else if (shape.PresetGeometry == "ellipse")
        {
            page.AddEllipse(x, y, width, height, color);
        }
        else if (shape.PresetGeometry == "custom" && shape.CustomPaths is { Count: > 0 })
        {
            foreach (var path in shape.CustomPaths)
            {
                if (path.Subpaths.Count == 0)
                    continue;

                var mappedSubpaths = path.Subpaths
                    .Where(subpath => subpath.Count >= 3)
                    .Select(subpath => subpath
                        .Select(p => new PdfPoint(
                            x + p.X * width,
                            y + height - p.Y * height))
                        .ToList())
                    .Where(subpath => subpath.Count >= 3)
                    .ToList();

                if (mappedSubpaths.Count == 0)
                    continue;

                // Always use AddCompoundPolygon so the even-odd fill rule
                // (path.UseEvenOddFill) is honoured for single-subpath shapes too.
                page.AddCompoundPolygon(mappedSubpaths, color, path.UseEvenOddFill);
            }
        }
        else
        {
            page.AddRectangle(x, y, width, height, color);
        }
    }

    // ── Image rendering ─────────────────────────────────────────────────

    private static void RenderImage(RenderState state, DocxImage image, string alignment = "left")
    {
        const float emuPerPoint = 914400f / 72f;

        var width = image.WidthEmu > 0 ? image.WidthEmu / emuPerPoint : 200f;
        var height = image.HeightEmu > 0 ? image.HeightEmu / emuPerPoint : 150f;

        var format = image.Extension;
        if (format != "jpg" && format != "png")
            return; // Only support JPEG and PNG

        // Anchor images: render at absolute offset position without advancing cursor
        if (image.IsAnchor && !image.IsWrapTopBottom)
        {
            // BehindDoc images are rendered on the page they appear on
            if (image.IsBehindDoc)
            {
                var pageIdx = state.Doc.Pages.Count - 1;
                if (!state.BehindDocImagesPerPage.TryGetValue(pageIdx, out var list))
                {
                    list = new List<(DocxImage, float)>();
                    state.BehindDocImagesPerPage[pageIdx] = list;
                }
                list.Add((image, state.CurrentY));
                return;
            }

            var anchorX = state.Options.MarginLeft + image.OffsetXEmu / emuPerPoint;
            var anchorY = state.CurrentY - image.OffsetYEmu / emuPerPoint;

            // Clamp to page bounds
            if (width > state.Options.PageWidth - state.Options.MarginLeft - state.Options.MarginRight)
            {
                var scale = (state.Options.PageWidth - state.Options.MarginLeft - state.Options.MarginRight) / width;
                width *= scale;
                height *= scale;
            }

            state.CurrentPage!.AddImage(image.Data, format, anchorX, anchorY - height, width, height, image.Alpha);
            return; // Don't advance Y
        }

        // Clamp to usable width
        if (width > state.UsableWidth)
        {
            var scale = state.UsableWidth / width;
            width *= scale;
            height *= scale;
        }

        // Check if image fits on current page
        if (state.CurrentY - height < state.Options.MarginBottom)
            state.EnsurePage();

        var x = state.Options.MarginLeft;
        if (image.IsWrapTopBottom)
        {
            // wrapTopAndBottom: use anchor horizontal offset
            const float emuPerPt = 914400f / 72f;
            x = state.Options.MarginLeft + image.OffsetXEmu / emuPerPt;
            if (x + width > state.Options.PageWidth - state.Options.MarginRight)
                x = state.Options.PageWidth - state.Options.MarginRight - width;
            if (x < state.Options.MarginLeft)
                x = state.Options.MarginLeft;
        }
        else if (alignment == "center")
            x = state.Options.MarginLeft + (state.UsableWidth - width) / 2;
        else if (alignment == "right")
            x = state.Options.MarginLeft + state.UsableWidth - width;
        var y = state.CurrentY - height;

        state.CurrentPage!.AddImage(image.Data, format, x, y, width, height);
        state.AdvanceY(height + 1f); // 1pt gap after image
    }

    // ── Header/footer element rendering ─────────────────────────────────

    /// <summary>
    /// Removes trailing empty paragraphs from a list of elements.
    /// Footer/header content often ends with an empty paragraph that should not
    /// inflate the estimated height (Word ignores these for layout purposes).
    /// </summary>
    private static List<DocxElement> TrimTrailingEmptyParagraphs(List<DocxElement> elements)
    {
        var trimmed = new List<DocxElement>(elements);
        while (trimmed.Count > 0
            && trimmed[^1] is DocxParagraph p
            && p.Runs.Count == 0
            && p.Images.Count == 0)
        {
            trimmed.RemoveAt(trimmed.Count - 1);
        }
        return trimmed;
    }

    /// <summary>
    /// Estimates the total height of a list of elements (paragraphs + tables)
    /// for header/footer sizing calculations.
    /// </summary>
    private static float EstimateElementsHeight(List<DocxElement> elements, ConversionOptions options)
    {
        const float emuPerPt = 914400f / 72f;
        float totalHeight = 0;
        float lastSpacingAfter = 0;
        var isFirst = true;
        foreach (var element in elements)
        {
            switch (element)
            {
                case DocxParagraph para:
                {
                    // Apply spacing before (with collapsing) for non-first paragraphs
                    if (!isFirst)
                    {
                        var sb = para.SpacingBefore > 0 ? para.SpacingBefore : 0f;
                        var gap = Math.Max(sb, lastSpacingAfter);
                        totalHeight += gap;
                    }

                    var fontSize = para.FontSize > 0 ? para.FontSize : options.FontSize;
                    float lineHeight;
                    if (para.LineSpacingAbsolute && para.LineSpacing > 0)
                        lineHeight = para.LineSpacing; // exact/atLeast: absolute points
                    else
                    {
                        var estFont = para.Runs.FirstOrDefault(r => !string.IsNullOrEmpty(r.FontName))?.FontName;
                        lineHeight = fontSize * GetFontMetricsFactor(estFont) * (para.LineSpacing > 0 ? para.LineSpacing : options.LineSpacing);
                    }
                    var usableW = options.PageWidth - options.MarginLeft - options.MarginRight;
                    foreach (var image in para.Images)
                    {
                        if (image.IsAnchor && !image.IsWrapTopBottom) continue;
                        var imgW = image.WidthEmu > 0 ? image.WidthEmu / emuPerPt : 100f;
                        var imgH = image.HeightEmu > 0 ? image.HeightEmu / emuPerPt : 75f;
                        if (imgW > usableW) imgH *= usableW / imgW;
                        totalHeight += imgH + 1f;
                    }
                    var text = string.Concat(para.Runs.Select(r => r.Text));
                    if (!string.IsNullOrEmpty(text))
                        totalHeight += lineHeight;
                    else if (para.Images.Count == 0)
                        totalHeight += lineHeight;

                    lastSpacingAfter = para.SpacingAfter >= 0 ? para.SpacingAfter : 0f;
                    isFirst = false;
                    break;
                }
                case DocxTable table:
                {
                    var usableWidth = options.PageWidth - options.MarginLeft - options.MarginRight;
                    var effectiveTableWidth = string.IsNullOrEmpty(table.Alignment) || table.Alignment == "left"
                        ? usableWidth - table.IndentLeft : usableWidth;
                    var colWidths = CalculateTableColumnWidths(table, effectiveTableWidth);
                    var cellPaddingH = (table.CellMarginLeft + table.CellMarginRight) / 2;
                    var cellPaddingV = Math.Max(1f, Math.Max(table.CellMarginTop, table.CellMarginBottom));
                    foreach (var row in table.Rows)
                    {
                        var rowH = CalculateRowHeight(row, colWidths, cellPaddingH, cellPaddingV, options, table.StyleLineSpacing, table.StyleSpacingAfter);
                        if (row.Height > 0)
                        {
                            if (row.HeightExact)
                                rowH = row.Height;
                            else
                                rowH = Math.Max(rowH, row.Height);
                        }
                        if (!row.HeightExact)
                            rowH = Math.Max(rowH, CalculateRowInlineImageFloorHeight(row, colWidths, cellPaddingH, cellPaddingV));
                        totalHeight += rowH;
                    }
                    lastSpacingAfter = 0;
                    isFirst = false;
                    break;
                }
            }
        }
        return totalHeight;
    }

    /// <summary>
    /// Renders header/footer elements (paragraphs + tables) directly on a page
    /// at the specified starting Y position, flowing downward.
    /// </summary>
    private static void RenderHeaderFooterElementsOnPage(PdfPage page, ConversionOptions options,
        List<DocxElement> elements, float startY, int pageIndex = 0, int totalPages = 1, int sectionPageNum = -1)
    {
        const float emuPerPt = 914400f / 72f;
        var y = startY;
        float lastSpacingAfter = 0;
        var isFirst = true;

        foreach (var element in elements)
        {
            switch (element)
            {
                case DocxParagraph para:
                {
                    // Apply spacing before (with collapsing) for non-first paragraphs
                    if (!isFirst)
                    {
                        var sb = para.SpacingBefore > 0 ? para.SpacingBefore : 0f;
                        var gap = Math.Max(sb, lastSpacingAfter);
                        y -= gap;
                    }

                    var usableW = options.PageWidth - options.MarginLeft - options.MarginRight;
                    // Render images (inline + behindDoc anchors + wrapTopAndBottom anchors)
                    foreach (var image in para.Images)
                    {
                        if (image.IsAnchor && !image.IsBehindDoc && !image.IsWrapTopBottom) continue;
                        var imgW = image.WidthEmu > 0 ? image.WidthEmu / emuPerPt : 100f;
                        var imgH = image.HeightEmu > 0 ? image.HeightEmu / emuPerPt : 75f;
                        var fmt = image.Extension;
                        if (fmt != "jpg" && fmt != "png") continue;

                        if (image.IsBehindDoc)
                        {
                            // Render behindDoc anchor at absolute position on this page
                            var ax = image.RelativeFromH == "page"
                                ? image.OffsetXEmu / emuPerPt
                                : options.MarginLeft + image.OffsetXEmu / emuPerPt;
                            var ay = image.RelativeFromV == "page"
                                ? options.PageHeight - image.OffsetYEmu / emuPerPt
                                : options.PageHeight - options.MarginTop - image.OffsetYEmu / emuPerPt;
                            page.AddImage(image.Data, fmt, ax, ay - imgH, imgW, imgH);
                            continue;
                        }

                        if (imgW > usableW) { var s = usableW / imgW; imgW *= s; imgH *= s; }
                        float imgX;
                        if (image.IsWrapTopBottom)
                        {
                            // wrapTopAndBottom: use anchor horizontal offset
                            imgX = (image.RelativeFromH == "margin" ? options.MarginLeft : options.MarginLeft)
                                + image.OffsetXEmu / emuPerPt;
                            // Clamp so image stays in page bounds
                            if (imgX + imgW > options.PageWidth - options.MarginRight)
                                imgX = options.PageWidth - options.MarginRight - imgW;
                            if (imgX < options.MarginLeft)
                                imgX = options.MarginLeft;
                        }
                        else
                        {
                            imgX = para.Alignment switch
                            {
                                "center" => options.MarginLeft + (usableW - imgW) / 2,
                                "right" => options.MarginLeft + usableW - imgW,
                                _ => options.MarginLeft
                            };
                        }
                        page.AddImage(image.Data, fmt, imgX, y - imgH, imgW, imgH);
                        y -= imgH + 1f;
                    }
                    // Render text runs (resolve page-number placeholders)
                    var effectivePageNum = sectionPageNum > 0 ? sectionPageNum : (pageIndex + 1);
                    var text = string.Concat(para.Runs.Select(r => r.Text));
                    text = ResolvePagePlaceholders(text, effectivePageNum, totalPages);
                    if (!string.IsNullOrEmpty(text))
                    {
                        var fontSize = para.FontSize > 0 ? para.FontSize : options.FontSize;
                        var firstRun = para.Runs.FirstOrDefault(r => !string.IsNullOrEmpty(r.Text));
                        var runFontSize = firstRun?.FontSize > 0 ? firstRun.FontSize : fontSize;
                        float lineHeight;
                        if (para.LineSpacingAbsolute && para.LineSpacing > 0)
                            lineHeight = para.LineSpacing;
                        else
                            lineHeight = runFontSize * GetFontMetricsFactor(firstRun?.FontName) * (para.LineSpacing > 0 ? para.LineSpacing : options.LineSpacing);
                        var textWidth = EstimateTextWidth(text, runFontSize);
                        var textX = para.Alignment switch
                        {
                            "center" => options.MarginLeft + (usableW - textWidth) / 2,
                            "right" => options.MarginLeft + usableW - textWidth,
                            _ => options.MarginLeft
                        };
                        y -= runFontSize;
                        page.AddText(text, textX, y, runFontSize, firstRun?.Color ?? para.Color,
                            bold: firstRun?.Bold ?? false, italic: firstRun?.Italic ?? false, preferredFontName: firstRun?.FontName);
                        y -= lineHeight - runFontSize;
                    }
                    lastSpacingAfter = para.SpacingAfter >= 0 ? para.SpacingAfter : 0f;
                    isFirst = false;
                    break;
                }
                case DocxTable table:
                {
                    y = RenderHeaderFooterTable(page, options, table, y);
                    lastSpacingAfter = 0;
                    isFirst = false;
                    break;
                }
            }
        }
    }

    /// <summary>
    /// Renders a table in header/footer area directly on the page.
    /// </summary>
    private static float RenderHeaderFooterTable(PdfPage page, ConversionOptions options,
        DocxTable table, float startY)
    {
        const float emuPerPt = 914400f / 72f;
        var usableWidth = options.PageWidth - options.MarginLeft - options.MarginRight;
        var effectiveTableWidth = string.IsNullOrEmpty(table.Alignment) || table.Alignment == "left"
            ? usableWidth - table.IndentLeft : usableWidth;
        var cellPaddingH = (table.CellMarginLeft + table.CellMarginRight) / 2;
        var cellPaddingV = Math.Max(table.CellMarginTop, table.CellMarginBottom);
        var colWidths = CalculateTableColumnWidths(table, effectiveTableWidth);
        var colCount = colWidths.Length;

        // Calculate table total width and center offset
        var tableWidth = colWidths.Sum();
        var tableOffsetX = table.Alignment switch
        {
            "center" => options.MarginLeft + (usableWidth - tableWidth) / 2,
            "right" => options.MarginLeft + usableWidth - tableWidth,
            _ => options.MarginLeft + table.IndentLeft
        };

        var y = startY;
        foreach (var row in table.Rows)
        {
            var rowHeight = CalculateRowHeight(row, colWidths, cellPaddingH, cellPaddingV, options, table.StyleLineSpacing, table.StyleSpacingAfter);
            if (row.Height > 0)
            {
                if (row.HeightExact)
                    rowHeight = row.Height;
                else
                    rowHeight = Math.Max(rowHeight, row.Height);
            }
            if (!row.HeightExact)
                rowHeight = Math.Max(rowHeight, CalculateRowInlineImageFloorHeight(row, colWidths, cellPaddingH, cellPaddingV));

            var cellX = tableOffsetX;
            var colIdx = row.GridBefore;
            // Advance cellX past skipped grid columns
            for (var gb = 0; gb < row.GridBefore && gb < colCount; gb++)
                cellX += colWidths[gb];

            for (var ci = 0; ci < row.Cells.Count && colIdx < colCount; ci++)
            {
                var cell = row.Cells[ci];
                var cellWidth = colWidths[colIdx];
                if (cell.GridSpan > 1)
                    for (var s = 1; s < cell.GridSpan && colIdx + s < colCount; s++)
                        cellWidth += colWidths[colIdx + s];
                colIdx += cell.GridSpan;
                if (cell.IsVMergeContinue) { cellX += cellWidth; continue; }

                // Per-cell margin overrides
                var effPaddingLeft = cell.CellMarginLeft >= 0 ? cell.CellMarginLeft : table.CellMarginLeft;
                var effPaddingRight = cell.CellMarginRight >= 0 ? cell.CellMarginRight : table.CellMarginRight;
                var effPaddingTop = cell.CellMarginTop >= 0 ? cell.CellMarginTop : cellPaddingV;
                var effPaddingBottom = cell.CellMarginBottom >= 0 ? cell.CellMarginBottom : cellPaddingV;
                var effPaddingH = (effPaddingLeft + effPaddingRight) / 2;
                var effPaddingV = Math.Max(effPaddingTop, effPaddingBottom);

                // Vertical alignment
                float vAlignOffset = 0;
                if (cell.VerticalAlignment != "top")
                {
                    var contentHeight = CalculateCellContentHeight(cell, cellWidth, effPaddingH, effPaddingV, options, table.StyleLineSpacing, table.StyleSpacingAfter);
                    var space = rowHeight - contentHeight;
                    if (space > 0)
                        vAlignOffset = cell.VerticalAlignment == "bottom" ? space : space / 2;
                }

                var textY = y - effPaddingV - vAlignOffset;
                float lastParaSpacingAfter = 0;
                foreach (var para in cell.Paragraphs)
                {
                    // Paragraph spacing before
                    var spBefore = para.SpacingBefore > 0 ? para.SpacingBefore : 0f;
                    if (spBefore > lastParaSpacingAfter)
                        textY -= spBefore - lastParaSpacingAfter;
                    lastParaSpacingAfter = 0;

                    // Render inline images
                    foreach (var image in para.Images)
                    {
                        if (image.IsAnchor) continue;
                        var imgW = image.WidthEmu > 0 ? image.WidthEmu / emuPerPt : 100f;
                        var imgH = image.HeightEmu > 0 ? image.HeightEmu / emuPerPt : 75f;
                        var maxW = cellWidth - effPaddingLeft - effPaddingRight;
                        if (imgW > maxW) { var s = maxW / imgW; imgW *= s; imgH *= s; }
                        var fmt = image.Extension;
                        if (fmt == "jpg" || fmt == "png")
                        {
                            page.AddImage(image.Data, fmt, cellX + effPaddingLeft, textY - imgH, imgW, imgH);
                            textY -= imgH + 1f;
                        }
                    }

                    // Render text
                    var text = AddInterScriptSpacing(string.Concat(para.Runs.Select(r => r.Text)), para.AutoSpaceDE, para.AutoSpaceDN);
                    if (string.IsNullOrEmpty(text)) continue;

                    var fontSize = para.FontSize > 0 ? para.FontSize : options.FontSize;
                    var firstRun = para.Runs.FirstOrDefault(r => !string.IsNullOrEmpty(r.Text));
                    var runFontSize = firstRun?.FontSize > 0 ? firstRun.FontSize : fontSize;
                    float lineHeight;
                    if (para.LineSpacingAbsolute && para.LineSpacing > 0)
                        lineHeight = para.LineSpacing;
                    else
                        lineHeight = runFontSize * GetFontMetricsFactor(firstRun?.FontName) * (para.LineSpacing > 0 ? para.LineSpacing : (table.StyleLineSpacing > 0 ? table.StyleLineSpacing : options.LineSpacing));
                    var textWidth = cellWidth - effPaddingLeft - effPaddingRight;
                    // Use Calibri widths only when the run font is Calibri-like;
                    // wide sans-serif fonts (e.g. Montserrat) need Helvetica-based estimation.
                    var cellUseCalibri = options.UseCalibriWidths
                        && !IsWideSansSerifFont(firstRun?.FontName);
                    s_overrideWidths = GetFontOverrideWidths(firstRun?.FontName);
                    s_wideSansSerifFont = IsWideSansSerifFont(firstRun?.FontName) && s_overrideWidths == null;
                    s_serifRunInCalibri = cellUseCalibri && !s_serifFont && IsSerifFont(firstRun?.FontName);
                    var lines = WordWrap(text, textWidth, textWidth, runFontSize, null,
                        firstRun?.Bold ?? false, firstRun?.CharSpacing ?? 0f, cellUseCalibri);

                    foreach (var line in lines)
                    {
                        textY -= runFontSize;
                        var lineTextWidth = EstimateWrapTextWidth(line, runFontSize,
                            firstRun?.Bold ?? false, firstRun?.CharSpacing ?? 0f, cellUseCalibri);
                        var lineX = para.Alignment switch
                        {
                            "center" => cellX + effPaddingLeft + (textWidth - lineTextWidth) / 2,
                            "right" => cellX + effPaddingLeft + textWidth - lineTextWidth,
                            _ => cellX + effPaddingLeft
                        };

                        float? cellMaxWidth = null;
                        var helveticaWidth = EstimateTextWidth(line, runFontSize, firstRun?.CharSpacing ?? 0f);
                        if (helveticaWidth > textWidth)
                            cellMaxWidth = textWidth;

                        page.AddText(line, lineX, textY, runFontSize, firstRun?.Color ?? para.Color,
                            maxWidth: cellMaxWidth, bold: firstRun?.Bold ?? false, italic: firstRun?.Italic ?? false,
                            preferredFontName: firstRun?.FontName);
                        textY -= lineHeight - runFontSize;
                    }
                    s_overrideWidths = null;
                    s_wideSansSerifFont = false;
                    s_serifRunInCalibri = false;

                    // Paragraph spacing after
                    var spAfter = para.SpacingAfter >= 0 ? para.SpacingAfter : (table.StyleSpacingAfter > 0 ? table.StyleSpacingAfter : 0f);
                    textY -= spAfter;
                    lastParaSpacingAfter = spAfter;
                }

                cellX += cellWidth;
            }

            y -= rowHeight;
        }
        return y;
    }

    // ── Table rendering ─────────────────────────────────────────────────

    private static void RenderTable(RenderState state, DocxTable table)
    {
        var options = state.Options;

        // When a grid-snapped paragraph precedes the table, its final
        // AdvanceY(lineHeight) included the ascent of the *next* line.
        // Tables have no ascent to consume, so pull CurrentY back up.
        if (state.LastGridAscentExcess > 0 && !state.IsTopOfPage)
        {
            state.CurrentY += state.LastGridAscentExcess;
            state.LastGridAscentExcess = 0;
        }

        var usableWidth = state.UsableWidth;
        var effectiveTableWidth = string.IsNullOrEmpty(table.Alignment) || table.Alignment == "left"
            ? usableWidth - table.IndentLeft : usableWidth;
        var cellPaddingH = (table.CellMarginLeft + table.CellMarginRight) / 2;
        var cellPaddingV = Math.Max(1f, Math.Max(table.CellMarginTop, table.CellMarginBottom));

        // Determine column widths
        var colWidths = CalculateTableColumnWidths(table, effectiveTableWidth);
        var colCount = colWidths.Length;

        // Calculate table alignment offset
        var tableWidth = colWidths.Sum();
        var tableOffsetX = table.Alignment switch
        {
            "center" => options.MarginLeft + (usableWidth - tableWidth) / 2,
            "right" => options.MarginLeft + usableWidth - tableWidth,
            _ => options.MarginLeft + table.IndentLeft
        };

        var isFirstRow = true;
        // Pre-calculate row heights for all rows so we can compute vMerge spans
        var rowHeights = new float[table.Rows.Count];
        // Track content-only heights (without trHeight "atLeast" minimum) for
        // page-fit decisions. Word treats trHeight (atLeast) as a visual
        // minimum but allows the row to render at content height when needed
        // to keep the row on the current page.
        var rowContentHeights = new float[table.Rows.Count];
        for (var ri = 0; ri < table.Rows.Count; ri++)
        {
            var r = table.Rows[ri];
            var ch = CalculateRowHeight(r, colWidths, cellPaddingH, cellPaddingV, options, table.StyleLineSpacing, table.StyleSpacingAfter);
            var rh = ch;
            if (r.Height > 0)
            {
                if (r.HeightExact)
                    rh = r.Height;
                else
                {
                    rh = Math.Max(rh, r.Height);
                }
            }
            rowHeights[ri] = rh;
            rowContentHeights[ri] = ch;
        }

        // Distribute vMerge restart cell heights across all merged rows.
        // CalculateRowHeight excludes vMergeRestart cells, so each row height
        // reflects only its non-merged cell content. This pass ensures the
        // merged cell content fits within the sum of spanned row heights.
        for (var ri = 0; ri < table.Rows.Count; ri++)
        {
            var r = table.Rows[ri];
            var colIdx2 = r.GridBefore;
            for (var ci2 = 0; ci2 < r.Cells.Count && colIdx2 < colCount; ci2++)
            {
                var cell2 = r.Cells[ci2];
                if (cell2.IsVMergeRestart)
                {
                    // Count merged rows and sum their current heights
                    var mergedColIdx2 = colIdx2;
                    var spanCount = 1;
                    for (var mr = ri + 1; mr < table.Rows.Count; mr++)
                    {
                        var nci2 = 0;
                        DocxTableCell? mc = null;
                        for (var nc = 0; nc < table.Rows[mr].Cells.Count; nc++)
                        {
                            if (nci2 == mergedColIdx2) { mc = table.Rows[mr].Cells[nc]; break; }
                            nci2 += table.Rows[mr].Cells[nc].GridSpan;
                            if (nci2 > mergedColIdx2) break;
                        }
                        if (mc is { IsVMergeContinue: true })
                            spanCount++;
                        else
                            break;
                    }

                    if (spanCount > 1)
                    {
                        // Calculate merged cell content height
                        var cellW2 = colWidths[colIdx2];
                        for (var s = 1; s < cell2.GridSpan && colIdx2 + s < colCount; s++)
                            cellW2 += colWidths[colIdx2 + s];
                        var mergedContentHeight = CalculateCellContentHeight(cell2, cellW2, cellPaddingH, cellPaddingV, options, table.StyleLineSpacing, table.StyleSpacingAfter);

                        // Sum current heights of all merged rows
                        var currentSum = 0f;
                        for (var mr = ri; mr < ri + spanCount; mr++)
                            currentSum += rowHeights[mr];

                        // If merged content needs more space, distribute excess evenly
                        if (mergedContentHeight > currentSum)
                        {
                            var excess = mergedContentHeight - currentSum;
                            var perRow = excess / spanCount;
                            for (var mr = ri; mr < ri + spanCount; mr++)
                                rowHeights[mr] += perRow;
                        }
                    }
                }
                colIdx2 += cell2.GridSpan;
            }
        }

        for (var rowIndex = 0; rowIndex < table.Rows.Count; rowIndex++)
        {
            var row = table.Rows[rowIndex];
            var rowHeight = rowHeights[rowIndex];
            var isLastRow = rowIndex == table.Rows.Count - 1;

            // Defensive floor for image-heavy rows: ensure inline image extents
            // are always respected, even when upstream row-height hints are small.
            if (!row.HeightExact)
                rowHeight = Math.Max(rowHeight, CalculateRowInlineImageFloorHeight(row, colWidths, cellPaddingH, cellPaddingV));

            state.EnsurePage();

            // Check if row fits on current page.
            // For non-exact-height rows, trHeight acts as an "atLeast" minimum;
            // Word allows the row to shrink to content height in order to keep
            // it on the current page when the visual minimum would force a
            // new page. Mirror that behavior so single tall template rows
            // (like newsletter sidebars) don't get pushed to a blank page.
            if (state.CurrentY - rowHeight < options.MarginBottom)
            {
                var contentOnly = rowContentHeights[rowIndex];
                if (!row.HeightExact)
                    contentOnly = Math.Max(contentOnly, CalculateRowInlineImageFloorHeight(row, colWidths, cellPaddingH, cellPaddingV));
                if (!row.HeightExact && row.Height > 0
                    && contentOnly < rowHeight
                    && state.CurrentY - contentOnly >= options.MarginBottom)
                {
                    rowHeight = contentOnly;
                }
                else
                {
                    state.RenderPageFootnotes();
                    state.CurrentPageFootnoteIds.Clear();
                    state.FootnoteReservedHeight = 0;
                    options.MarginBottom = state.BaseMarginBottom;
                    state.CurrentPage = state.Doc.AddPage(options.PageWidth, options.PageHeight);
                    state.CurrentY = options.PageHeight - options.MarginTop;
                    isFirstRow = true; // new page: draw top border again
                }
            }

            var cellX = tableOffsetX;
            var colIdx = row.GridBefore;
            // Advance cellX past skipped grid columns
            for (var gb = 0; gb < row.GridBefore && gb < colCount; gb++)
                cellX += colWidths[gb];

            for (var ci = 0; ci < row.Cells.Count && colIdx < colCount; ci++)
            {
                var cell = row.Cells[ci];
                var cellWidth = colWidths[colIdx];

                // Handle grid span
                if (cell.GridSpan > 1)
                {
                    for (var s = 1; s < cell.GridSpan && colIdx + s < colCount; s++)
                        cellWidth += colWidths[colIdx + s];
                }

                // Advance column index past spanned columns
                colIdx += cell.GridSpan;

                // Skip rendering content for vertically merged continuation cells
                if (cell.IsVMergeContinue)
                {
                    cellX += cellWidth;
                    continue;
                }

                // For vMerge restart cells, calculate the total height spanning all merged rows
                var cellRenderHeight = rowHeight;
                if (cell.IsVMergeRestart)
                {
                    var mergedColIdx = colIdx - cell.GridSpan; // column index for this cell
                    for (var mr = rowIndex + 1; mr < table.Rows.Count; mr++)
                    {
                        // Find the cell at the same column position in the next row
                        var nextRow = table.Rows[mr];
                        var nci = 0;
                        DocxTableCell? mergedCell = null;
                        for (var nc = 0; nc < nextRow.Cells.Count; nc++)
                        {
                            if (nci == mergedColIdx) { mergedCell = nextRow.Cells[nc]; break; }
                            nci += nextRow.Cells[nc].GridSpan;
                            if (nci > mergedColIdx) break;
                        }
                        if (mergedCell is { IsVMergeContinue: true })
                            cellRenderHeight += rowHeights[mr];
                        else
                            break;
                    }
                }

                // Draw cell shading
                if (cell.Shading != null)
                {
                    state.CurrentPage!.AddRectangle(cellX, state.CurrentY - cellRenderHeight, cellWidth, cellRenderHeight, cell.Shading);
                }

                // Apply vertical alignment offset
                // Per-cell margin overrides
                var effCellLeft = cell.CellMarginLeft >= 0 ? cell.CellMarginLeft : table.CellMarginLeft;
                var effCellRight = cell.CellMarginRight >= 0 ? cell.CellMarginRight : table.CellMarginRight;
                var effCellTop = cell.CellMarginTop >= 0 ? cell.CellMarginTop : cellPaddingV;
                var effCellBottom = cell.CellMarginBottom >= 0 ? cell.CellMarginBottom : cellPaddingV;
                var effCellPaddingV = Math.Max(effCellTop, effCellBottom);

                float vAlignOffset = 0;
                if (cell.VerticalAlignment != "top")
                {
                    var contentHeight = CalculateCellContentHeight(cell, cellWidth, (effCellLeft + effCellRight) / 2, effCellPaddingV, options, table.StyleLineSpacing, table.StyleSpacingAfter);
                    var space = cellRenderHeight - contentHeight;
                    if (space > 0)
                        vAlignOffset = cell.VerticalAlignment == "bottom" ? space : space / 2;
                }

                // Render cell content (images and text)
                var textY = state.CurrentY - effCellPaddingV - vAlignOffset;
                var cellParaList = cell.Paragraphs;
                for (var cellParaIdx = 0; cellParaIdx < cellParaList.Count; cellParaIdx++)
                {
                    var para = cellParaList[cellParaIdx];
                    var isFirstCellPara = cellParaIdx == 0;
                    var isLastCellPara = cellParaIdx == cellParaList.Count - 1;
                    // Skip SpacingBefore for the first paragraph in a table cell
                    // (Word default compat mode does not apply SpacingBefore to the first cell paragraph)
                    if (!isFirstCellPara && para.SpacingBefore > 0)
                        textY -= para.SpacingBefore;

                    // Render images inside table cells
                    const float emuPerPt = 914400f / 72f;
                    var cellHasInlineImages = false;
                    foreach (var image in para.Images)
                    {
                        if (image.IsAnchor) continue; // Skip anchor images in cell flow
                        cellHasInlineImages = true;
                        var imgW = image.WidthEmu > 0 ? image.WidthEmu / emuPerPt : 100f;
                        var imgH = image.HeightEmu > 0 ? image.HeightEmu / emuPerPt : 75f;
                        var maxImgW = cellWidth - effCellLeft - effCellRight;
                        if (imgW > maxImgW)
                        {
                            var s = maxImgW / imgW;
                            imgW *= s;
                            imgH *= s;
                        }
                        var fmt = image.Extension;
                        if (fmt == "jpg" || fmt == "png")
                        {
                            var imgY = textY - imgH;
                            state.CurrentPage!.AddImage(image.Data, fmt, cellX + effCellLeft, imgY, imgW, imgH);
                            textY -= imgH + 1f;
                        }
                    }

                    var fontSize = para.FontSize > 0 ? para.FontSize : options.FontSize;
                    var text = AddInterScriptSpacing(string.Concat(para.Runs.Select(r => r.Text)), para.AutoSpaceDE, para.AutoSpaceDN);
                    if (string.IsNullOrEmpty(text))
                    {
                        // Empty paragraph still takes up space (skip when images already rendered)
                        if (cellHasInlineImages) continue;
                        float emptyLineH;
                        if (para.LineSpacingAbsolute && para.LineSpacing > 0)
                            emptyLineH = para.LineSpacing;
                        else
                        {
                            var emptyRunFont = para.Runs.FirstOrDefault(r => !string.IsNullOrEmpty(r.FontName))?.FontName;
                            emptyLineH = fontSize * GetFontMetricsFactor(emptyRunFont) * (para.LineSpacing > 0 ? para.LineSpacing : (table.StyleLineSpacing > 0 ? table.StyleLineSpacing : options.LineSpacing));
                        }
                        // Snap empty paragraph line height to document grid when active
                        if (options.GridLinePitch > 0 && para.SnapToGrid && !(para.LineSpacingAbsolute && para.LineSpacingExact))
                        {
                            var gridPitch = options.GridLinePitch;
                            emptyLineH = Math.Max(gridPitch, Compat.Ceiling(emptyLineH / gridPitch) * gridPitch);
                        }
                        textY -= emptyLineH;
                        // Apply SpacingAfter: table style override takes precedence, else paragraph's own
                        // Skip SpacingAfter for the last paragraph in the cell (Word behaviour)
                        if (!isLastCellPara)
                        {
                            var spAfter = table.StyleSpacingAfter >= 0 ? table.StyleSpacingAfter : (para.SpacingAfter > 0 ? para.SpacingAfter : 0f);
                            if (spAfter > 0) textY -= spAfter;
                        }
                        continue;
                    }

                    var dominantRun = para.Runs.FirstOrDefault(r => !string.IsNullOrEmpty(r.Text));
                    var runFontSize = dominantRun?.FontSize > 0 ? dominantRun.FontSize : fontSize;
                    var effectiveFontSize = runFontSize;
                    var runColor = dominantRun?.Color ?? para.Color;
                    var cellRunBold = dominantRun?.Bold ?? false;
                    var cellRunItalic = dominantRun?.Italic ?? false;
                    var cellRunUnderline = dominantRun?.Underline ?? false;
                    var cellRunCharSpacing = dominantRun?.CharSpacing ?? 0f;
                    var cellRunFontName = dominantRun?.FontName;
                    float lineHeight;
                    if (para.LineSpacingAbsolute && para.LineSpacing > 0)
                        lineHeight = para.LineSpacing;
                    else
                        lineHeight = effectiveFontSize * GetFontMetricsFactor(cellRunFontName) * (para.LineSpacing > 0 ? para.LineSpacing : (table.StyleLineSpacing > 0 ? table.StyleLineSpacing : options.LineSpacing));

                    // Snap line height to document grid when active (CJK line grid)
                    if (options.GridLinePitch > 0 && para.SnapToGrid && !(para.LineSpacingAbsolute && para.LineSpacingExact))
                    {
                        var gridPitch = options.GridLinePitch;
                        lineHeight = Math.Max(gridPitch, Compat.Ceiling(lineHeight / gridPitch) * gridPitch);
                    }

                    var textWidth = cellWidth - effCellLeft - effCellRight;
                    // Add small CJK tolerance: CJK fonts may render fullwidth characters
                    // slightly narrower than the 1000-unit estimate, preventing unnecessary wraps.
                    var wrapWidth = textWidth + 0.5f;
                    // Use Calibri widths only when the run font is Calibri-like;
                    // wide sans-serif fonts (e.g. Montserrat) need Helvetica-based estimation.
                    var cellUseCalibri = options.UseCalibriWidths
                        && !IsWideSansSerifFont(cellRunFontName);
                    s_overrideWidths = GetFontOverrideWidths(cellRunFontName);
                    s_wideSansSerifFont = IsWideSansSerifFont(cellRunFontName) && s_overrideWidths == null;
                    s_serifRunInCalibri = cellUseCalibri && !s_serifFont && IsSerifFont(cellRunFontName);
                    var lines = WordWrap(text, wrapWidth, wrapWidth, effectiveFontSize, null, cellRunBold, cellRunCharSpacing, cellUseCalibri);

                    foreach (var line in lines)
                    {
                        textY -= effectiveFontSize;
                        if (textY < state.CurrentY - cellRenderHeight + effCellPaddingV) break; // clip
                        var lineTextWidth = EstimateWrapTextWidth(line, effectiveFontSize, cellRunBold, cellRunCharSpacing, cellUseCalibri);
                        var lineRenderX = para.Alignment switch
                        {
                            "center" => cellX + effCellLeft + (textWidth - lineTextWidth) / 2,
                            "right" => cellX + effCellLeft + textWidth - lineTextWidth,
                            _ => cellX + effCellLeft
                        };

                        // Use Tz compression to fit Helvetica rendering within cell boundaries
                        // when the estimated width (using non-Helvetica metrics) is narrower than
                        // what Helvetica would actually render. Also compress when the Helvetica
                        // rendering exceeds the cell text width.
                        float? cellMaxWidth = null;
                        var helveticaWidth = EstimateTextWidth(line, effectiveFontSize, cellRunCharSpacing);
                        if (helveticaWidth > textWidth)
                            cellMaxWidth = textWidth;

                        // Calculate word spacing for justified text in table cells
                        float cellWordSpacing = 0;
                        if (para.Alignment == "both" && line != lines[^1])
                        {
                            var spaceCount = line.Count(c => c == ' ');
                            if (spaceCount > 0)
                            {
                                var extraSpace = textWidth - lineTextWidth;
                                if (extraSpace > 0)
                                    cellWordSpacing = extraSpace / spaceCount;
                            }
                        }

                        state.CurrentPage!.AddText(line, lineRenderX, textY, effectiveFontSize, runColor, maxWidth: cellMaxWidth, bold: cellRunBold, italic: cellRunItalic, underline: cellRunUnderline, charSpacing: cellRunCharSpacing, wordSpacing: cellWordSpacing, preferredFontName: cellRunFontName);
                        textY -= lineHeight - effectiveFontSize;
                    }
                    s_overrideWidths = null;
                    s_wideSansSerifFont = false;
                    s_serifRunInCalibri = false;

                    // Apply SpacingAfter: table style override takes precedence, else paragraph's own
                    // Skip SpacingAfter for the last paragraph in the cell (Word behaviour)
                    if (!isLastCellPara)
                    {
                        var spAfter = table.StyleSpacingAfter >= 0 ? table.StyleSpacingAfter : (para.SpacingAfter > 0 ? para.SpacingAfter : 0f);
                        if (spAfter > 0) textY -= spAfter;
                    }
                }

                cellX += cellWidth;
            }

            // Draw per-cell borders with table-level fallback for unspecified edges
            {
                var rowTop = state.CurrentY;
                var rowBottom = state.CurrentY - rowHeight;
                var bx = tableOffsetX;
                var bci = 0;

                foreach (var cell in row.Cells)
                {
                    if (bci >= colWidths.Length) break;
                    var bCellWidth = colWidths[bci];
                    if (cell.GridSpan > 1)
                        for (var g = 1; g < cell.GridSpan && bci + g < colWidths.Length; g++)
                            bCellWidth += colWidths[bci + g];

                    var cellGridEnd = bci + (cell.GridSpan > 1 ? cell.GridSpan : 1);
                    var isFirstCell = bci == 0;
                    var isLastCell = cellGridEnd >= colCount;
                    var borders = cell.Borders;

                    // Suppress horizontal borders inside vertically-merged cell groups:
                    // - vMerge continue cells: suppress top border (internal to merge)
                    // - Any cell whose next row continues the merge at the same column: suppress bottom border
                    var suppressTop = cell.IsVMergeContinue;
                    var suppressBottom = false;
                    if (cell.IsVMergeRestart || cell.IsVMergeContinue)
                    {
                        // Check if the next row has a vMerge continue at the same column
                        var nextRowIdx = rowIndex + 1;
                        if (nextRowIdx < table.Rows.Count)
                        {
                            var nci = 0;
                            foreach (var nc in table.Rows[nextRowIdx].Cells)
                            {
                                if (nci == bci) { suppressBottom = nc.IsVMergeContinue; break; }
                                nci += nc.GridSpan;
                                if (nci > bci) break;
                            }
                        }
                    }

                    // Resolve each border: prefer cell-level, fall back to table-level
                    DocxBorderEdge? topBorder, bottomBorder, leftBorder, rightBorder;
                    if (borders != null)
                    {
                        topBorder = suppressTop ? null : (borders.Top ?? (table.HasBorders ? (isFirstRow ? table.BorderTop : table.BorderInsideH) : null));
                        bottomBorder = suppressBottom ? null : (borders.Bottom ?? (table.HasBorders ? (isLastRow ? table.BorderBottom : table.BorderInsideH) : null));
                        leftBorder = borders.Left ?? (table.HasBorders ? (isFirstCell ? table.BorderLeft : table.BorderInsideV) : null);
                        rightBorder = borders.Right ?? (table.HasBorders ? (isLastCell ? table.BorderRight : table.BorderInsideV) : null);
                    }
                    else if (table.HasBorders)
                    {
                        topBorder = suppressTop ? null : (isFirstRow ? table.BorderTop : table.BorderInsideH);
                        bottomBorder = suppressBottom ? null : (isLastRow ? table.BorderBottom : table.BorderInsideH);
                        leftBorder = isFirstCell ? table.BorderLeft : table.BorderInsideV;
                        rightBorder = isLastCell ? table.BorderRight : table.BorderInsideV;
                    }
                    else
                    {
                        topBorder = bottomBorder = leftBorder = rightBorder = null;
                    }

                    if (topBorder != null)
                        state.CurrentPage!.AddLine(bx, rowTop, bx + bCellWidth, rowTop, topBorder.Color, topBorder.Width);
                    if (bottomBorder != null)
                        state.CurrentPage!.AddLine(bx, rowBottom, bx + bCellWidth, rowBottom, bottomBorder.Color, bottomBorder.Width);
                    if (leftBorder != null)
                        state.CurrentPage!.AddLine(bx, rowTop, bx, rowBottom, leftBorder.Color, leftBorder.Width);
                    if (rightBorder != null)
                        state.CurrentPage!.AddLine(bx + bCellWidth, rowTop, bx + bCellWidth, rowBottom, rightBorder.Color, rightBorder.Width);

                    bx += bCellWidth;
                    bci += cell.GridSpan > 1 ? cell.GridSpan : 1;
                }
            }

            isFirstRow = false;
            state.AdvanceY(rowHeight);
        }

        // After table, add spacing to prevent the next paragraph's text from
        // overlapping with table cell content.  Table cells position the first
        // text baseline at (cellTop - fontSize), leaving only
        // (lineHeight - fontSize + cellPaddingV) between the last baseline and
        // the row bottom.  For text-only rows the gap can be as small as ~1 pt,
        // while image-heavy rows naturally provide a large gap.  Compute the
        // natural gap for the last row and add a deficit only when needed.
        if (table.Rows.Count > 0)
        {
            var lastRowIdx = table.Rows.Count - 1;
            var lastRow = table.Rows[lastRowIdx];
            var lastRowHeight = rowHeights[lastRowIdx];
            float maxTextContentH = 0;
            var colIdx2 = lastRow.GridBefore;
            for (var ci = 0; ci < lastRow.Cells.Count && colIdx2 < colWidths.Length; ci++)
            {
                var cell = lastRow.Cells[ci];
                var span = cell.GridSpan;
                var cw = colWidths[colIdx2];
                for (var s = 1; s < span && colIdx2 + s < colWidths.Length; s++)
                    cw += colWidths[colIdx2 + s];
                colIdx2 += span;
                if (!cell.IsVMergeContinue)
                {
                    // Only consider cells that contain visible text –
                    // image-only cells do not contribute a text baseline
                    // near the row bottom and should not shrink the gap.
                    var hasText = false;
                    foreach (var p in cell.Paragraphs)
                        foreach (var r in p.Runs)
                            if (!string.IsNullOrEmpty(r.Text)) { hasText = true; break; }
                    if (hasText)
                    {
                        var ch = CalculateCellContentHeight(cell, cw, cellPaddingH, cellPaddingV, options, table.StyleLineSpacing, table.StyleSpacingAfter);
                        maxTextContentH = Math.Max(maxTextContentH, ch);
                    }
                }
            }
            // naturalGap: distance from the tallest *text* cell's content
            // to the row bottom (rowHeight - textContentHeight + cellPaddingV).
            // Image-only cells are excluded so they don't mask a tight text fit.
            var naturalGap = lastRowHeight - maxTextContentH + cellPaddingV;
            // The next paragraph draws text at the baseline (state.CurrentY) and
            // the visual top extends upward by ~fontSize*0.6 (font ascent).  Use
            // fontSize*0.8 as the floor so the text visual top clears the table
            // bottom border with a small gap (~2 pt), matching Word behaviour.
            var postTableGap = Math.Max(options.FontSize * 0.8f, options.FontSize - naturalGap);
            state.AdvanceY(postTableGap);
        }
        else
        {
            state.AdvanceY(2f);
        }

        // Reset spacing context after table so the next paragraph's SpacingBefore
        // is not collapsed with the pre-table paragraph's SpacingAfter.
        state.LastSpacingAfter = 0;
    }

    private static float[] CalculateTableColumnWidths(DocxTable table, float usableWidth)
    {
        if (table.ColumnWidths.Count > 0)
        {
            var widths = table.ColumnWidths.ToArray();
            var total = widths.Sum();
            if (total <= 0)
            {
                // No valid widths, distribute evenly
                var maxCols = table.Rows.Count > 0 ? table.Rows.Max(r => r.Cells.Count) : 1;
                var cw = usableWidth / maxCols;
                var res = new float[maxCols];
                Compat.ArrayFill(res, cw);
                return res;
            }
            // Use actual DOCX widths; scale down proportionally only when they exceed
            // the usable width by more than a small tolerance. Word lets modestly
            // overflowing tables extend past the right margin instead of shrinking
            // declared column widths (e.g. layouts that intentionally overhang the
            // text area to bleed an image past the margin).
            if (total > usableWidth * 1.05f)
            {
                var scale = usableWidth / total;
                for (var i = 0; i < widths.Length; i++)
                    widths[i] *= scale;
            }
            return widths;
        }

        // Determine from cell count
        var maxCols2 = table.Rows.Count > 0 ? table.Rows.Max(r => r.Cells.Count) : 1;
        var colWidth = usableWidth / maxCols2;
        var result = new float[maxCols2];
        Compat.ArrayFill(result, colWidth);
        return result;
    }

    private static float CalculateCellContentHeight(DocxTableCell cell, float cellWidth, float cellPaddingH, float cellPaddingV, ConversionOptions options, float styleLineSpacing = -1, float styleSpacingAfter = -1)
    {
        // Honor cell-level top/bottom margin overrides so the calculated
        // content height matches the renderer's effective padding (which
        // uses Math.Max of the cell's own top/bottom margins). Without
        // this, cells with explicit larger margins underestimate height
        // and trigger spurious end-of-cell clipping.
        var effCellTop = cell.CellMarginTop >= 0 ? cell.CellMarginTop : cellPaddingV;
        var effCellBottom = cell.CellMarginBottom >= 0 ? cell.CellMarginBottom : cellPaddingV;
        var effCellPaddingV = Math.Max(effCellTop, effCellBottom);
        var cellHeight = effCellPaddingV * 2;
        var cellParas = cell.Paragraphs;
        for (var pi = 0; pi < cellParas.Count; pi++)
        {
            var para = cellParas[pi];
            var isFirstPara = pi == 0;
            var isLastPara = pi == cellParas.Count - 1;

            // Skip SpacingBefore for the first paragraph in a table cell
            // (Word default compat mode does not apply SpacingBefore to the first cell paragraph)
            if (!isFirstPara && para.SpacingBefore > 0)
                cellHeight += para.SpacingBefore;

            const float emuPerPt = 914400f / 72f;
            var hasInlineImages = false;
            foreach (var image in para.Images)
            {
                if (image.IsAnchor) continue;
                hasInlineImages = true;
                var imgW = image.WidthEmu > 0 ? image.WidthEmu / emuPerPt : 100f;
                var imgH = image.HeightEmu > 0 ? image.HeightEmu / emuPerPt : 75f;
                var maxImgW = cellWidth - cellPaddingH * 2;
                if (imgW > maxImgW)
                    imgH *= maxImgW / imgW;
                cellHeight += imgH + 1f;
            }

            var fontSize = para.FontSize > 0 ? para.FontSize : options.FontSize;
            var dominantRun = para.Runs.FirstOrDefault(r => !string.IsNullOrEmpty(r.Text));
            var runFontSize = dominantRun?.FontSize > 0 ? dominantRun.FontSize : fontSize;
            var runCharSpacing = dominantRun?.CharSpacing ?? 0f;
            var runBold = dominantRun?.Bold ?? false;
            float lineHeight;
            if (para.LineSpacingAbsolute && para.LineSpacing > 0)
                lineHeight = para.LineSpacing; // exact/atLeast: absolute points
            else
                lineHeight = runFontSize * GetFontMetricsFactor(dominantRun?.FontName) * (para.LineSpacing > 0 ? para.LineSpacing : (styleLineSpacing > 0 ? styleLineSpacing : options.LineSpacing));

            // Snap line height to document grid when active (CJK line grid)
            if (options.GridLinePitch > 0 && para.SnapToGrid && !(para.LineSpacingAbsolute && para.LineSpacingExact))
            {
                var gridPitch = options.GridLinePitch;
                lineHeight = Math.Max(gridPitch, Compat.Ceiling(lineHeight / gridPitch) * gridPitch);
            }

            var textWidth = cellWidth - cellPaddingH * 2;
            var text = AddInterScriptSpacing(string.Concat(para.Runs.Select(r => r.Text)), para.AutoSpaceDE, para.AutoSpaceDN);

            if (string.IsNullOrEmpty(text))
            {
                // Only add line height when no inline images were rendered
                // (inline images already account for the paragraph's vertical space)
                if (!hasInlineImages)
                    cellHeight += lineHeight;
                // Apply SpacingAfter for all non-last paragraphs (matching rendering logic)
                if (!isLastPara)
                {
                    var emptyAfter = styleSpacingAfter >= 0 ? styleSpacingAfter : (para.SpacingAfter > 0 ? para.SpacingAfter : 0f);
                    if (emptyAfter > 0)
                        cellHeight += emptyAfter;
                }
                continue;
            }

            // Use Calibri widths only when the run font is Calibri-like;
            // wide sans-serif fonts (e.g. Montserrat) need Helvetica-based estimation.
            var cellUseCalibri = options.UseCalibriWidths
                && !IsWideSansSerifFont(dominantRun?.FontName);
            s_overrideWidths = GetFontOverrideWidths(dominantRun?.FontName);
            s_wideSansSerifFont = IsWideSansSerifFont(dominantRun?.FontName) && s_overrideWidths == null;
            s_serifRunInCalibri = cellUseCalibri && !s_serifFont && IsSerifFont(dominantRun?.FontName);
            var lines = WordWrap(text, textWidth, textWidth, runFontSize, null, runBold, runCharSpacing, cellUseCalibri);
            s_overrideWidths = null;
            s_wideSansSerifFont = false;
            s_serifRunInCalibri = false;
            cellHeight += lines.Count * lineHeight;
            // Apply SpacingAfter for all non-last paragraphs (matching rendering logic)
            if (!isLastPara)
            {
                var textAfter = styleSpacingAfter >= 0 ? styleSpacingAfter : (para.SpacingAfter > 0 ? para.SpacingAfter : 0f);
                if (textAfter > 0)
                    cellHeight += textAfter;
            }
        }

        return cellHeight;
    }

    private static float CalculateRowHeight(DocxTableRow row, float[] colWidths, float cellPaddingH, float cellPaddingV, ConversionOptions options, float styleLineSpacing = -1, float styleSpacingAfter = -1)
    {
        // Use table style line spacing for minimum row height
        // (tables often define single spacing via style, e.g. TableGrid line=240).
        var minLineSpacing = options.LineSpacing;
        if (styleLineSpacing > 0)
            minLineSpacing = styleLineSpacing;
        else if (row.Cells.Count > 0 && row.Cells[0].Paragraphs.Count > 0)
        {
            var firstPara = row.Cells[0].Paragraphs[0];
            if (firstPara.LineSpacing > 0 && !firstPara.LineSpacingAbsolute)
                minLineSpacing = firstPara.LineSpacing;
        }
        var maxHeight = options.FontSize * FontMetricsFactor * minLineSpacing + cellPaddingV * 2;

        var colIdx = row.GridBefore;
        for (var cellIdx = 0; cellIdx < row.Cells.Count && colIdx < colWidths.Length; cellIdx++)
        {
            var cell = row.Cells[cellIdx];
            var span = cell.GridSpan;

            var cellWidth = colWidths[colIdx];
            for (var s = 1; s < span && colIdx + s < colWidths.Length; s++)
                cellWidth += colWidths[colIdx + s];

            colIdx += span;

            if (cell.IsVMergeContinue || cell.IsVMergeRestart)
                continue;

            var cellHeight = CalculateCellContentHeight(cell, cellWidth, cellPaddingH, cellPaddingV, options, styleLineSpacing, styleSpacingAfter);
            maxHeight = Math.Max(maxHeight, cellHeight);
        }

        return maxHeight;
    }

    private static float CalculateRowInlineImageFloorHeight(DocxTableRow row, float[] colWidths, float cellPaddingH, float cellPaddingV)
    {
        const float emuPerPt = 914400f / 72f;
        var maxHeight = 0f;
        var colIdx = row.GridBefore;

        for (var cellIdx = 0; cellIdx < row.Cells.Count && colIdx < colWidths.Length; cellIdx++)
        {
            var cell = row.Cells[cellIdx];
            var span = Math.Max(1, cell.GridSpan);

            var cellWidth = colWidths[colIdx];
            for (var s = 1; s < span && colIdx + s < colWidths.Length; s++)
                cellWidth += colWidths[colIdx + s];

            colIdx += span;

            if (cell.IsVMergeContinue)
                continue;

            var cellImageHeight = 0f;
            foreach (var para in cell.Paragraphs)
            {
                foreach (var image in para.Images)
                {
                    if (image.IsAnchor)
                        continue;

                    var imgW = image.WidthEmu > 0 ? image.WidthEmu / emuPerPt : 100f;
                    var imgH = image.HeightEmu > 0 ? image.HeightEmu / emuPerPt : 75f;
                    var maxImgW = Math.Max(1f, cellWidth - cellPaddingH * 2);
                    if (imgW > maxImgW)
                        imgH *= maxImgW / imgW;

                    cellImageHeight += imgH + 1f;
                }
            }

            if (cellImageHeight > 0)
                maxHeight = Math.Max(maxHeight, cellImageHeight + cellPaddingV * 2);
        }

        return maxHeight;
    }

    // ── Word wrapping ───────────────────────────────────────────────────

    private static string ExpandTabs(string text, float fontSize, List<DocxTabStop>? tabStops = null, bool useCalibriWidths = true, float currentXOffset = 0f)
    {
        if (!text.Contains('\t'))
            return text;

        // If tab stops define dot leaders, use them
        if (tabStops is { Count: > 0 })
        {
            var sb = new System.Text.StringBuilder();
            var segments = text.Split('\t');
            for (var i = 0; i < segments.Length; i++)
            {
                sb.Append(segments[i]);

                if (i < segments.Length - 1)
                {
                    // Find the next tab stop beyond current text width
                    var currentLineWidth = currentXOffset + EstimateTextWidth(sb.ToString(), fontSize);
                    DocxTabStop? matchedStop = null;
                    foreach (var ts in tabStops)
                    {
                        if (ts.Position > currentLineWidth)
                        {
                            matchedStop = ts;
                            break;
                        }
                    }

                    if (matchedStop != null)
                    {
                        var leaderChar = matchedStop.Leader switch
                        {
                            "dot" => '.',
                            "hyphen" => '-',
                            "underscore" => '_',
                            _ => ' '
                        };
                        // Use Calibri-equivalent scale so the dot count matches
                        // LibreOffice output (Calibri dots are narrower than Helvetica).
                        // The rendered line is compressed via Tz to fit the tab position.
                        var leaderCharWidth = fontSize * GetHelveticaCharWidth(leaderChar) / 1000f * 0.725f;
                        // Account for text after this tab when computing fill
                        var remainingTextWidth = i + 1 < segments.Length
                            ? EstimateTextWidth(segments[i + 1], fontSize)
                            : 0f;
                        var gapWidth = matchedStop.Position - currentLineWidth - remainingTextWidth;
                        var fillCount = Math.Max(1, (int)(gapWidth / leaderCharWidth));
                        sb.Append(leaderChar, fillCount);
                    }
                    else
                    {
                        // No matching tab stop; use default spacing
                        sb.Append(' ', 4);
                    }
                }
            }
            return sb.ToString();
        }

        const float defaultTabStopPt = 36f; // 0.5 inch default tab stop in points
        var spaceWidth = fontSize * GetWrapCharWidth(' ', useCalibriWidths) / 1000f;
        var sb2 = new System.Text.StringBuilder();
        float currentWidth = 0;
        foreach (var ch in text)
        {
            if (ch == '\t')
            {
                var nextStop = (float)(Math.Floor(currentWidth / defaultTabStopPt) + 1) * defaultTabStopPt;
                var gapWidth = Math.Max(spaceWidth, nextStop - currentWidth);
                var spaces = Math.Max(1, (int)Math.Ceiling(gapWidth / spaceWidth));
                sb2.Append(' ', spaces);
                currentWidth += spaces * spaceWidth;
            }
            else
            {
                sb2.Append(ch);
                currentWidth += fontSize * GetWrapCharWidth(ch, useCalibriWidths) / 1000f;
            }
        }
        return sb2.ToString();
    }

    private static List<string> WordWrap(string text, float firstLineWidth, float subsequentWidth, float fontSize, List<DocxTabStop>? tabStops = null, bool bold = false, float charSpacing = 0, bool useCalibriWidths = true)
    {
        if (string.IsNullOrEmpty(text))
            return [""];

        // When tab stops exceed available width, extend effective line width
        bool hasDefaultTabs = (tabStops is null or { Count: 0 }) && text.Contains('\t');
        if (tabStops is { Count: > 0 })
        {
            var maxTabPos = tabStops.Max(ts => ts.Position);
            // Scale up to account for Calibri-scaled dot expansion: ExpandTabs
            // produces more dots (for text extraction matching), and those dots
            // render wider in Helvetica. Prevent WordWrap from splitting.
            var expandedWidth = tabStops.Any(ts => ts.Leader is "dot" or "hyphen" or "underscore")
                ? maxTabPos / 0.725f
                : maxTabPos;
            firstLineWidth = Math.Max(firstLineWidth, expandedWidth);
            subsequentWidth = Math.Max(subsequentWidth, expandedWidth);
        }

        text = ExpandTabs(text, fontSize, tabStops, useCalibriWidths);

        // For default tab expansion (no explicit tabStops), extend line width
        // to prevent wrapping of tab-aligned content (Word doesn't wrap these)
        if (hasDefaultTabs)
        {
            var expandedWidth = EstimateWrapTextWidth(text, fontSize, bold, charSpacing, useCalibriWidths);
            firstLineWidth = Math.Max(firstLineWidth, expandedWidth);
            subsequentWidth = Math.Max(subsequentWidth, expandedWidth);
        }

        var lines = new List<string>();
        var paragraphLines = text.Split('\n');

        foreach (var pLine in paragraphLines)
        {
            if (string.IsNullOrEmpty(pLine))
            {
                lines.Add("");
                continue;
            }

            // Preserve leading spaces (e.g., code indentation in <w:br/>-separated blocks)
            var trimmedLine = pLine.TrimStart(' ');
            var leadingSpaceCount = pLine.Length - trimmedLine.Length;
            var words = trimmedLine.Split(' ');
            var currentLine = leadingSpaceCount > 0 ? new string(' ', leadingSpaceCount) : "";
            var maxWidth = lines.Count == 0 ? firstLineWidth : subsequentWidth;

            foreach (var word in words)
            {
                if (currentLine.Length == 0)
                {
                    currentLine = word;
                }
                else if (EstimateWrapTextWidth(currentLine + " " + word, fontSize, bold, charSpacing, useCalibriWidths) <= maxWidth)
                {
                    currentLine += " " + word;
                }
                else
                {
                    // Try breaking the word at hyphens before wrapping to the next line
                    var wrapped = false;
                    if (word.Contains('-'))
                    {
                        var parts = word.Split('-');
                        for (var pi = parts.Length - 1; pi >= 1; pi--)
                        {
                            var prefix = string.Join("-", parts.Take(pi)) + "-";
                            var candidate = currentLine.Length > 0 ? currentLine + " " + prefix : prefix;
                            if (EstimateWrapTextWidth(candidate, fontSize, bold, charSpacing, useCalibriWidths) <= maxWidth)
                            {
                                lines.Add(candidate);
                                currentLine = string.Join("-", parts.Skip(pi));
                                maxWidth = subsequentWidth;
                                wrapped = true;
                                break;
                            }
                        }
                    }
                    if (!wrapped)
                    {
                        lines.Add(currentLine);
                        currentLine = word;
                        maxWidth = subsequentWidth;
                    }
                }

                // Break oversized words only at CJK character boundaries
                while (EstimateWrapTextWidth(currentLine, fontSize, bold, charSpacing, useCalibriWidths) > maxWidth && currentLine.Length > 1)
                {
                    // Find the latest CJK or hyphen break point that fits, respecting kinsoku rules
                    var breakAt = -1;
                    for (var ci = 1; ci < currentLine.Length; ci++)
                    {
                        if (EstimateWrapTextWidth(currentLine[..ci], fontSize, bold, charSpacing, useCalibriWidths) > maxWidth)
                            break;
                        // Allow breaking before or after a CJK character
                        if (GetWrapCharWidth(currentLine[ci], useCalibriWidths) == 1000 || GetWrapCharWidth(currentLine[ci - 1], useCalibriWidths) == 1000)
                        {
                            // Kinsoku: don't break before closing/trailing punctuation
                            if (!IsNoStartChar(currentLine[ci]))
                                breakAt = ci;
                        }
                        // Allow breaking after a hyphen (e.g. "020-88888888" → "020-" + "88888888")
                        else if (currentLine[ci - 1] == '-' && ci > 1)
                        {
                            breakAt = ci;
                        }
                    }
                    if (breakAt <= 0) break; // No break point found
                    lines.Add(currentLine[..breakAt]);
                    currentLine = currentLine[breakAt..];
                    maxWidth = subsequentWidth;
                }
            }

            if (currentLine.Length > 0)
                lines.Add(currentLine);
        }

        if (lines.Count == 0)
            lines.Add("");

        return lines;
    }

    /// <summary>
    /// Computes a dynamic wrap-width scale for justified numbered paragraphs.
    /// Heuristics are based on first-line length and punctuation density.
    /// </summary>
    private static float GetDynamicNumberedWrapScale(DocxParagraph paragraph)
    {
        const float baseScale = 0.982f;
        var text = string.Concat(paragraph.Runs.Select(r => r.Text));
        if (string.IsNullOrWhiteSpace(text))
            return baseScale;

        var normalized = text.Replace("\r", "").Trim();
        var firstLine = normalized.Split('\n')[0];
        var firstLineLen = firstLine.Length;
        var totalLen = normalized.Length;

        // Focus on punctuation that tends to produce earlier wraps in reference output.
        var punctCount = normalized.Count(c => c == ',' || c == ';' || c == ':' || c == '/');
        var punctDensity = (float)punctCount / Math.Max(1, totalLen);

        var scale = baseScale;

        if (firstLineLen >= 95)
            scale -= 0.004f;
        if (totalLen >= 140)
            scale -= 0.003f;
        if (punctCount >= 3 && punctDensity >= 0.02f)
            scale -= 0.003f;

        // Keep the adjustment bounded to avoid regressions in other numbered paragraphs.
        return Compat.Clamp(scale, 0.972f, 0.986f);
    }

    /// <summary>
    /// Estimates text width using the appropriate font metrics for word-wrap layout.
    /// Uses Calibri widths for Calibri-based documents, Helvetica widths otherwise.
    /// </summary>
    /// <summary>
    /// Returns the font metrics factor used for line height calculation.
    /// Serif fonts like Times New Roman have smaller ascent/descent ratios and need a lower factor.
    /// </summary>
    private static float GetFontMetricsFactor(string? fontName)
    {
        if (fontName != null &&
            (fontName.Contains("Times", StringComparison.OrdinalIgnoreCase) ||
             fontName.Contains("Georgia", StringComparison.OrdinalIgnoreCase)))
            return FontMetricsFactorTimesNewRoman;
        return FontMetricsFactor;
    }

    /// <summary>
    /// Returns true when the font name is a well-known CJK (East Asian) font family.
    /// </summary>
    private static bool IsCjkFont(string fontName)
    {
        return fontName.Contains("SimSun", StringComparison.OrdinalIgnoreCase)
            || fontName.Contains("SimHei", StringComparison.OrdinalIgnoreCase)
            || fontName.Contains("NSimSun", StringComparison.OrdinalIgnoreCase)
            || fontName.Contains("FangSong", StringComparison.OrdinalIgnoreCase)
            || fontName.Contains("KaiTi", StringComparison.OrdinalIgnoreCase)
            || fontName.Contains("MingLiU", StringComparison.OrdinalIgnoreCase)
            || fontName.Contains("PMingLiU", StringComparison.OrdinalIgnoreCase)
            || fontName.Contains("MS Gothic", StringComparison.OrdinalIgnoreCase)
            || fontName.Contains("MS Mincho", StringComparison.OrdinalIgnoreCase)
            || fontName.Contains("Meiryo", StringComparison.OrdinalIgnoreCase)
            || fontName.Contains("Malgun", StringComparison.OrdinalIgnoreCase)
            || fontName.Contains("Batang", StringComparison.OrdinalIgnoreCase)
            || fontName.Contains("Gulim", StringComparison.OrdinalIgnoreCase)
            || fontName.Contains("DengXian", StringComparison.OrdinalIgnoreCase)
            || fontName.Contains("Microsoft YaHei", StringComparison.OrdinalIgnoreCase)
            || fontName.Contains("Microsoft JhengHei", StringComparison.OrdinalIgnoreCase)
            || fontName.Contains("Yu Gothic", StringComparison.OrdinalIgnoreCase)
            || fontName.Contains("Yu Mincho", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Returns true when the style is a Table of Contents style (TOC1–TOC9).
    /// </summary>
    private static bool IsTocStyle(string? styleId)
    {
        return styleId != null && styleId.Length == 4
            && styleId.StartsWith("TOC", StringComparison.OrdinalIgnoreCase)
            && char.IsDigit(styleId[3]);
    }

    /// <summary>
    /// Returns true when the font is a serif family whose Latin glyphs are
    /// noticeably narrower than Helvetica (used for width reduction tuning).
    /// </summary>
    private static bool IsSerifFont(string? fontName)
    {
        if (fontName == null) return false;
        return fontName.Contains("Times", StringComparison.OrdinalIgnoreCase)
            || fontName.Contains("Georgia", StringComparison.OrdinalIgnoreCase)
            || fontName.Contains("Cambria", StringComparison.OrdinalIgnoreCase)
            || fontName.Contains("Palatino", StringComparison.OrdinalIgnoreCase)
            || fontName.Contains("Garamond", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// When using Helvetica fallback, applies a kerning/hinting reduction since
    /// Helvetica is wider than most actual document fonts (Times New Roman, PMingLiU, etc.).
    /// </summary>
    private static float EstimateWrapTextWidth(string text, float fontSize, bool bold = false, float charSpacing = 0, bool useCalibriWidths = true)
    {
        if (s_overrideWidths != null && !useCalibriWidths)
            return EstimateOverrideTextWidth(text, fontSize, bold, charSpacing);
        if (useCalibriWidths)
            return EstimateCalibrTextWidth(text, fontSize, bold, charSpacing);
        var rawWidth = EstimateTextWidth(text, fontSize, charSpacing);
        // Bold text in serif fonts (e.g. Times New Roman) is typically ~5% wider
        // than regular weight. Without this, bold keywords fits too easily on a
        // line, causing fewer word-wrap breaks than the reference document.
        // Skip the inflation for wide sans-serif fonts (Franklin Gothic Demi,
        // Montserrat, etc.) - those are already heavy weights whose bold variant
        // is only marginally wider; over-inflating forces spurious wraps when
        // the text would actually fit (e.g. "ready to list?" headings).
        bool hasCjkPre = false;
        foreach (var c in text) if (c >= '\u2E80' && !char.IsHighSurrogate(c) && !char.IsLowSurrogate(c)) { hasCjkPre = true; break; }
        if (bold && !(s_wideSansSerifFont && !hasCjkPre)) rawWidth *= 1.10f;
        // Helvetica Latin metrics are wider than common CJK-Latin fonts
        // (PMingLiU, SimSun, etc.) which use narrower Latin glyphs.
        // Reduce the Latin portion to better match actual document font wrapping.
        // Must match the unit calculation in EstimateTextWidth (CJK-context spaces = 500).
        bool hasCjk = false;
        foreach (var c in text) if (c >= '\u2E80' && !char.IsHighSurrogate(c) && !char.IsLowSurrogate(c)) { hasCjk = true; break; }
        float latinUnits = 0;
        float totalUnits = 0;
        for (int i = 0; i < text.Length; i++)
        {
            var ch = text[i];
            if (char.IsHighSurrogate(ch) && i + 1 < text.Length && char.IsLowSurrogate(text[i + 1]))
            {
                latinUnits += 500;
                totalUnits += 500;
                i++;
                continue;
            }
            var w = GetHelveticaCharWidth(ch);
            var actual = (hasCjk && ch == ' ') ? 500 : w;
            totalUnits += actual;
            // Exclude CJK fullwidth glyphs and auto-inserted thin spaces (\u2009)
            // from the Latin fraction. Thin spaces are inter-script spacing that
            // doesn't shrink under CJK font Latin-glyph reduction.
            if (!(w == 1000 && ch >= '\u2E80') && ch != '\u2009')
                latinUnits += actual;
        }
        if (latinUnits > 0 && totalUnits > 0)
        {
            var latinFraction = latinUnits / totalUnits;
            // CJK fonts (PMingLiU, SimSun) use much narrower Latin glyphs than
            // Helvetica → 27% reduction (uppercase Latin glyphs in SimSun are
            // typically 500/1000 vs Helvetica's 667-722/1000). Serif fonts
            // (Times New Roman, Georgia) have noticeably narrower Latin glyphs → 10%.
            // Other non-Calibri sans-serif fonts are closer → 8%.
            var serifReduction = s_serifFont ? 0.10f : 0.08f;
            // Wide sans-serif fonts (Franklin Gothic, Montserrat) have Latin glyphs
            // close to Helvetica width; use a small reduction so WordWrap doesn't
            // over-pack lines (which would trigger Tz compression at render time).
            // Apply this for bold weights too: Franklin Gothic Demi Bold is only
            // marginally wider than Demi, so the regular 5% bold reduction works.
            if (s_wideSansSerifFont && !hasCjk)
                serifReduction = 0.04f;
            var reductionFactor = hasCjk ? 0.27f : (bold && !s_wideSansSerifFont ? 0.05f : serifReduction);
            rawWidth *= 1f - latinFraction * reductionFactor;
        }
        return rawWidth;
    }

    /// <summary>
    /// Gets character width using the appropriate font metrics for word-wrap layout.
    /// </summary>
    private static int GetWrapCharWidth(char ch, bool useCalibriWidths = true)
    {
        return useCalibriWidths ? GetCalibrCharWidth(ch) : GetHelveticaCharWidth(ch);
    }

    /// <summary>
    /// Resolves {PAGE}, {PAGE:roman}, {PAGE:ROMAN}, {NUMPAGES} placeholders.
    /// </summary>
    private static string ResolvePagePlaceholders(string text, int pageNum, int totalPages)
    {
        if (text.Contains("{PAGE:roman}"))
            text = text.Replace("{PAGE:roman}", ToRoman(pageNum, false));
        else if (text.Contains("{PAGE:ROMAN}"))
            text = text.Replace("{PAGE:ROMAN}", ToRoman(pageNum, true));
        text = text.Replace("{PAGE}", pageNum.ToString());
        text = text.Replace("{NUMPAGES}", totalPages.ToString());
        return text;
    }

    private static string ToRoman(int number, bool uppercase)
    {
        if (number <= 0) return number.ToString();
        var sb = new System.Text.StringBuilder();
        int[] values = { 1000, 900, 500, 400, 100, 90, 50, 40, 10, 9, 5, 4, 1 };
        string[] symbols = { "m", "cm", "d", "cd", "c", "xc", "l", "xl", "x", "ix", "v", "iv", "i" };
        for (int i = 0; i < values.Length; i++)
        {
            while (number >= values[i])
            {
                sb.Append(symbols[i]);
                number -= values[i];
            }
        }
        return uppercase ? sb.ToString().ToUpperInvariant() : sb.ToString();
    }

    /// <summary>
    /// Computes the actual rendered width matching PdfWriter.MeasureTextWidth,
    /// using HelveticaBold metrics for bold text (no Latin fraction reduction).
    /// </summary>
    private static float EstimateRenderedWidth(string text, float fontSize, bool bold, float charSpacing = 0)
    {
        float totalUnits = 0;
        foreach (var ch in text)
            totalUnits += bold ? PdfWriter.HelveticaBoldCharWidth(ch) : GetHelveticaCharWidth(ch);
        var width = fontSize * totalUnits / 1000f;
        if (charSpacing != 0 && text.Length > 1)
            width += charSpacing * (text.Length - 1);
        return width;
    }

    /// <summary>
    /// Estimates text width using override character widths (e.g. Century Gothic).
    /// No Latin‐fraction reduction is applied because the widths already match the actual font.
    /// </summary>
    private static float EstimateOverrideTextWidth(string text, float fontSize, bool bold, float charSpacing)
    {
        var widths = s_overrideWidths!;
        float totalUnits = 0;
        for (int i = 0; i < text.Length; i++)
        {
            var ch = text[i];
            if (char.IsHighSurrogate(ch) && i + 1 < text.Length && char.IsLowSurrogate(text[i + 1]))
            {
                totalUnits += 500;
                i++;
                continue;
            }
            if (ch < ' ') continue;
            if (ch == '\u2009') { totalUnits += 250; continue; }
            if (ch >= ' ' && ch <= '~')
                totalUnits += widths[ch - ' '];
            else
                totalUnits += GetHelveticaCharWidth(ch); // CJK / other Unicode fallback
        }
        if (bold) totalUnits *= 1.03f;
        var width = fontSize * totalUnits / 1000f;
        if (charSpacing != 0 && text.Length > 1)
            width += charSpacing * (text.Length - 1);
        return width;
    }

    /// <summary>
    /// Estimates the rendered width of a text string using Helvetica font metrics.
    /// </summary>
    private static float EstimateTextWidth(string text, float fontSize, float charSpacing = 0)
    {
        float totalUnits = 0;
        bool hasCjk = false;
        foreach (var c in text)
            if (c >= '\u2E80' && !char.IsHighSurrogate(c) && !char.IsLowSurrogate(c)) { hasCjk = true; break; }
        for (int i = 0; i < text.Length; i++)
        {
            var ch = text[i];
            // Handle surrogate pairs as single characters (Mathematical symbols, emoji, etc.)
            if (char.IsHighSurrogate(ch) && i + 1 < text.Length && char.IsLowSurrogate(text[i + 1]))
            {
                totalUnits += 500; // approximate width for SMP characters (math symbols etc.)
                i++; // skip low surrogate
            }
            else
            {
                totalUnits += (hasCjk && ch == ' ') ? 500 : GetHelveticaCharWidth(ch);
            }
        }
        var width = fontSize * totalUnits / 1000f;
        if (charSpacing != 0 && text.Length > 1)
            width += charSpacing * (text.Length - 1);
        return width;
    }

    /// <summary>
    /// Estimates text width using Calibri font metrics (for word-wrap layout matching Word/LibreOffice).
    /// </summary>
    private static float EstimateCalibrTextWidth(string text, float fontSize, bool bold = false, float charSpacing = 0)
    {
        float latinUnits = 0;
        float cjkUnits = 0;
        // When text contains CJK characters, the PDF renderer uses CJK fonts for the
        // entire text block (including ASCII spaces). CJK fonts render space at 500/1000,
        // not at the Calibri width (226/1000). Detect CJK context to use correct metrics.
        // However, spaces between Latin-only characters should use Calibri width to
        // produce line breaks that match Word/LibreOffice reference output.
        bool hasCjk = false;
        foreach (var c in text)
            if (c >= '\u2E80' && !char.IsHighSurrogate(c) && !char.IsLowSurrogate(c)) { hasCjk = true; break; }
        for (var i = 0; i < text.Length; i++)
        {
            var ch = text[i];
            // Handle surrogate pairs as single characters
            if (char.IsHighSurrogate(ch) && i + 1 < text.Length && char.IsLowSurrogate(text[i + 1]))
            {
                latinUnits += 500; // approximate width for SMP characters
                i++;
                continue;
            }
            var w = GetCalibrCharWidth(ch);
            if (w == 1000 && ch >= '\u2E80') // CJK full-width characters
                cjkUnits += w;
            else if (hasCjk && ch == ' ')
            {
                // Use CJK space width only when adjacent to a CJK/fullwidth character.
                // Spaces between Latin characters use Calibri width for accurate wrapping.
                bool adjCjk = false;
                if (i > 0) { var p = text[i - 1]; adjCjk |= GetCalibrCharWidth(p) == 1000 && p >= '\u2E80'; }
                if (!adjCjk && i + 1 < text.Length) { var n = text[i + 1]; adjCjk |= GetCalibrCharWidth(n) == 1000 && n >= '\u2E80'; }
                if (adjCjk)
                    cjkUnits += 500;
                else
                    latinUnits += w; // Calibri space (226)
            }
            else
                latinUnits += w;
        }
        // Calibri Bold is ~3% wider than Calibri Regular on average.
        // In multi-format paragraphs where runs use non-Calibri fonts (e.g.
        // Times New Roman Bold), the effective width difference is larger (~8-12%).
        if (bold) latinUnits *= 1.03f;
        // Approximate kerning/hinting reduction: actual rendered text is ~2.3% narrower
        // than raw glyph-width sum due to font-level kerning pairs and grid fitting.
        // Only apply to Latin characters; CJK fonts have no inter-character kerning.
        latinUnits *= 0.977f;
        // When the run font is a serif (Times New Roman etc.) inside a Calibri-default
        // document, Calibri Latin glyphs are ~5% wider than the serif glyphs Word will
        // actually render with. Apply an additional reduction so WordWrap matches Word's
        // line-break decisions for the serif text.
        if (s_serifRunInCalibri) latinUnits *= 0.95f;
        var totalUnits = latinUnits + cjkUnits;
        var width = fontSize * totalUnits / 1000f;
        if (charSpacing != 0 && text.Length > 1)
            width += charSpacing * (text.Length - 1);
        return width;
    }

    private static int GetHelveticaCharWidth(char ch)
    {
        if (ch < ' ') return 0; // control characters (\n, \r, \t, etc.)
        if (ch == '\u2009') return 250; // THIN SPACE: quarter-em for autoSpaceDE
        if (ch >= ' ' && ch <= '~')
            return HelveticaWidths[ch - ' '];
        if (ch >= '\u4E00' && ch <= '\u9FFF'    // CJK Unified Ideographs
            || ch >= '\u3400' && ch <= '\u4DBF'  // CJK Extension A
            || ch >= '\u3000' && ch <= '\u303F'  // CJK Symbols and Punctuation
            || ch >= '\u3040' && ch <= '\u309F'  // Hiragana
            || ch >= '\u30A0' && ch <= '\u30FF'  // Katakana
            || ch >= '\uF900' && ch <= '\uFAFF'  // CJK Compatibility Ideographs
            || ch >= '\uFF00' && ch <= '\uFFEF') // Halfwidth and Fullwidth Forms
            return 1000;
        return 500; // fallback for other Unicode chars
    }

    private static int GetCalibrCharWidth(char ch)
    {
        if (ch < ' ') return 0;
        if (ch >= ' ' && ch <= '~')
            return CalibrWidths[ch - ' '];
        if (ch >= '\u4E00' && ch <= '\u9FFF'
            || ch >= '\u3400' && ch <= '\u4DBF'
            || ch >= '\u3000' && ch <= '\u303F'
            || ch >= '\u3040' && ch <= '\u309F'
            || ch >= '\u30A0' && ch <= '\u30FF'
            || ch >= '\uF900' && ch <= '\uFAFF'
            || ch >= '\uFF00' && ch <= '\uFFEF')
            return 1000;
        return (int)(GetHelveticaCharWidth(ch) * CalibriWidthScale);
    }

    /// <summary>
    /// CJK kinsoku: characters that must not start a line (closing/trailing punctuation).
    /// </summary>
    private static bool IsNoStartChar(char ch) =>
        ch is '\u3001' or '\u3002'   // 、。
            or '\uFF0C' or '\uFF0E'  // ，．
            or '\uFF01' or '\uFF1F'  // ！？
            or '\uFF1B' or '\uFF1A'  // ；：
            or '\uFF09' or '\u3009'  // ）〉
            or '\u300B' or '\u300D'  // 》」
            or '\u300F' or '\u3011'  // 』】
            or '\uFF3D' or '\uFF5D'; // ］｝

    /// <summary>
    /// Inserts a space between Latin-script words and CJK characters to approximate
    /// Word/LibreOffice inter-script spacing, but avoids aggressive insertion for
    /// short labels and pure numeric tokens (e.g. "A期", "500字").
    /// When <paramref name="autoSpaceDE"/> is true (OOXML default), inserts spaces
    /// between Latin words and CJK characters (e.g. "如PECVD" → "如 PECVD").
    /// When <paramref name="autoSpaceDN"/> is true (OOXML default), also inserts
    /// spaces between digit sequences and CJK ideographs (e.g. "2025年" → "2025 年").
    /// </summary>
    private static string AddInterScriptSpacing(string text, bool autoSpaceDE = true, bool autoSpaceDN = true)
    {
        if (string.IsNullOrEmpty(text) || text.Length < 2) return text;
        var sb = new System.Text.StringBuilder(text.Length + 8);
        sb.Append(text[0]);
        for (var i = 1; i < text.Length; i++)
        {
            if (autoSpaceDE && ShouldInsertInterScriptSpace(text, i))
            {
                // Use THIN SPACE so the inter-script gap is not treated as a word
                // boundary by WordWrap (Split(' ')) and is not expanded by justify
                // word spacing.  Word's autoSpaceDE is visual spacing only – it does
                // NOT create a line-break opportunity.
                sb.Append('\u2009');
            }
            else if (autoSpaceDN && ShouldInsertDigitCjkSpace(text, i))
            {
                sb.Append('\u2009'); // THIN SPACE: ~1/4 em, not expanded by justified word spacing
            }
            sb.Append(text[i]);
        }
        return sb.ToString();
    }

    private static bool IsLatinOrDigit(char c) => c is >= '0' and <= '9' or >= 'A' and <= 'Z' or >= 'a' and <= 'z';
    private static bool IsLatinLetter(char c) => c is >= 'A' and <= 'Z' or >= 'a' and <= 'z';
    private static bool IsCjkIdeograph(char c) => c is >= '\u4E00' and <= '\u9FFF' or >= '\u3400' and <= '\u4DBF';
    private static bool IsAsciiDigit(char c) => c is >= '0' and <= '9';

    /// <summary>
    /// Returns true when a space should be inserted at <paramref name="boundaryIndex"/>
    /// to implement OOXML autoSpaceDE (digit↔CJK ideograph spacing).
    /// Word inserts spacing only at digit→CJK boundaries (e.g. "2025年" → "2025 年"),
    /// not at CJK→digit boundaries (e.g. "年11" stays as-is).
    /// </summary>
    private static bool ShouldInsertDigitCjkSpace(string text, int boundaryIndex)
    {
        if (boundaryIndex <= 0 || boundaryIndex >= text.Length) return false;
        var left = text[boundaryIndex - 1];
        var right = text[boundaryIndex];
        // digit→CJK boundary only (not CJK→digit)
        return IsAsciiDigit(left) && IsCjkIdeograph(right);
    }

    private static bool ShouldInsertInterScriptSpace(string text, int boundaryIndex)
    {
        // boundaryIndex points to the right-side character; left is boundaryIndex - 1.
        if (boundaryIndex <= 0 || boundaryIndex >= text.Length) return false;

        var left = text[boundaryIndex - 1];
        var right = text[boundaryIndex];
        var hasBoundary = (IsLatinOrDigit(left) && IsCjkIdeograph(right))
            || (IsCjkIdeograph(left) && IsLatinOrDigit(right));
        if (!hasBoundary) return false;

        var leftLatinLen = IsLatinOrDigit(left)
            ? CountContiguousLatinOrDigitLeft(text, boundaryIndex - 1)
            : 0;
        var rightLatinLen = IsLatinOrDigit(right)
            ? CountContiguousLatinOrDigitRight(text, boundaryIndex)
            : 0;

        // Only insert around actual Latin words, not one-letter labels.
        if (leftLatinLen < 2 && rightLatinLen < 2)
            return false;

        // Keep numeric+CJK compact to avoid "500 字"-style over-spacing.
        if (leftLatinLen > 0 && IsAsciiDigitsOnly(text, boundaryIndex - leftLatinLen, leftLatinLen))
            return false;
        if (rightLatinLen > 0 && IsAsciiDigitsOnly(text, boundaryIndex, rightLatinLen))
            return false;

        // At least one side should be a letter token.
        var leftHasLetter = leftLatinLen > 0 && ContainsAsciiLetter(text, boundaryIndex - leftLatinLen, leftLatinLen);
        var rightHasLetter = rightLatinLen > 0 && ContainsAsciiLetter(text, boundaryIndex, rightLatinLen);
        return leftHasLetter || rightHasLetter;
    }

    /// <summary>
    /// Checks whether a CJK half-width space gap should be inserted between two
    /// consecutive runs at a Latin↔CJK script boundary. Mirrors the conditions
    /// in <see cref="ShouldInsertInterScriptSpace"/> but operates across runs.
    /// </summary>
    private static bool NeedInterRunScriptGap(string prevText, string nextText)
    {
        if (string.IsNullOrEmpty(prevText) || string.IsNullOrEmpty(nextText)) return false;
        var left = prevText[^1];
        var right = nextText[0];
        var hasBoundary = (IsLatinOrDigit(left) && IsCjkIdeograph(right))
            || (IsCjkIdeograph(left) && IsLatinOrDigit(right));
        if (!hasBoundary) return false;

        if (IsLatinOrDigit(left))
        {
            var len = CountContiguousLatinOrDigitLeft(prevText, prevText.Length - 1);
            if (len < 2) return false;
            if (IsAsciiDigitsOnly(prevText, prevText.Length - len, len)) return false;
            if (!ContainsAsciiLetter(prevText, prevText.Length - len, len)) return false;
        }
        if (IsLatinOrDigit(right))
        {
            var len = CountContiguousLatinOrDigitRight(nextText, 0);
            if (len < 2) return false;
            if (IsAsciiDigitsOnly(nextText, 0, len)) return false;
            if (!ContainsAsciiLetter(nextText, 0, len)) return false;
        }
        return true;
    }

    private static int CountContiguousLatinOrDigitLeft(string text, int start)
    {
        var count = 0;
        for (var i = start; i >= 0 && IsLatinOrDigit(text[i]); i--)
            count++;
        return count;
    }

    private static int CountContiguousLatinOrDigitRight(string text, int start)
    {
        var count = 0;
        for (var i = start; i < text.Length && IsLatinOrDigit(text[i]); i++)
            count++;
        return count;
    }

    private static bool IsAsciiDigitsOnly(string text, int start, int length)
    {
        for (var i = start; i < start + length; i++)
            if (text[i] < '0' || text[i] > '9')
                return false;
        return true;
    }

    private static bool ContainsAsciiLetter(string text, int start, int length)
    {
        for (var i = start; i < start + length; i++)
            if (IsLatinLetter(text[i]))
                return true;
        return false;
    }

    // Helvetica character widths for ASCII 32..126 (in thousandths of a unit)
    private static readonly int[] HelveticaWidths =
    [
        278, // ' ' (32)
        278, // !
        355, // "
        556, // #
        556, // $
        889, // %
        667, // &
        191, // '
        333, // (
        333, // )
        389, // *
        584, // +
        278, // ,
        333, // -
        278, // .
        278, // /
        556, 556, 556, 556, 556, 556, 556, 556, 556, 556, // 0-9
        278, // :
        278, // ;
        584, // <
        584, // =
        584, // >
        556, // ?
        1015, // @
        667, 667, 722, 722, 667, 611, 778, 722, 278, // A-I
        500, 667, 556, 833, 722, 778, 667, 778, 722, 667, // J-S
        611, 722, 667, 944, 667, 667, 611, // T-Z
        278, // [
        278, // backslash
        278, // ]
        469, // ^
        556, // _
        333, // `
        556, 556, 500, 556, 556, 278, 556, 556, 222, // a-i
        222, 500, 222, 833, 556, 556, 556, 556, 333, 500, // j-s
        278, 556, 500, 722, 500, 500, 500, // t-z
        334, // {
        260, // |
        334, // }
        584, // ~
    ];

    // Calibri Regular character widths for ASCII 32..126 (in thousandths of a unit)
    // Extracted from Calibri.ttf (UPM=2048, scaled to 1000 units)
    private static readonly int[] CalibrWidths =
    [
        226, 326, 401, 498, 507, 715, 682, 221, 303, 303, // ' ' to )
        498, 498, 250, 306, 252, 386, // * to /
        507, 507, 507, 507, 507, 507, 507, 507, 507, 507, // 0-9
        268, 268, 498, 498, 498, 463, 894, // : to @
        579, 544, 533, 615, 488, 459, 631, 623, 252, // A-I
        319, 520, 420, 855, 646, 662, 517, 673, 543, 459, // J-S
        487, 642, 567, 890, 519, 487, 468, // T-Z
        307, 386, 307, 498, 498, 291, // [ to `
        479, 525, 423, 525, 498, 305, 471, 525, 229, // a-i
        239, 455, 229, 799, 525, 527, 525, 525, 349, 391, // j-s
        335, 525, 452, 715, 433, 453, 395, // t-z
        314, 460, 314, 498, // { to ~
    ];

    // Century Gothic character widths for ASCII 32..126 (in thousandths of a unit)
    // Extracted from CenturyGothic.ttf via Windows GDI ABC widths (UPM=2048, scaled to 1000 units)
    private static readonly int[] CenturyGothicWidths =
    [
        277, 295, 309, 720, 554, 775, 757, 198, 369, 369, // ' ' to )
        425, 606, 277, 332, 277, 437, // * to /
        554, 554, 554, 554, 554, 554, 554, 554, 554, 554, // 0-9
        277, 277, 606, 606, 606, 591, 867, // : to @
        740, 574, 813, 744, 536, 485, 872, 683, 226, // A-I
        482, 591, 462, 919, 740, 869, 592, 871, 607, 498, // J-S
        426, 655, 702, 960, 609, 592, 480, // T-Z
        351, 605, 351, 672, 500, 378, // [ to `
        683, 682, 647, 685, 650, 314, 673, 610, 200, // a-i
        203, 502, 200, 938, 610, 655, 682, 682, 301, 388, // j-s
        339, 608, 554, 831, 480, 536, 425, // t-z
        351, 672, 351, 606, // { to ~
    ];

    // Montserrat SemiBold character widths for ASCII 32..126 (in thousandths of a unit)
    // Montserrat is a geometric sans-serif (Google Font) that is wider than Helvetica,
    // especially in SemiBold/Bold weights.  These values approximate the actual glyph
    // advance widths, enabling accurate word wrapping for Montserrat-based documents.
    private static readonly int[] MontserratWidths =
    [
        260, 292, 458, 630, 610, 833, 730, 242, 330, 330, // ' ' to )
        471, 630, 260, 385, 260, 381, // * to /
        620, 620, 620, 620, 620, 620, 620, 620, 620, 620, // 0-9
        260, 260, 630, 630, 630, 535, 948, // : to @
        666, 666, 672, 730, 600, 572, 730, 747, 284, // A-I
        462, 646, 552, 887, 747, 757, 642, 757, 653, 586, // J-S
        576, 727, 642, 947, 608, 590, 581, // T-Z
        330, 381, 330, 530, 490, 365, // [ to `
        586, 631, 534, 631, 588, 340, 611, 618, 259, // a-i
        259, 557, 270, 930, 618, 622, 631, 631, 394, 494, // j-s
        377, 618, 532, 800, 520, 532, 506, // t-z
        366, 284, 366, 630, // { to ~
    ];

    private static bool IsCenturyGothicLikeFont(string? fontName) =>
        !string.IsNullOrEmpty(fontName)
        && fontName.Contains("Century Gothic", StringComparison.OrdinalIgnoreCase);

    private static bool IsMontserratFont(string? fontName) =>
        !string.IsNullOrEmpty(fontName)
        && fontName.Contains("Montserrat", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Returns the appropriate per-character width override table for fonts with
    /// significantly different glyph widths from Helvetica, or null for standard fonts.
    /// </summary>
    private static int[]? GetFontOverrideWidths(string? fontName) =>
        IsCenturyGothicLikeFont(fontName) ? CenturyGothicWidths
        : IsMontserratFont(fontName) ? MontserratWidths
        : null;

    /// <summary>
    /// Returns true for wide geometric sans-serif fonts whose Latin glyphs are
    /// significantly wider than Calibri, requiring Helvetica-based width estimation
    /// for accurate word wrapping.  Excludes CJK, serif, and standard sans-serif
    /// families (Arial, Helvetica) where Calibri-based estimation works well.
    /// </summary>
    private static bool IsWideSansSerifFont(string? fontName)
    {
        if (string.IsNullOrEmpty(fontName)) return false;
        return fontName.Contains("Montserrat", StringComparison.OrdinalIgnoreCase)
            || fontName.Contains("Century Gothic", StringComparison.OrdinalIgnoreCase)
            || fontName.Contains("Futura", StringComparison.OrdinalIgnoreCase)
            || fontName.Contains("Comfortaa", StringComparison.OrdinalIgnoreCase)
            || fontName.Contains("Poppins", StringComparison.OrdinalIgnoreCase)
            || fontName.Contains("Raleway", StringComparison.OrdinalIgnoreCase)
            || fontName.Contains("Franklin Gothic", StringComparison.OrdinalIgnoreCase);
    }
}
