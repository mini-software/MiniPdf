using MiniSoftware.Drawing.Metafile.Emf;
using MiniSoftware.Drawing.Metafile.Emf.Records;

namespace MiniSoftware.Drawing.Metafile.Emf.Records.StateRecordTypes
{
    using MiniSoftware.Drawing.Metafile.Emf.Enumerations;
    using System;
    using System.IO;

    internal class EMR_EXTSELECTCLIPRGN : Record
    {
        public uint RegionDataSize;

        public uint RegionMode;

        public byte[] RegionData { get; set; } = Array.Empty<byte>();

        public EMR_EXTSELECTCLIPRGN()
        {
            Header = new RecordHeader { Type = RecordType.EMR_EXTSELECTCLIPRGN };
        }

        public override void Read(BinaryReader reader)
        {
            RegionDataSize = reader.ReadUInt32();
            RegionMode = reader.ReadUInt32();
            RegionData = reader.ReadBytes(Convert.ToInt32(RegionDataSize));
        }

        public override void Write(BinaryWriter writer)
        {
            uint regionSize = RegionDataSize == 0 ? (uint)RegionData.Length : RegionDataSize;
            writer.Write(regionSize);
            writer.Write(RegionMode);
            writer.Write(RegionData);
        }
    }
}
