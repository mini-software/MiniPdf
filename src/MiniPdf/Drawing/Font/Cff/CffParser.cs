using System;
using System.Collections.Generic;
using System.Text;
using SysEncoding = System.Text.Encoding;

namespace MiniSoftware.Drawing.Font.Cff
{
    /// <summary>
    /// Parses the binary CFF (Compact Font Format) table and produces a <see cref="CffData"/>
    /// instance. Supports CFF version 1 (including CID-keyed fonts).
    /// </summary>
    internal static class CffParser
    {
        // ── CFF Standard Strings (Appendix A, 390 entries) ───────────────────

        private static readonly string[] s_stdStrings =
        {
            ".notdef","space","exclam","quotedbl","numbersign","dollar","percent",
            "ampersand","quoteright","parenleft","parenright","asterisk","plus","comma",
            "hyphen","period","slash","zero","one","two","three","four","five","six",
            "seven","eight","nine","colon","semicolon","less","equal","greater","question",
            "at","A","B","C","D","E","F","G","H","I","J","K","L","M","N","O","P","Q","R",
            "S","T","U","V","W","X","Y","Z","bracketleft","backslash","bracketright",
            "asciicircum","underscore","quoteleft","a","b","c","d","e","f","g","h","i",
            "j","k","l","m","n","o","p","q","r","s","t","u","v","w","x","y","z",
            "braceleft","bar","braceright","asciitilde","exclamdown","cent","sterling",
            "fraction","yen","florin","section","currency","quotesingle","quotedblleft",
            "guillemotleft","guilsinglleft","guilsinglright","fi","fl","endash","dagger",
            "daggerdbl","periodcentered","paragraph","bullet","quotesinglbase",
            "quotedblbase","quotedblright","guillemotright","ellipsis","perthousand",
            "questiondown","grave","acute","circumflex","tilde","macron","breve",
            "dotaccent","dieresis","ring","cedilla","hungarumlaut","ogonek","caron",
            "emdash","AE","ordfeminine","Lslash","Oslash","OE","ordmasculine","ae",
            "dotlessi","lslash","oslash","oe","germandbls","onesuperior","logicalnot",
            "mu","trademark","Eth","onehalf","plusminus","Thorn","onequarter","divide",
            "brokenbar","degree","thorn","threequarters","twosuperior","registered",
            "minus","eth","multiply","threesuperior","copyright","Aacute","Acircumflex",
            "Adieresis","Agrave","Aring","Atilde","Ccedilla","Eacute","Ecircumflex",
            "Edieresis","Egrave","Iacute","Icircumflex","Idieresis","Igrave","Ntilde",
            "Oacute","Ocircumflex","Odieresis","Ograve","Otilde","Scaron","Uacute",
            "Ucircumflex","Udieresis","Ugrave","Yacute","Ydieresis","Zcaron","aacute",
            "acircumflex","adieresis","agrave","aring","atilde","ccedilla","eacute",
            "ecircumflex","edieresis","egrave","iacute","icircumflex","idieresis",
            "igrave","ntilde","oacute","ocircumflex","odieresis","ograve","otilde",
            "scaron","uacute","ucircumflex","udieresis","ugrave","yacute","ydieresis",
            "zcaron","exclamsmall","Hungarumlautsmall","dollaroldstyle","dollarsuperior",
            "ampersandsmall","Acutesmall","parenleftsuperior","parenrightsuperior",
            "twodotenleader","onedotenleader","zerooldstyle","oneoldstyle","twooldstyle",
            "threeoldstyle","fouroldstyle","fiveoldstyle","sixoldstyle","sevenoldstyle",
            "eightoldstyle","nineoldstyle","commasuperior","threequartersemdash",
            "periodsuperior","questionsmall","asuperior","bsuperior","centsuperior",
            "dsuperior","esuperior","isuperior","lsuperior","msuperior","nsuperior",
            "osuperior","rsuperior","ssuperior","tsuperior","ff","ffi","ffl",
            "parenleftinferior","parenrightinferior","Circumflexsmall","hyphensuperior",
            "Gravesmall","Asmall","Bsmall","Csmall","Dsmall","Esmall","Fsmall","Gsmall",
            "Hsmall","Ismall","Jsmall","Ksmall","Lsmall","Msmall","Nsmall","Osmall",
            "Psmall","Qsmall","Rsmall","Ssmall","Tsmall","Usmall","Vsmall","Wsmall",
            "Xsmall","Ysmall","Zsmall","colonmonetary","onefitted","rupiah","Tildesmall",
            "exclamdownsmall","centoldstyle","Lslashsmall","Scaronsmall","Zcaronsmall",
            "Dieresissmall","Brevesmall","Caronsmall","Dotaccentsmall","Macronsmall",
            "figuredash","hypheninferior","Ogoneksmall","Ringsmall","Cedillasmall",
            "questiondownsmall","oneeighth","threeeighths","fiveeighths","seveneighths",
            "onethird","twothirds","zerosuperior","foursuperior","fivesuperior",
            "sixsuperior","sevensuperior","eightsuperior","ninesuperior","zeroinferior",
            "oneinferior","twoinferior","threeinferior","fourinferior","fiveinferior",
            "sixinferior","seveninferior","eightinferior","nineinferior","centinferior",
            "dollarinferior","periodinferior","commainferior","Agravesmall","Aacutesmall",
            "Acircumflexsmall","Atildesmall","Adieresissmall","Aringsmall","AEsmall",
            "Ccedillasmall","Egravesmall","Eacutesmall","Ecircumflexsmall","Edieresissmall",
            "Igravesmall","Iacutesmall","Icircumflexsmall","Idieresissmall","Ethsmall",
            "Ntildesmall","Ogravesmall","Oacutesmall","Ocircumflexsmall","Otildesmall",
            "Odieresissmall","OEsmall","Oslashsmall","Ugravesmall","Uacutesmall",
            "Ucircumflexsmall","Udieresissmall","Yacutesmall","Thornsmall",
            "Ydieresissmall","001.000","001.001","001.002","001.003","Black","Bold",
            "Book","Light","Medium","Regular","Roman","Semibold"
        }; // 390 entries

