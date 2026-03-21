using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class WptGridNamedLinesTests
    {
        private readonly ITestOutputHelper _output;
        public WptGridNamedLinesTests(ITestOutputHelper output) { _output = output; }

        [Fact] public void NamedArea_Header() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-areas:""h h"" ""a b"";grid-template-columns:100px 200px;grid-template-rows:50px 100px;width:300px'><div id='h' style='grid-area:h'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"h")!.ContentRect.Width - 300) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"h")!.ContentRect.Height - 50) < 2);
        }

        [Fact] public void NamedArea_Sidebar() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-areas:""h h"" ""s m"";grid-template-columns:100px 200px;grid-template-rows:50px 100px;width:300px'><div style='grid-area:h'></div><div id='s' style='grid-area:s'></div><div style='grid-area:m'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"s")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"s")!.ContentRect.Y - 50) < 2);
        }

        [Fact] public void NamedArea_Footer() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-areas:""h h"" ""a b"" ""f f"";grid-template-columns:100px 200px;grid-template-rows:40px 80px 30px;width:300px'><div style='grid-area:h'></div><div style='grid-area:a'></div><div style='grid-area:b'></div><div id='f' style='grid-area:f'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"f")!.ContentRect.Width - 300) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"f")!.ContentRect.Y - 120) < 2);
        }

        [Fact] public void GridColumn_StartEnd() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:100px 100px 100px;width:300px'><div id='t' style='grid-column:2/4;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.X - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 200) < 2);
        }

        [Fact] public void GridRow_StartEnd() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:100px;grid-template-rows:40px 40px 40px;width:100px'><div id='t' style='grid-row:2/4'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 40) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height - 80) < 2);
        }

        [Fact] public void GridColumn_Span2() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:100px 100px 100px;width:300px'><div id='t' style='grid-column:span 2;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 200) < 2);
        }

        [Fact] public void GridRow_Span2() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:100px;grid-template-rows:50px 50px;width:100px'><div id='t' style='grid-row:span 2'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height - 100) < 2);
        }

        [Fact] public void GridColumn_NegativeLine() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:1fr 1fr 1fr;width:300px'><div id='t' style='grid-column:1/-1;height:20px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 300) < 2);
        }

        [Fact] public void ExplicitPlacement_Col2_Row2() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:100px 100px;grid-template-rows:50px 50px;width:200px'><div id='t' style='grid-column:2;grid-row:2'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.X - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 50) < 2);
        }

        [Fact] public void AutoFlow_Row() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:100px 100px;width:200px'><div id='a' style='height:30px'></div><div id='b' style='height:30px'></div><div id='c' style='height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.X - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"c")!.ContentRect.X - 0) < 2);
            Assert.True(LayoutTestHelper.FindById(r,"c")!.ContentRect.Y > 28);
        }

        [Fact] public void Dense_FillsGaps() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:50px 50px 50px;grid-auto-flow:dense;width:150px'><div style='grid-column:2/4;height:20px'></div><div id='fill' style='height:20px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"fill")!.ContentRect.X < 2);
        }

        [Fact] public void ThreeAreas() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-areas:""h h h"" ""s m m"" ""f f f"";grid-template-columns:80px 1fr 1fr;grid-template-rows:40px 1fr 30px;width:300px;height:200px'><div id='h' style='grid-area:h'></div><div id='s' style='grid-area:s'></div><div id='m' style='grid-area:m'></div><div id='f' style='grid-area:f'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"h")!.ContentRect.Width - 300) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"s")!.ContentRect.Width - 80) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"f")!.ContentRect.Width - 300) < 2);
        }

        [Fact] public void SpanWithGap() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:80px 80px 80px;gap:20px;width:280px'><div id='t' style='grid-column:1/3;height:20px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 180) < 2);
        }
    }
}
