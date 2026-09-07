using System;
using System.Collections.Generic;
using System.IO;
using MiniPdf.Drawing.Font.IO;
using MiniPdf.Drawing.Font.Ttf;

namespace MiniPdf.Drawing.Font.Cff
{
    /// <summary>
    /// Serialises a <see cref="CffFont"/> back to a binary sfnt/CFF stream.
    /// <para>
    /// For OTF/CFF fonts (loaded from an sfnt container with the "CFF " table)
    /// the sfnt frame is rebuilt from the raw table bytes stored at load time,
    /// with the original "CFF " bytes re-inserted.
    /// </para>
    /// <para>
    /// Standalone raw CFF files are written back as-is without an sfnt wrapper.
    /// </para>
    /// </summary>
    public static class CffFontWriter
    {
        // ─────────────────────────────────────────────────────────────────────
        // Public entry point
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Serialises <paramref name="font"/> and writes the result to
        /// <paramref name="stream"/>.
        /// </summary>
        /// <exception cref="ArgumentNullException">Either argument is null.</exception>
        /// <exception cref="NotSupportedException">
        /// Thrown for standalone (non-sfnt) CFF files where round-trip serialisation
        /// is not yet supported.
        /// </exception>
        public static void Write(CffFont font, Stream stream)
        {
            if (font   == null) throw new ArgumentNullException(nameof(font));
            if (stream == null) throw new ArgumentNullException(nameof(stream));

            byte[] data = WriteToBytes(font);
            stream.Write(data, 0, data.Length);
        }

        /// <summary>
        /// Serialises <paramref name="font"/> and returns the resulting bytes.
        /// </summary>
        public static byte[] WriteToBytes(CffFont font)
        {
            if (font == null) throw new ArgumentNullException(nameof(font));

            if (font.SfntTables == null || font.RawCffBytes == null)
                throw new NotSupportedException(
                    "Serialisation of standalone (non-sfnt) CFF files is not supported.");

            // Collect the sfnt tables (head, hhea, maxp, etc.) and add the CFF table.
            var tables = TtfFontWriter.CollectTables(font.SfntTables);
            tables.Add(("CFF ", font.RawCffBytes));

            // Re-sort (CFF must fit into the canonical tag order).
            tables.Sort((a, b) => string.Compare(a.Tag, b.Tag, StringComparison.Ordinal));

            return TtfFontWriter.BuildSfnt(OpenTypeOffsetTable.SfVersionCff, tables);
        }
    }
}
