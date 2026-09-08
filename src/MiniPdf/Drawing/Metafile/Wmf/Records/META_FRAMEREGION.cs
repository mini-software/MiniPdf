using MiniSoftware.Drawing.Metafile.Wmf;

namespace MiniSoftware.Drawing.Metafile.Wmf.Records
{
    using MiniSoftware.Drawing.Metafile.Wmf.Enumerations;
    using System.IO;

    /// <summary>
    ///     The META_FRAMEREGION record draws a border around a specified region using a specified brush.
    /// </summary>
    /// <remarks>
    ///     2.3.3.8 META_FRAMEREGION Record
    /// </remarks>
    internal class META_FRAMEREGION : Record
    {
        /// <summary>
        ///     Used to index into the WMF Object Table to get the region to be framed.
        /// </summary>
        public ushort Region { get; set; }

        /// <summary>
        ///     Used to index into the WMF Object Table to get the Brush to use for filling the region.
        /// </summary>
        public ushort Brush { get; set; }

        /// <summary>
        ///     Defines the height, in logical units, of the region frame.
        /// </summary>
        public ushort Height { get; set; }

        /// <summary>
        ///     Defines the width, in logical units, of the region frame.
        /// </summary>
        public ushort Width { get; set; }

        public META_FRAMEREGION()
        {
            Header.RecordFunction = RecordType.META_FRAMEREGION;
        }

        public override void Read(BinaryReader reader)
        {
            Region = reader.ReadUInt16();
            Brush = reader.ReadUInt16();
            Height = reader.ReadUInt16();
            Width = reader.ReadUInt16();
        }

        public override void Write(BinaryWriter writer)
        {
            base.Write(writer);
            writer.Write(Region);
            writer.Write(Brush);
            writer.Write(Height);
            writer.Write(Width);
        }
    }
}
