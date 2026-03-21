using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class WptFlexboxMixedLayoutTests
    {
        private readonly ITestOutputHelper _output;
        public WptFlexboxMixedLayoutTests(ITestOutputHelper output) { _output = output; }

        [Fact] public void Flex_In_Block_FillsWidth() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='width:300px'><div id='t' style='display:flex;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 300) < 2);
        }

        [Fact] public void Block_In_FlexItem() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:300px'><div style='flex:1'><div id='t' style='height:30px'></div></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 300) < 2);
        }

        [Fact] public void Table_In_FlexItem() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:400px'><div style='flex:1'><table id='t' style='width:100%;margin:0;border-spacing:0'><tr><td style='height:30px'></td></tr></table></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 400) < 3);
        }

        [Fact] public void Flex_In_GridItem() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:300px;width:300px'><div style='display:flex'><div id='a' style='flex:1;height:30px'></div><div id='b' style='flex:1;height:30px'></div></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Width - 150) < 2);
        }

        [Fact] public void Grid_In_FlexItem() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:400px'><div style='flex:1;display:grid;grid-template-columns:1fr 1fr'><div id='a' style='height:30px'></div><div id='b' style='height:30px'></div></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Width - 200) < 2);
        }

        [Fact] public void InlineBlock_In_FlexItem() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:300px'><div style='flex:1'><div id='t' style='display:inline-block;width:80px;height:30px'></div></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 80) < 2);
        }

        [Fact] public void Float_In_FlexItem_Ignored() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:300px'><div id='t' style='float:left;width:100px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 100) < 2);
        }

        [Fact] public void Abspos_In_FlexItem() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;position:relative;width:300px;height:200px'><div id='t' style='position:absolute;top:10px;left:10px;width:50px;height:50px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.X - 10) < 2);
        }

        [Fact] public void Flex_Inside_OverflowHidden() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='overflow:hidden;width:200px;height:200px'><div style='display:flex'><div id='a' style='flex:1;height:30px'></div><div id='b' style='flex:1;height:30px'></div></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Width - 100) < 2);
        }

        [Fact] public void Nested_Flex_ThreeLevels() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:400px'><div style='display:flex;flex:1'><div style='display:flex;flex:1'><div id='t' style='flex:1;height:30px'></div></div></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 400) < 2);
        }

        [Fact] public void Column_Flex_In_Row_Flex() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:300px;height:200px'><div style='display:flex;flex-direction:column;flex:1'><div id='a' style='flex:1'></div><div id='b' style='flex:1'></div></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Height - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.Height - 100) < 2);
        }

        [Fact] public void Row_Flex_In_Column_Flex() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-direction:column;width:300px;height:200px'><div style='display:flex;flex:1'><div id='a' style='flex:1'></div><div id='b' style='flex:1'></div></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Width - 150) < 2);
        }

        [Fact] public void Flex_With_Margin_Auto_Centering() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:300px;height:200px'><div id='t' style='width:100px;height:80px;margin:auto'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.X - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 60) < 2);
        }

        [Fact] public void Flex_In_Table_Cell() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><table style='margin:0;border-spacing:0;width:300px'><tr><td><div style='display:flex'><div id='a' style='flex:1;height:30px'></div><div id='b' style='flex:1;height:30px'></div></div></td></tr></table></body>");
            Assert.True(LayoutTestHelper.FindById(r,"a")!.ContentRect.Width > 0);
            Assert.True(LayoutTestHelper.FindById(r,"b")!.ContentRect.Width > 0);
        }
    }
}
