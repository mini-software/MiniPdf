using System.IO.Compression;
using System.Xml.Linq;

namespace MiniSoftware;

/// <summary>
/// Reads basic text data from Excel (.xlsx) files.
/// Supports reading cell values (strings and numbers) without external dependencies.
/// </summary>
internal static class ExcelReader
{
    /// <summary>
    /// Reads all sheets from an Excel file and returns their data as a list of sheets,
    /// where each sheet is a list of rows, and each row is a list of cell values.
    /// </summary>
    internal static List<ExcelSheet> ReadSheets(Stream stream)
    {
        var sheets = new List<ExcelSheet>();

        using var archive = new ZipArchive(stream, ZipArchiveMode.Read, leaveOpen: true);

        // Read shared strings table
        var sharedStrings = ReadSharedStrings(archive);

        // Read styles (font colors)
        var fontColors = ReadFontColors(archive);
        var cellXfFontIndices = ReadCellXfFontIndices(archive);

        // Read workbook to get sheet names and order
        var sheetInfos = ReadWorkbook(archive);

        // Read each sheet
        foreach (var info in sheetInfos)
        {
            var entry = archive.GetEntry($"xl/worksheets/sheet{info.SheetId}.xml")
                        ?? archive.GetEntry($"xl/worksheets/{info.Name}.xml");

            // Try by relationship id pattern
            entry ??= archive.Entries.FirstOrDefault(e =>
                e.FullName.StartsWith("xl/worksheets/", StringComparison.OrdinalIgnoreCase) &&
                e.FullName.EndsWith(".xml", StringComparison.OrdinalIgnoreCase));

            if (entry == null) continue;

            var rows = ReadSheet(entry, sharedStrings, fontColors, cellXfFontIndices);
            var images = ReadSheetImages(archive, info.SheetId);
            var (colWidths, defaultColWidth) = ReadColumnWidths(entry);
            sheets.Add(new ExcelSheet(info.Name, rows, images, colWidths, defaultColWidth));
        }

        // If no sheets found via workbook, try reading sheet1 directly
        if (sheets.Count == 0)
        {
            var entry = archive.GetEntry("xl/worksheets/sheet1.xml");
            if (entry != null)
            {
                var rows = ReadSheet(entry, sharedStrings, fontColors, cellXfFontIndices);
                var images = ReadSheetImages(archive, 1);
                var (colWidths, defaultColWidth) = ReadColumnWidths(entry);
                sheets.Add(new ExcelSheet("Sheet1", rows, images, colWidths, defaultColWidth));
            }
        }

        // Second pass: read charts (needs sheet data to resolve cell references)
        for (var si = 0; si < sheets.Count; si++)
        {
            var sheetId = si < sheetInfos.Count ? sheetInfos[si].SheetId : 1;
            var charts = ReadSheetCharts(archive, sheetId, sheets);
            foreach (var chart in charts)
                sheets[si].Charts.Add(chart);
        }

        return sheets;
    }

    private static List<string> ReadSharedStrings(ZipArchive archive)
    {
        var strings = new List<string>();
        var entry = archive.GetEntry("xl/sharedStrings.xml");
        if (entry == null) return strings;

        using var stream = entry.Open();
        var doc = XDocument.Load(stream);
        var ns = doc.Root?.GetDefaultNamespace() ?? XNamespace.None;

        foreach (var si in doc.Descendants(ns + "si"))
        {
            // Handle both simple <t> and rich text <r><t> patterns
            var text = string.Concat(si.Descendants(ns + "t").Select(t => t.Value));
            strings.Add(text);
        }

        return strings;
    }

    private static List<SheetInfo> ReadWorkbook(ZipArchive archive)
    {
        var result = new List<SheetInfo>();
        var entry = archive.GetEntry("xl/workbook.xml");
        if (entry == null) return result;

        using var stream = entry.Open();
        var doc = XDocument.Load(stream);
        var ns = doc.Root?.GetDefaultNamespace() ?? XNamespace.None;

        var sheetId = 1;
        foreach (var sheet in doc.Descendants(ns + "sheet"))
        {
            var name = sheet.Attribute("name")?.Value ?? $"Sheet{sheetId}";
            result.Add(new SheetInfo(name, sheetId));
            sheetId++;
        }

        return result;
    }

