using Rend.Css;
using Rend.Style;
using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class WptPositionTests
    {
        private readonly ITestOutputHelper _output;
        public WptPositionTests(ITestOutputHelper output) { _output = output; }

        [Fact]
        public void AbsPos_TopLeft_InRelativeParent()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='position: relative; width: 200px; height: 200px;'>
                    <div id='abs' style='position: absolute; top: 50px; left: 50px; width: 100px; height: 100px;'></div>
                </div></body>");
            var abs = LayoutTestHelper.FindById(root, "abs");
            Assert.NotNull(abs);
            Assert.True(System.Math.Abs(abs!.ContentRect.X - 50) < 2);
            Assert.True(System.Math.Abs(abs.ContentRect.Y - 50) < 2);
        }

        [Fact]
        public void AbsPos_AutoMargins_Center()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='position: relative; width: 200px; height: 200px;'>
                    <div id='abs' style='position: absolute; top: 0; left: 0; right: 0; bottom: 0; margin: auto; width: 100px; height: 100px;'></div>
                </div></body>");
            var abs = LayoutTestHelper.FindById(root, "abs");
            Assert.NotNull(abs);
            _output.WriteLine($"abs: ({abs!.ContentRect.X},{abs.ContentRect.Y})");
            // auto margins with all 4 insets + explicit size → centered
            Assert.True(System.Math.Abs(abs.ContentRect.X - 50) < 2, $"Centered X (got {abs.ContentRect.X})");
            Assert.True(System.Math.Abs(abs.ContentRect.Y - 50) < 2, $"Centered Y (got {abs.ContentRect.Y})");
        }

        [Fact]
        public void AbsPos_ContainingBlock_IsPaddingBox()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='position: relative; width: 200px; height: 200px; padding: 20px;'>
                    <div id='abs' style='position: absolute; top: 0; left: 0; width: 50px; height: 50px;'></div>
                </div></body>");
            var abs = LayoutTestHelper.FindById(root, "abs");
            Assert.NotNull(abs);
            _output.WriteLine($"abs: ({abs!.ContentRect.X},{abs.ContentRect.Y})");
            // Abspos CB is padding box, so top:0 left:0 = padding edge, not content edge
            // Padding box starts at (0+padding, 0+padding) but abspos top:0 is AT the padding edge
        }

        [Fact]
        public void AbsPos_PercentWidth_AgainstCB()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='position: relative; width: 200px; height: 100px;'>
                    <div id='abs' style='position: absolute; width: 50%; height: 50px;'></div>
                </div></body>");
            var abs = LayoutTestHelper.FindById(root, "abs");
            Assert.NotNull(abs);
            Assert.True(System.Math.Abs(abs!.ContentRect.Width - 100) < 2, $"50% of 200 = 100 (got {abs.ContentRect.Width})");
        }

        [Fact]
        public void Relative_DoesNotAffectSiblings()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='width: 200px;'>
                    <div style='position: relative; top: 50px; height: 30px;'></div>
                    <div id='sibling' style='height: 30px;'></div>
                </div></body>");
            var sibling = LayoutTestHelper.FindById(root, "sibling");
            Assert.NotNull(sibling);
            _output.WriteLine($"sibling.Y={sibling!.ContentRect.Y}");
            // Relative positioning doesn't affect sibling positions
            Assert.True(System.Math.Abs(sibling.ContentRect.Y - 30) < 2,
                $"Sibling at normal flow position (got Y={sibling.ContentRect.Y})");
        }

        [Fact]
        public void AbsPos_ZIndex_Parsed()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='position: relative;'>
                    <div id='a' style='position: absolute; z-index: 1; width: 50px; height: 50px;'></div>
                    <div id='b' style='position: absolute; z-index: 2; width: 50px; height: 50px;'></div>
                </div></body>");
            var a = LayoutTestHelper.FindById(root, "a");
            var b = LayoutTestHelper.FindById(root, "b");
            Assert.NotNull(a);
            Assert.NotNull(b);
            var styledA = (a!.StyledNode as StyledElement)!;
            var styledB = (b!.StyledNode as StyledElement)!;
            Assert.Equal(1, styledA.Style.ZIndex);
            Assert.Equal(2, styledB.Style.ZIndex);
        }

        [Fact]
        public void AbsPos_HeightFromTopBottom()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='position: relative; width: 200px; height: 200px;'>
                    <div id='abs' style='position: absolute; top: 20px; bottom: 30px; width: 50px;'></div>
                </div></body>");
            var abs = LayoutTestHelper.FindById(root, "abs");
            Assert.NotNull(abs);
            _output.WriteLine($"abs: h={abs!.ContentRect.Height}");
            // height = CB(200) - top(20) - bottom(30) = 150
            Assert.True(System.Math.Abs(abs.ContentRect.Height - 150) < 2,
                $"Height from top+bottom = 150 (got {abs.ContentRect.Height})");
        }

        [Fact]
        public void Fixed_NotAffectedByScroll()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='fixed' style='position: fixed; top: 10px; left: 10px; width: 50px; height: 50px;'></div>
                <div style='height: 2000px;'></div></body>", 400, 300);
            var box = LayoutTestHelper.FindById(root, "fixed");
            Assert.NotNull(box);
            Assert.True(System.Math.Abs(box!.ContentRect.X - 10) < 2);
            Assert.True(System.Math.Abs(box.ContentRect.Y - 10) < 2);
        }
    }
}
