using System.IO.Compression;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
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
    private static readonly XNamespace WPS = "http://schemas.microsoft.com/office/word/2010/wordprocessingShape";
    private static readonly XNamespace WPG = "http://schemas.microsoft.com/office/word/2010/wordprocessingGroup";

    /// <summary>
    /// Unwraps SDT (Structured Document Tag) elements by replacing them with their sdtContent children.
    /// </summary>
    private static IEnumerable<XElement> UnwrapSdt(IEnumerable<XElement> elements)
    {
        foreach (var el in elements)
        {
            if (el.Name == W + "sdt")
            {
                var content = el.Element(W + "sdtContent");
                if (content != null)
                {
                    foreach (var inner in UnwrapSdt(content.Elements()))
                        yield return inner;
                }
            }
            else
            {
                yield return el;
            }
        }
    }

    /// <summary>
    /// Reads a DOCX file and returns a structured document model.
    /// </summary>
    internal static DocxDocument Read(Stream stream)
    {
        using var archive = new ZipArchive(stream, ZipArchiveMode.Read, leaveOpen: true);

        // Read relationships to resolve image references
        var relationships = ReadRelationships(archive);

        // Read styles
        var (styles, defaultLineSpacing, defaultLineSpacingAbsolute, defaultFontName) = ReadStyles(archive);

        // Read numbering definitions (for list bullets/numbers)
        var numbering = ReadNumbering(archive);

        // Read theme colors for resolving schemeClr references
        var themeColors = ReadThemeColors(archive);

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

        foreach (var child in UnwrapSdt(body.Elements()))
        {
            if (child.Name == W + "p")
            {
                var runNodes = child.Elements(W + "r").ToList();
                var allRunsHostTextBox = runNodes.Count > 0
                    && runNodes.All(r => r.Descendants(W + "txbxContent").Any());

                // Extract text box paragraphs from anchor drawings (before the containing paragraph)
                float textBoxSpacing = 0;
                bool emittedVisibleTextBoxParagraph = false;
                foreach (var anchor in child.Descendants(WP + "anchor"))
                {
                    var txbx = anchor.Descendants(WPS + "txbx").FirstOrDefault();
                    if (txbx == null) continue;
                    var txbxContent = txbx.Element(W + "txbxContent");
                    if (txbxContent == null) continue;

                    // Check for wrapTopAndBottom (text box displaces content vertically)
                    bool isWrapTopBottom = anchor.Element(WP + "wrapTopAndBottom") != null;

                    // Read anchor vertical offset and extent height for spacing
                    float anchorOffsetPt = 0;
                    var posV = anchor.Element(WP + "positionV");
                    if (posV != null)
                    {
                        var off = posV.Element(WP + "posOffset");
                        if (off != null && long.TryParse(off.Value, out var emu))
                            anchorOffsetPt = emu / 914400f * 72f;
                    }
                    float extentHeightPt = 0;
                    var extent = anchor.Element(WP + "extent");
                    if (extent != null && long.TryParse(extent.Attribute("cy")?.Value, out var cy))
                        extentHeightPt = cy / 914400f * 72f;

                    // Read text box outline (border) from shape properties
                    DocxTextBoxBorder? textBoxBorder = null;
                    var wsp = anchor.Descendants(WPS + "wsp").FirstOrDefault();
                    if (wsp != null)
                    {
                        var spPr = wsp.Element(WPS + "spPr") ?? wsp.Element(A + "spPr");
                        var ln = spPr?.Element(A + "ln");
                        if (ln != null)
                        {
                            var lnFill = ln.Element(A + "solidFill");
                            if (lnFill != null)
                            {
                                var lnSrgb = lnFill.Element(A + "srgbClr")?.Attribute("val")?.Value;
                                var lnColor = !string.IsNullOrEmpty(lnSrgb) ? PdfColor.FromHex(lnSrgb) : new PdfColor(0, 0, 0);
                                float lnWidth = 0.5f; // default
                                if (int.TryParse(ln.Attribute("w")?.Value, out var lnW))
                                    lnWidth = lnW / 914400f * 72f;
                                if (lnWidth < 0.25f) lnWidth = 0.5f;

                                // Box position and size
                                float boxXPt = 0;
                                var posH = anchor.Element(WP + "positionH");
                                if (posH != null)
                                {
                                    var off = posH.Element(WP + "posOffset");
                                    if (off != null && long.TryParse(off.Value, out var hEmu))
                                        boxXPt = hEmu / 914400f * 72f;
                                }
                                float boxWidthPt = extent != null && long.TryParse(extent.Attribute("cx")?.Value, out var cxEmu)
                                    ? cxEmu / 914400f * 72f : 0;
                                float boxHeightPt = extentHeightPt;

                                textBoxBorder = new DocxTextBoxBorder(lnWidth, lnColor, boxXPt, boxWidthPt, boxHeightPt, anchorOffsetPt);
                            }
                        }
                    }

                    bool firstContent = true;
                    foreach (var tp in txbxContent.Elements(W + "p"))
                    {
                        var tbPara = ReadParagraph(tp, styles, numbering, relationships, archive, themeColors);
                        if (tbPara != null)
                        {
                            var hasVisibleContent = tbPara.Images.Count > 0
                                || tbPara.Shading != null
                                || tbPara.Runs.Any(r => !string.IsNullOrWhiteSpace(r.Text));

                            // Word textbox content often carries large paragraph indents that
                            // behave like internal margins, not flow indents. Keeping them as
                            // normal paragraph indents can over-constrain wrap width.
                            tbPara = tbPara with { IndentLeft = 0, IndentRight = 0, IndentFirstLine = 0 };

                            if (firstContent && hasVisibleContent && anchorOffsetPt > 0)
                                tbPara = tbPara with { SpacingBefore = tbPara.SpacingBefore + anchorOffsetPt, ForceSpacingBefore = true };
                            if (firstContent && hasVisibleContent && textBoxBorder != null)
                                tbPara = tbPara with { TextBoxBorder = textBoxBorder };
                            elements.Add(tbPara);
                            if (hasVisibleContent)
                                emittedVisibleTextBoxParagraph = true;
                            if (hasVisibleContent)
                                firstContent = false;
                        }
                    }
                    // For wrapTopAndBottom, the text box height displaces subsequent content
                    if (isWrapTopBottom)
                        textBoxSpacing += anchorOffsetPt;
                }

                var paragraph = ReadParagraph(child, styles, numbering, relationships, archive, themeColors);
                if (paragraph != null)
                {
                    // Host paragraphs for anchored textboxes may contain synthetic fallback
                    // runs that duplicate textbox text; skip these when textbox content
                    // was already emitted as standalone paragraphs.
                    if (emittedVisibleTextBoxParagraph && allRunsHostTextBox)
                        continue;

                    // Add text box displacement as SpacingBefore on the containing paragraph
                    if (textBoxSpacing > 0)
                    {
                        paragraph = paragraph with { SpacingBefore = paragraph.SpacingBefore + textBoxSpacing };
                    }
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

        // Read header/footer content
        var headerText = ReadHeaderFooter(body, relationships, archive, styles, numbering, "headerReference");
        var footerText = ReadHeaderFooter(body, relationships, archive, styles, numbering, "footerReference");
        var headerShapes = ReadHeaderFooterShapes(body, relationships, archive, "headerReference", themeColors);
        var footerShapes = ReadHeaderFooterShapes(body, relationships, archive, "footerReference", themeColors);
        var headerRuns = ReadHeaderFooterRuns(body, relationships, archive, styles, numbering, "headerReference");
        var footerRuns = ReadHeaderFooterRuns(body, relationships, archive, styles, numbering, "footerReference");

        return new DocxDocument(elements, pageLayout, headerText, footerText, headerShapes, footerShapes, headerRuns, footerRuns,
            defaultLineSpacing, defaultLineSpacingAbsolute, defaultFontName);
    }

    private static DocxParagraph? ReadParagraph(XElement pElement, Dictionary<string, DocxStyleInfo> styles,
        Dictionary<string, DocxNumberingDef> numbering, Dictionary<string, string> relationships, ZipArchive archive,
        Dictionary<string, string>? themeColors = null)
    {
        var runs = new List<DocxRun>();
        var images = new List<DocxImage>();
        var shapes = new List<DocxShape>();

        // Read paragraph properties
        var pPr = pElement.Element(W + "pPr");
        var alignment = "left";
        float spacingBefore = -1;
        float spacingAfter = -1;
        float lineSpacing = 0;
        bool lineSpacingAbsolute = false;
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
        bool caps = false;
        float fontSize = 0;
        PdfColor? color = null;
        PdfColor? paragraphShading = null;
        List<DocxTabStop>? tabStops = null;
        DocxBorders? borders = null;
        float charSpacing = 0;

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
                    lineSpacingAbsolute = lineRule == "exact" || lineRule == "atLeast";
                    lineSpacing = lineSpacingAbsolute
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
                        listText = numDef.FormatListText(listLevel);
                    }

                    // Apply numbering level indentation as fallback per-property
                    // (paragraph ind may specify hanging but not left, or vice versa)
                    var lvlDef = numDef.Levels.FirstOrDefault(l => l.Ilvl == listLevel) ?? numDef.Levels.FirstOrDefault();
                    if (lvlDef != null)
                    {
                        if (indentLeft == 0 && lvlDef.IndentLeft > 0)
                            indentLeft = lvlDef.IndentLeft;
                        if (indentFirstLine == 0 && lvlDef.Hanging > 0)
                            indentFirstLine = -lvlDef.Hanging;
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

            // Paragraph borders
            var pBdr = pPr.Element(W + "pBdr");
            if (pBdr != null)
            {
                borders = new DocxBorders(
                    Top: ReadBorderEdge(pBdr.Element(W + "top")),
                    Bottom: ReadBorderEdge(pBdr.Element(W + "bottom")),
                    Left: ReadBorderEdge(pBdr.Element(W + "left")),
                    Right: ReadBorderEdge(pBdr.Element(W + "right"))
                );
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

            // Paragraph-level run properties (pPr > rPr is for the paragraph mark character,
            // NOT default run formatting; only read font size and charSpacing from it)
            var rPr = pPr.Element(W + "rPr");
            if (rPr != null)
            {
                var sz = rPr.Element(W + "sz")?.Attribute(W + "val")?.Value;
                if (float.TryParse(sz, out var s))
                    fontSize = s / 2f; // half-points to points
                // Paragraph-level character spacing
                var spacingEl2 = rPr.Element(W + "spacing");
                if (spacingEl2 != null && int.TryParse(spacingEl2.Attribute(W + "val")?.Value, out var pcs))
                    charSpacing = pcs / 20f;
            }
        }

        // Apply style defaults (fall back to Normal style if no explicit style)
        var effectiveStyleId = !string.IsNullOrEmpty(styleId) ? styleId : "Normal";
        bool contextualSpacing = false;
        if (styles.TryGetValue(effectiveStyleId, out var styleInfo))
        {
            if (fontSize == 0) fontSize = styleInfo.FontSize;
            if (!bold) bold = styleInfo.Bold;
            if (!italic) italic = styleInfo.Italic;
            if (!caps) caps = styleInfo.Caps;
            if (color == null) color = styleInfo.Color;
            if (alignment == "left" && !string.IsNullOrEmpty(styleInfo.Alignment))
                alignment = styleInfo.Alignment;
            if (spacingBefore < 0) spacingBefore = styleInfo.SpacingBefore;
            if (spacingAfter < 0) spacingAfter = styleInfo.SpacingAfter;
            contextualSpacing = styleInfo.ContextualSpacing;
        }
        // Paragraph-level contextualSpacing overrides style
        if (pPr?.Element(W + "contextualSpacing") != null)
            contextualSpacing = true;
        if (spacingBefore < 0) spacingBefore = 0;
        if (spacingAfter < 0) spacingAfter = 0;

        // Read runs (with field code tracking)
        int fieldDepth = 0;
        bool inFieldInstr = false; // between begin and separate
        string currentFieldInstr = ""; // accumulated field instruction text
        foreach (var child in UnwrapSdt(pElement.Elements()))
        {
            if (child.Name == W + "r")
            {
                // Track field codes
                var fldChar = child.Element(W + "fldChar");
                if (fldChar != null)
                {
                    var fldType = fldChar.Attribute(W + "fldCharType")?.Value;
                    if (fldType == "begin") { fieldDepth++; inFieldInstr = true; currentFieldInstr = ""; continue; }
                    if (fldType == "separate") { inFieldInstr = false; continue; }
                    if (fldType == "end") { fieldDepth--; if (fieldDepth <= 0) { fieldDepth = 0; inFieldInstr = false; } currentFieldInstr = ""; continue; }
                }
                if (child.Element(W + "instrText") != null)
                {
                    if (inFieldInstr)
                        currentFieldInstr += child.Element(W + "instrText")!.Value;
                    continue;
                }
                // For PAGE/NUMPAGES fields, emit placeholder instead of skipping
                if (fieldDepth > 0 && !inFieldInstr)
                {
                    var instr = currentFieldInstr.Trim().ToUpperInvariant();
                    if (instr == "PAGE" || instr == "NUMPAGES")
                    {
                        var placeholder = instr == "PAGE" ? "{PAGE}" : "{NUMPAGES}";
                        var rPr = child.Element(W + "rPr");
                        var fBold = bold; var fItalic = italic; var fSize = fontSize; var fColor = color;
                        if (rPr != null)
                        {
                            if (rPr.Element(W + "b") != null) fBold = true;
                            var sz = rPr.Element(W + "sz")?.Attribute(W + "val")?.Value;
                            if (float.TryParse(sz, out var s) && s > 0) fSize = s / 2f;
                        }
                        runs.Add(new DocxRun(placeholder, fBold, fItalic, fSize, fColor));
                    }
                    continue;
                }

                var run = ReadRun(child, bold, italic, fontSize, color, caps, charSpacing);
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

                    // Check for anchor shapes (filled rectangles without image blip)
                    shapes.AddRange(ReadAnchorShapes(drawing, themeColors));
                }
            }
            else if (child.Name == W + "hyperlink")
            {
                // Extract text from hyperlink runs
                foreach (var r in child.Elements(W + "r"))
                {
                    var run = ReadRun(r, bold, italic, fontSize, color, caps, charSpacing);
                    if (run != null)
                        runs.Add(run);
                }
            }
        }

        // Detect section break (sectPr inside pPr)
        DocxPageLayout? sectionBreakLayout = null;
        var sectPr = pPr?.Element(W + "sectPr");
        if (sectPr != null)
            sectionBreakLayout = ParseSectionProperties(sectPr);

        // If paragraph has no runs and no images, represent as empty paragraph for spacing
        return new DocxParagraph(runs, images, alignment, spacingBefore, spacingAfter,
            lineSpacing, lineSpacingAbsolute, indentLeft, indentRight, indentFirstLine,
            isBulletList, isNumberedList, listLevel, listText, styleId,
            bold, italic, fontSize, color, pageBreakBefore, pageBreakAfter, paragraphShading, tabStops,
            sectionBreakLayout, borders, shapes.Count > 0 ? shapes : null,
            ContextualSpacing: contextualSpacing);
    }

    private static DocxBorderEdge? ReadBorderEdge(XElement? el)
    {
        if (el == null) return null;
        var val = el.Attribute(W + "val")?.Value;
        if (string.IsNullOrEmpty(val) || val == "none" || val == "nil") return null;
        // sz is in eighths of a point
        float width = 1f;
        if (int.TryParse(el.Attribute(W + "sz")?.Value, out var sz))
            width = sz / 8f;
        var colorHex = el.Attribute(W + "color")?.Value;
        var color = !string.IsNullOrEmpty(colorHex) && colorHex != "auto"
            ? PdfColor.FromHex(colorHex)
            : new PdfColor(0, 0, 0);
        return new DocxBorderEdge(Math.Max(0.5f, width), color);
    }

    private static DocxRun? ReadRun(XElement rElement, bool parentBold, bool parentItalic, float parentFontSize, PdfColor? parentColor, bool parentCaps = false, float parentCharSpacing = 0)
    {
        // Textbox content is parsed separately from w:txbxContent paragraphs.
        // Skip host runs that carry textbox payload to avoid duplicate/garbled flow text.
        if (rElement.Descendants(W + "txbxContent").Any())
            return null;

        var rPr = rElement.Element(W + "rPr");
        var bold = parentBold;
        var italic = parentItalic;
        var fontSize = parentFontSize;
        var color = parentColor;
        var caps = parentCaps;
        var underline = false;
        var charSpacing = parentCharSpacing;

        if (rPr != null)
        {
            if (rPr.Element(W + "b") != null) bold = true;
            if (rPr.Element(W + "i") != null) italic = true;
            if (rPr.Element(W + "caps") != null) caps = true;
            var uEl = rPr.Element(W + "u");
            if (uEl != null)
            {
                var uVal = uEl.Attribute(W + "val")?.Value;
                if (!string.IsNullOrEmpty(uVal) && uVal != "none")
                    underline = true;
            }
            var sz = rPr.Element(W + "sz")?.Attribute(W + "val")?.Value;
            if (float.TryParse(sz, out var s) && s > 0)
                fontSize = s / 2f; // half-points to points
            var runColor = ReadRunColor(rPr);
            if (runColor != null) color = runColor;
            // Character spacing (w:spacing w:val in twips)
            var spacingEl = rPr.Element(W + "spacing");
            if (spacingEl != null && int.TryParse(spacingEl.Attribute(W + "val")?.Value, out var cs))
                charSpacing = cs / 20f; // twips to points
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

        if (caps && !string.IsNullOrEmpty(text))
            text = text.ToUpperInvariant();

        return new DocxRun(text, bold, italic, fontSize, color, isPageBreak, underline, charSpacing);
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
        // Try inline images first, then fall back to anchor images
        var container = drawing.Descendants(WP + "inline").FirstOrDefault();
        var isAnchor = false;
        if (container == null)
        {
            // Fall back to anchor (floating) images
            // Only include anchors that are in front of text (behindDoc!=1) and have an actual image blip
            var anchor = drawing.Descendants(WP + "anchor").FirstOrDefault();
            if (anchor != null && anchor.Attribute("behindDoc")?.Value != "1"
                               && anchor.Descendants(A + "blip").Any())
            {
                container = anchor;
                isAnchor = true;
            }
        }
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

        // Parse source rectangle crop (percentages in 1/1000ths of percent)
        var srcRectEl = container.Descendants(A + "srcRect").FirstOrDefault();
        float cropL = 0, cropT = 0, cropR = 0, cropB = 0;
        if (srcRectEl != null)
        {
            float.TryParse(srcRectEl.Attribute("l")?.Value, out cropL);
            float.TryParse(srcRectEl.Attribute("t")?.Value, out cropT);
            float.TryParse(srcRectEl.Attribute("r")?.Value, out cropR);
            float.TryParse(srcRectEl.Attribute("b")?.Value, out cropB);
            // Convert from 1/1000ths of percent to fraction (0..1)
            cropL /= 100000f; cropT /= 100000f; cropR /= 100000f; cropB /= 100000f;
        }
        var hasCrop = cropL > 0 || cropT > 0 || cropR > 0 || cropB > 0;

        // Convert vector signatures (EMF/WMF) to PNG so they can be embedded in PDF.
        if (ext is "emf" or "wmf")
        {
            var converted = TryConvertMetafileToPng(data, widthEmu, heightEmu, cropL, cropT, cropR, cropB);
            if (converted != null)
            {
                data = converted;
                ext = "png";
            }
            else
            {
                return null;
            }
        }
        else if (hasCrop && OperatingSystem.IsWindows())
        {
            var cropped = TryCropImagePng(data, cropL, cropT, cropR, cropB);
            if (cropped != null)
            {
                data = cropped;
                ext = "png";
            }
        }

        // Read anchor position offsets
        long offsetXEmu = 0, offsetYEmu = 0;
        if (isAnchor)
        {
            var posH = container.Element(WP + "positionH");
            var posV = container.Element(WP + "positionV");
            if (posH != null)
            {
                var off = posH.Element(WP + "posOffset");
                if (off != null) long.TryParse(off.Value, out offsetXEmu);
            }
            if (posV != null)
            {
                var off = posV.Element(WP + "posOffset");
                if (off != null) long.TryParse(off.Value, out offsetYEmu);
            }
        }

        return new DocxImage(data, ext, widthEmu, heightEmu, isAnchor, offsetXEmu, offsetYEmu);
    }

    private static byte[]? TryConvertMetafileToPng(byte[] sourceBytes, long widthEmu, long heightEmu,
        float cropL = 0, float cropT = 0, float cropR = 0, float cropB = 0)
    {
        if (!OperatingSystem.IsWindows())
            return null;

        try
        {
            // Keep source stream alive for Metafile lifetime.
            using var srcStream = new MemoryStream(sourceBytes, writable: false);
            using var meta = new Metafile(srcStream);

            var hasCrop = cropL > 0 || cropT > 0 || cropR > 0 || cropB > 0;

            // When srcRect crop is present, rasterize at the EMF's native aspect ratio
            // then crop to the specified region.
            double aspect;
            if (hasCrop && meta.Width > 0 && meta.Height > 0)
                aspect = (double)meta.Width / meta.Height;
            else
                aspect = widthEmu > 0 && heightEmu > 0
                    ? (double)widthEmu / heightEmu
                    : (meta.Width > 0 && meta.Height > 0 ? (double)meta.Width / meta.Height : 1.0);

            var targetHeight = 512;
            var targetWidth = (int)Math.Round(targetHeight * aspect);
            targetWidth = Math.Clamp(targetWidth, 32, 4096);
            targetHeight = Math.Clamp(targetHeight, 32, 4096);

            using var bmp = new Bitmap(targetWidth, targetHeight, PixelFormat.Format32bppArgb);
            using (var g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.White);
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                g.CompositingQuality = CompositingQuality.HighQuality;
                g.DrawImage(meta, new Rectangle(0, 0, targetWidth, targetHeight));
            }

            // Apply srcRect crop if specified
            if (hasCrop)
            {
                var cx = (int)Math.Round(targetWidth * cropL);
                var cy = (int)Math.Round(targetHeight * cropT);
                var cw = (int)Math.Round(targetWidth * (1 - cropL - cropR));
                var ch = (int)Math.Round(targetHeight * (1 - cropT - cropB));
                cw = Math.Max(1, cw); ch = Math.Max(1, ch);
                using var cropped = new Bitmap(cw, ch, PixelFormat.Format32bppArgb);
                using (var g2 = Graphics.FromImage(cropped))
                {
                    g2.Clear(Color.White);
                    g2.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    g2.DrawImage(bmp, new Rectangle(0, 0, cw, ch), new Rectangle(cx, cy, cw, ch), GraphicsUnit.Pixel);
                }
                using var outStream = new MemoryStream();
                cropped.Save(outStream, ImageFormat.Png);
                return outStream.ToArray();
            }

            using var outStream2 = new MemoryStream();
            bmp.Save(outStream2, ImageFormat.Png);
            return outStream2.ToArray();
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Crops a raster image (JPEG/PNG) according to srcRect percentages and returns PNG bytes.
    /// </summary>
    private static byte[]? TryCropImagePng(byte[] imageBytes, float cropL, float cropT, float cropR, float cropB)
    {
        try
        {
            using var ms = new MemoryStream(imageBytes, writable: false);
            using var img = Image.FromStream(ms);
            var cx = (int)Math.Round(img.Width * cropL);
            var cy = (int)Math.Round(img.Height * cropT);
            var cw = (int)Math.Round(img.Width * (1 - cropL - cropR));
            var ch = (int)Math.Round(img.Height * (1 - cropT - cropB));
            cw = Math.Max(1, cw); ch = Math.Max(1, ch);
            using var cropped = new Bitmap(cw, ch, PixelFormat.Format32bppArgb);
            using (var g = Graphics.FromImage(cropped))
            {
                g.Clear(Color.White);
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.DrawImage(img, new Rectangle(0, 0, cw, ch), new Rectangle(cx, cy, cw, ch), GraphicsUnit.Pixel);
            }
            using var outStream = new MemoryStream();
            cropped.Save(outStream, ImageFormat.Png);
            return outStream.ToArray();
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Reads anchor shapes (filled rectangles) from drawing elements.
    /// Handles both simple shapes and group shapes (wpg:wgp).
    /// </summary>
    private static List<DocxShape> ReadAnchorShapes(XElement drawing, Dictionary<string, string>? themeColors)
    {
        var result = new List<DocxShape>();
        var anchor = drawing.Descendants(WP + "anchor").FirstOrDefault();
        if (anchor == null) return result;

        // Only interested in behind-doc shapes (background fills)
        if (anchor.Attribute("behindDoc")?.Value != "1") return result;

        // Skip if it has a blip (it's an image, not a shape)
        if (anchor.Descendants(A + "blip").Any()) return result;

        // Get anchor position
        long anchorOffsetX = 0, anchorOffsetY = 0;
        var posH = anchor.Element(WP + "positionH");
        var posV = anchor.Element(WP + "positionV");
        if (posH != null)
        {
            var off = posH.Element(WP + "posOffset");
            if (off != null) long.TryParse(off.Value, out anchorOffsetX);
        }
        if (posV != null)
        {
            var off = posV.Element(WP + "posOffset");
            if (off != null) long.TryParse(off.Value, out anchorOffsetY);
        }

        // Check for group shape (wpg:wgp)
        var grpSp = anchor.Descendants(WPG + "wgp").FirstOrDefault();
        if (grpSp != null)
        {
            // Read group coordinate mapping
            var grpSpPr = grpSp.Element(WPG + "grpSpPr");
            var grpXfrm = grpSpPr?.Element(A + "xfrm");
            long grpExtCx = 1, grpExtCy = 1, chOffX = 0, chOffY = 0, chExtCx = 1, chExtCy = 1;
            if (grpXfrm != null)
            {
                var ext = grpXfrm.Element(A + "ext");
                if (ext != null)
                {
                    long.TryParse(ext.Attribute("cx")?.Value, out grpExtCx);
                    long.TryParse(ext.Attribute("cy")?.Value, out grpExtCy);
                }
                var chOff = grpXfrm.Element(A + "chOff");
                if (chOff != null)
                {
                    long.TryParse(chOff.Attribute("x")?.Value, out chOffX);
                    long.TryParse(chOff.Attribute("y")?.Value, out chOffY);
                }
                var chExt = grpXfrm.Element(A + "chExt");
                if (chExt != null)
                {
                    long.TryParse(chExt.Attribute("cx")?.Value, out chExtCx);
                    long.TryParse(chExt.Attribute("cy")?.Value, out chExtCy);
                }
            }
            if (chExtCx == 0) chExtCx = 1;
            if (chExtCy == 0) chExtCy = 1;

            // Read group-level fill for grpFill inheritance
            PdfColor? grpFillColor = null;
            float grpFillAlpha = 1f;
            var grpSolidFill = grpSpPr?.Element(A + "solidFill");
            if (grpSolidFill != null)
            {
                (grpFillColor, grpFillAlpha) = ResolveSolidFill(grpSolidFill, themeColors);
            }

            // Process each child shape in the group
            foreach (var childWsp in grpSp.Elements(WPS + "wsp"))
            {
                var childSpPr = childWsp.Element(WPS + "spPr") ?? childWsp.Element(A + "spPr");
                if (childSpPr == null) continue;
                if (childSpPr.Element(A + "noFill") != null) continue;

                PdfColor? fillColor;
                float alpha;
                var childFill = childSpPr.Element(A + "solidFill");
                if (childFill != null)
                {
                    (fillColor, alpha) = ResolveSolidFill(childFill, themeColors);
                }
                else if (childSpPr.Element(A + "grpFill") != null && grpFillColor != null)
                {
                    fillColor = grpFillColor;
                    alpha = grpFillAlpha;
                }
                else
                {
                    continue;
                }
                if (fillColor == null) continue;

                // Get child shape position/size in child coordinate space
                var childXfrm = childSpPr.Element(A + "xfrm");
                if (childXfrm == null) continue;
                long childOffX = 0, childOffY = 0, childCx = 0, childCy = 0;
                var cOff = childXfrm.Element(A + "off");
                if (cOff != null)
                {
                    long.TryParse(cOff.Attribute("x")?.Value, out childOffX);
                    long.TryParse(cOff.Attribute("y")?.Value, out childOffY);
                }
                var cExt = childXfrm.Element(A + "ext");
                if (cExt != null)
                {
                    long.TryParse(cExt.Attribute("cx")?.Value, out childCx);
                    long.TryParse(cExt.Attribute("cy")?.Value, out childCy);
                }

                var (childPresetGeom, childFrameThicknessRatio, childCustomPaths) =
                    ReadShapeGeometry(childSpPr, childCx, childCy);

                // Map child coordinates to page-relative EMU
                var pageX = anchorOffsetX + (childOffX - chOffX) * grpExtCx / chExtCx;
                var pageY = anchorOffsetY + (childOffY - chOffY) * grpExtCy / chExtCy;
                var pageW = childCx * grpExtCx / chExtCx;
                var pageH = childCy * grpExtCy / chExtCy;

                result.Add(new DocxShape(pageW, pageH, pageX, pageY, fillColor.Value, alpha,
                    childPresetGeom, childFrameThicknessRatio, childCustomPaths));
            }
            return result;
        }

        // Fall back to single-shape handling (non-group)
        var spPr = anchor.Descendants(WPS + "spPr").FirstOrDefault()
              ?? anchor.Descendants(A + "spPr").FirstOrDefault();
        if (spPr == null) return result;
        if (spPr.Element(A + "noFill") != null) return result;
        var solidFill = spPr.Element(A + "solidFill");
        if (solidFill == null) return result;

        var (singleColor, singleAlpha) = ResolveSolidFill(solidFill, themeColors);
        if (singleColor == null) return result;

        // Get extent (size)
        var extent = anchor.Element(WP + "extent");
        long widthEmu = 0, heightEmu = 0;
        if (extent != null)
        {
            long.TryParse(extent.Attribute("cx")?.Value, out widthEmu);
            long.TryParse(extent.Attribute("cy")?.Value, out heightEmu);
        }

        var (presetGeom, frameThicknessRatio, customPaths) =
            ReadShapeGeometry(spPr, widthEmu, heightEmu);

        result.Add(new DocxShape(widthEmu, heightEmu, anchorOffsetX, anchorOffsetY, singleColor.Value, singleAlpha,
            presetGeom, frameThicknessRatio, customPaths));
        return result;
    }

    private static (string? PresetGeometry, float FrameThicknessRatio, List<DocxCustomPath>? CustomPaths)
        ReadShapeGeometry(XElement spPr, long widthEmu, long heightEmu)
    {
        string? presetGeom = null;
        float frameThicknessRatio = 0.125f;
        List<DocxCustomPath>? customPaths = null;

        var prstGeom = spPr.Element(A + "prstGeom");
        if (prstGeom != null)
        {
            presetGeom = prstGeom.Attribute("prst")?.Value;
            if (presetGeom == "frame")
            {
                var avLst = prstGeom.Element(A + "avLst");
                var gd = avLst?.Elements(A + "gd")
                    .FirstOrDefault(g => g.Attribute("name")?.Value == "adj1");
                if (gd != null)
                {
                    var fmla = gd.Attribute("fmla")?.Value;
                    if (fmla != null && fmla.StartsWith("val ") &&
                        int.TryParse(fmla.AsSpan(4), out var v))
                        frameThicknessRatio = v / 100000f;
                }
            }
            return (presetGeom, frameThicknessRatio, customPaths);
        }

        var custGeom = spPr.Element(A + "custGeom");
        if (custGeom != null)
        {
            customPaths = ParseCustomGeometryPaths(custGeom, widthEmu, heightEmu);
            if (customPaths is { Count: > 0 })
                presetGeom = "custom";
        }

        return (presetGeom, frameThicknessRatio, customPaths);
    }

    private static List<DocxCustomPath>? ParseCustomGeometryPaths(XElement custGeom, long widthEmu, long heightEmu)
    {
        var pathList = custGeom.Element(A + "pathLst");
        if (pathList == null) return null;

        var result = new List<DocxCustomPath>();
        foreach (var path in pathList.Elements(A + "path"))
        {
            long pathW = widthEmu > 0 ? widthEmu : 1;
            long pathH = heightEmu > 0 ? heightEmu : 1;
            if (long.TryParse(path.Attribute("w")?.Value, out var parsedW) && parsedW > 0)
                pathW = parsedW;
            if (long.TryParse(path.Attribute("h")?.Value, out var parsedH) && parsedH > 0)
                pathH = parsedH;

            var vars = new Dictionary<string, double>(StringComparer.Ordinal)
            {
                ["w"] = pathW,
                ["h"] = pathH,
            };

            var gdList = custGeom.Element(A + "gdLst");
            if (gdList != null)
            {
                foreach (var gd in gdList.Elements(A + "gd"))
                {
                    var name = gd.Attribute("name")?.Value;
                    var fmla = gd.Attribute("fmla")?.Value;
                    if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(fmla))
                        continue;
                    vars[name] = EvaluateGuideFormula(fmla, vars);
                }
            }

            var subpaths = new List<List<DocxPolygonPoint>>();
            var current = new List<DocxPolygonPoint>();
            foreach (var cmd in path.Elements())
            {
                if (cmd.Name == A + "moveTo")
                {
                    if (current.Count >= 3)
                        subpaths.Add(current);
                    current = new List<DocxPolygonPoint>();
                    var pt = cmd.Element(A + "pt");
                    if (TryReadPathPoint(pt, vars, pathW, pathH, out var p))
                        current.Add(p);
                }
                else if (cmd.Name == A + "lnTo")
                {
                    var pt = cmd.Element(A + "pt");
                    if (TryReadPathPoint(pt, vars, pathW, pathH, out var p))
                        current.Add(p);
                }
                else if (cmd.Name == A + "arcTo")
                {
                    // Approximate arcTo segments into polyline points.
                    AppendArcToPoints(current, cmd, vars, pathW, pathH);
                }
                else if (cmd.Name == A + "quadBezTo")
                {
                    // Approximate quadratic Bezier segments into polyline points.
                    AppendQuadraticBezierPoints(current, cmd, vars, pathW, pathH);
                }
                else if (cmd.Name == A + "cubicBezTo")
                {
                    // Approximate cubic Bezier segments into polyline points.
                    AppendCubicBezierPoints(current, cmd, vars, pathW, pathH);
                }
                else if (cmd.Name == A + "close")
                {
                    if (current.Count >= 3)
                    {
                        subpaths.Add(current);
                        current = new List<DocxPolygonPoint>();
                    }
                }
            }

            if (current.Count >= 3)
                subpaths.Add(current);

            if (subpaths.Count > 0)
            {
                // Always use even-odd fill for custGeom paths. Complex single-subpath
                // polygons that self-intersect (e.g. decorative arcs) require even-odd
                // to render correctly instead of filling the entire bounding area solid.
                result.Add(new DocxCustomPath(subpaths, UseEvenOddFill: true));
            }
        }

        return result.Count > 0 ? result : null;
    }

    private static bool TryReadPathPoint(XElement? pt, Dictionary<string, double> vars, long pathW, long pathH,
        out DocxPolygonPoint point)
    {
        point = new DocxPolygonPoint(0, 0);
        if (pt == null) return false;

        var xToken = pt.Attribute("x")?.Value;
        var yToken = pt.Attribute("y")?.Value;
        if (string.IsNullOrEmpty(xToken) || string.IsNullOrEmpty(yToken))
            return false;

        var x = ResolveGuideToken(xToken, vars);
        var y = ResolveGuideToken(yToken, vars);
        if (pathW <= 0 || pathH <= 0) return false;

        var xNorm = (float)(x / pathW);
        var yNorm = (float)(y / pathH);
        point = new DocxPolygonPoint(
            Math.Clamp(xNorm, -0.25f, 1.25f),
            Math.Clamp(yNorm, -0.25f, 1.25f));
        return true;
    }

    private static void AppendArcToPoints(List<DocxPolygonPoint> current, XElement arcTo,
        Dictionary<string, double> vars, long pathW, long pathH)
    {
        if (current.Count == 0 || pathW <= 0 || pathH <= 0)
            return;

        var wR = ResolveGuideToken(arcTo.Attribute("wR")?.Value ?? "0", vars);
        var hR = ResolveGuideToken(arcTo.Attribute("hR")?.Value ?? "0", vars);
        var stAng = ResolveGuideToken(arcTo.Attribute("stAng")?.Value ?? "0", vars);
        var swAng = ResolveGuideToken(arcTo.Attribute("swAng")?.Value ?? "0", vars);

        if (Math.Abs(wR) < 0.0001 || Math.Abs(hR) < 0.0001 || Math.Abs(swAng) < 0.0001)
            return;

        var startRad = stAng / 60000d * Math.PI / 180d;
        var sweepRad = swAng / 60000d * Math.PI / 180d;

        // In DrawingML arcTo, the current point is on the ellipse at start angle.
        var startX = current[^1].X * pathW;
        var startY = current[^1].Y * pathH;
        var centerX = startX - wR * Math.Cos(startRad);
        var centerY = startY - hR * Math.Sin(startRad);

        var steps = Math.Clamp((int)Math.Ceiling(Math.Abs(sweepRad) / (Math.PI / 16d)), 4, 96);
        for (var i = 1; i <= steps; i++)
        {
            var t = startRad + sweepRad * i / steps;
            var x = centerX + wR * Math.Cos(t);
            var y = centerY + hR * Math.Sin(t);
            AppendNormalizedPoint(current, x, y, pathW, pathH);
        }
    }

    private static void AppendQuadraticBezierPoints(List<DocxPolygonPoint> current, XElement quadBezTo,
        Dictionary<string, double> vars, long pathW, long pathH)
    {
        if (current.Count == 0 || pathW <= 0 || pathH <= 0)
            return;

        var pts = quadBezTo.Elements(A + "pt").ToList();
        if (pts.Count < 2)
            return;

        if (!TryReadPathPoint(pts[0], vars, pathW, pathH, out var c1) ||
            !TryReadPathPoint(pts[1], vars, pathW, pathH, out var p2))
            return;

        var p0x = current[^1].X * pathW;
        var p0y = current[^1].Y * pathH;
        var p1x = c1.X * pathW;
        var p1y = c1.Y * pathH;
        var p2x = p2.X * pathW;
        var p2y = p2.Y * pathH;

        const int steps = 12;
        for (var i = 1; i <= steps; i++)
        {
            var t = i / (double)steps;
            var mt = 1d - t;
            var x = mt * mt * p0x + 2d * mt * t * p1x + t * t * p2x;
            var y = mt * mt * p0y + 2d * mt * t * p1y + t * t * p2y;
            AppendNormalizedPoint(current, x, y, pathW, pathH);
        }
    }

    private static void AppendCubicBezierPoints(List<DocxPolygonPoint> current, XElement cubicBezTo,
        Dictionary<string, double> vars, long pathW, long pathH)
    {
        if (current.Count == 0 || pathW <= 0 || pathH <= 0)
            return;

        var pts = cubicBezTo.Elements(A + "pt").ToList();
        if (pts.Count < 3)
            return;

        if (!TryReadPathPoint(pts[0], vars, pathW, pathH, out var c1) ||
            !TryReadPathPoint(pts[1], vars, pathW, pathH, out var c2) ||
            !TryReadPathPoint(pts[2], vars, pathW, pathH, out var p3))
            return;

        var p0x = current[^1].X * pathW;
        var p0y = current[^1].Y * pathH;
        var p1x = c1.X * pathW;
        var p1y = c1.Y * pathH;
        var p2x = c2.X * pathW;
        var p2y = c2.Y * pathH;
        var p3x = p3.X * pathW;
        var p3y = p3.Y * pathH;

        const int steps = 16;
        for (var i = 1; i <= steps; i++)
        {
            var t = i / (double)steps;
            var mt = 1d - t;
            var x = mt * mt * mt * p0x
                + 3d * mt * mt * t * p1x
                + 3d * mt * t * t * p2x
                + t * t * t * p3x;
            var y = mt * mt * mt * p0y
                + 3d * mt * mt * t * p1y
                + 3d * mt * t * t * p2y
                + t * t * t * p3y;
            AppendNormalizedPoint(current, x, y, pathW, pathH);
        }
    }

    private static void AppendNormalizedPoint(List<DocxPolygonPoint> current, double x, double y, long pathW, long pathH)
    {
        if (pathW <= 0 || pathH <= 0)
            return;

        var nx = (float)(x / pathW);
        var ny = (float)(y / pathH);
        nx = Math.Clamp(nx, -0.25f, 1.25f);
        ny = Math.Clamp(ny, -0.25f, 1.25f);

        if (current.Count > 0)
        {
            var last = current[^1];
            if (Math.Abs(last.X - nx) < 0.0001f && Math.Abs(last.Y - ny) < 0.0001f)
                return;
        }

        current.Add(new DocxPolygonPoint(nx, ny));
    }

    private static double EvaluateGuideFormula(string formula, Dictionary<string, double> vars)
    {
        var tokens = formula.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (tokens.Length == 0) return 0;

        if (tokens[0] == "val" && tokens.Length >= 2)
            return ResolveGuideToken(tokens[1], vars);

        if (tokens[0] == "*/" && tokens.Length >= 4)
        {
            var a = ResolveGuideToken(tokens[1], vars);
            var b = ResolveGuideToken(tokens[2], vars);
            var c = ResolveGuideToken(tokens[3], vars);
            return Math.Abs(c) < 0.0001 ? 0 : (a * b / c);
        }

        if (tokens[0] == "+-" && tokens.Length >= 4)
        {
            var a = ResolveGuideToken(tokens[1], vars);
            var b = ResolveGuideToken(tokens[2], vars);
            var c = ResolveGuideToken(tokens[3], vars);
            return a + b - c;
        }

        return tokens.Length == 1 ? ResolveGuideToken(tokens[0], vars) : 0;
    }

    private static double ResolveGuideToken(string token, Dictionary<string, double> vars)
    {
        if (double.TryParse(token, out var num)) return num;
        if (vars.TryGetValue(token, out var value)) return value;
        return 0;
    }

    /// <summary>
    /// Resolves a solidFill element to a PdfColor and alpha value.
    /// </summary>
    private static (PdfColor? Color, float Alpha) ResolveSolidFill(XElement solidFill, Dictionary<string, string>? themeColors)
    {
        PdfColor? fillColor = null;
        float alpha = 1f;

        var srgbClr = solidFill.Element(A + "srgbClr");
        if (srgbClr != null)
        {
            fillColor = PdfColor.FromHex(srgbClr.Attribute("val")?.Value ?? "000000");
            var alphaEl = srgbClr.Element(A + "alpha");
            if (alphaEl != null && int.TryParse(alphaEl.Attribute("val")?.Value, out var a))
                alpha = a / 100000f;
        }

        var schemeClr = solidFill.Element(A + "schemeClr");
        if (schemeClr != null && themeColors != null)
        {
            var schemeVal = schemeClr.Attribute("val")?.Value;
            var themeKey = schemeVal switch
            {
                "tx2" or "dk2" => "dk2",
                "tx1" or "dk1" => "dk1",
                "bg1" or "lt1" => "lt1",
                "bg2" or "lt2" => "lt2",
                "accent1" => "accent1",
                "accent2" => "accent2",
                "accent3" => "accent3",
                "accent4" => "accent4",
                "accent5" => "accent5",
                "accent6" => "accent6",
                _ => schemeVal
            };
            if (themeKey != null && themeColors.TryGetValue(themeKey, out var hex))
            {
                fillColor = PdfColor.FromHex(hex);
                var lumMod = schemeClr.Element(A + "lumMod");
                if (lumMod != null && int.TryParse(lumMod.Attribute("val")?.Value, out var lm))
                {
                    var factor = lm / 100000f;
                    var fc = fillColor.Value;
                    fillColor = new PdfColor(fc.R * factor, fc.G * factor, fc.B * factor);
                }
                var lumOff = schemeClr.Element(A + "lumOff");
                if (lumOff != null && int.TryParse(lumOff.Attribute("val")?.Value, out var lo))
                {
                    var offset = lo / 100000f;
                    var fc = fillColor.Value;
                    fillColor = new PdfColor(
                        Math.Min(1f, fc.R + offset),
                        Math.Min(1f, fc.G + offset),
                        Math.Min(1f, fc.B + offset));
                }
            }
            var alphaEl = schemeClr.Element(A + "alpha");
            if (alphaEl != null && int.TryParse(alphaEl.Attribute("val")?.Value, out var a))
                alpha = a / 100000f;
        }

        return (fillColor, alpha);
    }

    /// <summary>
    /// Reads theme colors from theme1.xml.
    /// </summary>
    private static Dictionary<string, string> ReadThemeColors(ZipArchive archive)
    {
        var colors = new Dictionary<string, string>();
        var entry = archive.GetEntry("word/theme/theme1.xml");
        if (entry == null) return colors;

        using var stream = entry.Open();
        var doc = XDocument.Load(stream);

        var colorScheme = doc.Descendants(A + "clrScheme").FirstOrDefault();
        if (colorScheme == null) return colors;

        foreach (var el in colorScheme.Elements())
        {
            var name = el.Name.LocalName; // dk1, lt1, dk2, lt2, accent1..6, hlink, folHlink
            var srgb = el.Element(A + "srgbClr");
            if (srgb != null)
            {
                colors[name] = srgb.Attribute("val")?.Value ?? "";
            }
            else
            {
                var sysClr = el.Element(A + "sysClr");
                if (sysClr != null)
                    colors[name] = sysClr.Attribute("lastClr")?.Value ?? "000000";
            }
        }

        return colors;
    }

    private static DocxTable? ReadTable(XElement tblElement, Dictionary<string, DocxStyleInfo> styles,
        Dictionary<string, DocxNumberingDef> numbering, Dictionary<string, string> relationships, ZipArchive archive)
    {
        var rows = new List<DocxTableRow>();

        // Read table properties (borders, column widths)
        var tblPr = tblElement.Element(W + "tblPr");
        var tblGrid = tblElement.Element(W + "tblGrid");
        var columnWidths = new List<float>();

        // Read table-level cell margins
        float cellMarginLeft = 5.4f, cellMarginRight = 5.4f, cellMarginTop = 0f, cellMarginBottom = 0f;
        var tblCellMar = tblPr?.Element(W + "tblCellMar");
        if (tblCellMar != null)
        {
            if (int.TryParse(tblCellMar.Element(W + "left")?.Attribute(W + "w")?.Value, out var ml))
                cellMarginLeft = ml / 20f;
            if (int.TryParse(tblCellMar.Element(W + "right")?.Attribute(W + "w")?.Value, out var mr))
                cellMarginRight = mr / 20f;
            if (int.TryParse(tblCellMar.Element(W + "top")?.Attribute(W + "w")?.Value, out var mt))
                cellMarginTop = mt / 20f;
            if (int.TryParse(tblCellMar.Element(W + "bottom")?.Attribute(W + "w")?.Value, out var mb))
                cellMarginBottom = mb / 20f;
        }

        // Detect whether the table has visible borders
        var hasBorders = false;
        var tblStyleVal = tblPr?.Element(W + "tblStyle")?.Attribute(W + "val")?.Value;
        if (!string.IsNullOrEmpty(tblStyleVal) && tblStyleVal.Contains("Grid", StringComparison.OrdinalIgnoreCase))
            hasBorders = true;
        var tblBorders = tblPr?.Element(W + "tblBorders");
        if (tblBorders != null)
        {
            // Explicit tblBorders overrides style-level default
            hasBorders = false;
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

            // Read row height from trPr
            float rowHeight = 0;
            var trPr = tr.Element(W + "trPr");
            var trHeightEl = trPr?.Element(W + "trHeight");
            if (trHeightEl != null && int.TryParse(trHeightEl.Attribute(W + "val")?.Value, out var rh))
                rowHeight = rh / 20f; // twips to points

            foreach (var tc in tr.Elements(W + "tc"))
            {
                var cellParagraphs = new List<DocxParagraph>();
                foreach (var child in UnwrapSdt(tc.Elements()))
                {
                    if (child.Name == W + "p")
                    {
                        var para = ReadParagraph(child, styles, numbering, relationships, archive);
                        if (para != null)
                            cellParagraphs.Add(para);
                    }
                    else if (child.Name == W + "tbl")
                    {
                        // Flatten nested table: join each row's cell text into a single paragraph
                        foreach (var nestedTr in child.Elements(W + "tr"))
                        {
                            var rowRuns = new List<DocxRun>();
                            foreach (var nestedTc in nestedTr.Elements(W + "tc"))
                            {
                                foreach (var nestedP in nestedTc.Elements(W + "p"))
                                {
                                    var para = ReadParagraph(nestedP, styles, numbering, relationships, archive);
                                    if (para != null)
                                    {
                                        if (rowRuns.Count > 0)
                                            rowRuns.Add(new DocxRun(" "));
                                        rowRuns.AddRange(para.Runs);
                                    }
                                }
                            }
                            if (rowRuns.Count > 0)
                                cellParagraphs.Add(new DocxParagraph(rowRuns, []));
                        }
                    }
                }

                // Read cell properties
                var tcPr = tc.Element(W + "tcPr");
                float cellWidth = 0;
                int gridSpan = 1;
                PdfColor? shading = null;
                DocxBorders? cellBorders = null;
                bool isVMergeContinue = false;
                bool isVMergeRestart = false;
                string verticalAlignment = "top";

                if (tcPr != null)
                {
                    // Detect vertical merge continuation
                    var vMergeEl = tcPr.Element(W + "vMerge");
                    if (vMergeEl != null)
                    {
                        var vMergeVal = vMergeEl.Attribute(W + "val")?.Value;
                        // vMerge with no val or val!="restart" means continuation
                        if (string.IsNullOrEmpty(vMergeVal) || vMergeVal != "restart")
                            isVMergeContinue = true;
                        else
                            isVMergeRestart = true;
                    }
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

                    var tcBorders = tcPr.Element(W + "tcBorders");
                    if (tcBorders != null)
                    {
                        cellBorders = new DocxBorders(
                            Top: ReadBorderEdge(tcBorders.Element(W + "top")),
                            Bottom: ReadBorderEdge(tcBorders.Element(W + "bottom")),
                            Left: ReadBorderEdge(tcBorders.Element(W + "left")),
                            Right: ReadBorderEdge(tcBorders.Element(W + "right"))
                        );
                    }

                    var vAlignEl = tcPr.Element(W + "vAlign");
                    if (vAlignEl != null)
                    {
                        var va = vAlignEl.Attribute(W + "val")?.Value;
                        if (!string.IsNullOrEmpty(va))
                            verticalAlignment = va;
                    }
                }

                cells.Add(new DocxTableCell(cellParagraphs, cellWidth, gridSpan, shading, cellBorders, isVMergeContinue, isVMergeRestart, verticalAlignment));
            }
            rows.Add(new DocxTableRow(cells, rowHeight));
        }

        return new DocxTable(rows, columnWidths, hasBorders, cellMarginLeft, cellMarginRight, cellMarginTop, cellMarginBottom);
    }

    private static DocxPageLayout? ReadPageLayout(XElement body)
    {
        var sectPr = body.Element(W + "sectPr");
        if (sectPr == null) return null;
        return ParseSectionProperties(sectPr);
    }

    private static string? ReadHeaderFooter(XElement body, Dictionary<string, string> relationships,
        ZipArchive archive, Dictionary<string, DocxStyleInfo> styles, Dictionary<string, DocxNumberingDef> numbering,
        string refElementName)
    {
        var sectPr = body.Element(W + "sectPr");
        if (sectPr == null) return null;

        var hfRef = sectPr.Element(W + refElementName);
        if (hfRef == null) return null;

        var rId = hfRef.Attribute(R + "id")?.Value;
        if (string.IsNullOrEmpty(rId) || !relationships.TryGetValue(rId, out var target))
            return null;

        var path = target.StartsWith("/") ? target.TrimStart('/') : "word/" + target;
        var entry = archive.GetEntry(path);
        if (entry == null) return null;

        using var stream = entry.Open();
        var doc = XDocument.Load(stream);

        var texts = new List<string>();
        foreach (var p in doc.Descendants(W + "p"))
        {
            var para = ReadParagraph(p, styles, numbering, relationships, archive);
            if (para != null)
            {
                var text = string.Concat(para.Runs.Select(r => r.Text));
                if (!string.IsNullOrEmpty(text))
                    texts.Add(text);
            }
        }

        return texts.Count > 0 ? string.Join("\n", texts) : null;
    }

    private static List<DocxRun>? ReadHeaderFooterRuns(XElement body, Dictionary<string, string> relationships,
        ZipArchive archive, Dictionary<string, DocxStyleInfo> styles, Dictionary<string, DocxNumberingDef> numbering,
        string refElementName)
    {
        var sectPr = body.Element(W + "sectPr");
        if (sectPr == null) return null;

        var hfRef = sectPr.Element(W + refElementName);
        if (hfRef == null) return null;

        var rId = hfRef.Attribute(R + "id")?.Value;
        if (string.IsNullOrEmpty(rId) || !relationships.TryGetValue(rId, out var target))
            return null;

        var path = target.StartsWith("/") ? target.TrimStart('/') : "word/" + target;
        var entry = archive.GetEntry(path);
        if (entry == null) return null;

        using var stream = entry.Open();
        var doc = XDocument.Load(stream);

        var allRuns = new List<DocxRun>();
        foreach (var p in doc.Descendants(W + "p"))
        {
            var para = ReadParagraph(p, styles, numbering, relationships, archive);
            if (para != null && para.Runs.Count > 0)
            {
                if (allRuns.Count > 0)
                    allRuns.Add(new DocxRun("\n", false, false, 0, null));
                allRuns.AddRange(para.Runs);
            }
        }

        return allRuns.Count > 0 ? allRuns : null;
    }

    private static List<DocxShape> ReadHeaderFooterShapes(
        XElement body,
        Dictionary<string, string> relationships,
        ZipArchive archive,
        string refElementName,
        Dictionary<string, string>? themeColors = null)
    {
        var sectPr = body.Element(W + "sectPr");
        if (sectPr == null) return [];

        var hfRef = sectPr.Element(W + refElementName);
        if (hfRef == null) return [];

        var rId = hfRef.Attribute(R + "id")?.Value;
        if (string.IsNullOrEmpty(rId) || !relationships.TryGetValue(rId, out var target))
            return [];

        var path = target.StartsWith("/") ? target.TrimStart('/') : "word/" + target;
        var entry = archive.GetEntry(path);
        if (entry == null) return [];

        using var stream = entry.Open();
        var doc = XDocument.Load(stream);
        var shapes = new List<DocxShape>();

        foreach (var drawing in doc.Descendants(W + "drawing"))
        {
            shapes.AddRange(ReadAnchorShapes(drawing, themeColors));
        }

        return shapes;
    }

    private static DocxPageLayout ParseSectionProperties(XElement sectPr)
    {
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

        // Parse document grid for CJK line snapping
        float gridLinePitch = 0;
        var docGrid = sectPr.Element(W + "docGrid");
        if (docGrid != null)
        {
            var gridType = docGrid.Attribute(W + "type")?.Value;
            if (gridType is "lines" or "linesAndChars" or "snapToChars")
            {
                if (float.TryParse(docGrid.Attribute(W + "linePitch")?.Value, out var lp) && lp > 0)
                    gridLinePitch = lp * twipsToPoints;
            }
        }

        return new DocxPageLayout(pageWidth, pageHeight, marginTop, marginBottom, marginLeft, marginRight, gridLinePitch);
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

    private static (Dictionary<string, DocxStyleInfo> Styles, float DefaultLineSpacing, bool DefaultLineSpacingAbsolute, string? DefaultFontName) ReadStyles(ZipArchive archive)
    {
        var styles = new Dictionary<string, DocxStyleInfo>();
        var (majorThemeLatinFont, minorThemeLatinFont) = ReadThemeLatinFonts(archive);
        var entry = archive.GetEntry("word/styles.xml");
        if (entry == null) return (styles, 0, false, null);

        using var stream = entry.Open();
        var doc = XDocument.Load(stream);

        // Read docDefaults for baseline paragraph/run properties
        float defaultFontSize = 11f;
        float defaultSpacingAfter = -1;
        float defaultSpacingBefore = 0;
        float defaultLineSpacing = 0;
        bool defaultLineSpacingAbsolute = false;

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
                    if (int.TryParse(spacing.Attribute(W + "after")?.Value, out var sa))
                        defaultSpacingAfter = sa / 20f;
                    if (int.TryParse(spacing.Attribute(W + "line")?.Value, out var sl))
                    {
                        var lineRule = spacing.Attribute(W + "lineRule")?.Value;
                        defaultLineSpacingAbsolute = lineRule == "exact" || lineRule == "atLeast";
                        defaultLineSpacing = defaultLineSpacingAbsolute
                            ? sl / 20f : sl / 240f;
                    }
                }
            }
        }

        // Two-pass style reading: first pass populates all styles, second pass resolves basedOn inheritance
        var styleElements = doc.Descendants(W + "style").ToList();
        var basedOnMap = new Dictionary<string, string>(); // styleId -> basedOn styleId

        // First pass: read all styles without inheritance
        foreach (var style in styleElements)
        {
            var styleId = style.Attribute(W + "styleId")?.Value;
            if (string.IsNullOrEmpty(styleId)) continue;

            var basedOn = style.Element(W + "basedOn")?.Attribute(W + "val")?.Value;
            if (!string.IsNullOrEmpty(basedOn))
                basedOnMap[styleId] = basedOn;

            var rPr = style.Element(W + "rPr");
            var pPr = style.Element(W + "pPr");

            float fontSize = defaultFontSize;
            bool bold = false;
            bool italic = false;
            PdfColor? color = null;
            string alignment = "";
            float spacingBefore = defaultSpacingBefore;
            float spacingAfter = defaultSpacingAfter;
            bool caps = false;
            float styleLineSpacing = 0;
            bool styleLineSpacingAbsolute = false;
            bool contextualSpacing = false;

            if (rPr != null)
            {
                if (rPr.Element(W + "b") != null) bold = true;
                if (rPr.Element(W + "i") != null) italic = true;
                if (rPr.Element(W + "caps") != null) caps = true;
                var sz = rPr.Element(W + "sz")?.Attribute(W + "val")?.Value;
                if (float.TryParse(sz, out var s) && s > 0)
                    fontSize = s / 2f;
                color = ReadRunColor(rPr);
            }

            if (pPr != null)
            {
                var jc = pPr.Element(W + "jc")?.Attribute(W + "val")?.Value;
                if (!string.IsNullOrEmpty(jc))
                    alignment = jc;
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
                        styleLineSpacingAbsolute = lineRule == "exact" || lineRule == "atLeast";
                        styleLineSpacing = styleLineSpacingAbsolute ? sl / 20f : sl / 240f;
                    }
                }
                if (pPr.Element(W + "contextualSpacing") != null)
                    contextualSpacing = true;
            }

            // Heading styles get bold by default
            if (styleId.StartsWith("Heading", StringComparison.OrdinalIgnoreCase) ||
                styleId.StartsWith("heading", StringComparison.Ordinal))
            {
                bold = true;
            }

            styles[styleId] = new DocxStyleInfo(fontSize, bold, italic, color, alignment, spacingBefore, spacingAfter, caps, styleLineSpacing, styleLineSpacingAbsolute, contextualSpacing);
        }

        // Second pass: resolve basedOn inheritance
        foreach (var style in styleElements)
        {
            var styleId = style.Attribute(W + "styleId")?.Value;
            if (string.IsNullOrEmpty(styleId)) continue;
            if (!basedOnMap.TryGetValue(styleId, out var basedOnId)) continue;
            if (!styles.TryGetValue(basedOnId, out var baseStyle)) continue;
            if (!styles.TryGetValue(styleId, out var current)) continue;

            var rPr = style.Element(W + "rPr");
            var pPr = style.Element(W + "pPr");

            // Merge: base values overridden by explicitly set values
            var fontSize = current.FontSize;
            if (rPr?.Element(W + "sz") == null) fontSize = baseStyle.FontSize;
            var bold = current.Bold || baseStyle.Bold;
            var italic = current.Italic || baseStyle.Italic;
            var caps2 = current.Caps || baseStyle.Caps;
            var color2 = current.Color ?? baseStyle.Color;
            var alignment = !string.IsNullOrEmpty(current.Alignment) ? current.Alignment : baseStyle.Alignment;
            var spacingEl = pPr?.Element(W + "spacing");
            var spacingBefore = spacingEl?.Attribute(W + "before") != null ? current.SpacingBefore : baseStyle.SpacingBefore;
            var spacingAfter = spacingEl?.Attribute(W + "after") != null ? current.SpacingAfter : baseStyle.SpacingAfter;
            var contextualSpacing2 = current.ContextualSpacing || baseStyle.ContextualSpacing;

            styles[styleId] = new DocxStyleInfo(fontSize, bold, italic, color2, alignment, spacingBefore, spacingAfter, caps2, ContextualSpacing: contextualSpacing2);
        }

        // Extract default font name from Normal style or docDefaults
        string? defaultFontName = null;
        foreach (var style in styleElements)
        {
            var sid = style.Attribute(W + "styleId")?.Value;
            if (sid == "Normal" || style.Attribute(W + "default")?.Value == "1")
            {
                var rFonts = style.Element(W + "rPr")?.Element(W + "rFonts");
                if (rFonts != null)
                    defaultFontName = ResolveFontNameFromRFonts(rFonts, majorThemeLatinFont, minorThemeLatinFont);
                break;
            }
        }
        if (defaultFontName == null && docDefaults != null)
        {
            var rFonts = docDefaults.Element(W + "rPrDefault")?.Element(W + "rPr")?.Element(W + "rFonts");
            if (rFonts != null)
                defaultFontName = ResolveFontNameFromRFonts(rFonts, majorThemeLatinFont, minorThemeLatinFont);
        }

        return (styles, defaultLineSpacing, defaultLineSpacingAbsolute, defaultFontName);
    }

    /// <summary>
    /// Reads major/minor Latin theme fonts from theme1.xml.
    /// </summary>
    private static (string? MajorLatinFont, string? MinorLatinFont) ReadThemeLatinFonts(ZipArchive archive)
    {
        var entry = archive.GetEntry("word/theme/theme1.xml");
        if (entry == null) return (null, null);

        using var stream = entry.Open();
        var doc = XDocument.Load(stream);

        var fontScheme = doc.Descendants(A + "fontScheme").FirstOrDefault();
        if (fontScheme == null) return (null, null);

        var majorLatin = fontScheme.Element(A + "majorFont")?.Element(A + "latin")?.Attribute("typeface")?.Value;
        var minorLatin = fontScheme.Element(A + "minorFont")?.Element(A + "latin")?.Attribute("typeface")?.Value;
        return (string.IsNullOrWhiteSpace(majorLatin) ? null : majorLatin,
            string.IsNullOrWhiteSpace(minorLatin) ? null : minorLatin);
    }

    /// <summary>
    /// Resolves effective Latin font name from w:rFonts, including theme references.
    /// </summary>
    private static string? ResolveFontNameFromRFonts(XElement rFonts, string? majorThemeLatinFont, string? minorThemeLatinFont)
    {
        var explicitFont = rFonts.Attribute(W + "ascii")?.Value
            ?? rFonts.Attribute(W + "hAnsi")?.Value;
        if (!string.IsNullOrWhiteSpace(explicitFont))
            return explicitFont;

        var themeFont = rFonts.Attribute(W + "asciiTheme")?.Value
            ?? rFonts.Attribute(W + "hAnsiTheme")?.Value;
        if (string.IsNullOrWhiteSpace(themeFont))
            return null;

        if (themeFont.StartsWith("major", StringComparison.OrdinalIgnoreCase))
            return majorThemeLatinFont;
        if (themeFont.StartsWith("minor", StringComparison.OrdinalIgnoreCase))
            return minorThemeLatinFont;

        return null;
    }

    private static Dictionary<string, DocxNumberingDef> ReadNumbering(ZipArchive archive)
    {
        var result = new Dictionary<string, DocxNumberingDef>();
        var entry = archive.GetEntry("word/numbering.xml");
        if (entry == null) return result;

        using var stream = entry.Open();
        var doc = XDocument.Load(stream);

        // Read abstract numbering definitions: abstractNumId → list of level defs
        var abstractDefs = new Dictionary<string, List<DocxNumberingLevelDef>>();
        foreach (var absNum in doc.Descendants(W + "abstractNum"))
        {
            var absId = absNum.Attribute(W + "abstractNumId")?.Value;
            if (string.IsNullOrEmpty(absId)) continue;

            var levels = new List<DocxNumberingLevelDef>();
            foreach (var lvl in absNum.Elements(W + "lvl"))
            {
                var ilvl = int.TryParse(lvl.Attribute(W + "ilvl")?.Value, out var iv) ? iv : 0;
                var numFmt = lvl.Element(W + "numFmt")?.Attribute(W + "val")?.Value ?? "decimal";
                var lvlText = lvl.Element(W + "lvlText")?.Attribute(W + "val")?.Value ?? "%1.";
                var startVal = int.TryParse(lvl.Element(W + "start")?.Attribute(W + "val")?.Value, out var sv) ? sv : 1;
                // Read level indentation (pPr/ind) for hanging indent support
                float lvlIndentLeft = 0, lvlHanging = 0;
                var lvlInd = lvl.Element(W + "pPr")?.Element(W + "ind");
                if (lvlInd != null)
                {
                    if (int.TryParse(lvlInd.Attribute(W + "left")?.Value, out var li))
                        lvlIndentLeft = li / 20f;
                    if (int.TryParse(lvlInd.Attribute(W + "hanging")?.Value, out var lh))
                        lvlHanging = lh / 20f;
                }
                levels.Add(new DocxNumberingLevelDef(ilvl, numFmt, lvlText, startVal, lvlIndentLeft, lvlHanging));
            }
            abstractDefs[absId] = levels;
        }

        // Map num IDs to abstract definitions
        foreach (var num in doc.Descendants(W + "num"))
        {
            var numId = num.Attribute(W + "numId")?.Value;
            if (string.IsNullOrEmpty(numId)) continue;

            var absRef = num.Element(W + "abstractNumId")?.Attribute(W + "val")?.Value;
            var levels = new List<DocxNumberingLevelDef>();
            if (!string.IsNullOrEmpty(absRef) && abstractDefs.TryGetValue(absRef, out var abLevels))
                levels = abLevels;

            var format = levels.Count > 0 ? levels[0].NumFmt : "decimal";
            result[numId] = new DocxNumberingDef(format, levels);
        }

        return result;
    }
}

