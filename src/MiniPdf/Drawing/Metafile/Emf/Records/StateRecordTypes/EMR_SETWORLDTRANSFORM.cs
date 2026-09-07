using MiniPdf.Drawing.Metafile.Emf;
using MiniPdf.Drawing.Metafile.Emf.Records;

namespace MiniPdf.Drawing.Metafile.Emf.Records.StateRecordTypes
{
    using MiniPdf.Drawing.Metafile.Emf.Enumerations;
    using System.IO;

    internal class EMR_SETWORLDTRANSFORM : Record
    {
        public float M11;

        public float M12;

        public float M21;

        public float M22;

        public float Dx;

        public float Dy;

        public EMR_SETWORLDTRANSFORM()
        {
            Header = new RecordHeader { Type = RecordType.EMR_SETWORLDTRANSFORM };
        }

        public override void Read(BinaryReader reader)
        {
            M11 = reader.ReadSingle();
            M12 = reader.ReadSingle();
            M21 = reader.ReadSingle();
            M22 = reader.ReadSingle();
            Dx = reader.ReadSingle();
            Dy = reader.ReadSingle();
        }

        public override void Write(BinaryWriter writer)
        {
            writer.Write(M11);
            writer.Write(M12);
            writer.Write(M21);
            writer.Write(M22);
            writer.Write(Dx);
            writer.Write(Dy);
        }
    }
}
