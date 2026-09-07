using MiniPdf.Drawing.Metafile.Emf;
using MiniPdf.Drawing.Metafile.Emf.Records;

namespace MiniPdf.Drawing.Metafile.Emf.Records.StateRecordTypes
{
    using System.IO;
    using MiniPdf.Drawing.Metafile.Emf.Enumerations;
    using MiniPdf.Drawing.Metafile.Wmf.Objects;

    internal class EMR_SETTEXTCOLOR : Record
    {
        public ColorRef Color;

        public EMR_SETTEXTCOLOR()
        {
            Header = new RecordHeader { Type = RecordType.EMR_SETTEXTCOLOR };
        }

        public override void Read(BinaryReader reader)
        {
            Color.Read(reader);
        }

        public override void Write(BinaryWriter writer)
        {
            Color.Write(writer);
        }
    }
}

