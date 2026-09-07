using MiniSoftware.Drawing.Metafile.Emf;
using MiniSoftware.Drawing.Metafile.Emf.Records;

namespace MiniSoftware.Drawing.Metafile.Emf.Records.StateRecordTypes
{
    using System;
    using System.IO;
    using MiniSoftware.Drawing.Metafile.Emf.Enumerations;
    using MiniSoftware.Drawing.Metafile.Wmf.Objects;

    internal class EMR_INVERTRGN : Record
    {
        public RectL Bounds;

        public uint RegionDataSize;

        public byte[] RegionData { get; set; } = Array.Empty<byte>();

        public EMR_INVERTRGN()
        {
            Header = new RecordHeader { Type = RecordType.EMR_INVERTRGN };
        }

        public override void Read(BinaryReader reader)
        {
            Bounds = new RectL();
            Bounds.Read(reader);
            RegionDataSize = reader.ReadUInt32();
            RegionData = reader.ReadBytes(Convert.ToInt32(RegionDataSize));
        }

        public override void Write(BinaryWriter writer)
        {
            uint regionSize = RegionDataSize == 0 ? (uint)RegionData.Length : RegionDataSize;
            Bounds.Write(writer);
            writer.Write(regionSize);
            writer.Write(RegionData);
        }
    }
}
