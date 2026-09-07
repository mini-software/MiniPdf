namespace MiniPdf.Drawing.Metafile.EmfPlus.Records
{
    using System.IO;
    using MiniPdf.Drawing.Metafile.EmfPlus.Enumerations;

    internal class EmfPlusTranslateWorldTransformRecord : Record
    {
        public float Dx;
        public float Dy;

        public EmfPlusMatrixOrder MatrixOrder => (EmfPlusMatrixOrder)(Header.Flags & 0x0001);

        public override void Read(BinaryReader reader)
        {
            if (Header.DataSize >= 8)
            {
                Dx = reader.ReadSingle();
                Dy = reader.ReadSingle();
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
            writer.Write(Dx);
            writer.Write(Dy);
            writer.Write(Data);
        }
    }
}
