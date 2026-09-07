namespace MiniPdf.Drawing.Metafile.EmfPlus.Records
{
    using System.IO;
    using MiniPdf.Drawing.Metafile.EmfPlus.Objects;

    internal class EmfPlusDrawEllipseRecord : Record
    {
        public ushort PenId => Header.Flags;

        public EmfPlusRectFObject Rect { get; set; } = new();

        public override void Read(BinaryReader reader)
        {
            if (Header.DataSize >= 16)
            {
                Rect.Read(reader);
                int remaining = (int)Header.DataSize - 16;
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
            writer.Write(Data);
        }
    }
}
