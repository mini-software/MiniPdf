using MiniSoftware.Drawing.Font.IO;
using System.IO;

namespace MiniSoftware.Drawing.Font.Ttf.Tables
{
    /// <summary>
    /// CMap format 12: segmented coverage mapping for the full Unicode range (U+0000–U+10FFFF).
    /// Each group specifies a contiguous run of code points that maps linearly to a
    /// contiguous run of glyph indices.
    /// Ref: OpenType spec §5.6.1.1.12.
    /// </summary>
    public sealed class TtfCMapFormat12Table : TtfCMapFormatBaseTable
    {
        private readonly int    _groupCount;
        private readonly uint[] _startCharCode;
        private readonly uint[] _endCharCode;
        private readonly uint[] _startGlyphId;

        private TtfCMapFormat12Table(
            ushort platformId, ushort encodingId,
            int groupCount,
            uint[] startCharCode, uint[] endCharCode, uint[] startGlyphId)
            : base(platformId, encodingId, format: 12)
        {
            _groupCount    = groupCount;
            _startCharCode = startCharCode;
            _endCharCode   = endCharCode;
            _startGlyphId  = startGlyphId;
        }

        // ── Factory ───────────────────────────────────────────────────────────

        /// <param name="cmapBytes">Full "cmap" table raw bytes.</param>
        /// <param name="subtableOffset">Byte offset of this subtable within <paramref name="cmapBytes"/>.</param>
        internal static TtfCMapFormat12Table? Parse(
            byte[] cmapBytes, int subtableOffset,
            ushort platformId, ushort encodingId)
        {
            using var reader = new FontBinaryReader(new MemoryStream(cmapBytes));
            reader.Seek(subtableOffset);

            reader.Skip(2); // format     (= 12)
            reader.Skip(2); // reserved   (= 0)
            reader.Skip(4); // length     (uint32)
            reader.Skip(4); // language   (uint32)

            uint numGroups = reader.ReadUInt32();
            if (numGroups == 0) return null;

            int count = (int)numGroups;
            var startCharCode = new uint[count];
            var endCharCode   = new uint[count];
            var startGlyphId  = new uint[count];

            for (int i = 0; i < count; i++)
            {
                startCharCode[i] = reader.ReadUInt32();
                endCharCode[i]   = reader.ReadUInt32();
                startGlyphId[i]  = reader.ReadUInt32();
            }

            return new TtfCMapFormat12Table(
                platformId, encodingId, count,
                startCharCode, endCharCode, startGlyphId);
        }

        // ── Lookup ────────────────────────────────────────────────────────────

        /// <inheritdoc/>
        public override uint GetGlyphIndex(uint codePoint)
        {
            // Binary search through groups.
            int lo = 0, hi = _groupCount - 1;
            while (lo <= hi)
            {
                int mid = (lo + hi) >> 1;
                if (codePoint < _startCharCode[mid])
                    hi = mid - 1;
                else if (codePoint > _endCharCode[mid])
                    lo = mid + 1;
                else
                    return _startGlyphId[mid] + (codePoint - _startCharCode[mid]);
            }
            return 0;
        }
    }
}