// ── Document model ──────────────────────────────────────────────────────

/// <summary>Represents a parsed DOCX document.</summary>
internal sealed record DocxDocument(
    List<DocxElement> Elements,
    DocxPageLayout? PageLayout = null,
    string? HeaderText = null,
    string? FooterText = null,
    List<DocxShape>? HeaderShapes = null,
    List<DocxShape>? FooterShapes = null,
    List<DocxRun>? HeaderRuns = null,
    List<DocxRun>? FooterRuns = null,
    float DefaultLineSpacing = 0,
    bool DefaultLineSpacingAbsolute = false,
    string? DefaultFontName = null
);

/// <summary>Page layout settings from sectPr.</summary>
internal sealed record DocxPageLayout(
    float PageWidth = 612,
    float PageHeight = 792,
    float MarginTop = 72,
    float MarginBottom = 72,
    float MarginLeft = 72,
    float MarginRight = 72,
    float GridLinePitch = 0
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
    bool LineSpacingAbsolute = false,
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
    List<DocxTabStop>? TabStops = null,
    DocxPageLayout? SectionBreak = null,
    DocxBorders? Borders = null,
    List<DocxShape>? Shapes = null,
    bool ForceSpacingBefore = false,
    DocxTextBoxBorder? TextBoxBorder = null,
    bool ContextualSpacing = false
) : DocxElement;

