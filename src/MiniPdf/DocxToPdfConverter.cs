namespace MiniSoftware;

/// <summary>
/// Converts Word (.docx) files to PDF documents.
/// Renders paragraphs, tables, and images using the built-in Helvetica font.
/// </summary>
internal static class DocxToPdfConverter
{
    // Single-spaced line height ≈ fontSize × this factor (ascent + descent for typical fonts)
    private const float FontMetricsFactor = 1.17f;
    // Helvetica ascent ratio: visual top of text is baseline + fontSize × AscentRatio
    private const float AscentRatio = 1.075f;
    // Calibri-to-Helvetica width ratio: most DOCX documents use Calibri (default since Word 2007).
    // Used only as a fallback; per-character Calibri widths are used when available.
    private const float CalibriWidthScale = 0.87f;

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
            options.UseCalibriWidths = docxDoc.DefaultFontName.Contains("Calibri", StringComparison.OrdinalIgnoreCase);

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
            var headerContentHeight = EstimateElementsHeight(docxDoc.HeaderElements, options);
            var headerAreaHeight = options.MarginTop - options.HeaderMargin;
            if (headerContentHeight > headerAreaHeight)
            {
                // Preserve the original gap between header area and body content
                options.MarginTop = options.HeaderMargin + headerContentHeight + headerAreaHeight;
            }
        }
        // Adjust bottom margin when footer content is taller than the default footer area
        if (docxDoc.FooterElements is { Count: > 0 })
        {
            var footerContentHeight = EstimateElementsHeight(docxDoc.FooterElements, options);
            var footerTopFromBottom = options.FooterMargin + footerContentHeight;
            if (footerTopFromBottom > options.MarginBottom)
                options.MarginBottom = footerTopFromBottom;
        }

        state.EnsurePage();
        // Record section 0 start page
        state.SectionStartPages.Add(0);

        var sectionIndex = 0;
        foreach (var element in processedElements)
        {
            switch (element)
            {
                case DocxParagraph paragraph:
                    RenderParagraph(state, paragraph);
                    if (paragraph.FloatingTextBoxes is { Count: > 0 })
                        RenderFloatingTextBoxes(state, paragraph.FloatingTextBoxes, state.LastParagraphStartY);
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
                                state.ForceNewPage();
                                if (nextLayout.ColumnCount > 1)
                                    state.EnterMultiColumnSection(nextLayout.ColumnCount, nextLayout.ColumnSpacing, nextLayout.ColumnWidths, nextLayout.ColumnGaps);
                            }
                        }
                        else
                        {
                            if (state.ColumnCount > 1)
                                state.ExitMultiColumnSection();
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
                            pdfDoc.Pages[pi].AddImage(img.Data, fmt, ax, ay - h, w, h);
                    }
                    else
                    {
                        // Render only on the anchor page — find it from the dictionary key
                        foreach (var (pageIdx2, entries2) in state.BehindDocImagesPerPage)
                        {
                            if (entries2.Any(e => e.Image == img) && pageIdx2 >= 0 && pageIdx2 < pdfDoc.Pages.Count)
                            {
                                pdfDoc.Pages[pageIdx2].AddImage(img.Data, fmt, ax, ay - h, w, h);
                                break;
                            }
                        }
                    }
                }
            }
        }

        // Render header/footer background shapes on all pages.
        if (docxDoc.HeaderShapes is { Count: > 0 } || docxDoc.FooterShapes is { Count: > 0 })
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
            }
        }

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
                    var footerContentHeight = EstimateElementsHeight(pageFooterElements, options);
                    var footerStartY = options.FooterMargin + footerContentHeight;
                    RenderHeaderFooterElementsOnPage(page, options, pageFooterElements, footerStartY, pi, totalPages, sectionPageNum);
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
            var resolvedRuns = new List<(string Text, bool Bold, string? FontName)>();
            foreach (var run in runs)
            {
                var text = ResolvePagePlaceholders(run.Text, pageIndex + 1, totalPages);
                if (text == "\n") continue; // skip paragraph breaks for width calc
                resolvedRuns.Add((text, run.Bold, run.FontName));
            }

            float totalWidth = 0;
            foreach (var (text, _, _) in resolvedRuns)
                totalWidth += EstimateTextWidth(text, fontSize);

            var startX = options.MarginLeft + (usableW - totalWidth) / 2;
            var currentX = startX;
            foreach (var (text, bold, runFontName) in resolvedRuns)
            {
                page.AddText(text, currentX, y, fontSize, color, bold: bold, preferredFontName: runFontName);
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
            return true;
        }

        public void EnsurePage()
        {
            if (CurrentPage == null || CurrentY < Options.MarginBottom)
            {
                // Try advancing to next column before creating a new page
                if (ColumnCount > 1 && CurrentPage != null && AdvanceToNextColumn())
                    return;

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

            if (ColumnCount > 1)
                ColumnTopY = CurrentY;
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
        float lineHeight;
        if (paragraph.LineSpacingAbsolute && paragraph.LineSpacing > 0)
            lineHeight = paragraph.LineSpacing; // exact/atLeast: absolute points
        else
        {
            var lineSpacingMul = paragraph.LineSpacing > 0 ? paragraph.LineSpacing : options.LineSpacing;
            lineHeight = fontSize * FontMetricsFactor * lineSpacingMul;
        }

        // Snap to document grid when active (CJK line grid)
        if (options.GridLinePitch > 0 && paragraph.SnapToGrid)
        {
            var gridPitch = options.GridLinePitch;
            if (paragraph.LineSpacing == 0)
            {
                // Auto-spaced: use max run font size for grid-snapped height.
                var maxFs = fontSize;
                foreach (var run in paragraph.Runs)
                {
                    var runFs = run.FontSize > 0 ? run.FontSize : fontSize;
                    if (runFs > maxFs) maxFs = runFs;
                }
                lineHeight = Math.Max(gridPitch, Compat.Ceiling(maxFs / gridPitch) * gridPitch);
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

        // Handle empty paragraphs before EnsurePage — they don't produce visible content
        // and should not force a new page (avoids spurious trailing pages).
        if (paragraph.Runs.Count == 0 && paragraph.Images.Count == 0 && paragraph.Shading == null
            && (paragraph.Shapes == null || paragraph.Shapes.Count == 0))
        {
            var totalEmptyAdvance = lineHeight;
            var spacingAfterEmpty = paragraph.SpacingAfter >= 0 ? paragraph.SpacingAfter : 0f;
            totalEmptyAdvance += spacingAfterEmpty;

            if (state.CurrentPage != null)
            {
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
        if (!wasTopOfPage && !paragraph.LineSpacingAbsolute && state.LastLineHeight > 0
            && lineHeight > state.LastLineHeight)
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
                if (paragraph.IsBulletList)
                {
                    var bulletSize = fontSize * 0.25f;
                    var bulletY = state.CurrentY + fontSize * 0.25f;
                    state.CurrentPage!.AddRectangle(numberX, bulletY, bulletSize, bulletSize, new PdfColor(0, 0, 0));
                }
                else
                {
                    state.CurrentPage!.AddText(paragraph.ListText, numberX, state.CurrentY, fontSize, preferredFontName: listFont);
                }
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
                    if (paragraph.IsBulletList)
                    {
                        var bulletSize = fontSize * 0.25f;
                        var bulletY = state.CurrentY + fontSize * 0.25f;
                        state.CurrentPage!.AddRectangle(numberX, bulletY, bulletSize, bulletSize, new PdfColor(0, 0, 0));
                    }
                    else
                    {
                        state.CurrentPage!.AddText(paragraph.ListText, numberX, state.CurrentY, fontSize, preferredFontName: listFont);
                    }
                    // x and availableWidth remain unchanged (indentLeft already accounts for list)
                }
                else
                {
                    var listIndent = 18f * (paragraph.ListLevel + 1);
                    if (paragraph.IsBulletList)
                    {
                        var bulletSize = fontSize * 0.25f;
                        var bulletX = x + listIndent - 12f;
                        var bulletY = state.CurrentY + fontSize * 0.25f;
                        state.CurrentPage!.AddRectangle(bulletX, bulletY, bulletSize, bulletSize, new PdfColor(0, 0, 0));
                    }
                    else
                    {
                        state.CurrentPage!.AddText(paragraph.ListText, x + listIndent - 12f, state.CurrentY, fontSize, preferredFontName: listFont);
                    }
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
        var wrapFirstLineWidth = firstLineWidth;
        var wrapAvailableWidth = availableWidth;

        // Numbered justified paragraphs in DOCX references tend to wrap slightly
        // earlier than our baseline estimation. Apply a tiny, localized reduction
        // so long lines (like affidavit items 1/2/3) break closer to reference.
        if (paragraph.Alignment == "both" && paragraph.IsNumberedList)
        {
            var numberedBothWrapScale = GetDynamicNumberedWrapScale(paragraph);
            wrapFirstLineWidth *= numberedBothWrapScale;
            wrapAvailableWidth *= numberedBothWrapScale;
        }


        // Render images first (inline images)
        foreach (var image in paragraph.Images)
        {
            RenderImage(state, image);
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
            // (inline images advance Y themselves via RenderImage)
            if (paragraph.Images.Count == 0 || !paragraph.Images.Any(img => !img.IsAnchor))
                state.AdvanceY(lineHeight);
            // Apply spacing after
            var spacingAfterEmpty = paragraph.SpacingAfter >= 0 ? paragraph.SpacingAfter : 0f;
            state.AdvanceY(spacingAfterEmpty);
            state.LastSpacingAfter = spacingAfterEmpty;

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
                paragraph.StyleId, paragraph.Bold, paragraph.Italic, paragraph.FontSize, paragraph.Color,
                paragraph.HasPageBreakBefore, paragraph.HasPageBreakAfter, paragraph.Shading, paragraph.TabStops,
                paragraph.SectionBreak, paragraph.Borders),
                x, firstLineX, wrapAvailableWidth, wrapFirstLineWidth, fontSize, lineHeight);

        }
        else
        {
            // Simple path: all runs share the same formatting
            var fullText = AddInterScriptSpacing(string.Concat(mergedRuns.Select(r => r.Text)));
            var dominantRun = mergedRuns.FirstOrDefault(r => !string.IsNullOrEmpty(r.Text));
            var runFontSize = dominantRun?.FontSize > 0 ? dominantRun.FontSize : fontSize;
            var runColor = dominantRun?.Color ?? paragraph.Color;
            var runBold = dominantRun?.Bold ?? false;
            var runUnderline = dominantRun?.Underline ?? false;
            var runCharSpacing = dominantRun?.CharSpacing ?? 0f;
            var runFontName = dominantRun?.FontName;

            var lines = WordWrap(fullText, wrapFirstLineWidth, wrapAvailableWidth, runFontSize, paragraph.TabStops, runBold, runCharSpacing, options.UseCalibriWidths);

            // If paragraph has leader tab stops, apply maxWidth so the Tz operator
            // compresses the extra-dot text to fit the intended tab position.
            float? tabLeaderMaxWidth = null;
            if (paragraph.TabStops?.Any(ts => ts.Leader is "dot" or "hyphen" or "underscore") == true)
            {
                tabLeaderMaxWidth = paragraph.TabStops.Max(ts => ts.Position);
            }

            for (var i = 0; i < lines.Count; i++)
            {
                // Proactive page break: ensure a full line box fits above bottom margin
                if (state.CurrentPage != null && !state.IsTopOfPage
                    && state.CurrentY - lineHeight < state.Options.MarginBottom)
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
                var textWidth = EstimateWrapTextWidth(line, runFontSize, runBold, runCharSpacing, options.UseCalibriWidths);
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
                        var extraSpace = lineW - textWidth;
                        if (extraSpace > 0)
                            wordSpacing = extraSpace / spaceCount;
                    }
                }

                state.CurrentPage!.AddText(line, renderX, state.CurrentY, runFontSize, runColor, maxWidth: renderMaxWidth, bold: runBold, underline: runUnderline, charSpacing: runCharSpacing, wordSpacing: wordSpacing, preferredFontName: runFontName);
                state.AdvanceY(lineHeight);
            }
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

        // Apply spacing after
        var spacingAfter = paragraph.SpacingAfter >= 0 ? paragraph.SpacingAfter : 0f;
        state.AdvanceY(spacingAfter);
        state.LastSpacingAfter = spacingAfter;

        state.LastLineHeight = lineHeight;

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
            // Horizontal: column/margin → offset from left margin; page → offset from page edge
            var boxLeft = box.HRelativeFrom == "page"
                ? box.XPt
                : options.MarginLeft + box.XPt;

            // Vertical positioning with cross-page support
            var targetPage = page;
            float boxTop;
            if (box.VRelativeFrom is "paragraph" or "line")
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

            // Render text content at absolute position
            var currentY = boxTop;
            foreach (var para in box.Paragraphs)
            {
                var fontSize = para.FontSize > 0 ? para.FontSize : options.FontSize;
                float lineHeight;
                if (para.LineSpacingAbsolute && para.LineSpacing > 0)
                    lineHeight = para.LineSpacing;
                else
                {
                    var lineSpacingMul = para.LineSpacing > 0 ? para.LineSpacing : options.LineSpacing;
                    lineHeight = fontSize * FontMetricsFactor * lineSpacingMul;
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

                // Wrap text within the box width
                var maxWidth = box.WidthPt > 0 ? box.WidthPt : state.UsableWidth;
                var lines = WordWrap(fullText, maxWidth, maxWidth, fontSize,
                    para.TabStops, useCalibriWidths: options.UseCalibriWidths);

                foreach (var line in lines)
                {
                    var x = boxLeft;
                    if (para.Alignment == "center")
                    {
                        var textWidth = EstimateWrapTextWidth(line, fontSize, bold, 0, options.UseCalibriWidths);
                        x = boxLeft + (maxWidth - textWidth) / 2;
                    }
                    else if (para.Alignment == "right")
                    {
                        var textWidth = EstimateWrapTextWidth(line, fontSize, bold, 0, options.UseCalibriWidths);
                        x = boxLeft + maxWidth - textWidth;
                    }
                    targetPage.AddText(line, x, currentY, fontSize, color, bold: bold);
                    currentY -= lineHeight;
                }
            }
        }
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
            var underlineMatch = current.Underline == next.Underline
                || (isWhitespaceOnly && !next.Underline)
                || (isCurWhitespace && !current.Underline);
            var charSpacingMatch = Math.Abs(current.CharSpacing - next.CharSpacing) < 0.01f || isWhitespaceOnly || isCurWhitespace;
            var fontNameMatch = string.Equals(current.FontName, next.FontName, StringComparison.OrdinalIgnoreCase) || isWhitespaceOnly || isCurWhitespace;
            var vertPosMatch = Math.Abs(current.VerticalPosition - next.VerticalPosition) < 0.01f;
            if (Math.Abs(curFs - nextFs) < 0.01f && colorMatch && boldMatch && underlineMatch && charSpacingMatch && fontNameMatch && vertPosMatch
                && !current.IsPageBreak && !next.IsPageBreak
                && !current.IsColumnBreak && !next.IsColumnBreak)
            {
                current = new DocxRun(current.Text + next.Text, current.Bold, current.Italic || next.Italic,
                    current.FontSize, current.Color, false, current.Underline, current.CharSpacing, current.FontName,
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
        // Proactive page break: ensure a full line box fits above bottom margin
        if (state.CurrentPage != null && !state.IsTopOfPage
            && state.CurrentY - lineHeight < state.Options.MarginBottom)
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
        if (paragraph.Alignment is "center" or "right")
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
                rText = AddInterScriptSpacing(rText);
                // Add inter-script gap at run boundaries (Latin↔CJK)
                if (prevRunTextPre != null && rText.Length > 0
                    && NeedInterRunScriptGap(prevRunTextPre, rText))
                {
                    totalWidth += rFs * 500f / 1000f;
                }
                totalWidth += EstimateWrapTextWidth(rText, rFs, r.Bold, r.CharSpacing, false);
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
            ? new List<(string Text, float X, float Y, float FontSize, PdfColor? Color, bool Bold, bool Underline, float CharSpacing, string? FontName, float? MaxWidth, float? UlWidth)>()
            : null;

        void BufferOrEmit(string text, float bx, float by, float bfs, PdfColor? bcolor,
            bool bbold, bool bunderline, float bcs, string? bfontName, float? bmaxW, float? bulW)
        {
            if (lineEntries != null)
                lineEntries.Add((text, bx, by, bfs, bcolor, bbold, bunderline, bcs, bfontName, bmaxW, bulW));
            else
                state.CurrentPage!.AddText(text, bx, by, bfs, bcolor,
                    bold: bbold, underline: bunderline, charSpacing: bcs,
                    preferredFontName: bfontName, maxWidth: bmaxW, underlineWidth: bulW);
        }

        void FlushLineEntries()
        {
            if (lineEntries == null || lineEntries.Count == 0) return;
            // Compute actual line content width from buffered entries
            float minX = float.MaxValue, maxEndX = 0;
            foreach (var e in lineEntries)
            {
                if (e.X < minX) minX = e.X;
                var w = EstimateWrapTextWidth(e.Text, e.FontSize, e.Bold, e.CharSpacing, false);
                var endX = e.X + w;
                if (endX > maxEndX) maxEndX = endX;
            }
            var lineTextWidth = maxEndX - minX;
            var lw = isFirstLine ? firstLineWidth : availableWidth;
            var shift = paragraph.Alignment == "center"
                ? Math.Max(0, (lw - lineTextWidth) / 2f)
                : Math.Max(0, lw - lineTextWidth);
            foreach (var e in lineEntries)
            {
                state.CurrentPage!.AddText(e.Text, e.X + shift, e.Y, e.FontSize, e.Color,
                    bold: e.Bold, underline: e.Underline, charSpacing: e.CharSpacing,
                    preferredFontName: e.FontName, maxWidth: e.MaxWidth, underlineWidth: e.UlWidth);
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

            var runFs = run.FontSize > 0 ? run.FontSize : defaultFontSize;
            var runColor = run.Color ?? paragraph.Color;

            // Split run text by hard line breaks first (from <w:br/>)
            var hardLines = run.Text.Split('\n');
            for (var hi = 0; hi < hardLines.Length; hi++)
            {
                // Force line break for each \n (except before the first segment)
                if (hi > 0)
                {
                    FlushLineEntries();
                    if (!state.IsTopOfPage && state.CurrentY - lineHeight < state.Options.MarginBottom)
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
                segment = AddInterScriptSpacing(segment);
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

                for (var wi = 0; wi < words.Length; wi++)
                {
                    var word = words[wi];
                    var wordWidth = EstimateWrapTextWidth(word, runFs, run.Bold, run.CharSpacing, useCalibri);
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
                                BufferOrEmit(pendingText, pendingX, state.CurrentY + run.VerticalPosition, runFs, runColor, run.Bold, run.Underline, run.CharSpacing, run.FontName, flushMaxW > 0 ? flushMaxW : (float?)null, null);
                                pendingText = "";
                            }
                            FlushLineEntries();
                            // Wrap to next line
                            if (!state.IsTopOfPage && state.CurrentY - lineHeight < state.Options.MarginBottom)
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
                            accWidth += runFs * GetWrapCharWidth(pendingText[ci], useCalibri) / 1000f;
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
                        BufferOrEmit(pendingText[..breakAt], pendingX, state.CurrentY + run.VerticalPosition, runFs, runColor, run.Bold, run.Underline, run.CharSpacing, run.FontName, cjkBrkMaxW > 0 ? cjkBrkMaxW : (float?)null, null);
                        FlushLineEntries();
                        pendingText = pendingText[breakAt..];
                        if (!state.IsTopOfPage && state.CurrentY - lineHeight < state.Options.MarginBottom)
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
                        currentX = baseX + EstimateWrapTextWidth(pendingText, runFs, run.Bold, run.CharSpacing, useCalibri);
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
                    var segMaxW = rightEdge - pendingX;
                    BufferOrEmit(pendingText, pendingX, state.CurrentY + run.VerticalPosition, runFs, runColor, run.Bold, run.Underline, run.CharSpacing, run.FontName, segMaxW > 0 ? segMaxW : (float?)null, ulWidth);
                }

            }
        }

        FlushLineEntries();
        state.AdvanceY(lineHeight);
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
    }

    /// <summary>
    /// Renders shape geometry. For "frame" shapes, draws only the border area.
    /// Supports frame, ellipse and custom polygon paths; defaults to rectangle.
    /// </summary>
    private static void RenderShapeGeometry(PdfPage page, float x, float y, float width, float height,
        PdfColor color, DocxShape shape)
    {
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

    private static void RenderImage(RenderState state, DocxImage image)
    {
        const float emuPerPoint = 914400f / 72f;

        var width = image.WidthEmu > 0 ? image.WidthEmu / emuPerPoint : 200f;
        var height = image.HeightEmu > 0 ? image.HeightEmu / emuPerPoint : 150f;

        var format = image.Extension;
        if (format != "jpg" && format != "png")
            return; // Only support JPEG and PNG

        // Anchor images: render at absolute offset position without advancing cursor
        if (image.IsAnchor)
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

            state.CurrentPage!.AddImage(image.Data, format, anchorX, anchorY - height, width, height);
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
        var y = state.CurrentY - height;

        state.CurrentPage!.AddImage(image.Data, format, x, y, width, height);
        state.AdvanceY(height + 1f); // 1pt gap after image
    }

    // ── Header/footer element rendering ─────────────────────────────────

    /// <summary>
    /// Estimates the total height of a list of elements (paragraphs + tables)
    /// for header/footer sizing calculations.
    /// </summary>
    private static float EstimateElementsHeight(List<DocxElement> elements, ConversionOptions options)
    {
        const float emuPerPt = 914400f / 72f;
        float totalHeight = 0;
        foreach (var element in elements)
        {
            switch (element)
            {
                case DocxParagraph para:
                {
                    var fontSize = para.FontSize > 0 ? para.FontSize : options.FontSize;
                    float lineHeight;
                    if (para.LineSpacingAbsolute && para.LineSpacing > 0)
                        lineHeight = para.LineSpacing; // exact/atLeast: absolute points
                    else
                        lineHeight = fontSize * FontMetricsFactor * (para.LineSpacing > 0 ? para.LineSpacing : options.LineSpacing);
                    var usableW = options.PageWidth - options.MarginLeft - options.MarginRight;
                    foreach (var image in para.Images)
                    {
                        if (image.IsAnchor) continue;
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
                    break;
                }
                case DocxTable table:
                {
                    var usableWidth = options.PageWidth - options.MarginLeft - options.MarginRight;
                    var colWidths = CalculateTableColumnWidths(table, usableWidth);
                    var cellPaddingH = (table.CellMarginLeft + table.CellMarginRight) / 2;
                    var cellPaddingV = Math.Max(table.CellMarginTop, table.CellMarginBottom);
                    foreach (var row in table.Rows)
                    {
                        var rowH = CalculateRowHeight(row, colWidths, cellPaddingH, cellPaddingV, options);
                        if (row.Height > 0) rowH = Math.Max(rowH, row.Height);
                        rowH = Math.Max(rowH, CalculateRowInlineImageFloorHeight(row, colWidths, cellPaddingH, cellPaddingV));
                        totalHeight += rowH;
                    }
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

        foreach (var element in elements)
        {
            switch (element)
            {
                case DocxParagraph para:
                {
                    var usableW = options.PageWidth - options.MarginLeft - options.MarginRight;
                    // Render images (inline + behindDoc anchors)
                    foreach (var image in para.Images)
                    {
                        if (image.IsAnchor && !image.IsBehindDoc) continue;
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
                        var imgX = para.Alignment switch
                        {
                            "center" => options.MarginLeft + (usableW - imgW) / 2,
                            "right" => options.MarginLeft + usableW - imgW,
                            _ => options.MarginLeft
                        };
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
                            lineHeight = runFontSize * FontMetricsFactor * (para.LineSpacing > 0 ? para.LineSpacing : options.LineSpacing);
                        var textWidth = EstimateTextWidth(text, runFontSize);
                        var textX = para.Alignment switch
                        {
                            "center" => options.MarginLeft + (usableW - textWidth) / 2,
                            "right" => options.MarginLeft + usableW - textWidth,
                            _ => options.MarginLeft
                        };
                        y -= runFontSize;
                        page.AddText(text, textX, y, runFontSize, firstRun?.Color ?? para.Color,
                            bold: firstRun?.Bold ?? false, preferredFontName: firstRun?.FontName);
                        y -= lineHeight - runFontSize;
                    }
                    break;
                }
                case DocxTable table:
                {
                    y = RenderHeaderFooterTable(page, options, table, y);
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
        var cellPaddingH = (table.CellMarginLeft + table.CellMarginRight) / 2;
        var cellPaddingV = Math.Max(table.CellMarginTop, table.CellMarginBottom);
        var colWidths = CalculateTableColumnWidths(table, usableWidth);
        var colCount = colWidths.Length;

        // Calculate table total width and center offset
        var tableWidth = colWidths.Sum();
        var tableOffsetX = table.Alignment switch
        {
            "center" => options.MarginLeft + (usableWidth - tableWidth) / 2,
            "right" => options.MarginLeft + usableWidth - tableWidth,
            _ => options.MarginLeft
        };

        var y = startY;
        foreach (var row in table.Rows)
        {
            var rowHeight = CalculateRowHeight(row, colWidths, cellPaddingH, cellPaddingV, options);
            if (row.Height > 0) rowHeight = Math.Max(rowHeight, row.Height);
            rowHeight = Math.Max(rowHeight, CalculateRowInlineImageFloorHeight(row, colWidths, cellPaddingH, cellPaddingV));

            var cellX = tableOffsetX;
            var colIdx = 0;

            for (var ci = 0; ci < row.Cells.Count && colIdx < colCount; ci++)
            {
                var cell = row.Cells[ci];
                var cellWidth = colWidths[colIdx];
                if (cell.GridSpan > 1)
                    for (var s = 1; s < cell.GridSpan && colIdx + s < colCount; s++)
                        cellWidth += colWidths[colIdx + s];
                colIdx += cell.GridSpan;
                if (cell.IsVMergeContinue) { cellX += cellWidth; continue; }

                // Vertical alignment
                float vAlignOffset = 0;
                if (cell.VerticalAlignment != "top")
                {
                    var contentHeight = CalculateCellContentHeight(cell, cellWidth, cellPaddingH, cellPaddingV, options);
                    var space = rowHeight - contentHeight;
                    if (space > 0)
                        vAlignOffset = cell.VerticalAlignment == "bottom" ? space : space / 2;
                }

                var textY = y - cellPaddingV - vAlignOffset;
                foreach (var para in cell.Paragraphs)
                {
                    // Render inline images
                    foreach (var image in para.Images)
                    {
                        if (image.IsAnchor) continue;
                        var imgW = image.WidthEmu > 0 ? image.WidthEmu / emuPerPt : 100f;
                        var imgH = image.HeightEmu > 0 ? image.HeightEmu / emuPerPt : 75f;
                        var maxW = cellWidth - cellPaddingH * 2;
                        if (imgW > maxW) { var s = maxW / imgW; imgW *= s; imgH *= s; }
                        var fmt = image.Extension;
                        if (fmt == "jpg" || fmt == "png")
                        {
                            page.AddImage(image.Data, fmt, cellX + cellPaddingH, textY - imgH, imgW, imgH);
                            textY -= imgH + 1f;
                        }
                    }

                    // Render text
                    var text = string.Concat(para.Runs.Select(r => r.Text));
                    if (string.IsNullOrEmpty(text)) continue;

                    var fontSize = para.FontSize > 0 ? para.FontSize : options.FontSize;
                    var firstRun = para.Runs.FirstOrDefault(r => !string.IsNullOrEmpty(r.Text));
                    var runFontSize = firstRun?.FontSize > 0 ? firstRun.FontSize : fontSize;
                    float lineHeight;
                    if (para.LineSpacingAbsolute && para.LineSpacing > 0)
                        lineHeight = para.LineSpacing;
                    else
                        lineHeight = runFontSize * FontMetricsFactor * (para.LineSpacing > 0 ? para.LineSpacing : options.LineSpacing);
                    var textWidth = cellWidth - cellPaddingH * 2;
                    var lines = WordWrap(text, textWidth, textWidth, runFontSize, null,
                        firstRun?.Bold ?? false, firstRun?.CharSpacing ?? 0f, options.UseCalibriWidths);

                    foreach (var line in lines)
                    {
                        textY -= runFontSize;
                        var lineTextWidth = EstimateWrapTextWidth(line, runFontSize,
                            firstRun?.Bold ?? false, firstRun?.CharSpacing ?? 0f, options.UseCalibriWidths);
                        var lineX = para.Alignment switch
                        {
                            "center" => cellX + cellPaddingH + (textWidth - lineTextWidth) / 2,
                            "right" => cellX + cellPaddingH + textWidth - lineTextWidth,
                            _ => cellX + cellPaddingH
                        };

                        float? cellMaxWidth = null;
                        var helveticaWidth = EstimateTextWidth(line, runFontSize, firstRun?.CharSpacing ?? 0f);
                        if (helveticaWidth > textWidth)
                            cellMaxWidth = textWidth;

                        page.AddText(line, lineX, textY, runFontSize, firstRun?.Color ?? para.Color,
                            maxWidth: cellMaxWidth, bold: firstRun?.Bold ?? false,
                            preferredFontName: firstRun?.FontName);
                        textY -= lineHeight - runFontSize;
                    }
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
        var usableWidth = state.UsableWidth;
        var cellPaddingH = (table.CellMarginLeft + table.CellMarginRight) / 2;
        var cellPaddingV = Math.Max(table.CellMarginTop, table.CellMarginBottom);

        // Determine column widths
        var colWidths = CalculateTableColumnWidths(table, usableWidth);
        var colCount = colWidths.Length;

        var isFirstRow = true;
        // Pre-calculate row heights for all rows so we can compute vMerge spans
        var rowHeights = new float[table.Rows.Count];
        for (var ri = 0; ri < table.Rows.Count; ri++)
        {
            var r = table.Rows[ri];
            var ch = CalculateRowHeight(r, colWidths, cellPaddingH, cellPaddingV, options);
            var rh = ch;
            if (r.Height > 0)
            {
                var hasVM = r.Cells.Any(c => c.IsVMergeRestart || c.IsVMergeContinue);
                if (hasVM)
                    rh = r.Height;
                else
                    rh = Math.Max(rh, r.Height);
            }
            rowHeights[ri] = rh;
        }

        for (var rowIndex = 0; rowIndex < table.Rows.Count; rowIndex++)
        {
            var row = table.Rows[rowIndex];
            var rowHeight = rowHeights[rowIndex];

            // Defensive floor for image-heavy rows: ensure inline image extents
            // are always respected, even when upstream row-height hints are small.
            rowHeight = Math.Max(rowHeight, CalculateRowInlineImageFloorHeight(row, colWidths, cellPaddingH, cellPaddingV));

            state.EnsurePage();

            // Check if row fits on current page
            if (state.CurrentY - rowHeight < options.MarginBottom)
            {
                state.CurrentPage = state.Doc.AddPage(options.PageWidth, options.PageHeight);
                state.CurrentY = options.PageHeight - options.MarginTop;
                isFirstRow = true; // new page: draw top border again
            }

            var cellX = options.MarginLeft;
            var colIdx = 0;

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
                float vAlignOffset = 0;
                if (cell.VerticalAlignment != "top")
                {
                    var contentHeight = CalculateCellContentHeight(cell, cellWidth, cellPaddingH, cellPaddingV, options);
                    var space = cellRenderHeight - contentHeight;
                    if (space > 0)
                        vAlignOffset = cell.VerticalAlignment == "bottom" ? space : space / 2;
                }

                // Render cell content (images and text)
                var textY = state.CurrentY - cellPaddingV - vAlignOffset;
                var cellParaList = cell.Paragraphs;
                for (var cellParaIdx = 0; cellParaIdx < cellParaList.Count; cellParaIdx++)
                {
                    var para = cellParaList[cellParaIdx];
                    var isFirstCellPara = cellParaIdx == 0;
                    var isLastCellPara = cellParaIdx == cellParaList.Count - 1;
                    // Apply spacing before (skip for first paragraph in cell)
                    if (!isFirstCellPara && para.SpacingBefore > 0)
                        textY -= para.SpacingBefore;

                    // Render images inside table cells
                    const float emuPerPt = 914400f / 72f;
                    foreach (var image in para.Images)
                    {
                        if (image.IsAnchor) continue; // Skip anchor images in cell flow
                        var imgW = image.WidthEmu > 0 ? image.WidthEmu / emuPerPt : 100f;
                        var imgH = image.HeightEmu > 0 ? image.HeightEmu / emuPerPt : 75f;
                        var maxImgW = cellWidth - cellPaddingH * 2;
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
                            state.CurrentPage!.AddImage(image.Data, fmt, cellX + cellPaddingH, imgY, imgW, imgH);
                            textY -= imgH + 1f;
                        }
                    }

                    var fontSize = para.FontSize > 0 ? para.FontSize : options.FontSize;
                    var text = string.Concat(para.Runs.Select(r => r.Text));
                    if (string.IsNullOrEmpty(text))
                    {
                        // Empty paragraph still takes up space
                        float emptyLineH;
                        if (para.LineSpacingAbsolute && para.LineSpacing > 0)
                            emptyLineH = para.LineSpacing;
                        else
                            emptyLineH = fontSize * FontMetricsFactor * (para.LineSpacing > 0 ? para.LineSpacing : options.LineSpacing);
                        textY -= emptyLineH;
                        // Suppress SpacingAfter for all paragraphs in table cells (TableGrid after=0)
                        continue;
                    }

                    var dominantRun = para.Runs.FirstOrDefault(r => !string.IsNullOrEmpty(r.Text));
                    var runFontSize = dominantRun?.FontSize > 0 ? dominantRun.FontSize : fontSize;
                    var effectiveFontSize = runFontSize;
                    var runColor = dominantRun?.Color ?? para.Color;
                    var cellRunBold = dominantRun?.Bold ?? false;
                    var cellRunUnderline = dominantRun?.Underline ?? false;
                    var cellRunCharSpacing = dominantRun?.CharSpacing ?? 0f;
                    var cellRunFontName = dominantRun?.FontName;
                    float lineHeight;
                    if (para.LineSpacingAbsolute && para.LineSpacing > 0)
                        lineHeight = para.LineSpacing;
                    else
                        lineHeight = effectiveFontSize * FontMetricsFactor * (para.LineSpacing > 0 ? para.LineSpacing : options.LineSpacing);
                    var textWidth = cellWidth - cellPaddingH * 2;
                    var lines = WordWrap(text, textWidth, textWidth, effectiveFontSize, null, cellRunBold, cellRunCharSpacing, options.UseCalibriWidths);

                    foreach (var line in lines)
                    {
                        textY -= effectiveFontSize;
                        if (textY < state.CurrentY - cellRenderHeight + cellPaddingV) break; // clip
                        var lineTextWidth = EstimateWrapTextWidth(line, effectiveFontSize, cellRunBold, cellRunCharSpacing, options.UseCalibriWidths);
                        var lineRenderX = para.Alignment switch
                        {
                            "center" => cellX + cellPaddingH + (textWidth - lineTextWidth) / 2,
                            "right" => cellX + cellPaddingH + textWidth - lineTextWidth,
                            _ => cellX + cellPaddingH
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

                        state.CurrentPage!.AddText(line, lineRenderX, textY, effectiveFontSize, runColor, maxWidth: cellMaxWidth, bold: cellRunBold, underline: cellRunUnderline, charSpacing: cellRunCharSpacing, wordSpacing: cellWordSpacing, preferredFontName: cellRunFontName);
                        textY -= lineHeight - effectiveFontSize;
                    }

                    // Suppress SpacingAfter for all paragraphs in table cells (TableGrid after=0)
                    _ = isLastCellPara; // variable kept for potential future use
                }

                cellX += cellWidth;
            }

            // Draw per-cell borders (or fall back to table-level grid)
            {
                var rowTop = state.CurrentY;
                var rowBottom = state.CurrentY - rowHeight;
                var bx = options.MarginLeft;
                var bci = 0;
                var hasAnyCellBorder = row.Cells.Any(c => c.Borders != null);

                if (hasAnyCellBorder)
                {
                    // Per-cell borders
                    foreach (var cell in row.Cells)
                    {
                        if (bci >= colWidths.Length) break;
                        var bCellWidth = colWidths[bci];
                        if (cell.GridSpan > 1)
                            for (var g = 1; g < cell.GridSpan && bci + g < colWidths.Length; g++)
                                bCellWidth += colWidths[bci + g];

                        var borders = cell.Borders;
                        if (borders != null)
                        {
                            if (borders.Top != null)
                                state.CurrentPage!.AddLine(bx, rowTop, bx + bCellWidth, rowTop, borders.Top.Color, borders.Top.Width);
                            if (borders.Bottom != null)
                                state.CurrentPage!.AddLine(bx, rowBottom, bx + bCellWidth, rowBottom, borders.Bottom.Color, borders.Bottom.Width);
                            if (borders.Left != null)
                                state.CurrentPage!.AddLine(bx, rowTop, bx, rowBottom, borders.Left.Color, borders.Left.Width);
                            if (borders.Right != null)
                                state.CurrentPage!.AddLine(bx + bCellWidth, rowTop, bx + bCellWidth, rowBottom, borders.Right.Color, borders.Right.Width);
                        }

                        bx += bCellWidth;
                        bci += cell.GridSpan > 1 ? cell.GridSpan : 1;
                    }
                }
                else if (table.HasBorders)
                {
                    // Fall back to table-level grid
                    var borderColor = PdfColor.FromRgb(0, 0, 0);
                    const float borderWidth = 0.5f;
                    var tableLeft = options.MarginLeft;
                    var tableRight = options.MarginLeft + colWidths.Sum();

                    if (isFirstRow)
                        state.CurrentPage!.AddLine(tableLeft, rowTop, tableRight, rowTop, borderColor, borderWidth);
                    state.CurrentPage!.AddLine(tableLeft, rowBottom, tableRight, rowBottom, borderColor, borderWidth);

                    var vx = tableLeft;
                    for (var c = 0; c <= colCount; c++)
                    {
                        state.CurrentPage!.AddLine(vx, rowTop, vx, rowBottom, borderColor, borderWidth);
                        if (c < colCount) vx += colWidths[c];
                    }
                }
            }

            isFirstRow = false;
            state.AdvanceY(rowHeight);
        }

        // Add some spacing after table
        state.AdvanceY(2f);
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
            // Use actual DOCX widths (don't scale to usable width)
            return widths;
        }

        // Determine from cell count
        var maxCols2 = table.Rows.Count > 0 ? table.Rows.Max(r => r.Cells.Count) : 1;
        var colWidth = usableWidth / maxCols2;
        var result = new float[maxCols2];
        Compat.ArrayFill(result, colWidth);
        return result;
    }

    private static float CalculateCellContentHeight(DocxTableCell cell, float cellWidth, float cellPaddingH, float cellPaddingV, ConversionOptions options)
    {
        var cellHeight = cellPaddingV * 2;
        var cellParas = cell.Paragraphs;
        for (var pi = 0; pi < cellParas.Count; pi++)
        {
            var para = cellParas[pi];
            var isFirstPara = pi == 0;
            var isLastPara = pi == cellParas.Count - 1;

            if (!isFirstPara && para.SpacingBefore > 0)
                cellHeight += para.SpacingBefore;

            const float emuPerPt = 914400f / 72f;
            foreach (var image in para.Images)
            {
                if (image.IsAnchor) continue;
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
                lineHeight = runFontSize * FontMetricsFactor * (para.LineSpacing > 0 ? para.LineSpacing : options.LineSpacing);
            var textWidth = cellWidth - cellPaddingH * 2;
            var text = string.Concat(para.Runs.Select(r => r.Text));

            if (string.IsNullOrEmpty(text))
            {
                cellHeight += lineHeight;
                // Suppress SpacingAfter for all paragraphs in table cells (TableGrid after=0)
                continue;
            }

            var lines = WordWrap(text, textWidth, textWidth, runFontSize, null, runBold, runCharSpacing, options.UseCalibriWidths);
            cellHeight += lines.Count * lineHeight;
            // Suppress SpacingAfter for all paragraphs in table cells
            // (TableGrid style sets after=0, matching Word/LibreOffice compact cell behavior)
        }

        return cellHeight;
    }

    private static float CalculateRowHeight(DocxTableRow row, float[] colWidths, float cellPaddingH, float cellPaddingV, ConversionOptions options)
    {
        var maxHeight = options.FontSize * FontMetricsFactor * options.LineSpacing + cellPaddingV * 2;

        var colIdx = 0;
        for (var cellIdx = 0; cellIdx < row.Cells.Count && colIdx < colWidths.Length; cellIdx++)
        {
            var cell = row.Cells[cellIdx];
            var span = cell.GridSpan;

            var cellWidth = colWidths[colIdx];
            for (var s = 1; s < span && colIdx + s < colWidths.Length; s++)
                cellWidth += colWidths[colIdx + s];

            colIdx += span;

            if (cell.IsVMergeContinue)
                continue;

            var cellHeight = CalculateCellContentHeight(cell, cellWidth, cellPaddingH, cellPaddingV, options);
            maxHeight = Math.Max(maxHeight, cellHeight);
        }

        return maxHeight;
    }

    private static float CalculateRowInlineImageFloorHeight(DocxTableRow row, float[] colWidths, float cellPaddingH, float cellPaddingV)
    {
        const float emuPerPt = 914400f / 72f;
        var maxHeight = 0f;
        var colIdx = 0;

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
                    // Find the latest CJK break point that fits, respecting kinsoku rules
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
                    }
                    if (breakAt <= 0) break; // No CJK break point found
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
    /// When using Helvetica fallback, applies a kerning/hinting reduction since
    /// Helvetica is wider than most actual document fonts (Times New Roman, PMingLiU, etc.).
    /// </summary>
    private static float EstimateWrapTextWidth(string text, float fontSize, bool bold = false, float charSpacing = 0, bool useCalibriWidths = true)
    {
        if (useCalibriWidths)
            return EstimateCalibrTextWidth(text, fontSize, bold, charSpacing);
        var rawWidth = EstimateTextWidth(text, fontSize, charSpacing);
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
            if (!(w == 1000 && ch >= '\u2E80'))
                latinUnits += actual;
        }
        if (latinUnits > 0 && totalUnits > 0)
        {
            var latinFraction = latinUnits / totalUnits;
            rawWidth *= 1f - latinFraction * 0.22f;
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
        // Calibri Bold is ~3% wider than Calibri Regular on average
        if (bold) latinUnits *= 1.03f;
        // Approximate kerning/hinting reduction: actual rendered text is ~2.3% narrower
        // than raw glyph-width sum due to font-level kerning pairs and grid fitting.
        // Only apply to Latin characters; CJK fonts have no inter-character kerning.
        latinUnits *= 0.977f;
        var totalUnits = latinUnits + cjkUnits;
        var width = fontSize * totalUnits / 1000f;
        if (charSpacing != 0 && text.Length > 1)
            width += charSpacing * (text.Length - 1);
        return width;
    }

    private static int GetHelveticaCharWidth(char ch)
    {
        if (ch < ' ') return 0; // control characters (\n, \r, \t, etc.)
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
    /// </summary>
    private static string AddInterScriptSpacing(string text)
    {
        if (string.IsNullOrEmpty(text) || text.Length < 2) return text;
        var sb = new System.Text.StringBuilder(text.Length + 8);
        sb.Append(text[0]);
        for (var i = 1; i < text.Length; i++)
        {
            if (ShouldInsertInterScriptSpace(text, i))
            {
                sb.Append(' ');
            }
            sb.Append(text[i]);
        }
        return sb.ToString();
    }

    private static bool IsLatinOrDigit(char c) => c is >= '0' and <= '9' or >= 'A' and <= 'Z' or >= 'a' and <= 'z';
    private static bool IsLatinLetter(char c) => c is >= 'A' and <= 'Z' or >= 'a' and <= 'z';
    private static bool IsCjkIdeograph(char c) => c is >= '\u4E00' and <= '\u9FFF' or >= '\u3400' and <= '\u4DBF';

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
}
