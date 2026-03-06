namespace MiniSoftware;

/// <summary>
/// Converts Word (.docx) files to PDF documents.
/// Renders paragraphs, tables, and images using the built-in Helvetica font.
/// </summary>
internal static class DocxToPdfConverter
{
    // Single-spaced line height ≈ fontSize × this factor (ascent + descent for typical fonts)
    private const float FontMetricsFactor = 1.18f;

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

        var state = new RenderState(pdfDoc, options);
        state.EnsurePage();

        foreach (var element in docxDoc.Elements)
        {
            switch (element)
            {
                case DocxParagraph paragraph:
                    RenderParagraph(state, paragraph);
                    break;
                case DocxTable table:
                    RenderTable(state, table);
                    break;
            }
        }

        // Ensure at least one page exists
        if (pdfDoc.Pages.Count == 0)
            pdfDoc.AddPage(options.PageWidth, options.PageHeight);

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
        var lineSpacingMul = paragraph.LineSpacing > 0 ? paragraph.LineSpacing : options.LineSpacing;
        var lineHeight = fontSize * FontMetricsFactor * lineSpacingMul;
        var avgCharWidth = fontSize * 0.47f;

        // Apply spacing before (skip at top of page to match Word behavior)
        var spacingBefore = paragraph.SpacingBefore > 0 ? paragraph.SpacingBefore : 0;
        if (spacingBefore > 0 && !state.IsTopOfPage)
            state.AdvanceY(spacingBefore);

        state.EnsurePage();

        // Calculate available width considering indentation
        var indentLeft = paragraph.IndentLeft;
        var indentRight = paragraph.IndentRight;
        var availableWidth = state.UsableWidth - indentLeft - indentRight;
        var x = options.MarginLeft + indentLeft;

        // Render list bullet/number
        if ((paragraph.IsBulletList || paragraph.IsNumberedList) && paragraph.ListText != null)
        {
            var listIndent = 18f * (paragraph.ListLevel + 1);
            state.CurrentPage!.AddText(paragraph.ListText, x + listIndent - 12f, state.CurrentY, fontSize);
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

        // Render paragraph background shading
        if (paragraph.Shading != null && paragraph.Runs.Count > 0)
        {
            var shadingHeight = lineHeight;
            var fullText = string.Concat(paragraph.Runs.Select(r => r.Text));
            if (!string.IsNullOrEmpty(fullText))
            {
                var shadingAvgCW = fontSize * 0.47f;
                var shadingLines = WordWrap(fullText, firstLineWidth, availableWidth, shadingAvgCW, paragraph.TabStops);
                shadingHeight = shadingLines.Count * lineHeight;
            }
            state.CurrentPage!.AddRectangle(options.MarginLeft, state.CurrentY - shadingHeight, state.UsableWidth, shadingHeight, paragraph.Shading);
        }

        // If paragraph has no text runs, still account for spacing
        if (paragraph.Runs.Count == 0)
        {
            state.AdvanceY(lineHeight);
            // Apply spacing after
            var spacingAfterEmpty = paragraph.SpacingAfter >= 0 ? paragraph.SpacingAfter : fontSize * 0.35f;
            state.AdvanceY(spacingAfterEmpty);

            // Handle page break after (even for empty paragraphs)
            if (paragraph.HasPageBreakAfter)
                state.ForceNewPage();
            return;
        }

        // Check if runs have varying formatting
        var hasVaryingFormat = paragraph.Runs.Count > 1 &&
            paragraph.Runs.Any(r => (r.FontSize > 0 && r.FontSize != fontSize) || r.Color != null);

        if (hasVaryingFormat)
        {
            // Render each run individually on the same line
            RenderMultiFormatRuns(state, paragraph, x, firstLineX, availableWidth, firstLineWidth, fontSize, lineHeight);
        }
        else
        {
            // Simple path: all runs share the same formatting
            var fullText = string.Concat(paragraph.Runs.Select(r => r.Text));
            var dominantRun = paragraph.Runs.FirstOrDefault(r => !string.IsNullOrEmpty(r.Text));
            var runFontSize = dominantRun?.FontSize > 0 ? dominantRun.FontSize : fontSize;
            var runColor = dominantRun?.Color ?? paragraph.Color;
            var runAvgCharWidth = runFontSize * 0.47f;

            var lines = WordWrap(fullText, firstLineWidth, availableWidth, runAvgCharWidth, paragraph.TabStops);

            for (var i = 0; i < lines.Count; i++)
            {
                state.EnsurePage();

                var line = lines[i];
                var lineX = i == 0 ? firstLineX : x;
                var lineW = i == 0 ? firstLineWidth : availableWidth;

                var textWidth = line.Length * runAvgCharWidth;
                var renderX = paragraph.Alignment switch
                {
                    "center" => lineX + (lineW - textWidth) / 2,
                    "right" => lineX + lineW - textWidth,
                    _ => lineX
                };

                state.CurrentPage!.AddText(line, renderX, state.CurrentY, runFontSize, runColor);
                state.AdvanceY(lineHeight);
            }
        }

        // Apply spacing after
        var defaultSpacing = (paragraph.IsBulletList || paragraph.IsNumberedList) ? 0f : fontSize * 0.35f;
        var spacingAfter = paragraph.SpacingAfter >= 0 ? paragraph.SpacingAfter : defaultSpacing;
        state.AdvanceY(spacingAfter);

        // Handle page break after
        if (paragraph.HasPageBreakAfter)
            state.ForceNewPage();
    }

