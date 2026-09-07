using System;
using System.IO;

namespace MiniSoftware.Drawing.Codecs.Png
{
    internal static class PngHelpers
    {
        public static int GetChannels(int colorType)
        {
            switch (colorType)
            {
                case 0: return 1;
                case 2: return 3;
                case 3: return 1;
                case 4: return 2;
                case 6: return 4;
                default: return 3;
            }
        }

        public static byte[] ReconstructFilters(byte[] raw, int width, int height, int stride, int bpp)
        {
            var result = new byte[height * stride];
            var prev = new byte[stride];

            for (int y = 0; y < height; y++)
            {
                int rowOff = y * (stride + 1);
                int filterType = raw[rowOff];
                int dstOff = y * stride;

                switch (filterType)
                {
                    case 0:
                        Buffer.BlockCopy(raw, rowOff + 1, result, dstOff, stride);
                        break;
                    case 1:
                        for (int i = 0; i < stride; i++)
                        {
                            byte a = i >= bpp ? result[dstOff + i - bpp] : (byte)0;
                            result[dstOff + i] = (byte)(raw[rowOff + 1 + i] + a);
                        }
                        break;
                    case 2:
                        for (int i = 0; i < stride; i++)
                            result[dstOff + i] = (byte)(raw[rowOff + 1 + i] + prev[i]);
                        break;
                    case 3:
                        for (int i = 0; i < stride; i++)
                        {
                            byte a = i >= bpp ? result[dstOff + i - bpp] : (byte)0;
                            byte b = prev[i];
                            result[dstOff + i] = (byte)(raw[rowOff + 1 + i] + (a + b) / 2);
                        }
                        break;
                    case 4:
                        for (int i = 0; i < stride; i++)
                        {
                            byte a = i >= bpp ? result[dstOff + i - bpp] : (byte)0;
                            byte b = prev[i];
                            byte c = i >= bpp ? prev[i - bpp] : (byte)0;
                            result[dstOff + i] = (byte)(raw[rowOff + 1 + i] + PaethPredictor(a, b, c));
                        }
                        break;
                    default:
                        Buffer.BlockCopy(raw, rowOff + 1, result, dstOff, stride);
                        break;
                }

                Buffer.BlockCopy(result, dstOff, prev, 0, stride);
            }

            return result;
        }

        public static byte PaethPredictor(byte a, byte b, byte c)
        {
            int p = a + b - c;
            int pa = Math.Abs(p - a);
            int pb = Math.Abs(p - b);
            int pc = Math.Abs(p - c);
            if (pa <= pb && pa <= pc) return a;
            if (pb <= pc) return b;
            return c;
        }

        public static void GetPixelRgba(byte[] pixels, int x, int y,
            int stride, int bitDepth, int colorType,
            byte[]? palette, byte[]? trns,
            out byte r, out byte g, out byte b, out byte a)
        {
            int rowOff = y * stride;
            r = g = b = 0; a = 255;

            switch (colorType)
            {
                case 0:
                {
                    byte grey;
                    if (bitDepth == 8)
                    {
                        grey = pixels[rowOff + x];
                    }
                    else if (bitDepth == 16)
                    {
                        grey = pixels[rowOff + x * 2];
                    }
                    else
                    {
                        int samplesPerByte = 8 / bitDepth;
                        int byteIdx = x / samplesPerByte;
                        int bitShift = 8 - bitDepth - (x % samplesPerByte) * bitDepth;
                        int mask = (1 << bitDepth) - 1;
                        int raw = (pixels[rowOff + byteIdx] >> bitShift) & mask;
                        grey = (byte)(raw * 255 / mask);
                    }
                    r = g = b = grey;
                    if (trns != null && trns.Length >= 2)
                    {
                        int trnsVal = (trns[0] << 8) | trns[1];
                        a = (grey == (byte)(trnsVal >> (bitDepth == 8 ? 0 : 8))) ? (byte)0 : (byte)255;
                    }
                    break;
                }
                case 2:
                {
                    if (bitDepth == 8)
                    {
                        int off = rowOff + x * 3;
                        r = pixels[off]; g = pixels[off + 1]; b = pixels[off + 2];
                    }
                    else
                    {
                        int off = rowOff + x * 6;
                        r = pixels[off]; g = pixels[off + 2]; b = pixels[off + 4];
                    }
                    if (trns != null && trns.Length >= 6)
                    {
                        if (r == trns[1] && g == trns[3] && b == trns[5]) a = 0;
                    }
                    break;
                }
                case 3:
                {
                    int idx;
                    if (bitDepth == 8)
                    {
                        idx = pixels[rowOff + x];
                    }
                    else
                    {
                        int samplesPerByte = 8 / bitDepth;
                        int byteIdx = x / samplesPerByte;
                        int bitShift = 8 - bitDepth - (x % samplesPerByte) * bitDepth;
                        int mask = (1 << bitDepth) - 1;
                        idx = (pixels[rowOff + byteIdx] >> bitShift) & mask;
                    }
                    if (palette != null && idx * 3 + 2 < palette.Length)
                    {
                        r = palette[idx * 3];
                        g = palette[idx * 3 + 1];
                        b = palette[idx * 3 + 2];
                    }
                    a = (trns != null && idx < trns.Length) ? trns[idx] : (byte)255;
                    break;
                }
                case 4:
                {
                    if (bitDepth == 8)
                    {
                        int off = rowOff + x * 2;
                        r = g = b = pixels[off]; a = pixels[off + 1];
                    }
                    else
                    {
                        int off = rowOff + x * 4;
                        r = g = b = pixels[off]; a = pixels[off + 2];
                    }
                    break;
                }
                case 6:
                {
                    if (bitDepth == 8)
                    {
                        int off = rowOff + x * 4;
                        r = pixels[off]; g = pixels[off + 1]; b = pixels[off + 2]; a = pixels[off + 3];
                    }
                    else
                    {
                        int off = rowOff + x * 8;
                        r = pixels[off]; g = pixels[off + 2]; b = pixels[off + 4]; a = pixels[off + 6];
                    }
                    break;
                }
            }
        }

        public static void ReadFull(Stream s, byte[] buf, int offset, int count)
        {
            int total = 0;
            while (total < count)
            {
                int n = s.Read(buf, offset + total, count - total);
                if (n <= 0) throw new EndOfStreamException("Unexpected end of PNG stream.");
                total += n;
            }
        }

        public static void WriteChunk(Stream s, string type, byte[] data)
        {
            var typeBuf = System.Text.Encoding.ASCII.GetBytes(type);

            var lenBuf = new byte[4];
            BinaryBE.WriteUInt32(lenBuf, 0, (uint)data.Length);
            s.Write(lenBuf, 0, 4);
            s.Write(typeBuf, 0, 4);
            if (data.Length > 0) s.Write(data, 0, data.Length);

            var combined = new byte[4 + data.Length];
            Buffer.BlockCopy(typeBuf, 0, combined, 0, 4);
            if (data.Length > 0) Buffer.BlockCopy(data, 0, combined, 4, data.Length);
            uint crc = Crc32.Compute(combined, 0, combined.Length);

            s.WriteByte((byte)(crc >> 24));
            s.WriteByte((byte)(crc >> 16));
            s.WriteByte((byte)(crc >> 8));
            s.WriteByte((byte)crc);
        }
    }
}