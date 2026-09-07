using System;
using System.Collections.Generic;
using System.IO;
using MiniSoftware.Drawing.Font.Core;
using MiniSoftware.Drawing.Font.Ttf;

namespace MiniSoftware.Drawing.Font.IO
{
    /// <summary>
    /// Parses a TrueType Collection (TTC) file and returns all contained font faces.
    /// </summary>
    /// <remarks>
    /// TTC header layout (version 1.0 and 2.0):
    /// <list type="table">
    ///   <item><term>Offset  0</term><description>ttcTag  — 4 bytes, must be 'ttcf' (0x74746366)</description></item>
    ///   <item><term>Offset  4</term><description>version — 4 bytes (0x00010000 or 0x00020000)</description></item>
    ///   <item><term>Offset  8</term><description>numFonts — 4 bytes</description></item>
    ///   <item><term>Offset 12</term><description>offsetTable[numFonts] — 4 bytes each, absolute offsets to sfnt headers</description></item>
    /// </list>
    /// Version 2.0 appends three extra DSIG fields after the offset array; they are
    /// ignored here (we only need the sfnt offsets).
    ///
    /// All table-record offsets inside each embedded sfnt are absolute offsets from
    /// the start of the TTC file, so the same seekable stream can be shared across
    /// all font reads.
    ///
    /// Specification: https://learn.microsoft.com/en-us/typography/opentype/spec/otff#ttc-header
    /// </remarks>
    internal static class TtcReader
    {
        /// <summary>TTC magic bytes 't','t','c','f' = 0x74746366.</summary>
        internal const uint Signature = 0x74746366u;

        // ── Public surface ────────────────────────────────────────────────────

        /// <summary>
        /// Reads all font faces from a TTC stream.
        /// </summary>
        /// <param name="stream">A seekable stream positioned at the start of the TTC data.</param>
        /// <returns>
        /// A list containing one <see cref="IFont"/> per face in the collection.
        /// The list contains at least one entry.
        /// </returns>
        /// <exception cref="ArgumentNullException">stream is null.</exception>
        /// <exception cref="InvalidOperationException">
        /// The data is not a valid TTC file, or a contained sfnt cannot be parsed.
        /// </exception>
        internal static IReadOnlyList<IFont> Read(Stream stream)
        {
            if (stream is null) throw new ArgumentNullException(nameof(stream));

            // Buffer the entire file so we can seek freely while reading multiple fonts.
            byte[] data = ReadAllBytes(stream);

            if (data.Length < 12)
                throw new InvalidOperationException(
                    "Data is too short to be a TTC file (minimum 12 bytes for header).");

            // ── TTC header ────────────────────────────────────────────────────
            uint sig = ReadU32(data, 0);
            if (sig != Signature)
                throw new InvalidOperationException(
                    $"Not a TTC file (signature 0x{sig:X8}, expected 0x74746366 'ttcf').");

            /* version */   ReadU32(data, 4);   // 0x00010000 or 0x00020000 — not needed
            uint numFonts = ReadU32(data, 8);

            if (numFonts == 0)
                throw new InvalidOperationException("TTC file reports zero fonts.");

            int minHeaderSize = 12 + (int)numFonts * 4;
            if (data.Length < minHeaderSize)
                throw new InvalidOperationException(
                    $"TTC file is too short to hold the offset table " +
                    $"(numFonts={numFonts}, need {minHeaderSize} bytes, have {data.Length}).");

            // ── Read sfnt offset array ────────────────────────────────────────
            var offsets = new uint[numFonts];
            for (int i = 0; i < (int)numFonts; i++)
                offsets[i] = ReadU32(data, 12 + i * 4);

            // ── Parse each embedded sfnt ──────────────────────────────────────
            // All table-data offsets inside each sfnt are absolute positions within
            // the TTC file, so we share one MemoryStream across all font reads.
            using var ms    = new MemoryStream(data, writable: false);
            var fonts       = new List<IFont>((int)numFonts);

            for (int i = 0; i < (int)numFonts; i++)
            {
                uint sfntOffset = offsets[i];

                if (sfntOffset >= data.Length)
                    throw new InvalidOperationException(
                        $"TTC font[{i}] sfnt offset 0x{sfntOffset:X} is beyond the end of the file.");

                ms.Position = sfntOffset;
                fonts.Add(TtfFontReader.Read(ms));
            }

            return fonts;
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private static uint ReadU32(byte[] d, int offset) =>
            ((uint)d[offset]     << 24) | ((uint)d[offset + 1] << 16) |
            ((uint)d[offset + 2] <<  8) |         d[offset + 3];

        private static byte[] ReadAllBytes(Stream stream)
        {
            using var ms = new MemoryStream();
            stream.CopyTo(ms);
            return ms.ToArray();
        }
    }
}
