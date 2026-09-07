using MiniPdf.Drawing.Metafile.Emf;

namespace MiniPdf.Drawing.Metafile.Emf.Records.StateRecordTypes
{
    using MiniPdf.Drawing.Metafile.Emf.Enumerations;
    using System.IO;

    internal class EMR_SCALEVIEWPORTEXTEX : Record
    {
        public int XNum;

        public int XDenom;

        public int YNum;

        public int YDenom;

        public EMR_SCALEVIEWPORTEXTEX()
        {
            Header = new RecordHeader { Type = RecordType.EMR_SCALEVIEWPORTEXTEX };
        }

        public override void Read(BinaryReader reader)
        {
            XNum = reader.ReadInt32();
            XDenom = reader.ReadInt32();
            YNum = reader.ReadInt32();
            YDenom = reader.ReadInt32();
        }

        public override void Write(BinaryWriter writer)
        {
            writer.Write(XNum);
            writer.Write(XDenom);
            writer.Write(YNum);
            writer.Write(YDenom);
        }
    }
}
