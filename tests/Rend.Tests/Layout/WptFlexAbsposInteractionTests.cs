using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class WptFlexAbsposInteractionTests
    {
        private readonly ITestOutputHelper _output;

        public WptFlexAbsposInteractionTests(ITestOutputHelper output)
        {
            _output = output;
        }

        // [CSS-FLEXBOX §4.1] Abspos children of flex container are not flex items
        [Fact]
        public void AbsposChild_NotFlexItem_DoesNotAffectSiblingPositions()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;position:relative;width:300px'>
                    <div id='a' style='width:80px;height:40px'></div>
                    <div id='abs' style='position:absolute;width:50px;height:50px'></div>
                    <div id='b' style='width:80px;height:40px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a");
            var itemB = LayoutTestHelper.FindById(root, "b");
            Assert.NotNull(itemA);
            Assert.NotNull(itemB);
            _output.WriteLine($"a.X={itemA!.ContentRect.X}, b.X={itemB!.ContentRect.X}");
            Assert.True(System.Math.Abs(itemB.ContentRect.X - 80) < 2,
                $"Abspos child skipped in flex layout, b.X should be 80 (got {itemB.ContentRect.X})");
        }

        // [CSS-FLEXBOX §4.1] Abspos child does not contribute to flex container intrinsic size
        [Fact]
        public void AbsposChild_DoesNotAffectContainerSize()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='flex' style='display:flex;position:relative;width:200px'>
                    <div style='width:60px;height:30px'></div>
                    <div style='position:absolute;width:500px;height:500px'></div>
                </div></body>");
            var flex = LayoutTestHelper.FindById(root, "flex");
            Assert.NotNull(flex);
            _output.WriteLine($"flex.Height={flex!.ContentRect.Height}");
            Assert.True(System.Math.Abs(flex.ContentRect.Height - 30) < 2,
                $"Abspos child doesn't inflate container height (got {flex.ContentRect.Height})");
        }

        // [CSS-FLEXBOX §4.1] Abspos child uses flex container as containing block when position:relative
        [Fact]
        public void AbsposChild_UsesFlexContainerAsContainingBlock()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;position:relative;width:200px;height:150px'>
                    <div id='abs' style='position:absolute;top:10px;left:20px;width:40px;height:40px'></div>
                </div></body>");
            var absChild = LayoutTestHelper.FindById(root, "abs");
            Assert.NotNull(absChild);
            _output.WriteLine($"abs.X={absChild!.ContentRect.X}, abs.Y={absChild.ContentRect.Y}");
            Assert.True(System.Math.Abs(absChild.ContentRect.X - 20) < 2,
                $"Abspos left relative to flex container (got {absChild.ContentRect.X})");
            Assert.True(System.Math.Abs(absChild.ContentRect.Y - 10) < 2,
                $"Abspos top relative to flex container (got {absChild.ContentRect.Y})");
        }

        // [CSS-FLEXBOX §4.1] Abspos flex child with all four insets
        [Fact]
        public void AbsposChild_WithTopLeftInsets()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;position:relative;width:300px;height:200px'>
                    <div id='abs' style='position:absolute;top:15px;left:25px;width:60px;height:40px'></div>
                </div></body>");
            var absChild = LayoutTestHelper.FindById(root, "abs");
            Assert.NotNull(absChild);
            Assert.True(System.Math.Abs(absChild!.ContentRect.X - 25) < 2,
                $"left:25px (got {absChild.ContentRect.X})");
            Assert.True(System.Math.Abs(absChild!.ContentRect.Y - 15) < 2,
                $"top:15px (got {absChild.ContentRect.Y})");
        }

        // [CSS-FLEXBOX §4.1] Abspos flex child with right/bottom insets
        [Fact]
        public void AbsposChild_WithRightBottomInsets()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;position:relative;width:300px;height:200px'>
                    <div id='abs' style='position:absolute;right:10px;bottom:20px;width:50px;height:30px'></div>
                </div></body>");
            var absChild = LayoutTestHelper.FindById(root, "abs");
            Assert.NotNull(absChild);
            _output.WriteLine($"abs.X={absChild!.ContentRect.X}, abs.Y={absChild.ContentRect.Y}");
            Assert.True(System.Math.Abs(absChild.ContentRect.X - 240) < 2,
                $"right:10px → X=240 (got {absChild.ContentRect.X})");
            Assert.True(System.Math.Abs(absChild.ContentRect.Y - 150) < 2,
                $"bottom:20px → Y=150 (got {absChild.ContentRect.Y})");
        }

        // [CSS-FLEXBOX §4.1] Abspos child with margin:auto centers in flex container
        [Fact]
        public void AbsposChild_MarginAuto_CentersInContainer()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;position:relative;width:200px;height:200px'>
                    <div id='abs' style='position:absolute;top:0;right:0;bottom:0;left:0;margin:auto;width:80px;height:60px'></div>
                </div></body>");
            var absChild = LayoutTestHelper.FindById(root, "abs");
            Assert.NotNull(absChild);
            _output.WriteLine($"abs.X={absChild!.ContentRect.X}, abs.Y={absChild.ContentRect.Y}");
            Assert.True(System.Math.Abs(absChild.ContentRect.X - 60) < 2,
                $"Centered horizontally: X=60 (got {absChild.ContentRect.X})");
            Assert.True(System.Math.Abs(absChild.ContentRect.Y - 70) < 2,
                $"Centered vertically: Y=70 (got {absChild.ContentRect.Y})");
        }

        // [CSS-FLEXBOX §4.1] Abspos child with percent width resolves against flex container
        [Fact]
        public void AbsposChild_PercentWidth_ResolvesAgainstContainer()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;position:relative;width:200px;height:100px'>
                    <div id='abs' style='position:absolute;width:50%;height:30px'></div>
                </div></body>");
            var absChild = LayoutTestHelper.FindById(root, "abs");
            Assert.NotNull(absChild);
            _output.WriteLine($"abs.Width={absChild!.ContentRect.Width}");
            Assert.True(System.Math.Abs(absChild.ContentRect.Width - 100) < 2,
                $"50% of 200px = 100px (got {absChild.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §4.1] Abspos child with percent height resolves against flex container
        [Fact]
        public void AbsposChild_PercentHeight_ResolvesAgainstContainer()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;position:relative;width:200px;height:160px'>
                    <div id='abs' style='position:absolute;width:40px;height:25%'></div>
                </div></body>");
            var absChild = LayoutTestHelper.FindById(root, "abs");
            Assert.NotNull(absChild);
            _output.WriteLine($"abs.Height={absChild!.ContentRect.Height}");
            Assert.True(System.Math.Abs(absChild.ContentRect.Height - 40) < 2,
                $"25% of 160px = 40px (got {absChild.ContentRect.Height})");
        }

        // [CSS-FLEXBOX §4.1] Multiple abspos children in flex container
        [Fact]
        public void MultipleAbsposChildren_IndependentPositioning()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;position:relative;width:300px;height:200px'>
                    <div id='a' style='position:absolute;top:0;left:0;width:40px;height:40px'></div>
                    <div id='b' style='position:absolute;top:0;right:0;width:40px;height:40px'></div>
                    <div id='c' style='position:absolute;bottom:0;left:0;width:40px;height:40px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a");
            var itemB = LayoutTestHelper.FindById(root, "b");
            var itemC = LayoutTestHelper.FindById(root, "c");
            Assert.NotNull(itemA);
            Assert.NotNull(itemB);
            Assert.NotNull(itemC);
            Assert.True(System.Math.Abs(itemA!.ContentRect.X - 0) < 2, $"a at top-left X (got {itemA.ContentRect.X})");
            Assert.True(System.Math.Abs(itemA.ContentRect.Y - 0) < 2, $"a at top-left Y (got {itemA.ContentRect.Y})");
            Assert.True(System.Math.Abs(itemB!.ContentRect.X - 260) < 2, $"b at top-right X (got {itemB.ContentRect.X})");
            Assert.True(System.Math.Abs(itemC!.ContentRect.Y - 160) < 2, $"c at bottom-left Y (got {itemC.ContentRect.Y})");
        }

        // [CSS-FLEXBOX §4.1] Abspos with z-index stacking in flex container
        [Fact]
        public void AbsposChild_ZIndex_ParsedCorrectly()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;position:relative;width:200px;height:100px'>
                    <div id='behind' style='position:absolute;z-index:1;width:60px;height:60px'></div>
                    <div id='front' style='position:absolute;z-index:10;width:60px;height:60px'></div>
                </div></body>");
            var behind = LayoutTestHelper.FindById(root, "behind");
            var front = LayoutTestHelper.FindById(root, "front");
            Assert.NotNull(behind);
            Assert.NotNull(front);
            var behindStyle = (behind!.StyledNode as Rend.Style.StyledElement)!;
            var frontStyle = (front!.StyledNode as Rend.Style.StyledElement)!;
            Assert.Equal(1, behindStyle.Style.ZIndex);
            Assert.Equal(10, frontStyle.Style.ZIndex);
        }

        // [CSS-FLEXBOX §5.4] order property does not affect abspos children
        [Fact]
        public void OrderProperty_DoesNotAffectAbsposPosition()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;position:relative;width:300px;height:100px'>
                    <div id='normal' style='order:5;width:80px;height:40px'></div>
                    <div id='abs' style='position:absolute;order:1;top:10px;left:10px;width:40px;height:40px'></div>
                </div></body>");
            var absChild = LayoutTestHelper.FindById(root, "abs");
            Assert.NotNull(absChild);
            _output.WriteLine($"abs.X={absChild!.ContentRect.X}, abs.Y={absChild.ContentRect.Y}");
            Assert.True(System.Math.Abs(absChild.ContentRect.X - 10) < 2,
                $"Order doesn't move abspos: X=10 (got {absChild.ContentRect.X})");
            Assert.True(System.Math.Abs(absChild.ContentRect.Y - 10) < 2,
                $"Order doesn't move abspos: Y=10 (got {absChild.ContentRect.Y})");
        }

        // [CSS-FLEXBOX §4.1] Abspos in column flex direction
        [Fact]
        public void AbsposChild_InColumnFlex_PositionedCorrectly()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;position:relative;width:200px;height:300px'>
                    <div id='item' style='height:50px'></div>
                    <div id='abs' style='position:absolute;bottom:10px;right:10px;width:40px;height:40px'></div>
                </div></body>");
            var absChild = LayoutTestHelper.FindById(root, "abs");
            Assert.NotNull(absChild);
            _output.WriteLine($"abs.X={absChild!.ContentRect.X}, abs.Y={absChild.ContentRect.Y}");
            Assert.True(System.Math.Abs(absChild.ContentRect.X - 150) < 2,
                $"right:10px in 200px container → X=150 (got {absChild.ContentRect.X})");
            Assert.True(System.Math.Abs(absChild.ContentRect.Y - 250) < 2,
                $"bottom:10px in 300px container → Y=250 (got {absChild.ContentRect.Y})");
        }

        // [CSS-FLEXBOX §4.1] Abspos child in column flex does not affect stacking of normal items
        [Fact]
        public void AbsposChild_InColumnFlex_DoesNotAffectSiblings()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;position:relative;width:200px'>
                    <div id='a' style='height:40px'></div>
                    <div style='position:absolute;width:100px;height:100px'></div>
                    <div id='b' style='height:40px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a");
            var itemB = LayoutTestHelper.FindById(root, "b");
            Assert.NotNull(itemA);
            Assert.NotNull(itemB);
            _output.WriteLine($"a.Y={itemA!.ContentRect.Y}, b.Y={itemB!.ContentRect.Y}");
            Assert.True(System.Math.Abs(itemB.ContentRect.Y - 40) < 2,
                $"Abspos skipped: b.Y should be 40 (got {itemB.ContentRect.Y})");
        }

        // [CSS-FLEXBOX §4.1] Abspos child in wrapping flex container
        [Fact]
        public void AbsposChild_InWrapFlex_PositionedCorrectly()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;position:relative;width:200px;height:200px'>
                    <div style='width:100px;height:50px'></div>
                    <div style='width:100px;height:50px'></div>
                    <div id='abs' style='position:absolute;top:5px;left:5px;width:30px;height:30px'></div>
                </div></body>");
            var absChild = LayoutTestHelper.FindById(root, "abs");
            Assert.NotNull(absChild);
            Assert.True(System.Math.Abs(absChild!.ContentRect.X - 5) < 2,
                $"Abspos at left:5 in wrap flex (got {absChild.ContentRect.X})");
            Assert.True(System.Math.Abs(absChild.ContentRect.Y - 5) < 2,
                $"Abspos at top:5 in wrap flex (got {absChild.ContentRect.Y})");
        }

        // [CSS2 §9.7] Relative flex item serves as containing block for abspos grandchild
        [Fact]
        public void RelativeFlexItem_ContainingBlockForAbsposGrandchild()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div style='position:relative;width:150px;height:100px'>
                        <div id='abs' style='position:absolute;top:10px;left:20px;width:40px;height:30px'></div>
                    </div>
                </div></body>");
            var absChild = LayoutTestHelper.FindById(root, "abs");
            Assert.NotNull(absChild);
            _output.WriteLine($"abs.X={absChild!.ContentRect.X}, abs.Y={absChild.ContentRect.Y}");
            Assert.True(System.Math.Abs(absChild.ContentRect.X - 20) < 2,
                $"Abspos left relative to flex item (got {absChild.ContentRect.X})");
            Assert.True(System.Math.Abs(absChild.ContentRect.Y - 10) < 2,
                $"Abspos top relative to flex item (got {absChild.ContentRect.Y})");
        }

        // [CSS2 §9.7] Relative flex item with bottom/right abspos grandchild
        [Fact]
        public void RelativeFlexItem_AbsposGrandchild_RightBottom()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div style='position:relative;width:200px;height:120px'>
                        <div id='abs' style='position:absolute;right:15px;bottom:10px;width:50px;height:30px'></div>
                    </div>
                </div></body>");
            var absChild = LayoutTestHelper.FindById(root, "abs");
            Assert.NotNull(absChild);
            _output.WriteLine($"abs.X={absChild!.ContentRect.X}, abs.Y={absChild.ContentRect.Y}");
            Assert.True(System.Math.Abs(absChild.ContentRect.X - 135) < 2,
                $"right:15px in 200px item → X=135 (got {absChild.ContentRect.X})");
            Assert.True(System.Math.Abs(absChild.ContentRect.Y - 80) < 2,
                $"bottom:10px in 120px item → Y=80 (got {absChild.ContentRect.Y})");
        }

        // [CSS-FLEXBOX §4.1] Abspos child with left+right auto width stretches
        [Fact]
        public void AbsposChild_LeftRight_AutoWidth_Stretches()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;position:relative;width:300px;height:100px'>
                    <div id='abs' style='position:absolute;left:20px;right:30px;height:40px'></div>
                </div></body>");
            var absChild = LayoutTestHelper.FindById(root, "abs");
            Assert.NotNull(absChild);
            _output.WriteLine($"abs.Width={absChild!.ContentRect.Width}");
            Assert.True(System.Math.Abs(absChild.ContentRect.Width - 250) < 2,
                $"left:20 + right:30 in 300px → width=250 (got {absChild.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §4.1] Abspos child with top+bottom auto height stretches
        [Fact]
        public void AbsposChild_TopBottom_AutoHeight_Stretches()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;position:relative;width:200px;height:200px'>
                    <div id='abs' style='position:absolute;top:20px;bottom:40px;width:50px'></div>
                </div></body>");
            var absChild = LayoutTestHelper.FindById(root, "abs");
            Assert.NotNull(absChild);
            _output.WriteLine($"abs.Height={absChild!.ContentRect.Height}");
            Assert.True(System.Math.Abs(absChild.ContentRect.Height - 140) < 2,
                $"top:20 + bottom:40 in 200px → height=140 (got {absChild.ContentRect.Height})");
        }

        // [CSS-FLEXBOX §4.1] Abspos children mixed with normal flex items, container auto height
        [Fact]
        public void AbsposChildren_DoNotAffectAutoHeightContainer()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='flex' style='display:flex;position:relative;width:200px'>
                    <div style='width:50px;height:45px'></div>
                    <div style='position:absolute;width:80px;height:200px'></div>
                    <div style='width:50px;height:45px'></div>
                </div></body>");
            var flex = LayoutTestHelper.FindById(root, "flex");
            Assert.NotNull(flex);
            _output.WriteLine($"flex.Height={flex!.ContentRect.Height}");
            Assert.True(System.Math.Abs(flex.ContentRect.Height - 45) < 2,
                $"Auto height based only on normal items: 45px (got {flex.ContentRect.Height})");
        }

        // [CSS-FLEXBOX §4.1] Abspos flex child with margin does not collapse
        [Fact]
        public void AbsposChild_WithMargin_OffsetsFromInsets()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;position:relative;width:200px;height:200px'>
                    <div id='abs' style='position:absolute;top:0;left:0;margin:15px;width:50px;height:50px'></div>
                </div></body>");
            var absChild = LayoutTestHelper.FindById(root, "abs");
            Assert.NotNull(absChild);
            _output.WriteLine($"abs.X={absChild!.ContentRect.X}, abs.Y={absChild.ContentRect.Y}");
            Assert.True(System.Math.Abs(absChild.ContentRect.X - 15) < 2,
                $"margin:15 + left:0 → X=15 (got {absChild.ContentRect.X})");
            Assert.True(System.Math.Abs(absChild.ContentRect.Y - 15) < 2,
                $"margin:15 + top:0 → Y=15 (got {absChild.ContentRect.Y})");
        }

        // [CSS-FLEXBOX §4.1] Without position:relative on flex, abspos uses nearest positioned ancestor
        [Fact]
        public void AbsposChild_FlexWithoutRelative_UsesOuterContainingBlock()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='outer' style='position:relative;width:400px;height:300px'>
                    <div style='display:flex;width:200px;height:100px;margin-left:50px;margin-top:30px'>
                        <div id='abs' style='position:absolute;top:5px;left:5px;width:30px;height:30px'></div>
                    </div>
                </div></body>");
            var outer = LayoutTestHelper.FindById(root, "outer");
            var absChild = LayoutTestHelper.FindById(root, "abs");
            Assert.NotNull(outer);
            Assert.NotNull(absChild);
            float outerX = outer!.ContentRect.X;
            float outerY = outer.ContentRect.Y;
            _output.WriteLine($"outer.X={outerX}, outer.Y={outerY}, abs.X={absChild!.ContentRect.X}, abs.Y={absChild.ContentRect.Y}");
            Assert.True(System.Math.Abs(absChild.ContentRect.X - (outerX + 5)) < 2,
                $"Abspos left:5 relative to outer div (got {absChild.ContentRect.X}, expected {outerX + 5})");
            Assert.True(System.Math.Abs(absChild.ContentRect.Y - (outerY + 5)) < 2,
                $"Abspos top:5 relative to outer div (got {absChild.ContentRect.Y}, expected {outerY + 5})");
        }

        // [CSS-FLEXBOX §4.1] Abspos in column flex with gap — gap does not apply to abspos
        [Fact]
        public void AbsposChild_InColumnFlexWithGap_GapNotApplied()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;gap:20px;position:relative;width:200px'>
                    <div id='a' style='height:30px'></div>
                    <div style='position:absolute;top:0;left:0;width:20px;height:20px'></div>
                    <div id='b' style='height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a");
            var itemB = LayoutTestHelper.FindById(root, "b");
            Assert.NotNull(itemA);
            Assert.NotNull(itemB);
            _output.WriteLine($"a.Y={itemA!.ContentRect.Y}, b.Y={itemB!.ContentRect.Y}");
            Assert.True(System.Math.Abs(itemB.ContentRect.Y - 50) < 2,
                $"Gap between normal items only: b.Y = 30+20 = 50 (got {itemB.ContentRect.Y})");
        }

        // [CSS-FLEXBOX §4.1] Abspos in row flex with gap — abspos doesn't consume gap
        [Fact]
        public void AbsposChild_InRowFlexWithGap_GapNotConsumed()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;gap:10px;position:relative;width:300px'>
                    <div id='a' style='width:50px;height:30px'></div>
                    <div style='position:absolute;width:20px;height:20px'></div>
                    <div id='b' style='width:50px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a");
            var itemB = LayoutTestHelper.FindById(root, "b");
            Assert.NotNull(itemA);
            Assert.NotNull(itemB);
            _output.WriteLine($"a.X={itemA!.ContentRect.X}, b.X={itemB!.ContentRect.X}");
            Assert.True(System.Math.Abs(itemB.ContentRect.X - 60) < 2,
                $"Gap between normal items: b.X = 50+10 = 60 (got {itemB.ContentRect.X})");
        }
    }
}
