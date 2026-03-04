using System.Globalization;

namespace MiniSoftware;

/// <summary>
/// Converts Excel (.xlsx) files to PDF documents.
/// Renders cell text in a simple table layout using the built-in Helvetica font.
/// </summary>
internal static class ExcelToPdfConverter
{
    /// <summary>
    /// Options for controlling Excel-to-PDF conversion.
    /// </summary>
    internal sealed class ConversionOptions
    {
        /// <summary>Font size in points (default: 10).</summary>
        public float FontSize { get; set; } = 10;

        /// <summary>Page left margin in points (default: 50).</summary>
        public float MarginLeft { get; set; } = 50;

        /// <summary>Page top margin in points (default: 72 = 1 inch).</summary>
        public float MarginTop { get; set; } = 72;

        /// <summary>Page right margin in points (default: 50).</summary>
        public float MarginRight { get; set; } = 50;

        /// <summary>Page bottom margin in points (default: 72 = 1 inch).</summary>
        public float MarginBottom { get; set; } = 72;

        /// <summary>Padding between columns in points (default: 4).</summary>
        public float ColumnPadding { get; set; } = 4;

        /// <summary>Line spacing multiplier (default: 1.5).</summary>
        public float LineSpacing { get; set; } = 1.5f;

        /// <summary>Page width in points (default: 612 = US Letter).</summary>
        public float PageWidth { get; set; } = 612;

        /// <summary>Page height in points (default: 792 = US Letter).</summary>
        public float PageHeight { get; set; } = 792;

        /// <summary>Whether to include sheet name as a header (default: false).</summary>
        public bool IncludeSheetName { get; set; } = false;
    }

    /// <summary>
    /// Converts an Excel file to a PDF document.
    /// </summary>
    /// <param name="excelPath">Path to the .xlsx file.</param>
    /// <param name="options">Optional conversion settings.</param>
    /// <returns>A PdfDocument containing the Excel data.</returns>
    internal static PdfDocument Convert(string excelPath, ConversionOptions? options = null)
    {
        using var stream = File.OpenRead(excelPath);
        return Convert(stream, options);
    }

    /// <summary>
    /// Converts an Excel stream to a PDF document.
    /// </summary>
    /// <param name="excelStream">Stream containing .xlsx data.</param>
    /// <param name="options">Optional conversion settings.</param>
    /// <returns>A PdfDocument containing the Excel data.</returns>
    internal static PdfDocument Convert(Stream excelStream, ConversionOptions? options = null)
    {
        options ??= new ConversionOptions();
        var sheets = ExcelReader.ReadSheets(excelStream);
        var doc = new PdfDocument();

        foreach (var sheet in sheets)
        {
            RenderSheet(doc, sheet, options);
        }

        // If no sheets found, create at least one empty page
        if (doc.Pages.Count == 0)
        {
            doc.AddPage(options.PageWidth, options.PageHeight);
        }

        return doc;
    }

    /// <summary>
    /// Converts an Excel file directly to a PDF file.
    /// </summary>
    /// <param name="excelPath">Path to the .xlsx file.</param>
    /// <param name="pdfPath">Path for the output .pdf file.</param>
    /// <param name="options">Optional conversion settings.</param>
    internal static void ConvertToFile(string excelPath, string pdfPath, ConversionOptions? options = null)
    {
        var doc = Convert(excelPath, options);
        doc.Save(pdfPath);
    }

    private static void RenderSheet(PdfDocument doc, ExcelSheet sheet, ConversionOptions options)
    {
        // Skip only if there's truly nothing to render (no rows AND no images).
        if (sheet.Rows.Count == 0 && sheet.Images.Count == 0 && sheet.Charts.Count == 0) return;

        var maxCols = sheet.Rows.Count > 0 ? sheet.Rows.Max(r => r.Count) : 0;

        // Extend column range to include any image anchor columns so images beyond
        // the data area (e.g. a chart placed in column E when data ends at C) are rendered.
        if (sheet.Images.Count > 0)
        {
            var maxImgColEnd = sheet.Images.Max(img => img.AnchorCol + Math.Max(1, img.SpanCols));
            maxCols = Math.Max(maxCols, maxImgColEnd);
        }

        // Extend column range for chart anchors too
        if (sheet.Charts.Count > 0)
        {
            var maxChartCol = sheet.Charts.Max(c => c.AnchorCol + 1);
            maxCols = Math.Max(maxCols, maxChartCol);
        }

        if (maxCols == 0)
        {
            // All rows are empty — still render an empty page worth of vertical space
            doc.AddPage(options.PageWidth, options.PageHeight);
            return;
        }

        var pageWidth = options.PageWidth;
        var pageHeight = options.PageHeight;
        var usableWidth = pageWidth - options.MarginLeft - options.MarginRight;
        var avgCharWidth = options.FontSize * 0.47f;

        // Determine column widths first to decide on layout strategy
        var columnPadding = options.ColumnPadding;
        if (maxCols > 6)
        {
            columnPadding = Math.Max(4f, options.ColumnPadding * 6f / maxCols);
        }

        // Calculate natural (unscaled) column widths to decide on grouping
        var naturalWidths = CalculateNaturalColumnWidths(sheet, maxCols, usableWidth, options);
        var totalNatural = naturalWidths.Sum() + columnPadding * (maxCols - 1);

        if (totalNatural > usableWidth && maxCols > 1)
        {
            // Columns don't fit — split into groups that fit on a page each
            RenderSheetColumnGroups(doc, sheet, options, pageWidth, pageHeight, usableWidth, columnPadding, avgCharWidth, naturalWidths);
        }
        else
        {
            // Single group — scale to fit if needed
            var colWidths = ScaleColumnWidths(naturalWidths, usableWidth, columnPadding, avgCharWidth);
            RenderSheetRows(doc, sheet, options, pageWidth, pageHeight, Enumerable.Range(0, maxCols).ToArray(), columnPadding, colWidths, avgCharWidth);
        }
    }

    /// <summary>
    /// Split columns into groups that fit within usable width, render each group on separate pages.
    /// </summary>
    private static void RenderSheetColumnGroups(PdfDocument doc, ExcelSheet sheet, ConversionOptions options,
        float pageWidth, float pageHeight, float usableWidth, float columnPadding, float avgCharWidth, float[] naturalWidths)
    {
        var maxCols = naturalWidths.Length;

        // Group columns to fit within usable width using pre-calculated natural widths
        var groups = new List<int[]>();
        var currentGroup = new List<int>();
        var currentWidth = 0f;

        for (var col = 0; col < maxCols; col++)
        {
            var colWithPadding = naturalWidths[col] + (currentGroup.Count > 0 ? columnPadding : 0);
            if (currentGroup.Count > 0 && currentWidth + colWithPadding > usableWidth)
            {
                // Start new group
                groups.Add(currentGroup.ToArray());
                currentGroup = new List<int> { col };
                currentWidth = naturalWidths[col];
            }
            else
            {
                currentGroup.Add(col);
                currentWidth += colWithPadding;
            }
        }
        if (currentGroup.Count > 0) groups.Add(currentGroup.ToArray());

        // Render each column group
        foreach (var group in groups)
        {
            // Extract column widths for this group
            var groupWidths = new float[group.Length];
            for (var i = 0; i < group.Length; i++)
            {
                groupWidths[i] = naturalWidths[group[i]];
            }

            // Scale to fit if needed
            var groupTotalWidth = groupWidths.Sum() + columnPadding * (group.Length - 1);
            if (groupTotalWidth > usableWidth)
            {
                var available = usableWidth - columnPadding * (group.Length - 1);
                var scale = available / groupWidths.Sum();
                for (var i = 0; i < groupWidths.Length; i++)
                {
                    groupWidths[i] = Math.Max(groupWidths[i] * scale, avgCharWidth);
                }
            }

            RenderSheetRows(doc, sheet, options, pageWidth, pageHeight, group, columnPadding, groupWidths, avgCharWidth);
        }
    }

