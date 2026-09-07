using System;
using System.IO;

namespace MiniPdf.Drawing.Codecs.Jpeg
{
    internal sealed class ProgressiveDecoder
    {
        private readonly FrameHeader _frame;
        private readonly QuantizationTable?[] _quantTables;
        private readonly HuffmanTable?[] _dcTables;
        private readonly HuffmanTable?[] _acTables;

        public ProgressiveDecoder(
            FrameHeader frame,
            QuantizationTable?[] quantTables,
            HuffmanTable?[] dcTables,
            HuffmanTable?[] acTables)
        {
            _frame = frame ?? throw new ArgumentNullException(nameof(frame));
            _quantTables = quantTables ?? throw new ArgumentNullException(nameof(quantTables));
            _dcTables = dcTables ?? throw new ArgumentNullException(nameof(dcTables));
            _acTables = acTables ?? throw new ArgumentNullException(nameof(acTables));
        }

        public void DecodeScan(
            ScanHeader scan,
            BitReader reader,
            CoefficientBuffer buffer,
            int restartInterval)
        {
            if (scan == null) throw new ArgumentNullException(nameof(scan));
            if (reader == null) throw new ArgumentNullException(nameof(reader));
            if (buffer == null) throw new ArgumentNullException(nameof(buffer));

            if (scan.Components.Length == 0)
                throw new InvalidDataException("SOS scan contains no components.");

            int[] scanToFrame = BuildScanToFrameMap(scan);
            int[] dcPredictors = new int[_frame.Components.Length];
            int eobRun = 0;
            int processedUnits = 0;

            if (scan.Components.Length == 1)
            {
                int scanComponentIndex = 0;
                int frameComponentIndex = scanToFrame[0];
                int blocksX = buffer.GetBlocksX(frameComponentIndex);
                int blocksY = buffer.GetBlocksY(frameComponentIndex);

                for (int by = 0; by < blocksY; by++)
                {
                    for (int bx = 0; bx < blocksX; bx++)
                    {
                        DecodeBlock(
                            scan,
                            scanComponentIndex,
                            frameComponentIndex,
                            bx,
                            by,
                            reader,
                            buffer,
                            dcPredictors,
                            ref eobRun);

                        processedUnits++;
                        if (restartInterval > 0 && (processedUnits % restartInterval) == 0)
                        {
                            Array.Clear(dcPredictors, 0, dcPredictors.Length);
                            eobRun = 0;
                            reader.Flush();
                        }
                    }
                }

                return;
            }

            for (int mcuY = 0; mcuY < buffer.McuCountY; mcuY++)
            {
                for (int mcuX = 0; mcuX < buffer.McuCountX; mcuX++)
                {
                    for (int scanComponentIndex = 0; scanComponentIndex < scan.Components.Length; scanComponentIndex++)
                    {
                        int frameComponentIndex = scanToFrame[scanComponentIndex];
                        var frameComponent = _frame.Components[frameComponentIndex];

                        for (int v = 0; v < frameComponent.VSampling; v++)
                        {
                            for (int h = 0; h < frameComponent.HSampling; h++)
                            {
                                int bx = mcuX * frameComponent.HSampling + h;
                                int by = mcuY * frameComponent.VSampling + v;

                                DecodeBlock(
                                    scan,
                                    scanComponentIndex,
                                    frameComponentIndex,
                                    bx,
                                    by,
                                    reader,
                                    buffer,
                                    dcPredictors,
                                    ref eobRun);
                            }
                        }
                    }

                    processedUnits++;
                    if (restartInterval > 0 && (processedUnits % restartInterval) == 0)
                    {
                        Array.Clear(dcPredictors, 0, dcPredictors.Length);
                        eobRun = 0;
                        reader.Flush();
                    }
                }
            }
        }

