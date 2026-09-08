namespace MiniSoftware.Drawing.Metafile.EmfPlus.Records
{
    using System;
    using System.IO;
    using MiniSoftware.Drawing.Metafile.EmfPlus.Objects;

    internal class EmfPlusFillRectsRecord : Record
    {
        public ushort BrushId => Header.Flags;

        public EmfPlusRectFObject[] Rects { get; set; } = [];

        public override void Read(BinaryReader reader)
        {
            if (Header.DataSize >= 16)
            {
                int count = (int)(Header.DataSize / 16);
                Rects = new EmfPlusRectFObject[count];
                for (int i = 0; i < count; i++)
                {
                    var rect = new EmfPlusRectFObject();
                    rect.Read(reader);
                    Rects[i] = rect;
                }

                int remaining = (int)Header.DataSize - (count * 16);
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
            foreach (var rect in Rects)
            {
                rect.Write(writer);
            }
            writer.Write(Data);
        }
    }
}
