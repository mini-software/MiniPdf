using System;

namespace MiniSoftware.Drawing.Font.Glyphs
{
    /// <summary>
    /// Represents a single glyph with its metrics and bounding box.
    /// Obtain instances via <see cref="IGlyphAccessor"/>.
    /// </summary>
    public sealed class Glyph
    {
        /// <summary>
        /// Initializes a new <see cref="Glyph"/> with the specified identifier, bounding box, and advance width.
        /// </summary>
        /// <param name="id">The identifier of this glyph within its font.</param>
        /// <param name="glyphBBox">Axis-aligned bounding box of the glyph outline, in design units.</param>
        /// <param name="advanceWidth">Horizontal advance width in design units.</param>
        public Glyph(GlyphId id, GlyphBoundingBox glyphBBox, double advanceWidth)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
            GlyphBBox = glyphBBox ?? throw new ArgumentNullException(nameof(glyphBBox));
            AdvanceWidth = advanceWidth;
        }

        /// <summary>The identifier of this glyph within its font.</summary>
        public GlyphId Id { get; }

        /// <summary>Axis-aligned bounding box of the glyph outline, in design units.</summary>
        public GlyphBoundingBox GlyphBBox { get; }

        /// <summary>
        /// Horizontal advance width in design units (how far the text cursor moves after
        /// rendering this glyph).
        /// </summary>
        public double AdvanceWidth { get; }

        /// <summary>
        /// Returns a string representation of this glyph.
        /// </summary>
        public override string ToString() =>
            $"Glyph({Id}, AdvW={AdvanceWidth}, {GlyphBBox})";
    }
}
