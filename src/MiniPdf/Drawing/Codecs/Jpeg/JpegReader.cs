using System;
using System.IO;

namespace MiniSoftware.Drawing.Codecs.Jpeg
{
    /// <summary>
    /// Reads the JPEG marker stream (SOI … EOI) segment by segment.
    /// After positioning the stream at the start of entropy-coded data,
    /// obtain a <see cref="BitReader"/> via <see cref="CreateBitReader"/>.
    /// </summary>
    internal sealed class JpegReader
    {
        private readonly Stream _stream;

        public JpegReader(Stream stream) =>
            _stream = stream ?? throw new ArgumentNullException(nameof(stream));

        // ── Public position ──────────────────────────────────────────────────────

        /// <summary>Current byte position in the underlying stream.</summary>
        public long Position => _stream.Position;

        // ── Marker reading ───────────────────────────────────────────────────────

        /// <summary>
        /// Reads the next JPEG marker and its payload.
        /// Stand-alone markers (SOI, EOI, RSTn) have an empty payload byte[].
        /// Skips any 0xFF fill bytes before the marker byte.
        /// </summary>
        public (JpegMarker marker, byte[] payload) ReadMarker()
        {
            // Skip padding / find leading 0xFF
            int b = ReadByteOrThrow();
            while (b != 0xFF)
                b = ReadByteOrThrow();

            // Skip 0xFF fill bytes
            int marker;
            do { marker = ReadByteOrThrow(); }
            while (marker == 0xFF);

            var m = (JpegMarker)(byte)marker;

            // Stand-alone markers carry no length field
            if (IsStandAlone(m))
                return (m, Array.Empty<byte>());

            // Read 2-byte big-endian length (includes the 2 length bytes)
            int hi = ReadByteOrThrow();
            int lo = ReadByteOrThrow();
            int length = (hi << 8) | lo;
            if (length < 2)
                throw new InvalidDataException($"Invalid JPEG segment length {length} for marker 0x{marker:X2}.");

            int dataLen = length - 2;
            byte[] payload = new byte[dataLen];
            int read = 0;
            while (read < dataLen)
            {
                int n = _stream.Read(payload, read, dataLen - read);
                if (n == 0) throw new EndOfStreamException("Unexpected end of JPEG stream.");
                read += n;
            }
            return (m, payload);
        }

        /// <summary>
        /// Reads entropy-coded bytes after SOS until the next non-restart marker.
        /// The returned data is de-stuffed (0xFF 0x00 becomes 0xFF) and restart
        /// markers are removed. Stream position is left at the next marker.
        /// </summary>
        public byte[] ReadEntropyData()
        {
            if (!_stream.CanSeek)
                throw new NotSupportedException("Entropy segment reading requires a seekable stream.");

            using var ms = new MemoryStream();
            while (true)
            {
                int b = _stream.ReadByte();
                if (b < 0)
                    break;

                if (b != 0xFF)
                {
                    ms.WriteByte((byte)b);
                    continue;
                }

                int next = _stream.ReadByte();
                if (next < 0)
                    break;

                // Stuffed entropy byte.
                if (next == 0x00)
                {
                    ms.WriteByte(0xFF);
                    continue;
                }

                // Fill bytes may appear before a marker.
                while (next == 0xFF)
                {
                    next = _stream.ReadByte();
                    if (next < 0)
                        return ms.ToArray();
                }

                // Restart markers belong to entropy coding; skip marker bytes and continue.
                if (next >= 0xD0 && next <= 0xD7)
                    continue;

                // Rewind to the marker prefix so ReadMarker() can consume it.
                _stream.Seek(-2, SeekOrigin.Current);
                break;
            }

            return ms.ToArray();
        }

        /// <summary>
        /// Returns a <see cref="BitReader"/> that reads entropy-coded data
        /// from the stream at its current position (right after a SOS payload).
        /// </summary>
        public BitReader CreateBitReader() => new BitReader(_stream);

        // ── Helpers ──────────────────────────────────────────────────────────────

        private int ReadByteOrThrow()
        {
            int b = _stream.ReadByte();
            if (b < 0) throw new EndOfStreamException("Unexpected end of JPEG stream.");
            return b;
        }

        private static bool IsStandAlone(JpegMarker m) =>
            m == JpegMarker.SOI || m == JpegMarker.EOI ||
            (m >= JpegMarker.RST0 && m <= JpegMarker.RST7);
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // BitReader — reads individual bits from a JPEG entropy-coded stream.
    // Handles byte-stuffing: 0xFF 0x00 → single 0xFF byte.
    // ─────────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Bit-level reader for JPEG entropy-coded data.
    /// Reads bytes MSB-first; transparently strips 0xFF 0x00 stuffing bytes.
    /// </summary>
    internal sealed class BitReader
    {
        private readonly Stream? _stream;
        private readonly byte[]? _bytes;
        private int _byteIndex;
        private int _buffer;    // up to 32 bits buffered
        private int _bitsLeft;  // valid bits in _buffer

        public BitReader(Stream stream) =>
            _stream = stream ?? throw new ArgumentNullException(nameof(stream));

        public BitReader(byte[] bytes) =>
            _bytes = bytes ?? throw new ArgumentNullException(nameof(bytes));

        // ── Bit access ───────────────────────────────────────────────────────────

        /// <summary>Read exactly <paramref name="n"/> bits (1–16) MSB-first.</summary>
        public int ReadBits(int n)
        {
            EnsureBits(n);
            _bitsLeft -= n;
            return (_buffer >> _bitsLeft) & ((1 << n) - 1);
        }

        /// <summary>Peek at the next <paramref name="n"/> bits without consuming them.</summary>
        public int PeekBits(int n)
        {
            EnsureBits(n);
            return (_buffer >> (_bitsLeft - n)) & ((1 << n) - 1);
        }

        /// <summary>Discard <paramref name="n"/> bits that were already peeked.</summary>
        public void ConsumeBits(int n) => _bitsLeft -= n;

        /// <summary>Read a single bit (0 or 1).</summary>
        public int ReadBit() => ReadBits(1);

        // ── Byte-aligned access (used after restart markers) ─────────────────────

        /// <summary>Discard any partial byte; reset buffer. Call after a restart marker.</summary>
        public void Flush() { _buffer = 0; _bitsLeft = 0; }

        // ── Private ──────────────────────────────────────────────────────────────

        private void EnsureBits(int n)
        {
            while (_bitsLeft < n)
                FillByte();
        }

        private void FillByte()
        {
            if (_bytes != null)
            {
                int value = _byteIndex < _bytes.Length ? _bytes[_byteIndex++] : 0;
                _buffer = (_buffer << 8) | value;
                _bitsLeft += 8;
                return;
            }

            int b = _stream!.ReadByte();
            if (b < 0) { _buffer = (_buffer << 8) | 0xFF; _bitsLeft += 8; return; }

            if (b == 0xFF)
            {
                int next = _stream.ReadByte();
                if (next == 0x00)
                {
                    // Byte stuffing: 0xFF 0x00 → 0xFF
                    b = 0xFF;
                }
                else if (next >= 0xD0 && next <= 0xD7)
                {
                    // Restart marker — treat as end-of-scan; stuff zeros
                    b = 0; _buffer = (_buffer << 8) | b; _bitsLeft += 8; return;
                }
                else if (next == 0xD9 || next == 0xDA)
                {
                    // EOI or next SOS — end of entropy data
                    b = 0;
                }
                // else: unknown marker in entropy data; ignore
            }
            _buffer = (_buffer << 8) | b;
            _bitsLeft += 8;
        }
    }
}
