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
    private static readonly XNamespace MC = "http://schemas.openxmlformats.org/markup-compatibility/2006";
    private static readonly XNamespace M = "http://schemas.openxmlformats.org/officeDocument/2006/math";

    /// <summary>
    /// Per-document context for resolving SDT (content control) data bindings and
    /// glossary placeholder doc-parts. Loaded once per Read() call.
    /// </summary>
    private sealed class SdtContext
    {
        // storeItemID (with braces, upper-case GUID) -> custom XML data store
        public Dictionary<string, XDocument> XmlStores { get; } = new(StringComparer.OrdinalIgnoreCase);
        // glossary docPart name -> inline runs extracted from its docPartBody paragraphs
        public Dictionary<string, IList<XElement>> InlinePlaceholders { get; } = new(StringComparer.OrdinalIgnoreCase);
    }

    [ThreadStatic] private static SdtContext? _sdtContext;

    private static SdtContext LoadSdtContext(ZipArchive archive)
    {
        var ctx = new SdtContext();

        // docProps/core.xml uses a reserved storeItemID for SDT data-binding.
        var coreEntry = archive.GetEntry("docProps/core.xml");
        if (coreEntry != null)
        {
            try
            {
                using var s = coreEntry.Open();
                ctx.XmlStores["{6C3C8BC8-F283-45AE-878A-BAB7291924A1}"] = XDocument.Load(s);
            }
            catch { /* ignore malformed core.xml */ }
        }

        // customXml/itemPropsN.xml -> ds:datastoreItem ds:itemID maps to customXml/itemN.xml
        XNamespace dsNs = "http://schemas.openxmlformats.org/officeDocument/2006/customXml";
        foreach (var e in archive.Entries)
        {
            var path = e.FullName.Replace('\\', '/');
            if (!path.StartsWith("customXml/itemProps", StringComparison.OrdinalIgnoreCase) ||
                !path.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
                continue;
            try
            {
                XDocument props;
                using (var s = e.Open())
                    props = XDocument.Load(s);
                var item = props.Descendants(dsNs + "datastoreItem").FirstOrDefault();
                var id = item?.Attribute(dsNs + "itemID")?.Value;
                if (string.IsNullOrEmpty(id)) continue;
                var itemPath = System.Text.RegularExpressions.Regex.Replace(
                    path, "itemProps", "item", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                var itemEntry = archive.GetEntry(itemPath);
                if (itemEntry == null) continue;
                using var s2 = itemEntry.Open();
                ctx.XmlStores[id!] = XDocument.Load(s2);
            }
            catch { /* ignore individual store failures */ }
        }

        // Glossary docParts provide placeholder text shown when a bound SDT resolves to empty.
        var glossaryEntry = archive.GetEntry("word/glossary/document.xml");
        if (glossaryEntry != null)
        {
            try
            {
                XDocument glossDoc;
                using (var s = glossaryEntry.Open())
                    glossDoc = XDocument.Load(s);
                foreach (var dp in glossDoc.Descendants(W + "docPart"))
                {
                    var name = dp.Element(W + "docPartPr")?.Element(W + "name")?.Attribute(W + "val")?.Value;
                    if (string.IsNullOrEmpty(name)) continue;
                    var body = dp.Element(W + "docPartBody");
                    if (body == null) continue;
                    var runs = new List<XElement>();
                    foreach (var p in body.Elements(W + "p"))
                    {
                        foreach (var r in p.Elements(W + "r"))
                            runs.Add(r);
                    }
                    if (runs.Count > 0)
                        ctx.InlinePlaceholders[name!] = runs;
                }
            }
            catch { /* ignore malformed glossary */ }
        }

        return ctx;
    }

    private static string? ResolveSdtBoundValue(XElement sdtPr)
    {
        var ctx = _sdtContext;
        if (ctx == null) return null;
        var dataBinding = sdtPr.Element(W + "dataBinding");
        if (dataBinding == null) return null;
        var storeItemID = dataBinding.Attribute(W + "storeItemID")?.Value;
        var xpath = dataBinding.Attribute(W + "xpath")?.Value;
        var prefixMappings = dataBinding.Attribute(W + "prefixMappings")?.Value ?? "";
        if (string.IsNullOrEmpty(storeItemID) || string.IsNullOrEmpty(xpath)) return null;
        if (!ctx.XmlStores.TryGetValue(storeItemID, out var doc) || doc.Root == null) return null;

        var nsMgr = new System.Xml.XmlNamespaceManager(new System.Xml.NameTable());
        foreach (System.Text.RegularExpressions.Match m in System.Text.RegularExpressions.Regex.Matches(
            prefixMappings, @"xmlns:([\w-]+)\s*=\s*['""]([^'""]+)['""]"))
        {
            try { nsMgr.AddNamespace(m.Groups[1].Value, m.Groups[2].Value); } catch { }
        }

        try
        {
            var node = System.Xml.XPath.Extensions.XPathEvaluate(doc, xpath, nsMgr);
            if (node is IEnumerable<object> seq)
            {
                foreach (var n in seq)
                {
                    if (n is XElement xe) return xe.Value;
                    if (n is XAttribute xa) return xa.Value;
                    if (n is XText xt) return xt.Value;
                }
                return null;
            }
            return node?.ToString();
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Unwraps SDT (Structured Document Tag) and mc:AlternateContent elements
    /// by replacing them with their inner children. SDTs with a w:dataBinding
    /// resolve their bound XML value; if the bound value is empty and a
    /// w:placeholder/w:docPart references a glossary entry, the placeholder
    /// runs are emitted instead of the cached sdtContent (which Word may have
    /// stamped with the local user name on last open).
    /// </summary>
    private static IEnumerable<XElement> UnwrapSdt(IEnumerable<XElement> elements)
    {
        foreach (var el in elements)
        {
            if (el.Name == W + "sdt")
            {
                var sdtPr = el.Element(W + "sdtPr");
                var content = el.Element(W + "sdtContent");
                var hasDataBinding = sdtPr?.Element(W + "dataBinding") != null;

                if (hasDataBinding && sdtPr != null && content != null && _sdtContext != null)
                {
                    var bound = ResolveSdtBoundValue(sdtPr);
                    bool isInline = content.Elements(W + "r").Any() && !content.Elements(W + "p").Any();

                    if (string.IsNullOrEmpty(bound))
                    {
                        // Bound value is empty -> use placeholder docPart text if available.
                        var placeholderName = sdtPr.Element(W + "placeholder")?.Element(W + "docPart")?.Attribute(W + "val")?.Value;
                        if (!string.IsNullOrEmpty(placeholderName) &&
                            _sdtContext.InlinePlaceholders.TryGetValue(placeholderName!, out var phRuns) &&
                            phRuns.Count > 0 && isInline)
                        {
                            foreach (var r in phRuns)
                                yield return r;
                            continue;
                        }
                        // Otherwise fall through to default sdtContent unwrap.
                    }
                    else if (isInline)
                    {
                        // Replace cached run text with the live bound value, preserving rPr from
                        // the first existing run if present.
                        var firstRun = content.Elements(W + "r").FirstOrDefault();
                        var rPr = firstRun?.Element(W + "rPr");
                        var newRun = new XElement(W + "r");
                        if (rPr != null) newRun.Add(new XElement(rPr));
                        newRun.Add(new XElement(W + "t",
                            new XAttribute(XNamespace.Xml + "space", "preserve"),
                            bound));
                        yield return newRun;
                        continue;
                    }
                }

                if (content != null)
                {
                    foreach (var inner in UnwrapSdt(content.Elements()))
                        yield return inner;
                }
            }
            else if (el.Name == MC + "AlternateContent")
            {
                // Prefer mc:Choice content over mc:Fallback
                var choice = el.Element(MC + "Choice");
                if (choice != null)
                {
                    foreach (var inner in UnwrapSdt(choice.Elements()))
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

        // SDT data-binding / glossary placeholder context (must be loaded before any
        // UnwrapSdt call, including footnotes/headers/body).
        _sdtContext = LoadSdtContext(archive);
        try
        {
            return ReadCore(archive);
        }
        finally
        {
            _sdtContext = null;
        }
    }

    private static DocxDocument ReadCore(ZipArchive archive)
    {
        // Read relationships to resolve image references
        var relationships = ReadRelationships(archive);

        // Read styles
        var (styles, defaultLineSpacing, defaultLineSpacingAbsolute, defaultFontName, defaultEastAsiaFontName, tableStyles) = ReadStyles(archive);

        // Read numbering definitions (for list bullets/numbers)
        var numbering = ReadNumbering(archive);

        // Read theme colors for resolving schemeClr references
        var themeColors = ReadThemeColors(archive);

        // Read footnotes
        var footnotes = ReadFootnotes(archive, styles, defaultFontName, defaultEastAsiaFontName);

        // Read settings.xml for defaultTabStop (used for list-label tab-suffix snapping).
        // Word's spec default is 720 twips (36pt); CJK templates often override to 480 (24pt).
        float defaultTabStopPt = 36f;
        var settingsEntry = archive.GetEntry("word/settings.xml");
        if (settingsEntry != null)
        {
            try
            {
                using var settingsStream = settingsEntry.Open();
                var settingsDoc = XDocument.Load(settingsStream);
                var dts = settingsDoc.Descendants(W + "defaultTabStop").FirstOrDefault();
                var dtsVal = dts?.Attribute(W + "val")?.Value;
                if (int.TryParse(dtsVal, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out var dtsTwips) && dtsTwips > 0)
                    defaultTabStopPt = dtsTwips / 20f;
            }
            catch { /* ignore malformed settings */ }
        }

        // Read main document
        var entry = archive.GetEntry("word/document.xml");
        if (entry == null)
            return new DocxDocument([]);

        XDocument doc;
        using (var docStream = entry.Open())
            doc = XDocument.Load(docStream);
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
                var seenTextBoxPositions = new HashSet<(int, int)>(); // dedup by position
                List<DocxFloatingTextBox>? floatingTextBoxes = null;
                foreach (var anchor in child.Descendants(WP + "anchor"))
                {
                    var txbx = anchor.Descendants(WPS + "txbx").FirstOrDefault();
                    if (txbx == null) continue;
                    var txbxContent = txbx.Element(W + "txbxContent");
                    if (txbxContent == null) continue;

                    // Check wrap mode
                    bool isWrapTopBottom = anchor.Element(WP + "wrapTopAndBottom") != null;
                    bool isWrapNone = !isWrapTopBottom
                        && anchor.Element(WP + "wrapSquare") == null
                        && anchor.Element(WP + "wrapTight") == null
                        && anchor.Element(WP + "wrapThrough") == null;

                    // Deduplicate text boxes at the same position (e.g. multiple identical anchors)
                    {
                        var dedupPosH = anchor.Element(WP + "positionH");
                        var dedupPosV = anchor.Element(WP + "positionV");
                        int hVal = 0, vVal = 0;
                        if (dedupPosH != null)
                        {
                            var off = dedupPosH.Element(WP + "posOffset");
                            if (off != null && int.TryParse(off.Value, out var h)) hVal = h;
                        }
                        if (dedupPosV != null)
                        {
                            var off = dedupPosV.Element(WP + "posOffset");
                            if (off != null && int.TryParse(off.Value, out var v)) vVal = v;
                        }
                        var posKey = (hVal / 1000, vVal / 1000); // round to avoid floating-point differences
                        if (!seenTextBoxPositions.Add(posKey))
                            continue; // skip duplicate text box at same position
                    }

                    // Read anchor position
                    float anchorOffsetPt = 0;
                    string vRelativeFrom = "paragraph";
                    string? vAlign = null;
                    var posV = anchor.Element(WP + "positionV");
                    if (posV != null)
                    {
                        vRelativeFrom = posV.Attribute("relativeFrom")?.Value ?? "paragraph";
                        var off = posV.Element(WP + "posOffset");
                        if (off != null && long.TryParse(off.Value, out var emu))
                            anchorOffsetPt = emu / 914400f * 72f;
                        var alignEl = posV.Element(WP + "align");
                        if (alignEl != null) vAlign = alignEl.Value;
                    }
                    float anchorXPt = 0;
                    string hRelativeFrom = "column";
                    string? hAlign = null;
                    var posH = anchor.Element(WP + "positionH");
                    if (posH != null)
                    {
                        hRelativeFrom = posH.Attribute("relativeFrom")?.Value ?? "column";
                        var off = posH.Element(WP + "posOffset");
                        if (off != null && long.TryParse(off.Value, out var hEmu))
                            anchorXPt = hEmu / 914400f * 72f;
                        var alignEl = posH.Element(WP + "align");
                        if (alignEl != null) hAlign = alignEl.Value;
                    }
                    float extentHeightPt = 0;
                    float extentWidthPt = 0;
                    var extent = anchor.Element(WP + "extent");
                    if (extent != null)
                    {
                        if (long.TryParse(extent.Attribute("cy")?.Value, out var cy))
                            extentHeightPt = cy / 914400f * 72f;
                        if (long.TryParse(extent.Attribute("cx")?.Value, out var cx))
                            extentWidthPt = cx / 914400f * 72f;
                    }

                    // Read text box outline (border) and fill from shape properties
                    DocxTextBoxBorder? textBoxBorder = null;
                    PdfColor? textBoxFillColor = null;
                    // When the anchor contains a group shape (wgp), the individual shapes
                    // in the group already render their own fills via ReadAnchorShapes /
                    // ProcessGroupChildren.  Extracting fill from the wsp here would
                    // duplicate the background rectangle and paint over child shapes.
                    bool anchorHasGroupShape = anchor.Descendants(WPG + "wgp").Any();
                    var wsp = anchor.Descendants(WPS + "wsp").FirstOrDefault();
                    if (wsp != null)
                    {
                        var spPr = wsp.Element(WPS + "spPr") ?? wsp.Element(A + "spPr");
                        // Parse shape fill (background) — skip for group shapes
                        var shapeFill = !anchorHasGroupShape ? spPr?.Element(A + "solidFill") : null;
                        if (shapeFill != null)
                        {
                            var (fc, _) = ResolveSolidFill(shapeFill, themeColors);
                            textBoxFillColor = fc;
                        }
                        var ln = spPr?.Element(A + "ln");
                        if (ln != null)
                        {
                            var lnFill = ln.Element(A + "solidFill");
                            if (lnFill != null)
                            {
                                var (resolvedLn, _) = ResolveSolidFill(lnFill, themeColors);
                                var lnColor = resolvedLn ?? new PdfColor(0, 0, 0);
                                float lnWidth = 0.5f; // default
                                if (int.TryParse(ln.Attribute("w")?.Value, out var lnW))
                                    lnWidth = lnW / 914400f * 72f;
                                if (lnWidth < 0.25f) lnWidth = 0.5f;

                                textBoxBorder = new DocxTextBoxBorder(lnWidth, lnColor, anchorXPt, extentWidthPt, extentHeightPt, anchorOffsetPt);
                            }
                        }
                    }

                    // Parse bodyPr text insets (defaults: lIns=91440, tIns=45720 EMU)
                    float topInsetPt = 3.6f;  // 45720 EMU default
                    float leftInsetPt = 7.2f; // 91440 EMU default
                    var bodyPr = wsp?.Element(WPS + "bodyPr");
                    if (bodyPr != null)
                    {
                        if (long.TryParse(bodyPr.Attribute("tIns")?.Value, out var tIns))
                            topInsetPt = tIns / 914400f * 72f;
                        if (long.TryParse(bodyPr.Attribute("lIns")?.Value, out var lIns))
                            leftInsetPt = lIns / 914400f * 72f;
                    }

                    if (isWrapNone)
                    {
                        // wrapNone text boxes are positioned absolutely and do not
                        // consume space in the main flow per the OOXML spec.
                        // Skip ghost flow; only render as floating overlay.
                        // The document's own empty paragraphs provide vertical spacing.
                        bool skipGhostFlow = true;

                        // wrapNone dual-render: emit content as ghost flow paragraphs for layout
                        // (advance Y but don't render text), AND create floating textbox for
                        // visual overlay at the absolute position.
                        var floatingParas = new List<DocxParagraph>();
                        bool firstContent = true;
                        foreach (var tp in UnwrapSdt(txbxContent.Elements()).Where(e => e.Name == W + "p"))
                        {
                            var tbPara = ReadParagraph(tp, styles, numbering, relationships, archive, themeColors);
                            if (tbPara != null)
                            {
                                var hasVisibleContent = tbPara.Images.Count > 0
                                    || tbPara.Shading != null
                                    || tbPara.Runs.Any(r => !string.IsNullOrWhiteSpace(r.Text));

                                tbPara = tbPara with { IndentLeft = 0, IndentRight = 0, IndentFirstLine = 0, IsTextBoxFlow = true, TextBoxWidth = extentWidthPt };

                                if (firstContent && hasVisibleContent && anchorOffsetPt > 0)
                                    tbPara = tbPara with { SpacingBefore = tbPara.SpacingBefore + anchorOffsetPt, ForceSpacingBefore = true };
                                if (firstContent && hasVisibleContent && textBoxBorder != null)
                                    tbPara = tbPara with { TextBoxBorder = textBoxBorder };
                                if (!skipGhostFlow)
                                {
                                    elements.Add(tbPara);
                                    if (hasVisibleContent)
                                        emittedVisibleTextBoxParagraph = true;
                                }
                                if (hasVisibleContent)
                                    firstContent = false;

                                // Also collect for floating overlay
                                floatingParas.Add(tbPara with { IsTextBoxFlow = false });
                            }
                        }
                        if (floatingParas.Count > 0)
                        {
                            floatingTextBoxes ??= new List<DocxFloatingTextBox>();
                            floatingTextBoxes.Add(new DocxFloatingTextBox(anchorXPt, anchorOffsetPt, extentWidthPt, extentHeightPt, floatingParas, textBoxBorder, hRelativeFrom, vRelativeFrom, textBoxFillColor, topInsetPt, leftInsetPt, hAlign, vAlign));
                        }
                    }
                    else
                    {
                        // wrapTopAndBottom / wrapSquare / etc.: emit as flow paragraphs
                        bool firstContent = true;
                        foreach (var tp in txbxContent.Elements(W + "p"))
                        {
                            var tbPara = ReadParagraph(tp, styles, numbering, relationships, archive, themeColors);
                            if (tbPara != null)
                            {
                                var hasVisibleContent = tbPara.Images.Count > 0
                                    || tbPara.Shading != null
                                    || tbPara.Runs.Any(r => !string.IsNullOrWhiteSpace(r.Text));

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
                        if (isWrapTopBottom)
                            textBoxSpacing += anchorOffsetPt;
                    }
                }

                // Parse connector/line shapes from anchors (straightConnector1, line)
                List<DocxConnectorLine>? connectorLines = null;
                foreach (var anchor in child.Descendants(WP + "anchor"))
                {
                    var wspEl = anchor.Descendants(WPS + "wsp").FirstOrDefault();
                    if (wspEl == null) continue;
                    if (anchor.Descendants(WPS + "txbx").Any()) continue; // already handled as text box

                    var spPrEl = wspEl.Element(WPS + "spPr") ?? wspEl.Element(A + "spPr");
                    if (spPrEl == null) continue;
                    var prstGeomEl = spPrEl.Element(A + "prstGeom");
                    var prst = prstGeomEl?.Attribute("prst")?.Value;
                    if (prst != "straightConnector1" && prst != "line") continue;

                    // Position
                    var cPosH = anchor.Element(WP + "positionH");
                    var cPosV = anchor.Element(WP + "positionV");
                    float cXPt = 0, cYPt = 0;
                    string cHRel = "column", cVRel = "paragraph";
                    if (cPosH != null)
                    {
                        cHRel = cPosH.Attribute("relativeFrom")?.Value ?? "column";
                        var off = cPosH.Element(WP + "posOffset");
                        if (off != null && long.TryParse(off.Value, out var hEmu))
                            cXPt = hEmu / 914400f * 72f;
                    }
                    if (cPosV != null)
                    {
                        cVRel = cPosV.Attribute("relativeFrom")?.Value ?? "paragraph";
                        var off = cPosV.Element(WP + "posOffset");
                        if (off != null && long.TryParse(off.Value, out var vEmu))
                            cYPt = vEmu / 914400f * 72f;
                    }

                    // Extent
                    float cWPt = 0, cHPt = 0;
                    var cExtent = anchor.Element(WP + "extent");
                    if (cExtent != null)
                    {
                        if (long.TryParse(cExtent.Attribute("cx")?.Value, out var cx))
                            cWPt = cx / 914400f * 72f;
                        if (long.TryParse(cExtent.Attribute("cy")?.Value, out var cy))
                            cHPt = cy / 914400f * 72f;
                    }

                    // Check for flip in xfrm
                    var xfrm = spPrEl.Element(A + "xfrm");
                    bool flipH = xfrm?.Attribute("flipH")?.Value == "1";
                    bool flipV = xfrm?.Attribute("flipV")?.Value == "1";

                    // Compute line endpoints
                    float x1 = cXPt, y1 = cYPt;
                    float x2 = cXPt + cWPt, y2 = cYPt + cHPt;
                    if (flipH) { x1 = cXPt + cWPt; x2 = cXPt; }
                    if (flipV) { y1 = cYPt + cHPt; y2 = cYPt; }

                    // Line properties
                    var lnEl = spPrEl.Element(A + "ln");
                    float lineW = 1f; // default
                    if (lnEl != null && int.TryParse(lnEl.Attribute("w")?.Value, out var lnWEmu))
                        lineW = lnWEmu / 914400f * 72f;
                    if (lineW < 0.25f) lineW = 0.75f;

                    // Dash pattern
                    float[]? dashArr = null;
                    var prstDash = lnEl?.Element(A + "prstDash");
                    if (prstDash != null)
                    {
                        var dashVal = prstDash.Attribute("val")?.Value;
                        dashArr = dashVal switch
                        {
                            "dash" => new[] { 4f * lineW, 3f * lineW },
                            "sysDash" => new[] { 3f * lineW, 1f * lineW },
                            "dot" => new[] { lineW, lineW },
                            "dashDot" => new[] { 4f * lineW, 2f * lineW, lineW, 2f * lineW },
                            "lgDash" => new[] { 8f * lineW, 3f * lineW },
                            _ => null
                        };
                    }

                    // Arrow heads
                    bool hasTailArrow = false, hasHeadArrow = false;
                    var tailEnd = lnEl?.Element(A + "tailEnd");
                    if (tailEnd != null && tailEnd.Attribute("type")?.Value is "triangle" or "arrow" or "stealth")
                        hasTailArrow = true;
                    var headEnd = lnEl?.Element(A + "headEnd");
                    if (headEnd != null && headEnd.Attribute("type")?.Value is "triangle" or "arrow" or "stealth")
                        hasHeadArrow = true;

                    // Resolve line color: try direct solidFill in ln, then style lnRef
                    PdfColor lineColor = new PdfColor(0, 0, 0); // default black
                    var lnFillEl = lnEl?.Element(A + "solidFill");
                    if (lnFillEl != null)
                    {
                        var (c, _) = ResolveSolidFill(lnFillEl, themeColors);
                        if (c != null) lineColor = c.Value;
                    }
                    else
                    {
                        // Try style element
                        var styleEl = wspEl.Element(WPS + "style");
                        var lnRef = styleEl?.Element(A + "lnRef");
                        var lnRefFill = lnRef?.Element(A + "schemeClr") ?? lnRef?.Element(A + "srgbClr");
                        if (lnRefFill != null)
                        {
                            // Create a temporary solidFill-like wrapper for ResolveSolidFill
                            var tempFill = new XElement(A + "solidFill", lnRefFill);
                            var (c, _) = ResolveSolidFill(tempFill, themeColors);
                            if (c != null) lineColor = c.Value;
                        }
                    }

                    connectorLines ??= new List<DocxConnectorLine>();
                    connectorLines.Add(new DocxConnectorLine(x1, y1, x2, y2, lineW, lineColor, dashArr, hasTailArrow, hasHeadArrow, cHRel, cVRel));
                }

                var paragraph = ReadParagraph(child, styles, numbering, relationships, archive, themeColors, defaultFontName, defaultEastAsiaFontName);
                if (paragraph != null)
                {
                    // Host paragraphs for anchored textboxes may contain synthetic fallback
                    // runs that duplicate textbox text; skip these when textbox content
                    // was already emitted as standalone paragraphs.
                    // For floating textboxes (wrapNone), still emit the host paragraph
                    // (with its spacing) to preserve vertical layout.
                    if (emittedVisibleTextBoxParagraph && allRunsHostTextBox)
                    {
                        // Even when skipping the host paragraph's runs, still emit a
                        // minimal paragraph if there are floating textboxes that need
                        // to be attached (they render at absolute positions as overlays).
                        if (floatingTextBoxes != null || connectorLines != null)
                        {
                            var emptyHost = paragraph with { Runs = [], FloatingTextBoxes = floatingTextBoxes, ConnectorLines = connectorLines };
                            elements.Add(emptyHost);
                        }
                        continue;
                    }

                    // Add text box displacement as SpacingBefore on the containing paragraph
                    if (textBoxSpacing > 0)
                    {
                        paragraph = paragraph with { SpacingBefore = paragraph.SpacingBefore + textBoxSpacing };
                    }
                    // Attach floating textboxes to the host paragraph for rendering at absolute positions
                    if (floatingTextBoxes != null)
                    {
                        paragraph = paragraph with { FloatingTextBoxes = floatingTextBoxes };
                    }
                    // Attach connector lines to the host paragraph
                    if (connectorLines != null)
                    {
                        paragraph = paragraph with { ConnectorLines = connectorLines };
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
                var table = ReadTable(child, styles, numbering, relationships, archive, defaultFontName, defaultEastAsiaFontName, tableStyles);
                if (table != null)
                    elements.Add(table);
            }
        }

        // Read page layout from sectPr
        var pageLayout = ReadPageLayout(body);

        // Read header/footer content
        var headerText = ReadHeaderFooter(body, relationships, archive, styles, numbering, "headerReference", defaultFontName, defaultEastAsiaFontName, tableStyles);
        var footerText = ReadHeaderFooter(body, relationships, archive, styles, numbering, "footerReference", defaultFontName, defaultEastAsiaFontName, tableStyles);
        var headerShapes = ReadHeaderFooterShapes(body, relationships, archive, "headerReference", themeColors);
        var footerShapes = ReadHeaderFooterShapes(body, relationships, archive, "footerReference", themeColors);
        var headerRuns = ReadHeaderFooterRuns(body, relationships, archive, styles, numbering, "headerReference", defaultFontName, defaultEastAsiaFontName);
        var footerRuns = ReadHeaderFooterRuns(body, relationships, archive, styles, numbering, "footerReference", defaultFontName, defaultEastAsiaFontName);
        var headerElements = ReadHeaderFooterElements(body, relationships, archive, styles, numbering, "headerReference", defaultFontName, defaultEastAsiaFontName, tableStyles);
        var footerElements = ReadHeaderFooterElements(body, relationships, archive, styles, numbering, "footerReference", defaultFontName, defaultEastAsiaFontName, tableStyles);
        // First-page-specific header/footer (used when sectPr has <w:titlePg/>).
        // Returns an empty list (not null) when titlePg is set but no first-type reference exists,
        // signaling "render no header/footer on the first page".
        List<DocxElement>? firstPageHeaderElements = null;
        List<DocxElement>? firstPageFooterElements = null;
        if (pageLayout?.TitlePg == true)
        {
            var bodySectPr = body.Element(W + "sectPr");
            if (bodySectPr != null)
            {
                var fhe = ReadHeaderFooterElementsFromSectPr(bodySectPr, relationships, archive, styles, numbering, "headerReference", defaultFontName, defaultEastAsiaFontName, tableStyles, typeFilter: "first");
                var ffe = ReadHeaderFooterElementsFromSectPr(bodySectPr, relationships, archive, styles, numbering, "footerReference", defaultFontName, defaultEastAsiaFontName, tableStyles, typeFilter: "first");
                firstPageHeaderElements = fhe;
                firstPageFooterElements = ffe;
            }
        }
        var hfImages = ReadHeaderFooterImages(body, relationships, archive, "headerReference");
        hfImages.AddRange(ReadHeaderFooterImages(body, relationships, archive, "footerReference"));

        // Read per-section footer elements from paragraph-level sectPr
        List<List<DocxElement>?>? sectionFooterElements = null;
        foreach (var element in elements)
        {
            if (element is DocxParagraph p && p.SectionBreak != null)
            {
                // Find the corresponding paragraph XML to extract sectPr
                // Match by searching body paragraphs with sectPr in pPr
                sectionFooterElements ??= new List<List<DocxElement>?>();
                // Default: no section-specific footer (will inherit body footer)
                sectionFooterElements.Add(null);
            }
        }
        // Now actually read the footer elements from each paragraph's sectPr
        if (sectionFooterElements != null)
        {
            int sIdx = 0;
            foreach (var pEl in UnwrapSdt(body.Elements()).Where(e => e.Name == W + "p"))
            {
                var pPr = pEl.Element(W + "pPr");
                var sectPrEl = pPr?.Element(W + "sectPr");
                if (sectPrEl == null) continue;
                if (sIdx < sectionFooterElements.Count)
                {
                    var secFooter = ReadHeaderFooterElementsFromSectPr(sectPrEl, relationships, archive, styles, numbering, "footerReference", defaultFontName, defaultEastAsiaFontName, tableStyles);
                    sectionFooterElements[sIdx] = secFooter.Count > 0 ? secFooter : null;
                }
                sIdx++;
            }
            // Add body sectPr footer as the last section entry
            sectionFooterElements.Add(footerElements.Count > 0 ? footerElements : null);

            // Footer inheritance: fill null entries with the most recent non-null footer
            List<DocxElement>? lastFooter = null;
            for (int i = 0; i < sectionFooterElements.Count; i++)
            {
                if (sectionFooterElements[i] != null)
                    lastFooter = sectionFooterElements[i];
                else if (lastFooter != null)
                    sectionFooterElements[i] = lastFooter;
            }
        }

        return new DocxDocument(elements, pageLayout, headerText, footerText, headerShapes, footerShapes, headerRuns, footerRuns,
            defaultLineSpacing, defaultLineSpacingAbsolute, defaultFontName, defaultEastAsiaFontName,
            headerElements.Count > 0 ? headerElements : null, footerElements.Count > 0 ? footerElements : null,
            sectionFooterElements, footnotes,
            hfImages.Count > 0 ? hfImages : null,
            defaultTabStopPt,
            firstPageHeaderElements,
            firstPageFooterElements);
    }

    private static DocxParagraph? ReadParagraph(XElement pElement, Dictionary<string, DocxStyleInfo> styles,
        Dictionary<string, DocxNumberingDef> numbering, Dictionary<string, string> relationships, ZipArchive archive,
        Dictionary<string, string>? themeColors = null, string? defaultLatinFontName = null, string? defaultEastAsiaFontName = null)
    {
        var runs = new List<DocxRun>();
        var images = new List<DocxImage>();
        var shapes = new List<DocxShape>();

        // Read paragraph properties
        var pPr = pElement.Element(W + "pPr");
        var alignment = "left";
        float spacingBefore = -1;
        float spacingAfter = -1;
        bool spacingAfterExplicit = false;
        bool spacingBeforeExplicit = false;
        float lineSpacing = 0;
        bool lineSpacingAbsolute = false;
        bool lineSpacingExact = false;
        float indentLeft = 0;
        float indentRight = 0;
        float indentFirstLine = 0;
        bool paraHasIndentLeft = false;
        bool paraHasIndentRight = false;
        bool paraHasIndentFirstLine = false;
        // Numbering-level indent captured separately so it can be applied as the
        // lowest-priority fallback (paragraph > style > numbering per OOXML cascade).
        float numLevelIndentLeft = 0;
        float numLevelIndentFirstLine = 0;
        // True when pPr/ind explicitly sets left/start/hanging numerically at the
        // paragraph level (i.e., the user positioned this paragraph rather than
        // inheriting from numbering/style). Word's auto-numbering "suff=tab" only
        // advances body text to the next tab stop when the paragraph does NOT
        // override numbering's indent — explicit ind disables that auto-tab.
        bool paraHasExplicitListIndent = false;
        bool isBulletList = false;
        bool isNumberedList = false;
        bool pageBreakBefore = false;
        bool pageBreakAfter = false;
        bool hasLastRenderedPageBreak = false;
        bool snapToGrid = true;
        int listLevel = 0;
        string? listText = null;
        bool listTextBold = false;
        string? listFontName = null;
        string? listNumFmt = null;
        string listSuff = "tab";
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
        int firstLineChars = 0;
        bool firstLineCharsExplicit = false;
        bool paragraphMarkUnderline = false;
        string? paragraphMarkFontName = null;
        bool hasExplicitAlignment = false;

        if (pPr != null)
        {
            // Style reference
            styleId = pPr.Element(W + "pStyle")?.Attribute(W + "val")?.Value;

            // Alignment
            var jc = pPr.Element(W + "jc")?.Attribute(W + "val")?.Value;
            if (!string.IsNullOrEmpty(jc))
            {
                alignment = jc;
                hasExplicitAlignment = true;
            }

            // framePr with xAlign overrides alignment (legacy positioned text frame)
            var framePrXAlign = pPr.Element(W + "framePr")?.Attribute(W + "xAlign")?.Value;
            if (!string.IsNullOrEmpty(framePrXAlign))
            {
                alignment = framePrXAlign; // "center", "right", etc.
                hasExplicitAlignment = true;
            }

            // Spacing (in twips: 1/20 of a point)
            var spacing = pPr.Element(W + "spacing");
            if (spacing != null)
            {
                if (int.TryParse(spacing.Attribute(W + "before")?.Value, out var sb))
                {
                    spacingBefore = sb / 20f;
                    spacingBeforeExplicit = true;
                }
                if (int.TryParse(spacing.Attribute(W + "after")?.Value, out var sa))
                {
                    spacingAfter = sa / 20f;
                    spacingAfterExplicit = true;
                }
                if (int.TryParse(spacing.Attribute(W + "line")?.Value, out var sl))
                {
                    var lineRule = spacing.Attribute(W + "lineRule")?.Value;
                    lineSpacingAbsolute = lineRule == "exact" || lineRule == "atLeast";
                    lineSpacingExact = lineRule == "exact";
                    lineSpacing = lineSpacingAbsolute
                        ? sl / 20f   // absolute value in points
                        : sl / 240f; // multiplier (auto: 240 = single spacing)
                }
            }

            // Indentation (in twips); w:start/w:end are bidi-aware equivalents of w:left/w:right
            // Per OOXML 17.3.1.12: w:leftChars overrides w:left; presence of either marks indent as paragraph-set.
            var ind = pPr.Element(W + "ind");
            if (ind != null)
            {
                var leftCharsAttr = ind.Attribute(W + "leftChars") ?? ind.Attribute(W + "startChars");
                var leftAttr = ind.Attribute(W + "left") ?? ind.Attribute(W + "start");
                int leftCharsVal = 0;
                bool hasLeftCharsNonZero = leftCharsAttr != null
                    && int.TryParse(leftCharsAttr.Value, out leftCharsVal)
                    && leftCharsVal != 0;
                int leftTwips = 0;
                bool hasLeftTwips = leftAttr != null && int.TryParse(leftAttr.Value, out leftTwips);
                if (hasLeftTwips)
                {
                    // When both w:left and w:leftChars are present, Word uses the
                    // cached explicit twip value (w:left). Word recalculates leftChars
                    // on save, so the explicit twip measurement is what was last laid out.
                    indentLeft = leftTwips / 20f;
                    paraHasIndentLeft = true;
                    paraHasExplicitListIndent = true;
                }
                else if (hasLeftCharsNonZero)
                {
                    // leftChars=N (non-zero) means N/100 East-Asian character widths.
                    indentLeft = (leftCharsVal / 100f) * 11f;
                    paraHasIndentLeft = true;
                }
                // leftChars="0" alone (no numeric left attribute) does NOT lock the
                // indent. Per OOXML 17.3.1.12, leftChars only suppresses chars-based
                // indent; the numerical left value continues to inherit from style
                // and (as final fallback) from numbering.
                var rightCharsAttr = ind.Attribute(W + "rightChars") ?? ind.Attribute(W + "endChars");
                var rightAttr = ind.Attribute(W + "right") ?? ind.Attribute(W + "end");
                int rightCharsVal = 0;
                bool hasRightCharsNonZero = rightCharsAttr != null
                    && int.TryParse(rightCharsAttr.Value, out rightCharsVal)
                    && rightCharsVal != 0;
                if (hasRightCharsNonZero)
                {
                    indentRight = (rightCharsVal / 100f) * 11f;
                    paraHasIndentRight = true;
                }
                else if (rightAttr != null && int.TryParse(rightAttr.Value, out var ir))
                {
                    indentRight = ir / 20f;
                    paraHasIndentRight = true;
                }
                // rightChars="0" alone: see comment for leftChars="0" above.
                var flAttr = ind.Attribute(W + "firstLine");
                if (flAttr != null && int.TryParse(flAttr.Value, out var fl))
                {
                    indentFirstLine = fl / 20f;
                    paraHasIndentFirstLine = true;
                }
                var hgAttr = ind.Attribute(W + "hanging");
                if (hgAttr != null && int.TryParse(hgAttr.Value, out var hg))
                {
                    indentFirstLine = -hg / 20f;
                    paraHasIndentFirstLine = true;
                    paraHasExplicitListIndent = true;
                }
                var flcAttr = ind.Attribute(W + "firstLineChars");
                if (flcAttr != null && int.TryParse(flcAttr.Value, out var flc))
                {
                    firstLineChars = flc;
                    firstLineCharsExplicit = true;
                }
            }

            // Page break before (respect w:val="false" / w:val="0" to disable)
            var pbBefore = pPr.Element(W + "pageBreakBefore");
            if (pbBefore != null)
            {
                var pbVal = pbBefore.Attribute(W + "val")?.Value;
                if (pbVal is null or not ("0" or "false"))
                    pageBreakBefore = true;
            }

            // Snap to grid (defaults to true; false opts out of document grid)
            if (pPr.Element(W + "snapToGrid")?.Attribute(W + "val")?.Value == "0")
                snapToGrid = false;

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
                        var lvlDef2 = numDef.Levels.FirstOrDefault(l => l.Ilvl == listLevel) ?? numDef.Levels.FirstOrDefault();
                        listText = MapBulletChar(lvlDef2?.LvlText, lvlDef2?.FontName);
                        listFontName = lvlDef2?.FontName;
                    }
                    else
                    {
                        isNumberedList = true;
                        listText = numDef.FormatListText(listLevel);
                    }

                    // Capture numbering level indentation; apply later as the LOWEST
                    // priority fallback (paragraph > style > numbering per OOXML cascade)
                    var lvlDef = numDef.Levels.FirstOrDefault(l => l.Ilvl == listLevel) ?? numDef.Levels.FirstOrDefault();
                    if (lvlDef != null)
                    {
                        if (lvlDef.Bold) listTextBold = true;
                        if (lvlDef.IndentLeft > 0) numLevelIndentLeft = lvlDef.IndentLeft;
                        if (lvlDef.Hanging > 0) numLevelIndentFirstLine = -lvlDef.Hanging;
                        listNumFmt = lvlDef.NumFmt;
                        listSuff = lvlDef.Suff;
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
                // Paragraph mark underline
                var pMarkU = rPr.Element(W + "u");
                if (pMarkU != null)
                {
                    var pMarkUVal = pMarkU.Attribute(W + "val")?.Value;
                    if (!string.IsNullOrEmpty(pMarkUVal) && pMarkUVal != "none")
                        paragraphMarkUnderline = true;
                }
                // Paragraph mark font name (used for empty paragraph line height)
                var pMarkRFonts = rPr.Element(W + "rFonts");
                if (pMarkRFonts != null)
                {
                    var pMarkFont = pMarkRFonts.Attribute(W + "ascii")?.Value
                                 ?? pMarkRFonts.Attribute(W + "hAnsi")?.Value;
                    if (!string.IsNullOrEmpty(pMarkFont))
                        paragraphMarkFontName = pMarkFont;
                }
            }
        }

        // Apply style defaults (fall back to Normal style if no explicit style)
        var effectiveStyleId = !string.IsNullOrEmpty(styleId) ? styleId : "Normal";
        bool contextualSpacing = false;
        string? paragraphFontName = null;
        bool styleProvidedIndentLeft = false;
        if (styles.TryGetValue(effectiveStyleId, out var styleInfo))
        {
            if (fontSize == 0) fontSize = styleInfo.FontSize;
            if (!bold) bold = styleInfo.Bold;
            if (!italic) italic = styleInfo.Italic;
            if (!caps) caps = styleInfo.Caps;
            if (color == null) color = styleInfo.Color;
            if (!hasExplicitAlignment && !string.IsNullOrEmpty(styleInfo.Alignment))
                alignment = styleInfo.Alignment;
            if (spacingBefore < 0)
            {
                spacingBefore = styleInfo.SpacingBefore;
                // Treat as explicit when inherited from a non-Normal pStyle —
                // i.e., the document author chose a custom paragraph style
                // (e.g., TableHeading) that defines its own spacing-before.
                // Inherited Normal/docDefault spacing-before stays non-explicit
                // so it can be suppressed at the top of a table cell to match
                // LibreOffice/Word collapse behaviour.
                if (styleInfo.SpacingBefore > 0
                    && !string.IsNullOrEmpty(styleId)
                    && !string.Equals(styleId, "Normal", StringComparison.OrdinalIgnoreCase))
                {
                    spacingBeforeExplicit = true;
                }
            }
            if (spacingAfter < 0) spacingAfter = styleInfo.SpacingAfter;
            if (lineSpacing == 0 && styleInfo.LineSpacing > 0)
            {
                lineSpacing = styleInfo.LineSpacing;
                lineSpacingAbsolute = styleInfo.LineSpacingAbsolute;
                lineSpacingExact = styleInfo.LineSpacingExact;
            }
            contextualSpacing = styleInfo.ContextualSpacing;
            paragraphFontName = styleInfo.FontName;
            if (charSpacing == 0) charSpacing = styleInfo.CharSpacing;

            // Inherit indents per-attribute from style if paragraph didn't set them
            if (!paraHasIndentLeft && styleInfo.HasIndentLeft) { indentLeft = styleInfo.IndentLeft; styleProvidedIndentLeft = true; }
            if (!paraHasIndentRight && styleInfo.HasIndentRight) indentRight = styleInfo.IndentRight;
            if (!paraHasIndentFirstLine && styleInfo.HasIndentFirstLine) indentFirstLine = styleInfo.IndentFirstLine;
        }
        // Numbering-level indent. Per OOXML 17.9.3, when a paragraph references a
        // numbering definition (numPr), the numbering's ind overrides any ind
        // inherited from the paragraph's style; only ind set directly on the
        // paragraph overrides numbering. For non-numbered paragraphs, numbering
        // ind is the lowest-priority fallback (only when neither paragraph nor
        // style set the value). Verified against MS Word output for CCU_article
        // TOC3-styled "4.7" entry whose numbering supplies ind left=739
        // hanging=538 and style supplies ind left=141 only — Word renders the
        // label hanging at the numbering's left/hanging slot, not the style's
        // narrow left=141.
        bool paragraphIsNumbered = numLevelIndentLeft > 0 || numLevelIndentFirstLine != 0;
        if (paragraphIsNumbered && !paraHasIndentLeft && numLevelIndentLeft > 0)
            indentLeft = numLevelIndentLeft;
        else if (!paraHasIndentLeft && indentLeft == 0 && numLevelIndentLeft > 0)
            indentLeft = numLevelIndentLeft;
        if (paragraphIsNumbered && !paraHasIndentFirstLine && numLevelIndentFirstLine != 0)
            indentFirstLine = numLevelIndentFirstLine;
        else if (!paraHasIndentFirstLine && indentFirstLine == 0 && numLevelIndentFirstLine != 0
            && !styleProvidedIndentLeft)
            indentFirstLine = numLevelIndentFirstLine;
        // Paragraph mark font overrides style font for line height calculation
        if (!string.IsNullOrEmpty(paragraphMarkFontName))
            paragraphFontName = paragraphMarkFontName;
        // Paragraph-level contextualSpacing overrides style
        if (pPr?.Element(W + "contextualSpacing") != null)
            contextualSpacing = true;
        // keepNext: paragraph-level pPr overrides style; absence of pPr element means use style default
        bool keepNext = styles.TryGetValue(effectiveStyleId, out var kni) && kni.KeepNext;
        if (pPr?.Element(W + "keepNext") is { } knEl)
            keepNext = knEl.Attribute(W + "val")?.Value is not ("0" or "false");
        // Heuristic: a top-level CJK-numbered heading paragraph (e.g. "一、", "（一）") with
        // an explicit spacing-before is structurally a section heading. Word/LibreOffice
        // typically keep such headings with the next body paragraph; if there isn't room
        // for the heading + at least 2 lines of follow-up content, the heading is pushed
        // to the next page. Mark these as keep-with-next so DocxToPdfConverter applies
        // its existing widow/orphan logic.
        if (!keepNext && isNumberedList && listLevel == 0 && spacingBefore > 0
            && listNumFmt is "taiwaneseCountingThousand" or "taiwaneseCounting"
                or "ideographTraditional" or "chineseCounting" or "chineseCountingThousand"
                or "japaneseCounting")
        {
            keepNext = true;
        }
        // autoSpaceDE: auto spacing between Latin text and East Asian text (default true per OOXML)
        bool autoSpaceDE = true;
        if (pPr?.Element(W + "autoSpaceDE")?.Attribute(W + "val")?.Value is "0" or "false")
            autoSpaceDE = false;
        // autoSpaceDN: auto spacing between numbers and East Asian text (default true per OOXML)
        bool autoSpaceDN = true;
        if (pPr?.Element(W + "autoSpaceDN")?.Attribute(W + "val")?.Value is "0" or "false")
            autoSpaceDN = false;
        if (spacingBefore < 0) spacingBefore = 0;
        if (spacingAfter < 0) spacingAfter = 0;

        // Convert character-based first-line indent now that fontSize is resolved
        if (firstLineChars != 0 && indentFirstLine == 0)
        {
            var charSize = fontSize > 0 ? fontSize : 10.5f;
            indentFirstLine = firstLineChars / 100f * charSize;
        }
        // Explicit w:firstLineChars="0" on the paragraph suppresses any inherited
        // POSITIVE first-line indent (from style); it does NOT zero out a hanging
        // indent (negative firstLine) inherited from numbering. Word's cascade
        // treats firstLineChars as the East-Asian recomputation of the chars-based
        // positive indent, but the auto-numbering hanging continues to apply
        // (verified against MS Word output for CJK ListParagraph paragraphs that
        // use <w:ind w:firstLineChars="0"/> with a numbering hanging — the label
        // still hangs at the left margin and the body sits at the indent left).
        // Explicit w:firstLineChars="0" on the paragraph suppresses any inherited
        // POSITIVE first-line indent (from style); it does NOT zero out a hanging
        // indent (negative firstLine) inherited from numbering. Word's cascade
        // treats firstLineChars as the East-Asian recomputation of the chars-based
        // positive indent, but the auto-numbering hanging continues to apply
        // (verified against MS Word output for CJK ListParagraph paragraphs that
        // use <w:ind w:firstLineChars="0"/> with a numbering hanging — the label
        // still hangs at the left margin and the body sits at the indent left).
        if (firstLineCharsExplicit && firstLineChars == 0 && !paraHasIndentFirstLine
            && indentFirstLine > 0)
        {
            // Restore numbering hanging (negative) if any; otherwise zero.
            indentFirstLine = numLevelIndentFirstLine < 0 ? numLevelIndentFirstLine : 0;
            paraHasIndentFirstLine = true;
        }

        // Read runs (with field code tracking)
        int fieldDepth = 0;
        bool inFieldInstr = false; // between begin and separate
        string currentFieldInstr = ""; // accumulated field instruction text
        bool fieldResultEmitted = false; // whether a PAGE/NUMPAGES placeholder was emitted between separate and end
        bool pendingMidParaBreak = false; // page break found with no content runs before it
        foreach (var child in UnwrapSdt(pElement.Elements()))
        {
            if (child.Name == W + "r")
            {
                // Track field codes
                var fldChar = child.Element(W + "fldChar");
                if (fldChar != null)
                {
                    var fldType = fldChar.Attribute(W + "fldCharType")?.Value;
                    if (fldType == "begin") { fieldDepth++; inFieldInstr = true; currentFieldInstr = ""; fieldResultEmitted = false; continue; }
                    if (fldType == "separate") { inFieldInstr = false; fieldResultEmitted = false; continue; }
                    if (fldType == "end")
                    {
                        // Emit PAGE/NUMPAGES placeholder if no result text was found between separate and end
                        if (!fieldResultEmitted && fieldDepth > 0 && !inFieldInstr)
                        {
                            var ft = GetFieldInstructionType(currentFieldInstr);
                            if (ft == "PAGE" || ft == "NUMPAGES")
                            {
                                var ph = ft == "PAGE" ? "{PAGE}" : "{NUMPAGES}";
                                if (ft == "PAGE")
                                {
                                    var instr = currentFieldInstr.Trim();
                                    if (instr.Contains("\\* roman", StringComparison.OrdinalIgnoreCase))
                                        ph = instr.Contains("\\* ROMAN") ? "{PAGE:ROMAN}" : "{PAGE:roman}";
                                }
                                runs.Add(new DocxRun(ph, bold, italic, fontSize, color));
                            }
                        }
                        fieldDepth--;
                        if (fieldDepth <= 0) { fieldDepth = 0; inFieldInstr = false; }
                        currentFieldInstr = "";
                        continue;
                    }
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
                    var fieldType = GetFieldInstructionType(currentFieldInstr);
                    if (fieldType == "PAGE" || fieldType == "NUMPAGES")
                    {
                        // Detect format switches like \* roman, \* ROMAN, \* arabic
                        var placeholder = fieldType == "PAGE" ? "{PAGE}" : "{NUMPAGES}";
                        if (fieldType == "PAGE")
                        {
                            var instr = currentFieldInstr.Trim();
                            if (instr.Contains("\\* roman", StringComparison.OrdinalIgnoreCase))
                                placeholder = instr.Contains("\\* ROMAN") ? "{PAGE:ROMAN}" : "{PAGE:roman}";
                        }
                        var rPr = child.Element(W + "rPr");
                        var fBold = bold; var fItalic = italic; var fSize = fontSize; var fColor = color;
                        if (rPr != null)
                        {
                            if (rPr.Element(W + "b") != null) fBold = true;
                            var sz = rPr.Element(W + "sz")?.Attribute(W + "val")?.Value;
                            if (float.TryParse(sz, out var s) && s > 0) fSize = s / 2f;
                        }
                        runs.Add(new DocxRun(placeholder, fBold, fItalic, fSize, fColor));
                        fieldResultEmitted = true;
                    }
                    continue;
                }

                // Detect lastRenderedPageBreak: Word's hint that a page break occurred
                // at this position in the last rendering pass.  Only honour it when the
                // marker appears before any visible content in the paragraph (i.e. a
                // paragraph-level page break, not a mid-paragraph line break).
                if (!hasLastRenderedPageBreak && runs.Count == 0 && images.Count == 0
                    && child.Element(W + "lastRenderedPageBreak") != null)
                {
                    hasLastRenderedPageBreak = true;
                }

                var run = ReadRun(child, bold, italic, fontSize, color, caps, charSpacing, paragraphFontName, defaultLatinFontName, defaultEastAsiaFontName, styles);
                if (run != null)
                {
                    if (run.IsPageBreak)
                    {
                        if (runs.Count == 0 && images.Count == 0)
                            pendingMidParaBreak = true; // defer: might become pageBreakBefore
                        else
                            pageBreakAfter = true;
                    }
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
                    var run = ReadRun(r, bold, italic, fontSize, color, caps, charSpacing, paragraphFontName, defaultLatinFontName, defaultEastAsiaFontName, styles);
                    if (run != null)
                        runs.Add(run);
                }
            }
            else if (child.Name == M + "oMathPara" || child.Name == M + "oMath")
            {
                // Linearize Office Math (OMML) elements into compact inline text.
                var mathText = LinearizeOmml(child);
                if (!string.IsNullOrEmpty(mathText))
                    runs.Add(new DocxRun(mathText, bold, italic, fontSize, color));
            }
        }

        // Resolve deferred mid-paragraph page break: if content runs followed the
        // break, treat as page-break-before so the content starts on a new page.
        // If no content followed (empty paragraph with only a break run), fall back
        // to page-break-after to preserve the original page transition semantics.
        if (pendingMidParaBreak)
        {
            if (runs.Count > 0 || images.Count > 0)
                pageBreakBefore = true;
            else
                pageBreakAfter = true;
        }

        // Detect section break (sectPr inside pPr)
        DocxPageLayout? sectionBreakLayout = null;
        var sectPr = pPr?.Element(W + "sectPr");
        if (sectPr != null)
            sectionBreakLayout = ParseSectionProperties(sectPr);

        // If paragraph has no runs and no images, represent as empty paragraph for spacing
        return new DocxParagraph(runs, images, alignment, spacingBefore, spacingAfter,
            lineSpacing, lineSpacingAbsolute, lineSpacingExact, indentLeft, indentRight, indentFirstLine,
            isBulletList, isNumberedList, listLevel, listText, listTextBold, styleId,
            bold, italic, fontSize, color, pageBreakBefore, pageBreakAfter, paragraphShading, tabStops,
            sectionBreakLayout, borders, shapes.Count > 0 ? shapes : null,
            ContextualSpacing: contextualSpacing, SnapToGrid: snapToGrid,
            ParagraphMarkUnderline: paragraphMarkUnderline,
            ParagraphFontName: paragraphFontName,
            KeepNext: keepNext,
            AutoSpaceDE: autoSpaceDE,
            AutoSpaceDN: autoSpaceDN,
            HasLastRenderedPageBreak: hasLastRenderedPageBreak,
            ListFontName: listFontName,
            SpacingAfterExplicit: spacingAfterExplicit,
            SpacingBeforeExplicit: spacingBeforeExplicit,
            HasExplicitListIndent: paraHasExplicitListIndent,
            ListSuff: listSuff);
    }

    /// <summary>
    /// Linearize OMML (Office Math Markup Language) element into compact inline text.
    /// Handles fractions, subscripts, superscripts, summations, and delimiters.
    /// </summary>
    private static string LinearizeOmml(XElement element)
    {
        var sb = new System.Text.StringBuilder();
        LinearizeOmmlCore(element, sb);
        return sb.ToString();
    }

    private static void LinearizeOmmlCore(XElement el, System.Text.StringBuilder sb)
    {
        if (el.Name == M + "f") // Fraction: num/den
        {
            var num = el.Element(M + "num");
            var den = el.Element(M + "den");
            if (num != null) LinearizeOmmlCore(num, sb);
            sb.Append('/');
            if (den != null) LinearizeOmmlCore(den, sb);
            return;
        }
        if (el.Name == M + "nary") // N-ary (summation, product, etc.)
        {
            var naryPr = el.Element(M + "naryPr");
            var chr = naryPr?.Element(M + "chr")?.Attribute(M + "val")?.Value ?? "\u2211"; // default Σ
            var sub = el.Element(M + "sub");
            var sup = el.Element(M + "sup");
            var e = el.Element(M + "e");
            sb.Append(chr);
            if (sub != null) { sb.Append('('); LinearizeOmmlCore(sub, sb); sb.Append(')'); }
            if (sup != null) { sb.Append("^("); LinearizeOmmlCore(sup, sb); sb.Append(')'); }
            sb.Append(' ');
            if (e != null) LinearizeOmmlCore(e, sb);
            return;
        }
        if (el.Name == M + "d") // Delimiter (parentheses, brackets)
        {
            var dPr = el.Element(M + "dPr");
            var begChr = dPr?.Element(M + "begChr")?.Attribute(M + "val")?.Value ?? "(";
            var endChr = dPr?.Element(M + "endChr")?.Attribute(M + "val")?.Value ?? ")";
            sb.Append(begChr);
            foreach (var de in el.Elements(M + "e"))
                LinearizeOmmlCore(de, sb);
            sb.Append(endChr);
            return;
        }
        if (el.Name == M + "sSub") // Subscript
        {
            var e = el.Element(M + "e");
            var sub = el.Element(M + "sub");
            if (e != null) LinearizeOmmlCore(e, sb);
            if (sub != null) LinearizeOmmlCore(sub, sb);
            return;
        }
        if (el.Name == M + "sSup") // Superscript
        {
            var e = el.Element(M + "e");
            var sup = el.Element(M + "sup");
            if (e != null) LinearizeOmmlCore(e, sb);
            if (sup != null) { sb.Append('^'); LinearizeOmmlCore(sup, sb); }
            return;
        }
        if (el.Name == M + "sSubSup") // Sub-superscript
        {
            var e = el.Element(M + "e");
            var sub = el.Element(M + "sub");
            var sup = el.Element(M + "sup");
            if (e != null) LinearizeOmmlCore(e, sb);
            if (sub != null) LinearizeOmmlCore(sub, sb);
            if (sup != null) { sb.Append('^'); LinearizeOmmlCore(sup, sb); }
            return;
        }
        if (el.Name == M + "r") // Math run: extract text
        {
            foreach (var mt in el.Elements(M + "t"))
            {
                if (!string.IsNullOrEmpty(mt.Value))
                    sb.Append(mt.Value);
            }
            return;
        }
        if (el.Name == M + "func") // Function (sin, cos, lim, etc.)
        {
            var fName = el.Element(M + "fName");
            var e = el.Element(M + "e");
            if (fName != null) LinearizeOmmlCore(fName, sb);
            if (e != null) LinearizeOmmlCore(e, sb);
            return;
        }
        if (el.Name == M + "rad") // Radical (square root)
        {
            sb.Append("\u221A(");
            var e = el.Element(M + "e");
            if (e != null) LinearizeOmmlCore(e, sb);
            sb.Append(')');
            return;
        }
        // For container elements (oMathPara, oMath, e, num, den, sub, sup, etc.), recurse into children
        foreach (var child in el.Elements())
            LinearizeOmmlCore(child, sb);
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

    /// <summary>Like <see cref="ReadBorderEdge"/> but returns
    /// <see cref="DocxBorderEdge.Nil"/> for explicit val="nil"/"none" so callers
    /// can distinguish "explicitly suppressed" from "not specified".</summary>
    private static DocxBorderEdge? ReadBorderEdgeAllowNil(XElement? el)
    {
        if (el == null) return null;
        var val = el.Attribute(W + "val")?.Value;
        if (val == "none" || val == "nil") return DocxBorderEdge.Nil;
        return ReadBorderEdge(el);
    }

    /// <summary>Layers <paramref name="over"/> on top of <paramref name="under"/>
    /// per OOXML conditional formatting precedence: a non-null edge in
    /// <paramref name="over"/> wins; otherwise the edge from <paramref name="under"/>
    /// is preserved (including <see cref="DocxBorderEdge.Nil"/> sentinels).</summary>
    private static DocxBorders? MergeBorders(DocxBorders? under, DocxBorders? over)
    {
        if (over == null) return under;
        if (under == null) return over;
        return new DocxBorders(
            Top: over.Top ?? under.Top,
            Bottom: over.Bottom ?? under.Bottom,
            Left: over.Left ?? under.Left,
            Right: over.Right ?? under.Right
        );
    }

    private static DocxRun? ReadRun(XElement rElement, bool parentBold, bool parentItalic, float parentFontSize, PdfColor? parentColor, bool parentCaps = false, float parentCharSpacing = 0, string? parentFontName = null, string? defaultLatinFontName = null, string? defaultEastAsiaFontName = null, Dictionary<string, DocxStyleInfo>? styles = null)
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
        var hasExplicitUnderlineDecl = false;
        var charSpacing = parentCharSpacing;
        var fontName = parentFontName;
        PdfColor? runShading = null;
        float verticalPosition = 0;

        // Resolve character style (rStyle) defaults before reading inline overrides
        string? rStyleVertAlign = null;
        if (rPr != null && styles != null)
        {
            var rStyleId = rPr.Element(W + "rStyle")?.Attribute(W + "val")?.Value;
            if (!string.IsNullOrEmpty(rStyleId) && styles.TryGetValue(rStyleId, out var charStyle))
            {
                if (charStyle.FontSize > 0) fontSize = charStyle.FontSize;
                if (charStyle.Bold) bold = true;
                if (charStyle.Italic) italic = true;
                if (charStyle.Caps) caps = true;
                if (charStyle.Color != null) color = charStyle.Color;
                if (!string.IsNullOrWhiteSpace(charStyle.FontName)) fontName = charStyle.FontName;
                if (charStyle.CharSpacing != 0) charSpacing = charStyle.CharSpacing;
                rStyleVertAlign = charStyle.VerticalAlign;
            }
        }

        if (rPr != null)
        {
            var bEl = rPr.Element(W + "b");
            if (bEl != null)
            {
                var bVal = bEl.Attribute(W + "val")?.Value;
                bold = bVal is not ("0" or "false");
            }
            var iEl = rPr.Element(W + "i");
            if (iEl != null)
            {
                var iVal = iEl.Attribute(W + "val")?.Value;
                italic = iVal is not ("0" or "false");
            }
            var capsEl = rPr.Element(W + "caps");
            if (capsEl != null)
            {
                var capsVal = capsEl.Attribute(W + "val")?.Value;
                caps = capsVal is not ("0" or "false");
            }
            var uEl = rPr.Element(W + "u");
            if (uEl != null)
            {
                hasExplicitUnderlineDecl = true;
                var uVal = uEl.Attribute(W + "val")?.Value;
                if (!string.IsNullOrEmpty(uVal) && uVal != "none")
                    underline = true;
            }
            var sz = rPr.Element(W + "sz")?.Attribute(W + "val")?.Value;
            if (float.TryParse(sz, out var s) && s > 0)
                fontSize = s / 2f; // half-points to points
            var colorEl = rPr.Element(W + "color");
            if (colorEl != null)
            {
                var colorVal = colorEl.Attribute(W + "val")?.Value;
                if (!string.IsNullOrEmpty(colorVal) && colorVal != "auto")
                    color = PdfColor.FromHex(colorVal);
                else
                    color = parentColor; // "auto" resets to paragraph default, overriding character style
            }
            runShading = ReadRunShading(rPr.Element(W + "shd"));
            // Character spacing (w:spacing w:val in twips)
            var spacingEl = rPr.Element(W + "spacing");
            if (spacingEl != null && int.TryParse(spacingEl.Attribute(W + "val")?.Value, out var cs))
                charSpacing = cs / 20f; // twips to points

            fontName = ResolveRunFontName(rPr, parentFontName, defaultLatinFontName, defaultEastAsiaFontName);

            // Vertical position (w:position w:val in half-points)
            var posEl = rPr.Element(W + "position");
            if (posEl != null && float.TryParse(posEl.Attribute(W + "val")?.Value, out var posVal))
                verticalPosition = posVal / 2f; // half-points to points
        }

        // Collect text from <w:t>, <w:tab>, <w:br> elements
        bool isPageBreak = false;
        bool isColumnBreak = false;
        string? footnoteId = null;
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
                else if (brType == "column")
                    isColumnBreak = true;
                else
                    text += "\n";
            }
            else if (child.Name == W + "footnoteReference")
            {
                // Footnote reference mark in the document body
                footnoteId = child.Attribute(W + "id")?.Value;
                if (!string.IsNullOrEmpty(footnoteId))
                    text += footnoteId; // Use id as display number (sequential for normal footnotes)
            }
            else if (child.Name == W + "footnoteRef")
            {
                // Footnote self-reference inside footnote text – skip in body parsing.
                // ReadFootnotes() handles this element separately.
            }
        }

        // Handle w:vertAlign for superscript/subscript
        // Inline rPr takes precedence over rStyle-inherited vertAlign
        string? effectiveVertAlign = null;
        if (rPr != null)
        {
            var vertAlignEl = rPr.Element(W + "vertAlign");
            if (vertAlignEl != null)
                effectiveVertAlign = vertAlignEl.Attribute(W + "val")?.Value;
        }
        if (effectiveVertAlign == null) effectiveVertAlign = rStyleVertAlign;
        if (effectiveVertAlign == "superscript")
        {
            verticalPosition = fontSize * 0.33f;
            fontSize *= 0.58f;
        }
        else if (effectiveVertAlign == "subscript")
        {
            verticalPosition = -fontSize * 0.2f;
            fontSize *= 0.58f;
        }

        if (string.IsNullOrEmpty(text) && !isPageBreak && !isColumnBreak)
            return null;

        if (caps && !string.IsNullOrEmpty(text))
            text = text.ToUpperInvariant();

        // Guard against glyph drop: if run contains CJK text but the resolved run font
        // looks like a Latin-only family, use EastAsia default font instead.
        if (!string.IsNullOrEmpty(text)
            && ContainsCjkText(text)
            && !IsKnownEastAsianFont(fontName)
            && !string.IsNullOrWhiteSpace(defaultEastAsiaFontName))
        {
            fontName = defaultEastAsiaFontName;
        }

        // Symmetric guard: w:hint="eastAsia" forces ResolveRunFontName to pick the
        // eastAsia face (e.g. SimSun) even for a pure-Latin run that explicitly
        // names ascii=Arial.  In Word, hint=eastAsia is only meant to disambiguate
        // characters that fall in mixed Unicode ranges — unambiguously Latin text
        // should still render in the ascii font.  Re-prefer the ascii font when
        // the run text contains no CJK characters and an explicit ascii (or hAnsi)
        // attribute is present.
        if (!string.IsNullOrEmpty(text)
            && !ContainsCjkText(text)
            && rPr != null)
        {
            var rFontsLatin = rPr.Element(W + "rFonts");
            var hintLatin = rFontsLatin?.Attribute(W + "hint")?.Value;
            if (string.Equals(hintLatin, "eastAsia", StringComparison.OrdinalIgnoreCase)
                && IsKnownEastAsianFont(fontName))
            {
                var asciiName = rFontsLatin?.Attribute(W + "ascii")?.Value
                               ?? rFontsLatin?.Attribute(W + "hAnsi")?.Value;
                if (!string.IsNullOrWhiteSpace(asciiName) && !IsKnownEastAsianFont(asciiName))
                    fontName = PdfWriter.MaybeFallbackForMissingFont(asciiName);
            }
        }

        // Some documents express weight via the font family itself (e.g. "Montserrat ExtraBold",
        // "Open Sans SemiBold") instead of a <w:b/> element. When MiniPdf falls back to its
        // built-in fonts it would otherwise render those runs in regular weight. Detect a bold-ish
        // weight in the family name and promote the run to bold so the visual weight is preserved.
        if (!bold && FontNameImpliesBold(fontName))
            bold = true;
        if (!italic && FontNameImpliesItalic(fontName))
            italic = true;

        // Final guarantee: every run has a non-empty FontName so the PdfWriter font-embedding
        // pipeline can route it through MaybeFallbackForMissingFont (e.g. Avenir → TNR).
        if (string.IsNullOrEmpty(fontName) && !string.IsNullOrEmpty(defaultLatinFontName))
            fontName = PdfWriter.MaybeFallbackForMissingFont(defaultLatinFontName);

        return new DocxRun(text, bold, italic, fontSize, color, isPageBreak, underline, charSpacing, fontName, hasExplicitUnderlineDecl, isColumnBreak, verticalPosition, footnoteId, runShading);
    }

    private static PdfColor? ReadRunShading(XElement? shdEl)
    {
        if (shdEl == null) return null;

        var fill = shdEl.Attribute(W + "fill")?.Value;
        if (string.IsNullOrEmpty(fill) || fill == "auto")
            fill = "FFFFFF";

        var baseColor = PdfColor.FromHex(fill);
        var val = shdEl.Attribute(W + "val")?.Value;
        if (!string.IsNullOrEmpty(val) && val.StartsWith("pct", StringComparison.OrdinalIgnoreCase)
            && int.TryParse(val.Substring(3), out var pct) && pct > 0)
        {
            var amount = Math.Min(100, pct) / 100f;
            return new PdfColor(
                baseColor.R * (1f - amount),
                baseColor.G * (1f - amount),
                baseColor.B * (1f - amount));
        }

        return baseColor;
    }

    private static bool FontNameImpliesBold(string? fontName)
    {
        if (string.IsNullOrEmpty(fontName))
            return false;
        var fn = fontName.ToUpperInvariant();
        // "Bold" covers Bold/ExtraBold/SemiBold/DemiBold/UltraBold; "Black"/"Heavy" are heavier than bold.
        // "Medium" sits between regular and bold; map to bold to better approximate visual weight
        // when only regular/bold weights are available in the fallback font.
        return fn.Contains("BOLD") || fn.Contains("BLACK") || fn.Contains("HEAVY") || fn.Contains("MEDIUM");
    }

    private static bool FontNameImpliesItalic(string? fontName)
    {
        if (string.IsNullOrEmpty(fontName))
            return false;
        var fn = fontName.ToUpperInvariant();
        return fn.Contains("ITALIC") || fn.Contains("OBLIQUE");
    }

    private static string? GetFieldInstructionType(string? instruction)
    {
        if (string.IsNullOrWhiteSpace(instruction))
            return null;

        // Field instructions may include switches, e.g. "PAGE \\* MERGEFORMAT".
        // We only need the first token to determine the field type.
        var normalized = instruction.Trim();
        var firstToken = normalized.Split(new[] { ' ', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
            .FirstOrDefault();
        if (string.IsNullOrEmpty(firstToken))
            return null;

        firstToken = firstToken.ToUpperInvariant();
        return firstToken is "PAGE" or "NUMPAGES" ? firstToken : null;
    }

    private static bool ContainsCjkText(string text)
    {
        foreach (var ch in text)
        {
            if ((ch >= '\u4E00' && ch <= '\u9FFF')    // CJK Unified Ideographs
                || (ch >= '\u3400' && ch <= '\u4DBF') // CJK Extension A
                || (ch >= '\u3040' && ch <= '\u30FF') // Hiragana / Katakana
                || (ch >= '\uAC00' && ch <= '\uD7AF')) // Hangul syllables
                return true;
        }
        return false;
    }

    private static bool IsKnownEastAsianFont(string? fontName)
    {
        if (string.IsNullOrWhiteSpace(fontName))
            return false;

        var n = fontName.ToLowerInvariant();
        return n.Contains("mingliu")
            || n.Contains("pmingliu")
            || n.Contains("kai")
            || n.Contains("dfkai")
            || n.Contains("bkai")
            || n.Contains("jhenghei")
            || n.Contains("yahei")
            || n.Contains("msjh")
            || n.Contains("msyh")
            || n.Contains("simsun")
            || n.Contains("simhei")
            || n.Contains("黑体")
            || n.Contains("pingfang")
            || n.Contains("heiti")
            || n.Contains("song")
            || n.Contains("宋体")
            || n.Contains("宋體")
            || n.Contains("noto sans cjk")
            || n.Contains("新細明體")
            || n.Contains("新细明体")
            || n.Contains("細明體")
            || n.Contains("细明体")
            || n.Contains("標楷體")
            || n.Contains("标楷体")
            || n.Contains("微軟正黑體")
            || n.Contains("微软正黑体")
            || n.Contains("微軟雅黑")
            || n.Contains("微软雅黑");
    }

    private static string? ResolveRunFontName(XElement rPr, string? parentFontName = null, string? defaultLatinFontName = null, string? defaultEastAsiaFontName = null)
    {
        var rFonts = rPr.Element(W + "rFonts");
        if (rFonts == null)
            return parentFontName;

        // For CJK runs, eastAsia should win (Word commonly sets hint=eastAsia).
        var hint = rFonts.Attribute(W + "hint")?.Value;
        if (string.Equals(hint, "eastAsia", StringComparison.OrdinalIgnoreCase))
        {
            var eastHint = rFonts.Attribute(W + "eastAsia")?.Value;
            if (!string.IsNullOrWhiteSpace(eastHint))
                return eastHint;

            var eastTheme = rFonts.Attribute(W + "eastAsiaTheme")?.Value;
            if (!string.IsNullOrWhiteSpace(eastTheme))
                return defaultEastAsiaFontName ?? parentFontName ?? defaultLatinFontName;
        }

        // Prefer Latin (ascii/hAnsi) explicit fonts over eastAsia.  CJK characters
        // will still be rendered with the correct East Asian font via PdfWriter's
        // per-character font slot assignment fallback.
        var explicitFont = rFonts.Attribute(W + "ascii")?.Value
            ?? rFonts.Attribute(W + "hAnsi")?.Value
            ?? rFonts.Attribute(W + "eastAsia")?.Value
            ?? rFonts.Attribute(W + "cs")?.Value;
        if (!string.IsNullOrWhiteSpace(explicitFont))
            return PdfWriter.MaybeFallbackForMissingFont(explicitFont);

        var hasLatinTheme = !string.IsNullOrWhiteSpace(rFonts.Attribute(W + "asciiTheme")?.Value)
            || !string.IsNullOrWhiteSpace(rFonts.Attribute(W + "hAnsiTheme")?.Value)
            || !string.IsNullOrWhiteSpace(rFonts.Attribute(W + "cstheme")?.Value);

        var hasEastAsiaTheme = !string.IsNullOrWhiteSpace(rFonts.Attribute(W + "eastAsiaTheme")?.Value)
            || (string.Equals(rFonts.Attribute(W + "asciiTheme")?.Value, "minorEastAsia", StringComparison.OrdinalIgnoreCase)
                || string.Equals(rFonts.Attribute(W + "asciiTheme")?.Value, "majorEastAsia", StringComparison.OrdinalIgnoreCase))
            || (string.Equals(rFonts.Attribute(W + "hAnsiTheme")?.Value, "minorEastAsia", StringComparison.OrdinalIgnoreCase)
                || string.Equals(rFonts.Attribute(W + "hAnsiTheme")?.Value, "majorEastAsia", StringComparison.OrdinalIgnoreCase));

        // Prefer Latin theme when available — this sets the preferred font hint
        // for the text block.  CJK codepoints that aren't covered by the Latin
        // font will fall through to the system CJK font via PdfWriter's fallback.
        if (hasLatinTheme)
            return PdfWriter.MaybeFallbackForMissingFont(defaultLatinFontName ?? parentFontName ?? defaultEastAsiaFontName);

        if (hasEastAsiaTheme)
            return PdfWriter.MaybeFallbackForMissingFont(defaultEastAsiaFontName ?? parentFontName ?? defaultLatinFontName);

        return PdfWriter.MaybeFallbackForMissingFont(parentFontName);
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
        var isBehindDoc = false;
        if (container == null)
        {
            // Fall back to anchor (floating) images, including behind-doc images
            var anchor = drawing.Descendants(WP + "anchor").FirstOrDefault();
            if (anchor != null && anchor.Descendants(A + "blip").Any())
            {
                container = anchor;
                isAnchor = true;
                isBehindDoc = anchor.Attribute("behindDoc")?.Value == "1";
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
        else if (hasCrop && Compat.IsWindows())
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
        string? relFromH = null, relFromV = null;
        if (isAnchor)
        {
            var posH = container.Element(WP + "positionH");
            var posV = container.Element(WP + "positionV");
            if (posH != null)
            {
                relFromH = posH.Attribute("relativeFrom")?.Value;
                var off = posH.Element(WP + "posOffset");
                if (off != null) long.TryParse(off.Value, out offsetXEmu);
            }
            if (posV != null)
            {
                relFromV = posV.Attribute("relativeFrom")?.Value;
                var off = posV.Element(WP + "posOffset");
                if (off != null) long.TryParse(off.Value, out offsetYEmu);
            }
        }

        // Detect wrapTopAndBottom on anchor
        bool isWrapTopBottom = false;
        if (isAnchor && container != null)
            isWrapTopBottom = container.Element(WP + "wrapTopAndBottom") != null;

        // Read alpha from alphaModFix
        float alpha = 1f;
        var alphaModFix = blip.Element(A + "alphaModFix");
        if (alphaModFix != null)
        {
            if (int.TryParse(alphaModFix.Attribute("amt")?.Value, out var amt))
                alpha = amt / 100000f;
        }

        return new DocxImage(data, ext, widthEmu, heightEmu, isAnchor, offsetXEmu, offsetYEmu, isBehindDoc, relFromH, relFromV, isWrapTopBottom, alpha);
    }

    private static byte[]? TryConvertMetafileToPng(byte[] sourceBytes, long widthEmu, long heightEmu,
        float cropL = 0, float cropT = 0, float cropR = 0, float cropB = 0)
    {
        if (!Compat.IsWindows())
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
            targetWidth = Compat.Clamp(targetWidth, 32, 4096);
            targetHeight = Compat.Clamp(targetHeight, 32, 4096);

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

            // Process child shapes (both direct wsp and nested grpSp sub-groups)
            ProcessGroupChildren(grpSp, result, themeColors, grpFillColor, grpFillAlpha,
                anchorOffsetX, anchorOffsetY, chOffX, chOffY, grpExtCx, grpExtCy, chExtCx, chExtCy);
            return result;
        }

        // Fall back to single-shape handling (non-group)
        var spPr = anchor.Descendants(WPS + "spPr").FirstOrDefault()
              ?? anchor.Descendants(A + "spPr").FirstOrDefault();
        if (spPr == null) return result;

        // Check for stroke
        PdfColor? singleStrokeColor = null;
        float singleStrokeWidth = 0;
        var singleLn = spPr.Element(A + "ln");
        if (singleLn != null)
        {
            var lnFill = singleLn.Element(A + "solidFill");
            if (lnFill != null)
            {
                (singleStrokeColor, _) = ResolveSolidFill(lnFill, themeColors);
                float.TryParse(singleLn.Attribute("w")?.Value, out singleStrokeWidth);
            }
        }

        bool singleHasNoFill = spPr.Element(A + "noFill") != null;
        if (singleHasNoFill && singleStrokeColor == null) return result;

        PdfColor? singleColor = null;
        float singleAlpha = 1f;
        var solidFill = spPr.Element(A + "solidFill");
        if (solidFill != null)
        {
            (singleColor, singleAlpha) = ResolveSolidFill(solidFill, themeColors);
        }
        if (singleColor == null && singleStrokeColor == null) return result;

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

        var shapeFill = singleColor ?? new PdfColor(1, 1, 1); // default white if stroke-only
        result.Add(new DocxShape(widthEmu, heightEmu, anchorOffsetX, anchorOffsetY, shapeFill, singleAlpha,
            presetGeom, frameThicknessRatio, customPaths,
            StrokeColor: singleStrokeColor, StrokeWidthEmu: singleStrokeWidth,
            FillOnly: singleColor != null && singleStrokeColor == null,
            HasFill: singleColor != null));
        return result;
    }

    private static void ProcessGroupChildren(
        XElement groupElement, List<DocxShape> result,
        Dictionary<string, string>? themeColors,
        PdfColor? grpFillColor, float grpFillAlpha,
        long anchorOffsetX, long anchorOffsetY,
        long chOffX, long chOffY,
        long grpExtCx, long grpExtCy,
        long chExtCx, long chExtCy)
    {
        foreach (var child in groupElement.Elements())
        {
            if (child.Name == WPS + "wsp")
            {
                ReadSingleWspShape(child, result, themeColors, grpFillColor, grpFillAlpha,
                    anchorOffsetX, anchorOffsetY, chOffX, chOffY, grpExtCx, grpExtCy, chExtCx, chExtCy);
            }
            else if (child.Name == WPG + "grpSp")
            {
                // Nested sub-group: read its own coordinate mapping, then recurse
                var subGrpSpPr = child.Element(WPG + "grpSpPr");
                var subXfrm = subGrpSpPr?.Element(A + "xfrm");
                long subOffX = 0, subOffY = 0, subCx = 1, subCy = 1;
                long subChOffX = 0, subChOffY = 0, subChCx = 1, subChCy = 1;
                if (subXfrm != null)
                {
                    var off = subXfrm.Element(A + "off");
                    if (off != null)
                    {
                        long.TryParse(off.Attribute("x")?.Value, out subOffX);
                        long.TryParse(off.Attribute("y")?.Value, out subOffY);
                    }
                    var ext = subXfrm.Element(A + "ext");
                    if (ext != null)
                    {
                        long.TryParse(ext.Attribute("cx")?.Value, out subCx);
                        long.TryParse(ext.Attribute("cy")?.Value, out subCy);
                    }
                    var cOff = subXfrm.Element(A + "chOff");
                    if (cOff != null)
                    {
                        long.TryParse(cOff.Attribute("x")?.Value, out subChOffX);
                        long.TryParse(cOff.Attribute("y")?.Value, out subChOffY);
                    }
                    var cExt = subXfrm.Element(A + "chExt");
                    if (cExt != null)
                    {
                        long.TryParse(cExt.Attribute("cx")?.Value, out subChCx);
                        long.TryParse(cExt.Attribute("cy")?.Value, out subChCy);
                    }
                }
                if (subChCx == 0) subChCx = 1;
                if (subChCy == 0) subChCy = 1;

                // Sub-group fill inheritance
                PdfColor? subGrpFill = grpFillColor;
                float subGrpAlpha = grpFillAlpha;
                var subFill = subGrpSpPr?.Element(A + "solidFill");
                if (subFill != null)
                    (subGrpFill, subGrpAlpha) = ResolveSolidFill(subFill, themeColors);

                // Map sub-group origin to parent coordinate space then to page EMU
                var mappedAnchorX = anchorOffsetX + (subOffX - chOffX) * grpExtCx / chExtCx;
                var mappedAnchorY = anchorOffsetY + (subOffY - chOffY) * grpExtCy / chExtCy;
                var mappedExtCx = subCx * grpExtCx / chExtCx;
                var mappedExtCy = subCy * grpExtCy / chExtCy;

                ProcessGroupChildren(child, result, themeColors, subGrpFill, subGrpAlpha,
                    mappedAnchorX, mappedAnchorY,
                    subChOffX, subChOffY,
                    mappedExtCx, mappedExtCy,
                    subChCx, subChCy);
            }
        }
    }

    private static void ReadSingleWspShape(
        XElement childWsp, List<DocxShape> result,
        Dictionary<string, string>? themeColors,
        PdfColor? grpFillColor, float grpFillAlpha,
        long anchorOffsetX, long anchorOffsetY,
        long chOffX, long chOffY,
        long grpExtCx, long grpExtCy,
        long chExtCx, long chExtCy)
    {
        var childSpPr = childWsp.Element(WPS + "spPr") ?? childWsp.Element(A + "spPr");
        if (childSpPr == null) return;

        // Read stroke/outline
        PdfColor? strokeColor = null;
        float strokeWidthEmu = 0;
        var ln = childSpPr.Element(A + "ln");
        if (ln != null)
        {
            var lnNoFill = ln.Element(A + "noFill");
            if (lnNoFill == null)
            {
                var lnFill = ln.Element(A + "solidFill");
                if (lnFill != null)
                {
                    (strokeColor, _) = ResolveSolidFill(lnFill, themeColors);
                    float.TryParse(ln.Attribute("w")?.Value, out strokeWidthEmu);
                }
            }
        }

        bool hasNoFill = childSpPr.Element(A + "noFill") != null;

        PdfColor? fillColor = null;
        float alpha = 1f;
        if (!hasNoFill)
        {
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
        }

        // Skip if neither fill nor stroke
        if (fillColor == null && strokeColor == null) return;

        // Get child shape position/size in child coordinate space
        var childXfrm = childSpPr.Element(A + "xfrm");
        if (childXfrm == null) return;
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

        var shapeFill = fillColor ?? new PdfColor(1, 1, 1);
        result.Add(new DocxShape(pageW, pageH, pageX, pageY, shapeFill, alpha,
            childPresetGeom, childFrameThicknessRatio, childCustomPaths,
            StrokeColor: strokeColor, StrokeWidthEmu: strokeWidthEmu,
            FillOnly: fillColor != null && strokeColor == null,
            HasFill: fillColor != null));
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
                        int.TryParse(fmla.Substring(4), out var v))
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
            Compat.Clamp(xNorm, -0.25f, 1.25f),
            Compat.Clamp(yNorm, -0.25f, 1.25f));
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

        var steps = Compat.Clamp((int)Math.Ceiling(Math.Abs(sweepRad) / (Math.PI / 16d)), 4, 96);
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
        nx = Compat.Clamp(nx, -0.25f, 1.25f);
        ny = Compat.Clamp(ny, -0.25f, 1.25f);

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
        var tokens = formula.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
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
                // Per OOXML spec, lumMod/lumOff modify the HSL luminance (L) channel
                // of the source color. Applying the factor/offset linearly to RGB
                // produces a desaturated (gray) result instead of the expected tint.
                var lumModEl = schemeClr.Element(A + "lumMod");
                var lumOffEl = schemeClr.Element(A + "lumOff");
                if (lumModEl != null || lumOffEl != null)
                {
                    var fc = fillColor.Value;
                    RgbToHsl(fc.R, fc.G, fc.B, out var h, out var s, out var l);
                    if (lumModEl != null && int.TryParse(lumModEl.Attribute("val")?.Value, out var lm))
                        l *= (lm / 100000f);
                    if (lumOffEl != null && int.TryParse(lumOffEl.Attribute("val")?.Value, out var lo))
                        l += (lo / 100000f);
                    if (l < 0f) l = 0f; else if (l > 1f) l = 1f;
                    HslToRgb(h, s, l, out var r2, out var g2, out var b2);
                    fillColor = new PdfColor(r2, g2, b2);
                }
            }
            var alphaEl = schemeClr.Element(A + "alpha");
            if (alphaEl != null && int.TryParse(alphaEl.Attribute("val")?.Value, out var a))
                alpha = a / 100000f;
        }

        return (fillColor, alpha);
    }

    private static void RgbToHsl(float r, float g, float b, out float h, out float s, out float l)
    {
        var max = Math.Max(r, Math.Max(g, b));
        var min = Math.Min(r, Math.Min(g, b));
        l = (max + min) / 2f;
        var d = max - min;
        if (d < 1e-6f)
        {
            h = 0f;
            s = 0f;
            return;
        }
        s = l > 0.5f ? d / (2f - max - min) : d / (max + min);
        if (max == r)
            h = ((g - b) / d + (g < b ? 6f : 0f)) / 6f;
        else if (max == g)
            h = ((b - r) / d + 2f) / 6f;
        else
            h = ((r - g) / d + 4f) / 6f;
    }

    private static void HslToRgb(float h, float s, float l, out float r, out float g, out float b)
    {
        if (s < 1e-6f)
        {
            r = g = b = l;
            return;
        }
        var q = l < 0.5f ? l * (1f + s) : l + s - l * s;
        var p = 2f * l - q;
        r = HueToRgb(p, q, h + 1f / 3f);
        g = HueToRgb(p, q, h);
        b = HueToRgb(p, q, h - 1f / 3f);
    }

    private static float HueToRgb(float p, float q, float t)
    {
        if (t < 0f) t += 1f;
        if (t > 1f) t -= 1f;
        if (t < 1f / 6f) return p + (q - p) * 6f * t;
        if (t < 1f / 2f) return q;
        if (t < 2f / 3f) return p + (q - p) * (2f / 3f - t) * 6f;
        return p;
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
        Dictionary<string, DocxNumberingDef> numbering, Dictionary<string, string> relationships, ZipArchive archive,
        string? defaultLatinFontName = null, string? defaultEastAsiaFontName = null,
        Dictionary<string, DocxTableStyleInfo>? tableStyles = null)
    {
        var rows = new List<DocxTableRow>();

        // Read table properties (borders, column widths)
        var tblPr = tblElement.Element(W + "tblPr");
        var tblGrid = tblElement.Element(W + "tblGrid");
        var columnWidths = new List<float>();

        // Read table-level cell margins
        float cellMarginLeft = 5.4f, cellMarginRight = 5.4f, cellMarginTop = 0f, cellMarginBottom = 0f;
        float cellSpacing = 0f;
        var tableAlignment = "left";
        var tblJc = tblPr?.Element(W + "jc")?.Attribute(W + "val")?.Value;
        if (!string.IsNullOrEmpty(tblJc))
            tableAlignment = tblJc;
        var tblCellSpacingEl = tblPr?.Element(W + "tblCellSpacing");
        if (tblCellSpacingEl != null
            && int.TryParse(tblCellSpacingEl.Attribute(W + "w")?.Value, out var tcs)
            && tcs > 0)
        {
            cellSpacing = tcs / 20f;
        }
        // Table indent (tblInd)
        float tableIndentLeft = 0;
        bool tableIndentExplicit = false;
        var tblIndEl = tblPr?.Element(W + "tblInd");
        if (tblIndEl != null && int.TryParse(tblIndEl.Attribute(W + "w")?.Value, out var tblIndW))
        {
            tableIndentLeft = tblIndW / 20f;
            tableIndentExplicit = true;
        }
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

        // Resolve table style
        var tblStyleVal = tblPr?.Element(W + "tblStyle")?.Attribute(W + "val")?.Value;
        DocxTableStyleInfo? tblStyleInfo = null;
        if (!string.IsNullOrEmpty(tblStyleVal) && tableStyles != null)
            tableStyles.TryGetValue(tblStyleVal, out tblStyleInfo);

        // Apply style-level cell margins (if not explicitly set on the table)
        if (tblCellMar == null && tblStyleInfo != null)
        {
            if (tblStyleInfo.CellMarginLeft >= 0) cellMarginLeft = tblStyleInfo.CellMarginLeft;
            if (tblStyleInfo.CellMarginRight >= 0) cellMarginRight = tblStyleInfo.CellMarginRight;
            if (tblStyleInfo.CellMarginTop >= 0) cellMarginTop = tblStyleInfo.CellMarginTop;
            if (tblStyleInfo.CellMarginBottom >= 0) cellMarginBottom = tblStyleInfo.CellMarginBottom;
        }

        // Detect whether the table has visible borders
        var hasBorders = tblStyleInfo?.HasBorders ?? false;
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

        // Read tblLook for banding
        var tblLook = tblPr?.Element(W + "tblLook");
        var noHBand = tblLook?.Attribute(W + "noHBand")?.Value == "1";
        var firstRowLook = tblLook?.Attribute(W + "firstRow")?.Value == "1";
        var firstColLook = tblLook?.Attribute(W + "firstColumn")?.Value == "1";
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

        var rowIndex = 0;
        foreach (var tr in tblElement.Elements(W + "tr"))
        {
            var cells = new List<DocxTableCell>();

            // Read row height from trPr
            float rowHeight = 0;
            bool rowHeightExact = false;
            int gridBefore = 0;
            var trPr = tr.Element(W + "trPr");
            var trHeightEl = trPr?.Element(W + "trHeight");
            if (trHeightEl != null && int.TryParse(trHeightEl.Attribute(W + "val")?.Value, out var rh))
            {
                rowHeight = rh / 20f; // twips to points
                rowHeightExact = trHeightEl.Attribute(W + "hRule")?.Value == "exact";
            }

            // Parse gridBefore: number of grid columns to skip before first cell
            var gridBeforeEl = trPr?.Element(W + "gridBefore");
            if (gridBeforeEl != null && int.TryParse(gridBeforeEl.Attribute(W + "val")?.Value, out var gbVal))
                gridBefore = gbVal;

            // Parse tblHeader: marks this row as a repeating header row
            var isHeaderRow = trPr?.Element(W + "tblHeader") != null;

            int cellIdx = 0;
            foreach (var tc in UnwrapSdt(tr.Elements()).Where(e => e.Name == W + "tc"))
            {
                var cellParagraphs = new List<DocxParagraph>();
                foreach (var child in UnwrapSdt(tc.Elements()))
                {
                    if (child.Name == W + "p")
                    {
                        var para = ReadParagraph(child, styles, numbering, relationships, archive, null, defaultLatinFontName, defaultEastAsiaFontName);
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
                                    var para = ReadParagraph(nestedP, styles, numbering, relationships, archive, null, defaultLatinFontName, defaultEastAsiaFontName);
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
                float tcMarTop = -1f;
                float tcMarBottom = -1f;
                float tcMarLeft = -1f;
                float tcMarRight = -1f;

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
                        // Use AllowNil here so an explicit w:val="nil"/"none" at the cell
                        // level produces the Width=0 sentinel that suppresses the inherited
                        // table-level border (instead of being indistinguishable from "edge
                        // not specified", which would let the table border win).
                        cellBorders = new DocxBorders(
                            Top: ReadBorderEdgeAllowNil(tcBorders.Element(W + "top")),
                            Bottom: ReadBorderEdgeAllowNil(tcBorders.Element(W + "bottom")),
                            Left: ReadBorderEdgeAllowNil(tcBorders.Element(W + "left")),
                            Right: ReadBorderEdgeAllowNil(tcBorders.Element(W + "right"))
                        );
                    }

                    var vAlignEl = tcPr.Element(W + "vAlign");
                    if (vAlignEl != null)
                    {
                        var va = vAlignEl.Attribute(W + "val")?.Value;
                        if (!string.IsNullOrEmpty(va))
                            verticalAlignment = va;
                    }

                    // Per-cell margins (tcMar)
                    var tcMarEl = tcPr.Element(W + "tcMar");
                    if (tcMarEl != null)
                    {
                        var topEl = tcMarEl.Element(W + "top");
                        if (topEl != null && int.TryParse(topEl.Attribute(W + "w")?.Value, out var mt))
                            tcMarTop = mt / 20f;
                        var bottomEl = tcMarEl.Element(W + "bottom");
                        if (bottomEl != null && int.TryParse(bottomEl.Attribute(W + "w")?.Value, out var mb))
                            tcMarBottom = mb / 20f;
                        var leftEl = tcMarEl.Element(W + "left") ?? tcMarEl.Element(W + "start");
                        if (leftEl != null && int.TryParse(leftEl.Attribute(W + "w")?.Value, out var ml))
                            tcMarLeft = ml / 20f;
                        var rightEl = tcMarEl.Element(W + "right") ?? tcMarEl.Element(W + "end");
                        if (rightEl != null && int.TryParse(rightEl.Attribute(W + "w")?.Value, out var mr))
                            tcMarRight = mr / 20f;
                    }
                }

                // Apply table style: band shading and conditional formatting borders
                if (tblStyleInfo != null)
                {
                    var isFirstRow = firstRowLook && rowIndex == 0;
                    var isFirstCol = firstColLook && cellIdx == 0;
                    // Determine band: rows after the (optional) header alternate band1 (odd) / band2 (even).
                    int bandIndex = -1;
                    if (!noHBand && !isFirstRow)
                        bandIndex = firstRowLook ? rowIndex - 1 : rowIndex;
                    var isBand1Horz = bandIndex >= 0 && bandIndex % 2 == 0;
                    var isBand2Horz = bandIndex >= 0 && bandIndex % 2 == 1;

                    if (shading == null && bandIndex >= 0)
                    {
                        if (isBand1Horz && tblStyleInfo.Band1HorzShading != null)
                            shading = tblStyleInfo.Band1HorzShading;
                        else if (isBand2Horz && tblStyleInfo.Band2HorzShading != null)
                            shading = tblStyleInfo.Band2HorzShading;
                    }
                    if (cellBorders == null)
                    {
                        // Per OOXML precedence (low->high): wholeTable, bands, firstCol/lastCol,
                        // firstRow/lastRow.  firstRow wins over firstCol on the top-left cell.
                        // We layer band borders first, then overlay firstCol/firstRow on top.
                        DocxBorders? layered = null;
                        if (isBand1Horz && tblStyleInfo.Band1HorzBorders != null)
                            layered = tblStyleInfo.Band1HorzBorders;
                        else if (isBand2Horz && tblStyleInfo.Band2HorzBorders != null)
                            layered = tblStyleInfo.Band2HorzBorders;

                        if (isFirstRow && tblStyleInfo.FirstRowBorders != null)
                            layered = MergeBorders(layered, tblStyleInfo.FirstRowBorders);
                        else if (isFirstCol && tblStyleInfo.FirstColBorders != null)
                            layered = MergeBorders(layered, tblStyleInfo.FirstColBorders);

                        cellBorders = layered;
                    }
                }

                cells.Add(new DocxTableCell(cellParagraphs, cellWidth, gridSpan, shading, cellBorders, isVMergeContinue, isVMergeRestart, verticalAlignment,
                    tcMarTop, tcMarBottom, tcMarLeft, tcMarRight));
                cellIdx++;
            }

            // Infer gridBefore when not explicitly set: if the row has fewer cells
            // than grid columns and the first cell's width matches a later grid column
            if (gridBefore == 0 && columnWidths.Count > 0 && cells.Count > 0)
            {
                var totalSpan = 0;
                foreach (var c in cells) totalSpan += c.GridSpan;
                if (totalSpan < columnWidths.Count && cells[0].Width > 0)
                {
                    // First cell's width doesn't match gridCol[0]: scan for matching column
                    if (Math.Abs(columnWidths[0] - cells[0].Width) > 2f)
                    {
                        for (int gi = 1; gi < columnWidths.Count; gi++)
                        {
                            if (Math.Abs(columnWidths[gi] - cells[0].Width) < 2f)
                            {
                                gridBefore = gi;
                                break;
                            }
                        }
                    }
                }
            }

            rows.Add(new DocxTableRow(cells, rowHeight, rowHeightExact, gridBefore, isHeaderRow));
            rowIndex++;
        }

        // Resolve table-level border edges (style then explicit override)
        DocxBorderEdge? finalBorderTop = tblStyleInfo?.BorderTop;
        DocxBorderEdge? finalBorderBottom = tblStyleInfo?.BorderBottom;
        DocxBorderEdge? finalBorderLeft = tblStyleInfo?.BorderLeft;
        DocxBorderEdge? finalBorderRight = tblStyleInfo?.BorderRight;
        DocxBorderEdge? finalInsideH = tblStyleInfo?.BorderInsideH;
        DocxBorderEdge? finalInsideV = tblStyleInfo?.BorderInsideV;
        if (tblBorders != null)
        {
            var ot = ReadBorderEdge(tblBorders.Element(W + "top"));
            var ob = ReadBorderEdge(tblBorders.Element(W + "bottom"));
            var ol = ReadBorderEdge(tblBorders.Element(W + "left"));
            var or2 = ReadBorderEdge(tblBorders.Element(W + "right"));
            var oih = ReadBorderEdge(tblBorders.Element(W + "insideH"));
            var oiv = ReadBorderEdge(tblBorders.Element(W + "insideV"));
            if (ot != null) finalBorderTop = ot;
            if (ob != null) finalBorderBottom = ob;
            if (ol != null) finalBorderLeft = ol;
            if (or2 != null) finalBorderRight = or2;
            if (oih != null) finalInsideH = oih;
            if (oiv != null) finalInsideV = oiv;
        }

        return new DocxTable(rows, columnWidths, hasBorders, cellMarginLeft, cellMarginRight, cellMarginTop, cellMarginBottom, tableAlignment,
            finalInsideH, finalInsideV, finalBorderTop, finalBorderBottom, finalBorderLeft, finalBorderRight,
            StyleLineSpacing: tblStyleInfo?.ParagraphLineSpacing ?? -1,
            StyleSpacingAfter: tblStyleInfo?.ParagraphSpacingAfter ?? -1,
            IndentLeft: tableIndentLeft,
            IndentLeftExplicit: tableIndentExplicit,
            CellSpacing: cellSpacing);
    }

    private static DocxPageLayout? ReadPageLayout(XElement body)
    {
        var sectPr = body.Element(W + "sectPr");
        if (sectPr == null) return null;
        return ParseSectionProperties(sectPr);
    }

    private static string? ReadHeaderFooter(XElement body, Dictionary<string, string> relationships,
        ZipArchive archive, Dictionary<string, DocxStyleInfo> styles, Dictionary<string, DocxNumberingDef> numbering,
        string refElementName, string? defaultLatinFontName = null, string? defaultEastAsiaFontName = null,
        Dictionary<string, DocxTableStyleInfo>? tableStyles = null)
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
            // Skip paragraphs inside mc:Fallback to avoid duplicate text box content
            if (p.Ancestors(MC + "Fallback").Any()) continue;

            var para = ReadParagraph(p, styles, numbering, relationships, archive, null, defaultLatinFontName, defaultEastAsiaFontName);
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
        string refElementName, string? defaultLatinFontName = null, string? defaultEastAsiaFontName = null)
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
            // Skip paragraphs inside mc:Fallback to avoid duplicate text box content
            if (p.Ancestors(MC + "Fallback").Any()) continue;

            var para = ReadParagraph(p, styles, numbering, relationships, archive, null, defaultLatinFontName, defaultEastAsiaFontName);
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

    /// <summary>
    /// Reads anchor images from header/footer XML (e.g. watermark globe images).
    /// </summary>
    private static List<DocxImage> ReadHeaderFooterImages(
        XElement body,
        Dictionary<string, string> relationships,
        ZipArchive archive,
        string refElementName)
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

        // Read header/footer-specific relationships
        var hfRelsPath = $"word/_rels/{target}.rels";
        var hfRels = ReadPartRelationships(archive, hfRelsPath);

        using var stream = entry.Open();
        var doc = XDocument.Load(stream);
        var images = new List<DocxImage>();

        foreach (var anchor in doc.Descendants(WP + "anchor"))
        {
            var blip = anchor.Descendants(A + "blip").FirstOrDefault();
            if (blip == null) continue;

            var embedId = blip.Attribute(R + "embed")?.Value;
            if (string.IsNullOrEmpty(embedId) || !hfRels.TryGetValue(embedId, out var imgTarget))
                continue;

            var imgPath = "word/" + imgTarget;
            var imgEntry = archive.GetEntry(imgPath);
            if (imgEntry == null) continue;

            using var imgStream = imgEntry.Open();
            using var ms = new MemoryStream();
            imgStream.CopyTo(ms);
            var data = ms.ToArray();

            var ext = Path.GetExtension(imgTarget).TrimStart('.').ToLowerInvariant();
            if (ext == "jpeg") ext = "jpg";
            if (ext is "emf" or "wmf") continue; // skip vector formats for now

            // Get extent
            var extent = anchor.Element(WP + "extent");
            long widthEmu = 0, heightEmu = 0;
            if (extent != null)
            {
                long.TryParse(extent.Attribute("cx")?.Value, out widthEmu);
                long.TryParse(extent.Attribute("cy")?.Value, out heightEmu);
            }

            // Get position
            long offsetXEmu = 0, offsetYEmu = 0;
            string? relFromH = null, relFromV = null;
            var posH = anchor.Element(WP + "positionH");
            var posV = anchor.Element(WP + "positionV");
            if (posH != null)
            {
                relFromH = posH.Attribute("relativeFrom")?.Value;
                var off = posH.Element(WP + "posOffset");
                if (off != null) long.TryParse(off.Value, out offsetXEmu);
            }
            if (posV != null)
            {
                relFromV = posV.Attribute("relativeFrom")?.Value;
                var off = posV.Element(WP + "posOffset");
                if (off != null) long.TryParse(off.Value, out offsetYEmu);
            }

            // Read alpha from alphaModFix
            float alpha = 1f;
            var alphaModFix = blip.Element(A + "alphaModFix");
            if (alphaModFix != null)
            {
                if (int.TryParse(alphaModFix.Attribute("amt")?.Value, out var amt))
                    alpha = amt / 100000f;
            }

            images.Add(new DocxImage(data, ext, widthEmu, heightEmu, true, offsetXEmu, offsetYEmu,
                false, relFromH, relFromV, false, alpha));
        }

        return images;
    }

    /// <summary>
    /// Reads header/footer content as full document elements (paragraphs + tables)
    /// using the header/footer-specific relationships for image resolution.
    /// </summary>
    private static List<DocxElement> ReadHeaderFooterElements(
        XElement body, Dictionary<string, string> relationships, ZipArchive archive,
        Dictionary<string, DocxStyleInfo> styles, Dictionary<string, DocxNumberingDef> numbering,
        string refElementName, string? defaultFontName, string? defaultEastAsiaFontName,
        Dictionary<string, DocxTableStyleInfo>? tableStyles = null)
    {
        var sectPr = body.Element(W + "sectPr");
        if (sectPr == null) return [];
        return ReadHeaderFooterElementsFromSectPr(sectPr, relationships, archive, styles, numbering, refElementName, defaultFontName, defaultEastAsiaFontName, tableStyles);
    }

    private static List<DocxElement> ReadHeaderFooterElementsFromSectPr(
        XElement sectPr, Dictionary<string, string> relationships, ZipArchive archive,
        Dictionary<string, DocxStyleInfo> styles, Dictionary<string, DocxNumberingDef> numbering,
        string refElementName, string? defaultFontName, string? defaultEastAsiaFontName,
        Dictionary<string, DocxTableStyleInfo>? tableStyles = null, string? typeFilter = null)
    {

        XElement? hfRef = null;
        foreach (var candidate in sectPr.Elements(W + refElementName))
        {
            var t = candidate.Attribute(W + "type")?.Value;
            // OOXML: type defaults to "default" when absent.
            var normalized = string.IsNullOrEmpty(t) ? "default" : t;
            if (typeFilter == null)
            {
                // Legacy behavior: prefer default type, fall back to the first reference seen.
                if (normalized == "default") { hfRef = candidate; break; }
                hfRef ??= candidate;
            }
            else if (string.Equals(normalized, typeFilter, StringComparison.OrdinalIgnoreCase))
            {
                hfRef = candidate;
                break;
            }
        }
        if (hfRef == null) return [];

        var rId = hfRef.Attribute(R + "id")?.Value;
        if (string.IsNullOrEmpty(rId) || !relationships.TryGetValue(rId, out var target))
            return [];

        var path = target.StartsWith("/") ? target.TrimStart('/') : "word/" + target;
        var entry = archive.GetEntry(path);
        if (entry == null) return [];

        // Read header/footer-specific relationships for image resolution
        var hfRelsPath = $"word/_rels/{target}.rels";
        var hfRels = ReadPartRelationships(archive, hfRelsPath);

        using var stream = entry.Open();
        var doc = XDocument.Load(stream);
        var root = doc.Root;
        if (root == null) return [];

        var elements = new List<DocxElement>();
        foreach (var child in UnwrapSdt(root.Elements()))
        {
            if (child.Name == W + "p")
            {
                var paragraph = ReadParagraph(child, styles, numbering, hfRels, archive, null, defaultFontName, defaultEastAsiaFontName);
                if (paragraph != null)
                    elements.Add(paragraph);
            }
            else if (child.Name == W + "tbl")
            {
                var table = ReadTable(child, styles, numbering, hfRels, archive, defaultFontName, defaultEastAsiaFontName, tableStyles);
                if (table != null)
                    elements.Add(table);
            }
        }

        // Extract text box content from anchor drawings (e.g., page numbers in positioned text boxes)
        foreach (var anchor in root.Descendants(WP + "anchor"))
        {
            // Skip anchors inside mc:Fallback to avoid duplicate content
            if (anchor.Ancestors(MC + "Fallback").Any()) continue;

            var txbx = anchor.Descendants(WPS + "txbx").FirstOrDefault();
            var txbxContent = txbx?.Element(W + "txbxContent");
            if (txbxContent == null) continue;

            // Determine alignment from anchor horizontal positioning
            var posH = anchor.Element(WP + "positionH");
            var hAlign = posH?.Element(WP + "align")?.Value;
            string? alignment = hAlign switch
            {
                "right" or "outside" => "right",
                "center" => "center",
                "left" or "inside" => "left",
                _ => null
            };
            // If no explicit align, infer from posOffset relative to page
            if (alignment == null && posH?.Attribute("relativeFrom")?.Value == "page")
            {
                var posOffsetStr = posH.Element(WP + "posOffset")?.Value;
                if (long.TryParse(posOffsetStr, out var posOffsetEmu))
                {
                    // A4 page width ≈ 7560000 EMU; centered if offset is 30-70% of page width
                    const long typicalPageWidthEmu = 7560000;
                    var fraction = (double)posOffsetEmu / typicalPageWidthEmu;
                    if (fraction >= 0.30 && fraction <= 0.70)
                        alignment = "center";
                    else if (fraction > 0.70)
                        alignment = "right";
                }
            }

            foreach (var txbxP in txbxContent.Elements(W + "p"))
            {
                var txbxPara = ReadParagraph(txbxP, styles, numbering, hfRels, archive, null, defaultFontName, defaultEastAsiaFontName);
                if (txbxPara == null) continue;
                if (alignment != null)
                    txbxPara = txbxPara with { Alignment = alignment };
                elements.Add(txbxPara);
            }
        }

        return elements;
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

        // Parse header/footer margins
        float headerMargin = marginTop / 2;
        float footerMargin = marginBottom / 2;
        if (pgMar != null)
        {
            if (float.TryParse(pgMar.Attribute(W + "header")?.Value, out var hm)) headerMargin = hm * twipsToPoints;
            if (float.TryParse(pgMar.Attribute(W + "footer")?.Value, out var fm)) footerMargin = fm * twipsToPoints;
        }

        // Parse section type (nextPage, continuous, evenPage, oddPage)
        var sectionType = "nextPage";
        var typeEl = sectPr.Element(W + "type");
        if (typeEl != null)
        {
            var typeVal = typeEl.Attribute(W + "val")?.Value;
            if (!string.IsNullOrEmpty(typeVal))
                sectionType = typeVal;
        }

        // Parse column layout
        int columnCount = 1;
        float columnSpacing = 36f;
        float[]? columnWidths = null;
        float[]? columnGaps = null;
        var colsEl = sectPr.Element(W + "cols");
        if (colsEl != null)
        {
            if (int.TryParse(colsEl.Attribute(W + "num")?.Value, out var cn) && cn > 1)
                columnCount = cn;
            if (float.TryParse(colsEl.Attribute(W + "space")?.Value, out var cs) && cs > 0)
                columnSpacing = cs * twipsToPoints;

            // Parse individual column widths for unequal-width layouts
            var equalWidth = colsEl.Attribute(W + "equalWidth")?.Value;
            if (equalWidth == "0")
            {
                var colElements = colsEl.Elements(W + "col").ToArray();
                if (colElements.Length > 0)
                {
                    columnWidths = new float[colElements.Length];
                    columnGaps = new float[colElements.Length];
                    for (int i = 0; i < colElements.Length; i++)
                    {
                        if (float.TryParse(colElements[i].Attribute(W + "w")?.Value, out var cw))
                            columnWidths[i] = cw * twipsToPoints;
                        if (float.TryParse(colElements[i].Attribute(W + "space")?.Value, out var cgs))
                            columnGaps[i] = cgs * twipsToPoints;
                    }
                    // Infer column count from col elements if not explicitly set
                    if (columnCount <= 1)
                        columnCount = colElements.Length;
                }
            }
        }

        // Parse page number start
        int pageNumStart = -1;
        var pgNumType = sectPr.Element(W + "pgNumType");
        if (pgNumType != null)
        {
            if (int.TryParse(pgNumType.Attribute(W + "start")?.Value, out var pns))
                pageNumStart = pns;
        }

        // Parse titlePg flag — when set, the first page of the section uses a
        // distinct first-page header/footer (or none when no first reference is given).
        bool titlePg = sectPr.Element(W + "titlePg") != null;

        return new DocxPageLayout(pageWidth, pageHeight, marginTop, marginBottom, marginLeft, marginRight, gridLinePitch, headerMargin, footerMargin, sectionType, columnCount, columnSpacing, pageNumStart, columnWidths, columnGaps, titlePg);
    }


    /// <summary>
    /// Parses word/footnotes.xml and returns a dictionary of footnote definitions (normal footnotes only).
    /// </summary>
    private static Dictionary<string, DocxFootnote>? ReadFootnotes(ZipArchive archive, Dictionary<string, DocxStyleInfo> styles,
        string? defaultFontName, string? defaultEastAsiaFontName)
    {
        var entry = archive.GetEntry("word/footnotes.xml");
        if (entry == null) return null;

        XDocument doc;
        using (var stream = entry.Open())
            doc = XDocument.Load(stream);

        var result = new Dictionary<string, DocxFootnote>();
        int seqNum = 0; // sequential number for normal footnotes

        foreach (var fn in doc.Root!.Elements(W + "footnote"))
        {
            var fnId = fn.Attribute(W + "id")?.Value;
            var fnType = fn.Attribute(W + "type")?.Value;
            if (string.IsNullOrEmpty(fnId)) continue;
            // Skip separator and continuationSeparator (type != null means special)
            if (fnType != null) continue;

            seqNum++;
            var runs = new List<DocxRun>();
            float fontSize = 10f; // default footnote text size

            foreach (var p in fn.Elements(W + "p"))
            {
                // Check paragraph style for footnote text size
                var pStyle = p.Element(W + "pPr")?.Element(W + "pStyle")?.Attribute(W + "val")?.Value;
                if (!string.IsNullOrEmpty(pStyle) && styles.TryGetValue(pStyle, out var fnStyle))
                {
                    if (fnStyle.FontSize > 0) fontSize = fnStyle.FontSize;
                }

                foreach (var r in p.Elements(W + "r"))
                {
                    var text = "";
                    foreach (var child in r.Elements())
                    {
                        if (child.Name == W + "t")
                            text += child.Value;
                        else if (child.Name == W + "footnoteRef")
                            text += seqNum.ToString(); // Self-reference: display the footnote number
                    }
                    if (!string.IsNullOrEmpty(text))
                        runs.Add(new DocxRun(text, FontSize: fontSize));
                }
            }

            result[fnId] = new DocxFootnote(fnId, fontSize, runs);
        }

        return result.Count > 0 ? result : null;
    }

    private static Dictionary<string, string> ReadRelationships(ZipArchive archive)
    {
        return ReadPartRelationships(archive, "word/_rels/document.xml.rels");
    }

    private static Dictionary<string, string> ReadPartRelationships(ZipArchive archive, string relsPath)
    {
        var rels = new Dictionary<string, string>();
        var entry = archive.GetEntry(relsPath);
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

    private static (Dictionary<string, DocxStyleInfo> Styles, float DefaultLineSpacing, bool DefaultLineSpacingAbsolute, string? DefaultFontName, string? DefaultEastAsiaFontName, Dictionary<string, DocxTableStyleInfo> TableStyles) ReadStyles(ZipArchive archive)
    {
        var styles = new Dictionary<string, DocxStyleInfo>();
        var (majorThemeLatinFont, minorThemeLatinFont, majorThemeEastAsiaFont, minorThemeEastAsiaFont) = ReadThemeFonts(archive);
        var entry = archive.GetEntry("word/styles.xml");
        if (entry == null) return (styles, 0, false, null, null, new Dictionary<string, DocxTableStyleInfo>());

        using var stream = entry.Open();
        var doc = XDocument.Load(stream);

        // Read docDefaults for baseline paragraph/run properties
        float defaultFontSize = 11f;
        float defaultSpacingAfter = -1;
        float defaultSpacingBefore = 0;
        float defaultLineSpacing = 0;
        bool defaultLineSpacingAbsolute = false;
        string? defaultEastAsiaLang = null;

        var docDefaults = doc.Descendants(W + "docDefaults").FirstOrDefault();
        if (docDefaults != null)
        {
            var rPrDefault = docDefaults.Element(W + "rPrDefault")?.Element(W + "rPr");
            if (rPrDefault != null)
            {
                var sz = rPrDefault.Element(W + "sz")?.Attribute(W + "val")?.Value;
                if (float.TryParse(sz, out var s) && s > 0)
                    defaultFontSize = s / 2f;

                defaultEastAsiaLang = rPrDefault.Element(W + "lang")?.Attribute(W + "eastAsia")?.Value;
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

        var effectiveThemeEastAsiaFont = ResolveThemeEastAsiaFont(defaultEastAsiaLang, majorThemeEastAsiaFont, minorThemeEastAsiaFont);

        // Two-pass style reading: first pass populates all styles, second pass resolves basedOn inheritance
        var styleElements = doc.Descendants(W + "style").ToList();
        var basedOnMap = new Dictionary<string, string>(); // styleId -> basedOn styleId
        string? defaultParagraphStyleId = null;

        // First pass: read all styles without inheritance
        foreach (var style in styleElements)
        {
            var styleId = style.Attribute(W + "styleId")?.Value;
            if (string.IsNullOrEmpty(styleId)) continue;
            var styleType = style.Attribute(W + "type")?.Value;
            var styleName = style.Element(W + "name")?.Attribute(W + "val")?.Value;
            if (styleType == "paragraph"
                && (style.Attribute(W + "default")?.Value == "1"
                    || string.Equals(styleName, "Normal", StringComparison.OrdinalIgnoreCase)))
            {
                defaultParagraphStyleId ??= styleId;
            }

            var basedOn = style.Element(W + "basedOn")?.Attribute(W + "val")?.Value;
            if (!string.IsNullOrEmpty(basedOn))
                basedOnMap[styleId] = basedOn;

            var rPr = style.Element(W + "rPr");
            var pPr = style.Element(W + "pPr");

            // Character styles should not inherit default font size;
            // they should only carry an explicit size if defined.
            float fontSize = styleType == "character" ? 0 : defaultFontSize;
            bool bold = false;
            bool italic = false;
            PdfColor? color = null;
            string alignment = "";
            float spacingBefore = defaultSpacingBefore;
            float spacingAfter = defaultSpacingAfter;
            bool caps = false;
            float styleLineSpacing = 0;
            bool styleLineSpacingAbsolute = false;
            bool styleLineSpacingExact = false;
            bool contextualSpacing = false;
            string? styleFontName = null;
            float styleCharSpacing = 0;
            string? styleVertAlign = null;
            float styleIndentLeft = 0, styleIndentRight = 0, styleIndentFirstLine = 0;
            bool styleHasIndentLeft = false, styleHasIndentRight = false, styleHasIndentFirstLine = false;

            if (rPr != null)
            {
                if (rPr.Element(W + "b") != null) bold = true;
                if (rPr.Element(W + "i") != null) italic = true;
                if (rPr.Element(W + "caps") != null) caps = true;
                var sz = rPr.Element(W + "sz")?.Attribute(W + "val")?.Value;
                if (float.TryParse(sz, out var s) && s > 0)
                    fontSize = s / 2f;
                color = ReadRunColor(rPr);
                var rFonts = rPr.Element(W + "rFonts");
                if (rFonts != null)
                    styleFontName = ResolveFontNameFromRFonts(rFonts, majorThemeLatinFont, minorThemeLatinFont,
                        effectiveThemeEastAsiaFont, effectiveThemeEastAsiaFont);
                var spacingEl = rPr.Element(W + "spacing");
                if (spacingEl != null && int.TryParse(spacingEl.Attribute(W + "val")?.Value, out var scs))
                    styleCharSpacing = scs / 20f;
                var vaEl = rPr.Element(W + "vertAlign");
                if (vaEl != null)
                    styleVertAlign = vaEl.Attribute(W + "val")?.Value;
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
                        styleLineSpacingExact = lineRule == "exact";
                        styleLineSpacing = styleLineSpacingAbsolute ? sl / 20f : sl / 240f;
                    }
                }
                if (pPr.Element(W + "contextualSpacing") != null)
                    contextualSpacing = true;

                // Indentation from style
                var styleInd = pPr.Element(W + "ind");
                if (styleInd != null)
                {
                    var leftAttr = styleInd.Attribute(W + "left") ?? styleInd.Attribute(W + "start");
                    if (leftAttr != null && int.TryParse(leftAttr.Value, out var sli))
                    {
                        styleIndentLeft = sli / 20f;
                        styleHasIndentLeft = true;
                    }
                    var rightAttr = styleInd.Attribute(W + "right") ?? styleInd.Attribute(W + "end");
                    if (rightAttr != null && int.TryParse(rightAttr.Value, out var sri))
                    {
                        styleIndentRight = sri / 20f;
                        styleHasIndentRight = true;
                    }
                    var flAttr = styleInd.Attribute(W + "firstLine");
                    if (flAttr != null && int.TryParse(flAttr.Value, out var sfl))
                    {
                        styleIndentFirstLine = sfl / 20f;
                        styleHasIndentFirstLine = true;
                    }
                    var hgAttr = styleInd.Attribute(W + "hanging");
                    if (hgAttr != null && int.TryParse(hgAttr.Value, out var shg))
                    {
                        styleIndentFirstLine = -shg / 20f;
                        styleHasIndentFirstLine = true;
                    }
                }
            }

            // keepNext: determined from pPr element
            var keepNextStyle = pPr?.Element(W + "keepNext") is { } kn &&
                kn.Attribute(W + "val")?.Value is not ("0" or "false");

            // Heading styles get bold by default
            if (styleId.StartsWith("Heading", StringComparison.OrdinalIgnoreCase) ||
                styleId.StartsWith("heading", StringComparison.Ordinal))
            {
                bold = true;
            }

            styles[styleId] = new DocxStyleInfo(fontSize, bold, italic, color, alignment, spacingBefore, spacingAfter, caps, styleLineSpacing, styleLineSpacingAbsolute, styleLineSpacingExact, contextualSpacing, styleFontName, styleCharSpacing, KeepNext: keepNextStyle, VerticalAlign: styleVertAlign,
                IndentLeft: styleIndentLeft, IndentRight: styleIndentRight, IndentFirstLine: styleIndentFirstLine,
                HasIndentLeft: styleHasIndentLeft, HasIndentRight: styleHasIndentRight, HasIndentFirstLine: styleHasIndentFirstLine);
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
            var styleFontName = !string.IsNullOrWhiteSpace(current.FontName) ? current.FontName : baseStyle.FontName;
            var spacingEl = pPr?.Element(W + "spacing");
            var spacingBefore = spacingEl?.Attribute(W + "before") != null ? current.SpacingBefore : baseStyle.SpacingBefore;
            var spacingAfter = spacingEl?.Attribute(W + "after") != null ? current.SpacingAfter : baseStyle.SpacingAfter;
            var lineSpacing3 = spacingEl?.Attribute(W + "line") != null ? current.LineSpacing : baseStyle.LineSpacing;
            var lineSpacingAbsolute3 = spacingEl?.Attribute(W + "line") != null ? current.LineSpacingAbsolute : baseStyle.LineSpacingAbsolute;
            var lineSpacingExact3 = spacingEl?.Attribute(W + "line") != null ? current.LineSpacingExact : baseStyle.LineSpacingExact;
            var contextualSpacing2 = current.ContextualSpacing || baseStyle.ContextualSpacing;
            var charSpacing2 = rPr?.Element(W + "spacing") != null ? current.CharSpacing : baseStyle.CharSpacing;
            // keepNext is inherited from base style if not explicitly set by the derived style
            var keepNext2 = current.KeepNext ||
                (pPr?.Element(W + "keepNext") == null && baseStyle.KeepNext);
            var vertAlign2 = !string.IsNullOrEmpty(current.VerticalAlign) ? current.VerticalAlign : baseStyle.VerticalAlign;

            // Indent: per-attribute fallback to base style
            bool curHasLeft = current.HasIndentLeft;
            bool curHasRight = current.HasIndentRight;
            bool curHasFirstLine = current.HasIndentFirstLine;
            float indLeft2 = curHasLeft ? current.IndentLeft : baseStyle.IndentLeft;
            float indRight2 = curHasRight ? current.IndentRight : baseStyle.IndentRight;
            float indFirst2 = curHasFirstLine ? current.IndentFirstLine : baseStyle.IndentFirstLine;
            bool hasLeft2 = curHasLeft || baseStyle.HasIndentLeft;
            bool hasRight2 = curHasRight || baseStyle.HasIndentRight;
            bool hasFirst2 = curHasFirstLine || baseStyle.HasIndentFirstLine;

            styles[styleId] = new DocxStyleInfo(fontSize, bold, italic, color2, alignment, spacingBefore, spacingAfter, caps2, lineSpacing3, lineSpacingAbsolute3, lineSpacingExact3, contextualSpacing2, styleFontName, charSpacing2, KeepNext: keepNext2, VerticalAlign: vertAlign2,
                IndentLeft: indLeft2, IndentRight: indRight2, IndentFirstLine: indFirst2,
                HasIndentLeft: hasLeft2, HasIndentRight: hasRight2, HasIndentFirstLine: hasFirst2);
        }

        if (!styles.ContainsKey("Normal")
            && defaultParagraphStyleId != null
            && styles.TryGetValue(defaultParagraphStyleId, out var defaultParagraphStyle))
        {
            styles["Normal"] = defaultParagraphStyle;
        }

        // Extract default font name from Normal style or docDefaults
        string? defaultFontName = null;
        string? defaultEastAsiaFontName = null;
        XElement? normalStyleRFonts = null;
        foreach (var style in styleElements)
        {
            var sid = style.Attribute(W + "styleId")?.Value;
            if (sid == "Normal" || style.Attribute(W + "default")?.Value == "1")
            {
                normalStyleRFonts = style.Element(W + "rPr")?.Element(W + "rFonts");
                if (normalStyleRFonts != null && RFontsHasLatinAttr(normalStyleRFonts))
                {
                    defaultFontName = ResolveFontNameFromRFonts(normalStyleRFonts, majorThemeLatinFont, minorThemeLatinFont,
                        effectiveThemeEastAsiaFont, effectiveThemeEastAsiaFont);
                    defaultEastAsiaFontName = ResolveFontNameFromRFonts(normalStyleRFonts, majorThemeLatinFont, minorThemeLatinFont,
                        effectiveThemeEastAsiaFont, effectiveThemeEastAsiaFont, preferEastAsiaTheme: true);
                }
                break;
            }
        }
        if (defaultFontName == null && docDefaults != null)
        {
            var rFonts = docDefaults.Element(W + "rPrDefault")?.Element(W + "rPr")?.Element(W + "rFonts");
            if (rFonts != null)
            {
                defaultFontName = ResolveFontNameFromRFonts(rFonts, majorThemeLatinFont, minorThemeLatinFont,
                    effectiveThemeEastAsiaFont, effectiveThemeEastAsiaFont);
                defaultEastAsiaFontName = ResolveFontNameFromRFonts(rFonts, majorThemeLatinFont, minorThemeLatinFont,
                    effectiveThemeEastAsiaFont, effectiveThemeEastAsiaFont, preferEastAsiaTheme: true);
            }
        }
        // Last resort: accept the Normal-style cs/eastAsia fallback (e.g. legacy docs without docDefaults)
        if (defaultFontName == null && normalStyleRFonts != null)
        {
            defaultFontName = ResolveFontNameFromRFonts(normalStyleRFonts, majorThemeLatinFont, minorThemeLatinFont,
                effectiveThemeEastAsiaFont, effectiveThemeEastAsiaFont);
            defaultEastAsiaFontName ??= ResolveFontNameFromRFonts(normalStyleRFonts, majorThemeLatinFont, minorThemeLatinFont,
                effectiveThemeEastAsiaFont, effectiveThemeEastAsiaFont, preferEastAsiaTheme: true);
        }
        if (defaultEastAsiaFontName == null)
            defaultEastAsiaFontName = effectiveThemeEastAsiaFont;

        // OOXML default line spacing is single (1.0) when not specified
        if (defaultLineSpacing == 0)
            defaultLineSpacing = 1.0f;

        // Parse table styles
        var tableStyles = new Dictionary<string, DocxTableStyleInfo>();
        foreach (var style in styleElements)
        {
            var styleType = style.Attribute(W + "type")?.Value;
            if (styleType != "table") continue;
            var styleId = style.Attribute(W + "styleId")?.Value;
            if (string.IsNullOrEmpty(styleId)) continue;

            var tblPrStyle = style.Element(W + "tblPr");
            var tblBordersStyle = tblPrStyle?.Element(W + "tblBorders");
            bool hasBorders = false;
            DocxBorderEdge? bTop = null, bBottom = null, bLeft = null, bRight = null, bInsideH = null, bInsideV = null;
            if (tblBordersStyle != null)
            {
                bTop = ReadBorderEdge(tblBordersStyle.Element(W + "top"));
                bBottom = ReadBorderEdge(tblBordersStyle.Element(W + "bottom"));
                bLeft = ReadBorderEdge(tblBordersStyle.Element(W + "left"));
                bRight = ReadBorderEdge(tblBordersStyle.Element(W + "right"));
                bInsideH = ReadBorderEdge(tblBordersStyle.Element(W + "insideH"));
                bInsideV = ReadBorderEdge(tblBordersStyle.Element(W + "insideV"));
                if (bTop != null || bBottom != null || bLeft != null || bRight != null || bInsideH != null || bInsideV != null)
                    hasBorders = true;
            }

            // Paragraph-level spacing from table style pPr
            float paraLineSpacing = -1;
            float paraSpacingAfter = -1;
            var stylePPr = style.Element(W + "pPr");
            if (stylePPr != null)
            {
                var spacingEl = stylePPr.Element(W + "spacing");
                if (spacingEl != null)
                {
                    var lineRule = spacingEl.Attribute(W + "lineRule")?.Value;
                    if (int.TryParse(spacingEl.Attribute(W + "line")?.Value, out var lineVal))
                    {
                        if (lineRule == "exact" || lineRule == "atLeast")
                            paraLineSpacing = lineVal / 20f; // twips to pt
                        else
                            paraLineSpacing = lineVal / 240f; // auto multiplier
                    }
                    if (int.TryParse(spacingEl.Attribute(W + "after")?.Value, out var afterVal))
                        paraSpacingAfter = afterVal / 20f; // twips to pt
                }
            }

            // Cell margins from table style
            float cmTop = -1, cmBottom = -1, cmLeft = -1, cmRight = -1;
            var styleCellMar = tblPrStyle?.Element(W + "tblCellMar");
            if (styleCellMar != null)
            {
                if (int.TryParse(styleCellMar.Element(W + "top")?.Attribute(W + "w")?.Value, out var smt))
                    cmTop = smt / 20f;
                if (int.TryParse(styleCellMar.Element(W + "bottom")?.Attribute(W + "w")?.Value, out var smb))
                    cmBottom = smb / 20f;
                if (int.TryParse(styleCellMar.Element(W + "left")?.Attribute(W + "w")?.Value, out var sml))
                    cmLeft = sml / 20f;
                if (int.TryParse(styleCellMar.Element(W + "right")?.Attribute(W + "w")?.Value, out var smr))
                    cmRight = smr / 20f;
            }

            // Band shading
            PdfColor? band1Horz = null, band2Horz = null;
            DocxBorders? firstRowBorders = null;
            DocxBorders? firstColBorders = null;
            DocxBorders? band1HorzBorders = null;
            DocxBorders? band2HorzBorders = null;
            foreach (var tsp in style.Elements(W + "tblStylePr"))
            {
                var tspType = tsp.Attribute(W + "type")?.Value;
                var tcPr = tsp.Element(W + "tcPr");
                if (tcPr == null) continue;
                var shd = tcPr.Element(W + "shd");
                var fill = shd?.Attribute(W + "fill")?.Value;
                PdfColor? shadingColor = !string.IsNullOrEmpty(fill) && fill != "auto" ? PdfColor.FromHex(fill) : null;

                if (tspType == "band1Horz")
                {
                    if (shadingColor != null) band1Horz = shadingColor;
                    var bBorders = tcPr.Element(W + "tcBorders");
                    if (bBorders != null)
                        band1HorzBorders = new DocxBorders(
                            Top: ReadBorderEdgeAllowNil(bBorders.Element(W + "top")),
                            Bottom: ReadBorderEdgeAllowNil(bBorders.Element(W + "bottom")),
                            Left: ReadBorderEdgeAllowNil(bBorders.Element(W + "left")),
                            Right: ReadBorderEdgeAllowNil(bBorders.Element(W + "right"))
                        );
                }
                else if (tspType == "band2Horz")
                {
                    if (shadingColor != null) band2Horz = shadingColor;
                    var bBorders = tcPr.Element(W + "tcBorders");
                    if (bBorders != null)
                        band2HorzBorders = new DocxBorders(
                            Top: ReadBorderEdgeAllowNil(bBorders.Element(W + "top")),
                            Bottom: ReadBorderEdgeAllowNil(bBorders.Element(W + "bottom")),
                            Left: ReadBorderEdgeAllowNil(bBorders.Element(W + "left")),
                            Right: ReadBorderEdgeAllowNil(bBorders.Element(W + "right"))
                        );
                }
                else if (tspType == "firstRow")
                {
                    var frBorders = tcPr.Element(W + "tcBorders");
                    if (frBorders != null)
                        firstRowBorders = new DocxBorders(
                            Top: ReadBorderEdgeAllowNil(frBorders.Element(W + "top")),
                            Bottom: ReadBorderEdgeAllowNil(frBorders.Element(W + "bottom")),
                            Left: ReadBorderEdgeAllowNil(frBorders.Element(W + "left")),
                            Right: ReadBorderEdgeAllowNil(frBorders.Element(W + "right"))
                        );
                }
                else if (tspType == "firstCol")
                {
                    var fcBorders = tcPr.Element(W + "tcBorders");
                    if (fcBorders != null)
                        firstColBorders = new DocxBorders(
                            Top: ReadBorderEdgeAllowNil(fcBorders.Element(W + "top")),
                            Bottom: ReadBorderEdgeAllowNil(fcBorders.Element(W + "bottom")),
                            Left: ReadBorderEdgeAllowNil(fcBorders.Element(W + "left")),
                            Right: ReadBorderEdgeAllowNil(fcBorders.Element(W + "right"))
                        );
                }
            }

            tableStyles[styleId] = new DocxTableStyleInfo(hasBorders, bTop, bBottom, bLeft, bRight, bInsideH, bInsideV,
                band1Horz, band2Horz, firstRowBorders, firstColBorders, cmTop, cmBottom, cmLeft, cmRight,
                ParagraphSpacingAfter: paraSpacingAfter, ParagraphLineSpacing: paraLineSpacing,
                Band1HorzBorders: band1HorzBorders, Band2HorzBorders: band2HorzBorders);
        }

        return (styles, defaultLineSpacing, defaultLineSpacingAbsolute, defaultFontName, defaultEastAsiaFontName, tableStyles);
    }

    /// <summary>
    /// Reads major/minor Latin and East-Asia script theme fonts from theme1.xml.
    /// </summary>
    private static (string? MajorLatinFont, string? MinorLatinFont, Dictionary<string, string> MajorEastAsiaFonts, Dictionary<string, string> MinorEastAsiaFonts) ReadThemeFonts(ZipArchive archive)
    {
        var entry = archive.GetEntry("word/theme/theme1.xml");
        if (entry == null) return (null, null, new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase), new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase));

        using var stream = entry.Open();
        var doc = XDocument.Load(stream);

        var fontScheme = doc.Descendants(A + "fontScheme").FirstOrDefault();
        if (fontScheme == null) return (null, null, new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase), new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase));

        var majorLatin = fontScheme.Element(A + "majorFont")?.Element(A + "latin")?.Attribute("typeface")?.Value;
        var minorLatin = fontScheme.Element(A + "minorFont")?.Element(A + "latin")?.Attribute("typeface")?.Value;

        var majorEastAsia = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var minorEastAsia = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        var majorFont = fontScheme.Element(A + "majorFont");
        var minorFont = fontScheme.Element(A + "minorFont");
        if (majorFont != null)
        {
            foreach (var f in majorFont.Elements(A + "font"))
            {
                var script = f.Attribute("script")?.Value;
                var typeface = f.Attribute("typeface")?.Value;
                if (!string.IsNullOrWhiteSpace(script) && !string.IsNullOrWhiteSpace(typeface))
                    majorEastAsia[script] = typeface;
            }
        }
        if (minorFont != null)
        {
            foreach (var f in minorFont.Elements(A + "font"))
            {
                var script = f.Attribute("script")?.Value;
                var typeface = f.Attribute("typeface")?.Value;
                if (!string.IsNullOrWhiteSpace(script) && !string.IsNullOrWhiteSpace(typeface))
                    minorEastAsia[script] = typeface;
            }
        }

        return (string.IsNullOrWhiteSpace(majorLatin) ? null : PdfWriter.MaybeFallbackForMissingFont(majorLatin),
            string.IsNullOrWhiteSpace(minorLatin) ? null : PdfWriter.MaybeFallbackForMissingFont(minorLatin),
            majorEastAsia, minorEastAsia);
    }

    private static bool RFontsHasLatinAttr(XElement rFonts) =>
        rFonts.Attribute(W + "ascii") != null
        || rFonts.Attribute(W + "hAnsi") != null
        || rFonts.Attribute(W + "asciiTheme") != null
        || rFonts.Attribute(W + "hAnsiTheme") != null;

    /// <summary>
    /// Resolves effective font name from w:rFonts, including Latin and EastAsia theme references.
    /// </summary>
    private static string? ResolveFontNameFromRFonts(XElement rFonts, string? majorThemeLatinFont, string? minorThemeLatinFont,
        string? majorThemeEastAsiaFont, string? minorThemeEastAsiaFont, bool preferEastAsiaTheme = false)
    {
        // OOXML rFonts attributes are scoped by script:
        //   ascii / asciiTheme / hAnsi / hAnsiTheme  → Latin / Western text
        //   eastAsia / eastAsiaTheme                 → CJK text
        //   cs / cstheme                             → complex scripts (Arabic, Hebrew, Thai)
        // For Latin runs, w:cs and w:eastAsia must NOT take priority over the Latin theme;
        // doing so causes themed Latin docs (where docDefaults sets cs="Times New Roman" for
        // complex-script fallback while ascii/hAnsiTheme point to Century Gothic) to render
        // body text in Times New Roman instead of the theme's Latin face.

        static string? ResolveThemeToken(string? themeFont,
            string? majorLatin, string? minorLatin, string? majorEa, string? minorEa)
        {
            if (string.IsNullOrWhiteSpace(themeFont)) return null;
            if (themeFont!.EndsWith("EastAsia", StringComparison.OrdinalIgnoreCase))
            {
                if (themeFont.StartsWith("major", StringComparison.OrdinalIgnoreCase))
                    return majorEa ?? majorLatin;
                if (themeFont.StartsWith("minor", StringComparison.OrdinalIgnoreCase))
                    return minorEa ?? minorLatin;
            }
            if (themeFont.StartsWith("major", StringComparison.OrdinalIgnoreCase))
                return majorLatin;
            if (themeFont.StartsWith("minor", StringComparison.OrdinalIgnoreCase))
                return minorLatin;
            return null;
        }

        if (preferEastAsiaTheme)
        {
            var eaExplicit = rFonts.Attribute(W + "eastAsia")?.Value;
            if (!string.IsNullOrWhiteSpace(eaExplicit))
                return eaExplicit;
            var eaThemeResolved = ResolveThemeToken(rFonts.Attribute(W + "eastAsiaTheme")?.Value,
                majorThemeLatinFont, minorThemeLatinFont, majorThemeEastAsiaFont, minorThemeEastAsiaFont);
            if (!string.IsNullOrWhiteSpace(eaThemeResolved))
                return eaThemeResolved;
        }

        // Latin (preferred): explicit ascii/hAnsi → asciiTheme/hAnsiTheme.
        var latinExplicit = rFonts.Attribute(W + "ascii")?.Value
            ?? rFonts.Attribute(W + "hAnsi")?.Value;
        if (!string.IsNullOrWhiteSpace(latinExplicit))
            return latinExplicit;

        var latinThemeResolved = ResolveThemeToken(
            rFonts.Attribute(W + "asciiTheme")?.Value ?? rFonts.Attribute(W + "hAnsiTheme")?.Value,
            majorThemeLatinFont, minorThemeLatinFont, majorThemeEastAsiaFont, minorThemeEastAsiaFont);
        if (!string.IsNullOrWhiteSpace(latinThemeResolved))
            return latinThemeResolved;

        // Final fallbacks: complex-script and East Asian explicit/theme entries.
        // Skip cs/cstheme for Latin runs when the value is a Word "(Body CS)"/"(Heading CS)" placeholder
        // — those are not real installed font names, and using them prevents inheritance from finding the
        // theme's Latin face (e.g. Century Gothic via docDefaults asciiTheme=minorHAnsi).
        var csValue = rFonts.Attribute(W + "cs")?.Value;
        var eaValue = rFonts.Attribute(W + "eastAsia")?.Value;
        var fallbackExplicit = csValue ?? eaValue;
        if (!string.IsNullOrWhiteSpace(fallbackExplicit) && !LooksLikeComplexScriptPlaceholder(fallbackExplicit))
            return fallbackExplicit;

        var fallbackTheme = ResolveThemeToken(
            rFonts.Attribute(W + "cstheme")?.Value ?? rFonts.Attribute(W + "eastAsiaTheme")?.Value,
            majorThemeLatinFont, minorThemeLatinFont, majorThemeEastAsiaFont, minorThemeEastAsiaFont);
        return fallbackTheme;
    }

    private static bool LooksLikeComplexScriptPlaceholder(string? font)
    {
        if (string.IsNullOrWhiteSpace(font)) return false;
        // Word stamps synthetic names like "Times New Roman (Body CS)" / "Calibri (Headings CS)"
        // when a theme's complex-script font is referenced but not yet resolved to an installed face.
        return font!.IndexOf("(Body CS)", StringComparison.OrdinalIgnoreCase) >= 0
            || font.IndexOf("(Headings CS)", StringComparison.OrdinalIgnoreCase) >= 0
            || font.IndexOf("(Heading CS)", StringComparison.OrdinalIgnoreCase) >= 0;
    }

    private static string? ResolveThemeEastAsiaFont(string? eastAsiaLang,
        Dictionary<string, string> majorThemeEastAsiaFonts,
        Dictionary<string, string> minorThemeEastAsiaFonts)
    {
        var script = GetThemeScriptFromEastAsiaLang(eastAsiaLang);
        if (script != null)
        {
            if (minorThemeEastAsiaFonts.TryGetValue(script, out var minor) && !string.IsNullOrWhiteSpace(minor))
                return minor;
            if (majorThemeEastAsiaFonts.TryGetValue(script, out var major) && !string.IsNullOrWhiteSpace(major))
                return major;
        }

        // Fallback to Traditional Chinese first because many zh-TW docs use it.
        if (minorThemeEastAsiaFonts.TryGetValue("Hant", out var hantMinor) && !string.IsNullOrWhiteSpace(hantMinor))
            return hantMinor;
        if (majorThemeEastAsiaFonts.TryGetValue("Hant", out var hantMajor) && !string.IsNullOrWhiteSpace(hantMajor))
            return hantMajor;

        return minorThemeEastAsiaFonts.Values.FirstOrDefault(v => !string.IsNullOrWhiteSpace(v))
            ?? majorThemeEastAsiaFonts.Values.FirstOrDefault(v => !string.IsNullOrWhiteSpace(v));
    }

    private static string? GetThemeScriptFromEastAsiaLang(string? eastAsiaLang)
    {
        if (string.IsNullOrWhiteSpace(eastAsiaLang)) return null;
        var lang = eastAsiaLang.Trim().ToLowerInvariant();
        if (lang.StartsWith("zh-tw", StringComparison.Ordinal)
            || lang.StartsWith("zh-hk", StringComparison.Ordinal)
            || lang.StartsWith("zh-mo", StringComparison.Ordinal)
            || lang.Contains("hant", StringComparison.Ordinal))
            return "Hant";
        if (lang.StartsWith("zh", StringComparison.Ordinal)
            || lang.Contains("hans", StringComparison.Ordinal))
            return "Hans";
        if (lang.StartsWith("ja", StringComparison.Ordinal))
            return "Jpan";
        if (lang.StartsWith("ko", StringComparison.Ordinal))
            return "Hang";
        return null;
    }

    /// <summary>
    /// Maps a bullet character from Wingdings/Symbol font encoding to a Unicode equivalent.
    /// </summary>
    private static string MapBulletChar(string? lvlText, string? fontName)
    {
        if (string.IsNullOrEmpty(lvlText))
            return "\u2022"; // fallback bullet

        var ch = lvlText[0];

        if (fontName != null && fontName.Contains("Wingdings", StringComparison.OrdinalIgnoreCase))
        {
            // Wingdings PUA → Unicode mappings (common bullets)
            return ch switch
            {
                '\uf0d8' => "\u27A2", // ➢ right arrowhead
                '\uf0a7' => "\u25AA", // ▪ small black square
                '\uf0a8' => "\u25CB", // ○ white circle
                '\uf076' => "\u2756", // ❖ black diamond minus white X
                '\uf0FC' => "\u2714", // ✔ check mark
                '\uf0FB' => "\u2718", // ✘ cross mark
                '\uf0E8' => "\u25BA", // ► right-pointing triangle
                '\uf0D2' => "\u27A4", // ➤ right arrowhead (filled)
                _ => "\u2022", // fallback
            };
        }

        if (fontName != null && fontName.Contains("Symbol", StringComparison.OrdinalIgnoreCase))
        {
            return ch switch
            {
                '\uf0b7' => "\u2022", // • bullet
                '\uf0a7' => "\u2666", // ♦ diamond
                '\uf0B0' => "\u2218", // ∘ ring operator
                _ => "\u2022",
            };
        }

        // For standard fonts, use the character as-is if printable
        if (ch >= ' ')
            return lvlText;

        return "\u2022"; // fallback
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
                // OOXML w:suff: separator between the level's auto-number and the
                // body text. Default is "tab"; explicit "space" or "nothing"
                // suppresses the auto-tab snap so body text follows the number
                // immediately. Used by the renderer to decide whether to advance
                // to the next default tab stop for inherited-indent list paragraphs.
                var lvlSuff = lvl.Element(W + "suff")?.Attribute(W + "val")?.Value ?? "tab";
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
                // Read bullet font name from rPr/rFonts (e.g. Wingdings, Symbol)
                var lvlRPr = lvl.Element(W + "rPr");
                var lvlFontName = lvlRPr?.Element(W + "rFonts")?.Attribute(W + "ascii")?.Value;
                // Read bold from numbering level rPr (used for list label rendering)
                var lvlBoldEl = lvlRPr?.Element(W + "b");
                var lvlBold = lvlBoldEl != null && lvlBoldEl.Attribute(W + "val")?.Value is not ("0" or "false");
                levels.Add(new DocxNumberingLevelDef(ilvl, numFmt, lvlText, startVal, lvlIndentLeft, lvlHanging, lvlFontName, lvlBold, lvlSuff));
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
    string? DefaultFontName = null,
    string? DefaultEastAsiaFontName = null,
    List<DocxElement>? HeaderElements = null,
    List<DocxElement>? FooterElements = null,
    List<List<DocxElement>?>? SectionFooterElements = null,
    Dictionary<string, DocxFootnote>? Footnotes = null,
    List<DocxImage>? HeaderFooterImages = null,
    float DefaultTabStopPt = 36f,
    List<DocxElement>? FirstPageHeaderElements = null,
    List<DocxElement>? FirstPageFooterElements = null
);

