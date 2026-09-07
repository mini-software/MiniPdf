using System;

namespace MiniPdf.Drawing.Codecs.Jpeg
{
    internal static class IdctTransform
    {
        private static readonly double InvSqrt2 = 1.0 / Math.Sqrt(2.0);
        private static readonly double[,] Cosine = BuildCosineTable();

        public static void TransformBlock(int[] coeffs, byte[] destination)
        {
            if (coeffs == null) throw new ArgumentNullException(nameof(coeffs));
            if (destination == null) throw new ArgumentNullException(nameof(destination));
            if (coeffs.Length < 64) throw new ArgumentException("IDCT requires 64 coefficients.", nameof(coeffs));
            if (destination.Length < 64) throw new ArgumentException("IDCT destination must contain at least 64 bytes.", nameof(destination));

            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 8; x++)
                {
                    double sum = 0.0;
                    for (int v = 0; v < 8; v++)
                    {
                        double cv = v == 0 ? InvSqrt2 : 1.0;
                        for (int u = 0; u < 8; u++)
                        {
                            double cu = u == 0 ? InvSqrt2 : 1.0;
                            int c = coeffs[v * 8 + u];
                            sum += cu * cv * c * Cosine[x, u] * Cosine[y, v];
                        }
                    }

                    int sample = (int)Math.Round(0.25 * sum + 128.0);
                    destination[y * 8 + x] = (byte)Math.Max(0, Math.Min(255, sample));
                }
            }
        }

        private static double[,] BuildCosineTable()
        {
            var table = new double[8, 8];
            for (int x = 0; x < 8; x++)
            {
                for (int u = 0; u < 8; u++)
                {
                    table[x, u] = Math.Cos(((2.0 * x + 1.0) * u * Math.PI) / 16.0);
                }
            }

            return table;
        }
    }
}
