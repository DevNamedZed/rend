#nullable enable
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Rend.Pdf.Parsing;

namespace Rend.PdfRendering
{
    internal sealed class PdfTextExtractor
    {
        private readonly PdfDocumentReader _reader;
        private readonly ContentStreamParser _parser = new ContentStreamParser();

        public PdfTextExtractor(PdfDocumentReader reader)
        {
            _reader = reader ?? throw new ArgumentNullException(nameof(reader));
        }

        public string ExtractText(int pageIndex)
        {
            var pageDict = _reader.Resolve(_reader.GetPage(pageIndex));
            if (pageDict.IsNull)
            {
                return "";
            }

            var contentBytes = GetPageContentBytes(pageDict);
            if (contentBytes.Length == 0)
            {
                return "";
            }

            var operators = _parser.Parse(contentBytes);
            return ExtractTextFromOperators(operators, pageDict);
        }

        private string ExtractTextFromOperators(List<ContentStreamOperator> operators, PdfObj pageDict)
        {
            var builder = new StringBuilder();
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
                        builder.Append('\n');
                        break;
                    case "Tf":
                        if (op.Operands.Count >= 1)
                        {
                            string fontName = ContentStreamParser.GetNameStr(op.Operands, 0);
                            ResolveFont(state, fontName, pageDict);
                        }
                        break;
                    case "Td":
                    case "TD":
                        if (op.Operands.Count >= 2)
                        {
                            float ty = ContentStreamParser.GetFloat(op.Operands, 1);
                            if (Math.Abs(ty) > 0.5f)
                            {
                                builder.Append('\n');
                            }
                            else if (builder.Length > 0 && builder[builder.Length - 1] != ' ' && builder[builder.Length - 1] != '\n')
                            {
                                builder.Append(' ');
                            }
                        }
                        break;
                    case "Tm":
                        if (builder.Length > 0 && builder[builder.Length - 1] != ' ' && builder[builder.Length - 1] != '\n')
                        {
                            builder.Append(' ');
                        }
                        break;
                    case "T*":
                        builder.Append('\n');
                        break;
                    case "Tj":
                        if (inText && op.Operands.Count > 0 && op.Operands[0] is byte[] tjBytes)
                        {
                            builder.Append(DecodeTextBytes(state, tjBytes));
                        }
                        break;
                    case "TJ":
                        if (inText && op.Operands.Count > 0 && op.Operands[0] is List<object> array)
                        {
                            foreach (var item in array)
                            {
                                if (item is byte[] textBytes)
                                {
                                    builder.Append(DecodeTextBytes(state, textBytes));
                                }
                                else if (item is double num && num < -100)
                                {
                                    builder.Append(' ');
                                }
                            }
                        }
                        break;
                    case "'":
                        builder.Append('\n');
                        if (inText && op.Operands.Count > 0 && op.Operands[0] is byte[] tickBytes)
                        {
                            builder.Append(DecodeTextBytes(state, tickBytes));
                        }
                        break;
                    case "\"":
                        builder.Append('\n');
                        if (inText && op.Operands.Count >= 3 && op.Operands[2] is byte[] dblBytes)
                        {
                            builder.Append(DecodeTextBytes(state, dblBytes));
                        }
                        break;
                }
            }

            string result = builder.ToString();
            while (result.Contains("\n\n\n"))
            {
                result = result.Replace("\n\n\n", "\n\n");
            }
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
            if (fonts.IsNull || string.IsNullOrEmpty(fontName))
            {
                return;
            }

            var fontDict = _reader.Resolve(fonts[fontName]);
            if (fontDict.IsNull)
            {
                return;
            }

            var fontType = _reader.Resolve(fontDict["Subtype"]).AsName();
            state.IsCIDFont = fontType == "Type0" || fontDict.ContainsKey("DescendantFonts");

            var toUnicode = _reader.Resolve(fontDict["ToUnicode"]);
            if (toUnicode.IsStream)
            {
                var cmapData = _reader.GetStreamBytes(toUnicode);
                if (cmapData != null)
                {
                    state.ToUnicodeMap = PdfFontResolver.ParseToUnicodeCMap(cmapData);
                }
            }

