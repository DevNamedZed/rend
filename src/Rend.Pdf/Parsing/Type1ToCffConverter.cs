#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Rend.Pdf.Parsing
{
    /// <summary>
    /// Converts Type1 PFA/PFB font data to a minimal OpenType/CFF font that Skia can load.
    /// Charstrings are interpreted into absolute outlines and re-emitted as Type2, rather
    /// than copied verbatim (Type1 and Type2 are different bytecodes).
    /// [SPEC] Adobe Type 1 Font Format (1990); Adobe CFF (Tech Note #5176).
    /// </summary>
    public static class Type1ToCffConverter
    {
        private const int FirstCustomStringId = 391;

        /// <summary>
        /// Attempts to convert Type1 font data (PFA or PFB) to OpenType/CFF.
        /// Returns null if conversion fails.
        /// </summary>
        public static byte[]? Convert(byte[] type1Data)
        {
            return Convert(type1Data, null);
        }

        /// <summary>
        /// Converts Type1 font data to OpenType/CFF, using <paramref name="glyphNameToUnicode"/>
        /// to build the Unicode → glyph cmap. When null, a built-in StandardEncoding/AGL
        /// mapping is used. The renderer passes its own glyph-name table so the cmap matches
        /// exactly the Unicode values it requests at draw time.
        /// </summary>
        public static byte[]? Convert(byte[] type1Data, Func<string, string?>? glyphNameToUnicode)
        {
            if (type1Data == null || type1Data.Length < 20)
            {
                return null;
            }

            byte[] asciiPart;
            byte[] binaryPart;
            try
            {
                if (type1Data[0] == 0x80)
                {
                    ExtractPfbSegments(type1Data, out asciiPart, out binaryPart);
                }
                else
                {
                    ExtractPfaSegments(type1Data, out asciiPart, out binaryPart);
                }
            }
            catch
            {
                return null;
            }

            if (asciiPart.Length == 0 || binaryPart.Length == 0)
            {
                return null;
            }

            try
            {
                Type1FontInfo fontInfo = ParseAsciiHeader(asciiPart);
                Type1PrivateData privateData = DecryptAndParsePrivateDict(binaryPart);
                if (privateData.CharStrings.Count == 0)
                {
                    return null;
                }

                var converted = ConvertGlyphs(privateData);
                byte[] cffData = BuildCff(fontInfo, converted);
                Dictionary<int, int> unicodeToGlyph = BuildUnicodeToGlyph(fontInfo, converted, glyphNameToUnicode);
                return OpenTypeFontBuilder.Build(cffData, fontInfo.FontName, converted.AdvanceWidths,
                    unicodeToGlyph, fontInfo.FontBBox);
            }
            catch
            {
                return null;
            }
        }

        private sealed class ConvertedGlyphs
        {
            public List<string> GlyphNames { get; } = new List<string>();
            public List<byte[]> CharStrings { get; } = new List<byte[]>();
            public List<int> AdvanceWidths { get; } = new List<int>();
        }

        private static ConvertedGlyphs ConvertGlyphs(Type1PrivateData privateData)
        {
            var result = new ConvertedGlyphs();
            var interpreter = new Type1CharStringInterpreter(privateData.Subrs,
                code =>
                {
                    string? name = Type1StandardEncoding.GetName(code);
                    return name != null && privateData.CharStrings.TryGetValue(name, out byte[]? cs) ? cs : null;
                });

            var glyphNames = new List<string> { ".notdef" };
            foreach (string name in privateData.CharStrings.Keys)
            {
                if (name != ".notdef")
                {
                    glyphNames.Add(name);
                }
            }

            foreach (string name in glyphNames)
            {
                if (privateData.CharStrings.TryGetValue(name, out byte[]? type1CharString))
                {
                    GlyphOutline outline = interpreter.Interpret(type1CharString);
                    result.CharStrings.Add(Type2CharStringWriter.Write(outline));
                    result.AdvanceWidths.Add((int)Math.Round(outline.AdvanceWidth));
                }
                else
                {
                    result.CharStrings.Add(new byte[] { 14 }); // endchar — empty .notdef
                    result.AdvanceWidths.Add(0);
                }
                result.GlyphNames.Add(name);
            }
            return result;
        }

        private static Dictionary<int, int> BuildUnicodeToGlyph(Type1FontInfo fontInfo,
            ConvertedGlyphs converted, Func<string, string?>? glyphNameToUnicode)
        {
            var map = new Dictionary<int, int>();
            var glyphIndexByName = new Dictionary<string, int>();
            for (int glyphIndex = 1; glyphIndex < converted.GlyphNames.Count; glyphIndex++)
            {
                string name = converted.GlyphNames[glyphIndex];
                glyphIndexByName[name] = glyphIndex;
                int codePoint = ResolveCodePoint(name, glyphNameToUnicode);
                if (codePoint > 0 && codePoint <= 0xFFFF && !map.ContainsKey(codePoint))
                {
                    map[codePoint] = glyphIndex;
                }
            }

            // Fallback for the renderer's raw-code path: map the font's own encoding
            // position (as a Latin-1 code point) to its glyph when not already mapped.
            for (int code = 0; code < 256; code++)
            {
                string? name = fontInfo.Encoding[code];
                if (name != null && glyphIndexByName.TryGetValue(name, out int glyphIndex) && !map.ContainsKey(code))
                {
                    map[code] = glyphIndex;
                }
            }
            return map;
        }

        private static int ResolveCodePoint(string name, Func<string, string?>? glyphNameToUnicode)
        {
            if (glyphNameToUnicode != null)
            {
                string? mapped = glyphNameToUnicode(name);
                if (!string.IsNullOrEmpty(mapped))
                {
                    return char.ConvertToUtf32(mapped, 0);
                }
            }
            return Type1StandardEncoding.GlyphNameToCodePoint(name);
        }

        private static void ExtractPfbSegments(byte[] data, out byte[] ascii, out byte[] binary)
        {
            var asciiParts = new List<byte>();
            var binaryParts = new List<byte>();
            int pos = 0;
            while (pos + 5 < data.Length && data[pos] == 0x80)
            {
                int type = data[pos + 1];
                int length = data[pos + 2] | (data[pos + 3] << 8) | (data[pos + 4] << 16) | (data[pos + 5] << 24);
                pos += 6;
                if (type == 1)
                {
                    for (int i = 0; i < length && pos + i < data.Length; i++)
                    {
                        asciiParts.Add(data[pos + i]);
                    }
                }
                else if (type == 2)
                {
                    for (int i = 0; i < length && pos + i < data.Length; i++)
                    {
                        binaryParts.Add(data[pos + i]);
                    }
                }
                else if (type == 3)
                {
                    break;
                }
                pos += length;
            }
            ascii = asciiParts.ToArray();
            binary = binaryParts.ToArray();
        }

        private static void ExtractPfaSegments(byte[] data, out byte[] ascii, out byte[] binary)
        {
            string text = Encoding.ASCII.GetString(data);
            int eexecPos = text.IndexOf("eexec", StringComparison.Ordinal);
            if (eexecPos < 0)
            {
                ascii = data;
                binary = Array.Empty<byte>();
                return;
            }

            ascii = Encoding.ASCII.GetBytes(text.Substring(0, eexecPos + 5));
            int dataStart = eexecPos + 5;
            while (dataStart < data.Length && (data[dataStart] == ' ' || data[dataStart] == '\n' ||
                   data[dataStart] == '\r' || data[dataStart] == '\t'))
            {
                dataStart++;
            }

            bool isHex = true;
            for (int probe = dataStart; probe < Math.Min(dataStart + 8, data.Length); probe++)
            {
                byte probeByte = data[probe];
                if (!((probeByte >= '0' && probeByte <= '9') || (probeByte >= 'a' && probeByte <= 'f') ||
                      (probeByte >= 'A' && probeByte <= 'F') || probeByte == ' ' || probeByte == '\n' || probeByte == '\r'))
                {
                    isHex = false;
                    break;
                }
            }

            if (isHex)
            {
                binary = DecodeHexBinary(data, dataStart);
            }
            else
            {
                int trimEnd = FindRawBinaryEnd(data, dataStart);
                binary = new byte[trimEnd - dataStart];
                Array.Copy(data, dataStart, binary, 0, binary.Length);
            }
        }

        private static byte[] DecodeHexBinary(byte[] data, int dataStart)
        {
            string hexText = Encoding.ASCII.GetString(data, dataStart, data.Length - dataStart);
            var binaryList = new List<byte>();
            for (int i = 0; i + 1 < hexText.Length; i += 2)
            {
                char high = hexText[i];
                char low = hexText[i + 1];
                while (high == ' ' || high == '\n' || high == '\r' || high == '\t')
                {
                    i++;
                    if (i + 1 >= hexText.Length) { break; }
                    high = hexText[i];
                    low = hexText[i + 1];
                }
                if (i + 1 >= hexText.Length) { break; }
                int highValue = HexVal(high);
                int lowValue = HexVal(low);
                if (highValue < 0 || lowValue < 0) { break; }
                binaryList.Add((byte)((highValue << 4) | lowValue));
            }
            return binaryList.ToArray();
        }

        private static int FindRawBinaryEnd(byte[] data, int dataStart)
        {
            for (int i = data.Length - 1; i > dataStart + 100; i--)
            {
                if (data[i] == '0' && i - 10 >= dataStart)
                {
                    bool allZeros = true;
                    for (int j = 0; j < 10; j++)
                    {
                        if (data[i - j] != '0') { allZeros = false; break; }
                    }
                    if (allZeros)
                    {
                        return i - 9;
                    }
                }
            }
            return data.Length;
        }

        private static int HexVal(char c)
        {
            if (c >= '0' && c <= '9') { return c - '0'; }
            if (c >= 'a' && c <= 'f') { return c - 'a' + 10; }
            if (c >= 'A' && c <= 'F') { return c - 'A' + 10; }
            return -1;
        }

        private sealed class Type1FontInfo
        {
            public string FontName = "Unknown";
            public string?[] Encoding = new string?[256];
            public float[] FontBBox = { 0, 0, 1000, 1000 };
        }

        private static Type1FontInfo ParseAsciiHeader(byte[] ascii)
        {
            var info = new Type1FontInfo();
            string text = Encoding.ASCII.GetString(ascii);
            ParseFontName(text, info);
            ParseFontBBox(text, info);
            ParseEncoding(text, info);
            return info;
        }

        private static void ParseFontName(string text, Type1FontInfo info)
        {
            int fontNamePos = text.IndexOf("/FontName", StringComparison.Ordinal);
            if (fontNamePos < 0) { return; }
            int nameStart = text.IndexOf('/', fontNamePos + 9);
            if (nameStart < 0) { return; }
            nameStart++;
            int nameEnd = nameStart;
            while (nameEnd < text.Length && text[nameEnd] != ' ' && text[nameEnd] != '\n' && text[nameEnd] != '\r')
            {
                nameEnd++;
            }
            info.FontName = text.Substring(nameStart, nameEnd - nameStart);
        }

        private static void ParseFontBBox(string text, Type1FontInfo info)
        {
            int bboxPos = text.IndexOf("/FontBBox", StringComparison.Ordinal);
            if (bboxPos < 0) { return; }
            int bracketStart = text.IndexOf('{', bboxPos);
            if (bracketStart < 0) { bracketStart = text.IndexOf('[', bboxPos); }
            if (bracketStart < 0) { return; }
            int bracketEnd = text.IndexOf('}', bracketStart);
            if (bracketEnd < 0) { bracketEnd = text.IndexOf(']', bracketStart); }
            if (bracketEnd < 0) { return; }

            string bboxStr = text.Substring(bracketStart + 1, bracketEnd - bracketStart - 1).Trim();
            string[] parts = bboxStr.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < 4 && i < parts.Length; i++)
            {
                if (float.TryParse(parts[i], System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture, out float value))
                {
                    info.FontBBox[i] = value;
                }
            }
        }

        private static void ParseEncoding(string text, Type1FontInfo info)
        {
            int encodingPos = text.IndexOf("/Encoding", StringComparison.Ordinal);
            if (encodingPos < 0) { return; }
            int searchEnd = text.IndexOf("readonly", encodingPos, StringComparison.Ordinal);
            if (searchEnd < 0) { searchEnd = text.IndexOf("def", encodingPos + 20, StringComparison.Ordinal); }
            if (searchEnd < 0) { searchEnd = text.Length; }

            string section = text.Substring(encodingPos, searchEnd - encodingPos);
            int dupPos = 0;
            while ((dupPos = section.IndexOf("dup ", dupPos, StringComparison.Ordinal)) >= 0)
            {
                dupPos += 4;
                int numEnd = dupPos;
                while (numEnd < section.Length && section[numEnd] >= '0' && section[numEnd] <= '9')
                {
                    numEnd++;
                }
                if (numEnd == dupPos) { continue; }
                if (int.TryParse(section.Substring(dupPos, numEnd - dupPos), out int charCode) &&
                    charCode >= 0 && charCode < 256)
                {
                    int slashPos = section.IndexOf('/', numEnd);
                    if (slashPos >= 0 && slashPos < numEnd + 10)
                    {
                        int nameStart = slashPos + 1;
                        int nameEnd = nameStart;
                        while (nameEnd < section.Length && section[nameEnd] != ' ' &&
                               section[nameEnd] != '\n' && section[nameEnd] != '\r')
                        {
                            nameEnd++;
                        }
                        info.Encoding[charCode] = section.Substring(nameStart, nameEnd - nameStart);
                    }
                }
            }
        }

        private sealed class Type1PrivateData
        {
            public Dictionary<string, byte[]> CharStrings = new Dictionary<string, byte[]>();
            public List<byte[]> Subrs = new List<byte[]>();
        }

        private static Type1PrivateData DecryptAndParsePrivateDict(byte[] encrypted)
        {
            byte[] decrypted = DecryptEexec(encrypted);
            string text = Encoding.ASCII.GetString(decrypted);
            int lenIV = ParseLenIV(text);
            return new Type1PrivateData
            {
                Subrs = ParseSubrs(decrypted, text, lenIV),
                CharStrings = ParseCharStrings(decrypted, text, lenIV),
            };
        }

        private static byte[] DecryptEexec(byte[] encrypted)
        {
            int key = 55665;
            byte[] decrypted = new byte[encrypted.Length];
            for (int i = 0; i < encrypted.Length; i++)
            {
                byte cipher = encrypted[i];
                decrypted[i] = (byte)(cipher ^ (key >> 8));
                key = ((cipher + key) * 52845 + 22719) & 0xFFFF;
            }
            return decrypted;
        }

        private static int ParseLenIV(string text)
        {
            int pos = text.IndexOf("/lenIV", StringComparison.Ordinal);
            if (pos < 0) { return 4; }
            pos += 6;
            while (pos < text.Length && (text[pos] == ' ' || text[pos] == '\t')) { pos++; }
            int start = pos;
            while (pos < text.Length && (char.IsDigit(text[pos]) || text[pos] == '-')) { pos++; }
            return pos > start && int.TryParse(text.Substring(start, pos - start), out int value) ? value : 4;
        }

        private static List<byte[]> ParseSubrs(byte[] decrypted, string text, int lenIV)
        {
            var byIndex = new Dictionary<int, byte[]>();
            int subrsPos = text.IndexOf("/Subrs", StringComparison.Ordinal);
            if (subrsPos < 0) { return new List<byte[]>(); }
            int limit = text.IndexOf("/CharStrings", subrsPos, StringComparison.Ordinal);
            if (limit < 0) { limit = text.Length; }

            int pos = subrsPos + 6;
            int maxIndex = -1;
            while (true)
            {
                int dupPos = text.IndexOf("dup ", pos, StringComparison.Ordinal);
                if (dupPos < 0 || dupPos >= limit) { break; }
                pos = dupPos + 4;
                int index = ReadInt(text, ref pos);
                int length = ReadInt(text, ref pos);
                int markerPos = FindBinaryMarker(text, pos, limit);
                if (markerPos < 0) { break; }
                int dataStart = markerPos + 2;
                if (dataStart < text.Length && text[dataStart] == ' ') { dataStart++; }
                if (index >= 0 && length >= 0 && dataStart + length <= decrypted.Length)
                {
                    byIndex[index] = DecryptCharString(decrypted, dataStart, length, lenIV);
                    if (index > maxIndex) { maxIndex = index; }
                }
                pos = dataStart + Math.Max(length, 0);
            }

            var list = new List<byte[]>();
            for (int i = 0; i <= maxIndex; i++)
            {
                list.Add(byIndex.TryGetValue(i, out byte[]? entry) ? entry : Array.Empty<byte>());
            }
            return list;
        }

        private static Dictionary<string, byte[]> ParseCharStrings(byte[] decrypted, string text, int lenIV)
        {
            var charStrings = new Dictionary<string, byte[]>();
            int csStart = text.IndexOf("/CharStrings", StringComparison.Ordinal);
            if (csStart < 0) { return charStrings; }

            int pos = csStart + "/CharStrings".Length;
            while (pos < text.Length)
            {
                int slashPos = text.IndexOf('/', pos);
                if (slashPos < 0) { break; }

                int nameStart = slashPos + 1;
                int nameEnd = nameStart;
                while (nameEnd < text.Length && text[nameEnd] != ' ' && text[nameEnd] != '\n' &&
                       text[nameEnd] != '\r' && text[nameEnd] != '\t')
                {
                    nameEnd++;
                }
                string glyphName = text.Substring(nameStart, nameEnd - nameStart);

                int lengthPos = nameEnd;
                int length = ReadInt(text, ref lengthPos);
                if (length < 0)
                {
                    pos = slashPos + 1;
                    continue;
                }

                int markerPos = FindBinaryMarker(text, lengthPos, text.Length);
                if (markerPos < 0)
                {
                    pos = slashPos + 1;
                    continue;
                }
                int dataStart = markerPos + 2;
                if (dataStart < text.Length && text[dataStart] == ' ') { dataStart++; }
                if (dataStart + length <= decrypted.Length)
                {
                    charStrings[glyphName] = DecryptCharString(decrypted, dataStart, length, lenIV);
                }
                pos = dataStart + length;
            }
            return charStrings;
        }

        private static int ReadInt(string text, ref int pos)
        {
            while (pos < text.Length && !char.IsDigit(text[pos]) && text[pos] != '-')
            {
                if (text[pos] == '/' || text[pos] == '}') { return -1; }
                pos++;
            }
            int start = pos;
            if (pos < text.Length && text[pos] == '-') { pos++; }
            while (pos < text.Length && char.IsDigit(text[pos])) { pos++; }
            return pos > start && int.TryParse(text.Substring(start, pos - start), out int value) ? value : -1;
        }

        private static int FindBinaryMarker(string text, int from, int limit)
        {
            int rd = text.IndexOf("RD", from, StringComparison.Ordinal);
            int alt = text.IndexOf("-|", from, StringComparison.Ordinal);
            int marker = rd < 0 ? alt : (alt < 0 ? rd : Math.Min(rd, alt));
            return marker >= 0 && marker < limit ? marker : -1;
        }

        private static byte[] DecryptCharString(byte[] source, int offset, int length, int lenIV)
        {
            int key = 4330;
            byte[] decrypted = new byte[length];
            for (int i = 0; i < length; i++)
            {
                byte cipher = source[offset + i];
                decrypted[i] = (byte)(cipher ^ (key >> 8));
                key = ((cipher + key) * 52845 + 22719) & 0xFFFF;
            }
            if (lenIV <= 0 || lenIV >= length)
            {
                return lenIV <= 0 ? decrypted : Array.Empty<byte>();
            }
            byte[] result = new byte[length - lenIV];
            Array.Copy(decrypted, lenIV, result, 0, result.Length);
            return result;
        }

        private static byte[] BuildCff(Type1FontInfo info, ConvertedGlyphs converted)
        {
            byte[] nameIndexData = BuildIndex(new[] { Encoding.ASCII.GetBytes(info.FontName) });

            var customStrings = new List<byte[]>();
            for (int glyphIndex = 1; glyphIndex < converted.GlyphNames.Count; glyphIndex++)
            {
                customStrings.Add(Encoding.ASCII.GetBytes(converted.GlyphNames[glyphIndex]));
            }
            byte[] stringIndexData = BuildIndex(customStrings.ToArray());
            byte[] globalSubrIndexData = BuildIndex(Array.Empty<byte[]>());
            byte[] charsetData = BuildCharset(converted.GlyphNames.Count);
            byte[] charStringsIndexData = BuildIndex(converted.CharStrings.ToArray());

            var privateStream = new MemoryStream();
            EncodeCffInt(privateStream, 0); privateStream.WriteByte(20); // defaultWidthX
            EncodeCffInt(privateStream, 0); privateStream.WriteByte(21); // nominalWidthX
            byte[] privateDictData = privateStream.ToArray();

            int headerSize = 4;
            int charsetOffset = 0;
            int charStringsOffset = 0;
            int privateDictOffset = 0;
            byte[] topDictIndexData = Array.Empty<byte>();

            for (int pass = 0; pass < 3; pass++)
            {
                topDictIndexData = BuildTopDictIndex(charsetOffset, charStringsOffset,
                    privateDictData.Length, privateDictOffset);
                charsetOffset = headerSize + nameIndexData.Length + topDictIndexData.Length +
                                stringIndexData.Length + globalSubrIndexData.Length;
                charStringsOffset = charsetOffset + charsetData.Length;
                privateDictOffset = charStringsOffset + charStringsIndexData.Length;
            }
            topDictIndexData = BuildTopDictIndex(charsetOffset, charStringsOffset,
                privateDictData.Length, privateDictOffset);

            var result = new MemoryStream();
            result.Write(new byte[] { 1, 0, 4, 1 }, 0, 4);
            result.Write(nameIndexData, 0, nameIndexData.Length);
            result.Write(topDictIndexData, 0, topDictIndexData.Length);
            result.Write(stringIndexData, 0, stringIndexData.Length);
            result.Write(globalSubrIndexData, 0, globalSubrIndexData.Length);
            result.Write(charsetData, 0, charsetData.Length);
            result.Write(charStringsIndexData, 0, charStringsIndexData.Length);
            result.Write(privateDictData, 0, privateDictData.Length);
            return result.ToArray();
        }

        private static byte[] BuildTopDictIndex(int charsetOffset, int charStringsOffset,
            int privateSize, int privateOffset)
        {
            var topDict = new MemoryStream();
            EncodeCffInt(topDict, charsetOffset); topDict.WriteByte(15);    // charset
            EncodeCffInt(topDict, charStringsOffset); topDict.WriteByte(17); // CharStrings
            EncodeCffInt(topDict, privateSize);
            EncodeCffInt(topDict, privateOffset);
            topDict.WriteByte(18); // Private
            return BuildIndex(new[] { topDict.ToArray() });
        }

        private static byte[] BuildCharset(int glyphCount)
        {
            var charset = new MemoryStream();
            charset.WriteByte(0); // format 0
            for (int glyphIndex = 1; glyphIndex < glyphCount; glyphIndex++)
            {
                int sid = FirstCustomStringId + (glyphIndex - 1);
                charset.WriteByte((byte)(sid >> 8));
                charset.WriteByte((byte)(sid & 0xFF));
            }
            return charset.ToArray();
        }

        private static byte[] BuildIndex(byte[][] items)
        {
            if (items.Length == 0)
            {
                return new byte[] { 0, 0 };
            }

            int totalDataSize = 0;
            foreach (byte[] item in items)
            {
                totalDataSize += item.Length;
            }
            int offSize = totalDataSize + 1 <= 255 ? 1 : (totalDataSize + 1 <= 65535 ? 2 : 4);

            using var stream = new MemoryStream();
            stream.WriteByte((byte)(items.Length >> 8));
            stream.WriteByte((byte)(items.Length & 0xFF));
            stream.WriteByte((byte)offSize);
            int offset = 1;
            for (int i = 0; i <= items.Length; i++)
            {
                WriteOffset(stream, offset, offSize);
                if (i < items.Length)
                {
                    offset += items[i].Length;
                }
            }
            foreach (byte[] item in items)
            {
                stream.Write(item, 0, item.Length);
            }
            return stream.ToArray();
        }

        private static void WriteOffset(Stream stream, int offset, int offSize)
        {
            switch (offSize)
            {
                case 1:
                    stream.WriteByte((byte)offset);
                    break;
                case 2:
                    stream.WriteByte((byte)(offset >> 8));
                    stream.WriteByte((byte)(offset & 0xFF));
                    break;
                default:
                    stream.WriteByte((byte)(offset >> 24));
                    stream.WriteByte((byte)(offset >> 16));
                    stream.WriteByte((byte)(offset >> 8));
                    stream.WriteByte((byte)(offset & 0xFF));
                    break;
            }
        }

        private static void EncodeCffInt(Stream stream, int value)
        {
            if (value >= -107 && value <= 107)
            {
                stream.WriteByte((byte)(value + 139));
            }
            else if (value >= 108 && value <= 1131)
            {
                int adjusted = value - 108;
                stream.WriteByte((byte)(adjusted / 256 + 247));
                stream.WriteByte((byte)(adjusted % 256));
            }
            else if (value >= -1131 && value <= -108)
            {
                int adjusted = -value - 108;
                stream.WriteByte((byte)(adjusted / 256 + 251));
                stream.WriteByte((byte)(adjusted % 256));
            }
            else
            {
                stream.WriteByte(29);
                stream.WriteByte((byte)(value >> 24));
                stream.WriteByte((byte)(value >> 16));
                stream.WriteByte((byte)(value >> 8));
                stream.WriteByte((byte)(value & 0xFF));
            }
        }
    }
}