/// <summary>Page layout settings from sectPr.</summary>
internal sealed record DocxPageLayout(
    float PageWidth = 612,
    float PageHeight = 792,
    float MarginTop = 72,
    float MarginBottom = 72,
    float MarginLeft = 72,
    float MarginRight = 72,
    float GridLinePitch = 0,
    float HeaderMargin = 36,
    float FooterMargin = 36,
    string SectionType = "nextPage",
    int ColumnCount = 1,
    float ColumnSpacing = 36,
    int PageNumStart = -1,
    float[]? ColumnWidths = null,
    float[]? ColumnGaps = null,
    bool TitlePg = false
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
    bool LineSpacingExact = false,
    float IndentLeft = 0,
    float IndentRight = 0,
    float IndentFirstLine = 0,
    bool IsBulletList = false,
    bool IsNumberedList = false,
    int ListLevel = 0,
    string? ListText = null,
    bool ListTextBold = false,
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
    bool ContextualSpacing = false,
    bool SnapToGrid = true,
    List<DocxFloatingTextBox>? FloatingTextBoxes = null,
    bool IsTextBoxFlow = false,
    float TextBoxWidth = 0,
    bool ParagraphMarkUnderline = false,
    string? ParagraphFontName = null,
    bool KeepNext = false,
    bool AutoSpaceDE = true,
    bool AutoSpaceDN = true,
    bool HasLastRenderedPageBreak = false,
    List<DocxConnectorLine>? ConnectorLines = null,
    // Font name declared by the numbering level rPr (e.g. "Wingdings", "Symbol").
    // Used so the bullet glyph is rendered with that family rather than falling
    // back to the run/text font, which often lacks the Wingdings-style glyph
    // that Word actually paints for codepoints like F0D8 (➢).
    string? ListFontName = null,
    // True when the paragraph's pPr/spacing/@after attribute was explicitly set
    // (vs inherited from style or docDefaults). Used so explicit paragraph spacing
    // wins over table style spacing per OOXML cascade order.
    bool SpacingAfterExplicit = false,
    // True when the paragraph's pPr/spacing/@before is explicitly set at the
    // paragraph level, OR when it is inherited from a non-Normal pStyle that
    // defines its own spacing-before. Used so the renderer can apply explicit
    // table-cell heading spacing-before (e.g., pStyle="TableHeading") at the
    // top of a cell while still suppressing inherited Normal/docDefault before
    // (which Word/LibreOffice collapse at cell boundaries).
    bool SpacingBeforeExplicit = false,
    // True when pPr/ind explicitly sets left/start/hanging numerically at the
    // paragraph level. When false (i.e., the list paragraph inherits its indent
    // from numbering or style), Word's auto-numbering "suff=tab" advances the
    // body text past the list label to the next default tab stop.
    bool HasExplicitListIndent = false,
    // OOXML w:suff for the resolved numbering level: "tab" (default), "space",
    // or "nothing". Renderer uses this to gate the auto-tab snap so body text
    // follows the number immediately when suff is "space" or "nothing".
    string ListSuff = "tab"
) : DocxElement;

