#nullable enable
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Rend.Pdf.Reading;

namespace Rend.PdfRendering
{
    /// <summary>
    /// Extracts text content from PDF pages by interpreting the content stream.
    /// </summary>
    internal sealed class PdfTextExtractor
    {
        private readonly PdfDocumentReader _reader;

        public PdfTextExtractor(PdfDocumentReader reader)
        {
            _reader = reader ?? throw new ArgumentNullException(nameof(reader));
        }

        public string ExtractText(int pageIndex)
        {
            var pageDict = _reader.Resolve(_reader.GetPage(pageIndex));
            if (pageDict.IsNull) return "";

            var contentBytes = GetPageContentBytes(pageDict);
            if (contentBytes.Length == 0) return "";

            var operators = ParseContentStream(contentBytes);
            return ExtractTextFromOperators(operators, pageDict);
        }

        private string ExtractTextFromOperators(List<PdfOperator> operators, PdfObj pageDict)
        {
            var sb = new StringBuilder();
            var state = new TextState();
            bool inText = false;

            foreach (var op in operators)
            {
                switch (op.Name)
                {
                    case "BT":
                        inText = true;
                        break;
                    case "ET":
                        inText = false;
                        sb.Append('\n');
                        break;
                    case "Tf":
                        if (op.Operands.Count >= 1)
                        {
                            string fontName = GetNameStr(op.Operands, 0);
                            ResolveFont(state, fontName, pageDict);
                        }
                        break;
                    case "Td":
                    case "TD":
                        if (op.Operands.Count >= 2)
                        {
                            float ty = GetFloat(op.Operands, 1);
                            // Significant Y movement suggests a new line
                            if (Math.Abs(ty) > 0.5f)
                                sb.Append('\n');
                            else if (sb.Length > 0 && sb[sb.Length - 1] != ' ' && sb[sb.Length - 1] != '\n')
                                sb.Append(' ');
                        }
                        break;
                    case "Tm":
                        // New text matrix often means repositioning
                        if (sb.Length > 0 && sb[sb.Length - 1] != ' ' && sb[sb.Length - 1] != '\n')
                            sb.Append(' ');
                        break;
                    case "T*":
                        sb.Append('\n');
                        break;
                    case "Tj":
                        if (inText && op.Operands.Count > 0 && op.Operands[0] is byte[] tjBytes)
                        {
                            sb.Append(DecodeTextBytes(state, tjBytes));
                        }
                        break;
                    case "TJ":
                        if (inText && op.Operands.Count > 0 && op.Operands[0] is List<object> array)
                        {
                            foreach (var item in array)
                            {
                                if (item is byte[] textBytes)
                                {
                                    sb.Append(DecodeTextBytes(state, textBytes));
                                }
                                else if (item is double num && num < -100)
                                {
                                    // Large negative adjustment = word break
                                    sb.Append(' ');
                                }
                            }
                        }
                        break;
                    case "'":
                        sb.Append('\n');
                        if (inText && op.Operands.Count > 0 && op.Operands[0] is byte[] tickBytes)
                        {
                            sb.Append(DecodeTextBytes(state, tickBytes));
                        }
                        break;
                    case "\"":
                        sb.Append('\n');
                        if (inText && op.Operands.Count >= 3 && op.Operands[2] is byte[] dblBytes)
                        {
                            sb.Append(DecodeTextBytes(state, dblBytes));
                        }
                        break;
                }
            }

            // Clean up: collapse multiple newlines
            string result = sb.ToString();
            while (result.Contains("\n\n\n"))
                result = result.Replace("\n\n\n", "\n\n");
            return result.Trim();
        }

