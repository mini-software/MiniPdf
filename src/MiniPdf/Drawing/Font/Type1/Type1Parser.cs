using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using SysEncoding = System.Text.Encoding;

namespace MiniPdf.Drawing.Font.Type1
{
    /// <summary>
    /// Reads PFB (binary) or PFA (ASCII-hex) Type 1 font files and returns
    /// a <see cref="Type1Data"/> containing all metadata, charstrings, and subrs.
    /// </summary>
    public static class Type1Parser
    {
        // ── Cipher constants ──────────────────────────────────────────────────

        private const ushort EexecKey      = 55665;
        private const ushort CharstringKey = 4330;
        private const uint   CipherC1      = 52845;
        private const uint   CipherC2      = 22719;

        // ── Public entry point ────────────────────────────────────────────────

        /// <summary>
        /// Parses a PFB or PFA byte array into a <see cref="Type1Data"/> instance.
        /// </summary>
        internal static Type1Data Parse(byte[] data)
        {
            if (data is null) throw new ArgumentNullException(nameof(data));

            return (data.Length >= 2 && data[0] == 0x80)
                ? ParsePfb(data)
                : ParsePfa(EncodingLatin1.GetString(data));
        }

        // ── Latin-1 encoding helper ───────────────────────────────────────────

        private static readonly SysEncoding EncodingLatin1 = SysEncoding.GetEncoding(28591);

        // ── PFB reader ────────────────────────────────────────────────────────

        private static Type1Data ParsePfb(byte[] data)
        {
            var result     = new Type1Data();
            var asciiAccum = new System.Text.StringBuilder();
            var eexecAccum = new System.Collections.Generic.List<byte[]>();
            bool seenEexec = false;
            int  pos = 0;

            while (pos < data.Length)
            {
                // ── PFB segment? ──────────────────────────────────────────────
                // Handle EOF marker (only 2 bytes) before the 6-byte check.
                if (pos + 2 <= data.Length && data[pos] == 0x80 && data[pos + 1] == 0x03)
                    break; // PFB EOF marker

                if (pos + 6 <= data.Length && data[pos] == 0x80)
                {
                    byte type = data[pos + 1];

                    int segLen = data[pos + 2]
                               | (data[pos + 3] << 8)
                               | (data[pos + 4] << 16)
                               | (data[pos + 5] << 24);
                    pos += 6;
                    if (pos + segLen > data.Length) segLen = data.Length - pos;

                    if (type == 0x01)
                    {
                        // ASCII segment: accumulate, but check for "eexec" token.
                        string asciiSeg = EncodingLatin1.GetString(data, pos, segLen);
                        if (!seenEexec)
                        {
                            int eexecInSeg = asciiSeg.IndexOf("eexec", StringComparison.Ordinal);
                            if (eexecInSeg >= 0)
                            {
                                // Accumulate ASCII up to (not including) "eexec".
                                asciiAccum.Append(asciiSeg.Substring(0, eexecInSeg));
                                seenEexec = true;
                                // Any bytes after "eexec\n" within this ASCII segment
                                // (uncommon in PFB) would be in-line eexec data.
                                int afterEexec = eexecInSeg + 5;
                                if (afterEexec < asciiSeg.Length
                                    && (asciiSeg[afterEexec] == '\n' || asciiSeg[afterEexec] == '\r'
                                        || asciiSeg[afterEexec] == ' '))
                                    afterEexec++;
                                if (afterEexec < asciiSeg.Length)
                                {
                                    byte[] trailing = EncodingLatin1.GetBytes(
                                        asciiSeg.Substring(afterEexec));
                                    eexecAccum.Add(trailing);
                                }
                            }
                            else
                            {
                                asciiAccum.Append(asciiSeg);
                            }
                        }
                        // (If seenEexec and type=1: additional cleartext segments after
                        //  the eexec block — rare but possible; ignore for now.)
                    }
                    else if (type == 0x02 && seenEexec)
                    {
                        var chunk = new byte[segLen];
                        Array.Copy(data, pos, chunk, 0, segLen);
                        eexecAccum.Add(chunk);
                    }
                    pos += segLen;
                    continue;
                }

                // ── Raw (non-segmented) bytes ─────────────────────────────────
                if (!seenEexec)
                {
                    // Scan for "eexec" keyword; everything before is ASCII.
                    byte[] needle = EncodingLatin1.GetBytes("eexec");
                    int eexecByte = IndexOfBytes(data, needle, pos);

                    if (eexecByte >= 0)
                    {
                        asciiAccum.Append(EncodingLatin1.GetString(data, pos, eexecByte - pos));
                        seenEexec = true;

                        // Advance past "eexec" + optional single whitespace
                        pos = eexecByte + 5;
                        if (pos < data.Length
                            && (data[pos] == '\n' || data[pos] == '\r'
                                || data[pos] == ' '  || data[pos] == '\t'))
                            pos++;

                        // If PFB binary segment follows, let the loop handle it.
                        if (pos + 2 <= data.Length
                            && data[pos] == 0x80 && data[pos + 1] == 0x02)
                            continue;

                        // Raw binary eexec data (no further PFB wrapping)
                        if (pos < data.Length)
                        {
                            var chunk = new byte[data.Length - pos];
                            Array.Copy(data, pos, chunk, 0, chunk.Length);
                            eexecAccum.Add(chunk);
                        }
                        break;
                    }
                    else
                    {
                        // No "eexec" in remaining bytes — all ASCII
                        asciiAccum.Append(EncodingLatin1.GetString(data, pos, data.Length - pos));
                        break;
                    }
                }
                else
                {
                    // We've already seen eexec but hit raw non-segmented bytes.
                    // Treat them as raw binary eexec continuation.
                    var chunk = new byte[data.Length - pos];
                    Array.Copy(data, pos, chunk, 0, chunk.Length);
                    eexecAccum.Add(chunk);
                    break;
                }
            }

            if (asciiAccum.Length > 0) ParseAsciiSection(asciiAccum.ToString(), result);

            if (eexecAccum.Count > 0)
            {
                // Concatenate all binary chunks into one eexec byte array
                int total = 0;
                foreach (var c in eexecAccum) total += c.Length;
                var eexecBytes = new byte[total];
                int off = 0;
                foreach (var c in eexecAccum) { Array.Copy(c, 0, eexecBytes, off, c.Length); off += c.Length; }
                ApplyEexecSection(eexecBytes, result);
            }

            return result;
        }

