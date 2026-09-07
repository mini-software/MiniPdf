using System;
using System.IO;
using MiniPdf.Drawing.Enums;
using MiniPdf.Drawing.Geometry;

namespace MiniPdf.Drawing.Imaging
{
    /// <summary>Callback used by GetThumbnailImage.</summary>
    public delegate bool GetThumbnailImageAbort();

    /// <summary>
    /// Abstract base class for raster and vector images.
    /// Full save/load codecs are added in later batches; BMP I/O is available in Batch 6.
    /// </summary>
    public abstract class Image : IDisposable
    {
        // Resolution (pixels per inch); subclasses may override.
        internal float _dpiX = 96f;
        internal float _dpiY = 96f;

        // ── Dimensions ───────────────────────────────────────────────────────────

        /// <summary>
        /// Gets the width of this image in pixels.
        /// </summary>
        public abstract int Width  { get; }

        /// <summary>
        /// Gets the height of this image in pixels.
        /// </summary>
        public abstract int Height { get; }

        /// <summary>
        /// Gets the size of this image in pixels.
        /// </summary>
        public Size  Size             => new Size(Width, Height);

        /// <summary>
        /// Gets the physical size of this image in pixels.
        /// </summary>
        public SizeF PhysicalDimension => new SizeF(Width, Height);

        // ── Format / flags ───────────────────────────────────────────────────────

        /// <summary>
        /// Gets the pixel format of this image.
        /// </summary>
        public abstract PixelFormat PixelFormat { get; }

        /// <summary>
        /// Gets the format of this image.
        /// </summary>
        public abstract ImageFormat RawFormat   { get; }

        /// <summary>
        /// Gets the flags for this image.
        /// </summary>
        public virtual  int         Flags       => (int)ImageFlags.ColorSpaceRgb;

        // ── Resolution ───────────────────────────────────────────────────────────

        /// <summary>
        /// Gets the horizontal resolution in pixels per inch.
        /// </summary>
        public virtual float HorizontalResolution => _dpiX;

        /// <summary>
        /// Gets the vertical resolution in pixels per inch.
        /// </summary>
        public virtual float VerticalResolution   => _dpiY;

        // ── Coordinate bounds ────────────────────────────────────────────────────

        /// <summary>
        /// Gets the bounds of this image in the specified unit.
        /// </summary>
        /// <param name="pageUnit">The unit to use for the bounds.</param>
        /// <returns>The bounds of the image.</returns>
        public RectangleF GetBounds(ref GraphicsUnit pageUnit)
        {
            pageUnit = GraphicsUnit.Pixel;
            return new RectangleF(0, 0, Width, Height);
        }

        // ── Clone ────────────────────────────────────────────────────────────────

        /// <summary>
        /// Creates an exact copy of this image.
        /// </summary>
        /// <returns>A new <see cref="Image"/> with the same content.</returns>
        public abstract Image Clone();

        // ── Frame access (single-frame for Bitmap) ───────────────────────────────

        /// <summary>
        /// Gets the number of frames in the specified dimension.
        /// </summary>
        /// <param name="dimension">The dimension to count frames in.</param>
        /// <returns>The number of frames.</returns>
        public virtual int GetFrameCount(FrameDimension dimension)              => 1;

        /// <summary>
        /// Selects the frame in the specified dimension.
        /// </summary>
        /// <param name="dimension">The dimension to select the frame in.</param>
        /// <param name="idx">The index of the frame to select.</param>
        /// <returns>The index of the previously selected frame.</returns>
        public virtual int SelectActiveFrame(FrameDimension dimension, int idx) => 0;

        // ── EXIF property items ──────────────────────────────────────────────────

        /// <summary>
        /// Gets the property item for the specified property ID.
        /// </summary>
        /// <param name="propid">The property ID.</param>
        /// <returns>The property item, or <c>null</c> if not found.</returns>
        public virtual PropertyItem?  GetPropertyItem(int propid)           => null;

