namespace MiniPdf.Drawing.Metafile.EmfPlus.Enumerations
{
    /// <summary>
    /// Specifies the text rendering hint for EMF+ text rendering.
    /// </summary>
    internal enum EmfPlusTextRenderingHint : ushort
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
        /// Use anti-alias with grid fit.
        /// </summary>
        AntiAliasGridFit = 3,

        /// <summary>
        /// Use anti-alias.
        /// </summary>
        AntiAlias = 4,

        /// <summary>
        /// Use ClearType with grid fit.
        /// </summary>
        ClearTypeGridFit = 5
    }
}
