namespace MiniSoftware.Drawing.Font.Rendering
{
    /// <summary>
    /// A "curve to" drawing command representing a cubic Bézier segment.
    /// Draws from the current point using two explicit control points to the
    /// specified endpoint.
    /// <list type="bullet">
    ///   <item>(<see cref="X1"/>, <see cref="Y1"/>) — first control point.</item>
    ///   <item>(<see cref="X2"/>, <see cref="Y2"/>) — second control point.</item>
    ///   <item>(<see cref="X3"/>, <see cref="Y3"/>) — endpoint (new current point).</item>
    /// </list>
    /// TrueType quadratic Bézier segments are converted to this cubic form before
    /// being emitted (see <see cref="TtfOutlineRenderer"/>).
    /// Coordinates are in font design units.
    /// </summary>
    public readonly struct CurveTo
    {
        /// <summary>First cubic control point X.</summary>
        public double X1 { get; }
        /// <summary>First cubic control point Y.</summary>
        public double Y1 { get; }

        /// <summary>Second cubic control point X.</summary>
        public double X2 { get; }
        /// <summary>Second cubic control point Y.</summary>
        public double Y2 { get; }

        /// <summary>Endpoint X (becomes new current point).</summary>
        public double X3 { get; }
        /// <summary>Endpoint Y (becomes new current point).</summary>
        public double Y3 { get; }

        /// <summary>
        /// Initializes a new <see cref="CurveTo"/> with the specified control points and endpoint.
        /// </summary>
        /// <param name="x1">First cubic control point X.</param>
        /// <param name="y1">First cubic control point Y.</param>
        /// <param name="x2">Second cubic control point X.</param>
        /// <param name="y2">Second cubic control point Y.</param>
        /// <param name="x3">Endpoint X.</param>
        /// <param name="y3">Endpoint Y.</param>
        public CurveTo(double x1, double y1, double x2, double y2, double x3, double y3)
        {
            X1 = x1; Y1 = y1;
            X2 = x2; Y2 = y2;
            X3 = x3; Y3 = y3;
        }

        /// <summary>
        /// Returns a string representation of this curve command.
        /// </summary>
        public override string ToString() =>
            $"CurveTo(cp1=({X1:F2},{Y1:F2}) cp2=({X2:F2},{Y2:F2}) end=({X3:F2},{Y3:F2}))";
    }
}
