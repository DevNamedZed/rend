using System;
using Rend.Core.Values;
using Xunit;

namespace Rend.Core.Tests
{
    public class CssColorTests
    {
        private const float Epsilon = 1e-2f;

        [Fact]
        public void Constructor_SetsRGBA()
        {
            var c = new CssColor(10, 20, 30, 40);
            Assert.Equal(10, c.R);
            Assert.Equal(20, c.G);
            Assert.Equal(30, c.B);
            Assert.Equal(40, c.A);
        }

        [Fact]
        public void Constructor_DefaultAlpha_Is255()
        {
            var c = new CssColor(100, 150, 200);
            Assert.Equal(255, c.A);
        }

        [Fact]
        public void Black_IsCorrect()
        {
            Assert.Equal(0, CssColor.Black.R);
            Assert.Equal(0, CssColor.Black.G);
            Assert.Equal(0, CssColor.Black.B);
            Assert.Equal(255, CssColor.Black.A);
        }

        [Fact]
        public void White_IsCorrect()
        {
            Assert.Equal(255, CssColor.White.R);
            Assert.Equal(255, CssColor.White.G);
            Assert.Equal(255, CssColor.White.B);
            Assert.Equal(255, CssColor.White.A);
        }

        [Fact]
        public void Transparent_IsCorrect()
        {
            Assert.Equal(0, CssColor.Transparent.R);
            Assert.Equal(0, CssColor.Transparent.G);
            Assert.Equal(0, CssColor.Transparent.B);
            Assert.Equal(0, CssColor.Transparent.A);
        }

        [Fact]
        public void FromRgba_CreatesCorrectColor()
        {
            var c = CssColor.FromRgba(10, 20, 30, 40);
            Assert.Equal(10, c.R);
            Assert.Equal(20, c.G);
            Assert.Equal(30, c.B);
            Assert.Equal(40, c.A);
        }

        [Fact]
        public void FromRgba_DefaultAlpha_Is255()
        {
            var c = CssColor.FromRgba(10, 20, 30);
            Assert.Equal(255, c.A);
        }

        [Fact]
        public void FromHsl_Red()
        {
            // HSL(0, 1, 0.5) = pure red
            var c = CssColor.FromHsl(0f, 1f, 0.5f);
            Assert.Equal(255, c.R);
            Assert.Equal(0, c.G);
            Assert.Equal(0, c.B);
            Assert.Equal(255, c.A);
        }

        [Fact]
        public void FromHsl_Green()
        {
            // HSL(120, 1, 0.5) = pure green
            var c = CssColor.FromHsl(120f, 1f, 0.5f);
            Assert.Equal(0, c.R);
            Assert.Equal(255, c.G);
            Assert.Equal(0, c.B);
        }

        [Fact]
        public void FromHsl_Blue()
        {
            // HSL(240, 1, 0.5) = pure blue
            var c = CssColor.FromHsl(240f, 1f, 0.5f);
            Assert.Equal(0, c.R);
            Assert.Equal(0, c.G);
            Assert.Equal(255, c.B);
        }

        [Fact]
        public void FromHsl_White()
        {
            // HSL(0, 0, 1) = white
            var c = CssColor.FromHsl(0f, 0f, 1f);
            Assert.Equal(255, c.R);
            Assert.Equal(255, c.G);
            Assert.Equal(255, c.B);
        }

        [Fact]
        public void FromHsl_Black()
        {
            // HSL(0, 0, 0) = black
            var c = CssColor.FromHsl(0f, 0f, 0f);
            Assert.Equal(0, c.R);
            Assert.Equal(0, c.G);
            Assert.Equal(0, c.B);
        }

        [Fact]
        public void FromHsl_Cyan()
        {
            // HSL(180, 1, 0.5) = cyan
            var c = CssColor.FromHsl(180f, 1f, 0.5f);
            Assert.Equal(0, c.R);
            Assert.Equal(255, c.G);
            Assert.Equal(255, c.B);
        }

        [Fact]
        public void FromHsl_Yellow()
        {
            // HSL(60, 1, 0.5) = yellow
            var c = CssColor.FromHsl(60f, 1f, 0.5f);
            Assert.Equal(255, c.R);
            Assert.Equal(255, c.G);
            Assert.Equal(0, c.B);
        }

