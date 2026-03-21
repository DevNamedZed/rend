using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class WptGridItemPlacementTests
    {
        private readonly ITestOutputHelper _output;
        public WptGridItemPlacementTests(ITestOutputHelper output) { _output = output; }

        // explicit column + row placement
        [Fact]
        public void ExplicitPlacement_Col2Row2()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px 100px;grid-template-rows:50px 50px;width:200px'>
                    <div id='t' style='grid-column:2;grid-row:2'></div>
                </div></body>");
            var t = LayoutTestHelper.FindById(r, "t")!;
            Assert.True(System.Math.Abs(t.ContentRect.X - 100) < 2);
            Assert.True(System.Math.Abs(t.ContentRect.Y - 50) < 2);
        }

        // span 2 columns
        [Fact]
        public void Span2Columns()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px 100px 100px;width:300px'>
                    <div id='t' style='grid-column:span 2;height:30px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Width - 200) < 2);
        }

        // span 2 rows
        [Fact]
        public void Span2Rows()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;grid-template-rows:40px 60px;width:100px'>
                    <div id='t' style='grid-row:span 2'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Height - 100) < 2);
        }

        // grid-column: 1 / -1 spans all
        [Fact]
        public void SpanAll_NegativeLine()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:repeat(5,40px);width:200px'>
                    <div id='t' style='grid-column:1/-1;height:20px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Width - 200) < 2);
        }

        // auto-flow: dense packs items
        [Fact]
        public void Dense_PacksIntoGaps()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:50px 50px 50px;grid-auto-flow:dense;width:150px'>
                    <div style='grid-column:2/4;height:20px'></div>
                    <div id='small' style='height:20px'></div>
                </div></body>");
            Assert.True(LayoutTestHelper.FindById(r, "small")!.ContentRect.X < 2);
        }

        // auto-flow: column fills by column
        [Fact]
        public void AutoFlowColumn()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-rows:30px 30px;grid-auto-flow:column;grid-auto-columns:80px;width:300px'>
                    <div id='a'></div>
                    <div id='b'></div>
                    <div id='c'></div>
                </div></body>");
            var b = LayoutTestHelper.FindById(r, "b")!;
            var c = LayoutTestHelper.FindById(r, "c")!;
            Assert.True(System.Math.Abs(b.ContentRect.Y - 30) < 2, $"b row 2 (Y={b.ContentRect.Y})");
            Assert.True(c.ContentRect.X >= 79, $"c col 2 (X={c.ContentRect.X})");
        }

        // named areas: header spans 2 cols
        [Fact]
        public void NamedAreas_HeaderSpans()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-areas:""h h"" ""a b"";grid-template-columns:100px 100px;grid-template-rows:40px 60px;width:200px'>
                    <div id='h' style='grid-area:h'></div>
                    <div id='a' style='grid-area:a'></div>
                    <div id='b' style='grid-area:b'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "h")!.ContentRect.Width - 200) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "a")!.ContentRect.Y - 40) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "b")!.ContentRect.X - 100) < 2);
        }

        // grid item margin:auto centers
        [Fact]
        public void MarginAuto_CentersInCell()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:200px;grid-template-rows:100px;width:200px'>
                    <div id='t' style='width:80px;height:40px;margin:auto'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.X - 60) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Y - 30) < 2);
        }

        // align-self: end
        [Fact]
        public void AlignSelf_End()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:200px;grid-template-rows:100px;width:200px'>
                    <div id='t' style='align-self:end;height:30px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Y - 70) < 2);
        }

        // justify-self: end
        [Fact]
        public void JustifySelf_End()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:200px;width:200px'>
                    <div id='t' style='justify-self:end;width:60px;height:30px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.X - 140) < 2);
        }

        // align-self: center
        [Fact]
        public void AlignSelf_Center()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:200px;grid-template-rows:100px;width:200px'>
                    <div id='t' style='align-self:center;height:40px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Y - 30) < 2);
        }

        // justify-self: center
        [Fact]
        public void JustifySelf_Center()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:200px;width:200px'>
                    <div id='t' style='justify-self:center;width:80px;height:30px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.X - 60) < 2);
        }

        // grid-auto-rows applies to implicit rows
        [Fact]
        public void AutoRows_ImplicitTracks()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;grid-auto-rows:50px;width:100px'>
                    <div id='a'></div>
                    <div id='b'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "a")!.ContentRect.Height - 50) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "b")!.ContentRect.Height - 50) < 2);
        }

        // span with gap: width includes inter-track gaps
        [Fact]
        public void Span_WithGap_IncludesGaps()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:60px 60px 60px;gap:20px;width:220px'>
                    <div id='t' style='grid-column:1/3;height:20px'></div>
                </div></body>");
            // 60+20+60 = 140
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Width - 140) < 2);
        }

        // 4 items in 2x2 grid, verify all positions
        [Fact]
        public void Grid2x2_AllPositions()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px 100px;grid-template-rows:50px 50px;width:200px'>
                    <div id='a'></div><div id='b'></div><div id='c'></div><div id='d'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "a")!.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "a")!.ContentRect.Y - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "b")!.ContentRect.X - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "b")!.ContentRect.Y - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "c")!.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "c")!.ContentRect.Y - 50) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "d")!.ContentRect.X - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "d")!.ContentRect.Y - 50) < 2);
        }
    }
}
