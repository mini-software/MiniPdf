using MiniSoftware.Drawing.Metafile.Emf.Records;

namespace MiniSoftware.Drawing.Metafile.Emf.Records.DrawingRecordTypes
{
    using System.IO;

    using MiniSoftware.Drawing.Metafile.Wmf.Objects;

    /// <summary>
    ///     The EMR_CHORD record specifies a chord, which is a region bounded by the intersection of an ellipse and a line segment, called a secant. 
    ///     The chord is outlined by using the current pen and filled by using the current brush.
    /// </summary>
    internal class EMR_CHORD : Record
    {
        /// <summary>
        ///     A 128-bit WMF RectL object, specified in [MS-WMF] section 2.2.2.19, which specifies the inclusive-inclusive bounding rectangle.
        /// </summary>
        public RectL Box;

        /// <summary>
        ///     A 64-bit WMF PointL object, specified in [MS-WMF] section 2.2.2.15, which specifies the logical coordinates of the endpoint of the radial defining the beginning of the chord.
        /// </summary>
        public PointL Start;

        /// <summary>
        ///     A 64-bit WMF PointL object that specifies the logical coordinates of the endpoint of the radial defining the end of the chord.
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
