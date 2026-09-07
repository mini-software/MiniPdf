namespace MiniPdf.Drawing.Metafile.EmfPlus.Records
{
    using System.IO;
    using MiniPdf.Drawing.Metafile.EmfPlus.Enumerations;
    using MiniPdf.Drawing.Metafile.EmfPlus.Objects;

    internal class EmfPlusSetClipRectRecord : Record
    {
        public EmfPlusRectFObject ClipRect { get; set; } = new();

        public EmfPlusCombineMode CombineMode => (EmfPlusCombineMode)(Header.Flags & 0x000F);

        public override void Read(BinaryReader reader)
        {
            if (Header.DataSize >= 16)
            {
                ClipRect.Read(reader);
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
            ClipRect.Write(writer);
            writer.Write(Data);
        }
    }
}
