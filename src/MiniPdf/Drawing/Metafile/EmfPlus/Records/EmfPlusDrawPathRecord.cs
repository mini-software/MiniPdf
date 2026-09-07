namespace MiniPdf.Drawing.Metafile.EmfPlus.Records
{
    using System.IO;

    internal class EmfPlusDrawPathRecord : Record
    {
        public ushort PenId => Header.Flags;

        public uint PathId;

        public override void Read(BinaryReader reader)
        {
            if (Header.DataSize >= 4)
            {
                PathId = reader.ReadUInt32();
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
            writer.Write(PathId);
            writer.Write(Data);
        }
    }
}
