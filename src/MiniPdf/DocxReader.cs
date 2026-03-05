using System.IO.Compression;
using System.Xml.Linq;

namespace MiniSoftware;

/// <summary>
/// Reads basic content from Word (.docx) files.
/// Supports reading paragraphs, tables, and embedded images without external dependencies.
/// </summary>
internal static class DocxReader
{
    private static readonly XNamespace W = "http://schemas.openxmlformats.org/wordprocessingml/2006/main";
    private static readonly XNamespace R = "http://schemas.openxmlformats.org/officeDocument/2006/relationships";
    private static readonly XNamespace WP = "http://schemas.openxmlformats.org/drawingml/2006/wordprocessingDrawing";
    private static readonly XNamespace A = "http://schemas.openxmlformats.org/drawingml/2006/main";
    private static readonly XNamespace PIC = "http://schemas.openxmlformats.org/drawingml/2006/picture";
    private static readonly XNamespace REL = "http://schemas.openxmlformats.org/package/2006/relationships";

    /// <summary>
    /// Reads a DOCX file and returns a structured document model.
    /// </summary>
    internal static DocxDocument Read(Stream stream)
    {
        using var archive = new ZipArchive(stream, ZipArchiveMode.Read, leaveOpen: true);

        // Read relationships to resolve image references
        var relationships = ReadRelationships(archive);

        // Read styles
        var styles = ReadStyles(archive);

        // Read numbering definitions (for list bullets/numbers)
        var numbering = ReadNumbering(archive);

        // Read main document
        var entry = archive.GetEntry("word/document.xml");
        if (entry == null)
            return new DocxDocument([]);

        using var docStream = entry.Open();
        var doc = XDocument.Load(docStream);
        var body = doc.Descendants(W + "body").FirstOrDefault();
        if (body == null)
            return new DocxDocument([]);

        var elements = new List<DocxElement>();

        foreach (var child in body.Elements())
        {
            if (child.Name == W + "p")
            {
                var paragraph = ReadParagraph(child, styles, numbering, relationships, archive);
                if (paragraph != null)
                    elements.Add(paragraph);
            }
            else if (child.Name == W + "tbl")
            {
                var table = ReadTable(child, styles, numbering, relationships, archive);
                if (table != null)
                    elements.Add(table);
            }
        }

        return new DocxDocument(elements);
    }

