using MiniPdf.Drawing.Metafile.Emf;
using MiniPdf.Drawing.Metafile.Emf.Records;

namespace MiniPdf.Drawing.Metafile.Emf.Records.StateRecordTypes
{
    using MiniPdf.Drawing.Metafile.Emf.Enumerations;
    using System.IO;

    internal class EMR_SCALEWINDOWEXTEX : Record
    {
        public int XNum;

        public int XDenom;

        public int YNum;

        public int YDenom;

        public EMR_SCALEWINDOWEXTEX()
        {
            Header = new RecordHeader { Type = RecordType.EMR_SCALEWINDOWEXTEX };
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
