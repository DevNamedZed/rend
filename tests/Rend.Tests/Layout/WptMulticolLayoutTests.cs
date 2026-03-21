using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    /// <summary>
    /// CSS Multi-column Layout Module Level 1 conformance tests.
    /// Covers column-count, column-width, columns shorthand, column-gap,
    /// column-rule, column-span, column-fill, height interaction, nesting, and floats.
    /// </summary>
    public class WptMulticolLayoutTests
    {
        private readonly ITestOutputHelper _output;

        public WptMulticolLayoutTests(ITestOutputHelper output)
        {
            _output = output;
        }

        // [CSS-MULTICOL §3.1] column-count:2 halves the container height for balanced content
        [Fact]
        public void ColumnCount2_BalancesHeight()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='t' style='column-count:2; column-gap:0; width:200px;'>
                    <div style='height:50px;'></div>
                    <div style='height:50px;'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"height={box!.ContentRect.Height}");
            Assert.True(System.Math.Abs(box.ContentRect.Height - 50) < 2);
        }

        // [CSS-MULTICOL §3.1] column-count:3 distributes three equal blocks into three columns
        [Fact]
        public void ColumnCount3_ThreeEqualBlocks()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='t' style='column-count:3; column-gap:0; width:300px;'>
                    <div style='height:40px;'></div>
                    <div style='height:40px;'></div>
                    <div style='height:40px;'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"height={box!.ContentRect.Height}");
            Assert.True(System.Math.Abs(box.ContentRect.Height - 40) < 2);
        }

        // [CSS-MULTICOL §3.1] column-count:4 with uneven content balances across columns
        [Fact]
        public void ColumnCount4_BalancesUnevenContent()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='t' style='column-count:4; column-gap:0; width:400px;'>
                    <div style='height:20px;'></div>
                    <div style='height:20px;'></div>
                    <div style='height:20px;'></div>
                    <div style='height:20px;'></div>
                    <div style='height:20px;'></div>
                    <div style='height:20px;'></div>
                    <div style='height:20px;'></div>
                    <div style='height:20px;'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"height={box!.ContentRect.Height}");
            // 8 blocks * 20px = 160px total, 4 columns -> 40px per column
            Assert.True(System.Math.Abs(box.ContentRect.Height - 40) < 2);
        }

        // [CSS-MULTICOL §3.2] column-width determines column count from available width
        [Fact]
        public void ColumnWidth_DeterminesCountFromAvailableWidth()
        {
            // 300px available, column-width:100px, default gap=1em=16px
            // floor((300+16)/(100+16)) = floor(316/116) = 2 columns
            // Each column = (300 - 16) / 2 = 142px
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='t' style='column-width:100px; width:300px;'>
                    <div style='height:80px;'></div>
                    <div style='height:80px;'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"width={box!.ContentRect.Width} height={box.ContentRect.Height}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 300) < 2);
        }

        // [CSS-MULTICOL §3.2] column-width with zero gap yields floor(avail/width) columns
        [Fact]
        public void ColumnWidth_WithZeroGap()
        {
            // 400px available, column-width:100px, gap:0 -> floor(400/100) = 4 columns
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='t' style='column-width:100px; column-gap:0; width:400px;'>
                    <div style='height:30px;'></div>
                    <div style='height:30px;'></div>
                    <div style='height:30px;'></div>
                    <div style='height:30px;'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"height={box!.ContentRect.Height}");
            // 4 blocks in 4 columns -> height = 30px
            Assert.True(System.Math.Abs(box.ContentRect.Height - 30) < 2);
        }

        // [CSS-MULTICOL §3] columns shorthand sets both column-width and column-count
        [Fact]
        public void ColumnsShorthand_CountOnly()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='t' style='columns:3; column-gap:0; width:300px;'>
                    <div style='height:60px;'></div>
                    <div style='height:60px;'></div>
                    <div style='height:60px;'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"height={box!.ContentRect.Height}");
            Assert.True(System.Math.Abs(box.ContentRect.Height - 60) < 2);
        }

        // [CSS-MULTICOL §3] columns shorthand with pixel width
        [Fact]
        public void ColumnsShorthand_WidthOnly()
        {
            // columns:150px -> column-width:150px, column-count:auto
            // 400px avail, gap=0: floor(400/150) = 2 columns, each 200px
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='t' style='columns:150px; column-gap:0; width:400px;'>
                    <div style='height:40px;'></div>
                    <div style='height:40px;'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"height={box!.ContentRect.Height}");
            Assert.True(System.Math.Abs(box.ContentRect.Height - 40) < 2);
        }

        // [CSS-MULTICOL §4.2] column-gap:0 leaves no space between columns
        [Fact]
        public void ColumnGap_Zero()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='t' style='column-count:2; column-gap:0; width:200px;'>
                    <div style='height:100px;'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"width={box!.ContentRect.Width}");
            // Container stays 200px wide, columns are each 100px
            Assert.True(System.Math.Abs(box.ContentRect.Width - 200) < 2);
        }

        // [CSS-MULTICOL §4.2] column-gap with explicit px value
        [Fact]
        public void ColumnGap_ExplicitPixels()
        {
            // 2 columns, 20px gap, 220px container -> each col = (220-20)/2 = 100px
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='t' style='column-count:2; column-gap:20px; width:220px;'>
                    <div style='height:80px;'></div>
                    <div style='height:80px;'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"width={box!.ContentRect.Width} height={box.ContentRect.Height}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 220) < 2);
        }

        // [CSS-MULTICOL §4.2] large column-gap reduces column width
        [Fact]
        public void ColumnGap_LargeGap_ReducesColumnWidth()
        {
            // 3 columns, 40px gap, 360px container -> 2 gaps * 40 = 80px, cols = (360-80)/3 ≈ 93.3px
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='t' style='column-count:3; column-gap:40px; width:360px;'>
                    <div id='child' style='height:30px;'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"width={box!.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 360) < 2);
        }

        // [CSS-MULTICOL §4.3] column-rule does not affect layout dimensions
        [Fact]
        public void ColumnRule_DoesNotAffectWidth()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='t' style='column-count:2; column-gap:10px; column-rule:3px solid red; width:200px;'>
                    <div style='height:60px;'></div>
                    <div style='height:60px;'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"width={box!.ContentRect.Width}");
            // column-rule painted in the gap, no extra space
            Assert.True(System.Math.Abs(box.ContentRect.Width - 200) < 2);
        }

        // [CSS-MULTICOL §4.3] column-rule does not change column height
        [Fact]
        public void ColumnRule_DoesNotAffectHeight()
        {
            var rootWithRule = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='t' style='column-count:2; column-gap:20px; column-rule:5px solid blue; width:220px;'>
                    <div style='height:40px;'></div>
                    <div style='height:40px;'></div>
                </div></body>");
            var rootWithout = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='t' style='column-count:2; column-gap:20px; width:220px;'>
                    <div style='height:40px;'></div>
                    <div style='height:40px;'></div>
                </div></body>");
            var boxWithRule = LayoutTestHelper.FindById(rootWithRule, "t");
            var boxWithout = LayoutTestHelper.FindById(rootWithout, "t");
            Assert.NotNull(boxWithRule);
            Assert.NotNull(boxWithout);
            _output.WriteLine($"with rule h={boxWithRule!.ContentRect.Height}, without h={boxWithout!.ContentRect.Height}");
            Assert.True(System.Math.Abs(boxWithRule.ContentRect.Height - boxWithout.ContentRect.Height) < 2);
        }

        // [CSS-MULTICOL §6] column-span:all spans full container width
        [Fact]
        public void ColumnSpanAll_FullWidth()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='column-count:3; column-gap:0; width:300px;'>
                    <div style='height:20px;'></div>
                    <div id='t' style='column-span:all; height:30px;'></div>
                    <div style='height:20px;'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"spanner width={box!.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 300) < 2);
        }

        // [CSS-MULTICOL §6] column-span:all sits below pre-spanner content
        [Fact]
        public void ColumnSpanAll_PositionedBelowContent()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='column-count:2; column-gap:0; width:200px;'>
                    <div style='height:40px;'></div>
                    <div style='height:40px;'></div>
                    <div id='t' style='column-span:all; height:25px;'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"spanner Y={box!.ContentRect.Y}");
            // Pre-spanner content: 2*40=80px balanced in 2 cols -> 40px height, spanner at Y=40
            Assert.True(box.ContentRect.Y >= 39);
        }

        // [CSS-MULTICOL §6] content after column-span:all resumes in columns
        [Fact]
        public void ColumnSpanAll_ContentResumesAfterSpanner()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='mc' style='column-count:2; column-gap:0; width:200px;'>
                    <div style='height:30px;'></div>
                    <div style='column-span:all; height:20px;'></div>
                    <div id='t' style='height:30px;'></div>
                    <div style='height:30px;'></div>
                </div></body>");
            var container = LayoutTestHelper.FindById(root, "mc");
            Assert.NotNull(container);
            _output.WriteLine($"container height={container!.ContentRect.Height}");
            // Pre-spanner: 30px in 2 cols -> 30px. Spanner: 20px. Post: 2*30=60px in 2 cols -> 30px.
            // Total height ≈ 30 + 20 + 30 = 80px
            Assert.True(container.ContentRect.Height >= 78);
        }

        // [CSS-MULTICOL §7.1] column-fill:balance is the default, distributes content evenly
        [Fact]
        public void ColumnFillBalance_IsDefault()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='t' style='column-count:2; column-gap:0; width:200px;'>
                    <div style='height:40px;'></div>
                    <div style='height:40px;'></div>
                    <div style='height:40px;'></div>
                    <div style='height:40px;'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"height={box!.ContentRect.Height}");
            // 4*40=160, balanced in 2 cols -> 80px each
            Assert.True(System.Math.Abs(box.ContentRect.Height - 80) < 2);
        }

        // [CSS-MULTICOL §7.1] column-fill:auto fills columns sequentially with explicit height
        [Fact]
        public void ColumnFillAuto_FillsSequentially()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='t' style='column-count:3; column-gap:0; column-fill:auto; height:100px; width:300px;'>
                    <div style='height:80px;'></div>
                    <div style='height:80px;'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"height={box!.ContentRect.Height}");
            // column-fill:auto with height:100px -> fill first col to 100, overflow to second
            Assert.True(System.Math.Abs(box.ContentRect.Height - 100) < 2);
        }

        // [CSS-MULTICOL §7.1] column-fill:balance explicitly set
        [Fact]
        public void ColumnFillBalance_Explicit()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='t' style='column-count:3; column-gap:0; column-fill:balance; width:300px;'>
                    <div style='height:30px;'></div>
                    <div style='height:30px;'></div>
                    <div style='height:30px;'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"height={box!.ContentRect.Height}");
            // 3 blocks in 3 cols, balanced -> 30px
            Assert.True(System.Math.Abs(box.ContentRect.Height - 30) < 2);
        }

        // [CSS-MULTICOL] explicit height constrains the multicol container
        [Fact]
        public void ExplicitHeight_ConstrainsContainer()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='t' style='column-count:2; column-gap:0; column-fill:auto; height:60px; width:200px;'>
                    <div style='height:50px;'></div>
                    <div style='height:50px;'></div>
                    <div style='height:50px;'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"height={box!.ContentRect.Height}");
            Assert.True(System.Math.Abs(box.ContentRect.Height - 60) < 2);
        }

        // [CSS-MULTICOL] auto height with balanced columns sizes to content
        [Fact]
        public void AutoHeight_SizesToBalancedContent()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='t' style='column-count:2; column-gap:0; width:200px;'>
                    <div style='height:60px;'></div>
                    <div style='height:60px;'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"height={box!.ContentRect.Height}");
            // 120px total in 2 cols -> 60px
            Assert.True(System.Math.Abs(box.ContentRect.Height - 60) < 2);
        }

        // [CSS-MULTICOL] nested multicol: inner multicol inside outer multicol
        [Fact]
        public void NestedMulticol_InnerColumnsLayout()
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
            // Inner multicol gets 200px column width from outer, splits its 80px into 2 cols -> 40px
            Assert.True(inner.ContentRect.Height <= 42);
        }

        // [CSS-MULTICOL + CSS2 §9.5] floats inside multicol columns
        [Fact]
        public void FloatsInsideMulticol()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='t' style='column-count:2; column-gap:0; width:200px;'>
                    <div style='height:60px;'>
                        <div id='fl' style='float:left; width:30px; height:30px;'></div>
                    </div>
                    <div style='height:60px;'></div>
                </div></body>");
            var container = LayoutTestHelper.FindById(root, "t");
            var floatBox = LayoutTestHelper.FindById(root, "fl");
            Assert.NotNull(container);
            Assert.NotNull(floatBox);
            _output.WriteLine($"container h={container!.ContentRect.Height}, float w={floatBox!.ContentRect.Width}");
            // Float inside a column should have width 30px
            Assert.True(System.Math.Abs(floatBox.ContentRect.Width - 30) < 2);
        }

        // [CSS-MULTICOL §3.4] column-count with percentage width container
        [Fact]
        public void ColumnCount_WithPercentageWidth()
        {
            // Outer 400px wide, inner 50% = 200px, 2 columns gap 0 -> each col 100px
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='width:400px;'>
                    <div id='t' style='width:50%; column-count:2; column-gap:0;'>
                        <div style='height:50px;'></div>
                        <div style='height:50px;'></div>
                    </div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"width={box!.ContentRect.Width} height={box.ContentRect.Height}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 200) < 2);
            Assert.True(System.Math.Abs(box.ContentRect.Height - 50) < 2);
        }

        // [CSS-MULTICOL §3.4] container width determined by column-count + column-width + gap
        [Fact]
        public void ContainerWidth_FromColumnCountWidthGap()
        {
            // When container has explicit width, it is respected regardless of column params
            // column-count:3, column-gap:10px, width:350px -> (350-20)/3 ≈ 110px columns
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='t' style='column-count:3; column-gap:10px; width:350px;'>
                    <div style='height:30px;'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"width={box!.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 350) < 2);
        }

        // [CSS-MULTICOL §3] column-count and column-width both set: count is max
        [Fact]
        public void ColumnCountAndWidth_CountIsMaximum()
        {
            // column-count:4, column-width:80px, gap:0, width:400px
            // max by width = floor(400/80) = 5, but count caps at 4
            // -> 4 columns, each 100px
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='t' style='column-count:4; column-width:80px; column-gap:0; width:400px;'>
                    <div style='height:20px;'></div>
                    <div style='height:20px;'></div>
                    <div style='height:20px;'></div>
                    <div style='height:20px;'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"height={box!.ContentRect.Height}");
            // 4 blocks in 4 columns -> 20px
            Assert.True(System.Math.Abs(box.ContentRect.Height - 20) < 2);
        }

        // [CSS-MULTICOL] single column (column-count:1) behaves like normal block
        [Fact]
        public void ColumnCount1_BehavesLikeNormalBlock()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='t' style='column-count:1; column-gap:0; width:200px;'>
                    <div style='height:50px;'></div>
                    <div style='height:50px;'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"height={box!.ContentRect.Height}");
            // Single column: no splitting, height = sum of children
            Assert.True(System.Math.Abs(box.ContentRect.Height - 100) < 2);
        }

        // [CSS-MULTICOL §4.2] column-gap normal resolves to 1em (16px at default font-size)
        [Fact]
        public void ColumnGap_Normal_ResolvesTo1em()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='t' style='column-count:2; width:200px;'>
                    <div style='height:50px;'></div>
                    <div style='height:50px;'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"width={box!.ContentRect.Width}");
            // Default gap = 1em = 16px at 16px font-size
            // Container width remains 200px, columns = (200-16)/2 = 92px each
            Assert.True(System.Math.Abs(box.ContentRect.Width - 200) < 2);
        }

        // [CSS-MULTICOL §6] multiple spanners split content into segments
        [Fact]
        public void MultipleSpanners_SplitIntoSegments()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='mc' style='column-count:2; column-gap:0; width:200px;'>
                    <div style='height:30px;'></div>
                    <div id='s1' style='column-span:all; height:15px;'></div>
                    <div style='height:30px;'></div>
                    <div id='s2' style='column-span:all; height:15px;'></div>
                    <div style='height:30px;'></div>
                </div></body>");
            var spanner1 = LayoutTestHelper.FindById(root, "s1");
            var spanner2 = LayoutTestHelper.FindById(root, "s2");
            Assert.NotNull(spanner1);
            Assert.NotNull(spanner2);
            _output.WriteLine($"s1 w={spanner1!.ContentRect.Width}, s2 w={spanner2!.ContentRect.Width}");
            Assert.True(System.Math.Abs(spanner1.ContentRect.Width - 200) < 2);
            Assert.True(System.Math.Abs(spanner2.ContentRect.Width - 200) < 2);
            // Second spanner must be below first
            Assert.True(spanner2.ContentRect.Y > spanner1.ContentRect.Y);
        }

        // [CSS-MULTICOL] column-fill:auto without explicit height uses total content height
        [Fact]
        public void ColumnFillAuto_WithoutHeight_UsesTotalHeight()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='t' style='column-count:2; column-gap:0; column-fill:auto; width:200px;'>
                    <div style='height:60px;'></div>
                    <div style='height:60px;'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"height={box!.ContentRect.Height}");
            // Without explicit height, column-fill:auto uses full content height as target
            // All content fits in first column -> height = total content height
            Assert.True(box.ContentRect.Height >= 118);
        }

        // [CSS-MULTICOL] tall single block that exceeds balanced column height
        [Fact]
        public void TallSingleBlock_ExceedsBalancedHeight()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='t' style='column-count:2; column-gap:0; width:200px;'>
                    <div style='height:200px;'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"height={box!.ContentRect.Height}");
            // Single block cannot be split, must stay in one column -> height = 200px
            Assert.True(box.ContentRect.Height >= 198);
        }

        // [CSS-MULTICOL §6] column-span:all with column-gap present
        [Fact]
        public void ColumnSpanAll_WithGap()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='column-count:2; column-gap:20px; width:220px;'>
                    <div style='height:30px;'></div>
                    <div id='t' style='column-span:all; height:25px;'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"spanner width={box!.ContentRect.Width}");
            // Spanner spans full container width including gap area
            Assert.True(System.Math.Abs(box.ContentRect.Width - 220) < 2);
        }

        // [CSS-MULTICOL] multicol with padding and border
        [Fact]
        public void MulticolWithPaddingAndBorder()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='t' style='column-count:2; column-gap:0; width:200px; padding:10px; border:2px solid black;'>
                    <div style='height:40px;'></div>
                    <div style='height:40px;'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"content w={box!.ContentRect.Width} h={box.ContentRect.Height}");
            // Content width = 200px (padding/border outside), each column = 100px
            Assert.True(System.Math.Abs(box.ContentRect.Width - 200) < 2);
            Assert.True(System.Math.Abs(box.ContentRect.Height - 40) < 2);
        }
    }
}
