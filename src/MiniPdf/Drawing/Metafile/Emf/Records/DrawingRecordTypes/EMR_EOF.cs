using MiniPdf.Drawing.Metafile.Emf;

namespace MiniPdf.Drawing.Metafile.Emf.Records.DrawingRecordTypes
{
    using MiniPdf.Drawing.Metafile.Emf.Enumerations;
    using System;
    using System.IO;

    internal class EMR_EOF : Record
    {
        public uint NumberOfHandles;

        public ushort Reserved;

        public ushort NumberOfRecords;

        public byte[] EndOfFileData { get; set; } = Array.Empty<byte>();

        public EMR_EOF()
        {
            Header = new RecordHeader { Type = RecordType.EMR_EOF };
        }

        public override void Read(BinaryReader reader)
        {
            NumberOfHandles = reader.ReadUInt32();
            Reserved = reader.ReadUInt16();
            NumberOfRecords = reader.ReadUInt16();

            int extraBytes = Convert.ToInt32(Header.Size) - 16;
            if (extraBytes > 0)
            {
                EndOfFileData = reader.ReadBytes(extraBytes);
            }
        }

        public override void Write(BinaryWriter writer)
        {
            writer.Write(NumberOfHandles);
            writer.Write(Reserved);
            writer.Write(NumberOfRecords);
            writer.Write(EndOfFileData);
        }
    }
}
