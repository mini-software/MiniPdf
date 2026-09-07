using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using MiniSoftware.Drawing.Colors;
using MiniSoftware.Drawing.Enums;
using MiniSoftware.Drawing.Geometry;

namespace MiniSoftware.Drawing.Imaging
{
    /// <summary>
    /// Raster image stored as a managed byte array (top-down, BGRA / BGR layout).
    /// BMP file I/O is supported natively; other formats throw NotSupportedException.
    /// </summary>
    public sealed class Bitmap : Image
    {
        internal byte[]      _pixels;   // length = _height * _stride
        internal int         _width, _height, _stride;
        internal PixelFormat _format;
        internal ImageFormat _rawFormat = ImageFormat.MemoryBmp;

        // LockBits support
        private GCHandle _lockHandle;
        private bool     _locked;

        // Multi-frame (animation) support — APNG, animated GIF.
        private List<AnimationFrame>? _frames;
        private FrameAnimator?        _animator;
        private byte[]?               _defaultPixels;
        private int                   _activeFrame = -1;
        private int                   _loopCount;

        // ── Constructors ────────────────────────────────────────────────────────

        /// <summary>
        /// Initializes a new <see cref="Bitmap"/> with the specified width and height.
        /// </summary>
        /// <param name="width">The width of the bitmap.</param>
        /// <param name="height">The height of the bitmap.</param>
        public Bitmap(int width, int height)
            : this(width, height, PixelFormat.Format32bppArgb) { }

        /// <summary>
        /// Initializes a new <see cref="Bitmap"/> with the specified width, height, and pixel format.
        /// </summary>
        /// <param name="width">The width of the bitmap.</param>
        /// <param name="height">The height of the bitmap.</param>
        /// <param name="format">The pixel format of the bitmap.</param>
        public Bitmap(int width, int height, PixelFormat format)
        {
            if (width  <= 0) throw new ArgumentOutOfRangeException(nameof(width));
            if (height <= 0) throw new ArgumentOutOfRangeException(nameof(height));
            _width  = width;
            _height = height;
            _format = format;
            _stride = CalcStride(width, format);
            _pixels = new byte[_height * _stride];
        }

        /// <summary>
        /// Initializes a new <see cref="Bitmap"/> from an external pixel buffer.
        /// </summary>
        /// <param name="width">The width of the bitmap.</param>
        /// <param name="height">The height of the bitmap.</param>
        /// <param name="stride">The stride (bytes per row) of the pixel buffer.</param>
        /// <param name="format">The pixel format of the bitmap.</param>
        /// <param name="scan0">A pointer to the pixel buffer.</param>
        public Bitmap(int width, int height, int stride, PixelFormat format, IntPtr scan0)
        {
            if (width  <= 0) throw new ArgumentOutOfRangeException(nameof(width));
            if (height <= 0) throw new ArgumentOutOfRangeException(nameof(height));
            _width  = width;
            _height = height;
            _stride = stride;
            _format = format;
            _pixels = new byte[Math.Abs(stride) * height];
            Marshal.Copy(scan0, _pixels, 0, _pixels.Length);
        }

        /// <summary>
        /// Initializes a new <see cref="Bitmap"/> from the specified file.
        /// </summary>
        /// <param name="filename">The file to load from.</param>
        public Bitmap(string filename)
        {
            using (var fs = File.OpenRead(filename))
            {
                var loaded = Codecs.CodecRegistry.Decode(fs);
                CopyLoaded(loaded);
            }
        }

        /// <summary>
        /// Initializes a new <see cref="Bitmap"/> from the specified stream.
        /// </summary>
        /// <param name="stream">The stream to load from.</param>
        public Bitmap(Stream stream)
        {
            var loaded = Codecs.CodecRegistry.Decode(stream);
            CopyLoaded(loaded);
        }

        /// <summary>
        /// Initializes a new <see cref="Bitmap"/> as a copy of an existing image.
        /// </summary>
        /// <param name="original">The image to copy from.</param>
        public Bitmap(Image original)
        {
            if (original == null) throw new ArgumentNullException(nameof(original));
            _width  = original.Width;
            _height = original.Height;
            _format = PixelFormat.Format32bppArgb;
            _stride = CalcStride(_width, _format);
            _pixels = new byte[_height * _stride];
            _dpiX   = original.HorizontalResolution;
            _dpiY   = original.VerticalResolution;

            if (original is Bitmap src)
            {
                CopyFrom(src);
            }
            else if (original is Metafile metafile)
            {
                using var rendered = metafile.RenderToBitmap(_width, _height);
                CopyFrom(rendered);
            }
        }