    private static List<PdfColor?> ReadFontColors(ZipArchive archive)
    {
        var colors = new List<PdfColor?>();
        var entry = archive.GetEntry("xl/styles.xml");
        if (entry == null) return colors;

        using var stream = entry.Open();
        var doc = XDocument.Load(stream);
        var ns = doc.Root?.GetDefaultNamespace() ?? XNamespace.None;

        // Read <fonts> -> <font> elements
        var fontsElement = doc.Descendants(ns + "fonts").FirstOrDefault();
        if (fontsElement == null) return colors;

        foreach (var font in fontsElement.Elements(ns + "font"))
        {
            var colorEl = font.Element(ns + "color");
            if (colorEl == null)
            {
                colors.Add(null);
                continue;
            }

            // Try rgb attribute (ARGB hex, e.g., "FF0000FF")
            var rgb = colorEl.Attribute("rgb")?.Value;
            if (!string.IsNullOrEmpty(rgb))
            {
                colors.Add(PdfColor.FromHex(rgb));
                continue;
            }

            // Try theme attribute (would need theme parsing - skip for now)
            // Try indexed attribute
            var indexed = colorEl.Attribute("indexed")?.Value;
            if (!string.IsNullOrEmpty(indexed) && int.TryParse(indexed, out var idx))
            {
                colors.Add(GetIndexedColor(idx));
                continue;
            }

            colors.Add(null);
        }

        return colors;
    }

    private static List<int> ReadCellXfFontIndices(ZipArchive archive)
    {
        var indices = new List<int>();
        var entry = archive.GetEntry("xl/styles.xml");
        if (entry == null) return indices;

        using var stream = entry.Open();
        var doc = XDocument.Load(stream);
        var ns = doc.Root?.GetDefaultNamespace() ?? XNamespace.None;

        // Read <cellXfs> -> <xf> elements to map style index -> font index
        var cellXfs = doc.Descendants(ns + "cellXfs").FirstOrDefault();
        if (cellXfs == null) return indices;

        foreach (var xf in cellXfs.Elements(ns + "xf"))
        {
            var fontId = xf.Attribute("fontId")?.Value;
            indices.Add(int.TryParse(fontId, out var fid) ? fid : 0);
        }

        return indices;
    }

    private static PdfColor? GetIndexedColor(int index)
    {
        // Standard Excel indexed colors (subset of the 64 built-in colors)
        return index switch
        {
            0 => PdfColor.FromRgb(0, 0, 0),        // Black
            1 => PdfColor.FromRgb(255, 255, 255),   // White
            2 => PdfColor.FromRgb(255, 0, 0),       // Red
            3 => PdfColor.FromRgb(0, 255, 0),       // Green
            4 => PdfColor.FromRgb(0, 0, 255),       // Blue
            5 => PdfColor.FromRgb(255, 255, 0),     // Yellow
            6 => PdfColor.FromRgb(255, 0, 255),     // Magenta
            7 => PdfColor.FromRgb(0, 255, 255),     // Cyan
            8 => PdfColor.FromRgb(0, 0, 0),         // Black
            9 => PdfColor.FromRgb(255, 255, 255),   // White
            10 => PdfColor.FromRgb(255, 0, 0),      // Red
            11 => PdfColor.FromRgb(0, 255, 0),      // Green
            12 => PdfColor.FromRgb(0, 0, 255),      // Blue
            13 => PdfColor.FromRgb(255, 255, 0),    // Yellow
            14 => PdfColor.FromRgb(255, 0, 255),    // Magenta
            15 => PdfColor.FromRgb(0, 255, 255),    // Cyan
            16 => PdfColor.FromRgb(128, 0, 0),      // Dark Red
            17 => PdfColor.FromRgb(0, 128, 0),      // Dark Green
            18 => PdfColor.FromRgb(0, 0, 128),      // Dark Blue
            19 => PdfColor.FromRgb(128, 128, 0),    // Olive
            20 => PdfColor.FromRgb(128, 0, 128),    // Purple
            21 => PdfColor.FromRgb(0, 128, 128),    // Teal
            22 => PdfColor.FromRgb(192, 192, 192),  // Silver
            23 => PdfColor.FromRgb(128, 128, 128),  // Grey
            _ => null
        };
    }

    private static PdfColor? ResolveCellColor(int styleIndex, List<PdfColor?> fontColors, List<int> cellXfFontIndices)
    {
        if (styleIndex < 0 || styleIndex >= cellXfFontIndices.Count)
            return null;

        var fontIndex = cellXfFontIndices[styleIndex];
        if (fontIndex < 0 || fontIndex >= fontColors.Count)
            return null;

        return fontColors[fontIndex];
    }

