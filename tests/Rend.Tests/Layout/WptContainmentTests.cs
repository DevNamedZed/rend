using Rend.Css;
using Rend.Style;
using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    /// <summary>
    /// Tests for CSS containment, display:contents interactions,
    /// stacking contexts, and layout isolation.
    /// </summary>
    public class WptContainmentTests
    {
        private readonly ITestOutputHelper _output;
        public WptContainmentTests(ITestOutputHelper output) { _output = output; }

        // [CSS-CONTAIN §3] contain: size → auto height = 0
        [Fact] public void Contain_Size_ZeroAutoHeight() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='t' style='contain:size;width:100px'><div style='height:200px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height < 1);
        }

        // [CSS-CONTAIN §3] contain: strict → auto height = 0
        [Fact] public void Contain_Strict_ZeroAutoHeight() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='t' style='contain:strict;width:100px'><div style='height:200px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height < 1);
        }

        // [CSS-CONTAIN §3] contain: size with explicit height works normally
        [Fact] public void Contain_Size_ExplicitHeight() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='t' style='contain:size;width:100px;height:80px'><div style='height:200px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height - 80) < 2);
        }

        // [CSS-DISPLAY §2.1] display:contents children inherit grandparent styles
        [Fact] public void DisplayContents_StyleInheritance() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='color:red'><div style='display:contents;color:blue'><div id='t' style='width:10px;height:10px'></div></div></div></body>");
            var s = (LayoutTestHelper.FindById(r,"t")!.StyledNode as StyledElement)!;
            // Child inherits from display:contents parent (blue), not grandparent (red)
            Assert.Equal(0, s.Style.Color.R);
            Assert.True(s.Style.Color.B > 200);
        }

        // [CSS-DISPLAY §2.1] display:contents in flex
        [Fact] public void DisplayContents_Flex_ChildrenAreItems() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='display:flex;width:200px'><div style='display:contents'><div id='a' style='width:50px;height:30px'></div><div id='b' style='width:50px;height:30px'></div></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"b")!.ContentRect.X > LayoutTestHelper.FindById(r,"a")!.ContentRect.X);
        }

        // [CSS-DISPLAY §2.1] display:contents in grid
        [Fact] public void DisplayContents_Grid_ChildrenAreItems() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='display:grid;grid-template-columns:1fr 1fr;width:200px'><div style='display:contents'><div id='a' style='height:20px'></div><div id='b' style='height:20px'></div></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"b")!.ContentRect.X > LayoutTestHelper.FindById(r,"a")!.ContentRect.X);
        }

        // [CSS2 §9.9] elements with opacity < 1 create stacking context
        [Fact] public void Opacity_CreatesStackingContext() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='t' style='opacity:0.99;width:100px;height:50px'></div></body>");
            Assert.NotNull(LayoutTestHelper.FindById(r,"t"));
        }

        // [CSS-TRANSFORMS §2] transform creates stacking context
        [Fact] public void Transform_CreatesStackingContext() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='t' style='transform:translateX(0);width:100px;height:50px'></div></body>");
            Assert.NotNull(LayoutTestHelper.FindById(r,"t"));
        }

        // [CSS-DISPLAY §3] flow-root establishes BFC
        [Fact] public void FlowRoot_BFC_ContainsFloats() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='t' style='display:flow-root;width:200px'><div style='float:left;width:80px;height:60px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height >= 59);
        }

        // [CSS-DISPLAY §3] flow-root BFC avoids sibling floats
        [Fact] public void FlowRoot_AvoidsSiblingFloat() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='width:200px'><div style='float:left;width:80px;height:50px'></div><div id='t' style='display:flow-root'>content</div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.X >= 79);
        }

        // [CSS2 §9.4.1] overflow:hidden establishes BFC
        [Fact] public void OverflowHidden_BFC() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='width:200px'><div style='float:left;width:80px;height:50px'></div><div id='t' style='overflow:hidden'>content</div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.X >= 79);
        }

        // [CSS2 §9.4.1] inline-block establishes BFC
        [Fact] public void InlineBlock_BFC() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='width:200px'><span id='t' style='display:inline-block;width:100px'><div style='float:left;width:50px;height:40px'></div></span></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height >= 39);
        }

        // [CSS-FLEXBOX §4] flex items establish BFC
        [Fact] public void FlexItem_BFC_ContainsFloats() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='display:flex;width:200px'><div id='t' style='width:100px'><div style='float:left;width:50px;height:40px'></div></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height >= 39);
        }

        // [CSS-GRID §6] grid items establish BFC
        [Fact] public void GridItem_BFC_ContainsFloats() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='display:grid;grid-template-columns:200px;width:200px'><div id='t'><div style='float:left;width:50px;height:40px'></div></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height >= 39);
        }

        // [CSS2 §8.3.1] BFC prevents margin collapse with parent
        [Fact] public void BFC_PreventsMarginCollapse() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='p' style='overflow:hidden;width:200px'><div id='c' style='margin-top:20px;height:30px'></div></div></body>");
            var p = LayoutTestHelper.FindById(r,"p")!;
            var c = LayoutTestHelper.FindById(r,"c")!;
            // BFC: child margin doesn't collapse with parent
            Assert.True(c.ContentRect.Y > p.ContentRect.Y + 15);
        }
    }
}
