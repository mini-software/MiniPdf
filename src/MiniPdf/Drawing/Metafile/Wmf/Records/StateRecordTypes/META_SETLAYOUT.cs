using MiniSoftware.Drawing.Metafile.Wmf;

namespace MiniSoftware.Drawing.Metafile.Wmf.Records.StateRecordTypes
{
    using MiniSoftware.Drawing.Metafile.Wmf.Enumerations;
    using MiniSoftware.Drawing.Metafile.Wmf.Records;
    using System.IO;

    internal class META_SETLAYOUT : Record
    {
        public Layout Layout;

        public ushort Reserved;

        public META_SETLAYOUT()
        {
            Header.RecordFunction = RecordType.META_SETLAYOUT;
        }

        public override void Read(BinaryReader reader)
        {
            Layout = (Layout)reader.ReadUInt16();
            Reserved = reader.ReadUInt16();
        }

        public override void Write(BinaryWriter writer)
        {
            base.Write(writer);
            writer.Write((ushort)Layout);
            writer.Write(Reserved);
        }
    }
}
