namespace MiniSoftware.Drawing.Drawing2D
{
    /// <summary>
    /// Specifies the quality level for compositing operations.
    /// </summary>
    public enum CompositingQuality
    {
        /// <summary>
        /// Invalid quality setting.
        /// </summary>
        Invalid         = -1,

        /// <summary>
        /// Default quality (balanced speed and quality).
        /// </summary>
        Default         = 0,

        /// <summary>
        /// High speed, lower quality compositing.
        /// </summary>
        HighSpeed       = 1,

        /// <summary>
        /// High quality, slower compositing.
        /// </summary>
        HighQuality     = 2,

        /// <summary>
        /// Gamma-corrected compositing.
        /// </summary>
        GammaCorrected  = 3,

        /// <summary>
        /// Assume linear color space for compositing.
        /// </summary>
        AssumeLinear    = 4,
    }
}
