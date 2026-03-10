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

        // Read styles (font styles, fill colors, borders, number formats)
        var fontStyles = ReadFontStyles(archive);
        var fillColors = ReadFillColors(archive);
        var borders = ReadBorders(archive);
        var numberFormats = ReadNumberFormats(archive);
        var (cellXfFontIndices, cellXfFillIndices, cellXfNumFmtIds, cellXfAlignments, cellXfVerticalAlignments, cellXfBorderIndices) = ReadCellXfStyles(archive);

        // Read workbook to get sheet names and order
        var (sheetInfos, printAreas) = ReadWorkbook(archive);

        // Read each sheet
        var sheetIndex = 0;
        foreach (var info in sheetInfos)
        {
            var currentIndex = sheetIndex++;
            if (info.IsHidden) continue;

            var entry = archive.GetEntry($"xl/worksheets/sheet{info.SheetId}.xml")
                        ?? archive.GetEntry($"xl/worksheets/{info.Name}.xml");

            // Try by relationship id pattern
            entry ??= archive.Entries.FirstOrDefault(e =>
                e.FullName.StartsWith("xl/worksheets/", StringComparison.OrdinalIgnoreCase) &&
                e.FullName.EndsWith(".xml", StringComparison.OrdinalIgnoreCase));

            if (entry == null) continue;

            var rows = ReadSheet(entry, sharedStrings, fontStyles, fillColors, borders, numberFormats, cellXfFontIndices, cellXfFillIndices, cellXfNumFmtIds, cellXfAlignments, cellXfVerticalAlignments, cellXfBorderIndices);
            var images = ReadSheetImages(archive, info.SheetId);
            var (colWidths, defaultColWidth) = ReadColumnWidths(entry);
            var mergedCells = ReadMergedCells(entry);
            var (rowHeights, defaultRowHeight) = ReadRowHeights(entry);
            var pageSetup = ReadPageSetup(entry);
            printAreas.TryGetValue(currentIndex, out var printArea);
            sheets.Add(new ExcelSheet(info.Name, rows, images, colWidths, defaultColWidth, mergedCells: mergedCells, rowHeights: rowHeights, defaultRowHeight: defaultRowHeight, isLandscape: pageSetup.IsLandscape, printScale: pageSetup.Scale, paperSize: pageSetup.PaperSize, printArea: printArea != default ? printArea : null, marginLeftPt: pageSetup.MarginLeftPt, marginRightPt: pageSetup.MarginRightPt, marginTopPt: pageSetup.MarginTopPt, marginBottomPt: pageSetup.MarginBottomPt, fitToPage: pageSetup.FitToPage));
        }

        // If no sheets found via workbook, try reading sheet1 directly
        if (sheets.Count == 0)
        {
            var entry = archive.GetEntry("xl/worksheets/sheet1.xml");
            if (entry != null)
            {
                var rows = ReadSheet(entry, sharedStrings, fontStyles, fillColors, borders, numberFormats, cellXfFontIndices, cellXfFillIndices, cellXfNumFmtIds, cellXfAlignments, cellXfVerticalAlignments, cellXfBorderIndices);
                var images = ReadSheetImages(archive, 1);
                var (colWidths, defaultColWidth) = ReadColumnWidths(entry);
                var mergedCells = ReadMergedCells(entry);
                var (rowHeights, defaultRowHeight) = ReadRowHeights(entry);
                var pageSetup = ReadPageSetup(entry);
                sheets.Add(new ExcelSheet("Sheet1", rows, images, colWidths, defaultColWidth, mergedCells: mergedCells, rowHeights: rowHeights, defaultRowHeight: defaultRowHeight, isLandscape: pageSetup.IsLandscape, printScale: pageSetup.Scale, paperSize: pageSetup.PaperSize, marginLeftPt: pageSetup.MarginLeftPt, marginRightPt: pageSetup.MarginRightPt, marginTopPt: pageSetup.MarginTopPt, marginBottomPt: pageSetup.MarginBottomPt, fitToPage: pageSetup.FitToPage));
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

    private static (List<SheetInfo> Sheets, Dictionary<int, (int StartCol, int StartRow, int EndCol, int EndRow)> PrintAreas) ReadWorkbook(ZipArchive archive)
    {
        var result = new List<SheetInfo>();
        var printAreas = new Dictionary<int, (int, int, int, int)>();
        var entry = archive.GetEntry("xl/workbook.xml");
        if (entry == null) return (result, printAreas);

        using var stream = entry.Open();
        var doc = XDocument.Load(stream);
        var ns = doc.Root?.GetDefaultNamespace() ?? XNamespace.None;

        var sheetId = 1;
        foreach (var sheet in doc.Descendants(ns + "sheet"))
        {
            var name = sheet.Attribute("name")?.Value ?? $"Sheet{sheetId}";
            var state = sheet.Attribute("state")?.Value;
            var isHidden = string.Equals(state, "hidden", StringComparison.OrdinalIgnoreCase)
                       || string.Equals(state, "veryHidden", StringComparison.OrdinalIgnoreCase);
            result.Add(new SheetInfo(name, sheetId, isHidden));
            sheetId++;
        }

        // Read print areas from defined names
        foreach (var dn in doc.Descendants(ns + "definedName"))
        {
            var dnName = dn.Attribute("name")?.Value;
            if (!string.Equals(dnName, "_xlnm.Print_Area", StringComparison.OrdinalIgnoreCase)) continue;
            var localIdStr = dn.Attribute("localSheetId")?.Value;
            if (localIdStr == null || !int.TryParse(localIdStr, out var localId)) continue;
            var val = dn.Value; // e.g. "Paystubs!$A$1:$J$37"
            if (string.IsNullOrEmpty(val)) continue;
            // Remove sheet name prefix
            var bangIdx = val.IndexOf('!');
            if (bangIdx >= 0) val = val.Substring(bangIdx + 1);
            // Parse range like $A$1:$J$37 — strip $ and use ParseCellRef which returns (row, col)
            var parts = val.Replace("$", "").Split(':');
            if (parts.Length == 2)
            {
                var (sr, sc) = ParseCellRef(parts[0]);
                var (er, ec) = ParseCellRef(parts[1]);
                if (sc >= 0 && sr >= 0 && ec >= 0 && er >= 0)
                    printAreas[localId] = (sc, sr, ec, er);
            }
        }

        return (result, printAreas);
    }

    private static List<FontStyleInfo> ReadFontStyles(ZipArchive archive)
    {
        var styles = new List<FontStyleInfo>();
        var entry = archive.GetEntry("xl/styles.xml");
        if (entry == null) return styles;

        using var stream = entry.Open();
        var doc = XDocument.Load(stream);
        var ns = doc.Root?.GetDefaultNamespace() ?? XNamespace.None;

        // Read <fonts> -> <font> elements
        var fontsElement = doc.Descendants(ns + "fonts").FirstOrDefault();
        if (fontsElement == null) return styles;

        foreach (var font in fontsElement.Elements(ns + "font"))
        {
            PdfColor? color = null;
            var colorEl = font.Element(ns + "color");
            if (colorEl != null)
            {
                var rgb = colorEl.Attribute("rgb")?.Value;
                if (!string.IsNullOrEmpty(rgb))
                    color = PdfColor.FromHex(rgb);
                else
                {
                    var indexed = colorEl.Attribute("indexed")?.Value;
                    if (!string.IsNullOrEmpty(indexed) && int.TryParse(indexed, out var idx))
                        color = GetIndexedColor(idx);
                }
            }

            // Read font size
            float fontSize = 11f;
            var szEl = font.Element(ns + "sz");
            if (szEl != null)
            {
                var szVal = szEl.Attribute("val")?.Value;
                if (float.TryParse(szVal, System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out var sz) && sz > 0)
                    fontSize = sz;
            }

            // Read bold
            var bold = font.Element(ns + "b") != null;

            // Read italic
            var italic = font.Element(ns + "i") != null;

            styles.Add(new FontStyleInfo(color, fontSize, bold, italic));
        }

        return styles;
    }

    /// <summary>
    /// Reads border definitions from styles.xml.
    /// Returns a list of CellBorderInfo indexed by borderId.
    /// </summary>
    private static List<CellBorderInfo?> ReadBorders(ZipArchive archive)
    {
        var borders = new List<CellBorderInfo?>();
        var entry = archive.GetEntry("xl/styles.xml");
        if (entry == null) return borders;

        using var stream = entry.Open();
        var doc = XDocument.Load(stream);
        var ns = doc.Root?.GetDefaultNamespace() ?? XNamespace.None;

        var bordersEl = doc.Descendants(ns + "borders").FirstOrDefault();
        if (bordersEl == null) return borders;

        foreach (var border in bordersEl.Elements(ns + "border"))
        {
            var left = ReadBorderSide(border.Element(ns + "left"), ns);
            var right = ReadBorderSide(border.Element(ns + "right"), ns);
            var top = ReadBorderSide(border.Element(ns + "top"), ns);
            var bottom = ReadBorderSide(border.Element(ns + "bottom"), ns);

            if (left == null && right == null && top == null && bottom == null)
                borders.Add(null);
            else
                borders.Add(new CellBorderInfo(left, right, top, bottom));
        }

        return borders;
    }

    private static BorderSide? ReadBorderSide(XElement? el, XNamespace ns)
    {
        if (el == null) return null;
        var style = el.Attribute("style")?.Value;
        if (string.IsNullOrEmpty(style) || style == "none") return null;

        PdfColor? color = null;
        var colorEl = el.Element(ns + "color");
        if (colorEl != null)
        {
            var rgb = colorEl.Attribute("rgb")?.Value;
            if (!string.IsNullOrEmpty(rgb))
                color = PdfColor.FromHex(rgb);
            else
            {
                var indexed = colorEl.Attribute("indexed")?.Value;
                if (!string.IsNullOrEmpty(indexed) && int.TryParse(indexed, out var idx))
                    color = GetIndexedColor(idx);
            }
        }
        // Default border color is black
        color ??= PdfColor.FromRgb(0, 0, 0);
        return new BorderSide(style, color);
    }

    /// <summary>
    /// Reads cellXf style entries from styles.xml.
    /// Returns (fontIndices, fillIndices, numFmtIds) parallel lists.
    /// </summary>
    private static (List<int> FontIndices, List<int> FillIndices, List<int> NumFmtIds, List<string> Alignments, List<string> VerticalAlignments, List<int> BorderIndices) ReadCellXfStyles(ZipArchive archive)
    {
        var fontIndices = new List<int>();
        var fillIndices = new List<int>();
        var numFmtIds = new List<int>();
        var alignments = new List<string>();
        var verticalAlignments = new List<string>();
        var borderIndices = new List<int>();
        var entry = archive.GetEntry("xl/styles.xml");
        if (entry == null) return (fontIndices, fillIndices, numFmtIds, alignments, verticalAlignments, borderIndices);

        using var stream = entry.Open();
        var doc = XDocument.Load(stream);
        var ns = doc.Root?.GetDefaultNamespace() ?? XNamespace.None;

        // Read <cellXfs> -> <xf> elements
        var cellXfs = doc.Descendants(ns + "cellXfs").FirstOrDefault();
        if (cellXfs == null) return (fontIndices, fillIndices, numFmtIds, alignments, verticalAlignments, borderIndices);

        foreach (var xf in cellXfs.Elements(ns + "xf"))
        {
            var fontId = xf.Attribute("fontId")?.Value;
            fontIndices.Add(int.TryParse(fontId, out var fid) ? fid : 0);

            var fillId = xf.Attribute("fillId")?.Value;
            fillIndices.Add(int.TryParse(fillId, out var filli) ? filli : 0);

            var numFmtId = xf.Attribute("numFmtId")?.Value;
            numFmtIds.Add(int.TryParse(numFmtId, out var nid) ? nid : 0);

            var alignment = xf.Element(ns + "alignment")?.Attribute("horizontal")?.Value ?? "general";
            alignments.Add(alignment);

            var verticalAlignment = xf.Element(ns + "alignment")?.Attribute("vertical")?.Value ?? "bottom";
            verticalAlignments.Add(verticalAlignment);

            var borderId = xf.Attribute("borderId")?.Value;
            borderIndices.Add(int.TryParse(borderId, out var bid) ? bid : 0);
        }

        return (fontIndices, fillIndices, numFmtIds, alignments, verticalAlignments, borderIndices);
    }

    /// <summary>
    /// Reads fill patterns from styles.xml.
    /// Returns a list of fill colors indexed by fillId (null for none/gray125).
    /// </summary>
    private static List<PdfColor?> ReadFillColors(ZipArchive archive)
    {
        var fills = new List<PdfColor?>();
        var entry = archive.GetEntry("xl/styles.xml");
        if (entry == null) return fills;

        using var stream = entry.Open();
        var doc = XDocument.Load(stream);
        var ns = doc.Root?.GetDefaultNamespace() ?? XNamespace.None;

        var fillsEl = doc.Descendants(ns + "fills").FirstOrDefault();
        if (fillsEl == null) return fills;

        foreach (var fill in fillsEl.Elements(ns + "fill"))
        {
            var patternFill = fill.Element(ns + "patternFill");
            if (patternFill == null)
            {
                fills.Add(null);
                continue;
            }

            var patternType = patternFill.Attribute("patternType")?.Value;
            if (string.IsNullOrEmpty(patternType) || patternType == "none")
            {
                fills.Add(null);
                continue;
            }

            // Read foreground and background colors
            var fgColor = patternFill.Element(ns + "fgColor");
            var bgColor = patternFill.Element(ns + "bgColor");

            PdfColor? fgPdf = ResolveColorElement(fgColor);
            PdfColor? bgPdf = ResolveColorElement(bgColor);

            if (patternType == "solid")
            {
                if (fgPdf != null)
                {
                    var c = fgPdf.Value;
                    // Skip pure white fills as they're invisible
                    if (c.R < 0.99f || c.G < 0.99f || c.B < 0.99f)
                        fills.Add(c);
                    else
                        fills.Add(null);
                }
                else
                    fills.Add(null);
            }
            else
            {
                // Non-solid patterns: approximate as a blended solid color
                // Use a tint of the foreground color to simulate the pattern effect
                var tint = patternType switch
                {
                    "darkGray" => 0.75f,
                    "mediumGray" => 0.50f,
                    "lightGray" => 0.25f,
                    "gray125" => 0.125f,
                    "gray0625" => 0.0625f,
                    "darkHorizontal" or "darkVertical" or "darkDown" or "darkUp" or "darkGrid" or "darkTrellis" => 0.50f,
                    "lightHorizontal" or "lightVertical" or "lightDown" or "lightUp" or "lightGrid" or "lightTrellis" => 0.25f,
                    _ => 0.30f
                };

                if (fgPdf != null)
                {
                    var fg = fgPdf.Value;
                    var bg = bgPdf ?? new PdfColor(1f, 1f, 1f);
                    // Blend: result = bg * (1 - tint) + fg * tint
                    var r = bg.R * (1 - tint) + fg.R * tint;
                    var g = bg.G * (1 - tint) + fg.G * tint;
                    var b = bg.B * (1 - tint) + fg.B * tint;
                    fills.Add(new PdfColor(r, g, b));
                }
                else
                {
                    // Pattern with no explicit fg: use gray based on tint
                    var gray = 1f - tint * 0.5f;
                    fills.Add(new PdfColor(gray, gray, gray));
                }
            }
        }

        return fills;
    }

    /// <summary>Resolves a color element (rgb or indexed) to a PdfColor.</summary>
    private static PdfColor? ResolveColorElement(XElement? el)
    {
        if (el == null) return null;
        var rgb = el.Attribute("rgb")?.Value;
        if (!string.IsNullOrEmpty(rgb)) return PdfColor.FromHex(rgb);
        var indexed = el.Attribute("indexed")?.Value;
        if (!string.IsNullOrEmpty(indexed) && int.TryParse(indexed, out var idx))
            return GetIndexedColor(idx);
        return null;
    }

    /// <summary>
    /// Reads custom number formats from styles.xml.
    /// Returns a dictionary mapping numFmtId to format code string.
    /// </summary>
    private static Dictionary<int, string> ReadNumberFormats(ZipArchive archive)
    {
        var formats = new Dictionary<int, string>();
        var entry = archive.GetEntry("xl/styles.xml");
        if (entry == null) return formats;

        using var stream = entry.Open();
        var doc = XDocument.Load(stream);
        var ns = doc.Root?.GetDefaultNamespace() ?? XNamespace.None;

        var numFmts = doc.Descendants(ns + "numFmts").FirstOrDefault();
        if (numFmts == null) return formats;

        foreach (var fmt in numFmts.Elements(ns + "numFmt"))
        {
            var id = fmt.Attribute("numFmtId")?.Value;
            var code = fmt.Attribute("formatCode")?.Value;
            if (int.TryParse(id, out var numId) && !string.IsNullOrEmpty(code))
                formats[numId] = code;
        }

        return formats;
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

    private static PdfColor? ResolveCellColor(int styleIndex, List<FontStyleInfo> fontStyles, List<int> fontIndices)
    {
        if (styleIndex < 0 || styleIndex >= fontIndices.Count)
            return null;

        var fontIndex = fontIndices[styleIndex];
        if (fontIndex < 0 || fontIndex >= fontStyles.Count)
            return null;

        return fontStyles[fontIndex].Color;
    }

    private static FontStyleInfo ResolveFontStyle(int styleIndex, List<FontStyleInfo> fontStyles, List<int> fontIndices)
    {
        if (styleIndex < 0 || styleIndex >= fontIndices.Count)
            return new FontStyleInfo(null);

        var fontIndex = fontIndices[styleIndex];
        if (fontIndex < 0 || fontIndex >= fontStyles.Count)
            return new FontStyleInfo(null);

        return fontStyles[fontIndex];
    }

    private static CellBorderInfo? ResolveBorder(int styleIndex, List<CellBorderInfo?> borders, List<int> borderIndices)
    {
        if (styleIndex < 0 || styleIndex >= borderIndices.Count)
            return null;

        var borderIndex = borderIndices[styleIndex];
        if (borderIndex < 0 || borderIndex >= borders.Count)
            return null;

        return borders[borderIndex];
    }

    private static PdfColor? ResolveFillColor(int styleIndex, List<PdfColor?> fillColors, List<int> fillIndices)
    {
        if (styleIndex < 0 || styleIndex >= fillIndices.Count)
            return null;

        var fillIndex = fillIndices[styleIndex];
        if (fillIndex < 0 || fillIndex >= fillColors.Count)
            return null;

        return fillColors[fillIndex];
    }

    private static List<List<ExcelCell>> ReadSheet(ZipArchiveEntry entry, List<string> sharedStrings,
        List<FontStyleInfo> fontStyles, List<PdfColor?> fillColors, List<CellBorderInfo?> borders, Dictionary<int, string> numberFormats,
        List<int> cellXfFontIndices, List<int> cellXfFillIndices, List<int> cellXfNumFmtIds, List<string> cellXfAlignments, List<string> cellXfVerticalAlignments, List<int> cellXfBorderIndices)
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
                    cells.Add(new ExcelCell(string.Empty, null, null));
                    lastColIndex++;
                }

                var type = cell.Attribute("t")?.Value;
                var value = cell.Element(ns + "v")?.Value ?? "";

                // Resolve color and fill from style index
                var styleAttr = cell.Attribute("s")?.Value;
                PdfColor? color = null;
                PdfColor? fillColor = null;
                int numFmtId = 0;
                var cellAlignment = "general";
                var cellVerticalAlignment = "bottom";
                float fontSize = 11f;
                bool bold = false;
                bool italic = false;
                CellBorderInfo? border = null;
                if (int.TryParse(styleAttr, out var styleIndex))
                {
                    var fontStyle = ResolveFontStyle(styleIndex, fontStyles, cellXfFontIndices);
                    color = fontStyle.Color;
                    fontSize = fontStyle.Size;
                    bold = fontStyle.Bold;
                    italic = fontStyle.Italic;
                    fillColor = ResolveFillColor(styleIndex, fillColors, cellXfFillIndices);
                    border = ResolveBorder(styleIndex, borders, cellXfBorderIndices);
                    if (styleIndex >= 0 && styleIndex < cellXfNumFmtIds.Count)
                        numFmtId = cellXfNumFmtIds[styleIndex];
                    if (styleIndex >= 0 && styleIndex < cellXfAlignments.Count)
                        cellAlignment = cellXfAlignments[styleIndex];
                    if (styleIndex >= 0 && styleIndex < cellXfVerticalAlignments.Count)
                        cellVerticalAlignment = cellXfVerticalAlignments[styleIndex];
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

                    // Format numeric cells using the cell's number format
                    if (string.IsNullOrEmpty(type) || type == "n")
                    {
                        if (!string.IsNullOrEmpty(text) &&
                            double.TryParse(text, System.Globalization.NumberStyles.Any,
                                System.Globalization.CultureInfo.InvariantCulture, out var numVal))
                        {
                            text = FormatNumber(numVal, numFmtId, numberFormats);
                        }
                    }


                }

                // Resolve "general" alignment: numbers right-align, booleans center, text left-aligns
                if (cellAlignment == "general")
                {
                    // Numeric cells (type "" or "n") with numeric values get right-aligned
                    var isNumericCell = (string.IsNullOrEmpty(type) || type == "n") &&
                                       double.TryParse(value, System.Globalization.NumberStyles.Any,
                                           System.Globalization.CultureInfo.InvariantCulture, out _);
                    // Boolean cells (type "b") center-align by default in Excel
                    cellAlignment = isNumericCell ? "right" : (type == "b" ? "center" : "left");
                }

                cells.Add(new ExcelCell(text, color, fillColor, cellAlignment, fontSize, bold, italic, border, cellVerticalAlignment));
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

    /// <summary>
    /// Formats a numeric value according to its Excel number format.
    /// Handles built-in formats and common custom patterns.
    /// </summary>
    private static string FormatNumber(double value, int numFmtId, Dictionary<int, string> customFormats)
    {
        var ci = System.Globalization.CultureInfo.InvariantCulture;

        // Check custom format first
        if (numFmtId > 0 && customFormats.TryGetValue(numFmtId, out var formatCode))
        {
            return ApplyNumberFormat(value, formatCode);
        }

        // Built-in number formats
        return numFmtId switch
        {
            0 => FormatGeneral(value),          // General
            1 => value.ToString("F0", ci),      // 0
            2 => value.ToString("F2", ci),      // 0.00
            3 => value.ToString("#,##0", ci),   // #,##0
            4 => value.ToString("#,##0.00", ci),// #,##0.00
            9 => (value * 100).ToString("F0", ci) + "%",  // 0%
            10 => (value * 100).ToString("F2", ci) + "%", // 0.00%
            11 => value.ToString("0.00E+00", ci),         // 0.00E+00
            // Date formats (14-22): Excel stores dates as serial numbers
            14 => FormatExcelDate(value, "MM/dd/yyyy"),
            15 => FormatExcelDate(value, "d-MMM-yy"),
            16 => FormatExcelDate(value, "d-MMM"),
            17 => FormatExcelDate(value, "MMM-yy"),
            18 => FormatExcelDate(value, "h:mm tt"),
            19 => FormatExcelDate(value, "h:mm:ss tt"),
            20 => FormatExcelDate(value, "H:mm"),
            21 => FormatExcelDate(value, "H:mm:ss"),
            22 => FormatExcelDate(value, "M/d/yyyy H:mm"),
            // More number formats
            37 => value.ToString("#,##0", ci),
            38 => value.ToString("#,##0", ci),
            39 => value.ToString("#,##0.00", ci),
            40 => value.ToString("#,##0.00", ci),
            _ => FormatGeneral(value)
        };
    }

    /// <summary>
    /// Applies a custom Excel number format code to a value.
    /// Handles common patterns like "0.00", "#,##0", "0.00E+00", currency, percentage, etc.
    /// </summary>
    private static string ApplyNumberFormat(double value, string formatCode)
    {
        var ci = System.Globalization.CultureInfo.InvariantCulture;

        // Handle multi-section formats (positive;negative;zero) - use the appropriate section
        var sections = formatCode.Split(';');
        string activeFormat;
        bool isNegativeSection = false;
        if (sections.Length >= 3 && value == 0)
            activeFormat = sections[2];
        else if (sections.Length >= 2 && value < 0)
        {
            activeFormat = sections[1];
            isNegativeSection = true;
            value = Math.Abs(value); // negative section handles sign display
        }
        else
            activeFormat = sections[0];

        // Strip color codes like [Red], [Blue], etc.
        activeFormat = System.Text.RegularExpressions.Regex.Replace(activeFormat, @"\[(?:Red|Blue|Green|Yellow|Magenta|Cyan|White|Black|Color\d+)\]", "", System.Text.RegularExpressions.RegexOptions.IgnoreCase);

        // Strip locale/currency codes like [$€-407], [$¥-411], [$-409]
        activeFormat = System.Text.RegularExpressions.Regex.Replace(activeFormat, @"\[\$([^-\]]*)-[^\]]+\]", "$1");
        // Also handle [$symbol] without locale
        activeFormat = System.Text.RegularExpressions.Regex.Replace(activeFormat, @"\[\$([^\]]*)\]", "$1");

        // Strip _X (spacing: add space the width of char X) → replace with space
        // Strip *X (fill: repeat char X to fill column) → remove entirely
        activeFormat = System.Text.RegularExpressions.Regex.Replace(activeFormat, @"_.", " ");
        activeFormat = System.Text.RegularExpressions.Regex.Replace(activeFormat, @"\*.", "");

        // Handle escaped literal characters: \( → (, \) → ), etc.
        activeFormat = System.Text.RegularExpressions.Regex.Replace(activeFormat, @"\\(.)", "$1");

        // Handle percentage format
        if (activeFormat.Contains('%'))
        {
            var pctFormat = activeFormat.Replace("%", "").Trim();
            var decPlaces = pctFormat.Contains('.') ? pctFormat.Length - pctFormat.IndexOf('.') - 1 : 0;
            return (value * 100).ToString($"F{decPlaces}", ci) + "%";
        }

        // Handle scientific notation
        if (activeFormat.Contains("E+") || activeFormat.Contains("E-"))
        {
            var decPlaces = activeFormat.Contains('.') ? activeFormat.IndexOf('E') - activeFormat.IndexOf('.') - 1 : 0;
            if (decPlaces < 0) decPlaces = 0;
            return value.ToString($"0.{new string('0', decPlaces)}E+00", ci);
        }

        // Handle date/time-like formats (must check before number handling)
        var lowerFmt = activeFormat.ToLowerInvariant();
        if (lowerFmt.Contains("yy") || lowerFmt.Contains("dd") ||
            (lowerFmt.Contains("mm") && (lowerFmt.Contains("dd") || lowerFmt.Contains("yy") || lowerFmt.Contains("hh") || lowerFmt.Contains("ss"))) ||
            lowerFmt.Contains("hh") || lowerFmt.Contains("h:") || lowerFmt.Contains("am/pm") || lowerFmt.Contains("a/p"))
        {
            return FormatExcelDate(value, activeFormat);
        }

        // Count decimal places from format
        var hasDecimal = activeFormat.Contains('.');
        var decimalPlaces = 0;
        if (hasDecimal)
        {
            var dotIdx = activeFormat.IndexOf('.');
            for (var i = dotIdx + 1; i < activeFormat.Length; i++)
            {
                if (activeFormat[i] == '0' || activeFormat[i] == '#')
                    decimalPlaces++;
                else
                    break;
            }
        }

        // Check if format has thousand separator
        var hasThousands = activeFormat.Contains("#,##") || activeFormat.Contains("0,0");

        // Check for zero-padding in integer part (e.g., "0000")
        var integerZeros = 0;
        var numPartForPad = activeFormat;
        var dotPos = numPartForPad.IndexOf('.');
        var intPart = dotPos >= 0 ? numPartForPad[..dotPos] : numPartForPad;
        // Strip non-format chars
        foreach (var ch in intPart)
        {
            if (ch == '0') integerZeros++;
        }

        // Extract prefix/suffix (currency symbols, text, etc.)
        var prefix = "";
        var suffix = "";
        var numPart = activeFormat;
        // Find where the number pattern starts
        var numStart = numPart.IndexOfAny(new[] { '0', '#', '.' });
        if (numStart > 0) { prefix = numPart[..numStart]; numPart = numPart[numStart..]; }
        // Find where the number pattern ends
        var numEnd = numPart.LastIndexOfAny(new[] { '0', '#' });
        if (numEnd >= 0 && numEnd < numPart.Length - 1) { suffix = numPart[(numEnd + 1)..]; numPart = numPart[..(numEnd + 1)]; }

        // Remove literal escape characters
        prefix = prefix.Replace("\\", "").Replace("\"", "");
        suffix = suffix.Replace("\\", "").Replace("\"", "");

        // Handle negative sign placement:
        // For currency formats, the minus sign should appear BEFORE the currency symbol
        // (e.g., -$180,000.00 not $-180,000.00).
        var negSign = "";
        if (isNegativeSection)
        {
            // The negative section already specifies the formatting for negative values.
            // If it contains a '-', it's already in the prefix/suffix.
            // If the format uses parentheses '(' for negative display, no sign needed.
            // If neither is present, we need to add a minus sign.
            if (!prefix.Contains('-') && !suffix.Contains('-') && !activeFormat.Contains('-')
                && !prefix.Contains('('))
                negSign = "-";
        }
        else if (value < 0)
        {
            // Single-section format with negative value: place minus before prefix
            // (e.g., "$#,##0.00" with -180000 → "-$180,000.00")
            value = Math.Abs(value);
            negSign = "-";
        }

        string formatted;
        if (hasThousands)
        {
            formatted = value.ToString($"N{decimalPlaces}", ci);
        }
        else if (hasDecimal)
        {
            formatted = value.ToString($"F{decimalPlaces}", ci);
        }
        else if (integerZeros > 1)
        {
            // Zero-padding: format "0000" → pad integer to that width
            formatted = ((long)Math.Round(value)).ToString(new string('0', integerZeros));
        }
        else if (value == Math.Floor(value))
        {
            formatted = value.ToString("F0", ci);
        }
        else
        {
            formatted = FormatGeneral(value);
        }

        return negSign + prefix + formatted + suffix;
    }

    /// <summary>
    /// Formats a number using Excel's "General" format logic.
    /// LibreOffice's General format adapts precision to fit approximately 10 characters,
    /// switching to scientific notation for very large/small values.
    /// </summary>
    private static string FormatGeneral(double value)
    {
        var ci = System.Globalization.CultureInfo.InvariantCulture;
        if (value == 0) return "0";
        var abs = Math.Abs(value);

        // Exact integer: show as integer if it fits within ~10 characters.
        // LibreOffice uses scientific notation for large integers that exceed display width.
        if (value == Math.Floor(value) && abs < 1e10)
            return value.ToString("F0", ci);

        // Very small numbers → prefer decimal if compact, else scientific
        if (abs > 0 && abs < 1e-4)
        {
            // F6 can represent values like 0.000001 in decimal form
            var dec = value.ToString("F6", ci).TrimEnd('0');
            if (dec.EndsWith('.')) dec = dec[..^1];
            if (dec.Length <= 10 && double.Parse(dec, ci) == value)
                return dec;
            // Fall through to G10
        }

        // Standard range: up to 10 significant digits.
        // FitNumericText in the converter will shorten if needed for column width.
        var g10 = value.ToString("G10", ci);

        // For values very close to integers (like 9999999.99 → 10000000),
        // check if rounding gives a shorter representation
        var rounded = Math.Round(value);
        if (rounded != 0 && Math.Abs(value - rounded) / Math.Abs(rounded) < 1e-8 && Math.Abs(rounded) < 1e10)
        {
            var intStr = rounded.ToString("F0", ci);
            if (intStr.Length <= g10.Length)
                return intStr;
        }

        return g10;
    }

    /// <summary>
    /// Converts an Excel serial date number to a date string using the given format code.
    /// Excel epoch: Jan 1, 1900 = serial number 1.
    /// </summary>
    private static string FormatExcelDate(double serialDate, string formatCode = "yyyy-MM-dd")
    {
        try
        {
            // Excel incorrectly considers 1900 as a leap year (Feb 29, 1900 = serial 60).
            // For dates after Feb 28, 1900, subtract 1 to correct.
            var days = (int)serialDate;
            if (days > 60) days--;
            var date = new DateTime(1900, 1, 1).AddDays(days - 1);

            // Handle fractional time component
            var fraction = serialDate - Math.Floor(serialDate);
            var timeSpan = TimeSpan.FromDays(fraction);
            var dateTime = date.Add(timeSpan);

            // Convert Excel format code to .NET format
            var dotNetFormat = ConvertExcelDateFormat(formatCode);

            return dateTime.ToString(dotNetFormat, System.Globalization.CultureInfo.InvariantCulture);
        }
        catch
        {
            return serialDate.ToString("G10", System.Globalization.CultureInfo.InvariantCulture);
        }
    }

    /// <summary>
    /// Converts an Excel date/time format code to a .NET DateTime format string.
    /// </summary>
    private static string ConvertExcelDateFormat(string excelFormat)
    {
        if (string.IsNullOrEmpty(excelFormat)) return "yyyy-MM-dd";

        // Strip color codes and locale codes
        var fmt = System.Text.RegularExpressions.Regex.Replace(excelFormat, @"\[(?:Red|Blue|Green|Yellow|Magenta|Cyan|White|Black|Color\d+)\]", "", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        fmt = System.Text.RegularExpressions.Regex.Replace(fmt, @"\[\$[^\]]*\]", "");

        // Map Excel date tokens to .NET tokens
        // Excel uses lowercase for dates: d, dd, m, mm, yy, yyyy
        // Excel uses h, hh, m, mm, s, ss for time
        // .NET uses d, dd, M, MM, yy, yyyy, h, hh, m, mm, s, ss

        // Need to be careful: 'm' means month near 'd'/'y' and minute near 'h'/'s'
        var sb = new System.Text.StringBuilder();
        var lower = fmt.ToLowerInvariant();

        // Track context: is m near h/s (minute) or near d/y (month)?
        // Simple approach: if format has h or s, treat m as minute
        var hasTime = lower.Contains('h') || lower.Contains('s');
        var hasDate = lower.Contains('d') || lower.Contains('y');

        // More precise: walk through and decide by proximity
        for (var i = 0; i < fmt.Length; i++)
        {
            var c = char.ToLower(fmt[i]);

            if (c == '\\' && i + 1 < fmt.Length)
            {
                sb.Append(fmt[i + 1]); // literal escape
                i++;
                continue;
            }

            if (c == '"')
            {
                // Quoted literal
                i++;
                while (i < fmt.Length && fmt[i] != '"')
                {
                    sb.Append(fmt[i]);
                    i++;
                }
                continue;
            }

            if (c == 'y')
            {
                var count = 1;
                while (i + 1 < fmt.Length && char.ToLower(fmt[i + 1]) == 'y') { count++; i++; }
                sb.Append(count >= 4 ? "yyyy" : "yy");
            }
            else if (c == 'd')
            {
                var count = 1;
                while (i + 1 < fmt.Length && char.ToLower(fmt[i + 1]) == 'd') { count++; i++; }
                sb.Append(count switch { 1 => "d", 2 => "dd", 3 => "ddd", _ => "dddd" });
            }
            else if (c == 'h')
            {
                var count = 1;
                while (i + 1 < fmt.Length && char.ToLower(fmt[i + 1]) == 'h') { count++; i++; }
                sb.Append(count >= 2 ? "HH" : "H");
            }
            else if (c == 's')
            {
                var count = 1;
                while (i + 1 < fmt.Length && char.ToLower(fmt[i + 1]) == 's') { count++; i++; }
                sb.Append(count >= 2 ? "ss" : "s");
            }
            else if (c == 'm')
            {
                var count = 1;
                while (i + 1 < fmt.Length && char.ToLower(fmt[i + 1]) == 'm') { count++; i++; }

                // Decide: month or minute?
                // If preceded by 'h' or followed by 's' → minute; otherwise month
                var isMinute = false;
                // Look backwards for 'h'
                for (var j = i - count; j >= 0; j--)
                {
                    var prev = char.ToLower(fmt[j]);
                    if (prev == 'h') { isMinute = true; break; }
                    if (prev == 'd' || prev == 'y') break;
                    if (prev == ':' || prev == ' ') continue;
                    break;
                }
                // Look forwards for 's'
                if (!isMinute)
                {
                    for (var j = i + 1; j < fmt.Length; j++)
                    {
                        var next = char.ToLower(fmt[j]);
                        if (next == 's') { isMinute = true; break; }
                        if (next == 'd' || next == 'y') break;
                        if (next == ':' || next == ' ') continue;
                        break;
                    }
                }

                if (isMinute)
                    sb.Append(count >= 2 ? "mm" : "m");
                else
                    sb.Append(count >= 2 ? "MM" : "M");
            }
            else if (c == 'a' && i + 4 < fmt.Length && lower.Substring(i, 5) == "am/pm")
            {
                sb.Append("tt");
                i += 4;
            }
            else if (c == 'a' && i + 2 < fmt.Length && lower.Substring(i, 3) == "a/p")
            {
                sb.Append("tt");
                i += 2;
            }
            else
            {
                sb.Append(fmt[i]); // separators, literals
            }
        }

        var result = sb.ToString().Trim();
        return string.IsNullOrEmpty(result) ? "yyyy-MM-dd" : result;
    }

    internal record SheetInfo(string Name, int SheetId, bool IsHidden = false);

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
    /// Reads page setup (orientation, scale, paper size, margins, fitToPage) from the sheet XML.
    /// </summary>
    private static PageSetupInfo ReadPageSetup(ZipArchiveEntry entry)
    {
        var isLandscape = false;
        var scale = 100;
        var paperSize = 1; // 1 = US Letter (default)
        float marginLeft = -1, marginRight = -1, marginTop = -1, marginBottom = -1;
        var fitToPage = false;

        using var stream = entry.Open();
        var doc = XDocument.Load(stream);
        var ns = doc.Root?.GetDefaultNamespace() ?? XNamespace.None;

        var pageSetup = doc.Descendants(ns + "pageSetup").FirstOrDefault();
        if (pageSetup != null)
        {
            var orient = pageSetup.Attribute("orientation")?.Value;
            if (string.Equals(orient, "landscape", StringComparison.OrdinalIgnoreCase))
                isLandscape = true;

            var scaleAttr = pageSetup.Attribute("scale")?.Value;
            if (int.TryParse(scaleAttr, out var s) && s > 0 && s <= 400)
                scale = s;

            var paperAttr = pageSetup.Attribute("paperSize")?.Value;
            if (int.TryParse(paperAttr, out var p) && p > 0)
                paperSize = p;
        }

        // Read page margins (in inches)
        var pageMargins = doc.Descendants(ns + "pageMargins").FirstOrDefault();
        if (pageMargins != null)
        {
            static float ParseInchesAttr(XElement el, string attr)
            {
                var val = el.Attribute(attr)?.Value;
                if (val != null && float.TryParse(val, System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out var inches))
                    return inches * 72f; // convert inches to points
                return -1;
            }
            marginLeft = ParseInchesAttr(pageMargins, "left");
            marginRight = ParseInchesAttr(pageMargins, "right");
            marginTop = ParseInchesAttr(pageMargins, "top");
            marginBottom = ParseInchesAttr(pageMargins, "bottom");
        }

        // Read fitToPage from sheetPr/pageSetUpPr
        var pageSetUpPr = doc.Descendants(ns + "pageSetUpPr").FirstOrDefault();
        if (pageSetUpPr != null)
        {
            fitToPage = pageSetUpPr.Attribute("fitToPage")?.Value == "1";
        }

        return new PageSetupInfo(isLandscape, scale, paperSize, marginLeft, marginRight, marginTop, marginBottom, fitToPage);
    }

    internal record PageSetupInfo(
        bool IsLandscape, int Scale, int PaperSize,
        float MarginLeftPt, float MarginRightPt, float MarginTopPt, float MarginBottomPt,
        bool FitToPage);

    /// <summary>
    /// Reads row heights from the sheet XML.
    /// Returns a dictionary of 0-based row index → height in points, plus the default row height.
    /// </summary>
    private static (Dictionary<int, float> heights, float defaultHeight) ReadRowHeights(ZipArchiveEntry entry)
    {
        var heights = new Dictionary<int, float>();
        var defaultHeight = 15f; // Excel default row height in points

        using var stream = entry.Open();
        var doc = XDocument.Load(stream);
        var ns = doc.Root?.GetDefaultNamespace() ?? XNamespace.None;

        // Read default row height from sheetFormatPr
        var fmtPr = doc.Descendants(ns + "sheetFormatPr").FirstOrDefault();
        if (fmtPr?.Attribute("defaultRowHeight") != null)
        {
            var drh = fmtPr.Attribute("defaultRowHeight")!.Value;
            if (float.TryParse(drh,
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture,
                out var parsed) && parsed > 0f)
            {
                defaultHeight = parsed;
            }
        }

        // Read explicit row heights (customHeight="1")
        foreach (var row in doc.Descendants(ns + "row"))
        {
            var rAttr = row.Attribute("r")?.Value;
            var htAttr = row.Attribute("ht")?.Value;
            if (rAttr == null || htAttr == null) continue;

            if (!int.TryParse(rAttr, out var rowNum)) continue;
            if (!float.TryParse(htAttr,
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture,
                out var ht)) continue;

            heights[rowNum - 1] = ht; // store as 0-based
        }

        return (heights, defaultHeight);
    }

    /// <summary>
    /// Reads merged cell regions from the sheet XML.
    /// Returns a list of (startRow, startCol, endRow, endCol) all 0-based.
    /// </summary>
    private static List<(int, int, int, int)> ReadMergedCells(ZipArchiveEntry entry)
    {
        var result = new List<(int, int, int, int)>();
        using var stream = entry.Open();
        var doc = XDocument.Load(stream);
        var ns = doc.Root?.GetDefaultNamespace() ?? XNamespace.None;

        foreach (var mc in doc.Descendants(ns + "mergeCell"))
        {
            var refAttr = mc.Attribute("ref")?.Value;
            if (string.IsNullOrEmpty(refAttr)) continue;
            // ref="A1:C1" → parse into (row0, col0, row1, col1)
            var parts = refAttr.Split(':');
            if (parts.Length != 2) continue;
            var (r0, c0) = ParseCellRef(parts[0]);
            var (r1, c1) = ParseCellRef(parts[1]);
            if (r0 >= 0 && c0 >= 0 && r1 >= 0 && c1 >= 0)
                result.Add((r0, c0, r1, c1));
        }
        return result;
    }

    /// <summary>Parses a cell reference like "C5" into (row=4, col=2) 0-based.</summary>
    private static (int row, int col) ParseCellRef(string cellRef)
    {
        var col = 0;
        var i = 0;
        while (i < cellRef.Length && char.IsLetter(cellRef[i]))
        {
            col = col * 26 + (char.ToUpper(cellRef[i]) - 'A' + 1);
            i++;
        }
        col--; // Convert 1-based to 0-based
        if (int.TryParse(cellRef[i..], out var row))
            return (row - 1, col); // 0-based
        return (-1, -1);
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
            string valAxisFmtCode = "";
            bool showDataLabelPercent = false;
            bool showDataLabelCatName = false;
            XElement? overlayChartTypeEl = null;
            var overlayChartType = "";
            var overlaySeries = new List<ExcelChartSeries>();

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

                    // Read bar direction: "bar" = horizontal bars, "col" = vertical columns
                    if (chartTypeEl != null)
                    {
                        var barDirEl = chartTypeEl.Element(cns + "barDir");
                        var barDirVal = barDirEl?.Attribute("val")?.Value;
                        if (barDirVal == "bar")
                            chartType = "horizontal_" + chartType;

                        // Read grouping: "clustered", "stacked", "percentStacked"
                        var groupingEl = chartTypeEl.Element(cns + "grouping");
                        var groupingVal = groupingEl?.Attribute("val")?.Value ?? "";
                        if (groupingVal.Contains("stacked", StringComparison.OrdinalIgnoreCase))
                            chartType = groupingVal.Contains("percent", StringComparison.OrdinalIgnoreCase)
                                ? "percentStacked_" + chartType
                                : "stacked_" + chartType;
                    }

                    // Extract axis titles
                    // For scatter/bubble charts, two valAx elements exist:
                    // first = X-axis (category), second = Y-axis (value).
                    var isScatterLike = chartType.Contains("scatter") || chartType.Contains("bubble");
                    var valAxCount = 0;
                    valAxisFmtCode = "";
                    foreach (var ax in plotArea.Elements().Where(e =>
                        e.Name.LocalName.EndsWith("Ax", StringComparison.Ordinal)))
                    {
                        var axTitle = ax.Element(cns + "title");
                        var axTitleText = axTitle != null ? string.Concat(axTitle.Descendants(a + "t").Select(t => t.Value)) : "";
                        var axNumFmt = ax.Element(cns + "numFmt")?.Attribute("formatCode")?.Value ?? "";

                        // catAx / dateAx → category axis; valAx → value axis
                        if (ax.Name.LocalName is "catAx" or "dateAx")
                        {
                            if (!string.IsNullOrEmpty(axTitleText)) catAxisTitle = axTitleText;
                        }
                        else if (ax.Name.LocalName == "valAx")
                        {
                            if (isScatterLike && valAxCount == 0)
                            {
                                if (!string.IsNullOrEmpty(axTitleText)) catAxisTitle = axTitleText;
                            }
                            else
                            {
                                if (!string.IsNullOrEmpty(axTitleText)) valAxisTitle = axTitleText;
                                if (!string.IsNullOrEmpty(axNumFmt)) valAxisFmtCode = axNumFmt;
                            }
                            valAxCount++;
                        }
                    }

                    // Parse data label settings from chart type element
                    if (chartTypeEl != null)
                    {
                        var dLbls = chartTypeEl.Element(cns + "dLbls");
                        if (dLbls != null)
                        {
                            if (dLbls.Element(cns + "showPercent")?.Attribute("val")?.Value == "1")
                                showDataLabelPercent = true;
                            if (dLbls.Element(cns + "showCatName")?.Attribute("val")?.Value == "1")
                                showDataLabelCatName = true;
                        }
                    }
                }

                // Detect overlay chart type (for combo charts, e.g., bar+line)
                if (chartTypeEl != null && plotArea != null)
                {
                    // Look for a second chart type element different from the primary
                    var primaryElementName = chartTypeEl.Name.LocalName;
                    foreach (var tn in new[] { "barChart", "bar3DChart", "lineChart", "line3DChart",
                        "areaChart", "area3DChart", "scatterChart" })
                    {
                        if (tn == primaryElementName) continue; // skip primary
                        var el = plotArea.Element(cns + tn);
                        if (el != null)
                        {
                            overlayChartTypeEl = el;
                            overlayChartType = tn;
                            break;
                        }
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

                        // Categories (or xVal for scatter/bubble charts)
                        string[] cats = Array.Empty<string>();
                        var catEl = ser.Element(cns + "cat") ?? ser.Element(cns + "xVal");
                        if (catEl != null)
                        {
                            cats = ResolveRefElement(catEl, cns, allSheets);
                        }

                        // Values (or yVal for scatter/bubble charts)
                        double[] vals = Array.Empty<double>();
                        var valEl = ser.Element(cns + "val") ?? ser.Element(cns + "yVal");
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

                // Read overlay series (combo chart)
                if (overlayChartTypeEl != null)
                {
                    foreach (var ser in overlayChartTypeEl.Elements(cns + "ser"))
                    {
                        var serName = "";
                        var txEl = ser.Element(cns + "tx");
                        if (txEl != null)
                        {
                            var sv = txEl.Element(cns + "v")?.Value;
                            if (!string.IsNullOrEmpty(sv))
                                serName = sv;
                            else
                            {
                                var strRef = txEl.Element(cns + "strRef");
                                var formula = strRef?.Element(cns + "f")?.Value;
                                if (!string.IsNullOrEmpty(formula))
                                {
                                    var resolved = ResolveCellReference(formula, allSheets);
                                    if (resolved.Length > 0) serName = resolved[0];
                                }
                            }
                        }

                        string[] cats = Array.Empty<string>();
                        var catEl = ser.Element(cns + "cat") ?? ser.Element(cns + "xVal");
                        if (catEl != null)
                            cats = ResolveRefElement(catEl, cns, allSheets);
                        // For overlay series that share the primary category axis,
                        // inherit categories from the primary series if not specified.
                        if (cats.Length == 0 && seriesList.Count > 0)
                            cats = seriesList[0].Categories;

                        double[] vals = Array.Empty<double>();
                        var valEl = ser.Element(cns + "val") ?? ser.Element(cns + "yVal");
                        if (valEl != null)
                        {
                            var valStrings = ResolveRefElement(valEl, cns, allSheets);
                            vals = valStrings.Select(v =>
                                double.TryParse(v, System.Globalization.NumberStyles.Any,
                                    System.Globalization.CultureInfo.InvariantCulture, out var d) ? d : 0.0)
                                .ToArray();
                        }

                        overlaySeries.Add(new ExcelChartSeries(serName, cats, vals));
                    }
                }
            }

            var chartInfo = new ExcelChartInfo(fromRow, fromCol, widthEmu, heightEmu, title, chartType,
                seriesList, catAxisTitle, valAxisTitle, showDataLabelPercent, showDataLabelCatName, valAxisFmtCode)
            {
                OverlaySeries = overlaySeries,
                OverlayChartType = overlayChartType
            };
            charts.Add(chartInfo);
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
/// Represents font styling information for a cell.
/// </summary>
internal sealed record FontStyleInfo(PdfColor? Color, float Size = 11f, bool Bold = false, bool Italic = false);

/// <summary>
/// Represents border styling for one side of a cell.
/// </summary>
internal sealed record BorderSide(string Style, PdfColor? Color);

/// <summary>
/// Represents border styling for a cell (all four sides).
/// </summary>
internal sealed record CellBorderInfo(BorderSide? Left, BorderSide? Right, BorderSide? Top, BorderSide? Bottom);

/// <summary>
/// Represents a cell read from an Excel file.
/// </summary>
internal sealed record ExcelCell(
    string Text,
    PdfColor? Color,
    PdfColor? FillColor,
    string Alignment = "general",
    float FontSize = 11f,
    bool Bold = false,
    bool Italic = false,
    CellBorderInfo? Border = null,
    string VerticalAlignment = "bottom"
);

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
    string ValueAxisTitle = "",     // Y-axis title
    bool ShowDataLabelPercent = false,  // show percentage data labels
    bool ShowDataLabelCatName = false,  // show category name data labels
    string ValueAxisFormatCode = ""    // numFmt formatCode for value axis (e.g. "#,##0")
)
{
    /// <summary>Overlay series for combo charts (e.g., line series over bar chart).</summary>
    public List<ExcelChartSeries> OverlaySeries { get; init; } = new();
    /// <summary>Chart type for overlay series (e.g., "lineChart" when primary is "barChart").</summary>
    public string OverlayChartType { get; init; } = "";
};

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
    /// <summary>Merged cell regions: (startRow, startCol, endRow, endCol) all 0-based.</summary>
    public List<(int StartRow, int StartCol, int EndRow, int EndCol)> MergedCells { get; }
    /// <summary>
    /// Excel row heights keyed by 0-based row index (in points).
    /// Only rows with explicitly customized heights are included.
    /// </summary>
    public Dictionary<int, float> RowHeights { get; }
    /// <summary>Default row height in points (typically 15).</summary>
    public float DefaultRowHeight { get; }
    /// <summary>Whether the sheet uses landscape orientation.</summary>
    public bool IsLandscape { get; }
    /// <summary>Print scale percentage (10-400, default 100).</summary>
    public int PrintScale { get; }
    /// <summary>Paper size code (1=Letter, 9=A4, etc). See ECMA-376 §18.8.22.</summary>
    public int PaperSize { get; }
    /// <summary>Print area range (startCol, startRow, endCol, endRow) 0-based, or null if not set.</summary>
    public (int StartCol, int StartRow, int EndCol, int EndRow)? PrintArea { get; }
    /// <summary>Page margins in points (-1 = use default).</summary>
    public float MarginLeftPt { get; }
    public float MarginRightPt { get; }
    public float MarginTopPt { get; }
    public float MarginBottomPt { get; }
    /// <summary>Whether fitToPage is enabled (auto-scale to fit page width).</summary>
    public bool FitToPage { get; }

    /// <summary>Converts Excel character-unit column width to PDF points.</summary>
    public static float CharUnitsToPoints(float charUnits)
        // Calibrated against LibreOffice reference PDFs: 8.43 char-units → 47.4pt
        => charUnits * 5.62f;

    internal ExcelSheet(string name, List<List<ExcelCell>> rows,
        List<ExcelEmbeddedImage>? images = null,
        Dictionary<int, float>? columnWidths = null,
        float defaultColumnWidth = 8.43f,
        List<ExcelChartInfo>? charts = null,
        List<(int, int, int, int)>? mergedCells = null,
        Dictionary<int, float>? rowHeights = null,
        float defaultRowHeight = 15f,
        bool isLandscape = false,
        int printScale = 100,
        int paperSize = 1,
        (int, int, int, int)? printArea = null,
        float marginLeftPt = -1, float marginRightPt = -1,
        float marginTopPt = -1, float marginBottomPt = -1,
        bool fitToPage = false)
    {
        Name = name;
        Rows = rows;
        Images = images ?? new List<ExcelEmbeddedImage>();
        Charts = charts ?? new List<ExcelChartInfo>();
        ColumnWidths = columnWidths ?? new Dictionary<int, float>();
        DefaultColumnWidth = defaultColumnWidth;
        MergedCells = mergedCells ?? new List<(int, int, int, int)>();
        RowHeights = rowHeights ?? new Dictionary<int, float>();
        DefaultRowHeight = defaultRowHeight;
        IsLandscape = isLandscape;
        PrintScale = printScale;
        PaperSize = paperSize;
        PrintArea = printArea;
        MarginLeftPt = marginLeftPt;
        MarginRightPt = marginRightPt;
        MarginTopPt = marginTopPt;
        MarginBottomPt = marginBottomPt;
        FitToPage = fitToPage;
    }
}
