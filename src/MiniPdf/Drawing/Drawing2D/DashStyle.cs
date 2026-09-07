namespace MiniPdf.Drawing.Drawing2D
{
    /// <summary>
    /// Specifies the style of dashed lines drawn with a <see cref="Pen"/>.
    /// </summary>
    public enum DashStyle
    {
        /// <summary>
        /// Solid line.
        /// </summary>
        Solid      = 0,

        /// <summary>
        /// Dashed line.
        /// </summary>
        Dash       = 1,

        /// <summary>
        /// Dotted line.
        /// </summary>
        Dot        = 2,

        /// <summary>
        /// Dash-dot line.
        /// </summary>
        DashDot    = 3,

        /// <summary>
        /// Dash-dot-dot line.
        /// </summary>
        DashDotDot = 4,

        /// <summary>
        /// Custom dash pattern.
        /// </summary>
        Custom     = 5,
    }
}
