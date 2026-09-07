namespace MiniPdf.Drawing.Metafile.Wmf.Records.ObjectRecordTypes
{
    using System.IO;

    /// <summary>
    ///     The META_SELECTPALETTE record defines the current logical palette with a specified Palette Object.
    /// </summary>
    /// <remarks>
    ///     2.3.4.11 META_SELECTPALETTE Record
    /// </remarks>
    internal class META_SELECTPALETTE : Record
    {
        /// <summary>
        ///     A 16-bit unsigned integer used to index into the WMF Object Table to get the Palette Object to be selected.
        /// </summary>
        public ushort Palette;

        public override void Read(BinaryReader reader)
        {
            this.Palette = reader.ReadUInt16();
        }
    }
}
