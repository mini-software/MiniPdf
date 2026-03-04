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

        // Read styles (font colors, fill colors, number formats)
        var fontColors = ReadFontColors(archive);
        var fillColors = ReadFillColors(archive);
        var numberFormats = ReadNumberFormats(archive);
        var (cellXfFontIndices, cellXfFillIndices, cellXfNumFmtIds) = ReadCellXfStyles(archive);

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

            var rows = ReadSheet(entry, sharedStrings, fontColors, fillColors, numberFormats, cellXfFontIndices, cellXfFillIndices, cellXfNumFmtIds);
            var images = ReadSheetImages(archive, info.SheetId);
            var (colWidths, defaultColWidth) = ReadColumnWidths(entry);
            var mergedCells = ReadMergedCells(entry);
            sheets.Add(new ExcelSheet(info.Name, rows, images, colWidths, defaultColWidth, mergedCells: mergedCells));
        }

        // If no sheets found via workbook, try reading sheet1 directly
        if (sheets.Count == 0)
        {
            var entry = archive.GetEntry("xl/worksheets/sheet1.xml");
            if (entry != null)
            {
                var rows = ReadSheet(entry, sharedStrings, fontColors, fillColors, numberFormats, cellXfFontIndices, cellXfFillIndices, cellXfNumFmtIds);
                var images = ReadSheetImages(archive, 1);
                var (colWidths, defaultColWidth) = ReadColumnWidths(entry);
                var mergedCells = ReadMergedCells(entry);
                sheets.Add(new ExcelSheet("Sheet1", rows, images, colWidths, defaultColWidth, mergedCells: mergedCells));
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

    /// <summary>
    /// Reads cellXf style entries from styles.xml.
    /// Returns (fontIndices, fillIndices, numFmtIds) parallel lists.
    /// </summary>
    private static (List<int> FontIndices, List<int> FillIndices, List<int> NumFmtIds) ReadCellXfStyles(ZipArchive archive)
    {
        var fontIndices = new List<int>();
        var fillIndices = new List<int>();
        var numFmtIds = new List<int>();
        var entry = archive.GetEntry("xl/styles.xml");
        if (entry == null) return (fontIndices, fillIndices, numFmtIds);

        using var stream = entry.Open();
        var doc = XDocument.Load(stream);
        var ns = doc.Root?.GetDefaultNamespace() ?? XNamespace.None;

        // Read <cellXfs> -> <xf> elements
        var cellXfs = doc.Descendants(ns + "cellXfs").FirstOrDefault();
        if (cellXfs == null) return (fontIndices, fillIndices, numFmtIds);

        foreach (var xf in cellXfs.Elements(ns + "xf"))
        {
            var fontId = xf.Attribute("fontId")?.Value;
            fontIndices.Add(int.TryParse(fontId, out var fid) ? fid : 0);

            var fillId = xf.Attribute("fillId")?.Value;
            fillIndices.Add(int.TryParse(fillId, out var filli) ? filli : 0);

            var numFmtId = xf.Attribute("numFmtId")?.Value;
            numFmtIds.Add(int.TryParse(numFmtId, out var nid) ? nid : 0);
        }

        return (fontIndices, fillIndices, numFmtIds);
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
            if (patternType != "solid")
            {
                fills.Add(null);
                continue;
            }

            // Read foreground color (fgColor) for solid fills
            var fgColor = patternFill.Element(ns + "fgColor");
            if (fgColor == null)
            {
                fills.Add(null);
                continue;
            }

            var rgb = fgColor.Attribute("rgb")?.Value;
            if (!string.IsNullOrEmpty(rgb))
            {
                var c = PdfColor.FromHex(rgb);
                // Skip pure white fills (FFFFFF) as they're invisible
                if (c.R < 0.99f || c.G < 0.99f || c.B < 0.99f)
                    fills.Add(c);
                else
                    fills.Add(null);
            }
            else
            {
                var indexed = fgColor.Attribute("indexed")?.Value;
                if (!string.IsNullOrEmpty(indexed) && int.TryParse(indexed, out var idx))
                    fills.Add(GetIndexedColor(idx));
                else
                    fills.Add(null);
            }
        }

        return fills;
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

    private static PdfColor? ResolveCellColor(int styleIndex, List<PdfColor?> fontColors, List<int> fontIndices)
    {
        if (styleIndex < 0 || styleIndex >= fontIndices.Count)
            return null;

        var fontIndex = fontIndices[styleIndex];
        if (fontIndex < 0 || fontIndex >= fontColors.Count)
            return null;

        return fontColors[fontIndex];
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
        List<PdfColor?> fontColors, List<PdfColor?> fillColors, Dictionary<int, string> numberFormats,
        List<int> cellXfFontIndices, List<int> cellXfFillIndices, List<int> cellXfNumFmtIds)
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
                if (int.TryParse(styleAttr, out var styleIndex))
                {
                    color = ResolveCellColor(styleIndex, fontColors, cellXfFontIndices);
                    fillColor = ResolveFillColor(styleIndex, fillColors, cellXfFillIndices);
                    if (styleIndex >= 0 && styleIndex < cellXfNumFmtIds.Count)
                        numFmtId = cellXfNumFmtIds[styleIndex];
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

                cells.Add(new ExcelCell(text, color, fillColor));
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
            14 or 15 or 16 or 17 or 22 => FormatExcelDate(value),
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
        if (sections.Length >= 3 && value == 0)
            activeFormat = sections[2];
        else if (sections.Length >= 2 && value < 0)
        {
            activeFormat = sections[1];
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

        // Handle date-like formats
        if (activeFormat.Contains("yy") || activeFormat.Contains("mm") || activeFormat.Contains("dd"))
        {
            return FormatExcelDate(value);
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

        string formatted;
        if (hasThousands)
            formatted = value.ToString($"N{decimalPlaces}", ci);
        else if (hasDecimal)
            formatted = value.ToString($"F{decimalPlaces}", ci);
        else if (value == Math.Floor(value))
            formatted = value.ToString("F0", ci);
        else
            formatted = FormatGeneral(value);

        return prefix + formatted + suffix;
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

        // Exact integer: show as integer (doubles that have no fractional part).
        // FitNumericText handles rounding near-integers to fit column width later.
        if (value == Math.Floor(value) && abs < 1e15)
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
        return value.ToString("G10", ci);
    }

    /// <summary>
    /// Converts an Excel serial date number to a date string (yyyy-mm-dd).
    /// Excel epoch: Jan 1, 1900 = serial number 1.
    /// </summary>
    private static string FormatExcelDate(double serialDate)
    {
        try
        {
            // Excel incorrectly considers 1900 as a leap year (Feb 29, 1900 = serial 60).
            // For dates after Feb 28, 1900, subtract 1 to correct.
            var days = (int)serialDate;
            if (days > 60) days--;
            var date = new DateTime(1900, 1, 1).AddDays(days - 1);
            return date.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
        }
        catch
        {
            return serialDate.ToString("G10", System.Globalization.CultureInfo.InvariantCulture);
        }
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
            }

            charts.Add(new ExcelChartInfo(fromRow, fromCol, widthEmu, heightEmu, title, chartType,
                seriesList, catAxisTitle, valAxisTitle, showDataLabelPercent, showDataLabelCatName, valAxisFmtCode));
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
internal sealed record ExcelCell(string Text, PdfColor? Color, PdfColor? FillColor);

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
    /// <summary>Merged cell regions: (startRow, startCol, endRow, endCol) all 0-based.</summary>
    public List<(int StartRow, int StartCol, int EndRow, int EndCol)> MergedCells { get; }

    /// <summary>Converts Excel character-unit column width to PDF points.</summary>
    public static float CharUnitsToPoints(float charUnits)
        // Calibrated against LibreOffice reference PDFs: 8.43 char-units → 47.4pt
        => charUnits * 5.62f;

    internal ExcelSheet(string name, List<List<ExcelCell>> rows,
        List<ExcelEmbeddedImage>? images = null,
        Dictionary<int, float>? columnWidths = null,
        float defaultColumnWidth = 8.43f,
        List<ExcelChartInfo>? charts = null,
        List<(int, int, int, int)>? mergedCells = null)
    {
        Name = name;
        Rows = rows;
        Images = images ?? new List<ExcelEmbeddedImage>();
        Charts = charts ?? new List<ExcelChartInfo>();
        ColumnWidths = columnWidths ?? new Dictionary<int, float>();
        DefaultColumnWidth = defaultColumnWidth;
        MergedCells = mergedCells ?? new List<(int, int, int, int)>();
    }
}
