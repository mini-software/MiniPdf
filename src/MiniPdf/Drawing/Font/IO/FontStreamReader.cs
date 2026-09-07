using System;
using System.IO;
using MiniSoftware.Drawing.Font.Core;

namespace MiniSoftware.Drawing.Font.IO
{
    /// <summary>
    /// Reads the magic bytes of a font data buffer to identify its format,
    /// decompresses WOFF containers as needed, and returns the raw sfnt bytes
    /// together with the detected <see cref="FontType"/>.
    /// </summary>
    /// <remarks>
    /// Supported magic values:
    /// <list type="table">
    ///   <item><term>0x00010000</term><description>TrueType 1.0</description></item>
    ///   <item><term>0x74727565 ('true')</term><description>Apple TrueType</description></item>
    ///   <item><term>0x4F54544F ('OTTO')</term><description>OpenType/CFF</description></item>
    ///   <item><term>0x74746366 ('ttcf')</term><description>TrueType Collection (Batch 12)</description></item>
    ///   <item><term>0x774F4646 ('wOFF')</term><description>WOFF 1.0 — decompressed inline</description></item>
    ///   <item><term>0x774F4632 ('wOF2')</term><description>WOFF 2.0 — NotSupportedException on netstandard2.0</description></item>
    /// </list>
    /// </remarks>
    internal static class FontStreamReader
    {
        // sfnt magic constants
        private const uint MagicTTF  = 0x00010000u; // TrueType 1.0
        private const uint MagicTRUE = 0x74727565u; // 'true' (Apple TrueType)
        private const uint MagicOTF  = 0x4F54544Fu; // 'OTTO' (OpenType/CFF)
        private const uint MagicTTC  = 0x74746366u; // 'ttcf' (TTC collection)

        // ── Public API ────────────────────────────────────────────────────────

        /// <summary>
        /// Reads <paramref name="stream"/> from its current position, identifies the
        /// font format, decompresses WOFF if needed, and returns the sfnt bytes.
        /// </summary>
        /// <returns>
        /// The raw sfnt bytes (or original bytes for uncompressed formats) and the
        /// detected <see cref="FontType"/>.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// The format magic bytes are not recognised.
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// WOFF2 was detected (requires .NET 6+), or TTC was detected (use
        /// <c>FontFactory.OpenCollection</c> for collections).
        /// </exception>
        internal static (byte[] FontBytes, FontType DetectedType) Detect(Stream stream)
        {
            if (stream is null) throw new ArgumentNullException(nameof(stream));

            using var ms = new MemoryStream();
            stream.CopyTo(ms);
            return Detect(ms.ToArray());
        }

        /// <summary>
        /// Identifies the font format from <paramref name="data"/>, decompresses WOFF
        /// if needed, and returns the raw sfnt bytes.
        /// </summary>
        internal static (byte[] FontBytes, FontType DetectedType) Detect(byte[] data)
        {
            if (data is null) throw new ArgumentNullException(nameof(data));
            if (data.Length < 4)
                throw new InvalidOperationException(
                    "Data is too short to identify font format (need at least 4 bytes).");

            uint magic = ((uint)data[0] << 24) | ((uint)data[1] << 16)
                       | ((uint)data[2] <<  8) |         data[3];

            switch (magic)
            {
                case MagicTTF:
                case MagicTRUE:
                    return (data, FontType.TTF);

                case MagicOTF:
                    return (data, FontType.OTF);

                case MagicTTC:
                    throw new NotSupportedException(
                        "TrueType Collections (TTC) must be opened via FontFactory.OpenCollection, " +
                        "not FontFactory.Open. (Full TTC support arrives in Batch 12.)");

                case WoffReader.Signature:
                {
                    byte[] sfnt = WoffReader.Decompress(data);

                    // Determine inner type from sfnt magic.
                    uint innerMagic = ((uint)sfnt[0] << 24) | ((uint)sfnt[1] << 16)
                                    | ((uint)sfnt[2] <<  8) |         sfnt[3];

                    FontType innerType = innerMagic == MagicOTF ? FontType.OTF : FontType.TTF;
                    return (sfnt, innerType);
                }

                case Woff2Reader.Signature:
                    // Propagate the descriptive NotSupportedException from Woff2Reader.
                    Woff2Reader.Decompress(data);
                    // unreachable — Decompress always throws
                    throw new InvalidOperationException("Unreachable.");

                default:
                    // Adobe Type 1 PFB: first byte 0x80, second byte 0x01
                    if (data[0] == 0x80 && data[1] == 0x01)
                        return (data, FontType.Type1);

                    throw new InvalidOperationException(
                        $"Unrecognised font format (magic bytes 0x{magic:X8}).");
            }
        }
    }
}
