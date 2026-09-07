using MiniPdf.Drawing.Metafile.Emf;

namespace MiniPdf.Drawing.Metafile.Emf.Records.StateRecordTypes
{
    using MiniPdf.Drawing.Metafile.Emf.Enumerations;
    using System;
    using System.IO;

    internal class EMR_COMMENT : Record
    {
        public uint DataSize;

        public byte[] CommentData { get; set; } = Array.Empty<byte>();

        public EMR_COMMENT()
        {
            Header = new RecordHeader { Type = RecordType.EMR_COMMENT };
        }

        public override void Read(BinaryReader reader)
        {
            DataSize = reader.ReadUInt32();
            CommentData = reader.ReadBytes(Convert.ToInt32(DataSize));
        }

        public override void Write(BinaryWriter writer)
        {
            uint size = DataSize == 0 ? (uint)CommentData.Length : DataSize;
            writer.Write(size);
            writer.Write(CommentData);
        }
    }
}
