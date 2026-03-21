using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class WptFlexColumnWidthStretchTests
    {
        private readonly ITestOutputHelper _output;
        public WptFlexColumnWidthStretchTests(ITestOutputHelper output) { _output = output; }

        [Fact]
        public void StretchFillsContainer200()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-direction:column;width:200px'><div id='t' style='height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.Width - 200) < 2);
        }

        [Fact]
        public void StretchFillsContainer300()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-direction:column;width:300px'><div id='t' style='height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.Width - 300) < 2);
        }

        [Fact]
        public void StretchFillsContainer400()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-direction:column;width:400px'></div></body>", viewportWidth: 500);
            root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-direction:column;width:400px'><div id='t' style='height:30px'></div></div></body>", viewportWidth: 500);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.Width - 400) < 2);
        }

        [Fact]
        public void ExplicitWidthOverridesStretch()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-direction:column;width:200px'><div id='t' style='width:80px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.Width - 80) < 2);
        }

        [Fact]
        public void AlignItemsFlexStartWithExplicitWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-direction:column;align-items:flex-start;width:200px'><div id='t' style='width:80px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.Width - 80) < 2);
        }

        [Fact]
        public void AlignItemsCenterWithExplicitWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-direction:column;align-items:center;width:200px'><div id='t' style='width:80px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.X - 60) < 2);
        }

        [Fact]
        public void AlignItemsFlexEndWithExplicitWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-direction:column;align-items:flex-end;width:200px'><div id='t' style='width:80px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.X - 120) < 2);
        }

        [Fact]
        public void AlignSelfCenterOverridesStretch()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-direction:column;width:200px'><div id='t' style='align-self:center;width:80px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.X - 60) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.Width - 80) < 2);
        }

        [Fact]
        public void AlignSelfFlexEndOverridesStretch()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-direction:column;width:200px'><div id='t' style='align-self:flex-end;width:80px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.X - 120) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.Width - 80) < 2);
        }

        [Fact]
        public void AlignSelfStretchFromFlexStart()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-direction:column;align-items:flex-start;width:200px'><div id='t' style='align-self:stretch;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.Width - 200) < 2);
        }

        [Fact]
        public void StretchWithPaddingReducesContentWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-direction:column;width:200px'><div id='t' style='height:30px;padding:20px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.Width - 160) < 2);
        }

        [Fact]
        public void StretchWithBorderReducesContentWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-direction:column;width:200px'><div id='t' style='height:30px;border:10px solid black'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.Width - 180) < 2);
        }

        [Fact]
        public void StretchWithMarginReducesAvailableWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-direction:column;width:200px'><div id='t' style='height:30px;margin:20px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.Width - 160) < 2);
        }

        [Fact]
        public void WidthPercentage50Of200()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-direction:column;width:200px'><div id='t' style='width:50%;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.Width - 100) < 2);
        }

        [Fact]
        public void WidthPercentage100Fills()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-direction:column;width:200px'><div id='t' style='width:100%;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.Width - 200) < 2);
        }

        [Fact]
        public void MinWidthConstrainsItem()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-direction:column;align-items:center;width:200px'><div id='t' style='width:50px;min-width:150px;height:30px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(root, "t")!.ContentRect.Width >= 149);
        }

        [Fact]
        public void MaxWidthClampsStretchedItem()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-direction:column;width:200px'><div id='t' style='max-width:100px;height:30px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(root, "t")!.ContentRect.Width <= 101);
        }

        [Fact]
        public void BorderBoxWidthMatchesContainer()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-direction:column;width:200px'><div id='t' style='box-sizing:border-box;width:200px;padding:20px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.Width - 160) < 2);
        }

        [Fact]
        public void WidthAutoFillsContainer()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-direction:column;width:200px'><div id='t' style='width:auto;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.Width - 200) < 2);
        }

        [Fact]
        public void MarginAutoCentersOnCrossAxis()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-direction:column;width:200px'><div id='t' style='width:80px;height:30px;margin-left:auto;margin-right:auto'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.X - 60) < 2);
        }

        [Fact]
        public void TwoItemsBothStretch()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-direction:column;width:200px'><div id='a' style='height:30px'></div><div id='b' style='height:40px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.Width - 200) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.Width - 200) < 2);
        }

        [Fact]
        public void ThreeItemsAllStretch()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-direction:column;width:300px'><div id='a' style='height:20px'></div><div id='b' style='height:30px'></div><div id='c' style='height:40px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.Width - 300) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.Width - 300) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "c")!.ContentRect.Width - 300) < 2);
        }

        [Fact]
        public void StretchWithColumnGapDoesNotAffectWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-direction:column;width:200px;column-gap:20px'><div id='t' style='height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.Width - 200) < 2);
        }

        [Fact]
        public void StretchWithContainerPadding()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-direction:column;width:200px;padding:10px'><div id='t' style='height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.Width - 200) < 2);
        }

        [Fact]
        public void StretchWithContainerBorder()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-direction:column;width:200px;border:5px solid black'><div id='t' style='height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.Width - 200) < 2);
        }

        [Fact]
        public void StretchWithItemPaddingAndBorder()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-direction:column;width:200px'><div id='t' style='height:30px;padding:10px;border:5px solid black'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.Width - 170) < 2);
        }

        [Fact]
        public void MarginLeftAutoPushesRight()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-direction:column;width:200px'><div id='t' style='width:80px;height:30px;margin-left:auto'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.X - 120) < 2);
        }
    }
}