    /// <summary>
    /// Render rows for a specific set of columns.
    /// </summary>
    private static void RenderSheetRows(PdfDocument doc, ExcelSheet sheet, ConversionOptions options,
        float pageWidth, float pageHeight, int[] columns, float columnPadding, float[] colWidths, float avgCharWidth)
    {
        var lineHeight = options.FontSize * options.LineSpacing;
        PdfPage? currentPage = null;
        var currentY = 0f;

        // Track cumulative X start position for each column (for image placement)
        var colXStarts = new float[columns.Length];
        {
            var x = options.MarginLeft;
            for (var i = 0; i < columns.Length; i++)
            {
                colXStarts[i] = x;
                x += colWidths[i] + columnPadding;
            }
        }

        // Map Excel row index → Y (bottom of that row's text block), for image placement.
        // We record the TOP of each row (currentY just before rendering it).
        var rowTopY = new Dictionary<int, float>(); // excelRowIndex → page Y at top of row
        var rowPage = new Dictionary<int, PdfPage>();
        var excelRowIndex = 0;

        void EnsurePage()
        {
            if (currentPage == null || currentY < options.MarginBottom)
            {
                currentPage = doc.AddPage(pageWidth, pageHeight);
                currentY = pageHeight - options.MarginTop;
            }
        }

        // Sheet header (only for first column group, skip generic names like Sheet1)
        if (columns[0] == 0 && options.IncludeSheetName && !string.IsNullOrEmpty(sheet.Name) && !IsDefaultSheetName(sheet.Name))
        {
            EnsurePage();
            currentPage!.AddText(sheet.Name, options.MarginLeft, currentY, options.FontSize + 4);
            currentY -= lineHeight * 1.5f;
        }

        // Build a merge lookup: for each (row, col) that is the start of a merge,
        // store the end column. Used to calculate effective text width for merged cells.
        var mergeEndCol = new Dictionary<(int, int), int>(); // (row, col) → endCol
        foreach (var (sr, sc, er, ec) in sheet.MergedCells)
        {
            for (var r = sr; r <= er; r++)
                mergeEndCol[(r, sc)] = ec;
        }

        // Render rows
        foreach (var row in sheet.Rows)
        {
            EnsurePage();

            // Record top-of-row state for image placement
            rowTopY[excelRowIndex] = currentY;
            rowPage[excelRowIndex] = currentPage!;

            if (row.Count == 0)
            {
                currentY -= lineHeight;
                excelRowIndex++;
                continue;
            }

            // Calculate wrapped lines for each column in this group
            var maxLinesInRow = 1;
            var virtualRowExtraLines = 0; // extra lines from virtual wrapping (text overflows page width)
            var cellLines = new string[columns.Length][];

            for (var i = 0; i < columns.Length; i++)
            {
                var col = columns[i];
                if (col < row.Count)
                {
                    var cellText = row[col].Text;

                    // Match LibreOffice General format: reformat numbers that don't fit column width.
                    if (!string.IsNullOrEmpty(cellText) && !cellText.Contains('\n'))
                        cellText = FitNumericText(cellText, colWidths[i], options.FontSize);

                    if (!string.IsNullOrEmpty(cellText))
                    {
                        // Handle explicit newlines in cell text (e.g., Alt+Enter in Excel).
                        // Otherwise write full text as a single line.
                        if (cellText.Contains('\n'))
                        {
                            cellLines[i] = cellText.Split('\n');
                        }
                        else
                        {
                            // Calculate effective width: if this cell starts a merge, use the merged span width.
                            var effectiveWidth = colWidths[i];
                            var isMerged = mergeEndCol.TryGetValue((excelRowIndex, col), out var endCol);
                            if (isMerged)
                            {
                                // Sum widths of merged columns — no extra padding within merged span
                                for (var mc = i + 1; mc < columns.Length && columns[mc] <= endCol; mc++)
                                    effectiveWidth += colWidths[mc];
                            }

                            // Excel/LibreOffice clip text at the column boundary when the
                            // next cell to the right contains content.  For the last column
                            // in the group (or when the next cell is empty) the text overflows.
                            // Merged cells are always clipped at the merge boundary.
                            var fitChars = FittingChars(cellText, effectiveWidth, options.FontSize);
                            var isLastCol = (i == columns.Length - 1);

                            // Find next non-merged column with content
                            var nextContentCol = -1;
                            var checkStart = isMerged ? endCol + 1 : col + 1;
                            for (var mi = i + 1; mi < columns.Length; mi++)
                            {
                                if (columns[mi] >= checkStart)
                                {
                                    var nc = columns[mi];
                                    if (nc < row.Count && !string.IsNullOrEmpty(row[nc].Text))
                                    {
                                        nextContentCol = mi;
                                    }
                                    break;
                                }
                            }
                            var nextCellHasContent = nextContentCol >= 0;

                            var shouldClip = isMerged || (!isLastCol && nextCellHasContent);
                            if (shouldClip && cellText.Length > fitChars)
                            {
                                // Truncate to effective column width (matches LibreOffice clip)
                                cellLines[i] = new[] { cellText[..fitChars] };
                            }
                            else if (!shouldClip && fitChars < cellText.Length && columns.Length == 1)
                            {
                                // Single-column overflow: clip text at page right edge.
                                // LibreOffice calculates row height from text wrapping at the default
                                // column width, but renders a single line clipped at the page boundary.
                                var pageClipChars = FittingChars(cellText, pageWidth - options.MarginLeft, options.FontSize);
                                var clippedText = pageClipChars < cellText.Length ? cellText[..pageClipChars] : cellText;
                                cellLines[i] = new[] { clippedText };

                                // Calculate virtual row height from wrapping at default column width
                                var defaultColPts = ExcelSheet.CharUnitsToPoints(8.43f);
                                var wrapChars = FittingChars(cellText, defaultColPts, options.FontSize);
                                if (wrapChars > 0)
                                {
                                    var virtualLines = (int)Math.Ceiling((double)cellText.Length / wrapChars);
                                    virtualRowExtraLines = Math.Max(virtualRowExtraLines, virtualLines - 1);
                                }
                            }
                            else
                            {
                                cellLines[i] = new[] { cellText };
                            }
                        }

                        maxLinesInRow = Math.Max(maxLinesInRow, cellLines[i].Length);
                    }
                    else
                    {
                        cellLines[i] = Array.Empty<string>();
                    }
                }
                else
                {
                    cellLines[i] = Array.Empty<string>();
                }
            }

            // Check space for wrapped lines
            var rowHeight = lineHeight * maxLinesInRow;
            var usablePageHeight = pageHeight - options.MarginTop - options.MarginBottom;

            if (currentY - rowHeight < options.MarginBottom && currentPage != null)
            {
                currentPage = doc.AddPage(pageWidth, pageHeight);
                currentY = pageHeight - options.MarginTop;
                // Update the row's top position on the new page
                rowTopY[excelRowIndex] = currentY;
                rowPage[excelRowIndex] = currentPage;
            }

            // Render cells — split across pages if the row is taller than a single page
            if (rowHeight > usablePageHeight)
            {
                // Multi-page row: render lines in batches that fit on each page
                var linesRendered = 0;
                while (linesRendered < maxLinesInRow)
                {
                    var linesAvailable = Math.Max(1, (int)((currentY - options.MarginBottom) / lineHeight));
                    var linesToRender = Math.Min(linesAvailable, maxLinesInRow - linesRendered);

                    var x = options.MarginLeft;
                    for (var i = 0; i < columns.Length; i++)
                    {
                        var lines = cellLines[i];
                        var col = columns[i];
                        var color = col < row.Count ? row[col].Color : null;
                        var cellY = currentY;

                        for (var lineIdx = linesRendered; lineIdx < linesRendered + linesToRender && lineIdx < lines.Length; lineIdx++)
                        {
                            if (!string.IsNullOrEmpty(lines[lineIdx]))
                                currentPage!.AddText(lines[lineIdx], x, cellY, options.FontSize, color);
                            cellY -= lineHeight;
                        }
                        x += colWidths[i] + columnPadding;
                    }

                    linesRendered += linesToRender;
                    currentY -= linesToRender * lineHeight;

                    if (linesRendered < maxLinesInRow)
                    {
                        currentPage = doc.AddPage(pageWidth, pageHeight);
                        currentY = pageHeight - options.MarginTop;
                    }
                }
            }
            else
            {
            // Render cells (normal path — row fits on one page)
            var x = options.MarginLeft;
            for (var i = 0; i < columns.Length; i++)
            {
                var lines = cellLines[i];
                var col = columns[i];
                var color = col < row.Count ? row[col].Color : null;
                var fillColor = col < row.Count ? row[col].FillColor : null;
                var cellY = currentY;

                // Draw fill rectangle behind cell if fill color is set
                if (fillColor != null)
                {
                    var fillHeight = lineHeight * maxLinesInRow;
                    currentPage!.AddRectangle(x, currentY - fillHeight, colWidths[i] + columnPadding, fillHeight, fillColor);
                }

                for (var lineIdx = 0; lineIdx < lines.Length; lineIdx++)
                {
                    if (!string.IsNullOrEmpty(lines[lineIdx]))
                    {
                        currentPage!.AddText(lines[lineIdx], x, cellY, options.FontSize, color);
                    }
                    cellY -= lineHeight;
                }

                x += colWidths[i] + columnPadding;
            }

            currentY -= rowHeight;
            }

            // If cells had virtual overflow height (single-column, text wider than page),
            // advance currentY by the extra lines to match LibreOffice's row height calculation.
            if (virtualRowExtraLines > 0)
            {
                var extraHeight = lineHeight * virtualRowExtraLines;
                while (extraHeight > 0)
                {
                    var spaceLeft = currentY - options.MarginBottom;
                    if (spaceLeft <= 0)
                    {
                        currentPage = doc.AddPage(pageWidth, pageHeight);
                        currentY = pageHeight - options.MarginTop;
                        spaceLeft = currentY - options.MarginBottom;
                    }
                    var consume = Math.Min(extraHeight, spaceLeft);
                    currentY -= consume;
                    extraHeight -= consume;
                    if (extraHeight > 0)
                    {
                        currentPage = doc.AddPage(pageWidth, pageHeight);
                        currentY = pageHeight - options.MarginTop;
                    }
                }
            }
            excelRowIndex++;
        }

        // Place embedded images and chart placeholders
        if (sheet.Images.Count == 0 && sheet.Charts.Count == 0) return;

        // For image/chart-only sheets (no data rows) ensure at least one page exists.
        EnsurePage();

        var usableWidth = pageWidth - options.MarginLeft - options.MarginRight;

        foreach (var img in sheet.Images)
        {
            // Only render image if its anchor column is within the current column group
            var colGroupStart = columns[0];
            var colGroupEnd = columns[^1];
            if (img.AnchorCol < colGroupStart || img.AnchorCol > colGroupEnd) continue;

            // Resolve anchor row position. Falls back to estimated Y for image-only sheets
            // where no data rows populated rowTopY.
            if (!rowTopY.TryGetValue(img.AnchorRow, out var imgTopY))
            {
                // Estimate: start at top-margin and step down by lineHeight per row.
                imgTopY = (pageHeight - options.MarginTop) - img.AnchorRow * lineHeight;
                if (imgTopY < options.MarginBottom) imgTopY = pageHeight - options.MarginTop;
            }
            if (!rowPage.TryGetValue(img.AnchorRow, out var imgPage))
            {
                imgPage = currentPage!;
            }

            // Calculate X: find position of anchor column within group
            var colGroupIdx = Array.IndexOf(columns, img.AnchorCol);
            if (colGroupIdx < 0)
            {
                // Anchor col not directly in group — start at margin
                colGroupIdx = 0;
            }
            var imgX = colXStarts[colGroupIdx];

            // Calculate render size.
            // Prefer explicit EMU dimensions (from <ext cx cy> in oneCellAnchor).
            // Fallback: derive from spanCols × column widths and spanRows × lineHeight.
            float imgRenderWidth, imgRenderHeight;
            const float EmuToPt = 1f / 12700f;
            if (img.WidthEmu > 0 && img.HeightEmu > 0)
            {
                imgRenderWidth  = Math.Min(img.WidthEmu  * EmuToPt, usableWidth * 0.95f);
                imgRenderHeight = Math.Min(img.HeightEmu * EmuToPt, pageHeight  * 0.75f);
            }
            else
            {
                imgRenderWidth = 0f;
                for (var ci = colGroupIdx; ci < Math.Min(colGroupIdx + img.SpanCols, columns.Length); ci++)
                    imgRenderWidth += colWidths[ci] + (ci > colGroupIdx ? columnPadding : 0);
                imgRenderWidth  = Math.Min(Math.Max(imgRenderWidth, 36f), usableWidth * 0.8f);
                imgRenderHeight = Math.Max(lineHeight * img.SpanRows, imgRenderWidth * 0.75f);
                imgRenderHeight = Math.Min(imgRenderHeight, pageHeight * 0.5f);
            }

            // In PDF coordinates: Y is bottom of image; top = imgTopY, bottom = top - height
            var imgY = imgTopY - imgRenderHeight;
            if (imgY < options.MarginBottom)
                imgY = options.MarginBottom;

            var format = img.Extension is "jpg" or "jpeg" ? "jpg" : "png";
            imgPage.AddImage(img.Data, format, imgX, imgY, imgRenderWidth, imgRenderHeight);
        }

        // Render charts as actual visual elements
        if (sheet.Charts.Count == 0) return;

        EnsurePage();

        // Track whether any chart is anchored to the right of data (not below)
        // to determine if we need an overflow page (matching LibreOffice behavior)
        var maxDataCols = sheet.Rows.Count > 0 ? sheet.Rows.Max(r => r.Count) : 0;
        var needsOverflowPage = false;

        foreach (var chart in sheet.Charts)
        {
            // Only render chart if its anchor column is within the current column group
            var colGroupStart = columns[0];
            var colGroupEnd = columns[^1];
            if (chart.AnchorCol < colGroupStart || chart.AnchorCol > colGroupEnd) continue;

            // Determine anchor Y position
            if (!rowTopY.TryGetValue(chart.AnchorRow, out var chartTopY))
            {
                chartTopY = (pageHeight - options.MarginTop) - chart.AnchorRow * lineHeight;
                if (chartTopY < options.MarginBottom) chartTopY = pageHeight - options.MarginTop;
            }
            if (!rowPage.TryGetValue(chart.AnchorRow, out var chartPage))
            {
                chartPage = currentPage!;
            }

            var chartColIdx = Array.IndexOf(columns, chart.AnchorCol);
            if (chartColIdx < 0) chartColIdx = 0;
            var chartX = colXStarts[chartColIdx];

            // Calculate chart render size from EMU (allow natural size for page overflow)
            const float EmuToPt2 = 1f / 12700f;
            var chartWidth = Math.Min(chart.WidthEmu * EmuToPt2, usableWidth * 0.95f);
            var chartHeight = chart.HeightEmu * EmuToPt2;
            if (chartWidth < 72) chartWidth = usableWidth * 0.6f;
            if (chartHeight < 72) chartHeight = chartWidth * 0.65f;
            // Cap height to avoid absurdly tall charts but allow page overflow
            chartHeight = Math.Min(chartHeight, pageHeight * 0.85f);

            // Ensure chart fits on page, start new page if needed
            var chartTop = chartTopY;
            if (chartTop - chartHeight < options.MarginBottom)
            {
                chartPage = doc.AddPage(pageWidth, pageHeight);
                chartTop = pageHeight - options.MarginTop;
            }

            RenderChart(chartPage, chart, chartX, chartTop, chartWidth, chartHeight, options.FontSize);

            // Charts anchored to the right of data columns cause LibreOffice to
            // produce an overflow page (the chart extends beyond the print area).
            if (chart.AnchorCol >= maxDataCols && maxDataCols > 0)
            {
                needsOverflowPage = true;
            }
        }

        // Add overflow page to match LibreOffice page count for right-anchored charts
        if (needsOverflowPage)
        {
            doc.AddPage(pageWidth, pageHeight);
        }
    }

