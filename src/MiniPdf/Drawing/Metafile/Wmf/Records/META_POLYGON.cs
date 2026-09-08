using System.IO;
using System.Collections.Generic;

using MiniSoftware.Drawing.Metafile.Wmf.Enumerations;
using MiniSoftware.Drawing.Metafile.Wmf;
using MiniSoftware.Drawing.Geometry;

namespace MiniSoftware.Drawing.Metafile.Wmf.Records
{
    internal class META_POLYGON : Record
    {
        public short NumberOfPoints => (short)Points.Count;
        public List<Point> Points { get; } = new List<Point>();

        public META_POLYGON()
        {
            Header.RecordFunction = RecordType.META_POLYGON;
        }

        public override void Read(BinaryReader reader)
        {
            short numberOfPoints = reader.ReadInt16();
            for (int i = 0; i < numberOfPoints; i++)
            {
                short x = reader.ReadInt16();
                short y = reader.ReadInt16();
                Points.Add(new Point(x, y));
            }
        }

        public override void Write(BinaryWriter writer)
        {
            base.Write(writer);
            writer.Write(NumberOfPoints);
            foreach (Point point in Points)
            {
                writer.Write((short)point.X);
                writer.Write((short)point.Y);
            }
        }
    }
}
