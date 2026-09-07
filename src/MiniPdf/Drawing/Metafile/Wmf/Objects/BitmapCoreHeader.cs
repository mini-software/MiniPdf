namespace MiniSoftware.Drawing.Metafile.Wmf.Objects
{
    using MiniSoftware.Drawing.Metafile.Wmf.Enumerations;
    using System.IO;

    /// <summary>
    ///     The BitmapCoreHeader Object contains information about the dimensions and color format of a device-independent bitmap (DIB).
    /// </summary>
    /// <remarks>
    ///     2.2.2.2 BitmapCoreHeader Object
    /// </remarks>
    internal struct BitmapCoreHeader
    {
        /// <summary>
        ///     A 32-bit unsigned integer that defines the size of this object, in bytes.
        /// </summary>
        public uint HeaderSize;

        /// <summary>
        ///     A 16-bit unsigned integer that defines the width of the DIB, in pixels.
        /// </summary>
        public ushort Width;

        /// <summary>
        ///     A 16-bit unsigned integer that defines the height of the DIB, in pixels.
        /// </summary>
        public ushort Height;

        /// <summary>
        ///     A 16-bit unsigned integer that defines the number of planes for the target device. This value MUST be 0x0001.
        /// </summary>
        public ushort Planes;

        /// <summary>
        ///     A 16-bit unsigned integer that defines the format of each pixel, and the maximum number of colors in the DIB.
        /// </summary>
        public BitCount BitCount;

        public void Read(BinaryReader reader)
        {
            this.HeaderSize = reader.ReadUInt32();
            this.Width = reader.ReadUInt16();
            this.Height = reader.ReadUInt16();
            this.Planes = reader.ReadUInt16();
            this.BitCount = (BitCount)reader.ReadUInt16();
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(this.HeaderSize);
            writer.Write(this.Width);
            writer.Write(this.Height);
            writer.Write(this.Planes);
            writer.Write((ushort)this.BitCount);
        }
    }
}
