using MiniPdf.Drawing.Metafile.Emf;
using MiniPdf.Drawing.Metafile.Emf.Records;

namespace MiniPdf.Drawing.Metafile.Emf.Records.StateRecordTypes
{
    using System;
    using System.IO;
    using MiniPdf.Drawing.Metafile.Emf.Enumerations;
    using MiniPdf.Drawing.Metafile.Wmf.Objects;

    internal class EMR_FILLRGN : Record
    {
        public RectL Bounds;

        public uint RegionDataSize;

        public uint IhBrush;

        public byte[] RegionData { get; set; } = Array.Empty<byte>();

        public EMR_FILLRGN()
        {
            Header = new RecordHeader { Type = RecordType.EMR_FILLRGN };
        }

        public override void Read(BinaryReader reader)
        {
            Bounds = new RectL();
            Bounds.Read(reader);
            RegionDataSize = reader.ReadUInt32();
            IhBrush = reader.ReadUInt32();
            RegionData = reader.ReadBytes(Convert.ToInt32(RegionDataSize));
        }

        public override void Write(BinaryWriter writer)
        {
            uint regionSize = RegionDataSize == 0 ? (uint)RegionData.Length : RegionDataSize;
            Bounds.Write(writer);
            writer.Write(regionSize);
            writer.Write(IhBrush);
            writer.Write(RegionData);
        }
    }
}
