using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    /// <summary>
    /// Tests for CSS Block Formatting Context (BFC) establishment and behavior.
    /// Covers CSS 2.1 §9.4.1: which elements establish new BFCs, float containment,
    /// margin collapse prevention, and BFC interaction with floats.
    /// </summary>
    public class WptBlockFormattingContextTests
    {
        private readonly ITestOutputHelper _output;

        public WptBlockFormattingContextTests(ITestOutputHelper output)
        {
            _output = output;
        }

        // [CSS2 §9.4.1] overflow:hidden establishes BFC and contains floats
        [Fact]
        public void OverflowHidden_CreatesBfc_ContainsFloats()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='bfc' style='overflow:hidden;width:200px'>
                    <div style='float:left;width:80px;height:100px'></div>
                </div></body>");
            var bfc = LayoutTestHelper.FindById(root, "bfc")!;
            _output.WriteLine($"bfc height={bfc.ContentRect.Height}");
            Assert.True(bfc.ContentRect.Height >= 99,
                $"overflow:hidden BFC should contain float (height={bfc.ContentRect.Height})");
        }

        // [CSS2 §9.4.1] overflow:auto establishes BFC and contains floats
        [Fact]
        public void OverflowAuto_CreatesBfc_ContainsFloats()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='bfc' style='overflow:auto;width:200px'>
                    <div style='float:left;width:80px;height:100px'></div>
                </div></body>");
            var bfc = LayoutTestHelper.FindById(root, "bfc")!;
            _output.WriteLine($"bfc height={bfc.ContentRect.Height}");
            Assert.True(bfc.ContentRect.Height >= 99,
                $"overflow:auto BFC should contain float (height={bfc.ContentRect.Height})");
        }

        // [CSS2 §9.4.1] overflow:scroll establishes BFC and contains floats
        [Fact]
        public void OverflowScroll_CreatesBfc_ContainsFloats()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='bfc' style='overflow:scroll;width:200px'>
                    <div style='float:left;width:80px;height:100px'></div>
                </div></body>");
            var bfc = LayoutTestHelper.FindById(root, "bfc")!;
            _output.WriteLine($"bfc height={bfc.ContentRect.Height}");
            Assert.True(bfc.ContentRect.Height >= 99,
                $"overflow:scroll BFC should contain float (height={bfc.ContentRect.Height})");
        }

        // [CSS-DISPLAY §3] display:flow-root establishes BFC and contains floats
        [Fact]
        public void DisplayFlowRoot_CreatesBfc_ContainsFloats()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='bfc' style='display:flow-root;width:200px'>
                    <div style='float:left;width:80px;height:100px'></div>
                </div></body>");
            var bfc = LayoutTestHelper.FindById(root, "bfc")!;
            _output.WriteLine($"bfc height={bfc.ContentRect.Height}");
            Assert.True(bfc.ContentRect.Height >= 99,
                $"display:flow-root BFC should contain float (height={bfc.ContentRect.Height})");
        }

        // [CSS-FLEXBOX §3] display:flex establishes BFC for its contents
        [Fact]
        public void DisplayFlex_CreatesBfc_ContainsFloats()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='bfc' style='display:flex;width:200px'>
                    <div style='float:left;width:80px;height:100px'></div>
                </div></body>");
            var bfc = LayoutTestHelper.FindById(root, "bfc")!;
            _output.WriteLine($"bfc height={bfc.ContentRect.Height}");
            Assert.True(bfc.ContentRect.Height >= 99,
                $"display:flex BFC should contain float (height={bfc.ContentRect.Height})");
        }

        // [CSS-GRID §3] display:grid establishes BFC for its contents
        [Fact]
        public void DisplayGrid_CreatesBfc_ContainsFloats()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='bfc' style='display:grid;width:200px'>
                    <div style='float:left;width:80px;height:100px'></div>
                </div></body>");
            var bfc = LayoutTestHelper.FindById(root, "bfc")!;
            _output.WriteLine($"bfc height={bfc.ContentRect.Height}");
            Assert.True(bfc.ContentRect.Height >= 99,
                $"display:grid BFC should contain float (height={bfc.ContentRect.Height})");
        }

        // [CSS2 §9.4.1] display:inline-block establishes BFC and contains floats
        [Fact]
        public void DisplayInlineBlock_CreatesBfc_ContainsFloats()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:300px'>
                    <span id='bfc' style='display:inline-block;width:200px'>
                        <div style='float:left;width:80px;height:100px'></div>
                    </span>
                </div></body>");
            var bfc = LayoutTestHelper.FindById(root, "bfc")!;
            _output.WriteLine($"bfc height={bfc.ContentRect.Height}");
            Assert.True(bfc.ContentRect.Height >= 99,
                $"display:inline-block BFC should contain float (height={bfc.ContentRect.Height})");
        }

        // [CSS2 §9.4.1] float element establishes BFC and contains child floats
        [Fact]
        public void Float_CreatesBfc_ContainsChildFloats()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:300px'>
                    <div id='bfc' style='float:left;width:200px'>
                        <div style='float:left;width:80px;height:100px'></div>
                    </div>
                </div></body>");
            var bfc = LayoutTestHelper.FindById(root, "bfc")!;
            _output.WriteLine($"bfc height={bfc.ContentRect.Height}");
            Assert.True(bfc.ContentRect.Height >= 99,
                $"float BFC should contain child float (height={bfc.ContentRect.Height})");
        }

        // [CSS2 §9.4.1] position:absolute establishes BFC
        [Fact]
        public void PositionAbsolute_CreatesBfc_PreventsMarginCollapse()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:200px;height:200px'>
                    <div id='bfc' style='position:absolute;top:0;left:0;width:200px'>
                        <div id='child' style='margin-top:30px;height:20px'></div>
                    </div>
                </div></body>");
            var bfc = LayoutTestHelper.FindById(root, "bfc")!;
            var child = LayoutTestHelper.FindById(root, "child")!;
            float gap = child.ContentRect.Y - bfc.ContentRect.Y;
            _output.WriteLine($"gap={gap}");
            Assert.True(gap >= 29,
                $"position:absolute BFC prevents margin collapse with first child (gap={gap})");
        }

        // [CSS2 §9.4.1] position:fixed establishes BFC
        [Fact]
        public void PositionFixed_CreatesBfc_PreventsMarginCollapse()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='bfc' style='position:fixed;top:0;left:0;width:200px'>
                    <div id='child' style='margin-top:30px;height:20px'></div>
                </div></body>");
            var bfc = LayoutTestHelper.FindById(root, "bfc")!;
            var child = LayoutTestHelper.FindById(root, "child")!;
            float gap = child.ContentRect.Y - bfc.ContentRect.Y;
            _output.WriteLine($"gap={gap}");
            Assert.True(gap >= 29,
                $"position:fixed BFC prevents margin collapse with first child (gap={gap})");
        }

        // [CSS2 §9.4.1] BFC prevents margin collapse between parent and first child
        [Fact]
        public void Bfc_PreventsMarginCollapseWithFirstChild()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='bfc' style='overflow:hidden;width:200px;margin-top:10px'>
                    <div id='child' style='margin-top:30px;height:20px'></div>
                </div></body>");
            var bfc = LayoutTestHelper.FindById(root, "bfc")!;
            var child = LayoutTestHelper.FindById(root, "child")!;
            float childInsideParent = child.ContentRect.Y - bfc.ContentRect.Y;
            _output.WriteLine($"child inside parent gap={childInsideParent}");
            Assert.True(childInsideParent >= 29,
                $"BFC should keep child margin inside (gap={childInsideParent})");
        }

        // [CSS2 §9.4.1] BFC prevents margin collapse between parent and last child
        [Fact]
        public void Bfc_PreventsMarginCollapseWithLastChild()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='bfc' style='overflow:hidden;width:200px'>
                    <div style='height:20px;margin-bottom:30px'></div>
                </div></body>");
            var bfc = LayoutTestHelper.FindById(root, "bfc")!;
            _output.WriteLine($"bfc height={bfc.ContentRect.Height}");
            Assert.True(bfc.ContentRect.Height >= 49,
                $"BFC should include last child margin-bottom in height (height={bfc.ContentRect.Height})");
        }

        // [CSS2 §9.4.1] Non-BFC allows margin collapse with first child
        [Fact]
        public void NonBfc_AllowsMarginCollapseWithFirstChild()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='parent' style='width:200px;margin-top:10px'>
                    <div id='child' style='margin-top:30px;height:20px'></div>
                </div></body>");
            var parent = LayoutTestHelper.FindById(root, "parent")!;
            var child = LayoutTestHelper.FindById(root, "child")!;
            _output.WriteLine($"parent.Y={parent.ContentRect.Y} child.Y={child.ContentRect.Y}");
            Assert.True(System.Math.Abs(parent.ContentRect.Y - child.ContentRect.Y) < 2,
                $"Non-BFC parent and first child margins should collapse (parent.Y={parent.ContentRect.Y}, child.Y={child.ContentRect.Y})");
        }

        // [CSS2 §10.6.7] BFC parent height includes floats
        [Fact]
        public void Bfc_ParentHeightIncludesFloats()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='bfc' style='overflow:hidden;width:200px'>
                    <div style='float:left;width:60px;height:80px'></div>
                    <div style='float:right;width:60px;height:120px'></div>
                    <div style='height:30px'></div>
                </div></body>");
            var bfc = LayoutTestHelper.FindById(root, "bfc")!;
            _output.WriteLine($"bfc height={bfc.ContentRect.Height}");
            Assert.True(bfc.ContentRect.Height >= 119,
                $"BFC height should include tallest float (height={bfc.ContentRect.Height})");
        }

        // [CSS2 §9.5] BFC next to float: BFC placed beside sibling float
        [Fact]
        public void Bfc_PlacedBesideSiblingFloat()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:300px'>
                    <div style='float:left;width:100px;height:60px'></div>
                    <div id='bfc' style='overflow:hidden;height:40px'></div>
                </div></body>");
            var bfc = LayoutTestHelper.FindById(root, "bfc")!;
            _output.WriteLine($"bfc X={bfc.ContentRect.X}, width={bfc.ContentRect.Width}");
            Assert.True(bfc.ContentRect.X >= 99,
                $"BFC should be placed beside float (X={bfc.ContentRect.X})");
        }

        // [CSS2 §9.5] Multiple BFCs next to float stack vertically when no room
        [Fact]
        public void MultipleBfcs_NextToFloat()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:200px'>
                    <div style='float:left;width:100px;height:100px'></div>
                    <div id='bfc1' style='overflow:hidden;width:150px;height:30px'></div>
                    <div id='bfc2' style='overflow:hidden;width:150px;height:30px'></div>
                </div></body>");
            var bfc1 = LayoutTestHelper.FindById(root, "bfc1")!;
            var bfc2 = LayoutTestHelper.FindById(root, "bfc2")!;
            _output.WriteLine($"bfc1 Y={bfc1.ContentRect.Y}, bfc2 Y={bfc2.ContentRect.Y}");
            Assert.True(bfc2.ContentRect.Y > bfc1.ContentRect.Y,
                $"Second BFC should be below first (bfc1.Y={bfc1.ContentRect.Y}, bfc2.Y={bfc2.ContentRect.Y})");
        }

        // [CSS2 §9.4.1] BFC with padding still contains floats
        [Fact]
        public void Bfc_WithPadding_ContainsFloats()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='bfc' style='overflow:hidden;width:200px;padding:20px'>
                    <div style='float:left;width:60px;height:80px'></div>
                </div></body>");
            var bfc = LayoutTestHelper.FindById(root, "bfc")!;
            float totalHeight = bfc.ContentRect.Height + 40; // content + top/bottom padding
            _output.WriteLine($"bfc content height={bfc.ContentRect.Height}, total={totalHeight}");
            Assert.True(bfc.ContentRect.Height >= 79,
                $"BFC with padding should contain float (content height={bfc.ContentRect.Height})");
        }

        // [CSS2 §9.4.1] BFC with border still contains floats
        [Fact]
        public void Bfc_WithBorder_ContainsFloats()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='bfc' style='overflow:hidden;width:200px;border:5px solid black'>
                    <div style='float:left;width:60px;height:80px'></div>
                </div></body>");
            var bfc = LayoutTestHelper.FindById(root, "bfc")!;
            _output.WriteLine($"bfc content height={bfc.ContentRect.Height}");
            Assert.True(bfc.ContentRect.Height >= 79,
                $"BFC with border should contain float (content height={bfc.ContentRect.Height})");
        }

        // [CSS-MULTICOL §3] column-count establishes BFC
        [Fact]
        public void ColumnCount_CreatesBfc_ContainsFloats()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='bfc' style='column-count:2;width:200px'>
                    <div style='float:left;width:60px;height:80px'></div>
                </div></body>");
            var bfc = LayoutTestHelper.FindById(root, "bfc")!;
            _output.WriteLine($"bfc height={bfc.ContentRect.Height}");
            // [CSS-MULTICOL §2/§3] A multicol container is a BFC AND a fragmentation context, so
            // it CONTAINS its float — but the float is FRAGMENTED across the balanced columns,
            // not kept at its full un-fragmented height. An 80px float in 2 columns balances to
            // 40px per column => container height = 40. Verified via Chrome layout dump
            // (height must be contained/non-zero, but it is 40, NOT the full float height 80).
            Assert.True(bfc.ContentRect.Height > 1 && System.Math.Abs(bfc.ContentRect.Height - 40) < 2,
                $"multicol BFC fragments the float across columns => height 40 (got {bfc.ContentRect.Height})");
        }

        // [CSS2 §17.5.4] table-cell establishes BFC
        [Fact]
        public void TableCell_CreatesBfc_ContainsFloats()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:table;width:200px'>
                    <div style='display:table-row'>
                        <div id='cell' style='display:table-cell'>
                            <div style='float:left;width:60px;height:80px'></div>
                        </div>
                    </div>
                </div></body>");
            var cell = LayoutTestHelper.FindById(root, "cell")!;
            _output.WriteLine($"cell height={cell.ContentRect.Height}");
            Assert.True(cell.ContentRect.Height >= 79,
                $"table-cell BFC should contain float (height={cell.ContentRect.Height})");
        }

        // [CSS-FLEXBOX §4] Flex items establish independent BFC
        [Fact]
        public void FlexItem_CreatesBfc_NoMarginCollapseWithChildren()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:200px'>
                    <div id='item' style='width:200px'>
                        <div id='child' style='margin-top:30px;height:20px'></div>
                    </div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            var child = LayoutTestHelper.FindById(root, "child")!;
            float gap = child.ContentRect.Y - item.ContentRect.Y;
            _output.WriteLine($"flex item→child gap={gap}");
            Assert.True(gap >= 29,
                $"Flex item BFC should prevent margin collapse (gap={gap})");
        }

        // [CSS-GRID §6] Grid items establish independent BFC
        [Fact]
        public void GridItem_CreatesBfc_NoMarginCollapseWithChildren()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:200px;width:200px'>
                    <div id='item'>
                        <div id='child' style='margin-top:30px;height:20px'></div>
                    </div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            var child = LayoutTestHelper.FindById(root, "child")!;
            float gap = child.ContentRect.Y - item.ContentRect.Y;
            _output.WriteLine($"grid item→child gap={gap}");
            Assert.True(gap >= 29,
                $"Grid item BFC should prevent margin collapse (gap={gap})");
        }

        // [CSS2 §9.4.1] Sibling margins still collapse inside a BFC
        [Fact]
        public void Bfc_SiblingMarginsStillCollapse()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='overflow:hidden;width:200px'>
                    <div id='first' style='margin-bottom:30px;height:20px'></div>
                    <div id='second' style='margin-top:20px;height:20px'></div>
                </div></body>");
            var first = LayoutTestHelper.FindById(root, "first")!;
            var second = LayoutTestHelper.FindById(root, "second")!;
            float gap = second.ContentRect.Y - (first.ContentRect.Y + 20);
            _output.WriteLine($"sibling gap={gap}");
            Assert.True(System.Math.Abs(gap - 30) < 2,
                $"Sibling margins should collapse to max(30,20)=30 inside BFC (gap={gap})");
        }

        // [CSS2 §9.5] flow-root BFC avoids sibling float
        [Fact]
        public void FlowRoot_AvoidsSiblingFloat()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:300px'>
                    <div style='float:left;width:100px;height:60px'></div>
                    <div id='bfc' style='display:flow-root;height:40px'></div>
                </div></body>");
            var bfc = LayoutTestHelper.FindById(root, "bfc")!;
            _output.WriteLine($"bfc X={bfc.ContentRect.X}");
            Assert.True(bfc.ContentRect.X >= 99,
                $"flow-root BFC should avoid sibling float (X={bfc.ContentRect.X})");
        }

        // [CSS2 §9.5] BFC drops below float when it cannot fit beside it
        [Fact]
        public void Bfc_DropsBelow_WhenCannotFitBesideFloat()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:200px'>
                    <div style='float:left;width:150px;height:60px'></div>
                    <div id='bfc' style='overflow:hidden;width:150px;height:40px'></div>
                </div></body>");
            var bfc = LayoutTestHelper.FindById(root, "bfc")!;
            _output.WriteLine($"bfc Y={bfc.ContentRect.Y}");
            Assert.True(bfc.ContentRect.Y >= 59,
                $"BFC should drop below float when too wide to fit (Y={bfc.ContentRect.Y})");
        }

        // [CSS2 §9.4.1] BFC with both floats and block children
        [Fact]
        public void Bfc_FloatsAndBlockChildren_CorrectHeight()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='bfc' style='overflow:hidden;width:300px'>
                    <div style='float:left;width:80px;height:150px'></div>
                    <div style='height:40px'></div>
                    <div style='height:40px'></div>
                </div></body>");
            var bfc = LayoutTestHelper.FindById(root, "bfc")!;
            _output.WriteLine($"bfc height={bfc.ContentRect.Height}");
            Assert.True(bfc.ContentRect.Height >= 149,
                $"BFC height should encompass float (height={bfc.ContentRect.Height})");
        }

        // [CSS2 §9.4.1] Nested BFCs: inner BFC contains its own floats independently
        [Fact]
        public void NestedBfcs_InnerContainsOwnFloats()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='overflow:hidden;width:300px'>
                    <div id='inner' style='overflow:hidden;width:200px'>
                        <div style='float:left;width:60px;height:80px'></div>
                    </div>
                    <div id='after' style='height:20px'></div>
                </div></body>");
            var inner = LayoutTestHelper.FindById(root, "inner")!;
            var after = LayoutTestHelper.FindById(root, "after")!;
            _output.WriteLine($"inner height={inner.ContentRect.Height}, after Y={after.ContentRect.Y}");
            Assert.True(inner.ContentRect.Height >= 79,
                $"Inner BFC should contain its float (height={inner.ContentRect.Height})");
            Assert.True(after.ContentRect.Y >= 79,
                $"Element after inner BFC should be below (Y={after.ContentRect.Y})");
        }

        // [CSS-FLEXBOX §4] Flex container margins do not collapse through items
        [Fact]
        public void FlexContainer_Bfc_NoMarginCollapseBetweenItems()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;width:200px'>
                    <div id='itemA' style='margin-bottom:30px;height:20px'></div>
                    <div id='itemB' style='margin-top:20px;height:20px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "itemA")!;
            var itemB = LayoutTestHelper.FindById(root, "itemB")!;
            float gap = itemB.ContentRect.Y - (itemA.ContentRect.Y + 20);
            _output.WriteLine($"flex item gap={gap}");
            Assert.True(System.Math.Abs(gap - 50) < 2,
                $"Flex items should not collapse margins (gap should be 50, got {gap})");
        }

        // [CSS2 §9.4.1] display:inline-block BFC avoids parent floats
        [Fact]
        public void InlineBlock_Bfc_ContainsChildFloatHeight()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:300px'>
                    <span id='ib' style='display:inline-block;width:200px'>
                        <div style='float:left;width:60px;height:70px'></div>
                        <div style='height:30px'></div>
                    </span>
                </div></body>");
            var inlineBlock = LayoutTestHelper.FindById(root, "ib")!;
            _output.WriteLine($"inline-block height={inlineBlock.ContentRect.Height}");
            Assert.True(inlineBlock.ContentRect.Height >= 69,
                $"inline-block BFC should contain float (height={inlineBlock.ContentRect.Height})");
        }

        // [CSS2 §9.4.1] Float element prevents child margin collapse
        [Fact]
        public void Float_CreatesBfc_PreventsChildMarginCollapse()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:300px'>
                    <div id='floated' style='float:left;width:200px'>
                        <div id='child' style='margin-top:30px;height:20px'></div>
                    </div>
                </div></body>");
            var floated = LayoutTestHelper.FindById(root, "floated")!;
            var child = LayoutTestHelper.FindById(root, "child")!;
            float gap = child.ContentRect.Y - floated.ContentRect.Y;
            _output.WriteLine($"float→child gap={gap}");
            Assert.True(gap >= 29,
                $"Float BFC should prevent child margin collapse (gap={gap})");
        }

        // [CSS-CONTAIN §3] contain:layout establishes BFC
        [Fact]
        public void ContainLayout_CreatesBfc_PreventsMarginCollapse()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='bfc' style='contain:layout;width:200px'>
                    <div id='child' style='margin-top:30px;height:20px'></div>
                </div></body>");
            var bfc = LayoutTestHelper.FindById(root, "bfc")!;
            var child = LayoutTestHelper.FindById(root, "child")!;
            float gap = child.ContentRect.Y - bfc.ContentRect.Y;
            _output.WriteLine($"contain:layout child gap={gap}");
            Assert.True(gap >= 29,
                $"contain:layout BFC should prevent margin collapse (gap={gap})");
        }
    }
}
