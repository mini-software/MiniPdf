using MiniPdf.Drawing.Metafile.Wmf;

namespace MiniPdf.Drawing.Metafile.Wmf.Records
{
    using System.IO;
    using MiniPdf.Drawing.Metafile.Wmf.Enumerations;
    using MiniPdf.Drawing.Metafile.Wmf.Objects;

    /// <summary>
    ///     The META_SETPIXEL record sets the pixel at the specified coordinates to the specified color.
    /// </summary>
    /// <remarks>
    ///     2.3.3.19 META_SETPIXEL Record
    /// </remarks>
    internal class META_SETPIXEL : Record
    {
        /// <summary>
        ///     A ColorRef Object (section 2.2.2.8) that defines the color value.
        /// </summary>
        public ColorRef ColorRef { get; set; } = new();

        /// <summary>
        ///     Defines the y-coordinate, in logical units, of the point to be set.
        /// </summary>
        public short Y { get; set; }

        /// <summary>
        ///     Defines the x-coordinate, in logical units, of the point to be set.
        /// </summary>
        public short X { get; set; }

        public META_SETPIXEL()
        {
            Header.RecordFunction = RecordType.META_SETPIXEL;
        }

        public override void Read(BinaryReader reader)
        {
            var colorRef = ColorRef;
            colorRef.Read(reader);
            ColorRef = colorRef;
            Y = reader.ReadInt16();
            X = reader.ReadInt16();
        }

        public override void Write(BinaryWriter writer)
        {
            base.Write(writer);
            ColorRef.Write(writer);
            writer.Write(Y);
            writer.Write(X);
        }
    }
}