        // ── Top DICT operator codes ───────────────────────────────────────────
        private const int Op_version      = 0;
        private const int Op_Notice       = 1;
        private const int Op_FullName     = 2;
        private const int Op_FamilyName   = 3;
        private const int Op_Weight       = 4;
        private const int Op_FontBBox     = 5;
        private const int Op_CharStrings  = 17;
        private const int Op_Private      = 18;   // two operands: size, offset
        private const int Op_Charset      = 15;
        private const int Op_Encoding     = 16;
        private const int Op_ROS          = 0x0C1E; // 12 30 — CID ROS
        private const int Op_FDArray      = 0x0C24; // 12 36
        private const int Op_FDSelect     = 0x0C25; // 12 37
        private const int Op_CIDFontName  = 0x0C26; // 12 38

        // ── Private DICT operator codes ───────────────────────────────────────
        private const int PDOp_Subrs         = 19;
        private const int PDOp_defaultWidthX = 20;
        private const int PDOp_nominalWidthX = 21;

        // ── Public entry point ────────────────────────────────────────────────

        /// <summary>
        /// Parses a complete CFF binary blob and returns a <see cref="CffData"/> instance.
        /// </summary>
        public static CffData Parse(byte[] data)
        {
            if (data is null || data.Length < 4)
                throw new ArgumentException("CFF data is too short.", nameof(data));

            byte major    = data[0];
            byte minor    = data[1];
            byte hdrSize  = data[2];
            // offSize (data[3]) is the default for INDEX offset sizes; not used directly here.

            if (major != 1)
                throw new NotSupportedException(
                    $"CFF major version {major} is not supported (only version 1).");

            int pos = hdrSize; // Name INDEX starts right after the header.

            // 1. Name INDEX → font name(s)
            byte[][] nameIndex = ReadIndex(data, ref pos);
            string fontName = (nameIndex.Length > 0)
                ? SysEncoding.ASCII.GetString(nameIndex[0]).TrimEnd('\0')
                : string.Empty;

            // 2. Top DICT INDEX
            byte[][] topDictIndex = ReadIndex(data, ref pos);
            if (topDictIndex.Length == 0)
                throw new InvalidOperationException("CFF Top DICT INDEX is empty.");
            var topDictEntries = ParseDict(topDictIndex[0], 0, topDictIndex[0].Length);

            // 3. String INDEX
            byte[][] stringIndex = ReadIndex(data, ref pos);
            string[] customStrings = new string[stringIndex.Length];
            for (int i = 0; i < stringIndex.Length; i++)
                customStrings[i] = SysEncoding.ASCII.GetString(stringIndex[i]).TrimEnd('\0');

            // 4. Global Subr INDEX
            byte[][] globalSubrs = ReadIndex(data, ref pos);

            // ── Resolve Top DICT entries ──────────────────────────────────────
            var result = new CffData { FontName = fontName };
            result.GlobalSubrs = globalSubrs.Length > 0 ? globalSubrs : null;

            int charStringsOffset = 0;
            int charsetOffset     = 0; // 0 = ISOAdobe predefined
            int privateSize       = 0;
            int privateOffset     = 0;
            bool isCID            = false;
            int fdArrayOffset     = 0;
            int fdSelectOffset    = 0;

            foreach (var (op, operands) in topDictEntries)
            {
                switch (op)
                {
                    case Op_FullName:
                        if (operands.Length > 0)
                            result.FullName = GetString((int)operands[0], customStrings);
                        break;
                    case Op_FamilyName:
                        if (operands.Length > 0)
                            result.FamilyName = GetString((int)operands[0], customStrings);
                        break;
                    case Op_FontBBox:
                        for (int i = 0; i < 4 && i < operands.Length; i++)
                            result.FontBBox[i] = operands[i];
                        break;
                    case Op_CharStrings:
                        if (operands.Length > 0) charStringsOffset = (int)operands[0];
                        break;
                    case Op_Charset:
                        if (operands.Length > 0) charsetOffset = (int)operands[0];
                        break;
                    case Op_Private:
                        if (operands.Length >= 2)
                        {
                            privateSize   = (int)operands[0];
                            privateOffset = (int)operands[1];
                        }
                        break;
                    case Op_ROS:
                        isCID = true;
                        break;
                    case Op_FDArray:
                        if (operands.Length > 0) fdArrayOffset = (int)operands[0];
                        break;
                    case Op_FDSelect:
                        if (operands.Length > 0) fdSelectOffset = (int)operands[0];
                        break;
                }
            }

            // 5. Parse CharStrings INDEX
            if (charStringsOffset > 0)
            {
                int csp = charStringsOffset;
                byte[][] charStrings = ReadIndex(data, ref csp);
                result.CharStrings = charStrings;
            }

            int numGlyphs = result.CharStrings.Length;

            // 6. Parse charset → glyph names
            result.GlyphNames = ReadCharset(data, charsetOffset, numGlyphs, customStrings);

            // 7. Parse Private DICT (and local subrs) for simple fonts
            if (privateOffset > 0 && privateSize > 0 && !isCID)
            {
                result.PrivateDict = ParsePrivateDict(data, privateOffset, privateSize);
            }

            // 8. CID font support
            if (isCID && fdArrayOffset > 0)
            {
                result.IsCIDFont = true;
                result.CIDFontDicts = ParseFDArray(data, fdArrayOffset);

                if (fdSelectOffset > 0 && numGlyphs > 0)
                    result.GlyphFDIndex = ParseFDSelect(data, fdSelectOffset, numGlyphs);
            }

            return result;
        }

