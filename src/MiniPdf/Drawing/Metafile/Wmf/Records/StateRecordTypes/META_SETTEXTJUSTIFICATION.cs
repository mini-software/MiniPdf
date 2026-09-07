using MiniSoftware.Drawing.Metafile.Wmf;

namespace MiniSoftware.Drawing.Metafile.Wmf.Records.StateRecordTypes
{
    using MiniSoftware.Drawing.Metafile.Wmf.Enumerations;
    using MiniSoftware.Drawing.Metafile.Wmf.Records;
    using System.IO;

    internal class META_SETTEXTJUSTIFICATION : Record
    {
        public short BreakExtra;

        public short BreakCount;

        public META_SETTEXTJUSTIFICATION()
        {
            Header.RecordFunction = RecordType.META_SETTEXTJUSTIFICATION;
        }

        public override void Read(BinaryReader reader)
        {
            BreakExtra = reader.ReadInt16();
            BreakCount = reader.ReadInt16();
        }

        public override void Write(BinaryWriter writer)
        {
            base.Write(writer);
            writer.Write(BreakExtra);
            writer.Write(BreakCount);
        }
    }
}
