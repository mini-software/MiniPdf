namespace MiniPdf.Drawing.Metafile.Wmf.Records
{
    using System.IO;
    using MiniPdf.Drawing.Metafile.Wmf.Objects;

    /// <summary>
    /// The META_PLACEABLE record is the first record in a placeable WMF metafile, which is an extension to the WMF metafile format. The information in this extension allows the specification of the placement and size of the target image, which makes it adaptable to different output devices.
    /// </summary>
    internal class META_PLACEABLE
    {
        /// <summary>
        ///     Identification value that indicates the presence of a placeable metafile header. This value MUST be 0x9AC6CDD7.
        /// </summary>
        public uint Key { get; set; }

        /// <summary>
        ///     The resource handle to the metafile, when the metafile is in memory. When the metafile is on disk, this field MUST contain 0x0000. 
        ///     This attribute of the metafile is specified in the Type field of the META_HEADER record.
        /// </summary>
        public ushort HWmf { get; set; }

        /// <summary>
        ///     The destination rectangle, measured in logical units, for displaying the metafile. The size of a logical unit is specified by the Inch field.
        /// </summary>
        public Rect BoundingBox { get; set; } = new();

        /// <summary>
        ///     The number of logical units per inch used to represent the image. This value can be used to scale an image.
        /// </summary>
        public ushort Inch { get; set; }

        /// <summary>
        ///     A 32-bit unsigned integer that is reserved and MUST be 0x00000000.
        /// </summary>
        public uint Reserved { get; set; }

        /// <summary>
        ///     A checksum for the previous 10 16-bit values in the header. This value can be used to determine whether the metafile has become corrupted.
        /// </summary>
        public ushort Checksum { get; set; }

        public META_PLACEABLE()
        {
            Key = 0x9AC6CDD7;
        }

        public void Read(BinaryReader reader)
        {
            Key = reader.ReadUInt32();
            HWmf = reader.ReadUInt16();
            var boundingBox = BoundingBox;
            boundingBox.Read(reader);
            BoundingBox = boundingBox;
            Inch = reader.ReadUInt16();
            Reserved = reader.ReadUInt32();
            Checksum = reader.ReadUInt16();
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(Key);
            writer.Write(HWmf);
            BoundingBox.Write(writer);
            writer.Write(Inch);
            writer.Write(Reserved);
            writer.Write(CalculateChecksum());
        }

        private ushort CalculateChecksum()
        {
            ushort checksum = 0;
            checksum ^= (ushort)(Key & 0xFFFF);
            checksum ^= (ushort)(Key >> 16);
            checksum ^= HWmf;
            checksum ^= (ushort)BoundingBox.Left;
            checksum ^= (ushort)BoundingBox.Top;
            checksum ^= (ushort)BoundingBox.Right;
            checksum ^= (ushort)BoundingBox.Bottom;
            checksum ^= Inch;
            checksum ^= (ushort)(Reserved & 0xFFFF);
            checksum ^= (ushort)(Reserved >> 16);
            return checksum;
        }
    }
}
