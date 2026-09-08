using MiniSoftware.Drawing.Metafile.Wmf;

namespace MiniSoftware.Drawing.Metafile.Wmf.Records.StateRecordTypes
{
    using MiniSoftware.Drawing.Metafile.Wmf.Enumerations;
    using MiniSoftware.Drawing.Metafile.Wmf.Records;
    using System.IO;

    internal class META_SETROP2 : Record
    {
        public BinaryRasterOperation DrawMode;

        public META_SETROP2()
        {
            Header.RecordFunction = RecordType.META_SETROP2;
        }

        public override void Read(BinaryReader reader)
        {
            DrawMode = (BinaryRasterOperation)reader.ReadUInt16();
        }

        public override void Write(BinaryWriter writer)
        {
            base.Write(writer);
            writer.Write((ushort)DrawMode);
        }
    }
}
