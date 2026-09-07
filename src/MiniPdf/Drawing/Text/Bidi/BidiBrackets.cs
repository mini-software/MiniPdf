namespace MiniSoftware.Drawing.Text.Bidi
{
    /// <summary>
    /// Bracket type for UAX #9 N0 (bracket pair resolution).
    /// </summary>
    internal enum BidiBracketType : byte
    {
        None  = 0,
        Open  = 1,
        Close = 2,
    }

    /// <summary>
    /// Bidirectional bracket pairs (UAX #9 §3.1.3, from BidiBrackets.txt).
    /// Maps an opening bracket code point to its closing bracket code point,
    /// and vice versa. Used by the N0 rule.
    /// </summary>
    internal static class BidiBrackets
    {
        // (codePoint, pairedCodePoint, type)
        private static readonly (uint Cp, uint Pair, BidiBracketType Type)[] s_brackets =
        {
            (0x0028, 0x0029, BidiBracketType.Open),   // ( )
            (0x0029, 0x0028, BidiBracketType.Close),
            (0x005B, 0x005D, BidiBracketType.Open),   // [ ]
            (0x005D, 0x005B, BidiBracketType.Close),
            (0x007B, 0x007D, BidiBracketType.Open),   // { }
            (0x007D, 0x007B, BidiBracketType.Close),
            (0x0F3A, 0x0F3B, BidiBracketType.Open),   // Tibetan marks
            (0x0F3B, 0x0F3A, BidiBracketType.Close),
            (0x0F3C, 0x0F3D, BidiBracketType.Open),
            (0x0F3D, 0x0F3C, BidiBracketType.Close),
            (0x169B, 0x169C, BidiBracketType.Open),   // Ogham
            (0x169C, 0x169B, BidiBracketType.Close),
            (0x2018, 0x2019, BidiBracketType.Open),   // ' '
            (0x2019, 0x2018, BidiBracketType.Close),
            (0x201C, 0x201D, BidiBracketType.Open),   // " "
            (0x201D, 0x201C, BidiBracketType.Close),
            (0x2039, 0x203A, BidiBracketType.Open),   // ‹ ›
            (0x203A, 0x2039, BidiBracketType.Close),
            (0x2045, 0x2046, BidiBracketType.Open),   // ⁅ ⁆
            (0x2046, 0x2045, BidiBracketType.Close),
            (0x207D, 0x207E, BidiBracketType.Open),   // ⁽ ⁾
            (0x207E, 0x207D, BidiBracketType.Close),
            (0x208D, 0x208E, BidiBracketType.Open),   // ₍ ₎
            (0x208E, 0x208D, BidiBracketType.Close),
            (0x2308, 0x2309, BidiBracketType.Open),   // ⌈ ⌉
            (0x2309, 0x2308, BidiBracketType.Close),
            (0x230A, 0x230B, BidiBracketType.Open),   // ⌊ ⌋
            (0x230B, 0x230A, BidiBracketType.Close),
            (0x2329, 0x232A, BidiBracketType.Open),   // 〈 〉
            (0x232A, 0x2329, BidiBracketType.Close),
            (0x2768, 0x2769, BidiBracketType.Open),   // ❨ ❩
            (0x2769, 0x2768, BidiBracketType.Close),
            (0x276A, 0x276B, BidiBracketType.Open),
            (0x276B, 0x276A, BidiBracketType.Close),
            (0x276C, 0x276D, BidiBracketType.Open),
            (0x276D, 0x276C, BidiBracketType.Close),
            (0x276E, 0x276F, BidiBracketType.Open),
            (0x276F, 0x276E, BidiBracketType.Close),
            (0x2770, 0x2771, BidiBracketType.Open),
            (0x2771, 0x2770, BidiBracketType.Close),
            (0x2772, 0x2773, BidiBracketType.Open),
            (0x2773, 0x2772, BidiBracketType.Close),
            (0x2774, 0x2775, BidiBracketType.Open),
            (0x2775, 0x2774, BidiBracketType.Close),
            (0x27C5, 0x27C6, BidiBracketType.Open),   // ⟅ ⟆
            (0x27C6, 0x27C5, BidiBracketType.Close),
            (0x27E6, 0x27E7, BidiBracketType.Open),   // ⟦ ⟧
            (0x27E7, 0x27E6, BidiBracketType.Close),
            (0x27E8, 0x27E9, BidiBracketType.Open),   // ⟨ ⟩
            (0x27E9, 0x27E8, BidiBracketType.Close),
            (0x27EA, 0x27EB, BidiBracketType.Open),   // ⟪ ⟫
            (0x27EB, 0x27EA, BidiBracketType.Close),
            (0x27EC, 0x27ED, BidiBracketType.Open),   // ⟬ ⟭
            (0x27ED, 0x27EC, BidiBracketType.Close),
            (0x27EE, 0x27EF, BidiBracketType.Open),   // ⟮ ⟯
            (0x27EF, 0x27EE, BidiBracketType.Close),
            (0x2983, 0x2984, BidiBracketType.Open),   // ⦃ ⦄
            (0x2984, 0x2983, BidiBracketType.Close),
            (0x2985, 0x2986, BidiBracketType.Open),
            (0x2986, 0x2985, BidiBracketType.Close),
            (0x2987, 0x2988, BidiBracketType.Open),
            (0x2988, 0x2987, BidiBracketType.Close),
            (0x2989, 0x298A, BidiBracketType.Open),
            (0x298A, 0x2989, BidiBracketType.Close),
            (0x298B, 0x298C, BidiBracketType.Open),
            (0x298C, 0x298B, BidiBracketType.Close),
            (0x298D, 0x2990, BidiBracketType.Open),
            (0x298E, 0x298F, BidiBracketType.Close),
            (0x298F, 0x298E, BidiBracketType.Open),
            (0x2990, 0x298D, BidiBracketType.Close),
            (0x2991, 0x2992, BidiBracketType.Open),
            (0x2992, 0x2991, BidiBracketType.Close),
            (0x2993, 0x2994, BidiBracketType.Open),
            (0x2994, 0x2993, BidiBracketType.Close),
            (0x2995, 0x2996, BidiBracketType.Open),
            (0x2996, 0x2995, BidiBracketType.Close),
            (0x2997, 0x2998, BidiBracketType.Open),
            (0x2998, 0x2997, BidiBracketType.Close),
            (0x29D8, 0x29D9, BidiBracketType.Open),
            (0x29D9, 0x29D8, BidiBracketType.Close),
            (0x29DA, 0x29DB, BidiBracketType.Open),
            (0x29DB, 0x29DA, BidiBracketType.Close),
            (0x29FC, 0x29FD, BidiBracketType.Open),
            (0x29FD, 0x29FC, BidiBracketType.Close),
            (0x2E22, 0x2E23, BidiBracketType.Open),   // ⸢ ⸣
            (0x2E23, 0x2E22, BidiBracketType.Close),
            (0x2E24, 0x2E25, BidiBracketType.Open),   // ⸤ ⸥
            (0x2E25, 0x2E24, BidiBracketType.Close),
            (0x2E26, 0x2E27, BidiBracketType.Open),
            (0x2E27, 0x2E26, BidiBracketType.Close),
            (0x2E28, 0x2E29, BidiBracketType.Open),
            (0x2E29, 0x2E28, BidiBracketType.Close),
            (0x3008, 0x3009, BidiBracketType.Open),   // 〈 〉
            (0x3009, 0x3008, BidiBracketType.Close),
            (0x300A, 0x300B, BidiBracketType.Open),   // 《 》
            (0x300B, 0x300A, BidiBracketType.Close),
            (0x300C, 0x300D, BidiBracketType.Open),   // 「 」
            (0x300D, 0x300C, BidiBracketType.Close),
            (0x300E, 0x300F, BidiBracketType.Open),   // 『 』
            (0x300F, 0x300E, BidiBracketType.Close),
            (0x3010, 0x3011, BidiBracketType.Open),   // 【 】
            (0x3011, 0x3010, BidiBracketType.Close),
            (0x3014, 0x3015, BidiBracketType.Open),   // 〔 〕
            (0x3015, 0x3014, BidiBracketType.Close),
            (0x3016, 0x3017, BidiBracketType.Open),   // 〖 〗
            (0x3017, 0x3016, BidiBracketType.Close),
            (0x3018, 0x3019, BidiBracketType.Open),   // 〘 〙
            (0x3019, 0x3018, BidiBracketType.Close),
            (0x301A, 0x301B, BidiBracketType.Open),   // 〚 〛
            (0x301B, 0x301A, BidiBracketType.Close),
            (0xFE59, 0xFE5A, BidiBracketType.Open),   // Small ( )
            (0xFE5A, 0xFE59, BidiBracketType.Close),
            (0xFE5B, 0xFE5C, BidiBracketType.Open),   // Small { }
            (0xFE5C, 0xFE5B, BidiBracketType.Close),
            (0xFE5D, 0xFE5E, BidiBracketType.Open),   // Small [ ]
            (0xFE5E, 0xFE5D, BidiBracketType.Close),
            (0xFF08, 0xFF09, BidiBracketType.Open),   // Fullwidth ( )
            (0xFF09, 0xFF08, BidiBracketType.Close),
            (0xFF3B, 0xFF3D, BidiBracketType.Open),   // Fullwidth [ ]
            (0xFF3D, 0xFF3B, BidiBracketType.Close),
            (0xFF5B, 0xFF5D, BidiBracketType.Open),   // Fullwidth { }
            (0xFF5D, 0xFF5B, BidiBracketType.Close),
            (0xFF5F, 0xFF60, BidiBracketType.Open),   // Fullwidth ｟ ｠
            (0xFF60, 0xFF5F, BidiBracketType.Close),
            (0xFF62, 0xFF63, BidiBracketType.Open),   // Halfwidth ｢ ｣
            (0xFF63, 0xFF62, BidiBracketType.Close),
        };

        /// <summary>
        /// Returns the bracket type for <paramref name="codePoint"/>,
        /// or <see cref="BidiBracketType.None"/> if it is not a bracket.
        /// </summary>
        internal static BidiBracketType GetBracketType(uint codePoint)
        {
            foreach (var (cp, pair, type) in s_brackets)
            {
                if (cp == codePoint)
                    return type;
                if (cp > codePoint)
                    break;
            }
            return BidiBracketType.None;
        }

        /// <summary>
        /// Returns the paired bracket code point for <paramref name="codePoint"/>,
        /// or 0 if <paramref name="codePoint"/> is not a bracket.
        /// </summary>
        internal static uint GetPairedBracket(uint codePoint)
        {
            foreach (var (cp, pair, type) in s_brackets)
            {
                if (cp == codePoint)
                    return pair;
                if (cp > codePoint)
                    break;
            }
            return 0;
        }
    }
}