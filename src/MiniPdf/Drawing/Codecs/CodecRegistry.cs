using System;
using System.IO;
using MiniSoftware.Drawing.Imaging;

namespace MiniSoftware.Drawing.Codecs
{
    /// <summary>
    /// Central registry that routes encode/decode operations to the appropriate codec.
    /// </summary>
    internal static class CodecRegistry
    {
        private static readonly IImageCodec[] Codecs =
        {
            new BmpCodec(),
            new PngCodec(),
            new GifCodec(),
            new JpegCodec(),
            new TiffCodec(),
            new IcoCodec(),
            new EmfCodec(),
        };

        // ─── Decode ──────────────────────────────────────────────────────────────

        public static Bitmap Decode(Stream stream)
        {
            // Ensure seekable
            Stream seekable;
            bool   ownStream = false;
            if (!stream.CanSeek)
            {
                var ms = new MemoryStream();
                stream.CopyTo(ms);
                ms.Position = 0;
                seekable  = ms;
                ownStream = true;
            }
            else
            {
                seekable = stream;
            }

            try
            {
                var header = new byte[12];
                int n = seekable.Read(header, 0, 12);
                seekable.Seek(-n, SeekOrigin.Current);

                foreach (var codec in Codecs)
                {
                    if (codec.CanDecode(header))
                        return codec.Decode(seekable);
                }

                throw new NotSupportedException("Unknown or unsupported image format.");
            }
            finally
            {
                if (ownStream) seekable.Dispose();
            }
        }

        // ─── Encode ──────────────────────────────────────────────────────────────

        public static void Encode(Bitmap bmp, Stream stream, ImageFormat format,
                                  EncoderParameters? parameters)
        {
            var codec = FindEncoder(format);
            if (codec == null)
                throw new NotSupportedException($"No encoder found for format {format?.Name}.");
            codec.Encode(bmp, stream, parameters);
        }

        private static IImageCodec? FindEncoder(ImageFormat? format)
        {
            if (format == null) return null;
            if (format.Equals(ImageFormat.Bmp)      || format.Equals(ImageFormat.MemoryBmp))
                return Codecs[0]; // BmpCodec
            if (format.Equals(ImageFormat.Png))  return Codecs[1]; // PngCodec
            if (format.Equals(ImageFormat.Gif))  return Codecs[2]; // GifCodec
            if (format.Equals(ImageFormat.Jpeg)) return Codecs[3]; // JpegCodec
            if (format.Equals(ImageFormat.Tiff)) return Codecs[4]; // TiffCodec
            return null;
        }
    }
}