        // ── INDEX reader ──────────────────────────────────────────────────────

        private static byte[][] ReadIndex(byte[] data, ref int pos)
        {
            if (pos + 2 > data.Length) return Array.Empty<byte[]>();

            int count = (data[pos] << 8) | data[pos + 1];
            pos += 2;

            if (count == 0) return Array.Empty<byte[]>();

            int offSize = data[pos++];
            if (offSize < 1 || offSize > 4)
                throw new InvalidOperationException($"CFF INDEX offSize {offSize} is out of range.");

            int[] offsets = new int[count + 1];
            for (int i = 0; i <= count; i++)
            {
                int v = 0;
                for (int b = 0; b < offSize; b++)
                    v = (v << 8) | data[pos++];
                offsets[i] = v;
            }

            int dataStart = pos; // offsets are 1-based from here
            byte[][] result = new byte[count][];
            for (int i = 0; i < count; i++)
            {
                int start = dataStart + offsets[i] - 1;
                int len   = offsets[i + 1] - offsets[i];
                if (len < 0) len = 0;
                byte[] item = new byte[len];
                if (len > 0) Buffer.BlockCopy(data, start, item, 0, len);
                result[i] = item;
            }

            pos = dataStart + offsets[count] - 1;
            return result;
        }

        // ── DICT parser ───────────────────────────────────────────────────────

