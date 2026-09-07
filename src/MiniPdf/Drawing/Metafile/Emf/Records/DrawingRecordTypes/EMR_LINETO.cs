using MiniSoftware.Drawing.Metafile.Emf;
using MiniSoftware.Drawing.Metafile.Emf.Records;

namespace MiniSoftware.Drawing.Metafile.Emf.Records.DrawingRecordTypes
{
    using System.IO;
    using MiniSoftware.Drawing.Metafile.Emf.Enumerations;
    using MiniSoftware.Drawing.Metafile.Wmf.Objects;

    internal class EMR_LINETO : Record
    {
        public PointL Point;

        public EMR_LINETO()
        {
            Header = new RecordHeader { Type = RecordType.EMR_LINETO };
        }

        public override void Read(BinaryReader reader)
        {
            Point = new PointL();
            Point.Read(reader);
        }

        public override void Write(BinaryWriter writer)
        {
            Point.Write(writer);
        }
    }
}
