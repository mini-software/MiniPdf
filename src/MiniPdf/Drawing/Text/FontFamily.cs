using System;
using System.Collections.Generic;
using MiniSoftware.Drawing.Enums;
using MiniSoftware.Drawing.Font.Core;

namespace MiniSoftware.Drawing.Text
{
    /// <summary>
    /// Represents a font family used for drawing text.
    /// </summary>
    /// <remarks>
    /// A <see cref="FontFamily"/> defines a set of fonts that share the same family name
    /// but differ in style (bold, italic, etc.). Use <see cref="GenericSerif"/>,
    /// <see cref="GenericSansSerif"/>, or <see cref="GenericMonospace"/> for generic families.
    /// </remarks>
    public sealed class FontFamily : IDisposable
    {
        private readonly FontFamilyData _data;

        /// <summary>
        /// Initializes a new <see cref="FontFamily"/> with the specified font family name.
        /// </summary>
        /// <param name="name">The name of the font family.</param>
        /// <remarks>
        /// The name is first looked up in the process-wide
        /// <see cref="InstalledFontCollection"/>. When a matching installed family is
        /// found it is used (with real metrics and glyph outlines). When no installed
        /// family matches, a fallback with guessed metrics is created so that the
        /// instance is still usable for measurement.
        /// </remarks>
        public FontFamily(string name)
            : this(ResolveFamilyData(name))
        {
        }

        /// <summary>
        /// Resolves a family name to <see cref="FontFamilyData"/> by consulting the
        /// installed system font collection first and falling back to guessed metrics.
        /// </summary>
        private static FontFamilyData ResolveFamilyData(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Font family name must not be empty.", nameof(name));

            FontFamily? installed = InstalledFontCollection.Default.FindFamily(name);
            if (installed != null)
                return installed.CloneData();

            return FontFamilyData.CreateFallback(name);
        }

        /// <summary>
        /// Initializes a new <see cref="FontFamily"/> with the specified generic font family.
        /// </summary>
        /// <param name="genericFamily">The generic font family.</param>
        public FontFamily(GenericFontFamilies genericFamily)
            : this(FontFamilyData.CreateGeneric(genericFamily))
        {
        }

        internal FontFamily(FontFamilyData data)
        {
            _data = data ?? throw new ArgumentNullException(nameof(data));
        }

        /// <summary>
        /// Gets a <see cref="FontFamily"/> representing the serif font family.
        /// </summary>
        public static FontFamily GenericSerif => new FontFamily(GenericFontFamilies.Serif);

        /// <summary>
        /// Gets a <see cref="FontFamily"/> representing the sans-serif font family.
        /// </summary>
        public static FontFamily GenericSansSerif => new FontFamily(GenericFontFamilies.SansSerif);

        /// <summary>
        /// Gets a <see cref="FontFamily"/> representing the monospace font family.
        /// </summary>
        public static FontFamily GenericMonospace => new FontFamily(GenericFontFamilies.Monospace);

        /// <summary>
        /// Gets the name of this font family.
        /// </summary>
        public string Name => _data.Name;

        /// <summary>
        /// Gets the em height of the font in design units.
        /// </summary>
        /// <param name="style">The font style.</param>
        /// <returns>The em height in design units.</returns>
        public int GetEmHeight(Enums.FontStyle style) => RoundMetric(_data.GetMetrics(style).UnitsPerEm, 2048);

        /// <summary>
        /// Gets the cell ascent of the font in design units.
        /// </summary>
        /// <param name="style">The font style.</param>
        /// <returns>The cell ascent in design units.</returns>
        public int GetCellAscent(Enums.FontStyle style) => RoundMetric(_data.GetMetrics(style).Ascender, 1854);

        /// <summary>
        /// Gets the cell descent of the font in design units.
        /// </summary>
        /// <param name="style">The font style.</param>
        /// <returns>The cell descent in design units.</returns>
        public int GetCellDescent(Enums.FontStyle style)
        {
            var metrics = _data.GetMetrics(style);
            double descent = metrics.Descender <= 0 ? -metrics.Descender : metrics.Descender;
            return RoundMetric(descent, 434);
        }

