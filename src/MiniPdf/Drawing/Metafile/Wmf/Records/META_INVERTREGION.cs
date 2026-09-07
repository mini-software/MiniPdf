namespace MiniPdf.Drawing.Metafile.Wmf.Records
{
    /// <summary>
    ///     The META_INVERTREGION record draws a region in which the colors are inverted.
    /// </summary>
    /// <remarks>
    ///     2.3.3.9 META_INVERTREGION Record
    /// </remarks>
    internal class META_INVERTREGION : Record
    {
        /// <summary>
        ///     Used to index into the WMF Object Table to get the region to be inverted.
        /// </summary>
        public ushort Region;
    }
}
