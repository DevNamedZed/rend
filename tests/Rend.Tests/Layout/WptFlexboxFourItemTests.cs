using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class WptFlexboxFourItemTests
    {
        private readonly ITestOutputHelper _output;
        public WptFlexboxFourItemTests(ITestOutputHelper output) { _output = output; }

        [Fact] public void FourEqual_Grow1() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:400px'><div id='a' style='flex:1;height:30px'></div><div id='b' style='flex:1;height:30px'></div><div id='c' style='flex:1;height:30px'></div><div id='d' style='flex:1;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"d")!.ContentRect.Width - 100) < 2);
        }

        [Fact] public void FourFixed_Positions() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:400px'><div id='a' style='width:80px;height:30px'></div><div id='b' style='width:100px;height:30px'></div><div id='c' style='width:90px;height:30px'></div><div id='d' style='width:70px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.X - 80) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"c")!.ContentRect.X - 180) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"d")!.ContentRect.X - 270) < 2);
        }

        [Fact] public void FourItems_WithGap() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;gap:10px;width:400px'><div id='a' style='width:50px;height:30px'></div><div id='b' style='width:50px;height:30px'></div><div id='c' style='width:50px;height:30px'></div><div id='d' style='width:50px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.X - 60) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"c")!.ContentRect.X - 120) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"d")!.ContentRect.X - 180) < 2);
        }

        [Fact] public void FourItems_SpaceBetween() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;justify-content:space-between;width:400px'><div id='a' style='width:50px;height:30px'></div><div id='b' style='width:50px;height:30px'></div><div id='c' style='width:50px;height:30px'></div><div id='d' style='width:50px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"d")!.ContentRect.X - 350) < 2);
        }

        [Fact] public void FourItems_SpaceEvenly() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;justify-content:space-evenly;width:400px'><div id='a' style='width:50px;height:30px'></div><div id='b' style='width:50px;height:30px'></div><div id='c' style='width:50px;height:30px'></div><div id='d' style='width:50px;height:30px'></div></div></body>");
            float gap = (400 - 200) / 5f;
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.X - gap) < 2);
        }

        [Fact] public void FourItems_Center() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;justify-content:center;width:400px'><div id='a' style='width:50px;height:30px'></div><div id='b' style='width:50px;height:30px'></div><div id='c' style='width:50px;height:30px'></div><div id='d' style='width:50px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.X - 100) < 2);
        }

        [Fact] public void FourItems_FlexEnd() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;justify-content:flex-end;width:400px'><div id='a' style='width:50px;height:30px'></div><div id='b' style='width:50px;height:30px'></div><div id='c' style='width:50px;height:30px'></div><div id='d' style='width:50px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.X - 200) < 2);
        }

        [Fact] public void FourItems_Grow_1_1_2_2() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:600px'><div id='a' style='flex:1 0 0px;height:30px'></div><div id='b' style='flex:1 0 0px;height:30px'></div><div id='c' style='flex:2 0 0px;height:30px'></div><div id='d' style='flex:2 0 0px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"c")!.ContentRect.Width - 200) < 2);
        }

        [Fact] public void FourItems_Column() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-direction:column;width:200px'><div id='a' style='height:30px'></div><div id='b' style='height:40px'></div><div id='c' style='height:50px'></div><div id='d' style='height:20px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"d")!.ContentRect.Y - 120) < 2);
        }

        [Fact] public void FourItems_ColumnGrow() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-direction:column;width:200px;height:400px'><div id='a' style='flex:1'></div><div id='b' style='flex:1'></div><div id='c' style='flex:1'></div><div id='d' style='flex:1'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Height - 100) < 2);
        }

        [Fact] public void FourItems_Order() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:400px'><div id='a' style='order:4;width:50px;height:30px'></div><div id='b' style='order:2;width:50px;height:30px'></div><div id='c' style='order:1;width:50px;height:30px'></div><div id='d' style='order:3;width:50px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"c")!.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.X - 50) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"d")!.ContentRect.X - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.X - 150) < 2);
        }

        [Fact] public void FourItems_Mixed_FixedGrow() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:400px'><div id='a' style='flex:0 0 60px;height:30px'></div><div id='b' style='flex:1;height:30px'></div><div id='c' style='flex:0 0 60px;height:30px'></div><div id='d' style='flex:1;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Width - 60) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.Width - 140) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"c")!.ContentRect.Width - 60) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"d")!.ContentRect.Width - 140) < 2);
        }

        [Fact] public void FourItems_ColumnWithGap() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-direction:column;gap:10px;width:200px'><div id='a' style='height:30px'></div><div id='b' style='height:30px'></div><div id='c' style='height:30px'></div><div id='d' style='height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"d")!.ContentRect.Y - 120) < 2);
        }

        [Fact] public void FourItems_AllSameY() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:400px'><div id='a' style='width:50px;height:30px'></div><div id='b' style='width:50px;height:40px'></div><div id='c' style='width:50px;height:50px'></div><div id='d' style='width:50px;height:20px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Y - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"d")!.ContentRect.Y - 0) < 2);
        }
    }
}
