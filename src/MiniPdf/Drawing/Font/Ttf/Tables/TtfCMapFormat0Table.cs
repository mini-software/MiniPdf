using MiniSoftware.Drawing.Font.IO;
using System.IO;

namespace MiniSoftware.Drawing.Font.Ttf.Tables
{
    /// <summary>
    /// CMap format 0: simple byte-array mapping of code points 0–255.
    /// Each entry is a single byte glyph index.
    /// Ref: OpenType spec §5.6.1.1.1.
    /// </summary>
    public sealed class TtfCMapFormat0Table : TtfCMapFormatBaseTable
    {
        private readonly byte[] _glyphIdArray; // 256 entries

        private TtfCMapFormat0Table(ushort platformId, ushort encodingId, byte[] glyphIdArray)
            : base(platformId, encodingId, format: 0)
        {
            _glyphIdArray = glyphIdArray;
        }

        // ── Factory ───────────────────────────────────────────────────────────

        /// <param name="cmapBytes">Full "cmap" table raw bytes.</param>
        /// <param name="subtableOffset">Byte offset of this subtable within <paramref name="cmapBytes"/>.</param>
        internal static TtfCMapFormat0Table Parse(
            byte[] cmapBytes, int subtableOffset,
            ushort platformId, ushort encodingId)
        {
            using var reader = new FontBinaryReader(new MemoryStream(cmapBytes));
            reader.Seek(subtableOffset);
            reader.Skip(2); // format (= 0)
            reader.Skip(2); // length
            reader.Skip(2); // language
            byte[] glyphIds = reader.ReadBytes(256);
            return new TtfCMapFormat0Table(platformId, encodingId, glyphIds);
        }

        // ── Lookup ────────────────────────────────────────────────────────────

        /// <inheritdoc/>
        public override uint GetGlyphIndex(uint codePoint)
        {
            if (codePoint > 255) return 0;
            return _glyphIdArray[(int)codePoint];
        }
    }
}
