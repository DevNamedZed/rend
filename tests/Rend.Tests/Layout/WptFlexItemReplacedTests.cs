using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    /// <summary>
    /// CSS Flexbox with replaced elements (img, input, select, textarea).
    /// Covers intrinsic sizing, flex-grow/shrink, alignment, aspect-ratio,
    /// min/max constraints, column direction, and margin:auto.
    /// </summary>
    public class WptFlexItemReplacedTests
    {
        private readonly ITestOutputHelper _output;

        public WptFlexItemReplacedTests(ITestOutputHelper output)
        {
            _output = output;
        }

        // [CSS-FLEXBOX §9.2] img with explicit CSS width/height as flex item preserves dimensions
        [Fact]
        public void ImgExplicitDimensions_PreservedInFlex()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <img id='t' style='width:120px;height:90px'>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            _output.WriteLine($"img: {target!.ContentRect.Width}x{target.ContentRect.Height}");
            Assert.True(System.Math.Abs(target.ContentRect.Width - 120) < 2,
                $"img width preserved (got {target.ContentRect.Width})");
            Assert.True(System.Math.Abs(target.ContentRect.Height - 90) < 2,
                $"img height preserved (got {target.ContentRect.Height})");
        }

        // [CSS-FLEXBOX §9.7] img with flex-grow distributes remaining space
        [Fact]
        public void ImgFlexGrow_ExpandsToFillSpace()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <img id='t' width='100' height='80' style='flex-grow:1'>
                    <div style='width:100px;height:80px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            _output.WriteLine($"img width with flex-grow: {target!.ContentRect.Width}");
            Assert.True(target.ContentRect.Width > 200,
                $"img with flex-grow should expand beyond intrinsic (got {target.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §9.7] img with flex-shrink shrinks below intrinsic size
        [Fact]
        public void ImgFlexShrink_ShrinksWhenOverflowing()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:200px'>
                    <img id='t' width='300' height='100' style='flex-shrink:1'>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            _output.WriteLine($"img width with flex-shrink: {target!.ContentRect.Width}");
            Assert.True(target.ContentRect.Width <= 201,
                $"img should shrink to fit container (got {target.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §8.3] align-items:center positions img at cross-axis center
        [Fact]
        public void ImgAlignItemsCenter_CenteredVertically()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;align-items:center;width:300px;height:200px'>
                    <img id='t' style='width:100px;height:60px'>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            float expectedY = (200 - 60) / 2f;
            _output.WriteLine($"img Y: {target!.ContentRect.Y}, expected ~{expectedY}");
            Assert.True(System.Math.Abs(target.ContentRect.Y - expectedY) < 2,
                $"img should be vertically centered (got Y={target.ContentRect.Y}, expected ~{expectedY})");
        }

        // [CSS-IMAGES §5.1] img aspect-ratio preserved via CSS aspect-ratio property
        [Fact]
        public void ImgAspectRatio_HeightDerivedFromWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <img id='t' width='200' height='100' style='width:100px;height:50px'>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            _output.WriteLine($"img: {target!.ContentRect.Width}x{target.ContentRect.Height}");
            Assert.True(System.Math.Abs(target.ContentRect.Width - 100) < 2,
                $"img width from CSS (got {target.ContentRect.Width})");
            Assert.True(System.Math.Abs(target.ContentRect.Height - 50) < 2,
                $"img height from CSS (got {target.ContentRect.Height})");
        }

        // [CSS-IMAGES §5.1] img with both CSS dimensions set in flex
        [Fact]
        public void ImgBothCssDimensions_InFlex()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <img id='t' style='width:160px;height:120px'>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            _output.WriteLine($"img: {target!.ContentRect.Width}x{target.ContentRect.Height}");
            Assert.True(System.Math.Abs(target.ContentRect.Height - 120) < 2,
                $"img height from CSS (got {target.ContentRect.Height})");
            Assert.True(System.Math.Abs(target.ContentRect.Width - 160) < 2,
                $"img width from CSS (got {target.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §4] input as flex item uses intrinsic width
        [Fact]
        public void InputTextAsFlexItem_UsesIntrinsicWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <input id='t' type='text'>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            _output.WriteLine($"input width: {target!.ContentRect.Width}");
            Assert.True(target.ContentRect.Width > 50,
                $"input should have meaningful intrinsic width (got {target.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §4] select as flex item uses intrinsic sizing
        [Fact]
        public void SelectAsFlexItem_UsesIntrinsicSize()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <select id='t'><option>Option 1</option></select>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            _output.WriteLine($"select: {target!.ContentRect.Width}x{target.ContentRect.Height}");
            Assert.True(target.ContentRect.Width > 20,
                $"select should have intrinsic width (got {target.ContentRect.Width})");
            Assert.True(target.ContentRect.Height > 10,
                $"select should have intrinsic height (got {target.ContentRect.Height})");
        }

        // [CSS-FLEXBOX §4] textarea as flex item uses intrinsic sizing
        [Fact]
        public void TextareaAsFlexItem_UsesIntrinsicSize()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <textarea id='t' rows='3' cols='20'></textarea>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            _output.WriteLine($"textarea: {target!.ContentRect.Width}x{target.ContentRect.Height}");
            Assert.True(target.ContentRect.Width > 100,
                $"textarea should have intrinsic width (got {target.ContentRect.Width})");
            Assert.True(target.ContentRect.Height > 30,
                $"textarea should have intrinsic height for 3 rows (got {target.ContentRect.Height})");
        }

        // [CSS-FLEXBOX §9.2] flex-basis:auto on replaced element uses intrinsic main size
        [Fact]
        public void ReplacedFlexBasisAuto_UsesIntrinsicSize()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <img id='t' width='150' height='100' style='flex:0 0 auto'>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            _output.WriteLine($"img flex-basis:auto width: {target!.ContentRect.Width}");
            Assert.True(System.Math.Abs(target.ContentRect.Width - 150) < 2,
                $"flex-basis:auto should use intrinsic width (got {target.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §9.2] explicit flex-basis overrides intrinsic size
        [Fact]
        public void ReplacedExplicitFlexBasis_OverridesIntrinsic()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <img id='t' width='150' height='100' style='flex:0 0 200px'>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            _output.WriteLine($"img with explicit flex-basis: {target!.ContentRect.Width}");
            Assert.True(System.Math.Abs(target.ContentRect.Width - 200) < 2,
                $"explicit flex-basis should override intrinsic (got {target.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §4.5] min-width on replaced flex item prevents shrinking
        [Fact]
        public void ReplacedMinWidth_PreventsShrink()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:100px'>
                    <img id='t' width='200' height='80' style='min-width:150px;flex-shrink:1'>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            _output.WriteLine($"img with min-width: {target!.ContentRect.Width}");
            Assert.True(target.ContentRect.Width >= 149,
                $"min-width should prevent further shrinking (got {target.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §4.5] max-width on replaced flex item clamps growth
        [Fact]
        public void ReplacedMaxWidth_ClampsGrowth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <img id='t' width='100' height='80' style='max-width:150px;flex-grow:1'>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            _output.WriteLine($"img with max-width and flex-grow: {target!.ContentRect.Width}");
            Assert.True(target.ContentRect.Width <= 151,
                $"max-width should clamp growth (got {target.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §4] multiple replaced elements share flex space equally
        [Fact]
        public void MultipleReplacedElements_EqualFlexGrow()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <img id='a' width='50' height='50' style='flex:1'>
                    <img id='b' width='50' height='50' style='flex:1'>
                    <img id='c' width='50' height='50' style='flex:1'>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a");
            var itemB = LayoutTestHelper.FindById(root, "b");
            var itemC = LayoutTestHelper.FindById(root, "c");
            Assert.NotNull(itemA);
            Assert.NotNull(itemB);
            Assert.NotNull(itemC);
            _output.WriteLine($"a={itemA!.ContentRect.Width}, b={itemB!.ContentRect.Width}, c={itemC!.ContentRect.Width}");
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 100) < 2,
                $"each should get ~100px (a got {itemA.ContentRect.Width})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 100) < 2,
                $"each should get ~100px (b got {itemB.ContentRect.Width})");
            Assert.True(System.Math.Abs(itemC.ContentRect.Width - 100) < 2,
                $"each should get ~100px (c got {itemC.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §9] img in column flex direction uses height as main axis
        [Fact]
        public void ImgInColumnFlex_HeightIsMainAxis()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;width:200px;height:300px'>
                    <img id='t' width='100' height='80' style='flex-grow:1'>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            _output.WriteLine($"img in column flex: {target!.ContentRect.Width}x{target.ContentRect.Height}");
            Assert.True(target.ContentRect.Height > 200,
                $"column flex-grow should expand height (got {target.ContentRect.Height})");
        }

        // [CSS-FLEXBOX §8.5] margin:auto on replaced flex item absorbs free space
        [Fact]
        public void ReplacedMarginAuto_AbsorbsFreeSpace()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px;height:200px'>
                    <img id='t' style='width:100px;height:60px;margin:auto'>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            float expectedX = (300 - 100) / 2f;
            float expectedY = (200 - 60) / 2f;
            _output.WriteLine($"img margin:auto position: ({target!.ContentRect.X}, {target.ContentRect.Y})");
            Assert.True(System.Math.Abs(target.ContentRect.X - expectedX) < 2,
                $"margin:auto should center horizontally (got X={target.ContentRect.X}, expected ~{expectedX})");
            Assert.True(System.Math.Abs(target.ContentRect.Y - expectedY) < 2,
                $"margin:auto should center vertically (got Y={target.ContentRect.Y}, expected ~{expectedY})");
        }

        // [CSS-FLEXBOX §9.2] percentage width on replaced flex item resolves against container
        [Fact]
        public void ReplacedPercentageWidth_ResolvesAgainstContainer()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <img id='t' width='200' height='100' style='width:50%'>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            _output.WriteLine($"img 50% width: {target!.ContentRect.Width}");
            Assert.True(System.Math.Abs(target.ContentRect.Width - 200) < 2,
                $"50% of 400 = 200 (got {target.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §8.3] align-items:stretch on img with explicit dimensions
        [Fact]
        public void ImgAlignStretch_WithExplicitHeight_NoDistortion()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;align-items:stretch;width:300px;height:200px'>
                    <img id='t' style='width:100px;height:80px'>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            _output.WriteLine($"img stretch: {target!.ContentRect.Width}x{target.ContentRect.Height}");
            Assert.True(target.ContentRect.Height >= 79,
                $"img height should be at least specified (got {target.ContentRect.Height})");
        }

        // [CSS-FLEXBOX §9.2] checkbox input as flex item preserves 13x13 intrinsic size
        [Fact]
        public void CheckboxInFlex_PreservesIntrinsicSize()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <input id='t' type='checkbox'>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            _output.WriteLine($"checkbox: {target!.ContentRect.Width}x{target.ContentRect.Height}");
            Assert.True(System.Math.Abs(target.ContentRect.Width - 13) < 2,
                $"checkbox intrinsic width is 13px (got {target.ContentRect.Width})");
            Assert.True(System.Math.Abs(target.ContentRect.Height - 13) < 2,
                $"checkbox intrinsic height is 13px (got {target.ContentRect.Height})");
        }

        // [CSS-FLEXBOX §8.3] align-items:flex-end on replaced element positions at bottom
        [Fact]
        public void ImgAlignFlexEnd_PositionedAtBottom()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;align-items:flex-end;width:300px;height:200px'>
                    <img id='t' style='width:100px;height:60px'>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            float expectedY = 200 - 60;
            _output.WriteLine($"img flex-end Y: {target!.ContentRect.Y}, expected ~{expectedY}");
            Assert.True(System.Math.Abs(target.ContentRect.Y - expectedY) < 2,
                $"flex-end should position at bottom (got Y={target.ContentRect.Y}, expected ~{expectedY})");
        }

        // [CSS-FLEXBOX §9.7] mixed replaced and non-replaced items with flex-grow
        [Fact]
        public void MixedReplacedAndDiv_FlexGrowDistribution()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <img id='img' width='80' height='60' style='flex-grow:1'>
                    <div id='div' style='width:80px;height:60px;flex-grow:1'></div>
                </div></body>");
            var imgBox = LayoutTestHelper.FindById(root, "img");
            var divBox = LayoutTestHelper.FindById(root, "div");
            Assert.NotNull(imgBox);
            Assert.NotNull(divBox);
            _output.WriteLine($"img={imgBox!.ContentRect.Width}, div={divBox!.ContentRect.Width}");
            Assert.True(System.Math.Abs(imgBox.ContentRect.Width - 200) < 2,
                $"img should grow equally (got {imgBox.ContentRect.Width})");
            Assert.True(System.Math.Abs(divBox.ContentRect.Width - 200) < 2,
                $"div should grow equally (got {divBox.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §9] column flex with multiple replaced items stacks vertically
        [Fact]
        public void ColumnFlex_MultipleImages_StackVertically()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='flex' style='display:flex;flex-direction:column;width:200px'>
                    <img id='a' width='100' height='40'>
                    <img id='b' width='100' height='60'>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a");
            var itemB = LayoutTestHelper.FindById(root, "b");
            var flexContainer = LayoutTestHelper.FindById(root, "flex");
            Assert.NotNull(itemA);
            Assert.NotNull(itemB);
            Assert.NotNull(flexContainer);
            _output.WriteLine($"a.Y={itemA!.ContentRect.Y}, b.Y={itemB!.ContentRect.Y}");
            Assert.True(itemB.ContentRect.Y >= itemA.ContentRect.Y + 39,
                $"second img should be below first (a.Y={itemA.ContentRect.Y}, b.Y={itemB.ContentRect.Y})");
            Assert.True(System.Math.Abs(flexContainer!.ContentRect.Height - 100) < 2,
                $"column flex auto-height should sum items (got {flexContainer.ContentRect.Height})");
        }

        // [CSS-FLEXBOX §9.7] img with flex-shrink:0 does not shrink
        [Fact]
        public void ImgFlexShrinkZero_DoesNotShrink()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:100px'>
                    <img id='t' width='200' height='80' style='flex-shrink:0'>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            _output.WriteLine($"img flex-shrink:0 width: {target!.ContentRect.Width}");
            Assert.True(System.Math.Abs(target.ContentRect.Width - 200) < 2,
                $"flex-shrink:0 should prevent shrinking (got {target.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §8.3] justify-content:center with replaced items
        [Fact]
        public void JustifyContentCenter_ReplacedItems()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;justify-content:center;width:400px'>
                    <img id='t' width='100' height='60'>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            float expectedX = (400 - 100) / 2f;
            _output.WriteLine($"img justify-center X: {target!.ContentRect.X}, expected ~{expectedX}");
            Assert.True(System.Math.Abs(target.ContentRect.X - expectedX) < 2,
                $"justify-content:center should center img (got X={target.ContentRect.X}, expected ~{expectedX})");
        }

        // [CSS-FLEXBOX §4.5] max-height on replaced flex item in column direction
        [Fact]
        public void ColumnFlex_ReplacedMaxHeight_ClampsGrowth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;width:200px;height:400px'>
                    <img id='t' width='100' height='50' style='flex-grow:1;max-height:150px'>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            _output.WriteLine($"img column max-height: {target!.ContentRect.Height}");
            Assert.True(target.ContentRect.Height <= 151,
                $"max-height should clamp column growth (got {target.ContentRect.Height})");
        }

        // [CSS-FLEXBOX §9.2] img with CSS width overriding attribute, flex-basis:auto uses CSS width
        [Fact]
        public void ImgCssWidth_FlexBasisAutoUsesCssWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <img id='t' width='200' height='100' style='width:80px;flex:0 0 auto'>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            _output.WriteLine($"img CSS width override: {target!.ContentRect.Width}");
            Assert.True(System.Math.Abs(target.ContentRect.Width - 80) < 2,
                $"flex-basis:auto should fall through to CSS width (got {target.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §9] replaced element with gap between items
        [Fact]
        public void ReplacedElements_WithGap()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px;gap:20px'>
                    <img id='a' width='100' height='60'>
                    <img id='b' width='100' height='60'>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a");
            var itemB = LayoutTestHelper.FindById(root, "b");
            Assert.NotNull(itemA);
            Assert.NotNull(itemB);
            float expectedBX = 100 + 20;
            _output.WriteLine($"a.X={itemA!.ContentRect.X}, b.X={itemB!.ContentRect.X}");
            Assert.True(System.Math.Abs(itemB!.ContentRect.X - expectedBX) < 2,
                $"gap should separate items (b.X got {itemB.ContentRect.X}, expected ~{expectedBX})");
        }

        // [CSS-FLEXBOX §4] radio input as flex item preserves 13x13 intrinsic size
        [Fact]
        public void RadioInFlex_PreservesIntrinsicSize()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <input id='t' type='radio'>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            _output.WriteLine($"radio: {target!.ContentRect.Width}x{target.ContentRect.Height}");
            Assert.True(System.Math.Abs(target.ContentRect.Width - 13) < 2,
                $"radio intrinsic width is 13px (got {target.ContentRect.Width})");
            Assert.True(System.Math.Abs(target.ContentRect.Height - 13) < 2,
                $"radio intrinsic height is 13px (got {target.ContentRect.Height})");
        }

        // [CSS-FLEXBOX §8.3] justify-content:space-between with replaced items
        [Fact]
        public void JustifyContentSpaceBetween_ReplacedItems()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;justify-content:space-between;width:400px'>
                    <img id='a' width='80' height='40'>
                    <img id='b' width='80' height='40'>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a");
            var itemB = LayoutTestHelper.FindById(root, "b");
            Assert.NotNull(itemA);
            Assert.NotNull(itemB);
            _output.WriteLine($"a.X={itemA!.ContentRect.X}, b.X={itemB!.ContentRect.X}");
            Assert.True(System.Math.Abs(itemA.ContentRect.X - 0) < 2,
                $"first item at start (got X={itemA.ContentRect.X})");
            Assert.True(System.Math.Abs(itemB.ContentRect.X - 320) < 2,
                $"second item at end (got X={itemB.ContentRect.X}, expected ~320)");
        }

        // [CSS-FLEXBOX §9] column flex with img using flex-shrink
        [Fact]
        public void ColumnFlex_ImgFlexShrink()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;width:200px;height:100px'>
                    <img id='t' width='100' height='200' style='flex-shrink:1'>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            _output.WriteLine($"img column shrink height: {target!.ContentRect.Height}");
            Assert.True(target.ContentRect.Height <= 101,
                $"img should shrink in column flex (got {target.ContentRect.Height})");
        }
    }
}
