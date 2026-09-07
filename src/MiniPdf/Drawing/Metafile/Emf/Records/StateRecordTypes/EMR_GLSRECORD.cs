using MiniSoftware.Drawing.Metafile.Emf;

namespace MiniSoftware.Drawing.Metafile.Emf.Records.StateRecordTypes
{
    using MiniSoftware.Drawing.Metafile.Emf.Enumerations;
    using System;
    using System.IO;

    internal class EMR_GLSRECORD : Record
    {
        public uint DataSize;

        public byte[] GlsData { get; set; } = Array.Empty<byte>();

        public EMR_GLSRECORD()
        {
            Header = new RecordHeader { Type = RecordType.EMR_GLSRECORD };
        }

        public override void Read(BinaryReader reader)
        {
            DataSize = reader.ReadUInt32();
            GlsData = reader.ReadBytes(Convert.ToInt32(DataSize));
        }

        public override void Write(BinaryWriter writer)
        {
            uint size = DataSize == 0 ? (uint)GlsData.Length : DataSize;
            writer.Write(size);
            writer.Write(GlsData);
        }
    }
}
