namespace MiniPdf.Drawing.Drawing2D
{
    /// <summary>
    /// Specifies the interpolation mode for scaling and rotating images.
    /// </summary>
    public enum InterpolationMode
    {
        /// <summary>
        /// Invalid interpolation mode.
        /// </summary>
        Invalid              = -1,

        /// <summary>
        /// Default interpolation mode.
        /// </summary>
        Default              = 0,

        /// <summary>
        /// Low quality interpolation.
        /// </summary>
        Low                  = 1,

        /// <summary>
        /// High quality interpolation.
        /// </summary>
        High                 = 2,

        /// <summary>
        /// Bilinear interpolation.
        /// </summary>
        Bilinear             = 3,

        /// <summary>
        /// Bicubic interpolation.
        /// </summary>
        Bicubic              = 4,

        /// <summary>
        /// Nearest neighbor interpolation.
        /// </summary>
        NearestNeighbor      = 5,

        /// <summary>
        /// High quality bilinear interpolation.
        /// </summary>
        HighQualityBilinear  = 6,

        /// <summary>
        /// High quality bicubic interpolation.
        /// </summary>
        HighQualityBicubic   = 7,
    }
}
