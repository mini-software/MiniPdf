using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniPdf.Drawing.Metafile.Wmf.Objects
{
    /// <summary>
    ///     The Palette Object specifies the colors in a logical palette.
    /// </summary>
    /// <remarks>
    ///     2.2.1.3 Palette Object
    /// </remarks>
    internal class Palette
    {
        /// <summary>
        ///     A 16-bit unsigned integer that defines the offset into the Palette Object when used with the META_SETPALENTRIES and META_ANIMATEPALETTE record types. When used with META_CREATEPALETTE, it MUST be 0x0300.
        /// </summary>
        public ushort Start;

        /// <summary>
        ///     A 16-bit unsigned integer that defines the number of objects in aPaletteEntries.
        /// </summary>
        public ushort NumberOfEntries;

        public List<PaletteEntry> PaletteEntries { get; set; } = new List<PaletteEntry>();

        public void Write(System.IO.BinaryWriter writer)
        {
            writer.Write(this.Start);
            writer.Write((ushort)PaletteEntries.Count);
            foreach (var entry in PaletteEntries)
            {
                entry.Write(writer);
            }
        }

        public void Read(System.IO.BinaryReader reader)
        {
            this.Start = reader.ReadUInt16();
            this.NumberOfEntries = reader.ReadUInt16();
            for (int i = 0; i < NumberOfEntries; i++)
            {
                var entry = new PaletteEntry();
                entry.Read(reader);
                PaletteEntries.Add(entry);
            }
        }
    }
}
