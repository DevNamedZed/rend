using Rend.Css;
using Rend.Style;
using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    /// <summary>
    /// Advanced grid layout tests covering auto-flow, spanning, alignment,
    /// auto-fill/auto-fit, and mixed track sizes.
    /// </summary>
    public class WptGridAdvancedTests
    {
        private readonly ITestOutputHelper _output;
        public WptGridAdvancedTests(ITestOutputHelper output) { _output = output; }

        // [CSS-GRID §8.5] grid-template-areas
        [Fact] public void Grid_Areas_3Column() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='display:grid;grid-template-areas:\"a b c\";grid-template-columns:100px 100px 100px;width:300px'><div id='a' style='grid-area:a;height:20px'></div><div id='b' style='grid-area:b;height:20px'></div><div id='c' style='grid-area:c;height:20px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.X - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"c")!.ContentRect.X - 200) < 2);
        }

        // [CSS-GRID §8.3] grid-column: span 2
        [Fact] public void Grid_ColumnSpan_2() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='display:grid;grid-template-columns:1fr 1fr 1fr;width:300px'><div id='t' style='grid-column:span 2;height:20px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 200) < 2);
        }

        // [CSS-GRID §8.3] grid-row: span 2
        [Fact] public void Grid_RowSpan_2() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='display:grid;grid-template-columns:100px 100px;grid-template-rows:50px 50px;width:200px'><div id='t' style='grid-row:span 2;height:auto'></div><div style='height:50px'></div><div style='height:50px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height >= 99);
        }

        // [CSS-GRID §10.4] justify-items: center on grid
        [Fact] public void Grid_JustifyItems_Center() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='display:grid;grid-template-columns:200px;justify-items:center;width:200px'><div id='t' style='width:50px;height:20px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.X - 75) < 2);
        }

        // [CSS-GRID §10.4] justify-items: end
        [Fact] public void Grid_JustifyItems_End() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='display:grid;grid-template-columns:200px;justify-items:end;width:200px'><div id='t' style='width:50px;height:20px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.X - 150) < 2);
        }

        // [CSS-GRID §7.3] repeat(auto-fill, ...)
        [Fact] public void Grid_AutoFill() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='display:grid;grid-template-columns:repeat(auto-fill,50px);width:200px'><div id='a' style='height:20px'></div><div id='b' style='height:20px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Width - 50) < 2);
        }

        // [CSS-GRID §7.2.1] minmax() with fr
        [Fact] public void Grid_Minmax_WithFr() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='display:grid;grid-template-columns:minmax(100px,1fr) 100px;width:300px'><div id='a' style='height:20px'></div><div id='b' style='height:20px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Width - 200) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.Width - 100) < 2);
        }

        // [CSS-GRID §7.5] grid-auto-rows
        [Fact] public void Grid_AutoRows() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='display:grid;grid-template-columns:100px;grid-auto-rows:40px;width:100px'><div id='a'></div><div id='b'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Height - 40) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.Height - 40) < 2);
        }

        // [CSS-GRID §7.5] grid-auto-columns
        [Fact] public void Grid_AutoColumns() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='display:grid;grid-template-rows:30px;grid-auto-flow:column;grid-auto-columns:60px;width:300px'><div id='a'></div><div id='b'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Width - 60) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.Width - 60) < 2);
        }

        // [CSS-GRID §7.6] grid-auto-flow: dense
        [Fact] public void Grid_Dense_FillsGap() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='display:grid;grid-template-columns:50px 50px 50px;grid-auto-flow:dense;width:150px'><div id='big' style='grid-column:2/4;height:20px'></div><div id='small' style='height:20px'></div></div></body>");
            var small = LayoutTestHelper.FindById(r,"small")!;
            Assert.True(small.ContentRect.X < 2, $"Dense fills gap in col 1 (X={small.ContentRect.X})");
        }

        // [CSS-GRID §8.3] negative line numbers
        [Fact] public void Grid_NegativeLine_SpanAll() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='display:grid;grid-template-columns:1fr 1fr 1fr;width:300px'><div id='t' style='grid-column:1/-1;height:20px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 300) < 2);
        }

        // [CSS-GRID §10.1] gap shorthand row + column
        [Fact] public void Grid_Gap_RowColumn() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='display:grid;grid-template-columns:1fr 1fr;gap:10px 20px;width:220px'><div id='a' style='height:30px'></div><div id='b' style='height:30px'></div><div id='c' style='height:30px'></div><div id='d' style='height:30px'></div></div></body>");
            float colGap = LayoutTestHelper.FindById(r,"b")!.ContentRect.X - (LayoutTestHelper.FindById(r,"a")!.ContentRect.X + LayoutTestHelper.FindById(r,"a")!.ContentRect.Width);
            float rowGap = LayoutTestHelper.FindById(r,"c")!.ContentRect.Y - (LayoutTestHelper.FindById(r,"a")!.ContentRect.Y + LayoutTestHelper.FindById(r,"a")!.ContentRect.Height);
            Assert.True(System.Math.Abs(colGap - 20) < 2);
            Assert.True(System.Math.Abs(rowGap - 10) < 2);
        }

        // [CSS-GRID §7.2] px + fr tracks
        [Fact] public void Grid_MixedTracks_PxAndFr() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='display:grid;grid-template-columns:100px 1fr 100px;width:400px'><div id='a' style='height:20px'></div><div id='b' style='height:20px'></div><div id='c' style='height:20px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.Width - 200) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"c")!.ContentRect.Width - 100) < 2);
        }

        // [CSS-GRID §7.2] percentage tracks
        [Fact] public void Grid_PercentTracks() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='display:grid;grid-template-columns:25% 75%;width:400px'><div id='a' style='height:20px'></div><div id='b' style='height:20px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.Width - 300) < 2);
        }

        // [CSS-GRID §10.5] align-self on grid item
        [Fact] public void Grid_AlignSelf_End() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-template-rows:100px;width:200px'><div id='t' style='align-self:end;height:30px'></div></div></body>");
            var t = LayoutTestHelper.FindById(r,"t")!;
            Assert.True(t.ContentRect.Y >= 69, $"align-self:end (Y={t.ContentRect.Y})");
        }
    }
}
