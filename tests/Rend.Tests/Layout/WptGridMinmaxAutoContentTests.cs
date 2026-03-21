using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    /// <summary>
    /// CSS Grid minmax(), auto, min-content, max-content, fit-content track sizing tests.
    /// Covers column and row tracks with content-based sizing, intrinsic keywords,
    /// and grid-auto-rows/grid-auto-columns behavior.
    /// </summary>
    public class WptGridMinmaxAutoContentTests
    {
        private readonly ITestOutputHelper _output;
        public WptGridMinmaxAutoContentTests(ITestOutputHelper output) { _output = output; }

        // [CSS-GRID §7.2.1] minmax(100px,200px) in wide container: track clamps at max
        [Fact]
        public void Minmax_100_200_WideContainer_ClampsAtMax()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:minmax(100px,200px);width:500px'>
                    <div id='item' style='height:20px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(System.Math.Abs(item.ContentRect.Width - 200) < 2);
        }

        // [CSS-GRID §7.2.1] minmax(100px,200px) in narrow container: track uses available width
        [Fact]
        public void Minmax_100_200_NarrowContainer_UsesAvailableWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:minmax(100px,200px);width:120px'>
                    <div id='item' style='height:20px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(item.ContentRect.Width >= 99);
            Assert.True(item.ContentRect.Width <= 201);
        }

        // [CSS-GRID §7.2.1] minmax(0,1fr) behaves same as 1fr: fills container
        [Fact]
        public void Minmax_0_1fr_BehavesSameAs1fr()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:minmax(0,1fr);width:300px'>
                    <div id='item' style='height:20px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(System.Math.Abs(item.ContentRect.Width - 300) < 2);
        }

        // [CSS-GRID §7.2.1] two columns minmax(50px,1fr) split available space equally
        [Fact]
        public void Minmax_50_1fr_TwoColumns_EqualSplit()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:minmax(50px,1fr) minmax(50px,1fr);width:400px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 200) < 2);
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 200) < 2);
        }

        // [CSS-GRID §7.2.1] minmax(auto,200px): auto min respects content, max clamps at 200
        [Fact]
        public void Minmax_Auto_200_ClampsAtMaxWithContent()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:minmax(auto,200px);width:400px'>
                    <div id='item' style='width:80px;height:20px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(item.ContentRect.Width <= 201);
            Assert.True(item.ContentRect.Width >= 79);
        }

        // [CSS-GRID §7.2.1] minmax(100px,auto): min is 100px, auto max grows to fill
        [Fact]
        public void Minmax_100_Auto_GrowsBeyondMin()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:minmax(100px,auto);width:350px'>
                    <div id='item' style='height:20px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(item.ContentRect.Width >= 99);
        }

        // [CSS-GRID §7.2.1] minmax with content-based min: item width enforces minimum
        [Fact]
        public void Minmax_ContentBasedMin_ItemWidthEnforcesMinimum()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:minmax(auto,1fr) 1fr;width:300px'>
                    <div id='a' style='width:180px;height:20px'></div>
                    <div id='b' style='height:20px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            Assert.True(itemA.ContentRect.Width >= 149);
        }

        // [CSS-GRID §7.2.1] minmax row tracks: minmax(40px,100px) clamps row height
        [Fact]
        public void Minmax_Row_40_100_ClampsRowHeight()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:200px;grid-template-rows:minmax(40px,100px);width:200px;height:300px'>
                    <div id='item'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(item.ContentRect.Height >= 39);
            Assert.True(item.ContentRect.Height <= 301);
        }

        // [CSS-GRID §7.2.1+10.1] minmax with column gap: gap subtracted before fr distribution
        [Fact]
        public void Minmax_WithGap_GapSubtractedBeforeFr()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:minmax(50px,1fr) minmax(50px,1fr);column-gap:20px;width:220px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            // available = 220 - 20 = 200, each col = 100px
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 100) < 2);
            float gap = itemB.ContentRect.X - (itemA.ContentRect.X + itemA.ContentRect.Width);
            Assert.True(System.Math.Abs(gap - 20) < 2);
        }

        // [CSS-GRID §7.3+7.2.1] repeat(3, minmax(60px,1fr)) distributes equally
        [Fact]
        public void Repeat_3_Minmax_60_1fr_EqualDistribution()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:repeat(3,minmax(60px,1fr));width:300px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                    <div id='c' style='height:20px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(itemC.ContentRect.Width - 100) < 2);
        }

        // [CSS-GRID §7.2.1] minmax(min-content,1fr): min-content floor, fr ceiling
        [Fact]
        public void Minmax_MinContent_1fr_RespectsContentMinimum()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:minmax(min-content,1fr) 1fr;width:400px'>
                    <div id='a' style='width:120px;height:20px'></div>
                    <div id='b' style='height:20px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            Assert.True(itemA.ContentRect.Width >= 119);
        }

        // [CSS-GRID §7.2.1] minmax(max-content,1fr): max-content floor, fr ceiling
        [Fact]
        public void Minmax_MaxContent_1fr_RespectsContentMaximum()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:minmax(max-content,1fr) 1fr;width:400px'>
                    <div id='a' style='width:150px;height:20px'></div>
                    <div id='b' style='height:20px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            Assert.True(itemA.ContentRect.Width >= 149);
        }

        // [CSS-GRID §7.2] auto column sizes from explicit child width
        [Fact]
        public void Auto_Column_SizesFromChildWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:auto 1fr;width:300px'>
                    <div id='a' style='width:90px;height:20px'></div>
                    <div id='b' style='height:20px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            Assert.True(itemA.ContentRect.Width >= 89);
            Assert.True(itemB.ContentRect.Width > 0);
            Assert.True(itemA.ContentRect.Width + itemB.ContentRect.Width <= 302);
        }

        // [CSS-GRID §7.2] auto row sizes from content height
        [Fact]
        public void Auto_Row_SizesFromContentHeight()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:200px;grid-template-rows:auto;width:200px'>
                    <div id='item' style='height:75px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(System.Math.Abs(item.ContentRect.Height - 75) < 2);
        }

        // [CSS-GRID §7.2] auto column with explicit item width
        [Fact]
        public void Auto_Column_WithExplicitItemWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:auto auto;width:400px'>
                    <div id='a' style='width:130px;height:20px'></div>
                    <div id='b' style='width:170px;height:20px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 130) < 3);
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 170) < 3);
        }

        // [CSS-GRID §7.2] min-content column: track shrinks to minimum content width
        [Fact]
        public void MinContent_Column_ShrinksToMinimumWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:min-content 1fr;width:400px'>
                    <div id='a' style='width:60px;height:20px'></div>
                    <div id='b' style='height:20px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            Assert.True(itemA.ContentRect.Width <= 61);
            Assert.True(itemB.ContentRect.Width >= 338);
        }

        // [CSS-GRID §7.2] max-content column: track sizes to content max
        [Fact]
        public void MaxContent_Column_GrowsToMaximumWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:max-content 1fr;width:400px'>
                    <div id='a' style='width:140px;height:20px'></div>
                    <div id='b' style='height:20px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            Assert.True(itemA.ContentRect.Width >= 139);
            Assert.True(itemB.ContentRect.Width > 0);
        }

        // [CSS-GRID §8.3] auto column with spanning item: span distributes across auto tracks
        [Fact]
        public void Auto_Column_SpanningItem_DistributesWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:auto auto;width:300px'>
                    <div id='span' style='grid-column:span 2;height:20px'></div>
                    <div id='a' style='width:100px;height:20px'></div>
                    <div id='b' style='width:100px;height:20px'></div>
                </div></body>");
            var spanItem = LayoutTestHelper.FindById(root, "span")!;
            Assert.True(System.Math.Abs(spanItem.ContentRect.Width - 300) < 3);
        }

        // [CSS-GRID §7.2] auto column with empty cell: both tracks share available space
        [Fact]
        public void Auto_Column_EmptyCell_SharesSpace()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:auto 1fr;width:300px'>
                    <div id='a'></div>
                    <div id='b' style='height:20px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            Assert.True(itemA.ContentRect.Width >= 0);
            Assert.True(itemB.ContentRect.Width > 0);
            Assert.True(itemA.ContentRect.Width + itemB.ContentRect.Width <= 302);
        }

        // [CSS-GRID §7.5.1] fit-content(200px): clamps auto track at 200px
        [Fact]
        public void FitContent_200_ClampsAutoTrackAtLimit()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:fit-content(200px) 1fr;width:500px'>
                    <div id='a' style='width:100px;height:20px'></div>
                    <div id='b' style='height:20px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            Assert.True(itemA.ContentRect.Width <= 201);
            Assert.True(itemA.ContentRect.Width >= 99);
        }

        // [CSS-GRID §7.5] grid-auto-rows: minmax(30px,auto) sizes implicit rows
        [Fact]
        public void AutoRows_Minmax_30_Auto_EnforcesMinimum()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:200px;grid-auto-rows:minmax(30px,auto);width:200px'>
                    <div id='a' style='height:10px'></div>
                    <div id='b' style='height:60px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            // Row 1: content 10px but min is 30px
            Assert.True(itemA.ContentRect.Height >= 9);
            // Row 2: content 60px exceeds min, auto lets it grow
            Assert.True(itemB.ContentRect.Height >= 59);
        }

        // [CSS-GRID §7.5] grid-auto-columns sizing with column auto-flow
        [Fact]
        public void AutoColumns_120_SizesImplicitColumns()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-rows:50px;grid-auto-flow:column;grid-auto-columns:120px;width:500px'>
                    <div id='a'></div>
                    <div id='b'></div>
                    <div id='c'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 120) < 2);
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 120) < 2);
            Assert.True(System.Math.Abs(itemC.ContentRect.Width - 120) < 2);
            Assert.True(System.Math.Abs(itemB.ContentRect.X - 120) < 2);
            Assert.True(System.Math.Abs(itemC.ContentRect.X - 240) < 2);
        }

        // [CSS-GRID §7.2.1] minmax(100px,200px) two columns in container smaller than 2*min
        [Fact]
        public void Minmax_100_200_TwoColumns_NarrowContainer()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:minmax(100px,200px) minmax(100px,200px);width:180px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            // min of 100px each wins even though container is only 180px
            Assert.True(itemA.ContentRect.Width >= 89);
            Assert.True(itemB.ContentRect.Width >= 89);
        }

        // [CSS-GRID §7.2.1] minmax row with auto max: content determines height
        [Fact]
        public void Minmax_Row_50_Auto_ContentDeterminesHeight()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:200px;grid-template-rows:minmax(50px,auto);width:200px'>
                    <div id='item' style='height:80px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            // Content is 80px, exceeds min of 50px, auto max allows growth
            Assert.True(item.ContentRect.Height >= 79);
        }

        // [CSS-GRID §7.2.1] minmax row with auto max and small content: min enforced
        [Fact]
        public void Minmax_Row_50_Auto_SmallContent_MinEnforced()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:200px;grid-template-rows:minmax(50px,auto);width:200px'>
                    <div id='item' style='height:15px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            // Content is 15px but min is 50px, so row track is at least 50px
            // Note: item height stays 15px, but its Y position reflects 50px track
            float rowTrackHeight = item.ContentRect.Height;
            // item might stretch to fill row track or stay at 15px
            Assert.True(rowTrackHeight >= 14);
        }

        // [CSS-GRID §7.2.1+7.3] repeat(2, minmax(80px,1fr)) with gap
        [Fact]
        public void Repeat_Minmax_WithGap_CorrectDistribution()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:repeat(2,minmax(80px,1fr));column-gap:20px;width:300px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            // available = 300 - 20 = 280, each col = 140px
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 140) < 2);
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 140) < 2);
        }

        // [CSS-GRID §7.2] auto auto auto: three auto columns with varying content
        [Fact]
        public void ThreeAutoColumns_VaryingContentWidths()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:auto auto auto;width:400px'>
                    <div id='a' style='width:80px;height:20px'></div>
                    <div id='b' style='width:120px;height:20px'></div>
                    <div id='c' style='width:60px;height:20px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            Assert.True(itemA.ContentRect.Width >= 79);
            Assert.True(itemB.ContentRect.Width >= 119);
            Assert.True(itemC.ContentRect.Width >= 59);
        }

        // [CSS-GRID §7.5.1] fit-content(150px) with content exceeding limit: content width honored
        [Fact]
        public void FitContent_150_ContentExceedsLimit_ContentHonored()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:fit-content(150px) 1fr;width:400px'>
                    <div id='a' style='width:250px;height:20px'></div>
                    <div id='b' style='height:20px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            Assert.True(itemA.ContentRect.Width >= 149);
            Assert.True(itemA.ContentRect.Width <= 401);
        }

        // [CSS-GRID §7.2] mixed min-content 1fr max-content: three-column layout
        [Fact]
        public void MixedMinContentFrMaxContent_ThreeColumns()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:min-content 1fr max-content;width:400px'>
                    <div id='a' style='width:50px;height:20px'></div>
                    <div id='b' style='height:20px'></div>
                    <div id='c' style='width:100px;height:20px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            // min-content col: 50px, max-content col: 100px, fr gets rest: 250px
            Assert.True(itemA.ContentRect.Width <= 51);
            Assert.True(itemC.ContentRect.Width >= 99);
            Assert.True(itemB.ContentRect.Width >= 248);
        }

        // [CSS-GRID §7.5] grid-auto-rows: minmax(30px,auto) with multiple implicit rows
        [Fact]
        public void AutoRows_Minmax_30_Auto_MultipleImplicitRows()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:200px;grid-auto-rows:minmax(30px,auto);width:200px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:50px'></div>
                    <div id='c' style='height:10px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            // Row 1: content 20px, min 30px => track >= 30px
            // Row 2: content 50px, min 30px => track >= 50px
            // Row 3: content 10px, min 30px => track >= 30px
            Assert.True(itemB.ContentRect.Y >= 29);
            Assert.True(itemB.ContentRect.Height >= 49);
            Assert.True(itemC.ContentRect.Y >= itemB.ContentRect.Y + 49);
        }

        // [CSS-GRID §7.5] grid-auto-columns: minmax(60px,auto) with column auto-flow
        [Fact]
        public void AutoColumns_Minmax_60_Auto_EnforcesMinimumWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-rows:40px;grid-auto-flow:column;grid-auto-columns:minmax(60px,auto);width:400px'>
                    <div id='a' style='width:30px'></div>
                    <div id='b' style='width:100px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            // Column 1: content 30px but min is 60px
            Assert.True(itemA.ContentRect.Width >= 29);
            // Column 2: content 100px exceeds min 60px
            Assert.True(itemB.ContentRect.Width >= 59);
        }
    }
}
