using System.Globalization;
using System.IO.Compression;
using System.Xml.Linq;

namespace MiniSoftware;

internal sealed class PptxDocument
{
    public float SlideWidth { get; }
    public float SlideHeight { get; }
    public List<PptxSlide> Slides { get; }

    public PptxDocument(float slideWidth, float slideHeight, List<PptxSlide> slides)
    {
        SlideWidth = slideWidth;
        SlideHeight = slideHeight;
        Slides = slides;
    }
}

internal sealed class PptxSlide
{
    public float Width { get; }
    public float Height { get; }
    public PdfColor? BackgroundColor { get; }
    public List<PptxElement> Elements { get; }

    public PptxSlide(float width, float height, PdfColor? backgroundColor, List<PptxElement> elements)
    {
        Width = width;
        Height = height;
        BackgroundColor = backgroundColor;
        Elements = elements;
    }
}

internal abstract class PptxElement
{
}

internal sealed class PptxShape : PptxElement
{
    public string ShapeType { get; }
    public PptxRect Bounds { get; }
    public PdfColor? FillColor { get; }
    public PptxOutline? Outline { get; }
    public List<PptxTextParagraph> Paragraphs { get; }
    public PptxTextBodyProperties TextBodyProperties { get; }

    public PptxShape(string shapeType, PptxRect bounds, PdfColor? fillColor, PptxOutline? outline, List<PptxTextParagraph> paragraphs, PptxTextBodyProperties textBodyProperties)
    {
        ShapeType = shapeType;
        Bounds = bounds;
        FillColor = fillColor;
        Outline = outline;
        Paragraphs = paragraphs;
        TextBodyProperties = textBodyProperties;
    }

    public PptxShape(string shapeType, PptxRect bounds, PdfColor? fillColor, PptxOutline? outline, List<PptxTextParagraph> paragraphs)
        : this(shapeType, bounds, fillColor, outline, paragraphs, PptxTextBodyProperties.Default)
    {
    }
}

internal readonly struct PptxTextBodyProperties
{
    public static readonly PptxTextBodyProperties Default = new(0, 0, 0, 0, "top");

    public float LeftInset { get; }
    public float TopInset { get; }
    public float RightInset { get; }
    public float BottomInset { get; }
    public string VerticalAnchor { get; }

    public PptxTextBodyProperties(float leftInset, float topInset, float rightInset, float bottomInset, string verticalAnchor)
    {
        LeftInset = leftInset;
        TopInset = topInset;
        RightInset = rightInset;
        BottomInset = bottomInset;
        VerticalAnchor = verticalAnchor;
    }
}

internal sealed class PptxPicture : PptxElement
{
    public PptxRect Bounds { get; }
    public byte[] Data { get; }
    public string Format { get; }
    public PptxCrop Crop { get; }

    public PptxPicture(PptxRect bounds, byte[] data, string format, PptxCrop crop)
    {
        Bounds = bounds;
        Data = data;
        Format = format;
        Crop = crop;
    }
}

internal sealed class PptxLine : PptxElement
{
    public float X1 { get; }
    public float Y1 { get; }
    public float X2 { get; }
    public float Y2 { get; }
    public PptxOutline Outline { get; }

    public PptxLine(float x1, float y1, float x2, float y2, PptxOutline outline)
    {
        X1 = x1;
        Y1 = y1;
        X2 = x2;
        Y2 = y2;
        Outline = outline;
    }
}

internal sealed class PptxTextParagraph
{
    public List<PptxTextRun> Runs { get; }
    public bool IsBullet { get; }
    public string Alignment { get; }
    public float MarginLeft { get; }
    public float Indent { get; }
    public float SpaceBefore { get; }
    public float? LineSpacing { get; }

    public PptxTextParagraph(List<PptxTextRun> runs, bool isBullet = false, string alignment = "left", float marginLeft = 0f, float indent = 0f, float spaceBefore = 0f, float? lineSpacing = null)
    {
        Runs = runs;
        IsBullet = isBullet;
        Alignment = alignment;
        MarginLeft = marginLeft;
        Indent = indent;
        SpaceBefore = spaceBefore;
        LineSpacing = lineSpacing;
    }
}

internal sealed class PptxTextRun
{
    public string Text { get; }
    public float FontSize { get; }
    public PdfColor Color { get; }
    public bool Bold { get; }
    public bool Italic { get; }
    public bool Underline { get; }
    public string? FontName { get; }

    public PptxTextRun(string text, float fontSize, PdfColor color, bool bold, bool italic, bool underline, string? fontName)
    {
        Text = text;
        FontSize = fontSize;
        Color = color;
        Bold = bold;
        Italic = italic;
        Underline = underline;
        FontName = fontName;
    }
}

internal sealed class PptxOutline
{
    public PdfColor Color { get; }
    public float Width { get; }
    public float[]? DashPattern { get; }

    public PptxOutline(PdfColor color, float width, float[]? dashPattern = null)
    {
        Color = color;
        Width = width;
        DashPattern = dashPattern;
    }
}

internal readonly struct PptxRect
{
    public float X { get; }
    public float Y { get; }
    public float Width { get; }
    public float Height { get; }

    public PptxRect(float x, float y, float width, float height)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
    }
}

internal readonly struct PptxCrop
{
    public static readonly PptxCrop None = new(0, 0, 0, 0);

    public float Left { get; }
    public float Top { get; }
    public float Right { get; }
    public float Bottom { get; }

    public PptxCrop(float left, float top, float right, float bottom)
    {
        Left = left;
        Top = top;
        Right = right;
        Bottom = bottom;
    }
}

internal static class PptxReader
{
    private const double EmusPerPoint = 12700d;
    private const long DefaultSlideWidthEmu = 9144000;
    private const long DefaultSlideHeightEmu = 6858000;

    private static readonly XNamespace P = "http://schemas.openxmlformats.org/presentationml/2006/main";
    private static readonly XNamespace A = "http://schemas.openxmlformats.org/drawingml/2006/main";
    private static readonly XNamespace Dsp = "http://schemas.microsoft.com/office/drawing/2008/diagram";
    private static readonly XNamespace R = "http://schemas.openxmlformats.org/officeDocument/2006/relationships";
    private static readonly XNamespace Rel = "http://schemas.openxmlformats.org/package/2006/relationships";

