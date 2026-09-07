using System;
using System.Collections.Generic;
using System.IO;
using MiniSoftware.Drawing.Codecs.Png;
using MiniSoftware.Drawing.Enums;
using MiniSoftware.Drawing.Imaging;

namespace MiniSoftware.Drawing.Codecs
{
    internal sealed class PngCodec : IImageCodec
    {
        private static readonly byte[] Signature = { 137, 80, 78, 71, 13, 10, 26, 10 };

        public bool CanDecode(byte[] header)
        {
            if (header.Length < 8) return false;
            for (int i = 0; i < 8; i++)
                if (header[i] != Signature[i]) return false;
            return true;
        }

        // ─── Decode ──────────────────────────────────────────────────────────────

        public Bitmap Decode(Stream stream)
        {
            var sig = new byte[8];
            PngHelpers.ReadFull(stream, sig, 0, 8);

            int width = 0, height = 0, bitDepth = 0, colorType = 0, interlace = 0;
            byte[]? palette = null;
            byte[]? trnsData = null;
            int dpiX = 0, dpiY = 0;
            var idatBuffer = new List<byte>();

            ApngAnimationControl? acTL = null;
            ApngFrameControl? pendingFC = null;
            ApngFrameControl? defaultImageFC = null;
            var frameDataList = new List<(ApngFrameControl fc, List<byte> data)>();
            bool defaultImageIsFrame0 = false;
            bool sawFirstIdat = false;

            while (true)
            {
                var lenBuf = new byte[8];
                PngHelpers.ReadFull(stream, lenBuf, 0, 8);
                int chunkLen = (int)BinaryBE.ReadUInt32(lenBuf, 0);
                string chunkType = System.Text.Encoding.ASCII.GetString(lenBuf, 4, 4);

                byte[] chunkData = new byte[chunkLen];
                if (chunkLen > 0) PngHelpers.ReadFull(stream, chunkData, 0, chunkLen);

                var crcBuf = new byte[4];
                PngHelpers.ReadFull(stream, crcBuf, 0, 4);

                switch (chunkType)
                {
                    case "IHDR":
                        width = (int)BinaryBE.ReadUInt32(chunkData, 0);
                        height = (int)BinaryBE.ReadUInt32(chunkData, 4);
                        bitDepth = chunkData[8];
                        colorType = chunkData[9];
                        interlace = chunkData[12];
                        if (interlace != 0)
                            throw new NotSupportedException("Interlaced PNG is not supported.");
                        break;

                    case "PLTE":
                        palette = chunkData;
                        break;

                    case "tRNS":
                        trnsData = chunkData;
                        break;

                    case "pHYs":
                        if (chunkLen >= 9 && chunkData[8] == 1)
                        {
                            dpiX = (int)(BinaryBE.ReadUInt32(chunkData, 0) * 0.0254f);
                            dpiY = (int)(BinaryBE.ReadUInt32(chunkData, 4) * 0.0254f);
                        }
                        break;

                    case "acTL":
                        acTL = ApngAnimationControl.Parse(chunkData);
                        break;

                    case "fcTL":
                        pendingFC = ApngFrameControl.Parse(chunkData);
                        if (!sawFirstIdat && defaultImageFC == null)
                        {
                            defaultImageFC = pendingFC;
                            defaultImageIsFrame0 = true;
                        }
                        break;

                    case "IDAT":
                        idatBuffer.AddRange(chunkData);
                        if (!sawFirstIdat)
                            sawFirstIdat = true;
                        break;

                    case "fdAT":
                        if (pendingFC.HasValue)
                        {
                            var data = new List<byte>(chunkLen - 4);
                            for (int i = 4; i < chunkLen; i++)
                                data.Add(chunkData[i]);
                            frameDataList.Add((pendingFC.Value, data));
                            pendingFC = null;
                        }
                        break;

                    case "IEND":
                        goto done;
                }
            }
            done:

            byte[] compressed = idatBuffer.ToArray();
            byte[] raw = ZlibHelper.Decompress(compressed);

            int channels = PngHelpers.GetChannels(colorType);
            int stride = (width * bitDepth * channels + 7) / 8;
            int bppFilter = Math.Max(1, (bitDepth * channels) / 8);

            byte[] pixels = PngHelpers.ReconstructFilters(raw, width, height, stride, bppFilter);

            var bmp = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            bmp._rawFormat = ImageFormat.Png;
            if (dpiX > 0) bmp._dpiX = dpiX;
            if (dpiY > 0) bmp._dpiY = dpiY;

            for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                PngHelpers.GetPixelRgba(pixels, x, y, stride, bitDepth, colorType,
                             palette, trnsData, out byte r, out byte g, out byte b, out byte a);
                int off = y * bmp._stride + x * 4;
                bmp._pixels[off] = b;
                bmp._pixels[off + 1] = g;
                bmp._pixels[off + 2] = r;
                bmp._pixels[off + 3] = a;
            }

