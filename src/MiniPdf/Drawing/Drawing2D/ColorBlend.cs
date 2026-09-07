using MiniPdf.Drawing.Colors;

namespace MiniPdf.Drawing.Drawing2D
{
    /// <summary>
    /// Stores color and position information for gradient interpolation.
    /// </summary>
    public sealed class ColorBlend
    {
        /// <summary>
        /// Initializes a new <see cref="ColorBlend"/> with empty color and position arrays.
        /// </summary>
        public ColorBlend()
        {
            Colors    = System.Array.Empty<Color>();
            Positions = System.Array.Empty<float>();
        }

        /// <summary>
        /// Initializes a new <see cref="ColorBlend"/> with the specified number of colors.
        /// </summary>
        /// <param name="count">The number of colors and positions.</param>
        public ColorBlend(int count)
        {
            Colors    = new Color[count];
            Positions = new float[count];
        }

        /// <summary>
        /// Gets or sets the array of colors for gradient interpolation.
        /// </summary>
        public Color[]  Colors    { get; set; }

        /// <summary>
        /// Gets or sets the array of positions corresponding to each color.
        /// </summary>
        public float[]  Positions { get; set; }
    }
}
