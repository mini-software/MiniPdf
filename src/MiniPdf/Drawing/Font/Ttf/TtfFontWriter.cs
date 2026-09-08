using System;
using System.Collections.Generic;
using System.IO;
using MiniSoftware.Drawing.Font.IO;

namespace MiniSoftware.Drawing.Font.Ttf
{
    /// <summary>
    /// Serialises a <see cref="TtfFont"/> back to a binary sfnt stream.
    /// Uses the raw table bytes already stored in each parsed table, so no
    /// per-table re-serialisation is required.
    /// </summary>
    public static class TtfFontWriter
    {
        // ─────────────────────────────────────────────────────────────────────
        // Public entry point
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Serialises <paramref name="font"/> and writes the resulting sfnt bytes to
        /// <paramref name="stream"/>.
        /// </summary>
        /// <exception cref="ArgumentNullException">Either argument is null.</exception>
        public static void Write(TtfFont font, Stream stream)
        {
            if (font   == null) throw new ArgumentNullException(nameof(font));
            if (stream == null) throw new ArgumentNullException(nameof(stream));

            byte[] data = WriteToBytes(font);
            stream.Write(data, 0, data.Length);
        }

        /// <summary>
        /// Serialises <paramref name="font"/> and returns the resulting sfnt bytes.
        /// </summary>
        public static byte[] WriteToBytes(TtfFont font)
        {
            if (font == null) throw new ArgumentNullException(nameof(font));

            // ── 1. Collect (tag, rawBytes) pairs in canonical sfnt order ─────
            var tables = CollectTables(font.TtfTables);

            // ── 2. Determine sfnt version ────────────────────────────────────
            // TrueType-outline fonts have a glyf table; CFF-flavoured OTF don't.
            // For an OTF/CFF font opened via TtfFont (edge-case), default to OTTO.
            uint sfVersion = (font.TtfTables.GlyfTable != null)
                ? OpenTypeOffsetTable.SfVersionTrueType
                : OpenTypeOffsetTable.SfVersionCff;

            return BuildSfnt(sfVersion, tables);
        }

        // ─────────────────────────────────────────────────────────────────────
        // Internal helpers (also called by CffFontWriter)
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Builds a complete sfnt binary from the supplied (tag, bytes) pairs.
        /// Patches <c>head.checkSumAdjustment</c> per spec.
        /// </summary>
        internal static byte[] BuildSfnt(uint sfVersion,
                                         IReadOnlyList<(string Tag, byte[] Data)> tables)
        {
            int n = tables.Count;

            // Derived sfnt offset-table fields (binary-search acceleration).
            int log2n        = Log2Floor(n);
            ushort searchRange   = (ushort)((1 << log2n) * 16);
            ushort entrySelector = (ushort)log2n;
            ushort rangeShift    = (ushort)(n * 16 - searchRange);

            // Compute the byte offsets of each table in the output.
            // Layout: 12-byte offset table + n*16 table records + table data (4-aligned).
            int dataStart = 12 + n * 16;
            var offsets = new int[n];
            var paddedLengths = new int[n];

            int currentOffset = dataStart;
            for (int i = 0; i < n; i++)
            {
                offsets[i] = currentOffset;
                int raw = tables[i].Data.Length;
                paddedLengths[i] = (raw + 3) & ~3;   // round up to multiple of 4
                currentOffset += paddedLengths[i];
            }

            int totalSize = currentOffset;
            byte[] buf = new byte[totalSize];

            // ── Write offset table (12 bytes) ─────────────────────────────────
            WriteUInt32BE(buf, 0,  sfVersion);
            WriteUInt16BE(buf, 4,  (ushort)n);
            WriteUInt16BE(buf, 6,  searchRange);
            WriteUInt16BE(buf, 8,  entrySelector);
            WriteUInt16BE(buf, 10, rangeShift);

            // ── Write table records (16 bytes each) ──────────────────────────
            // Tags must be sorted in ascending ASCII order per spec.
            // Our CollectTables already returns them sorted, but we still compute
            // checksums from the padded data at this step.
            int recBase = 12;
            for (int i = 0; i < n; i++)
            {
                var (tag, data) = tables[i];
                uint checksum = OpenTypeTableDirectory.ComputeChecksum(data);

                // Copy padded table data first so head patching can use final bytes.
                Array.Copy(data, 0, buf, offsets[i], data.Length);
                // Padding bytes are already zero (new byte[] is zero-initialised).

                WriteTagBytes(buf, recBase + i * 16, tag);
                WriteUInt32BE(buf, recBase + i * 16 + 4,  checksum);
                WriteUInt32BE(buf, recBase + i * 16 + 8,  (uint)offsets[i]);
                WriteUInt32BE(buf, recBase + i * 16 + 12, (uint)data.Length);
            }

            // ── Patch head.checkSumAdjustment ─────────────────────────────────
            // Per spec:
            //   1. Zero out bytes 8-11 of the head table.
            //   2. Sum all uint32 words in the entire file (big-endian, padded).
            //   3. checkSumAdjustment = 0xB1B0AFBA - totalSum.
            int headIdx = FindTableIndex(tables, "head");
            if (headIdx >= 0)
            {
                // Zero the checkSumAdjustment field in the buffer.
                int csa = offsets[headIdx] + 8;
                if (csa + 4 <= buf.Length)
                {
                    buf[csa]     = 0;
                    buf[csa + 1] = 0;
                    buf[csa + 2] = 0;
                    buf[csa + 3] = 0;
                }

                // Recompute head checksum with zeroed CSA and update table record.
                int headDataLen = tables[headIdx].Data.Length;
                byte[] headSlice = new byte[paddedLengths[headIdx]];
                Array.Copy(buf, offsets[headIdx], headSlice, 0, headSlice.Length);
                uint newHeadChecksum = OpenTypeTableDirectory.ComputeChecksum(headSlice);
                WriteUInt32BE(buf, recBase + headIdx * 16 + 4, newHeadChecksum);

                // Compute sum of the whole file.
                uint fileSum = SumFile(buf);

                // Write checkSumAdjustment.
                uint adj = unchecked(0xB1B0AFBAu - fileSum);
                WriteUInt32BE(buf, csa, adj);
            }

            return buf;
        }

