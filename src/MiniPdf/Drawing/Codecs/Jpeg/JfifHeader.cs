using System;

namespace MiniPdf.Drawing.Codecs.Jpeg
{
    /// <summary>
    /// Parsed JFIF APP0 header (present in most "plain" JPEG files).
    /// </summary>
    internal sealed class JfifHeader
    {
        /// <summary>JFIF major version (e.g. 1).</summary>
        public byte MajorVersion { get; private set; }

        /// <summary>JFIF minor version (e.g. 1 or 2).</summary>
        public byte MinorVersion { get; private set; }

        /// <summary>Density units: 0 = none, 1 = DPI, 2 = dots-per-cm.</summary>
        public byte DensityUnits { get; private set; }

        /// <summary>Horizontal pixel density.</summary>
        public int XDensity { get; private set; }

        /// <summary>Vertical pixel density.</summary>
        public int YDensity { get; private set; }

        /// <summary>Thumbnail width in pixels (0 if absent).</summary>
        public int ThumbnailWidth { get; private set; }

        /// <summary>Thumbnail height in pixels (0 if absent).</summary>
        public int ThumbnailHeight { get; private set; }

        // ── Factory ──────────────────────────────────────────────────────────────

        /// <summary>
        /// Attempts to parse an APP0 payload as a JFIF header.
        /// Returns null if the signature is absent or the payload is malformed.
        /// </summary>
        public static JfifHeader? TryParse(byte[] data)
        {
            if (data == null || data.Length < 14) return null;

            // Check "JFIF\0" identifier
            if (data[0] != 0x4A || data[1] != 0x46 || data[2] != 0x49 ||
                data[3] != 0x46 || data[4] != 0x00)
                return null;

            return new JfifHeader
            {
                MajorVersion   = data[5],
                MinorVersion   = data[6],
                DensityUnits   = data[7],
                XDensity       = (data[8]  << 8) | data[9],
                YDensity       = (data[10] << 8) | data[11],
                ThumbnailWidth = data[12],
                ThumbnailHeight= data[13],
            };
        }
    }
}
