using MiniPdf.Drawing.Brushes;
using MiniPdf.Drawing.Geometry;
using MiniPdf.Drawing.Text;

namespace MiniPdf.Drawing.Vector.Commands
{
    /// <summary>
    /// Draws a string with the specified font, brush, layout rectangle, and format.
    /// </summary>
    public sealed class DrawStringCommand : DrawingCommand
    {
        /// <summary>
        /// Gets the text to draw.
        /// </summary>
        public string Text { get; }

        /// <summary>
        /// Gets the font.
        /// </summary>
        public Text.Font Font { get; }

        /// <summary>
        /// Gets the brush.
        /// </summary>
        public Brush Brush { get; }

        /// <summary>
        /// Gets the layout rectangle.
        /// </summary>
        public RectangleF LayoutRect { get; }

        /// <summary>
        /// Gets the string format, or null.
        /// </summary>
        public StringFormat? Format { get; }

        /// <summary>
        /// Initializes a new <see cref="DrawStringCommand"/>, cloning the font, brush, and format.
        /// </summary>
        public DrawStringCommand(string text, Text.Font font, Brush brush,
                                 RectangleF layoutRect, StringFormat? format)
        {
            Text      = text;
            Font      = font.Clone();
            Brush     = brush.Clone();
            LayoutRect = layoutRect;
            Format    = format?.Clone();
        }

        /// <inheritdoc/>
        internal override void Replay(IDrawingContext ctx)
            => ctx.DrawString(Text, Font, Brush, LayoutRect, Format);

        /// <inheritdoc/>
        public override DrawingCommand Clone()
            => new DrawStringCommand(Text, Font, Brush, LayoutRect, Format);
    }
}