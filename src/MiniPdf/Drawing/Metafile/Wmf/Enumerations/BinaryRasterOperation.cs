namespace MiniPdf.Drawing.Metafile.Wmf.Enumerations
{
    /// <summary>
    ///     Lists the binary raster-operation codes. Raster-operation codes define how metafile processing combines the bits from the selected pen with the bits in the destination bitmap.
    /// </summary>
    internal enum BinaryRasterOperation
    {
        /// <summary>
        ///     0, Pixel is always 0.
        /// </summary>
        R2_BLACK = 0x0001,

        /// <summary>
        ///     DPon, Pixel is the inverse of the R2_MERGEPEN color.
        /// </summary>
        R2_NOTMERGEPEN = 0x0002,

        /// <summary>
        ///     DPna, Pixel is a combination of the screen color and the inverse of the pen color.
        /// </summary>
        R2_MASKNOTPEN = 0x0003,

        /// <summary>
        ///     Pn, Pixel is the inverse of the pen color.
        /// </summary>
        R2_NOTCOPYPEN = 0x0004,

        /// <summary>
        ///     PDna, Pixel is a combination of the colors common to both the pen and the inverse of the screen.
        /// </summary>
        R2_MASKPENNOT = 0x0005,

        /// <summary>
        ///     Dn, Pixel is the inverse of the screen color.
        /// </summary>
        R2_NOT = 0x0006,

        /// <summary>
        ///     DPx, Pixel is a combination of the colors in the pen or in the screen, but not in both.
        /// </summary>
        R2_XORPEN = 0x0007,

        /// <summary>
        ///     DPan, Pixel is the inverse of the R2_MASKPEN color.
        /// </summary>
        R2_NOTMASKPEN = 0x0008,

        /// <summary>
        ///     DPa, Pixel is a combination of the colors common to both the pen and the screen.
        /// </summary>
        R2_MASKPEN = 0x0009,

        /// <summary>
        ///     DPxn, Pixel is the inverse of the R2_XORPEN color.
        /// </summary>
        R2_NOTXORPEN = 0x000A,

        /// <summary>
        ///     D, Pixel remains unchanged.
        /// </summary>
        R2_NOP = 0x000B,

        /// <summary>
        ///     PDno, Pixel is a combination of the pen color and the inverse of the screen color.
        /// </summary>
        R2_MERGENOTPEN = 0x000C,

        /// <summary>
        ///     P, Pixel is the pen color.
        /// </summary>
        R2_COPYPEN = 0x000D,

        /// <summary>
        ///     PDno, Pixel is a combination of the pen color and the inverse of the screen color.
        /// </summary>
        R2_MERGEPENNOT = 0x000E,

        /// <summary>
        ///     DPo, Pixel is a combination of the pen color and the screen color.
        /// </summary>
        R2_MERGEPEN = 0x000F,

        /// <summary>
        ///     1, Pixel is always 1
        /// </summary>
        R2_WHITE = 0x0010
    }
}
