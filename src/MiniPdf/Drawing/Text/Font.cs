using MiniPdf.Drawing.Enums;
using System;

namespace MiniPdf.Drawing.Text
{
    /// <summary>
    /// Defines a text font used for drawing text with <see cref="Graphics"/>.
    /// </summary>
    /// <remarks>
    /// A <see cref="Font"/> is created from a <see cref="FontFamily"/> and size,
    /// and can be styled with <see cref="FontStyle"/> flags. Fonts are used with
    /// <see cref="Graphics.DrawString"/> to render text.
    /// </remarks>
    public sealed class Font : IDisposable
    {
        /// <summary>
        /// Initializes a new <see cref="Font"/> with the specified family name and size.
        /// </summary>
        /// <param name="familyName">The name of the font family.</param>
        /// <param name="emSize">The em size of the font in the specified units.</param>
        public Font(string familyName, float emSize)
            : this(new FontFamily(familyName), emSize, FontStyle.Regular, GraphicsUnit.Point, 0, false, familyName)
        {
        }

        /// <summary>
        /// Initializes a new <see cref="Font"/> with the specified family name, size, and style.
        /// </summary>
        /// <param name="familyName">The name of the font family.</param>
        /// <param name="emSize">The em size of the font in the specified units.</param>
        /// <param name="style">The style of the font.</param>
        public Font(string familyName, float emSize, FontStyle style)
            : this(new FontFamily(familyName), emSize, style, GraphicsUnit.Point, 0, false, familyName)
        {
        }

        /// <summary>
        /// Initializes a new <see cref="Font"/> with the specified family name, size, style, and unit.
        /// </summary>
        /// <param name="familyName">The name of the font family.</param>
        /// <param name="emSize">The em size of the font.</param>
        /// <param name="style">The style of the font.</param>
        /// <param name="unit">The unit of measurement for the font size.</param>
        public Font(string familyName, float emSize, FontStyle style, GraphicsUnit unit)
            : this(new FontFamily(familyName), emSize, style, unit, 0, false, familyName)
        {
        }

        /// <summary>
        /// Initializes a new <see cref="Font"/> with the specified font family and size.
        /// </summary>
        /// <param name="family">The font family.</param>
        /// <param name="emSize">The em size of the font.</param>
        public Font(FontFamily family, float emSize)
            : this(family, emSize, FontStyle.Regular, GraphicsUnit.Point, 0, false, GetFamilyName(family))
        {
        }

        /// <summary>
        /// Initializes a new <see cref="Font"/> with the specified font family, size, and style.
        /// </summary>
        /// <param name="family">The font family.</param>
        /// <param name="emSize">The em size of the font.</param>
        /// <param name="style">The style of the font.</param>
        public Font(FontFamily family, float emSize, FontStyle style)
            : this(family, emSize, style, GraphicsUnit.Point, 0, false, GetFamilyName(family))
        {
        }

        /// <summary>
        /// Initializes a new <see cref="Font"/> with the specified font family, size, style, and unit.
        /// </summary>
        /// <param name="family">The font family.</param>
        /// <param name="emSize">The em size of the font.</param>
        /// <param name="style">The style of the font.</param>
        /// <param name="unit">The unit of measurement for the font size.</param>
        public Font(FontFamily family, float emSize, FontStyle style, GraphicsUnit unit)
            : this(family, emSize, style, unit, 0, false, GetFamilyName(family))
        {
        }

        /// <summary>
        /// Initializes a new <see cref="Font"/> with the specified font family, size, style, unit, and GDI character set.
        /// </summary>
        /// <param name="family">The font family.</param>
        /// <param name="emSize">The em size of the font.</param>
        /// <param name="style">The style of the font.</param>
        /// <param name="unit">The unit of measurement for the font size.</param>
        /// <param name="gdiCharSet">The GDI character set.</param>
        public Font(FontFamily family, float emSize, FontStyle style, GraphicsUnit unit, byte gdiCharSet)
            : this(family, emSize, style, unit, gdiCharSet, false, GetFamilyName(family))
        {
        }

        private Font(FontFamily family, float emSize, FontStyle style, GraphicsUnit unit, byte gdiCharSet, bool gdiVerticalFont, string? originalFontName)
        {
            FontFamily = family ?? throw new ArgumentNullException(nameof(family));
            if (emSize <= 0f)
                throw new ArgumentOutOfRangeException(nameof(emSize), "Font size must be positive.");

            Size = emSize;
            Style = style;
            Unit = unit;
            GdiCharSet = gdiCharSet;
            GdiVerticalFont = gdiVerticalFont;
            OriginalFontName = string.IsNullOrWhiteSpace(originalFontName)
                ? FontFamily.Name
                : originalFontName!;
        }

