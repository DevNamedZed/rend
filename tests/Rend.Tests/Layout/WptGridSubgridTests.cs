using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class WptGridSubgridTests
    {
        private readonly ITestOutputHelper _output;
        public WptGridSubgridTests(ITestOutputHelper output) { _output = output; }

        [Fact] public void Grid_InGrid_InheritsWidth() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;width:200px'><div style='display:grid;grid-template-columns:1fr 1fr'><div id='a' style='height:20px'></div><div id='b' style='height:20px'></div></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.Width - 100) < 2);
        }

        [Fact] public void Grid_InFlex_RespondsToFlexSize() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:400px'><div style='flex:1;display:grid;grid-template-columns:1fr 1fr'><div id='a' style='height:20px'></div><div id='b' style='height:20px'></div></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Width - 200) < 2);
        }

        [Fact] public void Flex_InGrid_RespondsToGridSize() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:300px;width:300px'><div style='display:flex'><div id='a' style='flex:1;height:20px'></div><div id='b' style='flex:1;height:20px'></div></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Width - 150) < 2);
        }

        [Fact] public void Grid_3Level_Nesting() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:400px;width:400px'><div style='display:grid;grid-template-columns:1fr 1fr'><div style='display:grid;grid-template-columns:1fr 1fr'><div id='t' style='height:20px'></div></div></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 100) < 2);
        }

        [Fact] public void Grid_InBlock_UsesBlockWidth() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='width:300px'><div style='display:grid;grid-template-columns:1fr 1fr 1fr'><div id='a' style='height:20px'></div><div id='b' style='height:20px'></div><div id='c' style='height:20px'></div></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Width - 100) < 2);
        }

        [Fact] public void Grid_AutoFill_100px() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:repeat(auto-fill,100px);width:350px'><div id='a' style='height:20px'></div><div id='b' style='height:20px'></div><div id='c' style='height:20px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Width - 100) < 2);
        }

        [Fact] public void Grid_WithPadding_ChildrenFitContent() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:1fr;width:200px;padding:20px'><div id='t' style='height:20px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 200) < 2);
        }

        [Fact] public void Grid_WithBorder_ChildrenFitContent() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:1fr;width:200px;border:10px solid'><div id='t' style='height:20px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 200) < 2);
        }

        [Fact] public void Grid_BorderBox_ChildrenFitContent() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:1fr;box-sizing:border-box;width:200px;padding:20px'><div id='t' style='height:20px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 160) < 2);
        }

        [Fact] public void TwoGrids_SideBySide_InFlex() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:400px'><div style='flex:1;display:grid;grid-template-columns:1fr 1fr'><div id='a' style='height:20px'></div><div id='b' style='height:20px'></div></div><div style='flex:1;display:grid;grid-template-columns:1fr 1fr'><div id='c' style='height:20px'></div><div id='d' style='height:20px'></div></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"c")!.ContentRect.Width - 100) < 2);
        }

        [Fact] public void Grid_PercentWidth_InBlock() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='width:400px'><div style='display:grid;grid-template-columns:1fr 1fr;width:50%'><div id='a' style='height:20px'></div><div id='b' style='height:20px'></div></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Width - 100) < 2);
        }

        [Fact] public void Grid_Height_FromRows() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='g' style='display:grid;grid-template-columns:200px;grid-template-rows:50px 60px 70px;width:200px'><div></div><div></div><div></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"g")!.ContentRect.Height - 180) < 2);
        }
    }
}
