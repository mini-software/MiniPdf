using System;

namespace MiniSoftware.Drawing.Codecs.Jpeg
{
    internal sealed class Quantizer
    {
        private static readonly byte[] LuminanceBaseTable =
        {
            16,11,10,16,24,40,51,61,
            12,12,14,19,26,58,60,55,
            14,13,16,24,40,57,69,56,
            14,17,22,29,51,87,80,62,
            18,22,37,56,68,109,103,77,
            24,35,55,64,81,104,113,92,
            49,64,78,87,103,121,120,101,
            72,92,95,98,112,100,103,99,
        };

        private static readonly byte[] ChrominanceBaseTable =
        {
            17,18,24,47,99,99,99,99,
            18,21,26,66,99,99,99,99,
            24,26,56,99,99,99,99,99,
            47,66,99,99,99,99,99,99,
            99,99,99,99,99,99,99,99,
            99,99,99,99,99,99,99,99,
            99,99,99,99,99,99,99,99,
            99,99,99,99,99,99,99,99,
        };

        private Quantizer(ushort[] lumaNatural, ushort[] chromaNatural)
        {
            LuminanceNatural = lumaNatural;
            ChrominanceNatural = chromaNatural;
            LuminanceZigZag = ToZigZag(lumaNatural);
            ChrominanceZigZag = ToZigZag(chromaNatural);
        }

        public ushort[] LuminanceNatural { get; }
        public ushort[] ChrominanceNatural { get; }
        public byte[] LuminanceZigZag { get; }
        public byte[] ChrominanceZigZag { get; }

        public static Quantizer Create(int quality)
        {
            quality = Math.Max(1, Math.Min(100, quality));
            ushort[] luma = ScaleTable(LuminanceBaseTable, quality);
            ushort[] chroma = ScaleTable(ChrominanceBaseTable, quality);
            return new Quantizer(luma, chroma);
        }

        private static ushort[] ScaleTable(byte[] baseTable, int quality)
        {
            int scale = quality < 50 ? 5000 / quality : 200 - (2 * quality);
            var table = new ushort[64];

            for (int i = 0; i < 64; i++)
            {
                int value = (baseTable[i] * scale + 50) / 100;
                if (value < 1) value = 1;
                if (value > 255) value = 255;
                table[i] = (ushort)value;
            }

            return table;
        }

        private static byte[] ToZigZag(ushort[] naturalTable)
        {
            var zigzag = new byte[64];
            for (int i = 0; i < 64; i++)
            {
                int naturalIndex = QuantizationTable.ZigZagOrder[i];
                zigzag[i] = (byte)naturalTable[naturalIndex];
            }

            return zigzag;
        }
    }
}