    public static PptxDocument Read(Stream pptxStream)
    {
        if (pptxStream is null)
            throw new ArgumentNullException(nameof(pptxStream));

        using var archive = new ZipArchive(pptxStream, ZipArchiveMode.Read, leaveOpen: true);
        var presentationEntry = archive.GetEntry("ppt/presentation.xml")
            ?? throw new InvalidDataException("The PPTX package is missing ppt/presentation.xml.");

        var presentationXml = LoadXml(presentationEntry);
        var presentationRelationships = ReadRelationships(archive, "ppt/presentation.xml");
        var themeColors = ReadThemeColors(archive, presentationRelationships);

        var slideSize = presentationXml.Root?.Element(P + "sldSz");
        var slideWidthEmu = ReadLong(slideSize?.Attribute("cx")?.Value, DefaultSlideWidthEmu);
        var slideHeightEmu = ReadLong(slideSize?.Attribute("cy")?.Value, DefaultSlideHeightEmu);
        var slideWidth = EmuToPoint(slideWidthEmu);
        var slideHeight = EmuToPoint(slideHeightEmu);

        var slides = new List<PptxSlide>();
        var slideIdElements = presentationXml.Root?
            .Element(P + "sldIdLst")?
            .Elements(P + "sldId")
            .ToList() ?? new List<XElement>();

        foreach (var slideIdElement in slideIdElements)
        {
            var relationshipId = slideIdElement.Attribute(R + "id")?.Value;
            if (string.IsNullOrWhiteSpace(relationshipId))
                continue;
            if (!presentationRelationships.TryGetValue(relationshipId!, out var relationship))
                continue;

            var slidePartPath = ResolveRelationshipTarget("ppt/presentation.xml", relationship.Target);
            var slideEntry = archive.GetEntry(slidePartPath);
            if (slideEntry == null)
                continue;

            slides.Add(ReadSlide(archive, slideEntry, slidePartPath, slideWidth, slideHeight, slideWidthEmu, slideHeightEmu, themeColors));
        }

        if (slides.Count == 0)
        {
            var fallbackSlides = archive.Entries
                .Where(entry => entry.FullName.StartsWith("ppt/slides/slide", StringComparison.OrdinalIgnoreCase)
                    && entry.FullName.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
                .OrderBy(entry => entry.FullName, StringComparer.OrdinalIgnoreCase)
                .ToList();

            foreach (var slideEntry in fallbackSlides)
                slides.Add(ReadSlide(archive, slideEntry, slideEntry.FullName, slideWidth, slideHeight, slideWidthEmu, slideHeightEmu, themeColors));
        }

        return new PptxDocument(slideWidth, slideHeight, slides);
    }

    private static PptxSlide ReadSlide(
        ZipArchive archive,
        ZipArchiveEntry slideEntry,
        string slidePartPath,
        float slideWidth,
        float slideHeight,
        long slideWidthEmu,
        long slideHeightEmu,
        Dictionary<string, PdfColor> themeColors)
    {
        var slideXml = LoadXml(slideEntry);
        var slideRelationships = ReadRelationships(archive, slidePartPath);
        var layoutPart = ReadRelatedPart(archive, slidePartPath, slideRelationships, "/slideLayout");
        var masterPart = layoutPart == null
            ? null
            : ReadRelatedPart(archive, layoutPart.PartPath, layoutPart.Relationships, "/slideMaster");
        var slideThemeColors = ApplyColorMap(themeColors, slideXml, layoutPart?.Xml, masterPart?.Xml);
        var backgroundColor = ReadSlideBackground(slideXml, slideThemeColors)
            ?? (layoutPart == null ? null : ReadSlideBackground(layoutPart.Xml, slideThemeColors))
            ?? (masterPart == null ? null : ReadSlideBackground(masterPart.Xml, slideThemeColors));
        var elements = new List<PptxElement>();
        var rootMap = new CoordinateMap(0, 0, slideWidthEmu, slideHeightEmu, 0, 0, slideWidthEmu, slideHeightEmu);
        var placeholderDefaults = BuildPlaceholderDefaults(layoutPart?.Xml, masterPart?.Xml);

        var masterShapeTree = masterPart?.Xml.Root?
            .Element(P + "cSld")?
            .Element(P + "spTree");
        if (masterShapeTree != null)
            ReadShapeTree(archive, masterShapeTree, masterPart!.Relationships, slideThemeColors, rootMap, elements, null, includePlaceholderShapes: false);

        var layoutShapeTree = layoutPart?.Xml.Root?
            .Element(P + "cSld")?
            .Element(P + "spTree");
        if (layoutShapeTree != null)
            ReadShapeTree(archive, layoutShapeTree, layoutPart!.Relationships, slideThemeColors, rootMap, elements, null, includePlaceholderShapes: false);

        var shapeTree = slideXml.Root?
            .Element(P + "cSld")?
            .Element(P + "spTree");

        if (shapeTree != null)
            ReadShapeTree(archive, shapeTree, slideRelationships, slideThemeColors, rootMap, elements, placeholderDefaults);

        return new PptxSlide(slideWidth, slideHeight, backgroundColor, elements);
    }

    private static void ReadShapeTree(
        ZipArchive archive,
        XElement container,
        Dictionary<string, PptxRelationship> relationships,
        Dictionary<string, PdfColor> themeColors,
        CoordinateMap coordinateMap,
        List<PptxElement> elements,
        Dictionary<PptxPlaceholderKey, XElement>? placeholderDefaults = null,
        bool includePlaceholderShapes = true)
    {
        foreach (var child in container.Elements())
        {
            if (child.Name == P + "sp")
            {
                if (!includePlaceholderShapes && IsPlaceholderElement(child))
                    continue;

                ReadShape(child, themeColors, coordinateMap, elements, placeholderDefaults);
            }
            else if (child.Name == P + "pic")
            {
                if (!includePlaceholderShapes && IsPlaceholderElement(child))
                    continue;

                ReadPicture(archive, child, relationships, coordinateMap, elements);
            }
            else if (child.Name == P + "cxnSp")
            {
                ReadConnector(child, themeColors, coordinateMap, elements);
            }
            else if (child.Name == P + "graphicFrame")
            {
                ReadGraphicFrame(archive, child, relationships, themeColors, coordinateMap, elements);
            }
            else if (child.Name == P + "grpSp")
            {
                var groupMap = CreateGroupMap(child.Element(P + "grpSpPr")?.Element(A + "xfrm"), coordinateMap);
                ReadShapeTree(archive, child, relationships, themeColors, groupMap, elements, placeholderDefaults, includePlaceholderShapes);
            }
        }
    }

    private static void ReadShape(
        XElement shapeElement,
        Dictionary<string, PdfColor> themeColors,
        CoordinateMap coordinateMap,
        List<PptxElement> elements,
        Dictionary<PptxPlaceholderKey, XElement>? placeholderDefaults)
    {
        var inheritedShape = FindPlaceholderDefault(shapeElement, placeholderDefaults);
        var shapeProperties = shapeElement.Element(P + "spPr");
        var inheritedShapeProperties = inheritedShape?.Element(P + "spPr");
        var bounds = ReadBounds(shapeProperties?.Element(A + "xfrm") ?? inheritedShapeProperties?.Element(A + "xfrm"), coordinateMap);
        if (bounds.Width <= 0 || bounds.Height <= 0)
            return;

        var shapeType = shapeProperties?
            .Element(A + "prstGeom")?
            .Attribute("prst")?
            .Value
            ?? inheritedShapeProperties?
                .Element(A + "prstGeom")?
                .Attribute("prst")?
                .Value
            ?? "rect";

        var outline = ReadOutline(shapeProperties?.Element(A + "ln"), themeColors)
            ?? ReadStyleOutline(shapeElement.Element(P + "style"), themeColors)
            ?? ReadOutline(inheritedShapeProperties?.Element(A + "ln"), themeColors)
            ?? ReadStyleOutline(inheritedShape?.Element(P + "style"), themeColors);

        if (string.Equals(shapeType, "line", StringComparison.OrdinalIgnoreCase))
        {
            AddLineFromBounds(shapeProperties?.Element(A + "xfrm"), bounds, outline ?? new PptxOutline(PdfColor.Black, 1f), elements);
            return;
        }

        var fillColor = ReadShapeFill(shapeProperties, themeColors)
            ?? ReadStyleFill(shapeElement.Element(P + "style"), themeColors)
            ?? ReadShapeFill(inheritedShapeProperties, themeColors)
            ?? ReadStyleFill(inheritedShape?.Element(P + "style"), themeColors);
        var textBodyProperties = ReadTextBodyProperties(shapeElement, inheritedShape);
        var paragraphs = ReadTextParagraphs(shapeElement, inheritedShape, themeColors);

        if (fillColor == null && outline == null && paragraphs.Count == 0)
            return;

        elements.Add(new PptxShape(shapeType, bounds, fillColor, outline, paragraphs, textBodyProperties));
    }

    private static void ReadPicture(
        ZipArchive archive,
        XElement pictureElement,
        Dictionary<string, PptxRelationship> relationships,
        CoordinateMap coordinateMap,
        List<PptxElement> elements)
    {
        if (ReadBool(pictureElement.Element(P + "nvPicPr")?.Element(P + "cNvPr")?.Attribute("hidden")?.Value))
            return;

        var bounds = ReadBounds(pictureElement.Element(P + "spPr")?.Element(A + "xfrm"), coordinateMap);
        if (bounds.Width <= 0 || bounds.Height <= 0)
            return;

        var embedId = pictureElement
            .Element(P + "blipFill")?
            .Element(A + "blip")?
            .Attribute(R + "embed")?
            .Value
            ?? pictureElement
                .Element(P + "blipFill")?
                .Element(A + "blip")?
                .Descendants()
                .FirstOrDefault(element => element.Name.LocalName == "svgBlip")?
                .Attribute(R + "embed")?
                .Value;
        if (string.IsNullOrWhiteSpace(embedId))
            return;
        if (!relationships.TryGetValue(embedId!, out var relationship))
            return;
        if (relationship.TargetMode.Equals("External", StringComparison.OrdinalIgnoreCase))
            return;

        var sourcePartPath = GetSourcePartFromRelationshipPath(relationship.RelationshipPartPath);
        var mediaPath = ResolveRelationshipTarget(sourcePartPath, relationship.Target);
        var mediaEntry = archive.GetEntry(mediaPath);
        if (mediaEntry == null)
            return;

        var data = ReadEntryBytes(mediaEntry);
        var format = DetectImageFormat(mediaPath, data);
        if (format == null)
            return;

        var crop = ReadPictureCrop(pictureElement.Element(P + "blipFill")?.Element(A + "srcRect"));
        elements.Add(new PptxPicture(bounds, data, format, crop));
    }

    private static PptxCrop ReadPictureCrop(XElement? sourceRect)
    {
        if (sourceRect == null)
            return PptxCrop.None;

        return new PptxCrop(
            ReadCropValue(sourceRect.Attribute("l")?.Value),
            ReadCropValue(sourceRect.Attribute("t")?.Value),
            ReadCropValue(sourceRect.Attribute("r")?.Value),
            ReadCropValue(sourceRect.Attribute("b")?.Value));
    }

    private static float ReadCropValue(string? value)
    {
        if (!float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
            return 0f;
        return Compat.Clamp(result / 100000f, 0f, 0.95f);
    }

    private static void ReadConnector(
        XElement connectorElement,
        Dictionary<string, PdfColor> themeColors,
        CoordinateMap coordinateMap,
        List<PptxElement> elements)
    {
        var shapeProperties = connectorElement.Element(P + "spPr");
        var bounds = ReadBounds(shapeProperties?.Element(A + "xfrm"), coordinateMap);
        if (bounds.Width <= 0 && bounds.Height <= 0)
            return;

        var outline = ReadOutline(shapeProperties?.Element(A + "ln"), themeColors)
            ?? new PptxOutline(PdfColor.Black, 1f);
        AddLineFromBounds(shapeProperties?.Element(A + "xfrm"), bounds, outline, elements);
    }

    private static void ReadGraphicFrame(
        ZipArchive archive,
        XElement graphicFrameElement,
        Dictionary<string, PptxRelationship> relationships,
        Dictionary<string, PdfColor> themeColors,
        CoordinateMap coordinateMap,
        List<PptxElement> elements)
    {
        var bounds = ReadBounds(graphicFrameElement.Element(P + "xfrm"), coordinateMap);
        if (bounds.Width <= 0 || bounds.Height <= 0)
            return;

        var graphicData = graphicFrameElement
            .Element(A + "graphic")?
            .Element(A + "graphicData");
        if (graphicData == null)
            return;

        if (graphicData.Attribute("uri")?.Value == "http://schemas.openxmlformats.org/drawingml/2006/diagram")
        {
            ReadDiagramDrawingFrame(archive, graphicFrameElement, relationships, themeColors, bounds, elements);
            return;
        }

        var table = graphicData.Element(A + "tbl");
        if (table == null)
            return;

        var columnWidths = table.Element(A + "tblGrid")?
            .Elements(A + "gridCol")
            .Select(column => ReadLong(column.Attribute("w")?.Value, 0))
            .Where(width => width > 0)
            .ToList() ?? new List<long>();
        var rows = table.Elements(A + "tr").ToList();
        if (columnWidths.Count == 0 || rows.Count == 0)
            return;

        var totalColumnWidth = columnWidths.Sum();
        var rowHeights = rows.Select(row => ReadLong(row.Attribute("h")?.Value, 0)).ToList();
        var totalRowHeight = rowHeights.Where(height => height > 0).Sum();
        if (totalColumnWidth <= 0 || totalRowHeight <= 0)
            return;

        var top = bounds.Y;
        for (var rowIndex = 0; rowIndex < rows.Count; rowIndex++)
        {
            var row = rows[rowIndex];
            var rowHeight = bounds.Height * rowHeights[rowIndex] / totalRowHeight;
            var left = bounds.X;
            var columnIndex = 0;

            foreach (var cell in row.Elements(A + "tc"))
            {
                if (columnIndex >= columnWidths.Count)
                    break;

                var gridSpan = (int)Math.Max(1, ReadLong(cell.Attribute("gridSpan")?.Value, 1));
                var spannedWidth = 0L;
                for (var i = 0; i < gridSpan && columnIndex + i < columnWidths.Count; i++)
                    spannedWidth += columnWidths[columnIndex + i];

                var cellWidth = bounds.Width * spannedWidth / totalColumnWidth;
                var cellBounds = new PptxRect(left, top, cellWidth, rowHeight);
                var cellProperties = cell.Element(A + "tcPr");
                var fillColor = ReadShapeFill(cellProperties, themeColors);
                var paragraphs = ReadTextParagraphsFromTextBody(cell.Element(A + "txBody"), null, themeColors);
                if (fillColor != null || paragraphs.Count > 0)
                    elements.Add(new PptxShape("rect", cellBounds, fillColor, null, paragraphs, PptxTextBodyProperties.Default));

                AddTableBorderLines(cellProperties, cellBounds, themeColors, elements);

                left += cellWidth;
                columnIndex += gridSpan;
            }

            top += rowHeight;
        }
    }

    private static void ReadDiagramDrawingFrame(
        ZipArchive archive,
        XElement graphicFrameElement,
        Dictionary<string, PptxRelationship> relationships,
        Dictionary<string, PdfColor> themeColors,
        PptxRect frameBounds,
        List<PptxElement> elements)
    {
        var relationship = relationships.Values.FirstOrDefault(candidate =>
            candidate.Type.EndsWith("/diagramDrawing", StringComparison.OrdinalIgnoreCase));
        if (relationship == null || relationship.TargetMode.Equals("External", StringComparison.OrdinalIgnoreCase))
            return;

        var sourcePartPath = GetSourcePartFromRelationshipPath(relationship.RelationshipPartPath);
        var drawingPath = ResolveRelationshipTarget(sourcePartPath, relationship.Target);
        var drawingEntry = archive.GetEntry(drawingPath);
        if (drawingEntry == null)
            return;

        var frameTransform = graphicFrameElement.Element(P + "xfrm");
        var frameExtent = frameTransform?.Element(A + "ext");
        var frameWidthEmu = ReadLong(frameExtent?.Attribute("cx")?.Value, 0);
        var frameHeightEmu = ReadLong(frameExtent?.Attribute("cy")?.Value, 0);
        if (frameWidthEmu <= 0 || frameHeightEmu <= 0)
            return;

        var drawingMap = new CoordinateMap(
            frameBounds.X * EmusPerPoint,
            frameBounds.Y * EmusPerPoint,
            frameBounds.Width * EmusPerPoint,
            frameBounds.Height * EmusPerPoint,
            0,
            0,
            frameWidthEmu,
            frameHeightEmu);

        var drawingXml = LoadXml(drawingEntry);
        var shapeTree = drawingXml.Root?.Element(Dsp + "spTree");
        if (shapeTree == null)
            return;

        foreach (var shape in shapeTree.Elements(Dsp + "sp"))
            ReadDiagramShape(shape, themeColors, drawingMap, elements);
    }

    private static void ReadDiagramShape(
        XElement shapeElement,
        Dictionary<string, PdfColor> themeColors,
        CoordinateMap coordinateMap,
        List<PptxElement> elements)
    {
        var shapeProperties = shapeElement.Element(Dsp + "spPr");
        var bounds = ReadBounds(shapeProperties?.Element(A + "xfrm"), coordinateMap);
        if (bounds.Width <= 0 || bounds.Height <= 0)
            return;

        var shapeType = shapeProperties?
            .Element(A + "prstGeom")?
            .Attribute("prst")?
            .Value ?? "rect";
        var fillColor = ReadShapeFill(shapeProperties, themeColors);
        var outline = ReadOutline(shapeProperties?.Element(A + "ln"), themeColors);
        var paragraphs = ReadTextParagraphsFromTextBody(shapeElement.Element(Dsp + "txBody"), null, themeColors);

        if (fillColor == null && outline == null && paragraphs.Count == 0)
            return;

        elements.Add(new PptxShape(shapeType, bounds, fillColor, outline, paragraphs, PptxTextBodyProperties.Default));
    }

    private static void AddTableBorderLines(XElement? cellProperties, PptxRect bounds, Dictionary<string, PdfColor> themeColors, List<PptxElement> elements)
    {
        AddBorderLine(cellProperties?.Element(A + "lnL"), bounds.X, bounds.Y, bounds.X, bounds.Y + bounds.Height, themeColors, elements);
        AddBorderLine(cellProperties?.Element(A + "lnR"), bounds.X + bounds.Width, bounds.Y, bounds.X + bounds.Width, bounds.Y + bounds.Height, themeColors, elements);
        AddBorderLine(cellProperties?.Element(A + "lnT"), bounds.X, bounds.Y, bounds.X + bounds.Width, bounds.Y, themeColors, elements);
        AddBorderLine(cellProperties?.Element(A + "lnB"), bounds.X, bounds.Y + bounds.Height, bounds.X + bounds.Width, bounds.Y + bounds.Height, themeColors, elements);
    }

    private static void AddBorderLine(XElement? lineElement, float x1, float y1, float x2, float y2, Dictionary<string, PdfColor> themeColors, List<PptxElement> elements)
    {
        var outline = ReadOutline(lineElement, themeColors);
        if (outline != null)
            elements.Add(new PptxLine(x1, y1, x2, y2, outline));
    }

    private static void AddLineFromBounds(XElement? transformElement, PptxRect bounds, PptxOutline outline, List<PptxElement> elements)
    {
        var flipHorizontal = ReadBool(transformElement?.Attribute("flipH")?.Value);
        var flipVertical = ReadBool(transformElement?.Attribute("flipV")?.Value);

        var x1 = flipHorizontal ? bounds.X + bounds.Width : bounds.X;
        var x2 = flipHorizontal ? bounds.X : bounds.X + bounds.Width;
        var y1 = flipVertical ? bounds.Y + bounds.Height : bounds.Y;
        var y2 = flipVertical ? bounds.Y : bounds.Y + bounds.Height;

        elements.Add(new PptxLine(x1, y1, x2, y2, outline));
    }

    private static PptxRect ReadBounds(XElement? transformElement, CoordinateMap coordinateMap)
    {
        if (transformElement == null)
            return new PptxRect(0, 0, 0, 0);

        var offset = transformElement.Element(A + "off");
        var extent = transformElement.Element(A + "ext");
        var x = ReadLong(offset?.Attribute("x")?.Value, 0);
        var y = ReadLong(offset?.Attribute("y")?.Value, 0);
        var width = ReadLong(extent?.Attribute("cx")?.Value, 0);
        var height = ReadLong(extent?.Attribute("cy")?.Value, 0);

        return coordinateMap.MapRect(x, y, width, height);
    }

    private static CoordinateMap CreateGroupMap(XElement? transformElement, CoordinateMap parentMap)
    {
        if (transformElement == null)
            return parentMap;

        var offset = transformElement.Element(A + "off");
        var extent = transformElement.Element(A + "ext");
        var childOffset = transformElement.Element(A + "chOff");
        var childExtent = transformElement.Element(A + "chExt");

        var groupX = ReadLong(offset?.Attribute("x")?.Value, 0);
        var groupY = ReadLong(offset?.Attribute("y")?.Value, 0);
        var groupWidth = ReadLong(extent?.Attribute("cx")?.Value, 0);
        var groupHeight = ReadLong(extent?.Attribute("cy")?.Value, 0);
        if (groupWidth <= 0 || groupHeight <= 0)
            return parentMap;

        var childX = ReadLong(childOffset?.Attribute("x")?.Value, groupX);
        var childY = ReadLong(childOffset?.Attribute("y")?.Value, groupY);
        var childWidth = ReadLong(childExtent?.Attribute("cx")?.Value, groupWidth);
        var childHeight = ReadLong(childExtent?.Attribute("cy")?.Value, groupHeight);

        var slideX = parentMap.MapX(groupX);
        var slideY = parentMap.MapY(groupY);
        var slideRight = parentMap.MapX(groupX + groupWidth);
        var slideBottom = parentMap.MapY(groupY + groupHeight);

        return new CoordinateMap(slideX, slideY, slideRight - slideX, slideBottom - slideY, childX, childY, childWidth, childHeight);
    }

    private static List<PptxTextParagraph> ReadTextParagraphs(XElement shapeElement, XElement? inheritedShape, Dictionary<string, PdfColor> themeColors)
    {
        var textBody = shapeElement.Element(P + "txBody");
        if (textBody == null)
            return new List<PptxTextParagraph>();

        return ReadTextParagraphsFromTextBody(textBody, inheritedShape, themeColors);
    }

    private static List<PptxTextParagraph> ReadTextParagraphsFromTextBody(XElement? textBody, XElement? inheritedShape, Dictionary<string, PdfColor> themeColors)
    {
        if (textBody == null)
            return new List<PptxTextParagraph>();

        var paragraphs = new List<PptxTextParagraph>();
        foreach (var paragraphElement in textBody.Elements(A + "p"))
        {
            var level = ReadParagraphLevel(paragraphElement);
            var paragraphProperties = paragraphElement.Element(A + "pPr");
            var inheritedParagraphProperties = ReadInheritedParagraphProperties(inheritedShape, level);
            var defaultRunProperties = paragraphProperties?.Element(A + "defRPr")
                ?? inheritedParagraphProperties?.Element(A + "defRPr");
            var isBullet = IsBulletParagraph(paragraphElement, inheritedShape, level);
            var alignment = ReadParagraphAlignment(paragraphProperties, inheritedParagraphProperties);
            var marginLeft = ReadParagraphPointAttribute(paragraphProperties, inheritedParagraphProperties, "marL", 0f);
            var indent = ReadParagraphPointAttribute(paragraphProperties, inheritedParagraphProperties, "indent", 0f);
            var spaceBefore = ReadSpacing(paragraphProperties?.Element(A + "spcBef"), inheritedParagraphProperties?.Element(A + "spcBef"));
            var lineSpacing = ReadLineSpacing(paragraphProperties?.Element(A + "lnSpc"), inheritedParagraphProperties?.Element(A + "lnSpc"));
            var runs = new List<PptxTextRun>();
            var hasText = false;

            foreach (var child in paragraphElement.Elements())
            {
                if (child.Name == A + "r" || child.Name == A + "fld")
                {
                    var runProperties = child.Element(A + "rPr");
                    var text = child.Element(A + "t")?.Value ?? string.Empty;
                    if (text.Length == 0)
                        continue;
                    if (isBullet && !hasText)
                        text = "\u2022 " + text;
                    runs.Add(ReadTextRun(text, runProperties, defaultRunProperties, themeColors));
                    hasText = true;
                }
                else if (child.Name == A + "br")
                {
                    runs.Add(ReadTextRun("\n", child.Element(A + "rPr"), defaultRunProperties, themeColors));
                }
            }

            paragraphs.Add(new PptxTextParagraph(runs, isBullet, alignment, marginLeft, indent, spaceBefore, lineSpacing));
        }

        return paragraphs;
    }

    private static PptxTextBodyProperties ReadTextBodyProperties(XElement shapeElement, XElement? inheritedShape)
    {
        var bodyProperties = shapeElement.Element(P + "txBody")?.Element(A + "bodyPr");
        var inheritedBodyProperties = inheritedShape?.Element(P + "txBody")?.Element(A + "bodyPr");

        return new PptxTextBodyProperties(
            ReadInset(bodyProperties, inheritedBodyProperties, "lIns"),
            ReadInset(bodyProperties, inheritedBodyProperties, "tIns"),
            ReadInset(bodyProperties, inheritedBodyProperties, "rIns"),
            ReadInset(bodyProperties, inheritedBodyProperties, "bIns"),
            ReadVerticalAnchor(bodyProperties, inheritedBodyProperties));
    }

    private static float ReadInset(XElement? bodyProperties, XElement? inheritedBodyProperties, string attributeName)
    {
        return ReadEmuToPoint(bodyProperties?.Attribute(attributeName)?.Value)
            ?? ReadEmuToPoint(inheritedBodyProperties?.Attribute(attributeName)?.Value)
            ?? 0f;
    }

    private static string ReadVerticalAnchor(XElement? bodyProperties, XElement? inheritedBodyProperties)
    {
        var anchor = bodyProperties?.Attribute("anchor")?.Value
            ?? inheritedBodyProperties?.Attribute("anchor")?.Value;

        return anchor?.ToLowerInvariant() switch
        {
            "ctr" => "middle",
            "b" => "bottom",
            _ => "top",
        };
    }

    private static string ReadParagraphAlignment(XElement? paragraphProperties, XElement? inheritedParagraphProperties)
    {
        return (paragraphProperties?.Attribute("algn")?.Value ?? inheritedParagraphProperties?.Attribute("algn")?.Value)?.ToLowerInvariant() switch
        {
            "ctr" => "center",
            "r" => "right",
            _ => "left",
        };
    }

    private static XElement? ReadInheritedParagraphProperties(XElement? inheritedShape, int level)
    {
        if (inheritedShape == null)
            return null;

        var listStyle = inheritedShape.Element(P + "txBody")?.Element(A + "lstStyle");
        return listStyle?.Element(A + $"lvl{level + 1}pPr")
            ?? listStyle?.Element(A + "lvl1pPr");
    }

    private static float ReadParagraphPointAttribute(XElement? paragraphProperties, XElement? inheritedParagraphProperties, string attributeName, float fallback)
    {
        return ReadEmuToPoint(paragraphProperties?.Attribute(attributeName)?.Value)
            ?? ReadEmuToPoint(inheritedParagraphProperties?.Attribute(attributeName)?.Value)
            ?? fallback;
    }

    private static float? ReadEmuToPoint(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;
        return long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var emu)
            ? emu / (float)EmusPerPoint
            : null;
    }

    private static float ReadSpacing(XElement? spacingElement, XElement? inheritedSpacingElement)
    {
        return ReadSpacingValue(spacingElement)
            ?? ReadSpacingValue(inheritedSpacingElement)
            ?? 0f;
    }

    private static float? ReadSpacingValue(XElement? spacingElement)
    {
        if (spacingElement == null)
            return null;

        var pointValue = spacingElement.Element(A + "spcPts")?.Attribute("val")?.Value;
        if (int.TryParse(pointValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out var hundredthsOfPoint))
            return Math.Max(0f, hundredthsOfPoint / 100f);

        return null;
    }

    private static float? ReadLineSpacing(XElement? lineSpacingElement, XElement? inheritedLineSpacingElement)
    {
        return ReadLineSpacingValue(lineSpacingElement)
            ?? ReadLineSpacingValue(inheritedLineSpacingElement);
    }

    private static float? ReadLineSpacingValue(XElement? lineSpacingElement)
    {
        if (lineSpacingElement == null)
            return null;

        var percentValue = lineSpacingElement.Element(A + "spcPct")?.Attribute("val")?.Value;
        if (int.TryParse(percentValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out var thousandthsOfPercent))
            return Math.Max(0.1f, thousandthsOfPercent / 100000f);

        return null;
    }
    private static int ReadParagraphLevel(XElement paragraphElement)
    {
        var value = paragraphElement.Element(A + "pPr")?.Attribute("lvl")?.Value;
        return int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var level)
            ? Math.Max(0, Math.Min(level, 8))
            : 0;
    }

