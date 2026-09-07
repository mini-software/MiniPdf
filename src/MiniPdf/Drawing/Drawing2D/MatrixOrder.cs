namespace MiniSoftware.Drawing.Drawing2D
{
    /// <summary>
    /// Specifies the order for matrix operations.
    /// </summary>
    public enum MatrixOrder
    {
        /// <summary>
        /// The new operation is applied before the existing transformation.
        /// </summary>
        Prepend = 0,

        /// <summary>
        /// The new operation is applied after the existing transformation.
        /// </summary>
        Append  = 1,
    }
}
