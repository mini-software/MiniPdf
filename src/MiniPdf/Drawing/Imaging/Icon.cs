using System;
using System.IO;
using MiniSoftware.Drawing.Codecs;
using MiniSoftware.Drawing.Geometry;

namespace MiniSoftware.Drawing.Imaging
{
    /// <summary>
    /// Represents a Windows icon (.ico) as a managed <see cref="Bitmap"/>.
    /// Loading is performed by <see cref="IcoCodec"/>; saving writes a single-frame
    /// PNG-compressed ICO container.
    /// </summary>
    public sealed class Icon : IDisposable
    {
        private Bitmap _bitmap;
        private bool   _disposed;

        // ── Internal constructor (SystemIcons stubs) ─────────────────────────────

        internal Icon(Bitmap bitmap)
        {
            _bitmap = bitmap ?? throw new ArgumentNullException(nameof(bitmap));
        }

        // ── Public constructors ──────────────────────────────────────────────────

        /// <summary>
        /// Initializes a new <see cref="Icon"/> from the specified file.
        /// </summary>
        /// <param name="filename">The path to the icon file.</param>
        public Icon(string filename)
        {
            if (filename == null) throw new ArgumentNullException(nameof(filename));
            using var fs = File.OpenRead(filename);
            _bitmap = CodecRegistry.Decode(fs);
        }

        /// <summary>
        /// Initializes a new <see cref="Icon"/> from the specified stream.
        /// </summary>
        /// <param name="stream">The stream containing icon data.</param>
        public Icon(Stream stream)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));
            _bitmap = CodecRegistry.Decode(stream);
        }

        /// <summary>
        /// Initializes a new <see cref="Icon"/> as a copy of the specified icon with the specified dimensions.
        /// </summary>
        /// <param name="original">The original icon to copy.</param>
        /// <param name="width">The width of the new icon.</param>
        /// <param name="height">The height of the new icon.</param>
        public Icon(Icon original, int width, int height)
        {
            if (original == null) throw new ArgumentNullException(nameof(original));
            original.ThrowIfDisposed();
            // Clone the backing bitmap (resize is deferred to post-v1).
            _bitmap = (Bitmap)original._bitmap.Clone();
        }

        /// <summary>
        /// Initializes a new <see cref="Icon"/> as a copy of the specified icon with the specified size.
        /// </summary>
        /// <param name="original">The original icon to copy.</param>
        /// <param name="size">The size of the new icon.</param>
        public Icon(Icon original, Size size) : this(original, size.Width, size.Height) { }

        // ── Properties ───────────────────────────────────────────────────────────

        /// <summary>
        /// Gets the width of the icon.
        /// </summary>
        public int  Width  => _bitmap.Width;

        /// <summary>
        /// Gets the height of the icon.
        /// </summary>
        public int  Height => _bitmap.Height;

        /// <summary>
        /// Gets the size of the icon.
        /// </summary>
        public Size Size   => new Size(Width, Height);

        // ── Static factory ───────────────────────────────────────────────────────

        /// <summary>Not supported on this platform.</summary>
        public static Icon FromHandle(IntPtr handle)
            => throw new PlatformNotSupportedException("Icon.FromHandle is not supported on this platform.");

        /// <summary>Returns null; OS-level association lookup is not available.</summary>
        public static Icon? ExtractAssociatedIcon(string filePath) => null;

        // ── Operations ───────────────────────────────────────────────────────────

        /// <summary>Returns a new <see cref="Bitmap"/> copied from this icon's pixels.</summary>
        public Bitmap ToBitmap()
        {
            ThrowIfDisposed();
            return (Bitmap)_bitmap.Clone();
        }

        /// <summary>
        /// Writes this icon to <paramref name="outputStream"/> as a single-frame
        /// PNG-compressed ICO container.
        /// </summary>
        public void Save(Stream outputStream)
        {
            ThrowIfDisposed();
            if (outputStream == null) throw new ArgumentNullException(nameof(outputStream));

            // Encode the bitmap as PNG first, then wrap it in an ICO container.
            var pngBuf = new MemoryStream();
            _bitmap.Save(pngBuf, ImageFormat.Png);
            byte[] png = pngBuf.ToArray();

            int dataOffset = 6 + 16;   // ICONDIR(6) + one ICONDIRENTRY(16)
            int w = Math.Min(Width,  255);
            int h = Math.Min(Height, 255);

            // ICONDIR (6 bytes)
            outputStream.WriteByte(0); outputStream.WriteByte(0); // reserved
            outputStream.WriteByte(1); outputStream.WriteByte(0); // type = 1 (ICO)
            outputStream.WriteByte(1); outputStream.WriteByte(0); // count = 1

            // ICONDIRENTRY (16 bytes)
            outputStream.WriteByte((byte)(w == 256 ? 0 : w));
            outputStream.WriteByte((byte)(h == 256 ? 0 : h));
            outputStream.WriteByte(0);      // color count
            outputStream.WriteByte(0);      // reserved
            outputStream.WriteByte(1); outputStream.WriteByte(0); // planes
            outputStream.WriteByte(32); outputStream.WriteByte(0); // bpp
            WriteInt32LE(outputStream, png.Length);
            WriteInt32LE(outputStream, dataOffset);

            // PNG payload
            outputStream.Write(png, 0, png.Length);
        }

        // ── IDisposable ──────────────────────────────────────────────────────────

        /// <summary>
        /// Releases all resources used by this <see cref="Icon"/>.
        /// </summary>
        public void Dispose()
        {
            if (!_disposed)
            {
                _bitmap.Dispose();
                _disposed = true;
            }
        }

        // ── Private helpers ──────────────────────────────────────────────────────

        private static void WriteInt32LE(Stream s, int value)
        {
            s.WriteByte((byte)( value         & 0xFF));
            s.WriteByte((byte)((value >>  8)  & 0xFF));
            s.WriteByte((byte)((value >> 16)  & 0xFF));
            s.WriteByte((byte)((value >> 24)  & 0xFF));
        }

        private void ThrowIfDisposed()
        {
            if (_disposed) throw new ObjectDisposedException(nameof(Icon));
        }
    }
}
