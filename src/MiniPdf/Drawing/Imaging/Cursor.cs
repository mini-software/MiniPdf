using System;
using System.IO;
using MiniSoftware.Drawing.Codecs;
using MiniSoftware.Drawing.Enums;
using MiniSoftware.Drawing.Geometry;

namespace MiniSoftware.Drawing.Imaging
{
    /// <summary>
    /// Represents a mouse cursor, optionally loaded from a .cur or .ico file.
    /// The hotspot is extracted from CUR directory entries; ICO files default to (0, 0).
    /// </summary>
    public sealed class Cursor : IDisposable
    {
        private Bitmap _bitmap;
        private Point  _hotSpot;
        private bool   _disposed;

        // ── Constructors ─────────────────────────────────────────────────────────

        /// <summary>
        /// Initializes a new <see cref="Cursor"/> from the specified file.
        /// </summary>
        /// <param name="fileName">The path to the cursor file (.cur or .ico).</param>
        public Cursor(string fileName)
        {
            if (fileName == null) throw new ArgumentNullException(nameof(fileName));
            using var fs = File.OpenRead(fileName);
            (_bitmap, _hotSpot) = LoadFromStream(fs);
        }

        /// <summary>
        /// Initializes a new <see cref="Cursor"/> from the specified stream.
        /// </summary>
        /// <param name="stream">The stream containing cursor data.</param>
        public Cursor(Stream stream)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));
            (_bitmap, _hotSpot) = LoadFromStream(stream);
        }

        /// <summary>
        /// Initializes a new <see cref="Cursor"/> with the specified bitmap and hotspot.
        /// </summary>
        /// <param name="bitmap">The bitmap that represents the cursor.</param>
        /// <param name="hotSpot">The hotspot point of the cursor.</param>
        internal Cursor(Bitmap bitmap, Point hotSpot)
        {
            _bitmap  = bitmap  ?? throw new ArgumentNullException(nameof(bitmap));
            _hotSpot = hotSpot;
        }

        // ── Properties ───────────────────────────────────────────────────────────

        /// <summary>
        /// Gets the size of the cursor.
        /// </summary>
        public Size  Size    => new Size(_bitmap.Width, _bitmap.Height);

        /// <summary>
        /// Gets the hotspot point of the cursor.
        /// </summary>
        public Point HotSpot => _hotSpot;

        // Default cursor — initialized before Current so the auto-property is non-null.
        private static readonly Cursor s_default =
            new Cursor(new Bitmap(1, 1, PixelFormat.Format32bppArgb), Point.Empty);

        /// <summary>
        /// Returns a default 1×1 transparent cursor.
        /// Setting the cursor is a no-op in the pure-managed implementation.
        /// </summary>
        public static Cursor Current { get; set; } = s_default;

        // ── Draw ─────────────────────────────────────────────────────────────────

        /// <summary>Draws the cursor image into <paramref name="targetRect"/>.</summary>
        public void Draw(Graphics g, Rectangle targetRect)
        {
            ThrowIfDisposed();
            if (g == null) throw new ArgumentNullException(nameof(g));
            g.DrawImage(_bitmap, targetRect);
        }

        /// <summary>Draws the cursor stretched to fill <paramref name="targetRect"/>.</summary>
        public void DrawStretched(Graphics g, Rectangle targetRect) => Draw(g, targetRect);

        // ── IDisposable ──────────────────────────────────────────────────────────

        /// <summary>
        /// Releases all resources used by this <see cref="Cursor"/>.
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

        private static (Bitmap bitmap, Point hotSpot) LoadFromStream(Stream stream)
        {
            // Buffer the whole stream so we can inspect and optionally patch the header.
            byte[] data;
            if (stream.CanSeek && stream.Length - stream.Position < int.MaxValue)
            {
                int len = (int)(stream.Length - stream.Position);
                data = new byte[len];
                int read = 0;
                while (read < len)
                {
                    int n = stream.Read(data, read, len - read);
                    if (n == 0) break;
                    read += n;
                }
            }
            else
            {
                using var ms = new MemoryStream();
                stream.CopyTo(ms);
                data = ms.ToArray();
            }

            Point hotSpot = Point.Empty;

            // CUR file: ICONDIR type == 2; hotspot in ICONDIRENTRY[0] at offsets 10–13.
            if (data.Length >= 14
                && data[0] == 0 && data[1] == 0
                && data[2] == 2 && data[3] == 0)
            {
                int hx = data[10] | (data[11] << 8);
                int hy = data[12] | (data[13] << 8);
                hotSpot = new Point(hx, hy);

                // Patch type field to 1 (ICO) so the existing IcoCodec can decode it.
                data[2] = 1;
            }

            using var buf = new MemoryStream(data);
            var bitmap = CodecRegistry.Decode(buf);
            return (bitmap, hotSpot);
        }

        private void ThrowIfDisposed()
        {
            if (_disposed) throw new ObjectDisposedException(nameof(Cursor));
        }
    }
}