        // Byte-level search helper
        private static int IndexOfBytes(byte[] haystack, byte[] needle, int startPos)
        {
            int limit = haystack.Length - needle.Length;
            for (int i = startPos; i <= limit; i++)
            {
                bool match = true;
                for (int j = 0; j < needle.Length; j++)
                    if (haystack[i + j] != needle[j]) { match = false; break; }
                if (match) return i;
            }
            return -1;
        }

        // ── PFA reader ────────────────────────────────────────────────────────

        private static Type1Data ParsePfa(string text)
        {
            var result = new Type1Data();

            int eexecIdx = text.IndexOf("eexec", StringComparison.Ordinal);
            if (eexecIdx < 0)
            {
                ParseAsciiSection(text, result);
                return result;
            }

            ParseAsciiSection(text.Substring(0, eexecIdx), result);

            // Skip "eexec" + one whitespace character
            int hexStart = eexecIdx + 5;
            if (hexStart < text.Length &&
                (text[hexStart] == '\n' || text[hexStart] == '\r' || text[hexStart] == ' '))
                hexStart++;

            byte[] eexecBytes = DecodeHexString(text.Substring(hexStart));
            ApplyEexecSection(eexecBytes, result);

            return result;
        }

        // ── eexec section wrapper ─────────────────────────────────────────────

        private static void ApplyEexecSection(byte[] eexecBytes, Type1Data data)
        {
            byte[] decrypted = EexecDecrypt(eexecBytes);
            ParseEexecSectionBytes(decrypted, data);
        }

        // ── ASCII section parser ──────────────────────────────────────────────