    /// <summary>Standard chart color palette (matches common spreadsheet defaults).</summary>
    private static readonly PdfColor[] ChartColors = new[]
    {
        new PdfColor(0.310f, 0.506f, 0.741f), // blue   (#4F81BD)
        new PdfColor(0.753f, 0.314f, 0.302f), // red    (#C0504D)
        new PdfColor(0.608f, 0.733f, 0.349f), // green  (#9BBB59)
        new PdfColor(0.502f, 0.392f, 0.635f), // purple (#8064A2)
        new PdfColor(0.294f, 0.675f, 0.776f), // cyan   (#4BACC6)
        new PdfColor(0.969f, 0.588f, 0.275f), // orange (#F79646)
        new PdfColor(0.173f, 0.302f, 0.459f), // dark blue
        new PdfColor(0.467f, 0.173f, 0.165f), // dark red
    };

    /// <summary>
    /// Renders a chart (bar, line, pie, etc.) onto a PDF page.
    /// </summary>
    private static void RenderChart(PdfPage page, ExcelChartInfo chart,
        float x, float top, float width, float height, float baseFontSize)
    {
        var titleFontSize = baseFontSize + 2;
        var labelFontSize = baseFontSize - 1;
        var axisFontSize = baseFontSize - 2;
        var padding = 8f;

        // Draw chart title (clipped to chart width)
        var titleY = top;
        if (!string.IsNullOrEmpty(chart.Title))
        {
            var titleAvailWidth = width * 0.65f;  // title starts at 30%, leave ~5% right margin
            var titleChars = FittingChars(chart.Title, titleAvailWidth, titleFontSize);
            var clippedTitle = titleChars >= chart.Title.Length ? chart.Title : chart.Title[..titleChars];
            page.AddText(clippedTitle, x + width * 0.3f, titleY - titleFontSize, titleFontSize);
            titleY -= titleFontSize * 2.2f;
        }

        // Plot area bounds
        var plotLeft = x + padding + 40f;  // leave room for Y-axis labels
        var plotRight = x + width - padding - 10f;
        var plotTop = titleY - padding;
        var plotBottom = top - height + padding + 30f; // leave room for X-axis labels
        var plotWidth = plotRight - plotLeft;
        var plotHeight = plotTop - plotBottom;

        if (plotWidth < 20 || plotHeight < 20) return;

        // Route to specific chart type renderer
        var type = chart.ChartType.ToLowerInvariant();
        if (type.Contains("pie") || type.Contains("doughnut"))
        {
            RenderPieChart(page, chart, x, top, width, height, plotLeft, plotBottom, plotWidth, plotHeight, labelFontSize, type.Contains("doughnut"), chart.ShowDataLabelPercent);
        }
        else if (type.Contains("scatter") || type.Contains("bubble"))
        {
            RenderScatterChart(page, chart, plotLeft, plotBottom, plotWidth, plotHeight, labelFontSize, axisFontSize, chart.ValueAxisFormatCode);
        }
        else if (type.Contains("radar"))
        {
            RenderLineChart(page, chart, plotLeft, plotBottom, plotWidth, plotHeight, labelFontSize, axisFontSize, skipAxisLabels: true, axisFmtCode: chart.ValueAxisFormatCode);
            // Add spoke labels around the radar center
            if (chart.Series.Count > 0)
            {
                var categories = chart.Series[0].Categories;
                var centerX = plotLeft + plotWidth * 0.5f;
                var centerY = plotBottom + plotHeight * 0.5f;
                var labelRadius = Math.Min(plotWidth, plotHeight) * 0.5f + 8f;
                for (var ci = 0; ci < categories.Length; ci++)
                {
                    var angle = Math.PI / 2 - 2 * Math.PI * ci / categories.Length;
                    var lx = centerX + (float)(Math.Cos(angle) * labelRadius);
                    var ly = centerY + (float)(Math.Sin(angle) * labelRadius);
                    page.AddText(TruncateLabel(categories[ci], 15), lx - axisFontSize * 2, ly - axisFontSize * 0.3f, axisFontSize);
                }
                // Add concentric value labels along the top spoke
                var allVals = chart.Series.SelectMany(s => s.Values).ToArray();
                if (allVals.Length > 0)
                {
                    var (niceMin, niceMax, niceStep) = NiceAxisScale(0, allVals.Max());
                    for (var tickVal = niceMin; tickVal <= niceMax + niceStep * 0.01; tickVal += niceStep)
                    {
                        var frac = (float)((tickVal - niceMin) / (niceMax - niceMin));
                        var tickY = centerY + frac * plotHeight * 0.4f;
                        var label = FormatAxisValue(tickVal, chart.ValueAxisFormatCode);
                        page.AddText(label, centerX - axisFontSize, tickY, axisFontSize);
                    }
                }
            }
        }
        else if (type.Contains("line"))
        {
            RenderLineChart(page, chart, plotLeft, plotBottom, plotWidth, plotHeight, labelFontSize, axisFontSize, skipAxisLabels: false, axisFmtCode: chart.ValueAxisFormatCode);
        }
        else if (type.Contains("area"))
        {
            RenderAreaChart(page, chart, plotLeft, plotBottom, plotWidth, plotHeight, labelFontSize, axisFontSize, chart.ValueAxisFormatCode);
        }
        else if (type.Contains("horizontal"))
        {
            RenderHorizontalBarChart(page, chart, plotLeft, plotBottom, plotWidth, plotHeight, labelFontSize, axisFontSize, chart.ValueAxisFormatCode);
        }
        else
        {
            // Default: bar/column/bubble → bar chart
            RenderBarChart(page, chart, plotLeft, plotBottom, plotWidth, plotHeight, labelFontSize, axisFontSize, chart.ValueAxisFormatCode);
        }

        // Y-axis title (rotated text not supported, place vertically aligned)
        if (!string.IsNullOrEmpty(chart.ValueAxisTitle))
        {
            page.AddText(chart.ValueAxisTitle, x + 2, plotBottom + plotHeight * 0.4f, axisFontSize);
        }
        // X-axis title
        if (!string.IsNullOrEmpty(chart.CategoryAxisTitle))
        {
            page.AddText(chart.CategoryAxisTitle, plotLeft + plotWidth * 0.35f, plotBottom - 22f, axisFontSize);
        }
    }

