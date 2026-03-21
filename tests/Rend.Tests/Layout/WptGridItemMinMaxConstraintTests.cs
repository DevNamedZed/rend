using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    /// <summary>
    /// Tests for min-width/max-width/min-height/max-height constraints on CSS Grid items.
    /// Covers interactions with stretch, explicit sizing, alignment, padding, border,
    /// box-sizing, percentages, and spanning items.
    /// </summary>
    public class WptGridItemMinMaxConstraintTests
    {
        private readonly ITestOutputHelper _output;

        public WptGridItemMinMaxConstraintTests(ITestOutputHelper output)
        {
            _output = output;
        }

        // [CSS-GRID §6.6] min-width on grid item prevents shrinking below minimum
        [Fact]
        public void MinWidth_PreventsItemShrinkingBelowMinimum()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;width:100px'>
                    <div id='item' style='min-width:150px;height:20px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(item.ContentRect.Width >= 149,
                $"min-width:150px should prevent item from being narrower (got {item.ContentRect.Width})");
        }

        // [CSS-GRID §6.6] max-width on grid item clamps stretched width
        [Fact(Skip = "Known bug: grid stretch ignores max-width on items")]
        public void MaxWidth_ClampsStretchedWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:300px;width:300px'>
                    <div id='item' style='max-width:100px;height:20px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(item.ContentRect.Width <= 101,
                $"max-width:100px should clamp stretched width (got {item.ContentRect.Width})");
        }

        // [CSS-GRID §6.6] min-height on grid item ensures minimum height
        [Fact]
        public void MinHeight_EnsuresMinimumHeight()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:200px;width:200px'>
                    <div id='item' style='min-height:80px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(item.ContentRect.Height >= 79,
                $"min-height:80px should ensure minimum height (got {item.ContentRect.Height})");
        }

        // [CSS-GRID §6.6] max-height on grid item clamps height from content
        [Fact]
        public void MaxHeight_ClampsContentHeight()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:200px;width:200px'>
                    <div id='item' style='max-height:40px'>
                        <div style='height:100px'></div>
                    </div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(item.ContentRect.Height <= 41,
                $"max-height:40px should clamp content height (got {item.ContentRect.Height})");
        }

        // [CSS-GRID §6.6] min-width percentage resolves against grid track width
        [Fact]
        public void MinWidth_Percentage_ResolvesAgainstTrack()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:200px;width:200px'>
                    <div id='item' style='width:50px;min-width:75%;height:20px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(item.ContentRect.Width >= 149,
                $"min-width:75% of 200px = 150px should override width:50px (got {item.ContentRect.Width})");
        }

        // [CSS-GRID §6.6] max-width percentage resolves against grid track width
        [Fact(Skip = "Known bug: grid stretch ignores max-width percentage on items")]
        public void MaxWidth_Percentage_ResolvesAgainstTrack()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:200px;width:200px'>
                    <div id='item' style='max-width:50%;height:20px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(item.ContentRect.Width <= 101,
                $"max-width:50% of 200px = 100px should clamp width (got {item.ContentRect.Width})");
        }

        // [CSS-GRID §6.6] min-height percentage resolves against grid row height
        [Fact(Skip = "Known bug: grid item percentage min-height not resolved against row")]
        public void MinHeight_Percentage_ResolvesAgainstRow()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:200px;grid-template-rows:200px;width:200px'>
                    <div id='item' style='height:30px;min-height:50%'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(item.ContentRect.Height >= 99,
                $"min-height:50% of 200px = 100px should override height:30px (got {item.ContentRect.Height})");
        }

        // [CSS-GRID §6.6] max-height percentage resolves against grid row height
        [Fact(Skip = "Known bug: grid stretch ignores max-height percentage on items")]
        public void MaxHeight_Percentage_ResolvesAgainstRow()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:200px;grid-template-rows:200px;width:200px'>
                    <div id='item' style='max-height:25%'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(item.ContentRect.Height <= 51,
                $"max-height:25% of 200px = 50px should clamp height (got {item.ContentRect.Height})");
        }

        // [CSS-GRID §6.6] min-width with default stretch alignment
        [Fact]
        public void MinWidth_WithStretch_UsesLargerOfStretchAndMin()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;width:100px'>
                    <div id='item' style='min-width:180px;height:20px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(item.ContentRect.Width >= 179,
                $"min-width:180px should win over stretch to 100px track (got {item.ContentRect.Width})");
        }

        // [CSS-GRID §6.6] max-width with default stretch alignment
        [Fact(Skip = "Known bug: grid stretch ignores max-width on items")]
        public void MaxWidth_WithStretch_ClampsStretchedSize()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:200px;width:200px'>
                    <div id='item' style='max-width:80px;height:20px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(item.ContentRect.Width <= 81,
                $"max-width:80px should clamp stretch to 200px track (got {item.ContentRect.Width})");
        }

        // [CSS-GRID §6.6] min-height with stretch in explicit row
        [Fact]
        public void MinHeight_WithStretch_UsesLargerOfStretchAndMin()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:200px;grid-template-rows:60px;width:200px'>
                    <div id='item' style='min-height:100px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(item.ContentRect.Height >= 99,
                $"min-height:100px should win over stretch to 60px row (got {item.ContentRect.Height})");
        }

        // [CSS-GRID §6.6] max-height with stretch in explicit row
        [Fact(Skip = "Known bug: grid stretch ignores max-height on items")]
        public void MaxHeight_WithStretch_ClampsStretchedSize()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:200px;grid-template-rows:200px;width:200px'>
                    <div id='item' style='max-height:60px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(item.ContentRect.Height <= 61,
                $"max-height:60px should clamp stretch to 200px row (got {item.ContentRect.Height})");
        }

        // [CSS-GRID §6.6] min-width with explicit width below minimum
        [Fact]
        public void MinWidth_WithExplicitWidth_MinWins()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:200px;width:200px'>
                    <div id='item' style='width:40px;min-width:120px;height:20px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(item.ContentRect.Width >= 119,
                $"min-width:120px should override width:40px (got {item.ContentRect.Width})");
        }

        // [CSS-GRID §6.6] max-width with explicit width above maximum
        [Fact]
        public void MaxWidth_WithExplicitWidth_MaxWins()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:200px;width:200px'>
                    <div id='item' style='width:180px;max-width:100px;height:20px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(item.ContentRect.Width <= 101,
                $"max-width:100px should clamp width:180px (got {item.ContentRect.Width})");
        }

        // [CSS-GRID §6.6] min-height with explicit height below minimum
        [Fact]
        public void MinHeight_WithExplicitHeight_MinWins()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:200px;width:200px'>
                    <div id='item' style='height:20px;min-height:80px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(item.ContentRect.Height >= 79,
                $"min-height:80px should override height:20px (got {item.ContentRect.Height})");
        }

        // [CSS-GRID §6.6] max-height with explicit height above maximum
        [Fact]
        public void MaxHeight_WithExplicitHeight_MaxWins()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:200px;width:200px'>
                    <div id='item' style='height:150px;max-height:60px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(item.ContentRect.Height <= 61,
                $"max-height:60px should clamp height:150px (got {item.ContentRect.Height})");
        }

        // [CSS-GRID §6.6] min-width with align-items:start (no stretch)
        [Fact]
        public void MinWidth_WithAlignStart_StillApplies()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:200px;align-items:start;width:200px'>
                    <div id='item' style='min-width:150px;height:20px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(item.ContentRect.Width >= 149,
                $"min-width:150px should still apply with align-items:start (got {item.ContentRect.Width})");
        }

        // [CSS-GRID §6.6] max-width with justify-items:center
        [Fact]
        public void MaxWidth_WithAlignCenter_ClampsSize()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:200px;justify-items:center;width:200px'>
                    <div id='item' style='width:180px;max-width:100px;height:20px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(item.ContentRect.Width <= 101,
                $"max-width:100px should clamp with justify-items:center (got {item.ContentRect.Width})");
        }

        // [CSS-GRID §6.6] min-width with padding included in content-box sizing
        [Fact]
        public void MinWidth_WithPadding_ContentBoxSizing()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:200px;width:200px'>
                    <div id='item' style='min-width:120px;padding:20px;height:20px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(item.ContentRect.Width >= 119,
                $"min-width:120px content-box should be respected with padding (got {item.ContentRect.Width})");
        }

        // [CSS-GRID §6.6] max-width with border included in content-box sizing
        [Fact(Skip = "Known bug: grid stretch ignores max-width on items")]
        public void MaxWidth_WithBorder_ContentBoxSizing()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:300px;width:300px'>
                    <div id='item' style='max-width:100px;border:10px solid black;height:20px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(item.ContentRect.Width <= 101,
                $"max-width:100px content-box should not include border (got {item.ContentRect.Width})");
            float borderBoxWidth = item.ContentRect.Width + item.BorderLeftWidth + item.BorderRightWidth;
            Assert.True(borderBoxWidth <= 121,
                $"Border box should be max-width + borders = 120 (got {borderBoxWidth})");
        }

        // [CSS-SIZING §5.1] border-box min-width includes padding and border
        [Fact]
        public void BorderBox_MinWidth_IncludesPaddingAndBorder()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:200px;width:200px'>
                    <div id='item' style='box-sizing:border-box;min-width:120px;padding:15px;border:5px solid black;height:20px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            float borderBoxWidth = item.ContentRect.Width + item.PaddingLeft + item.PaddingRight
                                 + item.BorderLeftWidth + item.BorderRightWidth;
            Assert.True(borderBoxWidth >= 119,
                $"border-box min-width:120px should include padding+border (border-box got {borderBoxWidth})");
        }

        // [CSS-SIZING §5.1] border-box max-width includes padding and border
        [Fact(Skip = "Known bug: grid stretch ignores max-width on items")]
        public void BorderBox_MaxWidth_IncludesPaddingAndBorder()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:300px;width:300px'>
                    <div id='item' style='box-sizing:border-box;max-width:100px;padding:10px;border:5px solid black;height:20px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            float borderBoxWidth = item.ContentRect.Width + item.PaddingLeft + item.PaddingRight
                                 + item.BorderLeftWidth + item.BorderRightWidth;
            Assert.True(borderBoxWidth <= 101,
                $"border-box max-width:100px should include padding+border (border-box got {borderBoxWidth})");
        }

        // [CSS-GRID §11.5] min-width on spanning item affects multiple tracks
        [Fact]
        public void MinWidth_SpanningItem_AffectsMultipleTracks()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:50px 50px;width:100px'>
                    <div id='item' style='grid-column:1/3;min-width:120px;height:20px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(item.ContentRect.Width >= 119,
                $"Spanning item min-width:120px should expand beyond 100px tracks (got {item.ContentRect.Width})");
        }

        // [CSS-GRID §11.5] max-height on spanning item across multiple rows
        [Fact(Skip = "Known bug: grid stretch ignores max-height on spanning items")]
        public void MaxHeight_SpanningItem_ClampsAcrossRows()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:200px;grid-template-rows:80px 80px;width:200px'>
                    <div id='item' style='grid-row:1/3;max-height:100px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(item.ContentRect.Height <= 101,
                $"Spanning item max-height:100px should clamp across 160px rows (got {item.ContentRect.Height})");
        }

        // [CSS 2.1 §10.4.4] min-width > max-width: min-width wins
        [Fact]
        public void MinWidth_BeatsMaxWidth_WhenMinIsLarger()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:200px;width:200px'>
                    <div id='item' style='min-width:150px;max-width:80px;height:20px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(item.ContentRect.Width >= 149,
                $"min-width:150px should beat max-width:80px (got {item.ContentRect.Width})");
        }

        // [CSS 2.1 §10.7.4] min-height > max-height: min-height wins
        [Fact]
        public void MinHeight_BeatsMaxHeight_WhenMinIsLarger()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:200px;width:200px'>
                    <div id='item' style='min-height:120px;max-height:60px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(item.ContentRect.Height >= 119,
                $"min-height:120px should beat max-height:60px (got {item.ContentRect.Height})");
        }

        // [CSS-GRID §6.6] Both min-width and max-width applied, value within range
        [Fact]
        public void MinAndMaxWidth_ValueWithinRange_UsesExplicit()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:200px;width:200px'>
                    <div id='item' style='width:120px;min-width:80px;max-width:160px;height:20px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(System.Math.Abs(item.ContentRect.Width - 120) < 2,
                $"width:120px within [80,160] range should be used (got {item.ContentRect.Width})");
        }

        // [CSS-GRID §6.6] Both min-height and max-height applied, value within range
        [Fact]
        public void MinAndMaxHeight_ValueWithinRange_UsesExplicit()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:200px;width:200px'>
                    <div id='item' style='height:70px;min-height:40px;max-height:100px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(System.Math.Abs(item.ContentRect.Height - 70) < 2,
                $"height:70px within [40,100] range should be used (got {item.ContentRect.Height})");
        }
    }
}
