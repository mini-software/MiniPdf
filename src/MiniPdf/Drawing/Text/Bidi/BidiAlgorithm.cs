using System;
using System.Collections.Generic;

namespace MiniPdf.Drawing.Text.Bidi
{
    /// <summary>
    /// Implements the Unicode Bidirectional Algorithm (UAX #9).
    /// Pure function: string + paragraph direction → int[] embedding levels.
    /// </summary>
    internal static class BidiAlgorithm
    {
        private const int MaxDepth = 125;

        /// <summary>
        /// Computes the Unicode bidirectional embedding level for each UTF-16
        /// code unit in <paramref name="text"/>.
        /// </summary>
        internal static int GetLevels(string text, BidiParagraphDirection paragraphDirection, out int[] levels)
        {
            if (text == null) throw new ArgumentNullException(nameof(text));
            levels = new int[text.Length];

            if (text.Length == 0)
                return 0;

            // ── P1–P3: Determine paragraph level ────────────────────────────
            int baseLevel;
            if (paragraphDirection == BidiParagraphDirection.RTL)
                baseLevel = 1;
            else if (paragraphDirection == BidiParagraphDirection.LTR)
                baseLevel = 0;
            else
                baseLevel = DetermineParagraphLevel(text);

            // ── Get initial character types ─────────────────────────────────
            var types = new BidiCharacterType[text.Length];
            for (int i = 0; i < text.Length; i++)
                types[i] = BidiCharacterTable.GetType(text, i);

            // ── X1a: Resolve FSI → LRI or RLI ───────────────────────────────
            ResolveFSI(text, types);

            // ── X1–X10: Explicit levels and directions ──────────────────────
            ProcessExplicitLevels(text, types, levels, baseLevel);

            // ── W1–W7: Resolve weak types ───────────────────────────────────
            ResolveWeakTypes(types, levels, baseLevel);

            // ── N0–N2: Resolve neutral types ────────────────────────────────
            ResolveNeutralTypes(text, types, levels, baseLevel);

            // ── I1–I2: Resolve implicit levels ──────────────────────────────
            ResolveImplicitLevels(types, levels, baseLevel);

            // ── L1: Reset trailing whitespace/segment separators ─────────────
            ResetTrailingWhitespace(types, levels, baseLevel);

            return baseLevel;
        }

        // ── P1–P3 ───────────────────────────────────────────────────────────

        private static int DetermineParagraphLevel(string text)
        {
            for (int i = 0; i < text.Length; i++)
            {
                var t = BidiCharacterTable.GetType(text, i);

                if (t == BidiCharacterType.FSI || t == BidiCharacterType.LRI || t == BidiCharacterType.RLI)
                {
                    i = SkipIsolate(text, i);
                    continue;
                }

                if (t == BidiCharacterType.L)
                    return 0;
                if (t == BidiCharacterType.R || t == BidiCharacterType.AL)
                    return 1;
            }
            return 0;
        }

        private static int SkipIsolate(string text, int start)
        {
            int depth = 1;
            for (int i = start + 1; i < text.Length; i++)
            {
                var t = BidiCharacterTable.GetType(text, i);
                if (t == BidiCharacterType.LRI || t == BidiCharacterType.RLI || t == BidiCharacterType.FSI)
                    depth++;
                else if (t == BidiCharacterType.PDI)
                {
                    depth--;
                    if (depth == 0) return i;
                }
            }
            return text.Length - 1;
        }

        // ── X1a: Resolve FSI ────────────────────────────────────────────────

        private static void ResolveFSI(string text, BidiCharacterType[] types)
        {
            for (int i = 0; i < types.Length; i++)
            {
                if (types[i] != BidiCharacterType.FSI)
                    continue;

                int end = SkipIsolate(text, i);
                bool rtl = IsFirstStrongRTL(text, i + 1, end);
                types[i] = rtl ? BidiCharacterType.RLI : BidiCharacterType.LRI;
            }
        }

        private static bool IsFirstStrongRTL(string text, int start, int end)
        {
            for (int i = start; i < end && i < text.Length; i++)
            {
                var t = BidiCharacterTable.GetType(text, i);
                if (t == BidiCharacterType.L) return false;
                if (t == BidiCharacterType.R || t == BidiCharacterType.AL) return true;
                if (t == BidiCharacterType.FSI || t == BidiCharacterType.LRI || t == BidiCharacterType.RLI)
                    i = SkipIsolate(text, i);
            }
            return false;
        }

