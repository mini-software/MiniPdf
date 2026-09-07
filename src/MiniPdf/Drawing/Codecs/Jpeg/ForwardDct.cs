using System;

namespace MiniSoftware.Drawing.Codecs.Jpeg
{
    internal static class ForwardDct
    {
        private static readonly double InvSqrt2 = 1.0 / Math.Sqrt(2.0);
        private static readonly double[,] Cosine = BuildCosineTable();

        public static void TransformBlock(double[] samples, double[] output)
        {
            if (samples == null) throw new ArgumentNullException(nameof(samples));
            if (output == null) throw new ArgumentNullException(nameof(output));
            if (samples.Length < 64) throw new ArgumentException("Forward DCT requires 64 samples.", nameof(samples));
            if (output.Length < 64) throw new ArgumentException("Forward DCT output requires 64 values.", nameof(output));

            for (int v = 0; v < 8; v++)
            {
                double cv = v == 0 ? InvSqrt2 : 1.0;
                for (int u = 0; u < 8; u++)
                {
                    double cu = u == 0 ? InvSqrt2 : 1.0;
                    double sum = 0.0;

                    for (int y = 0; y < 8; y++)
                    {
                        for (int x = 0; x < 8; x++)
                        {
                            sum += samples[y * 8 + x] * Cosine[x, u] * Cosine[y, v];
                        }
                    }

                    output[v * 8 + u] = 0.25 * cu * cv * sum;
                }
            }
        }

        private static double[,] BuildCosineTable()
        {
            var table = new double[8, 8];
            for (int x = 0; x < 8; x++)
            {
                for (int u = 0; u < 8; u++)
                    table[x, u] = Math.Cos(((2.0 * x + 1.0) * u * Math.PI) / 16.0);
            }

            return table;
        }
    }
}
