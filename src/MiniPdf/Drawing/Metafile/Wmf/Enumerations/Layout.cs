namespace MiniPdf.Drawing.Metafile.Wmf.Enumerations
{
    /// <summary>
    ///     Defines options for controlling the direction in which text and graphics are drawn.
    /// </summary>
    /// <remarks>
    ///     2.1.1.13 Layout Enumeration
    /// </remarks>
    internal enum Layout
    {
        /// <summary>
        ///     Sets the default horizontal layout to be left-to-right.
        /// </summary>
        LAYOUT_LTR = 0x0000,

        /// <summary>
        ///     Sets the default horizontal layout to be right-to-left. Switching to this layout SHOULD cause the mapping mode in the playback device context to become MM_ISOTROPIC (section 2.1.1.16).
        /// </summary>
        LAYOUT_RTL = 0x0001,

        /// <summary>
        ///     Disables mirroring of bitmaps that are drawn by META_BITBLT and META_STRETCHBLT operations, when the layout is right-to-left.
        /// </summary>
        LAYOUT_BITMAPORIENTATIONPRESERVED = 0x0008
    }
}
