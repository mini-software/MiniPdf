using MiniSoftware.Drawing.Metafile.Wmf;

namespace MiniSoftware.Drawing.Metafile.Wmf.Records
{
    using System.IO;
    using MiniSoftware.Drawing.Metafile.Wmf.Enumerations;
    using MiniSoftware.Drawing.Metafile.Wmf.Objects;

    /// <summary>
    ///     The META_SETDIBTODEV record sets a block of pixels in the playback device context using device-independent color data.
    /// </summary>
    /// <remarks>
    ///     2.3.1.4 META_SETDIBTODEV Record
    /// </remarks>
    internal class META_SETDIBTODEV : Record
    {
        /// <summary>
        ///     Defines whether the Colors field of the DIB contains explicit RGB values or indexes into a palette.
        /// </summary>
        public ColorUsage ColorUsage { get; set; }

        /// <summary>
        ///     Defines the number of scan lines in the source.
        /// </summary>
        public ushort ScanCount { get; set; }

        /// <summary>
        ///     Defines the starting scan line in the source.
        /// </summary>
        public ushort StartScan { get; set; }

        /// <summary>
        ///     Defines the y-coordinate, in logical units, of the source rectangle.
        /// </summary>
        public ushort yDib { get; set; }

        /// <summary>
        ///     Defines the x-coordinate, in logical units, of the source rectangle.
        /// </summary>
        public ushort xDib { get; set; }

        /// <summary>
        ///     Defines the height, in logical units, of the source and destination rectangles.
        /// </summary>
        public ushort Height { get; set; }

        /// <summary>
        ///     Defines the width, in logical units, of the source and destination rectangles.
        /// </summary>
        public ushort Width { get; set; }

        /// <summary>
        ///     Defines the y-coordinate, in logical units, of the upper-left corner of the destination rectangle.
        /// </summary>
        public ushort yDest { get; set; }

        /// <summary>
        ///     Defines the x-coordinate, in logical units, of the upper-left corner of the destination rectangle.
        /// </summary>
        public ushort xDest { get; set; }

        /// <summary>
        ///     A variable-sized DeviceIndependentBitmap Object (section 2.2.2.9) that is the source of the color data.
        /// </summary>
        public DeviceIndependentBitmap DIB { get; set; } = new DeviceIndependentBitmap();

        public META_SETDIBTODEV()
        {
            Header.RecordFunction = RecordType.META_SETDIBTODEV;
        }

        public override void Read(BinaryReader reader)
        {
            ColorUsage = (ColorUsage)reader.ReadUInt16();
            ScanCount = reader.ReadUInt16();
            StartScan = reader.ReadUInt16();
            yDib = reader.ReadUInt16();
            xDib = reader.ReadUInt16();
            Height = reader.ReadUInt16();
            Width = reader.ReadUInt16();
            yDest = reader.ReadUInt16();
            xDest = reader.ReadUInt16();
            DIB.Read(reader);
        }

        public override void Write(BinaryWriter writer)
        {
            base.Write(writer);
            writer.Write((ushort)ColorUsage);
            writer.Write(ScanCount);
            writer.Write(StartScan);
            writer.Write(yDib);
            writer.Write(xDib);
            writer.Write(Height);
            writer.Write(Width);
            writer.Write(yDest);
            writer.Write(xDest);
            DIB.Write(writer);
        }
    }
}
