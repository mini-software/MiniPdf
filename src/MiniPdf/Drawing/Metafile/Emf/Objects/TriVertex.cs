namespace MiniPdf.Drawing.Metafile.Emf.Objects
{
    using System.IO;

    internal class TriVertex
    {
        public int X;

        public int Y;

        public ushort Red;

        public ushort Green;

        public ushort Blue;

        public ushort Alpha;

        public void Read(BinaryReader reader)
        {
            X = reader.ReadInt32();
            Y = reader.ReadInt32();
            Red = reader.ReadUInt16();
            Green = reader.ReadUInt16();
            Blue = reader.ReadUInt16();
            Alpha = reader.ReadUInt16();
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(X);
            writer.Write(Y);
            writer.Write(Red);
            writer.Write(Green);
            writer.Write(Blue);
            writer.Write(Alpha);
        }
    }
}
