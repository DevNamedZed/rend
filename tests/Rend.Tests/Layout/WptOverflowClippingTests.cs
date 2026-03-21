using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class WptOverflowClippingTests
    {
        private readonly ITestOutputHelper _output;
        public WptOverflowClippingTests(ITestOutputHelper output) { _output = output; }

        [Fact] public void OverflowHidden_ContainsFloats() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='p' style='overflow:hidden;width:200px'><div style='float:left;width:100px;height:80px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"p")!.ContentRect.Height >= 79);
        }

        [Fact] public void OverflowAuto_ContainsFloats() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='p' style='overflow:auto;width:200px'><div style='float:left;width:100px;height:80px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"p")!.ContentRect.Height >= 79);
        }

        [Fact] public void OverflowScroll_ContainsFloats() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='p' style='overflow:scroll;width:200px'><div style='float:left;width:100px;height:80px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"p")!.ContentRect.Height >= 79);
        }

        [Fact] public void OverflowVisible_DoesNotContainFloats() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='p' style='overflow:visible;width:200px'><div style='float:left;width:100px;height:80px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"p")!.ContentRect.Height < 81);
        }

        [Fact] public void OverflowHidden_NoMarginCollapse() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='overflow:hidden;margin-bottom:30px;height:50px'></div><div id='t' style='overflow:hidden;margin-top:20px;height:50px'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 80) < 2);
        }

        [Fact] public void OverflowHidden_ChildOverflow() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='overflow:hidden;width:100px;height:100px'><div id='t' style='width:200px;height:200px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 200) < 2);
        }

        [Fact] public void OverflowHidden_DoesNotAffectLayout() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='overflow:hidden;width:200px;height:200px'><div style='height:50px'></div><div id='t' style='height:50px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 50) < 2);
        }

        [Fact] public void OverflowHidden_CreatesNewBfc() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='margin-bottom:30px;height:50px'></div><div style='overflow:hidden'><div id='t' style='margin-top:20px;height:50px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y >= 79);
        }

        [Fact] public void OverflowHidden_Width_Respected() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='overflow:hidden;width:150px;height:100px'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 150) < 2);
        }

        [Fact] public void OverflowAuto_Width_Respected() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='overflow:auto;width:150px;height:100px'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 150) < 2);
        }

        [Fact] public void FlowRoot_ContainsFloats() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='p' style='display:flow-root;width:200px'><div style='float:left;width:100px;height:100px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"p")!.ContentRect.Height >= 99);
        }

        [Fact] public void OverflowHidden_Nested() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='overflow:hidden;width:200px;height:200px'><div style='overflow:hidden;width:150px;height:150px'><div id='t' style='width:300px;height:300px'></div></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 300) < 2);
        }

        [Fact] public void Overflow_With_Padding() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='overflow:hidden;width:200px;height:200px;padding:20px'><div id='t' style='height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 200) < 2);
        }

        [Fact] public void Overflow_With_Border() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='overflow:hidden;width:200px;height:200px;border:10px solid'><div id='t' style='height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 200) < 2);
        }
    }
}
