using MiniSoftware.Drawing.Metafile.EmfPlus;
using MiniSoftware.Drawing.Metafile.Wmf;
using MiniSoftware.Drawing.Metafile.Wmf.Records;

namespace MiniSoftware.Drawing.Metafile
{
    using MiniSoftware.Drawing.Metafile.Emf;
    using System;
    using System.Collections.Generic;
    using System.IO;

    /// <summary>
    /// Reads metafile formats (WMF, EMF, EMF+) from streams, byte arrays, or files.
    /// </summary>
    public class MetafileReader
    {
        private const uint WmfPlaceableKey = 0x9AC6CDD7;

        private const uint EmfSignature = 0x464D4520;

        private const uint EmfPlusCommentIdentifier = 0x2B464D45;

        /// <summary>
        /// Reads a metafile from the specified stream.
        /// </summary>
        /// <param name="stream">The stream containing metafile data.</param>
        /// <returns>The <see cref="MetafileDocument"/> read from the stream.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="stream"/> is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the stream does not support seeking.</exception>
        /// <exception cref="InvalidDataException">Thrown when the metafile format is unsupported or unknown.</exception>
        public MetafileDocument Read(Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            if (!stream.CanSeek)
            {
                throw new InvalidOperationException("The input stream must support seeking.");
            }

            stream.Position = 0;
            var format = DetectFormat(stream);
            stream.Position = 0;

            return format switch
            {
                MetafileFormat.Wmf => ReadWmf(stream),
                MetafileFormat.Emf => ReadEmf(stream),
                MetafileFormat.EmfPlus => ReadEmfPlus(stream),
                _ => throw new InvalidDataException("Unsupported or unknown metafile format."),
            };
        }

        /// <summary>
        /// Reads a metafile from the specified byte array.
        /// </summary>
        /// <param name="data">The byte array containing metafile data.</param>
        /// <returns>The <see cref="MetafileDocument"/> read from the byte array.</returns>
        public MetafileDocument Read(byte[] data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            using var stream = new MemoryStream(data, writable: false);
            return Read(stream);
        }

        /// <summary>
        /// Reads a metafile from the specified file path.
        /// </summary>
        /// <param name="filePath">The path to the metafile.</param>
        /// <returns>The <see cref="MetafileDocument"/> read from the file.</returns>
        public MetafileDocument Read(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException("The file path cannot be null or whitespace.", nameof(filePath));
            }

            using var stream = File.OpenRead(filePath);
            return Read(stream);
        }

        /// <summary>
        /// Attempts to read a metafile from the specified stream.
        /// </summary>
        /// <param name="stream">The stream containing metafile data.</param>
        /// <param name="document">The resulting <see cref="MetafileDocument"/>, or null if reading failed.</param>
        /// <returns><c>true</c> if the metafile was read successfully; otherwise, <c>false</c>.</returns>
        public bool TryRead(Stream stream, out MetafileDocument? document)
        {
            try
            {
                document = Read(stream);
                return true;
            }
            catch (InvalidDataException)
            {
                document = null;
                return false;
            }
        }

        /// <summary>
        /// Detects the format of a metafile from the specified stream.
        /// </summary>
        /// <param name="stream">The stream containing metafile data.</param>
        /// <returns>The detected <see cref="MetafileFormat"/>.</returns>
        public MetafileFormat DetectFormat(Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            if (!stream.CanSeek)
            {
                throw new InvalidOperationException("The input stream must support seeking.");
            }

            long originalPosition = stream.Position;
            try
            {
                if (LooksLikePlaceableWmf(stream))
                {
                    return MetafileFormat.Wmf;
                }

                if (LooksLikeEmf(stream))
                {
                    return MetafileFormat.Emf;
                }

                if (LooksLikeStandardWmf(stream))
                {
                    return MetafileFormat.Wmf;
                }

                if (LooksLikeEmfPlus(stream))
                {
                    return MetafileFormat.EmfPlus;
                }

                return MetafileFormat.Unknown;
            }
            finally
            {
                stream.Position = originalPosition;
            }
        }

        /// <summary>
        /// Detects the format of a metafile from the specified byte array.
        /// </summary>
        /// <param name="data">The byte array containing metafile data.</param>
        /// <returns>The detected <see cref="MetafileFormat"/>.</returns>
        public MetafileFormat DetectFormat(byte[] data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            using var stream = new MemoryStream(data, writable: false);
            return DetectFormat(stream);
        }

        private static MetafileDocument ReadWmf(Stream stream)
        {
            META_PLACEABLE? placeableHeader = null;

            if (LooksLikePlaceableWmf(stream))
            {
                using var reader = new BinaryReader(stream, System.Text.Encoding.Default, leaveOpen: true);
                placeableHeader = new META_PLACEABLE();
                placeableHeader.Read(reader);
            }

            var wmfReader = new WmfReader();
            wmfReader.Read(stream);

            return new MetafileDocument
            {
                Format = MetafileFormat.Wmf,
                WmfHeader = wmfReader.Header,
                WmfRecords = wmfReader.Records,
                PlaceableHeader = placeableHeader,
            };
        }

