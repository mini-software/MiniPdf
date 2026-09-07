using System;
using System.IO;

namespace MiniSoftware.Drawing.Font.Sources
{
    /// <summary>
    /// Reads a font from a file on the local file system.
    /// </summary>
    public sealed class FileSystemStreamSource : IStreamSource
    {
        /// <summary>
        /// Gets the absolute or relative path to the font file.
        /// </summary>
        public string FilePath { get; }

        /// <summary>
        /// Initializes a new <see cref="FileSystemStreamSource"/> with the specified file path.
        /// </summary>
        /// <param name="filePath">The path to the font file.</param>
        public FileSystemStreamSource(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("File path must not be empty.", nameof(filePath));
            FilePath = filePath;
        }

        /// <inheritdoc/>
        public Stream GetFontStream() => File.OpenRead(FilePath);
    }
}
