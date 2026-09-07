using MiniPdf.Drawing.Metafile.Wmf;

namespace MiniPdf.Drawing.Metafile.Wmf.Objects
{
    using MiniPdf.Drawing.Metafile.Wmf.Enumerations;
    using System.IO;
    using System.Text;

    /// <summary>
    ///     The LogColorSpaceW object specifies a logical color space, which can be defined by a color profile file with a name consisting of Unicode 16-bit characters.
    /// </summary>
    /// <remarks>
    ///     2.2.2.12 LogColorSpaceW Object
    /// </remarks>
    internal class LogColorSpaceW
    {
        private const int FixedSize = 68;

        /// <summary>
        ///     A 32-bit unsigned integer that specifies the signature of color space objects. 
        ///     This MUST be set to the value 0x50534F43, which is the ASCII encoding of the string "PSOC".
        /// </summary>
        public uint Signature { get; set; }

        /// <summary>
        ///     A 32-bit unsigned integer that defines a version number; it MUST be 0x00000400.
        /// </summary>
        public uint Version { get; set; }

        /// <summary>
        ///     A 32-bit unsigned integer that defines the size of this object, in bytes.
        /// </summary>
        public uint Size { get; set; }

        /// <summary>
        ///     A 32-bit signed integer that specifies the color space type. It MUST be defined in the LogicalColorSpace enumeration (section 2.1.1.14). 
        ///     If this value is LCS_sRGB or LCS_WINDOWS_COLOR_SPACE, the sRGB color space MUST be used.
        /// </summary>
        public LogicalColorSpace ColorSpaceType { get; set; }

        /// <summary>
        ///     A 32-bit signed integer that defines the gamut mapping intent. It MUST be defined in the GamutMappingIntent enumeration (section 2.1.1.11).
        /// </summary>
        public GamutMappingIntent Intent { get; set; }

        /// <summary>
        ///     A CIEXYZTriple object (section 2.2.2.7) that defines the CIE chromaticity x, y, and z coordinates of the three colors that correspond to the RGB endpoints for the logical color space associated with the bitmap. 
        ///     If the ColorSpaceType field does not specify LCS_CALIBRATED_RGB, this field MUST be ignored.
        /// </summary>
        public CIEXYZTriple Endpoints { get; set; } = new();

        /// <summary>
        ///     A 32-bit fixed point value that defines the toned response curve for red. 
        ///     If the ColorSpaceType field does not specify LCS_CALIBRATED_RGB, this field MUST be ignored.
        /// </summary>
        public uint GammaRed { get; set; }

        /// <summary>
        ///     A 32-bit fixed point value that defines the toned response curve for green.
        ///      If the ColorSpaceType field does not specify LCS_CALIBRATED_RGB, this field MUST be ignored.
        /// </summary>
        public uint GammaGreen { get; set; }

        /// <summary>
        ///     A 32-bit fixed point value that defines the toned response curve for blue. 
        ///     If the ColorSpaceType field does not specify LCS_CALIBRATED_RGB, this field MUST be ignored.
        /// </summary>
        public uint GammaBlue { get; set; }

        /// <summary>
        ///     An optional, null-terminated Unicode UTF16-LE character string, which specifies the name of a file that contains a color profile. If a file name is specified, and the ColorSpaceType field is set to LCS_CALIBRATED_RGB, the other fields of this structure SHOULD be ignored.
        /// </summary>
        public string Filename { get; set; } = string.Empty;

        public LogColorSpaceW()
        {
            Signature = 0x50534F43;
            Version = 0x00000400;
        }

        public void Read(BinaryReader reader)
        {
            Signature = reader.ReadUInt32();
            Version = reader.ReadUInt32();
            Size = reader.ReadUInt32();
            ColorSpaceType = (LogicalColorSpace)reader.ReadInt32();
            Intent = (GamutMappingIntent)reader.ReadInt32();
            var endpoints = Endpoints;
            endpoints.Read(reader);
            Endpoints = endpoints;
            GammaRed = reader.ReadUInt32();
            GammaGreen = reader.ReadUInt32();
            GammaBlue = reader.ReadUInt32();

            if (Size > FixedSize)
            {
                byte[] filenameBytes = reader.ReadBytes((int)(Size - FixedSize));
                Filename = Encoding.Unicode.GetString(filenameBytes).TrimEnd('\0');
            }
        }

        public void Write(BinaryWriter writer)
        {
            byte[] filenameBytes = Encoding.Unicode.GetBytes(Filename + "\0");
            Size = (uint)(FixedSize + filenameBytes.Length);

            writer.Write(Signature);
            writer.Write(Version);
            writer.Write(Size);
            writer.Write((int)ColorSpaceType);
            writer.Write((int)Intent);
            Endpoints.Write(writer);
            writer.Write(GammaRed);
            writer.Write(GammaGreen);
            writer.Write(GammaBlue);
            writer.Write(filenameBytes);
        }
    }
}
