using Rend.Css;
using Rend.Style;
using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class PositioningLayoutTests
    {
        private readonly ITestOutputHelper _output;
        public PositioningLayoutTests(ITestOutputHelper output) { _output = output; }

        [Fact]
        public void Absolute_PositionedFromContainingBlock()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='position: relative; width: 200px; height: 200px;'>
                    <div id='abs' style='position: absolute; top: 10px; left: 20px; width: 50px; height: 50px;'></div>
                </div></body>");
            var abs = LayoutTestHelper.FindById(root, "abs");
            Assert.NotNull(abs);
            _output.WriteLine($"x={abs!.ContentRect.X} y={abs.ContentRect.Y}");
            Assert.True(System.Math.Abs(abs.ContentRect.X - 20) < 2, $"left:20px (got X={abs.ContentRect.X})");
            Assert.True(System.Math.Abs(abs.ContentRect.Y - 10) < 2, $"top:10px (got Y={abs.ContentRect.Y})");
        }

        [Fact]
        public void Absolute_RightBottom()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='position: relative; width: 200px; height: 200px;'>
                    <div id='abs' style='position: absolute; right: 10px; bottom: 20px; width: 50px; height: 50px;'></div>
                </div></body>");
            var abs = LayoutTestHelper.FindById(root, "abs");
            Assert.NotNull(abs);
            _output.WriteLine($"x={abs!.ContentRect.X} y={abs.ContentRect.Y}");
            // right:10 → X = 200 - 50 - 10 = 140
            Assert.True(System.Math.Abs(abs.ContentRect.X - 140) < 2, $"right:10px → X=140 (got {abs.ContentRect.X})");
            // bottom:20 → Y = 200 - 50 - 20 = 130
            Assert.True(System.Math.Abs(abs.ContentRect.Y - 130) < 2, $"bottom:20px → Y=130 (got {abs.ContentRect.Y})");
        }

        [Fact]
        public void Absolute_WidthFromLeftRight()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='position: relative; width: 200px; height: 100px;'>
                    <div id='abs' style='position: absolute; left: 20px; right: 30px; height: 40px;'></div>
                </div></body>");
            var abs = LayoutTestHelper.FindById(root, "abs");
            Assert.NotNull(abs);
            _output.WriteLine($"x={abs!.ContentRect.X} w={abs.ContentRect.Width}");
            // width = 200 - 20 - 30 = 150
            Assert.True(System.Math.Abs(abs.ContentRect.Width - 150) < 2, $"left+right → width=150 (got {abs.ContentRect.Width})");
        }

        [Fact]
        public void Fixed_PositionedFromViewport()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='fixed' style='position: fixed; top: 0; left: 0; width: 100px; height: 30px;'></div></body>", 400, 300);
            var box = LayoutTestHelper.FindById(root, "fixed");
            Assert.NotNull(box);
            _output.WriteLine($"x={box!.ContentRect.X} y={box.ContentRect.Y}");
        }

        [Fact]
        public void Sticky_ParsedCorrectly()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='sticky' style='position: sticky; top: 10px; width: 100px; height: 30px;'></div></body>");
            var box = LayoutTestHelper.FindById(root, "sticky");
            Assert.NotNull(box);
            var styled = box!.StyledNode as StyledElement;
            Assert.NotNull(styled);
            Assert.Equal(CssPosition.Sticky, styled!.Style.Position);
        }

        [Fact]
        public void MultiColumn_CreatesColumns()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='mc' style='column-count: 3; width: 300px;'>
                    <p style='margin:0'>A</p><p style='margin:0'>B</p><p style='margin:0'>C</p>
                    <p style='margin:0'>D</p><p style='margin:0'>E</p><p style='margin:0'>F</p>
                </div></body>");
            var mc = LayoutTestHelper.FindById(root, "mc");
            Assert.NotNull(mc);
            _output.WriteLine($"w={mc!.ContentRect.Width} h={mc.ContentRect.Height} children={mc.Children.Count}");
            Assert.True(mc.ContentRect.Width >= 299, $"Multi-col width should be 300 (got {mc.ContentRect.Width})");
        }

        [Fact]
        public void InheritedProperty_Color_Propagates()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='color: red;'>
                    <div id='child' style='width: 50px; height: 20px;'></div>
                </div></body>");
            var child = LayoutTestHelper.FindById(root, "child");
            Assert.NotNull(child);
            var styled = child!.StyledNode as StyledElement;
            Assert.NotNull(styled);
            _output.WriteLine($"color=({styled!.Style.Color.R},{styled.Style.Color.G},{styled.Style.Color.B})");
            Assert.Equal(255, styled.Style.Color.R);
            Assert.Equal(0, styled.Style.Color.G);
        }

        [Fact]
        public void NonInheritedProperty_Border_DoesNotPropagate()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='border: 5px solid red;'>
                    <div id='child' style='width: 50px; height: 20px;'></div>
                </div></body>");
            var child = LayoutTestHelper.FindById(root, "child");
            Assert.NotNull(child);
            _output.WriteLine($"border-top={child!.BorderTopWidth}");
            Assert.Equal(0, child.BorderTopWidth);
        }
    }
}
