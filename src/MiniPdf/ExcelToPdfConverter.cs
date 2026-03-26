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
        /// <summary>Font size in points (default: 11).</summary>
        public float FontSize { get; set; } = 11;

        /// <summary>Page left margin in points (default: 50).</summary>
        public float MarginLeft { get; set; } = 54;

        /// <summary>Page top margin in points (default: 72 = 1 inch).</summary>
        public float MarginTop { get; set; } = 72;

        /// <summary>Page right margin in points (default: 50).</summary>
        public float MarginRight { get; set; } = 54;

        /// <summary>Page bottom margin in points (default: 72 = 1 inch).</summary>
        public float MarginBottom { get; set; } = 72;

        /// <summary>Padding between columns in points (default: 3).</summary>
        public float ColumnPadding { get; set; } = 3;

        /// <summary>Line spacing multiplier (default: 1.5).</summary>
        public float LineSpacing { get; set; } = 1.5f;

        /// <summary>Page width in points (default: 612 = US Letter).</summary>
        public float PageWidth { get; set; } = 612;

        /// <summary>Page height in points (default: 792 = US Letter).</summary>
        public float PageHeight { get; set; } = 792;

        /// <summary>Whether to include sheet name as a header (default: false).</summary>
        public bool IncludeSheetName { get; set; } = false;

        /// <summary>When true, scale cell-level font sizes by print scale factor
        /// during auto-row-height and rendering. Used when fitToHeight requires
        /// vertical compression to fit content on the specified number of pages.</summary>
        internal bool ScaleCellFonts { get; set; } = false;
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

        // Track page ranges per sheet for footer rendering
        var sheetPageRanges = new List<(ExcelSheet sheet, int startPage, int endPage)>();
        foreach (var sheet in sheets)
        {
            var startPage = doc.Pages.Count;
            RenderSheet(doc, sheet, options);
            var endPage = doc.Pages.Count - 1;
            if (startPage <= endPage)
                sheetPageRanges.Add((sheet, startPage, endPage));
        }

        // If no sheets found, create at least one empty page
        if (doc.Pages.Count == 0)
        {
            doc.AddPage(options.PageWidth, options.PageHeight);
        }

        // Render print footers on each page
        var totalPages = doc.Pages.Count;
        foreach (var (sheet, startPage, endPage) in sheetPageRanges)
        {
            if (string.IsNullOrEmpty(sheet.OddFooter)) continue;
            var footerY = sheet.FooterMarginPt > 0 ? sheet.FooterMarginPt : 18f;
            var marginL = sheet.MarginLeftPt > 0 ? sheet.MarginLeftPt : options.MarginLeft;
            var marginR = sheet.MarginRightPt > 0 ? sheet.MarginRightPt : options.MarginRight;
            RenderPageFooter(doc.Pages, sheet.OddFooter, startPage, endPage, totalPages, footerY, marginL, marginR);
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

    /// <summary>
    /// Render print footer on pages. Parses XLSX header/footer format codes
    /// (&amp;L, &amp;C, &amp;R sections; &amp;P page number; &amp;N total pages; &amp;nn font size).
    /// </summary>
    private static void RenderPageFooter(IReadOnlyList<PdfPage> pages, string footerFormat,
        int startPage, int endPage, int totalPages, float footerY, float marginL, float marginR)
    {
        // Parse footer into left/center/right sections
        var (left, center, right) = ParseHeaderFooterSections(footerFormat);

        for (var p = startPage; p <= endPage && p < pages.Count; p++)
        {
            var pageNum = p + 1;
            var page = pages[p];
            var pageWidth = page.Width;

            void RenderSection(string section, string align)
            {
                if (string.IsNullOrEmpty(section)) return;
                var fontSize = 6f; // default footer font size
                var text = new System.Text.StringBuilder();
                var i = 0;
                while (i < section.Length)
                {
                    if (section[i] == '&' && i + 1 < section.Length)
                    {
                        var next = section[i + 1];
                        if (next == 'P') { text.Append(pageNum); i += 2; continue; }
                        if (next == 'N') { text.Append(totalPages); i += 2; continue; }
                        if (next == 'A') { text.Append(""); i += 2; continue; } // sheet name - skip
                        if (next == 'D') { text.Append(""); i += 2; continue; } // date - skip
                        if (next == '"')
                        {
                            // Skip font specification: &"FontName,Style"
                            var closeQuote = section.IndexOf('"', i + 2);
                            if (closeQuote > 0) { i = closeQuote + 1; continue; }
                        }
                        if (char.IsDigit(next) && i + 2 < section.Length && char.IsDigit(section[i + 2]))
                        {
                            fontSize = (next - '0') * 10 + (section[i + 2] - '0');
                            i += 3; continue;
                        }
                        if (char.IsDigit(next))
                        {
                            fontSize = next - '0';
                            i += 2; continue;
                        }
                        // Unknown code - skip
                        i += 2; continue;
                    }
                    text.Append(section[i]);
                    i++;
                }

                var lines = text.ToString().Split('\n');
                // Render each line from bottom up (last line at footerY, previous lines above)
                for (var li = lines.Length - 1; li >= 0; li--)
                {
                    var line = lines[li].Trim();
                    if (string.IsNullOrEmpty(line)) continue;
                    var lineY = footerY + (lines.Length - 1 - li) * (fontSize * 1.3f);
                    float x;
                    if (align == "left")
                        x = marginL;
                    else if (align == "right")
                    {
                        var textWidth = (float)MeasureHelveticaWidth(line, fontSize);
                        x = pageWidth - marginR - textWidth;
                    }
                    else // center
                    {
                        var textWidth = (float)MeasureHelveticaWidth(line, fontSize);
                        x = (pageWidth - textWidth) / 2f;
                    }
                    page.AddText(line, x, lineY, fontSize);
                }
            }

            RenderSection(left, "left");
            RenderSection(center, "center");
            RenderSection(right, "right");
        }
    }

    /// <summary>
    /// Parse an XLSX header/footer string into left, center, and right sections.
    /// Sections are delimited by &amp;L, &amp;C, &amp;R markers.
    /// </summary>
    private static (string left, string center, string right) ParseHeaderFooterSections(string format)
    {
        var left = "";
        var center = "";
        var right = "";
        string? current = null;
        var sb = new System.Text.StringBuilder();

        void Flush()
        {
            var text = sb.ToString();
            sb.Clear();
            if (current == "L") left = text;
            else if (current == "C") center = text;
            else if (current == "R") right = text;
        }

        var i = 0;
        while (i < format.Length)
        {
            if (format[i] == '&' && i + 1 < format.Length)
            {
                var next = format[i + 1];
                if (next == 'L' || next == 'C' || next == 'R')
                {
                    Flush();
                    current = next.ToString();
                    i += 2;
                    continue;
                }
            }
            sb.Append(format[i]);
            i++;
        }
        Flush();
        return (left, center, right);
    }

    private static void RenderSheet(PdfDocument doc, ExcelSheet sheet, ConversionOptions options)
    {
        // Skip only if there's truly nothing to render (no rows AND no images).
        if (sheet.Rows.Count == 0 && sheet.Images.Count == 0 && sheet.Charts.Count == 0) return;

        // Apply sheet page setup: paper size, orientation, margins, and print scale
        // Determine base page dimensions from paper size
        var (baseW, baseH) = sheet.PaperSize switch
        {
            // Use precise point sizes converted from millimeters to match office renderers.
            9 => (595.2756f, 841.8898f),   // A4: 210×297mm
            8 => (841.8898f, 1190.5512f),  // A3: 297×420mm
            _ => (options.PageWidth, options.PageHeight), // Default (Letter 612×792)
        };
        if (sheet.IsLandscape)
        {
            (baseW, baseH) = (baseH, baseW);
        }
        // Apply XLSX margins if specified (> 0), otherwise keep defaults
        var mL = sheet.MarginLeftPt > 0 ? sheet.MarginLeftPt : options.MarginLeft;
        var mR = sheet.MarginRightPt > 0 ? sheet.MarginRightPt : options.MarginRight;
        var mT = sheet.MarginTopPt > 0 ? sheet.MarginTopPt : options.MarginTop;
        var mB = sheet.MarginBottomPt > 0 ? sheet.MarginBottomPt : options.MarginBottom;

        // fitToHeight: when fitToPage is active and fitToHeight is explicitly set
        // in the XML (> 0), recalculate printScale so all rows fit within the
        // target number of pages.  Use print area bounds when available so the
        // calculation matches the rows that will actually be rendered.
        // When fitToPage width scaling will also compress columns (and therefore
        // row heights proportionally), factor that into the calculation to avoid
        // double-compression.
        var scaleCellFonts = false;
        if (sheet.FitToPage && sheet.FitToHeight > 0 && sheet.PrintScale > 0)
        {
            var usableH = baseH - mT - mB;
            var usableW = baseW - mL - mR;
            var targetH = usableH * sheet.FitToHeight;
            var defRH = sheet.DefaultRowHeight > 0 ? sheet.DefaultRowHeight : options.FontSize * options.LineSpacing;
            var rawTotal = 0f;
            var startRow = sheet.PrintArea.HasValue ? sheet.PrintArea.Value.StartRow : 0;
            var endRow = sheet.PrintArea.HasValue ? Math.Min(sheet.PrintArea.Value.EndRow, sheet.Rows.Count - 1) : sheet.Rows.Count - 1;
            for (var r = startRow; r <= endRow; r++)
                rawTotal += sheet.RowHeights.TryGetValue(r, out var rh) ? rh : defRH;  // hidden rows have rh=0

            // Account for print title rows repeated on pages after manual row breaks.
            // Each break creates a new page that repeats the title rows at the top.
            if (sheet.RowBreaks.Count > 0 && sheet.PrintTitleRows.HasValue)
            {
                var titleStart = sheet.PrintTitleRows.Value.StartRow;
                var titleEnd = sheet.PrintTitleRows.Value.EndRow;
                var titleH = 0f;
                for (var tr = titleStart; tr <= titleEnd; tr++)
                    titleH += sheet.RowHeights.TryGetValue(tr, out var th) ? th : defRH;
                rawTotal += titleH * sheet.RowBreaks.Count;
            }

            // Estimate the fitToPage width compression that will also shrink rows.
            var printScaleF = sheet.PrintScale / 100f;
            var estColTotal = EstimateColumnWidthTotal(sheet, options) * printScaleF;
            var estFitToPageScale = estColTotal > usableW ? usableW / estColTotal : 1f;
            // Effective row scale = PrintScale × fitToPage width-compression
            var effectiveScale = printScaleF * estFitToPageScale;

            var requiredScale = rawTotal > 0 ? targetH / rawTotal : 1f;
            if (requiredScale < effectiveScale)
            {
                var combined = (int)Math.Max(10, Math.Floor(requiredScale * 100));
                sheet = new ExcelSheet(sheet.Name, sheet.Rows,
                    images: sheet.Images.Count > 0 ? sheet.Images : null,
                    columnWidths: sheet.ColumnWidths,
                    defaultColumnWidth: sheet.DefaultColumnWidth,
                    charts: sheet.Charts.Count > 0 ? sheet.Charts : null,
                    shapes: sheet.Shapes.Count > 0 ? sheet.Shapes : null,
                    mergedCells: sheet.MergedCells,
                    rowHeights: sheet.RowHeights,
                    defaultRowHeight: sheet.DefaultRowHeight,
                    customHeightRows: sheet.CustomHeightRows,
                    isLandscape: sheet.IsLandscape,
                    printScale: combined,
                    paperSize: sheet.PaperSize,
                    marginLeftPt: sheet.MarginLeftPt,
                    marginRightPt: sheet.MarginRightPt,
                    marginTopPt: sheet.MarginTopPt,
                    marginBottomPt: sheet.MarginBottomPt,
                    fitToPage: sheet.FitToPage,
                    fitToWidth: sheet.FitToWidth,
                    fitToHeight: sheet.FitToHeight,
                    horizontalCentered: sheet.HorizontalCentered,
                    printArea: sheet.PrintArea,
                    printTitleRows: sheet.PrintTitleRows,
                    rowBreaks: sheet.RowBreaks,
                    oddFooter: sheet.OddFooter, footerMarginPt: sheet.FooterMarginPt,
                    maxDigitWidthPx: sheet.MaxDigitWidthPx);
                scaleCellFonts = true;
            }

            // Per-segment check: when manual row breaks + print title rows exist,
            // verify each page segment (including repeated title rows) fits within
            // a single page.  If any segment overflows, compress the scale further.
            if (sheet.RowBreaks.Count > 0 && sheet.PrintTitleRows.HasValue)
            {
                var segPrintScaleF = sheet.PrintScale / 100f;
                var segColTotal = EstimateColumnWidthTotal(sheet, options) * segPrintScaleF;
                var segFitToPageScale = segColTotal > usableW ? usableW / segColTotal : 1f;
                var segScale = segPrintScaleF * segFitToPageScale;
                var titleStart2 = sheet.PrintTitleRows.Value.StartRow;
                var titleEnd2 = sheet.PrintTitleRows.Value.EndRow;
                var titleH2 = 0f;
                for (var tr = titleStart2; tr <= titleEnd2; tr++)
                    titleH2 += sheet.RowHeights.TryGetValue(tr, out var th) ? th : defRH;
                var breakRows = sheet.RowBreaks.Where(b => b >= startRow && b <= endRow).OrderBy(b => b).ToList();
                var segStarts = new List<int> { startRow };
                segStarts.AddRange(breakRows);
                for (var s = 0; s < segStarts.Count; s++)
                {
                    var segStart = segStarts[s];
                    var segEnd = s + 1 < segStarts.Count ? segStarts[s + 1] - 1 : endRow;
                    var segH = 0f;
                    for (var r = segStart; r <= segEnd; r++)
                        segH += sheet.RowHeights.TryGetValue(r, out var srh) ? srh : defRH;
                    if (s > 0) segH += titleH2;
                    if (segH * segScale > usableH)
                    {
                        var newScale = usableH / segH;
                        var combined2 = (int)Math.Max(10, Math.Floor(newScale * 100));
                        if (combined2 < sheet.PrintScale)
                        {
                            sheet = new ExcelSheet(sheet.Name, sheet.Rows,
                                images: sheet.Images.Count > 0 ? sheet.Images : null,
                                columnWidths: sheet.ColumnWidths, defaultColumnWidth: sheet.DefaultColumnWidth,
                                charts: sheet.Charts.Count > 0 ? sheet.Charts : null,
                                shapes: sheet.Shapes.Count > 0 ? sheet.Shapes : null,
                                mergedCells: sheet.MergedCells, rowHeights: sheet.RowHeights,
                                defaultRowHeight: sheet.DefaultRowHeight, customHeightRows: sheet.CustomHeightRows,
                                isLandscape: sheet.IsLandscape, printScale: combined2, paperSize: sheet.PaperSize,
                                marginLeftPt: sheet.MarginLeftPt, marginRightPt: sheet.MarginRightPt,
                                marginTopPt: sheet.MarginTopPt, marginBottomPt: sheet.MarginBottomPt,
                                fitToPage: sheet.FitToPage, fitToWidth: sheet.FitToWidth, fitToHeight: sheet.FitToHeight,
                                horizontalCentered: sheet.HorizontalCentered, printArea: sheet.PrintArea,
                                printTitleRows: sheet.PrintTitleRows, rowBreaks: sheet.RowBreaks,
                                oddFooter: sheet.OddFooter, footerMarginPt: sheet.FooterMarginPt,
                                maxDigitWidthPx: sheet.MaxDigitWidthPx);
                            scaleCellFonts = true;
                        }
                        break;
                    }
                }
            }
        }

        // When fitToPage is active for width fitting only (fitToHeight=0),
        // recalculate the PrintScale from actual column widths. LibreOffice
        // ignores the stored scale attribute and computes the minimum scale
        // needed to fit all columns within one page width.
        if (sheet.FitToPage && sheet.FitToWidth >= 1 && sheet.FitToHeight == 0
            && sheet.PrintScale > 0 && sheet.PrintScale < 100)
        {
            var usableW = baseW - mL - mR;
            var estTotal = EstimateColumnWidthTotal(sheet, options);
            if (estTotal > usableW && estTotal > 0)
            {
                var fitScaleF = usableW / estTotal;
                var fitPct = (int)Math.Max(10, Math.Floor(fitScaleF * 100));

                // Check if the width-based scale causes rows to barely overflow
                // one page height.  When the overflow is small (< 5%), reduce
                // the scale so content fits on a single page.  Larger overflows
                // are allowed to spill to additional pages naturally.
                {
                    var usableH = baseH - mT - mB;
                    var defRH = sheet.DefaultRowHeight > 0 ? sheet.DefaultRowHeight : options.FontSize * options.LineSpacing;
                    var startRow = sheet.PrintArea.HasValue ? sheet.PrintArea.Value.StartRow : 0;
                    var endRow = sheet.PrintArea.HasValue ? Math.Min(sheet.PrintArea.Value.EndRow, sheet.Rows.Count - 1) : sheet.Rows.Count - 1;
                    var rawTotal = 0f;
                    for (var r = startRow; r <= endRow; r++)
                        rawTotal += sheet.RowHeights.TryGetValue(r, out var rh) ? rh : defRH;
                    var scaledH = rawTotal * fitScaleF;
                    if (scaledH > usableH && scaledH < usableH * 1.05f)
                    {
                        // Small overflow: use height-based scale instead
                        var heightScale = usableH / rawTotal;
                        fitScaleF = heightScale;
                        fitPct = (int)Math.Max(10, Math.Floor(fitScaleF * 100));
                    }
                }

                sheet.PrintScale = fitPct;
                // Store precise float scale to avoid integer rounding loss
                sheet.EffectivePrintScaleF = fitScaleF;
            }
            else if (estTotal > 0 && sheet.PrintScale < 100)
            {
                // Columns already fit at 100% — no scaling needed
                sheet.PrintScale = 100;
                sheet.EffectivePrintScaleF = 1f;
            }
        }

        // For fitToPage sheets with a reduced printScale: produce larger pages at
        // natural content size instead of standard-size pages with scaled-down
        // content.  This matches how Excel/Office exports PDFs — the page
        // dimensions grow to accommodate the content so fonts stay at their
        // original point size and text renders crisply.
        // Margins are also scaled so the content region on the enlarged page
        // occupies the same proportional area as on the standard page.
        var pageScaleUp = 1f;
        if (sheet.FitToPage && sheet.PrintScale != 100 && sheet.PrintScale > 0)
        {
            pageScaleUp = 100f / sheet.PrintScale;
            sheet.PrintScale = 100; // content stays at natural size
            mL *= pageScaleUp;
            mR *= pageScaleUp;
            mT *= pageScaleUp;
            mB *= pageScaleUp;
        }

        var fontSize = options.FontSize;
        var colPadding = options.ColumnPadding;
        if (sheet.PrintScale != 100 && sheet.PrintScale > 0)
        {
            var scale = sheet.PrintScale / 100f;
            fontSize *= scale;
            colPadding *= scale;
        }
        options = new ConversionOptions
        {
            FontSize = fontSize,
            MarginLeft = mL,
            MarginTop = mT,
            MarginRight = mR,
            MarginBottom = mB,
            ColumnPadding = colPadding,
            LineSpacing = options.LineSpacing,
            PageWidth = baseW * pageScaleUp,
            PageHeight = baseH * pageScaleUp,
            IncludeSheetName = options.IncludeSheetName,
            ScaleCellFonts = scaleCellFonts,
        };

        // Apply print area: limit rows and columns to the defined range
        if (sheet.PrintArea.HasValue)
        {
            var pa = sheet.PrintArea.Value;
            // Trim rows to print area range
            var trimmedRows = new List<List<ExcelCell>>();
            for (var r = pa.StartRow; r <= pa.EndRow && r < sheet.Rows.Count; r++)
            {
                var srcRow = sheet.Rows[r];
                var newRow = new List<ExcelCell>();
                for (var c = pa.StartCol; c <= pa.EndCol; c++)
                {
                    newRow.Add(c < srcRow.Count ? srcRow[c] : new ExcelCell("", null, null));
                }
                trimmedRows.Add(newRow);
            }
            // Build trimmed column widths
            var trimmedColWidths = new Dictionary<int, float>();
            for (var c = pa.StartCol; c <= pa.EndCol; c++)
            {
                if (sheet.ColumnWidths.TryGetValue(c, out var w))
                    trimmedColWidths[c - pa.StartCol] = w;
            }
            // Build trimmed row heights
            var trimmedRowHeights = new Dictionary<int, float>();
            for (var r = pa.StartRow; r <= pa.EndRow; r++)
            {
                if (sheet.RowHeights.TryGetValue(r, out var h))
                    trimmedRowHeights[r - pa.StartRow] = h;
            }
            // Build trimmed merged cells
            var trimmedMerged = new List<(int, int, int, int)>();
            foreach (var mc in sheet.MergedCells)
            {
                if (mc.StartRow >= pa.StartRow && mc.EndRow <= pa.EndRow &&
                    mc.StartCol >= pa.StartCol && mc.EndCol <= pa.EndCol)
                {
                    trimmedMerged.Add((mc.StartRow - pa.StartRow, mc.StartCol - pa.StartCol,
                                       mc.EndRow - pa.StartRow, mc.EndCol - pa.StartCol));
                }
            }
            // Build trimmed images: keep images within print area and adjust anchors
            var trimmedImages = new List<ExcelEmbeddedImage>();
            var paCols = pa.EndCol - pa.StartCol + 1;
            var paRows = pa.EndRow - pa.StartRow + 1;
            foreach (var img in sheet.Images)
            {
                if (img.AnchorCol >= pa.StartCol && img.AnchorCol <= pa.EndCol &&
                    img.AnchorRow >= pa.StartRow && img.AnchorRow <= pa.EndRow)
                {
                    var newAnchorCol = img.AnchorCol - pa.StartCol;
                    var newAnchorRow = img.AnchorRow - pa.StartRow;
                    trimmedImages.Add(img with
                    {
                        AnchorRow = newAnchorRow,
                        AnchorCol = newAnchorCol,
                        SpanCols = Math.Min(img.SpanCols, paCols - newAnchorCol),
                        SpanRows = Math.Min(img.SpanRows, paRows - newAnchorRow),
                    });
                }
            }
            // Build trimmed charts: keep charts within print area and adjust anchors
            var trimmedCharts = new List<ExcelChartInfo>();
            foreach (var ch in sheet.Charts)
            {
                if (ch.AnchorCol >= pa.StartCol && ch.AnchorCol <= pa.EndCol &&
                    ch.AnchorRow >= pa.StartRow && ch.AnchorRow <= pa.EndRow)
                {
                    trimmedCharts.Add(ch with
                    {
                        AnchorRow = ch.AnchorRow - pa.StartRow,
                        AnchorCol = ch.AnchorCol - pa.StartCol
                    });
                }
            }
            // Build trimmed row breaks (re-index to print area offsets)
            var trimmedRowBreaks = new HashSet<int>();
            foreach (var rb in sheet.RowBreaks)
            {
                if (rb >= pa.StartRow && rb <= pa.EndRow)
                    trimmedRowBreaks.Add(rb - pa.StartRow);
            }
            // Re-index print title rows
            (int StartRow, int EndRow)? trimmedPrintTitleRows = null;
            if (sheet.PrintTitleRows.HasValue)
            {
                var pt = sheet.PrintTitleRows.Value;
                // Only include if the title rows overlap with the print area
                var tStart = Math.Max(pt.StartRow, pa.StartRow);
                var tEnd = Math.Min(pt.EndRow, pa.EndRow);
                if (tStart <= tEnd)
                    trimmedPrintTitleRows = (tStart - pa.StartRow, tEnd - pa.StartRow);
            }
            // Re-index custom height rows
            var trimmedCustomHeightRows = new HashSet<int>();
            foreach (var chr in sheet.CustomHeightRows)
            {
                if (chr >= pa.StartRow && chr <= pa.EndRow)
                    trimmedCustomHeightRows.Add(chr - pa.StartRow);
            }
            sheet = new ExcelSheet(sheet.Name, trimmedRows,
                images: trimmedImages.Count > 0 ? trimmedImages : null,
                columnWidths: trimmedColWidths,
                defaultColumnWidth: sheet.DefaultColumnWidth,
                charts: trimmedCharts.Count > 0 ? trimmedCharts : null,
                shapes: sheet.Shapes.Count > 0 ? sheet.Shapes : null,
                mergedCells: trimmedMerged,
                rowHeights: trimmedRowHeights,
                defaultRowHeight: sheet.DefaultRowHeight,
                customHeightRows: trimmedCustomHeightRows,
                isLandscape: sheet.IsLandscape,
                printScale: sheet.PrintScale,
                paperSize: sheet.PaperSize,
                marginLeftPt: sheet.MarginLeftPt,
                marginRightPt: sheet.MarginRightPt,
                marginTopPt: sheet.MarginTopPt,
                marginBottomPt: sheet.MarginBottomPt,
                fitToPage: sheet.FitToPage,
                fitToWidth: sheet.FitToWidth,
                fitToHeight: sheet.FitToHeight,
                horizontalCentered: sheet.HorizontalCentered,
                rowBreaks: trimmedRowBreaks,
                printTitleRows: trimmedPrintTitleRows,
                oddFooter: sheet.OddFooter, footerMarginPt: sheet.FooterMarginPt,
                maxDigitWidthPx: sheet.MaxDigitWidthPx);
        }

        var maxCols = sheet.Rows.Count > 0 ? sheet.Rows.Max(r => r.Count) : 0;
        var maxColsFromRows = maxCols;

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

        // Trim trailing columns that have no text content across any row.
        // Spreadsheets often include style-only cells (e.g. background, borders)
        // in columns beyond the data range — these inflate the column count and
        // cause excessive column-group page splits.
        // Preserve the minimum column range needed for image/chart anchors.
        var minColsForAnchors = 0;
        if (sheet.Images.Count > 0)
            minColsForAnchors = Math.Max(minColsForAnchors, sheet.Images.Max(img => img.AnchorCol + Math.Max(1, img.SpanCols)));
        if (sheet.Charts.Count > 0)
            minColsForAnchors = Math.Max(minColsForAnchors, sheet.Charts.Max(c => c.AnchorCol + 1));

        while (maxCols > minColsForAnchors)
        {
            var col = maxCols - 1;
            var hasContent = false;
            foreach (var row in sheet.Rows)
            {
                if (col < row.Count && CellHasContentOrStyle(row[col]))
                {
                    hasContent = true;
                    break;
                }
            }
            if (hasContent) break;
            maxCols--;
        }

        if (maxCols == 0)
        {
            return;
        }

        // Trim trailing rows that have no text content or styling.
        // Preserve rows needed for image/chart anchors.
        var minRowsForAnchors = 0;
        if (sheet.Images.Count > 0)
            minRowsForAnchors = Math.Max(minRowsForAnchors, sheet.Images.Max(img => img.AnchorRow + Math.Max(1, img.SpanRows)));
        if (sheet.Charts.Count > 0)
            minRowsForAnchors = Math.Max(minRowsForAnchors, sheet.Charts.Max(c => c.AnchorRow + 1));

        while (sheet.Rows.Count > minRowsForAnchors)
        {
            var lastRow = sheet.Rows[^1];
            var rowHasContent = false;
            for (var ci = 0; ci < Math.Min(lastRow.Count, maxCols); ci++)
            {
                if (CellHasContentOrStyle(lastRow[ci]))
                {
                    rowHasContent = true;
                    break;
                }
            }
            if (rowHasContent) break;
            sheet.Rows.RemoveAt(sheet.Rows.Count - 1);
        }

        var pageWidth = options.PageWidth;
        var pageHeight = options.PageHeight;
        var usableWidth = pageWidth - options.MarginLeft - options.MarginRight;
        var avgCharWidth = options.FontSize * 0.47f;

        // Determine column widths first to decide on layout strategy
        var hasExplicitColWidths = sheet.ColumnWidths.Count > 0 || sheet.DefaultColumnWidth > 0f;
        var columnPadding = options.ColumnPadding;
        if (hasExplicitColWidths || (sheet.FitToPage && sheet.FitToWidth >= 1))
        {
            // When the spreadsheet defines explicit column widths, or fitToPage is
            // active, use zero inter-column padding to match Excel/LibreOffice layout
            // (columns are adjacent, separated only by cell borders/padding).
            columnPadding = 0f;
        }
        else if (maxCols > 6)
        {
            columnPadding = options.ColumnPadding * 6f / maxCols;
        }

        // Calculate natural (unscaled) column widths to decide on grouping
        var naturalWidths = CalculateNaturalColumnWidths(sheet, maxCols, usableWidth, options);

        // Apply print scale to column widths so more columns fit per page.
        // Font size is already scaled in options, but Excel explicit column widths
        // from CharUnitsToPoints are at full size and need scaling too.
        if (sheet.PrintScale != 100 && sheet.PrintScale > 0)
        {
            var scale = sheet.PrintScale / 100f;
            for (var i = 0; i < naturalWidths.Length; i++)
                naturalWidths[i] *= scale;
        }

        var totalNatural = naturalWidths.Sum() + columnPadding * (maxCols - 1);

        // fitToPage: auto-scale column widths to fit in one page width.
        // The same scale factor is applied to row heights so rows are proportionally
        // smaller and pagination matches the real spreadsheet output.
        var fitToPageScale = 1f;

        if (sheet.FitToPage && totalNatural > usableWidth && maxCols > 1)
        {
            var fitScale = usableWidth / totalNatural;
            for (var i = 0; i < naturalWidths.Length; i++)
                naturalWidths[i] *= fitScale;
            columnPadding *= fitScale;
            totalNatural = usableWidth;
            fitToPageScale = fitScale;
        }

        if (naturalWidths.Sum() > usableWidth && maxCols > 1)
        {
            // Columns don't fit — split into groups that fit on a page each
            RenderSheetColumnGroups(doc, sheet, options, pageWidth, pageHeight, usableWidth, columnPadding, avgCharWidth, naturalWidths);
        }
        else
        {
            // Single group — scale to fit if needed
            var colWidths = ScaleColumnWidths(naturalWidths, usableWidth, columnPadding, avgCharWidth);

            // horizontalCentered: shift content right so it is centered in usable width
            if (sheet.HorizontalCentered)
            {
                var contentWidth = colWidths.Sum() + columnPadding * (maxCols - 1);
                var centerOffset = (usableWidth - contentWidth) / 2f;
                if (centerOffset > 0)
                    options.MarginLeft += centerOffset;
            }

            RenderSheetRows(doc, sheet, options, pageWidth, pageHeight, Enumerable.Range(0, maxCols).ToArray(), columnPadding, colWidths, avgCharWidth, fitToPageScale);
        }

        // Note: trailing empty page logic disabled — it was previously intended to
        // match LibreOffice behavior but caused extra pages in multi-sheet workbooks.
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
            // Skip hidden (0-width) columns entirely
            if (naturalWidths[col] <= 0f) continue;

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
            // Skip column groups where no row has any text in these columns.
            // This avoids generating blank pages for style-only column ranges.
            var groupHasContent = false;
            foreach (var row in sheet.Rows)
            {
                foreach (var col in group)
                {
                    if (col < row.Count && !string.IsNullOrEmpty(row[col].Text))
                    {
                        groupHasContent = true;
                        break;
                    }
                }
                if (groupHasContent) break;
            }
            if (!groupHasContent) continue;

            var pageCountBefore = doc.Pages.Count;

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

            // Remove trailing empty pages created by this column group.
            // When most rows have no text in this group's columns, the vertical
            // pagination produces pages with only background fills and borders.
            while (doc.Pages.Count > pageCountBefore)
            {
                var last = doc.Pages[doc.Pages.Count - 1];
                if (last.TextBlocks.Count == 0 &&
                    last.ImageBlocks.Count == 0 &&
                    last.RectBlocks.Count == 0 &&
                    last.LineBlocks.Count == 0)
                    doc.RemoveLastPage();
                else
                    break;
            }
        }
    }

    /// <summary>
    /// Render rows for a specific set of columns.
    /// </summary>
    private static void RenderSheetRows(PdfDocument doc, ExcelSheet sheet, ConversionOptions options,
        float pageWidth, float pageHeight, int[] columns, float columnPadding, float[] colWidths, float avgCharWidth,
        float fitToPageScale = 1f)
    {
        // Print scale factor for cell-level font sizes (column widths are already print-scaled by caller)
        var printScaleFactor = (sheet.PrintScale != 100 && sheet.PrintScale > 0) ? sheet.PrintScale / 100f : 1f;
        var borderScaleFactor = printScaleFactor * (fitToPageScale < 1f ? fitToPageScale : 1f);

        // Use the sheet's default row height if available, otherwise fall back to font-based calculation
        var defaultLineHeight = sheet.DefaultRowHeight > 0 ? sheet.DefaultRowHeight : options.FontSize * options.LineSpacing;
        // Apply print scale to row heights (like column widths, these define content size)
        if (printScaleFactor != 1f)
            defaultLineHeight *= printScaleFactor;
        // Apply fitToPage additional scale so rows shrink proportionally to columns
        if (fitToPageScale < 1f)
            defaultLineHeight *= fitToPageScale;
        var lineHeight = defaultLineHeight;
        PdfPage? currentPage = null;
        var currentY = 0f;
        var accumulatedOverflowHeight = 0f;

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
        // Also track interior cells of merge ranges so we can skip their fill/border.
        var mergeEndCol = new Dictionary<(int, int), int>(); // (row, col) → endCol
        var mergeInterior = new HashSet<(int, int)>(); // cells inside merge (not start col)
        foreach (var (sr, sc, er, ec) in sheet.MergedCells)
        {
            for (var r = sr; r <= er; r++)
            {
                mergeEndCol[(r, sc)] = ec;
                for (var c = sc + 1; c <= ec; c++)
                    mergeInterior.Add((r, c));
            }
        }

        // Print title rows support: detect the row range to repeat on each page
        var printTitleStart = -1;
        var printTitleEnd = -1;
        if (sheet.PrintTitleRows.HasValue)
        {
            printTitleStart = sheet.PrintTitleRows.Value.StartRow;
            printTitleEnd = sheet.PrintTitleRows.Value.EndRow;
        }

        // Helper: render print title rows at the current page position.
        // Returns the total height consumed by the title rows.
        float RenderPrintTitleRows()
        {
            if (printTitleStart < 0 || currentPage == null) return 0f;
            var totalHeight = 0f;
            var titleRowTopY = new Dictionary<int, float>();
            var titleRowTopYPos = currentY;
            for (var titleRowIdx = printTitleStart; titleRowIdx <= printTitleEnd && titleRowIdx < sheet.Rows.Count; titleRowIdx++)
            {
                titleRowTopYPos = currentY;
                var titleRow = sheet.Rows[titleRowIdx];

                // Calculate row height
                var hasTitleExplicitH = sheet.RowHeights.TryGetValue(titleRowIdx, out var titleExplicitH);
                if (hasTitleExplicitH && printScaleFactor != 1f) titleExplicitH *= printScaleFactor;
                if (hasTitleExplicitH && fitToPageScale < 1f) titleExplicitH *= fitToPageScale;

                // Compute cell lines and max lines
                var titleMaxLines = 1;
                var titleCellLines = new string[columns.Length][];
                var titleClipWidths = new float?[columns.Length];
                for (var i = 0; i < columns.Length; i++)
                {
                    var col = columns[i];
                    if (col < titleRow.Count && !string.IsNullOrEmpty(titleRow[col].Text))
                    {
                        var cellText = titleRow[col].Text;
                        var cellFs = titleRow[col].FontSize * printScaleFactor;
                        if (fitToPageScale < 1f) cellFs *= fitToPageScale;

                        var isMerged = mergeEndCol.TryGetValue((titleRowIdx, col), out var endCol);
                        var effectiveW = colWidths[i];
                        if (isMerged)
                            for (var mc = i + 1; mc < columns.Length && columns[mc] <= endCol; mc++)
                                effectiveW += colWidths[mc] + columnPadding;

                        var fitChars = FittingChars(cellText, effectiveW, cellFs);
                        if (titleRow[col].WrapText && fitChars < cellText.Length)
                        {
                            titleCellLines[i] = WrapCellText(cellText, effectiveW, cellFs);
                            titleClipWidths[i] = effectiveW;
                        }
                        else
                        {
                            // Check if next column has content (consistent with regular row logic)
                            var checkStart = isMerged ? endCol + 1 : col + 1;
                            var nextHasContent = false;
                            for (var mi = i + 1; mi < columns.Length; mi++)
                            {
                                if (columns[mi] >= checkStart)
                                {
                                    var nc = columns[mi];
                                    if (nc < titleRow.Count && !string.IsNullOrEmpty(titleRow[nc].Text))
                                        nextHasContent = true;
                                    break;
                                }
                            }
                            var shouldClip = isMerged || (i < columns.Length - 1 && nextHasContent);
                            if (shouldClip)
                            {
                                titleClipWidths[i] = effectiveW;
                            }
                            if (shouldClip && cellText.Length > fitChars)
                                titleCellLines[i] = new[] { cellText[..fitChars] };
                            else
                                titleCellLines[i] = new[] { cellText };
                        }
                        titleMaxLines = Math.Max(titleMaxLines, titleCellLines[i].Length);
                    }
                    else
                    {
                        titleCellLines[i] = Array.Empty<string>();
                    }
                }

                // Calculate row height
                var maxTitleFontSize = options.FontSize;
                for (var i = 0; i < columns.Length; i++)
                {
                    var col = columns[i];
                    if (col < titleRow.Count)
                    {
                        var fs = (sheet.FitToPage || options.ScaleCellFonts) ? titleRow[col].FontSize * printScaleFactor : titleRow[col].FontSize;
                        if (fs > maxTitleFontSize) maxTitleFontSize = fs;
                    }
                }
                var autoTitleH = maxTitleFontSize > options.FontSize
                    ? Math.Max(maxTitleFontSize * 1.3f, lineHeight) : lineHeight;
                var titleContentH = autoTitleH * titleMaxLines;
                var isTitleCustomHeight = hasTitleExplicitH && sheet.CustomHeightRows.Contains(titleRowIdx);
                var titleRowH = hasTitleExplicitH
                    ? (isTitleCustomHeight ? titleExplicitH : Math.Max(titleExplicitH, titleContentH))
                    : titleContentH;

                // If the title row uses one uniform fill and no borders, render one full-width
                // background rectangle to avoid seams between adjacent cells.
                PdfColor? titleUniformFill = null;
                var titleHasUniformFill = true;
                for (var i = 0; i < columns.Length; i++)
                {
                    var col = columns[i];
                    var c = col < titleRow.Count ? titleRow[col] : null;
                    if (c?.FillColor is not { } fc || c.Border != null || mergeInterior.Contains((titleRowIdx, col)))
                    {
                        titleHasUniformFill = false;
                        break;
                    }

                    if (titleUniformFill == null)
                        titleUniformFill = fc;
                    else if (titleUniformFill.Value != fc)
                    {
                        titleHasUniformFill = false;
                        break;
                    }
                }

                if (titleHasUniformFill && titleUniformFill != null)
                {
                    var rowWidth = colWidths.Sum() + columnPadding * (columns.Length - 1);
                    currentPage!.AddRectangle(options.MarginLeft, currentY - titleRowH, rowWidth, titleRowH, titleUniformFill.Value);
                }

                // Render cells
                var x = options.MarginLeft;
                for (var i = 0; i < columns.Length; i++)
                {
                    var col = columns[i];
                    var cell = col < titleRow.Count ? titleRow[col] : null;
                    var fillColor = cell?.FillColor;
                    var cellFs = cell != null ? cell.FontSize * printScaleFactor : options.FontSize;
                    if (fitToPageScale < 1f) cellFs *= fitToPageScale;
                    var alignment = cell?.Alignment ?? "left";
                    var vertAlign = cell?.VerticalAlignment ?? "bottom";

                    var cellWidth = colWidths[i];
                    if (mergeEndCol.TryGetValue((titleRowIdx, col), out var mergeEnd))
                        for (var mc = i + 1; mc < columns.Length && columns[mc] <= mergeEnd; mc++)
                            cellWidth += colWidths[mc] + columnPadding;

                    // Fill — skip for cells inside a merge range.
                    // Extend into column padding only when the next visible cell shares
                    // the same fill and neither side has explicit borders.
                    if (fillColor != null && !titleHasUniformFill && !mergeInterior.Contains((titleRowIdx, col)))
                    {
                        var fillWidth = cellWidth;
                        if (i + 1 < columns.Length && columns[i + 1] == col + 1)
                        {
                            var rightCol = columns[i + 1];
                            var rightCell = rightCol < titleRow.Count ? titleRow[rightCol] : null;
                            var hasSameFillRight = rightCell?.FillColor is { } rightFill
                                && rightFill == fillColor.Value
                                && cell?.Border == null
                                && rightCell.Border == null
                                && !mergeInterior.Contains((titleRowIdx, rightCol))
                                && !mergeEndCol.ContainsKey((titleRowIdx, rightCol));
                            if (hasSameFillRight)
                                fillWidth += columnPadding;
                        }
                        currentPage!.AddRectangle(x, currentY - titleRowH, fillWidth, titleRowH, fillColor);
                    }

                    // Text
                    var descent = options.FontSize * 0.31f;
                    float cellY;
                    if (vertAlign == "top")
                        cellY = currentY - cellFs;
                    else if (vertAlign == "center")
                    {
                        // LibreOffice keeps short, single-line center-aligned text
                        // slightly closer to the row baseline than strict geometric centering.
                        if (titleCellLines[i].Length <= 1)
                        {
                            var slack = Math.Max(0f, titleRowH - cellFs);
                            cellY = currentY - titleRowH + descent + slack * 0.35f;
                        }
                        else
                        {
                            var textBlock = cellFs + lineHeight * (titleCellLines[i].Length - 1);
                            cellY = currentY - (titleRowH - textBlock) / 2f - cellFs + descent;
                        }
                    }
                    else
                        cellY = currentY - titleRowH + descent + lineHeight * (titleCellLines[i].Length - 1);

                    // Render accounting prefix left-aligned in title rows.
                    if (cell?.AccountingPrefix != null && titleCellLines[i].Length > 0 && !string.IsNullOrEmpty(titleCellLines[i][0]))
                    {
                        currentPage!.AddText(cell.AccountingPrefix, x, cellY, cellFs, cell?.Color,
                            maxWidth: titleClipWidths[i],
                            bold: ShouldUsePdfBold(cell?.Bold ?? false, cellFs, cell?.FontName),
                            underline: false,
                            preferredFontName: cell?.FontName);
                    }

                    var titleIsBold = ShouldUsePdfBold(cell?.Bold ?? false, cellFs, cell?.FontName);
                    var titleIndentOff = (cell?.Indent ?? 0) * avgCharWidth;
                    var titleAcctPrefixW = cell?.AccountingPrefix != null
                        ? (float)MeasureHelveticaWidth(cell.AccountingPrefix, cellFs, bold: titleIsBold)
                        : 0f;
                    for (var lineIdx = 0; lineIdx < titleCellLines[i].Length; lineIdx++)
                    {
                        if (!string.IsNullOrEmpty(titleCellLines[i][lineIdx]))
                        {
                            var textX = x + titleIndentOff;
                            var lineMaxWidth = titleClipWidths[i];
                            if (alignment == "right" || titleAcctPrefixW > 0)
                            {
                                var tw = (float)MeasureHelveticaWidth(titleCellLines[i][lineIdx], cellFs, bold: titleIsBold);
                                textX = x + cellWidth - tw - titleIndentOff;
                                if (titleAcctPrefixW > 0 && textX < x + titleAcctPrefixW)
                                {
                                    textX = x + titleAcctPrefixW;
                                    lineMaxWidth = cellWidth - titleAcctPrefixW;
                                }
                            }
                            else if (alignment == "center")
                            {
                                var tw = (float)MeasureHelveticaWidth(titleCellLines[i][lineIdx], cellFs, bold: titleIsBold);
                                textX = x + (cellWidth - tw) / 2f;
                            }
                            currentPage!.AddText(titleCellLines[i][lineIdx], textX, cellY, cellFs, cell?.Color,
                                maxWidth: lineMaxWidth,
                                bold: titleIsBold,
                                underline: cell?.Underline ?? false,
                                preferredFontName: cell?.FontName);
                        }
                        cellY -= lineHeight;
                    }

                    x += colWidths[i] + columnPadding;
                }

                currentY -= titleRowH;
                totalHeight += titleRowH;
                // Track row positions for image placement within title rows
                titleRowTopY[titleRowIdx] = titleRowTopYPos;
                titleRowTopYPos = currentY;
            }

            // Render images anchored within print title rows
            const float EmuToPtTitle = 1f / 12700f;
            foreach (var img in sheet.Images)
            {
                if (img.AnchorRow < printTitleStart || img.AnchorRow > printTitleEnd) continue;
                var colGroupStart = columns[0];
                var colGroupEnd = columns[^1];
                if (img.AnchorCol < colGroupStart || img.AnchorCol > colGroupEnd) continue;

                if (!titleRowTopY.TryGetValue(img.AnchorRow, out var imgTopY)) continue;
                var colGroupIdx = Array.IndexOf(columns, img.AnchorCol);
                if (colGroupIdx < 0) colGroupIdx = 0;
                var imgX = colXStarts[colGroupIdx];

                // Apply sub-cell column offset
                var fromColOffPt = Math.Min(img.FromColOffEmu * EmuToPtTitle, colWidths[colGroupIdx]);
                imgX += fromColOffPt;
                // Apply sub-cell row offset
                imgTopY -= img.FromRowOffEmu * EmuToPtTitle;

                float imgW, imgH;
                if (img.WidthEmu > 0 && img.HeightEmu > 0)
                {
                    imgW = img.WidthEmu * EmuToPtTitle;
                    imgH = img.HeightEmu * EmuToPtTitle;
                }
                else
                {
                    imgW = Math.Max(0, colWidths[colGroupIdx] - fromColOffPt);
                    for (var ci = colGroupIdx + 1; ci < Math.Min(colGroupIdx + img.SpanCols, columns.Length); ci++)
                        imgW += colWidths[ci] + columnPadding;
                    var toColGroupIdx = colGroupIdx + img.SpanCols;
                    if (toColGroupIdx < columns.Length && img.ToColOffEmu > 0)
                    {
                        var toColOffPt = Math.Min(img.ToColOffEmu * EmuToPtTitle, colWidths[toColGroupIdx]);
                        imgW += toColOffPt + columnPadding;
                    }
                    imgW = Math.Max(imgW, 1f);

                    // Height: sum actual row heights for the spanned rows
                    imgH = 0;
                    var tPrintScale = (sheet.PrintScale > 0 && sheet.PrintScale != 100) ? sheet.PrintScale / 100f : 1f;
                    var tRowScale = tPrintScale * fitToPageScale;
                    for (var ri = img.AnchorRow; ri < img.AnchorRow + img.SpanRows; ri++)
                    {
                        imgH += (sheet.RowHeights.TryGetValue(ri, out var rh) ? rh : lineHeight / tRowScale) * tRowScale;
                    }
                    imgH -= img.FromRowOffEmu * EmuToPtTitle;
                    imgH += img.ToRowOffEmu * EmuToPtTitle;
                    imgH = Math.Max(imgH, 1f);
                }

                // Clamp image by remaining drawable space from its top-left anchor.
                var maxImgW = Math.Max(1f, pageWidth - options.MarginRight - imgX);
                var maxImgH = Math.Max(1f, imgTopY - options.MarginBottom);
                imgW = Math.Max(1f, Math.Min(imgW, maxImgW));
                imgH = Math.Max(1f, Math.Min(imgH, maxImgH));

                var imgY = imgTopY - imgH;
                if (imgY < options.MarginBottom) imgY = options.MarginBottom;
                var format = img.Extension is "jpg" or "jpeg" ? "jpg" : "png";
                currentPage!.AddImage(img.Data, format, imgX, imgY, imgW, imgH);
            }

            return totalHeight;
        }

        // Render rows
        foreach (var row in sheet.Rows)
        {
            EnsurePage();

            // Manual row break: force a new page before this row
            if (sheet.RowBreaks.Contains(excelRowIndex) && currentPage != null && currentY < pageHeight - options.MarginTop)
            {
                currentPage = doc.AddPage(pageWidth, pageHeight);
                currentY = pageHeight - options.MarginTop;
                // Render print title rows on the new page
                if (printTitleStart >= 0 && (excelRowIndex < printTitleStart || excelRowIndex > printTitleEnd))
                    RenderPrintTitleRows();
            }

            // Determine this row's effective height
            var hasExplicitHeight = sheet.RowHeights.TryGetValue(excelRowIndex, out var explicitRowHeight);

            // Skip hidden rows (height 0 in the RowHeights dictionary)
            if (hasExplicitHeight && explicitRowHeight <= 0f)
            {
                rowTopY[excelRowIndex] = currentY;
                rowPage[excelRowIndex] = currentPage!;
                excelRowIndex++;
                continue;
            }

            // Apply print scale to explicit row heights
            if (hasExplicitHeight && sheet.PrintScale != 100 && sheet.PrintScale > 0)
                explicitRowHeight *= sheet.PrintScale / 100f;
            // Apply fitToPage additional scale
            if (hasExplicitHeight && fitToPageScale < 1f)
                explicitRowHeight *= fitToPageScale;

            // Record top-of-row state for image placement
            rowTopY[excelRowIndex] = currentY;
            rowPage[excelRowIndex] = currentPage!;

            if (row.Count == 0)
            {
                var emptyH = hasExplicitHeight ? explicitRowHeight : lineHeight;
                currentY -= emptyH;
                excelRowIndex++;
                continue;
            }

            // Calculate wrapped lines for each column in this group
            var maxLinesInRow = 1;
            var virtualRowExtraLines = 0; // extra lines from virtual wrapping (text overflows page width)
            var cellLines = new string[columns.Length][];
            var cellClipWidth = new float?[columns.Length]; // maxWidth for cells that need horizontal scaling

            for (var i = 0; i < columns.Length; i++)
            {
                var col = columns[i];
                if (col < row.Count)
                {
                    var cellText = row[col].Text;

                    if (!string.IsNullOrEmpty(cellText))
                    {
                        // Use the cell's actual font size for width calculations,
                        // scaled by print scale to match print-scaled column widths
                        var cellFontSizeForFit = row[col].FontSize * printScaleFactor;

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
                                    effectiveWidth += colWidths[mc] + columnPadding;
                            }

                            // Excel/LibreOffice clip text at the column boundary when the
                            // next cell to the right contains content.  For the last column
                            // in the group (or when the next cell is empty) the text overflows.
                            // Merged cells are always clipped at the merge boundary.
                            //
                            // For General-format numeric cells, LibreOffice always reformats
                            // the number to fit the column width, even for the last column.
                            if (!cellText.Contains('\n'))
                                cellText = FitNumericText(cellText, effectiveWidth, cellFontSizeForFit);
                            // Subtract indent from available width so text doesn't overflow
                            // into adjacent columns when rendered at x + indentOff.
                            var cellIndentPts = (row[col].Indent) * avgCharWidth;
                            var textAvailWidth = effectiveWidth - cellIndentPts;
                            // FittingChars applies CalibriFittingScale internally, so it
                            // underestimates actual Helvetica character widths.  For the
                            // wrap/clip decision, compensate by passing a narrower target
                            // so that the check reflects how many characters truly fit
                            // when rendered.  Using 0.97 (slightly above CalibriFittingScale)
                            // allows a few more characters per line (closer to Calibri
                            // line breaks) while cellClipWidth catches any overflow with
                            // mild Tz horizontal compression.
                            var renderFitWidth = (float)(textAvailWidth * 0.97);
                            var fitChars = FittingChars(cellText, renderFitWidth, cellFontSizeForFit);
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

                            // When wrapText is set, always set cellClipWidth so that
                            // PdfWriter can apply Tz horizontal scaling if any line
                            // overflows the column.  FittingChars uses CalibriFittingScale
                            // (0.85×) which may allow more characters than actually fit
                            // when rendered in Helvetica.
                            if (row[col].WrapText)
                                cellClipWidth[i] = (float)textAvailWidth;

                            if (row[col].WrapText && fitChars < cellText.Length)
                            {
                                cellLines[i] = WrapCellText(cellText, renderFitWidth, cellFontSizeForFit);
                            }
                            else
                            {
                                var shouldClip = isMerged || (!isLastCol && nextCellHasContent);
                                // Don't set cellClipWidth (maxWidth) — text is already
                                // truncated by FittingChars below when it overflows.
                                // Setting maxWidth triggers Tz horizontal scaling in PdfWriter
                                // using raw Helvetica widths, which over-compresses text
                                // because FittingChars uses CalibriFittingScale (0.85×).
                                if (shouldClip)
                                {
                                    fitChars = FittingChars(cellText, textAvailWidth, cellFontSizeForFit);
                                }
                                if (shouldClip && cellText.Length > fitChars)
                                {
                                    // Truncate to effective column width (matches LibreOffice clip).
                                    cellLines[i] = new[] { cellText[..fitChars] };
                                }
                                else if (!shouldClip && fitChars < cellText.Length && columns.Length == 1)
                                {
                                    // Single-column overflow: clip text at page right edge.
                                    // LibreOffice calculates row height from text wrapping at the default
                                    // column width, but renders a single line clipped at the page boundary.
                                    var pageClipChars = FittingChars(cellText, pageWidth - options.MarginLeft, cellFontSizeForFit);
                                    var clippedText = pageClipChars < cellText.Length ? cellText[..pageClipChars] : cellText;
                                    cellLines[i] = new[] { clippedText };

                                    // Calculate virtual row height from wrapping at default column width.
                                    // Use raw Helvetica widths (counteract CalibriFittingScale) and subtract
                                    // cell content margins (~11pt) to match LibreOffice's internal wrapping.
                                    var defaultColPts = ExcelSheet.CharUnitsToPoints(
                                        sheet.DefaultColumnWidth > 0 ? sheet.DefaultColumnWidth : 8.43f);
                                    var wrapWidth = Math.Max(1f, (defaultColPts - 11f) * (float)CalibriFittingScale);
                                    var wrapChars = FittingChars(cellText, wrapWidth, cellFontSizeForFit);
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

            // Auto-expand row height for cells with large font sizes.
            // LibreOffice auto-grows rows to fit content; Calibri line metrics
            // show ~1.3× font-size ratio (e.g. 12pt→15.6, 18pt→23.4, 24pt→31.2).
            var maxCellFontSize = options.FontSize;
            for (var i = 0; i < columns.Length; i++)
            {
                var col = columns[i];
                if (col < row.Count)
                {
                    // When fitToPage is active, always compare scaled cell fonts so
                    // unscaled 11pt doesn't falsely exceed scaled fontSize (e.g. 5.28pt
                    // at 48%) and inflate every row.  Non-fitToPage sheets keep existing
                    // auto-expand behaviour to preserve proven page counts.
                    var fs = (sheet.FitToPage || options.ScaleCellFonts) ? row[col].FontSize * printScaleFactor : row[col].FontSize;
                    if (fs > maxCellFontSize) maxCellFontSize = fs;
                }
            }
            var autoRowHeight = maxCellFontSize > options.FontSize
                ? Math.Max(maxCellFontSize * 1.3f, lineHeight)
                : lineHeight;

            // Check space for wrapped lines
            var contentHeight = autoRowHeight * maxLinesInRow;
            // When the XLSX specifies explicit row heights, honour them.  For rows
            // where some cells have wrapText the wrapped content was calculated
            // above; the explicit height already accommodates the expected number
            // of wrapped lines (set by the spreadsheet author / LibreOffice).
            // Only auto-expand when content exceeds the explicit height AND none
            // of the cells use wrapText (i.e. it is an auto-sized row with large
            // fonts, not a deliberately sized row with wrapping).
            var anyWrapText = false;
            for (var i = 0; i < columns.Length; i++)
            {
                var col = columns[i];
                if (col < row.Count && row[col].WrapText && cellLines[i].Length > 1)
                {
                    anyWrapText = true;
                    break;
                }
            }
            // customHeight="1" means the row height is fixed by the spreadsheet author;
            // do not expand it even when content (large fonts / wrap) would exceed it.
            var isCustomHeight = hasExplicitHeight && sheet.CustomHeightRows.Contains(excelRowIndex);
            var rowHeight = hasExplicitHeight
                ? (isCustomHeight ? explicitRowHeight : Math.Max(explicitRowHeight, contentHeight))
                : contentHeight;
            var usablePageHeight = pageHeight - options.MarginTop - options.MarginBottom;

            if (currentY - rowHeight < options.MarginBottom && currentPage != null)
            {
                currentPage = doc.AddPage(pageWidth, pageHeight);
                currentY = pageHeight - options.MarginTop;
                // Render print title rows on the new page (skip if current row is within the title range)
                if (printTitleStart >= 0 && (excelRowIndex < printTitleStart || excelRowIndex > printTitleEnd))
                    RenderPrintTitleRows();
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
                        var cell = col < row.Count ? row[col] : null;
                        var color = col < row.Count ? row[col].Color : null;
                        var mpAlignment = col < row.Count ? row[col].Alignment : "left";
                        var cellY = currentY;

                        // For merged cells, compute the full available text width.
                        var mpCellWidth = colWidths[i];
                        if (mergeEndCol.TryGetValue((excelRowIndex, col), out var mpEndCol))
                        {
                            for (var mc = i + 1; mc < columns.Length && columns[mc] <= mpEndCol; mc++)
                                mpCellWidth += colWidths[mc] + columnPadding;
                        }

                        // Render accounting prefix left-aligned in overflow rows.
                        if (cell?.AccountingPrefix != null && linesRendered == 0 && lines.Length > 0 && !string.IsNullOrEmpty(lines[0]))
                        {
                            currentPage!.AddText(cell.AccountingPrefix, x, cellY, options.FontSize, color,
                                bold: ShouldUsePdfBold(cell?.Bold ?? false, options.FontSize, cell?.FontName),
                                underline: false,
                                preferredFontName: cell?.FontName);
                        }

                        var mpIsBold = ShouldUsePdfBold(cell?.Bold ?? false, options.FontSize, cell?.FontName);
                        var mpIndentOff = (cell?.Indent ?? 0) * avgCharWidth;
                        var mpAcctPrefixW = cell?.AccountingPrefix != null
                            ? (float)MeasureHelveticaWidth(cell.AccountingPrefix, options.FontSize, bold: mpIsBold)
                            : 0f;
                        for (var lineIdx = linesRendered; lineIdx < linesRendered + linesToRender && lineIdx < lines.Length; lineIdx++)
                        {
                            if (!string.IsNullOrEmpty(lines[lineIdx]))
                            {
                                var textX = x + mpIndentOff;
                                float mpLineMaxWidth = 0;
                                if (mpAlignment == "right" || mpAcctPrefixW > 0)
                                {
                                    var tw = (float)MeasureHelveticaWidth(lines[lineIdx], options.FontSize, bold: mpIsBold);
                                    textX = x + mpCellWidth - tw - mpIndentOff;
                                    if (mpAcctPrefixW > 0 && textX < x + mpAcctPrefixW)
                                    {
                                        textX = x + mpAcctPrefixW;
                                        mpLineMaxWidth = mpCellWidth - mpAcctPrefixW;
                                    }
                                }
                                else if (mpAlignment == "center")
                                {
                                    var tw = (float)MeasureHelveticaWidth(lines[lineIdx], options.FontSize, bold: mpIsBold);
                                    textX = x + (mpCellWidth - tw) / 2f;
                                }
                                currentPage!.AddText(lines[lineIdx], textX, cellY, options.FontSize, color,
                                    bold: mpIsBold,
                                    underline: cell?.Underline ?? false,
                                    preferredFontName: cell?.FontName,
                                    maxWidth: mpLineMaxWidth > 0 ? mpLineMaxWidth : null);
                            }
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

            // Detect rows that are effectively a single background band (same fill, no borders)
            // and paint them in one rectangle to prevent vertical seams.
            PdfColor? rowUniformFill = null;
            var rowHasUniformFill = true;
            for (var i = 0; i < columns.Length; i++)
            {
                var col = columns[i];
                var c = col < row.Count ? row[col] : null;
                if (c?.FillColor is not { } fc || c.Border != null || mergeInterior.Contains((excelRowIndex, col)))
                {
                    rowHasUniformFill = false;
                    break;
                }

                if (rowUniformFill == null)
                    rowUniformFill = fc;
                else if (rowUniformFill.Value != fc)
                {
                    rowHasUniformFill = false;
                    break;
                }
            }

            if (rowHasUniformFill && rowUniformFill != null)
            {
                var rowWidth = colWidths.Sum() + columnPadding * (columns.Length - 1);
                currentPage!.AddRectangle(options.MarginLeft, currentY - rowHeight, rowWidth, rowHeight, rowUniformFill.Value);
            }

            var x = options.MarginLeft;
            for (var i = 0; i < columns.Length; i++)
            {
                var lines = cellLines[i];
                var col = columns[i];
                var cell = col < row.Count ? row[col] : null;
                var color = cell?.Color;
                var fillColor = cell?.FillColor;
                var alignment = cell?.Alignment ?? "left";
                var cellFontSize = cell != null
                    ? cell.FontSize * printScaleFactor
                    : options.FontSize;
                var border = cell?.Border;
                var verticalAlignment = cell?.VerticalAlignment ?? "bottom";

                // Calculate vertical position based on vertical alignment.
                // Use base font descent (≈ 0.31 × fontSize) so all cells in the row
                // share the same baseline, preventing text extraction line-splitting.
                var descent = options.FontSize * 0.31f;
                // Compensate ascender differences only for larger fonts.
                // Applying this to smaller fonts shifts them upward and can merge
                // neighboring logical rows in extracted text and visual alignment.
                var ascentCompensation = cellFontSize > options.FontSize
                    ? (cellFontSize - options.FontSize) * 0.1f
                    : 0f;
                float cellY;
                var textBlock = cellFontSize + lineHeight * (lines.Length - 1);
                if (verticalAlignment == "top" || textBlock > rowHeight)
                    cellY = currentY - cellFontSize;
                else if (verticalAlignment == "center")
                {
                    // Keep single-line centered cells closer to the baseline so
                    // cross-column text rows stay aligned with reference output.
                    if (lines.Length <= 1)
                    {
                        var slack = Math.Max(0f, rowHeight - cellFontSize);
                        cellY = currentY - rowHeight + descent - ascentCompensation + slack * 0.35f;
                    }
                    else
                    {
                        cellY = currentY - (rowHeight - textBlock) / 2f - cellFontSize + descent - ascentCompensation;
                    }
                }
                else // "bottom" (default)
                    cellY = currentY - rowHeight + descent - ascentCompensation + lineHeight * (lines.Length - 1);

                // Skip fill/border for cells inside a merge range (not the start column).
                // Only the merge origin cell renders the fill covering the full merged area.
                var isInsideMerge = mergeInterior.Contains((excelRowIndex, col));

                // Draw fill rectangle behind cell if fill color is set.
                // For merged cells, extend the fill across the full merged column span.
                if (fillColor != null && !rowHasUniformFill && !isInsideMerge)
                {
                    var fillWidth = colWidths[i];
                    if (mergeEndCol.TryGetValue((excelRowIndex, col), out var fillEndCol))
                    {
                        for (var mc = i + 1; mc < columns.Length && columns[mc] <= fillEndCol; mc++)
                            fillWidth += colWidths[mc] + columnPadding;
                    }

                    // Avoid hairline seams only where neighboring cells share the same fill
                    // and have no explicit borders (do not bleed across styled boundaries).
                    var fillSeamOverlap = Math.Max(0.2f, columnPadding);
                    var fillY = currentY - rowHeight;
                    var fillHeight = rowHeight;
                    if (border == null)
                    {
                        var mergedHere = mergeEndCol.ContainsKey((excelRowIndex, col));

                        var hasSameFillRight = false;
                        if (!mergedHere && i + 1 < columns.Length && columns[i + 1] == col + 1)
                        {
                            var rightCol = columns[i + 1];
                            var rightCell = rightCol < row.Count ? row[rightCol] : null;
                            var rightInsideMerge = mergeInterior.Contains((excelRowIndex, rightCol));
                            var rightMerged = mergeEndCol.ContainsKey((excelRowIndex, rightCol));
                            hasSameFillRight = rightCell?.FillColor is { } rightFill
                                && rightFill == fillColor.Value
                                && rightCell.Border == null
                                && !rightInsideMerge
                                && !rightMerged;
                        }

                        var hasSameFillBelow = false;
                        if (!mergedHere && excelRowIndex + 1 < sheet.Rows.Count)
                        {
                            var belowRow = sheet.Rows[excelRowIndex + 1];
                            var belowCell = col < belowRow.Count ? belowRow[col] : null;
                            var belowInsideMerge = mergeInterior.Contains((excelRowIndex + 1, col));
                            var belowMerged = mergeEndCol.ContainsKey((excelRowIndex + 1, col));
                            hasSameFillBelow = belowCell?.FillColor is { } belowFill
                                && belowFill == fillColor.Value
                                && belowCell.Border == null
                                && !belowInsideMerge
                                && !belowMerged;
                        }

                        if (hasSameFillRight)
                            fillWidth += fillSeamOverlap;
                        if (hasSameFillBelow)
                        {
                            fillHeight += fillSeamOverlap;
                            fillY -= fillSeamOverlap;
                        }
                    }

                    currentPage!.AddRectangle(x, fillY, fillWidth, fillHeight, fillColor);
                }

                // Draw cell borders.
                // For merged cells, extend the right border to the end of the merged span.
                if (border != null && !isInsideMerge)
                {
                    var borderHeight = rowHeight;
                    var bx = x;
                    var byTop = currentY;
                    var byBottom = currentY - borderHeight;
                    var bxRight = x + colWidths[i] + columnPadding;
                    if (mergeEndCol.TryGetValue((excelRowIndex, col), out var borderEndCol))
                    {
                        for (var mc = i + 1; mc < columns.Length && columns[mc] <= borderEndCol; mc++)
                            bxRight += colWidths[mc] + columnPadding;
                    }
                    var borderColor = new PdfColor(0f, 0f, 0f);

                    if (border.Left is { Style: not "none" and not "" })
                    {
                        var bc = border.Left.Color ?? borderColor;
                        var bw = Math.Max(0.08f, BorderStyleWidth(border.Left.Style) * borderScaleFactor);
                        currentPage!.AddLine(bx, byTop, bx, byBottom, bc, bw, BorderDashPattern(border.Left.Style, bw));
                    }
                    if (border.Right is { Style: not "none" and not "" })
                    {
                        var bc = border.Right.Color ?? borderColor;
                        var bw = Math.Max(0.08f, BorderStyleWidth(border.Right.Style) * borderScaleFactor);
                        currentPage!.AddLine(bxRight, byTop, bxRight, byBottom, bc, bw, BorderDashPattern(border.Right.Style, bw));
                    }
                    if (border.Top is { Style: not "none" and not "" })
                    {
                        var bc = border.Top.Color ?? borderColor;
                        var bw = Math.Max(0.08f, BorderStyleWidth(border.Top.Style) * borderScaleFactor);
                        currentPage!.AddLine(bx, byTop, bxRight, byTop, bc, bw, BorderDashPattern(border.Top.Style, bw));
                    }
                    if (border.Bottom is { Style: not "none" and not "" })
                    {
                        var bc = border.Bottom.Color ?? borderColor;
                        var bw = Math.Max(0.08f, BorderStyleWidth(border.Bottom.Style) * borderScaleFactor);
                        currentPage!.AddLine(bx, byBottom, bxRight, byBottom, bc, bw, BorderDashPattern(border.Bottom.Style, bw));
                    }
                }

                // For merged cells, compute the full available text width.
                var cellWidth = colWidths[i];
                if (mergeEndCol.TryGetValue((excelRowIndex, col), out var textEndCol))
                {
                    for (var mc = i + 1; mc < columns.Length && columns[mc] <= textEndCol; mc++)
                        cellWidth += colWidths[mc] + columnPadding;
                }

                // Render accounting prefix (e.g., " $") left-aligned for accounting format cells.
                if (cell?.AccountingPrefix != null && lines.Length > 0 && !string.IsNullOrEmpty(lines[0]))
                {
                    currentPage!.AddText(cell.AccountingPrefix, x, cellY, cellFontSize, color,
                        maxWidth: cellClipWidth[i],
                        bold: ShouldUsePdfBold(cell.Bold, cellFontSize, cell.FontName),
                        underline: false,
                        preferredFontName: cell.FontName);
                }

                var isBold = ShouldUsePdfBold(cell?.Bold ?? false, cellFontSize, cell?.FontName);
                var boldPrefixRemaining = cell?.BoldPrefixLength ?? 0;
                var indentOff = (cell?.Indent ?? 0) * avgCharWidth;
                // For accounting format, reserve space for the left-aligned prefix
                // so the right-aligned number doesn't overlap it.
                var acctPrefixWidth = cell?.AccountingPrefix != null
                    ? (float)MeasureHelveticaWidth(cell.AccountingPrefix, cellFontSize, bold: isBold)
                    : 0f;
                for (var lineIdx = 0; lineIdx < lines.Length; lineIdx++)
                {
                    if (!string.IsNullOrEmpty(lines[lineIdx]))
                    {
                        var textX = x + indentOff;
                        var lineMaxWidth = cellClipWidth[i];
                        // Accounting format forces right-alignment for the number part
                        // regardless of the cell's explicit alignment, because the `*`
                        // fill character in the format code fills the gap between the
                        // left-aligned prefix and the right-aligned number.
                        if (alignment == "right" || acctPrefixWidth > 0)
                        {
                            var textWidth = (float)MeasureHelveticaWidth(lines[lineIdx], cellFontSize, bold: isBold);
                            textX = x + cellWidth - textWidth - indentOff;
                            // Clamp: don't start before the accounting prefix
                            if (acctPrefixWidth > 0 && textX < x + acctPrefixWidth)
                            {
                                textX = x + acctPrefixWidth;
                                lineMaxWidth = cellWidth - acctPrefixWidth;
                            }
                        }
                        else if (alignment == "center")
                        {
                            var textWidth = (float)MeasureHelveticaWidth(lines[lineIdx], cellFontSize, bold: isBold);
                            textX = x + (cellWidth - textWidth) / 2f;
                        }

                        // Rich text: render bold prefix and normal suffix separately
                        if (boldPrefixRemaining > 0 && !isBold)
                        {
                            var lineText = lines[lineIdx];
                            var boldLen = Math.Min(boldPrefixRemaining, lineText.Length);
                            var boldPart = lineText[..boldLen];
                            var normalPart = boldLen < lineText.Length ? lineText[boldLen..] : null;
                            // If normal text starts with a space, skip it and add an
                            // explicit gap so the space renders between the two AddText
                            // calls rather than being compressed by Tz scaling.
                            float spaceGap = 0;
                            if (normalPart != null && normalPart.Length > 0 && normalPart[0] == ' ')
                            {
                                spaceGap = (float)MeasureHelveticaWidth(" ", cellFontSize, bold: false);
                                normalPart = normalPart[1..];
                                if (normalPart.Length == 0) normalPart = null;
                            }

                            // Measure bold width using Helvetica Bold metrics.
                            // Set maxWidth=boldWidth so PdfWriter Tz-compresses the
                            // actual rendered font (e.g. Verdana-Bold) to exactly this
                            // width, preventing overlap with the normal text.
                            var boldWidth = (float)MeasureHelveticaWidth(boldPart, cellFontSize, bold: true);

                            currentPage!.AddText(boldPart, textX, cellY, cellFontSize, color,
                                maxWidth: boldWidth,
                                bold: true,
                                underline: cell?.Underline ?? false,
                                preferredFontName: cell?.FontName);

                            if (normalPart != null)
                            {
                                var normalX = textX + boldWidth + spaceGap;
                                var normalMax = lineMaxWidth > 0 ? lineMaxWidth - boldWidth - spaceGap : 0f;
                                currentPage!.AddText(normalPart, normalX, cellY, cellFontSize, color,
                                    maxWidth: normalMax > 0 ? normalMax : 0f,
                                    bold: false,
                                    underline: cell?.Underline ?? false,
                                    preferredFontName: cell?.FontName);
                            }
                            boldPrefixRemaining -= boldLen;
                        }
                        else
                        {
                            currentPage!.AddText(lines[lineIdx], textX, cellY, cellFontSize, color,
                                maxWidth: lineMaxWidth,
                                bold: isBold,
                                underline: cell?.Underline ?? false,
                                preferredFontName: cell?.FontName);
                        }
                    }
                    else
                    {
                        // Empty line still consumes bold prefix chars (newline splits)
                        boldPrefixRemaining = Math.Max(0, boldPrefixRemaining - 1);
                    }
                    cellY -= lineHeight;
                }

                x += colWidths[i] + columnPadding;
            }

            currentY -= rowHeight;
            }

            // Accumulate virtual overflow height but don't emit pages yet.
            // LibreOffice renders all rows on minimal pages first, then adds
            // empty overflow pages at the end of the sheet.
            if (virtualRowExtraLines > 0)
            {
                accumulatedOverflowHeight += lineHeight * virtualRowExtraLines;
            }
            excelRowIndex++;
        }

        // Emit accumulated virtual overflow pages at the end (matching LibreOffice layout).
        if (accumulatedOverflowHeight > 0)
        {
            var extraHeight = accumulatedOverflowHeight;
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

        // (Trailing empty page logic moved to RenderSheet for proper per-sheet page tracking)

        // Place drawing shapes (rectangles used as decorative frames)
        const float ShapeEmuToPt = 1f / 12700f;
        foreach (var shape in sheet.Shapes)
        {
            // Polygon shapes (custom geometry from group shapes)
            if (shape.PolygonPoints is { Count: >= 3 } polyPts && shape.WidthEmu > 0 && shape.HeightEmu > 0)
            {
                if (!rowTopY.TryGetValue(shape.FromRow, out var polyTop)) continue;
                var polyFromIdx = Array.IndexOf(columns, shape.FromCol);
                if (polyFromIdx < 0) polyFromIdx = 0;

                var polyX = colXStarts[polyFromIdx] + shape.FromColOffEmu * ShapeEmuToPt;
                polyTop -= shape.FromRowOffEmu * ShapeEmuToPt;
                // Apply group offset
                polyX += shape.OffsetXEmu * ShapeEmuToPt;
                polyTop -= shape.OffsetYEmu * ShapeEmuToPt;

                var polyW = shape.WidthEmu * ShapeEmuToPt;
                var polyH = shape.HeightEmu * ShapeEmuToPt;
                if (polyW <= 0 || polyH <= 0) continue;

                var polyPage = rowPage.TryGetValue(shape.FromRow, out var pp) ? pp : currentPage!;
                var pdfPoints = new List<PdfPoint>(polyPts.Count);
                foreach (var (nx, ny) in polyPts)
                {
                    // Normalized coordinates: X goes right, Y goes down in OOXML
                    // PDF coordinates: Y goes up
                    pdfPoints.Add(new PdfPoint(polyX + nx * polyW, polyTop - ny * polyH));
                }
                if (shape.FillColor is { } pfc)
                    polyPage.AddPolygon(pdfPoints, pfc);
                continue;
            }

            // Resolve anchor positions using rowTopY and colXStarts
            if (!rowTopY.TryGetValue(shape.FromRow, out var shapeTop))
                continue;
            if (!rowTopY.TryGetValue(shape.ToRow, out var shapeBot))
            {
                // If toRow is beyond rendered rows, use bottom margin
                shapeBot = options.MarginBottom;
            }

            // Determine which column group the shape's from-column falls into
            var fromIdx = Array.IndexOf(columns, shape.FromCol);
            if (fromIdx < 0) fromIdx = 0;
            var toIdx = Array.IndexOf(columns, shape.ToCol);
            if (toIdx < 0) toIdx = columns.Length - 1;

            var shapeX = colXStarts[fromIdx] + shape.FromColOffEmu * ShapeEmuToPt;
            var shapeXRight = toIdx < colXStarts.Length ?
                colXStarts[toIdx] + shape.ToColOffEmu * ShapeEmuToPt :
                colXStarts[^1] + colWidths[^1];

            // Adjust for row offsets
            shapeTop -= shape.FromRowOffEmu * ShapeEmuToPt;
            shapeBot -= shape.ToRowOffEmu * ShapeEmuToPt;

            var shapeW = shapeXRight - shapeX;
            var shapeH = shapeTop - shapeBot;
            if (shapeW <= 0 || shapeH <= 0) continue;

            var shapePage = rowPage.TryGetValue(shape.FromRow, out var sp) ? sp : currentPage!;

            // Render fill
            if (shape.FillColor is { } fc)
                shapePage.AddRectangle(shapeX, shapeBot, shapeW, shapeH, fc);

            // Render border
            if (shape.BorderColor is { } bc && shape.BorderWidthPt > 0)
            {
                var bw = shape.BorderWidthPt;
                shapePage.AddLine(shapeX, shapeTop, shapeXRight, shapeTop, bc, bw);         // top
                shapePage.AddLine(shapeX, shapeBot, shapeXRight, shapeBot, bc, bw);         // bottom
                shapePage.AddLine(shapeX, shapeTop, shapeX, shapeBot, bc, bw);              // left
                shapePage.AddLine(shapeXRight, shapeTop, shapeXRight, shapeBot, bc, bw);    // right
            }
        }

        // Place embedded images and chart placeholders
        if (sheet.Images.Count == 0 && sheet.Charts.Count == 0) return;

        // For image/chart-only sheets (no data rows) ensure at least one page exists.
        EnsurePage();
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

            // Apply sub-cell column offset (fromColOff) — shift image right within anchor column
            const float EmuToPt = 1f / 12700f;
            var fromColOffPt = Math.Min(img.FromColOffEmu * EmuToPt, colWidths[colGroupIdx]);
            imgX += fromColOffPt;

            // Apply sub-cell row offset (fromRowOff) — shift image down within anchor row
            var fromRowOffPt = img.FromRowOffEmu * EmuToPt;
            imgTopY -= fromRowOffPt;

            // Apply group-relative offset (for images inside grouped shapes)
            imgX += img.OffsetXEmu * EmuToPt;
            imgTopY -= img.OffsetYEmu * EmuToPt;

            // Calculate render size.
            // Prefer explicit EMU dimensions (from <ext cx cy> in oneCellAnchor).
            // Fallback: derive from spanCols × column widths and spanRows × lineHeight,
            // adjusting for sub-cell offsets (fromColOff / toColOff) in twoCellAnchor.
            float imgRenderWidth, imgRenderHeight;
            if (img.WidthEmu > 0 && img.HeightEmu > 0)
            {
                imgRenderWidth  = img.WidthEmu  * EmuToPt;
                imgRenderHeight = img.HeightEmu * EmuToPt;
            }
            else
            {
                // First column: only the portion after fromColOff
                imgRenderWidth = Math.Max(0, colWidths[colGroupIdx] - fromColOffPt);
                // Middle columns (full width + padding)
                for (var ci = colGroupIdx + 1; ci < Math.Min(colGroupIdx + img.SpanCols, columns.Length); ci++)
                    imgRenderWidth += colWidths[ci] + columnPadding;
                // "To" column: add toColOff portion if the to-column is within the group
                var toColGroupIdx = colGroupIdx + img.SpanCols;
                if (toColGroupIdx < columns.Length && img.ToColOffEmu > 0)
                {
                    var toColOffPt = Math.Min(img.ToColOffEmu * EmuToPt, colWidths[toColGroupIdx]);
                    imgRenderWidth += toColOffPt + columnPadding;
                }
                imgRenderWidth  = Math.Max(imgRenderWidth, 1f);

                // Height: sum actual row heights for the spanned rows
                imgRenderHeight = 0;
                var printScale = (sheet.PrintScale > 0 && sheet.PrintScale != 100) ? sheet.PrintScale / 100f : 1f;
                var rowScale = printScale * fitToPageScale;
                for (var ri = img.AnchorRow; ri < img.AnchorRow + img.SpanRows; ri++)
                {
                    imgRenderHeight += (sheet.RowHeights.TryGetValue(ri, out var rh) ? rh : lineHeight / rowScale) * rowScale;
                }
                // Adjust for sub-cell row offsets
                imgRenderHeight -= fromRowOffPt;
                imgRenderHeight += img.ToRowOffEmu * EmuToPt;
                imgRenderHeight = Math.Max(imgRenderHeight, 1f);
            }

            // Clamp image by remaining drawable space from its top-left anchor.
            var maxRenderW = Math.Max(1f, pageWidth - options.MarginRight - imgX);
            var maxRenderH = Math.Max(1f, imgTopY - options.MarginBottom);
            imgRenderWidth = Math.Max(1f, Math.Min(imgRenderWidth, maxRenderW));
            imgRenderHeight = Math.Max(1f, Math.Min(imgRenderHeight, maxRenderH));

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

        var usableWidth = pageWidth - options.MarginLeft - options.MarginRight;

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
                // Chart anchor row is beyond rendered rows — find closest earlier row
                // and extrapolate, or use current rendering Y position as the baseline.
                var bestRow = -1;
                foreach (var key in rowTopY.Keys)
                {
                    if (key <= chart.AnchorRow && key > bestRow) bestRow = key;
                }
                if (bestRow >= 0)
                {
                    chartTopY = rowTopY[bestRow] - (chart.AnchorRow - bestRow) * lineHeight;
                }
                else
                {
                    chartTopY = currentY;
                }
            }
            if (!rowPage.TryGetValue(chart.AnchorRow, out var chartPage))
            {
                // Use the page of the closest earlier row, or the last rendered page
                var bestRow = -1;
                foreach (var key in rowPage.Keys)
                {
                    if (key <= chart.AnchorRow && key > bestRow) bestRow = key;
                }
                chartPage = bestRow >= 0 ? rowPage[bestRow] : currentPage!;
            }

            var chartColIdx = Array.IndexOf(columns, chart.AnchorCol);
            if (chartColIdx < 0) chartColIdx = 0;
            var chartX = colXStarts[chartColIdx];

            // Calculate chart render size from EMU (cap to page like original)
            const float EmuToPt2 = 1f / 12700f;
            var chartWidth = Math.Min(chart.WidthEmu * EmuToPt2, usableWidth * 0.95f);
            var chartHeight = chart.HeightEmu * EmuToPt2;
            if (chartWidth < 72) chartWidth = usableWidth * 0.6f;
            if (chartHeight < 72) chartHeight = chartWidth * 0.65f;

            // Scale dominant charts (twoCellAnchor spanning >50% of sheet rows) to
            // fill usable page width, matching LibreOffice's full-page output.
            // Charts anchored within the data area (AnchorCol > 1) are rendered
            // inline at their anchor position alongside data, not scaled up.
            var estRowSpan = chart.HeightEmu / 304800f; // rough rows from EMU
            var isChartDominant = chart.IsTwoCellAnchor
                && sheet.Rows.Count > 0 && estRowSpan / sheet.Rows.Count > 0.5f;
            var isInlineChart = isChartDominant && chart.AnchorCol > 1;
            if (isChartDominant && !isInlineChart && chartWidth > 0 && chartWidth < usableWidth * 0.9f)
            {
                var scaleUp = usableWidth * 0.95f / chartWidth;
                chartHeight *= scaleUp;
                chartWidth = usableWidth * 0.95f;
                // After scale-up, if chart extends beyond page, move to left margin
                if (chartX + chartWidth > pageWidth - options.MarginRight)
                    chartX = options.MarginLeft;
            }
            // Cap height for inline (non-dominant) charts to avoid page overflow
            if (!isChartDominant)
                chartHeight = Math.Min(chartHeight, pageHeight * 0.85f);

            // Ensure chart fits on page, start new page if needed
            var chartTop = chartTopY;
            if (chartTop - chartHeight < options.MarginBottom)
            {
                chartPage = doc.AddPage(pageWidth, pageHeight);
                chartTop = pageHeight - options.MarginTop;
            }

            // Render the chart on the current page
            var renderHeight = isChartDominant
                ? Math.Min(chartHeight, chartTop - options.MarginBottom)
                : chartHeight;
            RenderChart(chartPage, chart, chartX, chartTop, chartWidth, renderHeight, options.FontSize);

            // Add overflow pages for dominant charts taller than 1 page
            if (isChartDominant)
            {
                var usablePageH = pageHeight - options.MarginTop - options.MarginBottom;
                var overflowHeight = chartHeight - renderHeight;
                // Only force minimum overflow pages for non-inline dominant charts
                // that are rendered as standalone (e.g. chart-only sheets in LibreOffice).
                // Inline charts (anchored within data area) should only get overflow
                // pages when their actual content exceeds what was rendered.
                if (!isInlineChart && overflowHeight < usablePageH + 1f)
                    overflowHeight = usablePageH + 1f;
                while (overflowHeight > 0f)
                {
                    chartPage = doc.AddPage(pageWidth, pageHeight);
                    overflowHeight -= usablePageH;
                }
            }

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
        var chartColor = chart.ChartTextColor;
        var chartFont = chart.ChartFontName;

        // Draw chart title (clipped to chart width)
        var titleY = top;
        if (!string.IsNullOrEmpty(chart.Title))
        {
            var titleAvailWidth = width - padding * 2;  // use nearly full chart width
            var titleChars = FittingChars(chart.Title, titleAvailWidth, titleFontSize);
            var clippedTitle = titleChars >= chart.Title.Length ? chart.Title : chart.Title[..titleChars];
            // Center the title horizontally
            var titleTextWidth = (float)MeasureHelveticaWidth(clippedTitle, titleFontSize);
            var titleX = x + (width - titleTextWidth) / 2f;
            page.AddText(clippedTitle, titleX, titleY - titleFontSize, titleFontSize, chartColor, preferredFontName: chartFont);
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
            RenderPieChart(page, chart, x, top, width, height, plotLeft, plotBottom, plotWidth, plotHeight, labelFontSize, type.Contains("doughnut"), chart.ShowDataLabelPercent, chartColor, chartFont);
        }
        else if (type.Contains("scatter") || type.Contains("bubble"))
        {
            RenderScatterChart(page, chart, plotLeft, plotBottom, plotWidth, plotHeight, labelFontSize, axisFontSize, chart.ValueAxisFormatCode, chartColor, chartFont);
        }
        else if (type.Contains("radar"))
        {
            RenderLineChart(page, chart, plotLeft, plotBottom, plotWidth, plotHeight, labelFontSize, axisFontSize, skipAxisLabels: true, axisFmtCode: chart.ValueAxisFormatCode, textColor: chartColor, fontName: chartFont);
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
                    page.AddText(TruncateLabel(categories[ci], 15), lx - axisFontSize * 2, ly - axisFontSize * 0.3f, axisFontSize, chartColor, preferredFontName: chartFont);
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
                        page.AddText(label, centerX - axisFontSize, tickY, axisFontSize, chartColor, preferredFontName: chartFont);
                    }
                }
            }
        }
        else if (type.Contains("line"))
        {
            RenderLineChart(page, chart, plotLeft, plotBottom, plotWidth, plotHeight, labelFontSize, axisFontSize, skipAxisLabels: false, axisFmtCode: chart.ValueAxisFormatCode, textColor: chartColor, fontName: chartFont);
        }
        else if (type.Contains("area"))
        {
            RenderAreaChart(page, chart, plotLeft, plotBottom, plotWidth, plotHeight, labelFontSize, axisFontSize, chart.ValueAxisFormatCode, chartColor, chartFont);
        }
        else if (type.Contains("horizontal"))
        {
            RenderHorizontalBarChart(page, chart, plotLeft, plotBottom, plotWidth, plotHeight, labelFontSize, axisFontSize, chart.ValueAxisFormatCode, chartColor, chartFont);
        }
        else
        {
            // Default: bar/column/bubble → bar chart
            RenderBarChart(page, chart, plotLeft, plotBottom, plotWidth, plotHeight, labelFontSize, axisFontSize, chart.ValueAxisFormatCode, chartColor, chartFont);
        }

        // Y-axis title (rotated text not supported, place vertically aligned)
        if (!string.IsNullOrEmpty(chart.ValueAxisTitle))
        {
            page.AddText(chart.ValueAxisTitle, x + 2, plotBottom + plotHeight * 0.4f, axisFontSize, chartColor, preferredFontName: chartFont);
        }
        // X-axis title
        if (!string.IsNullOrEmpty(chart.CategoryAxisTitle))
        {
            page.AddText(chart.CategoryAxisTitle, plotLeft + plotWidth * 0.35f, plotBottom - 22f, axisFontSize, chartColor, preferredFontName: chartFont);
        }
    }

    /// <summary>Renders a bar/column chart.</summary>
    private static void RenderBarChart(PdfPage page, ExcelChartInfo chart,
        float plotLeft, float plotBottom, float plotWidth, float plotHeight,
        float labelFontSize, float axisFontSize, string axisFmtCode = "",
        PdfColor? textColor = null, string? fontName = null)
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
            var allValues = series.SelectMany(s => s.Values)
                .Concat(chart.OverlaySeries.SelectMany(s => s.Values))
                .ToArray();
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
            page.AddText(label, plotLeft - label.Length * axisFontSize * 0.5f - 4f, gridY - axisFontSize * 0.3f, axisFontSize, textColor, preferredFontName: fontName);
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
                page.AddText(label, labelX, plotBottom - axisFontSize * 1.5f, axisFontSize, textColor, preferredFontName: fontName);
            }
        }

        // Draw axes
        page.AddLine(plotLeft, plotBottom, plotLeft, plotBottom + plotHeight,
            new PdfColor(0, 0, 0), 0.8f);
        page.AddLine(plotLeft, baselineY, plotLeft + plotWidth, baselineY,
            new PdfColor(0, 0, 0), 0.8f);

        // Draw overlay line series (for combo charts)
        if (chart.OverlaySeries.Count > 0)
        {
            var colorOffset = series.Count; // start colors after bar series
            for (var si = 0; si < chart.OverlaySeries.Count; si++)
            {
                var s = chart.OverlaySeries[si];
                var color = ChartColors[(colorOffset + si) % ChartColors.Length];
                for (var pi = 1; pi < s.Values.Length; pi++)
                {
                    var x1 = plotLeft + (pi - 1) * plotWidth / Math.Max(1, numCats - 1);
                    var y1 = plotBottom + (float)((s.Values[pi - 1] - niceMin) / range) * plotHeight;
                    var x2 = plotLeft + pi * plotWidth / Math.Max(1, numCats - 1);
                    var y2 = plotBottom + (float)((s.Values[pi] - niceMin) / range) * plotHeight;
                    page.AddLine(x1, y1, x2, y2, color, 2f);
                }
                // Line markers
                for (var pi = 0; pi < s.Values.Length; pi++)
                {
                    var px = plotLeft + pi * plotWidth / Math.Max(1, numCats - 1);
                    var py = plotBottom + (float)((s.Values[pi] - niceMin) / range) * plotHeight;
                    page.AddRectangle(px - 2.5f, py - 2.5f, 5, 5, color);
                }
            }
        }

        // Legend (include both bar and overlay series)
        var allSeries = new List<ExcelChartSeries>(series);
        allSeries.AddRange(chart.OverlaySeries);
        RenderLegend(page, allSeries, plotLeft + plotWidth * 0.05f, plotBottom + plotHeight + 5f, axisFontSize, isStacked, textColor, fontName);
    }

    /// <summary>Renders a horizontal bar chart (categories on Y-axis, values on X-axis).</summary>
    private static void RenderHorizontalBarChart(PdfPage page, ExcelChartInfo chart,
        float plotLeft, float plotBottom, float plotWidth, float plotHeight,
        float labelFontSize, float axisFontSize, string axisFmtCode = "",
        PdfColor? textColor = null, string? fontName = null)
    {
        var series = chart.Series;
        if (series.Count == 0) return;

        var allValues = series.SelectMany(s => s.Values).ToArray();
        if (allValues.Length == 0) return;

        var isStacked = chart.ChartType.Contains("stacked", StringComparison.OrdinalIgnoreCase);
        var isPercentStacked = chart.ChartType.Contains("percentStacked", StringComparison.OrdinalIgnoreCase);

        var categories = series[0].Categories;
        var numCats = Math.Max(categories.Length, series.Max(s => s.Values.Length));
        if (numCats == 0) return;
        var numSeries = series.Count;

        // Determine axis range
        double niceMin, niceMax, niceStep;
        if (isPercentStacked)
        {
            niceMin = 0; niceMax = 100; niceStep = 10;
        }
        else if (isStacked)
        {
            double dataMax = 0, dataMin = 0;
            for (var ci = 0; ci < numCats; ci++)
            {
                double posSum = 0, negSum = 0;
                for (var si = 0; si < numSeries; si++)
                {
                    var val = ci < series[si].Values.Length ? series[si].Values[ci] : 0;
                    if (val >= 0) posSum += val; else negSum += val;
                }
                dataMax = Math.Max(dataMax, posSum);
                dataMin = Math.Min(dataMin, negSum);
            }
            (niceMin, niceMax, niceStep) = NiceAxisScale(dataMin, dataMax);
        }
        else
        {
            var dataMax = allValues.Max();
            var dataMin = Math.Min(0, allValues.Min());
            (niceMin, niceMax, niceStep) = NiceAxisScale(dataMin, dataMax);
        }

        var range = niceMax - niceMin;
        if (range <= 0) range = 1;

        var groupHeight = plotHeight / numCats;
        var barHeight = isStacked ? groupHeight * 0.7f : groupHeight * 0.7f / numSeries;
        var groupPadding = groupHeight * 0.15f;

        var baselineX = plotLeft + (float)((0 - niceMin) / range) * plotWidth;

        // X-axis gridlines and labels (skip for percent-stacked with deleted axis)
        if (!isPercentStacked)
        {
            for (var tickVal = niceMin; tickVal <= niceMax + niceStep * 0.01; tickVal += niceStep)
            {
                var gridX = plotLeft + (float)((tickVal - niceMin) / range) * plotWidth;
                page.AddLine(gridX, plotBottom, gridX, plotBottom + plotHeight,
                    new PdfColor(0.85f, 0.85f, 0.85f), 0.5f);
                var label = FormatAxisValue(tickVal, axisFmtCode);
                page.AddText(label, gridX - label.Length * axisFontSize * 0.25f, plotBottom - axisFontSize * 1.5f, axisFontSize, textColor, preferredFontName: fontName);
            }
        }

        // Data label format
        var dlFmt = chart.DataLabelFormatCode;
        var showVal = chart.ShowDataLabelVal;

        // Draw horizontal bars
        for (var ci = 0; ci < numCats; ci++)
        {
            var groupY = plotBottom + ci * groupHeight + groupPadding;

            if (isStacked)
            {
                double cumPos = 0, cumNeg = 0;
                double catTotal = 0;
                if (isPercentStacked)
                {
                    for (var si = 0; si < numSeries; si++)
                        catTotal += Math.Abs(ci < series[si].Values.Length ? series[si].Values[ci] : 0);
                    if (catTotal == 0) catTotal = 1;
                }

                for (var si = 0; si < numSeries; si++)
                {
                    var rawVal = si < numSeries && ci < series[si].Values.Length
                        ? series[si].Values[ci] : 0;
                    var val = isPercentStacked ? (rawVal / catTotal * 100) : rawVal;
                    var barY = groupY;

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

                    var x0 = plotLeft + (float)((barBase - niceMin) / range) * plotWidth;
                    var x1 = plotLeft + (float)((barTop - niceMin) / range) * plotWidth;
                    var barDrawW = Math.Max(0.5f, Math.Abs(x1 - x0));
                    var color = ChartColors[si % ChartColors.Length];
                    page.AddRectangle(Math.Min(x0, x1), barY, barDrawW, barHeight, color);

                    // Data label centered in bar segment
                    if (showVal && barDrawW > 10)
                    {
                        var labelText = FormatDataLabel(rawVal, dlFmt);
                        var labelWidth = (float)MeasureHelveticaWidth(labelText, axisFontSize, true);
                        var barCenterX = Math.Min(x0, x1) + barDrawW / 2f - labelWidth / 2f;
                        var barCenterY = barY + barHeight / 2f - axisFontSize * 0.35f;
                        // Use white text for large segments, black for small
                        var isLargeSeg = barDrawW > plotWidth * 0.25f;
                        page.AddText(labelText, barCenterX, barCenterY, axisFontSize,
                            isLargeSeg ? new PdfColor(1f, 1f, 1f) : new PdfColor(0, 0, 0), bold: true);
                    }
                }
            }
            else
            {
                // Clustered: bars side by side
                for (var si = 0; si < numSeries; si++)
                {
                    var val = si < numSeries && ci < series[si].Values.Length
                        ? series[si].Values[ci] : 0;
                    var barY = groupY + si * barHeight;
                    var valX = plotLeft + (float)((val - niceMin) / range) * plotWidth;
                    var barLeft = Math.Min(valX, baselineX);
                    var barDrawW = Math.Abs(valX - baselineX);
                    if (barDrawW < 0.5f) barDrawW = 0.5f;

                    var color = ChartColors[si % ChartColors.Length];
                    page.AddRectangle(barLeft, barY, barDrawW, barHeight, color);

                    // Data label
                    if (showVal && barDrawW > 10)
                    {
                        var labelText = FormatDataLabel(val, dlFmt);
                        var labelWidth = (float)MeasureHelveticaWidth(labelText, axisFontSize, true);
                        var barCenterX = barLeft + barDrawW / 2f - labelWidth / 2f;
                        var barCenterY = barY + barHeight / 2f - axisFontSize * 0.35f;
                        page.AddText(labelText, barCenterX, barCenterY, axisFontSize, bold: true);
                    }
                }
            }

            // Category label (on Y-axis, left side)
            if (ci < categories.Length)
            {
                var label = TruncateLabel(categories[ci], 12);
                var labelY = plotBottom + (ci + 0.5f) * groupHeight - axisFontSize * 0.3f;
                page.AddText(label, plotLeft - label.Length * axisFontSize * 0.5f - 4f, labelY, axisFontSize, textColor, preferredFontName: fontName);
            }
        }

        // Draw axes
        page.AddLine(plotLeft, plotBottom, plotLeft, plotBottom + plotHeight,
            new PdfColor(0, 0, 0), 0.8f);
        if (!isPercentStacked)
            page.AddLine(plotLeft, plotBottom, plotLeft + plotWidth, plotBottom,
                new PdfColor(0, 0, 0), 0.8f);

        RenderLegend(page, series, plotLeft + plotWidth * 0.05f, plotBottom + plotHeight + 5f, axisFontSize, isStacked, textColor, fontName);
    }

    /// <summary>Renders a line chart.</summary>
    private static void RenderLineChart(PdfPage page, ExcelChartInfo chart,
        float plotLeft, float plotBottom, float plotWidth, float plotHeight,
        float labelFontSize, float axisFontSize, bool skipAxisLabels = false, string axisFmtCode = "",
        PdfColor? textColor = null, string? fontName = null)
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
                page.AddText(label, plotLeft - label.Length * axisFontSize * 0.5f - 4f, gridY - axisFontSize * 0.3f, axisFontSize, textColor, preferredFontName: fontName);
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
                page.AddText(label, xPos - axisFontSize, plotBottom - axisFontSize * 1.5f, axisFontSize, textColor, preferredFontName: fontName);
            }
        }

        // Axes
        page.AddLine(plotLeft, plotBottom, plotLeft, plotBottom + plotHeight,
            new PdfColor(0, 0, 0), 0.8f);
        page.AddLine(plotLeft, plotBottom, plotLeft + plotWidth, plotBottom,
            new PdfColor(0, 0, 0), 0.8f);

        RenderLegend(page, series, plotLeft + plotWidth * 0.05f, plotBottom + plotHeight + 5f, axisFontSize, textColor: textColor, fontName: fontName);
    }

    /// <summary>Renders a scatter (XY) chart or bubble chart with numeric X and Y axes.</summary>
    private static void RenderScatterChart(PdfPage page, ExcelChartInfo chart,
        float plotLeft, float plotBottom, float plotWidth, float plotHeight,
        float labelFontSize, float axisFontSize, string axisFmtCode = "",
        PdfColor? textColor = null, string? fontName = null)
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
            page.AddText(label, plotLeft - label.Length * axisFontSize * 0.5f - 4f, gridY - axisFontSize * 0.3f, axisFontSize, textColor, preferredFontName: fontName);
        }

        // X-axis tick labels
        for (var tickVal = xMin; tickVal <= xMax + xStep * 0.01; tickVal += xStep)
        {
            var gridX = plotLeft + (float)((tickVal - xMin) / xRange) * plotWidth;
            page.AddLine(gridX, plotBottom, gridX, plotBottom + plotHeight,
                new PdfColor(0.85f, 0.85f, 0.85f), 0.5f);
            var label = FormatAxisValue(tickVal, axisFmtCode);
            page.AddText(label, gridX - axisFontSize * 0.5f, plotBottom - axisFontSize * 1.5f, axisFontSize, textColor, preferredFontName: fontName);
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
            page.AddText(serName, legendX + 10, legendY, axisFontSize, textColor, preferredFontName: fontName);
            legendX += (serName.Length + 3) * axisFontSize * 0.5f;
        }
    }

    /// <summary>Renders an area chart (filled line chart) using vertical strips to approximate fill.</summary>
    private static void RenderAreaChart(PdfPage page, ExcelChartInfo chart,
        float plotLeft, float plotBottom, float plotWidth, float plotHeight,
        float labelFontSize, float axisFontSize, string axisFmtCode = "",
        PdfColor? textColor = null, string? fontName = null)
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
            page.AddText(label, plotLeft - label.Length * axisFontSize * 0.5f - 4f, gridY - axisFontSize * 0.3f, axisFontSize, textColor, preferredFontName: fontName);
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
            page.AddText(label, xPos - axisFontSize, plotBottom - axisFontSize * 1.5f, axisFontSize, textColor, preferredFontName: fontName);
        }

        // Axes
        page.AddLine(plotLeft, plotBottom, plotLeft, plotBottom + plotHeight,
            new PdfColor(0, 0, 0), 0.8f);
        page.AddLine(plotLeft, plotBottom, plotLeft + plotWidth, plotBottom,
            new PdfColor(0, 0, 0), 0.8f);

        // Legend (reversed for stacked charts)
        RenderLegend(page, series, plotLeft + plotWidth * 0.05f, plotBottom + plotHeight + 5f, axisFontSize, isStacked, textColor, fontName);
    }

    /// <summary>Renders a pie or doughnut chart using rectangles to approximate sectors.</summary>
    private static void RenderPieChart(PdfPage page, ExcelChartInfo chart,
        float chartX, float chartTop, float chartWidth, float chartHeight,
        float plotLeft, float plotBottom, float plotWidth, float plotHeight,
        float labelFontSize, bool isDoughnut, bool showPercent,
        PdfColor? textColor = null, string? fontName = null)
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
                // Use simple "Category Value" format to match LibreOffice text extraction
                var labelText = $"{catName}; {seriesName}; {valStr}; {pct}%";
                page.AddText(TruncateLabel(labelText, 30), lx, ly, labelFontSize - 1, textColor, preferredFontName: fontName);
            }
        }

        // Legend: vertical list of category names with color swatches below the pie
        var legendY = plotBottom - 10f;
        var legendTextX = plotLeft + plotWidth * 0.55f + 10f;
        for (var i = 0; i < values.Length; i++)
        {
            var color = ChartColors[i % ChartColors.Length];
            page.AddRectangle(plotLeft + plotWidth * 0.55f, legendY, 8, 8, color);
            var legendName = i < categories.Length ? categories[i] : $"Slice{i + 1}";
            page.AddText(legendName, legendTextX, legendY, labelFontSize, textColor, preferredFontName: fontName);
            legendY -= labelFontSize * 1.5f;
        }
    }

    /// <summary>Renders legend entries for chart series.</summary>
    private static void RenderLegend(PdfPage page, List<ExcelChartSeries> series,
        float x, float y, float fontSize, bool reverseOrder = false,
        PdfColor? textColor = null, string? fontName = null)
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
            page.AddText(name, legendX + 10, y, fontSize, textColor, preferredFontName: fontName);
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

    /// <summary>Formats a data label value using the chart's data label format code.</summary>
    private static string FormatDataLabel(double val, string formatCode)
    {
        if (string.IsNullOrEmpty(formatCode))
            return val == Math.Floor(val) ? $"{val:F0}" : $"{val:F1}";

        // Handle "$"#,##0 pattern (quoted dollar prefix + comma grouping)
        var prefix = "";
        var suffix = "";
        var code = formatCode;
        // Extract quoted prefix (e.g., "$")
        if (code.StartsWith("\"") || code.StartsWith("\""))
        {
            var closeIdx = code.IndexOf('"', 1);
            if (closeIdx > 1)
            {
                prefix = code.Substring(1, closeIdx - 1);
                code = code[(closeIdx + 1)..];
            }
        }
        // Extract quoted suffix
        if (code.EndsWith("\"") || code.EndsWith("\""))
        {
            var openIdx = code.LastIndexOf('"', code.Length - 2);
            if (openIdx >= 0)
            {
                suffix = code.Substring(openIdx + 1, code.Length - openIdx - 2);
                code = code[..openIdx];
            }
        }

        // Count decimal places from format
        var decimalPlaces = 0;
        var dotIdx = code.IndexOf('.');
        if (dotIdx >= 0)
        {
            for (var i = dotIdx + 1; i < code.Length && code[i] == '0'; i++)
                decimalPlaces++;
        }

        string formatted;
        if (code.Contains("#,##") || code.Contains("0,0"))
            formatted = val.ToString($"N{decimalPlaces}", System.Globalization.CultureInfo.InvariantCulture);
        else
            formatted = val.ToString($"F{decimalPlaces}", System.Globalization.CultureInfo.InvariantCulture);

        return prefix + formatted + suffix;
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
            return int.TryParse(name.Substring(5), out _);
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
        // Scale Helvetica character widths to approximate Calibri metrics.
        double used = 0;
        const double scale = CalibriFittingScale;
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

    /// <summary>Returns Helvetica-Bold character width in 1/1000 em units.</summary>
    private static int HelveticaBoldCharWidth(char ch) => ch switch
    {
        ' ' => 278, '!' => 333, '"' => 474, '#' => 556, '$' => 556, '%' => 889,
        '&' => 722, '\'' => 238, '(' => 333, ')' => 333, '*' => 389, '+' => 584,
        ',' => 278, '-' => 333, '.' => 278, '/' => 278,
        >= '0' and <= '9' => 556,
        ':' => 333, ';' => 333, '<' => 584, '=' => 584, '>' => 584, '?' => 611,
        '@' => 975,
        'A' => 722, 'B' => 722, 'C' => 722, 'D' => 722, 'E' => 667, 'F' => 611,
        'G' => 778, 'H' => 722, 'I' => 278, 'J' => 556, 'K' => 722, 'L' => 611,
        'M' => 833, 'N' => 722, 'O' => 778, 'P' => 667, 'Q' => 778, 'R' => 722,
        'S' => 667, 'T' => 611, 'U' => 722, 'V' => 667, 'W' => 944, 'X' => 667,
        'Y' => 667, 'Z' => 611,
        '[' => 333, '\\' => 278, ']' => 333, '^' => 584, '_' => 556, '`' => 333,
        'a' => 556, 'b' => 611, 'c' => 556, 'd' => 611, 'e' => 556, 'f' => 333,
        'g' => 611, 'h' => 611, 'i' => 278, 'j' => 278, 'k' => 556, 'l' => 278,
        'm' => 889, 'n' => 611, 'o' => 611, 'p' => 611, 'q' => 611, 'r' => 389,
        's' => 556, 't' => 333, 'u' => 611, 'v' => 556, 'w' => 778, 'x' => 556,
        'y' => 556, 'z' => 500,
        '{' => 389, '|' => 280, '}' => 389, '~' => 584,
        _ => IsFullWidthChar(ch) ? 1000 : 556
    };

    /// <summary>Calibri-to-Helvetica scale factor used by truncation and fitting functions.</summary>
    private const double CalibriFittingScale = 0.85;

    /// <summary>
    /// Returns true when a cell carries visible text or borders.
    /// Used to decide whether a row/column should be preserved
    /// rather than trimmed as empty trailing space.
    /// Fills are excluded because many sheets apply a background fill to all cells.
    /// </summary>
    private static bool CellHasContentOrStyle(ExcelCell cell)
    {
        if (!string.IsNullOrEmpty(cell.Text))
            return true;
        if (cell.Border is { } b)
        {
            if (b.Left is { Style: not "none" and not "" })
                return true;
            if (b.Right is { Style: not "none" and not "" })
                return true;
            if (b.Top is { Style: not "none" and not "" })
                return true;
            if (b.Bottom is { Style: not "none" and not "" })
                return true;
        }
        return false;
    }

    private static bool ShouldUsePdfBold(bool requestedBold, float fontSize, string? fontName = null)
    {
        if (!requestedBold)
            return false;

        // When the original font name already contains a weight keyword
        // (e.g. "Franklin Gothic Medium", "Arial Black"), the bold attribute
        // in the spreadsheet merely selects that weight – it does not mean
        // the text should be rendered with Helvetica-Bold.  Suppress PDF bold
        // so that the fallback Helvetica matches the visual weight of the
        // original font more closely.
        if (!string.IsNullOrEmpty(fontName))
        {
            var fn = fontName.ToUpperInvariant();
            if (fn.Contains("MEDIUM") || fn.Contains("SEMIBOLD") || fn.Contains("DEMIBOLD") ||
                fn.Contains("BLACK") || fn.Contains("HEAVY") || fn.Contains("LIGHT") || fn.Contains("THIN"))
                return false;
        }

        // Built-in Helvetica-Bold appears heavier than spreadsheet renderer output
        // for very large headings, so keep large titles in regular weight.
        // When a Unicode/CJK font is embedded, bold is simulated via stroke;
        // this does not produce the same heavy visual, so allow larger sizes.
        return fontSize <= 40f;
    }

    /// <summary>
    /// Maps OOXML border style names to PDF line widths (in points).
    /// </summary>
    private static float BorderStyleWidth(string style) => style switch
    {
        "thick" => 1.5f,
        "medium" or "mediumDashed" or "mediumDashDot" or "mediumDashDotDot" => 1f,
        "dotted" or "hair" => 0.1f,
        _ => 0.3f // thin, dashed, dashDot, dashDotDot, double, slantDashDot
    };

    /// <summary>
    /// Maps OOXML border style names to PDF dash patterns.
    /// Returns null for solid lines.
    /// </summary>
    private static float[]? BorderDashPattern(string style, float lineWidth) => style switch
    {
        "dotted" => new[] { 0.2f, 0.8f },
        "dashed" or "mediumDashed" => new[] { 4f, 2f },
        "dashDot" or "mediumDashDot" or "slantDashDot" => new[] { 4f, 1.5f, Math.Max(0.5f, lineWidth), 1.5f },
        "dashDotDot" or "mediumDashDotDot" => new[] { 4f, 1f, Math.Max(0.5f, lineWidth), 1f, Math.Max(0.5f, lineWidth), 1f },
        "hair" => new[] { 0.5f, 0.5f },
        _ => null // solid: thin, medium, thick
    };

    /// <summary>
    /// Measures text width more precisely using Helvetica character widths (in 1/1000 em units).
    /// Used for column-width-aware number formatting.
    /// </summary>
    private static double MeasureHelveticaWidth(string text, double fontSize, bool bold = false)
    {
        double total = 0;
        foreach (var ch in text)
            total += bold ? HelveticaBoldCharWidth(ch) : HelveticaCharWidth(ch);
        return total * fontSize / 1000.0;
    }

    /// <summary>
    /// Measures text width using Helvetica widths scaled by the Calibri fitting factor.
    /// Matches the same metric used by <see cref="FittingChars"/> for consistency.
    /// </summary>
    private static double MeasureScaledWidth(string text, double fontSize)
    {
        double total = 0;
        foreach (var ch in text)
            total += HelveticaCharWidth(ch);
        return total * fontSize / 1000.0 * CalibriFittingScale;
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

        // Use Calibri-scaled widths (matching FittingChars) so the "fits" check
        // is consistent with the truncation logic. A margin of ~3pt accounts for
        // sub-pixel differences between Calibri and scaled-Helvetica glyph widths,
        // matching LibreOffice's precision reduction behavior more closely.
        var textAreaWidth = colWidthPt - 3.0;

        // Check if current text already fits
        if (MeasureScaledWidth(text, fontSize) <= textAreaWidth)
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
                if (MeasureScaledWidth(dec, fontSize) <= textAreaWidth)
                    return dec;
            }
            // Try integer form
            var intForm = Math.Round(value).ToString("F0", ci);
            if (MeasureScaledWidth(intForm, fontSize) <= textAreaWidth)
                return intForm;
        }

        // Try scientific notation with decreasing precision until it fits
        for (int digits = 3; digits >= 0; digits--)
        {
            var fmt = digits > 0 ? "0." + new string('#', digits) + "E+00" : "0E+00";
            var sci = value.ToString(fmt, ci);
            if (MeasureScaledWidth(sci, fontSize) <= textAreaWidth)
                return sci;
        }

        return text; // Can't fit, return as-is
    }

    /// <summary>
    /// Calculates natural (unscaled) column widths with min/max bounds.
    /// Quickly estimate the total unscaled column width in points for fitToHeight
    /// interaction with fitToPage.  Uses print area column bounds when available.
    /// </summary>
    private static float EstimateColumnWidthTotal(ExcelSheet sheet, ConversionOptions options)
    {
        var startCol = sheet.PrintArea.HasValue ? sheet.PrintArea.Value.StartCol : 0;
        var endCol = sheet.PrintArea.HasValue
            ? sheet.PrintArea.Value.EndCol
            : (sheet.Rows.Count > 0 ? sheet.Rows.Max(r => r.Count) - 1 : 0);
        var total = 0f;
        for (var c = startCol; c <= endCol; c++)
        {
            if (sheet.ColumnWidths.TryGetValue(c, out var ew))
                total += ew > 0 ? sheet.CharUnitsToPointsScaled(ew) : 0f;
            else
            {
                var charUnits = sheet.DefaultColumnWidth > 0f ? sheet.DefaultColumnWidth : 8.43f;
                total += sheet.CharUnitsToPointsDefaultScaled(charUnits);
            }
        }
        return total;
    }

    /// <summary>
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
                var hasExplicitWidth = sheet.ColumnWidths.TryGetValue(i, out var ew);
                var charUnits = hasExplicitWidth
                    ? ew
                    : sheet.DefaultColumnWidth > 0f ? sheet.DefaultColumnWidth : 8.43f;
                // Hidden columns (width 0): skip entirely
                if (charUnits <= 0f)
                {
                    widths[i] = 0f;
                    continue;
                }
                var excelPts = hasExplicitWidth
                    ? sheet.CharUnitsToPointsScaled(charUnits)
                    : sheet.CharUnitsToPointsDefaultScaled(charUnits);
                // When the spreadsheet explicitly sets a narrow column width,
                // honour it (spacer columns etc.).  Only apply minColWidth for
                // columns using the default/fallback width.
                // Do not cap explicit widths — LibreOffice honours the author's
                // column widths as-is, so the column-grouping boundary matches.
                var floor = hasExplicitWidth ? 0f : minColWidth;
                widths[i] = hasExplicitWidth
                    ? Math.Max(excelPts, 0f)
                    : Compat.Clamp(excelPts, floor, maxColWidth);
            }
            else if (maxCols == 1)
            {
                // Single-column sheet: use content-based width so the column fills the page
                // (LibreOffice expands 1-column sheets to page width).
                var natural = colMaxWidthPts[i] + 2 * avgCharWidth;
                natural = Math.Max(natural, 5 * avgCharWidth); // minimum 5 chars
                widths[i] = Compat.Clamp(natural, minColWidth, maxColWidth);
            }
            else
            {
                // No explicit column widths — use Excel's default column width (8.43
                // char units) like LibreOffice does.  Text that exceeds the column
                // boundary is clipped in the rendering step (shouldClip logic).
                var defaultPts = ExcelSheet.CharUnitsToPoints(8.43f);
                widths[i] = Compat.Clamp(defaultPts, minColWidth, maxColWidth);
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