        /// <summary>
        /// Gets the line spacing of the font in design units.
        /// </summary>
        /// <param name="style">The font style.</param>
        /// <returns>The line spacing in design units.</returns>
        public int GetLineSpacing(Enums.FontStyle style)
        {
            var metrics = _data.GetMetrics(style);
            double descent = metrics.Descender <= 0 ? -metrics.Descender : metrics.Descender;
            return RoundMetric(metrics.Ascender + descent + metrics.LineGap, 2048);
        }

        /// <summary>
        /// Gets a value indicating whether the specified font style is available.
        /// </summary>
        /// <param name="style">The font style to check.</param>
        /// <returns><c>true</c> if the style is available; otherwise, <c>false</c>.</returns>
        public bool IsStyleAvailable(Enums.FontStyle style) => _data.IsStyleAvailable(style);

        /// <summary>
        /// Releases all resources used by this font family.
        /// </summary>
        public void Dispose()
        {
        }

        internal FontMetricsData GetMetricsData(Enums.FontStyle style) => _data.GetMetrics(style);

        internal FontFamilyData CloneData() => _data.Clone();

        internal void MergeFrom(FontFamilyData other) => _data.MergeFrom(other);

        internal IFont? GetFontFace(Enums.FontStyle style) => _data.GetFontFace(style);

        private static int RoundMetric(double value, int fallback)
        {
            if (value <= 0)
                return fallback;
            return (int)Math.Ceiling(value);
        }
    }

    internal sealed class FontFamilyData
    {
        private readonly Dictionary<Enums.FontStyle, FontMetricsData> _metricsByStyle;
        private readonly Dictionary<Enums.FontStyle, IFont>           _fontsByStyle = new Dictionary<Enums.FontStyle, IFont>();

        public FontFamilyData(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Font family name must not be empty.", nameof(name));

            Name = name;
            _metricsByStyle = new Dictionary<Enums.FontStyle, FontMetricsData>();
        }

        public string Name { get; }

        public static FontFamilyData CreateGeneric(GenericFontFamilies genericFamily)
        {
            string name;
            FontMetricsData metrics;

            switch (genericFamily)
            {
                case GenericFontFamilies.Serif:
                    name = "Serif";
                    metrics = FontMetricsData.GenericSerif;
                    break;
                case GenericFontFamilies.Monospace:
                    name = "Monospace";
                    metrics = FontMetricsData.GenericMonospace;
                    break;
                default:
                    name = "Sans Serif";
                    metrics = FontMetricsData.GenericSansSerif;
                    break;
            }

            var data = new FontFamilyData(name);
            data.AddDefaultStyles(metrics);
            return data;
        }

        public static FontFamilyData CreateFallback(string name)
        {
            var metrics = GuessGenericMetrics(name);
            var data = new FontFamilyData(name);
            data.AddDefaultStyles(metrics);
            return data;
        }

        public FontFamilyData Clone()
        {
            var clone = new FontFamilyData(Name);
            foreach (var pair in _metricsByStyle)
                clone._metricsByStyle[pair.Key] = pair.Value;
            foreach (var pair in _fontsByStyle)
                clone._fontsByStyle[pair.Key] = pair.Value;
            return clone;
        }

        public void MergeFrom(FontFamilyData other)
        {
            if (other == null)
                throw new ArgumentNullException(nameof(other));

            foreach (var pair in other._metricsByStyle)
                _metricsByStyle[pair.Key] = pair.Value;
            foreach (var pair in other._fontsByStyle)
                _fontsByStyle[pair.Key] = pair.Value;
        }

        public bool IsStyleAvailable(Enums.FontStyle style) => _metricsByStyle.ContainsKey(NormalizeStyle(style));

        public void AddStyle(Enums.FontStyle style, FontMetricsData metrics) => _metricsByStyle[NormalizeStyle(style)] = metrics;

        public void AddFontFace(Enums.FontStyle style, IFont font) => _fontsByStyle[NormalizeStyle(style)] = font;

