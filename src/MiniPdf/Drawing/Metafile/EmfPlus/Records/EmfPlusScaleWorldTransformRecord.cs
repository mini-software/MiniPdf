namespace MiniSoftware.Drawing.Metafile.EmfPlus.Records
{
    using System.IO;
    using MiniSoftware.Drawing.Metafile.EmfPlus.Enumerations;

    internal class EmfPlusScaleWorldTransformRecord : Record
    {
        public float Sx;
        public float Sy;

        public EmfPlusMatrixOrder MatrixOrder => (EmfPlusMatrixOrder)(Header.Flags & 0x0001);

        public override void Read(BinaryReader reader)
        {
            if (Header.DataSize >= 8)
            {
                Sx = reader.ReadSingle();
                Sy = reader.ReadSingle();
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
            writer.Write(Sx);
            writer.Write(Sy);
            writer.Write(Data);
        }
    }
}