        private static void ParseAsciiSection(string text, Type1Data data)
        {
            var tokens = Tokenize(text);

            for (int i = 0; i < tokens.Count; i++)
            {
                string tok = tokens[i];

                // /FontName /Courier def
                if (tok == "/FontName" && i + 1 < tokens.Count)
                {
                    string next = tokens[i + 1];
                    if (next.StartsWith("/", StringComparison.Ordinal))
                        data.FontName = next.Substring(1);
                    i++;
                    continue;
                }

                // /FontBBox {x0 y0 x1 y1} def
                if (tok == "/FontBBox" && i + 1 < tokens.Count)
                {
                    data.FontBBox = ParseNumericBlock(tokens[i + 1]);
                    i++;
                    continue;
                }

                // /Encoding StandardEncoding|ISOLatin1Encoding|<custom>
                if (tok == "/Encoding" && i + 1 < tokens.Count)
                {
                    if (tokens[i + 1] == "StandardEncoding")
                    {
                        data.Encoding = GetStandardEncoding();
                        i++;
                    }
                    else if (tokens[i + 1] == "ISOLatin1Encoding")
                    {
                        data.Encoding = GetIsoLatin1Encoding();
                        i++;
                    }
                    // Custom encodings are handled below via "dup N /name put" pattern
                    continue;
                }
            }

            // Parse custom encoding: dup <slot> /<name> put
            for (int i = 0; i + 3 < tokens.Count; i++)
            {
                if (tokens[i] == "dup"
                    && int.TryParse(tokens[i + 1], NumberStyles.Integer,
                                    CultureInfo.InvariantCulture, out int slot)
                    && slot >= 0 && slot < 256
                    && tokens[i + 2].StartsWith("/", StringComparison.Ordinal)
                    && tokens[i + 3] == "put")
                {
                    data.Encoding[slot] = tokens[i + 2].Substring(1);
                    i += 3;
                }
            }
        }

        // ── eexec section parser (byte-level, handles both binary RD and hex PFA) ──

        internal static void ParseEexecSectionBytes(byte[] decrypted, Type1Data data)
        {
            int pos = 0;
            var order = new List<string>();

            while (pos < decrypted.Length)
            {
                string tok = ReadByteToken(decrypted, ref pos);
                if (tok.Length == 0) break;

                // /lenIV N def
                if (tok == "/lenIV")
                {
                    string num = ReadByteToken(decrypted, ref pos);
                    if (int.TryParse(num, NumberStyles.Integer,
                                     CultureInfo.InvariantCulture, out int lv))
                        data.LenIV = lv;
                    continue;
                }

                // /Subrs N array dup 0 M RD … NP … def
                if (tok == "/Subrs")
                {
                    string countTok = ReadByteToken(decrypted, ref pos);
                    if (!int.TryParse(countTok, NumberStyles.Integer,
                                      CultureInfo.InvariantCulture, out int count))
                        continue;
                    var subrs = new byte[count][];
                    ReadByteToken(decrypted, ref pos); // consume "array"

                    while (pos < decrypted.Length)
                    {
                        int savedPos = pos;
                        string t = ReadByteToken(decrypted, ref pos);
                        if (t.Length == 0 || t == "def" || t == "readonly"
                            || t == "end" || t == "noaccess") break;
                        // A name literal (starts with '/') means we've left the Subrs
                        // section — rewind so the main loop can pick it up.
                        if (t.StartsWith("/", StringComparison.Ordinal)) { pos = savedPos; break; }
                        if (t != "dup") continue;

                        string idxTok = ReadByteToken(decrypted, ref pos);
                        if (!int.TryParse(idxTok, NumberStyles.Integer,
                                          CultureInfo.InvariantCulture, out int idx)
                            || idx < 0 || idx >= count)
                            continue;

                        byte[] raw = ReadCharsBytes(decrypted, ref pos);
                        if (raw.Length > 0)
                            subrs[idx] = DecryptCharstring(raw, data.LenIV);
                    }
                    data.Subrs = subrs;
                    continue;
                }

                // /CharStrings N dict begin … end
                if (tok == "/CharStrings")
                {
                    // Skip preamble tokens (N, "dict", "begin", etc.) until first /name
                    while (pos < decrypted.Length)
                    {
                        string t = ReadByteToken(decrypted, ref pos);
                        if (t.Length == 0 || t == "end" || t == "readonly"
                            || t == "currentdict") goto doneCharStrings;
                        if (!t.StartsWith("/", StringComparison.Ordinal)) continue;

                        // Process first glyph
                        string nm = t.Substring(1);
                        byte[] raw = ReadCharsBytes(decrypted, ref pos);
                        if (!data.CharStrings.ContainsKey(nm))
                        { data.CharStrings[nm] = DecryptCharstring(raw, data.LenIV); order.Add(nm); }
                        break;
                    }

                    // Process remaining glyphs
                    while (pos < decrypted.Length)
                    {
                        string t = ReadByteToken(decrypted, ref pos);
                        if (t.Length == 0 || t == "end" || t == "readonly"
                            || t == "currentdict") break;
                        if (!t.StartsWith("/", StringComparison.Ordinal)) continue;

                        string name = t.Substring(1);
                        byte[] raw  = ReadCharsBytes(decrypted, ref pos);
                        if (!data.CharStrings.ContainsKey(name))
                        { data.CharStrings[name] = DecryptCharstring(raw, data.LenIV); order.Add(name); }
                    }
                    doneCharStrings:
                    data.GlyphOrder = order.ToArray();
                    break; // found CharStrings — done with main loop
                }
            }

            if (data.GlyphOrder.Length == 0 && data.CharStrings.Count > 0)
            {
                data.GlyphOrder = new string[data.CharStrings.Count];
                int j = 0;
                foreach (string k in data.CharStrings.Keys) data.GlyphOrder[j++] = k;
            }
        }

