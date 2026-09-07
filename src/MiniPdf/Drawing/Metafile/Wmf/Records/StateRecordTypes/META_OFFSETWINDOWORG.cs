namespace MiniPdf.Drawing.Metafile.Wmf.Records.StateRecordTypes
{
    using MiniPdf.Drawing.Metafile.Wmf.Records;
    using System.IO;

    /// <summary>
    ///     The META_OFFSETWINDOWORG record moves the output window origin in the playback device context by specified horizontal and vertical offsets.
    /// </summary>
    /// <remarks>
    ///     2.3.5.7 META_OFFSETWINDOWORG Record
    /// </remarks>
    internal class META_OFFSETWINDOWORG : Record
    {
        /// <summary>
        ///     Defines the vertical offset, in device units.
        /// </summary>
        public short YOffset;

        /// <summary>
        ///     Defines the horizontal offset, in device units.
        /// </summary>
        public short XOffset;

        public override void Read(BinaryReader reader)
        {
            this.YOffset = reader.ReadInt16();
            this.XOffset = reader.ReadInt16();
        }
    }
}
