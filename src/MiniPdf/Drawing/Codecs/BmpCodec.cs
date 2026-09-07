using System;
using System.IO;
using MiniSoftware.Drawing.Enums;
using MiniSoftware.Drawing.Imaging;

namespace MiniSoftware.Drawing.Codecs
{
    /// <summary>BMP / DIB codec (native, no third-party library).</summary>
    internal sealed class BmpCodec : IImageCodec
    {
        public bool CanDecode(byte[] header)
            => header.Length >= 2 && header[0] == (byte)'B' && header[1] == (byte)'M';

        public Bitmap Decode(Stream stream)
        {
            using var br = new BinaryReader(stream, System.Text.Encoding.UTF8, leaveOpen: true);

            // File header (14 bytes)
            br.ReadByte(); br.ReadByte();  // 'B','M'
            br.ReadUInt32();               // file size
            br.ReadUInt16(); br.ReadUInt16(); // reserved
            br.ReadUInt32();               // pixelOffset (unused — we rely on sequential read)

            // DIB header (BITMAPINFOHEADER, 40 bytes minimum)
            uint headerSize = br.ReadUInt32();
            int  width      = br.ReadInt32();
            int  height     = br.ReadInt32();
            bool topDown    = height < 0;
            if (topDown) height = -height;

            br.ReadUInt16();               // planes
            ushort bpp         = br.ReadUInt16();
            uint   compression = br.ReadUInt32();
            br.ReadUInt32();               // image size
            int xPPM = br.ReadInt32();
            int yPPM = br.ReadInt32();
            br.ReadUInt32(); br.ReadUInt32(); // clrUsed, clrImportant

            // Skip any extra header bytes
            int extra = (int)headerSize - 40;
            if (extra > 0) br.ReadBytes(extra);

            float dpiX = xPPM > 0 ? xPPM * 0.0254f : 96f;
            float dpiY = yPPM > 0 ? yPPM * 0.0254f : 96f;

            var bmp = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            bmp._dpiX     = dpiX;
            bmp._dpiY     = dpiY;
            bmp._rawFormat = ImageFormat.Bmp;

            int srcBpp    = bpp / 8;
            int srcStride = ((width * bpp + 31) / 32) * 4;
            var rowBuf    = new byte[srcStride];

            for (int yi = 0; yi < height; yi++)
            {
                int dstY = topDown ? yi : height - 1 - yi;
                int read = br.Read(rowBuf, 0, srcStride);
                if (read < srcStride && read > 0)
                    Array.Clear(rowBuf, read, srcStride - read);

                int dstBase = dstY * bmp._stride;
                for (int x = 0; x < width; x++)
                {
                    int srcOff = x * srcBpp;
                    int dstOff = dstBase + x * 4;
                    bmp._pixels[dstOff]     = rowBuf[srcOff];
                    bmp._pixels[dstOff + 1] = rowBuf[srcOff + 1];
                    bmp._pixels[dstOff + 2] = rowBuf[srcOff + 2];
                    bmp._pixels[dstOff + 3] = srcBpp >= 4 ? rowBuf[srcOff + 3] : (byte)255;
                }
            }

            return bmp;
        }

        public void Encode(Bitmap bmp, Stream stream, EncoderParameters? parameters)
        {
            int bpp      = (bmp._format == PixelFormat.Format32bppArgb
                         || bmp._format == PixelFormat.Format32bppRgb
                         || bmp._format == PixelFormat.Format32bppPArgb) ? 32 : 24;
            int rowBytes = ((bmp._width * bpp + 31) / 32) * 4;
            int dataSize = rowBytes * bmp._height;

            using var bw = new BinaryWriter(stream, System.Text.Encoding.UTF8, leaveOpen: true);

            // File header (14 bytes)
            bw.Write((byte)'B'); bw.Write((byte)'M');
            bw.Write((uint)(54 + dataSize));
            bw.Write((ushort)0); bw.Write((ushort)0);
            bw.Write((uint)54);

            // DIB header – BITMAPINFOHEADER (40 bytes)
            bw.Write((uint)40);
            bw.Write((int)bmp._width);
            bw.Write((int)bmp._height);  // positive = bottom-up
            bw.Write((ushort)1);          // color planes
            bw.Write((ushort)bpp);
            bw.Write((uint)0);            // BI_RGB
            bw.Write((uint)0);            // image size (0 for BI_RGB)
            bw.Write((int)Math.Round(bmp._dpiX / 0.0254));
            bw.Write((int)Math.Round(bmp._dpiY / 0.0254));
            bw.Write((uint)0);
            bw.Write((uint)0);

            // Pixel data – BMP stores rows bottom-up
            int srcBpp = Bitmap.GetBytesPerPixel(bmp._format);
            int dstBpp = bpp / 8;
            var rowBuf = new byte[rowBytes];

            for (int yi = bmp._height - 1; yi >= 0; yi--)
            {
                int srcBase = yi * bmp._stride;
                for (int x = 0; x < bmp._width; x++)
                {
                    int srcOff = srcBase + x * srcBpp;
                    int dstOff = x * dstBpp;
                    rowBuf[dstOff]     = bmp._pixels[srcOff];
                    rowBuf[dstOff + 1] = bmp._pixels[srcOff + 1];
                    rowBuf[dstOff + 2] = bmp._pixels[srcOff + 2];
                    if (dstBpp == 4)
                        rowBuf[dstOff + 3] = srcBpp >= 4 ? bmp._pixels[srcOff + 3] : (byte)255;
                }
                for (int p = bmp._width * dstBpp; p < rowBytes; p++) rowBuf[p] = 0;
                bw.Write(rowBuf, 0, rowBytes);
            }
        }
    }
}
