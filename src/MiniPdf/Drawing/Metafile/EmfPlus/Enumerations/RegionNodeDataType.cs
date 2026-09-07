using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniPdf.Drawing.Metafile.EmfPlus.Enumerations
{
    internal enum RegionNodeDataType
    {
        RegionNodeDataTypeAnd = 0x00000001,
        RegionNodeDataTypeOr = 0x00000002,
        RegionNodeDataTypeXor = 0x00000003,
        RegionNodeDataTypeExclude = 0x00000004,
        RegionNodeDataTypeComplement = 0x00000005,
        RegionNodeDataTypeRect = 0x10000000,
        RegionNodeDataTypePath = 0x10000001,
        RegionNodeDataTypeEmpty = 0x10000002,
        RegionNodeDataTypeInfinite = 0x10000003
    }
}
