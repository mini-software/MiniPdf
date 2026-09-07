using MiniPdf.Drawing.Metafile.Emf;

namespace MiniPdf.Drawing.Metafile.Emf.Records.DrawingRecordTypes
{
    using System.IO;
    using MiniPdf.Drawing.Metafile.Emf.Enumerations;
    using MiniPdf.Drawing.Metafile.Wmf.Objects;

    internal class EMR_RECTANGLE : Record
    {
        public RectL Box;

        public EMR_RECTANGLE()
        {
            Header = new RecordHeader { Type = RecordType.EMR_RECTANGLE };
        }

        public override void Read(BinaryReader reader)
        {
            Box = new RectL();
            Box.Read(reader);
        }

        public override void Write(BinaryWriter writer)
        {
            Box.Write(writer);
        }
    }
}
