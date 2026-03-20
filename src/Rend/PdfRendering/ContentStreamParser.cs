#nullable enable
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Rend.PdfRendering
{
    internal sealed class ContentStreamParser
    {
        public List<ContentStreamOperator> Parse(byte[] data)
        {
            var result = new List<ContentStreamOperator>();
            var operands = new List<object>();
            int pos = 0;

            while (pos < data.Length)
            {
                SkipWhitespace(data, ref pos);
                if (pos >= data.Length)
                {
                    break;
                }

                byte ch = data[pos];

                if (ch == (byte)'%')
                {
                    while (pos < data.Length && data[pos] != (byte)'\n' && data[pos] != (byte)'\r')
                    {
                        pos++;
                    }
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
                        operands.Add(ReadInlineDict(data, ref pos));
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
                    {
                        operands.Add(num);
                    }
                    else
                    {
                        result.Add(new ContentStreamOperator { Name = token, Operands = new List<object>(operands) });
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
                        var inlineImage = ParseInlineImage(data, ref pos);
                        if (inlineImage != null)
                        {
                            result.Add(new ContentStreamOperator { Name = "BI_IMAGE", Operands = new List<object> { inlineImage } });
                        }
                        continue;
                    }

                    result.Add(new ContentStreamOperator { Name = token, Operands = new List<object>(operands) });
                    operands.Clear();
                    continue;
                }

                pos++;
            }

            return result;
        }

        private static void SkipWhitespace(byte[] data, ref int pos)
        {
            while (pos < data.Length)
            {
                byte c = data[pos];
                if (c == (byte)' ' || c == (byte)'\t' || c == (byte)'\n' || c == (byte)'\r' || c == 0 || c == 12)
                {
                    pos++;
                }
                else
                {
                    break;
                }
            }
        }

        private static bool IsDigit(byte c) => c >= (byte)'0' && c <= (byte)'9';
        private static bool IsAlpha(byte c) => (c >= (byte)'a' && c <= (byte)'z') || (c >= (byte)'A' && c <= (byte)'Z');

        private static bool IsDelimiter(byte c) =>
            c == (byte)'(' || c == (byte)')' || c == (byte)'<' || c == (byte)'>' ||
            c == (byte)'[' || c == (byte)']' || c == (byte)'{' || c == (byte)'}' ||
            c == (byte)'/' || c == (byte)'%';

        private static bool IsWhitespace(byte c) =>
            c == (byte)' ' || c == (byte)'\t' || c == (byte)'\n' || c == (byte)'\r' || c == 0 || c == 12;

        private static string ReadToken(byte[] data, ref int pos)
        {
            int start = pos;
            while (pos < data.Length && !IsWhitespace(data[pos]) && !IsDelimiter(data[pos]))
            {
                pos++;
            }
            return Encoding.ASCII.GetString(data, start, pos - start);
        }

        internal static byte[] ReadStringLiteral(byte[] data, ref int pos)
        {
            pos++;
            var result = new List<byte>();
            int depth = 1;
            while (pos < data.Length && depth > 0)
            {
                byte c = data[pos];
                if (c == (byte)'\\' && pos + 1 < data.Length)
                {
                    pos++;
                    byte next = data[pos];
                    switch (next)
                    {
                        case (byte)'n': result.Add((byte)'\n'); break;
                        case (byte)'r': result.Add((byte)'\r'); break;
                        case (byte)'t': result.Add((byte)'\t'); break;
                        case (byte)'b': result.Add((byte)'\b'); break;
                        case (byte)'f': result.Add(12); break;
                        case (byte)'(': result.Add((byte)'('); break;
                        case (byte)')': result.Add((byte)')'); break;
                        case (byte)'\\': result.Add((byte)'\\'); break;
                        default:
                            if (next >= (byte)'0' && next <= (byte)'7')
                            {
                                int octal = next - (byte)'0';
                                for (int i = 0; i < 2 && pos + 1 < data.Length; i++)
                                {
                                    byte d = data[pos + 1];
                                    if (d >= (byte)'0' && d <= (byte)'7')
                                    {
                                        octal = octal * 8 + (d - (byte)'0');
                                        pos++;
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                                result.Add((byte)(octal & 0xFF));
                            }
                            else
                            {
                                result.Add(next);
                            }
                            break;
                    }
                }
                else if (c == (byte)'(')
                {
                    depth++;
                    result.Add(c);
                }
                else if (c == (byte)')')
                {
                    depth--;
                    if (depth > 0)
                    {
                        result.Add(c);
                    }
                }
                else
                {
                    result.Add(c);
                }
                pos++;
            }
            return result.ToArray();
        }

        internal static byte[] ReadHexString(byte[] data, ref int pos)
        {
            pos++;
            var hex = new StringBuilder();
            while (pos < data.Length && data[pos] != (byte)'>')
            {
                byte c = data[pos];
                if ((c >= (byte)'0' && c <= (byte)'9') ||
                    (c >= (byte)'a' && c <= (byte)'f') ||
                    (c >= (byte)'A' && c <= (byte)'F'))
                {
                    hex.Append((char)c);
                }
                pos++;
            }
            if (pos < data.Length)
            {
                pos++;
            }

            string hexStr = hex.ToString();
            if (hexStr.Length % 2 != 0)
            {
                hexStr += "0";
            }
            var result = new byte[hexStr.Length / 2];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = byte.Parse(hexStr.Substring(i * 2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            }
            return result;
        }

        internal static List<object> ReadArray(byte[] data, ref int pos)
        {
            pos++;
            var items = new List<object>();
            while (pos < data.Length)
            {
                SkipWhitespace(data, ref pos);
                if (pos >= data.Length)
                {
                    break;
                }
                if (data[pos] == (byte)']') { pos++; break; }

                byte ch = data[pos];
                if (ch == (byte)'(')
                {
                    items.Add(ReadStringLiteral(data, ref pos));
                }
                else if (ch == (byte)'<')
                {
                    if (pos + 1 < data.Length && data[pos + 1] == (byte)'<')
                    {
                        items.Add(ReadInlineDict(data, ref pos));
                    }
                    else
                    {
                        items.Add(ReadHexString(data, ref pos));
                    }
                }
                else if (ch == (byte)'/')
                {
                    items.Add(ReadName(data, ref pos));
                }
                else if (ch == (byte)'[')
                {
                    items.Add(ReadArray(data, ref pos));
                }
                else
                {
                    var token = ReadToken(data, ref pos);
                    if (double.TryParse(token, NumberStyles.Float, CultureInfo.InvariantCulture, out double num))
                    {
                        items.Add(num);
                    }
                    else
                    {
                        items.Add(token);
                    }
                }
            }
            return items;
        }

        internal static string ReadName(byte[] data, ref int pos)
        {
            pos++;
            var nameBuilder = new StringBuilder();
            while (pos < data.Length && !IsWhitespace(data[pos]) && !IsDelimiter(data[pos]))
            {
                if (data[pos] == (byte)'#' && pos + 2 < data.Length)
                {
                    string hexChars = "" + (char)data[pos + 1] + (char)data[pos + 2];
                    if (byte.TryParse(hexChars, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out byte decoded))
                    {
                        nameBuilder.Append((char)decoded);
                        pos += 3;
                        continue;
                    }
                }
                nameBuilder.Append((char)data[pos]);
                pos++;
            }
            return "/" + nameBuilder.ToString();
        }

        private static object ReadInlineDict(byte[] data, ref int pos)
        {
            pos += 2;
            int depth = 1;
            while (pos < data.Length && depth > 0)
            {
                if (pos + 1 < data.Length && data[pos] == (byte)'<' && data[pos + 1] == (byte)'<')
                {
                    depth++;
                    pos += 2;
                }
                else if (pos + 1 < data.Length && data[pos] == (byte)'>' && data[pos + 1] == (byte)'>')
                {
                    depth--;
                    pos += 2;
                }
                else
                {
                    pos++;
                }
            }
            return "<<dict>>";
        }

        private static InlineImageData? ParseInlineImage(byte[] data, ref int pos)
        {
            var img = new InlineImageData();

            SkipWhitespace(data, ref pos);
            while (pos < data.Length)
            {
                SkipWhitespace(data, ref pos);
                if (pos >= data.Length)
                {
                    break;
                }

                if (data[pos] == (byte)'I' && pos + 1 < data.Length && data[pos + 1] == (byte)'D' &&
                    (pos + 2 >= data.Length || IsWhitespace(data[pos + 2])))
                {
                    pos += 2;
                    if (pos < data.Length && (data[pos] == ' ' || data[pos] == '\n' || data[pos] == '\r'))
                    {
                        pos++;
                    }
                    break;
                }

                string key;
                if (data[pos] == (byte)'/')
                {
                    key = ReadName(data, ref pos);
                }
                else
                {
                    key = ReadToken(data, ref pos);
                }
                SkipWhitespace(data, ref pos);

                if (pos >= data.Length)
                {
                    break;
                }
                string value;
                if (data[pos] == (byte)'/')
                {
                    value = ReadName(data, ref pos);
                }
                else if (data[pos] == (byte)'[')
                {
                    ReadArray(data, ref pos);
                    continue;
                }
                else
                {
                    value = ReadToken(data, ref pos);
                }

                switch (key)
                {
                    case "/W": case "/Width": img.Width = int.TryParse(value, out int w) ? w : 0; break;
                    case "/H": case "/Height": img.Height = int.TryParse(value, out int h) ? h : 0; break;
                    case "/BPC": case "/BitsPerComponent": img.BitsPerComponent = int.TryParse(value, out int bpc) ? bpc : 8; break;
                    case "/CS": case "/ColorSpace":
                        value = value.TrimStart('/');
                        if (value == "G" || value == "DeviceGray") { img.ColorSpace = "DeviceGray"; }
                        else if (value == "RGB" || value == "DeviceRGB") { img.ColorSpace = "DeviceRGB"; }
                        else if (value == "CMYK" || value == "DeviceCMYK") { img.ColorSpace = "DeviceCMYK"; }
                        else { img.ColorSpace = value; }
                        break;
                    case "/F": case "/Filter":
                        value = value.TrimStart('/');
                        if (value == "AHx" || value == "ASCIIHexDecode") { img.Filter = "ASCIIHexDecode"; }
                        else if (value == "A85" || value == "ASCII85Decode") { img.Filter = "ASCII85Decode"; }
                        else if (value == "Fl" || value == "FlateDecode") { img.Filter = "FlateDecode"; }
                        else if (value == "DCT" || value == "DCTDecode") { img.Filter = "DCTDecode"; }
                        else { img.Filter = value; }
                        break;
                }
            }

            int dataStart = pos;

            // For uncompressed images, compute expected byte count and skip directly
            int expectedBytes = ComputeInlineImageByteCount(img);
            if (expectedBytes > 0 && string.IsNullOrEmpty(img.Filter))
            {
                int dataEnd = dataStart + expectedBytes;
                if (dataEnd <= data.Length)
                {
                    img.Data = new byte[expectedBytes];
                    Array.Copy(data, dataStart, img.Data, 0, expectedBytes);
                    pos = dataEnd;
                    // Skip whitespace + EI
                    while (pos < data.Length && IsWhitespace(data[pos]))
                    {
                        pos++;
                    }
                    if (pos + 1 < data.Length && data[pos] == (byte)'E' && data[pos + 1] == (byte)'I')
                    {
                        pos += 2;
                    }
                    return img;
                }
            }

            // Fallback: scan for EI marker (required for compressed images)
            while (pos < data.Length)
            {
                if (data[pos] == (byte)'E' && pos + 1 < data.Length && data[pos + 1] == (byte)'I')
                {
                    if (pos + 2 >= data.Length || IsWhitespace(data[pos + 2]))
                    {
                        if (pos > 0 && IsWhitespace(data[pos - 1]))
                        {
                            int dataEnd = pos - 1;
                            img.Data = new byte[dataEnd - dataStart];
                            Array.Copy(data, dataStart, img.Data, 0, img.Data.Length);
                            pos += 2;
                            return img;
                        }
                    }
                }
                pos++;
            }

            return null;
        }

        internal static float GetFloat(List<object> args, int index)
        {
            if (index >= args.Count)
            {
                return 0f;
            }
            if (args[index] is double d)
            {
                return (float)d;
            }
            if (args[index] is float f)
            {
                return f;
            }
            if (args[index] is int i)
            {
                return i;
            }
            return 0f;
        }

        internal static string GetNameStr(List<object> args, int index)
        {
            if (index >= args.Count)
            {
                return "";
            }
            if (args[index] is string s)
            {
                if (s.StartsWith("/"))
                {
                    return s.Substring(1);
                }
                return s;
            }
            return "";
        }

        private static int ComputeInlineImageByteCount(InlineImageData img)
        {
            if (img.Width <= 0 || img.Height <= 0 || img.BitsPerComponent <= 0)
            {
                return 0;
            }

            int components;
            switch (img.ColorSpace)
            {
                case "DeviceGray": components = 1; break;
                case "DeviceRGB": components = 3; break;
                case "DeviceCMYK": components = 4; break;
                default: return 0;
            }

            int bitsPerRow = img.Width * components * img.BitsPerComponent;
            int bytesPerRow = (bitsPerRow + 7) / 8;
            return bytesPerRow * img.Height;
        }
    }
}