    private static List<List<ExcelCell>> ReadSheet(ZipArchiveEntry entry, List<string> sharedStrings, List<PdfColor?> fontColors, List<int> cellXfFontIndices)
    {
        var rows = new List<List<ExcelCell>>();

        using var stream = entry.Open();
        var doc = XDocument.Load(stream);
        var ns = doc.Root?.GetDefaultNamespace() ?? XNamespace.None;

        var lastRowNumber = 0;

        foreach (var row in doc.Descendants(ns + "row"))
        {
            // Parse the row number to detect gaps (sparse rows)
            var rowNumAttr = row.Attribute("r")?.Value;
            if (int.TryParse(rowNumAttr, out var rowNumber))
            {
                // Insert empty rows for any gaps
                while (lastRowNumber + 1 < rowNumber)
                {
                    rows.Add(new List<ExcelCell>());
                    lastRowNumber++;
                }
                lastRowNumber = rowNumber;
            }
            else
            {
                lastRowNumber++;
            }

            var cells = new List<ExcelCell>();
            var lastColIndex = 0;

            foreach (var cell in row.Elements(ns + "c"))
            {
                // Parse column reference to handle gaps (e.g., A1, C1 means B is empty)
                var reference = cell.Attribute("r")?.Value ?? "";
                var colIndex = ParseColumnIndex(reference);

                // Fill empty cells for gaps
                while (lastColIndex < colIndex)
                {
                    cells.Add(new ExcelCell(string.Empty, null));
                    lastColIndex++;
                }

                var type = cell.Attribute("t")?.Value;
                var value = cell.Element(ns + "v")?.Value ?? "";

                // Resolve color from style index
                var styleAttr = cell.Attribute("s")?.Value;
                PdfColor? color = null;
                if (int.TryParse(styleAttr, out var styleIndex))
                {
                    color = ResolveCellColor(styleIndex, fontColors, cellXfFontIndices);
                }

                string text;
                if (type == "s" && int.TryParse(value, out var idx) && idx < sharedStrings.Count)
                {
                    text = sharedStrings[idx];
                }
                else if (type == "inlineStr")
                {
                    text = string.Concat(cell.Descendants(ns + "t").Select(t => t.Value));
                }
                else if (type == "b")
                {
                    // Boolean: Excel stores "1"/"0", render as TRUE/FALSE to match LibreOffice
                    text = value == "1" ? "TRUE" : "FALSE";
                }
                else
                {
                    text = value;

                    // Normalize floating-point representation for numeric cells
                    if (string.IsNullOrEmpty(type) || type == "n")
                    {
                        if (!string.IsNullOrEmpty(text) &&
                            double.TryParse(text, System.Globalization.NumberStyles.Any,
                                System.Globalization.CultureInfo.InvariantCulture, out var numVal))
                        {
                            text = numVal.ToString("G15", System.Globalization.CultureInfo.InvariantCulture);
                        }
                    }


                }

                cells.Add(new ExcelCell(text, color));
                lastColIndex = colIndex + 1;
            }

            rows.Add(cells);
        }

        return rows;
    }

    private static int ParseColumnIndex(string cellReference)
    {
        var col = 0;
        foreach (var c in cellReference)
        {
            if (char.IsLetter(c))
            {
                col = col * 26 + (char.ToUpper(c) - 'A' + 1);
            }
            else
            {
                break;
            }
        }
        return col > 0 ? col - 1 : 0;
    }

    internal record SheetInfo(string Name, int SheetId);

    /// <summary>
    /// Reads column widths from a worksheet entry.
    /// Returns (columnWidths dict, defaultColumnWidth) where widths are in Excel character units.
    /// Only explicitly customised columns (customWidth="1") or an explicit defaultColWidth
    /// attribute on sheetFormatPr contribute; otherwise the dict/default remain at 0.
    /// </summary>
    private static (Dictionary<int, float> widths, float defaultWidth) ReadColumnWidths(ZipArchiveEntry entry)
    {
        var widths = new Dictionary<int, float>();
        var defaultWidth = 0f; // 0 = "not set explicitly"

        using var stream = entry.Open();
        var doc = XDocument.Load(stream);
        var ns = doc.Root?.GetDefaultNamespace() ?? XNamespace.None;

        // Only read defaultColWidth when the attribute is EXPLICITLY written by the author
        var fmtPr = doc.Descendants(ns + "sheetFormatPr").FirstOrDefault();
        if (fmtPr?.Attribute("defaultColWidth") != null)
        {
            var dcw = fmtPr.Attribute("defaultColWidth")!.Value;
            if (float.TryParse(dcw,
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture,
                out var parsed) && parsed > 0f)
            {
                defaultWidth = parsed;
            }
        }

        // Only use column widths that are explicitly customized (customWidth="1")
        foreach (var col in doc.Descendants(ns + "col"))
        {
            // Skip default-width columns (not customised by the spreadsheet author)
            var customWidth = col.Attribute("customWidth")?.Value;
            if (customWidth != "1") continue;

            var minAttr = col.Attribute("min")?.Value;
            var maxAttr = col.Attribute("max")?.Value;
            var widthAttr = col.Attribute("width")?.Value;
            if (minAttr == null || widthAttr == null) continue;

            if (!int.TryParse(minAttr, out var minCol)) continue;
            if (!int.TryParse(maxAttr ?? minAttr, out var maxCol)) continue;
            if (!float.TryParse(widthAttr,
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture,
                out var colWidth)) continue;

            for (var c = minCol; c <= maxCol; c++)
                widths[c - 1] = colWidth; // store as 0-based index
        }

        return (widths, defaultWidth);
    }

