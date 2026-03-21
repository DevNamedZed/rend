using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class WptFlexboxGapTests
    {
        private readonly ITestOutputHelper _output;
        public WptFlexboxGapTests(ITestOutputHelper output) { _output = output; }

        [Fact] public void Gap_Row_20px() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;gap:20px;width:300px'><div id='a' style='width:50px;height:30px'></div><div id='b' style='width:50px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.X - 70) < 2);
        }

        [Fact] public void Gap_Row_10px_ThreeItems() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;gap:10px;width:300px'><div id='a' style='width:50px;height:30px'></div><div id='b' style='width:50px;height:30px'></div><div id='c' style='width:50px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.X - 60) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"c")!.ContentRect.X - 120) < 2);
        }

        [Fact] public void Gap_Column_15px() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-direction:column;gap:15px;width:200px'><div id='a' style='height:30px'></div><div id='b' style='height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.Y - 45) < 2);
        }

        [Fact] public void RowGap_And_ColumnGap() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-wrap:wrap;row-gap:10px;column-gap:20px;width:220px'><div id='a' style='width:100px;height:30px'></div><div id='b' style='width:100px;height:30px'></div><div id='c' style='width:100px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.X - 120) < 2);
            Assert.True(LayoutTestHelper.FindById(r,"c")!.ContentRect.Y >= 39);
        }

        [Fact] public void Gap_0_NoEffect() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;gap:0;width:300px'><div id='a' style='width:50px;height:30px'></div><div id='b' style='width:50px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.X - 50) < 2);
        }

        [Fact] public void Gap_WithFlexGrow() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;gap:20px;width:220px'><div id='a' style='flex:1;height:30px'></div><div id='b' style='flex:1;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.Width - 100) < 2);
        }

        [Fact] public void Gap_SingleItem_NoGap() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;gap:20px;width:300px'><div id='t' style='width:100px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.X - 0) < 2);
        }

        [Fact] public void Gap_Percent() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;gap:10%;width:200px'><div id='a' style='width:50px;height:30px'></div><div id='b' style='width:50px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.X - 70) < 2);
        }

        [Fact] public void Gap_RowReverse() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-direction:row-reverse;gap:20px;width:300px'><div id='a' style='width:50px;height:30px'></div><div id='b' style='width:50px;height:30px'></div></div></body>");
            float gapBetween = LayoutTestHelper.FindById(r,"a")!.ContentRect.X - (LayoutTestHelper.FindById(r,"b")!.ContentRect.X + 50);
            Assert.True(System.Math.Abs(gapBetween - 20) < 2);
        }

        [Fact] public void Gap_ColumnReverse() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-direction:column-reverse;gap:15px;width:200px;height:200px'><div id='a' style='height:30px'></div><div id='b' style='height:30px'></div></div></body>");
            float gapBetween = LayoutTestHelper.FindById(r,"a")!.ContentRect.Y - (LayoutTestHelper.FindById(r,"b")!.ContentRect.Y + 30);
            Assert.True(System.Math.Abs(gapBetween - 15) < 2);
        }

        [Fact] public void Gap_SpaceBetween_Adds() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;justify-content:space-between;gap:10px;width:300px'><div id='a' style='width:50px;height:30px'></div><div id='b' style='width:50px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.X - 250) < 2);
        }

        [Fact] public void Gap_Center_Reduces_FreeSpace() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;justify-content:center;gap:20px;width:300px'><div id='a' style='width:50px;height:30px'></div><div id='b' style='width:50px;height:30px'></div></div></body>");
            float totalUsed = 50 + 20 + 50;
            float expectedOffset = (300 - totalUsed) / 2;
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.X - expectedOffset) < 2);
        }

        [Fact] public void Gap_Large_PushesItems() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;gap:100px;width:300px'><div id='a' style='width:50px;height:30px'></div><div id='b' style='width:50px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.X - 150) < 2);
        }

        [Fact] public void ColumnGap_Only() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;column-gap:30px;width:300px'><div id='a' style='width:50px;height:30px'></div><div id='b' style='width:50px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.X - 80) < 2);
        }
    }
}
