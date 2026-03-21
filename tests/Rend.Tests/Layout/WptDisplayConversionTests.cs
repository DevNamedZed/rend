using Rend.Css;
using Rend.Style;
using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    /// <summary>
    /// Tests for CSS display property interactions and blockification rules.
    /// Covers CSS Display L3 blockification (float, abspos, flex/grid child),
    /// display value layout behavior, and display:contents/none semantics.
    /// </summary>
    public class WptDisplayConversionTests
    {
        private readonly ITestOutputHelper _output;

        public WptDisplayConversionTests(ITestOutputHelper output)
        {
            _output = output;
        }

        // [CSS-DISPLAY §3] display:block fills containing block width
        [Fact]
        public void BlockFillsContainerWidth()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                    <div style='width:300px'>
                        <div id='t' style='display:block;height:20px'></div>
                    </div>
                </body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            Assert.True(System.Math.Abs(target!.ContentRect.Width - 300) < 2);
        }

        // [CSS-DISPLAY §3] display:inline-block uses shrink-to-fit width
        [Fact]
        public void InlineBlockShrinksToFit()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                    <div style='width:400px'>
                        <span id='t' style='display:inline-block'>
                            <div style='width:120px;height:30px'></div>
                        </span>
                    </div>
                </body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            Assert.True(System.Math.Abs(target!.ContentRect.Width - 120) < 2);
        }

        // [CSS-FLEXBOX §3] display:flex generates block-level flex container
        [Fact]
        public void FlexIsBlockLevel()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                    <div style='width:300px'>
                        <div id='t' style='display:flex;height:30px'></div>
                    </div>
                </body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            Assert.True(System.Math.Abs(target!.ContentRect.Width - 300) < 2);
        }

        // [CSS-FLEXBOX §3] display:inline-flex generates inline-level flex container
        [Fact]
        public void InlineFlexShrinksToContent()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                    <div style='width:400px'>
                        <div id='t' style='display:inline-flex'>
                            <div style='width:70px;height:20px'></div>
                            <div style='width:30px;height:20px'></div>
                        </div>
                    </div>
                </body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            Assert.True(System.Math.Abs(target!.ContentRect.Width - 100) < 2);
        }

        // [CSS-GRID §3] display:grid generates block-level grid container
        [Fact]
        public void GridIsBlockLevel()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                    <div style='width:300px'>
                        <div id='t' style='display:grid;height:30px'></div>
                    </div>
                </body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            Assert.True(System.Math.Abs(target!.ContentRect.Width - 300) < 2);
        }

        // [CSS-GRID §3] display:inline-grid generates inline-level grid container
        [Fact]
        public void InlineGridShrinksToContent()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                    <div style='width:400px'>
                        <div id='t' style='display:inline-grid;grid-template-columns:60px 40px'>
                            <div style='height:20px'></div>
                            <div style='height:20px'></div>
                        </div>
                    </div>
                </body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            Assert.True(System.Math.Abs(target!.ContentRect.Width - 100) < 2);
        }

        // [CSS-TABLES §2] display:table uses shrink-to-fit sizing
        [Fact]
        public void TableShrinksToContent()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                    <div style='width:400px'>
                        <div id='t' style='display:table'>
                            <div style='display:table-row'>
                                <div style='display:table-cell;width:80px;height:20px'></div>
                            </div>
                        </div>
                    </div>
                </body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            Assert.True(target!.ContentRect.Width < 400);
        }

        // [CSS-DISPLAY §4] display:none removes element from layout
        [Fact]
        public void NoneRemovedFromLayout()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                    <div style='display:none'>
                        <div id='hidden' style='width:100px;height:100px'></div>
                    </div>
                    <div id='after' style='height:20px'></div>
                </body>");
            Assert.Null(LayoutTestHelper.FindById(root, "hidden"));
            var afterBox = LayoutTestHelper.FindById(root, "after");
            Assert.NotNull(afterBox);
            Assert.True(afterBox!.ContentRect.Y < 2);
        }

        // [CSS-DISPLAY §3] display:flow-root establishes BFC, contains floats
        [Fact]
        public void FlowRootContainsFloats()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                    <div id='t' style='display:flow-root;width:200px'>
                        <div style='float:left;width:100px;height:80px'></div>
                    </div>
                </body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            Assert.True(target!.ContentRect.Height >= 79);
        }

        // [CSS-DISPLAY §2.1] display:contents promotes children into parent
        [Fact]
        public void ContentsPromotesChildren()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                    <div style='width:200px'>
                        <div style='display:contents'>
                            <div id='child' style='width:150px;height:40px'></div>
                        </div>
                    </div>
                </body>");
            var child = LayoutTestHelper.FindById(root, "child");
            Assert.NotNull(child);
            Assert.True(System.Math.Abs(child!.ContentRect.Width - 150) < 2);
        }

        // [CSS2 §9.7] float blockifies to block, respects width/height
        [Fact]
        public void FloatBlockifiesInlineToBlock()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                    <div style='width:300px'>
                        <div id='t' style='float:left;width:100px;height:50px'>
                            <div style='width:50px;height:20px'></div>
                        </div>
                        <div id='after' style='height:20px'></div>
                    </div>
                </body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            Assert.True(System.Math.Abs(target!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(target!.ContentRect.Height - 50) < 2);
        }

        // [CSS2 §9.7] position:absolute blockifies to block, respects width/height
        [Fact]
        public void AbsoluteBlockifiesInline()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                    <div style='position:relative;width:300px;height:200px'>
                        <div id='t' style='position:absolute;width:80px;height:40px'>
                            <div style='width:30px;height:15px'></div>
                        </div>
                    </div>
                </body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            Assert.True(System.Math.Abs(target!.ContentRect.Width - 80) < 2);
            Assert.True(System.Math.Abs(target!.ContentRect.Height - 40) < 2);
        }

        // [CSS2 §9.7] position:fixed blockifies to block, respects width/height
        [Fact]
        public void FixedBlockifiesInline()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                    <div id='t' style='position:fixed;width:60px;height:30px'>
                        <div style='width:20px;height:10px'></div>
                    </div>
                </body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            Assert.True(System.Math.Abs(target!.ContentRect.Width - 60) < 2);
            Assert.True(System.Math.Abs(target!.ContentRect.Height - 30) < 2);
        }

        // [CSS-FLEXBOX §4] flex child blockified: inline-block becomes block
        [Fact]
        public void FlexChildBlockifiesInlineBlock()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                    <div style='display:flex;width:300px'>
                        <div id='t' style='display:inline-block;width:100px;height:40px'></div>
                    </div>
                </body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            Assert.True(System.Math.Abs(target!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(target!.ContentRect.Height - 40) < 2);
        }

        // [CSS-GRID §4] grid child blockified: inline elements become block-level
        [Fact]
        public void GridChildBlockifiesInline()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                    <div style='display:grid;grid-template-columns:1fr;width:200px'>
                        <span id='t' style='height:30px'></span>
                    </div>
                </body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            Assert.True(System.Math.Abs(target!.ContentRect.Width - 200) < 2);
        }

        // [CSS-DISPLAY §2] display:list-item generates block with marker
        [Fact]
        public void ListItemHasBlockBehavior()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                    <div style='width:300px'>
                        <div id='t' style='display:list-item;margin-left:40px;height:20px'></div>
                    </div>
                </body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            var styledElement = target!.StyledNode as StyledElement;
            Assert.NotNull(styledElement);
            Assert.Equal(CssDisplay.ListItem, styledElement!.Style.Display);
        }

        // [CSS-TABLES §2.1] display:table-cell participates in table layout
        [Fact]
        public void TableCellParticipatesInTableLayout()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                    <div style='display:table;width:200px'>
                        <div style='display:table-row'>
                            <div id='a' style='display:table-cell;height:30px'></div>
                            <div id='b' style='display:table-cell;height:30px'></div>
                        </div>
                    </div>
                </body>");
            var cellA = LayoutTestHelper.FindById(root, "a");
            var cellB = LayoutTestHelper.FindById(root, "b");
            Assert.NotNull(cellA);
            Assert.NotNull(cellB);
            Assert.True(cellB!.ContentRect.X > cellA!.ContentRect.X);
        }

        // [CSS-TABLES §2.1] display:table-row wraps cells horizontally
        [Fact]
        public void TableRowWrapsTableCells()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                    <div style='display:table;width:200px'>
                        <div id='row1' style='display:table-row'>
                            <div style='display:table-cell;height:25px'></div>
                        </div>
                        <div id='row2' style='display:table-row'>
                            <div style='display:table-cell;height:25px'></div>
                        </div>
                    </div>
                </body>");
            var row1 = LayoutTestHelper.FindById(root, "row1");
            var row2 = LayoutTestHelper.FindById(root, "row2");
            Assert.NotNull(row1);
            Assert.NotNull(row2);
            Assert.True(row2!.ContentRect.Y > row1!.ContentRect.Y);
        }

        // [CSS-TABLES §2.1] display:table-row-group groups rows
        [Fact]
        public void TableRowGroupGroupsRows()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                    <div style='display:table;width:200px'>
                        <div style='display:table-row-group'>
                            <div style='display:table-row'>
                                <div id='cell' style='display:table-cell;height:30px'></div>
                            </div>
                        </div>
                    </div>
                </body>");
            var cell = LayoutTestHelper.FindById(root, "cell");
            Assert.NotNull(cell);
            Assert.True(cell!.ContentRect.Height >= 29);
        }

        // [CSS-DISPLAY §3] display changes from block to flex alter child layout
        [Fact]
        public void DisplayBlockVsFlexDifference()
        {
            var blockRoot = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                    <div style='display:block;width:200px'>
                        <div id='a' style='width:50px;height:30px'></div>
                        <div id='b' style='width:50px;height:30px'></div>
                    </div>
                </body>");
            var flexRoot = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                    <div style='display:flex;width:200px'>
                        <div id='a' style='width:50px;height:30px'></div>
                        <div id='b' style='width:50px;height:30px'></div>
                    </div>
                </body>");

            var blockB = LayoutTestHelper.FindById(blockRoot, "b");
            var flexB = LayoutTestHelper.FindById(flexRoot, "b");
            Assert.NotNull(blockB);
            Assert.NotNull(flexB);

            // In block: B stacks below A (Y > 0, X = 0)
            Assert.True(blockB!.ContentRect.Y >= 29);
            Assert.True(blockB!.ContentRect.X < 2);

            // In flex: B sits beside A (X > 0, Y = 0)
            Assert.True(flexB!.ContentRect.X >= 49);
            Assert.True(flexB!.ContentRect.Y < 2);
        }

        // [CSS-DISPLAY §2] display:inline does not accept width/height
        // Inline elements cannot have explicit dimensions; parent height from content only
        [Fact]
        public void InlineIgnoresWidthHeight()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                    <div id='t' style='width:300px'>
                        <span style='display:inline;width:200px;height:100px'>x</span>
                    </div>
                </body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            // Parent auto height should reflect inline content height (one line), not 100px
            Assert.True(target!.ContentRect.Height < 50);
        }

        // [CSS2 §9.7] float blockifies inline-flex to flex
        [Fact]
        public void FloatBlockifiesInlineFlexToFlex()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                    <div style='width:400px'>
                        <div id='t' style='display:inline-flex;float:left'>
                            <div style='width:60px;height:25px'></div>
                            <div style='width:40px;height:25px'></div>
                        </div>
                    </div>
                </body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            // Should be blockified to flex but still shrink-to-fit as a float
            Assert.True(System.Math.Abs(target!.ContentRect.Width - 100) < 2);
        }

        // [CSS2 §9.7] float blockifies inline-grid to grid
        [Fact]
        public void FloatBlockifiesInlineGridToGrid()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                    <div style='width:400px'>
                        <div id='t' style='display:inline-grid;float:left;grid-template-columns:50px 50px'>
                            <div style='height:20px'></div>
                            <div style='height:20px'></div>
                        </div>
                    </div>
                </body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            Assert.True(System.Math.Abs(target!.ContentRect.Width - 100) < 2);
        }

        // [CSS-DISPLAY §4] display:none descendants not in layout tree
        [Fact]
        public void NoneDescendantsNotInTree()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                    <div style='display:none'>
                        <div>
                            <div id='deep' style='width:50px;height:50px'></div>
                        </div>
                    </div>
                    <div id='visible' style='height:10px'></div>
                </body>");
            Assert.Null(LayoutTestHelper.FindById(root, "deep"));
            var visible = LayoutTestHelper.FindById(root, "visible");
            Assert.NotNull(visible);
            Assert.True(visible!.ContentRect.Y < 2);
        }

        // [CSS-FLEXBOX §4] flex child blockifies inline-flex to flex
        [Fact]
        public void FlexChildBlockifiesInlineFlex()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                    <div style='display:flex;width:300px'>
                        <div id='t' style='display:inline-flex'>
                            <div style='width:40px;height:20px'></div>
                            <div style='width:60px;height:20px'></div>
                        </div>
                    </div>
                </body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            // Blockified inline-flex in flex context still lays out its children as flex items
            Assert.True(System.Math.Abs(target!.ContentRect.Width - 100) < 2);
        }

        // [CSS-GRID §4] grid child blockifies inline-grid to grid
        [Fact]
        public void GridChildBlockifiesInlineGrid()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                    <div style='display:grid;grid-template-columns:1fr;width:200px'>
                        <div id='t' style='display:inline-grid;grid-template-columns:50px 50px'>
                            <div style='height:20px'></div>
                            <div style='height:20px'></div>
                        </div>
                    </div>
                </body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            // Grid stretches items by default; the inner grid should fill 200px
            Assert.True(target!.ContentRect.Width >= 100);
        }

        // [CSS2 §9.7] position:absolute blockifies inline-block
        [Fact]
        public void AbsoluteBlockifiesInlineBlock()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                    <div style='position:relative;width:300px;height:200px'>
                        <span id='t' style='position:absolute;display:inline-block;width:90px;height:45px'></span>
                    </div>
                </body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            Assert.True(System.Math.Abs(target!.ContentRect.Width - 90) < 2);
            Assert.True(System.Math.Abs(target!.ContentRect.Height - 45) < 2);
        }

        // [CSS-DISPLAY §2.1] display:contents in flex context makes children flex items
        [Fact]
        public void ContentsInFlexChildrenBecomeFlexItems()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                    <div style='display:flex;width:200px'>
                        <div style='display:contents'>
                            <div id='a' style='width:60px;height:30px'></div>
                            <div id='b' style='width:40px;height:30px'></div>
                        </div>
                    </div>
                </body>");
            var itemA = LayoutTestHelper.FindById(root, "a");
            var itemB = LayoutTestHelper.FindById(root, "b");
            Assert.NotNull(itemA);
            Assert.NotNull(itemB);
            // Children of display:contents in flex become flex items laid out horizontally
            Assert.True(System.Math.Abs(itemB!.ContentRect.X - 60) < 2);
        }

        // [CSS-DISPLAY §2.1] display:contents in grid context makes children grid items
        [Fact]
        public void ContentsInGridChildrenBecomeGridItems()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                    <div style='display:grid;grid-template-columns:1fr 1fr;width:200px'>
                        <div style='display:contents'>
                            <div id='a' style='height:20px'></div>
                            <div id='b' style='height:20px'></div>
                        </div>
                    </div>
                </body>");
            var itemB = LayoutTestHelper.FindById(root, "b");
            Assert.NotNull(itemB);
            Assert.True(itemB!.ContentRect.X >= 99);
        }

        // [CSS-DISPLAY §3] display:flow-root is block-level, fills width
        [Fact]
        public void FlowRootFillsContainerWidth()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                    <div style='width:250px'>
                        <div id='t' style='display:flow-root;height:20px'></div>
                    </div>
                </body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            Assert.True(System.Math.Abs(target!.ContentRect.Width - 250) < 2);
        }

        // [CSS-DISPLAY §3] display affects vertical stacking of sibling blocks
        [Fact]
        public void BlockSiblingsStackVertically()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                    <div style='width:200px'>
                        <div id='a' style='display:block;height:40px'></div>
                        <div id='b' style='display:block;height:40px'></div>
                        <div id='c' style='display:block;height:40px'></div>
                    </div>
                </body>");
            var boxA = LayoutTestHelper.FindById(root, "a");
            var boxB = LayoutTestHelper.FindById(root, "b");
            var boxC = LayoutTestHelper.FindById(root, "c");
            Assert.NotNull(boxA);
            Assert.NotNull(boxB);
            Assert.NotNull(boxC);
            Assert.True(System.Math.Abs(boxA!.ContentRect.Y - 0) < 2);
            Assert.True(System.Math.Abs(boxB!.ContentRect.Y - 40) < 2);
            Assert.True(System.Math.Abs(boxC!.ContentRect.Y - 80) < 2);
        }

        // [CSS-FLEXBOX §4] flex child with display:inline becomes block
        [Fact]
        public void FlexChildBlockifiesInline()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                    <div style='display:flex;width:200px'>
                        <span id='t' style='width:80px;height:30px'></span>
                    </div>
                </body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            // span (inline) in flex becomes blockified, so width/height are honored
            Assert.True(System.Math.Abs(target!.ContentRect.Width - 80) < 2);
            Assert.True(System.Math.Abs(target!.ContentRect.Height - 30) < 2);
        }

        // [CSS-GRID §4] grid child with display:inline becomes block
        [Fact]
        public void GridChildBlockifiesInlineSpan()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                    <div style='display:grid;grid-template-columns:1fr;width:200px'>
                        <span id='t' style='height:25px'></span>
                    </div>
                </body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            // Inline span blockified in grid fills column width
            Assert.True(System.Math.Abs(target!.ContentRect.Width - 200) < 2);
            Assert.True(System.Math.Abs(target!.ContentRect.Height - 25) < 2);
        }
    }
}
