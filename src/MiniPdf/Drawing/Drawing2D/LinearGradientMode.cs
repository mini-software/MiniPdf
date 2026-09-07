namespace MiniSoftware.Drawing.Drawing2D
{
    /// <summary>
    /// Specifies the direction of a linear gradient.
    /// </summary>
    public enum LinearGradientMode
    {
        /// <summary>
        /// Horizontal gradient.
        /// </summary>
        Horizontal       = 0,

        /// <summary>
        /// Vertical gradient.
        /// </summary>
        Vertical         = 1,

        /// <summary>
        /// Forward diagonal gradient (top-left to bottom-right).
        /// </summary>
        ForwardDiagonal  = 2,

        /// <summary>
        /// Backward diagonal gradient (top-right to bottom-left).
        /// </summary>
        BackwardDiagonal = 3,
    }
}
