namespace MiniPdf.Drawing.Metafile.Wmf.Enumerations
{
    /// <summary>
    ///     Specifies how the palette entry should be used.
    /// </summary>
    /// <remarks>
    ///     2.1.1.22 PaletteEntryFlag Enumeration
    /// </remarks>
    internal enum PaletteEntryFlag
    {
        /// <summary>
        ///     Specifies that the logical palette entry be used for palette animation. This value prevents other windows from matching colors to the palette entry because the color frequently changes. If an unused system-palette entry is available, the color is placed in that entry. 
        ///     Otherwise, the color is not available for animation.
        /// </summary>
        PC_RESERVED = 0x01,

        /// <summary>
        ///     Specifies that the low-order word of the logical palette entry designates a hardware palette index. 
        ///     This value allows the application to show the contents of the display device palette.
        /// </summary>
        PC_EXPLICIT = 0x02,

        /// <summary>
        ///     Specifies that the color be placed in an unused entry in the system palette instead of being matched to an existing color in the system palette. 
        ///     If there are no unused entries in the system palette, the color is matched normally. Once this color is in the system palette, colors in other logical palettes can be matched to this color.
        /// </summary>
        PC_NOCOLLAPSE = 0x04
    }
}
