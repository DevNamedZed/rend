using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    /// <summary>
    /// CSS Grid 2x2 layout tests covering fixed/fr/percentage tracks, gaps, padding,
    /// border, box-sizing, alignment, named areas, spanning, and container sizing
    /// for the fundamental 2-column 2-row grid configuration.
    /// </summary>
    public class WptGrid2x2LayoutTests
    {
        private readonly ITestOutputHelper _output;

        public WptGrid2x2LayoutTests(ITestOutputHelper output)
        {
            _output = output;
        }

        // [CSS-GRID §7.2] 2x2 fixed columns and rows, cell (1,1) at origin
        [Fact]
        public void FixedGrid_CellTopLeft_Position()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:120px 180px;grid-template-rows:50px 70px;width:300px'><div id='a'></div><div id='b'></div><div id='c'></div><div id='d'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.Y - 0) < 2);
        }

        // [CSS-GRID §7.2] 2x2 fixed columns and rows, cell (1,2) at column offset
        [Fact]
        public void FixedGrid_CellTopRight_Position()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:120px 180px;grid-template-rows:50px 70px;width:300px'><div id='a'></div><div id='b'></div><div id='c'></div><div id='d'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.X - 120) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.Y - 0) < 2);
        }

        // [CSS-GRID §7.2] 2x2 fixed columns and rows, cell (2,1) at row offset
        [Fact]
        public void FixedGrid_CellBottomLeft_Position()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:120px 180px;grid-template-rows:50px 70px;width:300px'><div id='a'></div><div id='b'></div><div id='c'></div><div id='d'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "c")!.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "c")!.ContentRect.Y - 50) < 2);
        }

        // [CSS-GRID §7.2] 2x2 fixed columns and rows, cell (2,2) at row+column offset
        [Fact]
        public void FixedGrid_CellBottomRight_Position()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:120px 180px;grid-template-rows:50px 70px;width:300px'><div id='a'></div><div id='b'></div><div id='c'></div><div id='d'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "d")!.ContentRect.X - 120) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "d")!.ContentRect.Y - 50) < 2);
        }

        // [CSS-GRID §7.2] 2x2 with 1fr columns splits container width equally
        [Fact]
        public void FrColumns_EqualSplit()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:1fr 1fr;grid-template-rows:40px 40px;width:300px'><div id='a'></div><div id='b'></div><div id='c'></div><div id='d'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.Width - 150) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.Width - 150) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.X - 150) < 2);
        }

        // [CSS-GRID §7.2] 2x2 with 1fr rows and explicit container height splits evenly
        [Fact]
        public void FrRows_EqualSplit_WithExplicitHeight()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:200px 200px;grid-template-rows:1fr 1fr;width:400px;height:200px'><div id='a'></div><div id='b'></div><div id='c'></div><div id='d'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.Height - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "c")!.ContentRect.Height - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "c")!.ContentRect.Y - 100) < 2);
        }

        // [CSS-GRID §10.1] 2x2 with gap shorthand applies to both axes
        [Fact]
        public void GapBothAxes_OffsetsCells()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:100px 100px;grid-template-rows:50px 50px;gap:20px;width:220px'><div id='a'></div><div id='b'></div><div id='c'></div><div id='d'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.X - 120) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "c")!.ContentRect.Y - 70) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "d")!.ContentRect.X - 120) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "d")!.ContentRect.Y - 70) < 2);
        }

        // [CSS-GRID §10.1] 2x2 with column-gap only, no row-gap
        [Fact]
        public void ColumnGapOnly_NoRowOffset()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:100px 100px;grid-template-rows:40px 40px;column-gap:30px;width:230px'><div id='a'></div><div id='b'></div><div id='c'></div><div id='d'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.X - 130) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "c")!.ContentRect.Y - 40) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "d")!.ContentRect.X - 130) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "d")!.ContentRect.Y - 40) < 2);
        }

        // [CSS-GRID §10.1] 2x2 with row-gap only, no column-gap
        [Fact]
        public void RowGapOnly_NoColumnOffset()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:100px 100px;grid-template-rows:40px 40px;row-gap:20px;width:200px'><div id='a'></div><div id='b'></div><div id='c'></div><div id='d'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.X - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "c")!.ContentRect.Y - 60) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "c")!.ContentRect.X - 0) < 2);
        }

        // [CSS-GRID §11.5] 2x2 with 1fr rows and explicit height distributes remaining space
        [Fact]
        public void FrRows_ExplicitHeight_DistributesSpace()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:150px 150px;grid-template-rows:1fr 1fr;width:300px;height:160px'><div id='a'></div><div id='b'></div><div id='c'></div><div id='d'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.Height - 80) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "c")!.ContentRect.Height - 80) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "c")!.ContentRect.Y - 80) < 2);
        }

        // [CSS-GRID §11.1] 2x2 cell widths match track widths
        [Fact]
        public void CellWidths_MatchTrackWidths()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:80px 220px;grid-template-rows:60px 60px;width:300px'><div id='a'></div><div id='b'></div><div id='c'></div><div id='d'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.Width - 80) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.Width - 220) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "c")!.ContentRect.Width - 80) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "d")!.ContentRect.Width - 220) < 2);
        }

        // [CSS-GRID §11.5] 2x2 cell heights match track heights
        [Fact]
        public void CellHeights_MatchTrackHeights()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:100px 100px;grid-template-rows:35px 65px;width:200px'><div id='a'></div><div id='b'></div><div id='c'></div><div id='d'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.Height - 35) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.Height - 35) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "c")!.ContentRect.Height - 65) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "d")!.ContentRect.Height - 65) < 2);
        }

        // [CSS-GRID §7.2] 2x2 with percentage columns resolves against container width
        [Fact]
        public void PercentageColumns_ResolveAgainstContainerWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:25% 75%;grid-template-rows:40px 40px;width:400px'><div id='a'></div><div id='b'></div><div id='c'></div><div id='d'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.Width - 300) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.X - 100) < 2);
        }

        // [CSS-GRID §7.2] 2x2 mixed fixed column + fr column
        [Fact]
        public void MixedFixedAndFr_ColumnWidths()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:100px 1fr;grid-template-rows:50px 50px;width:400px'><div id='a'></div><div id='b'></div><div id='c'></div><div id='d'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.Width - 300) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "c")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "d")!.ContentRect.Width - 300) < 2);
        }

        // [CSS-GRID §7.2] 2x2 with padding on container reduces available content area
        [Fact]
        public void ContainerPadding_ReducesContentArea()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:1fr 1fr;grid-template-rows:40px 40px;width:400px;padding:20px'><div id='a'></div><div id='b'></div><div id='c'></div><div id='d'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.Width - 200) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.Width - 200) < 2);
        }

        // [CSS-GRID §7.2] 2x2 with border on container reduces available content area
        [Fact]
        public void ContainerBorder_ReducesContentArea()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:1fr 1fr;grid-template-rows:40px 40px;width:400px;border:10px solid'><div id='a'></div><div id='b'></div><div id='c'></div><div id='d'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.Width - 200) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.Width - 200) < 2);
        }

        // [CSS-GRID §7.2] 2x2 container with border-box includes padding+border in width
        [Fact]
        public void ContainerBorderBox_IncludesPaddingBorderInWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:1fr 1fr;grid-template-rows:40px 40px;box-sizing:border-box;width:300px;padding:20px;border:10px solid'><div id='a'></div><div id='b'></div><div id='c'></div><div id='d'></div></div></body>");
            // content width = 300 - 2*(20+10) = 240, each col = 120
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.Width - 120) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.Width - 120) < 2);
        }

        // [CSS-GRID §10.4] 2x2 all cells stretch (default align-items/justify-items)
        [Fact]
        public void AllCellsStretch_DefaultAlignment()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:150px 150px;grid-template-rows:60px 60px;width:300px'><div id='a'></div><div id='b'></div><div id='c'></div><div id='d'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.Width - 150) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.Height - 60) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.Width - 150) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "c")!.ContentRect.Height - 60) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "d")!.ContentRect.Width - 150) < 2);
        }

        // [CSS-GRID §10.4] 2x2 align-items:center vertically centers items with explicit height
        [Fact]
        public void AlignItemsCenter_CentersVertically()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:150px 150px;grid-template-rows:100px 100px;align-items:center;width:300px'><div id='a' style='height:40px'></div><div id='b' style='height:40px'></div><div id='c' style='height:40px'></div><div id='d' style='height:40px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.Y - 30) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.Y - 30) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "c")!.ContentRect.Y - 130) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "d")!.ContentRect.Y - 130) < 2);
        }

        // [CSS-GRID §10.4] 2x2 justify-items:center horizontally centers items with explicit width
        [Fact]
        public void JustifyItemsCenter_CentersHorizontally()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:200px 200px;grid-template-rows:50px 50px;justify-items:center;width:400px'><div id='a' style='width:80px'></div><div id='b' style='width:80px'></div><div id='c' style='width:80px'></div><div id='d' style='width:80px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.X - 60) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.X - 260) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "c")!.ContentRect.X - 60) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "d")!.ContentRect.X - 260) < 2);
        }

        // [CSS-GRID §7.3] 2x2 with named grid-template-areas
        [Fact]
        public void NamedAreas_FourQuadrants()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-areas:""tl tr"" ""bl br"";grid-template-columns:130px 170px;grid-template-rows:60px 80px;width:300px'><div id='tl' style='grid-area:tl'></div><div id='tr' style='grid-area:tr'></div><div id='bl' style='grid-area:bl'></div><div id='br' style='grid-area:br'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "tl")!.ContentRect.Width - 130) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "tr")!.ContentRect.X - 130) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "tr")!.ContentRect.Width - 170) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "bl")!.ContentRect.Y - 60) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "br")!.ContentRect.X - 130) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "br")!.ContentRect.Y - 60) < 2);
        }

        // [CSS-GRID §8.3] 2x2 with column-spanning item in first row
        [Fact]
        public void ColumnSpan_FirstRow_FullWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:120px 180px;grid-template-rows:40px 60px;width:300px'><div id='header' style='grid-column:span 2'></div><div id='c'></div><div id='d'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "header")!.ContentRect.Width - 300) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "c")!.ContentRect.Y - 40) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "d")!.ContentRect.X - 120) < 2);
        }

        // [CSS-GRID §8.3] 2x2 with row-spanning item in first column
        [Fact]
        public void RowSpan_FirstColumn_FullHeight()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:100px 200px;grid-template-rows:50px 50px;width:300px'><div id='side' style='grid-row:span 2'></div><div id='b'></div><div id='d'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "side")!.ContentRect.Height - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "side")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.X - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "d")!.ContentRect.Y - 50) < 2);
        }

        // [CSS-GRID §12.4] 2x2 total container height equals sum of row tracks
        [Fact]
        public void ContainerHeight_SumOfRowTracks()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='grid' style='display:grid;grid-template-columns:100px 100px;grid-template-rows:45px 55px;width:200px'><div></div><div></div><div></div><div></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "grid")!.ContentRect.Height - 100) < 2);
        }

        // [CSS-GRID §12.4] 2x2 container height includes row-gap
        [Fact]
        public void ContainerHeight_IncludesRowGap()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='grid' style='display:grid;grid-template-columns:100px 100px;grid-template-rows:40px 40px;row-gap:20px;width:200px'><div></div><div></div><div></div><div></div></div></body>");
            // 40 + 20 + 40 = 100
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "grid")!.ContentRect.Height - 100) < 2);
        }

        // [CSS-GRID §7.2] 2x2 with unequal fr columns distributes proportionally
        [Fact]
        public void UnequalFrColumns_ProportionalDistribution()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:1fr 3fr;grid-template-rows:50px 50px;width:400px'><div id='a'></div><div id='b'></div><div id='c'></div><div id='d'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.Width - 300) < 2);
        }

        // [CSS-GRID §7.2] 2x2 with fr columns and column-gap subtracts gap before distributing
        [Fact]
        public void FrColumns_WithGap_SubtractsGapFirst()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:1fr 1fr;grid-template-rows:50px 50px;column-gap:20px;width:420px'><div id='a'></div><div id='b'></div><div id='c'></div><div id='d'></div></div></body>");
            // available = 420 - 20 = 400, each fr = 200
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.Width - 200) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.Width - 200) < 2);
        }

        // [CSS-GRID §10.4] 2x2 with place-items:center centers both axes
        [Fact]
        public void PlaceItemsCenter_CentersBothAxes()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:200px 200px;grid-template-rows:100px 100px;place-items:center;width:400px'><div id='a' style='width:60px;height:40px'></div><div id='b' style='width:60px;height:40px'></div><div id='c' style='width:60px;height:40px'></div><div id='d' style='width:60px;height:40px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.X - 70) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.Y - 30) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.X - 270) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "d")!.ContentRect.X - 270) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "d")!.ContentRect.Y - 130) < 2);
        }

        // [CSS-GRID §7.3] 2x2 named areas with spanning header
        [Fact]
        public void NamedAreas_HeaderSpansColumns()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-areas:""header header"" ""nav content"";grid-template-columns:100px 200px;grid-template-rows:50px 100px;width:300px'><div id='header' style='grid-area:header'></div><div id='nav' style='grid-area:nav'></div><div id='content' style='grid-area:content'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "header")!.ContentRect.Width - 300) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "header")!.ContentRect.Height - 50) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "nav")!.ContentRect.Y - 50) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "nav")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "content")!.ContentRect.X - 100) < 2);
        }

        // [CSS-GRID §7.2] 2x2 with padding on container offsets cell positions
        [Fact]
        public void ContainerPadding_OffsetsCellPositions()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:100px 100px;grid-template-rows:40px 40px;width:200px;padding:15px'><div id='a'></div><div id='b'></div><div id='c'></div><div id='d'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.X - 15) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.Y - 15) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.X - 115) < 2);
        }

        // [CSS-GRID §7.2] 2x2 with border on container offsets cell positions
        [Fact]
        public void ContainerBorder_OffsetsCellPositions()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:100px 100px;grid-template-rows:40px 40px;width:200px;border:5px solid'><div id='a'></div><div id='b'></div><div id='c'></div><div id='d'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.X - 5) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.Y - 5) < 2);
        }

        // [CSS-GRID §11.5] 2x2 with unequal fr rows and explicit height
        [Fact]
        public void UnequalFrRows_ExplicitHeight_ProportionalHeights()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:200px 200px;grid-template-rows:1fr 2fr;width:400px;height:300px'><div id='a'></div><div id='b'></div><div id='c'></div><div id='d'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.Height - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "c")!.ContentRect.Height - 200) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "c")!.ContentRect.Y - 100) < 2);
        }
    }
}
