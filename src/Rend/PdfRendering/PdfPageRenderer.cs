#nullable enable
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Rend.Pdf.Parsing;
using SkiaSharp;

namespace Rend.PdfRendering
{
    internal sealed class PdfPageRenderer : IDisposable
    {
        private readonly PdfDocumentReader _reader;
        private readonly ContentStreamParser _parser = new ContentStreamParser();
        private readonly Dictionary<int, SKTypeface> _typefaceCache = new Dictionary<int, SKTypeface>();
        private readonly List<string> _warnings = new List<string>();

        public IReadOnlyList<string> Warnings => _warnings;

        public PdfPageRenderer(PdfDocumentReader reader)
        {
            _reader = reader ?? throw new ArgumentNullException(nameof(reader));
        }

        public void Dispose()
        {
            foreach (var typeface in _typefaceCache.Values)
            {
                typeface.Dispose();
            }
            _typefaceCache.Clear();
        }

        public SKBitmap RenderPage(int pageIndex, float scale)
        {
            _warnings.Clear();
            var pageDict = _reader.Resolve(_reader.GetPage(pageIndex));
            var mediaBox = GetPageMediaBox(pageDict);
            var cropBox = GetPageCropBox(pageDict, mediaBox);

            float pageWidth = cropBox.Right - cropBox.Left;
            float pageHeight = cropBox.Top - cropBox.Bottom;

            int rotation = GetPageRotation(pageDict);
            bool swapDimensions = rotation == 90 || rotation == 270;
            float bitmapWidth = swapDimensions ? pageHeight : pageWidth;
            float bitmapHeight = swapDimensions ? pageWidth : pageHeight;

            int pixelWidth = Math.Max(1, (int)(bitmapWidth * scale + 0.5f));
            int pixelHeight = Math.Max(1, (int)(bitmapHeight * scale + 0.5f));

            // Fetch and parse the content stream before allocating native resources, so a
            // content/parse failure cannot leak the page bitmap or path.
            var contentBytes = GetPageContentBytes(pageDict);
            var operators = _parser.Parse(contentBytes);

            var bitmap = new SKBitmap(pixelWidth, pixelHeight, SKColorType.Rgba8888, SKAlphaType.Premul);
            using var canvas = new SKCanvas(bitmap);
            canvas.Clear(SKColors.White);

            // Apply rotation before the PDF coordinate transform
            if (rotation == 90)
            {
                canvas.Translate(pixelWidth, 0);
                canvas.RotateDegrees(90);
            }
            else if (rotation == 180)
            {
                canvas.Translate(pixelWidth, pixelHeight);
                canvas.RotateDegrees(180);
            }
            else if (rotation == 270)
            {
                canvas.Translate(0, pixelHeight);
                canvas.RotateDegrees(270);
            }

            canvas.Scale(scale, -scale);
            canvas.Translate(-cropBox.Left, -cropBox.Top);

            var state = new GraphicsState();
            var stateStack = new Stack<GraphicsState>();
            var path = new SKPath();

            foreach (var op in operators)
            {
                try
                {
                    ExecuteOperator(canvas, op, state, stateStack, path, pageDict);
                }
                catch (Exception ex)
                {
                    _warnings.Add($"Operator '{op.Name}': {ex.Message}");
                }
            }

            path.Dispose();
            state.TextClipPath?.Dispose();
            state.TextClipPath = null;
            return bitmap;
        }

        private PdfRect GetPageMediaBox(PdfObj pageDict)
        {
            var mediaBox = _reader.Resolve(pageDict["MediaBox"]);
            if (!mediaBox.IsNull)
            {
                return PdfRect.FromArray(mediaBox);
            }

            var parent = _reader.Resolve(pageDict["Parent"]);
            while (!parent.IsNull)
            {
                mediaBox = _reader.Resolve(parent["MediaBox"]);
                if (!mediaBox.IsNull)
                {
                    return PdfRect.FromArray(mediaBox);
                }
                parent = _reader.Resolve(parent["Parent"]);
            }

            return new PdfRect(0, 0, 612, 792);
        }

        private PdfRect GetPageCropBox(PdfObj pageDict, PdfRect mediaBox)
        {
            var cropBox = _reader.Resolve(pageDict["CropBox"]);
            if (!cropBox.IsNull)
            {
                return PdfRect.FromArray(cropBox);
            }

            var parent = _reader.Resolve(pageDict["Parent"]);
            while (!parent.IsNull)
            {
                cropBox = _reader.Resolve(parent["CropBox"]);
                if (!cropBox.IsNull)
                {
                    return PdfRect.FromArray(cropBox);
                }
                parent = _reader.Resolve(parent["Parent"]);
            }

            return mediaBox;
        }

        private void OpSetColorSpace(GraphicsState state, List<object> args, PdfObj pageDict, bool isFill)
        {
            string rawName = ContentStreamParser.GetNameStr(args, 0);
            string resolved = ResolveColorSpaceName(rawName, pageDict);

            if (isFill)
            {
                state.FillColorSpace = resolved;
                state.FillSeparationName = "";
            }
            else
            {
                state.StrokeColorSpace = resolved;
                state.StrokeSeparationName = "";
            }

            // For Separation color spaces, store the colorant name
            if (resolved.StartsWith("Separation:"))
            {
                string separationName = resolved.Substring("Separation:".Length);
                if (isFill)
                {
                    state.FillSeparationName = separationName;
                }
                else
                {
                    state.StrokeSeparationName = separationName;
                }
            }
        }

        private bool IsOptionalContentVisible(PdfObj ocEntry)
        {
            // [SPEC §8.11] Optional Content:
            // /OC can be an OCG dict or OCMD dict.
            // For screen rendering, check if the OCG is in the catalog's default "ON" list.
            var ocType = _reader.Resolve(ocEntry["Type"]).AsName();

            if (ocType == "OCMD" || ocType == "/OCMD")
            {
                // Optional Content Membership Dictionary — check referenced OCGs
                var ocgs = _reader.Resolve(ocEntry["OCGs"]);
                if (!ocgs.IsNull)
                {
                    // Check if the referenced OCG is visible
                    PdfObj ocgToCheck = ocgs.IsArray ? (ocgs.Count > 0 ? _reader.Resolve(ocgs[0]) : PdfObj.Null) : _reader.Resolve(ocgs);
                    return IsOcgVisible(ocgToCheck);
                }
                return true;
            }

            if (ocType == "OCG" || ocType == "/OCG")
            {
                return IsOcgVisible(ocEntry);
            }

            return true;
        }

