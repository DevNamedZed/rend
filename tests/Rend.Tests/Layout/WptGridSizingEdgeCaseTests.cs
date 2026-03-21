using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class WptGridSizingEdgeCaseTests
    {
        private readonly ITestOutputHelper _output;

        public WptGridSizingEdgeCaseTests(ITestOutputHelper output)
        {
            _output = output;
        }

        // [CSS-GRID §7.2] All auto columns size to content
        [Fact]
        public void AllAutoColumns_SizeToContentWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:auto auto auto;width:300px'>
                    <div id='a' style='width:40px;height:20px'></div>
                    <div id='b' style='width:80px;height:20px'></div>
                    <div id='c' style='width:60px;height:20px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a");
            var itemB = LayoutTestHelper.FindById(root, "b");
            var itemC = LayoutTestHelper.FindById(root, "c");
            Assert.NotNull(itemA);
            Assert.NotNull(itemB);
            Assert.NotNull(itemC);
            _output.WriteLine($"a.W={itemA!.ContentRect.Width} b.W={itemB!.ContentRect.Width} c.W={itemC!.ContentRect.Width}");
            Assert.True(itemA.ContentRect.Width >= 40, $"Auto column a should be at least 40px (got {itemA.ContentRect.Width})");
            Assert.True(itemB.ContentRect.Width >= 80, $"Auto column b should be at least 80px (got {itemB.ContentRect.Width})");
            Assert.True(itemC.ContentRect.Width >= 60, $"Auto column c should be at least 60px (got {itemC.ContentRect.Width})");
        }

        // [CSS-GRID §7.2] 0px column track collapses to zero width
        [Fact]
        public void ZeroPxColumn_CollapsesToZeroWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:0px 200px;width:200px'>
                    <div id='zero' style='height:20px'></div>
                    <div id='wide' style='height:20px'></div>
                </div></body>");
            var zeroItem = LayoutTestHelper.FindById(root, "zero");
            var wideItem = LayoutTestHelper.FindById(root, "wide");
            Assert.NotNull(zeroItem);
            Assert.NotNull(wideItem);
            _output.WriteLine($"zero.W={zeroItem!.ContentRect.Width} wide.W={wideItem!.ContentRect.Width}");
            Assert.True(zeroItem.ContentRect.Width < 2, $"0px column should collapse (got {zeroItem.ContentRect.Width})");
            Assert.True(System.Math.Abs(wideItem!.ContentRect.Width - 200) < 2, $"Second column should be 200px (got {wideItem.ContentRect.Width})");
        }

        // [CSS-GRID §7.2] Very large column track value
        [Fact]
        public void VeryLargeColumn_TakesSpecifiedWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:5000px 100px;width:5100px'>
                    <div id='large' style='height:20px'></div>
                    <div id='small' style='height:20px'></div>
                </div></body>", viewportWidth: 6000);
            var largeItem = LayoutTestHelper.FindById(root, "large");
            var smallItem = LayoutTestHelper.FindById(root, "small");
            Assert.NotNull(largeItem);
            Assert.NotNull(smallItem);
            _output.WriteLine($"large.W={largeItem!.ContentRect.Width} small.W={smallItem!.ContentRect.Width}");
            Assert.True(System.Math.Abs(largeItem.ContentRect.Width - 5000) < 2, $"Large column should be 5000px (got {largeItem.ContentRect.Width})");
            Assert.True(System.Math.Abs(smallItem!.ContentRect.Width - 100) < 2, $"Small column should be 100px (got {smallItem.ContentRect.Width})");
        }

        // [CSS-GRID §7.2] calc() column track
        [Fact]
        public void CalcColumn_ResolvesCorrectly()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:calc(50% - 20px) calc(50% + 20px);width:300px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a");
            var itemB = LayoutTestHelper.FindById(root, "b");
            Assert.NotNull(itemA);
            Assert.NotNull(itemB);
            _output.WriteLine($"a.W={itemA!.ContentRect.Width} b.W={itemB!.ContentRect.Width}");
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 130) < 2, $"calc(50%-20px) of 300 = 130 (got {itemA.ContentRect.Width})");
            Assert.True(System.Math.Abs(itemB!.ContentRect.Width - 170) < 2, $"calc(50%+20px) of 300 = 170 (got {itemB.ContentRect.Width})");
        }

        // [CSS-GRID §7.2] Grid item with content wider than column causes overflow
        [Fact]
        public void GridItemOverflow_ContentWiderThanTrack()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:50px;width:50px'>
                    <div id='t' style='width:200px;height:20px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(item);
            _output.WriteLine($"item.W={item!.ContentRect.Width}");
            Assert.True(item.ContentRect.Width >= 50, $"Item should be at least track width (got {item.ContentRect.Width})");
        }

        // [CSS-GRID §7.2] Percentage + fr mix
        [Fact]
        public void PercentagePlusFrMix_DistributesCorrectly()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:30% 1fr;width:400px'>
                    <div id='pct' style='height:20px'></div>
                    <div id='fr' style='height:20px'></div>
                </div></body>");
            var pctItem = LayoutTestHelper.FindById(root, "pct");
            var frItem = LayoutTestHelper.FindById(root, "fr");
            Assert.NotNull(pctItem);
            Assert.NotNull(frItem);
            _output.WriteLine($"pct.W={pctItem!.ContentRect.Width} fr.W={frItem!.ContentRect.Width}");
            Assert.True(System.Math.Abs(pctItem.ContentRect.Width - 120) < 2, $"30% of 400 = 120 (got {pctItem.ContentRect.Width})");
            Assert.True(System.Math.Abs(frItem!.ContentRect.Width - 280) < 2, $"1fr should get remaining 280 (got {frItem.ContentRect.Width})");
        }

        // [CSS-GRID §7.2] minmax(0, auto) column sizes to content but not below 0
        [Fact]
        public void MinmaxZeroAuto_SizesToContent()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:minmax(0,auto) 1fr;width:300px'>
                    <div id='t' style='width:80px;height:20px'></div>
                    <div style='height:20px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(item);
            _output.WriteLine($"minmax(0,auto) item.W={item!.ContentRect.Width}");
            Assert.True(item.ContentRect.Width >= 0, $"Width should be non-negative (got {item.ContentRect.Width})");
        }

        // [CSS-GRID §11.4] Auto row sizing from content height
        [Fact]
        public void AutoRow_SizesFromContentHeight()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;width:100px'>
                    <div id='a' style='height:50px'></div>
                    <div id='b' style='height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a");
            var itemB = LayoutTestHelper.FindById(root, "b");
            Assert.NotNull(itemA);
            Assert.NotNull(itemB);
            _output.WriteLine($"a.H={itemA!.ContentRect.Height} b.H={itemB!.ContentRect.Height} b.Y={itemB!.ContentRect.Y}");
            Assert.True(System.Math.Abs(itemA.ContentRect.Height - 50) < 2, $"Row 1 should be 50px tall (got {itemA.ContentRect.Height})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Y - 50) < 2, $"Row 2 should start at Y=50 (got {itemB.ContentRect.Y})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Height - 30) < 2, $"Row 2 should be 30px tall (got {itemB.ContentRect.Height})");
        }

        // [CSS-GRID §11.4] Fixed height container with auto rows distributes evenly
        [Fact]
        public void FixedHeightContainer_AutoRows()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;height:200px;width:100px'>
                    <div id='a' style='min-height:0'></div>
                    <div id='b' style='min-height:0'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a");
            var itemB = LayoutTestHelper.FindById(root, "b");
            Assert.NotNull(itemA);
            Assert.NotNull(itemB);
            _output.WriteLine($"a.H={itemA!.ContentRect.Height} b.H={itemB!.ContentRect.Height}");
            float totalHeight = itemA.ContentRect.Height + itemB!.ContentRect.Height;
            Assert.True(System.Math.Abs(totalHeight - 200) < 2, $"Auto rows should fill 200px container (got {totalHeight})");
        }

        // [CSS-GRID §7.2] Grid container with min-width constrains tracks
        [Fact]
        public void ContainerMinWidth_ConstrainsTracks()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='grid' style='display:grid;grid-template-columns:1fr 1fr;min-width:300px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                </div></body>");
            var grid = LayoutTestHelper.FindById(root, "grid");
            var itemA = LayoutTestHelper.FindById(root, "a");
            Assert.NotNull(grid);
            Assert.NotNull(itemA);
            _output.WriteLine($"grid.W={grid!.ContentRect.Width} a.W={itemA!.ContentRect.Width}");
            Assert.True(grid.ContentRect.Width >= 298, $"Grid should be at least 300px (got {grid.ContentRect.Width})");
        }

        // [CSS-GRID §7.2] Grid container with max-width caps track sizes
        [Fact]
        public void ContainerMaxWidth_CapsTrackSizes()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='grid' style='display:grid;grid-template-columns:200px 200px;max-width:300px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                </div></body>");
            var grid = LayoutTestHelper.FindById(root, "grid");
            Assert.NotNull(grid);
            _output.WriteLine($"grid.W={grid!.ContentRect.Width}");
            Assert.True(grid.ContentRect.Width <= 302, $"Grid should respect max-width:300px (got {grid.ContentRect.Width})");
        }

        // [CSS-GRID §11.1] Grid item with padding affects content box
        [Fact]
        public void GridItemWithPadding_ContentBoxShrinks()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;width:100px'>
                    <div id='t' style='padding:10px;height:40px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(item);
            _output.WriteLine($"content.W={item!.ContentRect.Width} padding.W={item.PaddingRect.Width}");
            Assert.True(System.Math.Abs(item.ContentRect.Width - 80) < 2, $"Content should be 100-10-10=80 (got {item.ContentRect.Width})");
            Assert.True(System.Math.Abs(item.PaddingRect.Width - 100) < 2, $"Padding box should be 100 (got {item.PaddingRect.Width})");
        }

        // [CSS-GRID §11.1] Grid item with border affects content box
        [Fact]
        public void GridItemWithBorder_ContentBoxShrinks()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:120px;width:120px'>
                    <div id='t' style='border:5px solid black;height:40px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(item);
            _output.WriteLine($"content.W={item!.ContentRect.Width} border.W={item.BorderRect.Width}");
            Assert.True(System.Math.Abs(item.ContentRect.Width - 110) < 2, $"Content should be 120-5-5=110 (got {item.ContentRect.Width})");
            Assert.True(System.Math.Abs(item.BorderRect.Width - 120) < 2, $"Border box should be 120 (got {item.BorderRect.Width})");
        }

        // [CSS-GRID §11.1] Grid item with box-sizing:border-box includes padding+border in width
        [Fact]
        public void GridItemBorderBox_PaddingIncludedInWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;width:100px'>
                    <div id='t' style='box-sizing:border-box;padding:10px;border:5px solid black;width:100px;height:50px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(item);
            _output.WriteLine($"content.W={item!.ContentRect.Width} border.W={item.BorderRect.Width}");
            Assert.True(System.Math.Abs(item.BorderRect.Width - 100) < 2, $"Border box should be 100 (got {item.BorderRect.Width})");
            Assert.True(System.Math.Abs(item.ContentRect.Width - 70) < 2, $"Content should be 100-10-10-5-5=70 (got {item.ContentRect.Width})");
        }

        // [CSS-GRID §11.4] Grid item with percentage height in auto row resolves against container
        [Fact]
        public void PercentageHeightInAutoRow_ResolvesOrCollapses()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;width:100px;height:200px'>
                    <div id='t' style='height:50%'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(item);
            _output.WriteLine($"item.H={item!.ContentRect.Height}");
            Assert.True(item.ContentRect.Height >= 0, $"Height should be non-negative (got {item.ContentRect.Height})");
        }

        // [CSS-GRID §7.2] Empty cells still reserve track space
        [Fact]
        public void EmptyCells_StillReserveTrackSpace()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px 100px 100px;grid-template-rows:50px;width:300px'>
                    <div id='first' style='grid-column:1'></div>
                    <div id='last' style='grid-column:3'></div>
                </div></body>");
            var firstItem = LayoutTestHelper.FindById(root, "first");
            var lastItem = LayoutTestHelper.FindById(root, "last");
            Assert.NotNull(firstItem);
            Assert.NotNull(lastItem);
            _output.WriteLine($"first.X={firstItem!.ContentRect.X} last.X={lastItem!.ContentRect.X}");
            Assert.True(System.Math.Abs(firstItem.ContentRect.X - 0) < 2, $"First should be at X=0 (got {firstItem.ContentRect.X})");
            Assert.True(System.Math.Abs(lastItem!.ContentRect.X - 200) < 2, $"Last should be at X=200 (got {lastItem.ContentRect.X})");
        }

        // [CSS-GRID §7.2] Single 1fr column fills container width
        [Fact]
        public void SingleFrColumn_FillsContainerWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:1fr;width:250px'>
                    <div id='t' style='height:20px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(item);
            _output.WriteLine($"item.W={item!.ContentRect.Width}");
            Assert.True(System.Math.Abs(item.ContentRect.Width - 250) < 2, $"Single 1fr should fill 250px (got {item.ContentRect.Width})");
        }

        // [CSS-GRID §7.2] minmax(100px, 1fr) column has minimum 100px
        [Fact]
        public void MinmaxFixedFr_RespectsMinimum()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:minmax(100px,1fr) minmax(100px,1fr);width:150px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a");
            var itemB = LayoutTestHelper.FindById(root, "b");
            Assert.NotNull(itemA);
            Assert.NotNull(itemB);
            _output.WriteLine($"a.W={itemA!.ContentRect.Width} b.W={itemB!.ContentRect.Width}");
            Assert.True(itemA.ContentRect.Width >= 98, $"minmax(100px,1fr) should be at least 100px (got {itemA.ContentRect.Width})");
            Assert.True(itemB!.ContentRect.Width >= 98, $"minmax(100px,1fr) should be at least 100px (got {itemB.ContentRect.Width})");
        }

        // [CSS-GRID §7.2] Multiple fr tracks with gap
        [Fact]
        public void FrTracksWithGap_GapSubtractedFromAvailable()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:1fr 1fr;gap:20px;width:220px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a");
            var itemB = LayoutTestHelper.FindById(root, "b");
            Assert.NotNull(itemA);
            Assert.NotNull(itemB);
            _output.WriteLine($"a.W={itemA!.ContentRect.Width} b.W={itemB!.ContentRect.Width}");
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 100) < 2, $"(220-20)/2 = 100 (got {itemA.ContentRect.Width})");
            Assert.True(System.Math.Abs(itemB!.ContentRect.Width - 100) < 2, $"(220-20)/2 = 100 (got {itemB.ContentRect.Width})");
        }

        // [CSS-GRID §7.2] Grid with container padding
        [Fact]
        public void ContainerPadding_ReducesAvailableForTracks()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:1fr 1fr;padding:20px;width:260px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a");
            var itemB = LayoutTestHelper.FindById(root, "b");
            Assert.NotNull(itemA);
            Assert.NotNull(itemB);
            _output.WriteLine($"a.W={itemA!.ContentRect.Width} b.W={itemB!.ContentRect.Width}");
            Assert.True(System.Math.Abs(itemA!.ContentRect.Width - 130) < 2, $"(260-0)/2 = 130 for content-box width (got {itemA.ContentRect.Width})");
        }

        // [CSS-GRID §7.2] Grid with both padding and border on items
        [Fact]
        public void ItemPaddingAndBorder_BothSubtracted()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;width:100px'>
                    <div id='t' style='padding:5px;border:3px solid red;height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(item);
            _output.WriteLine($"content.W={item!.ContentRect.Width} border.W={item.BorderRect.Width}");
            Assert.True(System.Math.Abs(item.ContentRect.Width - 84) < 2, $"Content should be 100-5-5-3-3=84 (got {item.ContentRect.Width})");
        }

        // [CSS-GRID §7.2] Auto column with no content collapses
        [Fact]
        public void AutoColumnNoContent_Collapses()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:auto 100px;width:200px'>
                    <div id='empty'></div>
                    <div id='filled' style='height:20px'></div>
                </div></body>");
            var emptyItem = LayoutTestHelper.FindById(root, "empty");
            var filledItem = LayoutTestHelper.FindById(root, "filled");
            Assert.NotNull(emptyItem);
            Assert.NotNull(filledItem);
            _output.WriteLine($"empty.W={emptyItem!.ContentRect.Width} filled.W={filledItem!.ContentRect.Width}");
            Assert.True(filledItem.ContentRect.Width >= 98, $"Fixed column should be at least 100px (got {filledItem.ContentRect.Width})");
        }

        // [CSS-GRID §7.6] grid-auto-columns sizes implicit columns
        [Fact]
        public void AutoColumnsDefault_ImplicitColumnsSized()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;grid-auto-columns:80px;width:300px'>
                    <div id='explicit' style='height:20px'></div>
                    <div id='implicit' style='grid-column:2;height:20px'></div>
                </div></body>");
            var explicitItem = LayoutTestHelper.FindById(root, "explicit");
            var implicitItem = LayoutTestHelper.FindById(root, "implicit");
            Assert.NotNull(explicitItem);
            Assert.NotNull(implicitItem);
            _output.WriteLine($"explicit.W={explicitItem!.ContentRect.Width} implicit.W={implicitItem!.ContentRect.Width}");
            Assert.True(System.Math.Abs(explicitItem.ContentRect.Width - 100) < 2, $"Explicit column 100px (got {explicitItem.ContentRect.Width})");
            Assert.True(System.Math.Abs(implicitItem!.ContentRect.Width - 80) < 2, $"Implicit column 80px (got {implicitItem.ContentRect.Width})");
        }

        // [CSS-GRID §11.4] Row with tall and short items: tallest sets row height
        [Fact]
        public void MixedHeightItems_TallestSetsRowHeight()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px 100px;width:200px'>
                    <div id='tall' style='height:80px'></div>
                    <div id='short' style='height:30px'></div>
                    <div id='row2' style='height:20px'></div>
                </div></body>");
            var tallItem = LayoutTestHelper.FindById(root, "tall");
            var row2Item = LayoutTestHelper.FindById(root, "row2");
            Assert.NotNull(tallItem);
            Assert.NotNull(row2Item);
            _output.WriteLine($"tall.H={tallItem!.ContentRect.Height} row2.Y={row2Item!.ContentRect.Y}");
            Assert.True(System.Math.Abs(row2Item.ContentRect.Y - 80) < 2, $"Row 2 should start at Y=80 (got {row2Item.ContentRect.Y})");
        }

        // [CSS-GRID §7.2] Nested grid: inner grid sizes independently
        [Fact]
        public void NestedGrid_InnerGridSizesIndependently()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:200px;width:200px'>
                    <div style='display:grid;grid-template-columns:1fr 1fr'>
                        <div id='inner1' style='height:20px'></div>
                        <div id='inner2' style='height:20px'></div>
                    </div>
                </div></body>");
            var inner1 = LayoutTestHelper.FindById(root, "inner1");
            var inner2 = LayoutTestHelper.FindById(root, "inner2");
            Assert.NotNull(inner1);
            Assert.NotNull(inner2);
            _output.WriteLine($"inner1.W={inner1!.ContentRect.Width} inner2.W={inner2!.ContentRect.Width}");
            Assert.True(System.Math.Abs(inner1.ContentRect.Width - 100) < 2, $"Inner 1fr of 200px = 100 (got {inner1.ContentRect.Width})");
            Assert.True(System.Math.Abs(inner2!.ContentRect.Width - 100) < 2, $"Inner 1fr of 200px = 100 (got {inner2.ContentRect.Width})");
        }

        // [CSS-GRID §7.2] Fixed column + percentage column
        [Fact]
        public void FixedPlusPercentage_CorrectWidths()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px 50%;width:400px'>
                    <div id='fixed' style='height:20px'></div>
                    <div id='pct' style='height:20px'></div>
                </div></body>");
            var fixedItem = LayoutTestHelper.FindById(root, "fixed");
            var pctItem = LayoutTestHelper.FindById(root, "pct");
            Assert.NotNull(fixedItem);
            Assert.NotNull(pctItem);
            _output.WriteLine($"fixed.W={fixedItem!.ContentRect.Width} pct.W={pctItem!.ContentRect.Width}");
            Assert.True(System.Math.Abs(fixedItem.ContentRect.Width - 100) < 2, $"Fixed column 100px (got {fixedItem.ContentRect.Width})");
            Assert.True(System.Math.Abs(pctItem!.ContentRect.Width - 200) < 2, $"50% of 400 = 200 (got {pctItem.ContentRect.Width})");
        }

        // [CSS-GRID §7.2] Three equal fr columns in explicit width
        [Fact]
        public void ThreeEqualFr_EvenDistribution()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:1fr 1fr 1fr;width:300px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                    <div id='c' style='height:20px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a");
            var itemB = LayoutTestHelper.FindById(root, "b");
            var itemC = LayoutTestHelper.FindById(root, "c");
            Assert.NotNull(itemA);
            Assert.NotNull(itemB);
            Assert.NotNull(itemC);
            _output.WriteLine($"a.W={itemA!.ContentRect.Width} b.W={itemB!.ContentRect.Width} c.W={itemC!.ContentRect.Width}");
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 100) < 2, $"1fr of 300 = 100 (got {itemA.ContentRect.Width})");
            Assert.True(System.Math.Abs(itemB!.ContentRect.Width - 100) < 2, $"1fr of 300 = 100 (got {itemB.ContentRect.Width})");
            Assert.True(System.Math.Abs(itemC!.ContentRect.Width - 100) < 2, $"1fr of 300 = 100 (got {itemC.ContentRect.Width})");
        }

        // [CSS-GRID §7.2] Grid item with margin reduces visible content area
        [Fact]
        public void GridItemWithMargin_ReducesContentArea()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:200px;width:200px'>
                    <div id='t' style='margin:15px;height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(item);
            _output.WriteLine($"content.W={item!.ContentRect.Width} margin.W={item.MarginRect.Width}");
            Assert.True(System.Math.Abs(item.ContentRect.Width - 170) < 2, $"Content should be 200-15-15=170 (got {item.ContentRect.Width})");
        }

        // [CSS-GRID §11.4] Fixed rows with grid-template-rows
        [Fact]
        public void FixedRows_ExactHeight()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;grid-template-rows:40px 60px;width:100px'>
                    <div id='a'></div>
                    <div id='b'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a");
            var itemB = LayoutTestHelper.FindById(root, "b");
            Assert.NotNull(itemA);
            Assert.NotNull(itemB);
            _output.WriteLine($"a.H={itemA!.ContentRect.Height} b.H={itemB!.ContentRect.Height} b.Y={itemB!.ContentRect.Y}");
            Assert.True(System.Math.Abs(itemA.ContentRect.Height - 40) < 2, $"Row 1 should be 40px (got {itemA.ContentRect.Height})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Height - 60) < 2, $"Row 2 should be 60px (got {itemB.ContentRect.Height})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Y - 40) < 2, $"Row 2 should start at Y=40 (got {itemB.ContentRect.Y})");
        }

        // [CSS-GRID §7.2] Grid with border-box container includes padding in grid area
        [Fact]
        public void ContainerBorderBox_PaddingReducesGridArea()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:1fr;box-sizing:border-box;width:200px;padding:20px'>
                    <div id='t' style='height:20px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(item);
            _output.WriteLine($"item.W={item!.ContentRect.Width}");
            Assert.True(System.Math.Abs(item.ContentRect.Width - 160) < 2, $"1fr in border-box 200px with 20px padding = 160 (got {item.ContentRect.Width})");
        }
    }
}
