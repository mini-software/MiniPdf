namespace MiniSoftware.Drawing.Metafile.Emf.Records.DrawingRecordTypes
{
    using System.IO;

    using MiniSoftware.Drawing.Metafile.Wmf;
    using MiniSoftware.Drawing.Metafile.Wmf.Enumerations;
    using MiniSoftware.Drawing.Metafile.Wmf.Objects;

    /// <summary>
    ///     The EMR_EXTFLOODFILL record fills an area of the display surface with the current brush.
    /// </summary>
    /// <remarks>
    ///     2.3.5.6 EMR_EXTFLOODFILL Record
    /// </remarks>
    internal class EMR_EXTFLOODFILL : Record
    {
        
        /// <summary>
        ///     A WMF PointL object ([MS-WMF] section 2.2.2.15), which specifies the coordinates, in logical units, where filling begins.
        /// </summary>
        public PointL Start;

        /// <summary>
        ///     A 64-bit WMF PointL object that specifies the coordinates, in logical units, of the ending point of the radial line defining the ending point of the arc.
        /// </summary>
        public ColorRef Color;

        /// <summary>
        ///     A 32-bit unsigned integer that specifies how to use the Color value to determine the area for the flood fill operation. 
        ///     The value MUST be in the FloodFill enumeration (section 2.1.13).
        /// </summary>
        public FloodFill FloodFillMode;

        public override void Read(BinaryReader reader)
        {
            
            this.Start = new PointL();
            this.Start.Read(reader);
            this.Color = new ColorRef();
            this.Color.Read(reader);
            this.FloodFillMode = (FloodFill)reader.ReadUInt32();
        }

        public override void Write(BinaryWriter writer)
        {
            this.Start.Write(writer);
            this.Color.Write(writer);
            writer.Write((uint)this.FloodFillMode);
        }
    }
}