    /// <summary>Renders a bar/column chart.</summary>
    private static void RenderBarChart(PdfPage page, ExcelChartInfo chart,
        float plotLeft, float plotBottom, float plotWidth, float plotHeight,
        float labelFontSize, float axisFontSize, string axisFmtCode = "")
    {
        var series = chart.Series;
        if (series.Count == 0) return;

        var isStacked = chart.ChartType.Contains("stacked", StringComparison.OrdinalIgnoreCase);
        var isPercentStacked = chart.ChartType.Contains("percentStacked", StringComparison.OrdinalIgnoreCase);

        var categories = series[0].Categories;
        var numCats = Math.Max(categories.Length, series.Max(s => s.Values.Length));
        if (numCats == 0) return;

        // Get all values to determine scale
        double dataMax, dataMin;
        if (isPercentStacked)
        {
            dataMin = 0;
            dataMax = 100;
        }
        else if (isStacked)
        {
            // For stacked charts: axis max = max cumulative sum across categories
            dataMax = 0;
            dataMin = 0;
            for (var ci = 0; ci < numCats; ci++)
            {
                double posSum = 0, negSum = 0;
                for (var si = 0; si < series.Count; si++)
                {
                    var val = ci < series[si].Values.Length ? series[si].Values[ci] : 0;
                    if (val >= 0) posSum += val; else negSum += val;
                }
                dataMax = Math.Max(dataMax, posSum);
                dataMin = Math.Min(dataMin, negSum);
            }
        }
        else
        {
            var allValues = series.SelectMany(s => s.Values).ToArray();
            if (allValues.Length == 0) return;
            dataMax = allValues.Max();
            dataMin = Math.Min(0, allValues.Min());
        }

        // Use nice axis scaling for round number labels
        var (niceMin, niceMax, niceStep) = isPercentStacked
            ? (0.0, 100.0, 10.0)
            : NiceAxisScale(dataMin, dataMax);
        var range = niceMax - niceMin;
        if (range <= 0) range = 1;

        var numSeries = series.Count;
        var groupWidth = plotWidth / numCats;
        var barWidth = isStacked ? groupWidth * 0.7f : groupWidth * 0.7f / numSeries;
        var groupPadding = groupWidth * 0.15f;

        // Y-axis baseline (where value=0 sits)
        var baselineY = plotBottom + (float)((0 - niceMin) / range) * plotHeight;

        // Draw Y-axis gridlines and labels at nice round numbers
        for (var tickVal = niceMin; tickVal <= niceMax + niceStep * 0.01; tickVal += niceStep)
        {
            var gridY = plotBottom + (float)((tickVal - niceMin) / range) * plotHeight;
            page.AddLine(plotLeft, gridY, plotLeft + plotWidth, gridY,
                new PdfColor(0.85f, 0.85f, 0.85f), 0.5f);
            var label = FormatAxisValue(tickVal, axisFmtCode);
            if (isPercentStacked) label += "%";
            page.AddText(label, plotLeft - label.Length * axisFontSize * 0.5f - 4f, gridY - axisFontSize * 0.3f, axisFontSize);
        }

        // Draw bars
        for (var ci = 0; ci < numCats; ci++)
        {
            var groupX = plotLeft + ci * groupWidth + groupPadding;

            if (isStacked)
            {
                // Stacked: accumulate values at same X position
                double cumPos = 0, cumNeg = 0;

                // Compute totals for percent stacked
                double catTotal = 0;
                if (isPercentStacked)
                {
                    for (var si = 0; si < numSeries; si++)
                        catTotal += Math.Abs(ci < series[si].Values.Length ? series[si].Values[ci] : 0);
                    if (catTotal == 0) catTotal = 1;
                }

                for (var si = 0; si < numSeries; si++)
                {
                    var rawVal = si < series.Count && ci < series[si].Values.Length
                        ? series[si].Values[ci] : 0;
                    var val = isPercentStacked ? (rawVal / catTotal * 100) : rawVal;
                    var barX = groupX;

                    double barBase, barTop;
                    if (val >= 0)
                    {
                        barBase = cumPos;
                        cumPos += val;
                        barTop = cumPos;
                    }
                    else
                    {
                        barTop = cumNeg;
                        cumNeg += val;
                        barBase = cumNeg;
                    }

                    var y0 = plotBottom + (float)((barBase - niceMin) / range) * plotHeight;
                    var y1 = plotBottom + (float)((barTop - niceMin) / range) * plotHeight;
                    var barDrawH = Math.Max(0.5f, Math.Abs(y1 - y0));
                    var color = ChartColors[si % ChartColors.Length];
                    page.AddRectangle(barX, Math.Min(y0, y1), barWidth, barDrawH, color);
                }
            }
            else
            {
                // Clustered: bars side by side
                for (var si = 0; si < numSeries; si++)
                {
                    var val = si < series.Count && ci < series[si].Values.Length
                        ? series[si].Values[ci] : 0;
                    var barX = groupX + si * barWidth;
                    var valY = plotBottom + (float)((val - niceMin) / range) * plotHeight;
                    var barBottom = Math.Min(valY, baselineY);
                    var barDrawH = Math.Abs(valY - baselineY);
                    if (barDrawH < 0.5f) barDrawH = 0.5f;

                    var color = ChartColors[si % ChartColors.Length];
                    page.AddRectangle(barX, barBottom, barWidth, barDrawH, color);
                }
            }

            // Category label
            if (ci < categories.Length)
            {
                var label = TruncateLabel(categories[ci], (int)(groupWidth / (axisFontSize * 0.4f)));
                var labelX = plotLeft + ci * groupWidth + groupWidth * 0.1f;
                page.AddText(label, labelX, plotBottom - axisFontSize * 1.5f, axisFontSize);
            }
        }

        // Draw axes
        page.AddLine(plotLeft, plotBottom, plotLeft, plotBottom + plotHeight,
            new PdfColor(0, 0, 0), 0.8f);
        page.AddLine(plotLeft, baselineY, plotLeft + plotWidth, baselineY,
            new PdfColor(0, 0, 0), 0.8f);

        // Legend (reversed for stacked charts to match Excel bottom-to-top stacking order)
        RenderLegend(page, series, plotLeft + plotWidth * 0.05f, plotBottom + plotHeight + 5f, axisFontSize, isStacked);
    }

