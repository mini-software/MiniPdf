using System;
using System.IO;
using MiniPdf.Drawing.Enums;
using MiniPdf.Drawing.Imaging;

namespace MiniPdf.Drawing.Codecs
{
    /// <summary>ICO codec — decode only; selects the largest frame.</summary>
    internal sealed class IcoCodec : IImageCodec
    {
        private static readonly byte[] PngSig = { 137, 80, 78, 71, 13, 10, 26, 10 };

        public bool CanDecode(byte[] header)
            => header.Length >= 4
            && header[0] == 0 && header[1] == 0
            && header[2] == 1 && header[3] == 0;

        // ─── Decode ──────────────────────────────────────────────────────────────

        public Bitmap Decode(Stream stream)
        {
            byte[] file = ReadAll(stream);

            // ICONDIR
            int count = file[4] | (file[5] << 8);
            if (count == 0) throw new InvalidDataException("ICO contains no images.");

            // Pick best entry: largest area, then highest bit-depth
            int bestIdx = 0, bestArea = -1, bestBpp = -1;
            for (int i = 0; i < count; i++)
            {
                int e = 6 + i * 16;
                int fw  = file[e]     == 0 ? 256 : file[e];
                int fh  = file[e + 1] == 0 ? 256 : file[e + 1];
                int bpp = file[e + 6] | (file[e + 7] << 8);
                int area = fw * fh;
                if (area > bestArea || (area == bestArea && bpp > bestBpp))
                { bestArea = area; bestBpp = bpp; bestIdx = i; }
            }

            int dirEntry = 6 + bestIdx * 16;
            int imgW   = file[dirEntry]     == 0 ? 256 : file[dirEntry];
            int imgH   = file[dirEntry + 1] == 0 ? 256 : file[dirEntry + 1];
            int imgBpp = file[dirEntry + 6] | (file[dirEntry + 7] << 8);
            int imgOff = file[dirEntry + 12] | (file[dirEntry + 13] << 8)
                       | (file[dirEntry + 14] << 16) | (file[dirEntry + 15] << 24);

            // Check if the image data is a PNG
            bool isPng = imgOff + 8 <= file.Length;
            if (isPng)
            {
                for (int i = 0; i < 8; i++)
                    if (file[imgOff + i] != PngSig[i]) { isPng = false; break; }
            }

            if (isPng)
            {
                using var ms = new MemoryStream(file, imgOff, file.Length - imgOff);
                return new PngCodec().Decode(ms);
            }

            // Parse BMP DIB header (no BM file-header in ICO)
            int dibSize = file[imgOff]     | (file[imgOff+1] << 8)
                        | (file[imgOff+2] << 16) | (file[imgOff+3] << 24);
            int dibW    = file[imgOff+4]  | (file[imgOff+5]  << 8)
                        | (file[imgOff+6] << 16) | (file[imgOff+7] << 24);
            int rawH    = file[imgOff+8]  | (file[imgOff+9]  << 8)
                        | (file[imgOff+10]<< 16) | (file[imgOff+11]<< 24);
            int actualH = Math.Abs(rawH / 2); // ICO height is doubled
            int dibBpp  = file[imgOff+14] | (file[imgOff+15] << 8);
            if (dibBpp == 0) dibBpp = imgBpp;

            int xorOff  = imgOff + dibSize;
            int srcBpp  = dibBpp / 8;
            int xorStride = ((dibW * dibBpp + 31) / 32) * 4;
            int andStride = ((dibW * 1 + 31) / 32) * 4;
            int andOff    = xorOff + xorStride * actualH;

            var bmp = new Bitmap(dibW, actualH, PixelFormat.Format32bppArgb);
            bmp._rawFormat = ImageFormat.Icon;

            for (int y = 0; y < actualH; y++)
            {
                // XOR mask is stored bottom-up
                int srcRow = (actualH - 1 - y) * xorStride + xorOff;
                for (int x = 0; x < dibW; x++)
                {
                    byte r, g, bl, a;
                    if (dibBpp == 32)
                    {
                        int off = srcRow + x * 4;
                        bl = file[off]; g = file[off+1]; r = file[off+2]; a = file[off+3];
                    }
                    else if (dibBpp == 24)
                    {
                        int off = srcRow + x * 3;
                        bl = file[off]; g = file[off+1]; r = file[off+2];
                        // AND mask: bit=1 → transparent, bit=0 → opaque
                        int andRow = andOff + (actualH - 1 - y) * andStride;
                        int andByte = andRow + x / 8;
                        int andBit  = 7 - (x % 8);
                        a = (andByte < file.Length && ((file[andByte] >> andBit) & 1) != 0) ? (byte)0 : (byte)255;
                    }
                    else
                    {
                        r = g = bl = 0; a = 255;
                    }
                    int dstOff = y * bmp._stride + x * 4;
                    bmp._pixels[dstOff]     = bl;
                    bmp._pixels[dstOff + 1] = g;
                    bmp._pixels[dstOff + 2] = r;
                    bmp._pixels[dstOff + 3] = a;
                }
            }

            return bmp;
        }

        public void Encode(Bitmap bmp, Stream stream, EncoderParameters? parameters)
            => throw new NotSupportedException("ICO encoding is not supported.");

        private static byte[] ReadAll(Stream s)
        {
            using var ms = new MemoryStream();
            s.CopyTo(ms);
            return ms.ToArray();
        }
    }
}
