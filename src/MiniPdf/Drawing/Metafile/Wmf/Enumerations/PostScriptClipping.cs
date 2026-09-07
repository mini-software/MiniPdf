namespace MiniSoftware.Drawing.Metafile.Wmf.Enumerations
{
    /// <summary>
    ///     Defines functions that can be applied to the clipping path used for PostScript output.
    /// </summary>
    /// <remarks>
    ///     2.1.1.27 PostScriptClipping Enumeration
    /// </remarks>
    internal enum PostScriptClipping
    {
        /// <summary>
        ///     Saves the current PostScript clipping path.
        /// </summary>
        CLIP_SAVE = 0x0000,

        /// <summary>
        ///     Restores the PostScript clipping path to the last clipping path that was saved by a previous CLIP_SAVE function applied by a CLIP_TO_PATH record (section 2.3.6.6).
        /// </summary>
        CLIP_RESTORE = 0x0001,

        /// <summary>
        ///     Intersects the current PostScript clipping path with the current clipping path and saves the result as the new PostScript clipping path.
        /// </summary>
        CLIP_INCLUSIVE = 0x0002
    }
}
