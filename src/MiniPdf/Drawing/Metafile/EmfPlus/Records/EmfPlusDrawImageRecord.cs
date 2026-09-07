namespace MiniSoftware.Drawing.Metafile.EmfPlus.Records
{
    using System.IO;
    using MiniSoftware.Drawing.Metafile.EmfPlus.Enumerations;
    using MiniSoftware.Drawing.Metafile.EmfPlus.Objects;

    internal class EmfPlusDrawImageRecord : Record
    {
        public uint ImageAttributesId;

        public EmfPlusUnitType SourceUnit;

        public EmfPlusRectFObject SourceRect { get; set; } = new();

        public EmfPlusRectFObject DestRect { get; set; } = new();

        public override void Read(BinaryReader reader)
        {
            if (Header.DataSize >= 40)
            {
                ImageAttributesId = reader.ReadUInt32();
                SourceUnit = (EmfPlusUnitType)reader.ReadUInt32();
                SourceRect.Read(reader);
                DestRect.Read(reader);
                int remaining = (int)Header.DataSize - 40;
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
            writer.Write(ImageAttributesId);
            writer.Write((uint)SourceUnit);
            SourceRect.Write(writer);
            DestRect.Write(writer);
            writer.Write(Data);
        }
    }
}
