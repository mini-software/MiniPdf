using System;

namespace MiniSoftware.Drawing.Imaging
{
    /// <summary>
    /// Specifies the attributes of an image.
    /// </summary>
    [Flags]
    public enum ImageFlags
    {
        /// <summary>
        /// No attributes.
        /// </summary>
        None              = 0,

        /// <summary>
        /// Image is scalable.
        /// </summary>
        Scalable          = 0x0001,

        /// <summary>
        /// Image has an alpha channel.
        /// </summary>
        HasAlpha          = 0x0002,

        /// <summary>
        /// Image has translucent pixels.
        /// </summary>
        HasTranslucent    = 0x0004,

        /// <summary>
        /// Image is partially scalable.
        /// </summary>
        PartiallyScalable = 0x0008,

        /// <summary>
        /// Image uses RGB color space.
        /// </summary>
        ColorSpaceRgb     = 0x0010,

        /// <summary>
        /// Image uses CMYK color space.
        /// </summary>
        ColorSpaceCmyk    = 0x0020,

        /// <summary>
        /// Image uses grayscale color space.
        /// </summary>
        ColorSpaceGray    = 0x0040,

        /// <summary>
        /// Image uses YCbCr color space.
        /// </summary>
        ColorSpaceYcbcr   = 0x0080,

        /// <summary>
        /// Image uses YCCK color space.
        /// </summary>
        ColorSpaceYcck    = 0x0100,

        /// <summary>
        /// Image has real DPI information.
        /// </summary>
        HasRealDpi        = 0x1000,

        /// <summary>
        /// Image has real pixel size information.
        /// </summary>
        HasRealPixelSize  = 0x2000,

        /// <summary>
        /// Image is read-only.
        /// </summary>
        ReadOnly          = 0x00010000,

        /// <summary>
        /// Image supports caching.
        /// </summary>
        Caching           = 0x00020000,
    }
}
