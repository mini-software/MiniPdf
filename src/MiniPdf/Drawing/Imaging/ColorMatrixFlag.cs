namespace MiniSoftware.Drawing.Imaging
{
    /// <summary>
    /// Specifies the flags for color matrix operations.
    /// </summary>
    public enum ColorMatrixFlag
    {
        /// <summary>
        /// Default color matrix behavior.
        /// </summary>
        Default   = 0,

        /// <summary>
        /// Skip grayscale colors.
        /// </summary>
        SkipGrays = 1,

        /// <summary>
        /// Use alternate grayscale.
        /// </summary>
        AltGray   = 2,
    }
}
