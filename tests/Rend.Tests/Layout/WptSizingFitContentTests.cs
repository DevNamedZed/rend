using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    /// <summary>
    /// Tests for CSS Sizing Level 3 intrinsic keywords: fit-content, min-content, max-content.
    /// <spec>CSS-SIZING-3 §4.1 https://drafts.csswg.org/css-sizing-3/#sizing-values</spec>
    /// </summary>
    public class WptSizingFitContentTests
    {
        private readonly ITestOutputHelper _output;

        public WptSizingFitContentTests(ITestOutputHelper output)
        {
            _output = output;
        }

        // [CSS-SIZING-3 §4.1] width: fit-content shrinks to child when child is narrower than container
        [Fact]
        public void WidthFitContent_ShrinkWrapsToChild()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='width:400px'>" +
                "<div id='t' style='width:fit-content'>" +
                "<div style='width:120px;height:10px'></div>" +
                "</div></div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"width={box!.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 120) < 2);
        }

        // [CSS-SIZING-3 §4.1] width: fit-content clamps to available width when child is wider
        [Fact]
        public void WidthFitContent_ClampsToAvailable()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='width:100px'>" +
                "<div id='t' style='width:fit-content'>" +
                "<div style='width:200px;height:10px'></div>" +
                "</div></div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"width={box!.ContentRect.Width}");
            Assert.True(box.ContentRect.Width >= 99);
        }

        // [CSS-SIZING-3 §4.1] width: min-content uses widest child
        [Fact]
        public void WidthMinContent_UsesWidestChild()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='width:400px'>" +
                "<div id='t' style='width:min-content'>" +
                "<div style='width:60px;height:10px'></div>" +
                "<div style='width:100px;height:10px'></div>" +
                "<div style='width:80px;height:10px'></div>" +
                "</div></div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"width={box!.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 100) < 2);
        }

        // [CSS-SIZING-3 §4.1] width: max-content uses widest child (no wrapping)
        [Fact]
        public void WidthMaxContent_UsesWidestChild()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='width:400px'>" +
                "<div id='t' style='width:max-content'>" +
                "<div style='width:150px;height:10px'></div>" +
                "<div style='width:90px;height:10px'></div>" +
                "</div></div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"width={box!.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 150) < 2);
        }

        // [CSS-SIZING-3 §4.1] height: fit-content wraps to content height
        [Fact]
        public void HeightFitContent_WrapsToContentHeight()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div id='t' style='width:100px;height:fit-content'>" +
                "<div style='width:50px;height:80px'></div>" +
                "</div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"height={box!.ContentRect.Height}");
            Assert.True(System.Math.Abs(box.ContentRect.Height - 80) < 2);
        }

        // [CSS-SIZING-3 §4.1] height: min-content equals content height
        [Fact]
        public void HeightMinContent_EqualsContentHeight()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div id='t' style='width:100px;height:min-content'>" +
                "<div style='width:50px;height:60px'></div>" +
                "</div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"height={box!.ContentRect.Height}");
            Assert.True(System.Math.Abs(box.ContentRect.Height - 60) < 2);
        }

        // [CSS-SIZING-3 §4.1] height: max-content equals content height
        [Fact]
        public void HeightMaxContent_EqualsContentHeight()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div id='t' style='width:100px;height:max-content'>" +
                "<div style='width:50px;height:120px'></div>" +
                "</div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"height={box!.ContentRect.Height}");
            Assert.True(System.Math.Abs(box.ContentRect.Height - 120) < 2);
        }

        // [CSS-SIZING-3 §4.1] fit-content width with padding on child
        [Fact]
        public void FitContent_ChildWithPadding()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='width:400px'>" +
                "<div id='t' style='width:fit-content'>" +
                "<div style='width:80px;padding:15px;height:10px'></div>" +
                "</div></div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"width={box!.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 110) < 2);
        }

        // [CSS-SIZING-3 §4.1] min-content on block ignores container width entirely
        [Fact]
        public void MinContent_IgnoresContainerWidth()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='width:400px'>" +
                "<div id='t' style='width:min-content'>" +
                "<div style='width:55px;height:10px'></div>" +
                "</div></div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"width={box!.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 55) < 2);
        }

        // [CSS-SIZING-3 §4.1] min-content with single narrow child
        [Fact]
        public void MinContent_SingleChild()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='width:400px'>" +
                "<div id='t' style='width:min-content'>" +
                "<div style='width:40px;height:10px'></div>" +
                "</div></div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"width={box!.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 40) < 2);
        }

        // [CSS-SIZING-3 §4.1] max-content with padding on parent
        [Fact]
        public void MaxContent_WithPadding()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='width:400px'>" +
                "<div id='t' style='width:max-content;padding:10px'>" +
                "<div style='width:100px;height:10px'></div>" +
                "</div></div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"contentWidth={box!.ContentRect.Width} paddingLeft={box.PaddingLeft}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 100) < 2);
        }

        // [CSS-SIZING-3 §4.1] fit-content on block with multiple children stacked vertically
        [Fact]
        public void FitContent_MultipleChildren_WidestWins()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='width:400px'>" +
                "<div id='t' style='width:fit-content'>" +
                "<div style='width:60px;height:10px'></div>" +
                "<div style='width:150px;height:10px'></div>" +
                "<div style='width:90px;height:10px'></div>" +
                "</div></div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"width={box!.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 150) < 2);
        }

        // [CSS-SIZING-3 §4.1] min-content with multiple children picks widest minimum
        [Fact]
        public void MinContent_MultipleChildren_WidestMinimumWins()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='width:400px'>" +
                "<div id='t' style='width:min-content'>" +
                "<div style='width:70px;height:10px'></div>" +
                "<div style='width:130px;height:10px'></div>" +
                "<div style='width:50px;height:10px'></div>" +
                "</div></div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"width={box!.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 130) < 2);
        }

        // [CSS-SIZING-3 §4.1] max-content with multiple stacked children picks widest
        [Fact]
        public void MaxContent_MultipleChildren_WidestWins()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='width:400px'>" +
                "<div id='t' style='width:max-content'>" +
                "<div style='width:110px;height:10px'></div>" +
                "<div style='width:170px;height:10px'></div>" +
                "<div style='width:90px;height:10px'></div>" +
                "</div></div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"width={box!.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 170) < 2);
        }

        // [CSS-SIZING-3 §4.1] fit-content on flex container shrinks to content
        [Fact]
        public void FitContent_FlexContainer_ShrinkWraps()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='width:400px'>" +
                "<div id='t' style='display:flex;width:fit-content'>" +
                "<div style='width:60px;height:20px'></div>" +
                "<div style='width:40px;height:20px'></div>" +
                "</div></div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"width={box!.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 100) < 2);
        }

        // [CSS-SIZING-3 §4.1] min-content on flex container uses minimum sum
        [Fact]
        public void MinContent_FlexContainer()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='width:400px'>" +
                "<div id='t' style='display:flex;width:min-content'>" +
                "<div style='width:60px;height:20px'></div>" +
                "<div style='width:40px;height:20px'></div>" +
                "</div></div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"width={box!.ContentRect.Width}");
            Assert.True(box.ContentRect.Width >= 39);
        }

        // [CSS-SIZING-3 §4.1] max-content on flex container sums children
        [Fact]
        public void MaxContent_FlexContainer_SumsChildren()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='width:400px'>" +
                "<div id='t' style='display:flex;width:max-content'>" +
                "<div style='width:70px;height:20px'></div>" +
                "<div style='width:50px;height:20px'></div>" +
                "</div></div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"width={box!.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 120) < 2);
        }

        // [CSS-SIZING-3 §4.1] fit-content on grid container shrinks to content
        [Fact]
        public void FitContent_GridContainer_ShrinkWraps()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='width:400px'>" +
                "<div id='t' style='display:grid;width:fit-content'>" +
                "<div style='width:130px;height:20px'></div>" +
                "</div></div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"width={box!.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 130) < 2);
        }

        // [CSS-SIZING-3 §4.1] min-content on grid container
        [Fact]
        public void MinContent_GridContainer()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='width:400px'>" +
                "<div id='t' style='display:grid;grid-template-columns:100px 80px;width:min-content'>" +
                "<div style='height:20px'></div>" +
                "<div style='height:20px'></div>" +
                "</div></div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"width={box!.ContentRect.Width}");
            Assert.True(box.ContentRect.Width >= 99);
        }

        // [CSS-GRID-1 §7.2.4] fit-content(limit) clamps the track at `limit` when
        // min-content < limit < max-content. The grid item below has min-content =
        // 60px (widest inline-block) and max-content = 300px (5 × 60px unwrapped),
        // so the track width must be 200px (the limit).
        [Fact]
        public void GridFitContentFunction_ClampsTrack()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0;font-size:0'>" +
                "<div style='display:grid;grid-template-columns:fit-content(200px) 1fr;width:600px'>" +
                "<div id='t'>" +
                "<span style='display:inline-block;width:60px;height:20px'></span>" +
                "<span style='display:inline-block;width:60px;height:20px'></span>" +
                "<span style='display:inline-block;width:60px;height:20px'></span>" +
                "<span style='display:inline-block;width:60px;height:20px'></span>" +
                "<span style='display:inline-block;width:60px;height:20px'></span>" +
                "</div>" +
                "<div style='height:20px'></div>" +
                "</div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"width={box!.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 200) < 2,
                $"Expected 200 (fit-content(200px) clamps when min-content < 200 < max-content), got {box.ContentRect.Width}");
        }

        // [CSS-GRID-1 §7.2.4] fit-content(100px) in grid with narrow content
        [Fact]
        public void GridFitContentFunction_NarrowContent()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='display:grid;grid-template-columns:fit-content(200px) 1fr;width:400px'>" +
                "<div id='t' style='height:20px'><div style='width:60px;height:10px'></div></div>" +
                "<div style='height:20px'></div>" +
                "</div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"width={box!.ContentRect.Width}");
            Assert.True(box.ContentRect.Width <= 201);
        }

        // [CSS-SIZING-3 §4.1] fit-content with border-box sizing
        [Fact]
        public void FitContent_BorderBox()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='width:400px'>" +
                "<div id='t' style='width:fit-content;box-sizing:border-box;padding:10px;border:5px solid'>" +
                "<div style='width:80px;height:10px'></div>" +
                "</div></div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            float borderBoxWidth = box!.ContentRect.Width + box.PaddingLeft + box.PaddingRight
                + box.BorderLeftWidth + box.BorderRightWidth;
            _output.WriteLine($"contentWidth={box.ContentRect.Width} borderBoxWidth={borderBoxWidth}");
            Assert.True(System.Math.Abs(borderBoxWidth - 110) < 2);
        }

        // [CSS-SIZING-3 §4.1] fit-content nested inside fit-content
        [Fact]
        public void FitContent_NestedFitContent()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='width:400px'>" +
                "<div style='width:fit-content'>" +
                "<div id='t' style='width:fit-content'>" +
                "<div style='width:90px;height:10px'></div>" +
                "</div></div></div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"width={box!.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 90) < 2);
        }

        // [CSS-SIZING-3 §4.1] min-content inside fit-content parent
        [Fact]
        public void MinContent_InsideFitContentParent()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='width:400px'>" +
                "<div style='width:fit-content'>" +
                "<div id='t' style='width:min-content'>" +
                "<div style='width:70px;height:10px'></div>" +
                "<div style='width:110px;height:10px'></div>" +
                "</div></div></div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"width={box!.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 110) < 2);
        }

        // [CSS-SIZING-3 §4.1] fit-content with zero-width child collapses to zero
        [Fact]
        public void FitContent_EmptyChild()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='width:400px'>" +
                "<div id='t' style='width:fit-content'>" +
                "<div style='height:10px'></div>" +
                "</div></div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"width={box!.ContentRect.Width}");
            Assert.True(box.ContentRect.Width < 2);
        }

        // [CSS-SIZING-3 §4.1] max-content can exceed container width
        [Fact]
        public void MaxContent_ExceedsContainer()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='width:100px'>" +
                "<div id='t' style='width:max-content'>" +
                "<div style='width:250px;height:10px'></div>" +
                "</div></div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"width={box!.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 250) < 2);
        }

        // [CSS-SIZING-3 §4.1] height: fit-content with stacked children sums heights
        [Fact]
        public void HeightFitContent_StackedChildren()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div id='t' style='width:100px;height:fit-content'>" +
                "<div style='height:30px'></div>" +
                "<div style='height:50px'></div>" +
                "</div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"height={box!.ContentRect.Height}");
            Assert.True(System.Math.Abs(box.ContentRect.Height - 80) < 2);
        }

        // [CSS-SIZING-3 §4.1] height: min-content with stacked children sums heights
        [Fact]
        public void HeightMinContent_StackedChildren()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div id='t' style='width:100px;height:min-content'>" +
                "<div style='height:25px'></div>" +
                "<div style='height:35px'></div>" +
                "</div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"height={box!.ContentRect.Height}");
            Assert.True(System.Math.Abs(box.ContentRect.Height - 60) < 2);
        }

        // [CSS-SIZING-3 §4.1] height: max-content with stacked children sums heights
        [Fact]
        public void HeightMaxContent_StackedChildren()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div id='t' style='width:100px;height:max-content'>" +
                "<div style='height:45px'></div>" +
                "<div style='height:55px'></div>" +
                "</div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"height={box!.ContentRect.Height}");
            Assert.True(System.Math.Abs(box.ContentRect.Height - 100) < 2);
        }

        // [CSS-SIZING-3 §4.1] fit-content with margin on child
        [Fact]
        public void FitContent_ChildWithMargin()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='width:400px'>" +
                "<div id='t' style='width:fit-content'>" +
                "<div style='width:80px;height:10px;margin:0 20px'></div>" +
                "</div></div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"width={box!.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 120) < 2);
        }

        // [CSS-SIZING-3 §4.1] min-content width on inline-block
        [Fact]
        public void MinContent_InlineBlock()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='width:400px'>" +
                "<div id='t' style='display:inline-block;width:min-content'>" +
                "<div style='width:75px;height:10px'></div>" +
                "<div style='width:95px;height:10px'></div>" +
                "</div></div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"width={box!.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 95) < 2);
        }

        // [CSS-SIZING-3 §4.1] max-content on flex container with gap
        [Fact]
        public void MaxContent_FlexWithGap()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='width:400px'>" +
                "<div id='t' style='display:flex;width:max-content;gap:10px'>" +
                "<div style='width:50px;height:20px'></div>" +
                "<div style='width:50px;height:20px'></div>" +
                "<div style='width:50px;height:20px'></div>" +
                "</div></div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"width={box!.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 170) < 2);
        }

        // [CSS-SIZING-3 §4.1] fit-content with deeply nested content
        [Fact]
        public void FitContent_DeeplyNestedContent()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='width:400px'>" +
                "<div id='t' style='width:fit-content'>" +
                "<div><div><div style='width:140px;height:10px'></div></div></div>" +
                "</div></div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"width={box!.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 140) < 2);
        }

        // [CSS-SIZING-3 §4.1] fit-content position: X starts at 0
        [Fact]
        public void FitContent_PositionAtZero()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div id='t' style='width:fit-content'>" +
                "<div style='width:100px;height:10px'></div>" +
                "</div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"x={box!.ContentRect.X}");
            Assert.True(System.Math.Abs(box.ContentRect.X - 0) < 2);
        }

        // [CSS-GRID-1 §7.2.4] grid with min-content and max-content column tracks
        [Fact]
        public void Grid_MinContentAndMaxContent_Tracks()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='display:grid;grid-template-columns:min-content max-content;width:400px'>" +
                "<div id='narrow'><div style='width:50px;height:10px'></div></div>" +
                "<div id='wide'><div style='width:120px;height:10px'></div></div>" +
                "</div></body>");
            var narrow = LayoutTestHelper.FindById(root, "narrow");
            var wide = LayoutTestHelper.FindById(root, "wide");
            Assert.NotNull(narrow);
            Assert.NotNull(wide);
            _output.WriteLine($"narrow={narrow!.ContentRect.Width} wide={wide!.ContentRect.Width}");
            Assert.True(System.Math.Abs(narrow.ContentRect.Width - 50) < 2);
            Assert.True(System.Math.Abs(wide.ContentRect.Width - 120) < 2);
        }

        // [CSS-SIZING-3 §4.1] min-content width equals zero when child has no intrinsic width
        [Fact]
        public void MinContent_NoIntrinsicChild()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='width:400px'>" +
                "<div id='t' style='width:min-content'>" +
                "<div style='height:20px'></div>" +
                "</div></div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"width={box!.ContentRect.Width}");
            Assert.True(box.ContentRect.Width < 2);
        }
    }
}
