using MiniPdf.Drawing.Metafile.Wmf;

namespace MiniPdf.Drawing.Metafile.Wmf.Records.StateRecordTypes
{
    using MiniPdf.Drawing.Metafile.Wmf.Enumerations;
    using MiniPdf.Drawing.Metafile.Wmf.Records;
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