        // ── X1–X10: Explicit embedding / override / isolate processing ──────

        private struct DirectionalStatus
        {
            public int EmbeddingLevel;
            public BidiCharacterType OverrideStatus;
            public bool IsolateStatus;
        }

        private static void ProcessExplicitLevels(
            string text,
            BidiCharacterType[] types,
            int[] levels,
            int baseLevel)
        {
            var stack = new Stack<DirectionalStatus>();
            stack.Push(new DirectionalStatus
            {
                EmbeddingLevel = baseLevel,
                OverrideStatus = BidiCharacterType.ON,
                IsolateStatus = false
            });

            int overflowIsolate = 0;
            int overflowEmbedding = 0;
            int validIsolate = 0;

            for (int i = 0; i < text.Length; i++)
            {
                var t = types[i];
                var current = stack.Peek();

                switch (t)
                {
                    case BidiCharacterType.LRI:
                    case BidiCharacterType.RLI:
                        levels[i] = current.EmbeddingLevel;
                        if (current.OverrideStatus == BidiCharacterType.L)
                            types[i] = BidiCharacterType.L;
                        else if (current.OverrideStatus == BidiCharacterType.R)
                            types[i] = BidiCharacterType.R;
                        else
                            types[i] = BidiCharacterType.BN;

                        if (overflowIsolate > 0)
                        {
                            overflowIsolate++;
                        }
                        else if (current.EmbeddingLevel + 1 > MaxDepth)
                        {
                            overflowIsolate++;
                        }
                        else
                        {
                            validIsolate++;
                            int newLevel = t == BidiCharacterType.LRI
                                ? NextOddLevel(current.EmbeddingLevel)
                                : NextEvenLevel(current.EmbeddingLevel);
                            stack.Push(new DirectionalStatus
                            {
                                EmbeddingLevel = newLevel,
                                OverrideStatus = BidiCharacterType.ON,
                                IsolateStatus = true
                            });
                        }
                        break;

                    case BidiCharacterType.PDI:
                        if (overflowIsolate > 0)
                        {
                            overflowIsolate--;
                            levels[i] = current.EmbeddingLevel;
                            types[i] = BidiCharacterType.BN;
                        }
                        else if (validIsolate > 0)
                        {
                            overflowEmbedding = 0;
                            while (stack.Count > 1 && !stack.Peek().IsolateStatus)
                                stack.Pop();
                            if (stack.Count > 1)
                                stack.Pop();
                            validIsolate--;
                            current = stack.Peek();
                            levels[i] = current.EmbeddingLevel;
                            if (current.OverrideStatus == BidiCharacterType.L)
                                types[i] = BidiCharacterType.L;
                            else if (current.OverrideStatus == BidiCharacterType.R)
                                types[i] = BidiCharacterType.R;
                            else
                                types[i] = BidiCharacterType.BN;
                        }
                        else
                        {
                            levels[i] = current.EmbeddingLevel;
                            types[i] = BidiCharacterType.BN;
                        }
                        break;

                    case BidiCharacterType.LRE:
                    case BidiCharacterType.RLE:
                    case BidiCharacterType.LRO:
                    case BidiCharacterType.RLO:
                        {
                            levels[i] = current.EmbeddingLevel;
                            if (current.OverrideStatus != BidiCharacterType.ON)
                                types[i] = current.OverrideStatus;
                            else
                                types[i] = BidiCharacterType.BN;

                            bool isOverride = t == BidiCharacterType.LRO || t == BidiCharacterType.RLO;
                            BidiCharacterType overrideStatus = BidiCharacterType.ON;
                            if (isOverride)
                                overrideStatus = (t == BidiCharacterType.LRO) ? BidiCharacterType.L : BidiCharacterType.R;

                            if (overflowIsolate > 0 || overflowEmbedding > 0)
                            {
                                overflowEmbedding++;
                            }
                            else
                            {
                                int newLevel = (t == BidiCharacterType.LRE || t == BidiCharacterType.LRO)
                                    ? NextOddLevel(current.EmbeddingLevel)
                                    : NextEvenLevel(current.EmbeddingLevel);
                                if (newLevel > MaxDepth)
                                {
                                    overflowEmbedding++;
                                }
                                else
                                {
                                    stack.Push(new DirectionalStatus
                                    {
                                        EmbeddingLevel = newLevel,
                                        OverrideStatus = overrideStatus,
                                        IsolateStatus = false
                                    });
                                }
                            }
                        }
                        break;

                    case BidiCharacterType.PDF:
                        levels[i] = current.EmbeddingLevel;
                        types[i] = BidiCharacterType.BN;
                        if (overflowIsolate > 0)
                        {
                            // ignore
                        }
                        else if (overflowEmbedding > 0)
                        {
                            overflowEmbedding--;
                        }
                        else if (stack.Count > 1 && !stack.Peek().IsolateStatus)
                        {
                            stack.Pop();
                        }
                        break;

                    case BidiCharacterType.B:
                        levels[i] = baseLevel;
                        break;

                    case BidiCharacterType.BN:
                        levels[i] = current.EmbeddingLevel;
                        break;

                    default:
                        levels[i] = current.EmbeddingLevel;
                        if (current.OverrideStatus == BidiCharacterType.L)
                            types[i] = BidiCharacterType.L;
                        else if (current.OverrideStatus == BidiCharacterType.R)
                            types[i] = BidiCharacterType.R;
                        break;
                }
            }
        }