        /// <summary>
        /// Gets the property IDs for all property items.
        /// </summary>
        /// <returns>An array of property IDs.</returns>
        public virtual int[]          GetPropertyIdList()                    => Array.Empty<int>();

        /// <summary>
        /// Gets all property items.
        /// </summary>
        /// <returns>An array of property items.</returns>
        public virtual PropertyItem[] PropertyItems                          => Array.Empty<PropertyItem>();

        // ── Encoder parameter info (stub) ────────────────────────────────────────

        /// <summary>
        /// Gets the encoder parameters for the specified encoder.
        /// </summary>
        /// <param name="encoderClsid">The encoder CLSID.</param>
        /// <returns>The encoder parameters.</returns>
        public virtual EncoderParameters GetEncoderParameterList(Guid encoderClsid)
            => new EncoderParameters();

        // ── Save ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// Saves this image to the specified file.
        /// </summary>
        /// <param name="filename">The file name to save to.</param>
        public abstract void Save(string filename);

        /// <summary>
        /// Saves this image to the specified file in the specified format.
        /// </summary>
        /// <param name="filename">The file name to save to.</param>
        /// <param name="format">The format to save in.</param>
        public abstract void Save(string filename, ImageFormat format);

        /// <summary>
        /// Saves this image to the specified stream in the specified format.
        /// </summary>
        /// <param name="stream">The stream to save to.</param>
        /// <param name="format">The format to save in.</param>
        public abstract void Save(Stream stream, ImageFormat format);

        /// <summary>
        /// Saves this image to the specified stream in the specified format with encoder parameters.
        /// </summary>
        /// <param name="stream">The stream to save to.</param>
        /// <param name="format">The format to save in.</param>
        /// <param name="encoderParams">The encoder parameters.</param>
        public virtual void Save(Stream stream, ImageFormat format, EncoderParameters? encoderParams)
            => Save(stream, format);

        // ── Rotate/flip ──────────────────────────────────────────────────────────

        /// <summary>
        /// Rotates and flips the image.
        /// </summary>
        /// <param name="rotateFlipType">The rotation and flip type.</param>
        public abstract void RotateFlip(RotateFlipType rotateFlipType);

        // ── Thumbnail (requires rasterizer — Batch 7) ────────────────────────────

        /// <summary>
        /// Returns a thumbnail image of the specified size.
        /// </summary>
        /// <param name="thumbWidth">The width of the thumbnail.</param>
        /// <param name="thumbHeight">The height of the thumbnail.</param>
        /// <param name="callback">The callback for aborting the operation.</param>
        /// <param name="callbackData">Callback data.</param>
        /// <returns>The thumbnail image.</returns>
        public virtual Image GetThumbnailImage(
            int thumbWidth, int thumbHeight,
            GetThumbnailImageAbort? callback, IntPtr callbackData)
            => throw new NotImplementedException("GetThumbnailImage requires the Batch 7 rasterizer.");

        // ── Static factory ───────────────────────────────────────────────────────

        /// <summary>
        /// Creates an <see cref="Image"/> from the specified file.
        /// </summary>
        /// <param name="filename">The file to load from.</param>
        /// <returns>The loaded image.</returns>
        public static Image FromFile(string filename)   => Bitmap.FromFile(filename);

        /// <summary>
        /// Creates an <see cref="Image"/> from the specified stream.
        /// </summary>
        /// <param name="stream">The stream to load from.</param>
        /// <returns>The loaded image.</returns>
        public static Image FromStream(Stream stream)   => Bitmap.FromStream(stream);

        // ── IDisposable ──────────────────────────────────────────────────────────

        private bool _disposed;

        /// <summary>
        /// Releases the unmanaged resources used by this image and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">
        /// <c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.
        /// </param>
        protected virtual void Dispose(bool disposing) { _disposed = true; }

        /// <summary>
        /// Releases all resources used by this image.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
