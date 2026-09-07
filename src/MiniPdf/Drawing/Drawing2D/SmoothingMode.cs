namespace MiniPdf.Drawing.Drawing2D
{
    /// <summary>
    /// Specifies the smoothing mode for rendering lines and curves.
    /// </summary>
    public enum SmoothingMode
    {
        /// <summary>
        /// Invalid smoothing mode.
        /// </summary>
        Invalid     = -1,

        /// <summary>
        /// Default smoothing mode (no anti-aliasing).
        /// </summary>
        Default     = 0,

        /// <summary>
        /// High speed, no smoothing.
        /// </summary>
        HighSpeed   = 1,

        /// <summary>
        /// High quality smoothing.
        /// </summary>
        HighQuality = 2,

        /// <summary>
        /// No smoothing.
        /// </summary>
        None        = 3,

        /// <summary>
        /// Anti-aliased smoothing.
        /// </summary>
        AntiAlias   = 4,
    }
}
