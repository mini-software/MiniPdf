namespace MiniPdf.Drawing.Metafile.Wmf.Enumerations
{
    /// <summary>
    ///     Specifies the different possible brush types that can be used in graphics operations.
    /// </summary>
    internal enum BrushStyle : ushort
    {
        /// <summary>
        ///     A brush that paints a single, constant color, either solid or dithered.
        /// </summary>
        BS_SOLID = 0x0000,

        /// <summary>
        ///     A brush that does nothing. Using a BS_NULL brush in a graphics operation MUST have the same effect as using no brush at all.
        /// </summary>
        BS_NULL = 0x0001,

        /// <summary>
        ///     A brush that paints a predefined simple pattern, or "hatch", onto a solid background.
        /// </summary>
        BS_HATCHED = 0x0002,

        /// <summary>
        ///     A brush that paints a pattern defined by a bitmap, which MAY be a Bitmap16 Object or a DeviceIndependentBitmap (DIB) Object.
        /// </summary>
        BS_PATTERN = 0x0003,

        /// <summary>
        ///     Not supported.
        /// </summary>
        BS_INDEXED = 0x0004,

        /// <summary>
        ///     A pattern brush specified by a DIB.
        /// </summary>
        BS_DIBPATTERN = 0x0005,

        /// <summary>
        ///     A pattern brush specified by a DIB.
        /// </summary>
        BS_DIBPATTERNPT = 0x0006,

        /// <summary>
        ///     Not supported.
        /// </summary>
        BS_PATTERN8X8 = 0x0007,

        /// <summary>
        ///     Not supported.
        /// </summary>
        BS_DIBPATTERN8X8 = 0x0008,

        /// <summary>
        ///     Not supported.
        /// </summary>
        BS_MONOPATTERN = 0x0009
    }
}
