using MiniPdf.Drawing.Metafile.Emf;

namespace MiniPdf.Drawing.Metafile.Emf.Records.StateRecordTypes
{
    using MiniPdf.Drawing.Metafile.Emf.Enumerations;
    using System.IO;

    internal class EMR_SETMAPPERFLAGS : Record
    {
        public uint MapperValues;

        public EMR_SETMAPPERFLAGS()
        {
            Header = new RecordHeader { Type = RecordType.EMR_SETMAPPERFLAGS };
        }

        public override void Read(BinaryReader reader)
        {
            MapperValues = reader.ReadUInt32();
        }

        public override void Write(BinaryWriter writer)
        {
            writer.Write(MapperValues);
        }
    }
}

