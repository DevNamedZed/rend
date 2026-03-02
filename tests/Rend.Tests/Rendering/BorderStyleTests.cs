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
    public class BorderStyleTests
    {
        [Fact]
        public void Paint_Groove_RendersTwoTrapezoids()
        {
            var target = new BorderRecordingTarget();
            var box = CreateBoxWithBorder(CssBorderStyle.Groove, 4f, new CssColor(128, 128, 128, 255));

            BorderPainter.Paint(box, target);

            // Groove renders outer half (dark) + inner half (light) for each side
            // 4 sides × 2 halves = 8 filled paths
            Assert.True(target.FilledPaths.Count >= 8);
        }

        [Fact]
        public void Paint_Ridge_RendersTwoTrapezoids()
        {
            var target = new BorderRecordingTarget();
            var box = CreateBoxWithBorder(CssBorderStyle.Ridge, 4f, new CssColor(128, 128, 128, 255));

            BorderPainter.Paint(box, target);

            // Ridge is the opposite of groove: same 2-half rendering per side
            Assert.True(target.FilledPaths.Count >= 8);
        }

        [Fact]
        public void Paint_Inset_RendersAllSides()
        {
            var target = new BorderRecordingTarget();
            var box = CreateBoxWithBorder(CssBorderStyle.Inset, 3f, new CssColor(128, 128, 128, 255));

            BorderPainter.Paint(box, target);

            // Inset renders each side with either dark or light color
            Assert.True(target.FilledPaths.Count >= 4);
        }

        [Fact]
        public void Paint_Outset_RendersAllSides()
        {
            var target = new BorderRecordingTarget();
            var box = CreateBoxWithBorder(CssBorderStyle.Outset, 3f, new CssColor(128, 128, 128, 255));

            BorderPainter.Paint(box, target);

            Assert.True(target.FilledPaths.Count >= 4);
        }

        [Fact]
        public void Paint_Solid_SingleTrapezoidPerSide()
        {
            var target = new BorderRecordingTarget();
            var box = CreateBoxWithBorder(CssBorderStyle.Solid, 2f, CssColor.Black);

            BorderPainter.Paint(box, target);

            // 4 sides, each one trapezoid
            Assert.Equal(4, target.FilledPaths.Count);
        }

        [Fact]
        public void Paint_Dashed_StrokesPerSide()
        {
            var target = new BorderRecordingTarget();
            var box = CreateBoxWithBorder(CssBorderStyle.Dashed, 2f, CssColor.Black);

            BorderPainter.Paint(box, target);

            Assert.Equal(4, target.StrokedPaths.Count);
        }

        private static LayoutBox CreateBoxWithBorder(CssBorderStyle borderStyle, float width, CssColor color)
        {
            var values = new PropertyValue[PropertyId.Count];
            var refValues = new object?[PropertyId.Count];
            InitialValues.CopyTo(values, refValues);

            values[PropertyId.BorderTopStyle] = PropertyValue.FromKeyword((int)borderStyle);
            values[PropertyId.BorderRightStyle] = PropertyValue.FromKeyword((int)borderStyle);
            values[PropertyId.BorderBottomStyle] = PropertyValue.FromKeyword((int)borderStyle);
            values[PropertyId.BorderLeftStyle] = PropertyValue.FromKeyword((int)borderStyle);

            values[PropertyId.BorderTopWidth] = PropertyValue.FromLength(width);
            values[PropertyId.BorderRightWidth] = PropertyValue.FromLength(width);
            values[PropertyId.BorderBottomWidth] = PropertyValue.FromLength(width);
            values[PropertyId.BorderLeftWidth] = PropertyValue.FromLength(width);

            values[PropertyId.BorderTopColor] = PropertyValue.FromColor(color);
            values[PropertyId.BorderRightColor] = PropertyValue.FromColor(color);
            values[PropertyId.BorderBottomColor] = PropertyValue.FromColor(color);
            values[PropertyId.BorderLeftColor] = PropertyValue.FromColor(color);

            var style = new ComputedStyle(values, refValues);
            var styledNode = new StyledText("test", style);
            var box = new LayoutBox(styledNode, BoxType.Block);
            box.ContentRect = new RectF(110, 110, 180, 40);
            box.BorderTopWidth = width;
            box.BorderRightWidth = width;
            box.BorderBottomWidth = width;
            box.BorderLeftWidth = width;

            return box;
        }

        private sealed class BorderRecordingTarget : IRenderTarget
        {
            public List<(PathData Path, BrushInfo Brush)> FilledPaths { get; } = new List<(PathData, BrushInfo)>();
            public List<(PathData Path, PenInfo Pen)> StrokedPaths { get; } = new List<(PathData, PenInfo)>();
            public List<(RectF Rect, BrushInfo Brush)> FilledRects { get; } = new List<(RectF, BrushInfo)>();

            public void BeginPage(float width, float height) { }
            public void EndPage() { }
            public void Save() { }
            public void Restore() { }
            public void SetTransform(Matrix3x2 transform) { }
            public void SetOpacity(float opacity) { }
            public void SetBlendMode(Rend.Css.CssMixBlendMode blendMode) { }
            public void SetImageRendering(Rend.Css.CssImageRendering rendering) { }
            public void ApplyFilter(CssFilterEffect[] effects) { }
            public void SetMaskBlur(float sigma) { }
            public void BeginMask() { }
            public void EndMask(GradientInfo gradient, RectF bounds) { }
            public void PushClipRect(RectF rect) { }
            public void PushClipPath(PathData path) { }
            public void PopClip() { }
            public void StrokeRect(RectF rect, PenInfo pen) { }
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

            public void StrokePath(PathData path, PenInfo pen)
            {
                StrokedPaths.Add((path, pen));
            }
        }
    }
}
