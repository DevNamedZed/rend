using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    /// <summary>
    /// Block formatting context tests: margin collapsing edge cases,
    /// float interactions, auto height calculations, and BFC establishment.
    /// </summary>
    public class WptBlockFormattingTests
    {
        private readonly ITestOutputHelper _output;
        public WptBlockFormattingTests(ITestOutputHelper output) { _output = output; }

        // margin collapse: 3 adjacent siblings with different margins
        [Fact]
        public void MarginCollapse_ThreeSiblings()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:200px'>
                    <div style='margin-bottom:30px;height:20px'></div>
                    <div id='mid' style='margin-top:20px;margin-bottom:40px;height:20px'></div>
                    <div id='last' style='margin-top:25px;height:20px'></div>
                </div></body>");
            // First→mid: collapse(30,20)=30. mid.Y=20+30=50.
            // mid→last: collapse(40,25)=40. last.Y=50+20+40=110.
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "mid")!.ContentRect.Y - 50) < 2,
                $"mid.Y=50 (got {LayoutTestHelper.FindById(r, "mid")!.ContentRect.Y})");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "last")!.ContentRect.Y - 110) < 2,
                $"last.Y=110 (got {LayoutTestHelper.FindById(r, "last")!.ContentRect.Y})");
        }

        // margin collapse blocked by border on parent
        [Fact]
        public void MarginCollapse_BlockedByBorder()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='parent' style='border-top:1px solid;width:200px'>
                    <div id='child' style='margin-top:30px;height:20px'></div>
                </div></body>");
            var parent = LayoutTestHelper.FindById(r, "parent")!;
            var child = LayoutTestHelper.FindById(r, "child")!;
            // Border prevents collapse: child margin is inside parent
            float gap = child.ContentRect.Y - parent.ContentRect.Y;
            Assert.True(gap >= 30, $"Border prevents collapse (gap={gap})");
        }

        // margin collapse blocked by padding on parent
        [Fact]
        public void MarginCollapse_BlockedByPadding()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='parent' style='padding-top:1px;width:200px'>
                    <div id='child' style='margin-top:30px;height:20px'></div>
                </div></body>");
            var child = LayoutTestHelper.FindById(r, "child")!;
            // Padding prevents collapse
            Assert.True(child.ContentRect.Y >= 31, $"Padding prevents collapse (Y={child.ContentRect.Y})");
        }

        // float containment: overflow:hidden contains floats
        [Fact]
        public void OverflowHidden_ContainsFloat_HeightIncludes()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='bfc' style='overflow:hidden;width:200px'>
                    <div style='float:left;width:80px;height:120px'></div>
                    <div style='height:30px'></div>
                </div></body>");
            // BFC height = max(float bottom, content bottom) = 120
            Assert.True(LayoutTestHelper.FindById(r, "bfc")!.ContentRect.Height >= 119,
                $"BFC contains float (h={LayoutTestHelper.FindById(r, "bfc")!.ContentRect.Height})");
        }

        // float containment: normal block doesn't contain floats
        // TODO: Known bug — CalculateAutoHeight includes float children even for non-BFC blocks.
        // Multiple code paths set height; need to ensure all paths check BFC status.
        [Fact]
        public void NormalBlock_DoesNotContainFloat()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='normal' style='width:200px'>
                    <div style='float:left;width:80px;height:120px'></div>
                </div></body>");
            // Normal block auto height = 0 (no in-flow content)
            Assert.True(LayoutTestHelper.FindById(r, "normal")!.ContentRect.Height < 2,
                $"Normal block ignores float (h={LayoutTestHelper.FindById(r, "normal")!.ContentRect.Height})");
        }

        // BFC avoids sibling float: overflow:hidden next to float
        [Fact]
        public void BFC_AvoidsSiblingFloat_OverflowHidden()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:300px'>
                    <div style='float:left;width:100px;height:60px'></div>
                    <div id='bfc' style='overflow:hidden;height:40px'></div>
                </div></body>");
            Assert.True(LayoutTestHelper.FindById(r, "bfc")!.ContentRect.X >= 99,
                $"BFC avoids float (X={LayoutTestHelper.FindById(r, "bfc")!.ContentRect.X})");
        }

        // BFC avoids sibling float: flow-root next to float
        [Fact]
        public void FlowRoot_AvoidsSiblingFloat()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:300px'>
                    <div style='float:left;width:100px;height:60px'></div>
                    <div id='bfc' style='display:flow-root;height:40px'></div>
                </div></body>");
            Assert.True(LayoutTestHelper.FindById(r, "bfc")!.ContentRect.X >= 99,
                $"flow-root avoids float (X={LayoutTestHelper.FindById(r, "bfc")!.ContentRect.X})");
        }

        // auto height with mix of block children
        [Fact]
        public void AutoHeight_MultipleChildren()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='parent' style='width:200px;overflow:hidden'>
                    <div style='height:30px'></div>
                    <div style='height:50px'></div>
                    <div style='height:20px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "parent")!.ContentRect.Height - 100) < 2);
        }

        // auto height with margins between children
        [Fact]
        public void AutoHeight_WithMargins()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='parent' style='width:200px;overflow:hidden'>
                    <div style='margin-bottom:10px;height:30px'></div>
                    <div style='margin-top:10px;height:30px'></div>
                </div></body>");
            // collapse(10,10)=10. Total = 30+10+30 = 70.
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "parent")!.ContentRect.Height - 70) < 2,
                $"Auto height with margins (h={LayoutTestHelper.FindById(r, "parent")!.ContentRect.Height})");
        }

        // clear:left positions below left float only
        [Fact]
        public void ClearLeft_BelowLeftOnly()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:300px'>
                    <div style='float:left;width:80px;height:40px'></div>
                    <div style='float:right;width:80px;height:80px'></div>
                    <div id='t' style='clear:left;height:20px'></div>
                </div></body>");
            var t = LayoutTestHelper.FindById(r, "t")!;
            Assert.True(t.ContentRect.Y >= 39 && t.ContentRect.Y < 79,
                $"clear:left below left(40) not right(80) (Y={t.ContentRect.Y})");
        }

        // clear:right positions below right float only
        [Fact]
        public void ClearRight_BelowRightOnly()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:300px'>
                    <div style='float:left;width:80px;height:80px'></div>
                    <div style='float:right;width:80px;height:40px'></div>
                    <div id='t' style='clear:right;height:20px'></div>
                </div></body>");
            var t = LayoutTestHelper.FindById(r, "t")!;
            Assert.True(t.ContentRect.Y >= 39 && t.ContentRect.Y < 79,
                $"clear:right below right(40) not left(80) (Y={t.ContentRect.Y})");
        }

        // block width: auto width with negative margin
        [Fact]
        public void AutoWidth_NegativeMargin_Expands()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:200px'>
                    <div id='t' style='margin-left:-20px;height:20px'></div>
                </div></body>");
            // auto width = 200 - (-20) = 220
            Assert.True(LayoutTestHelper.FindById(r, "t")!.ContentRect.Width >= 219,
                $"Negative margin expands (w={LayoutTestHelper.FindById(r, "t")!.ContentRect.Width})");
        }

        // inline-block establishes BFC
        [Fact]
        public void InlineBlock_EstablishesBFC()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:300px'>
                    <span id='ib' style='display:inline-block;width:150px'>
                        <div style='float:left;width:60px;height:50px'></div>
                    </span>
                </div></body>");
            Assert.True(LayoutTestHelper.FindById(r, "ib")!.ContentRect.Height >= 49,
                $"inline-block BFC contains float (h={LayoutTestHelper.FindById(r, "ib")!.ContentRect.Height})");
        }

        // flex items establish BFC (child margins don't collapse through)
        [Fact]
        public void FlexItem_BFC_NoMarginCollapse()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;width:200px'>
                    <div id='a' style='margin-bottom:30px;height:20px'></div>
                    <div id='b' style='margin-top:20px;height:20px'></div>
                </div></body>");
            // Flex: NO collapse. gap = 30+20 = 50.
            float gap = LayoutTestHelper.FindById(r, "b")!.ContentRect.Y
                      - (LayoutTestHelper.FindById(r, "a")!.ContentRect.Y + 20);
            Assert.True(System.Math.Abs(gap - 50) < 2, $"Flex no collapse (gap={gap})");
        }

        // grid items establish BFC (child margins don't collapse through)
        [Fact]
        public void GridItem_BFC_ContainsFloat()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:200px;width:200px'>
                    <div id='item'>
                        <div style='float:left;width:60px;height:50px'></div>
                    </div>
                </div></body>");
            Assert.True(LayoutTestHelper.FindById(r, "item")!.ContentRect.Height >= 49);
        }

        // display:none removes element from layout
        [Fact]
        public void DisplayNone_NoSpace()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:200px'>
                    <div style='height:30px'></div>
                    <div style='display:none;height:100px'></div>
                    <div id='t' style='height:30px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Y - 30) < 2,
                $"display:none takes no space (Y={LayoutTestHelper.FindById(r, "t")!.ContentRect.Y})");
        }

        // visibility:hidden takes space but doesn't paint
        [Fact]
        public void VisibilityHidden_TakesSpace()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:200px'>
                    <div style='visibility:hidden;height:50px'></div>
                    <div id='t' style='height:30px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Y - 50) < 2);
        }

        // min-height on auto-height element
        [Fact]
        public void MinHeight_OnAutoHeight_NoContent()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='width:100px;min-height:80px'></div></body>");
            Assert.True(LayoutTestHelper.FindById(r, "t")!.ContentRect.Height >= 79);
        }

        // max-height clamps auto-height element
        [Fact]
        public void MaxHeight_ClampsContent()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='width:100px;max-height:40px'>
                    <div style='height:200px'></div>
                </div></body>");
            Assert.True(LayoutTestHelper.FindById(r, "t")!.ContentRect.Height <= 41);
        }

        // contain:size → auto height = 0
        [Fact]
        public void ContainSize_AutoHeightZero()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='contain:size;width:100px'>
                    <div style='height:200px'></div>
                </div></body>");
            Assert.True(LayoutTestHelper.FindById(r, "t")!.ContentRect.Height < 1);
        }
    }
}