    private static DocxParagraph? ReadParagraph(XElement pElement, Dictionary<string, DocxStyleInfo> styles,
        Dictionary<string, DocxNumberingDef> numbering, Dictionary<string, string> relationships, ZipArchive archive)
    {
        var runs = new List<DocxRun>();
        var images = new List<DocxImage>();

        // Read paragraph properties
        var pPr = pElement.Element(W + "pPr");
        var alignment = "left";
        float spacingBefore = 0;
        float spacingAfter = 0;
        float lineSpacing = 0;
        float indentLeft = 0;
        float indentRight = 0;
        float indentFirstLine = 0;
        bool isBulletList = false;
        bool isNumberedList = false;
        int listLevel = 0;
        string? listText = null;
        string? styleId = null;
        bool bold = false;
        bool italic = false;
        float fontSize = 0;
        PdfColor? color = null;

        if (pPr != null)
        {
            // Style reference
            styleId = pPr.Element(W + "pStyle")?.Attribute(W + "val")?.Value;

            // Alignment
            var jc = pPr.Element(W + "jc")?.Attribute(W + "val")?.Value;
            if (!string.IsNullOrEmpty(jc))
                alignment = jc;

            // Spacing (in twips: 1/20 of a point)
            var spacing = pPr.Element(W + "spacing");
            if (spacing != null)
            {
                if (int.TryParse(spacing.Attribute(W + "before")?.Value, out var sb))
                    spacingBefore = sb / 20f;
                if (int.TryParse(spacing.Attribute(W + "after")?.Value, out var sa))
                    spacingAfter = sa / 20f;
                if (int.TryParse(spacing.Attribute(W + "line")?.Value, out var sl))
                    lineSpacing = sl / 20f;
            }

            // Indentation (in twips)
            var ind = pPr.Element(W + "ind");
            if (ind != null)
            {
                if (int.TryParse(ind.Attribute(W + "left")?.Value, out var il))
                    indentLeft = il / 20f;
                if (int.TryParse(ind.Attribute(W + "right")?.Value, out var ir))
                    indentRight = ir / 20f;
                if (int.TryParse(ind.Attribute(W + "firstLine")?.Value, out var fl))
                    indentFirstLine = fl / 20f;
                if (int.TryParse(ind.Attribute(W + "hanging")?.Value, out var hg))
                    indentFirstLine = -hg / 20f;
            }

            // Numbering (lists)
            var numPr = pPr.Element(W + "numPr");
            if (numPr != null)
            {
                var numId = numPr.Element(W + "numId")?.Attribute(W + "val")?.Value;
                var ilvl = numPr.Element(W + "ilvl")?.Attribute(W + "val")?.Value;
                listLevel = int.TryParse(ilvl, out var lv) ? lv : 0;

                if (!string.IsNullOrEmpty(numId) && numId != "0" && numbering.TryGetValue(numId, out var numDef))
                {
                    if (numDef.Format == "bullet")
                    {
                        isBulletList = true;
                        listText = "\u2022"; // bullet character
                    }
                    else
                    {
                        isNumberedList = true;
                        numDef.Counter++;
                        listText = numDef.Counter + ".";
                    }
                }
            }

            // Paragraph-level run properties
            var rPr = pPr.Element(W + "rPr");
            if (rPr != null)
            {
                bold = rPr.Element(W + "b") != null;
                italic = rPr.Element(W + "i") != null;
                var sz = rPr.Element(W + "sz")?.Attribute(W + "val")?.Value;
                if (float.TryParse(sz, out var s))
                    fontSize = s / 2f; // half-points to points
                color = ReadRunColor(rPr);
            }
        }

        // Apply style defaults
        if (!string.IsNullOrEmpty(styleId) && styles.TryGetValue(styleId, out var styleInfo))
        {
            if (fontSize == 0) fontSize = styleInfo.FontSize;
            if (!bold) bold = styleInfo.Bold;
            if (!italic) italic = styleInfo.Italic;
            if (color == null) color = styleInfo.Color;
            if (alignment == "left" && !string.IsNullOrEmpty(styleInfo.Alignment))
                alignment = styleInfo.Alignment;
            if (spacingBefore == 0) spacingBefore = styleInfo.SpacingBefore;
            if (spacingAfter == 0) spacingAfter = styleInfo.SpacingAfter;
        }

        // Read runs
        foreach (var child in pElement.Elements())
        {
            if (child.Name == W + "r")
            {
                var run = ReadRun(child, bold, italic, fontSize, color);
                if (run != null)
                    runs.Add(run);

                // Check for inline images in the run
                var drawing = child.Descendants(W + "drawing").FirstOrDefault();
                if (drawing != null)
                {
                    var image = ReadImage(drawing, relationships, archive);
                    if (image != null)
                        images.Add(image);
                }
            }
            else if (child.Name == W + "hyperlink")
            {
                // Extract text from hyperlink runs
                foreach (var r in child.Elements(W + "r"))
                {
                    var run = ReadRun(r, bold, italic, fontSize, color);
                    if (run != null)
                        runs.Add(run);
                }
            }
        }

        // If paragraph has no runs and no images, represent as empty paragraph for spacing
        return new DocxParagraph(runs, images, alignment, spacingBefore, spacingAfter,
            lineSpacing, indentLeft, indentRight, indentFirstLine,
            isBulletList, isNumberedList, listLevel, listText, styleId,
            bold, italic, fontSize, color);
    }

