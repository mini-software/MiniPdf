using MiniSoftware.Drawing.Metafile.Wmf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniSoftware.Drawing.Metafile.Wmf.Records
{
    using MiniSoftware.Drawing.Metafile.Wmf.Enumerations;
    using System.IO;

    internal struct RecordHeader
    {
        /// <summary>
        ///     A 32-bit unsigned integer that defines the number of 16-bit WORDs in the record.
        /// </summary>
        public uint RecordSize;

        /// <summary>
        ///     A 16-bit unsigned integer that defines the type of this record. 
        ///     The low-order byte MUST match the low-order byte of one of the values in the RecordType Enumeration.
        /// </summary>
        public RecordType RecordFunction;

        public void Read(BinaryReader reader)
        {
            this.RecordSize = reader.ReadUInt32();
            this.RecordFunction = (RecordType)reader.ReadUInt16();
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(this.RecordSize);
            writer.Write((ushort)this.RecordFunction);
        }
    }
}
