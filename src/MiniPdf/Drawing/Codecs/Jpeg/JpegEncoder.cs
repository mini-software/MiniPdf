using System;
using System.IO;
using MiniPdf.Drawing.Colors;
using MiniPdf.Drawing.Imaging;

namespace MiniPdf.Drawing.Codecs.Jpeg
{
    internal sealed class JpegEncoder
    {
        public void Encode(Bitmap bmp, Stream stream, EncoderParameters? parameters)
        {
            if (bmp == null) throw new ArgumentNullException(nameof(bmp));
            if (stream == null) throw new ArgumentNullException(nameof(stream));

            int quality = GetQuality(parameters);
            int width = bmp._width;
            int height = bmp._height;

            double[] yPlane = new double[width * height];
            double[] cbPlane = new double[width * height];
            double[] crPlane = new double[width * height];
            BuildYCbCrPlanes(bmp, yPlane, cbPlane, crPlane);

            Quantizer quantizer = Quantizer.Create(quality);
            var writer = new JpegWriter(stream);
            writer.WriteHeaders(width, height, quantizer);

            var huffman = new HuffmanEncoder(stream);
            int blocksX = (width + 7) / 8;
            int blocksY = (height + 7) / 8;

            var samples = new double[64];
            var dct = new double[64];
            var quantized = new int[64];
            int previousY = 0;
            int previousCb = 0;
            int previousCr = 0;

            for (int blockY = 0; blockY < blocksY; blockY++)
            {
                for (int blockX = 0; blockX < blocksX; blockX++)
                {
                    EncodeBlock(yPlane, width, height, blockX, blockY, quantizer.LuminanceNatural, huffman, ref previousY, false, samples, dct, quantized);
                    EncodeBlock(cbPlane, width, height, blockX, blockY, quantizer.ChrominanceNatural, huffman, ref previousCb, true, samples, dct, quantized);
                    EncodeBlock(crPlane, width, height, blockX, blockY, quantizer.ChrominanceNatural, huffman, ref previousCr, true, samples, dct, quantized);
                }
            }

            huffman.Flush();
            writer.WriteEndOfImage();
        }

        private static int GetQuality(EncoderParameters? parameters)
        {
            int quality = 75;
            if (parameters == null)
                return quality;

            foreach (EncoderParameter p in parameters.Param)
            {
                if (p.Encoder == Encoder.Quality.Guid)
                {
                    quality = (int)Math.Max(1, Math.Min(100, p._valueLong));
                    break;
                }
            }

            return quality;
        }

        private static void BuildYCbCrPlanes(Bitmap bmp, double[] yPlane, double[] cbPlane, double[] crPlane)
        {
            int width = bmp._width;
            int height = bmp._height;

            for (int y = 0; y < height; y++)
            {
                int row = y * width;
                for (int x = 0; x < width; x++)
                {
                    Color color = bmp.GetPixel(x, y);
                    double r = color.R;
                    double g = color.G;
                    double b = color.B;

                    double yy = 0.299 * r + 0.587 * g + 0.114 * b;
                    double cb = -0.168736 * r - 0.331264 * g + 0.5 * b + 128.0;
                    double cr = 0.5 * r - 0.418688 * g - 0.081312 * b + 128.0;

                    int index = row + x;
                    yPlane[index] = Clamp(yy, 0.0, 255.0);
                    cbPlane[index] = Clamp(cb, 0.0, 255.0);
                    crPlane[index] = Clamp(cr, 0.0, 255.0);
                }
            }
        }

        private static void EncodeBlock(
            double[] plane,
            int width,
            int height,
            int blockX,
            int blockY,
            ushort[] quantTable,
            HuffmanEncoder huffman,
            ref int previousDc,
            bool chroma,
            double[] samples,
            double[] dct,
            int[] quantized)
        {
            int baseX = blockX * 8;
            int baseY = blockY * 8;

            for (int y = 0; y < 8; y++)
            {
                int srcY = Math.Min(height - 1, baseY + y);
                int srcRow = srcY * width;
                int dstRow = y * 8;

                for (int x = 0; x < 8; x++)
                {
                    int srcX = Math.Min(width - 1, baseX + x);
                    samples[dstRow + x] = plane[srcRow + srcX] - 128.0;
                }
            }

            ForwardDct.TransformBlock(samples, dct);

            for (int i = 0; i < 64; i++)
                quantized[i] = (int)Math.Round(dct[i] / quantTable[i]);

            huffman.EncodeBlock(quantized, ref previousDc, chroma);
        }

        private static double Clamp(double value, double min, double max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }
    }
}
