using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class WptFlexboxMinContentTests
    {
        private readonly ITestOutputHelper _output;
        public WptFlexboxMinContentTests(ITestOutputHelper output) { _output = output; }

        [Fact] public void MinContent_Width_SingleItem() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='width:fit-content'><div style='display:flex'><div id='t' style='width:100px;height:30px'></div></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width >= 99);
        }

        [Fact] public void MinContent_Width_TwoItems() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='width:fit-content'><div style='display:flex'><div style='width:80px;height:30px'></div><div id='b' style='width:120px;height:30px'></div></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.Width - 120) < 2);
        }

        [Fact] public void Flex_InMinContentContext() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:min-content'><div id='t' style='width:150px;height:30px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width >= 149);
        }

        [Fact] public void Flex_InMaxContentContext() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:max-content'><div id='a' style='width:100px;height:30px'></div><div id='b' style='width:80px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.X - 100) < 2);
        }

        [Fact] public void Flex_FitContent() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:fit-content'><div id='t' style='width:120px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 120) < 2);
        }

        [Fact] public void InlineFlex_ShrinkToFit_TwoItems() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='width:400px'><div id='flex' style='display:inline-flex'><div style='width:60px;height:30px'></div><div style='width:80px;height:30px'></div></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"flex")!.ContentRect.Width - 140) < 2);
        }

        [Fact] public void InlineFlex_ShrinkToFit_WithGap() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='width:400px'><div id='flex' style='display:inline-flex;gap:10px'><div style='width:60px;height:30px'></div><div style='width:80px;height:30px'></div></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"flex")!.ContentRect.Width - 150) < 2);
        }

        [Fact] public void Flex_MinContent_SingleChild() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='display:flex;width:min-content'><div style='width:80px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 80) < 2);
        }

        [Fact] public void Flex_MaxContent_SingleChild() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='display:flex;width:max-content'><div style='width:150px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 150) < 2);
        }

        [Fact] public void InlineFlex_ThreeItems() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='width:400px'><div id='flex' style='display:inline-flex'><div style='width:50px;height:30px'></div><div style='width:60px;height:30px'></div><div style='width:70px;height:30px'></div></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"flex")!.ContentRect.Width - 180) < 2);
        }

        [Fact] public void InlineFlex_ExplicitWidth_Overrides() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='width:400px'><div id='flex' style='display:inline-flex;width:250px'><div style='width:50px;height:30px'></div></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"flex")!.ContentRect.Width - 250) < 2);
        }

        [Fact] public void Flex_FitContent_Constrained() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='width:200px'><div id='t' style='display:flex;width:fit-content'><div style='width:300px;height:30px'></div></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width <= 201);
        }
    }
}
