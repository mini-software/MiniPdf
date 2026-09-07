using System.IO;
using System.Collections.Generic;
using System.Drawing;
using MiniSoftware.Drawing.Metafile.Wmf.Enumerations;
using MiniSoftware.Drawing.Metafile.Wmf;

namespace MiniSoftware.Drawing.Metafile.Wmf.Records
{
    internal class META_POLYPOLYGON : Record
    {
        public short NumberOfPolygons => (short)Polygons.Count;
        public List<List<Point>> Polygons { get; } = new List<List<Point>>();

        public META_POLYPOLYGON()
        {
            Header.RecordFunction = RecordType.META_POLYPOLYGON;
        }

        public override void Read(BinaryReader reader)
        {
            short numberOfPolygons = reader.ReadInt16();

            var pointsPerPolygon = new int[numberOfPolygons];
            for (int i = 0; i < numberOfPolygons; i++)
            {
                pointsPerPolygon[i] = reader.ReadInt16();
            }

            for (int i = 0; i < numberOfPolygons; i++)
            {
                var polygon = new List<Point>();
                for (int j = 0; j < pointsPerPolygon[i]; j++)
                {
                    short x = reader.ReadInt16();
                    short y = reader.ReadInt16();
                    polygon.Add(new Point(x, y));
                }
                Polygons.Add(polygon);
            }
        }

        public override void Write(BinaryWriter writer)
        {
            base.Write(writer);
            writer.Write(NumberOfPolygons);

            foreach (var polygon in Polygons)
            {
                writer.Write((short)polygon.Count);
            }

            foreach (var polygon in Polygons)
            {
                foreach (Point point in polygon)
                {
                    writer.Write((short)point.X);
                    writer.Write((short)point.Y);
                }
            }
        }
    }
}
