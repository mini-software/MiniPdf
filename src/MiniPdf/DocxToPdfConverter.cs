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
        }

        var pdfDoc = new PdfDocument();

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
        }

        var state = new RenderState(pdfDoc, options);
        state.EnsurePage();

        var sectionIndex = 0;
        foreach (var element in docxDoc.Elements)
        {
            switch (element)
            {
                case DocxParagraph paragraph:
                    RenderParagraph(state, paragraph);
                    if (paragraph.SectionBreak != null)
                    {
                        sectionIndex++;
                        if (sectionIndex < sectionLayouts.Count)
                        {
                            var nextLayout = sectionLayouts[sectionIndex];
                            state.Options.PageWidth = nextLayout.PageWidth;
                            state.Options.PageHeight = nextLayout.PageHeight;
                            state.Options.MarginTop = nextLayout.MarginTop;
                            state.Options.MarginBottom = nextLayout.MarginBottom;
                            state.Options.MarginLeft = nextLayout.MarginLeft;
                            state.Options.MarginRight = nextLayout.MarginRight;
                        }
                        state.ForceNewPage();
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
        if (docxDoc.HeaderText != null || docxDoc.FooterText != null)
        {
            const float headerFooterFontSize = 9f;
            var headerColor = PdfColor.FromRgb(128, 128, 128);
            var totalPages = pdfDoc.Pages.Count;
            for (int pi = 0; pi < totalPages; pi++)
            {
                var page = pdfDoc.Pages[pi];
                var usableW = page.Width - options.MarginLeft - options.MarginRight;
                if (docxDoc.HeaderText != null)
                {
                    var headerResolved = docxDoc.HeaderText
                        .Replace("{PAGE}", (pi + 1).ToString())
                        .Replace("{NUMPAGES}", totalPages.ToString());
                    var headerTextWidth = EstimateTextWidth(headerResolved, headerFooterFontSize);
                    var headerX = options.MarginLeft + (usableW - headerTextWidth) / 2;
                    var headerY = page.Height - options.MarginTop / 2;
                    page.AddText(headerResolved, headerX, headerY, headerFooterFontSize, headerColor);
                }
                if (docxDoc.FooterText != null)
                {
                    var footerResolved = docxDoc.FooterText
                        .Replace("{PAGE}", (pi + 1).ToString())
                        .Replace("{NUMPAGES}", totalPages.ToString());
                    var footerTextWidth = EstimateTextWidth(footerResolved, headerFooterFontSize);
                    var footerX = options.MarginLeft + (usableW - footerTextWidth) / 2;
                    var footerY = options.MarginBottom / 2;
                    page.AddText(footerResolved, footerX, footerY, headerFooterFontSize, headerColor);
                }
            }
        }

        return pdfDoc;
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

        public float UsableWidth => Options.PageWidth - Options.MarginLeft - Options.MarginRight;

        public RenderState(PdfDocument doc, ConversionOptions options)
        {
            Doc = doc;
            Options = options;
        }

        public void EnsurePage()
        {
            if (CurrentPage == null || CurrentY < Options.MarginBottom)
            {
                CurrentPage = Doc.AddPage(Options.PageWidth, Options.PageHeight);
                CurrentY = Options.PageHeight - Options.MarginTop;
                IsTopOfPage = true;
            }
        }

        public void AdvanceY(float amount)
        {
            CurrentY -= amount;
            IsTopOfPage = false;
        }

        public void ForceNewPage()
        {
            CurrentPage = Doc.AddPage(Options.PageWidth, Options.PageHeight);
            CurrentY = Options.PageHeight - Options.MarginTop;
            IsTopOfPage = true;
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

        // Apply spacing before (skip at top of page to match Word behavior, unless forced)
        var spacingBefore = paragraph.SpacingBefore > 0 ? paragraph.SpacingBefore : 0;
        if (spacingBefore > 0 && (!state.IsTopOfPage || paragraph.ForceSpacingBefore))
            state.AdvanceY(spacingBefore);

        state.EnsurePage();

        // At top of page, offset by font ascent so text visual top aligns with margin
        if (state.IsTopOfPage)
            state.AdvanceY(fontSize * AscentRatio);

        // Track paragraph start position for borders
        var paragraphStartY = state.CurrentY;

        // Calculate available width considering indentation
        var indentLeft = paragraph.IndentLeft;
        var indentRight = paragraph.IndentRight;
        var availableWidth = state.UsableWidth - indentLeft - indentRight;
        var x = options.MarginLeft + indentLeft;

        // Render list bullet/number
        if ((paragraph.IsBulletList || paragraph.IsNumberedList) && paragraph.ListText != null)
        {
            var listIndent = 18f * (paragraph.ListLevel + 1);
            if (paragraph.IsBulletList)
            {
                // Render bullet as a small filled rectangle (not text) since text
                // extraction differs between fonts — Helvetica "•" vs Symbol U+F0B7.
                var bulletSize = fontSize * 0.25f;
                var bulletX = x + listIndent - 12f;
                var bulletY = state.CurrentY + fontSize * 0.25f;
                state.CurrentPage!.AddRectangle(bulletX, bulletY, bulletSize, bulletSize, new PdfColor(0, 0, 0));
            }
            else
            {
                state.CurrentPage!.AddText(paragraph.ListText, x + listIndent - 12f, state.CurrentY, fontSize);
            }
            x += listIndent;
            availableWidth -= listIndent;
        }

        // First line indent
        var firstLineX = x + Math.Max(0, paragraph.IndentFirstLine);
        var firstLineWidth = availableWidth - Math.Max(0, paragraph.IndentFirstLine);

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
        if (paragraph.Shading != null && paragraph.Runs.Count > 0)
        {
            var shadingHeight = lineHeight;
            var fullText = string.Concat(paragraph.Runs.Select(r => r.Text));
            if (!string.IsNullOrEmpty(fullText))
            {
                var shadingLines = WordWrap(fullText, firstLineWidth, availableWidth, fontSize, paragraph.TabStops);
                shadingHeight = shadingLines.Count * lineHeight;
            }
            state.CurrentPage!.AddRectangle(options.MarginLeft, state.CurrentY - shadingHeight, state.UsableWidth, shadingHeight, paragraph.Shading);
        }

        // If paragraph has no text runs, still account for spacing
        if (paragraph.Runs.Count == 0)
        {
            state.AdvanceY(lineHeight);
            // Apply spacing after
            var spacingAfterEmpty = paragraph.SpacingAfter >= 0 ? paragraph.SpacingAfter : 0f;
            state.AdvanceY(spacingAfterEmpty);

            // Handle page break after (even for empty paragraphs)
            if (paragraph.HasPageBreakAfter)
                state.ForceNewPage();
            return;
        }

        // Check if runs have varying formatting
        // Merge consecutive runs with identical formatting to reduce text extraction artifacts
        var mergedRuns = MergeConsecutiveRuns(paragraph.Runs, fontSize);

        var hasVaryingFormat = mergedRuns.Count > 1 &&
            mergedRuns.Any(r => (r.FontSize > 0 && r.FontSize != fontSize) || r.Color != null
                || r.Bold != mergedRuns[0].Bold || r.Underline != mergedRuns[0].Underline);

        if (hasVaryingFormat)
        {
            // Render each run individually on the same line
            RenderMultiFormatRuns(state, new DocxParagraph(mergedRuns, paragraph.Images, paragraph.Alignment,
                paragraph.SpacingBefore, paragraph.SpacingAfter, paragraph.LineSpacing, paragraph.LineSpacingAbsolute,
                paragraph.IndentLeft, paragraph.IndentRight, paragraph.IndentFirstLine,
                paragraph.IsBulletList, paragraph.IsNumberedList, paragraph.ListLevel, paragraph.ListText,
                paragraph.StyleId, paragraph.Bold, paragraph.Italic, paragraph.FontSize, paragraph.Color,
                paragraph.HasPageBreakBefore, paragraph.HasPageBreakAfter, paragraph.Shading, paragraph.TabStops,
                paragraph.SectionBreak, paragraph.Borders),
                x, firstLineX, availableWidth, firstLineWidth, fontSize, lineHeight);
        }
        else
        {
            // Simple path: all runs share the same formatting
            var fullText = AddInterScriptSpacing(string.Concat(mergedRuns.Select(r => r.Text)));
            var dominantRun = paragraph.Runs.FirstOrDefault(r => !string.IsNullOrEmpty(r.Text));
            var runFontSize = dominantRun?.FontSize > 0 ? dominantRun.FontSize : fontSize;
            var runColor = dominantRun?.Color ?? paragraph.Color;
            var runBold = dominantRun?.Bold ?? false;
            var runUnderline = dominantRun?.Underline ?? false;
            var runCharSpacing = dominantRun?.CharSpacing ?? 0f;

            var lines = WordWrap(fullText, firstLineWidth, availableWidth, runFontSize, paragraph.TabStops, runBold, runCharSpacing);

            // If paragraph has leader tab stops, apply maxWidth so the Tz operator
            // compresses the extra-dot text to fit the intended tab position.
            float? tabLeaderMaxWidth = null;
            if (paragraph.TabStops?.Any(ts => ts.Leader is "dot" or "hyphen" or "underscore") == true)
            {
                tabLeaderMaxWidth = paragraph.TabStops.Max(ts => ts.Position);
            }

            for (var i = 0; i < lines.Count; i++)
            {
                state.EnsurePage();
                if (state.IsTopOfPage)
                    state.AdvanceY(runFontSize * AscentRatio);

                var line = lines[i];
                var lineX = i == 0 ? firstLineX : x;
                var lineW = i == 0 ? firstLineWidth : availableWidth;

                // Use Tz compression to fit Helvetica text into Calibri-width lines\n only when text actually exceeds available width
                var renderMaxWidth = tabLeaderMaxWidth;
                var textWidth = EstimateTextWidth(line, runFontSize, runCharSpacing);
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

                state.CurrentPage!.AddText(line, renderX, state.CurrentY, runFontSize, runColor, maxWidth: renderMaxWidth, bold: runBold, underline: runUnderline, charSpacing: runCharSpacing);
                state.AdvanceY(lineHeight);
            }
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

        // Apply spacing after
        var spacingAfter = paragraph.SpacingAfter >= 0 ? paragraph.SpacingAfter : 0f;
        state.AdvanceY(spacingAfter);

        // Handle page break after
        if (paragraph.HasPageBreakAfter)
            state.ForceNewPage();


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
            var isWhitespaceOnly = string.IsNullOrWhiteSpace(next.Text);
            var isCurWhitespace = string.IsNullOrWhiteSpace(current.Text);
            var colorMatch = current.Color == next.Color || isWhitespaceOnly || isCurWhitespace;
            var boldMatch = current.Bold == next.Bold || isWhitespaceOnly || isCurWhitespace;
            var underlineMatch = current.Underline == next.Underline || isWhitespaceOnly || isCurWhitespace;
            var charSpacingMatch = Math.Abs(current.CharSpacing - next.CharSpacing) < 0.01f || isWhitespaceOnly || isCurWhitespace;
            if (Math.Abs(curFs - nextFs) < 0.01f && colorMatch && boldMatch && underlineMatch && charSpacingMatch
                && !current.IsPageBreak && !next.IsPageBreak)
            {
                current = new DocxRun(current.Text + next.Text, current.Bold, current.Italic || next.Italic,
                    current.FontSize, current.Color, false, current.Underline, current.CharSpacing);
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
        state.EnsurePage();
        if (state.IsTopOfPage)
            state.AdvanceY(defaultFontSize * AscentRatio);
        var currentX = firstLineX;
        var isFirstLine = true;
        var rightEdge = state.Options.MarginLeft + state.UsableWidth - paragraph.IndentRight;

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
                    state.AdvanceY(lineHeight);
                    state.EnsurePage();
                    if (state.IsTopOfPage)
                        state.AdvanceY(runFs * AscentRatio);
                    currentX = baseX;
                    isFirstLine = false;
                }

                var segment = AddInterScriptSpacing(hardLines[hi]);
                if (string.IsNullOrEmpty(segment)) continue;

                // Split segment by spaces for word wrapping, but accumulate text
                // per line to produce fewer AddText calls (improves text extraction).
                var words = segment.Split(' ');
                var pendingText = "";
                var pendingX = currentX;

                for (var wi = 0; wi < words.Length; wi++)
                {
                    var word = words[wi];
                    var wordWidth = EstimateTextWidth(word, runFs, run.CharSpacing);
                    var spaceWidth = wi > 0 ? runFs * GetHelveticaCharWidth(' ') / 1000f + run.CharSpacing : 0;

                    // Check if word fits on current line
                    if (currentX + spaceWidth + wordWidth > rightEdge && (pendingText.Length > 0 || currentX > baseX + 1))
                    {
                        // Flush pending text before wrapping
                        if (pendingText.Length > 0)
                        {
                            state.CurrentPage!.AddText(pendingText, pendingX, state.CurrentY, runFs, runColor, bold: run.Bold, underline: run.Underline, charSpacing: run.CharSpacing);
                            pendingText = "";
                        }
                        // Wrap to next line
                        state.AdvanceY(lineHeight);
                        state.EnsurePage();
                        if (state.IsTopOfPage)
                            state.AdvanceY(runFs * AscentRatio);
                        currentX = baseX;
                        pendingX = currentX;
                        isFirstLine = false;
                        spaceWidth = 0;
                    }

                    if (wi > 0)
                    {
                        pendingText += " ";
                        currentX += spaceWidth;
                    }

                    pendingText += word;
                    currentX += wordWidth;

                    // Break oversized CJK words at character boundaries (kinsoku)
                    while (currentX > rightEdge && pendingText.Length > 1)
                    {
                        var breakAt = -1;
                        float accWidth = 0;
                        for (var ci = 0; ci < pendingText.Length; ci++)
                        {
                            accWidth += runFs * GetHelveticaCharWidth(pendingText[ci]) / 1000f;
                            if (pendingX + accWidth > rightEdge && breakAt >= 0)
                                break;
                            if (ci > 0 && (GetHelveticaCharWidth(pendingText[ci]) == 1000 || GetHelveticaCharWidth(pendingText[ci - 1]) == 1000))
                            {
                                if (!IsNoStartChar(pendingText[ci]))
                                    breakAt = ci;
                            }
                        }
                        if (breakAt <= 0) break;
                        state.CurrentPage!.AddText(pendingText[..breakAt], pendingX, state.CurrentY, runFs, runColor, bold: run.Bold, underline: run.Underline, charSpacing: run.CharSpacing);
                        pendingText = pendingText[breakAt..];
                        state.AdvanceY(lineHeight);
                        state.EnsurePage();
                        if (state.IsTopOfPage)
                            state.AdvanceY(runFs * AscentRatio);
                        currentX = baseX + runFs * pendingText.Sum(c => GetHelveticaCharWidth(c)) / 1000f;
                        pendingX = baseX;
                        isFirstLine = false;
                    }
                }

                // Flush remaining text for this segment
                if (pendingText.Length > 0)
                {
                    state.CurrentPage!.AddText(pendingText, pendingX, state.CurrentY, runFs, runColor, bold: run.Bold, underline: run.Underline, charSpacing: run.CharSpacing);
                }
            }
        }

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

        state.CurrentPage!.AddRectangle(x, y, width, height, blended);
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

        page.AddRectangle(x, y, width, height, blended);
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

    // ── Table rendering ─────────────────────────────────────────────────

    private static void RenderTable(RenderState state, DocxTable table)
    {
        var options = state.Options;
        var usableWidth = state.UsableWidth;
        var cellPaddingH = 5.4f;  // horizontal (left/right) cell padding
        var cellPaddingV = 1f;    // vertical (top/bottom) cell padding

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

                // Render cell content (images and text)
                var textY = state.CurrentY - cellPaddingV;
                var isFirstCellPara = true;
                foreach (var para in cell.Paragraphs)
                {
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
                        var emptyLineH = fontSize * FontMetricsFactor * (para.LineSpacing > 0 ? para.LineSpacing : options.LineSpacing);
                        textY -= emptyLineH;
                        if (para.SpacingAfter >= 0) textY -= para.SpacingAfter;
                        isFirstCellPara = false;
                        continue;
                    }

                    var dominantRun = para.Runs.FirstOrDefault(r => !string.IsNullOrEmpty(r.Text));
                    var runFontSize = dominantRun?.FontSize > 0 ? dominantRun.FontSize : fontSize;
                    var effectiveFontSize = runFontSize;
                    var runColor = dominantRun?.Color ?? para.Color;
                    var cellRunBold = dominantRun?.Bold ?? false;
                    var cellRunUnderline = dominantRun?.Underline ?? false;
                    var cellRunCharSpacing = dominantRun?.CharSpacing ?? 0f;
                    var lineHeight = effectiveFontSize * FontMetricsFactor * (para.LineSpacing > 0 ? para.LineSpacing : options.LineSpacing);
                    var textWidth = cellWidth - cellPaddingH * 2;
                    var lines = WordWrap(text, textWidth, textWidth, effectiveFontSize, null, cellRunBold, cellRunCharSpacing);

                    foreach (var line in lines)
                    {
                        textY -= effectiveFontSize;
                        if (textY < state.CurrentY - cellRenderHeight + cellPaddingV) break; // clip
                        var lineTextWidth = EstimateTextWidth(line, effectiveFontSize, cellRunCharSpacing);
                        var lineRenderX = para.Alignment switch
                        {
                            "center" => cellX + cellPaddingH + (textWidth - lineTextWidth) / 2,
                            "right" => cellX + cellPaddingH + textWidth - lineTextWidth,
                            _ => cellX + cellPaddingH
                        };
                        state.CurrentPage!.AddText(line, lineRenderX, textY, effectiveFontSize, runColor, bold: cellRunBold, underline: cellRunUnderline, charSpacing: cellRunCharSpacing);
                        textY -= lineHeight - effectiveFontSize;
                    }

                    // Apply spacing after
                    if (para.SpacingAfter > 0) textY -= para.SpacingAfter;
                    isFirstCellPara = false;
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
            if (total > 0)
            {
                // Scale to fit usable width
                var scale = usableWidth / total;
                for (var i = 0; i < widths.Length; i++)
                    widths[i] *= scale;
            }
            return widths;
        }

        // Determine from cell count
        var maxCols = table.Rows.Count > 0 ? table.Rows.Max(r => r.Cells.Count) : 1;
        var colWidth = usableWidth / maxCols;
        var result = new float[maxCols];
        Array.Fill(result, colWidth);
        return result;
    }

    private static float CalculateRowHeight(DocxTableRow row, float[] colWidths, float cellPaddingH, float cellPaddingV, ConversionOptions options)
    {
        var maxHeight = options.FontSize * FontMetricsFactor * options.LineSpacing + cellPaddingV * 2;

        var colIdx = 0;
        for (var cellIdx = 0; cellIdx < row.Cells.Count && colIdx < colWidths.Length; cellIdx++)
        {
            var cell = row.Cells[cellIdx];
            var span = cell.GridSpan;

            // Calculate cell width from column widths
            var cellWidth = colWidths[colIdx];
            for (var s = 1; s < span && colIdx + s < colWidths.Length; s++)
                cellWidth += colWidths[colIdx + s];

            // Advance column index past spanned columns
            colIdx += span;

            // Skip vertically merged continuation cells in height calculation
            if (cell.IsVMergeContinue)
                continue;

            var cellHeight = cellPaddingV * 2;
            var isFirstPara = true;
            foreach (var para in cell.Paragraphs)
            {
                // Account for paragraph spacing
                if (!isFirstPara && para.SpacingBefore > 0)
                    cellHeight += para.SpacingBefore;

                // Account for images in row height
                const float emuPerPt = 914400f / 72f;
                foreach (var image in para.Images)
                {
                    if (image.IsAnchor) continue; // Anchor images don't consume cell space
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
                var effectiveFontSize = runFontSize;
                var runCharSpacing = dominantRun?.CharSpacing ?? 0f;
                var runBold = dominantRun?.Bold ?? false;
                var lineHeight = effectiveFontSize * FontMetricsFactor * (para.LineSpacing > 0 ? para.LineSpacing : options.LineSpacing);
                var textWidth = cellWidth - cellPaddingH * 2;
                var text = string.Concat(para.Runs.Select(r => r.Text));

                if (string.IsNullOrEmpty(text))
                {
                    cellHeight += lineHeight;
                    if (para.SpacingAfter > 0) cellHeight += para.SpacingAfter;
                    isFirstPara = false;
                    continue;
                }

                var lines = WordWrap(text, textWidth, textWidth, effectiveFontSize, null, runBold, runCharSpacing);
                cellHeight += lines.Count * lineHeight;
                if (para.SpacingAfter > 0) cellHeight += para.SpacingAfter;
                isFirstPara = false;
            }

            maxHeight = Math.Max(maxHeight, cellHeight);
        }

        return maxHeight;
    }

    // ── Word wrapping ───────────────────────────────────────────────────

    private static string ExpandTabs(string text, float fontSize, List<DocxTabStop>? tabStops = null)
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
                    var currentWidth = EstimateTextWidth(sb.ToString(), fontSize);
                    DocxTabStop? matchedStop = null;
                    foreach (var ts in tabStops)
                    {
                        if (ts.Position > currentWidth)
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
                        var gapWidth = matchedStop.Position - currentWidth - remainingTextWidth;
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
        var spaceWidth = fontSize * GetHelveticaCharWidth(' ') / 1000f;
        var tabChars = Math.Max(4, (int)(defaultTabStopPt / spaceWidth));
        var sb2 = new System.Text.StringBuilder();
        var col = 0;
        foreach (var ch in text)
        {
            if (ch == '\t')
            {
                var next = ((col / tabChars) + 1) * tabChars;
                var spaces = next - col;
                sb2.Append(' ', spaces);
                col = next;
            }
            else
            {
                sb2.Append(ch);
                col++;
            }
        }
        return sb2.ToString();
    }

    private static List<string> WordWrap(string text, float firstLineWidth, float subsequentWidth, float fontSize, List<DocxTabStop>? tabStops = null, bool bold = false, float charSpacing = 0)
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

        text = ExpandTabs(text, fontSize, tabStops);

        var lines = new List<string>();
        var paragraphLines = text.Split('\n');

        foreach (var pLine in paragraphLines)
        {
            if (string.IsNullOrEmpty(pLine))
            {
                lines.Add("");
                continue;
            }

            var words = pLine.Split(' ');
            var currentLine = "";
            var maxWidth = lines.Count == 0 ? firstLineWidth : subsequentWidth;

            foreach (var word in words)
            {
                if (currentLine.Length == 0)
                {
                    currentLine = word;
                }
                else if (EstimateCalibrTextWidth(currentLine + " " + word, fontSize, bold, charSpacing) <= maxWidth)
                {
                    currentLine += " " + word;
                }
                else
                {
                    lines.Add(currentLine);
                    currentLine = word;
                    maxWidth = subsequentWidth;
                }

                // Break oversized words only at CJK character boundaries
                while (EstimateCalibrTextWidth(currentLine, fontSize, bold, charSpacing) > maxWidth && currentLine.Length > 1)
                {
                    // Find the latest CJK break point that fits, respecting kinsoku rules
                    var breakAt = -1;
                    for (var ci = 1; ci < currentLine.Length; ci++)
                    {
                        if (EstimateCalibrTextWidth(currentLine[..ci], fontSize, bold, charSpacing) > maxWidth)
                            break;
                        // Allow breaking before or after a CJK character
                        if (GetHelveticaCharWidth(currentLine[ci]) == 1000 || GetHelveticaCharWidth(currentLine[ci - 1]) == 1000)
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
    /// Estimates the rendered width of a text string using Helvetica font metrics.
    /// </summary>
    private static float EstimateTextWidth(string text, float fontSize, float charSpacing = 0)
    {
        float totalUnits = 0;
        foreach (var ch in text)
            totalUnits += GetHelveticaCharWidth(ch);
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
        float totalUnits = 0;
        foreach (var ch in text)
            totalUnits += GetCalibrCharWidth(ch);
        // Calibri Bold is ~5% wider than Calibri Regular on average
        if (bold) totalUnits *= 1.05f;
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
    /// Inserts a space between Latin-script/digit characters and CJK characters
    /// to match Word/LibreOffice's automatic inter-script spacing behavior.
    /// LibreOffice only adds space at Latin→CJK boundaries (not CJK→Latin).
    /// </summary>
    private static string AddInterScriptSpacing(string text)
    {
        if (string.IsNullOrEmpty(text) || text.Length < 2) return text;
        var sb = new System.Text.StringBuilder(text.Length + 8);
        sb.Append(text[0]);
        for (var i = 1; i < text.Length; i++)
        {
            var prev = text[i - 1];
            var curr = text[i];
            // Insert space at Latin→CJK boundary only
            if (IsLatinOrDigit(prev) && IsCjkIdeograph(curr))
            {
                sb.Append(' ');
            }
            sb.Append(curr);
        }
        return sb.ToString();

        static bool IsLatinOrDigit(char c) => c is >= '0' and <= '9' or >= 'A' and <= 'Z' or >= 'a' and <= 'z';
        static bool IsCjkIdeograph(char c) => c is >= '\u4E00' and <= '\u9FFF' or >= '\u3400' and <= '\u4DBF';
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
    private static readonly int[] CalibrWidths =
    [
        226, // ' ' (32)
        247, // !
        340, // "
        510, // #
        460, // $
        668, // %
        592, // &
        183, // '
        309, // (
        309, // )
        363, // *
        510, // +
        228, // ,
        306, // -
        228, // .
        321, // /
        507, 507, 507, 507, 507, 507, 507, 507, 507, 507, // 0-9
        228, // :
        228, // ;
        510, // <
        510, // =
        510, // >
        417, // ?
        813, // @
        536, 533, 488, 574, 459, 432, 539, 580, 252, // A-I
        317, 516, 447, 698, 586, 570, 507, 570, 534, 488, // J-S
        479, 564, 508, 730, 492, 475, 468, // T-Z
        268, // [
        321, // backslash
        268, // ]
        510, // ^
        327, // _
        500, // `
        494, 537, 418, 537, 478, 274, 506, 538, 228, // a-i
        228, 460, 228, 832, 538, 510, 537, 537, 327, 393, // j-s
        312, 538, 428, 656, 449, 428, 408, // t-z
        334, // {
        256, // |
        334, // }
        510, // ~
    ];
}
