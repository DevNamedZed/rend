using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    /// <summary>
    /// Tests verifying correct Y positions for block-level siblings in normal flow.
    /// Each sibling's Y must equal the sum of preceding siblings' heights plus
    /// any margins, padding, or border that separate them.
    /// </summary>
    public class WptBlockSiblingYPositionTests
    {
        private readonly ITestOutputHelper _output;

        public WptBlockSiblingYPositionTests(ITestOutputHelper output)
        {
            _output = output;
        }

        // [CSS2 §9.4.1] Two siblings: second starts at first's height
        [Fact]
        public void TwoSiblings_SecondStartsAtHeightOfFirst()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:200px'>
                    <div style='height:40px'></div>
                    <div id='second' style='height:30px'></div>
                </div></body>");
            var second = LayoutTestHelper.FindById(root, "second")!;
            _output.WriteLine($"second.Y={second.ContentRect.Y}");
            Assert.True(System.Math.Abs(second.ContentRect.Y - 40) < 1,
                $"Second sibling Y should be 40 (got {second.ContentRect.Y})");
        }

        // [CSS2 §9.4.1] Three siblings stack sequentially
        [Fact]
        public void ThreeSiblings_StackSequentially()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:200px'>
                    <div style='height:30px'></div>
                    <div id='second' style='height:40px'></div>
                    <div id='third' style='height:50px'></div>
                </div></body>");
            var second = LayoutTestHelper.FindById(root, "second")!;
            var third = LayoutTestHelper.FindById(root, "third")!;
            _output.WriteLine($"second.Y={second.ContentRect.Y}, third.Y={third.ContentRect.Y}");
            Assert.True(System.Math.Abs(second.ContentRect.Y - 30) < 1,
                $"Second Y should be 30 (got {second.ContentRect.Y})");
            Assert.True(System.Math.Abs(third.ContentRect.Y - 70) < 1,
                $"Third Y should be 70 (got {third.ContentRect.Y})");
        }

        // [CSS2 §9.4.1] Four equal-height siblings
        [Fact]
        public void FourSiblings_EqualHeight20()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:200px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                    <div id='c' style='height:20px'></div>
                    <div id='d' style='height:20px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.Y - 0) < 1);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.Y - 20) < 1);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "c")!.ContentRect.Y - 40) < 1);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "d")!.ContentRect.Y - 60) < 1);
        }

        // [CSS2 §9.4.1] Five equal-height siblings
        [Fact]
        public void FiveSiblings_EqualHeight20()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:200px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                    <div id='c' style='height:20px'></div>
                    <div id='d' style='height:20px'></div>
                    <div id='e' style='height:20px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.Y - 0) < 1);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.Y - 20) < 1);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "c")!.ContentRect.Y - 40) < 1);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "d")!.ContentRect.Y - 60) < 1);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "e")!.ContentRect.Y - 80) < 1);
        }

        // [CSS2 §8.3] margin-top on second sibling pushes it down
        [Fact]
        public void MarginTop_PushesSecondSiblingDown()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:200px'>
                    <div style='height:40px'></div>
                    <div id='second' style='margin-top:20px;height:30px'></div>
                </div></body>");
            var second = LayoutTestHelper.FindById(root, "second")!;
            _output.WriteLine($"second.Y={second.ContentRect.Y}");
            Assert.True(System.Math.Abs(second.ContentRect.Y - 60) < 1,
                $"Second Y should be 40+20=60 (got {second.ContentRect.Y})");
        }

        // [CSS2 §8.3] margin-bottom on first sibling adds space before second
        [Fact]
        public void MarginBottom_AddsSpaceBeforeNextSibling()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:200px'>
                    <div style='height:40px;margin-bottom:25px'></div>
                    <div id='second' style='height:30px'></div>
                </div></body>");
            var second = LayoutTestHelper.FindById(root, "second")!;
            _output.WriteLine($"second.Y={second.ContentRect.Y}");
            Assert.True(System.Math.Abs(second.ContentRect.Y - 65) < 1,
                $"Second Y should be 40+25=65 (got {second.ContentRect.Y})");
        }

        // [CSS2 §8.4] padding-bottom on first sibling: Y accounts for padding
        [Fact]
        public void PaddingBottom_OnFirstSibling_AffectsSecondY()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:200px'>
                    <div style='height:40px;padding-bottom:15px'></div>
                    <div id='second' style='height:30px'></div>
                </div></body>");
            var second = LayoutTestHelper.FindById(root, "second")!;
            _output.WriteLine($"second.Y={second.ContentRect.Y}");
            // first box content=40 + padding-bottom=15 = border-box height 55
            Assert.True(System.Math.Abs(second.ContentRect.Y - 55) < 1,
                $"Second Y should be 40+15=55 (got {second.ContentRect.Y})");
        }

        // [CSS2 §8.5] border-bottom on first sibling: Y accounts for border
        [Fact]
        public void BorderBottom_OnFirstSibling_AffectsSecondY()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:200px'>
                    <div style='height:40px;border-bottom:10px solid black'></div>
                    <div id='second' style='height:30px'></div>
                </div></body>");
            var second = LayoutTestHelper.FindById(root, "second")!;
            _output.WriteLine($"second.Y={second.ContentRect.Y}");
            // first box content=40 + border-bottom=10 = border-box height 50
            Assert.True(System.Math.Abs(second.ContentRect.Y - 50) < 1,
                $"Second Y should be 40+10=50 (got {second.ContentRect.Y})");
        }

        // [CSS2 §9.2.4] display:none sibling takes no space; next sibling unaffected
        [Fact]
        public void DisplayNone_SiblingSkipped()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:200px'>
                    <div style='height:40px'></div>
                    <div style='display:none;height:100px'></div>
                    <div id='third' style='height:30px'></div>
                </div></body>");
            var third = LayoutTestHelper.FindById(root, "third")!;
            _output.WriteLine($"third.Y={third.ContentRect.Y}");
            Assert.True(System.Math.Abs(third.ContentRect.Y - 40) < 1,
                $"Third Y should be 40, display:none skipped (got {third.ContentRect.Y})");
        }

        // [CSS2 §11.2] visibility:hidden sibling still occupies space
        [Fact]
        public void VisibilityHidden_SiblingTakesSpace()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:200px'>
                    <div style='height:40px'></div>
                    <div style='visibility:hidden;height:50px'></div>
                    <div id='third' style='height:30px'></div>
                </div></body>");
            var third = LayoutTestHelper.FindById(root, "third")!;
            _output.WriteLine($"third.Y={third.ContentRect.Y}");
            Assert.True(System.Math.Abs(third.ContentRect.Y - 90) < 1,
                $"Third Y should be 40+50=90, hidden takes space (got {third.ContentRect.Y})");
        }

        // [CSS2 §9.6.1] position:absolute sibling is out of flow, not counted
        [Fact]
        public void AbsolutePosition_SiblingOutOfFlow()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:200px;position:relative'>
                    <div style='height:40px'></div>
                    <div style='position:absolute;height:100px;width:50px'></div>
                    <div id='third' style='height:30px'></div>
                </div></body>");
            var third = LayoutTestHelper.FindById(root, "third")!;
            _output.WriteLine($"third.Y={third.ContentRect.Y}");
            Assert.True(System.Math.Abs(third.ContentRect.Y - 40) < 1,
                $"Third Y should be 40, abspos out of flow (got {third.ContentRect.Y})");
        }

        // [CSS2 §8.3.1] negative margin-top pulls sibling up, overlapping
        [Fact]
        public void NegativeMarginTop_OverlapsSibling()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:200px'>
                    <div style='height:40px'></div>
                    <div id='second' style='margin-top:-15px;height:30px'></div>
                </div></body>");
            var second = LayoutTestHelper.FindById(root, "second")!;
            _output.WriteLine($"second.Y={second.ContentRect.Y}");
            Assert.True(System.Math.Abs(second.ContentRect.Y - 25) < 1,
                $"Second Y should be 40-15=25 (got {second.ContentRect.Y})");
        }

        // [CSS2 §8.3.1] adjacent margins collapse: larger wins
        [Fact]
        public void MarginCollapse_LargerWins()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:200px'>
                    <div style='height:30px;margin-bottom:20px'></div>
                    <div id='second' style='margin-top:35px;height:30px'></div>
                </div></body>");
            var second = LayoutTestHelper.FindById(root, "second")!;
            _output.WriteLine($"second.Y={second.ContentRect.Y}");
            // collapse(20, 35) = 35
            Assert.True(System.Math.Abs(second.ContentRect.Y - 65) < 1,
                $"Second Y should be 30+35=65 (got {second.ContentRect.Y})");
        }

        // [CSS2 §8.3.1] adjacent equal margins collapse to one
        [Fact]
        public void MarginCollapse_EqualMargins()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:200px'>
                    <div style='height:30px;margin-bottom:25px'></div>
                    <div id='second' style='margin-top:25px;height:30px'></div>
                </div></body>");
            var second = LayoutTestHelper.FindById(root, "second")!;
            _output.WriteLine($"second.Y={second.ContentRect.Y}");
            // collapse(25, 25) = 25
            Assert.True(System.Math.Abs(second.ContentRect.Y - 55) < 1,
                $"Second Y should be 30+25=55 (got {second.ContentRect.Y})");
        }

        // [CSS Flexbox §3] flex column: margins do NOT collapse between siblings
        [Fact]
        public void FlexColumn_NoMarginCollapse()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;width:200px'>
                    <div style='height:30px;margin-bottom:20px'></div>
                    <div id='second' style='margin-top:15px;height:30px'></div>
                </div></body>");
            var second = LayoutTestHelper.FindById(root, "second")!;
            _output.WriteLine($"second.Y={second.ContentRect.Y}");
            // Flex: no collapse, gap = 20 + 15 = 35
            Assert.True(System.Math.Abs(second.ContentRect.Y - 65) < 2,
                $"Flex column: second Y should be 30+20+15=65 (got {second.ContentRect.Y})");
        }

        // [CSS2 §9.4.1] block flow: margins collapse, so gap differs from flex
        [Fact]
        public void BlockFlow_MarginsCollapse_VsFlexComparison()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:200px'>
                    <div style='height:30px;margin-bottom:20px'></div>
                    <div id='second' style='margin-top:15px;height:30px'></div>
                </div></body>");
            var second = LayoutTestHelper.FindById(root, "second")!;
            _output.WriteLine($"second.Y={second.ContentRect.Y}");
            // Block: collapse(20, 15) = 20
            Assert.True(System.Math.Abs(second.ContentRect.Y - 50) < 1,
                $"Block flow: second Y should be 30+20=50 (got {second.ContentRect.Y})");
        }

        // [CSS2 §9.4.1] nested children stack within parent; parent Y accumulates
        [Fact]
        public void NestedChildren_StackWithinParent()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:200px'>
                    <div style='height:30px'></div>
                    <div id='parent' style='padding:10px'>
                        <div id='child1' style='height:20px'></div>
                        <div id='child2' style='height:25px'></div>
                    </div>
                    <div id='after' style='height:15px'></div>
                </div></body>");
            var parent = LayoutTestHelper.FindById(root, "parent")!;
            var child1 = LayoutTestHelper.FindById(root, "child1")!;
            var child2 = LayoutTestHelper.FindById(root, "child2")!;
            var after = LayoutTestHelper.FindById(root, "after")!;
            _output.WriteLine($"parent.Y={parent.ContentRect.Y}, child1.Y={child1.ContentRect.Y}, child2.Y={child2.ContentRect.Y}, after.Y={after.ContentRect.Y}");
            // parent content starts at 30 + 10 (padding-top) = 40
            Assert.True(System.Math.Abs(child1.ContentRect.Y - 40) < 1,
                $"child1 Y should be 40 (got {child1.ContentRect.Y})");
            // child2 at 40 + 20 = 60
            Assert.True(System.Math.Abs(child2.ContentRect.Y - 60) < 1,
                $"child2 Y should be 60 (got {child2.ContentRect.Y})");
            // after parent: 30 (first) + 10 (pad-top) + 20 + 25 + 10 (pad-bottom) = 95
            Assert.True(System.Math.Abs(after.ContentRect.Y - 95) < 1,
                $"after Y should be 95 (got {after.ContentRect.Y})");
        }

        // [CSS2 §9.5.2] after float with clear:left, sibling stacks below float
        [Fact]
        public void AfterFloat_ClearLeft_SiblingBelowFloat()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:200px'>
                    <div style='float:left;width:80px;height:60px'></div>
                    <div style='clear:left;height:30px'></div>
                    <div id='after' style='height:20px'></div>
                </div></body>");
            var after = LayoutTestHelper.FindById(root, "after")!;
            _output.WriteLine($"after.Y={after.ContentRect.Y}");
            // cleared div at Y=60, height=30, so after at 90
            Assert.True(System.Math.Abs(after.ContentRect.Y - 90) < 1,
                $"After float+clear, Y should be 90 (got {after.ContentRect.Y})");
        }

        // [CSS2 §9.4.1] five blocks with varying heights sum to correct final Y
        [Fact]
        public void FiveBlocks_VaryingHeights_SumToCorrectY()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:200px'>
                    <div style='height:10px'></div>
                    <div style='height:25px'></div>
                    <div style='height:15px'></div>
                    <div style='height:35px'></div>
                    <div id='last' style='height:20px'></div>
                </div></body>");
            var last = LayoutTestHelper.FindById(root, "last")!;
            _output.WriteLine($"last.Y={last.ContentRect.Y}");
            // 10 + 25 + 15 + 35 = 85
            Assert.True(System.Math.Abs(last.ContentRect.Y - 85) < 1,
                $"Last Y should be 10+25+15+35=85 (got {last.ContentRect.Y})");
        }

        // [CSS2 §9.4.1] different heights accumulate precisely
        [Fact]
        public void DifferentHeights_AccumulatePrecisely()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:200px'>
                    <div id='a' style='height:17px'></div>
                    <div id='b' style='height:33px'></div>
                    <div id='c' style='height:11px'></div>
                    <div id='d' style='height:29px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.Y - 0) < 1);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.Y - 17) < 1);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "c")!.ContentRect.Y - 50) < 1);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "d")!.ContentRect.Y - 61) < 1);
        }

        // [CSS2 §8.3] margin-top + margin-bottom combined spacing
        [Fact]
        public void MarginTopAndBottom_CombinedSpacing()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:200px'>
                    <div style='height:30px;margin-bottom:10px'></div>
                    <div id='second' style='margin-top:10px;height:20px;margin-bottom:15px'></div>
                    <div id='third' style='margin-top:5px;height:25px'></div>
                </div></body>");
            var second = LayoutTestHelper.FindById(root, "second")!;
            var third = LayoutTestHelper.FindById(root, "third")!;
            _output.WriteLine($"second.Y={second.ContentRect.Y}, third.Y={third.ContentRect.Y}");
            // first→second: collapse(10, 10) = 10, second.Y = 30+10 = 40
            Assert.True(System.Math.Abs(second.ContentRect.Y - 40) < 1,
                $"Second Y should be 40 (got {second.ContentRect.Y})");
            // second→third: collapse(15, 5) = 15, third.Y = 40+20+15 = 75
            Assert.True(System.Math.Abs(third.ContentRect.Y - 75) < 1,
                $"Third Y should be 75 (got {third.ContentRect.Y})");
        }

        // [CSS2 §8.4] padding-top on sibling offsets its content Y
        [Fact]
        public void PaddingTop_OnSecondSibling_OffsetsItsContentY()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:200px'>
                    <div style='height:40px'></div>
                    <div id='second' style='padding-top:12px;height:30px'></div>
                </div></body>");
            var second = LayoutTestHelper.FindById(root, "second")!;
            _output.WriteLine($"second.ContentRect.Y={second.ContentRect.Y}");
            // ContentRect.Y = border box Y + padding-top = 40 + 12 = 52
            Assert.True(System.Math.Abs(second.ContentRect.Y - 52) < 1,
                $"Second content Y should be 40+12=52 (got {second.ContentRect.Y})");
        }

        // [CSS2 §8.5] border-top on sibling offsets its content Y
        [Fact]
        public void BorderTop_OnSecondSibling_OffsetsItsContentY()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:200px'>
                    <div style='height:40px'></div>
                    <div id='second' style='border-top:8px solid black;height:30px'></div>
                </div></body>");
            var second = LayoutTestHelper.FindById(root, "second")!;
            _output.WriteLine($"second.ContentRect.Y={second.ContentRect.Y}");
            // ContentRect.Y = border box Y + border-top = 40 + 8 = 48
            Assert.True(System.Math.Abs(second.ContentRect.Y - 48) < 1,
                $"Second content Y should be 40+8=48 (got {second.ContentRect.Y})");
        }

        // [CSS2 §8.3.1] negative margin-bottom on first reduces gap
        [Fact]
        public void NegativeMarginBottom_ReducesGap()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:200px'>
                    <div style='height:50px;margin-bottom:-10px'></div>
                    <div id='second' style='height:30px'></div>
                </div></body>");
            var second = LayoutTestHelper.FindById(root, "second")!;
            _output.WriteLine($"second.Y={second.ContentRect.Y}");
            Assert.True(System.Math.Abs(second.ContentRect.Y - 40) < 1,
                $"Second Y should be 50-10=40 (got {second.ContentRect.Y})");
        }

        // [CSS2 §9.4.1] multiple display:none siblings all skipped
        [Fact]
        public void MultipleDisplayNone_AllSkipped()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:200px'>
                    <div style='height:30px'></div>
                    <div style='display:none;height:50px'></div>
                    <div style='display:none;height:60px'></div>
                    <div style='display:none;height:70px'></div>
                    <div id='visible' style='height:20px'></div>
                </div></body>");
            var visible = LayoutTestHelper.FindById(root, "visible")!;
            _output.WriteLine($"visible.Y={visible.ContentRect.Y}");
            Assert.True(System.Math.Abs(visible.ContentRect.Y - 30) < 1,
                $"Visible Y should be 30, all none siblings skipped (got {visible.ContentRect.Y})");
        }

        // [CSS2 §9.6.1] fixed position sibling is out of flow
        [Fact]
        public void FixedPosition_SiblingOutOfFlow()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:200px'>
                    <div style='height:40px'></div>
                    <div style='position:fixed;height:100px;width:50px'></div>
                    <div id='third' style='height:30px'></div>
                </div></body>");
            var third = LayoutTestHelper.FindById(root, "third")!;
            _output.WriteLine($"third.Y={third.ContentRect.Y}");
            Assert.True(System.Math.Abs(third.ContentRect.Y - 40) < 1,
                $"Third Y should be 40, fixed position out of flow (got {third.ContentRect.Y})");
        }

        // [CSS2 §8.3.1] three margins collapsing: max of all positive margins wins
        [Fact]
        public void ThreeWayMarginCollapse_MaxWins()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:200px'>
                    <div style='height:20px;margin-bottom:30px'></div>
                    <div style='margin-top:10px;margin-bottom:15px'></div>
                    <div id='third' style='margin-top:25px;height:20px'></div>
                </div></body>");
            var third = LayoutTestHelper.FindById(root, "third")!;
            _output.WriteLine($"third.Y={third.ContentRect.Y}");
            // Self-collapsing middle: all margins collapse: max(30, 10, 15, 25) = 30
            Assert.True(System.Math.Abs(third.ContentRect.Y - 50) < 1,
                $"Third Y should be 20+30=50 (got {third.ContentRect.Y})");
        }

        // [CSS2 §9.4.1] padding + border + margin on first, plain second
        [Fact]
        public void PaddingBorderMargin_AllContributeToSecondY()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:200px'>
                    <div style='height:20px;padding-bottom:5px;border-bottom:3px solid black;margin-bottom:7px'></div>
                    <div id='second' style='height:15px'></div>
                </div></body>");
            var second = LayoutTestHelper.FindById(root, "second")!;
            _output.WriteLine($"second.Y={second.ContentRect.Y}");
            // 20 (content) + 5 (padding) + 3 (border) + 7 (margin) = 35
            Assert.True(System.Math.Abs(second.ContentRect.Y - 35) < 1,
                $"Second Y should be 20+5+3+7=35 (got {second.ContentRect.Y})");
        }

        // [CSS2 §9.4.1] six siblings accumulate correctly with margins
        [Fact]
        public void SixSiblings_WithMargins()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:200px'>
                    <div style='height:10px;margin-bottom:5px'></div>
                    <div style='height:10px;margin-bottom:5px'></div>
                    <div style='height:10px;margin-bottom:5px'></div>
                    <div style='height:10px;margin-bottom:5px'></div>
                    <div style='height:10px;margin-bottom:5px'></div>
                    <div id='sixth' style='height:10px'></div>
                </div></body>");
            var sixth = LayoutTestHelper.FindById(root, "sixth")!;
            _output.WriteLine($"sixth.Y={sixth.ContentRect.Y}");
            // 5 * (10 + 5) = 75
            Assert.True(System.Math.Abs(sixth.ContentRect.Y - 75) < 1,
                $"Sixth Y should be 5*(10+5)=75 (got {sixth.ContentRect.Y})");
        }

        // [CSS2 §9.5.2] clear:both after float pushes sibling below
        [Fact]
        public void ClearBoth_AfterFloat_SiblingBelowFloat()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:200px'>
                    <div style='float:left;width:80px;height:45px'></div>
                    <div style='clear:both;height:25px'></div>
                    <div id='third' style='height:20px'></div>
                </div></body>");
            var third = LayoutTestHelper.FindById(root, "third")!;
            _output.WriteLine($"third.Y={third.ContentRect.Y}");
            // cleared at Y=45, height=25, so third at 70
            Assert.True(System.Math.Abs(third.ContentRect.Y - 70) < 1,
                $"Third Y should be 45+25=70 (got {third.ContentRect.Y})");
        }

        // [CSS2 §8.3.1] positive and negative margin collapse: sum of max positive + min negative
        [Fact]
        public void PositiveAndNegativeMargins_CollapseCorrectly()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:200px'>
                    <div style='height:40px;margin-bottom:30px'></div>
                    <div id='second' style='margin-top:-10px;height:20px'></div>
                </div></body>");
            var second = LayoutTestHelper.FindById(root, "second")!;
            _output.WriteLine($"second.Y={second.ContentRect.Y}");
            // CSS2 §8.3.1: result = max(positive) + min(negative) = 30 + (-10) = 20
            Assert.True(System.Math.Abs(second.ContentRect.Y - 60) < 1,
                $"Second Y should be 40+30-10=60 (got {second.ContentRect.Y})");
        }

        // [CSS2 §9.4.1] sibling with both padding-top and margin-top
        [Fact]
        public void PaddingAndMarginTop_BothContribute()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:200px'>
                    <div style='height:30px'></div>
                    <div id='second' style='margin-top:10px;padding-top:8px;height:20px'></div>
                </div></body>");
            var second = LayoutTestHelper.FindById(root, "second")!;
            _output.WriteLine($"second.ContentRect.Y={second.ContentRect.Y}");
            // border-box Y = 30 + 10(margin) = 40, content Y = 40 + 8(padding) = 48
            Assert.True(System.Math.Abs(second.ContentRect.Y - 48) < 1,
                $"Second content Y should be 30+10+8=48 (got {second.ContentRect.Y})");
        }

        // [CSS2 §9.4.1] zero-height sibling still marks a boundary for margin collapse
        [Fact]
        public void ZeroHeightSibling_WithBorder_PreventsThrough()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:200px'>
                    <div style='height:30px;margin-bottom:20px'></div>
                    <div style='border-top:1px solid black;margin-top:10px;margin-bottom:15px'></div>
                    <div id='third' style='margin-top:5px;height:20px'></div>
                </div></body>");
            var third = LayoutTestHelper.FindById(root, "third")!;
            _output.WriteLine($"third.Y={third.ContentRect.Y}");
            // First bottom(20) collapses with middle top(10) → 20. Middle has border so not self-collapsing.
            // Middle bottom(15) collapses with third top(5) → 15.
            // third.Y = 30 + 20 + 1(border) + 15 = 66
            Assert.True(System.Math.Abs(third.ContentRect.Y - 66) < 1,
                $"Third Y should be 66 (got {third.ContentRect.Y})");
        }

        // [CSS Flexbox §3] flex row: items side by side, all at same Y
        [Fact]
        public void FlexRow_SiblingsAtSameY()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:200px'>
                    <div id='a' style='width:50px;height:40px'></div>
                    <div id='b' style='width:50px;height:30px'></div>
                    <div id='c' style='width:50px;height:50px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            _output.WriteLine($"a.Y={itemA.ContentRect.Y}, b.Y={itemB.ContentRect.Y}, c.Y={itemC.ContentRect.Y}");
            // In flex row, all items share the same Y (cross-start)
            Assert.True(System.Math.Abs(itemA.ContentRect.Y - itemB.ContentRect.Y) < 1);
            Assert.True(System.Math.Abs(itemA.ContentRect.Y - itemC.ContentRect.Y) < 1);
        }

        // [CSS2 §9.4.1] large number of small blocks: Y accumulates linearly
        [Fact]
        public void TenSmallBlocks_LinearAccumulation()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:200px'>
                    <div style='height:10px'></div>
                    <div style='height:10px'></div>
                    <div style='height:10px'></div>
                    <div style='height:10px'></div>
                    <div style='height:10px'></div>
                    <div style='height:10px'></div>
                    <div style='height:10px'></div>
                    <div style='height:10px'></div>
                    <div style='height:10px'></div>
                    <div id='tenth' style='height:10px'></div>
                </div></body>");
            var tenth = LayoutTestHelper.FindById(root, "tenth")!;
            _output.WriteLine($"tenth.Y={tenth.ContentRect.Y}");
            Assert.True(System.Math.Abs(tenth.ContentRect.Y - 90) < 1,
                $"Tenth Y should be 9*10=90 (got {tenth.ContentRect.Y})");
        }

        // [CSS2 §9.4.1] sibling after element with border-box sizing
        [Fact]
        public void BorderBoxSizing_SiblingYAccountsForTotalHeight()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:200px'>
                    <div style='box-sizing:border-box;height:60px;padding:10px;border:5px solid black'></div>
                    <div id='second' style='height:20px'></div>
                </div></body>");
            var second = LayoutTestHelper.FindById(root, "second")!;
            _output.WriteLine($"second.Y={second.ContentRect.Y}");
            // border-box: total height is 60px (includes padding+border)
            Assert.True(System.Math.Abs(second.ContentRect.Y - 60) < 1,
                $"Second Y should be 60 (border-box total) (got {second.ContentRect.Y})");
        }

        // [CSS2 §9.4.1] content-box: border and padding add to total height
        [Fact]
        public void ContentBoxSizing_BorderPaddingAddToHeight()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:200px'>
                    <div style='height:30px;padding:10px;border:5px solid black'></div>
                    <div id='second' style='height:20px'></div>
                </div></body>");
            var second = LayoutTestHelper.FindById(root, "second")!;
            _output.WriteLine($"second.Y={second.ContentRect.Y}");
            // content-box: total = 5(bt) + 10(pt) + 30(content) + 10(pb) + 5(bb) = 60
            Assert.True(System.Math.Abs(second.ContentRect.Y - 60) < 1,
                $"Second Y should be 5+10+30+10+5=60 (got {second.ContentRect.Y})");
        }
    }
}
