namespace MiniSoftware.Drawing.Metafile.Emf.Records.DrawingRecordTypes
{
    using System.IO;

    using MiniSoftware.Drawing.Metafile.Wmf.Objects;

    /// <summary>
    ///     The EMR_ARC record specifies an elliptical arc.
    /// </summary>
    /// <remarks>
    ///     2.3.5.2 EMR_ARC Record
    /// </remarks>
    internal class EMR_ARC : Record
    {
        /// <summary>
        ///     A 128-bit WMF RectL object, specified in [MS-WMF] section 2.2.2.19, which specifies the inclusive-inclusive bounding rectangle.
        /// </summary>
        public RectL Box;

        /// <summary>
        ///     A 64-bit WMF PointL object, specified in [MS-WMF] section 2.2.2.15, which specifies the coordinates, in logical units, of the ending point of the radial line defining the starting point of the arc.
        /// </summary>
        public PointL Start;

        /// <summary>
        ///     A 64-bit WMF PointL object that specifies the coordinates, in logical units, of the ending point of the radial line defining the ending point of the arc.
        /// </summary>
        public PointL End;

        public override void Read(BinaryReader reader)
        {
            this.Box = new RectL();
            this.Box.Read(reader);
            this.Start = new PointL();
            this.Start.Read(reader);
            this.End = new PointL();
            this.End.Read(reader);
        }

        public override void Write(BinaryWriter writer)
        {
            this.Box.Write(writer);
            this.Start.Write(writer);
            this.End.Write(writer);
        }
    }
}
