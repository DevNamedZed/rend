using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    /// <summary>
    /// Tests for min/max width and height interactions, aspect-ratio
    /// with constraints, and box-sizing interactions with min/max.
    /// </summary>
    public class WptMinMaxSizingTests
    {
        private readonly ITestOutputHelper _output;
        public WptMinMaxSizingTests(ITestOutputHelper output) { _output = output; }

        // [CSS2 §10.4] min-width overrides max-width when min > max
        [Fact] public void MinWidth_OverridesMaxWidth() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='width:400px'><div id='t' style='min-width:200px;max-width:100px;height:20px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width >= 199);
        }

        // [CSS2 §10.7] min-height overrides max-height when min > max
        [Fact] public void MinHeight_OverridesMaxHeight() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='t' style='width:100px;min-height:200px;max-height:100px'></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height >= 199);
        }

        // [CSS2 §10.4] max-width clamps width
        [Fact] public void MaxWidth_Clamps_Width() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='width:400px'><div id='t' style='width:300px;max-width:150px;height:20px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 150) < 2);
        }

        // [CSS2 §10.7] max-height clamps height
        [Fact] public void MaxHeight_Clamps_Height() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='t' style='width:100px;height:300px;max-height:100px'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height - 100) < 2);
        }

        // [CSS2 §10.4] min-width expands
        [Fact] public void MinWidth_Expands_Width() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='width:400px'><div id='t' style='width:50px;min-width:150px;height:20px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 150) < 2);
        }

        // [CSS2 §10.7] min-height expands
        [Fact] public void MinHeight_Expands_Height() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='t' style='width:100px;height:50px;min-height:150px'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height - 150) < 2);
        }

        // [CSS2 §10.4] max-width: none = no constraint
        [Fact] public void MaxWidth_None() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='width:400px'><div id='t' style='max-width:none;height:20px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 400) < 2);
        }

        // [CSS-UI §3.2] box-sizing with min/max
        [Fact] public void BoxSizing_BorderBox_WithMinWidth() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='t' style='box-sizing:border-box;min-width:100px;padding:10px;border:5px solid;height:50px'></div></body>");
            var t = LayoutTestHelper.FindById(r,"t")!;
            float totalW = t.ContentRect.Width + t.PaddingLeft + t.PaddingRight + t.BorderLeftWidth + t.BorderRightWidth;
            Assert.True(totalW >= 99);
        }

        [Fact] public void BoxSizing_BorderBox_WithMaxWidth() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='width:400px'><div id='t' style='box-sizing:border-box;max-width:100px;padding:10px;border:5px solid;height:50px'></div></div></body>");
            var t = LayoutTestHelper.FindById(r,"t")!;
            float totalW = t.ContentRect.Width + t.PaddingLeft + t.PaddingRight + t.BorderLeftWidth + t.BorderRightWidth;
            Assert.True(totalW <= 101);
        }

        // [CSS2 §10.4] percentage min/max-width
        [Fact] public void MinWidth_Percent() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='width:400px'><div id='t' style='width:50px;min-width:50%;height:20px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 200) < 2);
        }

        [Fact] public void MaxWidth_Percent() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='width:400px'><div id='t' style='max-width:25%;height:20px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 100) < 2);
        }

        // [CSS2 §10.7] percentage min/max-height with definite parent
        [Fact] public void MinHeight_Percent() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='width:100px;height:200px'><div id='t' style='min-height:50%'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height >= 99);
        }

        [Fact] public void MaxHeight_Percent() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='width:100px;height:200px'><div id='t' style='height:300px;max-height:50%'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height - 100) < 2);
        }

        // [CSS-SIZING §5.1] aspect-ratio with min-width
        [Fact] public void AspectRatio_WithMinWidth() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='t' style='height:50px;aspect-ratio:1/1;min-width:100px'></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width >= 99);
        }

        // [CSS-SIZING §5.1] aspect-ratio with max-width
        [Fact] public void AspectRatio_WithMaxWidth() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='t' style='height:200px;aspect-ratio:2/1;max-width:100px'></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width <= 101);
        }

        // [CSS-FLEXBOX §7] flex item min/max
        [Fact] public void FlexItem_MinWidth() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='display:flex;width:200px'><div id='t' style='flex:1;min-width:150px;height:30px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width >= 149);
        }

        [Fact] public void FlexItem_MaxWidth() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='display:flex;width:200px'><div id='t' style='flex:1;max-width:80px;height:30px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width <= 81);
        }

        [Fact] public void FlexItem_MinHeight_Column() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='display:flex;flex-direction:column;height:200px;width:100px'><div id='t' style='flex:1;min-height:150px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height >= 149);
        }

        [Fact] public void FlexItem_MaxHeight_Column() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='display:flex;flex-direction:column;height:200px;width:100px'><div id='t' style='flex:1;max-height:80px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height <= 81);
        }

        // [CSS2 §10.4] auto width respects min-width
        [Fact] public void AutoWidth_RespectsMinWidth() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='width:400px'><div id='t' style='min-width:500px;height:20px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width >= 499);
        }

        // [CSS2 §10.4] auto width respects max-width
        [Fact] public void AutoWidth_RespectsMaxWidth() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='width:400px'><div id='t' style='max-width:200px;height:20px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width <= 201);
        }

        // min-height on auto-height block
        [Fact] public void MinHeight_OnAutoHeight() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='t' style='width:100px;min-height:80px'></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height >= 79);
        }

        // max-height on auto-height block with tall content
        [Fact] public void MaxHeight_OnAutoHeight_WithContent() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='t' style='width:100px;max-height:50px'><div style='height:200px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height <= 51);
        }
    }
}
