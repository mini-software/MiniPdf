using MiniPdf.Drawing.Brushes;

namespace MiniPdf.Drawing.Text
{
    /// <summary>
    /// A positioned span of text within a <see cref="TextLine"/>.
    /// </summary>
    internal sealed class TextRun
    {
        internal TextRun(string text, Font font, float x, float baseline, float width)
            : this(text, font, null, 0, false, x, baseline, width)
        {
        }

        internal TextRun(
            string    text,
            Font      font,
            Brush?    brush,
            int       bidiLevel,
            bool      isRTL,
            float     x,
            float     baseline,
            float     width)
        {
            Text       = text;
            Font       = font;
            Brush      = brush;
            BidiLevel  = bidiLevel;
            IsRTL      = isRTL;
            X          = x;
            Baseline   = baseline;
            Width      = width;
        }

        public string  Text       { get; }
        public Font    Font       { get; }
        public Brush?  Brush      { get; }
        public int     BidiLevel  { get; }
        public bool    IsRTL      { get; }
        public float   X          { get; set; }
        public float   Baseline   { get; set; }
        public float   Width      { get; set; }
    }
}