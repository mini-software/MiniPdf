namespace MiniPdf.Drawing.Metafile.Wmf.Enumerations
{
    /// <summary>
    /// Defines line-joining capabilities for use with a PostScript printer driver.
    /// </summary>
    internal enum PostScriptJoin
    {
        /// <summary>
        ///     Specifies that the line-joining style has not been set, and that a default style MAY <28> be used.
        /// </summary>
        PostScriptNotSet = -2,

        /// <summary>
        ///     Specifies a mitered join. This value MUST produce a sharp or clipped corner.
        /// </summary>
        PostScriptMiterJoin = 0,

        /// <summary>
        ///     Specifies a circular join. This value MUST produce a smooth, circular arc between the lines.
        /// </summary>
        PostScriptRoundJoin = 1,

        /// <summary>
        ///     Specifies a beveled join. This value MUST produce a diagonal corner.
        /// </summary>
        PostScriptBevelJoin = 2
    }
}
