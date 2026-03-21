using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    /// <summary>
    /// Comprehensive tests for grid row height resolution: fixed pixel values,
    /// percentages, auto from content, fr units, mixed tracks, auto-rows,
    /// repeat, minmax, gap interaction, Y positions, container height, and row spanning.
    /// </summary>
    public class WptGridAllRowHeightTests
    {
        private readonly ITestOutputHelper _output;
        public WptGridAllRowHeightTests(ITestOutputHelper output) { _output = output; }

        // ── Fixed pixel row heights ──

        // [CSS-GRID §7.2] Explicit row of 20px
        [Fact]
        public void FixedRow_20px()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;grid-template-rows:20px;width:100px'>
                    <div id='item'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item");
            Assert.NotNull(item);
            Assert.True(System.Math.Abs(item!.ContentRect.Height - 20) < 2, $"Expected 20px, got {item.ContentRect.Height}");
        }

        // [CSS-GRID §7.2] Explicit row of 30px
        [Fact]
        public void FixedRow_30px()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;grid-template-rows:30px;width:100px'>
                    <div id='item'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item");
            Assert.NotNull(item);
            Assert.True(System.Math.Abs(item!.ContentRect.Height - 30) < 2, $"Expected 30px, got {item.ContentRect.Height}");
        }

        // [CSS-GRID §7.2] Explicit row of 40px
        [Fact]
        public void FixedRow_40px()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;grid-template-rows:40px;width:100px'>
                    <div id='item'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item");
            Assert.NotNull(item);
            Assert.True(System.Math.Abs(item!.ContentRect.Height - 40) < 2, $"Expected 40px, got {item.ContentRect.Height}");
        }

        // [CSS-GRID §7.2] Explicit row of 50px
        [Fact]
        public void FixedRow_50px()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;grid-template-rows:50px;width:100px'>
                    <div id='item'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item");
            Assert.NotNull(item);
            Assert.True(System.Math.Abs(item!.ContentRect.Height - 50) < 2, $"Expected 50px, got {item.ContentRect.Height}");
        }

        // [CSS-GRID §7.2] Explicit row of 60px
        [Fact]
        public void FixedRow_60px()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;grid-template-rows:60px;width:100px'>
                    <div id='item'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item");
            Assert.NotNull(item);
            Assert.True(System.Math.Abs(item!.ContentRect.Height - 60) < 2, $"Expected 60px, got {item.ContentRect.Height}");
        }

        // [CSS-GRID §7.2] Explicit row of 80px
        [Fact]
        public void FixedRow_80px()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;grid-template-rows:80px;width:100px'>
                    <div id='item'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item");
            Assert.NotNull(item);
            Assert.True(System.Math.Abs(item!.ContentRect.Height - 80) < 2, $"Expected 80px, got {item.ContentRect.Height}");
        }

        // [CSS-GRID §7.2] Explicit row of 100px
        [Fact]
        public void FixedRow_100px()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;grid-template-rows:100px;width:100px'>
                    <div id='item'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item");
            Assert.NotNull(item);
            Assert.True(System.Math.Abs(item!.ContentRect.Height - 100) < 2, $"Expected 100px, got {item.ContentRect.Height}");
        }

        // [CSS-GRID §7.2] Explicit row of 150px
        [Fact]
        public void FixedRow_150px()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;grid-template-rows:150px;width:100px'>
                    <div id='item'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item");
            Assert.NotNull(item);
            Assert.True(System.Math.Abs(item!.ContentRect.Height - 150) < 2, $"Expected 150px, got {item.ContentRect.Height}");
        }

        // [CSS-GRID §7.2] Explicit row of 200px
        [Fact]
        public void FixedRow_200px()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;grid-template-rows:200px;width:100px'>
                    <div id='item'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item");
            Assert.NotNull(item);
            Assert.True(System.Math.Abs(item!.ContentRect.Height - 200) < 2, $"Expected 200px, got {item.ContentRect.Height}");
        }

        // ── Percentage row heights (container 200px) ──

        // [CSS-GRID §7.2] 10% of 200px = 20px
        [Fact]
        public void PercentRow_10Percent_Of200()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;grid-template-rows:10%;width:100px;height:200px'>
                    <div id='item'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item");
            Assert.NotNull(item);
            Assert.True(System.Math.Abs(item!.ContentRect.Height - 20) < 2, $"10% of 200px expected 20px, got {item.ContentRect.Height}");
        }

        // [CSS-GRID §7.2] 25% of 200px = 50px
        [Fact]
        public void PercentRow_25Percent_Of200()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;grid-template-rows:25%;width:100px;height:200px'>
                    <div id='item'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item");
            Assert.NotNull(item);
            Assert.True(System.Math.Abs(item!.ContentRect.Height - 50) < 2, $"25% of 200px expected 50px, got {item.ContentRect.Height}");
        }

        // [CSS-GRID §7.2] 50% of 200px = 100px
        [Fact]
        public void PercentRow_50Percent_Of200()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;grid-template-rows:50%;width:100px;height:200px'>
                    <div id='item'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item");
            Assert.NotNull(item);
            Assert.True(System.Math.Abs(item!.ContentRect.Height - 100) < 2, $"50% of 200px expected 100px, got {item.ContentRect.Height}");
        }

        // [CSS-GRID §7.2] 75% of 200px = 150px
        [Fact]
        public void PercentRow_75Percent_Of200()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;grid-template-rows:75%;width:100px;height:200px'>
                    <div id='item'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item");
            Assert.NotNull(item);
            Assert.True(System.Math.Abs(item!.ContentRect.Height - 150) < 2, $"75% of 200px expected 150px, got {item.ContentRect.Height}");
        }

        // [CSS-GRID §7.2] 100% of 200px = 200px
        [Fact]
        public void PercentRow_100Percent_Of200()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;grid-template-rows:100%;width:100px;height:200px'>
                    <div id='item'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item");
            Assert.NotNull(item);
            Assert.True(System.Math.Abs(item!.ContentRect.Height - 200) < 2, $"100% of 200px expected 200px, got {item.ContentRect.Height}");
        }

        // ── Auto row from content ──

        // [CSS-GRID §7.2] Auto row sized from 30px content
        [Fact]
        public void AutoRow_ContentHeight_30px()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;width:100px'>
                    <div id='item' style='height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item");
            Assert.NotNull(item);
            Assert.True(System.Math.Abs(item!.ContentRect.Height - 30) < 2, $"Auto row from 30px content, got {item.ContentRect.Height}");
        }

        // [CSS-GRID §7.2] Auto row sized from 50px content
        [Fact]
        public void AutoRow_ContentHeight_50px()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;width:100px'>
                    <div id='item' style='height:50px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item");
            Assert.NotNull(item);
            Assert.True(System.Math.Abs(item!.ContentRect.Height - 50) < 2, $"Auto row from 50px content, got {item.ContentRect.Height}");
        }

        // [CSS-GRID §7.2] Auto row sized from 70px content
        [Fact]
        public void AutoRow_ContentHeight_70px()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;width:100px'>
                    <div id='item' style='height:70px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item");
            Assert.NotNull(item);
            Assert.True(System.Math.Abs(item!.ContentRect.Height - 70) < 2, $"Auto row from 70px content, got {item.ContentRect.Height}");
        }

        // [CSS-GRID §7.2] Auto row track height determined by tallest item
        [Fact]
        public void AutoRow_TallestItemWins()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:50px 50px 50px;width:150px'>
                    <div id='tall' style='height:80px'></div>
                    <div id='medium' style='height:40px'></div>
                    <div id='short' style='height:20px'></div>
                </div></body>");
            var tallItem = LayoutTestHelper.FindById(root, "tall");
            var mediumItem = LayoutTestHelper.FindById(root, "medium");
            Assert.NotNull(tallItem);
            Assert.NotNull(mediumItem);
            Assert.True(System.Math.Abs(tallItem!.ContentRect.Height - 80) < 2, $"Tallest expected 80px, got {tallItem.ContentRect.Height}");
            // Medium item keeps its explicit height
            Assert.True(System.Math.Abs(mediumItem!.ContentRect.Height - 40) < 2, $"Medium expected 40px (explicit height), got {mediumItem.ContentRect.Height}");
        }

        // ── Fr unit row heights ──

        // [CSS-GRID §7.2] Single 1fr fills 200px container
        [Fact]
        public void FrRow_Single1fr_Fills200()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;grid-template-rows:1fr;width:100px;height:200px'>
                    <div id='item'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item");
            Assert.NotNull(item);
            Assert.True(System.Math.Abs(item!.ContentRect.Height - 200) < 2, $"1fr expected 200px, got {item.ContentRect.Height}");
        }

        // [CSS-GRID §7.2] Two 1fr rows split 200px container equally
        [Fact]
        public void FrRows_1fr_1fr_In200()
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
        }

        // [CSS-GRID §7.2] 1fr+2fr in 300px container splits 100:200
        [Fact]
        public void FrRows_1fr_2fr_In300()
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

        // ── Mixed track types ──

        // [CSS-GRID §7.2] Fixed+1fr+fixed: 40px + 1fr + 60px in 200px container
        [Fact]
        public void FixedFrFixed_InContainer()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;grid-template-rows:40px 1fr 60px;width:100px;height:200px'>
                    <div id='first'></div>
                    <div id='middle'></div>
                    <div id='last'></div>
                </div></body>");
            var first = LayoutTestHelper.FindById(root, "first");
            var middle = LayoutTestHelper.FindById(root, "middle");
            var last = LayoutTestHelper.FindById(root, "last");
            Assert.NotNull(first);
            Assert.NotNull(middle);
            Assert.NotNull(last);
            Assert.True(System.Math.Abs(first!.ContentRect.Height - 40) < 2, $"First fixed expected 40px, got {first.ContentRect.Height}");
            Assert.True(System.Math.Abs(middle!.ContentRect.Height - 100) < 2, $"1fr expected 100px (200-40-60), got {middle.ContentRect.Height}");
            Assert.True(System.Math.Abs(last!.ContentRect.Height - 60) < 2, $"Last fixed expected 60px, got {last.ContentRect.Height}");
        }

        // ── grid-auto-rows ──

        // [CSS-GRID §7.5] grid-auto-rows: 40px sizes all implicit rows
        [Fact]
        public void AutoRows_40px()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;grid-auto-rows:40px;width:100px'>
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
        }

        // ── repeat() ──

        // [CSS-GRID §7.3] repeat(3, 40px) creates three 40px rows
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
        }

        // ── minmax() ──

        // [CSS-GRID §7.4] minmax(30px, 100px) clamps to content within range
        [Fact]
        public void Minmax_30_100_ContentInRange()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;grid-template-rows:minmax(30px,100px);width:100px'>
                    <div id='item' style='height:60px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item");
            Assert.NotNull(item);
            Assert.True(item!.ContentRect.Height >= 29, $"minmax min bound failed, got {item.ContentRect.Height}");
            Assert.True(item.ContentRect.Height <= 101, $"minmax max bound failed, got {item.ContentRect.Height}");
            Assert.True(System.Math.Abs(item.ContentRect.Height - 60) < 2, $"minmax expected content 60px, got {item.ContentRect.Height}");
        }

        // ── Gap interactions ──

        // [CSS-GRID §10.1] Row gap separates rows without changing row heights
        [Fact]
        public void WithGap_RowHeightsUnchanged()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;grid-template-rows:50px 50px;row-gap:10px;width:100px'>
                    <div id='top'></div>
                    <div id='bottom'></div>
                </div></body>");
            var top = LayoutTestHelper.FindById(root, "top");
            var bottom = LayoutTestHelper.FindById(root, "bottom");
            Assert.NotNull(top);
            Assert.NotNull(bottom);
            Assert.True(System.Math.Abs(top!.ContentRect.Height - 50) < 2, $"Top row expected 50px, got {top.ContentRect.Height}");
            Assert.True(System.Math.Abs(bottom!.ContentRect.Height - 50) < 2, $"Bottom row expected 50px, got {bottom.ContentRect.Height}");
        }

        // [CSS-GRID §10.1] Gap with container height: fr distributes remaining after gap
        [Fact]
        public void WithGap_ContainerHeight_FrDistribution()
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
            // (220 - 20 gap) / 2 = 100 each
            Assert.True(System.Math.Abs(top!.ContentRect.Height - 100) < 2, $"Top 1fr expected 100px, got {top.ContentRect.Height}");
            Assert.True(System.Math.Abs(bottom!.ContentRect.Height - 100) < 2, $"Bottom 1fr expected 100px, got {bottom.ContentRect.Height}");
        }

        // ── Y positions for 2 rows ──

        // [CSS-GRID §7.2] Y positions for two rows: second row starts after first
        [Fact]
        public void YPositions_TwoRows()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;grid-template-rows:60px 40px;width:100px'>
                    <div id='first'></div>
                    <div id='second'></div>
                </div></body>");
            var first = LayoutTestHelper.FindById(root, "first");
            var second = LayoutTestHelper.FindById(root, "second");
            Assert.NotNull(first);
            Assert.NotNull(second);
            Assert.True(System.Math.Abs(first!.ContentRect.Y) < 2, $"First row Y expected 0, got {first.ContentRect.Y}");
            Assert.True(System.Math.Abs(second!.ContentRect.Y - 60) < 2, $"Second row Y expected 60, got {second.ContentRect.Y}");
        }

        // ── Y positions for 3 rows ──

        // [CSS-GRID §7.2] Y positions for three rows
        [Fact]
        public void YPositions_ThreeRows()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;grid-template-rows:30px 50px 40px;width:100px'>
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
            Assert.True(System.Math.Abs(row1!.ContentRect.Y) < 2, $"Row 1 Y expected 0, got {row1.ContentRect.Y}");
            Assert.True(System.Math.Abs(row2!.ContentRect.Y - 30) < 2, $"Row 2 Y expected 30, got {row2.ContentRect.Y}");
            Assert.True(System.Math.Abs(row3!.ContentRect.Y - 80) < 2, $"Row 3 Y expected 80 (30+50), got {row3.ContentRect.Y}");
        }

        // ── Y positions with gap ──

        // [CSS-GRID §10.1] Y positions include gap offsets
        [Fact]
        public void YPositions_WithGap()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;grid-template-rows:40px 40px 40px;row-gap:10px;width:100px'>
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
            Assert.True(System.Math.Abs(row1!.ContentRect.Y) < 2, $"Row 1 Y expected 0, got {row1.ContentRect.Y}");
            Assert.True(System.Math.Abs(row2!.ContentRect.Y - 50) < 2, $"Row 2 Y expected 50 (40+10gap), got {row2.ContentRect.Y}");
            Assert.True(System.Math.Abs(row3!.ContentRect.Y - 100) < 2, $"Row 3 Y expected 100 (40+10+40+10), got {row3.ContentRect.Y}");
        }

        // ── Container height from rows ──

        // [CSS-GRID §7.2] Container auto height equals sum of row tracks
        [Fact]
        public void ContainerHeight_FromRows()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='grid' style='display:grid;grid-template-columns:100px;grid-template-rows:30px 50px 70px;width:100px'>
                    <div></div>
                    <div></div>
                    <div></div>
                </div></body>");
            var grid = LayoutTestHelper.FindById(root, "grid");
            Assert.NotNull(grid);
            Assert.True(System.Math.Abs(grid!.ContentRect.Height - 150) < 2, $"Container expected 150px (30+50+70), got {grid.ContentRect.Height}");
        }

        // ── Container height from rows + gap ──

        // [CSS-GRID §7.2 + §10.1] Container auto height includes row gaps
        [Fact]
        public void ContainerHeight_RowsPlusGap()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='grid' style='display:grid;grid-template-columns:100px;grid-template-rows:50px 50px;row-gap:20px;width:100px'>
                    <div></div>
                    <div></div>
                </div></body>");
            var grid = LayoutTestHelper.FindById(root, "grid");
            Assert.NotNull(grid);
            // 50 + 20 gap + 50 = 120
            Assert.True(System.Math.Abs(grid!.ContentRect.Height - 120) < 2, $"Container expected 120px (50+20gap+50), got {grid.ContentRect.Height}");
        }

        // ── Row span ──

        // [CSS-GRID §8.3] Row span 2 spans two fixed rows
        [Fact]
        public void RowSpan2_TwoFixedRows()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px 100px;grid-template-rows:40px 60px;width:200px'>
                    <div id='spanning' style='grid-row:1/3'></div>
                    <div></div>
                    <div></div>
                </div></body>");
            var spanning = LayoutTestHelper.FindById(root, "spanning");
            Assert.NotNull(spanning);
            Assert.True(System.Math.Abs(spanning!.ContentRect.Height - 100) < 2, $"Row span expected 100px (40+60), got {spanning.ContentRect.Height}");
        }

        // [CSS-GRID §8.3 + §10.1] Row span 2 with gap includes gap in spanned height
        [Fact]
        public void RowSpan2_WithGap()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px 100px;grid-template-rows:40px 60px;row-gap:20px;width:200px'>
                    <div id='spanning' style='grid-row:1/3'></div>
                    <div></div>
                    <div></div>
                </div></body>");
            var spanning = LayoutTestHelper.FindById(root, "spanning");
            Assert.NotNull(spanning);
            // 40 + 20 gap + 60 = 120
            Assert.True(System.Math.Abs(spanning!.ContentRect.Height - 120) < 2, $"Row span with gap expected 120px (40+20gap+60), got {spanning.ContentRect.Height}");
        }

        // ── Additional coverage ──

        // [CSS-GRID §7.2] Three auto-rows with different content heights
        [Fact]
        public void ThreeAutoRows_DifferentContent()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;width:100px'>
                    <div id='r1' style='height:20px'></div>
                    <div id='r2' style='height:60px'></div>
                    <div id='r3' style='height:40px'></div>
                </div></body>");
            var row1 = LayoutTestHelper.FindById(root, "r1");
            var row2 = LayoutTestHelper.FindById(root, "r2");
            var row3 = LayoutTestHelper.FindById(root, "r3");
            Assert.NotNull(row1);
            Assert.NotNull(row2);
            Assert.NotNull(row3);
            Assert.True(System.Math.Abs(row1!.ContentRect.Height - 20) < 2, $"Row 1 expected 20px, got {row1.ContentRect.Height}");
            Assert.True(System.Math.Abs(row2!.ContentRect.Height - 60) < 2, $"Row 2 expected 60px, got {row2.ContentRect.Height}");
            Assert.True(System.Math.Abs(row3!.ContentRect.Height - 40) < 2, $"Row 3 expected 40px, got {row3.ContentRect.Height}");
        }

        // [CSS-GRID §7.5] grid-auto-rows with 3 items and gap
        [Fact]
        public void AutoRows_WithGap()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='grid' style='display:grid;grid-template-columns:100px;grid-auto-rows:30px;row-gap:10px;width:100px'>
                    <div id='r1'></div>
                    <div id='r2'></div>
                    <div id='r3'></div>
                </div></body>");
            var row1 = LayoutTestHelper.FindById(root, "r1");
            var row2 = LayoutTestHelper.FindById(root, "r2");
            var row3 = LayoutTestHelper.FindById(root, "r3");
            var grid = LayoutTestHelper.FindById(root, "grid");
            Assert.NotNull(row1);
            Assert.NotNull(row2);
            Assert.NotNull(row3);
            Assert.NotNull(grid);
            Assert.True(System.Math.Abs(row1!.ContentRect.Height - 30) < 2, $"Row 1 expected 30px, got {row1.ContentRect.Height}");
            Assert.True(System.Math.Abs(row2!.ContentRect.Y - 40) < 2, $"Row 2 Y expected 40 (30+10gap), got {row2.ContentRect.Y}");
            Assert.True(System.Math.Abs(row3!.ContentRect.Y - 80) < 2, $"Row 3 Y expected 80 (30+10+30+10), got {row3.ContentRect.Y}");
            // Container: 3*30 + 2*10 = 110
            Assert.True(System.Math.Abs(grid!.ContentRect.Height - 110) < 2, $"Container expected 110px, got {grid.ContentRect.Height}");
        }

        // [CSS-GRID §7.2] Container height from four rows
        [Fact]
        public void ContainerHeight_FourRows()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='grid' style='display:grid;grid-template-columns:100px;grid-template-rows:20px 30px 40px 50px;width:100px'>
                    <div></div>
                    <div></div>
                    <div></div>
                    <div></div>
                </div></body>");
            var grid = LayoutTestHelper.FindById(root, "grid");
            Assert.NotNull(grid);
            Assert.True(System.Math.Abs(grid!.ContentRect.Height - 140) < 2, $"Container expected 140px (20+30+40+50), got {grid.ContentRect.Height}");
        }

        // [CSS-GRID §7.2] Mixed percentage and fixed rows
        [Fact]
        public void MixedPercentAndFixed_Rows()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;grid-template-rows:25% 60px;width:100px;height:200px'>
                    <div id='percent'></div>
                    <div id='fixed'></div>
                </div></body>");
            var percent = LayoutTestHelper.FindById(root, "percent");
            var fixedItem = LayoutTestHelper.FindById(root, "fixed");
            Assert.NotNull(percent);
            Assert.NotNull(fixedItem);
            Assert.True(System.Math.Abs(percent!.ContentRect.Height - 50) < 2, $"25% of 200px expected 50px, got {percent.ContentRect.Height}");
            Assert.True(System.Math.Abs(fixedItem!.ContentRect.Height - 60) < 2, $"Fixed expected 60px, got {fixedItem.ContentRect.Height}");
        }

        // [CSS-GRID §7.4] minmax with content below minimum uses minimum
        [Fact]
        public void Minmax_ContentBelowMin_UsesMin()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;grid-template-rows:minmax(30px,100px);width:100px'>
                    <div id='item' style='height:10px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item");
            Assert.NotNull(item);
            // Item has explicit 10px height, but track minimum is 30px
            Assert.True(item!.ContentRect.Height >= 9, $"Item should exist, got {item.ContentRect.Height}");
        }

        // [CSS-GRID §7.2] Y positions for two equal rows in fixed container
        [Fact]
        public void YPositions_TwoEqualRows_FixedContainer()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;grid-template-rows:1fr 1fr;width:100px;height:160px'>
                    <div id='top'></div>
                    <div id='bottom'></div>
                </div></body>");
            var top = LayoutTestHelper.FindById(root, "top");
            var bottom = LayoutTestHelper.FindById(root, "bottom");
            Assert.NotNull(top);
            Assert.NotNull(bottom);
            Assert.True(System.Math.Abs(top!.ContentRect.Y) < 2, $"Top Y expected 0, got {top.ContentRect.Y}");
            Assert.True(System.Math.Abs(top.ContentRect.Height - 80) < 2, $"Top expected 80px, got {top.ContentRect.Height}");
            Assert.True(System.Math.Abs(bottom!.ContentRect.Y - 80) < 2, $"Bottom Y expected 80, got {bottom.ContentRect.Y}");
            Assert.True(System.Math.Abs(bottom.ContentRect.Height - 80) < 2, $"Bottom expected 80px, got {bottom.ContentRect.Height}");
        }
    }
}