        private static MetafileDocument ReadEmf(Stream stream)
        {
            var emfReader = new EmfReader();
            emfReader.Read(stream);
            var embeddedEmfPlusRecords = ExtractEmbeddedEmfPlusRecords(emfReader);

            return new MetafileDocument
            {
                Format = MetafileFormat.Emf,
                EmfRecords = emfReader.Records,
                EmbeddedEmfPlusRecords = embeddedEmfPlusRecords,
            };
        }

        private static MetafileDocument ReadEmfPlus(Stream stream)
        {
            var emfPlusReader = new EmfPlusReader();
            emfPlusReader.Read(stream);

            return new MetafileDocument
            {
                Format = MetafileFormat.EmfPlus,
                EmfPlusDataSize = emfPlusReader.DataSize,
                EmfPlusCommentIdentifier = emfPlusReader.CommentIdentifier,
                EmfPlusRecords = emfPlusReader.Records,
            };
        }

        private static bool LooksLikePlaceableWmf(Stream stream)
        {
            if (stream.Length < 22)
            {
                return false;
            }

            long originalPosition = stream.Position;
            try
            {
                stream.Position = 0;
                using var reader = new BinaryReader(stream, System.Text.Encoding.Default, leaveOpen: true);
                return reader.ReadUInt32() == WmfPlaceableKey;
            }
            finally
            {
                stream.Position = originalPosition;
            }
        }

        private static bool LooksLikeStandardWmf(Stream stream)
        {
            if (stream.Length < 18)
            {
                return false;
            }

            long originalPosition = stream.Position;
            try
            {
                stream.Position = 0;
                using var reader = new BinaryReader(stream, System.Text.Encoding.Default, leaveOpen: true);

                var type = reader.ReadUInt16();
                var headerSize = reader.ReadUInt16();
                var version = reader.ReadUInt16();

                return (type == 0x0001 || type == 0x0002)
                    && headerSize == 9
                    && (version == 0x0100 || version == 0x0300);
            }
            finally
            {
                stream.Position = originalPosition;
            }
        }

        private static bool LooksLikeEmf(Stream stream)
        {
            if (stream.Length < 44)
            {
                return false;
            }

            long originalPosition = stream.Position;
            try
            {
                stream.Position = 0;
                using var reader = new BinaryReader(stream, System.Text.Encoding.Default, leaveOpen: true);

                var type = reader.ReadUInt32();
                var size = reader.ReadUInt32();

                if (type != (uint)Metafile.Emf.Enumerations.RecordType.EMR_HEADER || size < 80)
                {
                    return false;
                }

                stream.Position = 40;
                var signature = reader.ReadUInt32();
                return signature == EmfSignature;
            }
            finally
            {
                stream.Position = originalPosition;
            }
        }

        private static bool LooksLikeEmfPlus(Stream stream)
        {
            if (stream.Length < 20)
            {
                return false;
            }

            long originalPosition = stream.Position;
            try
            {
                stream.Position = 0;
                using var reader = new BinaryReader(stream, System.Text.Encoding.Default, leaveOpen: true);

                var dataSize = reader.ReadUInt32();
                var commentIdentifier = reader.ReadUInt32();

                if (dataSize == 0 || commentIdentifier != EmfPlusCommentIdentifier)
                {
                    return false;
                }

                var recordType = reader.ReadUInt16();
                return recordType >= (ushort)Metafile.EmfPlus.Enumerations.RecordType.EmfPlusHeader
                    && recordType <= (ushort)Metafile.EmfPlus.Enumerations.RecordType.EmfPlusSetTSClip;
            }
            finally
            {
                stream.Position = originalPosition;
            }
        }

        private static IReadOnlyList<IReadOnlyList<Metafile.EmfPlus.Records.Record>> ExtractEmbeddedEmfPlusRecords(EmfReader emfReader)
        {
            var embeddedRecords = new List<IReadOnlyList<Metafile.EmfPlus.Records.Record>>();

            foreach (var record in emfReader.Records)
            {
                if (record is not Metafile.Emf.Records.StateRecordTypes.EMR_COMMENT commentRecord)
                {
                    continue;
                }

                using var commentStream = new MemoryStream(commentRecord.CommentData, writable: false);
                if (!LooksLikeEmfPlus(commentStream))
                {
                    continue;
                }

                commentStream.Position = 0;

                try
                {
                    var emfPlusReader = new EmfPlusReader();
                    emfPlusReader.Read(commentStream);
                    embeddedRecords.Add(emfPlusReader.Records);
                }
                catch (IOException)
                {
                }
                catch (ArgumentException)
                {
                }
            }

            return embeddedRecords;
        }
    }
}
