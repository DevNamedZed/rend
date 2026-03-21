using Rend.Css;
using Rend.Style;
using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    /// <summary>
    /// Tests for CSS position:sticky behavior.
    /// Sticky positioning acts as relative in static rendering (no scroll offset).
    /// CSS Position L3 §3.4
    /// </summary>
    public class WptPositionStickyTests
    {
        private readonly ITestOutputHelper _output;

        public WptPositionStickyTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void StickyElement_ActsAsRelative_InitialPosition()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:200px'>
                    <div style='height:20px'></div>
                    <div id='t' style='position:sticky;top:0;height:30px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"sticky Y={target.ContentRect.Y}");
            Assert.True(System.Math.Abs(target.ContentRect.Y - 20) < 2,
                $"Sticky element at normal flow position Y=20 (got {target.ContentRect.Y})");
        }

        [Fact]
        public void StickyElement_PreservesFlowSpace()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:200px'>
                    <div id='sticky' style='position:sticky;top:0;height:40px'></div>
                    <div id='after' style='height:20px'></div>
                </div></body>");
            var after = LayoutTestHelper.FindById(root, "after")!;
            _output.WriteLine($"after Y={after.ContentRect.Y}");
            Assert.True(System.Math.Abs(after.ContentRect.Y - 40) < 2,
                $"Element after sticky preserves flow space, Y=40 (got {after.ContentRect.Y})");
        }

        [Fact]
        public void StickyElement_DoesNotAffectSiblingPositions()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:200px'>
                    <div id='before' style='height:25px'></div>
                    <div style='position:sticky;top:10px;height:30px'></div>
                    <div id='after' style='height:25px'></div>
                </div></body>");
            var before = LayoutTestHelper.FindById(root, "before")!;
            var after = LayoutTestHelper.FindById(root, "after")!;
            _output.WriteLine($"before Y={before.ContentRect.Y}, after Y={after.ContentRect.Y}");
            Assert.True(System.Math.Abs(before.ContentRect.Y - 0) < 2,
                $"Before element unaffected (got Y={before.ContentRect.Y})");
            Assert.True(System.Math.Abs(after.ContentRect.Y - 55) < 2,
                $"After element at normal flow Y=55 (got {after.ContentRect.Y})");
        }

        [Fact]
        public void StickyElement_WithTopOffset_AppliedAsRelative()
        {
            // [CSS-POS3 §3.4] Sticky treated as relative in static rendering: top offset shifts element down
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:200px'>
                    <div style='height:50px'></div>
                    <div id='t' style='position:sticky;top:20px;height:30px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"sticky top offset Y={target.ContentRect.Y}");
            Assert.True(System.Math.Abs(target.ContentRect.Y - 70) < 2,
                $"Sticky with top:20px shifts from flow Y=50 to Y=70 (got {target.ContentRect.Y})");
        }

        [Fact]
        public void StickyElement_WithBottomOffset_AppliedAsRelative()
        {
            // [CSS-POS3 §3.4] Sticky treated as relative: bottom offset shifts element up
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:200px;height:200px'>
                    <div style='height:30px'></div>
                    <div id='t' style='position:sticky;bottom:10px;height:30px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"sticky bottom offset Y={target.ContentRect.Y}");
            Assert.True(System.Math.Abs(target.ContentRect.Y - 20) < 2,
                $"Sticky with bottom:10px shifts from flow Y=30 to Y=20 (got {target.ContentRect.Y})");
        }

        [Fact]
        public void StickyElement_WithLeftOffset_AppliedAsRelative()
        {
            // [CSS-POS3 §3.4] Sticky treated as relative: left offset shifts element right
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:300px'>
                    <div id='t' style='position:sticky;left:50px;width:100px;height:30px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"sticky left offset X={target.ContentRect.X}");
            Assert.True(System.Math.Abs(target.ContentRect.X - 50) < 2,
                $"Sticky with left:50px shifts from flow X=0 to X=50 (got {target.ContentRect.X})");
        }

        [Fact]
        public void StickyElement_WithRightOffset_AppliedAsRelative()
        {
            // [CSS-POS3 §3.4] Sticky treated as relative: right offset shifts element left
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:300px'>
                    <div id='t' style='position:sticky;right:20px;width:100px;height:30px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"sticky right offset X={target.ContentRect.X}");
            Assert.True(System.Math.Abs(target.ContentRect.X - (-20)) < 2,
                $"Sticky with right:20px shifts from flow X=0 to X=-20 (got {target.ContentRect.X})");
        }

        [Fact]
        public void StickyElement_InsideOverflowHidden()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:200px;height:100px;overflow:hidden'>
                    <div style='height:20px'></div>
                    <div id='t' style='position:sticky;top:0;height:30px'></div>
                    <div style='height:200px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"sticky in overflow:hidden Y={target.ContentRect.Y}");
            Assert.True(System.Math.Abs(target.ContentRect.Y - 20) < 2,
                $"Sticky in overflow:hidden at flow position (got {target.ContentRect.Y})");
        }

        [Fact]
        public void StickyElement_Width_MatchesContainingBlock()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:250px'>
                    <div id='t' style='position:sticky;top:0;height:30px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"sticky width={target.ContentRect.Width}");
            Assert.True(System.Math.Abs(target.ContentRect.Width - 250) < 2,
                $"Sticky block fills containing block width=250 (got {target.ContentRect.Width})");
        }

        [Fact]
        public void StickyElement_ExplicitWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:300px'>
                    <div id='t' style='position:sticky;top:0;width:150px;height:30px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"sticky explicit width={target.ContentRect.Width}");
            Assert.True(System.Math.Abs(target.ContentRect.Width - 150) < 2,
                $"Sticky respects explicit width=150 (got {target.ContentRect.Width})");
        }

        [Fact]
        public void StickyElement_WithMargin()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:200px'>
                    <div id='t' style='position:sticky;top:0;margin:15px;height:30px'></div>
                    <div id='after' style='height:20px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            var after = LayoutTestHelper.FindById(root, "after")!;
            _output.WriteLine($"sticky with margin X={target.ContentRect.X}, Y={target.ContentRect.Y}");
            Assert.True(System.Math.Abs(target.ContentRect.X - 15) < 2,
                $"Sticky with margin-left=15 (got X={target.ContentRect.X})");
            Assert.True(System.Math.Abs(target.ContentRect.Y - 15) < 2,
                $"Sticky with margin-top=15 (got Y={target.ContentRect.Y})");
            Assert.True(System.Math.Abs(after.ContentRect.Y - 60) < 2,
                $"After sticky+margin: Y=15+30+15=60 (got {after.ContentRect.Y})");
        }

        [Fact]
        public void StickyElement_InsideFlexItem()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div style='flex:1;height:100px'>
                        <div style='height:10px'></div>
                        <div id='t' style='position:sticky;top:0;height:20px'></div>
                    </div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"sticky in flex item Y={target.ContentRect.Y}");
            Assert.True(System.Math.Abs(target.ContentRect.Y - 10) < 2,
                $"Sticky inside flex item at flow Y=10 (got {target.ContentRect.Y})");
        }

        [Fact]
        public void StickyElement_InsideGridItem()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:1fr;width:300px'>
                    <div style='height:100px'>
                        <div style='height:15px'></div>
                        <div id='t' style='position:sticky;top:0;height:25px'></div>
                    </div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"sticky in grid item Y={target.ContentRect.Y}");
            Assert.True(System.Math.Abs(target.ContentRect.Y - 15) < 2,
                $"Sticky inside grid item at flow Y=15 (got {target.ContentRect.Y})");
        }

        [Fact]
        public void StickyElement_WithZIndex()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:200px'>
                    <div id='t' style='position:sticky;top:0;z-index:10;height:30px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            var styledElement = (target.StyledNode as StyledElement)!;
            _output.WriteLine($"sticky z-index={styledElement.Style.ZIndex}");
            Assert.Equal(10, styledElement.Style.ZIndex);
        }

        [Fact]
        public void MultipleStickyElements_IndependentPositions()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:200px'>
                    <div id='s1' style='position:sticky;top:0;height:20px'></div>
                    <div id='s2' style='position:sticky;top:0;height:25px'></div>
                    <div id='s3' style='position:sticky;top:0;height:30px'></div>
                </div></body>");
            var sticky1 = LayoutTestHelper.FindById(root, "s1")!;
            var sticky2 = LayoutTestHelper.FindById(root, "s2")!;
            var sticky3 = LayoutTestHelper.FindById(root, "s3")!;
            _output.WriteLine($"s1 Y={sticky1.ContentRect.Y}, s2 Y={sticky2.ContentRect.Y}, s3 Y={sticky3.ContentRect.Y}");
            Assert.True(System.Math.Abs(sticky1.ContentRect.Y - 0) < 2,
                $"First sticky at Y=0 (got {sticky1.ContentRect.Y})");
            Assert.True(System.Math.Abs(sticky2.ContentRect.Y - 20) < 2,
                $"Second sticky at Y=20 (got {sticky2.ContentRect.Y})");
            Assert.True(System.Math.Abs(sticky3.ContentRect.Y - 45) < 2,
                $"Third sticky at Y=45 (got {sticky3.ContentRect.Y})");
        }

        [Fact]
        public void StickyElement_ContainingBlockIsParent()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='parent' style='width:200px;height:150px'>
                    <div id='t' style='position:sticky;top:0;height:40px'></div>
                </div></body>");
            var parent = LayoutTestHelper.FindById(root, "parent")!;
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"parent height={parent.ContentRect.Height}");
            Assert.True(System.Math.Abs(parent.ContentRect.Height - 150) < 2,
                $"Parent keeps explicit height=150 (got {parent.ContentRect.Height})");
            Assert.True(System.Math.Abs(target.ContentRect.Y - 0) < 2,
                $"Sticky at top of parent (got Y={target.ContentRect.Y})");
        }

        [Fact]
        public void StickyElement_ParentAutoHeight_IncludesSticky()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='parent' style='width:200px'>
                    <div style='height:20px'></div>
                    <div style='position:sticky;top:0;height:35px'></div>
                    <div style='height:25px'></div>
                </div></body>");
            var parent = LayoutTestHelper.FindById(root, "parent")!;
            _output.WriteLine($"parent auto height={parent.ContentRect.Height}");
            Assert.True(System.Math.Abs(parent.ContentRect.Height - 80) < 2,
                $"Parent auto height includes sticky: 20+35+25=80 (got {parent.ContentRect.Height})");
        }

        [Fact]
        public void StickyElement_WithPadding()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:200px'>
                    <div id='t' style='position:sticky;top:0;padding:10px;height:30px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"sticky with padding: content width={target.ContentRect.Width}, height={target.ContentRect.Height}");
            Assert.True(System.Math.Abs(target.ContentRect.Width - 180) < 2,
                $"Content width = 200 - 10 - 10 = 180 (got {target.ContentRect.Width})");
            Assert.True(System.Math.Abs(target.ContentRect.Height - 30) < 2,
                $"Content height = 30 (got {target.ContentRect.Height})");
        }

        [Fact]
        public void StickyElement_WithBorder()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:200px'>
                    <div id='t' style='position:sticky;top:0;border:5px solid black;height:30px'></div>
                    <div id='after' style='height:20px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            var after = LayoutTestHelper.FindById(root, "after")!;
            _output.WriteLine($"sticky with border: content W={target.ContentRect.Width}, H={target.ContentRect.Height}");
            Assert.True(System.Math.Abs(target.ContentRect.Width - 190) < 2,
                $"Content width = 200 - 5 - 5 = 190 (got {target.ContentRect.Width})");
            Assert.True(System.Math.Abs(after.ContentRect.Y - 40) < 2,
                $"After element: Y = 30 + 5 + 5 = 40 (got {after.ContentRect.Y})");
        }

        [Fact]
        public void StickyElement_PositionProperty_IsParsed()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:200px'>
                    <div id='t' style='position:sticky;top:0;height:30px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            var styledElement = (target.StyledNode as StyledElement)!;
            Assert.Equal(CssPosition.Sticky, styledElement.Style.Position);
        }

        [Fact]
        public void StickyElement_WithTopAndBottom_TopTakesPrecedence()
        {
            // [CSS 2.1 §9.4.3] When both top and bottom are set, top wins for relative positioning
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:200px;height:300px'>
                    <div style='height:50px'></div>
                    <div id='t' style='position:sticky;top:10px;bottom:10px;height:40px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"sticky top+bottom Y={target.ContentRect.Y}, H={target.ContentRect.Height}");
            Assert.True(System.Math.Abs(target.ContentRect.Y - 60) < 2,
                $"Sticky with top:10px wins over bottom, Y=50+10=60 (got {target.ContentRect.Y})");
            Assert.True(System.Math.Abs(target.ContentRect.Height - 40) < 2,
                $"Sticky height unchanged (got {target.ContentRect.Height})");
        }

        [Fact]
        public void StickyElement_WithLeftAndRight_LeftTakesPrecedence()
        {
            // [CSS 2.1 §9.4.3] When both left and right are set, left wins for relative positioning (LTR)
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:200px'>
                    <div id='t' style='position:sticky;left:10px;right:10px;height:30px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"sticky left+right X={target.ContentRect.X}, W={target.ContentRect.Width}");
            Assert.True(System.Math.Abs(target.ContentRect.X - 10) < 2,
                $"Sticky with left:10px wins over right, X=0+10=10 (got {target.ContentRect.X})");
        }

        [Fact]
        public void StickyElement_NestedInsideRelativeParent()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:200px;height:200px'>
                    <div style='height:30px'></div>
                    <div id='t' style='position:sticky;top:0;height:25px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"sticky in relative parent Y={target.ContentRect.Y}");
            Assert.True(System.Math.Abs(target.ContentRect.Y - 30) < 2,
                $"Sticky in relative parent at flow Y=30 (got {target.ContentRect.Y})");
        }

        [Fact]
        public void StickyElement_WithPercentageWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:300px'>
                    <div id='t' style='position:sticky;top:0;width:50%;height:30px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"sticky 50% width={target.ContentRect.Width}");
            Assert.True(System.Math.Abs(target.ContentRect.Width - 150) < 2,
                $"Sticky 50% of 300 = 150 (got {target.ContentRect.Width})");
        }

        [Fact]
        public void StickyElement_SiblingBeforeAndAfter()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:200px'>
                    <div id='before' style='height:40px'></div>
                    <div id='sticky' style='position:sticky;top:0;height:30px'></div>
                    <div id='after' style='height:50px'></div>
                </div></body>");
            var before = LayoutTestHelper.FindById(root, "before")!;
            var sticky = LayoutTestHelper.FindById(root, "sticky")!;
            var after = LayoutTestHelper.FindById(root, "after")!;
            _output.WriteLine($"before Y={before.ContentRect.Y}, sticky Y={sticky.ContentRect.Y}, after Y={after.ContentRect.Y}");
            Assert.True(System.Math.Abs(before.ContentRect.Y - 0) < 2);
            Assert.True(System.Math.Abs(sticky.ContentRect.Y - 40) < 2);
            Assert.True(System.Math.Abs(after.ContentRect.Y - 70) < 2);
        }

        [Fact]
        public void StickyElement_InsideOverflowScroll()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:200px;height:80px;overflow:scroll'>
                    <div style='height:20px'></div>
                    <div id='t' style='position:sticky;top:0;height:30px'></div>
                    <div style='height:300px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"sticky in overflow:scroll Y={target.ContentRect.Y}");
            Assert.True(System.Math.Abs(target.ContentRect.Y - 20) < 2,
                $"Sticky in overflow:scroll at flow position (got {target.ContentRect.Y})");
        }

        [Fact]
        public void StickyElement_WithMarginTop_FlowPosition()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:200px'>
                    <div style='height:20px'></div>
                    <div id='t' style='position:sticky;top:0;margin-top:10px;height:30px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"sticky with margin-top Y={target.ContentRect.Y}");
            Assert.True(System.Math.Abs(target.ContentRect.Y - 30) < 2,
                $"Sticky Y = 20 + margin-top 10 = 30 (got {target.ContentRect.Y})");
        }
    }
}
