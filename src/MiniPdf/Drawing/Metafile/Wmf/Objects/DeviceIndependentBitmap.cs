using MiniSoftware.Drawing.Metafile.Wmf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniSoftware.Drawing.Metafile.Wmf.Objects
{
    using MiniSoftware.Drawing.Metafile.Wmf.Enumerations;
    using System.IO;

    /// <summary>
    ///     The DeviceIndependentBitmap Object defines an image in device-independent bitmap (DIB) format.
    /// </summary>
    /// <remarks>
    ///     2.2.2.9 DeviceIndependentBitmap Object
    /// </remarks>
    internal class DeviceIndependentBitmap
    {
        public BitmapCoreHeader? BitmapCoreHeader { get; set; }

        public BitmapInfoHeader? BitmapInfoHeader { get; set; }

        public RGBQuad[]? Colors { get; set; }

        public byte[]? aData { get; set; }

        public void Read(BinaryReader reader)
        {
            long initialPosition = reader.BaseStream.Position;
            uint headerSize = reader.ReadUInt32();
            reader.BaseStream.Position = initialPosition;

            int colorCount = 0;
            if (headerSize == 12) // BitmapCoreHeader
            {
                var header = new BitmapCoreHeader();
                header.Read(reader);
                this.BitmapCoreHeader = header;
                if ((int)header.BitCount <= 8)
                    colorCount = 1 << (int)header.BitCount;
            }
            else
            {
                var header = new BitmapInfoHeader();
                header.Read(reader);
                this.BitmapInfoHeader = header;
                if (header.ColorUsed != 0)
                {
                    colorCount = (int)header.ColorUsed;
                }
                else if ((int)header.BitCount <= 8)
                {
                    colorCount = 1 << (int)header.BitCount;
                }
            }

            if (colorCount > 0)
            {
                this.Colors = new RGBQuad[colorCount];
                for (int i = 0; i < colorCount; i++)
                {
                    this.Colors[i] = new RGBQuad();
                    this.Colors[i].Read(reader);
                }
            }

            uint imageSize = this.BitmapInfoHeader?.ImageSize ?? 0;
            if (imageSize > 0)
            {
                 this.aData = reader.ReadBytes((int)imageSize);
            }
            else
            {
                // If ImageSize is not set, we need to calculate it based on dimensions and BitCount
                int width = this.BitmapInfoHeader?.Width ?? this.BitmapCoreHeader?.Width ?? 0;
                int height = this.BitmapInfoHeader?.Height ?? this.BitmapCoreHeader?.Height ?? 0;
                BitCount bitCount = this.BitmapInfoHeader?.BitCount ?? this.BitmapCoreHeader?.BitCount ?? 0;

                if (width > 0 && height > 0 && bitCount > 0)
                {
                    // Calculate row size (stride), rounded up to the nearest 4-byte boundary
                    int stride = (((width * (int)bitCount) + 31) & ~31) / 8;
                    imageSize = (uint)(stride * Math.Abs(height));
                    this.aData = reader.ReadBytes((int)imageSize);
                }
            }
        }

        public void Write(BinaryWriter writer)
        {
            if (BitmapCoreHeader.HasValue)
            {
                BitmapCoreHeader.Value.Write(writer);
            }
            else if (BitmapInfoHeader.HasValue)
            {
                BitmapInfoHeader.Value.Write(writer);
            }

            if (Colors != null)
            {
                foreach (var color in Colors)
                {
                    color.Write(writer);
                }
            }

            if (aData != null)
            {
                writer.Write(aData);
            }
        }
    }
}
