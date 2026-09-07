using System;
using System.Collections.Generic;
using MiniSoftware.Drawing.Brushes;
using MiniSoftware.Drawing.Enums;
using MiniSoftware.Drawing.Font.Core;
using MiniSoftware.Drawing.Geometry;

namespace MiniSoftware.Drawing.Text
{
    /// <summary>
    /// Lays out text into a sequence of <see cref="TextLine"/> objects within a
    /// pixel-space rectangle, applying word-wrap, tab expansion, and alignment.
    /// All input coordinates and returned positions are in pixels.
    /// </summary>
    internal static class TextLayoutEngine
    {
        internal const float DefaultCharWidthFactor = 0.5f;

        // ── Single-run layout (preserved for backward compat) ────────────────

        public static List<TextLine> Layout(
            string        text,
            Font          font,
            RectangleF    layoutRect,
            StringFormat? sf,
            float         dpiX,
            float         dpiY)
        {
            if (string.IsNullOrEmpty(text))
                return new List<TextLine>();

            sf = sf ?? StringFormat.GenericDefault;

            var metrics    = font.FontFamily.GetMetricsData(font.Style);
            float emPixels = FontSizeToPixels(font.Size, font.Unit, dpiY);
            float emScale  = metrics.UnitsPerEm > 0
                ? (float)(emPixels / metrics.UnitsPerEm)
                : 1f;

            double rawDesc  = metrics.Descender <= 0 ? -metrics.Descender : metrics.Descender;
            float  ascender = (float)(metrics.Ascender * emScale);
            float  descender = (float)(rawDesc * emScale);
            float  lineGap   = (float)(metrics.LineGap * emScale);
            float  lineHeight = ascender + descender + lineGap;
            if (lineHeight < 1f) lineHeight = emPixels * 1.2f;

            IFont? fontFace      = font.FontFamily.GetFontFace(font.Style);
            float  fallbackWidth = emPixels * DefaultCharWidthFactor;

            float   firstTabOffset;
            float[] tabStops = sf.GetTabStops(out firstTabOffset);

            bool  noWrap = (sf.FormatFlags & StringFormatFlags.NoWrap) != 0;
            float maxW   = layoutRect.Width > 0f ? layoutRect.Width : float.MaxValue;

            var paragraphs = SplitOnHardBreaks(text);
            var lines      = new List<TextLine>();

            foreach (string para in paragraphs)
            {
                IReadOnlyList<string> segments = noWrap
                    ? (IReadOnlyList<string>)new[] { para }
                    : WordWrap(para, fontFace, emScale, fallbackWidth, maxW,
                               firstTabOffset, tabStops, emPixels);

                foreach (string seg in segments)
                {
                    float w = MeasureLineWidth(seg, fontFace, emScale, fallbackWidth,
                                               firstTabOffset, tabStops, emPixels);
                    var line = new TextLine { Width = w, LineHeight = lineHeight };
                    line.Runs.Add(new TextRun(seg, font, 0f, 0f, w));
                    lines.Add(line);
                }
            }

            if (lines.Count == 0)
                return lines;

            float totalH = lines.Count * lineHeight;
            float startBaseline;

            switch (sf.LineAlignment)
            {
                case StringAlignment.Center:
                    startBaseline = layoutRect.Y + (layoutRect.Height - totalH) / 2f + ascender;
                    break;
                case StringAlignment.Far:
                    startBaseline = layoutRect.Y + layoutRect.Height - totalH + ascender;
                    break;
                default:
                    startBaseline = layoutRect.Y + ascender;
                    break;
            }

            for (int i = 0; i < lines.Count; i++)
            {
                float bl = startBaseline + i * lineHeight;
                lines[i].Baseline = bl;
                foreach (var run in lines[i].Runs)
                    run.Baseline = bl;
            }

            foreach (var line in lines)
            {
                float lineX;
                switch (sf.Alignment)
                {
                    case StringAlignment.Center:
                        lineX = layoutRect.X + (layoutRect.Width - line.Width) / 2f;
                        break;
                    case StringAlignment.Far:
                        lineX = layoutRect.X + layoutRect.Width - line.Width;
                        break;
                    default:
                        lineX = layoutRect.X;
                        break;
                }

                float curX = lineX;
                foreach (var run in line.Runs)
                {
                    run.X = curX;
                    curX += run.Width;
                }
            }

            return lines;
        }

