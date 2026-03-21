using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    /// <summary>
    /// Comprehensive grid item sizing tests covering stretch, explicit dimensions, percentages,
    /// min/max constraints, box-sizing, padding, border, margin, auto margins, alignment
    /// (align-items, justify-items, align-self, justify-self), aspect-ratio, calc(), spanning,
    /// gaps, auto height from content, and nested blocks.
    /// </summary>
    public class WptGridItemAllSizingTests
    {
        private readonly ITestOutputHelper _output;

        public WptGridItemAllSizingTests(ITestOutputHelper output)
        {
            _output = output;
        }

        // [CSS-GRID §10.4] Default stretch fills column track width
        [Fact]
        public void StretchFillsColumnTrack()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:240px;width:240px'>
                    <div id='item' style='height:20px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(System.Math.Abs(item.ContentRect.Width - 240) < 2,
                $"Stretch should fill column track width 240 (got {item.ContentRect.Width})");
        }

        // [CSS-GRID §10.4] Default stretch fills row track height
        [Fact]
        public void StretchFillsRowTrack()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:200px;grid-template-rows:120px;width:200px'>
                    <div id='item'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(System.Math.Abs(item.ContentRect.Height - 120) < 2,
                $"Stretch should fill row track height 120 (got {item.ContentRect.Height})");
        }

        // [CSS-GRID §10.4] Explicit width overrides stretch
        [Fact]
        public void ExplicitWidthOverridesStretch()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:300px;width:300px'>
                    <div id='item' style='width:120px;height:20px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(System.Math.Abs(item.ContentRect.Width - 120) < 2,
                $"Explicit width 120 should override stretch (got {item.ContentRect.Width})");
        }

        // [CSS-GRID §10.4] Explicit height overrides row stretch
        [Fact]
        public void ExplicitHeightOverridesStretch()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:200px;grid-template-rows:150px;width:200px'>
                    <div id='item' style='height:50px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(System.Math.Abs(item.ContentRect.Height - 50) < 2,
                $"Explicit height 50 should override stretch (got {item.ContentRect.Height})");
        }

        // [CSS-GRID §10.4] width:50% resolves against column track
        [Fact]
        public void WidthFiftyPercentOfTrack()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:200px;width:200px'>
                    <div id='item' style='width:50%;height:20px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(System.Math.Abs(item.ContentRect.Width - 100) < 2,
                $"width:50% of 200px track should be 100 (got {item.ContentRect.Width})");
        }

        // [CSS-GRID §10.4] Explicit pixel height less than row track
        [Fact]
        public void ExplicitHeightHalfOfRow()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:200px;grid-template-rows:100px;width:200px'>
                    <div id='item' style='height:50px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(System.Math.Abs(item.ContentRect.Height - 50) < 2,
                $"height:50px should be 50 (got {item.ContentRect.Height})");
        }

        // [CSS-GRID §10.4] min-width clamps smaller width upward
        [Fact]
        public void MinWidthClampsUpward()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:200px;width:200px'>
                    <div id='item' style='width:40px;min-width:100px;height:20px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(item.ContentRect.Width >= 99,
                $"min-width:100px should clamp width upward (got {item.ContentRect.Width})");
        }

        // [CSS-GRID §10.4] max-width clamps explicit width downward
        [Fact]
        public void MaxWidthClampsExplicitWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:200px;width:200px'>
                    <div id='item' style='width:150px;max-width:80px;height:20px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(item.ContentRect.Width <= 81,
                $"max-width:80px should clamp explicit width 150 downward (got {item.ContentRect.Width})");
        }

        // [CSS-GRID §10.4] min-height clamps smaller height upward
        [Fact]
        public void MinHeightClampsUpward()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:200px;width:200px'>
                    <div id='item' style='min-height:60px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(item.ContentRect.Height >= 59,
                $"min-height:60px should clamp height upward (got {item.ContentRect.Height})");
        }

        // [CSS-GRID §10.4] max-height clamps explicit height downward
        [Fact]
        public void MaxHeightClampsExplicitHeight()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:200px;grid-template-rows:100px;width:200px'>
                    <div id='item' style='height:90px;max-height:40px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(item.ContentRect.Height <= 41,
                $"max-height:40px should clamp explicit height 90 downward (got {item.ContentRect.Height})");
        }

        // [CSS-GRID §10.4] border-box: padding+border inside specified width
        [Fact]
        public void BorderBoxWidthIncludesPaddingAndBorder()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:200px;width:200px'>
                    <div id='item' style='box-sizing:border-box;width:160px;padding:15px;border:5px solid;height:40px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            float expectedContentWidth = 160 - 15 - 15 - 5 - 5;
            Assert.True(System.Math.Abs(item.ContentRect.Width - expectedContentWidth) < 2,
                $"border-box content width should be {expectedContentWidth} (got {item.ContentRect.Width})");
            Assert.True(System.Math.Abs(item.BorderRect.Width - 160) < 2,
                $"border-box border rect width should be 160 (got {item.BorderRect.Width})");
        }

        // [CSS-GRID §10.4] Padding reduces content area when stretching
        [Fact]
        public void PaddingReducesContentOnStretch()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:200px;grid-template-rows:100px;width:200px'>
                    <div id='item' style='padding:20px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(System.Math.Abs(item.ContentRect.Width - 160) < 2,
                $"Padding 20px each side should reduce content width to 160 (got {item.ContentRect.Width})");
            Assert.True(System.Math.Abs(item.ContentRect.Height - 60) < 2,
                $"Padding 20px each side should reduce content height to 60 (got {item.ContentRect.Height})");
        }

        // [CSS-GRID §10.4] Border reduces content area when stretching
        [Fact]
        public void BorderReducesContentOnStretch()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:200px;grid-template-rows:100px;width:200px'>
                    <div id='item' style='border:10px solid black'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(System.Math.Abs(item.ContentRect.Width - 180) < 2,
                $"Border 10px each side should reduce content width to 180 (got {item.ContentRect.Width})");
            Assert.True(System.Math.Abs(item.ContentRect.Height - 80) < 2,
                $"Border 10px each side should reduce content height to 80 (got {item.ContentRect.Height})");
        }

        // [CSS-GRID §10.4] Margin reduces available stretch space
        [Fact]
        public void MarginReducesStretchSpace()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:200px;grid-template-rows:100px;width:200px'>
                    <div id='item' style='margin:20px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(System.Math.Abs(item.ContentRect.Width - 160) < 2,
                $"Margin 20px each side should reduce stretch width to 160 (got {item.ContentRect.Width})");
            Assert.True(System.Math.Abs(item.ContentRect.Height - 60) < 2,
                $"Margin 20px each side should reduce stretch height to 60 (got {item.ContentRect.Height})");
        }

        // [CSS-GRID §10.4] margin:auto centers item in cell
        [Fact]
        public void MarginAutoCentersInCell()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:200px;grid-template-rows:100px;width:200px'>
                    <div id='item' style='width:80px;height:40px;margin:auto'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(System.Math.Abs(item.ContentRect.X - 60) < 2,
                $"margin:auto should center horizontally at X=60 (got {item.ContentRect.X})");
            Assert.True(System.Math.Abs(item.ContentRect.Y - 30) < 2,
                $"margin:auto should center vertically at Y=30 (got {item.ContentRect.Y})");
        }

        // [CSS-GRID §10.4] align-items:start positions at track top
        [Fact]
        public void AlignItemsStartPositionsAtTop()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:200px;grid-template-rows:100px;align-items:start;width:200px'>
                    <div id='item' style='height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(System.Math.Abs(item.ContentRect.Y) < 2,
                $"align-items:start should position at Y=0 (got {item.ContentRect.Y})");
            Assert.True(System.Math.Abs(item.ContentRect.Height - 30) < 2,
                $"align-items:start should preserve explicit height 30 (got {item.ContentRect.Height})");
        }

        // [CSS-GRID §10.4] align-items:end positions at track bottom
        [Fact]
        public void AlignItemsEndPositionsAtBottom()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:200px;grid-template-rows:100px;align-items:end;width:200px'>
                    <div id='item' style='height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(System.Math.Abs(item.ContentRect.Y - 70) < 2,
                $"align-items:end should position at Y=70 (got {item.ContentRect.Y})");
        }

        // [CSS-GRID §10.4] align-items:center positions at track center
        [Fact]
        public void AlignItemsCenterPositionsAtCenter()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:200px;grid-template-rows:100px;align-items:center;width:200px'>
                    <div id='item' style='height:40px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(System.Math.Abs(item.ContentRect.Y - 30) < 2,
                $"align-items:center should position at Y=30 (got {item.ContentRect.Y})");
        }

        // [CSS-GRID §10.4] justify-items:start positions at track left
        [Fact]
        public void JustifyItemsStartPositionsAtLeft()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:200px;justify-items:start;width:200px'>
                    <div id='item' style='width:60px;height:20px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(System.Math.Abs(item.ContentRect.X) < 2,
                $"justify-items:start should position at X=0 (got {item.ContentRect.X})");
            Assert.True(System.Math.Abs(item.ContentRect.Width - 60) < 2,
                $"justify-items:start should preserve width 60 (got {item.ContentRect.Width})");
        }

        // [CSS-GRID §10.4] justify-items:end positions at track right
        [Fact]
        public void JustifyItemsEndPositionsAtRight()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:200px;justify-items:end;width:200px'>
                    <div id='item' style='width:60px;height:20px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(System.Math.Abs(item.ContentRect.X - 140) < 2,
                $"justify-items:end should position at X=140 (got {item.ContentRect.X})");
        }

        // [CSS-GRID §10.4] justify-items:center positions at track center
        [Fact]
        public void JustifyItemsCenterPositionsAtCenter()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:200px;justify-items:center;width:200px'>
                    <div id='item' style='width:80px;height:20px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(System.Math.Abs(item.ContentRect.X - 60) < 2,
                $"justify-items:center should position at X=60 (got {item.ContentRect.X})");
        }

        // [CSS-GRID §10.5] align-self overrides container align-items
        [Fact]
        public void AlignSelfOverridesAlignItems()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:200px;grid-template-rows:100px;align-items:start;width:200px'>
                    <div id='item' style='align-self:end;height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(System.Math.Abs(item.ContentRect.Y - 70) < 2,
                $"align-self:end should override align-items:start, Y=70 (got {item.ContentRect.Y})");
        }

        // [CSS-GRID §10.5] justify-self overrides container justify-items
        [Fact]
        public void JustifySelfOverridesJustifyItems()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:200px;justify-items:start;width:200px'>
                    <div id='item' style='justify-self:end;width:60px;height:20px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(System.Math.Abs(item.ContentRect.X - 140) < 2,
                $"justify-self:end should override justify-items:start, X=140 (got {item.ContentRect.X})");
        }

        // [CSS-SIZING §5.1] aspect-ratio determines height from stretched width
        [Fact]
        public void AspectRatioDeterminesHeightFromWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:200px;width:200px'>
                    <div id='item' style='aspect-ratio:2/1'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(System.Math.Abs(item.ContentRect.Width - 200) < 2,
                $"Stretch should fill column width 200 (got {item.ContentRect.Width})");
            Assert.True(System.Math.Abs(item.ContentRect.Height - 100) < 2,
                $"aspect-ratio 2/1 with width 200 should give height 100 (got {item.ContentRect.Height})");
        }

        // [CSS-VALUES §8.1] calc() width resolves against track
        [Fact]
        public void CalcWidthResolvesAgainstTrack()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:200px;width:200px'>
                    <div id='item' style='width:calc(100% - 60px);height:20px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(System.Math.Abs(item.ContentRect.Width - 140) < 2,
                $"calc(100% - 60px) of 200px track should be 140 (got {item.ContentRect.Width})");
        }

        // [CSS-GRID §10.4] Percentage width resolves against column track
        [Fact]
        public void PercentageWidthResolvesAgainstTrack()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:300px;width:300px'>
                    <div id='item' style='width:75%;height:20px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(System.Math.Abs(item.ContentRect.Width - 225) < 2,
                $"width:75% of 300px track should be 225 (got {item.ContentRect.Width})");
        }

        // [CSS-GRID §7.5] Auto height determined by content
        [Fact]
        public void AutoHeightFromContent()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:200px;width:200px'>
                    <div id='item'><div style='height:75px'></div></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(System.Math.Abs(item.ContentRect.Height - 75) < 2,
                $"Auto height should come from 75px child content (got {item.ContentRect.Height})");
        }

        // [CSS-GRID §8.3] Spanning item width covers two columns plus gap
        [Fact]
        public void SpanningItemWidthCoversColumnsAndGap()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px 100px;gap:20px;width:220px'>
                    <div id='item' style='grid-column:span 2;height:20px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(System.Math.Abs(item.ContentRect.Width - 220) < 2,
                $"Spanning 2 columns (100+20+100) should give width 220 (got {item.ContentRect.Width})");
        }

        // [CSS-GRID §8.3] Spanning item height covers two rows plus gap
        [Fact]
        public void SpanningItemHeightCoversRowsAndGap()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:200px;grid-template-rows:50px 50px;row-gap:10px;width:200px'>
                    <div id='item' style='grid-row:span 2'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(System.Math.Abs(item.ContentRect.Height - 110) < 2,
                $"Spanning 2 rows (50+10+50) should give height 110 (got {item.ContentRect.Height})");
        }

        // [CSS-GRID §10.1] Column gap separates items properly
        [Fact]
        public void WithGapItemsProperlySpaced()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:1fr 1fr;column-gap:20px;width:220px'>
                    <div id='first' style='height:20px'></div>
                    <div id='second' style='height:20px'></div>
                </div></body>");
            var first = LayoutTestHelper.FindById(root, "first")!;
            var second = LayoutTestHelper.FindById(root, "second")!;
            Assert.True(System.Math.Abs(first.ContentRect.Width - 100) < 2,
                $"First item should be (220-20)/2 = 100 (got {first.ContentRect.Width})");
            Assert.True(System.Math.Abs(second.ContentRect.X - 120) < 2,
                $"Second item should start at 100+20 = 120 (got {second.ContentRect.X})");
        }

        // [CSS-GRID §10.4] Nested block fills grid item
        [Fact]
        public void NestedBlockFillsGridItem()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:200px;width:200px'>
                    <div id='item'><div id='inner' style='height:40px'></div></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            var inner = LayoutTestHelper.FindById(root, "inner")!;
            Assert.True(System.Math.Abs(inner.ContentRect.Width - 200) < 2,
                $"Nested block should fill grid item width 200 (got {inner.ContentRect.Width})");
        }

        // [CSS-GRID §10.5] align-self:center with explicit height
        [Fact]
        public void AlignSelfCenterWithExplicitHeight()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:200px;grid-template-rows:100px;width:200px'>
                    <div id='item' style='align-self:center;height:40px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(System.Math.Abs(item.ContentRect.Y - 30) < 2,
                $"align-self:center in 100px row with 40px height should be at Y=30 (got {item.ContentRect.Y})");
        }

        // [CSS-GRID §10.5] justify-self:center with explicit width
        [Fact]
        public void JustifySelfCenterWithExplicitWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:200px;width:200px'>
                    <div id='item' style='justify-self:center;width:100px;height:20px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(System.Math.Abs(item.ContentRect.X - 50) < 2,
                $"justify-self:center in 200px col with 100px width should be at X=50 (got {item.ContentRect.X})");
        }

        // [CSS-GRID §10.4] border-box height includes padding and border
        [Fact]
        public void BorderBoxHeightIncludesPaddingAndBorder()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:200px;grid-template-rows:100px;width:200px'>
                    <div id='item' style='box-sizing:border-box;height:80px;padding:10px;border:5px solid'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            float expectedContentHeight = 80 - 10 - 10 - 5 - 5;
            Assert.True(System.Math.Abs(item.ContentRect.Height - expectedContentHeight) < 2,
                $"border-box content height should be {expectedContentHeight} (got {item.ContentRect.Height})");
            Assert.True(System.Math.Abs(item.BorderRect.Height - 80) < 2,
                $"border-box border rect height should be 80 (got {item.BorderRect.Height})");
        }

        // [CSS-GRID §10.4] Asymmetric margin reduces stretch differently per side
        [Fact]
        public void AsymmetricMarginReducesStretch()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:200px;grid-template-rows:100px;width:200px'>
                    <div id='item' style='margin:10px 30px 20px 40px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(System.Math.Abs(item.ContentRect.Width - 130) < 2,
                $"Width should be 200 - 30 - 40 = 130 (got {item.ContentRect.Width})");
            Assert.True(System.Math.Abs(item.ContentRect.Height - 70) < 2,
                $"Height should be 100 - 10 - 20 = 70 (got {item.ContentRect.Height})");
        }

        // [CSS-GRID §10.4] Combined padding, border, and margin on stretch
        [Fact]
        public void CombinedPaddingBorderMarginOnStretch()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:200px;grid-template-rows:100px;width:200px'>
                    <div id='item' style='margin:5px;padding:10px;border:3px solid'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            float expectedContentWidth = 200 - 5 - 5 - 10 - 10 - 3 - 3;
            float expectedContentHeight = 100 - 5 - 5 - 10 - 10 - 3 - 3;
            Assert.True(System.Math.Abs(item.ContentRect.Width - expectedContentWidth) < 2,
                $"Content width should be {expectedContentWidth} (got {item.ContentRect.Width})");
            Assert.True(System.Math.Abs(item.ContentRect.Height - expectedContentHeight) < 2,
                $"Content height should be {expectedContentHeight} (got {item.ContentRect.Height})");
        }

        // [CSS-GRID §10.4] margin-left:auto pushes item to right edge
        [Fact]
        public void MarginLeftAutoPushesRight()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:200px;width:200px'>
                    <div id='item' style='margin-left:auto;width:50px;height:20px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(System.Math.Abs(item.ContentRect.X - 150) < 2,
                $"margin-left:auto should push to X=150 (got {item.ContentRect.X})");
        }

        // [CSS-GRID §10.4] margin-top:auto pushes item to bottom edge
        [Fact]
        public void MarginTopAutoPushesDown()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:200px;grid-template-rows:100px;width:200px'>
                    <div id='item' style='margin-top:auto;height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(System.Math.Abs(item.ContentRect.Y - 70) < 2,
                $"margin-top:auto should push to Y=70 (got {item.ContentRect.Y})");
        }

        // [CSS-GRID §10.5] align-self:stretch overrides align-items:end
        [Fact]
        public void AlignSelfStretchOverridesAlignItemsEnd()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:200px;grid-template-rows:100px;align-items:end;width:200px'>
                    <div id='item' style='align-self:stretch'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(System.Math.Abs(item.ContentRect.Height - 100) < 2,
                $"align-self:stretch should override align-items:end (got {item.ContentRect.Height})");
        }

        // [CSS-GRID §10.5] justify-self:stretch overrides justify-items:center
        [Fact]
        public void JustifySelfStretchOverridesJustifyItemsCenter()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:200px;justify-items:center;width:200px'>
                    <div id='item' style='justify-self:stretch;height:20px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(System.Math.Abs(item.ContentRect.Width - 200) < 2,
                $"justify-self:stretch should override justify-items:center (got {item.ContentRect.Width})");
        }

        // [CSS-SIZING §5.1] aspect-ratio with explicit width
        [Fact]
        public void AspectRatioWithExplicitWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:200px;width:200px'>
                    <div id='item' style='width:120px;aspect-ratio:3/2'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(System.Math.Abs(item.ContentRect.Width - 120) < 2,
                $"Explicit width should be 120 (got {item.ContentRect.Width})");
            Assert.True(System.Math.Abs(item.ContentRect.Height - 80) < 2,
                $"aspect-ratio 3/2 with width 120 should give height 80 (got {item.ContentRect.Height})");
        }

        // [CSS-GRID §10.4] Stretch with row gap between items
        [Fact]
        public void StretchWithRowGap()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:200px;grid-template-rows:50px 50px;row-gap:20px;width:200px'>
                    <div id='first'></div>
                    <div id='second'></div>
                </div></body>");
            var first = LayoutTestHelper.FindById(root, "first")!;
            var second = LayoutTestHelper.FindById(root, "second")!;
            Assert.True(System.Math.Abs(first.ContentRect.Height - 50) < 2,
                $"First item should stretch to 50 (got {first.ContentRect.Height})");
            Assert.True(System.Math.Abs(second.ContentRect.Y - 70) < 2,
                $"Second item should start at Y=50+20=70 (got {second.ContentRect.Y})");
            Assert.True(System.Math.Abs(second.ContentRect.Height - 50) < 2,
                $"Second item should stretch to 50 (got {second.ContentRect.Height})");
        }

        // [CSS-GRID §10.4] Multiple children determine auto row height, stretch applies
        [Fact]
        public void AutoRowHeightFromTallestChild()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px 100px;width:200px'>
                    <div id='tall' style='height:90px'></div>
                    <div id='stretched'></div>
                </div></body>");
            var tall = LayoutTestHelper.FindById(root, "tall")!;
            var stretched = LayoutTestHelper.FindById(root, "stretched")!;
            Assert.True(System.Math.Abs(tall.ContentRect.Height - 90) < 2,
                $"Tall item should be 90 (got {tall.ContentRect.Height})");
            Assert.True(System.Math.Abs(stretched.ContentRect.Height - 90) < 2,
                $"Sibling should stretch to row height 90 (got {stretched.ContentRect.Height})");
        }

        // [CSS-VALUES §8.1] calc() with percentage and fixed value
        [Fact]
        public void CalcPercentagePlusFixed()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:200px;width:200px'>
                    <div id='item' style='width:calc(50% + 20px);height:20px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(System.Math.Abs(item.ContentRect.Width - 120) < 2,
                $"calc(50% + 20px) of 200px should be 120 (got {item.ContentRect.Width})");
        }

        // [CSS-GRID §10.4] Stretch with both column-gap and row-gap
        [Fact]
        public void StretchWithBothGaps()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:1fr 1fr;grid-template-rows:60px 60px;column-gap:20px;row-gap:10px;width:220px'>
                    <div id='topLeft'></div>
                    <div id='topRight'></div>
                    <div id='bottomLeft'></div>
                    <div id='bottomRight'></div>
                </div></body>");
            var topLeft = LayoutTestHelper.FindById(root, "topLeft")!;
            var bottomRight = LayoutTestHelper.FindById(root, "bottomRight")!;
            Assert.True(System.Math.Abs(topLeft.ContentRect.Width - 100) < 2,
                $"Each column should be (220-20)/2 = 100 (got {topLeft.ContentRect.Width})");
            Assert.True(System.Math.Abs(topLeft.ContentRect.Height - 60) < 2,
                $"First row should be 60 (got {topLeft.ContentRect.Height})");
            Assert.True(System.Math.Abs(bottomRight.ContentRect.Y - 70) < 2,
                $"Second row should start at 60+10=70 (got {bottomRight.ContentRect.Y})");
            Assert.True(System.Math.Abs(bottomRight.ContentRect.X - 120) < 2,
                $"Second column should start at 100+20=120 (got {bottomRight.ContentRect.X})");
        }
    }
}
