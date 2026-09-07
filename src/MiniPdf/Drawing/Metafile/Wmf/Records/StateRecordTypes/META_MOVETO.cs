namespace MiniPdf.Drawing.Metafile.Wmf.Records.StateRecordTypes
{
    using MiniPdf.Drawing.Metafile.Wmf.Records;
    using System.IO;

    /// <summary>
    ///     The META_MOVETO record sets the output position in the playback device context to a specified point.
    /// </summary>
    /// <remarks>
    ///     2.3.5.4 META_MOVETO Record
    /// </remarks>
    internal class META_MOVETO : Record
    {
        /// <summary>
        ///     Defines the y-coordinate, in logical units.
        /// </summary>
        public short Y;

        /// <summary>
        ///     Defines the x-coordinate, in logical units.
        /// </summary>
        public short X;

        public override void Read(BinaryReader reader)
        {
            this.Y = reader.ReadInt16();
            this.X = reader.ReadInt16();
        }
    }
}
