namespace MiniPdf.Drawing.Metafile.Wmf.Enumerations
{
    /// <summary>
    ///     Specifies where the metafile is stored.
    /// </summary>
    /// <remarks>
    ///     2.1.1.18 MetafileType Enumeration
    /// </remarks>
    internal enum MetafileType
    {
        /// <summary>
        ///     Metafile is stored in memory.
        /// </summary>
        MEMORYMETAFILE = 0x0001,

        /// <summary>
        ///     Metafile is stored on disk.
        /// </summary>
        DISKMETAFILE = 0x0002
    }
}
