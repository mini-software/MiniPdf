namespace MiniSoftware;

/// <summary>
/// Represents a text block to be rendered on a PDF page.
/// </summary>
internal sealed class PdfTextBlock
{
    /// <summary>
    /// The text content.
    /// </summary>
    public string Text { get; }

    /// <summary>
    /// X position in points from the left edge.
    /// </summary>
    public float X { get; }

    /// <summary>
    /// Y position in points from the bottom edge.
    /// </summary>
    public float Y { get; }

    /// <summary>
    /// Font size in points.
    /// </summary>
    public float FontSize { get; }

    /// <summary>
    /// Text color (default: black).
    /// </summary>
    public PdfColor Color { get; }

    /// <summary>
    /// Optional clipping rectangle (X, Y, Width, Height) in PDF points.
    /// When set, text is rendered inside a clipping path so it doesn't
    /// visually overflow the cell, but the full text remains in the PDF
    /// for text extraction.
    /// </summary>
    public (float X, float Y, float Width, float Height)? ClipRect { get; }

    /// <summary>
    /// Optional maximum rendering width in points. When the natural Helvetica
    /// width of the text exceeds this value, horizontal scaling (Tz) is applied
    /// so the text fits within the specified width. This keeps all characters
    /// intact for text extraction while preventing visual overflow into
    /// adjacent columns.
    /// </summary>
    public float? MaxWidth { get; }

    /// <summary>
    /// Whether to render text using the bold font variant.
    /// </summary>
    public bool Bold { get; }

    /// <summary>
    /// Whether to render an underline below the text.
    /// </summary>
    public bool Underline { get; }

    /// <summary>
    /// Character spacing in points (PDF Tc operator). 0 means default.
    /// </summary>
    public float CharSpacing { get; }

    /// <summary>
    /// Word spacing in points (PDF Tw operator). 0 means default.
    /// Used for justified text to distribute extra space between words.
    /// </summary>
    public float WordSpacing { get; }

    internal PdfTextBlock(string text, float x, float y, float fontSize, PdfColor? color = null, (float, float, float, float)? clipRect = null, float? maxWidth = null, bool bold = false, bool underline = false, float charSpacing = 0, float wordSpacing = 0)
    {
        Text = text;
        X = x;
        Y = y;
        FontSize = fontSize;
        Color = color ?? PdfColor.Black;
        ClipRect = clipRect;
        MaxWidth = maxWidth;
        Bold = bold;
        Underline = underline;
        CharSpacing = charSpacing;
        WordSpacing = wordSpacing;
    }
}
