using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    /// <summary>
    /// Tests for nested layout patterns: flex-in-grid, grid-in-flex,
    /// tables-in-flex, positioned elements in flex/grid, and deep nesting.
    /// </summary>
    public class WptNestedLayoutTests
    {
        private readonly ITestOutputHelper _output;
        public WptNestedLayoutTests(ITestOutputHelper output) { _output = output; }

        // [CSS-FLEXBOX + CSS-GRID] flex inside grid
        [Fact] public void Flex_Inside_Grid() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='display:grid;grid-template-columns:200px;width:200px'><div style='display:flex'><div id='a' style='width:60px;height:30px'></div><div id='b' style='width:60px;height:30px'></div></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"b")!.ContentRect.X > LayoutTestHelper.FindById(r,"a")!.ContentRect.X);
        }

        // [CSS-GRID + CSS-FLEXBOX] grid inside flex
        [Fact] public void Grid_Inside_Flex() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='display:flex;width:300px'><div style='display:grid;grid-template-columns:1fr 1fr;flex:1'><div id='a' style='height:20px'></div><div id='b' style='height:20px'></div></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"b")!.ContentRect.X > LayoutTestHelper.FindById(r,"a")!.ContentRect.X);
        }

        // [CSS-FLEXBOX] table inside flex item
        [Fact] public void Table_Inside_Flex() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='display:flex;width:300px'><table style='border-collapse:collapse'><tr><td id='t' style='height:30px'>A</td></tr></table></div></body>");
            Assert.NotNull(LayoutTestHelper.FindById(r,"t"));
        }

        // [CSS-FLEXBOX] flex inside flex (nested row)
        [Fact] public void Flex_Inside_Flex_Row() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='display:flex;width:300px'><div style='display:flex;flex:1'><div id='a' style='flex:1;height:30px'></div><div id='b' style='flex:1;height:30px'></div></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"b")!.ContentRect.X > LayoutTestHelper.FindById(r,"a")!.ContentRect.X);
        }

        // [CSS-GRID] grid inside grid
        [Fact] public void Grid_Inside_Grid() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='display:grid;grid-template-columns:200px;width:200px'><div style='display:grid;grid-template-columns:1fr 1fr'><div id='a' style='height:20px'></div><div id='b' style='height:20px'></div></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"b")!.ContentRect.X > LayoutTestHelper.FindById(r,"a")!.ContentRect.X);
        }

        // [CSS2 §9.3] abspos inside flex item
        [Fact] public void AbsPos_Inside_FlexItem() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='display:flex;width:200px'><div style='position:relative;width:100px;height:80px'><div id='abs' style='position:absolute;top:10px;left:10px;width:30px;height:30px'></div></div></div></body>");
            var abs = LayoutTestHelper.FindById(r,"abs")!;
            Assert.True(abs.ContentRect.X >= 9);
            Assert.True(abs.ContentRect.Y >= 9);
        }

        // [CSS2 §9.5] float inside flex item
        [Fact] public void Float_Inside_FlexItem() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='display:flex;width:200px'><div id='item' style='width:150px'><div style='float:left;width:50px;height:40px'></div></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"item")!.ContentRect.Height >= 39);
        }

        // [CSS2 §9.5] float inside grid item
        [Fact] public void Float_Inside_GridItem() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='display:grid;grid-template-columns:200px;width:200px'><div id='item'><div style='float:left;width:50px;height:40px'></div></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"item")!.ContentRect.Height >= 39);
        }

        // deep nesting: 5 levels of blocks
        [Fact] public void DeepNesting_Blocks() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='width:200px'><div><div><div><div id='t' style='height:20px'></div></div></div></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 200) < 2);
        }

        // deep nesting: flex > grid > flex
        [Fact] public void DeepNesting_FlexGridFlex() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='display:flex;width:300px'><div style='display:grid;grid-template-columns:1fr;flex:1'><div style='display:flex'><div id='t' style='flex:1;height:20px'></div></div></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width >= 299);
        }

        // [CSS-MULTICOL] multicol inside flex
        [Fact] public void Multicol_Inside_Flex() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='display:flex;width:300px'><div id='mc' style='column-count:2;flex:1'><div style='height:40px'></div><div style='height:40px'></div></div></div></body>");
            Assert.NotNull(LayoutTestHelper.FindById(r,"mc"));
        }

        // [CSS2 §9.2.1.1] mixed block and inline content
        [Fact] public void MixedContent_BlocksAndText() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='width:200px'><div id='a' style='height:20px'></div><div id='b' style='height:20px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"b")!.ContentRect.Y >= 19);
        }

        // [CSS-FLEXBOX §4] display:contents in nested flex
        [Fact] public void DisplayContents_NestedFlex() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='display:flex;width:200px'><div style='display:contents'><div style='display:flex;flex:1'><div id='t' style='flex:1;height:20px'></div></div></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width >= 199);
        }

        // overflow: hidden inside flex
        [Fact] public void OverflowHidden_Inside_Flex() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='display:flex;width:200px'><div id='t' style='overflow:hidden;width:100px;height:50px'><div style='width:300px;height:300px'></div></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height - 50) < 2);
        }

        // position: relative inside grid
        [Fact] public void Relative_Inside_Grid() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='display:grid;grid-template-columns:200px;width:200px'><div id='t' style='position:relative;top:20px;height:30px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y >= 19);
        }

        // flex item with border-box
        [Fact] public void FlexItem_BorderBox() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='display:flex;width:200px'><div id='t' style='box-sizing:border-box;width:100px;padding:10px;border:5px solid;height:50px'></div></div></body>");
            var t = LayoutTestHelper.FindById(r,"t")!;
            float totalW = t.ContentRect.Width + t.PaddingLeft + t.PaddingRight + t.BorderLeftWidth + t.BorderRightWidth;
            Assert.True(System.Math.Abs(totalW - 100) < 2);
        }

        // grid item with border-box
        [Fact] public void GridItem_BorderBox() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='display:grid;grid-template-columns:100px;width:100px'><div id='t' style='box-sizing:border-box;padding:10px;border:5px solid;height:50px'></div></div></body>");
            var t = LayoutTestHelper.FindById(r,"t")!;
            float totalW = t.ContentRect.Width + t.PaddingLeft + t.PaddingRight + t.BorderLeftWidth + t.BorderRightWidth;
            Assert.True(System.Math.Abs(totalW - 100) < 2);
        }
    }
}
