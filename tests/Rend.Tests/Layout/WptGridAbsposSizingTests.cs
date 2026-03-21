using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    /// <summary>
    /// Tests for absolute positioning interactions with CSS Grid layout.
    /// Covers containing block resolution, sizing, percentage resolution,
    /// centering, gap interaction, and grid-area scoping.
    /// </summary>
    public class WptGridAbsposSizingTests
    {
        private readonly ITestOutputHelper _output;
        public WptGridAbsposSizingTests(ITestOutputHelper output) { _output = output; }

        // [CSS-GRID §6.1] Grid container with position:relative is CB for abspos children
        [Fact]
        public void AbsposInGrid_UsesGridAsContainingBlock()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;position:relative;width:300px;height:200px;grid-template-columns:1fr'>
                    <div id='t' style='position:absolute;top:10px;left:20px;width:50px;height:50px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.X - 20) < 2, $"Left=20 (got {target.ContentRect.X})");
            Assert.True(System.Math.Abs(target.ContentRect.Y - 10) < 2, $"Top=10 (got {target.ContentRect.Y})");
        }

        // [CSS-POSITION §5.1] Abspos element does not participate in grid sizing
        [Fact]
        public void AbsposDoesNotAffectGridHeight()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='grid' style='display:grid;position:relative;width:200px;grid-template-columns:1fr'>
                    <div style='height:40px'></div>
                    <div style='position:absolute;top:0;left:0;width:300px;height:500px'></div>
                </div></body>");
            var grid = LayoutTestHelper.FindById(root, "grid")!;
            Assert.True(System.Math.Abs(grid.ContentRect.Height - 40) < 2, $"Grid height=40, not inflated by abspos (got {grid.ContentRect.Height})");
        }

        // [CSS-POSITION §5.3] Abspos percentage width resolves against grid container padding box
        [Fact]
        public void AbsposPercentageWidthResolvesAgainstGrid()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;position:relative;width:400px;height:200px;grid-template-columns:1fr 1fr'>
                    <div id='t' style='position:absolute;width:50%;height:30px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 200) < 2, $"50% of 400=200 (got {target.ContentRect.Width})");
        }

        // [CSS-POSITION §5.3] Abspos percentage height resolves against grid container
        [Fact]
        public void AbsposPercentageHeightResolvesAgainstGrid()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;position:relative;width:300px;height:400px;grid-template-columns:1fr'>
                    <div id='t' style='position:absolute;width:50px;height:25%'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Height - 100) < 2, $"25% of 400=100 (got {target.ContentRect.Height})");
        }

        // [CSS-POSITION §5.3] Abspos with inset:0 fills grid container padding box
        [Fact]
        public void AbsposInsetZeroFillsGrid()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;position:relative;width:300px;height:200px;grid-template-columns:1fr'>
                    <div id='t' style='position:absolute;top:0;right:0;bottom:0;left:0'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 300) < 2, $"Width=300 (got {target.ContentRect.Width})");
            Assert.True(System.Math.Abs(target.ContentRect.Height - 200) < 2, $"Height=200 (got {target.ContentRect.Height})");
        }

        // [CSS-POSITION §5.3] Abspos centered in grid via margin:auto + inset:0
        [Fact]
        public void AbsposCenteredInGridWithMarginAuto()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;position:relative;width:300px;height:300px;grid-template-columns:1fr'>
                    <div id='t' style='position:absolute;inset:0;margin:auto;width:100px;height:100px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.X - 100) < 2, $"X centered at 100 (got {target.ContentRect.X})");
            Assert.True(System.Math.Abs(target.ContentRect.Y - 100) < 2, $"Y centered at 100 (got {target.ContentRect.Y})");
        }

        // [CSS-POSITION §5.1] Abspos inside grid item with position:relative uses item as CB
        [Fact]
        public void AbsposInGridItemUsesItemAsContainingBlock()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:200px;width:200px'>
                    <div style='position:relative;height:100px'>
                        <div id='t' style='position:absolute;top:10px;left:10px;width:30px;height:30px'></div>
                    </div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.X - 10) < 2, $"Left=10 relative to item (got {target.ContentRect.X})");
            Assert.True(System.Math.Abs(target.ContentRect.Y - 10) < 2, $"Top=10 relative to item (got {target.ContentRect.Y})");
        }

        // [CSS-POSITION §5.3] Abspos with explicit width and height in grid container
        [Fact]
        public void AbsposExplicitWidthHeightInGrid()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;position:relative;width:400px;height:300px;grid-template-columns:1fr 1fr'>
                    <div id='t' style='position:absolute;top:20px;left:30px;width:150px;height:80px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 150) < 2, $"Width=150 (got {target.ContentRect.Width})");
            Assert.True(System.Math.Abs(target.ContentRect.Height - 80) < 2, $"Height=80 (got {target.ContentRect.Height})");
            Assert.True(System.Math.Abs(target.ContentRect.X - 30) < 2, $"Left=30 (got {target.ContentRect.X})");
            Assert.True(System.Math.Abs(target.ContentRect.Y - 20) < 2, $"Top=20 (got {target.ContentRect.Y})");
        }

        // [CSS-POSITION §5.1] Multiple abspos children in same grid container
        [Fact]
        public void MultipleAbsposInGrid()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;position:relative;width:300px;height:300px;grid-template-columns:1fr'>
                    <div id='a' style='position:absolute;top:0;left:0;width:60px;height:60px'></div>
                    <div id='b' style='position:absolute;top:100px;left:100px;width:60px;height:60px'></div>
                    <div id='c' style='position:absolute;bottom:0;right:0;width:60px;height:60px'></div>
                </div></body>");
            var boxA = LayoutTestHelper.FindById(root, "a")!;
            var boxB = LayoutTestHelper.FindById(root, "b")!;
            var boxC = LayoutTestHelper.FindById(root, "c")!;
            Assert.True(System.Math.Abs(boxA.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(boxA.ContentRect.Y - 0) < 2);
            Assert.True(System.Math.Abs(boxB.ContentRect.X - 100) < 2);
            Assert.True(System.Math.Abs(boxB.ContentRect.Y - 100) < 2);
            Assert.True(System.Math.Abs(boxC.ContentRect.X - 240) < 2, $"Right=0 => X=240 (got {boxC.ContentRect.X})");
            Assert.True(System.Math.Abs(boxC.ContentRect.Y - 240) < 2, $"Bottom=0 => Y=240 (got {boxC.ContentRect.Y})");
        }

        // [CSS-GRID §10.1] Abspos does not interact with grid gap
        [Fact]
        public void AbsposIgnoresGridGap()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;position:relative;width:300px;height:200px;grid-template-columns:1fr 1fr;gap:20px'>
                    <div id='t' style='position:absolute;top:0;left:0;width:50%;height:50%'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 150) < 2, $"50% of 300=150 regardless of gap (got {target.ContentRect.Width})");
            Assert.True(System.Math.Abs(target.ContentRect.Height - 100) < 2, $"50% of 200=100 regardless of gap (got {target.ContentRect.Height})");
        }

        // [CSS-GRID §6.1] Abspos is not a grid item, does not occupy grid tracks
        [Fact]
        public void AbsposNotAGridItem()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='grid' style='display:grid;position:relative;width:200px;grid-template-columns:100px 100px;grid-auto-rows:50px'>
                    <div style='position:absolute;width:300px;height:300px'></div>
                    <div id='item' style='height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(item.ContentRect.X < 2, $"Grid item starts at col 1 (got {item.ContentRect.X})");
        }

        // [CSS-GRID §8.3] Abspos coexists with spanning grid items
        [Fact]
        public void AbsposWithSpanningGridItems()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;position:relative;width:300px;grid-template-columns:100px 100px 100px;grid-auto-rows:40px'>
                    <div id='span' style='grid-column:1/3;height:40px'></div>
                    <div style='height:40px'></div>
                    <div id='abs' style='position:absolute;top:5px;left:5px;width:50px;height:50px'></div>
                </div></body>");
            var spanning = LayoutTestHelper.FindById(root, "span")!;
            var abspos = LayoutTestHelper.FindById(root, "abs")!;
            Assert.True(System.Math.Abs(spanning.ContentRect.Width - 200) < 2, $"Span 2 cols = 200 (got {spanning.ContentRect.Width})");
            Assert.True(System.Math.Abs(abspos.ContentRect.X - 5) < 2, $"Abspos left=5 (got {abspos.ContentRect.X})");
            Assert.True(System.Math.Abs(abspos.ContentRect.Y - 5) < 2, $"Abspos top=5 (got {abspos.ContentRect.Y})");
        }

        // [CSS-GRID §8.5] Abspos inside grid item that uses named area
        [Fact]
        public void AbsposInsideNamedAreaItem()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-areas:""header header"" ""side main"";grid-template-columns:100px 200px;grid-template-rows:50px 150px;width:300px'>
                    <div style='grid-area:main;position:relative'>
                        <div id='t' style='position:absolute;top:10px;right:10px;width:40px;height:40px'></div>
                    </div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 40) < 2, $"Width=40 (got {target.ContentRect.Width})");
            Assert.True(System.Math.Abs(target.ContentRect.Height - 40) < 2, $"Height=40 (got {target.ContentRect.Height})");
        }

        // [CSS-POSITION §5.1] Fixed position in grid uses viewport as CB
        [Fact]
        public void FixedPositionInGrid()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;width:200px;grid-template-columns:1fr'>
                    <div id='t' style='position:fixed;top:5px;left:5px;width:80px;height:80px'></div>
                </div></body>", 400, 300);
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.X - 5) < 2, $"Fixed left=5 (got {target.ContentRect.X})");
            Assert.True(System.Math.Abs(target.ContentRect.Y - 5) < 2, $"Fixed top=5 (got {target.ContentRect.Y})");
            Assert.True(System.Math.Abs(target.ContentRect.Width - 80) < 2, $"Width=80 (got {target.ContentRect.Width})");
        }

        // [CSS-VALUES §8.1] Abspos with calc() width in grid
        [Fact]
        public void AbsposWithCalcInGrid()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;position:relative;width:400px;height:200px;grid-template-columns:1fr'>
                    <div id='t' style='position:absolute;top:0;left:0;width:calc(50% - 20px);height:calc(100% - 40px)'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 180) < 2, $"calc(50%-20px) of 400=180 (got {target.ContentRect.Width})");
            Assert.True(System.Math.Abs(target.ContentRect.Height - 160) < 2, $"calc(100%-40px) of 200=160 (got {target.ContentRect.Height})");
        }

        // [CSS-GRID §6.1] Abspos between normal grid items does not affect item placement
        [Fact]
        public void AbsposBetweenGridItems()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;position:relative;width:300px;grid-template-columns:100px 100px 100px;grid-auto-rows:40px'>
                    <div id='a' style='height:40px'></div>
                    <div style='position:absolute;top:0;left:0;width:20px;height:20px'></div>
                    <div id='b' style='height:40px'></div>
                    <div style='position:absolute;bottom:0;right:0;width:20px;height:20px'></div>
                    <div id='c' style='height:40px'></div>
                </div></body>");
            var boxA = LayoutTestHelper.FindById(root, "a")!;
            var boxB = LayoutTestHelper.FindById(root, "b")!;
            var boxC = LayoutTestHelper.FindById(root, "c")!;
            Assert.True(System.Math.Abs(boxA.ContentRect.X - 0) < 2, $"Item a at col 1 (got {boxA.ContentRect.X})");
            Assert.True(System.Math.Abs(boxB.ContentRect.X - 100) < 2, $"Item b at col 2 (got {boxB.ContentRect.X})");
            Assert.True(System.Math.Abs(boxC.ContentRect.X - 200) < 2, $"Item c at col 3 (got {boxC.ContentRect.X})");
        }

        // [CSS-POSITION §5.3] Abspos with right+bottom insets in grid
        [Fact]
        public void AbsposRightBottomInsetsInGrid()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;position:relative;width:300px;height:200px;grid-template-columns:1fr'>
                    <div id='t' style='position:absolute;right:20px;bottom:30px;width:60px;height:40px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.X - 220) < 2, $"Right=20 => X=220 (got {target.ContentRect.X})");
            Assert.True(System.Math.Abs(target.ContentRect.Y - 130) < 2, $"Bottom=30 => Y=130 (got {target.ContentRect.Y})");
        }

        // [CSS-GRID §6.1] Abspos with grid padding, CB is padding box
        [Fact]
        public void AbsposInGridWithPadding()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;position:relative;width:200px;height:200px;padding:20px;grid-template-columns:1fr'>
                    <div id='t' style='position:absolute;top:0;left:0;right:0;bottom:0'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 240) < 2, $"Fills padding box width 200+40=240 (got {target.ContentRect.Width})");
            Assert.True(System.Math.Abs(target.ContentRect.Height - 240) < 2, $"Fills padding box height 200+40=240 (got {target.ContentRect.Height})");
        }

        // [CSS-POSITION §5.3] Abspos horizontal centering via left:0;right:0;margin:0 auto in grid
        [Fact]
        public void AbsposHorizontalCenterInGrid()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;position:relative;width:400px;height:100px;grid-template-columns:1fr'>
                    <div id='t' style='position:absolute;left:0;right:0;margin:0 auto;width:120px;height:40px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.X - 140) < 2, $"Centered: X=140 (got {target.ContentRect.X})");
        }

        // [CSS-POSITION §5.3] Abspos vertical centering via top:0;bottom:0;margin:auto 0 in grid
        [Fact]
        public void AbsposVerticalCenterInGrid()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;position:relative;width:200px;height:300px;grid-template-columns:1fr'>
                    <div id='t' style='position:absolute;top:0;bottom:0;margin:auto 0;width:60px;height:80px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Y - 110) < 2, $"Centered: Y=110 (got {target.ContentRect.Y})");
        }

        // [CSS-POSITION §5.3] Abspos with left+right auto width stretches in grid
        [Fact]
        public void AbsposLeftRightAutoWidthInGrid()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;position:relative;width:400px;height:100px;grid-template-columns:1fr 1fr'>
                    <div id='t' style='position:absolute;left:30px;right:50px;height:40px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 320) < 2, $"400-30-50=320 (got {target.ContentRect.Width})");
            Assert.True(System.Math.Abs(target.ContentRect.X - 30) < 2, $"Left=30 (got {target.ContentRect.X})");
        }

        // [CSS-POSITION §5.3] Abspos with top+bottom auto height stretches in grid
        [Fact]
        public void AbsposTopBottomAutoHeightInGrid()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;position:relative;width:200px;height:300px;grid-template-columns:1fr'>
                    <div id='t' style='position:absolute;top:40px;bottom:60px;width:50px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Height - 200) < 2, $"300-40-60=200 (got {target.ContentRect.Height})");
            Assert.True(System.Math.Abs(target.ContentRect.Y - 40) < 2, $"Top=40 (got {target.ContentRect.Y})");
        }

        // [CSS-GRID §6.1] Grid items and abspos do not overlap in layout flow
        [Fact]
        public void AbsposDoesNotDisplaceGridItems()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;position:relative;width:200px;grid-template-columns:100px 100px;grid-auto-rows:50px'>
                    <div id='item1' style='height:50px'></div>
                    <div id='item2' style='height:50px'></div>
                    <div style='position:absolute;top:0;left:0;width:200px;height:200px'></div>
                </div></body>");
            var item1 = LayoutTestHelper.FindById(root, "item1")!;
            var item2 = LayoutTestHelper.FindById(root, "item2")!;
            Assert.True(System.Math.Abs(item1.ContentRect.X - 0) < 2, $"Item1 at col 1 (got {item1.ContentRect.X})");
            Assert.True(System.Math.Abs(item2.ContentRect.X - 100) < 2, $"Item2 at col 2 (got {item2.ContentRect.X})");
            Assert.True(System.Math.Abs(item1.ContentRect.Y - 0) < 2, $"Item1 at row 1 (got {item1.ContentRect.Y})");
        }

        // [CSS-GRID §10.1] Abspos percentage with grid gap does not include gap in CB
        [Fact]
        public void AbsposPercentageIgnoresGap()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;position:relative;width:300px;height:200px;grid-template-columns:1fr 1fr;gap:50px'>
                    <div id='t' style='position:absolute;width:100%;height:100%'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 300) < 2, $"100% = 300 (got {target.ContentRect.Width})");
            Assert.True(System.Math.Abs(target.ContentRect.Height - 200) < 2, $"100% = 200 (got {target.ContentRect.Height})");
        }

        // [CSS-POSITION §5.3] Abspos with margin in grid
        [Fact]
        public void AbsposWithMarginInGrid()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;position:relative;width:300px;height:200px;grid-template-columns:1fr'>
                    <div id='t' style='position:absolute;top:0;left:0;margin:15px;width:60px;height:40px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.X - 15) < 2, $"Margin-left=15 (got {target.ContentRect.X})");
            Assert.True(System.Math.Abs(target.ContentRect.Y - 15) < 2, $"Margin-top=15 (got {target.ContentRect.Y})");
        }

        // [CSS-POSITION §5.1] Abspos with auto width shrinks to fit content in grid
        [Fact]
        public void AbsposAutoWidthShrinkToFitInGrid()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;position:relative;width:400px;height:200px;grid-template-columns:1fr'>
                    <div id='t' style='position:absolute;top:0;left:0'>
                        <div style='width:80px;height:20px'></div>
                    </div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(target.ContentRect.Width <= 82, $"Shrink-to-fit width <= 82 (got {target.ContentRect.Width})");
        }

        // [CSS-GRID §6.1] Fixed pos child inside grid item with transform establishes new CB
        [Fact]
        public void FixedPositionPercentInGrid()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;width:200px;grid-template-columns:1fr'>
                    <div id='t' style='position:fixed;top:0;left:0;width:50%;height:50%'></div>
                </div></body>", 400, 300);
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 200) < 2, $"50% of viewport 400=200 (got {target.ContentRect.Width})");
            Assert.True(System.Math.Abs(target.ContentRect.Height - 150) < 2, $"50% of viewport 300=150 (got {target.ContentRect.Height})");
        }

        // [CSS-GRID §8.5] Abspos alongside items in multi-row grid
        [Fact]
        public void AbsposWithMultiRowGrid()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;position:relative;width:200px;grid-template-columns:100px 100px;grid-template-rows:60px 60px;'>
                    <div id='r1c1' style='height:60px'></div>
                    <div id='r1c2' style='height:60px'></div>
                    <div id='r2c1' style='height:60px'></div>
                    <div id='r2c2' style='height:60px'></div>
                    <div id='abs' style='position:absolute;top:10px;left:10px;width:40px;height:40px'></div>
                </div></body>");
            var row2col2 = LayoutTestHelper.FindById(root, "r2c2")!;
            var abspos = LayoutTestHelper.FindById(root, "abs")!;
            Assert.True(System.Math.Abs(row2col2.ContentRect.X - 100) < 2, $"r2c2 at col 2 (got {row2col2.ContentRect.X})");
            Assert.True(System.Math.Abs(row2col2.ContentRect.Y - 60) < 2, $"r2c2 at row 2 (got {row2col2.ContentRect.Y})");
            Assert.True(System.Math.Abs(abspos.ContentRect.X - 10) < 2, $"Abspos left=10 (got {abspos.ContentRect.X})");
            Assert.True(System.Math.Abs(abspos.ContentRect.Y - 10) < 2, $"Abspos top=10 (got {abspos.ContentRect.Y})");
        }

        // [CSS-GRID §6.1] Abspos in grid with border on container
        [Fact]
        public void AbsposInGridWithBorder()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;position:relative;width:200px;height:200px;border:10px solid black;grid-template-columns:1fr'>
                    <div id='t' style='position:absolute;left:0;right:0;top:0;bottom:0'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 200) < 2, $"Fills padding box width=200 (got {target.ContentRect.Width})");
            Assert.True(System.Math.Abs(target.ContentRect.Height - 200) < 2, $"Fills padding box height=200 (got {target.ContentRect.Height})");
        }
    }
}
