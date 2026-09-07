namespace MiniSoftware.Drawing.Drawing2D
{
    /// <summary>
    /// Specifies the type of a <see cref="Pen"/> fill.
    /// </summary>
    public enum PenType
    {
        /// <summary>
        /// Solid color pen.
        /// </summary>
        SolidColor      = 0,

        /// <summary>
        /// Hatch fill pen.
        /// </summary>
        HatchFill       = 1,

        /// <summary>
        /// Texture fill pen.
        /// </summary>
        TextureFill     = 2,

        /// <summary>
        /// Path gradient pen.
        /// </summary>
        PathGradient    = 3,

        /// <summary>
        /// Linear gradient pen.
        /// </summary>
        LinearGradient  = 4,
    }
}