        // ── Multi-run layout (rich text / bidi / fallback) ──────────────────

        public static List<TextLine> Layout(
            List<ResolvedRun> runs,
            RectangleF        layoutRect,
            StringFormat?     sf,
            float             dpiX,
            float             dpiY)
        {
            if (runs == null || runs.Count == 0)
                return new List<TextLine>();

            sf = sf ?? StringFormat.GenericDefault;

            var runData = new List<RunMetrics>(runs.Count);
            foreach (var run in runs)
            {
                var metrics    = run.Font.FontFamily.GetMetricsData(run.Font.Style);
                float emPixels = FontSizeToPixels(run.Font.Size, run.Font.Unit, dpiY);
                float emScale  = metrics.UnitsPerEm > 0
                    ? (float)(emPixels / metrics.UnitsPerEm)
                    : 1f;

                double rawDesc = metrics.Descender <= 0 ? -metrics.Descender : metrics.Descender;
                float  ascent  = (float)(metrics.Ascender * emScale);
                float  descent = (float)(rawDesc * emScale);
                float  lineGap = (float)(metrics.LineGap * emScale);

                runData.Add(new RunMetrics
                {
                    Font = run.Font,
                    FontFace = run.Font.FontFamily.GetFontFace(run.Font.Style),
                    EmScale = emScale,
                    EmPixels = emPixels,
                    Ascent = ascent,
                    Descent = descent,
                    LineGap = lineGap,
                    FallbackWidth = emPixels * DefaultCharWidthFactor,
                    Brush = run.Brush,
                    BidiLevel = run.BidiLevel,
                    IsRTL = run.IsRTL,
                });
            }

            float firstTabOffset;
            float[] tabStops = sf.GetTabStops(out firstTabOffset);
            bool noWrap = (sf.FormatFlags & StringFormatFlags.NoWrap) != 0;
            float maxW = layoutRect.Width > 0f ? layoutRect.Width : float.MaxValue;

            var lines = new List<TextLine>();

            var flatChars = new List<(int RunIdx, int CharIdx, char C, float Advance, bool IsBreak)>();
            for (int ri = 0; ri < runs.Count; ri++)
            {
                var rd = runData[ri];
                var runText = runs[ri].Text;
                for (int ci = 0; ci < runText.Length; ci++)
                {
                    char c = runText[ci];
                    float adv = GetCharAdvance(c, 0f, rd.FontFace, rd.EmScale,
                                               rd.FallbackWidth, firstTabOffset, tabStops, rd.EmPixels);
                    bool isBreak = c == '\r' || c == '\n';
                    flatChars.Add((ri, ci, c, adv, isBreak));
                }
            }

            var paragraphs = new List<List<(int, int, char, float, bool)>>();
            var curPara = new List<(int, int, char, float, bool)>();
            bool skipNextLF = false;
            foreach (var fc in flatChars)
            {
                if (fc.IsBreak)
                {
                    if (fc.C == '\r') { skipNextLF = true; }
                    else if (fc.C == '\n' && skipNextLF) { skipNextLF = false; continue; }
                    skipNextLF = fc.C == '\r';
                    paragraphs.Add(curPara);
                    curPara = new List<(int, int, char, float, bool)>();
                }
                else
                {
                    curPara.Add(fc);
                    skipNextLF = false;
                }
            }
            paragraphs.Add(curPara);

            foreach (var para in paragraphs)
            {
                if (para.Count == 0)
                {
                    var rd0 = runData[0];
                    lines.Add(new TextLine
                    {
                        Width = 0,
                        LineHeight = rd0.Ascent + rd0.Descent + rd0.LineGap,
                        Ascent = rd0.Ascent,
                        Descent = rd0.Descent
                    });
                    continue;
                }

                if (noWrap || maxW <= 0f)
                {
                    AddLineFromFlat(para, runs, runData, firstTabOffset, tabStops, lines);
                    continue;
                }

                int start = 0;
                while (start < para.Count)
                {
                    int end = FindLineEndMulti(para, start, maxW);
                    var lineChars = para.GetRange(start, end - start);
                    if (end < para.Count && para[end].Item3 == ' ')
                        end++;
                    AddLineFromFlat(lineChars, runs, runData, firstTabOffset, tabStops, lines);
                    start = end;
                }
            }

            if (lines.Count == 0)
                return lines;

            foreach (var line in lines)
                ApplyBidiL2(line);

            foreach (var line in lines)
            {
                float maxA = 0, maxD = 0, maxG = 0;
                foreach (var run in line.Runs)
                {
                    var rd = FindRunMetrics(runData, run.Font);
                    if (rd != null)
                    {
                        var r = rd.Value;
                        if (r.Ascent > maxA) maxA = r.Ascent;
                        if (r.Descent > maxD) maxD = r.Descent;
                        if (r.LineGap > maxG) maxG = r.LineGap;
                    }
                }
                line.Ascent = maxA;
                line.Descent = maxD;
                line.LineHeight = maxA + maxD + maxG;
                if (line.LineHeight < 1f) line.LineHeight = 12f;
            }

            float totalH = 0;
            foreach (var line in lines)
                totalH += line.LineHeight;

            float startBaselineY;
            switch (sf.LineAlignment)
            {
                case StringAlignment.Center:
                    startBaselineY = layoutRect.Y + (layoutRect.Height - totalH) / 2f + lines[0].Ascent;
                    break;
                case StringAlignment.Far:
                    startBaselineY = layoutRect.Y + layoutRect.Height - totalH + lines[0].Ascent;
                    break;
                default:
                    startBaselineY = layoutRect.Y + lines[0].Ascent;
                    break;
            }

            float curY = startBaselineY;
            for (int i = 0; i < lines.Count; i++)
            {
                lines[i].Baseline = curY;
                foreach (var run in lines[i].Runs)
                    run.Baseline = curY;
                curY += lines[i].LineHeight;
            }

            bool paragraphRTL = (sf.FormatFlags & StringFormatFlags.DirectionRightToLeft) != 0;
            foreach (var line in lines)
            {
                float lineX;
                if (paragraphRTL)
                {
                    switch (sf.Alignment)
                    {
                        case StringAlignment.Center:
                            lineX = layoutRect.X + (layoutRect.Width - line.Width) / 2f;
                            break;
                        case StringAlignment.Far:
                            lineX = layoutRect.X;
                            break;
                        default:
                            lineX = layoutRect.X + layoutRect.Width - line.Width;
                            break;
                    }
                }
                else
                {
                    switch (sf.Alignment)
                    {
                        case StringAlignment.Center:
                            lineX = layoutRect.X + (layoutRect.Width - line.Width) / 2f;
                            break;
                        case StringAlignment.Far:
                            lineX = layoutRect.X + layoutRect.Width - line.Width;
                            break;
                        default:
                            lineX = layoutRect.X;
                            break;
                    }
                }

                float curX = lineX;
                foreach (var run in line.Runs)
                {
                    run.X = curX;
                    curX += run.Width;
                }
            }

            return lines;
        }

