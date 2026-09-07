using System;
using System.Collections.Generic;
using System.IO;
using MiniPdf.Drawing.Enums;
using MiniPdf.Drawing.Imaging;

namespace MiniPdf.Drawing.Codecs
{
    /// <summary>
    /// GIF87a / GIF89a codec — reads first frame; writes indexed colour with LZW.
    /// </summary>
    internal sealed class GifCodec : IImageCodec
    {
        public bool CanDecode(byte[] header)
            => header.Length >= 6
            && header[0] == 'G' && header[1] == 'I' && header[2] == 'F'
            && header[3] == '8' && (header[4] == '7' || header[4] == '9') && header[5] == 'a';

        // ─── Decode ──────────────────────────────────────────────────────────────

        public Bitmap Decode(Stream stream)
        {
            var data = ReadAll(stream);
            int pos  = 0;

            // Header (6 bytes)
            pos += 6;

            // Logical Screen Descriptor (7 bytes)
            int canvasW   = data[pos] | (data[pos+1] << 8); pos += 2;
            int canvasH   = data[pos] | (data[pos+1] << 8); pos += 2;
            byte packed   = data[pos++];
            bool hasGct   = (packed & 0x80) != 0;
            int  gctSz    = hasGct ? 2 << (packed & 0x07) : 0;
            /*bgIndex*/     pos++; // background colour index
            /*aspect*/      pos++; // aspect ratio

            // Global Colour Table
            byte[]? gct = null;
            if (hasGct)
            {
                gct = new byte[gctSz * 3];
                Buffer.BlockCopy(data, pos, gct, 0, gct.Length);
                pos += gct.Length;
            }

            // GCE fields (set by Graphic Control Extension)
            bool transpFlag  = false;
            int  transpIndex = 0;

            // Parse blocks until we find the first image
            while (pos < data.Length)
            {
                byte blockId = data[pos++];

                if (blockId == 0x3B) break; // trailer

                if (blockId == 0x21) // extension
                {
                    byte extLabel = data[pos++];
                    if (extLabel == 0xF9 && pos + 1 < data.Length) // Graphic Control Extension
                    {
                        int bsize = data[pos++];
                        if (bsize >= 4)
                        {
                            byte gceFlags = data[pos];
                            transpFlag    = (gceFlags & 1) != 0;
                            transpIndex   = data[pos + 3];
                        }
                    }
                    // Skip sub-blocks
                    pos = SkipSubBlocks(data, pos);
                    continue;
                }

                if (blockId == 0x2C) // image descriptor
                {
                    int imgLeft = data[pos] | (data[pos+1] << 8); pos += 2;
                    int imgTop  = data[pos] | (data[pos+1] << 8); pos += 2;
                    int imgW    = data[pos] | (data[pos+1] << 8); pos += 2;
                    int imgH    = data[pos] | (data[pos+1] << 8); pos += 2;
                    byte imgPacked = data[pos++];
                    bool hasLct  = (imgPacked & 0x80) != 0;
                    int  lctSz   = hasLct ? 2 << (imgPacked & 0x07) : 0;

                    byte[]? lct = null;
                    if (hasLct)
                    {
                        lct = new byte[lctSz * 3];
                        Buffer.BlockCopy(data, pos, lct, 0, lct.Length);
                        pos += lct.Length;
                    }

                    byte[] palette = lct ?? gct ?? new byte[0];

                    int minCodeSize = data[pos++];
                    byte[] lzwData  = ReadSubBlocks(data, ref pos);

                    // Decompress LZW
                    var indices = LzwDecode(lzwData, minCodeSize);

                    // Build bitmap
                    int bmpW = canvasW > 0 ? canvasW : imgW;
                    int bmpH = canvasH > 0 ? canvasH : imgH;
                    var bmp  = new Bitmap(bmpW, bmpH, PixelFormat.Format32bppArgb);
                    bmp._rawFormat = ImageFormat.Gif;

                    for (int y = 0; y < imgH && y < bmpH; y++)
                    for (int x = 0; x < imgW && x < bmpW;  x++)
                    {
                        int idx = (y * imgW + x < indices.Length) ? indices[y * imgW + x] : 0;
                        byte r = 0, g = 0, b = 0;
                        if (idx * 3 + 2 < palette.Length)
                        { r = palette[idx*3]; g = palette[idx*3+1]; b = palette[idx*3+2]; }
                        byte a = (transpFlag && idx == transpIndex) ? (byte)0 : (byte)255;
                        int off = (imgTop + y) * bmp._stride + (imgLeft + x) * 4;
                        if (off + 3 < bmp._pixels.Length)
                        { bmp._pixels[off]=b; bmp._pixels[off+1]=g; bmp._pixels[off+2]=r; bmp._pixels[off+3]=a; }
                    }

                    return bmp;
                }

                // Unknown block — try to skip
                pos = SkipSubBlocks(data, pos);
            }

            throw new InvalidDataException("No image frame found in GIF.");
        }

        // ─── Encode ──────────────────────────────────────────────────────────────

