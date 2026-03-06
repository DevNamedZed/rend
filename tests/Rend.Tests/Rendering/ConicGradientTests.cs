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
    public class ConicGradientTests
    {
        // ═══════════════════════════════════════════
        // Basic conic-gradient parsing
        // ═══════════════════════════════════════════

        [Fact]
        public void ConicGradient_TwoStops_FillsRect()
        {
            var target = new GradientRecordingTarget();
            var box = CreateGradientBox("conic-gradient", new List<CssValue>
            {
                new CssColorValue(new CssColor(255, 0, 0, 255)),
                new CssColorValue(new CssColor(0, 0, 255, 255)),
            });

            BackgroundPainter.Paint(box, target, null);

            Assert.Single(target.FilledRects);
            var brush = target.FilledRects[0].Brush;
            Assert.NotNull(brush.Gradient);
            Assert.Equal(GradientType.Conic, brush.Gradient.Type);
            Assert.Equal(2, brush.Gradient.Stops.Length);
        }

        [Fact]
        public void ConicGradient_ThreeStops_AllIncluded()
        {
            var target = new GradientRecordingTarget();
            var box = CreateGradientBox("conic-gradient", new List<CssValue>
            {
                new CssColorValue(new CssColor(255, 0, 0, 255)),
                new CssColorValue(new CssColor(0, 255, 0, 255)),
                new CssColorValue(new CssColor(0, 0, 255, 255)),
            });

            BackgroundPainter.Paint(box, target, null);

            Assert.Single(target.FilledRects);
            var gradient = target.FilledRects[0].Brush.Gradient;
            Assert.Equal(3, gradient.Stops.Length);
            Assert.Equal(0f, gradient.Stops[0].Position, 0.01);
            Assert.Equal(0.5f, gradient.Stops[1].Position, 0.01);
            Assert.Equal(1f, gradient.Stops[2].Position, 0.01);
        }

        [Fact]
        public void ConicGradient_DefaultAngle_IsZero()
        {
            var target = new GradientRecordingTarget();
            var box = CreateGradientBox("conic-gradient", new List<CssValue>
            {
                new CssColorValue(new CssColor(255, 0, 0, 255)),
                new CssColorValue(new CssColor(0, 0, 255, 255)),
            });

            BackgroundPainter.Paint(box, target, null);

            var gradient = target.FilledRects[0].Brush.Gradient;
            Assert.Equal(0f, gradient.Angle, 0.01);
        }

        [Fact]
        public void ConicGradient_DefaultCenter_IsFiftyPercent()
        {
            var target = new GradientRecordingTarget();
            var box = CreateGradientBox("conic-gradient", new List<CssValue>
            {
                new CssColorValue(new CssColor(255, 0, 0, 255)),
                new CssColorValue(new CssColor(0, 0, 255, 255)),
            });

            BackgroundPainter.Paint(box, target, null);

            var gradient = target.FilledRects[0].Brush.Gradient;
            Assert.Equal(0.5f, gradient.Center.X, 0.01);
            Assert.Equal(0.5f, gradient.Center.Y, 0.01);
        }

        // ═══════════════════════════════════════════
        // from <angle> syntax
        // ═══════════════════════════════════════════

        [Fact]
        public void ConicGradient_FromAngle_SetsAngle()
        {
            var target = new GradientRecordingTarget();
            var box = CreateGradientBox("conic-gradient", new List<CssValue>
            {
                new CssKeywordValue("from"),
                new CssDimensionValue(45, "deg"),
                new CssColorValue(new CssColor(255, 0, 0, 255)),
                new CssColorValue(new CssColor(0, 0, 255, 255)),
            });

            BackgroundPainter.Paint(box, target, null);

            var gradient = target.FilledRects[0].Brush.Gradient;
            Assert.Equal(45f, gradient.Angle, 0.01);
            Assert.Equal(GradientType.Conic, gradient.Type);
        }

        [Fact]
        public void ConicGradient_FromAngle90_SetsAngle()
        {
            var target = new GradientRecordingTarget();
            var box = CreateGradientBox("conic-gradient", new List<CssValue>
            {
                new CssKeywordValue("from"),
                new CssDimensionValue(90, "deg"),
                new CssColorValue(new CssColor(255, 0, 0, 255)),
                new CssColorValue(new CssColor(0, 0, 255, 255)),
            });

            BackgroundPainter.Paint(box, target, null);

            var gradient = target.FilledRects[0].Brush.Gradient;
            Assert.Equal(90f, gradient.Angle, 0.01);
        }

        // ═══════════════════════════════════════════
        // at <position> syntax
        // ═══════════════════════════════════════════

        [Fact]
        public void ConicGradient_AtPosition_SetsCenter()
        {
            var target = new GradientRecordingTarget();
            var box = CreateGradientBox("conic-gradient", new List<CssValue>
            {
                new CssKeywordValue("at"),
                new CssPercentageValue(25),
                new CssPercentageValue(75),
                new CssColorValue(new CssColor(255, 0, 0, 255)),
                new CssColorValue(new CssColor(0, 0, 255, 255)),
            });

            BackgroundPainter.Paint(box, target, null);

            var gradient = target.FilledRects[0].Brush.Gradient;
            Assert.Equal(0.25f, gradient.Center.X, 0.01);
            Assert.Equal(0.75f, gradient.Center.Y, 0.01);
        }

        [Fact]
        public void ConicGradient_AtKeywords_SetsCenter()
        {
            var target = new GradientRecordingTarget();
            var box = CreateGradientBox("conic-gradient", new List<CssValue>
            {
                new CssKeywordValue("at"),
                new CssKeywordValue("left"),
                new CssKeywordValue("top"),
                new CssColorValue(new CssColor(255, 0, 0, 255)),
                new CssColorValue(new CssColor(0, 0, 255, 255)),
            });

            BackgroundPainter.Paint(box, target, null);

            var gradient = target.FilledRects[0].Brush.Gradient;
            Assert.Equal(0f, gradient.Center.X, 0.01);
            Assert.Equal(0f, gradient.Center.Y, 0.01);
        }

        // ═══════════════════════════════════════════
        // Combined from + at
        // ═══════════════════════════════════════════

        [Fact]
        public void ConicGradient_FromAngleAtPosition_SetsBoth()
        {
            var target = new GradientRecordingTarget();
            var box = CreateGradientBox("conic-gradient", new List<CssValue>
            {
                new CssKeywordValue("from"),
                new CssDimensionValue(180, "deg"),
                new CssKeywordValue("at"),
                new CssPercentageValue(0),
                new CssPercentageValue(100),
                new CssColorValue(new CssColor(255, 0, 0, 255)),
                new CssColorValue(new CssColor(0, 0, 255, 255)),
            });

            BackgroundPainter.Paint(box, target, null);

            var gradient = target.FilledRects[0].Brush.Gradient;
            Assert.Equal(180f, gradient.Angle, 0.01);
            Assert.Equal(0f, gradient.Center.X, 0.01);
            Assert.Equal(1f, gradient.Center.Y, 0.01);
        }

        // ═══════════════════════════════════════════
        // Color stops with explicit positions
        // ═══════════════════════════════════════════

        [Fact]
        public void ConicGradient_ExplicitStopPositions_Preserved()
        {
            var target = new GradientRecordingTarget();
            var box = CreateGradientBox("conic-gradient", new List<CssValue>
            {
                new CssColorValue(new CssColor(255, 0, 0, 255)),
                new CssPercentageValue(30),
                new CssColorValue(new CssColor(0, 255, 0, 255)),
                new CssPercentageValue(70),
                new CssColorValue(new CssColor(0, 0, 255, 255)),
            });

            BackgroundPainter.Paint(box, target, null);

            var gradient = target.FilledRects[0].Brush.Gradient;
            Assert.Equal(3, gradient.Stops.Length);
            Assert.Equal(0.30f, gradient.Stops[0].Position, 0.01);
            Assert.Equal(0.70f, gradient.Stops[1].Position, 0.01);
            Assert.Equal(1f, gradient.Stops[2].Position, 0.01);
        }

        [Fact]
        public void ConicGradient_SingleStop_ReturnsNull()
        {
            var target = new GradientRecordingTarget();
            var box = CreateGradientBox("conic-gradient", new List<CssValue>
            {
                new CssColorValue(new CssColor(255, 0, 0, 255)),
            });

            BackgroundPainter.Paint(box, target, null);

            // Single stop is not a valid gradient — no fill
            Assert.Empty(target.FilledRects);
        }

        // ═══════════════════════════════════════════
        // Named color stops
        // ═══════════════════════════════════════════

        [Fact]
        public void ConicGradient_NamedColors_Parsed()
        {
            var target = new GradientRecordingTarget();
            var box = CreateGradientBox("conic-gradient", new List<CssValue>
            {
                new CssKeywordValue("red"),
                new CssKeywordValue("blue"),
            });

            BackgroundPainter.Paint(box, target, null);

            Assert.Single(target.FilledRects);
            var gradient = target.FilledRects[0].Brush.Gradient;
            Assert.Equal(GradientType.Conic, gradient.Type);
            Assert.Equal(2, gradient.Stops.Length);
            Assert.Equal(255, gradient.Stops[0].Color.R);
            Assert.Equal(0, gradient.Stops[0].Color.G);
            Assert.Equal(255, gradient.Stops[1].Color.B);
        }

        // ═══════════════════════════════════════════
        // Helpers
        // ═══════════════════════════════════════════

        private static LayoutBox CreateGradientBox(string fnName, List<CssValue> args,
            float x = 0, float y = 0, float w = 200, float h = 200)
        {
            var values = new PropertyValue[PropertyId.Count];
            var refValues = new object?[PropertyId.Count];
            InitialValues.CopyTo(values, refValues);

            var gradientFn = new CssFunctionValue(fnName, args);
            refValues[PropertyId.BackgroundImage] = gradientFn;

            var style = new ComputedStyle(values, refValues);
            var styledNode = new StyledText("test", style);
            var box = new LayoutBox(styledNode, BoxType.Block);
            box.ContentRect = new RectF(x, y, w, h);

            return box;
        }

        private sealed class GradientRecordingTarget : IRenderTarget
        {
            public List<(RectF Rect, BrushInfo Brush)> FilledRects { get; } = new List<(RectF, BrushInfo)>();

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
                // Gradient can come through FillPath when border-radius is set,
                // but for tests we only track FillRect
                FilledRects.Add((new RectF(0, 0, 0, 0), brush));
            }
        }
    }
}