        private struct RunMetrics
        {
            public Font Font;
            public IFont? FontFace;
            public float EmScale;
            public float EmPixels;
            public float Ascent;
            public float Descent;
            public float LineGap;
            public float FallbackWidth;
            public Brush? Brush;
            public int BidiLevel;
            public bool IsRTL;
        }

        private static RunMetrics? FindRunMetrics(List<RunMetrics> runData, Font font)
        {
            foreach (var rd in runData)
            {
                if (ReferenceEquals(rd.Font, font))
                    return rd;
            }
            return null;
        }

        private static int FindLineEndMulti(
            List<(int RunIdx, int CharIdx, char C, float Advance, bool IsBreak)> para,
            int startIdx,
            float maxWidth)
        {
            float x = 0f;
            int lastSpace = startIdx;

            for (int i = startIdx; i < para.Count; i++)
            {
                var (_, _, c, adv, _) = para[i];
                x += adv;

                if (c == ' ')
                    lastSpace = i;

                if (x > maxWidth && i > startIdx)
                    return lastSpace > startIdx ? lastSpace + 1 : i;
            }

            return para.Count;
        }

        private static void AddLineFromFlat(
            List<(int RunIdx, int CharIdx, char C, float Advance, bool IsBreak)> lineChars,
            List<ResolvedRun> runs,
            List<RunMetrics> runData,
            float firstTabOffset,
            float[] tabStops,
            List<TextLine> lines)
        {
            var line = new TextLine();
            float lineW = 0f;

            int i = 0;
            while (i < lineChars.Count)
            {
                int runIdx = lineChars[i].RunIdx;
                int segStart = i;
                while (i < lineChars.Count && lineChars[i].RunIdx == runIdx)
                    i++;
                int segEnd = i;

                var runText = runs[runIdx].Text;
                int charStart = lineChars[segStart].CharIdx;
                int charLen = lineChars[segEnd - 1].CharIdx - charStart + 1;

                var subText = runText.Substring(charStart, charLen);
                var rd = runData[runIdx];

                float segW = 0f;
                foreach (char c in subText)
                    segW += GetCharAdvance(c, 0f, rd.FontFace, rd.EmScale,
                                           rd.FallbackWidth, firstTabOffset, tabStops, rd.EmPixels);

                line.Runs.Add(new TextRun(
                    subText, runs[runIdx].Font, rd.Brush, rd.BidiLevel, rd.IsRTL,
                    0f, 0f, segW));
                lineW += segW;
            }

            line.Width = lineW;
            lines.Add(line);
        }

