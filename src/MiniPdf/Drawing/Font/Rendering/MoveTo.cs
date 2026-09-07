namespace MiniPdf.Drawing.Font.Rendering
{
    /// <summary>
    /// A "move to" drawing command: lifts the pen and sets the current point
    /// to (<see cref="X"/>, <see cref="Y"/>) without drawing anything.
    /// Coordinates are in font design units.
    /// </summary>
    public readonly struct MoveTo
    {
        /// <summary>
        /// Gets the X coordinate of the move-to point.
        /// </summary>
        public double X { get; }

        /// <summary>
        /// Gets the Y coordinate of the move-to point.
        /// </summary>
        public double Y { get; }

        /// <summary>
        /// Initializes a new <see cref="MoveTo"/> with the specified coordinates.
        /// </summary>
        /// <param name="x">The X coordinate.</param>
        /// <param name="y">The Y coordinate.</param>
        public MoveTo(double x, double y) { X = x; Y = y; }

        /// <summary>
        /// Returns a string representation of this <see cref="MoveTo"/>.
        /// </summary>
        /// <returns>A string in the format "MoveTo(x, y)".</returns>
        public override string ToString() => $"MoveTo({X:F2}, {Y:F2})";
    }
}
