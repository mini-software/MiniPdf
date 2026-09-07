using System.Collections.Generic;
using MiniSoftware.Drawing.Enums;
using MiniSoftware.Drawing.Font.Core;
using FontStyle = MiniSoftware.Drawing.Enums.FontStyle;

namespace MiniSoftware.Drawing.Text
{
    /// <summary>
    /// Caches per-(FontFamily, FontStyle) IFont lookups and per-code-point
    /// coverage to avoid re-querying the cmap for every character on every line.
    /// Constructed per DrawString/MeasureString call and discarded.
    /// </summary>
    internal sealed class FontFallbackResolver
    {
        private readonly FontFamily _primaryFamily;
        private readonly FontStyle _primaryStyle;
        private readonly FontFallbackChain? _chain;
        private readonly bool _fallbackEnabled;

        private readonly Dictionary<(FontFamily, FontStyle), IFont?> _fontFaceCache
            = new Dictionary<(FontFamily, FontStyle), IFont?>();
        private readonly Dictionary<(FontFamily, uint), bool> _coverageCache
            = new Dictionary<(FontFamily, uint), bool>();

        internal FontFallbackResolver(
            FontFamily primaryFamily,
            FontStyle primaryStyle,
            FontFallbackChain? chain,
            bool fallbackEnabled)
        {
            _primaryFamily = primaryFamily;
            _primaryStyle = primaryStyle;
            _chain = chain;
            _fallbackEnabled = fallbackEnabled;
        }

        /// <summary>
        /// Returns the <see cref="IFont"/> (primary or fallback) that provides a
        /// glyph for <paramref name="codePoint"/>. When fallback is disabled or
        /// no fallback font has the glyph, returns the primary IFont (or null
        /// if the primary has no face for the style).
        /// </summary>
        internal IFont? Resolve(uint codePoint)
        {
            if (HasGlyph(_primaryFamily, _primaryStyle, codePoint))
                return GetFontFace(_primaryFamily, _primaryStyle);

            if (!_fallbackEnabled || _chain == null)
                return GetFontFace(_primaryFamily, _primaryStyle);

            var fallbackFamily = _chain.Resolve(codePoint, _primaryStyle);
            if (fallbackFamily != null)
                return GetFontFace(fallbackFamily, _primaryStyle);

            return GetFontFace(_primaryFamily, _primaryStyle);
        }

        /// <summary>
        /// Returns the <see cref="FontFamily"/> that <see cref="Resolve"/> would
        /// use for <paramref name="codePoint"/>. Used by RunSegmenter to detect
        /// font-change boundaries without allocating an IFont.
        /// </summary>
        internal FontFamily ResolveFamily(uint codePoint)
        {
            if (HasGlyph(_primaryFamily, _primaryStyle, codePoint))
                return _primaryFamily;

            if (!_fallbackEnabled || _chain == null)
                return _primaryFamily;

            var fallbackFamily = _chain.Resolve(codePoint, _primaryStyle);
            return fallbackFamily ?? _primaryFamily;
        }

        private IFont? GetFontFace(FontFamily family, FontStyle style)
        {
            var key = (family, style);
            if (!_fontFaceCache.TryGetValue(key, out var face))
            {
                face = family.GetFontFace(style);
                _fontFaceCache[key] = face;
            }
            return face;
        }

        private bool HasGlyph(FontFamily family, FontStyle style, uint codePoint)
        {
            var key = (family, codePoint);
            if (_coverageCache.TryGetValue(key, out var has))
                return has;

            var face = GetFontFace(family, style);
            has = face != null && face.Encoding.HasGlyph(codePoint);
            _coverageCache[key] = has;
            return has;
        }
    }
}