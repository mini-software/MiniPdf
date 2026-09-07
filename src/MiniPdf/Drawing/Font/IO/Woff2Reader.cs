using System;
using System.IO;

namespace MiniPdf.Drawing.Font.IO
{
    /// <summary>
    /// WOFF2 container decompression stub.
    /// </summary>
    /// <remarks>
    /// WOFF2 uses Brotli compression, which requires <c>System.IO.Compression.BrotliStream</c>
    /// available on .NET 6 and later.  This library targets <c>netstandard2.0</c>, so WOFF2
    /// decompression is not available; every entry point throws
    /// <see cref="NotSupportedException"/>.
    ///
    /// Specification: https://www.w3.org/TR/WOFF2/
    /// </remarks>
    internal static class Woff2Reader
    {
        /// <summary>WOFF2 magic bytes 'w','O','F','2' = 0x774F4632.</summary>
        internal const uint Signature = 0x774F4632u;

        private const string NotSupportedMessage =
            "WOFF2 decompression requires .NET 6 or later (System.IO.Compression.BrotliStream). " +
            "To load a WOFF2 font on this platform, supply the pre-decompressed sfnt bytes " +
            "via ByteContentStreamSource with FontType.TTF or FontType.OTF instead.";

        /// <summary>
        /// Not supported on netstandard2.0; always throws <see cref="NotSupportedException"/>.
        /// </summary>
        internal static byte[] Decompress(byte[] data)
        {
            _ = data;
            throw new NotSupportedException(NotSupportedMessage);
        }

        /// <summary>
        /// Not supported on netstandard2.0; always throws <see cref="NotSupportedException"/>.
        /// </summary>
        internal static byte[] Decompress(Stream stream)
        {
            _ = stream;
            throw new NotSupportedException(NotSupportedMessage);
        }
    }
}
