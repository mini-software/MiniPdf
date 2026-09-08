using System;
using System.Collections.Generic;
using MiniSoftware.Drawing.Geometry;

namespace MiniSoftware.Drawing.Imaging
{
    internal sealed class FrameAnimator
    {
        private readonly int _canvasWidth;
        private readonly int _canvasHeight;
        private readonly int _canvasStride;
        private readonly List<AnimationFrame> _frames;

        private readonly byte[] _output;
        private readonly byte[] _previous;
        private int _lastComposedIndex = -1;

        public FrameAnimator(int canvasWidth, int canvasHeight, List<AnimationFrame> frames)
        {
            _canvasWidth = canvasWidth;
            _canvasHeight = canvasHeight;
            _canvasStride = canvasWidth * 4;
            _frames = frames;
            _output = new byte[canvasHeight * _canvasStride];
            _previous = new byte[canvasHeight * _canvasStride];
        }

        public byte[] Compose(int targetIndex)
        {
            if (targetIndex < 0 || targetIndex >= _frames.Count)
                throw new ArgumentOutOfRangeException(nameof(targetIndex));

            if (targetIndex < _lastComposedIndex)
            {
                Array.Clear(_output, 0, _output.Length);
                _lastComposedIndex = -1;
            }

            for (int i = _lastComposedIndex + 1; i <= targetIndex; i++)
            {
                var frame = _frames[i];

                if (frame.DisposeMethod == FrameDisposeMethod.Previous)
                    SnapshotRegion(_output, _previous, frame.Bounds);

                if (frame.BlendMethod == FrameBlendMethod.Source)
                    BlendSource(_output, frame);
                else
                    BlendOver(_output, frame);

                if (i < targetIndex)
                    ApplyDispose(_output, frame);
            }

            _lastComposedIndex = targetIndex;
            return _output;
        }

        private void BlendSource(byte[] canvas, AnimationFrame frame)
        {
            var b = frame.Bounds;
            int frameStride = frame.Stride;
            byte[] src = frame.Pixels;
            for (int y = 0; y < b.Height; y++)
            {
                int srcOff = y * frameStride;
                int dstOff = (b.Y + y) * _canvasStride + b.X * 4;
                Buffer.BlockCopy(src, srcOff, canvas, dstOff, b.Width * 4);
            }
        }

        private void BlendOver(byte[] canvas, AnimationFrame frame)
        {
            var b = frame.Bounds;
            int frameStride = frame.Stride;
            byte[] src = frame.Pixels;
            for (int y = 0; y < b.Height; y++)
            {
                int srcOff = y * frameStride;
                int dstOff = (b.Y + y) * _canvasStride + b.X * 4;
                for (int x = 0; x < b.Width; x++)
                {
                    int s = srcOff + x * 4;
                    int d = dstOff + x * 4;
                    byte sa = src[s + 3];
                    if (sa == 0) continue;
                    if (sa == 255)
                    {
                        canvas[d] = src[s];
                        canvas[d + 1] = src[s + 1];
                        canvas[d + 2] = src[s + 2];
                        canvas[d + 3] = src[s + 3];
                        continue;
                    }
                    int inv = 255 - sa;
                    canvas[d] = (byte)((src[s] * sa + canvas[d] * inv) / 255);
                    canvas[d + 1] = (byte)((src[s + 1] * sa + canvas[d + 1] * inv) / 255);
                    canvas[d + 2] = (byte)((src[s + 2] * sa + canvas[d + 2] * inv) / 255);
                    canvas[d + 3] = (byte)(sa + canvas[d + 3] * inv / 255);
                }
            }
        }

        private void ApplyDispose(byte[] canvas, AnimationFrame frame)
        {
            var b = frame.Bounds;
            switch (frame.DisposeMethod)
            {
                case FrameDisposeMethod.None:
                    break;
                case FrameDisposeMethod.Background:
                    for (int y = 0; y < b.Height; y++)
                    {
                        int off = (b.Y + y) * _canvasStride + b.X * 4;
                        Array.Clear(canvas, off, b.Width * 4);
                    }
                    break;
                case FrameDisposeMethod.Previous:
                    RestoreRegion(_previous, canvas, frame.Bounds);
                    break;
            }
        }

        private void SnapshotRegion(byte[] src, byte[] dst, Rectangle b)
        {
            for (int y = 0; y < b.Height; y++)
            {
                int off = (b.Y + y) * _canvasStride + b.X * 4;
                Buffer.BlockCopy(src, off, dst, off, b.Width * 4);
            }
        }

        private void RestoreRegion(byte[] src, byte[] dst, Rectangle b)
        {
            for (int y = 0; y < b.Height; y++)
            {
                int off = (b.Y + y) * _canvasStride + b.X * 4;
                Buffer.BlockCopy(src, off, dst, off, b.Width * 4);
            }
        }
    }
}