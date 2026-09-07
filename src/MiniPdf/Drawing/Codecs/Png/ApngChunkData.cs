using System;
using MiniPdf.Drawing.Geometry;
using MiniPdf.Drawing.Imaging;

namespace MiniPdf.Drawing.Codecs.Png
{
    internal readonly struct ApngFrameControl
    {
        public uint SequenceNumber { get; }
        public int Width { get; }
        public int Height { get; }
        public int XOffset { get; }
        public int YOffset { get; }
        public ushort DelayNum { get; }
        public ushort DelayDen { get; }
        public FrameDisposeMethod DisposeMethod { get; }
        public FrameBlendMethod BlendMethod { get; }

        public ApngFrameControl(
            uint sequenceNumber, int width, int height,
            int xOffset, int yOffset,
            ushort delayNum, ushort delayDen,
            FrameDisposeMethod disposeMethod, FrameBlendMethod blendMethod)
        {
            SequenceNumber = sequenceNumber;
            Width = width;
            Height = height;
            XOffset = xOffset;
            YOffset = yOffset;
            DelayNum = delayNum;
            DelayDen = delayDen;
            DisposeMethod = disposeMethod;
            BlendMethod = blendMethod;
        }

        public int DelayMilliseconds
        {
            get
            {
                int den = DelayDen == 0 ? 100 : DelayDen;
                if (den == 0) den = 100;
                return (int)((DelayNum * 1000L) / den);
            }
        }

        public Rectangle Bounds => new Rectangle(XOffset, YOffset, Width, Height);

        public static ApngFrameControl Parse(byte[] data)
        {
            if (data == null || data.Length < 26)
                throw new System.IO.InvalidDataException("fcTL chunk too short.");

            uint seq = BinaryBE.ReadUInt32(data, 0);
            int w = (int)BinaryBE.ReadUInt32(data, 4);
            int h = (int)BinaryBE.ReadUInt32(data, 8);
            int x = (int)BinaryBE.ReadUInt32(data, 12);
            int y = (int)BinaryBE.ReadUInt32(data, 16);
            ushort delayNum = BinaryBE.ReadUInt16(data, 20);
            ushort delayDen = BinaryBE.ReadUInt16(data, 22);
            var dispose = (FrameDisposeMethod)data[24];
            var blend = (FrameBlendMethod)data[25];
            return new ApngFrameControl(seq, w, h, x, y, delayNum, delayDen, dispose, blend);
        }
    }

    internal readonly struct ApngAnimationControl
    {
        public int NumFrames { get; }
        public int NumPlays { get; }

        public ApngAnimationControl(int numFrames, int numPlays)
        {
            NumFrames = numFrames;
            NumPlays = numPlays;
        }

        public static ApngAnimationControl Parse(byte[] data)
        {
            if (data == null || data.Length < 8)
                throw new System.IO.InvalidDataException("acTL chunk too short.");
            int n = (int)BinaryBE.ReadUInt32(data, 0);
            int p = (int)BinaryBE.ReadUInt32(data, 4);
            return new ApngAnimationControl(n, p);
        }
    }
}