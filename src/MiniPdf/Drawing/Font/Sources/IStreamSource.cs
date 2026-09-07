using System.IO;

namespace MiniPdf.Drawing.Font.Sources
{
    /// <summary>
    /// Provides a readable <see cref="Stream"/> containing raw font bytes.
    /// Each call to <see cref="GetFontStream"/> must return a freshly opened,
    /// independently positioned stream that the caller owns and must dispose.
    /// </summary>
    public interface IStreamSource
    {
        /// <summary>
        /// Opens and returns a stream over the font data.
        /// The caller is responsible for disposing the returned stream.
        /// </summary>
        Stream GetFontStream();
    }
}
