namespace MiniPdf.Drawing.Font.Ttf.Glyphs
{
    /// <summary>
    /// A single point in a TrueType glyph outline contour.
    /// Coordinates are in font design units (integers).
    /// </summary>
    public readonly struct TtfPoint
    {
        /// <summary>X coordinate in design units.</summary>
        public short X { get; }

        /// <summary>Y coordinate in design units.</summary>
        public short Y { get; }

        /// <summary>
        /// True if this point lies on the curve (explicit line segment endpoint or spline endpoint).
        /// False if this is an off-curve quadratic Bézier control point.
        /// </summary>
        public bool OnCurve { get; }

        /// <summary>
        /// True if this point is the last point of its contour
        /// (i.e. its index appears in <c>endPtsOfContours</c>).
        /// </summary>
        public bool IsEndPoint { get; }

        /// <summary>
        /// Initializes a new <see cref="TtfPoint"/> with the specified coordinates and flags.
        /// </summary>
        /// <param name="x">The X coordinate in design units.</param>
        /// <param name="y">The Y coordinate in design units.</param>
        /// <param name="onCurve">True if this point lies on the curve; false if it is an off-curve control point.</param>
        /// <param name="isEndPoint">True if this point is the last point of its contour.</param>
        public TtfPoint(short x, short y, bool onCurve, bool isEndPoint)
        {
            X          = x;
            Y          = y;
            OnCurve    = onCurve;
            IsEndPoint = isEndPoint;
        }

        /// <summary>
        /// Returns a string representation of this <see cref="TtfPoint"/>.
        /// </summary>
        /// <returns>A string in the format "(x, y) on/off end".</returns>
        public override string ToString() =>
            $"({X}, {Y}) {(OnCurve ? "on" : "off")}{(IsEndPoint ? " end" : "")}";
    }
}