        private static void ApplyBidiL2(TextLine line)
        {
            if (line.Runs.Count <= 1)
                return;

            int maxLevel = 0;
            int minLevel = int.MaxValue;
            foreach (var run in line.Runs)
            {
                if (run.BidiLevel > maxLevel) maxLevel = run.BidiLevel;
                if (run.BidiLevel < minLevel) minLevel = run.BidiLevel;
            }

            for (int level = maxLevel; level > minLevel; level--)
            {
                int start = -1;
                for (int i = 0; i < line.Runs.Count; i++)
                {
                    if (line.Runs[i].BidiLevel >= level)
                    {
                        if (start < 0) start = i;
                    }
                    else
                    {
                        if (start >= 0)
                        {
                            ReverseRange(line.Runs, start, i - 1);
                            start = -1;
                        }
                    }
                }
                if (start >= 0)
                    ReverseRange(line.Runs, start, line.Runs.Count - 1);
            }
        }

        private static void ReverseRange(List<TextRun> runs, int lo, int hi)
        {
            while (lo < hi)
            {
                var tmp = runs[lo];
                runs[lo] = runs[hi];
                runs[hi] = tmp;
                lo++;
                hi--;
            }
        }

        // ── Advance-width helper ─────────────────────────────────────────────

        internal static float GetCharAdvance(
            char    c,
            float   currentX,
            IFont?  fontFace,
            float   emScale,
            float   fallbackWidth,
            float   firstTabOffset,
            float[] tabStops,
            float   emPixels)
        {
            if (c == '\t')
                return AdvanceToNextTabStop(currentX, firstTabOffset, tabStops, emPixels) - currentX;

            if (fontFace != null)
            {
                var gid = fontFace.Encoding.DecodeToGid(c);
                if (gid != null)
                    return (float)(fontFace.Metrics.GetGlyphWidth(gid) * emScale);
            }

            return fallbackWidth;
        }

        // ── Font-size conversion ─────────────────────────────────────────────

