using System.Collections.Generic;

namespace MiniPdf.Drawing.Font.Type1
{
    /// <summary>
    /// Internal representation of a parsed Type 1 font file.
    /// Populated by <see cref="Type1Parser"/>.
    /// </summary>
    internal sealed class Type1Data
    {
        /// <summary>PostScript font name from /FontName.</summary>
        public string FontName = string.Empty;

        /// <summary>Font bounding box [xMin, yMin, xMax, yMax].</summary>
        public double[] FontBBox = new double[4];

        /// <summary>
        /// Number of random bytes prepended to each encrypted charstring.
        /// Typically 4; 0 means no encryption seed bytes.
        /// </summary>
        public int LenIV = 4;

        /// <summary>Decrypted charstrings keyed by glyph name.</summary>
        public Dictionary<string, byte[]> CharStrings = new Dictionary<string, byte[]>();

        /// <summary>
        /// Glyph names in the order they were encountered during parsing.
        /// Index in this array is the glyph index used by <see cref="IGlyphAccessor"/>.
        /// </summary>
        public string[] GlyphOrder = System.Array.Empty<string>();

        /// <summary>
        /// Decrypted subroutine charstrings from /Subrs.
        /// Null when the font has no subroutines.
        /// </summary>
        public byte[][]? Subrs;

        /// <summary>
        /// Font encoding: maps byte value (0-255) to a glyph name.
        /// Defaults to .notdef for unmapped slots.
        /// </summary>
        public string[] Encoding = new string[256];

        public Type1Data()
        {
            for (int i = 0; i < 256; i++) Encoding[i] = ".notdef";
        }
    }
}