        public void Encode(Bitmap bmp, Stream stream, EncoderParameters? parameters)
        {
            // Collect unique colours
            var colorMap = new Dictionary<int, int>();
            var colors   = new List<(byte r, byte g, byte b)>();
            bool hasTransp = false;
            int  transpIdx = -1;

            for (int y = 0; y < bmp._height; y++)
            for (int x = 0; x < bmp._width;  x++)
            {
                var c = bmp.GetPixel(x, y);
                if (c.A < 128) { hasTransp = true; continue; }
                int key = (c.R << 16) | (c.G << 8) | c.B;
                if (!colorMap.ContainsKey(key))
                {
                    colorMap[key] = colors.Count;
                    colors.Add((c.R, c.G, c.B));
                }
            }

            // If image is entirely transparent, add one dummy colour
            if (colors.Count == 0) colors.Add((0, 0, 0));

            // Reserve a slot for transparency if needed
            if (hasTransp)
            {
                transpIdx = colors.Count;
                colors.Add((0, 0, 0));
            }

            // Quantize to ≤ 256 colours if necessary
            if (colors.Count > 256)
            {
                colors = colors.GetRange(0, hasTransp ? 255 : 256);
                if (hasTransp) { transpIdx = 255; colors.Add((0, 0, 0)); }
                // Rebuild colorMap
                colorMap.Clear();
                for (int i = 0; i < colors.Count - (hasTransp ? 1 : 0); i++)
                {
                    var (r2, g2, b2) = colors[i];
                    colorMap[(r2 << 16) | (g2 << 8) | b2] = i;
                }
            }

            int numColors = colors.Count;

            // GCT size: smallest power-of-2 that fits
            int gctPow = 0;
            int tableSize = 2;
            while (tableSize < numColors && gctPow < 7) { gctPow++; tableSize *= 2; }

            int minCodeSize = Math.Max(2, gctPow + 1);

            // Build index array
            var indices = new byte[bmp._width * bmp._height];
            for (int y = 0; y < bmp._height; y++)
            for (int x = 0; x < bmp._width;  x++)
            {
                var c = bmp.GetPixel(x, y);
                if (c.A < 128)
                { indices[y * bmp._width + x] = (byte)(transpIdx >= 0 ? transpIdx : 0); }
                else
                {
                    int key = (c.R << 16) | (c.G << 8) | c.B;
                    indices[y * bmp._width + x] = colorMap.TryGetValue(key, out int idx)
                        ? (byte)idx : (byte)0;
                }
            }

            byte[] lzwData = LzwEncode(indices, minCodeSize);

            // ── Write GIF ────────────────────────────────────────────────────────
            var w = new BinaryWriter(stream, System.Text.Encoding.UTF8, leaveOpen: true);

            // Header
            stream.Write(System.Text.Encoding.ASCII.GetBytes("GIF89a"), 0, 6);

            // Logical Screen Descriptor
            w.Write((ushort)bmp._width);
            w.Write((ushort)bmp._height);
            byte lsdPacked = (byte)(0x80 | (gctPow & 7) | ((gctPow & 7) << 4));
            w.Write(lsdPacked);
            w.Write((byte)0); // background index
            w.Write((byte)0); // aspect ratio

            // Global Colour Table (padded to tableSize entries)
            for (int i = 0; i < tableSize; i++)
            {
                if (i < colors.Count) { var (r2,g2,b2) = colors[i]; w.Write(r2); w.Write(g2); w.Write(b2); }
                else                  { w.Write((byte)0); w.Write((byte)0); w.Write((byte)0); }
            }

            // Graphic Control Extension (only if transparent)
            if (hasTransp && transpIdx >= 0)
            {
                w.Write((byte)0x21); w.Write((byte)0xF9); w.Write((byte)4);
                w.Write((byte)1); // transparent flag
                w.Write((ushort)0); // delay
                w.Write((byte)transpIdx);
                w.Write((byte)0); // block terminator
            }

            // Image Descriptor
            w.Write((byte)0x2C);
            w.Write((ushort)0); w.Write((ushort)0); // left, top
            w.Write((ushort)bmp._width);
            w.Write((ushort)bmp._height);
            w.Write((byte)0); // no local colour table, not interlaced

            // LZW Minimum Code Size
            w.Write((byte)minCodeSize);

            // LZW data in sub-blocks
            WriteSubBlocks(stream, lzwData);

            // Trailer
            w.Write((byte)0x3B);
        }

        // ─── Private helpers ─────────────────────────────────────────────────────

        private static byte[] ReadAll(Stream stream)
        {
            using var ms = new MemoryStream();
            stream.CopyTo(ms);
            return ms.ToArray();
        }

        private static int SkipSubBlocks(byte[] data, int pos)
        {
            while (pos < data.Length)
            {
                int sz = data[pos++];
                if (sz == 0) break;
                pos += sz;
            }
            return pos;
        }

        private static byte[] ReadSubBlocks(byte[] data, ref int pos)
        {
            var result = new List<byte>();
            while (pos < data.Length)
            {
                int sz = data[pos++];
                if (sz == 0) break;
                for (int i = 0; i < sz && pos < data.Length; i++)
                    result.Add(data[pos++]);
            }
            return result.ToArray();
        }

