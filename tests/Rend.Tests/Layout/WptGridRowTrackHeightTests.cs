using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    /// <summary>
    /// Tests for grid row track height resolution: fixed, auto, fr, minmax,
    /// repeat, percentage, gap interaction, and container height derivation.
    /// </summary>
    public class WptGridRowTrackHeightTests
    {
        private readonly ITestOutputHelper _output;
        public WptGridRowTrackHeightTests(ITestOutputHelper output) { _output = output; }

        // [CSS-GRID §7.2] Single explicit row track of 50px
        [Fact]
        public void SingleRow_50px_Height()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;grid-template-rows:50px;width:100px'>
                    <div id='item' style='background:red'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item");
            Assert.NotNull(item);
            Assert.True(System.Math.Abs(item!.ContentRect.Height - 50) < 2, $"Expected 50px height, got {item.ContentRect.Height}");
        }

        // [CSS-GRID §7.2] Single explicit row track of 100px
        [Fact]
        public void SingleRow_100px_Height()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;grid-template-rows:100px;width:100px'>
                    <div id='item' style='background:red'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item");
            Assert.NotNull(item);
            Assert.True(System.Math.Abs(item!.ContentRect.Height - 100) < 2, $"Expected 100px height, got {item.ContentRect.Height}");
        }

        // [CSS-GRID §7.2] Two explicit row tracks 40px and 60px
        [Fact]
        public void TwoRows_40px_60px()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;grid-template-rows:40px 60px;width:100px'>
                    <div id='first'></div>
                    <div id='second'></div>
                </div></body>");
            var first = LayoutTestHelper.FindById(root, "first");
            var second = LayoutTestHelper.FindById(root, "second");
            Assert.NotNull(first);
            Assert.NotNull(second);
            Assert.True(System.Math.Abs(first!.ContentRect.Height - 40) < 2, $"First row expected 40px, got {first.ContentRect.Height}");
            Assert.True(System.Math.Abs(second!.ContentRect.Height - 60) < 2, $"Second row expected 60px, got {second.ContentRect.Height}");
            Assert.True(System.Math.Abs(second.ContentRect.Y - 40) < 2, $"Second row Y expected 40, got {second.ContentRect.Y}");
        }

        // [CSS-GRID §7.2] Three explicit row tracks 30px, 40px, 50px
        [Fact]
        public void ThreeRows_30_40_50()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;grid-template-rows:30px 40px 50px;width:100px'>
                    <div id='r1'></div>
                    <div id='r2'></div>
                    <div id='r3'></div>
                </div></body>");
            var row1 = LayoutTestHelper.FindById(root, "r1");
            var row2 = LayoutTestHelper.FindById(root, "r2");
            var row3 = LayoutTestHelper.FindById(root, "r3");
            Assert.NotNull(row1);
            Assert.NotNull(row2);
            Assert.NotNull(row3);
            Assert.True(System.Math.Abs(row1!.ContentRect.Height - 30) < 2, $"Row 1 expected 30px, got {row1.ContentRect.Height}");
            Assert.True(System.Math.Abs(row2!.ContentRect.Height - 40) < 2, $"Row 2 expected 40px, got {row2.ContentRect.Height}");
            Assert.True(System.Math.Abs(row3!.ContentRect.Height - 50) < 2, $"Row 3 expected 50px, got {row3.ContentRect.Height}");
            Assert.True(System.Math.Abs(row2.ContentRect.Y - 30) < 2, $"Row 2 Y expected 30, got {row2.ContentRect.Y}");
            Assert.True(System.Math.Abs(row3.ContentRect.Y - 70) < 2, $"Row 3 Y expected 70, got {row3.ContentRect.Y}");
        }

        // [CSS-GRID §7.2] Auto row sized from content height of 70px
        [Fact]
        public void AutoRow_ContentHeight_70px()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;width:100px'>
                    <div id='item' style='height:70px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item");
            Assert.NotNull(item);
            Assert.True(System.Math.Abs(item!.ContentRect.Height - 70) < 2, $"Auto row expected 70px from content, got {item.ContentRect.Height}");
        }

        // [CSS-GRID §7.2] Auto row track height determined by tallest item; auto-height items stretch
        [Fact]
        public void AutoRow_TallestItemWins()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:50px 50px 50px;grid-template-rows:auto 30px;width:150px'>
                    <div id='tall' style='height:80px'></div>
                    <div id='stretched'></div>
                    <div style='height:50px'></div>
                    <div id='secondRow'></div>
                </div></body>");
            var tallItem = LayoutTestHelper.FindById(root, "tall");
            var stretchedItem = LayoutTestHelper.FindById(root, "stretched");
            var secondRow = LayoutTestHelper.FindById(root, "secondRow");
            Assert.NotNull(tallItem);
            Assert.NotNull(stretchedItem);
            Assert.NotNull(secondRow);
            Assert.True(System.Math.Abs(tallItem!.ContentRect.Height - 80) < 2, $"Tallest item expected 80px, got {tallItem.ContentRect.Height}");
            // Auto-height item stretches to row track height
            Assert.True(System.Math.Abs(stretchedItem!.ContentRect.Height - 80) < 2, $"Stretched item expected 80px, got {stretchedItem.ContentRect.Height}");
            // Second row starts after the first 80px row
            Assert.True(System.Math.Abs(secondRow!.ContentRect.Y - 80) < 2, $"Second row Y expected 80, got {secondRow.ContentRect.Y}");
        }

        // [CSS-GRID §7.2] 1fr row fills container height of 200px
        [Fact]
        public void FrRow_FillsContainerHeight()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;grid-template-rows:1fr;width:100px;height:200px'>
                    <div id='item'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item");
            Assert.NotNull(item);
            Assert.True(System.Math.Abs(item!.ContentRect.Height - 200) < 2, $"1fr row expected 200px, got {item.ContentRect.Height}");
        }

        // [CSS-GRID §7.2] Two 1fr rows split 200px equally
        [Fact]
        public void TwoFrRows_SplitEvenly()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;grid-template-rows:1fr 1fr;width:100px;height:200px'>
                    <div id='top'></div>
                    <div id='bottom'></div>
                </div></body>");
            var top = LayoutTestHelper.FindById(root, "top");
            var bottom = LayoutTestHelper.FindById(root, "bottom");
            Assert.NotNull(top);
            Assert.NotNull(bottom);
            Assert.True(System.Math.Abs(top!.ContentRect.Height - 100) < 2, $"Top 1fr expected 100px, got {top.ContentRect.Height}");
            Assert.True(System.Math.Abs(bottom!.ContentRect.Height - 100) < 2, $"Bottom 1fr expected 100px, got {bottom.ContentRect.Height}");
            Assert.True(System.Math.Abs(bottom.ContentRect.Y - 100) < 2, $"Bottom Y expected 100, got {bottom.ContentRect.Y}");
        }

        // [CSS-GRID §7.2] 1fr and 2fr rows split 300px in 1:2 ratio
        [Fact]
        public void FrRows_1fr_2fr_SplitRatio()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;grid-template-rows:1fr 2fr;width:100px;height:300px'>
                    <div id='small'></div>
                    <div id='large'></div>
                </div></body>");
            var small = LayoutTestHelper.FindById(root, "small");
            var large = LayoutTestHelper.FindById(root, "large");
            Assert.NotNull(small);
            Assert.NotNull(large);
            Assert.True(System.Math.Abs(small!.ContentRect.Height - 100) < 2, $"1fr expected 100px, got {small.ContentRect.Height}");
            Assert.True(System.Math.Abs(large!.ContentRect.Height - 200) < 2, $"2fr expected 200px, got {large.ContentRect.Height}");
        }

        // [CSS-GRID §7.2] Fixed + fr row: 80px fixed + 1fr in 200px container
        [Fact]
        public void FixedPlusFrRow_Mix()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;grid-template-rows:80px 1fr;width:100px;height:200px'>
                    <div id='fixed'></div>
                    <div id='flex'></div>
                </div></body>");
            var fixedItem = LayoutTestHelper.FindById(root, "fixed");
            var flexItem = LayoutTestHelper.FindById(root, "flex");
            Assert.NotNull(fixedItem);
            Assert.NotNull(flexItem);
            Assert.True(System.Math.Abs(fixedItem!.ContentRect.Height - 80) < 2, $"Fixed row expected 80px, got {fixedItem.ContentRect.Height}");
            Assert.True(System.Math.Abs(flexItem!.ContentRect.Height - 120) < 2, $"Fr row expected 120px (200-80), got {flexItem.ContentRect.Height}");
            Assert.True(System.Math.Abs(flexItem.ContentRect.Y - 80) < 2, $"Fr row Y expected 80, got {flexItem.ContentRect.Y}");
        }

        // [CSS-GRID §7.5] grid-auto-rows: 50px sizes implicit rows
        [Fact]
        public void AutoRows_50px()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;grid-auto-rows:50px;width:100px'>
                    <div id='r1'></div>
                    <div id='r2'></div>
                    <div id='r3'></div>
                </div></body>");
            var row1 = LayoutTestHelper.FindById(root, "r1");
            var row2 = LayoutTestHelper.FindById(root, "r2");
            var row3 = LayoutTestHelper.FindById(root, "r3");
            Assert.NotNull(row1);
            Assert.NotNull(row2);
            Assert.NotNull(row3);
            Assert.True(System.Math.Abs(row1!.ContentRect.Height - 50) < 2, $"Row 1 expected 50px, got {row1.ContentRect.Height}");
            Assert.True(System.Math.Abs(row2!.ContentRect.Height - 50) < 2, $"Row 2 expected 50px, got {row2.ContentRect.Height}");
            Assert.True(System.Math.Abs(row3!.ContentRect.Height - 50) < 2, $"Row 3 expected 50px, got {row3.ContentRect.Height}");
            Assert.True(System.Math.Abs(row2.ContentRect.Y - 50) < 2, $"Row 2 Y expected 50, got {row2.ContentRect.Y}");
            Assert.True(System.Math.Abs(row3.ContentRect.Y - 100) < 2, $"Row 3 Y expected 100, got {row3.ContentRect.Y}");
        }

        // [CSS-GRID §7.3] repeat(3, 40px) creates three 40px row tracks
        [Fact]
        public void Repeat_3_40px()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;grid-template-rows:repeat(3,40px);width:100px'>
                    <div id='r1'></div>
                    <div id='r2'></div>
                    <div id='r3'></div>
                </div></body>");
            var row1 = LayoutTestHelper.FindById(root, "r1");
            var row2 = LayoutTestHelper.FindById(root, "r2");
            var row3 = LayoutTestHelper.FindById(root, "r3");
            Assert.NotNull(row1);
            Assert.NotNull(row2);
            Assert.NotNull(row3);
            Assert.True(System.Math.Abs(row1!.ContentRect.Height - 40) < 2, $"Row 1 expected 40px, got {row1.ContentRect.Height}");
            Assert.True(System.Math.Abs(row2!.ContentRect.Height - 40) < 2, $"Row 2 expected 40px, got {row2.ContentRect.Height}");
            Assert.True(System.Math.Abs(row3!.ContentRect.Height - 40) < 2, $"Row 3 expected 40px, got {row3.ContentRect.Height}");
            Assert.True(System.Math.Abs(row3.ContentRect.Y - 80) < 2, $"Row 3 Y expected 80, got {row3.ContentRect.Y}");
        }

        // [CSS-GRID §7.4] minmax(30px, 100px) with mid-range content uses content height
        [Fact]
        public void Minmax_30_100_MidContent_UsesContent()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;grid-template-rows:minmax(30px,100px);width:100px'>
                    <div id='item' style='height:60px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item");
            Assert.NotNull(item);
            Assert.True(System.Math.Abs(item!.ContentRect.Height - 60) < 2, $"Minmax mid content expected 60px, got {item.ContentRect.Height}");
        }

        // [CSS-GRID §7.4] minmax(min-content, 1fr) in fixed-height container
        [Fact]
        public void Minmax_MinContent_1fr_InFixedContainer()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;grid-template-rows:minmax(20px,1fr);width:100px;height:150px'>
                    <div id='item'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item");
            Assert.NotNull(item);
            Assert.True(System.Math.Abs(item!.ContentRect.Height - 150) < 2, $"minmax(20px,1fr) in 150px expected 150px, got {item.ContentRect.Height}");
        }

        // [CSS-GRID §7.2] Explicit 200px row track with smaller content item
        [Fact]
        public void ExplicitRow_200px_SmallerContent()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;grid-template-rows:200px;width:100px'>
                    <div id='item' style='height:50px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item");
            Assert.NotNull(item);
            // Item has explicit height 50px, doesn't stretch (has explicit height)
            Assert.True(System.Math.Abs(item!.ContentRect.Height - 50) < 2, $"Item with explicit height expected 50px, got {item.ContentRect.Height}");
        }

        // [CSS-GRID §7.2] Percentage row: 50% of 200px container = 100px
        [Fact]
        public void PercentageRow_50Percent_Of200()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;grid-template-rows:50%;width:100px;height:200px'>
                    <div id='item'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item");
            Assert.NotNull(item);
            Assert.True(System.Math.Abs(item!.ContentRect.Height - 100) < 2, $"50% of 200px expected 100px, got {item.ContentRect.Height}");
        }

        // [CSS-GRID §10.1] Row gap separates row tracks
        [Fact]
        public void RowGap_SeparatesRows()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;grid-template-rows:40px 40px;row-gap:20px;width:100px'>
                    <div id='top'></div>
                    <div id='bottom'></div>
                </div></body>");
            var top = LayoutTestHelper.FindById(root, "top");
            var bottom = LayoutTestHelper.FindById(root, "bottom");
            Assert.NotNull(top);
            Assert.NotNull(bottom);
            Assert.True(System.Math.Abs(top!.ContentRect.Height - 40) < 2, $"Top row expected 40px, got {top.ContentRect.Height}");
            Assert.True(System.Math.Abs(bottom!.ContentRect.Y - 60) < 2, $"Bottom row Y expected 60 (40+20gap), got {bottom.ContentRect.Y}");
            Assert.True(System.Math.Abs(bottom.ContentRect.Height - 40) < 2, $"Bottom row expected 40px, got {bottom.ContentRect.Height}");
        }

        // [CSS-GRID §7.2] Container auto height derived from row track sum
        [Fact]
        public void ContainerAutoHeight_FromRows()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='grid' style='display:grid;grid-template-columns:100px;grid-template-rows:30px 50px 20px;width:100px'>
                    <div></div>
                    <div></div>
                    <div></div>
                </div></body>");
            var grid = LayoutTestHelper.FindById(root, "grid");
            Assert.NotNull(grid);
            Assert.True(System.Math.Abs(grid!.ContentRect.Height - 100) < 2, $"Container height expected 100px (30+50+20), got {grid.ContentRect.Height}");
        }

        // [CSS-GRID §7.2 + §10.1] Container auto height from rows plus gap
        [Fact]
        public void ContainerAutoHeight_FromRowsPlusGap()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='grid' style='display:grid;grid-template-columns:100px;grid-template-rows:40px 40px 40px;row-gap:10px;width:100px'>
                    <div></div>
                    <div></div>
                    <div></div>
                </div></body>");
            var grid = LayoutTestHelper.FindById(root, "grid");
            Assert.NotNull(grid);
            // 3 rows * 40px + 2 gaps * 10px = 120 + 20 = 140
            Assert.True(System.Math.Abs(grid!.ContentRect.Height - 140) < 2, $"Container height expected 140px (3*40+2*10), got {grid.ContentRect.Height}");
        }

        // [CSS-GRID §7.2] Three 1fr rows split container height equally
        [Fact]
        public void ThreeFrRows_SplitEvenly()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;grid-template-rows:1fr 1fr 1fr;width:100px;height:300px'>
                    <div id='r1'></div>
                    <div id='r2'></div>
                    <div id='r3'></div>
                </div></body>");
            var row1 = LayoutTestHelper.FindById(root, "r1");
            var row2 = LayoutTestHelper.FindById(root, "r2");
            var row3 = LayoutTestHelper.FindById(root, "r3");
            Assert.NotNull(row1);
            Assert.NotNull(row2);
            Assert.NotNull(row3);
            Assert.True(System.Math.Abs(row1!.ContentRect.Height - 100) < 2, $"Row 1 expected 100px, got {row1.ContentRect.Height}");
            Assert.True(System.Math.Abs(row2!.ContentRect.Height - 100) < 2, $"Row 2 expected 100px, got {row2.ContentRect.Height}");
            Assert.True(System.Math.Abs(row3!.ContentRect.Height - 100) < 2, $"Row 3 expected 100px, got {row3.ContentRect.Height}");
        }

        // [CSS-GRID §7.2] Four equal fixed rows
        [Fact]
        public void FourEqualRows_25px()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;grid-template-rows:25px 25px 25px 25px;width:100px'>
                    <div id='r1'></div>
                    <div id='r2'></div>
                    <div id='r3'></div>
                    <div id='r4'></div>
                </div></body>");
            var row1 = LayoutTestHelper.FindById(root, "r1");
            var row2 = LayoutTestHelper.FindById(root, "r2");
            var row3 = LayoutTestHelper.FindById(root, "r3");
            var row4 = LayoutTestHelper.FindById(root, "r4");
            Assert.NotNull(row1);
            Assert.NotNull(row2);
            Assert.NotNull(row3);
            Assert.NotNull(row4);
            Assert.True(System.Math.Abs(row1!.ContentRect.Height - 25) < 2, $"Row 1 expected 25px, got {row1.ContentRect.Height}");
            Assert.True(System.Math.Abs(row2!.ContentRect.Y - 25) < 2, $"Row 2 Y expected 25, got {row2.ContentRect.Y}");
            Assert.True(System.Math.Abs(row3!.ContentRect.Y - 50) < 2, $"Row 3 Y expected 50, got {row3.ContentRect.Y}");
            Assert.True(System.Math.Abs(row4!.ContentRect.Y - 75) < 2, $"Row 4 Y expected 75, got {row4.ContentRect.Y}");
        }

        // [CSS-GRID §7.2] 1fr row with gap subtracts gap before distributing
        [Fact]
        public void FrRows_WithGap_SubtractsGap()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;grid-template-rows:1fr 1fr;row-gap:20px;width:100px;height:220px'>
                    <div id='top'></div>
                    <div id='bottom'></div>
                </div></body>");
            var top = LayoutTestHelper.FindById(root, "top");
            var bottom = LayoutTestHelper.FindById(root, "bottom");
            Assert.NotNull(top);
            Assert.NotNull(bottom);
            // 220px - 20px gap = 200px / 2 = 100px each
            Assert.True(System.Math.Abs(top!.ContentRect.Height - 100) < 2, $"Top 1fr expected 100px (220-20gap)/2, got {top.ContentRect.Height}");
            Assert.True(System.Math.Abs(bottom!.ContentRect.Height - 100) < 2, $"Bottom 1fr expected 100px, got {bottom.ContentRect.Height}");
            Assert.True(System.Math.Abs(bottom.ContentRect.Y - 120) < 2, $"Bottom Y expected 120 (100+20gap), got {bottom.ContentRect.Y}");
        }

        // [CSS-GRID §7.2] Two fixed + fr row combination: 50px + 50px + 1fr in 200px
        [Fact]
        public void FixedFixedFr_RowCombination()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;grid-template-rows:50px 50px 1fr;width:100px;height:200px'>
                    <div id='fixed1'></div>
                    <div id='fixed2'></div>
                    <div id='frRow'></div>
                </div></body>");
            var fixed1 = LayoutTestHelper.FindById(root, "fixed1");
            var fixed2 = LayoutTestHelper.FindById(root, "fixed2");
            var frItem = LayoutTestHelper.FindById(root, "frRow");
            Assert.NotNull(fixed1);
            Assert.NotNull(fixed2);
            Assert.NotNull(frItem);
            Assert.True(System.Math.Abs(fixed1!.ContentRect.Height - 50) < 2, $"Fixed row 1 expected 50px, got {fixed1.ContentRect.Height}");
            Assert.True(System.Math.Abs(fixed2!.ContentRect.Height - 50) < 2, $"Fixed row 2 expected 50px, got {fixed2.ContentRect.Height}");
            // fr = 200 - 50 - 50 = 100
            Assert.True(System.Math.Abs(frItem!.ContentRect.Height - 100) < 2, $"Fr row expected 100px (200-50-50), got {frItem.ContentRect.Height}");
            Assert.True(System.Math.Abs(frItem.ContentRect.Y - 100) < 2, $"Fr row Y expected 100, got {frItem.ContentRect.Y}");
        }

        // [CSS-GRID §7.3] repeat(4, 25px) creates four 25px rows
        [Fact]
        public void Repeat_4_25px()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;grid-template-rows:repeat(4,25px);width:100px'>
                    <div id='r1'></div>
                    <div id='r2'></div>
                    <div id='r3'></div>
                    <div id='r4'></div>
                </div></body>");
            var row1 = LayoutTestHelper.FindById(root, "r1");
            var row4 = LayoutTestHelper.FindById(root, "r4");
            Assert.NotNull(row1);
            Assert.NotNull(row4);
            Assert.True(System.Math.Abs(row1!.ContentRect.Height - 25) < 2, $"Row 1 expected 25px, got {row1.ContentRect.Height}");
            Assert.True(System.Math.Abs(row4!.ContentRect.Y - 75) < 2, $"Row 4 Y expected 75 (3*25), got {row4.ContentRect.Y}");
            Assert.True(System.Math.Abs(row4.ContentRect.Height - 25) < 2, $"Row 4 expected 25px, got {row4.ContentRect.Height}");
        }

        // [CSS-GRID §7.2] Two percentage rows: 30% + 70% of 200px
        [Fact]
        public void TwoPercentageRows_30_70()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;grid-template-rows:30% 70%;width:100px;height:200px'>
                    <div id='top'></div>
                    <div id='bottom'></div>
                </div></body>");
            var top = LayoutTestHelper.FindById(root, "top");
            var bottom = LayoutTestHelper.FindById(root, "bottom");
            Assert.NotNull(top);
            Assert.NotNull(bottom);
            Assert.True(System.Math.Abs(top!.ContentRect.Height - 60) < 2, $"30% of 200px expected 60px, got {top.ContentRect.Height}");
            Assert.True(System.Math.Abs(bottom!.ContentRect.Height - 140) < 2, $"70% of 200px expected 140px, got {bottom.ContentRect.Height}");
        }

        // [CSS-GRID §7.5] Auto rows stretch items without explicit height to track size
        [Fact]
        public void AutoRows_StretchesAutoHeightItems()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;grid-auto-rows:60px;width:100px'>
                    <div id='r1'></div>
                    <div id='r2'></div>
                </div></body>");
            var row1 = LayoutTestHelper.FindById(root, "r1");
            var row2 = LayoutTestHelper.FindById(root, "r2");
            Assert.NotNull(row1);
            Assert.NotNull(row2);
            Assert.True(System.Math.Abs(row1!.ContentRect.Height - 60) < 2, $"Auto row 1 expected 60px, got {row1.ContentRect.Height}");
            Assert.True(System.Math.Abs(row2!.ContentRect.Height - 60) < 2, $"Auto row 2 expected 60px, got {row2.ContentRect.Height}");
            Assert.True(System.Math.Abs(row2.ContentRect.Y - 60) < 2, $"Row 2 Y expected 60, got {row2.ContentRect.Y}");
        }

        // [CSS-GRID §7.2] Row spanning item crosses two rows
        [Fact]
        public void RowSpan_AcrossTwoRows()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px 100px;grid-template-rows:50px 50px;width:200px'>
                    <div id='spanning' style='grid-row:1/3'></div>
                    <div id='topRight'></div>
                    <div id='bottomRight'></div>
                </div></body>");
            var spanning = LayoutTestHelper.FindById(root, "spanning");
            Assert.NotNull(spanning);
            Assert.True(System.Math.Abs(spanning!.ContentRect.Height - 100) < 2, $"Spanning item expected 100px (2*50), got {spanning.ContentRect.Height}");
        }

        // [CSS-GRID §7.2 + §10.1] Row spanning with gap includes gap
        [Fact]
        public void RowSpan_WithGap_IncludesGap()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px 100px;grid-template-rows:40px 40px;row-gap:20px;width:200px'>
                    <div id='spanning' style='grid-row:1/3'></div>
                    <div id='topRight'></div>
                    <div id='bottomRight'></div>
                </div></body>");
            var spanning = LayoutTestHelper.FindById(root, "spanning");
            Assert.NotNull(spanning);
            // 40 + 20gap + 40 = 100
            Assert.True(System.Math.Abs(spanning!.ContentRect.Height - 100) < 2, $"Spanning item expected 100px (40+20gap+40), got {spanning.ContentRect.Height}");
        }

        // [CSS-GRID §7.2] Mixed fr rows: 1fr 3fr in 200px
        [Fact]
        public void FrRows_1fr_3fr_Ratio()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;grid-template-rows:1fr 3fr;width:100px;height:200px'>
                    <div id='small'></div>
                    <div id='large'></div>
                </div></body>");
            var small = LayoutTestHelper.FindById(root, "small");
            var large = LayoutTestHelper.FindById(root, "large");
            Assert.NotNull(small);
            Assert.NotNull(large);
            Assert.True(System.Math.Abs(small!.ContentRect.Height - 50) < 2, $"1fr expected 50px (200/4), got {small.ContentRect.Height}");
            Assert.True(System.Math.Abs(large!.ContentRect.Height - 150) < 2, $"3fr expected 150px (200*3/4), got {large.ContentRect.Height}");
        }
    }
}
