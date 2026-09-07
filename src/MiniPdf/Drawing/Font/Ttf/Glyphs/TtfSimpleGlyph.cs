using System;

namespace MiniPdf.Drawing.Font.Ttf.Glyphs
{
    /// <summary>
    /// Represents a TrueType simple-glyph outline: contour data parsed from
    /// a non-composite, non-empty entry in the "glyf" table.
    /// </summary>
    public sealed class TtfSimpleGlyph
    {
        /// <summary>Number of closed contours.</summary>
        public int NumberOfContours { get; }

        /// <summary>
        /// All points of all contours, in order.
        /// Use <see cref="EndPtsOfContours"/> to determine contour boundaries.
        /// </summary>
        public TtfPoint[] Points { get; }

        /// <summary>
        /// Index (into <see cref="Points"/>) of the last point of each contour.
        /// Length == <see cref="NumberOfContours"/>.
        /// </summary>
        public ushort[] EndPtsOfContours { get; }

        /// <summary>TrueType hinting instructions (may be empty).</summary>
        public byte[] Instructions { get; }

        /// <summary>
        /// Initializes a new <see cref="TtfSimpleGlyph"/> with the specified contour data.
        /// </summary>
        /// <param name="numberOfContours">Number of closed contours.</param>
        /// <param name="points">All points of all contours, in order.</param>
        /// <param name="endPtsOfContours">Index into Points of the last point of each contour.</param>
        /// <param name="instructions">TrueType hinting instructions.</param>
        public TtfSimpleGlyph(
            int numberOfContours,
            TtfPoint[] points,
            ushort[] endPtsOfContours,
            byte[] instructions)
        {
            NumberOfContours  = numberOfContours;
            Points            = points            ?? Array.Empty<TtfPoint>();
            EndPtsOfContours  = endPtsOfContours  ?? Array.Empty<ushort>();
            Instructions      = instructions      ?? Array.Empty<byte>();
        }

        /// <summary>A zero-contour simple glyph (e.g. space character).</summary>
        public static readonly TtfSimpleGlyph Empty =
            new TtfSimpleGlyph(0, Array.Empty<TtfPoint>(), Array.Empty<ushort>(), Array.Empty<byte>());
    }
}