        private static void WriteSubBlocks(Stream s, byte[] data)
        {
            int i = 0;
            while (i < data.Length)
            {
                int sz = Math.Min(255, data.Length - i);
                s.WriteByte((byte)sz);
                s.Write(data, i, sz);
                i += sz;
            }
            s.WriteByte(0); // block terminator
        }

        // ── GIF LZW decoder (LSB-first bit stream) ────────────────────────────

        private static int[] LzwDecode(byte[] data, int minCodeSize)
        {
            int clearCode = 1 << minCodeSize;
            int eoiCode   = clearCode + 1;
            int codeSize  = minCodeSize + 1;
            int codeMask  = (1 << codeSize) - 1;

            // String table: code → list of indices
            var table = new List<int[]>();

            void InitTable()
            {
                table.Clear();
                for (int i = 0; i <= eoiCode; i++)
                    table.Add(i < clearCode ? new[] { i } : Array.Empty<int>());
                codeSize = minCodeSize + 1;
                codeMask = (1 << codeSize) - 1;
            }

            InitTable();

            var output = new List<int>();
            int bitBuf  = 0, bitsLeft = 0, dataPos = 0;

            int ReadCode()
            {
                while (bitsLeft < codeSize && dataPos < data.Length)
                {
                    bitBuf   |= data[dataPos++] << bitsLeft;
                    bitsLeft += 8;
                }
                int code  = bitBuf & codeMask;
                bitBuf  >>= codeSize;
                bitsLeft -= codeSize;
                return code;
            }

            int prevCode = -1;

            while (true)
            {
                int code = ReadCode();
                if (code == eoiCode) break;

                if (code == clearCode)
                {
                    InitTable();
                    prevCode = -1;
                    continue;
                }

                int[] entry;
                if (code < table.Count && table[code].Length > 0)
                {
                    entry = table[code];
                }
                else if (code == table.Count && prevCode >= 0)
                {
                    // Special case: code == next available, entry = prev + prev[0]
                    var prev2 = table[prevCode];
                    entry = new int[prev2.Length + 1];
                    Buffer.BlockCopy(prev2, 0, entry, 0, prev2.Length * sizeof(int));
                    entry[prev2.Length] = prev2[0];
                }
                else
                {
                    break; // corrupt stream
                }

                foreach (var idx in entry) output.Add(idx);

                if (prevCode >= 0)
                {
                    var prevEntry = table[prevCode];
                    var newEntry  = new int[prevEntry.Length + 1];
                    Buffer.BlockCopy(prevEntry, 0, newEntry, 0, prevEntry.Length * sizeof(int));
                    newEntry[prevEntry.Length] = entry[0];
                    table.Add(newEntry);

                    if (table.Count == (1 << codeSize) && codeSize < 12)
                    {
                        codeSize++;
                        codeMask = (1 << codeSize) - 1;
                    }
                }

                prevCode = code;
            }

            return output.ToArray();
        }

        // ── GIF LZW encoder (LSB-first bit stream) ────────────────────────────

        private static byte[] LzwEncode(byte[] indices, int minCodeSize)
        {
            int clearCode = 1 << minCodeSize;
            int eoiCode   = clearCode + 1;

            var  output    = new List<byte>();
            int  bitBuf    = 0, bitsLeft = 0;
            int  codeSize  = minCodeSize + 1;
            int  nextCode  = eoiCode + 1;
            var  dict      = new Dictionary<(int prefix, byte suffix), int>();

            void Flush()
            {
                while (bitsLeft >= 8) { output.Add((byte)(bitBuf & 0xFF)); bitBuf >>= 8; bitsLeft -= 8; }
            }

            void EmitCode(int code)
            {
                bitBuf   |= code << bitsLeft;
                bitsLeft += codeSize;
                Flush();
            }

            void Reset()
            {
                dict.Clear();
                nextCode  = eoiCode + 1;
                codeSize  = minCodeSize + 1;
            }

            EmitCode(clearCode);

            if (indices.Length == 0)
            {
                EmitCode(eoiCode);
                if (bitsLeft > 0) { output.Add((byte)(bitBuf & 0xFF)); }
                return output.ToArray();
            }

            int cur = indices[0];

            for (int i = 1; i < indices.Length; i++)
            {
                byte b = indices[i];
                if (dict.TryGetValue((cur, b), out int next))
                {
                    cur = next;
                }
                else
                {
                    EmitCode(cur);
                    if (nextCode < 4096)
                    {
                        dict[(cur, b)] = nextCode++;
                        if (nextCode > (1 << codeSize) && codeSize < 12)
                            codeSize++;
                    }
                    else
                    {
                        EmitCode(clearCode);
                        Reset();
                    }
                    cur = b;
                }
            }

            EmitCode(cur);
            EmitCode(eoiCode);
            if (bitsLeft > 0) { output.Add((byte)(bitBuf & 0xFF)); }

            return output.ToArray();
        }
    }
}