    private static DocxRun? ReadRun(XElement rElement, bool parentBold, bool parentItalic, float parentFontSize, PdfColor? parentColor)
    {
        var rPr = rElement.Element(W + "rPr");
        var bold = parentBold;
        var italic = parentItalic;
        var fontSize = parentFontSize;
        var color = parentColor;

        if (rPr != null)
        {
            if (rPr.Element(W + "b") != null) bold = true;
            if (rPr.Element(W + "i") != null) italic = true;
            var sz = rPr.Element(W + "sz")?.Attribute(W + "val")?.Value;
            if (float.TryParse(sz, out var s) && s > 0)
                fontSize = s / 2f; // half-points to points
            var runColor = ReadRunColor(rPr);
            if (runColor != null) color = runColor;
        }

        // Collect text from <w:t>, <w:tab>, <w:br> elements
        var text = "";
        foreach (var child in rElement.Elements())
        {
            if (child.Name == W + "t")
                text += child.Value;
            else if (child.Name == W + "tab")
                text += "\t";
            else if (child.Name == W + "br")
                text += "\n";
        }

        if (string.IsNullOrEmpty(text))
            return null;

        return new DocxRun(text, bold, italic, fontSize, color);
    }

    private static PdfColor? ReadRunColor(XElement rPr)
    {
        var colorEl = rPr.Element(W + "color");
        if (colorEl == null) return null;
        var val = colorEl.Attribute(W + "val")?.Value;
        if (string.IsNullOrEmpty(val) || val == "auto") return null;
        return PdfColor.FromHex(val);
    }

    private static DocxImage? ReadImage(XElement drawing, Dictionary<string, string> relationships, ZipArchive archive)
    {
        // Try inline image first, then anchor
        var inline = drawing.Descendants(WP + "inline").FirstOrDefault();
        var anchor = drawing.Descendants(WP + "anchor").FirstOrDefault();
        var container = inline ?? anchor;
        if (container == null) return null;

        // Get extent (size in EMUs)
        var extent = container.Element(WP + "extent");
        long widthEmu = 0, heightEmu = 0;
        if (extent != null)
        {
            long.TryParse(extent.Attribute("cx")?.Value, out widthEmu);
            long.TryParse(extent.Attribute("cy")?.Value, out heightEmu);
        }

        // Find the blip (image reference)
        var blip = container.Descendants(A + "blip").FirstOrDefault();
        if (blip == null) return null;

        var rEmbed = blip.Attribute(R + "embed")?.Value;
        if (string.IsNullOrEmpty(rEmbed) || !relationships.TryGetValue(rEmbed, out var target))
            return null;

        // Read image data from archive
        var imagePath = "word/" + target;
        var imageEntry = archive.GetEntry(imagePath);
        if (imageEntry == null) return null;

        using var imgStream = imageEntry.Open();
        using var ms = new MemoryStream();
        imgStream.CopyTo(ms);
        var data = ms.ToArray();

        var ext = Path.GetExtension(target).TrimStart('.').ToLowerInvariant();
        if (ext == "jpeg") ext = "jpg";

        return new DocxImage(data, ext, widthEmu, heightEmu);
    }

    private static DocxTable? ReadTable(XElement tblElement, Dictionary<string, DocxStyleInfo> styles,
        Dictionary<string, DocxNumberingDef> numbering, Dictionary<string, string> relationships, ZipArchive archive)
    {
        var rows = new List<DocxTableRow>();

        // Read table properties (borders, column widths)
        var tblPr = tblElement.Element(W + "tblPr");
        var tblGrid = tblElement.Element(W + "tblGrid");
        var columnWidths = new List<float>();
        if (tblGrid != null)
        {
            foreach (var col in tblGrid.Elements(W + "gridCol"))
            {
                if (int.TryParse(col.Attribute(W + "w")?.Value, out var w))
                    columnWidths.Add(w / 20f); // twips to points
                else
                    columnWidths.Add(72f); // default 1 inch
            }
        }

        foreach (var tr in tblElement.Elements(W + "tr"))
        {
            var cells = new List<DocxTableCell>();
            foreach (var tc in tr.Elements(W + "tc"))
            {
                var cellParagraphs = new List<DocxParagraph>();
                foreach (var p in tc.Elements(W + "p"))
                {
                    var para = ReadParagraph(p, styles, numbering, relationships, archive);
                    if (para != null)
                        cellParagraphs.Add(para);
                }

                // Read cell properties
                var tcPr = tc.Element(W + "tcPr");
                float cellWidth = 0;
                int gridSpan = 1;
                PdfColor? shading = null;

                if (tcPr != null)
                {
                    var wEl = tcPr.Element(W + "tcW");
                    if (wEl != null && int.TryParse(wEl.Attribute(W + "w")?.Value, out var cw))
                        cellWidth = cw / 20f;

                    var gsEl = tcPr.Element(W + "gridSpan");
                    if (gsEl != null && int.TryParse(gsEl.Attribute(W + "val")?.Value, out var gs))
                        gridSpan = gs;

                    var shdEl = tcPr.Element(W + "shd");
                    if (shdEl != null)
                    {
                        var fill = shdEl.Attribute(W + "fill")?.Value;
                        if (!string.IsNullOrEmpty(fill) && fill != "auto")
                            shading = PdfColor.FromHex(fill);
                    }
                }

                cells.Add(new DocxTableCell(cellParagraphs, cellWidth, gridSpan, shading));
            }
            rows.Add(new DocxTableRow(cells));
        }

        return new DocxTable(rows, columnWidths);
    }

