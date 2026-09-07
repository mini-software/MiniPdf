using System.IO;
using MiniPdf.Drawing.Metafile.Wmf;
using MiniPdf.Drawing.Metafile.Wmf.Enumerations;

namespace MiniPdf.Drawing.Metafile.Wmf.Records
{
    internal class META_CHORD : Record
    {
        public short YEndRadial { get; set; }
        public short XEndRadial { get; set; }
        public short YStartRadial { get; set; }
        public short XStartRadial { get; set; }
        public short BottomRect { get; set; }
        public short RightRect { get; set; }
        public short TopRect { get; set; }
        public short LeftRect { get; set; }

        public META_CHORD()
        {
            Header.RecordFunction = RecordType.META_CHORD;
        }

        public override void Read(BinaryReader reader)
        {
            YEndRadial = reader.ReadInt16();
            XEndRadial = reader.ReadInt16();
            YStartRadial = reader.ReadInt16();
            XStartRadial = reader.ReadInt16();
            BottomRect = reader.ReadInt16();
            RightRect = reader.ReadInt16();
            TopRect = reader.ReadInt16();
            LeftRect = reader.ReadInt16();
        }

        public override void Write(BinaryWriter writer)
        {
            base.Write(writer);
            writer.Write(YEndRadial);
            writer.Write(XEndRadial);
            writer.Write(YStartRadial);
            writer.Write(XStartRadial);
            writer.Write(BottomRect);
            writer.Write(RightRect);
            writer.Write(TopRect);
            writer.Write(LeftRect);
        }
    }
}
