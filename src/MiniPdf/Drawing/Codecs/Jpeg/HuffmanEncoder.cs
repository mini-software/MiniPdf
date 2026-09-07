using System;
using System.IO;

namespace MiniSoftware.Drawing.Codecs.Jpeg
{
    internal sealed class HuffmanEncoder
    {
        // Counts for code lengths 1..16.
        internal static readonly byte[] DcLuminanceCounts = { 0,1,5,1,1,1,1,1,1,0,0,0,0,0,0,0 };
        internal static readonly byte[] DcChrominanceCounts = { 0,3,1,1,1,1,1,1,1,1,1,0,0,0,0,0 };
        internal static readonly byte[] AcLuminanceCounts = { 0,2,1,3,3,2,4,3,5,5,4,4,0,0,1,125 };
        internal static readonly byte[] AcChrominanceCounts = { 0,2,1,2,4,4,3,4,7,5,4,4,0,1,2,119 };

        internal static readonly byte[] DcLuminanceValues = { 0,1,2,3,4,5,6,7,8,9,10,11 };
        internal static readonly byte[] DcChrominanceValues = { 0,1,2,3,4,5,6,7,8,9,10,11 };

        internal static readonly byte[] AcLuminanceValues =
        {
            0x01,0x02,0x03,0x00,0x04,0x11,0x05,0x12,0x21,0x31,0x41,0x06,0x13,0x51,0x61,0x07,
            0x22,0x71,0x14,0x32,0x81,0x91,0xA1,0x08,0x23,0x42,0xB1,0xC1,0x15,0x52,0xD1,0xF0,
            0x24,0x33,0x62,0x72,0x82,0x09,0x0A,0x16,0x17,0x18,0x19,0x1A,0x25,0x26,0x27,0x28,
            0x29,0x2A,0x34,0x35,0x36,0x37,0x38,0x39,0x3A,0x43,0x44,0x45,0x46,0x47,0x48,0x49,
            0x4A,0x53,0x54,0x55,0x56,0x57,0x58,0x59,0x5A,0x63,0x64,0x65,0x66,0x67,0x68,0x69,
            0x6A,0x73,0x74,0x75,0x76,0x77,0x78,0x79,0x7A,0x83,0x84,0x85,0x86,0x87,0x88,0x89,
            0x8A,0x92,0x93,0x94,0x95,0x96,0x97,0x98,0x99,0x9A,0xA2,0xA3,0xA4,0xA5,0xA6,0xA7,
            0xA8,0xA9,0xAA,0xB2,0xB3,0xB4,0xB5,0xB6,0xB7,0xB8,0xB9,0xBA,0xC2,0xC3,0xC4,0xC5,
            0xC6,0xC7,0xC8,0xC9,0xCA,0xD2,0xD3,0xD4,0xD5,0xD6,0xD7,0xD8,0xD9,0xDA,0xE1,0xE2,
            0xE3,0xE4,0xE5,0xE6,0xE7,0xE8,0xE9,0xEA,0xF1,0xF2,0xF3,0xF4,0xF5,0xF6,0xF7,0xF8,
            0xF9,0xFA,
        };

        internal static readonly byte[] AcChrominanceValues =
        {
            0x00,0x01,0x02,0x03,0x11,0x04,0x05,0x21,0x31,0x06,0x12,0x41,0x51,0x07,0x61,0x71,
            0x13,0x22,0x32,0x81,0x08,0x14,0x42,0x91,0xA1,0xB1,0xC1,0x09,0x23,0x33,0x52,0xF0,
            0x15,0x62,0x72,0xD1,0x0A,0x16,0x24,0x34,0xE1,0x25,0xF1,0x17,0x18,0x19,0x1A,0x26,
            0x27,0x28,0x29,0x2A,0x35,0x36,0x37,0x38,0x39,0x3A,0x43,0x44,0x45,0x46,0x47,0x48,
            0x49,0x4A,0x53,0x54,0x55,0x56,0x57,0x58,0x59,0x5A,0x63,0x64,0x65,0x66,0x67,0x68,
            0x69,0x6A,0x73,0x74,0x75,0x76,0x77,0x78,0x79,0x7A,0x82,0x83,0x84,0x85,0x86,0x87,
            0x88,0x89,0x8A,0x92,0x93,0x94,0x95,0x96,0x97,0x98,0x99,0x9A,0xA2,0xA3,0xA4,0xA5,
            0xA6,0xA7,0xA8,0xA9,0xAA,0xB2,0xB3,0xB4,0xB5,0xB6,0xB7,0xB8,0xB9,0xBA,0xC2,0xC3,
            0xC4,0xC5,0xC6,0xC7,0xC8,0xC9,0xCA,0xD2,0xD3,0xD4,0xD5,0xD6,0xD7,0xD8,0xD9,0xDA,
            0xE2,0xE3,0xE4,0xE5,0xE6,0xE7,0xE8,0xE9,0xEA,0xF2,0xF3,0xF4,0xF5,0xF6,0xF7,0xF8,
            0xF9,0xFA,
        };