        public EntropyDecodeResult BuildImageData(CoefficientBuffer buffer)
        {
            if (buffer == null) throw new ArgumentNullException(nameof(buffer));

            int componentCount = _frame.Components.Length;
            var planes = new byte[componentCount][];
            var planeWidths = new int[componentCount];
            var planeHeights = new int[componentCount];

            var dequantized = new int[64];
            var pixels = new byte[64];

            for (int componentIndex = 0; componentIndex < componentCount; componentIndex++)
            {
                int blocksX = buffer.GetBlocksX(componentIndex);
                int blocksY = buffer.GetBlocksY(componentIndex);
                int width = blocksX * 8;
                int height = blocksY * 8;

                planeWidths[componentIndex] = width;
                planeHeights[componentIndex] = height;
                planes[componentIndex] = new byte[width * height];

                int qtId = _frame.Components[componentIndex].QuantizationTableId;
                var quant = _quantTables[qtId] ?? throw new InvalidDataException($"Missing quantization table {qtId}.");

                short[] source = buffer.GetComponentData(componentIndex);

                for (int by = 0; by < blocksY; by++)
                {
                    for (int bx = 0; bx < blocksX; bx++)
                    {
                        int offset = buffer.GetBlockOffset(componentIndex, bx, by);
                        for (int i = 0; i < 64; i++)
                            dequantized[i] = source[offset + i] * quant.Coefficients[i];

                        IdctTransform.TransformBlock(dequantized, pixels);
                        WriteBlock(planes[componentIndex], width, height, pixels, bx * 8, by * 8);
                    }
                }
            }

            return new EntropyDecodeResult(
                planes,
                planeWidths,
                planeHeights,
                buffer.MaxHorizontalSampling,
                buffer.MaxVerticalSampling,
                buffer.McuCountX,
                buffer.McuCountY);
        }

        private int[] BuildScanToFrameMap(ScanHeader scan)
        {
            int[] map = new int[scan.Components.Length];
            for (int i = 0; i < scan.Components.Length; i++)
            {
                byte id = scan.Components[i].ComponentId;
                int found = -1;
                for (int j = 0; j < _frame.Components.Length; j++)
                {
                    if (_frame.Components[j].Id == id)
                    {
                        found = j;
                        break;
                    }
                }

                if (found < 0)
                    throw new InvalidDataException($"SOS references unknown component id {id}.");

                map[i] = found;
            }

            return map;
        }

        private void DecodeBlock(
            ScanHeader scan,
            int scanComponentIndex,
            int frameComponentIndex,
            int blockX,
            int blockY,
            BitReader reader,
            CoefficientBuffer buffer,
            int[] dcPredictors,
            ref int eobRun)
        {
            int ss = scan.Ss;
            int se = scan.Se;
            int ah = scan.Ah;
            int al = scan.Al;

            var scanComponent = scan.Components[scanComponentIndex];
            HuffmanTable? dc = (uint)scanComponent.DcTableId < _dcTables.Length ? _dcTables[scanComponent.DcTableId] : null;
            HuffmanTable? ac = (uint)scanComponent.AcTableId < _acTables.Length ? _acTables[scanComponent.AcTableId] : null;

            if (ss == 0)
            {
                if (ah == 0)
                    DecodeDcInitial(dc, reader, buffer, frameComponentIndex, blockX, blockY, ref dcPredictors[frameComponentIndex], al);
                else
                    DecodeDcRefinement(reader, buffer, frameComponentIndex, blockX, blockY, al);
            }

            if (se > 0)
            {
                if (ah == 0)
                    DecodeAcInitial(ac, reader, buffer, frameComponentIndex, blockX, blockY, ss, se, al, ref eobRun);
                else
                    DecodeAcRefinement(ac, reader, buffer, frameComponentIndex, blockX, blockY, ss, se, al, ref eobRun);
            }
        }

        private static void DecodeDcInitial(
            HuffmanTable? dcTable,
            BitReader reader,
            CoefficientBuffer buffer,
            int componentIndex,
            int blockX,
            int blockY,
            ref int predictor,
            int al)
        {
            if (dcTable == null)
                throw new InvalidDataException("Missing DC Huffman table for progressive scan.");

            int size = dcTable.Decode(reader);
            int diff = ReceiveAndExtend(reader, size);
            predictor += diff;
            buffer.SetCoefficient(componentIndex, blockX, blockY, 0, (short)(predictor << al));
        }

        private static void DecodeDcRefinement(
            BitReader reader,
            CoefficientBuffer buffer,
            int componentIndex,
            int blockX,
            int blockY,
            int al)
        {
            int bit = reader.ReadBit();
            if (bit == 0)
                return;

            short current = buffer.GetCoefficient(componentIndex, blockX, blockY, 0);
            short delta = (short)(1 << al);

            if (current >= 0)
                buffer.AddCoefficient(componentIndex, blockX, blockY, 0, delta);
            else
                buffer.AddCoefficient(componentIndex, blockX, blockY, 0, (short)-delta);
        }

