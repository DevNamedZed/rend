using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class WptFlexboxAutoMarginTests
    {
        private readonly ITestOutputHelper _output;
        public WptFlexboxAutoMarginTests(ITestOutputHelper output) { _output = output; }

        [Fact] public void MarginAuto_CentersItem() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:300px;height:100px'><div id='t' style='width:100px;height:50px;margin:auto'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.X - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 25) < 2);
        }

        [Fact] public void MarginLeftAuto_PushesRight() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:300px'><div id='t' style='width:100px;height:30px;margin-left:auto'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.X - 200) < 2);
        }

        [Fact] public void MarginRightAuto_PushesLeft() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:300px'><div id='t' style='width:100px;height:30px;margin-right:auto'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.X - 0) < 2);
        }

        [Fact] public void MarginLeftAuto_Between_Items() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:300px'><div id='a' style='width:50px;height:30px'></div><div id='b' style='width:50px;height:30px;margin-left:auto'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.X - 250) < 2);
        }

        [Fact] public void MarginRightAuto_SplitsItems() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:300px'><div id='a' style='width:50px;height:30px;margin-right:auto'></div><div id='b' style='width:50px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.X - 250) < 2);
        }

        [Fact] public void MarginTopAuto_PushesDown() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:200px;height:100px'><div id='t' style='width:50px;height:30px;margin-top:auto'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 70) < 2);
        }

        [Fact] public void MarginBottomAuto_PushesUp() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:200px;height:100px'><div id='t' style='width:50px;height:30px;margin-bottom:auto'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 0) < 2);
        }

        [Fact] public void MarginAutoY_CentersVertically() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:200px;height:100px'><div id='t' style='width:50px;height:30px;margin-top:auto;margin-bottom:auto'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 35) < 2);
        }

        [Fact] public void MarginAutoX_CentersHorizontally() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:300px'><div id='t' style='width:100px;height:30px;margin-left:auto;margin-right:auto'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.X - 100) < 2);
        }

        [Fact] public void MarginAuto_OverridesAlignItems() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;align-items:flex-start;width:200px;height:100px'><div id='t' style='width:50px;height:30px;margin-top:auto'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 70) < 2);
        }

        [Fact] public void MarginAuto_OverridesJustifyContent() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;justify-content:flex-start;width:300px'><div id='t' style='width:100px;height:30px;margin-left:auto'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.X - 200) < 2);
        }

        [Fact] public void TwoItems_BothMarginLeftAuto() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:300px'><div id='a' style='width:50px;height:30px;margin-left:auto'></div><div id='b' style='width:50px;height:30px;margin-left:auto'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"a")!.ContentRect.X > 50);
            Assert.True(LayoutTestHelper.FindById(r,"b")!.ContentRect.X > LayoutTestHelper.FindById(r,"a")!.ContentRect.X);
        }

        [Fact] public void Column_MarginTopAuto() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-direction:column;width:200px;height:200px'><div id='a' style='height:30px'></div><div id='b' style='height:30px;margin-top:auto'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.Y - 170) < 2);
        }

        [Fact] public void Column_MarginAutoX_Centers() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-direction:column;width:200px'><div id='t' style='width:80px;height:30px;margin-left:auto;margin-right:auto'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.X - 60) < 2);
        }

        [Fact] public void MarginAuto_NoFreeSpace() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:100px'><div id='a' style='width:50px;height:30px'></div><div id='b' style='width:50px;height:30px;margin-left:auto'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.X - 50) < 2);
        }
    }
}
