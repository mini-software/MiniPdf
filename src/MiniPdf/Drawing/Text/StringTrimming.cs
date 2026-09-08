namespace MiniSoftware.Drawing.Text
{
    /// <summary>
    /// Specifies how to trim characters from a string that does not fit into a layout shape.
    /// </summary>
    public enum StringTrimming
    {
        /// <summary>
        /// No trimming.
        /// </summary>
        None = 0,

        /// <summary>
        /// Trim to the nearest character.
        /// </summary>
        Character = 1,

        /// <summary>
        /// Trim to the nearest word.
        /// </summary>
        Word = 2,

        /// <summary>
        /// Trim to the nearest character and append ellipsis.
        /// </summary>
        EllipsisCharacter = 3,

        /// <summary>
        /// Trim to the nearest word and append ellipsis.
        /// </summary>
        EllipsisWord = 4,

        /// <summary>
        /// Trim to the center of the word and append ellipsis.
        /// </summary>
        EllipsisPath = 5
    }
}