using MiniSoftware.Drawing.Metafile.Emf;

namespace MiniSoftware.Drawing.Metafile.Emf.Records.DrawingRecordTypes
{
    using System;
    using System.IO;
    using MiniSoftware.Drawing.Metafile.Emf.Enumerations;
    using MiniSoftware.Drawing.Metafile.Wmf.Objects;

    internal class EMR_POLYPOLYLINE : Record
    {
        public RectL Bounds = new();

        public uint NumberOfPolys;

        public uint Count;

        public uint[] PolyCounts { get; set; } = Array.Empty<uint>();

        public PointL[] Points { get; set; } = Array.Empty<PointL>();

        public EMR_POLYPOLYLINE()
        {
            Header = new RecordHeader { Type = RecordType.EMR_POLYPOLYLINE };
        }

        public override void Read(BinaryReader reader)
        {
            Bounds.Read(reader);
            NumberOfPolys = reader.ReadUInt32();
            Count = reader.ReadUInt32();

            PolyCounts = new uint[NumberOfPolys];
            for (int i = 0; i < NumberOfPolys; i++)
            {
                PolyCounts[i] = reader.ReadUInt32();
            }

            Points = new PointL[Count];
            for (int i = 0; i < Count; i++)
            {
                var point = new PointL();
                point.Read(reader);
                Points[i] = point;
            }
        }

        public override void Write(BinaryWriter writer)
        {
            uint numberOfPolys = NumberOfPolys == 0 ? (uint)PolyCounts.Length : NumberOfPolys;
            uint count = Count == 0 ? (uint)Points.Length : Count;

            Bounds.Write(writer);
            writer.Write(numberOfPolys);
            writer.Write(count);
            for (int i = 0; i < numberOfPolys; i++)
            {
                writer.Write(PolyCounts[i]);
            }

            for (int i = 0; i < count; i++)
            {
                Points[i].Write(writer);
            }
        }
    }
}
