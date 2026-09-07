using System.IO;
using MiniPdf.Drawing.Font.Core;
using MiniPdf.Drawing.Font.IO;

namespace MiniPdf.Drawing.Font.Ttf.Tables
{
    /// <summary>
    /// Parsed OpenType "OS/2" table (OS/2 and Windows metrics).
    /// Contains typographic metrics and embedding license flags used by
    /// both text layout and PDF font embedding consumers.
    /// Ref: OpenType spec §5.7.
    /// </summary>
    public sealed class TtfOs2Table : TtfTableBase
    {
        /// <summary>
        /// The table tag for the OS/2 table.
        /// </summary>
        public const string TableTag = "OS/2";

        // ── Parsed fields ─────────────────────────────────────────────────────

        /// <summary>
        /// Embedding licensing flags (raw <c>fsType</c> value from the table).
        /// Use <see cref="GetLicenseFlags"/> for interpreted per-bit access.
        /// </summary>
        public ushort FsType { get; }

        /// <summary>Typographic ascender above baseline, in design units.</summary>
        public short TypoAscender { get; }

        /// <summary>Typographic descender below baseline (negative), in design units.</summary>
        public short TypoDescender { get; }

        /// <summary>Typographic line gap, in design units.</summary>
        public short TypoLineGap { get; }

        /// <summary>Windows-compatible ascender, in design units.</summary>
        public ushort WinAscent { get; }

        /// <summary>Windows-compatible descender (positive), in design units.</summary>
        public ushort WinDescent { get; }

        /// <summary>Code-page coverage bitmask 1 (version ≥ 1 only; null for v0).</summary>
        public int? CodePageRange1 { get; }

        /// <summary>Code-page coverage bitmask 2 (version ≥ 1 only; null for v0).</summary>
        public int? CodePageRange2 { get; }

        /// <summary>Strikeout stroke thickness, in design units (from OS/2 yStrikeoutSize).</summary>
        public short StrikeoutSize { get; }

        /// <summary>
        /// Distance from baseline to top of strikeout stroke, in design units
        /// (from OS/2 yStrikeoutPosition). Positive values are above the baseline.
        /// </summary>
        public short StrikeoutPosition { get; }

        // ── Constructor ───────────────────────────────────────────────────────

        private TtfOs2Table(
            byte[] rawBytes,
            ushort fsType,
            short typoAscender, short typoDescender, short typoLineGap,
            ushort winAscent, ushort winDescent,
            int? codePageRange1, int? codePageRange2,
            short strikeoutSize, short strikeoutPosition)
            : base(TableTag, rawBytes)
        {
            FsType            = fsType;
            TypoAscender      = typoAscender;
            TypoDescender     = typoDescender;
            TypoLineGap       = typoLineGap;
            WinAscent         = winAscent;
            WinDescent        = winDescent;
            CodePageRange1    = codePageRange1;
            CodePageRange2    = codePageRange2;
            StrikeoutSize     = strikeoutSize;
            StrikeoutPosition = strikeoutPosition;
        }

        // ── Factory ───────────────────────────────────────────────────────────

        /// <summary>Parses the "OS/2" table from its raw bytes.</summary>
        public static TtfOs2Table Parse(byte[] rawBytes)
        {
            using var reader = new FontBinaryReader(new MemoryStream(rawBytes));

            // Byte offsets per spec (all versions share this prefix):
            ushort version    = reader.ReadUInt16();  //  0  version
            reader.Skip(2);                           //  2  xAvgCharWidth
            reader.Skip(2);                           //  4  usWeightClass
            reader.Skip(2);                           //  6  usWidthClass
            ushort fsType     = reader.ReadUInt16();  //  8  fsType  ← LICENSE FLAGS
            reader.Skip(16);                          // 10–25 ySubscript/ySuperscript (8 shorts)
            short  strikeoutSize     = reader.ReadInt16(); // 26  yStrikeoutSize
            short  strikeoutPosition = reader.ReadInt16(); // 28  yStrikeoutPosition
            reader.Skip(2);                           // 30  sFamilyClass
            reader.Skip(10);                          // 32  panose[10]
            reader.Skip(16);                          // 42  ulUnicodeRange1–4
            reader.Skip(4);                           // 58  achVendID
            reader.Skip(2);                           // 62  fsSelection
            reader.Skip(2);                           // 64  usFirstCharIndex
            reader.Skip(2);                           // 66  usLastCharIndex
            short  typoAscender  = reader.ReadInt16();  // 68
            short  typoDescender = reader.ReadInt16();  // 70
            short  typoLineGap   = reader.ReadInt16();  // 72
            ushort winAscent     = reader.ReadUInt16(); // 74
            ushort winDescent    = reader.ReadUInt16(); // 76

            int? codePageRange1 = null;
            int? codePageRange2 = null;
            if (version >= 1 && rawBytes.Length >= 86)
            {
                codePageRange1 = reader.ReadInt32();  // 78
                codePageRange2 = reader.ReadInt32();  // 82
            }

            return new TtfOs2Table(rawBytes, fsType,
                typoAscender, typoDescender, typoLineGap,
                winAscent, winDescent,
                codePageRange1, codePageRange2,
                strikeoutSize, strikeoutPosition);
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        /// <summary>
        /// Returns a <see cref="LicenseFlags"/> instance that exposes per-bit
        /// embedding restriction properties derived from <see cref="FsType"/>.
        /// </summary>
        public LicenseFlags GetLicenseFlags() => new LicenseFlags(FsType);
    }
}