    private static Dictionary<string, string> ReadRelationships(ZipArchive archive)
    {
        var rels = new Dictionary<string, string>();
        var entry = archive.GetEntry("word/_rels/document.xml.rels");
        if (entry == null) return rels;

        using var stream = entry.Open();
        var doc = XDocument.Load(stream);

        foreach (var rel in doc.Descendants(REL + "Relationship"))
        {
            var id = rel.Attribute("Id")?.Value;
            var target = rel.Attribute("Target")?.Value;
            if (!string.IsNullOrEmpty(id) && !string.IsNullOrEmpty(target))
                rels[id] = target;
        }

        return rels;
    }

    private static Dictionary<string, DocxStyleInfo> ReadStyles(ZipArchive archive)
    {
        var styles = new Dictionary<string, DocxStyleInfo>();
        var entry = archive.GetEntry("word/styles.xml");
        if (entry == null) return styles;

        using var stream = entry.Open();
        var doc = XDocument.Load(stream);

        foreach (var style in doc.Descendants(W + "style"))
        {
            var styleId = style.Attribute(W + "styleId")?.Value;
            if (string.IsNullOrEmpty(styleId)) continue;

            var rPr = style.Element(W + "rPr");
            var pPr = style.Element(W + "pPr");

            float fontSize = 11f;
            bool bold = false;
            bool italic = false;
            PdfColor? color = null;
            string alignment = "";
            float spacingBefore = 0;
            float spacingAfter = 0;

            if (rPr != null)
            {
                bold = rPr.Element(W + "b") != null;
                italic = rPr.Element(W + "i") != null;
                var sz = rPr.Element(W + "sz")?.Attribute(W + "val")?.Value;
                if (float.TryParse(sz, out var s) && s > 0)
                    fontSize = s / 2f;
                color = ReadRunColor(rPr);
            }

            if (pPr != null)
            {
                alignment = pPr.Element(W + "jc")?.Attribute(W + "val")?.Value ?? "";
                var spacing = pPr.Element(W + "spacing");
                if (spacing != null)
                {
                    if (int.TryParse(spacing.Attribute(W + "before")?.Value, out var sb))
                        spacingBefore = sb / 20f;
                    if (int.TryParse(spacing.Attribute(W + "after")?.Value, out var sa))
                        spacingAfter = sa / 20f;
                }
            }

            // Heading styles get larger font sizes
            if (styleId.StartsWith("Heading", StringComparison.OrdinalIgnoreCase) ||
                styleId.StartsWith("heading", StringComparison.Ordinal))
            {
                bold = true;
                if (styleId.EndsWith("1")) fontSize = Math.Max(fontSize, 24f);
                else if (styleId.EndsWith("2")) fontSize = Math.Max(fontSize, 18f);
                else if (styleId.EndsWith("3")) fontSize = Math.Max(fontSize, 14f);
                else if (styleId.EndsWith("4")) fontSize = Math.Max(fontSize, 12f);
            }

            styles[styleId] = new DocxStyleInfo(fontSize, bold, italic, color, alignment, spacingBefore, spacingAfter);
        }

        return styles;
    }

