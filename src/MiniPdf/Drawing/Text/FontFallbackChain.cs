using System;
using System.Collections.Generic;
using MiniSoftware.Drawing.Enums;

namespace MiniSoftware.Drawing.Text
{
    /// <summary>
    /// An ordered list of font families queried to find a glyph for a code
    /// point that the primary font lacks. Used for automatic font fallback
    /// during text layout.
    /// </summary>
    public sealed class FontFallbackChain
    {
        private readonly List<FontFamily> _families = new List<FontFamily>();

        /// <summary>Initializes an empty fallback chain.</summary>
        public FontFallbackChain() { }

        /// <summary>Initializes a chain containing the specified families.</summary>
        public FontFallbackChain(params FontFamily[] families)
        {
            if (families == null) throw new ArgumentNullException(nameof(families));
            foreach (var f in families) _families.Add(f);
        }

        /// <summary>The ordered list of fallback font families.</summary>
        public IReadOnlyList<FontFamily> Families => _families;

        /// <summary>Adds a fallback family to the end of the chain.</summary>
        public FontFallbackChain Add(FontFamily family)
        {
            if (family == null) throw new ArgumentNullException(nameof(family));
            _families.Add(family);
            return this;
        }

        /// <summary>
        /// Returns the family from the chain that provides a glyph for
        /// <paramref name="codePoint"/>, or null if none do.
        /// </summary>
        public FontFamily? Resolve(uint codePoint, FontStyle style)
        {
            foreach (var family in _families)
            {
                var face = family.GetFontFace(style);
                if (face != null && face.Encoding.HasGlyph(codePoint))
                    return family;
            }
            return null;
        }

        /// <summary>
        /// Creates the default fallback chain for the current operating system,
        /// using installed fonts. Returns an empty chain when no fallback
        /// families are installed.
        /// </summary>
        public static FontFallbackChain CreateDefault()
        {
            var chain = new FontFallbackChain();

            string[] candidateNames;
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                candidateNames = new[]
                {
                    "Segoe UI", "Segoe UI Symbol", "Segoe UI Emoji",
                    "Segoe UI Historic", "Tahoma", "Arial Unicode MS",
                    "Arial", "Times New Roman"
                };
            }
            else if (Environment.OSVersion.Platform == PlatformID.Unix)
            {
                candidateNames = new[]
                {
                    "DejaVu Sans", "Noto Sans", "Noto Sans Symbols",
                    "Noto Color Emoji", "Liberation Sans"
                };
            }
            else if (Environment.OSVersion.Platform == PlatformID.MacOSX)
            {
                candidateNames = new[]
                {
                    "Helvetica Neue", "Apple Color Emoji",
                    "Apple Symbols", "Geneva"
                };
            }
            else
            {
                return chain;
            }

            var installed = InstalledFontCollection.Default;
            foreach (var name in candidateNames)
            {
                foreach (var fam in installed.Families)
                {
                    if (string.Equals(fam.Name, name, StringComparison.OrdinalIgnoreCase))
                    {
                        chain._families.Add(fam);
                        break;
                    }
                }
            }

            return chain;
        }
    }
}