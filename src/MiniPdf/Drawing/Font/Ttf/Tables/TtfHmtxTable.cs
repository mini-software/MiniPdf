using MiniPdf.Drawing.Font.IO;
using System;
using System.IO;

namespace MiniPdf.Drawing.Font.Ttf.Tables
{
    /// <summary>
    /// Parsed OpenType "hmtx" table (horizontal metrics).
    /// Stores advance width and left side-bearing for every glyph.
    /// The first <c>numberOfHMetrics</c> glyphs each have their own advance width;
    /// glyphs beyond that index share the last advance width value.
    /// Ref: OpenType spec §5.2.4.
    /// </summary>
    public sealed class TtfHmtxTable : TtfTableBase
    {
        /// <summary>
        /// The table tag for the hmtx table.
        /// </summary>
        public const string TableTag = "hmtx";

        // ── Private storage ───────────────────────────────────────────────────

        // Advance widths for glyphs 0 .. numberOfHMetrics−1.
        private readonly ushort[] _advanceWidths;

        // Left side-bearings for glyphs 0 .. numberOfHMetrics−1.
        private readonly short[] _lsbs;

        // Left side-bearings for glyphs numberOfHMetrics .. numGlyphs−1
        // (these share the last advance width entry above).
        private readonly short[] _extraLsbs;

        // ── Constructor ───────────────────────────────────────────────────────

        private TtfHmtxTable(
            byte[] rawBytes,
            ushort[] advanceWidths,
            short[] lsbs,
            short[] extraLsbs)
            : base(TableTag, rawBytes)
        {
            _advanceWidths = advanceWidths;
            _lsbs          = lsbs;
            _extraLsbs     = extraLsbs;
        }

        // ── Factory ───────────────────────────────────────────────────────────

        /// <summary>
        /// Parses the "hmtx" table from its raw bytes.
        /// </summary>
        /// <param name="rawBytes">Raw table data.</param>
        /// <param name="numberOfHMetrics">
        /// Value from <see cref="TtfHheaTable.NumberOfHMetrics"/>.
        /// Determines how many full (width+lsb) entries are present.
        /// </param>
        /// <param name="numGlyphs">
        /// Total glyph count from <see cref="TtfMaxpTable.NumGlyphs"/>.
        /// Determines how many extra LSB-only entries follow.
        /// </param>
        public static TtfHmtxTable Parse(byte[] rawBytes, ushort numberOfHMetrics, ushort numGlyphs)
        {
            using var reader = new FontBinaryReader(new MemoryStream(rawBytes));

            var advanceWidths = new ushort[numberOfHMetrics];
            var lsbs          = new short[numberOfHMetrics];

            for (int i = 0; i < numberOfHMetrics; i++)
            {
                advanceWidths[i] = reader.ReadUInt16();
                lsbs[i]          = reader.ReadInt16();
            }

            // Glyphs beyond numberOfHMetrics share the last advance width and
            // have individual LSB entries only.
            int extraCount = numGlyphs > numberOfHMetrics
                ? numGlyphs - numberOfHMetrics
                : 0;

            var extraLsbs = new short[extraCount];
            for (int i = 0; i < extraCount; i++)
                extraLsbs[i] = reader.ReadInt16();

            return new TtfHmtxTable(rawBytes, advanceWidths, lsbs, extraLsbs);
        }

        // ── Public accessors ──────────────────────────────────────────────────

        /// <summary>
        /// Returns the advance width for <paramref name="glyphIndex"/>, in design units.
        /// Glyphs with index ≥ numberOfHMetrics return the last recorded advance width.
        /// </summary>
        public ushort GetAdvanceWidth(int glyphIndex)
        {
            if (glyphIndex < 0 || _advanceWidths.Length == 0) return 0;
            int idx = Math.Min(glyphIndex, _advanceWidths.Length - 1);
            return _advanceWidths[idx];
        }

        /// <summary>
        /// Returns the left side-bearing for <paramref name="glyphIndex"/>, in design units.
        /// </summary>
        public short GetLsb(int glyphIndex)
        {
            if (glyphIndex < 0) return 0;

            if (glyphIndex < _lsbs.Length)
                return _lsbs[glyphIndex];

            int extraIndex = glyphIndex - _lsbs.Length;
            if (extraIndex < _extraLsbs.Length)
                return _extraLsbs[extraIndex];

            return 0;
        }
    }
}
