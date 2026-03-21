using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class WptFlexboxTwoItemTests
    {
        private readonly ITestOutputHelper _output;
        public WptFlexboxTwoItemTests(ITestOutputHelper output) { _output = output; }

        [Fact] public void TwoFixed_Widths() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:300px'><div id='a' style='width:100px;height:30px'></div><div id='b' style='width:80px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.Width - 80) < 2);
        }

        [Fact] public void TwoFixed_Positions() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:300px'><div id='a' style='width:100px;height:30px'></div><div id='b' style='width:80px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.X - 100) < 2);
        }

        [Fact] public void TwoGrow1_EqualSplit() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:400px'><div id='a' style='flex:1;height:30px'></div><div id='b' style='flex:1;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Width - 200) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.Width - 200) < 2);
        }

        [Fact] public void Grow_1_2_Ratio() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:300px'><div id='a' style='flex:1;height:30px'></div><div id='b' style='flex:2;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.Width - 200) < 2);
        }

        [Fact] public void Fixed_Plus_Grow() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:300px'><div id='a' style='flex:0 0 80px;height:30px'></div><div id='b' style='flex:1;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Width - 80) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.Width - 220) < 2);
        }

        [Fact] public void TwoShrink_Equal() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:100px'><div id='a' style='flex:0 1 80px;min-width:0;height:30px'></div><div id='b' style='flex:0 1 80px;min-width:0;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Width - 50) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.Width - 50) < 2);
        }

        [Fact] public void TwoItems_Center() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;justify-content:center;width:400px'><div id='a' style='width:80px;height:30px'></div><div id='b' style='width:80px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.X - 120) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.X - 200) < 2);
        }

        [Fact] public void TwoItems_FlexEnd() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;justify-content:flex-end;width:400px'><div id='a' style='width:80px;height:30px'></div><div id='b' style='width:80px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.X - 240) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.X - 320) < 2);
        }

        [Fact] public void TwoItems_SpaceBetween() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;justify-content:space-between;width:400px'><div id='a' style='width:80px;height:30px'></div><div id='b' style='width:80px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.X - 320) < 2);
        }

        [Fact] public void TwoItems_WithGap() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;gap:20px;width:300px'><div id='a' style='width:80px;height:30px'></div><div id='b' style='width:80px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.X - 100) < 2);
        }

        [Fact] public void TwoItems_DifferentHeights_Stretch() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:300px'><div id='a' style='width:80px'></div><div id='b' style='width:80px;height:60px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Height - 60) < 2);
        }

        [Fact] public void TwoItems_Column() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-direction:column;width:200px'><div id='a' style='height:40px'></div><div id='b' style='height:50px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Y - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.Y - 40) < 2);
        }

        [Fact] public void TwoItems_MarginBetween() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:300px'><div id='a' style='width:80px;margin-right:30px;height:30px'></div><div id='b' style='width:80px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.X - 110) < 2);
        }

        [Fact] public void TwoItems_MarginAuto_Split() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:400px'><div id='a' style='width:80px;margin-right:auto;height:30px'></div><div id='b' style='width:80px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.X - 320) < 2);
        }

        [Fact] public void TwoItems_RowReverse() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-direction:row-reverse;width:300px'><div id='a' style='width:80px;height:30px'></div><div id='b' style='width:80px;height:30px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"a")!.ContentRect.X > LayoutTestHelper.FindById(r,"b")!.ContentRect.X);
        }
    }
}
