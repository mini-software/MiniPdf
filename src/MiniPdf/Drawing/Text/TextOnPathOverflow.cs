namespace MiniPdf.Drawing.Text
{
    /// <summary>
    /// Controls how text longer than the available path length is handled
    /// by text-on-path layout.
    /// </summary>
    public enum TextOnPathOverflow
    {
        /// <summary>
        /// Place glyphs until the path runs out, then stop. Remaining
        /// characters are not rendered.
        /// </summary>
        Stop = 0,

        /// <summary>
        /// Place glyphs until the path runs out, then clip. Identical
        /// observable behavior to <see cref="Stop"/> in the first cut
        /// (reserved for future wrap-to-next-subpath behavior).
        /// </summary>
        Clip = 1,
    }
}