namespace MiniSoftware.Drawing.Metafile.Emf.Objects
{
    using System.IO;

    using MiniSoftware.Drawing.Metafile.Wmf.Objects;

    /// <summary>
    ///     The Header object defines the EMF metafile header. 
    ///     It specifies properties of the device on which the image in the metafile was created.
    /// </summary>
    /// <remarks>
    ///     2.2.9 Header Object
    /// </remarks>
    internal class Header
    {
        /// <summary>
        ///     A WMF RectL object ([MS-WMF] section 2.2.2.19) that specifies the rectangular inclusive-inclusive bounds in device units of the smallest rectangle that can be drawn around the image stored in the metafile.
        /// </summary>
        public RectL Bounds;

        /// <summary>
        ///     A WMF RectL object that specifies the rectangular inclusive-inclusive dimensions, in .01 millimeter units, of a rectangle that surrounds the image stored in the metafile.
        /// </summary>
        public RectL Frame;

        /// <summary>
        ///     A 32-bit unsigned integer that specifies the record signature. This MUST be ENHMETA_SIGNATURE, from the FormatSignature enumeration (section 2.1.14).
        /// </summary>
        public uint RecordSignature;

        /// <summary>
        ///     A 32-bit unsigned integer that specifies EMF metafile interoperability. This SHOULD be 0x00010000.
        /// </summary>
        public uint Version;

        /// <summary>
        ///     A 32-bit unsigned integer that specifies the size of the metafile, in bytes.
        /// </summary>
        public uint Bytes;

        /// <summary>
        ///     A 32-bit unsigned integer that specifies the number of records in the metafile.
        /// </summary>
        public uint Records;

        /// <summary>
        ///     A 16-bit unsigned integer that specifies the number of graphics objects that will be used during the processing of the metafile.
        /// </summary>
        public ushort Handles;

        public ushort Reserved;

        /// <summary>
        ///     A 32-bit unsigned integer that specifies the number of characters in the array that contains the description of the metafile's contents. This is zero if there is no description string.
        /// </summary>
        public uint nDescription;

        /// <summary>
        ///     A 32-bit unsigned integer that specifies the offset from the beginning of this record to the array that contains the description of the metafile's contents.
        /// </summary>
        public uint offDescription;

        /// <summary>
        ///     A 32-bit unsigned integer that specifies the number of entries in the metafile palette. The palette is located in the EMR_EOF record.
        /// </summary>
        public uint nPalEntries;

        /// <summary>
        ///     A WMF SizeL object ([MS-WMF] section 2.2.2.22) that specifies the size of the reference device, in pixels.
        /// </summary>
        public SizeL Device;

        /// <summary>
        ///     A WMF SizeL object that specifies the size of the reference device, in millimeters.
        /// </summary>
        public SizeL Millimeters;

        public void Read(BinaryReader reader)
        {
            this.Bounds = new RectL();
            this.Bounds.Read(reader);
            this.Frame = new RectL();
            this.Frame.Read(reader);
            this.RecordSignature = reader.ReadUInt32();
            this.Version = reader.ReadUInt32();
            this.Bytes = reader.ReadUInt32();
            this.Records = reader.ReadUInt32();
            this.Handles = reader.ReadUInt16();
            this.Reserved = reader.ReadUInt16();
            this.nDescription = reader.ReadUInt32();
            this.offDescription = reader.ReadUInt32();
            this.nPalEntries = reader.ReadUInt32();
            this.Device = new SizeL();
            this.Device.Read(reader);
            this.Millimeters = new SizeL();
            this.Millimeters.Read(reader);
        }

        public void Write(BinaryWriter writer)
        {
            this.Bounds.Write(writer);
            this.Frame.Write(writer);
            writer.Write(this.RecordSignature);
            writer.Write(this.Version);
            writer.Write(this.Bytes);
            writer.Write(this.Records);
            writer.Write(this.Handles);
            writer.Write(this.Reserved);
            writer.Write(this.nDescription);
            writer.Write(this.offDescription);
            writer.Write(this.nPalEntries);
            this.Device.Write(writer);
            this.Millimeters.Write(writer);
        }
    }
}
