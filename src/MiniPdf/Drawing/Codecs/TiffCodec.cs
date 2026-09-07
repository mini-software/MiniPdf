using System;
using System.Collections.Generic;
using System.IO;
using MiniSoftware.Drawing.Enums;
using MiniSoftware.Drawing.Imaging;

namespace MiniSoftware.Drawing.Codecs
{
    /// <summary>
    /// TIFF codec — write/read uncompressed baseline TIFF (little-endian).
    /// Supports RGB (samples-per-pixel = 3) and RGBA (spp = 4).
    /// </summary>
    internal sealed class TiffCodec : IImageCodec
    {
        public bool CanDecode(byte[] header)
            => header.Length >= 4
            && ((header[0] == 0x49 && header[1] == 0x49 && header[2] == 0x2A && header[3] == 0x00) // LE
             || (header[0] == 0x4D && header[1] == 0x4D && header[2] == 0x00 && header[3] == 0x2A)); // BE

        // ─── Decode ──────────────────────────────────────────────────────────────

        public Bitmap Decode(Stream stream)
        {
            byte[] file = ReadAll(stream);
            bool isLE = file[0] == 0x49;

            uint ifdOff = ReadU32(file, 4, isLE);
            uint ifdCount = ReadU16(file, (int)ifdOff, isLE);

            uint imageWidth  = 0, imageHeight  = 0;
            uint compression = 1;
            uint photoInterp = 2;
            uint spp         = 3;
            uint bitsPerSamp = 8;
            uint stripOff    = 0;
            uint stripBytes  = 0;
            uint extraSamples = 0;

            int entryBase = (int)ifdOff + 2;
            for (int i = 0; i < ifdCount; i++)
            {
                int e = entryBase + i * 12;
                uint tag   = ReadU16(file, e, isLE);
                uint type  = ReadU16(file, e + 2, isLE);
                uint count = ReadU32(file, e + 4, isLE);
                uint valOff = ReadU32(file, e + 8, isLE);

                // If value fits in 4 bytes it's stored inline (LE: LSB first)
                switch (tag)
                {
                    case 256: imageWidth  = GetU32Val(file, e, type, count, valOff, isLE); break;
                    case 257: imageHeight = GetU32Val(file, e, type, count, valOff, isLE); break;
                    case 258: bitsPerSamp = GetU16InlineVal(file, e, isLE); break;
                    case 259: compression = GetU16InlineVal(file, e, isLE); break;
                    case 262: photoInterp = GetU16InlineVal(file, e, isLE); break;
                    case 273: stripOff   = GetU32Val(file, e, type, count, valOff, isLE); break;
                    case 277: spp        = GetU16InlineVal(file, e, isLE); break;
                    case 279: stripBytes = GetU32Val(file, e, type, count, valOff, isLE); break;
                    case 338: extraSamples = GetU16InlineVal(file, e, isLE); break;
                }
            }

            if (compression != 1)
                throw new NotSupportedException("Only uncompressed TIFF (compression=1) is supported.");

            int w = (int)imageWidth, h = (int)imageHeight;
            bool hasAlpha = (spp == 4 && extraSamples == 1)
                         || (spp == 4 && photoInterp == 2);

            var bmp = new Bitmap(w, h, PixelFormat.Format32bppArgb);
            bmp._rawFormat = ImageFormat.Tiff;

            int srcSpp = (int)spp;
            for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                int srcOff = (int)stripOff + (y * w + x) * srcSpp;
                byte r = file[srcOff];
                byte g = file[srcOff + 1];
                byte bl = file[srcOff + 2];
                byte a  = (srcSpp >= 4) ? file[srcOff + 3] : (byte)255;
                int dstOff = y * bmp._stride + x * 4;
                bmp._pixels[dstOff]     = bl;
                bmp._pixels[dstOff + 1] = g;
                bmp._pixels[dstOff + 2] = r;
                bmp._pixels[dstOff + 3] = a;
            }

            return bmp;
        }