/// <summary>Represents a single border edge.  Width=0 is a sentinel for an
/// explicit OOXML "nil"/"none" border that suppresses inheritance.</summary>
internal sealed record DocxBorderEdge(
    float Width,        // in points; 0 means explicit "nil" (suppress inherited border)
    PdfColor Color
)
{
    public static readonly DocxBorderEdge Nil = new(0f, new PdfColor(0, 0, 0));
}

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
    float CharSpacing = 0,
    string? FontName = null,
    bool HasExplicitUnderlineDecl = false,
    bool IsColumnBreak = false,
    float VerticalPosition = 0,
    string? FootnoteId = null,
    PdfColor? Shading = null
);

/// <summary>Represents a footnote definition parsed from word/footnotes.xml.</summary>
internal sealed record DocxFootnote(
    string Id,
    float FontSize,
    List<DocxRun> Runs
);

/// <summary>Represents an embedded image.</summary>
internal sealed record DocxImage(
    byte[] Data,
    string Extension,
    long WidthEmu = 0,
    long HeightEmu = 0,
    bool IsAnchor = false,
    long OffsetXEmu = 0,
    long OffsetYEmu = 0,
    bool IsBehindDoc = false,
    string? RelativeFromH = null,
    string? RelativeFromV = null,
    bool IsWrapTopBottom = false,
    float Alpha = 1f
);

