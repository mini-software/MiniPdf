using MiniSoftware.Drawing.Brushes;

namespace MiniSoftware.Drawing.Text
{
    /// <summary>
    /// A text run after rich-run, bidi, and fallback resolution. Each
    /// <see cref="ResolvedRun"/> has a single font, single brush, single
    /// direction, and single bidi level.
    /// </summary>
    internal sealed class ResolvedRun
    {
        internal ResolvedRun(
            string    text,
            Font      font,
            Brush?    brush,
            int       bidiLevel,
            int       sourceStart)
        {
            Text       = text;
            Font       = font;
            Brush      = brush;
            BidiLevel  = bidiLevel;
            IsRTL      = (bidiLevel & 1) == 1;
            SourceStart = sourceStart;
        }

        /// <summary>The characters in this run (a substring of the source text).</summary>
        public string  Text        { get; }

        /// <summary>Font used for this run (may differ from the default when fallback or rich overrides apply).</summary>
        public Font    Font        { get; }

        /// <summary>Foreground brush for this run, or null to use the default brush.</summary>
        public Brush?  Brush       { get; }

        /// <summary>Unicode bidirectional embedding level (even = LTR, odd = RTL).</summary>
        public int     BidiLevel   { get; }

        /// <summary>True when this run is right-to-left (bidiLevel is odd).</summary>
        public bool    IsRTL       { get; }

        /// <summary>Start index of this run's text within the original source string.</summary>
        public int     SourceStart { get; }
    }
}