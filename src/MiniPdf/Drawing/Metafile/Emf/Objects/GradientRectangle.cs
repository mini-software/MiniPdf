namespace MiniSoftware.Drawing.Metafile.Emf.Objects
{
    using System.IO;

    internal class GradientRectangle
    {
        public uint UpperLeft;

        public uint LowerRight;

        public void Read(BinaryReader reader)
        {
            UpperLeft = reader.ReadUInt32();
            LowerRight = reader.ReadUInt32();
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(UpperLeft);
            writer.Write(LowerRight);
        }
    }
}