    /// <summary>Renders a horizontal bar chart (categories on Y-axis, values on X-axis).</summary>
    private static void RenderHorizontalBarChart(PdfPage page, ExcelChartInfo chart,
        float plotLeft, float plotBottom, float plotWidth, float plotHeight,
        float labelFontSize, float axisFontSize, string axisFmtCode = "")
    {
        var series = chart.Series;
        if (series.Count == 0) return;

        var allValues = series.SelectMany(s => s.Values).ToArray();
        if (allValues.Length == 0) return;

        var dataMax = allValues.Max();
        var dataMin = Math.Min(0, allValues.Min());

        var (niceMin, niceMax, niceStep) = NiceAxisScale(dataMin, dataMax);
        var range = niceMax - niceMin;
        if (range <= 0) range = 1;

        var categories = series[0].Categories;
        var numCats = Math.Max(categories.Length, series.Max(s => s.Values.Length));
        if (numCats == 0) return;

        var numSeries = series.Count;
        var groupHeight = plotHeight / numCats;
        var barHeight = groupHeight * 0.7f / numSeries;
        var groupPadding = groupHeight * 0.15f;

        var baselineX = plotLeft + (float)((0 - niceMin) / range) * plotWidth;

        // X-axis gridlines and labels at nice round numbers
        for (var tickVal = niceMin; tickVal <= niceMax + niceStep * 0.01; tickVal += niceStep)
        {
            var gridX = plotLeft + (float)((tickVal - niceMin) / range) * plotWidth;
            page.AddLine(gridX, plotBottom, gridX, plotBottom + plotHeight,
                new PdfColor(0.85f, 0.85f, 0.85f), 0.5f);
            var label = FormatAxisValue(tickVal, axisFmtCode);
            page.AddText(label, gridX - label.Length * axisFontSize * 0.25f, plotBottom - axisFontSize * 1.5f, axisFontSize);
        }

        // Draw horizontal bars (categories from bottom to top, matching spreadsheet convention)
        for (var ci = 0; ci < numCats; ci++)
        {
            var groupY = plotBottom + ci * groupHeight + groupPadding;
            for (var si = 0; si < numSeries; si++)
            {
                var val = si < series.Count && ci < series[si].Values.Length
                    ? series[si].Values[ci] : 0;
                var barY = groupY + si * barHeight;
                var valX = plotLeft + (float)((val - niceMin) / range) * plotWidth;
                var barLeft = Math.Min(valX, baselineX);
                var barDrawW = Math.Abs(valX - baselineX);
                if (barDrawW < 0.5f) barDrawW = 0.5f;

                var color = ChartColors[si % ChartColors.Length];
                page.AddRectangle(barLeft, barY, barDrawW, barHeight, color);
            }

            // Category label (on Y-axis, left side)
            if (ci < categories.Length)
            {
                var label = TruncateLabel(categories[ci], 12);
                var labelY = plotBottom + (ci + 0.5f) * groupHeight - axisFontSize * 0.3f;
                page.AddText(label, plotLeft - label.Length * axisFontSize * 0.5f - 4f, labelY, axisFontSize);
            }
        }

        // Draw axes
        page.AddLine(plotLeft, plotBottom, plotLeft, plotBottom + plotHeight,
            new PdfColor(0, 0, 0), 0.8f);
        page.AddLine(plotLeft, plotBottom, plotLeft + plotWidth, plotBottom,
            new PdfColor(0, 0, 0), 0.8f);

        var hIsStacked = chart.ChartType.Contains("stacked", StringComparison.OrdinalIgnoreCase);
        RenderLegend(page, series, plotLeft + plotWidth * 0.05f, plotBottom + plotHeight + 5f, axisFontSize, hIsStacked);
    }

    /// <summary>Renders a line chart.</summary>
    private static void RenderLineChart(PdfPage page, ExcelChartInfo chart,
        float plotLeft, float plotBottom, float plotWidth, float plotHeight,
        float labelFontSize, float axisFontSize, bool skipAxisLabels = false, string axisFmtCode = "")
    {
        var series = chart.Series;
        if (series.Count == 0) return;

        var allValues = series.SelectMany(s => s.Values).ToArray();
        if (allValues.Length == 0) return;

        var dataMax = allValues.Max();
        var dataMin = Math.Min(0, allValues.Min());

        // Use nice axis scaling for round number labels
        var (niceMin, niceMax, niceStep) = NiceAxisScale(dataMin, dataMax);
        var range = niceMax - niceMin;
        if (range <= 0) range = 1;

        var categories = series[0].Categories;
        var numPoints = Math.Max(categories.Length, series.Max(s => s.Values.Length));
        if (numPoints == 0) return;

        // Y-axis gridlines and labels at nice round numbers
        for (var tickVal = niceMin; tickVal <= niceMax + niceStep * 0.01; tickVal += niceStep)
        {
            var gridY = plotBottom + (float)((tickVal - niceMin) / range) * plotHeight;
            page.AddLine(plotLeft, gridY, plotLeft + plotWidth, gridY,
                new PdfColor(0.85f, 0.85f, 0.85f), 0.5f);
            if (!skipAxisLabels)
            {
                var label = FormatAxisValue(tickVal, axisFmtCode);
                page.AddText(label, plotLeft - label.Length * axisFontSize * 0.5f - 4f, gridY - axisFontSize * 0.3f, axisFontSize);
            }
        }

        // Draw lines for each series
        for (var si = 0; si < series.Count; si++)
        {
            var s = series[si];
            var color = ChartColors[si % ChartColors.Length];
            for (var pi = 1; pi < s.Values.Length; pi++)
            {
                var x1 = plotLeft + (pi - 1) * plotWidth / Math.Max(1, numPoints - 1);
                var y1 = plotBottom + (float)((s.Values[pi - 1] - niceMin) / range) * plotHeight;
                var x2 = plotLeft + pi * plotWidth / Math.Max(1, numPoints - 1);
                var y2 = plotBottom + (float)((s.Values[pi] - niceMin) / range) * plotHeight;
                page.AddLine(x1, y1, x2, y2, color, 1.5f);
            }
            // Draw data point markers (small rectangles)
            for (var pi = 0; pi < s.Values.Length; pi++)
            {
                var px = plotLeft + pi * plotWidth / Math.Max(1, numPoints - 1);
                var py = plotBottom + (float)((s.Values[pi] - niceMin) / range) * plotHeight;
                page.AddRectangle(px - 2, py - 2, 4, 4, color);
            }
        }

        // Category labels (skip for radar charts — they use spoke labels instead)
        if (!skipAxisLabels)
        {
            for (var ci = 0; ci < categories.Length; ci++)
            {
                var xPos = plotLeft + ci * plotWidth / Math.Max(1, numPoints - 1);
                var label = TruncateLabel(categories[ci], 15);
                page.AddText(label, xPos - axisFontSize, plotBottom - axisFontSize * 1.5f, axisFontSize);
            }
        }

        // Axes
        page.AddLine(plotLeft, plotBottom, plotLeft, plotBottom + plotHeight,
            new PdfColor(0, 0, 0), 0.8f);
        page.AddLine(plotLeft, plotBottom, plotLeft + plotWidth, plotBottom,
            new PdfColor(0, 0, 0), 0.8f);

        RenderLegend(page, series, plotLeft + plotWidth * 0.05f, plotBottom + plotHeight + 5f, axisFontSize);
    }

    /// <summary>Renders a scatter (XY) chart or bubble chart with numeric X and Y axes.</summary>
    private static void RenderScatterChart(PdfPage page, ExcelChartInfo chart,
        float plotLeft, float plotBottom, float plotWidth, float plotHeight,
        float labelFontSize, float axisFontSize, string axisFmtCode = "")
    {
        var series = chart.Series;
        if (series.Count == 0) return;

        // Parse X values from categories (stored as string from xVal element)
        var allXValues = new List<double>();
        var allYValues = new List<double>();
        var seriesData = new List<(double[] xs, double[] ys)>();

        foreach (var s in series)
        {
            var xs = s.Categories.Select(c =>
                double.TryParse(c, System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out var d) ? d : 0.0)
                .ToArray();
            var ys = s.Values;
            seriesData.Add((xs, ys));
            allXValues.AddRange(xs);
            allYValues.AddRange(ys);
        }

        if (allXValues.Count == 0 || allYValues.Count == 0) return;

        // Compute nice axis scales for both X and Y
        var (xMin, xMax, xStep) = NiceAxisScale(Math.Min(0, allXValues.Min()), allXValues.Max());
        var (yMin, yMax, yStep) = NiceAxisScale(Math.Min(0, allYValues.Min()), allYValues.Max());
        var xRange = xMax - xMin;
        var yRange = yMax - yMin;
        if (xRange <= 0) xRange = 1;
        if (yRange <= 0) yRange = 1;

        // Y-axis gridlines and labels
        for (var tickVal = yMin; tickVal <= yMax + yStep * 0.01; tickVal += yStep)
        {
            var gridY = plotBottom + (float)((tickVal - yMin) / yRange) * plotHeight;
            page.AddLine(plotLeft, gridY, plotLeft + plotWidth, gridY,
                new PdfColor(0.85f, 0.85f, 0.85f), 0.5f);
            var label = FormatAxisValue(tickVal, axisFmtCode);
            page.AddText(label, plotLeft - label.Length * axisFontSize * 0.5f - 4f, gridY - axisFontSize * 0.3f, axisFontSize);
        }

        // X-axis tick labels
        for (var tickVal = xMin; tickVal <= xMax + xStep * 0.01; tickVal += xStep)
        {
            var gridX = plotLeft + (float)((tickVal - xMin) / xRange) * plotWidth;
            page.AddLine(gridX, plotBottom, gridX, plotBottom + plotHeight,
                new PdfColor(0.85f, 0.85f, 0.85f), 0.5f);
            var label = FormatAxisValue(tickVal, axisFmtCode);
            page.AddText(label, gridX - axisFontSize * 0.5f, plotBottom - axisFontSize * 1.5f, axisFontSize);
        }

        // Plot data points
        for (var si = 0; si < seriesData.Count; si++)
        {
            var (xs, ys) = seriesData[si];
            var color = ChartColors[si % ChartColors.Length];
            var count = Math.Min(xs.Length, ys.Length);
            for (var pi = 0; pi < count; pi++)
            {
                var px = plotLeft + (float)((xs[pi] - xMin) / xRange) * plotWidth;
                var py = plotBottom + (float)((ys[pi] - yMin) / yRange) * plotHeight;
                // Draw marker (small filled rectangle)
                page.AddRectangle(px - 3, py - 3, 6, 6, color);
            }
        }

        // Axes
        page.AddLine(plotLeft, plotBottom, plotLeft, plotBottom + plotHeight,
            new PdfColor(0, 0, 0), 0.8f);
        page.AddLine(plotLeft, plotBottom, plotLeft + plotWidth, plotBottom,
            new PdfColor(0, 0, 0), 0.8f);

        // Legend (always show for scatter/bubble, even single series)
        var legendX = plotLeft + plotWidth * 0.05f;
        var legendY = plotBottom + plotHeight + 5f;
        for (var i = 0; i < series.Count; i++)
        {
            var color = ChartColors[i % ChartColors.Length];
            page.AddRectangle(legendX, legendY, 8, 8, color);
            var serName = string.IsNullOrEmpty(series[i].Name) ? $"Series{i + 1}" : series[i].Name;
            page.AddText(TruncateLabel(serName, 12), legendX + 10, legendY, axisFontSize);
            legendX += (serName.Length + 3) * axisFontSize * 0.5f;
        }
    }

