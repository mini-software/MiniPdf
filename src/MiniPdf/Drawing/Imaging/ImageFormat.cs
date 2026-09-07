using System;

namespace MiniPdf.Drawing.Imaging
{
    /// <summary>
    /// Identifies an image codec (format).
    /// </summary>
    public sealed class ImageFormat
    {
        /// <summary>
        /// Gets the GUID of this image format.
        /// </summary>
        public Guid Guid { get; }

        /// <summary>
        /// Gets the name of this image format.
        /// </summary>
        public string Name { get; }

        private ImageFormat(string name, string guid)
        {
            Name = name;
            Guid = new Guid(guid);
        }

        // ── Known formats ────────────────────────────────────────────────────────

        /// <summary>
        /// Gets the Windows Bitmap (BMP) image format.
        /// </summary>
        public static readonly ImageFormat Bmp  = new ImageFormat("Bmp",  "b96b3cab-0728-11d3-9d7b-0000f81ef32e");

        /// <summary>
        /// Gets the Windows Enhanced Metafile (EMF) image format.
        /// </summary>
        public static readonly ImageFormat Emf  = new ImageFormat("Emf",  "b96b3cac-0728-11d3-9d7b-0000f81ef32e");

        /// <summary>
        /// Gets the Windows Metafile (WMF) image format.
        /// </summary>
        public static readonly ImageFormat Wmf  = new ImageFormat("Wmf",  "b96b3cad-0728-11d3-9d7b-0000f81ef32e");

        /// <summary>
        /// Gets the Joint Photographic Experts Group (JPEG) image format.
        /// </summary>
        public static readonly ImageFormat Jpeg = new ImageFormat("Jpeg", "b96b3cae-0728-11d3-9d7b-0000f81ef32e");

        /// <summary>
        /// Gets the Portable Network Graphics (PNG) image format.
        /// </summary>
        public static readonly ImageFormat Png  = new ImageFormat("Png",  "b96b3caf-0728-11d3-9d7b-0000f81ef32e");

        /// <summary>
        /// Gets the Graphics Interchange Format (GIF) image format.
        /// </summary>
        public static readonly ImageFormat Gif  = new ImageFormat("Gif",  "b96b3cb0-0728-11d3-9d7b-0000f81ef32e");

        /// <summary>
        /// Gets the Tagged Image File Format (TIFF) image format.
        /// </summary>
        public static readonly ImageFormat Tiff = new ImageFormat("Tiff", "b96b3cb1-0728-11d3-9d7b-0000f81ef32e");

        /// <summary>
        /// Gets the Windows Icon (ICO) image format.
        /// </summary>
        public static readonly ImageFormat Icon = new ImageFormat("Icon", "b96b3cb5-0728-11d3-9d7b-0000f81ef32e");

        /// <summary>
        /// Gets the Scalable Vector Graphics (SVG) image format.
        /// </summary>
        public static readonly ImageFormat Svg = new ImageFormat("Svg", "a6b1c000-0000-0000-0000-0000000000a6");

        /// <summary>
        /// Gets the memory bitmap image format.
        /// </summary>
        public static readonly ImageFormat MemoryBmp = new ImageFormat("MemoryBmp", "b96b3caa-0728-11d3-9d7b-0000f81ef32e");

        /// <summary>
        /// Returns a string representation of this image format.
        /// </summary>
        /// <returns>A string containing the format GUID.</returns>
        public override string ToString() => $"[ImageFormat: {Guid}]";

        /// <summary>
        /// Determines whether this image format is equal to another object.
        /// </summary>
        /// <param name="obj">The object to compare.</param>
        /// <returns><c>true</c> if the formats are equal; otherwise, <c>false</c>.</returns>
        public override bool   Equals(object? obj) => obj is ImageFormat f && f.Guid == Guid;

        /// <summary>
        /// Returns the hash code for this image format.
        /// </summary>
        /// <returns>The hash code.</returns>
        public override int    GetHashCode() => Guid.GetHashCode();
    }
}
