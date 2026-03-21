using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class WptFlexboxAlignContentTests
    {
        private readonly ITestOutputHelper _output;
        public WptFlexboxAlignContentTests(ITestOutputHelper output) { _output = output; }

        [Fact] public void AlignContent_FlexStart() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-wrap:wrap;align-content:flex-start;width:100px;height:200px'><div id='a' style='width:80px;height:30px'></div><div id='b' style='width:80px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Y - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.Y - 30) < 2);
        }

        [Fact] public void AlignContent_FlexEnd() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-wrap:wrap;align-content:flex-end;width:100px;height:200px'><div id='a' style='width:80px;height:30px'></div><div id='b' style='width:80px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Y - 140) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.Y - 170) < 2);
        }

        [Fact] public void AlignContent_Center() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-wrap:wrap;align-content:center;width:100px;height:200px'><div id='a' style='width:80px;height:30px'></div><div id='b' style='width:80px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Y - 70) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.Y - 100) < 2);
        }

        [Fact] public void AlignContent_SpaceBetween() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-wrap:wrap;align-content:space-between;width:100px;height:200px'><div id='a' style='width:80px;height:30px'></div><div id='b' style='width:80px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Y - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.Y - 170) < 2);
        }

        [Fact] public void AlignContent_SpaceAround() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-wrap:wrap;align-content:space-around;width:100px;height:200px'><div id='a' style='width:80px;height:30px'></div><div id='b' style='width:80px;height:30px'></div></div></body>");
            float freeSpace = 200 - 60;
            float lineSpace = freeSpace / 2;
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Y - lineSpace / 2) < 2);
        }

        [Fact] public void AlignContent_SpaceEvenly() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-wrap:wrap;align-content:space-evenly;width:100px;height:200px'><div id='a' style='width:80px;height:30px'></div><div id='b' style='width:80px;height:30px'></div></div></body>");
            float gap = (200 - 60) / 3f;
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Y - gap) < 3);
        }

        [Fact] public void AlignContent_Stretch() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-wrap:wrap;align-content:stretch;width:100px;height:200px'><div id='a' style='width:80px'></div><div id='b' style='width:80px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Height - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.Height - 100) < 2);
        }

        [Fact] public void AlignContent_ThreeLines_Center() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-wrap:wrap;align-content:center;width:100px;height:300px'><div id='a' style='width:80px;height:30px'></div><div id='b' style='width:80px;height:30px'></div><div id='c' style='width:80px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Y - 105) < 2);
        }

        [Fact] public void AlignContent_ThreeLines_SpaceBetween() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-wrap:wrap;align-content:space-between;width:100px;height:300px'><div id='a' style='width:80px;height:30px'></div><div id='b' style='width:80px;height:30px'></div><div id='c' style='width:80px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Y - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"c")!.ContentRect.Y - 270) < 2);
        }

        [Fact] public void AlignContent_NoWrap_Ignored() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;align-content:center;width:300px;height:200px'><div id='t' style='width:100px;height:50px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 0) < 2);
        }

        [Fact] public void AlignContent_SingleLine_FlexStart() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-wrap:wrap;align-content:flex-start;width:300px;height:200px'><div id='t' style='width:100px;height:50px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 0) < 2);
        }

        [Fact] public void AlignContent_With_Gap() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-wrap:wrap;align-content:flex-start;row-gap:10px;width:100px;height:200px'><div id='a' style='width:80px;height:30px'></div><div id='b' style='width:80px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.Y - 40) < 2);
        }

        [Fact] public void AlignContent_Stretch_ExplicitHeight() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-wrap:wrap;align-content:stretch;width:100px;height:200px'><div id='t' style='width:80px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height - 30) < 2);
        }
    }
}