    private static Dictionary<string, DocxNumberingDef> ReadNumbering(ZipArchive archive)
    {
        var result = new Dictionary<string, DocxNumberingDef>();
        var entry = archive.GetEntry("word/numbering.xml");
        if (entry == null) return result;

        using var stream = entry.Open();
        var doc = XDocument.Load(stream);

        // Read abstract numbering definitions
        var abstractDefs = new Dictionary<string, string>(); // abstractNumId → format
        foreach (var absNum in doc.Descendants(W + "abstractNum"))
        {
            var absId = absNum.Attribute(W + "abstractNumId")?.Value;
            if (string.IsNullOrEmpty(absId)) continue;

            var lvl = absNum.Elements(W + "lvl").FirstOrDefault();
            var numFmt = lvl?.Element(W + "numFmt")?.Attribute(W + "val")?.Value ?? "decimal";
            abstractDefs[absId] = numFmt;
        }

        // Map num IDs to abstract definitions
        foreach (var num in doc.Descendants(W + "num"))
        {
            var numId = num.Attribute(W + "numId")?.Value;
            if (string.IsNullOrEmpty(numId)) continue;

            var absRef = num.Element(W + "abstractNumId")?.Attribute(W + "val")?.Value;
            var format = "decimal";
            if (!string.IsNullOrEmpty(absRef) && abstractDefs.TryGetValue(absRef, out var f))
                format = f;

            result[numId] = new DocxNumberingDef(format);
        }

        return result;
    }
}

// ── Document model ──────────────────────────────────────────────────────

/// <summary>Represents a parsed DOCX document.</summary>
internal sealed record DocxDocument(List<DocxElement> Elements);

/// <summary>Base type for document elements (paragraphs, tables).</summary>
internal abstract record DocxElement;

/// <summary>Represents a paragraph in a DOCX document.</summary>
internal sealed record DocxParagraph(
    List<DocxRun> Runs,
    List<DocxImage> Images,
    string Alignment = "left",
    float SpacingBefore = 0,
    float SpacingAfter = 0,
    float LineSpacing = 0,
    float IndentLeft = 0,
    float IndentRight = 0,
    float IndentFirstLine = 0,
    bool IsBulletList = false,
    bool IsNumberedList = false,
    int ListLevel = 0,
    string? ListText = null,
    string? StyleId = null,
    bool Bold = false,
    bool Italic = false,
    float FontSize = 0,
    PdfColor? Color = null
) : DocxElement;

/// <summary>Represents a run of formatted text.</summary>
internal sealed record DocxRun(
    string Text,
    bool Bold = false,
    bool Italic = false,
    float FontSize = 0,
    PdfColor? Color = null
);

/// <summary>Represents an embedded image.</summary>
internal sealed record DocxImage(
    byte[] Data,
    string Extension,
    long WidthEmu = 0,
    long HeightEmu = 0
);

/// <summary>Represents a table.</summary>
internal sealed record DocxTable(
    List<DocxTableRow> Rows,
    List<float> ColumnWidths
) : DocxElement;

/// <summary>Represents a table row.</summary>
internal sealed record DocxTableRow(List<DocxTableCell> Cells);

/// <summary>Represents a table cell.</summary>
internal sealed record DocxTableCell(
    List<DocxParagraph> Paragraphs,
    float Width = 0,
    int GridSpan = 1,
    PdfColor? Shading = null
);

/// <summary>Style definition from styles.xml.</summary>
internal sealed record DocxStyleInfo(
    float FontSize = 11f,
    bool Bold = false,
    bool Italic = false,
    PdfColor? Color = null,
    string Alignment = "",
    float SpacingBefore = 0,
    float SpacingAfter = 0
);

/// <summary>Numbering definition for lists.</summary>
internal sealed class DocxNumberingDef
{
    public string Format { get; }
    public int Counter { get; set; }

    public DocxNumberingDef(string format)
    {
        Format = format;
        Counter = 0;
    }
}
