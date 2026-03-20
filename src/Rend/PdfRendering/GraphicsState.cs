#nullable enable
using System.Collections.Generic;
using Rend.Pdf.Parsing;
using SkiaSharp;

namespace Rend.PdfRendering
{
    internal sealed class GraphicsState
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
        public string FillSeparationName = "";
        public string StrokeSeparationName = "";

        public string FontName = "";
        public float FontSize = 12f;
        public SKTypeface? Typeface;
        public PdfObj? FontDict;
        public Dictionary<int, string>? ToUnicodeMap;
        public Dictionary<int, string>? Encoding;
        public Dictionary<int, float>? FontWidths;
        public int FontFirstChar;
        public float FontDefaultWidth = 1000f;
        public bool IsCIDFont;
        public float HorizontalScaling = 100f;
        public string BlendMode = "Normal";
        public SKMatrix TextMatrix = SKMatrix.Identity;
        public SKMatrix TextLineMatrix = SKMatrix.Identity;
        public float CharSpacing;
        public float WordSpacing;
        public float TextLeading;
        public int TextRenderMode;
        public float TextRise;

        public float CurrentX;
        public float CurrentY;
        public bool PendingClipNonZero;
        public bool PendingClipEvenOdd;
        public SKPath? TextClipPath;

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
                FillSeparationName = FillSeparationName,
                StrokeSeparationName = StrokeSeparationName,
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
                PendingClipNonZero = PendingClipNonZero,
                PendingClipEvenOdd = PendingClipEvenOdd,
                TextClipPath = null,
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
            FillSeparationName = other.FillSeparationName;
            StrokeSeparationName = other.StrokeSeparationName;
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
            PendingClipNonZero = other.PendingClipNonZero;
            PendingClipEvenOdd = other.PendingClipEvenOdd;
            TextClipPath = null;
        }
    }
}
