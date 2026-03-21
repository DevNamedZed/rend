using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    /// <summary>
    /// Tests covering CSS2 float wrapping and stacking behavior:
    /// placement, line fitting, clear interaction, margins, padding, borders,
    /// and percentage widths in various container configurations.
    /// </summary>
    public class WptFloatWrappingTests
    {
        private readonly ITestOutputHelper _output;

        public WptFloatWrappingTests(ITestOutputHelper output)
        {
            _output = output;
        }

        // [CSS2 §9.5.1] Two left floats that fit side-by-side on one line
        [Fact]
        public void TwoLeftFloatsFitOnOneLine()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='width:200px'>" +
                "<div id='a' style='float:left;width:80px;height:40px'></div>" +
                "<div id='b' style='float:left;width:80px;height:40px'></div>" +
                "</div></body>");
            var floatA = LayoutTestHelper.FindById(root, "a")!;
            var floatB = LayoutTestHelper.FindById(root, "b")!;
            Assert.True(System.Math.Abs(floatA.ContentRect.X - 0) < 2, $"First float X={floatA.ContentRect.X}");
            Assert.True(System.Math.Abs(floatB.ContentRect.X - 80) < 2, $"Second float X={floatB.ContentRect.X}");
            Assert.True(System.Math.Abs(floatA.ContentRect.Y - floatB.ContentRect.Y) < 2, "Both on same line");
        }

        // [CSS2 §9.5.1] Three left floats all fit in a wide container
        [Fact]
        public void ThreeLeftFloatsFitInWideContainer()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='width:300px'>" +
                "<div id='a' style='float:left;width:80px;height:40px'></div>" +
                "<div id='b' style='float:left;width:80px;height:40px'></div>" +
                "<div id='c' style='float:left;width:80px;height:40px'></div>" +
                "</div></body>");
            var floatA = LayoutTestHelper.FindById(root, "a")!;
            var floatB = LayoutTestHelper.FindById(root, "b")!;
            var floatC = LayoutTestHelper.FindById(root, "c")!;
            Assert.True(System.Math.Abs(floatA.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(floatB.ContentRect.X - 80) < 2);
            Assert.True(System.Math.Abs(floatC.ContentRect.X - 160) < 2);
            Assert.True(System.Math.Abs(floatA.ContentRect.Y - floatC.ContentRect.Y) < 2,
                "All three on same line");
        }

        // [CSS2 §9.5.1] Non-floated block with clear:left clears a left float
        [Fact]
        public void ClearLeftBlockBelowLeftFloat()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='width:200px'>" +
                "<div style='float:left;width:80px;height:50px'></div>" +
                "<div id='t' style='clear:left;height:20px'></div>" +
                "</div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(target.ContentRect.Y >= 49,
                $"Cleared block Y={target.ContentRect.Y} should be below float (50px)");
        }

        // [CSS2 §9.5.1] Left float and right float that fit share the same Y
        [Fact]
        public void LeftAndRightFloatsSameY()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='width:300px'>" +
                "<div id='a' style='float:left;width:100px;height:40px'></div>" +
                "<div id='b' style='float:right;width:100px;height:40px'></div>" +
                "</div></body>");
            var floatA = LayoutTestHelper.FindById(root, "a")!;
            var floatB = LayoutTestHelper.FindById(root, "b")!;
            Assert.True(System.Math.Abs(floatA.ContentRect.Y - floatB.ContentRect.Y) < 2,
                "Left and right float on same line");
        }

        // [CSS2 §9.5.1] Float:left position X is at left content edge
        [Fact]
        public void FloatLeftPositionXAtLeftEdge()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='width:300px'>" +
                "<div id='t' style='float:left;width:50px;height:50px'></div>" +
                "</div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.X - 0) < 2,
                $"Float:left X={target.ContentRect.X} should be at 0");
        }

        // [CSS2 §9.5.1] Float:right position X is container-width minus float-width
        [Fact]
        public void FloatRightPositionXAtRightEdge()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='width:300px'>" +
                "<div id='t' style='float:right;width:50px;height:50px'></div>" +
                "</div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.X - 250) < 2,
                $"Float:right X={target.ContentRect.X} should be at 250");
        }

        // [CSS2 §9.5.1] Two floats that fill container width exactly
        [Fact]
        public void FloatsFillContainerWidthExactly()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='width:200px'>" +
                "<div id='a' style='float:left;width:100px;height:40px'></div>" +
                "<div id='b' style='float:left;width:100px;height:40px'></div>" +
                "</div></body>");
            var floatA = LayoutTestHelper.FindById(root, "a")!;
            var floatB = LayoutTestHelper.FindById(root, "b")!;
            Assert.True(System.Math.Abs(floatA.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(floatB.ContentRect.X - 100) < 2);
            Assert.True(System.Math.Abs(floatA.ContentRect.Y - floatB.ContentRect.Y) < 2,
                "Both floats on same line when they exactly fill width");
        }

        // [CSS2 §9.5.1] Floats with different heights placed side-by-side
        [Fact]
        public void FloatsWithDifferentHeightsSideBySide()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='width:200px'>" +
                "<div id='a' style='float:left;width:80px;height:60px'></div>" +
                "<div id='b' style='float:left;width:80px;height:30px'></div>" +
                "</div></body>");
            var floatA = LayoutTestHelper.FindById(root, "a")!;
            var floatB = LayoutTestHelper.FindById(root, "b")!;
            Assert.True(System.Math.Abs(floatA.ContentRect.Y - floatB.ContentRect.Y) < 2,
                "Both floats start at same Y regardless of height difference");
            Assert.True(System.Math.Abs(floatA.ContentRect.Height - 60) < 2);
            Assert.True(System.Math.Abs(floatB.ContentRect.Height - 30) < 2);
        }

        // [CSS2 §9.5.2] Non-floated block with clear:both below left and right floats
        [Fact]
        public void ClearBothBlockBelowBothFloats()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='width:300px'>" +
                "<div style='float:left;width:100px;height:40px'></div>" +
                "<div style='float:right;width:100px;height:80px'></div>" +
                "<div id='t' style='clear:both;height:20px'></div>" +
                "</div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(target.ContentRect.Y >= 79,
                $"Cleared element Y={target.ContentRect.Y} should be below tallest float (80px)");
        }

        // [CSS2 §9.5.1] Left float and right float X positions
        [Fact]
        public void LeftAndRightFloatXPositions()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='width:300px'>" +
                "<div id='a' style='float:left;width:100px;height:40px'></div>" +
                "<div id='b' style='float:right;width:100px;height:40px'></div>" +
                "</div></body>");
            var floatA = LayoutTestHelper.FindById(root, "a")!;
            var floatB = LayoutTestHelper.FindById(root, "b")!;
            Assert.True(System.Math.Abs(floatA.ContentRect.X - 0) < 2,
                $"Left float X={floatA.ContentRect.X}");
            Assert.True(System.Math.Abs(floatB.ContentRect.X - 200) < 2,
                $"Right float X={floatB.ContentRect.X} should be at 200");
        }

        // [CSS2 §9.5.1] Floats with horizontal margins affect placement
        [Fact]
        public void FloatsWithMargins()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='width:200px'>" +
                "<div id='a' style='float:left;width:60px;height:40px;margin-left:10px;margin-right:10px'></div>" +
                "<div id='b' style='float:left;width:60px;height:40px;margin-left:10px;margin-right:10px'></div>" +
                "</div></body>");
            var floatA = LayoutTestHelper.FindById(root, "a")!;
            var floatB = LayoutTestHelper.FindById(root, "b")!;
            Assert.True(System.Math.Abs(floatA.ContentRect.X - 10) < 2,
                $"First float X={floatA.ContentRect.X}");
            Assert.True(System.Math.Abs(floatB.ContentRect.X - 90) < 2,
                $"Second float X={floatB.ContentRect.X}");
        }

        // [CSS2 §9.5.1] Floats with padding increase total size
        [Fact]
        public void FloatsWithPadding()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='width:200px'>" +
                "<div id='a' style='float:left;width:60px;height:40px;padding:10px'></div>" +
                "<div id='b' style='float:left;width:60px;height:40px;padding:10px'></div>" +
                "</div></body>");
            var floatA = LayoutTestHelper.FindById(root, "a")!;
            var floatB = LayoutTestHelper.FindById(root, "b")!;
            Assert.True(System.Math.Abs(floatA.ContentRect.X - 10) < 2);
            Assert.True(System.Math.Abs(floatB.ContentRect.X - 90) < 2,
                $"Second float X={floatB.ContentRect.X}");
        }

        // [CSS2 §9.5.1] Float inside container with padding
        [Fact]
        public void FloatInsidePaddedContainer()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='width:200px;padding:20px'>" +
                "<div id='t' style='float:left;width:80px;height:40px'></div>" +
                "</div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.X - 20) < 2,
                $"Float X={target.ContentRect.X} should be offset by container padding");
        }

        // [CSS2 §9.5.1] Float inside container with border
        [Fact]
        public void FloatInsideBorderedContainer()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='width:200px;border:5px solid black'>" +
                "<div id='t' style='float:left;width:80px;height:40px'></div>" +
                "</div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.X - 5) < 2,
                $"Float X={target.ContentRect.X} should be offset by container border");
        }

        // [CSS2 §10.3.5] Float percentage width resolves against container
        [Fact]
        public void FloatPercentageWidthNarrowContainer()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='width:200px'>" +
                "<div id='t' style='float:left;width:50%;height:40px'></div>" +
                "</div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 100) < 2,
                $"50% of 200px = 100px, got {target.ContentRect.Width}");
        }

        // [CSS2 §10.3.5] Float percentage width in wider container
        [Fact]
        public void FloatPercentageWidthWideContainer()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='width:400px'>" +
                "<div id='t' style='float:left;width:25%;height:40px'></div>" +
                "</div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 100) < 2,
                $"25% of 400px = 100px, got {target.ContentRect.Width}");
        }

        // [CSS2 §9.5.1] Three left floats fitting side-by-side exactly
        [Fact]
        public void ThreeLeftFloatsSideBySide()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='width:300px'>" +
                "<div id='a' style='float:left;width:100px;height:40px'></div>" +
                "<div id='b' style='float:left;width:100px;height:40px'></div>" +
                "<div id='c' style='float:left;width:100px;height:40px'></div>" +
                "</div></body>");
            var floatA = LayoutTestHelper.FindById(root, "a")!;
            var floatB = LayoutTestHelper.FindById(root, "b")!;
            var floatC = LayoutTestHelper.FindById(root, "c")!;
            Assert.True(System.Math.Abs(floatA.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(floatB.ContentRect.X - 100) < 2);
            Assert.True(System.Math.Abs(floatC.ContentRect.X - 200) < 2);
            Assert.True(System.Math.Abs(floatA.ContentRect.Y - floatC.ContentRect.Y) < 2,
                "All three on same line");
        }

        // [CSS2 §9.5.1] Four left floats: all four fit in container
        [Fact]
        public void FourLeftFloatsFitSideBySide()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='width:400px'>" +
                "<div id='a' style='float:left;width:90px;height:40px'></div>" +
                "<div id='b' style='float:left;width:90px;height:40px'></div>" +
                "<div id='c' style='float:left;width:90px;height:40px'></div>" +
                "<div id='d' style='float:left;width:90px;height:40px'></div>" +
                "</div></body>");
            var floatA = LayoutTestHelper.FindById(root, "a")!;
            var floatB = LayoutTestHelper.FindById(root, "b")!;
            var floatC = LayoutTestHelper.FindById(root, "c")!;
            var floatD = LayoutTestHelper.FindById(root, "d")!;
            Assert.True(System.Math.Abs(floatA.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(floatB.ContentRect.X - 90) < 2);
            Assert.True(System.Math.Abs(floatC.ContentRect.X - 180) < 2);
            Assert.True(System.Math.Abs(floatD.ContentRect.X - 270) < 2);
            Assert.True(System.Math.Abs(floatA.ContentRect.Y - floatD.ContentRect.Y) < 2,
                "All four on same line");
        }

        // [CSS2 §9.5.2] Non-floated block clear:left below a left float
        [Fact]
        public void ClearLeftBelowLeftFloat()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='width:300px'>" +
                "<div style='float:left;width:100px;height:60px'></div>" +
                "<div id='t' style='clear:left;height:20px'></div>" +
                "</div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(target.ContentRect.Y >= 59,
                $"Cleared block Y={target.ContentRect.Y} should be at or below float bottom (60)");
        }

        // [CSS2 §9.5.1] Adjacent floats with margin-right gap between them
        [Fact]
        public void AdjacentFloatsWithGap()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='width:300px'>" +
                "<div id='a' style='float:left;width:80px;height:40px;margin-right:20px'></div>" +
                "<div id='b' style='float:left;width:80px;height:40px'></div>" +
                "</div></body>");
            var floatA = LayoutTestHelper.FindById(root, "a")!;
            var floatB = LayoutTestHelper.FindById(root, "b")!;
            Assert.True(System.Math.Abs(floatB.ContentRect.X - 100) < 2,
                $"Second float X={floatB.ContentRect.X} should be at 100 (80+20 margin-right)");
            Assert.True(System.Math.Abs(floatA.ContentRect.Y - floatB.ContentRect.Y) < 2,
                "Both on same line");
        }

        // [CSS2 §9.5.1] Two right floats stack from right edge inward
        [Fact]
        public void TwoRightFloatsStackFromRight()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='width:300px'>" +
                "<div id='a' style='float:right;width:80px;height:40px'></div>" +
                "<div id='b' style='float:right;width:80px;height:40px'></div>" +
                "</div></body>");
            var floatA = LayoutTestHelper.FindById(root, "a")!;
            var floatB = LayoutTestHelper.FindById(root, "b")!;
            Assert.True(System.Math.Abs(floatA.ContentRect.X - 220) < 2,
                $"First right float X={floatA.ContentRect.X}");
            Assert.True(System.Math.Abs(floatB.ContentRect.X - 140) < 2,
                $"Second right float X={floatB.ContentRect.X}");
        }

        // [CSS2 §9.5.1] Left float followed by right float on same line
        [Fact]
        public void LeftThenRightFloatSameLine()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='width:300px'>" +
                "<div id='left' style='float:left;width:100px;height:40px'></div>" +
                "<div id='right' style='float:right;width:100px;height:40px'></div>" +
                "</div></body>");
            var leftFloat = LayoutTestHelper.FindById(root, "left")!;
            var rightFloat = LayoutTestHelper.FindById(root, "right")!;
            Assert.True(System.Math.Abs(leftFloat.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(rightFloat.ContentRect.X - 200) < 2,
                $"Right float X={rightFloat.ContentRect.X}");
            Assert.True(System.Math.Abs(leftFloat.ContentRect.Y - rightFloat.ContentRect.Y) < 2);
        }

        // [CSS2 §9.5.1] Margin-top on float is honored (no collapse)
        [Fact]
        public void FloatMarginTopHonored()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='width:200px'>" +
                "<div id='t' style='float:left;width:80px;height:40px;margin-top:15px'></div>" +
                "</div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Y - 15) < 2,
                $"Float margin-top Y={target.ContentRect.Y} should be 15");
        }

        // [CSS2 §9.5.1] Float with both padding and border offsets next float
        [Fact]
        public void FloatWithPaddingAndBorder()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='width:300px'>" +
                "<div id='a' style='float:left;width:60px;height:40px;padding:5px;border:2px solid black'></div>" +
                "<div id='b' style='float:left;width:60px;height:40px'></div>" +
                "</div></body>");
            var floatA = LayoutTestHelper.FindById(root, "a")!;
            var floatB = LayoutTestHelper.FindById(root, "b")!;
            Assert.True(System.Math.Abs(floatA.ContentRect.X - 7) < 2,
                $"First float content X={floatA.ContentRect.X} should be at border+padding=7");
            Assert.True(System.Math.Abs(floatB.ContentRect.X - 74) < 2,
                $"Second float X={floatB.ContentRect.X} should be after first float total=74");
        }

        // [CSS2 §9.5.2] Clear:both after left and right floats of different heights
        [Fact]
        public void ClearBothBelowTallestFloat()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='width:300px'>" +
                "<div style='float:left;width:100px;height:40px'></div>" +
                "<div style='float:right;width:100px;height:80px'></div>" +
                "<div id='t' style='clear:both;height:20px'></div>" +
                "</div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(target.ContentRect.Y >= 79,
                $"Cleared element Y={target.ContentRect.Y} should be below tallest float (80px)");
        }

        // [CSS2 §9.5.1] Float stacking with mixed heights all fitting
        [Fact]
        public void FloatStackingMixedHeights()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='width:300px'>" +
                "<div id='a' style='float:left;width:90px;height:80px'></div>" +
                "<div id='b' style='float:left;width:90px;height:30px'></div>" +
                "<div id='c' style='float:left;width:90px;height:60px'></div>" +
                "</div></body>");
            var floatA = LayoutTestHelper.FindById(root, "a")!;
            var floatB = LayoutTestHelper.FindById(root, "b")!;
            var floatC = LayoutTestHelper.FindById(root, "c")!;
            Assert.True(System.Math.Abs(floatA.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(floatB.ContentRect.X - 90) < 2);
            Assert.True(System.Math.Abs(floatC.ContentRect.X - 180) < 2);
        }

        // [CSS2 §9.5.1] Container padding reduces available width but floats still fit
        [Fact]
        public void ContainerPaddingReducesFloatSpace()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='width:200px;padding:0 30px'>" +
                "<div id='a' style='float:left;width:80px;height:40px'></div>" +
                "<div id='b' style='float:left;width:80px;height:40px'></div>" +
                "</div></body>");
            var floatA = LayoutTestHelper.FindById(root, "a")!;
            var floatB = LayoutTestHelper.FindById(root, "b")!;
            Assert.True(System.Math.Abs(floatA.ContentRect.Y - floatB.ContentRect.Y) < 2,
                "Both fit in content area of padded container");
            Assert.True(System.Math.Abs(floatA.ContentRect.X - 30) < 2,
                $"Float A X={floatA.ContentRect.X} offset by container padding");
        }

        // [CSS2 §9.5.1] Clear:right on left float does not affect placement
        [Fact]
        public void ClearRightDoesNotAffectLeftFloats()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='width:300px'>" +
                "<div id='a' style='float:left;width:100px;height:50px'></div>" +
                "<div id='b' style='float:left;width:100px;height:40px;clear:right'></div>" +
                "</div></body>");
            var floatA = LayoutTestHelper.FindById(root, "a")!;
            var floatB = LayoutTestHelper.FindById(root, "b")!;
            Assert.True(System.Math.Abs(floatA.ContentRect.Y - floatB.ContentRect.Y) < 2,
                $"clear:right should not push second left float below. A.Y={floatA.ContentRect.Y}, B.Y={floatB.ContentRect.Y}");
        }

        // [CSS2 §9.5.1] Float right with margin-right offsets from container edge
        [Fact]
        public void FloatRightWithMarginRight()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='width:300px'>" +
                "<div id='t' style='float:right;width:80px;height:40px;margin-right:20px'></div>" +
                "</div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            // Right float: content at 300-80-20=200
            Assert.True(System.Math.Abs(target.ContentRect.X - 200) < 2,
                $"Float:right with margin-right X={target.ContentRect.X} should be at 200");
        }

        // [CSS2 §9.5.1] Float left with margin-bottom does not collapse with next float
        [Fact]
        public void FloatMarginBottomDoesNotCollapse()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='width:200px'>" +
                "<div id='a' style='float:left;width:80px;height:40px;margin-bottom:20px'></div>" +
                "<div id='b' style='float:left;width:80px;height:40px'></div>" +
                "</div></body>");
            var floatA = LayoutTestHelper.FindById(root, "a")!;
            var floatB = LayoutTestHelper.FindById(root, "b")!;
            // Both fit side by side, margin-bottom on A doesn't affect B's Y
            Assert.True(System.Math.Abs(floatA.ContentRect.Y - floatB.ContentRect.Y) < 2,
                "Margin-bottom on float does not push adjacent float down");
        }

        // [CSS2 §9.5.2] Clear:right clears right float
        [Fact]
        public void ClearRightBelowRightFloat()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='width:200px'>" +
                "<div style='float:right;width:80px;height:50px'></div>" +
                "<div id='t' style='clear:right;height:20px'></div>" +
                "</div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(target.ContentRect.Y >= 49,
                $"Cleared block Y={target.ContentRect.Y} should be below right float (50px)");
        }

        // [CSS2 §10.3.5] Two 50% floats fill container exactly
        [Fact]
        public void TwoPercentageFloatsFillContainer()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='width:200px'>" +
                "<div id='a' style='float:left;width:50%;height:40px'></div>" +
                "<div id='b' style='float:left;width:50%;height:40px'></div>" +
                "</div></body>");
            var floatA = LayoutTestHelper.FindById(root, "a")!;
            var floatB = LayoutTestHelper.FindById(root, "b")!;
            Assert.True(System.Math.Abs(floatA.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(floatB.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(floatB.ContentRect.X - 100) < 2,
                $"Second 50% float X={floatB.ContentRect.X}");
        }

        // [CSS2 §9.5.1] Float with border-box width includes border in total
        [Fact]
        public void FloatBorderBoxWidth()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='width:300px'>" +
                "<div id='a' style='float:left;width:100px;height:40px;box-sizing:border-box;border:10px solid black'></div>" +
                "<div id='b' style='float:left;width:50px;height:40px'></div>" +
                "</div></body>");
            var floatA = LayoutTestHelper.FindById(root, "a")!;
            var floatB = LayoutTestHelper.FindById(root, "b")!;
            // border-box: total=100px, content=80px, border=10px each side
            Assert.True(System.Math.Abs(floatA.ContentRect.Width - 80) < 2,
                $"Border-box content width={floatA.ContentRect.Width} should be 80");
            Assert.True(System.Math.Abs(floatB.ContentRect.X - 100) < 2,
                $"Second float X={floatB.ContentRect.X} should start after 100px total");
        }

        // [CSS2 §9.5.1] Three right floats stack from right inward
        [Fact]
        public void ThreeRightFloatsStackInward()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='width:300px'>" +
                "<div id='a' style='float:right;width:60px;height:40px'></div>" +
                "<div id='b' style='float:right;width:60px;height:40px'></div>" +
                "<div id='c' style='float:right;width:60px;height:40px'></div>" +
                "</div></body>");
            var floatA = LayoutTestHelper.FindById(root, "a")!;
            var floatB = LayoutTestHelper.FindById(root, "b")!;
            var floatC = LayoutTestHelper.FindById(root, "c")!;
            Assert.True(System.Math.Abs(floatA.ContentRect.X - 240) < 2,
                $"First right float X={floatA.ContentRect.X}");
            Assert.True(System.Math.Abs(floatB.ContentRect.X - 180) < 2,
                $"Second right float X={floatB.ContentRect.X}");
            Assert.True(System.Math.Abs(floatC.ContentRect.X - 120) < 2,
                $"Third right float X={floatC.ContentRect.X}");
        }

        // [CSS2 §9.5.1] Float inside container with both padding and border
        [Fact]
        public void FloatInsideContainerWithPaddingAndBorder()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='width:200px;padding:10px;border:5px solid black'>" +
                "<div id='t' style='float:left;width:80px;height:40px'></div>" +
                "</div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            // Content area starts at border(5) + padding(10) = 15
            Assert.True(System.Math.Abs(target.ContentRect.X - 15) < 2,
                $"Float X={target.ContentRect.X} should be at 15 (5px border + 10px padding)");
        }
    }
}
