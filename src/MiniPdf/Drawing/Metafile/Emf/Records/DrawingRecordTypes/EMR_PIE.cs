using MiniSoftware.Drawing.Metafile.Emf;
using MiniSoftware.Drawing.Metafile.Emf.Records;

namespace MiniSoftware.Drawing.Metafile.Emf.Records.DrawingRecordTypes
{
    using System.IO;
    using MiniSoftware.Drawing.Metafile.Emf.Enumerations;
    using MiniSoftware.Drawing.Metafile.Wmf.Objects;

    internal class EMR_PIE : Record
    {
        public RectL Box;

        public PointL Start;

        public PointL End;

        public EMR_PIE()
        {
            Header = new RecordHeader { Type = RecordType.EMR_PIE };
        }

        public override void Read(BinaryReader reader)
        {
            Box = new RectL();
            Box.Read(reader);
            Start = new PointL();
            Start.Read(reader);
            End = new PointL();
            End.Read(reader);
        }

        public override void Write(BinaryWriter writer)
        {
            Box.Write(writer);
            Start.Write(writer);
            End.Write(writer);
        }
    }
}