/// <summary>Represents a connector line or straight line shape.</summary>
internal sealed record DocxConnectorLine(
    float X1Pt,
    float Y1Pt,
    float X2Pt,
    float Y2Pt,
    float LineWidthPt,
    PdfColor Color,
    float[]? DashPattern = null,
    bool HasTailArrow = false,
    bool HasHeadArrow = false,
    string HRelativeFrom = "column",
    string VRelativeFrom = "paragraph"
);

/// <summary>Represents a floating text box (wrapNone) with absolute position.</summary>
internal sealed record DocxFloatingTextBox(
    float XPt,
    float YPt,
    float WidthPt,
    float HeightPt,
    List<DocxParagraph> Paragraphs,
    DocxTextBoxBorder? Border = null,
    string HRelativeFrom = "column",
    string VRelativeFrom = "paragraph",
    PdfColor? FillColor = null,
    float TopInsetPt = 3.6f,
    float LeftInsetPt = 7.2f,
    string? HAlign = null,
    string? VAlign = null
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
    List<DocxCustomPath>? CustomPaths = null,
    PdfColor? StrokeColor = null,
    float StrokeWidthEmu = 0f,
    bool FillOnly = true,
    bool HasFill = true
);

/// <summary>Represents a table.</summary>
internal sealed record DocxTable(
    List<DocxTableRow> Rows,
    List<float> ColumnWidths,
    bool HasBorders = true,
    float CellMarginLeft = 5.4f,
    float CellMarginRight = 5.4f,
    float CellMarginTop = 0f,
    float CellMarginBottom = 0f,
    string Alignment = "left",
    DocxBorderEdge? BorderInsideH = null,
    DocxBorderEdge? BorderInsideV = null,
    DocxBorderEdge? BorderTop = null,
    DocxBorderEdge? BorderBottom = null,
    DocxBorderEdge? BorderLeft = null,
    DocxBorderEdge? BorderRight = null,
    float StyleLineSpacing = -1,
    float StyleSpacingAfter = -1,
    float IndentLeft = 0,
    bool IndentLeftExplicit = false,
    float CellSpacing = 0
) : DocxElement;

