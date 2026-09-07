namespace MiniPdf.Drawing.Drawing2D
{
    /// <summary>
    /// Specifies how the interior of a shape defined by a <see cref="GraphicsPath"/> is filled.
    /// </summary>
    public enum FillMode
    {
        /// <summary>
        /// Alternate fill mode (default).
        /// </summary>
        Alternate = 0,

        /// <summary>
        /// Winding fill mode.
        /// </summary>
        Winding   = 1,
    }
}
