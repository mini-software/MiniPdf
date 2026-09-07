namespace MiniPdf.Drawing.Metafile.Emf.Objects
{
    using System.IO;

    internal class GradientTriangle
    {
        public uint Vertex1;

        public uint Vertex2;

        public uint Vertex3;

        public void Read(BinaryReader reader)
        {
            Vertex1 = reader.ReadUInt32();
            Vertex2 = reader.ReadUInt32();
            Vertex3 = reader.ReadUInt32();
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(Vertex1);
            writer.Write(Vertex2);
            writer.Write(Vertex3);
        }
    }
}
