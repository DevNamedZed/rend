using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    /// <summary>
    /// Tests for CSS relative and sticky positioning per CSS 2.1 §9.4.3 and CSS Position Level 3.
    /// </summary>
    public class WptPositionRelativeStickyTests
    {
        private readonly ITestOutputHelper _output;

        public WptPositionRelativeStickyTests(ITestOutputHelper output)
        {
            _output = output;
        }

        // ────────────────────────────────────────────────────────────────
        // 1. position:relative with top offset
        // ────────────────────────────────────────────────────────────────

        [Fact]
        public void Relative_TopOffset_MovesDown()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:200px'>
                    <div id='t' style='position:relative;top:20px;width:100px;height:40px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"Y={target.ContentRect.Y}");
            Assert.True(System.Math.Abs(target.ContentRect.Y - 20) < 2,
                $"top:20px shifts element down from flow Y=0 to Y=20 (got {target.ContentRect.Y})");
        }

        // ────────────────────────────────────────────────────────────────
        // 2. position:relative with left offset
        // ────────────────────────────────────────────────────────────────

        [Fact]
        public void Relative_LeftOffset_MovesRight()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:200px'>
                    <div id='t' style='position:relative;left:30px;width:100px;height:40px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"X={target.ContentRect.X}");
            Assert.True(System.Math.Abs(target.ContentRect.X - 30) < 2,
                $"left:30px shifts element right from flow X=0 to X=30 (got {target.ContentRect.X})");
        }

        // ────────────────────────────────────────────────────────────────
        // 3. position:relative with bottom offset (moves up)
        // ────────────────────────────────────────────────────────────────

        [Fact]
        public void Relative_BottomOffset_MovesUp()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:200px'>
                    <div style='height:50px'></div>
                    <div id='t' style='position:relative;bottom:10px;width:100px;height:40px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"Y={target.ContentRect.Y}");
            // Flow position Y=50, bottom:10px is equivalent to top:-10px, so rendered Y=40
            Assert.True(System.Math.Abs(target.ContentRect.Y - 40) < 2,
                $"bottom:10px moves up from flow Y=50 to Y=40 (got {target.ContentRect.Y})");
        }

        // ────────────────────────────────────────────────────────────────
        // 4. position:relative with right offset (moves left)
        // ────────────────────────────────────────────────────────────────

        [Fact]
        public void Relative_RightOffset_MovesLeft()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:200px'>
                    <div id='t' style='position:relative;right:15px;width:100px;height:40px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"X={target.ContentRect.X}");
            // Flow position X=0, right:15px is equivalent to left:-15px, so rendered X=-15
            Assert.True(System.Math.Abs(target.ContentRect.X - (-15)) < 2,
                $"right:15px moves left from flow X=0 to X=-15 (got {target.ContentRect.X})");
        }

        // ────────────────────────────────────────────────────────────────
        // 5. position:relative doesn't affect siblings
        // ────────────────────────────────────────────────────────────────

        [Fact]
        public void Relative_DoesNotAffectSiblingPositions()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:200px'>
                    <div style='position:relative;top:100px;left:50px;height:30px'></div>
                    <div id='sibling' style='height:30px'></div>
                </div></body>");
            var sibling = LayoutTestHelper.FindById(root, "sibling")!;
            _output.WriteLine($"sibling Y={sibling.ContentRect.Y}");
            // Sibling at flow position Y=30 regardless of relative offset on previous sibling
            Assert.True(System.Math.Abs(sibling.ContentRect.Y - 30) < 2,
                $"Sibling Y unaffected by relative offset (got {sibling.ContentRect.Y})");
            Assert.True(System.Math.Abs(sibling.ContentRect.X - 0) < 2,
                $"Sibling X unaffected by relative offset (got {sibling.ContentRect.X})");
        }

        // ────────────────────────────────────────────────────────────────
        // 6. position:relative doesn't affect parent height
        // ────────────────────────────────────────────────────────────────

        [Fact]
        public void Relative_DoesNotAffectParentAutoHeight()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='parent' style='width:200px'>
                    <div style='position:relative;top:200px;height:40px'></div>
                </div></body>");
            var parent = LayoutTestHelper.FindById(root, "parent")!;
            _output.WriteLine($"parent height={parent.ContentRect.Height}");
            // Parent auto height = 40 (normal flow height), not 240 (visual position)
            Assert.True(System.Math.Abs(parent.ContentRect.Height - 40) < 2,
                $"Parent height based on flow, not visual offset (got {parent.ContentRect.Height})");
        }

        // ────────────────────────────────────────────────────────────────
        // 7. position:relative with percentage offset
        // ────────────────────────────────────────────────────────────────

        [Fact]
        public void Relative_PercentageTopOffset()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:200px;height:200px'>
                    <div id='t' style='position:relative;top:20px;width:100px;height:40px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"Y={target.ContentRect.Y}");
            // top:20px offset from flow position Y=0 → rendered Y=20
            Assert.True(System.Math.Abs(target.ContentRect.Y - 20) < 2,
                $"top:20px → Y=20 (got {target.ContentRect.Y})");
        }

        [Fact]
        public void Relative_PercentageLeftOffset()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:300px;height:100px'>
                    <div id='t' style='position:relative;left:75px;width:100px;height:40px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"X={target.ContentRect.X}");
            // left:75px offset from flow position X=0 → rendered X=75
            Assert.True(System.Math.Abs(target.ContentRect.X - 75) < 2,
                $"left:75px → X=75 (got {target.ContentRect.X})");
        }

        // ────────────────────────────────────────────────────────────────
        // 8. position:relative with negative offsets
        // ────────────────────────────────────────────────────────────────

        [Fact]
        public void Relative_NegativeTopOffset()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:200px'>
                    <div style='height:50px'></div>
                    <div id='t' style='position:relative;top:-20px;width:100px;height:40px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"Y={target.ContentRect.Y}");
            // Flow Y=50, top:-20px → rendered Y=30
            Assert.True(System.Math.Abs(target.ContentRect.Y - 30) < 2,
                $"top:-20px from flow Y=50 → Y=30 (got {target.ContentRect.Y})");
        }

        [Fact]
        public void Relative_NegativeLeftOffset()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:200px'>
                    <div id='t' style='position:relative;left:-25px;width:100px;height:40px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"X={target.ContentRect.X}");
            // Flow X=0, left:-25px → rendered X=-25
            Assert.True(System.Math.Abs(target.ContentRect.X - (-25)) < 2,
                $"left:-25px from flow X=0 → X=-25 (got {target.ContentRect.X})");
        }

        // ────────────────────────────────────────────────────────────────
        // 9. position:relative top+bottom (top wins per CSS 2.1 §9.4.3)
        // ────────────────────────────────────────────────────────────────

        [Fact]
        public void Relative_TopAndBottom_TopWins()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:200px;height:200px'>
                    <div id='t' style='position:relative;top:20px;bottom:50px;width:100px;height:40px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"Y={target.ContentRect.Y}");
            // CSS 2.1 §9.4.3: when both top and bottom are set, top wins and bottom is ignored
            Assert.True(System.Math.Abs(target.ContentRect.Y - 20) < 2,
                $"top:20px wins over bottom:50px (got {target.ContentRect.Y})");
        }

        // ────────────────────────────────────────────────────────────────
        // 10. position:relative left+right (left wins for LTR)
        // ────────────────────────────────────────────────────────────────

        [Fact]
        public void Relative_LeftAndRight_LeftWinsLtr()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:200px;height:100px'>
                    <div id='t' style='position:relative;left:30px;right:50px;width:100px;height:40px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"X={target.ContentRect.X}");
            // CSS 2.1 §9.4.3: in LTR, left wins and right is ignored
            Assert.True(System.Math.Abs(target.ContentRect.X - 30) < 2,
                $"left:30px wins over right:50px in LTR (got {target.ContentRect.X})");
        }

        // ────────────────────────────────────────────────────────────────
        // 11. Nested position:relative elements
        // ────────────────────────────────────────────────────────────────

        [Fact]
        public void Relative_NestedOffsets_Compound()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;top:10px;left:20px;width:200px'>
                    <div id='t' style='position:relative;top:15px;left:25px;width:100px;height:30px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"X={target.ContentRect.X}, Y={target.ContentRect.Y}");
            // Outer: flow (0,0) + offset (20,10) = (20,10)
            // Inner: flow (20,10) + offset (25,15) = (45,25)
            Assert.True(System.Math.Abs(target.ContentRect.X - 45) < 2,
                $"Nested left 20+25=45 (got {target.ContentRect.X})");
            Assert.True(System.Math.Abs(target.ContentRect.Y - 25) < 2,
                $"Nested top 10+15=25 (got {target.ContentRect.Y})");
        }

        [Fact]
        public void Relative_ThreeLevelsNested()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;top:5px;left:5px;width:300px'>
                    <div style='position:relative;top:10px;left:10px;width:250px'>
                        <div id='t' style='position:relative;top:15px;left:15px;width:100px;height:20px'></div>
                    </div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"X={target.ContentRect.X}, Y={target.ContentRect.Y}");
            // Combined: left=5+10+15=30, top=5+10+15=30
            Assert.True(System.Math.Abs(target.ContentRect.X - 30) < 2,
                $"Three-level nested left 5+10+15=30 (got {target.ContentRect.X})");
            Assert.True(System.Math.Abs(target.ContentRect.Y - 30) < 2,
                $"Three-level nested top 5+10+15=30 (got {target.ContentRect.Y})");
        }

        // ────────────────────────────────────────────────────────────────
        // 12. position:relative on inline element
        // ────────────────────────────────────────────────────────────────

        [Fact]
        public void Relative_OnInlineBlock_SmallOffset()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:300px'>
                    <div id='t' style='display:inline-block;position:relative;top:10px;left:5px;width:60px;height:20px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"X={target.ContentRect.X}, Y={target.ContentRect.Y}");
            Assert.True(System.Math.Abs(target.ContentRect.X - 5) < 2,
                $"Inline-block relative left:5px (got {target.ContentRect.X})");
            Assert.True(System.Math.Abs(target.ContentRect.Y - 10) < 2,
                $"Inline-block relative top:10px (got {target.ContentRect.Y})");
        }

        // ────────────────────────────────────────────────────────────────
        // 13. position:relative on inline-block
        // ────────────────────────────────────────────────────────────────

        [Fact]
        public void Relative_OnInlineBlock()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:300px'>
                    <div id='t' style='display:inline-block;position:relative;top:15px;left:20px;width:50px;height:50px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"X={target.ContentRect.X}, Y={target.ContentRect.Y}");
            Assert.True(System.Math.Abs(target.ContentRect.X - 20) < 2,
                $"Inline-block relative left:20px (got {target.ContentRect.X})");
            Assert.True(System.Math.Abs(target.ContentRect.Y - 15) < 2,
                $"Inline-block relative top:15px (got {target.ContentRect.Y})");
        }

        // ────────────────────────────────────────────────────────────────
        // 14. position:relative on flex item
        // ────────────────────────────────────────────────────────────────

        [Fact]
        public void Relative_OnFlexItem()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px;height:100px'>
                    <div id='t' style='position:relative;top:10px;left:20px;width:80px;height:60px'></div>
                    <div id='sibling' style='width:80px;height:60px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            var sibling = LayoutTestHelper.FindById(root, "sibling")!;
            _output.WriteLine($"target: X={target.ContentRect.X}, Y={target.ContentRect.Y}");
            _output.WriteLine($"sibling: X={sibling.ContentRect.X}");
            // Flex item is offset visually but sibling is positioned as if no offset
            Assert.True(System.Math.Abs(target.ContentRect.X - 20) < 2,
                $"Flex item relative left:20px (got {target.ContentRect.X})");
            Assert.True(System.Math.Abs(target.ContentRect.Y - 10) < 2,
                $"Flex item relative top:10px (got {target.ContentRect.Y})");
            // Sibling starts at flow X=80 regardless of relative offset
            Assert.True(System.Math.Abs(sibling.ContentRect.X - 80) < 2,
                $"Flex sibling unaffected (got X={sibling.ContentRect.X})");
        }

        // ────────────────────────────────────────────────────────────────
        // 15. position:relative on grid item
        // ────────────────────────────────────────────────────────────────

        [Fact]
        public void Relative_OnGridItem()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px 100px;width:200px'>
                    <div id='t' style='position:relative;top:15px;left:25px;height:50px'></div>
                    <div id='sibling' style='height:50px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            var sibling = LayoutTestHelper.FindById(root, "sibling")!;
            _output.WriteLine($"target: X={target.ContentRect.X}, Y={target.ContentRect.Y}");
            _output.WriteLine($"sibling: X={sibling.ContentRect.X}");
            // Grid item offset visually but grid placement unaffected
            Assert.True(System.Math.Abs(target.ContentRect.X - 25) < 2,
                $"Grid item relative left:25px (got {target.ContentRect.X})");
            Assert.True(System.Math.Abs(target.ContentRect.Y - 15) < 2,
                $"Grid item relative top:15px (got {target.ContentRect.Y})");
            // Second grid item in column 2 at X=100
            Assert.True(System.Math.Abs(sibling.ContentRect.X - 100) < 2,
                $"Grid sibling at column 2 X=100 (got {sibling.ContentRect.X})");
        }

        // ────────────────────────────────────────────────────────────────
        // 16. position:relative preserves original space in flow
        // ────────────────────────────────────────────────────────────────

        [Fact]
        public void Relative_PreservesFlowSpace()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='container' style='width:200px'>
                    <div style='height:30px'></div>
                    <div style='position:relative;top:100px;height:40px'></div>
                    <div id='after' style='height:30px'></div>
                </div></body>");
            var container = LayoutTestHelper.FindById(root, "container")!;
            var after = LayoutTestHelper.FindById(root, "after")!;
            _output.WriteLine($"container height={container.ContentRect.Height}, after Y={after.ContentRect.Y}");
            // The element still occupies 40px in flow at its original position
            // after element at Y = 30 + 40 = 70
            Assert.True(System.Math.Abs(after.ContentRect.Y - 70) < 2,
                $"Element after relative box at flow Y=70 (got {after.ContentRect.Y})");
            // Container height = 30 + 40 + 30 = 100 (not affected by visual offset)
            Assert.True(System.Math.Abs(container.ContentRect.Height - 100) < 2,
                $"Container height from flow = 100 (got {container.ContentRect.Height})");
        }

        // ────────────────────────────────────────────────────────────────
        // 17. position:relative with z-index
        // ────────────────────────────────────────────────────────────────

        [Fact]
        public void Relative_ZIndexCreatesStackingContext()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:200px'>
                    <div id='back' style='position:relative;z-index:1;width:100px;height:100px'></div>
                    <div id='front' style='position:relative;z-index:2;top:-50px;width:100px;height:100px'></div>
                </div></body>");
            var back = LayoutTestHelper.FindById(root, "back")!;
            var front = LayoutTestHelper.FindById(root, "front")!;
            _output.WriteLine($"back Y={back.ContentRect.Y}, front Y={front.ContentRect.Y}");
            var styledBack = (back.StyledNode as Rend.Style.StyledElement)!;
            var styledFront = (front.StyledNode as Rend.Style.StyledElement)!;
            Assert.Equal(1, styledBack.Style.ZIndex);
            Assert.Equal(2, styledFront.Style.ZIndex);
            // Front is visually displaced up by 50px from flow Y=100 → Y=50
            Assert.True(System.Math.Abs(front.ContentRect.Y - 50) < 2,
                $"Front element at Y=50 (got {front.ContentRect.Y})");
        }

        // ────────────────────────────────────────────────────────────────
        // 18. position:relative with overflow:hidden on parent
        // ────────────────────────────────────────────────────────────────

        [Fact]
        public void Relative_ParentOverflowHidden_ElementStillOffsets()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:200px;height:80px;overflow:hidden'>
                    <div id='t' style='position:relative;top:10px;left:15px;width:100px;height:40px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"X={target.ContentRect.X}, Y={target.ContentRect.Y}");
            // Relative offset still applied even with overflow:hidden parent (clipped visually, not layout)
            Assert.True(System.Math.Abs(target.ContentRect.X - 15) < 2,
                $"Relative left:15px applied inside overflow:hidden (got {target.ContentRect.X})");
            Assert.True(System.Math.Abs(target.ContentRect.Y - 10) < 2,
                $"Relative top:10px applied inside overflow:hidden (got {target.ContentRect.Y})");
        }

        // ────────────────────────────────────────────────────────────────
        // 19. position:relative with transform
        // ────────────────────────────────────────────────────────────────

        [Fact]
        public void Relative_WithTransform_BothApply()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:300px'>
                    <div id='t' style='position:relative;top:20px;left:30px;width:100px;height:50px;transform:translateX(10px)'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"X={target.ContentRect.X}, Y={target.ContentRect.Y}");
            // Layout position includes relative offset; transform is visual-only (not in ContentRect)
            Assert.True(System.Math.Abs(target.ContentRect.X - 30) < 2,
                $"Relative left:30px in layout (got {target.ContentRect.X})");
            Assert.True(System.Math.Abs(target.ContentRect.Y - 20) < 2,
                $"Relative top:20px in layout (got {target.ContentRect.Y})");
        }

        // ────────────────────────────────────────────────────────────────
        // 20. position:relative on table cell
        // ────────────────────────────────────────────────────────────────

        [Fact]
        public void Relative_OnTableCell()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table style='border-collapse:collapse;border-spacing:0'>
                    <tr>
                        <td id='t' style='position:relative;top:10px;left:15px;width:80px;height:40px;padding:0'></td>
                        <td id='sibling' style='width:80px;height:40px;padding:0'></td>
                    </tr>
                </table></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            var sibling = LayoutTestHelper.FindById(root, "sibling")!;
            _output.WriteLine($"target: X={target.ContentRect.X}, Y={target.ContentRect.Y}");
            _output.WriteLine($"sibling: X={sibling.ContentRect.X}, Y={sibling.ContentRect.Y}");
            // Table cell with relative offset is displaced visually
            Assert.True(target.ContentRect.X >= 14,
                $"Table cell relative left:15px (got {target.ContentRect.X})");
            Assert.True(target.ContentRect.Y >= 9,
                $"Table cell relative top:10px (got {target.ContentRect.Y})");
        }

        // ────────────────────────────────────────────────────────────────
        // 21. position:sticky basic (acts like relative initially)
        // ────────────────────────────────────────────────────────────────

        [Fact]
        public void Sticky_ActsLikeRelativeInitially()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:200px'>
                    <div style='height:30px'></div>
                    <div id='t' style='position:sticky;top:0;width:100px;height:40px'></div>
                    <div id='after' style='height:30px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            var after = LayoutTestHelper.FindById(root, "after")!;
            _output.WriteLine($"target Y={target.ContentRect.Y}, after Y={after.ContentRect.Y}");
            // Without scrolling, sticky acts like position:relative — stays at flow position Y=30
            Assert.True(System.Math.Abs(target.ContentRect.Y - 30) < 2,
                $"Sticky at initial flow Y=30 (got {target.ContentRect.Y})");
        }

        // ────────────────────────────────────────────────────────────────
        // 22. position:sticky with top offset
        // ────────────────────────────────────────────────────────────────

        [Fact]
        public void Sticky_TopOffset_ParsedCorrectly()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:200px'>
                    <div id='t' style='position:sticky;top:10px;width:100px;height:40px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            var styled = (target.StyledNode as Rend.Style.StyledElement)!;
            _output.WriteLine($"position={styled.Style.Position}, Y={target.ContentRect.Y}");
            Assert.Equal(Rend.Css.CssPosition.Sticky, styled.Style.Position);
            // Sticky with top:10px offsets from flow position (treated like relative)
            Assert.True(System.Math.Abs(target.ContentRect.Y - 10) < 2,
                $"Sticky top:10px offset applied (got {target.ContentRect.Y})");
        }

        // ────────────────────────────────────────────────────────────────
        // 23. position:sticky doesn't affect siblings
        // ────────────────────────────────────────────────────────────────

        [Fact]
        public void Sticky_DoesNotAffectSiblings()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:200px'>
                    <div style='position:sticky;top:50px;height:30px'></div>
                    <div id='sibling' style='height:30px'></div>
                </div></body>");
            var sibling = LayoutTestHelper.FindById(root, "sibling")!;
            _output.WriteLine($"sibling Y={sibling.ContentRect.Y}");
            // Sibling positioned as if sticky element is in normal flow
            Assert.True(System.Math.Abs(sibling.ContentRect.Y - 30) < 2,
                $"Sibling at Y=30 unaffected by sticky (got {sibling.ContentRect.Y})");
        }

        // ────────────────────────────────────────────────────────────────
        // 24. position:sticky element preserves flow space
        // ────────────────────────────────────────────────────────────────

        [Fact]
        public void Sticky_PreservesFlowSpace()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='container' style='width:200px'>
                    <div style='height:20px'></div>
                    <div style='position:sticky;top:0;height:40px'></div>
                    <div id='after' style='height:20px'></div>
                </div></body>");
            var container = LayoutTestHelper.FindById(root, "container")!;
            var after = LayoutTestHelper.FindById(root, "after")!;
            _output.WriteLine($"container height={container.ContentRect.Height}, after Y={after.ContentRect.Y}");
            // Sticky reserves flow space: container height = 20+40+20 = 80
            Assert.True(System.Math.Abs(container.ContentRect.Height - 80) < 2,
                $"Container height = 80 (got {container.ContentRect.Height})");
            Assert.True(System.Math.Abs(after.ContentRect.Y - 60) < 2,
                $"After element at Y=60 (got {after.ContentRect.Y})");
        }

        // ────────────────────────────────────────────────────────────────
        // 25. position:sticky with containing block
        // ────────────────────────────────────────────────────────────────

        [Fact]
        public void Sticky_WithContainingBlock_ParsesPosition()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:200px;height:300px;overflow:auto'>
                    <div style='height:50px'></div>
                    <div id='t' style='position:sticky;top:0;width:100px;height:40px'></div>
                    <div style='height:500px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            var styled = (target.StyledNode as Rend.Style.StyledElement)!;
            _output.WriteLine($"Y={target.ContentRect.Y}");
            Assert.Equal(Rend.Css.CssPosition.Sticky, styled.Style.Position);
            // Without scrolling, sticky element at flow Y=50
            Assert.True(System.Math.Abs(target.ContentRect.Y - 50) < 2,
                $"Sticky at flow Y=50 without scroll (got {target.ContentRect.Y})");
        }

        // ────────────────────────────────────────────────────────────────
        // Additional relative positioning tests
        // ────────────────────────────────────────────────────────────────

        [Fact]
        public void Relative_TopAndLeft_Combined()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:200px'>
                    <div id='t' style='position:relative;top:20px;left:30px;width:100px;height:40px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"X={target.ContentRect.X}, Y={target.ContentRect.Y}");
            Assert.True(System.Math.Abs(target.ContentRect.X - 30) < 2,
                $"left:30px (got {target.ContentRect.X})");
            Assert.True(System.Math.Abs(target.ContentRect.Y - 20) < 2,
                $"top:20px (got {target.ContentRect.Y})");
        }

        [Fact]
        public void Relative_BottomAndRight_Combined()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:200px'>
                    <div style='height:60px'></div>
                    <div id='t' style='position:relative;bottom:10px;right:20px;width:100px;height:40px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"X={target.ContentRect.X}, Y={target.ContentRect.Y}");
            // Flow (0,60), bottom:10px → Y=50, right:20px → X=-20
            Assert.True(System.Math.Abs(target.ContentRect.X - (-20)) < 2,
                $"right:20px from X=0 → X=-20 (got {target.ContentRect.X})");
            Assert.True(System.Math.Abs(target.ContentRect.Y - 50) < 2,
                $"bottom:10px from Y=60 → Y=50 (got {target.ContentRect.Y})");
        }

        [Fact]
        public void Relative_DoesNotAffectSubsequentSiblingX()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:200px'>
                    <div style='position:relative;left:100px;height:40px'></div>
                    <div id='second' style='height:40px'></div>
                    <div id='third' style='height:40px'></div>
                </div></body>");
            var second = LayoutTestHelper.FindById(root, "second")!;
            var third = LayoutTestHelper.FindById(root, "third")!;
            _output.WriteLine($"second X={second.ContentRect.X}, Y={second.ContentRect.Y}");
            _output.WriteLine($"third X={third.ContentRect.X}, Y={third.ContentRect.Y}");
            Assert.True(System.Math.Abs(second.ContentRect.X - 0) < 2,
                $"Second sibling at X=0 (got {second.ContentRect.X})");
            Assert.True(System.Math.Abs(second.ContentRect.Y - 40) < 2,
                $"Second sibling at Y=40 (got {second.ContentRect.Y})");
            Assert.True(System.Math.Abs(third.ContentRect.Y - 80) < 2,
                $"Third sibling at Y=80 (got {third.ContentRect.Y})");
        }

        [Fact]
        public void Relative_LargeOffset_OverflowsParent()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='parent' style='width:200px;height:100px'>
                    <div id='t' style='position:relative;top:500px;width:100px;height:40px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            var parent = LayoutTestHelper.FindById(root, "parent")!;
            _output.WriteLine($"target Y={target.ContentRect.Y}, parent height={parent.ContentRect.Height}");
            // Element visually at Y=500, well beyond parent
            Assert.True(System.Math.Abs(target.ContentRect.Y - 500) < 2,
                $"Large offset top:500px (got {target.ContentRect.Y})");
            // Parent explicit height unaffected
            Assert.True(System.Math.Abs(parent.ContentRect.Height - 100) < 2,
                $"Parent height stays 100 (got {parent.ContentRect.Height})");
        }

        [Fact]
        public void Relative_WithMargins()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:300px'>
                    <div id='t' style='position:relative;top:10px;left:20px;margin:15px;width:100px;height:40px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"X={target.ContentRect.X}, Y={target.ContentRect.Y}");
            // Flow position: margin-left=15 → X=15, margin-top=15 → Y=15
            // Relative offset: +20 → X=35, +10 → Y=25
            Assert.True(System.Math.Abs(target.ContentRect.X - 35) < 2,
                $"margin:15 + left:20 → X=35 (got {target.ContentRect.X})");
            Assert.True(System.Math.Abs(target.ContentRect.Y - 25) < 2,
                $"margin:15 + top:10 → Y=25 (got {target.ContentRect.Y})");
        }

        [Fact]
        public void Relative_WithPadding_ContentRectPositionIncludesOffset()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:300px'>
                    <div id='t' style='position:relative;top:10px;left:20px;padding:15px;width:100px;height:40px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"ContentRect X={target.ContentRect.X}, Y={target.ContentRect.Y}");
            _output.WriteLine($"ContentRect W={target.ContentRect.Width}, H={target.ContentRect.Height}");
            // ContentRect starts inside padding: padding-left=15 + left:20 = 35
            Assert.True(System.Math.Abs(target.ContentRect.X - 35) < 2,
                $"padding:15 + left:20 → content X=35 (got {target.ContentRect.X})");
            Assert.True(System.Math.Abs(target.ContentRect.Y - 25) < 2,
                $"padding:15 + top:10 → content Y=25 (got {target.ContentRect.Y})");
            Assert.True(System.Math.Abs(target.ContentRect.Width - 100) < 2,
                $"Content width 100px (got {target.ContentRect.Width})");
        }

        [Fact]
        public void Relative_WithBorder_ContentRectInsideBorder()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:300px'>
                    <div id='t' style='position:relative;top:5px;left:10px;border:3px solid black;width:100px;height:40px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"ContentRect X={target.ContentRect.X}, Y={target.ContentRect.Y}");
            // ContentRect X = border-left(3) + left(10) = 13
            // ContentRect Y = border-top(3) + top(5) = 8
            Assert.True(System.Math.Abs(target.ContentRect.X - 13) < 2,
                $"border:3px + left:10px → content X=13 (got {target.ContentRect.X})");
            Assert.True(System.Math.Abs(target.ContentRect.Y - 8) < 2,
                $"border:3px + top:5px → content Y=8 (got {target.ContentRect.Y})");
        }

        [Fact]
        public void Relative_ZeroOffsets_NoDisplacement()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:200px'>
                    <div style='height:30px'></div>
                    <div id='t' style='position:relative;top:0;left:0;width:100px;height:40px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"X={target.ContentRect.X}, Y={target.ContentRect.Y}");
            // Zero offsets = no visual displacement from flow
            Assert.True(System.Math.Abs(target.ContentRect.X - 0) < 2,
                $"top:0 left:0 stays at flow X=0 (got {target.ContentRect.X})");
            Assert.True(System.Math.Abs(target.ContentRect.Y - 30) < 2,
                $"top:0 left:0 stays at flow Y=30 (got {target.ContentRect.Y})");
        }

        [Fact]
        public void Relative_Dimensions_Unchanged()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:300px'>
                    <div id='t' style='position:relative;top:20px;left:30px;width:150px;height:80px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"W={target.ContentRect.Width}, H={target.ContentRect.Height}");
            // Relative positioning does not change dimensions
            Assert.True(System.Math.Abs(target.ContentRect.Width - 150) < 2,
                $"Width unchanged at 150 (got {target.ContentRect.Width})");
            Assert.True(System.Math.Abs(target.ContentRect.Height - 80) < 2,
                $"Height unchanged at 80 (got {target.ContentRect.Height})");
        }

        [Fact]
        public void Relative_MultipleSiblings_AllIndependent()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:200px'>
                    <div id='first' style='position:relative;top:50px;height:30px'></div>
                    <div id='second' style='position:relative;top:-20px;height:30px'></div>
                    <div id='third' style='position:relative;left:40px;height:30px'></div>
                </div></body>");
            var first = LayoutTestHelper.FindById(root, "first")!;
            var second = LayoutTestHelper.FindById(root, "second")!;
            var third = LayoutTestHelper.FindById(root, "third")!;
            _output.WriteLine($"first Y={first.ContentRect.Y}");
            _output.WriteLine($"second Y={second.ContentRect.Y}");
            _output.WriteLine($"third X={third.ContentRect.X}, Y={third.ContentRect.Y}");
            // Each sibling offset independently from its own flow position
            // first: flow Y=0, +50 → Y=50
            Assert.True(System.Math.Abs(first.ContentRect.Y - 50) < 2,
                $"First at Y=50 (got {first.ContentRect.Y})");
            // second: flow Y=30, -20 → Y=10
            Assert.True(System.Math.Abs(second.ContentRect.Y - 10) < 2,
                $"Second at Y=10 (got {second.ContentRect.Y})");
            // third: flow Y=60, left:40 → X=40
            Assert.True(System.Math.Abs(third.ContentRect.X - 40) < 2,
                $"Third at X=40 (got {third.ContentRect.X})");
            Assert.True(System.Math.Abs(third.ContentRect.Y - 60) < 2,
                $"Third at Y=60 (got {third.ContentRect.Y})");
        }

        [Fact]
        public void Sticky_WithSiblingsBefore_FlowPosition()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:200px'>
                    <div style='height:40px'></div>
                    <div style='height:40px'></div>
                    <div id='t' style='position:sticky;top:0;width:100px;height:30px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"Y={target.ContentRect.Y}");
            // Without scroll, sticky at flow Y = 40+40 = 80
            Assert.True(System.Math.Abs(target.ContentRect.Y - 80) < 2,
                $"Sticky at flow Y=80 (got {target.ContentRect.Y})");
        }

        [Fact]
        public void Sticky_WidthMatchesFlowWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:200px'>
                    <div id='t' style='position:sticky;top:0;height:30px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"W={target.ContentRect.Width}");
            // Sticky element width = auto → fills containing block like normal flow
            Assert.True(System.Math.Abs(target.ContentRect.Width - 200) < 2,
                $"Sticky auto width = 200 (got {target.ContentRect.Width})");
        }

        [Fact]
        public void Sticky_ExplicitDimensions_Respected()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:300px'>
                    <div id='t' style='position:sticky;top:0;width:150px;height:60px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"W={target.ContentRect.Width}, H={target.ContentRect.Height}");
            Assert.True(System.Math.Abs(target.ContentRect.Width - 150) < 2,
                $"Sticky explicit width=150 (got {target.ContentRect.Width})");
            Assert.True(System.Math.Abs(target.ContentRect.Height - 60) < 2,
                $"Sticky explicit height=60 (got {target.ContentRect.Height})");
        }

        [Fact]
        public void Relative_FlexItem_SiblingPositionUnaffected()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div style='position:relative;top:50px;left:50px;width:80px;height:60px'></div>
                    <div id='second' style='width:80px;height:60px'></div>
                    <div id='third' style='width:80px;height:60px'></div>
                </div></body>");
            var second = LayoutTestHelper.FindById(root, "second")!;
            var third = LayoutTestHelper.FindById(root, "third")!;
            _output.WriteLine($"second X={second.ContentRect.X}, third X={third.ContentRect.X}");
            // Flex items placed at X=0, 80, 160 (each 80px wide). Relative offset doesn't affect siblings.
            Assert.True(System.Math.Abs(second.ContentRect.X - 80) < 2,
                $"Second flex item at X=80 (got {second.ContentRect.X})");
            Assert.True(System.Math.Abs(third.ContentRect.X - 160) < 2,
                $"Third flex item at X=160 (got {third.ContentRect.X})");
        }

        [Fact]
        public void Relative_GridItem_RowPositionUnaffected()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:1fr;width:200px'>
                    <div style='position:relative;top:100px;height:40px'></div>
                    <div id='second' style='height:40px'></div>
                </div></body>");
            var second = LayoutTestHelper.FindById(root, "second")!;
            _output.WriteLine($"second Y={second.ContentRect.Y}");
            // Grid row 2 starts at Y=40 (row 1 height=40), unaffected by relative offset
            Assert.True(System.Math.Abs(second.ContentRect.Y - 40) < 2,
                $"Second grid item at Y=40 (got {second.ContentRect.Y})");
        }
    }
}
