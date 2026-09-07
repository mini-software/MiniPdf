using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniPdf.Drawing.Metafile.Wmf.Objects
{
    using System.IO;

    /// <summary>
    ///     The CIEXYZ Object defines information about the CIEXYZ chromaticity object.
    /// </summary>
    /// <remarks>
    ///     2.2.2.6 CIEXYZ Object
    /// </remarks>
    internal struct CIEXYZ
    {
        /// <summary>
        ///     A 32-bit 2.30 fixed point type that defines the x chromaticity value.
        /// </summary>
        public uint ciexyzX;

        /// <summary>
        ///     A 32-bit 2.30 fixed point type that defines the y chromaticity value.
        /// </summary>
        public uint ciexyzY;

        /// <summary>
        ///     A 32-bit 2.30 fixed point type that defines the z chromaticity value.
        /// </summary>
        public uint ciexyzZ;

        public void Read(BinaryReader reader)
        {
            this.ciexyzX = reader.ReadUInt32();
            this.ciexyzY = reader.ReadUInt32();
            this.ciexyzZ = reader.ReadUInt32();
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(this.ciexyzX);
            writer.Write(this.ciexyzY);
            writer.Write(this.ciexyzZ);
        }
        //TODO Read 2.30 decimal https://msdn.microsoft.com/en-us/library/t1de0ya1(v=vs.110).aspx
    }
}
