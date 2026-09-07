namespace MiniPdf.Drawing.Imaging
{
    /// <summary>
    /// Specifies the unit of measurement for metafile frames.
    /// </summary>
    public enum MetafileFrameUnit
    {
        /// <summary>
        /// Pixel units.
        /// </summary>
        Pixel,

        /// <summary>
        /// Point units (1/72 inch).
        /// </summary>
        Point,

        /// <summary>
        /// Inch units.
        /// </summary>
        Inch,

        /// <summary>
        /// Document units (1/300 inch).
        /// </summary>
        Document,

        /// <summary>
        /// Millimeter units.
        /// </summary>
        Millimeter,

        /// <summary>
        /// GDI-compatible units.
        /// </summary>
        GdiCompatible,
    }
}