    /// <summary>Renders an area chart (filled line chart) using vertical strips to approximate fill.</summary>
    private static void RenderAreaChart(PdfPage page, ExcelChartInfo chart,
        float plotLeft, float plotBottom, float plotWidth, float plotHeight,
        float labelFontSize, float axisFontSize, string axisFmtCode = "")
    {
        var series = chart.Series;
        if (series.Count == 0) return;

        var isStacked = chart.ChartType.Contains("stacked", StringComparison.OrdinalIgnoreCase);
        var isPercentStacked = chart.ChartType.Contains("percentStacked", StringComparison.OrdinalIgnoreCase);

        var categories = series[0].Categories;
        var numPoints = Math.Max(categories.Length, series.Max(s => s.Values.Length));
        if (numPoints == 0) return;

        // Determine axis scale
        double dataMax, dataMin;
        if (isPercentStacked)
        {
            dataMin = 0; dataMax = 100;
        }
        else if (isStacked)
        {
            dataMax = 0; dataMin = 0;
            for (var pi = 0; pi < numPoints; pi++)
            {
                double posSum = 0, negSum = 0;
                for (var si = 0; si < series.Count; si++)
                {
                    var val = pi < series[si].Values.Length ? series[si].Values[pi] : 0;
                    if (val >= 0) posSum += val; else negSum += val;
                }
                dataMax = Math.Max(dataMax, posSum);
                dataMin = Math.Min(dataMin, negSum);
            }
        }
        else
        {
            var allValues = series.SelectMany(s => s.Values).ToArray();
            if (allValues.Length == 0) return;
            dataMax = allValues.Max();
            dataMin = Math.Min(0, allValues.Min());
        }

        var (niceMin, niceMax, niceStep) = isPercentStacked
            ? (0.0, 100.0, 10.0)
            : NiceAxisScale(dataMin, dataMax);
        var range = niceMax - niceMin;
        if (range <= 0) range = 1;

        // Y-axis gridlines and labels
        for (var tickVal = niceMin; tickVal <= niceMax + niceStep * 0.01; tickVal += niceStep)
        {
            var gridY = plotBottom + (float)((tickVal - niceMin) / range) * plotHeight;
            page.AddLine(plotLeft, gridY, plotLeft + plotWidth, gridY,
                new PdfColor(0.85f, 0.85f, 0.85f), 0.5f);
            var label = FormatAxisValue(tickVal);
            if (isPercentStacked) label += "%";
            page.AddText(label, plotLeft - label.Length * axisFontSize * 0.5f - 4f, gridY - axisFontSize * 0.3f, axisFontSize);
        }

        var baselineY = plotBottom + (float)((0 - niceMin) / range) * plotHeight;
        var stripWidth = Math.Max(1f, plotWidth / Math.Max(1, numPoints * 4));

        if (isStacked)
        {
            // Stacked area: compute cumulative base arrays per point
            // Render from bottom (first series) to top (last series)
            // cumulativeBase[pi] = sum of previous series at point pi
            var cumulativeBase = new double[numPoints];

            for (var si = 0; si < series.Count; si++)
            {
                var s = series[si];
                var color = ChartColors[si % ChartColors.Length];
                var fillColor = new PdfColor(
                    Math.Min(1f, color.R * 0.5f + 0.5f),
                    Math.Min(1f, color.G * 0.5f + 0.5f),
                    Math.Min(1f, color.B * 0.5f + 0.5f));

                // Compute top values for this series = cumulativeBase + series values
                var topValues = new double[numPoints];
                var catTotals = isPercentStacked ? new double[numPoints] : null;
                if (isPercentStacked)
                {
                    for (var pi = 0; pi < numPoints; pi++)
                    {
                        double total = 0;
                        for (var sj = 0; sj < series.Count; sj++)
                            total += Math.Abs(pi < series[sj].Values.Length ? series[sj].Values[pi] : 0);
                        catTotals![pi] = total == 0 ? 1 : total;
                    }
                }

                for (var pi = 0; pi < numPoints; pi++)
                {
                    var rawVal = pi < s.Values.Length ? s.Values[pi] : 0;
                    var val = isPercentStacked ? (rawVal / catTotals![pi] * 100) : rawVal;
                    topValues[pi] = cumulativeBase[pi] + val;
                }

                // Draw filled strips between base and top
                for (var px = 0f; px < plotWidth; px += stripWidth)
                {
                    var fraction = px / plotWidth;
                    var dataIdx = fraction * (numPoints - 1);
                    var idx0 = (int)Math.Floor(dataIdx);
                    var idx1 = Math.Min(idx0 + 1, numPoints - 1);
                    var t = dataIdx - idx0;
                    var valTop = topValues[idx0] * (1 - t) + topValues[idx1] * t;
                    var valBase = cumulativeBase[idx0] * (1 - t) + cumulativeBase[idx1] * t;

                    var yTop = plotBottom + (float)((valTop - niceMin) / range) * plotHeight;
                    var yBase = plotBottom + (float)((valBase - niceMin) / range) * plotHeight;
                    var fillHeight = Math.Abs(yTop - yBase);
                    if (fillHeight > 0.5f)
                        page.AddRectangle(plotLeft + px, Math.Min(yTop, yBase), stripWidth, fillHeight, fillColor);
                }

                // Draw top line
                for (var pi = 1; pi < numPoints; pi++)
                {
                    var x1 = plotLeft + (pi - 1) * plotWidth / Math.Max(1, numPoints - 1);
                    var y1 = plotBottom + (float)((topValues[pi - 1] - niceMin) / range) * plotHeight;
                    var x2 = plotLeft + pi * plotWidth / Math.Max(1, numPoints - 1);
                    var y2 = plotBottom + (float)((topValues[pi] - niceMin) / range) * plotHeight;
                    page.AddLine(x1, y1, x2, y2, color, 1.5f);
                }

                // Update cumulative base
                for (var pi = 0; pi < numPoints; pi++)
                    cumulativeBase[pi] = topValues[pi];
            }
        }
        else
        {
            // Non-stacked: render each series independently (back to front)
            for (var si = series.Count - 1; si >= 0; si--)
            {
                var s = series[si];
                var color = ChartColors[si % ChartColors.Length];
                var fillColor = new PdfColor(
                    Math.Min(1f, color.R * 0.5f + 0.5f),
                    Math.Min(1f, color.G * 0.5f + 0.5f),
                    Math.Min(1f, color.B * 0.5f + 0.5f));

                for (var px = 0f; px < plotWidth; px += stripWidth)
                {
                    var fraction = px / plotWidth;
                    var dataIdx = fraction * (s.Values.Length - 1);
                    var idx0 = (int)Math.Floor(dataIdx);
                    var idx1 = Math.Min(idx0 + 1, s.Values.Length - 1);
                    var t = dataIdx - idx0;
                    var val = s.Values[idx0] * (1 - t) + s.Values[idx1] * t;

                    var valY = plotBottom + (float)((val - niceMin) / range) * plotHeight;
                    var fillBottom = Math.Min(valY, baselineY);
                    var fillHeight = Math.Abs(valY - baselineY);
                    if (fillHeight > 0.5f)
                        page.AddRectangle(plotLeft + px, fillBottom, stripWidth, fillHeight, fillColor);
                }

                for (var pi = 1; pi < s.Values.Length; pi++)
                {
                    var x1 = plotLeft + (pi - 1) * plotWidth / Math.Max(1, numPoints - 1);
                    var y1 = plotBottom + (float)((s.Values[pi - 1] - niceMin) / range) * plotHeight;
                    var x2 = plotLeft + pi * plotWidth / Math.Max(1, numPoints - 1);
                    var y2 = plotBottom + (float)((s.Values[pi] - niceMin) / range) * plotHeight;
                    page.AddLine(x1, y1, x2, y2, color, 1.5f);
                }
            }
        }

        // Category labels
        for (var ci = 0; ci < categories.Length; ci++)
        {
            var xPos = plotLeft + ci * plotWidth / Math.Max(1, numPoints - 1);
            var label = TruncateLabel(categories[ci], 15);
            page.AddText(label, xPos - axisFontSize, plotBottom - axisFontSize * 1.5f, axisFontSize);
        }

        // Axes
        page.AddLine(plotLeft, plotBottom, plotLeft, plotBottom + plotHeight,
            new PdfColor(0, 0, 0), 0.8f);
        page.AddLine(plotLeft, plotBottom, plotLeft + plotWidth, plotBottom,
            new PdfColor(0, 0, 0), 0.8f);

        // Legend (reversed for stacked charts)
        RenderLegend(page, series, plotLeft + plotWidth * 0.05f, plotBottom + plotHeight + 5f, axisFontSize, isStacked);
    }

