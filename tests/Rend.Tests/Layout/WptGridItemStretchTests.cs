using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    /// <summary>
    /// CSS Grid item stretch alignment tests. Covers the default stretch behavior
    /// for align-items/justify-items, overrides via explicit sizing and non-stretch
    /// alignment values, and interactions with padding, border, margin, box-sizing, and gaps.
    /// </summary>
    public class WptGridItemStretchTests
    {
        private readonly ITestOutputHelper _output;

        public WptGridItemStretchTests(ITestOutputHelper output)
        {
            _output = output;
        }

        // [CSS-GRID §10.4] Default align-items/justify-items is stretch
        [Fact]
        public void DefaultStretch_FillsColumnWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:200px;width:200px'>
                    <div id='item' style='height:20px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(System.Math.Abs(item.ContentRect.Width - 200) < 2,
                $"Default stretch should fill column width (got {item.ContentRect.Width})");
        }

        // [CSS-GRID §10.4] Default stretch fills row height when row is explicit
        [Fact]
        public void DefaultStretch_FillsRowHeight()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:200px;grid-template-rows:100px;width:200px'>
                    <div id='item'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(System.Math.Abs(item.ContentRect.Height - 100) < 2,
                $"Default stretch should fill row height (got {item.ContentRect.Height})");
        }

        // [CSS-GRID §10.4] Explicit width overrides stretch in inline axis
        [Fact]
        public void ExplicitWidth_OverridesStretch()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:200px;width:200px'>
                    <div id='item' style='width:80px;height:20px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(System.Math.Abs(item.ContentRect.Width - 80) < 2,
                $"Explicit width should override stretch (got {item.ContentRect.Width})");
        }

        // [CSS-GRID §10.4] Explicit height overrides stretch in block axis
        [Fact]
        public void ExplicitHeight_OverridesStretch()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:200px;grid-template-rows:100px;width:200px'>
                    <div id='item' style='height:40px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(System.Math.Abs(item.ContentRect.Height - 40) < 2,
                $"Explicit height should override stretch (got {item.ContentRect.Height})");
        }

        // [CSS-GRID §10.4] Stretch with padding reduces content area
        [Fact]
        public void StretchWithPadding_ReducesContentArea()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:200px;grid-template-rows:100px;width:200px'>
                    <div id='item' style='padding:10px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(System.Math.Abs(item.ContentRect.Width - 180) < 2,
                $"Content width should be 200 - 10 - 10 = 180 (got {item.ContentRect.Width})");
            Assert.True(System.Math.Abs(item.ContentRect.Height - 80) < 2,
                $"Content height should be 100 - 10 - 10 = 80 (got {item.ContentRect.Height})");
        }

        // [CSS-GRID §10.4] Stretch with border reduces content area
        [Fact]
        public void StretchWithBorder_ReducesContentArea()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:200px;grid-template-rows:100px;width:200px'>
                    <div id='item' style='border:5px solid black'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(System.Math.Abs(item.ContentRect.Width - 190) < 2,
                $"Content width should be 200 - 5 - 5 = 190 (got {item.ContentRect.Width})");
            Assert.True(System.Math.Abs(item.ContentRect.Height - 90) < 2,
                $"Content height should be 100 - 5 - 5 = 90 (got {item.ContentRect.Height})");
        }

        // [CSS-GRID §10.4] Stretch with margin reduces available space
        [Fact]
        public void StretchWithMargin_ReducesAvailableSpace()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:200px;grid-template-rows:100px;width:200px'>
                    <div id='item' style='margin:15px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(System.Math.Abs(item.ContentRect.Width - 170) < 2,
                $"Content width should be 200 - 15 - 15 = 170 (got {item.ContentRect.Width})");
            Assert.True(System.Math.Abs(item.ContentRect.Height - 70) < 2,
                $"Content height should be 100 - 15 - 15 = 70 (got {item.ContentRect.Height})");
        }

        // [CSS-GRID §10.4] Stretch with border-box: padding+border inside track size
        [Fact]
        public void StretchWithBorderBox_PaddingInsideTrackSize()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:200px;grid-template-rows:100px;width:200px'>
                    <div id='item' style='box-sizing:border-box;padding:10px;border:5px solid black'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            float expectedContentWidth = 200 - 10 - 10 - 5 - 5;
            float expectedContentHeight = 100 - 10 - 10 - 5 - 5;
            Assert.True(System.Math.Abs(item.ContentRect.Width - expectedContentWidth) < 2,
                $"Border-box content width should be {expectedContentWidth} (got {item.ContentRect.Width})");
            Assert.True(System.Math.Abs(item.ContentRect.Height - expectedContentHeight) < 2,
                $"Border-box content height should be {expectedContentHeight} (got {item.ContentRect.Height})");
            Assert.True(System.Math.Abs(item.BorderRect.Width - 200) < 2,
                $"Border rect width should be 200 (got {item.BorderRect.Width})");
        }

        // [CSS-GRID §10.4] Stretch in 2-column grid
        [Fact]
        public void StretchInTwoColumnGrid()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:120px 180px;width:300px'>
                    <div id='first' style='height:20px'></div>
                    <div id='second' style='height:20px'></div>
                </div></body>");
            var first = LayoutTestHelper.FindById(root, "first")!;
            var second = LayoutTestHelper.FindById(root, "second")!;
            Assert.True(System.Math.Abs(first.ContentRect.Width - 120) < 2,
                $"First item should stretch to 120px (got {first.ContentRect.Width})");
            Assert.True(System.Math.Abs(second.ContentRect.Width - 180) < 2,
                $"Second item should stretch to 180px (got {second.ContentRect.Width})");
        }

        // [CSS-GRID §10.4] Stretch in 3-column grid
        [Fact]
        public void StretchInThreeColumnGrid()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:80px 120px 100px;width:300px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                    <div id='c' style='height:20px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 80) < 2,
                $"Item a should stretch to 80px (got {itemA.ContentRect.Width})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 120) < 2,
                $"Item b should stretch to 120px (got {itemB.ContentRect.Width})");
            Assert.True(System.Math.Abs(itemC.ContentRect.Width - 100) < 2,
                $"Item c should stretch to 100px (got {itemC.ContentRect.Width})");
        }

        // [CSS-GRID §10.4] Stretch in 2-row grid
        [Fact]
        public void StretchInTwoRowGrid()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:200px;grid-template-rows:60px 40px;width:200px'>
                    <div id='first'></div>
                    <div id='second'></div>
                </div></body>");
            var first = LayoutTestHelper.FindById(root, "first")!;
            var second = LayoutTestHelper.FindById(root, "second")!;
            Assert.True(System.Math.Abs(first.ContentRect.Height - 60) < 2,
                $"First row item should stretch to 60px (got {first.ContentRect.Height})");
            Assert.True(System.Math.Abs(second.ContentRect.Height - 40) < 2,
                $"Second row item should stretch to 40px (got {second.ContentRect.Height})");
        }

        // [CSS-GRID §10.1] Stretch with column gap
        [Fact]
        public void StretchWithGap()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:1fr 1fr;gap:20px;width:220px'>
                    <div id='a' style='height:30px'></div>
                    <div id='b' style='height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 100) < 2,
                $"Each item should be (220 - 20) / 2 = 100px (got {itemA.ContentRect.Width})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 100) < 2,
                $"Each item should be 100px (got {itemB.ContentRect.Width})");
        }

        // [CSS-GRID §10.4] align-items:start prevents vertical stretch
        [Fact]
        public void AlignItemsStart_NoStretchHeight()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:200px;grid-template-rows:100px;align-items:start;width:200px'>
                    <div id='item' style='height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(System.Math.Abs(item.ContentRect.Height - 30) < 2,
                $"align-items:start should not stretch height (got {item.ContentRect.Height})");
            Assert.True(System.Math.Abs(item.ContentRect.Y - 0) < 2,
                $"align-items:start should place at top (got {item.ContentRect.Y})");
        }

        // [CSS-GRID §10.4] align-items:end prevents vertical stretch
        [Fact]
        public void AlignItemsEnd_NoStretchHeight()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:200px;grid-template-rows:100px;align-items:end;width:200px'>
                    <div id='item' style='height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(System.Math.Abs(item.ContentRect.Height - 30) < 2,
                $"align-items:end should not stretch height (got {item.ContentRect.Height})");
            Assert.True(System.Math.Abs(item.ContentRect.Y - 70) < 2,
                $"align-items:end should place at bottom (got {item.ContentRect.Y})");
        }

        // [CSS-GRID §10.4] justify-items:start prevents horizontal stretch
        [Fact]
        public void JustifyItemsStart_NoStretchWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:200px;justify-items:start;width:200px'>
                    <div id='item' style='width:60px;height:20px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(System.Math.Abs(item.ContentRect.Width - 60) < 2,
                $"justify-items:start should not stretch width (got {item.ContentRect.Width})");
            Assert.True(System.Math.Abs(item.ContentRect.X - 0) < 2,
                $"justify-items:start should place at left (got {item.ContentRect.X})");
        }

        // [CSS-GRID §10.4] justify-items:end prevents horizontal stretch
        [Fact]
        public void JustifyItemsEnd_NoStretchWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:200px;justify-items:end;width:200px'>
                    <div id='item' style='width:60px;height:20px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(System.Math.Abs(item.ContentRect.Width - 60) < 2,
                $"justify-items:end should not stretch width (got {item.ContentRect.Width})");
            Assert.True(System.Math.Abs(item.ContentRect.X - 140) < 2,
                $"justify-items:end should place at right (got {item.ContentRect.X})");
        }

        // [CSS-GRID §10.5] align-self:stretch overrides container align-items:start
        [Fact]
        public void AlignSelfStretch_OverridesContainerStart()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:200px;grid-template-rows:100px;align-items:start;width:200px'>
                    <div id='item' style='align-self:stretch'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(System.Math.Abs(item.ContentRect.Height - 100) < 2,
                $"align-self:stretch should override align-items:start (got {item.ContentRect.Height})");
        }

        // [CSS-GRID §10.5] justify-self:stretch overrides container justify-items:end
        [Fact]
        public void JustifySelfStretch_OverridesContainerEnd()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:200px;justify-items:end;width:200px'>
                    <div id='item' style='justify-self:stretch;height:20px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(System.Math.Abs(item.ContentRect.Width - 200) < 2,
                $"justify-self:stretch should override justify-items:end (got {item.ContentRect.Width})");
        }

        // [CSS-GRID §7.2] Stretch with fr columns
        [Fact]
        public void StretchWithFrColumns()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:1fr 2fr;width:300px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 100) < 2,
                $"1fr item should stretch to 100px (got {itemA.ContentRect.Width})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 200) < 2,
                $"2fr item should stretch to 200px (got {itemB.ContentRect.Width})");
        }

        // [CSS-GRID §7.2] Stretch with percentage columns
        [Fact]
        public void StretchWithPercentageColumns()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:40% 60%;width:300px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 120) < 2,
                $"40% item should stretch to 120px (got {itemA.ContentRect.Width})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 180) < 2,
                $"60% item should stretch to 180px (got {itemB.ContentRect.Width})");
        }

        // [CSS-GRID §7.5] Auto rows sized from content, stretch fills that height
        [Fact]
        public void StretchWithAutoRowsFromContent()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px 100px;width:200px'>
                    <div id='tall' style='height:80px'></div>
                    <div id='short'></div>
                </div></body>");
            var tall = LayoutTestHelper.FindById(root, "tall")!;
            var shortItem = LayoutTestHelper.FindById(root, "short")!;
            Assert.True(System.Math.Abs(tall.ContentRect.Height - 80) < 2,
                $"Tall item should be 80px (got {tall.ContentRect.Height})");
            Assert.True(System.Math.Abs(shortItem.ContentRect.Height - 80) < 2,
                $"Short item should stretch to row height of 80px (got {shortItem.ContentRect.Height})");
        }

        // [CSS-GRID §10.4] Stretch width and height simultaneously
        [Fact]
        public void StretchBothAxes()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:150px;grid-template-rows:80px;width:150px'>
                    <div id='item'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(System.Math.Abs(item.ContentRect.Width - 150) < 2,
                $"Should stretch width to 150 (got {item.ContentRect.Width})");
            Assert.True(System.Math.Abs(item.ContentRect.Height - 80) < 2,
                $"Should stretch height to 80 (got {item.ContentRect.Height})");
        }

        // [CSS-GRID §10.4] Stretch with asymmetric padding
        [Fact]
        public void StretchWithAsymmetricPadding()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:200px;grid-template-rows:100px;width:200px'>
                    <div id='item' style='padding:5px 10px 15px 20px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(System.Math.Abs(item.ContentRect.Width - 170) < 2,
                $"Content width should be 200 - 10 - 20 = 170 (got {item.ContentRect.Width})");
            Assert.True(System.Math.Abs(item.ContentRect.Height - 80) < 2,
                $"Content height should be 100 - 5 - 15 = 80 (got {item.ContentRect.Height})");
        }

        // [CSS-GRID §10.4] Stretch with padding and border combined
        [Fact]
        public void StretchWithPaddingAndBorder()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:200px;grid-template-rows:100px;width:200px'>
                    <div id='item' style='padding:8px;border:2px solid black'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            float expectedContentWidth = 200 - 8 - 8 - 2 - 2;
            float expectedContentHeight = 100 - 8 - 8 - 2 - 2;
            Assert.True(System.Math.Abs(item.ContentRect.Width - expectedContentWidth) < 2,
                $"Content width should be {expectedContentWidth} (got {item.ContentRect.Width})");
            Assert.True(System.Math.Abs(item.ContentRect.Height - expectedContentHeight) < 2,
                $"Content height should be {expectedContentHeight} (got {item.ContentRect.Height})");
        }

        // [CSS-GRID §10.4] Stretch with margin and padding combined
        [Fact]
        public void StretchWithMarginAndPadding()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:200px;grid-template-rows:100px;width:200px'>
                    <div id='item' style='margin:10px;padding:5px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            float expectedContentWidth = 200 - 10 - 10 - 5 - 5;
            float expectedContentHeight = 100 - 10 - 10 - 5 - 5;
            Assert.True(System.Math.Abs(item.ContentRect.Width - expectedContentWidth) < 2,
                $"Content width should be {expectedContentWidth} (got {item.ContentRect.Width})");
            Assert.True(System.Math.Abs(item.ContentRect.Height - expectedContentHeight) < 2,
                $"Content height should be {expectedContentHeight} (got {item.ContentRect.Height})");
        }

        // [CSS-GRID §10.4] align-items:center prevents stretch, item uses content height
        [Fact]
        public void AlignItemsCenter_NoStretch()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:200px;grid-template-rows:100px;align-items:center;width:200px'>
                    <div id='item' style='height:40px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(System.Math.Abs(item.ContentRect.Height - 40) < 2,
                $"align-items:center should not stretch (got {item.ContentRect.Height})");
            Assert.True(System.Math.Abs(item.ContentRect.Y - 30) < 2,
                $"Should be vertically centered (got {item.ContentRect.Y})");
        }

        // [CSS-GRID §10.4] justify-items:center prevents stretch, item uses content width
        [Fact]
        public void JustifyItemsCenter_NoStretch()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:200px;justify-items:center;width:200px'>
                    <div id='item' style='width:80px;height:20px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(System.Math.Abs(item.ContentRect.Width - 80) < 2,
                $"justify-items:center should not stretch (got {item.ContentRect.Width})");
            Assert.True(System.Math.Abs(item.ContentRect.X - 60) < 2,
                $"Should be horizontally centered (got {item.ContentRect.X})");
        }

        // [CSS-GRID §10.4] Stretch with row and column gap both
        [Fact]
        public void StretchWithRowAndColumnGap()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:1fr 1fr;grid-template-rows:50px 50px;row-gap:10px;column-gap:20px;width:220px'>
                    <div id='a'></div>
                    <div id='b'></div>
                    <div id='c'></div>
                    <div id='d'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemD = LayoutTestHelper.FindById(root, "d")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 100) < 2,
                $"Item width should be (220-20)/2 = 100 (got {itemA.ContentRect.Width})");
            Assert.True(System.Math.Abs(itemA.ContentRect.Height - 50) < 2,
                $"Item height should be 50 (got {itemA.ContentRect.Height})");
            Assert.True(System.Math.Abs(itemD.ContentRect.Y - 60) < 2,
                $"Item d Y should be 50+10 = 60 (got {itemD.ContentRect.Y})");
        }

        // [CSS-GRID §10.5] align-self:start on one item while others stretch
        [Fact]
        public void AlignSelfStart_WhileOthersStretch()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px 100px;grid-template-rows:80px;width:200px'>
                    <div id='start' style='align-self:start;height:30px'></div>
                    <div id='stretch'></div>
                </div></body>");
            var startItem = LayoutTestHelper.FindById(root, "start")!;
            var stretchItem = LayoutTestHelper.FindById(root, "stretch")!;
            Assert.True(System.Math.Abs(startItem.ContentRect.Height - 30) < 2,
                $"align-self:start item should keep its height (got {startItem.ContentRect.Height})");
            Assert.True(System.Math.Abs(stretchItem.ContentRect.Height - 80) < 2,
                $"Default item should stretch to row height (got {stretchItem.ContentRect.Height})");
        }

        // [CSS-GRID §10.5] justify-self:end on one item while others stretch
        [Fact]
        public void JustifySelfEnd_WhileOthersStretch()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:200px;width:200px'>
                    <div id='end' style='justify-self:end;width:50px;height:20px'></div>
                    <div id='stretch' style='height:20px'></div>
                </div></body>");
            var endItem = LayoutTestHelper.FindById(root, "end")!;
            var stretchItem = LayoutTestHelper.FindById(root, "stretch")!;
            Assert.True(System.Math.Abs(endItem.ContentRect.Width - 50) < 2,
                $"justify-self:end item should keep its width (got {endItem.ContentRect.Width})");
            Assert.True(System.Math.Abs(endItem.ContentRect.X - 150) < 2,
                $"justify-self:end item should be at right (got {endItem.ContentRect.X})");
            Assert.True(System.Math.Abs(stretchItem.ContentRect.Width - 200) < 2,
                $"Default item should stretch to column width (got {stretchItem.ContentRect.Width})");
        }

        // [CSS-GRID §10.4] Stretch fills both column and row in multi-track grid
        [Fact]
        public void StretchInMultiTrackGrid()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px 200px;grid-template-rows:60px 40px;width:300px'>
                    <div id='a'></div>
                    <div id='b'></div>
                    <div id='c'></div>
                    <div id='d'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            var itemD = LayoutTestHelper.FindById(root, "d")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(itemA.ContentRect.Height - 60) < 2);
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 200) < 2);
            Assert.True(System.Math.Abs(itemB.ContentRect.Height - 60) < 2);
            Assert.True(System.Math.Abs(itemC.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(itemC.ContentRect.Height - 40) < 2);
            Assert.True(System.Math.Abs(itemD.ContentRect.Width - 200) < 2);
            Assert.True(System.Math.Abs(itemD.ContentRect.Height - 40) < 2);
        }
    }
}
