namespace MiniPdf.Drawing.Font.Glyphs
{
    /// <summary>
    /// Axis-aligned bounding box of a glyph outline, in design units.
    /// </summary>
    public sealed class GlyphBoundingBox
    {
        /// <summary>
        /// An empty bounding box with zero dimensions.
        /// </summary>
        public static readonly GlyphBoundingBox Empty = new GlyphBoundingBox(0, 0, 0, 0);

        /// <summary>
        /// Initializes a new <see cref="GlyphBoundingBox"/> with the specified coordinates.
        /// </summary>
        /// <param name="xMin">Leftmost x-coordinate of the glyph outline.</param>
        /// <param name="yMin">Bottommost y-coordinate of the glyph outline.</param>
        /// <param name="xMax">Rightmost x-coordinate of the glyph outline.</param>
        /// <param name="yMax">Topmost y-coordinate of the glyph outline.</param>
        public GlyphBoundingBox(double xMin, double yMin, double xMax, double yMax)
        {
            XMin = xMin;
            YMin = yMin;
            XMax = xMax;
            YMax = yMax;
        }

        /// <summary>Leftmost x-coordinate of the glyph outline.</summary>
        public double XMin { get; }

        /// <summary>Bottommost y-coordinate of the glyph outline.</summary>
        public double YMin { get; }

        /// <summary>Rightmost x-coordinate of the glyph outline.</summary>
        public double XMax { get; }

        /// <summary>Topmost y-coordinate of the glyph outline.</summary>
        public double YMax { get; }

        /// <summary>Width of the bounding box (XMax − XMin).</summary>
        public double Width => XMax - XMin;

        /// <summary>Height of the bounding box (YMax − YMin).</summary>
        public double Height => YMax - YMin;

        /// <summary>
        /// Returns a string representation of this bounding box.
        /// </summary>
        public override string ToString() =>
            $"BBox(XMin={XMin}, YMin={YMin}, XMax={XMax}, YMax={YMax})";
    }
}
