namespace MiniSoftware.Drawing.Text
{
    /// <summary>
    /// Specifies the substitution method for digit shapes in text.
    /// </summary>
    public enum StringDigitSubstitute
    {
        /// <summary>
        /// User-defined digit substitution.
        /// </summary>
        User = 0,

        /// <summary>
        /// No digit substitution.
        /// </summary>
        None = 1,

        /// <summary>
        /// National digit substitution.
        /// </summary>
        National = 2,

        /// <summary>
        /// Traditional digit substitution.
        /// </summary>
        Traditional = 3
    }
}