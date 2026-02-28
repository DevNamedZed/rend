using System;
using Rend.Core.Values;
using Xunit;

namespace Rend.Core.Tests
{
    public class CssLengthTests
    {
        private static readonly CssResolutionContext DefaultCtx = CssResolutionContext.Default;
        private const float Epsilon = 1e-3f;

        [Fact]
        public void Constructor_SetsProperties()
        {
            var l = new CssLength(42f, CssLengthUnit.Px);
            Assert.Equal(42f, l.Value);
            Assert.Equal(CssLengthUnit.Px, l.Unit);
        }

        [Fact]
        public void Zero_IsZeroPx()
        {
            Assert.Equal(0f, CssLength.Zero.Value);
            Assert.Equal(CssLengthUnit.Px, CssLength.Zero.Unit);
        }

        [Fact]
        public void Auto_HasAutoUnit()
        {
            Assert.True(CssLength.Auto.IsAuto);
            Assert.Equal(CssLengthUnit.Auto, CssLength.Auto.Unit);
        }

        [Fact]
        public void IsAuto_TrueForAuto()
        {
            Assert.True(CssLength.Auto.IsAuto);
            Assert.False(CssLength.Px(10).IsAuto);
        }

        [Fact]
        public void IsNone_TrueForNone()
        {
            var none = new CssLength(0, CssLengthUnit.None);
            Assert.True(none.IsNone);
            Assert.False(CssLength.Px(10).IsNone);
        }

        [Fact]
        public void IsZero_TrueForZeroWithNonSpecialUnit()
        {
            Assert.True(CssLength.Zero.IsZero);
            Assert.False(CssLength.Auto.IsZero);
            Assert.False(new CssLength(0, CssLengthUnit.None).IsZero);
            Assert.False(CssLength.Px(10).IsZero);
        }

        // Factory methods
        [Fact]
        public void Px_Factory()
        {
            var l = CssLength.Px(100f);
            Assert.Equal(100f, l.Value);
            Assert.Equal(CssLengthUnit.Px, l.Unit);
        }

        [Fact]
        public void Pt_Factory()
        {
            var l = CssLength.Pt(12f);
            Assert.Equal(12f, l.Value);
            Assert.Equal(CssLengthUnit.Pt, l.Unit);
        }

        [Fact]
        public void Em_Factory()
        {
            var l = CssLength.Em(1.5f);
            Assert.Equal(1.5f, l.Value);
            Assert.Equal(CssLengthUnit.Em, l.Unit);
        }

        [Fact]
        public void Rem_Factory()
        {
            var l = CssLength.Rem(2f);
            Assert.Equal(2f, l.Value);
            Assert.Equal(CssLengthUnit.Rem, l.Unit);
        }

        [Fact]
        public void Percent_Factory()
        {
            var l = CssLength.Percent(50f);
            Assert.Equal(50f, l.Value);
            Assert.Equal(CssLengthUnit.Percent, l.Unit);
        }

        [Fact]
        public void Cm_Factory()
        {
            var l = CssLength.Cm(2.54f);
            Assert.Equal(2.54f, l.Value);
            Assert.Equal(CssLengthUnit.Cm, l.Unit);
        }

        [Fact]
        public void Mm_Factory()
        {
            var l = CssLength.Mm(25.4f);
            Assert.Equal(25.4f, l.Value);
            Assert.Equal(CssLengthUnit.Mm, l.Unit);
        }

        [Fact]
        public void In_Factory()
        {
            var l = CssLength.In(1f);
            Assert.Equal(1f, l.Value);
            Assert.Equal(CssLengthUnit.In, l.Unit);
        }

        // ToPx conversions
        [Fact]
        public void ToPx_Px_ReturnsSame()
        {
            Assert.Equal(100f, CssLength.Px(100f).ToPx(DefaultCtx), Epsilon);
        }

        [Fact]
        public void ToPx_Pt_ConvertsCorrectly()
        {
            // 72pt = 96px (1 inch)
            Assert.Equal(96f, CssLength.Pt(72f).ToPx(DefaultCtx), Epsilon);
        }

