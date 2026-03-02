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
/// Represents a sheet read from an Excel file.
/// </summary>
internal sealed class ExcelSheet
{
    public string Name { get; }
    public List<List<ExcelCell>> Rows { get; }
    public List<ExcelEmbeddedImage> Images { get; }
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
        // Helvetica 10pt: digit "0" is ~5.5pt wide, plus ~5pt padding
        => charUnits * 5.5f + 5f;

    internal ExcelSheet(string name, List<List<ExcelCell>> rows,
        List<ExcelEmbeddedImage>? images = null,
        Dictionary<int, float>? columnWidths = null,
        float defaultColumnWidth = 8.43f)
    {
        Name = name;
        Rows = rows;
        Images = images ?? new List<ExcelEmbeddedImage>();
        ColumnWidths = columnWidths ?? new Dictionary<int, float>();
        DefaultColumnWidth = defaultColumnWidth;
    }
}
