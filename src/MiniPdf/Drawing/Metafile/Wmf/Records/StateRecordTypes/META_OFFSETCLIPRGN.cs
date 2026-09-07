namespace MiniPdf.Drawing.Metafile.Wmf.Records.StateRecordTypes
{
    using MiniPdf.Drawing.Metafile.Wmf.Records;
    using System.IO;

    /// <summary>
    ///     The META_OFFSETCLIPRGN record moves the clipping region in the playback device context by the specified offsets.
    /// </summary>
    /// <remarks>
    ///     2.3.5.5 META_OFFSETCLIPRGN Record
    /// </remarks>
    internal class META_OFFSETCLIPRGN : Record
    {
        /// <summary>
        ///     Defines the number of logical units to move up or down.
        /// </summary>
        public short YOffset;

        /// <summary>
        ///     Defines the number of logical units to move left or right.
        /// </summary>
        public short XOffset;

        public override void Read(BinaryReader reader)
        {
            this.YOffset = reader.ReadInt16();
            this.XOffset = reader.ReadInt16();
        }
    }
}