    /// <summary>
    /// Reads all images embedded in a given worksheet.
    /// Returns a list of ExcelEmbeddedImage with anchor positions and raw image bytes.
    /// </summary>
    private static List<ExcelEmbeddedImage> ReadSheetImages(ZipArchive archive, int sheetId)
    {
        var images = new List<ExcelEmbeddedImage>();

        // Step 1: Find the sheet relationships file to locate the drawing
        var sheetRelsPath = $"xl/worksheets/_rels/sheet{sheetId}.xml.rels";
        var relsEntry = archive.GetEntry(sheetRelsPath);
        if (relsEntry == null) return images;

        string? drawingFileName = null;
        using (var relsStream = relsEntry.Open())
        {
            var relsDoc = XDocument.Load(relsStream);
            var drawingRel = relsDoc.Descendants()
                .FirstOrDefault(el =>
                    el.Attribute("Type")?.Value.EndsWith("/drawing", StringComparison.OrdinalIgnoreCase) == true);
            if (drawingRel == null) return images;
            var target = drawingRel.Attribute("Target")?.Value;
            if (string.IsNullOrEmpty(target)) return images;
            // Target like "../drawings/drawing1.xml" → filename = "drawing1.xml"
            drawingFileName = System.IO.Path.GetFileName(target);
        }

        var drawingPath = $"xl/drawings/{drawingFileName}";
        var drawingEntry = archive.GetEntry(drawingPath);
        if (drawingEntry == null) return images;

        // Step 2: Read drawing relationships to map rId → media path
        var drawingRelsPath = $"xl/drawings/_rels/{drawingFileName}.rels";
        var drawingRelsEntry = archive.GetEntry(drawingRelsPath);
        if (drawingRelsEntry == null) return images;

        var rIdToMedia = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        using (var drStream = drawingRelsEntry.Open())
        {
            var drDoc = XDocument.Load(drStream);
            foreach (var rel in drDoc.Descendants())
            {
                var id = rel.Attribute("Id")?.Value;
                var target = rel.Attribute("Target")?.Value;
                if (id == null || string.IsNullOrEmpty(target)) continue;

                // Target may be an absolute pack URI (leading '/') or relative to xl/drawings/.
                // Absolute:  "/xl/media/image1.jpeg" → strip '/' → "xl/media/image1.jpeg"
                // Relative:  "../media/image1.jpg"  → resolve → "xl/media/image1.jpg"
                string zipPath;
                if (target.StartsWith('/'))
                {
                    zipPath = target.TrimStart('/');
                }
                else
                {
                    var segments = ("xl/drawings/" + target).Split('/');
                    var resolved = new System.Collections.Generic.Stack<string>();
                    foreach (var seg in segments)
                    {
                        if (seg == "..") { if (resolved.Count > 0) resolved.Pop(); }
                        else if (seg != "." && seg != "") resolved.Push(seg);
                    }
                    zipPath = string.Join("/", resolved.Reverse());
                }
                rIdToMedia[id] = zipPath;
            }
        }

        // Step 3: Parse the drawing XML for image anchors
        using var dStream = drawingEntry.Open();
        var dDoc = XDocument.Load(dStream);

        var xdr = XNamespace.Get("http://schemas.openxmlformats.org/drawingml/2006/spreadsheetDrawing");
        var a = XNamespace.Get("http://schemas.openxmlformats.org/drawingml/2006/main");
        var r = XNamespace.Get("http://schemas.openxmlformats.org/officeDocument/2006/relationships");

        var anchors = dDoc.Descendants(xdr + "twoCellAnchor")
            .Concat(dDoc.Descendants(xdr + "oneCellAnchor"))
            .Concat(dDoc.Descendants(xdr + "absoluteAnchor"));

        foreach (var anchor in anchors)
        {
            var fromEl = anchor.Element(xdr + "from");
            var toEl = anchor.Element(xdr + "to");
            var extEl = anchor.Element(xdr + "ext");

            int fromRow = 0, fromCol = 0, toRow = 1, toCol = 1;
            if (fromEl != null)
            {
                int.TryParse(fromEl.Element(xdr + "row")?.Value, out fromRow);
                int.TryParse(fromEl.Element(xdr + "col")?.Value, out fromCol);
            }
            if (toEl != null)
            {
                int.TryParse(toEl.Element(xdr + "row")?.Value, out toRow);
                int.TryParse(toEl.Element(xdr + "col")?.Value, out toCol);
            }

            // For oneCellAnchor / absoluteAnchor, read EMU size from <ext cx cy>.
            long widthEmu = 0, heightEmu = 0;
            if (extEl != null)
            {
                long.TryParse(extEl.Attribute("cx")?.Value, out widthEmu);
                long.TryParse(extEl.Attribute("cy")?.Value, out heightEmu);
            }

            // Find the blip (image reference)
            var blip = anchor.Descendants(a + "blip").FirstOrDefault();
            if (blip == null) continue;

            var rId = blip.Attribute(r + "embed")?.Value;
            if (string.IsNullOrEmpty(rId)) continue;
            if (!rIdToMedia.TryGetValue(rId, out var mediaPath)) continue;

            var mediaEntry = archive.GetEntry(mediaPath);
            if (mediaEntry == null) continue;

            byte[] imageData;
            using (var ms = new System.IO.MemoryStream())
            {
                using var imgStream = mediaEntry.Open();
                imgStream.CopyTo(ms);
                imageData = ms.ToArray();
            }

            var ext = System.IO.Path.GetExtension(mediaPath).TrimStart('.').ToLowerInvariant();
            // Normalise jpeg/jpg
            if (ext == "jpeg") ext = "jpg";

            images.Add(new ExcelEmbeddedImage(
                AnchorRow: fromRow,
                AnchorCol: fromCol,
                SpanRows: Math.Max(1, toRow - fromRow),
                SpanCols: Math.Max(1, toCol - fromCol),
                Data: imageData,
                Extension: ext,
                WidthEmu: widthEmu,
                HeightEmu: heightEmu
            ));
        }

        return images;
    }

