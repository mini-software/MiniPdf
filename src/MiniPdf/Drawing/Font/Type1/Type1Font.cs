using System;
using System.Collections.Generic;
using System.IO;
using MiniSoftware.Drawing.Font.Core;
using MiniSoftware.Drawing.Font.Encoding;
using MiniSoftware.Drawing.Font.Glyphs;
using MiniSoftware.Drawing.Font.Rendering;

namespace MiniSoftware.Drawing.Font.Type1
{
    /// <summary>
    /// Represents a loaded Type 1 font face (PFB or PFA).
    /// Charstrings are decoded via <see cref="Type1CharStringInterpreter"/>.
    /// </summary>
    public sealed class Type1Font : IFont
    {
        // ── Private state ─────────────────────────────────────────────────────

        internal readonly Type1Data    Data;
        private readonly  FontMetrics  _metrics;
        private readonly  IFontEncoding _encoding;
        private readonly  Type1GlyphAccessor _glyphAccessor;

        // ── Internal name→index map ───────────────────────────────────────────

        // Maps glyph name to its index in GlyphOrder (used for encoding lookup)
        internal readonly IReadOnlyDictionary<string, int> NameToIndex;

        // ── Constructor ───────────────────────────────────────────────────────

        internal Type1Font(Type1Data data)
        {
            Data = data ?? throw new ArgumentNullException(nameof(data));

            // Build name→index map once
            var nameToIdx = new Dictionary<string, int>(
                data.GlyphOrder.Length, StringComparer.Ordinal);
            for (int i = 0; i < data.GlyphOrder.Length; i++)
                nameToIdx[data.GlyphOrder[i]] = i;
            NameToIndex = nameToIdx;

            _metrics       = BuildMetrics(data);
            _encoding      = new Type1FontEncoding(data, nameToIdx);
            _glyphAccessor = new Type1GlyphAccessor(data, this);
        }

        // ── IFont ─────────────────────────────────────────────────────────────

        /// <inheritdoc/>
        public string FontName   => Data.FontName.Length > 0 ? Data.FontName : "(Type1)";

        /// <inheritdoc/>
        public string FontFamily => StripStyleSuffix(FontName);

        /// <inheritdoc/>
        public int NumGlyphs => Data.CharStrings.Count;

        /// <inheritdoc/>
        public FontFaceStyle Style => FontFaceStyle.Regular; // Type 1 has no macStyle bits

        /// <inheritdoc/>
        public IFontMetrics Metrics => _metrics;

        /// <inheritdoc/>
        public IFontEncoding Encoding => _encoding;

        /// <inheritdoc/>
        public IGlyphAccessor GlyphAccessor => _glyphAccessor;

        /// <inheritdoc/>
        /// <remarks>Type 1 serialisation is outside the scope of this library.</remarks>
        public void Save(Stream stream)
        {
            if (stream is null) throw new ArgumentNullException(nameof(stream));
            throw new NotSupportedException("Type1Font serialisation is not supported.");
        }

        /// <inheritdoc/>
        public void Save(string filePath)
        {
            if (filePath is null) throw new ArgumentNullException(nameof(filePath));
            using var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write,
                                          FileShare.None, bufferSize: 4096);
            Save(fs);
        }

        /// <summary>
        /// Returns a string representation of this Type 1 font.
        /// </summary>
        public override string ToString() =>
            $"Type1Font(\"{FontName}\", {NumGlyphs} glyphs)";

        // ── Helpers ───────────────────────────────────────────────────────────

        private static FontMetrics BuildMetrics(Type1Data data)
        {
            // Type 1 fonts use a 1000-unit em square by default.
            // /FontBBox = [xMin yMin xMax yMax]
            double yMax = data.FontBBox.Length > 3 ? data.FontBBox[3] : 800;
            double yMin = data.FontBBox.Length > 1 ? data.FontBBox[1] : -200;

            double upm = (yMax > 0 && yMin < yMax) ? yMax - yMin : 1000.0;
            if (upm <= 0) upm = 1000.0;

            var m = new FontMetrics(gid =>
            {
                if (!(gid is GlyphUInt32Id uid)) return 0.0;
                int idx = (int)uid.Value;
                if (idx < 0 || idx >= data.GlyphOrder.Length) return 0.0;
                string name = data.GlyphOrder[idx];
                if (!data.CharStrings.TryGetValue(name, out byte[]? cs)) return 0.0;
                var interp = new Type1CharStringInterpreter();
                return interp.Interpret(cs, NopPainter.Instance, data.Subrs, data.CharStrings);
            })
            {
                Ascender      = yMax,
                Descender     = yMin,
                LineGap       = 0,
                UnitsPerEM    = upm,
                TypoAscender  = yMax,
                TypoDescender = yMin,
            };
            return m;
        }

