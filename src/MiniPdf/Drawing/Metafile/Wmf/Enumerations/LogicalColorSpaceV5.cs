namespace MiniSoftware.Drawing.Metafile.Wmf.Enumerations
{
    /// <summary>
    ///     Used to specify where to find color profile information for a DeviceIndependentBitmap (DIB) Object (section 2.2.2.9) that has a header of type BitmapV5Header Object (section 2.2.2.5).
    /// </summary>
    /// <remarks>
    ///     2.1.1.15 LogicalColorSpaceV5 Enumeration
    /// </remarks>
    internal enum LogicalColorSpaceV5
    {
        /// <summary>
        ///     The value consists of the string "LINK" from the Windows character set (code page 1252). 
        ///     It indicates that the color profile MUST be linked with the DIB Object.
        /// </summary>
        LCS_PROFILE_LINKED = 0x4C494E4B,

        /// <summary>
        ///     The value consists of the string "MBED" from the Windows character set (code page 1252). 
        ///     It indicates that the color profile MUST be embedded in the DIB Object.
        /// </summary>
        LCS_PROFILE_EMBEDDED = 0x4D424544
    }
}
