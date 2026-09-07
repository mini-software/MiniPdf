using MiniSoftware.Drawing.Metafile.Emf;

namespace MiniSoftware.Drawing.Metafile.Emf.Records
{
    using MiniSoftware.Drawing.Metafile.Emf.Enumerations;
    using System.IO;

    internal struct RecordHeader
    {
       
        public RecordType Type;

        /// <summary>
        ///     A 32-bit unsigned integer that defines the number of 16-bit WORDs in the record.
        /// </summary>
        public uint Size;

        

        public void Read(BinaryReader reader)
        {
            this.Type = (RecordType)reader.ReadUInt32();
            this.Size = reader.ReadUInt32();
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write((uint)this.Type);
            writer.Write(this.Size);
        }
    }
}
