using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    /// <summary>
    /// CSS Grid item height tests covering stretch, explicit height, percentage height,
    /// alignment positioning, align-self overrides, auto height from content,
    /// min/max-height constraints, padding/border/margin interactions, border-box,
    /// height:0, multiple rows, and row gap.
    /// </summary>
    public class WptGridAllItemHeightTests
    {
        private readonly ITestOutputHelper _output;

        public WptGridAllItemHeightTests(ITestOutputHelper output)
        {
            _output = output;
        }

        // [CSS-GRID §10.4] Stretch fills row of 50px
        [Fact]
        public void Stretch_FillsRow50()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-template-rows:50px;width:200px'>
                    <div id='item'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(System.Math.Abs(item.ContentRect.Height - 50) < 2,
                $"Stretch should fill 50px row (got {item.ContentRect.Height})");
        }

        // [CSS-GRID §10.4] Stretch fills row of 80px
        [Fact]
        public void Stretch_FillsRow80()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-template-rows:80px;width:200px'>
                    <div id='item'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(System.Math.Abs(item.ContentRect.Height - 80) < 2,
                $"Stretch should fill 80px row (got {item.ContentRect.Height})");
        }

        // [CSS-GRID §10.4] Stretch fills row of 100px
        [Fact]
        public void Stretch_FillsRow100()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-template-rows:100px;width:200px'>
                    <div id='item'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(System.Math.Abs(item.ContentRect.Height - 100) < 2,
                $"Stretch should fill 100px row (got {item.ContentRect.Height})");
        }

        // [CSS-GRID §10.4] Stretch fills row of 150px
        [Fact]
        public void Stretch_FillsRow150()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-template-rows:150px;width:200px'>
                    <div id='item'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(System.Math.Abs(item.ContentRect.Height - 150) < 2,
                $"Stretch should fill 150px row (got {item.ContentRect.Height})");
        }

        // [CSS-GRID §10.4] Stretch fills row of 200px
        [Fact]
        public void Stretch_FillsRow200()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-template-rows:200px;width:200px'>
                    <div id='item'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(System.Math.Abs(item.ContentRect.Height - 200) < 2,
                $"Stretch should fill 200px row (got {item.ContentRect.Height})");
        }

        // [CSS-GRID §10.4] Explicit height 30px overrides stretch in 100px row
        [Fact]
        public void ExplicitHeight30_OverridesStretch()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-template-rows:100px;width:200px'>
                    <div id='item' style='height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(System.Math.Abs(item.ContentRect.Height - 30) < 2,
                $"Explicit height 30px should override stretch (got {item.ContentRect.Height})");
        }

        // [CSS-GRID §10.4] Explicit height 50px overrides stretch in 100px row
        [Fact]
        public void ExplicitHeight50_OverridesStretch()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-template-rows:100px;width:200px'>
                    <div id='item' style='height:50px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(System.Math.Abs(item.ContentRect.Height - 50) < 2,
                $"Explicit height 50px should override stretch (got {item.ContentRect.Height})");
        }

        // [CSS-GRID §10.4] Explicit height 80px overrides stretch in 100px row
        [Fact]
        public void ExplicitHeight80_OverridesStretch()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-template-rows:100px;width:200px'>
                    <div id='item' style='height:80px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(System.Math.Abs(item.ContentRect.Height - 80) < 2,
                $"Explicit height 80px should override stretch (got {item.ContentRect.Height})");
        }

        // [CSS-GRID §10.4] Percentage height 50% of 100px row = 50px
        [Fact(Skip = "Known bug: percentage height on grid item does not resolve against row track")]
        public void PercentageHeight50_OfRow100()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-template-rows:100px;width:200px'>
                    <div id='item' style='height:50%'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(System.Math.Abs(item.ContentRect.Height - 50) < 2,
                $"50% of 100px row should be 50px (got {item.ContentRect.Height})");
        }

        // [CSS-GRID §10.4] align-items:start with height 30px, Y=0
        [Fact]
        public void AlignItemsStart_Height30_YIsZero()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-template-rows:100px;align-items:start;width:200px'>
                    <div id='item' style='height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(System.Math.Abs(item.ContentRect.Height - 30) < 2,
                $"Height should be 30px (got {item.ContentRect.Height})");
            Assert.True(System.Math.Abs(item.ContentRect.Y - 0) < 2,
                $"Y should be 0 for align-items:start (got {item.ContentRect.Y})");
        }

        // [CSS-GRID §10.4] align-items:end with height 30px in row 100, Y=70
        [Fact]
        public void AlignItemsEnd_Height30_Row100_YIs70()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-template-rows:100px;align-items:end;width:200px'>
                    <div id='item' style='height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(System.Math.Abs(item.ContentRect.Height - 30) < 2,
                $"Height should be 30px (got {item.ContentRect.Height})");
            Assert.True(System.Math.Abs(item.ContentRect.Y - 70) < 2,
                $"Y should be 70 for align-items:end in 100px row (got {item.ContentRect.Y})");
        }

        // [CSS-GRID §10.4] align-items:center with height 30px in row 100, Y=35
        [Fact]
        public void AlignItemsCenter_Height30_Row100_YIs35()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-template-rows:100px;align-items:center;width:200px'>
                    <div id='item' style='height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(System.Math.Abs(item.ContentRect.Height - 30) < 2,
                $"Height should be 30px (got {item.ContentRect.Height})");
            Assert.True(System.Math.Abs(item.ContentRect.Y - 35) < 2,
                $"Y should be 35 for center in 100px row (got {item.ContentRect.Y})");
        }

        // [CSS-GRID §10.5] align-self:start overrides container align-items:end
        [Fact]
        public void AlignSelfStart_OverridesAlignItemsEnd()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-template-rows:100px;align-items:end;width:200px'>
                    <div id='item' style='align-self:start;height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(System.Math.Abs(item.ContentRect.Y - 0) < 2,
                $"align-self:start should override end, Y=0 (got {item.ContentRect.Y})");
        }

        // [CSS-GRID §10.5] align-self:end overrides container align-items:start
        [Fact]
        public void AlignSelfEnd_OverridesAlignItemsStart()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-template-rows:100px;align-items:start;width:200px'>
                    <div id='item' style='align-self:end;height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(System.Math.Abs(item.ContentRect.Y - 70) < 2,
                $"align-self:end should override start, Y=70 (got {item.ContentRect.Y})");
        }

        // [CSS-GRID §10.5] align-self:center overrides container align-items:start
        [Fact]
        public void AlignSelfCenter_OverridesAlignItemsStart()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-template-rows:100px;align-items:start;width:200px'>
                    <div id='item' style='align-self:center;height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(System.Math.Abs(item.ContentRect.Y - 35) < 2,
                $"align-self:center should override start, Y=35 (got {item.ContentRect.Y})");
        }

        // [CSS-GRID §10.5] align-self:stretch overrides container align-items:center
        [Fact]
        public void AlignSelfStretch_OverridesAlignItemsCenter()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-template-rows:100px;align-items:center;width:200px'>
                    <div id='item' style='align-self:stretch'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(System.Math.Abs(item.ContentRect.Height - 100) < 2,
                $"align-self:stretch should fill 100px row (got {item.ContentRect.Height})");
        }

        // [CSS-GRID §7.5] Auto height sized from child content of 60px
        [Fact]
        public void AutoHeight_FromContent60()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;width:200px'>
                    <div id='item'><div style='height:60px'></div></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(System.Math.Abs(item.ContentRect.Height - 60) < 2,
                $"Auto height should match child content 60px (got {item.ContentRect.Height})");
        }

        // [CSS-GRID §10.4] min-height 100px enforced when row is auto
        [Fact]
        public void MinHeight100_Enforced()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;width:200px'>
                    <div id='item' style='min-height:100px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(item.ContentRect.Height >= 99,
                $"min-height:100px should be enforced (got {item.ContentRect.Height})");
        }

        // [CSS-GRID §10.4] max-height 60px clamps stretched height in 100px row
        [Fact(Skip = "Known bug: max-height not applied to stretched grid item")]
        public void MaxHeight60_ClampsStretchIn100Row()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-template-rows:100px;width:200px'>
                    <div id='item' style='max-height:60px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(item.ContentRect.Height <= 61,
                $"max-height:60px should clamp stretch (got {item.ContentRect.Height})");
        }

        // [CSS-GRID §10.4] Padding reduces content height when stretching
        [Fact]
        public void Padding_ReducesContentHeight()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-template-rows:100px;width:200px'>
                    <div id='item' style='padding:15px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(System.Math.Abs(item.ContentRect.Height - 70) < 2,
                $"Content height should be 100 - 15 - 15 = 70 (got {item.ContentRect.Height})");
        }

        // [CSS-GRID §10.4] Border reduces content height when stretching
        [Fact]
        public void Border_ReducesContentHeight()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-template-rows:100px;width:200px'>
                    <div id='item' style='border:10px solid black'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(System.Math.Abs(item.ContentRect.Height - 80) < 2,
                $"Content height should be 100 - 10 - 10 = 80 (got {item.ContentRect.Height})");
        }

        // [CSS-GRID §10.4] border-box height includes padding and border
        [Fact]
        public void BorderBoxHeight_IncludesPaddingAndBorder()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-template-rows:100px;width:200px'>
                    <div id='item' style='box-sizing:border-box;height:80px;padding:10px;border:5px solid black'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            float expectedContentHeight = 80 - 10 - 10 - 5 - 5;
            Assert.True(System.Math.Abs(item.ContentRect.Height - expectedContentHeight) < 2,
                $"Border-box content height should be {expectedContentHeight} (got {item.ContentRect.Height})");
        }

        // [CSS-GRID §10.4] Margin reduces available height when stretching
        [Fact]
        public void Margin_ReducesAvailableHeight()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-template-rows:100px;width:200px'>
                    <div id='item' style='margin:20px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(System.Math.Abs(item.ContentRect.Height - 60) < 2,
                $"Content height should be 100 - 20 - 20 = 60 (got {item.ContentRect.Height})");
        }

        // [CSS-GRID §10.4] Explicit height:0 is valid
        [Fact]
        public void HeightZero_IsValid()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-template-rows:100px;width:200px'>
                    <div id='item' style='height:0'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(System.Math.Abs(item.ContentRect.Height - 0) < 2,
                $"height:0 should produce 0 content height (got {item.ContentRect.Height})");
        }

        // [CSS-GRID §10.4] Two rows with different heights, items stretch to respective rows
        [Fact]
        public void TwoRows_DifferentHeights_StretchToRespectiveRows()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-template-rows:60px 120px;width:200px'>
                    <div id='first'></div>
                    <div id='second'></div>
                </div></body>");
            var first = LayoutTestHelper.FindById(root, "first")!;
            var second = LayoutTestHelper.FindById(root, "second")!;
            Assert.True(System.Math.Abs(first.ContentRect.Height - 60) < 2,
                $"First row item should stretch to 60px (got {first.ContentRect.Height})");
            Assert.True(System.Math.Abs(second.ContentRect.Height - 120) < 2,
                $"Second row item should stretch to 120px (got {second.ContentRect.Height})");
        }

        // [CSS-GRID §10.1] Row gap affects Y position but not item height
        [Fact]
        public void RowGap_AffectsYPositionNotHeight()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-template-rows:50px 50px;row-gap:20px;width:200px'>
                    <div id='first'></div>
                    <div id='second'></div>
                </div></body>");
            var first = LayoutTestHelper.FindById(root, "first")!;
            var second = LayoutTestHelper.FindById(root, "second")!;
            Assert.True(System.Math.Abs(first.ContentRect.Height - 50) < 2,
                $"First row height should be 50px (got {first.ContentRect.Height})");
            Assert.True(System.Math.Abs(second.ContentRect.Height - 50) < 2,
                $"Second row height should be 50px (got {second.ContentRect.Height})");
            Assert.True(System.Math.Abs(second.ContentRect.Y - 70) < 2,
                $"Second row Y should be 50+20=70 (got {second.ContentRect.Y})");
        }

        // [CSS-GRID §10.4] Padding and border combined reduce content height
        [Fact]
        public void PaddingAndBorder_CombinedReduceContentHeight()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-template-rows:100px;width:200px'>
                    <div id='item' style='padding:8px;border:2px solid black'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            float expectedContentHeight = 100 - 8 - 8 - 2 - 2;
            Assert.True(System.Math.Abs(item.ContentRect.Height - expectedContentHeight) < 2,
                $"Content height should be {expectedContentHeight} (got {item.ContentRect.Height})");
        }

        // [CSS-GRID §10.4] Asymmetric vertical padding reduces content height
        [Fact]
        public void AsymmetricPadding_ReducesContentHeight()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-template-rows:100px;width:200px'>
                    <div id='item' style='padding-top:10px;padding-bottom:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(System.Math.Abs(item.ContentRect.Height - 60) < 2,
                $"Content height should be 100 - 10 - 30 = 60 (got {item.ContentRect.Height})");
        }

        // [CSS-GRID §10.4] Stretch with margin and border combined
        [Fact]
        public void MarginAndBorder_CombinedReduceHeight()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-template-rows:100px;width:200px'>
                    <div id='item' style='margin:10px;border:5px solid black'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            float expectedContentHeight = 100 - 10 - 10 - 5 - 5;
            Assert.True(System.Math.Abs(item.ContentRect.Height - expectedContentHeight) < 2,
                $"Content height should be {expectedContentHeight} (got {item.ContentRect.Height})");
        }

        // [CSS-GRID §10.4] Three rows with different heights
        [Fact]
        public void ThreeRows_DifferentHeights_StretchCorrectly()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-template-rows:40px 60px 80px;width:200px'>
                    <div id='first'></div>
                    <div id='second'></div>
                    <div id='third'></div>
                </div></body>");
            var first = LayoutTestHelper.FindById(root, "first")!;
            var second = LayoutTestHelper.FindById(root, "second")!;
            var third = LayoutTestHelper.FindById(root, "third")!;
            Assert.True(System.Math.Abs(first.ContentRect.Height - 40) < 2,
                $"First row should be 40px (got {first.ContentRect.Height})");
            Assert.True(System.Math.Abs(second.ContentRect.Height - 60) < 2,
                $"Second row should be 60px (got {second.ContentRect.Height})");
            Assert.True(System.Math.Abs(third.ContentRect.Height - 80) < 2,
                $"Third row should be 80px (got {third.ContentRect.Height})");
        }

        // [CSS-GRID §10.4] Explicit height larger than row track
        [Fact]
        public void ExplicitHeight_LargerThanRow_Overflows()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-template-rows:50px;width:200px'>
                    <div id='item' style='height:80px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(System.Math.Abs(item.ContentRect.Height - 80) < 2,
                $"Explicit height 80px should overflow 50px row (got {item.ContentRect.Height})");
        }

        // [CSS-GRID §10.4] min-height with explicit smaller height
        [Fact]
        public void MinHeight_OverridesExplicitSmallerHeight()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;width:200px'>
                    <div id='item' style='height:30px;min-height:80px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(item.ContentRect.Height >= 79,
                $"min-height:80px should override height:30px (got {item.ContentRect.Height})");
        }

        // [CSS-GRID §10.4] max-height with explicit larger height
        [Fact]
        public void MaxHeight_ClampsExplicitLargerHeight()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;width:200px'>
                    <div id='item' style='height:120px;max-height:60px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(item.ContentRect.Height <= 61,
                $"max-height:60px should clamp height:120px (got {item.ContentRect.Height})");
        }

        // [CSS-GRID §10.4] border-box height with only border
        [Fact]
        public void BorderBoxHeight_WithOnlyBorder()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-template-rows:100px;width:200px'>
                    <div id='item' style='box-sizing:border-box;height:60px;border:10px solid black'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            float expectedContentHeight = 60 - 10 - 10;
            Assert.True(System.Math.Abs(item.ContentRect.Height - expectedContentHeight) < 2,
                $"Border-box content height should be {expectedContentHeight} (got {item.ContentRect.Height})");
        }

        // [CSS-GRID §10.4] Percentage height 100% fills entire row
        [Fact(Skip = "Known bug: percentage height on grid item does not resolve against row track")]
        public void PercentageHeight100_FillsEntireRow()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-template-rows:80px;width:200px'>
                    <div id='item' style='height:100%'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(System.Math.Abs(item.ContentRect.Height - 80) < 2,
                $"100% height should fill 80px row (got {item.ContentRect.Height})");
        }

        // [CSS-GRID §10.5] align-self:end in second row with gap
        [Fact]
        public void AlignSelfEnd_InSecondRow_WithGap()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-template-rows:50px 80px;row-gap:10px;width:200px'>
                    <div id='first' style='height:20px'></div>
                    <div id='second' style='align-self:end;height:30px'></div>
                </div></body>");
            var second = LayoutTestHelper.FindById(root, "second")!;
            Assert.True(System.Math.Abs(second.ContentRect.Height - 30) < 2,
                $"Height should be 30px (got {second.ContentRect.Height})");
            Assert.True(System.Math.Abs(second.ContentRect.Y - 110) < 2,
                $"Y should be 50+10+80-30=110 (got {second.ContentRect.Y})");
        }

        // [CSS-GRID §10.4] Auto height from multiple children
        [Fact]
        public void AutoHeight_FromMultipleChildren()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;width:200px'>
                    <div id='item'><div style='height:25px'></div><div style='height:35px'></div></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(System.Math.Abs(item.ContentRect.Height - 60) < 2,
                $"Auto height should be sum of children 25+35=60 (got {item.ContentRect.Height})");
        }
    }
}