        // Read one charstring's raw (inner-encrypted) bytes.
        // Handles: hex  → <hexdata>
        //          binary → N RD <N bytes>  (RD variants: "RD", "-|")
        //          ND terminators: "ND", "--", "|-", "NP", "|"
        internal static byte[] ReadCharsBytes(byte[] data, ref int pos)
        {
            string tok1 = ReadByteToken(data, ref pos);

            // Hex format (PFA-style after eexec-decrypt): <hexdata>
            if (tok1.StartsWith("<", StringComparison.Ordinal)
                && tok1.EndsWith(">", StringComparison.Ordinal))
                return ParseHexToken(tok1);

            // Binary format: N RD <N bytes>
            // RD operators: "RD" or "-|" (dash-pipe)
            if (int.TryParse(tok1, NumberStyles.Integer,
                             CultureInfo.InvariantCulture, out int blen) && blen > 0)
            {
                string rdTok = ReadByteToken(data, ref pos); // "RD", "-|"
                if ((rdTok == "RD" || rdTok == "-|"))
                {
                    // After the RD operator there is exactly one mandatory separator byte
                    // (space or newline) before the binary charstring bytes.  Skip it.
                    if (pos < data.Length
                        && (data[pos] == ' ' || data[pos] == '\t'
                            || data[pos] == '\r' || data[pos] == '\n'))
                        pos++;

                    if (pos + blen > data.Length) return Array.Empty<byte>();

                    var raw = new byte[blen];
                    Array.Copy(data, pos, raw, 0, blen);
                    pos += blen;
                    // Skip optional whitespace before the ND terminator
                    while (pos < data.Length
                           && (data[pos] == ' ' || data[pos] == '\t'
                               || data[pos] == '\r' || data[pos] == '\n'))
                        pos++;
                    // Detect ND at raw-byte level so that pipe-based NDs
                    // immediately followed by the next token (e.g. "|dup", "||dup",
                    // "|-/name") are consumed without folding the next token.
                    if (pos < data.Length && data[pos] == '|')
                    {
                        pos++; // consume leading '|'
                        if (pos < data.Length
                            && (data[pos] == '|' || data[pos] == '-'))
                            pos++; // consume second char of "||" or "|-"
                    }
                    else if (pos + 1 < data.Length
                             && data[pos] == '-' && data[pos + 1] == '-')
                    {
                        pos += 2; // consume "--"
                    }
                    else
                    {
                        int ndPos = pos;
                        string nd = ReadByteToken(data, ref ndPos);
                        if (nd == "ND" || nd == "NP" || nd == "noaccess")
                            pos = ndPos;
                    }
                    return raw;
                }
            }

            return Array.Empty<byte>();
        }

