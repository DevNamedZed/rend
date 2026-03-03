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
    public class BoxShadowTests
    {
        // ═══════════════════════════════════════════
        // Basic shadow rendering
        // ═══════════════════════════════════════════

        [Fact]
        public void Paint_NoShadow_NothingRendered()
        {
            var target = new ShadowRecordingTarget();
            var box = CreateBoxWithShadow(null); // no box-shadow

            BoxShadowPainter.Paint(box, target);

            Assert.Empty(target.FilledRects);
            Assert.Empty(target.FilledPaths);
        }

        [Fact]
        public void Paint_NoneKeyword_NothingRendered()
        {
            var target = new ShadowRecordingTarget();
            var box = CreateBoxWithShadow(new CssKeywordValue("none"));

            BoxShadowPainter.Paint(box, target);

            Assert.Empty(target.FilledRects);
            Assert.Empty(target.FilledPaths);
        }

        [Fact]
        public void Paint_SharpShadow_SingleRectRendered()
        {
            // box-shadow: 5px 5px black
            var shadowValue = new CssListValue(new List<CssValue>
            {
                new CssDimensionValue(5, "px"),
                new CssDimensionValue(5, "px"),
                new CssColorValue(CssColor.Black),
            }, ' ');

            var target = new ShadowRecordingTarget();
            var box = CreateBoxWithShadow(shadowValue, 100, 100, 200, 50);

            BoxShadowPainter.Paint(box, target);

            Assert.Single(target.FilledRects);
            var (rect, brush) = target.FilledRects[0];
            // Shadow rect should be offset by 5px in both directions
            Assert.Equal(105f, rect.X);
            Assert.Equal(105f, rect.Y);
            Assert.Equal(200f, rect.Width);
            Assert.Equal(50f, rect.Height);
        }

        [Fact]
        public void Paint_ShadowWithSpread_ExpandedRect()
        {
            // box-shadow: 0 0 0 10px red — spread 10px, no blur, no offset
            var shadowValue = new CssListValue(new List<CssValue>
            {
                new CssNumberValue(0),
                new CssNumberValue(0),
                new CssNumberValue(0),
                new CssDimensionValue(10, "px"),
                new CssColorValue(new CssColor(255, 0, 0, 255)),
            }, ' ');

            var target = new ShadowRecordingTarget();
            var box = CreateBoxWithShadow(shadowValue, 50, 50, 100, 100);

            BoxShadowPainter.Paint(box, target);

            Assert.Single(target.FilledRects);
            var (rect, _) = target.FilledRects[0];
            // Spread expands by 10px on each side
            Assert.Equal(40f, rect.X);
            Assert.Equal(40f, rect.Y);
            Assert.Equal(120f, rect.Width);
            Assert.Equal(120f, rect.Height);
        }

        [Fact]
        public void Paint_ShadowWithBlur_SingleRectWithMaskBlur()
        {
            // box-shadow: 0 0 10px black — blur only
            var shadowValue = new CssListValue(new List<CssValue>
            {
                new CssNumberValue(0),
                new CssNumberValue(0),
                new CssDimensionValue(10, "px"),
                new CssColorValue(CssColor.Black),
            }, ' ');

            var target = new ShadowRecordingTarget();
            var box = CreateBoxWithShadow(shadowValue, 50, 50, 100, 100);

            BoxShadowPainter.Paint(box, target);

            // Single rect with mask blur (SetMaskBlur called)
            Assert.Equal(1, target.FilledRects.Count);
            Assert.True(target.MaskBlurSet, "SetMaskBlur should have been called");
        }

        [Fact]
        public void Paint_InsetShadow_RendersStrips()
        {
            // box-shadow: inset 5px 5px black
            var shadowValue = new CssListValue(new List<CssValue>
            {
                new CssKeywordValue("inset"),
                new CssDimensionValue(5, "px"),
                new CssDimensionValue(5, "px"),
                new CssColorValue(CssColor.Black),
            }, ' ');

            var target = new ShadowRecordingTarget();
            var box = CreateBoxWithShadow(shadowValue, 50, 50, 100, 100);

            BoxShadowPainter.Paint(box, target);

            // Inset shadows render edge strips (up to 4 sides)
            Assert.True(target.FilledRects.Count > 0);
        }

        [Fact]
        public void Paint_MultipleShadows_AllRendered()
        {
            // box-shadow: 2px 2px red, 4px 4px blue
            var shadow1 = new CssListValue(new List<CssValue>
            {
                new CssDimensionValue(2, "px"),
                new CssDimensionValue(2, "px"),
                new CssColorValue(new CssColor(255, 0, 0, 255)),
            }, ' ');
            var shadow2 = new CssListValue(new List<CssValue>
            {
                new CssDimensionValue(4, "px"),
                new CssDimensionValue(4, "px"),
                new CssColorValue(new CssColor(0, 0, 255, 255)),
            }, ' ');
            var commaList = new CssListValue(new List<CssValue> { shadow1, shadow2 }, ',');

            var target = new ShadowRecordingTarget();
            var box = CreateBoxWithShadow(commaList, 50, 50, 100, 100);

            BoxShadowPainter.Paint(box, target);

            // Two shadows rendered in reverse order (CSS: last shadow is bottommost)
            Assert.Equal(2, target.FilledRects.Count);
        }

        [Fact]
        public void Paint_NamedColor_Parsed()
        {
            // box-shadow: 5px 5px red
            var shadowValue = new CssListValue(new List<CssValue>
            {
                new CssDimensionValue(5, "px"),
                new CssDimensionValue(5, "px"),
                new CssKeywordValue("red"),
            }, ' ');

            var target = new ShadowRecordingTarget();
            var box = CreateBoxWithShadow(shadowValue, 50, 50, 100, 100);

            BoxShadowPainter.Paint(box, target);

            Assert.Single(target.FilledRects);
            var (_, brush) = target.FilledRects[0];
            Assert.Equal(255, brush.Color.R);
            Assert.Equal(0, brush.Color.G);
            Assert.Equal(0, brush.Color.B);
        }

        [Fact]
        public void Paint_RgbColor_Parsed()
        {
            // box-shadow: 5px 5px rgb(0, 128, 255)
            var rgbFn = new CssFunctionValue("rgb", new List<CssValue>
            {
                new CssNumberValue(0),
                new CssNumberValue(128),
                new CssNumberValue(255),
            });
            var shadowValue = new CssListValue(new List<CssValue>
            {
                new CssDimensionValue(5, "px"),
                new CssDimensionValue(5, "px"),
                rgbFn,
            }, ' ');

            var target = new ShadowRecordingTarget();
            var box = CreateBoxWithShadow(shadowValue, 50, 50, 100, 100);

            BoxShadowPainter.Paint(box, target);

            Assert.Single(target.FilledRects);
            var (_, brush) = target.FilledRects[0];
            Assert.Equal(0, brush.Color.R);
            Assert.Equal(128, brush.Color.G);
            Assert.Equal(255, brush.Color.B);
        }

        [Fact]
        public void Paint_DefaultColor_IsBlack()
        {
            // box-shadow: 5px 5px — no color specified, should default to black
            var shadowValue = new CssListValue(new List<CssValue>
            {
                new CssDimensionValue(5, "px"),
                new CssDimensionValue(5, "px"),
            }, ' ');

            var target = new ShadowRecordingTarget();
            var box = CreateBoxWithShadow(shadowValue, 50, 50, 100, 100);

            BoxShadowPainter.Paint(box, target);

            Assert.Single(target.FilledRects);
            var (_, brush) = target.FilledRects[0];
            Assert.Equal(0, brush.Color.R);
            Assert.Equal(0, brush.Color.G);
            Assert.Equal(0, brush.Color.B);
            Assert.Equal(255, brush.Color.A);
        }

        // ═══════════════════════════════════════════
        // Border radius
        // ═══════════════════════════════════════════

        [Fact]
        public void Paint_WithBorderRadius_UsesFillPath()
        {
            // box-shadow: 5px 5px black on element with border-radius
            var shadowValue = new CssListValue(new List<CssValue>
            {
                new CssDimensionValue(5, "px"),
                new CssDimensionValue(5, "px"),
                new CssColorValue(CssColor.Black),
            }, ' ');

            var target = new ShadowRecordingTarget();
            var box = CreateBoxWithShadow(shadowValue, 50, 50, 100, 100, borderRadius: 10f);

            BoxShadowPainter.Paint(box, target);

            // With border radius, should use FillPath instead of FillRect
            Assert.Empty(target.FilledRects);
            Assert.Single(target.FilledPaths);
        }

        [Fact]
        public void Paint_OnlyOneLength_NothingRendered()
        {
            // Invalid: only one length (need at least offset-x and offset-y)
            var shadowValue = new CssListValue(new List<CssValue>
            {
                new CssDimensionValue(5, "px"),
            }, ' ');

            var target = new ShadowRecordingTarget();
            var box = CreateBoxWithShadow(shadowValue, 50, 50, 100, 100);

            BoxShadowPainter.Paint(box, target);

            Assert.Empty(target.FilledRects);
        }

        [Fact]
        public void Paint_NullStyledNode_NothingRendered()
        {
            var target = new ShadowRecordingTarget();
            var box = new LayoutBox(null, BoxType.Block);

            BoxShadowPainter.Paint(box, target);

            Assert.Empty(target.FilledRects);
        }

        // ═══════════════════════════════════════════
        // Helpers
        // ═══════════════════════════════════════════

        private static LayoutBox CreateBoxWithShadow(CssValue? shadowValue,
            float x = 100, float y = 100, float w = 200, float h = 50,
            float borderRadius = 0f)
        {
            var values = new PropertyValue[PropertyId.Count];
            var refValues = new object?[PropertyId.Count];
            InitialValues.CopyTo(values, refValues);

            // Set border radius if needed
            if (borderRadius > 0)
            {
                values[PropertyId.BorderTopLeftRadius] = PropertyValue.FromLength(borderRadius);
                values[PropertyId.BorderTopRightRadius] = PropertyValue.FromLength(borderRadius);
                values[PropertyId.BorderBottomRightRadius] = PropertyValue.FromLength(borderRadius);
                values[PropertyId.BorderBottomLeftRadius] = PropertyValue.FromLength(borderRadius);
            }

            // Store box-shadow as raw CssValue
            if (shadowValue != null)
            {
                refValues[PropertyId.BoxShadow] = shadowValue;
            }

            var style = new ComputedStyle(values, refValues);
            var styledNode = new StyledText("test", style);
            var box = new LayoutBox(styledNode, BoxType.Block);
            box.ContentRect = new RectF(x, y, w, h);

            return box;
        }

        private sealed class ShadowRecordingTarget : IRenderTarget
        {
            public List<(RectF Rect, BrushInfo Brush)> FilledRects { get; } = new List<(RectF, BrushInfo)>();
            public List<(PathData Path, BrushInfo Brush)> FilledPaths { get; } = new List<(PathData, BrushInfo)>();
            public bool MaskBlurSet { get; private set; }

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
            public void SetMaskBlur(float sigma) { if (sigma > 0) MaskBlurSet = true; }
            public void BeginMask() { }
            public void EndMask(GradientInfo gradient, RectF bounds) { }
            public void PushClipRect(RectF rect) { }
            public void PushClipPath(PathData path) { }
            public void PopClip() { }
            public void StrokeRect(RectF rect, PenInfo pen) { }
            public void StrokePath(PathData path, PenInfo pen) { }
            public void DrawImage(ImageData image, RectF destRect) { }
            public void DrawText(string text, float x, float y, TextStyle style) { }
            public void DrawGlyphs(ShapedTextRun run, float x, float y, CssColor color, FontDescriptor font) { }
            public void Finish(Stream output) { }
            public void AddLink(RectF rect, string uri) { }
            public void AddBookmark(string title, int level, float yPosition) { }

            public void FillRect(RectF rect, BrushInfo brush)
            {
                FilledRects.Add((rect, brush));
            }

            public void FillPath(PathData path, BrushInfo brush)
            {
                FilledPaths.Add((path, brush));
            }
        }
    }
}
