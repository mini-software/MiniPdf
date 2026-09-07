using System;
using System.IO;

namespace MiniPdf.Drawing.Codecs.Jpeg
{
    internal sealed class EntropyDecoder
    {
        private readonly FrameHeader _frame;
        private readonly ScanHeader _scan;
        private readonly QuantizationTable?[] _quantTables;
        private readonly HuffmanTable?[] _dcTables;
        private readonly HuffmanTable?[] _acTables;
        private readonly int _restartInterval;

        public EntropyDecoder(
            FrameHeader frame,
            ScanHeader scan,
            QuantizationTable?[] quantTables,
            HuffmanTable?[] dcTables,
            HuffmanTable?[] acTables,
            int restartInterval)
        {
            _frame = frame ?? throw new ArgumentNullException(nameof(frame));
            _scan = scan ?? throw new ArgumentNullException(nameof(scan));
            _quantTables = quantTables ?? throw new ArgumentNullException(nameof(quantTables));
            _dcTables = dcTables ?? throw new ArgumentNullException(nameof(dcTables));
            _acTables = acTables ?? throw new ArgumentNullException(nameof(acTables));
            _restartInterval = restartInterval;
        }

        public EntropyDecodeResult Decode(BitReader reader)
        {
            if (reader == null) throw new ArgumentNullException(nameof(reader));

            int componentCount = _frame.Components.Length;
            if (componentCount == 0)
                throw new InvalidDataException("Frame has no components.");

            int maxH = 1;
            int maxV = 1;
            for (int i = 0; i < componentCount; i++)
            {
                maxH = Math.Max(maxH, _frame.Components[i].HSampling);
                maxV = Math.Max(maxV, _frame.Components[i].VSampling);
            }

            int mcuWidth = 8 * maxH;
            int mcuHeight = 8 * maxV;
            int mcuCountX = (_frame.Width + mcuWidth - 1) / mcuWidth;
            int mcuCountY = (_frame.Height + mcuHeight - 1) / mcuHeight;

            var planes = new byte[componentCount][];
            var planeWidths = new int[componentCount];
            var planeHeights = new int[componentCount];

            for (int i = 0; i < componentCount; i++)
            {
                int h = _frame.Components[i].HSampling;
                int v = _frame.Components[i].VSampling;
                int width = mcuCountX * h * 8;
                int height = mcuCountY * v * 8;
                planeWidths[i] = width;
                planeHeights[i] = height;
                planes[i] = new byte[width * height];
            }

            var scanToFrame = new int[_scan.Components.Length];
            for (int i = 0; i < _scan.Components.Length; i++)
            {
                byte id = _scan.Components[i].ComponentId;
                int found = -1;
                for (int j = 0; j < componentCount; j++)
                {
                    if (_frame.Components[j].Id == id)
                    {
                        found = j;
                        break;
                    }
                }

                if (found < 0)
                    throw new InvalidDataException($"SOS references unknown component id {id}.");
                scanToFrame[i] = found;
            }

            var dcPredictors = new int[componentCount];
            var coeffs = new int[64];
            var blockPixels = new byte[64];
            int decodedMcus = 0;

            for (int mcuY = 0; mcuY < mcuCountY; mcuY++)
            {
                for (int mcuX = 0; mcuX < mcuCountX; mcuX++)
                {
                    for (int scanIndex = 0; scanIndex < _scan.Components.Length; scanIndex++)
                    {
                        int frameIndex = scanToFrame[scanIndex];
                        var frameComponent = _frame.Components[frameIndex];
                        var scanComponent = _scan.Components[scanIndex];

                        var quant = _quantTables[frameComponent.QuantizationTableId]
                            ?? throw new InvalidDataException($"Missing quantization table {frameComponent.QuantizationTableId}.");
                        var dcTable = _dcTables[scanComponent.DcTableId]
                            ?? throw new InvalidDataException($"Missing DC Huffman table {scanComponent.DcTableId}.");
                        var acTable = _acTables[scanComponent.AcTableId]
                            ?? throw new InvalidDataException($"Missing AC Huffman table {scanComponent.AcTableId}.");

                        int hSamples = frameComponent.HSampling;
                        int vSamples = frameComponent.VSampling;

                        for (int v = 0; v < vSamples; v++)
                        {
                            for (int h = 0; h < hSamples; h++)
                            {
                                DecodeBlock(reader, dcTable, acTable, ref dcPredictors[frameIndex], coeffs);
                                Dequantize(coeffs, quant.Coefficients);
                                IdctTransform.TransformBlock(coeffs, blockPixels);

                                int dstX = (mcuX * hSamples + h) * 8;
                                int dstY = (mcuY * vSamples + v) * 8;
                                WriteBlock(
                                    planes[frameIndex],
                                    planeWidths[frameIndex],
                                    planeHeights[frameIndex],
                                    blockPixels,
                                    dstX,
                                    dstY);
                            }
                        }
                    }

                    decodedMcus++;
                    if (_restartInterval > 0 && (decodedMcus % _restartInterval) == 0)
                    {
                        Array.Clear(dcPredictors, 0, dcPredictors.Length);
                        reader.Flush();
                    }
                }
            }

            return new EntropyDecodeResult(
                planes,
                planeWidths,
                planeHeights,
                maxH,
                maxV,
                mcuCountX,
                mcuCountY);
        }

