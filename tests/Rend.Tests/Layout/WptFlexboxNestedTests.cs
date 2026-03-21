using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class WptFlexboxNestedTests
    {
        private readonly ITestOutputHelper _output;
        public WptFlexboxNestedTests(ITestOutputHelper output) { _output = output; }

        [Fact] public void Flex_Inside_Flex() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:400px'><div style='display:flex;flex:1'><div id='a' style='width:50px;height:30px'></div><div id='b' style='width:50px;height:30px'></div></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.X - 50) < 2);
        }

        [Fact] public void Flex_Inside_Grid() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:300px;width:300px'><div style='display:flex'><div id='a' style='width:50px;height:30px'></div><div id='b' style='width:50px;height:30px'></div></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.X - 50) < 2);
        }

        [Fact] public void Grid_Inside_Flex() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:400px'><div style='display:grid;grid-template-columns:100px 100px;flex:1'><div id='a' style='height:30px'></div><div id='b' style='height:30px'></div></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Width - 100) < 2);
        }

        [Fact] public void Flex_Inside_Block() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='width:300px'><div style='display:flex'><div id='a' style='flex:1;height:30px'></div><div id='b' style='flex:1;height:30px'></div></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Width - 150) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.Width - 150) < 2);
        }

        [Fact] public void Grid_Inside_Grid() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:300px;width:300px'><div style='display:grid;grid-template-columns:1fr 1fr'><div id='a' style='height:30px'></div><div id='b' style='height:30px'></div></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Width - 150) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.Width - 150) < 2);
        }

        [Fact] public void Block_Inside_Flex() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:300px'><div style='flex:1'><div id='t' style='height:30px'></div></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 300) < 2);
        }

        [Fact] public void Column_Flex_Inside_Row_Flex() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:300px'><div style='display:flex;flex-direction:column;flex:1'><div id='a' style='height:30px'></div><div id='b' style='height:30px'></div></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Y - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.Y - 30) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Width - 300) < 2);
        }

        [Fact] public void Row_Flex_Inside_Column_Flex() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-direction:column;width:300px;height:200px'><div style='display:flex;flex:1'><div id='a' style='width:50px'></div><div id='b' style='width:50px'></div></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.X - 50) < 2);
        }

        [Fact] public void ThreeLevel_Nesting() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:400px'><div style='display:flex;flex:1'><div style='display:flex;flex:1'><div id='t' style='flex:1;height:30px'></div></div></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 400) < 2);
        }

        [Fact] public void Flex_Item_As_Grid_Container() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:300px'><div style='flex:1;display:grid;grid-template-columns:1fr 1fr'><div id='a' style='height:30px'></div><div id='b' style='height:30px'></div></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Width - 150) < 2);
        }

        [Fact] public void Flex_Grow_Inside_FixedFlex() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:400px'><div style='width:200px;display:flex'><div id='a' style='flex:1;height:30px'></div><div id='b' style='flex:1;height:30px'></div></div><div style='flex:1;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.Width - 100) < 2);
        }

        [Fact] public void Nested_Justify_Content() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;justify-content:center;width:300px'><div style='display:flex;justify-content:space-between;width:200px'><div id='a' style='width:50px;height:30px'></div><div id='b' style='width:50px;height:30px'></div></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.X - 50) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.X - 200) < 2);
        }

        [Fact] public void Nested_AlignItems() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;align-items:center;height:100px;width:300px'><div style='display:flex;align-items:flex-end;height:60px;width:200px'><div id='t' style='width:50px;height:20px'></div></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 60) < 2);
        }
    }
}
