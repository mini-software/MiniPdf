using System;
using System.IO;

namespace MiniPdf.Drawing.Font.Sources
{
    /// <summary>
    /// Reads a font from a byte array already held in memory.
    /// </summary>
    public sealed class ByteContentStreamSource : IStreamSource
    {
        private readonly byte[] _data;

        /// <summary>
        /// Initializes a new <see cref="ByteContentStreamSource"/> with the specified byte array.
        /// </summary>
        /// <param name="data">The byte array containing font data.</param>
        public ByteContentStreamSource(byte[] data)
        {
            _data = data ?? throw new ArgumentNullException(nameof(data));
        }

        /// <inheritdoc/>
        /// <remarks>Returns a new <see cref="MemoryStream"/> each time; the caller must dispose it.</remarks>
        public Stream GetFontStream() => new MemoryStream(_data, writable: false);
    }
}
