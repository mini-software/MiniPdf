using System;
using System.IO;
using System.Runtime.InteropServices;
using MiniSoftware.Drawing.Font.Sources;

namespace MiniSoftware.Drawing.Text
{
    /// <summary>
    /// Represents a collection of font families provided by the application.
    /// </summary>
    public sealed class PrivateFontCollection : FontCollection
    {
        /// <summary>
        /// Adds a font from the specified file to the collection.
        /// </summary>
        /// <param name="filename">The path to the font file.</param>
        public void AddFontFile(string filename)
        {
            if (string.IsNullOrWhiteSpace(filename))
                throw new ArgumentException("File path must not be empty.", nameof(filename));
            if (!File.Exists(filename))
                throw new FileNotFoundException("Font file was not found.", filename);

            AddFacesFromSource(new FileSystemStreamSource(filename));
        }

        /// <summary>
        /// Adds a font from memory to the collection.
        /// </summary>
        /// <param name="memory">A pointer to the memory block containing the font data.</param>
        /// <param name="length">The length of the font data in bytes.</param>
        public void AddMemoryFont(IntPtr memory, int length)
        {
            if (memory == IntPtr.Zero)
                throw new ArgumentNullException(nameof(memory));
            if (length <= 0)
                throw new ArgumentOutOfRangeException(nameof(length), "Length must be positive.");

            var bytes = new byte[length];
            Marshal.Copy(memory, bytes, 0, length);
            AddFacesFromSource(new ByteContentStreamSource(bytes));
        }
    }
}