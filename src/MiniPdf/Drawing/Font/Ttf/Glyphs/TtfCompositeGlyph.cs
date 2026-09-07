using System;

namespace MiniSoftware.Drawing.Font.Ttf.Glyphs
{
    /// <summary>
    /// One component entry within a TrueType composite glyph.
    /// </summary>
    public sealed class TtfCompositeComponent
    {
        /// <summary>Glyph index of the referenced component glyph.</summary>
        public ushort GlyphIndex { get; }

        /// <summary>
        /// 2×3 affine transformation matrix applied to the component:
        /// <code>
        /// | Transform[0,0]  Transform[0,1]  Transform[0,2] |   | a  b  dx |
        /// | Transform[1,0]  Transform[1,1]  Transform[1,2] | = | c  d  dy |
        /// </code>
        /// Point transform: x' = a·x + b·y + dx,  y' = c·x + d·y + dy.<br/>
        /// Identity (pure translation): a=1, b=0, c=0, d=1.
        /// </summary>
        public float[,] Transform { get; }

        /// <summary>
        /// True when <c>Transform[0,2]</c> and <c>Transform[1,2]</c> are xy offsets.
        /// False when Arg1/Arg2 are matched point indices (anchor alignment).
        /// </summary>
        public bool ArgsAreXYValues { get; }

        /// <summary>
        /// Raw Arg1 from the component record.
        /// When <see cref="ArgsAreXYValues"/> is false, this is a point index in the
        /// composite glyph outline (matched to <see cref="Arg2"/> in the component).
        /// </summary>
        public short Arg1 { get; }

        /// <summary>
        /// Raw Arg2 from the component record.
        /// When <see cref="ArgsAreXYValues"/> is false, this is a point index in the
        /// component glyph outline.
        /// </summary>
        public short Arg2 { get; }

        internal TtfCompositeComponent(
            ushort glyphIndex,
            float[,] transform,
            bool argsAreXYValues,
            short arg1, short arg2)
        {
            GlyphIndex      = glyphIndex;
            Transform       = transform;
            ArgsAreXYValues = argsAreXYValues;
            Arg1            = arg1;
            Arg2            = arg2;
        }
    }

    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Represents a composite TrueType glyph: an ordered list of component
    /// glyphs each with its own affine transformation.
    /// </summary>
    public sealed class TtfCompositeGlyph
    {
        /// <summary>Component records in render order.</summary>
        public TtfCompositeComponent[] Components { get; }

        /// <summary>Hinting instructions for the assembled composite (may be empty).</summary>
        public byte[] Instructions { get; }

        internal TtfCompositeGlyph(TtfCompositeComponent[] components, byte[] instructions)
        {
            Components   = components   ?? Array.Empty<TtfCompositeComponent>();
            Instructions = instructions ?? Array.Empty<byte>();
        }
    }
}