        private readonly BitWriter _writer;
        private readonly HuffmanCode[] _dcLuma;
        private readonly HuffmanCode[] _dcChroma;
        private readonly HuffmanCode[] _acLuma;
        private readonly HuffmanCode[] _acChroma;

        public HuffmanEncoder(Stream stream)
        {
            _writer = new BitWriter(stream ?? throw new ArgumentNullException(nameof(stream)));
            _dcLuma = BuildCodes(DcLuminanceCounts, DcLuminanceValues);
            _dcChroma = BuildCodes(DcChrominanceCounts, DcChrominanceValues);
            _acLuma = BuildCodes(AcLuminanceCounts, AcLuminanceValues);
            _acChroma = BuildCodes(AcChrominanceCounts, AcChrominanceValues);
        }

        public void EncodeBlock(int[] blockNatural, ref int previousDc, bool chroma)
        {
            if (blockNatural == null) throw new ArgumentNullException(nameof(blockNatural));
            if (blockNatural.Length < 64) throw new ArgumentException("Block must contain 64 coefficients.", nameof(blockNatural));

            HuffmanCode[] dc = chroma ? _dcChroma : _dcLuma;
            HuffmanCode[] ac = chroma ? _acChroma : _acLuma;

            int dcDiff = blockNatural[0] - previousDc;
            previousDc = blockNatural[0];

            int dcBits = MagnitudeCategory(dcDiff);
            WriteCode(dc, dcBits);
            if (dcBits > 0)
                _writer.WriteBits(ToAmplitudeBits(dcDiff, dcBits), dcBits);

            int zeroRun = 0;
            for (int i = 1; i < 64; i++)
            {
                int idx = QuantizationTable.ZigZagOrder[i];
                int value = blockNatural[idx];
                if (value == 0)
                {
                    zeroRun++;
                    continue;
                }

                while (zeroRun >= 16)
                {
                    WriteCode(ac, 0xF0);
                    zeroRun -= 16;
                }

                int acBits = MagnitudeCategory(value);
                int symbol = (zeroRun << 4) | acBits;
                WriteCode(ac, symbol);
                _writer.WriteBits(ToAmplitudeBits(value, acBits), acBits);
                zeroRun = 0;
            }

            if (zeroRun > 0)
                WriteCode(ac, 0x00); // EOB
        }

        public void Flush() => _writer.Flush();

        private void WriteCode(HuffmanCode[] table, int symbol)
        {
            HuffmanCode code = table[symbol & 0xFF];
            if (code.Size == 0)
                throw new InvalidDataException($"Missing Huffman code for symbol 0x{symbol:X2}.");

            _writer.WriteBits(code.Code, code.Size);
        }

        private static int MagnitudeCategory(int value)
        {
            int abs = Math.Abs(value);
            int bits = 0;
            while (abs != 0)
            {
                abs >>= 1;
                bits++;
            }

            return bits;
        }

        private static int ToAmplitudeBits(int value, int bits)
        {
            if (value >= 0)
                return value;

            return value + ((1 << bits) - 1);
        }

        private static HuffmanCode[] BuildCodes(byte[] counts, byte[] values)
        {
            var table = new HuffmanCode[256];
            int code = 0;
            int index = 0;

            for (int bitLength = 1; bitLength <= 16; bitLength++)
            {
                int count = counts[bitLength - 1];
                for (int i = 0; i < count; i++)
                {
                    int symbol = values[index++];
                    table[symbol] = new HuffmanCode((ushort)code, (byte)bitLength);
                    code++;
                }

                code <<= 1;
            }

            return table;
        }

        private readonly struct HuffmanCode
        {
            public HuffmanCode(ushort code, byte size)
            {
                Code = code;
                Size = size;
            }

            public ushort Code { get; }
            public byte Size { get; }
        }

        private sealed class BitWriter
        {
            private readonly Stream _stream;
            private uint _bitBuffer;
            private int _bitCount;

            public BitWriter(Stream stream)
            {
                _stream = stream;
            }

            public void WriteBits(int bits, int count)
            {
                if (count <= 0)
                    return;

                uint valueMask = count == 32 ? uint.MaxValue : (uint)((1u << count) - 1u);
                _bitBuffer = (_bitBuffer << count) | ((uint)bits & valueMask);
                _bitCount += count;

                while (_bitCount >= 8)
                {
                    int shift = _bitCount - 8;
                    byte b = (byte)((_bitBuffer >> shift) & 0xFF);
                    WriteByteStuffed(b);
                    _bitCount -= 8;

                    if (_bitCount == 0)
                    {
                        _bitBuffer = 0;
                    }
                    else
                    {
                        _bitBuffer &= (uint)((1u << _bitCount) - 1u);
                    }
                }
            }

            public void Flush()
            {
                int pad = (8 - (_bitCount & 7)) & 7;
                if (pad > 0)
                    WriteBits((1 << pad) - 1, pad);
            }

            private void WriteByteStuffed(byte value)
            {
                _stream.WriteByte(value);
                if (value == 0xFF)
                    _stream.WriteByte(0x00);
            }
        }
    }
}
