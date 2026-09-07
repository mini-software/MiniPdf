namespace MiniPdf.Drawing.Metafile
{
    /// <summary>
    /// Provides helper methods for extracting bit fields from integer values.
    /// </summary>
    public static class BitHelper
    {
        /// <summary>
        /// Extracts a specified number of bits from an unsigned integer value.
        /// </summary>
        /// <param name="value">The source value to extract bits from.</param>
        /// <param name="start">The zero-based starting bit position.</param>
        /// <param name="length">The number of bits to extract.</param>
        /// <param name="signed">Whether the extracted value should be treated as signed.</param>
        /// <returns>The extracted bit field value.</returns>
        public static int GetBits(uint value, int start, int length, bool signed)
        {
            int mask = (1 << length) - 1;
            return (int)((value >> start) & mask);
        }
    }

    /// <summary>
    /// Provides helper methods for bit manipulation operations.
    /// </summary>
    public static class BitManipulation
    {
        /// <summary>
        /// Extracts a specified number of bits from a byte value.
        /// </summary>
        /// <param name="value">The source byte to extract bits from.</param>
        /// <param name="start">The zero-based starting bit position.</param>
        /// <param name="length">The number of bits to extract.</param>
        /// <returns>The extracted bit field value.</returns>
        public static int GetBits(byte value, int start, int length)
        {
            int mask = (1 << length) - 1;
            return (value >> start) & mask;
        }
    }
}
