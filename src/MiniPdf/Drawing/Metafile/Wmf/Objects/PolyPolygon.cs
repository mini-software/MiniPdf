namespace MiniPdf.Drawing.Metafile.Wmf.Objects
{
    /// <summary>
    ///     The PolyPolygon Object defines a series of closed polygons.
    /// </summary>
    /// <remarks>
    ///     2.2.2.17 PolyPolygon Object
    /// </remarks>
    internal struct PolyPolygon
    {
        /// <summary>
        ///     A 16-bit unsigned integer that defines the number of polygons in the object.
        /// </summary>
        public ushort NumberOfPolygons;

        /// <summary>
        ///     A NumberOfPolygons array of 16-bit unsigned integers that define the number of points for each polygon in the object.
        /// </summary>
        public uint[] aPointsPerPolygon;

        /// <summary>
        ///     An array of 16-bit unsigned integers that define the coordinates of the polygons.
        /// </summary>
        public uint[] aPoints;
    }
}
