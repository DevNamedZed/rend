using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class WptGridRowPositionTests
    {
        private readonly ITestOutputHelper _output;
        public WptGridRowPositionTests(ITestOutputHelper output) { _output = output; }

        [Fact] public void TwoRows_YPositions() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-template-rows:50px 60px;width:200px'><div id='a'></div><div id='b'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Y - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.Y - 50) < 2);
        }

        [Fact] public void ThreeRows_YPositions() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-template-rows:30px 40px 50px;width:200px'><div id='a'></div><div id='b'></div><div id='c'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.Y - 30) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"c")!.ContentRect.Y - 70) < 2);
        }

        [Fact] public void FourRows_YPositions() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-template-rows:repeat(4,40px);width:200px'><div id='a'></div><div id='b'></div><div id='c'></div><div id='d'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"d")!.ContentRect.Y - 120) < 2);
        }

        [Fact] public void Rows_WithGap() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-template-rows:40px 40px;row-gap:20px;width:200px'><div id='a'></div><div id='b'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.Y - 60) < 2);
        }

        [Fact] public void ThreeRows_WithGap() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-template-rows:30px 30px 30px;row-gap:10px;width:200px'><div id='a'></div><div id='b'></div><div id='c'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.Y - 40) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"c")!.ContentRect.Y - 80) < 2);
        }

        [Fact] public void FrRows_Positions() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-template-rows:1fr 1fr;width:200px;height:200px'><div id='a'></div><div id='b'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.Y - 100) < 2);
        }

        [Fact] public void AutoRows_Positions() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-auto-rows:50px;width:200px'><div id='a'></div><div id='b'></div><div id='c'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.Y - 50) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"c")!.ContentRect.Y - 100) < 2);
        }

        [Fact] public void TwoCol_TwoRow_AllPositions() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:100px 100px;grid-template-rows:50px 50px;width:200px'><div id='a'></div><div id='b'></div><div id='c'></div><div id='d'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Y - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.Y - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"c")!.ContentRect.Y - 50) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"d")!.ContentRect.Y - 50) < 2);
        }

        [Fact] public void ThreeCol_TwoRow_SixItems() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:repeat(3,100px);grid-template-rows:40px 40px;width:300px'><div></div><div></div><div></div><div id='d'></div><div id='e'></div><div id='f'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"d")!.ContentRect.Y - 40) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"f")!.ContentRect.Y - 40) < 2);
        }

        [Fact] public void MixedFixedFr_Rows() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-template-rows:50px 1fr;width:200px;height:200px'><div id='a'></div><div id='b'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Height - 50) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.Y - 50) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.Height - 150) < 2);
        }

        [Fact] public void RowSpan_Position() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:100px 100px;grid-template-rows:40px 40px;width:200px'><div id='t' style='grid-row:span 2'></div><div id='b'></div><div id='d'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height - 80) < 2);
        }

        [Fact] public void AutoRow_ContentHeight() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;width:200px'><div style='height:60px'></div><div id='b' style='height:40px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.Y - 60) < 2);
        }

        [Fact] public void GridHeight_FromRows() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='g' style='display:grid;grid-template-columns:200px;grid-template-rows:40px 50px 60px;width:200px'><div></div><div></div><div></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"g")!.ContentRect.Height - 150) < 2);
        }

        [Fact] public void GridHeight_FromRows_WithGap() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='g' style='display:grid;grid-template-columns:200px;grid-template-rows:40px 40px 40px;row-gap:10px;width:200px'><div></div><div></div><div></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"g")!.ContentRect.Height - 140) < 2);
        }
    }
}
