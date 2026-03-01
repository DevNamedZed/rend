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

        // --- CSS Color Level 4 ---

        [Fact]
        public void FromHwb_Red()
        {
            // hwb(0 0% 0%) = red
            var c = CssColor.FromHwb(0f, 0f, 0f);
            Assert.Equal(255, c.R);
            Assert.Equal(0, c.G);
            Assert.Equal(0, c.B);
        }

        [Fact]
        public void FromHwb_White()
        {
            // hwb(0 100% 0%) = white
            var c = CssColor.FromHwb(0f, 1f, 0f);
            Assert.Equal(255, c.R);
            Assert.Equal(255, c.G);
            Assert.Equal(255, c.B);
        }

        [Fact]
        public void FromHwb_Black()
        {
            // hwb(0 0% 100%) = black
            var c = CssColor.FromHwb(0f, 0f, 1f);
            Assert.Equal(0, c.R);
            Assert.Equal(0, c.G);
            Assert.Equal(0, c.B);
        }

        [Fact]
        public void FromHwb_Gray_WhenWhitenessAndBlacknessExceed1()
        {
            // hwb(0 80% 80%) = gray (normalized 50%)
            var c = CssColor.FromHwb(0f, 0.8f, 0.8f);
            Assert.Equal(128, c.R);
            Assert.Equal(128, c.G);
            Assert.Equal(128, c.B);
        }

        [Fact]
        public void FromHwb_WithAlpha()
        {
            var c = CssColor.FromHwb(0f, 0f, 0f, 0.5f);
            Assert.Equal(255, c.R);
            Assert.Equal(128, c.A);
        }

        [Fact]
        public void FromLab_White()
        {
            // lab(100 0 0) = white
            var c = CssColor.FromLab(100f, 0f, 0f);
            Assert.InRange(c.R, 250, 255);
            Assert.InRange(c.G, 250, 255);
            Assert.InRange(c.B, 250, 255);
        }

        [Fact]
        public void FromLab_Black()
        {
            // lab(0 0 0) = black
            var c = CssColor.FromLab(0f, 0f, 0f);
            Assert.InRange(c.R, 0, 5);
            Assert.InRange(c.G, 0, 5);
            Assert.InRange(c.B, 0, 5);
        }

        [Fact]
        public void FromLab_Red_Approximate()
        {
            // lab(53.23 80.11 67.22) ≈ red
            var c = CssColor.FromLab(53.23f, 80.11f, 67.22f);
            Assert.InRange(c.R, 240, 255);
            Assert.InRange(c.G, 0, 15);
            Assert.InRange(c.B, 0, 15);
        }

        [Fact]
        public void FromLch_Red_Approximate()
        {
            // lch(53.23 104.55 40) ≈ red
            var c = CssColor.FromLch(53.23f, 104.55f, 40f);
            Assert.InRange(c.R, 240, 255);
            Assert.InRange(c.G, 0, 20);
        }

        [Fact]
        public void FromOklab_White()
        {
            // oklab(1 0 0) = white
            var c = CssColor.FromOklab(1f, 0f, 0f);
            Assert.InRange(c.R, 250, 255);
            Assert.InRange(c.G, 250, 255);
            Assert.InRange(c.B, 250, 255);
        }

        [Fact]
        public void FromOklab_Black()
        {
            // oklab(0 0 0) = black
            var c = CssColor.FromOklab(0f, 0f, 0f);
            Assert.Equal(0, c.R);
            Assert.Equal(0, c.G);
            Assert.Equal(0, c.B);
        }

        [Fact]
        public void FromOklch_Red_Approximate()
        {
            // oklch(0.6278 0.2577 29.23) ≈ red
            var c = CssColor.FromOklch(0.6278f, 0.2577f, 29.23f);
            Assert.InRange(c.R, 240, 255);
            Assert.InRange(c.G, 0, 20);
        }

        [Fact]
        public void FromOklch_WithAlpha()
        {
            var c = CssColor.FromOklch(1f, 0f, 0f, 0.5f);
            Assert.InRange(c.R, 250, 255);
            Assert.Equal(128, c.A);
        }

        [Fact]
        public void Mix_Equal_Weights()
        {
            var red = CssColor.FromRgba(255, 0, 0);
            var blue = CssColor.FromRgba(0, 0, 255);
            var mixed = CssColor.Mix(red, 0.5f, blue, 0.5f);
            Assert.Equal(128, mixed.R);
            Assert.Equal(0, mixed.G);
            Assert.Equal(128, mixed.B);
        }

        [Fact]
        public void Mix_Weighted()
        {
            var red = CssColor.FromRgba(255, 0, 0);
            var blue = CssColor.FromRgba(0, 0, 255);
            var mixed = CssColor.Mix(red, 0.75f, blue, 0.25f);
            Assert.Equal(191, mixed.R);
            Assert.Equal(0, mixed.G);
            Assert.Equal(64, mixed.B);
        }
    }
}
