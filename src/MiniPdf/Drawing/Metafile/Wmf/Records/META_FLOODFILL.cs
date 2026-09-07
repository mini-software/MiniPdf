using MiniPdf.Drawing.Metafile.Wmf;

namespace MiniPdf.Drawing.Metafile.Wmf.Records
{
    using System.IO;
    using MiniPdf.Drawing.Metafile.Wmf.Enumerations;
    using MiniPdf.Drawing.Metafile.Wmf.Objects;

    /// <summary>
    ///     The META_FLOODFILL record fills an area of the output surface with the brush that is defined in the playback device context.
    /// </summary>
    /// <remarks>
    ///     2.3.3.7 META_FLOODFILL Record
    /// </remarks>
    internal class META_FLOODFILL : Record
    {
        /// <summary>
        ///     A 32-bit ColorRef Object that defines the color value.
        /// </summary>
        public ColorRef ColorRef { get; set; } = new();

        /// <summary>
        ///     Defines the y-coordinate, in logical units, of the point where filling is to start.
        /// </summary>
        public short YStart { get; set; }

        /// <summary>
        ///     Defines the x-coordinate, in logical units, of the point where filling is to start.
        /// </summary>
        public short XStart { get; set; }

        public META_FLOODFILL()
        {
            Header.RecordFunction = RecordType.META_FLOODFILL;
        }

        public override void Read(BinaryReader reader)
        {
            var colorRef = ColorRef;
            colorRef.Read(reader);
            ColorRef = colorRef;
            YStart = reader.ReadInt16();
            XStart = reader.ReadInt16();
        }

        public override void Write(BinaryWriter writer)
        {
            base.Write(writer);
            ColorRef.Write(writer);
            writer.Write(YStart);
            writer.Write(XStart);
        }
    }
}
