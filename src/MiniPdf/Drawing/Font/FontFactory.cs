using System;
using System.IO;
using MiniSoftware.Drawing.Font.Cff;
using MiniSoftware.Drawing.Font.Core;
using MiniSoftware.Drawing.Font.IO;
using MiniSoftware.Drawing.Font.Sources;
using MiniSoftware.Drawing.Font.Ttf;
using MiniSoftware.Drawing.Font.Type1;

namespace MiniSoftware.Drawing.Font
{
    /// <summary>
    /// Entry point for loading fonts. Dispatches to the appropriate reader
    /// based on <see cref="FontDefinition.FontType"/> and the stream magic bytes.
    /// </summary>
    public static class FontFactory
    {
        /// <summary>
        /// Opens a single font face from the supplied <paramref name="definition"/>.
        /// </summary>
        /// <returns>
        /// An <see cref="IFont"/> implementation appropriate for the font type
        /// (e.g. <c>TtfFont</c>, <c>CffFont</c>, <c>Type1Font</c>).
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="definition"/> is null.
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// The font format is not yet supported.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// The font data is corrupt or cannot be parsed.
        /// </exception>
        public static IFont Open(FontDefinition definition)
        {
            if (definition is null) throw new ArgumentNullException(nameof(definition));

            var source = definition.FileDefinitions[0].Source;
            using var stream = source.GetFontStream();

            switch (definition.FontType)
            {
                case FontType.TTF:
                case FontType.OTF:
                    return TtfFontReader.Read(stream);

                case FontType.CFF:
                    return CffFontReader.Read(stream);

                case FontType.Type1:
                    return new Type1Font(Type1Parser.Parse(ReadAllBytes(stream)));

                case FontType.WOFF:
                {
                    byte[] sfntBytes = WoffReader.Decompress(stream);
                    using var sfntStream = new MemoryStream(sfntBytes);
                    return TtfFontReader.Read(sfntStream);
                }

                case FontType.WOFF2:
                    // WoffReader.Decompress always throws NotSupportedException on netstandard2.0.
                    Woff2Reader.Decompress(stream);
                    // unreachable
                    throw new InvalidOperationException("Unreachable.");

                default:
                    throw new NotSupportedException(
                        $"Font format '{definition.FontType}' is not yet implemented.");
            }
        }

        /// <summary>
        /// Opens all font faces from a font collection file (TTC).
        /// </summary>
        /// <returns>
        /// A <see cref="FontFaceCollection"/> containing one <see cref="IFont"/> per face.
        /// </returns>
        public static FontFaceCollection OpenCollection(IStreamSource source)
        {
            if (source is null) throw new ArgumentNullException(nameof(source));

            using var stream = source.GetFontStream();
            byte[] data = ReadAllBytes(stream);

            if (data.Length < 4)
                throw new InvalidOperationException("Font data is too short to identify the format.");

            uint magic = ((uint)data[0] << 24) | ((uint)data[1] << 16)
                       | ((uint)data[2] <<  8) |         data[3];

            if (magic == TtcReader.Signature)
            {
                using var ttcStream = new MemoryStream(data, writable: false);
                return new FontFaceCollection(TtcReader.Read(ttcStream));
            }

            // Single-font file — detect format and wrap in a one-element collection.
            IFont single = OpenSingleFontFromBytes(magic, data);
            return new FontFaceCollection(new[] { single });
        }

        // Opens a single IFont from already-buffered bytes, dispatching on magic.
        // FontStreamReader.Detect handles WOFF decompression internally and returns
        // the inner sfnt bytes with TTF/OTF type — no separate WOFF case needed.
        private static IFont OpenSingleFontFromBytes(uint magic, byte[] data)
        {
            var (fontBytes, detectedType) = FontStreamReader.Detect(data);

            switch (detectedType)
            {
                case FontType.TTF:
                case FontType.OTF:
                {
                    using var ms = new MemoryStream(fontBytes, writable: false);
                    return TtfFontReader.Read(ms);
                }

                case FontType.CFF:
                {
                    using var ms = new MemoryStream(fontBytes, writable: false);
                    return CffFontReader.Read(ms);
                }

                case FontType.Type1:
                    return new Type1Font(Type1Parser.Parse(fontBytes));

                default:
                    throw new NotSupportedException(
                        $"Font format (magic 0x{magic:X8}) is not yet supported in OpenCollection.");
            }
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private static byte[] ReadAllBytes(System.IO.Stream stream)
        {
            using var ms = new System.IO.MemoryStream();
            stream.CopyTo(ms);
            return ms.ToArray();
        }
    }
}