/// <summary>Represents a table row.</summary>
internal sealed record DocxTableRow(List<DocxTableCell> Cells, float Height = 0, bool HeightExact = false, int GridBefore = 0, bool IsHeader = false);

/// <summary>Represents a table cell.</summary>
internal sealed record DocxTableCell(
    List<DocxParagraph> Paragraphs,
    float Width = 0,
    int GridSpan = 1,
    PdfColor? Shading = null,
    DocxBorders? Borders = null,
    bool IsVMergeContinue = false,
    bool IsVMergeRestart = false,
    string VerticalAlignment = "top",
    float CellMarginTop = -1f,
    float CellMarginBottom = -1f,
    float CellMarginLeft = -1f,
    float CellMarginRight = -1f
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
    bool LineSpacingExact = false,
    bool ContextualSpacing = false,
    string? FontName = null,
    float CharSpacing = 0,
    bool KeepNext = false,
    string? VerticalAlign = null,
    float IndentLeft = 0,
    float IndentRight = 0,
    float IndentFirstLine = 0,
    bool HasIndentLeft = false,
    bool HasIndentRight = false,
    bool HasIndentFirstLine = false
);

/// <summary>Table style definition from styles.xml.</summary>
internal sealed record DocxTableStyleInfo(
    bool HasBorders = false,
    DocxBorderEdge? BorderTop = null,
    DocxBorderEdge? BorderBottom = null,
    DocxBorderEdge? BorderLeft = null,
    DocxBorderEdge? BorderRight = null,
    DocxBorderEdge? BorderInsideH = null,
    DocxBorderEdge? BorderInsideV = null,
    PdfColor? Band1HorzShading = null,
    PdfColor? Band2HorzShading = null,
    DocxBorders? FirstRowBorders = null,
    DocxBorders? FirstColBorders = null,
    float CellMarginTop = -1,
    float CellMarginBottom = -1,
    float CellMarginLeft = -1,
    float CellMarginRight = -1,
    float ParagraphSpacingAfter = -1,
    float ParagraphLineSpacing = -1,
    DocxBorders? Band1HorzBorders = null,
    DocxBorders? Band2HorzBorders = null
);

