namespace MiniSoftware.Drawing.Metafile.Wmf.Records.ObjectRecordTypes
{
    using System.IO;

    /// <summary>
    ///     The META_SELECTCLIPREGION record specifies a Region Object (section 2.2.1.5) to be the current clipping region.
    /// </summary>
    /// <remarks>   
    ///     2.3.4.9 META_SELECTCLIPREGION Record
    /// </remarks>
    internal class META_SELECTCLIPREGION : Record
    {
        /// <summary>
        ///     A 16-bit unsigned integer used to index into the WMF Object Table to get the region to be inverted.
        /// </summary>
        public ushort Region;

        public override void Read(BinaryReader reader)
        {
            this.Region = reader.ReadUInt16();
        }

        public override void Write(BinaryWriter writer)
        {
            writer.Write(this.Region);
        }
    }
}
