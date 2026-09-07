namespace MiniSoftware.Drawing.Codecs.Jpeg
{
    /// <summary>JPEG marker codes (second byte of the 0xFF XX pair).</summary>
    internal enum JpegMarker : byte
    {
        // ── Start / End ─────────────────────────────────────────────────────────
        SOI = 0xD8,   // Start of Image
        EOI = 0xD9,   // End   of Image

        // ── Frame headers ────────────────────────────────────────────────────────
        SOF0  = 0xC0, // Baseline DCT
        SOF1  = 0xC1, // Extended Sequential DCT
        SOF2  = 0xC2, // Progressive DCT
        SOF3  = 0xC3, // Lossless Sequential
        SOF5  = 0xC5, // Differential Sequential DCT
        SOF6  = 0xC6, // Differential Progressive DCT
        SOF9  = 0xC9, // Extended Sequential, Arithmetic
        SOF10 = 0xCA, // Progressive, Arithmetic
        SOF11 = 0xCB, // Lossless Sequential, Arithmetic

        // ── Table / misc markers ─────────────────────────────────────────────────
        DHT = 0xC4,   // Define Huffman Table
        DAC = 0xCC,   // Define Arithmetic Coding
        DQT = 0xDB,   // Define Quantization Table
        DRI = 0xDD,   // Define Restart Interval
        COM = 0xFE,   // Comment

        // ── Restart markers ──────────────────────────────────────────────────────
        RST0 = 0xD0,
        RST1 = 0xD1,
        RST2 = 0xD2,
        RST3 = 0xD3,
        RST4 = 0xD4,
        RST5 = 0xD5,
        RST6 = 0xD6,
        RST7 = 0xD7,

        // ── Scan header ──────────────────────────────────────────────────────────
        SOS = 0xDA,   // Start of Scan

        // ── Application markers (APPn) ───────────────────────────────────────────
        APP0  = 0xE0,
        APP1  = 0xE1,
        APP2  = 0xE2,
        APP3  = 0xE3,
        APP4  = 0xE4,
        APP5  = 0xE5,
        APP6  = 0xE6,
        APP7  = 0xE7,
        APP8  = 0xE8,
        APP9  = 0xE9,
        APP10 = 0xEA,
        APP11 = 0xEB,
        APP12 = 0xEC,
        APP13 = 0xED,
        APP14 = 0xEE,
        APP15 = 0xEF,
    }
}
