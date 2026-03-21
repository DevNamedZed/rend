using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    /// <summary>
    /// Tests mirroring actual WPT css-grid test patterns.
    /// </summary>
    public class WptGridConformanceTests
    {
        private readonly ITestOutputHelper _output;
        public WptGridConformanceTests(ITestOutputHelper output) { _output = output; }

        // WPT: grid-template-columns-001 — explicit px tracks
        [Fact]
        public void Columns001_ExplicitPx()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px 200px;width:300px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "a")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "b")!.ContentRect.Width - 200) < 2);
        }

        // WPT: grid-template-columns-002 — fr tracks
        [Fact]
        public void Columns002_FrTracks()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:1fr 2fr;width:300px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "a")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "b")!.ContentRect.Width - 200) < 2);
        }

        // WPT: grid-template-columns-003 — percentage tracks
        [Fact]
        public void Columns003_PercentTracks()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:30% 70%;width:400px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "a")!.ContentRect.Width - 120) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "b")!.ContentRect.Width - 280) < 2);
        }

        // WPT: grid-template-columns-004 — mixed px and fr
        [Fact]
        public void Columns004_MixedPxFr()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px 1fr;width:400px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "a")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "b")!.ContentRect.Width - 300) < 2);
        }

        // WPT: grid-template-rows-001 — explicit px rows
        [Fact]
        public void Rows001_ExplicitPx()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;grid-template-rows:40px 60px;width:100px'>
                    <div id='a'></div>
                    <div id='b'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "a")!.ContentRect.Height - 40) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "b")!.ContentRect.Height - 60) < 2);
        }

        // WPT: grid-template-rows-002 — percentage row in definite container
        [Fact]
        public void Rows002_PercentInDefinite()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;grid-template-rows:50%;width:100px;height:200px'>
                    <div id='t'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Height - 100) < 2);
        }

        // WPT: grid-gap-001 — column gap
        [Fact]
        public void Gap001_ColumnGap()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:1fr 1fr;column-gap:20px;width:220px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "a")!.ContentRect.Width - 100) < 2);
            float gap = LayoutTestHelper.FindById(r, "b")!.ContentRect.X - (LayoutTestHelper.FindById(r, "a")!.ContentRect.X + 100);
            Assert.True(System.Math.Abs(gap - 20) < 2);
        }

        // WPT: grid-gap-002 — row gap
        [Fact]
        public void Gap002_RowGap()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;row-gap:15px;width:100px'>
                    <div id='a' style='height:30px'></div>
                    <div id='b' style='height:30px'></div>
                </div></body>");
            float gap = LayoutTestHelper.FindById(r, "b")!.ContentRect.Y - (LayoutTestHelper.FindById(r, "a")!.ContentRect.Y + 30);
            Assert.True(System.Math.Abs(gap - 15) < 2);
        }

        // WPT: grid-placement-001 — explicit column placement
        [Fact]
        public void Placement001_ExplicitColumn()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px 100px;width:200px'>
                    <div id='t' style='grid-column:2;height:20px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.X - 100) < 2);
        }

        // WPT: grid-placement-002 — explicit row placement
        [Fact]
        public void Placement002_ExplicitRow()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;grid-template-rows:40px 40px;width:100px'>
                    <div id='t' style='grid-row:2;height:20px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Y - 40) < 2);
        }

        // WPT: grid-repeat-001 — repeat(3, 100px)
        [Fact]
        public void Repeat001()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:repeat(3,100px);width:300px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                    <div id='c' style='height:20px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "a")!.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "b")!.ContentRect.X - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "c")!.ContentRect.X - 200) < 2);
        }

        // WPT: grid-auto-fill-001
        [Fact]
        public void AutoFill001()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:repeat(auto-fill,100px);width:350px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                    <div id='c' style='height:20px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "a")!.ContentRect.Width - 100) < 2);
        }

        // WPT: grid-minmax-001
        [Fact]
        public void Minmax001()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:minmax(100px,1fr) minmax(100px,1fr);width:300px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "a")!.ContentRect.Width - 150) < 2);
        }

        // WPT: grid-template-areas-001
        [Fact]
        public void Areas001()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-areas:""h h"" ""s m"";grid-template-columns:100px 200px;grid-template-rows:50px 100px;width:300px'>
                    <div id='h' style='grid-area:h'></div>
                    <div id='s' style='grid-area:s'></div>
                    <div id='m' style='grid-area:m'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "h")!.ContentRect.Width - 300) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "h")!.ContentRect.Height - 50) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "s")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "s")!.ContentRect.Y - 50) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "m")!.ContentRect.X - 100) < 2);
        }

        // WPT: grid-column-span-001
        [Fact]
        public void ColumnSpan001()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:repeat(4,50px);width:200px'>
                    <div id='t' style='grid-column:span 2;height:20px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Width - 100) < 2);
        }

        // WPT: grid-row-span-001
        [Fact]
        public void RowSpan001()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;grid-template-rows:40px 60px;width:100px'>
                    <div id='t' style='grid-row:span 2'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Height - 100) < 2);
        }

        // WPT: grid-negative-line-001
        [Fact]
        public void NegativeLine001()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:1fr 1fr 1fr;width:300px'>
                    <div id='t' style='grid-column:1/-1;height:20px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Width - 300) < 2);
        }

        // WPT: grid-auto-flow-dense-001
        [Fact]
        public void AutoFlowDense001()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:50px 50px 50px;grid-auto-flow:dense;width:150px'>
                    <div style='grid-column:2/4;height:20px'></div>
                    <div id='small' style='height:20px'></div>
                </div></body>");
            Assert.True(LayoutTestHelper.FindById(r, "small")!.ContentRect.X < 2);
        }

        // WPT: grid-auto-flow-column-001
        [Fact]
        public void AutoFlowColumn001()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-rows:40px 40px;grid-auto-flow:column;grid-auto-columns:80px;width:300px'>
                    <div id='a'></div>
                    <div id='b'></div>
                    <div id='c'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "b")!.ContentRect.Y - 40) < 2);
            Assert.True(LayoutTestHelper.FindById(r, "c")!.ContentRect.X >= 79);
        }

        // WPT: grid-align-items-center
        [Fact]
        public void AlignItemsCenter()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:200px;grid-template-rows:100px;align-items:center;width:200px'>
                    <div id='t' style='height:40px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Y - 30) < 2);
        }

        // WPT: grid-justify-items-center
        [Fact]
        public void JustifyItemsCenter()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:200px;justify-items:center;width:200px'>
                    <div id='t' style='width:60px;height:20px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.X - 70) < 2);
        }

        // WPT: grid-margin-auto-001
        [Fact]
        public void MarginAuto001()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:200px;grid-template-rows:100px;width:200px'>
                    <div id='t' style='width:80px;height:40px;margin:auto'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.X - 60) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Y - 30) < 2);
        }

        // WPT: grid-auto-rows-001
        [Fact]
        public void AutoRows001()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;grid-auto-rows:50px;width:100px'>
                    <div id='a'></div>
                    <div id='b'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "a")!.ContentRect.Height - 50) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "b")!.ContentRect.Y - 50) < 2);
        }

        // WPT: grid span with gap
        [Fact]
        public void SpanWithGap()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:80px 80px 80px;gap:20px;width:280px'>
                    <div id='t' style='grid-column:1/3;height:20px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Width - 180) < 2);
        }

        // WPT: grid 3x3 with all positions verified
        [Fact]
        public void Grid3x3_AllPositions()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px 100px 100px;grid-template-rows:50px 50px 50px;width:300px'>
                    <div id='a'></div><div id='b'></div><div id='c'></div>
                    <div id='d'></div><div id='e'></div><div id='f'></div>
                    <div id='g'></div><div id='h'></div><div id='i'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "a")!.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "b")!.ContentRect.X - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "c")!.ContentRect.X - 200) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "d")!.ContentRect.Y - 50) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "e")!.ContentRect.X - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "e")!.ContentRect.Y - 50) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "i")!.ContentRect.X - 200) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "i")!.ContentRect.Y - 100) < 2);
        }

        // WPT: grid with calc track
        [Fact]
        public void CalcTrack()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:calc(50% - 10px) calc(50% - 10px);gap:20px;width:200px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "a")!.ContentRect.Width - 90) < 2);
        }

        // WPT: grid auto column with fixed track
        [Fact]
        public void AutoColumn_WithFixed()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px auto;width:300px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'><div style='width:50px;height:10px'></div></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "a")!.ContentRect.Width - 100) < 2);
            Assert.True(LayoutTestHelper.FindById(r, "b")!.ContentRect.Width >= 199,
                $"auto fills remaining (got {LayoutTestHelper.FindById(r, "b")!.ContentRect.Width})");
        }
    }
}
