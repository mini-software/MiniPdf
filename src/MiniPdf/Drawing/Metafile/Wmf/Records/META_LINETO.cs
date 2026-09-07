namespace MiniPdf.Drawing.Metafile.Wmf.Records
{
    /// <summary>
    ///     The META_LINETO record draws a line from the drawing position that is defined in the playback device context up to, but not including, the specified point.
    /// </summary>
    /// <remarks>
    ///     2.3.3.10 META_LINETO Record
    /// </remarks>
    internal class META_LINETO : Record
    {
        /// <summary>
        ///     Defines the vertical component of the drawing destination position, in logical units.
        /// </summary>
        public short Y;

        /// <summary>
        ///     Defines the horizontal component of the drawing destination position, in logical units.
        /// </summary>
        public short X;
    }
}