        private static int NextOddLevel(int level)
        {
            return (level & 1) == 0 ? level + 1 : level + 2;
        }

        private static int NextEvenLevel(int level)
        {
            return (level & 1) == 0 ? level + 2 : level + 1;
        }

        // ── W1–W7 ───────────────────────────────────────────────────────────

        private static void ResolveWeakTypes(BidiCharacterType[] types, int[] levels, int baseLevel)
        {
            int len = types.Length;
            if (len == 0) return;

            var embedDir = baseLevel == 0 ? BidiCharacterType.L : BidiCharacterType.R;

            // W1: NSM inherits previous char's type (or embedding direction at start)
            {
                BidiCharacterType prevType = embedDir;
                for (int i = 0; i < len; i++)
                {
                    if (types[i] == BidiCharacterType.NSM)
                    {
                        types[i] = prevType;
                    }
                    else if (types[i] != BidiCharacterType.BN)
                    {
                        prevType = types[i];
                    }
                }
            }

            // W2: EN after R/AL → AN
            {
                BidiCharacterType lastStrong = embedDir;
                for (int i = 0; i < len; i++)
                {
                    if (types[i] == BidiCharacterType.EN)
                    {
                        if (lastStrong == BidiCharacterType.R || lastStrong == BidiCharacterType.AL)
                            types[i] = BidiCharacterType.AN;
                    }
                    else if (IsStrong(types[i]))
                    {
                        lastStrong = types[i];
                    }
                }
            }

            // W3: AL → R
            for (int i = 0; i < len; i++)
                if (types[i] == BidiCharacterType.AL)
                    types[i] = BidiCharacterType.R;

            // W4: ES between EN-EN → EN; CS between EN-AN or AN-AN → match
            for (int i = 1; i < len - 1; i++)
            {
                if (types[i] == BidiCharacterType.ES &&
                    types[i - 1] == BidiCharacterType.EN &&
                    types[i + 1] == BidiCharacterType.EN)
                    types[i] = BidiCharacterType.EN;
                else if (types[i] == BidiCharacterType.CS)
                {
                    if (types[i - 1] == BidiCharacterType.EN && types[i + 1] == BidiCharacterType.AN)
                        types[i] = BidiCharacterType.EN;
                    else if (types[i - 1] == BidiCharacterType.AN && types[i + 1] == BidiCharacterType.EN)
                        types[i] = BidiCharacterType.AN;
                    else if (types[i - 1] == BidiCharacterType.AN && types[i + 1] == BidiCharacterType.AN)
                        types[i] = BidiCharacterType.AN;
                }
            }

            // W5: ET sequences adjacent to EN → EN
            for (int i = 0; i < len; i++)
            {
                if (types[i] == BidiCharacterType.ET)
                {
                    int j = i;
                    while (j < len && types[j] == BidiCharacterType.ET) j++;
                    bool convert = (j < len && types[j] == BidiCharacterType.EN) ||
                                   (i > 0 && types[i - 1] == BidiCharacterType.EN);
                    if (convert)
                        for (int k = i; k < j; k++)
                            types[k] = BidiCharacterType.EN;
                    i = j - 1;
                }
            }

            // W6: Remaining ES, ET, CS → ON
            for (int i = 0; i < len; i++)
                if (types[i] == BidiCharacterType.ES || types[i] == BidiCharacterType.ET || types[i] == BidiCharacterType.CS)
                    types[i] = BidiCharacterType.ON;

            // W7: EN with last strong L → L
            {
                BidiCharacterType lastStrong = embedDir;
                for (int i = 0; i < len; i++)
                {
                    if (types[i] == BidiCharacterType.EN && lastStrong == BidiCharacterType.L)
                        types[i] = BidiCharacterType.L;
                    else if (IsStrong(types[i]))
                        lastStrong = types[i];
                }
            }
        }

