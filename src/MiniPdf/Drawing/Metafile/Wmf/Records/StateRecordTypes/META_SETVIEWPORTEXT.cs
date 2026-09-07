using MiniPdf.Drawing.Metafile.Wmf;

namespace MiniPdf.Drawing.Metafile.Wmf.Records.StateRecordTypes
{
    using MiniPdf.Drawing.Metafile.Wmf.Enumerations;
    using MiniPdf.Drawing.Metafile.Wmf.Records;
    using System.IO;

    internal class META_SETVIEWPORTEXT : Record
    {
        public short Y;

        public short X;

        public META_SETVIEWPORTEXT()
        {
            Header.RecordFunction = RecordType.META_SETVIEWPORTEXT;
        }

        public override void Read(BinaryReader reader)
        {
            Y = reader.ReadInt16();
            X = reader.ReadInt16();
        }

        public override void Write(BinaryWriter writer)
        {
            base.Write(writer);
            writer.Write(Y);
            writer.Write(X);
        }
    }
}
