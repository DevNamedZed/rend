using Rend.Css;
using Rend.Style;
using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class GridAutoFlowTests
    {
        private readonly ITestOutputHelper _output;
        public GridAutoFlowTests(ITestOutputHelper output) { _output = output; }

        [Fact]
        public void AutoFlow_Dense_FillsGaps()
        {
            // Dense packing should fill gaps left by explicit placement
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display: grid; grid-template-columns: 50px 50px 50px; grid-auto-flow: dense; width: 150px;'>
                    <div id='a' style='grid-column: 2; height: 20px;'></div>
                    <div id='b' style='height: 20px;'></div>
                    <div id='c' style='height: 20px;'></div>
                </div></body>");
            var a = LayoutTestHelper.FindById(root, "a");
            var b = LayoutTestHelper.FindById(root, "b");
            Assert.NotNull(a);
            Assert.NotNull(b);
            _output.WriteLine($"a.X={a!.ContentRect.X} b.X={b!.ContentRect.X}");
            // A is explicitly in column 2. B should fill gap in column 1 (dense).
            Assert.True(a.ContentRect.X >= 49, $"A in column 2 (X={a.ContentRect.X})");
            Assert.True(b.ContentRect.X < 2, $"B should fill column 1 gap with dense (X={b.ContentRect.X})");
        }

        [Fact]
        public void AutoFlow_Row_Default()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display: grid; grid-template-columns: 50px 50px; width: 100px;'>
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
            _output.WriteLine($"a=({a!.ContentRect.X},{a.ContentRect.Y}) b=({b!.ContentRect.X},{b.ContentRect.Y}) c=({c!.ContentRect.X},{c.ContentRect.Y})");
            // Default row flow: a(0,0) b(50,0) c(0,row2)
            Assert.True(b.ContentRect.X > a.ContentRect.X, "B right of A");
            Assert.True(c.ContentRect.Y > a.ContentRect.Y, "C below A (wraps)");
            Assert.True(c.ContentRect.X < 2, "C starts at column 1");
        }

        [Fact]
        public void AutoFlow_Column()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display: grid; grid-template-rows: 30px 30px; grid-auto-flow: column; width: 200px;'>
                    <div id='a' style='width: 50px;'></div>
                    <div id='b' style='width: 50px;'></div>
                    <div id='c' style='width: 50px;'></div>
                </div></body>");
            var a = LayoutTestHelper.FindById(root, "a");
            var b = LayoutTestHelper.FindById(root, "b");
            var c = LayoutTestHelper.FindById(root, "c");
            Assert.NotNull(a);
            Assert.NotNull(b);
            Assert.NotNull(c);
            _output.WriteLine($"a=({a!.ContentRect.X},{a.ContentRect.Y}) b=({b!.ContentRect.X},{b.ContentRect.Y}) c=({c!.ContentRect.X},{c.ContentRect.Y})");
            // Column flow: a(0,0) b(0,30) c(col2,0)
            Assert.True(b.ContentRect.Y > a.ContentRect.Y, "B below A (same column)");
            Assert.True(c.ContentRect.X > a.ContentRect.X, "C in next column");
        }

        [Fact]
        public void Grid_NegativeLineNumber()
        {
            // grid-column: 1 / -1 should span all columns
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display: grid; grid-template-columns: 1fr 1fr 1fr; width: 300px;'>
                    <div id='span' style='grid-column: 1 / -1; height: 30px;'></div>
                </div></body>");
            var span = LayoutTestHelper.FindById(root, "span");
            Assert.NotNull(span);
            _output.WriteLine($"span: w={span!.ContentRect.Width}");
            Assert.True(System.Math.Abs(span.ContentRect.Width - 300) < 2,
                $"grid-column: 1/-1 should span all (got {span.ContentRect.Width})");
        }

        [Fact]
        public void Grid_TemplateAreas_WithEmptyCell()
        {
            // "." marks an empty cell
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display: grid; grid-template-areas: ""a . b""; grid-template-columns: 1fr 1fr 1fr; width: 300px;'>
                    <div id='a' style='grid-area: a; height: 30px;'></div>
                    <div id='b' style='grid-area: b; height: 30px;'></div>
                </div></body>");
            var a = LayoutTestHelper.FindById(root, "a");
            var b = LayoutTestHelper.FindById(root, "b");
            Assert.NotNull(a);
            Assert.NotNull(b);
            _output.WriteLine($"a.X={a!.ContentRect.X} b.X={b!.ContentRect.X}");
            // a in column 1, b in column 3 (skipping empty column 2)
            Assert.True(a.ContentRect.X < 2, $"a in column 1 (X={a.ContentRect.X})");
            Assert.True(b.ContentRect.X >= 199, $"b in column 3 (X={b.ContentRect.X})");
        }
    }
}
