#nullable enable
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Rend.Pdf.Reading;
using SkiaSharp;

namespace Rend.PdfRendering
{
    internal sealed class PdfPageRenderer
    {
        private readonly PdfDocumentReader _reader;
        private readonly Dictionary<int, SKTypeface> _typefaceCache = new Dictionary<int, SKTypeface>();

        public PdfPageRenderer(PdfDocumentReader reader)
        {
            _reader = reader ?? throw new ArgumentNullException(nameof(reader));
        }

        public SKBitmap RenderPage(int pageIndex, float scale)
        {
            var pageDict = _reader.Resolve(_reader.GetPage(pageIndex));
            var mediaBox = GetPageMediaBox(pageDict);
            var cropBox = GetPageCropBox(pageDict, mediaBox);

            float pageWidth = cropBox.Right - cropBox.Left;
            float pageHeight = cropBox.Top - cropBox.Bottom;

            int pixelWidth = Math.Max(1, (int)(pageWidth * scale + 0.5f));
            int pixelHeight = Math.Max(1, (int)(pageHeight * scale + 0.5f));

            var bitmap = new SKBitmap(pixelWidth, pixelHeight, SKColorType.Rgba8888, SKAlphaType.Premul);
            using var canvas = new SKCanvas(bitmap);
            canvas.Clear(SKColors.White);

            // PDF coordinate system: origin at bottom-left, Y up
            // Skia coordinate system: origin at top-left, Y down
            canvas.Scale(scale, -scale);
            canvas.Translate(-cropBox.Left, -cropBox.Top);

            var state = new GraphicsState();
            var stateStack = new Stack<GraphicsState>();
            var path = new SKPath();
            bool pendingClipNonZero = false;
            bool pendingClipEvenOdd = false;

            var contentBytes = GetPageContentBytes(pageDict);
            var operators = ParseContentStream(contentBytes);

            foreach (var op in operators)
            {
                try
                {
                    ExecuteOperator(canvas, op, state, stateStack, path,
                        ref pendingClipNonZero, ref pendingClipEvenOdd, pageDict);
                }
                catch
                {
                    // Skip bad operators silently
                }
            }

            path.Dispose();
            return bitmap;
        }

        private PdfRect GetPageMediaBox(PdfObj pageDict)
        {
            var mb = _reader.Resolve(pageDict["MediaBox"]);
            if (!mb.IsNull)
                return PdfRect.FromArray(mb);

            // Walk up the page tree
            var parent = _reader.Resolve(pageDict["Parent"]);
            while (!parent.IsNull)
            {
                mb = _reader.Resolve(parent["MediaBox"]);
                if (!mb.IsNull)
                    return PdfRect.FromArray(mb);
                parent = _reader.Resolve(parent["Parent"]);
            }

            return new PdfRect(0, 0, 612, 792); // Letter
        }

        private PdfRect GetPageCropBox(PdfObj pageDict, PdfRect mediaBox)
        {
            var cb = _reader.Resolve(pageDict["CropBox"]);
            if (!cb.IsNull)
                return PdfRect.FromArray(cb);

            var parent = _reader.Resolve(pageDict["Parent"]);
            while (!parent.IsNull)
            {
                cb = _reader.Resolve(parent["CropBox"]);
                if (!cb.IsNull)
                    return PdfRect.FromArray(cb);
                parent = _reader.Resolve(parent["Parent"]);
            }

            return mediaBox;
        }

        private byte[] GetPageContentBytes(PdfObj pageDict)
        {
            var contents = _reader.Resolve(pageDict["Contents"]);
            if (contents.IsNull)
                return Array.Empty<byte>();

            if (contents.IsArray)
            {
                using var ms = new MemoryStream();
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
            {
                return _reader.GetStreamBytes(contents) ?? Array.Empty<byte>();
            }

            return Array.Empty<byte>();
        }

        #region Content Stream Parser

        private struct PdfOperator
        {
            public string Name;
            public List<object> Operands;
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

                // Comment
                if (ch == (byte)'%')
                {
                    while (pos < data.Length && data[pos] != (byte)'\n' && data[pos] != (byte)'\r')
                        pos++;
                    continue;
                }

                // String literal
                if (ch == (byte)'(')
                {
                    operands.Add(ReadStringLiteral(data, ref pos));
                    continue;
                }

                // Hex string
                if (ch == (byte)'<')
                {
                    if (pos + 1 < data.Length && data[pos + 1] == (byte)'<')
                    {
                        // Inline dict — skip for content streams (shouldn't appear as operand normally)
                        operands.Add(ReadInlineDict(data, ref pos));
                        continue;
                    }
                    operands.Add(ReadHexString(data, ref pos));
                    continue;
                }

                // Array
                if (ch == (byte)'[')
                {
                    operands.Add(ReadArray(data, ref pos));
                    continue;
                }

                // Name
                if (ch == (byte)'/')
                {
                    operands.Add(ReadName(data, ref pos));
                    continue;
                }

                // Number or keyword/operator
                if (IsDigit(ch) || ch == (byte)'+' || ch == (byte)'-' || ch == (byte)'.')
                {
                    var token = ReadToken(data, ref pos);
                    if (double.TryParse(token, NumberStyles.Float, CultureInfo.InvariantCulture, out double num))
                    {
                        operands.Add(num);
                    }
                    else
                    {
                        // It's an operator
                        result.Add(new PdfOperator { Name = token, Operands = new List<object>(operands) });
                        operands.Clear();
                    }
                    continue;
                }

                // Keyword or operator
                if (IsAlpha(ch) || ch == (byte)'*' || ch == (byte)'\'')
                {
                    var token = ReadToken(data, ref pos);

                    // true/false/null are operands
                    if (token == "true") { operands.Add(true); continue; }
                    if (token == "false") { operands.Add(false); continue; }
                    if (token == "null") { operands.Add(null!); continue; }

                    // Handle inline image (BI ... ID ... EI)
                    if (token == "BI")
                    {
                        var inlineImage = ParseInlineImage(data, ref pos);
                        if (inlineImage != null)
                        {
                            result.Add(new PdfOperator { Name = "BI_IMAGE", Operands = new List<object> { inlineImage } });
                        }
                        continue;
                    }

                    // It's an operator
                    result.Add(new PdfOperator { Name = token, Operands = new List<object>(operands) });
                    operands.Clear();
                    continue;
                }

                // Unknown byte, skip
                pos++;
            }

            return result;
        }

        private void SkipWhitespace(byte[] data, ref int pos)
        {
            while (pos < data.Length)
            {
                byte c = data[pos];
                if (c == (byte)' ' || c == (byte)'\t' || c == (byte)'\n' || c == (byte)'\r' || c == 0 || c == 12)
                    pos++;
                else
                    break;
            }
        }

        private bool IsDigit(byte c) => c >= (byte)'0' && c <= (byte)'9';
        private bool IsAlpha(byte c) => (c >= (byte)'a' && c <= (byte)'z') || (c >= (byte)'A' && c <= (byte)'Z');

        private bool IsDelimiter(byte c) =>
            c == (byte)'(' || c == (byte)')' || c == (byte)'<' || c == (byte)'>' ||
            c == (byte)'[' || c == (byte)']' || c == (byte)'{' || c == (byte)'}' ||
            c == (byte)'/' || c == (byte)'%';

        private bool IsWhitespace(byte c) =>
            c == (byte)' ' || c == (byte)'\t' || c == (byte)'\n' || c == (byte)'\r' || c == 0 || c == 12;

        private string ReadToken(byte[] data, ref int pos)
        {
            int start = pos;
            while (pos < data.Length && !IsWhitespace(data[pos]) && !IsDelimiter(data[pos]))
                pos++;
            return Encoding.ASCII.GetString(data, start, pos - start);
        }

