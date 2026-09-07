using System;
using System.Collections.Generic;
using System.IO;
using MiniPdf.Drawing.Font.IO;
using MiniPdf.Drawing.Font.Ttf.Tables;

namespace MiniPdf.Drawing.Font.Ttf
{
    /// <summary>
    /// Reads a TrueType or OpenType/TT font from a stream and returns a fully
    /// parsed <see cref="TtfFont"/>.
    /// </summary>
    /// <remarks>
    /// The stream is read sequentially from its current position; it must be
    /// seekable because individual table bytes are read by absolute offset.
    /// The stream is <em>not</em> disposed by this reader.
    /// </remarks>
    public static class TtfFontReader
    {
        /// <summary>
        /// Parses the sfnt data from <paramref name="stream"/> and returns a
        /// <see cref="TtfFont"/>.
        /// </summary>
        /// <exception cref="ArgumentNullException">stream is null.</exception>
        /// <exception cref="InvalidOperationException">
        /// The stream does not start with a recognised sfnt magic value, or a
        /// required table is missing.
        /// </exception>
        public static TtfFont Read(Stream stream)
        {
            if (stream is null) throw new ArgumentNullException(nameof(stream));

            using var reader = new FontBinaryReader(stream, leaveOpen: true);

            // ── Parse offset table + table directory ──────────────────────────
            var (header, records) = OpenTypeTableDirectory.Parse(reader);

            // ── Read all raw table bytes keyed by tag ─────────────────────────
            var rawTables = OpenTypeTableDirectory.ReadAllTableBytes(reader, records);

            // ── Required tables ───────────────────────────────────────────────
            TtfHeadTable head = ParseRequired<TtfHeadTable>(rawTables, "head",
                b => TtfHeadTable.Parse(b));

            TtfHheaTable hhea = ParseRequired<TtfHheaTable>(rawTables, "hhea",
                b => TtfHheaTable.Parse(b));

            TtfMaxpTable maxp = ParseRequired<TtfMaxpTable>(rawTables, "maxp",
                b => TtfMaxpTable.Parse(b));

            TtfNameTable name = ParseRequired<TtfNameTable>(rawTables, "name",
                b => TtfNameTable.Parse(b));

            TtfHmtxTable hmtx = ParseRequired<TtfHmtxTable>(rawTables, "hmtx",
                b => TtfHmtxTable.Parse(b, hhea.NumberOfHMetrics, maxp.NumGlyphs));

            // ── Optional tables ───────────────────────────────────────────────
            TtfOs2Table?  os2  = ParseOptional(rawTables, "OS/2",
                b => TtfOs2Table.Parse(b));

            TtfPostTable? post = ParseOptional(rawTables, "post",
                b => TtfPostTable.Parse(b));

            TtfCMapTable? cmap = ParseOptional(rawTables, "cmap",
                b => TtfCMapTable.Parse(b));

            // "loca" and "glyf" are only present in TrueType-outline fonts.
            TtfLocaTable? loca = null;
            if (rawTables.ContainsKey("loca"))
            {
                loca = TtfLocaTable.Parse(
                    rawTables["loca"],
                    head.IndexToLocFormat,
                    (ushort)maxp.NumGlyphs);
            }

            TtfGlyfTable? glyf = null;
            if (rawTables.ContainsKey("glyf") && loca != null)
            {
                glyf = TtfGlyfTable.Create(rawTables["glyf"], loca);
            }

            // ── Batch 13 optional tables ──────────────────────────────────────
            TtfKernTable? kern = ParseOptional(rawTables, "kern",
                b => TtfKernTable.Parse(b));

            TtfGdefTable? gdef = ParseOptional(rawTables, "GDEF",
                b => TtfGdefTable.Parse(b));

            TtfGsubTable? gsub = ParseOptional(rawTables, "GSUB",
                b => TtfGsubTable.Parse(b));

            TtfGposTable? gpos = ParseOptional(rawTables, "GPOS",
                b => TtfGposTable.Parse(b));
            // ── Batch 14 optional tables ──────────────────────────────────────────────────
            TtfFvarTable? fvar = ParseOptional(rawTables, "fvar",
                b => TtfFvarTable.Parse(b));

            TtfGvarTable? gvar = ParseOptional(rawTables, "gvar",
                b => TtfGvarTable.Parse(b));

            TtfAvarTable? avar = ParseOptional(rawTables, "avar",
                b => TtfAvarTable.Parse(b));

            // ── Batch 15 optional tables ──────────────────────────────────────
            // CBLC and CBDT must be parsed together; CBLC provides the glyph offsets.
            TtfCbdtTable? cbdt = null;
            if (rawTables.TryGetValue("CBLC", out byte[]? cblcBytes) &&
                rawTables.TryGetValue("CBDT", out byte[]? cbdtBytes))
            {
                try
                {
                    var cblc = TtfCblcTable.Parse(cblcBytes!);
                    cbdt = TtfCbdtTable.Parse(cbdtBytes!, cblcBytes!, cblc);
                }
                catch { /* malformed table — leave cbdt null */ }
            }

            TtfSvgTable? svg = ParseOptional(rawTables, "SVG ",
                b => TtfSvgTable.Parse(b));

            // ── Assemble and return ───────────────────────────────────────────
            var tables = new TtfTables(head, hhea, maxp, name, hmtx,
                                       os2, post, loca, glyf, cmap,
                                       kern, gdef, gsub, gpos,
                                       fvar, gvar, avar,
                                       cbdt, svg);
            return new TtfFont(tables);
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private static T ParseRequired<T>(
            IReadOnlyDictionary<string, byte[]> rawTables,
            string tag,
            Func<byte[], T> parser)
        {
            if (!rawTables.TryGetValue(tag, out byte[]? raw))
                throw new InvalidOperationException(
                    $"Required OpenType table '{tag}' is missing from the font.");
            return parser(raw);
        }

        private static T? ParseOptional<T>(
            IReadOnlyDictionary<string, byte[]> rawTables,
            string tag,
            Func<byte[], T> parser)
            where T : class
        {
            if (!rawTables.TryGetValue(tag, out byte[]? raw)) return null;
            try   { return parser(raw); }
            catch { return null; }       // Corrupt optional table — skip gracefully.
        }
    }
}