        // Private constructor used by Clone/Load helpers
        private Bitmap() { _pixels = Array.Empty<byte>(); }

        // ── Properties ──────────────────────────────────────────────────────────

        /// <summary>
        /// Gets the width of this bitmap in pixels.
        /// </summary>
        public override int         Width       => _width;

        /// <summary>
        /// Gets the height of this bitmap in pixels.
        /// </summary>
        public override int         Height      => _height;

        /// <summary>
        /// Gets the pixel format of this bitmap.
        /// </summary>
        public override PixelFormat PixelFormat => _format;

        /// <summary>
        /// Gets the format of this bitmap.
        /// </summary>
        public override ImageFormat RawFormat   => _rawFormat;

        /// <summary>
        /// Gets the flags for this bitmap.
        /// </summary>
        public override int         Flags
            => (int)(ImageFlags.ColorSpaceRgb | ImageFlags.HasRealDpi | ImageFlags.HasRealPixelSize);

        // ── Multi-frame (animation) ──────────────────────────────────────────────

        internal void SetAnimationFrames(List<AnimationFrame> frames, int loopCount)
        {
            _frames = frames;
            _loopCount = loopCount;
            _animator = new FrameAnimator(_width, _height, frames);
            _defaultPixels = (byte[])_pixels.Clone();
            _activeFrame = -1;
        }

        internal List<AnimationFrame>? GetAnimationFrames() => _frames;

        internal int GetAnimationLoopCount() => _loopCount;

        public override int GetFrameCount(FrameDimension dimension)
        {
            if (dimension != null && dimension.Equals(FrameDimension.Time) && _frames != null)
                return _frames.Count;
            return 1;
        }

        public override int SelectActiveFrame(FrameDimension dimension, int idx)
        {
            if (dimension == null || !dimension.Equals(FrameDimension.Time) || _frames == null)
                return 0;

            if (idx < 0 || idx >= _frames.Count)
                throw new ArgumentOutOfRangeException(nameof(idx));

            int previous = _activeFrame;

            if (_defaultPixels == null)
                _defaultPixels = (byte[])_pixels.Clone();

            if (idx == _activeFrame)
                return previous;

            byte[] composed = _animator!.Compose(idx);
            Buffer.BlockCopy(composed, 0, _pixels, 0, Math.Min(composed.Length, _pixels.Length));
            _activeFrame = idx;

            return previous < 0 ? 0 : previous;
        }

        // ── Pixel access ─────────────────────────────────────────────────────────

        /// <summary>
        /// Gets the color of the pixel at the specified coordinates.
        /// </summary>
        /// <param name="x">The x-coordinate of the pixel.</param>
        /// <param name="y">The y-coordinate of the pixel.</param>
        /// <returns>The color of the pixel.</returns>
        public Color GetPixel(int x, int y)
        {
            if ((uint)x >= (uint)_width || (uint)y >= (uint)_height)
                throw new ArgumentOutOfRangeException();

            int bpp = GetBytesPerPixel(_format);
            int off = y * _stride + x * bpp;

            switch (_format)
            {
                case PixelFormat.Format32bppArgb:
                case PixelFormat.Format32bppRgb:
                {
                    byte b = _pixels[off],   g = _pixels[off + 1],
                         r = _pixels[off + 2], a = _pixels[off + 3];
                    return Color.FromArgb(a, r, g, b);
                }
                case PixelFormat.Format32bppPArgb:
                {
                    byte pb = _pixels[off], pg = _pixels[off + 1],
                         pr = _pixels[off + 2], a = _pixels[off + 3];
                    if (a == 0) return Color.FromArgb(0, 0, 0, 0);
                    int r = Math.Min(255, (pb * 255) / a);
                    int g = Math.Min(255, (pg * 255) / a);
                    int bl = Math.Min(255, (pr * 255) / a);
                    return Color.FromArgb(a, bl, g, r);
                }
                case PixelFormat.Format24bppRgb:
                {
                    byte b = _pixels[off], g = _pixels[off + 1], r = _pixels[off + 2];
                    return Color.FromArgb(255, r, g, b);
                }
                case PixelFormat.Format16bppRgb565:
                {
                    int v = _pixels[off] | (_pixels[off + 1] << 8);
                    int r = ((v >> 11) & 0x1F) * 255 / 31;
                    int g = ((v >>  5) & 0x3F) * 255 / 63;
                    int b = ( v        & 0x1F) * 255 / 31;
                    return Color.FromArgb(255, r, g, b);
                }
                default:
                    throw new NotSupportedException($"GetPixel not supported for {_format}.");
            }
        }

