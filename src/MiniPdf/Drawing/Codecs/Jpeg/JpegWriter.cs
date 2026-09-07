using System;
using System.IO;

namespace MiniSoftware.Drawing.Codecs.Jpeg
{
    internal sealed class JpegWriter
    {
        private readonly Stream _stream;

        public JpegWriter(Stream stream)
        {
            _stream = stream ?? throw new ArgumentNullException(nameof(stream));
        }

        public void WriteHeaders(int width, int height, Quantizer quantizer)
        {
            if (width <= 0) throw new ArgumentOutOfRangeException(nameof(width));
            if (height <= 0) throw new ArgumentOutOfRangeException(nameof(height));
            if (quantizer == null) throw new ArgumentNullException(nameof(quantizer));

            WriteMarker(JpegMarker.SOI);
            WriteApp0Jfif();
            WriteDqt(quantizer);
            WriteSof0(width, height);
            WriteDht();
            WriteSos();
        }

        public void WriteEndOfImage() => WriteMarker(JpegMarker.EOI);

        private void WriteApp0Jfif()
        {
            WriteMarker(JpegMarker.APP0);
            WriteUInt16(16);
            WriteAscii("JFIF");
            _stream.WriteByte(0x00);
            _stream.WriteByte(0x01);
            _stream.WriteByte(0x01);
            _stream.WriteByte(0x01); // units = DPI
            WriteUInt16(72);
            WriteUInt16(72);
            _stream.WriteByte(0x00); // no thumbnail
            _stream.WriteByte(0x00);
        }

        private void WriteDqt(Quantizer quantizer)
        {
            WriteMarker(JpegMarker.DQT);
            int length = 2 + (1 + 64) + (1 + 64);
            WriteUInt16((ushort)length);

            _stream.WriteByte(0x00); // Pq=0, Tq=0 (luma)
            _stream.Write(quantizer.LuminanceZigZag, 0, 64);

            _stream.WriteByte(0x01); // Pq=0, Tq=1 (chroma)
            _stream.Write(quantizer.ChrominanceZigZag, 0, 64);
        }

        private void WriteSof0(int width, int height)
        {
            WriteMarker(JpegMarker.SOF0);
            WriteUInt16(17);
            _stream.WriteByte(8); // precision
            WriteUInt16((ushort)height);
            WriteUInt16((ushort)width);
            _stream.WriteByte(3); // component count

            // Y component
            _stream.WriteByte(1);    // id
            _stream.WriteByte(0x11); // sampling 1x1
            _stream.WriteByte(0);    // quant table 0

            // Cb component
            _stream.WriteByte(2);
            _stream.WriteByte(0x11);
            _stream.WriteByte(1);

            // Cr component
            _stream.WriteByte(3);
            _stream.WriteByte(0x11);
            _stream.WriteByte(1);
        }

        private void WriteDht()
        {
            WriteMarker(JpegMarker.DHT);

            int length = 2
                         + 1 + 16 + HuffmanEncoder.DcLuminanceValues.Length
                         + 1 + 16 + HuffmanEncoder.DcChrominanceValues.Length
                         + 1 + 16 + HuffmanEncoder.AcLuminanceValues.Length
                         + 1 + 16 + HuffmanEncoder.AcChrominanceValues.Length;
            WriteUInt16((ushort)length);

            WriteHuffmanTable(0x00, HuffmanEncoder.DcLuminanceCounts, HuffmanEncoder.DcLuminanceValues);
            WriteHuffmanTable(0x01, HuffmanEncoder.DcChrominanceCounts, HuffmanEncoder.DcChrominanceValues);
            WriteHuffmanTable(0x10, HuffmanEncoder.AcLuminanceCounts, HuffmanEncoder.AcLuminanceValues);
            WriteHuffmanTable(0x11, HuffmanEncoder.AcChrominanceCounts, HuffmanEncoder.AcChrominanceValues);
        }

        private void WriteSos()
        {
            WriteMarker(JpegMarker.SOS);
            WriteUInt16(12);
            _stream.WriteByte(3); // components in scan

            _stream.WriteByte(1); // Y
            _stream.WriteByte(0x00); // DC0/AC0

            _stream.WriteByte(2); // Cb
            _stream.WriteByte(0x11); // DC1/AC1

            _stream.WriteByte(3); // Cr
            _stream.WriteByte(0x11); // DC1/AC1

            _stream.WriteByte(0x00); // Ss
            _stream.WriteByte(0x3F); // Se
            _stream.WriteByte(0x00); // Ah/Al
        }

        private void WriteHuffmanTable(byte classAndId, byte[] counts, byte[] values)
        {
            _stream.WriteByte(classAndId);
            _stream.Write(counts, 0, 16);
            _stream.Write(values, 0, values.Length);
        }

        private void WriteMarker(JpegMarker marker)
        {
            _stream.WriteByte(0xFF);
            _stream.WriteByte((byte)marker);
        }

        private void WriteUInt16(ushort value)
        {
            _stream.WriteByte((byte)((value >> 8) & 0xFF));
            _stream.WriteByte((byte)(value & 0xFF));
        }

        private void WriteAscii(string text)
        {
            for (int i = 0; i < text.Length; i++)
                _stream.WriteByte((byte)text[i]);
        }
    }
}
