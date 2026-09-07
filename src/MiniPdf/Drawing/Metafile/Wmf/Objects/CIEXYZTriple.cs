namespace MiniSoftware.Drawing.Metafile.Wmf.Objects
{
    using System.IO;

    /// <summary>
    ///     The CIEXYZTriple Object defines information about the CIEXYZTriple color object.
    /// </summary>
    /// <remarks>
    ///     2.2.2.7 CIEXYZTriple Object
    /// </remarks>
    internal struct CIEXYZTriple
    {
        /// <summary>
        ///     A 96-bit CIEXYZ Object that defines the red chromaticity values.
        /// </summary>
        public CIEXYZ ciexyzRed;

        /// <summary>
        ///     A 96-bit CIEXYZ Object that defines the green chromaticity values.
        /// </summary>
        public CIEXYZ ciexyzGreen;

        /// <summary>
        ///     A 96-bit CIEXYZ Object that defines the blue chromaticity values.
        /// </summary>
        public CIEXYZ ciexyzBlue;

        public void Read(BinaryReader reader)
        {
            this.ciexyzRed.Read(reader);
            this.ciexyzGreen.Read(reader);
            this.ciexyzBlue.Read(reader);
        }

        public void Write(BinaryWriter writer)
        {
            this.ciexyzRed.Write(writer);
            this.ciexyzGreen.Write(writer);
            this.ciexyzBlue.Write(writer);
        }
    }
}