        /// <summary>
        /// Sets the color of the pixel at the specified coordinates.
        /// </summary>
        /// <param name="x">The x-coordinate of the pixel.</param>
        /// <param name="y">The y-coordinate of the pixel.</param>
        /// <param name="color">The color to set.</param>
        public void SetPixel(int x, int y, Color color)
        {
            if ((uint)x >= (uint)_width || (uint)y >= (uint)_height)
                throw new ArgumentOutOfRangeException();

            int bpp = GetBytesPerPixel(_format);
            int off = y * _stride + x * bpp;

            switch (_format)
            {
                case PixelFormat.Format32bppArgb:
                case PixelFormat.Format32bppRgb:
                    _pixels[off]     = color.B;
                    _pixels[off + 1] = color.G;
                    _pixels[off + 2] = color.R;
                    _pixels[off + 3] = color.A;
                    break;

                case PixelFormat.Format32bppPArgb:
                {
                    byte a = color.A;
                    _pixels[off]     = (byte)(color.B * a / 255);
                    _pixels[off + 1] = (byte)(color.G * a / 255);
                    _pixels[off + 2] = (byte)(color.R * a / 255);
                    _pixels[off + 3] = a;
                    break;
                }

                case PixelFormat.Format24bppRgb:
                    _pixels[off]     = color.B;
                    _pixels[off + 1] = color.G;
                    _pixels[off + 2] = color.R;
                    break;

                default:
                    throw new NotSupportedException($"SetPixel not supported for {_format}.");
            }
        }

        // ── LockBits / UnlockBits ───────────────────────────────────────────────

        /// <summary>
        /// Locks a rectangular region of the bitmap for pixel access.
        /// </summary>
        /// <param name="rect">The region to lock.</param>
        /// <param name="flags">The lock mode flags.</param>
        /// <param name="format">The pixel format to use for the locked data.</param>
        /// <returns>The locked bitmap data.</returns>
        public BitmapData LockBits(Rectangle rect, ImageLockMode flags, PixelFormat format)
        {
            if (_locked) throw new InvalidOperationException("Bitmap is already locked.");
            ValidateRect(rect);

            // For same-format lock we pin the original buffer directly.
            // For format-converting lock we create a temporary converted buffer.
            byte[] buffer;
            int    stride;

            if (format == _format)
            {
                buffer = _pixels;
                stride = _stride;
            }
            else
            {
                // Convert to requested format
                buffer = ConvertRegion(rect, format, out stride);
            }

            _lockHandle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            _locked     = true;

            int bpp = GetBytesPerPixel(format);
            IntPtr scan0;

            if (format == _format)
            {
                // Offset within the main buffer
                scan0 = IntPtr.Add(_lockHandle.AddrOfPinnedObject(),
                                   rect.Y * _stride + rect.X * bpp);
            }
            else
            {
                scan0 = _lockHandle.AddrOfPinnedObject();
            }

            return new BitmapData
            {
                Width       = rect.Width,
                Height      = rect.Height,
                Stride      = stride,
                PixelFormat = format,
                Scan0       = scan0,
                Reserved    = (format != _format) ? 1 : 0, // flag: converted
            };
        }

        /// <summary>
        /// Unlocks a previously locked region of the bitmap.
        /// </summary>
        /// <param name="bitmapData">The locked bitmap data to unlock.</param>
        public void UnlockBits(BitmapData bitmapData)
        {
            if (!_locked) return;
            if (_lockHandle.IsAllocated)
                _lockHandle.Free();
            _locked = false;
        }

