using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class WptGridFlexCombinedTests
    {
        private readonly ITestOutputHelper _output;

        public WptGridFlexCombinedTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void FlexContainerInsideGridItem_FillsGridCell()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:200px;width:200px'>
                    <div id='t' style='display:flex;height:60px'>
                        <div id='a' style='flex:1;height:60px'></div>
                        <div id='b' style='flex:1;height:60px'></div>
                    </div>
                </div></body>");
            var flexContainer = LayoutTestHelper.FindById(root, "t")!;
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            Assert.True(System.Math.Abs(flexContainer.ContentRect.Width - 200) < 2,
                $"flex container fills grid cell width=200 (got {flexContainer.ContentRect.Width})");
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 100) < 2,
                $"flex item a=100 (got {itemA.ContentRect.Width})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 100) < 2,
                $"flex item b=100 (got {itemB.ContentRect.Width})");
        }

        [Fact]
        public void GridInsideFlexItem_ReceivesFlexItemWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div style='flex:1'>
                        <div id='t' style='display:grid;grid-template-columns:1fr 1fr'>
                            <div id='a' style='height:20px'></div>
                            <div id='b' style='height:20px'></div>
                        </div>
                    </div>
                    <div style='flex:1;height:40px'></div>
                </div></body>");
            var gridContainer = LayoutTestHelper.FindById(root, "t")!;
            var columnA = LayoutTestHelper.FindById(root, "a")!;
            var columnB = LayoutTestHelper.FindById(root, "b")!;
            Assert.True(System.Math.Abs(gridContainer.ContentRect.Width - 150) < 2,
                $"grid fills flex item width=150 (got {gridContainer.ContentRect.Width})");
            Assert.True(System.Math.Abs(columnA.ContentRect.Width - 75) < 2,
                $"grid col a=75 (got {columnA.ContentRect.Width})");
            Assert.True(System.Math.Abs(columnB.ContentRect.Width - 75) < 2,
                $"grid col b=75 (got {columnB.ContentRect.Width})");
        }

        [Fact]
        public void FlexInsideGridInsideFlex_ThreeLevelNesting()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div style='flex:1'>
                        <div style='display:grid;grid-template-columns:1fr 1fr'>
                            <div id='t' style='display:flex'>
                                <div id='inner' style='flex:1;height:20px'></div>
                            </div>
                            <div style='height:20px'></div>
                        </div>
                    </div>
                </div></body>");
            var innerFlex = LayoutTestHelper.FindById(root, "inner")!;
            Assert.True(System.Math.Abs(innerFlex.ContentRect.Width - 200) < 2,
                $"inner flex item fills grid cell=200 (got {innerFlex.ContentRect.Width})");
        }

        [Fact]
        public void GridOfFlexContainers_EachCellHasFlexLayout()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:200px 200px;width:400px'>
                    <div style='display:flex'>
                        <div id='a1' style='flex:1;height:30px'></div>
                        <div id='a2' style='flex:2;height:30px'></div>
                    </div>
                    <div style='display:flex'>
                        <div id='b1' style='flex:3;height:30px'></div>
                        <div id='b2' style='flex:1;height:30px'></div>
                    </div>
                </div></body>");
            var cellA1 = LayoutTestHelper.FindById(root, "a1")!;
            var cellA2 = LayoutTestHelper.FindById(root, "a2")!;
            var cellB1 = LayoutTestHelper.FindById(root, "b1")!;
            var cellB2 = LayoutTestHelper.FindById(root, "b2")!;
            float expectedA1 = 200f / 3f;
            float expectedA2 = 400f / 3f;
            Assert.True(System.Math.Abs(cellA1.ContentRect.Width - expectedA1) < 2,
                $"a1 flex:1 of 200 = ~66.7 (got {cellA1.ContentRect.Width})");
            Assert.True(System.Math.Abs(cellA2.ContentRect.Width - expectedA2) < 2,
                $"a2 flex:2 of 200 = ~133.3 (got {cellA2.ContentRect.Width})");
            Assert.True(System.Math.Abs(cellB1.ContentRect.Width - 150) < 2,
                $"b1 flex:3 of 200 = 150 (got {cellB1.ContentRect.Width})");
            Assert.True(System.Math.Abs(cellB2.ContentRect.Width - 50) < 2,
                $"b2 flex:1 of 200 = 50 (got {cellB2.ContentRect.Width})");
        }

        [Fact]
        public void FlexRowOfGridItems_GridsShareFlexSpace()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='g1' style='flex:1;display:grid;grid-template-columns:1fr 1fr'>
                        <div id='c1' style='height:30px'></div>
                        <div id='c2' style='height:30px'></div>
                    </div>
                    <div id='g2' style='flex:1;display:grid;grid-template-columns:1fr 1fr 1fr'>
                        <div id='d1' style='height:30px'></div>
                        <div id='d2' style='height:30px'></div>
                        <div id='d3' style='height:30px'></div>
                    </div>
                </div></body>");
            var grid1 = LayoutTestHelper.FindById(root, "g1")!;
            var grid2 = LayoutTestHelper.FindById(root, "g2")!;
            Assert.True(System.Math.Abs(grid1.ContentRect.Width - 200) < 2,
                $"grid1 flex:1 = 200 (got {grid1.ContentRect.Width})");
            Assert.True(System.Math.Abs(grid2.ContentRect.Width - 200) < 2,
                $"grid2 flex:1 = 200 (got {grid2.ContentRect.Width})");
            var col1 = LayoutTestHelper.FindById(root, "c1")!;
            var col2 = LayoutTestHelper.FindById(root, "c2")!;
            Assert.True(System.Math.Abs(col1.ContentRect.Width - 100) < 2,
                $"c1 = 100 (got {col1.ContentRect.Width})");
            Assert.True(System.Math.Abs(col2.ContentRect.Width - 100) < 2,
                $"c2 = 100 (got {col2.ContentRect.Width})");
            float expectedTrackWidth = 200f / 3f;
            var track1 = LayoutTestHelper.FindById(root, "d1")!;
            Assert.True(System.Math.Abs(track1.ContentRect.Width - expectedTrackWidth) < 2,
                $"d1 = ~66.7 (got {track1.ContentRect.Width})");
        }

        [Fact]
        public void ColumnFlexWithGridItems_StacksVertically()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;width:300px'>
                    <div id='top' style='display:grid;grid-template-columns:1fr 1fr;height:60px'>
                        <div style='height:60px'></div>
                        <div style='height:60px'></div>
                    </div>
                    <div id='bottom' style='display:grid;grid-template-columns:1fr 1fr 1fr;height:40px'>
                        <div style='height:40px'></div>
                        <div style='height:40px'></div>
                        <div style='height:40px'></div>
                    </div>
                </div></body>");
            var topGrid = LayoutTestHelper.FindById(root, "top")!;
            var bottomGrid = LayoutTestHelper.FindById(root, "bottom")!;
            Assert.True(System.Math.Abs(topGrid.ContentRect.Y - 0) < 2,
                $"top grid at Y=0 (got {topGrid.ContentRect.Y})");
            Assert.True(System.Math.Abs(bottomGrid.ContentRect.Y - 60) < 2,
                $"bottom grid at Y=60 (got {bottomGrid.ContentRect.Y})");
            Assert.True(System.Math.Abs(topGrid.ContentRect.Width - 300) < 2,
                $"top grid fills width=300 (got {topGrid.ContentRect.Width})");
            Assert.True(System.Math.Abs(bottomGrid.ContentRect.Width - 300) < 2,
                $"bottom grid fills width=300 (got {bottomGrid.ContentRect.Width})");
        }

        [Fact]
        public void FlexContainerAsGridItem_StretchesToCellHeight()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:200px 200px;grid-template-rows:100px;width:400px'>
                    <div id='t' style='display:flex;align-items:center'>
                        <div id='child' style='width:50px;height:30px'></div>
                    </div>
                    <div style='height:100px'></div>
                </div></body>");
            var flexContainer = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(flexContainer.ContentRect.Height - 100) < 2,
                $"flex container stretches to grid row height=100 (got {flexContainer.ContentRect.Height})");
        }

        [Fact]
        public void GridItemAsFlexContainer_FillsCell()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:300px;width:300px'>
                    <div id='t' style='display:flex;justify-content:space-between'>
                        <div id='left' style='width:80px;height:40px'></div>
                        <div id='right' style='width:80px;height:40px'></div>
                    </div>
                </div></body>");
            var flexCell = LayoutTestHelper.FindById(root, "t")!;
            var leftItem = LayoutTestHelper.FindById(root, "left")!;
            var rightItem = LayoutTestHelper.FindById(root, "right")!;
            Assert.True(System.Math.Abs(flexCell.ContentRect.Width - 300) < 2,
                $"flex fills cell=300 (got {flexCell.ContentRect.Width})");
            Assert.True(System.Math.Abs(leftItem.ContentRect.X - 0) < 2,
                $"left at X=0 (got {leftItem.ContentRect.X})");
            Assert.True(System.Math.Abs(rightItem.ContentRect.X - 220) < 2,
                $"right at X=220 (got {rightItem.ContentRect.X})");
        }

        [Fact]
        public void NestedFlexAndGrid_ThreeLevels_ProperSizing()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:1fr 1fr;width:400px'>
                    <div style='display:flex;flex-direction:column'>
                        <div style='display:grid;grid-template-columns:1fr 1fr'>
                            <div id='t' style='height:30px'></div>
                            <div style='height:30px'></div>
                        </div>
                    </div>
                    <div style='height:30px'></div>
                </div></body>");
            var deepItem = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(deepItem.ContentRect.Width - 100) < 2,
                $"deep grid cell = 100 (got {deepItem.ContentRect.Width})");
        }

        [Fact]
        public void FlexGrowInsideGridCell_DistributesSpace()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:300px;width:300px'>
                    <div style='display:flex'>
                        <div id='fixed' style='width:100px;height:40px'></div>
                        <div id='grow' style='flex-grow:1;height:40px'></div>
                    </div>
                </div></body>");
            var fixedItem = LayoutTestHelper.FindById(root, "fixed")!;
            var growItem = LayoutTestHelper.FindById(root, "grow")!;
            Assert.True(System.Math.Abs(fixedItem.ContentRect.Width - 100) < 2,
                $"fixed=100 (got {fixedItem.ContentRect.Width})");
            Assert.True(System.Math.Abs(growItem.ContentRect.Width - 200) < 2,
                $"grow fills remaining=200 (got {growItem.ContentRect.Width})");
        }

        [Fact]
        public void GridFrInsideFlexItem_TracksResolveToFlexWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:360px'>
                    <div style='flex:0 0 240px'>
                        <div id='t' style='display:grid;grid-template-columns:1fr 2fr'>
                            <div id='narrow' style='height:30px'></div>
                            <div id='wide' style='height:30px'></div>
                        </div>
                    </div>
                    <div style='flex:1;height:30px'></div>
                </div></body>");
            var narrowTrack = LayoutTestHelper.FindById(root, "narrow")!;
            var wideTrack = LayoutTestHelper.FindById(root, "wide")!;
            Assert.True(System.Math.Abs(narrowTrack.ContentRect.Width - 80) < 2,
                $"1fr of 240 = 80 (got {narrowTrack.ContentRect.Width})");
            Assert.True(System.Math.Abs(wideTrack.ContentRect.Width - 160) < 2,
                $"2fr of 240 = 160 (got {wideTrack.ContentRect.Width})");
        }

        [Fact]
        public void SidebarMainLayout_GridOuterFlexInner()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px 1fr;width:400px'>
                    <div id='sidebar' style='height:80px'></div>
                    <div id='main' style='display:flex'>
                        <div id='content' style='flex:1;height:80px'></div>
                        <div id='aside' style='width:60px;height:80px'></div>
                    </div>
                </div></body>");
            var sidebar = LayoutTestHelper.FindById(root, "sidebar")!;
            var mainArea = LayoutTestHelper.FindById(root, "main")!;
            var contentArea = LayoutTestHelper.FindById(root, "content")!;
            var asideArea = LayoutTestHelper.FindById(root, "aside")!;
            Assert.True(System.Math.Abs(sidebar.ContentRect.Width - 100) < 2,
                $"sidebar=100 (got {sidebar.ContentRect.Width})");
            Assert.True(System.Math.Abs(mainArea.ContentRect.Width - 300) < 2,
                $"main=300 (got {mainArea.ContentRect.Width})");
            Assert.True(System.Math.Abs(contentArea.ContentRect.Width - 240) < 2,
                $"content=240 (got {contentArea.ContentRect.Width})");
            Assert.True(System.Math.Abs(asideArea.ContentRect.Width - 60) < 2,
                $"aside=60 (got {asideArea.ContentRect.Width})");
        }

        [Fact]
        public void DashboardPattern_GridForCardsFlexForContent()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:1fr 1fr;gap:20px;width:400px'>
                    <div id='card1' style='display:flex;flex-direction:column'>
                        <div id='header1' style='height:30px'></div>
                        <div id='body1' style='flex:1;height:50px'></div>
                    </div>
                    <div id='card2' style='display:flex;flex-direction:column'>
                        <div id='header2' style='height:30px'></div>
                        <div id='body2' style='flex:1;height:70px'></div>
                    </div>
                </div></body>");
            var card1 = LayoutTestHelper.FindById(root, "card1")!;
            var card2 = LayoutTestHelper.FindById(root, "card2")!;
            float expectedCardWidth = (400 - 20) / 2f;
            Assert.True(System.Math.Abs(card1.ContentRect.Width - expectedCardWidth) < 2,
                $"card1=190 (got {card1.ContentRect.Width})");
            Assert.True(System.Math.Abs(card2.ContentRect.Width - expectedCardWidth) < 2,
                $"card2=190 (got {card2.ContentRect.Width})");
            Assert.True(System.Math.Abs(card2.ContentRect.X - (expectedCardWidth + 20)) < 2,
                $"card2 at X=210 (got {card2.ContentRect.X})");
        }

        [Fact]
        public void FlexAndGridWithGap_GapsIndependent()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:1fr;row-gap:10px;width:300px'>
                    <div id='row1' style='display:flex;gap:20px;height:40px'>
                        <div id='a' style='flex:1;height:40px'></div>
                        <div id='b' style='flex:1;height:40px'></div>
                    </div>
                    <div id='row2' style='height:40px'></div>
                </div></body>");
            var row1 = LayoutTestHelper.FindById(root, "row1")!;
            var row2 = LayoutTestHelper.FindById(root, "row2")!;
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            float gridGap = row2.ContentRect.Y - (row1.ContentRect.Y + row1.ContentRect.Height);
            Assert.True(System.Math.Abs(gridGap - 10) < 2,
                $"grid row-gap=10 (got {gridGap})");
            float expectedFlexItemWidth = (300 - 20) / 2f;
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - expectedFlexItemWidth) < 2,
                $"flex item a=140 (got {itemA.ContentRect.Width})");
            float flexGap = itemB.ContentRect.X - (itemA.ContentRect.X + itemA.ContentRect.Width);
            Assert.True(System.Math.Abs(flexGap - 20) < 2,
                $"flex gap=20 (got {flexGap})");
        }

        [Fact]
        public void FlexAndGridWithAlignment_CenterBoth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:200px;grid-template-rows:100px;
                            align-items:center;justify-items:center;width:200px'>
                    <div id='t' style='display:flex;align-items:center;justify-content:center;
                                       width:120px;height:60px'>
                        <div id='child' style='width:40px;height:20px'></div>
                    </div>
                </div></body>");
            var flexContainer = LayoutTestHelper.FindById(root, "t")!;
            var childItem = LayoutTestHelper.FindById(root, "child")!;
            Assert.True(System.Math.Abs(flexContainer.ContentRect.X - 40) < 2,
                $"flex centered X=40 (got {flexContainer.ContentRect.X})");
            Assert.True(System.Math.Abs(flexContainer.ContentRect.Y - 20) < 2,
                $"flex centered Y=20 (got {flexContainer.ContentRect.Y})");
            float childRelativeX = childItem.ContentRect.X - flexContainer.ContentRect.X;
            Assert.True(System.Math.Abs(childRelativeX - 40) < 2,
                $"child centered in flex X=40 (got {childRelativeX})");
        }

        [Fact]
        public void FlexWrapInsideGridCell_WrapsWithinCellWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:160px;width:160px'>
                    <div style='display:flex;flex-wrap:wrap'>
                        <div id='a' style='width:100px;height:30px'></div>
                        <div id='b' style='width:100px;height:30px'></div>
                    </div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.Y - 0) < 2,
                $"a on first line Y=0 (got {itemA.ContentRect.Y})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Y - 30) < 2,
                $"b wraps to Y=30 (got {itemB.ContentRect.Y})");
        }

        [Fact]
        public void GridInsideFlexGrow_ExpandsWithFlexItem()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div style='width:100px;height:50px'></div>
                    <div style='flex-grow:1'>
                        <div id='t' style='display:grid;grid-template-columns:1fr 1fr'>
                            <div id='left' style='height:50px'></div>
                            <div id='right' style='height:50px'></div>
                        </div>
                    </div>
                </div></body>");
            var gridContainer = LayoutTestHelper.FindById(root, "t")!;
            var leftColumn = LayoutTestHelper.FindById(root, "left")!;
            var rightColumn = LayoutTestHelper.FindById(root, "right")!;
            Assert.True(System.Math.Abs(gridContainer.ContentRect.Width - 300) < 2,
                $"grid expands to flex-grow width=300 (got {gridContainer.ContentRect.Width})");
            Assert.True(System.Math.Abs(leftColumn.ContentRect.Width - 150) < 2,
                $"left=150 (got {leftColumn.ContentRect.Width})");
            Assert.True(System.Math.Abs(rightColumn.ContentRect.Width - 150) < 2,
                $"right=150 (got {rightColumn.ContentRect.Width})");
        }

        [Fact]
        public void FlexColumnInsideGridRow_HeightFromContent()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:200px;width:200px'>
                    <div id='t' style='display:flex;flex-direction:column'>
                        <div id='first' style='height:40px'></div>
                        <div id='second' style='height:60px'></div>
                    </div>
                </div></body>");
            var flexColumn = LayoutTestHelper.FindById(root, "t")!;
            var firstItem = LayoutTestHelper.FindById(root, "first")!;
            var secondItem = LayoutTestHelper.FindById(root, "second")!;
            Assert.True(System.Math.Abs(flexColumn.ContentRect.Height - 100) < 2,
                $"column flex height=100 (got {flexColumn.ContentRect.Height})");
            Assert.True(System.Math.Abs(firstItem.ContentRect.Y - 0) < 2,
                $"first at Y=0 (got {firstItem.ContentRect.Y})");
            Assert.True(System.Math.Abs(secondItem.ContentRect.Y - 40) < 2,
                $"second at Y=40 (got {secondItem.ContentRect.Y})");
        }

        [Fact]
        public void GridAutoRowsWithFlexContent_RowsSizeToContent()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:1fr 1fr;grid-auto-rows:auto;width:300px'>
                    <div id='tall' style='display:flex;flex-direction:column'>
                        <div style='height:40px'></div>
                        <div style='height:40px'></div>
                    </div>
                    <div id='short'></div>
                </div></body>");
            var tallCell = LayoutTestHelper.FindById(root, "tall")!;
            var shortCell = LayoutTestHelper.FindById(root, "short")!;
            Assert.True(tallCell.ContentRect.Height >= 78,
                $"tall cell at least 80 from flex content (got {tallCell.ContentRect.Height})");
            Assert.True(System.Math.Abs(shortCell.ContentRect.Height - tallCell.ContentRect.Height) < 2,
                $"short cell stretches to match tall row (got {shortCell.ContentRect.Height})");
        }

        [Fact]
        public void FlexSpaceBetweenInGridCell_EvenDistribution()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:300px;width:300px'>
                    <div style='display:flex;justify-content:space-between'>
                        <div id='first' style='width:60px;height:30px'></div>
                        <div id='middle' style='width:60px;height:30px'></div>
                        <div id='last' style='width:60px;height:30px'></div>
                    </div>
                </div></body>");
            var firstItem = LayoutTestHelper.FindById(root, "first")!;
            var middleItem = LayoutTestHelper.FindById(root, "middle")!;
            var lastItem = LayoutTestHelper.FindById(root, "last")!;
            Assert.True(System.Math.Abs(firstItem.ContentRect.X - 0) < 2,
                $"first at X=0 (got {firstItem.ContentRect.X})");
            Assert.True(System.Math.Abs(lastItem.ContentRect.X - 240) < 2,
                $"last at X=240 (got {lastItem.ContentRect.X})");
            Assert.True(System.Math.Abs(middleItem.ContentRect.X - 120) < 2,
                $"middle at X=120 (got {middleItem.ContentRect.X})");
        }

        [Fact]
        public void GridSpanningItemWithFlexChildren_SpansColumns()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:1fr 1fr;width:400px'>
                    <div id='t' style='grid-column:1/-1;display:flex'>
                        <div id='a' style='flex:1;height:30px'></div>
                        <div id='b' style='flex:1;height:30px'></div>
                        <div id='c' style='flex:1;height:30px'></div>
                    </div>
                </div></body>");
            var spanningItem = LayoutTestHelper.FindById(root, "t")!;
            var flexA = LayoutTestHelper.FindById(root, "a")!;
            Assert.True(System.Math.Abs(spanningItem.ContentRect.Width - 400) < 2,
                $"spanning item=400 (got {spanningItem.ContentRect.Width})");
            float expectedChildWidth = 400f / 3f;
            Assert.True(System.Math.Abs(flexA.ContentRect.Width - expectedChildWidth) < 2,
                $"flex child=~133 (got {flexA.ContentRect.Width})");
        }

        [Fact]
        public void FlexShrinkInsideGridFixedColumn_ShrinksBeyondBasis()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:200px;width:200px'>
                    <div style='display:flex'>
                        <div id='a' style='flex:0 1 150px;height:30px'></div>
                        <div id='b' style='flex:0 1 150px;height:30px'></div>
                    </div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 100) < 2,
                $"a shrinks to 100 (got {itemA.ContentRect.Width})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 100) < 2,
                $"b shrinks to 100 (got {itemB.ContentRect.Width})");
        }

        [Fact]
        public void GridAlignStretchWithFlexChild_FlexGetsFullHeight()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:1fr 1fr;grid-template-rows:120px;width:300px'>
                    <div style='display:flex;align-items:flex-end'>
                        <div id='t' style='width:50px;height:30px'></div>
                    </div>
                    <div style='height:120px'></div>
                </div></body>");
            var child = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(child.ContentRect.Y - 90) < 2,
                $"flex-end in 120px cell, child at Y=90 (got {child.ContentRect.Y})");
        }

        [Fact]
        public void HolyGrailLayout_GridOuterFlexInnerColumns()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-rows:40px 1fr 30px;height:300px;width:400px'>
                    <div id='header' style='height:40px'></div>
                    <div id='middle' style='display:flex'>
                        <div id='nav' style='width:80px'></div>
                        <div id='content' style='flex:1'></div>
                        <div id='ads' style='width:60px'></div>
                    </div>
                    <div id='footer' style='height:30px'></div>
                </div></body>");
            var header = LayoutTestHelper.FindById(root, "header")!;
            var middle = LayoutTestHelper.FindById(root, "middle")!;
            var footer = LayoutTestHelper.FindById(root, "footer")!;
            var nav = LayoutTestHelper.FindById(root, "nav")!;
            var content = LayoutTestHelper.FindById(root, "content")!;
            var ads = LayoutTestHelper.FindById(root, "ads")!;
            Assert.True(System.Math.Abs(header.ContentRect.Height - 40) < 2,
                $"header=40 (got {header.ContentRect.Height})");
            Assert.True(System.Math.Abs(footer.ContentRect.Y - 270) < 2,
                $"footer at Y=270 (got {footer.ContentRect.Y})");
            Assert.True(System.Math.Abs(nav.ContentRect.Width - 80) < 2,
                $"nav=80 (got {nav.ContentRect.Width})");
            Assert.True(System.Math.Abs(content.ContentRect.Width - 260) < 2,
                $"content=260 (got {content.ContentRect.Width})");
            Assert.True(System.Math.Abs(ads.ContentRect.Width - 60) < 2,
                $"ads=60 (got {ads.ContentRect.Width})");
        }

        [Fact]
        public void GridWithFlexAndPercentageWidth_ResolvesProperly()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:1fr;width:400px'>
                    <div style='display:flex'>
                        <div id='t' style='width:50%;height:40px'></div>
                        <div style='flex:1;height:40px'></div>
                    </div>
                </div></body>");
            var percentItem = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(percentItem.ContentRect.Width - 200) < 2,
                $"50% of 400=200 (got {percentItem.ContentRect.Width})");
        }

        [Fact]
        public void FlexWithMinWidthInsideGrid_RespectsMinimum()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:200px;width:200px'>
                    <div style='display:flex'>
                        <div id='t' style='flex:1;min-width:120px;height:30px'></div>
                        <div id='other' style='flex:1;height:30px'></div>
                    </div>
                </div></body>");
            var minItem = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(minItem.ContentRect.Width >= 118,
                $"min-width:120 respected (got {minItem.ContentRect.Width})");
        }

        [Fact]
        public void GridFlexCombined_AlignItemsCenter_BothAxes()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:200px;grid-template-rows:100px;
                            align-items:center;width:200px'>
                    <div style='display:flex;justify-content:center;height:40px'>
                        <div id='t' style='width:60px;height:40px'></div>
                    </div>
                </div></body>");
            var flexContainer = LayoutTestHelper.FindByTag(root, "div");
            var centered = LayoutTestHelper.FindById(root, "t")!;
            float relativeX = centered.ContentRect.X;
            Assert.True(System.Math.Abs(relativeX - 70) < 2,
                $"centered X=70 (got {relativeX})");
        }

        [Fact]
        public void GridTwoColumnFlexCards_EqualHeightRows()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:1fr 1fr;gap:10px;width:400px'>
                    <div id='card1' style='display:flex;flex-direction:column;justify-content:space-between'>
                        <div id='top1' style='height:20px'></div>
                        <div id='bot1' style='height:20px'></div>
                    </div>
                    <div id='card2' style='display:flex;flex-direction:column'>
                        <div style='height:20px'></div>
                        <div style='height:20px'></div>
                        <div style='height:20px'></div>
                    </div>
                </div></body>");
            var card1 = LayoutTestHelper.FindById(root, "card1")!;
            var card2 = LayoutTestHelper.FindById(root, "card2")!;
            Assert.True(System.Math.Abs(card1.ContentRect.Height - card2.ContentRect.Height) < 2,
                $"cards same height: {card1.ContentRect.Height} vs {card2.ContentRect.Height}");
        }

        [Fact]
        public void FlexReverseInsideGrid_ItemsReversed()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:300px;width:300px'>
                    <div style='display:flex;flex-direction:row-reverse'>
                        <div id='first' style='width:80px;height:30px'></div>
                        <div id='second' style='width:80px;height:30px'></div>
                    </div>
                </div></body>");
            var firstItem = LayoutTestHelper.FindById(root, "first")!;
            var secondItem = LayoutTestHelper.FindById(root, "second")!;
            Assert.True(firstItem.ContentRect.X > secondItem.ContentRect.X,
                $"first DOM item at right edge (first.X={firstItem.ContentRect.X}, second.X={secondItem.ContentRect.X})");
            Assert.True(System.Math.Abs(firstItem.ContentRect.X - 220) < 2,
                $"first at X=220 (got {firstItem.ContentRect.X})");
        }

        [Fact]
        public void GridInsideFlexWithBorderBox_SizesCorrectly()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div style='flex:0 0 200px;box-sizing:border-box;padding:10px;border:5px solid'>
                        <div id='t' style='display:grid;grid-template-columns:1fr 1fr'>
                            <div id='a' style='height:20px'></div>
                            <div id='b' style='height:20px'></div>
                        </div>
                    </div>
                </div></body>");
            var gridContainer = LayoutTestHelper.FindById(root, "t")!;
            float expectedContentWidth = 200 - 10 * 2 - 5 * 2;
            Assert.True(System.Math.Abs(gridContainer.ContentRect.Width - expectedContentWidth) < 2,
                $"grid content width=170 (got {gridContainer.ContentRect.Width})");
            var colA = LayoutTestHelper.FindById(root, "a")!;
            Assert.True(System.Math.Abs(colA.ContentRect.Width - expectedContentWidth / 2f) < 2,
                $"col a=85 (got {colA.ContentRect.Width})");
        }
    }
}
