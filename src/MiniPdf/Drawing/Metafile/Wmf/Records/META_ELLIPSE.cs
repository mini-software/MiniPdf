using System.IO;
using MiniSoftware.Drawing.Metafile.Wmf;
using MiniSoftware.Drawing.Metafile.Wmf.Enumerations;

namespace MiniSoftware.Drawing.Metafile.Wmf.Records
{
    internal class META_ELLIPSE : Record
    {
        public short BottomRect { get; set; }
        public short RightRect { get; set; }
        public short TopRect { get; set; }
        public short LeftRect { get; set; }

        public META_ELLIPSE()
        {
            Header.RecordFunction = RecordType.META_ELLIPSE;
        }

        public override void Read(BinaryReader reader)
        {
            BottomRect = reader.ReadInt16();
            RightRect = reader.ReadInt16();
            TopRect = reader.ReadInt16();
            LeftRect = reader.ReadInt16();
        }

        public override void Write(BinaryWriter writer)
        {
            base.Write(writer);
            writer.Write(BottomRect);
            writer.Write(RightRect);
            writer.Write(TopRect);
            writer.Write(LeftRect);
        }
    }
}