        private static string StripStyleSuffix(string name)
        {
            foreach (string suffix in new[] { "-Bold", "-Italic", "-BoldItalic", "-Regular" })
            {
                if (name.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
                    return name.Substring(0, name.Length - suffix.Length);
            }
            return name;
        }

        // ── No-op painter (for width measurement only) ─────────────────────────

        private sealed class NopPainter : IGlyphOutlinePainter
        {
            internal static readonly NopPainter Instance = new NopPainter();
            public void MoveTo(MoveTo cmd)    { }
            public void LineTo(LineTo cmd)    { }
            public void CurveTo(CurveTo cmd)  { }
            public void ClosePath()           { }
        }
    }

    // ── Type1GlyphAccessor ────────────────────────────────────────────────────

    internal sealed class Type1GlyphAccessor : IGlyphAccessor
    {
        private readonly Type1Data _data;
        private readonly Type1Font _font;

        internal Type1GlyphAccessor(Type1Data data, Type1Font font)
        {
            _data = data;
            _font = font;
        }

        public Glyph? GetGlyphById(GlyphId gid)
        {
            if (!(gid is GlyphUInt32Id uid)) return null;
            int idx = (int)uid.Value;
            if (idx < 0 || idx >= _data.GlyphOrder.Length) return null;

            string name = _data.GlyphOrder[idx];
            if (!_data.CharStrings.TryGetValue(name, out byte[]? cs)) return null;

            var recording = new BBoxPainter();
            var interp    = new Type1CharStringInterpreter();
            double width  = interp.Interpret(cs, recording, _data.Subrs, _data.CharStrings);

            return new Glyph(gid, recording.GetBBox(), width);
        }

        public Glyph? GetGlyphByIndex(uint index)
            => GetGlyphById(new GlyphUInt32Id(index));

        // ── BBox recording painter ────────────────────────────────────────────

        private sealed class BBoxPainter : IGlyphOutlinePainter
        {
            private double _xMin = double.MaxValue, _yMin = double.MaxValue;
            private double _xMax = double.MinValue, _yMax = double.MinValue;
            private bool   _has;

            private void Track(double x, double y)
            {
                if (x < _xMin) _xMin = x;
                if (x > _xMax) _xMax = x;
                if (y < _yMin) _yMin = y;
                if (y > _yMax) _yMax = y;
                _has = true;
            }

            public void MoveTo(MoveTo c)  => Track(c.X, c.Y);
            public void LineTo(LineTo c)  => Track(c.X, c.Y);
            public void CurveTo(CurveTo c)
            {
                Track(c.X1, c.Y1);
                Track(c.X2, c.Y2);
                Track(c.X3, c.Y3);
            }
            public void ClosePath() { }

            public GlyphBoundingBox GetBBox()
                => _has
                    ? new GlyphBoundingBox(_xMin, _yMin, _xMax, _yMax)
                    : new GlyphBoundingBox(0, 0, 0, 0);
        }
    }

    // ── Type1FontEncoding ─────────────────────────────────────────────────────

    internal sealed class Type1FontEncoding : IFontEncoding
    {
        // Unicode codepoint → glyph name (compact Adobe Glyph List subset)
        private static readonly Dictionary<uint, string> AglCodepointToName =
            BuildAgl();

        private readonly string[]                  _encoding;  // slot 0-255 → glyph name
        private readonly IReadOnlyDictionary<string, int> _nameToIndex;

        internal Type1FontEncoding(
            Type1Data data,
            IReadOnlyDictionary<string, int> nameToIndex)
        {
            _encoding    = data.Encoding;
            _nameToIndex = nameToIndex;
        }

