using MiniSoftware.Drawing.Font.IO;
using System.IO;

namespace MiniSoftware.Drawing.Font.Ttf.Tables
{
    /// <summary>
    /// Parsed OpenType "loca" table (index to location).
    /// Maps glyph indices to their byte offsets within the "glyf" table.
    /// </summary>
    /// <remarks>
    /// The table stores <c>numGlyphs + 1</c> offsets so the length of any glyph's
    /// data can be derived as <c>loca[i+1] − loca[i]</c>.
    /// A zero-length entry means the glyph has no outline (e.g. the space character).
    ///
    /// Format is controlled by <c>head.indexToLocFormat</c>:
    /// <list type="bullet">
    ///   <item>0 (short): 16-bit values, each multiplied by 2 to get the byte offset.</item>
    ///   <item>1 (long):  32-bit values used as-is.</item>
    /// </list>
    /// Ref: OpenType spec §5.2.5.
    /// </remarks>
    public sealed class TtfLocaTable : TtfTableBase
    {
        /// <summary>
        /// The table tag for the loca table.
        /// </summary>
        public const string TableTag = "loca";

        // ── Private storage ───────────────────────────────────────────────────

        // Absolute byte offsets, length = numGlyphs + 1.
        private readonly uint[] _offsets;

        /// <summary>
        /// The <c>indexToLocFormat</c> value from the "head" table:
        /// 0 = short format, 1 = long format.
        /// </summary>
        public short IndexToLocFormat { get; }

        // ── Constructor ───────────────────────────────────────────────────────

        private TtfLocaTable(byte[] rawBytes, uint[] offsets, short indexToLocFormat)
            : base(TableTag, rawBytes)
        {
            _offsets         = offsets;
            IndexToLocFormat = indexToLocFormat;
        }

        // ── Factory ───────────────────────────────────────────────────────────

        /// <summary>Parses the "loca" table from its raw bytes.</summary>
        /// <param name="rawBytes">Raw table data.</param>
        /// <param name="indexToLocFormat">
        /// Value from <see cref="TtfHeadTable.IndexToLocFormat"/>:
        /// 0 = short (×2), 1 = long.
        /// </param>
        /// <param name="numGlyphs">
        /// Total glyph count from <see cref="TtfMaxpTable.NumGlyphs"/>.
        /// The table will contain <c>numGlyphs + 1</c> offset entries.
        /// </param>
        public static TtfLocaTable Parse(byte[] rawBytes, short indexToLocFormat, ushort numGlyphs)
        {
            using var reader = new FontBinaryReader(new MemoryStream(rawBytes));

            int count = numGlyphs + 1; // n+1 entries to allow length computation
            var offsets = new uint[count];

            if (indexToLocFormat == 0)
            {
                // Short format: 16-bit values scaled by 2.
                for (int i = 0; i < count; i++)
                    offsets[i] = (uint)(reader.ReadUInt16() * 2);
            }
            else
            {
                // Long format: 32-bit values used as-is.
                for (int i = 0; i < count; i++)
                    offsets[i] = reader.ReadUInt32();
            }

            return new TtfLocaTable(rawBytes, offsets, indexToLocFormat);
        }

        // ── Public accessors ──────────────────────────────────────────────────

        /// <summary>
        /// Returns the absolute byte offset of glyph <paramref name="glyphIndex"/>
        /// within the "glyf" table data.
        /// Returns 0 for out-of-range indices.
        /// </summary>
        public uint GetGlyphOffset(int glyphIndex)
        {
            if (glyphIndex < 0 || glyphIndex >= _offsets.Length - 1)
                return 0;
            return _offsets[glyphIndex];
        }

        /// <summary>
        /// Returns the byte length of the glyph data for <paramref name="glyphIndex"/>.
        /// A return value of 0 indicates a glyph with no outline (e.g. space).
        /// </summary>
        public uint GetGlyphLength(int glyphIndex)
        {
            if (glyphIndex < 0 || glyphIndex >= _offsets.Length - 1)
                return 0;
            return _offsets[glyphIndex + 1] - _offsets[glyphIndex];
        }

        /// <summary>Total number of glyph entries (excluding the sentinel).</summary>
        public int GlyphCount => _offsets.Length > 0 ? _offsets.Length - 1 : 0;
    }
}
