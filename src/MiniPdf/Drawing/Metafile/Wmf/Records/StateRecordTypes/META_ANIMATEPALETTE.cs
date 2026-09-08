using MiniSoftware.Drawing.Metafile.Wmf;

namespace MiniSoftware.Drawing.Metafile.Wmf.Records.StateRecordTypes
{
    using System.IO;

    using MiniSoftware.Drawing.Metafile.Wmf.Enumerations;
    using MiniSoftware.Drawing.Metafile.Wmf.Objects;

    /// <summary>
    ///     The META_ANIMATEPALETTE record redefines entries in the logical palette that is defined in the playback device context with the specified Palette object (section 2.2.1.3).
    /// </summary>
    /// <remarks>
    ///     2.3.5.1 META_ANIMATEPALETTE Record
    /// </remarks>
    internal class META_ANIMATEPALETTE : Record
    {
        /// <summary>
        ///     A variable-sized Palette object that specifies a logical palette.
        /// </summary>
        public Palette Palette { get; set; } = new Palette();

        public META_ANIMATEPALETTE()
        {
            Header.RecordFunction = RecordType.META_ANIMATEPALETTE;
        }

        public override void Read(BinaryReader reader)
        {
            Palette.Read(reader);
        }

        public override void Write(BinaryWriter writer)
        {
            base.Write(writer);
            Palette.Write(writer);
        }
    }
}
