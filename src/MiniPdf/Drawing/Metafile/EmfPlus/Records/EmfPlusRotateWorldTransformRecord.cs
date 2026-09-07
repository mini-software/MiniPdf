namespace MiniPdf.Drawing.Metafile.EmfPlus.Records
{
    using System.IO;
    using MiniPdf.Drawing.Metafile.EmfPlus.Enumerations;

    internal class EmfPlusRotateWorldTransformRecord : Record
    {
        public float Angle;

        public EmfPlusMatrixOrder MatrixOrder => (EmfPlusMatrixOrder)(Header.Flags & 0x0001);

        public override void Read(BinaryReader reader)
        {
            if (Header.DataSize >= 4)
            {
                Angle = reader.ReadSingle();
                int remaining = (int)Header.DataSize - 4;
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
            writer.Write(Angle);
            writer.Write(Data);
        }
    }
}
