using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    /// <summary>
    /// Grid cell Y position tests covering row placement, row-gap, fr rows,
    /// auto rows, multi-column grids, spanning items, and container offsets.
    /// </summary>
    public class WptGridCellYPositionTests
    {
        private readonly ITestOutputHelper _output;

        public WptGridCellYPositionTests(ITestOutputHelper output)
        {
            _output = output;
        }

        // [CSS-GRID §7.2] 2 fixed rows: 50px + 60px
        [Fact]
        public void TwoFixedRows_50_60_SecondRowStartsAt50()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;grid-template-rows:50px 60px;width:100px'>
                    <div id='r1' style='background:red'></div>
                    <div id='r2' style='background:blue'></div>
                </div></body>");
            var row1 = LayoutTestHelper.FindById(root, "r1")!;
            var row2 = LayoutTestHelper.FindById(root, "r2")!;
            Assert.True(System.Math.Abs(row1.ContentRect.Y - 0) < 2, $"Row 1 Y={row1.ContentRect.Y}");
            Assert.True(System.Math.Abs(row1.ContentRect.Height - 50) < 2, $"Row 1 H={row1.ContentRect.Height}");
            Assert.True(System.Math.Abs(row2.ContentRect.Y - 50) < 2, $"Row 2 Y={row2.ContentRect.Y}");
            Assert.True(System.Math.Abs(row2.ContentRect.Height - 60) < 2, $"Row 2 H={row2.ContentRect.Height}");
        }

        // [CSS-GRID §7.2] 3 fixed rows: 30px + 40px + 50px
        [Fact]
        public void ThreeFixedRows_30_40_50_CorrectYPositions()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;grid-template-rows:30px 40px 50px;width:100px'>
                    <div id='r1'></div>
                    <div id='r2'></div>
                    <div id='r3'></div>
                </div></body>");
            var row1 = LayoutTestHelper.FindById(root, "r1")!;
            var row2 = LayoutTestHelper.FindById(root, "r2")!;
            var row3 = LayoutTestHelper.FindById(root, "r3")!;
            Assert.True(System.Math.Abs(row1.ContentRect.Y - 0) < 2, $"Row 1 Y={row1.ContentRect.Y}");
            Assert.True(System.Math.Abs(row2.ContentRect.Y - 30) < 2, $"Row 2 Y={row2.ContentRect.Y}");
            Assert.True(System.Math.Abs(row3.ContentRect.Y - 70) < 2, $"Row 3 Y={row3.ContentRect.Y}");
        }

        // [CSS-GRID §7.2] 4 equal rows: 40px each
        [Fact]
        public void FourEqualRows_40Each_CorrectYPositions()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;grid-template-rows:40px 40px 40px 40px;width:100px'>
                    <div id='r1'></div>
                    <div id='r2'></div>
                    <div id='r3'></div>
                    <div id='r4'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r1")!.ContentRect.Y - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r2")!.ContentRect.Y - 40) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r3")!.ContentRect.Y - 80) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r4")!.ContentRect.Y - 120) < 2);
        }

        // [CSS-GRID §10.1] row-gap=10 with 2 rows of 50px
        [Fact]
        public void RowGap10_TwoRows50_SecondRowAt60()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;grid-template-rows:50px 50px;row-gap:10px;width:100px'>
                    <div id='r1'></div>
                    <div id='r2'></div>
                </div></body>");
            var row2 = LayoutTestHelper.FindById(root, "r2")!;
            Assert.True(System.Math.Abs(row2.ContentRect.Y - 60) < 2, $"Row 2 Y with gap={row2.ContentRect.Y}");
        }

        // [CSS-GRID §10.1] row-gap=15 with 3 rows of 30px
        [Fact]
        public void RowGap15_ThreeRows30_CorrectYPositions()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;grid-template-rows:30px 30px 30px;row-gap:15px;width:100px'>
                    <div id='r1'></div>
                    <div id='r2'></div>
                    <div id='r3'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r1")!.ContentRect.Y - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r2")!.ContentRect.Y - 45) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r3")!.ContentRect.Y - 90) < 2);
        }

        // [CSS-GRID §10.1] row-gap=10 with 4 rows of 40px
        [Fact]
        public void RowGap10_FourRows40_CorrectYPositions()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;grid-template-rows:40px 40px 40px 40px;row-gap:10px;width:100px'>
                    <div id='r1'></div>
                    <div id='r2'></div>
                    <div id='r3'></div>
                    <div id='r4'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r1")!.ContentRect.Y - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r2")!.ContentRect.Y - 50) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r3")!.ContentRect.Y - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r4")!.ContentRect.Y - 150) < 2);
        }

        // [CSS-GRID §7.2] 1fr 1fr rows with height=200
        [Fact]
        public void FrRows_1fr_1fr_Height200_EqualSplit()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;grid-template-rows:1fr 1fr;height:200px;width:100px'>
                    <div id='r1'></div>
                    <div id='r2'></div>
                </div></body>");
            var row1 = LayoutTestHelper.FindById(root, "r1")!;
            var row2 = LayoutTestHelper.FindById(root, "r2")!;
            Assert.True(System.Math.Abs(row1.ContentRect.Y - 0) < 2);
            Assert.True(System.Math.Abs(row1.ContentRect.Height - 100) < 2, $"Row 1 H={row1.ContentRect.Height}");
            Assert.True(System.Math.Abs(row2.ContentRect.Y - 100) < 2, $"Row 2 Y={row2.ContentRect.Y}");
            Assert.True(System.Math.Abs(row2.ContentRect.Height - 100) < 2, $"Row 2 H={row2.ContentRect.Height}");
        }

        // [CSS-GRID §7.2] 1fr 2fr rows with height=300
        [Fact]
        public void FrRows_1fr_2fr_Height300_OneThirdTwoThirds()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;grid-template-rows:1fr 2fr;height:300px;width:100px'>
                    <div id='r1'></div>
                    <div id='r2'></div>
                </div></body>");
            var row1 = LayoutTestHelper.FindById(root, "r1")!;
            var row2 = LayoutTestHelper.FindById(root, "r2")!;
            Assert.True(System.Math.Abs(row1.ContentRect.Height - 100) < 2, $"Row 1 H={row1.ContentRect.Height}");
            Assert.True(System.Math.Abs(row2.ContentRect.Y - 100) < 2, $"Row 2 Y={row2.ContentRect.Y}");
            Assert.True(System.Math.Abs(row2.ContentRect.Height - 200) < 2, $"Row 2 H={row2.ContentRect.Height}");
        }

        // [CSS-GRID §7.2] mixed fixed + fr rows: 60px + 1fr with height=200
        [Fact]
        public void MixedRows_Fixed60_1fr_Height200()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;grid-template-rows:60px 1fr;height:200px;width:100px'>
                    <div id='r1'></div>
                    <div id='r2'></div>
                </div></body>");
            var row1 = LayoutTestHelper.FindById(root, "r1")!;
            var row2 = LayoutTestHelper.FindById(root, "r2")!;
            Assert.True(System.Math.Abs(row1.ContentRect.Height - 60) < 2, $"Row 1 H={row1.ContentRect.Height}");
            Assert.True(System.Math.Abs(row2.ContentRect.Y - 60) < 2, $"Row 2 Y={row2.ContentRect.Y}");
            Assert.True(System.Math.Abs(row2.ContentRect.Height - 140) < 2, $"Row 2 H={row2.ContentRect.Height}");
        }

        // [CSS-GRID §7.2] mixed fixed + fr rows: 50px + 1fr + 50px with height=250
        [Fact]
        public void MixedRows_50_1fr_50_Height250()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;grid-template-rows:50px 1fr 50px;height:250px;width:100px'>
                    <div id='r1'></div>
                    <div id='r2'></div>
                    <div id='r3'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r1")!.ContentRect.Y - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r2")!.ContentRect.Y - 50) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r2")!.ContentRect.Height - 150) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r3")!.ContentRect.Y - 200) < 2);
        }

        // [CSS-GRID §7.2] auto rows sized by content
        [Fact]
        public void AutoRows_FromContent_TwoRows()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;width:100px'>
                    <div id='r1' style='height:35px'></div>
                    <div id='r2' style='height:45px'></div>
                </div></body>");
            var row1 = LayoutTestHelper.FindById(root, "r1")!;
            var row2 = LayoutTestHelper.FindById(root, "r2")!;
            Assert.True(System.Math.Abs(row1.ContentRect.Height - 35) < 2, $"Row 1 H={row1.ContentRect.Height}");
            Assert.True(System.Math.Abs(row2.ContentRect.Y - 35) < 2, $"Row 2 Y={row2.ContentRect.Y}");
            Assert.True(System.Math.Abs(row2.ContentRect.Height - 45) < 2, $"Row 2 H={row2.ContentRect.Height}");
        }

        // [CSS-GRID §7.2] auto rows with 3 different content heights
        [Fact]
        public void AutoRows_FromContent_ThreeRows()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;width:100px'>
                    <div id='r1' style='height:20px'></div>
                    <div id='r2' style='height:40px'></div>
                    <div id='r3' style='height:60px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r1")!.ContentRect.Y - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r2")!.ContentRect.Y - 20) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r3")!.ContentRect.Y - 60) < 2);
        }

        // [CSS-GRID §7.5] grid-auto-rows: 40px
        [Fact]
        public void GridAutoRows40_ThreeItems_CorrectYPositions()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;grid-auto-rows:40px;width:100px'>
                    <div id='r1'></div>
                    <div id='r2'></div>
                    <div id='r3'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r1")!.ContentRect.Y - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r1")!.ContentRect.Height - 40) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r2")!.ContentRect.Y - 40) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r3")!.ContentRect.Y - 80) < 2);
        }

        // [CSS-GRID §7.5] grid-auto-rows: 40px with row-gap
        [Fact]
        public void GridAutoRows40_WithRowGap10_CorrectYPositions()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;grid-auto-rows:40px;row-gap:10px;width:100px'>
                    <div id='r1'></div>
                    <div id='r2'></div>
                    <div id='r3'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r1")!.ContentRect.Y - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r2")!.ContentRect.Y - 50) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r3")!.ContentRect.Y - 100) < 2);
        }

        // [CSS-GRID §8.2] 2-column grid: second row Y position
        [Fact]
        public void TwoColumnGrid_SecondRowY()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px 100px;grid-template-rows:50px 60px;width:200px'>
                    <div id='a'></div>
                    <div id='b'></div>
                    <div id='c'></div>
                    <div id='d'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.Y - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.Y - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "c")!.ContentRect.Y - 50) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "d")!.ContentRect.Y - 50) < 2);
        }

        // [CSS-GRID §8.2] 3-column grid: second row Y position
        [Fact]
        public void ThreeColumnGrid_SecondRowY()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:80px 80px 80px;grid-template-rows:45px 55px;width:240px'>
                    <div id='a'></div>
                    <div id='b'></div>
                    <div id='c'></div>
                    <div id='d'></div>
                    <div id='e'></div>
                    <div id='f'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.Y - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.Y - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "c")!.ContentRect.Y - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "d")!.ContentRect.Y - 45) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "e")!.ContentRect.Y - 45) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "f")!.ContentRect.Y - 45) < 2);
        }

        // [CSS-GRID §8.2] 2-column grid with row-gap: second row Y includes gap
        [Fact]
        public void TwoColumnGrid_WithRowGap_SecondRowY()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px 100px;grid-template-rows:50px 50px;row-gap:20px;width:200px'>
                    <div id='a'></div>
                    <div id='b'></div>
                    <div id='c'></div>
                    <div id='d'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "c")!.ContentRect.Y - 70) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "d")!.ContentRect.Y - 70) < 2);
        }

        // [CSS-GRID §8.3] spanning item Y position: grid-row: span 2
        [Fact]
        public void SpanningItem_RowSpan2_YPosition()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px 100px;grid-template-rows:40px 40px;width:200px'>
                    <div id='span' style='grid-row:1/3'></div>
                    <div id='topRight'></div>
                    <div id='bottomRight'></div>
                </div></body>");
            var spanItem = LayoutTestHelper.FindById(root, "span")!;
            var topRight = LayoutTestHelper.FindById(root, "topRight")!;
            var bottomRight = LayoutTestHelper.FindById(root, "bottomRight")!;
            Assert.True(System.Math.Abs(spanItem.ContentRect.Y - 0) < 2, $"Spanning item Y={spanItem.ContentRect.Y}");
            Assert.True(spanItem.ContentRect.Height >= 79, $"Spanning item H={spanItem.ContentRect.Height}");
            Assert.True(System.Math.Abs(topRight.ContentRect.Y - 0) < 2);
            Assert.True(System.Math.Abs(bottomRight.ContentRect.Y - 40) < 2);
        }

        // [CSS-GRID §8.3] spanning item with row-gap
        [Fact]
        public void SpanningItem_RowSpan2_WithRowGap()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px 100px;grid-template-rows:40px 40px;row-gap:10px;width:200px'>
                    <div id='span' style='grid-row:1/3'></div>
                    <div id='topRight'></div>
                    <div id='bottomRight'></div>
                </div></body>");
            var spanItem = LayoutTestHelper.FindById(root, "span")!;
            Assert.True(System.Math.Abs(spanItem.ContentRect.Y - 0) < 2);
            Assert.True(System.Math.Abs(spanItem.ContentRect.Height - 90) < 2, $"Spanning H (40+10+40)={spanItem.ContentRect.Height}");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "bottomRight")!.ContentRect.Y - 50) < 2);
        }

        // [CSS §5.3] container padding offsets cell Y
        [Fact]
        public void ContainerPadding_OffsetsCellY()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;grid-template-rows:50px;padding:20px;width:100px'>
                    <div id='cell'></div>
                </div></body>");
            var cell = LayoutTestHelper.FindById(root, "cell")!;
            Assert.True(System.Math.Abs(cell.ContentRect.Y - 20) < 2, $"Cell Y with padding={cell.ContentRect.Y}");
        }

        // [CSS §5.3] container padding with 2 rows
        [Fact]
        public void ContainerPadding_TwoRows_CorrectYPositions()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;grid-template-rows:40px 60px;padding:15px;width:100px'>
                    <div id='r1'></div>
                    <div id='r2'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r1")!.ContentRect.Y - 15) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r2")!.ContentRect.Y - 55) < 2);
        }

        // [CSS §8.5.1] container border offsets cell Y
        [Fact]
        public void ContainerBorder_OffsetsCellY()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;grid-template-rows:50px;border:10px solid black;width:100px'>
                    <div id='cell'></div>
                </div></body>");
            var cell = LayoutTestHelper.FindById(root, "cell")!;
            Assert.True(System.Math.Abs(cell.ContentRect.Y - 10) < 2, $"Cell Y with border={cell.ContentRect.Y}");
        }

        // [CSS §8.5.1] container border with 2 rows
        [Fact]
        public void ContainerBorder_TwoRows_CorrectYPositions()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;grid-template-rows:30px 50px;border:5px solid black;width:100px'>
                    <div id='r1'></div>
                    <div id='r2'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r1")!.ContentRect.Y - 5) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r2")!.ContentRect.Y - 35) < 2);
        }

        // [CSS §5.3 + §8.5.1] container padding + border combined
        [Fact]
        public void ContainerPaddingAndBorder_CombinedYOffset()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;grid-template-rows:40px;padding:10px;border:5px solid black;width:100px'>
                    <div id='cell'></div>
                </div></body>");
            var cell = LayoutTestHelper.FindById(root, "cell")!;
            Assert.True(System.Math.Abs(cell.ContentRect.Y - 15) < 2, $"Cell Y with padding+border={cell.ContentRect.Y}");
        }

        // [CSS-GRID §7.2] fr rows with row-gap
        [Fact]
        public void FrRows_1fr_1fr_WithRowGap_Height200()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;grid-template-rows:1fr 1fr;row-gap:20px;height:200px;width:100px'>
                    <div id='r1'></div>
                    <div id='r2'></div>
                </div></body>");
            var row1 = LayoutTestHelper.FindById(root, "r1")!;
            var row2 = LayoutTestHelper.FindById(root, "r2")!;
            Assert.True(System.Math.Abs(row1.ContentRect.Height - 90) < 2, $"Row 1 H=(200-20)/2={row1.ContentRect.Height}");
            Assert.True(System.Math.Abs(row2.ContentRect.Y - 110) < 2, $"Row 2 Y=90+20={row2.ContentRect.Y}");
        }

        // [CSS-GRID §7.2] 3 fr rows with row-gap
        [Fact]
        public void FrRows_1fr_1fr_1fr_WithRowGap10_Height210()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;grid-template-rows:1fr 1fr 1fr;row-gap:10px;height:210px;width:100px'>
                    <div id='r1'></div>
                    <div id='r2'></div>
                    <div id='r3'></div>
                </div></body>");
            float expectedRowHeight = (210 - 20) / 3f;
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r1")!.ContentRect.Y - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r2")!.ContentRect.Y - (expectedRowHeight + 10)) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r3")!.ContentRect.Y - (2 * expectedRowHeight + 20)) < 2);
        }

        // [CSS-GRID §8.2] 2-column grid: auto rows sized by tallest item in row
        [Fact]
        public void TwoColumnGrid_AutoRowHeight_TallestItemWins()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px 100px;width:200px'>
                    <div id='a' style='height:30px'></div>
                    <div id='b' style='height:70px'></div>
                    <div id='c' style='height:20px'></div>
                </div></body>");
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            Assert.True(System.Math.Abs(itemC.ContentRect.Y - 70) < 2, $"Row 2 Y matches tallest in row 1={itemC.ContentRect.Y}");
        }

        // [CSS-GRID §8.2] 3-column grid auto rows: tallest in each row determines row height
        [Fact]
        public void ThreeColumnGrid_AutoRowHeight_TallestItemInEachRow()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:80px 80px 80px;width:240px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:50px'></div>
                    <div id='c' style='height:30px'></div>
                    <div id='d' style='height:10px'></div>
                </div></body>");
            var itemD = LayoutTestHelper.FindById(root, "d")!;
            Assert.True(System.Math.Abs(itemD.ContentRect.Y - 50) < 2, $"Row 2 Y={itemD.ContentRect.Y}");
        }

        // [CSS-GRID §7.2] fixed + fr + fixed rows with height
        [Fact]
        public void MixedRows_30_1fr_2fr_30_Height300()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;grid-template-rows:30px 1fr 2fr 30px;height:300px;width:100px'>
                    <div id='r1'></div>
                    <div id='r2'></div>
                    <div id='r3'></div>
                    <div id='r4'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r1")!.ContentRect.Y - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r2")!.ContentRect.Y - 30) < 2);
            float frSpace = 300 - 30 - 30;
            float oneFr = frSpace / 3f;
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r3")!.ContentRect.Y - (30 + oneFr)) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r4")!.ContentRect.Y - 270) < 2);
        }
    }
}
