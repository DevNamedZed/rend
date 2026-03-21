using Rend.Css;
using Rend.Style;
using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    /// <summary>
    /// Tests for CSS2 absolute and fixed positioning, containing blocks,
    /// percent resolution, auto margins, and over-constrained cases.
    /// </summary>
    public class WptAbsolutePositionTests
    {
        private readonly ITestOutputHelper _output;
        public WptAbsolutePositionTests(ITestOutputHelper output) { _output = output; }

        // [CSS2 §10.3.7] abspos auto width = shrink-to-fit
        [Fact] public void AbsPos_AutoWidth_ShrinkToFit() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='position:relative;width:300px;height:100px'><div id='t' style='position:absolute;top:0;left:0'><div style='width:80px;height:20px'></div></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width <= 81);
        }

        // [CSS2 §10.3.7] abspos left+right with auto width
        [Fact] public void AbsPos_LeftRight_AutoWidth() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='position:relative;width:300px;height:100px'><div id='t' style='position:absolute;left:20px;right:30px;height:50px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 250) < 2);
        }

        // [CSS2 §10.6.4] abspos top+bottom with auto height
        [Fact] public void AbsPos_TopBottom_AutoHeight() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='position:relative;width:200px;height:200px'><div id='t' style='position:absolute;top:20px;bottom:30px;width:50px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height - 150) < 2);
        }

        // [CSS2 §10.3.7] abspos with auto margins centers
        [Fact] public void AbsPos_AutoMargins_CenterHorizontal() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='position:relative;width:200px;height:100px'><div id='t' style='position:absolute;left:0;right:0;margin:0 auto;width:100px;height:50px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.X - 50) < 2);
        }

        // [CSS2 §10.6.4] abspos with auto margins centers vertically
        [Fact] public void AbsPos_AutoMargins_CenterVertical() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='position:relative;width:200px;height:200px'><div id='t' style='position:absolute;top:0;bottom:0;margin:auto 0;width:50px;height:100px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 50) < 2);
        }

        // [CSS2 §10.3.7] abspos with all 4 insets + auto margins = centered both
        [Fact] public void AbsPos_AllInsets_AutoMargins_Center() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='position:relative;width:200px;height:200px'><div id='t' style='position:absolute;top:0;right:0;bottom:0;left:0;margin:auto;width:100px;height:100px'></div></div></body>");
            var t = LayoutTestHelper.FindById(r,"t")!;
            Assert.True(System.Math.Abs(t.ContentRect.X - 50) < 2);
            Assert.True(System.Math.Abs(t.ContentRect.Y - 50) < 2);
        }

        // [CSS2 §10.5] abspos percentage height
        [Fact] public void AbsPos_PercentHeight() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='position:relative;width:200px'><div id='t' style='position:absolute;width:50px;height:50%'></div><div style='height:200px'></div></div></body>");
            var t = LayoutTestHelper.FindById(r,"t")!;
            Assert.True(System.Math.Abs(t.ContentRect.Height - 100) < 2);
        }

        // [CSS2 §10.3] abspos percentage width
        [Fact] public void AbsPos_PercentWidth() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='position:relative;width:300px;height:100px'><div id='t' style='position:absolute;width:50%;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 150) < 2);
        }

        // [CSS2 §9.3.2] relative positioning doesn't affect siblings
        [Fact] public void Relative_NoSiblingEffect() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='width:200px'><div style='position:relative;top:50px;left:50px;height:30px'></div><div id='t' style='height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 30) < 2);
        }

        // [CSS2 §9.3.2] relative positioning offset
        [Fact] public void Relative_Offset() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='width:200px'><div id='t' style='position:relative;top:20px;left:30px;height:30px'></div></div></body>");
            var t = LayoutTestHelper.FindById(r,"t")!;
            Assert.True(t.ContentRect.X >= 29);
            Assert.True(t.ContentRect.Y >= 19);
        }

        // [CSS2 §9.6.1] fixed position
        [Fact] public void Fixed_Position() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='t' style='position:fixed;top:10px;left:10px;width:50px;height:50px'></div></body>", 400, 300);
            var t = LayoutTestHelper.FindById(r,"t")!;
            Assert.True(System.Math.Abs(t.ContentRect.X - 10) < 2);
            Assert.True(System.Math.Abs(t.ContentRect.Y - 10) < 2);
        }

        // [CSS2 §9.7] z-index creates stacking context
        [Fact] public void ZIndex_Parsed() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='t' style='position:relative;z-index:5;width:50px;height:50px'></div></body>");
            Assert.Equal(5, ((LayoutTestHelper.FindById(r,"t")!.StyledNode as StyledElement)!).Style.ZIndex);
        }

        // [CSS2 §9.7] negative z-index
        [Fact] public void ZIndex_Negative() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='t' style='position:relative;z-index:-3;width:50px;height:50px'></div></body>");
            Assert.Equal(-3, ((LayoutTestHelper.FindById(r,"t")!.StyledNode as StyledElement)!).Style.ZIndex);
        }

        // [CSS2 §10.6.4] abspos with display:table
        [Fact] public void AbsPos_Table_PercentHeight() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='position:relative;width:100px'><div id='t' style='position:absolute;display:table;width:100%;height:100%'></div><div style='height:100px'></div></div></body>");
            var t = LayoutTestHelper.FindById(r,"t")!;
            Assert.True(t.ContentRect.Width >= 99);
            Assert.True(t.ContentRect.Height >= 99);
        }
    }
}
