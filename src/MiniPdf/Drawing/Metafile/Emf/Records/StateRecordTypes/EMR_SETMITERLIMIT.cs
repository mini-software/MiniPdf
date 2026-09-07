using MiniPdf.Drawing.Metafile.Emf;

namespace MiniPdf.Drawing.Metafile.Emf.Records.StateRecordTypes
{
    using MiniPdf.Drawing.Metafile.Emf.Enumerations;
    using System.IO;

    internal class EMR_SETMITERLIMIT : Record
    {
        public float MiterLimit;

        public EMR_SETMITERLIMIT()
        {
            Header = new RecordHeader { Type = RecordType.EMR_SETMITERLIMIT };
        }

        public override void Read(BinaryReader reader)
        {
            MiterLimit = reader.ReadSingle();
        }

        public override void Write(BinaryWriter writer)
        {
            writer.Write(MiterLimit);
        }
    }
}

