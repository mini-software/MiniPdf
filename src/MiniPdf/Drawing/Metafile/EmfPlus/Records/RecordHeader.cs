using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniPdf.Drawing.Metafile.EmfPlus.Records
{
    using System.IO;

    using MiniPdf.Drawing.Metafile.EmfPlus.Enumerations;

    internal struct RecordHeader
    {

        public RecordType Type;

        public ushort Flags;
        
        public uint Size;

        public uint DataSize;

        public void Read(BinaryReader reader)
        {
            this.Type = (RecordType)reader.ReadUInt16();
            this.Flags = reader.ReadUInt16();
            this.Size = reader.ReadUInt32();
            this.DataSize = reader.ReadUInt32();
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write((ushort)this.Type);
            writer.Write(this.Flags);
            writer.Write(this.Size);
            writer.Write(this.DataSize);
        }
    }
}
