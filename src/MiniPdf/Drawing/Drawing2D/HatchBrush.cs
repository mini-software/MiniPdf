using MiniPdf.Drawing.Brushes;
using MiniPdf.Drawing.Colors;

namespace MiniPdf.Drawing.Drawing2D
{
    /// <summary>
    /// Defines a rectangular, horizontal, vertical, or crosshatch brush.
    /// </summary>
    /// <remarks>
    /// A <see cref="HatchBrush"/> fills shapes with a hatching pattern defined by
    /// <see cref="HatchStyle"/>, using a foreground and background color.
    /// </remarks>
    public sealed class HatchBrush : Brush
    {
        /// <summary>
        /// Initializes a new <see cref="HatchBrush"/> with the specified hatch style and foreground color.
        /// The background color defaults to black.
        /// </summary>
        /// <param name="hatchStyle">The hatch style enumeration specifying the pattern.</param>
        /// <param name="foreColor">The foreground color.</param>
        public HatchBrush(HatchStyle hatchStyle, Color foreColor)
            : this(hatchStyle, foreColor, Color.Black) { }

        /// <summary>
        /// Initializes a new <see cref="HatchBrush"/> with the specified hatch style, foreground color, and background color.
        /// </summary>
        /// <param name="hatchStyle">The hatch style enumeration specifying the pattern.</param>
        /// <param name="foreColor">The foreground color.</param>
        /// <param name="backColor">The background color.</param>
        public HatchBrush(HatchStyle hatchStyle, Color foreColor, Color backColor)
        {
            HatchStyle       = hatchStyle;
            ForegroundColor  = foreColor;
            BackgroundColor  = backColor;
        }

        /// <summary>
        /// Gets the hatch style of this brush.
        /// </summary>
        public HatchStyle HatchStyle      { get; }

        /// <summary>
        /// Gets the foreground color of this brush.
        /// </summary>
        public Color      ForegroundColor { get; }

        /// <summary>
        /// Gets the background color of this brush.
        /// </summary>
        public Color      BackgroundColor { get; }

        /// <summary>
        /// Creates an exact copy of this <see cref="HatchBrush"/>.
        /// </summary>
        /// <returns>A new <see cref="HatchBrush"/> with the same properties.</returns>
        public override Brush Clone()
            => new HatchBrush(HatchStyle, ForegroundColor, BackgroundColor);
    }
}
