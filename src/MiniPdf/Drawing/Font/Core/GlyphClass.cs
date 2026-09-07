namespace MiniSoftware.Drawing.Font.Core
{
    /// <summary>
    /// OpenType glyph classification as defined in the GDEF table (§6.3.1).
    /// Ref: https://learn.microsoft.com/en-us/typography/opentype/spec/gdef
    /// </summary>
    public enum GlyphClass
    {
        /// <summary>Glyph is not assigned to any specific class.</summary>
        Unclassified = 0,

        /// <summary>Base glyph — a single character that may have combining marks attached.</summary>
        Base         = 1,

        /// <summary>Ligature glyph — formed from two or more characters.</summary>
        Ligature     = 2,

        /// <summary>Combining mark glyph.</summary>
        Mark         = 3,

        /// <summary>Component glyph — part of a ligature not individually encoded.</summary>
        Component    = 4,
    }
}
