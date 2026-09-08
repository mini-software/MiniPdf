namespace MiniSoftware.Drawing.Drawing2D
{
    /// <summary>
    /// Specifies the join style for the connection of line segments drawn with a <see cref="Pen"/>.
    /// </summary>
    public enum LineJoin
    {
        /// <summary>
        /// Miter join.
        /// </summary>
        Miter        = 0,

        /// <summary>
        /// Bevel join.
        /// </summary>
        Bevel        = 1,

        /// <summary>
        /// Round join.
        /// </summary>
        Round        = 2,

        /// <summary>
        /// Miter clipped join.
        /// </summary>
        MiterClipped = 3,
    }
}
