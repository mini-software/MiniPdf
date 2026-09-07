namespace MiniSoftware.Drawing.Font.Glyphs
{
    /// <summary>
    /// A glyph identifier backed by an unsigned 32-bit glyph index,
    /// as used in TrueType, OpenType and CFF fonts.
    /// </summary>
    public sealed class GlyphUInt32Id : GlyphId
    {
        /// <summary>
        /// The sentinel value that represents the ".notdef" glyph (index 0).
        /// Encoding lookups return this when the requested code point has no mapping.
        /// </summary>
        public static readonly GlyphUInt32Id NotDefId = new GlyphUInt32Id(0);

        /// <summary>Zero-based glyph index within the font.</summary>
        public uint Value { get; }

        /// <summary>
        /// Initializes a new <see cref="GlyphUInt32Id"/> with the specified glyph index.
        /// </summary>
        /// <param name="value">Zero-based glyph index within the font.</param>
        public GlyphUInt32Id(uint value)
        {
            Value = value;
        }

        /// <inheritdoc/>
        public override bool Equals(object? obj) =>
            obj is GlyphUInt32Id other && other.Value == Value;

        /// <inheritdoc/>
        public override int GetHashCode() => Value.GetHashCode();

        /// <inheritdoc/>
        public override string ToString() => $"GlyphId({Value})";
    }
}
