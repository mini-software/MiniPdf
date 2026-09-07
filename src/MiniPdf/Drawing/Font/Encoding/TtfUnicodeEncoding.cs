using MiniPdf.Drawing.Font.Glyphs;
using MiniPdf.Drawing.Font.Ttf.Tables;

namespace MiniPdf.Drawing.Font.Encoding
{
    /// <summary>
    /// Implements <see cref="IFontEncoding"/> by delegating to a CMap format subtable
    /// (<see cref="TtfCMapFormatBaseTable"/>) selected from a TrueType/OpenType font's
    /// "cmap" table.
    /// </summary>
    public sealed class TtfUnicodeEncoding : IFontEncoding
    {
        private readonly TtfCMapFormatBaseTable _cmapTable;

        /// <param name="cmapTable">
        /// The best Unicode-capable subtable as returned by
        /// <see cref="TtfCMapTable.FindUnicodeTable"/>.
        /// Must not be null.
        /// </param>
        public TtfUnicodeEncoding(TtfCMapFormatBaseTable cmapTable)
        {
            _cmapTable = cmapTable;
        }

        /// <inheritdoc/>
        /// <remarks>
        /// Returns <see cref="GlyphUInt32Id.NotDefId"/> (glyph index 0) for code
        /// points not present in the font.
        /// </remarks>
        public GlyphId? DecodeToGid(uint codePoint)
        {
            uint glyphIndex = _cmapTable.GetGlyphIndex(codePoint);
            return new GlyphUInt32Id(glyphIndex);
        }

        /// <inheritdoc/>
        public GlyphId? DecodeToGid(char ch) => DecodeToGid((uint)ch);

        /// <inheritdoc/>
        public bool TryGetGlyph(uint codePoint, out uint glyphId)
        {
            glyphId = _cmapTable.GetGlyphIndex(codePoint);
            return glyphId != 0;
        }

        /// <inheritdoc/>
        public bool HasGlyph(uint codePoint) => _cmapTable.GetGlyphIndex(codePoint) != 0;
    }
}