    /// <summary>Renders a pie or doughnut chart using rectangles to approximate sectors.</summary>
    private static void RenderPieChart(PdfPage page, ExcelChartInfo chart,
        float chartX, float chartTop, float chartWidth, float chartHeight,
        float plotLeft, float plotBottom, float plotWidth, float plotHeight,
        float labelFontSize, bool isDoughnut, bool showPercent)
    {
        var series = chart.Series;
        if (series.Count == 0 || series[0].Values.Length == 0) return;

        var values = series[0].Values;
        var categories = series[0].Categories;
        var total = values.Where(v => v > 0).Sum();
        if (total <= 0) return;

        // Approximate pie chart using colored rectangles arranged in a grid
        // Each slice gets a rectangle proportional to its share
        var centerX = plotLeft + plotWidth * 0.4f;
        var centerY = plotBottom + plotHeight * 0.5f;
        var radius = Math.Min(plotWidth, plotHeight) * 0.35f;

        // Draw pie slices as approximate rectangular blocks (layered from center)
        // Use a grid-based approach: divide the pie area into small cells
        var gridSize = 1.5f;
        var numCells = (int)(radius * 2 / gridSize);
        var cumulativeAngle = 0.0;
        var sliceAngles = new double[values.Length];
        for (var i = 0; i < values.Length; i++)
        {
            sliceAngles[i] = values[i] > 0 ? values[i] / total * 360.0 : 0;
        }

        // Render using small rectangles to approximate circular sectors
        for (var gx = -numCells; gx <= numCells; gx++)
        {
            for (var gy = -numCells; gy <= numCells; gy++)
            {
                var px = gx * gridSize;
                var py = gy * gridSize;
                var dist = Math.Sqrt(px * px + py * py);
                if (dist > radius) continue;
                if (isDoughnut && dist < radius * 0.5) continue;

                // Determine which slice this pixel belongs to
                var angle = Math.Atan2(py, px) * 180.0 / Math.PI;
                if (angle < 0) angle += 360;
                // Start from top (90°)
                angle = (90 - angle + 360) % 360;

                var cumAngle = 0.0;
                var sliceIdx = 0;
                for (var i = 0; i < values.Length; i++)
                {
                    cumAngle += sliceAngles[i];
                    if (angle < cumAngle)
                    {
                        sliceIdx = i;
                        break;
                    }
                }

                var color = ChartColors[sliceIdx % ChartColors.Length];
                page.AddRectangle(centerX + px, centerY + py, gridSize, gridSize, color);
            }
        }

        // Labels for each slice (only when data labels are enabled)
        if (showPercent)
        {
            var seriesName = series[0].Name;
            cumulativeAngle = 0;
            for (var i = 0; i < values.Length; i++)
            {
                var midAngle = cumulativeAngle + sliceAngles[i] / 2;
                cumulativeAngle += sliceAngles[i];

                var labelDist = radius + 15;
                var radians = (90 - midAngle) * Math.PI / 180.0;
                var lx = centerX + (float)(Math.Cos(radians) * labelDist);
                var ly = centerY + (float)(Math.Sin(radians) * labelDist);

                var catName = i < categories.Length ? categories[i] : $"Slice{i + 1}";
                var pct = total > 0 ? (int)Math.Round(values[i] / total * 100) : 0;
                var valStr = FormatAxisValue(values[i]);
                var labelText = $"{catName}; {seriesName}; {valStr}; {pct}%";
                page.AddText(TruncateLabel(labelText, 30), lx, ly, labelFontSize - 1);
            }
        }

        // Legend: vertical list of category names with color swatches below the pie
        var legendY = plotBottom - 10f;
        for (var i = 0; i < values.Length; i++)
        {
            var color = ChartColors[i % ChartColors.Length];
            page.AddRectangle(plotLeft + plotWidth * 0.55f, legendY, 8, 8, color);
            var legendName = i < categories.Length ? categories[i] : $"Slice{i + 1}";
            page.AddText(TruncateLabel(legendName, 12), plotLeft + plotWidth * 0.55f + 10, legendY, labelFontSize - 1);
            legendY -= labelFontSize * 1.5f;
        }
    }

    /// <summary>Renders legend entries for chart series.</summary>
    private static void RenderLegend(PdfPage page, List<ExcelChartSeries> series,
        float x, float y, float fontSize, bool reverseOrder = false)
    {
        if (series.Count <= 1) return;
        var legendX = x;
        var count = series.Count;
        for (var ii = 0; ii < count; ii++)
        {
            var i = reverseOrder ? (count - 1 - ii) : ii;
            var color = ChartColors[i % ChartColors.Length];
            page.AddRectangle(legendX, y, 8, 8, color);
            var name = string.IsNullOrEmpty(series[i].Name) ? $"Series{i + 1}" : series[i].Name;
            page.AddText(TruncateLabel(name, 12), legendX + 10, y, fontSize);
            legendX += (name.Length + 3) * fontSize * 0.5f;
        }
    }

    /// <summary>Formats an axis value label, optionally using the chart's axis format code.</summary>
    private static string FormatAxisValue(double val, string formatCode = "")
    {
        // Apply comma formatting when axis formatCode indicates it (e.g., "#,##0")
        if (!string.IsNullOrEmpty(formatCode) &&
            (formatCode.Contains("#,##") || formatCode.Contains("0,0")))
        {
            if (val == Math.Floor(val))
                return val.ToString("N0", System.Globalization.CultureInfo.InvariantCulture);
            return val.ToString("N1", System.Globalization.CultureInfo.InvariantCulture);
        }

        if (val == Math.Floor(val))
            return $"{val:F0}";
        return $"{val:F1}";
    }

    /// <summary>
    /// Calculates "nice" axis bounds and step for chart axis labeling.
    /// Returns (niceMin, niceMax, step) that produce round-number axis labels.
    /// </summary>
    private static (double NiceMin, double NiceMax, double Step) NiceAxisScale(double dataMin, double dataMax, int desiredTicks = 6)
    {
        if (dataMax <= dataMin) dataMax = dataMin + 1;
        var rawRange = dataMax - dataMin;
        // Calculate a rough step size
        var roughStep = rawRange / desiredTicks;
        // Find the magnitude of the step
        var mag = Math.Pow(10, Math.Floor(Math.Log10(roughStep)));
        var residual = roughStep / mag;
        // Round to a nice step: 1, 2, 5, 10
        double niceStep;
        if (residual <= 1.5) niceStep = 1 * mag;
        else if (residual <= 3.5) niceStep = 2 * mag;
        else if (residual <= 7.5) niceStep = 5 * mag;
        else niceStep = 10 * mag;

        var niceMin = Math.Floor(dataMin / niceStep) * niceStep;
        var niceMax = Math.Ceiling(dataMax / niceStep) * niceStep;
        // Ensure headroom: if niceMax is too close to dataMax, add one tick step
        if (niceStep > 0 && (niceMax - dataMax) < 0.3 * niceStep)
            niceMax += niceStep;
        // Ensure at least 2 ticks
        if (niceMax <= niceMin + niceStep) niceMax = niceMin + niceStep * 2;
        return (niceMin, niceMax, niceStep);
    }

    /// <summary>Truncates a label to max characters.</summary>
    private static string TruncateLabel(string label, int maxChars)
        => label.Length <= maxChars ? label : label[..(maxChars - 1)] + "\u2026";

    /// <summary>
    /// Wrap a single cell text into multiple lines using precise Helvetica widths.
    /// </summary>
    private static string[] WrapCellText(string text, float widthPts, float fontSize)
    {
        if (FittingChars(text, widthPts, fontSize) >= text.Length)
            return new[] { text };

        var lines = new List<string>();
        var remaining = text.AsSpan();
        while (remaining.Length > 0)
        {
            var fit = FittingChars(remaining.ToString(), widthPts, fontSize);
            if (fit >= remaining.Length)
            {
                lines.Add(remaining.ToString());
                break;
            }
            // Try to break at a space within the fitted portion
            var breakAt = fit;
            for (var j = fit - 1; j >= fit / 2; j--)
            {
                if (remaining[j] == ' ')
                {
                    breakAt = j;
                    break;
                }
            }
            if (breakAt == fit && breakAt < remaining.Length)
            {
                // No space found — hard break
                lines.Add(remaining[..breakAt].ToString());
                remaining = remaining[breakAt..];
            }
            else
            {
                lines.Add(remaining[..breakAt].ToString());
                remaining = remaining[(breakAt + 1)..]; // skip the space
            }
        }
        return lines.ToArray();
    }

    /// <summary>
    /// Checks if a sheet name is a generic default like Sheet1, Sheet2, etc.
    /// </summary>
    private static bool IsDefaultSheetName(string name)
    {
        if (name.StartsWith("Sheet", StringComparison.OrdinalIgnoreCase) && name.Length <= 8)
        {
            return int.TryParse(name.AsSpan(5), out _);
        }
        return false;
    }

    /// <summary>
    /// Returns true for characters that are full-width in CJK fonts
    /// (CJK Unified Ideographs, CJK Compatibility, Hangul, Kana, etc.).
    /// </summary>
    private static bool IsFullWidthChar(char ch)
        => ch >= '\u2E80' && (
               ch <= '\u9FFF'   // CJK Radicals, Kangxi Radicals, CJK Strokes, CJK Unified Ideographs, etc.
            || (ch >= '\uAC00' && ch <= '\uD7AF')   // Hangul Syllables
            || (ch >= '\uF900' && ch <= '\uFAFF')   // CJK Compatibility Ideographs
            || (ch >= '\uFE30' && ch <= '\uFE4F')   // CJK Compatibility Forms
            || (ch >= '\uFF00' && ch <= '\uFFEF'));  // Fullwidth Forms

