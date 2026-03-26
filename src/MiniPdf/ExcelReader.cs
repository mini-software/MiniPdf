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
        var (sharedStrings, boldPrefixLengths) = ReadSharedStrings(archive);

        // Read theme colors and styles
        var themeColors = ReadThemeColors(archive);
        var normalFontFamily = ReadNormalFontFamily(archive);
        var normalFontSize = ReadNormalFontSize(archive);
        var maxDigitWidthPx = ExcelSheet.LookupMaxDigitWidthPx(normalFontFamily, normalFontSize);
        var fontStyles = ReadFontStyles(archive, themeColors);
        var fillColors = ReadFillColors(archive, themeColors);
        var borders = ReadBorders(archive, themeColors);
        var numberFormats = ReadNumberFormats(archive);
        var dxfStyles = ReadDxfStyles(archive, themeColors);
        var (cellXfFontIndices, cellXfFillIndices, cellXfNumFmtIds, cellXfAlignments, cellXfVerticalAlignments, cellXfBorderIndices, cellXfWrapTexts, cellXfIndents) = ReadCellXfStyles(archive);

        // Read workbook to get sheet names and order
        var (sheetInfos, printAreas, printTitleRows) = ReadWorkbook(archive);

        // Read each sheet
        var sheetIndex = 0;
        var sheetEntries = new List<ZipArchiveEntry>(); // track entries for conditional formatting pass
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

            var rows = ReadSheet(entry, sharedStrings, boldPrefixLengths, fontStyles, fillColors, borders, numberFormats, cellXfFontIndices, cellXfFillIndices, cellXfNumFmtIds, cellXfAlignments, cellXfVerticalAlignments, cellXfBorderIndices, cellXfWrapTexts, cellXfIndents);
            var images = ReadSheetImages(archive, info.SheetId);
            var drawingShapes = ReadSheetShapes(archive, info.SheetId, themeColors);
            var (colWidths, defaultColWidth) = ReadColumnWidths(entry);
            var mergedCells = ReadMergedCells(entry);
            var (rowHeights, defaultRowHeight, customHeightRows) = ReadRowHeights(entry);
            var rowBreaks = ReadRowBreaks(entry);
            var pageSetup = ReadPageSetup(entry);
            printAreas.TryGetValue(currentIndex, out var printArea);
            printTitleRows.TryGetValue(currentIndex, out var printTitleRow);
            ApplyTableStyleFormatting(archive, info.SheetId, rows, themeColors);
            sheets.Add(new ExcelSheet(info.Name, rows, images, colWidths, defaultColWidth, mergedCells: mergedCells, shapes: drawingShapes, rowHeights: rowHeights, defaultRowHeight: defaultRowHeight, customHeightRows: customHeightRows, isLandscape: pageSetup.IsLandscape, printScale: pageSetup.Scale, paperSize: pageSetup.PaperSize, printArea: printArea != default ? printArea : null, marginLeftPt: pageSetup.MarginLeftPt, marginRightPt: pageSetup.MarginRightPt, marginTopPt: pageSetup.MarginTopPt, marginBottomPt: pageSetup.MarginBottomPt, fitToPage: pageSetup.FitToPage, fitToWidth: pageSetup.FitToWidth, fitToHeight: pageSetup.FitToHeight, horizontalCentered: pageSetup.HorizontalCentered, printTitleRows: printTitleRow != default ? printTitleRow : null, rowBreaks: rowBreaks, oddFooter: pageSetup.OddFooter, footerMarginPt: pageSetup.FooterMarginPt, maxDigitWidthPx: maxDigitWidthPx));
            sheetEntries.Add(entry);
        }

        // If no sheets found via workbook, try reading sheet1 directly
        if (sheets.Count == 0)
        {
            var entry = archive.GetEntry("xl/worksheets/sheet1.xml");
            if (entry != null)
            {
                var rows = ReadSheet(entry, sharedStrings, boldPrefixLengths, fontStyles, fillColors, borders, numberFormats, cellXfFontIndices, cellXfFillIndices, cellXfNumFmtIds, cellXfAlignments, cellXfVerticalAlignments, cellXfBorderIndices, cellXfWrapTexts, cellXfIndents);
                var images = ReadSheetImages(archive, 1);
                var (colWidths, defaultColWidth) = ReadColumnWidths(entry);
                var mergedCells = ReadMergedCells(entry);
                var (rowHeights, defaultRowHeight, customHeightRows) = ReadRowHeights(entry);
                var pageSetup = ReadPageSetup(entry);
                sheets.Add(new ExcelSheet("Sheet1", rows, images, colWidths, defaultColWidth, mergedCells: mergedCells, rowHeights: rowHeights, defaultRowHeight: defaultRowHeight, customHeightRows: customHeightRows, isLandscape: pageSetup.IsLandscape, printScale: pageSetup.Scale, paperSize: pageSetup.PaperSize, marginLeftPt: pageSetup.MarginLeftPt, marginRightPt: pageSetup.MarginRightPt, marginTopPt: pageSetup.MarginTopPt, marginBottomPt: pageSetup.MarginBottomPt, fitToPage: pageSetup.FitToPage, fitToWidth: pageSetup.FitToWidth, fitToHeight: pageSetup.FitToHeight, horizontalCentered: pageSetup.HorizontalCentered, maxDigitWidthPx: maxDigitWidthPx));
            }
        }

        // Propagate paper size: sheets without explicit paperSize inherit from the first sheet that specifies one
        var firstExplicitPaperSize = sheets.FirstOrDefault(s => s.PaperSize > 0)?.PaperSize ?? 1;
        foreach (var sheet in sheets)
        {
            if (sheet.PaperSize == 0)
                sheet.PaperSize = firstExplicitPaperSize;
        }

        // Second pass: read charts (needs sheet data to resolve cell references)
        for (var si = 0; si < sheets.Count; si++)
        {
            var sheetId = si < sheetInfos.Count ? sheetInfos[si].SheetId : 1;
            var charts = ReadSheetCharts(archive, sheetId, sheets);
            foreach (var chart in charts)
                sheets[si].Charts.Add(chart);
        }

        // Third pass: apply conditional formatting (needs all sheet data for defined name resolution)
        if (dxfStyles.Count > 0)
        {
            var definedNames = ReadDefinedNameValues(archive, sheets);
            for (var si = 0; si < sheets.Count && si < sheetEntries.Count; si++)
            {
                var cfRules = ReadConditionalFormatting(sheetEntries[si]);
                if (cfRules.Count > 0)
                    ApplyConditionalFormatting(sheets[si].Rows, cfRules, dxfStyles, definedNames);
            }
        }

        return sheets;
    }

    private static (List<string> Strings, Dictionary<int, int> BoldPrefixLengths) ReadSharedStrings(ZipArchive archive)
    {
        var strings = new List<string>();
        var boldPrefixLengths = new Dictionary<int, int>();
        var entry = archive.GetEntry("xl/sharedStrings.xml");
        if (entry == null) return (strings, boldPrefixLengths);

        using var stream = entry.Open();
        var doc = XDocument.Load(stream);
        var ns = doc.Root?.GetDefaultNamespace() ?? XNamespace.None;

        foreach (var si in doc.Descendants(ns + "si"))
        {
            var runs = si.Elements(ns + "r").ToList();
            if (runs.Count >= 2)
            {
                // Rich text: check if the first run is bold and subsequent runs are not.
                var firstRpr = runs[0].Element(ns + "rPr");
                var firstBold = firstRpr?.Element(ns + "b") != null;
                if (firstBold)
                {
                    var firstText = string.Concat(runs[0].Descendants(ns + "t").Select(t => t.Value));
                    if (firstText.Length > 0)
                        boldPrefixLengths[strings.Count] = firstText.Length;
                }
            }
            var text = string.Concat(si.Descendants(ns + "t").Select(t => t.Value));
            strings.Add(text);
        }

        return (strings, boldPrefixLengths);
    }

    private static (List<SheetInfo> Sheets, Dictionary<int, (int StartCol, int StartRow, int EndCol, int EndRow)> PrintAreas, Dictionary<int, (int StartRow, int EndRow)> PrintTitleRows) ReadWorkbook(ZipArchive archive)
    {
        var result = new List<SheetInfo>();
        var printAreas = new Dictionary<int, (int, int, int, int)>();
        var printTitleRows = new Dictionary<int, (int, int)>();
        var entry = archive.GetEntry("xl/workbook.xml");
        if (entry == null) return (result, printAreas, printTitleRows);

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
            var localIdStr = dn.Attribute("localSheetId")?.Value;
            if (localIdStr == null || !int.TryParse(localIdStr, out var localId)) continue;
            var val = dn.Value;
            if (string.IsNullOrEmpty(val)) continue;

            if (string.Equals(dnName, "_xlnm.Print_Area", StringComparison.OrdinalIgnoreCase))
            {
                // e.g. "Paystubs!$A$1:$J$37"
                // Remove sheet name prefix
                var bangIdx = val.IndexOf('!');
                if (bangIdx >= 0) val = val.Substring(bangIdx + 1);
                // Parse range like $A$1:$J$37 — strip $ and use ParseCellRef which returns (row, col)
                // Also handle column-only ranges like $A:$L (entire columns)
                var parts = val.Replace("$", "").Split(':');
                if (parts.Length == 2)
                {
                    var (sr, sc) = ParseCellRef(parts[0]);
                    var (er, ec) = ParseCellRef(parts[1]);
                    // Handle column-only ranges (e.g. A:L) where ParseCellRef returns row=-1
                    if (sc >= 0 && sr < 0) sr = 0;
                    if (ec >= 0 && er < 0) er = 1_048_575; // max Excel row index
                    if (sc >= 0 && sr >= 0 && ec >= 0 && er >= 0)
                        printAreas[localId] = (sc, sr, ec, er);
                }
            }
            else if (string.Equals(dnName, "_xlnm.Print_Titles", StringComparison.OrdinalIgnoreCase))
            {
                // e.g. "'Sheet'!$1:$4" — row-only range for repeating header rows
                var bangIdx = val.IndexOf('!');
                if (bangIdx >= 0) val = val.Substring(bangIdx + 1);
                var parts = val.Replace("$", "").Split(':');
                if (parts.Length == 2)
                {
                    // Try row-only range (e.g. "1:4")
                    if (int.TryParse(parts[0], out var startRow) && int.TryParse(parts[1], out var endRow))
                        printTitleRows[localId] = (startRow - 1, endRow - 1); // convert to 0-based
                }
            }
        }

        return (result, printAreas, printTitleRows);
    }

    private static List<FontStyleInfo> ReadFontStyles(ZipArchive archive, List<PdfColor> themeColors)
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
            var colorEl = font.Element(ns + "color");
            PdfColor? color = ResolveColorElement(colorEl, themeColors);

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

            // Read font name
            string? fontName = null;
            var nameEl = font.Element(ns + "name");
            if (nameEl != null)
                fontName = nameEl.Attribute("val")?.Value;

            // Read bold / italic / underline
            var bold = font.Element(ns + "b") != null;
            var italic = font.Element(ns + "i") != null;
            var underline = font.Element(ns + "u") != null;

            styles.Add(new FontStyleInfo(color, fontSize, bold, italic, underline, fontName));
        }

        return styles;
    }

    /// <summary>
    /// Reads border definitions from styles.xml.
    /// Returns a list of CellBorderInfo indexed by borderId.
    /// </summary>
    private static List<CellBorderInfo?> ReadBorders(ZipArchive archive, List<PdfColor> themeColors)
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
            var left = ReadBorderSide(border.Element(ns + "left"), ns, themeColors);
            var right = ReadBorderSide(border.Element(ns + "right"), ns, themeColors);
            var top = ReadBorderSide(border.Element(ns + "top"), ns, themeColors);
            var bottom = ReadBorderSide(border.Element(ns + "bottom"), ns, themeColors);

            if (left == null && right == null && top == null && bottom == null)
                borders.Add(null);
            else
                borders.Add(new CellBorderInfo(left, right, top, bottom));
        }

        return borders;
    }

    private static BorderSide? ReadBorderSide(XElement? el, XNamespace ns, List<PdfColor> themeColors)
    {
        if (el == null) return null;
        var style = el.Attribute("style")?.Value;
        if (string.IsNullOrEmpty(style) || style == "none") return null;

        var color = ResolveColorElement(el.Element(ns + "color"), themeColors);
        // Default border color is black
        color ??= PdfColor.FromRgb(0, 0, 0);
        return new BorderSide(style, color);
    }

    /// <summary>
    /// Reads table style definitions from styles.xml and maps each table style name
    /// to the border info defined by the wholeTable DXF element.
    /// </summary>
    private static Dictionary<string, CellBorderInfo> ReadTableStyleBorders(ZipArchive archive, List<PdfColor> themeColors)
    {
        var result = new Dictionary<string, CellBorderInfo>(StringComparer.OrdinalIgnoreCase);
        var entry = archive.GetEntry("xl/styles.xml");
        if (entry == null) return result;

        using var stream = entry.Open();
        var doc = XDocument.Load(stream);
        var ns = doc.Root?.GetDefaultNamespace() ?? XNamespace.None;

        // Read all DXF border entries
        var dxfBorders = new List<CellBorderInfo?>();
        var dxfs = doc.Descendants(ns + "dxfs").FirstOrDefault();
        if (dxfs != null)
        {
            foreach (var dxf in dxfs.Elements(ns + "dxf"))
            {
                var borderEl = dxf.Element(ns + "border");
                if (borderEl != null)
                {
                    var left = ReadBorderSide(borderEl.Element(ns + "left"), ns, themeColors);
                    var right = ReadBorderSide(borderEl.Element(ns + "right"), ns, themeColors);
                    var top = ReadBorderSide(borderEl.Element(ns + "top"), ns, themeColors);
                    var bottom = ReadBorderSide(borderEl.Element(ns + "bottom"), ns, themeColors);
                    dxfBorders.Add(left != null || right != null || top != null || bottom != null
                        ? new CellBorderInfo(left, right, top, bottom)
                        : null);
                }
                else
                {
                    dxfBorders.Add(null);
                }
            }
        }

        // Map table style names to their wholeTable border
        foreach (var ts in doc.Descendants(ns + "tableStyle"))
        {
            var name = ts.Attribute("name")?.Value;
            if (string.IsNullOrEmpty(name)) continue;

            var wholeTable = ts.Elements(ns + "tableStyleElement")
                .FirstOrDefault(e => e.Attribute("type")?.Value == "wholeTable");
            if (wholeTable == null) continue;

            if (int.TryParse(wholeTable.Attribute("dxfId")?.Value, out var dxfId) &&
                dxfId >= 0 && dxfId < dxfBorders.Count && dxfBorders[dxfId] != null)
            {
                result[name] = dxfBorders[dxfId]!;
            }
        }

        return result;
    }

    /// <summary>
    /// Reads table definitions for a worksheet and applies table-style borders
    /// to cells within each table range that don't already have explicit borders.
    /// </summary>
    private static void ApplyTableBorders(ZipArchive archive, int sheetId,
        List<List<ExcelCell>> rows, Dictionary<string, CellBorderInfo> tableStyleBorders)
    {
        if (tableStyleBorders.Count == 0) return;

        var relsPath = $"xl/worksheets/_rels/sheet{sheetId}.xml.rels";
        var relsEntry = archive.GetEntry(relsPath);
        if (relsEntry == null) return;

        var tableFiles = new List<string>();
        using (var relsStream = relsEntry.Open())
        {
            var relsDoc = XDocument.Load(relsStream);
            foreach (var rel in relsDoc.Descendants())
            {
                if (rel.Attribute("Type")?.Value?.EndsWith("/table", StringComparison.OrdinalIgnoreCase) == true)
                {
                    var target = rel.Attribute("Target")?.Value;
                    if (!string.IsNullOrEmpty(target))
                        tableFiles.Add(System.IO.Path.GetFileName(target));
                }
            }
        }

        foreach (var tableFile in tableFiles)
        {
            var tableEntry = archive.GetEntry($"xl/tables/{tableFile}");
            if (tableEntry == null) continue;

            string? refAttr, styleName;
            using (var tableStream = tableEntry.Open())
            {
                var tableDoc = XDocument.Load(tableStream);
                refAttr = tableDoc.Root?.Attribute("ref")?.Value;
                var styleInfo = tableDoc.Root?.Descendants().FirstOrDefault(e => e.Name.LocalName == "tableStyleInfo");
                styleName = styleInfo?.Attribute("name")?.Value;
            }

            if (string.IsNullOrEmpty(refAttr) || string.IsNullOrEmpty(styleName)) continue;
            if (!tableStyleBorders.TryGetValue(styleName, out var tableBorder)) continue;

            var rangeParts = refAttr.Split(':');
            if (rangeParts.Length != 2) continue;

            var (startRow, startCol) = ParseCellRef(rangeParts[0]);
            var (endRow, endCol) = ParseCellRef(rangeParts[1]);

            for (var r = startRow; r <= endRow && r < rows.Count; r++)
            {
                var row = rows[r];
                // Ensure row has enough cells to cover table columns
                while (row.Count <= endCol)
                    row.Add(new ExcelCell(string.Empty, null, null));

                for (var c = startCol; c <= endCol; c++)
                {
                    var cell = row[c];
                    // Do not synthesize borders into empty cells.
                    // Empty-table grid lines are not rendered by reference output.
                    if (cell.Border == null && !string.IsNullOrEmpty(cell.Text))
                    {
                        row[c] = cell with { Border = tableBorder };
                    }
                }
            }
        }
    }

    /// <summary>
    /// Reads table definitions for a worksheet and applies table-style formatting
    /// (bold, fill) to header rows, total rows, and first-column cells.
    /// Resolves dxf-based style info including theme colors for fills.
    /// </summary>
    private static void ApplyTableStyleFormatting(ZipArchive archive, int sheetId,
        List<List<ExcelCell>> rows, List<PdfColor> themeColors)
    {
        var relsPath = $"xl/worksheets/_rels/sheet{sheetId}.xml.rels";
        var relsEntry = archive.GetEntry(relsPath);
        if (relsEntry == null) return;

        var tableFiles = new List<string>();
        using (var relsStream = relsEntry.Open())
        {
            var relsDoc = XDocument.Load(relsStream);
            foreach (var rel in relsDoc.Descendants())
            {
                if (rel.Attribute("Type")?.Value?.EndsWith("/table", StringComparison.OrdinalIgnoreCase) == true)
                {
                    var target = rel.Attribute("Target")?.Value;
                    if (!string.IsNullOrEmpty(target))
                        tableFiles.Add(System.IO.Path.GetFileName(target));
                }
            }
        }

        if (tableFiles.Count == 0) return;

        // Read dxf entries and table style mappings from styles.xml
        var stylesEntry = archive.GetEntry("xl/styles.xml");
        if (stylesEntry == null) return;

        List<(bool? Bold, PdfColor? FillColor)> dxfInfos;
        Dictionary<string, (int HeaderDxf, int TotalDxf, int WholeDxf, int FirstColDxf, int FirstRowStripeDxf)> tableStyleMap;
        using (var stylesStream = stylesEntry.Open())
        {
            var stylesDoc = XDocument.Load(stylesStream);
            var ns = stylesDoc.Root?.GetDefaultNamespace() ?? XNamespace.None;

            // Parse dxf entries
            dxfInfos = new List<(bool?, PdfColor?)>();
            var dxfs = stylesDoc.Descendants(ns + "dxfs").FirstOrDefault();
            if (dxfs != null)
            {
                foreach (var dxf in dxfs.Elements(ns + "dxf"))
                {
                    bool? bold = null;
                    PdfColor? fillColor = null;

                    var fontEl = dxf.Element(ns + "font");
                    if (fontEl != null)
                    {
                        var bEl = fontEl.Element(ns + "b");
                        if (bEl != null)
                        {
                            var val = bEl.Attribute("val")?.Value;
                            bold = val == null || val == "1" || string.Equals(val, "true", StringComparison.OrdinalIgnoreCase);
                        }
                    }

                    var fillEl = dxf.Element(ns + "fill");
                    if (fillEl != null)
                    {
                        var bgEl = fillEl.Descendants(ns + "bgColor").FirstOrDefault();
                        if (bgEl != null)
                            fillColor = ResolveColorElement(bgEl, themeColors);
                        if (fillColor == null)
                        {
                            var fgEl = fillEl.Descendants(ns + "fgColor").FirstOrDefault();
                            if (fgEl != null)
                                fillColor = ResolveColorElement(fgEl, themeColors);
                        }
                    }

                    dxfInfos.Add((bold, fillColor));
                }
            }

            // Parse table styles: map style name → (headerRow dxfId, totalRow dxfId, wholeTable dxfId, firstColumn dxfId, firstRowStripe dxfId)
            tableStyleMap = new Dictionary<string, (int, int, int, int, int)>(StringComparer.OrdinalIgnoreCase);
            foreach (var ts in stylesDoc.Descendants(ns + "tableStyle"))
            {
                var name = ts.Attribute("name")?.Value;
                if (string.IsNullOrEmpty(name)) continue;

                int headerDxf = -1, totalDxf = -1, wholeDxf = -1, firstColDxf = -1, firstRowStripeDxf = -1;
                foreach (var tse in ts.Elements(ns + "tableStyleElement"))
                {
                    var type = tse.Attribute("type")?.Value;
                    if (int.TryParse(tse.Attribute("dxfId")?.Value, out var dxfId))
                    {
                        if (type == "headerRow") headerDxf = dxfId;
                        else if (type == "totalRow") totalDxf = dxfId;
                        else if (type == "wholeTable") wholeDxf = dxfId;
                        else if (type == "firstColumn") firstColDxf = dxfId;
                        else if (type == "firstRowStripe") firstRowStripeDxf = dxfId;
                    }
                }
                tableStyleMap[name] = (headerDxf, totalDxf, wholeDxf, firstColDxf, firstRowStripeDxf);
            }
        }

        foreach (var tableFile in tableFiles)
        {
            var tableEntry = archive.GetEntry($"xl/tables/{tableFile}");
            if (tableEntry == null) continue;

            string? refAttr, styleName;
            int headerRowCount = 1;
            int totalsRowCount = 0;
            bool showFirstColumn = false;
            bool showRowStripes = false;
            using (var tableStream = tableEntry.Open())
            {
                var tableDoc = XDocument.Load(tableStream);
                refAttr = tableDoc.Root?.Attribute("ref")?.Value;

                if (int.TryParse(tableDoc.Root?.Attribute("headerRowCount")?.Value, out var hrc))
                    headerRowCount = hrc;
                if (int.TryParse(tableDoc.Root?.Attribute("totalsRowCount")?.Value, out var trc))
                    totalsRowCount = trc;

                var styleInfo = tableDoc.Root?.Descendants().FirstOrDefault(e => e.Name.LocalName == "tableStyleInfo");
                styleName = styleInfo?.Attribute("name")?.Value;
                if (styleInfo != null)
                {
                    showFirstColumn = styleInfo.Attribute("showFirstColumn")?.Value == "1";
                    showRowStripes = styleInfo.Attribute("showRowStripes")?.Value == "1";
                }
            }

            if (string.IsNullOrEmpty(refAttr)) continue;
            var rangeParts = refAttr.Split(':');
            if (rangeParts.Length != 2) continue;

            var (startRow, startCol) = ParseCellRef(rangeParts[0]);
            var (endRow, endCol) = ParseCellRef(rangeParts[1]);


            // Resolve table style dxf entries
            (bool? Bold, PdfColor? FillColor) headerStyle = (true, null);
            (bool? Bold, PdfColor? FillColor) totalStyle = (true, null);
            (bool? Bold, PdfColor? FillColor) wholeStyle = (null, null);
            (bool? Bold, PdfColor? FillColor) firstColStyle = (null, null);
            PdfColor? firstRowStripeFill = null;

            if (!string.IsNullOrEmpty(styleName) && tableStyleMap.TryGetValue(styleName, out var dxfIds))
            {
                if (dxfIds.HeaderDxf >= 0 && dxfIds.HeaderDxf < dxfInfos.Count)
                {
                    var dxf = dxfInfos[dxfIds.HeaderDxf];
                    // Preserve default bold=true for header rows when DXF doesn't override it
                    headerStyle = (dxf.Bold ?? headerStyle.Bold, dxf.FillColor ?? headerStyle.FillColor);
                }
                if (dxfIds.TotalDxf >= 0 && dxfIds.TotalDxf < dxfInfos.Count)
                {
                    var dxf = dxfInfos[dxfIds.TotalDxf];
                    totalStyle = (dxf.Bold ?? totalStyle.Bold, dxf.FillColor ?? totalStyle.FillColor);
                }
                if (dxfIds.WholeDxf >= 0 && dxfIds.WholeDxf < dxfInfos.Count)
                    wholeStyle = dxfInfos[dxfIds.WholeDxf];
                if (dxfIds.FirstColDxf >= 0 && dxfIds.FirstColDxf < dxfInfos.Count)
                    firstColStyle = dxfInfos[dxfIds.FirstColDxf];
                if (dxfIds.FirstRowStripeDxf >= 0 && dxfIds.FirstRowStripeDxf < dxfInfos.Count)
                    firstRowStripeFill = dxfInfos[dxfIds.FirstRowStripeDxf].FillColor;
            }

            void ApplyStyle(List<ExcelCell> row, int c, bool? bold, PdfColor? fill)
            {
                if (c >= row.Count) return;
                var cell = row[c];
                var newBold = bold == true && !cell.Bold ? true : cell.Bold;
                var newFill = fill != null && cell.FillColor == null ? fill : cell.FillColor;
                if (newBold != cell.Bold || newFill != cell.FillColor)
                    row[c] = cell with { Bold = newBold, FillColor = newFill };
            }

            // Header rows: apply table headerRow bold and fill.
            for (var r = startRow; r < startRow + headerRowCount && r <= endRow && r < rows.Count; r++)
            {
                for (var c = startCol; c <= endCol; c++)
                    ApplyStyle(rows[r], c, headerStyle.Bold, headerStyle.FillColor);
            }

            // Totals rows
            if (totalsRowCount > 0)
            {
                for (var r = endRow - totalsRowCount + 1; r <= endRow && r < rows.Count; r++)
                {
                    for (var c = startCol; c <= endCol; c++)
                        ApplyStyle(rows[r], c, totalStyle.Bold, totalStyle.FillColor);
                }
            }

            // Data rows: apply wholeTable fill and row stripe banding
            var dataStart = startRow + headerRowCount;
            var dataEnd = totalsRowCount > 0 ? endRow - totalsRowCount : endRow;
            for (var r = dataStart; r <= dataEnd && r < rows.Count; r++)
            {
                var dataRowIndex = r - dataStart;
                var isFirstStripe = showRowStripes && firstRowStripeFill != null && (dataRowIndex % 2 == 0);

                for (var c = startCol; c <= endCol; c++)
                {
                    if (c >= rows[r].Count) continue;
                    var cell = rows[r][c];
                    PdfColor? newFill = cell.FillColor;

                    if (isFirstStripe)
                    {
                        // Banded row: apply firstRowStripe fill when cell has no
                        // fill or its fill matches the wholeTable default.
                        if (newFill == null || ColorsMatch(newFill, wholeStyle.FillColor))
                            newFill = firstRowStripeFill;
                    }
                    else if (wholeStyle.FillColor != null && newFill == null)
                    {
                        newFill = wholeStyle.FillColor;
                    }

                    var newBold = wholeStyle.Bold == true && !cell.Bold ? true : cell.Bold;
                    if (newBold != cell.Bold || newFill != cell.FillColor)
                        rows[r][c] = cell with { Bold = newBold, FillColor = newFill };
                }
            }

            // First column: apply firstColumn DXF style (when showFirstColumn)
            if (showFirstColumn && (firstColStyle.Bold != null || firstColStyle.FillColor != null))
            {
                for (var r = dataStart; r <= dataEnd && r < rows.Count; r++)
                    ApplyStyle(rows[r], startCol, firstColStyle.Bold, firstColStyle.FillColor);
            }
        }
    }

    /// <summary>
    /// Compares two PdfColor values allowing a small tolerance for theme rounding.
    /// </summary>
    private static bool ColorsMatch(PdfColor? a, PdfColor? b)
        => a.HasValue && b.HasValue
            && Math.Abs(a.Value.R - b.Value.R) <= 0.02f
            && Math.Abs(a.Value.G - b.Value.G) <= 0.02f
            && Math.Abs(a.Value.B - b.Value.B) <= 0.02f;

    /// <summary>
    /// Reads cellXf style entries from styles.xml.
    /// Returns (fontIndices, fillIndices, numFmtIds) parallel lists.
    /// </summary>
    private static (List<int> FontIndices, List<int> FillIndices, List<int> NumFmtIds, List<string> Alignments, List<string> VerticalAlignments, List<int> BorderIndices, List<bool> WrapTexts, List<int> Indents) ReadCellXfStyles(ZipArchive archive)
    {
        var fontIndices = new List<int>();
        var fillIndices = new List<int>();
        var numFmtIds = new List<int>();
        var alignments = new List<string>();
        var verticalAlignments = new List<string>();
        var borderIndices = new List<int>();
        var wrapTexts = new List<bool>();
        var indents = new List<int>();
        var entry = archive.GetEntry("xl/styles.xml");
        if (entry == null) return (fontIndices, fillIndices, numFmtIds, alignments, verticalAlignments, borderIndices, wrapTexts, indents);

        using var stream = entry.Open();
        var doc = XDocument.Load(stream);
        var ns = doc.Root?.GetDefaultNamespace() ?? XNamespace.None;

        // Read <cellXfs> -> <xf> elements
        var cellXfs = doc.Descendants(ns + "cellXfs").FirstOrDefault();
        if (cellXfs == null) return (fontIndices, fillIndices, numFmtIds, alignments, verticalAlignments, borderIndices, wrapTexts, indents);

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

            var wrapTextAttr = xf.Element(ns + "alignment")?.Attribute("wrapText")?.Value;
            wrapTexts.Add(wrapTextAttr == "1" || string.Equals(wrapTextAttr, "true", StringComparison.OrdinalIgnoreCase));

            var indentAttr = xf.Element(ns + "alignment")?.Attribute("indent")?.Value;
            indents.Add(int.TryParse(indentAttr, out var ind) ? ind : 0);
        }

        return (fontIndices, fillIndices, numFmtIds, alignments, verticalAlignments, borderIndices, wrapTexts, indents);
    }

    /// <summary>
    /// Reads fill patterns from styles.xml.
    /// Returns a list of fill colors indexed by fillId (null for none/gray125).
    /// </summary>
    private static List<PdfColor?> ReadFillColors(ZipArchive archive, List<PdfColor> themeColors)
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

            PdfColor? fgPdf = ResolveColorElement(fgColor, themeColors);
            PdfColor? bgPdf = ResolveColorElement(bgColor, themeColors);

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
    private static PdfColor? ResolveColorElement(XElement? el, List<PdfColor> themeColors)
    {
        if (el == null) return null;
        var rgb = el.Attribute("rgb")?.Value;
        if (!string.IsNullOrEmpty(rgb)) return PdfColor.FromHex(rgb);
        var indexed = el.Attribute("indexed")?.Value;
        if (!string.IsNullOrEmpty(indexed) && int.TryParse(indexed, out var idx))
            return GetIndexedColor(idx);
        var themeAttr = el.Attribute("theme")?.Value;
        if (!string.IsNullOrEmpty(themeAttr) && int.TryParse(themeAttr, out var themeIdx)
            && themeIdx >= 0 && themeIdx < themeColors.Count)
        {
            var c = themeColors[themeIdx];
            var tintAttr = el.Attribute("tint")?.Value;
            if (!string.IsNullOrEmpty(tintAttr) && double.TryParse(tintAttr,
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out var tint))
            {
                c = ApplyTint(c, tint);
            }
            return c;
        }
        return null;
    }

    private static PdfColor ApplyTint(PdfColor color, double tint)
    {
        // ECMA-376 §18.8.19: Apply tint via HSL luminance adjustment.
        // Convert RGB → HSL, adjust L, convert back.
        float r = color.R, g = color.G, b = color.B;
        float max = Math.Max(r, Math.Max(g, b));
        float min = Math.Min(r, Math.Min(g, b));
        float h = 0, s = 0, l = (max + min) / 2f;

        if (max != min)
        {
            float d = max - min;
            s = l > 0.5f ? d / (2f - max - min) : d / (max + min);
            if (max == r) h = (g - b) / d + (g < b ? 6 : 0);
            else if (max == g) h = (b - r) / d + 2;
            else h = (r - g) / d + 4;
            h /= 6f;
        }

        // Apply tint to luminance
        if (tint < 0)
            l = (float)(l * (1.0 + tint));
        else
            l = (float)(l * (1.0 - tint) + tint);

        // HSL → RGB
        if (s == 0)
        {
            r = g = b = l;
        }
        else
        {
            float q = l < 0.5f ? l * (1 + s) : l + s - l * s;
            float p = 2 * l - q;
            r = HueToRgb(p, q, h + 1f / 3f);
            g = HueToRgb(p, q, h);
            b = HueToRgb(p, q, h - 1f / 3f);
        }

        return new PdfColor(
            Compat.Clamp(r, 0f, 1f),
            Compat.Clamp(g, 0f, 1f),
            Compat.Clamp(b, 0f, 1f));
    }

    private static float HueToRgb(float p, float q, float t)
    {
        if (t < 0) t += 1f;
        if (t > 1) t -= 1f;
        if (t < 1f / 6f) return p + (q - p) * 6f * t;
        if (t < 1f / 2f) return q;
        if (t < 2f / 3f) return p + (q - p) * (2f / 3f - t) * 6f;
        return p;
    }

    /// <summary>
    /// Reads the minor (body/normal) font family name from the workbook theme.
    /// Falls back to the first font in styles.xml if the theme is unavailable.
    /// </summary>
    private static string? ReadNormalFontFamily(ZipArchive archive)
    {
        // Try theme first — the minor font is the "body" / normal style font
        var themeEntry = archive.GetEntry("xl/theme/theme1.xml");
        if (themeEntry != null)
        {
            using var ts = themeEntry.Open();
            var tdoc = XDocument.Load(ts);
            var ans = XNamespace.Get("http://schemas.openxmlformats.org/drawingml/2006/main");
            var minorFont = tdoc.Descendants(ans + "minorFont").FirstOrDefault();
            var latin = minorFont?.Element(ans + "latin");
            var typeface = latin?.Attribute("typeface")?.Value;
            if (!string.IsNullOrWhiteSpace(typeface))
                return typeface;
        }

        // Fallback: first <font> in styles.xml (index 0 = normal style in most workbooks)
        var stylesEntry = archive.GetEntry("xl/styles.xml");
        if (stylesEntry != null)
        {
            using var ss = stylesEntry.Open();
            var sdoc = XDocument.Load(ss);
            var sNs = sdoc.Root?.GetDefaultNamespace() ?? XNamespace.None;
            var firstFont = sdoc.Descendants(sNs + "font").FirstOrDefault();
            var name = firstFont?.Element(sNs + "name")?.Attribute("val")?.Value;
            if (!string.IsNullOrWhiteSpace(name))
                return name;
        }

        return null;
    }

    /// <summary>
    /// Reads the Normal style font size (in points) from styles.xml.
    /// The Normal style is cellStyleXfs[0]; its fontId → fonts[fontId].sz gives the size.
    /// Returns 11 (modern Excel default) when the size cannot be determined.
    /// </summary>
    private static float ReadNormalFontSize(ZipArchive archive)
    {
        var stylesEntry = archive.GetEntry("xl/styles.xml");
        if (stylesEntry == null) return 11f;

        using var ss = stylesEntry.Open();
        var sdoc = XDocument.Load(ss);
        var ns = sdoc.Root?.GetDefaultNamespace() ?? XNamespace.None;

        // cellStyleXfs[0] is the Normal style
        var cellStyleXfs = sdoc.Descendants(ns + "cellStyleXfs").FirstOrDefault();
        var firstXf = cellStyleXfs?.Elements(ns + "xf").FirstOrDefault();
        if (firstXf == null) return 11f;

        if (!int.TryParse(firstXf.Attribute("fontId")?.Value, out var fontId))
            return 11f;

        var fonts = sdoc.Descendants(ns + "fonts").FirstOrDefault();
        if (fonts == null) return 11f;

        var fontEls = fonts.Elements(ns + "font").ToList();
        if (fontId < 0 || fontId >= fontEls.Count) return 11f;

        var szEl = fontEls[fontId].Element(ns + "sz");
        if (szEl != null && float.TryParse(szEl.Attribute("val")?.Value,
            System.Globalization.NumberStyles.Any,
            System.Globalization.CultureInfo.InvariantCulture, out var sz) && sz > 0)
        {
            return sz;
        }

        return 11f;
    }

    private static List<PdfColor> ReadThemeColors(ZipArchive archive)
    {
        var colors = new List<PdfColor>();
        var entry = archive.GetEntry("xl/theme/theme1.xml");
        if (entry == null) return colors;

        using var stream = entry.Open();
        var doc = XDocument.Load(stream);
        var ns = XNamespace.Get("http://schemas.openxmlformats.org/drawingml/2006/main");
        var scheme = doc.Descendants(ns + "clrScheme").FirstOrDefault();
        if (scheme == null) return colors;

        // Excel theme index order: 0=lt1, 1=dk1, 2=lt2, 3=dk2, 4-9=accent1-6, 10=hlink, 11=folHlink
        var names = new[] { "lt1", "dk1", "lt2", "dk2", "accent1", "accent2",
                            "accent3", "accent4", "accent5", "accent6", "hlink", "folHlink" };
        foreach (var name in names)
        {
            var el = scheme.Element(ns + name);
            if (el != null)
            {
                var srgb = el.Element(ns + "srgbClr");
                var sys = el.Element(ns + "sysClr");
                string? hex = srgb?.Attribute("val")?.Value ?? sys?.Attribute("lastClr")?.Value;
                if (!string.IsNullOrEmpty(hex))
                    colors.Add(PdfColor.FromHex(hex));
                else
                    colors.Add(new PdfColor(0, 0, 0));
            }
            else
                colors.Add(new PdfColor(0, 0, 0));
        }
        return colors;
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

    /// <summary>
    /// Reads differential formatting (dxf) styles from styles.xml.
    /// These are used by conditional formatting rules to override cell appearance.
    /// </summary>
    private static List<(PdfColor? FontColor, PdfColor? FillColor)> ReadDxfStyles(ZipArchive archive, List<PdfColor> themeColors)
    {
        var result = new List<(PdfColor? FontColor, PdfColor? FillColor)>();
        var entry = archive.GetEntry("xl/styles.xml");
        if (entry == null) return result;

        using var stream = entry.Open();
        var doc = XDocument.Load(stream);
        var ns = doc.Root?.GetDefaultNamespace() ?? XNamespace.None;

        var dxfs = doc.Descendants(ns + "dxfs").FirstOrDefault();
        if (dxfs == null) return result;

        foreach (var dxf in dxfs.Elements(ns + "dxf"))
        {
            PdfColor? fontColor = null;
            PdfColor? fillColor = null;

            var fontEl = dxf.Element(ns + "font");
            if (fontEl != null)
            {
                var colorEl = fontEl.Element(ns + "color");
                fontColor = ResolveColorElement(colorEl, themeColors);
            }

            var fillEl = dxf.Element(ns + "fill");
            if (fillEl != null)
            {
                var bgEl = fillEl.Descendants(ns + "bgColor").FirstOrDefault()
                        ?? fillEl.Descendants(ns + "fgColor").FirstOrDefault();
                fillColor = ResolveColorElement(bgEl, themeColors);
            }

            result.Add((fontColor, fillColor));
        }

        return result;
    }

    /// <summary>
    /// Reads conditional formatting rules from a worksheet entry.
    /// </summary>
    private static List<(string Sqref, string Type, string Operator, string Formula, int DxfId)> ReadConditionalFormatting(ZipArchiveEntry entry)
    {
        var rules = new List<(string, string, string, string, int)>();
        using var stream = entry.Open();
        var doc = XDocument.Load(stream);
        var ns = doc.Root?.GetDefaultNamespace() ?? XNamespace.None;

        foreach (var cf in doc.Descendants(ns + "conditionalFormatting"))
        {
            var sqref = cf.Attribute("sqref")?.Value ?? "";
            foreach (var rule in cf.Elements(ns + "cfRule"))
            {
                var type = rule.Attribute("type")?.Value ?? "";
                var op = rule.Attribute("operator")?.Value ?? "";
                var formula = rule.Element(ns + "formula")?.Value ?? "";
                var dxfIdStr = rule.Attribute("dxfId")?.Value;
                if (int.TryParse(dxfIdStr, out var dxfId))
                    rules.Add((sqref, type, op, formula, dxfId));
            }
        }

        return rules;
    }

    /// <summary>
    /// Reads defined name values from workbook.xml, resolving simple cell references to their values
    /// from the given sheets data.
    /// </summary>
    private static Dictionary<string, double> ReadDefinedNameValues(ZipArchive archive, List<ExcelSheet> sheets)
    {
        var values = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
        var entry = archive.GetEntry("xl/workbook.xml");
        if (entry == null) return values;

        using var stream = entry.Open();
        var doc = XDocument.Load(stream);
        var ns = doc.Root?.GetDefaultNamespace() ?? XNamespace.None;

        foreach (var dn in doc.Descendants(ns + "definedName"))
        {
            var name = dn.Attribute("name")?.Value;
            if (string.IsNullOrEmpty(name) || name.StartsWith("_xlnm.")) continue;

            var val = dn.Value; // e.g. "'Cash flow forecast'!$J$4"
            if (string.IsNullOrEmpty(val)) continue;

            // Parse sheet reference: 'Sheet Name'!$COL$ROW or SheetName!$COL$ROW
            var bangIdx = val.IndexOf('!');
            if (bangIdx < 0) continue;

            var sheetRef = val[..bangIdx].Trim('\'');
            var cellRef = val[(bangIdx + 1)..].Replace("$", "");

            // Find the sheet by name
            var sheet = sheets.FirstOrDefault(s => string.Equals(s.Name, sheetRef, StringComparison.OrdinalIgnoreCase));
            if (sheet == null) continue;

            // Parse cell reference
            var (row, col) = ParseCellRef(cellRef);
            if (row < 0 || col < 0 || row >= sheet.Rows.Count) continue;
            var rowData = sheet.Rows[row];
            if (col >= rowData.Count) continue;

            // Try to parse the cell's text as a number (stripping currency formatting)
            var cellText = rowData[col].Text;
            if (TryGetCellNumericValue(rowData[col], out var numVal))
            {
                values[name] = numVal;
            }
            else if (DateTime.TryParse(cellText, System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None, out var dateVal))
            {
                // Date-formatted cell: convert back to Excel serial number
                values[name] = DateToExcelSerial(dateVal);
            }
        }

        return values;
    }

    /// <summary>
    /// Applies conditional formatting rules to cell data, overriding font/fill colors where rules match.
    /// </summary>
    private static void ApplyConditionalFormatting(
        List<List<ExcelCell>> rows,
        List<(string Sqref, string Type, string Operator, string Formula, int DxfId)> cfRules,
        List<(PdfColor? FontColor, PdfColor? FillColor)> dxfStyles,
        Dictionary<string, double> definedNames)
    {
        foreach (var (sqref, type, op, formula, dxfId) in cfRules)
        {
            if (dxfId < 0 || dxfId >= dxfStyles.Count) continue;
            var (fontColor, fillColor) = dxfStyles[dxfId];
            if (fontColor == null && fillColor == null) continue;

            // For expression-type CFs, try range-wide evaluation first (e.g. NAME+N=TODAY())
            bool? rangeWideResult = null;
            if (type == "expression")
                rangeWideResult = EvaluateRangeExpression(formula, definedNames);

            // Parse sqref (may contain multiple space-separated ranges)
            foreach (var rangeStr in sqref.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            {
                var rangeParts = rangeStr.Split(':');
                var (startRow, startCol) = ParseCellRef(rangeParts[0]);
                var (endRow, endCol) = rangeParts.Length > 1 ? ParseCellRef(rangeParts[1]) : (startRow, startCol);
                if (startRow < 0) startRow = 0;
                if (endRow < 0) endRow = rows.Count - 1;

                // Range-wide expression: apply to all cells (including empty) if true
                if (rangeWideResult == true)
                {
                    for (var r = startRow; r <= endRow && r < rows.Count; r++)
                    {
                        var row = rows[r];
                        for (var c = startCol; c <= endCol && c < row.Count; c++)
                        {
                            var cell = row[c];
                            row[c] = cell with
                            {
                                Color = fontColor ?? cell.Color,
                                FillColor = fillColor ?? cell.FillColor
                            };
                        }
                    }
                    continue;
                }
                else if (rangeWideResult == false)
                {
                    continue; // Expression is false for entire range
                }

                for (var r = startRow; r <= endRow && r < rows.Count; r++)
                {
                    var row = rows[r];
                    for (var c = startCol; c <= endCol && c < row.Count; c++)
                    {
                        var cell = row[c];
                        if (string.IsNullOrEmpty(cell.Text)) continue;

                        var matches = EvaluateCondition(cell, type, op, formula, definedNames);

                        if (matches)
                        {
                            row[c] = cell with
                            {
                                Color = fontColor ?? cell.Color,
                                FillColor = fillColor ?? cell.FillColor
                            };

                        }
                    }
                }
            }
        }
    }

    private static bool EvaluateCondition(ExcelCell cell, string type, string op, string formula,
        Dictionary<string, double> definedNames)
    {
        if (type == "cellIs")
        {
            // Try numeric comparison first
            if (TryGetCellNumericValue(cell, out var cellVal) &&
                double.TryParse(formula, System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out var threshold))
            {
                return op switch
                {
                    "lessThan" => cellVal < threshold,
                    "greaterThan" => cellVal > threshold,
                    "equal" => Math.Abs(cellVal - threshold) < 1e-10,
                    "notEqual" => Math.Abs(cellVal - threshold) >= 1e-10,
                    "lessThanOrEqual" => cellVal <= threshold,
                    "greaterThanOrEqual" => cellVal >= threshold,
                    "between" => cellVal >= threshold, // simplified
                    _ => false
                };
            }

            // Fall back to string comparison (e.g. operator=equal, formula="✔")
            var trimmedFormula = formula.Trim('"');
            return op == "equal" && cell.Text == trimmedFormula;
        }

        if (type == "expression")
        {
            // Support simple patterns like "C8<Cash_Minimum" → current cell value < defined name
            // The formula references the first cell in the range, but semantically means "this cell"
            if (!TryGetCellNumericValue(cell, out var cellVal)) return false;

            // Try to match pattern: CELLREF < NAME or CELLREF > NAME etc.
            var m = System.Text.RegularExpressions.Regex.Match(formula, @"^[A-Z]+\d+\s*(<|>|<=|>=|=)\s*(.+)$",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            if (m.Success)
            {
                var compOp = m.Groups[1].Value;
                var rhs = m.Groups[2].Value.Trim();

                double rhsVal;
                if (!double.TryParse(rhs, System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out rhsVal))
                {
                    // Try defined name
                    if (!definedNames.TryGetValue(rhs, out rhsVal))
                        return false;
                }

                return compOp switch
                {
                    "<" => cellVal < rhsVal,
                    ">" => cellVal > rhsVal,
                    "<=" => cellVal <= rhsVal,
                    ">=" => cellVal >= rhsVal,
                    "=" => Math.Abs(cellVal - rhsVal) < 1e-10,
                    _ => false
                };
            }

            return false;
        }

        return false;
    }

    /// <summary>
    /// Evaluates a range-wide expression formula that doesn't depend on individual cell values.
    /// Supports patterns like "NAME+N=TODAY()" or "NAME=TODAY()".
    /// Returns true/false/null (null = can't evaluate).
    /// </summary>
    private static bool? EvaluateRangeExpression(string formula, Dictionary<string, double> definedNames)
    {
        // Pattern: NAME+N=TODAY() or NAME-N=TODAY() or NAME=TODAY()
        var m = System.Text.RegularExpressions.Regex.Match(formula,
            @"^(\w+)\s*([+\-]\s*\d+)?\s*=\s*TODAY\(\)\s*$",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        if (!m.Success) return null;

        var name = m.Groups[1].Value;
        if (!definedNames.TryGetValue(name, out var baseVal)) return null;

        double offset = 0;
        if (m.Groups[2].Success)
        {
            var offsetStr = m.Groups[2].Value.Replace(" ", "");
            if (!double.TryParse(offsetStr, System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out offset))
                return null;
        }

        var todaySerial = DateToExcelSerial(DateTime.Today);
        return Math.Abs(baseVal + offset - todaySerial) < 1e-10;
    }

    /// <summary>
    /// Converts a DateTime to an Excel serial date number (days since Jan 1, 1900, with the 1900 leap year bug).
    /// </summary>
    private static double DateToExcelSerial(DateTime date)
    {
        // Excel epoch: Jan 1, 1900 = serial 1.
        // Excel incorrectly treats 1900 as a leap year, so dates after Feb 28, 1900 are off by 1.
        var serial = (date - new DateTime(1899, 12, 30)).TotalDays;
        return serial;
    }

    /// <summary>
    /// Tries to extract a numeric value from a cell, stripping currency symbols and formatting.
    /// </summary>
    private static bool TryGetCellNumericValue(ExcelCell cell, out double value)
    {
        value = 0;
        if (string.IsNullOrWhiteSpace(cell.Text)) return false;

        // Strip common currency/accounting formatting: $, parentheses (negative), spaces
        var text = cell.Text.Replace("$", "").Replace(",", "").Trim();
        // Handle accounting negative: (1,234.56) → -1234.56
        if (text.StartsWith('(') && text.EndsWith(')'))
            text = "-" + text[1..^1];
        // Handle accounting zero: "- " or "-"
        if (text.Trim() == "-") { value = 0; return true; }

        return double.TryParse(text.Trim(), System.Globalization.NumberStyles.Any,
            System.Globalization.CultureInfo.InvariantCulture, out value);
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

    private static List<List<ExcelCell>> ReadSheet(ZipArchiveEntry entry, List<string> sharedStrings, Dictionary<int, int> boldPrefixLengths,
        List<FontStyleInfo> fontStyles, List<PdfColor?> fillColors, List<CellBorderInfo?> borders, Dictionary<int, string> numberFormats,
        List<int> cellXfFontIndices, List<int> cellXfFillIndices, List<int> cellXfNumFmtIds, List<string> cellXfAlignments, List<string> cellXfVerticalAlignments, List<int> cellXfBorderIndices, List<bool> cellXfWrapTexts, List<int> cellXfIndents)
    {
        var rows = new List<List<ExcelCell>>();

        using var stream = entry.Open();
        var doc = XDocument.Load(stream);
        var ns = doc.Root?.GetDefaultNamespace() ?? XNamespace.None;

        // Hyperlink cells often rely on workbook defaults for visual style.
        // Track refs so we can apply the expected blue + underline appearance.
        var hyperlinkRefs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var hyperlink in doc.Descendants(ns + "hyperlink"))
        {
            var href = hyperlink.Attribute("ref")?.Value;
            if (string.IsNullOrEmpty(href))
                continue;

            foreach (var part in href.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            {
                var rangePart = part.Split(':', StringSplitOptions.RemoveEmptyEntries)[0];
                var normalized = rangePart.Replace("$", "");
                if (!string.IsNullOrEmpty(normalized))
                    hyperlinkRefs.Add(normalized);
            }
        }

        var lastRowNumber = 0;

        // Build column-level default fill map (1-based column index → fillColor)
        // Cells without an explicit 's' attribute inherit the fill from their column's default style.
        var colDefaultFill = new Dictionary<int, PdfColor?>(); // 1-based column
        var colsEl = doc.Descendants(ns + "cols").FirstOrDefault();
        if (colsEl != null)
        {
            foreach (var col in colsEl.Elements(ns + "col"))
            {
                var colStyleAttr = col.Attribute("style")?.Value;
                if (!int.TryParse(colStyleAttr, out var colStyleIdx) || colStyleIdx <= 0) continue;
                var colFill = ResolveFillColor(colStyleIdx, fillColors, cellXfFillIndices);
                if (colFill == null) continue;
                if (!int.TryParse(col.Attribute("min")?.Value, out var minCol)) continue;
                if (!int.TryParse(col.Attribute("max")?.Value, out var maxCol)) continue;
                for (int ci = minCol; ci <= maxCol && ci <= 16384; ci++)
                    colDefaultFill[ci] = colFill;
            }
        }

        // Accumulate numeric cell values by reference for formula evaluation
        var cellNumericValues = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);

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

            // Row-level default fill: applied to cells without an explicit style.
            PdfColor? rowDefaultFill = null;
            var rowCustomFormat = row.Attribute("customFormat")?.Value;
            if (rowCustomFormat == "1")
            {
                var rowStyleAttr = row.Attribute("s")?.Value;
                if (int.TryParse(rowStyleAttr, out var rowStyleIdx) && rowStyleIdx > 0)
                    rowDefaultFill = ResolveFillColor(rowStyleIdx, fillColors, cellXfFillIndices);
            }

            var cells = new List<ExcelCell>();
            var lastColIndex = 0;

            foreach (var cell in row.Elements(ns + "c"))
            {
                // Parse column reference to handle gaps (e.g., A1, C1 means B is empty)
                var reference = cell.Attribute("r")?.Value ?? "";
                var colIndex = ParseColumnIndex(reference);

                // Fill empty cells for gaps, applying row/column default fills if applicable.
                while (lastColIndex < colIndex)
                {
                    PdfColor? gapFill = rowDefaultFill;
                    if (gapFill == null && colDefaultFill.TryGetValue(lastColIndex + 1, out var gapColFill))
                        gapFill = gapColFill;
                    cells.Add(new ExcelCell(string.Empty, null, gapFill));
                    lastColIndex++;
                }

                var type = cell.Attribute("t")?.Value;
                var value = cell.Element(ns + "v")?.Value ?? "";
                var formula = cell.Element(ns + "f")?.Value ?? "";

                // Recalculate simple volatile formulas so output matches reference renderers
                // that evaluate formulas at conversion time.
                if (!string.IsNullOrEmpty(formula))
                {
                    if (IsTodayFormula(formula))
                    {
                        value = DateTimeToExcelSerial(DateTime.Today)
                            .ToString(System.Globalization.CultureInfo.InvariantCulture);
                    }
                    else if (IsNowFormula(formula))
                    {
                        value = DateTimeToExcelSerial(DateTime.Now)
                            .ToString(System.Globalization.CultureInfo.InvariantCulture);
                    }
                    else if (TryEvaluateSimpleConcatFormula(formula, out var concatValue))
                    {
                        value = concatValue;
                    }
                    else if (TryEvaluateDatePartFormula(formula, out var datePartValue))
                    {
                        value = datePartValue;
                    }
                    else if (string.IsNullOrEmpty(value) &&
                             TryEvaluateArithmeticFormula(formula, cellNumericValues, out var arithmeticValue))
                    {
                        value = arithmeticValue.ToString(System.Globalization.CultureInfo.InvariantCulture);
                    }
                }

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
                bool underline = false;
                CellBorderInfo? border = null;
                bool wrapText = false;
                string? fontName = null;
                int cellIndent = 0;
                if (int.TryParse(styleAttr, out var styleIndex))
                {
                    var fontStyle = ResolveFontStyle(styleIndex, fontStyles, cellXfFontIndices);
                    color = fontStyle.Color;
                    fontSize = fontStyle.Size;
                    bold = fontStyle.Bold;
                    italic = fontStyle.Italic;
                    underline = fontStyle.Underline;
                    fontName = fontStyle.FontName;
                    fillColor = ResolveFillColor(styleIndex, fillColors, cellXfFillIndices);
                    border = ResolveBorder(styleIndex, borders, cellXfBorderIndices);
                    if (styleIndex >= 0 && styleIndex < cellXfNumFmtIds.Count)
                        numFmtId = cellXfNumFmtIds[styleIndex];
                    if (styleIndex >= 0 && styleIndex < cellXfAlignments.Count)
                        cellAlignment = cellXfAlignments[styleIndex];
                    if (styleIndex >= 0 && styleIndex < cellXfVerticalAlignments.Count)
                        cellVerticalAlignment = cellXfVerticalAlignments[styleIndex];
                    if (styleIndex >= 0 && styleIndex < cellXfWrapTexts.Count)
                        wrapText = cellXfWrapTexts[styleIndex];
                    if (styleIndex >= 0 && styleIndex < cellXfIndents.Count)
                        cellIndent = cellXfIndents[styleIndex];
                }
                else
                {
                    // Apply fill from row-level or column-level default style when cell has no explicit style.
                    // Priority: row default > column default.
                    if (rowDefaultFill != null)
                        fillColor = rowDefaultFill;
                    else if (colDefaultFill.TryGetValue(colIndex, out var colFill))
                        fillColor = colFill;
                }

                string text;
                string? acctPrefix = null;
                int cellBoldPrefixLen = 0;
                if (type == "s" && int.TryParse(value, out var idx) && idx < sharedStrings.Count)
                {
                    text = sharedStrings[idx];
                    if (boldPrefixLengths.TryGetValue(idx, out var bpl))
                        cellBoldPrefixLen = bpl;
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
                            text = FormatNumber(numVal, numFmtId, numberFormats, out var fmtColor, out acctPrefix);
                            // Number format color overrides font color (e.g., [Red] for negatives)
                            if (fmtColor != null)
                                color = fmtColor;
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

                // Apply common hyperlink visual style for linked cells.
                var normalizedRef = reference.Replace("$", "");
                if (!string.IsNullOrEmpty(normalizedRef) && hyperlinkRefs.Contains(normalizedRef))
                {
                    color = PdfColor.FromRgb(5, 99, 193);
                    underline = true;
                }

                // Store numeric value for formula evaluation by later cells
                if (!string.IsNullOrEmpty(reference) && !string.IsNullOrEmpty(value) &&
                    double.TryParse(value, System.Globalization.NumberStyles.Any,
                        System.Globalization.CultureInfo.InvariantCulture, out var storedVal))
                {
                    cellNumericValues[reference.Replace("$", "")] = storedVal;
                }

                cells.Add(new ExcelCell(text, color, fillColor, cellAlignment, fontSize, bold, italic, underline, border, cellVerticalAlignment, wrapText, acctPrefix, fontName, cellIndent, cellBoldPrefixLen));
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

    private static bool IsTodayFormula(string formula)
    {
        var normalized = NormalizeFormula(formula);
        return string.Equals(normalized, "today()", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsNowFormula(string formula)
    {
        var normalized = NormalizeFormula(formula);
        return string.Equals(normalized, "now()", StringComparison.OrdinalIgnoreCase);
    }

    private static string NormalizeFormula(string formula)
    {
        if (string.IsNullOrWhiteSpace(formula))
            return string.Empty;

        var normalized = formula.Trim();
        if (normalized.StartsWith("=", StringComparison.Ordinal))
            normalized = normalized[1..];

        if (normalized.StartsWith("@", StringComparison.Ordinal))
            normalized = normalized[1..];

        if (normalized.StartsWith("_xlfn.", StringComparison.OrdinalIgnoreCase))
            normalized = normalized[6..];

        // Ignore spaces often inserted by some generators (e.g., "TODAY( )").
        return normalized.Replace(" ", string.Empty);
    }

    /// <summary>
    /// Tries to evaluate simple date-part formulas that wrap TODAY() or NOW(),
    /// such as YEAR(TODAY()), MONTH(NOW()), DAY(TODAY()).
    /// </summary>
    private static bool TryEvaluateDatePartFormula(string formula, out string value)
    {
        value = string.Empty;
        var normalized = NormalizeFormula(formula);

        // Check for YEAR(TODAY()), YEAR(NOW()), MONTH(...), DAY(...)
        bool isToday, isNow;
        DateTime dt;
        int result;

        if (IsWrappedVolatile(normalized, "year(", out isToday, out isNow))
        {
            dt = isToday ? DateTime.Today : DateTime.Now;
            result = dt.Year;
        }
        else if (IsWrappedVolatile(normalized, "month(", out isToday, out isNow))
        {
            dt = isToday ? DateTime.Today : DateTime.Now;
            result = dt.Month;
        }
        else if (IsWrappedVolatile(normalized, "day(", out isToday, out isNow))
        {
            dt = isToday ? DateTime.Today : DateTime.Now;
            result = dt.Day;
        }
        else
        {
            return false;
        }

        value = result.ToString(System.Globalization.CultureInfo.InvariantCulture);
        return true;
    }

    /// <summary>
    /// Evaluates simple arithmetic formulas (SUM, addition of cell references)
    /// when the cached value is missing. Supports:
    ///   =SUM(A1:A5)         — range SUM
    ///   =SUM(A1:A5,B1:B5)   — multi-range SUM
    ///   =A1+B1+C1           — addition of cell references
    /// </summary>
    private static bool TryEvaluateArithmeticFormula(string formula, Dictionary<string, double> cellValues, out double result)
    {
        result = 0;
        if (string.IsNullOrWhiteSpace(formula))
            return false;

        var expr = formula.Trim();
        if (expr.StartsWith("=", StringComparison.Ordinal))
            expr = expr[1..];

        // SUM(range, ...) pattern
        if (expr.StartsWith("SUM(", StringComparison.OrdinalIgnoreCase) && expr.EndsWith(")"))
        {
            var inner = expr[4..^1].Trim();
            if (string.IsNullOrEmpty(inner))
                return false;

            double total = 0;
            foreach (var part in inner.Split(','))
            {
                var p = part.Trim();
                if (p.Contains(':'))
                {
                    if (!TrySumRange(p, cellValues, out var rangeSum))
                        return false;
                    total += rangeSum;
                }
                else
                {
                    var key = p.Replace("$", "");
                    if (cellValues.TryGetValue(key, out var v))
                        total += v;
                    // Cell not found — treat as 0 (empty cell)
                }
            }
            result = total;
            return true;
        }

        // Simple addition: A1+B1+C1 (cell references separated by +)
        if (System.Text.RegularExpressions.Regex.IsMatch(expr, @"^[A-Za-z$]+[0-9$]+(\+[A-Za-z$]+[0-9$]+)+$"))
        {
            double total = 0;
            foreach (var refPart in expr.Split('+'))
            {
                var key = refPart.Trim().Replace("$", "");
                if (cellValues.TryGetValue(key, out var v))
                    total += v;
            }
            result = total;
            return true;
        }

        return false;
    }

    private static bool TrySumRange(string range, Dictionary<string, double> cellValues, out double sum)
    {
        sum = 0;
        var parts = range.Split(':');
        if (parts.Length != 2) return false;

        var startRef = parts[0].Trim().Replace("$", "");
        var endRef = parts[1].Trim().Replace("$", "");

        var startCol = ParseColumnLetters(startRef);
        var startRow = ParseRowNumber(startRef);
        var endCol = ParseColumnLetters(endRef);
        var endRow = ParseRowNumber(endRef);

        if (startCol == 0 || startRow == 0 || endCol == 0 || endRow == 0)
            return false;

        for (int r = startRow; r <= endRow; r++)
        {
            for (int c = startCol; c <= endCol; c++)
            {
                var key = ColumnNumberToLetters(c) + r.ToString();
                if (cellValues.TryGetValue(key, out var v))
                    sum += v;
            }
        }
        return true;
    }

    private static int ParseColumnLetters(string cellRef)
    {
        int col = 0;
        foreach (var c in cellRef)
        {
            if (char.IsLetter(c))
                col = col * 26 + (char.ToUpper(c) - 'A' + 1);
            else
                break;
        }
        return col;
    }

    private static int ParseRowNumber(string cellRef)
    {
        var numPart = new string(cellRef.Where(char.IsDigit).ToArray());
        return int.TryParse(numPart, out var row) ? row : 0;
    }

    private static string ColumnNumberToLetters(int col)
    {
        var sb = new System.Text.StringBuilder();
        while (col > 0)
        {
            col--;
            sb.Insert(0, (char)('A' + col % 26));
            col /= 26;
        }
        return sb.ToString();
    }

    /// <summary>
    /// Tries to evaluate simple concatenation formulas where each token is either
    /// a quoted literal or a supported volatile date expression.
    /// </summary>
    private static bool TryEvaluateSimpleConcatFormula(string formula, out string value)
    {
        value = string.Empty;
        if (string.IsNullOrWhiteSpace(formula) || formula.IndexOf('&') < 0)
            return false;

        var expr = formula.Trim();
        if (expr.StartsWith("=", StringComparison.Ordinal))
            expr = expr[1..];

        var tokens = SplitConcatTokens(expr);
        if (tokens.Count == 0)
            return false;

        var sb = new System.Text.StringBuilder();
        foreach (var rawToken in tokens)
        {
            var token = rawToken.Trim();
            if (token.Length == 0)
                continue;

            if (TryParseQuotedLiteral(token, out var literal))
            {
                sb.Append(literal);
                continue;
            }

            if (TryEvaluateDateToken(token, out var evaluated))
            {
                sb.Append(evaluated);
                continue;
            }

            return false;
        }

        value = sb.ToString();
        return true;
    }

    private static List<string> SplitConcatTokens(string expr)
    {
        var tokens = new List<string>();
        if (string.IsNullOrEmpty(expr))
            return tokens;

        var sb = new System.Text.StringBuilder();
        var inQuotes = false;
        for (var i = 0; i < expr.Length; i++)
        {
            var ch = expr[i];
            if (ch == '"')
            {
                sb.Append(ch);
                if (inQuotes && i + 1 < expr.Length && expr[i + 1] == '"')
                {
                    // Escaped quote "" inside literal.
                    sb.Append(expr[i + 1]);
                    i++;
                }
                else
                {
                    inQuotes = !inQuotes;
                }
                continue;
            }

            if (!inQuotes && ch == '&')
            {
                tokens.Add(sb.ToString());
                sb.Clear();
                continue;
            }

            sb.Append(ch);
        }

        if (sb.Length > 0)
            tokens.Add(sb.ToString());

        return tokens;
    }

    private static bool TryParseQuotedLiteral(string token, out string literal)
    {
        literal = string.Empty;
        if (token.Length < 2 || token[0] != '"' || token[^1] != '"')
            return false;

        literal = token[1..^1].Replace("\"\"", "\"");
        return true;
    }

    private static bool TryEvaluateDateToken(string token, out string value)
    {
        value = string.Empty;
        var normalized = NormalizeFormula(token);
        var ci = System.Globalization.CultureInfo.InvariantCulture;

        if (string.Equals(normalized, "today()", StringComparison.OrdinalIgnoreCase))
        {
            value = DateTimeToExcelSerial(DateTime.Today).ToString(ci);
            return true;
        }
        if (string.Equals(normalized, "now()", StringComparison.OrdinalIgnoreCase))
        {
            value = DateTimeToExcelSerial(DateTime.Now).ToString(ci);
            return true;
        }

        bool isToday, isNow;
        DateTime dt;
        if (IsWrappedVolatile(normalized, "year(", out isToday, out isNow))
        {
            dt = isToday ? DateTime.Today : DateTime.Now;
            value = dt.Year.ToString(ci);
            return true;
        }
        if (IsWrappedVolatile(normalized, "month(", out isToday, out isNow))
        {
            dt = isToday ? DateTime.Today : DateTime.Now;
            value = dt.Month.ToString(ci);
            return true;
        }
        if (IsWrappedVolatile(normalized, "day(", out isToday, out isNow))
        {
            dt = isToday ? DateTime.Today : DateTime.Now;
            value = dt.Day.ToString(ci);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Checks whether the normalized formula is outerFunc + TODAY()/NOW() + closing paren.
    /// e.g. "year(today())" or "year(now())".
    /// </summary>
    private static bool IsWrappedVolatile(string normalized, string outerPrefix,
        out bool isToday, out bool isNow)
    {
        isToday = false;
        isNow = false;
        if (!normalized.StartsWith(outerPrefix, StringComparison.OrdinalIgnoreCase))
            return false;

        var inner = normalized[outerPrefix.Length..];
        if (inner.Equals("today())", StringComparison.OrdinalIgnoreCase))
        {
            isToday = true;
            return true;
        }
        if (inner.Equals("now())", StringComparison.OrdinalIgnoreCase))
        {
            isNow = true;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Formats a numeric value according to its Excel number format.
    /// Handles built-in formats and common custom patterns.
    /// </summary>
    private static string FormatNumber(double value, int numFmtId, Dictionary<int, string> customFormats, out PdfColor? formatColor, out string? accountingPrefix)
    {
        formatColor = null;
        accountingPrefix = null;
        var ci = System.Globalization.CultureInfo.InvariantCulture;

        // Check custom format first
        if (numFmtId > 0 && customFormats.TryGetValue(numFmtId, out var formatCode))
        {
            return ApplyNumberFormat(value, formatCode, out formatColor, out accountingPrefix);
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
            14 => FormatExcelDate(value, "M/d/yyyy"),
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
    private static string ApplyNumberFormat(double value, string formatCode, out PdfColor? formatColor, out string? accountingPrefix)
    {
        formatColor = null;
        accountingPrefix = null;
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

        // Empty section means "suppress output" (e.g., format ";;" hides all values)
        if (string.IsNullOrWhiteSpace(activeFormat))
            return "";

        // Extract color codes like [Red], [Blue], etc. before stripping
        var colorMatch = System.Text.RegularExpressions.Regex.Match(activeFormat,
            @"\[(Red|Blue|Green|Yellow|Magenta|Cyan|White|Black|Color\d+)\]",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        if (colorMatch.Success)
        {
            formatColor = colorMatch.Groups[1].Value.ToLowerInvariant() switch
            {
                "red" => PdfColor.FromRgb(255, 0, 0),
                "blue" => PdfColor.FromRgb(0, 0, 255),
                "green" => PdfColor.FromRgb(0, 128, 0),
                "yellow" => PdfColor.FromRgb(255, 255, 0),
                "magenta" => PdfColor.FromRgb(255, 0, 255),
                "cyan" => PdfColor.FromRgb(0, 255, 255),
                "white" => PdfColor.FromRgb(255, 255, 255),
                "black" => PdfColor.FromRgb(0, 0, 0),
                _ => null
            };
        }

        // Strip color codes
        activeFormat = System.Text.RegularExpressions.Regex.Replace(activeFormat, @"\[(?:Red|Blue|Green|Yellow|Magenta|Cyan|White|Black|Color\d+)\]", "", System.Text.RegularExpressions.RegexOptions.IgnoreCase);

        // Strip locale/currency codes like [$€-407], [$¥-411], [$-409]
        activeFormat = System.Text.RegularExpressions.Regex.Replace(activeFormat, @"\[\$([^-\]]*)-[^\]]+\]", "$1");
        // Also handle [$symbol] without locale
        activeFormat = System.Text.RegularExpressions.Regex.Replace(activeFormat, @"\[\$([^\]]*)\]", "$1");

        // Single-pass parse to handle _X, *X, \X, "text", and ? correctly
        // This avoids regex interference between quoted text and escape sequences.
        var parsedFormat = new System.Text.StringBuilder();
        var literals = new System.Text.StringBuilder(); // accumulated literal text outside number pattern
        for (var pi = 0; pi < activeFormat.Length; pi++)
        {
            var ch = activeFormat[pi];
            if (ch == '_' && pi + 1 < activeFormat.Length)
            {
                parsedFormat.Append(' '); // spacing placeholder
                pi++; // skip the width character
            }
            else if (ch == '*' && pi + 1 < activeFormat.Length)
            {
                // Repeat-fill character: split into accounting prefix (left-aligned)
                // and number part (right-aligned) for proper accounting format rendering.
                if (accountingPrefix == null)
                {
                    accountingPrefix = parsedFormat.ToString();
                    parsedFormat.Clear();
                }
                pi++; // skip fill character
            }
            else if (ch == '\\' && pi + 1 < activeFormat.Length)
            {
                parsedFormat.Append(activeFormat[pi + 1]); // literal escaped char
                pi++;
            }
            else if (ch == '"')
            {
                // Quoted literal text
                pi++;
                while (pi < activeFormat.Length && activeFormat[pi] != '"')
                {
                    parsedFormat.Append(activeFormat[pi]);
                    pi++;
                }
            }
            else
            {
                parsedFormat.Append(ch);
            }
        }
        activeFormat = parsedFormat.ToString();

        // Handle date/time-like formats early (before numeric placeholder check,
        // since date formats like d-mmm-yy have no 0/# placeholders)
        var lowerFmt = activeFormat.ToLowerInvariant();
        if (lowerFmt.Contains("yy") || lowerFmt.Contains("dd") ||
            (lowerFmt.Contains("mm") && (lowerFmt.Contains("dd") || lowerFmt.Contains("yy") || lowerFmt.Contains("hh") || lowerFmt.Contains("ss"))) ||
            lowerFmt.Contains("mmm") ||
            lowerFmt.Contains("hh") || lowerFmt.Contains("h:") || lowerFmt.Contains("am/pm") || lowerFmt.Contains("a/p"))
        {
            return FormatExcelDate(value, activeFormat);
        }

        // Check if format has any numeric placeholders (0, #, .)
        // If not, it's a pure literal format (e.g., accounting zero section like: $ -  )
        var hasNumericPlaceholder = false;
        foreach (var ch in activeFormat)
        {
            if (ch == '0' || ch == '#' || ch == '.')
            {
                hasNumericPlaceholder = true;
                break;
            }
        }
        if (!hasNumericPlaceholder)
        {
            // Replace ? with space (digit placeholder shows space for absent digits)
            var literal = activeFormat.Replace('?', ' ');
            // When accounting prefix is set, preserve trailing spaces for right-indent alignment
            return accountingPrefix != null ? literal : literal.Trim();
        }

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

        // Count decimal places from format
        var hasDecimal = activeFormat.Contains('.');
        var decimalPlaces = 0;
        if (hasDecimal)
        {
            var dotIdx = activeFormat.IndexOf('.');
            for (var i = dotIdx + 1; i < activeFormat.Length; i++)
            {
                if (activeFormat[i] == '0' || activeFormat[i] == '#' || activeFormat[i] == '?')
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

        // Pre-round through 15-significant-digit decimal to fix floating-point
        // midpoint rounding (e.g. 127.25*0.06 = 7.6349999999999998 → 7.635 → 7.64).
        // This matches Excel/LibreOffice rounding behavior.
        if (hasDecimal && decimalPlaces >= 0)
        {
            var decValue = decimal.Parse(value.ToString("G15", ci),
                System.Globalization.NumberStyles.Float, ci);
            decValue = Math.Round(decValue, decimalPlaces, MidpointRounding.AwayFromZero);
            value = (double)decValue;
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
    /// Converts a DateTime to Excel's serial date value (1900 date system with leap-year bug).
    /// </summary>
    private static double DateTimeToExcelSerial(DateTime dt)
    {
        var date = dt.Date;
        var days = (date - new DateTime(1900, 1, 1)).TotalDays + 1;
        if (days >= 60)
            days += 1; // Excel's fictitious 1900-02-29

        var frac = (dt - date).TotalDays;
        return days + frac;
    }

    /// <summary>
    /// Checks whether the given format code is a date/time format.
    /// </summary>
    private static bool IsDateFormat(string formatCode)
    {
        var lower = formatCode.ToLowerInvariant();
        return lower.Contains("yy") || lower.Contains("dd") || lower.Contains("mmm") ||
               (lower.Contains("mm") && (lower.Contains("dd") || lower.Contains("yy") || lower.Contains("hh") || lower.Contains("ss"))) ||
               lower.Contains("hh") || lower.Contains("h:") || lower.Contains("am/pm");
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
                    sb.Append(count switch { 1 => "M", 2 => "MM", 3 => "MMM", _ => "MMMM" });
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
        // or explicitly hidden (hidden="1").
        foreach (var col in doc.Descendants(ns + "col"))
        {
            var isHidden = col.Attribute("hidden")?.Value == "1";

            // Skip default-width columns (not customised by the spreadsheet author)
            var customWidth = col.Attribute("customWidth")?.Value;
            if (customWidth != "1" && !isHidden) continue;

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
                widths[c - 1] = isHidden ? 0f : colWidth; // hidden columns get 0 width
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
        var paperSize = 0; // 0 = not specified (will inherit from first sheet or default to US Letter)
        float marginLeft = -1, marginRight = -1, marginTop = -1, marginBottom = -1, footerMargin = -1;
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

        // Read fitToWidth / fitToHeight from pageSetup.
        // ECMA-376 defaults both to 1, but in practice when these attributes are
        // absent the stored 'scale' already encodes the horizontal fit and rows
        // paginate naturally.  Use 0 (unlimited) as default so fitToHeight
        // compression only triggers when the attribute is explicitly present.
        var fitToWidth = 1;
        var fitToHeight = 0;
        if (pageSetup != null)
        {
            var ftwAttr = pageSetup.Attribute("fitToWidth")?.Value;
            if (int.TryParse(ftwAttr, out var ftw) && ftw >= 0)
                fitToWidth = ftw;
            var fthAttr = pageSetup.Attribute("fitToHeight")?.Value;
            if (int.TryParse(fthAttr, out var fth) && fth >= 0)
                fitToHeight = fth;
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
            footerMargin = ParseInchesAttr(pageMargins, "footer");
        }

        // Read fitToPage from sheetPr/pageSetUpPr
        var pageSetUpPr = doc.Descendants(ns + "pageSetUpPr").FirstOrDefault();
        if (pageSetUpPr != null)
        {
            fitToPage = pageSetUpPr.Attribute("fitToPage")?.Value == "1";
        }

        // Per ECMA-376 §18.3.1.65, fitToHeight defaults to 1 when not specified.
        // Apply this default when fitToPage is enabled and the attribute is absent.
        if (fitToPage && fitToHeight == 0 && pageSetup?.Attribute("fitToHeight") == null)
            fitToHeight = 1;

        // Infer fitToPage when fitToHeight is explicitly set in <pageSetup> but
        // <pageSetUpPr fitToPage="1"/> is absent.  Many real-world files omit the
        // sheetPr flag while still relying on fitToHeight/fitToWidth; LibreOffice
        // and Excel both honour the pageSetup attributes in this scenario.
        if (!fitToPage && fitToHeight > 0)
            fitToPage = true;

        // Read printOptions (horizontalCentered)
        var horizontalCentered = false;
        var printOptions = doc.Descendants(ns + "printOptions").FirstOrDefault();
        if (printOptions != null)
        {
            horizontalCentered = printOptions.Attribute("horizontalCentered")?.Value == "1";
        }

        // Read headerFooter — oddFooter / oddHeader
        string? oddFooter = null;
        string? oddHeader = null;
        var headerFooter = doc.Descendants(ns + "headerFooter").FirstOrDefault();
        if (headerFooter != null)
        {
            oddFooter = headerFooter.Element(ns + "oddFooter")?.Value;
            oddHeader = headerFooter.Element(ns + "oddHeader")?.Value;
        }

        return new PageSetupInfo(isLandscape, scale, paperSize, marginLeft, marginRight, marginTop, marginBottom, fitToPage, fitToWidth, fitToHeight, horizontalCentered, oddFooter, oddHeader, footerMargin);
    }

    internal record PageSetupInfo(
        bool IsLandscape, int Scale, int PaperSize,
        float MarginLeftPt, float MarginRightPt, float MarginTopPt, float MarginBottomPt,
        bool FitToPage, int FitToWidth, int FitToHeight, bool HorizontalCentered = false,
        string? OddFooter = null, string? OddHeader = null, float FooterMarginPt = -1);

    /// <summary>
    /// Reads row heights from the sheet XML.
    /// Returns a dictionary of 0-based row index → height in points, plus the default row height.
    /// </summary>
    private static (Dictionary<int, float> heights, float defaultHeight, HashSet<int> customHeightRows) ReadRowHeights(ZipArchiveEntry entry)
    {
        var heights = new Dictionary<int, float>();
        var customHeightRows = new HashSet<int>();
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

        // Read explicit row heights (hidden rows get height 0)
        foreach (var row in doc.Descendants(ns + "row"))
        {
            var rAttr = row.Attribute("r")?.Value;
            if (rAttr == null) continue;
            if (!int.TryParse(rAttr, out var rowNum)) continue;

            var isHidden = row.Attribute("hidden")?.Value == "1";
            if (isHidden)
            {
                heights[rowNum - 1] = 0f;
                continue;
            }

            var htAttr = row.Attribute("ht")?.Value;
            if (htAttr == null) continue;
            if (!float.TryParse(htAttr,
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture,
                out var ht)) continue;

            heights[rowNum - 1] = ht; // store as 0-based
            if (row.Attribute("customHeight")?.Value == "1")
                customHeightRows.Add(rowNum - 1);
        }

        return (heights, defaultHeight, customHeightRows);
    }

    private static HashSet<int> ReadRowBreaks(ZipArchiveEntry entry)
    {
        var breaks = new HashSet<int>();
        using var stream = entry.Open();
        var doc = XDocument.Load(stream);
        var ns = doc.Root?.GetDefaultNamespace() ?? XNamespace.None;

        foreach (var brk in doc.Descendants(ns + "rowBreaks").SelectMany(rb => rb.Elements(ns + "brk")))
        {
            var idAttr = brk.Attribute("id")?.Value;
            // ECMA-376 §18.3.1.2: id is the zero-based row index of the break.
            // Break occurs BEFORE this row (this row starts a new page).
            if (idAttr != null && int.TryParse(idAttr, out var rowId) && rowId > 0)
                breaks.Add(rowId);
        }

        return breaks;
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

    /// <summary>Parses a cell reference like "C5" into (row=4, col=2) 0-based.
    /// For column-only references like "C", returns (row=-1, col=2).</summary>
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
        if (i < cellRef.Length && int.TryParse(cellRef[i..], out var row))
            return (row - 1, col); // 0-based
        return (-1, col); // Column-only: valid col, row=-1
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
            // Handle group shapes: iterate each pic inside grpSp individually
            var grpSp = anchor.Element(xdr + "grpSp");
            if (grpSp != null)
            {
                var grpXfrm = grpSp.Element(xdr + "grpSpPr")?.Element(a + "xfrm");
                long grpExtCx = 0, grpExtCy = 0, chOffX = 0, chOffY = 0, chExtCx = 0, chExtCy = 0;
                if (grpXfrm != null)
                {
                    var grpExt = grpXfrm.Element(a + "ext");
                    long.TryParse(grpExt?.Attribute("cx")?.Value, out grpExtCx);
                    long.TryParse(grpExt?.Attribute("cy")?.Value, out grpExtCy);
                    var chOff = grpXfrm.Element(a + "chOff");
                    long.TryParse(chOff?.Attribute("x")?.Value, out chOffX);
                    long.TryParse(chOff?.Attribute("y")?.Value, out chOffY);
                    var chExt = grpXfrm.Element(a + "chExt");
                    long.TryParse(chExt?.Attribute("cx")?.Value, out chExtCx);
                    long.TryParse(chExt?.Attribute("cy")?.Value, out chExtCy);
                }

                // Read anchor from/to for AnchorRow/Col
                var gFromEl = anchor.Element(xdr + "from");
                var gToEl = anchor.Element(xdr + "to");
                int gFromRow = 0, gFromCol = 0, gToRow = 1, gToCol = 1;
                long gFromColOff = 0, gFromRowOff = 0;
                if (gFromEl != null)
                {
                    int.TryParse(gFromEl.Element(xdr + "row")?.Value, out gFromRow);
                    int.TryParse(gFromEl.Element(xdr + "col")?.Value, out gFromCol);
                    long.TryParse(gFromEl.Element(xdr + "colOff")?.Value, out gFromColOff);
                    long.TryParse(gFromEl.Element(xdr + "rowOff")?.Value, out gFromRowOff);
                }
                if (gToEl != null)
                {
                    int.TryParse(gToEl.Element(xdr + "row")?.Value, out gToRow);
                    int.TryParse(gToEl.Element(xdr + "col")?.Value, out gToCol);
                }

                foreach (var pic in grpSp.Elements(xdr + "pic"))
                {
                    var picBlip = pic.Descendants(a + "blip").FirstOrDefault();
                    var picRid = picBlip?.Attribute(r + "embed")?.Value;
                    if (string.IsNullOrEmpty(picRid)) continue;

                    if (!rIdToMedia.TryGetValue(picRid, out var gMediaPath)) continue;
                    var gMediaEntry = archive.GetEntry(gMediaPath);
                    if (gMediaEntry == null) continue;

                    byte[] gImgData;
                    using (var ms = new System.IO.MemoryStream())
                    {
                        using var imS = gMediaEntry.Open();
                        imS.CopyTo(ms);
                        gImgData = ms.ToArray();
                    }

                    var gExt = System.IO.Path.GetExtension(gMediaPath).TrimStart('.').ToLowerInvariant();
                    if (gExt == "jpeg") gExt = "jpg";

                    // Read pic's own xfrm in child coordinate space
                    var picXfrm = pic.Descendants(a + "xfrm").FirstOrDefault();
                    long picOffX = 0, picOffY = 0, picCx = 0, picCy = 0;
                    if (picXfrm != null)
                    {
                        long.TryParse(picXfrm.Element(a + "off")?.Attribute("x")?.Value, out picOffX);
                        long.TryParse(picXfrm.Element(a + "off")?.Attribute("y")?.Value, out picOffY);
                        long.TryParse(picXfrm.Element(a + "ext")?.Attribute("cx")?.Value, out picCx);
                        long.TryParse(picXfrm.Element(a + "ext")?.Attribute("cy")?.Value, out picCy);
                    }

                    // Transform from child coords to sheet-relative EMU offset from anchor
                    long offsetXEmu = 0, offsetYEmu = 0, wEmu = 0, hEmu = 0;
                    if (chExtCx > 0 && chExtCy > 0)
                    {
                        offsetXEmu = (picOffX - chOffX) * grpExtCx / chExtCx;
                        offsetYEmu = (picOffY - chOffY) * grpExtCy / chExtCy;
                        wEmu = picCx * grpExtCx / chExtCx;
                        hEmu = picCy * grpExtCy / chExtCy;
                    }
                    else
                    {
                        wEmu = picCx;
                        hEmu = picCy;
                    }

                    images.Add(new ExcelEmbeddedImage(
                        AnchorRow: gFromRow,
                        AnchorCol: gFromCol,
                        SpanRows: Math.Max(1, gToRow - gFromRow),
                        SpanCols: Math.Max(1, gToCol - gFromCol),
                        Data: gImgData,
                        Extension: gExt,
                        WidthEmu: wEmu,
                        HeightEmu: hEmu,
                        FromColOffEmu: gFromColOff,
                        FromRowOffEmu: gFromRowOff,
                        OffsetXEmu: offsetXEmu,
                        OffsetYEmu: offsetYEmu
                    ));
                }
                continue;
            }

            var fromEl = anchor.Element(xdr + "from");
            var toEl = anchor.Element(xdr + "to");
            var extEl = anchor.Element(xdr + "ext");

            int fromRow = 0, fromCol = 0, toRow = 1, toCol = 1;
            long fromColOff = 0, fromRowOff = 0, toColOff = 0, toRowOff = 0;
            if (fromEl != null)
            {
                int.TryParse(fromEl.Element(xdr + "row")?.Value, out fromRow);
                int.TryParse(fromEl.Element(xdr + "col")?.Value, out fromCol);
                long.TryParse(fromEl.Element(xdr + "colOff")?.Value, out fromColOff);
                long.TryParse(fromEl.Element(xdr + "rowOff")?.Value, out fromRowOff);
            }
            if (toEl != null)
            {
                int.TryParse(toEl.Element(xdr + "row")?.Value, out toRow);
                int.TryParse(toEl.Element(xdr + "col")?.Value, out toCol);
                long.TryParse(toEl.Element(xdr + "colOff")?.Value, out toColOff);
                long.TryParse(toEl.Element(xdr + "rowOff")?.Value, out toRowOff);
            }

            // For oneCellAnchor / absoluteAnchor, read EMU size from <ext cx cy>.
            long widthEmu = 0, heightEmu = 0;
            if (extEl != null)
            {
                long.TryParse(extEl.Attribute("cx")?.Value, out widthEmu);
                long.TryParse(extEl.Attribute("cy")?.Value, out heightEmu);
            }
            // Some twoCellAnchor images (e.g. template logos) omit xdr:ext and only
            // store size under a:xfrm/a:ext on the picture shape.
            if (widthEmu <= 0 || heightEmu <= 0)
            {
                var xfrmExtEl = anchor.Descendants(a + "xfrm")
                    .Elements(a + "ext")
                    .FirstOrDefault();
                if (xfrmExtEl != null)
                {
                    if (widthEmu <= 0)
                        long.TryParse(xfrmExtEl.Attribute("cx")?.Value, out widthEmu);
                    if (heightEmu <= 0)
                        long.TryParse(xfrmExtEl.Attribute("cy")?.Value, out heightEmu);
                }
            }

            // Find image reference IDs and prefer raster (png/jpg/jpeg) targets.
            var embedIds = anchor.Descendants(a + "blip")
                .Select(b => b.Attribute(r + "embed")?.Value)
                .Where(id => !string.IsNullOrEmpty(id))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Cast<string>()
                .ToList();
            if (embedIds.Count == 0) continue;

            static bool IsRasterImagePath(string path)
            {
                var e = System.IO.Path.GetExtension(path).TrimStart('.').ToLowerInvariant();
                return e is "png" or "jpg" or "jpeg";
            }

            var chosenRid = embedIds.FirstOrDefault(id => rIdToMedia.TryGetValue(id, out var p) && IsRasterImagePath(p))
                ?? embedIds.First();
            if (!rIdToMedia.TryGetValue(chosenRid, out var mediaPath)) continue;

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
                HeightEmu: heightEmu,
                FromColOffEmu: fromColOff,
                FromRowOffEmu: fromRowOff,
                ToColOffEmu: toColOff,
                ToRowOffEmu: toRowOff
            ));
        }

        return images;
    }

    /// <summary>
    /// Reads decorative shape elements (rectangles, rounded rectangles) from a worksheet's drawing.
    /// </summary>
    private static List<ExcelDrawingShape> ReadSheetShapes(ZipArchive archive, int sheetId, List<PdfColor> themeColors)
    {
        var shapes = new List<ExcelDrawingShape>();
        var relsEntry = archive.GetEntry($"xl/worksheets/_rels/sheet{sheetId}.xml.rels");
        if (relsEntry == null) return shapes;

        string drawingPath;
        using (var relsStream = relsEntry.Open())
        {
            var relsDoc = XDocument.Load(relsStream);
            var drawingRel = relsDoc.Descendants().FirstOrDefault(e =>
                e.Attribute("Type")?.Value?.EndsWith("/drawing") == true);
            if (drawingRel == null) return shapes;
            var target = drawingRel.Attribute("Target")?.Value;
            if (string.IsNullOrEmpty(target)) return shapes;
            drawingPath = target.StartsWith('/') ? target.TrimStart('/') : "xl/" + target.TrimStart('.', '/');
        }

        var drawingEntry = archive.GetEntry(drawingPath);
        if (drawingEntry == null) return shapes;

        using var dStream = drawingEntry.Open();
        var dDoc = XDocument.Load(dStream);
        var xdr = XNamespace.Get("http://schemas.openxmlformats.org/drawingml/2006/spreadsheetDrawing");
        var a = XNamespace.Get("http://schemas.openxmlformats.org/drawingml/2006/main");
        // Map scheme color names to theme indices (must match ReadThemeColors order: 0=lt1, 1=dk1, 2=lt2, 3=dk2, 4-9=accent1-6)
        static int SchemeColorToThemeIndex(string val) => val switch
        {
            "lt1" or "bg1" => 0, "dk1" or "tx1" => 1,
            "lt2" or "bg2" => 2, "dk2" or "tx2" => 3,
            "accent1" => 4, "accent2" => 5, "accent3" => 6, "accent4" => 7,
            "accent5" => 8, "accent6" => 9, "hlink" => 10, "folHlink" => 11,
            _ => -1
        };

        foreach (var anchor in dDoc.Descendants(xdr + "twoCellAnchor"))
        {
            // Handle group shapes: extract custGeom sp elements as polygons
            var grpSp = anchor.Element(xdr + "grpSp");
            if (grpSp != null)
            {
                var grpXfrm = grpSp.Element(xdr + "grpSpPr")?.Element(a + "xfrm");
                long grpExtCx = 0, grpExtCy = 0, chOffX = 0, chOffY = 0, chExtCx = 0, chExtCy = 0;
                if (grpXfrm != null)
                {
                    var grpExt = grpXfrm.Element(a + "ext");
                    long.TryParse(grpExt?.Attribute("cx")?.Value, out grpExtCx);
                    long.TryParse(grpExt?.Attribute("cy")?.Value, out grpExtCy);
                    var chOff = grpXfrm.Element(a + "chOff");
                    long.TryParse(chOff?.Attribute("x")?.Value, out chOffX);
                    long.TryParse(chOff?.Attribute("y")?.Value, out chOffY);
                    var chExt = grpXfrm.Element(a + "chExt");
                    long.TryParse(chExt?.Attribute("cx")?.Value, out chExtCx);
                    long.TryParse(chExt?.Attribute("cy")?.Value, out chExtCy);
                }

                var gFromEl = anchor.Element(xdr + "from");
                var gToEl = anchor.Element(xdr + "to");
                int gFromRow = 0, gFromCol = 0, gToRow = 1, gToCol = 1;
                long gFromColOff = 0, gFromRowOff = 0;
                if (gFromEl != null)
                {
                    int.TryParse(gFromEl.Element(xdr + "row")?.Value, out gFromRow);
                    int.TryParse(gFromEl.Element(xdr + "col")?.Value, out gFromCol);
                    long.TryParse(gFromEl.Element(xdr + "colOff")?.Value, out gFromColOff);
                    long.TryParse(gFromEl.Element(xdr + "rowOff")?.Value, out gFromRowOff);
                }
                if (gToEl != null)
                {
                    int.TryParse(gToEl.Element(xdr + "row")?.Value, out gToRow);
                    int.TryParse(gToEl.Element(xdr + "col")?.Value, out gToCol);
                }

                foreach (var gSp in grpSp.Elements(xdr + "sp"))
                {
                    var gSpPr = gSp.Element(xdr + "spPr") ?? gSp.Element(a + "spPr");
                    if (gSpPr == null) continue;
                    var custGeom = gSpPr.Element(a + "custGeom");
                    if (custGeom == null) continue;

                    // Read fill color
                    PdfColor? gFill = null;
                    var gSolidFill = gSpPr.Element(a + "solidFill");
                    if (gSolidFill != null)
                    {
                        var gSrgb = gSolidFill.Element(a + "srgbClr");
                        if (gSrgb != null)
                        {
                            gFill = PdfColor.FromHex(gSrgb.Attribute("val")?.Value ?? "");
                            // Apply alpha: blend with white background
                            var alphaEl = gSrgb.Element(a + "alpha");
                            if (alphaEl != null && int.TryParse(alphaEl.Attribute("val")?.Value, out var alphaVal))
                            {
                                var alpha = alphaVal / 100000f;
                                var fc = gFill.Value;
                                gFill = new PdfColor(
                                    fc.R * alpha + 1f * (1 - alpha),
                                    fc.G * alpha + 1f * (1 - alpha),
                                    fc.B * alpha + 1f * (1 - alpha));
                            }
                        }
                    }
                    if (gFill == null) continue;

                    // Read sp xfrm in child coords
                    var spXfrm = gSpPr.Element(a + "xfrm");
                    long spOffX = 0, spOffY = 0, spCx = 0, spCy = 0;
                    if (spXfrm != null)
                    {
                        long.TryParse(spXfrm.Element(a + "off")?.Attribute("x")?.Value, out spOffX);
                        long.TryParse(spXfrm.Element(a + "off")?.Attribute("y")?.Value, out spOffY);
                        long.TryParse(spXfrm.Element(a + "ext")?.Attribute("cx")?.Value, out spCx);
                        long.TryParse(spXfrm.Element(a + "ext")?.Attribute("cy")?.Value, out spCy);
                    }

                    // Transform to sheet-relative EMU
                    long offsetXEmu = 0, offsetYEmu = 0, wEmu = spCx, hEmu = spCy;
                    if (chExtCx > 0 && chExtCy > 0)
                    {
                        offsetXEmu = (spOffX - chOffX) * grpExtCx / chExtCx;
                        offsetYEmu = (spOffY - chOffY) * grpExtCy / chExtCy;
                        wEmu = spCx * grpExtCx / chExtCx;
                        hEmu = spCy * grpExtCy / chExtCy;
                    }

                    // Parse custGeom path to polygon points
                    var pathEl = custGeom.Descendants(a + "path").FirstOrDefault();
                    if (pathEl == null) continue;
                    long.TryParse(pathEl.Attribute("w")?.Value, out var pathW);
                    long.TryParse(pathEl.Attribute("h")?.Value, out var pathH);
                    if (pathW <= 0 || pathH <= 0) continue;

                    var polyPts = new List<(float X, float Y)>();
                    float curX = 0, curY = 0;
                    foreach (var cmd in pathEl.Elements())
                    {
                        var cmdName = cmd.Name.LocalName;
                        if (cmdName == "moveTo")
                        {
                            var pt = cmd.Element(a + "pt");
                            long.TryParse(pt?.Attribute("x")?.Value, out var mx);
                            long.TryParse(pt?.Attribute("y")?.Value, out var my);
                            curX = (float)mx / pathW;
                            curY = (float)my / pathH;
                            polyPts.Add((curX, curY));
                        }
                        else if (cmdName == "lnTo")
                        {
                            var pt = cmd.Element(a + "pt");
                            long.TryParse(pt?.Attribute("x")?.Value, out var lx);
                            long.TryParse(pt?.Attribute("y")?.Value, out var ly);
                            curX = (float)lx / pathW;
                            curY = (float)ly / pathH;
                            polyPts.Add((curX, curY));
                        }
                        else if (cmdName == "cubicBezTo")
                        {
                            var pts = cmd.Elements(a + "pt").ToArray();
                            if (pts.Length < 3) continue;
                            long.TryParse(pts[0].Attribute("x")?.Value, out var c1x);
                            long.TryParse(pts[0].Attribute("y")?.Value, out var c1y);
                            long.TryParse(pts[1].Attribute("x")?.Value, out var c2x);
                            long.TryParse(pts[1].Attribute("y")?.Value, out var c2y);
                            long.TryParse(pts[2].Attribute("x")?.Value, out var epx);
                            long.TryParse(pts[2].Attribute("y")?.Value, out var epy);
                            float x0 = curX, y0 = curY;
                            float x1 = (float)c1x / pathW, y1 = (float)c1y / pathH;
                            float x2 = (float)c2x / pathW, y2 = (float)c2y / pathH;
                            float x3 = (float)epx / pathW, y3 = (float)epy / pathH;
                            const int segs = 16;
                            for (int si = 1; si <= segs; si++)
                            {
                                float t = (float)si / segs;
                                float u = 1 - t;
                                float bx = u * u * u * x0 + 3 * u * u * t * x1 + 3 * u * t * t * x2 + t * t * t * x3;
                                float by = u * u * u * y0 + 3 * u * u * t * y1 + 3 * u * t * t * y2 + t * t * t * y3;
                                polyPts.Add((bx, by));
                            }
                            curX = x3; curY = y3;
                        }
                    }
                    if (polyPts.Count < 3) continue;

                    shapes.Add(new ExcelDrawingShape(
                        gFromRow, gFromCol, gToRow, gToCol,
                        gFromColOff, gFromRowOff, 0, 0,
                        gFill, null, 0,
                        OffsetXEmu: offsetXEmu,
                        OffsetYEmu: offsetYEmu,
                        WidthEmu: wEmu,
                        HeightEmu: hEmu,
                        PolygonPoints: polyPts));
                }
                continue;
            }

            // Only process direct sp children (not grouped shapes)
            var sp = anchor.Element(xdr + "sp");
            if (sp == null) continue;

            var spPr = sp.Element(xdr + "spPr") ?? sp.Element(a + "spPr");
            if (spPr == null) continue;

            // Only render rectangle-like preset shapes
            var prstGeom = spPr.Element(a + "prstGeom");
            var prst = prstGeom?.Attribute("prst")?.Value ?? "";
            if (prst is not ("rect" or "roundRect" or "round1Rect" or "round2SameRect"
                or "round2DiagRect" or "snip1Rect" or "snip2SameRect" or "snipRoundRect"))
                continue;

            // Read anchor positions
            var fromEl = anchor.Element(xdr + "from");
            var toEl = anchor.Element(xdr + "to");
            if (fromEl == null || toEl == null) continue;

            int.TryParse(fromEl.Element(xdr + "row")?.Value, out var fromRow);
            int.TryParse(fromEl.Element(xdr + "col")?.Value, out var fromCol);
            long.TryParse(fromEl.Element(xdr + "colOff")?.Value, out var fromColOff);
            long.TryParse(fromEl.Element(xdr + "rowOff")?.Value, out var fromRowOff);
            int.TryParse(toEl.Element(xdr + "row")?.Value, out var toRow);
            int.TryParse(toEl.Element(xdr + "col")?.Value, out var toCol);
            long.TryParse(toEl.Element(xdr + "colOff")?.Value, out var toColOff);
            long.TryParse(toEl.Element(xdr + "rowOff")?.Value, out var toRowOff);

            // Read fill
            PdfColor? fillColor = null;
            var solidFill = spPr.Element(a + "solidFill");
            if (solidFill != null)
            {
                var srgb = solidFill.Element(a + "srgbClr");
                var schemeClr = solidFill.Element(a + "schemeClr");
                if (srgb != null)
                    fillColor = PdfColor.FromHex(srgb.Attribute("val")?.Value ?? "");
                else if (schemeClr != null)
                {
                    var idx = SchemeColorToThemeIndex(schemeClr.Attribute("val")?.Value ?? "");
                    if (idx >= 0 && idx < themeColors.Count)
                    {
                        fillColor = themeColors[idx];
                        var lumMod = schemeClr.Element(a + "lumMod");
                        var lumOff = schemeClr.Element(a + "lumOff");
                        if (lumMod != null || lumOff != null)
                        {
                            var mod = lumMod != null && int.TryParse(lumMod.Attribute("val")?.Value, out var m) ? m / 100000.0 : 1.0;
                            var off = lumOff != null && int.TryParse(lumOff.Attribute("val")?.Value, out var o) ? o / 100000.0 : 0.0;
                            var fc = fillColor.Value;
                            fillColor = new PdfColor(
                                (float)Math.Min(1, fc.R * mod + off),
                                (float)Math.Min(1, fc.G * mod + off),
                                (float)Math.Min(1, fc.B * mod + off));
                        }
                    }
                }
            }

            // Read border (line)
            PdfColor? borderColor = null;
            float borderWidthPt = 0;
            var ln = spPr.Element(a + "ln");
            if (ln != null)
            {
                if (long.TryParse(ln.Attribute("w")?.Value, out var wEmu))
                    borderWidthPt = wEmu / 914400f * 72f;
                var lnFill = ln.Element(a + "solidFill");
                if (lnFill != null)
                {
                    var srgb = lnFill.Element(a + "srgbClr");
                    var schemeClr = lnFill.Element(a + "schemeClr");
                    if (srgb != null)
                        borderColor = PdfColor.FromHex(srgb.Attribute("val")?.Value ?? "");
                    else if (schemeClr != null)
                    {
                        var idx = SchemeColorToThemeIndex(schemeClr.Attribute("val")?.Value ?? "");
                        if (idx >= 0 && idx < themeColors.Count)
                        {
                            borderColor = themeColors[idx];
                            var lumMod = schemeClr.Element(a + "lumMod");
                            if (lumMod != null && int.TryParse(lumMod.Attribute("val")?.Value, out var m))
                            {
                                var fc = borderColor.Value;
                                var mod = m / 100000.0;
                                borderColor = new PdfColor(
                                    (float)Math.Min(1, fc.R * mod),
                                    (float)Math.Min(1, fc.G * mod),
                                    (float)Math.Min(1, fc.B * mod));
                            }
                        }
                    }
                }
            }

            if (fillColor == null && borderColor == null) continue;

            shapes.Add(new ExcelDrawingShape(
                fromRow, fromCol, toRow, toCol,
                fromColOff, fromRowOff, toColOff, toRowOff,
                fillColor, borderColor, borderWidthPt));
        }

        return shapes;
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
            var isTwoCellAnchor = anchor.Name == xdr + "twoCellAnchor";
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
            bool showDataLabelVal = false;
            string dataLabelFmtCode = "";
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
                            if (dLbls.Element(cns + "showVal")?.Attribute("val")?.Value == "1")
                                showDataLabelVal = true;
                            var dlNumFmt = dLbls.Element(cns + "numFmt");
                            if (dlNumFmt != null)
                                dataLabelFmtCode = dlNumFmt.Attribute("formatCode")?.Value ?? "";
                        }
                        // Also check per-series dLbls for showVal and format code
                        foreach (var ser in chartTypeEl.Elements(cns + "ser"))
                        {
                            var serDLbls = ser.Element(cns + "dLbls");
                            if (serDLbls != null)
                            {
                                if (serDLbls.Element(cns + "showVal")?.Attribute("val")?.Value == "1")
                                    showDataLabelVal = true;
                                if (string.IsNullOrEmpty(dataLabelFmtCode))
                                {
                                    var serNumFmt = serDLbls.Element(cns + "numFmt");
                                    if (serNumFmt != null)
                                        dataLabelFmtCode = serNumFmt.Attribute("formatCode")?.Value ?? "";
                                }
                                if (showDataLabelVal && !string.IsNullOrEmpty(dataLabelFmtCode))
                                    break;
                            }
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
                seriesList, catAxisTitle, valAxisTitle, showDataLabelPercent, showDataLabelCatName,
                showDataLabelVal, dataLabelFmtCode, valAxisFmtCode, isTwoCellAnchor)
            {
                OverlaySeries = overlaySeries,
                OverlayChartType = overlayChartType
            };
            charts.Add(chartInfo);
        }

        return charts;
    }

    /// <summary>
    /// Resolves a numRef or strRef element to string values.
    /// For numRef: prefers embedded numCache (raw values) over formatted cell text.
    /// For strRef: prefers cell resolution, falls back to strCache.
    /// </summary>
    private static string[] ResolveRefElement(XElement parent, XNamespace cns, List<ExcelSheet> allSheets)
    {
        // numRef: prefer cache (raw numeric values) over formatted cell text
        var numRefEl = parent.Element(cns + "numRef");
        if (numRefEl != null)
        {
            var cacheEl = numRefEl.Element(cns + "numCache");
            if (cacheEl != null)
            {
                var formatCode = cacheEl.Element(cns + "formatCode")?.Value;
                var cached = cacheEl.Elements(cns + "pt")
                    .OrderBy(pt => int.TryParse(pt.Attribute("idx")?.Value, out var idx) ? idx : 0)
                    .Select(pt => pt.Element(cns + "v")?.Value ?? "0")
                    .ToArray();
                if (cached.Length > 0)
                {
                    // If the cache has a date format code, convert serial numbers to date strings
                    if (!string.IsNullOrEmpty(formatCode) && IsDateFormat(formatCode))
                    {
                        for (var i = 0; i < cached.Length; i++)
                        {
                            if (double.TryParse(cached[i], System.Globalization.NumberStyles.Any,
                                System.Globalization.CultureInfo.InvariantCulture, out var serial) && serial > 1)
                                cached[i] = FormatExcelDate(serial, formatCode);
                        }
                    }
                    return cached;
                }
            }
            // Fall back to cell resolution
            var formula = numRefEl.Element(cns + "f")?.Value;
            if (!string.IsNullOrEmpty(formula))
            {
                var resolved = ResolveCellReference(formula, allSheets);
                if (resolved.Length > 0)
                    return resolved;
            }
        }
        // strRef: prefer cell resolution, fall back to strCache
        var strRefEl = parent.Element(cns + "strRef");
        if (strRefEl != null)
        {
            var formula = strRefEl.Element(cns + "f")?.Value;
            if (!string.IsNullOrEmpty(formula))
            {
                var resolved = ResolveCellReference(formula, allSheets);
                if (resolved.Length > 0)
                    return resolved;
            }
            var cacheEl = strRefEl.Element(cns + "strCache");
            if (cacheEl != null)
            {
                var cached = cacheEl.Elements(cns + "pt")
                    .OrderBy(pt => int.TryParse(pt.Attribute("idx")?.Value, out var idx) ? idx : 0)
                    .Select(pt => pt.Element(cns + "v")?.Value ?? "")
                    .ToArray();
                if (cached.Length > 0)
                    return cached;
            }
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
        int.TryParse(addr.Substring(i), out var row);
        row--; // convert to 0-based
        return (col, row);
    }
}

/// <summary>
/// Represents font styling information for a cell.
/// </summary>
internal sealed record FontStyleInfo(PdfColor? Color, float Size = 11f, bool Bold = false, bool Italic = false, bool Underline = false, string? FontName = null);

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
    bool Underline = false,
    CellBorderInfo? Border = null,
    string VerticalAlignment = "bottom",
    bool WrapText = false,
    string? AccountingPrefix = null,
    string? FontName = null,
    int Indent = 0,
    int BoldPrefixLength = 0
);

/// <summary>
/// Represents an image embedded in an Excel worksheet.
/// </summary>
/// <summary>
/// Represents a decorative shape (rectangle, rounded rectangle) from XLSX drawings.
/// </summary>
internal sealed record ExcelDrawingShape(
    int FromRow, int FromCol, int ToRow, int ToCol,
    long FromColOffEmu, long FromRowOffEmu, long ToColOffEmu, long ToRowOffEmu,
    PdfColor? FillColor,
    PdfColor? BorderColor,
    float BorderWidthPt,
    long OffsetXEmu = 0,
    long OffsetYEmu = 0,
    long WidthEmu = 0,
    long HeightEmu = 0,
    List<(float X, float Y)>? PolygonPoints = null
);

internal sealed record ExcelEmbeddedImage(
    int AnchorRow,    // 0-based row index of the top-left anchor
    int AnchorCol,    // 0-based column index of the top-left anchor
    int SpanRows,     // number of rows spanned
    int SpanCols,     // number of columns spanned
    byte[] Data,      // raw image bytes (JPEG or PNG)
    string Extension, // file extension without dot, lower-case, e.g. "jpg"
    long WidthEmu = 0,    // explicit EMU width from <ext>, 0 = not set
    long HeightEmu = 0,   // explicit EMU height from <ext>, 0 = not set
    long FromColOffEmu = 0,  // sub-cell X offset from left edge of AnchorCol (EMU)
    long FromRowOffEmu = 0,  // sub-cell Y offset from top edge of AnchorRow (EMU)
    long ToColOffEmu = 0,    // sub-cell X offset within the "to" column (EMU)
    long ToRowOffEmu = 0,    // sub-cell Y offset within the "to" row (EMU)
    long OffsetXEmu = 0,     // additional X offset from anchor (for grouped images)
    long OffsetYEmu = 0      // additional Y offset from anchor (for grouped images)
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
    bool ShowDataLabelVal = false,       // show value data labels
    string DataLabelFormatCode = "",     // numFmt formatCode for data labels (e.g. "$#,##0")
    string ValueAxisFormatCode = "",    // numFmt formatCode for value axis (e.g. "#,##0")
    bool IsTwoCellAnchor = false,        // true when chart uses twoCellAnchor (cell-relative sizing)
    PdfColor? ChartTextColor = null,     // text color for chart labels/titles (null = black)
    string ChartFontName = ""            // font name for chart text
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
    public List<ExcelDrawingShape> Shapes { get; }
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
    /// <summary>Set of 0-based row indices that have customHeight="1" (fixed height, no auto-expand).</summary>
    public HashSet<int> CustomHeightRows { get; }
    /// <summary>Default row height in points (typically 15).</summary>
    public float DefaultRowHeight { get; }
    /// <summary>Whether the sheet uses landscape orientation.</summary>
    public bool IsLandscape { get; }
    /// <summary>Print scale percentage (10-400, default 100).</summary>
    public int PrintScale { get; internal set; }
    /// <summary>Precise effective print scale factor (overrides PrintScale/100 when set).</summary>
    internal float? EffectivePrintScaleF { get; set; }
    /// <summary>Paper size code (1=Letter, 9=A4, etc). See ECMA-376 §18.8.22.</summary>
    public int PaperSize { get; internal set; }
    /// <summary>Print area range (startCol, startRow, endCol, endRow) 0-based, or null if not set.</summary>
    public (int StartCol, int StartRow, int EndCol, int EndRow)? PrintArea { get; }
    /// <summary>Page margins in points (-1 = use default).</summary>
    public float MarginLeftPt { get; }
    public float MarginRightPt { get; }
    public float MarginTopPt { get; }
    public float MarginBottomPt { get; }
    /// <summary>Whether fitToPage is enabled (auto-scale to fit page width).</summary>
    public bool FitToPage { get; }
    /// <summary>Number of horizontal pages to fit to (ECMA-376 default: 1). 0 = unlimited.</summary>
    public int FitToWidth { get; }
    /// <summary>Number of vertical pages to fit to (ECMA-376 default: 1). 0 = unlimited.</summary>
    public int FitToHeight { get; }
    /// <summary>Whether to center content horizontally on the page.</summary>
    public bool HorizontalCentered { get; }
    /// <summary>Row range to repeat at the top of each printed page (startRow, endRow) 0-based, or null.</summary>
    public (int StartRow, int EndRow)? PrintTitleRows { get; }
    /// <summary>Set of 0-based row indices where a manual page break occurs (break BEFORE this row).</summary>
    public HashSet<int> RowBreaks { get; }
    /// <summary>Raw oddFooter string from XLSX headerFooter element (e.g. "&amp;L&amp;6 text&amp;C&amp;6 Page &amp;P of &amp;N").</summary>
    public string? OddFooter { get; }
    /// <summary>Footer margin in points (distance from page bottom edge to footer text).</summary>
    public float FooterMarginPt { get; }

    /// <summary>Maximum digit width in pixels (96 DPI) for the workbook's normal font.
    /// Used by the ECMA-376-based column width conversion. Default matches Calibri.</summary>
    public float MaxDigitWidthPx { get; }

    /// <summary>Converts Excel character-unit column width to PDF points.</summary>
    public static float CharUnitsToPoints(float charUnits)
        // Calibrated against LibreOffice reference PDFs: 8.43 char-units → 47.4pt
        => charUnits * 5.62f;

    /// <summary>Font-aware char-to-point conversion that scales the calibrated
    /// factor by the ratio of the workbook font's max digit width to Calibri's.</summary>
    public float CharUnitsToPointsScaled(float charUnits)
    {
        const float calibrationMdw = 7.378f; // mdw implied by the 5.5334 calibration factor
        var ratio = MaxDigitWidthPx / calibrationMdw;
        return charUnits * 5.5334f * ratio + 0.3232f * ratio;
    }

    /// <summary>Font-aware default column width conversion.</summary>
    public float CharUnitsToPointsDefaultScaled(float charUnits)
    {
        const float calibrationMdw = 7.378f;
        var ratio = MaxDigitWidthPx / calibrationMdw;
        return charUnits * 5.62f * ratio;
    }

    internal ExcelSheet(string name, List<List<ExcelCell>> rows,
        List<ExcelEmbeddedImage>? images = null,
        Dictionary<int, float>? columnWidths = null,
        float defaultColumnWidth = 8.43f,
        List<ExcelChartInfo>? charts = null,
        List<ExcelDrawingShape>? shapes = null,
        List<(int, int, int, int)>? mergedCells = null,
        Dictionary<int, float>? rowHeights = null,
        float defaultRowHeight = 15f,
        HashSet<int>? customHeightRows = null,
        bool isLandscape = false,
        int printScale = 100,
        int paperSize = 1,
        (int, int, int, int)? printArea = null,
        float marginLeftPt = -1, float marginRightPt = -1,
        float marginTopPt = -1, float marginBottomPt = -1,
        bool fitToPage = false,
        int fitToWidth = 1, int fitToHeight = 1,
        bool horizontalCentered = false,
        (int StartRow, int EndRow)? printTitleRows = null,
        HashSet<int>? rowBreaks = null,
        string? oddFooter = null,
        float footerMarginPt = -1,
        float maxDigitWidthPx = 7.378f)
    {
        Name = name;
        Rows = rows;
        Images = images ?? new List<ExcelEmbeddedImage>();
        Shapes = shapes ?? new List<ExcelDrawingShape>();
        Charts = charts ?? new List<ExcelChartInfo>();
        ColumnWidths = columnWidths ?? new Dictionary<int, float>();
        DefaultColumnWidth = defaultColumnWidth;
        MergedCells = mergedCells ?? new List<(int, int, int, int)>();
        RowHeights = rowHeights ?? new Dictionary<int, float>();
        DefaultRowHeight = defaultRowHeight;
        CustomHeightRows = customHeightRows ?? new HashSet<int>();
        IsLandscape = isLandscape;
        PrintScale = printScale;
        PaperSize = paperSize;
        PrintArea = printArea;
        MarginLeftPt = marginLeftPt;
        MarginRightPt = marginRightPt;
        MarginTopPt = marginTopPt;
        MarginBottomPt = marginBottomPt;
        FitToPage = fitToPage;
        FitToWidth = fitToWidth;
        FitToHeight = fitToHeight;
        HorizontalCentered = horizontalCentered;
        PrintTitleRows = printTitleRows;
        RowBreaks = rowBreaks ?? new HashSet<int>();
        OddFooter = oddFooter;
        FooterMarginPt = footerMarginPt;
        MaxDigitWidthPx = maxDigitWidthPx;
    }

    /// <summary>
    /// Looks up the max digit width (pixels at 96 DPI) for common fonts.
    /// The lookup table stores values measured at 11pt. When the Normal style
    /// uses a different font size, the value is scaled proportionally so that
    /// column-width-to-points conversion matches the actual workbook layout.
    /// Returns the calibration default for unknown fonts so existing behaviour is preserved.
    /// </summary>
    internal static float LookupMaxDigitWidthPx(string? fontName, float fontSizePt = 11f)
    {
        const float calibrationSizePt = 11f;
        float baseMdw;
        if (string.IsNullOrWhiteSpace(fontName))
        {
            baseMdw = 7.378f;
        }
        else
        {
            baseMdw = fontName.Trim().ToLowerInvariant() switch
            {
                "century gothic" => 8.13f,
                "franklin gothic medium" => 8.5f,
                "franklin gothic book" => 8.2f,
                "georgia" => 7.8f,
                "verdana" => 8.3f,
                "trebuchet ms" => 7.6f,
                "tahoma" => 8.0f,
                "garamond" => 6.8f,
                "corbel" => 7.6f,
                "times new roman" => 6.8f,
                "consolas" or "courier new" => 7.3f,
                _ => 7.378f, // Calibri / Arial / Aptos and other common fonts
            };
        }

        // Scale from the 11pt calibration size to the actual Normal font size
        if (fontSizePt > 0 && Math.Abs(fontSizePt - calibrationSizePt) > 0.01f)
            baseMdw *= fontSizePt / calibrationSizePt;

        return baseMdw;
    }
}
