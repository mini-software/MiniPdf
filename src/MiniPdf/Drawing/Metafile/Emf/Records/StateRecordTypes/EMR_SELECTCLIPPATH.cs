using MiniSoftware.Drawing.Metafile.Emf;

namespace MiniSoftware.Drawing.Metafile.Emf.Records.StateRecordTypes
{
    using System.IO;
    using MiniSoftware.Drawing.Metafile.Emf.Enumerations;

    internal class EMR_SELECTCLIPPATH : Record
    {
        public RegionMode RegionMode;

        public EMR_SELECTCLIPPATH()
        {
            Header = new RecordHeader { Type = RecordType.EMR_SELECTCLIPPATH };
        }

        public override void Read(BinaryReader reader)
        {
            RegionMode = (RegionMode)reader.ReadUInt32();
        }

        public override void Write(BinaryWriter writer)
        {
            writer.Write((uint)RegionMode);
        }
    }
}