        // ── Transparency ────────────────────────────────────────────────────────

        /// <summary>
        /// Makes the default color transparent for this bitmap.
        /// </summary>
        public void MakeTransparent()
        {
            // Use the bottom-left pixel color as the transparent color
            if (_width == 0 || _height == 0) return;
            MakeTransparent(GetPixel(0, _height - 1));
        }

        /// <summary>
        /// Makes the specified color transparent for this bitmap.
        /// </summary>
        /// <param name="transparentColor">The color to make transparent.</param>
        public void MakeTransparent(Color transparentColor)
        {
            EnsureAlpha();
            uint target = unchecked((uint)transparentColor.ToArgb()) | 0xFF000000u; // ignore source alpha
            int bpp = GetBytesPerPixel(_format);
            for (int y = 0; y < _height; y++)
            for (int x = 0; x < _width;  x++)
            {
                int off  = y * _stride + x * bpp;
                uint pix = ((uint)_pixels[off + 3] << 24) | ((uint)_pixels[off + 2] << 16)
                          | ((uint)_pixels[off + 1] <<  8) |  _pixels[off];
                uint cmp = pix | 0xFF000000u;
                if (cmp == target)
                    _pixels[off + 3] = 0; // set alpha to 0
            }
        }

        // ── Clone ────────────────────────────────────────────────────────────────

        /// <summary>
        /// Creates an exact copy of this <see cref="Bitmap"/>.
        /// </summary>
        /// <returns>A new <see cref="Bitmap"/> with the same pixel data.</returns>
        public override Image Clone() => Clone(new Rectangle(0, 0, _width, _height), _format);

        /// <summary>
        /// Creates a copy of the specified region of this <see cref="Bitmap"/> in the specified format.
        /// </summary>
        /// <param name="rect">The region to clone.</param>
        /// <param name="format">The pixel format for the cloned bitmap.</param>
        /// <returns>The cloned bitmap.</returns>
        public Bitmap Clone(Rectangle rect, PixelFormat format)
        {
            ValidateRect(rect);
            var dst = new Bitmap(rect.Width, rect.Height, format);
            dst._dpiX = _dpiX;
            dst._dpiY = _dpiY;
            int srcBpp = GetBytesPerPixel(_format);
            int dstBpp = GetBytesPerPixel(format);
            for (int y = 0; y < rect.Height; y++)
            for (int x = 0; x < rect.Width;  x++)
            {
                var c = GetPixel(rect.X + x, rect.Y + y);
                dst.SetPixel(x, y, c);
            }
            return dst;
        }

        // ── RotateFlip ──────────────────────────────────────────────────────────

        /// <summary>
        /// Rotates and flips the bitmap.
        /// </summary>
        /// <param name="rft">The rotation and flip type.</param>
        public override void RotateFlip(RotateFlipType rft)
        {
            int rotations = (int)rft & 3;   // 0–3 × 90° CW
            bool flipX   = ((int)rft & 4) != 0;

            for (int i = 0; i < rotations; i++)
                Rotate90CW();
            if (flipX)
                FlipHorizontal();
        }

        // ── Save ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// Saves this bitmap to the specified file.
        /// </summary>
        /// <param name="filename">The file name to save to.</param>
        public override void Save(string filename)
        {
            var ext = Path.GetExtension(filename)?.ToLowerInvariant() ?? ".bmp";
            ImageFormat fmt;
            switch (ext)
            {
                case ".png":            fmt = ImageFormat.Png;  break;
                case ".jpg":
                case ".jpeg":           fmt = ImageFormat.Jpeg; break;
                case ".gif":            fmt = ImageFormat.Gif;  break;
                case ".tiff":
                case ".tif":            fmt = ImageFormat.Tiff; break;
                default:                fmt = ImageFormat.Bmp;  break;
            }
            Save(filename, fmt);
        }

        /// <summary>
        /// Saves this bitmap to the specified file in the specified format.
        /// </summary>
        /// <param name="filename">The file name to save to.</param>
        /// <param name="format">The format to save in.</param>
        public override void Save(string filename, ImageFormat format)
        {
            using (var fs = File.Open(filename, FileMode.Create, FileAccess.Write))
                Save(fs, format);
        }

