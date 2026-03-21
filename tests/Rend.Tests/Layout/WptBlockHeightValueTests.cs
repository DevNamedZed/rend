using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    /// <summary>
    /// CSS 2.1 section 10.5/10.6/10.7 block-level height value resolution:
    /// explicit px heights, auto height from children, percentage heights,
    /// viewport-relative heights, calc() heights, min/max-height interactions,
    /// box-sizing border-box, and auto height in flex/grid containers.
    /// </summary>
    public class WptBlockHeightValueTests
    {
        private readonly ITestOutputHelper _output;

        public WptBlockHeightValueTests(ITestOutputHelper output)
        {
            _output = output;
        }

        // [CSS2 §10.5] explicit height:50px
        [Fact]
        public void ExplicitHeight_50px()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='t' style='width:100px;height:50px'></div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"height={box.ContentRect.Height}");
            Assert.True(System.Math.Abs(box.ContentRect.Height - 50) < 2,
                $"height:50px should produce 50 (got {box.ContentRect.Height})");
        }

        // [CSS2 §10.5] explicit height:100px
        [Fact]
        public void ExplicitHeight_100px()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='t' style='width:100px;height:100px'></div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"height={box.ContentRect.Height}");
            Assert.True(System.Math.Abs(box.ContentRect.Height - 100) < 2,
                $"height:100px should produce 100 (got {box.ContentRect.Height})");
        }

        // [CSS2 §10.5] explicit height:200px
        [Fact]
        public void ExplicitHeight_200px()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='t' style='width:100px;height:200px'></div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"height={box.ContentRect.Height}");
            Assert.True(System.Math.Abs(box.ContentRect.Height - 200) < 2,
                $"height:200px should produce 200 (got {box.ContentRect.Height})");
        }

        // [CSS2 §10.6.3] auto height with no children equals zero
        [Fact]
        public void AutoHeight_Empty_IsZero()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='t' style='width:100px'></div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"height={box.ContentRect.Height}");
            Assert.True(box.ContentRect.Height < 1,
                $"Auto height with no children should be 0 (got {box.ContentRect.Height})");
        }

        // [CSS2 §10.6.3] auto height with one child equals child height
        [Fact]
        public void AutoHeight_OneChild_EqualsChildHeight()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='t' style='width:200px'>
                    <div style='height:75px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"height={box.ContentRect.Height}");
            Assert.True(System.Math.Abs(box.ContentRect.Height - 75) < 2,
                $"Auto height with one 75px child should be 75 (got {box.ContentRect.Height})");
        }

        // [CSS2 §10.6.3] auto height with two children equals sum
        [Fact]
        public void AutoHeight_TwoChildren_EqualsSum()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='t' style='width:200px'>
                    <div style='height:40px'></div>
                    <div style='height:60px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"height={box.ContentRect.Height}");
            Assert.True(System.Math.Abs(box.ContentRect.Height - 100) < 2,
                $"Auto height with two children should be 100 (got {box.ContentRect.Height})");
        }

        // [CSS2 §10.6.3] auto height with three children equals sum
        [Fact]
        public void AutoHeight_ThreeChildren_EqualsSum()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='t' style='width:200px'>
                    <div style='height:25px'></div>
                    <div style='height:35px'></div>
                    <div style='height:40px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"height={box.ContentRect.Height}");
            Assert.True(System.Math.Abs(box.ContentRect.Height - 100) < 2,
                $"Auto height with three children should be 100 (got {box.ContentRect.Height})");
        }

        // [CSS2 §10.5] height:50% of parent 200px equals 100px
        [Fact]
        public void PercentageHeight_50PercentOf200_Equals100()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='height:200px;width:200px'>
                    <div id='t' style='height:50%'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"height={box.ContentRect.Height}");
            Assert.True(System.Math.Abs(box.ContentRect.Height - 100) < 2,
                $"50% of 200 should be 100 (got {box.ContentRect.Height})");
        }

        // [CSS2 §10.5] height:100% of parent 200px equals 200px
        [Fact]
        public void PercentageHeight_100PercentOf200_Equals200()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='height:200px;width:200px'>
                    <div id='t' style='height:100%'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"height={box.ContentRect.Height}");
            Assert.True(System.Math.Abs(box.ContentRect.Height - 200) < 2,
                $"100% of 200 should be 200 (got {box.ContentRect.Height})");
        }

        // [CSS2 §10.5] height:25% of parent 400px equals 100px
        [Fact]
        public void PercentageHeight_25PercentOf400_Equals100()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='height:400px;width:200px'>
                    <div id='t' style='height:25%'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"height={box.ContentRect.Height}");
            Assert.True(System.Math.Abs(box.ContentRect.Height - 100) < 2,
                $"25% of 400 should be 100 (got {box.ContentRect.Height})");
        }

        // [CSS3-VALUES §5.1.2] height:50vh on 300px viewport equals 150px
        [Fact]
        public void VhHeight_50vh_On300Viewport_Equals150()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='t' style='width:100px;height:50vh'></div></body>",
                viewportWidth: 400, viewportHeight: 300);
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"height={box.ContentRect.Height}");
            Assert.True(System.Math.Abs(box.ContentRect.Height - 150) < 2,
                $"50vh of 300 should be 150 (got {box.ContentRect.Height})");
        }

        // [CSS3-VALUES §5.1.2] height:100vh on 300px viewport equals 300px
        [Fact]
        public void VhHeight_100vh_On300Viewport_Equals300()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='t' style='width:100px;height:100vh'></div></body>",
                viewportWidth: 400, viewportHeight: 300);
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"height={box.ContentRect.Height}");
            Assert.True(System.Math.Abs(box.ContentRect.Height - 300) < 2,
                $"100vh of 300 should be 300 (got {box.ContentRect.Height})");
        }

        // [CSS3-VALUES §8.1] height:calc(50% + 20px) in 200px parent equals 120px
        [Fact]
        public void CalcHeight_50PercentPlus20px_In200Parent_Equals120()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='height:200px;width:200px'>
                    <div id='t' style='height:calc(50% + 20px)'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"height={box.ContentRect.Height}");
            Assert.True(System.Math.Abs(box.ContentRect.Height - 120) < 2,
                $"calc(50% + 20px) of 200 should be 120 (got {box.ContentRect.Height})");
        }

        // [CSS2 §10.7] min-height:100px on empty block
        [Fact]
        public void MinHeight_100px_OnEmptyBlock()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='t' style='width:100px;min-height:100px'></div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"height={box.ContentRect.Height}");
            Assert.True(System.Math.Abs(box.ContentRect.Height - 100) < 2,
                $"min-height:100px should produce 100 (got {box.ContentRect.Height})");
        }

        // [CSS2 §10.7] max-height:80px clamps explicit height:200px
        [Fact]
        public void MaxHeight_80px_ClampsHeight200px()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='t' style='width:100px;height:200px;max-height:80px'></div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"height={box.ContentRect.Height}");
            Assert.True(System.Math.Abs(box.ContentRect.Height - 80) < 2,
                $"max-height:80px should clamp 200 to 80 (got {box.ContentRect.Height})");
        }

        // [CSS2 §10.7] min-height overrides max-height when min > max
        [Fact]
        public void MinHeight_OverridesMaxHeight_WhenMinGreater()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='t' style='width:100px;min-height:120px;max-height:80px'></div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"height={box.ContentRect.Height}");
            Assert.True(System.Math.Abs(box.ContentRect.Height - 120) < 2,
                $"min-height:120px should override max-height:80px (got {box.ContentRect.Height})");
        }

        // [CSS2 §10.5] height:0 produces zero-height box
        [Fact]
        public void ExplicitHeight_Zero()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='t' style='width:100px;height:0'></div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"height={box.ContentRect.Height}");
            Assert.True(box.ContentRect.Height < 1,
                $"height:0 should produce 0 (got {box.ContentRect.Height})");
        }

        // [CSS2 §10.6.3] auto height with padding: content height unchanged
        [Fact]
        public void AutoHeight_WithPadding_ContentHeightUnchanged()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='t' style='width:200px;padding:20px'>
                    <div style='height:60px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"contentHeight={box.ContentRect.Height} paddingTop={box.PaddingTop} paddingBottom={box.PaddingBottom}");
            Assert.True(System.Math.Abs(box.ContentRect.Height - 60) < 2,
                $"Padding should not affect content height (got {box.ContentRect.Height})");
            Assert.True(System.Math.Abs(box.PaddingTop - 20) < 2,
                $"PaddingTop should be 20 (got {box.PaddingTop})");
            Assert.True(System.Math.Abs(box.PaddingBottom - 20) < 2,
                $"PaddingBottom should be 20 (got {box.PaddingBottom})");
        }

        // [CSS2 §10.6.3] auto height with border: content height unchanged
        [Fact]
        public void AutoHeight_WithBorder_ContentHeightUnchanged()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='t' style='width:200px;border:10px solid black'>
                    <div style='height:60px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"contentHeight={box.ContentRect.Height} borderTop={box.BorderTopWidth} borderBottom={box.BorderBottomWidth}");
            Assert.True(System.Math.Abs(box.ContentRect.Height - 60) < 2,
                $"Border should not affect content height (got {box.ContentRect.Height})");
            Assert.True(System.Math.Abs(box.BorderTopWidth - 10) < 2,
                $"BorderTopWidth should be 10 (got {box.BorderTopWidth})");
            Assert.True(System.Math.Abs(box.BorderBottomWidth - 10) < 2,
                $"BorderBottomWidth should be 10 (got {box.BorderBottomWidth})");
        }

        // [CSS-SIZING §5.3.3] border-box height:100px with padding:15px yields 70px content height
        [Fact]
        public void BorderBox_Height100px_Padding15px_ContentEquals70()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='t' style='box-sizing:border-box;width:200px;height:100px;padding:15px'></div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"contentHeight={box.ContentRect.Height}");
            Assert.True(System.Math.Abs(box.ContentRect.Height - 70) < 2,
                $"border-box height:100px - padding 15*2 = 70 content (got {box.ContentRect.Height})");
        }

        // [CSS-FLEXBOX §9.4] auto height flex row equals tallest item
        [Fact]
        public void AutoHeight_FlexRow_EqualsTallestItem()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='t' style='display:flex;width:300px'>
                    <div style='width:100px;height:40px'></div>
                    <div style='width:100px;height:90px'></div>
                    <div style='width:100px;height:60px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"height={box.ContentRect.Height}");
            Assert.True(System.Math.Abs(box.ContentRect.Height - 90) < 2,
                $"Flex row auto height should be tallest item 90 (got {box.ContentRect.Height})");
        }

        // [CSS-FLEXBOX §9.4] auto height flex column equals sum of items
        [Fact]
        public void AutoHeight_FlexColumn_EqualsSumOfItems()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='t' style='display:flex;flex-direction:column;width:200px'>
                    <div style='height:30px'></div>
                    <div style='height:40px'></div>
                    <div style='height:50px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"height={box.ContentRect.Height}");
            Assert.True(System.Math.Abs(box.ContentRect.Height - 120) < 2,
                $"Flex column auto height should be sum 120 (got {box.ContentRect.Height})");
        }

        // [CSS-GRID §12.4] auto height grid equals sum of row heights
        [Fact]
        public void AutoHeight_Grid_EqualsRowsSum()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='t' style='display:grid;grid-template-columns:100px 100px;width:200px'>
                    <div style='height:50px'></div>
                    <div style='height:70px'></div>
                    <div style='height:30px'></div>
                    <div style='height:40px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"height={box.ContentRect.Height}");
            // Row 1: max(50, 70) = 70, Row 2: max(30, 40) = 40, total = 110
            Assert.True(System.Math.Abs(box.ContentRect.Height - 110) < 2,
                $"Grid auto height should be row sums 110 (got {box.ContentRect.Height})");
        }

        // [CSS2 §10.7] min-height with content smaller than min
        [Fact]
        public void MinHeight_WithSmallContent_AppliesMin()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='t' style='width:200px;min-height:100px'>
                    <div style='height:20px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"height={box.ContentRect.Height}");
            Assert.True(System.Math.Abs(box.ContentRect.Height - 100) < 2,
                $"min-height:100px should override auto 20 (got {box.ContentRect.Height})");
        }

        // [CSS2 §10.7] max-height with auto height larger than max
        [Fact]
        public void MaxHeight_WithLargeContent_ClampsToMax()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='t' style='width:200px;max-height:80px'>
                    <div style='height:50px'></div>
                    <div style='height:50px'></div>
                    <div style='height:50px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"height={box.ContentRect.Height}");
            Assert.True(System.Math.Abs(box.ContentRect.Height - 80) < 2,
                $"max-height:80px should clamp auto 150 (got {box.ContentRect.Height})");
        }
    }
}
