using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    /// <summary>
    /// Grid item dimension tests covering width/height stretch, explicit overrides,
    /// percentages, min/max constraints, box-sizing, padding, border, margin,
    /// aspect-ratio, calc, em units, zero dimensions, and nested content.
    /// </summary>
    public class WptGridItemDimensionTests
    {
        private readonly ITestOutputHelper _output;

        public WptGridItemDimensionTests(ITestOutputHelper output)
        {
            _output = output;
        }

        // [CSS-GRID §12.1] Grid item with auto width stretches to column track width
        [Fact]
        public void WidthStretchesToColumnTrack()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:250px;width:250px'>
                    <div id='t' style='height:30px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 250) < 2,
                $"Expected width 250, got {target.ContentRect.Width}");
        }

        // [CSS-GRID §12.1] Grid item with auto height stretches to row track height
        [Fact]
        public void HeightStretchesToRowTrack()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:200px;grid-template-rows:120px;width:200px'>
                    <div id='t'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Height - 120) < 2,
                $"Expected height 120, got {target.ContentRect.Height}");
        }

        // [CSS-GRID §12.1] Explicit width overrides stretch behavior
        [Fact]
        public void ExplicitWidthOverridesStretch()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:300px;width:300px'>
                    <div id='t' style='width:150px;height:20px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 150) < 2,
                $"Expected width 150, got {target.ContentRect.Width}");
        }

        // [CSS-GRID §12.1] Explicit height overrides stretch behavior
        [Fact]
        public void ExplicitHeightOverridesStretch()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:200px;grid-template-rows:100px;width:200px'>
                    <div id='t' style='height:45px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Height - 45) < 2,
                $"Expected height 45, got {target.ContentRect.Height}");
        }

        // [CSS-GRID §12.1] width:50% resolves against column track
        [Fact]
        public void WidthFiftyPercentOfTrack()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:200px;width:200px'>
                    <div id='t' style='width:50%;height:20px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 100) < 2,
                $"Expected width 100, got {target.ContentRect.Width}");
        }

        // [CSS-GRID §12.1] height:50% resolves against row track
        [Fact]
        public void HeightFiftyPercentOfRow()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:200px;grid-template-rows:80px;width:200px'>
                    <div id='t' style='height:50%'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Height - 40) < 2,
                $"Expected height 40, got {target.ContentRect.Height}");
        }

        // [CSS-SIZING §4.4] min-width constrains grid item
        [Fact]
        public void MinWidthOnGridItem()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:200px;justify-items:start;width:200px'>
                    <div id='t' style='width:30px;min-width:80px;height:20px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(target.ContentRect.Width >= 79,
                $"Expected min-width 80, got {target.ContentRect.Width}");
        }

        // [CSS-SIZING §4.4] max-width constrains grid item
        [Fact]
        public void MaxWidthOnGridItem()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:200px;width:200px'>
                    <div id='t' style='max-width:120px;height:20px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(target.ContentRect.Width <= 121,
                $"Expected max-width 120, got {target.ContentRect.Width}");
        }

        // [CSS-SIZING §4.4] min-height constrains grid item
        [Fact]
        public void MinHeightOnGridItem()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:200px;width:200px'>
                    <div id='t' style='min-height:60px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(target.ContentRect.Height >= 59,
                $"Expected min-height 60, got {target.ContentRect.Height}");
        }

        // [CSS-SIZING §4.4] max-height constrains grid item
        [Fact]
        public void MaxHeightOnGridItem()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:200px;grid-template-rows:100px;width:200px'>
                    <div id='t' style='max-height:50px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(target.ContentRect.Height <= 51,
                $"Expected max-height 50, got {target.ContentRect.Height}");
        }

        // [CSS-BOX §6.1] border-box width includes padding and border
        [Fact]
        public void BorderBoxWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:200px;width:200px'>
                    <div id='t' style='box-sizing:border-box;width:150px;padding:10px;border:5px solid black;height:30px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            float totalWidth = target.ContentRect.Width + target.PaddingLeft + target.PaddingRight
                + target.BorderLeftWidth + target.BorderRightWidth;
            Assert.True(System.Math.Abs(totalWidth - 150) < 2,
                $"Expected border-box total 150, got {totalWidth}");
        }

        // [CSS-BOX §6.1] border-box height includes padding and border
        [Fact]
        public void BorderBoxHeight()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:200px;width:200px'>
                    <div id='t' style='box-sizing:border-box;height:100px;padding:10px;border:5px solid black'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            float totalHeight = target.ContentRect.Height + target.PaddingTop + target.PaddingBottom
                + target.BorderTopWidth + target.BorderBottomWidth;
            Assert.True(System.Math.Abs(totalHeight - 100) < 2,
                $"Expected border-box total 100, got {totalHeight}");
        }

        // [CSS-BOX §5.1] Padding reduces content area within stretched grid item
        [Fact]
        public void PaddingReducesContent()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:200px;width:200px'>
                    <div id='t' style='padding:20px;height:30px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 160) < 2,
                $"Expected content width 160 (200 - 40 padding), got {target.ContentRect.Width}");
        }

        // [CSS-BOX §5.2] Border reduces content area within stretched grid item
        [Fact]
        public void BorderReducesContent()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:200px;width:200px'>
                    <div id='t' style='border:10px solid black;height:30px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 180) < 2,
                $"Expected content width 180 (200 - 20 border), got {target.ContentRect.Width}");
        }

        // [CSS-GRID §12.1] Margin reduces available space for stretched grid item
        [Fact]
        public void MarginReducesAvailable()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:200px;width:200px'>
                    <div id='t' style='margin:15px;height:30px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 170) < 2,
                $"Expected content width 170 (200 - 30 margin), got {target.ContentRect.Width}");
        }

        // [CSS-GRID §10.3] margin:auto centers grid item in cell
        [Fact]
        public void MarginAutoCenters()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:200px;grid-template-rows:100px;width:200px'>
                    <div id='t' style='width:80px;height:40px;margin:auto'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.X - 60) < 2,
                $"Expected X centered at 60, got {target.ContentRect.X}");
            Assert.True(System.Math.Abs(target.ContentRect.Y - 30) < 2,
                $"Expected Y centered at 30, got {target.ContentRect.Y}");
        }

        // [CSS-SIZING §5.1] aspect-ratio on grid item
        [Fact]
        public void AspectRatioOnGridItem()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:200px;width:200px'>
                    <div id='t' style='aspect-ratio:2/1'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 200) < 2,
                $"Expected width 200, got {target.ContentRect.Width}");
            Assert.True(System.Math.Abs(target.ContentRect.Height - 100) < 2,
                $"Expected height 100 (200/2), got {target.ContentRect.Height}");
        }

        // [CSS-VALUES §8.1] calc() width on grid item
        [Fact]
        public void CalcWidthOnGridItem()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:200px;justify-items:start;width:200px'>
                    <div id='t' style='width:calc(100px + 30px);height:20px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 130) < 2,
                $"Expected calc width 130, got {target.ContentRect.Width}");
        }

        // [CSS-VALUES §5.2] em width resolves against grid item font-size
        [Fact]
        public void EmWidthOnGridItem()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:200px;justify-items:start;width:200px'>
                    <div id='t' style='font-size:20px;width:5em;height:20px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 100) < 2,
                $"Expected 5em=100px, got {target.ContentRect.Width}");
        }

        // [CSS-SIZING §4.1] width:0 produces zero content width
        [Fact]
        public void WidthZeroOnGridItem()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:200px;width:200px'>
                    <div id='t' style='width:0;height:20px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(target.ContentRect.Width < 2,
                $"Expected width 0, got {target.ContentRect.Width}");
        }

        // [CSS-SIZING §4.1] height:0 produces zero content height
        [Fact]
        public void HeightZeroOnGridItem()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:200px;grid-template-rows:80px;width:200px'>
                    <div id='t' style='height:0'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(target.ContentRect.Height < 2,
                $"Expected height 0, got {target.ContentRect.Height}");
        }

        // [CSS-SIZING §5.2] percentage width on direct grid item
        [Fact]
        public void PercentageWidthOnGridItem()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:240px;justify-items:start;width:240px'>
                    <div id='t' style='width:75%;height:20px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 180) < 2,
                $"Expected 75% of 240 = 180, got {target.ContentRect.Width}");
        }

        // [CSS-SIZING §5.2] percentage height on grid item with explicit row
        [Fact]
        public void PercentageHeightOnGridItem()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:200px;grid-template-rows:120px;width:200px'>
                    <div id='t' style='height:25%'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Height - 30) < 2,
                $"Expected 25% of 120 = 30, got {target.ContentRect.Height}");
        }

        // [CSS-GRID §12.1] Child content determines auto row height
        [Fact]
        public void ChildContentDeterminesAutoHeight()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:200px;width:200px'>
                    <div id='t'><div style='height:55px'></div></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(target.ContentRect.Height >= 54,
                $"Expected height >= 55 from child, got {target.ContentRect.Height}");
        }

        // [CSS-SIZING §5.1] width:100% fills column track
        [Fact]
        public void WidthHundredPercentFillsTrack()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:180px;width:180px'>
                    <div id='t' style='width:100%;height:20px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 180) < 2,
                $"Expected 100% = 180, got {target.ContentRect.Width}");
        }

        // [CSS-GRID §12.1] Nested block inside grid item fills track width
        [Fact]
        public void NestedBlockFillsGridItem()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:220px;width:220px'>
                    <div><div id='inner' style='height:20px'></div></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "inner")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 220) < 2,
                $"Expected nested block width 220, got {target.ContentRect.Width}");
        }

        // [CSS-GRID §12.1] Grid item containing flex children
        [Fact]
        public void GridItemWithFlexChildren()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:300px;width:300px'>
                    <div style='display:flex'>
                        <div id='flexA' style='flex:1;height:40px'></div>
                        <div id='flexB' style='flex:2;height:40px'></div>
                    </div>
                </div></body>");
            var flexA = LayoutTestHelper.FindById(root, "flexA")!;
            var flexB = LayoutTestHelper.FindById(root, "flexB")!;
            Assert.True(System.Math.Abs(flexA.ContentRect.Width - 100) < 2,
                $"Expected flex:1 = 100, got {flexA.ContentRect.Width}");
            Assert.True(System.Math.Abs(flexB.ContentRect.Width - 200) < 2,
                $"Expected flex:2 = 200, got {flexB.ContentRect.Width}");
        }

        // [CSS-GRID §12.1] Two-column grid items both stretch independently
        [Fact]
        public void TwoColumnStretch()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:120px 180px;width:300px'>
                    <div id='a' style='height:30px'></div>
                    <div id='b' style='height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 120) < 2,
                $"Expected col1 width 120, got {itemA.ContentRect.Width}");
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 180) < 2,
                $"Expected col2 width 180, got {itemB.ContentRect.Width}");
        }

        // [CSS-GRID §12.1] min-width wins over max-width when min > max
        [Fact]
        public void MinWidthOverridesMaxWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:200px;justify-items:start;width:200px'>
                    <div id='t' style='width:100px;min-width:150px;max-width:120px;height:20px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(target.ContentRect.Width >= 149,
                $"Expected min-width 150 to win, got {target.ContentRect.Width}");
        }

        // [CSS-GRID §12.1] min-height wins over max-height when min > max
        [Fact]
        public void MinHeightOverridesMaxHeight()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:200px;width:200px'>
                    <div id='t' style='min-height:90px;max-height:60px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(target.ContentRect.Height >= 89,
                $"Expected min-height 90 to win over max-height 60, got {target.ContentRect.Height}");
        }

        // [CSS-BOX §5] Padding + border together reduce content area
        [Fact]
        public void PaddingAndBorderReduceContent()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:200px;width:200px'>
                    <div id='t' style='padding:10px;border:5px solid black;height:50px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 170) < 2,
                $"Expected content width 170 (200 - 20 pad - 10 border), got {target.ContentRect.Width}");
        }

        // [CSS-VALUES §8.1] calc() with percentage on grid item
        [Fact]
        public void CalcPercentageWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:200px;justify-items:start;width:200px'>
                    <div id='t' style='width:calc(50% + 20px);height:20px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 120) < 2,
                $"Expected calc(50% + 20px) = 120, got {target.ContentRect.Width}");
        }

        // [CSS-GRID §12.1] margin-left:auto pushes item to right edge
        [Fact]
        public void MarginLeftAutoPushesRight()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:200px;width:200px'>
                    <div id='t' style='margin-left:auto;width:60px;height:30px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.X - 140) < 2,
                $"Expected X at 140, got {target.ContentRect.X}");
        }

        // [CSS-GRID §12.1] margin-top:auto pushes item to bottom of row
        [Fact]
        public void MarginTopAutoPushesDown()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:200px;grid-template-rows:100px;width:200px'>
                    <div id='t' style='margin-top:auto;height:30px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Y - 70) < 2,
                $"Expected Y at 70, got {target.ContentRect.Y}");
        }

        // [CSS-GRID §12.1] Grid item with fr track stretches to available
        [Fact]
        public void FrTrackItemStretches()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:1fr;width:300px'>
                    <div id='t' style='height:20px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 300) < 2,
                $"Expected 1fr = 300, got {target.ContentRect.Width}");
        }

        // [CSS-GRID §12.1] Stretch in auto row with siblings determines row height
        [Fact]
        public void AutoRowHeightFromTallestSibling()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px 100px;width:200px'>
                    <div id='short'></div>
                    <div id='tall' style='height:80px'></div>
                </div></body>");
            var shortItem = LayoutTestHelper.FindById(root, "short")!;
            var tallItem = LayoutTestHelper.FindById(root, "tall")!;
            Assert.True(System.Math.Abs(shortItem.ContentRect.Height - 80) < 2,
                $"Expected auto item to stretch to 80, got {shortItem.ContentRect.Height}");
            Assert.True(System.Math.Abs(tallItem.ContentRect.Height - 80) < 2,
                $"Expected tall item height 80, got {tallItem.ContentRect.Height}");
        }

        // [CSS-BOX §6.1] border-box with padding and border on grid item
        [Fact]
        public void BorderBoxWithPaddingAndBorder()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:200px;width:200px'>
                    <div id='t' style='box-sizing:border-box;width:180px;height:100px;padding:15px;border:5px solid black'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            float totalWidth = target.ContentRect.Width + target.PaddingLeft + target.PaddingRight
                + target.BorderLeftWidth + target.BorderRightWidth;
            float totalHeight = target.ContentRect.Height + target.PaddingTop + target.PaddingBottom
                + target.BorderTopWidth + target.BorderBottomWidth;
            Assert.True(System.Math.Abs(totalWidth - 180) < 2,
                $"Expected border-box width 180, got {totalWidth}");
            Assert.True(System.Math.Abs(totalHeight - 100) < 2,
                $"Expected border-box height 100, got {totalHeight}");
        }

        // [CSS-GRID §12.1] Grid item with max-width smaller than track does not stretch beyond max
        [Fact]
        public void MaxWidthSmallerThanTrack()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:300px;width:300px'>
                    <div id='t' style='max-width:150px;height:20px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(target.ContentRect.Width <= 151,
                $"Expected max-width 150 constraint, got {target.ContentRect.Width}");
        }

        // [CSS-GRID §12.1] Grid item with max-height smaller than row does not stretch beyond max
        [Fact]
        public void MaxHeightSmallerThanRow()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:200px;grid-template-rows:100px;width:200px'>
                    <div id='t' style='max-height:40px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(target.ContentRect.Height <= 41,
                $"Expected max-height 40 constraint, got {target.ContentRect.Height}");
        }

        // [CSS-GRID §12.1] Multiple children inside grid item contribute to auto height
        [Fact]
        public void MultipleChildrenDetermineAutoHeight()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:200px;width:200px'>
                    <div id='t'>
                        <div style='height:25px'></div>
                        <div style='height:35px'></div>
                    </div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(target.ContentRect.Height >= 59,
                $"Expected height >= 60 from children, got {target.ContentRect.Height}");
        }

        // [CSS-VALUES §5.2] em height resolves against grid item font-size
        [Fact]
        public void EmHeightOnGridItem()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:200px;width:200px'>
                    <div id='t' style='font-size:20px;height:3em'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Height - 60) < 2,
                $"Expected 3em=60px, got {target.ContentRect.Height}");
        }
    }
}