        public GlyphId? DecodeToGid(uint codePoint)
        {
            // 1. Try Adobe Glyph List: codepoint → glyph name → index
            if (AglCodepointToName.TryGetValue(codePoint, out string? aglName)
                && _nameToIndex.TryGetValue(aglName, out int aglIdx))
                return new GlyphUInt32Id((uint)aglIdx);

            // 2. Try the font's own encoding array for single-byte codepoints
            if (codePoint < 256)
            {
                string slotName = _encoding[codePoint];
                if (slotName != ".notdef" && _nameToIndex.TryGetValue(slotName, out int slotIdx))
                    return new GlyphUInt32Id((uint)slotIdx);
            }

            return GlyphUInt32Id.NotDefId;
        }

        public GlyphId? DecodeToGid(char ch) => DecodeToGid((uint)ch);

        public bool TryGetGlyph(uint codePoint, out uint glyphId)
        {
            var gid = DecodeToGid(codePoint);
            if (gid is Glyphs.GlyphUInt32Id u32 && u32.Value != 0)
            {
                glyphId = u32.Value;
                return true;
            }
            glyphId = 0;
            return false;
        }

        public bool HasGlyph(uint codePoint) => TryGetGlyph(codePoint, out _);

        // ── Adobe Glyph List (compact subset: all ASCII + full Latin-1/2) ─────

