using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class WptFlexboxStretchTests
    {
        private readonly ITestOutputHelper _output;
        public WptFlexboxStretchTests(ITestOutputHelper output) { _output = output; }

        [Fact] public void Stretch_FillsContainerHeight() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;height:200px;width:300px'><div id='t' style='width:100px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height - 200) < 2);
        }

        [Fact] public void Stretch_TwoItems_BothFill() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;height:150px;width:300px'><div id='a' style='width:100px'></div><div id='b' style='width:100px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Height - 150) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.Height - 150) < 2);
        }

        [Fact] public void Stretch_ExplicitHeight_NoStretch() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;height:200px;width:300px'><div id='t' style='width:100px;height:50px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height - 50) < 2);
        }

        [Fact] public void Stretch_WithPadding() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;height:100px;width:300px'><div id='t' style='width:100px;padding:10px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height + 20 - 100) < 2);
        }

        [Fact] public void Stretch_WithBorder() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;height:100px;width:300px'><div id='t' style='width:100px;border:5px solid'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height + 10 - 100) < 2);
        }

        [Fact] public void Stretch_WithMargin() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;height:100px;width:300px'><div id='t' style='width:100px;margin:10px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height - 80) < 2);
        }

        [Fact] public void Stretch_WithMaxHeight() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;height:200px;width:300px'><div id='t' style='width:100px;max-height:80px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height <= 81);
        }

        [Fact] public void Stretch_WithMinHeight() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;height:50px;width:300px'><div id='t' style='width:100px;min-height:100px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height >= 99);
        }

        [Fact] public void AlignFlexStart_NoStretch() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;align-items:flex-start;height:200px;width:300px'><div id='t' style='width:100px;height:50px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height - 50) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 0) < 2);
        }

        [Fact] public void AlignCenter_NoStretch() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;align-items:center;height:200px;width:300px'><div id='t' style='width:100px;height:50px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height - 50) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 75) < 2);
        }

        [Fact] public void AlignFlexEnd_NoStretch() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;align-items:flex-end;height:200px;width:300px'><div id='t' style='width:100px;height:50px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 150) < 2);
        }

        [Fact] public void AlignSelf_Stretch_Override() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;align-items:flex-start;height:200px;width:300px'><div id='t' style='align-self:stretch;width:100px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height - 200) < 2);
        }

        [Fact] public void Column_Stretch_FillsWidth() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-direction:column;width:300px'><div id='t' style='height:50px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 300) < 2);
        }

        [Fact] public void Column_ExplicitWidth_NoStretch() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-direction:column;width:300px'><div id='t' style='width:100px;height:50px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 100) < 2);
        }

        [Fact] public void Stretch_AutoHeight_TallestItem() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:300px'><div id='a' style='width:100px'></div><div style='width:100px;height:80px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Height - 80) < 2);
        }

        [Fact] public void Stretch_BorderBox() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;height:100px;width:300px'><div id='t' style='box-sizing:border-box;width:100px;padding:15px;border:5px solid'></div></div></body>");
            float totalHeight = LayoutTestHelper.FindById(r,"t")!.ContentRect.Height + 30 + 10;
            Assert.True(System.Math.Abs(totalHeight - 100) < 2);
        }
    }
}
