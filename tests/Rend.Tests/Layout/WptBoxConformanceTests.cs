using Rend.Css;
using Rend.Style;
using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    /// <summary>
    /// Tests for CSS Box Model: margin, padding, border, box-sizing,
    /// auto margins, negative margins, percentage margins.
    /// </summary>
    public class WptBoxConformanceTests
    {
        private readonly ITestOutputHelper _output;
        public WptBoxConformanceTests(ITestOutputHelper output) { _output = output; }

        // box-sizing: content-box (default)
        [Fact]
        public void ContentBox_WidthIsContentOnly()
        {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='t' style='width:100px;padding:10px;border:5px solid;height:50px'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Width - 100) < 1);
        }

        // box-sizing: border-box
        [Fact]
        public void BorderBox_WidthIncludesPaddingBorder()
        {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='t' style='box-sizing:border-box;width:100px;padding:10px;border:5px solid;height:50px'></div></body>");
            var t = LayoutTestHelper.FindById(r, "t")!;
            float total = t.ContentRect.Width + t.PaddingLeft + t.PaddingRight + t.BorderLeftWidth + t.BorderRightWidth;
            Assert.True(System.Math.Abs(total - 100) < 1);
        }

        // margin: auto centers block
        [Fact]
        public void MarginAuto_Centers()
        {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='width:400px'><div id='t' style='width:200px;margin:0 auto;height:20px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.X - 100) < 2);
        }

        // margin-left: auto pushes right
        [Fact]
        public void MarginLeftAuto_PushesRight()
        {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='width:400px'><div id='t' style='width:100px;margin-left:auto;height:20px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.X - 300) < 2);
        }

        // negative margin pulls element up
        [Fact]
        public void NegativeMarginTop_PullsUp()
        {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='width:200px'><div style='height:50px'></div><div id='t' style='margin-top:-20px;height:30px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r, "t")!.ContentRect.Y < 50);
        }

        // negative margin expands auto width
        [Fact]
        public void NegativeMarginLeft_ExpandsWidth()
        {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='width:200px'><div id='t' style='margin-left:-20px;height:20px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r, "t")!.ContentRect.Width >= 219);
        }

        // padding shorthand: 1 value
        [Fact]
        public void Padding_1Value()
        {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='t' style='padding:15px;width:50px;height:50px'></div></body>");
            var t = LayoutTestHelper.FindById(r, "t")!;
            Assert.Equal(15, t.PaddingTop);
            Assert.Equal(15, t.PaddingRight);
            Assert.Equal(15, t.PaddingBottom);
            Assert.Equal(15, t.PaddingLeft);
        }

        // padding shorthand: 2 values
        [Fact]
        public void Padding_2Values()
        {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='t' style='padding:10px 20px;width:50px;height:50px'></div></body>");
            var t = LayoutTestHelper.FindById(r, "t")!;
            Assert.Equal(10, t.PaddingTop);
            Assert.Equal(20, t.PaddingRight);
            Assert.Equal(10, t.PaddingBottom);
            Assert.Equal(20, t.PaddingLeft);
        }

        // padding shorthand: 4 values
        [Fact]
        public void Padding_4Values()
        {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='t' style='padding:5px 10px 15px 20px;width:50px;height:50px'></div></body>");
            var t = LayoutTestHelper.FindById(r, "t")!;
            Assert.Equal(5, t.PaddingTop);
            Assert.Equal(10, t.PaddingRight);
            Assert.Equal(15, t.PaddingBottom);
            Assert.Equal(20, t.PaddingLeft);
        }

        // margin shorthand: 2 values
        [Fact]
        public void Margin_2Values()
        {
            var r = LayoutTestHelper.Layout("<body style='margin:0;overflow:hidden'><div id='t' style='margin:10px 20px;width:50px;height:50px'></div></body>");
            var t = LayoutTestHelper.FindById(r, "t")!;
            Assert.Equal(10, t.StyledNode!.Style.MarginTop);
            Assert.Equal(20, t.StyledNode!.Style.MarginRight);
            Assert.Equal(10, t.StyledNode!.Style.MarginBottom);
            Assert.Equal(20, t.StyledNode!.Style.MarginLeft);
        }

        // margin shorthand: 4 values
        [Fact]
        public void Margin_4Values()
        {
            var r = LayoutTestHelper.Layout("<body style='margin:0;overflow:hidden'><div id='t' style='margin:5px 10px 15px 20px;width:50px;height:50px'></div></body>");
            var t = LayoutTestHelper.FindById(r, "t")!;
            Assert.Equal(5, t.StyledNode!.Style.MarginTop);
            Assert.Equal(10, t.StyledNode!.Style.MarginRight);
            Assert.Equal(15, t.StyledNode!.Style.MarginBottom);
            Assert.Equal(20, t.StyledNode!.Style.MarginLeft);
        }

        // percentage padding resolves against parent WIDTH
        [Fact]
        public void PercentPadding_AgainstWidth()
        {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='width:200px'><div id='t' style='padding:10%;height:0'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.PaddingTop - 20) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.PaddingBottom - 20) < 2);
        }

        // percentage margin resolves against parent WIDTH
        [Fact]
        public void PercentMargin_AgainstWidth()
        {
            var r = LayoutTestHelper.Layout("<body style='margin:0;overflow:hidden'><div style='width:400px'><div id='t' style='margin:5%;width:50px;height:10px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.MarginTop - 20) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.MarginLeft - 20) < 2);
        }

        // auto width = container - margin - padding - border
        [Fact]
        public void AutoWidth_SubtractsAll()
        {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='width:300px'><div id='t' style='margin:0 10px;padding:0 15px;border:5px solid;height:20px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Width - 240) < 2);
        }

        // visibility:hidden takes space
        [Fact]
        public void VisibilityHidden_TakesSpace()
        {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='width:200px'><div style='visibility:hidden;height:50px'></div><div id='t' style='height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Y - 50) < 2);
        }

        // display:none takes no space
        [Fact]
        public void DisplayNone_NoSpace()
        {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='width:200px'><div style='display:none;height:100px'></div><div id='t' style='height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Y - 0) < 2);
        }

        // min-width overrides max-width when min > max
        [Fact]
        public void MinWidth_OverridesMaxWidth()
        {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='width:400px'><div id='t' style='min-width:200px;max-width:100px;height:20px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r, "t")!.ContentRect.Width >= 199);
        }

        // min-height overrides max-height when min > max
        [Fact]
        public void MinHeight_OverridesMaxHeight()
        {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='t' style='width:100px;min-height:200px;max-height:100px'></div></body>");
            Assert.True(LayoutTestHelper.FindById(r, "t")!.ContentRect.Height >= 199);
        }

        // border shorthand sets all sides
        [Fact]
        public void Border_AllSides()
        {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='t' style='border:3px solid red;width:100px;height:50px'></div></body>");
            var t = LayoutTestHelper.FindById(r, "t")!;
            Assert.Equal(3, t.BorderTopWidth);
            Assert.Equal(3, t.BorderRightWidth);
            Assert.Equal(3, t.BorderBottomWidth);
            Assert.Equal(3, t.BorderLeftWidth);
        }

        // border-style:none → width computed to 0
        [Fact]
        public void BorderNone_ZeroWidth()
        {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='t' style='border:5px none red;width:100px;height:50px'></div></body>");
            Assert.Equal(0, LayoutTestHelper.FindById(r, "t")!.BorderTopWidth);
        }

        // inset shorthand = top right bottom left
        [Fact]
        public void Inset_AllSides()
        {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='position:relative;width:200px;height:200px'><div id='t' style='position:absolute;inset:10px'></div></div></body>");
            var t = LayoutTestHelper.FindById(r, "t")!;
            Assert.True(System.Math.Abs(t.ContentRect.Width - 180) < 2);
            Assert.True(System.Math.Abs(t.ContentRect.Height - 180) < 2);
        }
    }
}
