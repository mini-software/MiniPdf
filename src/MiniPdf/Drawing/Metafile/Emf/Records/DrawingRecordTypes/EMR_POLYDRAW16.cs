using MiniPdf.Drawing.Metafile.Emf;
using MiniPdf.Drawing.Metafile.Emf.Records;

namespace MiniPdf.Drawing.Metafile.Emf.Records.DrawingRecordTypes
{
    using System;
    using System.IO;
    using MiniPdf.Drawing.Metafile.Emf.Enumerations;
    using MiniPdf.Drawing.Metafile.Wmf.Objects;

    internal class EMR_POLYDRAW16 : Record
    {
        public RectL Bounds = new();

        public uint Count;

        public PointS[] Points { get; set; } = Array.Empty<PointS>();

        public byte[] AbTypes { get; set; } = Array.Empty<byte>();

        public EMR_POLYDRAW16()
        {
            Header = new RecordHeader { Type = RecordType.EMR_POLYDRAW16 };
        }

        public override void Read(BinaryReader reader)
        {
            Bounds.Read(reader);
            Count = reader.ReadUInt32();

            Points = new PointS[Count];
            for (int i = 0; i < Count; i++)
            {
                var point = new PointS();
                point.Read(reader);
                Points[i] = point;
            }

            AbTypes = reader.ReadBytes(Convert.ToInt32(Count));
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

            if (AbTypes.Length == count)
            {
                writer.Write(AbTypes);
            }
            else
            {
                for (int i = 0; i < count; i++)
                {
                    writer.Write((byte)0);
                }
            }
        }
    }
}
