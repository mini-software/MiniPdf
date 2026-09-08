using System;
using System.IO;
using MiniSoftware.Drawing.Imaging;

namespace MiniSoftware.Drawing.Codecs
{
    /// <summary>
    /// JPEG codec.
    /// Encode and decode use the managed MiniSoftware.Drawing JPEG pipeline.
    /// </summary>
    internal sealed class JpegCodec : IImageCodec
    {
        public bool CanDecode(byte[] header)
            => header.Length >= 3
            && header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF;

        // ─── Decode ──────────────────────────────────────────────────────────────

        public Bitmap Decode(Stream stream)
        {
            var decoder = new Jpeg.JpegDecoder();
            return decoder.Decode(stream);
        }

        // ─── Encode ──────────────────────────────────────────────────────────────

        public void Encode(Bitmap bmp, Stream stream, EncoderParameters? parameters)
        {
            var encoder = new Jpeg.JpegEncoder();
            encoder.Encode(bmp, stream, parameters);
        }
    }
}