/// <summary>Represents a single border edge.</summary>
internal sealed record DocxBorderEdge(
    float Width,        // in points
    PdfColor Color
);

/// <summary>Represents paragraph borders (top, bottom, left, right).</summary>
internal sealed record DocxBorders(
    DocxBorderEdge? Top = null,
    DocxBorderEdge? Bottom = null,
    DocxBorderEdge? Left = null,
    DocxBorderEdge? Right = null
);

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
    bool IsPageBreak = false,
    bool Underline = false,
    float CharSpacing = 0
);

/// <summary>Represents an embedded image.</summary>
internal sealed record DocxImage(
    byte[] Data,
    string Extension,
    long WidthEmu = 0,
    long HeightEmu = 0,
    bool IsAnchor = false,
    long OffsetXEmu = 0,
    long OffsetYEmu = 0
);

/// <summary>Represents a text box outline border (rectangle drawn around text box content).</summary>
internal sealed record DocxTextBoxBorder(
    float LineWidth,
    PdfColor Color,
    float BoxXPt,
    float BoxWidthPt,
    float BoxHeightPt,
    float VerticalOffsetPt
);

/// <summary>Represents a normalized point in a custom shape path.</summary>
internal sealed record DocxPolygonPoint(
    float X,
    float Y
);

/// <summary>Represents one custom geometry path composed of one or more subpaths.</summary>
internal sealed record DocxCustomPath(
    List<List<DocxPolygonPoint>> Subpaths,
    bool UseEvenOddFill = false
);

