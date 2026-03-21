using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    /// <summary>
    /// Flex sizing edge cases: basis resolution, shrink distribution,
    /// min/max constraints, and cross-axis sizing.
    /// </summary>
    public class WptFlexSizingTests
    {
        private readonly ITestOutputHelper _output;
        public WptFlexSizingTests(ITestOutputHelper output) { _output = output; }

        // [CSS-FLEXBOX §9.2] flex-basis: auto falls through to width
        [Fact]
        public void FlexBasisAuto_UsesWidth()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div id='t' style='flex:0 0 auto;width:120px;height:30px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Width - 120) < 2);
        }

        // [CSS-FLEXBOX §9.2] flex-basis: 0 ignores width
        [Fact]
        public void FlexBasis0_IgnoresWidth()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div id='t' style='flex:0 0 0px;width:120px;height:30px'></div>
                </div></body>");
            Assert.True(LayoutTestHelper.FindById(r, "t")!.ContentRect.Width < 2);
        }

        // [CSS-FLEXBOX §9.7] flex-grow with 4 items: 1:1:2:2 ratio
        [Fact]
        public void FlexGrow_1122_Ratio()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:600px'>
                    <div id='a' style='flex:1 0 0px;height:30px'></div>
                    <div id='b' style='flex:1 0 0px;height:30px'></div>
                    <div id='c' style='flex:2 0 0px;height:30px'></div>
                    <div id='d' style='flex:2 0 0px;height:30px'></div>
                </div></body>");
            // Total grow = 6. a,b = 100 each. c,d = 200 each.
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "a")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "c")!.ContentRect.Width - 200) < 2);
        }

        // [CSS-FLEXBOX §9.7] flex-shrink: equal basis, unequal shrink
        [Fact]
        public void FlexShrink_UnequalFactors()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:200px'>
                    <div id='a' style='flex:0 1 150px;height:30px'></div>
                    <div id='b' style='flex:0 3 150px;height:30px'></div>
                </div></body>");
            // Overflow=100. Scaled: a=1*150=150, b=3*150=450. Total=600.
            // a shrinks 100*150/600=25→125. b shrinks 100*450/600=75→75.
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "a")!.ContentRect.Width - 125) < 3);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "b")!.ContentRect.Width - 75) < 3);
        }

        // [CSS-FLEXBOX §4.5] flex item min-height:0 allows column shrink
        [Fact]
        public void ColumnFlex_MinHeight0_Shrinks()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;height:100px;width:100px'>
                    <div id='t' style='flex:1 1 200px;min-height:0'></div>
                </div></body>");
            Assert.True(LayoutTestHelper.FindById(r, "t")!.ContentRect.Height <= 101);
        }

        // [CSS-FLEXBOX §9.4] flex container auto height = tallest line for row
        [Fact]
        public void FlexRow_AutoHeight()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='flex' style='display:flex;width:200px'>
                    <div style='width:50px;height:40px'></div>
                    <div style='width:50px;height:80px'></div>
                    <div style='width:50px;height:60px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "flex")!.ContentRect.Height - 80) < 2);
        }

        // [CSS-FLEXBOX §9.4] flex container auto height = sum of items for column
        [Fact]
        public void FlexColumn_AutoHeight()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='flex' style='display:flex;flex-direction:column;width:200px'>
                    <div style='height:40px'></div>
                    <div style='height:60px'></div>
                    <div style='height:30px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "flex")!.ContentRect.Height - 130) < 2);
        }

        // [CSS-FLEXBOX §9.4] flex column auto height with gap
        [Fact]
        public void FlexColumn_AutoHeight_WithGap()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='flex' style='display:flex;flex-direction:column;gap:10px;width:200px'>
                    <div style='height:30px'></div>
                    <div style='height:30px'></div>
                    <div style='height:30px'></div>
                </div></body>");
            // 30*3 + 10*2 = 110
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "flex")!.ContentRect.Height - 110) < 2);
        }

        // [CSS-FLEXBOX §7.1] flex: initial = 0 1 auto
        [Fact]
        public void Flex_Initial_NoGrow()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div id='t' style='flex:initial;width:80px;height:30px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Width - 80) < 2);
        }

        // [CSS-FLEXBOX §7.1] flex: auto = 1 1 auto (grows to fill)
        [Fact]
        public void Flex_Auto_Grows()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div id='t' style='flex:auto;width:80px;height:30px'></div>
                </div></body>");
            Assert.True(LayoutTestHelper.FindById(r, "t")!.ContentRect.Width >= 299);
        }

        // [CSS-FLEXBOX §7.1] flex: none = 0 0 auto (no flex at all)
        [Fact]
        public void Flex_None_NoFlex()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div id='t' style='flex:none;width:80px;height:30px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Width - 80) < 2);
        }

        // [CSS-FLEXBOX §9] flex items with max-width constraint
        [Fact]
        public void FlexGrow_MaxWidth_Clamps()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div id='a' style='flex:1;max-width:80px;height:30px'></div>
                    <div id='b' style='flex:1;height:30px'></div>
                </div></body>");
            Assert.True(LayoutTestHelper.FindById(r, "a")!.ContentRect.Width <= 81);
            Assert.True(LayoutTestHelper.FindById(r, "b")!.ContentRect.Width >= 219);
        }

        // [CSS-FLEXBOX §9] flex items with min-width constraint
        [Fact]
        public void FlexShrink_MinWidth_Prevents()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:200px'>
                    <div id='a' style='flex:0 1 150px;min-width:120px;height:30px'></div>
                    <div id='b' style='flex:0 1 150px;height:30px'></div>
                </div></body>");
            Assert.True(LayoutTestHelper.FindById(r, "a")!.ContentRect.Width >= 119);
        }

        // [CSS-FLEXBOX §8.1] main-axis auto margin pushes to end
        [Fact]
        public void MainAxisAutoMargin_PushesToEnd()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div style='width:50px;height:30px'></div>
                    <div id='t' style='margin-left:auto;width:50px;height:30px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.X - 250) < 2);
        }

        // [CSS-FLEXBOX §8.1] cross-axis auto margins center
        [Fact]
        public void CrossAxisAutoMargins_Center()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;height:100px;width:200px'>
                    <div id='t' style='margin-top:auto;margin-bottom:auto;width:50px;height:30px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Y - 35) < 2);
        }

        // [CSS-FLEXBOX §5.4] order doesn't affect non-flex items
        [Fact]
        public void Order_OnlyAffectsFlex()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:200px'>
                    <div id='a' style='order:2;width:50px;height:30px'></div>
                    <div id='b' style='order:1;width:50px;height:30px'></div>
                </div></body>");
            // b (order:1) comes before a (order:2) visually
            Assert.True(LayoutTestHelper.FindById(r, "b")!.ContentRect.X < LayoutTestHelper.FindById(r, "a")!.ContentRect.X);
        }
    }
}
