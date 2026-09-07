using System;
using MiniPdf.Drawing.Font.Glyphs;

namespace MiniPdf.Drawing.Font.Core
{
    /// <summary>
    /// Concrete, mutable implementation of <see cref="IFontMetrics"/>.
    /// Table parsers set the properties directly; glyph-width queries are
    /// delegated to a resolver function supplied at construction time.
    /// </summary>
    public sealed class FontMetrics : IFontMetrics
    {
        private readonly Func<GlyphId, double> _glyphWidthResolver;

        /// <param name="glyphWidthResolver">
        /// A function that, given a <see cref="GlyphId"/>, returns the advance
        /// width of that glyph in design units. Called once per unique glyph;
        /// callers are responsible for caching if needed.
        /// </param>
        public FontMetrics(Func<GlyphId, double> glyphWidthResolver)
        {
            _glyphWidthResolver = glyphWidthResolver
                ?? throw new ArgumentNullException(nameof(glyphWidthResolver));
        }

        /// <inheritdoc/>
        public double Ascender { get; set; }

        /// <inheritdoc/>
        public double Descender { get; set; }

        /// <inheritdoc/>
        public double TypoAscender { get; set; }

        /// <inheritdoc/>
        public double TypoDescender { get; set; }

        /// <inheritdoc/>
        public double LineGap { get; set; }

        /// <inheritdoc/>
        public double UnitsPerEM { get; set; }

        /// <inheritdoc/>
        public double GetGlyphWidth(GlyphId glyphId)
        {
            if (glyphId is null) throw new ArgumentNullException(nameof(glyphId));
            return _glyphWidthResolver(glyphId);
        }

        /// <inheritdoc/>
        public double UnderlinePosition { get; set; }

        /// <inheritdoc/>
        public double UnderlineThickness { get; set; }

        /// <inheritdoc/>
        public double StrikeoutPosition { get; set; }

        /// <inheritdoc/>
        public double StrikeoutSize { get; set; }
    }
}
