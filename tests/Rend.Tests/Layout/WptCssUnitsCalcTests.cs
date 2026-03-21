using Rend.Css;
using Rend.Style;
using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    /// <summary>
    /// Tests for CSS unit resolution, calc(), min(), max(), clamp(),
    /// and percentage resolution in various contexts.
    /// </summary>
    public class WptCssUnitsCalcTests
    {
        private readonly ITestOutputHelper _output;
        public WptCssUnitsCalcTests(ITestOutputHelper output) { _output = output; }

        // [CSS-VALUES §6.1] absolute units
        [Fact] public void Unit_Px() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='t' style='width:100px;height:50px'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 100) < 1);
        }

        [Fact] public void Unit_Pt() {
            // 1pt = 96/72 px ≈ 1.333px; 72pt = 96px
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='t' style='width:72pt;height:10px'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 96) < 2);
        }

        [Fact] public void Unit_In() {
            // 1in = 96px
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='t' style='width:1in;height:10px'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 96) < 2);
        }

        [Fact] public void Unit_Cm() {
            // 1cm = 96/2.54 ≈ 37.8px; 2.54cm = 96px
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='t' style='width:2.54cm;height:10px'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 96) < 2);
        }

        [Fact] public void Unit_Mm() {
            // 10mm = 1cm = 37.8px
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='t' style='width:25.4mm;height:10px'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 96) < 2);
        }

        // [CSS-VALUES §6.1] font-relative units
        [Fact] public void Unit_Em_Nested() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='font-size:16px'><div style='font-size:2em'><div id='t' style='width:1em;height:10px'></div></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 32) < 2);
        }

        [Fact] public void Unit_Rem_IgnoresParent() {
            var r = LayoutTestHelper.Layout("<html style='font-size:10px'><body style='margin:0'><div style='font-size:50px'><div id='t' style='width:10rem;height:10px'></div></div></body></html>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 100) < 2);
        }

        // [CSS-VALUES §6.3] viewport units
        [Fact] public void Unit_Vmin() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='t' style='width:50vmin;height:10px'></div></body>", 400, 300);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 150) < 2);
        }

        [Fact] public void Unit_Vmax() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='t' style='width:50vmax;height:10px'></div></body>", 400, 300);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 200) < 2);
        }

        // [CSS-VALUES §5] percentage in different contexts
        [Fact] public void Percent_Width() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='width:400px'><div id='t' style='width:25%;height:10px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 100) < 2);
        }

        [Fact] public void Percent_Height_Definite() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='width:100px;height:200px'><div id='t' style='height:75%'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height - 150) < 2);
        }

        [Fact] public void Percent_Margin() {
            var r = LayoutTestHelper.Layout("<body style='margin:0;overflow:hidden'><div style='width:200px'><div id='t' style='margin:10%;width:50px;height:10px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.MarginTop - 20) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.MarginLeft - 20) < 2);
        }

        [Fact] public void Percent_Padding_All() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='width:300px'><div id='t' style='padding:5%;height:0'></div></div></body>");
            var t = LayoutTestHelper.FindById(r,"t")!;
            Assert.True(System.Math.Abs(t.PaddingTop - 15) < 2);
            Assert.True(System.Math.Abs(t.PaddingRight - 15) < 2);
            Assert.True(System.Math.Abs(t.PaddingBottom - 15) < 2);
            Assert.True(System.Math.Abs(t.PaddingLeft - 15) < 2);
        }

        // [CSS-VALUES §8.1] calc() expressions
        [Fact] public void Calc_Addition() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='t' style='width:calc(50px + 30px);height:10px'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 80) < 2);
        }

        [Fact] public void Calc_Subtraction() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='t' style='width:calc(100px - 30px);height:10px'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 70) < 2);
        }

        [Fact] public void Calc_Multiplication() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='t' style='width:calc(25px * 4);height:10px'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 100) < 2);
        }

        [Fact] public void Calc_Division() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='t' style='width:calc(200px / 4);height:10px'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 50) < 2);
        }

        [Fact] public void Calc_PercentPx() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='width:400px'><div id='t' style='width:calc(50% + 20px);height:10px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 220) < 2);
        }

        [Fact] public void Calc_Nested() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='t' style='width:calc(calc(50px + 50px) + 20px);height:10px'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 120) < 2);
        }

        [Fact] public void Calc_Height_PercentMinusPx() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='width:100px;height:400px'><div id='t' style='height:calc(100% - 50px)'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height - 350) < 2);
        }

        // [CSS-VALUES §8.2] min() function
        [Fact] public void Min_TwoValues() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='width:400px'><div id='t' style='width:min(300px,50%);height:10px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 200) < 2);
        }

        [Fact] public void Min_ThreeValues() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='t' style='width:min(500px,300px,200px);height:10px'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 200) < 2);
        }

        // [CSS-VALUES §8.2] max() function
        [Fact] public void Max_TwoValues() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='width:400px'><div id='t' style='width:max(100px,25%);height:10px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 100) < 2);
        }

        [Fact] public void Max_PercentWins() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='width:400px'><div id='t' style='width:max(50px,50%);height:10px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 200) < 2);
        }

        // [CSS-VALUES §8.3] clamp() function
        [Fact] public void Clamp_Middle() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='width:400px'><div id='t' style='width:clamp(50px,50%,300px);height:10px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 200) < 2);
        }

        [Fact] public void Clamp_Min() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='width:60px'><div id='t' style='width:clamp(50px,50%,200px);height:10px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 50) < 2);
        }

        [Fact] public void Clamp_Max() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='width:1000px'><div id='t' style='width:clamp(50px,50%,200px);height:10px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 200) < 2);
        }

        // [CSS-VALUES §5] percentage 0% = 0
        [Fact] public void Percent_Zero() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='width:400px'><div id='t' style='width:0%;height:10px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width < 1);
        }

        // [CSS-VALUES §5] percentage 100% = parent
        [Fact] public void Percent_100() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='width:250px'><div id='t' style='width:100%;height:10px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 250) < 2);
        }
    }
}