    private static XElement? ReadInheritedDefaultRunProperties(XElement? inheritedShape, int level)
    {
        if (inheritedShape == null)
            return null;

        var listStyle = inheritedShape.Element(P + "txBody")?.Element(A + "lstStyle");
        var levelProperties = listStyle?.Element(A + $"lvl{level + 1}pPr")
            ?? listStyle?.Element(A + "lvl1pPr");
        return levelProperties?.Element(A + "defRPr");
    }

    private static bool IsBulletParagraph(XElement paragraphElement, XElement? inheritedShape, int level)
    {
        var paragraphProperties = paragraphElement.Element(A + "pPr");
        if (paragraphProperties?.Element(A + "buNone") != null)
            return false;
        if (paragraphProperties?.Element(A + "buChar") != null || paragraphProperties?.Element(A + "buAutoNum") != null)
            return true;

        var inheritedLevelProperties = inheritedShape?
            .Element(P + "txBody")?
            .Element(A + "lstStyle")?
            .Element(A + $"lvl{level + 1}pPr");
        if (inheritedLevelProperties?.Element(A + "buNone") != null)
            return false;
        if (inheritedLevelProperties?.Element(A + "buChar") != null || inheritedLevelProperties?.Element(A + "buAutoNum") != null)
            return true;

        return level > 0 && inheritedLevelProperties != null;
    }

