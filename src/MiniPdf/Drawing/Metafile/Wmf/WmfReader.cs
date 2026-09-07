using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniPdf.Drawing.Metafile.Wmf
{
    using System.IO;
    using MiniPdf.Drawing.Metafile.Wmf.Enumerations;
    using MiniPdf.Drawing.Metafile.Wmf.Records;
    using MiniPdf.Drawing.Metafile.Wmf.Records.EscapeRecordTypes;
    using MiniPdf.Drawing.Metafile.Wmf.Records.ObjectRecordTypes;
    using MiniPdf.Drawing.Metafile.Wmf.Records.StateRecordTypes;

    internal class WmfReader
    {
        public List<Record> Records { get; } = new();

        public MetaHeader Header { get; private set; } = new();

        public void Read(Stream stream)
        {
            var reader = new BinaryReader(stream);

            this.Header = new MetaHeader();
            this.Header.Read(reader);

            while (stream.Position < stream.Length)
            {
                var header = new RecordHeader();
                header.Read(reader);

                long expectedEndPosition = stream.Position + (header.RecordSize * 2) - 6;
                var record = CreateRecord(header, reader);
                if (record == null)
                {
                    record = new Record
                    {
                        Header = header
                    };
                }
                Records.Add(record);
                stream.Position = expectedEndPosition;
            }
        }

        public static Record? CreateRecord(RecordHeader header, BinaryReader reader)
        {
            Record? record = header.RecordFunction switch
            {
                RecordType.META_EOF => new META_EOF(),
                RecordType.META_REALIZEPALETTE => new META_REALIZEPALETTE(),
                RecordType.META_SETPALENTRIES => new META_SETPALENTRIES(),
                RecordType.META_SETBKMODE => new META_SETBKMODE(),
                RecordType.META_SETMAPMODE => new META_SETMAPMODE(),
                RecordType.META_SETROP2 => new META_SETROP2(),
                RecordType.META_SETRELABS => new META_SETRELABS(),
                RecordType.META_SETPOLYFILLMODE => new META_SETPOLYFILLMODE(),
                RecordType.META_SETSTRETCHBLTMODE => new META_SETSTRETCHBLTMODE(),
                RecordType.META_SETTEXTCHAREXTRA => new META_SETTEXTCHAREXTRA(),
                RecordType.META_RESTOREDC => new META_RESTOREDC(),
                RecordType.META_RESIZEPALETTE => new META_RESIZEPALETTE(),
                RecordType.META_DIBCREATEPATTERNBRUSH => new META_DIBCREATEPATTERNBRUSH(),
                RecordType.META_SETLAYOUT => new META_SETLAYOUT(),
                RecordType.META_SETBKCOLOR => new META_SETBKCOLOR(),
                RecordType.META_SETTEXTCOLOR => new META_SETTEXTCOLOR(),
                RecordType.META_OFFSETVIEWPORTORG => new META_OFFSETVIEWPORTORG(),
                RecordType.META_LINETO => new META_LINETO(),
                RecordType.META_MOVETO => new META_MOVETO(),
                RecordType.META_OFFSETCLIPRGN => new META_OFFSETCLIPRGN(),
                RecordType.META_FILLREGION => new META_FILLREGION(),
                RecordType.META_SETMAPPERFLAGS => new META_SETMAPPERFLAGS(),
                RecordType.META_SELECTPALETTE => new META_SELECTPALETTE(),
                RecordType.META_POLYGON => new META_POLYGON(),
                RecordType.META_POLYLINE => new META_POLYLINE(),
                RecordType.META_SETTEXTJUSTIFICATION => new META_SETTEXTJUSTIFICATION(),
                RecordType.META_SETWINDOWORG => new META_SETWINDOWORG(),
                RecordType.META_SETWINDOWEXT => new META_SETWINDOWEXT(),
                RecordType.META_SETVIEWPORTORG => new META_SETVIEWPORTORG(),
                RecordType.META_SETVIEWPORTEXT => new META_SETVIEWPORTEXT(),
                RecordType.META_OFFSETWINDOWORG => new META_OFFSETWINDOWORG(),
                RecordType.META_SCALEWINDOWEXT => new META_SCALEWINDOWEXT(),
                RecordType.META_SCALEVIEWPORTEXT => new META_SCALEVIEWPORTEXT(),
                RecordType.META_EXCLUDECLIPRECT => new META_EXCLUDECLIPRECT(),
                RecordType.META_INTERSECTCLIPRECT => new META_INTERSECTCLIPRECT(),
                RecordType.META_ELLIPSE => new META_ELLIPSE(),
                RecordType.META_FLOODFILL => new META_FLOODFILL(),
                RecordType.META_FRAMEREGION => new META_FRAMEREGION(),
                RecordType.META_ANIMATEPALETTE => new META_ANIMATEPALETTE(),
                RecordType.META_TEXTOUT => new META_TEXTOUT(),
                RecordType.META_POLYPOLYGON => new META_POLYPOLYGON(),
                RecordType.META_EXTFLOODFILL => new META_EXTFLOODFILL(),
                RecordType.META_ESCAPE => new MetaEscape(),
                RecordType.META_RECTANGLE => new META_RECTANGLE(),
                RecordType.META_ROUNDRECT => new META_ROUNDRECT(),
                RecordType.META_SETPIXEL => new META_SETPIXEL(),
                RecordType.META_PATBLT => new META_PATBLT(),
                RecordType.META_SAVEDC => new META_SAVEDC(),
                RecordType.META_PIE => new META_PIE(),
                RecordType.META_STRETCHBLT => new META_STRETCHBLT(),
                RecordType.META_INVERTREGION => new META_INVERTREGION(),
                RecordType.META_PAINTREGION => new META_PAINTREGION(),
                RecordType.META_SELECTCLIPREGION => new META_SELECTCLIPREGION(),
                RecordType.META_SELECTOBJECT => new META_SELECTOBJECT(),
                RecordType.META_SETTEXTALIGN => new META_SETTEXTALIGN(),
                RecordType.META_ARC => new META_ARC(),
                RecordType.META_CHORD => new META_CHORD(),
                RecordType.META_BITBLT => new META_BITBLT(),
                RecordType.META_EXTTEXTOUT => new META_EXTTEXTOUT(),
                RecordType.META_SETDIBTODEV => new META_SETDIBTODEV(),
                RecordType.META_STRETCHDIB => new META_STRETCHDIB(),
                RecordType.META_DELETEOBJECT => new META_DELETEOBJECT(),
                RecordType.META_CREATEPALETTE => new META_CREATEPALETTE(),
                RecordType.META_CREATEPATTERNBRUSH => new META_CREATEPATTERNBRUSH(),
                RecordType.META_CREATEPENINDIRECT => new META_CREATEPENINDIRECT(),
                RecordType.META_CREATEFONTINDIRECT => new META_CREATEFONTINDIRECT(),
                RecordType.META_CREATEBRUSHINDIRECT => new MetaCreatebrushindirect(),
                RecordType.META_CREATEREGION => new META_CREATEREGION(),
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
