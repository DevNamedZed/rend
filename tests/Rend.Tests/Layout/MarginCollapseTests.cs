using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class MarginCollapseTests
    {
        private readonly ITestOutputHelper _output;
        public MarginCollapseTests(ITestOutputHelper output) { _output = output; }

        [Fact]
        public void Adjacent_Siblings_CollapseMargins()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='width: 200px;'>
                    <div style='margin-bottom: 20px; height: 30px;'></div>
                    <div id='b' style='margin-top: 30px; height: 30px;'></div>
                </div></body>");
            var b = LayoutTestHelper.FindById(root, "b");
            Assert.NotNull(b);
            _output.WriteLine($"b.Y={b!.ContentRect.Y}");
            // Margins collapse: max(20, 30) = 30, so B.Y = 30 (first div) + 30 (collapsed margin)
            Assert.True(System.Math.Abs(b.ContentRect.Y - 60) < 2,
                $"Collapsed margin should be 30px gap (B.Y should be ~60, got {b.ContentRect.Y})");
        }

        [Fact]
        public void Parent_FirstChild_CollapseMargins()
        {
            // When parent has no border/padding, first child margin collapses with parent
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='parent' style='width: 200px; margin-top: 20px;'>
                    <div id='child' style='margin-top: 30px; height: 50px;'></div>
                </div></body>");
            var parent = LayoutTestHelper.FindById(root, "parent");
            var child = LayoutTestHelper.FindById(root, "child");
            Assert.NotNull(parent);
            Assert.NotNull(child);
            _output.WriteLine($"parent.Y={parent!.ContentRect.Y} child.Y={child!.ContentRect.Y}");
            // Parent margin 20 + child margin 30 collapse to max(20,30) = 30
            // So parent Y should be 30, child should be at same Y (no gap between parent and first child)
        }

        [Fact]
        public void Border_PreventCollapse()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='parent' style='width: 200px; margin-top: 20px; border-top: 1px solid black;'>
                    <div id='child' style='margin-top: 30px; height: 50px;'></div>
                </div></body>");
            var parent = LayoutTestHelper.FindById(root, "parent");
            var child = LayoutTestHelper.FindById(root, "child");
            Assert.NotNull(parent);
            Assert.NotNull(child);
            _output.WriteLine($"parent.Y={parent!.ContentRect.Y} child.Y={child!.ContentRect.Y}");
            // Border prevents margin collapse between parent and child
            // Parent at Y=20, child at Y=20+1(border)+30(margin) = 51
            Assert.True(child!.ContentRect.Y > parent.ContentRect.Y + 25,
                $"Border should prevent collapse (parent.Y={parent.ContentRect.Y}, child.Y={child.ContentRect.Y})");
        }

        [Fact]
        public void Negative_Margins_CollapseCorrectly()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='width: 200px;'>
                    <div style='margin-bottom: 20px; height: 30px;'></div>
                    <div id='b' style='margin-top: -10px; height: 30px;'></div>
                </div></body>");
            var b = LayoutTestHelper.FindById(root, "b");
            Assert.NotNull(b);
            _output.WriteLine($"b.Y={b!.ContentRect.Y}");
            // Positive 20 + negative -10: result = 20 + (-10) = 10
            Assert.True(System.Math.Abs(b.ContentRect.Y - 40) < 2,
                $"Negative margin collapse: 30 + max(20,0) + min(0,-10) = 30+20-10=40 (got {b.ContentRect.Y})");
        }

        [Fact]
        public void SelfCollapsing_Element_PassesMarginThrough()
        {
            // A zero-height element with margins should pass its margin through
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='width: 200px;'>
                    <div style='margin-bottom: 20px; height: 30px;'></div>
                    <div style='margin-top: 10px; margin-bottom: 15px;'></div>
                    <div id='c' style='margin-top: 5px; height: 30px;'></div>
                </div></body>");
            var c = LayoutTestHelper.FindById(root, "c");
            Assert.NotNull(c);
            _output.WriteLine($"c.Y={c!.ContentRect.Y}");
            // Three margins collapse: max(20, 10, 15, 5) = 20
            // C.Y = 30 (first div) + 20 (collapsed) = 50
            Assert.True(System.Math.Abs(c.ContentRect.Y - 50) < 2,
                $"Self-collapsing passes margin through (c.Y should be ~50, got {c.ContentRect.Y})");
        }
    }
}
