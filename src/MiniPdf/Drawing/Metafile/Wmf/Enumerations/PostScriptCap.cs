namespace MiniSoftware.Drawing.Metafile.Wmf.Enumerations
{
    /// <summary>
    /// Defines line-ending types for use with a PostScript printer driver.
    /// </summary>
    internal enum PostScriptCap
    {
        /// <summary>
        ///     Specifies that the line-ending style has not been set, and that a default style MAY <24> be used.
        /// </summary>
        PostScriptNotSet = -2,

        /// <summary>
        ///     Specifies that the line ends at the last point. The end is squared off.
        /// </summary>
        PostScriptFlatCap = 0,

        /// <summary>
        ///     Specifies a circular cap. The center of the circle is the last point in the line. The diameter of the circle is the same as the line width; that is, the thickness of the line.
        /// </summary>
        PostScriptRoundCap = 1,

        /// <summary>
        ///     Specifies a square cap. The center of the square is the last point in the line. 
        ///     The height and width of the square are the same as the line width; that is, the thickness of the line.
        /// </summary>
        PostScriptSquareCap = 2
    }
}