/// <summary>Represents an anchor shape (filled rectangle or frame).</summary>
internal sealed record DocxShape(
    long WidthEmu,
    long HeightEmu,
    long OffsetXEmu,
    long OffsetYEmu,
    PdfColor FillColor,
    float Alpha = 1f,
    string? PresetGeometry = null,
    float FrameThicknessRatio = 0.125f,
    List<DocxCustomPath>? CustomPaths = null
);

/// <summary>Represents a table.</summary>
internal sealed record DocxTable(
    List<DocxTableRow> Rows,
    List<float> ColumnWidths,
    bool HasBorders = true,
    float CellMarginLeft = 5.4f,
    float CellMarginRight = 5.4f,
    float CellMarginTop = 0f,
    float CellMarginBottom = 0f
) : DocxElement;

/// <summary>Represents a table row.</summary>
internal sealed record DocxTableRow(List<DocxTableCell> Cells, float Height = 0);

/// <summary>Represents a table cell.</summary>
internal sealed record DocxTableCell(
    List<DocxParagraph> Paragraphs,
    float Width = 0,
    int GridSpan = 1,
    PdfColor? Shading = null,
    DocxBorders? Borders = null,
    bool IsVMergeContinue = false,
    bool IsVMergeRestart = false,
    string VerticalAlignment = "top"
);

