using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    /// <summary>
    /// Tests covering CSS2 float placement, clearing, containment, and interaction
    /// with BFC, flex, and positioned containers.
    /// </summary>
    public class WptFloatClearanceTests
    {
        private readonly ITestOutputHelper _output;
        public WptFloatClearanceTests(ITestOutputHelper output) { _output = output; }

        // ──────────────────────────────────────────────
        // 1. float:left basic — element at left edge
        // ──────────────────────────────────────────────
        // [CSS2 §9.5.1] A left float is placed at the left content edge of its container.
        [Fact]
        public void FloatLeft_BasicPosition()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='width:400px'>" +
                "<div id='t' style='float:left;width:100px;height:50px;background:red'></div>" +
                "<div>Text flows around the float</div>" +
                "</div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(target.ContentRect.Y - 0) < 2);
        }

        // ──────────────────────────────────────────────
        // 2. float:right basic — element at right edge
        // ──────────────────────────────────────────────
        // [CSS2 §9.5.1] A right float's right outer edge touches the right content edge.
        // In a 400px container, a 100px-wide float:right sits at X=300.
        [Fact]
        public void FloatRight_BasicPosition()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='width:400px'>" +
                "<div id='t' style='float:right;width:100px;height:50px'></div>" +
                "</div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.X - 300) < 2);
        }

        // ──────────────────────────────────────────────
        // 3. float:left preserves explicit width and height
        // ──────────────────────────────────────────────
        // [CSS2 §10.3.5] Explicit width on float is honored.
        [Fact]
        public void FloatLeft_WidthHeight()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='width:400px'>" +
                "<div id='t' style='float:left;width:120px;height:80px'></div>" +
                "</div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 120) < 2);
            Assert.True(System.Math.Abs(target.ContentRect.Height - 80) < 2);
        }

        // ──────────────────────────────────────────────
        // 4. Float at X=0 when first in container
        // ──────────────────────────────────────────────
        // [CSS2 §9.5.1] First float:left starts at container's left content edge.
        [Fact]
        public void FloatLeft_FirstInContainer_AtOrigin()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='width:300px;padding:0'>" +
                "<div id='t' style='float:left;width:50px;height:50px'></div>" +
                "</div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(target.ContentRect.Y - 0) < 2);
        }

        // ──────────────────────────────────────────────
        // 5. Two float:left items side by side
        // ──────────────────────────────────────────────
        // [CSS2 §9.5.1] Second left float's left outer edge touches first's right outer edge.
        [Fact]
        public void TwoFloatLeft_SideBySide()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='width:400px'>" +
                "<div id='a' style='float:left;width:100px;height:50px'></div>" +
                "<div id='b' style='float:left;width:100px;height:50px'></div>" +
                "</div></body>");
            var first = LayoutTestHelper.FindById(root, "a")!;
            var second = LayoutTestHelper.FindById(root, "b")!;
            Assert.True(System.Math.Abs(first.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(second.ContentRect.X - 100) < 2);
            Assert.True(System.Math.Abs(second.ContentRect.Y - 0) < 2);
        }

        // ──────────────────────────────────────────────
        // 6. Two float:right items — second is further left
        // ──────────────────────────────────────────────
        // [CSS2 §9.5.1] In 400px container, first float:right 100px at X=300, second at X=200.
        [Fact]
        public void TwoFloatRight_SecondFurtherLeft()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='width:400px'>" +
                "<div id='a' style='float:right;width:100px;height:50px'></div>" +
                "<div id='b' style='float:right;width:100px;height:50px'></div>" +
                "</div></body>");
            var first = LayoutTestHelper.FindById(root, "a")!;
            var second = LayoutTestHelper.FindById(root, "b")!;
            Assert.True(System.Math.Abs(first.ContentRect.X - 300) < 2);
            Assert.True(System.Math.Abs(second.ContentRect.X - 200) < 2);
        }

        // ──────────────────────────────────────────────
        // 7. BFC parent auto height includes floats
        // ──────────────────────────────────────────────
        // [CSS2 §10.6.7] In a BFC root, auto height accounts for floated children.
        [Fact]
        public void Float_BfcParent_AutoHeightIncludesFloat()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div id='parent' style='width:400px;overflow:hidden'>" +
                "<div style='float:left;width:100px;height:200px'></div>" +
                "</div></body>");
            var parent = LayoutTestHelper.FindById(root, "parent")!;
            Assert.True(System.Math.Abs(parent.ContentRect.Height - 200) < 2,
                $"BFC parent auto-height should include float, got {parent.ContentRect.Height}");
        }

        // ──────────────────────────────────────────────
        // 8. clear:left clears left floats
        // ──────────────────────────────────────────────
        // [CSS2 §9.5.2] clear:left forces top border edge below bottom of all preceding left floats.
        [Fact]
        public void ClearLeft_ClearsLeftFloat()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='width:400px'>" +
                "<div style='float:left;width:100px;height:60px'></div>" +
                "<div id='t' style='clear:left;width:100px;height:30px'></div>" +
                "</div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(target.ContentRect.Y >= 59,
                $"clear:left element should be at Y>=60, got {target.ContentRect.Y}");
        }

        // ──────────────────────────────────────────────
        // 9. clear:right clears right floats
        // ──────────────────────────────────────────────
        // [CSS2 §9.5.2] clear:right forces top border edge below bottom of all preceding right floats.
        [Fact]
        public void ClearRight_ClearsRightFloat()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='width:400px'>" +
                "<div style='float:right;width:100px;height:70px'></div>" +
                "<div id='t' style='clear:right;width:100px;height:30px'></div>" +
                "</div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(target.ContentRect.Y >= 69,
                $"clear:right element should be at Y>=70, got {target.ContentRect.Y}");
        }

        // ──────────────────────────────────────────────
        // 10. clear:both clears both left and right floats
        // ──────────────────────────────────────────────
        // [CSS2 §9.5.2] clear:both below the taller of left and right floats.
        [Fact]
        public void ClearBoth_ClearsBothFloats()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='width:400px'>" +
                "<div style='float:left;width:100px;height:40px'></div>" +
                "<div style='float:right;width:100px;height:90px'></div>" +
                "<div id='t' style='clear:both;width:100px;height:20px'></div>" +
                "</div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(target.ContentRect.Y >= 89,
                $"clear:both should be below taller float (90px), got Y={target.ContentRect.Y}");
        }

        // ──────────────────────────────────────────────
        // 11. Clear forces element below float
        // ──────────────────────────────────────────────
        // [CSS2 §9.5.2] clear:left on a non-float block moves it below the float.
        [Fact]
        public void Clear_ForcesElementBelowFloat()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='width:400px'>" +
                "<div style='float:left;width:50px;height:100px'></div>" +
                "<div style='width:200px;height:30px'></div>" +
                "<div id='t' style='clear:left;width:200px;height:30px'></div>" +
                "</div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(target.ContentRect.Y >= 99,
                $"Cleared element should be at Y>=100, got {target.ContentRect.Y}");
        }

        // ──────────────────────────────────────────────
        // 12. Float with margin offsets position
        // ──────────────────────────────────────────────
        // [CSS2 §9.5.1] Margins on floats offset their position from container edge.
        [Fact]
        public void FloatLeft_WithMargin()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='width:400px'>" +
                "<div id='t' style='float:left;width:100px;height:50px;margin-left:20px;margin-top:15px'></div>" +
                "</div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.X - 20) < 2);
            Assert.True(System.Math.Abs(target.ContentRect.Y - 15) < 2);
        }

        // ──────────────────────────────────────────────
        // 13. Float with percentage width
        // ──────────────────────────────────────────────
        // [CSS2 §10.3.5] Percentage width resolves against containing block.
        // 25% of 400px = 100px.
        [Fact]
        public void FloatLeft_PercentageWidth()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='width:400px'>" +
                "<div id='t' style='float:left;width:25%;height:50px'></div>" +
                "</div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 100) < 2);
        }

        // ──────────────────────────────────────────────
        // 14. Float fits beside non-float block content
        // ──────────────────────────────────────────────
        // [CSS2 §9.5.1] Block after float has full width but content avoids float.
        // The block box itself starts at X=0 (blocks take full width), but text flows around.
        [Fact]
        public void FloatLeft_ContentFlowsAround()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='width:400px'>" +
                "<div style='float:left;width:100px;height:50px'></div>" +
                "<div id='t' style='height:50px'>Some text here</div>" +
                "</div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            // Block-level box takes full container width; float displaces inline content, not the block.
            Assert.True(System.Math.Abs(target.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(target.ContentRect.Width - 400) < 2);
        }

        // ──────────────────────────────────────────────
        // 15. Four left floats that fit horizontally
        // ──────────────────────────────────────────────
        // [CSS2 §9.5.1] Four 100px floats in 400px container all share one line.
        [Fact]
        public void FourFloatLeft_AllFitOnSameLine()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='width:400px'>" +
                "<div id='a' style='float:left;width:100px;height:40px'></div>" +
                "<div id='b' style='float:left;width:100px;height:40px'></div>" +
                "<div id='c' style='float:left;width:100px;height:40px'></div>" +
                "<div id='d' style='float:left;width:100px;height:40px'></div>" +
                "</div></body>");
            var first = LayoutTestHelper.FindById(root, "a")!;
            var second = LayoutTestHelper.FindById(root, "b")!;
            var third = LayoutTestHelper.FindById(root, "c")!;
            var fourth = LayoutTestHelper.FindById(root, "d")!;
            Assert.True(System.Math.Abs(first.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(second.ContentRect.X - 100) < 2);
            Assert.True(System.Math.Abs(third.ContentRect.X - 200) < 2);
            Assert.True(System.Math.Abs(fourth.ContentRect.X - 300) < 2);
        }

        // ──────────────────────────────────────────────
        // 16. Nested floats
        // ──────────────────────────────────────────────
        // [CSS2 §9.5.1] A float inside another float positions relative to the outer float's content box.
        [Fact]
        public void NestedFloat_PositionedInsideParent()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='width:400px'>" +
                "<div id='outer' style='float:left;width:200px;height:100px'>" +
                "<div id='inner' style='float:left;width:80px;height:40px'></div>" +
                "</div></div></body>");
            var outer = LayoutTestHelper.FindById(root, "outer")!;
            var inner = LayoutTestHelper.FindById(root, "inner")!;
            Assert.True(System.Math.Abs(outer.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(inner.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(inner.ContentRect.Width - 80) < 2);
        }

        // ──────────────────────────────────────────────
        // 17. Float inside positioned container
        // ──────────────────────────────────────────────
        // [CSS2 §9.5.1] Float positions relative to its containing block (the positioned element).
        [Fact]
        public void Float_InsidePositionedContainer()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='position:relative;left:50px;top:30px;width:300px'>" +
                "<div id='t' style='float:left;width:100px;height:50px'></div>" +
                "</div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            // Float at X=0 within the positioned container (container itself is offset by 50px)
            Assert.True(System.Math.Abs(target.ContentRect.X - 50) < 2);
            Assert.True(System.Math.Abs(target.ContentRect.Y - 30) < 2);
        }

        // ──────────────────────────────────────────────
        // 18. float:left then float:right on same line
        // ──────────────────────────────────────────────
        // [CSS2 §9.5.1] Left and right floats share the same line band if they fit.
        [Fact]
        public void FloatLeft_ThenFloatRight_SameLine()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='width:400px'>" +
                "<div id='left' style='float:left;width:100px;height:50px'></div>" +
                "<div id='right' style='float:right;width:100px;height:50px'></div>" +
                "</div></body>");
            var leftFloat = LayoutTestHelper.FindById(root, "left")!;
            var rightFloat = LayoutTestHelper.FindById(root, "right")!;
            Assert.True(System.Math.Abs(leftFloat.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(rightFloat.ContentRect.X - 300) < 2);
            Assert.True(System.Math.Abs(leftFloat.ContentRect.Y - rightFloat.ContentRect.Y) < 2);
        }

        // ──────────────────────────────────────────────
        // 19. Float clearance with margins
        // ──────────────────────────────────────────────
        // [CSS2 §9.5.2] Clearance + margin-top: element clears past float, margin-top doesn't
        // collapse through clearance.
        [Fact]
        public void ClearLeft_WithMarginTop()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='width:400px'>" +
                "<div style='float:left;width:100px;height:60px'></div>" +
                "<div id='t' style='clear:left;margin-top:20px;width:100px;height:30px'></div>" +
                "</div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            // Clearance pushes below float (60px). Margin-top adds space but clearance
            // absorbs it if clearance > margin. Y should be at least 60.
            Assert.True(target.ContentRect.Y >= 59,
                $"Cleared element with margin-top should be at Y>=60, got {target.ContentRect.Y}");
        }

        // ──────────────────────────────────────────────
        // 20. Float left and right with different heights
        // ──────────────────────────────────────────────
        // [CSS2 §9.5.1] Left and right floats can have different heights, both start at Y=0.
        [Fact]
        public void FloatLeftAndRight_DifferentHeights()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='width:400px'>" +
                "<div id='a' style='float:left;width:100px;height:30px'></div>" +
                "<div id='b' style='float:right;width:100px;height:60px'></div>" +
                "</div></body>");
            var leftFloat = LayoutTestHelper.FindById(root, "a")!;
            var rightFloat = LayoutTestHelper.FindById(root, "b")!;
            Assert.True(System.Math.Abs(leftFloat.ContentRect.Y - 0) < 2);
            Assert.True(System.Math.Abs(rightFloat.ContentRect.Y - 0) < 2);
            Assert.True(System.Math.Abs(leftFloat.ContentRect.Height - 30) < 2);
            Assert.True(System.Math.Abs(rightFloat.ContentRect.Height - 60) < 2);
        }

        // ──────────────────────────────────────────────
        // 21. overflow:hidden on parent creates BFC that contains floats
        // ──────────────────────────────────────────────
        // [CSS2 §9.4.1] BFC root (overflow:hidden) expands to contain floated children.
        [Fact]
        public void OverflowHidden_ContainsFloats()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div id='parent' style='width:400px;overflow:hidden'>" +
                "<div style='float:left;width:100px;height:150px'></div>" +
                "</div></body>");
            var parent = LayoutTestHelper.FindById(root, "parent")!;
            Assert.True(parent.ContentRect.Height >= 149,
                $"overflow:hidden parent should contain float, got height={parent.ContentRect.Height}");
        }

        // ──────────────────────────────────────────────
        // 22. overflow:auto on parent creates BFC that contains floats
        // ──────────────────────────────────────────────
        // [CSS2 §9.4.1] overflow:auto also establishes a BFC.
        [Fact]
        public void OverflowAuto_ContainsFloats()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div id='parent' style='width:400px;overflow:auto'>" +
                "<div style='float:left;width:100px;height:120px'></div>" +
                "</div></body>");
            var parent = LayoutTestHelper.FindById(root, "parent")!;
            Assert.True(parent.ContentRect.Height >= 119,
                $"overflow:auto parent should contain float, got height={parent.ContentRect.Height}");
        }

        // ──────────────────────────────────────────────
        // 23. Float with negative margin
        // ──────────────────────────────────────────────
        // [CSS2 §9.5.1] Negative margins can pull a float outside container edges.
        [Fact]
        public void FloatLeft_NegativeMargin()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='width:400px'>" +
                "<div id='t' style='float:left;width:100px;height:50px;margin-left:-20px'></div>" +
                "</div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.X - (-20)) < 2);
        }

        // ──────────────────────────────────────────────
        // 24. Wide float takes full container width
        // ──────────────────────────────────────────────
        // [CSS2 §9.5.1] Float with 100% width fills the container.
        [Fact]
        public void FloatLeft_FullWidth()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='width:400px'>" +
                "<div id='t' style='float:left;width:100%;height:50px'></div>" +
                "</div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 400) < 2);
        }

        // ──────────────────────────────────────────────
        // 25. Float with block content after it
        // ──────────────────────────────────────────────
        // [CSS2 §9.5] Block after float still starts at container edge; float overlaps it.
        [Fact]
        public void FloatLeft_BlockContentAfter()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='width:400px'>" +
                "<div style='float:left;width:100px;height:80px'></div>" +
                "<div id='t' style='width:400px;height:120px'></div>" +
                "</div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(target.ContentRect.Y - 0) < 2);
        }

        // ──────────────────────────────────────────────
        // 26. Float shrink-to-fit width (auto width)
        // ──────────────────────────────────────────────
        // [CSS2 §10.3.5] Float with auto width shrinks to fit content.
        [Fact]
        public void FloatLeft_ShrinkToFitAutoWidth()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='width:400px'>" +
                "<div id='t' style='float:left'>" +
                "<div style='width:75px;height:30px'></div>" +
                "</div></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 75) < 2);
        }

        // ──────────────────────────────────────────────
        // 27. Float with explicit width in percentage container
        // ──────────────────────────────────────────────
        // [CSS2 §10.3.5] 50% of 400px = 200px float width.
        [Fact]
        public void FloatLeft_FiftyPercentWidth()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='width:400px'>" +
                "<div id='t' style='float:left;width:50%;height:50px'></div>" +
                "</div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 200) < 2);
        }

        // ──────────────────────────────────────────────
        // 28. Float with max-width
        // ──────────────────────────────────────────────
        // [CSS2 §10.4] max-width constrains float width.
        [Fact]
        public void FloatLeft_MaxWidth()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='width:400px'>" +
                "<div id='t' style='float:left;width:300px;max-width:150px;height:50px'></div>" +
                "</div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 150) < 2);
        }

        // ──────────────────────────────────────────────
        // 29. clear:both after multiple floats
        // ──────────────────────────────────────────────
        // [CSS2 §9.5.2] clear:both below all preceding floats regardless of side.
        [Fact]
        public void ClearBoth_AfterMultipleFloats()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='width:400px'>" +
                "<div style='float:left;width:100px;height:30px'></div>" +
                "<div style='float:right;width:100px;height:50px'></div>" +
                "<div style='float:left;width:100px;height:80px'></div>" +
                "<div id='t' style='clear:both;width:100px;height:20px'></div>" +
                "</div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            // Tallest float is third at 80px height
            Assert.True(target.ContentRect.Y >= 79,
                $"clear:both should be below tallest float (80px), got Y={target.ContentRect.Y}");
        }

        // ──────────────────────────────────────────────
        // 30. Float in flex container — float is ignored
        // ──────────────────────────────────────────────
        // [CSS-FLEXBOX §3] float has no effect on flex items.
        [Fact]
        public void Float_IgnoredInFlexContainer()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='display:flex;width:400px'>" +
                "<div id='a' style='float:left;width:100px;height:50px'></div>" +
                "<div id='b' style='width:100px;height:50px'></div>" +
                "</div></body>");
            var first = LayoutTestHelper.FindById(root, "a")!;
            var second = LayoutTestHelper.FindById(root, "b")!;
            // Flex lays them out side by side regardless of float
            Assert.True(System.Math.Abs(first.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(second.ContentRect.X - 100) < 2);
            Assert.True(System.Math.Abs(first.ContentRect.Y - second.ContentRect.Y) < 2);
        }

        // ──────────────────────────────────────────────
        // 31. clear:left does not clear right floats
        // ──────────────────────────────────────────────
        // [CSS2 §9.5.2] clear:left only clears left floats, not right.
        [Fact]
        public void ClearLeft_DoesNotClearRightFloat()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='width:400px'>" +
                "<div style='float:right;width:100px;height:80px'></div>" +
                "<div id='t' style='clear:left;width:200px;height:30px'></div>" +
                "</div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            // clear:left should NOT push below the right float
            Assert.True(target.ContentRect.Y < 2,
                $"clear:left should not clear right floats, got Y={target.ContentRect.Y}");
        }

        // ──────────────────────────────────────────────
        // 32. clear:right does not clear left floats
        // ──────────────────────────────────────────────
        // [CSS2 §9.5.2] clear:right only clears right floats, not left.
        [Fact]
        public void ClearRight_DoesNotClearLeftFloat()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='width:400px'>" +
                "<div style='float:left;width:100px;height:80px'></div>" +
                "<div id='t' style='clear:right;width:200px;height:30px'></div>" +
                "</div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            // clear:right should NOT push below the left float
            Assert.True(target.ContentRect.Y < 2,
                $"clear:right should not clear left floats, got Y={target.ContentRect.Y}");
        }

        // ──────────────────────────────────────────────
        // 33. Float right with margin-right offsets from right edge
        // ──────────────────────────────────────────────
        // [CSS2 §9.5.1] margin-right on float:right creates space from container's right edge.
        // In 400px container, float 100px wide with margin-right:30px => X = 400-100-30 = 270.
        [Fact]
        public void FloatRight_WithMarginRight()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='width:400px'>" +
                "<div id='t' style='float:right;width:100px;height:50px;margin-right:30px'></div>" +
                "</div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.X - 270) < 2);
        }

        // ──────────────────────────────────────────────
        // 34. Three left floats fit on same line
        // ──────────────────────────────────────────────
        // [CSS2 §9.5.1] Three 100px floats in a 400px container all fit on one line.
        [Fact]
        public void ThreeFloatLeft_AllFitOnSameLine()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='width:400px'>" +
                "<div id='a' style='float:left;width:100px;height:50px'></div>" +
                "<div id='b' style='float:left;width:100px;height:50px'></div>" +
                "<div id='c' style='float:left;width:100px;height:50px'></div>" +
                "</div></body>");
            var first = LayoutTestHelper.FindById(root, "a")!;
            var second = LayoutTestHelper.FindById(root, "b")!;
            var third = LayoutTestHelper.FindById(root, "c")!;
            Assert.True(System.Math.Abs(first.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(second.ContentRect.X - 100) < 2);
            Assert.True(System.Math.Abs(third.ContentRect.X - 200) < 2);
            Assert.True(System.Math.Abs(first.ContentRect.Y - second.ContentRect.Y) < 2);
            Assert.True(System.Math.Abs(second.ContentRect.Y - third.ContentRect.Y) < 2);
        }

        // ──────────────────────────────────────────────
        // 35. Float with padding — content rect inside padding
        // ──────────────────────────────────────────────
        // [CSS2 §8.1] Padding shifts content rect inward, total box size includes padding.
        [Fact]
        public void FloatLeft_WithPadding()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='width:400px'>" +
                "<div id='t' style='float:left;width:100px;height:50px;padding:10px'></div>" +
                "</div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            // Content rect starts after padding
            Assert.True(System.Math.Abs(target.ContentRect.X - 10) < 2);
            Assert.True(System.Math.Abs(target.ContentRect.Width - 100) < 2);
        }

        // ──────────────────────────────────────────────
        // 36. Second left float after taller first — Y alignment
        // ──────────────────────────────────────────────
        // [CSS2 §9.5.1] Both floats start at same Y when they fit side by side.
        [Fact]
        public void TwoFloatLeft_SameY_DifferentHeight()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='width:400px'>" +
                "<div id='a' style='float:left;width:100px;height:80px'></div>" +
                "<div id='b' style='float:left;width:100px;height:40px'></div>" +
                "</div></body>");
            var first = LayoutTestHelper.FindById(root, "a")!;
            var second = LayoutTestHelper.FindById(root, "b")!;
            Assert.True(System.Math.Abs(first.ContentRect.Y - second.ContentRect.Y) < 2);
        }

        // ──────────────────────────────────────────────
        // 37. Float with border — content offset by border
        // ──────────────────────────────────────────────
        // [CSS2 §8.1] Border shifts content rect inward.
        [Fact]
        public void FloatLeft_WithBorder()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='width:400px'>" +
                "<div id='t' style='float:left;width:100px;height:50px;border:5px solid black'></div>" +
                "</div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.X - 5) < 2);
            Assert.True(System.Math.Abs(target.ContentRect.Y - 5) < 2);
            Assert.True(System.Math.Abs(target.ContentRect.Width - 100) < 2);
        }

        // ──────────────────────────────────────────────
        // 38. Float inside container with padding
        // ──────────────────────────────────────────────
        // [CSS2 §9.5.1] Float starts at container's content edge (inside padding).
        [Fact]
        public void FloatLeft_InsidePaddedContainer()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='width:400px;padding:20px'>" +
                "<div id='t' style='float:left;width:100px;height:50px'></div>" +
                "</div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            // Float positioned at container's content edge, which is at X=20 (after padding)
            Assert.True(System.Math.Abs(target.ContentRect.X - 20) < 2);
            Assert.True(System.Math.Abs(target.ContentRect.Y - 20) < 2);
        }

        // ──────────────────────────────────────────────
        // 39. Float right percentage width in narrow container
        // ──────────────────────────────────────────────
        // [CSS2 §10.3.5] 75% of 200px = 150px. float:right at X = 200-150 = 50.
        [Fact]
        public void FloatRight_PercentWidth_NarrowContainer()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='width:200px'>" +
                "<div id='t' style='float:right;width:75%;height:40px'></div>" +
                "</div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 150) < 2);
            Assert.True(System.Math.Abs(target.ContentRect.X - 50) < 2);
        }

        // ──────────────────────────────────────────────
        // 40. display:flow-root contains floats
        // ──────────────────────────────────────────────
        // [CSS-DISPLAY §3] display:flow-root establishes BFC, contains floated children.
        [Fact]
        public void FlowRoot_ContainsFloats()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div id='parent' style='width:400px;display:flow-root'>" +
                "<div style='float:left;width:100px;height:130px'></div>" +
                "</div></body>");
            var parent = LayoutTestHelper.FindById(root, "parent")!;
            Assert.True(parent.ContentRect.Height >= 129,
                $"flow-root parent should contain float, got height={parent.ContentRect.Height}");
        }

        // ──────────────────────────────────────────────
        // 41. Float right inside narrow container
        // ──────────────────────────────────────────────
        // [CSS2 §9.5.1] Float:right 100px in 150px container => X=50.
        [Fact]
        public void FloatRight_NarrowContainer()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='width:150px'>" +
                "<div id='t' style='float:right;width:100px;height:40px'></div>" +
                "</div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.X - 50) < 2);
        }

        // ──────────────────────────────────────────────
        // 42. Float left with margin between two floats
        // ──────────────────────────────────────────────
        // [CSS2 §9.5.1] Margin between floats creates gap.
        // First at X=0 (w=80), margin-right:20. Second at X=80+20=100 (margin only on first).
        [Fact]
        public void TwoFloatLeft_WithMarginBetween()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='width:400px'>" +
                "<div id='a' style='float:left;width:80px;height:40px;margin-right:20px'></div>" +
                "<div id='b' style='float:left;width:80px;height:40px'></div>" +
                "</div></body>");
            var first = LayoutTestHelper.FindById(root, "a")!;
            var second = LayoutTestHelper.FindById(root, "b")!;
            Assert.True(System.Math.Abs(first.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(second.ContentRect.X - 100) < 2);
        }

        // ──────────────────────────────────────────────
        // 43. BFC sibling avoids left float — X offset
        // ──────────────────────────────────────────────
        // [CSS2 §9.4.1] An overflow:hidden block beside a float starts at float's right edge.
        [Fact]
        public void BfcSibling_AvoidsLeftFloat_XOffset()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='width:400px'>" +
                "<div style='float:left;width:120px;height:60px'></div>" +
                "<div id='t' style='overflow:hidden;height:60px'>Content</div>" +
                "</div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(target.ContentRect.X >= 119,
                $"BFC sibling should start at X>=120, got {target.ContentRect.X}");
        }

        // ──────────────────────────────────────────────
        // 44. Float with box-sizing border-box
        // ──────────────────────────────────────────────
        // [CSS-UI §3.2] width includes padding and border when box-sizing is border-box.
        [Fact]
        public void FloatLeft_BoxSizingBorderBox()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='width:400px'>" +
                "<div id='t' style='float:left;width:100px;height:50px;padding:10px;border:5px solid;box-sizing:border-box'></div>" +
                "</div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            // border-box: total 100px, padding 10*2=20, border 5*2=10 => content = 100-20-10 = 70
            Assert.True(System.Math.Abs(target.ContentRect.Width - 70) < 2);
        }

        // ──────────────────────────────────────────────
        // 45. Float:right with box-sizing border-box position
        // ──────────────────────────────────────────────
        // [CSS2 §9.5.1] float:right position accounts for full border-box width.
        // In 400px container, 100px border-box float:right starts at X = 400 - 100 + padding + border = 315.
        [Fact]
        public void FloatRight_BoxSizingBorderBox_Position()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='width:400px'>" +
                "<div id='t' style='float:right;width:100px;height:50px;padding:10px;border:5px solid;box-sizing:border-box'></div>" +
                "</div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            // border-box width=100, so right outer edge at X=400. Content starts at X=400-100+5+10=315.
            Assert.True(System.Math.Abs(target.ContentRect.X - 315) < 2);
        }
    }
}
