using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class WptFlexboxThreeItemTests
    {
        private readonly ITestOutputHelper _output;
        public WptFlexboxThreeItemTests(ITestOutputHelper output) { _output = output; }

        [Fact] public void ThreeFixed_Widths() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:400px'><div id='a' style='width:80px;height:30px'></div><div id='b' style='width:100px;height:30px'></div><div id='c' style='width:120px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Width - 80) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"c")!.ContentRect.Width - 120) < 2);
        }

        [Fact] public void ThreeFixed_Positions() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:400px'><div id='a' style='width:80px;height:30px'></div><div id='b' style='width:100px;height:30px'></div><div id='c' style='width:120px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.X - 80) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"c")!.ContentRect.X - 180) < 2);
        }

        [Fact] public void ThreeGrow1_Equal() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:300px'><div id='a' style='flex:1;height:30px'></div><div id='b' style='flex:1;height:30px'></div><div id='c' style='flex:1;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"c")!.ContentRect.Width - 100) < 2);
        }

        [Fact] public void Grow_1_2_3_Ratio() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:600px'><div id='a' style='flex:1 0 0px;height:30px'></div><div id='b' style='flex:2 0 0px;height:30px'></div><div id='c' style='flex:3 0 0px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.Width - 200) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"c")!.ContentRect.Width - 300) < 2);
        }

        [Fact] public void Fixed_Grow_Fixed() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:400px'><div id='a' style='flex:0 0 80px;height:30px'></div><div id='b' style='flex:1;height:30px'></div><div id='c' style='flex:0 0 80px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Width - 80) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.Width - 240) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"c")!.ContentRect.Width - 80) < 2);
        }

        [Fact] public void ThreeItems_Center() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;justify-content:center;width:400px'><div id='a' style='width:60px;height:30px'></div><div id='b' style='width:60px;height:30px'></div><div id='c' style='width:60px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.X - 110) < 2);
        }

        [Fact] public void ThreeItems_SpaceBetween() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;justify-content:space-between;width:400px'><div id='a' style='width:60px;height:30px'></div><div id='b' style='width:60px;height:30px'></div><div id='c' style='width:60px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"c")!.ContentRect.X - 340) < 2);
        }

        [Fact] public void ThreeItems_SpaceEvenly() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;justify-content:space-evenly;width:400px'><div id='a' style='width:60px;height:30px'></div><div id='b' style='width:60px;height:30px'></div><div id='c' style='width:60px;height:30px'></div></div></body>");
            float gap = (400 - 180) / 4f;
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.X - gap) < 2);
        }

        [Fact] public void ThreeItems_WithGap() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;gap:10px;width:400px'><div id='a' style='width:60px;height:30px'></div><div id='b' style='width:60px;height:30px'></div><div id='c' style='width:60px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.X - 70) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"c")!.ContentRect.X - 140) < 2);
        }

        [Fact] public void ThreeItems_Column() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-direction:column;width:200px'><div id='a' style='height:30px'></div><div id='b' style='height:40px'></div><div id='c' style='height:50px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Y - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.Y - 30) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"c")!.ContentRect.Y - 70) < 2);
        }

        [Fact] public void ThreeShrink_Equal() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:150px'><div id='a' style='flex:0 1 80px;min-width:0;height:30px'></div><div id='b' style='flex:0 1 80px;min-width:0;height:30px'></div><div id='c' style='flex:0 1 80px;min-width:0;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Width - 50) < 2);
        }

        [Fact] public void Grow_1_1_2() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:400px'><div id='a' style='flex:1 0 0px;height:30px'></div><div id='b' style='flex:1 0 0px;height:30px'></div><div id='c' style='flex:2 0 0px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"c")!.ContentRect.Width - 200) < 2);
        }

        [Fact] public void ThreeItems_AllSameY() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:300px'><div id='a' style='width:60px;height:30px'></div><div id='b' style='width:60px;height:50px'></div><div id='c' style='width:60px;height:40px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Y - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.Y - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"c")!.ContentRect.Y - 0) < 2);
        }

        [Fact] public void ThreeItems_Order_Reorders() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:300px'><div id='a' style='order:3;width:50px;height:30px'></div><div id='b' style='order:1;width:50px;height:30px'></div><div id='c' style='order:2;width:50px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"c")!.ContentRect.X - 50) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.X - 100) < 2);
        }
    }
}
