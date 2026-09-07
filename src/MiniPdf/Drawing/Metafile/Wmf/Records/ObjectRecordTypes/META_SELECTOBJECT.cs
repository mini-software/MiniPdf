using MiniPdf.Drawing.Metafile.Wmf;

namespace MiniPdf.Drawing.Metafile.Wmf.Records.ObjectRecordTypes
{
    using MiniPdf.Drawing.Metafile.Wmf.Enumerations;
    using System.IO;

    /// <summary>
    ///     The META_SELECTOBJECT record specifies a graphics object for the playback device context. 
    ///     The new object replaces the previous object of the same type, unless if the previous object is a palette object. If the previous object is a palette object, then the META_SELECTPALETTE record must be used instead of the META_SELECTOBJECT record, as the META_SELECTOBJECT record does not support replacing the palette object type.
    /// </summary>
    internal class META_SELECTOBJECT : Record
    {
        /// <summary>
        ///     A 16-bit unsigned integer used to index into the WMF Object Table to get the object to be selected.
        /// </summary>
        public ushort ObjectIndex;

        public META_SELECTOBJECT()
        {
            Header.RecordFunction = RecordType.META_SELECTOBJECT;
        }

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
