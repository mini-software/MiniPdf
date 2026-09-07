namespace MiniPdf.Drawing.Text
{
    /// <summary>
    /// Specifies the text rendering hint for text quality.
    /// </summary>
    public enum TextRenderingHint
    {
        /// <summary>
        /// Use the system default text rendering hint.
        /// </summary>
        SystemDefault = 0,

        /// <summary>
        /// Use single bit per pixel with grid fit.
        /// </summary>
        SingleBitPerPixelGridFit = 1,

        /// <summary>
        /// Use single bit per pixel.
        /// </summary>
        SingleBitPerPixel = 2,

        /// <summary>
        /// Use anti-aliasing with grid fit.
        /// </summary>
        AntiAliasGridFit = 3,

        /// <summary>
        /// Use anti-aliasing.
        /// </summary>
        AntiAlias = 4,

        /// <summary>
        /// Use ClearType font rendering.
        /// </summary>
        ClearTypeGridFit = 5
    }
}
