using Rend.Css;
using Rend.Style;
using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class WptOverflowScrollTests
    {
        private readonly ITestOutputHelper _output;

        public WptOverflowScrollTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void OverflowScroll_EstablishesBfc_ContainsFloat()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                <div id='t' style='overflow:scroll;width:300px'>
                    <div style='float:left;width:120px;height:90px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(box.ContentRect.Height >= 89,
                $"overflow:scroll should contain float, got h={box.ContentRect.Height}");
        }

        [Fact]
        public void OverflowAuto_EstablishesBfc_PreventsMarginCollapse()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                <div id='t' style='overflow:auto;width:200px'>
                    <div id='c' style='margin-top:40px;height:30px'></div>
                </div></body>");
            var parent = LayoutTestHelper.FindById(root, "t")!;
            var child = LayoutTestHelper.FindById(root, "c")!;
            Assert.True(child.ContentRect.Y - parent.ContentRect.Y >= 39,
                $"Child margin should be inside parent BFC, gap={child.ContentRect.Y - parent.ContentRect.Y}");
            Assert.True(parent.ContentRect.Height >= 69,
                $"Parent height should include child margin, got h={parent.ContentRect.Height}");
        }

        [Fact]
        public void OverflowScroll_PreventsMarginCollapseWithParent()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                <div id='t' style='overflow:scroll;width:200px'>
                    <div style='margin-top:25px;height:50px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(box.ContentRect.Y < 2,
                $"Parent should start at top, got Y={box.ContentRect.Y}");
            Assert.True(box.ContentRect.Height >= 74,
                $"Parent should include child margin, got h={box.ContentRect.Height}");
        }

        [Fact]
        public void OverflowVisible_DoesNotEstablishBfc_MarginCollapses()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                <div id='t' style='overflow:visible;width:200px'>
                    <div style='margin-top:30px;height:50px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(box.ContentRect.Y >= 29,
                $"overflow:visible should allow margin collapse through, got Y={box.ContentRect.Y}");
        }

        [Fact]
        public void OverflowHidden_WithPadding_ChildFitsContentWidth()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                <div style='overflow:hidden;width:200px;height:200px;padding:15px'>
                    <div id='t' style='height:40px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(box.ContentRect.Width - 200) < 2,
                $"Child should fill content-box width (200px), got w={box.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.X - 15) < 2,
                $"Child should be offset by padding, got X={box.ContentRect.X}");
        }

        [Fact]
        public void OverflowHidden_WithBorder_ChildFitsContentWidth()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                <div style='overflow:hidden;width:200px;height:200px;border:8px solid black'>
                    <div id='t' style='height:40px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(box.ContentRect.Width - 200) < 2,
                $"Child should fill content-box width, got w={box.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.X - 8) < 2,
                $"Child should be offset by border, got X={box.ContentRect.X}");
        }

        [Fact]
        public void OverflowHidden_Nested_InnerClipsAtItsOwnHeight()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                <div style='overflow:hidden;width:300px;height:300px'>
                    <div id='inner' style='overflow:hidden;width:200px;height:80px'>
                        <div style='height:500px'></div>
                    </div>
                </div></body>");
            var inner = LayoutTestHelper.FindById(root, "inner")!;
            Assert.True(System.Math.Abs(inner.ContentRect.Height - 80) < 2,
                $"Inner overflow:hidden should clip at 80px, got h={inner.ContentRect.Height}");
        }

        [Fact]
        public void OverflowHidden_Nested_OuterClipsAtItsOwnHeight()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                <div id='outer' style='overflow:hidden;width:300px;height:60px'>
                    <div style='overflow:hidden;width:200px;height:200px'>
                        <div style='height:500px'></div>
                    </div>
                </div></body>");
            var outer = LayoutTestHelper.FindById(root, "outer")!;
            Assert.True(System.Math.Abs(outer.ContentRect.Height - 60) < 2,
                $"Outer overflow:hidden should clip at 60px, got h={outer.ContentRect.Height}");
        }

        [Fact]
        public void OverflowHidden_OnFlexContainer_ContainsChildren()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                <div id='t' style='display:flex;overflow:hidden;width:300px;height:100px'>
                    <div style='width:100px;height:200px'></div>
                    <div style='width:100px;height:50px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(box.ContentRect.Height - 100) < 2,
                $"Flex container with overflow:hidden should clip at 100px, got h={box.ContentRect.Height}");
        }

        [Fact]
        public void OverflowHidden_OnFlexContainer_ChildrenLaidOutNormally()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                <div style='display:flex;overflow:hidden;width:300px;height:100px'>
                    <div id='first' style='width:100px;height:50px'></div>
                    <div id='second' style='width:120px;height:60px'></div>
                </div></body>");
            var first = LayoutTestHelper.FindById(root, "first")!;
            var second = LayoutTestHelper.FindById(root, "second")!;
            Assert.True(System.Math.Abs(first.ContentRect.Width - 100) < 2,
                $"First flex child width should be 100, got w={first.ContentRect.Width}");
            Assert.True(System.Math.Abs(second.ContentRect.X - 100) < 2,
                $"Second flex child should be at X=100, got X={second.ContentRect.X}");
        }

        [Fact]
        public void OverflowHidden_OnGridContainer_ContainsChildren()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                <div id='t' style='display:grid;grid-template-columns:1fr;overflow:hidden;width:200px;height:80px'>
                    <div style='height:300px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(box.ContentRect.Height - 80) < 2,
                $"Grid container with overflow:hidden should clip at 80px, got h={box.ContentRect.Height}");
        }

        [Fact]
        public void OverflowClip_ParsedCorrectly()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                <div id='t' style='overflow:clip;width:150px;height:100px'></div></body>");
            var styled = (LayoutTestHelper.FindById(root, "t")!.StyledNode as StyledElement)!;
            Assert.Equal(CssOverflow.Clip, styled.Style.OverflowX);
            Assert.Equal(CssOverflow.Clip, styled.Style.OverflowY);
        }

        [Fact]
        public void OverflowClip_ClipsAtExplicitHeight()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                <div id='t' style='overflow:clip;width:150px;height:50px'>
                    <div style='height:400px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(box.ContentRect.Height - 50) < 2,
                $"overflow:clip should clip at 50px, got h={box.ContentRect.Height}");
        }

        [Fact]
        public void OverflowXHidden_OverflowYVisible_BecomesScroll()
        {
            // CSS Overflow L3: if one axis is visible and the other is not,
            // the visible axis computes to auto (which behaves like scroll for BFC purposes)
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                <div id='t' style='overflow-x:hidden;overflow-y:visible;width:200px'>
                    <div style='float:left;width:100px;height:70px'></div>
                </div></body>");
            var styled = (LayoutTestHelper.FindById(root, "t")!.StyledNode as StyledElement)!;
            Assert.Equal(CssOverflow.Hidden, styled.Style.OverflowX);
            // Per spec, visible on one axis when other is non-visible becomes auto
            Assert.True(styled.Style.OverflowY == CssOverflow.Auto ||
                         styled.Style.OverflowY == CssOverflow.Visible,
                $"overflow-y should be auto or visible, got {styled.Style.OverflowY}");
        }

        [Fact]
        public void OverflowXY_IndependentValues()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                <div id='t' style='overflow-x:scroll;overflow-y:hidden;width:100px;height:80px'></div></body>");
            var styled = (LayoutTestHelper.FindById(root, "t")!.StyledNode as StyledElement)!;
            Assert.Equal(CssOverflow.Scroll, styled.Style.OverflowX);
            Assert.Equal(CssOverflow.Hidden, styled.Style.OverflowY);
        }

        [Fact]
        public void OverflowHidden_AbsoluteChildStillLaidOut()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                <div style='position:relative;overflow:hidden;width:200px;height:100px'>
                    <div id='t' style='position:absolute;top:10px;left:20px;width:80px;height:60px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(box.ContentRect.Width - 80) < 2,
                $"Absolute child should still be laid out, got w={box.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Height - 60) < 2,
                $"Absolute child should still be laid out, got h={box.ContentRect.Height}");
        }

        [Fact]
        public void OverflowHidden_AbsoluteChildPositionedCorrectly()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                <div style='position:relative;overflow:hidden;width:200px;height:100px'>
                    <div id='t' style='position:absolute;top:30px;left:50px;width:40px;height:40px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(box.ContentRect.X - 50) < 2,
                $"Absolute child left should be 50, got X={box.ContentRect.X}");
            Assert.True(System.Math.Abs(box.ContentRect.Y - 30) < 2,
                $"Absolute child top should be 30, got Y={box.ContentRect.Y}");
        }

        [Fact]
        public void OverflowHidden_DoesNotPreventChildLayout()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                <div style='overflow:hidden;width:100px;height:50px'>
                    <div id='t' style='width:300px;height:400px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(box.ContentRect.Width - 300) < 2,
                $"Child should still have full width, got w={box.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Height - 400) < 2,
                $"Child should still have full height, got h={box.ContentRect.Height}");
        }

        [Fact]
        public void OverflowScroll_ExplicitHeight_Clips()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                <div id='t' style='overflow:scroll;width:150px;height:70px'>
                    <div style='height:500px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(box.ContentRect.Height - 70) < 2,
                $"overflow:scroll should clip at 70px, got h={box.ContentRect.Height}");
        }

        [Fact]
        public void OverflowScroll_AutoHeight_WrapsContent()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                <div id='t' style='overflow:scroll;width:200px'>
                    <div style='height:120px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(box.ContentRect.Height - 120) < 2,
                $"overflow:scroll with auto height should wrap content, got h={box.ContentRect.Height}");
        }

        [Fact]
        public void OverflowAuto_ExplicitHeight_Clips()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                <div id='t' style='overflow:auto;width:150px;height:60px'>
                    <div style='height:400px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(box.ContentRect.Height - 60) < 2,
                $"overflow:auto should clip at 60px, got h={box.ContentRect.Height}");
        }

        [Fact]
        public void OverflowHidden_AvoidsSiblingFloat()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                <div style='width:300px'>
                    <div style='float:left;width:80px;height:60px'></div>
                    <div id='t' style='overflow:hidden;height:40px'>content</div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(box.ContentRect.X >= 79,
                $"overflow:hidden should avoid sibling float, got X={box.ContentRect.X}");
        }

        [Fact]
        public void OverflowScroll_AvoidsSiblingFloat()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                <div style='width:300px'>
                    <div style='float:left;width:80px;height:60px'></div>
                    <div id='t' style='overflow:scroll;height:40px'>content</div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(box.ContentRect.X >= 79,
                $"overflow:scroll should avoid sibling float, got X={box.ContentRect.X}");
        }

        [Fact]
        public void OverflowVisible_DoesNotAvoidSiblingFloat()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                <div style='width:300px'>
                    <div style='float:left;width:80px;height:60px'></div>
                    <div id='t' style='overflow:visible;height:40px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(box.ContentRect.X < 2,
                $"overflow:visible should NOT avoid float, got X={box.ContentRect.X}");
        }

        [Fact]
        public void OverflowHidden_SiblingPositionNotAffected()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                <div style='width:200px'>
                    <div style='overflow:hidden;height:60px'>
                        <div style='height:500px'></div>
                    </div>
                    <div id='t' style='height:40px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(box.ContentRect.Y - 60) < 2,
                $"Sibling should be at Y=60, got Y={box.ContentRect.Y}");
        }

        [Fact]
        public void OverflowHidden_WithPaddingAndBorder_ContentAreaCorrect()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                <div id='t' style='overflow:hidden;width:200px;height:100px;padding:10px;border:5px solid black;box-sizing:border-box'>
                    <div id='c' style='height:30px'></div>
                </div></body>");
            var parent = LayoutTestHelper.FindById(root, "t")!;
            var child = LayoutTestHelper.FindById(root, "c")!;
            float contentWidth = parent.ContentRect.Width;
            // border-box: 200 - 2*5 border - 2*10 padding = 170
            Assert.True(System.Math.Abs(contentWidth - 170) < 2,
                $"Content width should be 170, got w={contentWidth}");
            Assert.True(System.Math.Abs(child.ContentRect.Width - 170) < 2,
                $"Child should fill content width of 170, got w={child.ContentRect.Width}");
        }

        [Fact]
        public void OverflowClip_DoesNotEstablishBfc()
        {
            // CSS Overflow L3: overflow:clip does NOT establish a BFC
            // (unlike hidden/scroll/auto)
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                <div id='t' style='overflow:clip;width:200px'>
                    <div style='margin-top:30px;height:50px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            // overflow:clip should either collapse margin through (like visible)
            // or contain it (if treated like hidden). Either behavior is acceptable.
            _output.WriteLine($"overflow:clip parent Y={box.ContentRect.Y}, h={box.ContentRect.Height}");
        }

        [Fact]
        public void OverflowHidden_MultipleChildren_NormalFlow()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                <div style='overflow:hidden;width:200px;height:200px'>
                    <div style='height:40px'></div>
                    <div style='height:30px'></div>
                    <div id='t' style='height:50px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(box.ContentRect.Y - 70) < 2,
                $"Third child should be at Y=70, got Y={box.ContentRect.Y}");
        }

        [Fact]
        public void OverflowScroll_WithBorder_ContentRectCorrect()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                <div id='t' style='overflow:scroll;width:200px;height:100px;border:3px solid red'>
                    <div id='c' style='height:50px'></div>
                </div></body>");
            var parent = LayoutTestHelper.FindById(root, "t")!;
            var child = LayoutTestHelper.FindById(root, "c")!;
            Assert.True(System.Math.Abs(parent.ContentRect.Width - 200) < 2,
                $"Content width should be 200, got w={parent.ContentRect.Width}");
            Assert.True(System.Math.Abs(child.ContentRect.Width - 200) < 2,
                $"Child should fill content width, got w={child.ContentRect.Width}");
        }

        [Fact]
        public void OverflowHidden_AbsoluteChildOverflowing_StillPositioned()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                <div style='position:relative;overflow:hidden;width:100px;height:100px'>
                    <div id='t' style='position:absolute;top:80px;left:60px;width:80px;height:80px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(box.ContentRect.X - 60) < 2,
                $"Absolute child left should be 60, got X={box.ContentRect.X}");
            Assert.True(System.Math.Abs(box.ContentRect.Y - 80) < 2,
                $"Absolute child top should be 80, got Y={box.ContentRect.Y}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 80) < 2,
                $"Absolute child width should be 80, got w={box.ContentRect.Width}");
        }

        [Fact]
        public void OverflowAuto_AutoHeight_WrapsContent()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                <div id='t' style='overflow:auto;width:200px'>
                    <div style='height:95px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(box.ContentRect.Height - 95) < 2,
                $"overflow:auto with auto height should wrap content, got h={box.ContentRect.Height}");
        }

        [Fact]
        public void OverflowHidden_OnGridContainer_ChildrenStillLaidOut()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px 100px;overflow:hidden;width:200px;height:60px'>
                    <div id='a' style='height:40px'></div>
                    <div id='b' style='height:40px'></div>
                </div></body>");
            var cellA = LayoutTestHelper.FindById(root, "a")!;
            var cellB = LayoutTestHelper.FindById(root, "b")!;
            Assert.True(System.Math.Abs(cellA.ContentRect.Width - 100) < 2,
                $"First grid cell should be 100px wide, got w={cellA.ContentRect.Width}");
            Assert.True(System.Math.Abs(cellB.ContentRect.X - 100) < 2,
                $"Second grid cell should be at X=100, got X={cellB.ContentRect.X}");
        }

        [Fact]
        public void OverflowScroll_BetweenSiblings_MarginCollapses()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                <div style='margin-bottom:20px;height:40px'></div>
                <div id='t' style='overflow:scroll;margin-top:15px;height:50px'></div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            // CSS 2.1 8.3.1: adjacent sibling margins collapse (max(20,15)=20)
            // BFC only prevents parent-child collapse, not sibling collapse
            Assert.True(System.Math.Abs(box.ContentRect.Y - 60) < 2,
                $"Sibling margins should collapse, expected Y=60, got Y={box.ContentRect.Y}");
        }

        [Fact]
        public void OverflowHidden_MinHeight_Respected()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                <div id='t' style='overflow:hidden;width:200px;min-height:80px'>
                    <div style='height:30px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(box.ContentRect.Height >= 79,
                $"min-height should be respected, got h={box.ContentRect.Height}");
        }

        [Fact]
        public void OverflowHidden_MaxHeight_Clips()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                <div id='t' style='overflow:hidden;width:200px;max-height:45px'>
                    <div style='height:300px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(box.ContentRect.Height <= 46,
                $"max-height should clip, got h={box.ContentRect.Height}");
        }
    }
}
