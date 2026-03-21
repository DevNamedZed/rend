using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class WptFlexboxContainerSizingTests
    {
        private readonly ITestOutputHelper _output;
        public WptFlexboxContainerSizingTests(ITestOutputHelper output) { _output = output; }

        [Fact] public void Flex_ExplicitWidth() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='display:flex;width:300px'><div style='width:50px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 300) < 2);
        }

        [Fact] public void Flex_AutoWidth_Block() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='width:400px'><div id='t' style='display:flex'><div style='width:50px;height:30px'></div></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 400) < 2);
        }

        [Fact] public void Flex_PercentWidth() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='width:400px'><div id='t' style='display:flex;width:50%'><div style='width:50px;height:30px'></div></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 200) < 2);
        }

        [Fact] public void Flex_ExplicitHeight() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='display:flex;width:200px;height:150px'><div style='width:50px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height - 150) < 2);
        }

        [Fact] public void Flex_AutoHeight_TallestItem() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='display:flex;width:200px'><div style='width:50px;height:30px'></div><div style='width:50px;height:80px'></div><div style='width:50px;height:50px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height - 80) < 2);
        }

        [Fact] public void Flex_AutoHeight_ColumnSum() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='display:flex;flex-direction:column;width:200px'><div style='height:30px'></div><div style='height:40px'></div><div style='height:50px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height - 120) < 2);
        }

        [Fact] public void Flex_MinWidth() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='display:flex;width:100px;min-width:200px'><div style='width:50px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 200) < 2);
        }

        [Fact] public void Flex_MaxWidth() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='display:flex;width:300px;max-width:200px'><div style='width:50px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 200) < 2);
        }

        [Fact] public void Flex_MinHeight() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='display:flex;width:200px;min-height:100px'><div style='width:50px;height:30px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height >= 99);
        }

        [Fact] public void Flex_MaxHeight() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='display:flex;width:200px;height:200px;max-height:100px'><div style='width:50px;height:30px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height <= 101);
        }

        [Fact] public void Flex_WithPadding() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='display:flex;width:300px;padding:20px'><div style='width:50px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 300) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.PaddingLeft - 20) < 2);
        }

        [Fact] public void Flex_WithBorder() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='display:flex;width:300px;border:10px solid'><div style='width:50px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 300) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.BorderLeftWidth - 10) < 1);
        }

        [Fact] public void Flex_BorderBox() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='display:flex;box-sizing:border-box;width:300px;padding:20px;border:10px solid'><div style='height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 240) < 2);
        }

        [Fact] public void Flex_MarginAutoCenter() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='display:flex;width:200px;height:30px;margin:0 auto'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.X - 100) < 2);
        }

        [Fact] public void Flex_CalcWidth() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='display:flex;width:calc(200px + 100px)'><div style='width:50px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 300) < 2);
        }

        [Fact] public void Flex_VwWidth() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='display:flex;width:50vw'><div style='width:50px;height:30px'></div></div></body>", 400, 300);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 200) < 2);
        }

        [Fact] public void InlineFlex_ShrinkToFit() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='width:400px'><div id='t' style='display:inline-flex'><div style='width:80px;height:30px'></div><div style='width:60px;height:30px'></div></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 140) < 2);
        }

        [Fact] public void InlineFlex_ExplicitWidth() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='width:400px'><div id='t' style='display:inline-flex;width:250px'><div style='width:50px;height:30px'></div></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 250) < 2);
        }

        [Fact] public void Flex_EmptyContainer_Height() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='display:flex;width:200px'></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height < 2);
        }

        [Fact] public void Flex_EmptyContainer_ExplicitHeight() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='display:flex;width:200px;height:100px'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height - 100) < 2);
        }
    }
}
