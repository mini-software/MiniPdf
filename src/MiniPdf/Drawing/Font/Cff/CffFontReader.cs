using System;
using System.IO;
using MiniSoftware.Drawing.Font.IO;
using MiniSoftware.Drawing.Font.Ttf;
using MiniSoftware.Drawing.Font.Ttf.Tables;

namespace MiniSoftware.Drawing.Font.Cff
{
    /// <summary>
    /// Reads a CFF font from a stream and returns a <see cref="CffFont"/>.
    /// Auto-detects OTF/CFF sfnt containers (magic <c>OTTO</c>) from standalone
    /// raw CFF files.
    /// </summary>
    public static class CffFontReader
    {
        private const uint OttoMagic = 0x4F54544F; // "OTTO"

        /// <summary>
        /// Parses the CFF font data from <paramref name="stream"/> and returns a
        /// <see cref="CffFont"/>.
        /// </summary>
        /// <remarks>
        /// The stream must be readable and seekable. It is not disposed by this reader.
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="stream"/> is null.</exception>
        /// <exception cref="InvalidOperationException">
        /// The data cannot be parsed as a valid CFF or OTF/CFF font.
        /// </exception>
        public static CffFont Read(Stream stream)
        {
            if (stream is null) throw new ArgumentNullException(nameof(stream));

            // Peek at the first four bytes to distinguish OTF sfnt from raw CFF.
            using var reader = new FontBinaryReader(stream, leaveOpen: true);
            uint magic = reader.ReadUInt32();
            reader.Seek(0);

            return magic == OttoMagic
                ? ReadFromSfnt(reader)
                : ReadStandaloneCff(stream);
        }

        // ── OTF/CFF sfnt container ────────────────────────────────────────────

        private static CffFont ReadFromSfnt(FontBinaryReader reader)
        {
            reader.Seek(0);

            var (_, records) = OpenTypeTableDirectory.Parse(reader);
            var rawTables    = OpenTypeTableDirectory.ReadAllTableBytes(reader, records);

            // Parse optional sfnt metadata tables (same pattern as TtfFontReader).
            TtfHeadTable? head = TryParse(rawTables, "head", b => TtfHeadTable.Parse(b));
            TtfHheaTable? hhea = TryParse(rawTables, "hhea", b => TtfHheaTable.Parse(b));
            TtfMaxpTable? maxp = TryParse(rawTables, "maxp", b => TtfMaxpTable.Parse(b));
            TtfNameTable? name = TryParse(rawTables, "name", b => TtfNameTable.Parse(b));

            TtfHmtxTable? hmtx = null;
            if (hhea != null && maxp != null && rawTables.ContainsKey("hmtx"))
                hmtx = TryParse(rawTables, "hmtx",
                    b => TtfHmtxTable.Parse(b, hhea.NumberOfHMetrics, maxp.NumGlyphs));

            TtfOs2Table?  os2  = TryParse(rawTables, "OS/2", b => TtfOs2Table.Parse(b));
            TtfPostTable? post = TryParse(rawTables, "post", b => TtfPostTable.Parse(b));
            TtfCMapTable? cmap = TryParse(rawTables, "cmap", b => TtfCMapTable.Parse(b));

            // Assemble sfnt tables (loca/glyf absent in CFF-flavoured OTF)
            TtfTables? sfnt = null;
            if (head != null && hhea != null && maxp != null && name != null && hmtx != null)
                sfnt = new TtfTables(head, hhea, maxp, name, hmtx,
                                     os2, post, loca: null, glyfTable: null, cmapTable: cmap);

            // Extract and parse the "CFF " table
            if (!rawTables.TryGetValue("CFF ", out byte[]? cffBytes))
                throw new InvalidOperationException(
                    "OTF font has an 'OTTO' magic but no 'CFF ' table.");

            var cffData = CffParser.Parse(cffBytes);

            // Patch font name from sfnt name table when available
            if (sfnt != null)
            {
                if (cffData.FullName is null)   cffData.FullName   = sfnt.Name.FullName;
                if (cffData.FamilyName is null) cffData.FamilyName = sfnt.Name.FamilyName;
                if (string.IsNullOrEmpty(cffData.FontName))
                    cffData.FontName = sfnt.Name.PostScriptName ?? sfnt.Name.FullName ?? string.Empty;
            }

            return new CffFont(cffData, sfnt, cffBytes);
        }

        // ── Standalone CFF ────────────────────────────────────────────────────

        private static CffFont ReadStandaloneCff(Stream stream)
        {
            byte[] cffBytes = ReadAllBytes(stream);
            var cffData = CffParser.Parse(cffBytes);
            return new CffFont(cffData, sfntTables: null);
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private static byte[] ReadAllBytes(Stream stream)
        {
            if (stream.CanSeek)
            {
                long rem = stream.Length - stream.Position;
                if (rem < 0) rem = 0;
                byte[] buf = new byte[rem];
                int read = 0;
                while (read < buf.Length)
                {
                    int n = stream.Read(buf, read, buf.Length - read);
                    if (n == 0) break;
                    read += n;
                }
                return buf;
            }
            // Non-seekable fall-through
            using var ms = new MemoryStream();
            stream.CopyTo(ms);
            return ms.ToArray();
        }

        private static T? TryParse<T>(
            System.Collections.Generic.IReadOnlyDictionary<string, byte[]> rawTables,
            string tag,
            Func<byte[], T> parser)
            where T : class
        {
            if (!rawTables.TryGetValue(tag, out byte[]? raw)) return null;
            try   { return parser(raw); }
            catch { return null; }
        }
    }
}
