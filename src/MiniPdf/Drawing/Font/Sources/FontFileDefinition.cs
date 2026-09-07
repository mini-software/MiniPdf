using System;

namespace MiniPdf.Drawing.Font.Sources
{
    /// <summary>
    /// Pairs a file-extension hint (e.g. "ttf", "otf", "cff", "pfb") with the
    /// <see cref="IStreamSource"/> that provides the raw font bytes.
    /// </summary>
    public sealed class FontFileDefinition
    {
        /// <summary>
        /// File extension without the leading dot, lower-case (e.g. "ttf", "woff2").
        /// Used as a hint when the font format cannot be inferred from the stream magic bytes.
        /// </summary>
        public string FileExtension { get; }

        /// <summary>
        /// Gets the source from which the font bytes are obtained.
        /// </summary>
        public IStreamSource Source { get; }

        /// <summary>
        /// Initializes a new <see cref="FontFileDefinition"/> with the specified file extension and source.
        /// </summary>
        /// <param name="fileExtension">The file extension without the leading dot (e.g. "ttf", "otf").</param>
        /// <param name="source">The source from which the font bytes are obtained.</param>
        public FontFileDefinition(string fileExtension, IStreamSource source)
        {
            if (string.IsNullOrWhiteSpace(fileExtension))
                throw new ArgumentException("File extension must not be empty.", nameof(fileExtension));
            FileExtension = fileExtension.TrimStart('.').ToLowerInvariant();
            Source = source ?? throw new ArgumentNullException(nameof(source));
        }
    }
}
