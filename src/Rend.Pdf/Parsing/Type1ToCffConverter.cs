#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Rend.Pdf.Parsing
{
    /// <summary>
    /// Converts Type1 PFA/PFB font data to a minimal OpenType/CFF font that Skia can load.
    /// [SPEC] Adobe Type 1 Font Format (1990), Adobe CFF Spec (Tech Note #5176)
    /// </summary>
    public static class Type1ToCffConverter
    {
        /// <summary>
        /// Attempts to convert Type1 font data (PFA or PFB) to OpenType/CFF.
        /// Returns null if conversion fails.
        /// </summary>
        public static byte[]? Convert(byte[] type1Data)
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
                var fontInfo = ParseAsciiHeader(asciiPart);
                var charStrings = DecryptAndParseCharStrings(binaryPart);

                if (charStrings.Count == 0)
                {
                    return null;
                }

                byte[] cffData = BuildCff(fontInfo, charStrings);
                int numGlyphs = charStrings.Count;
                if (!charStrings.ContainsKey(".notdef"))
                {
                    numGlyphs++;
                }
                byte[] otfData = WrapInOpenType(cffData, fontInfo, numGlyphs);
                return otfData;
            }
            catch
            {
                return null;
            }
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

            // After "eexec" and whitespace, check if hex or binary
            int dataStart = eexecPos + 5;
            while (dataStart < data.Length && (data[dataStart] == ' ' || data[dataStart] == '\n' || data[dataStart] == '\r' || data[dataStart] == '\t'))
            {
                dataStart++;
            }

            // Check if the data is hex-encoded or raw binary
            bool isHex = true;
            for (int probe = dataStart; probe < Math.Min(dataStart + 8, data.Length); probe++)
            {
                byte probeB = data[probe];
                if (!((probeB >= '0' && probeB <= '9') || (probeB >= 'a' && probeB <= 'f') || (probeB >= 'A' && probeB <= 'F') ||
                      probeB == ' ' || probeB == '\n' || probeB == '\r'))
                {
                    isHex = false;
                    break;
                }
            }

            if (isHex)
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
                    int hv = HexVal(high);
                    int lv = HexVal(low);
                    if (hv < 0 || lv < 0) { break; }
                    binaryList.Add((byte)((hv << 4) | lv));
                }
                binary = binaryList.ToArray();
            }
            else
            {
                // Raw binary after eexec
                int binaryLength = data.Length - dataStart;
                // Trim trailing ASCII (the cleartext section at the end)
                // Look for "0000000000" pattern or "cleartomark" near the end
                int trimEnd = data.Length;
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
                            trimEnd = i - 9;
                            break;
                        }
                    }
                }
                binaryLength = trimEnd - dataStart;
                binary = new byte[binaryLength];
                Array.Copy(data, dataStart, binary, 0, binaryLength);
            }
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
            public string[] Encoding = new string[256];
            public float[] FontMatrix = { 0.001f, 0, 0, 0.001f, 0, 0 };
            public float[] FontBBox = { 0, 0, 1000, 1000 };
            public int UnitsPerEm = 1000;
        }

        private static Type1FontInfo ParseAsciiHeader(byte[] ascii)
        {
            var info = new Type1FontInfo();
            string text = Encoding.ASCII.GetString(ascii);

            // Extract FontName
            int fontNamePos = text.IndexOf("/FontName", StringComparison.Ordinal);
            if (fontNamePos >= 0)
            {
                int nameStart = text.IndexOf('/', fontNamePos + 9);
                if (nameStart >= 0)
                {
                    nameStart++;
                    int nameEnd = nameStart;
                    while (nameEnd < text.Length && text[nameEnd] != ' ' && text[nameEnd] != '\n' && text[nameEnd] != '\r')
                    {
                        nameEnd++;
                    }
                    info.FontName = text.Substring(nameStart, nameEnd - nameStart);
                }
            }

            // Extract FontBBox
            int bboxPos = text.IndexOf("/FontBBox", StringComparison.Ordinal);
            if (bboxPos >= 0)
            {
                int bracketStart = text.IndexOf('{', bboxPos);
                if (bracketStart < 0) { bracketStart = text.IndexOf('[', bboxPos); }
                if (bracketStart >= 0)
                {
                    int bracketEnd = text.IndexOf('}', bracketStart);
                    if (bracketEnd < 0) { bracketEnd = text.IndexOf(']', bracketStart); }
                    if (bracketEnd >= 0)
                    {
                        string bboxStr = text.Substring(bracketStart + 1, bracketEnd - bracketStart - 1).Trim();
                        string[] parts = bboxStr.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length >= 4)
                        {
                            for (int i = 0; i < 4; i++)
                            {
                                if (float.TryParse(parts[i], System.Globalization.NumberStyles.Float,
                                    System.Globalization.CultureInfo.InvariantCulture, out float val))
                                {
                                    info.FontBBox[i] = val;
                                }
                            }
                        }
                    }
                }
            }

            // Extract Encoding
            int encodingPos = text.IndexOf("/Encoding", StringComparison.Ordinal);
            if (encodingPos >= 0)
            {
                // Look for "dup <index> /<name> put" patterns
                int searchStart = encodingPos;
                int searchEnd = text.IndexOf("readonly", searchStart);
                if (searchEnd < 0) { searchEnd = text.IndexOf("def", searchStart + 20); }
                if (searchEnd < 0) { searchEnd = text.Length; }

                string encodingSection = text.Substring(searchStart, searchEnd - searchStart);
                int dupPos = 0;
                while ((dupPos = encodingSection.IndexOf("dup ", dupPos, StringComparison.Ordinal)) >= 0)
                {
                    dupPos += 4;
                    // Parse "dup <number> /<name> put"
                    int numEnd = dupPos;
                    while (numEnd < encodingSection.Length && encodingSection[numEnd] >= '0' && encodingSection[numEnd] <= '9')
                    {
                        numEnd++;
                    }
                    if (numEnd == dupPos) { continue; }

                    if (int.TryParse(encodingSection.Substring(dupPos, numEnd - dupPos), out int charCode) &&
                        charCode >= 0 && charCode < 256)
                    {
                        int slashPos = encodingSection.IndexOf('/', numEnd);
                        if (slashPos >= 0 && slashPos < numEnd + 10)
                        {
                            int nameStart = slashPos + 1;
                            int nameEnd = nameStart;
                            while (nameEnd < encodingSection.Length && encodingSection[nameEnd] != ' ' &&
                                   encodingSection[nameEnd] != '\n' && encodingSection[nameEnd] != '\r')
                            {
                                nameEnd++;
                            }
                            info.Encoding[charCode] = encodingSection.Substring(nameStart, nameEnd - nameStart);
                        }
                    }
                }
            }

            return info;
        }

        private static Dictionary<string, byte[]> DecryptAndParseCharStrings(byte[] encrypted)
        {
            var charStrings = new Dictionary<string, byte[]>();

            // eexec decryption: key starts at 55665, decrypt 4 random bytes first
            int key = 55665;
            byte[] decrypted = new byte[encrypted.Length];
            for (int i = 0; i < encrypted.Length; i++)
            {
                byte cipher = encrypted[i];
                byte plain = (byte)(cipher ^ (key >> 8));
                key = ((cipher + key) * 52845 + 22719) & 0xFFFF;
                decrypted[i] = plain;
            }

            // Skip first 4 random bytes
            string decryptedText = Encoding.ASCII.GetString(decrypted, 4, decrypted.Length - 4);

            // Find CharStrings section
            int csStart = decryptedText.IndexOf("/CharStrings", StringComparison.Ordinal);
            if (csStart < 0)
            {
                return charStrings;
            }

            // Parse "/<name> <length> RD <binary_data> ND" entries
            int pos = csStart;
            while (pos < decryptedText.Length)
            {
                int slashPos = decryptedText.IndexOf('/', pos);
                if (slashPos < 0)
                {
                    break;
                }

                // Check for end of CharStrings
                string remaining = decryptedText.Substring(slashPos, Math.Min(20, decryptedText.Length - slashPos));
                if (remaining.StartsWith("/FontName") || remaining.StartsWith("/Subrs"))
                {
                    pos = slashPos + 1;
                    continue;
                }

                // Parse glyph name
                int nameStart = slashPos + 1;
                int nameEnd = nameStart;
                while (nameEnd < decryptedText.Length && decryptedText[nameEnd] != ' ' &&
                       decryptedText[nameEnd] != '\n' && decryptedText[nameEnd] != '\r')
                {
                    nameEnd++;
                }
                string glyphName = decryptedText.Substring(nameStart, nameEnd - nameStart);

                // Parse length
                int numStart = nameEnd;
                while (numStart < decryptedText.Length && !char.IsDigit(decryptedText[numStart]))
                {
                    numStart++;
                }
                int numEnd2 = numStart;
                while (numEnd2 < decryptedText.Length && char.IsDigit(decryptedText[numEnd2]))
                {
                    numEnd2++;
                }

                if (numStart < numEnd2 && int.TryParse(decryptedText.Substring(numStart, numEnd2 - numStart), out int csLength))
                {
                    // Find RD or -| marker
                    int rdPos = decryptedText.IndexOf("RD", numEnd2, StringComparison.Ordinal);
                    if (rdPos < 0) { rdPos = decryptedText.IndexOf("-|", numEnd2, StringComparison.Ordinal); }

                    if (rdPos >= 0)
                    {
                        int dataStart = rdPos + 2;
                        if (dataStart < decryptedText.Length && decryptedText[dataStart] == ' ')
                        {
                            dataStart++;
                        }

                        // Extract binary charstring data (from the decrypted bytes, not text)
                        int absoluteOffset = dataStart + 4; // +4 for the skipped random bytes
                        if (absoluteOffset + csLength <= decrypted.Length)
                        {
                            // CharString data needs its own decryption (key=4330)
                            byte[] csEncrypted = new byte[csLength];
                            Array.Copy(decrypted, absoluteOffset, csEncrypted, 0, csLength);

                            byte[] csDecrypted = DecryptCharString(csEncrypted);
                            charStrings[glyphName] = csDecrypted;
                        }

                        pos = dataStart + csLength;
                        continue;
                    }
                }

                pos = slashPos + 1;
            }

            return charStrings;
        }

        private static byte[] DecryptCharString(byte[] encrypted)
        {
            int key = 4330;
            byte[] decrypted = new byte[encrypted.Length];
            for (int i = 0; i < encrypted.Length; i++)
            {
                byte cipher = encrypted[i];
                byte plain = (byte)(cipher ^ (key >> 8));
                key = ((cipher + key) * 52845 + 22719) & 0xFFFF;
                decrypted[i] = plain;
            }

            // Skip lenIV random bytes (default 4)
            if (decrypted.Length > 4)
            {
                byte[] result = new byte[decrypted.Length - 4];
                Array.Copy(decrypted, 4, result, 0, result.Length);
                return result;
            }
            return decrypted;
        }

        private static byte[] BuildCff(Type1FontInfo info, Dictionary<string, byte[]> charStrings)
        {
            // Build glyph list with .notdef first
            var glyphNames = new List<string> { ".notdef" };
            foreach (var name in charStrings.Keys)
            {
                if (name != ".notdef")
                {
                    glyphNames.Add(name);
                }
            }

            // Build static sections
            byte[] nameIndexData = BuildIndex(new[] { Encoding.ASCII.GetBytes(info.FontName) });
            byte[] stringIndexData = BuildIndex(Array.Empty<byte[]>());
            byte[] globalSubrIndexData = BuildIndex(Array.Empty<byte[]>());

            // Charset (format 0)
            var charsetBytes = new MemoryStream();
            charsetBytes.WriteByte(0);
            for (int i = 1; i < glyphNames.Count; i++)
            {
                int sid = GetStandardSid(glyphNames[i]);
                charsetBytes.WriteByte((byte)(sid >> 8));
                charsetBytes.WriteByte((byte)(sid & 0xFF));
            }
            byte[] charsetData = charsetBytes.ToArray();

            // CharStrings INDEX
            var charStringEntries = new List<byte[]>();
            foreach (var name in glyphNames)
            {
                if (charStrings.TryGetValue(name, out byte[]? cs))
                {
                    charStringEntries.Add(cs);
                }
                else
                {
                    charStringEntries.Add(new byte[] { 14 }); // endchar
                }
            }
            byte[] charStringsIndexData = BuildIndex(charStringEntries.ToArray());

            // Private DICT (minimal — required by CFF spec)
            var privateDictStream = new MemoryStream();
            // defaultWidthX = 0
            EncodeCffInt(privateDictStream, 0); privateDictStream.WriteByte(20);
            // nominalWidthX = 0
            EncodeCffInt(privateDictStream, 0); privateDictStream.WriteByte(21);
            byte[] privateDictData = privateDictStream.ToArray();

            // Iterative offset resolution (Top DICT contains offsets to other sections)
            int headerSize = 4;
            byte[] topDictIndexData;
            int charsetOffset;
            int charStringsOffset;
            int privateDictOffset;

            // First pass estimate
            charsetOffset = 100; // placeholder
            charStringsOffset = charsetOffset + charsetData.Length;
            privateDictOffset = charStringsOffset + charStringsIndexData.Length;

            for (int pass = 0; pass < 3; pass++)
            {
                var topDict = new MemoryStream();
                EncodeCffInt(topDict, charsetOffset); topDict.WriteByte(15); // charset
                EncodeCffInt(topDict, charStringsOffset); topDict.WriteByte(17); // CharStrings
                // Private DICT: size then offset (operator 18)
                EncodeCffInt(topDict, privateDictData.Length);
                EncodeCffInt(topDict, privateDictOffset);
                topDict.WriteByte(18); // Private
                byte[] topDictData = topDict.ToArray();
                topDictIndexData = BuildIndex(new[] { topDictData });

                charsetOffset = headerSize + nameIndexData.Length + topDictIndexData.Length +
                                stringIndexData.Length + globalSubrIndexData.Length;
                charStringsOffset = charsetOffset + charsetData.Length;
                privateDictOffset = charStringsOffset + charStringsIndexData.Length;
            }

            // Final Top DICT with stable offsets
            var finalTopDict = new MemoryStream();
            EncodeCffInt(finalTopDict, charsetOffset); finalTopDict.WriteByte(15);
            EncodeCffInt(finalTopDict, charStringsOffset); finalTopDict.WriteByte(17);
            EncodeCffInt(finalTopDict, privateDictData.Length);
            EncodeCffInt(finalTopDict, privateDictOffset);
            finalTopDict.WriteByte(18);
            topDictIndexData = BuildIndex(new[] { finalTopDict.ToArray() });

            // Assemble CFF
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

        private static byte[] BuildIndex(byte[][] items)
        {
            if (items.Length == 0)
            {
                return new byte[] { 0, 0 }; // count=0
            }

            int totalDataSize = 0;
            foreach (var item in items)
            {
                totalDataSize += item.Length;
            }

            int offSize = totalDataSize + 1 <= 255 ? 1 : (totalDataSize + 1 <= 65535 ? 2 : 4);

            using var stream = new MemoryStream();
            // count
            stream.WriteByte((byte)(items.Length >> 8));
            stream.WriteByte((byte)(items.Length & 0xFF));
            // offSize
            stream.WriteByte((byte)offSize);
            // offsets (1-based)
            int offset = 1;
            for (int i = 0; i <= items.Length; i++)
            {
                WriteOffset(stream, offset, offSize);
                if (i < items.Length)
                {
                    offset += items[i].Length;
                }
            }
            // data
            foreach (var item in items)
            {
                stream.Write(item, 0, item.Length);
            }

            return stream.ToArray();
        }

        private static void WriteIndex(Stream stream, byte[][] items)
        {
            byte[] indexData = BuildIndex(items);
            stream.Write(indexData, 0, indexData.Length);
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
                case 4:
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
                int v = value - 108;
                stream.WriteByte((byte)(v / 256 + 247));
                stream.WriteByte((byte)(v % 256));
            }
            else if (value >= -1131 && value <= -108)
            {
                int v = -value - 108;
                stream.WriteByte((byte)(v / 256 + 251));
                stream.WriteByte((byte)(v % 256));
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

        private static int GetStandardSid(string glyphName)
        {
            // CFF standard strings: first 391 SIDs are predefined
            // For custom names, we'd need to add to the String INDEX
            // For now, return a high SID (custom string area)
            // Common glyph names have well-known SIDs:
            switch (glyphName)
            {
                case ".notdef": return 0;
                case "space": return 1;
                case "exclam": return 12;
                case "quotedbl": return 13;
                case "numbersign": return 14;
                case "dollar": return 15;
                case "percent": return 16;
                case "ampersand": return 17;
                case "quoteright": return 18;
                case "parenleft": return 19;
                case "parenright": return 20;
                case "asterisk": return 21;
                case "plus": return 22;
                case "comma": return 23;
                case "hyphen": return 24;
                case "period": return 25;
                case "slash": return 26;
                case "zero": return 27;
                case "one": return 28;
                case "two": return 29;
                case "three": return 30;
                case "four": return 31;
                case "five": return 32;
                case "six": return 33;
                case "seven": return 34;
                case "eight": return 35;
                case "nine": return 36;
                case "colon": return 37;
                case "semicolon": return 38;
                case "less": return 39;
                case "equal": return 40;
                case "greater": return 41;
                case "question": return 42;
                case "at": return 43;
                case "A": return 44;
                case "B": return 45;
                case "C": return 46;
                case "D": return 47;
                case "E": return 48;
                case "F": return 49;
                case "G": return 50;
                case "H": return 51;
                case "I": return 52;
                case "J": return 53;
                case "K": return 54;
                case "L": return 55;
                case "M": return 56;
                case "N": return 57;
                case "O": return 58;
                case "P": return 59;
                case "Q": return 60;
                case "R": return 61;
                case "S": return 62;
                case "T": return 63;
                case "U": return 64;
                case "V": return 65;
                case "W": return 66;
                case "X": return 67;
                case "Y": return 68;
                case "Z": return 69;
                case "bracketleft": return 70;
                case "backslash": return 71;
                case "bracketright": return 72;
                case "asciicircum": return 73;
                case "underscore": return 74;
                case "quoteleft": return 75;
                case "a": return 76;
                case "b": return 77;
                case "c": return 78;
                case "d": return 79;
                case "e": return 80;
                case "f": return 81;
                case "g": return 82;
                case "h": return 83;
                case "i": return 84;
                case "j": return 85;
                case "k": return 86;
                case "l": return 87;
                case "m": return 88;
                case "n": return 89;
                case "o": return 90;
                case "p": return 91;
                case "q": return 92;
                case "r": return 93;
                case "s": return 94;
                case "t": return 95;
                case "u": return 96;
                case "v": return 97;
                case "w": return 98;
                case "x": return 99;
                case "y": return 100;
                case "z": return 101;
                case "braceleft": return 102;
                case "bar": return 103;
                case "braceright": return 104;
                case "asciitilde": return 105;
                default: return 0; // Use .notdef for unknown
            }
        }

        private static byte[] WrapInOpenType(byte[] cffData, Type1FontInfo info, int numGlyphs)
        {
            if (cffData.Length == 0)
            {
                return Array.Empty<byte>();
            }

            var tables = new Dictionary<string, byte[]>();

            // CFF table
            tables["CFF "] = cffData;

            // head table
            tables["head"] = BuildHeadTable(info);

            // hhea table
            tables["hhea"] = BuildHheaTable(info, numGlyphs);

            // hmtx table (all glyphs get default width 500)
            tables["hmtx"] = BuildHmtxTable(numGlyphs);

            // maxp table
            tables["maxp"] = BuildMaxpTable(numGlyphs);

            // name table
            tables["name"] = BuildNameTable(info.FontName);

            // OS/2 table
            tables["OS/2"] = BuildOs2Table(info);

            // post table
            tables["post"] = BuildPostTable();

            // cmap table (identity mapping)
            tables["cmap"] = BuildCmapTable();

            return BuildSfnt(tables);
        }

        private static byte[] BuildHeadTable(Type1FontInfo info)
        {
            var data = new byte[54];
            WriteU16(data, 0, 0x0001); WriteU16(data, 2, 0x0000); // version 1.0
            WriteU16(data, 4, 0x0001); WriteU16(data, 6, 0x0000); // fontRevision
            WriteU32(data, 8, 0);       // checksumAdjustment (patched later)
            WriteU32(data, 12, 0x5F0F3CF5); // magicNumber
            WriteU16(data, 16, 0x000B); // flags
            WriteU16(data, 18, 1000);   // unitsPerEm
            // created/modified timestamps (8 bytes each) = 0
            WriteI16(data, 36, (short)info.FontBBox[0]); // xMin
            WriteI16(data, 38, (short)info.FontBBox[1]); // yMin
            WriteI16(data, 40, (short)info.FontBBox[2]); // xMax
            WriteI16(data, 42, (short)info.FontBBox[3]); // yMax
            WriteU16(data, 44, 0);      // macStyle
            WriteU16(data, 46, 8);      // lowestRecPPEM
            WriteI16(data, 48, 2);      // fontDirectionHint
            WriteI16(data, 50, 0);      // indexToLocFormat
            WriteI16(data, 52, 0);      // glyphDataFormat
            return data;
        }

        private static byte[] BuildHheaTable(Type1FontInfo info, int numGlyphs)
        {
            var data = new byte[36];
            WriteU16(data, 0, 0x0001); WriteU16(data, 2, 0x0000); // version
            WriteI16(data, 4, (short)info.FontBBox[3]);  // ascent
            WriteI16(data, 6, (short)info.FontBBox[1]);  // descent
            WriteI16(data, 8, 0);       // lineGap
            WriteU16(data, 10, 1000);   // advanceWidthMax
            // rest is zeros (minLSB, minRSB, xMaxExtent, caretSlope, reserved)
            WriteI16(data, 12, 0);      // minLeftSideBearing
            WriteI16(data, 22, 1);      // caretSlopeRise
            WriteU16(data, 34, (ushort)numGlyphs); // numberOfHMetrics
            return data;
        }

        private static byte[] BuildHmtxTable(int numGlyphs)
        {
            var data = new byte[numGlyphs * 4];
            for (int i = 0; i < numGlyphs; i++)
            {
                WriteU16(data, i * 4, 500);  // advanceWidth
                WriteI16(data, i * 4 + 2, 0); // lsb
            }
            return data;
        }

        private static byte[] BuildMaxpTable(int numGlyphs)
        {
            var data = new byte[6];
            WriteU16(data, 0, 0x0000); WriteU16(data, 2, 0x5000); // version 0.5 (CFF)
            WriteU16(data, 4, (ushort)numGlyphs);
            return data;
        }

        private static byte[] BuildNameTable(string fontName)
        {
            byte[] nameBytes = Encoding.UTF8.GetBytes(fontName);
            // Minimal name table with just nameID 1 (family), 2 (subfamily), 4 (full), 6 (postscript)
            int recordCount = 4;
            int stringOffset = 6 + recordCount * 12;

            using var stream = new MemoryStream();
            WriteU16(stream, 0);              // format
            WriteU16(stream, (ushort)recordCount);
            WriteU16(stream, (ushort)stringOffset);

            // platformID=3 (Windows), encodingID=1 (Unicode BMP), languageID=0x0409 (English)
            ushort[] nameIds = { 1, 2, 4, 6 };
            byte[] unicodeNameBytes = Encoding.BigEndianUnicode.GetBytes(fontName);
            byte[] regularBytes = Encoding.BigEndianUnicode.GetBytes("Regular");

            int offset = 0;
            foreach (ushort nameId in nameIds)
            {
                byte[] strBytes = (nameId == 2) ? regularBytes : unicodeNameBytes;
                WriteU16(stream, 3);                       // platformID
                WriteU16(stream, 1);                       // encodingID
                WriteU16(stream, 0x0409);                  // languageID
                WriteU16(stream, nameId);                  // nameID
                WriteU16(stream, (ushort)strBytes.Length);  // length
                WriteU16(stream, (ushort)offset);          // offset
                offset += strBytes.Length;
            }

            // String data
            foreach (ushort nameId in nameIds)
            {
                byte[] strBytes = (nameId == 2) ? regularBytes : unicodeNameBytes;
                stream.Write(strBytes, 0, strBytes.Length);
            }

            return stream.ToArray();
        }

        private static byte[] BuildOs2Table(Type1FontInfo info)
        {
            var data = new byte[78]; // version 1
            WriteU16(data, 0, 0x0001);  // version
            WriteI16(data, 2, 500);     // xAvgCharWidth
            WriteU16(data, 4, 400);     // usWeightClass (normal)
            WriteU16(data, 6, 5);       // usWidthClass (medium)
            // fsType, ySubscript*, ySuperscript*, yStrikeout* = 0
            WriteI16(data, 68, (short)info.FontBBox[3]);  // sTypoAscender
            WriteI16(data, 70, (short)info.FontBBox[1]);  // sTypoDescender
            WriteI16(data, 72, 0);      // sTypoLineGap
            WriteU16(data, 74, (ushort)Math.Max(0, info.FontBBox[3])); // usWinAscent
            WriteU16(data, 76, (ushort)Math.Max(0, -info.FontBBox[1])); // usWinDescent
            return data;
        }

        private static byte[] BuildPostTable()
        {
            var data = new byte[32];
            WriteU32(data, 0, 0x00030000); // version 3.0 (no glyph names)
            // italicAngle, underlinePosition, underlineThickness, isFixedPitch = 0
            return data;
        }

        private static byte[] BuildCmapTable()
        {
            // Format 0: byte encoding, maps char codes 0-255 directly to glyph indices
            using var stream = new MemoryStream();
            WriteU16(stream, 0);        // version
            WriteU16(stream, 1);        // numTables

            // Encoding record: platform=1 (Mac), encoding=0 (Roman)
            WriteU16(stream, 1);        // platformID
            WriteU16(stream, 0);        // encodingID
            WriteU32(stream, 12);       // offset to subtable

            // Format 0 subtable
            WriteU16(stream, 0);        // format
            WriteU16(stream, 262);      // length (6 header + 256 mapping)
            WriteU16(stream, 0);        // language

            // Identity mapping: glyph[i] = i (capped at numGlyphs)
            for (int i = 0; i < 256; i++)
            {
                stream.WriteByte((byte)i);
            }

            return stream.ToArray();
        }

        private static byte[] BuildSfnt(Dictionary<string, byte[]> tables)
        {
            int numTables = tables.Count;

            // Calculate searchRange, entrySelector, rangeShift
            int searchRange = 1;
            int entrySelector = 0;
            while (searchRange * 2 <= numTables)
            {
                searchRange *= 2;
                entrySelector++;
            }
            searchRange *= 16;
            int rangeShift = numTables * 16 - searchRange;

            using var stream = new MemoryStream();

            // SFNT header for CFF: 'OTTO'
            stream.Write(new byte[] { (byte)'O', (byte)'T', (byte)'T', (byte)'O' }, 0, 4);
            WriteU16(stream, (ushort)numTables);
            WriteU16(stream, (ushort)searchRange);
            WriteU16(stream, (ushort)entrySelector);
            WriteU16(stream, (ushort)rangeShift);

            // Calculate table offsets
            int headerSize = 12 + numTables * 16;
            int currentOffset = headerSize;

            var sortedTags = new List<string>(tables.Keys);
            sortedTags.Sort(StringComparer.Ordinal);

            // Table directory entries
            var offsets = new Dictionary<string, int>();
            foreach (string tag in sortedTags)
            {
                offsets[tag] = currentOffset;
                int dataLen = tables[tag].Length;
                int paddedLen = (dataLen + 3) & ~3; // 4-byte align
                currentOffset += paddedLen;
            }

            // Write table directory
            foreach (string tag in sortedTags)
            {
                byte[] tableData = tables[tag];
                // tag
                stream.Write(Encoding.ASCII.GetBytes(tag), 0, 4);
                // checksum
                WriteU32(stream, CalculateChecksum(tableData));
                // offset
                WriteU32(stream, (uint)offsets[tag]);
                // length
                WriteU32(stream, (uint)tableData.Length);
            }

            // Write table data (4-byte aligned)
            foreach (string tag in sortedTags)
            {
                byte[] tableData = tables[tag];
                stream.Write(tableData, 0, tableData.Length);
                // Pad to 4-byte boundary
                int padding = ((tableData.Length + 3) & ~3) - tableData.Length;
                for (int i = 0; i < padding; i++)
                {
                    stream.WriteByte(0);
                }
            }

            return stream.ToArray();
        }

        private static uint CalculateChecksum(byte[] data)
        {
            uint sum = 0;
            int length = (data.Length + 3) & ~3;
            for (int i = 0; i < length; i += 4)
            {
                uint val = 0;
                for (int j = 0; j < 4; j++)
                {
                    val <<= 8;
                    if (i + j < data.Length)
                    {
                        val |= data[i + j];
                    }
                }
                sum += val;
            }
            return sum;
        }

        private static void WriteU16(byte[] data, int offset, ushort value)
        {
            data[offset] = (byte)(value >> 8);
            data[offset + 1] = (byte)(value & 0xFF);
        }

        private static void WriteI16(byte[] data, int offset, short value)
        {
            data[offset] = (byte)((ushort)value >> 8);
            data[offset + 1] = (byte)(value & 0xFF);
        }

        private static void WriteU32(byte[] data, int offset, uint value)
        {
            data[offset] = (byte)(value >> 24);
            data[offset + 1] = (byte)(value >> 16);
            data[offset + 2] = (byte)(value >> 8);
            data[offset + 3] = (byte)(value & 0xFF);
        }

        private static void WriteU16(Stream stream, ushort value)
        {
            stream.WriteByte((byte)(value >> 8));
            stream.WriteByte((byte)(value & 0xFF));
        }

        private static void WriteU32(Stream stream, uint value)
        {
            stream.WriteByte((byte)(value >> 24));
            stream.WriteByte((byte)(value >> 16));
            stream.WriteByte((byte)(value >> 8));
            stream.WriteByte((byte)(value & 0xFF));
        }
    }
}