/// <summary>Style definition from styles.xml.</summary>
internal sealed record DocxStyleInfo(
    float FontSize = 11f,
    bool Bold = false,
    bool Italic = false,
    PdfColor? Color = null,
    string Alignment = "",
    float SpacingBefore = 0,
    float SpacingAfter = -1,
    bool Caps = false,
    float LineSpacing = 0,
    bool LineSpacingAbsolute = false,
    bool ContextualSpacing = false
);

/// <summary>Numbering definition for lists.</summary>
internal sealed class DocxNumberingDef
{
    public string Format { get; }
    public int Counter { get; set; }
    public List<DocxNumberingLevelDef> Levels { get; }

    public DocxNumberingDef(string format, List<DocxNumberingLevelDef>? levels = null)
    {
        Format = format;
        Counter = 0;
        Levels = levels ?? [];
    }

    public string FormatListText(int ilvl)
    {
        var level = Levels.FirstOrDefault(l => l.Ilvl == ilvl) ?? Levels.FirstOrDefault();
        if (level == null)
            return Counter + ".";

        var formatted = FormatNumber(Counter, level.NumFmt);
        // Replace %1, %2 etc. placeholders in lvlText with formatted number
        var text = level.LvlText;
        text = text.Replace("%" + (ilvl + 1), formatted);
        // Also replace other level placeholders with empty if they exist
        for (var i = 1; i <= 9; i++)
            text = text.Replace("%" + i, "");
        return text;
    }

