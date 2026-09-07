using MiniSoftware.Drawing.Geometry;

namespace MiniSoftware.Drawing.Drawing2D
{
    /// <summary>
    /// Contains the data needed to create or modify a <see cref="GraphicsPath"/>.
    /// </summary>
    public sealed class PathData
    {
        /// <summary>
        /// Gets or sets the array of points that define the path.
        /// </summary>
        public PointF[] Points { get; set; } = System.Array.Empty<PointF>();

        /// <summary>
        /// Gets or sets the array of point types (PathPointType values) for the path.
        /// </summary>
        public byte[]   Types  { get; set; } = System.Array.Empty<byte>();
    }
}
