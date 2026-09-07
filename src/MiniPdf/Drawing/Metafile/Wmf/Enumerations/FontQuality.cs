namespace MiniPdf.Drawing.Metafile.Wmf.Enumerations
{
    /// <summary>
    ///     Specifies how closely the attributes of the logical font should match those of the physical font when rendering text.
    /// </summary>
    /// <remarks>
    ///     2.1.1.10 FontQuality Enumeration
    /// </remarks>
    internal enum FontQuality
    {
        /// <summary>
        ///     Specifies that the character quality of the font does not matter, so DRAFT_QUALITY can be used.
        /// </summary>
        DEFAULT_QUALITY = 0x00,

        /// <summary>
        ///     Specifies that the character quality of the font is less important than the matching of logical attribuetes. 
        ///     For rasterized fonts, scaling SHOULD be enabled, which means that more font sizes are available.
        /// </summary>
        DRAFT_QUALITY = 0x01,

        /// <summary>
        ///     Specifies that the character quality of the font is more important than the matching of logical attributes. 
        ///     For rasterized fonts, scaling SHOULD be disabled, and the font closest in size SHOULD be chosen.
        /// </summary>
        PROOF_QUALITY = 0x02,

        /// <summary>
        ///     Specifies that anti-aliasing SHOULD NOT be used when rendering text.
        /// </summary>
        NONANTIALIASED_QUALITY = 0x03,

        /// <summary>
        ///     Specifies that anti-aliasing SHOULD be used when rendering text, if the font supports it.
        /// </summary>
        ANTIALIASED_QUALITY = 0x04,

        /// <summary>
        ///     Specifies that ClearType anti-aliasing SHOULD be used when rendering text, if the font supports it.
        /// </summary>
        CLEARTYPE_QUALITY = 0x05
    }
}
