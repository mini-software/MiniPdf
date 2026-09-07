using MiniSoftware.Drawing.Font.Core;
using System;

namespace MiniSoftware.Drawing.Font.Sources
{
    /// <summary>
    /// Fully describes how to locate and load a font: its declared <see cref="FontType"/>
    /// and one or more <see cref="FontFileDefinition"/> entries (most fonts have a single
    /// file; Type 1 may supply a .pfb and an optional .afm).
    /// </summary>
    public sealed class FontDefinition
    {
        /// <summary>Declared format of the font.</summary>
        public FontType FontType { get; }

        /// <summary>Ordered list of file definitions that together make up the font.</summary>
        public FontFileDefinition[] FileDefinitions { get; }

        /// <summary>
        /// Convenience constructor for single-file fonts (TTF, OTF, CFF, WOFF, WOFF2).
        /// </summary>
        public FontDefinition(FontType fontType, FontFileDefinition fileDefinition)
        {
            FontType = fontType;
            FileDefinitions = new[]
            {
                fileDefinition ?? throw new ArgumentNullException(nameof(fileDefinition))
            };
        }

        /// <summary>
        /// Constructor for fonts that span multiple files (e.g. Type 1: .pfb + .afm).
        /// </summary>
        public FontDefinition(FontType fontType, FontFileDefinition[] fileDefinitions)
        {
            if (fileDefinitions == null || fileDefinitions.Length == 0)
                throw new ArgumentException(
                    "At least one file definition is required.", nameof(fileDefinitions));
            FontType = fontType;
            FileDefinitions = fileDefinitions;
        }
    }
}
