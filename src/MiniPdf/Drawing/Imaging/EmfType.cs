namespace MiniSoftware.Drawing.Imaging
{
    /// <summary>
    /// Specifies the type of Enhanced Metafile (EMF) to create.
    /// </summary>
    public enum EmfType
    {
        /// <summary>
        /// Enhanced Metafile only (no EMF+).
        /// </summary>
        EmfOnly,

        /// <summary>
        /// EMF+ only (no original EMF records).
        /// </summary>
        EmfPlusOnly,

        /// <summary>
        /// Both EMF and EMF+ records.
        /// </summary>
        EmfPlusDual,
    }
}
