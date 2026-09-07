using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniSoftware.Drawing.Metafile.EmfPlus
{
    using System.IO;

    using MiniSoftware.Drawing.Metafile.EmfPlus.Enumerations;
    using MiniSoftware.Drawing.Metafile.EmfPlus.Records;

    internal class EmfPlusReader
    {
        public List<Record> Records { get; } = new();

        public uint DataSize { get; private set; }

        public uint CommentIdentifier { get; private set; }

        //public MetaHeader Header { get; private set; }

        public void Read(Stream stream)
        {
            var reader = new BinaryReader(stream);

            DataSize = reader.ReadUInt32();
            CommentIdentifier = reader.ReadUInt32();

            while (stream.Position < stream.Length)
            {
                
                var header = new RecordHeader();
                header.Read(reader);

                //long expectedEndPosition = stream.Position + (header.Size) - 8;
                var record = CreateRecord(header, reader);
                if (record == null)
                {
                    record = new Record { Header = header };
                    record.Read(reader);
                }
                
                this.Records.Add(record);
                //stream.Position = expectedEndPosition;
            }
        }

        public static Record? CreateRecord(RecordHeader header, BinaryReader reader)
        {
            Record? record = header.Type switch
            {
                RecordType.EmfPlusHeader => new EmfPlusHeaderRecord(),
                RecordType.EmfPlusEndOfFile => new EmfPlusEndOfFileRecord(),
                RecordType.EmfPlusComment => new EmfPlusCommentRecord(),
                RecordType.EmfPlusGetDC => new EmfPlusGetDCRecord(),
                RecordType.EmfPlusMultiFormatStart => new EmfPlusMultiFormatStartRecord(),
                RecordType.EmfPlusMultiFormatSection => new EmfPlusMultiFormatSectionRecord(),
                RecordType.EmfPlusMultiFormatEnd => new EmfPlusMultiFormatEndRecord(),
                RecordType.EmfPlusObject => new EmfPlusObjectRecord(),
                RecordType.EmfPlusClear => new EmfPlusClearRecord(),
                RecordType.EmfPlusFillRects => new EmfPlusFillRectsRecord(),
                RecordType.EmfPlusDrawRects => new EmfPlusDrawRectsRecord(),
                RecordType.EmfPlusFillPolygon => new EmfPlusFillPolygonRecord(),
                RecordType.EmfPlusDrawLines => new EmfPlusDrawLinesRecord(),
                RecordType.EmfPlusFillEllipse => new EmfPlusFillEllipseRecord(),
                RecordType.EmfPlusDrawEllipse => new EmfPlusDrawEllipseRecord(),
                RecordType.EmfPlusFillPie => new EmfPlusFillPieRecord(),
                RecordType.EmfPlusDrawPie => new EmfPlusDrawPieRecord(),
                RecordType.EmfPlusDrawArc => new EmfPlusDrawArcRecord(),
                RecordType.EmfPlusFillRegion => new EmfPlusFillRegionRecord(),
                RecordType.EmfPlusFillPath => new EmfPlusFillPathRecord(),
                RecordType.EmfPlusDrawPath => new EmfPlusDrawPathRecord(),
                RecordType.EmfPlusFillClosedCurve => new EmfPlusFillClosedCurveRecord(),
                RecordType.EmfPlusDrawClosedCurve => new EmfPlusDrawClosedCurveRecord(),
                RecordType.EmfPlusDrawCurve => new EmfPlusDrawCurveRecord(),
                RecordType.EmfPlusDrawBeziers => new EmfPlusDrawBeziersRecord(),
                RecordType.EmfPlusDrawImage => new EmfPlusDrawImageRecord(),
                RecordType.EmfPlusDrawImagePoints => new EmfPlusDrawImagePointsRecord(),
                RecordType.EmfPlusDrawString => new EmfPlusDrawStringRecord(),
                RecordType.EmfPlusSetRenderingOrigin => new EmfPlusSetRenderingOriginRecord(),
                RecordType.EmfPlusSetAntiAliasMode => new EmfPlusSetAntiAliasModeRecord(),
                RecordType.EmfPlusSetTextRenderingHint => new EmfPlusSetTextRenderingHintRecord(),
                RecordType.EmfPlusSetTextContrast => new EmfPlusSetTextContrastRecord(),
                RecordType.EmfPlusSetInterpolationMode => new EmfPlusSetInterpolationModeRecord(),
                RecordType.EmfPlusSetPixelOffsetMode => new EmfPlusSetPixelOffsetModeRecord(),
                RecordType.EmfPlusSetCompositingMode => new EmfPlusSetCompositingModeRecord(),
                RecordType.EmfPlusSetCompositingQuality => new EmfPlusSetCompositingQualityRecord(),
                RecordType.EmfPlusSave => new EmfPlusSaveRecord(),
                RecordType.EmfPlusRestore => new EmfPlusRestoreRecord(),
                RecordType.EmfPlusBeginContainer => new EmfPlusBeginContainerRecord(),
                RecordType.EmfPlusBeginContainerNoParams => new EmfPlusBeginContainerNoParamsRecord(),
                RecordType.EmfPlusEndContainer => new EmfPlusEndContainerRecord(),
                RecordType.EmfPlusSetWorldTransform => new EmfPlusSetWorldTransformRecord(),
                RecordType.EmfPlusResetWorldTransform => new EmfPlusResetWorldTransformRecord(),
                RecordType.EmfPlusMultiplyWorldTransform => new EmfPlusMultiplyWorldTransformRecord(),
                RecordType.EmfPlusTranslateWorldTransform => new EmfPlusTranslateWorldTransformRecord(),
                RecordType.EmfPlusScaleWorldTransform => new EmfPlusScaleWorldTransformRecord(),
                RecordType.EmfPlusRotateWorldTransform => new EmfPlusRotateWorldTransformRecord(),
                RecordType.EmfPlusSetPageTransform => new EmfPlusSetPageTransformRecord(),
                RecordType.EmfPlusResetClip => new EmfPlusResetClipRecord(),
                RecordType.EmfPlusSetClipRect => new EmfPlusSetClipRectRecord(),
                RecordType.EmfPlusSetClipPath => new EmfPlusSetClipPathRecord(),
                RecordType.EmfPlusSetClipRegion => new EmfPlusSetClipRegionRecord(),
                RecordType.EmfPlusOffsetClip => new EmfPlusOffsetClipRecord(),
                RecordType.EmfPlusDrawDriverstring => new EmfPlusDrawDriverstringRecord(),
                RecordType.EmfPlusStrokeFillPath => new EmfPlusStrokeFillPathRecord(),
                RecordType.EmfPlusSerializableObject => new EmfPlusSerializableObjectRecord(),
                RecordType.EmfPlusSetTSGraphics => new EmfPlusSetTSGraphicsRecord(),
                RecordType.EmfPlusSetTSClip => new EmfPlusSetTSClipRecord(),
                _ => null,
            };

            if (record is not null)
            {
                record.Header = header;
                record.Read(reader);
            }

            return record;
        }
    }
}
