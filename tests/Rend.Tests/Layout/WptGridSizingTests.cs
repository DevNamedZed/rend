using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    /// <summary>
    /// Grid track sizing, item placement, alignment, and spanning tests.
    /// </summary>
    public class WptGridSizingTests
    {
        private readonly ITestOutputHelper _output;
        public WptGridSizingTests(ITestOutputHelper output) { _output = output; }

        // [CSS-GRID §7.2] 2fr + 1fr = 2:1 ratio
        [Fact]
        public void FrTracks_2to1_Ratio()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:2fr 1fr;width:300px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "a")!.ContentRect.Width - 200) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "b")!.ContentRect.Width - 100) < 2);
        }

        // [CSS-GRID §7.2] 100px + 1fr + 100px in 400px = 100, 200, 100
        [Fact]
        public void MixedTracks_FixedFrFixed()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px 1fr 100px;width:400px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                    <div id='c' style='height:20px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "a")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "b")!.ContentRect.Width - 200) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "c")!.ContentRect.Width - 100) < 2);
        }

        // [CSS-GRID §7.2] percentage tracks
        [Fact]
        public void PercentTracks_25_75()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:25% 75%;width:400px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "a")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "b")!.ContentRect.Width - 300) < 2);
        }

        // [CSS-GRID §7.3] repeat(auto-fill, 100px) in 350px = 3 columns
        [Fact]
        public void AutoFill_100px_In350()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:repeat(auto-fill,100px);width:350px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                    <div id='c' style='height:20px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "a")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "c")!.ContentRect.X - 200) < 2);
        }

        // [CSS-GRID §10.1] row-gap and column-gap separate
        [Fact]
        public void RowGap_ColumnGap_Different()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:1fr 1fr;row-gap:10px;column-gap:30px;width:230px'>
                    <div id='a' style='height:40px'></div>
                    <div id='b' style='height:40px'></div>
                    <div id='c' style='height:40px'></div>
                </div></body>");
            float colGap = LayoutTestHelper.FindById(r, "b")!.ContentRect.X - (LayoutTestHelper.FindById(r, "a")!.ContentRect.X + LayoutTestHelper.FindById(r, "a")!.ContentRect.Width);
            float rowGap = LayoutTestHelper.FindById(r, "c")!.ContentRect.Y - (LayoutTestHelper.FindById(r, "a")!.ContentRect.Y + LayoutTestHelper.FindById(r, "a")!.ContentRect.Height);
            Assert.True(System.Math.Abs(colGap - 30) < 2, $"col-gap=30 (got {colGap})");
            Assert.True(System.Math.Abs(rowGap - 10) < 2, $"row-gap=10 (got {rowGap})");
        }

        // [CSS-GRID §8.3] grid-column: 1 / -1 spans all explicit columns
        [Fact]
        public void SpanAll_WithNegativeLine()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:repeat(4,50px);width:200px'>
                    <div id='t' style='grid-column:1/-1;height:20px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Width - 200) < 2);
        }

        // [CSS-GRID §8.3] grid-column: span 2 with gap
        [Fact]
        public void Span2_WithGap()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:repeat(3,60px);gap:20px;width:220px'>
                    <div id='t' style='grid-column:span 2;height:20px'></div>
                </div></body>");
            // 2 cols: 60+20+60 = 140
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Width - 140) < 2);
        }

        // [CSS-GRID §10.4] align-items: end aligns to bottom of row
        [Fact]
        public void AlignItems_End_BottomOfRow()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:200px;grid-template-rows:100px;align-items:end;width:200px'>
                    <div id='t' style='height:30px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Y - 70) < 2);
        }

        // [CSS-GRID §10.4] justify-items: center centers horizontally
        [Fact]
        public void JustifyItems_Center()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:200px;justify-items:center;width:200px'>
                    <div id='t' style='width:60px;height:20px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.X - 70) < 2);
        }

        // [CSS-GRID §10.3] margin:auto centers grid item in cell
        [Fact]
        public void MarginAuto_CentersInCell()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:200px;grid-template-rows:100px;width:200px'>
                    <div id='t' style='width:60px;height:40px;margin:auto'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.X - 70) < 2, $"X centered (got {LayoutTestHelper.FindById(r, "t")!.ContentRect.X})");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Y - 30) < 2, $"Y centered (got {LayoutTestHelper.FindById(r, "t")!.ContentRect.Y})");
        }

        // [CSS-GRID §7.5] grid-auto-rows with explicit value
        [Fact]
        public void AutoRows_ExplicitSize()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;grid-auto-rows:50px;width:100px'>
                    <div id='a'></div>
                    <div id='b'></div>
                    <div id='c'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "a")!.ContentRect.Height - 50) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "b")!.ContentRect.Y - 50) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "c")!.ContentRect.Y - 100) < 2);
        }

        // [CSS-GRID §8.5] grid-template-areas with 2x2 layout
        [Fact]
        public void TemplateAreas_2x2()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-areas:""a b"" ""c d"";grid-template-columns:150px 150px;grid-template-rows:50px 50px;width:300px'>
                    <div id='a' style='grid-area:a'></div>
                    <div id='d' style='grid-area:d'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "a")!.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "a")!.ContentRect.Y - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "d")!.ContentRect.X - 150) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "d")!.ContentRect.Y - 50) < 2);
        }

        // [CSS-GRID §7.6] grid-auto-flow: dense fills gaps
        [Fact]
        public void AutoFlowDense_FillsGap()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:50px 50px 50px;grid-auto-flow:dense;width:150px'>
                    <div style='grid-column:2/4;height:20px'></div>
                    <div id='fill' style='height:20px'></div>
                </div></body>");
            // Dense: fill should go to column 1 (gap left by spanning item)
            Assert.True(LayoutTestHelper.FindById(r, "fill")!.ContentRect.X < 2);
        }

        // [CSS-GRID §10.5] align-self: center on grid item
        [Fact]
        public void AlignSelf_Center_GridItem()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:200px;grid-template-rows:100px;width:200px'>
                    <div id='t' style='align-self:center;height:40px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Y - 30) < 2);
        }
    }
}
