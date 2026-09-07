using MiniSoftware.Drawing.Metafile.Emf;
using MiniSoftware.Drawing.Metafile.Emf.Records;

namespace MiniSoftware.Drawing.Metafile.Emf.Records.StateRecordTypes
{
    using MiniSoftware.Drawing.Metafile.Emf.Enumerations;
    using System.IO;
    

    internal class EMR_SETMAPMODE : Record
    {
        public Wmf.Enumerations.MapMode MapMode;

        public EMR_SETMAPMODE()
        {
            Header = new RecordHeader { Type = RecordType.EMR_SETMAPMODE };
        }

        public override void Read(BinaryReader reader)
        {
            MapMode = (Wmf.Enumerations.MapMode)reader.ReadUInt32();
        }

        public override void Write(BinaryWriter writer)
        {
            writer.Write((uint)MapMode);
        }
    }
}

