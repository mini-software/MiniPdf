namespace MiniSoftware;

/// <summary>
/// Represents an image to be rendered on a PDF page.
/// </summary>
internal sealed record PdfImageBlock(
    byte[] Data,         // raw image bytes (JPEG or PNG)
    string Format,       // "jpg" or "png"
    float X,             // left edge in points (PDF origin = bottom-left)
    float Y,             // bottom edge in points
    float RenderWidth,   // rendered width in points
    float RenderHeight   // rendered height in points
);

/// <summary>
/// Represents a filled rectangle on a PDF page.
/// </summary>
internal sealed record PdfRectBlock(
    float X,             // left edge in points
    float Y,             // bottom edge in points
    float Width,         // width in points
    float Height,        // height in points
    PdfColor FillColor   // fill color
);

/// <summary>
/// Represents a filled ellipse on a PDF page.
/// </summary>
internal sealed record PdfEllipseBlock(
    float X,
    float Y,
    float Width,
    float Height,
    PdfColor FillColor
);

/// <summary>
/// Represents a point in a polygon path.
/// </summary>
internal sealed record PdfPoint(
    float X,
    float Y
);

/// <summary>
/// Represents a filled polygon on a PDF page.
/// </summary>
internal sealed record PdfPolygonBlock(
    List<PdfPoint> Points,
    PdfColor FillColor,
    List<List<PdfPoint>>? Subpaths = null,
    bool EvenOddFill = false
);

/// <summary>
/// Represents a line segment on a PDF page.
/// </summary>
internal sealed record PdfLineBlock(
    float X1,            // start X in points
    float Y1,            // start Y in points
    float X2,            // end X in points
    float Y2,            // end Y in points
    PdfColor Color,      // stroke color
    float LineWidth = 1f, // stroke width in points
    float[]? DashPattern = null // PDF dash array (e.g. [1,1] for dotted, [3,3] for dashed)
);

/// <summary>
/// Represents a single page in a PDF document.
/// </summary>
internal sealed class PdfPage
{
    private readonly List<PdfTextBlock> _textBlocks = [];
    private readonly List<PdfImageBlock> _imageBlocks = [];
    private readonly List<PdfRectBlock> _rectBlocks = [];
    private readonly List<PdfEllipseBlock> _ellipseBlocks = [];
    private readonly List<PdfPolygonBlock> _polygonBlocks = [];
    private readonly List<PdfLineBlock> _lineBlocks = [];

    /// <summary>
    /// Page width in points.
    /// </summary>
    public float Width { get; }

    /// <summary>
    /// Page height in points.
    /// </summary>
    public float Height { get; }

    /// <summary>
    /// Gets the text blocks on this page.
    /// </summary>
    public IReadOnlyList<PdfTextBlock> TextBlocks => _textBlocks;

    /// <summary>
    /// Gets the image blocks on this page.
    /// </summary>
    public IReadOnlyList<PdfImageBlock> ImageBlocks => _imageBlocks;

    /// <summary>
    /// Gets the rectangle blocks on this page.
    /// </summary>
    public IReadOnlyList<PdfRectBlock> RectBlocks => _rectBlocks;

    /// <summary>
    /// Gets the ellipse blocks on this page.
    /// </summary>
    public IReadOnlyList<PdfEllipseBlock> EllipseBlocks => _ellipseBlocks;

    /// <summary>
    /// Gets the polygon blocks on this page.
    /// </summary>
    public IReadOnlyList<PdfPolygonBlock> PolygonBlocks => _polygonBlocks;

    /// <summary>
    /// Gets the line blocks on this page.
    /// </summary>
    public IReadOnlyList<PdfLineBlock> LineBlocks => _lineBlocks;

    internal PdfPage(float width, float height)
    {
        Width = width;
        Height = height;
    }

    /// <summary>
    /// Adds a text block at the specified position.
    /// </summary>
    /// <param name="text">The text to render.</param>
    /// <param name="x">X position in points from the left edge.</param>
    /// <param name="y">Y position in points from the bottom edge.</param>
    /// <param name="fontSize">Font size in points (default: 12).</param>
    /// <param name="color">Text color (default: black).</param>
    /// <param name="clipRect">Optional clipping rectangle for visual overflow control.</param>
    /// <param name="maxWidth">Optional max render width; applies horizontal scaling when needed.</param>
    /// <param name="bold">Whether to use the bold variant for WinAnsi text.</param>
    /// <param name="underline">Whether to draw an underline under the text.</param>
    /// <param name="charSpacing">Character spacing in points (PDF Tc).</param>
    /// <param name="wordSpacing">Word spacing in points (PDF Tw).</param>
    /// <param name="preferredFontName">Optional preferred Unicode font family hint.</param>
    /// <param name="underlineWidth">Optional explicit underline width override in points.</param>
    /// <returns>The current page for chaining.</returns>
    public PdfPage AddText(string text, float x, float y, float fontSize = 12, PdfColor? color = null, (float, float, float, float)? clipRect = null, float? maxWidth = null, bool bold = false, bool underline = false, float charSpacing = 0, float wordSpacing = 0, string? preferredFontName = null, float? underlineWidth = null)
    {
        _textBlocks.Add(new PdfTextBlock(text, x, y, fontSize, color, clipRect, maxWidth, bold, underline, charSpacing, wordSpacing, preferredFontName, underlineWidth));
        return this;
    }

