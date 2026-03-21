using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    /// <summary>
    /// CSS Grid percentage track sizing conformance tests.
    /// <spec>CSS-GRID §7.2 https://drafts.csswg.org/css-grid/#track-sizing</spec>
    /// </summary>
    public class WptGridPercentTrackTests
    {
        private readonly ITestOutputHelper _output;

        public WptGridPercentTrackTests(ITestOutputHelper output)
        {
            _output = output;
        }

        // [CSS-GRID §7.2] Two equal percentage columns split container evenly
        [Fact]
        public void Column_50_50_EqualSplit()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:50% 50%;width:400px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.Width - 200) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.Width - 200) < 2);
        }

        // [CSS-GRID §7.2] Unequal percentage columns 25% + 75%
        [Fact]
        public void Column_25_75_UnequalSplit()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:25% 75%;width:400px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.Width - 300) < 2);
        }

        // [CSS-GRID §7.2] Three equal percentage columns 33.33% each
        [Fact]
        public void Column_33_33_33_ThreeEqual()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:33.33% 33.33% 33.33%;width:300px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                    <div id='c' style='height:20px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "c")!.ContentRect.Width - 100) < 2);
        }

        // [CSS-GRID §7.2] Mixed percentage + fixed pixel column tracks
        [Fact]
        public void Column_PercentagePlusFixed()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:60% 120px;width:300px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.Width - 180) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.Width - 120) < 2);
        }

        // [CSS-GRID §7.2] Mixed percentage + fr column tracks
        [Fact]
        public void Column_PercentagePlusFr()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:40% 1fr;width:400px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.Width - 160) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.Width - 240) < 2);
        }

        // [CSS-GRID §7.2] Percentage row tracks with explicit container height
        [Fact]
        public void Row_PercentageWithExplicitHeight()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;grid-template-rows:40% 60%;width:100px;height:200px'>
                    <div id='a'></div>
                    <div id='b'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.Height - 80) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.Height - 120) < 2);
        }

        // [CSS-GRID §7.2] Percentage row tracks without explicit height resolve to auto
        [Fact]
        public void Row_PercentageWithoutHeight_ResolvesToAuto()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;grid-template-rows:50% 50%;width:100px'>
                    <div id='a' style='height:30px'></div>
                    <div id='b' style='height:40px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            // Without definite height, percentage rows resolve to auto (content-sized)
            Assert.True(itemA.ContentRect.Height >= 28);
            Assert.True(itemB.ContentRect.Height >= 38);
        }

        // [CSS-GRID §7.2] Percentage columns with column-gap
        [Fact]
        public void PercentageColumnsWithGap()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:50% 50%;column-gap:20px;width:400px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            // 50% of 400 = 200 each; gap = 20 between
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 200) < 2);
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 200) < 2);
            float gapBetween = itemB.ContentRect.X - (itemA.ContentRect.X + itemA.ContentRect.Width);
            Assert.True(System.Math.Abs(gapBetween - 20) < 2, $"Expected 20px gap, got {gapBetween}");
        }

        // [CSS-GRID §7.3] repeat(3, 33.33%) creates three equal percentage tracks
        [Fact]
        public void Repeat3_33Percent()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:repeat(3, 33.33%);width:300px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                    <div id='c' style='height:20px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "c")!.ContentRect.Width - 100) < 2);
        }

        // [CSS-GRID §7.3] repeat(2, 50%) creates two equal percentage tracks
        [Fact]
        public void Repeat2_50Percent()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:repeat(2, 50%);width:400px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.Width - 200) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.Width - 200) < 2);
        }

        // [CSS-GRID §7.2] Percentage columns exceeding 100% overflow the container
        [Fact]
        public void PercentageExceeding100_Overflow()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:60% 60%;width:400px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.Width - 240) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.Width - 240) < 2);
        }

        // [CSS-GRID §7.2] Small percentage (5%) produces narrow track
        [Fact]
        public void SmallPercentage_5Percent()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:5% 95%;width:400px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.Width - 20) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.Width - 380) < 2);
        }

        // [CSS-GRID §7.2] Percentage tracks with padding on the grid container
        [Fact]
        public void PercentageWithPaddingOnGrid()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:50% 50%;width:400px;padding:20px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                </div></body>");
            // Percentage resolves against content box (400px), not padding box (440px)
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.Width - 200) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.Width - 200) < 2);
        }

        // [CSS-GRID §7.2] Percentage tracks with border on the grid container
        [Fact]
        public void PercentageWithBorderOnGrid()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:50% 50%;width:400px;border:10px solid black'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                </div></body>");
            // Percentage resolves against content box (400px), not border box (420px)
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.Width - 200) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.Width - 200) < 2);
        }

        // [CSS-GRID §7.2] Percentage tracks with border-box sizing on grid container
        [Fact]
        public void PercentageWithBorderBoxOnGrid()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:50% 50%;width:400px;padding:20px;box-sizing:border-box'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                </div></body>");
            // border-box: width 400 includes padding, content box = 400 - 40 = 360
            // 50% of 360 = 180
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.Width - 180) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.Width - 180) < 2);
        }

        // [CSS-GRID §7.2] Single 100% column fills entire container
        [Fact]
        public void SingleColumn_100Percent()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100%;width:400px'>
                    <div id='t' style='height:20px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.Width - 400) < 2);
        }

        // [CSS-GRID §7.3] auto-fill with percentage track size
        [Fact]
        public void AutoFillWithPercentage()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:repeat(auto-fill, 25%);width:400px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                    <div id='c' style='height:20px'></div>
                    <div id='d' style='height:20px'></div>
                </div></body>");
            // auto-fill with 25%: should create 4 columns of 100px each
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "d")!.ContentRect.Width - 100) < 2);
        }

        // [CSS-GRID §7.2] Percentage item width inside percentage track
        [Fact]
        public void PercentageItemWidthInsidePercentageTrack()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:50% 50%;width:400px'>
                    <div id='t' style='width:50%;height:20px'></div>
                    <div style='height:20px'></div>
                </div></body>");
            // Track is 200px, item is 50% of track = 100px
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.Width - 100) < 2);
        }

        // [CSS-GRID §7.2] Percentage rows with gap and explicit height
        [Fact]
        public void Row_PercentageWithGapAndHeight()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;grid-template-rows:50% 50%;row-gap:20px;width:100px;height:200px'>
                    <div id='a'></div>
                    <div id='b'></div>
                </div></body>");
            // 50% of 200 = 100 each, gap = 20
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.Height - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.Height - 100) < 2);
            float rowGap = LayoutTestHelper.FindById(root, "b")!.ContentRect.Y
                         - (LayoutTestHelper.FindById(root, "a")!.ContentRect.Y + LayoutTestHelper.FindById(root, "a")!.ContentRect.Height);
            Assert.True(System.Math.Abs(rowGap - 20) < 2, $"Expected 20px row gap, got {rowGap}");
        }

        // [CSS-GRID §7.2] Three percentage columns that don't sum to 100 (10% + 20% + 30%)
        [Fact]
        public void Column_PercentageNotSumming100()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:10% 20% 30%;width:400px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                    <div id='c' style='height:20px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.Width - 40) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.Width - 80) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "c")!.ContentRect.Width - 120) < 2);
        }

        // [CSS-GRID §7.2] Percentage + fr + fixed mixed three-track layout
        [Fact]
        public void Column_PercentagePlusFrPlusFixed()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:25% 1fr 100px;width:400px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                    <div id='c' style='height:20px'></div>
                </div></body>");
            // 25% of 400 = 100; fixed = 100; fr gets remainder = 200
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.Width - 200) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "c")!.ContentRect.Width - 100) < 2);
        }

        // [CSS-GRID §7.2] Percentage tracks X positions are correctly accumulated
        [Fact]
        public void PercentageTrack_ItemXPositions()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:20% 30% 50%;width:400px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                    <div id='c' style='height:20px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.X - 80) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "c")!.ContentRect.X - 200) < 2);
        }

        // [CSS-GRID §7.2] Percentage column tracks with narrow container width
        [Fact]
        public void PercentageWithNarrowContainer()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:50% 50%;width:50px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.Width - 25) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.Width - 25) < 2);
        }

        // [CSS-GRID §7.2] Percentage row Y positions with explicit height
        [Fact]
        public void Row_PercentageYPositions()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;grid-template-rows:25% 25% 50%;width:100px;height:200px'>
                    <div id='a'></div>
                    <div id='b'></div>
                    <div id='c'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.Y - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.Y - 50) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "c")!.ContentRect.Y - 100) < 2);
        }

        // [CSS-GRID §7.2] Percentage columns with gap and items positioned correctly
        [Fact]
        public void PercentageColumnsWithGap_ItemPositions()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:25% 25% 25% 25%;column-gap:10px;width:400px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                    <div id='c' style='height:20px'></div>
                    <div id='d' style='height:20px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            // Each column = 25% of 400 = 100px
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 100) < 2);
            // Second item starts after first column + gap
            float expectedBX = itemA.ContentRect.Width + 10;
            Assert.True(System.Math.Abs(itemB.ContentRect.X - expectedBX) < 2, $"Expected X={expectedBX}, got {itemB.ContentRect.X}");
        }

        // [CSS-GRID §7.2] Percentage row spanning two percentage rows
        [Fact]
        public void Row_PercentageSpanning()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px 100px;grid-template-rows:30% 70%;width:200px;height:200px'>
                    <div id='t' style='grid-row:1/3'></div>
                    <div style='height:20px'></div>
                    <div style='height:20px'></div>
                </div></body>");
            // Spanning both rows: 30% + 70% = 100% of 200 = 200
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.Height - 200) < 2);
        }

        // [CSS-GRID §7.2] Repeat percentage with gap
        [Fact]
        public void Repeat4_25PercentWithGap()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:repeat(4, 25%);gap:10px;width:400px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                    <div id='c' style='height:20px'></div>
                    <div id='d' style='height:20px'></div>
                </div></body>");
            // Each column 25% of 400 = 100px
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "d")!.ContentRect.Width - 100) < 2);
        }

        // [CSS-GRID §7.2] Percentage with border+padding on grid using border-box
        [Fact]
        public void PercentageWithBorderPaddingBorderBox()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:50% 50%;width:400px;padding:10px;border:5px solid black;box-sizing:border-box'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                </div></body>");
            // border-box: width 400 includes border+padding, content = 400 - 10 - 10 - 5 - 5 = 370
            // 50% of 370 = 185
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.Width - 185) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.Width - 185) < 2);
        }

        // [CSS-GRID §7.2] Percentage column spanning multiple percentage tracks
        [Fact]
        public void Column_PercentageSpanning()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:25% 25% 50%;width:400px'>
                    <div id='t' style='grid-column:1/3;height:20px'></div>
                    <div style='height:20px'></div>
                </div></body>");
            // Spanning first two columns: 25% + 25% = 50% of 400 = 200
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.Width - 200) < 2);
        }
    }
}
