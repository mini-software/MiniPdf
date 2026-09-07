using MiniSoftware.Drawing.Font.IO;
using System.IO;

namespace MiniSoftware.Drawing.Font.Ttf.Tables
{
    /// <summary>
    /// CMap format 4: segmented BMP character-to-glyph-index mapping.
    /// Covers Unicode code points in the range U+0000–U+FFFF using a
    /// series of contiguous segments defined by start/end code arrays.
    /// Ref: OpenType spec §5.6.1.1.4.
    /// </summary>
    public sealed class TtfCMapFormat4Table : TtfCMapFormatBaseTable
    {
        private readonly int      _segCount;
        private readonly ushort[] _endCount;
        private readonly ushort[] _startCount;
        private readonly short[]  _idDelta;
        private readonly ushort[] _idRangeOffset;
        private readonly ushort[] _glyphIdArray;

        private TtfCMapFormat4Table(
            ushort platformId, ushort encodingId,
            int segCount,
            ushort[] endCount, ushort[] startCount,
            short[] idDelta, ushort[] idRangeOffset,
            ushort[] glyphIdArray)
            : base(platformId, encodingId, format: 4)
        {
            _segCount      = segCount;
            _endCount      = endCount;
            _startCount    = startCount;
            _idDelta       = idDelta;
            _idRangeOffset = idRangeOffset;
            _glyphIdArray  = glyphIdArray;
        }

        // ── Factory ───────────────────────────────────────────────────────────

        /// <param name="cmapBytes">Full "cmap" table raw bytes.</param>
        /// <param name="subtableOffset">Byte offset of this subtable within <paramref name="cmapBytes"/>.</param>
        internal static TtfCMapFormat4Table? Parse(
            byte[] cmapBytes, int subtableOffset,
            ushort platformId, ushort encodingId)
        {
            using var reader = new FontBinaryReader(new MemoryStream(cmapBytes));
            reader.Seek(subtableOffset);

            reader.Skip(2);                      // format (= 4)
            ushort length     = reader.ReadUInt16();
            reader.Skip(2);                      // language
            ushort segCountX2 = reader.ReadUInt16();
            reader.Skip(6);                      // searchRange, entrySelector, rangeShift

            int segCount = segCountX2 / 2;
            if (segCount <= 0) return null;

            var endCount = new ushort[segCount];
            for (int i = 0; i < segCount; i++)
                endCount[i] = reader.ReadUInt16();

            reader.Skip(2); // reservedPad

            var startCount = new ushort[segCount];
            for (int i = 0; i < segCount; i++)
                startCount[i] = reader.ReadUInt16();

            var idDelta = new short[segCount];
            for (int i = 0; i < segCount; i++)
                idDelta[i] = reader.ReadInt16();

            var idRangeOffset = new ushort[segCount];
            for (int i = 0; i < segCount; i++)
                idRangeOffset[i] = reader.ReadUInt16();

            // glyphIdArray fills the remaining bytes in the subtable.
            // Header bytes consumed: 2(fmt)+2(len)+2(lang)+2(segCountX2)+6(search fields) = 14
            //   + segCount×2(endCount) + 2(pad) + segCount×2(start) + segCount×2(delta) + segCount×2(range) = 2 + segCount×8
            // Total = 16 + segCount×8
            int glyphIdArrayBytes = (int)length - 16 - segCount * 8;
            int glyphIdCount = glyphIdArrayBytes > 0 ? glyphIdArrayBytes / 2 : 0;

            var glyphIdArray = new ushort[glyphIdCount];
            for (int i = 0; i < glyphIdCount; i++)
                glyphIdArray[i] = reader.ReadUInt16();

            return new TtfCMapFormat4Table(
                platformId, encodingId, segCount,
                endCount, startCount, idDelta, idRangeOffset, glyphIdArray);
        }

        // ── Lookup ────────────────────────────────────────────────────────────

        /// <inheritdoc/>
        /// <remarks>
        /// Standard OpenType format 4 lookup:
        /// binary-searches segments by <c>endCount</c>, then applies
        /// <c>idDelta</c> or the <c>glyphIdArray</c> indirection.
        /// </remarks>
        public override uint GetGlyphIndex(uint codePoint)
        {
            if (codePoint > 0xFFFF) return 0;
            ushort cp = (ushort)codePoint;

            // Binary search: find the leftmost segment where endCount[i] >= cp.
            int lo = 0, hi = _segCount - 1, segIndex = -1;
            while (lo <= hi)
            {
                int mid = (lo + hi) >> 1;
                if (_endCount[mid] < cp)
                    lo = mid + 1;
                else
                {
                    segIndex = mid;
                    hi = mid - 1; // keep searching left for leftmost match
                }
            }

            if (segIndex < 0 || _startCount[segIndex] > cp)
                return 0;

            if (_idRangeOffset[segIndex] == 0)
            {
                // Simple offset: apply idDelta (mod 65536).
                return (uint)(((int)cp + _idDelta[segIndex]) & 0xFFFF);
            }
            else
            {
                // Pointer indirection via glyphIdArray.
                // Index = (segIndex + idRangeOffset[i]/2 + (cp - startCount[i])) - segCount
                // See OpenType spec §5.6.1.1.4 for the derivation of this formula.
                int glyphIdArrayIndex =
                    segIndex
                    + _idRangeOffset[segIndex] / 2
                    + (cp - _startCount[segIndex])
                    - _segCount;

                if (glyphIdArrayIndex < 0 || glyphIdArrayIndex >= _glyphIdArray.Length)
                    return 0;

                ushort rawGlyphId = _glyphIdArray[glyphIdArrayIndex];
                if (rawGlyphId == 0) return 0;
                return (uint)((rawGlyphId + _idDelta[segIndex]) & 0xFFFF);
            }
        }
    }
}
