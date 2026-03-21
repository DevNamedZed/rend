using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    /// <summary>
    /// Comprehensive margin collapsing tests per CSS2 §8.3.1.
    /// </summary>
    public class WptMarginCollapseConformanceTests
    {
        private readonly ITestOutputHelper _output;
        public WptMarginCollapseConformanceTests(ITestOutputHelper output) { _output = output; }

        // adjacent siblings: max(20,30) = 30
        [Fact]
        public void Adjacent_MaxWins()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='width:200px'><div style='margin-bottom:20px;height:10px'></div><div id='t' style='margin-top:30px;height:10px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Y - 40) < 2);
        }

        // adjacent siblings: equal margins collapse to one
        [Fact]
        public void Adjacent_Equal()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='width:200px'><div style='margin-bottom:25px;height:10px'></div><div id='t' style='margin-top:25px;height:10px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Y - 35) < 2);
        }

        // parent-first-child: collapse when no border/padding
        [Fact]
        public void ParentChild_Collapse()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='p' style='margin-top:20px;width:200px'><div style='margin-top:30px;height:10px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r, "p")!.ContentRect.Y >= 29);
        }

        // parent-first-child: blocked by border-top
        [Fact]
        public void ParentChild_BlockedByBorder()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='border-top:1px solid;width:200px'><div id='c' style='margin-top:30px;height:10px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r, "c")!.ContentRect.Y >= 31);
        }

        // parent-first-child: blocked by padding-top
        [Fact]
        public void ParentChild_BlockedByPadding()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='padding-top:1px;width:200px'><div id='c' style='margin-top:30px;height:10px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r, "c")!.ContentRect.Y >= 31);
        }

        // parent-first-child: blocked by BFC (overflow:hidden)
        [Fact]
        public void ParentChild_BlockedByBFC()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='p' style='overflow:hidden;width:200px'><div id='c' style='margin-top:30px;height:10px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r, "c")!.ContentRect.Y - LayoutTestHelper.FindById(r, "p")!.ContentRect.Y >= 29);
        }

        // negative+positive: max(pos) + min(neg) = 20 + (-10) = 10
        [Fact]
        public void NegativePositive()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='width:200px'><div style='margin-bottom:20px;height:10px'></div><div id='t' style='margin-top:-10px;height:10px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Y - 20) < 2);
        }

        // two negatives: min(-10,-20) = -20
        [Fact]
        public void TwoNegatives()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='width:200px'><div style='margin-bottom:-10px;height:30px'></div><div id='t' style='margin-top:-20px;height:10px'></div></div></body>");
            // max(0,0)+min(-10,-20) = -20. Y = 30-20 = 10.
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Y - 10) < 2);
        }

        // self-collapsing element: margins pass through
        [Fact]
        public void SelfCollapsing_PassThrough()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='width:200px'><div style='margin-bottom:20px;height:10px'></div><div style='margin-top:10px;margin-bottom:15px'></div><div id='t' style='margin-top:5px;height:10px'></div></div></body>");
            // max(20,10,15,5) = 20. Y = 10+20 = 30.
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Y - 30) < 2);
        }

        // flex items: margins never collapse
        [Fact]
        public void FlexItems_NoCollapse()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-direction:column;width:200px'><div id='a' style='margin-bottom:20px;height:10px'></div><div id='b' style='margin-top:15px;height:10px'></div></div></body>");
            float gap = LayoutTestHelper.FindById(r, "b")!.ContentRect.Y - (LayoutTestHelper.FindById(r, "a")!.ContentRect.Y + 10);
            Assert.True(System.Math.Abs(gap - 35) < 2);
        }

        // grid items: margins never collapse
        [Fact]
        public void GridItems_NoCollapse()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;width:200px'><div id='a' style='margin-bottom:20px;height:10px'></div><div id='b' style='margin-top:15px;height:10px'></div></div></body>");
            float gap = LayoutTestHelper.FindById(r, "b")!.ContentRect.Y - (LayoutTestHelper.FindById(r, "a")!.ContentRect.Y + 10);
            Assert.True(System.Math.Abs(gap - 35) < 2);
        }

        // three siblings with different margins
        [Fact]
        public void ThreeSiblings()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='width:200px'><div style='margin-bottom:30px;height:10px'></div><div style='margin-top:20px;margin-bottom:40px;height:10px'></div><div id='t' style='margin-top:25px;height:10px'></div></div></body>");
            // First gap: max(30,20)=30. mid at 40. Second gap: max(40,25)=40. last at 90.
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Y - 90) < 2);
        }

        // flow-root prevents collapse
        [Fact]
        public void FlowRoot_PreventsCollapse()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='p' style='display:flow-root;width:200px'><div id='c' style='margin-top:30px;height:10px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r, "c")!.ContentRect.Y - LayoutTestHelper.FindById(r, "p")!.ContentRect.Y >= 29);
        }

        // inline-block prevents collapse
        [Fact]
        public void InlineBlock_PreventsCollapse()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='width:300px'><span id='ib' style='display:inline-block;width:200px'><div id='c' style='margin-top:30px;height:10px'></div></span></div></body>");
            Assert.True(LayoutTestHelper.FindById(r, "c")!.ContentRect.Y - LayoutTestHelper.FindById(r, "ib")!.ContentRect.Y >= 29);
        }
    }
}
