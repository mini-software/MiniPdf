namespace MiniPdf.Drawing.Font.Rendering
{
    /// <summary>
    /// A "line to" drawing command: draws a straight line from the current point
    /// to (<see cref="X"/>, <see cref="Y"/>).
    /// Coordinates are in font design units.
    /// </summary>
    public readonly struct LineTo
    {
        /// <summary>
        /// Gets the X coordinate of the line-to point.
        /// </summary>
        public double X { get; }

        /// <summary>
        /// Gets the Y coordinate of the line-to point.
        /// </summary>
        public double Y { get; }

        /// <summary>
        /// Initializes a new <see cref="LineTo"/> with the specified coordinates.
        /// </summary>
        /// <param name="x">The X coordinate.</param>
        /// <param name="y">The Y coordinate.</param>
        public LineTo(double x, double y) { X = x; Y = y; }

        /// <summary>
        /// Returns a string representation of this <see cref="LineTo"/>.
        /// </summary>
        /// <returns>A string in the format "LineTo(x, y)".</returns>
        public override string ToString() => $"LineTo({X:F2}, {Y:F2})";
    }
}
