namespace MiniSoftware.Drawing.Text.Native
{
    /// <summary>
    /// Describes a single font face discovered via a native operating-system API.
    /// </summary>
    internal readonly struct NativeFontInfo
    {
        /// <summary>
        /// The font family name as reported by the operating system.
        /// </summary>
        public string FamilyName { get; }

        /// <summary>
        /// The absolute file system path to the font file.
        /// </summary>
        public string FilePath { get; }

        public NativeFontInfo(string familyName, string filePath)
        {
            FamilyName = familyName;
            FilePath = filePath;
        }
    }
}