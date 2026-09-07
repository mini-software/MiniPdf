using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniPdf.Drawing.Metafile.Wmf.Records
{
    internal class Record
    {
                public RecordHeader Header;

        public virtual void Read(BinaryReader reader)
        {
        }

        public virtual void Write(BinaryWriter writer)
        {
        }
    }
}
