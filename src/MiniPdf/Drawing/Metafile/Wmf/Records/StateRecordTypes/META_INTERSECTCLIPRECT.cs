namespace MiniSoftware.Drawing.Metafile.Wmf.Records.StateRecordTypes
{
    using System.IO;

    /// <summary>
    ///     The META_INTERSECTCLIPRECT record sets the clipping region in the playback device context to the intersection of the existing clipping region and the specified rectangle.
    /// </summary>
    /// <remarks>
    ///     2.3.5.3 META_INTERSECTCLIPRECT Record
    /// </remarks>
    internal class META_INTERSECTCLIPRECT : Record
    {
        /// <summary>
        ///     Defines the y-coordinate, in logical units, of the lower-right corner of the rectangle.
        /// </summary>
        public ushort Bottom;

        /// <summary>
        ///     Defines the x-coordinate, in logical units, of the lower-right corner of the rectangle.
        /// </summary>
        public ushort Right;

        /// <summary>
        ///     Defines the y-coordinate, in logical units, of the upper-left corner of the rectangle.
        /// </summary>
        public ushort Top;

        /// <summary>
        ///     Defines the x-coordinate, in logical units, of the upper-left corner of the rectangle.
        /// </summary>
        public ushort Left;

        public override void Read(BinaryReader reader)
        {
            this.Bottom = reader.ReadUInt16();
            this.Right = reader.ReadUInt16();
            this.Top = reader.ReadUInt16();
            this.Left = reader.ReadUInt16();
        }
    }
}
