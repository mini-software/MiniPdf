namespace MiniSoftware.Drawing.Metafile.Wmf.Enumerations
{
    /// <summary>
    ///     The GamutMappingIntent Enumeration specifies the relationship between logical and physical colors.
    /// </summary>
    /// <remarks>
    ///     2.1.1.11 GamutMappingIntent Enumeration
    /// </remarks>
    internal enum GamutMappingIntent
    {
        /// <summary>
        ///     Specifies that the white point SHOULD be maintained. Typically used when logical colors MUST be matched to their nearest physical color in the destination color gamut.
        /// </summary>
        LCS_GM_ABS_COLORIMETRIC = 0x00000008,

        /// <summary>
        ///     Specifies that saturation SHOULD be maintained. Typically used for business charts and other situations in which dithering is not required.
        /// </summary>
        LCS_GM_BUSINESS = 0x00000001,

        /// <summary>
        ///     Specifies that a colorimetric match SHOULD be maintained. Typically used for graphic designs and named colors.
        /// </summary>
        LCS_GM_GRAPHICS = 0x00000002,

        /// <summary>
        ///     Specifies that contrast SHOULD be maintained. Typically used for photographs and natural images.
        /// </summary>
        LCS_GM_IMAGES = 0x00000004
    }
}
