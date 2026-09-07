using MiniPdf.Drawing.Metafile.Emf;

namespace MiniPdf.Drawing.Metafile.Emf.Records.StateRecordTypes
{
    using System.IO;
    using MiniPdf.Drawing.Metafile.Emf.Enumerations;
    using MiniPdf.Drawing.Metafile.Emf.Objects;
    

    internal class EMR_SETCOLORADJUSTMENT : Record
    {
        public Objects.ColorAdjustment ColorAdjustment { get; set; } = new();

        public EMR_SETCOLORADJUSTMENT()
        {
            Header = new RecordHeader { Type = RecordType.EMR_SETCOLORADJUSTMENT };
        }

        public override void Read(BinaryReader reader)
        {
            ColorAdjustment.Read(reader);
        }

        public override void Write(BinaryWriter writer)
        {
            ColorAdjustment.Write(writer);
        }
    }
}
