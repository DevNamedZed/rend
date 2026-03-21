using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class WptFlexboxContainerAutoHeightTests
    {
        private readonly ITestOutputHelper _output;
        public WptFlexboxContainerAutoHeightTests(ITestOutputHelper output) { _output = output; }

        [Fact] public void Row_AutoHeight_SingleItem() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='f' style='display:flex;width:200px'><div style='width:50px;height:60px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"f")!.ContentRect.Height - 60) < 2);
        }

        [Fact] public void Row_AutoHeight_TallestItem() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='f' style='display:flex;width:200px'><div style='width:50px;height:30px'></div><div style='width:50px;height:80px'></div><div style='width:50px;height:50px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"f")!.ContentRect.Height - 80) < 2);
        }

        [Fact] public void Column_AutoHeight_Sum() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='f' style='display:flex;flex-direction:column;width:200px'><div style='height:30px'></div><div style='height:40px'></div><div style='height:50px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"f")!.ContentRect.Height - 120) < 2);
        }

        [Fact] public void Column_AutoHeight_WithGap() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='f' style='display:flex;flex-direction:column;gap:10px;width:200px'><div style='height:30px'></div><div style='height:30px'></div><div style='height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"f")!.ContentRect.Height - 110) < 2);
        }

        [Fact] public void Row_AutoHeight_Empty() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='f' style='display:flex;width:200px'></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"f")!.ContentRect.Height < 2);
        }

        [Fact] public void Row_ExplicitHeight_Overrides() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='f' style='display:flex;width:200px;height:150px'><div style='width:50px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"f")!.ContentRect.Height - 150) < 2);
        }

        [Fact] public void Row_MinHeight_Enforced() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='f' style='display:flex;width:200px;min-height:100px'><div style='width:50px;height:30px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"f")!.ContentRect.Height >= 99);
        }

        [Fact] public void Row_MaxHeight_Clamps() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='f' style='display:flex;width:200px;height:200px;max-height:100px'><div style='width:50px;height:30px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"f")!.ContentRect.Height <= 101);
        }

        [Fact] public void Row_AutoHeight_AbsposExcluded() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='f' style='display:flex;position:relative;width:200px'><div style='width:50px;height:50px'></div><div style='position:absolute;width:100px;height:500px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"f")!.ContentRect.Height - 50) < 2);
        }

        [Fact] public void Wrap_AutoHeight_SumLines() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='f' style='display:flex;flex-wrap:wrap;width:100px'><div style='width:80px;height:30px'></div><div style='width:80px;height:40px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"f")!.ContentRect.Height - 70) < 2);
        }

        [Fact] public void Wrap_AutoHeight_WithRowGap() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='f' style='display:flex;flex-wrap:wrap;row-gap:10px;width:100px'><div style='width:80px;height:30px'></div><div style='width:80px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"f")!.ContentRect.Height - 70) < 2);
        }

        [Fact] public void Row_AutoHeight_WithPadding() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='f' style='display:flex;width:200px;padding:20px'><div style='width:50px;height:40px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"f")!.ContentRect.Height - 40) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"f")!.PaddingTop - 20) < 2);
        }

        [Fact] public void Row_AutoHeight_WithBorder() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='f' style='display:flex;width:200px;border:10px solid'><div style='width:50px;height:40px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"f")!.ContentRect.Height - 40) < 2);
        }

        [Fact] public void Column_AutoHeight_TwoItems() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='f' style='display:flex;flex-direction:column;width:200px'><div style='height:60px'></div><div style='height:40px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"f")!.ContentRect.Height - 100) < 2);
        }

        [Fact] public void Column_AutoHeight_FourItems() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='f' style='display:flex;flex-direction:column;width:200px'><div style='height:20px'></div><div style='height:30px'></div><div style='height:40px'></div><div style='height:50px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"f")!.ContentRect.Height - 140) < 2);
        }
    }
}
