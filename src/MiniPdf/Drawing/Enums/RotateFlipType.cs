namespace MiniSoftware.Drawing.Enums
{
    /// <summary>
    /// Specifies the type of rotation and/or flipping to apply to an image.
    /// </summary>
    public enum RotateFlipType
    {
        /// <summary>
        /// No rotation and no flipping.
        /// </summary>
        RotateNoneFlipNone = 0,

        /// <summary>
        /// 90-degree rotation with no flipping.
        /// </summary>
        Rotate90FlipNone   = 1,

        /// <summary>
        /// 180-degree rotation with no flipping.
        /// </summary>
        Rotate180FlipNone  = 2,

        /// <summary>
        /// 270-degree rotation with no flipping.
        /// </summary>
        Rotate270FlipNone  = 3,

        /// <summary>
        /// No rotation with horizontal flipping.
        /// </summary>
        RotateNoneFlipX    = 4,

        /// <summary>
        /// 90-degree rotation with horizontal flipping.
        /// </summary>
        Rotate90FlipX      = 5,

        /// <summary>
        /// 180-degree rotation with horizontal flipping.
        /// </summary>
        Rotate180FlipX     = 6,

        /// <summary>
        /// 270-degree rotation with horizontal flipping.
        /// </summary>
        Rotate270FlipX     = 7,
    }
}
