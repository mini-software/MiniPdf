namespace MiniPdf.Drawing.Metafile.Emf.Records
{
    using System;
    using System.IO;

    internal class Record
    {
        public RecordHeader Header { get; set; }

        public byte[] Data { get; set; } = Array.Empty<byte>();

        public virtual void Read(BinaryReader reader)
        {
        }

        public virtual void Write(BinaryWriter writer)
        {
            if (Data.Length > 0)
            {
                writer.Write(Data);
            }
        }

        public override string ToString()
        {
            return string.Format("{0} {1}", Header.Type.ToString(), Header.Size);
        }
    }
}