        [Fact]
        public void FromHsl_Magenta()
        {
            // HSL(300, 1, 0.5) = magenta
            var c = CssColor.FromHsl(300f, 1f, 0.5f);
            Assert.Equal(255, c.R);
            Assert.Equal(0, c.G);
            Assert.Equal(255, c.B);
        }

        [Fact]
        public void FromHsl_WithAlpha()
        {
            var c = CssColor.FromHsl(0f, 1f, 0.5f, 0.5f);
            Assert.Equal(255, c.R);
            Assert.Equal(0, c.G);
            Assert.Equal(0, c.B);
            Assert.Equal(128, c.A);
        }

        [Fact]
        public void FromHsl_NegativeHue_WrapsAround()
        {
            // -60 degrees should be same as 300 degrees (magenta)
            var c = CssColor.FromHsl(-60f, 1f, 0.5f);
            Assert.Equal(255, c.R);
            Assert.Equal(0, c.G);
            Assert.Equal(255, c.B);
        }

        [Fact]
        public void FromHsl_HueOver360_WrapsAround()
        {
            // 420 degrees should be same as 60 degrees (yellow)
            var c = CssColor.FromHsl(420f, 1f, 0.5f);
            Assert.Equal(255, c.R);
            Assert.Equal(255, c.G);
            Assert.Equal(0, c.B);
        }

        [Fact]
        public void FromHsl_SaturationClamped()
        {
            // Saturation > 1 should be clamped to 1
            var c = CssColor.FromHsl(0f, 2f, 0.5f);
            Assert.Equal(255, c.R);
            Assert.Equal(0, c.G);
            Assert.Equal(0, c.B);
        }

        [Fact]
        public void ToFloatRgb_ReturnsNormalizedValues()
        {
            var c = new CssColor(255, 128, 0);
            c.ToFloatRgb(out float r, out float g, out float b);
            Assert.Equal(1f, r, Epsilon);
            Assert.Equal(128f / 255f, g, Epsilon);
            Assert.Equal(0f, b, Epsilon);
        }

        [Fact]
        public void AlphaF_ReturnsNormalizedAlpha()
        {
            var opaque = new CssColor(0, 0, 0, 255);
            Assert.Equal(1f, opaque.AlphaF, Epsilon);

            var transparent = new CssColor(0, 0, 0, 0);
            Assert.Equal(0f, transparent.AlphaF, Epsilon);

            var half = new CssColor(0, 0, 0, 128);
            Assert.Equal(128f / 255f, half.AlphaF, Epsilon);
        }

        [Fact]
        public void Equals_SameValues_ReturnsTrue()
        {
            var a = new CssColor(10, 20, 30, 40);
            var b = new CssColor(10, 20, 30, 40);
            Assert.True(a.Equals(b));
            Assert.True(a == b);
            Assert.False(a != b);
        }

        [Fact]
        public void Equals_DifferentValues_ReturnsFalse()
        {
            var a = new CssColor(10, 20, 30, 40);
            Assert.False(a.Equals(new CssColor(99, 20, 30, 40)));
            Assert.False(a.Equals(new CssColor(10, 99, 30, 40)));
            Assert.False(a.Equals(new CssColor(10, 20, 99, 40)));
            Assert.False(a.Equals(new CssColor(10, 20, 30, 99)));
        }

        [Fact]
        public void Equals_BoxedObject_Works()
        {
            var a = new CssColor(10, 20, 30, 40);
            object b = new CssColor(10, 20, 30, 40);
            Assert.True(a.Equals(b));
        }

        [Fact]
        public void Equals_NonColorObject_ReturnsFalse()
        {
            var a = new CssColor(10, 20, 30);
            Assert.False(a.Equals("not a color"));
            Assert.False(a.Equals(null));
        }

        [Fact]
        public void GetHashCode_EqualColors_SameHash()
        {
            var a = new CssColor(10, 20, 30, 40);
            var b = new CssColor(10, 20, 30, 40);
            Assert.Equal(a.GetHashCode(), b.GetHashCode());
        }

        [Fact]
        public void ToString_OpaqueColor_UsesRgbFormat()
        {
            var c = new CssColor(10, 20, 30);
            Assert.Equal("rgb(10, 20, 30)", c.ToString());
        }

        [Fact]
        public void ToString_TransparentColor_UsesRgbaFormat()
        {
            var c = new CssColor(10, 20, 30, 128);
            var str = c.ToString();
            Assert.StartsWith("rgba(10, 20, 30,", str);
        }
    }
}
