using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class WptBlockStackingOrderTests
    {
        private readonly ITestOutputHelper _output;

        public WptBlockStackingOrderTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void TwoBlocks_H30_H40_StackAtCorrectY()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='width:200px'>
                    <div id='a' style='height:30px'></div>
                    <div id='b' style='height:40px'></div>
                </div></body>");
            var blockA = LayoutTestHelper.FindById(root, "a")!;
            var blockB = LayoutTestHelper.FindById(root, "b")!;
            _output.WriteLine($"a.Y={blockA.ContentRect.Y} b.Y={blockB.ContentRect.Y}");
            Assert.True(System.Math.Abs(blockA.ContentRect.Y - 0) < 1, $"First block at Y=0 (got {blockA.ContentRect.Y})");
            Assert.True(System.Math.Abs(blockB.ContentRect.Y - 30) < 1, $"Second block at Y=30 (got {blockB.ContentRect.Y})");
        }

        [Fact]
        public void ThreeBlocks_H20_H30_H40_StackAtCorrectY()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='width:200px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:30px'></div>
                    <div id='c' style='height:40px'></div>
                </div></body>");
            var blockA = LayoutTestHelper.FindById(root, "a")!;
            var blockB = LayoutTestHelper.FindById(root, "b")!;
            var blockC = LayoutTestHelper.FindById(root, "c")!;
            _output.WriteLine($"a.Y={blockA.ContentRect.Y} b.Y={blockB.ContentRect.Y} c.Y={blockC.ContentRect.Y}");
            Assert.True(System.Math.Abs(blockA.ContentRect.Y - 0) < 1, $"First block at Y=0 (got {blockA.ContentRect.Y})");
            Assert.True(System.Math.Abs(blockB.ContentRect.Y - 20) < 1, $"Second block at Y=20 (got {blockB.ContentRect.Y})");
            Assert.True(System.Math.Abs(blockC.ContentRect.Y - 50) < 1, $"Third block at Y=50 (got {blockC.ContentRect.Y})");
        }

        [Fact]
        public void FourEqualBlocks_H25_StackAtCorrectY()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='width:200px'>
                    <div id='a' style='height:25px'></div>
                    <div id='b' style='height:25px'></div>
                    <div id='c' style='height:25px'></div>
                    <div id='d' style='height:25px'></div>
                </div></body>");
            var blockA = LayoutTestHelper.FindById(root, "a")!;
            var blockB = LayoutTestHelper.FindById(root, "b")!;
            var blockC = LayoutTestHelper.FindById(root, "c")!;
            var blockD = LayoutTestHelper.FindById(root, "d")!;
            _output.WriteLine($"a.Y={blockA.ContentRect.Y} b.Y={blockB.ContentRect.Y} c.Y={blockC.ContentRect.Y} d.Y={blockD.ContentRect.Y}");
            Assert.True(System.Math.Abs(blockA.ContentRect.Y - 0) < 1, $"Block A at Y=0 (got {blockA.ContentRect.Y})");
            Assert.True(System.Math.Abs(blockB.ContentRect.Y - 25) < 1, $"Block B at Y=25 (got {blockB.ContentRect.Y})");
            Assert.True(System.Math.Abs(blockC.ContentRect.Y - 50) < 1, $"Block C at Y=50 (got {blockC.ContentRect.Y})");
            Assert.True(System.Math.Abs(blockD.ContentRect.Y - 75) < 1, $"Block D at Y=75 (got {blockD.ContentRect.Y})");
        }

        [Fact]
        public void FiveEqualBlocks_H20_StackAtCorrectY()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='width:200px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                    <div id='c' style='height:20px'></div>
                    <div id='d' style='height:20px'></div>
                    <div id='e' style='height:20px'></div>
                </div></body>");
            var blockA = LayoutTestHelper.FindById(root, "a")!;
            var blockB = LayoutTestHelper.FindById(root, "b")!;
            var blockC = LayoutTestHelper.FindById(root, "c")!;
            var blockD = LayoutTestHelper.FindById(root, "d")!;
            var blockE = LayoutTestHelper.FindById(root, "e")!;
            _output.WriteLine($"a.Y={blockA.ContentRect.Y} b.Y={blockB.ContentRect.Y} c.Y={blockC.ContentRect.Y} d.Y={blockD.ContentRect.Y} e.Y={blockE.ContentRect.Y}");
            Assert.True(System.Math.Abs(blockA.ContentRect.Y - 0) < 1, $"Block A at Y=0 (got {blockA.ContentRect.Y})");
            Assert.True(System.Math.Abs(blockB.ContentRect.Y - 20) < 1, $"Block B at Y=20 (got {blockB.ContentRect.Y})");
            Assert.True(System.Math.Abs(blockC.ContentRect.Y - 40) < 1, $"Block C at Y=40 (got {blockC.ContentRect.Y})");
            Assert.True(System.Math.Abs(blockD.ContentRect.Y - 60) < 1, $"Block D at Y=60 (got {blockD.ContentRect.Y})");
            Assert.True(System.Math.Abs(blockE.ContentRect.Y - 80) < 1, $"Block E at Y=80 (got {blockE.ContentRect.Y})");
        }

        [Fact]
        public void SixEqualBlocks_H15_StackAtCorrectY()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='width:200px'>
                    <div id='a' style='height:15px'></div>
                    <div id='b' style='height:15px'></div>
                    <div id='c' style='height:15px'></div>
                    <div id='d' style='height:15px'></div>
                    <div id='e' style='height:15px'></div>
                    <div id='f' style='height:15px'></div>
                </div></body>");
            var blockA = LayoutTestHelper.FindById(root, "a")!;
            var blockB = LayoutTestHelper.FindById(root, "b")!;
            var blockC = LayoutTestHelper.FindById(root, "c")!;
            var blockD = LayoutTestHelper.FindById(root, "d")!;
            var blockE = LayoutTestHelper.FindById(root, "e")!;
            var blockF = LayoutTestHelper.FindById(root, "f")!;
            _output.WriteLine($"a.Y={blockA.ContentRect.Y} b.Y={blockB.ContentRect.Y} c.Y={blockC.ContentRect.Y} d.Y={blockD.ContentRect.Y} e.Y={blockE.ContentRect.Y} f.Y={blockF.ContentRect.Y}");
            Assert.True(System.Math.Abs(blockA.ContentRect.Y - 0) < 1, $"Block A at Y=0 (got {blockA.ContentRect.Y})");
            Assert.True(System.Math.Abs(blockB.ContentRect.Y - 15) < 1, $"Block B at Y=15 (got {blockB.ContentRect.Y})");
            Assert.True(System.Math.Abs(blockC.ContentRect.Y - 30) < 1, $"Block C at Y=30 (got {blockC.ContentRect.Y})");
            Assert.True(System.Math.Abs(blockD.ContentRect.Y - 45) < 1, $"Block D at Y=45 (got {blockD.ContentRect.Y})");
            Assert.True(System.Math.Abs(blockE.ContentRect.Y - 60) < 1, $"Block E at Y=60 (got {blockE.ContentRect.Y})");
            Assert.True(System.Math.Abs(blockF.ContentRect.Y - 75) < 1, $"Block F at Y=75 (got {blockF.ContentRect.Y})");
        }

        [Fact]
        public void TwoBlocks_WithMarginTop_StackAtCorrectY()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='width:200px;overflow:hidden'>
                    <div id='a' style='height:30px;margin-bottom:10px'></div>
                    <div id='b' style='height:40px;margin-top:20px'></div>
                </div></body>");
            var blockA = LayoutTestHelper.FindById(root, "a")!;
            var blockB = LayoutTestHelper.FindById(root, "b")!;
            _output.WriteLine($"a.Y={blockA.ContentRect.Y} b.Y={blockB.ContentRect.Y}");
            Assert.True(System.Math.Abs(blockA.ContentRect.Y - 0) < 1, $"Block A at Y=0 (got {blockA.ContentRect.Y})");
            Assert.True(System.Math.Abs(blockB.ContentRect.Y - 50) < 1, $"Block B at Y=50 after collapsed margin max(10,20)=20 (got {blockB.ContentRect.Y})");
        }

        [Fact]
        public void ThreeBlocks_WithPadding_StackAtCorrectY()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='width:200px'>
                    <div id='a' style='height:20px;padding:10px'></div>
                    <div id='b' style='height:20px;padding:5px'></div>
                    <div id='c' style='height:20px;padding:15px'></div>
                </div></body>");
            var blockA = LayoutTestHelper.FindById(root, "a")!;
            var blockB = LayoutTestHelper.FindById(root, "b")!;
            var blockC = LayoutTestHelper.FindById(root, "c")!;
            _output.WriteLine($"a.Y={blockA.ContentRect.Y} b.Y={blockB.ContentRect.Y} c.Y={blockC.ContentRect.Y}");
            Assert.True(System.Math.Abs(blockA.ContentRect.Y - 10) < 1, $"Block A content at Y=10 (padTop=10) (got {blockA.ContentRect.Y})");
            Assert.True(System.Math.Abs(blockB.ContentRect.Y - 45) < 1, $"Block B content at Y=45 (10+20+10+5) (got {blockB.ContentRect.Y})");
            Assert.True(System.Math.Abs(blockC.ContentRect.Y - 85) < 1, $"Block C content at Y=85 (40+5+20+5+15) (got {blockC.ContentRect.Y})");
        }

        [Fact]
        public void TwoBlocks_WithBorder_StackAtCorrectY()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='width:200px'>
                    <div id='a' style='height:30px;border:5px solid black'></div>
                    <div id='b' style='height:40px;border:3px solid black'></div>
                </div></body>");
            var blockA = LayoutTestHelper.FindById(root, "a")!;
            var blockB = LayoutTestHelper.FindById(root, "b")!;
            _output.WriteLine($"a.Y={blockA.ContentRect.Y} b.Y={blockB.ContentRect.Y}");
            Assert.True(System.Math.Abs(blockA.ContentRect.Y - 5) < 1, $"Block A content at Y=5 (borderTop=5) (got {blockA.ContentRect.Y})");
            Assert.True(System.Math.Abs(blockB.ContentRect.Y - 43) < 1, $"Block B content at Y=43 (5+30+5+3) (got {blockB.ContentRect.Y})");
        }

        [Fact]
        public void DisplayNone_SkippedInStacking()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='width:200px'>
                    <div id='a' style='height:30px'></div>
                    <div style='display:none;height:100px'></div>
                    <div id='b' style='height:40px'></div>
                </div></body>");
            var blockA = LayoutTestHelper.FindById(root, "a")!;
            var blockB = LayoutTestHelper.FindById(root, "b")!;
            _output.WriteLine($"a.Y={blockA.ContentRect.Y} b.Y={blockB.ContentRect.Y}");
            Assert.True(System.Math.Abs(blockA.ContentRect.Y - 0) < 1, $"Block A at Y=0 (got {blockA.ContentRect.Y})");
            Assert.True(System.Math.Abs(blockB.ContentRect.Y - 30) < 1, $"Block B at Y=30, display:none skipped (got {blockB.ContentRect.Y})");
        }

        [Fact]
        public void VisibilityHidden_TakesSpaceInStacking()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='width:200px'>
                    <div id='a' style='height:30px'></div>
                    <div id='hidden' style='visibility:hidden;height:50px'></div>
                    <div id='b' style='height:40px'></div>
                </div></body>");
            var blockA = LayoutTestHelper.FindById(root, "a")!;
            var hiddenBlock = LayoutTestHelper.FindById(root, "hidden")!;
            var blockB = LayoutTestHelper.FindById(root, "b")!;
            _output.WriteLine($"a.Y={blockA.ContentRect.Y} hidden.Y={hiddenBlock.ContentRect.Y} b.Y={blockB.ContentRect.Y}");
            Assert.True(System.Math.Abs(blockA.ContentRect.Y - 0) < 1, $"Block A at Y=0 (got {blockA.ContentRect.Y})");
            Assert.True(System.Math.Abs(hiddenBlock.ContentRect.Y - 30) < 1, $"Hidden block at Y=30 (got {hiddenBlock.ContentRect.Y})");
            Assert.True(System.Math.Abs(blockB.ContentRect.Y - 80) < 1, $"Block B at Y=80, hidden takes space (got {blockB.ContentRect.Y})");
        }

        [Fact]
        public void AbsolutelyPositioned_SkippedInNormalFlow()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='width:200px;position:relative'>
                    <div id='a' style='height:30px'></div>
                    <div style='position:absolute;height:100px;width:50px'></div>
                    <div id='b' style='height:40px'></div>
                </div></body>");
            var blockA = LayoutTestHelper.FindById(root, "a")!;
            var blockB = LayoutTestHelper.FindById(root, "b")!;
            _output.WriteLine($"a.Y={blockA.ContentRect.Y} b.Y={blockB.ContentRect.Y}");
            Assert.True(System.Math.Abs(blockA.ContentRect.Y - 0) < 1, $"Block A at Y=0 (got {blockA.ContentRect.Y})");
            Assert.True(System.Math.Abs(blockB.ContentRect.Y - 30) < 1, $"Block B at Y=30, abspos skipped (got {blockB.ContentRect.Y})");
        }

        [Fact]
        public void NegativeMarginTop_CausesOverlap()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='width:200px;overflow:hidden'>
                    <div id='a' style='height:40px'></div>
                    <div id='b' style='height:30px;margin-top:-15px'></div>
                </div></body>");
            var blockA = LayoutTestHelper.FindById(root, "a")!;
            var blockB = LayoutTestHelper.FindById(root, "b")!;
            _output.WriteLine($"a.Y={blockA.ContentRect.Y} b.Y={blockB.ContentRect.Y}");
            Assert.True(System.Math.Abs(blockA.ContentRect.Y - 0) < 1, $"Block A at Y=0 (got {blockA.ContentRect.Y})");
            Assert.True(System.Math.Abs(blockB.ContentRect.Y - 25) < 1, $"Block B at Y=25 (40-15) overlap (got {blockB.ContentRect.Y})");
        }

        [Fact]
        public void MarginCollapse_TwoSiblings_LargerWins()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='width:200px;overflow:hidden'>
                    <div id='a' style='height:30px;margin-bottom:20px'></div>
                    <div id='b' style='height:40px;margin-top:30px'></div>
                </div></body>");
            var blockA = LayoutTestHelper.FindById(root, "a")!;
            var blockB = LayoutTestHelper.FindById(root, "b")!;
            _output.WriteLine($"a.Y={blockA.ContentRect.Y} b.Y={blockB.ContentRect.Y}");
            Assert.True(System.Math.Abs(blockA.ContentRect.Y - 0) < 1, $"Block A at Y=0 (got {blockA.ContentRect.Y})");
            Assert.True(System.Math.Abs(blockB.ContentRect.Y - 60) < 1, $"Block B at Y=60 (30+max(20,30)) (got {blockB.ContentRect.Y})");
        }

        [Fact]
        public void AfterFloatClear_StacksBelowFloat()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='width:200px'>
                    <div style='float:left;width:80px;height:60px'></div>
                    <div id='cleared' style='clear:both;height:30px'></div>
                    <div id='after' style='height:25px'></div>
                </div></body>");
            var cleared = LayoutTestHelper.FindById(root, "cleared")!;
            var after = LayoutTestHelper.FindById(root, "after")!;
            _output.WriteLine($"cleared.Y={cleared.ContentRect.Y} after.Y={after.ContentRect.Y}");
            Assert.True(cleared.ContentRect.Y >= 59, $"Cleared block below float at Y>=60 (got {cleared.ContentRect.Y})");
            Assert.True(System.Math.Abs(after.ContentRect.Y - (cleared.ContentRect.Y + 30)) < 1, $"After block stacks below cleared (got {after.ContentRect.Y})");
        }

        [Fact]
        public void NestedContainer_InnerBlocksStackCorrectly()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='width:300px'>
                    <div id='outer' style='height:20px'></div>
                    <div style='width:200px'>
                        <div id='inner1' style='height:25px'></div>
                        <div id='inner2' style='height:35px'></div>
                    </div>
                </div></body>");
            var outer = LayoutTestHelper.FindById(root, "outer")!;
            var inner1 = LayoutTestHelper.FindById(root, "inner1")!;
            var inner2 = LayoutTestHelper.FindById(root, "inner2")!;
            _output.WriteLine($"outer.Y={outer.ContentRect.Y} inner1.Y={inner1.ContentRect.Y} inner2.Y={inner2.ContentRect.Y}");
            Assert.True(System.Math.Abs(outer.ContentRect.Y - 0) < 1, $"Outer at Y=0 (got {outer.ContentRect.Y})");
            Assert.True(System.Math.Abs(inner1.ContentRect.Y - 20) < 1, $"Inner1 at Y=20 (got {inner1.ContentRect.Y})");
            Assert.True(System.Math.Abs(inner2.ContentRect.Y - 45) < 1, $"Inner2 at Y=45 (got {inner2.ContentRect.Y})");
        }

        [Fact]
        public void DifferentWidths_DoNotAffectY()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='width:400px'>
                    <div id='a' style='height:30px;width:100px'></div>
                    <div id='b' style='height:30px;width:200px'></div>
                    <div id='c' style='height:30px;width:50px'></div>
                </div></body>");
            var blockA = LayoutTestHelper.FindById(root, "a")!;
            var blockB = LayoutTestHelper.FindById(root, "b")!;
            var blockC = LayoutTestHelper.FindById(root, "c")!;
            _output.WriteLine($"a.Y={blockA.ContentRect.Y} b.Y={blockB.ContentRect.Y} c.Y={blockC.ContentRect.Y}");
            Assert.True(System.Math.Abs(blockA.ContentRect.Y - 0) < 1, $"Block A at Y=0 (got {blockA.ContentRect.Y})");
            Assert.True(System.Math.Abs(blockB.ContentRect.Y - 30) < 1, $"Block B at Y=30 regardless of width (got {blockB.ContentRect.Y})");
            Assert.True(System.Math.Abs(blockC.ContentRect.Y - 60) < 1, $"Block C at Y=60 regardless of width (got {blockC.ContentRect.Y})");
        }

        [Fact]
        public void AllBlocks_X_IsZero()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='width:300px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                    <div id='c' style='height:20px'></div>
                </div></body>");
            var blockA = LayoutTestHelper.FindById(root, "a")!;
            var blockB = LayoutTestHelper.FindById(root, "b")!;
            var blockC = LayoutTestHelper.FindById(root, "c")!;
            _output.WriteLine($"a.X={blockA.ContentRect.X} b.X={blockB.ContentRect.X} c.X={blockC.ContentRect.X}");
            Assert.True(System.Math.Abs(blockA.ContentRect.X - 0) < 1, $"Block A at X=0 (got {blockA.ContentRect.X})");
            Assert.True(System.Math.Abs(blockB.ContentRect.X - 0) < 1, $"Block B at X=0 (got {blockB.ContentRect.X})");
            Assert.True(System.Math.Abs(blockC.ContentRect.X - 0) < 1, $"Block C at X=0 (got {blockC.ContentRect.X})");
        }

        [Fact]
        public void AutoHeight_EqualsSumOfChildHeights()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='parent' style='width:200px'>
                    <div style='height:30px'></div>
                    <div style='height:40px'></div>
                    <div style='height:50px'></div>
                </div></body>");
            var parent = LayoutTestHelper.FindById(root, "parent")!;
            _output.WriteLine($"parent.Height={parent.ContentRect.Height}");
            Assert.True(System.Math.Abs(parent.ContentRect.Height - 120) < 1, $"Auto height = 30+40+50 = 120 (got {parent.ContentRect.Height})");
        }

        [Fact]
        public void FlexColumnChildren_StackAtCorrectY()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display:flex;flex-direction:column;width:200px'>
                    <div id='a' style='height:30px'></div>
                    <div id='b' style='height:40px'></div>
                    <div id='c' style='height:50px'></div>
                </div></body>");
            var blockA = LayoutTestHelper.FindById(root, "a")!;
            var blockB = LayoutTestHelper.FindById(root, "b")!;
            var blockC = LayoutTestHelper.FindById(root, "c")!;
            _output.WriteLine($"a.Y={blockA.ContentRect.Y} b.Y={blockB.ContentRect.Y} c.Y={blockC.ContentRect.Y}");
            Assert.True(System.Math.Abs(blockA.ContentRect.Y - 0) < 1, $"Flex child A at Y=0 (got {blockA.ContentRect.Y})");
            Assert.True(System.Math.Abs(blockB.ContentRect.Y - 30) < 1, $"Flex child B at Y=30 (got {blockB.ContentRect.Y})");
            Assert.True(System.Math.Abs(blockC.ContentRect.Y - 70) < 1, $"Flex child C at Y=70 (got {blockC.ContentRect.Y})");
        }

        [Fact]
        public void GridSingleColumn_ChildrenStackAtCorrectY()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display:grid;grid-template-columns:200px;width:200px'>
                    <div id='a' style='height:30px'></div>
                    <div id='b' style='height:40px'></div>
                    <div id='c' style='height:50px'></div>
                </div></body>");
            var blockA = LayoutTestHelper.FindById(root, "a")!;
            var blockB = LayoutTestHelper.FindById(root, "b")!;
            var blockC = LayoutTestHelper.FindById(root, "c")!;
            _output.WriteLine($"a.Y={blockA.ContentRect.Y} b.Y={blockB.ContentRect.Y} c.Y={blockC.ContentRect.Y}");
            Assert.True(System.Math.Abs(blockA.ContentRect.Y - 0) < 1, $"Grid child A at Y=0 (got {blockA.ContentRect.Y})");
            Assert.True(System.Math.Abs(blockB.ContentRect.Y - 30) < 1, $"Grid child B at Y=30 (got {blockB.ContentRect.Y})");
            Assert.True(System.Math.Abs(blockC.ContentRect.Y - 70) < 1, $"Grid child C at Y=70 (got {blockC.ContentRect.Y})");
        }

        [Fact]
        public void MultipleDisplayNone_AllSkipped()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='width:200px'>
                    <div id='a' style='height:30px'></div>
                    <div style='display:none;height:50px'></div>
                    <div style='display:none;height:60px'></div>
                    <div id='b' style='height:40px'></div>
                </div></body>");
            var blockA = LayoutTestHelper.FindById(root, "a")!;
            var blockB = LayoutTestHelper.FindById(root, "b")!;
            _output.WriteLine($"a.Y={blockA.ContentRect.Y} b.Y={blockB.ContentRect.Y}");
            Assert.True(System.Math.Abs(blockA.ContentRect.Y - 0) < 1, $"Block A at Y=0 (got {blockA.ContentRect.Y})");
            Assert.True(System.Math.Abs(blockB.ContentRect.Y - 30) < 1, $"Block B at Y=30 after two display:none (got {blockB.ContentRect.Y})");
        }

        [Fact]
        public void MixedHeights_VaryingBlocks_StackCorrectly()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='width:200px'>
                    <div id='a' style='height:10px'></div>
                    <div id='b' style='height:50px'></div>
                    <div id='c' style='height:5px'></div>
                    <div id='d' style='height:100px'></div>
                </div></body>");
            var blockA = LayoutTestHelper.FindById(root, "a")!;
            var blockB = LayoutTestHelper.FindById(root, "b")!;
            var blockC = LayoutTestHelper.FindById(root, "c")!;
            var blockD = LayoutTestHelper.FindById(root, "d")!;
            _output.WriteLine($"a.Y={blockA.ContentRect.Y} b.Y={blockB.ContentRect.Y} c.Y={blockC.ContentRect.Y} d.Y={blockD.ContentRect.Y}");
            Assert.True(System.Math.Abs(blockA.ContentRect.Y - 0) < 1, $"Block A at Y=0 (got {blockA.ContentRect.Y})");
            Assert.True(System.Math.Abs(blockB.ContentRect.Y - 10) < 1, $"Block B at Y=10 (got {blockB.ContentRect.Y})");
            Assert.True(System.Math.Abs(blockC.ContentRect.Y - 60) < 1, $"Block C at Y=60 (got {blockC.ContentRect.Y})");
            Assert.True(System.Math.Abs(blockD.ContentRect.Y - 65) < 1, $"Block D at Y=65 (got {blockD.ContentRect.Y})");
        }

        [Fact]
        public void NegativeMarginBottom_PullsNextBlockUp()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='width:200px;overflow:hidden'>
                    <div id='a' style='height:40px;margin-bottom:-10px'></div>
                    <div id='b' style='height:30px'></div>
                </div></body>");
            var blockA = LayoutTestHelper.FindById(root, "a")!;
            var blockB = LayoutTestHelper.FindById(root, "b")!;
            _output.WriteLine($"a.Y={blockA.ContentRect.Y} b.Y={blockB.ContentRect.Y}");
            Assert.True(System.Math.Abs(blockA.ContentRect.Y - 0) < 1, $"Block A at Y=0 (got {blockA.ContentRect.Y})");
            Assert.True(System.Math.Abs(blockB.ContentRect.Y - 30) < 1, $"Block B at Y=30 (40-10) (got {blockB.ContentRect.Y})");
        }

        [Fact]
        public void PaddingAndBorder_CombinedAffectY()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='width:200px'>
                    <div id='a' style='height:20px;padding:8px;border:2px solid black'></div>
                    <div id='b' style='height:30px'></div>
                </div></body>");
            var blockA = LayoutTestHelper.FindById(root, "a")!;
            var blockB = LayoutTestHelper.FindById(root, "b")!;
            _output.WriteLine($"a.Y={blockA.ContentRect.Y} b.Y={blockB.ContentRect.Y}");
            Assert.True(System.Math.Abs(blockA.ContentRect.Y - 10) < 1, $"Block A content at Y=10 (pad8+border2) (got {blockA.ContentRect.Y})");
            Assert.True(System.Math.Abs(blockB.ContentRect.Y - 40) < 1, $"Block B at Y=40 (2+8+20+8+2) (got {blockB.ContentRect.Y})");
        }

        [Fact]
        public void ZeroHeightBlock_DoesNotAffectStacking()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='width:200px'>
                    <div id='a' style='height:30px'></div>
                    <div style='height:0'></div>
                    <div id='b' style='height:40px'></div>
                </div></body>");
            var blockA = LayoutTestHelper.FindById(root, "a")!;
            var blockB = LayoutTestHelper.FindById(root, "b")!;
            _output.WriteLine($"a.Y={blockA.ContentRect.Y} b.Y={blockB.ContentRect.Y}");
            Assert.True(System.Math.Abs(blockA.ContentRect.Y - 0) < 1, $"Block A at Y=0 (got {blockA.ContentRect.Y})");
            Assert.True(System.Math.Abs(blockB.ContentRect.Y - 30) < 1, $"Block B at Y=30 after zero-height (got {blockB.ContentRect.Y})");
        }

        [Fact]
        public void AutoHeight_WithMarginCollapse_CorrectSum()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='parent' style='width:200px;overflow:hidden'>
                    <div style='height:30px;margin-bottom:20px'></div>
                    <div style='height:40px;margin-top:10px'></div>
                </div></body>");
            var parent = LayoutTestHelper.FindById(root, "parent")!;
            _output.WriteLine($"parent.Height={parent.ContentRect.Height}");
            Assert.True(System.Math.Abs(parent.ContentRect.Height - 90) < 1, $"Auto height = 30+max(20,10)+40 = 90 (got {parent.ContentRect.Height})");
        }

        [Fact]
        public void MarginCollapse_ThreeAdjacentSiblings()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='width:200px;overflow:hidden'>
                    <div id='a' style='height:20px;margin-bottom:15px'></div>
                    <div id='b' style='height:25px;margin-top:10px;margin-bottom:25px'></div>
                    <div id='c' style='height:30px;margin-top:20px'></div>
                </div></body>");
            var blockA = LayoutTestHelper.FindById(root, "a")!;
            var blockB = LayoutTestHelper.FindById(root, "b")!;
            var blockC = LayoutTestHelper.FindById(root, "c")!;
            _output.WriteLine($"a.Y={blockA.ContentRect.Y} b.Y={blockB.ContentRect.Y} c.Y={blockC.ContentRect.Y}");
            Assert.True(System.Math.Abs(blockA.ContentRect.Y - 0) < 1, $"Block A at Y=0 (got {blockA.ContentRect.Y})");
            Assert.True(System.Math.Abs(blockB.ContentRect.Y - 35) < 1, $"Block B at Y=35 (20+max(15,10)=35) (got {blockB.ContentRect.Y})");
            Assert.True(System.Math.Abs(blockC.ContentRect.Y - 85) < 1, $"Block C at Y=85 (35+25+max(25,20)=85) (got {blockC.ContentRect.Y})");
        }

        [Fact]
        public void FixedPosition_SkippedInNormalFlow()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='width:200px'>
                    <div id='a' style='height:30px'></div>
                    <div style='position:fixed;height:100px;width:50px'></div>
                    <div id='b' style='height:40px'></div>
                </div></body>");
            var blockA = LayoutTestHelper.FindById(root, "a")!;
            var blockB = LayoutTestHelper.FindById(root, "b")!;
            _output.WriteLine($"a.Y={blockA.ContentRect.Y} b.Y={blockB.ContentRect.Y}");
            Assert.True(System.Math.Abs(blockA.ContentRect.Y - 0) < 1, $"Block A at Y=0 (got {blockA.ContentRect.Y})");
            Assert.True(System.Math.Abs(blockB.ContentRect.Y - 30) < 1, $"Block B at Y=30, fixed skipped (got {blockB.ContentRect.Y})");
        }

        [Fact]
        public void FloatedElement_SkippedInNormalFlow()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='width:200px'>
                    <div id='a' style='height:30px'></div>
                    <div style='float:left;width:50px;height:100px'></div>
                    <div id='b' style='height:40px'></div>
                </div></body>");
            var blockA = LayoutTestHelper.FindById(root, "a")!;
            var blockB = LayoutTestHelper.FindById(root, "b")!;
            _output.WriteLine($"a.Y={blockA.ContentRect.Y} b.Y={blockB.ContentRect.Y}");
            Assert.True(System.Math.Abs(blockA.ContentRect.Y - 0) < 1, $"Block A at Y=0 (got {blockA.ContentRect.Y})");
            Assert.True(System.Math.Abs(blockB.ContentRect.Y - 30) < 1, $"Block B at Y=30, float out of flow (got {blockB.ContentRect.Y})");
        }

        [Fact]
        public void NestedBlocks_DeepNesting_StackCorrectly()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='width:300px'>
                    <div id='a' style='height:20px'></div>
                    <div>
                        <div>
                            <div id='deep' style='height:30px'></div>
                        </div>
                    </div>
                    <div id='b' style='height:25px'></div>
                </div></body>");
            var blockA = LayoutTestHelper.FindById(root, "a")!;
            var deep = LayoutTestHelper.FindById(root, "deep")!;
            var blockB = LayoutTestHelper.FindById(root, "b")!;
            _output.WriteLine($"a.Y={blockA.ContentRect.Y} deep.Y={deep.ContentRect.Y} b.Y={blockB.ContentRect.Y}");
            Assert.True(System.Math.Abs(blockA.ContentRect.Y - 0) < 1, $"Block A at Y=0 (got {blockA.ContentRect.Y})");
            Assert.True(System.Math.Abs(deep.ContentRect.Y - 20) < 1, $"Deep block at Y=20 (got {deep.ContentRect.Y})");
            Assert.True(System.Math.Abs(blockB.ContentRect.Y - 50) < 1, $"Block B at Y=50 (got {blockB.ContentRect.Y})");
        }

        [Fact]
        public void AutoWidth_BlocksFillContainer()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='width:250px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:30px'></div>
                </div></body>");
            var blockA = LayoutTestHelper.FindById(root, "a")!;
            var blockB = LayoutTestHelper.FindById(root, "b")!;
            _output.WriteLine($"a.Width={blockA.ContentRect.Width} b.Width={blockB.ContentRect.Width}");
            Assert.True(System.Math.Abs(blockA.ContentRect.Width - 250) < 1, $"Block A fills container width 250 (got {blockA.ContentRect.Width})");
            Assert.True(System.Math.Abs(blockB.ContentRect.Width - 250) < 1, $"Block B fills container width 250 (got {blockB.ContentRect.Width})");
        }

        [Fact]
        public void MarginTopOnFirstChild_WithOverflowHidden()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='parent' style='width:200px;overflow:hidden'>
                    <div id='a' style='height:30px;margin-top:15px'></div>
                    <div id='b' style='height:40px'></div>
                </div></body>");
            var parent = LayoutTestHelper.FindById(root, "parent")!;
            var blockA = LayoutTestHelper.FindById(root, "a")!;
            var blockB = LayoutTestHelper.FindById(root, "b")!;
            _output.WriteLine($"parent.Y={parent.ContentRect.Y} a.Y={blockA.ContentRect.Y} b.Y={blockB.ContentRect.Y}");
            Assert.True(System.Math.Abs(blockA.ContentRect.Y - 15) < 1, $"Block A at Y=15 (margin-top inside BFC) (got {blockA.ContentRect.Y})");
            Assert.True(System.Math.Abs(blockB.ContentRect.Y - 45) < 1, $"Block B at Y=45 (15+30) (got {blockB.ContentRect.Y})");
        }

        [Fact]
        public void LargeNumberOfBlocks_TenBlocks_StackCorrectly()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='width:200px'>
                    <div id='b0' style='height:10px'></div>
                    <div id='b1' style='height:10px'></div>
                    <div id='b2' style='height:10px'></div>
                    <div id='b3' style='height:10px'></div>
                    <div id='b4' style='height:10px'></div>
                    <div id='b5' style='height:10px'></div>
                    <div id='b6' style='height:10px'></div>
                    <div id='b7' style='height:10px'></div>
                    <div id='b8' style='height:10px'></div>
                    <div id='b9' style='height:10px'></div>
                </div></body>");
            for (int index = 0; index < 10; index++)
            {
                var block = LayoutTestHelper.FindById(root, $"b{index}")!;
                float expectedY = index * 10;
                _output.WriteLine($"b{index}.Y={block.ContentRect.Y} expected={expectedY}");
                Assert.True(System.Math.Abs(block.ContentRect.Y - expectedY) < 1, $"Block b{index} at Y={expectedY} (got {block.ContentRect.Y})");
            }
        }

        [Fact]
        public void AutoHeight_WithDisplayNone_ExcludesHiddenChild()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='parent' style='width:200px'>
                    <div style='height:30px'></div>
                    <div style='display:none;height:100px'></div>
                    <div style='height:40px'></div>
                </div></body>");
            var parent = LayoutTestHelper.FindById(root, "parent")!;
            _output.WriteLine($"parent.Height={parent.ContentRect.Height}");
            Assert.True(System.Math.Abs(parent.ContentRect.Height - 70) < 1, $"Auto height = 30+40 = 70, display:none excluded (got {parent.ContentRect.Height})");
        }

        [Fact]
        public void FlexColumnWithGap_ChildrenStackWithGap()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display:flex;flex-direction:column;gap:10px;width:200px'>
                    <div id='a' style='height:30px'></div>
                    <div id='b' style='height:40px'></div>
                    <div id='c' style='height:20px'></div>
                </div></body>");
            var blockA = LayoutTestHelper.FindById(root, "a")!;
            var blockB = LayoutTestHelper.FindById(root, "b")!;
            var blockC = LayoutTestHelper.FindById(root, "c")!;
            _output.WriteLine($"a.Y={blockA.ContentRect.Y} b.Y={blockB.ContentRect.Y} c.Y={blockC.ContentRect.Y}");
            Assert.True(System.Math.Abs(blockA.ContentRect.Y - 0) < 1, $"Flex A at Y=0 (got {blockA.ContentRect.Y})");
            Assert.True(System.Math.Abs(blockB.ContentRect.Y - 40) < 1, $"Flex B at Y=40 (30+10gap) (got {blockB.ContentRect.Y})");
            Assert.True(System.Math.Abs(blockC.ContentRect.Y - 90) < 1, $"Flex C at Y=90 (40+40+10gap) (got {blockC.ContentRect.Y})");
        }

        [Fact]
        public void GridSingleColumnWithGap_ChildrenStackWithGap()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display:grid;grid-template-columns:200px;row-gap:15px;width:200px'>
                    <div id='a' style='height:25px'></div>
                    <div id='b' style='height:35px'></div>
                    <div id='c' style='height:20px'></div>
                </div></body>");
            var blockA = LayoutTestHelper.FindById(root, "a")!;
            var blockB = LayoutTestHelper.FindById(root, "b")!;
            var blockC = LayoutTestHelper.FindById(root, "c")!;
            _output.WriteLine($"a.Y={blockA.ContentRect.Y} b.Y={blockB.ContentRect.Y} c.Y={blockC.ContentRect.Y}");
            Assert.True(System.Math.Abs(blockA.ContentRect.Y - 0) < 1, $"Grid A at Y=0 (got {blockA.ContentRect.Y})");
            Assert.True(System.Math.Abs(blockB.ContentRect.Y - 40) < 1, $"Grid B at Y=40 (25+15gap) (got {blockB.ContentRect.Y})");
            Assert.True(System.Math.Abs(blockC.ContentRect.Y - 90) < 1, $"Grid C at Y=90 (40+35+15gap) (got {blockC.ContentRect.Y})");
        }

        [Fact]
        public void MarginAndPadding_Combined_StackCorrectly()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='width:200px;overflow:hidden'>
                    <div id='a' style='height:20px;padding-bottom:5px;margin-bottom:10px'></div>
                    <div id='b' style='height:30px;margin-top:15px;padding-top:8px'></div>
                </div></body>");
            var blockA = LayoutTestHelper.FindById(root, "a")!;
            var blockB = LayoutTestHelper.FindById(root, "b")!;
            _output.WriteLine($"a.Y={blockA.ContentRect.Y} b.Y={blockB.ContentRect.Y}");
            Assert.True(System.Math.Abs(blockA.ContentRect.Y - 0) < 1, $"Block A at Y=0 (got {blockA.ContentRect.Y})");
            Assert.True(System.Math.Abs(blockB.ContentRect.Y - 48) < 1, $"Block B content at Y=48 (20+5pad+max(10,15)margin+8pad) (got {blockB.ContentRect.Y})");
        }

        [Fact]
        public void PercentageHeight_StacksCorrectly()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='width:200px;height:200px'>
                    <div id='a' style='height:25%'></div>
                    <div id='b' style='height:25%'></div>
                    <div id='c' style='height:25%'></div>
                </div></body>");
            var blockA = LayoutTestHelper.FindById(root, "a")!;
            var blockB = LayoutTestHelper.FindById(root, "b")!;
            var blockC = LayoutTestHelper.FindById(root, "c")!;
            _output.WriteLine($"a.Y={blockA.ContentRect.Y} b.Y={blockB.ContentRect.Y} c.Y={blockC.ContentRect.Y}");
            Assert.True(System.Math.Abs(blockA.ContentRect.Y - 0) < 1, $"Block A at Y=0 (got {blockA.ContentRect.Y})");
            Assert.True(System.Math.Abs(blockB.ContentRect.Y - 50) < 1, $"Block B at Y=50 (25% of 200) (got {blockB.ContentRect.Y})");
            Assert.True(System.Math.Abs(blockC.ContentRect.Y - 100) < 1, $"Block C at Y=100 (50% of 200) (got {blockC.ContentRect.Y})");
        }
    }
}
