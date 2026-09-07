namespace MiniPdf.Drawing.Metafile.Wmf.Enumerations
{
    /// <summary>
    ///     Defines values for output precision, which is the requirement for the font mapper to match specific font parameters, including height, width, character orientation, escapement, pitch, and font type.
    /// </summary>
    /// <remarks>
    ///     2.1.1.21 OutPrecision Enumeration
    /// </remarks>
    internal enum OutPrecision
    {
        /// <summary>
        ///     A value that specifies default behavior.
        /// </summary>
        OUT_DEFAULT_PRECIS = 0x00000000,

        /// <summary>
        ///     A value that is returned when rasterized fonts are enumerated.
        /// </summary>
        OUT_STRING_PRECIS = 0x00000001,

        /// <summary>
        ///     A value that is returned when TrueType and other outline fonts, and vector fonts are enumerated.
        /// </summary>
        OUT_STROKE_PRECIS = 0x00000003,

        /// <summary>
        ///     A value that specifies the choice of a TrueType font when the system contains multiple fonts with the same name.
        /// </summary>
        OUT_TT_PRECIS = 0x00000004,

        /// <summary>
        ///     A value that specifies the choice of a device font when the system contains multiple fonts with the same name.
        /// </summary>
        OUT_DEVICE_PRECIS = 0x00000005,

        /// <summary>
        ///     A value that specifies the choice of a rasterized font when the system contains multiple fonts with the same name.
        /// </summary>
        OUT_RASTER_PRECIS = 0x00000006,

        /// <summary>
        ///     A value that specifies the requirement for only TrueType fonts. If there are no TrueType fonts installed in the system, default behavior is specified.
        /// </summary>
        OUT_TT_ONLY_PRECIS = 0x00000007,

        /// <summary>
        ///     A value that specifies the requirement for TrueType and other outline fonts.
        /// </summary>
        OUT_OUTLINE_PRECIS = 0x00000008,

        /// <summary>
        ///     A value that specifies a preference for TrueType and other outline fonts.
        /// </summary>
        OUT_SCREEN_OUTLINE_PRECIS = 0x00000009,

        /// <summary>
        ///     A value that specifies a requirement for only PostScript fonts. If there are no PostScript fonts installed in the system, default behavior is specified.
        /// </summary>
        OUT_PS_ONLY_PRECIS = 0x0000000A
    }
}