        // Byte-level PostScript token reader (skips whitespace and comments).
        internal static string ReadByteToken(byte[] data, ref int pos)
        {
            // Skip whitespace and comments
            while (pos < data.Length)
            {
                byte b = data[pos];
                if (b == ' ' || b == '\t' || b == '\r' || b == '\n' || b == '\f' || b == 0)
                { pos++; continue; }
                if (b == '%')
                { while (pos < data.Length && data[pos] != '\n' && data[pos] != '\r') pos++; continue; }
                break;
            }
            if (pos >= data.Length) return string.Empty;

            byte c = data[pos];

            // Proc body { }
            if (c == '{')
            {
                int depth = 1, start = pos++;
                while (pos < data.Length && depth > 0)
                {
                    if (data[pos] == '{') depth++;
                    else if (data[pos] == '}') depth--;
                    pos++;
                }
                return EncodingLatin1.GetString(data, start, pos - start);
            }

            // Array [ ]
            if (c == '[')
            {
                int depth = 1, start = pos++;
                while (pos < data.Length && depth > 0)
                {
                    if (data[pos] == '[') depth++;
                    else if (data[pos] == ']') depth--;
                    pos++;
                }
                return EncodingLatin1.GetString(data, start, pos - start);
            }

            // Dict << >>
            if (c == '<' && pos + 1 < data.Length && data[pos + 1] == '<')
            { pos += 2; return "<<"; }
            if (c == '>' && pos + 1 < data.Length && data[pos + 1] == '>')
            { pos += 2; return ">>"; }

            // Hex string <…>
            if (c == '<')
            {
                int start = ++pos;
                while (pos < data.Length && data[pos] != '>') pos++;
                string hex = EncodingLatin1.GetString(data, start, pos - start);
                if (pos < data.Length) pos++;
                return "<" + hex + ">";
            }

            // PS string (…)
            if (c == '(')
            {
                int depth = 1, start = pos++;
                while (pos < data.Length && depth > 0)
                {
                    if (data[pos] == '\\') { pos += 2; continue; }
                    if (data[pos] == '(') depth++;
                    else if (data[pos] == ')') depth--;
                    pos++;
                }
                return EncodingLatin1.GetString(data, start, pos - start);
            }

            // Single-char delimiters not part of above
            if (c == ')' || c == ']' || c == '}' || c == '>')
            { pos++; return EncodingLatin1.GetString(data, pos - 1, 1); }

            // Name (starts with /) or keyword/number
            {
                int start = pos;
                if (c == '/') pos++; // include leading '/'
                while (pos < data.Length && !IsByteTokDelim(data[pos])) pos++;
                return pos > start
                    ? EncodingLatin1.GetString(data, start, pos - start)
                    : string.Empty;
            }
        }

        internal static bool IsByteTokDelim(byte b)
            => b == ' ' || b == '\t' || b == '\r' || b == '\n' || b == '\f' || b == 0
            || b == '(' || b == ')' || b == '<' || b == '>'
            || b == '[' || b == ']' || b == '{' || b == '}'
            || b == '/' || b == '%';

        // ── eexec decryption ──────────────────────────────────────────────────

        internal static byte[] EexecDecrypt(byte[] data)
        {
            if (data.Length <= 4) return Array.Empty<byte>();

            ushort r = EexecKey;

            // Prime the cipher with the first 4 bytes (discarded plaintext)
            for (int i = 0; i < 4; i++)
                r = Step(r, data[i]);

            var result = new byte[data.Length - 4];
            for (int i = 0; i < result.Length; i++)
            {
                byte c = data[i + 4];
                result[i] = (byte)(c ^ (r >> 8));
                r = Step(r, c);
            }
            return result;
        }

        // ── Charstring decryption ─────────────────────────────────────────────

        internal static byte[] DecryptCharstring(byte[] data, int lenIV)
        {
            if (lenIV <= 0) return data;
            if (data.Length <= lenIV) return Array.Empty<byte>();

            ushort r = CharstringKey;

            for (int i = 0; i < lenIV; i++)
                r = Step(r, data[i]);

            var result = new byte[data.Length - lenIV];
            for (int i = 0; i < result.Length; i++)
            {
                byte c = data[i + lenIV];
                result[i] = (byte)(c ^ (r >> 8));
                r = Step(r, c);
            }
            return result;
        }

        // ── Cipher step ───────────────────────────────────────────────────────

        private static ushort Step(ushort r, byte c)
            => (ushort)((c + r) * CipherC1 + CipherC2);

        // ── PFA hex decoder ───────────────────────────────────────────────────