        private static List<(int op, double[] operands)> ParseDict(byte[] data, int start, int length)
        {
            var result   = new List<(int, double[])>();
            var operands = new List<double>();
            int pos = start;
            int end = start + length;

            while (pos < end)
            {
                byte b0 = data[pos];

                if (b0 == 12)
                {
                    // Two-byte operator
                    pos++;
                    if (pos >= end) break;
                    byte b1 = data[pos++];
                    result.Add((0x0C00 | b1, operands.ToArray()));
                    operands.Clear();
                }
                else if (b0 <= 21)
                {
                    // Single-byte operator
                    result.Add((b0, operands.ToArray()));
                    operands.Clear();
                    pos++;
                }
                else if (b0 == 28)
                {
                    // ShortInt: 3 bytes
                    pos++;
                    if (pos + 1 >= end) break;
                    int v = (sbyte)data[pos] << 8 | data[pos + 1];
                    pos += 2;
                    operands.Add(v);
                }
                else if (b0 == 29)
                {
                    // LongInt: 5 bytes
                    pos++;
                    if (pos + 3 >= end) break;
                    int v = (int)((uint)(data[pos] << 24) | (uint)(data[pos + 1] << 16) |
                                  (uint)(data[pos + 2] << 8) | data[pos + 3]);
                    pos += 4;
                    operands.Add(v);
                }
                else if (b0 == 30)
                {
                    // Real number (nibble-encoded)
                    pos++;
                    operands.Add(ReadRealNumber(data, ref pos, end));
                }
                else if (b0 >= 32 && b0 <= 246)
                {
                    operands.Add(b0 - 139);
                    pos++;
                }
                else if (b0 >= 247 && b0 <= 250)
                {
                    pos++;
                    if (pos >= end) break;
                    byte b1 = data[pos++];
                    operands.Add((b0 - 247) * 256.0 + b1 + 108);
                }
                else if (b0 >= 251 && b0 <= 254)
                {
                    pos++;
                    if (pos >= end) break;
                    byte b1 = data[pos++];
                    operands.Add(-((b0 - 251) * 256.0 + b1 + 108));
                }
                else
                {
                    pos++; // skip reserved bytes
                }
            }

            return result;
        }

        private static double ReadRealNumber(byte[] data, ref int pos, int end)
        {
            var sb = new System.Text.StringBuilder();
            while (pos < end)
            {
                byte b = data[pos++];
                int hi = (b >> 4) & 0x0F;
                int lo = b & 0x0F;
                if (!AppendNibble(sb, hi)) break;
                if (!AppendNibble(sb, lo)) break;
            }
            return double.TryParse(sb.ToString(),
                System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture,
                out double v) ? v : 0.0;
        }

        private static bool AppendNibble(System.Text.StringBuilder sb, int nibble)
        {
            switch (nibble)
            {
                case 0x0: case 0x1: case 0x2: case 0x3: case 0x4:
                case 0x5: case 0x6: case 0x7: case 0x8: case 0x9:
                    sb.Append((char)('0' + nibble)); return true;
                case 0xA: sb.Append('.'); return true;
                case 0xB: sb.Append('E'); return true;
                case 0xC: sb.Append("E-"); return true;
                case 0xD: return true; // reserved — skip
                case 0xE: sb.Append('-'); return true;
                case 0xF: return false; // end of number
                default:  return false;
            }
        }

        // ── Private DICT ──────────────────────────────────────────────────────

        private static CffPrivateDict ParsePrivateDict(byte[] data, int offset, int size)
        {
            var pd = new CffPrivateDict();
            var entries = ParseDict(data, offset, size);
            int subrsRelOffset = 0;
            foreach (var (op, operands) in entries)
            {
                switch (op)
                {
                    case PDOp_defaultWidthX:
                        if (operands.Length > 0) pd.DefaultWidthX = operands[0];
                        break;
                    case PDOp_nominalWidthX:
                        if (operands.Length > 0) pd.NominalWidthX = operands[0];
                        break;
                    case PDOp_Subrs:
                        if (operands.Length > 0) subrsRelOffset = (int)operands[0];
                        break;
                }
            }
            if (subrsRelOffset != 0)
            {
                int subrsAbs = offset + subrsRelOffset;
                if (subrsAbs < data.Length)
                {
                    int p = subrsAbs;
                    byte[][] localSubrs = ReadIndex(data, ref p);
                    if (localSubrs.Length > 0)
                        pd.LocalSubrs = localSubrs;
                }
            }
            return pd;
        }

