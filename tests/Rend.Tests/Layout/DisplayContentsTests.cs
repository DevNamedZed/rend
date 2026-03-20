using Rend.Css;
using Rend.Style;
using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class DisplayContentsTests
    {
        private readonly ITestOutputHelper _output;
        public DisplayContentsTests(ITestOutputHelper output) { _output = output; }

        [Fact]
        public void DisplayContents_ChildrenVisibleButWrapperInvisible()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='width: 200px;'>
                    <div style='display: contents;'>
                        <div id='child' style='width: 100px; height: 50px;'></div>
                    </div>
                </div></body>");
            var child = LayoutTestHelper.FindById(root, "child");
            Assert.NotNull(child);
            _output.WriteLine($"child: {child!.ContentRect.Width}x{child.ContentRect.Height}");
            Assert.True(child.ContentRect.Width >= 99, $"Child should be visible (w={child.ContentRect.Width})");
            Assert.True(child.ContentRect.Height >= 49, $"Child should be visible (h={child.ContentRect.Height})");
        }

        [Fact]
        public void DisplayContents_InheritsParentStyles()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='color: red;'>
                    <div style='display: contents; color: blue;'>
                        <div id='child' style='width: 50px; height: 20px;'></div>
                    </div>
                </div></body>");
            var child = LayoutTestHelper.FindById(root, "child");
            Assert.NotNull(child);
            var styled = child!.StyledNode as StyledElement;
            Assert.NotNull(styled);
            _output.WriteLine($"color=({styled!.Style.Color.R},{styled.Style.Color.G},{styled.Style.Color.B})");
            // display:contents element's styles still inherit to children
            Assert.Equal(0, styled.Style.Color.R);
            Assert.Equal(0, styled.Style.Color.G);
            Assert.True(styled.Style.Color.B > 200, "Should inherit blue from display:contents parent");
        }

        [Fact]
        public void DisplayContents_InFlex_ChildrenBecomeFlexItems()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display: flex; width: 200px;'>
                    <div style='display: contents;'>
                        <div id='a' style='width: 50px; height: 30px;'></div>
                        <div id='b' style='width: 50px; height: 30px;'></div>
                    </div>
                </div></body>");
            var a = LayoutTestHelper.FindById(root, "a");
            var b = LayoutTestHelper.FindById(root, "b");
            Assert.NotNull(a);
            Assert.NotNull(b);
            _output.WriteLine($"a.X={a!.ContentRect.X} b.X={b!.ContentRect.X}");
            // Children of display:contents should become direct flex items
            Assert.True(b.ContentRect.X > a.ContentRect.X,
                $"B should be right of A in flex (a.X={a.ContentRect.X}, b.X={b.ContentRect.X})");
        }

        [Fact]
        public void DisplayContents_InGrid_ChildrenBecomeGridItems()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display: grid; grid-template-columns: 1fr 1fr; width: 200px;'>
                    <div style='display: contents;'>
                        <div id='a' style='height: 30px;'></div>
                        <div id='b' style='height: 30px;'></div>
                    </div>
                </div></body>");
            var a = LayoutTestHelper.FindById(root, "a");
            var b = LayoutTestHelper.FindById(root, "b");
            Assert.NotNull(a);
            Assert.NotNull(b);
            _output.WriteLine($"a.X={a!.ContentRect.X} b.X={b!.ContentRect.X}");
            Assert.True(b.ContentRect.X > a.ContentRect.X,
                $"B should be in second column (a.X={a.ContentRect.X}, b.X={b.ContentRect.X})");
        }

        [Fact]
        public void DisplayNone_ChildrenNotRendered()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='width: 200px;'>
                    <div style='display: none;'>
                        <div id='hidden' style='width: 100px; height: 50px;'></div>
                    </div>
                    <div id='visible' style='width: 100px; height: 50px;'></div>
                </div></body>");
            var hidden = LayoutTestHelper.FindById(root, "hidden");
            var visible = LayoutTestHelper.FindById(root, "visible");
            // hidden should not be in the layout tree
            Assert.Null(hidden);
            Assert.NotNull(visible);
            _output.WriteLine($"visible.Y={visible!.ContentRect.Y}");
            // visible should be at top since display:none takes no space
            Assert.True(visible.ContentRect.Y < 2, $"Visible should be at top (Y={visible.ContentRect.Y})");
        }
    }
}
