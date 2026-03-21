using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class WptGridAutoFlowTests
    {
        private readonly ITestOutputHelper _output;
        public WptGridAutoFlowTests(ITestOutputHelper output) { _output = output; }

        [Fact] public void AutoFlow_Row_Default() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:100px 100px;width:200px'><div id='a' style='height:30px'></div><div id='b' style='height:30px'></div><div id='c' style='height:30px'></div><div id='d' style='height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.X - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"c")!.ContentRect.Y - 30) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"d")!.ContentRect.X - 100) < 2);
        }

        [Fact] public void AutoFlow_Column() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-rows:40px 40px;grid-auto-flow:column;grid-auto-columns:80px;width:300px'><div id='a'></div><div id='b'></div><div id='c'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.Y - 40) < 2);
            Assert.True(LayoutTestHelper.FindById(r,"c")!.ContentRect.X >= 79);
        }

        [Fact] public void AutoFlow_Dense() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:50px 50px 50px;grid-auto-flow:dense;width:150px'><div style='grid-column:2/4;height:20px'></div><div id='fill' style='height:20px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"fill")!.ContentRect.X < 2);
        }

        [Fact] public void AutoRows_Sizing() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-auto-rows:60px;width:200px'><div id='a'></div><div id='b'></div><div id='c'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Height - 60) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.Y - 60) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"c")!.ContentRect.Y - 120) < 2);
        }

        [Fact] public void AutoColumns_Sizing() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-rows:50px;grid-auto-flow:column;grid-auto-columns:100px;width:400px'><div id='a'></div><div id='b'></div><div id='c'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.X - 100) < 2);
        }

        [Fact] public void ImplicitRows_Created() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:100px;grid-template-rows:40px;width:100px'><div id='a'></div><div id='b' style='height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Height - 40) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.Y - 40) < 2);
        }

        [Fact] public void ExplicitItem_SkipsPosition() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:100px 100px;width:200px'><div id='placed' style='grid-column:2;height:30px'></div><div id='auto' style='height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"placed")!.ContentRect.X - 100) < 2);
        }

        [Fact] public void AutoRows_MinMax() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-auto-rows:minmax(30px,auto);width:200px'><div id='t' style='height:80px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height >= 79);
        }

        [Fact] public void FiveItems_2Col_3Rows() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:100px 100px;grid-auto-rows:40px;width:200px'><div id='a'></div><div id='b'></div><div id='c'></div><div id='d'></div><div id='e'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"e")!.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"e")!.ContentRect.Y - 80) < 2);
        }

        [Fact] public void Dense_Column_MultipleGaps() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:50px 50px 50px;grid-auto-flow:dense;width:150px'><div style='grid-column:3;height:20px'></div><div id='fill1' style='height:20px'></div><div id='fill2' style='height:20px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"fill1")!.ContentRect.X < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"fill2")!.ContentRect.X - 50) < 2);
        }

        [Fact] public void AutoRows_Default_Auto() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;width:200px'><div id='t' style='height:50px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height - 50) < 2);
        }
    }
}
