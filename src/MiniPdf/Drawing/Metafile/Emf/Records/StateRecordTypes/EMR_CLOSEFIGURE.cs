using MiniPdf.Drawing.Metafile.Emf;
using MiniPdf.Drawing.Metafile.Emf.Records;

namespace MiniPdf.Drawing.Metafile.Emf.Records.StateRecordTypes
{
    using MiniPdf.Drawing.Metafile.Emf.Enumerations;
    using System.IO;

    internal class EMR_CLOSEFIGURE : Record
    {
        public EMR_CLOSEFIGURE()
        {
            Header = new RecordHeader { Type = RecordType.EMR_CLOSEFIGURE };
        }

        public override void Read(BinaryReader reader)
        {
        }

        public override void Write(BinaryWriter writer)
        {
        }
    }
}
