namespace MiniPdf.Drawing.Imaging
{
    /// <summary>
    /// Specifies the type of color adjustment for image attributes.
    /// </summary>
    public enum ColorAdjustType
    {
        /// <summary>
        /// Default color adjustment type.
        /// </summary>
        Default = 0,

        /// <summary>
        /// Bitmap color adjustment.
        /// </summary>
        Bitmap  = 1,

        /// <summary>
        /// Brush color adjustment.
        /// </summary>
        Brush   = 2,

        /// <summary>
        /// Pen color adjustment.
        /// </summary>
        Pen     = 3,

        /// <summary>
        /// Text color adjustment.
        /// </summary>
        Text    = 4,

        /// <summary>
        /// Count of color adjustment types.
        /// </summary>
        Count   = 5,

        /// <summary>
        /// Any color adjustment type.
        /// </summary>
        Any     = 6,
    }
}
