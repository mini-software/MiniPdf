namespace MiniSoftware.Drawing.Metafile.Wmf.Enumerations
{
    /// <summary>
    ///     Specifies the type of fill operation to be performed.
    /// </summary>
    /// <remarks>
    ///     2.1.1.9 FloodFill Enumeration
    /// </remarks>
    internal enum FloodFill
    {
        /// <summary>
        ///     The fill area is bounded by the color specified by the Color member. 
        ///     This style is identical to the filling performed by the META_FLOODFILL record.
        /// </summary>
        FLOODFILLBORDER = 0x0000,

        /// <summary>
        ///     The fill area is bounded by the color that is specified by the Color member. Filling continues outward in all directions as long as the color is encountered. This style is useful for filling areas with multicolored boundaries.
        /// </summary>
        FLOODFILLSURFACE = 0x0001
    }
}