        private static void DecodeAcInitial(
            HuffmanTable? acTable,
            BitReader reader,
            CoefficientBuffer buffer,
            int componentIndex,
            int blockX,
            int blockY,
            int ss,
            int se,
            int al,
            ref int eobRun)
        {
            if (acTable == null)
                throw new InvalidDataException("Missing AC Huffman table for progressive scan.");

            if (eobRun > 0)
            {
                eobRun--;
                return;
            }

            int k = ss;
            while (k <= se)
            {
                int rs = acTable.Decode(reader);
                int run = (rs >> 4) & 0x0F;
                int size = rs & 0x0F;

                if (size == 0)
                {
                    if (run == 15)
                    {
                        k += 16;
                        continue;
                    }

                    eobRun = (1 << run) - 1;
                    if (run > 0)
                        eobRun += reader.ReadBits(run);
                    break;
                }

                k += run;
                if (k > se)
                    break;

                int value = ReceiveAndExtend(reader, size) << al;
                int idx = QuantizationTable.ZigZagOrder[k];
                buffer.SetCoefficient(componentIndex, blockX, blockY, idx, (short)value);
                k++;
            }
        }

        private static void DecodeAcRefinement(
            HuffmanTable? acTable,
            BitReader reader,
            CoefficientBuffer buffer,
            int componentIndex,
            int blockX,
            int blockY,
            int ss,
            int se,
            int al,
            ref int eobRun)
        {
            if (acTable == null)
                throw new InvalidDataException("Missing AC Huffman table for progressive scan.");

            short refineDelta = (short)(1 << al);
            short negativeRefineDelta = (short)-refineDelta;
            int k = ss;

            if (eobRun == 0)
            {
                while (k <= se)
                {
                    int rs = acTable.Decode(reader);
                    int run = (rs >> 4) & 0x0F;
                    int size = rs & 0x0F;

                    short newCoefficient = 0;
                    if (size != 0)
                    {
                        if (size != 1)
                            throw new InvalidDataException("Invalid progressive AC refinement symbol.");

                        newCoefficient = reader.ReadBit() != 0 ? refineDelta : negativeRefineDelta;
                    }
                    else if (run != 15)
                    {
                        eobRun = 1 << run;
                        if (run > 0)
                            eobRun += reader.ReadBits(run);
                        break;
                    }

                    while (k <= se)
                    {
                        int idx = QuantizationTable.ZigZagOrder[k];
                        short current = buffer.GetCoefficient(componentIndex, blockX, blockY, idx);

                        if (current != 0)
                        {
                            RefineCoefficient(reader, buffer, componentIndex, blockX, blockY, idx, current, refineDelta, negativeRefineDelta);
                        }
                        else
                        {
                            if (run == 0)
                                break;
                            run--;
                        }

                        k++;
                    }

                    if (newCoefficient != 0)
                    {
                        if (k <= se)
                        {
                            int idx = QuantizationTable.ZigZagOrder[k];
                            buffer.SetCoefficient(componentIndex, blockX, blockY, idx, newCoefficient);
                            k++;
                        }
                    }
                }
            }

            if (eobRun > 0)
            {
                ApplyRefinementBits(reader, buffer, componentIndex, blockX, blockY, k, se, refineDelta, negativeRefineDelta);
                eobRun--;
            }
        }

        private static void RefineCoefficient(
            BitReader reader,
            CoefficientBuffer buffer,
            int componentIndex,
            int blockX,
            int blockY,
            int naturalIndex,
            short current,
            short refineDelta,
            short negativeRefineDelta)
        {
            if (reader.ReadBit() == 0)
                return;

            if ((current & refineDelta) != 0)
                return;

            short delta = current > 0 ? refineDelta : negativeRefineDelta;
            buffer.AddCoefficient(componentIndex, blockX, blockY, naturalIndex, delta);
        }

        private static void ApplyRefinementBits(
            BitReader reader,
            CoefficientBuffer buffer,
            int componentIndex,
            int blockX,
            int blockY,
            int startK,
            int endK,
            short refineDelta,
            short negativeRefineDelta)
        {
            for (int k = startK; k <= endK; k++)
            {
                int idx = QuantizationTable.ZigZagOrder[k];
                short current = buffer.GetCoefficient(componentIndex, blockX, blockY, idx);
                if (current == 0)
                    continue;

                RefineCoefficient(reader, buffer, componentIndex, blockX, blockY, idx, current, refineDelta, negativeRefineDelta);
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
}