        public IFont? GetFontFace(Enums.FontStyle style)
        {
            if (_fontsByStyle.TryGetValue(NormalizeStyle(style), out var face))
                return face;
            if (_fontsByStyle.TryGetValue(Enums.FontStyle.Regular, out face))
                return face;
            foreach (var pair in _fontsByStyle)
                return pair.Value;
            return null;
        }

        public FontMetricsData GetMetrics(Enums.FontStyle style)
        {
            if (_metricsByStyle.TryGetValue(NormalizeStyle(style), out var metrics))
                return metrics;

            if (_metricsByStyle.TryGetValue(Enums.FontStyle.Regular, out metrics))
                return metrics;

            foreach (var pair in _metricsByStyle)
                return pair.Value;

            return GuessGenericMetrics(Name);
        }

        private void AddDefaultStyles(FontMetricsData metrics)
        {
            _metricsByStyle[Enums.FontStyle.Regular] = metrics;
            _metricsByStyle[Enums.FontStyle.Bold] = metrics;
            _metricsByStyle[Enums.FontStyle.Italic] = metrics;
            _metricsByStyle[Enums.FontStyle.Bold | Enums.FontStyle.Italic] = metrics;
        }

        private static FontMetricsData GuessGenericMetrics(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return FontMetricsData.GenericSansSerif;

            string normalized = name.Trim();
            if (normalized.IndexOf("mono", StringComparison.OrdinalIgnoreCase) >= 0
                || normalized.IndexOf("courier", StringComparison.OrdinalIgnoreCase) >= 0
                || normalized.IndexOf("console", StringComparison.OrdinalIgnoreCase) >= 0)
                return FontMetricsData.GenericMonospace;

            if (normalized.IndexOf("serif", StringComparison.OrdinalIgnoreCase) >= 0
                && normalized.IndexOf("sans", StringComparison.OrdinalIgnoreCase) < 0)
                return FontMetricsData.GenericSerif;

            return FontMetricsData.GenericSansSerif;
        }

        private static Enums.FontStyle NormalizeStyle(Enums.FontStyle style)
        {
            Enums.FontStyle normalized = style & (Enums.FontStyle.Bold | Enums.FontStyle.Italic);
            return normalized == 0 ? Enums.FontStyle.Regular : normalized;
        }
    }

    internal readonly struct FontMetricsData
    {
        public static readonly FontMetricsData GenericSansSerif = new FontMetricsData(2048, 1854, -434, 67, -100, 50, 512, 50);
        public static readonly FontMetricsData GenericSerif = new FontMetricsData(2048, 1825, -443, 90, -100, 50, 512, 50);
        public static readonly FontMetricsData GenericMonospace = new FontMetricsData(2048, 1705, -443, 0, -100, 50, 512, 50);

        public FontMetricsData(double unitsPerEm, double ascender, double descender, double lineGap,
            double underlinePosition, double underlineThickness, double strikeoutPosition, double strikeoutSize)
        {
            UnitsPerEm = unitsPerEm;
            Ascender = ascender;
            Descender = descender;
            LineGap = lineGap;
            UnderlinePosition = underlinePosition;
            UnderlineThickness = underlineThickness;
            StrikeoutPosition = strikeoutPosition;
            StrikeoutSize = strikeoutSize;
        }

        public double UnitsPerEm { get; }
        public double Ascender { get; }
        public double Descender { get; }
        public double LineGap { get; }
        public double UnderlinePosition { get; }
        public double UnderlineThickness { get; }
        public double StrikeoutPosition { get; }
        public double StrikeoutSize { get; }

        public static FontMetricsData FromMetrics(IFontMetrics metrics)
        {
            if (metrics == null)
                throw new ArgumentNullException(nameof(metrics));

            return new FontMetricsData(
                metrics.UnitsPerEM,
                metrics.Ascender,
                metrics.Descender,
                metrics.LineGap,
                metrics.UnderlinePosition,
                metrics.UnderlineThickness,
                metrics.StrikeoutPosition,
                metrics.StrikeoutSize);
        }
    }
}
