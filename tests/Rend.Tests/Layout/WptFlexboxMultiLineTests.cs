using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class WptFlexboxMultiLineTests
    {
        private readonly ITestOutputHelper _output;
        public WptFlexboxMultiLineTests(ITestOutputHelper output) { _output = output; }

        [Fact] public void Wrap_TwoLines() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-wrap:wrap;width:200px'><div id='a' style='width:120px;height:30px'></div><div id='b' style='width:120px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Y - 0) < 2);
            Assert.True(LayoutTestHelper.FindById(r,"b")!.ContentRect.Y >= 29);
        }

        [Fact] public void Wrap_ThreeLines() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-wrap:wrap;width:100px'><div id='a' style='width:80px;height:20px'></div><div id='b' style='width:80px;height:20px'></div><div id='c' style='width:80px;height:20px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Y - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.Y - 20) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"c")!.ContentRect.Y - 40) < 2);
        }

        [Fact] public void NoWrap_Default() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:100px'><div id='a' style='flex-shrink:0;width:60px;height:30px'></div><div id='b' style='flex-shrink:0;width:60px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Y - LayoutTestHelper.FindById(r,"b")!.ContentRect.Y) < 2);
        }

        [Fact] public void Wrap_ExactFit_NoWrap() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-wrap:wrap;width:200px'><div id='a' style='width:100px;height:30px'></div><div id='b' style='width:100px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Y - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.Y - 0) < 2);
        }

        [Fact] public void Wrap_TwoItemsPerLine() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-wrap:wrap;width:200px'><div id='a' style='width:80px;height:30px'></div><div id='b' style='width:80px;height:30px'></div><div id='c' style='width:80px;height:30px'></div><div id='d' style='width:80px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Y - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.Y - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"c")!.ContentRect.Y - 30) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"d")!.ContentRect.Y - 30) < 2);
        }

        [Fact] public void WrapReverse_ReversesCrossAxis() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-wrap:wrap-reverse;width:100px;height:100px'><div id='a' style='width:80px;height:30px'></div><div id='b' style='width:80px;height:30px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"a")!.ContentRect.Y > LayoutTestHelper.FindById(r,"b")!.ContentRect.Y);
        }

        [Fact] public void Wrap_AlignContent_FlexStart() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-wrap:wrap;align-content:flex-start;width:100px;height:200px'><div id='a' style='width:80px;height:30px'></div><div id='b' style='width:80px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Y - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.Y - 30) < 2);
        }

        [Fact] public void Wrap_AlignContent_FlexEnd() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-wrap:wrap;align-content:flex-end;width:100px;height:200px'><div id='a' style='width:80px;height:30px'></div><div id='b' style='width:80px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.Y - 170) < 2);
        }

        [Fact] public void Wrap_AlignContent_Center() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-wrap:wrap;align-content:center;width:100px;height:200px'><div id='a' style='width:80px;height:30px'></div><div id='b' style='width:80px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Y - 70) < 2);
        }

        [Fact] public void Wrap_AlignContent_SpaceBetween() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-wrap:wrap;align-content:space-between;width:100px;height:200px'><div id='a' style='width:80px;height:30px'></div><div id='b' style='width:80px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Y - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.Y - 170) < 2);
        }

        [Fact] public void Wrap_AlignContent_SpaceAround() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-wrap:wrap;align-content:space-around;width:100px;height:200px'><div id='a' style='width:80px;height:30px'></div><div id='b' style='width:80px;height:30px'></div></div></body>");
            float freeSpace = 200 - 60;
            float lineSpace = freeSpace / 2;
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Y - lineSpace / 2) < 2);
        }

        [Fact] public void Wrap_AlignContent_Stretch() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-wrap:wrap;align-content:stretch;width:100px;height:200px'><div id='a' style='width:80px'></div><div id='b' style='width:80px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Height - 100) < 2);
        }

        [Fact] public void Wrap_With_Gap() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-wrap:wrap;gap:10px;width:220px'><div id='a' style='width:100px;height:30px'></div><div id='b' style='width:100px;height:30px'></div><div id='c' style='width:100px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.X - 110) < 2);
            Assert.True(LayoutTestHelper.FindById(r,"c")!.ContentRect.Y >= 29);
        }

        [Fact] public void Wrap_DifferentHeights() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-wrap:wrap;width:200px'><div style='width:100px;height:50px'></div><div style='width:100px;height:30px'></div><div id='t' style='width:100px;height:20px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 50) < 2);
        }

        [Fact] public void Wrap_GrowOnLine() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-wrap:wrap;width:200px'><div id='a' style='flex-grow:1;width:80px;height:30px'></div><div id='b' style='flex-grow:1;width:80px;height:30px'></div><div id='c' style='width:80px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.Width - 100) < 2);
        }

        [Fact] public void Wrap_Column() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-flow:column wrap;width:200px;height:100px'><div id='a' style='width:50px;height:60px'></div><div id='b' style='width:50px;height:60px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"b")!.ContentRect.X >= 49);
        }

        [Fact] public void SingleItem_NoWrap() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-wrap:wrap;width:200px'><div id='t' style='width:80px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.X - 0) < 2);
        }

        [Fact] public void Wrap_PercentWidth_Items() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-wrap:wrap;width:200px'><div id='a' style='width:60%;height:30px'></div><div id='b' style='width:60%;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Y - 0) < 2);
            Assert.True(LayoutTestHelper.FindById(r,"b")!.ContentRect.Y >= 29);
        }
    }
}