        [Fact]
        public void ToPx_In_ConvertsCorrectly()
        {
            // 1in = 96px
            Assert.Equal(96f, CssLength.In(1f).ToPx(DefaultCtx), Epsilon);
        }

        [Fact]
        public void ToPx_Cm_ConvertsCorrectly()
        {
            // 2.54cm = 1in = 96px
            Assert.Equal(96f, CssLength.Cm(2.54f).ToPx(DefaultCtx), Epsilon);
        }

        [Fact]
        public void ToPx_Mm_ConvertsCorrectly()
        {
            // 25.4mm = 1in = 96px
            Assert.Equal(96f, CssLength.Mm(25.4f).ToPx(DefaultCtx), Epsilon);
        }

        [Fact]
        public void ToPx_Em_UsesContextFontSize()
        {
            var ctx = new CssResolutionContext(20f, 16f, 1920f, 1080f);
            // 2em with 20px font size = 40px
            Assert.Equal(40f, CssLength.Em(2f).ToPx(ctx), Epsilon);
        }

        [Fact]
        public void ToPx_Rem_UsesRootFontSize()
        {
            var ctx = new CssResolutionContext(20f, 24f, 1920f, 1080f);
            // 2rem with 24px root font size = 48px
            Assert.Equal(48f, CssLength.Rem(2f).ToPx(ctx), Epsilon);
        }

        [Fact]
        public void ToPx_Vw_UsesViewportWidth()
        {
            // Default viewport is 1920 wide
            // 50vw = 960px
            Assert.Equal(960f, new CssLength(50f, CssLengthUnit.Vw).ToPx(DefaultCtx), Epsilon);
        }

        [Fact]
        public void ToPx_Vh_UsesViewportHeight()
        {
            // Default viewport is 1080 tall
            // 50vh = 540px
            Assert.Equal(540f, new CssLength(50f, CssLengthUnit.Vh).ToPx(DefaultCtx), Epsilon);
        }

        [Fact]
        public void ToPx_Vmin_UsesSmallestViewportDimension()
        {
            // Default: min(1920, 1080) = 1080
            // 50vmin = 540px
            Assert.Equal(540f, new CssLength(50f, CssLengthUnit.Vmin).ToPx(DefaultCtx), Epsilon);
        }

        [Fact]
        public void ToPx_Vmax_UsesLargestViewportDimension()
        {
            // Default: max(1920, 1080) = 1920
            // 50vmax = 960px
            Assert.Equal(960f, new CssLength(50f, CssLengthUnit.Vmax).ToPx(DefaultCtx), Epsilon);
        }

        [Fact]
        public void ToPx_Percent_UsesPercentBase()
        {
            var ctx = new CssResolutionContext(16f, 16f, 1920f, 1080f, 500f);
            // 50% of 500 = 250
            Assert.Equal(250f, CssLength.Percent(50f).ToPx(ctx), Epsilon);
        }

        [Fact]
        public void ToPx_Auto_ReturnsZero()
        {
            Assert.Equal(0f, CssLength.Auto.ToPx(DefaultCtx), Epsilon);
        }

        [Fact]
        public void ToPx_None_ReturnsZero()
        {
            var none = new CssLength(0, CssLengthUnit.None);
            Assert.Equal(0f, none.ToPx(DefaultCtx), Epsilon);
        }

        [Fact]
        public void ToPx_Pc_ConvertsCorrectly()
        {
            // 6pc = 1in = 96px
            var pc = new CssLength(6f, CssLengthUnit.Pc);
            Assert.Equal(96f, pc.ToPx(DefaultCtx), Epsilon);
        }

        [Fact]
        public void ToPx_Q_ConvertsCorrectly()
        {
            // 1Q = 1/4 mm, so 101.6Q = 1in = 96px
            var q = new CssLength(101.6f, CssLengthUnit.Q);
            Assert.Equal(96f, q.ToPx(DefaultCtx), Epsilon);
        }

