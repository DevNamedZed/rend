using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class WptFlexboxPositionTests
    {
        private readonly ITestOutputHelper _output;
        public WptFlexboxPositionTests(ITestOutputHelper output) { _output = output; }

        [Fact] public void TwoItems_XPositions() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:300px'><div id='a' style='width:80px;height:30px'></div><div id='b' style='width:100px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.X - 80) < 2);
        }

        [Fact] public void ThreeItems_XPositions() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:400px'><div id='a' style='width:60px;height:30px'></div><div id='b' style='width:80px;height:30px'></div><div id='c' style='width:100px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"c")!.ContentRect.X - 140) < 2);
        }

        [Fact] public void FourItems_XPositions() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:400px'><div id='a' style='width:50px;height:30px'></div><div id='b' style='width:60px;height:30px'></div><div id='c' style='width:70px;height:30px'></div><div id='d' style='width:80px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"d")!.ContentRect.X - 180) < 2);
        }

        [Fact] public void Items_AllY0() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:300px'><div id='a' style='width:50px;height:30px'></div><div id='b' style='width:50px;height:40px'></div><div id='c' style='width:50px;height:20px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Y - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.Y - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"c")!.ContentRect.Y - 0) < 2);
        }

        [Fact] public void Center_TwoItems_Offset() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;justify-content:center;width:400px'><div id='a' style='width:60px;height:30px'></div><div id='b' style='width:60px;height:30px'></div></div></body>");
            float offset = (400 - 120) / 2f;
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.X - offset) < 2);
        }

        [Fact] public void FlexEnd_SingleItem() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;justify-content:flex-end;width:400px'><div id='t' style='width:100px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.X - 300) < 2);
        }

        [Fact] public void Gap_ItemPositions() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;gap:10px;width:400px'><div id='a' style='width:50px;height:30px'></div><div id='b' style='width:50px;height:30px'></div><div id='c' style='width:50px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.X - 60) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"c")!.ContentRect.X - 120) < 2);
        }

        [Fact] public void Column_YPositions() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-direction:column;width:200px'><div id='a' style='height:40px'></div><div id='b' style='height:50px'></div><div id='c' style='height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.Y - 40) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"c")!.ContentRect.Y - 90) < 2);
        }

        [Fact] public void Column_Gap_YPositions() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-direction:column;gap:15px;width:200px'><div id='a' style='height:30px'></div><div id='b' style='height:30px'></div><div id='c' style='height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.Y - 45) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"c")!.ContentRect.Y - 90) < 2);
        }

        [Fact] public void Grow_ItemPositions() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:300px'><div id='a' style='flex:1;height:30px'></div><div id='b' style='flex:1;height:30px'></div><div id='c' style='flex:1;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.X - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"c")!.ContentRect.X - 200) < 2);
        }

        [Fact] public void AlignCenter_YPosition() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;align-items:center;height:100px;width:200px'><div id='t' style='width:50px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 35) < 2);
        }

        [Fact] public void AlignFlexEnd_YPosition() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;align-items:flex-end;height:100px;width:200px'><div id='t' style='width:50px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 70) < 2);
        }

        [Fact] public void Margin_XPosition() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:300px'><div id='a' style='width:50px;margin-right:20px;height:30px'></div><div id='b' style='width:50px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.X - 70) < 2);
        }

        [Fact] public void MarginAuto_PushesRight() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:300px'><div id='a' style='width:50px;margin-right:auto;height:30px'></div><div id='b' style='width:50px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.X - 250) < 2);
        }

        [Fact] public void Fixed_Grow_Fixed_Positions() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:400px'><div id='a' style='flex:0 0 80px;height:30px'></div><div id='b' style='flex:1;height:30px'></div><div id='c' style='flex:0 0 80px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.X - 80) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"c")!.ContentRect.X - 320) < 2);
        }
    }
}