    /// <summary>
    /// Reads chart anchors and basic chart metadata from a worksheet's drawing.
    /// </summary>
    private static List<ExcelChartInfo> ReadSheetCharts(ZipArchive archive, int sheetId, List<ExcelSheet> allSheets)
    {
        var charts = new List<ExcelChartInfo>();

        // Step 1: Find the drawing file from sheet relationships
        var sheetRelsPath = $"xl/worksheets/_rels/sheet{sheetId}.xml.rels";
        var relsEntry = archive.GetEntry(sheetRelsPath);
        if (relsEntry == null) return charts;

        string? drawingFileName = null;
        using (var relsStream = relsEntry.Open())
        {
            var relsDoc = XDocument.Load(relsStream);
            var drawingRel = relsDoc.Descendants()
                .FirstOrDefault(el =>
                    el.Attribute("Type")?.Value.EndsWith("/drawing", StringComparison.OrdinalIgnoreCase) == true);
            if (drawingRel == null) return charts;
            var target = drawingRel.Attribute("Target")?.Value;
            if (string.IsNullOrEmpty(target)) return charts;
            drawingFileName = System.IO.Path.GetFileName(target);
        }

        var drawingPath = $"xl/drawings/{drawingFileName}";
        var drawingEntry = archive.GetEntry(drawingPath);
        if (drawingEntry == null) return charts;

        // Step 2: Read drawing relationships to map rId → chart path
        var drawingRelsPath = $"xl/drawings/_rels/{drawingFileName}.rels";
        var drawingRelsEntry = archive.GetEntry(drawingRelsPath);
        if (drawingRelsEntry == null) return charts;

        var rIdToChart = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        using (var drStream = drawingRelsEntry.Open())
        {
            var drDoc = XDocument.Load(drStream);
            foreach (var rel in drDoc.Descendants())
            {
                var id = rel.Attribute("Id")?.Value;
                var relTarget = rel.Attribute("Target")?.Value;
                var type = rel.Attribute("Type")?.Value ?? "";
                if (id == null || string.IsNullOrEmpty(relTarget)) continue;
                if (!type.EndsWith("/chart", StringComparison.OrdinalIgnoreCase)) continue;

                // Resolve path
                string zipPath;
                if (relTarget.StartsWith('/'))
                    zipPath = relTarget.TrimStart('/');
                else
                {
                    var segments = ("xl/drawings/" + relTarget).Split('/');
                    var resolved = new Stack<string>();
                    foreach (var seg in segments)
                    {
                        if (seg == "..") { if (resolved.Count > 0) resolved.Pop(); }
                        else if (seg != "." && seg != "") resolved.Push(seg);
                    }
                    zipPath = string.Join("/", resolved.Reverse());
                }
                rIdToChart[id] = zipPath;
            }
        }

        if (rIdToChart.Count == 0) return charts;

        // Step 3: Parse drawing XML for chart anchors (graphicFrame elements)
        using var dStream = drawingEntry.Open();
        var dDoc = XDocument.Load(dStream);

        var xdr = XNamespace.Get("http://schemas.openxmlformats.org/drawingml/2006/spreadsheetDrawing");
        var a = XNamespace.Get("http://schemas.openxmlformats.org/drawingml/2006/main");
        var c = XNamespace.Get("http://schemas.openxmlformats.org/drawingml/2006/chart");
        var r = XNamespace.Get("http://schemas.openxmlformats.org/officeDocument/2006/relationships");

        var anchors = dDoc.Descendants(xdr + "twoCellAnchor")
            .Concat(dDoc.Descendants(xdr + "oneCellAnchor"))
            .Concat(dDoc.Descendants(xdr + "absoluteAnchor"));

        foreach (var anchor in anchors)
        {
            // Look for graphicFrame → graphic → graphicData containing a chart reference
            var chartRef = anchor.Descendants(c + "chart").FirstOrDefault();
            if (chartRef == null) continue;

            var chartRId = chartRef.Attribute(r + "id")?.Value;
            if (string.IsNullOrEmpty(chartRId) || !rIdToChart.TryGetValue(chartRId, out var chartPath))
                continue;

            // Read anchor position
            var fromEl = anchor.Element(xdr + "from");
            int fromRow = 0, fromCol = 0;
            if (fromEl != null)
            {
                int.TryParse(fromEl.Element(xdr + "row")?.Value, out fromRow);
                int.TryParse(fromEl.Element(xdr + "col")?.Value, out fromCol);
            }

            long widthEmu = 0, heightEmu = 0;
            var extEl = anchor.Element(xdr + "ext");
            if (extEl != null)
            {
                long.TryParse(extEl.Attribute("cx")?.Value, out widthEmu);
                long.TryParse(extEl.Attribute("cy")?.Value, out heightEmu);
            }
            // Fall back to two-cell anchor dimensions
            if (widthEmu == 0 || heightEmu == 0)
            {
                var toEl = anchor.Element(xdr + "to");
                if (toEl != null)
                {
                    int.TryParse(toEl.Element(xdr + "row")?.Value, out var toRow);
                    int.TryParse(toEl.Element(xdr + "col")?.Value, out var toCol);
                    // Estimate from row/col span: ~914400 EMU per inch, ~72 pt per inch
                    if (widthEmu == 0)
                        widthEmu = Math.Max(1, toCol - fromCol) * 914400;
                    if (heightEmu == 0)
                        heightEmu = Math.Max(1, toRow - fromRow) * 304800;
                }
            }
            // Default chart size if still unknown
            if (widthEmu == 0) widthEmu = 5400000; // ~6 inches
            if (heightEmu == 0) heightEmu = 3600000; // ~4 inches

            // Step 4: Read chart XML for title, type, series data, axes
            var chartEntry = archive.GetEntry(chartPath);
            string title = "";
            string chartType = "chart";
            var seriesList = new List<ExcelChartSeries>();
            string catAxisTitle = "";
            string valAxisTitle = "";

            if (chartEntry != null)
            {
                using var cStream = chartEntry.Open();
                var cDoc = XDocument.Load(cStream);
                var cns = XNamespace.Get("http://schemas.openxmlformats.org/drawingml/2006/chart");

                // Extract chart title from <c:chart><c:title><c:tx><c:rich><a:r><a:t>
                var titleEl = cDoc.Descendants(cns + "title").FirstOrDefault();
                if (titleEl != null)
                {
                    title = string.Concat(titleEl.Descendants(a + "t").Select(t => t.Value));
                }

                // Detect chart type from plotArea children
                var plotArea = cDoc.Descendants(cns + "plotArea").FirstOrDefault();
                XElement? chartTypeEl = null;
                if (plotArea != null)
                {
                    var typeNames = new[] { "barChart", "bar3DChart", "lineChart", "line3DChart",
                        "pieChart", "pie3DChart", "areaChart", "area3DChart", "scatterChart",
                        "doughnutChart", "radarChart", "bubbleChart", "stockChart", "surfaceChart" };
                    foreach (var tn in typeNames)
                    {
                        chartTypeEl = plotArea.Element(cns + tn);
                        if (chartTypeEl != null)
                        {
                            chartType = tn;
                            break;
                        }
                    }

                    // Extract axis titles
                    foreach (var ax in plotArea.Elements().Where(e =>
                        e.Name.LocalName.EndsWith("Ax", StringComparison.Ordinal)))
                    {
                        var axTitle = ax.Element(cns + "title");
                        if (axTitle == null) continue;
                        var axTitleText = string.Concat(axTitle.Descendants(a + "t").Select(t => t.Value));
                        // catAx / dateAx → category axis; valAx → value axis
                        if (ax.Name.LocalName is "catAx" or "dateAx")
                            catAxisTitle = axTitleText;
                        else if (ax.Name.LocalName == "valAx")
                            valAxisTitle = axTitleText;
                    }
                }

                // Extract series data from chart type element
                if (chartTypeEl != null)
                {
                    foreach (var ser in chartTypeEl.Elements(cns + "ser"))
                    {
                        // Series name
                        var serName = "";
                        var txEl = ser.Element(cns + "tx");
                        if (txEl != null)
                        {
                            var sv = txEl.Element(cns + "v")?.Value;
                            if (!string.IsNullOrEmpty(sv))
                                serName = sv;
                            else
                            {
                                // Try strRef → f to resolve from sheet
                                var strRef = txEl.Element(cns + "strRef");
                                var formula = strRef?.Element(cns + "f")?.Value;
                                if (!string.IsNullOrEmpty(formula))
                                {
                                    var resolved = ResolveCellReference(formula, allSheets);
                                    if (resolved.Length > 0) serName = resolved[0];
                                }
                            }
                        }

                        // Categories
                        string[] cats = Array.Empty<string>();
                        var catEl = ser.Element(cns + "cat");
                        if (catEl != null)
                        {
                            cats = ResolveRefElement(catEl, cns, allSheets);
                        }

                        // Values
                        double[] vals = Array.Empty<double>();
                        var valEl = ser.Element(cns + "val");
                        if (valEl != null)
                        {
                            var valStrings = ResolveRefElement(valEl, cns, allSheets);
                            vals = valStrings.Select(v =>
                                double.TryParse(v, System.Globalization.NumberStyles.Any,
                                    System.Globalization.CultureInfo.InvariantCulture, out var d) ? d : 0.0)
                                .ToArray();
                        }

                        seriesList.Add(new ExcelChartSeries(serName, cats, vals));
                    }
                }
            }

            charts.Add(new ExcelChartInfo(fromRow, fromCol, widthEmu, heightEmu, title, chartType,
                seriesList, catAxisTitle, valAxisTitle));
        }

        return charts;
    }

