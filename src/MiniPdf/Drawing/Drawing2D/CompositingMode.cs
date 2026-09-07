namespace MiniSoftware.Drawing.Drawing2D
{
    /// <summary>
    /// Specifies how pixels are composited during rendering.
    /// </summary>
    public enum CompositingMode
    {
        /// <summary>
        /// Source pixels are blended with destination pixels based on alpha values.
        /// </summary>
        SourceOver = 0,

        /// <summary>
        /// Source pixels replace destination pixels.
        /// </summary>
        SourceCopy = 1,
    }
}
