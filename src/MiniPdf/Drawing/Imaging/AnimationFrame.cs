using MiniSoftware.Drawing.Geometry;

namespace MiniSoftware.Drawing.Imaging
{
    internal sealed class AnimationFrame
    {
        public Rectangle Bounds { get; }
        public int DelayMilliseconds { get; set; }
        public FrameDisposeMethod DisposeMethod { get; set; }
        public FrameBlendMethod BlendMethod { get; set; }
        public byte[] Pixels { get; }

        public int Stride => Bounds.Width * 4;

        public AnimationFrame(Rectangle bounds, byte[] pixels)
        {
            Bounds = bounds;
            Pixels = pixels;
        }
    }
}