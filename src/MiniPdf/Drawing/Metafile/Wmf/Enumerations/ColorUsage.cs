namespace MiniPdf.Drawing.Metafile.Wmf.Enumerations
{
    /// <summary>
    ///     Specifies whether a color table exists in a device-independent bitmap (DIB) and how to interpret its values.
    /// </summary>
    internal enum ColorUsage
    {
        /// <summary>
        ///     The color table contains RGB values specified by RGBQuad Objects (section 2.2.2.20).
        /// </summary>
        DIB_RGB_COLORS = 0x0000,

        /// <summary>
        ///     The color table contains 16-bit indices into the current logical palette in the playback device context.
        /// </summary>
        DIB_PAL_COLORS = 0x0001,

        /// <summary>
        ///     No color table exists. The pixels in the DIB are indices into the current logical palette in the playback device context.
        /// </summary>
        DIB_PAL_INDICES = 0x0002
    }
}
