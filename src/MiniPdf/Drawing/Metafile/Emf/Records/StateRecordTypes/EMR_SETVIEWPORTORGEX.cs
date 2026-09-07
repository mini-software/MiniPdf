using MiniPdf.Drawing.Metafile.Emf;

namespace MiniPdf.Drawing.Metafile.Emf.Records.StateRecordTypes
{
    using MiniPdf.Drawing.Metafile.Emf.Enumerations;
    using System.IO;

    internal class EMR_SETVIEWPORTORGEX : Record
    {
        public int X;

        public int Y;

        public EMR_SETVIEWPORTORGEX()
        {
            Header = new RecordHeader { Type = RecordType.EMR_SETVIEWPORTORGEX };
        }

        public override void Read(BinaryReader reader)
        {
            X = reader.ReadInt32();
            Y = reader.ReadInt32();
        }

        public override void Write(BinaryWriter writer)
        {
            writer.Write(X);
            writer.Write(Y);
        }
    }
}

