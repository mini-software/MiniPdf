using System;
using System.IO;
using System.IO.Compression;

namespace MiniPdf.Drawing.Font.IO
{
    /// <summary>
    /// Decompresses a WOFF 1.0 container into a raw sfnt byte array that can be
    /// passed to <see cref="Ttf.TtfFontReader"/> or <see cref="Cff.CffFontReader"/>.
    /// </summary>
    /// <remarks>
    /// Each WOFF table is individually compressed using raw DEFLATE (RFC 1951).
    /// Many tools (e.g., fonttools) instead produce zlib-wrapped DEFLATE (RFC 1950).
    /// Both are detected automatically by checking for the two-byte zlib header.
    ///
    /// Specification: https://www.w3.org/TR/WOFF/
    /// </remarks>
    internal static class WoffReader
    {
        /// <summary>WOFF 1.0 magic bytes 'w','O','F','F' = 0x774F4646.</summary>
        internal const uint Signature = 0x774F4646u;

        // ── Public surface ────────────────────────────────────────────────────

        /// <summary>
        /// Reads a WOFF 1.0 stream and returns the reconstructed sfnt bytes.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// The data is not a valid WOFF 1.0 file or a table cannot be decompressed.
        /// </exception>
        internal static byte[] Decompress(Stream stream)
        {
            if (stream is null) throw new ArgumentNullException(nameof(stream));

            using var ms = new MemoryStream();
            stream.CopyTo(ms);
            return Decompress(ms.ToArray());
        }

        /// <summary>
        /// Parses a WOFF 1.0 byte array and returns the reconstructed sfnt bytes.
        /// </summary>
        internal static byte[] Decompress(byte[] data)
        {
            if (data is null) throw new ArgumentNullException(nameof(data));
            if (data.Length < 44)
                throw new InvalidOperationException(
                    "Data is too short to be a WOFF file (minimum 44 bytes for header).");

            int pos = 0;

            // ── WOFF header (44 bytes total) ──────────────────────────────────
            uint sig = ReadU32(data, ref pos);
            if (sig != Signature)
                throw new InvalidOperationException(
                    $"Not a WOFF 1.0 file (signature 0x{sig:X8}, expected 0x774F4646).");

            uint flavor        = ReadU32(data, ref pos);
            /* length       */   ReadU32(data, ref pos);   // total WOFF file size
            ushort numTables   = ReadU16(data, ref pos);
            /* reserved     */   ReadU16(data, ref pos);
            /* totalSfntSize*/   ReadU32(data, ref pos);   // hint only; we rebuild
            /* majorVersion */   ReadU16(data, ref pos);
            /* minorVersion */   ReadU16(data, ref pos);
            /* metaOffset   */   ReadU32(data, ref pos);
            /* metaLength   */   ReadU32(data, ref pos);
            /* metaOrigLen  */   ReadU32(data, ref pos);
            /* privOffset   */   ReadU32(data, ref pos);
            /* privLength   */   ReadU32(data, ref pos);
            // pos == 44 here

            // ── Table directory entries (20 bytes each) ───────────────────────
            var entries   = new WoffEntry[numTables];
            var tableData = new byte[numTables][];

            for (int i = 0; i < numTables; i++)
            {
                string tag    = ReadTag(data, ref pos);
                uint woffOff  = ReadU32(data, ref pos);
                uint compLen  = ReadU32(data, ref pos);
                uint origLen  = ReadU32(data, ref pos);
                uint checksum = ReadU32(data, ref pos);
                entries[i] = new WoffEntry(tag, woffOff, compLen, origLen, checksum);
            }

            // ── Decompress each table ─────────────────────────────────────────
            for (int i = 0; i < numTables; i++)
            {
                var e = entries[i];

                if ((long)e.WoffOffset + e.CompLen > data.Length)
                    throw new InvalidOperationException(
                        $"WOFF table '{e.Tag}' data extends beyond file bounds " +
                        $"(offset={e.WoffOffset}, compLen={e.CompLen}, fileLen={data.Length}).");

                tableData[i] = e.CompLen < e.OrigLen
                    ? DecompressTable(data, (int)e.WoffOffset, (int)e.CompLen, (int)e.OrigLen, e.Tag)
                    : Slice(data, (int)e.WoffOffset, (int)e.CompLen);
            }

            // ── Reconstruct sfnt binary ───────────────────────────────────────
            return BuildSfnt(flavor, entries, tableData);
        }

        // ── Private helpers ───────────────────────────────────────────────────

        /// <summary>
        /// Decompresses one WOFF table block.
        /// Handles both raw DEFLATE and zlib-wrapped DEFLATE automatically.
        /// </summary>
        private static byte[] DecompressTable(
            byte[] data, int offset, int compLen, int origLen, string tag)
        {
            // Detect zlib header: first byte CM nibble == 8 (DEFLATE) and
            // the two-byte big-endian value is divisible by 31 (FCHECK).
            bool hasZlibHeader = compLen >= 2
                && (data[offset] & 0x0F) == 8
                && ((data[offset] << 8) | data[offset + 1]) % 31 == 0;

            int start  = hasZlibHeader ? offset + 2 : offset;
            int length = compLen - (hasZlibHeader ? 2 : 0);

            using var ms      = new MemoryStream(data, start, length);
            using var deflate = new DeflateStream(ms, CompressionMode.Decompress);

            var result    = new byte[origLen];
            int totalRead = 0;
            while (totalRead < origLen)
            {
                int n = deflate.Read(result, totalRead, origLen - totalRead);
                if (n == 0) break;
                totalRead += n;
            }

            if (totalRead != origLen)
                throw new InvalidOperationException(
                    $"WOFF table '{tag}': expected {origLen} decompressed bytes, got {totalRead}.");

            return result;
        }

