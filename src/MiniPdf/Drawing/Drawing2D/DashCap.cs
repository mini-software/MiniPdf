namespace MiniPdf.Drawing.Drawing2D
{
    /// <summary>
    /// Specifies the dash cap style for lines drawn with a <see cref="Pen"/>.
    /// </summary>
    public enum DashCap
    {
        /// <summary>
        /// Square cap with the same width as the line.
        /// </summary>
        Flat     = 0,

        /// <summary>
        /// Round cap with a semicircular arc.
        /// </summary>
        Round    = 2,

        /// <summary>
        /// Triangular cap pointing inward.
        /// </summary>
        Triangle = 3,
    }
}
