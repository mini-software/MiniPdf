using MiniSoftware.Drawing.Metafile.Wmf;

namespace MiniSoftware.Drawing.Metafile.Wmf.Records.StateRecordTypes
{
    using MiniSoftware.Drawing.Metafile.Wmf.Enumerations;
    using MiniSoftware.Drawing.Metafile.Wmf.Flags;
    using MiniSoftware.Drawing.Metafile.Wmf.Records;
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