    /// <summary>
    /// Renders runs with varying font sizes/colors on the same line(s).
    /// </summary>
    private static void RenderMultiFormatRuns(RenderState state, DocxParagraph paragraph,
        float baseX, float firstLineX, float availableWidth, float firstLineWidth,
        float defaultFontSize, float lineHeight)
    {
        state.EnsurePage();
        var currentX = firstLineX;
        var maxWidth = firstLineWidth;
        var isFirstLine = true;

        foreach (var run in paragraph.Runs)
        {
            if (string.IsNullOrEmpty(run.Text)) continue;

            var runFs = run.FontSize > 0 ? run.FontSize : defaultFontSize;
            var runColor = run.Color ?? paragraph.Color;
            var runCharWidth = runFs * 0.47f;

            // Split run text by spaces for word wrapping
            var words = run.Text.Split(' ');
            for (var wi = 0; wi < words.Length; wi++)
            {
                var word = words[wi];
                var wordWidth = word.Length * runCharWidth;
                var spaceWidth = wi > 0 ? runCharWidth : 0;

                // Check if word fits on current line
                if (currentX + spaceWidth + wordWidth > baseX + (isFirstLine ? firstLineWidth : availableWidth) + state.Options.MarginLeft)
                {
                    // Wrap to next line
                    state.AdvanceY(lineHeight);
                    state.EnsurePage();
                    currentX = baseX;
                    isFirstLine = false;
                    spaceWidth = 0;
                }

                if (wi > 0 && spaceWidth > 0)
                    currentX += spaceWidth;

                state.CurrentPage!.AddText(word, currentX, state.CurrentY, runFs, runColor);
                currentX += wordWidth;

                // Add space after word (except last)
                if (wi < words.Length - 1)
                    currentX += runCharWidth;
            }
        }

        state.AdvanceY(lineHeight);
    }

    // ── Image rendering ─────────────────────────────────────────────────

