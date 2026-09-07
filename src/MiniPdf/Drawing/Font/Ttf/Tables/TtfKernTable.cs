using MiniPdf.Drawing.Font.IO;
using System;
using System.Collections.Generic;
using System.IO;

namespace MiniPdf.Drawing.Font.Ttf.Tables
{
    /// <summary>
    /// Parsed OpenType "kern" table — horizontal kerning adjustments between glyph pairs.
    /// </summary>
    /// <remarks>
    /// Only OpenType kern version 0, format 0 (ordered pairs) subtables are loaded.
    /// Format 2 (class-pair) subtables and Apple kern version 1 (AAT) tables are
    /// silently skipped; <see cref="PairCount"/> will be zero for those fonts.
    ///
    /// Ref: https://learn.microsoft.com/en-us/typography/opentype/spec/kern
    /// </remarks>
    public sealed class TtfKernTable : TtfTableBase
    {
        /// <summary>
        /// The table tag for the kern table.
        /// </summary>
        public const string TableTag = "kern";

        // Key = (leftGlyphId << 16) | rightGlyphId; Value = kern adjustment (FUnits, signed).
        private readonly Dictionary<uint, short> _pairs;

        private TtfKernTable(byte[] rawBytes, Dictionary<uint, short> pairs)
            : base(TableTag, rawBytes)
        {
            _pairs = pairs;
        }

        // ── Public API ────────────────────────────────────────────────────────

        /// <summary>Total number of kerning pairs loaded from format 0 subtables.</summary>
        public int PairCount => _pairs.Count;

        /// <summary>
        /// Returns the kerning value (in design units) for the given glyph pair,
        /// or 0 if no kern value is defined for the pair.
        /// A negative value means the glyphs are moved closer together.
        /// </summary>
        public short GetKerning(ushort leftGlyphId, ushort rightGlyphId)
        {
            uint key = ((uint)leftGlyphId << 16) | rightGlyphId;
            return _pairs.TryGetValue(key, out short value) ? value : (short)0;
        }

        // ── Parser ────────────────────────────────────────────────────────────

        /// <summary>Parses the "kern" table from its raw bytes.</summary>
        public static TtfKernTable Parse(byte[] rawBytes)
        {
            if (rawBytes is null) throw new ArgumentNullException(nameof(rawBytes));

            var pairs = new Dictionary<uint, short>();

            if (rawBytes.Length < 4)
                return new TtfKernTable(rawBytes, pairs);

            using var reader = new FontBinaryReader(new MemoryStream(rawBytes));

            // ── Version detection ─────────────────────────────────────────────
            // OpenType kern v0: first uint16 = 0.
            // Apple kern v1   : first uint16 = 1 (full uint32 = 0x00010000).
            ushort version = reader.ReadUInt16();
            if (version == 1)
            {
                // Apple AAT kern — not parsed; return empty table.
                return new TtfKernTable(rawBytes, pairs);
            }
            if (version != 0)
                return new TtfKernTable(rawBytes, pairs);

            ushort nTables = reader.ReadUInt16();

            for (int t = 0; t < nTables; t++)
            {
                long subtableStart = reader.Position;

                if (subtableStart + 6 > rawBytes.Length) break;

                /* subtable version (always 0 in v0) */ reader.ReadUInt16();
                ushort length   = reader.ReadUInt16();  // total subtable length incl. header
                ushort coverage = reader.ReadUInt16();

                // coverage high byte = format, low byte = flags
                int  format       = (coverage >> 8) & 0xFF;
                bool isHorizontal = (coverage & 0x01) != 0;

                if (format == 0 && isHorizontal)
                {
                    if (reader.Position + 8 > rawBytes.Length) break;

                    ushort nPairs      = reader.ReadUInt16();
                    /* searchRange */   reader.ReadUInt16();
                    /* entrySelector */ reader.ReadUInt16();
                    /* rangeShift */    reader.ReadUInt16();

                    for (int p = 0; p < nPairs; p++)
                    {
                        if (reader.Position + 6 > rawBytes.Length) break;

                        ushort left  = reader.ReadUInt16();
                        ushort right = reader.ReadUInt16();
                        short  value = reader.ReadInt16();

                        uint key = ((uint)left << 16) | right;
                        pairs[key] = value;
                    }
                }

                // Advance to next subtable (length is from subtableStart, inclusive of header).
                long nextSubtable = subtableStart + length;
                if (nextSubtable <= subtableStart) break; // safety: corrupt length
                if (nextSubtable > rawBytes.Length) break;
                reader.Seek(nextSubtable);
            }

            return new TtfKernTable(rawBytes, pairs);
        }
    }
}
