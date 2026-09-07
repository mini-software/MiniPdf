using System;
using System.IO;
using MiniSoftware.Drawing.Imaging;

namespace MiniSoftware.Drawing.Codecs
{
    /// <summary>EMF/WMF decoder backed by the MiniSoftware.Drawing metafile parser and playback engine.</summary>
    internal sealed class EmfCodec : IImageCodec
    {
        public bool CanDecode(byte[] header)
            => LooksLikeEmf(header) || LooksLikePlaceableWmf(header) || LooksLikeStandardWmf(header);

        public Bitmap Decode(Stream stream)
        {
            using var metafile = new MiniSoftware.Drawing.Imaging.Metafile(stream);
            return metafile.RenderToBitmap();
        }

        public void Encode(Bitmap bmp, Stream stream, EncoderParameters? parameters)
            => throw new NotSupportedException("EMF encoding is not supported.");

        private static bool LooksLikeEmf(byte[] header)
            => header.Length >= 4
            && header[0] == 0x01 && header[1] == 0x00 && header[2] == 0x00 && header[3] == 0x00;

        private static bool LooksLikePlaceableWmf(byte[] header)
            => header.Length >= 4
            && header[0] == 0xD7 && header[1] == 0xCD && header[2] == 0xC6 && header[3] == 0x9A;

        private static bool LooksLikeStandardWmf(byte[] header)
            => header.Length >= 6
            && (header[0] == 0x01 || header[0] == 0x02)
            && header[1] == 0x00
            && header[2] == 0x09
            && header[3] == 0x00
            && header[4] == 0x00
            && (header[5] == 0x01 || header[5] == 0x03);
    }
}
