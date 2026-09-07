using MiniPdf.Drawing.Metafile.Emf;

namespace MiniPdf.Drawing.Metafile.Emf.Records.StateRecordTypes
{
    using MiniPdf.Drawing.Metafile.Emf.Enumerations;
    using System.IO;

    internal class EMR_SETLAYOUT : Record
    {
        public Metafile.Wmf.Enumerations.Layout LayoutMode;

        public EMR_SETLAYOUT()
        {
            Header = new RecordHeader { Type = RecordType.EMR_SETLAYOUT };
        }

        public override void Read(BinaryReader reader)
        {
            LayoutMode = (Metafile.Wmf.Enumerations.Layout)reader.ReadUInt32();
        }

        public override void Write(BinaryWriter writer)
        {
            writer.Write((uint)LayoutMode);
        }
    }
}

