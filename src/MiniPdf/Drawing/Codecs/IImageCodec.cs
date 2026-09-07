using System.IO;
using MiniSoftware.Drawing.Imaging;

namespace MiniSoftware.Drawing.Codecs
{
    /// <summary>
    /// Contract for a single image format encoder/decoder.
    /// CanDecode receives the first 12 bytes of the stream; the stream position
    /// is never changed by this call.
    /// </summary>
    internal interface IImageCodec
    {
        /// <summary>Returns true if the supplied magic bytes identify this format.</summary>
        bool CanDecode(byte[] header);

        /// <summary>Decode a complete image stream, returning a Bitmap.</summary>
        Bitmap Decode(Stream stream);

        /// <summary>Encode <paramref name="bmp"/> into <paramref name="stream"/>.</summary>
        void Encode(Bitmap bmp, Stream stream, EncoderParameters? parameters);
    }
}
