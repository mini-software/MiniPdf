namespace MiniPdf.Drawing.Metafile.EmfPlus.Records
{
    using System;
    using System.IO;

    internal class Record
    {
        public RecordHeader Header { get; set; }

        public byte[] Data { get; set; } = Array.Empty<byte>();

        public override string ToString()
        {
            return string.Format("{0} {1}", Header.Type.ToString(), Header.Size);
        }

        public virtual void Read(BinaryReader reader)
        {
            Data = reader.ReadBytes(Convert.ToInt32(Header.DataSize));
        }

        public virtual void Write(BinaryWriter writer)
        {
            writer.Write(Data);
        }
    }
}
