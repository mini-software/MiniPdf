namespace MiniPdf.Drawing.Font.Core
{
    /// <summary>
    /// Represents the embedding and licensing restrictions declared in the
    /// OS/2 table's <c>fsType</c> field (OpenType spec §5.7).
    /// </summary>
    /// <remarks>
    /// Obtain an instance via <c>TtfOs2Table.GetLicenseFlags()</c>.
    /// A null return from that method indicates the OS/2 table is absent.
    /// When the instance is non-null but <see cref="FSTypeAbsent"/> is true,
    /// the font declares no embedding restrictions (fsType == 0).
    /// </remarks>
    public sealed class LicenseFlags
    {
        /// <summary>Raw OS/2 <c>fsType</c> value.</summary>
        public ushort FsType { get; }

        /// <summary>
        /// Initializes a new <see cref="LicenseFlags"/> instance from the specified fsType value.
        /// </summary>
        /// <param name="fsType">The raw OS/2 fsType value.</param>
        public LicenseFlags(ushort fsType)
        {
            FsType = fsType;
        }

        // ── Embedding-type bits (bits 1–3 are the key restriction flags) ──────

        /// <summary>
        /// True if <c>fsType == 0</c>, meaning the font declares no embedding
        /// restrictions and may be freely embedded.
        /// </summary>
        public bool FSTypeAbsent => FsType == 0;

        /// <summary>
        /// Bit 1 (mask 0x0002).  The font must not be modified or embedded in any
        /// manner without explicit permission of the legal owner.
        /// </summary>
        public bool IsRestrictedLicenseEmbedding => (FsType & 0x0002) != 0;

        /// <summary>
        /// Bit 2 (mask 0x0004), bit 1 clear.  The font may be embedded for preview
        /// and print but must not be installed on the target system.
        /// </summary>
        public bool IsPreviewAndPrintEmbedding =>
            (FsType & 0x0004) != 0 && (FsType & 0x0002) == 0;

        /// <summary>
        /// Bit 3 (mask 0x0008), bits 1–2 clear.  The font may be embedded and
        /// documents using it may be edited on the target system.
        /// </summary>
        public bool IsEditableEmbedding =>
            (FsType & 0x0008) != 0 && (FsType & 0x0006) == 0;

        /// <summary>
        /// No restriction bits (1–3) are set; the font may be permanently installed
        /// on target systems (installable embedding).
        /// </summary>
        public bool IsInstallableEmbedding =>
            !FSTypeAbsent
            && !IsRestrictedLicenseEmbedding
            && !IsPreviewAndPrintEmbedding
            && !IsEditableEmbedding;

        // ── Additional flags ──────────────────────────────────────────────────

        /// <summary>
        /// Bit 8 (mask 0x0100).  The font may not be subsetted prior to embedding.
        /// </summary>
        public bool NoSubsetting => (FsType & 0x0100) != 0;

        /// <summary>
        /// Bit 9 (mask 0x0200).  Only bitmaps contained in the font may be embedded;
        /// no outline data may be embedded.
        /// </summary>
        public bool BitmapEmbeddingOnly => (FsType & 0x0200) != 0;
    }
}
