using System;
using System.IO;

namespace MiniPdf.Drawing.Codecs.Jpeg
{
    /// <summary>
    /// Parsed payload of a JPEG SOS (Start-of-Scan) marker.
    /// </summary>
    internal sealed class ScanHeader
    {
        /// <summary>Components included in this scan.</summary>
        public ScanComponent[] Components { get; private set; } = Array.Empty<ScanComponent>();

        /// <summary>Start of spectral selection (0 for sequential).</summary>
        public int Ss { get; private set; }

        /// <summary>End of spectral selection (63 for sequential).</summary>
        public int Se { get; private set; }

        /// <summary>Successive approximation high (0 for first/only pass).</summary>
        public int Ah { get; private set; }

        /// <summary>Successive approximation low (0 for sequential).</summary>
        public int Al { get; private set; }

        // ── Factory ──────────────────────────────────────────────────────────────

        /// <summary>Parses a SOS segment payload.</summary>
        public static ScanHeader Parse(byte[] data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            if (data.Length < 4)
                throw new InvalidDataException("SOS segment too short.");

            int ns = data[0];
            if (data.Length < 1 + ns * 2 + 3)
                throw new InvalidDataException("SOS segment truncated.");

            var sh = new ScanHeader();
            sh.Components = new ScanComponent[ns];
            for (int i = 0; i < ns; i++)
            {
                int off  = 1 + i * 2;
                byte sel = data[off + 1];
                sh.Components[i] = new ScanComponent
                {
                    ComponentId = data[off],
                    DcTableId   = (byte)((sel >> 4) & 0xF),
                    AcTableId   = (byte)(sel & 0xF),
                };
            }

            int tail = 1 + ns * 2;
            sh.Ss = data[tail];
            sh.Se = data[tail + 1];
            byte ahAl = data[tail + 2];
            sh.Ah = (ahAl >> 4) & 0xF;
            sh.Al =  ahAl       & 0xF;
            return sh;
        }
    }
}
