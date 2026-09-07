using System;
using System.IO;
using SysEncoding = System.Text.Encoding;

namespace MiniPdf.Drawing.Font.IO
{
    /// <summary>
    /// A big-endian binary reader for OpenType/TrueType font data.
    /// All multi-byte integers are read in network byte order (most-significant byte first),
    /// as required by the OpenType specification.
    /// </summary>
    public sealed class FontBinaryReader : IDisposable
    {
        private readonly BinaryReader _reader;
        private readonly bool _leaveOpen;

        /// <param name="stream">The stream to read from.  Must support seeking.</param>
        /// <param name="leaveOpen">
        /// When <c>true</c> the underlying stream is NOT disposed when this reader is disposed.
        /// Use this when the stream's lifetime is managed by an outer scope.
        /// </param>
        public FontBinaryReader(Stream stream, bool leaveOpen = false)
        {
            if (stream is null) throw new ArgumentNullException(nameof(stream));
            _reader = new BinaryReader(stream, SysEncoding.ASCII, leaveOpen: true);
            _leaveOpen = leaveOpen;
        }

        // ── Stream control ────────────────────────────────────────────────────

        /// <summary>Current byte position within the underlying stream.</summary>
        public long Position => _reader.BaseStream.Position;

        /// <summary>Total length of the underlying stream in bytes.</summary>
        public long Length => _reader.BaseStream.Length;

        /// <summary>Seeks to an absolute byte offset from the beginning of the stream.</summary>
        public void Seek(long offset) =>
            _reader.BaseStream.Seek(offset, SeekOrigin.Begin);

        /// <summary>Advances the read position by <paramref name="count"/> bytes.</summary>
        public void Skip(int count) =>
            _reader.BaseStream.Seek(count, SeekOrigin.Current);

        // ── Primitive reads (big-endian) ──────────────────────────────────────

        /// <summary>Reads one unsigned byte.</summary>
        public byte ReadUInt8() => _reader.ReadByte();

        /// <summary>Reads an unsigned 16-bit integer (big-endian).</summary>
        public ushort ReadUInt16()
        {
            uint b0 = _reader.ReadByte();
            uint b1 = _reader.ReadByte();
            return (ushort)((b0 << 8) | b1);
        }

        /// <summary>Reads a signed 16-bit integer (big-endian).</summary>
        public short ReadInt16() => (short)ReadUInt16();

        /// <summary>Reads an unsigned 32-bit integer (big-endian).</summary>
        public uint ReadUInt32()
        {
            uint b0 = _reader.ReadByte();
            uint b1 = _reader.ReadByte();
            uint b2 = _reader.ReadByte();
            uint b3 = _reader.ReadByte();
            return (b0 << 24) | (b1 << 16) | (b2 << 8) | b3;
        }

        /// <summary>Reads a signed 32-bit integer (big-endian).</summary>
        public int ReadInt32() => (int)ReadUInt32();

        /// <summary>
        /// Reads a signed 64-bit integer (big-endian).
        /// Used for LONGDATETIME fields (e.g. head.created / head.modified).
        /// </summary>
        public long ReadInt64()
        {
            ulong hi = ReadUInt32();
            ulong lo = ReadUInt32();
            return (long)((hi << 32) | lo);
        }

        // ── Fixed-point reads ─────────────────────────────────────────────────

        /// <summary>
        /// Reads a 16.16 fixed-point number (signed 32-bit integer divided by 65536).
        /// Used for the sfVersion field, <c>post.italicAngle</c>, etc.
        /// </summary>
        public double ReadFixed() => ReadInt32() / 65536.0;

        /// <summary>
        /// Reads a 2.14 fixed-point number (signed 16-bit integer divided by 16384).
        /// Used for normalised variation coordinates in <c>gvar</c>/<c>avar</c>.
        /// </summary>
        public double ReadF2Dot14() => ReadInt16() / 16384.0;

        // ── Composite reads ───────────────────────────────────────────────────

        /// <summary>
        /// Reads a 4-byte ASCII tag string (e.g. "head", "cmap", "OTTO").
        /// </summary>
        public string ReadTag()
        {
            byte[] bytes = _reader.ReadBytes(4);
            return SysEncoding.ASCII.GetString(bytes);
        }

        /// <summary>Reads exactly <paramref name="count"/> bytes from the current position.</summary>
        public byte[] ReadBytes(int count) => _reader.ReadBytes(count);

        // ── IDisposable ───────────────────────────────────────────────────────

        /// <inheritdoc/>
        public void Dispose()
        {
            if (_leaveOpen)
                // Only dispose the BinaryReader wrapper, not the underlying stream.
                // BinaryReader.Dispose(false) is private, so we just suppress finalisation.
                GC.SuppressFinalize(this);
            else
                _reader.Dispose();
        }
    }
}
