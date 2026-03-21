using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class WptFlexboxDisplayTests
    {
        private readonly ITestOutputHelper _output;
        public WptFlexboxDisplayTests(ITestOutputHelper output) { _output = output; }

        [Fact] public void DisplayFlex_IsBlock() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='display:flex;height:30px'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 400) < 2);
        }

        [Fact] public void DisplayInlineFlex_ShrinkToFit() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='width:400px'><div id='t' style='display:inline-flex'><div style='width:80px;height:30px'></div></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 80) < 2);
        }

        [Fact] public void DisplayInlineFlex_MultipleItems() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='width:400px'><div id='t' style='display:inline-flex'><div style='width:50px;height:30px'></div><div style='width:60px;height:30px'></div></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 110) < 2);
        }

        [Fact] public void Flex_ChildrenAreFlexItems() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:300px'><div id='a' style='width:100px;height:30px'></div><div id='b' style='width:100px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.X - 100) < 2);
        }

        [Fact] public void Flex_FloatIgnored() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:300px'><div id='t' style='float:left;width:100px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.X - 0) < 2);
        }

        [Fact] public void Flex_ClearIgnored() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:300px'><div style='width:50px;height:30px'></div><div id='t' style='clear:both;width:50px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.X - 50) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 0) < 2);
        }

        [Fact] public void Flex_VerticalAlignIgnored() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:300px;height:100px'><div id='t' style='vertical-align:bottom;width:50px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 0) < 2);
        }

        [Fact] public void InlineFlex_WithExplicitWidth() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='width:400px'><div id='t' style='display:inline-flex;width:200px'><div style='width:50px;height:30px'></div></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 200) < 2);
        }

        [Fact] public void Flex_EmptyContainer() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='display:flex;width:200px;height:50px'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 200) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height - 50) < 2);
        }

        [Fact] public void Flex_AutoHeight_FromChildren() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='display:flex;width:200px'><div style='width:50px;height:80px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height - 80) < 2);
        }

        [Fact] public void Flex_AutoHeight_TallestChild() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='display:flex;width:200px'><div style='width:50px;height:30px'></div><div style='width:50px;height:60px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height - 60) < 2);
        }

        [Fact] public void Flex_PercentWidth() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='width:400px'><div id='t' style='display:flex;width:50%;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 200) < 2);
        }

        [Fact] public void Flex_MarginAutoCenter() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='display:flex;width:200px;height:30px;margin:0 auto'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.X - 100) < 2);
        }

        [Fact] public void Flex_InBlock() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='width:300px'><div id='t' style='display:flex'><div style='width:50px;height:20px'></div></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 300) < 2);
        }

        [Fact] public void Flex_NestedInFlex() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:400px'><div id='inner' style='display:flex;flex:1'><div style='width:50px;height:30px'></div></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"inner")!.ContentRect.Width - 400) < 2);
        }
    }
}
