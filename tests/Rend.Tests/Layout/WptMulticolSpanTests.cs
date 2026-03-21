using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    /// <summary>
    /// Tests covering CSS Multi-column Layout: column-span, column-fill, fragmentation,
    /// column-count, column-width, column-gap, column-rule, and columns shorthand.
    /// </summary>
    public class WptMulticolSpanTests
    {
        private readonly ITestOutputHelper _output;

        public WptMulticolSpanTests(ITestOutputHelper output)
        {
            _output = output;
        }

        // [CSS-MULTICOL §6.1] column-span:all spans full container width
        [Fact]
        public void ColumnSpanAll_FullWidth()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='mc' style='column-count:2; column-gap:0; width:300px;'>
                    <div style='height:40px;'></div>
                    <div id='t' style='column-span:all; height:20px;'></div>
                    <div style='height:40px;'></div>
                </div></body>");
            var spanner = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(spanner);
            _output.WriteLine($"spanner width={spanner!.ContentRect.Width}");
            Assert.True(System.Math.Abs(spanner.ContentRect.Width - 300) < 2,
                $"column-span:all should span full 300px (got {spanner.ContentRect.Width})");
        }

        // [CSS-MULTICOL §6.1] column-span:all splits columnar content into before/after segments
        [Fact]
        public void ColumnSpanAll_SplitsContent()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='mc' style='column-count:2; column-gap:0; width:200px;'>
                    <div id='before' style='height:40px;'></div>
                    <div id='t' style='column-span:all; height:30px;'></div>
                    <div id='after' style='height:40px;'></div>
                </div></body>");
            var spanner = LayoutTestHelper.FindById(root, "t");
            var container = LayoutTestHelper.FindById(root, "mc");
            Assert.NotNull(spanner);
            Assert.NotNull(container);
            _output.WriteLine($"spanner Y={spanner!.ContentRect.Y}, container height={container!.ContentRect.Height}");
            // Spanner should appear between the two columnar segments
            Assert.True(spanner.ContentRect.Y > 0,
                $"Spanner should be below pre-spanner content (Y={spanner.ContentRect.Y})");
        }

        // [CSS-MULTICOL §6.1] content before spanner is laid out in columns
        [Fact]
        public void ContentBeforeSpanner_InColumns()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='mc' style='column-count:2; column-gap:0; width:200px;'>
                    <div style='height:40px;'></div>
                    <div style='height:40px;'></div>
                    <div id='t' style='column-span:all; height:20px;'></div>
                </div></body>");
            var spanner = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(spanner);
            _output.WriteLine($"spanner Y={spanner!.ContentRect.Y}");
            // 2 blocks of 40px balanced across 2 columns = 40px column height
            Assert.True(spanner.ContentRect.Y < 82,
                $"Pre-spanner content should be columnar, spanner Y should be ~40 (got {spanner.ContentRect.Y})");
        }

        // [CSS-MULTICOL §6.1] content after spanner is laid out in columns
        [Fact]
        public void ContentAfterSpanner_InColumns()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='mc' style='column-count:2; column-gap:0; width:200px;'>
                    <div id='t' style='column-span:all; height:20px;'></div>
                    <div style='height:50px;'></div>
                    <div style='height:50px;'></div>
                </div></body>");
            var container = LayoutTestHelper.FindById(root, "mc");
            Assert.NotNull(container);
            _output.WriteLine($"container height={container!.ContentRect.Height}");
            // spanner 20px + post-spanner 2*50px balanced across 2 cols = 50px => total ~70px
            Assert.True(container.ContentRect.Height < 121,
                $"Post-spanner should be columnar, total height < 120 (got {container.ContentRect.Height})");
        }

        // [CSS-MULTICOL §6.1] multiple spanners in same multicol container
        [Fact]
        public void MultipleSpanners()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='mc' style='column-count:2; column-gap:0; width:200px;'>
                    <div style='height:30px;'></div>
                    <div id='s1' style='column-span:all; height:20px;'></div>
                    <div style='height:30px;'></div>
                    <div id='s2' style='column-span:all; height:20px;'></div>
                    <div style='height:30px;'></div>
                </div></body>");
            var firstSpanner = LayoutTestHelper.FindById(root, "s1");
            var secondSpanner = LayoutTestHelper.FindById(root, "s2");
            Assert.NotNull(firstSpanner);
            Assert.NotNull(secondSpanner);
            _output.WriteLine($"s1 Y={firstSpanner!.ContentRect.Y}, s2 Y={secondSpanner!.ContentRect.Y}");
            Assert.True(secondSpanner!.ContentRect.Y > firstSpanner.ContentRect.Y,
                $"Second spanner should be below first (s1 Y={firstSpanner.ContentRect.Y}, s2 Y={secondSpanner.ContentRect.Y})");
        }

        // [CSS-MULTICOL §6.1] spanner with margin
        [Fact]
        public void SpannerWithMargin()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='mc' style='column-count:2; column-gap:0; width:200px;'>
                    <div style='height:30px;'></div>
                    <div id='t' style='column-span:all; height:20px; margin:10px 0;'></div>
                    <div style='height:30px;'></div>
                </div></body>");
            var spanner = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(spanner);
            _output.WriteLine($"spanner marginTop={spanner!.MarginTop}, marginBottom={spanner.MarginBottom}");
            Assert.True(spanner.ContentRect.Height >= 19,
                $"Spanner height should be 20px (got {spanner.ContentRect.Height})");
        }

        // [CSS-MULTICOL §6.1] spanner with padding
        [Fact]
        public void SpannerWithPadding()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='mc' style='column-count:2; column-gap:0; width:200px;'>
                    <div style='height:30px;'></div>
                    <div id='t' style='column-span:all; height:20px; padding:10px;'></div>
                    <div style='height:30px;'></div>
                </div></body>");
            var spanner = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(spanner);
            _output.WriteLine($"spanner paddingTop={spanner!.PaddingTop}, borderBox height={spanner.BorderRect.Height}");
            Assert.True(System.Math.Abs(spanner.PaddingTop - 10) < 2,
                $"Spanner should have 10px padding-top (got {spanner.PaddingTop})");
            Assert.True(System.Math.Abs(spanner.PaddingBottom - 10) < 2,
                $"Spanner should have 10px padding-bottom (got {spanner.PaddingBottom})");
        }

        // [CSS-MULTICOL §6.1] spanner with border
        [Fact]
        public void SpannerWithBorder()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='mc' style='column-count:2; column-gap:0; width:200px;'>
                    <div style='height:30px;'></div>
                    <div id='t' style='column-span:all; height:20px; border:5px solid black;'></div>
                    <div style='height:30px;'></div>
                </div></body>");
            var spanner = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(spanner);
            _output.WriteLine($"spanner borderTop={spanner!.BorderTopWidth}, borderBox={spanner.BorderRect.Height}");
            Assert.True(System.Math.Abs(spanner.BorderTopWidth - 5) < 2,
                $"Spanner should have 5px border-top (got {spanner.BorderTopWidth})");
            Assert.True(System.Math.Abs(spanner.BorderRect.Height - 30) < 2,
                $"Spanner border-box height should be 30px (got {spanner.BorderRect.Height})");
        }

        // [CSS-MULTICOL §7.1] column-fill:balance distributes content evenly
        [Fact]
        public void ColumnFillBalance_DistributesEvenly()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='t' style='column-count:3; column-fill:balance; column-gap:0; width:300px;'>
                    <div style='height:30px;'></div>
                    <div style='height:30px;'></div>
                    <div style='height:30px;'></div>
                    <div style='height:30px;'></div>
                    <div style='height:30px;'></div>
                    <div style='height:30px;'></div>
                </div></body>");
            var container = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(container);
            _output.WriteLine($"container height={container!.ContentRect.Height}");
            // 6 blocks * 30px = 180px total, 3 columns balanced => 60px per column
            Assert.True(System.Math.Abs(container.ContentRect.Height - 60) < 2,
                $"Balanced 3-column height should be ~60px (got {container.ContentRect.Height})");
        }

        // [CSS-MULTICOL §7.1] column-fill:auto fills columns sequentially with explicit height
        [Fact]
        public void ColumnFillAuto_FillsSequentially()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='t' style='column-count:3; column-fill:auto; column-gap:0; width:300px; height:100px;'>
                    <div style='height:80px;'></div>
                    <div style='height:80px;'></div>
                </div></body>");
            var container = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(container);
            _output.WriteLine($"container height={container!.ContentRect.Height}");
            // column-fill:auto with explicit height 100px: fill first column to 100px, overflow to next
            Assert.True(System.Math.Abs(container.ContentRect.Height - 100) < 2,
                $"column-fill:auto should respect explicit height 100px (got {container.ContentRect.Height})");
        }

        // [CSS-MULTICOL §3.1] column-count:3 produces 3 equal-width columns
        [Fact]
        public void ColumnCount3_ColumnWidths()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='t' style='column-count:3; column-gap:0; width:300px;'>
                    <div style='height:90px;'></div>
                    <div style='height:90px;'></div>
                    <div style='height:90px;'></div>
                </div></body>");
            var container = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(container);
            _output.WriteLine($"container width={container!.ContentRect.Width}, height={container.ContentRect.Height}");
            // 300px / 3 columns = 100px each, balanced height = 90px
            Assert.True(System.Math.Abs(container.ContentRect.Width - 300) < 2,
                $"Container width should be 300px (got {container.ContentRect.Width})");
            Assert.True(container.ContentRect.Height <= 91,
                $"3 columns balanced should give ~90px height (got {container.ContentRect.Height})");
        }

        // [CSS-MULTICOL §3.1] column-count:4 produces 4 equal-width columns
        [Fact]
        public void ColumnCount4_ColumnWidths()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='t' style='column-count:4; column-gap:0; width:400px;'>
                    <div style='height:40px;'></div>
                    <div style='height:40px;'></div>
                    <div style='height:40px;'></div>
                    <div style='height:40px;'></div>
                </div></body>");
            var container = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(container);
            _output.WriteLine($"container height={container!.ContentRect.Height}");
            // 4 blocks of 40px across 4 columns = 40px height
            Assert.True(System.Math.Abs(container.ContentRect.Height - 40) < 2,
                $"4 columns should give ~40px height (got {container.ContentRect.Height})");
        }

        // [CSS-MULTICOL §3.2] column-width determines actual column count
        [Fact]
        public void ColumnWidth_DeterminesCount()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='t' style='column-width:100px; column-gap:0; width:300px;'>
                    <div style='height:60px;'></div>
                    <div style='height:60px;'></div>
                    <div style='height:60px;'></div>
                </div></body>");
            var container = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(container);
            _output.WriteLine($"container height={container!.ContentRect.Height}");
            // 300px / 100px = 3 columns, balanced 3*60px / 3 = 60px
            Assert.True(container.ContentRect.Height <= 61,
                $"column-width:100px in 300px should give 3 columns, height ~60px (got {container.ContentRect.Height})");
        }

        // [CSS-MULTICOL §3.3] columns shorthand sets both count and width
        [Fact]
        public void ColumnsShorthand()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='t' style='columns:2; column-gap:0; width:200px;'>
                    <div style='height:50px;'></div>
                    <div style='height:50px;'></div>
                </div></body>");
            var container = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(container);
            _output.WriteLine($"container height={container!.ContentRect.Height}");
            // columns:2 => column-count:2, balanced: 2*50px / 2 = 50px
            Assert.True(System.Math.Abs(container.ContentRect.Height - 50) < 2,
                $"columns:2 shorthand should give 2 columns, height ~50px (got {container.ContentRect.Height})");
        }

        // [CSS-MULTICOL §5.1] column-gap:0 leaves no gap between columns
        [Fact]
        public void ColumnGapZero()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='t' style='column-count:2; column-gap:0; width:200px;'>
                    <div style='height:60px;'></div>
                    <div style='height:60px;'></div>
                </div></body>");
            var container = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(container);
            _output.WriteLine($"container width={container!.ContentRect.Width}, height={container.ContentRect.Height}");
            // 2 columns, each 100px wide, no gap, balanced 60px height
            Assert.True(System.Math.Abs(container.ContentRect.Width - 200) < 2,
                $"Container should use full width with gap:0 (got {container.ContentRect.Width})");
        }

        // [CSS-MULTICOL §5.1] column-gap:20px reduces column width
        [Fact]
        public void ColumnGap20px()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='t' style='column-count:2; column-gap:20px; width:220px;'>
                    <div style='height:60px;'></div>
                    <div style='height:60px;'></div>
                </div></body>");
            var container = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(container);
            _output.WriteLine($"container width={container!.ContentRect.Width}, height={container.ContentRect.Height}");
            // (220 - 20) / 2 = 100px per column, balanced 60px
            Assert.True(System.Math.Abs(container.ContentRect.Width - 220) < 2,
                $"Container should maintain full width with gap (got {container.ContentRect.Width})");
            Assert.True(System.Math.Abs(container.ContentRect.Height - 60) < 2,
                $"Balanced 2-column height should be ~60px (got {container.ContentRect.Height})");
        }

        // [CSS-MULTICOL §5.2] column-rule does not affect layout dimensions
        [Fact]
        public void ColumnRule_DoesNotAffectLayout()
        {
            var rootWithRule = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='t' style='column-count:2; column-gap:20px; column-rule:5px solid red; width:220px;'>
                    <div style='height:60px;'></div>
                    <div style='height:60px;'></div>
                </div></body>");
            var rootWithoutRule = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='t' style='column-count:2; column-gap:20px; width:220px;'>
                    <div style='height:60px;'></div>
                    <div style='height:60px;'></div>
                </div></body>");
            var withRule = LayoutTestHelper.FindById(rootWithRule, "t");
            var withoutRule = LayoutTestHelper.FindById(rootWithoutRule, "t");
            Assert.NotNull(withRule);
            Assert.NotNull(withoutRule);
            _output.WriteLine($"withRule h={withRule!.ContentRect.Height}, withoutRule h={withoutRule!.ContentRect.Height}");
            Assert.True(System.Math.Abs(withRule.ContentRect.Height - withoutRule.ContentRect.Height) < 2,
                "column-rule should not affect layout height");
            Assert.True(System.Math.Abs(withRule.ContentRect.Width - withoutRule.ContentRect.Width) < 2,
                "column-rule should not affect layout width");
        }

        // [CSS-MULTICOL §3.1] column-count with explicit height constrains
        [Fact]
        public void ColumnCountWithExplicitHeight()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='t' style='column-count:2; column-gap:0; width:200px; height:50px;'>
                    <div style='height:80px;'></div>
                    <div style='height:80px;'></div>
                </div></body>");
            var container = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(container);
            _output.WriteLine($"container height={container!.ContentRect.Height}");
            // Explicit height should be respected
            Assert.True(System.Math.Abs(container.ContentRect.Height - 50) < 2,
                $"Explicit height:50px should be respected (got {container.ContentRect.Height})");
        }

        // [CSS-MULTICOL §3] nested multicol containers
        [Fact]
        public void NestedMulticol()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='outer' style='column-count:2; column-gap:0; width:400px;'>
                    <div id='inner' style='column-count:2; column-gap:0;'>
                        <div style='height:40px;'></div>
                        <div style='height:40px;'></div>
                    </div>
                </div></body>");
            var outer = LayoutTestHelper.FindById(root, "outer");
            var inner = LayoutTestHelper.FindById(root, "inner");
            Assert.NotNull(outer);
            Assert.NotNull(inner);
            _output.WriteLine($"outer h={outer!.ContentRect.Height}, inner h={inner!.ContentRect.Height}");
            // Inner multicol should work within outer column width
            Assert.True(inner.ContentRect.Height < 81,
                $"Inner multicol should columnize content (got {inner.ContentRect.Height})");
        }

        // [CSS-MULTICOL §3] multicol with auto height sizes to content
        [Fact]
        public void MulticolAutoHeight()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='t' style='column-count:2; column-gap:0; width:200px;'>
                    <div style='height:100px;'></div>
                    <div style='height:100px;'></div>
                </div></body>");
            var container = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(container);
            _output.WriteLine($"container height={container!.ContentRect.Height}");
            // Auto height with balance: 200px / 2 columns = 100px
            Assert.True(container.ContentRect.Height > 0,
                $"Auto height should size to balanced content (got {container.ContentRect.Height})");
            Assert.True(container.ContentRect.Height <= 101,
                $"Balanced columns should give ~100px height (got {container.ContentRect.Height})");
        }

        // [CSS-MULTICOL + CSS2 §9.5] float inside multicol stays within column
        [Fact]
        public void MulticolWithFloatInside()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='mc' style='column-count:2; column-gap:0; width:200px;'>
                    <div style='height:30px;'>
                        <div id='t' style='float:left; width:40px; height:20px;'></div>
                    </div>
                    <div style='height:30px;'></div>
                </div></body>");
            var floatBox = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(floatBox);
            _output.WriteLine($"float width={floatBox!.ContentRect.Width}, X={floatBox.ContentRect.X}");
            Assert.True(System.Math.Abs(floatBox.ContentRect.Width - 40) < 2,
                $"Float should maintain 40px width (got {floatBox.ContentRect.Width})");
        }

        // [CSS-BREAK §3.3] break-inside:avoid keeps block together
        [Fact]
        public void BreakInsideAvoid()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='mc' style='column-count:2; column-gap:0; width:200px;'>
                    <div style='height:30px;'></div>
                    <div id='t' style='break-inside:avoid; height:60px;'></div>
                </div></body>");
            var container = LayoutTestHelper.FindById(root, "mc");
            var avoidBlock = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(container);
            Assert.NotNull(avoidBlock);
            _output.WriteLine($"container h={container!.ContentRect.Height}, avoidBlock h={avoidBlock!.ContentRect.Height}");
            // The block with break-inside:avoid should not be split
            Assert.True(System.Math.Abs(avoidBlock.ContentRect.Height - 60) < 2,
                $"break-inside:avoid block should stay intact at 60px (got {avoidBlock.ContentRect.Height})");
        }

        // [CSS-BREAK §3.1] break-before:column forces column break
        [Fact]
        public void BreakBeforeColumn()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='mc' style='column-count:2; column-gap:0; width:200px;'>
                    <div style='height:20px;'></div>
                    <div id='t' style='break-before:column; height:20px;'></div>
                </div></body>");
            var container = LayoutTestHelper.FindById(root, "mc");
            Assert.NotNull(container);
            _output.WriteLine($"container h={container!.ContentRect.Height}");
            // With break-before:column, first block in col1, second forced to col2
            // Container height should be ~20px (each column has 20px content)
            Assert.True(container.ContentRect.Height <= 21,
                $"break-before:column should force new column, height ~20px (got {container.ContentRect.Height})");
        }

        // [CSS-MULTICOL §6.1] spanner with margin-top and margin-bottom adds spacing
        [Fact]
        public void SpannerMarginAffectsPosition()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='mc' style='column-count:2; column-gap:0; width:200px;'>
                    <div style='height:40px;'></div>
                    <div id='t' style='column-span:all; height:20px; margin-top:10px; margin-bottom:10px;'></div>
                    <div id='after' style='height:40px;'></div>
                </div></body>");
            var spanner = LayoutTestHelper.FindById(root, "t");
            var afterContent = LayoutTestHelper.FindById(root, "after");
            Assert.NotNull(spanner);
            Assert.NotNull(afterContent);
            _output.WriteLine($"spanner Y={spanner!.ContentRect.Y}, after Y={afterContent!.ContentRect.Y}");
            // Post-spanner content should be below spanner + its margins
            float spannerBottom = spanner.ContentRect.Y + spanner.ContentRect.Height + spanner.MarginBottom;
            Assert.True(afterContent.ContentRect.Y >= spannerBottom - 2,
                $"Content after spanner should respect spanner margins (after Y={afterContent.ContentRect.Y}, spannerBottom={spannerBottom})");
        }

        // [CSS-MULTICOL §3.1] single column when column-count:1
        [Fact]
        public void ColumnCount1_SingleColumn()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='t' style='column-count:1; column-gap:0; width:200px;'>
                    <div style='height:50px;'></div>
                    <div style='height:50px;'></div>
                </div></body>");
            var container = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(container);
            _output.WriteLine($"container height={container!.ContentRect.Height}");
            // column-count:1 = single column, total height = 100px
            Assert.True(System.Math.Abs(container.ContentRect.Height - 100) < 2,
                $"column-count:1 should give full stacked height 100px (got {container.ContentRect.Height})");
        }

        // [CSS-MULTICOL §6.1] spanner width equals container width minus padding
        [Fact]
        public void SpannerWidth_ContainerWithPadding()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='mc' style='column-count:2; column-gap:0; width:300px; padding:10px;'>
                    <div style='height:30px;'></div>
                    <div id='t' style='column-span:all; height:20px;'></div>
                </div></body>");
            var spanner = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(spanner);
            _output.WriteLine($"spanner width={spanner!.ContentRect.Width}");
            // Container content width = 300px (width is content-box by default)
            Assert.True(System.Math.Abs(spanner.ContentRect.Width - 300) < 2,
                $"Spanner should span container content width 300px (got {spanner.ContentRect.Width})");
        }

        // [CSS-MULTICOL §7.1] balanced columns with uneven content
        [Fact]
        public void ColumnFillBalance_UnevenContent()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='t' style='column-count:2; column-fill:balance; column-gap:0; width:200px;'>
                    <div style='height:30px;'></div>
                    <div style='height:30px;'></div>
                    <div style='height:30px;'></div>
                </div></body>");
            var container = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(container);
            _output.WriteLine($"container height={container!.ContentRect.Height}");
            // 3 blocks * 30px = 90px total, 2 columns balanced => col1: 60px, col2: 30px => height=60px
            Assert.True(container.ContentRect.Height <= 61,
                $"Uneven content should balance to ~60px height (got {container.ContentRect.Height})");
        }

        // [CSS-MULTICOL §5.1] default column-gap is 1em (16px at default font-size)
        [Fact]
        public void ColumnGapDefault_1em()
        {
            var rootNoGap = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='t' style='column-count:2; column-gap:0; width:200px;'>
                    <div style='height:60px;'></div>
                </div></body>");
            var rootDefaultGap = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='t' style='column-count:2; width:200px;'>
                    <div style='height:60px;'></div>
                </div></body>");
            var noGapContainer = LayoutTestHelper.FindById(rootNoGap, "t");
            var defaultGapContainer = LayoutTestHelper.FindById(rootDefaultGap, "t");
            Assert.NotNull(noGapContainer);
            Assert.NotNull(defaultGapContainer);
            _output.WriteLine($"noGap w={noGapContainer!.ContentRect.Width}, defaultGap w={defaultGapContainer!.ContentRect.Width}");
            // Both containers should have same total width; difference is in column widths
            Assert.True(System.Math.Abs(noGapContainer.ContentRect.Width - defaultGapContainer!.ContentRect.Width) < 2,
                "Container width should be same regardless of gap");
        }

        // [CSS-MULTICOL §3.1] large column-count with small content still works
        [Fact]
        public void LargeColumnCount_SmallContent()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='t' style='column-count:5; column-gap:0; width:500px;'>
                    <div style='height:20px;'></div>
                </div></body>");
            var container = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(container);
            _output.WriteLine($"container height={container!.ContentRect.Height}");
            // Only 20px content, 5 columns — content goes in first column, height = 20px
            Assert.True(container.ContentRect.Height <= 21,
                $"Small content in many columns should give height ~20px (got {container.ContentRect.Height})");
        }

        // [CSS-MULTICOL §6.1] spanner between blocks preserves block ordering
        [Fact]
        public void SpannerPreservesBlockOrder()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='mc' style='column-count:2; column-gap:0; width:200px;'>
                    <div id='before' style='height:30px;'></div>
                    <div id='span' style='column-span:all; height:10px;'></div>
                    <div id='after' style='height:30px;'></div>
                </div></body>");
            var before = LayoutTestHelper.FindById(root, "before");
            var span = LayoutTestHelper.FindById(root, "span");
            var after = LayoutTestHelper.FindById(root, "after");
            Assert.NotNull(before);
            Assert.NotNull(span);
            Assert.NotNull(after);
            _output.WriteLine($"before Y={before!.ContentRect.Y}, span Y={span!.ContentRect.Y}, after Y={after!.ContentRect.Y}");
            Assert.True(span.ContentRect.Y >= before.ContentRect.Y,
                "Spanner should be at or below pre-spanner content");
            Assert.True(after.ContentRect.Y >= span.ContentRect.Y,
                "Post-spanner content should be at or below spanner");
        }

        // [CSS-MULTICOL §3.2] column-width larger than container gives single column
        [Fact]
        public void ColumnWidthLargerThanContainer()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='t' style='column-width:500px; column-gap:0; width:200px;'>
                    <div style='height:50px;'></div>
                    <div style='height:50px;'></div>
                </div></body>");
            var container = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(container);
            _output.WriteLine($"container height={container!.ContentRect.Height}");
            // column-width:500px > container 200px => 1 column, stacked height = 100px
            Assert.True(System.Math.Abs(container.ContentRect.Height - 100) < 2,
                $"column-width > container should give single column, height 100px (got {container.ContentRect.Height})");
        }
    }
}
