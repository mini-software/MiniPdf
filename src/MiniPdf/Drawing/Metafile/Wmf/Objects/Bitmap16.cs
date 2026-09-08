using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniSoftware.Drawing.Metafile.Wmf.Objects
{
    using System.IO;

    /// <summary>
    ///     The Bitmap16 Object specifies information about the dimensions and color format of a bitmap.
    /// </summary>
    /// <remarks>
    ///     2.2.2.1 Bitmap16 Object
    /// </remarks>
    internal struct Bitmap16
    {
        /// <summary>
        ///     A 16-bit signed integer that defines the bitmap type.
        /// </summary>
        public short Type;

        /// <summary>
        ///     A 16-bit signed integer that defines the width of the bitmap in pixels.
        /// </summary>
        public short Width;

        /// <summary>
        ///     A 16-bit signed integer that defines the height of the bitmap in scan lines.
        /// </summary>
        public short Height;

        /// <summary>
        ///     A 16-bit signed integer that defines the number of bytes per scan line.
        /// </summary>
        public short WidthBytes;

        /// <summary>
        ///     An 8-bit unsigned integer that defines the number of color planes in the bitmap. The value of this field MUST be 0x01.
        /// </summary>
        public byte Planes;

        /// <summary>
        ///     An 8-bit unsigned integer that defines the number of adjacent color bits on each plane.
        /// </summary>
        public byte BitsPixel;

        public byte[] Bits;

        public int Length
        {
            get
            {
                return (((Width * BitsPixel + 15) >> 4) << 1) * Height;
            }
        }
        public void Read(BinaryReader reader)
        {
            this.Type = reader.ReadInt16();
            this.Width = reader.ReadInt16();
            this.Height = reader.ReadInt16();
            this.WidthBytes = reader.ReadInt16();
            this.Planes = reader.ReadByte();
            this.BitsPixel = reader.ReadByte();
            this.Bits = reader.ReadBytes(this.Length);
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(this.Type);
            writer.Write(this.Width);
            writer.Write(this.Height);
            writer.Write(this.WidthBytes);
            writer.Write(this.Planes);
            writer.Write(this.BitsPixel);
            if (this.Bits == null || this.Bits.Length != this.Length)
            {
                throw new InvalidOperationException("Bits array is null or does not match the expected length.");
            }
            writer.Write(this.Bits);
        }
    }
}
