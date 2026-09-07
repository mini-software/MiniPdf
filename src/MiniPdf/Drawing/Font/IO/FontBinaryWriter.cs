using System;
using System.IO;
using SysEncoding = System.Text.Encoding;

namespace MiniPdf.Drawing.Font.IO
{
    /// <summary>
    /// A big-endian binary writer for OpenType/TrueType font data.
    /// All multi-byte integers are written in network byte order (most-significant byte first),
    /// as required by the OpenType specification.
    /// </summary>
    public sealed class FontBinaryWriter : IDisposable
    {
        private readonly BinaryWriter _writer;
        private readonly bool _leaveOpen;

        /// <param name="stream">The stream to write to.</param>
        /// <param name="leaveOpen">
        /// When <c>true</c> the underlying stream is NOT disposed when this writer is disposed.
        /// </param>
        public FontBinaryWriter(Stream stream, bool leaveOpen = false)
        {
            if (stream is null) throw new ArgumentNullException(nameof(stream));
            _writer = new BinaryWriter(stream, SysEncoding.ASCII, leaveOpen: true);
            _leaveOpen = leaveOpen;
        }

        // ── Stream control ────────────────────────────────────────────────────

        /// <summary>Current byte position within the underlying stream.</summary>
        public long Position => _writer.BaseStream.Position;

        /// <summary>Seeks to an absolute byte offset from the beginning of the stream.</summary>
        public void Seek(long offset) =>
            _writer.BaseStream.Seek(offset, SeekOrigin.Begin);

        /// <summary>
        /// Pads the current position forward with zero bytes until the stream position
        /// is aligned to a 4-byte boundary.  No-op when already aligned.
        /// Required by OpenType: every table must begin on a 4-byte boundary.
        /// </summary>
        public void Align4()
        {
            long pos = _writer.BaseStream.Position;
            int rem = (int)(pos % 4);
            if (rem != 0)
            {
                for (int i = 0; i < 4 - rem; i++)
                    _writer.Write((byte)0);
            }
        }

        // ── Primitive writes (big-endian) ─────────────────────────────────────

        /// <summary>Writes one unsigned byte.</summary>
        public void WriteUInt8(byte value) => _writer.Write(value);

        /// <summary>Writes an unsigned 16-bit integer (big-endian).</summary>
        public void WriteUInt16(ushort value)
        {
            _writer.Write((byte)(value >> 8));
            _writer.Write((byte)(value & 0xFF));
        }

        /// <summary>Writes a signed 16-bit integer (big-endian).</summary>
        public void WriteInt16(short value) => WriteUInt16((ushort)value);

        /// <summary>Writes an unsigned 32-bit integer (big-endian).</summary>
        public void WriteUInt32(uint value)
        {
            _writer.Write((byte)((value >> 24) & 0xFF));
            _writer.Write((byte)((value >> 16) & 0xFF));
            _writer.Write((byte)((value >> 8)  & 0xFF));
            _writer.Write((byte)( value         & 0xFF));
        }

        /// <summary>Writes a signed 32-bit integer (big-endian).</summary>
        public void WriteInt32(int value) => WriteUInt32((uint)value);

        /// <summary>Writes a signed 64-bit integer (big-endian).</summary>
        public void WriteInt64(long value)
        {
            WriteUInt32((uint)((ulong)value >> 32));
            WriteUInt32((uint)((ulong)value & 0xFFFFFFFF));
        }

        // ── Fixed-point writes ────────────────────────────────────────────────

        /// <summary>Writes a 16.16 fixed-point number.</summary>
        public void WriteFixed(double value) => WriteInt32((int)(value * 65536.0));

        /// <summary>Writes a 2.14 fixed-point number.</summary>
        public void WriteF2Dot14(double value) => WriteInt16((short)(value * 16384.0));

        // ── Composite writes ──────────────────────────────────────────────────

        /// <summary>
        /// Writes a 4-byte ASCII tag.  If <paramref name="tag"/> is shorter than
        /// 4 characters it is right-padded with spaces; if longer it is truncated.
        /// </summary>
        public void WriteTag(string tag)
        {
            if (tag is null) throw new ArgumentNullException(nameof(tag));
            byte[] bytes = SysEncoding.ASCII.GetBytes(tag.PadRight(4).Substring(0, 4));
            _writer.Write(bytes);
        }

        /// <summary>Writes a raw byte array verbatim.</summary>
        public void WriteBytes(byte[] data)
        {
            if (data is null) throw new ArgumentNullException(nameof(data));
            _writer.Write(data);
        }

        /// <summary>Flushes the underlying stream.</summary>
        public void Flush() => _writer.Flush();

        // ── IDisposable ───────────────────────────────────────────────────────

        /// <inheritdoc/>
        public void Dispose()
        {
            if (!_leaveOpen)
                _writer.Dispose();
        }
    }
}
