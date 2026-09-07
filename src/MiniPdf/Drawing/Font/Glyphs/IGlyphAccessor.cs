namespace MiniPdf.Drawing.Font.Glyphs
{
    /// <summary>
    /// Provides access to individual glyphs within a font.
    /// </summary>
    public interface IGlyphAccessor
    {
        /// <summary>
        /// Returns the <see cref="Glyph"/> for the given <paramref name="glyphId"/>,
        /// or <c>null</c> if the glyph is not present in the font.
        /// </summary>
        Glyph? GetGlyphById(GlyphId glyphId);

        /// <summary>
        /// Returns the <see cref="Glyph"/> for the given zero-based glyph
        /// <paramref name="index"/>, or <c>null</c> if out of range.
        /// </summary>
        Glyph? GetGlyphByIndex(uint index);
    }
}
