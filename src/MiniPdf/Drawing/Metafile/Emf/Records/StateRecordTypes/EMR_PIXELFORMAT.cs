using MiniPdf.Drawing.Metafile.Emf;

namespace MiniPdf.Drawing.Metafile.Emf.Records.StateRecordTypes
{
    using MiniPdf.Drawing.Metafile.Emf.Enumerations;
    using System;
    using System.IO;

    internal class EMR_PIXELFORMAT : Record
    {
        public uint PixelFormatSize;

        public byte[] PixelFormatDescriptor { get; set; } = Array.Empty<byte>();

        public EMR_PIXELFORMAT()
        {
            Header = new RecordHeader { Type = RecordType.EMR_PIXELFORMAT };
        }

        public override void Read(BinaryReader reader)
        {
            PixelFormatSize = reader.ReadUInt32();
            PixelFormatDescriptor = reader.ReadBytes(Convert.ToInt32(PixelFormatSize));
        }

        public override void Write(BinaryWriter writer)
        {
            uint size = PixelFormatSize == 0 ? (uint)PixelFormatDescriptor.Length : PixelFormatSize;
            writer.Write(size);
            writer.Write(PixelFormatDescriptor);
        }
    }
}