    private static PptxTextRun ReadTextRun(
        string text,
        XElement? runProperties,
        XElement? defaultRunProperties,
        Dictionary<string, PdfColor> themeColors)
    {
        var fontSize = ReadFontSize(runProperties?.Attribute("sz")?.Value)
            ?? ReadFontSize(defaultRunProperties?.Attribute("sz")?.Value)
            ?? 18f;
        var color = ReadSolidFill(runProperties, themeColors)
            ?? ReadSolidFill(defaultRunProperties, themeColors)
            ?? (themeColors.TryGetValue("tx1", out var defaultTextColor) ? defaultTextColor : PdfColor.Black);
        var bold = ReadBool(runProperties?.Attribute("b")?.Value)
            || (runProperties?.Attribute("b") == null && ReadBool(defaultRunProperties?.Attribute("b")?.Value));
        var italic = ReadBool(runProperties?.Attribute("i")?.Value)
            || (runProperties?.Attribute("i") == null && ReadBool(defaultRunProperties?.Attribute("i")?.Value));
        var underlineValue = runProperties?.Attribute("u")?.Value ?? defaultRunProperties?.Attribute("u")?.Value;
        var underline = !string.IsNullOrWhiteSpace(underlineValue)
            && !underlineValue!.Equals("none", StringComparison.OrdinalIgnoreCase);
        if (!underline && runProperties?.Element(A + "hlinkClick") != null)
            underline = true;
        var fontName = ReadFontName(runProperties) ?? ReadFontName(defaultRunProperties);

        return new PptxTextRun(text, fontSize, color, bold, italic, underline, fontName);
    }

