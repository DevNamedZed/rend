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
    public class OutlineRenderingTests
    {
        [Fact]
        public void Paint_NoOutline_NothingRendered()
        {
            var target = new OutlineRecordingTarget();
            var box = CreateBoxWithOutline(CssBorderStyle.None, 0, CssColor.Black, 0);

            OutlinePainter.Paint(box, target);

            Assert.Empty(target.StrokedRects);
        }

        [Fact]
        public void Paint_SolidOutline_StrokesRect()
        {
            var target = new OutlineRecordingTarget();
            var box = CreateBoxWithOutline(CssBorderStyle.Solid, 2f, new CssColor(255, 0, 0, 255), 0);

            OutlinePainter.Paint(box, target);

            Assert.Single(target.StrokedRects);
            var (_, pen) = target.StrokedRects[0];
            Assert.Equal(255, pen.Color.R);
            Assert.Equal(2f, pen.Width, 0.01);
        }

        [Fact]
        public void Paint_DashedOutline_HasDashPattern()
        {
            var target = new OutlineRecordingTarget();
            var box = CreateBoxWithOutline(CssBorderStyle.Dashed, 2f, CssColor.Black, 0);

            OutlinePainter.Paint(box, target);

            Assert.Single(target.StrokedRects);
            var (_, pen) = target.StrokedRects[0];
            Assert.NotNull(pen.DashPattern);
        }

        [Fact]
        public void Paint_DottedOutline_HasDashPattern()
        {
            var target = new OutlineRecordingTarget();
            var box = CreateBoxWithOutline(CssBorderStyle.Dotted, 3f, CssColor.Black, 0);

            OutlinePainter.Paint(box, target);

            Assert.Single(target.StrokedRects);
            var (_, pen) = target.StrokedRects[0];
            Assert.NotNull(pen.DashPattern);
        }

        [Fact]
        public void Paint_DoubleOutline_TwoStrokes()
        {
            var target = new OutlineRecordingTarget();
            var box = CreateBoxWithOutline(CssBorderStyle.Double, 6f, CssColor.Black, 0);

            OutlinePainter.Paint(box, target);

            Assert.Equal(2, target.StrokedRects.Count);
        }

        [Fact]
        public void Paint_WithOffset_ExpandsRect()
        {
            var target = new OutlineRecordingTarget();
            var box = CreateBoxWithOutline(CssBorderStyle.Solid, 2f, CssColor.Black, 5f);

            OutlinePainter.Paint(box, target);

            Assert.Single(target.StrokedRects);
            var (rect, _) = target.StrokedRects[0];
            // Box is at (100, 100, 200, 50). With offset=5, width=2 (half=1),
            // expand = 5 + 1 = 6
            Assert.True(rect.X < 100f);
            Assert.True(rect.Y < 100f);
            Assert.True(rect.Width > 200f);
            Assert.True(rect.Height > 50f);
        }

        [Fact]
        public void Paint_TransparentColor_NothingRendered()
        {
            var target = new OutlineRecordingTarget();
            var box = CreateBoxWithOutline(CssBorderStyle.Solid, 2f, CssColor.Transparent, 0);

            OutlinePainter.Paint(box, target);

            Assert.Empty(target.StrokedRects);
        }

        [Fact]
        public void Paint_ZeroWidth_NothingRendered()
        {
            var target = new OutlineRecordingTarget();
            var box = CreateBoxWithOutline(CssBorderStyle.Solid, 0f, CssColor.Black, 0);

            OutlinePainter.Paint(box, target);

            Assert.Empty(target.StrokedRects);
        }

        private static LayoutBox CreateBoxWithOutline(CssBorderStyle outlineStyle, float outlineWidth,
            CssColor outlineColor, float outlineOffset)
        {
            var values = new PropertyValue[PropertyId.Count];
            var refValues = new object?[PropertyId.Count];
            InitialValues.CopyTo(values, refValues);

            values[PropertyId.OutlineStyle] = PropertyValue.FromKeyword((int)outlineStyle);
            values[PropertyId.OutlineWidth] = PropertyValue.FromLength(outlineWidth);
            values[PropertyId.OutlineColor] = PropertyValue.FromColor(outlineColor);
            values[PropertyId.OutlineOffset] = PropertyValue.FromLength(outlineOffset);

            var style = new ComputedStyle(values, refValues);
            var styledNode = new StyledText("test", style);
            var box = new LayoutBox(styledNode, BoxType.Block);
            box.ContentRect = new RectF(100, 100, 200, 50);

            return box;
        }

        private sealed class OutlineRecordingTarget : IRenderTarget
        {
            public List<(RectF Rect, PenInfo Pen)> StrokedRects { get; } = new List<(RectF, PenInfo)>();

            public void BeginPage(float width, float height) { }
            public void EndPage() { }
            public void Save() { }
            public void Restore() { }
            public void SetTransform(Matrix3x2 transform) { }
            public void SetOpacity(float opacity) { }
            public void SetBlendMode(Rend.Css.CssMixBlendMode blendMode) { }
            public void SetImageRendering(Rend.Css.CssImageRendering rendering) { }
            public void SetMaskBlur(float sigma) { }
            public void PushClipRect(RectF rect) { }
            public void PushClipPath(PathData path) { }
            public void PopClip() { }
            public void FillRect(RectF rect, BrushInfo brush) { }
            public void FillPath(PathData path, BrushInfo brush) { }
            public void StrokePath(PathData path, PenInfo pen) { }
            public void DrawImage(ImageData image, RectF destRect) { }
            public void DrawText(string text, float x, float y, TextStyle style) { }
            public void DrawGlyphs(ShapedTextRun run, float x, float y, CssColor color, FontDescriptor font) { }
            public void Finish(Stream output) { }
            public void AddLink(RectF rect, string uri) { }
            public void AddBookmark(string title, int level, float yPosition) { }

            public void StrokeRect(RectF rect, PenInfo pen)
            {
                StrokedRects.Add((rect, pen));
            }
        }
    }
}
