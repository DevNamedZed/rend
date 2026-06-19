using Rend.Css;
using Rend.Style;
using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    /// <summary>
    /// Tests covering CSS2 box model, margin collapsing, padding percentage resolution,
    /// box-sizing, border-style/border-width interactions, and auto margins.
    /// </summary>
    public class WptBoxModelTests
    {
        private readonly ITestOutputHelper _output;
        public WptBoxModelTests(ITestOutputHelper output) { _output = output; }

        // [CSS2 §8.1] padding percentage resolves against containing block WIDTH (even vertical)
        [Fact] public void PaddingPercent_Vertical_ResolvesAgainstWidth() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='width:200px'><div id='t' style='padding-top:10%;padding-bottom:10%;height:0'></div></div></body>");
            var t = LayoutTestHelper.FindById(r,"t")!;
            Assert.True(System.Math.Abs(t.PaddingTop - 20) < 2);
            Assert.True(System.Math.Abs(t.PaddingBottom - 20) < 2);
        }

        // [CSS2 §8.3] margin: auto on left+right centers block
        [Fact] public void MarginAuto_Centers_Block() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='width:400px'><div id='t' style='width:200px;margin:0 auto;height:20px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.X - 100) < 2);
        }

        // [CSS2 §8.3] margin: auto on single side absorbs remaining space
        [Fact] public void MarginAuto_Left_Only() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='width:400px'><div id='t' style='width:100px;margin-left:auto;height:20px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.X - 300) < 2);
        }

        // [CSS2 §8.3.1] negative margins collapse correctly
        [Fact] public void NegativeMargin_Collapse() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='width:200px'><div style='margin-bottom:20px;height:30px'></div><div id='t' style='margin-top:-10px;height:30px'></div></div></body>");
            // max(20,0) + min(0,-10) = 20 + (-10) = 10
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 40) < 2);
        }

        // [CSS2 §8.3.1] self-collapsing element passes margins through
        [Fact] public void SelfCollapsing_Margins() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='width:200px'><div style='margin-bottom:20px;height:30px'></div><div style='margin-top:15px;margin-bottom:25px'></div><div id='t' style='margin-top:10px;height:30px'></div></div></body>");
            // Collapse: max(20,15,25,10) = 25
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 55) < 2);
        }

        // [CSS-UI §3.2] box-sizing: border-box
        [Fact] public void BoxSizing_BorderBox_Width() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='t' style='box-sizing:border-box;width:100px;padding:10px;border:5px solid;height:50px'></div></body>");
            var t = LayoutTestHelper.FindById(r,"t")!;
            Assert.True(System.Math.Abs(t.ContentRect.Width - 70) < 1);
        }

        // [CSS-UI §3.2] box-sizing: border-box height
        [Fact] public void BoxSizing_BorderBox_Height() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='t' style='box-sizing:border-box;width:100px;height:80px;padding:10px;border:5px solid'></div></body>");
            var t = LayoutTestHelper.FindById(r,"t")!;
            Assert.True(System.Math.Abs(t.ContentRect.Height - 50) < 1);
        }

        // [CSS2 §8.5.1] border-width computed to 0 when border-style is none
        [Fact] public void BorderNone_ZeroWidth() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='t' style='border:5px none red;width:100px;height:50px'></div></body>");
            Assert.Equal(0, LayoutTestHelper.FindById(r,"t")!.BorderTopWidth);
        }

        // [CSS2 §8.5] border-style: hidden same as none
        [Fact] public void BorderHidden_ZeroWidth() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='t' style='border:5px hidden red;width:100px;height:50px'></div></body>");
            Assert.Equal(0, LayoutTestHelper.FindById(r,"t")!.BorderTopWidth);
        }

        // [CSS2 §8.5] border widths: thin/medium/thick
        [Fact] public void BorderWidth_Thin() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='t' style='border:thin solid black;width:100px;height:50px'></div></body>");
            Assert.Equal(1, LayoutTestHelper.FindById(r,"t")!.BorderTopWidth);
        }

        [Fact] public void BorderWidth_Medium() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='t' style='border:medium solid black;width:100px;height:50px'></div></body>");
            Assert.Equal(3, LayoutTestHelper.FindById(r,"t")!.BorderTopWidth);
        }

        [Fact] public void BorderWidth_Thick() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='t' style='border:thick solid black;width:100px;height:50px'></div></body>");
            Assert.Equal(5, LayoutTestHelper.FindById(r,"t")!.BorderTopWidth);
        }

        // [CSS2 §8.4] padding shorthand: 1/2/3/4 values
        [Fact] public void Padding_1Value() {
            var r = LayoutTestHelper.Layout("<body style='margin:0;overflow:hidden'><div id='t' style='padding:10px;width:50px;height:50px'></div></body>");
            var t = LayoutTestHelper.FindById(r,"t")!;
            Assert.Equal(10, t.PaddingTop); Assert.Equal(10, t.PaddingRight);
            Assert.Equal(10, t.PaddingBottom); Assert.Equal(10, t.PaddingLeft);
        }

        [Fact] public void Padding_2Values() {
            var r = LayoutTestHelper.Layout("<body style='margin:0;overflow:hidden'><div id='t' style='padding:10px 20px;width:50px;height:50px'></div></body>");
            var t = LayoutTestHelper.FindById(r,"t")!;
            Assert.Equal(10, t.PaddingTop); Assert.Equal(20, t.PaddingRight);
            Assert.Equal(10, t.PaddingBottom); Assert.Equal(20, t.PaddingLeft);
        }

        [Fact] public void Padding_3Values() {
            var r = LayoutTestHelper.Layout("<body style='margin:0;overflow:hidden'><div id='t' style='padding:10px 20px 30px;width:50px;height:50px'></div></body>");
            var t = LayoutTestHelper.FindById(r,"t")!;
            Assert.Equal(10, t.PaddingTop); Assert.Equal(20, t.PaddingRight);
            Assert.Equal(30, t.PaddingBottom); Assert.Equal(20, t.PaddingLeft);
        }

        [Fact] public void Padding_4Values() {
            var r = LayoutTestHelper.Layout("<body style='margin:0;overflow:hidden'><div id='t' style='padding:10px 20px 30px 40px;width:50px;height:50px'></div></body>");
            var t = LayoutTestHelper.FindById(r,"t")!;
            Assert.Equal(10, t.PaddingTop); Assert.Equal(20, t.PaddingRight);
            Assert.Equal(30, t.PaddingBottom); Assert.Equal(40, t.PaddingLeft);
        }

        // [CSS2 §8.3] margin shorthand: 1/2/3/4 values
        [Fact] public void Margin_1Value() {
            var r = LayoutTestHelper.Layout("<body style='margin:0;overflow:hidden'><div id='t' style='margin:15px;width:50px;height:50px'></div></body>");
            var t = LayoutTestHelper.FindById(r,"t")!;
            Assert.Equal(15, t.StyledNode!.Style.MarginTop); Assert.Equal(15, t.StyledNode!.Style.MarginRight);
            Assert.Equal(15, t.StyledNode!.Style.MarginBottom); Assert.Equal(15, t.StyledNode!.Style.MarginLeft);
        }

        [Fact] public void Margin_3Values() {
            var r = LayoutTestHelper.Layout("<body style='margin:0;overflow:hidden'><div id='t' style='margin:10px 20px 30px;width:50px;height:50px'></div></body>");
            var t = LayoutTestHelper.FindById(r,"t")!;
            Assert.Equal(10, t.StyledNode!.Style.MarginTop); Assert.Equal(20, t.StyledNode!.Style.MarginRight);
            Assert.Equal(30, t.StyledNode!.Style.MarginBottom); Assert.Equal(20, t.StyledNode!.Style.MarginLeft);
        }

        [Fact] public void Margin_4Values() {
            var r = LayoutTestHelper.Layout("<body style='margin:0;overflow:hidden'><div id='t' style='margin:10px 20px 30px 40px;width:50px;height:50px'></div></body>");
            var t = LayoutTestHelper.FindById(r,"t")!;
            Assert.Equal(10, t.StyledNode!.Style.MarginTop); Assert.Equal(20, t.StyledNode!.Style.MarginRight);
            Assert.Equal(30, t.StyledNode!.Style.MarginBottom); Assert.Equal(40, t.StyledNode!.Style.MarginLeft);
        }

        // [CSS2 §10.6.7] auto height includes float bottom
        [Fact] public void AutoHeight_IncludesFloat_InBFC() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='t' style='overflow:hidden;width:200px'><div style='float:left;width:50px;height:80px'></div><div style='height:30px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height >= 79);
        }

        // [CSS2 §10.3.3] width auto with margin
        [Fact] public void AutoWidth_WithPaddingBorderMargin() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='width:300px'><div id='t' style='margin:0 10px;padding:0 5px;border:2px solid;height:20px'></div></div></body>");
            // width = 300 - 10*2(margin) - 5*2(padding) - 2*2(border) = 300 - 34 = 266
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 266) < 2);
        }
    }
}
