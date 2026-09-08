using MiniSoftware.Drawing.Font.Glyphs;

namespace MiniSoftware.Drawing.Font.Core
{
    /// <summary>
    /// Provides access to horizontal and typographic font metrics in design units.
    /// </summary>
    public interface IFontMetrics
    {
        /// <summary>
        /// Maximum ascender value above the baseline, in design units (from hhea.Ascender).
        /// </summary>
        double Ascender { get; }

        /// <summary>
        /// Maximum descender value below the baseline, in design units (from hhea.Descender,
        /// typically negative).
        /// </summary>
        double Descender { get; }

        /// <summary>
        /// Typographic ascender from the OS/2 sTypoAscender field, in design units.
        /// </summary>
        double TypoAscender { get; }

        /// <summary>
        /// Typographic descender from the OS/2 sTypoDescender field, in design units
        /// (typically negative).
        /// </summary>
        double TypoDescender { get; }

        /// <summary>
        /// Line gap (leading) recommended by the font, in design units (from hhea.LineGap).
        /// </summary>
        double LineGap { get; }

        /// <summary>
        /// Number of design units that make up one em square.
        /// </summary>
        double UnitsPerEM { get; }

        /// <summary>
        /// Returns the horizontal advance width of the glyph identified by
        /// <paramref name="glyphId"/>, in design units.
        /// </summary>
        double GetGlyphWidth(GlyphId glyphId);

        // ── Decoration metrics (design units) ──────────────────────────────────────

        /// <summary>
        /// Offset of the top of the underline stroke from the baseline, in design units
        /// (from the 'post' table). Negative values place the underline below the baseline.
        /// </summary>
        double UnderlinePosition { get; }

        /// <summary>Thickness of the underline stroke, in design units (from the 'post' table).</summary>
        double UnderlineThickness { get; }

        /// <summary>
        /// Distance from the baseline to the top of the strikeout stroke, in design units
        /// (from OS/2 yStrikeoutPosition). Positive values are above the baseline.
        /// </summary>
        double StrikeoutPosition { get; }

        /// <summary>Thickness of the strikeout stroke, in design units (from OS/2 yStrikeoutSize).</summary>
        double StrikeoutSize { get; }
    }
}
