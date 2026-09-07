namespace MiniPdf.Drawing.Metafile.EmfPlus.Records
{
    using System.IO;
    using MiniPdf.Drawing.Metafile.EmfPlus.Enumerations;

    internal class EmfPlusSetPageTransformRecord : Record
    {
        public float PageScale;
        public EmfPlusUnitType PageUnit;

        public override void Read(BinaryReader reader)
        {
            if (Header.DataSize >= 8)
            {
                PageScale = reader.ReadSingle();
                PageUnit = (EmfPlusUnitType)reader.ReadUInt32();
                int remaining = (int)Header.DataSize - 8;
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
            writer.Write(PageScale);
            writer.Write((uint)PageUnit);
            writer.Write(Data);
        }
    }
}