        /// <summary>
        /// Assembles the decompressed tables into a standard sfnt binary.
        /// </summary>
        private static byte[] BuildSfnt(uint flavor, WoffEntry[] entries, byte[][] tableData)
        {
            int n = entries.Length;

            // sfnt requires tables sorted lexicographically by tag.
            Array.Sort(entries, tableData,
                System.Collections.Generic.Comparer<WoffEntry>.Create(
                    (a, b) => string.Compare(a.Tag, b.Tag, StringComparison.Ordinal)));

            // Calculate the offset of each table in the output sfnt.
            // Layout: 12-byte offset table  +  n×16-byte directory  +  padded table data.
            int  directoryEnd = 12 + n * 16;
            var  sfntOffsets  = new uint[n];
            uint cursor       = (uint)directoryEnd;

            for (int i = 0; i < n; i++)
            {
                sfntOffsets[i] = cursor;
                cursor += (uint)tableData[i].Length;
                if (cursor % 4 != 0) cursor += 4 - cursor % 4;   // 4-byte align
            }

            var sfnt = new byte[cursor];
            int w    = 0;

            // ── sfnt offset table (12 bytes) ──────────────────────────────────
            WriteU32(sfnt, ref w, flavor);
            WriteU16(sfnt, ref w, (ushort)n);

            // Binary-search parameters:
            int p2 = 1;
            while (p2 * 2 <= n) p2 *= 2;

            int log2 = 0, t = p2;
            while (t > 1) { log2++; t >>= 1; }

            WriteU16(sfnt, ref w, (ushort)(p2 * 16));          // searchRange
            WriteU16(sfnt, ref w, (ushort)log2);               // entrySelector
            WriteU16(sfnt, ref w, (ushort)(n * 16 - p2 * 16)); // rangeShift

            // ── Table directory (16 bytes per entry) ──────────────────────────
            for (int i = 0; i < n; i++)
            {
                WriteTag(sfnt, ref w, entries[i].Tag);
                WriteU32(sfnt, ref w, entries[i].Checksum);
                WriteU32(sfnt, ref w, sfntOffsets[i]);
                WriteU32(sfnt, ref w, entries[i].OrigLen);
            }

            // ── Table data ────────────────────────────────────────────────────
            for (int i = 0; i < n; i++)
                Array.Copy(tableData[i], 0, sfnt, (int)sfntOffsets[i], tableData[i].Length);
            // Padding bytes between tables are zero from array initialisation.

            return sfnt;
        }

        // ── Low-level byte I/O ────────────────────────────────────────────────

        private static uint ReadU32(byte[] d, ref int p)
        {
            uint v = ((uint)d[p] << 24) | ((uint)d[p + 1] << 16)
                   | ((uint)d[p + 2] <<  8) |         d[p + 3];
            p += 4;
            return v;
        }

        private static ushort ReadU16(byte[] d, ref int p)
        {
            ushort v = (ushort)((d[p] << 8) | d[p + 1]);
            p += 2;
            return v;
        }

        private static string ReadTag(byte[] d, ref int p)
        {
            var s = new string(new[] { (char)d[p], (char)d[p + 1], (char)d[p + 2], (char)d[p + 3] });
            p += 4;
            return s;
        }

        private static byte[] Slice(byte[] d, int offset, int length)
        {
            var r = new byte[length];
            Array.Copy(d, offset, r, 0, length);
            return r;
        }

        private static void WriteU32(byte[] b, ref int p, uint v)
        {
            b[p++] = (byte)(v >> 24); b[p++] = (byte)(v >> 16);
            b[p++] = (byte)(v >>  8); b[p++] = (byte)v;
        }

        private static void WriteU16(byte[] b, ref int p, ushort v)
        {
            b[p++] = (byte)(v >> 8); b[p++] = (byte)v;
        }

        private static void WriteTag(byte[] b, ref int p, string tag)
        {
            b[p++] = (byte)tag[0]; b[p++] = (byte)tag[1];
            b[p++] = (byte)tag[2]; b[p++] = (byte)tag[3];
        }

        // ── WoffEntry value type ──────────────────────────────────────────────

        private struct WoffEntry
        {
            public string Tag;
            public uint   WoffOffset;
            public uint   CompLen;
            public uint   OrigLen;
            public uint   Checksum;

            public WoffEntry(string tag, uint woffOffset,
                             uint compLen, uint origLen, uint checksum)
            {
                Tag        = tag;
                WoffOffset = woffOffset;
                CompLen    = compLen;
                OrigLen    = origLen;
                Checksum   = checksum;
            }
        }
    }
}
