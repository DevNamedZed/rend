using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class WptGridAutoFlowColumnTests
    {
        private readonly ITestOutputHelper _output;
        public WptGridAutoFlowColumnTests(ITestOutputHelper output) { _output = output; }

        [Fact] public void ColumnFlow_FillsTopToBottom() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-rows:40px 40px;grid-auto-flow:column;grid-auto-columns:80px;width:300px'><div id='a'></div><div id='b'></div><div id='c'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Y - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.Y - 40) < 2);
            Assert.True(LayoutTestHelper.FindById(r,"c")!.ContentRect.X >= 79);
        }

        [Fact] public void ColumnFlow_SecondColumnX() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-rows:40px 40px;grid-auto-flow:column;grid-auto-columns:100px;width:300px'><div id='a'></div><div id='b'></div><div id='c'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"c")!.ContentRect.X - 100) < 2);
        }

        [Fact] public void ColumnFlow_ThreeRows_SixItems() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-rows:30px 30px 30px;grid-auto-flow:column;grid-auto-columns:80px;width:300px'><div id='a'></div><div id='b'></div><div id='c'></div><div id='d'></div><div id='e'></div><div id='f'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"d")!.ContentRect.X - 80) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"d")!.ContentRect.Y - 0) < 2);
        }

        [Fact] public void ColumnFlow_AutoColumnWidth() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-rows:50px;grid-auto-flow:column;grid-auto-columns:120px;width:400px'><div id='a'></div><div id='b'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Width - 120) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.X - 120) < 2);
        }

        [Fact] public void ColumnFlow_WithGap() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-rows:40px 40px;grid-auto-flow:column;grid-auto-columns:80px;column-gap:10px;width:300px'><div id='a'></div><div id='b'></div><div id='c'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"c")!.ContentRect.X - 90) < 2);
        }

        [Fact] public void ColumnFlow_WithRowGap() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-rows:40px 40px;grid-auto-flow:column;grid-auto-columns:80px;row-gap:10px;width:300px'><div id='a'></div><div id='b'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.Y - 50) < 2);
        }

        [Fact] public void ColumnFlow_FourItems_TwoByTwo() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-rows:50px 50px;grid-auto-flow:column;grid-auto-columns:100px;width:300px'><div id='a'></div><div id='b'></div><div id='c'></div><div id='d'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.Y - 50) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"c")!.ContentRect.X - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"d")!.ContentRect.X - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"d")!.ContentRect.Y - 50) < 2);
        }

        [Fact] public void ColumnFlow_SingleRow() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-rows:50px;grid-auto-flow:column;grid-auto-columns:80px;width:400px'><div id='a'></div><div id='b'></div><div id='c'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.X - 80) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"c")!.ContentRect.X - 160) < 2);
        }

        [Fact] public void ColumnFlow_RowHeight() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-rows:60px 40px;grid-auto-flow:column;grid-auto-columns:100px;width:300px'><div id='a'></div><div id='b'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Height - 60) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.Height - 40) < 2);
        }

        [Fact] public void ColumnFlow_FiveItems_ThreeRows() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-rows:30px 30px 30px;grid-auto-flow:column;grid-auto-columns:80px;width:300px'><div id='a'></div><div id='b'></div><div id='c'></div><div id='d'></div><div id='e'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"d")!.ContentRect.X - 80) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"e")!.ContentRect.X - 80) < 2);
        }
    }
}
