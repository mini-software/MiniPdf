namespace MiniSoftware.Drawing.Metafile.EmfPlus.Objects
{
    using System.IO;

    internal class EmfPlusRectFObject
    {
        public float X { get; set; }

        public float Y { get; set; }

        public float Width { get; set; }

        public float Height { get; set; }

        public void Read(BinaryReader reader)
        {
            X = reader.ReadSingle();
            Y = reader.ReadSingle();
            Width = reader.ReadSingle();
            Height = reader.ReadSingle();
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(X);
            writer.Write(Y);
            writer.Write(Width);
            writer.Write(Height);
        }
    }
}
