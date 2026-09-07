using MiniSoftware.Drawing.Metafile.Wmf;

namespace MiniSoftware.Drawing.Metafile.Wmf.Records.StateRecordTypes
{
    using MiniSoftware.Drawing.Metafile.Wmf.Enumerations;
    using System.IO;

    internal class META_SETPOLYFILLMODE : Record
    {
        public PolyFillMode PolyFillMode;

        public META_SETPOLYFILLMODE()
        {
            Header.RecordFunction = RecordType.META_SETPOLYFILLMODE;
        }

        public override void Read(BinaryReader reader)
        {
            PolyFillMode = (PolyFillMode)reader.ReadUInt16();
        }

        public override void Write(BinaryWriter writer)
        {
            base.Write(writer);
            writer.Write((ushort)PolyFillMode);
        }
    }
}
