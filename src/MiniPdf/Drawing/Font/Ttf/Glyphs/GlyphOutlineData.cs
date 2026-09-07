using MiniPdf.Drawing.Font.Glyphs;

namespace MiniPdf.Drawing.Font.Ttf.Glyphs
{
    /// <summary>
    /// Parsed outline data for one glyph, as extracted from the "glyf" table.
    /// </summary>
    public sealed class GlyphOutlineData
    {
        /// <summary>True for composite glyphs (numberOfContours == −1 in the glyf header).</summary>
        public bool IsComposite { get; }

        /// <summary>
        /// Non-null for simple (non-composite) glyphs, including zero-contour empty glyphs.
        /// </summary>
        public TtfSimpleGlyph? Simple { get; }

        /// <summary>Non-null for composite glyphs.</summary>
        public TtfCompositeGlyph? Composite { get; }

        /// <summary>Design-space bounding box read from the glyph header.</summary>
        public GlyphBoundingBox BBox { get; }

        /// <summary>
        /// Initializes a new <see cref="GlyphOutlineData"/> with the specified values.
        /// </summary>
        /// <param name="isComposite">True for composite glyphs.</param>
        /// <param name="simple">Non-null for simple (non-composite) glyphs.</param>
        /// <param name="composite">Non-null for composite glyphs.</param>
        /// <param name="bbox">Design-space bounding box read from the glyph header.</param>
        public GlyphOutlineData(
            bool isComposite,
            TtfSimpleGlyph? simple,
            TtfCompositeGlyph? composite,
            GlyphBoundingBox bbox)
        {
            IsComposite = isComposite;
            Simple      = simple;
            Composite   = composite;
            BBox        = bbox;
        }

        /// <summary>Outline for an empty glyph (e.g. space — no contours, no bbox).</summary>
        public static readonly GlyphOutlineData Empty =
            new GlyphOutlineData(false, TtfSimpleGlyph.Empty, null, GlyphBoundingBox.Empty);
    }
}
