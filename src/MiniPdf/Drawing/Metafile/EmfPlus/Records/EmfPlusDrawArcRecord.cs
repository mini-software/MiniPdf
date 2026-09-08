namespace MiniSoftware.Drawing.Metafile.EmfPlus.Records
{
    using System.IO;
    using MiniSoftware.Drawing.Metafile.EmfPlus.Objects;

    internal class EmfPlusDrawArcRecord : Record
    {
        public ushort PenId => Header.Flags;

        public EmfPlusRectFObject Rect { get; set; } = new();

        public float StartAngle;

        public float SweepAngle;

        public override void Read(BinaryReader reader)
        {
            if (Header.DataSize >= 24)
            {
                Rect.Read(reader);
                StartAngle = reader.ReadSingle();
                SweepAngle = reader.ReadSingle();

                int remaining = (int)Header.DataSize - 24;
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

        public override void Write(BinaryWriter writer)
        {
            Rect.Write(writer);
            writer.Write(StartAngle);
            writer.Write(SweepAngle);
            writer.Write(Data);
        }
    }
}
