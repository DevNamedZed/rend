using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class WptSizingIntrinsicTests
    {
        private readonly ITestOutputHelper _output;
        public WptSizingIntrinsicTests(ITestOutputHelper output) { _output = output; }

        [Fact] public void FitContent_Block() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='width:fit-content'><div id='t' style='width:150px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 150) < 2);
        }

        [Fact] public void MinContent_Block() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='width:min-content'><div id='t' style='width:150px;height:30px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width <= 151);
        }

        [Fact] public void MaxContent_Block() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='width:max-content'><div id='t' style='width:150px;height:30px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width >= 149);
        }

        [Fact] public void Auto_Width_FillsContainer() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='width:300px'><div id='t' style='height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 300) < 2);
        }

        [Fact] public void Auto_Height_FromContent() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='width:200px'><div style='height:50px'></div><div style='height:70px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height - 120) < 2);
        }

        [Fact] public void InlineBlock_ShrinkToFit() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='width:400px'><div id='t' style='display:inline-block'><div style='width:80px;height:30px'></div></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 80) < 2);
        }

        [Fact] public void Float_ShrinkToFit() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='width:400px'><div id='t' style='float:left'><div style='width:120px;height:30px'></div></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 120) < 2);
        }

        [Fact] public void Abspos_ShrinkToFit() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='position:relative;width:400px;height:200px'><div id='t' style='position:absolute'><div style='width:100px;height:30px'></div></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width <= 101);
        }

        [Fact] public void Width_100Percent() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='width:300px'><div id='t' style='width:100%;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 300) < 2);
        }

        [Fact] public void Width_Auto_With_Margin() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='width:300px'><div id='t' style='margin:0 50px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 200) < 2);
        }

        [Fact] public void Width_Auto_With_Padding() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='width:300px'><div id='t' style='padding:0 30px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 240) < 2);
        }

        [Fact] public void Flex_MinContent() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:min-content'><div id='t' style='width:100px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 100) < 2);
        }

        [Fact] public void Flex_MaxContent() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:max-content'><div style='width:100px;height:30px'></div><div id='b' style='width:80px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.X - 100) < 2);
        }

        [Fact] public void Table_AutoWidth_ShrinkToFit() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><table id='t' style='margin:0;border-spacing:0'><tr><td style='width:80px;height:30px'></td><td style='width:120px;height:30px'></td></tr></table></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 200) < 5);
        }

        [Fact] public void Width_Calc_Percent() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='width:400px'><div id='t' style='width:calc(100% - 80px);height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 320) < 2);
        }

        [Fact] public void MinWidth_Larger_Than_Width() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='width:100px;min-width:200px;height:30px'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 200) < 2);
        }

        [Fact] public void MaxWidth_Smaller_Than_Width() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='width:300px;max-width:150px;height:30px'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 150) < 2);
        }

        [Fact] public void MinHeight_Larger_Than_Height() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='width:200px;height:50px;min-height:100px'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height - 100) < 2);
        }

        [Fact] public void MaxHeight_Smaller_Than_Height() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='width:200px;height:200px;max-height:100px'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height - 100) < 2);
        }

        [Fact] public void Nested_AutoWidth_Chain() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='width:400px'><div><div><div id='t' style='height:30px'></div></div></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 400) < 2);
        }
    }
}