        private void ResolveFont(TextState state, string fontName, PdfObj pageDict)
        {
            state.FontName = fontName;
            state.ToUnicodeMap = null;
            state.Encoding = null;
            state.IsCIDFont = false;

            var resources = _reader.Resolve(pageDict["Resources"]);
            var fonts = _reader.Resolve(resources["Font"]);
            if (fonts.IsNull || string.IsNullOrEmpty(fontName)) return;

            var fontDict = _reader.Resolve(fonts[fontName]);
            if (fontDict.IsNull) return;

            var fontType = _reader.Resolve(fontDict["Subtype"]).AsName();
            state.IsCIDFont = fontType == "Type0" || fontDict.ContainsKey("DescendantFonts");

            // Parse ToUnicode CMap
            var toUnicode = _reader.Resolve(fontDict["ToUnicode"]);
            if (toUnicode.IsStream)
            {
                var cmapData = _reader.GetStreamBytes(toUnicode);
                if (cmapData != null)
                    state.ToUnicodeMap = ParseToUnicodeCMap(cmapData);
            }

            // Parse encoding differences
            state.Encoding = ResolveEncoding(fontDict);
        }

        private string DecodeTextBytes(TextState state, byte[] bytes)
        {
            bool isTwoByte = state.IsCIDFont ||
                             (state.ToUnicodeMap != null && IsTwoByteEncoding(state.ToUnicodeMap));

            if (state.ToUnicodeMap != null && state.ToUnicodeMap.Count > 0)
            {
                var sb = new StringBuilder();
                if (isTwoByte && bytes.Length >= 2)
                {
                    for (int i = 0; i + 1 < bytes.Length; i += 2)
                    {
                        int code = (bytes[i] << 8) | bytes[i + 1];
                        if (state.ToUnicodeMap.TryGetValue(code, out string? mapped))
                            sb.Append(mapped);
                        else
                            sb.Append((char)code);
                    }
                }
                else
                {
                    for (int i = 0; i < bytes.Length; i++)
                    {
                        if (state.ToUnicodeMap.TryGetValue(bytes[i], out string? mapped))
                            sb.Append(mapped);
                        else
                            sb.Append((char)bytes[i]);
                    }
                }
                return sb.ToString();
            }

            if (isTwoByte)
            {
                var sb = new StringBuilder();
                for (int i = 0; i + 1 < bytes.Length; i += 2)
                {
                    int code = (bytes[i] << 8) | bytes[i + 1];
                    sb.Append((char)code);
                }
                return sb.ToString();
            }

            if (state.Encoding != null)
            {
                var sb = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    if (state.Encoding.TryGetValue(bytes[i], out string? mapped))
                        sb.Append(mapped);
                    else
                        sb.Append((char)bytes[i]);
                }
                return sb.ToString();
            }

            {
                var sb = new StringBuilder(bytes.Length);
                for (int i = 0; i < bytes.Length; i++)
                    sb.Append((char)bytes[i]);
                return sb.ToString();
            }
        }

        private bool IsTwoByteEncoding(Dictionary<int, string> map)
        {
            foreach (var key in map.Keys)
            {
                if (key > 255) return true;
            }
            return false;
        }

        private Dictionary<int, string>? ResolveEncoding(PdfObj fontDict)
        {
            var encoding = _reader.Resolve(fontDict["Encoding"]);
            if (encoding.IsNull) return null;

            if (encoding.IsDict && encoding.ContainsKey("Differences"))
            {
                var diffs = _reader.Resolve(encoding["Differences"]);
                if (diffs.IsArray)
                    return ParseEncodingDifferences(diffs);
            }

            return null;
        }

        private Dictionary<int, string> ParseEncodingDifferences(PdfObj diffsArray)
        {
            var map = new Dictionary<int, string>();
            int code = 0;
            for (int i = 0; i < diffsArray.Count; i++)
            {
                var item = _reader.Resolve(diffsArray[i]);
                if (item.AsInt() != 0 || item.AsName() == "")
                {
                    code = (int)item.AsInt();
                }
                else
                {
                    string glyphName = item.AsName();
                    if (glyphName.StartsWith("/")) glyphName = glyphName.Substring(1);
                    // Simple name → char mapping
                    if (glyphName.Length == 1)
                        map[code] = glyphName;
                    else if (glyphName.StartsWith("uni") && glyphName.Length == 7 &&
                        int.TryParse(glyphName.Substring(3), NumberStyles.HexNumber,
                            CultureInfo.InvariantCulture, out int cp))
                        map[code] = char.ConvertFromUtf32(cp);
                    code++;
                }
            }
            return map;
        }

