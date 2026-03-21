using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class WptPositionFixedTests
    {
        private readonly ITestOutputHelper _output;

        public WptPositionFixedTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void Fixed_TopLeft_PositionsFromViewport()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='position:fixed;top:20px;left:30px;width:50px;height:50px'></div>
                </body>", 800, 600);
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"target: ({target.ContentRect.X},{target.ContentRect.Y})");
            Assert.True(System.Math.Abs(target.ContentRect.X - 30) < 2, $"left:30px (got {target.ContentRect.X})");
            Assert.True(System.Math.Abs(target.ContentRect.Y - 20) < 2, $"top:20px (got {target.ContentRect.Y})");
        }

        [Fact]
        public void Fixed_RightBottom_PositionsFromViewportEdges()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='position:fixed;right:10px;bottom:20px;width:60px;height:40px'></div>
                </body>", 800, 600);
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"target: ({target.ContentRect.X},{target.ContentRect.Y})");
            Assert.True(System.Math.Abs(target.ContentRect.X - 730) < 2, $"right:10 -> X=730 (got {target.ContentRect.X})");
            Assert.True(System.Math.Abs(target.ContentRect.Y - 540) < 2, $"bottom:20 -> Y=540 (got {target.ContentRect.Y})");
        }

        [Fact]
        public void Fixed_Inset0_FillsViewport()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='position:fixed;top:0;right:0;bottom:0;left:0'></div>
                </body>", 800, 600);
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"target: ({target.ContentRect.X},{target.ContentRect.Y}) {target.ContentRect.Width}x{target.ContentRect.Height}");
            Assert.True(System.Math.Abs(target.ContentRect.X) < 2, $"X=0 (got {target.ContentRect.X})");
            Assert.True(System.Math.Abs(target.ContentRect.Y) < 2, $"Y=0 (got {target.ContentRect.Y})");
            Assert.True(System.Math.Abs(target.ContentRect.Width - 800) < 2, $"width=800 (got {target.ContentRect.Width})");
            Assert.True(System.Math.Abs(target.ContentRect.Height - 600) < 2, $"height=600 (got {target.ContentRect.Height})");
        }

        [Fact]
        public void Fixed_MarginAuto_CentersHorizontally()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='position:fixed;left:0;right:0;margin:0 auto;width:200px;height:50px;top:0'></div>
                </body>", 800, 600);
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"target X={target.ContentRect.X}");
            Assert.True(System.Math.Abs(target.ContentRect.X - 300) < 2, $"centered X=300 (got {target.ContentRect.X})");
        }

        [Fact]
        public void Fixed_MarginAuto_CentersVertically()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='position:fixed;top:0;bottom:0;margin:auto 0;width:100px;height:200px;left:0'></div>
                </body>", 800, 600);
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"target Y={target.ContentRect.Y}");
            Assert.True(System.Math.Abs(target.ContentRect.Y - 200) < 2, $"centered Y=200 (got {target.ContentRect.Y})");
        }

        [Fact]
        public void Fixed_MarginAuto_CentersBothAxes()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='position:fixed;inset:0;margin:auto;width:200px;height:100px'></div>
                </body>", 800, 600);
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"target: ({target.ContentRect.X},{target.ContentRect.Y})");
            Assert.True(System.Math.Abs(target.ContentRect.X - 300) < 2, $"centered X=300 (got {target.ContentRect.X})");
            Assert.True(System.Math.Abs(target.ContentRect.Y - 250) < 2, $"centered Y=250 (got {target.ContentRect.Y})");
        }

        [Fact]
        public void Fixed_PercentWidth_ResolvesAgainstViewport()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='position:fixed;top:0;left:0;width:50%;height:40px'></div>
                </body>", 800, 600);
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 400) < 2, $"50% of 800=400 (got {target.ContentRect.Width})");
        }

        [Fact]
        public void Fixed_PercentHeight_ResolvesAgainstViewport()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='position:fixed;top:0;left:0;width:40px;height:25%'></div>
                </body>", 800, 600);
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Height - 150) < 2, $"25% of 600=150 (got {target.ContentRect.Height})");
        }

        [Fact]
        public void Fixed_DoesNotAffectSiblingPositions()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='height:40px'></div>
                <div id='fixed' style='position:fixed;top:0;left:0;width:100px;height:500px'></div>
                <div id='sibling' style='height:40px'></div>
                </body>", 800, 600);
            var sibling = LayoutTestHelper.FindById(root, "sibling")!;
            _output.WriteLine($"sibling Y={sibling.ContentRect.Y}");
            Assert.True(System.Math.Abs(sibling.ContentRect.Y - 40) < 2,
                $"sibling at normal flow Y=40, not pushed by fixed (got {sibling.ContentRect.Y})");
        }

        [Fact]
        public void Fixed_DoesNotAffectParentAutoHeight()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='parent' style='width:200px'>
                    <div style='height:50px'></div>
                    <div style='position:fixed;top:0;left:0;height:1000px;width:100px'></div>
                </div>
                </body>", 800, 600);
            var parent = LayoutTestHelper.FindById(root, "parent")!;
            _output.WriteLine($"parent height={parent.ContentRect.Height}");
            Assert.True(System.Math.Abs(parent.ContentRect.Height - 50) < 2,
                $"fixed child should not affect parent auto height (got {parent.ContentRect.Height})");
        }

        [Fact]
        public void Fixed_VwUnits_ResolveAgainstViewportWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='position:fixed;top:0;left:0;width:50vw;height:30px'></div>
                </body>", 800, 600);
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 400) < 2, $"50vw=400 (got {target.ContentRect.Width})");
        }

        [Fact]
        public void Fixed_VhUnits_ResolveAgainstViewportHeight()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='position:fixed;top:0;left:0;width:30px;height:50vh'></div>
                </body>", 800, 600);
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Height - 300) < 2, $"50vh=300 (got {target.ContentRect.Height})");
        }

        [Fact]
        public void Fixed_InNestedPositionedContext_StillUsesViewport()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;margin:50px;width:200px;height:200px'>
                    <div style='position:absolute;top:20px;left:20px;width:100px;height:100px'>
                        <div id='t' style='position:fixed;top:10px;left:10px;width:50px;height:50px'></div>
                    </div>
                </div>
                </body>", 800, 600);
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"target: ({target.ContentRect.X},{target.ContentRect.Y})");
            Assert.True(System.Math.Abs(target.ContentRect.X - 10) < 2,
                $"fixed ignores nested positioned ancestors, X=10 (got {target.ContentRect.X})");
            Assert.True(System.Math.Abs(target.ContentRect.Y - 10) < 2,
                $"fixed ignores nested positioned ancestors, Y=10 (got {target.ContentRect.Y})");
        }

        [Fact]
        public void Fixed_ExplicitWidthHeight()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='position:fixed;top:0;left:0;width:123px;height:456px'></div>
                </body>", 800, 600);
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 123) < 2, $"width=123 (got {target.ContentRect.Width})");
            Assert.True(System.Math.Abs(target.ContentRect.Height - 456) < 2, $"height=456 (got {target.ContentRect.Height})");
        }

        [Fact]
        public void Fixed_ShrinkToFit_AutoWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='position:fixed;top:0;left:0'>
                    <div style='width:80px;height:20px'></div>
                </div>
                </body>", 800, 600);
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"target width={target.ContentRect.Width}");
            Assert.True(target.ContentRect.Width <= 81,
                $"shrink-to-fit: width should be ~80 (got {target.ContentRect.Width})");
            Assert.True(target.ContentRect.Width >= 79,
                $"shrink-to-fit: width should be ~80 (got {target.ContentRect.Width})");
        }

        [Fact]
        public void Fixed_WithPadding()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='position:fixed;top:10px;left:10px;width:100px;height:50px;padding:15px'></div>
                </body>", 800, 600);
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 100) < 2, $"content width=100 (got {target.ContentRect.Width})");
            Assert.True(System.Math.Abs(target.ContentRect.Height - 50) < 2, $"content height=50 (got {target.ContentRect.Height})");
            Assert.True(System.Math.Abs(target.ContentRect.X - 25) < 2, $"content X=10+15=25 (got {target.ContentRect.X})");
            Assert.True(System.Math.Abs(target.ContentRect.Y - 25) < 2, $"content Y=10+15=25 (got {target.ContentRect.Y})");
        }

        [Fact]
        public void Fixed_WithBorder()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='position:fixed;top:5px;left:5px;width:100px;height:60px;border:3px solid black'></div>
                </body>", 800, 600);
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 100) < 2, $"content width=100 (got {target.ContentRect.Width})");
            Assert.True(System.Math.Abs(target.ContentRect.Height - 60) < 2, $"content height=60 (got {target.ContentRect.Height})");
            Assert.True(System.Math.Abs(target.ContentRect.X - 8) < 2, $"content X=5+3=8 (got {target.ContentRect.X})");
            Assert.True(System.Math.Abs(target.ContentRect.Y - 8) < 2, $"content Y=5+3=8 (got {target.ContentRect.Y})");
        }

        [Fact]
        public void Fixed_WithBorderBox_SizingIncludesBorderAndPadding()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='position:fixed;top:0;left:0;width:200px;height:100px;padding:10px;border:5px solid black;box-sizing:border-box'></div>
                </body>", 800, 600);
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"target content: {target.ContentRect.Width}x{target.ContentRect.Height}");
            float expectedContentWidth = 200 - 10 - 10 - 5 - 5;
            float expectedContentHeight = 100 - 10 - 10 - 5 - 5;
            Assert.True(System.Math.Abs(target.ContentRect.Width - expectedContentWidth) < 2,
                $"border-box content width={expectedContentWidth} (got {target.ContentRect.Width})");
            Assert.True(System.Math.Abs(target.ContentRect.Height - expectedContentHeight) < 2,
                $"border-box content height={expectedContentHeight} (got {target.ContentRect.Height})");
        }

        [Fact]
        public void Fixed_ZIndex_IsStored()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='position:fixed;top:0;left:0;width:50px;height:50px;z-index:42'></div>
                </body>", 800, 600);
            var target = LayoutTestHelper.FindById(root, "t")!;
            var styled = (target.StyledNode as Rend.Style.StyledElement)!;
            Assert.Equal(42, styled.Style.ZIndex);
        }

        [Fact]
        public void Fixed_MultipleElements_IndependentPositions()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='topLeft' style='position:fixed;top:0;left:0;width:50px;height:50px'></div>
                <div id='topRight' style='position:fixed;top:0;right:0;width:50px;height:50px'></div>
                <div id='bottomLeft' style='position:fixed;bottom:0;left:0;width:50px;height:50px'></div>
                <div id='bottomRight' style='position:fixed;bottom:0;right:0;width:50px;height:50px'></div>
                </body>", 800, 600);
            var topLeft = LayoutTestHelper.FindById(root, "topLeft")!;
            var topRight = LayoutTestHelper.FindById(root, "topRight")!;
            var bottomLeft = LayoutTestHelper.FindById(root, "bottomLeft")!;
            var bottomRight = LayoutTestHelper.FindById(root, "bottomRight")!;

            Assert.True(System.Math.Abs(topLeft.ContentRect.X) < 2);
            Assert.True(System.Math.Abs(topLeft.ContentRect.Y) < 2);

            Assert.True(System.Math.Abs(topRight.ContentRect.X - 750) < 2, $"topRight X=750 (got {topRight.ContentRect.X})");
            Assert.True(System.Math.Abs(topRight.ContentRect.Y) < 2);

            Assert.True(System.Math.Abs(bottomLeft.ContentRect.X) < 2);
            Assert.True(System.Math.Abs(bottomLeft.ContentRect.Y - 550) < 2, $"bottomLeft Y=550 (got {bottomLeft.ContentRect.Y})");

            Assert.True(System.Math.Abs(bottomRight.ContentRect.X - 750) < 2, $"bottomRight X=750 (got {bottomRight.ContentRect.X})");
            Assert.True(System.Math.Abs(bottomRight.ContentRect.Y - 550) < 2, $"bottomRight Y=550 (got {bottomRight.ContentRect.Y})");
        }

        [Fact]
        public void Fixed_50vw_HalfViewportWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='position:fixed;top:0;left:0;width:50vw;height:50vh'></div>
                </body>", 800, 600);
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 400) < 2, $"50vw=400 (got {target.ContentRect.Width})");
            Assert.True(System.Math.Abs(target.ContentRect.Height - 300) < 2, $"50vh=300 (got {target.ContentRect.Height})");
        }

        [Fact]
        public void Fixed_BottomRightCorner_Positioned()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='position:fixed;bottom:15px;right:25px;width:70px;height:40px'></div>
                </body>", 800, 600);
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.X - 705) < 2, $"right:25 -> X=705 (got {target.ContentRect.X})");
            Assert.True(System.Math.Abs(target.ContentRect.Y - 545) < 2, $"bottom:15 -> Y=545 (got {target.ContentRect.Y})");
        }

        [Fact]
        public void Fixed_WidthFromLeftRight_SpansViewport()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='position:fixed;top:0;left:50px;right:50px;height:40px'></div>
                </body>", 800, 600);
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 700) < 2, $"width=800-50-50=700 (got {target.ContentRect.Width})");
            Assert.True(System.Math.Abs(target.ContentRect.X - 50) < 2, $"X=50 (got {target.ContentRect.X})");
        }

        [Fact]
        public void Fixed_HeightFromTopBottom_SpansViewport()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='position:fixed;left:0;top:30px;bottom:70px;width:40px'></div>
                </body>", 800, 600);
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Height - 500) < 2, $"height=600-30-70=500 (got {target.ContentRect.Height})");
            Assert.True(System.Math.Abs(target.ContentRect.Y - 30) < 2, $"Y=30 (got {target.ContentRect.Y})");
        }

        [Fact]
        public void Fixed_InsideScrollableContent_StillAtViewportCoords()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='height:2000px'>
                    <div id='t' style='position:fixed;top:5px;left:5px;width:60px;height:60px'></div>
                </div>
                </body>", 800, 600);
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.X - 5) < 2, $"fixed X=5 regardless of scroll (got {target.ContentRect.X})");
            Assert.True(System.Math.Abs(target.ContentRect.Y - 5) < 2, $"fixed Y=5 regardless of scroll (got {target.ContentRect.Y})");
        }

        [Fact]
        public void Fixed_WithMargin_OffsetsFromInsetPosition()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='position:fixed;top:10px;left:10px;width:100px;height:50px;margin:20px'></div>
                </body>", 800, 600);
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"target: ({target.ContentRect.X},{target.ContentRect.Y})");
            Assert.True(System.Math.Abs(target.ContentRect.X - 30) < 2, $"left:10+margin:20=30 (got {target.ContentRect.X})");
            Assert.True(System.Math.Abs(target.ContentRect.Y - 30) < 2, $"top:10+margin:20=30 (got {target.ContentRect.Y})");
        }

        [Fact]
        public void Fixed_PercentWidthAndHeight_BothResolveAgainstViewport()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='position:fixed;top:0;left:0;width:75%;height:40%'></div>
                </body>", 800, 600);
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 600) < 2, $"75% of 800=600 (got {target.ContentRect.Width})");
            Assert.True(System.Math.Abs(target.ContentRect.Height - 240) < 2, $"40% of 600=240 (got {target.ContentRect.Height})");
        }

        [Fact]
        public void Fixed_WithPaddingAndBorder_ContentBoxCorrect()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='position:fixed;top:0;left:0;width:100px;height:60px;padding:8px;border:2px solid red'></div>
                </body>", 800, 600);
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 100) < 2, $"content width=100 (got {target.ContentRect.Width})");
            Assert.True(System.Math.Abs(target.ContentRect.Height - 60) < 2, $"content height=60 (got {target.ContentRect.Height})");
            Assert.True(System.Math.Abs(target.ContentRect.X - 10) < 2, $"content X=border(2)+padding(8)=10 (got {target.ContentRect.X})");
            Assert.True(System.Math.Abs(target.ContentRect.Y - 10) < 2, $"content Y=border(2)+padding(8)=10 (got {target.ContentRect.Y})");
        }

        [Fact]
        public void Fixed_NormalFlowSiblingsIgnoreFixed()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='first' style='height:30px;width:100px'></div>
                <div style='position:fixed;top:0;left:0;height:500px;width:500px'></div>
                <div id='second' style='height:30px;width:100px'></div>
                <div id='third' style='height:30px;width:100px'></div>
                </body>", 800, 600);
            var first = LayoutTestHelper.FindById(root, "first")!;
            var second = LayoutTestHelper.FindById(root, "second")!;
            var third = LayoutTestHelper.FindById(root, "third")!;
            Assert.True(System.Math.Abs(first.ContentRect.Y) < 2);
            Assert.True(System.Math.Abs(second.ContentRect.Y - 30) < 2, $"second Y=30 (got {second.ContentRect.Y})");
            Assert.True(System.Math.Abs(third.ContentRect.Y - 60) < 2, $"third Y=60 (got {third.ContentRect.Y})");
        }

        [Fact]
        public void Fixed_IgnoresParentTransform_PositionFromViewport()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='margin:100px;width:200px;height:200px'>
                    <div id='t' style='position:fixed;top:0;left:0;width:30px;height:30px'></div>
                </div>
                </body>", 800, 600);
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.X) < 2,
                $"fixed ignores parent margin, X=0 (got {target.ContentRect.X})");
            Assert.True(System.Math.Abs(target.ContentRect.Y) < 2,
                $"fixed ignores parent margin, Y=0 (got {target.ContentRect.Y})");
        }

        [Fact]
        public void Fixed_TopOnly_LeftDefaultsToAutoStaticPosition()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='position:fixed;top:10px;width:50px;height:50px'></div>
                </body>", 800, 600);
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Y - 10) < 2, $"top:10px (got {target.ContentRect.Y})");
        }

        [Fact]
        public void Fixed_LeftOnly_TopDefaultsToAutoStaticPosition()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='position:fixed;left:20px;width:50px;height:50px'></div>
                </body>", 800, 600);
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.X - 20) < 2, $"left:20px (got {target.ContentRect.X})");
        }
    }
}
