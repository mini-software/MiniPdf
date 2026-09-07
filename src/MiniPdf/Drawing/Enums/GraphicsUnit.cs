namespace MiniPdf.Drawing.Enums
{
    /// <summary>
    /// Specifies the unit of measure for graphics operations.
    /// </summary>
    public enum GraphicsUnit
    {
        /// <summary>
        /// World coordinate (multiple document units per inch).
        /// </summary>
        World      = 0,

        /// <summary>
        /// Display device units (multiple pixels per inch).
        /// </summary>
        Display    = 1,

        /// <summary>
        /// Measurement in pixels.
        /// </summary>
        Pixel      = 2,

        /// <summary>
        /// Measurement in points (1/72 inch).
        /// </summary>
        Point      = 3,

        /// <summary>
        /// Measurement in inches.
        /// </summary>
        Inch       = 4,

        /// <summary>
        /// Measurement in document units (1/300 inch).
        /// </summary>
        Document   = 5,

        /// <summary>
        /// Measurement in millimeters.
        /// </summary>
        Millimeter = 6,
    }
}