        /// <summary>
        /// Saves this bitmap to the specified stream in the specified format.
        /// </summary>
        /// <param name="stream">The stream to save to.</param>
        /// <param name="format">The format to save in.</param>
        public override void Save(Stream stream, ImageFormat format)
        {
            Codecs.CodecRegistry.Encode(this, stream, format ?? ImageFormat.Bmp, null);
        }

        /// <summary>
        /// Saves this bitmap to the specified stream in the specified format with encoder parameters.
        /// </summary>
        /// <param name="stream">The stream to save to.</param>
        /// <param name="format">The format to save in.</param>
        /// <param name="encoderParams">The encoder parameters.</param>
        public override void Save(Stream stream, ImageFormat format, EncoderParameters? encoderParams)
        {
            Codecs.CodecRegistry.Encode(this, stream, format ?? ImageFormat.Bmp, encoderParams);
        }

        // ── GetHbitmap ───────────────────────────────────────────────────────────

        /// <summary>
        /// Returns a handle to this bitmap.
        /// </summary>
        /// <returns>A handle to the bitmap.</returns>
        /// <exception cref="PlatformNotSupportedException">This method is not supported on this platform.</exception>
        public IntPtr GetHbitmap()
            => throw new PlatformNotSupportedException("GDI HBITMAP is not supported on this platform.");

        /// <summary>
        /// Returns a handle to this bitmap with the specified background color.
        /// </summary>
        /// <param name="background">The background color.</param>
        /// <returns>A handle to the bitmap.</returns>
        /// <exception cref="PlatformNotSupportedException">This method is not supported on this platform.</exception>
        public IntPtr GetHbitmap(Color background)
            => GetHbitmap();

        // ── Static factory ───────────────────────────────────────────────────────

        /// <summary>
        /// Creates a <see cref="Bitmap"/> from the specified file.
        /// </summary>
        /// <param name="filename">The file to load from.</param>
        /// <returns>The loaded bitmap.</returns>
        public static new Bitmap FromFile(string filename)
            => new Bitmap(filename);

        /// <summary>
        /// Creates a <see cref="Bitmap"/> from the specified stream.
        /// </summary>
        /// <param name="stream">The stream to load from.</param>
        /// <returns>The loaded bitmap.</returns>
        public static new Bitmap FromStream(Stream stream)
            => new Bitmap(stream);

        // ── IDisposable ──────────────────────────────────────────────────────────

        /// <summary>
        /// Releases the unmanaged resources used by this bitmap and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">
        /// <c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.
        /// </param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && _locked && _lockHandle.IsAllocated)
                _lockHandle.Free();
            base.Dispose(disposing);
        }

        // ═══════════════════════════════════════════════════════════════════════
        // Private helpers
        // ═══════════════════════════════════════════════════════════════════════

        internal static int CalcStride(int width, PixelFormat format)
        {
            int bits = GetBitsPerPixel(format);
            return ((width * bits + 31) / 32) * 4;
        }

        private void CopyLoaded(Bitmap loaded)
        {
            _width     = loaded._width;
            _height    = loaded._height;
            _stride    = loaded._stride;
            _format    = loaded._format;
            _pixels    = loaded._pixels;
            _dpiX      = loaded._dpiX;
            _dpiY      = loaded._dpiY;
            _rawFormat = loaded._rawFormat;

            if (loaded._frames != null)
            {
                _frames        = loaded._frames;
                _animator      = loaded._animator;
                _loopCount     = loaded._loopCount;
                _defaultPixels = loaded._defaultPixels;
                _activeFrame   = loaded._activeFrame;
            }
        }

        internal static int GetBitsPerPixel(PixelFormat format)
        {
            switch (format)
            {
                case PixelFormat.Format1bppIndexed:                    return  1;
                case PixelFormat.Format4bppIndexed:                    return  4;
                case PixelFormat.Format8bppIndexed:                    return  8;
                case PixelFormat.Format16bppRgb555:
                case PixelFormat.Format16bppRgb565:
                case PixelFormat.Format16bppArgb1555:
                case PixelFormat.Format16bppGrayScale:                 return 16;
                case PixelFormat.Format24bppRgb:                       return 24;
                case PixelFormat.Format32bppRgb:
                case PixelFormat.Format32bppArgb:
                case PixelFormat.Format32bppPArgb:                     return 32;
                case PixelFormat.Format48bppRgb:                       return 48;
                case PixelFormat.Format64bppArgb:
                case PixelFormat.Format64bppPArgb:                     return 64;
                default:                                               return 32;
            }
        }

