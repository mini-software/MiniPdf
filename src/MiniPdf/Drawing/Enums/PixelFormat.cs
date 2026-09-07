namespace MiniSoftware.Drawing.Enums
{
    /// <summary>
    /// Specifies the format of the color data for each pixel in an image.
    /// </summary>
    public enum PixelFormat
    {
        /// <summary>
        /// Undefined pixel format.
        /// </summary>
        Undefined           = 0,

        /// <summary>
        /// Pixel format is not important.
        /// </summary>
        DontCare            = 0,

        /// <summary>
        /// Maximum value for pixel format enumeration.
        /// </summary>
        Max                 = 15,

        /// <summary>
        /// Indexed color format.
        /// </summary>
        Indexed             = 0x00010000,

        /// <summary>
        /// GDI-supported pixel format.
        /// </summary>
        Gdi                 = 0x00020000,

        /// <summary>
        /// 16 bits per pixel; 5 bits each for red, green, and blue; 5 bits for alpha.
        /// </summary>
        Format16bppRgb555   = 0x00021005,

        /// <summary>
        /// 16 bits per pixel; 5 bits for red, 6 bits for green, 5 bits for blue.
        /// </summary>
        Format16bppRgb565   = 0x00021006,

        /// <summary>
        /// 24 bits per pixel; 8 bits each for red, green, and blue.
        /// </summary>
        Format24bppRgb      = 0x00021808,

        /// <summary>
        /// 32 bits per pixel; 8 bits each for red, green, and blue.
        /// </summary>
        Format32bppRgb      = 0x00022009,

        /// <summary>
        /// 1 bit per pixel indexed color.
        /// </summary>
        Format1bppIndexed   = 0x00030101,

        /// <summary>
        /// 4 bits per pixel indexed color.
        /// </summary>
        Format4bppIndexed   = 0x00030402,

        /// <summary>
        /// 8 bits per pixel indexed color.
        /// </summary>
        Format8bppIndexed   = 0x00030803,

        /// <summary>
        /// Alpha channel present.
        /// </summary>
        Alpha               = 0x00040000,

        /// <summary>
        /// 16 bits per pixel; 1 bit for alpha, 5 bits each for red, green, and blue.
        /// </summary>
        Format16bppArgb1555 = 0x00061007,

        /// <summary>
        /// Premultiplied alpha format.
        /// </summary>
        PAlpha              = 0x00080000,

        /// <summary>
        /// 32 bits per pixel; 8 bits each for red, green, blue, and alpha; premultiplied.
        /// </summary>
        Format32bppPArgb    = 0x000E200B,

        /// <summary>
        /// Extended pixel format.
        /// </summary>
        Extended            = 0x00100000,

        /// <summary>
        /// 16 bits per pixel grayscale.
        /// </summary>
        Format16bppGrayScale= 0x00101004,

        /// <summary>
        /// 48 bits per pixel; 16 bits each for red, green, and blue.
        /// </summary>
        Format48bppRgb      = 0x0010300C,

        /// <summary>
        /// 64 bits per pixel; 16 bits each for red, green, blue, and alpha; premultiplied.
        /// </summary>
        Format64bppPArgb    = 0x001C400E,

        /// <summary>
        /// Canonical pixel format.
        /// </summary>
        Canonical           = 0x00200000,

        /// <summary>
        /// 32 bits per pixel; 8 bits each for alpha, red, green, and blue.
        /// </summary>
        Format32bppArgb     = 0x0026200A,

        /// <summary>
        /// 64 bits per pixel; 16 bits each for alpha, red, green, and blue.
        /// </summary>
        Format64bppArgb     = 0x0034400D,
    }
}
