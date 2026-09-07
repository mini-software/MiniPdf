namespace MiniPdf.Drawing.Metafile.Emf.Records.DrawingRecordTypes
{
    using System.IO;

    using MiniPdf.Drawing.Metafile.Wmf.Objects;

    /// <summary>
    ///     The EMR_ARCTO record specifies an elliptical arc. It resets the current position to the end point of the arc
    /// </summary>
    /// <remarks>
    ///     2.3.5.3 EMR_ARCTO Record
    /// </remarks>
    internal class EMR_ARCTO : Record
    {
        /// <summary>
        ///     A 128-bit WMF RectL object, specified in [MS-WMF] section 2.2.2.19, which specifies the inclusive-inclusive bounding rectangle.
        /// </summary>
        public RectL Box;

        /// <summary>
        ///     A 64-bit WMF PointL object, specified in [MS-WMF] section 2.2.2.15, which specifies the coordinates of the first radial ending point, in logical units.
        /// </summary>
        public PointL Start;

        /// <summary>
        ///     A 64-bit WMF PointL object that specifies the coordinates of the second radial ending point, in logical units.
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
