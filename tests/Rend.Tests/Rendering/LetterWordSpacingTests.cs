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
    public class LetterWordSpacingTests
    {
        // ═══════════════════════════════════════════
        // TextStyle properties
        // ═══════════════════════════════════════════

        [Fact]
        public void TextStyle_Default_SpacingIsZero()
        {
            var style = new TextStyle();
            Assert.Equal(0f, style.LetterSpacing);
            Assert.Equal(0f, style.WordSpacing);
        }

        [Fact]
        public void TextStyle_LetterSpacing_CanBeSet()
        {
            var style = new TextStyle { LetterSpacing = 2.5f };
            Assert.Equal(2.5f, style.LetterSpacing);
        }

        [Fact]
        public void TextStyle_WordSpacing_CanBeSet()
        {
            var style = new TextStyle { WordSpacing = 5f };
            Assert.Equal(5f, style.WordSpacing);
        }

        // ═══════════════════════════════════════════
        // TextPainter integration: spacing passed to DrawText
        // ═══════════════════════════════════════════

        [Fact]
        public void Paint_WithLetterSpacing_UsesDrawText()
        {
            var target = new TextRecordingTarget();
            var fragment = CreateTextFragment("Hello", letterSpacing: 2f);
            var style = CreateStyle(letterSpacing: 2f);

            TextPainter.Paint(fragment, 0, 0, 10f, target, style);

            // When letter-spacing is set, should use DrawText (not DrawGlyphs) to pass spacing
            Assert.Single(target.TextCalls);
            Assert.Equal(2f, target.TextCalls[0].Style.LetterSpacing);
        }

        [Fact]
        public void Paint_WithWordSpacing_UsesDrawText()
        {
            var target = new TextRecordingTarget();
            var fragment = CreateTextFragment("Hello World", wordSpacing: 4f);
            var style = CreateStyle(wordSpacing: 4f);

            TextPainter.Paint(fragment, 0, 0, 10f, target, style);

            Assert.Single(target.TextCalls);
            Assert.Equal(4f, target.TextCalls[0].Style.WordSpacing);
        }

        [Fact]
        public void Paint_WithBothSpacings_BothPassedThrough()
        {
            var target = new TextRecordingTarget();
            var fragment = CreateTextFragment("Hello World", letterSpacing: 1f, wordSpacing: 3f);
            var style = CreateStyle(letterSpacing: 1f, wordSpacing: 3f);

            TextPainter.Paint(fragment, 0, 0, 10f, target, style);

            Assert.Single(target.TextCalls);
            Assert.Equal(1f, target.TextCalls[0].Style.LetterSpacing);
            Assert.Equal(3f, target.TextCalls[0].Style.WordSpacing);
        }

        [Fact]
        public void Paint_NoSpacing_UsesDrawGlyphs()
        {
            var target = new TextRecordingTarget();
            var shapedRun = new ShapedTextRun(
                new[] { new ShapedGlyph(1, 0, 10f, 0f, 0f, 0f) },
                "H", 16f);
            var fragment = new LineFragment
            {
                X = 0, Width = 10, Height = 16, Baseline = 12,
                ShapedRun = shapedRun
            };
            var style = CreateStyle();

            TextPainter.Paint(fragment, 0, 0, 10f, target, style);

            // No spacing → should use DrawGlyphs path
            Assert.Single(target.GlyphCalls);
            Assert.Empty(target.TextCalls);
        }

        [Fact]
        public void Paint_WithSpacingAndShapedRun_FallsBackToDrawText()
        {
            var target = new TextRecordingTarget();
            var shapedRun = new ShapedTextRun(
                new[] { new ShapedGlyph(1, 0, 10f, 0f, 0f, 0f), new ShapedGlyph(2, 1, 10f, 0f, 0f, 0f) },
                "Hi", 16f);
            var fragment = new LineFragment
            {
                X = 0, Width = 20, Height = 16, Baseline = 12,
                ShapedRun = shapedRun
            };
            var style = CreateStyle(letterSpacing: 2f);

            TextPainter.Paint(fragment, 0, 0, 10f, target, style);

            // Spacing set → should fall back to DrawText even with ShapedRun
            Assert.Single(target.TextCalls);
            Assert.Equal("Hi", target.TextCalls[0].Text);
            Assert.Equal(2f, target.TextCalls[0].Style.LetterSpacing);
        }

        [Fact]
        public void Paint_NegativeLetterSpacing_PassedThrough()
        {
            var target = new TextRecordingTarget();
            var fragment = CreateTextFragment("Hello", letterSpacing: -0.5f);
            var style = CreateStyle(letterSpacing: -0.5f);

            TextPainter.Paint(fragment, 0, 0, 10f, target, style);

            Assert.Single(target.TextCalls);
            Assert.Equal(-0.5f, target.TextCalls[0].Style.LetterSpacing);
        }

        // ═══════════════════════════════════════════
        // Helpers
        // ═══════════════════════════════════════════

        private static LineFragment CreateTextFragment(string text, float letterSpacing = 0, float wordSpacing = 0)
        {
            return new LineFragment
            {
                X = 0,
                Width = text.Length * 10,
                Height = 16,
                Baseline = 12,
                Text = text
            };
        }

        private static ComputedStyle CreateStyle(float letterSpacing = 0, float wordSpacing = 0)
        {
            var values = new PropertyValue[PropertyId.Count];
            var refValues = new object?[PropertyId.Count];
            InitialValues.CopyTo(values, refValues);

            if (letterSpacing != 0)
                values[PropertyId.LetterSpacing] = PropertyValue.FromLength(letterSpacing);
            if (wordSpacing != 0)
                values[PropertyId.WordSpacing] = PropertyValue.FromLength(wordSpacing);

            return new ComputedStyle(values, refValues);
        }

        private sealed class TextRecordingTarget : IRenderTarget
        {
            public List<(string Text, TextStyle Style)> TextCalls { get; } = new List<(string, TextStyle)>();
            public List<(ShapedTextRun Run, CssColor Color)> GlyphCalls { get; } = new List<(ShapedTextRun, CssColor)>();

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
            public void SetMaskBlur(float sigma) { }
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
                TextCalls.Add((text, style));
            }

            public void DrawGlyphs(ShapedTextRun run, float x, float y, CssColor color, FontDescriptor font)
            {
                GlyphCalls.Add((run, color));
            }
        }
    }
}
