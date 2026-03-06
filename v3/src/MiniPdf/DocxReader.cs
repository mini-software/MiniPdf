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
        var styleListCounter = 0; // counter for style-based numbered lists

        foreach (var child in body.Elements())
        {
            if (child.Name == W + "p")
            {
                var paragraph = ReadParagraph(child, styles, numbering, relationships, archive);
                if (paragraph != null)
                {
                    // Fix up style-based numbered list counter
                    if (paragraph.IsNumberedList && paragraph.ListText == "1.")
                    {
                        styleListCounter++;
                        paragraph = paragraph with { ListText = styleListCounter + "." };
                    }
                    else if (!paragraph.IsNumberedList)
                    {
                        styleListCounter = 0;
                    }
                    elements.Add(paragraph);
                }
            }
            else if (child.Name == W + "tbl")
            {
                var table = ReadTable(child, styles, numbering, relationships, archive);
                if (table != null)
                    elements.Add(table);
            }
        }

        // Read page layout from sectPr
        var pageLayout = ReadPageLayout(body);

        return new DocxDocument(elements, pageLayout);
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
        float spacingAfter = -1;
        float lineSpacing = 0;
        float indentLeft = 0;
        float indentRight = 0;
        float indentFirstLine = 0;
        bool isBulletList = false;
        bool isNumberedList = false;
        bool pageBreakBefore = false;
        bool pageBreakAfter = false;
        int listLevel = 0;
        string? listText = null;
        string? styleId = null;
        bool bold = false;
        bool italic = false;
        float fontSize = 0;
        PdfColor? color = null;
        PdfColor? paragraphShading = null;
        List<DocxTabStop>? tabStops = null;

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
                {
                    var lineRule = spacing.Attribute(W + "lineRule")?.Value;
                    lineSpacing = (lineRule == "exact" || lineRule == "atLeast")
                        ? sl / 20f   // absolute value in points
                        : sl / 240f; // multiplier (auto: 240 = single spacing)
                }
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

            // Page break before
            if (pPr.Element(W + "pageBreakBefore") != null)
                pageBreakBefore = true;

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

            // Detect list paragraphs by style name (when numPr is on the style, not the paragraph)
            if (!isBulletList && !isNumberedList && !string.IsNullOrEmpty(styleId))
            {
                if (styleId.StartsWith("ListBullet", StringComparison.OrdinalIgnoreCase))
                {
                    isBulletList = true;
                    listText = "\u2022";
                }
                else if (styleId.StartsWith("ListNumber", StringComparison.OrdinalIgnoreCase))
                {
                    isNumberedList = true;
                    listText = "1."; // placeholder; proper counter would require style-level numPr resolution
                }
            }

            // Paragraph shading
            var pShd = pPr.Element(W + "shd");
            if (pShd != null)
            {
                var pFill = pShd.Attribute(W + "fill")?.Value;
                if (!string.IsNullOrEmpty(pFill) && pFill != "auto")
                    paragraphShading = PdfColor.FromHex(pFill);
            }

            // Tab stops
            var tabsEl = pPr.Element(W + "tabs");
            if (tabsEl != null)
            {
                tabStops = tabsEl.Elements(W + "tab")
                    .Select(t => new DocxTabStop(
                        float.TryParse(t.Attribute(W + "pos")?.Value, out var pos) ? pos / 20f : 0f,
                        t.Attribute(W + "val")?.Value ?? "left",
                        t.Attribute(W + "leader")?.Value ?? "none"))
                    .OrderBy(t => t.Position)
                    .ToList();
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

        // Apply style defaults (fall back to Normal style if no explicit style)
        var effectiveStyleId = !string.IsNullOrEmpty(styleId) ? styleId : "Normal";
        if (styles.TryGetValue(effectiveStyleId, out var styleInfo))
        {
            if (fontSize == 0) fontSize = styleInfo.FontSize;
            if (!bold) bold = styleInfo.Bold;
            if (!italic) italic = styleInfo.Italic;
            if (color == null) color = styleInfo.Color;
            if (alignment == "left" && !string.IsNullOrEmpty(styleInfo.Alignment))
                alignment = styleInfo.Alignment;
            if (spacingBefore == 0) spacingBefore = styleInfo.SpacingBefore;
            if (spacingAfter < 0) spacingAfter = styleInfo.SpacingAfter;
        }

        // Read runs
        foreach (var child in pElement.Elements())
        {
            if (child.Name == W + "r")
            {
                var run = ReadRun(child, bold, italic, fontSize, color);
                if (run != null)
                {
                    if (run.IsPageBreak)
                        pageBreakAfter = true;
                    else
                        runs.Add(run);
                }

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
            bold, italic, fontSize, color, pageBreakBefore, pageBreakAfter, paragraphShading, tabStops);
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
        bool isPageBreak = false;
        var text = "";
        foreach (var child in rElement.Elements())
        {
            if (child.Name == W + "t")
                text += child.Value;
            else if (child.Name == W + "tab")
                text += "\t";
            else if (child.Name == W + "br")
            {
                var brType = child.Attribute(W + "type")?.Value;
                if (brType == "page")
                    isPageBreak = true;
                else
                    text += "\n";
            }
        }

        if (string.IsNullOrEmpty(text) && !isPageBreak)
            return null;

        return new DocxRun(text, bold, italic, fontSize, color, isPageBreak);
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

        // Detect whether the table has visible borders
        var hasBorders = false;
        var tblStyleVal = tblPr?.Element(W + "tblStyle")?.Attribute(W + "val")?.Value;
        if (!string.IsNullOrEmpty(tblStyleVal) && tblStyleVal.Contains("Grid", StringComparison.OrdinalIgnoreCase))
            hasBorders = true;
        var tblBorders = tblPr?.Element(W + "tblBorders");
        if (tblBorders != null)
        {
            foreach (var side in new[] { "top", "bottom", "left", "right", "insideH", "insideV" })
            {
                var val = tblBorders.Element(W + side)?.Attribute(W + "val")?.Value;
                if (!string.IsNullOrEmpty(val) && val != "none" && val != "nil")
                    hasBorders = true;
            }
        }
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
                foreach (var child in tc.Elements())
                {
                    if (child.Name == W + "p")
                    {
                        var para = ReadParagraph(child, styles, numbering, relationships, archive);
                        if (para != null)
                            cellParagraphs.Add(para);
                    }
                    else if (child.Name == W + "tbl")
                    {
                        // Flatten nested table: extract text from each cell as paragraphs
                        foreach (var nestedTr in child.Elements(W + "tr"))
                        {
                            foreach (var nestedTc in nestedTr.Elements(W + "tc"))
                            {
                                foreach (var nestedP in nestedTc.Elements(W + "p"))
                                {
                                    var para = ReadParagraph(nestedP, styles, numbering, relationships, archive);
                                    if (para != null)
                                        cellParagraphs.Add(para);
                                }
                            }
                        }
                    }
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

        return new DocxTable(rows, columnWidths, hasBorders);
    }

    private static DocxPageLayout? ReadPageLayout(XElement body)
    {
        var sectPr = body.Element(W + "sectPr");
        if (sectPr == null) return null;

        const float twipsToPoints = 1f / 20f;

        var pgSz = sectPr.Element(W + "pgSz");
        var pgMar = sectPr.Element(W + "pgMar");

        var pageWidth = 612f;
        var pageHeight = 792f;
        if (pgSz != null)
        {
            if (float.TryParse(pgSz.Attribute(W + "w")?.Value, out var pw)) pageWidth = pw * twipsToPoints;
            if (float.TryParse(pgSz.Attribute(W + "h")?.Value, out var ph)) pageHeight = ph * twipsToPoints;
        }

        var marginTop = 72f;
        var marginBottom = 72f;
        var marginLeft = 72f;
        var marginRight = 72f;
        if (pgMar != null)
        {
            if (float.TryParse(pgMar.Attribute(W + "top")?.Value, out var mt)) marginTop = mt * twipsToPoints;
            if (float.TryParse(pgMar.Attribute(W + "bottom")?.Value, out var mb)) marginBottom = mb * twipsToPoints;
            if (float.TryParse(pgMar.Attribute(W + "left")?.Value, out var ml)) marginLeft = ml * twipsToPoints;
            if (float.TryParse(pgMar.Attribute(W + "right")?.Value, out var mr)) marginRight = mr * twipsToPoints;
        }

        return new DocxPageLayout(pageWidth, pageHeight, marginTop, marginBottom, marginLeft, marginRight);
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

        // Read docDefaults for baseline paragraph/run properties
        float defaultFontSize = 11f;
        float defaultSpacingAfter = -1;
        float defaultSpacingBefore = 0;
        float defaultLineSpacing = 0;

        var docDefaults = doc.Descendants(W + "docDefaults").FirstOrDefault();
        if (docDefaults != null)
        {
            var rPrDefault = docDefaults.Element(W + "rPrDefault")?.Element(W + "rPr");
            if (rPrDefault != null)
            {
                var sz = rPrDefault.Element(W + "sz")?.Attribute(W + "val")?.Value;
                if (float.TryParse(sz, out var s) && s > 0)
                    defaultFontSize = s / 2f;
            }

            var pPrDefault = docDefaults.Element(W + "pPrDefault")?.Element(W + "pPr");
            if (pPrDefault != null)
            {
                var spacing = pPrDefault.Element(W + "spacing");
                if (spacing != null)
                {
                    if (int.TryParse(spacing.Attribute(W + "before")?.Value, out var sb))
                        defaultSpacingBefore = sb / 20f;
                    if (int.TryParse(spacing.Attribute(W + "line")?.Value, out var sl))
                    {
                        var lineRule = spacing.Attribute(W + "lineRule")?.Value;
                        defaultLineSpacing = (lineRule == "exact" || lineRule == "atLeast")
                            ? sl / 20f : sl / 240f;
                    }
                }
            }
        }

        foreach (var style in doc.Descendants(W + "style"))
        {
            var styleId = style.Attribute(W + "styleId")?.Value;
            if (string.IsNullOrEmpty(styleId)) continue;

            var rPr = style.Element(W + "rPr");
            var pPr = style.Element(W + "pPr");

            float fontSize = defaultFontSize;
            bool bold = false;
            bool italic = false;
            PdfColor? color = null;
            string alignment = "";
            float spacingBefore = defaultSpacingBefore;
            float spacingAfter = defaultSpacingAfter;

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

            // Heading styles get bold by default
            if (styleId.StartsWith("Heading", StringComparison.OrdinalIgnoreCase) ||
                styleId.StartsWith("heading", StringComparison.Ordinal))
            {
                bold = true;
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
internal sealed record DocxDocument(List<DocxElement> Elements, DocxPageLayout? PageLayout = null);

/// <summary>Page layout settings from sectPr.</summary>
internal sealed record DocxPageLayout(
    float PageWidth = 612,
    float PageHeight = 792,
    float MarginTop = 72,
    float MarginBottom = 72,
    float MarginLeft = 72,
    float MarginRight = 72
);

/// <summary>Base type for document elements (paragraphs, tables).</summary>
internal abstract record DocxElement;

/// <summary>Represents a paragraph in a DOCX document.</summary>
internal sealed record DocxParagraph(
    List<DocxRun> Runs,
    List<DocxImage> Images,
    string Alignment = "left",
    float SpacingBefore = 0,
    float SpacingAfter = -1,
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
    PdfColor? Color = null,
    bool HasPageBreakBefore = false,
    bool HasPageBreakAfter = false,
    PdfColor? Shading = null,
    List<DocxTabStop>? TabStops = null
) : DocxElement;

/// <summary>Represents a tab stop definition.</summary>
internal sealed record DocxTabStop(
    float Position,
    string Alignment = "left",
    string Leader = "none"
);

/// <summary>Represents a run of formatted text.</summary>
internal sealed record DocxRun(
    string Text,
    bool Bold = false,
    bool Italic = false,
    float FontSize = 0,
    PdfColor? Color = null,
    bool IsPageBreak = false
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
    List<float> ColumnWidths,
    bool HasBorders = true
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
    float SpacingAfter = -1
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
