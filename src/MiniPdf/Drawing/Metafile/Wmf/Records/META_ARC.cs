using System.IO;
using MiniSoftware.Drawing.Metafile.Wmf;
using MiniSoftware.Drawing.Metafile.Wmf.Enumerations;

namespace MiniSoftware.Drawing.Metafile.Wmf.Records
{
    internal class META_ARC : Record
    {
        public short YEnd { get; set; }
        public short XEnd { get; set; }
        public short YStart { get; set; }
        public short XStart { get; set; }
        public short BottomRect { get; set; }
        public short RightRect { get; set; }
        public short TopRect { get; set; }
        public short LeftRect { get; set; }

        public META_ARC()
        {
            Header.RecordFunction = RecordType.META_ARC;
        }

        public override void Read(BinaryReader reader)
        {
            YEnd = reader.ReadInt16();
            XEnd = reader.ReadInt16();
            YStart = reader.ReadInt16();
            XStart = reader.ReadInt16();
            BottomRect = reader.ReadInt16();
            RightRect = reader.ReadInt16();
            TopRect = reader.ReadInt16();
            LeftRect = reader.ReadInt16();
        }

        public override void Write(BinaryWriter writer)
        { 
            base.Write(writer);
            writer.Write(YEnd);
            writer.Write(XEnd);
            writer.Write(YStart);
            writer.Write(XStart);
            writer.Write(BottomRect);
            writer.Write(RightRect);
            writer.Write(TopRect);
            writer.Write(LeftRect);
        }
    }
}
