using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    /// <summary>
    /// Edge-case tests for CSS 2.1 margin collapsing rules.
    /// [CSS2 §8.3.1] Collapsing margins
    /// </summary>
    public class WptMarginCollapseEdgeCaseTests
    {
        private readonly ITestOutputHelper _output;

        public WptMarginCollapseEdgeCaseTests(ITestOutputHelper output)
        {
            _output = output;
        }

        // [CSS2 §8.3.1] Adjacent sibling margins collapse; larger margin wins
        [Fact]
        public void SiblingCollapse_LargerMarginWins()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='margin-bottom:10px;height:40px'></div>" +
                "<div id='t' style='margin-top:30px;height:40px'></div>" +
                "</body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"t.Y={target.ContentRect.Y}");
            // max(10, 30) = 30, so Y = 40 + 30 = 70
            Assert.True(System.Math.Abs(target.ContentRect.Y - 70) < 2);
        }

        // [CSS2 §8.3.1] Parent-child top margin collapse when parent has no border/padding
        [Fact]
        public void ParentChildCollapse_NoBorderNoPadding()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div id='parent' style='margin-top:10px'>" +
                "<div id='child' style='margin-top:20px;height:30px'></div>" +
                "</div></body>");
            var parent = LayoutTestHelper.FindById(root, "parent")!;
            var child = LayoutTestHelper.FindById(root, "child")!;
            _output.WriteLine($"parent.Y={parent.ContentRect.Y} child.Y={child.ContentRect.Y}");
            // Margins collapse: max(10,20)=20. Parent and child start at same Y.
            Assert.True(System.Math.Abs(parent.ContentRect.Y - 20) < 2);
            Assert.True(System.Math.Abs(child.ContentRect.Y - 20) < 2);
        }

        // [CSS2 §8.3.1] Border on parent prevents parent-child margin collapse
        [Fact]
        public void NoCollapse_WithBorderTop()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div id='parent' style='margin-top:10px;border-top:1px solid black'>" +
                "<div id='child' style='margin-top:20px;height:30px'></div>" +
                "</div></body>");
            var parent = LayoutTestHelper.FindById(root, "parent")!;
            var child = LayoutTestHelper.FindById(root, "child")!;
            _output.WriteLine($"parent.Y={parent.ContentRect.Y} child.Y={child.ContentRect.Y}");
            // Parent at Y=10 (content after 1px border = 11), child at 11+20=31
            Assert.True(System.Math.Abs(parent.ContentRect.Y - 11) < 2);
            Assert.True(System.Math.Abs(child.ContentRect.Y - 31) < 2);
        }

        // [CSS2 §8.3.1] Padding on parent prevents parent-child margin collapse
        [Fact]
        public void NoCollapse_WithPaddingTop()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div id='parent' style='margin-top:10px;padding-top:5px'>" +
                "<div id='child' style='margin-top:20px;height:30px'></div>" +
                "</div></body>");
            var parent = LayoutTestHelper.FindById(root, "parent")!;
            var child = LayoutTestHelper.FindById(root, "child")!;
            _output.WriteLine($"parent.Y={parent.ContentRect.Y} child.Y={child.ContentRect.Y}");
            // Parent content starts at 10+5=15, child at 15+20=35
            Assert.True(System.Math.Abs(parent.ContentRect.Y - 15) < 2);
            Assert.True(System.Math.Abs(child.ContentRect.Y - 35) < 2);
        }

        // [CSS2 §8.3.1] overflow:hidden establishes BFC, prevents collapse with children
        [Fact]
        public void NoCollapse_OverflowHidden()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div id='parent' style='margin-top:10px;overflow:hidden'>" +
                "<div id='child' style='margin-top:20px;height:30px'></div>" +
                "</div></body>");
            var parent = LayoutTestHelper.FindById(root, "parent")!;
            var child = LayoutTestHelper.FindById(root, "child")!;
            _output.WriteLine($"parent.Y={parent.ContentRect.Y} child.Y={child.ContentRect.Y}");
            // overflow:hidden prevents collapse; parent at Y=10, child at 10+20=30
            Assert.True(System.Math.Abs(parent.ContentRect.Y - 10) < 2);
            Assert.True(System.Math.Abs(child.ContentRect.Y - 30) < 2);
        }

        // [CSS3 Flexbox §3] Flex containers establish BFC; no margin collapse with children
        [Fact]
        public void NoCollapse_InFlexContainer()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div id='parent' style='display:flex;flex-direction:column;margin-top:10px'>" +
                "<div id='child' style='margin-top:20px;height:30px'></div>" +
                "</div></body>");
            var parent = LayoutTestHelper.FindById(root, "parent")!;
            var child = LayoutTestHelper.FindById(root, "child")!;
            _output.WriteLine($"parent.Y={parent.ContentRect.Y} child.Y={child.ContentRect.Y}");
            // Flex prevents collapse; parent at Y=10, child at 10+20=30
            Assert.True(System.Math.Abs(parent.ContentRect.Y - 10) < 2);
            Assert.True(System.Math.Abs(child.ContentRect.Y - 30) < 2);
        }

        // [CSS Grid §3] Grid containers establish BFC; no margin collapse with children
        [Fact]
        public void NoCollapse_InGridContainer()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div id='parent' style='display:grid;margin-top:10px'>" +
                "<div id='child' style='margin-top:20px;height:30px'></div>" +
                "</div></body>");
            var parent = LayoutTestHelper.FindById(root, "parent")!;
            var child = LayoutTestHelper.FindById(root, "child")!;
            _output.WriteLine($"parent.Y={parent.ContentRect.Y} child.Y={child.ContentRect.Y}");
            // Grid prevents collapse; parent at Y=10, child at 10+20=30
            Assert.True(System.Math.Abs(parent.ContentRect.Y - 10) < 2);
            Assert.True(System.Math.Abs(child.ContentRect.Y - 30) < 2);
        }

        // [CSS2 §8.3.1] Two negative margins: most negative (smallest value) wins
        [Fact]
        public void NegativeMargins_MostNegativeWins()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='margin-bottom:-10px;height:40px'></div>" +
                "<div id='t' style='margin-top:-25px;height:40px'></div>" +
                "</body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"t.Y={target.ContentRect.Y}");
            // Both negative: min(-10,-25)=-25, so Y = 40 + (-25) = 15
            Assert.True(System.Math.Abs(target.ContentRect.Y - 15) < 2);
        }

        // [CSS2 §8.3.1] Mixed positive+negative: max positive + min negative
        [Fact]
        public void MixedPositiveNegative_Collapse()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='margin-bottom:30px;height:40px'></div>" +
                "<div id='t' style='margin-top:-10px;height:40px'></div>" +
                "</body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"t.Y={target.ContentRect.Y}");
            // max(30,0) + min(0,-10) = 30-10 = 20, so Y = 40+20 = 60
            Assert.True(System.Math.Abs(target.ContentRect.Y - 60) < 2);
        }

        // [CSS2 §8.3.1] Three adjacent siblings: cascade of collapses
        [Fact]
        public void ThreeSiblings_CascadeCollapse()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='margin-bottom:15px;height:20px'></div>" +
                "<div style='margin-top:10px;margin-bottom:25px;height:20px'></div>" +
                "<div id='t' style='margin-top:20px;height:20px'></div>" +
                "</body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"t.Y={target.ContentRect.Y}");
            // First gap: max(15,10)=15 => second div at Y=35
            // Second gap: max(25,20)=25 => third div at Y=35+20+25=80
            Assert.True(System.Math.Abs(target.ContentRect.Y - 80) < 2);
        }

        // [CSS2 §8.3.1] Empty block: top margin separate, bottom collapses with sibling
        [Fact]
        public void EmptyBlock_SelfCollapse()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='margin-top:15px;margin-bottom:25px'></div>" +
                "<div id='t' style='margin-top:10px;height:30px'></div>" +
                "</body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"t.Y={target.ContentRect.Y}");
            // Empty block margin-top 15 positions it, margin-bottom 25 collapses
            // with sibling margin-top 10 => max(25,10)=25, so Y = 15+25 = 40
            Assert.True(System.Math.Abs(target.ContentRect.Y - 40) < 2);
        }

        // [CSS2 §8.3.1] Parent-child top margin collapse propagates outward
        [Fact]
        public void ParentChildTop_CollapsePropagates()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='margin-top:15px'>" +
                "<div id='t' style='margin-top:30px;height:20px'></div>" +
                "</div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"t.Y={target.ContentRect.Y}");
            // max(15,30) = 30 collapsed margin; child starts at Y=30
            Assert.True(System.Math.Abs(target.ContentRect.Y - 30) < 2);
        }

        // [CSS2 §8.3.1] Last-child bottom margin collapse with parent (auto height, no bottom padding/border)
        [Fact]
        public void LastChild_BottomCollapse_WithParent()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div id='parent' style='margin-bottom:10px'>" +
                "<div style='height:40px;margin-bottom:30px'></div>" +
                "</div>" +
                "<div id='t' style='margin-top:5px;height:20px'></div>" +
                "</body>");
            var parent = LayoutTestHelper.FindById(root, "parent")!;
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"parent.Y={parent.ContentRect.Y} parent.H={parent.ContentRect.Height} t.Y={target.ContentRect.Y}");
            // Parent auto height=40. Last-child bottom margin (30) collapses with parent bottom margin (10) => max(30,10)=30
            // Then collapses with next sibling top margin (5) => max(30,5)=30
            // t.Y = 40 + 30 = 70
            Assert.True(System.Math.Abs(target.ContentRect.Y - 70) < 2);
        }

        // [CSS2 §8.3.1] Floats do not collapse margins with siblings
        [Fact]
        public void NoCollapse_WithFloat()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='float:left;width:50px;height:30px;margin-bottom:20px'></div>" +
                "<div id='t' style='clear:left;margin-top:15px;height:30px'></div>" +
                "</body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"t.Y={target.ContentRect.Y}");
            // Float margins do not collapse with cleared element margins
            // Float bottom = 30+20=50, cleared element at max(50,15)=50
            Assert.True(target.ContentRect.Y >= 30,
                $"Float should prevent normal collapse (t.Y={target.ContentRect.Y})");
        }

        // [CSS2 §8.3.1] Absolutely positioned elements do not collapse margins
        [Fact]
        public void NoCollapse_WithAbspos()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='position:relative'>" +
                "<div style='position:absolute;top:0;margin-bottom:30px;height:20px'></div>" +
                "<div id='t' style='margin-top:10px;height:20px'></div>" +
                "</div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"t.Y={target.ContentRect.Y}");
            // Abspos is out of flow; its margins do not collapse with in-flow siblings
            // t sees only its own margin-top: 10
            Assert.True(System.Math.Abs(target.ContentRect.Y - 10) < 2);
        }

        // [CSS2 §8.3.1] Inline-block does not collapse margins with parent
        [Fact]
        public void NoCollapse_InlineBlock()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div id='parent' style='margin-top:15px;display:inline-block'>" +
                "<div id='child' style='margin-top:20px;height:30px'></div>" +
                "</div></body>");
            var parent = LayoutTestHelper.FindById(root, "parent")!;
            var child = LayoutTestHelper.FindById(root, "child")!;
            _output.WriteLine($"parent.Y={parent.ContentRect.Y} child.Y={child.ContentRect.Y}");
            // Inline-block establishes BFC; child margin doesn't collapse with parent
            Assert.True(child.ContentRect.Y > parent.ContentRect.Y + 15,
                $"Inline-block should prevent collapse (parent.Y={parent.ContentRect.Y}, child.Y={child.ContentRect.Y})");
        }

        // [CSS2 §8.3.1] Collapse through multiple consecutive empty blocks
        [Fact]
        public void CollapseThrough_MultipleEmptyBlocks()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='height:20px;margin-bottom:10px'></div>" +
                "<div style='margin-top:5px;margin-bottom:8px'></div>" +
                "<div style='margin-top:3px;margin-bottom:12px'></div>" +
                "<div style='margin-top:6px;margin-bottom:4px'></div>" +
                "<div id='t' style='margin-top:7px;height:20px'></div>" +
                "</body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"t.Y={target.ContentRect.Y}");
            // All empty blocks self-collapse, then all margins collapse together
            // Positive margins: 10,5,8,3,12,6,4,7 => max = 12
            // t.Y = 20 + 12 = 32
            Assert.True(System.Math.Abs(target.ContentRect.Y - 32) < 2);
        }

        // [CSS2 §8.3.1] Zero margin collapses with positive: positive wins
        [Fact]
        public void ZeroMargin_CollapseWithPositive()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='margin-bottom:0;height:40px'></div>" +
                "<div id='t' style='margin-top:20px;height:40px'></div>" +
                "</body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"t.Y={target.ContentRect.Y}");
            // max(0,20) = 20, so Y = 40 + 20 = 60
            Assert.True(System.Math.Abs(target.ContentRect.Y - 60) < 2);
        }

        // [CSS2 §8.3.1] Zero margins on both sides: zero gap
        [Fact]
        public void ZeroMargins_BothSides()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='margin-bottom:0;height:40px'></div>" +
                "<div id='t' style='margin-top:0;height:40px'></div>" +
                "</body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"t.Y={target.ContentRect.Y}");
            // max(0,0)=0, so Y = 40
            Assert.True(System.Math.Abs(target.ContentRect.Y - 40) < 2);
        }

        // [CSS2 §8.3.1] Equal margins: collapsed margin equals either value
        [Fact]
        public void EqualMargins_Collapse()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='margin-bottom:20px;height:30px'></div>" +
                "<div id='t' style='margin-top:20px;height:30px'></div>" +
                "</body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"t.Y={target.ContentRect.Y}");
            // max(20,20)=20, so Y = 30 + 20 = 50
            Assert.True(System.Math.Abs(target.ContentRect.Y - 50) < 2);
        }

        // [CSS2 §8.3.1] Nested parent-child-grandchild collapse without border/padding
        [Fact]
        public void NestedThreeLevel_Collapse()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='margin-top:10px'>" +
                "<div style='margin-top:20px'>" +
                "<div id='t' style='margin-top:30px;height:20px'></div>" +
                "</div></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"t.Y={target.ContentRect.Y}");
            // All three collapse: max(10,20,30) = 30
            Assert.True(System.Math.Abs(target.ContentRect.Y - 30) < 2);
        }

        // [CSS2 §8.3.1] Bottom border prevents last-child bottom margin collapse
        [Fact]
        public void NoBottomCollapse_WithBorderBottom()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div id='parent' style='margin-bottom:10px;border-bottom:1px solid black'>" +
                "<div style='height:30px;margin-bottom:25px'></div>" +
                "</div>" +
                "<div id='t' style='margin-top:5px;height:20px'></div>" +
                "</body>");
            var parent = LayoutTestHelper.FindById(root, "parent")!;
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"parent.Y={parent.ContentRect.Y} parent.H={parent.ContentRect.Height} t.Y={target.ContentRect.Y}");
            // Border prevents child-parent bottom collapse
            // Parent height includes child margin: content 30+margin25=55, plus 1px border
            // Parent margin-bottom 10 collapses with t margin-top 5 => max(10,5)=10
            Assert.True(target.ContentRect.Y > 50,
                $"Border-bottom should prevent child-parent bottom collapse (t.Y={target.ContentRect.Y})");
        }

        // [CSS2 §8.3.1] overflow:auto establishes BFC, prevents collapse with children
        [Fact]
        public void NoCollapse_OverflowAuto()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div id='parent' style='margin-top:10px;overflow:auto'>" +
                "<div id='child' style='margin-top:20px;height:30px'></div>" +
                "</div></body>");
            var parent = LayoutTestHelper.FindById(root, "parent")!;
            var child = LayoutTestHelper.FindById(root, "child")!;
            _output.WriteLine($"parent.Y={parent.ContentRect.Y} child.Y={child.ContentRect.Y}");
            // overflow:auto establishes BFC; no collapse
            Assert.True(System.Math.Abs(parent.ContentRect.Y - 10) < 2);
            Assert.True(System.Math.Abs(child.ContentRect.Y - 30) < 2);
        }

        // [CSS2 §8.3.1] Empty block with negative margins between siblings
        [Fact]
        public void EmptyBlock_SelfCollapse_Negative()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='height:50px;margin-bottom:20px'></div>" +
                "<div style='margin-top:-5px;margin-bottom:-15px'></div>" +
                "<div id='t' style='margin-top:10px;height:20px'></div>" +
                "</body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"t.Y={target.ContentRect.Y}");
            // First gap: max(20,0)+min(0,-5) = 20-5 = 15, empty div at Y=65
            // Second gap: max(10,0)+min(0,-15) = 10-15 = -5, so t.Y = 65-5 = 60
            Assert.True(System.Math.Abs(target.ContentRect.Y - 60) < 2);
        }

        // [CSS2 §8.3.1] Sibling collapse: bottom-margin only (no top-margin on second)
        [Fact]
        public void SiblingCollapse_OnlyBottomMargin()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='margin-bottom:25px;height:30px'></div>" +
                "<div id='t' style='height:30px'></div>" +
                "</body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"t.Y={target.ContentRect.Y}");
            // max(25,0)=25, so Y = 30+25 = 55
            Assert.True(System.Math.Abs(target.ContentRect.Y - 55) < 2);
        }

        // [CSS2 §8.3.1] Sibling collapse: top-margin only (no bottom-margin on first)
        [Fact]
        public void SiblingCollapse_OnlyTopMargin()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='height:30px'></div>" +
                "<div id='t' style='margin-top:25px;height:30px'></div>" +
                "</body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"t.Y={target.ContentRect.Y}");
            // max(0,25)=25, so Y = 30+25 = 55
            Assert.True(System.Math.Abs(target.ContentRect.Y - 55) < 2);
        }

        // [CSS2 §8.3.1] Bottom padding prevents last-child bottom collapse
        [Fact]
        public void NoBottomCollapse_WithPaddingBottom()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div id='parent' style='margin-bottom:10px;padding-bottom:5px'>" +
                "<div style='height:30px;margin-bottom:20px'></div>" +
                "</div>" +
                "<div id='t' style='margin-top:8px;height:20px'></div>" +
                "</body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"t.Y={target.ContentRect.Y}");
            // Padding-bottom prevents child bottom margin collapsing with parent
            // Parent height: 30(child) + 20(child margin) + 5(padding) = 55 content area
            // Parent margin-bottom 10, t margin-top 8 => max(10,8) = 10
            Assert.True(target.ContentRect.Y > 55,
                $"Padding-bottom should prevent child-parent bottom collapse (t.Y={target.ContentRect.Y})");
        }

        // [CSS2 §8.3.1] Deeply nested empty blocks with parent-child collapse
        [Fact]
        public void DeeplyNestedEmpty_AllCollapse()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='height:20px;margin-bottom:5px'></div>" +
                "<div style='margin-top:10px'>" +
                "<div style='margin-top:15px'>" +
                "<div style='margin-top:8px'></div>" +
                "</div></div>" +
                "<div id='t' style='margin-top:12px;height:20px'></div>" +
                "</body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"t.Y={target.ContentRect.Y}");
            // Parent-child margins collapse: max(10,15,8)=15 for the nested group
            // First gap: max(5,15)=15, nested group at Y=35
            // Second gap: max(0,12)=12, t.Y = 35+12 = 47
            Assert.True(System.Math.Abs(target.ContentRect.Y - 47) < 2);
        }

        // [CSS2 §8.3.1] Large negative eats positive: net negative gap
        [Fact]
        public void LargeNegative_EatsPositive()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='margin-bottom:10px;height:50px'></div>" +
                "<div id='t' style='margin-top:-30px;height:40px'></div>" +
                "</body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"t.Y={target.ContentRect.Y}");
            // max(10,0) + min(0,-30) = 10-30 = -20, so Y = 50-20 = 30 (overlaps)
            Assert.True(System.Math.Abs(target.ContentRect.Y - 30) < 2);
        }

        // [CSS2 §8.3.1] Flex items (direct children) do not collapse margins between each other
        [Fact]
        public void NoCollapse_BetweenFlexItems()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='display:flex;flex-direction:column'>" +
                "<div style='margin-bottom:20px;height:30px'></div>" +
                "<div id='t' style='margin-top:15px;height:30px'></div>" +
                "</div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"t.Y={target.ContentRect.Y}");
            // Flex items do NOT collapse: gap = 20 + 15 = 35, so Y = 30 + 35 = 65
            Assert.True(System.Math.Abs(target.ContentRect.Y - 65) < 2);
        }

        // [CSS Grid §3] Grid items do not collapse margins between each other
        [Fact]
        public void NoCollapse_BetweenGridItems()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='display:grid;grid-template-rows:auto auto'>" +
                "<div style='margin-bottom:20px;height:30px'></div>" +
                "<div id='t' style='margin-top:15px;height:30px'></div>" +
                "</div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"t.Y={target.ContentRect.Y}");
            // Grid items do NOT collapse: gap = 20 + 15 = 35, so Y = 30 + 35 = 65
            Assert.True(System.Math.Abs(target.ContentRect.Y - 65) < 2);
        }
    }
}