/// <summary>Numbering definition for lists.</summary>
internal sealed class DocxNumberingDef
{
    public string Format { get; }
    public List<DocxNumberingLevelDef> Levels { get; }
    private readonly Dictionary<int, int> _counters = new();

    public DocxNumberingDef(string format, List<DocxNumberingLevelDef>? levels = null)
    {
        Format = format;
        Levels = levels ?? [];
    }

    public string FormatListText(int ilvl)
    {
        var level = Levels.FirstOrDefault(l => l.Ilvl == ilvl) ?? Levels.FirstOrDefault();
        if (level == null)
        {
            var c = IncrementCounter(ilvl, 1);
            return c + ".";
        }

        var counter = IncrementCounter(ilvl, level.Start);
        // Replace all level placeholders (%1, %2, ...) with their respective counter values.
        // %1 = ilvl 0, %2 = ilvl 1, etc.  Iterate 9→1 to avoid partial matches.
        var text = level.LvlText;
        for (var i = 9; i >= 1; i--)
        {
            var placeholder = "%" + i;
            if (!text.Contains(placeholder)) continue;
            var levelIdx = i - 1;
            if (levelIdx == ilvl)
            {
                text = text.Replace(placeholder, FormatNumber(counter, level.NumFmt));
            }
            else
            {
                var refLevel = Levels.FirstOrDefault(l => l.Ilvl == levelIdx);
                var refFmt = refLevel?.NumFmt ?? "decimal";
                var refCounter = _counters.TryGetValue(levelIdx, out var rc) ? rc : 0;
                // When a parent-level counter was never explicitly set (e.g. TOC
                // multi-level lists where chapter headings lack numPr), fall back
                // to the level's start value so "%1.%2" renders "1.1" not ".1".
                if (refCounter == 0 && refLevel != null)
                    refCounter = refLevel.Start;
                text = text.Replace(placeholder, refCounter > 0 ? FormatNumber(refCounter, refFmt) : "");
            }
        }
        return text;
    }

