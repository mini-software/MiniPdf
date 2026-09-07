using MiniPdf.Drawing.Metafile.Emf;

namespace MiniPdf.Drawing.Metafile.Emf.Records.StateRecordTypes
{
    using System;
    using System.IO;
    using MiniPdf.Drawing.Metafile.Emf.Enumerations;
    using MiniPdf.Drawing.Metafile.Wmf.Objects;

    internal class EMR_GLSBOUNDEDRECORD : Record
    {
        public RectL Bounds;

        public uint DataSize;

        public byte[] GlsData { get; set; } = Array.Empty<byte>();

        public EMR_GLSBOUNDEDRECORD()
        {
            Header = new RecordHeader { Type = RecordType.EMR_GLSBOUNDEDRECORD };
        }

        public override void Read(BinaryReader reader)
        {
            Bounds = new RectL();
            Bounds.Read(reader);
            DataSize = reader.ReadUInt32();
            GlsData = reader.ReadBytes(Convert.ToInt32(DataSize));
        }

        public override void Write(BinaryWriter writer)
        {
            uint size = DataSize == 0 ? (uint)GlsData.Length : DataSize;
            Bounds.Write(writer);
            writer.Write(size);
            writer.Write(GlsData);
        }
    }
}
