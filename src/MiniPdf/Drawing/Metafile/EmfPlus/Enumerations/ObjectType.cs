using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniPdf.Drawing.Metafile.EmfPlus.Enumerations
{
    internal enum ObjectType : ushort
    {
        ObjectTypeInvalid = 0x00000000,
        ObjectTypeBrush = 0x00000001,
        ObjectTypePen = 0x00000002,
        ObjectTypePath = 0x00000003,
        ObjectTypeRegion = 0x00000004,
        ObjectTypeImage = 0x00000005,
        ObjectTypeFont = 0x00000006,
        ObjectTypeStringFormat = 0x00000007,
        ObjectTypeImageAttributes = 0x00000008,
        ObjectTypeCustomLineCap = 0x00000009
    }
}
