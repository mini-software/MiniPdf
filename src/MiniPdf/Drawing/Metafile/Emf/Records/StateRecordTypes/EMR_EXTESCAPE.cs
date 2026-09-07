using MiniSoftware.Drawing.Metafile.Emf;

namespace MiniSoftware.Drawing.Metafile.Emf.Records.StateRecordTypes
{
    using MiniSoftware.Drawing.Metafile.Emf.Enumerations;
    using System;
    using System.IO;

    internal class EMR_EXTESCAPE : Record
    {
        public uint Escape;

        public uint DataSize;

        public byte[] EscapeData { get; set; } = Array.Empty<byte>();

        public EMR_EXTESCAPE()
        {
            Header = new RecordHeader { Type = RecordType.EMR_EXTESCAPE };
        }

        public override void Read(BinaryReader reader)
        {
            Escape = reader.ReadUInt32();
            DataSize = reader.ReadUInt32();
            EscapeData = reader.ReadBytes(Convert.ToInt32(DataSize));
        }

        public override void Write(BinaryWriter writer)
        {
            uint size = DataSize == 0 ? (uint)EscapeData.Length : DataSize;
            writer.Write(Escape);
            writer.Write(size);
            writer.Write(EscapeData);
        }
    }
}
