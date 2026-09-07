using MiniSoftware.Drawing.Metafile.Wmf;

namespace MiniSoftware.Drawing.Metafile.Wmf.Objects
{
    using MiniSoftware.Drawing.Metafile.Wmf.Enumerations;
    using System.IO;

    /// <summary>
    ///     The BitmapInfoHeader Object contains information about the dimensions and color format of a device-independent bitmap (DIB).
    /// </summary>
    /// <remarks>
    ///     2.2.2.3 BitmapInfoHeader Object
    /// </remarks>
    internal struct BitmapInfoHeader
    {
        /// <summary>
        ///     A 32-bit unsigned integer that defines the size of this object, in bytes.
        /// </summary>
        public uint HeaderSize;

        /// <summary>
        /// A 32-bit signed integer that defines the width of the DIB, in pixels. This value MUST be positive.
        /// </summary>
        public int Width;

        /// <summary>
        ///     A 32-bit signed integer that defines the height of the DIB, in pixels. This value MUST NOT be zero.
        /// </summary>
        public int Height;

        /// <summary>
        ///     A 16-bit unsigned integer that defines the number of planes for the target device. This value MUST be 0x0001.
        /// </summary>
        public ushort Planes;

        /// <summary>
        ///     A 16-bit unsigned integer that defines the number of bits that define each pixel and the maximum number of colors in the DIB.
        /// </summary>
        public BitCount BitCount;

        /// <summary>
        ///     A 32-bit unsigned integer that defines the compression mode of the DIB.
        /// </summary>
        public Compression Compression;

        /// <summary>
        ///     A 32-bit unsigned integer that defines the size, in bytes, of the image.
        /// </summary>
        public uint ImageSize;

        /// <summary>
        ///     A 32-bit signed integer that defines the horizontal resolution, in pixels-per-meter, of the target device for the DIB.
        /// </summary>
        public int XPelsPerMeter;

        /// <summary>
        ///     A 32-bit signed integer that defines the vertical resolution, in pixels-per-meter, of the target device for the DIB.
        /// </summary>
        public int YPelsPerMeter;

        /// <summary>
        ///     32-bit unsigned integer that specifies the number of indexes in the color table used by the DIB, as follows:
        /// </summary>
        public uint ColorUsed;

        /// <summary>
        ///     A 32-bit unsigned integer that defines the number of color indexes that are required for displaying the DIB. If this value is zero, all color indexes are required.
        /// </summary>
        public uint ColorImportant;

        public void Write(BinaryWriter writer)
        {
            writer.Write(this.HeaderSize);
            writer.Write(this.Width);
            writer.Write(this.Height);
            writer.Write(this.Planes);
            writer.Write((ushort)this.BitCount);
            writer.Write((uint)this.Compression);
            writer.Write(this.ImageSize);
            writer.Write(this.XPelsPerMeter);
            writer.Write(this.YPelsPerMeter);
            writer.Write(this.ColorUsed);
            writer.Write(this.ColorImportant);
        }

        public void Read(BinaryReader reader)
        {
            this.HeaderSize = reader.ReadUInt32();
            this.Width = reader.ReadInt32();
            this.Height = reader.ReadInt32();
            this.Planes = reader.ReadUInt16();
            this.BitCount = (BitCount)reader.ReadUInt16();
            this.Compression = (Compression)reader.ReadUInt32();
            this.ImageSize = reader.ReadUInt32();
            this.XPelsPerMeter = reader.ReadInt32();
            this.YPelsPerMeter = reader.ReadInt32();
            this.ColorUsed = reader.ReadUInt32();
            this.ColorImportant = reader.ReadUInt32();
        }
    }
}
