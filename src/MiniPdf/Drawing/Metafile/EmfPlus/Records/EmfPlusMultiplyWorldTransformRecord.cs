namespace MiniSoftware.Drawing.Metafile.EmfPlus.Records
{
    using System.IO;
    using MiniSoftware.Drawing.Metafile.EmfPlus.Enumerations;

    internal class EmfPlusMultiplyWorldTransformRecord : Record
    {
        public float M11;
        public float M12;
        public float M21;
        public float M22;
        public float Dx;
        public float Dy;

        public EmfPlusMatrixOrder MatrixOrder => (EmfPlusMatrixOrder)(Header.Flags & 0x0001);

        public override void Read(BinaryReader reader)
        {
            if (Header.DataSize >= 24)
            {
                M11 = reader.ReadSingle();
                M12 = reader.ReadSingle();
                M21 = reader.ReadSingle();
                M22 = reader.ReadSingle();
                Dx = reader.ReadSingle();
                Dy = reader.ReadSingle();
                int remaining = (int)Header.DataSize - 24;
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
            writer.Write(M11);
            writer.Write(M12);
            writer.Write(M21);
            writer.Write(M22);
            writer.Write(Dx);
            writer.Write(Dy);
            writer.Write(Data);
        }
    }
}
