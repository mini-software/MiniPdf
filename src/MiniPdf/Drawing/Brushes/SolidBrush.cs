using MiniSoftware.Drawing.Colors;

namespace MiniSoftware.Drawing.Brushes
{
    /// <summary>
    /// Defines a brush of a single color.
    /// </summary>
    /// <remarks>
    /// A <see cref="SolidBrush"/> is the simplest brush that fills shapes with a solid color.
    /// It is commonly used for drawing filled rectangles, ellipses, and other shapes.
    /// </remarks>
    public sealed class SolidBrush : Brush
    {
        private Color _color;

        /// <summary>
        /// Initializes a new <see cref="SolidBrush"/> object of the specified color.
        /// </summary>
        /// <param name="color">The color of the brush.</param>
        public SolidBrush(Color color)
        {
            _color = color;
        }

        /// <summary>
        /// Gets or sets the color of this brush.
        /// </summary>
        public Color Color
        {
            get
            {
                CheckDisposed();
                return _color;
            }
            set
            {
                CheckDisposed();
                _color = value;
            }
        }

        /// <summary>
        /// Creates an exact copy of this <see cref="SolidBrush"/>.
        /// </summary>
        /// <returns>A new <see cref="SolidBrush"/> with the same color.</returns>
        public override Brush Clone() => new SolidBrush(_color);
    }
}
