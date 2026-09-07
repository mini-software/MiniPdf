using MiniSoftware.Drawing.Metafile.Emf;

namespace MiniSoftware.Drawing.Metafile.Emf.Records.StateRecordTypes
{
    using System;
    using System.IO;
    using MiniSoftware.Drawing.Metafile.Emf.Enumerations;
    using MiniSoftware.Drawing.Metafile.Wmf.Objects;

    internal class EMR_FRAMERGN : Record
    {
        public RectL Bounds;

        public uint RegionDataSize;

        public uint IhBrush;

        public int Width;

        public int Height;

        public byte[] RegionData { get; set; } = Array.Empty<byte>();

        public EMR_FRAMERGN()
        {
            Header = new RecordHeader { Type = RecordType.EMR_FRAMERGN };
        }

        public override void Read(BinaryReader reader)
        {
            Bounds = new RectL();
            Bounds.Read(reader);
            RegionDataSize = reader.ReadUInt32();
            IhBrush = reader.ReadUInt32();
            Width = reader.ReadInt32();
            Height = reader.ReadInt32();
            RegionData = reader.ReadBytes(Convert.ToInt32(RegionDataSize));
        }

        public override void Write(BinaryWriter writer)
        {
            uint regionSize = RegionDataSize == 0 ? (uint)RegionData.Length : RegionDataSize;
            Bounds.Write(writer);
            writer.Write(regionSize);
            writer.Write(IhBrush);
            writer.Write(Width);
            writer.Write(Height);
            writer.Write(RegionData);
        }
    }
}
