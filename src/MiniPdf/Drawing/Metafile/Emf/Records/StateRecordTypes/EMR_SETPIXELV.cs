using MiniPdf.Drawing.Metafile.Emf;
using MiniPdf.Drawing.Metafile.Emf.Records;

namespace MiniPdf.Drawing.Metafile.Emf.Records.StateRecordTypes
{
    using System.IO;
    using MiniPdf.Drawing.Metafile.Emf.Enumerations;
    using MiniPdf.Drawing.Metafile.Wmf.Objects;

    internal class EMR_SETPIXELV : Record
    {
        public PointL Position;

        public ColorRef Color;

        public EMR_SETPIXELV()
        {
            Header = new RecordHeader { Type = RecordType.EMR_SETPIXELV };
        }

        public override void Read(BinaryReader reader)
        {
            Position = new PointL();
            Position.Read(reader);
            Color.Read(reader);
        }

        public override void Write(BinaryWriter writer)
        {
            Position.Write(writer);
            Color.Write(writer);
        }
    }
}