    private static string? ReadFontName(XElement? runProperties)
    {
        var latin = runProperties?.Element(A + "latin")?.Attribute("typeface")?.Value;
        if (!string.IsNullOrWhiteSpace(latin) && !latin!.StartsWith("+", StringComparison.Ordinal))
            return latin;

        var eastAsian = runProperties?.Element(A + "ea")?.Attribute("typeface")?.Value;
        if (!string.IsNullOrWhiteSpace(eastAsian) && !eastAsian!.StartsWith("+", StringComparison.Ordinal))
            return eastAsian;

        return null;
    }

    private static float? ReadFontSize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;
        if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var hundredthsOfPoint))
            return Math.Max(1f, hundredthsOfPoint / 100f);
        return null;
    }

    private static PdfColor? ReadSlideBackground(XDocument slideXml, Dictionary<string, PdfColor> themeColors)
    {
        var background = slideXml.Root?
            .Element(P + "cSld")?
            .Element(P + "bg");
        var backgroundProperties = background?.Element(P + "bgPr");
        if (backgroundProperties != null)
            return ReadSolidFill(backgroundProperties, themeColors);

        var backgroundReference = background?.Element(P + "bgRef");
        return ReadColorElement(backgroundReference?.Elements().FirstOrDefault(), themeColors);
    }

    private static PdfColor? ReadShapeFill(XElement? shapeProperties, Dictionary<string, PdfColor> themeColors)
    {
        if (shapeProperties == null)
            return null;
        if (shapeProperties.Element(A + "noFill") != null)
            return null;
        return ReadSolidFill(shapeProperties, themeColors);
    }

    private static PdfColor? ReadStyleFill(XElement? styleElement, Dictionary<string, PdfColor> themeColors)
    {
        var fillReference = styleElement?.Element(A + "fillRef");
        return ReadColorElement(fillReference?.Elements().FirstOrDefault(), themeColors);
    }

    private static PptxOutline? ReadStyleOutline(XElement? styleElement, Dictionary<string, PdfColor> themeColors)
    {
        var lineReference = styleElement?.Element(A + "lnRef");
        var color = ReadColorElement(lineReference?.Elements().FirstOrDefault(), themeColors);
        return color == null ? null : new PptxOutline(color.Value, 1f);
    }

    private static PptxOutline? ReadOutline(XElement? lineElement, Dictionary<string, PdfColor> themeColors)
    {
        if (lineElement == null)
            return null;
        if (lineElement.Element(A + "noFill") != null)
            return null;

        var width = ReadLong(lineElement.Attribute("w")?.Value, 12700) / (float)EmusPerPoint;
        if (width <= 0)
            width = 1f;
        var color = ReadSolidFill(lineElement, themeColors) ?? PdfColor.Black;
        var dashPattern = ReadDashPattern(lineElement.Element(A + "prstDash")?.Attribute("val")?.Value);

        return new PptxOutline(color, width, dashPattern);
    }

    private static float[]? ReadDashPattern(string? value)
    {
        if (string.IsNullOrWhiteSpace(value) || value!.Equals("solid", StringComparison.OrdinalIgnoreCase))
            return null;

        return value.ToLowerInvariant() switch
        {
            "dot" or "sysDot" => new[] { 1f, 2f },
            "dash" or "sysDash" => new[] { 3f, 3f },
            "lgDash" => new[] { 6f, 3f },
            "dashDot" or "sysDashDot" => new[] { 3f, 2f, 1f, 2f },
            _ => null,
        };
    }

    private static PdfColor? ReadSolidFill(XElement? element, Dictionary<string, PdfColor> themeColors)
    {
        if (element == null)
            return null;

        var solidFill = element.Name == A + "solidFill"
            ? element
            : element.Element(A + "solidFill");
        if (solidFill == null)
            return null;

        return ReadColorElement(solidFill.Elements().FirstOrDefault(), themeColors);
    }

    private static PdfColor? ReadColorElement(XElement? colorElement, Dictionary<string, PdfColor> themeColors)
    {
        if (colorElement == null)
            return null;

        PdfColor? color = null;
        if (colorElement.Name == A + "srgbClr")
        {
            var value = colorElement.Attribute("val")?.Value;
            if (!string.IsNullOrWhiteSpace(value))
                color = PdfColor.FromHex(value!);
        }
        else if (colorElement.Name == A + "schemeClr")
        {
            var value = colorElement.Attribute("val")?.Value;
            if (!string.IsNullOrWhiteSpace(value) && themeColors.TryGetValue(value!, out var schemeColor))
                color = schemeColor;
        }
        else if (colorElement.Name == A + "sysClr")
        {
            var value = colorElement.Attribute("lastClr")?.Value ?? colorElement.Attribute("val")?.Value;
            if (!string.IsNullOrWhiteSpace(value))
                color = PdfColor.FromHex(value!);
        }
        else if (colorElement.Name == A + "prstClr")
        {
            color = ReadPresetColor(colorElement.Attribute("val")?.Value);
        }

        if (color == null)
            return null;

        return ApplyLuminosity(color.Value, colorElement);
    }

    private static PdfColor? ReadPresetColor(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        return value!.ToLowerInvariant() switch
        {
            "black" => PdfColor.Black,
            "white" => PdfColor.White,
            "red" => PdfColor.Red,
            "green" => PdfColor.Green,
            "blue" => PdfColor.Blue,
            "yellow" => PdfColor.FromRgb(255, 255, 0),
            _ => null,
        };
    }

    private static PdfColor ApplyLuminosity(PdfColor color, XElement colorElement)
    {
        var lumMod = 1f;
        var lumOff = 0f;
        var hasLuminosityTransform = false;

        var lumModElement = colorElement.Element(A + "lumMod");
        if (lumModElement != null && int.TryParse(lumModElement.Attribute("val")?.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var lumModValue))
        {
            lumMod = lumModValue / 100000f;
            hasLuminosityTransform = true;
        }

        var lumOffElement = colorElement.Element(A + "lumOff");
        if (lumOffElement != null && int.TryParse(lumOffElement.Attribute("val")?.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var lumOffValue))
        {
            lumOff = lumOffValue / 100000f;
            hasLuminosityTransform = true;
        }

        if (!hasLuminosityTransform)
            return color;

        var (hue, saturation, luminance) = RgbToHsl(color);
        luminance = Compat.Clamp(luminance * lumMod + lumOff, 0f, 1f);
        return HslToRgb(hue, saturation, luminance);
    }

    private static (float Hue, float Saturation, float Luminance) RgbToHsl(PdfColor color)
    {
        var max = Math.Max(color.R, Math.Max(color.G, color.B));
        var min = Math.Min(color.R, Math.Min(color.G, color.B));
        var luminance = (max + min) / 2f;

        if (Math.Abs(max - min) < 0.00001f)
            return (0f, 0f, luminance);

        var delta = max - min;
        var saturation = luminance > 0.5f
            ? delta / (2f - max - min)
            : delta / (max + min);
        float hue;
        if (Math.Abs(max - color.R) < 0.00001f)
            hue = (color.G - color.B) / delta + (color.G < color.B ? 6f : 0f);
        else if (Math.Abs(max - color.G) < 0.00001f)
            hue = (color.B - color.R) / delta + 2f;
        else
            hue = (color.R - color.G) / delta + 4f;

        return (hue / 6f, saturation, luminance);
    }

    private static PdfColor HslToRgb(float hue, float saturation, float luminance)
    {
        if (saturation <= 0.00001f)
            return new PdfColor(luminance, luminance, luminance);

        var upperBound = luminance < 0.5f
            ? luminance * (1f + saturation)
            : luminance + saturation - luminance * saturation;
        var lowerBound = 2f * luminance - upperBound;

        return new PdfColor(
            HueToRgb(lowerBound, upperBound, hue + 1f / 3f),
            HueToRgb(lowerBound, upperBound, hue),
            HueToRgb(lowerBound, upperBound, hue - 1f / 3f));
    }

    private static float HueToRgb(float lowerBound, float upperBound, float hueOffset)
    {
        if (hueOffset < 0f)
            hueOffset += 1f;
        if (hueOffset > 1f)
            hueOffset -= 1f;
        if (hueOffset < 1f / 6f)
            return lowerBound + (upperBound - lowerBound) * 6f * hueOffset;
        if (hueOffset < 1f / 2f)
            return upperBound;
        if (hueOffset < 2f / 3f)
            return lowerBound + (upperBound - lowerBound) * (2f / 3f - hueOffset) * 6f;
        return lowerBound;
    }

    private static PptxRelatedPart? ReadRelatedPart(
        ZipArchive archive,
        string sourcePartPath,
        Dictionary<string, PptxRelationship> relationships,
        string relationshipTypeSuffix)
    {
        var relationship = relationships.Values.FirstOrDefault(candidate =>
            candidate.Type.EndsWith(relationshipTypeSuffix, StringComparison.OrdinalIgnoreCase));
        if (relationship == null || relationship.TargetMode.Equals("External", StringComparison.OrdinalIgnoreCase))
            return null;

        var partPath = ResolveRelationshipTarget(sourcePartPath, relationship.Target);
        var entry = archive.GetEntry(partPath);
        if (entry == null)
            return null;

        var xml = LoadXml(entry);
        var partRelationships = ReadRelationships(archive, partPath);
        return new PptxRelatedPart(partPath, xml, partRelationships);
    }

    private static Dictionary<PptxPlaceholderKey, XElement> BuildPlaceholderDefaults(XDocument? layoutXml, XDocument? masterXml)
    {
        var placeholders = new Dictionary<PptxPlaceholderKey, XElement>();
        AddPlaceholderDefaults(placeholders, masterXml);
        AddPlaceholderDefaults(placeholders, layoutXml);
        return placeholders;
    }

    private static void AddPlaceholderDefaults(Dictionary<PptxPlaceholderKey, XElement> placeholders, XDocument? document)
    {
        var shapeTree = document?.Root?
            .Element(P + "cSld")?
            .Element(P + "spTree");
        if (shapeTree == null)
            return;

        foreach (var shape in shapeTree.Descendants(P + "sp"))
        {
            var key = ReadPlaceholderKey(shape);
            if (key == null)
                continue;

            placeholders[key.Value] = shape;
            var typeKey = new PptxPlaceholderKey(key.Value.Type, null);
            if (!placeholders.ContainsKey(typeKey))
                placeholders[typeKey] = shape;
        }
    }

    private static XElement? FindPlaceholderDefault(XElement shapeElement, Dictionary<PptxPlaceholderKey, XElement>? placeholderDefaults)
    {
        if (placeholderDefaults == null || placeholderDefaults.Count == 0)
            return null;

        var key = ReadPlaceholderKey(shapeElement);
        if (key == null)
            return null;

        if (placeholderDefaults.TryGetValue(key.Value, out var exactMatch))
            return exactMatch;

        if (!string.IsNullOrWhiteSpace(key.Value.Index))
        {
            var typeOnlyKey = new PptxPlaceholderKey(key.Value.Type, null);
            if (placeholderDefaults.TryGetValue(typeOnlyKey, out var typeMatch))
                return typeMatch;
        }

        return null;
    }

    private static bool IsPlaceholderElement(XElement element)
    {
        return ReadPlaceholderKey(element) != null;
    }

    private static PptxPlaceholderKey? ReadPlaceholderKey(XElement element)
    {
        var placeholder = element
            .Element(P + "nvSpPr")?
            .Element(P + "nvPr")?
            .Element(P + "ph")
            ?? element
                .Element(P + "nvPicPr")?
                .Element(P + "nvPr")?
                .Element(P + "ph");
        if (placeholder == null)
            return null;

        var type = placeholder.Attribute("type")?.Value;
        if (string.IsNullOrWhiteSpace(type))
            type = "body";

        var index = placeholder.Attribute("idx")?.Value;
        return new PptxPlaceholderKey(type!, string.IsNullOrWhiteSpace(index) ? null : index);
    }

    private static Dictionary<string, PdfColor> ReadThemeColors(
        ZipArchive archive,
        Dictionary<string, PptxRelationship> presentationRelationships)
    {
        var colors = CreateDefaultThemeColors();
        var themePath = presentationRelationships.Values
            .Where(relationship => relationship.Type.EndsWith("/theme", StringComparison.OrdinalIgnoreCase))
            .Select(relationship => ResolveRelationshipTarget("ppt/presentation.xml", relationship.Target))
            .FirstOrDefault() ?? "ppt/theme/theme1.xml";

        var themeEntry = archive.GetEntry(themePath);
        if (themeEntry == null)
            return colors;

        try
        {
            var themeXml = LoadXml(themeEntry);
            var colorScheme = themeXml.Descendants(A + "clrScheme").FirstOrDefault();
            if (colorScheme == null)
                return colors;

            foreach (var colorSlot in colorScheme.Elements())
            {
                var slotName = colorSlot.Name.LocalName;
                var color = ReadColorElement(colorSlot.Elements().FirstOrDefault(), colors);
                if (color != null)
                    colors[slotName] = color.Value;
            }

            AddThemeAliases(colors);
        }
        catch (Exception) when (IsXmlReadException())
        {
            return colors;
        }

        return colors;
    }

    private static Dictionary<string, PdfColor> CreateDefaultThemeColors()
    {
        var colors = new Dictionary<string, PdfColor>(StringComparer.OrdinalIgnoreCase)
        {
            ["lt1"] = PdfColor.White,
            ["dk1"] = PdfColor.Black,
            ["lt2"] = PdfColor.FromHex("EEECE1"),
            ["dk2"] = PdfColor.FromHex("1F497D"),
            ["accent1"] = PdfColor.FromHex("4F81BD"),
            ["accent2"] = PdfColor.FromHex("C0504D"),
            ["accent3"] = PdfColor.FromHex("9BBB59"),
            ["accent4"] = PdfColor.FromHex("8064A2"),
            ["accent5"] = PdfColor.FromHex("4BACC6"),
            ["accent6"] = PdfColor.FromHex("F79646"),
            ["hlink"] = PdfColor.FromHex("0000FF"),
            ["folHlink"] = PdfColor.FromHex("800080"),
        };
        AddThemeAliases(colors);
        return colors;
    }

    private static void AddThemeAliases(Dictionary<string, PdfColor> colors)
    {
        if (colors.TryGetValue("lt1", out var background1))
            colors["bg1"] = background1;
        if (colors.TryGetValue("dk1", out var text1))
            colors["tx1"] = text1;
        if (colors.TryGetValue("lt2", out var background2))
            colors["bg2"] = background2;
        if (colors.TryGetValue("dk2", out var text2))
            colors["tx2"] = text2;
    }

    private static Dictionary<string, PdfColor> ApplyColorMap(
        Dictionary<string, PdfColor> themeColors,
        XDocument slideXml,
        XDocument? layoutXml,
        XDocument? masterXml)
    {
        var mappedColors = new Dictionary<string, PdfColor>(themeColors, StringComparer.OrdinalIgnoreCase);
        var colorMap = ReadOverrideColorMap(slideXml)
            ?? (layoutXml == null ? null : ReadOverrideColorMap(layoutXml))
            ?? masterXml?.Root?.Element(P + "clrMap");
        if (colorMap == null)
            return mappedColors;

        foreach (var alias in new[] { "bg1", "tx1", "bg2", "tx2", "accent1", "accent2", "accent3", "accent4", "accent5", "accent6", "hlink", "folHlink" })
        {
            var source = colorMap.Attribute(alias)?.Value;
            if (!string.IsNullOrWhiteSpace(source) && themeColors.TryGetValue(source!, out var color))
                mappedColors[alias] = color;
        }

        return mappedColors;
    }

    private static XElement? ReadOverrideColorMap(XDocument document)
    {
        var colorMapOverride = document.Root?.Element(P + "clrMapOvr");
        return colorMapOverride?.Element(A + "overrideClrMapping");
    }

    private static Dictionary<string, PptxRelationship> ReadRelationships(ZipArchive archive, string partPath)
    {
        var relationshipPartPath = GetRelationshipPartPath(partPath);
        var entry = archive.GetEntry(relationshipPartPath);
        var relationships = new Dictionary<string, PptxRelationship>(StringComparer.OrdinalIgnoreCase);
        if (entry == null)
            return relationships;

        var document = LoadXml(entry);
        foreach (var relationshipElement in document.Root?.Elements(Rel + "Relationship") ?? Enumerable.Empty<XElement>())
        {
            var id = relationshipElement.Attribute("Id")?.Value;
            var target = relationshipElement.Attribute("Target")?.Value;
            if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(target))
                continue;

            relationships[id!] = new PptxRelationship(
                relationshipElement.Attribute("Type")?.Value ?? string.Empty,
                target!,
                relationshipElement.Attribute("TargetMode")?.Value ?? string.Empty,
                relationshipPartPath);
        }

        return relationships;
    }

    private static string GetRelationshipPartPath(string partPath)
    {
        var slashIndex = partPath.LastIndexOf('/');
        var directory = slashIndex >= 0 ? partPath.Substring(0, slashIndex + 1) : string.Empty;
        var fileName = slashIndex >= 0 ? partPath.Substring(slashIndex + 1) : partPath;
        return directory + "_rels/" + fileName + ".rels";
    }

    private static string GetSourcePartFromRelationshipPath(string relationshipPartPath)
    {
        var marker = "/_rels/";
        var markerIndex = relationshipPartPath.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
        if (markerIndex < 0)
            return relationshipPartPath;

        var directory = relationshipPartPath.Substring(0, markerIndex + 1);
        var fileName = relationshipPartPath.Substring(markerIndex + marker.Length);
        if (fileName.EndsWith(".rels", StringComparison.OrdinalIgnoreCase))
            fileName = fileName.Substring(0, fileName.Length - ".rels".Length);
        return directory + fileName;
    }

    private static string ResolveRelationshipTarget(string sourcePartPath, string target)
    {
        if (target.StartsWith("/", StringComparison.Ordinal))
            return target.TrimStart('/');

        var slashIndex = sourcePartPath.LastIndexOf('/');
        var baseDirectory = slashIndex >= 0 ? sourcePartPath.Substring(0, slashIndex + 1) : string.Empty;
        var combined = baseDirectory + target;
        var normalized = new List<string>();
        foreach (var segment in combined.Split('/'))
        {
            if (segment.Length == 0 || segment == ".")
                continue;
            if (segment == "..")
            {
                if (normalized.Count > 0)
                    normalized.RemoveAt(normalized.Count - 1);
                continue;
            }
            normalized.Add(segment);
        }

        return string.Join("/", normalized);
    }

    private static string? DetectImageFormat(string mediaPath, byte[] data)
    {
        var extension = Path.GetExtension(mediaPath).ToLowerInvariant();
        if (extension == ".jpg" || extension == ".jpeg")
            return "jpg";
        if (extension == ".png")
            return "png";
        if (extension == ".svg")
            return "svg";

        if (data.Length >= 8
            && data[0] == 0x89 && data[1] == 0x50 && data[2] == 0x4E && data[3] == 0x47)
            return "png";
        if (data.Length >= 3 && data[0] == 0xFF && data[1] == 0xD8 && data[2] == 0xFF)
            return "jpg";
        if (data.Length >= 4 && data[0] == '<' && data[1] == 's' && data[2] == 'v' && data[3] == 'g')
            return "svg";

        return null;
    }

    private static XDocument LoadXml(ZipArchiveEntry entry)
    {
        using var stream = entry.Open();
        return XDocument.Load(stream, LoadOptions.None);
    }

    private static byte[] ReadEntryBytes(ZipArchiveEntry entry)
    {
        using var stream = entry.Open();
        using var memoryStream = new MemoryStream();
        stream.CopyTo(memoryStream);
        return memoryStream.ToArray();
    }

    private static long ReadLong(string? value, long fallback)
    {
        return long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result)
            ? result
            : fallback;
    }

    private static bool ReadBool(string? value)
    {
        return value == "1" || value?.Equals("true", StringComparison.OrdinalIgnoreCase) == true;
    }

    private static float EmuToPoint(double emu)
    {
        return (float)(emu / EmusPerPoint);
    }

    private static bool IsXmlReadException() => true;

    private sealed class PptxRelationship
    {
        public string Type { get; }
        public string Target { get; }
        public string TargetMode { get; }
        public string RelationshipPartPath { get; }

        public PptxRelationship(string type, string target, string targetMode, string relationshipPartPath)
        {
            Type = type;
            Target = target;
            TargetMode = targetMode;
            RelationshipPartPath = relationshipPartPath;
        }
    }

    private sealed class PptxRelatedPart
    {
        public string PartPath { get; }
        public XDocument Xml { get; }
        public Dictionary<string, PptxRelationship> Relationships { get; }

        public PptxRelatedPart(string partPath, XDocument xml, Dictionary<string, PptxRelationship> relationships)
        {
            PartPath = partPath;
            Xml = xml;
            Relationships = relationships;
        }
    }

    private readonly struct PptxPlaceholderKey : IEquatable<PptxPlaceholderKey>
    {
        public string Type { get; }
        public string? Index { get; }

        public PptxPlaceholderKey(string type, string? index)
        {
            Type = type;
            Index = index;
        }

        public bool Equals(PptxPlaceholderKey other)
        {
            return string.Equals(Type, other.Type, StringComparison.OrdinalIgnoreCase)
                && string.Equals(Index, other.Index, StringComparison.OrdinalIgnoreCase);
        }

        public override bool Equals(object? obj)
        {
            return obj is PptxPlaceholderKey other && Equals(other);
        }

        public override int GetHashCode()
        {
            var typeHash = StringComparer.OrdinalIgnoreCase.GetHashCode(Type);
            var indexHash = Index == null ? 0 : StringComparer.OrdinalIgnoreCase.GetHashCode(Index);
            return (typeHash * 397) ^ indexHash;
        }
    }

    private sealed class CoordinateMap
    {
        private readonly double _x;
        private readonly double _y;
        private readonly double _width;
        private readonly double _height;
        private readonly double _childX;
        private readonly double _childY;
        private readonly double _childWidth;
        private readonly double _childHeight;

        public CoordinateMap(double x, double y, double width, double height, double childX, double childY, double childWidth, double childHeight)
        {
            _x = x;
            _y = y;
            _width = width;
            _height = height;
            _childX = childX;
            _childY = childY;
            _childWidth = Math.Abs(childWidth) < 0.001 ? 1 : childWidth;
            _childHeight = Math.Abs(childHeight) < 0.001 ? 1 : childHeight;
        }

        public double MapX(double x) => _x + (x - _childX) * _width / _childWidth;

        public double MapY(double y) => _y + (y - _childY) * _height / _childHeight;

        public PptxRect MapRect(double x, double y, double width, double height)
        {
            var left = MapX(x);
            var top = MapY(y);
            var right = MapX(x + width);
            var bottom = MapY(y + height);
            return new PptxRect(
                EmuToPoint(left),
                EmuToPoint(top),
                EmuToPoint(Math.Max(0, right - left)),
                EmuToPoint(Math.Max(0, bottom - top)));
        }
    }
}