        // ToPt
        [Fact]
        public void ToPt_ConvertsPxToPoints()
        {
            // 96px = 72pt
            Assert.Equal(72f, CssLength.Px(96f).ToPt(DefaultCtx), Epsilon);
        }

        [Fact]
        public void ToPt_InchToPoints()
        {
            // 1in = 96px = 72pt
            Assert.Equal(72f, CssLength.In(1f).ToPt(DefaultCtx), Epsilon);
        }

        // Equality
        [Fact]
        public void Equals_SameValues_ReturnsTrue()
        {
            var a = CssLength.Px(100f);
            var b = CssLength.Px(100f);
            Assert.True(a.Equals(b));
            Assert.True(a == b);
            Assert.False(a != b);
        }

        [Fact]
        public void Equals_DifferentValue_ReturnsFalse()
        {
            var a = CssLength.Px(100f);
            var b = CssLength.Px(200f);
            Assert.False(a.Equals(b));
        }

        [Fact]
        public void Equals_DifferentUnit_ReturnsFalse()
        {
            var a = new CssLength(100f, CssLengthUnit.Px);
            var b = new CssLength(100f, CssLengthUnit.Pt);
            Assert.False(a.Equals(b));
        }

        [Fact]
        public void Equals_BoxedObject_Works()
        {
            var a = CssLength.Px(50f);
            object b = CssLength.Px(50f);
            Assert.True(a.Equals(b));
        }

        [Fact]
        public void Equals_NonCssLengthObject_ReturnsFalse()
        {
            var a = CssLength.Px(50f);
            Assert.False(a.Equals("not a length"));
            Assert.False(a.Equals(null));
        }

        [Fact]
        public void GetHashCode_EqualValues_SameHash()
        {
            var a = CssLength.Em(1.5f);
            var b = CssLength.Em(1.5f);
            Assert.Equal(a.GetHashCode(), b.GetHashCode());
        }

        // ToString
        [Fact]
        public void ToString_Px()
        {
            Assert.Equal("100px", CssLength.Px(100f).ToString());
        }

        [Fact]
        public void ToString_Auto()
        {
            Assert.Equal("auto", CssLength.Auto.ToString());
        }

        [Fact]
        public void ToString_None()
        {
            Assert.Equal("none", new CssLength(0, CssLengthUnit.None).ToString());
        }

        [Fact]
        public void ToString_Em()
        {
            Assert.Equal("1.5em", CssLength.Em(1.5f).ToString());
        }

        [Fact]
        public void ToString_Percent()
        {
            Assert.Equal("50%", CssLength.Percent(50f).ToString());
        }

        [Fact]
        public void ToString_Rem()
        {
            Assert.Equal("2rem", CssLength.Rem(2f).ToString());
        }
    }

    public class CssResolutionContextTests
    {
        [Fact]
        public void Constructor_SetsAllProperties()
        {
            var ctx = new CssResolutionContext(16f, 18f, 1920f, 1080f, 500f);
            Assert.Equal(16f, ctx.FontSize);
            Assert.Equal(18f, ctx.RootFontSize);
            Assert.Equal(1920f, ctx.ViewportWidth);
            Assert.Equal(1080f, ctx.ViewportHeight);
            Assert.Equal(500f, ctx.PercentBase);
        }

        [Fact]
        public void Default_HasExpectedValues()
        {
            var ctx = CssResolutionContext.Default;
            Assert.Equal(16f, ctx.FontSize);
            Assert.Equal(16f, ctx.RootFontSize);
            Assert.Equal(1920f, ctx.ViewportWidth);
            Assert.Equal(1080f, ctx.ViewportHeight);
        }

        [Fact]
        public void Constructor_DefaultPercentBase_IsZero()
        {
            var ctx = new CssResolutionContext(16f, 16f, 1920f, 1080f);
            Assert.Equal(0f, ctx.PercentBase);
        }
    }
}
