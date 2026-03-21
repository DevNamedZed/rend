using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class WptGridFiveColumnTests
    {
        private readonly ITestOutputHelper _output;
        public WptGridFiveColumnTests(ITestOutputHelper output) { _output = output; }

        [Fact] public void FiveCol_Equal_1fr() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:repeat(5,1fr);width:400px'><div id='a' style='height:20px'></div><div id='b' style='height:20px'></div><div id='c' style='height:20px'></div><div id='d' style='height:20px'></div><div id='e' style='height:20px'></div></div></body>", 400, 300);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Width - 80) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"e")!.ContentRect.Width - 80) < 2);
        }

        [Fact] public void FiveCol_Positions() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:repeat(5,80px);width:400px'><div id='a' style='height:20px'></div><div id='b' style='height:20px'></div><div id='c' style='height:20px'></div><div id='d' style='height:20px'></div><div id='e' style='height:20px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.X - 80) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"c")!.ContentRect.X - 160) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"d")!.ContentRect.X - 240) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"e")!.ContentRect.X - 320) < 2);
        }

        [Fact] public void FiveCol_WithGap() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:repeat(5,60px);column-gap:10px;width:340px'><div id='a' style='height:20px'></div><div id='b' style='height:20px'></div><div id='c' style='height:20px'></div><div id='d' style='height:20px'></div><div id='e' style='height:20px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.X - 70) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"e")!.ContentRect.X - 280) < 2);
        }

        [Fact] public void FiveCol_SpanAll() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:repeat(5,80px);width:400px'><div id='t' style='grid-column:1/-1;height:20px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 400) < 2);
        }

        [Fact] public void FiveCol_Span3() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:repeat(5,80px);width:400px'><div id='t' style='grid-column:span 3;height:20px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 240) < 2);
        }

        [Fact] public void FiveCol_TenItems_TwoRows() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:repeat(5,80px);grid-auto-rows:30px;width:400px'><div></div><div></div><div></div><div></div><div></div><div id='f'></div><div></div><div></div><div></div><div id='j'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"f")!.ContentRect.Y - 30) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"j")!.ContentRect.X - 320) < 2);
        }

        [Fact] public void FiveCol_MixedWidths() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:40px 60px 80px 100px 120px;width:400px'><div id='a' style='height:20px'></div><div id='b' style='height:20px'></div><div id='c' style='height:20px'></div><div id='d' style='height:20px'></div><div id='e' style='height:20px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Width - 40) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"e")!.ContentRect.Width - 120) < 2);
        }

        [Fact] public void FiveCol_Fr_1_1_2_1_1() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:1fr 1fr 2fr 1fr 1fr;width:600px'><div id='a' style='height:20px'></div><div id='b' style='height:20px'></div><div id='c' style='height:20px'></div><div id='d' style='height:20px'></div><div id='e' style='height:20px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"c")!.ContentRect.Width - 200) < 2);
        }

        [Fact] public void FiveCol_Percent() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:repeat(5,20%);width:400px'><div id='a' style='height:20px'></div><div id='b' style='height:20px'></div><div id='c' style='height:20px'></div><div id='d' style='height:20px'></div><div id='e' style='height:20px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Width - 80) < 2);
        }

        [Fact] public void FiveCol_FixedFrFixed() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:60px 1fr 1fr 1fr 60px;width:400px'><div id='a' style='height:20px'></div><div id='b' style='height:20px'></div><div id='c' style='height:20px'></div><div id='d' style='height:20px'></div><div id='e' style='height:20px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Width - 60) < 3);
            float frWidth = (400 - 120) / 3f;
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.Width - frWidth) < 2);
        }
    }
}
