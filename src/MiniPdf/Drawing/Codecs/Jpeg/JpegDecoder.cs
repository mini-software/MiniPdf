using System;
using System.IO;
using MiniPdf.Drawing.Enums;
using MiniPdf.Drawing.Imaging;

namespace MiniPdf.Drawing.Codecs.Jpeg
{
    internal sealed class JpegDecoder
    {
        private readonly QuantizationTable?[] _quantTables = new QuantizationTable[4];
        private readonly HuffmanTable?[] _dcTables = new HuffmanTable[4];
        private readonly HuffmanTable?[] _acTables = new HuffmanTable[4];

        private FrameHeader? _frame;
        private JfifHeader? _jfif;
        private int _restartInterval;

        public Bitmap Decode(Stream stream)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));

            var reader = new JpegReader(stream);
            CoefficientBuffer? progressiveBuffer = null;
            ProgressiveDecoder? progressiveDecoder = null;

            var (firstMarker, _) = reader.ReadMarker();
            if (firstMarker != JpegMarker.SOI)
                throw new InvalidDataException("Invalid JPEG stream: SOI marker missing.");

            while (true)
            {
                var (marker, payload) = reader.ReadMarker();
                switch (marker)
                {
                    case JpegMarker.APP0:
                        _jfif = JfifHeader.TryParse(payload);
                        break;

                    case JpegMarker.DQT:
                        foreach (var table in QuantizationTable.ParseDqt(payload))
                        {
                            if ((uint)table.Id < 4)
                                _quantTables[table.Id] = table;
                        }
                        break;

                    case JpegMarker.DHT:
                        foreach (var table in HuffmanTable.ParseDht(payload))
                        {
                            if ((uint)table.Id >= 4)
                                continue;

                            if (table.Class == 0)
                                _dcTables[table.Id] = table;
                            else if (table.Class == 1)
                                _acTables[table.Id] = table;
                        }
                        break;

                    case JpegMarker.DRI:
                        if (payload.Length >= 2)
                            _restartInterval = (payload[0] << 8) | payload[1];
                        break;

                    case JpegMarker.SOF0:
                    case JpegMarker.SOF1:
                        _frame = FrameHeader.Parse(marker, payload);
                        progressiveBuffer = null;
                        progressiveDecoder = null;
                        break;

                    case JpegMarker.SOF2:
                        _frame = FrameHeader.Parse(marker, payload);
                        progressiveBuffer = new CoefficientBuffer(_frame);
                        progressiveDecoder = new ProgressiveDecoder(_frame, _quantTables, _dcTables, _acTables);
                        break;

                    case JpegMarker.SOS:
                        if (_frame == null)
                            throw new InvalidDataException("SOS encountered before SOF marker.");

                        if (_frame.Type == SofType.Progressive)
                        {
                            if (progressiveBuffer == null || progressiveDecoder == null)
                                throw new InvalidDataException("Progressive decoder state is not initialized.");

                            var scan = ScanHeader.Parse(payload);
                            byte[] entropyData = reader.ReadEntropyData();
                            progressiveDecoder.DecodeScan(scan, new BitReader(entropyData), progressiveBuffer, _restartInterval);
                            break;
                        }

                        return DecodeSequentialScan(reader, payload);

                    case JpegMarker.EOI:
                        if (_frame != null && _frame.Type == SofType.Progressive)
                        {
                            if (progressiveBuffer == null || progressiveDecoder == null)
                                throw new InvalidDataException("Progressive decoder state is not initialized.");

                            EntropyDecodeResult imageData = progressiveDecoder.BuildImageData(progressiveBuffer);
                            Bitmap progressiveBitmap = BuildBitmap(_frame, imageData);
                            progressiveBitmap._rawFormat = ImageFormat.Jpeg;
                            ApplyJfifDensity(progressiveBitmap, _jfif);
                            return progressiveBitmap;
                        }

                        throw new InvalidDataException("Invalid JPEG stream: no SOS marker found.");
                }
            }
        }

        private Bitmap DecodeSequentialScan(JpegReader reader, byte[] sosPayload)
        {
            if (_frame == null)
                throw new InvalidDataException("SOS encountered before SOF marker.");

            var scan = ScanHeader.Parse(sosPayload);

            if (_frame.Components.Length > 1 && scan.Components.Length != _frame.Components.Length)
                throw new NotSupportedException("Non-interleaved sequential JPEG scans are not supported yet.");

            var entropyDecoder = new EntropyDecoder(
                _frame,
                scan,
                _quantTables,
                _dcTables,
                _acTables,
                _restartInterval);

            byte[] entropyData = reader.ReadEntropyData();
            EntropyDecodeResult entropy = entropyDecoder.Decode(new BitReader(entropyData));
            Bitmap bitmap = BuildBitmap(_frame, entropy);
            bitmap._rawFormat = ImageFormat.Jpeg;
            ApplyJfifDensity(bitmap, _jfif);
            return bitmap;
        }

        private static Bitmap BuildBitmap(FrameHeader frame, EntropyDecodeResult entropy)
        {
            var bmp = new Bitmap(frame.Width, frame.Height, PixelFormat.Format32bppArgb);

            int componentCount = frame.Components.Length;
            if (componentCount == 1)
            {
                var c = frame.Components[0];
                byte[] yPlane = UpsampleBuffer.UpsampleToFullResolution(
                    entropy.Planes[0],
                    entropy.PlaneWidths[0],
                    entropy.PlaneHeights[0],
                    frame.Width,
                    frame.Height,
                    c.HSampling,
                    c.VSampling,
                    entropy.MaxHorizontalSampling,
                    entropy.MaxVerticalSampling);

                for (int y = 0; y < frame.Height; y++)
                {
                    int srcRow = y * frame.Width;
                    int dstRow = y * bmp._stride;
                    for (int x = 0; x < frame.Width; x++)
                    {
                        ColorConverter.GrayscaleToRgb(yPlane[srcRow + x], out byte r, out byte g, out byte b);
                        int o = dstRow + x * 4;
                        bmp._pixels[o] = b;
                        bmp._pixels[o + 1] = g;
                        bmp._pixels[o + 2] = r;
                        bmp._pixels[o + 3] = 255;
                    }
                }

                return bmp;
            }

            if (componentCount < 3)
                throw new NotSupportedException("JPEG with two components is not supported.");

            int yIndex = ResolveComponentIndex(frame, 1, 0);
            int cbIndex = ResolveComponentIndex(frame, 2, 1);
            int crIndex = ResolveComponentIndex(frame, 3, 2);

            byte[] fullY = Upsample(frame, entropy, yIndex, frame.Width, frame.Height);
            byte[] fullCb = Upsample(frame, entropy, cbIndex, frame.Width, frame.Height);
            byte[] fullCr = Upsample(frame, entropy, crIndex, frame.Width, frame.Height);

            for (int y = 0; y < frame.Height; y++)
            {
                int srcRow = y * frame.Width;
                int dstRow = y * bmp._stride;
                for (int x = 0; x < frame.Width; x++)
                {
                    int src = srcRow + x;
                    ColorConverter.YCbCrToRgb(fullY[src], fullCb[src], fullCr[src], out byte r, out byte g, out byte b);

                    int dst = dstRow + x * 4;
                    bmp._pixels[dst] = b;
                    bmp._pixels[dst + 1] = g;
                    bmp._pixels[dst + 2] = r;
                    bmp._pixels[dst + 3] = 255;
                }
            }

            return bmp;
        }

        private static byte[] Upsample(FrameHeader frame, EntropyDecodeResult entropy, int componentIndex, int width, int height)
        {
            var component = frame.Components[componentIndex];
            return UpsampleBuffer.UpsampleToFullResolution(
                entropy.Planes[componentIndex],
                entropy.PlaneWidths[componentIndex],
                entropy.PlaneHeights[componentIndex],
                width,
                height,
                component.HSampling,
                component.VSampling,
                entropy.MaxHorizontalSampling,
                entropy.MaxVerticalSampling);
        }

        private static int ResolveComponentIndex(FrameHeader frame, byte componentId, int fallbackIndex)
        {
            for (int i = 0; i < frame.Components.Length; i++)
            {
                if (frame.Components[i].Id == componentId)
                    return i;
            }

            return Math.Min(frame.Components.Length - 1, Math.Max(0, fallbackIndex));
        }

        private static void ApplyJfifDensity(Bitmap bmp, JfifHeader? jfif)
        {
            if (jfif == null)
                return;

            if (jfif.DensityUnits == 1)
            {
                if (jfif.XDensity > 0) bmp._dpiX = jfif.XDensity;
                if (jfif.YDensity > 0) bmp._dpiY = jfif.YDensity;
            }
            else if (jfif.DensityUnits == 2)
            {
                if (jfif.XDensity > 0) bmp._dpiX = (float)(jfif.XDensity * 2.54);
                if (jfif.YDensity > 0) bmp._dpiY = (float)(jfif.YDensity * 2.54);
            }
        }
    }
}
