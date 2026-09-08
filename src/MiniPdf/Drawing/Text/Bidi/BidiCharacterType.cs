namespace MiniSoftware.Drawing.Text.Bidi
{
    /// <summary>
    /// Unicode bidirectional character types (UAX #9 §4.2).
    /// </summary>
    internal enum BidiCharacterType : byte
    {
        /// <summary>Left-to-Right (strong).</summary>
        L = 0,

        /// <summary>Right-to-Left (strong).</summary>
        R = 1,

        /// <summary>Right-to-Left Arabic (strong).</summary>
        AL = 2,

        /// <summary>European Number (weak).</summary>
        EN = 3,

        /// <summary>European Separator (weak).</summary>
        ES = 4,

        /// <summary>European Terminator (weak).</summary>
        ET = 5,

        /// <summary>Common Number Separator (weak).</summary>
        AN = 6,

        /// <summary>Common Separator (weak).</summary>
        CS = 7,

        /// <summary>Nonspacing Mark (weak).</summary>
        NSM = 8,

        /// <summary>Boundary Neutral (weak).</summary>
        BN = 9,

        /// <summary>Paragraph Separator (neutral).</summary>
        B = 10,

        /// <summary>Segment Separator (neutral).</summary>
        S = 11,

        /// <summary>Whitespace (neutral).</summary>
        WS = 12,

        /// <summary>Other Neutral (neutral).</summary>
        ON = 13,

        /// <summary>Left-to-Right Embedding (format, LRE).</summary>
        LRE = 14,

        /// <summary>Right-to-Left Embedding (format, RLE).</summary>
        RLE = 15,

        /// <summary>Left-to-Right Override (format, LRO).</summary>
        LRO = 16,

        /// <summary>Right-to-Left Override (format, RLO).</summary>
        RLO = 17,

        /// <summary>Pop Directional Format (format, PDF).</summary>
        PDF = 18,

        /// <summary>Left-to-Right Isolate (format, LRI).</summary>
        LRI = 19,

        /// <summary>Right-to-Left Isolate (format, RLI).</summary>
        RLI = 20,

        /// <summary>First Strong Isolate (format, FSI).</summary>
        FSI = 21,

        /// <summary>Pop Directional Isolate (format, PDI).</summary>
        PDI = 22,
    }
}