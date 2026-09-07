using MiniPdf.Drawing.Metafile.Wmf;

namespace MiniPdf.Drawing.Metafile.Wmf.Records.StateRecordTypes
{
    using MiniPdf.Drawing.Metafile.Wmf.Enumerations;
    using MiniPdf.Drawing.Metafile.Wmf.Records;
    using System.IO;

    internal class META_SETRELABS : Record
    {
        public ushort Relative;

        public META_SETRELABS()
        {
            Header.RecordFunction = RecordType.META_SETRELABS;
        }

        public override void Read(BinaryReader reader)
        {
            Relative = reader.ReadUInt16();
        }

        public override void Write(BinaryWriter writer)
        {
            base.Write(writer);
            writer.Write(Relative);
        }
    }
}