            state.Encoding = ResolveEncoding(fontDict);
        }

        private string DecodeTextBytes(TextState state, byte[] bytes)
        {
            bool isTwoByte = state.IsCIDFont ||
                             (state.ToUnicodeMap != null && IsTwoByteEncoding(state.ToUnicodeMap));

            if (state.ToUnicodeMap != null && state.ToUnicodeMap.Count > 0)
            {
                var builder = new StringBuilder();
                if (isTwoByte && bytes.Length >= 2)
                {
                    for (int i = 0; i + 1 < bytes.Length; i += 2)
                    {
                        int code = (bytes[i] << 8) | bytes[i + 1];
                        if (state.ToUnicodeMap.TryGetValue(code, out string? mapped))
                        {
                            builder.Append(mapped);
                        }
                        else
                        {
                            builder.Append((char)code);
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < bytes.Length; i++)
                    {
                        if (state.ToUnicodeMap.TryGetValue(bytes[i], out string? mapped))
                        {
                            builder.Append(mapped);
                        }
                        else
                        {
                            builder.Append((char)bytes[i]);
                        }
                    }
                }
                return builder.ToString();
            }

            if (isTwoByte)
            {
                var builder = new StringBuilder();
                for (int i = 0; i + 1 < bytes.Length; i += 2)
                {
                    int code = (bytes[i] << 8) | bytes[i + 1];
                    builder.Append((char)code);
                }
                return builder.ToString();
            }

            if (state.Encoding != null)
            {
                var builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    if (state.Encoding.TryGetValue(bytes[i], out string? mapped))
                    {
                        builder.Append(mapped);
                    }
                    else
                    {
                        builder.Append((char)bytes[i]);
                    }
                }
                return builder.ToString();
            }

            {
                var builder = new StringBuilder(bytes.Length);
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append((char)bytes[i]);
                }
                return builder.ToString();
            }
        }

        private bool IsTwoByteEncoding(Dictionary<int, string> map)
        {
            foreach (var key in map.Keys)
            {
                if (key > 255)
                {
                    return true;
                }
            }
            return false;
        }

        private Dictionary<int, string>? ResolveEncoding(PdfObj fontDict)
        {
            var encoding = _reader.Resolve(fontDict["Encoding"]);
            if (encoding.IsNull)
            {
                return null;
            }

            if (encoding.IsDict && encoding.ContainsKey("Differences"))
            {
                var diffs = _reader.Resolve(encoding["Differences"]);
                if (diffs.IsArray)
                {
                    return ParseEncodingDifferences(diffs);
                }
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
                if (item.IsInt || item.IsReal)
                {
                    code = (int)item.AsInt();
                }
                else if (item.IsName)
                {
                    string glyphName = item.AsName();
                    if (glyphName.StartsWith("/"))
                    {
                        glyphName = glyphName.Substring(1);
                    }
                    string? unicode = PdfFontResolver.GlyphNameToUnicode(glyphName);
                    if (unicode != null)
                    {
                        map[code] = unicode;
                    }
                    code++;
                }
            }
            return map;
        }

        private byte[] GetPageContentBytes(PdfObj pageDict)
        {
            var contents = _reader.Resolve(pageDict["Contents"]);
            if (contents.IsNull)
            {
                return Array.Empty<byte>();
            }

            if (contents.IsArray)
            {
                using var memoryStream = new System.IO.MemoryStream();
                for (int i = 0; i < contents.Count; i++)
                {
                    var streamObj = _reader.Resolve(contents[i]);
                    if (streamObj.IsStream)
                    {
                        var bytes = _reader.GetStreamBytes(streamObj);
                        if (bytes != null && bytes.Length > 0)
                        {
                            memoryStream.Write(bytes, 0, bytes.Length);
                            memoryStream.WriteByte((byte)'\n');
                        }
                    }
                }
                return memoryStream.ToArray();
            }

            if (contents.IsStream)
            {
                return _reader.GetStreamBytes(contents) ?? Array.Empty<byte>();
            }

            return Array.Empty<byte>();
        }

        private sealed class TextState
        {
            public string FontName = "";
            public Dictionary<int, string>? ToUnicodeMap;
            public Dictionary<int, string>? Encoding;
            public bool IsCIDFont;
        }
    }
}
