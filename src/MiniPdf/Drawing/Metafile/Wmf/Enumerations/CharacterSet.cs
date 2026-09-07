namespace MiniPdf.Drawing.Metafile.Wmf.Enumerations
{
    /// <summary>
    ///     Defines the possible sets of character glyphs that are defined in fonts for graphics output.
    /// </summary>
    /// <remarks>
    ///     2.1.1.5 CharacterSet Enumeration
    /// </remarks>
    internal enum CharacterSet
    {
        /// <summary>
        ///     Specifies the English character set.
        /// </summary>
        ANSI_CHARSET = 0x00000000,

        /// <summary>
        ///     Specifies a character set based on the current system locale; for example, when the system locale is United States English, the default character set is ANSI_CHARSET.
        /// </summary>
        DEFAULT_CHARSET = 0x00000001,

        /// <summary>
        ///     Specifies a character set of symbols.
        /// </summary>
        SYMBOL_CHARSET = 0x00000002,

        /// <summary>
        ///     Specifies the Apple Macintosh character set.
        /// </summary>
        MAC_CHARSET = 0x0000004D,

        /// <summary>
        ///     Specifies the Japanese character set.
        /// </summary>
        SHIFTJIS_CHARSET = 0x00000080,

        /// <summary>
        ///     Specifies the Hangul Korean character set.
        /// </summary>
        HANGUL_CHARSET = 0x00000081,
        JOHAB_CHARSET = 0x00000082,
        GB2312_CHARSET = 0x00000086,
        CHINESEBIG5_CHARSET = 0x00000088,
        GREEK_CHARSET = 0x000000A1,
        TURKISH_CHARSET = 0x000000A2,
        VIETNAMESE_CHARSET = 0x000000A3,
        HEBREW_CHARSET = 0x000000B1,
        ARABIC_CHARSET = 0x000000B2,
        BALTIC_CHARSET = 0x000000BA,
        RUSSIAN_CHARSET = 0x000000CC,
        THAI_CHARSET = 0x000000DE,
        EASTEUROPE_CHARSET = 0x000000EE,
        OEM_CHARSET = 0x000000FF
    }
}
