using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    /// <summary>
    /// Tests for grid item placement: explicit, auto-flow, spanning, named areas.
    /// </summary>
    public class WptGridPlacementTests
    {
        private readonly ITestOutputHelper _output;
        public WptGridPlacementTests(ITestOutputHelper output) { _output = output; }

        // explicit column placement
        [Fact] public void Col_1() { AssertCol(1, 0); }
        [Fact] public void Col_2() { AssertCol(2, 100); }
        [Fact] public void Col_3() { AssertCol(3, 200); }

        // explicit row placement
        [Fact] public void Row_1() { AssertRow(1, 0); }
        [Fact] public void Row_2() { AssertRow(2, 50); }

        // column span
        [Fact] public void ColSpan_2() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:100px 100px 100px;width:300px'><div id='t' style='grid-column:span 2;height:20px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 200) < 2);
        }

        [Fact] public void ColSpan_3() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:100px 100px 100px;width:300px'><div id='t' style='grid-column:span 3;height:20px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 300) < 2);
        }

        // row span
        [Fact] public void RowSpan_2() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:100px;grid-template-rows:40px 60px;width:100px'><div id='t' style='grid-row:span 2'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height - 100) < 2);
        }

        // negative line: 1/-1 spans all
        [Fact] public void NegativeLine_SpanAll() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:1fr 1fr 1fr;width:300px'><div id='t' style='grid-column:1/-1;height:20px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 300) < 2);
        }

        // grid-column: 2/4
        [Fact] public void Col_2_4() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:repeat(4,50px);width:200px'><div id='t' style='grid-column:2/4;height:20px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.X - 50) < 2);
        }

        // auto-flow: dense
        [Fact] public void Dense_FillsGap() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:50px 50px 50px;grid-auto-flow:dense;width:150px'><div style='grid-column:2/4;height:20px'></div><div id='fill' style='height:20px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"fill")!.ContentRect.X < 2);
        }

        // auto-flow: column
        [Fact] public void AutoFlow_Column() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-rows:30px 30px;grid-auto-flow:column;grid-auto-columns:80px;width:300px'><div id='a'></div><div id='b'></div><div id='c'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.Y - 30) < 2);
            Assert.True(LayoutTestHelper.FindById(r,"c")!.ContentRect.X >= 79);
        }

        // named areas: "h h" "a b"
        [Fact] public void NamedAreas() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-areas:""h h"" ""a b"";grid-template-columns:100px 100px;grid-template-rows:40px 60px;width:200px'><div id='h' style='grid-area:h'></div><div id='a' style='grid-area:a'></div><div id='b' style='grid-area:b'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"h")!.ContentRect.Width - 200) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Y - 40) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.X - 100) < 2);
        }

        // named areas: "h h h" "s m m" "f f f"
        [Fact] public void NamedAreas_3x3() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-areas:""h h h"" ""s m m"" ""f f f"";grid-template-columns:80px 1fr 1fr;grid-template-rows:40px 1fr 30px;width:300px;height:200px'><div id='h' style='grid-area:h'></div><div id='s' style='grid-area:s'></div><div id='m' style='grid-area:m'></div><div id='f' style='grid-area:f'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"h")!.ContentRect.Width - 300) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"s")!.ContentRect.Width - 80) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"f")!.ContentRect.Width - 300) < 2);
        }

        // span with gap
        [Fact] public void SpanWithGap() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:80px 80px 80px;gap:20px;width:280px'><div id='t' style='grid-column:1/3;height:20px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 180) < 2);
        }

        // 2x2 grid all positions
        [Fact] public void Grid2x2() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:100px 100px;grid-template-rows:50px 50px;width:200px'><div id='a'></div><div id='b'></div><div id='c'></div><div id='d'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.X) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.X - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"c")!.ContentRect.Y - 50) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"d")!.ContentRect.X - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"d")!.ContentRect.Y - 50) < 2);
        }

        // 3x3 grid all positions
        [Fact] public void Grid3x3() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:100px 100px 100px;grid-template-rows:40px 40px 40px;width:300px'><div id='a'></div><div id='b'></div><div id='c'></div><div id='d'></div><div id='e'></div><div id='f'></div><div id='g'></div><div id='h'></div><div id='i'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"e")!.ContentRect.X - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"e")!.ContentRect.Y - 40) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"i")!.ContentRect.X - 200) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"i")!.ContentRect.Y - 80) < 2);
        }

        // explicit col+row
        [Fact] public void ExplicitColRow() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:100px 100px;grid-template-rows:50px 50px;width:200px'><div id='t' style='grid-column:2;grid-row:2'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.X - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 50) < 2);
        }

        private void AssertCol(int col, float expectedX) {
            var r = LayoutTestHelper.Layout($@"<body style='margin:0'><div style='display:grid;grid-template-columns:100px 100px 100px;width:300px'><div id='t' style='grid-column:{col};height:20px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.X - expectedX) < 2);
        }

        private void AssertRow(int row, float expectedY) {
            var r = LayoutTestHelper.Layout($@"<body style='margin:0'><div style='display:grid;grid-template-columns:100px;grid-template-rows:50px 50px;width:100px'><div id='t' style='grid-row:{row}'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - expectedY) < 2);
        }
    }
}
