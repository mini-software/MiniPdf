using MiniPdf.Drawing.Metafile.Emf;

namespace MiniPdf.Drawing.Metafile.Emf.Records.StateRecordTypes
{
    using MiniPdf.Drawing.Metafile.Emf.Enumerations;
    using System.IO;

    internal class EMR_SETTEXTALIGN : Record
    {
        public Wmf.Flags.TextAlignmentMode TextAlignmentMode;

        public EMR_SETTEXTALIGN()
        {
            Header = new RecordHeader { Type = RecordType.EMR_SETTEXTALIGN };
        }

        public override void Read(BinaryReader reader)
        {
            TextAlignmentMode = (Wmf.Flags.TextAlignmentMode)reader.ReadUInt32();
        }

        public override void Write(BinaryWriter writer)
        {
            writer.Write((uint)TextAlignmentMode);
        }
    }
}

