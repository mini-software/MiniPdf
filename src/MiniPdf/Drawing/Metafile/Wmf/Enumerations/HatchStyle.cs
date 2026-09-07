namespace MiniSoftware.Drawing.Metafile.Wmf.Enumerations
{
    /// <summary>
    ///     Specifies the hatch pattern.
    /// </summary>
    /// <remarks>
    ///     2.1.1.12 HatchStyle Enumeration
    /// </remarks>
    internal enum HatchStyle
    {
        /// <summary>
        ///     A horizontal hatch.
        /// </summary>
        HS_HORIZONTAL = 0x0000,

        /// <summary>
        ///     A vertical hatch.
        /// </summary>
        HS_VERTICAL = 0x0001,

        /// <summary>
        ///     A 45-degree downward, left-to-right hatch.
        /// </summary>
        HS_FDIAGONAL = 0x0002,

        /// <summary>
        ///     A 45-degree upward, left-to-right hatch.
        /// </summary>
        HS_BDIAGONAL = 0x0003,

        /// <summary>
        ///     A horizontal and vertical cross-hatch.
        /// </summary>
        HS_CROSS = 0x0004,

        /// <summary>
        ///     A 45-degree crosshatch.
        /// </summary>
        HS_DIAGCROSS = 0x0005
    }
}
