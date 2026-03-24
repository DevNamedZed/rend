using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    /// <summary>
    /// Edge-case tests for CSS 2.1 §10.6.3 block-level auto height computation,
    /// explicit height overrides, percentage heights, min/max-height, and
    /// interactions with display modes and positioning schemes.
    /// </summary>
    public class WptBlockHeightAutoEdgeTests
    {
        private readonly ITestOutputHelper _output;

        public WptBlockHeightAutoEdgeTests(ITestOutputHelper output)
        {
            _output = output;
        }

        // [CSS2 §10.6.3] auto height with single child equals child height
        [Fact]
        public void AutoHeight_SingleChild_EqualsChildHeight()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='parent' style='width:200px'>
                    <div style='height:50px'></div>
                </div></body>");
            var parent = LayoutTestHelper.FindById(root, "parent")!;
            _output.WriteLine($"height={parent.ContentRect.Height}");
            Assert.True(System.Math.Abs(parent.ContentRect.Height - 50) < 2,
                $"Auto height single child should be 50 (got {parent.ContentRect.Height})");
        }

        // [CSS2 §10.6.3] auto height with two children sums their heights
        [Fact]
        public void AutoHeight_TwoChildren_SumsHeights()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='parent' style='width:200px'>
                    <div style='height:30px'></div>
                    <div style='height:70px'></div>
                </div></body>");
            var parent = LayoutTestHelper.FindById(root, "parent")!;
            _output.WriteLine($"height={parent.ContentRect.Height}");
            Assert.True(System.Math.Abs(parent.ContentRect.Height - 100) < 2,
                $"Auto height two children should be 100 (got {parent.ContentRect.Height})");
        }

        // [CSS2 §10.6.3] auto height with three children sums their heights
        [Fact]
        public void AutoHeight_ThreeChildren_SumsHeights()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='parent' style='width:200px'>
                    <div style='height:20px'></div>
                    <div style='height:40px'></div>
                    <div style='height:60px'></div>
                </div></body>");
            var parent = LayoutTestHelper.FindById(root, "parent")!;
            _output.WriteLine($"height={parent.ContentRect.Height}");
            Assert.True(System.Math.Abs(parent.ContentRect.Height - 120) < 2,
                $"Auto height three children should be 120 (got {parent.ContentRect.Height})");
        }

        // [CSS2 §10.6.3] auto height with no children is zero
        [Fact]
        public void AutoHeight_Empty_IsZero()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='parent' style='width:200px'></div></body>");
            var parent = LayoutTestHelper.FindById(root, "parent")!;
            _output.WriteLine($"height={parent.ContentRect.Height}");
            Assert.True(parent.ContentRect.Height < 1,
                $"Auto height empty should be 0 (got {parent.ContentRect.Height})");
        }

        // [CSS2 §10.6.3] padding does not change content height computation
        [Fact]
        public void AutoHeight_WithPadding_ContentHeightUnchanged()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='parent' style='width:200px;padding:20px'>
                    <div style='height:50px'></div>
                </div></body>");
            var parent = LayoutTestHelper.FindById(root, "parent")!;
            _output.WriteLine($"contentHeight={parent.ContentRect.Height}");
            Assert.True(System.Math.Abs(parent.ContentRect.Height - 50) < 2,
                $"Padding should not affect content height (got {parent.ContentRect.Height})");
        }

        // [CSS2 §10.6.3] border does not change content height computation
        [Fact]
        public void AutoHeight_WithBorder_ContentHeightUnchanged()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='parent' style='width:200px;border:10px solid black'>
                    <div style='height:50px'></div>
                </div></body>");
            var parent = LayoutTestHelper.FindById(root, "parent")!;
            _output.WriteLine($"contentHeight={parent.ContentRect.Height}");
            Assert.True(System.Math.Abs(parent.ContentRect.Height - 50) < 2,
                $"Border should not affect content height (got {parent.ContentRect.Height})");
        }

        // [CSS2 §10.6.3] display:none children excluded from auto height
        [Fact]
        public void AutoHeight_DisplayNone_Excluded()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='parent' style='width:200px'>
                    <div style='height:30px'></div>
                    <div style='display:none;height:100px'></div>
                    <div style='height:30px'></div>
                </div></body>");
            var parent = LayoutTestHelper.FindById(root, "parent")!;
            _output.WriteLine($"height={parent.ContentRect.Height}");
            Assert.True(System.Math.Abs(parent.ContentRect.Height - 60) < 2,
                $"display:none excluded from height (got {parent.ContentRect.Height})");
        }

        // [CSS2 §10.6.3] visibility:hidden children included in auto height
        [Fact]
        public void AutoHeight_VisibilityHidden_Included()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='parent' style='width:200px'>
                    <div style='height:30px'></div>
                    <div style='visibility:hidden;height:50px'></div>
                    <div style='height:30px'></div>
                </div></body>");
            var parent = LayoutTestHelper.FindById(root, "parent")!;
            _output.WriteLine($"height={parent.ContentRect.Height}");
            Assert.True(System.Math.Abs(parent.ContentRect.Height - 110) < 2,
                $"visibility:hidden included in height (got {parent.ContentRect.Height})");
        }

        // [CSS2 §10.6.3] absolutely positioned children excluded from auto height
        [Fact]
        public void AutoHeight_AbsolutelyPositioned_Excluded()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='parent' style='position:relative;width:200px'>
                    <div style='height:30px'></div>
                    <div style='position:absolute;height:200px;width:100px'></div>
                </div></body>");
            var parent = LayoutTestHelper.FindById(root, "parent")!;
            _output.WriteLine($"height={parent.ContentRect.Height}");
            Assert.True(System.Math.Abs(parent.ContentRect.Height - 30) < 2,
                $"Abspos excluded from auto height (got {parent.ContentRect.Height})");
        }

        // [CSS2 §10.6.7] BFC auto height includes floats
        [Fact]
        public void AutoHeight_BfcContainsFloats()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='parent' style='overflow:hidden;width:200px'>
                    <div style='float:left;width:80px;height:100px'></div>
                    <div style='height:30px'></div>
                </div></body>");
            var parent = LayoutTestHelper.FindById(root, "parent")!;
            _output.WriteLine($"height={parent.ContentRect.Height}");
            Assert.True(parent.ContentRect.Height >= 99,
                $"BFC contains floats (got {parent.ContentRect.Height})");
        }

        // [CSS2 §10.6.3] non-BFC auto height ignores floats
        [Fact]
        public void AutoHeight_NonBfcIgnoresFloats()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='parent' style='width:200px'>
                    <div style='float:left;width:80px;height:100px'></div>
                </div></body>");
            var parent = LayoutTestHelper.FindById(root, "parent")!;
            _output.WriteLine($"height={parent.ContentRect.Height}");
            Assert.True(parent.ContentRect.Height < 2,
                $"Non-BFC ignores floats (got {parent.ContentRect.Height})");
        }

        // [CSS2 §10.6.2] explicit height overrides auto
        [Fact]
        public void ExplicitHeight_OverridesAuto()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='parent' style='width:200px;height:150px'>
                    <div style='height:30px'></div>
                </div></body>");
            var parent = LayoutTestHelper.FindById(root, "parent")!;
            _output.WriteLine($"height={parent.ContentRect.Height}");
            Assert.True(System.Math.Abs(parent.ContentRect.Height - 150) < 2,
                $"Explicit height overrides auto (got {parent.ContentRect.Height})");
        }

        // [CSS2 §10.5] percentage height with definite parent
        [Fact]
        public void PercentageHeight_WithDefiniteParent()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='height:200px;width:200px'>
                    <div id='child' style='height:50%'></div>
                </div></body>");
            var child = LayoutTestHelper.FindById(root, "child")!;
            _output.WriteLine($"height={child.ContentRect.Height}");
            Assert.True(System.Math.Abs(child.ContentRect.Height - 100) < 2,
                $"50% of 200 should be 100 (got {child.ContentRect.Height})");
        }

        // [CSS2 §10.5] percentage height with auto parent resolves to auto (0 with no content)
        [Fact]
        public void PercentageHeight_WithAutoParent_ResolvesToZero()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='width:200px'>
                    <div id='child' style='height:50%'></div>
                </div></body>");
            var child = LayoutTestHelper.FindById(root, "child")!;
            _output.WriteLine($"height={child.ContentRect.Height}");
            Assert.True(child.ContentRect.Height < 2,
                $"Percentage height with auto parent should be 0 (got {child.ContentRect.Height})");
        }

        // [CSS2 §10.7] min-height floors auto height
        [Fact]
        public void MinHeight_FloorsAutoHeight()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='parent' style='width:200px;min-height:100px'>
                    <div style='height:30px'></div>
                </div></body>");
            var parent = LayoutTestHelper.FindById(root, "parent")!;
            _output.WriteLine($"height={parent.ContentRect.Height}");
            Assert.True(System.Math.Abs(parent.ContentRect.Height - 100) < 2,
                $"min-height should floor auto height (got {parent.ContentRect.Height})");
        }

        // [CSS2 §10.7] max-height caps auto height
        [Fact]
        public void MaxHeight_CapsAutoHeight()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='parent' style='width:200px;max-height:40px'>
                    <div style='height:30px'></div>
                    <div style='height:30px'></div>
                    <div style='height:30px'></div>
                </div></body>");
            var parent = LayoutTestHelper.FindById(root, "parent")!;
            _output.WriteLine($"height={parent.ContentRect.Height}");
            Assert.True(parent.ContentRect.Height <= 41,
                $"max-height should cap auto height (got {parent.ContentRect.Height})");
        }

        // [CSS2 §10.6.3] auto height with nested blocks
        [Fact]
        public void AutoHeight_NestedBlocks()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='outer' style='width:200px'>
                    <div>
                        <div>
                            <div style='height:40px'></div>
                        </div>
                    </div>
                </div></body>");
            var outer = LayoutTestHelper.FindById(root, "outer")!;
            _output.WriteLine($"height={outer.ContentRect.Height}");
            Assert.True(System.Math.Abs(outer.ContentRect.Height - 40) < 2,
                $"Nested blocks propagate auto height (got {outer.ContentRect.Height})");
        }

        // [CSS2 §10.6.3] auto height includes child margins (within BFC)
        [Fact]
        public void AutoHeight_WithChildMargins()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='parent' style='width:200px;overflow:hidden'>
                    <div style='margin-top:10px;margin-bottom:20px;height:40px'></div>
                </div></body>");
            var parent = LayoutTestHelper.FindById(root, "parent")!;
            _output.WriteLine($"height={parent.ContentRect.Height}");
            Assert.True(System.Math.Abs(parent.ContentRect.Height - 70) < 2,
                $"Auto height includes margins 10+40+20=70 (got {parent.ContentRect.Height})");
        }

        // [CSS-FLEX §9.4] auto height flex container wraps content
        [Fact]
        public void AutoHeight_FlexContainer()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='flex' style='display:flex;width:200px'>
                    <div style='width:100px;height:80px'></div>
                    <div style='width:100px;height:60px'></div>
                </div></body>");
            var flex = LayoutTestHelper.FindById(root, "flex")!;
            _output.WriteLine($"height={flex.ContentRect.Height}");
            Assert.True(System.Math.Abs(flex.ContentRect.Height - 80) < 2,
                $"Flex auto height = tallest item = 80 (got {flex.ContentRect.Height})");
        }

        // [CSS-GRID §12.4] auto height grid container wraps content
        [Fact]
        public void AutoHeight_GridContainer()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='grid' style='display:grid;grid-template-columns:100px 100px;width:200px'>
                    <div style='height:50px'></div>
                    <div style='height:70px'></div>
                </div></body>");
            var grid = LayoutTestHelper.FindById(root, "grid")!;
            _output.WriteLine($"height={grid.ContentRect.Height}");
            Assert.True(System.Math.Abs(grid.ContentRect.Height - 70) < 2,
                $"Grid auto height = tallest row item = 70 (got {grid.ContentRect.Height})");
        }

        // [CSS-VALUES §5.1.2] vh unit for height
        [Fact]
        public void VhHeight_ResolvesFromViewport()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='test' style='width:200px;height:50vh'></div></body>",
                viewportWidth: 400, viewportHeight: 600);
            var box = LayoutTestHelper.FindById(root, "test")!;
            _output.WriteLine($"height={box.ContentRect.Height}");
            Assert.True(System.Math.Abs(box.ContentRect.Height - 300) < 2,
                $"50vh of 600 should be 300 (got {box.ContentRect.Height})");
        }

        // [CSS-VALUES §8.1] calc height
        [Fact]
        public void CalcHeight_Resolves()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='width:200px;height:400px'>
                    <div id='test' style='height:calc(50% - 30px)'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "test")!;
            _output.WriteLine($"height={box.ContentRect.Height}");
            Assert.True(System.Math.Abs(box.ContentRect.Height - 170) < 2,
                $"calc(50% - 30px) of 400 = 170 (got {box.ContentRect.Height})");
        }

        // [CSS2 §10.6.2] height:0 produces zero-height box
        [Fact]
        public void HeightZero_ProducesZeroHeightBox()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='test' style='width:200px;height:0'>
                    <div style='height:100px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "test")!;
            _output.WriteLine($"height={box.ContentRect.Height}");
            Assert.True(box.ContentRect.Height < 1,
                $"height:0 should produce zero height (got {box.ContentRect.Height})");
        }

        // [CSS2 §10.7] min-height with no content
        [Fact]
        public void MinHeight_Empty_AppliesMinimum()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='test' style='width:200px;min-height:60px'></div></body>");
            var box = LayoutTestHelper.FindById(root, "test")!;
            _output.WriteLine($"height={box.ContentRect.Height}");
            Assert.True(System.Math.Abs(box.ContentRect.Height - 60) < 2,
                $"min-height on empty box (got {box.ContentRect.Height})");
        }

        // [CSS2 §10.7] max-height with content smaller than max has no effect
        [Fact]
        public void MaxHeight_ContentSmallerThanMax_NoEffect()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='test' style='width:200px;max-height:200px'>
                    <div style='height:50px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "test")!;
            _output.WriteLine($"height={box.ContentRect.Height}");
            Assert.True(System.Math.Abs(box.ContentRect.Height - 50) < 2,
                $"max-height should not affect smaller content (got {box.ContentRect.Height})");
        }

        // [CSS2 §10.7] min-height overrides max-height when min > max
        [Fact]
        public void MinHeight_OverridesMaxHeight_WhenMinGreater()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='test' style='width:200px;min-height:150px;max-height:80px'></div></body>");
            var box = LayoutTestHelper.FindById(root, "test")!;
            _output.WriteLine($"height={box.ContentRect.Height}");
            Assert.True(box.ContentRect.Height >= 149,
                $"min-height wins over max-height (got {box.ContentRect.Height})");
        }

        // [CSS2 §10.6.3] auto height with padding+border on parent does not double-count
        [Fact]
        public void AutoHeight_PaddingAndBorder_NoDoubleCount()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='parent' style='width:200px;padding:15px;border:5px solid black'>
                    <div style='height:40px'></div>
                </div></body>");
            var parent = LayoutTestHelper.FindById(root, "parent")!;
            _output.WriteLine($"contentHeight={parent.ContentRect.Height}");
            Assert.True(System.Math.Abs(parent.ContentRect.Height - 40) < 2,
                $"Content height should be 40 despite padding+border (got {parent.ContentRect.Height})");
        }

        // [CSS-FLEX §9.4] auto height flex column sums children
        [Fact]
        public void AutoHeight_FlexColumn_SumsChildren()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='flex' style='display:flex;flex-direction:column;width:200px'>
                    <div style='height:30px'></div>
                    <div style='height:50px'></div>
                    <div style='height:20px'></div>
                </div></body>");
            var flex = LayoutTestHelper.FindById(root, "flex")!;
            _output.WriteLine($"height={flex.ContentRect.Height}");
            Assert.True(System.Math.Abs(flex.ContentRect.Height - 100) < 2,
                $"Flex column auto height sums items (got {flex.ContentRect.Height})");
        }

        // [CSS-GRID §12.4] auto height grid with multiple rows
        [Fact]
        public void AutoHeight_GridMultipleRows()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='grid' style='display:grid;grid-template-columns:100px;width:200px'>
                    <div style='height:40px'></div>
                    <div style='height:60px'></div>
                </div></body>");
            var grid = LayoutTestHelper.FindById(root, "grid")!;
            _output.WriteLine($"height={grid.ContentRect.Height}");
            Assert.True(System.Math.Abs(grid.ContentRect.Height - 100) < 2,
                $"Grid multiple rows sum heights (got {grid.ContentRect.Height})");
        }

        // [CSS2 §10.6.3] multiple display:none children, auto height is zero
        [Fact]
        public void AutoHeight_AllDisplayNone_IsZero()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='parent' style='width:200px'>
                    <div style='display:none;height:50px'></div>
                    <div style='display:none;height:80px'></div>
                </div></body>");
            var parent = LayoutTestHelper.FindById(root, "parent")!;
            _output.WriteLine($"height={parent.ContentRect.Height}");
            Assert.True(parent.ContentRect.Height < 1,
                $"All display:none children should yield 0 height (got {parent.ContentRect.Height})");
        }

        // [CSS2 §10.6.2] explicit height:0 with min-height still applies min-height
        [Fact]
        public void HeightZero_WithMinHeight_AppliesMin()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='test' style='width:200px;height:0;min-height:50px'></div></body>");
            var box = LayoutTestHelper.FindById(root, "test")!;
            _output.WriteLine($"height={box.ContentRect.Height}");
            Assert.True(System.Math.Abs(box.ContentRect.Height - 50) < 2,
                $"min-height overrides height:0 (got {box.ContentRect.Height})");
        }

        // [CSS-FLEX §9.4] auto height flex row with gap
        [Fact]
        public void AutoHeight_FlexRowWithGap()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='flex' style='display:flex;gap:10px;width:200px'>
                    <div style='width:50px;height:40px'></div>
                    <div style='width:50px;height:60px'></div>
                </div></body>");
            var flex = LayoutTestHelper.FindById(root, "flex")!;
            _output.WriteLine($"height={flex.ContentRect.Height}");
            Assert.True(System.Math.Abs(flex.ContentRect.Height - 60) < 2,
                $"Flex row height = tallest item regardless of gap (got {flex.ContentRect.Height})");
        }
    }
}
