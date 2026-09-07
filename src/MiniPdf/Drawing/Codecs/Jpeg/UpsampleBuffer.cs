using System;

namespace MiniSoftware.Drawing.Codecs.Jpeg
{
    internal static class UpsampleBuffer
    {
        public static byte[] UpsampleToFullResolution(
            byte[] source,
            int sourceWidth,
            int sourceHeight,
            int targetWidth,
            int targetHeight,
            int horizontalSampling,
            int verticalSampling,
            int maxHorizontalSampling,
            int maxVerticalSampling)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (sourceWidth <= 0 || sourceHeight <= 0)
                throw new ArgumentOutOfRangeException(nameof(sourceWidth));
            if (targetWidth <= 0 || targetHeight <= 0)
                throw new ArgumentOutOfRangeException(nameof(targetWidth));

            var upsampled = new byte[targetWidth * targetHeight];

            for (int y = 0; y < targetHeight; y++)
            {
                int sy = (y * verticalSampling) / maxVerticalSampling;
                if (sy >= sourceHeight) sy = sourceHeight - 1;

                int dstRow = y * targetWidth;
                int srcRow = sy * sourceWidth;
                for (int x = 0; x < targetWidth; x++)
                {
                    int sx = (x * horizontalSampling) / maxHorizontalSampling;
                    if (sx >= sourceWidth) sx = sourceWidth - 1;
                    upsampled[dstRow + x] = source[srcRow + sx];
                }
            }

            return upsampled;
        }
    }
}
