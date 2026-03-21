using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    /// <summary>
    /// Real layout verification tests that check actual computed positions and sizes.
    /// Each test reproduces a specific WPT test pattern and verifies the exact output.
    /// </summary>
    public class WptLayoutVerificationTests
    {
        private readonly ITestOutputHelper _output;
        public WptLayoutVerificationTests(ITestOutputHelper output) { _output = output; }

        // WPT css-flexbox: flex items with percentage padding resolve against container width
        [Fact]
        public void FlexItem_PercentPadding_ResolvesAgainstContainerWidth()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:200px'>
                    <div id='t' style='padding:10%;width:50px;height:20px'></div>
                </div></body>");
            var t = LayoutTestHelper.FindById(r, "t")!;
            _output.WriteLine($"padding: T={t.PaddingTop} R={t.PaddingRight} B={t.PaddingBottom} L={t.PaddingLeft}");
            // 10% of 200px container = 20px on each side
            Assert.True(System.Math.Abs(t.PaddingTop - 20) < 2, $"padding-top 10% of 200 = 20 (got {t.PaddingTop})");
            Assert.True(System.Math.Abs(t.PaddingLeft - 20) < 2, $"padding-left 10% of 200 = 20 (got {t.PaddingLeft})");
        }

        // WPT css-flexbox: flex-grow with unequal basis
        [Fact]
        public void FlexGrow_WithUnequalBasis()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div id='a' style='flex-grow:1;flex-basis:100px;height:30px'></div>
                    <div id='b' style='flex-grow:1;flex-basis:50px;height:30px'></div>
                </div></body>");
            var a = LayoutTestHelper.FindById(r, "a")!;
            var b = LayoutTestHelper.FindById(r, "b")!;
            _output.WriteLine($"a.w={a.ContentRect.Width} b.w={b.ContentRect.Width}");
            // Free space = 300 - 100 - 50 = 150. Split equally: +75 each.
            // a = 175, b = 125
            Assert.True(System.Math.Abs(a.ContentRect.Width - 175) < 2, $"a = 175 (got {a.ContentRect.Width})");
            Assert.True(System.Math.Abs(b.ContentRect.Width - 125) < 2, $"b = 125 (got {b.ContentRect.Width})");
        }

        // WPT css-flexbox: weighted flex-shrink
        [Fact]
        public void FlexShrink_Weighted()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:200px'>
                    <div id='a' style='flex-shrink:1;width:200px;height:30px'></div>
                    <div id='b' style='flex-shrink:3;width:200px;height:30px'></div>
                </div></body>");
            var a = LayoutTestHelper.FindById(r, "a")!;
            var b = LayoutTestHelper.FindById(r, "b")!;
            _output.WriteLine($"a.w={a.ContentRect.Width} b.w={b.ContentRect.Width}");
            // Overflow = 200. shrink ratio: a=1*200=200, b=3*200=600. Total=800.
            // a shrinks by 200*(200/800)=50 → 150. b shrinks by 200*(600/800)=150 → 50.
            Assert.True(System.Math.Abs(a.ContentRect.Width - 150) < 3, $"a = 150 (got {a.ContentRect.Width})");
            Assert.True(System.Math.Abs(b.ContentRect.Width - 50) < 3, $"b = 50 (got {b.ContentRect.Width})");
        }

        // WPT css-grid: grid item with margin auto centers in cell
        [Fact]
        public void GridItem_MarginAuto_Centers()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:200px;grid-template-rows:100px;width:200px'>
                    <div id='t' style='width:80px;height:40px;margin:auto'></div>
                </div></body>");
            var t = LayoutTestHelper.FindById(r, "t")!;
            _output.WriteLine($"t: ({t.ContentRect.X},{t.ContentRect.Y}) {t.ContentRect.Width}x{t.ContentRect.Height}");
            // margin:auto in grid cell centers: X=(200-80)/2=60, Y=(100-40)/2=30
            Assert.True(System.Math.Abs(t.ContentRect.X - 60) < 2, $"Centered X (got {t.ContentRect.X})");
            Assert.True(System.Math.Abs(t.ContentRect.Y - 30) < 2, $"Centered Y (got {t.ContentRect.Y})");
        }

        // WPT css-sizing: block with calc(100% - 40px) inside 200px parent
        [Fact]
        public void CalcPercentMinusPx_ResolvesCorrectly()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:200px'>
                    <div id='t' style='width:calc(100% - 40px);height:20px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Width - 160) < 2);
        }

        // WPT css-sizing: min(50%, 100px) in 300px container = 100px
        [Fact]
        public void Min_PercentAndPx_PicksSmaller()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:300px'>
                    <div id='t' style='width:min(50%, 100px);height:20px'></div>
                </div></body>");
            // min(150, 100) = 100
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Width - 100) < 2);
        }

        // WPT css-position: abspos with all 4 insets and margin:auto = centered
        [Fact]
        public void AbsPos_Inset_MarginAuto_Centers()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:300px;height:300px'>
                    <div id='t' style='position:absolute;inset:0;margin:auto;width:100px;height:100px'></div>
                </div></body>");
            var t = LayoutTestHelper.FindById(r, "t")!;
            _output.WriteLine($"t: ({t.ContentRect.X},{t.ContentRect.Y})");
            Assert.True(System.Math.Abs(t.ContentRect.X - 100) < 2, $"Centered X (got {t.ContentRect.X})");
            Assert.True(System.Math.Abs(t.ContentRect.Y - 100) < 2, $"Centered Y (got {t.ContentRect.Y})");
        }

        // WPT css-flexbox: flex container auto height = sum of items
        [Fact]
        public void FlexColumn_AutoHeight_SumsItems()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='flex' style='display:flex;flex-direction:column;width:200px'>
                    <div style='height:40px'></div>
                    <div style='height:60px'></div>
                    <div style='height:30px'></div>
                </div></body>");
            var flex = LayoutTestHelper.FindById(r, "flex")!;
            _output.WriteLine($"flex.h={flex.ContentRect.Height}");
            Assert.True(System.Math.Abs(flex.ContentRect.Height - 130) < 2, $"Auto height = 40+60+30 = 130 (got {flex.ContentRect.Height})");
        }

        // WPT css-flexbox: flex row auto height = tallest item
        [Fact]
        public void FlexRow_AutoHeight_TallestItem()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='flex' style='display:flex;width:200px'>
                    <div style='width:50px;height:30px'></div>
                    <div style='width:50px;height:80px'></div>
                    <div style='width:50px;height:50px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "flex")!.ContentRect.Height - 80) < 2);
        }

        // WPT css-grid: 3 column layout with explicit heights
        [Fact]
        public void Grid_3Col_ExactPositions()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px 100px 100px;width:300px'>
                    <div id='a' style='height:50px'></div>
                    <div id='b' style='height:50px'></div>
                    <div id='c' style='height:50px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "a")!.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "b")!.ContentRect.X - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "c")!.ContentRect.X - 200) < 2);
        }

        // WPT css-grid: grid with gap, verify exact positions
        [Fact]
        public void Grid_WithGap_ExactPositions()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:90px 90px;gap:20px;width:200px'>
                    <div id='a' style='height:40px'></div>
                    <div id='b' style='height:40px'></div>
                    <div id='c' style='height:40px'></div>
                    <div id='d' style='height:40px'></div>
                </div></body>");
            var a = LayoutTestHelper.FindById(r, "a")!;
            var b = LayoutTestHelper.FindById(r, "b")!;
            var c = LayoutTestHelper.FindById(r, "c")!;
            float colGap = b.ContentRect.X - (a.ContentRect.X + a.ContentRect.Width);
            float rowGap = c.ContentRect.Y - (a.ContentRect.Y + a.ContentRect.Height);
            Assert.True(System.Math.Abs(colGap - 20) < 2, $"col gap = 20 (got {colGap})");
            Assert.True(System.Math.Abs(rowGap - 20) < 2, $"row gap = 20 (got {rowGap})");
        }

        // WPT css-tables: percentage width table
        [Fact]
        public void Table_PercentWidth_ResolvesAgainstParent()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px'>
                    <table id='t' style='width:50%;border-collapse:collapse'>
                        <tr><td style='height:30px'>A</td></tr>
                    </table>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Width - 200) < 2);
        }

        // WPT css-position: relative positioning preserves flow position for siblings
        [Fact]
        public void Relative_SiblingNotAffected()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:200px'>
                    <div style='position:relative;top:100px;left:100px;height:30px'></div>
                    <div id='sib' style='height:30px'></div>
                </div></body>");
            // Sibling should be at Y=30 (normal flow), not Y=130
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "sib")!.ContentRect.Y - 30) < 2,
                $"Sibling Y={LayoutTestHelper.FindById(r, "sib")!.ContentRect.Y}");
        }

        // WPT css-sizing: auto width block subtracts all horizontal spacing
        [Fact]
        public void AutoWidth_SubtractsAllSpacing()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:300px'>
                    <div id='t' style='margin:0 15px;padding:0 10px;border:5px solid;height:20px'></div>
                </div></body>");
            // width = 300 - 15*2(margin) - 10*2(padding) - 5*2(border) = 240
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Width - 240) < 2);
        }

        // WPT css-multicol: 2 columns with 4 equal blocks should give 2 per column
        [Fact]
        public void Multicol_EvenDistribution()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='mc' style='column-count:2;column-gap:0;width:200px'>
                    <div style='height:30px'></div>
                    <div style='height:30px'></div>
                    <div style='height:30px'></div>
                    <div style='height:30px'></div>
                </div></body>");
            var mc = LayoutTestHelper.FindById(r, "mc")!;
            _output.WriteLine($"mc.h={mc.ContentRect.Height}");
            // 4 blocks of 30px = 120px, 2 columns → 60px each
            Assert.True(System.Math.Abs(mc.ContentRect.Height - 60) < 2, $"Balanced columns (got {mc.ContentRect.Height})");
        }

        // WPT css-flexbox: flex:1 with 3 items distributes equally
        [Fact]
        public void Flex1_ThreeItems_EqualWidth()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div id='a' style='flex:1;height:30px'></div>
                    <div id='b' style='flex:1;height:30px'></div>
                    <div id='c' style='flex:1;height:30px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "a")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "b")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "c")!.ContentRect.Width - 100) < 2);
        }

        // WPT css-flexbox: justify-content:space-between with 3 items
        [Fact]
        public void SpaceBetween_ThreeItems_ExactPositions()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;justify-content:space-between;width:200px'>
                    <div id='a' style='width:20px;height:30px'></div>
                    <div id='b' style='width:20px;height:30px'></div>
                    <div id='c' style='width:20px;height:30px'></div>
                </div></body>");
            // Free = 200 - 60 = 140. 2 gaps of 70.
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "a")!.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "b")!.ContentRect.X - 90) < 2, $"b.X={LayoutTestHelper.FindById(r, "b")!.ContentRect.X}");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "c")!.ContentRect.X - 180) < 2);
        }

        // WPT css-position: abspos percentage height resolves against CB
        [Fact]
        public void AbsPos_PercentHeight_AgainstCB()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:200px'>
                    <div id='abs' style='position:absolute;width:50px;height:50%'></div>
                    <div style='height:300px'></div>
                </div></body>");
            var abs = LayoutTestHelper.FindById(r, "abs")!;
            _output.WriteLine($"abs.h={abs.ContentRect.Height}");
            Assert.True(System.Math.Abs(abs.ContentRect.Height - 150) < 2, $"50% of 300 = 150 (got {abs.ContentRect.Height})");
        }

        // WPT css-flexbox: flex-basis: 0 makes all items grow from zero
        [Fact]
        public void FlexBasis0_GrowFromZero()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div id='a' style='flex:1 0 0px;height:30px'></div>
                    <div id='b' style='flex:3 0 0px;height:30px'></div>
                </div></body>");
            // basis=0, grow 1:3. a=75, b=225.
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "a")!.ContentRect.Width - 75) < 2, $"a.w={LayoutTestHelper.FindById(r, "a")!.ContentRect.Width}");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "b")!.ContentRect.Width - 225) < 2, $"b.w={LayoutTestHelper.FindById(r, "b")!.ContentRect.Width}");
        }

        // WPT css-sizing: aspect-ratio 16/9 width=320 → height=180
        [Fact]
        public void AspectRatio_16by9_Exact()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='width:320px;aspect-ratio:16/9'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Height - 180) < 2);
        }

        // WPT css-sizing: aspect-ratio with max-height constraint
        [Fact]
        public void AspectRatio_MaxHeight_Clamps()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='width:200px;aspect-ratio:1/1;max-height:100px'></div></body>");
            Assert.True(LayoutTestHelper.FindById(r, "t")!.ContentRect.Height <= 101, $"max-height clamps (got {LayoutTestHelper.FindById(r, "t")!.ContentRect.Height})");
        }

        // WPT css-grid: auto-fill with minmax
        [Fact]
        public void Grid_AutoFill_Minmax()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:repeat(auto-fill, minmax(80px, 1fr));width:250px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                    <div id='c' style='height:20px'></div>
                </div></body>");
            // 250/80 = 3 columns. Each = 250/3 ≈ 83px
            var a = LayoutTestHelper.FindById(r, "a")!;
            _output.WriteLine($"a.w={a.ContentRect.Width}");
            Assert.True(a.ContentRect.Width >= 80, $"auto-fill minmax columns >= 80 (got {a.ContentRect.Width})");
        }

        // WPT css-flexbox: margin:auto on main axis absorbs space
        [Fact]
        public void FlexItem_MainMarginAuto_PushesRight()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div style='width:50px;height:30px'></div>
                    <div id='t' style='margin-left:auto;width:50px;height:30px'></div>
                </div></body>");
            // margin-left:auto pushes item to right edge
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.X - 250) < 2);
        }

        // WPT css-flexbox: cross-axis margin:auto centers
        [Fact]
        public void FlexItem_CrossMarginAuto_Centers()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;height:100px;width:200px'>
                    <div id='t' style='margin:auto 0;width:50px;height:30px'></div>
                </div></body>");
            var t = LayoutTestHelper.FindById(r, "t")!;
            _output.WriteLine($"t.Y={t.ContentRect.Y}");
            // margin:auto 0 → vertical auto margins center: Y = (100-30)/2 = 35
            Assert.True(System.Math.Abs(t.ContentRect.Y - 35) < 2, $"Cross auto margin centers (Y={t.ContentRect.Y})");
        }

        // WPT css-sizing: clamp width in narrow container → uses min
        [Fact]
        public void Clamp_NarrowContainer_UsesMin()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:40px'>
                    <div id='t' style='width:clamp(60px, 50%, 200px);height:20px'></div>
                </div></body>");
            // clamp(60, 20, 200) = max(60, min(20, 200)) = 60
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Width - 60) < 2);
        }
    }
}
