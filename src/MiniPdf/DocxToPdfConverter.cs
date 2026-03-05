namespace MiniSoftware;

/// <summary>
/// Converts Word (.docx) files to PDF documents.
/// Renders paragraphs, tables, and images using the built-in Helvetica font.
/// </summary>
internal static class DocxToPdfConverter
{
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
            }
        }

        public void AdvanceY(float amount)
        {
            CurrentY -= amount;
        }
    }

    // ── Paragraph rendering ─────────────────────────────────────────────

    private static void RenderParagraph(RenderState state, DocxParagraph paragraph)
    {
        var options = state.Options;
        var fontSize = paragraph.FontSize > 0 ? paragraph.FontSize : options.FontSize;
        var lineHeight = paragraph.LineSpacing > 0 ? paragraph.LineSpacing : fontSize * options.LineSpacing;
        var avgCharWidth = fontSize * 0.47f;

        // Apply spacing before
        var spacingBefore = paragraph.SpacingBefore > 0 ? paragraph.SpacingBefore : 0;
        if (spacingBefore > 0)
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

        // If paragraph has no text runs, still account for spacing
        if (paragraph.Runs.Count == 0)
        {
            state.AdvanceY(lineHeight);
            // Apply spacing after
            var spacingAfterEmpty = paragraph.SpacingAfter > 0 ? paragraph.SpacingAfter : fontSize * 0.5f;
            state.AdvanceY(spacingAfterEmpty);
            return;
        }

        // Concatenate all run texts for word wrapping
        var fullText = string.Concat(paragraph.Runs.Select(r => r.Text));

        // Determine dominant run properties (use first run with content)
        var dominantRun = paragraph.Runs.FirstOrDefault(r => !string.IsNullOrEmpty(r.Text));
        var runFontSize = dominantRun?.FontSize > 0 ? dominantRun.FontSize : fontSize;
        var runColor = dominantRun?.Color ?? paragraph.Color;
        var runAvgCharWidth = runFontSize * 0.47f;

        // Word wrap the text
        var lines = WordWrap(fullText, firstLineWidth, availableWidth, runAvgCharWidth);

        for (var i = 0; i < lines.Count; i++)
        {
            state.EnsurePage();

            var line = lines[i];
            var lineX = i == 0 ? firstLineX : x;
            var lineW = i == 0 ? firstLineWidth : availableWidth;

            // Apply alignment
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

        // Apply spacing after
        var spacingAfter = paragraph.SpacingAfter > 0 ? paragraph.SpacingAfter : fontSize * 0.5f;
        state.AdvanceY(spacingAfter);
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
        state.AdvanceY(height + 4f); // 4pt gap after image
    }

    // ── Table rendering ─────────────────────────────────────────────────

    private static void RenderTable(RenderState state, DocxTable table)
    {
        var options = state.Options;
        var usableWidth = state.UsableWidth;
        var cellPadding = 4f;

        // Determine column widths
        var colWidths = CalculateTableColumnWidths(table, usableWidth);
        var colCount = colWidths.Length;

        foreach (var row in table.Rows)
        {
            state.EnsurePage();

            // Calculate row height based on cell content
            var rowHeight = CalculateRowHeight(row, colWidths, cellPadding, options);

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
                state.CurrentPage!.AddLine(cellX, state.CurrentY, cellX + cellWidth, state.CurrentY, PdfColor.FromRgb(200, 200, 200));
                state.CurrentPage!.AddLine(cellX, state.CurrentY - rowHeight, cellX + cellWidth, state.CurrentY - rowHeight, PdfColor.FromRgb(200, 200, 200));
                state.CurrentPage!.AddLine(cellX, state.CurrentY, cellX, state.CurrentY - rowHeight, PdfColor.FromRgb(200, 200, 200));
                state.CurrentPage!.AddLine(cellX + cellWidth, state.CurrentY, cellX + cellWidth, state.CurrentY - rowHeight, PdfColor.FromRgb(200, 200, 200));

                // Render cell text
                var textY = state.CurrentY - cellPadding;
                foreach (var para in cell.Paragraphs)
                {
                    var fontSize = para.FontSize > 0 ? para.FontSize : options.FontSize;
                    var text = string.Concat(para.Runs.Select(r => r.Text));
                    if (string.IsNullOrEmpty(text)) continue;

                    var runFontSize = para.Runs.FirstOrDefault(r => !string.IsNullOrEmpty(r.Text))?.FontSize;
                    var effectiveFontSize = runFontSize > 0 ? runFontSize.Value : fontSize;
                    var runColor = para.Runs.FirstOrDefault(r => !string.IsNullOrEmpty(r.Text))?.Color ?? para.Color;
                    var lineHeight = effectiveFontSize * options.LineSpacing;
                    var avgCharWidth = effectiveFontSize * 0.47f;
                    var textWidth = cellWidth - cellPadding * 2;
                    var lines = WordWrap(text, textWidth, textWidth, avgCharWidth);

                    foreach (var line in lines)
                    {
                        textY -= effectiveFontSize;
                        if (textY < state.CurrentY - rowHeight + cellPadding) break; // clip
                        state.CurrentPage!.AddText(line, cellX + cellPadding, textY, effectiveFontSize, runColor);
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
        state.AdvanceY(8f);
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

    private static float CalculateRowHeight(DocxTableRow row, float[] colWidths, float cellPadding, ConversionOptions options)
    {
        var maxHeight = options.FontSize * options.LineSpacing + cellPadding * 2;

        for (var ci = 0; ci < row.Cells.Count && ci < colWidths.Length; ci++)
        {
            var cell = row.Cells[ci];
            var cellWidth = colWidths[ci];
            if (cell.GridSpan > 1)
            {
                for (var s = 1; s < cell.GridSpan && ci + s < colWidths.Length; s++)
                    cellWidth += colWidths[ci + s];
            }

            var cellHeight = cellPadding * 2;
            foreach (var para in cell.Paragraphs)
            {
                var fontSize = para.FontSize > 0 ? para.FontSize : options.FontSize;
                var runFontSize = para.Runs.FirstOrDefault(r => !string.IsNullOrEmpty(r.Text))?.FontSize;
                var effectiveFontSize = runFontSize > 0 ? runFontSize.Value : fontSize;
                var lineHeight = effectiveFontSize * options.LineSpacing;
                var avgCharWidth = effectiveFontSize * 0.47f;
                var textWidth = cellWidth - cellPadding * 2;
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

    private static List<string> WordWrap(string text, float firstLineWidth, float subsequentWidth, float avgCharWidth)
    {
        if (string.IsNullOrEmpty(text))
            return [""];

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