        private bool IsOcgVisible(PdfObj ocgDict)
        {
            if (ocgDict.IsNull)
            {
                return true;
            }

            // Check the catalog's OCProperties/D/ON array
            var catalog = _reader.Catalog;
            var ocProperties = _reader.Resolve(catalog["OCProperties"]);
            if (ocProperties.IsNull)
            {
                return true;
            }

            var defaultConfig = _reader.Resolve(ocProperties["D"]);
            if (defaultConfig.IsNull)
            {
                return true;
            }

            // BaseState: "ON" (default) or "OFF"
            string baseState = _reader.Resolve(defaultConfig["BaseState"]).AsName();
            if (baseState == "OFF" || baseState == "/OFF")
            {
                // Check if this OCG is in the ON array
                var onArray = _reader.Resolve(defaultConfig["ON"]);
                if (onArray.IsNull || !onArray.IsArray)
                {
                    return false;
                }
                // Check if ocgDict's reference matches any in the ON array
                for (int i = 0; i < onArray.Count; i++)
                {
                    if (_reader.Resolve(onArray[i]) == ocgDict)
                    {
                        return true;
                    }
                }
                return false;
            }

            // BaseState is "ON" — check if this OCG is in the OFF array
            var offArray = _reader.Resolve(defaultConfig["OFF"]);
            if (!offArray.IsNull && offArray.IsArray)
            {
                for (int i = 0; i < offArray.Count; i++)
                {
                    if (_reader.Resolve(offArray[i]) == ocgDict)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private string ResolveColorSpaceName(string name, PdfObj pageDict)
        {
            // Standard device color spaces
            if (name == "DeviceGray" || name == "DeviceRGB" || name == "DeviceCMYK" ||
                name == "Gray" || name == "RGB" || name == "CMYK")
            {
                return name;
            }

            // Resolve named color space from resources
            var resources = _reader.Resolve(pageDict["Resources"]);
            var colorSpaces = _reader.Resolve(resources["ColorSpace"]);
            if (colorSpaces.IsNull)
            {
                return name;
            }

            var csObj = _reader.Resolve(colorSpaces[name]);
            if (csObj.IsNull)
            {
                _warnings.Add($"Color space '{name}' not found in resources");
                return name;
            }

            // Color space can be a name or an array [name, params...]
            string csName;
            if (csObj.IsName)
            {
                csName = csObj.AsName();
            }
            else if (csObj.IsArray && csObj.Count > 0)
            {
                csName = _reader.Resolve(csObj[0]).AsName();
            }
            else
            {
                return name;
            }

            if (csName.StartsWith("/"))
            {
                csName = csName.Substring(1);
            }

            // Map to device color space
            switch (csName)
            {
                case "DeviceGray":
                case "CalGray":
                    return "DeviceGray";
                case "DeviceRGB":
                case "CalRGB":
                    return "DeviceRGB";
                case "DeviceCMYK":
                    return "DeviceCMYK";
                case "ICCBased":
                    if (csObj.IsArray && csObj.Count > 1)
                    {
                        var iccStream = _reader.Resolve(csObj[1]);
                        int components = (int)_reader.Resolve(iccStream["N"]).AsInt();
                        if (components == 1) { return "DeviceGray"; }
                        if (components == 4) { return "DeviceCMYK"; }
                        return "DeviceRGB";
                    }
                    return "DeviceRGB";
                case "Separation":
                    if (csObj.IsArray && csObj.Count > 1)
                    {
                        string colorantName = _reader.Resolve(csObj[1]).AsName();
                        if (colorantName.StartsWith("/"))
                        {
                            colorantName = colorantName.Substring(1);
                        }
                        return "Separation:" + colorantName;
                    }
                    return "Separation:";
                case "DeviceN":
                    return name;
                default:
                    return csName;
            }
        }

        private int GetPageRotation(PdfObj pageDict)
        {
            var rotate = _reader.Resolve(pageDict["Rotate"]);
            if (!rotate.IsNull)
            {
                int value = (int)rotate.AsInt();
                if (value == 90 || value == 180 || value == 270)
                {
                    return value;
                }
            }

            var parent = _reader.Resolve(pageDict["Parent"]);
            while (!parent.IsNull)
            {
                rotate = _reader.Resolve(parent["Rotate"]);
                if (!rotate.IsNull)
                {
                    int value = (int)rotate.AsInt();
                    if (value == 90 || value == 180 || value == 270)
                    {
                        return value;
                    }
                }
                parent = _reader.Resolve(parent["Parent"]);
            }

            return 0;
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
                using var memoryStream = new MemoryStream();
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

        private void ExecuteOperator(SKCanvas canvas, ContentStreamOperator op, GraphicsState state,
            Stack<GraphicsState> stateStack, SKPath path, PdfObj pageDict)
        {
            var args = op.Operands;

            switch (op.Name)
            {
                case "q": PushState(canvas, state, stateStack); break;
                case "Q": PopState(canvas, state, stateStack); break;
                case "cm": OpConcat(canvas, args); break;
                case "w": state.LineWidth = ContentStreamParser.GetFloat(args, 0); break;
                case "J": state.LineCap = GetLineCap((int)ContentStreamParser.GetFloat(args, 0)); break;
                case "j": state.LineJoin = GetLineJoin((int)ContentStreamParser.GetFloat(args, 0)); break;
                case "M": state.MiterLimit = ContentStreamParser.GetFloat(args, 0); break;
                case "d": OpSetDash(state, args); break;
                case "gs": OpSetExtGState(state, args, pageDict); break;

                case "m": path.MoveTo(ContentStreamParser.GetFloat(args, 0), ContentStreamParser.GetFloat(args, 1));
                    state.CurrentX = ContentStreamParser.GetFloat(args, 0); state.CurrentY = ContentStreamParser.GetFloat(args, 1); break;
                case "l": path.LineTo(ContentStreamParser.GetFloat(args, 0), ContentStreamParser.GetFloat(args, 1));
                    state.CurrentX = ContentStreamParser.GetFloat(args, 0); state.CurrentY = ContentStreamParser.GetFloat(args, 1); break;
                case "c":
                    path.CubicTo(ContentStreamParser.GetFloat(args, 0), ContentStreamParser.GetFloat(args, 1),
                        ContentStreamParser.GetFloat(args, 2), ContentStreamParser.GetFloat(args, 3),
                        ContentStreamParser.GetFloat(args, 4), ContentStreamParser.GetFloat(args, 5));
                    state.CurrentX = ContentStreamParser.GetFloat(args, 4); state.CurrentY = ContentStreamParser.GetFloat(args, 5);
                    break;
                case "v":
                    path.CubicTo(state.CurrentX, state.CurrentY,
                        ContentStreamParser.GetFloat(args, 0), ContentStreamParser.GetFloat(args, 1),
                        ContentStreamParser.GetFloat(args, 2), ContentStreamParser.GetFloat(args, 3));
                    state.CurrentX = ContentStreamParser.GetFloat(args, 2); state.CurrentY = ContentStreamParser.GetFloat(args, 3);
                    break;
                case "y":
                    path.CubicTo(ContentStreamParser.GetFloat(args, 0), ContentStreamParser.GetFloat(args, 1),
                        ContentStreamParser.GetFloat(args, 2), ContentStreamParser.GetFloat(args, 3),
                        ContentStreamParser.GetFloat(args, 2), ContentStreamParser.GetFloat(args, 3));
                    state.CurrentX = ContentStreamParser.GetFloat(args, 2); state.CurrentY = ContentStreamParser.GetFloat(args, 3);
                    break;
                case "h": path.Close(); break;
                case "re": OpRect(path, state, args); break;

                case "S": PaintPath(canvas, path, state, false, true, false); break;
                case "s": path.Close(); PaintPath(canvas, path, state, false, true, false); break;
                case "f":
                case "F": PaintPath(canvas, path, state, true, false, false); break;
                case "f*": PaintPath(canvas, path, state, true, false, true); break;
                case "B": PaintPath(canvas, path, state, true, true, false); break;
                case "B*": PaintPath(canvas, path, state, true, true, true); break;
                case "b": path.Close(); PaintPath(canvas, path, state, true, true, false); break;
                case "b*": path.Close(); PaintPath(canvas, path, state, true, true, true); break;
                case "n": ApplyPendingClip(canvas, path, state); path.Reset(); break;

                case "W": state.PendingClipNonZero = true; break;
                case "W*": state.PendingClipEvenOdd = true; break;

                case "g": state.FillColor = PdfColorHelper.GrayToColor(ContentStreamParser.GetFloat(args, 0)); break;
                case "G": state.StrokeColor = PdfColorHelper.GrayToColor(ContentStreamParser.GetFloat(args, 0)); break;
                case "rg": state.FillColor = PdfColorHelper.RgbToColor(ContentStreamParser.GetFloat(args, 0), ContentStreamParser.GetFloat(args, 1), ContentStreamParser.GetFloat(args, 2)); break;
                case "RG": state.StrokeColor = PdfColorHelper.RgbToColor(ContentStreamParser.GetFloat(args, 0), ContentStreamParser.GetFloat(args, 1), ContentStreamParser.GetFloat(args, 2)); break;
                case "k": state.FillColor = PdfColorHelper.CmykToColor(ContentStreamParser.GetFloat(args, 0), ContentStreamParser.GetFloat(args, 1), ContentStreamParser.GetFloat(args, 2), ContentStreamParser.GetFloat(args, 3)); break;
                case "K": state.StrokeColor = PdfColorHelper.CmykToColor(ContentStreamParser.GetFloat(args, 0), ContentStreamParser.GetFloat(args, 1), ContentStreamParser.GetFloat(args, 2), ContentStreamParser.GetFloat(args, 3)); break;
                case "cs": OpSetColorSpace(state, args, pageDict, true); break;
                case "CS": OpSetColorSpace(state, args, pageDict, false); break;
                case "sc":
                case "scn": PdfColorHelper.OpSetColor(state, args, true); break;
                case "SC":
                case "SCN": PdfColorHelper.OpSetColor(state, args, false); break;

                case "BT": OpBeginText(state); break;
                case "ET": OpEndText(canvas, state); break;
                case "Tf": OpSetFont(state, args, pageDict); break;
                case "Td": OpTextMove(state, args); break;
                case "TD": OpTextMoveTD(state, args); break;
                case "Tm": OpSetTextMatrix(state, args); break;
                case "T*": OpTextNextLine(state); break;
                case "Tj": OpShowText(canvas, state, args); break;
                case "TJ": OpShowTextArray(canvas, state, args); break;
                case "'": OpTextNextLine(state); OpShowText(canvas, state, args); break;
                case "\"": OpShowTextQuoteDbl(canvas, state, args); break;
                case "Tc": state.CharSpacing = ContentStreamParser.GetFloat(args, 0); break;
                case "Tw": state.WordSpacing = ContentStreamParser.GetFloat(args, 0); break;
                case "TL": state.TextLeading = ContentStreamParser.GetFloat(args, 0); break;
                case "Tr": state.TextRenderMode = (int)ContentStreamParser.GetFloat(args, 0); break;
                case "Ts": state.TextRise = ContentStreamParser.GetFloat(args, 0); break;
                case "Tz": state.HorizontalScaling = ContentStreamParser.GetFloat(args, 0); break;

                case "Do": OpDoXObject(canvas, state, stateStack, args, pageDict); break;

                case "BI_IMAGE":
                    if (args.Count > 0 && args[0] is InlineImageData img)
                    {
                        PdfImageRenderer.DrawInlineImage(canvas, state, img);
                    }
                    break;

                case "sh": OpPaintShading(canvas, state, args, pageDict); break;
                case "i": break;
                case "ri": break;
                case "BMC": break;
                case "BDC": break;
                case "EMC": break;
                case "MP": break;
                case "DP": break;
                default: break;
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
                state.TextClipPath?.Dispose();
                var restored = stateStack.Pop();
                state.CopyFrom(restored);
                canvas.Restore();
            }
        }

        private void OpConcat(SKCanvas canvas, List<object> args)
        {
            float a = ContentStreamParser.GetFloat(args, 0), b = ContentStreamParser.GetFloat(args, 1);
            float c = ContentStreamParser.GetFloat(args, 2), d = ContentStreamParser.GetFloat(args, 3);
            float e = ContentStreamParser.GetFloat(args, 4), f = ContentStreamParser.GetFloat(args, 5);

            var matrix = new SKMatrix(a, c, e, b, d, f, 0, 0, 1);
            canvas.Concat(matrix);
        }

        private void OpSetDash(GraphicsState state, List<object> args)
        {
            if (args.Count >= 2 && args[0] is List<object> dashList)
            {
                state.DashArray = dashList.Select(x => x is double d ? (float)d : 0f).ToArray();
                state.DashPhase = ContentStreamParser.GetFloat(args, 1);
            }
        }

        private void OpSetExtGState(GraphicsState state, List<object> args, PdfObj pageDict)
        {
            string gsName = ContentStreamParser.GetNameStr(args, 0);
            if (string.IsNullOrEmpty(gsName))
            {
                return;
            }

            var resources = _reader.Resolve(pageDict["Resources"]);
            var extGState = _reader.Resolve(resources["ExtGState"]);
            var gsDict = _reader.Resolve(extGState[gsName]);
            if (gsDict.IsNull)
            {
                return;
            }

            if (gsDict.ContainsKey("ca"))
            {
                state.FillAlpha = gsDict["ca"].AsFloat();
            }
            if (gsDict.ContainsKey("CA"))
            {
                state.StrokeAlpha = gsDict["CA"].AsFloat();
            }
            if (gsDict.ContainsKey("LW"))
            {
                state.LineWidth = gsDict["LW"].AsFloat();
            }
            if (gsDict.ContainsKey("LC"))
            {
                state.LineCap = GetLineCap((int)gsDict["LC"].AsInt());
            }
            if (gsDict.ContainsKey("LJ"))
            {
                state.LineJoin = GetLineJoin((int)gsDict["LJ"].AsInt());
            }
            if (gsDict.ContainsKey("ML"))
            {
                state.MiterLimit = gsDict["ML"].AsFloat();
            }
            if (gsDict.ContainsKey("BM"))
            {
                var blendMode = _reader.Resolve(gsDict["BM"]).AsName();
                if (blendMode.StartsWith("/"))
                {
                    blendMode = blendMode.Substring(1);
                }
                state.BlendMode = blendMode;
            }
        }

        private void OpRect(SKPath path, GraphicsState state, List<object> args)
        {
            float x = ContentStreamParser.GetFloat(args, 0), y = ContentStreamParser.GetFloat(args, 1);
            float w = ContentStreamParser.GetFloat(args, 2), h = ContentStreamParser.GetFloat(args, 3);
            path.MoveTo(x, y);
            path.LineTo(x + w, y);
            path.LineTo(x + w, y + h);
            path.LineTo(x, y + h);
            path.Close();
            state.CurrentX = x;
            state.CurrentY = y;
        }

        private void PaintPath(SKCanvas canvas, SKPath path, GraphicsState state,
            bool fill, bool stroke, bool evenOdd)
        {
            ApplyPendingClip(canvas, path, state);

            if (fill)
            {
                path.FillType = evenOdd ? SKPathFillType.EvenOdd : SKPathFillType.Winding;
                using var paint = new SKPaint
                {
                    Color = PdfColorHelper.WithAlpha(state.FillColor, state.FillAlpha),
                    Style = SKPaintStyle.Fill,
                    IsAntialias = true,
                    BlendMode = PdfColorHelper.GetSkBlendMode(state.BlendMode),
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

        private void ApplyPendingClip(SKCanvas canvas, SKPath path, GraphicsState state)
        {
            if (state.PendingClipNonZero || state.PendingClipEvenOdd)
            {
                var clipPath = new SKPath(path);
                clipPath.FillType = state.PendingClipEvenOdd ? SKPathFillType.EvenOdd : SKPathFillType.Winding;
                canvas.ClipPath(clipPath, SKClipOperation.Intersect, true);
                clipPath.Dispose();
                state.PendingClipNonZero = false;
                state.PendingClipEvenOdd = false;
            }
        }

        private SKPaint CreateStrokePaint(GraphicsState state)
        {
            var paint = new SKPaint
            {
                Color = PdfColorHelper.WithAlpha(state.StrokeColor, state.StrokeAlpha),
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

        private void OpBeginText(GraphicsState state)
        {
            state.TextMatrix = SKMatrix.Identity;
            state.TextLineMatrix = SKMatrix.Identity;
        }

        private void OpEndText(SKCanvas canvas, GraphicsState state)
        {
            if (state.TextClipPath != null && !state.TextClipPath.IsEmpty)
            {
                canvas.ClipPath(state.TextClipPath, SKClipOperation.Intersect, true);
                state.TextClipPath.Dispose();
                state.TextClipPath = null;
            }
        }

        private void OpSetFont(GraphicsState state, List<object> args, PdfObj pageDict)
        {
            state.FontName = ContentStreamParser.GetNameStr(args, 0);
            state.FontSize = ContentStreamParser.GetFloat(args, 1);
            PdfFontResolver.ResolveFontTypeface(_reader, state, pageDict, _typefaceCache);
        }

        private void OpTextMove(GraphicsState state, List<object> args)
        {
            float tx = ContentStreamParser.GetFloat(args, 0);
            float ty = ContentStreamParser.GetFloat(args, 1);

            var translate = SKMatrix.CreateTranslation(tx, ty);
            state.TextLineMatrix = SKMatrix.Concat(state.TextLineMatrix, translate);
            state.TextMatrix = state.TextLineMatrix;
        }

        private void OpTextMoveTD(GraphicsState state, List<object> args)
        {
            float tx = ContentStreamParser.GetFloat(args, 0);
            float ty = ContentStreamParser.GetFloat(args, 1);
            state.TextLeading = -ty;

            var translate = SKMatrix.CreateTranslation(tx, ty);
            state.TextLineMatrix = SKMatrix.Concat(state.TextLineMatrix, translate);
            state.TextMatrix = state.TextLineMatrix;
        }

        private void OpSetTextMatrix(GraphicsState state, List<object> args)
        {
            float a = ContentStreamParser.GetFloat(args, 0), b = ContentStreamParser.GetFloat(args, 1);
            float c = ContentStreamParser.GetFloat(args, 2), d = ContentStreamParser.GetFloat(args, 3);
            float e = ContentStreamParser.GetFloat(args, 4), f = ContentStreamParser.GetFloat(args, 5);

            var matrix = new SKMatrix(a, c, e, b, d, f, 0, 0, 1);
            state.TextMatrix = matrix;
            state.TextLineMatrix = matrix;
        }

        private void OpTextNextLine(GraphicsState state)
        {
            float ty = -state.TextLeading;

            var translate = SKMatrix.CreateTranslation(0, ty);
            state.TextLineMatrix = SKMatrix.Concat(state.TextLineMatrix, translate);
            state.TextMatrix = state.TextLineMatrix;
        }

        private void OpShowText(SKCanvas canvas, GraphicsState state, List<object> args)
        {
            if (args.Count < 1)
            {
                return;
            }
            if (args[0] is byte[] textBytes)
            {
                DrawTextBytes(canvas, state, textBytes);
            }
        }

        private void OpShowTextArray(SKCanvas canvas, GraphicsState state, List<object> args)
        {
            if (args.Count < 1 || !(args[0] is List<object> array))
            {
                return;
            }

            foreach (var item in array)
            {
                if (item is byte[] textBytes)
                {
                    DrawTextBytes(canvas, state, textBytes);
                }
                else if (item is double num)
                {
                    float hScale = state.HorizontalScaling / 100f;
                    float adjust = (float)(-num / 1000.0) * state.FontSize * hScale;
                    var translate = SKMatrix.CreateTranslation(adjust, 0);
                    state.TextMatrix = SKMatrix.Concat(state.TextMatrix, translate);
                }
            }
        }

        private void OpShowTextQuoteDbl(SKCanvas canvas, GraphicsState state, List<object> args)
        {
            if (args.Count >= 3)
            {
                state.WordSpacing = ContentStreamParser.GetFloat(args, 0);
                state.CharSpacing = ContentStreamParser.GetFloat(args, 1);
                OpTextNextLine(state);

                if (args[2] is byte[] textBytes)
                {
                    DrawTextBytes(canvas, state, textBytes);
                }
            }
        }

        private void DrawTextBytes(SKCanvas canvas, GraphicsState state, byte[] textBytes)
        {
            var typeface = state.Typeface ?? SKTypeface.Default;
            float fontSize = Math.Abs(state.FontSize);
            float hScale = state.HorizontalScaling / 100f;
            float textRise = state.TextRise;

            int[] codes = PdfFontResolver.GetCharCodes(state, textBytes);
            string[] decodedPerCode = PdfFontResolver.DecodeTextBytesPerCode(state, textBytes);

            bool doFill = state.TextRenderMode == 0 || state.TextRenderMode == 2 ||
                          state.TextRenderMode == 4 || state.TextRenderMode == 6;
            bool doStroke = state.TextRenderMode == 1 || state.TextRenderMode == 2 ||
                            state.TextRenderMode == 5 || state.TextRenderMode == 6;
            bool doClip = state.TextRenderMode >= 4 && state.TextRenderMode <= 7;
            bool invisible = state.TextRenderMode == 3 || state.TextRenderMode == 7;

            // For non-clipping, non-stroking text (the common case 99%+), use the fast path:
            // compute device coordinates directly and batch draw without Save/SetMatrix/Restore.
            bool canUseFastPath = doFill && !doStroke && !doClip && !invisible &&
                                  state.TextMatrix.SkewX == 0 && state.TextMatrix.SkewY == 0;

            if (canUseFastPath)
            {
                DrawTextBytesFast(canvas, state, typeface, fontSize, hScale, textRise, codes, decodedPerCode);
            }
            else
            {
                DrawTextBytesFull(canvas, state, typeface, fontSize, hScale, textRise, codes, decodedPerCode,
                    doFill, doStroke, doClip, invisible);
            }
        }

        private void DrawTextBytesFast(SKCanvas canvas, GraphicsState state, SKTypeface typeface,
            float fontSize, float hScale, float textRise, int[] codes, string[] decodedPerCode)
        {
            // Fast path: compute device position from CTM × Trm without per-character Save/Restore.
            // The font is created at the effective size (fontSize * matrix scale) and drawn at device coords.
            var ctm = canvas.TotalMatrix;
            float tmScaleX = state.TextMatrix.ScaleX;
            float tmScaleY = state.TextMatrix.ScaleY;

            // Effective font size in device pixels
            float effectiveSize = fontSize * Math.Abs(tmScaleX) * hScale;
            float deviceFontSize = effectiveSize * Math.Abs(ctm.ScaleX);
            if (deviceFontSize < 0.5f)
            {
                return;
            }

            using var font = new SKFont(typeface, deviceFontSize)
            {
                Edging = SKFontEdging.SubpixelAntialias,
                Subpixel = true,
            };

            using var paint = new SKPaint
            {
                IsAntialias = true,
                Style = SKPaintStyle.Fill,
                Color = PdfColorHelper.WithAlpha(state.FillColor, state.FillAlpha),
            };

            using var measureFont = new SKFont(typeface, fontSize);

            canvas.Save();
            canvas.SetMatrix(SKMatrix.Identity);

            for (int codeIndex = 0; codeIndex < codes.Length; codeIndex++)
            {
                string decodedChars = codeIndex < decodedPerCode.Length ? decodedPerCode[codeIndex] : "";
                if (!string.IsNullOrEmpty(decodedChars))
                {
                    float tmE = state.TextMatrix.TransX;
                    float tmF = state.TextMatrix.TransY;
                    float trmTransX = tmE;
                    float trmTransY = tmF;

                    float deviceX = ctm.ScaleX * trmTransX + ctm.SkewX * trmTransY + ctm.TransX;
                    float deviceY = ctm.SkewY * trmTransX + ctm.ScaleY * trmTransY + ctm.TransY;

                    canvas.DrawText(decodedChars, deviceX, deviceY, font, paint);
                }

                int charCode = codes[codeIndex];
                float advanceWidth = GetCharAdvance(state, measureFont, fontSize, charCode,
                    !string.IsNullOrEmpty(decodedChars) ? decodedChars : " ", paint);
                float displacement = (advanceWidth * fontSize + state.CharSpacing) * hScale;
                if (!string.IsNullOrEmpty(decodedChars) && decodedChars == " ")
                {
                    displacement += state.WordSpacing * hScale;
                }
                var translateAdv = SKMatrix.CreateTranslation(displacement, 0);
                state.TextMatrix = SKMatrix.Concat(state.TextMatrix, translateAdv);
            }

            canvas.Restore();
        }

        private void DrawTextBytesFull(SKCanvas canvas, GraphicsState state, SKTypeface typeface,
            float fontSize, float hScale, float textRise, int[] codes, string[] decodedPerCode,
            bool doFill, bool doStroke, bool doClip, bool invisible)
        {
            using var font = new SKFont(typeface, 1f)
            {
                Edging = SKFontEdging.SubpixelAntialias,
                Subpixel = true,
            };
            using var measureFont = new SKFont(typeface, fontSize);

            using var paint = new SKPaint
            {
                IsAntialias = true,
            };

            var savedCtm = canvas.TotalMatrix;

            for (int codeIndex = 0; codeIndex < codes.Length; codeIndex++)
            {
                string decodedChars = codeIndex < decodedPerCode.Length ? decodedPerCode[codeIndex] : "";
                bool hasVisibleChars = !string.IsNullOrEmpty(decodedChars);

                if (hasVisibleChars)
                {
                    float tmA = state.TextMatrix.ScaleX;
                    float tmB = state.TextMatrix.SkewY;
                    float tmC = state.TextMatrix.SkewX;
                    float tmD = state.TextMatrix.ScaleY;
                    float tmE = state.TextMatrix.TransX;
                    float tmF = state.TextMatrix.TransY;

                    float trmScaleX = fontSize * hScale * tmA;
                    float trmSkewY = fontSize * hScale * tmB;
                    float trmSkewX = fontSize * tmC;
                    float trmScaleY = fontSize * tmD;
                    float trmTransX = textRise * tmC + tmE;
                    float trmTransY = textRise * tmD + tmF;

                    var trm = new SKMatrix(
                        trmScaleX, trmSkewX, trmTransX,
                        -trmSkewY, -trmScaleY, trmTransY,
                        0, 0, 1);

                    canvas.Save();
                    canvas.SetMatrix(SKMatrix.Concat(savedCtm, trm));

                    if (!invisible)
                    {
                        if (doFill)
                        {
                            paint.Style = SKPaintStyle.Fill;
                            paint.Color = PdfColorHelper.WithAlpha(state.FillColor, state.FillAlpha);
                            canvas.DrawText(decodedChars, 0, 0, font, paint);
                        }
                        if (doStroke)
                        {
                            paint.Style = SKPaintStyle.Stroke;
                            paint.Color = PdfColorHelper.WithAlpha(state.StrokeColor, state.StrokeAlpha);
                            paint.StrokeWidth = state.LineWidth / fontSize;
                            canvas.DrawText(decodedChars, 0, 0, font, paint);
                        }
                    }

                    if (doClip)
                    {
                        var glyphPath = font.GetTextPath(decodedChars, new SKPoint(0, 0));
                        if (glyphPath != null && !glyphPath.IsEmpty)
                        {
                            if (state.TextClipPath == null)
                            {
                                state.TextClipPath = new SKPath();
                            }
                            var transformedPath = new SKPath();
                            glyphPath.Transform(canvas.TotalMatrix, transformedPath);
                            state.TextClipPath.AddPath(transformedPath);
                            transformedPath.Dispose();
                            glyphPath.Dispose();
                        }
                    }

                    canvas.Restore();
                }

                int charCode = codes[codeIndex];
                float advanceWidth = GetCharAdvance(state, measureFont, fontSize, charCode,
                    hasVisibleChars ? decodedChars : " ", paint);
                float displacement = (advanceWidth * fontSize + state.CharSpacing) * hScale;
                if (hasVisibleChars && decodedChars == " ")
                {
                    displacement += state.WordSpacing * hScale;
                }
                var translateAdv = SKMatrix.CreateTranslation(displacement, 0);
                state.TextMatrix = SKMatrix.Concat(state.TextMatrix, translateAdv);
            }
        }

        private static float GetCharAdvance(GraphicsState state, SKFont measureFont, float fontSize,
            int charCode, string decodedChars, SKPaint paint)
        {
            if (state.FontWidths != null && state.FontWidths.TryGetValue(charCode, out float pdfWidth))
            {
                return pdfWidth / 1000f;
            }
            if (state.FontWidths != null && state.FontDefaultWidth > 0)
            {
                return state.FontDefaultWidth / 1000f;
            }
            float measuredWidth = measureFont.MeasureText(decodedChars, paint);
            return measuredWidth / fontSize;
        }

        private void OpPaintShading(SKCanvas canvas, GraphicsState state, List<object> args, PdfObj pageDict)
        {
            string shadingName = ContentStreamParser.GetNameStr(args, 0);
            if (string.IsNullOrEmpty(shadingName))
            {
                return;
            }

            var resources = _reader.Resolve(pageDict["Resources"]);
            var shadings = _reader.Resolve(resources["Shading"]);
            var shadingDict = _reader.Resolve(shadings[shadingName]);
            if (shadingDict.IsNull)
            {
                return;
            }

            int shadingType = (int)_reader.Resolve(shadingDict["ShadingType"]).AsInt();
            var colorSpaceObj = _reader.Resolve(shadingDict["ColorSpace"]);
            string colorSpace;
            if (colorSpaceObj.IsName)
            {
                colorSpace = colorSpaceObj.AsName();
                if (colorSpace.StartsWith("/"))
                {
                    colorSpace = colorSpace.Substring(1);
                }
            }
            else if (colorSpaceObj.IsArray && colorSpaceObj.Count > 0)
            {
                string csName = _reader.Resolve(colorSpaceObj[0]).AsName();
                if (csName.StartsWith("/")) { csName = csName.Substring(1); }
                if (csName == "ICCBased" && colorSpaceObj.Count > 1)
                {
                    var iccStream = _reader.Resolve(colorSpaceObj[1]);
                    int components = (int)_reader.Resolve(iccStream["N"]).AsInt();
                    colorSpace = components == 1 ? "DeviceGray" : components == 4 ? "DeviceCMYK" : "DeviceRGB";
                }
                else
                {
                    colorSpace = csName;
                }
            }
            else
            {
                colorSpace = "DeviceRGB";
            }

            // Check for Background color first
            var bgObj = _reader.Resolve(shadingDict["Background"]);
            if (!bgObj.IsNull && bgObj.IsArray)
            {
                SKColor bgColor = ResolveColorFromArray(bgObj, colorSpace);
                using var bgPaint = new SKPaint { Color = PdfColorHelper.WithAlpha(bgColor, state.FillAlpha) };
                canvas.DrawRect(canvas.LocalClipBounds, bgPaint);
            }

            if (shadingType == 2)
            {
                PaintAxialShading(canvas, state, shadingDict, colorSpace);
            }
            else if (shadingType == 3)
            {
                PaintRadialShading(canvas, state, shadingDict, colorSpace);
            }
        }

        private void PaintAxialShading(SKCanvas canvas, GraphicsState state, PdfObj shadingDict, string colorSpace)
        {
            var coordsObj = _reader.Resolve(shadingDict["Coords"]);
            if (coordsObj.IsNull || coordsObj.Count < 4)
            {
                return;
            }

            float x0 = coordsObj[0].AsFloat();
            float y0 = coordsObj[1].AsFloat();
            float x1 = coordsObj[2].AsFloat();
            float y1 = coordsObj[3].AsFloat();

            var functionObj = _reader.Resolve(shadingDict["Function"]);
            var colors = EvaluateShadingFunction(functionObj, colorSpace);

            if (colors.Count < 2)
            {
                return;
            }

            var skColors = colors.ToArray();
            float[] positions = new float[colors.Count];
            for (int i = 0; i < positions.Length; i++)
            {
                positions[i] = (float)i / (positions.Length - 1);
            }

            using var shader = SKShader.CreateLinearGradient(
                new SKPoint(x0, y0), new SKPoint(x1, y1),
                skColors, positions, SKShaderTileMode.Clamp);
            using var paint = new SKPaint
            {
                Shader = shader,
                IsAntialias = true,
                Color = PdfColorHelper.WithAlpha(SKColors.White, state.FillAlpha),
            };
            canvas.DrawRect(canvas.LocalClipBounds, paint);
        }

        private void PaintRadialShading(SKCanvas canvas, GraphicsState state, PdfObj shadingDict, string colorSpace)
        {
            var coordsObj = _reader.Resolve(shadingDict["Coords"]);
            if (coordsObj.IsNull || coordsObj.Count < 6)
            {
                return;
            }

            float x0 = coordsObj[0].AsFloat();
            float y0 = coordsObj[1].AsFloat();
            float r0 = coordsObj[2].AsFloat();
            float x1 = coordsObj[3].AsFloat();
            float y1 = coordsObj[4].AsFloat();
            float r1 = coordsObj[5].AsFloat();

            var functionObj = _reader.Resolve(shadingDict["Function"]);
            var colors = EvaluateShadingFunction(functionObj, colorSpace);

            if (colors.Count < 2)
            {
                return;
            }

            var skColors = colors.ToArray();
            float[] positions = new float[colors.Count];
            for (int i = 0; i < positions.Length; i++)
            {
                positions[i] = (float)i / (positions.Length - 1);
            }

            using var shader = SKShader.CreateRadialGradient(
                new SKPoint(x1, y1), r1,
                skColors, positions, SKShaderTileMode.Clamp);
            using var paint = new SKPaint
            {
                Shader = shader,
                IsAntialias = true,
            };
            canvas.DrawRect(canvas.LocalClipBounds, paint);
        }

        private List<SKColor> EvaluateShadingFunction(PdfObj functionObj, string colorSpace)
        {
            var colors = new List<SKColor>();
            if (functionObj.IsNull)
            {
                colors.Add(SKColors.White);
                colors.Add(SKColors.Black);
                return colors;
            }

            // Type 2 (exponential) function: C0 + t^N * (C1 - C0)
            int functionType = (int)_reader.Resolve(functionObj["FunctionType"]).AsInt();

            if (functionType == 2)
            {
                var c0Obj = _reader.Resolve(functionObj["C0"]);
                var c1Obj = _reader.Resolve(functionObj["C1"]);

                SKColor startColor = c0Obj.IsNull ? SKColors.White : ResolveColorFromArray(c0Obj, colorSpace);
                SKColor endColor = c1Obj.IsNull ? SKColors.Black : ResolveColorFromArray(c1Obj, colorSpace);
                colors.Add(startColor);
                colors.Add(endColor);
            }
            else if (functionType == 3)
            {
                // Stitching function — multiple sub-functions
                var functions = _reader.Resolve(functionObj["Functions"]);
                if (functions.IsArray)
                {
                    for (int i = 0; i < functions.Count; i++)
                    {
                        var subFunc = _reader.Resolve(functions[i]);
                        var subColors = EvaluateShadingFunction(subFunc, colorSpace);
                        if (i == 0)
                        {
                            colors.AddRange(subColors);
                        }
                        else if (subColors.Count > 1)
                        {
                            colors.AddRange(subColors.GetRange(1, subColors.Count - 1));
                        }
                    }
                }
            }
            else if (functionType == 0)
            {
                // Sampled function — read the sample table
                var rangeObj = _reader.Resolve(functionObj["Range"]);
                var sizeObj = _reader.Resolve(functionObj["Size"]);
                int outputComponents = rangeObj.IsNull ? 1 : rangeObj.Count / 2;

                byte[] sampleData;
                if (functionObj.IsStream)
                {
                    sampleData = _reader.GetStreamBytes(functionObj);
                }
                else
                {
                    sampleData = Array.Empty<byte>();
                }

                int bitsPerSample = (int)_reader.Resolve(functionObj["BitsPerSample"]).AsInt();
                if (bitsPerSample <= 0) { bitsPerSample = 8; }
                int sampleCount = sizeObj.IsNull ? 2 : (int)sizeObj[0].AsInt();

                if (sampleData.Length > 0 && sampleCount >= 2)
                {
                    int bytesPerSample = bitsPerSample / 8;
                    int bytesPerEntry = outputComponents * bytesPerSample;

                    // Sample at start and end
                    for (int sampleIndex = 0; sampleIndex < sampleCount; sampleIndex += Math.Max(1, sampleCount - 1))
                    {
                        int offset = sampleIndex * bytesPerEntry;
                        if (offset + bytesPerEntry > sampleData.Length) { break; }

                        float[] values = new float[outputComponents];
                        for (int comp = 0; comp < outputComponents; comp++)
                        {
                            int byteOffset = offset + comp * bytesPerSample;
                            if (bytesPerSample == 1)
                            {
                                values[comp] = sampleData[byteOffset] / 255f;
                            }
                            else if (bytesPerSample == 2)
                            {
                                values[comp] = ((sampleData[byteOffset] << 8) | sampleData[byteOffset + 1]) / 65535f;
                            }
                            else
                            {
                                values[comp] = sampleData[byteOffset] / 255f;
                            }
                        }

                        SKColor sampleColor;
                        if (colorSpace == "DeviceGray" && values.Length >= 1)
                        {
                            sampleColor = PdfColorHelper.GrayToColor(values[0]);
                        }
                        else if (colorSpace == "DeviceCMYK" && values.Length >= 4)
                        {
                            sampleColor = PdfColorHelper.CmykToColor(values[0], values[1], values[2], values[3]);
                        }
                        else if (values.Length >= 3)
                        {
                            sampleColor = PdfColorHelper.RgbToColor(values[0], values[1], values[2]);
                        }
                        else
                        {
                            sampleColor = PdfColorHelper.GrayToColor(values.Length > 0 ? values[0] : 0);
                        }
                        colors.Add(sampleColor);
                    }
                }

                if (colors.Count < 2)
                {
                    colors.Clear();
                    colors.Add(SKColors.White);
                    colors.Add(SKColors.Black);
                }
            }
            else
            {
                colors.Add(SKColors.White);
                colors.Add(SKColors.Black);
            }

            return colors;
        }

        private SKColor ResolveColorFromArray(PdfObj array, string colorSpace)
        {
            if (colorSpace == "DeviceGray" || colorSpace == "CalGray")
            {
                float g = array.Count > 0 ? array[0].AsFloat() : 0;
                return PdfColorHelper.GrayToColor(g);
            }
            if (colorSpace == "DeviceCMYK")
            {
                return PdfColorHelper.CmykToColor(
                    array.Count > 0 ? array[0].AsFloat() : 0,
                    array.Count > 1 ? array[1].AsFloat() : 0,
                    array.Count > 2 ? array[2].AsFloat() : 0,
                    array.Count > 3 ? array[3].AsFloat() : 0);
            }
            return PdfColorHelper.RgbToColor(
                array.Count > 0 ? array[0].AsFloat() : 0,
                array.Count > 1 ? array[1].AsFloat() : 0,
                array.Count > 2 ? array[2].AsFloat() : 0);
        }

        private void OpDoXObject(SKCanvas canvas, GraphicsState state,
            Stack<GraphicsState> stateStack, List<object> args, PdfObj pageDict)
        {
            string name = ContentStreamParser.GetNameStr(args, 0);
            if (string.IsNullOrEmpty(name))
            {
                return;
            }

            var resources = _reader.Resolve(pageDict["Resources"]);
            var xobjects = _reader.Resolve(resources["XObject"]);
            var xobj = _reader.Resolve(xobjects[name]);
            if (xobj.IsNull)
            {
                return;
            }

            string subtype = _reader.Resolve(xobj["Subtype"]).AsName();

            if (subtype == "/Image" || subtype == "Image")
            {
                PdfImageRenderer.DrawImageXObject(canvas, state, xobj, _reader);
            }
            else if (subtype == "/Form" || subtype == "Form")
            {
                DrawFormXObject(canvas, state, stateStack, xobj, pageDict);
            }
        }

        private void DrawFormXObject(SKCanvas canvas, GraphicsState state,
            Stack<GraphicsState> stateStack, PdfObj formDict, PdfObj pageDict)
        {
            // [SPEC §8.11.4.2] Skip forms with Optional Content (OC) that are hidden
            var optionalContent = _reader.Resolve(formDict["OC"]);
            if (!optionalContent.IsNull)
            {
                if (!IsOptionalContentVisible(optionalContent))
                {
                    return;
                }
            }

            byte[] formData = _reader.GetStreamBytes(formDict);
            if (formData == null || formData.Length == 0)
            {
                return;
            }

            canvas.Save();

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

            var bbox = _reader.Resolve(formDict["BBox"]);
            if (!bbox.IsNull && bbox.IsArray && bbox.Count >= 4)
            {
                var rect = PdfRect.FromArray(bbox);
                float minX = Math.Min(rect.Left, rect.Right);
                float maxX = Math.Max(rect.Left, rect.Right);
                float minY = Math.Min(rect.Bottom, rect.Top);
                float maxY = Math.Max(rect.Bottom, rect.Top);
                canvas.ClipRect(new SKRect(minX, minY, maxX, maxY));
            }

            var formResources = _reader.Resolve(formDict["Resources"]);
            PdfObj effectivePage;
            if (!formResources.IsNull)
            {
                var pageResources = _reader.Resolve(pageDict["Resources"]);
                effectivePage = new MergedResourcePage(_reader, formResources, pageResources);
            }
            else
            {
                effectivePage = pageDict;
            }

            var formState = new GraphicsState();
            formState.CopyFrom(state);
            var formStateStack = new Stack<GraphicsState>();
            var formPath = new SKPath();

            var operators = _parser.Parse(formData);
            int executedOps = 0;
            int failedOps = 0;
            foreach (var op in operators)
            {
                try
                {
                    ExecuteOperator(canvas, op, formState, formStateStack, formPath, effectivePage);
                    executedOps++;
                }
                catch (Exception ex)
                {
                    failedOps++;
                    if (failedOps <= 3)
                    {
                        _warnings.Add($"Form operator '{op.Name}': {ex.Message}");
                    }
                }
            }

            if (failedOps > 0)
            {
                _warnings.Add($"Form XObject: {executedOps} ops executed, {failedOps} failed");
            }

            formPath.Dispose();
            formState.TextClipPath?.Dispose();
            formState.TextClipPath = null;
            canvas.Restore();
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
    }
}
