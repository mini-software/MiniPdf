namespace MiniSoftware.Drawing.Drawing2D
{
    /// <summary>
    /// Specifies how texture and gradient brushes are tiled when they extend beyond the shape.
    /// </summary>
    public enum WrapMode
    {
        /// <summary>
        /// Tiles the brush horizontally and vertically.
        /// </summary>
        Tile      = 0,

        /// <summary>
        /// Tiles the brush horizontally and flips it vertically.
        /// </summary>
        TileFlipX = 1,

        /// <summary>
        /// Tiles the brush vertically and flips it horizontally.
        /// </summary>
        TileFlipY = 2,

        /// <summary>
        /// Tiles the brush horizontally and vertically and flips both.
        /// </summary>
        TileFlipXY = 3,

        /// <summary>
        /// Clamps the brush to the edges.
        /// </summary>
        Clamp     = 4,
    }
}
