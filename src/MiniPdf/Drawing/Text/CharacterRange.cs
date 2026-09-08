using System;

namespace MiniSoftware.Drawing.Text
{
    /// <summary>
    /// Specifies a range of character positions within a string.
    /// </summary>
    public struct CharacterRange
    {
        /// <summary>
        /// Initializes a new <see cref="CharacterRange"/> with the specified first character and length.
        /// </summary>
        /// <param name="first">The zero-based position of the first character.</param>
        /// <param name="length">The number of characters in the range.</param>
        public CharacterRange(int first, int length)
        {
            First = first;
            Length = length;
        }

        /// <summary>
        /// Gets or sets the zero-based position of the first character in the range.
        /// </summary>
        public int First { get; set; }

        /// <summary>
        /// Gets or sets the number of characters in the range.
        /// </summary>
        public int Length { get; set; }
    }
}