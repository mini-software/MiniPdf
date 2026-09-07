namespace MiniPdf.Drawing.Metafile.Wmf.Records
{
    /// <summary>
    ///     The META_PIE record draws a pie-shaped wedge bounded by the intersection of an ellipse and two radials. 
    ///     The pie is outlined by using the pen and filled by using the brush that are defined in the playback device context.
    /// </summary>
    /// <remarks>
    ///     2.3.3.13 META_PIE Record
    /// </remarks>
    internal class META_PIE : Record
    {
        /// <summary>
        ///     Defines the y-coordinate, in logical coordinates, of the endpoint of the second radial.
        /// </summary>
        public short YRadial2;

        /// <summary>
        ///     Defines the x-coordinate, in logical coordinates, of the endpoint of the second radial.
        /// </summary>
        public short XRadial2;

        /// <summary>
        ///     Defines the y-coordinate, in logical coordinates, of the endpoint of the first radial.
        /// </summary>
        public short YRadial1;

        /// <summary>
        ///     Defines the x-coordinate, in logical coordinates, of the endpoint of the first radial.
        /// </summary>
        public short XRadial1;

        /// <summary>
        ///     Defines the y-coordinate, in logical units, of the lower-right corner of the bounding rectangle.
        /// </summary>
        public short BottomRect;

        /// <summary>
        ///     Defines the x-coordinate, in logical units, of the lower-right corner of the bounding rectangle.
        /// </summary>
        public short RightRect;

        /// <summary>
        ///     Defines the y-coordinate, in logical units, of the upper-left corner of the bounding rectangle.
        /// </summary>
        public short TopRect;

        /// <summary>
        ///     Defines the x-coordinate, in logical units, of the upper-left corner of the bounding rectangle.
        /// </summary>
        public short LeftRect;
    }
}
