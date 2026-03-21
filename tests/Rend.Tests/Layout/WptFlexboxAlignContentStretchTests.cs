using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class WptFlexboxAlignContentStretchTests
    {
        private readonly ITestOutputHelper _output;
        public WptFlexboxAlignContentStretchTests(ITestOutputHelper output) { _output = output; }

        [Fact] public void Stretch_TwoLines_EqualHeight() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-wrap:wrap;align-content:stretch;width:100px;height:200px'><div id='a' style='width:80px'></div><div id='b' style='width:80px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Height - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.Height - 100) < 2);
        }

        [Fact] public void Stretch_ThreeLines_EqualHeight() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-wrap:wrap;align-content:stretch;width:100px;height:300px'><div id='a' style='width:80px'></div><div id='b' style='width:80px'></div><div id='c' style='width:80px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Height - 100) < 2);
        }

        [Fact] public void Stretch_ExplicitHeight_NoStretch() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-wrap:wrap;align-content:stretch;width:100px;height:200px'><div id='t' style='width:80px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height - 30) < 2);
        }

        [Fact] public void Stretch_Default_Behavior() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-wrap:wrap;width:100px;height:200px'><div id='a' style='width:80px'></div><div id='b' style='width:80px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Height - 100) < 2);
        }

        [Fact] public void Stretch_NoFreeSpace() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-wrap:wrap;align-content:stretch;width:100px;height:60px'><div id='a' style='width:80px;height:30px'></div><div id='b' style='width:80px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Height - 30) < 2);
        }

        [Fact] public void Stretch_WithGap() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-wrap:wrap;align-content:stretch;row-gap:20px;width:100px;height:220px'><div id='a' style='width:80px'></div><div id='b' style='width:80px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Height - 100) < 2);
        }

        [Fact] public void Stretch_AutoHeight_NoStretch() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='flex' style='display:flex;flex-wrap:wrap;align-content:stretch;width:100px'><div style='width:80px;height:30px'></div><div style='width:80px;height:40px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"flex")!.ContentRect.Height - 70) < 2);
        }

        [Fact] public void Stretch_SingleLine_FillsContainer() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-wrap:wrap;align-content:stretch;width:300px;height:200px'><div id='t' style='width:80px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height - 200) < 2);
        }

        [Fact] public void Stretch_MixedHeights() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-wrap:wrap;align-content:stretch;width:100px;height:200px'><div style='width:80px;height:40px'></div><div id='b' style='width:80px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"b")!.ContentRect.Height > 0);
        }

        [Fact] public void FlexStart_NoStretch() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-wrap:wrap;align-content:flex-start;width:100px;height:200px'><div id='a' style='width:80px;height:30px'></div><div id='b' style='width:80px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Height - 30) < 2);
        }

        [Fact] public void FlexEnd_TwoLines() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-wrap:wrap;align-content:flex-end;width:100px;height:200px'><div id='a' style='width:80px;height:30px'></div><div id='b' style='width:80px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Y - 140) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.Y - 170) < 2);
        }

        [Fact] public void Center_TwoLines() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-wrap:wrap;align-content:center;width:100px;height:200px'><div id='a' style='width:80px;height:30px'></div><div id='b' style='width:80px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Y - 70) < 2);
        }

        [Fact] public void SpaceBetween_TwoLines() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-wrap:wrap;align-content:space-between;width:100px;height:200px'><div id='a' style='width:80px;height:30px'></div><div id='b' style='width:80px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Y - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.Y - 170) < 2);
        }

        [Fact] public void SpaceAround_TwoLines() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-wrap:wrap;align-content:space-around;width:100px;height:200px'><div id='a' style='width:80px;height:30px'></div><div id='b' style='width:80px;height:30px'></div></div></body>");
            float space = (200 - 60) / 2f;
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Y - space / 2) < 2);
        }

        [Fact] public void SpaceEvenly_TwoLines() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-wrap:wrap;align-content:space-evenly;width:100px;height:200px'><div id='a' style='width:80px;height:30px'></div><div id='b' style='width:80px;height:30px'></div></div></body>");
            float gap = (200 - 60) / 3f;
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Y - gap) < 3);
        }
    }
}
