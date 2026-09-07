using MiniSoftware.Drawing.Enums;
using System;

namespace MiniSoftware.Drawing.Imaging
{
    /// <summary>
    /// Specifies the attributes of a bitmap used for lock bits operations.
    /// </summary>
    public sealed class BitmapData
    {
        /// <summary>
        /// Gets or sets the width of the locked portion of the bitmap in pixels.
        /// </summary>
        public int         Width       { get; set; }

        /// <summary>
        /// Gets or sets the height of the locked portion of the bitmap in pixels.
        /// </summary>
        public int         Height      { get; set; }

        /// <summary>
        /// Gets or sets the stride (bytes per scan line) of the locked portion of the bitmap.
        /// </summary>
        public int         Stride      { get; set; }

        /// <summary>
        /// Gets or sets the pixel format of the locked portion of the bitmap.
        /// </summary>
        public PixelFormat PixelFormat { get; set; }

        /// <summary>
        /// Gets or sets a pointer to the beginning of the scan line data.
        /// </summary>
        public IntPtr      Scan0       { get; set; }

        /// <summary>
        /// Gets or sets the reserved value (reserved for future use).
        /// </summary>
        public int         Reserved    { get; set; }
    }
}
