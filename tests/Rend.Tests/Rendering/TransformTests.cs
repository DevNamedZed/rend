using System;
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
    public class TransformTests
    {
        private const float Tolerance = 0.01f;

        // ═══════════════════════════════════════════
        // BuildTransformMatrix — individual functions
        // ═══════════════════════════════════════════

        [Fact]
        public void BuildTransformMatrix_Translate()
        {
            var fn = new CssFunctionValue("translate", new List<CssValue>
            {
                new CssDimensionValue(10, "px"),
                new CssDimensionValue(20, "px"),
            });

            var m = TransformHandler.BuildTransformMatrix(fn);

            Assert.Equal(1f, m.M11, Tolerance);
            Assert.Equal(0f, m.M12, Tolerance);
            Assert.Equal(0f, m.M21, Tolerance);
            Assert.Equal(1f, m.M22, Tolerance);
            Assert.Equal(10f, m.M31, Tolerance);
            Assert.Equal(20f, m.M32, Tolerance);
        }

        [Fact]
        public void BuildTransformMatrix_TranslateX()
        {
            var fn = new CssFunctionValue("translateX", new List<CssValue>
            {
                new CssDimensionValue(15, "px"),
            });

            var m = TransformHandler.BuildTransformMatrix(fn);

            Assert.Equal(15f, m.M31, Tolerance);
            Assert.Equal(0f, m.M32, Tolerance);
        }

        [Fact]
        public void BuildTransformMatrix_TranslateY()
        {
            var fn = new CssFunctionValue("translateY", new List<CssValue>
            {
                new CssDimensionValue(25, "px"),
            });

            var m = TransformHandler.BuildTransformMatrix(fn);

            Assert.Equal(0f, m.M31, Tolerance);
            Assert.Equal(25f, m.M32, Tolerance);
        }

        [Fact]
        public void BuildTransformMatrix_Scale()
        {
            var fn = new CssFunctionValue("scale", new List<CssValue>
            {
                new CssNumberValue(2),
                new CssNumberValue(3),
            });

            var m = TransformHandler.BuildTransformMatrix(fn);

            Assert.Equal(2f, m.M11, Tolerance);
            Assert.Equal(3f, m.M22, Tolerance);
        }

        [Fact]
        public void BuildTransformMatrix_ScaleUniform()
        {
            // scale(2) — sy defaults to sx
            var fn = new CssFunctionValue("scale", new List<CssValue>
            {
                new CssNumberValue(2),
            });

            var m = TransformHandler.BuildTransformMatrix(fn);

            Assert.Equal(2f, m.M11, Tolerance);
            Assert.Equal(2f, m.M22, Tolerance);
        }

        [Fact]
        public void BuildTransformMatrix_Rotate90deg()
        {
            var fn = new CssFunctionValue("rotate", new List<CssValue>
            {
                new CssDimensionValue(90, "deg"),
            });

            var m = TransformHandler.BuildTransformMatrix(fn);

            // cos(90°) ≈ 0, sin(90°) ≈ 1
            Assert.Equal(0f, m.M11, Tolerance);
            Assert.Equal(1f, m.M12, Tolerance);
            Assert.Equal(-1f, m.M21, Tolerance);
            Assert.Equal(0f, m.M22, Tolerance);
        }

        [Fact]
        public void BuildTransformMatrix_RotateRad()
        {
            var fn = new CssFunctionValue("rotate", new List<CssValue>
            {
                new CssDimensionValue((float)Math.PI, "rad"),
            });

            var m = TransformHandler.BuildTransformMatrix(fn);

            // cos(π) = -1, sin(π) ≈ 0
            Assert.Equal(-1f, m.M11, Tolerance);
            Assert.Equal(0f, m.M12, Tolerance);
        }

        [Fact]
        public void BuildTransformMatrix_SkewX()
        {
            var fn = new CssFunctionValue("skewX", new List<CssValue>
            {
                new CssDimensionValue(45, "deg"),
            });

            var m = TransformHandler.BuildTransformMatrix(fn);

            Assert.Equal(1f, m.M11, Tolerance);
            Assert.Equal(1f, m.M21, Tolerance); // tan(45°) = 1
            Assert.Equal(0f, m.M12, Tolerance);
            Assert.Equal(1f, m.M22, Tolerance);
        }

        [Fact]
        public void BuildTransformMatrix_Matrix()
        {
            var fn = new CssFunctionValue("matrix", new List<CssValue>
            {
                new CssNumberValue(1), new CssNumberValue(2),
                new CssNumberValue(3), new CssNumberValue(4),
                new CssNumberValue(5), new CssNumberValue(6),
            });

            var m = TransformHandler.BuildTransformMatrix(fn);

            Assert.Equal(1f, m.M11, Tolerance);
            Assert.Equal(2f, m.M12, Tolerance);
            Assert.Equal(3f, m.M21, Tolerance);
            Assert.Equal(4f, m.M22, Tolerance);
            Assert.Equal(5f, m.M31, Tolerance);
            Assert.Equal(6f, m.M32, Tolerance);
        }

        [Fact]
        public void BuildTransformMatrix_NoneKeyword_Identity()
        {
            var value = new CssKeywordValue("none");
            // This is handled in Apply(), but BuildTransformMatrix returns identity for non-function values
            var m = TransformHandler.BuildTransformMatrix(value);
            Assert.Equal(Matrix3x2.Identity, m);
        }

        // ═══════════════════════════════════════════
        // Combined transforms
        // ═══════════════════════════════════════════

        [Fact]
        public void BuildTransformMatrix_CombinedTranslateScale()
        {
            // transform: translate(10px, 0) scale(2)
            var translateFn = new CssFunctionValue("translate", new List<CssValue>
            {
                new CssDimensionValue(10, "px"),
                new CssNumberValue(0),
            });
            var scaleFn = new CssFunctionValue("scale", new List<CssValue>
            {
                new CssNumberValue(2),
            });
            var combined = new CssListValue(new List<CssValue> { translateFn, scaleFn }, ' ');

            var m = TransformHandler.BuildTransformMatrix(combined);

            // translate(10,0) * scale(2) → M31 = 10*2 = 20 (translation is scaled)
            Assert.Equal(2f, m.M11, Tolerance);
            Assert.Equal(2f, m.M22, Tolerance);
            Assert.Equal(20f, m.M31, Tolerance);
        }

        // ═══════════════════════════════════════════
        // Apply with LayoutBox
        // ═══════════════════════════════════════════

        [Fact]
        public void Apply_NoTransform_ReturnsFalse()
        {
            var target = new TransformRecordingTarget();
            var box = CreateBoxWithTransform(null);

            bool applied = TransformHandler.Apply(box, target);

            Assert.False(applied);
            Assert.Empty(target.Transforms);
        }

        [Fact]
        public void Apply_NoneKeyword_ReturnsFalse()
        {
            var target = new TransformRecordingTarget();
            var box = CreateBoxWithTransform(new CssKeywordValue("none"));

            bool applied = TransformHandler.Apply(box, target);

            Assert.False(applied);
        }

        [Fact]
        public void Apply_WithTranslate_SavesAndSetsTransform()
        {
            var target = new TransformRecordingTarget();
            var transformValue = new CssFunctionValue("translate", new List<CssValue>
            {
                new CssDimensionValue(10, "px"),
                new CssDimensionValue(20, "px"),
            });
            var box = CreateBoxWithTransform(transformValue, 50, 50, 100, 100);

            bool applied = TransformHandler.Apply(box, target);

            Assert.True(applied);
            Assert.Equal(1, target.SaveCount);
            Assert.Single(target.Transforms);
        }

        [Fact]
        public void Apply_NullStyledNode_ReturnsFalse()
        {
            var target = new TransformRecordingTarget();
            var box = new LayoutBox(null, BoxType.Block);

            bool applied = TransformHandler.Apply(box, target);

            Assert.False(applied);
        }

        // ═══════════════════════════════════════════
        // Matrix3x2.CreateSkew
        // ═══════════════════════════════════════════

        [Fact]
        public void CreateSkew_Zero_IsIdentity()
        {
            var m = Matrix3x2.CreateSkew(0, 0);
            Assert.Equal(Matrix3x2.Identity, m);
        }

        [Fact]
        public void CreateSkew_45Degrees()
        {
            float angle = (float)(Math.PI / 4); // 45 degrees
            var m = Matrix3x2.CreateSkew(angle, 0);

            Assert.Equal(1f, m.M11, Tolerance);
            Assert.Equal(1f, m.M21, Tolerance); // tan(45°) = 1
            Assert.Equal(0f, m.M12, Tolerance);
            Assert.Equal(1f, m.M22, Tolerance);
        }

        // ═══════════════════════════════════════════
        // Helpers
        // ═══════════════════════════════════════════

        private static LayoutBox CreateBoxWithTransform(CssValue? transformValue,
            float x = 100, float y = 100, float w = 200, float h = 50)
        {
            var values = new PropertyValue[PropertyId.Count];
            var refValues = new object?[PropertyId.Count];
            InitialValues.CopyTo(values, refValues);

            if (transformValue != null)
            {
                refValues[PropertyId.Transform] = transformValue;
            }

            var style = new ComputedStyle(values, refValues);
            var styledNode = new StyledText("test", style);
            var box = new LayoutBox(styledNode, BoxType.Block);
            box.ContentRect = new RectF(x, y, w, h);

            return box;
        }

        private sealed class TransformRecordingTarget : IRenderTarget
        {
            public List<Matrix3x2> Transforms { get; } = new List<Matrix3x2>();
            public int SaveCount { get; private set; }

            public void BeginPage(float width, float height) { }
            public void EndPage() { }
            public void Save() { SaveCount++; }
            public void Restore() { }
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
            public void DrawText(string text, float x, float y, TextStyle style) { }
            public void DrawGlyphs(ShapedTextRun run, float x, float y, CssColor color, FontDescriptor font) { }
            public void Finish(Stream output) { }
            public void AddLink(RectF rect, string uri) { }
            public void AddBookmark(string title, int level, float yPosition) { }

            public void SetTransform(Matrix3x2 transform)
            {
                Transforms.Add(transform);
            }
            public void ConcatTransform(Matrix3x2 transform) { }
        }
    }
}