        private byte[] ReadStringLiteral(byte[] data, ref int pos)
        {
            pos++; // skip '('
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
                                    else break;
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
                    if (depth > 0) result.Add(c);
                }
                else
                {
                    result.Add(c);
                }
                pos++;
            }
            return result.ToArray();
        }

        private byte[] ReadHexString(byte[] data, ref int pos)
        {
            pos++; // skip '<'
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
            if (pos < data.Length) pos++; // skip '>'

            string hexStr = hex.ToString();
            if (hexStr.Length % 2 != 0) hexStr += "0";
            var result = new byte[hexStr.Length / 2];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = byte.Parse(hexStr.Substring(i * 2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            }
            return result;
        }

        private List<object> ReadArray(byte[] data, ref int pos)
        {
            pos++; // skip '['
            var items = new List<object>();
            while (pos < data.Length)
            {
                SkipWhitespace(data, ref pos);
                if (pos >= data.Length) break;
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
                        items.Add(num);
                    else
                        items.Add(token);
                }
            }
            return items;
        }

        private string ReadName(byte[] data, ref int pos)
        {
            pos++; // skip '/'
            var sb = new StringBuilder();
            while (pos < data.Length && !IsWhitespace(data[pos]) && !IsDelimiter(data[pos]))
            {
                if (data[pos] == (byte)'#' && pos + 2 < data.Length)
                {
                    string hexChars = "" + (char)data[pos + 1] + (char)data[pos + 2];
                    if (byte.TryParse(hexChars, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out byte decoded))
                    {
                        sb.Append((char)decoded);
                        pos += 3;
                        continue;
                    }
                }
                sb.Append((char)data[pos]);
                pos++;
            }
            return "/" + sb.ToString();
        }

        private object ReadInlineDict(byte[] data, ref int pos)
        {
            // Skip << ... >> — inline dicts in content streams are rare
            pos += 2; // skip '<<'
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

        private sealed class InlineImageData
        {
            public int Width;
            public int Height;
            public int BitsPerComponent = 8;
            public string ColorSpace = "DeviceRGB";
            public string Filter = "";
            public byte[] Data = Array.Empty<byte>();
        }

        private InlineImageData? ParseInlineImage(byte[] data, ref int pos)
        {
            var img = new InlineImageData();

            // Parse key-value pairs until ID
            SkipWhitespace(data, ref pos);
            while (pos < data.Length)
            {
                SkipWhitespace(data, ref pos);
                if (pos >= data.Length) break;

                // Check for ID keyword
                if (data[pos] == (byte)'I' && pos + 1 < data.Length && data[pos + 1] == (byte)'D' &&
                    (pos + 2 >= data.Length || IsWhitespace(data[pos + 2])))
                {
                    pos += 2;
                    // Skip single whitespace after ID
                    if (pos < data.Length && (data[pos] == ' ' || data[pos] == '\n' || data[pos] == '\r'))
                        pos++;
                    break;
                }

                // Read key
                string key;
                if (data[pos] == (byte)'/')
                    key = ReadName(data, ref pos);
                else
                    key = ReadToken(data, ref pos);
                SkipWhitespace(data, ref pos);

                // Read value
                if (pos >= data.Length) break;
                string value;
                if (data[pos] == (byte)'/')
                    value = ReadName(data, ref pos);
                else if (data[pos] == (byte)'[')
                {
                    // Skip array values
                    ReadArray(data, ref pos);
                    continue;
                }
                else
                    value = ReadToken(data, ref pos);

                // Map abbreviated keys to full names
                switch (key)
                {
                    case "/W": case "/Width": img.Width = int.TryParse(value, out int w) ? w : 0; break;
                    case "/H": case "/Height": img.Height = int.TryParse(value, out int h) ? h : 0; break;
                    case "/BPC": case "/BitsPerComponent": img.BitsPerComponent = int.TryParse(value, out int bpc) ? bpc : 8; break;
                    case "/CS": case "/ColorSpace":
                        value = value.TrimStart('/');
                        if (value == "G" || value == "DeviceGray") img.ColorSpace = "DeviceGray";
                        else if (value == "RGB" || value == "DeviceRGB") img.ColorSpace = "DeviceRGB";
                        else if (value == "CMYK" || value == "DeviceCMYK") img.ColorSpace = "DeviceCMYK";
                        else img.ColorSpace = value;
                        break;
                    case "/F": case "/Filter":
                        value = value.TrimStart('/');
                        if (value == "AHx" || value == "ASCIIHexDecode") img.Filter = "ASCIIHexDecode";
                        else if (value == "A85" || value == "ASCII85Decode") img.Filter = "ASCII85Decode";
                        else if (value == "Fl" || value == "FlateDecode") img.Filter = "FlateDecode";
                        else if (value == "DCT" || value == "DCTDecode") img.Filter = "DCTDecode";
                        else img.Filter = value;
                        break;
                }
            }

            // Read image data until EI
            int dataStart = pos;
            while (pos < data.Length)
            {
                if (data[pos] == (byte)'E' && pos + 1 < data.Length && data[pos + 1] == (byte)'I')
                {
                    if (pos + 2 >= data.Length || IsWhitespace(data[pos + 2]))
                    {
                        if (pos > 0 && IsWhitespace(data[pos - 1]))
                        {
                            int dataEnd = pos - 1; // exclude whitespace before EI
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

        private void DrawInlineImage(SKCanvas canvas, GraphicsState state, InlineImageData img)
        {
            if (img.Width <= 0 || img.Height <= 0 || img.Data.Length == 0) return;

            byte[] pixelData = img.Data;

            // Decompress if needed
            if (img.Filter == "FlateDecode")
            {
                try
                {
                    using var input = new MemoryStream(pixelData);
                    // Skip zlib header (2 bytes)
                    input.ReadByte();
                    input.ReadByte();
                    using var deflate = new System.IO.Compression.DeflateStream(input, System.IO.Compression.CompressionMode.Decompress);
                    using var output = new MemoryStream();
                    deflate.CopyTo(output);
                    pixelData = output.ToArray();
                }
                catch { return; }
            }
            else if (img.Filter == "DCTDecode")
            {
                // JPEG — let Skia decode it
                using var bitmap = SKBitmap.Decode(pixelData);
                if (bitmap == null) return;
                DrawInlineBitmap(canvas, state, bitmap, img.Width, img.Height);
                return;
            }

            // Create bitmap from raw pixels
            SKBitmap? bmp = null;
            if (img.ColorSpace == "DeviceGray")
                bmp = CreateGrayBitmap(pixelData, img.Width, img.Height, img.BitsPerComponent);
            else if (img.ColorSpace == "DeviceCMYK")
                bmp = CreateCmykBitmap(pixelData, img.Width, img.Height);
            else
                bmp = CreateRgbBitmap(pixelData, img.Width, img.Height, img.BitsPerComponent);

            if (bmp == null) return;
            try
            {
                DrawInlineBitmap(canvas, state, bmp, img.Width, img.Height);
            }
            finally
            {
                bmp.Dispose();
            }
        }

        private void DrawInlineBitmap(SKCanvas canvas, GraphicsState state, SKBitmap bitmap, int width, int height)
        {
            canvas.Save();
            var imageMatrix = new SKMatrix(
                1f / width, 0, 0,
                0, -1f / height, 1,
                0, 0, 1);
            canvas.Concat(imageMatrix);

            using var paint = new SKPaint
            {
                IsAntialias = true,
                Color = new SKColor(255, 255, 255, ClampByte(state.FillAlpha * 255f)),
            };
            canvas.DrawBitmap(bitmap, 0, 0, paint);
            canvas.Restore();
        }

        #endregion

        #region Operator Execution

        private void ExecuteOperator(SKCanvas canvas, PdfOperator op, GraphicsState state,
            Stack<GraphicsState> stateStack, SKPath path,
            ref bool pendingClipNonZero, ref bool pendingClipEvenOdd, PdfObj pageDict)
        {
            var args = op.Operands;

            switch (op.Name)
            {
                // Graphics state
                case "q": PushState(canvas, state, stateStack); break;
                case "Q": PopState(canvas, state, stateStack); break;
                case "cm": OpConcat(canvas, state, args); break;
                case "w": state.LineWidth = GetFloat(args, 0); break;
                case "J": state.LineCap = GetLineCap((int)GetFloat(args, 0)); break;
                case "j": state.LineJoin = GetLineJoin((int)GetFloat(args, 0)); break;
                case "M": state.MiterLimit = GetFloat(args, 0); break;
                case "d": OpSetDash(state, args); break;
                case "gs": OpSetExtGState(state, args, pageDict); break;

                // Path construction
                case "m": path.MoveTo(GetFloat(args, 0), GetFloat(args, 1));
                    state.CurrentX = GetFloat(args, 0); state.CurrentY = GetFloat(args, 1); break;
                case "l": path.LineTo(GetFloat(args, 0), GetFloat(args, 1));
                    state.CurrentX = GetFloat(args, 0); state.CurrentY = GetFloat(args, 1); break;
                case "c":
                    path.CubicTo(GetFloat(args, 0), GetFloat(args, 1),
                        GetFloat(args, 2), GetFloat(args, 3),
                        GetFloat(args, 4), GetFloat(args, 5));
                    state.CurrentX = GetFloat(args, 4); state.CurrentY = GetFloat(args, 5);
                    break;
                case "v":
                    path.CubicTo(state.CurrentX, state.CurrentY,
                        GetFloat(args, 0), GetFloat(args, 1),
                        GetFloat(args, 2), GetFloat(args, 3));
                    state.CurrentX = GetFloat(args, 2); state.CurrentY = GetFloat(args, 3);
                    break;
                case "y":
                    path.CubicTo(GetFloat(args, 0), GetFloat(args, 1),
                        GetFloat(args, 2), GetFloat(args, 3),
                        GetFloat(args, 2), GetFloat(args, 3));
                    state.CurrentX = GetFloat(args, 2); state.CurrentY = GetFloat(args, 3);
                    break;
                case "h": path.Close(); break;
                case "re": OpRect(path, state, args); break;

                // Path painting
                case "S": PaintPath(canvas, path, state, false, true, false, ref pendingClipNonZero, ref pendingClipEvenOdd); break;
                case "s": path.Close(); PaintPath(canvas, path, state, false, true, false, ref pendingClipNonZero, ref pendingClipEvenOdd); break;
                case "f":
                case "F": PaintPath(canvas, path, state, true, false, false, ref pendingClipNonZero, ref pendingClipEvenOdd); break;
                case "f*": PaintPath(canvas, path, state, true, false, true, ref pendingClipNonZero, ref pendingClipEvenOdd); break;
                case "B": PaintPath(canvas, path, state, true, true, false, ref pendingClipNonZero, ref pendingClipEvenOdd); break;
                case "B*": PaintPath(canvas, path, state, true, true, true, ref pendingClipNonZero, ref pendingClipEvenOdd); break;
                case "b": path.Close(); PaintPath(canvas, path, state, true, true, false, ref pendingClipNonZero, ref pendingClipEvenOdd); break;
                case "b*": path.Close(); PaintPath(canvas, path, state, true, true, true, ref pendingClipNonZero, ref pendingClipEvenOdd); break;
                case "n": ApplyPendingClip(canvas, path, ref pendingClipNonZero, ref pendingClipEvenOdd); path.Reset(); break;

                // Clipping
                case "W": pendingClipNonZero = true; break;
                case "W*": pendingClipEvenOdd = true; break;

                // Color
                case "g": state.FillColor = GrayToColor(GetFloat(args, 0)); break;
                case "G": state.StrokeColor = GrayToColor(GetFloat(args, 0)); break;
                case "rg": state.FillColor = RgbToColor(GetFloat(args, 0), GetFloat(args, 1), GetFloat(args, 2)); break;
                case "RG": state.StrokeColor = RgbToColor(GetFloat(args, 0), GetFloat(args, 1), GetFloat(args, 2)); break;
                case "k": state.FillColor = CmykToColor(GetFloat(args, 0), GetFloat(args, 1), GetFloat(args, 2), GetFloat(args, 3)); break;
                case "K": state.StrokeColor = CmykToColor(GetFloat(args, 0), GetFloat(args, 1), GetFloat(args, 2), GetFloat(args, 3)); break;
                case "cs": state.FillColorSpace = GetNameStr(args, 0); break;
                case "CS": state.StrokeColorSpace = GetNameStr(args, 0); break;
                case "sc":
                case "scn": OpSetColor(state, args, true); break;
                case "SC":
                case "SCN": OpSetColor(state, args, false); break;

                // Text
                case "BT": OpBeginText(state); break;
                case "ET": break;
                case "Tf": OpSetFont(state, args, pageDict); break;
                case "Td": OpTextMove(state, args); break;
                case "TD": OpTextMoveTD(state, args); break;
                case "Tm": OpSetTextMatrix(state, args); break;
                case "T*": OpTextNextLine(state); break;
                case "Tj": OpShowText(canvas, state, args); break;
                case "TJ": OpShowTextArray(canvas, state, args); break;
                case "'": OpTextNextLine(state); OpShowText(canvas, state, args); break;
                case "\"": OpShowTextQuoteDbl(canvas, state, args); break;
                case "Tc": state.CharSpacing = GetFloat(args, 0); break;
                case "Tw": state.WordSpacing = GetFloat(args, 0); break;
                case "TL": state.TextLeading = GetFloat(args, 0); break;
                case "Tr": state.TextRenderMode = (int)GetFloat(args, 0); break;
                case "Ts": state.TextRise = GetFloat(args, 0); break;
                case "Tz": state.HorizontalScaling = GetFloat(args, 0); break;

                // XObject
                case "Do": OpDoXObject(canvas, state, stateStack, args, pageDict); break;

                // Inline image
                case "BI_IMAGE":
                    if (args.Count > 0 && args[0] is InlineImageData img)
                        DrawInlineImage(canvas, state, img);
                    break;

                // Ignored operators
                case "i": break; // flatness
                case "ri": break; // rendering intent
                case "BMC": break;
                case "BDC": break;
                case "EMC": break;
                case "MP": break;
                case "DP": break;
                default: break; // unknown, skip
            }
        }

        private void PushState(SKCanvas canvas, GraphicsState state, Stack<GraphicsState> stateStack)
        {
            stateStack.Push(state.Clone());
            canvas.Save();
        }

        private void PopState(SKCanvas canvas, GraphicsState state, Stack<GraphicsState> stateStack)
        {
            if (stateStack.Count > 0)
            {
                var restored = stateStack.Pop();
                state.CopyFrom(restored);
            }
            canvas.Restore();
        }

        private void OpConcat(SKCanvas canvas, GraphicsState state, List<object> args)
        {
            float a = GetFloat(args, 0), b = GetFloat(args, 1);
            float c = GetFloat(args, 2), d = GetFloat(args, 3);
            float e = GetFloat(args, 4), f = GetFloat(args, 5);

            // PDF matrix: [a b c d e f]
            // SKMatrix:   ScaleX=a SkewX=c TransX=e
            //             SkewY=b ScaleY=d TransY=f
            var matrix = new SKMatrix(a, c, e, b, d, f, 0, 0, 1);
            canvas.Concat(matrix);
        }

        private void OpSetDash(GraphicsState state, List<object> args)
        {
            if (args.Count >= 2 && args[0] is List<object> dashList)
            {
                state.DashArray = dashList.Select(x => x is double d ? (float)d : 0f).ToArray();
                state.DashPhase = GetFloat(args, 1);
            }
        }

        private void OpSetExtGState(GraphicsState state, List<object> args, PdfObj pageDict)
        {
            string gsName = GetNameStr(args, 0);
            if (string.IsNullOrEmpty(gsName)) return;

            var resources = _reader.Resolve(pageDict["Resources"]);
            var extGState = _reader.Resolve(resources["ExtGState"]);
            var gsDict = _reader.Resolve(extGState[gsName]);
            if (gsDict.IsNull) return;

            // /ca — fill alpha
            if (gsDict.ContainsKey("ca"))
                state.FillAlpha = gsDict["ca"].AsFloat();
            // /CA — stroke alpha
            if (gsDict.ContainsKey("CA"))
                state.StrokeAlpha = gsDict["CA"].AsFloat();
            // /LW — line width
            if (gsDict.ContainsKey("LW"))
                state.LineWidth = gsDict["LW"].AsFloat();
            // /LC — line cap
            if (gsDict.ContainsKey("LC"))
                state.LineCap = GetLineCap((int)gsDict["LC"].AsInt());
            // /LJ — line join
            if (gsDict.ContainsKey("LJ"))
                state.LineJoin = GetLineJoin((int)gsDict["LJ"].AsInt());
            // /ML — miter limit
            if (gsDict.ContainsKey("ML"))
                state.MiterLimit = gsDict["ML"].AsFloat();
            // /BM — blend mode (basic support)
            if (gsDict.ContainsKey("BM"))
            {
                var bm = _reader.Resolve(gsDict["BM"]).AsName();
                if (bm.StartsWith("/")) bm = bm.Substring(1);
                state.BlendMode = bm;
            }
        }

        private void OpRect(SKPath path, GraphicsState state, List<object> args)
        {
            float x = GetFloat(args, 0), y = GetFloat(args, 1);
            float w = GetFloat(args, 2), h = GetFloat(args, 3);
            path.MoveTo(x, y);
            path.LineTo(x + w, y);
            path.LineTo(x + w, y + h);
            path.LineTo(x, y + h);
            path.Close();
            state.CurrentX = x;
            state.CurrentY = y;
        }

        private void PaintPath(SKCanvas canvas, SKPath path, GraphicsState state,
            bool fill, bool stroke, bool evenOdd,
            ref bool pendingClipNonZero, ref bool pendingClipEvenOdd)
        {
            ApplyPendingClip(canvas, path, ref pendingClipNonZero, ref pendingClipEvenOdd);

            if (fill)
            {
                path.FillType = evenOdd ? SKPathFillType.EvenOdd : SKPathFillType.Winding;
                using var paint = new SKPaint
                {
                    Color = WithAlpha(state.FillColor, state.FillAlpha),
                    Style = SKPaintStyle.Fill,
                    IsAntialias = true,
                    BlendMode = GetSkBlendMode(state.BlendMode),
                };
                canvas.DrawPath(path, paint);
            }

            if (stroke)
            {
                using var paint = CreateStrokePaint(state);
                canvas.DrawPath(path, paint);
            }

            path.Reset();
        }

        private void ApplyPendingClip(SKCanvas canvas, SKPath path,
            ref bool pendingClipNonZero, ref bool pendingClipEvenOdd)
        {
            if (pendingClipNonZero || pendingClipEvenOdd)
            {
                var clipPath = new SKPath(path);
                clipPath.FillType = pendingClipEvenOdd ? SKPathFillType.EvenOdd : SKPathFillType.Winding;
                canvas.ClipPath(clipPath, SKClipOperation.Intersect, true);
                clipPath.Dispose();
                pendingClipNonZero = false;
                pendingClipEvenOdd = false;
            }
        }

        private SKPaint CreateStrokePaint(GraphicsState state)
        {
            var paint = new SKPaint
            {
                Color = WithAlpha(state.StrokeColor, state.StrokeAlpha),
                Style = SKPaintStyle.Stroke,
                StrokeWidth = state.LineWidth,
                StrokeCap = state.LineCap,
                StrokeJoin = state.LineJoin,
                StrokeMiter = state.MiterLimit,
                IsAntialias = true,
            };

            if (state.DashArray != null && state.DashArray.Length > 0 &&
                state.DashArray.Any(d => d > 0))
            {
                // SKPathEffect.CreateDash requires even-length array
                float[] dashes = state.DashArray;
                if (dashes.Length % 2 != 0)
                {
                    dashes = new float[state.DashArray.Length * 2];
                    Array.Copy(state.DashArray, 0, dashes, 0, state.DashArray.Length);
                    Array.Copy(state.DashArray, 0, dashes, state.DashArray.Length, state.DashArray.Length);
                }
                paint.PathEffect = SKPathEffect.CreateDash(dashes, state.DashPhase);
            }

            return paint;
        }

        #endregion

        #region Color Helpers

        private static SKColor GrayToColor(float g)
        {
            byte v = ClampByte(g * 255f);
            return new SKColor(v, v, v);
        }

        private static SKColor RgbToColor(float r, float g, float b)
        {
            return new SKColor(ClampByte(r * 255f), ClampByte(g * 255f), ClampByte(b * 255f));
        }

        private static SKColor CmykToColor(float c, float m, float y, float k)
        {
            float r = (1 - c) * (1 - k);
            float g = (1 - m) * (1 - k);
            float b = (1 - y) * (1 - k);
            return new SKColor(ClampByte(r * 255f), ClampByte(g * 255f), ClampByte(b * 255f));
        }

        private static SKColor WithAlpha(SKColor color, float alpha)
        {
            if (alpha >= 1f) return color;
            return new SKColor(color.Red, color.Green, color.Blue, ClampByte(alpha * 255f));
        }

        private static byte ClampByte(float v)
        {
            int i = (int)(v + 0.5f);
            if (i < 0) return 0;
            if (i > 255) return 255;
            return (byte)i;
        }

        private void OpSetColor(GraphicsState state, List<object> args, bool isFill)
        {
            string cs = isFill ? state.FillColorSpace : state.StrokeColorSpace;

            // Filter out name operands (pattern names in scn/SCN)
            var numArgs = args.Where(a => a is double).Select(a => (float)(double)a).ToList();

            SKColor color;
            if (cs == "DeviceCMYK" || cs == "CMYK" || numArgs.Count == 4)
            {
                color = CmykToColor(
                    numArgs.Count > 0 ? numArgs[0] : 0,
                    numArgs.Count > 1 ? numArgs[1] : 0,
                    numArgs.Count > 2 ? numArgs[2] : 0,
                    numArgs.Count > 3 ? numArgs[3] : 0);
            }
            else if (cs == "DeviceGray" || cs == "Gray" || cs == "CalGray" || numArgs.Count == 1)
            {
                color = GrayToColor(numArgs.Count > 0 ? numArgs[0] : 0);
            }
            else
            {
                // DeviceRGB or default
                color = RgbToColor(
                    numArgs.Count > 0 ? numArgs[0] : 0,
                    numArgs.Count > 1 ? numArgs[1] : 0,
                    numArgs.Count > 2 ? numArgs[2] : 0);
            }

            if (isFill) state.FillColor = color;
            else state.StrokeColor = color;
        }

        #endregion

        #region Text Operations

        private void OpBeginText(GraphicsState state)
        {
            state.TextMatrix = SKMatrix.Identity;
            state.TextLineMatrix = SKMatrix.Identity;
        }

        private void OpSetFont(GraphicsState state, List<object> args, PdfObj pageDict)
        {
            state.FontName = GetNameStr(args, 0);
            state.FontSize = GetFloat(args, 1);

            // Resolve font dict and cache typeface
            ResolveFontTypeface(state, pageDict);
        }

        private void ResolveFontTypeface(GraphicsState state, PdfObj pageDict)
        {
            var resources = _reader.Resolve(pageDict["Resources"]);
            var fonts = _reader.Resolve(resources["Font"]);
            if (fonts.IsNull || string.IsNullOrEmpty(state.FontName)) return;

            var fontDict = _reader.Resolve(fonts[state.FontName]);
            if (fontDict.IsNull) return;

            state.FontDict = fontDict;

            // Detect CIDFont (Type 0 composite font with 2-byte encoding)
            var fontType = _reader.Resolve(fontDict["Subtype"]).AsName();
            state.IsCIDFont = fontType == "Type0" || fontDict.ContainsKey("DescendantFonts");

            // For Type0 fonts, also resolve the descendant CIDFont for font program extraction
            PdfObj cidFontDict = fontDict;
            if (state.IsCIDFont)
            {
                var descendants = _reader.Resolve(fontDict["DescendantFonts"]);
                if (descendants.IsArray && descendants.Count > 0)
                    cidFontDict = _reader.Resolve(descendants[0]);
            }

            // Parse ToUnicode CMap if present
            var toUnicode = _reader.Resolve(fontDict["ToUnicode"]);
            if (toUnicode.IsStream)
            {
                var cmapData = _reader.GetStreamBytes(toUnicode);
                if (cmapData != null)
                    state.ToUnicodeMap = ParseToUnicodeCMap(cmapData);
            }

            // Parse encoding
            state.Encoding = ResolveEncoding(fontDict);

            // Parse font widths for accurate glyph positioning
            state.FontWidths = ResolveFontWidths(fontDict, cidFontDict);
            var firstCharObj = _reader.Resolve(fontDict["FirstChar"]);
            state.FontFirstChar = firstCharObj.IsNull ? 0 : (int)firstCharObj.AsInt();
            var defaultWidthObj = _reader.Resolve(cidFontDict["DW"]);
            state.FontDefaultWidth = defaultWidthObj.IsNull ? 1000f : defaultWidthObj.AsFloat();

            // Get or create typeface
            int cacheKey = GetFontCacheKey(fontDict);
            if (_typefaceCache.TryGetValue(cacheKey, out var cached))
            {
                state.Typeface = cached;
                return;
            }

            // For CIDFonts, skip embedded font — the subset has a minimal cmap that
            // Skia can't use for Unicode→glyph mapping. Use system font instead.
            // For non-CID fonts, try embedded font first.
            byte[]? fontData = null;
            if (!state.IsCIDFont)
            {
                fontData = _reader.GetFontProgramData(cidFontDict);
                if (fontData == null || fontData.Length == 0)
                    fontData = _reader.GetFontProgramData(fontDict);
            }
            if (fontData != null && fontData.Length > 0)
            {
                try
                {
                    using var skData = SKData.CreateCopy(fontData);
                    var tf = SKTypeface.FromData(skData);
                    if (tf != null)
                    {
                        _typefaceCache[cacheKey] = tf;
                        state.Typeface = tf;
                        return;
                    }
                }
                catch
                {
                    // Fall through to system font mapping
                }
            }

            // Map to system font (check descendant CIDFont first for BaseFont name)
            var baseFontName = _reader.Resolve(cidFontDict["BaseFont"]).AsName();
            if (string.IsNullOrEmpty(baseFontName))
                baseFontName = _reader.Resolve(fontDict["BaseFont"]).AsName();
            if (baseFontName.StartsWith("/")) baseFontName = baseFontName.Substring(1);
            var systemName = MapPdfFontToSystem(baseFontName);
            var sysTf = SKTypeface.FromFamilyName(systemName, GetFontStyle(baseFontName));
            if (sysTf != null)
            {
                _typefaceCache[cacheKey] = sysTf;
                state.Typeface = sysTf;
            }
        }

        private int GetFontCacheKey(PdfObj fontDict)
        {
            // Use object identity hash; fallback to font name hash
            if (fontDict is PdfRef pdfRef)
                return pdfRef.ObjNum;
            return fontDict.GetHashCode();
        }

        private string MapPdfFontToSystem(string baseFontName)
        {
            if (string.IsNullOrEmpty(baseFontName)) return "Arial";

            string name = baseFontName.Replace(",", "-");

            if (name.StartsWith("Helvetica", StringComparison.OrdinalIgnoreCase) ||
                name.StartsWith("Arial", StringComparison.OrdinalIgnoreCase))
                return "Arial";
            if (name.StartsWith("Times", StringComparison.OrdinalIgnoreCase))
                return "Times New Roman";
            if (name.StartsWith("Courier", StringComparison.OrdinalIgnoreCase))
                return "Courier New";
            if (name.StartsWith("Symbol", StringComparison.OrdinalIgnoreCase))
                return "Symbol";
            if (name.StartsWith("ZapfDingbats", StringComparison.OrdinalIgnoreCase))
                return "Wingdings";

            // Try to use the base font name directly
            // Strip subset prefix (e.g., "ABCDEF+FontName" → "FontName")
            int plus = name.IndexOf('+');
            if (plus >= 0 && plus < 7)
                name = name.Substring(plus + 1);

            // Strip style suffixes for family name
            string family = name.Replace("-Bold", "").Replace("-Italic", "")
                .Replace("-BoldItalic", "").Replace("-Oblique", "")
                .Replace("-BoldOblique", "").Replace("-Roman", "")
                .Replace("-Regular", "");

            return family;
        }

        private SKFontStyle GetFontStyle(string baseFontName)
        {
            if (string.IsNullOrEmpty(baseFontName)) return SKFontStyle.Normal;

            bool bold = baseFontName.IndexOf("Bold", StringComparison.OrdinalIgnoreCase) >= 0;
            bool italic = baseFontName.IndexOf("Italic", StringComparison.OrdinalIgnoreCase) >= 0 ||
                          baseFontName.IndexOf("Oblique", StringComparison.OrdinalIgnoreCase) >= 0;

            if (bold && italic) return SKFontStyle.BoldItalic;
            if (bold) return SKFontStyle.Bold;
            if (italic) return SKFontStyle.Italic;
            return SKFontStyle.Normal;
        }

        private Dictionary<int, string>? ResolveEncoding(PdfObj fontDict)
        {
            var encoding = _reader.Resolve(fontDict["Encoding"]);
            if (encoding.IsNull) return null;

            string encodingName = encoding.AsName();
            if (string.IsNullOrEmpty(encodingName) && encoding.IsDict)
                encodingName = _reader.Resolve(encoding["BaseEncoding"]).AsName();

            if (encodingName.Contains("WinAnsi") || encodingName.Contains("WinAnsiEncoding"))
                return null; // WinAnsi is basically Windows-1252, close enough to default

            if (encodingName.Contains("MacRoman") || encodingName.Contains("MacRomanEncoding"))
                return null; // Close enough for most chars

            // Handle /Differences array
            if (encoding.IsDict && encoding.ContainsKey("Differences"))
            {
                var diffs = _reader.Resolve(encoding["Differences"]);
                if (diffs.IsArray)
                    return ParseEncodingDifferences(diffs);
            }

            return null;
        }

        private Dictionary<int, float>? ResolveFontWidths(PdfObj fontDict, PdfObj cidFontDict)
        {
            // For simple fonts: /Widths array with /FirstChar offset
            var widths = _reader.Resolve(fontDict["Widths"]);
            if (!widths.IsNull && widths.IsArray && widths.Count > 0)
            {
                var firstChar = _reader.Resolve(fontDict["FirstChar"]);
                int fc = firstChar.IsNull ? 0 : (int)firstChar.AsInt();
                var map = new Dictionary<int, float>();
                for (int i = 0; i < widths.Count; i++)
                {
                    float w = widths[i].AsFloat();
                    if (w > 0) map[fc + i] = w;
                }
                return map.Count > 0 ? map : null;
            }

            // For CIDFonts: /W array — [cid [w1 w2 ...]] or [cidFirst cidLast w]
            var wArr = _reader.Resolve(cidFontDict["W"]);
            if (!wArr.IsNull && wArr.IsArray && wArr.Count > 0)
            {
                var map = new Dictionary<int, float>();
                int i = 0;
                while (i < wArr.Count)
                {
                    int startCid = (int)_reader.Resolve(wArr[i]).AsInt();
                    i++;
                    if (i >= wArr.Count) break;

                    var next = _reader.Resolve(wArr[i]);
                    if (next.IsArray)
                    {
                        // [startCid [w1 w2 ...]]
                        for (int j = 0; j < next.Count; j++)
                        {
                            map[startCid + j] = next[j].AsFloat();
                        }
                        i++;
                    }
                    else
                    {
                        // [cidFirst cidLast w]
                        int endCid = (int)next.AsInt();
                        i++;
                        if (i >= wArr.Count) break;
                        float w = _reader.Resolve(wArr[i]).AsFloat();
                        i++;
                        for (int c = startCid; c <= endCid; c++)
                            map[c] = w;
                    }
                }
                return map.Count > 0 ? map : null;
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
                    // It's a number
                    code = (int)item.AsInt();
                }
                else
                {
                    string glyphName = item.AsName();
                    if (glyphName.StartsWith("/")) glyphName = glyphName.Substring(1);
                    string? unicode = GlyphNameToUnicode(glyphName);
                    if (unicode != null)
                        map[code] = unicode;
                    code++;
                }
            }
            return map;
        }

        private static string? GlyphNameToUnicode(string name)
        {
            // Common glyph name to unicode mappings
            switch (name)
            {
                case "space": return " ";
                case "exclam": return "!";
                case "quotedbl": return "\"";
                case "numbersign": return "#";
                case "dollar": return "$";
                case "percent": return "%";
                case "ampersand": return "&";
                case "quotesingle": return "'";
                case "parenleft": return "(";
                case "parenright": return ")";
                case "asterisk": return "*";
                case "plus": return "+";
                case "comma": return ",";
                case "hyphen": return "-";
                case "period": return ".";
                case "slash": return "/";
                case "zero": return "0";
                case "one": return "1";
                case "two": return "2";
                case "three": return "3";
                case "four": return "4";
                case "five": return "5";
                case "six": return "6";
                case "seven": return "7";
                case "eight": return "8";
                case "nine": return "9";
                case "colon": return ":";
                case "semicolon": return ";";
                case "less": return "<";
                case "equal": return "=";
                case "greater": return ">";
                case "question": return "?";
                case "at": return "@";
                case "bracketleft": return "[";
                case "backslash": return "\\";
                case "bracketright": return "]";
                case "asciicircum": return "^";
                case "underscore": return "_";
                case "grave": return "`";
                case "braceleft": return "{";
                case "bar": return "|";
                case "braceright": return "}";
                case "asciitilde": return "~";
                case "bullet": return "\u2022";
                case "endash": return "\u2013";
                case "emdash": return "\u2014";
                case "quotedblleft": return "\u201C";
                case "quotedblright": return "\u201D";
                case "quoteleft": return "\u2018";
                case "quoteright": return "\u2019";
                case "fi": return "\uFB01";
                case "fl": return "\uFB02";
                case "ellipsis": return "\u2026";
                case "trademark": return "\u2122";
                case "copyright": return "\u00A9";
                case "registered": return "\u00AE";
                case "degree": return "\u00B0";
                case "mu": return "\u00B5";
                case "paragraph": return "\u00B6";
                case "section": return "\u00A7";
                case "dagger": return "\u2020";
                case "daggerdbl": return "\u2021";
                default:
                    // If name is "uniXXXX", parse the hex
                    if (name.StartsWith("uni") && name.Length == 7)
                    {
                        if (int.TryParse(name.Substring(3), NumberStyles.HexNumber,
                            CultureInfo.InvariantCulture, out int cp))
                            return char.ConvertFromUtf32(cp);
                    }
                    // Single letter names
                    if (name.Length == 1) return name;
                    // Try first letter for A-Z, a-z
                    if (name.Length > 0 && char.IsLetter(name[0]) &&
                        (name == name[0].ToString() ||
                         name.Length > 1 && !char.IsLetter(name[1])))
                        return name[0].ToString();
                    return name;
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
                    // Each range: <start> <end> <dstStart>
                    for (int i = 0; i + 2 < hexValues.Count; i += 3)
                    {
                        int start = hexValues[i].code;
                        int end = hexValues[i + 1].code;
                        int dst = hexValues[i + 2].code;
                        string? dstStr = hexValues[i + 2].str;

                        for (int c = start; c <= end; c++)
                        {
                            if (dstStr != null && dstStr.Length > 2)
                            {
                                // Multi-byte unicode
                                int offset = c - start;
                                map[c] = IncrementUnicodeString(dstStr, offset);
                            }
                            else
                            {
                                map[c] = char.ConvertFromUtf32(dst + (c - start));
                            }
                        }
                    }
                }
                else
                {
                    // Each char: <src> <dst>
                    for (int i = 0; i + 1 < hexValues.Count; i += 2)
                    {
                        int src = hexValues[i].code;
                        string? dstStr = hexValues[i + 1].str;
                        int dst = hexValues[i + 1].code;

                        if (dstStr != null && dstStr.Length > 2)
                            map[src] = DecodeUtf16Hex(dstStr);
                        else
                            map[src] = char.ConvertFromUtf32(dst);
                    }
                }

                pos = endPos + endMarker.Length;
            }

            return map;
        }

        private struct HexEntry
        {
            public int code;
            public string? str; // raw hex string for multi-byte
        }

        private List<HexEntry> ExtractHexValues(string section)
        {
            var result = new List<HexEntry>();
            int i = 0;
            while (i < section.Length)
            {
                int open = section.IndexOf('<', i);
                if (open < 0) break;
                int close = section.IndexOf('>', open);
                if (close < 0) break;

                string hex = section.Substring(open + 1, close - open - 1).Trim();
                int code = 0;
                if (hex.Length > 0 && int.TryParse(hex, NumberStyles.HexNumber,
                    CultureInfo.InvariantCulture, out int parsed))
                {
                    code = parsed;
                }

                result.Add(new HexEntry { code = code, str = hex.Length > 4 ? hex : null });
                i = close + 1;
            }
            return result;
        }

        private string DecodeUtf16Hex(string hex)
        {
            // Hex string like "00410042" → UTF-16BE code units
            var sb = new StringBuilder();
            for (int i = 0; i + 3 < hex.Length; i += 4)
            {
                if (int.TryParse(hex.Substring(i, 4), NumberStyles.HexNumber,
                    CultureInfo.InvariantCulture, out int val))
                {
                    sb.Append((char)val);
                }
            }
            return sb.Length > 0 ? sb.ToString() : "?";
        }

        private string IncrementUnicodeString(string hex, int offset)
        {
            // Increment the last character code point by offset
            int baseVal = 0;
            if (hex.Length >= 4 && int.TryParse(hex.Substring(hex.Length - 4, 4),
                NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int parsed))
            {
                baseVal = parsed;
            }
            return char.ConvertFromUtf32(baseVal + offset);
        }

        private void OpTextMove(GraphicsState state, List<object> args)
        {
            float tx = GetFloat(args, 0);
            float ty = GetFloat(args, 1);

            var translate = SKMatrix.CreateTranslation(tx, ty);
            state.TextLineMatrix = SKMatrix.Concat(state.TextLineMatrix, translate);
            state.TextMatrix = state.TextLineMatrix;
        }

        private void OpTextMoveTD(GraphicsState state, List<object> args)
        {
            float tx = GetFloat(args, 0);
            float ty = GetFloat(args, 1);
            state.TextLeading = -ty;

            var translate = SKMatrix.CreateTranslation(tx, ty);
            state.TextLineMatrix = SKMatrix.Concat(state.TextLineMatrix, translate);
            state.TextMatrix = state.TextLineMatrix;
        }

        private void OpSetTextMatrix(GraphicsState state, List<object> args)
        {
            float a = GetFloat(args, 0), b = GetFloat(args, 1);
            float c = GetFloat(args, 2), d = GetFloat(args, 3);
            float e = GetFloat(args, 4), f = GetFloat(args, 5);

            var matrix = new SKMatrix(a, c, e, b, d, f, 0, 0, 1);
            state.TextMatrix = matrix;
            state.TextLineMatrix = matrix;
        }

        private void OpTextNextLine(GraphicsState state)
        {
            float tx = 0;
            float ty = -state.TextLeading;

            var translate = SKMatrix.CreateTranslation(tx, ty);
            state.TextLineMatrix = SKMatrix.Concat(state.TextLineMatrix, translate);
            state.TextMatrix = state.TextLineMatrix;
        }

        private void OpShowText(SKCanvas canvas, GraphicsState state, List<object> args)
        {
            if (args.Count < 1) return;
            byte[] textBytes;
            if (args[0] is byte[] bytes)
                textBytes = bytes;
            else
                return;

            DrawTextBytes(canvas, state, textBytes);
        }

        private void OpShowTextArray(SKCanvas canvas, GraphicsState state, List<object> args)
        {
            if (args.Count < 1 || !(args[0] is List<object> array)) return;

            foreach (var item in array)
            {
                if (item is byte[] textBytes)
                {
                    DrawTextBytes(canvas, state, textBytes);
                }
                else if (item is double num)
                {
                    // Negative number = move right, positive = move left (in thousandths of text space unit)
                    float adjust = (float)(-num / 1000.0) * state.FontSize;
                    var translate = SKMatrix.CreateTranslation(adjust, 0);
                    state.TextMatrix = SKMatrix.Concat(state.TextMatrix, translate);
                }
            }
        }

        private void OpShowTextQuoteDbl(SKCanvas canvas, GraphicsState state, List<object> args)
        {
            if (args.Count >= 3)
            {
                state.WordSpacing = GetFloat(args, 0);
                state.CharSpacing = GetFloat(args, 1);
                OpTextNextLine(state);

                if (args[2] is byte[] textBytes)
                    DrawTextBytes(canvas, state, textBytes);
            }
        }

        private void DrawTextBytes(SKCanvas canvas, GraphicsState state, byte[] textBytes)
        {
            string text = DecodeTextBytes(state, textBytes);
            if (string.IsNullOrEmpty(text)) return;

            var typeface = state.Typeface ?? SKTypeface.Default;
            float fontSize = Math.Abs(state.FontSize);
            float hScale = state.HorizontalScaling / 100f;

            using var font = new SKFont(typeface, fontSize)
            {
                Edging = SKFontEdging.SubpixelAntialias,
                Subpixel = true,
            };

            using var paint = new SKPaint
            {
                IsAntialias = true,
            };

            bool doFill = state.TextRenderMode == 0 || state.TextRenderMode == 2 ||
                          state.TextRenderMode == 4 || state.TextRenderMode == 6;
            bool doStroke = state.TextRenderMode == 1 || state.TextRenderMode == 2 ||
                            state.TextRenderMode == 5 || state.TextRenderMode == 6;
            bool invisible = state.TextRenderMode == 3 || state.TextRenderMode == 7;

            float textRise = state.TextRise;

            // Build byte-to-code mapping for width lookups
            int[] codes = GetCharCodes(state, textBytes);

            for (int ci = 0; ci < text.Length; ci++)
            {
                char ch = text[ci];
                string s = ch.ToString();

                // Position via text matrix
                float tx = state.TextMatrix.TransX;
                float ty = state.TextMatrix.TransY;

                canvas.Save();

                float a = state.TextMatrix.ScaleX;
                float b = state.TextMatrix.SkewY;
                float c = state.TextMatrix.SkewX;
                float d = state.TextMatrix.ScaleY;

                // Build rendering matrix with horizontal scaling
                var renderMatrix = new SKMatrix(
                    a * hScale, c, tx,
                    -b * hScale, -d, ty + textRise,
                    0, 0, 1);

                canvas.SetMatrix(SKMatrix.Concat(canvas.TotalMatrix, renderMatrix));

                if (!invisible)
                {
                    if (doFill)
                    {
                        paint.Style = SKPaintStyle.Fill;
                        paint.Color = WithAlpha(state.FillColor, state.FillAlpha);
                        canvas.DrawText(s, 0, 0, font, paint);
                    }
                    if (doStroke)
                    {
                        paint.Style = SKPaintStyle.Stroke;
                        paint.Color = WithAlpha(state.StrokeColor, state.StrokeAlpha);
                        paint.StrokeWidth = state.LineWidth;
                        canvas.DrawText(s, 0, 0, font, paint);
                    }
                }

                canvas.Restore();

                // Get advance width from font width table if available
                float advanceInTextSpace;
                int charCode = ci < codes.Length ? codes[ci] : (int)ch;
                if (state.FontWidths != null && state.FontWidths.TryGetValue(charCode, out float pdfWidth))
                {
                    // PDF widths are in 1/1000 of text space units
                    advanceInTextSpace = pdfWidth / 1000f;
                }
                else if (state.FontWidths != null && state.FontDefaultWidth > 0)
                {
                    advanceInTextSpace = state.FontDefaultWidth / 1000f;
                }
                else
                {
                    // Fall back to measuring with the font
                    float measuredWidth = font.MeasureText(s, paint);
                    advanceInTextSpace = measuredWidth / fontSize;
                }

                // PDF spec: tx = ((w0 - Tj/1000) * Tfs + Tc + Tw) * Th
                float displacement = (advanceInTextSpace * fontSize + state.CharSpacing) * hScale;
                if (ch == ' ')
                    displacement += state.WordSpacing * hScale;

                // Advance in text matrix coordinates
                float matScale = Math.Abs(state.TextMatrix.ScaleX);
                if (matScale == 0) matScale = 1;
                var translateAdv = SKMatrix.CreateTranslation(displacement / matScale, 0);
                state.TextMatrix = SKMatrix.Concat(state.TextMatrix, translateAdv);
            }
        }

        private int[] GetCharCodes(GraphicsState state, byte[] textBytes)
        {
            bool isTwoByte = state.IsCIDFont ||
                             (state.ToUnicodeMap != null && IsTwoByteEncoding(state.ToUnicodeMap));

            if (isTwoByte)
            {
                int count = textBytes.Length / 2;
                var codes = new int[count];
                for (int i = 0; i < count; i++)
                    codes[i] = (textBytes[i * 2] << 8) | textBytes[i * 2 + 1];
                return codes;
            }
            else
            {
                var codes = new int[textBytes.Length];
                for (int i = 0; i < textBytes.Length; i++)
                    codes[i] = textBytes[i];
                return codes;
            }
        }

        private string DecodeTextBytes(GraphicsState state, byte[] bytes)
        {
            // CIDFont (Type 0) always uses 2-byte CID encoding
            bool isTwoByte = state.IsCIDFont ||
                             (state.ToUnicodeMap != null && IsTwoByteEncoding(state.ToUnicodeMap));

            // Try ToUnicode map first
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
                        int code = bytes[i];
                        if (state.ToUnicodeMap.TryGetValue(code, out string? mapped))
                            sb.Append(mapped);
                        else
                            sb.Append((char)code);
                    }
                }
                return sb.ToString();
            }

            // CIDFont without ToUnicode: decode 2-byte CIDs as Unicode code points
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

            // Try encoding differences
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

            // Default: map byte-to-char directly (ISO 8859-1 / Latin-1)
            {
                var sb = new StringBuilder(bytes.Length);
                for (int i = 0; i < bytes.Length; i++)
                    sb.Append((char)bytes[i]);
                return sb.ToString();
            }
        }

        private bool IsTwoByteEncoding(Dictionary<int, string> map)
        {
            // If any key > 255, it's a two-byte encoding
            foreach (var key in map.Keys)
            {
                if (key > 255) return true;
            }
            return false;
        }

        #endregion

        #region XObject Operations

        private void OpDoXObject(SKCanvas canvas, GraphicsState state,
            Stack<GraphicsState> stateStack, List<object> args, PdfObj pageDict)
        {
            string name = GetNameStr(args, 0);
            if (string.IsNullOrEmpty(name)) return;

            var resources = _reader.Resolve(pageDict["Resources"]);
            var xobjects = _reader.Resolve(resources["XObject"]);
            var xobj = _reader.Resolve(xobjects[name]);
            if (xobj.IsNull) return;

            string subtype = _reader.Resolve(xobj["Subtype"]).AsName();

            if (subtype == "/Image" || subtype == "Image")
                DrawImageXObject(canvas, state, xobj);
            else if (subtype == "/Form" || subtype == "Form")
                DrawFormXObject(canvas, state, stateStack, xobj, pageDict);
        }

        private void DrawImageXObject(SKCanvas canvas, GraphicsState state, PdfObj imageDict)
        {
            int width = (int)_reader.Resolve(imageDict["Width"]).AsInt();
            int height = (int)_reader.Resolve(imageDict["Height"]).AsInt();
            if (width <= 0 || height <= 0) return;

            byte[] imageData = _reader.GetStreamBytes(imageDict);
            if (imageData == null || imageData.Length == 0) return;

            SKBitmap? bitmap = null;

            // Check filter to determine format
            string filter = _reader.Resolve(imageDict["Filter"]).AsName();

            if (filter.Contains("DCTDecode") || filter.Contains("DCT"))
            {
                // JPEG data — decode directly
                bitmap = SKBitmap.Decode(imageData);
            }
            else if (filter.Contains("JPXDecode") || filter.Contains("JPX"))
            {
                // JPEG2000 — try decoding
                bitmap = SKBitmap.Decode(imageData);
            }
            else
            {
                // Raw pixel data (after FlateDecode etc. already applied by GetStreamBytes)
                string colorSpace = ResolveColorSpaceName(imageDict);
                int bitsPerComponent = (int)_reader.Resolve(imageDict["BitsPerComponent"]).AsInt();
                if (bitsPerComponent <= 0) bitsPerComponent = 8;

                bitmap = CreateBitmapFromRawPixels(imageData, width, height, colorSpace, bitsPerComponent, imageDict);
            }

            if (bitmap == null) return;

            try
            {
                // In PDF, images are drawn in a 1x1 unit square, scaled by CTM
                // We need to draw the bitmap into the unit square
                canvas.Save();

                // Image space: bottom-left origin, 1x1
                // Scale to image dimensions, flip Y
                var imageMatrix = new SKMatrix(
                    1f / width, 0, 0,
                    0, -1f / height, 1,
                    0, 0, 1);
                canvas.Concat(imageMatrix);

                using var paint = new SKPaint
                {
                    IsAntialias = true,
                    Color = new SKColor(255, 255, 255, ClampByte(state.FillAlpha * 255f)),
                };

                canvas.DrawBitmap(bitmap, 0, 0, paint);
                canvas.Restore();
            }
            finally
            {
                bitmap.Dispose();
            }
        }

        private string ResolveColorSpaceName(PdfObj imageDict)
        {
            var cs = _reader.Resolve(imageDict["ColorSpace"]);
            if (cs.IsNull) return "DeviceRGB";

            string name = cs.AsName();
            if (!string.IsNullOrEmpty(name))
            {
                if (name.StartsWith("/")) name = name.Substring(1);
                return name;
            }

            // Array form: [/ICCBased stream] or [/Indexed /DeviceRGB ...]
            if (cs.IsArray && cs.Count > 0)
            {
                string arrayName = _reader.Resolve(cs[0]).AsName();
                if (arrayName.StartsWith("/")) arrayName = arrayName.Substring(1);

                if (arrayName == "ICCBased" && cs.Count > 1)
                {
                    var iccStream = _reader.Resolve(cs[1]);
                    int n = (int)_reader.Resolve(iccStream["N"]).AsInt();
                    if (n == 1) return "DeviceGray";
                    if (n == 4) return "DeviceCMYK";
                    return "DeviceRGB";
                }

                if (arrayName == "Indexed") return "Indexed";
                if (arrayName == "CalGray") return "DeviceGray";
                if (arrayName == "CalRGB") return "DeviceRGB";

                return arrayName;
            }

            return "DeviceRGB";
        }

        private SKBitmap? CreateBitmapFromRawPixels(byte[] data, int width, int height,
            string colorSpace, int bitsPerComponent, PdfObj imageDict)
        {
            if (colorSpace == "DeviceGray" || colorSpace == "CalGray")
                return CreateGrayBitmap(data, width, height, bitsPerComponent);
            if (colorSpace == "DeviceCMYK")
                return CreateCmykBitmap(data, width, height);
            if (colorSpace == "Indexed")
                return CreateIndexedBitmap(data, width, height, bitsPerComponent, imageDict);

            // DeviceRGB default
            return CreateRgbBitmap(data, width, height, bitsPerComponent);
        }

        private SKBitmap CreateRgbBitmap(byte[] data, int width, int height, int bitsPerComponent)
        {
            var bitmap = new SKBitmap(width, height, SKColorType.Rgba8888, SKAlphaType.Premul);
            int stride = (width * 3 * bitsPerComponent + 7) / 8;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int offset = y * stride + x * 3;
                    if (offset + 2 >= data.Length) break;
                    bitmap.SetPixel(x, y, new SKColor(data[offset], data[offset + 1], data[offset + 2]));
                }
            }
            return bitmap;
        }

        private SKBitmap CreateGrayBitmap(byte[] data, int width, int height, int bitsPerComponent)
        {
            var bitmap = new SKBitmap(width, height, SKColorType.Rgba8888, SKAlphaType.Premul);

            if (bitsPerComponent == 8)
            {
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        int offset = y * width + x;
                        if (offset >= data.Length) break;
                        byte g = data[offset];
                        bitmap.SetPixel(x, y, new SKColor(g, g, g));
                    }
                }
            }
            else if (bitsPerComponent == 1)
            {
                int stride = (width + 7) / 8;
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        int byteOffset = y * stride + x / 8;
                        if (byteOffset >= data.Length) break;
                        int bit = (data[byteOffset] >> (7 - (x % 8))) & 1;
                        byte g = bit == 1 ? (byte)255 : (byte)0;
                        bitmap.SetPixel(x, y, new SKColor(g, g, g));
                    }
                }
            }
            else if (bitsPerComponent == 4)
            {
                int stride = (width + 1) / 2;
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        int byteOffset = y * stride + x / 2;
                        if (byteOffset >= data.Length) break;
                        int nibble = (x % 2 == 0)
                            ? (data[byteOffset] >> 4) & 0xF
                            : data[byteOffset] & 0xF;
                        byte g = (byte)(nibble * 17); // scale 0-15 to 0-255
                        bitmap.SetPixel(x, y, new SKColor(g, g, g));
                    }
                }
            }

            return bitmap;
        }

        private SKBitmap CreateCmykBitmap(byte[] data, int width, int height)
        {
            var bitmap = new SKBitmap(width, height, SKColorType.Rgba8888, SKAlphaType.Premul);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int offset = y * width * 4 + x * 4;
                    if (offset + 3 >= data.Length) break;

                    float c = data[offset] / 255f;
                    float m = data[offset + 1] / 255f;
                    float yy = data[offset + 2] / 255f;
                    float k = data[offset + 3] / 255f;

                    byte r = ClampByte((1 - c) * (1 - k) * 255f);
                    byte g = ClampByte((1 - m) * (1 - k) * 255f);
                    byte b = ClampByte((1 - yy) * (1 - k) * 255f);
                    bitmap.SetPixel(x, y, new SKColor(r, g, b));
                }
            }
            return bitmap;
        }

        private SKBitmap? CreateIndexedBitmap(byte[] data, int width, int height,
            int bitsPerComponent, PdfObj imageDict)
        {
            var cs = _reader.Resolve(imageDict["ColorSpace"]);
            if (!cs.IsArray || cs.Count < 4) return null;

            // [/Indexed base hival lookup]
            int hival = (int)_reader.Resolve(cs[2]).AsInt();
            var lookupObj = _reader.Resolve(cs[3]);

            byte[] palette;
            if (lookupObj.IsStream)
                palette = _reader.GetStreamBytes(lookupObj) ?? Array.Empty<byte>();
            else
                palette = lookupObj.AsBytes();

            if (palette.Length == 0) return null;

            string baseCs = _reader.Resolve(cs[1]).AsName();
            if (baseCs.StartsWith("/")) baseCs = baseCs.Substring(1);
            int componentsPerColor = baseCs == "DeviceCMYK" ? 4 : (baseCs == "DeviceGray" || baseCs == "CalGray" ? 1 : 3);

            var bitmap = new SKBitmap(width, height, SKColorType.Rgba8888, SKAlphaType.Premul);

            if (bitsPerComponent == 8)
            {
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        int offset = y * width + x;
                        if (offset >= data.Length) break;
                        int index = data[offset];
                        bitmap.SetPixel(x, y, GetPaletteColor(palette, index, componentsPerColor));
                    }
                }
            }
            else if (bitsPerComponent == 4)
            {
                int stride = (width + 1) / 2;
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        int byteOffset = y * stride + x / 2;
                        if (byteOffset >= data.Length) break;
                        int index = (x % 2 == 0)
                            ? (data[byteOffset] >> 4) & 0xF
                            : data[byteOffset] & 0xF;
                        bitmap.SetPixel(x, y, GetPaletteColor(palette, index, componentsPerColor));
                    }
                }
            }
            else if (bitsPerComponent == 1)
            {
                int stride = (width + 7) / 8;
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        int byteOffset = y * stride + x / 8;
                        if (byteOffset >= data.Length) break;
                        int index = (data[byteOffset] >> (7 - (x % 8))) & 1;
                        bitmap.SetPixel(x, y, GetPaletteColor(palette, index, componentsPerColor));
                    }
                }
            }

            return bitmap;
        }

        private SKColor GetPaletteColor(byte[] palette, int index, int components)
        {
            int offset = index * components;
            if (components == 1)
            {
                byte g = offset < palette.Length ? palette[offset] : (byte)0;
                return new SKColor(g, g, g);
            }
            if (components == 4 && offset + 3 < palette.Length)
            {
                float c = palette[offset] / 255f;
                float m = palette[offset + 1] / 255f;
                float y = palette[offset + 2] / 255f;
                float k = palette[offset + 3] / 255f;
                return new SKColor(
                    ClampByte((1 - c) * (1 - k) * 255f),
                    ClampByte((1 - m) * (1 - k) * 255f),
                    ClampByte((1 - y) * (1 - k) * 255f));
            }
            // RGB
            if (offset + 2 < palette.Length)
                return new SKColor(palette[offset], palette[offset + 1], palette[offset + 2]);
            return SKColors.Black;
        }

        private void DrawFormXObject(SKCanvas canvas, GraphicsState state,
            Stack<GraphicsState> stateStack, PdfObj formDict, PdfObj pageDict)
        {
            byte[] formData = _reader.GetStreamBytes(formDict);
            if (formData == null || formData.Length == 0) return;

            canvas.Save();

            // Apply form matrix if present
            var matrixObj = _reader.Resolve(formDict["Matrix"]);
            if (!matrixObj.IsNull && matrixObj.IsArray && matrixObj.Count >= 6)
            {
                float a = matrixObj[0].AsFloat();
                float b = matrixObj[1].AsFloat();
                float c = matrixObj[2].AsFloat();
                float d = matrixObj[3].AsFloat();
                float e = matrixObj[4].AsFloat();
                float f = matrixObj[5].AsFloat();
                var formMatrix = new SKMatrix(a, c, e, b, d, f, 0, 0, 1);
                canvas.Concat(formMatrix);
            }

            // Apply BBox clipping
            var bbox = _reader.Resolve(formDict["BBox"]);
            if (!bbox.IsNull && bbox.IsArray && bbox.Count >= 4)
            {
                var rect = PdfRect.FromArray(bbox);
                canvas.ClipRect(new SKRect(rect.Left, rect.Bottom, rect.Right, rect.Top));
            }

            // Use the form's own resources if it has them, otherwise fall back to page resources
            var formResources = _reader.Resolve(formDict["Resources"]);
            PdfObj effectivePage;
            if (!formResources.IsNull)
            {
                // Create a synthetic page dict with the form's resources
                effectivePage = formDict;
            }
            else
            {
                effectivePage = pageDict;
            }

            // Parse and execute the form's content stream
            var formState = new GraphicsState();
            formState.CopyFrom(state);
            var formStateStack = new Stack<GraphicsState>();
            var formPath = new SKPath();
            bool formClipNZ = false, formClipEO = false;

            var operators = ParseContentStream(formData);
            foreach (var op in operators)
            {
                try
                {
                    ExecuteOperator(canvas, op, formState, formStateStack, formPath,
                        ref formClipNZ, ref formClipEO, effectivePage);
                }
                catch
                {
                    // Skip bad operators
                }
            }

            formPath.Dispose();
            canvas.Restore();
        }

        #endregion

        #region Helpers

        private static float GetFloat(List<object> args, int index)
        {
            if (index >= args.Count) return 0f;
            if (args[index] is double d) return (float)d;
            if (args[index] is float f) return f;
            if (args[index] is int i) return i;
            return 0f;
        }

        private static string GetNameStr(List<object> args, int index)
        {
            if (index >= args.Count) return "";
            if (args[index] is string s)
            {
                if (s.StartsWith("/")) return s.Substring(1);
                return s;
            }
            return "";
        }

        private static SKStrokeCap GetLineCap(int value)
        {
            switch (value)
            {
                case 0: return SKStrokeCap.Butt;
                case 1: return SKStrokeCap.Round;
                case 2: return SKStrokeCap.Square;
                default: return SKStrokeCap.Butt;
            }
        }

        private static SKStrokeJoin GetLineJoin(int value)
        {
            switch (value)
            {
                case 0: return SKStrokeJoin.Miter;
                case 1: return SKStrokeJoin.Round;
                case 2: return SKStrokeJoin.Bevel;
                default: return SKStrokeJoin.Miter;
            }
        }

        private static SKBlendMode GetSkBlendMode(string mode)
        {
            switch (mode)
            {
                case "Normal":
                case "Compatible": return SKBlendMode.SrcOver;
                case "Multiply": return SKBlendMode.Multiply;
                case "Screen": return SKBlendMode.Screen;
                case "Overlay": return SKBlendMode.Overlay;
                case "Darken": return SKBlendMode.Darken;
                case "Lighten": return SKBlendMode.Lighten;
                case "ColorDodge": return SKBlendMode.ColorDodge;
                case "ColorBurn": return SKBlendMode.ColorBurn;
                case "HardLight": return SKBlendMode.HardLight;
                case "SoftLight": return SKBlendMode.SoftLight;
                case "Difference": return SKBlendMode.Difference;
                case "Exclusion": return SKBlendMode.Exclusion;
                case "Hue": return SKBlendMode.Hue;
                case "Saturation": return SKBlendMode.Saturation;
                case "Color": return SKBlendMode.Color;
                case "Luminosity": return SKBlendMode.Luminosity;
                default: return SKBlendMode.SrcOver;
            }
        }

        #endregion

        #region Graphics State

        private sealed class GraphicsState
        {
            public SKColor FillColor = SKColors.Black;
            public SKColor StrokeColor = SKColors.Black;
            public float LineWidth = 1f;
            public SKStrokeCap LineCap = SKStrokeCap.Butt;
            public SKStrokeJoin LineJoin = SKStrokeJoin.Miter;
            public float MiterLimit = 10f;
            public float[]? DashArray;
            public float DashPhase;
            public float FillAlpha = 1f;
            public float StrokeAlpha = 1f;
            public string FillColorSpace = "DeviceRGB";
            public string StrokeColorSpace = "DeviceRGB";

            // Text state
            public string FontName = "";
            public float FontSize = 12f;
            public SKTypeface? Typeface;
            public PdfObj? FontDict;
            public Dictionary<int, string>? ToUnicodeMap;
            public Dictionary<int, string>? Encoding;
            public Dictionary<int, float>? FontWidths; // glyph widths in 1/1000 of text space
            public int FontFirstChar;
            public float FontDefaultWidth = 1000f;
            public bool IsCIDFont;
            public float HorizontalScaling = 100f; // Tz: percentage (100 = normal)
            public string BlendMode = "Normal";
            public SKMatrix TextMatrix = SKMatrix.Identity;
            public SKMatrix TextLineMatrix = SKMatrix.Identity;
            public float CharSpacing;
            public float WordSpacing;
            public float TextLeading;
            public int TextRenderMode;
            public float TextRise;

            // Path state
            public float CurrentX;
            public float CurrentY;

            public GraphicsState Clone()
            {
                return new GraphicsState
                {
                    FillColor = FillColor,
                    StrokeColor = StrokeColor,
                    LineWidth = LineWidth,
                    LineCap = LineCap,
                    LineJoin = LineJoin,
                    MiterLimit = MiterLimit,
                    DashArray = DashArray != null ? (float[])DashArray.Clone() : null,
                    DashPhase = DashPhase,
                    FillAlpha = FillAlpha,
                    StrokeAlpha = StrokeAlpha,
                    FillColorSpace = FillColorSpace,
                    StrokeColorSpace = StrokeColorSpace,
                    FontName = FontName,
                    FontSize = FontSize,
                    Typeface = Typeface,
                    FontDict = FontDict,
                    ToUnicodeMap = ToUnicodeMap,
                    Encoding = Encoding,
                    FontWidths = FontWidths,
                    FontFirstChar = FontFirstChar,
                    FontDefaultWidth = FontDefaultWidth,
                    IsCIDFont = IsCIDFont,
                    HorizontalScaling = HorizontalScaling,
                    BlendMode = BlendMode,
                    TextMatrix = TextMatrix,
                    TextLineMatrix = TextLineMatrix,
                    CharSpacing = CharSpacing,
                    WordSpacing = WordSpacing,
                    TextLeading = TextLeading,
                    TextRenderMode = TextRenderMode,
                    TextRise = TextRise,
                    CurrentX = CurrentX,
                    CurrentY = CurrentY,
                };
            }

            public void CopyFrom(GraphicsState other)
            {
                FillColor = other.FillColor;
                StrokeColor = other.StrokeColor;
                LineWidth = other.LineWidth;
                LineCap = other.LineCap;
                LineJoin = other.LineJoin;
                MiterLimit = other.MiterLimit;
                DashArray = other.DashArray != null ? (float[])other.DashArray.Clone() : null;
                DashPhase = other.DashPhase;
                FillAlpha = other.FillAlpha;
                StrokeAlpha = other.StrokeAlpha;
                FillColorSpace = other.FillColorSpace;
                StrokeColorSpace = other.StrokeColorSpace;
                FontName = other.FontName;
                FontSize = other.FontSize;
                Typeface = other.Typeface;
                FontDict = other.FontDict;
                ToUnicodeMap = other.ToUnicodeMap;
                Encoding = other.Encoding;
                FontWidths = other.FontWidths;
                FontFirstChar = other.FontFirstChar;
                FontDefaultWidth = other.FontDefaultWidth;
                IsCIDFont = other.IsCIDFont;
                HorizontalScaling = other.HorizontalScaling;
                BlendMode = other.BlendMode;
                TextMatrix = other.TextMatrix;
                TextLineMatrix = other.TextLineMatrix;
                CharSpacing = other.CharSpacing;
                WordSpacing = other.WordSpacing;
                TextLeading = other.TextLeading;
                TextRenderMode = other.TextRenderMode;
                TextRise = other.TextRise;
                CurrentX = other.CurrentX;
                CurrentY = other.CurrentY;
            }
        }

        #endregion

        // Uses Rend.Pdf.Reading.PdfRect
    }
}
