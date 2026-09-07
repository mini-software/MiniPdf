namespace MiniPdf.Drawing.Metafile.Emf.Records.DrawingRecordTypes
{
    using System.IO;

    using MiniPdf.Drawing.Metafile.Wmf.Objects;

    /// <summary>
    ///     The EMR_ELLIPSE record specifies an ellipse. The center of the ellipse is the center of the specified bounding rectangle. 
    ///     The ellipse is outlined by using the current pen and is filled by using the current brush.
    /// </summary>
    /// <remarks>
    ///     2.3.5.5 EMR_ELLIPSE Record - MS-EMF Page 147
    /// </remarks>
    internal class EMR_ELLIPSE : Record
    {
        /// <summary>
        ///     A 128-bit WMF RectL object, specified in [MS-WMF] section 2.2.2.19, which specifies the inclusive-inclusive bounding rectangle.
        /// </summary>
        public RectL Box;

        public override void Read(BinaryReader reader)
        {
            this.Box = new RectL();
            this.Box.Read(reader);
            
        }

        public override void Write(BinaryWriter writer)
        {
            this.Box.Write(writer);
        }
    }
}
