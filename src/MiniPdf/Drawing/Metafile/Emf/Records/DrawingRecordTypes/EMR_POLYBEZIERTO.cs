using MiniPdf.Drawing.Metafile.Emf;

namespace MiniPdf.Drawing.Metafile.Emf.Records.DrawingRecordTypes
{
    using System;
    using System.IO;
    using MiniPdf.Drawing.Metafile.Emf.Enumerations;
    using MiniPdf.Drawing.Metafile.Wmf.Objects;

    internal class EMR_POLYBEZIERTO : Record
    {
        public RectL Bounds = new();

        public uint Count;

        public PointL[] Points { get; set; } = Array.Empty<PointL>();

        public EMR_POLYBEZIERTO()
        {
            Header = new RecordHeader { Type = RecordType.EMR_POLYBEZIERTO };
        }

        public override void Read(BinaryReader reader)
        {
            Bounds.Read(reader);
            Count = reader.ReadUInt32();
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
            uint count = Count == 0 ? (uint)Points.Length : Count;
            Bounds.Write(writer);
            writer.Write(count);
            for (int i = 0; i < count; i++)
            {
                Points[i].Write(writer);
            }
        }
    }
}
