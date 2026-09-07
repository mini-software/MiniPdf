namespace MiniPdf.Drawing.Metafile.Emf.Records.DrawingRecordTypes
{
    using System.IO;

    using MiniPdf.Drawing.Metafile.Wmf.Objects;

    /// <summary>
    ///     The EMR_ANGLEARC record specifies a line segment of an arc. The line segment is drawn from the current position to the beginning of the arc. The arc is drawn along the perimeter of a circle with the given radius and center. 
    ///     The length of the arc is defined by the given start and sweep angles.
    /// </summary>
    /// <remarks>
    ///     2.3.5.1 EMR_ANGLEARC Record
    /// </remarks>
    internal class EMR_ANGLEARC : Record
    {
        /// <summary>
        ///     A 64-bit WMF PointL object, specified in [MS-WMF] section 2.2.2.15, which specifies the logical coordinates of the circle's center.
        /// </summary>
        public PointL Center;

        public uint Radius;

        public float StartAngle;

        public float SweepAngle;

        public override void Read(BinaryReader reader)
        {
            this.Center = new PointL();
            this.Center.Read(reader);
            this.Radius = reader.ReadUInt32();
            this.StartAngle = reader.ReadSingle();
            this.SweepAngle = reader.ReadSingle();
        }

        public override void Write(BinaryWriter writer)
        {
            this.Center.Write(writer);
            writer.Write(this.Radius);
            writer.Write(this.StartAngle);
            writer.Write(this.SweepAngle);
        }
    }
}