        private static bool IsStrong(BidiCharacterType t)
            => t == BidiCharacterType.L || t == BidiCharacterType.R;

        // ── N0–N2 ───────────────────────────────────────────────────────────

        private static void ResolveNeutralTypes(string text, BidiCharacterType[] types, int[] levels, int baseLevel)
        {
            int len = types.Length;
            if (len == 0) return;

            // N0: Resolve bracket pairs
            ResolveBracketPairs(text, types, baseLevel);

            // N1–N2: Resolve neutral sequences
            for (int i = 0; i < len; i++)
            {
                if (!IsNeutral(types[i])) continue;

                int start = i;
                while (i < len && IsNeutral(types[i])) i++;
                int end = i;

                BidiCharacterType? left = LeftStrongOrNumber(types, start);
                BidiCharacterType? right = RightStrongOrNumber(types, end);

                BidiCharacterType resolved;
                if (left.HasValue && right.HasValue)
                {
                    if (left == right)
                        resolved = left.Value;
                    else
                        resolved = baseLevel == 0 ? BidiCharacterType.L : BidiCharacterType.R;
                }
                else if (left.HasValue)
                    resolved = left.Value;
                else if (right.HasValue)
                    resolved = right.Value;
                else
                    resolved = baseLevel == 0 ? BidiCharacterType.L : BidiCharacterType.R;

                for (int j = start; j < end; j++)
                    types[j] = resolved;

                i = end - 1;
            }
        }

        private static BidiCharacterType? LeftStrongOrNumber(BidiCharacterType[] types, int from)
        {
            for (int j = from - 1; j >= 0; j--)
            {
                if (IsStrong(types[j])) return types[j];
                if (types[j] == BidiCharacterType.AN || types[j] == BidiCharacterType.EN)
                    return BidiCharacterType.R;
            }
            return null;
        }

        private static BidiCharacterType? RightStrongOrNumber(BidiCharacterType[] types, int from)
        {
            for (int j = from; j < types.Length; j++)
            {
                if (IsStrong(types[j])) return types[j];
                if (types[j] == BidiCharacterType.AN || types[j] == BidiCharacterType.EN)
                    return BidiCharacterType.R;
            }
            return null;
        }

        private static bool IsNeutral(BidiCharacterType t)
            => t == BidiCharacterType.ON || t == BidiCharacterType.WS ||
               t == BidiCharacterType.B  || t == BidiCharacterType.S;

