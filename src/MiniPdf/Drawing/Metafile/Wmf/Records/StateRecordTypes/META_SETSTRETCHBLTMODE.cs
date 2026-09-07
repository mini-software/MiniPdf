using MiniSoftware.Drawing.Metafile.Wmf;

namespace MiniSoftware.Drawing.Metafile.Wmf.Records.StateRecordTypes
{
    using MiniSoftware.Drawing.Metafile.Wmf.Enumerations;
    using MiniSoftware.Drawing.Metafile.Wmf.Records;
    using System.IO;

    internal class META_SETSTRETCHBLTMODE : Record
    {
        public StretchMode StretchMode;

        public META_SETSTRETCHBLTMODE()
        {
            Header.RecordFunction = RecordType.META_SETSTRETCHBLTMODE;
        }

        public override void Read(BinaryReader reader)
        {
            StretchMode = (StretchMode)reader.ReadUInt16();
        }

        public override void Write(BinaryWriter writer)
        {
            base.Write(writer);
            writer.Write((ushort)StretchMode);
        }
    }
}
