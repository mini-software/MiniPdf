namespace MiniSoftware.Drawing.Text.Bidi
{
    /// <summary>
    /// Paragraph embedding direction for the Unicode Bidirectional Algorithm.
    /// </summary>
    internal enum BidiParagraphDirection
    {
        /// <summary>Left-to-right paragraph (base level 0).</summary>
        LTR,

        /// <summary>Right-to-left paragraph (base level 1).</summary>
        RTL,

        /// <summary>Auto-detect from first strong character (UAX #9 P1).</summary>
        Auto
    }
}