using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class GridEdgeCaseTests
    {
        private readonly ITestOutputHelper _output;
        public GridEdgeCaseTests(ITestOutputHelper output) { _output = output; }

        [Fact]
        public void Grid_Repeat_FixedTracks()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display: grid; grid-template-columns: repeat(3, 50px); width: 200px;'>
                    <div id='a' style='height: 20px;'></div>
                    <div id='b' style='height: 20px;'></div>
                    <div id='c' style='height: 20px;'></div>
                </div></body>");
            var a = LayoutTestHelper.FindById(root, "a");
            var b = LayoutTestHelper.FindById(root, "b");
            var c = LayoutTestHelper.FindById(root, "c");
            Assert.NotNull(a);
            Assert.NotNull(b);
            Assert.NotNull(c);
            _output.WriteLine($"a.X={a!.ContentRect.X} b.X={b!.ContentRect.X} c.X={c!.ContentRect.X}");
            Assert.True(System.Math.Abs(a.ContentRect.Width - 50) < 2, $"repeat(3, 50px) (got {a.ContentRect.Width})");
            Assert.True(b.ContentRect.X > a.ContentRect.X, "B after A");
            Assert.True(c.ContentRect.X > b.ContentRect.X, "C after B");
        }

        [Fact]
        public void Grid_NamedAreas()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display: grid; grid-template-areas: ""header header"" ""nav main"";
                     grid-template-rows: 40px 60px; grid-template-columns: 80px 1fr; width: 200px;'>
                    <div id='header' style='grid-area: header;'></div>
                    <div id='nav' style='grid-area: nav;'></div>
                    <div id='main' style='grid-area: main;'></div>
                </div></body>");
            var header = LayoutTestHelper.FindById(root, "header");
            var nav = LayoutTestHelper.FindById(root, "nav");
            var main = LayoutTestHelper.FindById(root, "main");
            Assert.NotNull(header);
            Assert.NotNull(nav);
            Assert.NotNull(main);
            _output.WriteLine($"header: {header!.ContentRect.Width}x{header.ContentRect.Height}");
            _output.WriteLine($"nav: {nav!.ContentRect.Width}x{nav.ContentRect.Height} at ({nav.ContentRect.X},{nav.ContentRect.Y})");
            _output.WriteLine($"main: {main!.ContentRect.Width}x{main.ContentRect.Height} at ({main.ContentRect.X},{main.ContentRect.Y})");
            // Header should span 2 columns = full width
            Assert.True(header.ContentRect.Width >= 199,
                $"Header should span 2 columns (got {header.ContentRect.Width})");
            // Nav should be 80px wide in column 1
            Assert.True(System.Math.Abs(nav.ContentRect.Width - 80) < 2,
                $"Nav should be 80px (got {nav.ContentRect.Width})");
        }

        [Fact]
        public void Grid_JustifyItems_Center()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display: grid; grid-template-columns: 200px; justify-items: center; width: 200px;'>
                    <div id='item' style='width: 50px; height: 30px;'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item");
            Assert.NotNull(item);
            _output.WriteLine($"item.X={item!.ContentRect.X} w={item.ContentRect.Width}");
            // Centered in 200px column: (200-50)/2 = 75
            Assert.True(item.ContentRect.X >= 74 && item.ContentRect.X <= 76,
                $"justify-items:center (got X={item.ContentRect.X})");
        }

        [Fact]
        public void Grid_Colspan_SpanMultiple()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display: grid; grid-template-columns: 1fr 1fr 1fr; width: 300px;'>
                    <div id='span2' style='grid-column: span 2; height: 30px;'></div>
                    <div style='height: 30px;'></div>
                </div></body>");
            var span2 = LayoutTestHelper.FindById(root, "span2");
            Assert.NotNull(span2);
            _output.WriteLine($"span2: w={span2!.ContentRect.Width}");
            // Spans 2 of 3 equal columns = 200px
            Assert.True(System.Math.Abs(span2.ContentRect.Width - 200) < 2,
                $"grid-column:span 2 should be 200px (got {span2.ContentRect.Width})");
        }

        [Fact]
        public void Grid_AutoFill_RepeatTracks()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display: grid; grid-template-columns: repeat(auto-fill, 50px); width: 200px;'>
                    <div id='a' style='height: 20px;'></div>
                    <div id='b' style='height: 20px;'></div>
                    <div id='c' style='height: 20px;'></div>
                </div></body>");
            var a = LayoutTestHelper.FindById(root, "a");
            Assert.NotNull(a);
            _output.WriteLine($"a.width={a!.ContentRect.Width}");
            // auto-fill with 50px in 200px → 4 columns
            Assert.True(System.Math.Abs(a.ContentRect.Width - 50) < 2,
                $"auto-fill 50px columns (got {a.ContentRect.Width})");
        }

        // [CSS-SIZING-3 §5.2.2] Percentage heights on grid items resolve against
        // the grid area size. When the row is auto-sized and the inline-grid has
        // no definite container height, the row must first be sized by content
        // contributions from sibling items; only then can the percent-height item
        // resolve against the now-definite row size.
        [Fact]
        public void Grid_PercentHeight_AutoRow_ResolvesAgainstContentSizedRow()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display:inline-grid;grid-template:auto/50px 50px;'>
                    <div id='pct' style='width:100%;height:100%;background:green'></div>
                    <div id='fixed' style='height:100px;background:green'><br></div>
                </div></body>");
            var percentItem = LayoutTestHelper.FindById(root, "pct");
            var fixedItem = LayoutTestHelper.FindById(root, "fixed");
            Assert.NotNull(percentItem);
            Assert.NotNull(fixedItem);
            _output.WriteLine($"pct: {percentItem!.ContentRect.Width}x{percentItem.ContentRect.Height}");
            _output.WriteLine($"fixed: {fixedItem!.ContentRect.Width}x{fixedItem.ContentRect.Height}");
            // Row is sized by fixed item to 100px; percent item should fill 100x50
            Assert.True(System.Math.Abs(percentItem.ContentRect.Height - 100) < 2,
                $"Expected percent item height 100 (row sized by sibling), got {percentItem.ContentRect.Height}");
            Assert.True(System.Math.Abs(percentItem.ContentRect.Width - 50) < 2,
                $"Expected percent item width 50 (column track), got {percentItem.ContentRect.Width}");
        }
    }
}