        // ─────────────────────────────────────────────────────────────────────
        // Table collection
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Enumerates all non-null tables present in <paramref name="t"/> and returns
        /// them sorted by tag (ascending ASCII, as required by the sfnt spec).
        /// </summary>
        internal static List<(string Tag, byte[] Data)> CollectTables(TtfTables t)
        {
            var result = new List<(string Tag, byte[] Data)>(24);

            Add(result, "head",  t.Head.RawBytes);
            Add(result, "hhea",  t.Hhea.RawBytes);
            Add(result, "maxp",  t.Maxp.RawBytes);
            Add(result, "name",  t.Name.RawBytes);
            Add(result, "hmtx",  t.Hmtx.RawBytes);

            if (t.Os2Table  != null) Add(result, "OS/2", t.Os2Table.RawBytes);
            if (t.Post      != null) Add(result, "post", t.Post.RawBytes);
            if (t.Loca      != null) Add(result, "loca", t.Loca.RawBytes);
            if (t.GlyfTable != null) Add(result, "glyf", t.GlyfTable.RawBytes);
            if (t.CMapTable != null) Add(result, "cmap", t.CMapTable.RawBytes);

            // Batch 13 — advanced layout
            if (t.KernTable != null) Add(result, "kern", t.KernTable.RawBytes);
            if (t.GdefTable != null) Add(result, "GDEF", t.GdefTable.RawBytes);
            if (t.GsubTable != null) Add(result, "GSUB", t.GsubTable.RawBytes);
            if (t.GposTable != null) Add(result, "GPOS", t.GposTable.RawBytes);

            // Batch 14 — variable fonts
            if (t.FvarTable != null) Add(result, "fvar", t.FvarTable.RawBytes);
            if (t.GvarTable != null) Add(result, "gvar", t.GvarTable.RawBytes);
            if (t.AvarTable != null) Add(result, "avar", t.AvarTable.RawBytes);

            // Batch 15 — colour bitmaps (CBLC must precede CBDT)
            if (t.CbdtTable != null)
            {
                Add(result, "CBLC", t.CbdtTable.CblcRawBytes);
                Add(result, "CBDT", t.CbdtTable.RawBytes);
            }
            if (t.SvgTable  != null) Add(result, "SVG ", t.SvgTable.RawBytes);

            // Sort ascending by tag (ASCII order) — sfnt spec requirement.
            result.Sort((a, b) => string.Compare(a.Tag, b.Tag, StringComparison.Ordinal));

            return result;
        }

        // ─────────────────────────────────────────────────────────────────────
        // Low-level binary helpers
        // ─────────────────────────────────────────────────────────────────────

        private static void Add(List<(string Tag, byte[] Data)> list, string tag, byte[] data)
        {
            if (data != null && data.Length >= 0)
                list.Add((tag, data));
        }

        private static int FindTableIndex(IReadOnlyList<(string Tag, byte[] Data)> tables,
                                          string tag)
        {
            for (int i = 0; i < tables.Count; i++)
                if (tables[i].Tag == tag) return i;
            return -1;
        }

        private static uint SumFile(byte[] buf)
        {
            uint sum = 0;
            int wordCount = (buf.Length + 3) / 4;
            for (int i = 0; i < wordCount; i++)
            {
                int pos = i * 4;
                uint w = 0;
                for (int b = 0; b < 4; b++)
                    w = (w << 8) | (uint)(pos + b < buf.Length ? buf[pos + b] : 0);
                sum = unchecked(sum + w);
            }
            return sum;
        }

        internal static void WriteUInt32BE(byte[] buf, int offset, uint value)
        {
            buf[offset    ] = (byte)(value >> 24);
            buf[offset + 1] = (byte)(value >> 16);
            buf[offset + 2] = (byte)(value >>  8);
            buf[offset + 3] = (byte) value;
        }

        internal static void WriteUInt16BE(byte[] buf, int offset, ushort value)
        {
            buf[offset    ] = (byte)(value >> 8);
            buf[offset + 1] = (byte) value;
        }

        private static void WriteTagBytes(byte[] buf, int offset, string tag)
        {
            // Tags are exactly 4 ASCII characters; pad with spaces if shorter.
            for (int i = 0; i < 4; i++)
                buf[offset + i] = (byte)(i < tag.Length ? tag[i] : ' ');
        }

        private static int Log2Floor(int n)
        {
            if (n <= 0) return 0;
            int r = 0;
            while (n > 1) { n >>= 1; r++; }
            return r;
        }
    }
}