    /// <summary>
    /// Measures the approximate display width of a string in points, accounting
    /// for full-width CJK characters (~1.0 × fontSize) and half-width Latin
    /// characters (~0.47 × fontSize).
    /// </summary>
    private static float MeasureStringWidth(string text, float fontSize)
    {
        var latinFactor = fontSize * 0.47f;
        var cjkFactor = fontSize * 0.95f;
        var total = 0f;
        foreach (var ch in text)
            total += IsFullWidthChar(ch) ? cjkFactor : latinFactor;
        return total;
    }

    /// <summary>
    /// Returns the approximate maximum number of characters of <paramref name="text"/>
    /// that fit within <paramref name="widthPts"/> points, accounting for CJK width.
    /// If the text has no CJK characters, this is equivalent to widthPts / avgCharWidth.
    /// </summary>
    private static int FittingChars(string text, float widthPts, float fontSize)
    {
        // Use Helvetica character widths scaled by ~0.90 to approximate Calibri/LibreOffice
        // rendering (which uses narrower fonts). This ensures text clipping matches
        // what LibreOffice shows in reference PDFs.
        double used = 0;
        const double scale = 0.93;
        for (var i = 0; i < text.Length; i++)
        {
            used += HelveticaCharWidth(text[i]) * fontSize / 1000.0 * scale;
            if (used > widthPts) return Math.Max(1, i);
        }
        return text.Length;
    }

    /// <summary>Returns Helvetica character width in 1/1000 em units.</summary>
    private static int HelveticaCharWidth(char ch) => ch switch
    {
        ' ' => 278, '!' => 278, '"' => 355, '#' => 556, '$' => 556, '%' => 889,
        '&' => 667, '\'' => 191, '(' => 333, ')' => 333, '*' => 389, '+' => 584,
        ',' => 278, '-' => 333, '.' => 278, '/' => 278,
        >= '0' and <= '9' => 556,
        ':' => 278, ';' => 278, '<' => 584, '=' => 584, '>' => 584, '?' => 556,
        '@' => 1015,
        'A' => 667, 'B' => 667, 'C' => 722, 'D' => 722, 'E' => 667, 'F' => 611,
        'G' => 778, 'H' => 722, 'I' => 278, 'J' => 500, 'K' => 667, 'L' => 556,
        'M' => 833, 'N' => 722, 'O' => 778, 'P' => 667, 'Q' => 778, 'R' => 722,
        'S' => 667, 'T' => 611, 'U' => 722, 'V' => 667, 'W' => 944, 'X' => 667,
        'Y' => 667, 'Z' => 611,
        '[' => 278, '\\' => 278, ']' => 278, '^' => 469, '_' => 556, '`' => 333,
        'a' => 556, 'b' => 556, 'c' => 500, 'd' => 556, 'e' => 556, 'f' => 278,
        'g' => 556, 'h' => 556, 'i' => 222, 'j' => 222, 'k' => 500, 'l' => 222,
        'm' => 833, 'n' => 556, 'o' => 556, 'p' => 556, 'q' => 556, 'r' => 333,
        's' => 500, 't' => 278, 'u' => 556, 'v' => 500, 'w' => 722, 'x' => 500,
        'y' => 500, 'z' => 500,
        '{' => 334, '|' => 260, '}' => 334, '~' => 584,
        _ => IsFullWidthChar(ch) ? 1000 : 556
    };

    /// <summary>
    /// Measures text width more precisely using Helvetica character widths (in 1/1000 em units).
    /// Used for column-width-aware number formatting.
    /// </summary>
    private static double MeasureHelveticaWidth(string text, double fontSize)
    {
        double total = 0;
        foreach (var ch in text)
            total += HelveticaCharWidth(ch);
        return total * fontSize / 1000.0;
    }

    /// <summary>
    /// Re-formats numeric cell text to fit within the column width, matching LibreOffice's
    /// General format auto-shrink behavior. When a number doesn't fit the column, it
    /// progressively tries: integer form, reduced decimal precision, scientific notation.
    /// </summary>
    private static string FitNumericText(string text, double colWidthPt, double fontSize)
    {
        var ci = System.Globalization.CultureInfo.InvariantCulture;

        // Only re-format if it looks like a plain number (no currency symbols, etc.)
        if (!double.TryParse(text, System.Globalization.NumberStyles.Float, ci, out var value))
            return text;

        // Account for minimal internal cell margin that LibreOffice applies.
        var textAreaWidth = colWidthPt - 1.0;

        // Check if current text already fits
        if (MeasureHelveticaWidth(text, fontSize) <= textAreaWidth)
            return text;

        var abs = Math.Abs(value);

        // For normal-range numbers (1e-4 to 1e10), prefer decimal precision reduction
        if (abs >= 1e-4 && abs < 1e10)
        {
            // Determine max decimal places to try (based on position of decimal point)
            var intDigits = abs >= 1 ? (int)Math.Floor(Math.Log10(abs)) + 1 : 1;
            var maxDecimals = Math.Max(0, 10 - intDigits - 1); // ~10 sig digits minus int part minus dot
            for (int d = maxDecimals; d >= 1; d--)
            {
                var dec = value.ToString($"F{d}", ci);
                if (MeasureHelveticaWidth(dec, fontSize) <= textAreaWidth)
                    return dec;
            }
            // Try integer form
            var intForm = Math.Round(value).ToString("F0", ci);
            if (MeasureHelveticaWidth(intForm, fontSize) <= textAreaWidth)
                return intForm;
        }

        // Try scientific notation with decreasing precision until it fits
        for (int digits = 3; digits >= 0; digits--)
        {
            var fmt = digits > 0 ? "0." + new string('#', digits) + "E+00" : "0E+00";
            var sci = value.ToString(fmt, ci);
            if (MeasureHelveticaWidth(sci, fontSize) <= textAreaWidth)
                return sci;
        }

        return text; // Can't fit, return as-is
    }

    /// <summary>
    /// Calculates natural (unscaled) column widths with min/max bounds.
    /// When an Excel column width is explicitly set (or default), that takes precedence
    /// over content-based width so the output matches the source spreadsheet layout.
    /// </summary>
    private static float[] CalculateNaturalColumnWidths(ExcelSheet sheet, int maxCols, float usableWidth, ConversionOptions options)
    {
        var avgCharWidth = options.FontSize * 0.47f;
        // Track the max measured width (in points) per column rather than raw char count.
        // This accounts for CJK characters being ~2× wider than Latin chars.
        var colMaxWidthPts = new float[maxCols];

        foreach (var row in sheet.Rows)
        {
            for (var col = 0; col < row.Count && col < maxCols; col++)
            {
                var w = MeasureStringWidth(row[col].Text, options.FontSize);
                if (w > colMaxWidthPts[col]) colMaxWidthPts[col] = w;
            }
        }

        // Max column width: relax for sheets with few columns
        var maxColWidth = maxCols <= 2 ? usableWidth * 0.95f : usableWidth * 0.6f;

        // Min column width: enforce readability (wider for many-column sheets)
        var minColWidth = maxCols > 12 ? avgCharWidth * 9 : avgCharWidth * 4;

        var widths = new float[maxCols];
        // Use Excel column widths only when the spreadsheet explicitly specifies them
        var hasExcelWidths = sheet.ColumnWidths.Count > 0 || sheet.DefaultColumnWidth > 0f;

        for (var i = 0; i < maxCols; i++)
        {
            if (hasExcelWidths)
            {
                // Use Excel column width (explicit override or explicit default)
                var charUnits = sheet.ColumnWidths.TryGetValue(i, out var ew)
                    ? ew
                    : sheet.DefaultColumnWidth > 0f ? sheet.DefaultColumnWidth : 8.43f;
                var excelPts = ExcelSheet.CharUnitsToPoints(charUnits);
                // Clamp to reasonable bounds but respect the spreadsheet's intent
                widths[i] = Math.Clamp(excelPts, minColWidth, maxColWidth);
            }
            else if (maxCols >= 2)
            {
                // No explicit column widths — use Excel's default column width (8.43 char units).
                // This matches LibreOffice/Excel behaviour where unset multi-column sheets use the
                // workbook default, producing text clipping identical to the reference PDF.
                var defaultPts = ExcelSheet.CharUnitsToPoints(8.43f);
                widths[i] = Math.Clamp(defaultPts, minColWidth, maxColWidth);
            }
            else
            {
                // Single-column sheet: use content-based width so the column fills the page
                // (LibreOffice expands 1-column sheets to page width).
                var natural = colMaxWidthPts[i] + 2 * avgCharWidth;
                natural = Math.Max(natural, 5 * avgCharWidth); // minimum 5 chars
                widths[i] = Math.Clamp(natural, minColWidth, maxColWidth);
            }
        }

        return widths;
    }

    /// <summary>
    /// Scales column widths to fit within usable width if they exceed it.
    /// </summary>
    private static float[] ScaleColumnWidths(float[] naturalWidths, float usableWidth, float columnPadding, float avgCharWidth)
    {
        var maxCols = naturalWidths.Length;
        var totalPadding = columnPadding * (maxCols - 1);
        var total = naturalWidths.Sum() + totalPadding;

        if (total <= usableWidth)
            return (float[])naturalWidths.Clone();

        var result = (float[])naturalWidths.Clone();
        var available = usableWidth - totalPadding;
        if (available <= 0)
            available = usableWidth * 0.9f;
        var scale = available / naturalWidths.Sum();
        for (var i = 0; i < result.Length; i++)
        {
            result[i] = Math.Max(result[i] * scale, avgCharWidth);
        }

        return result;
    }
}
