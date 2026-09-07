namespace MiniPdf.Drawing.Metafile.Wmf.Enumerations
{
    /// <summary>
    ///     Specifies the type of compression for a bitmap image.
    /// </summary>
    /// <remarks>
    ///     2.1.1.7 Compression Enumeration
    /// </remarks>
    internal enum Compression
    {
        /// <summary>
        ///     The bitmap is in uncompressed red green blue (RGB) format that is not compressed and does not use color masks.
        /// </summary>
        BI_RGB = 0x0000,

        /// <summary>
        ///     An RGB format that uses run-length encoding (RLE) compression for bitmaps with 8 bits per pixel. 
        ///     The compression uses a 2-byte format consisting of a count byte followed by a byte containing a color index.
        /// </summary>
        BI_RLE8 = 0x0001,

        /// <summary>
        ///     An RGB format that uses RLE compression for bitmaps with 4 bits per pixel. 
        ///     The compression uses a 2-byte format consisting of a count byte followed by two word-length color indexes.
        /// </summary>
        BI_RLE4 = 0x0002,

        /// <summary>
        ///     The bitmap is not compressed and the color table consists of three DWORD color masks that specify the red, green, and blue components, respectively, of each pixel. 
        ///     This is valid when used with 16 and 32-bits per pixel bitmaps.
        /// </summary>
        BI_BITFIELDS = 0x0003,

        /// <summary>
        ///     The image is a JPEG image, as specified in [JFIF]. This value SHOULD only be used in certain bitmap operations, such as JPEG pass-through. The application MUST query for the pass-through support, since not all devices support JPEG pass-through. Using non-RGB bitmaps MAY limit the portability of the metafile to other devices. 
        ///     For instance, display device contexts generally do not support this pass-through.
        /// </summary>
        BI_JPEG = 0x0004,

        /// <summary>
        ///     The image is a PNG image, as specified in [RFC2083]. This value SHOULD only be used certain bitmap operations, such as JPEG/PNG pass-through. The application MUST query for the pass-through support, because not all devices support JPEG/PNG pass-through. Using non-RGB bitmaps MAY limit the portability of the metafile to other devices. 
        ///     For instance, display device contexts generally do not support this pass-through.
        /// </summary>
        BI_PNG = 0x0005,

        /// <summary>
        ///     The image is an uncompressed CMYK format.
        /// </summary>
        BI_CMYK = 0x000B,

        /// <summary>
        ///     A CMYK format that uses RLE compression for bitmaps with 8 bits per pixel. 
        ///     The compression uses a 2-byte format consisting of a count byte followed by a byte containing a color index.
        /// </summary>
        BI_CMYKRLE8 = 0x000C,

        /// <summary>
        ///     A CMYK format that uses RLE compression for bitmaps with 4 bits per pixel. The compression uses a 2-byte format consisting of a count byte followed by two word-length color indexes.
        /// </summary>
        BI_CMYKRLE4 = 0x000D
    }
}
