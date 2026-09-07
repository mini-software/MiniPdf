using MiniPdf.Drawing.Metafile.Wmf.Records;
using System.Collections.Generic;

namespace MiniPdf.Drawing.Metafile
{
    /// <summary>
    /// Represents a metafile document containing WMF, EMF, and EMF+ records.
    /// </summary>
    public class MetafileDocument
    {
        /// <summary>
        /// Gets or sets the metafile format.
        /// </summary>
        public MetafileFormat Format { get; set; }

        /// <summary>
        /// Gets or sets the WMF header.
        /// </summary>
        internal MetaHeader? WmfHeader { get; set; }

        /// <summary>
        /// Gets or sets the WMF records.
        /// </summary>
        internal IReadOnlyList<Record> WmfRecords { get; set; } = new List<Record>();

        /// <summary>
        /// Gets or sets the EMF records.
        /// </summary>
        internal IReadOnlyList<Emf.Records.Record> EmfRecords { get; set; } = new List<Metafile.Emf.Records.Record>();

        /// <summary>
        /// Gets or sets the EMF+ data size.
        /// </summary>
        public uint EmfPlusDataSize { get; set; }

        /// <summary>
        /// Gets or sets the EMF+ comment identifier.
        /// </summary>
        public uint EmfPlusCommentIdentifier { get; set; }

        /// <summary>
        /// Gets or sets the EMF+ records.
        /// </summary>
        internal IReadOnlyList<Metafile.EmfPlus.Records.Record> EmfPlusRecords { get; set; } = new List<Metafile.EmfPlus.Records.Record>();

        /// <summary>
        /// Gets or sets the embedded EMF+ records.
        /// </summary>
        internal IReadOnlyList<IReadOnlyList<Metafile.EmfPlus.Records.Record>> EmbeddedEmfPlusRecords { get; set; } = new List<IReadOnlyList<Metafile.EmfPlus.Records.Record>>();

        /// <summary>
        /// Gets or sets the placeable header.
        /// </summary>
        internal META_PLACEABLE? PlaceableHeader { get; set; }
    }
}
