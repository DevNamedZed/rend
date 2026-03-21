using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class WptFlexboxColumnGapTests
    {
        private readonly ITestOutputHelper _output;
        public WptFlexboxColumnGapTests(ITestOutputHelper output) { _output = output; }

        [Fact] public void ColumnGap_10px() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-direction:column;gap:10px;width:200px'><div id='a' style='height:30px'></div><div id='b' style='height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.Y - 40) < 2);
        }

        [Fact] public void ColumnGap_20px_Three() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-direction:column;gap:20px;width:200px'><div id='a' style='height:30px'></div><div id='b' style='height:30px'></div><div id='c' style='height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.Y - 50) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"c")!.ContentRect.Y - 100) < 2);
        }

        [Fact] public void RowGap_15px() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;gap:15px;width:300px'><div id='a' style='width:50px;height:30px'></div><div id='b' style='width:50px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.X - 65) < 2);
        }

        [Fact] public void ColumnGap_WithGrow() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-direction:column;gap:20px;width:200px;height:220px'><div id='a' style='flex:1'></div><div id='b' style='flex:1'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Height - 100) < 2);
        }

        [Fact] public void RowGap_WithGrow() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;gap:20px;width:220px'><div id='a' style='flex:1;height:30px'></div><div id='b' style='flex:1;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Width - 100) < 2);
        }

        [Fact] public void Gap_SingleItem() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;gap:20px;width:300px'><div id='t' style='width:100px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.X - 0) < 2);
        }

        [Fact] public void Gap_Zero() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;gap:0;width:300px'><div id='a' style='width:50px;height:30px'></div><div id='b' style='width:50px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.X - 50) < 2);
        }

        [Fact] public void ColumnGap_AutoHeight() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='flex' style='display:flex;flex-direction:column;gap:10px;width:200px'><div style='height:30px'></div><div style='height:30px'></div><div style='height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"flex")!.ContentRect.Height - 110) < 2);
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

        [Fact] public void Gap_WithSpaceBetween() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;justify-content:space-between;gap:10px;width:300px'><div id='a' style='width:50px;height:30px'></div><div id='b' style='width:50px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.X - 250) < 2);
        }

        [Fact] public void Gap_WithCenter() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;justify-content:center;gap:20px;width:300px'><div id='a' style='width:50px;height:30px'></div><div id='b' style='width:50px;height:30px'></div></div></body>");
            float totalUsed = 50 + 20 + 50;
            float offset = (300 - totalUsed) / 2;
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.X - offset) < 2);
        }

        [Fact] public void RowGap_And_ColumnGap_Different() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-wrap:wrap;row-gap:15px;column-gap:25px;width:200px'><div id='a' style='width:80px;height:30px'></div><div id='b' style='width:80px;height:30px'></div><div id='c' style='width:80px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.X - 105) < 2);
            Assert.True(LayoutTestHelper.FindById(r,"c")!.ContentRect.Y >= 44);
        }

        [Fact] public void Gap_FourItems_ThreeGaps() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;gap:10px;width:400px'><div id='a' style='width:50px;height:30px'></div><div id='b' style='width:50px;height:30px'></div><div id='c' style='width:50px;height:30px'></div><div id='d' style='width:50px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"d")!.ContentRect.X - 180) < 2);
        }

        [Fact] public void Gap_Large_ExceedsContainer() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;gap:200px;width:300px'><div id='a' style='width:50px;height:30px'></div><div id='b' style='width:50px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.X - 250) < 2);
        }
    }
}