        internal static float FontSizeToPixels(float size, GraphicsUnit unit, float dpi)
        {
            switch (unit)
            {
                case GraphicsUnit.Pixel:      return size;
                case GraphicsUnit.Point:      return size * dpi / 72f;
                case GraphicsUnit.Inch:       return size * dpi;
                case GraphicsUnit.Document:   return size * dpi / 300f;
                case GraphicsUnit.Millimeter: return size * dpi / 25.4f;
                default:                      return size;
            }
        }

        // ── Private helpers ──────────────────────────────────────────────────

        private static List<string> SplitOnHardBreaks(string text)
        {
            var result = new List<string>();
            int start = 0;
            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] == '\r')
                {
                    result.Add(text.Substring(start, i - start));
                    if (i + 1 < text.Length && text[i + 1] == '\n') i++;
                    start = i + 1;
                }
                else if (text[i] == '\n')
                {
                    result.Add(text.Substring(start, i - start));
                    start = i + 1;
                }
            }
            result.Add(text.Substring(start));
            return result;
        }

        private static List<string> WordWrap(
            string  text,
            IFont?  fontFace,
            float   emScale,
            float   fallbackWidth,
            float   maxWidth,
            float   firstTabOffset,
            float[] tabStops,
            float   emPixels)
        {
            var lines = new List<string>();
            if (maxWidth <= 0f || string.IsNullOrEmpty(text))
            {
                lines.Add(text ?? string.Empty);
                return lines;
            }

            int start = 0;
            while (start < text.Length)
            {
                int end = FindLineEnd(text, start, fontFace, emScale, fallbackWidth,
                                      maxWidth, firstTabOffset, tabStops, emPixels);
                lines.Add(text.Substring(start, end - start));
                start = end;
                if (start < text.Length && text[start] == ' ')
                    start++;
            }

            if (lines.Count == 0)
                lines.Add(string.Empty);

            return lines;
        }

        private static int FindLineEnd(
            string  text,
            int     startIdx,
            IFont?  fontFace,
            float   emScale,
            float   fallbackWidth,
            float   maxWidth,
            float   firstTabOffset,
            float[] tabStops,
            float   emPixels)
        {
            float x = 0f;
            int lastSpace = startIdx;

            for (int i = startIdx; i < text.Length; i++)
            {
                char c = text[i];
                float adv = GetCharAdvance(c, x, fontFace, emScale, fallbackWidth,
                                           firstTabOffset, tabStops, emPixels);
                x += adv;

                if (c == ' ') lastSpace = i;

                if (x > maxWidth && i > startIdx)
                    return lastSpace > startIdx ? lastSpace : i;
            }

            return text.Length;
        }

        private static float MeasureLineWidth(
            string  text,
            IFont?  fontFace,
            float   emScale,
            float   fallbackWidth,
            float   firstTabOffset,
            float[] tabStops,
            float   emPixels)
        {
            float x = 0f;
            foreach (char c in text)
                x += GetCharAdvance(c, x, fontFace, emScale, fallbackWidth,
                                    firstTabOffset, tabStops, emPixels);
            return x;
        }

        private static float AdvanceToNextTabStop(
            float   x,
            float   firstTabOffset,
            float[] tabStops,
            float   emPixels)
        {
            float defaultInterval = emPixels * 4f;
            if (defaultInterval <= 0f) defaultInterval = 48f;

            if (tabStops == null || tabStops.Length == 0)
                return (float)(Math.Floor(x / defaultInterval) + 1.0) * defaultInterval;

            float pos = firstTabOffset;
            if (x < pos) return pos;

            for (int i = 0; i < tabStops.Length; i++)
            {
                pos += tabStops[i];
                if (x < pos) return pos;
            }

            float lastInterval = tabStops[tabStops.Length - 1];
            if (lastInterval <= 0f) lastInterval = defaultInterval;
            float over = x - pos;
            return pos + ((float)Math.Floor(over / lastInterval) + 1f) * lastInterval;
        }
    }
}