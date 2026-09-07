using MiniSoftware.Drawing.Metafile.Wmf;

namespace MiniSoftware.Drawing.Metafile.Wmf.Records
{
    using MiniSoftware.Drawing.Metafile.Wmf.Enumerations;
    using System.IO;

    /// <summary>
    ///     The META_HEADER record is the first record in a standard (nonplaceable) WMF metafile.
    /// </summary>
    /// <remarks>
    ///     2.3.2.2 META_HEADER Record
    /// </remarks>
    internal class MetaHeader
    {
        /// <summary>
        ///     Defines the type of metafile.
        /// </summary>
        public MetafileType Type;

        /// <summary>
        ///     Defines the number of 16-bit words in the header.
        /// </summary>
        public ushort HeaderSize;

        /// <summary>
        ///     Defines the metafile version.
        /// </summary>
        public MetafileVersion Version;

        /// <summary>
        ///     Defines the low-order word of the number of 16-bit words in the entire metafile.
        /// </summary>
        public ushort SizeLow;

        /// <summary>
        ///     Defines the high-order word of the number of 16-bit words in the entire metafile.
        /// </summary>
        public ushort SizeHigh;

        /// <summary>
        ///     Specifies the number of graphics objects that are defined in the entire metafile. These objects include brushes, pens, and the other objects specified in section 2.2.1.
        /// </summary>
        public ushort NumberOfObjects;

        /// <summary>
        ///     Specifies the size of the largest record used in the metafile (in 16-bit elements).
        /// </summary>
        public uint MaxRecord;

        /// <summary>
        ///     A 16-bit unsigned integer that is not used. It SHOULD be 0x0000.
        /// </summary>
        public ushort NumberOfMembers;

        public void Read(BinaryReader reader)
        {
            this.Type = (MetafileType)reader.ReadUInt16();
            this.HeaderSize = reader.ReadUInt16();
            this.Version = (MetafileVersion)reader.ReadUInt16();
            this.SizeLow = reader.ReadUInt16();
            this.SizeHigh = reader.ReadUInt16();
            this.NumberOfObjects = reader.ReadUInt16();
            this.MaxRecord = reader.ReadUInt32();
            this.NumberOfMembers = reader.ReadUInt16();
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write((ushort)this.Type);
            writer.Write(this.HeaderSize);
            writer.Write((ushort)this.Version);
            writer.Write(this.SizeLow);
            writer.Write(this.SizeHigh);
            writer.Write(this.NumberOfObjects);
            writer.Write(this.MaxRecord);
            writer.Write(this.NumberOfMembers);
        }
    }
}
