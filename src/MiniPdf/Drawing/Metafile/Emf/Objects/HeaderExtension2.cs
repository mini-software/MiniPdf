using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniPdf.Drawing.Metafile.Emf.Objects
{
    using System.IO;

    /// <summary>
    ///     The HeaderExtension2 object defines the second extension to the EMF metafile header. It adds the ability to measure device surfaces in micrometers, which enhances the resolution and scalability of EMF metafiles.
    /// </summary>
    /// <remarks>
    ///     2.2.11 HeaderExtension2 Object
    /// </remarks>
    internal class HeaderExtension2
    {
        /// <summary>
        ///     The 32-bit horizontal size of the display device for which the metafile image was generated, in micrometers.
        /// </summary>
        public uint MicrometersX;

        /// <summary>
        ///     The 32-bit vertical size of the display device for which the metafile image was generated, in micrometers.
        /// </summary>
        public uint MicrometersY;

        public void Read(BinaryReader reader)
        {
            this.MicrometersX = reader.ReadUInt32();
            this.MicrometersY = reader.ReadUInt32();
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(this.MicrometersX);
            writer.Write(this.MicrometersY);
        }
    }
}
