using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MiniSoftware.Drawing.Metafile.Wmf;
using MiniSoftware.Drawing.Metafile.Wmf.Enumerations;

namespace MiniSoftware.Drawing.Metafile.Wmf.Records
{
    using System.IO;

    /// <summary>
    ///     The META_TEXTOUT record outputs a character string at the specified location by using the font, background color, and text color that are defined in the playback device context.
    /// </summary>
    /// <remarks>
    ///     2.3.3.20 META_TEXTOUT Record
    /// </remarks>
    internal class META_TEXTOUT : Record
    {
        /// <summary>
        ///     Defines the length of the string, in bytes, pointed to by String.
        /// </summary>
        public short StringLength;

        /// <summary>
        ///     The size of this field MUST be a multiple of two. If StringLength is an odd number, 
        ///     then this field MUST be of a size greater than or equal to StringLength + 1. 
        ///     A variable-length string that specifies the text to be drawn. 
        ///     The string does not need to be null-terminated, because StringLength specifies the length of the string. 
        ///     The string is written at the location specified by the XStart and YStart fields.
        /// </summary>
        public string String = string.Empty;

        public META_TEXTOUT()
        {
            Header.RecordFunction = RecordType.META_TEXTOUT;
        }

        /// <summary>
        ///     Defines the vertical (y-axis) coordinate, in logical units, of the point where drawing is to start.
        /// </summary>
        public short YStart;

        /// <summary>
        ///     Defines the horizontal (x-axis) coordinate, in logical units, of the point where drawing is to start.
        /// </summary>
        public short XStart;

        public override void Read(BinaryReader reader)
        {
            this.StringLength = reader.ReadInt16();
            this.String = Encoding.ASCII.GetString(reader.ReadBytes(this.StringLength));
            this.YStart = reader.ReadInt16();
            this.XStart = reader.ReadInt16();
        }

        public override void Write(BinaryWriter writer)
        {
            writer.Write(this.StringLength);
            writer.Write(Encoding.ASCII.GetBytes(this.String));
            writer.Write(this.YStart);
            writer.Write(this.XStart);
        }
    }
}
