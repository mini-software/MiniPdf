using System.IO;

namespace MiniSoftware.Drawing.Metafile.Wmf.Records
{
    /// <summary>
    ///     The META_PAINTREGION record paints the specified region by using the brush that is defined in the playback device context.
    /// </summary>
    /// <remarks>
    ///     2.3.3.11 META_PAINTREGION Record
    /// </remarks>
    internal class META_PAINTREGION : Record
    {
        /// <summary>
        ///     Used to index into the WMF Object Table to get the region to be painted.
        /// </summary>
                public ushort Region;

        public override void Read(BinaryReader reader)
        {
            Region = reader.ReadUInt16();
        }

        public override void Write(BinaryWriter writer)
        {
            writer.Write(Region);
        }
    }
}
