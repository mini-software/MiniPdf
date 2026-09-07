using System;

namespace MiniPdf.Drawing.Drawing2D
{
    /// <summary>
    /// Specifies the type of a path point. Upper bits carry flags; lower 3 bits are the point type.
    /// </summary>
    [Flags]
    public enum PathPointType : byte
    {
        /// <summary>
        /// Indicates the start of a new figure.
        /// </summary>
        Start        = 0,

        /// <summary>
        /// Indicates a line segment.
        /// </summary>
        Line         = 1,

        /// <summary>
        /// Indicates a Bézier curve segment with one control point.
        /// </summary>
        Bezier       = 3,

        /// <summary>
        /// Indicates a Bézier curve segment with two control points.
        /// </summary>
        Bezier3      = 3,

        /// <summary>
        /// A mask that isolates the point type from the flags.
        /// </summary>
        PathTypeMask = 0x07,

        /// <summary>
        /// Indicates that the point is a dash mode marker.
        /// </summary>
        DashMode     = 0x10,

        /// <summary>
        /// Indicates that the point is a path marker.
        /// </summary>
        PathMarker   = 0x20,

        /// <summary>
        /// Indicates that the figure should be closed.
        /// </summary>
        CloseSubpath = 0x80,
    }
}
