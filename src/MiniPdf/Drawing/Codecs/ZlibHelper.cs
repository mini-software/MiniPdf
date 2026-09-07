using System;
using System.IO;
using System.IO.Compression;

namespace MiniPdf.Drawing.Codecs
{
    // ─────────────────────────────────────────────────────────────────────────
    // CRC-32 (ISO 3309 / PNG use)
    // ─────────────────────────────────────────────────────────────────────────
    internal static class Crc32
    {
        private static readonly uint[] _table = BuildTable();

        private static uint[] BuildTable()
        {
            var t = new uint[256];
            for (uint n = 0; n < 256; n++)
            {
                uint c = n;
                for (int k = 0; k < 8; k++)
                    c = (c & 1) != 0 ? 0xEDB88320u ^ (c >> 1) : (c >> 1);
                t[n] = c;
            }
            return t;
        }

        public static uint Compute(byte[] buf, int offset, int length)
        {
            uint crc = 0xFFFFFFFF;
            for (int i = offset; i < offset + length; i++)
                crc = _table[(crc ^ buf[i]) & 0xFF] ^ (crc >> 8);
            return ~crc;
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Zlib helpers (wrap DeflateStream, add 2-byte header and Adler-32 tail)
    // ─────────────────────────────────────────────────────────────────────────
    internal static class ZlibHelper
    {
        /// <summary>
        /// Decompress a zlib-framed deflate block (2-byte header + deflate + 4-byte Adler32).
        /// </summary>
        public static byte[] Decompress(byte[] zlibData)
        {
            if (zlibData == null || zlibData.Length < 6)
                throw new InvalidDataException("Zlib stream too short.");

            // Skip the 2-byte zlib header; ignore the 4-byte Adler32 trailer
            int deflateLength = zlibData.Length - 6;
            if (deflateLength < 0) deflateLength = 0;

            using var input  = new MemoryStream(zlibData, 2, deflateLength);
            using var ds     = new DeflateStream(input, CompressionMode.Decompress);
            using var output = new MemoryStream();
            ds.CopyTo(output);
            return output.ToArray();
        }

        /// <summary>
        /// Compress <paramref name="data"/> into a zlib-framed deflate block.
        /// </summary>
        public static byte[] Compress(byte[] data)
        {
            using var output = new MemoryStream();

            // zlib header: CMF = 0x78 (deflate, window 32 K), FLG = 0x9C (default, no dict)
            // 0x789C is divisible by 31, as required by the spec.
            output.WriteByte(0x78);
            output.WriteByte(0x9C);

            using (var ds = new DeflateStream(output, CompressionMode.Compress, leaveOpen: true))
                ds.Write(data, 0, data.Length);

            // Adler-32 checksum of original data, big-endian
            uint adler = ComputeAdler32(data, 0, data.Length);
            output.WriteByte((byte)(adler >> 24));
            output.WriteByte((byte)(adler >> 16));
            output.WriteByte((byte)(adler >>  8));
            output.WriteByte((byte) adler);

            return output.ToArray();
        }

        internal static uint ComputeAdler32(byte[] data, int offset, int length)
        {
            const uint ModAdler = 65521;
            uint a = 1, b = 0;
            for (int i = offset; i < offset + length; i++)
            {
                a = (a + data[i]) % ModAdler;
                b = (b + a)       % ModAdler;
            }
            return (b << 16) | a;
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Big-endian read helpers shared by PNG / TIFF
    // ─────────────────────────────────────────────────────────────────────────
    internal static class BinaryBE
    {
        public static uint ReadUInt32(byte[] buf, int off)
            => ((uint)buf[off] << 24) | ((uint)buf[off+1] << 16)
             | ((uint)buf[off+2] << 8) | buf[off+3];

        public static ushort ReadUInt16(byte[] buf, int off)
            => (ushort)(((ushort)buf[off] << 8) | buf[off+1]);

        public static void WriteUInt32(byte[] buf, int off, uint v)
        {
            buf[off]   = (byte)(v >> 24); buf[off+1] = (byte)(v >> 16);
            buf[off+2] = (byte)(v >>  8); buf[off+3] = (byte) v;
        }

        public static void WriteUInt16(byte[] buf, int off, ushort v)
        { buf[off] = (byte)(v >> 8); buf[off+1] = (byte)v; }
    }
}