        private static byte[] DecodeHexString(string hex)
        {
            // Strip whitespace and count valid hex chars
            var clean = new System.Text.StringBuilder(hex.Length);
            foreach (char c in hex)
            {
                if ((c >= '0' && c <= '9') || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F'))
                    clean.Append(c);
            }

            string s = clean.ToString();
            int count = s.Length / 2;
            var bytes = new byte[count];
            for (int i = 0; i < count; i++)
                bytes[i] = Convert.ToByte(s.Substring(i * 2, 2), 16);
            return bytes;
        }

        // ── Tokenizer ─────────────────────────────────────────────────────────

        internal static List<string> Tokenize(string text)
        {
            var tokens = new List<string>();
            int i = 0, n = text.Length;

            while (i < n)
            {
                char c = text[i];

                // Whitespace
                if (c == ' ' || c == '\t' || c == '\r' || c == '\n' || c == '\f' || c == '\0')
                { i++; continue; }

                // Comment
                if (c == '%')
                { while (i < n && text[i] != '\n') i++; continue; }

                // Dictionary << >>
                if (c == '<' && i + 1 < n && text[i + 1] == '<')
                { tokens.Add("<<"); i += 2; continue; }
                if (c == '>' && i + 1 < n && text[i + 1] == '>')
                { tokens.Add(">>"); i += 2; continue; }

                // Hex string <hexdata>
                if (c == '<')
                {
                    int start = ++i;
                    while (i < n && text[i] != '>') i++;
                    tokens.Add("<" + text.Substring(start, i - start) + ">");
                    if (i < n) i++;
                    continue;
                }

                // Procedure { }
                if (c == '{')
                {
                    int depth = 1, start = i++;
                    while (i < n && depth > 0)
                    {
                        if (text[i] == '{') depth++;
                        else if (text[i] == '}') depth--;
                        i++;
                    }
                    tokens.Add(text.Substring(start, i - start));
                    continue;
                }

                // Array [ ]
                if (c == '[')
                {
                    int depth = 1, start = i++;
                    while (i < n && depth > 0)
                    {
                        if (text[i] == '[') depth++;
                        else if (text[i] == ']') depth--;
                        i++;
                    }
                    tokens.Add(text.Substring(start, i - start));
                    continue;
                }

                // PS string ( )
                if (c == '(')
                {
                    int depth = 1, start = i++;
                    while (i < n && depth > 0)
                    {
                        if (text[i] == '\\') { i += 2; continue; }
                        if (text[i] == '(') depth++;
                        else if (text[i] == ')') depth--;
                        i++;
                    }
                    tokens.Add(text.Substring(start, i - start));
                    continue;
                }

                // Delimiter chars not already handled
                if (c == ')' || c == ']' || c == '}' || c == '>')
                { tokens.Add(c.ToString()); i++; continue; }

                // Name/keyword/number: read until a delimiter
                {
                    int start = i;
                    if (c == '/') i++; // consume '/' as part of the literal name
                    while (i < n && !IsTokDelim(text[i])) i++;
                    if (i > start) tokens.Add(text.Substring(start, i - start));
                    else i++;
                }
            }

            return tokens;
        }

        private static bool IsTokDelim(char c)
            => c == ' ' || c == '\t' || c == '\r' || c == '\n' || c == '\f' || c == '\0'
            || c == '(' || c == ')' || c == '<' || c == '>'
            || c == '[' || c == ']' || c == '{' || c == '}'
            || c == '/' || c == '%';

        private static bool IsSkipKeyword(string t)
            => t == "executeonly" || t == "put"      || t == "def"
            || t == "noaccess"   || t == "readonly"  || t == "bind"
            || t == "ND"         || t == "|-"        || t == "--"
            || t == "NP"         || t == "RD";

        // ── Numeric block parser ──────────────────────────────────────────────

        private static double[] ParseNumericBlock(string token)
        {
            string inner = token.Trim('{', '}', '[', ']');
            string[] parts = inner.Split(new[] { ' ', '\t', '\r', '\n' },
                                         StringSplitOptions.RemoveEmptyEntries);
            var nums = new double[parts.Length];
            for (int i = 0; i < parts.Length; i++)
                double.TryParse(parts[i], NumberStyles.Float,
                                CultureInfo.InvariantCulture, out nums[i]);
            return nums;
        }

        // ── Hex token decoder ─────────────────────────────────────────────────

        private static byte[] ParseHexToken(string token)
        {
            // Strip surrounding < >
            int start = token.StartsWith("<", StringComparison.Ordinal) ? 1 : 0;
            int end   = token.EndsWith(">", StringComparison.Ordinal)   ? token.Length - 1 : token.Length;

            var sb = new System.Text.StringBuilder(end - start);
            for (int i = start; i < end; i++)
            {
                char c = token[i];
                if ((c >= '0' && c <= '9') || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F'))
                    sb.Append(c);
            }

            string hex = sb.ToString();
            if (hex.Length % 2 != 0) hex += "0";

            var bytes = new byte[hex.Length / 2];
            for (int i = 0; i < bytes.Length; i++)
                bytes[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
            return bytes;
        }

        // ── Standard Encoding ─────────────────────────────────────────────────

        /// <summary>
        /// Returns the 256-entry PostScript Standard Encoding array.
        /// Each element is a glyph name; unmapped slots contain ".notdef".
        /// </summary>
        public static string[] GetStandardEncoding()
        {
            var enc = new string[256];
            for (int i = 0; i < 256; i++) enc[i] = ".notdef";

            enc[32]  = "space";       enc[33]  = "exclam";     enc[34]  = "quotedbl";
            enc[35]  = "numbersign";  enc[36]  = "dollar";     enc[37]  = "percent";
            enc[38]  = "ampersand";   enc[39]  = "quoteright"; enc[40]  = "parenleft";
            enc[41]  = "parenright";  enc[42]  = "asterisk";   enc[43]  = "plus";
            enc[44]  = "comma";       enc[45]  = "hyphen";     enc[46]  = "period";
            enc[47]  = "slash";
            enc[48]  = "zero";  enc[49] = "one";   enc[50] = "two";   enc[51] = "three";
            enc[52]  = "four";  enc[53] = "five";  enc[54] = "six";   enc[55] = "seven";
            enc[56]  = "eight"; enc[57] = "nine";
            enc[58]  = "colon";       enc[59]  = "semicolon";  enc[60]  = "less";
            enc[61]  = "equal";       enc[62]  = "greater";    enc[63]  = "question";
            enc[64]  = "at";

            // A–Z: standard glyph names match the character itself
            for (int i = 0; i < 26; i++) enc[65 + i] = ((char)('A' + i)).ToString();

            enc[91]  = "bracketleft"; enc[92]  = "backslash";  enc[93]  = "bracketright";
            enc[94]  = "asciicircum"; enc[95]  = "underscore"; enc[96]  = "quoteleft";

            // a–z
            for (int i = 0; i < 26; i++) enc[97 + i] = ((char)('a' + i)).ToString();

            enc[123] = "braceleft";   enc[124] = "bar";        enc[125] = "braceright";
            enc[126] = "asciitilde";

            enc[161] = "exclamdown";  enc[162] = "cent";       enc[163] = "sterling";
            enc[164] = "fraction";    enc[165] = "yen";        enc[166] = "florin";
            enc[167] = "section";     enc[168] = "currency";   enc[169] = "quotesingle";
            enc[170] = "quotedblleft"; enc[171] = "guillemotleft";
            enc[172] = "guilsinglleft"; enc[173] = "guilsinglright";
            enc[174] = "fi";          enc[175] = "fl";
            enc[177] = "endash";      enc[178] = "dagger";     enc[179] = "daggerdbl";
            enc[180] = "periodcentered"; enc[182] = "paragraph"; enc[183] = "bullet";
            enc[184] = "quotesinglbase"; enc[185] = "quotedblbase";
            enc[186] = "quotedblright"; enc[187] = "guillemotright";
            enc[188] = "ellipsis";    enc[189] = "perthousand";
            enc[191] = "questiondown";
            enc[193] = "grave";       enc[194] = "acute";      enc[195] = "circumflex";
            enc[196] = "tilde";       enc[197] = "macron";     enc[198] = "breve";
            enc[199] = "dotaccent";   enc[200] = "dieresis";   enc[202] = "ring";
            enc[203] = "cedilla";     enc[205] = "hungarumlaut"; enc[206] = "ogonek";
            enc[207] = "caron";       enc[208] = "emdash";
            enc[225] = "AE";          enc[227] = "ordfeminine";
            enc[232] = "Lslash";      enc[233] = "Oslash";     enc[234] = "OE";
            enc[235] = "ordmasculine"; enc[241] = "ae";        enc[243] = "dotlessi";
            enc[246] = "lslash";      enc[247] = "oslash";     enc[248] = "oe";
            enc[249] = "germandbls";
            return enc;
        }

        // ── ISO Latin-1 Encoding ──────────────────────────────────────────────

        private static string[] GetIsoLatin1Encoding()
        {
            var enc = new string[256];
            for (int i = 0; i < 256; i++) enc[i] = ".notdef";

            // Copy ASCII range from Standard Encoding
            string[] std = GetStandardEncoding();
            for (int i = 32; i < 128; i++) enc[i] = std[i];

            // Latin-1 supplement (0xA0–0xFF)
            enc[160] = "space";       enc[161] = "exclamdown"; enc[162] = "cent";
            enc[163] = "sterling";    enc[164] = "currency";   enc[165] = "yen";
            enc[166] = "brokenbar";   enc[167] = "section";    enc[168] = "dieresis";
            enc[169] = "copyright";   enc[170] = "ordfeminine"; enc[171] = "guillemotleft";
            enc[172] = "logicalnot";  enc[173] = "softhyphen"; enc[174] = "registered";
            enc[175] = "macron";      enc[176] = "degree";     enc[177] = "plusminus";
            enc[178] = "twosuperior"; enc[179] = "threesuperior"; enc[180] = "acute";
            enc[181] = "mu";          enc[182] = "paragraph";  enc[183] = "periodcentered";
            enc[184] = "cedilla";     enc[185] = "onesuperior"; enc[186] = "ordmasculine";
            enc[187] = "guillemotright"; enc[188] = "onequarter"; enc[189] = "onehalf";
            enc[190] = "threequarters"; enc[191] = "questiondown";
            enc[192] = "Agrave";  enc[193] = "Aacute";  enc[194] = "Acircumflex";
            enc[195] = "Atilde";  enc[196] = "Adieresis"; enc[197] = "Aring";
            enc[198] = "AE";      enc[199] = "Ccedilla"; enc[200] = "Egrave";
            enc[201] = "Eacute";  enc[202] = "Ecircumflex"; enc[203] = "Edieresis";
            enc[204] = "Igrave";  enc[205] = "Iacute";  enc[206] = "Icircumflex";
            enc[207] = "Idieresis"; enc[208] = "Eth";   enc[209] = "Ntilde";
            enc[210] = "Ograve";  enc[211] = "Oacute";  enc[212] = "Ocircumflex";
            enc[213] = "Otilde";  enc[214] = "Odieresis"; enc[215] = "multiply";
            enc[216] = "Oslash";  enc[217] = "Ugrave";  enc[218] = "Uacute";
            enc[219] = "Ucircumflex"; enc[220] = "Udieresis"; enc[221] = "Yacute";
            enc[222] = "Thorn";   enc[223] = "germandbls";
            enc[224] = "agrave";  enc[225] = "aacute";  enc[226] = "acircumflex";
            enc[227] = "atilde";  enc[228] = "adieresis"; enc[229] = "aring";
            enc[230] = "ae";      enc[231] = "ccedilla"; enc[232] = "egrave";
            enc[233] = "eacute";  enc[234] = "ecircumflex"; enc[235] = "edieresis";
            enc[236] = "igrave";  enc[237] = "iacute";  enc[238] = "icircumflex";
            enc[239] = "idieresis"; enc[240] = "eth";   enc[241] = "ntilde";
            enc[242] = "ograve";  enc[243] = "oacute";  enc[244] = "ocircumflex";
            enc[245] = "otilde";  enc[246] = "odieresis"; enc[247] = "divide";
            enc[248] = "oslash";  enc[249] = "ugrave";  enc[250] = "uacute";
            enc[251] = "ucircumflex"; enc[252] = "udieresis"; enc[253] = "yacute";
            enc[254] = "thorn";   enc[255] = "ydieresis";
            return enc;
        }
    }
}
