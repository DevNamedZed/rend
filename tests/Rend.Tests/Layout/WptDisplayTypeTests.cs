using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class WptDisplayTypeTests
    {
        private readonly ITestOutputHelper _output;
        public WptDisplayTypeTests(ITestOutputHelper output) { _output = output; }

        [Fact] public void DisplayBlock_FullWidth() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='display:block;height:30px'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 400) < 2);
        }

        [Fact] public void DisplayNone_NoSpace() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='height:50px'></div><div style='display:none;height:100px'></div><div id='t' style='height:30px'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 50) < 2);
        }

        [Fact] public void DisplayNone_NotInTree() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='display:none;width:100px;height:100px'></div></body>");
            Assert.Null(LayoutTestHelper.FindById(r,"t"));
        }

        [Fact] public void DisplayInlineBlock_Width() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='width:400px'><div id='t' style='display:inline-block;width:100px;height:50px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 100) < 2);
        }

        [Fact] public void DisplayInlineBlock_Height() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='width:400px'><div id='t' style='display:inline-block;width:100px;height:50px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height - 50) < 2);
        }

        [Fact] public void DisplayFlex_FullWidth() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='display:flex;height:30px'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 400) < 2);
        }

        [Fact] public void DisplayInlineFlex_ShrinkToFit() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='width:400px'><div id='t' style='display:inline-flex'><div style='width:80px;height:30px'></div></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 80) < 2);
        }

        [Fact] public void DisplayGrid_FullWidth() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='display:grid;grid-template-columns:1fr;height:30px'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 400) < 2);
        }

        [Fact] public void DisplayInlineGrid_ShrinkToFit() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='width:400px'><div id='t' style='display:inline-grid;grid-template-columns:80px'><div style='height:30px'></div></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 80) < 2);
        }

        [Fact] public void DisplayTable_ShrinkToFit() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='display:table'><div style='display:table-row'><div style='display:table-cell;width:100px;height:30px'></div></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 100) < 2);
        }

        [Fact] public void DisplayTable_FullWidth_Explicit() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='display:table;width:100%'><div style='display:table-row'><div style='display:table-cell;height:30px'></div></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 400) < 2);
        }

        [Fact] public void FlowRoot_ContainsFloats() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='p' style='display:flow-root;width:200px'><div style='float:left;width:100px;height:80px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"p")!.ContentRect.Height >= 79);
        }

        [Fact] public void DisplayBlock_Stacking() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:block;height:40px'></div><div id='t' style='display:block;height:30px'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 40) < 2);
        }

        [Fact] public void DisplayFlex_Children_Row() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:300px'><div id='a' style='width:100px;height:30px'></div><div id='b' style='width:100px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.X - 100) < 2);
        }

        [Fact] public void DisplayGrid_Children_Columns() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:100px 100px;width:200px'><div id='a' style='height:30px'></div><div id='b' style='height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.X - 100) < 2);
        }

        [Fact] public void DisplayInlineBlock_TwoOnLine() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='width:400px'><div id='a' style='display:inline-block;width:100px;height:30px'></div><div id='b' style='display:inline-block;width:100px;height:30px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"a")!.ContentRect.X < LayoutTestHelper.FindById(r,"b")!.ContentRect.X);
        }

        [Fact] public void InlineBlock_InBlock() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='width:200px'><div id='t' style='display:inline-block;width:80px;height:40px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 80) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height - 40) < 2);
        }

        [Fact] public void Flex_InBlock() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='width:300px'><div id='t' style='display:flex'><div style='width:50px;height:20px'></div><div style='width:50px;height:20px'></div></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 300) < 2);
        }

        [Fact] public void Grid_InBlock() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='width:300px'><div id='t' style='display:grid;grid-template-columns:1fr 1fr'><div style='height:20px'></div><div style='height:20px'></div></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 300) < 2);
        }
    }
}
