using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class WptGridItemPositionTests
    {
        private readonly ITestOutputHelper _output;
        public WptGridItemPositionTests(ITestOutputHelper output) { _output = output; }

        [Fact] public void Item_1_1_AtOrigin() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:100px 100px;grid-template-rows:50px 50px;width:200px'><div id='t'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 0) < 2);
        }

        [Fact] public void Item_2_1_AtX100() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:100px 100px;grid-template-rows:50px;width:200px'><div></div><div id='t'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.X - 100) < 2);
        }

        [Fact] public void Item_1_2_AtY50() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:100px 100px;grid-template-rows:50px 50px;width:200px'><div></div><div></div><div id='t'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 50) < 2);
        }

        [Fact] public void FourItems_2x2() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:100px 100px;grid-template-rows:50px 50px;width:200px'><div id='a'></div><div id='b'></div><div id='c'></div><div id='d'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.X - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"c")!.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"c")!.ContentRect.Y - 50) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"d")!.ContentRect.X - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"d")!.ContentRect.Y - 50) < 2);
        }

        [Fact] public void NineItems_3x3() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:80px 80px 80px;grid-template-rows:40px 40px 40px;width:240px'><div id='a'></div><div></div><div></div><div></div><div id='e'></div><div></div><div></div><div></div><div id='i'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"e")!.ContentRect.X - 80) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"e")!.ContentRect.Y - 40) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"i")!.ContentRect.X - 160) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"i")!.ContentRect.Y - 80) < 2);
        }

        [Fact] public void WithColumnGap() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:100px 100px;column-gap:20px;width:220px'><div id='a'></div><div id='b'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.X - 120) < 2);
        }

        [Fact] public void WithRowGap() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-template-rows:40px 40px;row-gap:15px;width:200px'><div id='a'></div><div id='b'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Y - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.Y - 55) < 2);
        }

        [Fact] public void ExplicitPlacement_Col2Row2() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:100px 100px;grid-template-rows:50px 50px;width:200px'><div id='t' style='grid-column:2;grid-row:2'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.X - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 50) < 2);
        }

        [Fact] public void SpanningItem_FullWidth() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:100px 100px 100px;width:300px'><div id='t' style='grid-column:1/-1;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 300) < 2);
        }

        [Fact] public void FrColumns_Positions() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:1fr 1fr 1fr;width:300px'><div id='a' style='height:20px'></div><div id='b' style='height:20px'></div><div id='c' style='height:20px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.X - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"c")!.ContentRect.X - 200) < 2);
        }

        [Fact] public void FrAndFixed_Positions() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:100px 1fr;width:400px'><div id='a' style='height:20px'></div><div id='b' style='height:20px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.X - 100) < 2);
        }

        [Fact] public void WithGap_ThreeCol() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:80px 80px 80px;column-gap:10px;width:260px'><div id='a' style='height:20px'></div><div id='b' style='height:20px'></div><div id='c' style='height:20px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.X - 90) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"c")!.ContentRect.X - 180) < 2);
        }

        [Fact] public void RowAndCol_3x3_WithGap() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:80px 80px 80px;grid-template-rows:40px 40px 40px;gap:10px;width:260px'><div></div><div></div><div></div><div></div><div id='e'></div><div></div><div></div><div></div><div id='i'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"e")!.ContentRect.X - 90) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"e")!.ContentRect.Y - 50) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"i")!.ContentRect.X - 180) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"i")!.ContentRect.Y - 100) < 2);
        }
    }
}
