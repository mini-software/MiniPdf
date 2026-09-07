using System;

namespace MiniSoftware.Drawing.Codecs.Jpeg
{
    internal static class ColorConverter
    {
        public static void YCbCrToRgb(int y, int cb, int cr, out byte r, out byte g, out byte b)
        {
            int d = cb - 128;
            int e = cr - 128;

            int rr = y + ((91881 * e + 32768) >> 16);
            int gg = y - ((22554 * d + 46802 * e + 32768) >> 16);
            int bb = y + ((116130 * d + 32768) >> 16);

            r = (byte)ClampToByte(rr);
            g = (byte)ClampToByte(gg);
            b = (byte)ClampToByte(bb);
        }

        public static void GrayscaleToRgb(int y, out byte r, out byte g, out byte b)
        {
            byte v = (byte)ClampToByte(y);
            r = v;
            g = v;
            b = v;
        }

        private static int ClampToByte(int value) => Math.Max(0, Math.Min(255, value));
    }
}