    private static void RenderImage(RenderState state, DocxImage image)
    {
        const float emuPerPoint = 914400f / 72f;

        var width = image.WidthEmu > 0 ? image.WidthEmu / emuPerPoint : 200f;
        var height = image.HeightEmu > 0 ? image.HeightEmu / emuPerPoint : 150f;

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

        var format = image.Extension;
        if (format != "jpg" && format != "png")
            return; // Only support JPEG and PNG

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

        foreach (var row in table.Rows)
        {
            state.EnsurePage();

            // Calculate row height based on cell content
            var rowHeight = CalculateRowHeight(row, colWidths, cellPaddingH, cellPaddingV, options);

            // Check if row fits on current page
            if (state.CurrentY - rowHeight < options.MarginBottom)
            {
                state.CurrentPage = state.Doc.AddPage(options.PageWidth, options.PageHeight);
                state.CurrentY = options.PageHeight - options.MarginTop;
            }

            var cellX = options.MarginLeft;

            for (var ci = 0; ci < row.Cells.Count && ci < colCount; ci++)
            {
                var cell = row.Cells[ci];
                var cellWidth = colWidths[ci];

                // Handle grid span
                if (cell.GridSpan > 1)
                {
                    for (var s = 1; s < cell.GridSpan && ci + s < colCount; s++)
                        cellWidth += colWidths[ci + s];
                }

                // Draw cell shading
                if (cell.Shading != null)
                {
                    state.CurrentPage!.AddRectangle(cellX, state.CurrentY - rowHeight, cellWidth, rowHeight, cell.Shading);
                }

                // Draw cell border
                if (table.HasBorders)
                {
                    var borderColor = PdfColor.FromRgb(0, 0, 0);
                    const float borderWidth = 0.5f;
                    state.CurrentPage!.AddLine(cellX, state.CurrentY, cellX + cellWidth, state.CurrentY, borderColor, borderWidth);
                    state.CurrentPage!.AddLine(cellX, state.CurrentY - rowHeight, cellX + cellWidth, state.CurrentY - rowHeight, borderColor, borderWidth);
                    state.CurrentPage!.AddLine(cellX, state.CurrentY, cellX, state.CurrentY - rowHeight, borderColor, borderWidth);
                    state.CurrentPage!.AddLine(cellX + cellWidth, state.CurrentY, cellX + cellWidth, state.CurrentY - rowHeight, borderColor, borderWidth);
                }

                // Render cell content (images and text)
                var textY = state.CurrentY - cellPaddingV;
                foreach (var para in cell.Paragraphs)
                {
                    // Render images inside table cells
                    const float emuPerPt = 914400f / 72f;
                    foreach (var image in para.Images)
                    {
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
                    if (string.IsNullOrEmpty(text)) continue;

                    var runFontSize = para.Runs.FirstOrDefault(r => !string.IsNullOrEmpty(r.Text))?.FontSize;
                    var effectiveFontSize = runFontSize > 0 ? runFontSize.Value : fontSize;
                    var runColor = para.Runs.FirstOrDefault(r => !string.IsNullOrEmpty(r.Text))?.Color ?? para.Color;
                var lineHeight = effectiveFontSize * FontMetricsFactor * (para.LineSpacing > 0 ? para.LineSpacing : options.LineSpacing);
                    var avgCharWidth = effectiveFontSize * 0.47f;
                    var textWidth = cellWidth - cellPaddingH * 2;
                    var lines = WordWrap(text, textWidth, textWidth, avgCharWidth);

                    foreach (var line in lines)
                    {
                        textY -= effectiveFontSize;
                        if (textY < state.CurrentY - rowHeight + cellPaddingV) break; // clip
                        state.CurrentPage!.AddText(line, cellX + cellPaddingH, textY, effectiveFontSize, runColor);
                        textY -= lineHeight - effectiveFontSize;
                    }
                }

                cellX += cellWidth;

                // Skip columns covered by grid span
                if (cell.GridSpan > 1)
                    ci += cell.GridSpan - 1;
            }

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

        for (var ci = 0; ci < row.Cells.Count && ci < colWidths.Length; ci++)
        {
            var cell = row.Cells[ci];
            var cellWidth = colWidths[ci];
            if (cell.GridSpan > 1)
            {
                for (var s = 1; s < cell.GridSpan && ci + s < colWidths.Length; s++)
                    cellWidth += colWidths[ci + s];
            }

            var cellHeight = cellPaddingV * 2;
            foreach (var para in cell.Paragraphs)
            {
                // Account for images in row height
                const float emuPerPt = 914400f / 72f;
                foreach (var image in para.Images)
                {
                    var imgW = image.WidthEmu > 0 ? image.WidthEmu / emuPerPt : 100f;
                    var imgH = image.HeightEmu > 0 ? image.HeightEmu / emuPerPt : 75f;
                    var maxImgW = cellWidth - cellPaddingH * 2;
                    if (imgW > maxImgW)
                        imgH *= maxImgW / imgW;
                    cellHeight += imgH + 1f;
                }

                var fontSize = para.FontSize > 0 ? para.FontSize : options.FontSize;
                var runFontSize = para.Runs.FirstOrDefault(r => !string.IsNullOrEmpty(r.Text))?.FontSize;
                var effectiveFontSize = runFontSize > 0 ? runFontSize.Value : fontSize;
                var lineHeight = effectiveFontSize * FontMetricsFactor * (para.LineSpacing > 0 ? para.LineSpacing : options.LineSpacing);
                var avgCharWidth = effectiveFontSize * 0.47f;
                var textWidth = cellWidth - cellPaddingH * 2;
                var text = string.Concat(para.Runs.Select(r => r.Text));

                if (string.IsNullOrEmpty(text))
                {
                    cellHeight += lineHeight;
                    continue;
                }

                var lines = WordWrap(text, textWidth, textWidth, avgCharWidth);
                cellHeight += lines.Count * lineHeight;
            }

            maxHeight = Math.Max(maxHeight, cellHeight);
        }

        return maxHeight;
    }

    // ── Word wrapping ───────────────────────────────────────────────────

    private static string ExpandTabs(string text, float avgCharWidth, List<DocxTabStop>? tabStops = null)
    {
        if (!text.Contains('\t'))
            return text;

        // If tab stops define dot leaders, use them
        if (tabStops is { Count: > 0 })
        {
            var sb = new System.Text.StringBuilder();
            var segments = text.Split('\t');
            var currentCol = 0;
            for (var i = 0; i < segments.Length; i++)
            {
                sb.Append(segments[i]);
                currentCol += segments[i].Length;

                if (i < segments.Length - 1)
                {
                    // Find the next tab stop beyond current position
                    var currentPos = currentCol * avgCharWidth;
                    DocxTabStop? matchedStop = null;
                    foreach (var ts in tabStops)
                    {
                        if (ts.Position > currentPos + avgCharWidth)
                        {
                            matchedStop = ts;
                            break;
                        }
                    }

                    if (matchedStop != null)
                    {
                        var targetCol = (int)(matchedStop.Position / avgCharWidth);
                        var fillCount = Math.Max(1, targetCol - currentCol);
                        var leaderChar = matchedStop.Leader switch
                        {
                            "dot" => '.',
                            "hyphen" => '-',
                            "underscore" => '_',
                            _ => ' '
                        };
                        sb.Append(leaderChar, fillCount);
                        currentCol = targetCol;
                    }
                    else
                    {
                        // No matching tab stop; use default spacing
                        sb.Append(' ', 4);
                        currentCol += 4;
                    }
                }
            }
            return sb.ToString();
        }

        const float defaultTabStopPt = 36f; // 0.5 inch default tab stop in points
        var tabChars = (int)(defaultTabStopPt / avgCharWidth);
        if (tabChars < 4) tabChars = 4;
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

    private static List<string> WordWrap(string text, float firstLineWidth, float subsequentWidth, float avgCharWidth, List<DocxTabStop>? tabStops = null)
    {
        if (string.IsNullOrEmpty(text))
            return [""];

        text = ExpandTabs(text, avgCharWidth, tabStops);

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
            var maxChars = (int)(maxWidth / avgCharWidth);
            if (maxChars < 1) maxChars = 1;

            foreach (var word in words)
            {
                if (currentLine.Length == 0)
                {
                    currentLine = word;
                }
                else if ((currentLine.Length + 1 + word.Length) * avgCharWidth <= maxWidth)
                {
                    currentLine += " " + word;
                }
                else
                {
                    lines.Add(currentLine);
                    currentLine = word;
                    maxWidth = subsequentWidth;
                    maxChars = (int)(maxWidth / avgCharWidth);
                    if (maxChars < 1) maxChars = 1;
                }
            }

            if (currentLine.Length > 0)
                lines.Add(currentLine);
        }

        if (lines.Count == 0)
            lines.Add("");

        return lines;
    }
}
