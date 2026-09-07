using System;

namespace MiniPdf.Drawing.Codecs.Jpeg
{
    /// <summary>
    /// Reads the Orientation tag from an Exif APP1 payload.
    /// All other IFD tags are ignored.
    /// </summary>
    internal static class ExifReader
    {
        private const ushort TagOrientation = 274;   // 0x0112

        /// <summary>
        /// Returns the Exif Orientation value (1–8), or 1 (no rotation) if the
        /// APP1 payload is not a valid Exif block or the tag is absent.
        /// </summary>
        public static int ReadOrientation(byte[] app1Data)
        {
            if (app1Data == null || app1Data.Length < 12) return 1;

            // APP1 starts with "Exif\0\0" (6 bytes)
            if (app1Data[0] != 0x45 || app1Data[1] != 0x78 || app1Data[2] != 0x69 ||
                app1Data[3] != 0x66 || app1Data[4] != 0x00 || app1Data[5] != 0x00)
                return 1;

            // TIFF header starts at offset 6
            int tiffBase = 6;
            if (app1Data.Length < tiffBase + 8) return 1;

            bool le = app1Data[tiffBase] == 0x49 && app1Data[tiffBase + 1] == 0x49;
            // Validate TIFF magic (42)
            int magic = ReadU16(app1Data, tiffBase + 2, le);
            if (magic != 42) return 1;

            // Offset to first IFD
            int ifd0Offset = (int)ReadU32(app1Data, tiffBase + 4, le);
            int ifd0Abs    = tiffBase + ifd0Offset;
            if (ifd0Abs + 2 > app1Data.Length) return 1;

            int entryCount = ReadU16(app1Data, ifd0Abs, le);
            for (int i = 0; i < entryCount; i++)
            {
                int entryAbs = ifd0Abs + 2 + i * 12;
                if (entryAbs + 12 > app1Data.Length) break;

                int tag   = ReadU16(app1Data, entryAbs,     le);
                int type  = ReadU16(app1Data, entryAbs + 2, le);
                long count = ReadU32(app1Data, entryAbs + 4, le);

                if (tag == TagOrientation && type == 3 /*SHORT*/ && count == 1)
                {
                    // Value is inline in the 4-byte value/offset field
                    int value = ReadU16(app1Data, entryAbs + 8, le);
                    return Math.Max(1, Math.Min(8, value));
                }
            }
            return 1;
        }

        // ── Byte-order-aware helpers ──────────────────────────────────────────────

        private static int ReadU16(byte[] data, int offset, bool le)
        {
            if (offset + 2 > data.Length) return 0;
            return le
                ? data[offset] | (data[offset + 1] << 8)
                : (data[offset] << 8) | data[offset + 1];
        }

        private static long ReadU32(byte[] data, int offset, bool le)
        {
            if (offset + 4 > data.Length) return 0;
            return le
                ? (uint)(data[offset] | (data[offset+1] << 8) | (data[offset+2] << 16) | (data[offset+3] << 24))
                : (uint)((data[offset] << 24) | (data[offset+1] << 16) | (data[offset+2] << 8) | data[offset+3]);
        }
    }
}
