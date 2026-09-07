using System;

namespace MiniSoftware.Drawing.Text.Bidi
{
    /// <summary>
    /// Lookup table for Unicode bidirectional character types (UAX #9 §4.2).
    /// Encoded as a sorted range table covering the major script ranges.
    /// Code points not listed default to <see cref="BidiCharacterType.L"/>
    /// (Left-to-Right), which is correct for unassigned code points per UAX #9.
    /// </summary>
    internal static class BidiCharacterTable
    {
        // Sorted by start code point. Binary-searched at runtime.
        private static readonly (uint Start, uint End, BidiCharacterType Type)[] s_ranges;

        static BidiCharacterTable()
        {
            s_ranges = new (uint Start, uint End, BidiCharacterType Type)[]
            {
            // ── Control / format / whitespace ──────────────────────────────
            (0x0009, 0x0009, BidiCharacterType.S),    // TAB
            (0x000A, 0x000A, BidiCharacterType.B),    // LF
            (0x000B, 0x000B, BidiCharacterType.S),    // VT
            (0x000C, 0x000C, BidiCharacterType.B),    // FF
            (0x000D, 0x000D, BidiCharacterType.B),    // CR
            (0x001C, 0x001E, BidiCharacterType.B),    // File/Group/Record Separator
            (0x001F, 0x001F, BidiCharacterType.S),    // Unit Separator
            (0x0020, 0x0020, BidiCharacterType.WS),   // SPACE
            (0x0085, 0x0085, BidiCharacterType.B),    // NEL
            (0x00A0, 0x00A0, BidiCharacterType.CS),   // NBSP
            (0x00A1, 0x00A1, BidiCharacterType.ON),   // ¡
            (0x00A2, 0x00A5, BidiCharacterType.ET),   // ¢ £ ¤ ¥
            (0x00A6, 0x00A9, BidiCharacterType.ON),   // ¦ § ¨ ©
            (0x00AA, 0x00AA, BidiCharacterType.L),    // ª (feminine ordinal)
            (0x00AB, 0x00AC, BidiCharacterType.ON),   // « ¬
            (0x00AD, 0x00AD, BidiCharacterType.BN),   // SOFT HYPHEN
            (0x00AE, 0x00AF, BidiCharacterType.ON),   // ® ¯
            (0x00B0, 0x00B1, BidiCharacterType.ET),   // ° ±
            (0x00B2, 0x00B3, BidiCharacterType.EN),   // ² ³
            (0x00B4, 0x00B4, BidiCharacterType.ON),   // ´
            (0x00B5, 0x00B5, BidiCharacterType.L),    // µ
            (0x00B6, 0x00B8, BidiCharacterType.ON),   // ¶ · ¸
            (0x00B9, 0x00B9, BidiCharacterType.EN),   // ¹
            (0x00BA, 0x00BA, BidiCharacterType.L),    // º (masculine ordinal)
            (0x00BB, 0x00BF, BidiCharacterType.ON),   // » ¼ ½ ¾ ¿
            (0x00D7, 0x00D7, BidiCharacterType.ON),   // ×
            (0x00F7, 0x00F7, BidiCharacterType.ON),   // ÷

            // ── European digits ────────────────────────────────────────────
            (0x0030, 0x0039, BidiCharacterType.EN),   // 0-9
            (0x002B, 0x002B, BidiCharacterType.ES),   // +
            (0x002D, 0x002D, BidiCharacterType.ES),   // -
            (0x002E, 0x002E, BidiCharacterType.CS),   // .
            (0x002F, 0x002F, BidiCharacterType.CS),   // /
            (0x0021, 0x0022, BidiCharacterType.ON),   // ! "
            (0x0023, 0x0025, BidiCharacterType.ET),   // # $ %
            (0x0026, 0x0027, BidiCharacterType.ON),   // & '
            (0x0028, 0x0029, BidiCharacterType.ON),   // ( )
            (0x002A, 0x002A, BidiCharacterType.ON),   // *
            (0x002C, 0x002C, BidiCharacterType.CS),   // ,
            (0x003A, 0x003B, BidiCharacterType.CS),   // : ;
            (0x003C, 0x003E, BidiCharacterType.ON),   // < = >
            (0x003F, 0x0040, BidiCharacterType.ON),   // ? @
            (0x005B, 0x005E, BidiCharacterType.ON),   // [ \ ] ^
            (0x0060, 0x0060, BidiCharacterType.ON),   // `
            (0x007B, 0x007E, BidiCharacterType.ON),   // { | } ~

            // ── Superscript/subscript digits ──────────────────────────────
            // ── Arabic ────────────────────────────────────────────────────
            (0x0600, 0x0605, BidiCharacterType.AN),   // Arabic number signs
            (0x0606, 0x0607, BidiCharacterType.ON),   // Arabic operator symbols
            (0x0608, 0x0608, BidiCharacterType.ON),   // Arabic ray
            (0x0609, 0x060A, BidiCharacterType.ET),   // Arabic-Indic per mille, per ten thousand
            (0x060B, 0x060B, BidiCharacterType.AL),   // Arabic afghanistan sign
            (0x060C, 0x060C, BidiCharacterType.CS),   // Arabic comma
            (0x060D, 0x060F, BidiCharacterType.AL),   // Arabic date separator, etc.
            (0x0610, 0x061A, BidiCharacterType.NSM),  // Arabic marks
            (0x061B, 0x061B, BidiCharacterType.ON),   // Arabic semicolon
            (0x061C, 0x061C, BidiCharacterType.AL),   // Arabic letter mark
            (0x061E, 0x061F, BidiCharacterType.ON),   // Arabic question mark
            (0x0620, 0x063F, BidiCharacterType.AL),   // Arabic letters
            (0x0640, 0x0640, BidiCharacterType.AL),   // Arabic tatweel
            (0x0641, 0x064A, BidiCharacterType.AL),   // Arabic letters
            (0x064B, 0x065F, BidiCharacterType.NSM),  // Arabic diacritics
            (0x0660, 0x0669, BidiCharacterType.AN),   // Arabic-Indic digits
            (0x066A, 0x066A, BidiCharacterType.ET),   // Arabic percent
            (0x066B, 0x066C, BidiCharacterType.AN),   // Arabic decimal separator, thousands separator
            (0x066D, 0x066F, BidiCharacterType.AL),   // Arabic five-pointed star etc.
            (0x0670, 0x0670, BidiCharacterType.NSM),  // Arabic superscript alef
            (0x0671, 0x06D5, BidiCharacterType.AL),   // Arabic letters
            (0x06D6, 0x06DC, BidiCharacterType.NSM),  // Arabic marks
            (0x06DD, 0x06DD, BidiCharacterType.AN),   // Arabic end of ayah
            (0x06DE, 0x06DE, BidiCharacterType.ON),   // Arabic start of rub el hizb
            (0x06DF, 0x06E4, BidiCharacterType.NSM),  // Arabic marks
            (0x06E5, 0x06E6, BidiCharacterType.AL),   // Arabic sawal, madda
            (0x06E7, 0x06E8, BidiCharacterType.NSM),  // Arabic marks
            (0x06E9, 0x06E9, BidiCharacterType.ON),   // Arabic place of sajdah
            (0x06EA, 0x06ED, BidiCharacterType.NSM),  // Arabic marks
            (0x06EE, 0x06EF, BidiCharacterType.AL),   // Arabic letter dal with ring, dot
            (0x06F0, 0x06F9, BidiCharacterType.EN),   // Extended Arabic-Indic digits
            (0x06FA, 0x06FF, BidiCharacterType.AL),   // Arabic letters

            // ── Syriac ────────────────────────────────────────────────────
            (0x0700, 0x070D, BidiCharacterType.AL),
            (0x070F, 0x070F, BidiCharacterType.AL),   // Syriac abbreviation mark
            (0x0710, 0x074A, BidiCharacterType.AL),
            (0x074D, 0x074F, BidiCharacterType.AL),

            // ── Arabic Supplement ─────────────────────────────────────────
            (0x0750, 0x077F, BidiCharacterType.AL),

            // ── Thaana ────────────────────────────────────────────────────
            (0x0780, 0x07A5, BidiCharacterType.AL),
            (0x07A6, 0x07B0, BidiCharacterType.NSM),
            (0x07B1, 0x07B1, BidiCharacterType.AL),

            // ── NKo ───────────────────────────────────────────────────────
            (0x07C0, 0x07FA, BidiCharacterType.R),

            // ── Samaritan ─────────────────────────────────────────────────
            (0x0800, 0x082D, BidiCharacterType.AL),
            (0x0830, 0x083E, BidiCharacterType.AL),

            // ── Mandaic ───────────────────────────────────────────────────
            (0x0840, 0x085B, BidiCharacterType.AL),
            (0x085E, 0x085E, BidiCharacterType.AL),

            // ── Arabic Extended-A ─────────────────────────────────────────
            (0x08A0, 0x08B4, BidiCharacterType.AL),
            (0x08B6, 0x08BD, BidiCharacterType.AL),

            // ── Hebrew ────────────────────────────────────────────────────
            (0x0591, 0x05BD, BidiCharacterType.NSM),
            (0x05BE, 0x05BE, BidiCharacterType.R),
            (0x05BF, 0x05BF, BidiCharacterType.NSM),
            (0x05C0, 0x05C0, BidiCharacterType.R),
            (0x05C1, 0x05C2, BidiCharacterType.NSM),
            (0x05C3, 0x05C3, BidiCharacterType.R),
            (0x05C4, 0x05C5, BidiCharacterType.NSM),
            (0x05C6, 0x05C6, BidiCharacterType.R),
            (0x05C7, 0x05C7, BidiCharacterType.NSM),
            (0x05D0, 0x05EA, BidiCharacterType.R),
            (0x05F0, 0x05F4, BidiCharacterType.R),

            // ── Format characters (explicit embedding/override/isolate) ───
            (0x200B, 0x200D, BidiCharacterType.BN),   // ZWSP, ZWNJ, ZWJ
            (0x200E, 0x200E, BidiCharacterType.L),    // LRM
            (0x200F, 0x200F, BidiCharacterType.R),    // RLM
            (0x2028, 0x2028, BidiCharacterType.WS),   // LINE SEPARATOR (treated as B in some impls)
            (0x2029, 0x2029, BidiCharacterType.B),    // PARAGRAPH SEPARATOR
            (0x202A, 0x202A, BidiCharacterType.LRE),
            (0x202B, 0x202B, BidiCharacterType.RLE),
            (0x202C, 0x202C, BidiCharacterType.PDF),
            (0x202D, 0x202D, BidiCharacterType.LRO),
            (0x202E, 0x202E, BidiCharacterType.RLO),
            (0x2060, 0x2064, BidiCharacterType.BN),   // Word joiner etc.
            (0x2066, 0x2066, BidiCharacterType.LRI),
            (0x2067, 0x2067, BidiCharacterType.RLI),
            (0x2068, 0x2068, BidiCharacterType.FSI),
            (0x2069, 0x2069, BidiCharacterType.PDI),
            (0x206A, 0x206F, BidiCharacterType.BN),   // Deprecated format controls

            // ── Whitespace ────────────────────────────────────────────────
            (0x1680, 0x1680, BidiCharacterType.WS),   // OGHAM SPACE MARK
            (0x2000, 0x200A, BidiCharacterType.WS),   // Various spaces
            (0x2028, 0x2028, BidiCharacterType.WS),   // LINE SEPARATOR
            (0x202F, 0x202F, BidiCharacterType.CS),   // NARROW NBSP
            (0x205F, 0x205F, BidiCharacterType.WS),   // MEDIUM MATHEMATICAL SPACE
            (0x3000, 0x3000, BidiCharacterType.WS),   // IDEOGRAPHIC SPACE

            // ── Currency symbols ──────────────────────────────────────────
            (0x09F2, 0x09F3, BidiCharacterType.ET),   // Bengali currency
            (0x0AF1, 0x0AF1, BidiCharacterType.ET),   // Gujarati rupee
            (0x0BF9, 0x0BF9, BidiCharacterType.ET),   // Tamil rupee
            (0x0E3F, 0x0E3F, BidiCharacterType.ET),   // Thai baht
            (0x17DB, 0x17DB, BidiCharacterType.ET),   // Khmer riel
            (0x2030, 0x2034, BidiCharacterType.ET),   // ‰ ‱ ′ ″ ‴
            (0x20A0, 0x20C0, BidiCharacterType.ET),   // Currency symbols
            (0xFE5F, 0xFE6F, BidiCharacterType.ET),   // Small currency symbols

            // ── Hebrew presentation forms ─────────────────────────────────
            (0xFB1D, 0xFB4F, BidiCharacterType.R),

            // ── Arabic presentation forms-A ───────────────────────────────
            (0xFB1E, 0xFB1E, BidiCharacterType.NSM),  // Hebrew point
            (0xFB50, 0xFDFF, BidiCharacterType.AL),

            // ── Arabic presentation forms-B ───────────────────────────────
            (0xFE00, 0xFE0F, BidiCharacterType.NSM),  // Variation selectors
            (0xFE70, 0xFEFE, BidiCharacterType.AL),

            // ── Right-to-left historic scripts ────────────────────────────
            (0x10800, 0x10805, BidiCharacterType.R),  // Cypriot syllabary
            (0x10808, 0x10808, BidiCharacterType.R),
            (0x1080A, 0x10835, BidiCharacterType.R),
            (0x10837, 0x10838, BidiCharacterType.R),
            (0x1083C, 0x1083C, BidiCharacterType.R),
            (0x1083F, 0x1083F, BidiCharacterType.R),
            (0x10900, 0x1091B, BidiCharacterType.R),  // Phoenician
            (0x10920, 0x10939, BidiCharacterType.R),  // Lydian
            (0x10980, 0x109B7, BidiCharacterType.R),  // Meroitic hieroglyphs
            (0x10A00, 0x10A03, BidiCharacterType.R),  // Kharoshthi
            (0x10A05, 0x10A06, BidiCharacterType.R),
            (0x10A0C, 0x10A13, BidiCharacterType.R),
            (0x10A15, 0x10A17, BidiCharacterType.R),
            (0x10A19, 0x10A33, BidiCharacterType.R),
            (0x10A38, 0x10A3A, BidiCharacterType.NSM),
            (0x10A3F, 0x10A3F, BidiCharacterType.NSM),
            (0x10A40, 0x10A47, BidiCharacterType.AN), // Kharoshthi numbers
            (0x10A50, 0x10A58, BidiCharacterType.R),
            (0x10A60, 0x10A7C, BidiCharacterType.R),  // Old South Arabian
            (0x10A80, 0x10A9C, BidiCharacterType.R),  // Old North Arabian
            (0x10AC0, 0x10AC7, BidiCharacterType.R),  // Manichaean
            (0x10AC9, 0x10AE4, BidiCharacterType.R),
            (0x10AE7, 0x10AEA, BidiCharacterType.NSM),
            (0x10AEB, 0x10AEF, BidiCharacterType.R),
            (0x10B00, 0x10B35, BidiCharacterType.R),  // Avestan
            (0x10B40, 0x10B55, BidiCharacterType.R),  // Inscriptional Parthian
            (0x10B58, 0x10B72, BidiCharacterType.R),  // Inscriptional Pahlavi
            (0x10B78, 0x10B91, BidiCharacterType.R),  // Psalter Pahlavi
            (0x10C00, 0x10C48, BidiCharacterType.R),  // Old Turkic
            (0x10C80, 0x10CB2, BidiCharacterType.R),  // Old Hungarian
            (0x10D00, 0x10D23, BidiCharacterType.R),  // Hanifi Rohingya
            (0x10E60, 0x10E7E, BidiCharacterType.AN), // Rumi number digits
            (0x10F30, 0x10F45, BidiCharacterType.R),  // Sogdian
            (0x10F51, 0x10F54, BidiCharacterType.R),
            (0x10FB0, 0x10FC4, BidiCharacterType.R),  // Chorasmian
            (0x10FE0, 0x10FF6, BidiCharacterType.R),  // Sogdian (old)
            (0x1E800, 0x1E8C4, BidiCharacterType.R),  // Mende Kikakui
            (0x1E8C7, 0x1E8CF, BidiCharacterType.AN),
            (0x1E900, 0x1E943, BidiCharacterType.AL), // Adlam
            (0x1E94B, 0x1E94B, BidiCharacterType.AL),

            // ── Tibetan digits ────────────────────────────────────────────
            (0x0F20, 0x0F33, BidiCharacterType.ON),   // Tibetan digits (treated as ON)

            // ── Ideographic ───────────────────────────────────────────────
            // CJK, Hiragana, Katakana, Hangul etc. are all L (left-to-right)
            // per UAX #9. No entry needed — defaults to L.
        };

            // Sort by start code point so binary search works correctly.
            System.Array.Sort(s_ranges, (a, b) => a.Start.CompareTo(b.Start));
        }

        /// <summary>
        /// Returns the bidirectional character type for <paramref name="codePoint"/>.
        /// Unlisted code points default to <see cref="BidiCharacterType.L"/>.
        /// </summary>
        internal static BidiCharacterType GetType(uint codePoint)
        {
            int lo = 0, hi = s_ranges.Length - 1;
            while (lo <= hi)
            {
                int mid = lo + (hi - lo) / 2;
                var (start, end, type) = s_ranges[mid];
                if (codePoint < start)
                    hi = mid - 1;
                else if (codePoint > end)
                    lo = mid + 1;
                else
                    return type;
            }
            return BidiCharacterType.L;
        }

        /// <summary>
        /// Returns the bidirectional character type for a UTF-16 code unit.
        /// Surrogate pairs are resolved to a full code point before lookup.
        /// </summary>
        internal static BidiCharacterType GetType(string text, int index)
        {
            char c = text[index];
            if (char.IsHighSurrogate(c) && index + 1 < text.Length && char.IsLowSurrogate(text[index + 1]))
            {
                uint cp = (uint)char.ConvertToUtf32(c, text[index + 1]);
                return GetType(cp);
            }
            return GetType((uint)c);
        }
    }
}