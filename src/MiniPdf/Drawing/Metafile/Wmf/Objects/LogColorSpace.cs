using MiniSoftware.Drawing.Metafile.Wmf;

namespace MiniSoftware.Drawing.Metafile.Wmf.Objects
{
    using MiniSoftware.Drawing.Metafile.Wmf.Enumerations;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;

    /// <summary>
    ///     The LogColorSpace object specifies a logical color space for the playback device context, which can be the name of a color profile in ASCII characters.
    /// </summary>
    /// <remarks>
    ///     2.2.2.11 LogColorSpace Object
    /// </remarks>
    internal class LogColorSpace
    {
        /// <summary>
        ///     A 32-bit unsigned integer that specifies the signature of color space objects. 
        ///     This MUST be set to the value 0x50534F43, which is the ASCII encoding of the string "PSOC".
        /// </summary>
        public uint Signature;

        /// <summary>
        ///     A 32-bit unsigned integer that defines a version number; it MUST be 0x00000400.
        /// </summary>
        public uint Version;

        /// <summary>
        ///     A 32-bit unsigned integer that defines the size of this object, in bytes.
        /// </summary>
        public uint Size;

        /// <summary>
        ///     A 32-bit signed integer that specifies the color space type. It MUST be defined in the LogicalColorSpace enumeration (section 2.1.1.14). 
        ///     If this value is LCS_sRGB or LCS_WINDOWS_COLOR_SPACE, the sRGB color space MUST be used.
        /// </summary>
        public LogicalColorSpace ColorSpaceType;

        /// <summary>
        ///     A 32-bit signed integer that defines the gamut mapping intent. It MUST be defined in the GamutMappingIntent enumeration (section 2.1.1.11).
        /// </summary>
        public GamutMappingIntent Intent;

        /// <summary>
        ///     A CIEXYZTriple object (section 2.2.2.7) that defines the CIE chromaticity x, y, and z coordinates of the three colors that correspond to the RGB endpoints for the logical color space associated with the bitmap. 
        ///     If the ColorSpaceType field does not specify LCS_CALIBRATED_RGB, this field MUST be ignored.
        /// </summary>
        public CIEXYZTriple Endpoints;

        /// <summary>
        ///     A 32-bit fixed point value that defines the toned response curve for red. 
        ///     If the ColorSpaceType field does not specify LCS_CALIBRATED_RGB, this field MUST be ignored.
        /// </summary>
        public decimal GammaRed;

        /// <summary>
        ///     A 32-bit fixed point value that defines the toned response curve for green.
        ///      If the ColorSpaceType field does not specify LCS_CALIBRATED_RGB, this field MUST be ignored.
        /// </summary>
        public decimal GammaGreen;

        /// <summary>
        ///     A 32-bit fixed point value that defines the toned response curve for blue. 
        ///     If the ColorSpaceType field does not specify LCS_CALIBRATED_RGB, this field MUST be ignored.
        /// </summary>
        public decimal GammaBlue;

        /// <summary>
        ///     An optional, ASCII charactger string that specifies the name of a file that contains a color profile. If a file name is specified, and the ColorSpaceType field is set to LCS_CALIBRATED_RGB, the other fields of this structure SHOULD be ignored.
        /// </summary>
        public string? Filename;

        /// <summary>
        /// Reads the LogColorSpace fields from a BinaryReader in the correct order.
        /// </summary>
        public void ReadFrom(BinaryReader reader)
        {
            this.Signature = reader.ReadUInt32();
            this.Version = reader.ReadUInt32();
            this.Size = reader.ReadUInt32();
            this.ColorSpaceType = (LogicalColorSpace)reader.ReadInt32();
            this.Intent = (GamutMappingIntent)reader.ReadInt32();

            if (this.ColorSpaceType == LogicalColorSpace.LCS_CALIBRATED_RGB)
            {
                this.Endpoints = new CIEXYZTriple();
                this.Endpoints.Read(reader);
                this.GammaRed = ReadFixedPoint2_30(reader);
                this.GammaGreen = ReadFixedPoint2_30(reader);
                this.GammaBlue = ReadFixedPoint2_30(reader);

                if (reader.BaseStream.Position < reader.BaseStream.Length)
                {
                    this.Filename = ReadAsciiString(reader);
                }
                else
                {
                    this.Filename = null;
                }
            }
            else
            {
                this.Endpoints = default(CIEXYZTriple);
                this.GammaRed = 0;
                this.GammaGreen = 0;
                this.GammaBlue = 0;
                this.Filename = ReadAsciiString(reader);
            }
        }

        /// <summary>
        /// Writes the LogColorSpace fields to a BinaryWriter in the correct order.
        /// </summary>
        public void Write(BinaryWriter writer)
        {
            writer.Write(this.Signature);
            writer.Write(this.Version);
            writer.Write(this.Size);
            writer.Write((int)this.ColorSpaceType);
            writer.Write((int)this.Intent);

            if (this.ColorSpaceType == LogicalColorSpace.LCS_CALIBRATED_RGB)
            {
                this.Endpoints.Write(writer);
                WriteFixedPoint2_30(writer, this.GammaRed);
                WriteFixedPoint2_30(writer, this.GammaGreen);
                WriteFixedPoint2_30(writer, this.GammaBlue);
                if (!string.IsNullOrEmpty(this.Filename))
                    WriteAsciiString(writer, this.Filename);
            }
            else
            {
                WriteAsciiString(writer, this.Filename ?? string.Empty);
            }
        }

        /// <summary>
        /// Reads a 32-bit 2.30 fixed-point value from the reader and converts to decimal.
        /// </summary>
        private static decimal ReadFixedPoint2_30(BinaryReader reader)
        {
            uint raw = reader.ReadUInt32();
            // 2 bits integer, 30 bits fraction
            return raw / (decimal)(1 << 30);
        }

        /// <summary>
        /// Reads an ASCII string up to a null terminator or end of stream.
        /// </summary>
        private static string ReadAsciiString(BinaryReader reader)
        {
            var bytes = new List<byte>();
            while (reader.BaseStream.Position < reader.BaseStream.Length)
            {
                byte b = reader.ReadByte();
                if (b == 0) break;
                bytes.Add(b);
            }
            return Encoding.ASCII.GetString(bytes.ToArray());
        }

        /// <summary>
        /// Writes a decimal as a 32-bit 2.30 fixed-point value to the writer.
        /// </summary>
        private static void WriteFixedPoint2_30(BinaryWriter writer, decimal value)
        {
            uint raw = (uint)(value * (1 << 30));
            writer.Write(raw);
        }

        /// <summary>
        /// Writes an ASCII string with a null terminator.
        /// </summary>
        private static void WriteAsciiString(BinaryWriter writer, string value)
        {            
            byte[] bytes = Encoding.ASCII.GetBytes(value);
            writer.Write(bytes);
            writer.Write((byte)0); // Null terminator
        }
    }
}
