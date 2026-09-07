namespace MiniPdf.Drawing.Metafile.EmfPlus.Records
{
    using System;
    using System.IO;
    using System.Text;
    using MiniPdf.Drawing.Metafile.EmfPlus.Objects;

    internal class EmfPlusDrawStringRecord : Record
    {
        public uint BrushId { get; set; }

        public uint FormatId { get; set; }

        public uint Length { get; set; }

        public EmfPlusRectFObject LayoutRect { get; set; } = new();

        public string StringData { get; set; } = string.Empty;

        public override void Read(BinaryReader reader)
        {
            BrushId = reader.ReadUInt32();
            FormatId = reader.ReadUInt32();
            Length = reader.ReadUInt32();
            LayoutRect.Read(reader);
            StringData = Encoding.Unicode.GetString(reader.ReadBytes(Convert.ToInt32(Length * 2)));

            int bytesLeft = Convert.ToInt32(Header.DataSize - 28 - (Length * 2));
            if (bytesLeft > 0)
            {
                reader.ReadBytes(bytesLeft);
            }
        }

        public override void Write(BinaryWriter writer)
        {
            writer.Write(BrushId);
            writer.Write(FormatId);

            byte[] stringBytes = Encoding.Unicode.GetBytes(StringData);
            Length = (uint)StringData.Length;
            writer.Write(Length);

            LayoutRect.Write(writer);
            writer.Write(stringBytes);
        }
    }
}
