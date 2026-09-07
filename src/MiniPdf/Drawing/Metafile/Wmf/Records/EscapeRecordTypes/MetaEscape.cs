using MiniPdf.Drawing.Metafile.Wmf;

namespace MiniPdf.Drawing.Metafile.Wmf.Records.EscapeRecordTypes
{
    using System;
    using System.IO;
    using MiniPdf.Drawing.Metafile.Wmf.Enumerations;

    /// <summary>
    ///     The META_ESCAPE record specifies extensions to WMF functionality that are not directly available through other records defined in the RecordType enumeration (section 2.1.1.1). 
    ///     The MetafileEscapes enumeration (section 2.1.1.17) lists these extensions.
    /// </summary>
    internal class MetaEscape : Record
    {
        /// <summary>
        ///     Defines the escape function. The value MUST be from the MetafileEscapes enumeration.
        /// </summary>
        public MetafileEscapes EscapeFunction { get; set; }

        /// <summary>
        ///     Specifies the size, in bytes, of the EscapeData field.
        /// </summary>
        public ushort ByteCount { get; set; }

        /// <summary>
        ///     An array of bytes of size ByteCount.
        /// </summary>
        public byte[] EscapeData { get; set; } = Array.Empty<byte>();

        public MetaEscape()
        {
            Header.RecordFunction = RecordType.META_ESCAPE;
        }

        public override void Read(BinaryReader reader)
        {
            EscapeFunction = (MetafileEscapes)reader.ReadUInt16();
            ByteCount = reader.ReadUInt16();
            EscapeData = reader.ReadBytes(ByteCount);
        }

        public override void Write(BinaryWriter writer)
        {
            base.Write(writer);
            writer.Write((ushort)EscapeFunction);
            writer.Write(ByteCount);
            writer.Write(EscapeData);
        }
    }
}
