namespace MiniPdf.Drawing.Metafile.EmfPlus.Records
{
    using System.IO;

    internal class EmfPlusFillRegionRecord : Record
    {
        public ushort BrushId => Header.Flags;

        public uint RegionId;

        public override void Read(BinaryReader reader)
        {
            if (Header.DataSize >= 4)
            {
                RegionId = reader.ReadUInt32();
                var remaining = (int)Header.DataSize - 4;
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
            writer.Write(RegionId);
            writer.Write(Data);
        }
    }
}
