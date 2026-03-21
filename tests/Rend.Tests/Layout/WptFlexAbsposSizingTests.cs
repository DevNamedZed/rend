using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class WptFlexAbsposSizingTests
    {
        private readonly ITestOutputHelper _output;

        public WptFlexAbsposSizingTests(ITestOutputHelper output)
        {
            _output = output;
        }

        // [CSS-FLEXBOX §4.1] Abspos child uses flex container padding box as containing block
        [Fact]
        public void AbsposChildUsesFlexContainerAsContainingBlock()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;position:relative;width:300px;height:200px;padding:10px'>
                    <div id='t' style='position:absolute;top:0;left:0;right:0;bottom:0'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            _output.WriteLine($"abspos: {target!.ContentRect.Width}x{target.ContentRect.Height}");
            Assert.True(System.Math.Abs(target.ContentRect.Width - 320) < 2,
                $"Abspos should fill flex padding box width (got {target.ContentRect.Width})");
            Assert.True(System.Math.Abs(target.ContentRect.Height - 220) < 2,
                $"Abspos should fill flex padding box height (got {target.ContentRect.Height})");
        }

        // [CSS-FLEXBOX §4.1] Abspos child does not affect flex container intrinsic height
        [Fact]
        public void AbsposDoesNotAffectFlexContainerHeight()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='flex' style='display:flex;position:relative;width:300px'>
                    <div style='width:50px;height:40px'></div>
                    <div style='position:absolute;width:100px;height:500px'></div>
                </div></body>");
            var flex = LayoutTestHelper.FindById(root, "flex");
            Assert.NotNull(flex);
            _output.WriteLine($"flex height: {flex!.ContentRect.Height}");
            Assert.True(System.Math.Abs(flex.ContentRect.Height - 40) < 2,
                $"Abspos child should not inflate flex height (got {flex.ContentRect.Height})");
        }

        // [CSS-FLEXBOX §4.1] Abspos child does not affect flex container intrinsic width
        [Fact]
        public void AbsposDoesNotAffectFlexContainerWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:inline-flex;position:relative' id='flex'>
                    <div style='width:80px;height:30px'></div>
                    <div style='position:absolute;width:500px;height:20px'></div>
                </div></body>");
            var flex = LayoutTestHelper.FindById(root, "flex");
            Assert.NotNull(flex);
            _output.WriteLine($"inline-flex width: {flex!.ContentRect.Width}");
            Assert.True(System.Math.Abs(flex.ContentRect.Width - 80) < 2,
                $"Abspos child should not inflate inline-flex width (got {flex.ContentRect.Width})");
        }

        // [CSS2 §10.3] Abspos percentage width resolves against flex container
        [Fact]
        public void AbsposPercentageWidthResolvesAgainstFlexContainer()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;position:relative;width:400px;height:100px'>
                    <div id='t' style='position:absolute;width:50%;height:30px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            _output.WriteLine($"50% width: {target!.ContentRect.Width}");
            Assert.True(System.Math.Abs(target.ContentRect.Width - 200) < 2,
                $"50% of 400px flex container = 200px (got {target.ContentRect.Width})");
        }

        // [CSS2 §10.5] Abspos percentage height resolves against flex container
        [Fact]
        public void AbsposPercentageHeightResolvesAgainstFlexContainer()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;position:relative;width:200px;height:300px'>
                    <div id='t' style='position:absolute;width:40px;height:25%'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            _output.WriteLine($"25% height: {target!.ContentRect.Height}");
            Assert.True(System.Math.Abs(target.ContentRect.Height - 75) < 2,
                $"25% of 300px flex container = 75px (got {target.ContentRect.Height})");
        }

        // [CSS-FLEXBOX §4.1] Abspos with inset:0 fills flex container
        [Fact]
        public void AbsposInsetZeroFillsFlexContainer()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;position:relative;width:250px;height:180px'>
                    <div id='t' style='position:absolute;top:0;right:0;bottom:0;left:0'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            _output.WriteLine($"inset:0 size: {target!.ContentRect.Width}x{target.ContentRect.Height}");
            Assert.True(System.Math.Abs(target.ContentRect.Width - 250) < 2,
                $"inset:0 should fill flex width (got {target.ContentRect.Width})");
            Assert.True(System.Math.Abs(target.ContentRect.Height - 180) < 2,
                $"inset:0 should fill flex height (got {target.ContentRect.Height})");
        }

        // [CSS2 §10.3.7] Abspos centered with margin:auto in flex container
        [Fact]
        public void AbsposCenteredWithMarginAutoInFlex()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;position:relative;width:300px;height:200px'>
                    <div id='t' style='position:absolute;top:0;right:0;bottom:0;left:0;margin:auto;width:100px;height:60px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            _output.WriteLine($"centered: x={target!.ContentRect.X}, y={target.ContentRect.Y}");
            Assert.True(System.Math.Abs(target.ContentRect.X - 100) < 2,
                $"margin:auto horizontal center in 300px = x:100 (got {target.ContentRect.X})");
            Assert.True(System.Math.Abs(target.ContentRect.Y - 70) < 2,
                $"margin:auto vertical center in 200px = y:70 (got {target.ContentRect.Y})");
        }

        // [CSS-FLEXBOX §4.1] Abspos in column flex uses flex container as CB
        [Fact]
        public void AbsposInColumnFlexUsesFlexContainerAsCB()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;position:relative;width:200px;height:300px'>
                    <div id='t' style='position:absolute;left:0;right:0;top:0;bottom:0'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            _output.WriteLine($"column abspos: {target!.ContentRect.Width}x{target.ContentRect.Height}");
            Assert.True(System.Math.Abs(target.ContentRect.Width - 200) < 2,
                $"Column flex abspos width (got {target.ContentRect.Width})");
            Assert.True(System.Math.Abs(target.ContentRect.Height - 300) < 2,
                $"Column flex abspos height (got {target.ContentRect.Height})");
        }

        // [CSS2 §10.3.7] Abspos with explicit width in flex container
        [Fact]
        public void AbsposWithExplicitWidthInFlex()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;position:relative;width:400px;height:100px'>
                    <div id='t' style='position:absolute;width:150px;height:50px;left:20px;top:10px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            _output.WriteLine($"explicit: {target!.ContentRect.Width}x{target.ContentRect.Height} at ({target.ContentRect.X},{target.ContentRect.Y})");
            Assert.True(System.Math.Abs(target.ContentRect.Width - 150) < 2,
                $"Explicit width preserved (got {target.ContentRect.Width})");
            Assert.True(System.Math.Abs(target.ContentRect.X - 20) < 2,
                $"left:20px applied (got {target.ContentRect.X})");
        }

        // [CSS2 §10.6.4] Abspos with explicit height in flex container
        [Fact]
        public void AbsposWithExplicitHeightInFlex()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;position:relative;width:200px;height:300px'>
                    <div id='t' style='position:absolute;width:80px;height:120px;top:30px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            _output.WriteLine($"explicit height: {target!.ContentRect.Height} at y={target.ContentRect.Y}");
            Assert.True(System.Math.Abs(target.ContentRect.Height - 120) < 2,
                $"Explicit height preserved (got {target.ContentRect.Height})");
            Assert.True(System.Math.Abs(target.ContentRect.Y - 30) < 2,
                $"top:30px applied (got {target.ContentRect.Y})");
        }

        // [CSS-FLEXBOX §4.1] Multiple abspos children in flex container
        [Fact]
        public void MultipleAbsposInFlexContainer()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;position:relative;width:300px;height:200px'>
                    <div id='a' style='position:absolute;left:0;top:0;width:50px;height:50px'></div>
                    <div id='b' style='position:absolute;right:0;bottom:0;width:60px;height:60px'></div>
                </div></body>");
            var boxA = LayoutTestHelper.FindById(root, "a");
            var boxB = LayoutTestHelper.FindById(root, "b");
            Assert.NotNull(boxA);
            Assert.NotNull(boxB);
            _output.WriteLine($"a: ({boxA!.ContentRect.X},{boxA.ContentRect.Y}), b: ({boxB!.ContentRect.X},{boxB.ContentRect.Y})");
            Assert.True(System.Math.Abs(boxA.ContentRect.X) < 2, $"a left:0 (got {boxA.ContentRect.X})");
            Assert.True(System.Math.Abs(boxA.ContentRect.Y) < 2, $"a top:0 (got {boxA.ContentRect.Y})");
            Assert.True(System.Math.Abs(boxB.ContentRect.X - 240) < 2,
                $"b right:0 with width 60 in 300px = x:240 (got {boxB.ContentRect.X})");
            Assert.True(System.Math.Abs(boxB.ContentRect.Y - 140) < 2,
                $"b bottom:0 with height 60 in 200px = y:140 (got {boxB.ContentRect.Y})");
        }

        // [CSS-FLEXBOX §4.1] Abspos does not participate in flex grow distribution
        [Fact]
        public void AbsposWithFlexGrowSibling()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;position:relative;width:300px;height:100px'>
                    <div id='grow' style='flex-grow:1;height:40px'></div>
                    <div style='position:absolute;width:200px;height:200px'></div>
                </div></body>");
            var grow = LayoutTestHelper.FindById(root, "grow");
            Assert.NotNull(grow);
            _output.WriteLine($"flex-grow item width: {grow!.ContentRect.Width}");
            Assert.True(System.Math.Abs(grow.ContentRect.Width - 300) < 2,
                $"flex-grow:1 should get all 300px since abspos is out of flow (got {grow.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §4.1] Abspos between flex items does not take space
        [Fact]
        public void AbsposBetweenFlexItemsDoesNotTakeSpace()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;position:relative;width:300px;height:100px'>
                    <div id='first' style='width:100px;height:40px'></div>
                    <div style='position:absolute;width:50px;height:50px'></div>
                    <div id='second' style='width:100px;height:40px'></div>
                </div></body>");
            var first = LayoutTestHelper.FindById(root, "first");
            var second = LayoutTestHelper.FindById(root, "second");
            Assert.NotNull(first);
            Assert.NotNull(second);
            _output.WriteLine($"first.X={first!.ContentRect.X}, second.X={second!.ContentRect.X}");
            Assert.True(System.Math.Abs(second.ContentRect.X - 100) < 2,
                $"Second item should be at x:100, not shifted by abspos (got {second.ContentRect.X})");
        }

        // [CSS2 §9.3.1] Abspos inside a flex item (not direct child of flex container)
        [Fact]
        public void AbsposInFlexItemNotDirectChild()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px;height:200px'>
                    <div style='position:relative;width:200px;height:150px'>
                        <div id='t' style='position:absolute;left:10px;top:10px;width:80px;height:60px'></div>
                    </div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            _output.WriteLine($"nested abspos: ({target!.ContentRect.X},{target.ContentRect.Y}) {target.ContentRect.Width}x{target.ContentRect.Height}");
            Assert.True(System.Math.Abs(target.ContentRect.Width - 80) < 2,
                $"Nested abspos width (got {target.ContentRect.Width})");
            Assert.True(System.Math.Abs(target.ContentRect.Height - 60) < 2,
                $"Nested abspos height (got {target.ContentRect.Height})");
        }

        // [CSS2 §10.1] Relative flex item serves as containing block for abspos grandchild
        [Fact]
        public void RelativeFlexItemAsContainingBlockForAbsposGrandchild()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px;height:200px'>
                    <div style='position:relative;width:200px;height:100px'>
                        <div id='t' style='position:absolute;left:0;right:0;top:0;bottom:0'></div>
                    </div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            _output.WriteLine($"grandchild fills: {target!.ContentRect.Width}x{target.ContentRect.Height}");
            Assert.True(System.Math.Abs(target.ContentRect.Width - 200) < 2,
                $"Grandchild fills relative flex item width (got {target.ContentRect.Width})");
            Assert.True(System.Math.Abs(target.ContentRect.Height - 100) < 2,
                $"Grandchild fills relative flex item height (got {target.ContentRect.Height})");
        }

        // [CSS2 §9.6.1] Fixed position child is out of flow in flex container
        [Fact]
        public void FixedPositionOutOfFlowInFlexContainer()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='flex' style='display:flex;position:relative;width:200px'>
                    <div style='width:80px;height:50px'></div>
                    <div style='position:fixed;width:100px;height:300px'></div>
                </div></body>");
            var flex = LayoutTestHelper.FindById(root, "flex");
            Assert.NotNull(flex);
            _output.WriteLine($"flex height with fixed child: {flex!.ContentRect.Height}");
            Assert.True(System.Math.Abs(flex.ContentRect.Height - 50) < 2,
                $"Fixed child should not affect flex height (got {flex.ContentRect.Height})");
        }

        // [CSS-VALUES §8.1] Abspos with calc() width in flex container
        [Fact]
        public void AbsposWithCalcWidthInFlex()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;position:relative;width:400px;height:100px'>
                    <div id='t' style='position:absolute;width:calc(50% - 20px);height:40px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            _output.WriteLine($"calc width: {target!.ContentRect.Width}");
            Assert.True(System.Math.Abs(target.ContentRect.Width - 180) < 2,
                $"calc(50% - 20px) of 400px = 180px (got {target.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §4.1] Abspos in flex with gap - abspos not affected by gap
        [Fact]
        public void AbsposNotAffectedByFlexGap()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;position:relative;width:300px;height:100px;gap:20px'>
                    <div style='width:100px;height:40px'></div>
                    <div id='t' style='position:absolute;left:0;top:0;width:60px;height:30px'></div>
                    <div style='width:100px;height:40px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            _output.WriteLine($"abspos with gap: x={target!.ContentRect.X}");
            Assert.True(System.Math.Abs(target.ContentRect.X) < 2,
                $"Abspos not shifted by gap, left:0 = x:0 (got {target.ContentRect.X})");
        }

        // [CSS-FLEXBOX §4.1] Abspos in column flex with percentage height
        [Fact]
        public void AbsposPercentageHeightInColumnFlex()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;position:relative;width:200px;height:400px'>
                    <div id='t' style='position:absolute;width:80px;height:50%'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            _output.WriteLine($"column flex abspos 50% height: {target!.ContentRect.Height}");
            Assert.True(System.Math.Abs(target.ContentRect.Height - 200) < 2,
                $"50% of 400px column flex = 200px (got {target.ContentRect.Height})");
        }

        // [CSS2 §10.3.7] Abspos right-aligned in flex container
        [Fact]
        public void AbsposRightAlignedInFlexContainer()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;position:relative;width:300px;height:100px'>
                    <div id='t' style='position:absolute;right:0;width:80px;height:40px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            _output.WriteLine($"right-aligned: x={target!.ContentRect.X}");
            Assert.True(System.Math.Abs(target.ContentRect.X - 220) < 2,
                $"right:0 with width 80 in 300px = x:220 (got {target.ContentRect.X})");
        }

        // [CSS2 §10.6.4] Abspos bottom-aligned in flex container
        [Fact]
        public void AbsposBottomAlignedInFlexContainer()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;position:relative;width:200px;height:250px'>
                    <div id='t' style='position:absolute;bottom:0;width:60px;height:50px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            _output.WriteLine($"bottom-aligned: y={target!.ContentRect.Y}");
            Assert.True(System.Math.Abs(target.ContentRect.Y - 200) < 2,
                $"bottom:0 with height 50 in 250px = y:200 (got {target.ContentRect.Y})");
        }

        // [CSS-FLEXBOX §4.1] Abspos with left+right auto margins centers horizontally in flex
        [Fact]
        public void AbsposAutoMarginHorizontalCenterInFlex()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;position:relative;width:400px;height:100px'>
                    <div id='t' style='position:absolute;left:0;right:0;margin-left:auto;margin-right:auto;width:120px;height:40px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            _output.WriteLine($"auto margin center: x={target!.ContentRect.X}");
            Assert.True(System.Math.Abs(target.ContentRect.X - 140) < 2,
                $"Centered in 400px with 120px width = x:140 (got {target.ContentRect.X})");
        }

        // [CSS-FLEXBOX §4.1] Abspos with top+bottom auto margins centers vertically in flex
        [Fact]
        public void AbsposAutoMarginVerticalCenterInFlex()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;position:relative;width:200px;height:300px'>
                    <div id='t' style='position:absolute;top:0;bottom:0;margin-top:auto;margin-bottom:auto;width:60px;height:80px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            _output.WriteLine($"auto margin vertical center: y={target!.ContentRect.Y}");
            Assert.True(System.Math.Abs(target.ContentRect.Y - 110) < 2,
                $"Centered in 300px with 80px height = y:110 (got {target.ContentRect.Y})");
        }

        // [CSS-FLEXBOX §4.1] Abspos with offset in column flex
        [Fact]
        public void AbsposWithOffsetInColumnFlex()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;position:relative;width:200px;height:300px'>
                    <div id='t' style='position:absolute;left:25px;top:50px;width:100px;height:80px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            _output.WriteLine($"column offset: ({target!.ContentRect.X},{target.ContentRect.Y})");
            Assert.True(System.Math.Abs(target.ContentRect.X - 25) < 2,
                $"left:25px in column flex (got {target.ContentRect.X})");
            Assert.True(System.Math.Abs(target.ContentRect.Y - 50) < 2,
                $"top:50px in column flex (got {target.ContentRect.Y})");
        }

        // [CSS-FLEXBOX §4.1] Flex container height not affected by multiple abspos children
        [Fact]
        public void FlexContainerHeightUnaffectedByMultipleAbspos()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='flex' style='display:flex;position:relative;width:300px'>
                    <div style='width:80px;height:60px'></div>
                    <div style='position:absolute;width:200px;height:400px'></div>
                    <div style='position:absolute;width:150px;height:350px'></div>
                    <div style='position:absolute;width:100px;height:500px'></div>
                </div></body>");
            var flex = LayoutTestHelper.FindById(root, "flex");
            Assert.NotNull(flex);
            _output.WriteLine($"flex height with 3 abspos: {flex!.ContentRect.Height}");
            Assert.True(System.Math.Abs(flex.ContentRect.Height - 60) < 2,
                $"Height should be 60px from normal child only (got {flex.ContentRect.Height})");
        }

        // [CSS2 §10.3.7] Abspos with left+right fills containing block in flex
        [Fact]
        public void AbsposLeftRightFillsContainingBlockInFlex()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;position:relative;width:400px;height:200px'>
                    <div id='t' style='position:absolute;left:30px;right:50px;height:25px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            _output.WriteLine($"left+right fill: {target!.ContentRect.Width}");
            Assert.True(System.Math.Abs(target.ContentRect.Width - 320) < 2,
                $"left:30+right:50 in 400px = 320px (got {target.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §4.1] Abspos with calc height in flex container
        [Fact]
        public void AbsposWithCalcHeightInFlex()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;position:relative;width:200px;height:400px'>
                    <div id='t' style='position:absolute;width:80px;height:calc(25% + 10px)'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            _output.WriteLine($"calc height: {target!.ContentRect.Height}");
            Assert.True(System.Math.Abs(target.ContentRect.Height - 110) < 2,
                $"calc(25% + 10px) of 400px = 110px (got {target.ContentRect.Height})");
        }

        // [CSS-FLEXBOX §4.1] Abspos with padding in flex container
        [Fact]
        public void AbsposWithPaddingInFlexContainer()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;position:relative;width:300px;height:200px;padding:20px'>
                    <div id='t' style='position:absolute;left:0;right:0;height:50px;padding:10px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            _output.WriteLine($"abspos with padding: content={target!.ContentRect.Width}, border={target.BorderRect.Width}");
            Assert.True(System.Math.Abs(target.BorderRect.Width - 340) < 2,
                $"Abspos fills 300+40 padded flex border width (got {target.BorderRect.Width})");
            Assert.True(System.Math.Abs(target.ContentRect.Width - 320) < 2,
                $"Content width = 340 - 2*10 padding = 320 (got {target.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §9.9.3] Abspos flex item static position is before flex items
        [Fact]
        public void AbsposStaticPositionInFlexContainer()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;position:relative;width:300px;height:100px'>
                    <div style='width:60px;height:40px'></div>
                    <div id='t' style='position:absolute;width:50px;height:30px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            _output.WriteLine($"static position abspos: x={target!.ContentRect.X}, y={target.ContentRect.Y}");
            Assert.True(System.Math.Abs(target.ContentRect.Width - 50) < 2,
                $"Abspos explicit width preserved (got {target.ContentRect.Width})");
            Assert.True(System.Math.Abs(target.ContentRect.Height - 30) < 2,
                $"Abspos explicit height preserved (got {target.ContentRect.Height})");
        }
    }
}