        // ── Charset ───────────────────────────────────────────────────────────

        private static string[] ReadCharset(byte[] data, int offset, int numGlyphs, string[] customStrings)
        {
            if (numGlyphs == 0) return Array.Empty<string>();

            var names = new string[numGlyphs];
            names[0] = ".notdef";

            if (offset <= 2)
            {
                // Predefined charsets 0/1/2 — glyph names not critical for rendering
                for (int i = 1; i < numGlyphs; i++)
                    names[i] = string.Empty;
                return names;
            }

            if (offset >= data.Length) return names;

            int format = data[offset];
            int pos = offset + 1;

            switch (format)
            {
                case 0: // SID per glyph
                    for (int gid = 1; gid < numGlyphs && pos + 1 < data.Length; gid++)
                    {
                        int sid = (data[pos] << 8) | data[pos + 1];
                        pos += 2;
                        names[gid] = GetString(sid, customStrings);
                    }
                    break;

                case 1: // Range1: {SID(2), nLeft(1)}+
                {
                    int gid = 1;
                    while (gid < numGlyphs && pos + 2 < data.Length)
                    {
                        int sid   = (data[pos] << 8) | data[pos + 1];
                        int nLeft = data[pos + 2];
                        pos += 3;
                        for (int k = 0; k <= nLeft && gid < numGlyphs; k++, gid++)
                            names[gid] = GetString(sid + k, customStrings);
                    }
                    break;
                }

                case 2: // Range2: {SID(2), nLeft(2)}+
                {
                    int gid = 1;
                    while (gid < numGlyphs && pos + 3 < data.Length)
                    {
                        int sid   = (data[pos] << 8) | data[pos + 1];
                        int nLeft = (data[pos + 2] << 8) | data[pos + 3];
                        pos += 4;
                        for (int k = 0; k <= nLeft && gid < numGlyphs; k++, gid++)
                            names[gid] = GetString(sid + k, customStrings);
                    }
                    break;
                }
            }

            return names;
        }

        // ── CID FDArray / FDSelect ────────────────────────────────────────────

        private static CffCIDFontDict[] ParseFDArray(byte[] data, int offset)
        {
            int p = offset;
            byte[][] fdBytes = ReadIndex(data, ref p);
            var result = new CffCIDFontDict[fdBytes.Length];
            for (int i = 0; i < fdBytes.Length; i++)
            {
                var fd = new CffCIDFontDict();
                var fdEntries = ParseDict(fdBytes[i], 0, fdBytes[i].Length);
                int privSize = 0, privOffset = 0;
                foreach (var (op, operands) in fdEntries)
                {
                    if (op == Op_Private && operands.Length >= 2)
                    {
                        privSize   = (int)operands[0];
                        privOffset = (int)operands[1];
                    }
                }
                if (privOffset > 0 && privSize > 0)
                    fd.PrivateDict = ParsePrivateDict(data, privOffset, privSize);
                result[i] = fd;
            }
            return result;
        }

        private static int[] ParseFDSelect(byte[] data, int offset, int numGlyphs)
        {
            var result = new int[numGlyphs];
            if (offset >= data.Length) return result;

            int format = data[offset];
            int pos = offset + 1;

            if (format == 0)
            {
                for (int i = 0; i < numGlyphs && pos < data.Length; i++)
                    result[i] = data[pos++];
            }
            else if (format == 3)
            {
                int nRanges = (data[pos] << 8) | data[pos + 1];
                pos += 2;
                for (int r = 0; r < nRanges && pos + 2 < data.Length; r++)
                {
                    int first  = (data[pos] << 8) | data[pos + 1];
                    int fdIdx  = data[pos + 2];
                    pos += 3;
                    int end    = (r + 1 < nRanges && pos + 1 < data.Length)
                                 ? (data[pos] << 8) | data[pos + 1]
                                 : numGlyphs;
                    for (int g = first; g < end && g < numGlyphs; g++)
                        result[g] = fdIdx;
                }
            }

            return result;
        }

        // ── String lookup ─────────────────────────────────────────────────────

        private static string GetString(int sid, string[] customStrings)
        {
            if (sid < 0) return string.Empty;
            if (sid < s_stdStrings.Length) return s_stdStrings[sid];
            int ci = sid - s_stdStrings.Length;
            return ci < customStrings.Length ? customStrings[ci] : string.Empty;
        }
    }
}
