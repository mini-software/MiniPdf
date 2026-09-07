namespace MiniSoftware.Drawing.Metafile.EmfPlus.Records
{
    using System.IO;

    internal class EmfPlusSetRenderingOriginRecord : Record
    {
        public int X;
        public int Y;

        public override void Read(BinaryReader reader)
        {
            if (Header.DataSize >= 8)
            {
                X = reader.ReadInt32();
                Y = reader.ReadInt32();
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
            writer.Write(X);
            writer.Write(Y);
            writer.Write(Data);
        }
    }
}