        private static void ResolveBracketPairs(string text, BidiCharacterType[] types, int baseLevel)
        {
            int len = types.Length;
            var embedDir = baseLevel == 0 ? BidiCharacterType.L : BidiCharacterType.R;

            for (int i = 0; i < len; i++)
            {
                uint cp = CodePointAt(text, i);
                var bracketType = BidiBrackets.GetBracketType(cp);

                if (bracketType != BidiBracketType.Open)
                    continue;

                uint pair = BidiBrackets.GetPairedBracket(cp);

                // Scan forward for the matching close bracket, respecting nesting
                int depth = 1;
                int closeIdx = -1;
                for (int j = i + 1; j < len; j++)
                {
                    // Skip characters on a different embedding level
                    // (bracket pairs must be on the same level)
                    uint jcp = CodePointAt(text, j);
                    var jt = BidiBrackets.GetBracketType(jcp);

                    if (jt == BidiBracketType.Open)
                        depth++;
                    else if (jt == BidiBracketType.Close)
                    {
                        if (BidiBrackets.GetPairedBracket(jcp) == cp || jcp == pair)
                        {
                            depth--;
                            if (depth == 0)
                            {
                                closeIdx = j;
                                break;
                            }
                        }
                    }

                    // Advance past surrogate pairs
                    if (char.IsHighSurrogate(text[j]) && j + 1 < text.Length && char.IsLowSurrogate(text[j + 1]))
                        j++;
                }

                if (closeIdx < 0)
                    continue;

                // Check the direction of the enclosing context
                // Look at the strong type immediately before the opening bracket
                BidiCharacterType? dirBefore = null;
                for (int j = i - 1; j >= 0; j--)
                {
                    if (IsStrong(types[j]))
                    {
                        dirBefore = types[j];
                        break;
                    }
                    if (types[j] == BidiCharacterType.AN || types[j] == BidiCharacterType.EN)
                    {
                        dirBefore = BidiCharacterType.R;
                        break;
                    }
                }

                // Look at the strong type immediately after the closing bracket
                BidiCharacterType? dirAfter = null;
                for (int j = closeIdx + 1; j < len; j++)
                {
                    if (IsStrong(types[j]))
                    {
                        dirAfter = types[j];
                        break;
                    }
                    if (types[j] == BidiCharacterType.AN || types[j] == BidiCharacterType.EN)
                    {
                        dirAfter = BidiCharacterType.R;
                        break;
                    }
                }

                // N0b: If dirBefore == dirAfter, bracket content takes that direction
                // N0c: If dirBefore != dirAfter, check if content has a strong char matching embedDir
                BidiCharacterType bracketDir;
                if (dirBefore.HasValue && dirAfter.HasValue && dirBefore == dirAfter)
                {
                    bracketDir = dirBefore.Value;
                }
                else
                {
                    // Check if any char inside brackets is strong and matches embedding direction
                    bool foundEmbed = false;
                    bool foundOpposite = false;
                    for (int j = i + 1; j < closeIdx; j++)
                    {
                        if (IsStrong(types[j]))
                        {
                            if (types[j] == embedDir) foundEmbed = true;
                            else foundOpposite = true;
                        }
                        if (types[j] == BidiCharacterType.AN || types[j] == BidiCharacterType.EN)
                        {
                            if (embedDir == BidiCharacterType.R) foundEmbed = true;
                            else foundOpposite = true;
                        }
                    }

                    if (foundEmbed)
                        bracketDir = embedDir;
                    else if (dirBefore.HasValue)
                        bracketDir = dirBefore.Value;
                    else if (dirAfter.HasValue)
                        bracketDir = dirAfter.Value;
                    else
                        bracketDir = embedDir;
                }

                // Set bracket characters and enclosed neutrals to bracketDir
                types[i] = bracketDir;
                types[closeIdx] = bracketDir;
                for (int j = i + 1; j < closeIdx; j++)
                {
                    if (IsNeutral(types[j]))
                        types[j] = bracketDir;
                }

                // Advance past the close bracket
                i = closeIdx;
            }
        }

        private static uint CodePointAt(string text, int index)
        {
            char c = text[index];
            if (char.IsHighSurrogate(c) && index + 1 < text.Length && char.IsLowSurrogate(text[index + 1]))
                return (uint)char.ConvertToUtf32(c, text[index + 1]);
            return (uint)c;
        }

        // ── I1–I2 ───────────────────────────────────────────────────────────

        private static void ResolveImplicitLevels(BidiCharacterType[] types, int[] levels, int baseLevel)
        {
            for (int i = 0; i < types.Length; i++)
            {
                if ((levels[i] & 1) == 0)
                {
                    if (types[i] == BidiCharacterType.R)
                        levels[i] += 1;
                    else if (types[i] == BidiCharacterType.AN || types[i] == BidiCharacterType.EN)
                        levels[i] += 2;
                }
                else
                {
                    if (types[i] == BidiCharacterType.L ||
                        types[i] == BidiCharacterType.EN ||
                        types[i] == BidiCharacterType.AN)
                    {
                        levels[i] += 1;
                    }
                }
            }
        }

        // ── L1 ──────────────────────────────────────────────────────────────

        private static void ResetTrailingWhitespace(BidiCharacterType[] types, int[] levels, int baseLevel)
        {
            int len = levels.Length;

            for (int i = len - 1; i >= 0; i--)
            {
                var t = types[i];
                if (t == BidiCharacterType.WS || t == BidiCharacterType.B ||
                    t == BidiCharacterType.S  || t == BidiCharacterType.BN)
                {
                    levels[i] = baseLevel;
                }
                else
                    break;
            }

            for (int i = 0; i < len; i++)
            {
                if (types[i] == BidiCharacterType.B)
                {
                    for (int j = i - 1; j >= 0; j--)
                    {
                        if (types[j] == BidiCharacterType.WS || types[j] == BidiCharacterType.S || types[j] == BidiCharacterType.BN)
                            levels[j] = baseLevel;
                        else
                            break;
                    }
                    levels[i] = baseLevel;
                }
                else if (types[i] == BidiCharacterType.S)
                {
                    levels[i] = baseLevel;
                }
            }
        }
    }
}