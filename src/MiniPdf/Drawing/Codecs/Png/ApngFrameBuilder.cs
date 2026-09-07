using MiniPdf.Drawing.Geometry;
using MiniPdf.Drawing.Imaging;

namespace MiniPdf.Drawing.Codecs.Png
{
    internal static class ApngFrameBuilder
    {
        public static AnimationFrame Build(
            ApngFrameControl fc,
            byte[] compressed,
            int bitDepth, int colorType,
            byte[]? palette, byte[]? trns)
        {
            byte[] raw = ZlibHelper.Decompress(compressed);

            int channels = PngHelpers.GetChannels(colorType);
            int stride = (fc.Width * bitDepth * channels + 7) / 8;
            int bpp = System.Math.Max(1, (bitDepth * channels) / 8);

            byte[] pixels = PngHelpers.ReconstructFilters(raw, fc.Width, fc.Height, stride, bpp);

            var bgra = new byte[fc.Width * fc.Height * 4];
            for (int y = 0; y < fc.Height; y++)
            for (int x = 0; x < fc.Width; x++)
            {
                PngHelpers.GetPixelRgba(pixels, x, y, stride, bitDepth, colorType,
                                         palette, trns, out byte r, out byte g, out byte b, out byte a);
                int off = y * fc.Width * 4 + x * 4;
                bgra[off] = b;
                bgra[off + 1] = g;
                bgra[off + 2] = r;
                bgra[off + 3] = a;
            }

            var frame = new AnimationFrame(fc.Bounds, bgra)
            {
                DelayMilliseconds = fc.DelayMilliseconds,
                DisposeMethod = fc.DisposeMethod,
                BlendMethod = fc.BlendMethod
            };
            return frame;
        }
    }
}