        private static void DecodeBlock(
            BitReader reader,
            HuffmanTable dcTable,
            HuffmanTable acTable,
            ref int dcPredictor,
            int[] coeffs)
        {
            Array.Clear(coeffs, 0, 64);

            int dcLength = dcTable.Decode(reader);
            int dcDiff = ReceiveAndExtend(reader, dcLength);
            dcPredictor += dcDiff;
            coeffs[0] = dcPredictor;

            int k = 1;
            while (k < 64)
            {
                int rs = acTable.Decode(reader);
                int run = (rs >> 4) & 0xF;
                int size = rs & 0xF;

                if (size == 0)
                {
                    if (run == 15)
                    {
                        k += 16;
                        continue;
                    }

                    break;
                }

                k += run;
                if (k >= 64)
                    break;

                int value = ReceiveAndExtend(reader, size);
                coeffs[QuantizationTable.ZigZagOrder[k]] = value;
                k++;
            }
        }

        private static int ReceiveAndExtend(BitReader reader, int bitCount)
        {
            if (bitCount == 0)
                return 0;

            int value = reader.ReadBits(bitCount);
            int threshold = 1 << (bitCount - 1);
            if (value < threshold)
                value -= (1 << bitCount) - 1;
            return value;
        }

        private static void Dequantize(int[] coeffs, int[] quant)
        {
            for (int i = 0; i < 64; i++)
                coeffs[i] *= quant[i];
        }

        private static void WriteBlock(
            byte[] plane,
            int planeWidth,
            int planeHeight,
            byte[] block,
            int dstX,
            int dstY)
        {
            for (int y = 0; y < 8; y++)
            {
                int py = dstY + y;
                if ((uint)py >= (uint)planeHeight)
                    continue;

                int srcRow = y * 8;
                int dstRow = py * planeWidth;
                for (int x = 0; x < 8; x++)
                {
                    int px = dstX + x;
                    if ((uint)px >= (uint)planeWidth)
                        continue;

                    plane[dstRow + px] = block[srcRow + x];
                }
            }
        }
    }

    internal sealed class EntropyDecodeResult
    {
        public EntropyDecodeResult(
            byte[][] planes,
            int[] planeWidths,
            int[] planeHeights,
            int maxHorizontalSampling,
            int maxVerticalSampling,
            int mcuCountX,
            int mcuCountY)
        {
            Planes = planes;
            PlaneWidths = planeWidths;
            PlaneHeights = planeHeights;
            MaxHorizontalSampling = maxHorizontalSampling;
            MaxVerticalSampling = maxVerticalSampling;
            McuCountX = mcuCountX;
            McuCountY = mcuCountY;
        }

        public byte[][] Planes { get; }
        public int[] PlaneWidths { get; }
        public int[] PlaneHeights { get; }
        public int MaxHorizontalSampling { get; }
        public int MaxVerticalSampling { get; }
        public int McuCountX { get; }
        public int McuCountY { get; }
    }
}
