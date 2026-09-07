namespace MiniPdf.Drawing.Drawing2D
{
    /// <summary>
    /// Specifies the cap style for the start and end of lines drawn with a <see cref="Pen"/>.
    /// </summary>
    public enum LineCap
    {
        /// <summary>
        /// Flat line cap.
        /// </summary>
        Flat          = 0,

        /// <summary>
        /// Square line cap.
        /// </summary>
        Square        = 1,

        /// <summary>
        /// Round line cap.
        /// </summary>
        Round         = 2,

        /// <summary>
        /// Triangular line cap.
        /// </summary>
        Triangle      = 3,

        /// <summary>
        /// No anchor.
        /// </summary>
        NoAnchor      = 16,

        /// <summary>
        /// Square anchor.
        /// </summary>
        SquareAnchor  = 17,

        /// <summary>
        /// Round anchor.
        /// </summary>
        RoundAnchor   = 18,

        /// <summary>
        /// Diamond anchor.
        /// </summary>
        DiamondAnchor = 19,

        /// <summary>
        /// Arrow anchor.
        /// </summary>
        ArrowAnchor   = 20,

        /// <summary>
        /// Anchor mask.
        /// </summary>
        AnchorMask    = 240,

        /// <summary>
        /// Custom line cap.
        /// </summary>
        Custom        = 255,
    }
}
