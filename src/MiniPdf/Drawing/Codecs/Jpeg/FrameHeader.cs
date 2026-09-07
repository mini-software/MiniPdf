using System;
using System.IO;

namespace MiniPdf.Drawing.Codecs.Jpeg
{
    /// <summary>JPEG Start-of-Frame type.</summary>
    internal enum SofType
    {
        Baseline          = 0,  // SOF0
        ExtendedSequential = 1, // SOF1
        Progressive        = 2, // SOF2
    }

    /// <summary>
    /// Parsed payload of a JPEG SOF0 / SOF1 / SOF2 marker.
    /// </summary>
    internal sealed class FrameHeader
    {
        /// <summary>Sample precision in bits (typically 8).</summary>
        public int Precision { get; private set; }

        /// <summary>Frame height in pixels.</summary>
        public int Height { get; private set; }

        /// <summary>Frame width in pixels.</summary>
        public int Width { get; private set; }

        /// <summary>Image components (1 = grayscale, 3 = YCbCr, 4 = CMYK).</summary>
        public FrameComponent[] Components { get; private set; } = Array.Empty<FrameComponent>();

        /// <summary>SOF type that was used to produce this header.</summary>
        public SofType Type { get; private set; }

        // ── Factory ──────────────────────────────────────────────────────────────

        /// <summary>
        /// Parses a SOF0/SOF1/SOF2 segment payload.
        /// <paramref name="marker"/> determines the <see cref="SofType"/>.
        /// </summary>
        public static FrameHeader Parse(JpegMarker marker, byte[] data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            if (data.Length < 6)
                throw new InvalidDataException("SOF segment too short.");

            var fh = new FrameHeader
            {
                Type      = MarkerToSofType(marker),
                Precision = data[0],
                Height    = (data[1] << 8) | data[2],
                Width     = (data[3] << 8) | data[4],
            };

            int nf = data[5];
            if (data.Length < 6 + nf * 3)
                throw new InvalidDataException("SOF segment component data truncated.");

            fh.Components = new FrameComponent[nf];
            for (int i = 0; i < nf; i++)
            {
                int off = 6 + i * 3;
                byte samp = data[off + 1];
                fh.Components[i] = new FrameComponent
                {
                    Id                  = data[off],
                    HSampling           = (byte)((samp >> 4) & 0xF),
                    VSampling           = (byte)(samp & 0xF),
                    QuantizationTableId = data[off + 2],
                };
            }
            return fh;
        }

        private static SofType MarkerToSofType(JpegMarker m) => m switch
        {
            JpegMarker.SOF0 => SofType.Baseline,
            JpegMarker.SOF1 => SofType.ExtendedSequential,
            JpegMarker.SOF2 => SofType.Progressive,
            _               => SofType.Baseline,
        };
    }
}
