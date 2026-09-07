using MiniPdf.Drawing.Metafile.Wmf;

namespace MiniPdf.Drawing.Metafile.Wmf.Records.StateRecordTypes
{
    using System.IO;
    using MiniPdf.Drawing.Metafile.Wmf.Enumerations;

    internal class META_SETMAPMODE : Record
    {
        public MapMode MapMode;

        public META_SETMAPMODE()
        {
            Header.RecordFunction = RecordType.META_SETMAPMODE;
        }

        public override void Read(BinaryReader reader)
        {
            MapMode = (MapMode)reader.ReadUInt16();
        }

        public override void Write(BinaryWriter writer)
        {
            base.Write(writer);
            writer.Write((ushort)MapMode);
        }
    }
}