            if (acTL.HasValue && acTL.Value.NumFrames > 0)
            {
                var frames = new List<AnimationFrame>();

                if (defaultImageIsFrame0 && defaultImageFC.HasValue)
                {
                    frames.Add(ApngFrameBuilder.Build(
                        defaultImageFC.Value, compressed, bitDepth, colorType, palette, trnsData));
                }

                foreach (var (fc, data) in frameDataList)
                {
                    frames.Add(ApngFrameBuilder.Build(
                        fc, data.ToArray(), bitDepth, colorType, palette, trnsData));
                }

                bmp.SetAnimationFrames(frames, acTL.Value.NumPlays);
            }

            return bmp;
        }

        // ─── Encode ──────────────────────────────────────────────────────────────

        public void Encode(Bitmap bmp, Stream stream, EncoderParameters? parameters)
        {
            int frameCount = bmp.GetFrameCount(FrameDimension.Time);
            if (frameCount > 1)
            {
                EncodeApng(bmp, stream, frameCount);
                return;
            }

            EncodeStatic(bmp, stream);
        }

        private static void EncodeStatic(Bitmap bmp, Stream stream)
        {
            bool hasAlpha = bmp._format == PixelFormat.Format32bppArgb
                         || bmp._format == PixelFormat.Format32bppPArgb;
            int channels = hasAlpha ? 4 : 3;
            int colorTypeOut = hasAlpha ? 6 : 2;

            int rawStride = bmp._width * channels;
            var rawData = new byte[(rawStride + 1) * bmp._height];
            for (int y = 0; y < bmp._height; y++)
            {
                int rawRow = y * (rawStride + 1);
                rawData[rawRow] = 0;
                for (int x = 0; x < bmp._width; x++)
                {
                    int srcOff = y * bmp._stride + x * Bitmap.GetBytesPerPixel(bmp._format);
                    byte b = bmp._pixels[srcOff];
                    byte g = bmp._pixels[srcOff + 1];
                    byte r = bmp._pixels[srcOff + 2];
                    byte a = Bitmap.GetBytesPerPixel(bmp._format) >= 4 ? bmp._pixels[srcOff + 3] : (byte)255;

                    int dst = rawRow + 1 + x * channels;
                    rawData[dst] = r;
                    rawData[dst + 1] = g;
                    rawData[dst + 2] = b;
                    if (hasAlpha) rawData[dst + 3] = a;
                }
            }

            byte[] idatData = ZlibHelper.Compress(rawData);

            uint ppmX = (uint)Math.Round(bmp._dpiX / 0.0254);
            uint ppmY = (uint)Math.Round(bmp._dpiY / 0.0254);

            stream.Write(Signature, 0, 8);

            var ihdr = new byte[13];
            BinaryBE.WriteUInt32(ihdr, 0, (uint)bmp._width);
            BinaryBE.WriteUInt32(ihdr, 4, (uint)bmp._height);
            ihdr[8] = 8;
            ihdr[9] = (byte)colorTypeOut;
            ihdr[10] = 0; ihdr[11] = 0; ihdr[12] = 0;
            PngHelpers.WriteChunk(stream, "IHDR", ihdr);

            var phys = new byte[9];
            BinaryBE.WriteUInt32(phys, 0, ppmX);
            BinaryBE.WriteUInt32(phys, 4, ppmY);
            phys[8] = 1;
            PngHelpers.WriteChunk(stream, "pHYs", phys);

            PngHelpers.WriteChunk(stream, "IDAT", idatData);
            PngHelpers.WriteChunk(stream, "IEND", Array.Empty<byte>());
        }

