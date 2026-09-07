using MiniSoftware.Drawing.Metafile.Emf;
using MiniSoftware.Drawing.Metafile.Emf.Records;

namespace MiniSoftware.Drawing.Metafile.Emf.Records.StateRecordTypes
{
    using System.IO;
    using MiniSoftware.Drawing.Metafile.Emf.Enumerations;
    using MiniSoftware.Drawing.Metafile.Wmf.Objects;

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

