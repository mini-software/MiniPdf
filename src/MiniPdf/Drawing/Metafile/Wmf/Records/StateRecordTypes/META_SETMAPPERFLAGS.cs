using MiniSoftware.Drawing.Metafile.Wmf;

namespace MiniSoftware.Drawing.Metafile.Wmf.Records.StateRecordTypes
{
    using MiniSoftware.Drawing.Metafile.Wmf.Enumerations;
    using System.IO;

    internal class META_SETMAPPERFLAGS : Record
    {
        public uint MapperValues;

        public META_SETMAPPERFLAGS()
        {
            Header.RecordFunction = RecordType.META_SETMAPPERFLAGS;
        }

        public override void Read(BinaryReader reader)
        {
            MapperValues = reader.ReadUInt32();
        }

        public override void Write(BinaryWriter writer)
        {
            base.Write(writer);
            writer.Write(MapperValues);
        }
    }
}
