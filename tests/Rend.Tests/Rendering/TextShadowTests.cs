using System.Collections.Generic;
using System.IO;
using Rend.Core.Values;
using Rend.Css;
using Rend.Css.Properties.Internal;
using Rend.Fonts;
using Rend.Layout;
using Rend.Rendering;
using Rend.Rendering.Internal;
using Rend.Style;
using Rend.Text;
using Xunit;

namespace Rend.Tests.Rendering
{
    public class TextShadowTests
    {
        [Fact]
        public void Paint_NoShadow_NoExtraDrawCalls()
        {
            var target = new TextShadowRecordingTarget();
            var fragment = CreateFragment("Hello");
            var style = CreateStyle(null);

            TextPainter.Paint(fragment, 0, 0, 10f, target, style);

            // Only the main text draw call, no shadow calls
            Assert.Single(target.TextCalls);
        }

        [Fact]
        public void Paint_NoneShadow_NoExtraDrawCalls()
        {
            var target = new TextShadowRecordingTarget();
            var fragment = CreateFragment("Hello");
            var style = CreateStyle(new CssKeywordValue("none"));

            TextPainter.Paint(fragment, 0, 0, 10f, target, style);

            Assert.Single(target.TextCalls);
        }

        [Fact]
        public void Paint_SingleShadow_DrawsShadowBeforeText()
        {
            // text-shadow: 2px 3px black
            var shadowValue = new CssListValue(new List<CssValue>
            {
                new CssDimensionValue(2, "px"),
                new CssDimensionValue(3, "px"),
                new CssColorValue(CssColor.Black),
            }, ' ');

            var target = new TextShadowRecordingTarget();
            var fragment = CreateFragment("Hello");
            var style = CreateStyle(shadowValue);

            TextPainter.Paint(fragment, 0, 0, 10f, target, style);

            // Shadow + main text = 2 draw calls
            Assert.Equal(2, target.TextCalls.Count);
            // Shadow is drawn first (at offset position)
            Assert.Equal(2f, target.TextCalls[0].X, 0.1f);
            Assert.Equal(13f, target.TextCalls[0].Y, 0.1f); // 10 (baseline) + 3 (offset)
        }

        [Fact]
        public void Paint_ShadowWithColor_UsesSpecifiedColor()
        {
            // text-shadow: 1px 1px red
            var shadowValue = new CssListValue(new List<CssValue>
            {
                new CssDimensionValue(1, "px"),
                new CssDimensionValue(1, "px"),
                new CssColorValue(new CssColor(255, 0, 0, 255)),
            }, ' ');

            var target = new TextShadowRecordingTarget();
            var fragment = CreateFragment("Test");
            var style = CreateStyle(shadowValue);

            TextPainter.Paint(fragment, 0, 0, 10f, target, style);

            Assert.Equal(2, target.TextCalls.Count);
            // Shadow color should be red
            Assert.Equal(255, target.TextCalls[0].Style.Color.R);
            Assert.Equal(0, target.TextCalls[0].Style.Color.G);
        }

        [Fact]
        public void Paint_MultipleShadows_AllRendered()
        {
            // text-shadow: 1px 1px red, 2px 2px blue
            var shadow1 = new CssListValue(new List<CssValue>
            {
                new CssDimensionValue(1, "px"),
                new CssDimensionValue(1, "px"),
                new CssColorValue(new CssColor(255, 0, 0, 255)),
            }, ' ');
            var shadow2 = new CssListValue(new List<CssValue>
            {
                new CssDimensionValue(2, "px"),
                new CssDimensionValue(2, "px"),
                new CssColorValue(new CssColor(0, 0, 255, 255)),
            }, ' ');
            var commaList = new CssListValue(new List<CssValue> { shadow1, shadow2 }, ',');

            var target = new TextShadowRecordingTarget();
            var fragment = CreateFragment("Test");
            var style = CreateStyle(commaList);

            TextPainter.Paint(fragment, 0, 0, 10f, target, style);

            // 2 shadows + 1 main text = 3 calls
            Assert.Equal(3, target.TextCalls.Count);
        }