        #region Content Stream Parser (shared with PdfPageRenderer)

        private struct PdfOperator
        {
            public string Name;
            public List<object> Operands;
        }

        private byte[] GetPageContentBytes(PdfObj pageDict)
        {
            var contents = _reader.Resolve(pageDict["Contents"]);
            if (contents.IsNull) return Array.Empty<byte>();

            if (contents.IsArray)
            {
                using var ms = new System.IO.MemoryStream();
                for (int i = 0; i < contents.Count; i++)
                {
                    var streamObj = _reader.Resolve(contents[i]);
                    if (streamObj.IsStream)
                    {
                        var bytes = _reader.GetStreamBytes(streamObj);
                        if (bytes != null && bytes.Length > 0)
                        {
                            ms.Write(bytes, 0, bytes.Length);
                            ms.WriteByte((byte)'\n');
                        }
                    }
                }
                return ms.ToArray();
            }

            if (contents.IsStream)
                return _reader.GetStreamBytes(contents) ?? Array.Empty<byte>();

            return Array.Empty<byte>();
        }

        private List<PdfOperator> ParseContentStream(byte[] data)
        {
            var result = new List<PdfOperator>();
            var operands = new List<object>();
            int pos = 0;

            while (pos < data.Length)
            {
                SkipWhitespace(data, ref pos);
                if (pos >= data.Length) break;

                byte ch = data[pos];

                if (ch == (byte)'%')
                {
                    while (pos < data.Length && data[pos] != (byte)'\n' && data[pos] != (byte)'\r')
                        pos++;
                    continue;
                }

                if (ch == (byte)'(')
                {
                    operands.Add(ReadStringLiteral(data, ref pos));
                    continue;
                }

                if (ch == (byte)'<')
                {
                    if (pos + 1 < data.Length && data[pos + 1] == (byte)'<')
                    {
                        SkipInlineDict(data, ref pos);
                        continue;
                    }
                    operands.Add(ReadHexString(data, ref pos));
                    continue;
                }

                if (ch == (byte)'[')
                {
                    operands.Add(ReadArray(data, ref pos));
                    continue;
                }

                if (ch == (byte)'/')
                {
                    operands.Add(ReadName(data, ref pos));
                    continue;
                }

                if (IsDigit(ch) || ch == (byte)'+' || ch == (byte)'-' || ch == (byte)'.')
                {
                    var token = ReadToken(data, ref pos);
                    if (double.TryParse(token, NumberStyles.Float, CultureInfo.InvariantCulture, out double num))
                        operands.Add(num);
                    else
                    {
                        result.Add(new PdfOperator { Name = token, Operands = new List<object>(operands) });
                        operands.Clear();
                    }
                    continue;
                }

                if (IsAlpha(ch) || ch == (byte)'*' || ch == (byte)'\'')
                {
                    var token = ReadToken(data, ref pos);
                    if (token == "true") { operands.Add(true); continue; }
                    if (token == "false") { operands.Add(false); continue; }
                    if (token == "null") { operands.Add(null!); continue; }

                    if (token == "BI")
                    {
                        SkipInlineImage(data, ref pos);
                        continue;
                    }

                    result.Add(new PdfOperator { Name = token, Operands = new List<object>(operands) });
                    operands.Clear();
                    continue;
                }

                pos++;
            }

            return result;
        }

        private void SkipWhitespace(byte[] data, ref int pos)
        {
            while (pos < data.Length)
            {
                byte c = data[pos];
                if (c == ' ' || c == '\t' || c == '\n' || c == '\r' || c == 0 || c == 12)
                    pos++;
                else break;
            }
        }