    /// <summary>
    /// Resolves a numRef or strRef element to string values by reading the cell reference formula.
    /// </summary>
    private static string[] ResolveRefElement(XElement parent, XNamespace cns, List<ExcelSheet> allSheets)
    {
        // Try numRef and strRef
        var refEl = parent.Element(cns + "numRef") ?? parent.Element(cns + "strRef");
        if (refEl != null)
        {
            var formula = refEl.Element(cns + "f")?.Value;
            if (!string.IsNullOrEmpty(formula))
                return ResolveCellReference(formula, allSheets);
        }
        // Try numLit (inline values)
        var litEl = parent.Element(cns + "numLit");
        if (litEl != null)
        {
            return litEl.Elements(cns + "pt")
                .OrderBy(pt => int.TryParse(pt.Attribute("idx")?.Value, out var idx) ? idx : 0)
                .Select(pt => pt.Element(cns + "v")?.Value ?? "0")
                .ToArray();
        }
        return Array.Empty<string>();
    }

    /// <summary>
    /// Resolves an Excel cell reference formula like "'Sheet1'!$A$2:$A$6" or "Sheet1!B1"
    /// to actual cell values from the sheet data.
    /// </summary>
    private static string[] ResolveCellReference(string formula, List<ExcelSheet> allSheets)
    {
        // Parse: 'SheetName'!$A$2:$B$6  or  SheetName!A2:A6  or  SheetName!B1
        var parts = formula.Split('!');
        if (parts.Length != 2) return Array.Empty<string>();

        var sheetName = parts[0].Trim('\'');
        var cellRef = parts[1].Replace("$", "");

        var sheet = allSheets.FirstOrDefault(s =>
            s.Name.Equals(sheetName, StringComparison.OrdinalIgnoreCase));
        if (sheet == null) return Array.Empty<string>();

        // Parse range: A2:B6 or single cell A2
        var rangeParts = cellRef.Split(':');
        var (startCol, startRow) = ParseCellAddress(rangeParts[0]);
        var (endCol, endRow) = rangeParts.Length > 1
            ? ParseCellAddress(rangeParts[1])
            : (startCol, startRow);

        var result = new List<string>();
        for (var row = startRow; row <= endRow; row++)
        {
            for (var col = startCol; col <= endCol; col++)
            {
                if (row < sheet.Rows.Count && col < sheet.Rows[row].Count)
                    result.Add(sheet.Rows[row][col].Text);
                else
                    result.Add("");
            }
        }
        return result.ToArray();
    }

