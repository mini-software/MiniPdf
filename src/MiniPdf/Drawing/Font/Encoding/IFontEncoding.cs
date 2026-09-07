using MiniPdf.Drawing.Font.Glyphs;

namespace MiniPdf.Drawing.Font.Encoding
{
    /// <summary>
    /// Maps Unicode code points (and characters) to glyph identifiers within a font.
    /// Typically backed by a CMap subtable selected from the font's "cmap" table.
    /// </summary>
    public interface IFontEncoding
    {
        /// <summary>
        /// Resolves the Unicode <paramref name="codePoint"/> to a glyph identifier.
        /// Returns <see cref="GlyphUInt32Id.NotDefId"/> when the font has no glyph
        /// for the given code point, or <c>null</c> when the encoding map is absent.
        /// </summary>
        GlyphId? DecodeToGid(uint codePoint);

        /// <summary>
        /// Convenience overload that accepts a <see cref="char"/> (UTF-16 BMP code unit).
        /// For code points outside the BMP, use <see cref="DecodeToGid(uint)"/> directly.
        /// </summary>
        GlyphId? DecodeToGid(char ch);

        /// <summary>
        /// Returns true when the font provides a real glyph (not <c>.notdef</c>)
        /// for <paramref name="codePoint"/>. Used by the fallback resolver to decide
        /// whether to substitute a fallback font.
        /// </summary>
        /// <param name="codePoint">The Unicode code point to test.</param>
        /// <param name="glyphId">
        /// When this method returns true, the resolved glyph identifier; otherwise 0.
        /// </param>
        bool TryGetGlyph(uint codePoint, out uint glyphId);

        /// <summary>
        /// Returns true when the font provides a real glyph (not <c>.notdef</c>)
        /// for <paramref name="codePoint"/>.
        /// </summary>
        bool HasGlyph(uint codePoint);
    }

    /// <summary>
    /// Default implementation of <see cref="IFontEncoding.TryGetGlyph"/> and
    /// <see cref="IFontEncoding.HasGlyph"/> that derives coverage from
    /// <see cref="IFontEncoding.DecodeToGid(uint)"/>. Implementers that can
    /// distinguish "no cmap entry" from "cmap entry → glyph 0" should override
    /// these methods directly.
    /// </summary>
    public static class FontEncodingDefaults
    {
        /// <summary>
        /// Default <see cref="IFontEncoding.TryGetGlyph"/> implementation.
        /// </summary>
        public static bool TryGetGlyph(IFontEncoding encoding, uint codePoint, out uint glyphId)
        {
            var gid = encoding.DecodeToGid(codePoint);
            if (gid is GlyphUInt32Id u32 && u32.Value != 0)
            {
                glyphId = u32.Value;
                return true;
            }
            glyphId = 0;
            return false;
        }

        /// <summary>
        /// Default <see cref="IFontEncoding.HasGlyph"/> implementation.
        /// </summary>
        public static bool HasGlyph(IFontEncoding encoding, uint codePoint)
            => TryGetGlyph(encoding, codePoint, out _);
    }
}
