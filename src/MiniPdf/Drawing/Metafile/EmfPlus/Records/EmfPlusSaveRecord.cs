namespace MiniPdf.Drawing.Metafile.EmfPlus.Records
{
    using System.IO;

    internal class EmfPlusSaveRecord : Record
    {
        public uint StackIndex;

        public override void Read(BinaryReader reader)
        {
            if (Header.DataSize >= 4)
            {
                StackIndex = reader.ReadUInt32();
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
            writer.Write(StackIndex);
            writer.Write(Data);
        }
    }
}
