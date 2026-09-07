namespace MiniSoftware.Drawing.Metafile.Wmf.Enumerations
{
    /// <summary>
    ///     Defines values that are used to retrieve information about specific features in a PostScript printer driver.
    /// </summary>
    internal enum PostScriptFeatureSetting
    {
        /// <summary>
        ///     Specifies the n-up printing (page layout) setting.
        /// </summary>
        FEATURESETTING_NUP = 0x00000000,

        /// <summary>
        ///     Specifies PostScript driver output options.
        /// </summary>
        FEATURESETTING_OUTPUT = 0x00000001,

        /// <summary>
        ///     Specifies the language level.
        /// </summary>
        FEATURESETTING_PSLEVEL = 0x00000002,

        /// <summary>
        ///     Specifies custom paper parameters.
        /// </summary>
        FEATURESETTING_CUSTPAPER = 0x00000003,

        /// <summary>
        ///     Specifies the mirrored output setting.
        /// </summary>
        FEATURESETTING_MIRROR = 0x00000004,

        /// <summary>
        ///     Specifies the negative output setting.
        /// </summary>
        FEATURESETTING_NEGATIVE = 0x00000005,

        /// <summary>
        ///     Specifies the output protocol setting.
        /// </summary>
        FEATURESETTING_PROTOCOL = 0x00000006,

        /// <summary>
        ///     Specifies the start of a range of values that a driver MAY use for retrieving data concerning proprietary features.
        /// </summary>
        FEATURESETTING_PRIVATE_BEGIN = 0x00001000,

        /// <summary>
        ///     Specifies the end of a range of values that a driver MAY use for retrieving data concerning proprietary features.
        /// </summary>
        FEATURESETTING_PRIVATE_END = 0x00001FFF
    }
}