    /// <summary>
    /// Parses a cell address like "A2" or "AB10" into (col, row) 0-based indices.
    /// </summary>
    private static (int col, int row) ParseCellAddress(string addr)
    {
        var col = 0;
        var i = 0;
        while (i < addr.Length && char.IsLetter(addr[i]))
        {
            col = col * 26 + (char.ToUpper(addr[i]) - 'A' + 1);
            i++;
        }
        col--; // convert to 0-based
        int.TryParse(addr.AsSpan(i), out var row);
        row--; // convert to 0-based
        return (col, row);
    }
}

/// <summary>
/// Represents a cell read from an Excel file.
/// </summary>
internal sealed record ExcelCell(string Text, PdfColor? Color);

/// <summary>
/// Represents an image embedded in an Excel worksheet.
/// </summary>
internal sealed record ExcelEmbeddedImage(
    int AnchorRow,    // 0-based row index of the top-left anchor
    int AnchorCol,    // 0-based column index of the top-left anchor
    int SpanRows,     // number of rows spanned
    int SpanCols,     // number of columns spanned
    byte[] Data,      // raw image bytes (JPEG or PNG)
    string Extension, // file extension without dot, lower-case, e.g. "jpg"
    long WidthEmu = 0,    // explicit EMU width from <ext>, 0 = not set
    long HeightEmu = 0    // explicit EMU height from <ext>, 0 = not set
);

