using Rend.Css;
using Rend.Style;
using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    /// <summary>
    /// Tests for inline formatting context, inline-block, vertical-align,
    /// line-height, and inline element sizing.
    /// </summary>
    public class WptInlineLayoutTests
    {
        private readonly ITestOutputHelper _output;
        public WptInlineLayoutTests(ITestOutputHelper output) { _output = output; }

        // [CSS2 §9.2.2] inline-block generates block box with inline outer
        [Fact] public void InlineBlock_HasBlockBox() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='width:300px'><span id='t' style='display:inline-block;width:80px;height:40px'></span></div></body>");
            var t = LayoutTestHelper.FindById(r,"t")!;
            Assert.True(System.Math.Abs(t.ContentRect.Width - 80) < 2);
            Assert.True(System.Math.Abs(t.ContentRect.Height - 40) < 2);
        }

        // [CSS2 §9.2.2] inline-block shrink-to-fit
        [Fact] public void InlineBlock_ShrinkToFit() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='width:300px'><span id='t' style='display:inline-block'><div style='width:60px;height:20px'></div></span></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 60) < 2);
        }

        // [CSS2 §10.8] inline-block baseline
        [Fact] public void InlineBlock_Baseline() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='width:300px'><span id='t' style='display:inline-block;width:80px;height:40px;vertical-align:baseline'></span></div></body>");
            Assert.NotNull(LayoutTestHelper.FindById(r,"t"));
        }

        // [CSS2 §10.8] vertical-align: middle
        [Fact] public void VerticalAlign_Middle() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='width:300px;line-height:60px'><span id='t' style='display:inline-block;width:20px;height:20px;vertical-align:middle'></span></div></body>");
            var t = LayoutTestHelper.FindById(r,"t");
            Assert.NotNull(t);
            _output.WriteLine($"t.Y={t!.ContentRect.Y}");
        }

        // [CSS2 §10.8] vertical-align: top
        [Fact] public void VerticalAlign_Top() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='width:300px;line-height:60px'><span id='t' style='display:inline-block;width:20px;height:20px;vertical-align:top'></span></div></body>");
            Assert.NotNull(LayoutTestHelper.FindById(r,"t"));
        }

        // [CSS2 §10.8] vertical-align: bottom
        [Fact] public void VerticalAlign_Bottom() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='width:300px;line-height:60px'><span id='t' style='display:inline-block;width:20px;height:20px;vertical-align:bottom'></span></div></body>");
            Assert.NotNull(LayoutTestHelper.FindById(r,"t"));
        }

        // [CSS2 §10.8.1] line-height: normal
        [Fact] public void LineHeight_Normal() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='t' style='line-height:normal;width:200px'>x</div></body>");
            var lh = ((LayoutTestHelper.FindById(r,"t")!.StyledNode as StyledElement)!).Style.LineHeight;
            Assert.True(float.IsNaN(lh)); // normal stored as NaN
        }

        // [CSS2 §9.2.1.1] anonymous block creation
        [Fact] public void AnonymousBlock_MixedContent() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='width:200px'><div id='block' style='height:20px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"block")!.ContentRect.Height >= 19);
        }

        // [CSS2 §10.6] block-level elements stack vertically
        [Fact] public void Block_StacksVertically() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='width:200px'><div id='a' style='height:30px'></div><div id='b' style='height:30px'></div><div id='c' style='height:30px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"b")!.ContentRect.Y >= 29);
            Assert.True(LayoutTestHelper.FindById(r,"c")!.ContentRect.Y >= 59);
        }

        // [CSS2 §10.3.3] block fills available width
        [Fact] public void Block_FillsWidth() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='width:250px'><div id='t' style='height:10px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 250) < 2);
        }

        // [CSS-DISPLAY §3] inline-grid
        [Fact] public void InlineGrid_ShrinkToFit() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='width:400px'><div id='t' style='display:inline-grid;grid-template-columns:50px 50px'><div style='height:20px'></div><div style='height:20px'></div></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 100) < 2);
        }

        // [CSS-DISPLAY §3] inline-flex
        [Fact] public void InlineFlex_ShrinkToFit() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='width:400px'><div id='t' style='display:inline-flex'><div style='width:50px;height:20px'></div><div style='width:50px;height:20px'></div></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 100) < 2);
        }
    }
}