        private bool IsDigit(byte c) => c >= '0' && c <= '9';
        private bool IsAlpha(byte c) => (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z');
        private bool IsWhitespace(byte c) => c == ' ' || c == '\t' || c == '\n' || c == '\r' || c == 0 || c == 12;
        private bool IsDelimiter(byte c) =>
            c == '(' || c == ')' || c == '<' || c == '>' ||
            c == '[' || c == ']' || c == '{' || c == '}' || c == '/' || c == '%';

        private string ReadToken(byte[] data, ref int pos)
        {
            int start = pos;
            while (pos < data.Length && !IsWhitespace(data[pos]) && !IsDelimiter(data[pos]))
                pos++;
            return Encoding.ASCII.GetString(data, start, pos - start);
        }

        private byte[] ReadStringLiteral(byte[] data, ref int pos)
        {
            pos++;
            var result = new List<byte>();
            int depth = 1;
            while (pos < data.Length && depth > 0)
            {
                byte c = data[pos];
                if (c == '\\' && pos + 1 < data.Length)
                {
                    pos++;
                    byte next = data[pos];
                    switch (next)
                    {
                        case (byte)'n': result.Add((byte)'\n'); break;
                        case (byte)'r': result.Add((byte)'\r'); break;
                        case (byte)'t': result.Add((byte)'\t'); break;
                        case (byte)'(': result.Add((byte)'('); break;
                        case (byte)')': result.Add((byte)')'); break;
                        case (byte)'\\': result.Add((byte)'\\'); break;
                        default:
                            if (next >= '0' && next <= '7')
                            {
                                int octal = next - '0';
                                for (int i = 0; i < 2 && pos + 1 < data.Length; i++)
                                {
                                    byte d = data[pos + 1];
                                    if (d >= '0' && d <= '7') { octal = octal * 8 + (d - '0'); pos++; }
                                    else break;
                                }
                                result.Add((byte)(octal & 0xFF));
                            }
                            else result.Add(next);
                            break;
                    }
                }
                else if (c == '(') { depth++; result.Add(c); }
                else if (c == ')') { depth--; if (depth > 0) result.Add(c); }
                else result.Add(c);
                pos++;
            }
            return result.ToArray();
        }

        private byte[] ReadHexString(byte[] data, ref int pos)
        {
            pos++;
            var hex = new StringBuilder();
            while (pos < data.Length && data[pos] != '>')
            {
                byte c = data[pos];
                if ((c >= '0' && c <= '9') || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F'))
                    hex.Append((char)c);
                pos++;
            }
            if (pos < data.Length) pos++;

            string hexStr = hex.ToString();
            if (hexStr.Length % 2 != 0) hexStr += "0";
            var result = new byte[hexStr.Length / 2];
            for (int i = 0; i < result.Length; i++)
                result[i] = byte.Parse(hexStr.Substring(i * 2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            return result;
        }

        private List<object> ReadArray(byte[] data, ref int pos)
        {
            pos++;
            var items = new List<object>();
            while (pos < data.Length)
            {
                SkipWhitespace(data, ref pos);
                if (pos >= data.Length) break;
                if (data[pos] == ']') { pos++; break; }

                byte ch = data[pos];
                if (ch == '(') items.Add(ReadStringLiteral(data, ref pos));
                else if (ch == '<') { if (pos + 1 < data.Length && data[pos + 1] == '<') { SkipInlineDict(data, ref pos); } else items.Add(ReadHexString(data, ref pos)); }
                else if (ch == '/') items.Add(ReadName(data, ref pos));
                else if (ch == '[') items.Add(ReadArray(data, ref pos));
                else
                {
                    var token = ReadToken(data, ref pos);
                    if (double.TryParse(token, NumberStyles.Float, CultureInfo.InvariantCulture, out double num))
                        items.Add(num);
                    else items.Add(token);
                }
            }
            return items;
        }

        private string ReadName(byte[] data, ref int pos)
        {
            pos++;
            var sb = new StringBuilder();
            while (pos < data.Length && !IsWhitespace(data[pos]) && !IsDelimiter(data[pos]))
            {
                sb.Append((char)data[pos]);
                pos++;
            }
            return "/" + sb.ToString();
        }

        private void SkipInlineDict(byte[] data, ref int pos)
        {
            pos += 2;
            int depth = 1;
            while (pos < data.Length && depth > 0)
            {
                if (pos + 1 < data.Length && data[pos] == '<' && data[pos + 1] == '<') { depth++; pos += 2; }
                else if (pos + 1 < data.Length && data[pos] == '>' && data[pos + 1] == '>') { depth--; pos += 2; }
                else pos++;
            }
        }

        private void SkipInlineImage(byte[] data, ref int pos)
        {
            while (pos < data.Length)
            {
                if (data[pos] == 'E' && pos + 1 < data.Length && data[pos + 1] == 'I')
                {
                    if (pos + 2 >= data.Length || IsWhitespace(data[pos + 2]))
                    {
                        if (pos > 0 && IsWhitespace(data[pos - 1]))
                        {
                            pos += 2;
                            return;
                        }
                    }
                }
                pos++;
            }
        }

        private Dictionary<int, string> ParseToUnicodeCMap(byte[] data)
        {
            var map = new Dictionary<int, string>();
            string text = Encoding.ASCII.GetString(data);
            int pos = 0;

            while (pos < text.Length)
            {
                int bfcharStart = text.IndexOf("beginbfchar", pos, StringComparison.Ordinal);
                int bfrangeStart = text.IndexOf("beginbfrange", pos, StringComparison.Ordinal);

                int nextSection;
                bool isRange;

                if (bfcharStart < 0 && bfrangeStart < 0) break;

                if (bfcharStart >= 0 && (bfrangeStart < 0 || bfcharStart < bfrangeStart))
                {
                    nextSection = bfcharStart + "beginbfchar".Length;
                    isRange = false;
                }
                else
                {
                    nextSection = bfrangeStart + "beginbfrange".Length;
                    isRange = true;
                }

                string endMarker = isRange ? "endbfrange" : "endbfchar";
                int endPos = text.IndexOf(endMarker, nextSection, StringComparison.Ordinal);
                if (endPos < 0) { pos = nextSection; continue; }

                string section = text.Substring(nextSection, endPos - nextSection);
                var hexValues = ExtractHexValues(section);

                if (isRange)
                {
                    for (int i = 0; i + 2 < hexValues.Count; i += 3)
                    {
                        int start = hexValues[i];
                        int end = hexValues[i + 1];
                        int dst = hexValues[i + 2];
                        for (int c = start; c <= end; c++)
                            map[c] = char.ConvertFromUtf32(dst + (c - start));
                    }
                }
                else
                {
                    for (int i = 0; i + 1 < hexValues.Count; i += 2)
                        map[hexValues[i]] = char.ConvertFromUtf32(hexValues[i + 1]);
                }

                pos = endPos + endMarker.Length;
            }

            return map;
        }

        private List<int> ExtractHexValues(string section)
        {
            var result = new List<int>();
            int i = 0;
            while (i < section.Length)
            {
                int open = section.IndexOf('<', i);
                if (open < 0) break;
                int close = section.IndexOf('>', open);
                if (close < 0) break;

                string hex = section.Substring(open + 1, close - open - 1).Trim();
                if (hex.Length > 0 && int.TryParse(hex, NumberStyles.HexNumber,
                    CultureInfo.InvariantCulture, out int parsed))
                    result.Add(parsed);

                i = close + 1;
            }
            return result;
        }

        private static float GetFloat(List<object> args, int index)
        {
            if (index >= args.Count) return 0f;
            if (args[index] is double d) return (float)d;
            return 0f;
        }

        private static string GetNameStr(List<object> args, int index)
        {
            if (index >= args.Count) return "";
            if (args[index] is string s) return s.StartsWith("/") ? s.Substring(1) : s;
            return "";
        }

        #endregion

        private sealed class TextState
        {
            public string FontName = "";
            public Dictionary<int, string>? ToUnicodeMap;
            public Dictionary<int, string>? Encoding;
            public bool IsCIDFont;
        }
    }
}
