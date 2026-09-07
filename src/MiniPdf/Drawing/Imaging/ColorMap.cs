using MiniPdf.Drawing.Colors;

namespace MiniPdf.Drawing.Imaging
{
    /// <summary>
    /// Maps one color to another during image rendering.
    /// </summary>
    public sealed class ColorMap
    {
        /// <summary>
        /// Gets or sets the source color to map from.
        /// </summary>
        public Color OldColor { get; set; }

        /// <summary>
        /// Gets or sets the destination color to map to.
        /// </summary>
        public Color NewColor { get; set; }
    }
}