        private static Dictionary<uint, string> BuildAgl()
        {
            var d = new Dictionary<uint, string>();

            // ASCII printable
            d[0x20] = "space";       d[0x21] = "exclam";      d[0x22] = "quotedbl";
            d[0x23] = "numbersign";  d[0x24] = "dollar";      d[0x25] = "percent";
            d[0x26] = "ampersand";   d[0x27] = "quotesingle"; d[0x28] = "parenleft";
            d[0x29] = "parenright";  d[0x2A] = "asterisk";    d[0x2B] = "plus";
            d[0x2C] = "comma";       d[0x2D] = "hyphen";      d[0x2E] = "period";
            d[0x2F] = "slash";
            d[0x30] = "zero";        d[0x31] = "one";         d[0x32] = "two";
            d[0x33] = "three";       d[0x34] = "four";        d[0x35] = "five";
            d[0x36] = "six";         d[0x37] = "seven";       d[0x38] = "eight";
            d[0x39] = "nine";
            d[0x3A] = "colon";       d[0x3B] = "semicolon";   d[0x3C] = "less";
            d[0x3D] = "equal";       d[0x3E] = "greater";     d[0x3F] = "question";
            d[0x40] = "at";

            // A–Z
            for (uint i = 0; i < 26; i++)
                d[0x41 + i] = ((char)('A' + i)).ToString();

            d[0x5B] = "bracketleft"; d[0x5C] = "backslash";   d[0x5D] = "bracketright";
            d[0x5E] = "asciicircum"; d[0x5F] = "underscore";  d[0x60] = "grave";

            // a–z
            for (uint i = 0; i < 26; i++)
                d[0x61 + i] = ((char)('a' + i)).ToString();

            d[0x7B] = "braceleft";   d[0x7C] = "bar";         d[0x7D] = "braceright";
            d[0x7E] = "asciitilde";

            // Latin-1 supplement (U+00A0–U+00FF)
            d[0x00A0] = "nbspace";         d[0x00A1] = "exclamdown";
            d[0x00A2] = "cent";            d[0x00A3] = "sterling";
            d[0x00A4] = "currency";        d[0x00A5] = "yen";
            d[0x00A6] = "brokenbar";       d[0x00A7] = "section";
            d[0x00A8] = "dieresis";        d[0x00A9] = "copyright";
            d[0x00AA] = "ordfeminine";     d[0x00AB] = "guillemotleft";
            d[0x00AC] = "logicalnot";      d[0x00AD] = "softhyphen";
            d[0x00AE] = "registered";      d[0x00AF] = "macron";
            d[0x00B0] = "degree";          d[0x00B1] = "plusminus";
            d[0x00B2] = "twosuperior";     d[0x00B3] = "threesuperior";
            d[0x00B4] = "acute";           d[0x00B5] = "mu";
            d[0x00B6] = "paragraph";       d[0x00B7] = "periodcentered";
            d[0x00B8] = "cedilla";         d[0x00B9] = "onesuperior";
            d[0x00BA] = "ordmasculine";    d[0x00BB] = "guillemotright";
            d[0x00BC] = "onequarter";      d[0x00BD] = "onehalf";
            d[0x00BE] = "threequarters";   d[0x00BF] = "questiondown";
            d[0x00C0] = "Agrave";          d[0x00C1] = "Aacute";
            d[0x00C2] = "Acircumflex";     d[0x00C3] = "Atilde";
            d[0x00C4] = "Adieresis";       d[0x00C5] = "Aring";
            d[0x00C6] = "AE";              d[0x00C7] = "Ccedilla";
            d[0x00C8] = "Egrave";          d[0x00C9] = "Eacute";
            d[0x00CA] = "Ecircumflex";     d[0x00CB] = "Edieresis";
            d[0x00CC] = "Igrave";          d[0x00CD] = "Iacute";
            d[0x00CE] = "Icircumflex";     d[0x00CF] = "Idieresis";
            d[0x00D0] = "Eth";             d[0x00D1] = "Ntilde";
            d[0x00D2] = "Ograve";          d[0x00D3] = "Oacute";
            d[0x00D4] = "Ocircumflex";     d[0x00D5] = "Otilde";
            d[0x00D6] = "Odieresis";       d[0x00D7] = "multiply";
            d[0x00D8] = "Oslash";          d[0x00D9] = "Ugrave";
            d[0x00DA] = "Uacute";          d[0x00DB] = "Ucircumflex";
            d[0x00DC] = "Udieresis";       d[0x00DD] = "Yacute";
            d[0x00DE] = "Thorn";           d[0x00DF] = "germandbls";
            d[0x00E0] = "agrave";          d[0x00E1] = "aacute";
            d[0x00E2] = "acircumflex";     d[0x00E3] = "atilde";
            d[0x00E4] = "adieresis";       d[0x00E5] = "aring";
            d[0x00E6] = "ae";              d[0x00E7] = "ccedilla";
            d[0x00E8] = "egrave";          d[0x00E9] = "eacute";
            d[0x00EA] = "ecircumflex";     d[0x00EB] = "edieresis";
            d[0x00EC] = "igrave";          d[0x00ED] = "iacute";
            d[0x00EE] = "icircumflex";     d[0x00EF] = "idieresis";
            d[0x00F0] = "eth";             d[0x00F1] = "ntilde";
            d[0x00F2] = "ograve";          d[0x00F3] = "oacute";
            d[0x00F4] = "ocircumflex";     d[0x00F5] = "otilde";
            d[0x00F6] = "odieresis";       d[0x00F7] = "divide";
            d[0x00F8] = "oslash";          d[0x00F9] = "ugrave";
            d[0x00FA] = "uacute";          d[0x00FB] = "ucircumflex";
            d[0x00FC] = "udieresis";       d[0x00FD] = "yacute";
            d[0x00FE] = "thorn";           d[0x00FF] = "ydieresis";

            // Selected Latin Extended-A / special typographic chars
            d[0x0131] = "dotlessi";  d[0x0141] = "Lslash";   d[0x0142] = "lslash";
            d[0x0152] = "OE";        d[0x0153] = "oe";        d[0x0160] = "Scaron";
            d[0x0161] = "scaron";    d[0x0178] = "Ydieresis"; d[0x017D] = "Zcaron";
            d[0x017E] = "zcaron";    d[0x0192] = "florin";

            // Punctuation / typographic
            d[0x2013] = "endash";     d[0x2014] = "emdash";
            d[0x2018] = "quoteleft";  d[0x2019] = "quoteright";
            d[0x201A] = "quotesinglbase";
            d[0x201C] = "quotedblleft"; d[0x201D] = "quotedblright";
            d[0x201E] = "quotedblbase";
            d[0x2020] = "dagger";     d[0x2021] = "daggerdbl";
            d[0x2022] = "bullet";     d[0x2026] = "ellipsis";
            d[0x2030] = "perthousand";
            d[0x2039] = "guilsinglleft"; d[0x203A] = "guilsinglright";
            d[0x2044] = "fraction";
            d[0x20AC] = "Euro";       d[0x2122] = "trademark";
            d[0x2202] = "partialdiff";
            d[0x2206] = "Delta";      d[0x220F] = "product";
            d[0x2211] = "summation";  d[0x2212] = "minus";
            d[0x221A] = "radical";    d[0x221E] = "infinity";
            d[0x222B] = "integral";   d[0x2248] = "approxequal";
            d[0x2260] = "notequal";   d[0x2264] = "lessequal";
            d[0x2265] = "greaterequal"; d[0x25CA] = "lozenge";
            d[0xFB01] = "fi";         d[0xFB02] = "fl";

            return d;
        }
    }
}
