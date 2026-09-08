using System;
using System.IO;

namespace MiniSoftware.Drawing.Codecs.Jpeg
{
    /// <summary>
    /// JPEG Huffman table built from a DHT segment.
    /// Uses a 512-entry (9-bit lookahead) fast-decode table for codes ≤ 9 bits;
    /// falls back to a linear search for longer codes.
    /// </summary>
    internal sealed class HuffmanTable
    {
        // Fast-path: 512 entries, indexed by top 9 bits of the bit-stream.
        // Entry format: bits[11:8] = code length (0 = slow path), bits[7:0] = symbol.
        private readonly int[] _fast = new int[512];

        // Slow-path data (codes > 9 bits)
        private readonly int[]  _maxCode  = new int[17];   // max code at each length
        private readonly int[]  _minCode  = new int[17];
        private readonly int[]  _valPtr   = new int[17];   // index into _huffVal
        private readonly byte[] _huffVal;                   // all symbol values in order

        /// <summary>Table class: 0 = DC, 1 = AC.</summary>
        public int Class { get; private set; }

        /// <summary>Table destination id (0–3).</summary>
        public int Id { get; private set; }

        // ── Factory ──────────────────────────────────────────────────────────────

        /// <summary>
        /// Parses one or more Huffman tables from a DHT segment payload.
        /// Returns all tables found.
        /// </summary>
        public static HuffmanTable[] ParseDht(byte[] data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            var tables = new System.Collections.Generic.List<HuffmanTable>(4);
            int pos = 0;
            while (pos < data.Length)
            {
                byte tcTh = data[pos++];
                int tc = (tcTh >> 4) & 0xF;   // table class (0=DC,1=AC)
                int th =  tcTh       & 0xF;   // table destination id

                // BITS[1..16] — count of codes of each length
                int[] bits = new int[17];
                int totalSymbols = 0;
                for (int i = 1; i <= 16; i++)
                {
                    bits[i] = data[pos++];
                    totalSymbols += bits[i];
                }

                // HUFFVAL — all symbol bytes in code-length order
                byte[] huffVal = new byte[totalSymbols];
                for (int i = 0; i < totalSymbols; i++)
                    huffVal[i] = data[pos++];

                tables.Add(Build(tc, th, bits, huffVal));
            }
            return tables.ToArray();
        }

        /// <summary>Decode the next symbol from <paramref name="reader"/>.</summary>
        public int Decode(BitReader reader)
        {
            // Fast path: peek 9 bits
            int top9 = reader.PeekBits(9);
            int entry = _fast[top9];
            int len   = entry >> 8;
            if (len > 0 && len <= 9)
            {
                reader.ConsumeBits(len);
                return entry & 0xFF;
            }

            // Slow path: iterate lengths 1..16
            int code = 0;
            for (int l = 1; l <= 16; l++)
            {
                code = (code << 1) | reader.ReadBit();
                if (code <= _maxCode[l] && _maxCode[l] != -1)
                {
                    int idx = _valPtr[l] + (code - _minCode[l]);
                    return _huffVal[idx];
                }
            }
            throw new InvalidDataException("Invalid Huffman code in JPEG stream.");
        }

        // ── Construction ─────────────────────────────────────────────────────────

        private static HuffmanTable Build(int tc, int th, int[] bits, byte[] huffVal)
        {
            var t = new HuffmanTable(huffVal) { Class = tc, Id = th };

            // Build canonical codes and min/max/valPtr tables
            int code   = 0;
            int valIdx = 0;
            for (int l = 1; l <= 16; l++)
            {
                if (bits[l] > 0)
                {
                    t._minCode[l] = code;
                    t._valPtr[l]  = valIdx;
                    code   += bits[l];
                    valIdx += bits[l];
                    t._maxCode[l] = code - 1;
                }
                else
                {
                    t._maxCode[l] = -1; // no codes of this length
                }
                code <<= 1;
            }

            // Fill fast 9-bit lookup table
            code   = 0;
            valIdx = 0;
            for (int l = 1; l <= 9; l++)
            {
                for (int i = 0; i < bits[l]; i++)
                {
                    // Every 9-bit prefix that starts with this code
                    int baseIdx = code << (9 - l);
                    int count   = 1 << (9 - l);
                    int entry   = (l << 8) | huffVal[valIdx];
                    for (int k = 0; k < count; k++)
                        t._fast[baseIdx + k] = entry;
                    code++;
                    valIdx++;
                }
                code <<= 1;
            }
            return t;
        }

        private HuffmanTable(byte[] huffVal) => _huffVal = huffVal;
    }
}
