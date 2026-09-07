namespace MiniPdf.Drawing.Metafile.EmfPlus.Records
{
    using System.IO;
    using MiniPdf.Drawing.Metafile.EmfPlus.Enumerations;

    internal class EmfPlusSetClipPathRecord : Record
    {
        public uint PathId;

        public EmfPlusCombineMode CombineMode => (EmfPlusCombineMode)(Header.Flags & 0x000F);

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
