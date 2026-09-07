namespace MiniPdf.Drawing.Text
{
    /// <summary>
    /// Options controlling text placement along a baseline path for
    /// <see cref="Graphics.DrawStringOnPath(string, Text.Font, MiniPdf.Drawing.Brushes.Brush, MiniPdf.Drawing.Drawing2D.GraphicsPath, StringFormat, TextOnPathOptions)"/>
    /// and
    /// <see cref="MiniPdf.Drawing.Drawing2D.GraphicsPath.AddStringOnPath(string, Text.FontFamily, int, float, MiniPdf.Drawing.Drawing2D.GraphicsPath, StringFormat, TextOnPathOptions)"/>.
    /// </summary>
    public sealed class TextOnPathOptions
    {
        /// <summary>
        /// Distance along the path (in page units for
        /// <c>DrawStringOnPath</c>, pixels for <c>AddStringOnPath</c>) at
        /// which to start placing the first glyph's pen position. Default 0.
        /// </summary>
        public float StartOffset { get; set; }

        /// <summary>
        /// Perpendicular offset of each glyph's baseline from the path.
        /// Positive values place glyphs "above" the path (toward smaller Y
        /// for a rightward-traveling path). In page units for
        /// <c>DrawStringOnPath</c>, pixels for <c>AddStringOnPath</c>.
        /// Default 0 (glyphs sit on the path).
        /// </summary>
        public float PerpendicularOffset { get; set; }

        /// <summary>
        /// Controls alignment of the text block along the path, overriding
        /// <see cref="StringFormat.Alignment"/>. When non-null, the text is
        /// aligned within the usable path length (total length minus
        /// <see cref="StartOffset"/>). Default null (use StringFormat.Alignment).
        /// </summary>
        public StringAlignment? Alignment { get; set; }

        /// <summary>
        /// Behavior when the text is longer than the available path length.
        /// Default <see cref="TextOnPathOverflow.Stop"/> (place glyphs until
        /// the path ends, then stop).
        /// </summary>
        public TextOnPathOverflow Overflow { get; set; } = TextOnPathOverflow.Stop;
    }
}