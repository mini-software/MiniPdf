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

        /// <summary>Page top margin in points (default: 50).</summary>
        public float MarginTop { get; set; } = 50;

        /// <summary>Page right margin in points (default: 50).</summary>
        public float MarginRight { get; set; } = 50;

        /// <summary>Page bottom margin in points (default: 50).</summary>
        public float MarginBottom { get; set; } = 50;

        /// <summary>Padding between columns in points (default: 4).</summary>
        public float ColumnPadding { get; set; } = 4;

        /// <summary>Line spacing multiplier (default: 1.6).</summary>
        public float LineSpacing { get; set; } = 1.6f;

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
            var cellLines = new string[columns.Length][];

            for (var i = 0; i < columns.Length; i++)
            {
                var col = columns[i];
                if (col < row.Count)
                {
                    var cellText = row[col].Text;
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
                            // Excel/LibreOffice clip text at the column boundary when the
                            // next cell to the right contains content.  For the last column
                            // in the group (or when the next cell is empty) the text overflows.
                            var maxChars = Math.Max(1, (int)(colWidths[i] / avgCharWidth));
                            var isLastCol = (i == columns.Length - 1);
                            var nextCellHasContent = false;
                            if (!isLastCol)
                            {
                                var nextCol = columns[i + 1];
                                nextCellHasContent = nextCol < row.Count
                                    && !string.IsNullOrEmpty(row[nextCol].Text);
                            }
                            if (!isLastCol && nextCellHasContent && cellText.Length > maxChars)
                            {
                                // Truncate to column width (matches LibreOffice clip)
                                cellLines[i] = new[] { cellText[..maxChars] };
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
            if (currentY - rowHeight < options.MarginBottom && currentPage != null)
            {
                currentPage = doc.AddPage(pageWidth, pageHeight);
                currentY = pageHeight - options.MarginTop;
                // Update the row's top position on the new page
                rowTopY[excelRowIndex] = currentY;
                rowPage[excelRowIndex] = currentPage;
            }

            // Render cells
            var x = options.MarginLeft;
            for (var i = 0; i < columns.Length; i++)
            {
                var lines = cellLines[i];
                var col = columns[i];
                var color = col < row.Count ? row[col].Color : null;
                var cellY = currentY;

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
        new PdfColor(0.259f, 0.522f, 0.957f), // blue
        new PdfColor(0.914f, 0.306f, 0.220f), // red
        new PdfColor(0.984f, 0.737f, 0.016f), // yellow/gold
        new PdfColor(0.129f, 0.671f, 0.369f), // green
        new PdfColor(1.000f, 0.420f, 0.000f), // orange
        new PdfColor(0.400f, 0.310f, 0.643f), // purple
        new PdfColor(0.000f, 0.690f, 0.710f), // teal
        new PdfColor(0.890f, 0.180f, 0.490f), // pink
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

        // Draw chart title
        var titleY = top;
        if (!string.IsNullOrEmpty(chart.Title))
        {
            page.AddText(chart.Title, x + width * 0.3f, titleY - titleFontSize, titleFontSize);
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
            RenderPieChart(page, chart, x, top, width, height, plotLeft, plotBottom, plotWidth, plotHeight, labelFontSize, type.Contains("doughnut"));
        }
        else if (type.Contains("line") || type.Contains("scatter") || type.Contains("radar"))
        {
            var skipAxisLabels = type.Contains("radar");
            RenderLineChart(page, chart, plotLeft, plotBottom, plotWidth, plotHeight, labelFontSize, axisFontSize, skipAxisLabels);
        }
        else if (type.Contains("area"))
        {
            RenderAreaChart(page, chart, plotLeft, plotBottom, plotWidth, plotHeight, labelFontSize, axisFontSize);
        }
        else
        {
            // Default: bar/column/stock/bubble → bar chart
            RenderBarChart(page, chart, plotLeft, plotBottom, plotWidth, plotHeight, labelFontSize, axisFontSize);
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
        float labelFontSize, float axisFontSize)
    {
        var series = chart.Series;
        if (series.Count == 0) return;

        // Get all values to determine scale
        var allValues = series.SelectMany(s => s.Values).ToArray();
        if (allValues.Length == 0) return;

        var dataMax = allValues.Max();
        var dataMin = Math.Min(0, allValues.Min());

        // Use nice axis scaling for round number labels
        var (niceMin, niceMax, niceStep) = NiceAxisScale(dataMin, dataMax);
        var range = niceMax - niceMin;
        if (range <= 0) range = 1;

        var categories = series[0].Categories;
        var numCats = Math.Max(categories.Length, series.Max(s => s.Values.Length));
        if (numCats == 0) return;

        var numSeries = series.Count;
        var groupWidth = plotWidth / numCats;
        var barWidth = groupWidth * 0.7f / numSeries;
        var groupPadding = groupWidth * 0.15f;

        // Y-axis baseline (where value=0 sits)
        var baselineY = plotBottom + (float)((0 - niceMin) / range) * plotHeight;

        // Draw Y-axis gridlines and labels at nice round numbers
        for (var tickVal = niceMin; tickVal <= niceMax + niceStep * 0.01; tickVal += niceStep)
        {
            var gridY = plotBottom + (float)((tickVal - niceMin) / range) * plotHeight;
            page.AddLine(plotLeft, gridY, plotLeft + plotWidth, gridY,
                new PdfColor(0.85f, 0.85f, 0.85f), 0.5f);
            var label = FormatAxisValue(tickVal);
            page.AddText(label, plotLeft - label.Length * axisFontSize * 0.5f - 4f, gridY - axisFontSize * 0.3f, axisFontSize);
        }

        // Draw bars
        for (var ci = 0; ci < numCats; ci++)
        {
            var groupX = plotLeft + ci * groupWidth + groupPadding;
            for (var si = 0; si < numSeries; si++)
            {
                var val = si < series.Count && ci < series[si].Values.Length
                    ? series[si].Values[ci] : 0;
                var barX = groupX + si * barWidth;
                // Bar extends from baseline (value=0) to the data value
                var valY = plotBottom + (float)((val - niceMin) / range) * plotHeight;
                var barBottom = Math.Min(valY, baselineY);
                var barDrawH = Math.Abs(valY - baselineY);
                if (barDrawH < 0.5f) barDrawH = 0.5f;

                var color = ChartColors[si % ChartColors.Length];
                page.AddRectangle(barX, barBottom, barWidth, barDrawH, color);
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

        // Legend
        RenderLegend(page, series, plotLeft + plotWidth * 0.05f, plotBottom + plotHeight + 5f, axisFontSize);
    }

    /// <summary>Renders a line chart.</summary>
    private static void RenderLineChart(PdfPage page, ExcelChartInfo chart,
        float plotLeft, float plotBottom, float plotWidth, float plotHeight,
        float labelFontSize, float axisFontSize, bool skipAxisLabels = false)
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
                var label = FormatAxisValue(tickVal);
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
            var step = Math.Max(1, numPoints / 10);
            for (var ci = 0; ci < categories.Length; ci += step)
            {
                var xPos = plotLeft + ci * plotWidth / Math.Max(1, numPoints - 1);
                var label = TruncateLabel(categories[ci], 12);
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

    /// <summary>Renders an area chart (filled line chart) using vertical strips to approximate fill.</summary>
    private static void RenderAreaChart(PdfPage page, ExcelChartInfo chart,
        float plotLeft, float plotBottom, float plotWidth, float plotHeight,
        float labelFontSize, float axisFontSize)
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
        var numPoints = Math.Max(categories.Length, series.Max(s => s.Values.Length));
        if (numPoints == 0) return;

        // Y-axis gridlines and labels
        for (var tickVal = niceMin; tickVal <= niceMax + niceStep * 0.01; tickVal += niceStep)
        {
            var gridY = plotBottom + (float)((tickVal - niceMin) / range) * plotHeight;
            page.AddLine(plotLeft, gridY, plotLeft + plotWidth, gridY,
                new PdfColor(0.85f, 0.85f, 0.85f), 0.5f);
            var label = FormatAxisValue(tickVal);
            page.AddText(label, plotLeft - label.Length * axisFontSize * 0.5f - 4f, gridY - axisFontSize * 0.3f, axisFontSize);
        }

        var baselineY = plotBottom + (float)((0 - niceMin) / range) * plotHeight;

        // Fill area for each series using vertical strips (render in reverse order for layering)
        var stripWidth = Math.Max(1f, plotWidth / Math.Max(1, numPoints * 4));
        for (var si = series.Count - 1; si >= 0; si--)
        {
            var s = series[si];
            var color = ChartColors[si % ChartColors.Length];
            // Use a lighter shade for fill (blend toward white)
            var fillColor = new PdfColor(
                Math.Min(1f, color.R * 0.5f + 0.5f),
                Math.Min(1f, color.G * 0.5f + 0.5f),
                Math.Min(1f, color.B * 0.5f + 0.5f));

            // Draw filled strips from baseline to data value
            for (var px = 0f; px < plotWidth; px += stripWidth)
            {
                // Interpolate value at this x position
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
                {
                    page.AddRectangle(plotLeft + px, fillBottom, stripWidth, fillHeight, fillColor);
                }
            }

            // Draw top line
            for (var pi = 1; pi < s.Values.Length; pi++)
            {
                var x1 = plotLeft + (pi - 1) * plotWidth / Math.Max(1, numPoints - 1);
                var y1 = plotBottom + (float)((s.Values[pi - 1] - niceMin) / range) * plotHeight;
                var x2 = plotLeft + pi * plotWidth / Math.Max(1, numPoints - 1);
                var y2 = plotBottom + (float)((s.Values[pi] - niceMin) / range) * plotHeight;
                page.AddLine(x1, y1, x2, y2, color, 1.5f);
            }
        }

        // Category labels
        var step = Math.Max(1, numPoints / 10);
        for (var ci = 0; ci < categories.Length; ci += step)
        {
            var xPos = plotLeft + ci * plotWidth / Math.Max(1, numPoints - 1);
            var label = TruncateLabel(categories[ci], 8);
            page.AddText(label, xPos - axisFontSize, plotBottom - axisFontSize * 1.5f, axisFontSize);
        }

        // Axes
        page.AddLine(plotLeft, plotBottom, plotLeft, plotBottom + plotHeight,
            new PdfColor(0, 0, 0), 0.8f);
        page.AddLine(plotLeft, plotBottom, plotLeft + plotWidth, plotBottom,
            new PdfColor(0, 0, 0), 0.8f);

        RenderLegend(page, series, plotLeft + plotWidth * 0.05f, plotBottom + plotHeight + 5f, axisFontSize);
    }

    /// <summary>Renders a pie or doughnut chart using rectangles to approximate sectors.</summary>
    private static void RenderPieChart(PdfPage page, ExcelChartInfo chart,
        float chartX, float chartTop, float chartWidth, float chartHeight,
        float plotLeft, float plotBottom, float plotWidth, float plotHeight,
        float labelFontSize, bool isDoughnut)
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
        var gridSize = 2f;
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

        // Labels for each slice
        cumulativeAngle = 0;
        for (var i = 0; i < values.Length; i++)
        {
            var midAngle = cumulativeAngle + sliceAngles[i] / 2;
            cumulativeAngle += sliceAngles[i];

            var labelDist = radius + 15;
            var radians = (90 - midAngle) * Math.PI / 180.0;
            var lx = centerX + (float)(Math.Cos(radians) * labelDist);
            var ly = centerY + (float)(Math.Sin(radians) * labelDist);

            var label = i < categories.Length ? categories[i] : $"Slice{i + 1}";
            page.AddText(TruncateLabel(label, 12), lx, ly, labelFontSize - 1);
        }
    }

    /// <summary>Renders legend entries for chart series.</summary>
    private static void RenderLegend(PdfPage page, List<ExcelChartSeries> series,
        float x, float y, float fontSize)
    {
        if (series.Count <= 1) return;
        var legendX = x;
        for (var i = 0; i < series.Count; i++)
        {
            var color = ChartColors[i % ChartColors.Length];
            page.AddRectangle(legendX, y, 8, 8, color);
            var name = string.IsNullOrEmpty(series[i].Name) ? $"Series{i + 1}" : series[i].Name;
            page.AddText(TruncateLabel(name, 12), legendX + 10, y, fontSize);
            legendX += (name.Length + 3) * fontSize * 0.5f;
        }
    }

    /// <summary>Formats an axis value label using full numbers (e.g. 12000 → "12000").</summary>
    private static string FormatAxisValue(double val)
    {
        if (val == Math.Floor(val)) return $"{val:F0}";
        return $"{val:F1}";
    }

    /// <summary>
    /// Calculates "nice" axis bounds and step for chart axis labeling.
    /// Returns (niceMin, niceMax, step) that produce round-number axis labels.
    /// </summary>
    private static (double NiceMin, double NiceMax, double Step) NiceAxisScale(double dataMin, double dataMax, int desiredTicks = 5)
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
        // Ensure at least 2 ticks
        if (niceMax <= niceMin + niceStep) niceMax = niceMin + niceStep * 2;
        return (niceMin, niceMax, niceStep);
    }

    /// <summary>Truncates a label to max characters.</summary>
    private static string TruncateLabel(string label, int maxChars)
        => label.Length <= maxChars ? label : label[..(maxChars - 1)] + "\u2026";

    /// <summary>
    /// Wrap a single cell text into multiple lines at word boundaries.
    /// </summary>
    private static string[] WrapCellText(string text, float widthPts, float avgCharWidth)
        => WrapCellText(text, Math.Max(1, (int)(widthPts / avgCharWidth)));

    private static string[] WrapCellText(string text, int maxCharsPerLine)
    {
        if (maxCharsPerLine <= 0) maxCharsPerLine = 1;
        if (text.Length <= maxCharsPerLine) return new[] { text };

        var lines = new List<string>();
        var words = text.Split(' ');

        var currentLine = "";
        foreach (var word in words)
        {
            if (currentLine.Length == 0)
            {
                currentLine = word;
            }
            else if (currentLine.Length + 1 + word.Length <= maxCharsPerLine)
            {
                currentLine += " " + word;
            }
            else
            {
                // If current line overflows, hard-break it
                while (currentLine.Length > maxCharsPerLine)
                {
                    lines.Add(currentLine[..maxCharsPerLine]);
                    currentLine = currentLine[maxCharsPerLine..];
                }
                if (currentLine.Length > 0)
                    lines.Add(currentLine);
                currentLine = word;
            }
        }

        // Handle the last line — might also need hard-breaking
        while (currentLine.Length > maxCharsPerLine)
        {
            lines.Add(currentLine[..maxCharsPerLine]);
            currentLine = currentLine[maxCharsPerLine..];
        }
        if (currentLine.Length > 0)
            lines.Add(currentLine);

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
    /// Calculates natural (unscaled) column widths with min/max bounds.
    /// When an Excel column width is explicitly set (or default), that takes precedence
    /// over content-based width so the output matches the source spreadsheet layout.
    /// </summary>
    private static float[] CalculateNaturalColumnWidths(ExcelSheet sheet, int maxCols, float usableWidth, ConversionOptions options)
    {
        var avgCharWidth = options.FontSize * 0.47f;
        var colMaxLengths = new int[maxCols];

        foreach (var row in sheet.Rows)
        {
            for (var col = 0; col < row.Count && col < maxCols; col++)
            {
                colMaxLengths[col] = Math.Max(colMaxLengths[col], row[col].Text.Length);
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
                var natural = (Math.Max(colMaxLengths[i], 3) + 2) * avgCharWidth;
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
