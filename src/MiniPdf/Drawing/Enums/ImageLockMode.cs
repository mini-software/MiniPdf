namespace MiniSoftware.Drawing.Enums
{
    /// <summary>
    /// Specifies the mode for locking image bits.
    /// </summary>
    public enum ImageLockMode
    {
        /// <summary>
        /// Read-only access to image bits.
        /// </summary>
        ReadOnly        = 1,

        /// <summary>
        /// Write-only access to image bits.
        /// </summary>
        WriteOnly       = 2,

        /// <summary>
        /// Read-write access to image bits.
        /// </summary>
        ReadWrite       = 3,

        /// <summary>
        /// User input buffer mode.
        /// </summary>
        UserInputBuffer = 4,
    }
}
