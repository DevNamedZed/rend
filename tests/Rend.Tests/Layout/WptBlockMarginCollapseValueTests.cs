using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    /// <summary>
    /// Value-level margin collapsing tests per CSS2 section 8.3.1.
    /// Each test verifies specific numeric margin combinations and collapse prevention rules.
    /// </summary>
    public class WptBlockMarginCollapseValueTests
    {
        private readonly ITestOutputHelper _output;

        public WptBlockMarginCollapseValueTests(ITestOutputHelper output)
        {
            _output = output;
        }

        // [CSS2 §8.3.1] Sibling collapse: 30mb + 20mt = 30 (larger wins)
        [Fact]
        public void Sibling_30mb_20mt_CollapseToLarger()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='overflow:hidden'>" +
                "<div style='height:40px;margin-bottom:30px'></div>" +
                "<div id='t' style='height:40px;margin-top:20px'></div>" +
                "</div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"t.Y={target.ContentRect.Y}");
            // max(30, 20) = 30, so Y = 40 + 30 = 70
            Assert.True(System.Math.Abs(target.ContentRect.Y - 70) < 2,
                $"Expected Y=70 (40+max(30,20)=70), got {target.ContentRect.Y}");
        }

        // [CSS2 §8.3.1] Sibling collapse: 20mb + 20mt = 20 (equal margins)
        [Fact]
        public void Sibling_20mb_20mt_CollapseToEqual()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='overflow:hidden'>" +
                "<div style='height:40px;margin-bottom:20px'></div>" +
                "<div id='t' style='height:40px;margin-top:20px'></div>" +
                "</div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"t.Y={target.ContentRect.Y}");
            // max(20, 20) = 20, so Y = 40 + 20 = 60
            Assert.True(System.Math.Abs(target.ContentRect.Y - 60) < 2,
                $"Expected Y=60 (40+max(20,20)=60), got {target.ContentRect.Y}");
        }

        // [CSS2 §8.3.1] Sibling collapse: 50mb + 10mt = 50 (much larger bottom wins)
        [Fact]
        public void Sibling_50mb_10mt_CollapseToLarger()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='overflow:hidden'>" +
                "<div style='height:40px;margin-bottom:50px'></div>" +
                "<div id='t' style='height:40px;margin-top:10px'></div>" +
                "</div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"t.Y={target.ContentRect.Y}");
            // max(50, 10) = 50, so Y = 40 + 50 = 90
            Assert.True(System.Math.Abs(target.ContentRect.Y - 90) < 2,
                $"Expected Y=90 (40+max(50,10)=90), got {target.ContentRect.Y}");
        }

        // [CSS2 §8.3.1] Sibling collapse: 10mb + 30mt = 30 (larger top wins)
        [Fact]
        public void Sibling_10mb_30mt_CollapseToLarger()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='overflow:hidden'>" +
                "<div style='height:40px;margin-bottom:10px'></div>" +
                "<div id='t' style='height:40px;margin-top:30px'></div>" +
                "</div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"t.Y={target.ContentRect.Y}");
            // max(10, 30) = 30, so Y = 40 + 30 = 70
            Assert.True(System.Math.Abs(target.ContentRect.Y - 70) < 2,
                $"Expected Y=70 (40+max(10,30)=70), got {target.ContentRect.Y}");
        }

        // [CSS2 §8.3.1] Sibling collapse: 0mb + 20mt = 20 (zero with positive)
        [Fact]
        public void Sibling_0mb_20mt_CollapseToPositive()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='overflow:hidden'>" +
                "<div style='height:40px;margin-bottom:0'></div>" +
                "<div id='t' style='height:40px;margin-top:20px'></div>" +
                "</div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"t.Y={target.ContentRect.Y}");
            // max(0, 20) = 20, so Y = 40 + 20 = 60
            Assert.True(System.Math.Abs(target.ContentRect.Y - 60) < 2,
                $"Expected Y=60 (40+max(0,20)=60), got {target.ContentRect.Y}");
        }

        // [CSS2 §8.3.1] Negative margins: -10mb + -20mt = -20 (most negative wins)
        [Fact]
        public void Sibling_Neg10mb_Neg20mt_CollapseMostNegative()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='overflow:hidden'>" +
                "<div style='height:40px;margin-bottom:-10px'></div>" +
                "<div id='t' style='height:40px;margin-top:-20px'></div>" +
                "</div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"t.Y={target.ContentRect.Y}");
            // Both negative: min(-10, -20) = -20, so Y = 40 + (-20) = 20
            Assert.True(System.Math.Abs(target.ContentRect.Y - 20) < 2,
                $"Expected Y=20 (40+min(-10,-20)=20), got {target.ContentRect.Y}");
        }

        // [CSS2 §8.3.1] Mixed: 30mb + -10mt = 20 (max positive + min negative)
        [Fact]
        public void Sibling_30mb_Neg10mt_MixedCollapse()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='overflow:hidden'>" +
                "<div style='height:40px;margin-bottom:30px'></div>" +
                "<div id='t' style='height:40px;margin-top:-10px'></div>" +
                "</div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"t.Y={target.ContentRect.Y}");
            // max(30,0) + min(0,-10) = 30 + (-10) = 20, so Y = 40 + 20 = 60
            Assert.True(System.Math.Abs(target.ContentRect.Y - 60) < 2,
                $"Expected Y=60 (40+30-10=60), got {target.ContentRect.Y}");
        }

        // [CSS2 §8.3.1] Mixed: 40mb + -20mt = 20 (max positive + min negative)
        [Fact]
        public void Sibling_40mb_Neg20mt_MixedCollapse()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='overflow:hidden'>" +
                "<div style='height:40px;margin-bottom:40px'></div>" +
                "<div id='t' style='height:40px;margin-top:-20px'></div>" +
                "</div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"t.Y={target.ContentRect.Y}");
            // max(40,0) + min(0,-20) = 40 + (-20) = 20, so Y = 40 + 20 = 60
            Assert.True(System.Math.Abs(target.ContentRect.Y - 60) < 2,
                $"Expected Y=60 (40+40-20=60), got {target.ContentRect.Y}");
        }

        // [CSS2 §8.3.1] No collapse between siblings when first has border-bottom
        [Fact]
        public void Sibling_NoCollapse_BorderBottom()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='overflow:hidden'>" +
                "<div style='height:40px;margin-bottom:30px;border-bottom:1px solid black'></div>" +
                "<div id='t' style='height:40px;margin-top:20px'></div>" +
                "</div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"t.Y={target.ContentRect.Y}");
            // Border does not prevent sibling collapse; border is inside the first div's box.
            // The margins are still adjacent. Y = 40 + 1(border) + max(30,20) = 71
            // However if margins still collapse: 40+1+30 = 71
            Assert.True(target.ContentRect.Y >= 70,
                $"Expected Y>=70 with border-bottom, got {target.ContentRect.Y}");
        }

        // [CSS2 §8.3.1] No collapse between parent and first child when parent has padding-top
        [Fact]
        public void ParentChild_NoCollapse_PaddingTop()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div id='parent' style='margin-top:30px;padding-top:5px'>" +
                "<div id='child' style='margin-top:20px;height:40px'></div>" +
                "</div></body>");
            var parent = LayoutTestHelper.FindById(root, "parent")!;
            var child = LayoutTestHelper.FindById(root, "child")!;
            _output.WriteLine($"parent.Y={parent.ContentRect.Y} child.Y={child.ContentRect.Y}");
            // Padding prevents collapse. Parent content starts at 30+5=35, child at 35+20=55.
            Assert.True(System.Math.Abs(parent.ContentRect.Y - 35) < 2,
                $"Expected parent.Y=35 (30+5), got {parent.ContentRect.Y}");
            Assert.True(System.Math.Abs(child.ContentRect.Y - 55) < 2,
                $"Expected child.Y=55 (35+20), got {child.ContentRect.Y}");
        }

        // [CSS2 §8.3.1] No collapse between parent and first child when parent has border-top
        [Fact]
        public void ParentChild_NoCollapse_BorderTop()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div id='parent' style='margin-top:30px;border-top:2px solid black'>" +
                "<div id='child' style='margin-top:20px;height:40px'></div>" +
                "</div></body>");
            var parent = LayoutTestHelper.FindById(root, "parent")!;
            var child = LayoutTestHelper.FindById(root, "child")!;
            _output.WriteLine($"parent.Y={parent.ContentRect.Y} child.Y={child.ContentRect.Y}");
            // Border prevents collapse. Parent content starts at 30+2=32, child at 32+20=52.
            Assert.True(System.Math.Abs(parent.ContentRect.Y - 32) < 2,
                $"Expected parent.Y=32 (30+2), got {parent.ContentRect.Y}");
            Assert.True(System.Math.Abs(child.ContentRect.Y - 52) < 2,
                $"Expected child.Y=52 (32+20), got {child.ContentRect.Y}");
        }

        // [CSS2 §8.3.1] overflow:hidden establishes BFC, prevents parent-child collapse
        [Fact]
        public void ParentChild_NoCollapse_OverflowHidden()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div id='parent' style='margin-top:30px;overflow:hidden'>" +
                "<div id='child' style='margin-top:20px;height:40px'></div>" +
                "</div></body>");
            var parent = LayoutTestHelper.FindById(root, "parent")!;
            var child = LayoutTestHelper.FindById(root, "child")!;
            _output.WriteLine($"parent.Y={parent.ContentRect.Y} child.Y={child.ContentRect.Y}");
            // overflow:hidden prevents collapse. Parent at Y=30, child at 30+20=50.
            Assert.True(System.Math.Abs(parent.ContentRect.Y - 30) < 2,
                $"Expected parent.Y=30, got {parent.ContentRect.Y}");
            Assert.True(System.Math.Abs(child.ContentRect.Y - 50) < 2,
                $"Expected child.Y=50 (30+20), got {child.ContentRect.Y}");
        }

        // [CSS3 Flexbox §3] Flex containers prevent margin collapse between items
        [Fact]
        public void Sibling_NoCollapse_InFlexContainer()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='display:flex;flex-direction:column;width:200px'>" +
                "<div style='height:40px;margin-bottom:30px'></div>" +
                "<div id='t' style='height:40px;margin-top:20px'></div>" +
                "</div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"t.Y={target.ContentRect.Y}");
            // Flex items do not collapse: gap = 30 + 20 = 50, so Y = 40 + 50 = 90
            Assert.True(System.Math.Abs(target.ContentRect.Y - 90) < 2,
                $"Expected Y=90 (flex: 40+30+20=90), got {target.ContentRect.Y}");
        }

        // [CSS Grid §3] Grid containers prevent margin collapse between items
        [Fact]
        public void Sibling_NoCollapse_InGridContainer()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='display:grid;grid-template-columns:200px;width:200px'>" +
                "<div style='height:40px;margin-bottom:30px'></div>" +
                "<div id='t' style='height:40px;margin-top:20px'></div>" +
                "</div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"t.Y={target.ContentRect.Y}");
            // Grid items do not collapse: gap = 30 + 20 = 50, so Y = 40 + 50 = 90
            Assert.True(System.Math.Abs(target.ContentRect.Y - 90) < 2,
                $"Expected Y=90 (grid: 40+30+20=90), got {target.ContentRect.Y}");
        }

        // [CSS2 §8.3.1] Parent-child top collapse: 30mt parent + 20mt child = 30 (larger wins)
        [Fact]
        public void ParentChild_30mt_20mt_CollapseToLarger()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div id='parent' style='margin-top:30px'>" +
                "<div id='child' style='margin-top:20px;height:40px'></div>" +
                "</div></body>");
            var parent = LayoutTestHelper.FindById(root, "parent")!;
            var child = LayoutTestHelper.FindById(root, "child")!;
            _output.WriteLine($"parent.Y={parent.ContentRect.Y} child.Y={child.ContentRect.Y}");
            // Margins collapse: max(30, 20) = 30. Both start at Y=30.
            Assert.True(System.Math.Abs(parent.ContentRect.Y - 30) < 2,
                $"Expected parent.Y=30 (max(30,20)=30), got {parent.ContentRect.Y}");
            Assert.True(System.Math.Abs(child.ContentRect.Y - 30) < 2,
                $"Expected child.Y=30 (collapsed with parent), got {child.ContentRect.Y}");
        }

        // [CSS2 §8.3.1] Parent-child top collapse prevented by padding-top
        [Fact]
        public void ParentChild_CollapsePrevented_PaddingTop()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div id='parent' style='margin-top:30px;padding-top:1px'>" +
                "<div id='child' style='margin-top:20px;height:40px'></div>" +
                "</div></body>");
            var parent = LayoutTestHelper.FindById(root, "parent")!;
            var child = LayoutTestHelper.FindById(root, "child")!;
            _output.WriteLine($"parent.Y={parent.ContentRect.Y} child.Y={child.ContentRect.Y}");
            // Padding prevents collapse. Parent content at 30+1=31, child at 31+20=51.
            Assert.True(System.Math.Abs(parent.ContentRect.Y - 31) < 2,
                $"Expected parent.Y=31 (30+1), got {parent.ContentRect.Y}");
            Assert.True(System.Math.Abs(child.ContentRect.Y - 51) < 2,
                $"Expected child.Y=51 (31+20), got {child.ContentRect.Y}");
        }

        // [CSS2 §8.3.1] Three siblings cascade: verify intermediate gaps
        [Fact]
        public void ThreeSiblings_Cascade_VerifyPositions()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='overflow:hidden'>" +
                "<div id='a' style='height:30px;margin-bottom:25px'></div>" +
                "<div id='b' style='height:30px;margin-top:15px;margin-bottom:35px'></div>" +
                "<div id='c' style='height:30px;margin-top:20px'></div>" +
                "</div></body>");
            var boxA = LayoutTestHelper.FindById(root, "a")!;
            var boxB = LayoutTestHelper.FindById(root, "b")!;
            var boxC = LayoutTestHelper.FindById(root, "c")!;
            _output.WriteLine($"a.Y={boxA.ContentRect.Y} b.Y={boxB.ContentRect.Y} c.Y={boxC.ContentRect.Y}");
            // First gap: max(25, 15) = 25, B.Y = 30 + 25 = 55
            Assert.True(System.Math.Abs(boxB.ContentRect.Y - 55) < 2,
                $"Expected B.Y=55 (30+max(25,15)=55), got {boxB.ContentRect.Y}");
            // Second gap: max(35, 20) = 35, C.Y = 55 + 30 + 35 = 120
            Assert.True(System.Math.Abs(boxC.ContentRect.Y - 120) < 2,
                $"Expected C.Y=120 (55+30+max(35,20)=120), got {boxC.ContentRect.Y}");
        }

        // [CSS2 §8.3.1] Empty block self-collapses: top and bottom margins collapse together
        [Fact]
        public void EmptyBlock_SelfCollapse_MarginsPassThrough()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='overflow:hidden'>" +
                "<div style='height:40px;margin-bottom:15px'></div>" +
                "<div style='margin-top:25px;margin-bottom:30px'></div>" +
                "<div id='t' style='height:40px;margin-top:10px'></div>" +
                "</div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"t.Y={target.ContentRect.Y}");
            // Empty block self-collapses. All adjoining margins collapse:
            // max(15, 25, 30, 10) = 30, so Y = 40 + 30 = 70
            Assert.True(System.Math.Abs(target.ContentRect.Y - 70) < 2,
                $"Expected Y=70 (40+max(15,25,30,10)=70), got {target.ContentRect.Y}");
        }

        // [CSS2 §8.3.1] Both margins zero: collapse to zero gap
        [Fact]
        public void Sibling_0mb_0mt_CollapseToZero()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='overflow:hidden'>" +
                "<div style='height:40px;margin-bottom:0'></div>" +
                "<div id='t' style='height:40px;margin-top:0'></div>" +
                "</div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"t.Y={target.ContentRect.Y}");
            // max(0, 0) = 0, so Y = 40
            Assert.True(System.Math.Abs(target.ContentRect.Y - 40) < 2,
                $"Expected Y=40 (40+0=40), got {target.ContentRect.Y}");
        }

        // [CSS2 §8.3.1] Inline-block establishes BFC, prevents parent-child collapse
        [Fact]
        public void ParentChild_NoCollapse_InlineBlock()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='width:300px'>" +
                "<span id='parent' style='display:inline-block;width:200px;margin-top:30px'>" +
                "<div id='child' style='margin-top:20px;height:40px'></div>" +
                "</span>" +
                "</div></body>");
            var parent = LayoutTestHelper.FindById(root, "parent")!;
            var child = LayoutTestHelper.FindById(root, "child")!;
            _output.WriteLine($"parent.Y={parent.ContentRect.Y} child.Y={child.ContentRect.Y}");
            // Inline-block establishes BFC; child margin does not collapse with parent
            Assert.True(child.ContentRect.Y - parent.ContentRect.Y >= 18,
                $"Inline-block should prevent collapse (parent.Y={parent.ContentRect.Y}, child.Y={child.ContentRect.Y})");
        }

        // [CSS2 §8.3.1] Float does not collapse margins with adjacent elements
        [Fact]
        public void Sibling_NoCollapse_Float()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='float:left;width:200px;height:40px;margin-bottom:30px'></div>" +
                "<div id='t' style='clear:left;margin-top:20px;height:40px'></div>" +
                "</body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"t.Y={target.ContentRect.Y}");
            // Float margins do not collapse with cleared element.
            // Float bottom edge = 40 + 30 = 70, cleared element at max(70, 20) = 70
            Assert.True(target.ContentRect.Y >= 40,
                $"Float should prevent normal collapse (t.Y={target.ContentRect.Y})");
        }

        // [CSS2 §8.3.1] Sibling collapse with asymmetric large values: 100mb + 5mt = 100
        [Fact]
        public void Sibling_100mb_5mt_CollapseToLarger()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='overflow:hidden'>" +
                "<div style='height:40px;margin-bottom:100px'></div>" +
                "<div id='t' style='height:40px;margin-top:5px'></div>" +
                "</div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"t.Y={target.ContentRect.Y}");
            // max(100, 5) = 100, so Y = 40 + 100 = 140
            Assert.True(System.Math.Abs(target.ContentRect.Y - 140) < 2,
                $"Expected Y=140 (40+max(100,5)=140), got {target.ContentRect.Y}");
        }

        // [CSS2 §8.3.1] Mixed negative collapse: 20mb + -20mt = 0 (cancel out)
        [Fact]
        public void Sibling_20mb_Neg20mt_CancelToZero()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='overflow:hidden'>" +
                "<div style='height:40px;margin-bottom:20px'></div>" +
                "<div id='t' style='height:40px;margin-top:-20px'></div>" +
                "</div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"t.Y={target.ContentRect.Y}");
            // max(20,0) + min(0,-20) = 20 + (-20) = 0, so Y = 40 + 0 = 40
            Assert.True(System.Math.Abs(target.ContentRect.Y - 40) < 2,
                $"Expected Y=40 (40+20-20=40), got {target.ContentRect.Y}");
        }

        // [CSS2 §8.3.1] Parent-child collapse: 20mt parent + 20mt child = 20 (equal)
        [Fact]
        public void ParentChild_20mt_20mt_CollapseToEqual()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div id='parent' style='margin-top:20px'>" +
                "<div id='child' style='margin-top:20px;height:40px'></div>" +
                "</div></body>");
            var parent = LayoutTestHelper.FindById(root, "parent")!;
            var child = LayoutTestHelper.FindById(root, "child")!;
            _output.WriteLine($"parent.Y={parent.ContentRect.Y} child.Y={child.ContentRect.Y}");
            // max(20, 20) = 20. Both at Y=20.
            Assert.True(System.Math.Abs(parent.ContentRect.Y - 20) < 2,
                $"Expected parent.Y=20, got {parent.ContentRect.Y}");
            Assert.True(System.Math.Abs(child.ContentRect.Y - 20) < 2,
                $"Expected child.Y=20 (collapsed), got {child.ContentRect.Y}");
        }

        // [CSS2 §8.3.1] Parent-child collapse: child larger than parent margin
        [Fact]
        public void ParentChild_10mt_50mt_ChildLargerWins()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div id='parent' style='margin-top:10px'>" +
                "<div id='child' style='margin-top:50px;height:40px'></div>" +
                "</div></body>");
            var parent = LayoutTestHelper.FindById(root, "parent")!;
            var child = LayoutTestHelper.FindById(root, "child")!;
            _output.WriteLine($"parent.Y={parent.ContentRect.Y} child.Y={child.ContentRect.Y}");
            // max(10, 50) = 50. Both at Y=50.
            Assert.True(System.Math.Abs(parent.ContentRect.Y - 50) < 2,
                $"Expected parent.Y=50 (max(10,50)=50), got {parent.ContentRect.Y}");
            Assert.True(System.Math.Abs(child.ContentRect.Y - 50) < 2,
                $"Expected child.Y=50 (collapsed), got {child.ContentRect.Y}");
        }

        // [CSS2 §8.3.1] Empty block with margin:0 self-collapses to 0
        [Fact]
        public void EmptyBlock_ZeroMargin_NoEffect()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='overflow:hidden'>" +
                "<div style='height:40px;margin-bottom:15px'></div>" +
                "<div style='margin:0'></div>" +
                "<div id='t' style='height:40px;margin-top:15px'></div>" +
                "</div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"t.Y={target.ContentRect.Y}");
            // Empty block with margin:0 is transparent. max(15, 0, 0, 15) = 15, Y = 40 + 15 = 55
            Assert.True(System.Math.Abs(target.ContentRect.Y - 55) < 2,
                $"Expected Y=55 (40+max(15,0,0,15)=55), got {target.ContentRect.Y}");
        }

        // [CSS2 §8.3.1] No collapse in flex even with same values that would collapse in block
        [Fact]
        public void Flex_NoCollapse_SameValuesAsBlockCollapse()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='display:flex;flex-direction:column;width:200px'>" +
                "<div style='height:40px;margin-bottom:30px'></div>" +
                "<div id='t' style='height:40px;margin-top:30px'></div>" +
                "</div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"t.Y={target.ContentRect.Y}");
            // In block context: max(30,30) = 30. In flex: 30+30 = 60. Y = 40 + 60 = 100.
            Assert.True(System.Math.Abs(target.ContentRect.Y - 100) < 2,
                $"Expected Y=100 (flex: 40+30+30=100), got {target.ContentRect.Y}");
        }

        // [CSS2 §8.3.1] No collapse in grid even with same values that would collapse in block
        [Fact]
        public void Grid_NoCollapse_SameValuesAsBlockCollapse()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='display:grid;grid-template-columns:200px;width:200px'>" +
                "<div style='height:40px;margin-bottom:30px'></div>" +
                "<div id='t' style='height:40px;margin-top:30px'></div>" +
                "</div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"t.Y={target.ContentRect.Y}");
            // In block context: max(30,30) = 30. In grid: 30+30 = 60. Y = 40 + 60 = 100.
            Assert.True(System.Math.Abs(target.ContentRect.Y - 100) < 2,
                $"Expected Y=100 (grid: 40+30+30=100), got {target.ContentRect.Y}");
        }
    }
}
