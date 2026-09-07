namespace MiniSoftware.Drawing.Metafile.EmfPlus.Records
{
    using System;
    using System.IO;
    using MiniSoftware.Drawing.Metafile.EmfPlus.Enumerations;

    internal class EmfPlusDrawImagePointsRecord : Record
    {
        public uint ImageAttributesId;

        public EmfPlusUnitType SourceUnit;

        public float SourceX;

        public float SourceY;

        public float SourceWidth;

        public float SourceHeight;

        public uint PointCount;

        public float[] Points { get; set; } = [];

        public override void Read(BinaryReader reader)
        {
            if (Header.DataSize >= 32)
            {
                ImageAttributesId = reader.ReadUInt32();
                SourceUnit = (EmfPlusUnitType)reader.ReadUInt32();
                SourceX = reader.ReadSingle();
                SourceY = reader.ReadSingle();
                SourceWidth = reader.ReadSingle();
                SourceHeight = reader.ReadSingle();
                PointCount = reader.ReadUInt32();

                int pointsFloatCount = (int)(PointCount * 2);
                int expectedPointBytes = pointsFloatCount * 4;
                if (Header.DataSize >= 32 + expectedPointBytes)
                {
                    Points = new float[pointsFloatCount];
                    for (int i = 0; i < pointsFloatCount; i++)
                    {
                        Points[i] = reader.ReadSingle();
                    }

                    int remaining = (int)Header.DataSize - 32 - expectedPointBytes;
                    if (remaining > 0)
                    {
                        Data = reader.ReadBytes(remaining);
                    }
                }
                else
                {
                    base.Read(reader);
                }
            }
            else
            {
                base.Read(reader);
            }
        }

        public override void Write(BinaryWriter writer)
        {
            uint pointCount = PointCount == 0 ? (uint)(Points.Length / 2) : PointCount;
            writer.Write(ImageAttributesId);
            writer.Write((uint)SourceUnit);
            writer.Write(SourceX);
            writer.Write(SourceY);
            writer.Write(SourceWidth);
            writer.Write(SourceHeight);
            writer.Write(pointCount);
            int pointFloatCount = (int)(pointCount * 2);
            for (int i = 0; i < pointFloatCount; i++)
            {
                writer.Write(Points[i]);
            }
            writer.Write(Data);
        }
    }
}
