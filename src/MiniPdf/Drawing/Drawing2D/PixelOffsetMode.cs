namespace MiniSoftware.Drawing.Drawing2D
{
    /// <summary>
    /// Specifies the pixel offset mode for rendering.
    /// </summary>
    public enum PixelOffsetMode
    {
        /// <summary>
        /// Invalid pixel offset mode.
        /// </summary>
        Invalid     = -1,

        /// <summary>
        /// Default pixel offset mode.
        /// </summary>
        Default     = 0,

        /// <summary>
        /// High speed pixel offset mode.
        /// </summary>
        HighSpeed   = 1,

        /// <summary>
        /// High quality pixel offset mode.
        /// </summary>
        HighQuality = 2,

        /// <summary>
        /// No pixel offset.
        /// </summary>
        None        = 3,

        /// <summary>
        /// Half-pixel offset for improved quality.
        /// </summary>
        Half        = 4,
    }
}
