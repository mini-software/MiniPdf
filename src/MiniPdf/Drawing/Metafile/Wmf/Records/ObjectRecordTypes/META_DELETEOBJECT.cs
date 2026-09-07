namespace MiniPdf.Drawing.Metafile.Wmf.Records.ObjectRecordTypes
{
    using System.IO;

    /// <summary>
    ///     The META_DELETEOBJECT record deletes an object, including Bitmap16, Brush, DeviceIndependentBitmap, Font, Palette, Pen, and Region. 
    ///     After the object is deleted, its index in the WMF Object Table is no longer valid but is available to be reused.
    /// </summary>
    /// <remarks>
    ///     2.3.4.7 META_DELETEOBJECT Record
    /// </remarks>
    internal class META_DELETEOBJECT : Record
    {
        /// <summary>
        ///     A 16-bit unsigned integer used to index into the WMF Object Table to get the object to be deleted.
        /// </summary>
        public ushort ObjectIndex;

        public override void Read(BinaryReader reader)
        {
            this.ObjectIndex = reader.ReadUInt16();
        }

        public override void Write(BinaryWriter writer)
        {
            writer.Write(this.ObjectIndex);
        }
    }
}