    /// <summary>
    /// Adds an image at the specified position.
    /// </summary>
    /// <param name="data">Raw image bytes (JPEG or PNG).</param>
    /// <param name="format">Image format string: "jpg" or "png".</param>
    /// <param name="x">X position in points from the left edge.</param>
    /// <param name="y">Y position of the bottom edge in points from the bottom of the page.</param>
    /// <param name="width">Rendered width in points.</param>
    /// <param name="height">Rendered height in points.</param>
    /// <returns>The current page for chaining.</returns>
    public PdfPage AddImage(byte[] data, string format, float x, float y, float width, float height)
    {
        _imageBlocks.Add(new PdfImageBlock(data, format, x, y, width, height));
        return this;
    }

    /// <summary>
    /// Adds a filled rectangle at the specified position.
    /// </summary>
    public PdfPage AddRectangle(float x, float y, float width, float height, PdfColor? fillColor = null)
    {
        _rectBlocks.Add(new PdfRectBlock(x, y, width, height, fillColor ?? new PdfColor(0.92f, 0.92f, 0.92f)));
        return this;
    }

    /// <summary>
    /// Adds a filled ellipse at the specified position.
    /// </summary>
    public PdfPage AddEllipse(float x, float y, float width, float height, PdfColor? fillColor = null)
    {
        _ellipseBlocks.Add(new PdfEllipseBlock(x, y, width, height, fillColor ?? new PdfColor(0.92f, 0.92f, 0.92f)));
        return this;
    }

    /// <summary>
    /// Adds a filled polygon from the provided points.
    /// </summary>
    public PdfPage AddPolygon(List<PdfPoint> points, PdfColor? fillColor = null)
    {
        if (points.Count >= 3)
            _polygonBlocks.Add(new PdfPolygonBlock(points, fillColor ?? new PdfColor(0.92f, 0.92f, 0.92f)));
        return this;
    }

    /// <summary>
    /// Adds a filled compound polygon composed of multiple closed subpaths.
    /// </summary>
    public PdfPage AddCompoundPolygon(List<List<PdfPoint>> subpaths, PdfColor? fillColor = null, bool evenOddFill = true)
    {
        if (subpaths.Count == 0)
            return this;

        var valid = subpaths.Where(p => p.Count >= 3).ToList();
        if (valid.Count == 0)
            return this;

        _polygonBlocks.Add(new PdfPolygonBlock(valid[0],
            fillColor ?? new PdfColor(0.92f, 0.92f, 0.92f), valid, evenOddFill));
        return this;
    }

    /// <summary>
    /// Adds a line segment at the specified coordinates.
    /// </summary>
    public PdfPage AddLine(float x1, float y1, float x2, float y2, PdfColor? color = null, float lineWidth = 1f, float[]? dashPattern = null)
    {
        _lineBlocks.Add(new PdfLineBlock(x1, y1, x2, y2, color ?? new PdfColor(0, 0, 0), lineWidth, dashPattern));
        return this;
    }

    /// <summary>
    /// Adds text that automatically wraps within the specified region.
    /// Text flows from top to bottom, left to right within the given bounds.
    /// </summary>
    /// <param name="text">The text to render.</param>
    /// <param name="x">X position of the left edge.</param>
    /// <param name="y">Y position of the top edge.</param>
    /// <param name="maxWidth">Maximum width for text wrapping.</param>
    /// <param name="fontSize">Font size in points (default: 12).</param>
    /// <param name="lineSpacing">Line spacing multiplier (default: 1.2).</param>
    /// <param name="color">Text color (default: black).</param>
    /// <returns>The current page for chaining.</returns>
    public PdfPage AddTextWrapped(string text, float x, float y, float maxWidth, float fontSize = 12, float lineSpacing = 1.2f, PdfColor? color = null)
    {
        if (string.IsNullOrEmpty(text))
            return this;

        var lineHeight = fontSize * lineSpacing;
        // Approximate character width for Helvetica at given font size
        var avgCharWidth = fontSize * 0.5f;
        var charsPerLine = (int)(maxWidth / avgCharWidth);
        if (charsPerLine < 1) charsPerLine = 1;

        var lines = WrapText(text, charsPerLine);
        var currentY = y;

        foreach (var line in lines)
        {
            // PDF y-coordinate is from bottom, so subtract to go down
            AddText(line, x, currentY, fontSize, color);
            currentY -= lineHeight;
        }

        return this;
    }

    private static List<string> WrapText(string text, int maxCharsPerLine)
    {
        var result = new List<string>();
        var paragraphs = text.Split('\n');

        foreach (var paragraph in paragraphs)
        {
            if (string.IsNullOrEmpty(paragraph))
            {
                result.Add(string.Empty);
                continue;
            }

            var words = paragraph.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var currentLine = "";

            foreach (var word in words)
            {
                if (currentLine.Length == 0)
                {
                    currentLine = word;
                }
                else if (currentLine.Length + 1 + word.Length <= maxCharsPerLine)
                {
                    currentLine += " " + word;
                }
                else
                {
                    result.Add(currentLine);
                    currentLine = word;
                }
            }

            if (currentLine.Length > 0)
                result.Add(currentLine);
        }

        return result;
    }
}
