using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    /// <summary>
    /// Tests for CSS Grid fr-unit row track sizing and Y position placement.
    /// Covers single and multiple fr rows, mixed fixed+fr rows, gaps,
    /// repeat(), percentage rows, and auto-height containers.
    /// </summary>
    public class WptGridFrRowHeightPositionTests
    {
        private readonly ITestOutputHelper _output;

        public WptGridFrRowHeightPositionTests(ITestOutputHelper output)
        {
            _output = output;
        }

        // [CSS-GRID §7.2.3] Single 1fr row fills entire container height
        [Fact]
        public void SingleFrRow_FillsContainerHeight()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;grid-template-rows:1fr;height:200px;width:100px'>
                    <div id='item' style='background:red'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(System.Math.Abs(item.ContentRect.Height - 200) < 2, $"1fr should fill 200px (got {item.ContentRect.Height})");
        }

        // [CSS-GRID §7.2.3] Two equal 1fr rows split container evenly
        [Fact]
        public void TwoEqualFrRows_SplitEvenly()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;grid-template-rows:1fr 1fr;height:200px;width:100px'>
                    <div id='row1'></div>
                    <div id='row2'></div>
                </div></body>");
            var row1 = LayoutTestHelper.FindById(root, "row1")!;
            var row2 = LayoutTestHelper.FindById(root, "row2")!;
            Assert.True(System.Math.Abs(row1.ContentRect.Height - 100) < 2, $"First 1fr row should be 100px (got {row1.ContentRect.Height})");
            Assert.True(System.Math.Abs(row2.ContentRect.Height - 100) < 2, $"Second 1fr row should be 100px (got {row2.ContentRect.Height})");
        }

        // [CSS-GRID §7.2.3] Three equal 1fr rows in 300px container
        [Fact]
        public void ThreeEqualFrRows_SplitEvenly()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;grid-template-rows:1fr 1fr 1fr;height:300px;width:100px'>
                    <div id='row1'></div>
                    <div id='row2'></div>
                    <div id='row3'></div>
                </div></body>");
            var row1 = LayoutTestHelper.FindById(root, "row1")!;
            var row2 = LayoutTestHelper.FindById(root, "row2")!;
            var row3 = LayoutTestHelper.FindById(root, "row3")!;
            Assert.True(System.Math.Abs(row1.ContentRect.Height - 100) < 2, $"Row 1 height (got {row1.ContentRect.Height})");
            Assert.True(System.Math.Abs(row2.ContentRect.Height - 100) < 2, $"Row 2 height (got {row2.ContentRect.Height})");
            Assert.True(System.Math.Abs(row3.ContentRect.Height - 100) < 2, $"Row 3 height (got {row3.ContentRect.Height})");
        }

        // [CSS-GRID §7.2.3] 1fr + 2fr rows distribute in 1:2 ratio
        [Fact]
        public void FrRows_1to2_Ratio()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;grid-template-rows:1fr 2fr;height:300px;width:100px'>
                    <div id='row1'></div>
                    <div id='row2'></div>
                </div></body>");
            var row1 = LayoutTestHelper.FindById(root, "row1")!;
            var row2 = LayoutTestHelper.FindById(root, "row2")!;
            Assert.True(System.Math.Abs(row1.ContentRect.Height - 100) < 2, $"1fr should be 100px (got {row1.ContentRect.Height})");
            Assert.True(System.Math.Abs(row2.ContentRect.Height - 200) < 2, $"2fr should be 200px (got {row2.ContentRect.Height})");
        }

        // [CSS-GRID §7.2.3] 1fr + 2fr + 3fr rows distribute in 1:2:3 ratio
        [Fact]
        public void FrRows_1_2_3_Ratio()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;grid-template-rows:1fr 2fr 3fr;height:600px;width:100px'>
                    <div id='row1'></div>
                    <div id='row2'></div>
                    <div id='row3'></div>
                </div></body>");
            var row1 = LayoutTestHelper.FindById(root, "row1")!;
            var row2 = LayoutTestHelper.FindById(root, "row2")!;
            var row3 = LayoutTestHelper.FindById(root, "row3")!;
            Assert.True(System.Math.Abs(row1.ContentRect.Height - 100) < 2, $"1fr = 100px (got {row1.ContentRect.Height})");
            Assert.True(System.Math.Abs(row2.ContentRect.Height - 200) < 2, $"2fr = 200px (got {row2.ContentRect.Height})");
            Assert.True(System.Math.Abs(row3.ContentRect.Height - 300) < 2, $"3fr = 300px (got {row3.ContentRect.Height})");
        }

        // [CSS-GRID §7.2.3] Fixed + 1fr + fixed rows: fr absorbs remaining space
        [Fact]
        public void FixedFrFixed_FrAbsorbsRemainder()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;grid-template-rows:50px 1fr 50px;height:200px;width:100px'>
                    <div id='top'></div>
                    <div id='middle'></div>
                    <div id='bottom'></div>
                </div></body>");
            var top = LayoutTestHelper.FindById(root, "top")!;
            var middle = LayoutTestHelper.FindById(root, "middle")!;
            var bottom = LayoutTestHelper.FindById(root, "bottom")!;
            Assert.True(System.Math.Abs(top.ContentRect.Height - 50) < 2, $"Top fixed (got {top.ContentRect.Height})");
            Assert.True(System.Math.Abs(middle.ContentRect.Height - 100) < 2, $"Middle 1fr (got {middle.ContentRect.Height})");
            Assert.True(System.Math.Abs(bottom.ContentRect.Height - 50) < 2, $"Bottom fixed (got {bottom.ContentRect.Height})");
        }

        // [CSS-GRID §7.2.3] Single 1fr row with row-gap
        [Fact]
        public void SingleFrRow_WithGap_FillsContainerHeight()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;grid-template-rows:1fr;row-gap:20px;height:200px;width:100px'>
                    <div id='item'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(System.Math.Abs(item.ContentRect.Height - 200) < 2, $"Single 1fr with gap still fills container (got {item.ContentRect.Height})");
        }

        // [CSS-GRID §7.2.3] Two 1fr rows with row-gap: gap subtracted from available space
        [Fact]
        public void TwoFrRows_WithGap_GapSubtracted()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;grid-template-rows:1fr 1fr;row-gap:20px;height:220px;width:100px'>
                    <div id='row1'></div>
                    <div id='row2'></div>
                </div></body>");
            var row1 = LayoutTestHelper.FindById(root, "row1")!;
            var row2 = LayoutTestHelper.FindById(root, "row2")!;
            Assert.True(System.Math.Abs(row1.ContentRect.Height - 100) < 2, $"Row 1 = (220-20)/2 = 100 (got {row1.ContentRect.Height})");
            Assert.True(System.Math.Abs(row2.ContentRect.Height - 100) < 2, $"Row 2 = (220-20)/2 = 100 (got {row2.ContentRect.Height})");
        }

        // [CSS-GRID §7.2.3] 1fr + 2fr rows with row-gap
        [Fact]
        public void FrRows_1to2_WithGap()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;grid-template-rows:1fr 2fr;row-gap:30px;height:330px;width:100px'>
                    <div id='row1'></div>
                    <div id='row2'></div>
                </div></body>");
            var row1 = LayoutTestHelper.FindById(root, "row1")!;
            var row2 = LayoutTestHelper.FindById(root, "row2")!;
            Assert.True(System.Math.Abs(row1.ContentRect.Height - 100) < 2, $"1fr = (330-30)/3 = 100 (got {row1.ContentRect.Height})");
            Assert.True(System.Math.Abs(row2.ContentRect.Height - 200) < 2, $"2fr = 2*(330-30)/3 = 200 (got {row2.ContentRect.Height})");
        }

        // [CSS-GRID §7.2.3] Y position: single 1fr row starts at Y=0
        [Fact]
        public void YPosition_SingleFrRow_StartsAtZero()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;grid-template-rows:1fr;height:200px;width:100px'>
                    <div id='item'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(System.Math.Abs(item.ContentRect.Y - 0) < 2, $"Single fr row Y=0 (got {item.ContentRect.Y})");
        }

        // [CSS-GRID §7.2.3] Y positions for two equal fr rows
        [Fact]
        public void YPosition_TwoEqualFrRows()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;grid-template-rows:1fr 1fr;height:200px;width:100px'>
                    <div id='row1'></div>
                    <div id='row2'></div>
                </div></body>");
            var row1 = LayoutTestHelper.FindById(root, "row1")!;
            var row2 = LayoutTestHelper.FindById(root, "row2")!;
            Assert.True(System.Math.Abs(row1.ContentRect.Y - 0) < 2, $"Row 1 Y=0 (got {row1.ContentRect.Y})");
            Assert.True(System.Math.Abs(row2.ContentRect.Y - 100) < 2, $"Row 2 Y=100 (got {row2.ContentRect.Y})");
        }

        // [CSS-GRID §7.2.3] Y positions for 1fr + 2fr + 3fr
        [Fact]
        public void YPosition_FrRows_1_2_3()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;grid-template-rows:1fr 2fr 3fr;height:600px;width:100px'>
                    <div id='row1'></div>
                    <div id='row2'></div>
                    <div id='row3'></div>
                </div></body>");
            var row1 = LayoutTestHelper.FindById(root, "row1")!;
            var row2 = LayoutTestHelper.FindById(root, "row2")!;
            var row3 = LayoutTestHelper.FindById(root, "row3")!;
            Assert.True(System.Math.Abs(row1.ContentRect.Y - 0) < 2, $"Row 1 Y=0 (got {row1.ContentRect.Y})");
            Assert.True(System.Math.Abs(row2.ContentRect.Y - 100) < 2, $"Row 2 Y=100 (got {row2.ContentRect.Y})");
            Assert.True(System.Math.Abs(row3.ContentRect.Y - 300) < 2, $"Row 3 Y=300 (got {row3.ContentRect.Y})");
        }

        // [CSS-GRID §7.2.3] Y positions for fixed + 1fr + fixed rows
        [Fact]
        public void YPosition_FixedFrFixed()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;grid-template-rows:50px 1fr 50px;height:200px;width:100px'>
                    <div id='top'></div>
                    <div id='middle'></div>
                    <div id='bottom'></div>
                </div></body>");
            var top = LayoutTestHelper.FindById(root, "top")!;
            var middle = LayoutTestHelper.FindById(root, "middle")!;
            var bottom = LayoutTestHelper.FindById(root, "bottom")!;
            Assert.True(System.Math.Abs(top.ContentRect.Y - 0) < 2, $"Top Y=0 (got {top.ContentRect.Y})");
            Assert.True(System.Math.Abs(middle.ContentRect.Y - 50) < 2, $"Middle Y=50 (got {middle.ContentRect.Y})");
            Assert.True(System.Math.Abs(bottom.ContentRect.Y - 150) < 2, $"Bottom Y=150 (got {bottom.ContentRect.Y})");
        }

        // [CSS-GRID §7.2.3] Y positions with row-gap between two 1fr rows
        [Fact]
        public void YPosition_TwoFrRows_WithGap()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;grid-template-rows:1fr 1fr;row-gap:20px;height:220px;width:100px'>
                    <div id='row1'></div>
                    <div id='row2'></div>
                </div></body>");
            var row1 = LayoutTestHelper.FindById(root, "row1")!;
            var row2 = LayoutTestHelper.FindById(root, "row2")!;
            Assert.True(System.Math.Abs(row1.ContentRect.Y - 0) < 2, $"Row 1 Y=0 (got {row1.ContentRect.Y})");
            Assert.True(System.Math.Abs(row2.ContentRect.Y - 120) < 2, $"Row 2 Y=100+20gap=120 (got {row2.ContentRect.Y})");
        }

        // [CSS-GRID §7.2.3] Y positions for 1fr + 2fr with gap
        [Fact]
        public void YPosition_FrRows_1to2_WithGap()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;grid-template-rows:1fr 2fr;row-gap:30px;height:330px;width:100px'>
                    <div id='row1'></div>
                    <div id='row2'></div>
                </div></body>");
            var row1 = LayoutTestHelper.FindById(root, "row1")!;
            var row2 = LayoutTestHelper.FindById(root, "row2")!;
            Assert.True(System.Math.Abs(row1.ContentRect.Y - 0) < 2, $"Row 1 Y=0 (got {row1.ContentRect.Y})");
            Assert.True(System.Math.Abs(row2.ContentRect.Y - 130) < 2, $"Row 2 Y=100+30gap=130 (got {row2.ContentRect.Y})");
        }

        // [CSS-GRID §7.2.3] fr rows with no explicit container height resolve to auto (content-sized)
        [Fact]
        public void FrRows_AutoHeight_FallbackToContentSize()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;grid-template-rows:1fr 1fr;width:100px'>
                    <div id='row1' style='height:40px'></div>
                    <div id='row2' style='height:60px'></div>
                </div></body>");
            var row1 = LayoutTestHelper.FindById(root, "row1")!;
            var row2 = LayoutTestHelper.FindById(root, "row2")!;
            Assert.True(row1.ContentRect.Height >= 39, $"Auto-height fr row 1 uses content height (got {row1.ContentRect.Height})");
            Assert.True(row2.ContentRect.Height >= 59, $"Auto-height fr row 2 uses content height (got {row2.ContentRect.Height})");
        }

        // [CSS-GRID §7.3] repeat(3, 1fr) rows in 300px container
        [Fact]
        public void RepeatThreeFrRows_In300()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;grid-template-rows:repeat(3,1fr);height:300px;width:100px'>
                    <div id='row1'></div>
                    <div id='row2'></div>
                    <div id='row3'></div>
                </div></body>");
            var row1 = LayoutTestHelper.FindById(root, "row1")!;
            var row2 = LayoutTestHelper.FindById(root, "row2")!;
            var row3 = LayoutTestHelper.FindById(root, "row3")!;
            Assert.True(System.Math.Abs(row1.ContentRect.Height - 100) < 2, $"repeat(3,1fr) row 1 = 100 (got {row1.ContentRect.Height})");
            Assert.True(System.Math.Abs(row2.ContentRect.Height - 100) < 2, $"repeat(3,1fr) row 2 = 100 (got {row2.ContentRect.Height})");
            Assert.True(System.Math.Abs(row3.ContentRect.Height - 100) < 2, $"repeat(3,1fr) row 3 = 100 (got {row3.ContentRect.Height})");
        }

        // [CSS-GRID §7.3] repeat(4, 1fr) rows in 400px container
        [Fact]
        public void RepeatFourFrRows_In400()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;grid-template-rows:repeat(4,1fr);height:400px;width:100px'>
                    <div id='row1'></div>
                    <div id='row2'></div>
                    <div id='row3'></div>
                    <div id='row4'></div>
                </div></body>");
            var row1 = LayoutTestHelper.FindById(root, "row1")!;
            var row2 = LayoutTestHelper.FindById(root, "row2")!;
            var row3 = LayoutTestHelper.FindById(root, "row3")!;
            var row4 = LayoutTestHelper.FindById(root, "row4")!;
            Assert.True(System.Math.Abs(row1.ContentRect.Height - 100) < 2, $"Row 1 = 100 (got {row1.ContentRect.Height})");
            Assert.True(System.Math.Abs(row2.ContentRect.Height - 100) < 2, $"Row 2 = 100 (got {row2.ContentRect.Height})");
            Assert.True(System.Math.Abs(row3.ContentRect.Height - 100) < 2, $"Row 3 = 100 (got {row3.ContentRect.Height})");
            Assert.True(System.Math.Abs(row4.ContentRect.Height - 100) < 2, $"Row 4 = 100 (got {row4.ContentRect.Height})");
        }

        // [CSS-GRID §7.3] repeat(4, 1fr) Y positions in 400px container
        [Fact]
        public void YPosition_RepeatFourFrRows_In400()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;grid-template-rows:repeat(4,1fr);height:400px;width:100px'>
                    <div id='row1'></div>
                    <div id='row2'></div>
                    <div id='row3'></div>
                    <div id='row4'></div>
                </div></body>");
            var row1 = LayoutTestHelper.FindById(root, "row1")!;
            var row2 = LayoutTestHelper.FindById(root, "row2")!;
            var row3 = LayoutTestHelper.FindById(root, "row3")!;
            var row4 = LayoutTestHelper.FindById(root, "row4")!;
            Assert.True(System.Math.Abs(row1.ContentRect.Y - 0) < 2, $"Row 1 Y=0 (got {row1.ContentRect.Y})");
            Assert.True(System.Math.Abs(row2.ContentRect.Y - 100) < 2, $"Row 2 Y=100 (got {row2.ContentRect.Y})");
            Assert.True(System.Math.Abs(row3.ContentRect.Y - 200) < 2, $"Row 3 Y=200 (got {row3.ContentRect.Y})");
            Assert.True(System.Math.Abs(row4.ContentRect.Y - 300) < 2, $"Row 4 Y=300 (got {row4.ContentRect.Y})");
        }

        // [CSS-GRID §7.2] Percentage row: 50% of 200px container height
        [Fact]
        public void PercentRow_50Percent_Of200()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;grid-template-rows:50% 50%;height:200px;width:100px'>
                    <div id='row1'></div>
                    <div id='row2'></div>
                </div></body>");
            var row1 = LayoutTestHelper.FindById(root, "row1")!;
            var row2 = LayoutTestHelper.FindById(root, "row2")!;
            Assert.True(System.Math.Abs(row1.ContentRect.Height - 100) < 2, $"50% of 200 = 100 (got {row1.ContentRect.Height})");
            Assert.True(System.Math.Abs(row2.ContentRect.Height - 100) < 2, $"50% of 200 = 100 (got {row2.ContentRect.Height})");
        }

        // [CSS-GRID §7.2] Container height determined by fixed row tracks
        [Fact]
        public void ContainerHeight_FromFixedRows()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='grid' style='display:grid;grid-template-columns:100px;grid-template-rows:80px 120px;width:100px'>
                    <div></div>
                    <div></div>
                </div></body>");
            var grid = LayoutTestHelper.FindById(root, "grid")!;
            Assert.True(System.Math.Abs(grid.ContentRect.Height - 200) < 2, $"Container = 80+120 = 200 (got {grid.ContentRect.Height})");
        }

        // [CSS-GRID §7.2] Container height determined by auto rows with content
        [Fact]
        public void ContainerHeight_FromAutoRowsWithContent()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='grid' style='display:grid;grid-template-columns:100px;width:100px'>
                    <div style='height:50px'></div>
                    <div style='height:70px'></div>
                </div></body>");
            var grid = LayoutTestHelper.FindById(root, "grid")!;
            Assert.True(System.Math.Abs(grid.ContentRect.Height - 120) < 2, $"Container = 50+70 = 120 (got {grid.ContentRect.Height})");
        }

        // [CSS-GRID §7.2.3] Three 1fr rows with gap: Y positions account for gaps
        [Fact]
        public void YPosition_ThreeFrRows_WithGap()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;grid-template-rows:1fr 1fr 1fr;row-gap:10px;height:320px;width:100px'>
                    <div id='row1'></div>
                    <div id='row2'></div>
                    <div id='row3'></div>
                </div></body>");
            var row1 = LayoutTestHelper.FindById(root, "row1")!;
            var row2 = LayoutTestHelper.FindById(root, "row2")!;
            var row3 = LayoutTestHelper.FindById(root, "row3")!;
            // available = 320 - 2*10 = 300, each 1fr = 100
            Assert.True(System.Math.Abs(row1.ContentRect.Height - 100) < 2, $"Row 1 = 100 (got {row1.ContentRect.Height})");
            Assert.True(System.Math.Abs(row1.ContentRect.Y - 0) < 2, $"Row 1 Y=0 (got {row1.ContentRect.Y})");
            Assert.True(System.Math.Abs(row2.ContentRect.Y - 110) < 2, $"Row 2 Y=100+10gap=110 (got {row2.ContentRect.Y})");
            Assert.True(System.Math.Abs(row3.ContentRect.Y - 220) < 2, $"Row 3 Y=200+20gap=220 (got {row3.ContentRect.Y})");
        }

        // [CSS-GRID §7.2.3] Mixed fixed + fr + fr rows
        [Fact]
        public void MixedRows_Fixed_Fr_Fr()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;grid-template-rows:60px 1fr 2fr;height:360px;width:100px'>
                    <div id='fixed'></div>
                    <div id='fr1'></div>
                    <div id='fr2'></div>
                </div></body>");
            var fixedRow = LayoutTestHelper.FindById(root, "fixed")!;
            var fr1Row = LayoutTestHelper.FindById(root, "fr1")!;
            var fr2Row = LayoutTestHelper.FindById(root, "fr2")!;
            // remaining = 360-60 = 300, 1fr=100, 2fr=200
            Assert.True(System.Math.Abs(fixedRow.ContentRect.Height - 60) < 2, $"Fixed = 60 (got {fixedRow.ContentRect.Height})");
            Assert.True(System.Math.Abs(fr1Row.ContentRect.Height - 100) < 2, $"1fr = 100 (got {fr1Row.ContentRect.Height})");
            Assert.True(System.Math.Abs(fr2Row.ContentRect.Height - 200) < 2, $"2fr = 200 (got {fr2Row.ContentRect.Height})");
        }

        // [CSS-GRID §7.2.3] Y positions for mixed fixed + fr + fr rows
        [Fact]
        public void YPosition_MixedRows_Fixed_Fr_Fr()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;grid-template-rows:60px 1fr 2fr;height:360px;width:100px'>
                    <div id='fixed'></div>
                    <div id='fr1'></div>
                    <div id='fr2'></div>
                </div></body>");
            var fixedRow = LayoutTestHelper.FindById(root, "fixed")!;
            var fr1Row = LayoutTestHelper.FindById(root, "fr1")!;
            var fr2Row = LayoutTestHelper.FindById(root, "fr2")!;
            Assert.True(System.Math.Abs(fixedRow.ContentRect.Y - 0) < 2, $"Fixed Y=0 (got {fixedRow.ContentRect.Y})");
            Assert.True(System.Math.Abs(fr1Row.ContentRect.Y - 60) < 2, $"1fr Y=60 (got {fr1Row.ContentRect.Y})");
            Assert.True(System.Math.Abs(fr2Row.ContentRect.Y - 160) < 2, $"2fr Y=60+100=160 (got {fr2Row.ContentRect.Y})");
        }

        // [CSS-GRID §7.2.3] Percent row + fr row combined
        [Fact]
        public void PercentRow_PlusFrRow()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;grid-template-rows:25% 1fr;height:200px;width:100px'>
                    <div id='percent'></div>
                    <div id='fr'></div>
                </div></body>");
            var percentRow = LayoutTestHelper.FindById(root, "percent")!;
            var frRow = LayoutTestHelper.FindById(root, "fr")!;
            Assert.True(System.Math.Abs(percentRow.ContentRect.Height - 50) < 2, $"25% of 200 = 50 (got {percentRow.ContentRect.Height})");
            Assert.True(System.Math.Abs(frRow.ContentRect.Height - 150) < 2, $"1fr = 200-50 = 150 (got {frRow.ContentRect.Height})");
        }

        // [CSS-GRID §7.2.3] Y position for percent row + fr row
        [Fact]
        public void YPosition_PercentRow_PlusFrRow()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;grid-template-rows:25% 1fr;height:200px;width:100px'>
                    <div id='percent'></div>
                    <div id='fr'></div>
                </div></body>");
            var percentRow = LayoutTestHelper.FindById(root, "percent")!;
            var frRow = LayoutTestHelper.FindById(root, "fr")!;
            Assert.True(System.Math.Abs(percentRow.ContentRect.Y - 0) < 2, $"Percent Y=0 (got {percentRow.ContentRect.Y})");
            Assert.True(System.Math.Abs(frRow.ContentRect.Y - 50) < 2, $"Fr Y=50 (got {frRow.ContentRect.Y})");
        }

        // [CSS-GRID §7.2.3] Large fr value: 5fr single row fills container
        [Fact]
        public void LargeFrValue_SingleRow_FillsContainer()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;grid-template-rows:5fr;height:250px;width:100px'>
                    <div id='item'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(System.Math.Abs(item.ContentRect.Height - 250) < 2, $"5fr single fills 250px (got {item.ContentRect.Height})");
        }

        // [CSS-GRID §7.2.3] Container height from fixed rows with gap
        [Fact]
        public void ContainerHeight_FixedRows_WithGap()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='grid' style='display:grid;grid-template-columns:100px;grid-template-rows:50px 50px 50px;row-gap:10px;width:100px'>
                    <div></div>
                    <div></div>
                    <div></div>
                </div></body>");
            var grid = LayoutTestHelper.FindById(root, "grid")!;
            // 3*50 + 2*10 = 170
            Assert.True(System.Math.Abs(grid.ContentRect.Height - 170) < 2, $"Container = 3*50 + 2*10 = 170 (got {grid.ContentRect.Height})");
        }

        // [CSS-GRID §7.2.3] Fixed + fr + fixed rows with gap: fr absorbs remainder minus gaps
        [Fact]
        public void FixedFrFixed_WithGap()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;grid-template-rows:40px 1fr 40px;row-gap:10px;height:200px;width:100px'>
                    <div id='top'></div>
                    <div id='middle'></div>
                    <div id='bottom'></div>
                </div></body>");
            var top = LayoutTestHelper.FindById(root, "top")!;
            var middle = LayoutTestHelper.FindById(root, "middle")!;
            var bottom = LayoutTestHelper.FindById(root, "bottom")!;
            // available for fr = 200 - 40 - 40 - 2*10 = 100
            Assert.True(System.Math.Abs(top.ContentRect.Height - 40) < 2, $"Top = 40 (got {top.ContentRect.Height})");
            Assert.True(System.Math.Abs(middle.ContentRect.Height - 100) < 2, $"Middle 1fr = 100 (got {middle.ContentRect.Height})");
            Assert.True(System.Math.Abs(bottom.ContentRect.Height - 40) < 2, $"Bottom = 40 (got {bottom.ContentRect.Height})");
        }

        // [CSS-GRID §7.2.3] Y positions for fixed + fr + fixed with gap
        [Fact]
        public void YPosition_FixedFrFixed_WithGap()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;grid-template-rows:40px 1fr 40px;row-gap:10px;height:200px;width:100px'>
                    <div id='top'></div>
                    <div id='middle'></div>
                    <div id='bottom'></div>
                </div></body>");
            var top = LayoutTestHelper.FindById(root, "top")!;
            var middle = LayoutTestHelper.FindById(root, "middle")!;
            var bottom = LayoutTestHelper.FindById(root, "bottom")!;
            Assert.True(System.Math.Abs(top.ContentRect.Y - 0) < 2, $"Top Y=0 (got {top.ContentRect.Y})");
            Assert.True(System.Math.Abs(middle.ContentRect.Y - 50) < 2, $"Middle Y=40+10gap=50 (got {middle.ContentRect.Y})");
            Assert.True(System.Math.Abs(bottom.ContentRect.Y - 160) < 2, $"Bottom Y=50+100+10gap=160 (got {bottom.ContentRect.Y})");
        }
    }
}