    private static string FormatNumber(int num, string fmt) => fmt switch
    {
        "decimal" => num.ToString(),
        "upperLetter" => num >= 1 && num <= 26 ? ((char)('A' + num - 1)).ToString() : num.ToString(),
        "lowerLetter" => num >= 1 && num <= 26 ? ((char)('a' + num - 1)).ToString() : num.ToString(),
        "upperRoman" => ToRoman(num),
        "lowerRoman" => ToRoman(num).ToLowerInvariant(),
        "japaneseCounting" or "chineseCounting" or "ideographTraditional" =>
            num >= 1 && num <= 10 ? "\u4e00\u4e8c\u4e09\u56db\u4e94\u516d\u4e03\u516b\u4e5d\u5341"[num - 1].ToString() : num.ToString(),
        _ => num.ToString()
    };

    private static string ToRoman(int num)
    {
        if (num <= 0 || num > 3999) return num.ToString();
        string[] thousands = ["", "M", "MM", "MMM"];
        string[] hundreds = ["", "C", "CC", "CCC", "CD", "D", "DC", "DCC", "DCCC", "CM"];
        string[] tens = ["", "X", "XX", "XXX", "XL", "L", "LX", "LXX", "LXXX", "XC"];
        string[] ones = ["", "I", "II", "III", "IV", "V", "VI", "VII", "VIII", "IX"];
        return thousands[num / 1000] + hundreds[num % 1000 / 100] + tens[num % 100 / 10] + ones[num % 10];
    }
}

internal sealed record DocxNumberingLevelDef(int Ilvl, string NumFmt, string LvlText, int Start, float IndentLeft = 0, float Hanging = 0);