        private static void EncodeApng(Bitmap bmp, Stream stream, int frameCount)
        {
            int canvasW = bmp._width;
            int canvasH = bmp._height;

            uint ppmX = (uint)Math.Round(bmp._dpiX / 0.0254);
            uint ppmY = (uint)Math.Round(bmp._dpiY / 0.0254);

            stream.Write(Signature, 0, 8);

            var ihdr = new byte[13];
            BinaryBE.WriteUInt32(ihdr, 0, (uint)canvasW);
            BinaryBE.WriteUInt32(ihdr, 4, (uint)canvasH);
            ihdr[8] = 8;
            ihdr[9] = 6;
            ihdr[10] = 0; ihdr[11] = 0; ihdr[12] = 0;
            PngHelpers.WriteChunk(stream, "IHDR", ihdr);

            var phys = new byte[9];
            BinaryBE.WriteUInt32(phys, 0, ppmX);
            BinaryBE.WriteUInt32(phys, 4, ppmY);
            phys[8] = 1;
            PngHelpers.WriteChunk(stream, "pHYs", phys);

            var actl = new byte[8];
            BinaryBE.WriteUInt32(actl, 0, (uint)frameCount);
            BinaryBE.WriteUInt32(actl, 4, 0);
            PngHelpers.WriteChunk(stream, "acTL", actl);

            uint seq = 0;
            for (int i = 0; i < frameCount; i++)
            {
                bmp.SelectActiveFrame(FrameDimension.Time, i);
                byte[] framePixels = bmp._pixels;

                int rawStride = canvasW * 4;
                var rawData = new byte[(rawStride + 1) * canvasH];
                for (int y = 0; y < canvasH; y++)
                {
                    int rawRow = y * (rawStride + 1);
                    rawData[rawRow] = 0;
                    int srcOff = y * bmp._stride;
                    for (int x = 0; x < canvasW; x++)
                    {
                        int dst = rawRow + 1 + x * 4;
                        rawData[dst] = framePixels[srcOff + x * 4 + 2];
                        rawData[dst + 1] = framePixels[srcOff + x * 4 + 1];
                        rawData[dst + 2] = framePixels[srcOff + x * 4];
                        rawData[dst + 3] = framePixels[srcOff + x * 4 + 3];
                    }
                }

                byte[] compressed = ZlibHelper.Compress(rawData);

                var fctl = new byte[26];
                BinaryBE.WriteUInt32(fctl, 0, seq++);
                BinaryBE.WriteUInt32(fctl, 4, (uint)canvasW);
                BinaryBE.WriteUInt32(fctl, 8, (uint)canvasH);
                BinaryBE.WriteUInt32(fctl, 12, 0);
                BinaryBE.WriteUInt32(fctl, 16, 0);
                BinaryBE.WriteUInt16(fctl, 20, 10);
                BinaryBE.WriteUInt16(fctl, 22, 100);
                fctl[24] = 0;
                fctl[25] = 0;
                PngHelpers.WriteChunk(stream, "fcTL", fctl);

                if (i == 0)
                {
                    PngHelpers.WriteChunk(stream, "IDAT", compressed);
                }
                else
                {
                    var fdat = new byte[4 + compressed.Length];
                    BinaryBE.WriteUInt32(fdat, 0, seq++);
                    Buffer.BlockCopy(compressed, 0, fdat, 4, compressed.Length);
                    PngHelpers.WriteChunk(stream, "fdAT", fdat);
                }
            }

            PngHelpers.WriteChunk(stream, "IEND", Array.Empty<byte>());
        }
    }
}