        internal static int GetBytesPerPixel(PixelFormat format)
        {
            int bits = GetBitsPerPixel(format);
            return Math.Max(1, (bits + 7) / 8);
        }

        private void ValidateRect(Rectangle rect)
        {
            if (rect.X < 0 || rect.Y < 0
                || rect.Right  > _width
                || rect.Bottom > _height)
                throw new ArgumentOutOfRangeException(nameof(rect));
        }

        private void EnsureAlpha()
        {
            if (_format != PixelFormat.Format32bppArgb &&
                _format != PixelFormat.Format32bppPArgb)
            {
                // Convert to 32bppArgb
                int newStride = CalcStride(_width, PixelFormat.Format32bppArgb);
                var newPixels = new byte[_height * newStride];
                for (int y = 0; y < _height; y++)
                for (int x = 0; x < _width;  x++)
                {
                    var c = GetPixel(x, y);
                    int off = y * newStride + x * 4;
                    newPixels[off]     = c.B;
                    newPixels[off + 1] = c.G;
                    newPixels[off + 2] = c.R;
                    newPixels[off + 3] = c.A;
                }
                _pixels = newPixels;
                _stride = newStride;
                _format = PixelFormat.Format32bppArgb;
            }
        }

        private void CopyFrom(Bitmap src)
        {
            for (int y = 0; y < Math.Min(_height, src._height); y++)
            for (int x = 0; x < Math.Min(_width,  src._width);  x++)
                SetPixel(x, y, src.GetPixel(x, y));
        }

        private byte[] ConvertRegion(Rectangle rect, PixelFormat format, out int stride)
        {
            stride = CalcStride(rect.Width, format);
            var buf = new byte[rect.Height * stride];
            int dstBpp = GetBytesPerPixel(format);
            for (int y = 0; y < rect.Height; y++)
            for (int x = 0; x < rect.Width;  x++)
            {
                var c = GetPixel(rect.X + x, rect.Y + y);
                int off = y * stride + x * dstBpp;
                if (dstBpp >= 4) { buf[off]=c.B; buf[off+1]=c.G; buf[off+2]=c.R; buf[off+3]=c.A; }
                else if (dstBpp == 3) { buf[off]=c.B; buf[off+1]=c.G; buf[off+2]=c.R; }
            }
            return buf;
        }

        // ── Rotate 90° clockwise ─────────────────────────────────────────────────

        private void Rotate90CW()
        {
            // new(nx, ny) = old(H-1-ny, nx)  where newW=H, newH=W
            int newW = _height, newH = _width;
            int newStride = CalcStride(newW, _format);
            var newPixels = new byte[newH * newStride];
            int bpp = GetBytesPerPixel(_format);

            for (int y = 0; y < _height; y++)
            for (int x = 0; x < _width;  x++)
            {
                int nx = _height - 1 - y;   // new x
                int ny = x;                  // new y
                int srcOff = y * _stride   + x  * bpp;
                int dstOff = ny * newStride + nx * bpp;
                for (int b = 0; b < bpp; b++)
                    newPixels[dstOff + b] = _pixels[srcOff + b];
            }

            _pixels = newPixels;
            _stride = newStride;
            (_width, _height) = (newW, newH);
        }

        // ── Flip horizontally (mirror left-right) ────────────────────────────────

        private void FlipHorizontal()
        {
            int bpp = GetBytesPerPixel(_format);
            for (int y = 0; y < _height; y++)
            for (int x = 0; x < _width / 2; x++)
            {
                int x2     = _width - 1 - x;
                int srcOff = y * _stride + x  * bpp;
                int dstOff = y * _stride + x2 * bpp;
                for (int b = 0; b < bpp; b++)
                {
                    byte tmp          = _pixels[srcOff + b];
                    _pixels[srcOff + b] = _pixels[dstOff + b];
                    _pixels[dstOff + b] = tmp;
                }
            }
        }
    }
}
