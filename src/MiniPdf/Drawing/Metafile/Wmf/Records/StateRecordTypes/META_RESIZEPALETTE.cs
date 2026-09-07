namespace MiniPdf.Drawing.Metafile.Wmf.Records.StateRecordTypes
{
    /// <summary>
    ///     The META_RESIZEPALETTE record redefines the size of the logical palette that is defined in the playback device context.
    /// </summary>
    /// <remarks>
    ///     2.3.5.9 META_RESIZEPALETTE Record
    /// </remarks>
    internal class META_RESIZEPALETTE : Record
    {
        /// <summary>
        ///     Defines the number of entries in the logical palette.
        /// </summary>
        public ushort NumberOfEntries;
    }
}
