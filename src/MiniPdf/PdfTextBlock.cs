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

    internal PdfTextBlock(string text, float x, float y, float fontSize, PdfColor? color = null, (float, float, float, float)? clipRect = null)
    {
        Text = text;
        X = x;
        Y = y;
        FontSize = fontSize;
        Color = color ?? PdfColor.Black;
        ClipRect = clipRect;
    }
}
