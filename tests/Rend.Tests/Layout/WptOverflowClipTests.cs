using Rend.Css;
using Rend.Style;
using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    /// <summary>
    /// CSS overflow clipping behavior and edge cases, covering overflow-x/y axes,
    /// clip value, interaction with display modes, positioning, padding, border,
    /// nesting, and auto height.
    /// </summary>
    public class WptOverflowClipTests
    {
        private readonly ITestOutputHelper _output;
        public WptOverflowClipTests(ITestOutputHelper output) { _output = output; }

        [Fact]
        public void OverflowXHidden_ClipsHorizontally()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='overflow-x:hidden;width:100px;height:50px'>
                    <div style='width:300px;height:30px'></div>
                </div></body>");
            var container = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(container.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(container.ContentRect.Height - 50) < 2);
        }

        [Fact]
        public void OverflowYHidden_ClipsVertically()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='overflow-y:hidden;width:100px;height:60px'>
                    <div style='height:400px'></div>
                </div></body>");
            var container = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(container.ContentRect.Height - 60) < 2);
        }

        [Fact]
        public void OverflowXAuto_ParsedCorrectly()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='overflow-x:auto;width:100px;height:50px'></div></body>");
            var styled = (LayoutTestHelper.FindById(root, "t")!.StyledNode as StyledElement)!;
            Assert.Equal(CssOverflow.Auto, styled.Style.OverflowX);
        }

        [Fact]
        public void OverflowYAuto_ParsedCorrectly()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='overflow-y:auto;width:100px;height:50px'></div></body>");
            var styled = (LayoutTestHelper.FindById(root, "t")!.StyledNode as StyledElement)!;
            Assert.Equal(CssOverflow.Auto, styled.Style.OverflowY);
        }

        [Fact]
        public void OverflowXScroll_ParsedCorrectly()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='overflow-x:scroll;width:100px;height:50px'></div></body>");
            var styled = (LayoutTestHelper.FindById(root, "t")!.StyledNode as StyledElement)!;
            Assert.Equal(CssOverflow.Scroll, styled.Style.OverflowX);
        }

        [Fact]
        public void OverflowYScroll_ParsedCorrectly()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='overflow-y:scroll;width:100px;height:50px'></div></body>");
            var styled = (LayoutTestHelper.FindById(root, "t")!.StyledNode as StyledElement)!;
            Assert.Equal(CssOverflow.Scroll, styled.Style.OverflowY);
        }

        [Fact]
        public void OverflowClip_ParsedOnBothAxes()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='overflow:clip;width:100px;height:50px'></div></body>");
            var styled = (LayoutTestHelper.FindById(root, "t")!.StyledNode as StyledElement)!;
            Assert.Equal(CssOverflow.Clip, styled.Style.OverflowX);
            Assert.Equal(CssOverflow.Clip, styled.Style.OverflowY);
        }

        // [CSS-OVERFLOW-3 §3] Mixed overflow axes: hidden on X, visible on Y
        [Fact]
        public void OverflowXHidden_OverflowYVisible_IndependentAxes()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='overflow-x:hidden;overflow-y:visible;width:100px;height:50px'></div></body>");
            var styled = (LayoutTestHelper.FindById(root, "t")!.StyledNode as StyledElement)!;
            Assert.Equal(CssOverflow.Hidden, styled.Style.OverflowX);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.Height - 50) < 2);
        }

        [Fact]
        public void OverflowHidden_FixedWidth_ContainerWidthRespected()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='overflow:hidden;width:120px;height:80px'>
                    <div style='width:500px;height:500px'></div>
                </div></body>");
            var container = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(container.ContentRect.Width - 120) < 2);
            Assert.True(System.Math.Abs(container.ContentRect.Height - 80) < 2);
        }

        [Fact]
        public void OverflowHidden_FixedHeight_ContainerHeightRespected()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='overflow:hidden;width:100px;height:40px'>
                    <div style='height:300px'></div>
                </div></body>");
            var container = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(container.ContentRect.Height - 40) < 2);
        }

        [Fact]
        public void OverflowHidden_InlineBlock_ClipsContent()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div>
                    <span id='t' style='display:inline-block;overflow:hidden;width:80px;height:40px'>
                        <span style='display:inline-block;width:200px;height:200px'></span>
                    </span>
                </div></body>");
            var container = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(container.ContentRect.Width - 80) < 2);
            Assert.True(System.Math.Abs(container.ContentRect.Height - 40) < 2);
        }

        [Fact]
        public void OverflowHidden_FlexContainer_ClipsContent()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='display:flex;overflow:hidden;width:150px;height:60px'>
                    <div style='width:300px;height:300px;flex-shrink:0'></div>
                </div></body>");
            var container = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(container.ContentRect.Width - 150) < 2);
            Assert.True(System.Math.Abs(container.ContentRect.Height - 60) < 2);
        }

        [Fact]
        public void OverflowHidden_GridContainer_ClipsContent()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='display:grid;overflow:hidden;width:100px;height:50px'>
                    <div style='width:300px;height:300px'></div>
                </div></body>");
            var container = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(container.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(container.ContentRect.Height - 50) < 2);
        }

        [Fact]
        public void OverflowHidden_WithPadding_ClipsAtPaddingEdge()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='overflow:hidden;width:100px;height:60px;padding:10px'>
                    <div style='height:400px'></div>
                </div></body>");
            var container = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(container.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(container.ContentRect.Height - 60) < 2);
            Assert.True(System.Math.Abs(container.PaddingTop - 10) < 2);
            Assert.True(System.Math.Abs(container.PaddingBottom - 10) < 2);
        }

        [Fact]
        public void OverflowHidden_WithBorder_ClipsAtBorderEdge()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='overflow:hidden;width:100px;height:60px;border:5px solid black'>
                    <div style='height:400px'></div>
                </div></body>");
            var container = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(container.ContentRect.Height - 60) < 2);
            Assert.True(System.Math.Abs(container.BorderTopWidth - 5) < 2);
            Assert.True(System.Math.Abs(container.BorderBottomWidth - 5) < 2);
            float borderBoxHeight = container.ContentRect.Height + container.PaddingTop +
                container.PaddingBottom + container.BorderTopWidth + container.BorderBottomWidth;
            Assert.True(System.Math.Abs(borderBoxHeight - 70) < 2);
        }

        [Fact]
        public void OverflowHidden_RelativeChild_ContainedInFlow()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='overflow:hidden;width:200px;height:100px'>
                    <div id='child' style='position:relative;top:10px;left:20px;width:50px;height:50px'></div>
                </div></body>");
            var container = LayoutTestHelper.FindById(root, "t")!;
            var child = LayoutTestHelper.FindById(root, "child")!;
            Assert.True(System.Math.Abs(container.ContentRect.Height - 100) < 2);
            Assert.True(System.Math.Abs(child.ContentRect.Width - 50) < 2);
            Assert.True(System.Math.Abs(child.ContentRect.Height - 50) < 2);
        }

        // [CSS-OVERFLOW-3] Absolute children are clipped by overflow:hidden on containing block
        [Fact]
        public void OverflowHidden_AbsoluteChild_ContainerEstablishesContainingBlock()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='position:relative;overflow:hidden;width:200px;height:100px'>
                    <div id='child' style='position:absolute;top:0;left:0;width:400px;height:400px'></div>
                </div></body>");
            var container = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(container.ContentRect.Width - 200) < 2);
            Assert.True(System.Math.Abs(container.ContentRect.Height - 100) < 2);
            var child = LayoutTestHelper.FindById(root, "child")!;
            Assert.True(System.Math.Abs(child.ContentRect.Width - 400) < 2);
        }

        [Fact]
        public void OverflowHidden_NestedContainers_EachClipsIndependently()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='outer' style='overflow:hidden;width:200px;height:150px'>
                    <div id='inner' style='overflow:hidden;width:100px;height:80px'>
                        <div style='height:500px'></div>
                    </div>
                    <div style='height:500px'></div>
                </div></body>");
            var outer = LayoutTestHelper.FindById(root, "outer")!;
            var inner = LayoutTestHelper.FindById(root, "inner")!;
            Assert.True(System.Math.Abs(outer.ContentRect.Height - 150) < 2);
            Assert.True(System.Math.Abs(inner.ContentRect.Height - 80) < 2);
        }

        [Fact]
        public void OverflowHidden_AutoHeight_ExpandsToFitContent()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='overflow:hidden;width:200px'>
                    <div style='height:90px'></div>
                </div></body>");
            var container = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(container.ContentRect.Height - 90) < 2);
        }

        [Fact]
        public void OverflowVisible_Default_NoClipping()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='width:100px;height:50px'>
                    <div style='width:300px;height:300px'></div>
                </div></body>");
            var container = LayoutTestHelper.FindById(root, "t")!;
            var styled = (container.StyledNode as StyledElement)!;
            Assert.Equal(CssOverflow.Visible, styled.Style.OverflowX);
            Assert.Equal(CssOverflow.Visible, styled.Style.OverflowY);
            Assert.True(System.Math.Abs(container.ContentRect.Height - 50) < 2);
        }

        [Fact]
        public void OverflowScroll_ExplicitHeight_ClipsContent()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='overflow:scroll;width:100px;height:70px'>
                    <div style='height:500px'></div>
                </div></body>");
            var container = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(container.ContentRect.Height - 70) < 2);
        }

        [Fact]
        public void OverflowAuto_ExplicitHeight_ClipsContent()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='overflow:auto;width:100px;height:55px'>
                    <div style='height:400px'></div>
                </div></body>");
            var container = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(container.ContentRect.Height - 55) < 2);
        }

        [Fact]
        public void OverflowHidden_EstablishesBfc_ContainsFloat()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='overflow:hidden;width:200px'>
                    <div style='float:left;width:60px;height:100px'></div>
                </div></body>");
            var container = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(container.ContentRect.Height >= 99,
                $"overflow:hidden establishes BFC, should contain float (h={container.ContentRect.Height})");
        }

        [Fact]
        public void OverflowAuto_EstablishesBfc_ContainsFloat()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='overflow:auto;width:200px'>
                    <div style='float:right;width:60px;height:110px'></div>
                </div></body>");
            var container = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(container.ContentRect.Height >= 109,
                $"overflow:auto establishes BFC, should contain float (h={container.ContentRect.Height})");
        }

        [Fact]
        public void OverflowHidden_PreventsMarginCollapseWithParent()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='parent' style='overflow:hidden;width:200px'>
                    <div id='child' style='margin-top:40px;height:30px'></div>
                </div></body>");
            var parent = LayoutTestHelper.FindById(root, "parent")!;
            var child = LayoutTestHelper.FindById(root, "child")!;
            float gap = child.ContentRect.Y - parent.ContentRect.Y;
            Assert.True(gap >= 39, $"Margin should not collapse through BFC (gap={gap})");
            Assert.True(parent.ContentRect.Height >= 69,
                $"Parent should include child margin (h={parent.ContentRect.Height})");
        }

        [Fact]
        public void OverflowHidden_SiblingNotAffected()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:200px'>
                    <div style='overflow:hidden;height:60px'>
                        <div style='height:500px'></div>
                    </div>
                    <div id='sibling' style='height:20px'></div>
                </div></body>");
            var sibling = LayoutTestHelper.FindById(root, "sibling")!;
            Assert.True(System.Math.Abs(sibling.ContentRect.Y - 60) < 2,
                $"Sibling should be at Y=60 (got {sibling.ContentRect.Y})");
        }

        [Fact]
        public void OverflowHidden_WithBorderRadius_ParsedCorrectly()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='overflow:hidden;width:100px;height:100px;border-radius:10px'>
                    <div style='height:300px'></div>
                </div></body>");
            var container = LayoutTestHelper.FindById(root, "t")!;
            var styled = (container.StyledNode as StyledElement)!;
            Assert.Equal(CssOverflow.Hidden, styled.Style.OverflowX);
            Assert.True(System.Math.Abs(container.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(container.ContentRect.Height - 100) < 2);
        }

        [Fact]
        public void OverflowHidden_WithPaddingAndBorder_ContentBoxSize()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='overflow:hidden;width:100px;height:80px;padding:15px;border:3px solid red'>
                    <div style='height:500px'></div>
                </div></body>");
            var container = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(container.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(container.ContentRect.Height - 80) < 2);
            Assert.True(System.Math.Abs(container.PaddingTop - 15) < 2);
            Assert.True(System.Math.Abs(container.BorderTopWidth - 3) < 2);
        }

        [Fact]
        public void OverflowClip_ExplicitHeight_ClipsContent()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='overflow:clip;width:100px;height:45px'>
                    <div style='height:300px'></div>
                </div></body>");
            var container = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(container.ContentRect.Height - 45) < 2,
                $"overflow:clip should clip at explicit height (h={container.ContentRect.Height})");
        }

        [Fact]
        public void OverflowHidden_FlexItem_ClipsOverflow()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px;height:100px'>
                    <div id='t' style='overflow:hidden;width:100px;height:60px;flex-shrink:0'>
                        <div style='height:400px'></div>
                    </div>
                </div></body>");
            var container = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(container.ContentRect.Height - 60) < 2);
        }

        [Fact]
        public void OverflowHidden_GridItem_ClipsOverflow()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;width:200px'>
                    <div id='t' style='overflow:hidden;height:50px'>
                        <div style='height:400px'></div>
                    </div>
                </div></body>");
            var container = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(container.ContentRect.Height - 50) < 2);
        }

        [Fact]
        public void OverflowHidden_MaxHeight_ClipsAtMaxHeight()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='overflow:hidden;max-height:75px;width:200px'>
                    <div style='height:400px'></div>
                </div></body>");
            var container = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(container.ContentRect.Height <= 76,
                $"max-height should limit container (h={container.ContentRect.Height})");
        }

        [Fact]
        public void OverflowHidden_MinHeight_RespectsMinHeight()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='overflow:hidden;min-height:100px;width:200px'>
                    <div style='height:30px'></div>
                </div></body>");
            var container = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(container.ContentRect.Height >= 99,
                $"min-height should be respected (h={container.ContentRect.Height})");
        }

        [Fact]
        public void OverflowHidden_BorderBox_ContentAreaClips()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='overflow:hidden;box-sizing:border-box;width:120px;height:80px;padding:10px;border:5px solid blue'>
                    <div style='height:500px'></div>
                </div></body>");
            var container = LayoutTestHelper.FindById(root, "t")!;
            float borderBoxHeight = container.ContentRect.Height + container.PaddingTop +
                container.PaddingBottom + container.BorderTopWidth + container.BorderBottomWidth;
            Assert.True(System.Math.Abs(borderBoxHeight - 80) < 2,
                $"border-box total should be 80 (got {borderBoxHeight})");
        }

        [Fact]
        public void OverflowAuto_AutoHeight_ExpandsToContent()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='overflow:auto;width:200px'>
                    <div style='height:120px'></div>
                </div></body>");
            var container = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(container.ContentRect.Height - 120) < 2,
                $"overflow:auto with auto height should wrap content (h={container.ContentRect.Height})");
        }

        [Fact]
        public void OverflowScroll_AutoHeight_ExpandsToContent()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='overflow:scroll;width:200px'>
                    <div style='height:95px'></div>
                </div></body>");
            var container = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(container.ContentRect.Height - 95) < 2,
                $"overflow:scroll with auto height should wrap content (h={container.ContentRect.Height})");
        }
    }
}