/// <summary>
/// Represents one data series in a chart.
/// </summary>
internal sealed record ExcelChartSeries(
    string Name,           // series name (e.g. column header)
    string[] Categories,   // category labels (X-axis)
    double[] Values        // numeric values (Y-axis)
);

/// <summary>
/// Represents a chart embedded in an Excel worksheet.
/// </summary>
internal sealed record ExcelChartInfo(
    int AnchorRow,       // 0-based row of top-left anchor
    int AnchorCol,       // 0-based column of top-left anchor
    long WidthEmu,       // chart width in EMU
    long HeightEmu,      // chart height in EMU
    string Title,        // chart title (may be empty)
    string ChartType,    // e.g. "barChart", "lineChart", "pieChart"
    List<ExcelChartSeries> Series,  // data series
    string CategoryAxisTitle = "",  // X-axis title
    string ValueAxisTitle = ""      // Y-axis title
);

/// <summary>
/// Represents a sheet read from an Excel file.
/// </summary>
internal sealed class ExcelSheet
{
    public string Name { get; }
    public List<List<ExcelCell>> Rows { get; }
    public List<ExcelEmbeddedImage> Images { get; }
    public List<ExcelChartInfo> Charts { get; }
    /// <summary>
    /// Excel column widths keyed by 0-based column index.
    /// Values are in Excel character units (convert to points via ExcelSheet.CharUnitsToPoints).
    /// Missing entries mean the default column width applies.
    /// </summary>
    public Dictionary<int, float> ColumnWidths { get; }
    /// <summary>Default column width in Excel character units (typically 8.43).</summary>
    public float DefaultColumnWidth { get; }

    /// <summary>Converts Excel character-unit column width to PDF points.</summary>
    public static float CharUnitsToPoints(float charUnits)
        // Calibrated against LibreOffice reference PDFs: 8.43 char-units → 47.4pt
        => charUnits * 5.62f;

    internal ExcelSheet(string name, List<List<ExcelCell>> rows,
        List<ExcelEmbeddedImage>? images = null,
        Dictionary<int, float>? columnWidths = null,
        float defaultColumnWidth = 8.43f,
        List<ExcelChartInfo>? charts = null)
    {
        Name = name;
        Rows = rows;
        Images = images ?? new List<ExcelEmbeddedImage>();
        Charts = charts ?? new List<ExcelChartInfo>();
        ColumnWidths = columnWidths ?? new Dictionary<int, float>();
        DefaultColumnWidth = defaultColumnWidth;
    }
}
