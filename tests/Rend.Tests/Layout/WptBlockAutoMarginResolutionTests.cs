using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class WptBlockAutoMarginResolutionTests
    {
        private readonly ITestOutputHelper _output;
        public WptBlockAutoMarginResolutionTests(ITestOutputHelper output) { _output = output; }

        // --- Both auto margins center the element ---

        [Fact]
        public void BothAutoMargins_CentersIn400pxViewport()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div id='t' style='width:200px;height:30px;margin-left:auto;margin-right:auto'></div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(box.ContentRect.X - 100) < 2);
        }

        [Fact]
        public void MarginZeroAutoShorthand_CentersIn400pxViewport()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div id='t' style='width:200px;height:30px;margin:0 auto'></div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(box.ContentRect.X - 100) < 2);
        }

        [Fact]
        public void MarginAutoShorthand_AllFour_CentersHorizontally()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div id='t' style='width:200px;height:30px;margin:auto'></div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(box.ContentRect.X - 100) < 2);
        }

        [Fact]
        public void MarginAutoFourValue_ZeroAutoZeroAuto_Centers()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div id='t' style='width:200px;height:30px;margin:0 auto 0 auto'></div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(box.ContentRect.X - 100) < 2);
        }

        // --- margin-left:auto pushes right ---

        [Fact]
        public void MarginLeftAuto_PushesToRightEdge()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div id='t' style='width:200px;height:30px;margin-left:auto'></div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(box.ContentRect.X - 200) < 2);
        }

        // --- margin-right:auto pushes left ---

        [Fact]
        public void MarginRightAuto_StaysAtLeftEdge()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div id='t' style='width:200px;height:30px;margin-right:auto'></div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(box.ContentRect.X - 0) < 2);
        }

        // --- auto with explicit width in different container widths ---

        [Fact]
        public void AutoMargin_In200pxContainer_CentersChild()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='width:200px'><div id='t' style='width:100px;height:30px;margin:0 auto'></div></div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(box.ContentRect.X - 50) < 2);
        }

        [Fact]
        public void AutoMargin_In300pxContainer_CentersChild()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='width:300px'><div id='t' style='width:100px;height:30px;margin:0 auto'></div></div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(box.ContentRect.X - 100) < 2);
        }

        [Fact]
        public void AutoMargin_In400pxContainer_CentersChild()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='width:400px'><div id='t' style='width:100px;height:30px;margin:0 auto'></div></div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(box.ContentRect.X - 150) < 2);
        }

        [Fact]
        public void AutoMargin_In500pxContainer_CentersChild()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='width:500px'><div id='t' style='width:100px;height:30px;margin:0 auto'></div></div></body>",
                viewportWidth: 600);
            var box = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(box.ContentRect.X - 200) < 2);
        }

        // --- auto with percentage width ---

        [Fact]
        public void AutoMargin_WithPercentageWidth_Centers()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div id='t' style='width:50%;height:30px;margin:0 auto'></div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(box.ContentRect.Width - 200) < 2);
            Assert.True(System.Math.Abs(box.ContentRect.X - 100) < 2);
        }

        // --- auto with no width (auto width) has no centering effect ---

        [Fact]
        public void AutoMargin_NoWidth_FillsContainer()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div id='t' style='height:30px;margin:0 auto'></div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(box.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(box.ContentRect.Width - 400) < 2);
        }

        // --- margin:auto with padding ---

        [Fact]
        public void AutoMargin_WithPadding_CentersIncludingPadding()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div id='t' style='width:200px;height:30px;padding:10px;margin:0 auto'></div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            float borderBoxWidth = box.ContentRect.Width + box.PaddingLeft + box.PaddingRight;
            float expectedX = (400 - borderBoxWidth) / 2 + box.PaddingLeft;
            Assert.True(System.Math.Abs(box.ContentRect.X - expectedX) < 2);
            Assert.True(System.Math.Abs(box.ContentRect.Width - 200) < 2);
        }

        // --- margin:auto with border ---

        [Fact]
        public void AutoMargin_WithBorder_CentersIncludingBorder()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div id='t' style='width:200px;height:30px;border:5px solid black;margin:0 auto'></div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            float borderBoxWidth = box.ContentRect.Width + box.PaddingLeft + box.PaddingRight
                                   + box.BorderLeftWidth + box.BorderRightWidth;
            float expectedX = (400 - borderBoxWidth) / 2 + box.PaddingLeft + box.BorderLeftWidth;
            Assert.True(System.Math.Abs(box.ContentRect.X - expectedX) < 2);
            Assert.True(System.Math.Abs(box.ContentRect.Width - 200) < 2);
        }

        // --- margin:auto with border-box ---

        [Fact]
        public void AutoMargin_WithBorderBox_CentersCorrectly()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div id='t' style='box-sizing:border-box;width:200px;height:60px;padding:10px;border:5px solid black;margin:0 auto'></div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            float borderBoxWidth = box.ContentRect.Width + box.PaddingLeft + box.PaddingRight
                                   + box.BorderLeftWidth + box.BorderRightWidth;
            Assert.True(System.Math.Abs(borderBoxWidth - 200) < 2);
            float expectedX = (400 - 200) / 2 + box.PaddingLeft + box.BorderLeftWidth;
            Assert.True(System.Math.Abs(box.ContentRect.X - expectedX) < 2);
        }

        // --- margin auto in nested block ---

        [Fact]
        public void AutoMargin_NestedBlock_CentersWithinParent()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='width:300px;margin:0 auto'><div id='t' style='width:100px;height:30px;margin:0 auto'></div></div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            float parentX = 50;
            float expectedX = parentX + (300 - 100) / 2;
            Assert.True(System.Math.Abs(box.ContentRect.X - expectedX) < 2);
        }

        // --- margin auto in flex item (cross axis) ---

        [Fact]
        public void AutoMargin_FlexItemCrossAxis_CentersVertically()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:flex;height:200px;width:400px'>
                    <div id='t' style='width:100px;height:50px;margin:auto 0'></div>
                  </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            float expectedY = (200 - 50) / 2;
            Assert.True(System.Math.Abs(box.ContentRect.Y - expectedY) < 2);
        }

        [Fact]
        public void AutoMargin_FlexItemAllAuto_CentersBothAxes()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:flex;height:200px;width:400px'>
                    <div id='t' style='width:100px;height:50px;margin:auto'></div>
                  </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            float expectedY = (200 - 50) / 2;
            Assert.True(System.Math.Abs(box.ContentRect.Y - expectedY) < 2);
        }

        // --- margin auto in grid item ---

        [Fact]
        public void AutoMargin_GridItem_CentersHorizontally()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:400px;width:400px'>
                    <div id='t' style='width:100px;height:30px;margin:0 auto'></div>
                  </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            float expectedX = (400 - 100) / 2;
            Assert.True(System.Math.Abs(box.ContentRect.X - expectedX) < 2);
        }

        [Fact]
        public void AutoMargin_GridItem_CentersVertically()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-rows:200px;grid-template-columns:400px;width:400px'>
                    <div id='t' style='width:100px;height:50px;margin:auto 0'></div>
                  </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            float expectedY = (200 - 50) / 2;
            Assert.True(System.Math.Abs(box.ContentRect.Y - expectedY) < 2);
        }

        // --- wide element with auto margin (wider than container) ---

        [Fact]
        public void AutoMargin_WiderThanContainer_NoNegativeMargin()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div id='t' style='width:500px;height:30px;margin:0 auto'></div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(box.ContentRect.X - 0) < 2);
        }

        [Fact]
        public void AutoMargin_ExactlyContainerWidth_ZeroMargins()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div id='t' style='width:400px;height:30px;margin:0 auto'></div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(box.ContentRect.X - 0) < 2);
        }

        // --- margin auto with min-width ---

        [Fact]
        public void AutoMargin_WithMinWidth_UsesMinWidthForCentering()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div id='t' style='width:100px;min-width:200px;height:30px;margin:0 auto'></div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(box.ContentRect.Width - 200) < 2);
            Assert.True(System.Math.Abs(box.ContentRect.X - 100) < 2);
        }

        // --- margin auto with max-width ---

        [Fact]
        public void AutoMargin_WithMaxWidth_UsesMaxWidthForCentering()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div id='t' style='width:300px;max-width:200px;height:30px;margin:0 auto'></div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(box.ContentRect.Width - 200) < 2);
            Assert.True(System.Math.Abs(box.ContentRect.X - 100) < 2);
        }

        // --- margin-left:auto with explicit margin-right ---

        [Fact]
        public void MarginLeftAuto_WithExplicitMarginRight_AbsorbsRemaining()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div id='t' style='width:200px;height:30px;margin-left:auto;margin-right:50px'></div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(box.ContentRect.X - 150) < 2);
        }

        // --- margin-right:auto with explicit margin-left ---

        [Fact]
        public void MarginRightAuto_WithExplicitMarginLeft_LeftMarginApplied()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div id='t' style='width:200px;height:30px;margin-left:50px;margin-right:auto'></div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(box.ContentRect.X - 50) < 2);
        }

        // --- margin:auto with padding and border combined ---

        [Fact]
        public void AutoMargin_WithPaddingAndBorder_CentersCorrectly()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div id='t' style='width:160px;height:30px;padding:10px;border:5px solid black;margin:0 auto'></div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            float borderBoxWidth = 160 + 20 + 10;
            float expectedContentX = (400 - borderBoxWidth) / 2 + 5 + 10;
            Assert.True(System.Math.Abs(box.ContentRect.X - expectedContentX) < 2);
        }

        // --- deeply nested auto margin ---

        [Fact]
        public void AutoMargin_DeeplyNested_CentersWithinImmediateParent()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                    <div style='width:400px'>
                      <div style='width:300px;margin:0 auto'>
                        <div id='t' style='width:100px;height:30px;margin:0 auto'></div>
                      </div>
                    </div>
                  </body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            float outerParentX = (400 - 300) / 2;
            float expectedX = outerParentX + (300 - 100) / 2;
            Assert.True(System.Math.Abs(box.ContentRect.X - expectedX) < 2);
        }

        // --- margin:auto vertical has no effect in block flow ---

        [Fact]
        public void MarginTopAuto_InBlockFlow_ResolvesToZero()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div id='t' style='width:200px;height:30px;margin-top:auto'></div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(box.ContentRect.Y - 0) < 2);
        }

        [Fact]
        public void MarginBottomAuto_InBlockFlow_ResolvesToZero()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                    <div style='height:50px'></div>
                    <div id='t' style='width:200px;height:30px;margin-bottom:auto'></div>
                    <div id='after' style='height:20px'></div>
                  </body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            var afterBox = LayoutTestHelper.FindById(root, "after")!;
            Assert.True(System.Math.Abs(box.ContentRect.Y - 50) < 2);
            Assert.True(System.Math.Abs(afterBox.ContentRect.Y - 80) < 2);
        }
    }
}