        // ─── Encode ──────────────────────────────────────────────────────────────
        //
        // Layout (LE, uncompressed):
        //   [8 header] [IFD: 2+12*n+4] [BitsPerSample array] [XRes RATIONAL] [YRes RATIONAL] [pixels]
        //
        public void Encode(Bitmap bmp, Stream stream, EncoderParameters? parameters)
        {
            bool hasAlpha = bmp._format == PixelFormat.Format32bppArgb
                         || bmp._format == PixelFormat.Format32bppPArgb;
            int spp       = hasAlpha ? 4 : 3;
            int w         = bmp._width, h = bmp._height;
            int pixelSize = w * h * spp;
            int numTags   = hasAlpha ? 13 : 12;

            // Offsets (all relative to file start)
            // header=8, IFD=8, ifd size = 2 + 12*numTags + 4
            int ifdOff    = 8;
            int ifdSize   = 2 + 12 * numTags + 4;
            int bpsOff    = ifdOff + ifdSize;          // BitsPerSample SHORT[spp]
            int bpsSize   = spp * 2;
            int xResOff   = bpsOff + bpsSize;          // XResolution RATIONAL
            int yResOff   = xResOff + 8;               // YResolution RATIONAL
            int pixelOff  = yResOff + 8;

            // Build IFD entries (sorted by tag)
            var tags = new List<(ushort tag, ushort type, uint count, uint val)>();

            void AddTag(ushort tag, ushort type, uint count, uint val)
                => tags.Add((tag, type, count, val));

            const ushort SHORT  = 3;
            const ushort LONG   = 4;
            const ushort RATIONAL = 5;

            AddTag(256, LONG,     1,          (uint)w);
            AddTag(257, LONG,     1,          (uint)h);
            AddTag(258, SHORT,    (uint)spp,  (uint)bpsOff); // offset to BPS array
            AddTag(259, SHORT,    1,          1);             // Compression = none
            AddTag(262, SHORT,    1,          2);             // RGB
            AddTag(273, LONG,     1,          (uint)pixelOff);
            AddTag(277, SHORT,    1,          (uint)spp);
            AddTag(278, LONG,     1,          (uint)h);
            AddTag(279, LONG,     1,          (uint)pixelSize);
            AddTag(282, RATIONAL, 1,          (uint)xResOff);
            AddTag(283, RATIONAL, 1,          (uint)yResOff);
            AddTag(296, SHORT,    1,          2);             // ResolutionUnit = inch
            if (hasAlpha)
                AddTag(338, SHORT, 1,         1);             // ExtraSamples = unassociated alpha

            // ── Write ────────────────────────────────────────────────────────────
            using var bw = new BinaryWriter(stream, System.Text.Encoding.UTF8, leaveOpen: true);

            // Header
            bw.Write((byte)'I'); bw.Write((byte)'I'); // little-endian
            bw.Write((ushort)42);
            bw.Write((uint)ifdOff);

            // IFD
            bw.Write((ushort)numTags);
            foreach (var (tag, type, count, val) in tags)
            {
                bw.Write(tag); bw.Write(type); bw.Write(count); bw.Write(val);
            }
            bw.Write((uint)0); // next IFD offset = 0

            // BitsPerSample array (8,8,8 or 8,8,8,8)
            for (int i = 0; i < spp; i++) bw.Write((ushort)8);

            // XResolution and YResolution RATIONALs (96 dpi = 96/1)
            uint dpiN = (uint)Math.Round(bmp._dpiX);
            bw.Write(dpiN); bw.Write((uint)1);
            uint dpiNy = (uint)Math.Round(bmp._dpiY);
            bw.Write(dpiNy); bw.Write((uint)1);

            // Pixel data: top-down, RGB or RGBA (not BGRA)
            for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                int srcOff = y * bmp._stride + x * Bitmap.GetBytesPerPixel(bmp._format);
                byte b = bmp._pixels[srcOff];
                byte g = bmp._pixels[srcOff + 1];
                byte r = bmp._pixels[srcOff + 2];
                byte a = Bitmap.GetBytesPerPixel(bmp._format) >= 4 ? bmp._pixels[srcOff + 3] : (byte)255;
                bw.Write(r); bw.Write(g); bw.Write(b);
                if (hasAlpha) bw.Write(a);
            }
        }

        // ─── Private helpers ─────────────────────────────────────────────────────

        private static byte[] ReadAll(Stream s)
        {
            using var ms = new MemoryStream();
            s.CopyTo(ms);
            return ms.ToArray();
        }

        private static uint ReadU32(byte[] d, int off, bool isLE)
            => isLE
             ? (uint)(d[off] | (d[off+1]<<8) | (d[off+2]<<16) | (d[off+3]<<24))
             : (uint)((d[off]<<24) | (d[off+1]<<16) | (d[off+2]<<8) | d[off+3]);

        private static uint ReadU16(byte[] d, int off, bool isLE)
            => isLE ? (uint)(d[off] | (d[off+1] << 8))
                    : (uint)((d[off] << 8) | d[off+1]);

        // Return the 32-bit value for an IFD entry, handling inline vs. offset storage
        private static uint GetU32Val(byte[] file, int entryBase, uint type, uint count, uint valOff, bool isLE)
        {
            // LONG (4) × 1 → inline; SHORT (3) × 1 → inline (low 2 bytes)
            if (type == 4 && count == 1) return ReadU32(file, entryBase + 8, isLE);
            if (type == 3 && count == 1) return ReadU16(file, entryBase + 8, isLE);
            // Offset case (e.g. multiple strips — just return first)
            if (type == 4) return ReadU32(file, (int)valOff, isLE);
            if (type == 3) return ReadU16(file, (int)valOff, isLE);
            return valOff;
        }

        // Return inline SHORT value (first 2 bytes of value/offset field)
        private static uint GetU16InlineVal(byte[] file, int entryBase, bool isLE)
            => ReadU16(file, entryBase + 8, isLE);
    }
}
