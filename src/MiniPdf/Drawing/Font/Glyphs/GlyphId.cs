namespace MiniPdf.Drawing.Font.Glyphs
{
    /// <summary>
    /// Abstract base class for all glyph identifiers.
    /// Concrete implementations: <see cref="GlyphUInt32Id"/> (index-based).
    /// </summary>
    public abstract class GlyphId
    {
        // Prevent instantiation outside this assembly hierarchy.
        internal GlyphId() { }

        /// <inheritdoc/>
        public abstract override bool Equals(object? obj);

        /// <inheritdoc/>
        public abstract override int GetHashCode();

        /// <summary>
        /// Determines whether two <see cref="GlyphId"/> instances are equal.
        /// </summary>
        public static bool operator ==(GlyphId? left, GlyphId? right)
        {
            if (ReferenceEquals(left, right)) return true;
            if (left is null || right is null) return false;
            return left.Equals(right);
        }

        /// <summary>
        /// Determines whether two <see cref="GlyphId"/> instances are not equal.
        /// </summary>
        public static bool operator !=(GlyphId? left, GlyphId? right) => !(left == right);
    }
}