        [Fact]
        public void Paint_ShadowWithBlur_ReducedOpacity()
        {
            // text-shadow: 0 0 10px black
            var shadowValue = new CssListValue(new List<CssValue>
            {
                new CssNumberValue(0),
                new CssNumberValue(0),
                new CssDimensionValue(10, "px"),
                new CssColorValue(CssColor.Black),
            }, ' ');

            var target = new TextShadowRecordingTarget();
            var fragment = CreateFragment("Test");
            var style = CreateStyle(shadowValue);

            TextPainter.Paint(fragment, 0, 0, 10f, target, style);

            Assert.Equal(2, target.TextCalls.Count);
            // Blur shadow should have reduced alpha
            Assert.True(target.TextCalls[0].Style.Color.A < 255);
        }

        [Fact]
        public void Paint_DefaultColor_IsBlack()
        {
            // text-shadow: 2px 2px — no color
            var shadowValue = new CssListValue(new List<CssValue>
            {
                new CssDimensionValue(2, "px"),
                new CssDimensionValue(2, "px"),
            }, ' ');

            var target = new TextShadowRecordingTarget();
            var fragment = CreateFragment("Test");
            var style = CreateStyle(shadowValue);

            TextPainter.Paint(fragment, 0, 0, 10f, target, style);

            Assert.Equal(2, target.TextCalls.Count);
            Assert.Equal(0, target.TextCalls[0].Style.Color.R);
            Assert.Equal(0, target.TextCalls[0].Style.Color.G);
            Assert.Equal(0, target.TextCalls[0].Style.Color.B);
        }

        // ═══════════════════════════════════════════
        // Helpers
        // ═══════════════════════════════════════════

        private static LineFragment CreateFragment(string text)
        {
            return new LineFragment
            {
                X = 0,
                Width = text.Length * 10,
                Height = 16,
                Baseline = 10,
                Text = text
            };
        }

        private static ComputedStyle CreateStyle(CssValue? shadowValue)
        {
            var values = new PropertyValue[PropertyId.Count];
            var refValues = new object?[PropertyId.Count];
            InitialValues.CopyTo(values, refValues);

            if (shadowValue != null)
            {
                refValues[PropertyId.TextShadow] = shadowValue;
            }

            return new ComputedStyle(values, refValues);
        }

        private sealed class TextShadowRecordingTarget : IRenderTarget
        {
            public List<(string Text, float X, float Y, TextStyle Style)> TextCalls { get; }
                = new List<(string, float, float, TextStyle)>();

            public void BeginPage(float width, float height) { }
            public void EndPage() { }
            public void Save() { }
            public void Restore() { }
            public void SetTransform(Matrix3x2 transform) { }
            public void ConcatTransform(Matrix3x2 transform) { }
            public void SetOpacity(float opacity) { }
            public void SetBlendMode(Rend.Css.CssMixBlendMode blendMode) { }
            public void SetImageRendering(Rend.Css.CssImageRendering rendering) { }
            public void ApplyFilter(CssFilterEffect[] effects) { }
            public void SetMaskBlur(float sigma, bool inner = false) { }
            public void BeginMask() { }
            public void EndMask(GradientInfo gradient, RectF bounds) { }
            public void PushClipRect(RectF rect) { }
            public void PushClipPath(PathData path) { }
            public void PopClip() { }
            public void FillRect(RectF rect, BrushInfo brush) { }
            public void StrokeRect(RectF rect, PenInfo pen) { }
            public void FillPath(PathData path, BrushInfo brush) { }
            public void StrokePath(PathData path, PenInfo pen) { }
            public void DrawImage(ImageData image, RectF destRect) { }
            public void Finish(Stream output) { }
            public void AddLink(RectF rect, string uri) { }
            public void AddBookmark(string title, int level, float yPosition) { }

            public void DrawText(string text, float x, float y, TextStyle style)
            {
                TextCalls.Add((text, x, y, style));
            }

            public void DrawGlyphs(ShapedTextRun run, float x, float y, CssColor color, FontDescriptor font)
            {
                // Record as text call for simplicity
                var ts = new TextStyle { Color = color, FontSize = run.FontSize };
                TextCalls.Add((run.OriginalText, x, y, ts));
            }
        }
    }
}
