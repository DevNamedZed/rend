using Rend.Css;
using Rend.Style;
using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    /// <summary>
    /// Tests for CSS Values and Units: absolute units, relative units,
    /// viewport units, calc(), min(), max(), clamp(), custom properties.
    /// </summary>
    public class WptValuesConformanceTests
    {
        private readonly ITestOutputHelper _output;
        public WptValuesConformanceTests(ITestOutputHelper output) { _output = output; }

        // absolute units
        [Fact] public void Unit_1px() { AssertWidth("1px", 1, 400); }
        [Fact] public void Unit_100px() { AssertWidth("100px", 100, 400); }
        [Fact] public void Unit_72pt() { AssertWidth("72pt", 96, 400); } // 72pt = 1in = 96px
        [Fact] public void Unit_1in() { AssertWidth("1in", 96, 400); }
        [Fact] public void Unit_2_54cm() { AssertWidth("2.54cm", 96, 400); }
        [Fact] public void Unit_25_4mm() { AssertWidth("25.4mm", 96, 400); }

        // font-relative units
        [Fact]
        public void Unit_Em()
        {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='font-size:20px'><div id='t' style='width:5em;height:10px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Width - 100) < 2);
        }

        [Fact]
        public void Unit_Em_Nested()
        {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='font-size:10px'><div style='font-size:2em'><div id='t' style='width:3em;height:10px'></div></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Width - 60) < 2);
        }

        [Fact]
        public void Unit_Rem()
        {
            var r = LayoutTestHelper.Layout("<html style='font-size:20px'><body style='margin:0'><div style='font-size:10px'><div id='t' style='width:5rem;height:10px'></div></div></body></html>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Width - 100) < 2);
        }

        [Fact]
        public void Unit_Rem_DefaultRoot()
        {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='t' style='width:1rem;height:10px'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Width - 16) < 2);
        }

        // viewport units
        [Fact] public void Unit_50vw() { AssertWidthVp("50vw", 200, 400, 300); }
        [Fact] public void Unit_100vw() { AssertWidthVp("100vw", 400, 400, 300); }
        [Fact] public void Unit_25vh() { AssertHeightVp("25vh", 75, 400, 300); }
        [Fact] public void Unit_50vh() { AssertHeightVp("50vh", 150, 400, 300); }
        [Fact] public void Unit_50vmin() { AssertWidthVp("50vmin", 150, 400, 300); }
        [Fact] public void Unit_50vmax() { AssertWidthVp("50vmax", 200, 400, 300); }

        // percentages
        [Fact] public void Percent_50_Width() { AssertWidth("50%", 200, 400); }
        [Fact] public void Percent_100_Width() { AssertWidth("100%", 400, 400); }
        [Fact] public void Percent_25_Width() { AssertWidth("25%", 100, 400); }
        [Fact] public void Percent_0_Width() { AssertWidth("0%", 0, 400); }

        [Fact]
        public void Percent_Height_Definite()
        {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='width:100px;height:200px'><div id='t' style='height:50%'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Height - 100) < 2);
        }

        [Fact]
        public void Percent_Height_Indefinite()
        {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='width:100px'><div id='t' style='height:50%'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r, "t")!.ContentRect.Height < 1);
        }

        [Fact]
        public void Percent_Padding_AgainstWidth()
        {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='width:200px'><div id='t' style='padding:10%;height:0'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.PaddingTop - 20) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.PaddingLeft - 20) < 2);
        }

        [Fact]
        public void Percent_Margin_AgainstWidth()
        {
            var r = LayoutTestHelper.Layout("<body style='margin:0;overflow:hidden'><div style='width:300px'><div id='t' style='margin:10%;width:50px;height:10px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.MarginTop - 30) < 2);
        }

        // calc()
        [Fact] public void Calc_Add() { AssertWidth("calc(50px + 30px)", 80, 400); }
        [Fact] public void Calc_Sub() { AssertWidth("calc(100px - 30px)", 70, 400); }
        [Fact] public void Calc_Mul() { AssertWidth("calc(25px * 4)", 100, 400); }
        [Fact] public void Calc_Div() { AssertWidth("calc(200px / 4)", 50, 400); }
        [Fact] public void Calc_PercentPx() { AssertWidth("calc(50% + 20px)", 220, 400); }
        [Fact] public void Calc_PercentMinusPx() { AssertWidth("calc(50% - 20px)", 180, 400); }

        [Fact]
        public void Calc_Height()
        {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='width:100px;height:400px'><div id='t' style='height:calc(25% + 10px)'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Height - 110) < 2);
        }

        [Fact]
        public void Calc_Nested()
        {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='t' style='width:calc(calc(50px + 50px) + 20px);height:10px'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Width - 120) < 2);
        }

        [Fact]
        public void Calc_WithEm()
        {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='font-size:16px'><div id='t' style='width:calc(10em + 20px);height:10px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Width - 180) < 2);
        }

        [Fact]
        public void Calc_WithVw()
        {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='t' style='width:calc(50vw + 20px);height:10px'></div></body>", 400, 300);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Width - 220) < 2);
        }

        // min()
        [Fact]
        public void Min_PxPercent()
        {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='width:400px'><div id='t' style='width:min(300px,50%);height:10px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Width - 200) < 2);
        }

        [Fact]
        public void Min_PxWins()
        {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='width:400px'><div id='t' style='width:min(100px,80%);height:10px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Width - 100) < 2);
        }

        // max()
        [Fact]
        public void Max_PxPercent()
        {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='width:400px'><div id='t' style='width:max(100px,30%);height:10px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Width - 120) < 2);
        }

        [Fact]
        public void Max_PercentWins()
        {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='width:400px'><div id='t' style='width:max(50px,50%);height:10px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Width - 200) < 2);
        }

        // clamp()
        [Fact]
        public void Clamp_Middle()
        {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='width:400px'><div id='t' style='width:clamp(50px,40%,300px);height:10px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Width - 160) < 2);
        }

        [Fact]
        public void Clamp_Min()
        {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='width:100px'><div id='t' style='width:clamp(80px,10%,200px);height:10px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Width - 80) < 2);
        }

        [Fact]
        public void Clamp_Max()
        {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='width:1000px'><div id='t' style='width:clamp(50px,80%,200px);height:10px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Width - 200) < 2);
        }

        // CSS custom properties
        [Fact]
        public void Var_CustomProperty()
        {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='--w:150px'><div id='t' style='width:var(--w);height:10px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Width - 150) < 2);
        }

        [Fact]
        public void Var_Fallback()
        {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='t' style='width:var(--undefined,120px);height:10px'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Width - 120) < 2);
        }

        [Fact]
        public void Var_Inherited()
        {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='--size:80px'><div><div id='t' style='width:var(--size);height:var(--size)'></div></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Width - 80) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Height - 80) < 2);
        }

        // inherit keyword
        [Fact]
        public void Inherit_FontSize()
        {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='font-size:24px'><div id='t' style='font-size:inherit;width:10px;height:10px'>x</div></div></body>");
            Assert.True(System.Math.Abs(((LayoutTestHelper.FindById(r, "t")!.StyledNode as StyledElement)!).Style.FontSize - 24) < 1);
        }

        // initial keyword
        [Fact]
        public void Initial_FontSize()
        {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='font-size:24px'><div id='t' style='font-size:initial;width:10px;height:10px'>x</div></div></body>");
            Assert.True(System.Math.Abs(((LayoutTestHelper.FindById(r, "t")!.StyledNode as StyledElement)!).Style.FontSize - 16) < 1);
        }

        // unset on inherited = inherit
        [Fact]
        public void Unset_Inherited()
        {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='font-size:24px'><div id='t' style='font-size:unset;width:10px;height:10px'>x</div></div></body>");
            Assert.True(System.Math.Abs(((LayoutTestHelper.FindById(r, "t")!.StyledNode as StyledElement)!).Style.FontSize - 24) < 1);
        }

        private void AssertWidth(string css, float expected, float containerWidth)
        {
            var r = LayoutTestHelper.Layout($"<body style='margin:0'><div style='width:{containerWidth}px'><div id='t' style='width:{css};height:10px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Width - expected) < 2,
                $"{css} = {expected} (got {LayoutTestHelper.FindById(r, "t")!.ContentRect.Width})");
        }

        private void AssertWidthVp(string css, float expected, float vw, float vh)
        {
            var r = LayoutTestHelper.Layout($"<body style='margin:0'><div id='t' style='width:{css};height:10px'></div></body>", vw, vh);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Width - expected) < 2);
        }

        private void AssertHeightVp(string css, float expected, float vw, float vh)
        {
            var r = LayoutTestHelper.Layout($"<body style='margin:0'><div id='t' style='width:10px;height:{css}'></div></body>", vw, vh);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Height - expected) < 2);
        }
    }
}
