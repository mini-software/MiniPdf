using System;

namespace MiniSoftware.Drawing.Codecs.Jpeg
{
    internal sealed class CoefficientBuffer
    {
        private readonly short[][] _componentData;
        private readonly int[] _blocksX;
        private readonly int[] _blocksY;

        public CoefficientBuffer(FrameHeader frame)
        {
            if (frame == null) throw new ArgumentNullException(nameof(frame));

            Frame = frame;
            ComponentCount = frame.Components.Length;

            int maxH = 1;
            int maxV = 1;
            for (int i = 0; i < ComponentCount; i++)
            {
                maxH = Math.Max(maxH, frame.Components[i].HSampling);
                maxV = Math.Max(maxV, frame.Components[i].VSampling);
            }

            MaxHorizontalSampling = maxH;
            MaxVerticalSampling = maxV;
            McuCountX = (frame.Width + (8 * maxH) - 1) / (8 * maxH);
            McuCountY = (frame.Height + (8 * maxV) - 1) / (8 * maxV);

            _componentData = new short[ComponentCount][];
            _blocksX = new int[ComponentCount];
            _blocksY = new int[ComponentCount];

            for (int i = 0; i < ComponentCount; i++)
            {
                int bx = McuCountX * frame.Components[i].HSampling;
                int by = McuCountY * frame.Components[i].VSampling;
                _blocksX[i] = bx;
                _blocksY[i] = by;
                _componentData[i] = new short[bx * by * 64];
            }
        }

        public FrameHeader Frame { get; }
        public int ComponentCount { get; }
        public int MaxHorizontalSampling { get; }
        public int MaxVerticalSampling { get; }
        public int McuCountX { get; }
        public int McuCountY { get; }

        public int GetBlocksX(int componentIndex) => _blocksX[componentIndex];
        public int GetBlocksY(int componentIndex) => _blocksY[componentIndex];

        public short[] GetComponentData(int componentIndex) => _componentData[componentIndex];

        public int GetBlockOffset(int componentIndex, int blockX, int blockY)
            => ((blockY * _blocksX[componentIndex]) + blockX) * 64;

        public short GetCoefficient(int componentIndex, int blockX, int blockY, int naturalIndex)
        {
            int offset = GetBlockOffset(componentIndex, blockX, blockY) + naturalIndex;
            return _componentData[componentIndex][offset];
        }

        public void SetCoefficient(int componentIndex, int blockX, int blockY, int naturalIndex, short value)
        {
            int offset = GetBlockOffset(componentIndex, blockX, blockY) + naturalIndex;
            _componentData[componentIndex][offset] = value;
        }

        public void AddCoefficient(int componentIndex, int blockX, int blockY, int naturalIndex, short delta)
        {
            int offset = GetBlockOffset(componentIndex, blockX, blockY) + naturalIndex;
            _componentData[componentIndex][offset] = (short)(_componentData[componentIndex][offset] + delta);
        }
    }
}
