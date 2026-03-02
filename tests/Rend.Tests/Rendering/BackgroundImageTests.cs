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
    public class BackgroundImageTests
    {
        // ═══════════════════════════════════════════
        // No background-image
        // ═══════════════════════════════════════════

        [Fact]
        public void Paint_NoImage_OnlyColor()
        {
            var target = new BgRecordingTarget();
            var box = CreateBox(bgColor: new CssColor(255, 0, 0, 255));

            BackgroundPainter.Paint(box, target, null);

            Assert.Single(target.FilledRects);
            Assert.Empty(target.DrawnImages);
        }

        [Fact]
        public void Paint_NoneImage_OnlyColor()
        {
            var target = new BgRecordingTarget();
            var box = CreateBox(bgColor: new CssColor(255, 0, 0, 255), bgImageUrl: "none");
            ImageResolverDelegate resolver = _ => null;

            BackgroundPainter.Paint(box, target, resolver);

            Assert.Single(target.FilledRects);
            Assert.Empty(target.DrawnImages);
        }

        // ═══════════════════════════════════════════
        // Basic background-image
        // ═══════════════════════════════════════════

        [Fact]
        public void Paint_WithImage_DrawsImage()
        {
            var target = new BgRecordingTarget();
            var imageData = new ImageData(new byte[] { 1, 2, 3 }, 100, 50, "png");
            var box = CreateBox(bgImageUrl: "test.png",
                bgRepeat: (int)CssBackgroundRepeat.NoRepeat);
            ImageResolverDelegate resolver = src => src == "test.png" ? imageData : null;

            BackgroundPainter.Paint(box, target, resolver);

            Assert.Single(target.DrawnImages);
            var (img, rect) = target.DrawnImages[0];
            Assert.Same(imageData, img);
        }

        [Fact]
        public void Paint_ImageNotFound_NoImageDrawn()
        {
            var target = new BgRecordingTarget();
            var box = CreateBox(bgImageUrl: "missing.png",
                bgRepeat: (int)CssBackgroundRepeat.NoRepeat);
            ImageResolverDelegate resolver = _ => null;

            BackgroundPainter.Paint(box, target, resolver);

            Assert.Empty(target.DrawnImages);
        }

        // ═══════════════════════════════════════════
        // Background-position
        // ═══════════════════════════════════════════

        [Fact]
        public void Paint_PositionTopLeft_ImageAtOrigin()
        {
            var target = new BgRecordingTarget();
            var imageData = new ImageData(new byte[] { 1 }, 50, 50, "png");
            var position = new CssListValue(new List<CssValue>
            {
                new CssPercentageValue(0),
                new CssPercentageValue(0),
            }, ' ');
            var box = CreateBox(bgImageUrl: "test.png",
                bgRepeat: (int)CssBackgroundRepeat.NoRepeat,
                bgPosition: position,
                x: 10, y: 20, w: 200, h: 100);
            ImageResolverDelegate resolver = _ => imageData;

            BackgroundPainter.Paint(box, target, resolver);

            Assert.Single(target.DrawnImages);
            var (_, rect) = target.DrawnImages[0];
            Assert.Equal(10f, rect.X, 0.01);
            Assert.Equal(20f, rect.Y, 0.01);
        }

        [Fact]
        public void Paint_PositionCenter_ImageCentered()
        {
            var target = new BgRecordingTarget();
            var imageData = new ImageData(new byte[] { 1 }, 50, 50, "png");
            var position = new CssListValue(new List<CssValue>
            {
                new CssPercentageValue(50),
                new CssPercentageValue(50),
            }, ' ');
            var box = CreateBox(bgImageUrl: "test.png",
                bgRepeat: (int)CssBackgroundRepeat.NoRepeat,
                bgPosition: position,
                x: 0, y: 0, w: 200, h: 100);
            ImageResolverDelegate resolver = _ => imageData;

            BackgroundPainter.Paint(box, target, resolver);

            Assert.Single(target.DrawnImages);
            var (_, rect) = target.DrawnImages[0];
            // CSS: position = (containerSize - imageSize) * percentage
            Assert.Equal(75f, rect.X, 0.01); // (200-50)*0.5 = 75
            Assert.Equal(25f, rect.Y, 0.01); // (100-50)*0.5 = 25
        }

        // ═══════════════════════════════════════════
        // Background-size
        // ═══════════════════════════════════════════

        [Fact]
        public void Paint_SizeCover_FillsContainer()
        {
            var target = new BgRecordingTarget();
            var imageData = new ImageData(new byte[] { 1 }, 100, 50, "png"); // 2:1 aspect
            var box = CreateBox(bgImageUrl: "test.png",
                bgRepeat: (int)CssBackgroundRepeat.NoRepeat,
                bgSize: new CssKeywordValue("cover"),
                x: 0, y: 0, w: 200, h: 200);
            ImageResolverDelegate resolver = _ => imageData;

            BackgroundPainter.Paint(box, target, resolver);

            Assert.Single(target.DrawnImages);
            var (_, rect) = target.DrawnImages[0];
            // Cover: max ratio. ratioW=200/100=2, ratioH=200/50=4, use 4
            Assert.Equal(400f, rect.Width, 0.01);
            Assert.Equal(200f, rect.Height, 0.01);
        }

        [Fact]
        public void Paint_SizeContain_FitsContainer()
        {
            var target = new BgRecordingTarget();
            var imageData = new ImageData(new byte[] { 1 }, 100, 50, "png"); // 2:1 aspect
            var box = CreateBox(bgImageUrl: "test.png",
                bgRepeat: (int)CssBackgroundRepeat.NoRepeat,
                bgSize: new CssKeywordValue("contain"),
                x: 0, y: 0, w: 200, h: 200);
            ImageResolverDelegate resolver = _ => imageData;

            BackgroundPainter.Paint(box, target, resolver);

            Assert.Single(target.DrawnImages);
            var (_, rect) = target.DrawnImages[0];
            // Contain: min ratio. ratioW=200/100=2, ratioH=200/50=4, use 2
            Assert.Equal(200f, rect.Width, 0.01);
            Assert.Equal(100f, rect.Height, 0.01);
        }

        [Fact]
        public void Paint_SizeExplicit_ScalesImage()
        {
            var target = new BgRecordingTarget();
            var imageData = new ImageData(new byte[] { 1 }, 100, 50, "png");
            var sizeValue = new CssListValue(new List<CssValue>
            {
                new CssDimensionValue(150, "px"),
                new CssDimensionValue(75, "px"),
            }, ' ');
            var box = CreateBox(bgImageUrl: "test.png",
                bgRepeat: (int)CssBackgroundRepeat.NoRepeat,
                bgSize: sizeValue,
                x: 0, y: 0, w: 200, h: 200);
            ImageResolverDelegate resolver = _ => imageData;

            BackgroundPainter.Paint(box, target, resolver);

            Assert.Single(target.DrawnImages);
            var (_, rect) = target.DrawnImages[0];
            Assert.Equal(150f, rect.Width, 0.01);
            Assert.Equal(75f, rect.Height, 0.01);
        }

        // ═══════════════════════════════════════════
        // Background-repeat
        // ═══════════════════════════════════════════

        [Fact]
        public void Paint_RepeatBoth_TilesImage()
        {
            var target = new BgRecordingTarget();
            var imageData = new ImageData(new byte[] { 1 }, 50, 50, "png");
            var box = CreateBox(bgImageUrl: "test.png",
                bgRepeat: (int)CssBackgroundRepeat.Repeat,
                x: 0, y: 0, w: 100, h: 100);
            ImageResolverDelegate resolver = _ => imageData;

            BackgroundPainter.Paint(box, target, resolver);

            // 100/50 = 2 tiles in each direction = 4 total
            Assert.Equal(4, target.DrawnImages.Count);
        }

        [Fact]
        public void Paint_RepeatX_TilesHorizontally()
        {
            var target = new BgRecordingTarget();
            var imageData = new ImageData(new byte[] { 1 }, 50, 50, "png");
            var box = CreateBox(bgImageUrl: "test.png",
                bgRepeat: (int)CssBackgroundRepeat.RepeatX,
                x: 0, y: 0, w: 100, h: 100);
            ImageResolverDelegate resolver = _ => imageData;

            BackgroundPainter.Paint(box, target, resolver);

            // 100/50 = 2 tiles horizontally, 1 vertically = 2 total
            Assert.Equal(2, target.DrawnImages.Count);
        }

        [Fact]
        public void Paint_RepeatY_TilesVertically()
        {
            var target = new BgRecordingTarget();
            var imageData = new ImageData(new byte[] { 1 }, 50, 50, "png");
            var box = CreateBox(bgImageUrl: "test.png",
                bgRepeat: (int)CssBackgroundRepeat.RepeatY,
                x: 0, y: 0, w: 100, h: 100);
            ImageResolverDelegate resolver = _ => imageData;

            BackgroundPainter.Paint(box, target, resolver);

            // 1 horizontally, 100/50 = 2 vertically = 2 total
            Assert.Equal(2, target.DrawnImages.Count);
        }

        [Fact]
        public void Paint_NoRepeat_SingleImage()
        {
            var target = new BgRecordingTarget();
            var imageData = new ImageData(new byte[] { 1 }, 50, 50, "png");
            var box = CreateBox(bgImageUrl: "test.png",
                bgRepeat: (int)CssBackgroundRepeat.NoRepeat,
                x: 0, y: 0, w: 200, h: 200);
            ImageResolverDelegate resolver = _ => imageData;

            BackgroundPainter.Paint(box, target, resolver);

            Assert.Single(target.DrawnImages);
        }

        // ═══════════════════════════════════════════
        // Clipping
        // ═══════════════════════════════════════════

        [Fact]
        public void Paint_Repeat_ClipsToContainer()
        {
            var target = new BgRecordingTarget();
            var imageData = new ImageData(new byte[] { 1 }, 50, 50, "png");
            var box = CreateBox(bgImageUrl: "test.png",
                bgRepeat: (int)CssBackgroundRepeat.Repeat,
                x: 0, y: 0, w: 100, h: 100);
            ImageResolverDelegate resolver = _ => imageData;

            BackgroundPainter.Paint(box, target, resolver);

            // Should push clip rect before tiling, pop after
            Assert.Equal(1, target.ClipPushCount);
            Assert.Equal(1, target.ClipPopCount);
        }

        [Fact]
        public void Paint_NoRepeat_NoClipping()
        {
            var target = new BgRecordingTarget();
            var imageData = new ImageData(new byte[] { 1 }, 50, 50, "png");
            var box = CreateBox(bgImageUrl: "test.png",
                bgRepeat: (int)CssBackgroundRepeat.NoRepeat,
                x: 0, y: 0, w: 200, h: 200);
            ImageResolverDelegate resolver = _ => imageData;

            BackgroundPainter.Paint(box, target, resolver);

            Assert.Equal(0, target.ClipPushCount);
        }

        // ═══════════════════════════════════════════
        // Background-origin / background-clip
        // ═══════════════════════════════════════════

        [Fact]
        public void Paint_OriginContentBox_UsesContentRect()
        {
            var target = new BgRecordingTarget();
            var box = CreateBox(bgColor: new CssColor(255, 0, 0, 255),
                bgOrigin: CssBackgroundOrigin.ContentBox);
            box.PaddingTop = 10;
            box.PaddingLeft = 10;
            box.PaddingBottom = 10;
            box.PaddingRight = 10;

            BackgroundPainter.Paint(box, target, null);

            // Background-clip defaults to border-box, so color fills border rect.
            // Origin only affects image positioning, not color.
            Assert.Single(target.FilledRects);
        }

        [Fact]
        public void Paint_ClipContentBox_FillsContentRect()
        {
            var target = new BgRecordingTarget();
            var box = CreateBox(bgColor: new CssColor(255, 0, 0, 255),
                bgClip: CssBackgroundClip.ContentBox);
            box.PaddingTop = 10;
            box.PaddingLeft = 10;
            box.PaddingBottom = 10;
            box.PaddingRight = 10;
            box.ContentRect = new RectF(10, 10, 200, 200);

            BackgroundPainter.Paint(box, target, null);

            Assert.Single(target.FilledRects);
            var fillRect = target.FilledRects[0].Rect;
            // Content rect should be used for clipping
            Assert.Equal(10f, fillRect.X);
            Assert.Equal(10f, fillRect.Y);
            Assert.Equal(200f, fillRect.Width);
            Assert.Equal(200f, fillRect.Height);
        }

        [Fact]
        public void Paint_OriginPaddingBox_Default()
        {
            // Default origin is padding-box — verify it works
            var target = new BgRecordingTarget();
            var imageData = new ImageData(new byte[] { 1 }, 50, 50, "png");
            var box = CreateBox(bgImageUrl: "test.png",
                bgRepeat: (int)CssBackgroundRepeat.NoRepeat);
            ImageResolverDelegate resolver = _ => imageData;

            BackgroundPainter.Paint(box, target, resolver);

            Assert.Single(target.DrawnImages);
        }

        // ═══════════════════════════════════════════
        // Helpers
        // ═══════════════════════════════════════════

        private static LayoutBox CreateBox(
            CssColor? bgColor = null,
            string? bgImageUrl = null,
            int bgRepeat = 0,
            CssValue? bgPosition = null,
            CssValue? bgSize = null,
            CssBackgroundOrigin? bgOrigin = null,
            CssBackgroundClip? bgClip = null,
            float x = 0, float y = 0, float w = 200, float h = 200)
        {
            var values = new PropertyValue[PropertyId.Count];
            var refValues = new object?[PropertyId.Count];
            InitialValues.CopyTo(values, refValues);

            if (bgColor.HasValue)
            {
                values[PropertyId.BackgroundColor] = PropertyValue.FromColor(bgColor.Value);
            }

            if (bgImageUrl != null)
            {
                refValues[PropertyId.BackgroundImage] = bgImageUrl;
            }

            values[PropertyId.BackgroundRepeat] = PropertyValue.FromKeyword(bgRepeat);

            if (bgPosition != null)
            {
                refValues[PropertyId.BackgroundPosition] = bgPosition;
            }

            if (bgSize != null)
            {
                refValues[PropertyId.BackgroundSize] = bgSize;
            }

            if (bgOrigin.HasValue)
            {
                values[PropertyId.BackgroundOrigin] = PropertyValue.FromKeyword((int)bgOrigin.Value);
            }

            if (bgClip.HasValue)
            {
                values[PropertyId.BackgroundClip] = PropertyValue.FromKeyword((int)bgClip.Value);
            }

            var style = new ComputedStyle(values, refValues);
            var styledNode = new StyledText("test", style);
            var box = new LayoutBox(styledNode, BoxType.Block);
            box.ContentRect = new RectF(x, y, w, h);

            return box;
        }

        private sealed class BgRecordingTarget : IRenderTarget
        {
            public List<(RectF Rect, BrushInfo Brush)> FilledRects { get; } = new List<(RectF, BrushInfo)>();
            public List<(ImageData Image, RectF Rect)> DrawnImages { get; } = new List<(ImageData, RectF)>();
            public int ClipPushCount { get; private set; }
            public int ClipPopCount { get; private set; }

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
            public void PushClipRect(RectF rect) { ClipPushCount++; }
            public void PushClipPath(PathData path) { ClipPushCount++; }
            public void PopClip() { ClipPopCount++; }
            public void StrokeRect(RectF rect, PenInfo pen) { }
            public void FillPath(PathData path, BrushInfo brush) { }
            public void StrokePath(PathData path, PenInfo pen) { }
            public void DrawText(string text, float x, float y, TextStyle style) { }
            public void DrawGlyphs(ShapedTextRun run, float x, float y, CssColor color, FontDescriptor font) { }
            public void Finish(Stream output) { }
            public void AddLink(RectF rect, string uri) { }
            public void AddBookmark(string title, int level, float yPosition) { }

            public void FillRect(RectF rect, BrushInfo brush)
            {
                FilledRects.Add((rect, brush));
            }

            public void DrawImage(ImageData image, RectF destRect)
            {
                DrawnImages.Add((image, destRect));
            }
        }
    }
}
