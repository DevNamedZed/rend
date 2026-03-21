using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class WptFlexboxFiveItemTests
    {
        private readonly ITestOutputHelper _output;
        public WptFlexboxFiveItemTests(ITestOutputHelper output) { _output = output; }

        [Fact] public void FiveEqual_Grow1() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:400px'><div id='a' style='flex:1;height:30px'></div><div id='b' style='flex:1;height:30px'></div><div id='c' style='flex:1;height:30px'></div><div id='d' style='flex:1;height:30px'></div><div id='e' style='flex:1;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Width - 80) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"e")!.ContentRect.Width - 80) < 2);
        }

        [Fact] public void FiveFixed_Positions() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:400px'><div id='a' style='width:50px;height:30px'></div><div id='b' style='width:60px;height:30px'></div><div id='c' style='width:70px;height:30px'></div><div id='d' style='width:80px;height:30px'></div><div id='e' style='width:40px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.X - 50) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"c")!.ContentRect.X - 110) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"d")!.ContentRect.X - 180) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"e")!.ContentRect.X - 260) < 2);
        }

        [Fact] public void FiveItems_SpaceBetween() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;justify-content:space-between;width:400px'><div id='a' style='width:40px;height:30px'></div><div style='width:40px;height:30px'></div><div style='width:40px;height:30px'></div><div style='width:40px;height:30px'></div><div id='e' style='width:40px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"e")!.ContentRect.X - 360) < 2);
        }

        [Fact] public void FiveItems_WithGap() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;gap:10px;width:400px'><div id='a' style='width:40px;height:30px'></div><div id='b' style='width:40px;height:30px'></div><div id='c' style='width:40px;height:30px'></div><div id='d' style='width:40px;height:30px'></div><div id='e' style='width:40px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"e")!.ContentRect.X - 200) < 2);
        }

        [Fact] public void FiveItems_Column() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-direction:column;width:200px'><div style='height:20px'></div><div style='height:30px'></div><div style='height:25px'></div><div style='height:35px'></div><div id='e' style='height:15px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"e")!.ContentRect.Y - 110) < 2);
        }

        [Fact] public void FiveItems_Grow_1_1_2_2_4() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:400px'><div id='a' style='flex:1 0 0px;height:30px'></div><div id='b' style='flex:1 0 0px;height:30px'></div><div id='c' style='flex:2 0 0px;height:30px'></div><div id='d' style='flex:2 0 0px;height:30px'></div><div id='e' style='flex:4 0 0px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Width - 40) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"e")!.ContentRect.Width - 160) < 2);
        }

        [Fact] public void FiveItems_Center() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;justify-content:center;width:400px'><div id='a' style='width:40px;height:30px'></div><div style='width:40px;height:30px'></div><div style='width:40px;height:30px'></div><div style='width:40px;height:30px'></div><div style='width:40px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.X - 100) < 2);
        }

        [Fact] public void FiveItems_Order() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:400px'><div id='a' style='order:5;width:30px;height:30px'></div><div id='b' style='order:3;width:30px;height:30px'></div><div id='c' style='order:1;width:30px;height:30px'></div><div id='d' style='order:4;width:30px;height:30px'></div><div id='e' style='order:2;width:30px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"c")!.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"e")!.ContentRect.X - 30) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.X - 60) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"d")!.ContentRect.X - 90) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.X - 120) < 2);
        }

        [Fact] public void FiveItems_FlexEnd() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;justify-content:flex-end;width:400px'><div id='a' style='width:40px;height:30px'></div><div style='width:40px;height:30px'></div><div style='width:40px;height:30px'></div><div style='width:40px;height:30px'></div><div style='width:40px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.X - 200) < 2);
        }

        [Fact] public void FiveItems_ColumnGrow() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-direction:column;width:200px;height:400px'><div id='a' style='flex:1'></div><div id='b' style='flex:1'></div><div id='c' style='flex:1'></div><div id='d' style='flex:1'></div><div id='e' style='flex:1'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Height - 80) < 2);
        }
    }
}
