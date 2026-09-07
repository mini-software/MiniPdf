using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniPdf.Drawing.Metafile.Emf
{
    using System.IO;
    using MiniPdf.Drawing.Metafile.Emf.Enumerations;
    using MiniPdf.Drawing.Metafile.Emf.Records;
    using MiniPdf.Drawing.Metafile.Emf.Records.DrawingRecordTypes;
    using MiniPdf.Drawing.Metafile.Emf.Records.ObjectRecordTypes;
    using MiniPdf.Drawing.Metafile.Emf.Records.StateRecordTypes;
    using MiniPdf.Drawing.Metafile.EmfPlus;

    internal class EmfReader
    {
        public List<Record> Records = new List<Record>();

        //public MetaHeader Header { get; private set; }

        public void Read(Stream stream)
        {
            var reader = new BinaryReader(stream);

            

            while (stream.Position < stream.Length)
            {
                var header = new RecordHeader();
                header.Read(reader);

                long expectedEndPosition = stream.Position + (header.Size) - 8;
                var record = CreateRecord(header, reader);
                if (record == null)
                {
                    record = new Record();
                    record.Header = header;
                    record.Data = reader.ReadBytes(Convert.ToInt32(header.Size) - 8);

                }

                if (header.Type == RecordType.EMR_COMMENT)
                {
                    byte[] commentPayload = record switch
                    {
                        EMR_COMMENT commentRecord => commentRecord.CommentData,
                        _ => record.Data,
                    };

                    using (var commentStream = new MemoryStream(commentPayload))
                    {
                        var emfPlusReader = new EmfPlusReader();
                        emfPlusReader.Read(commentStream);
                    }
                }

                this.Records.Add(record);
                stream.Position = expectedEndPosition;
            }
        }

        public static Record? CreateRecord(RecordHeader header, BinaryReader reader)
        {
            Record? record = header.Type switch
            {
                RecordType.EMR_HEADER => new EMR_HEADER(),
                RecordType.EMR_POLYBEZIER => new EMR_POLYBEZIER(),
                RecordType.EMR_POLYGON => new EMR_POLYGON(),
                RecordType.EMR_POLYLINE => new EMR_POLYLINE(),
                RecordType.EMR_POLYBEZIERTO => new EMR_POLYBEZIERTO(),
                RecordType.EMR_POLYLINETO => new EMR_POLYLINETO(),
                RecordType.EMR_POLYPOLYLINE => new EMR_POLYPOLYLINE(),
                RecordType.EMR_POLYPOLYGON => new EMR_POLYPOLYGON(),
                RecordType.EMR_ANGLEARC => new EMR_ANGLEARC(),
                RecordType.EMR_EOF => new EMR_EOF(),
                RecordType.EMR_LINETO => new EMR_LINETO(),
                RecordType.EMR_ARCTO => new EMR_ARCTO(),
                RecordType.EMR_ARC => new EMR_ARC(),
                RecordType.EMR_CHORD => new EMR_CHORD(),
                RecordType.EMR_ELLIPSE => new EMR_ELLIPSE(),
                RecordType.EMR_RECTANGLE => new EMR_RECTANGLE(),
                RecordType.EMR_ROUNDRECT => new EMR_ROUNDRECT(),
                RecordType.EMR_PIE => new EMR_PIE(),
                RecordType.EMR_EXTFLOODFILL => new EMR_EXTFLOODFILL(),
                RecordType.EMR_SETWINDOWEXTEX => new EMR_SETWINDOWEXTEX(),
                RecordType.EMR_SETWINDOWORGEX => new EMR_SETWINDOWORGEX(),
                RecordType.EMR_SETVIEWPORTEXTEX => new EMR_SETVIEWPORTEXTEX(),
                RecordType.EMR_SETVIEWPORTORGEX => new EMR_SETVIEWPORTORGEX(),
                RecordType.EMR_SETBRUSHORGEX => new EMR_SETBRUSHORGEX(),
                RecordType.EMR_SETPIXELV => new EMR_SETPIXELV(),
                RecordType.EMR_SETMAPPERFLAGS => new EMR_SETMAPPERFLAGS(),
                RecordType.EMR_SETMAPMODE => new EMR_SETMAPMODE(),
                RecordType.EMR_SETBKMODE => new EMR_SETBKMODE(),
                RecordType.EMR_SETPOLYFILLMODE => new EMR_SETPOLYFILLMODE(),
                RecordType.EMR_SETROP2 => new EMR_SETROP2(),
                RecordType.EMR_SETSTRETCHBLTMODE => new EMR_SETSTRETCHBLTMODE(),
                RecordType.EMR_SETTEXTALIGN => new EMR_SETTEXTALIGN(),
                RecordType.EMR_SETCOLORADJUSTMENT => new EMR_SETCOLORADJUSTMENT(),
                RecordType.EMR_SETTEXTCOLOR => new EMR_SETTEXTCOLOR(),
                RecordType.EMR_SETBKCOLOR => new EMR_SETBKCOLOR(),
                RecordType.EMR_OFFSETCLIPRGN => new EMR_OFFSETCLIPRGN(),
                RecordType.EMR_MOVETOEX => new EMR_MOVETOEX(),
                RecordType.EMR_SETMETARGN => new EMR_SETMETARGN(),
                RecordType.EMR_EXCLUDECLIPRECT => new EMR_EXCLUDECLIPRECT(),
                RecordType.EMR_INTERSECTCLIPRECT => new EMR_INTERSECTCLIPRECT(),
                RecordType.EMR_SCALEVIEWPORTEXTEX => new EMR_SCALEVIEWPORTEXTEX(),
                RecordType.EMR_SCALEWINDOWEXTEX => new EMR_SCALEWINDOWEXTEX(),
                RecordType.EMR_SAVEDC => new EMR_SAVEDC(),
                RecordType.EMR_RESTOREDC => new EMR_RESTOREDC(),
                RecordType.EMR_SETWORLDTRANSFORM => new EMR_SETWORLDTRANSFORM(),
                RecordType.EMR_MODIFYWORLDTRANSFORM => new EMR_MODIFYWORLDTRANSFORM(),
                RecordType.EMR_SELECTOBJECT => new EMR_SELECTOBJECT(),
                RecordType.EMR_CREATEPEN => new EMR_CREATEPEN(),
                RecordType.EMR_CREATEBRUSHINDIRECT => new EMR_CREATEBRUSHINDIRECT(),
                RecordType.EMR_SELECTPALETTE => new EMR_SELECTPALETTE(),
                RecordType.EMR_CREATEPALETTE => new EMR_CREATEPALETTE(),
                RecordType.EMR_SETPALETTEENTRIES => new EMR_SETPALETTEENTRIES(),
                RecordType.EMR_RESIZEPALETTE => new EMR_RESIZEPALETTE(),
                RecordType.EMR_REALIZEPALETTE => new EMR_REALIZEPALETTE(),
                RecordType.EMR_DELETEOBJECT => new EMR_DELETEOBJECT(),
                RecordType.EMR_SETARCDIRECTION => new EMR_SETARCDIRECTION(),
                RecordType.EMR_SETMITERLIMIT => new EMR_SETMITERLIMIT(),
                RecordType.EMR_POLYDRAW => new EMR_POLYDRAW(),
                RecordType.EMR_BEGINPATH => new EMR_BEGINPATH(),
                RecordType.EMR_ENDPATH => new EMR_ENDPATH(),
                RecordType.EMR_CLOSEFIGURE => new EMR_CLOSEFIGURE(),
                RecordType.EMR_FILLPATH => new EMR_FILLPATH(),
                RecordType.EMR_STROKEANDFILLPATH => new EMR_STROKEANDFILLPATH(),
                RecordType.EMR_STROKEPATH => new EMR_STROKEPATH(),
                RecordType.EMR_FLATTENPATH => new EMR_FLATTENPATH(),
                RecordType.EMR_WIDENPATH => new EMR_WIDENPATH(),
                RecordType.EMR_SELECTCLIPPATH => new EMR_SELECTCLIPPATH(),
                RecordType.EMR_ABORTPATH => new EMR_ABORTPATH(),
                RecordType.EMR_COMMENT => new EMR_COMMENT(),
                RecordType.EMR_FILLRGN => new EMR_FILLRGN(),
                RecordType.EMR_FRAMERGN => new EMR_FRAMERGN(),
                RecordType.EMR_INVERTRGN => new EMR_INVERTRGN(),
                RecordType.EMR_PAINTRGN => new EMR_PAINTRGN(),
                RecordType.EMR_EXTSELECTCLIPRGN => new EMR_EXTSELECTCLIPRGN(),
                RecordType.EMR_BITBLT => new EMR_BITBLT(),
                RecordType.EMR_STRETCHBLT => new EMR_STRETCHBLT(),
                RecordType.EMR_MASKBLT => new EMR_MASKBLT(),
                RecordType.EMR_PLGBLT => new EMR_PLGBLT(),
                RecordType.EMR_SETDIBITSTODEVICE => new EMR_SETDIBITSTODEVICE(),
                RecordType.EMR_STRETCHDIBITS => new EMR_STRETCHDIBITS(),
                RecordType.EMR_EXTCREATEFONTINDIRECTW => new EMR_EXTCREATEFONTINDIRECTW(),
                RecordType.EMR_EXTTEXTOUTA => new EMR_EXTTEXTOUTA(),
                RecordType.EMR_EXTTEXTOUTW => new EMR_EXTTEXTOUTW(),
                RecordType.EMR_POLYBEZIER16 => new EMR_POLYBEZIER16(),
                RecordType.EMR_POLYGON16 => new EMR_POLYGON16(),
                RecordType.EMR_POLYLINE16 => new EMR_POLYLINE16(),
                RecordType.EMR_POLYBEZIERTO16 => new EMR_POLYBEZIERTO16(),
                RecordType.EMR_POLYLINETO16 => new EMR_POLYLINETO16(),
                RecordType.EMR_POLYPOLYLINE16 => new EMR_POLYPOLYLINE16(),
                RecordType.EMR_POLYPOLYGON16 => new EMR_POLYPOLYGON16(),
                RecordType.EMR_POLYDRAW16 => new EMR_POLYDRAW16(),
                RecordType.EMR_CREATEMONOBRUSH => new EMR_CREATEMONOBRUSH(),
                RecordType.EMR_CREATEDIBPATTERNBRUSHPT => new EMR_CREATEDIBPATTERNBRUSHPT(),
                RecordType.EMR_EXTCREATEPEN => new EMR_EXTCREATEPEN(),
                RecordType.EMR_POLYTEXTOUTA => new EMR_POLYTEXTOUTA(),
                RecordType.EMR_POLYTEXTOUTW => new EMR_POLYTEXTOUTW(),
                RecordType.EMR_SETICMMODE => new EMR_SETICMMODE(),
                RecordType.EMR_CREATECOLORSPACE => new EMR_CREATECOLORSPACE(),
                RecordType.EMR_SETCOLORSPACE => new EMR_SETCOLORSPACE(),
                RecordType.EMR_DELETECOLORSPACE => new EMR_DELETECOLORSPACE(),
                RecordType.EMR_GLSRECORD => new EMR_GLSRECORD(),
                RecordType.EMR_GLSBOUNDEDRECORD => new EMR_GLSBOUNDEDRECORD(),
                RecordType.EMR_PIXELFORMAT => new EMR_PIXELFORMAT(),
                RecordType.EMR_DRAWESCAPE => new EMR_DRAWESCAPE(),
                RecordType.EMR_EXTESCAPE => new EMR_EXTESCAPE(),
                RecordType.EMR_SMALLTEXTOUT => new EMR_SMALLTEXTOUT(),
                RecordType.EMR_FORCEUFIMAPPING => new EMR_FORCEUFIMAPPING(),
                RecordType.EMR_NAMEDESCAPE => new EMR_NAMEDESCAPE(),
                RecordType.EMR_COLORCORRECTPALETTE => new EMR_COLORCORRECTPALETTE(),
                RecordType.EMR_SETICMPROFILEA => new EMR_SETICMPROFILEA(),
                RecordType.EMR_SETICMPROFILEW => new EMR_SETICMPROFILEW(),
                RecordType.EMR_ALPHABLEND => new EMR_ALPHABLEND(),
                RecordType.EMR_TRANSPARENTBLT => new EMR_TRANSPARENTBLT(),
                RecordType.EMR_GRADIENTFILL => new EMR_GRADIENTFILL(),
                RecordType.EMR_SETLINKEDUFIS => new EMR_SETLINKEDUFIS(),
                RecordType.EMR_COLORMATCHTOTARGETW => new EMR_COLORMATCHTOTARGETW(),
                RecordType.EMR_CREATECOLORSPACEW => new EMR_CREATECOLORSPACEW(),
                RecordType.EMR_SETLAYOUT => new EMR_SETLAYOUT(),
                RecordType.EMR_SETTEXTJUSTIFICATION => new EMR_SETTEXTJUSTIFICATION(),
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
