using MiniPdf.Drawing.Metafile.Emf;

namespace MiniPdf.Drawing.Metafile.Emf.Records.StateRecordTypes
{
    using MiniPdf.Drawing.Metafile.Emf.Enumerations;
    using System.IO;

    internal class EMR_ENDPATH : Record
    {
        public EMR_ENDPATH()
        {
            Header = new RecordHeader { Type = RecordType.EMR_ENDPATH };
        }

        public override void Read(BinaryReader reader)
        {
        }

        public override void Write(BinaryWriter writer)
        {
        }
    }
}
