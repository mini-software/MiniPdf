using System;
using System.Collections.Generic;
using MiniPdf.Drawing.Brushes;
using MiniPdf.Drawing.Enums;
using MiniPdf.Drawing.Text.Bidi;

namespace MiniPdf.Drawing.Text
{
    /// <summary>
    /// Orchestrates rich-run → bidi → fallback splitting to produce
    /// <see cref="ResolvedRun"/> objects for multi-run text layout.
    /// </summary>
    internal static class RunSegmenter
    {
        internal static List<ResolvedRun> Segment(
            string             text,
            Font               defaultFont,
            RichTextOptions?   rich,
            StringFormat       sf,
            FontFallbackChain? chain)
        {
            if (text == null) throw new ArgumentNullException(nameof(text));
            if (defaultFont == null) throw new ArgumentNullException(nameof(defaultFont));
            if (sf == null) throw new ArgumentNullException(nameof(sf));

            bool fallbackEnabled = (sf.FormatFlags & StringFormatFlags.NoFontFallback) == 0;

            // ── 1. Rich runs (or one default run) ──────────────────────────
            var rawRuns = new List<(string Text, Font Font, Brush? Brush, int Start)>();

            if (rich != null && rich.Runs.Count > 0)
            {
                SplitByRichRuns(text, rich, defaultFont, rawRuns);
            }
            else
            {
                rawRuns.Add((text, defaultFont, null, 0));
            }

            // ── 2. Bidi on the full string ─────────────────────────────────
            var paraDir = (sf.FormatFlags & StringFormatFlags.DirectionRightToLeft) != 0
                ? BidiParagraphDirection.RTL
                : BidiParagraphDirection.Auto;

            int baseLevel = BidiAlgorithm.GetLevels(text, paraDir, out int[] levels);

            // Split each rich run at bidi-level boundaries
            var bidiRuns = new List<(string Text, Font Font, Brush? Brush, int BidiLevel, int Start)>();
            foreach (var raw in rawRuns)
            {
                int segStart = 0;
                for (int i = 1; i <= raw.Text.Length; i++)
                {
                    int srcIdx = raw.Start + i - 1;
                    bool levelBoundary = (i >= raw.Text.Length) ||
                                         (raw.Start + i < levels.Length &&
                                          levels[raw.Start + i] != levels[srcIdx]);
                    if (levelBoundary)
                    {
                        int segEnd = i;
                        int srcStart = raw.Start + segStart;
                        bidiRuns.Add((
                            raw.Text.Substring(segStart, segEnd - segStart),
                            raw.Font,
                            raw.Brush,
                            levels[srcStart],
                            srcStart));
                        segStart = i;
                    }
                }
            }

            // ── 3. Fallback per visual segment ─────────────────────────────
            var finalRuns = new List<ResolvedRun>();

            foreach (var br in bidiRuns)
            {
                var resolver = new FontFallbackResolver(
                    br.Font.FontFamily, br.Font.Style, chain, fallbackEnabled);

                var curFamily = br.Font.FontFamily;
                int segStart = 0;

                for (int k = 0; k < br.Text.Length; k++)
                {
                    uint cp = CodePointAt(br.Text, k);
                    var family = resolver.ResolveFamily(cp);
                    if (family != curFamily)
                    {
                        if (k > segStart)
                        {
                            finalRuns.Add(MakeRun(br, segStart, k, curFamily));
                        }
                        curFamily = family;
                        segStart = k;
                    }

                    // Advance past low surrogate
                    if (char.IsHighSurrogate(br.Text[k]) && k + 1 < br.Text.Length && char.IsLowSurrogate(br.Text[k + 1]))
                        k++;
                }

                if (segStart < br.Text.Length)
                {
                    finalRuns.Add(MakeRun(br, segStart, br.Text.Length, curFamily));
                }
            }

            return finalRuns;
        }

        internal static int GetBaseLevel(string text, StringFormat sf)
        {
            var paraDir = (sf.FormatFlags & StringFormatFlags.DirectionRightToLeft) != 0
                ? BidiParagraphDirection.RTL
                : BidiParagraphDirection.Auto;
            return BidiAlgorithm.GetLevels(text, paraDir, out _);
        }

        private static void SplitByRichRuns(
            string text,
            RichTextOptions rich,
            Font defaultFont,
            List<(string Text, Font Font, Brush? Brush, int Start)> result)
        {
            var sortedRuns = new List<RichTextRun>(rich.Runs);
            sortedRuns.Sort((a, b) => a.Start.CompareTo(b.Start));

            int pos = 0;
            foreach (var run in sortedRuns)
            {
                // Gap before this run uses default font
                if (run.Start > pos)
                {
                    int len = Math.Min(run.Start, text.Length) - pos;
                    if (len > 0)
                        result.Add((text.Substring(pos, len), defaultFont, null, pos));
                    pos = Math.Min(run.Start, text.Length);
                }

                int runEnd = Math.Min(run.Start + run.Length, text.Length);
                if (runEnd <= pos)
                    continue;

                var family = run.FontFamily ?? rich.FontFamily;
                var style = run.FontStyle ?? rich.FontStyle;
                var emSize = run.EmSize ?? rich.EmSize;

                var font = new Font(family, emSize, style, rich.Unit);
                result.Add((text.Substring(pos, runEnd - pos), font, run.Brush, pos));
                pos = runEnd;
            }

            // Trailing gap
            if (pos < text.Length)
                result.Add((text.Substring(pos), defaultFont, null, pos));
        }

        private static ResolvedRun MakeRun(
            (string Text, Font Font, Brush? Brush, int BidiLevel, int Start) br,
            int segStart, int segEnd, FontFamily family)
        {
            var subText = br.Text.Substring(segStart, segEnd - segStart);
            var font = family == br.Font.FontFamily
                ? br.Font
                : new Font(family, br.Font.Size, br.Font.Style, br.Font.Unit);
            return new ResolvedRun(
                subText, font, br.Brush, br.BidiLevel, br.Start + segStart);
        }

        internal static uint CodePointAt(string text, int index)
        {
            char c = text[index];
            if (char.IsHighSurrogate(c) && index + 1 < text.Length && char.IsLowSurrogate(text[index + 1]))
                return (uint)char.ConvertToUtf32(c, text[index + 1]);
            return (uint)c;
        }
    }
}