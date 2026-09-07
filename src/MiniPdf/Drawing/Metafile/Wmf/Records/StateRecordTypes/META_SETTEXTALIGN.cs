using MiniPdf.Drawing.Metafile.Wmf;

namespace MiniPdf.Drawing.Metafile.Wmf.Records.StateRecordTypes
{
    using MiniPdf.Drawing.Metafile.Wmf.Enumerations;
    using MiniPdf.Drawing.Metafile.Wmf.Flags;
    using MiniPdf.Drawing.Metafile.Wmf.Records;
    using System.IO;

    internal class META_SETTEXTALIGN : Record
    {
        public TextAlignmentMode TextAlignmentMode;

        public META_SETTEXTALIGN()
        {
            Header.RecordFunction = RecordType.META_SETTEXTALIGN;
        }

        public override void Read(BinaryReader reader)
        {
            TextAlignmentMode = (TextAlignmentMode)reader.ReadUInt16();
        }

        public override void Write(BinaryWriter writer)
        {
            base.Write(writer);
            writer.Write((ushort)TextAlignmentMode);
        }
    }
}
