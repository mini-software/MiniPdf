namespace MiniSoftware.Drawing.Metafile.Wmf.Enumerations
{
    /// <summary>
    ///     Specifies the number of bits that define each pixel and the maximum number of colors in a device-independent bitmap (DIB).
    /// </summary>
    /// <remarks>
    ///     2.1.1.3 BitCount Enumeration
    /// </remarks>
    internal enum BitCount
    {
        /// <summary>
        ///     The number of bits per pixel is undefined.
        /// </summary>
        BI_BITCOUNT_0 = 0x0000,

        /// <summary>
        ///     The image is specified with two colors.
        /// </summary>
        BI_BITCOUNT_1 = 0x0001,
        BI_BITCOUNT_2 = 0x0004,
        BI_BITCOUNT_3 = 0x0008,
        BI_BITCOUNT_4 = 0x0010,
        BI_BITCOUNT_5 = 0x0018,

        /// <summary>
        ///     The bitmap has a maximum of 2^24 colors.
        /// </summary>
        BI_BITCOUNT_6 = 0x0020
    }
}
