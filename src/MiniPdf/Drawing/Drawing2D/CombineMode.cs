namespace MiniPdf.Drawing.Drawing2D
{
    /// <summary>
    /// Specifies the combine mode for region operations.
    /// </summary>
    public enum CombineMode
    {
        /// <summary>
        /// Replace the existing region.
        /// </summary>
        Replace = 0,

        /// <summary>
        /// Intersect with the existing region.
        /// </summary>
        Intersect = 1,

        /// <summary>
        /// Union with the existing region.
        /// </summary>
        Union = 2,

        /// <summary>
        /// Exclusive OR with the existing region.
        /// </summary>
        Xor = 3,

        /// <summary>
        /// Exclude from the existing region.
        /// </summary>
        Exclude = 4,

        /// <summary>
        /// Complement the existing region.
        /// </summary>
        Complement = 5
    }
}
