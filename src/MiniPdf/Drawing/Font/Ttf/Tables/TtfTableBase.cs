using System;
using System.IO;
using MiniPdf.Drawing.Font.IO;

namespace MiniPdf.Drawing.Font.Ttf.Tables
{
    /// <summary>
    /// Abstract base for all parsed OpenType/TrueType table objects.
    /// Holds the raw table bytes so individual parsers can be re-run on demand.
    /// </summary>
    public abstract class TtfTableBase
    {
        /// <summary>Four-character ASCII table tag (e.g. "head", "cmap").</summary>
        public string Tag { get; }

        /// <summary>
        /// Raw bytes of this table as extracted from the font file.
        /// The slice corresponds exactly to the byte range described by the
        /// <see cref="OpenTypeTableRecord"/> entry in the table directory.
        /// </summary>
        public byte[] RawBytes { get; }

        /// <summary>
        /// Initializes a new <see cref="TtfTableBase"/> with the specified tag and raw bytes.
        /// </summary>
        /// <param name="tag">The four-character ASCII table tag.</param>
        /// <param name="rawBytes">The raw bytes of the table.</param>
        protected TtfTableBase(string tag, byte[] rawBytes)
        {
            Tag      = tag      ?? throw new ArgumentNullException(nameof(tag));
            RawBytes = rawBytes ?? throw new ArgumentNullException(nameof(rawBytes));
        }

        /// <summary>
        /// Creates a new <see cref="FontBinaryReader"/> positioned at byte 0 of
        /// <see cref="RawBytes"/>.  The caller owns the returned reader and must
        /// dispose it (which also disposes the underlying <see cref="MemoryStream"/>).
        /// </summary>
        protected FontBinaryReader CreateReader() =>
            new FontBinaryReader(new MemoryStream(RawBytes));
    }
}
