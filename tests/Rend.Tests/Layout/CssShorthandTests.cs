using Rend.Css;
using Rend.Style;
using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class CssShorthandTests
    {
        private readonly ITestOutputHelper _output;
        public CssShorthandTests(ITestOutputHelper output) { _output = output; }

        [Fact]
        public void Flex_Shorthand_1()
        {
            // flex: 1 = flex-grow:1, flex-shrink:1, flex-basis:0
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display: flex; width: 300px;'>
                    <div id='a' style='flex: 1; height: 30px;'></div>
                    <div id='b' style='flex: 2; height: 30px;'></div>
                </div></body>");
            var a = LayoutTestHelper.FindById(root, "a");
            var b = LayoutTestHelper.FindById(root, "b");
            Assert.NotNull(a);
            Assert.NotNull(b);
            _output.WriteLine($"a={a!.ContentRect.Width} b={b!.ContentRect.Width}");
            Assert.True(System.Math.Abs(a.ContentRect.Width - 100) < 2, $"flex:1 = 100px (got {a.ContentRect.Width})");
            Assert.True(System.Math.Abs(b.ContentRect.Width - 200) < 2, $"flex:2 = 200px (got {b.ContentRect.Width})");
        }

        [Fact]
        public void Flex_Shorthand_None()
        {
            // flex: none = flex: 0 0 auto
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display: flex; width: 300px;'>
                    <div id='item' style='flex: none; width: 80px; height: 30px;'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item");
            Assert.NotNull(item);
            _output.WriteLine($"width={item!.ContentRect.Width}");
            Assert.True(System.Math.Abs(item.ContentRect.Width - 80) < 2, $"flex:none keeps width (got {item.ContentRect.Width})");
        }

        [Fact]
        public void Flex_Shorthand_Auto()
        {
            // flex: auto = flex: 1 1 auto
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display: flex; width: 300px;'>
                    <div id='item' style='flex: auto; width: 100px; height: 30px;'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item");
            Assert.NotNull(item);
            _output.WriteLine($"width={item!.ContentRect.Width}");
            // flex:auto with grow:1 fills remaining space
            Assert.True(item.ContentRect.Width >= 299, $"flex:auto fills space (got {item.ContentRect.Width})");
        }

        [Fact]
        public void Gap_Shorthand()
        {
            // gap: 10px 20px = row-gap:10px column-gap:20px
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display: grid; grid-template-columns: 1fr 1fr; gap: 10px 20px; width: 220px;'>
                    <div id='a' style='height: 30px;'></div>
                    <div id='b' style='height: 30px;'></div>
                    <div id='c' style='height: 30px;'></div>
                    <div id='d' style='height: 30px;'></div>
                </div></body>");
            var a = LayoutTestHelper.FindById(root, "a");
            var b = LayoutTestHelper.FindById(root, "b");
            var c = LayoutTestHelper.FindById(root, "c");
            Assert.NotNull(a);
            Assert.NotNull(b);
            Assert.NotNull(c);
            float colGap = b!.ContentRect.X - (a!.ContentRect.X + a.ContentRect.Width);
            float rowGap = c!.ContentRect.Y - (a.ContentRect.Y + a.ContentRect.Height);
            _output.WriteLine($"col-gap={colGap} row-gap={rowGap}");
            Assert.True(System.Math.Abs(colGap - 20) < 2, $"column-gap should be 20 (got {colGap})");
            Assert.True(System.Math.Abs(rowGap - 10) < 2, $"row-gap should be 10 (got {rowGap})");
        }

        [Fact]
        public void Border_Shorthand()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='test' style='border: 3px solid red; width: 100px; height: 50px;'></div></body>");
            var box = LayoutTestHelper.FindById(root, "test");
            Assert.NotNull(box);
            Assert.Equal(3, box!.BorderTopWidth);
            Assert.Equal(3, box.BorderRightWidth);
            Assert.Equal(3, box.BorderBottomWidth);
            Assert.Equal(3, box.BorderLeftWidth);
        }

        [Fact]
        public void Margin_Shorthand_TwoValues()
        {
            // margin: 10px 20px = top/bottom:10, left/right:20
            // Use overflow:hidden on parent to prevent margin collapse
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0; overflow:hidden;'>
                <div id='test' style='margin: 10px 20px; width: 100px; height: 50px;'></div></body>");
            var box = LayoutTestHelper.FindById(root, "test");
            Assert.NotNull(box);
            _output.WriteLine($"margins: T={box!.MarginTop} R={box.MarginRight} B={box.MarginBottom} L={box.MarginLeft}");
            Assert.Equal(10, box.StyledNode!.Style.MarginTop);
            Assert.Equal(20, box.StyledNode!.Style.MarginRight);
            Assert.Equal(10, box.StyledNode!.Style.MarginBottom);
            Assert.Equal(20, box.StyledNode!.Style.MarginLeft);
        }
        [Fact]
        public void Flex_Shorthand_TwoNumbers_BasisIsZero()
        {
            // flex: 0 0 → flex-grow:0 flex-shrink:0 flex-basis:0 (NOT auto)
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display: flex; width: 200px;'>
                    <div id='item' style='flex: 0 0; width: 80px; height: 30px;'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item");
            Assert.NotNull(item);
            _output.WriteLine($"width={item!.ContentRect.Width}");
            // flex-basis: 0 overrides width: 80px → item should be 0 width
            Assert.True(item.ContentRect.Width < 1,
                $"flex: 0 0 → basis:0 overrides width (got {item.ContentRect.Width})");
        }

        [Fact]
        public void Flex_Shorthand_WithExplicitAuto_KeepsBasis()
        {
            // flex: 0 0 auto → flex-basis: auto (explicit, keeps width)
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display: flex; width: 200px;'>
                    <div id='item' style='flex: 0 0 auto; width: 80px; height: 30px;'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item");
            Assert.NotNull(item);
            _output.WriteLine($"width={item!.ContentRect.Width}");
            // flex-basis: auto → use width: 80px
            Assert.True(System.Math.Abs(item.ContentRect.Width - 80) < 2,
                $"flex: 0 0 auto → basis:auto uses width (got {item.ContentRect.Width})");
        }
    }
}

