using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    /// <summary>
    /// CSS Grid 3x3 layout position tests covering cell X/Y coordinates, widths, heights,
    /// gaps, fr units, mixed tracks, named areas, percentages, container sizing,
    /// padding, border, and spanning across a 3-column 3-row grid configuration.
    /// </summary>
    public class WptGrid3x3PositionTests
    {
        private readonly ITestOutputHelper _output;

        public WptGrid3x3PositionTests(ITestOutputHelper output)
        {
            _output = output;
        }

        // [CSS-GRID §7.2] 3x3 fixed grid, verify X position for each cell in the grid
        [Fact]
        public void FixedGrid_AllCells_XPositions()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:100px 120px 80px;grid-template-rows:40px 50px 60px;width:300px'><div id='r1c1'></div><div id='r1c2'></div><div id='r1c3'></div><div id='r2c1'></div><div id='r2c2'></div><div id='r2c3'></div><div id='r3c1'></div><div id='r3c2'></div><div id='r3c3'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r1c1")!.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r1c2")!.ContentRect.X - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r1c3")!.ContentRect.X - 220) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r2c1")!.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r2c2")!.ContentRect.X - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r2c3")!.ContentRect.X - 220) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r3c1")!.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r3c2")!.ContentRect.X - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r3c3")!.ContentRect.X - 220) < 2);
        }

        // [CSS-GRID §7.2] 3x3 fixed grid, verify Y position for each cell in the grid
        [Fact]
        public void FixedGrid_AllCells_YPositions()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:100px 120px 80px;grid-template-rows:40px 50px 60px;width:300px'><div id='r1c1'></div><div id='r1c2'></div><div id='r1c3'></div><div id='r2c1'></div><div id='r2c2'></div><div id='r2c3'></div><div id='r3c1'></div><div id='r3c2'></div><div id='r3c3'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r1c1")!.ContentRect.Y - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r1c2")!.ContentRect.Y - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r1c3")!.ContentRect.Y - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r2c1")!.ContentRect.Y - 40) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r2c2")!.ContentRect.Y - 40) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r2c3")!.ContentRect.Y - 40) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r3c1")!.ContentRect.Y - 90) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r3c2")!.ContentRect.Y - 90) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r3c3")!.ContentRect.Y - 90) < 2);
        }

        // [CSS-GRID §11.1] 3x3 fixed grid, each cell width matches its column track width
        [Fact]
        public void FixedGrid_CellWidths_MatchColumnTracks()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:90px 130px 80px;grid-template-rows:40px 40px 40px;width:300px'><div id='r1c1'></div><div id='r1c2'></div><div id='r1c3'></div><div id='r2c1'></div><div id='r2c2'></div><div id='r2c3'></div><div id='r3c1'></div><div id='r3c2'></div><div id='r3c3'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r1c1")!.ContentRect.Width - 90) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r1c2")!.ContentRect.Width - 130) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r1c3")!.ContentRect.Width - 80) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r2c1")!.ContentRect.Width - 90) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r2c2")!.ContentRect.Width - 130) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r2c3")!.ContentRect.Width - 80) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r3c1")!.ContentRect.Width - 90) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r3c2")!.ContentRect.Width - 130) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r3c3")!.ContentRect.Width - 80) < 2);
        }

        // [CSS-GRID §11.5] 3x3 fixed grid, each cell height matches its row track height
        [Fact]
        public void FixedGrid_CellHeights_MatchRowTracks()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:100px 100px 100px;grid-template-rows:30px 50px 70px;width:300px'><div id='r1c1'></div><div id='r1c2'></div><div id='r1c3'></div><div id='r2c1'></div><div id='r2c2'></div><div id='r2c3'></div><div id='r3c1'></div><div id='r3c2'></div><div id='r3c3'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r1c1")!.ContentRect.Height - 30) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r1c2")!.ContentRect.Height - 30) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r1c3")!.ContentRect.Height - 30) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r2c1")!.ContentRect.Height - 50) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r2c2")!.ContentRect.Height - 50) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r2c3")!.ContentRect.Height - 50) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r3c1")!.ContentRect.Height - 70) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r3c2")!.ContentRect.Height - 70) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r3c3")!.ContentRect.Height - 70) < 2);
        }

        // [CSS-GRID §10.1] 3x3 with gap, cell positions offset by gap in both axes
        [Fact]
        public void WithGap_CellPositions_IncludeGapOffsets()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:80px 80px 80px;grid-template-rows:40px 40px 40px;gap:10px;width:260px'><div id='r1c1'></div><div id='r1c2'></div><div id='r1c3'></div><div id='r2c1'></div><div id='r2c2'></div><div id='r2c3'></div><div id='r3c1'></div><div id='r3c2'></div><div id='r3c3'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r1c1")!.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r1c2")!.ContentRect.X - 90) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r1c3")!.ContentRect.X - 180) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r2c1")!.ContentRect.Y - 50) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r2c2")!.ContentRect.X - 90) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r2c2")!.ContentRect.Y - 50) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r3c1")!.ContentRect.Y - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r3c3")!.ContentRect.X - 180) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r3c3")!.ContentRect.Y - 100) < 2);
        }

        // [CSS-GRID §7.2] 3x3 with different column widths, verify second and third column offsets
        [Fact]
        public void DifferentColumnWidths_PositionsAccumulate()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:50px 150px 100px;grid-template-rows:60px 60px 60px;width:300px'><div id='r1c1'></div><div id='r1c2'></div><div id='r1c3'></div><div id='r2c1'></div><div id='r2c2'></div><div id='r2c3'></div><div id='r3c1'></div><div id='r3c2'></div><div id='r3c3'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r1c1")!.ContentRect.Width - 50) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r1c2")!.ContentRect.X - 50) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r1c2")!.ContentRect.Width - 150) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r1c3")!.ContentRect.X - 200) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r1c3")!.ContentRect.Width - 100) < 2);
        }

        // [CSS-GRID §11.5] 3x3 with different row heights, verify second and third row offsets
        [Fact]
        public void DifferentRowHeights_PositionsAccumulate()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:100px 100px 100px;grid-template-rows:25px 45px 80px;width:300px'><div id='r1c1'></div><div id='r1c2'></div><div id='r1c3'></div><div id='r2c1'></div><div id='r2c2'></div><div id='r2c3'></div><div id='r3c1'></div><div id='r3c2'></div><div id='r3c3'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r1c1")!.ContentRect.Height - 25) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r2c1")!.ContentRect.Y - 25) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r2c1")!.ContentRect.Height - 45) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r3c1")!.ContentRect.Y - 70) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r3c1")!.ContentRect.Height - 80) < 2);
        }

        // [CSS-GRID §7.2] 3x3 with equal fr columns distributes container width equally
        [Fact]
        public void FrColumns_EqualThreeWaySplit()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:1fr 1fr 1fr;grid-template-rows:40px 40px 40px;width:300px'><div id='r1c1'></div><div id='r1c2'></div><div id='r1c3'></div><div id='r2c1'></div><div id='r2c2'></div><div id='r2c3'></div><div id='r3c1'></div><div id='r3c2'></div><div id='r3c3'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r1c1")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r1c2")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r1c3")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r1c2")!.ContentRect.X - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r1c3")!.ContentRect.X - 200) < 2);
        }

        // [CSS-GRID §11.5] 3x3 with equal fr rows and explicit height distributes evenly
        [Fact]
        public void FrRows_EqualThreeWaySplit_WithExplicitHeight()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:100px 100px 100px;grid-template-rows:1fr 1fr 1fr;width:300px;height:210px'><div id='r1c1'></div><div id='r1c2'></div><div id='r1c3'></div><div id='r2c1'></div><div id='r2c2'></div><div id='r2c3'></div><div id='r3c1'></div><div id='r3c2'></div><div id='r3c3'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r1c1")!.ContentRect.Height - 70) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r2c1")!.ContentRect.Height - 70) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r3c1")!.ContentRect.Height - 70) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r2c1")!.ContentRect.Y - 70) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r3c1")!.ContentRect.Y - 140) < 2);
        }

        // [CSS-GRID §7.2] 3x3 with mixed fixed+fr columns: fixed takes space first, fr gets remainder
        [Fact]
        public void MixedFixedAndFrColumns_CorrectWidths()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:60px 1fr 2fr;grid-template-rows:50px 50px 50px;width:360px'><div id='r1c1'></div><div id='r1c2'></div><div id='r1c3'></div><div id='r2c1'></div><div id='r2c2'></div><div id='r2c3'></div><div id='r3c1'></div><div id='r3c2'></div><div id='r3c3'></div></div></body>");
            // remaining = 360 - 60 = 300, 1fr = 100, 2fr = 200
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r1c1")!.ContentRect.Width - 60) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r1c2")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r1c3")!.ContentRect.Width - 200) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r1c2")!.ContentRect.X - 60) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r1c3")!.ContentRect.X - 160) < 2);
        }

        // [CSS-GRID §11.5] 3x3 with mixed fixed+fr rows: fixed takes space first, fr gets remainder
        [Fact]
        public void MixedFixedAndFrRows_CorrectHeights()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:100px 100px 100px;grid-template-rows:40px 1fr 2fr;width:300px;height:340px'><div id='r1c1'></div><div id='r1c2'></div><div id='r1c3'></div><div id='r2c1'></div><div id='r2c2'></div><div id='r2c3'></div><div id='r3c1'></div><div id='r3c2'></div><div id='r3c3'></div></div></body>");
            // remaining = 340 - 40 = 300, 1fr = 100, 2fr = 200
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r1c1")!.ContentRect.Height - 40) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r2c1")!.ContentRect.Height - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r3c1")!.ContentRect.Height - 200) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r2c1")!.ContentRect.Y - 40) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r3c1")!.ContentRect.Y - 140) < 2);
        }

        // [CSS-GRID §7.3] 3x3 with named grid-template-areas, verify all 9 area positions
        [Fact]
        public void NamedAreas_NineCells_CorrectPositions()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-areas:""hd hd hd"" ""nav main side"" ""ft ft ft"";grid-template-columns:80px 140px 80px;grid-template-rows:40px 100px 40px;width:300px'><div id='hd' style='grid-area:hd'></div><div id='nav' style='grid-area:nav'></div><div id='main' style='grid-area:main'></div><div id='side' style='grid-area:side'></div><div id='ft' style='grid-area:ft'></div></div></body>");
            // header spans all 3 columns
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "hd")!.ContentRect.Width - 300) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "hd")!.ContentRect.Height - 40) < 2);
            // nav at (0, 40) width 80
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "nav")!.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "nav")!.ContentRect.Y - 40) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "nav")!.ContentRect.Width - 80) < 2);
            // main at (80, 40) width 140
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "main")!.ContentRect.X - 80) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "main")!.ContentRect.Y - 40) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "main")!.ContentRect.Width - 140) < 2);
            // side at (220, 40) width 80
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "side")!.ContentRect.X - 220) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "side")!.ContentRect.Y - 40) < 2);
            // footer spans all 3 columns at row 3
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "ft")!.ContentRect.Width - 300) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "ft")!.ContentRect.Y - 140) < 2);
        }

        // [CSS-GRID §7.2] 3x3 with percentage columns resolves against container width
        [Fact]
        public void PercentageColumns_ResolveAgainstContainerWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:20% 50% 30%;grid-template-rows:40px 40px 40px;width:400px'><div id='r1c1'></div><div id='r1c2'></div><div id='r1c3'></div><div id='r2c1'></div><div id='r2c2'></div><div id='r2c3'></div><div id='r3c1'></div><div id='r3c2'></div><div id='r3c3'></div></div></body>");
            // 20% of 400 = 80, 50% of 400 = 200, 30% of 400 = 120
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r1c1")!.ContentRect.Width - 80) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r1c2")!.ContentRect.Width - 200) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r1c3")!.ContentRect.Width - 120) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r1c2")!.ContentRect.X - 80) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r1c3")!.ContentRect.X - 280) < 2);
        }

        // [CSS-GRID §12.4] 3x3 container height equals sum of all row tracks
        [Fact]
        public void ContainerHeight_SumOfThreeRowTracks()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='grid' style='display:grid;grid-template-columns:100px 100px 100px;grid-template-rows:35px 45px 70px;width:300px'><div></div><div></div><div></div><div></div><div></div><div></div><div></div><div></div><div></div></div></body>");
            // 35 + 45 + 70 = 150
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "grid")!.ContentRect.Height - 150) < 2);
        }

        // [CSS-GRID §12.4] 3x3 container height includes two row-gaps
        [Fact]
        public void ContainerHeight_IncludesTwoRowGaps()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='grid' style='display:grid;grid-template-columns:100px 100px 100px;grid-template-rows:30px 30px 30px;row-gap:15px;width:300px'><div></div><div></div><div></div><div></div><div></div><div></div><div></div><div></div><div></div></div></body>");
            // 30 + 15 + 30 + 15 + 30 = 120
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "grid")!.ContentRect.Height - 120) < 2);
        }

        // [CSS-GRID §7.2] 3x3 with padding on container, cells offset by padding
        [Fact]
        public void ContainerPadding_OffsetsCellPositions()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:80px 80px 80px;grid-template-rows:40px 40px 40px;width:240px;padding:20px'><div id='r1c1'></div><div id='r1c2'></div><div id='r1c3'></div><div id='r2c1'></div><div id='r2c2'></div><div id='r2c3'></div><div id='r3c1'></div><div id='r3c2'></div><div id='r3c3'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r1c1")!.ContentRect.X - 20) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r1c1")!.ContentRect.Y - 20) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r1c2")!.ContentRect.X - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r1c3")!.ContentRect.X - 180) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r3c3")!.ContentRect.X - 180) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r3c3")!.ContentRect.Y - 100) < 2);
        }

        // [CSS-GRID §7.2] 3x3 with border on container, cells offset by border width
        [Fact]
        public void ContainerBorder_OffsetsCellPositions()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:80px 80px 80px;grid-template-rows:40px 40px 40px;width:240px;border:8px solid'><div id='r1c1'></div><div id='r1c2'></div><div id='r1c3'></div><div id='r2c1'></div><div id='r2c2'></div><div id='r2c3'></div><div id='r3c1'></div><div id='r3c2'></div><div id='r3c3'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r1c1")!.ContentRect.X - 8) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r1c1")!.ContentRect.Y - 8) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r1c2")!.ContentRect.X - 88) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r1c3")!.ContentRect.X - 168) < 2);
        }

        // [CSS-GRID §8.3] 3x3 with first row spanning all 3 columns
        [Fact]
        public void ColumnSpan_FirstRow_SpansAllThreeColumns()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:100px 120px 80px;grid-template-rows:40px 50px 60px;width:300px'><div id='header' style='grid-column:span 3'></div><div id='r2c1'></div><div id='r2c2'></div><div id='r2c3'></div><div id='r3c1'></div><div id='r3c2'></div><div id='r3c3'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "header")!.ContentRect.Width - 300) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "header")!.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "header")!.ContentRect.Y - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r2c1")!.ContentRect.Y - 40) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r2c2")!.ContentRect.X - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r2c3")!.ContentRect.X - 220) < 2);
        }

        // [CSS-GRID §7.2] 3x3 with unequal fr columns (1fr 2fr 3fr)
        [Fact]
        public void UnequalFrColumns_ProportionalDistribution()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:1fr 2fr 3fr;grid-template-rows:50px 50px 50px;width:360px'><div id='r1c1'></div><div id='r1c2'></div><div id='r1c3'></div><div id='r2c1'></div><div id='r2c2'></div><div id='r2c3'></div><div id='r3c1'></div><div id='r3c2'></div><div id='r3c3'></div></div></body>");
            // 1fr = 60, 2fr = 120, 3fr = 180
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r1c1")!.ContentRect.Width - 60) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r1c2")!.ContentRect.Width - 120) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r1c3")!.ContentRect.Width - 180) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r1c2")!.ContentRect.X - 60) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r1c3")!.ContentRect.X - 180) < 2);
        }

        // [CSS-GRID §11.5] 3x3 with unequal fr rows (1fr 2fr 1fr), explicit container height
        [Fact]
        public void UnequalFrRows_ProportionalDistribution()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:100px 100px 100px;grid-template-rows:1fr 2fr 1fr;width:300px;height:200px'><div id='r1c1'></div><div id='r1c2'></div><div id='r1c3'></div><div id='r2c1'></div><div id='r2c2'></div><div id='r2c3'></div><div id='r3c1'></div><div id='r3c2'></div><div id='r3c3'></div></div></body>");
            // 1fr = 50, 2fr = 100, 1fr = 50
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r1c1")!.ContentRect.Height - 50) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r2c1")!.ContentRect.Height - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r3c1")!.ContentRect.Height - 50) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r2c1")!.ContentRect.Y - 50) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r3c1")!.ContentRect.Y - 150) < 2);
        }

        // [CSS-GRID §10.1] 3x3 with gap and fr columns, gap subtracted before fr distribution
        [Fact]
        public void FrColumnsWithGap_GapSubtractedFirst()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:1fr 1fr 1fr;grid-template-rows:40px 40px 40px;column-gap:15px;width:330px'><div id='r1c1'></div><div id='r1c2'></div><div id='r1c3'></div><div id='r2c1'></div><div id='r2c2'></div><div id='r2c3'></div><div id='r3c1'></div><div id='r3c2'></div><div id='r3c3'></div></div></body>");
            // available = 330 - 2*15 = 300, each fr = 100
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r1c1")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r1c2")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r1c3")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r1c2")!.ContentRect.X - 115) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r1c3")!.ContentRect.X - 230) < 2);
        }

        // [CSS-GRID §7.2] 3x3 with padding on container reduces fr column widths
        [Fact]
        public void ContainerPadding_ReducesFrColumnWidths()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:1fr 1fr 1fr;grid-template-rows:40px 40px 40px;width:360px;padding:30px'><div id='r1c1'></div><div id='r1c2'></div><div id='r1c3'></div><div id='r2c1'></div><div id='r2c2'></div><div id='r2c3'></div><div id='r3c1'></div><div id='r3c2'></div><div id='r3c3'></div></div></body>");
            // content width = 360, each fr = 120
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r1c1")!.ContentRect.Width - 120) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r1c2")!.ContentRect.Width - 120) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r1c3")!.ContentRect.Width - 120) < 2);
        }

        // [CSS-GRID §7.2] 3x3 with border on container reduces fr column widths
        [Fact]
        public void ContainerBorder_ReducesFrColumnWidths()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:1fr 1fr 1fr;grid-template-rows:40px 40px 40px;width:300px;border:10px solid'><div id='r1c1'></div><div id='r1c2'></div><div id='r1c3'></div><div id='r2c1'></div><div id='r2c2'></div><div id='r2c3'></div><div id='r3c1'></div><div id='r3c2'></div><div id='r3c3'></div></div></body>");
            // content width = 300, each fr = 100
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r1c1")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r1c2")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r1c3")!.ContentRect.Width - 100) < 2);
        }

        // [CSS-GRID §8.3] 3x3 spanning first row with gap, spanned width includes gap
        [Fact]
        public void ColumnSpan_FirstRowWithGap_WidthIncludesGaps()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:80px 100px 80px;grid-template-rows:40px 50px 50px;column-gap:10px;width:280px'><div id='header' style='grid-column:span 3'></div><div id='r2c1'></div><div id='r2c2'></div><div id='r2c3'></div><div id='r3c1'></div><div id='r3c2'></div><div id='r3c3'></div></div></body>");
            // spanning width = 80 + 10 + 100 + 10 + 80 = 280
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "header")!.ContentRect.Width - 280) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r2c1")!.ContentRect.Y - 40) < 2);
        }

        // [CSS-GRID §12.4] 3x3 with explicit container height, items use specified row heights
        [Fact]
        public void ExplicitContainerHeight_FixedRows()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='grid' style='display:grid;grid-template-columns:100px 100px 100px;grid-template-rows:60px 80px 60px;width:300px;height:200px'><div id='r1c1'></div><div id='r1c2'></div><div id='r1c3'></div><div id='r2c1'></div><div id='r2c2'></div><div id='r2c3'></div><div id='r3c1'></div><div id='r3c2'></div><div id='r3c3'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r1c1")!.ContentRect.Height - 60) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r2c1")!.ContentRect.Height - 80) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r3c1")!.ContentRect.Height - 60) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r2c1")!.ContentRect.Y - 60) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r3c1")!.ContentRect.Y - 140) < 2);
        }

        // [CSS-GRID §7.2] 3x3 with gap and different column widths, center cell position
        [Fact]
        public void GapWithDifferentColumns_CenterCellPosition()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:60px 120px 60px;grid-template-rows:30px 60px 30px;gap:10px;width:260px'><div id='r1c1'></div><div id='r1c2'></div><div id='r1c3'></div><div id='r2c1'></div><div id='r2c2'></div><div id='r2c3'></div><div id='r3c1'></div><div id='r3c2'></div><div id='r3c3'></div></div></body>");
            // center cell (r2c2): X = 60 + 10 = 70, Y = 30 + 10 = 40
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r2c2")!.ContentRect.X - 70) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r2c2")!.ContentRect.Y - 40) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r2c2")!.ContentRect.Width - 120) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r2c2")!.ContentRect.Height - 60) < 2);
            // bottom-right cell (r3c3): X = 60+10+120+10 = 200, Y = 30+10+60+10 = 110
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r3c3")!.ContentRect.X - 200) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r3c3")!.ContentRect.Y - 110) < 2);
        }

        // [CSS-GRID §7.3] 3x3 named areas with distinct per-cell placement
        [Fact]
        public void NamedAreas_DistinctPerCell_AllNinePlaced()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-areas:""a b c"" ""d e f"" ""g h i"";grid-template-columns:100px 100px 100px;grid-template-rows:50px 50px 50px;width:300px'><div id='a' style='grid-area:a'></div><div id='b' style='grid-area:b'></div><div id='c' style='grid-area:c'></div><div id='d' style='grid-area:d'></div><div id='e' style='grid-area:e'></div><div id='f' style='grid-area:f'></div><div id='g' style='grid-area:g'></div><div id='h' style='grid-area:h'></div><div id='i' style='grid-area:i'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.Y - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.X - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "c")!.ContentRect.X - 200) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "d")!.ContentRect.Y - 50) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "e")!.ContentRect.X - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "e")!.ContentRect.Y - 50) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "f")!.ContentRect.X - 200) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "f")!.ContentRect.Y - 50) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "g")!.ContentRect.Y - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "h")!.ContentRect.X - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "h")!.ContentRect.Y - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "i")!.ContentRect.X - 200) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "i")!.ContentRect.Y - 100) < 2);
        }

        // [CSS-GRID §7.2] 3x3 with percentage columns that do not sum to 100%
        [Fact]
        public void PercentageColumns_PartialSum_CorrectWidths()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:10% 30% 40%;grid-template-rows:40px 40px 40px;width:500px'><div id='r1c1'></div><div id='r1c2'></div><div id='r1c3'></div><div id='r2c1'></div><div id='r2c2'></div><div id='r2c3'></div><div id='r3c1'></div><div id='r3c2'></div><div id='r3c3'></div></div></body>");
            // 10% of 500 = 50, 30% of 500 = 150, 40% of 500 = 200
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r1c1")!.ContentRect.Width - 50) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r1c2")!.ContentRect.Width - 150) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r1c3")!.ContentRect.Width - 200) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r1c2")!.ContentRect.X - 50) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r1c3")!.ContentRect.X - 200) < 2);
        }

        // [CSS-GRID §7.2] 3x3 with padding and border combined, cells offset by both
        [Fact]
        public void PaddingAndBorder_CombinedOffset()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:80px 80px 80px;grid-template-rows:40px 40px 40px;width:240px;padding:10px;border:5px solid'><div id='r1c1'></div><div id='r1c2'></div><div id='r1c3'></div><div id='r2c1'></div><div id='r2c2'></div><div id='r2c3'></div><div id='r3c1'></div><div id='r3c2'></div><div id='r3c3'></div></div></body>");
            // offset = padding 10 + border 5 = 15
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r1c1")!.ContentRect.X - 15) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r1c1")!.ContentRect.Y - 15) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r1c2")!.ContentRect.X - 95) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r1c3")!.ContentRect.X - 175) < 2);
        }

        // [CSS-GRID §10.1] 3x3 with separate row-gap and column-gap
        [Fact]
        public void SeparateRowAndColumnGap_DifferentSpacing()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:80px 80px 80px;grid-template-rows:40px 40px 40px;row-gap:10px;column-gap:20px;width:280px'><div id='r1c1'></div><div id='r1c2'></div><div id='r1c3'></div><div id='r2c1'></div><div id='r2c2'></div><div id='r2c3'></div><div id='r3c1'></div><div id='r3c2'></div><div id='r3c3'></div></div></body>");
            // column offsets: 0, 80+20=100, 100+80+20=200
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r1c2")!.ContentRect.X - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r1c3")!.ContentRect.X - 200) < 2);
            // row offsets: 0, 40+10=50, 50+40+10=100
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r2c1")!.ContentRect.Y - 50) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r3c1")!.ContentRect.Y - 100) < 2);
        }

        // [CSS-GRID §7.2] 3x3 with mixed percentage and fixed columns
        [Fact]
        public void MixedPercentageAndFixedColumns_CorrectWidths()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:25% 100px 25%;grid-template-rows:40px 40px 40px;width:400px'><div id='r1c1'></div><div id='r1c2'></div><div id='r1c3'></div><div id='r2c1'></div><div id='r2c2'></div><div id='r2c3'></div><div id='r3c1'></div><div id='r3c2'></div><div id='r3c3'></div></div></body>");
            // 25% of 400 = 100, fixed = 100, 25% of 400 = 100
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r1c1")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r1c2")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r1c3")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r1c2")!.ContentRect.X - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r1c3")!.ContentRect.X - 200) < 2);
        }

        // [CSS-GRID §8.3] 3x3 spanning first row with padding on container
        [Fact]
        public void ColumnSpan_FirstRowWithPadding_CorrectWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:80px 80px 80px;grid-template-rows:40px 50px 50px;width:240px;padding:15px'><div id='header' style='grid-column:span 3'></div><div id='r2c1'></div><div id='r2c2'></div><div id='r2c3'></div><div id='r3c1'></div><div id='r3c2'></div><div id='r3c3'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "header")!.ContentRect.Width - 240) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "header")!.ContentRect.X - 15) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "header")!.ContentRect.Y - 15) < 2);
        }

        // [CSS-GRID §12.4] 3x3 container height with gap and padding
        [Fact]
        public void ContainerHeight_WithGapAndPadding()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='grid' style='display:grid;grid-template-columns:100px 100px 100px;grid-template-rows:30px 40px 30px;row-gap:10px;width:300px;padding:20px'><div></div><div></div><div></div><div></div><div></div><div></div><div></div><div></div><div></div></div></body>");
            // content height = 30 + 10 + 40 + 10 + 30 = 120
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "grid")!.ContentRect.Height - 120) < 2);
        }
    }
}
