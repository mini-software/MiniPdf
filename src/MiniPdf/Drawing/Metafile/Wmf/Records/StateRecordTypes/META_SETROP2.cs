using MiniPdf.Drawing.Metafile.Wmf;

namespace MiniPdf.Drawing.Metafile.Wmf.Records.StateRecordTypes
{
    using MiniPdf.Drawing.Metafile.Wmf.Enumerations;
    using MiniPdf.Drawing.Metafile.Wmf.Records;
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