    private int IncrementCounter(int ilvl, int startVal)
    {
        // Word restarts deeper list levels when a parent level advances.
        foreach (var level in _counters.Keys.Where(k => k > ilvl).ToList())
            _counters.Remove(level);

        if (!_counters.TryGetValue(ilvl, out var current))
        {
            _counters[ilvl] = startVal;
            return startVal;
        }
        _counters[ilvl] = current + 1;
        return current + 1;
    }

    private static string FormatNumber(int num, string fmt) => fmt switch
    {
        "decimal" => num.ToString(),
        "upperLetter" => num >= 1 && num <= 26 ? ((char)('A' + num - 1)).ToString() : num.ToString(),
        "lowerLetter" => num >= 1 && num <= 26 ? ((char)('a' + num - 1)).ToString() : num.ToString(),
        "upperRoman" => ToRoman(num),
        "lowerRoman" => ToRoman(num).ToLowerInvariant(),
        "japaneseCounting" or "chineseCounting" or "chineseCountingThousand" or "taiwaneseCountingThousand" or "taiwaneseCounting" =>
            FormatChineseCounting(num),
        "ideographTraditional" =>
            num >= 1 && num <= 10 ? "\u7532\u4e59\u4e19\u4e01\u620a\u5df1\u5e9a\u8f9b\u58ec\u7678"[num - 1].ToString() : num.ToString(),
        _ => num.ToString()
    };

    private static string FormatChineseCounting(int num)
    {
        if (num <= 0 || num > 9999) return num.ToString();
        string[] d = ["零", "一", "二", "三", "四", "五", "六", "七", "八", "九"];
        if (num <= 10) return num == 10 ? "十" : d[num];
        if (num < 20) return "十" + d[num - 10];
        if (num < 100)
        {
            var t = num / 10; var o = num % 10;
            return d[t] + "十" + (o > 0 ? d[o] : "");
        }
        if (num < 1000)
        {
            var h = num / 100; var rem = num % 100;
            var s = d[h] + "百";
            if (rem == 0) return s;
            if (rem < 10) return s + "零" + d[rem];
            return s + FormatChineseCounting(rem);
        }
        var th = num / 1000; var r = num % 1000;
        var str = d[th] + "千";
        if (r == 0) return str;
        if (r < 100) return str + "零" + FormatChineseCounting(r);
        return str + FormatChineseCounting(r);
    }

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

internal sealed record DocxNumberingLevelDef(int Ilvl, string NumFmt, string LvlText, int Start, float IndentLeft = 0, float Hanging = 0, string? FontName = null, bool Bold = false, string Suff = "tab");
