namespace MiniPdf.Drawing.Metafile.Wmf.Records.StateRecordTypes
{
    using MiniPdf.Drawing.Metafile.Wmf.Records;
    using System.IO;

    /// <summary>
    ///     The META_OFFSETVIEWPORTORG record moves the viewport origin in the playback device context by specified horizontal and vertical offsets.
    /// </summary>
    /// <remarks>
    ///     2.3.5.6 META_OFFSETVIEWPORTORG Record
    /// </remarks>
    internal class META_OFFSETVIEWPORTORG : Record
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