        /// <summary>
        /// Gets the font family used by this font.
        /// </summary>
        public FontFamily FontFamily { get; }

        /// <summary>
        /// Gets the name of this font.
        /// </summary>
        public string Name => FontFamily.Name;

        /// <summary>
        /// Gets the size of this font in the specified units.
        /// </summary>
        public float Size { get; }

        /// <summary>
        /// Gets the unit of measurement for the font size.
        /// </summary>
        public GraphicsUnit Unit { get; }

        /// <summary>
        /// Gets the style of this font.
        /// </summary>
        public FontStyle Style { get; }

        /// <summary>
        /// Gets a value indicating whether this font is bold.
        /// </summary>
        public bool Bold => (Style & FontStyle.Bold) != 0;

        /// <summary>
        /// Gets a value indicating whether this font is italic.
        /// </summary>
        public bool Italic => (Style & FontStyle.Italic) != 0;

        /// <summary>
        /// Gets a value indicating whether this font has underlined text.
        /// </summary>
        public bool Underline => (Style & FontStyle.Underline) != 0;

        /// <summary>
        /// Gets a value indicating whether this font has strikethrough text.
        /// </summary>
        public bool Strikeout => (Style & FontStyle.Strikeout) != 0;

        /// <summary>
        /// Gets the line spacing height of this font in pixels.
        /// </summary>
        public int Height
        {
            get
            {
                int emHeight = Math.Max(1, FontFamily.GetEmHeight(Style));
                float emInPixels = ToPixels(Size, Unit, 96f);
                float emScale = emInPixels / emHeight;
                return (int)Math.Ceiling((FontFamily.GetCellAscent(Style) + FontFamily.GetCellDescent(Style)) * emScale);
            }
        }

        /// <summary>
        /// Gets the size of this font in points.
        /// </summary>
        public float SizeInPoints => ToPoints(Size, Unit, 96f);

        /// <summary>
        /// Gets the GDI character set.
        /// </summary>
        public byte GdiCharSet { get; }

        /// <summary>
        /// Gets a value indicating whether this font uses vertical GDI font metrics.
        /// </summary>
        public bool GdiVerticalFont { get; }

        /// <summary>
        /// Gets the original font name passed to the constructor.
        /// </summary>
        public string OriginalFontName { get; }

        /// <summary>
        /// Creates an exact copy of this font.
        /// </summary>
        /// <returns>A new <see cref="Font"/> with the same family, size, style, and unit.</returns>
        public Font Clone()
            => new Font(FontFamily, Size, Style, Unit, GdiCharSet, GdiVerticalFont, OriginalFontName);

        /// <summary>
        /// Releases all resources used by this font.
        /// </summary>
        public void Dispose()
        {
        }

        /// <summary>
        /// Converts a value from the specified unit to pixels.
        /// </summary>
        private static float ToPixels(float value, GraphicsUnit unit, float dpi)
        {
            switch (unit)
            {
                case GraphicsUnit.Pixel:
                    return value;
                case GraphicsUnit.Point:
                    return value * dpi / 72f;
                case GraphicsUnit.Inch:
                    return value * dpi;
                case GraphicsUnit.Document:
                    return value * dpi / 300f;
                case GraphicsUnit.Millimeter:
                    return value * dpi / 25.4f;
                default:
                    return value;
            }
        }

        /// <summary>
        /// Converts a value from the specified unit to points.
        /// </summary>
        private static float ToPoints(float value, GraphicsUnit unit, float dpi)
        {
            switch (unit)
            {
                case GraphicsUnit.Pixel:
                    return value * 72f / dpi;
                case GraphicsUnit.Point:
                    return value;
                case GraphicsUnit.Inch:
                    return value * 72f;
                case GraphicsUnit.Document:
                    return value * 72f / 300f;
                case GraphicsUnit.Millimeter:
                    return value * 72f / 25.4f;
                default:
                    return value;
            }
        }

        private static string GetFamilyName(FontFamily family)
        {
            if (family == null)
                throw new ArgumentNullException(nameof(family));
            return family.Name;
        }
    }
}