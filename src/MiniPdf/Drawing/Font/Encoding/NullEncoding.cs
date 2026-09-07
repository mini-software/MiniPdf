using MiniSoftware.Drawing.Font.Glyphs;

namespace MiniSoftware.Drawing.Font.Encoding
{
    /// <summary>
    /// A no-op encoding that always returns <see cref="GlyphUInt32Id.NotDefId"/>.
    /// Used as a placeholder before a real CMap table is parsed.
    /// </summary>
    public sealed class NullEncoding : IFontEncoding
    {
        /// <summary>Shared singleton instance.</summary>
        public static readonly NullEncoding Instance = new NullEncoding();

        private NullEncoding() { }

        /// <inheritdoc/>
        public GlyphId? DecodeToGid(uint codePoint) => GlyphUInt32Id.NotDefId;

        /// <inheritdoc/>
        public GlyphId? DecodeToGid(char ch) => GlyphUInt32Id.NotDefId;

        /// <inheritdoc/>
        public bool TryGetGlyph(uint codePoint, out uint glyphId)
        {
            glyphId = 0;
            return false;
        }

        /// <inheritdoc/>
        public bool HasGlyph(uint codePoint) => false;
    }
}
