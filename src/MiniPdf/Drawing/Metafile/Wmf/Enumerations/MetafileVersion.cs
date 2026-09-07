namespace MiniPdf.Drawing.Metafile.Wmf.Enumerations
{
    /// <summary>
    ///     Defines values that specify support for device-independent bitmaps (DIBs) in metafiles.
    /// </summary>
    /// <remarks>
    ///     2.1.1.19 MetafileVersion Enumeration
    /// </remarks>
    internal enum MetafileVersion
    {
        /// <summary>
        ///     DIBs are not supported.
        /// </summary>
        METAVERSION100 = 0x0100,

        /// <summary>
        ///     DIBs are supported.
        /// </summary>
        METAVERSION300 = 0x0300
    }
}
