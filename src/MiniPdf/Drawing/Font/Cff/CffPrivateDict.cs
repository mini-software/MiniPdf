namespace MiniPdf.Drawing.Font.Cff
{
    /// <summary>
    /// Parsed contents of a CFF Private DICT.
    /// Holds the default/nominal advance-width values and optional local subroutines.
    /// </summary>
    public sealed class CffPrivateDict
    {
        /// <summary>
        /// Width used when no explicit width is encoded in the charstring.
        /// Corresponds to the CFF <c>defaultWidthX</c> entry (key 20).
        /// </summary>
        public double DefaultWidthX { get; internal set; } = 0.0;

        /// <summary>
        /// Nominal width added to the delta stored in the charstring.
        /// Corresponds to the CFF <c>nominalWidthX</c> entry (key 21).
        /// </summary>
        public double NominalWidthX { get; internal set; } = 0.0;

        /// <summary>
        /// Local subroutines for this Private DICT, or <see langword="null"/>
        /// when the <c>Subrs</c> entry (key 19) is absent.
        /// </summary>
        public byte[][]? LocalSubrs { get; internal set; }
    }
}
