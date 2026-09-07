using MiniSoftware.Drawing.Metafile.Wmf;

namespace MiniSoftware.Drawing.Metafile.Wmf.Records
{
    using MiniSoftware.Drawing.Metafile.Wmf.Enumerations;
    using System.IO;

    /// <summary>
    ///     The META_FILLREGION record fills a region using a specified brush.
    /// </summary>
    /// <remarks>
    ///     2.3.3.6 META_FILLREGION Record
    /// </remarks>
    internal class META_FILLREGION : Record
    {
        /// <summary>
        ///     Used to index into the WMF Object Table to get the region to be filled.
        /// </summary>
        public ushort Region { get; set; }

        /// <summary>
        ///     Used to index into the WMF Object Table to get the brush to use for filling the region.
        /// </summary>
        public ushort Brush { get; set; }

        public META_FILLREGION()
        {
            Header.RecordFunction = RecordType.META_FILLREGION;
        }

        public override void Read(BinaryReader reader)
        {
            Region = reader.ReadUInt16();
            Brush = reader.ReadUInt16();
        }

        public override void Write(BinaryWriter writer)
        {
            base.Write(writer);
            writer.Write(Region);
            writer.Write(Brush);
        }
    }
}
