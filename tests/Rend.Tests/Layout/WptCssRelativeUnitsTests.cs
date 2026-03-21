using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    /// <summary>
    /// Tests for CSS relative length units: em, rem, vw, vh, vmin, vmax, ch, ex.
    /// Validates that each unit resolves to the correct pixel value in layout.
    /// </summary>
    public class WptCssRelativeUnitsTests
    {
        private readonly ITestOutputHelper _output;

        public WptCssRelativeUnitsTests(ITestOutputHelper output)
        {
            _output = output;
        }

        // [CSS-VALUES §6.1] em: relative to parent font-size (default 16px)
        [Fact]
        public void Em_RelativeToParentFontSize()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='font-size:16px'><div id='t' style='width:2em;height:10px'></div></div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"width={box.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 32) < 2,
                $"2em of 16px parent should be 32px (got {box.ContentRect.Width})");
        }

        // [CSS-VALUES §6.1] em: with explicit font-size on element
        [Fact]
        public void Em_WithFontSize20px()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='font-size:20px'><div id='t' style='width:2em;height:10px'></div></div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"width={box.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 40) < 2,
                $"2em of 20px should be 40px (got {box.ContentRect.Width})");
        }

        // [CSS-VALUES §6.1] rem: relative to root (html) font-size
        [Fact]
        public void Rem_RelativeToRootFontSize()
        {
            var root = LayoutTestHelper.Layout(
                @"<html style='font-size:10px'><body style='margin:0'><div id='t' style='width:5rem;height:10px'></div></body></html>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"width={box.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 50) < 2,
                $"5rem of 10px root should be 50px (got {box.ContentRect.Width})");
        }

        // [CSS-VALUES §6.1] rem: ignores parent font-size, uses root only
        [Fact]
        public void Rem_IgnoresParentFontSize()
        {
            var root = LayoutTestHelper.Layout(
                @"<html style='font-size:12px'><body style='margin:0'><div style='font-size:40px'><div id='t' style='width:3rem;height:10px'></div></div></body></html>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"width={box.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 36) < 2,
                $"3rem should use root 12px, not parent 40px (got {box.ContentRect.Width})");
        }

        // [CSS-VALUES §6.3] vw: 1vw = 1% of viewport width
        [Fact]
        public void Vw_50vwIn400pxViewport()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div id='t' style='width:50vw;height:10px'></div></body>", 400, 300);
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"width={box.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 200) < 2,
                $"50vw of 400px viewport should be 200px (got {box.ContentRect.Width})");
        }

        // [CSS-VALUES §6.3] vh: 1vh = 1% of viewport height
        [Fact]
        public void Vh_50vhIn300pxViewport()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div id='t' style='width:50vh;height:10px'></div></body>", 400, 300);
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"width={box.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 150) < 2,
                $"50vh of 300px viewport should be 150px (got {box.ContentRect.Width})");
        }

        // [CSS-VALUES §6.3] vmin: smaller of vw and vh
        [Fact]
        public void Vmin_UsesSmaller()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div id='t' style='width:50vmin;height:10px'></div></body>", 400, 300);
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"width={box.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 150) < 2,
                $"50vmin of 400x300 should use 300 (smaller), giving 150px (got {box.ContentRect.Width})");
        }

        // [CSS-VALUES §6.3] vmax: larger of vw and vh
        [Fact]
        public void Vmax_UsesLarger()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div id='t' style='width:50vmax;height:10px'></div></body>", 400, 300);
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"width={box.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 200) < 2,
                $"50vmax of 400x300 should use 400 (larger), giving 200px (got {box.ContentRect.Width})");
        }

        // [CSS-VALUES §6.1] em in padding
        [Fact]
        public void Em_InPadding()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='font-size:10px'><div id='t' style='padding:2em;width:50px;height:50px'></div></div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"paddingTop={box.PaddingTop} paddingLeft={box.PaddingLeft}");
            Assert.True(System.Math.Abs(box.PaddingTop - 20) < 2,
                $"2em padding-top with 10px font should be 20px (got {box.PaddingTop})");
            Assert.True(System.Math.Abs(box.PaddingLeft - 20) < 2,
                $"2em padding-left with 10px font should be 20px (got {box.PaddingLeft})");
        }

        // [CSS-VALUES §6.1] em in margin
        [Fact]
        public void Em_InMargin()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0;overflow:hidden'><div style='font-size:10px'><div id='t' style='margin:1.5em;width:50px;height:50px'></div></div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"marginTop={box.MarginTop} marginLeft={box.MarginLeft}");
            Assert.True(System.Math.Abs(box.MarginTop - 15) < 2,
                $"1.5em margin-top with 10px font should be 15px (got {box.MarginTop})");
            Assert.True(System.Math.Abs(box.MarginLeft - 15) < 2,
                $"1.5em margin-left with 10px font should be 15px (got {box.MarginLeft})");
        }

        // [CSS-VALUES §6.1] em in border-width
        [Fact]
        public void Em_InBorderWidth()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='font-size:20px'><div id='t' style='border:0.5em solid black;width:100px;height:100px'></div></div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"borderTopWidth={box.BorderTopWidth}");
            Assert.True(System.Math.Abs(box.BorderTopWidth - 10) < 2,
                $"0.5em border with 20px font should be 10px (got {box.BorderTopWidth})");
        }

        // [CSS-VALUES §6.1] rem in width
        [Fact]
        public void Rem_InWidth()
        {
            var root = LayoutTestHelper.Layout(
                @"<html style='font-size:20px'><body style='margin:0'><div id='t' style='width:4rem;height:10px'></div></body></html>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"width={box.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 80) < 2,
                $"4rem of 20px root should be 80px (got {box.ContentRect.Width})");
        }

        // [CSS-VALUES §6.3] vw used in height property
        [Fact]
        public void Vw_InHeight()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div id='t' style='width:50px;height:25vw'></div></body>", 400, 300);
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"height={box.ContentRect.Height}");
            Assert.True(System.Math.Abs(box.ContentRect.Height - 100) < 2,
                $"25vw of 400px viewport in height should be 100px (got {box.ContentRect.Height})");
        }

        // [CSS-VALUES §6.1] nested em: em of em compounds through font-size inheritance
        [Fact]
        public void Em_NestedDoubleCompounding()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='font-size:10px'><div style='font-size:2em'><div id='t' style='width:3em;height:10px'></div></div></div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"width={box.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 60) < 2,
                $"10px -> 2em=20px -> 3em=60px (got {box.ContentRect.Width})");
        }

        // [CSS-VALUES §6.1] em with font-size inheritance from ancestor chain
        [Fact]
        public void Em_FontSizeInheritance()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='font-size:24px'><div><div id='t' style='width:1em;height:10px'></div></div></div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"width={box.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 24) < 2,
                $"1em should inherit 24px font-size through intermediate div (got {box.ContentRect.Width})");
        }

        // [CSS-VALUES §8.1] calc() mixing em and px
        [Fact]
        public void Calc_EmPlusPx()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='font-size:20px'><div id='t' style='width:calc(2em + 10px);height:10px'></div></div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"width={box.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 50) < 2,
                $"calc(2em + 10px) with 20px font = 40+10 = 50px (got {box.ContentRect.Width})");
        }

        // [CSS-VALUES §6.3] vw with a non-default viewport width
        [Fact]
        public void Vw_DifferentViewportSize()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div id='t' style='width:25vw;height:10px'></div></body>", 800, 600);
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"width={box.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 200) < 2,
                $"25vw of 800px viewport should be 200px (got {box.ContentRect.Width})");
        }

        // [CSS-VALUES §6.3] vh with a non-default viewport height
        [Fact]
        public void Vh_DifferentViewportSize()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div id='t' style='width:10vh;height:10px'></div></body>", 800, 600);
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"width={box.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 60) < 2,
                $"10vh of 600px viewport should be 60px (got {box.ContentRect.Width})");
        }

        // [CSS-VALUES §6.3] 100vw fills entire viewport width
        [Fact]
        public void Vw_100vwFillsViewport()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div id='t' style='width:100vw;height:10px'></div></body>", 400, 300);
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"width={box.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 400) < 2,
                $"100vw should fill 400px viewport (got {box.ContentRect.Width})");
        }

        // [CSS-VALUES §6.3] 100vh fills entire viewport height
        [Fact]
        public void Vh_100vhFillsViewport()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div id='t' style='width:10px;height:100vh'></div></body>", 400, 300);
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"height={box.ContentRect.Height}");
            Assert.True(System.Math.Abs(box.ContentRect.Height - 300) < 2,
                $"100vh should fill 300px viewport (got {box.ContentRect.Height})");
        }

        // [CSS-VALUES §6.3] vmin with square viewport (both equal)
        [Fact]
        public void Vmin_SquareViewport()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div id='t' style='width:50vmin;height:10px'></div></body>", 500, 500);
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"width={box.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 250) < 2,
                $"50vmin of 500x500 should be 250px (got {box.ContentRect.Width})");
        }

        // [CSS-VALUES §6.3] vmax with square viewport (both equal)
        [Fact]
        public void Vmax_SquareViewport()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div id='t' style='width:50vmax;height:10px'></div></body>", 500, 500);
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"width={box.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 250) < 2,
                $"50vmax of 500x500 should be 250px (got {box.ContentRect.Width})");
        }

        // [CSS-VALUES §6.1] em in font-size property itself resolves against parent
        [Fact]
        public void Em_InFontSizeProperty()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='font-size:16px'><div style='font-size:1.5em'><div id='t' style='width:1em;height:10px'></div></div></div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"width={box.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 24) < 2,
                $"16px * 1.5em = 24px, then 1em = 24px (got {box.ContentRect.Width})");
        }

        // [CSS-VALUES §6.1] rem in height
        [Fact]
        public void Rem_InHeight()
        {
            var root = LayoutTestHelper.Layout(
                @"<html style='font-size:15px'><body style='margin:0'><div id='t' style='width:10px;height:2rem'></div></body></html>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"height={box.ContentRect.Height}");
            Assert.True(System.Math.Abs(box.ContentRect.Height - 30) < 2,
                $"2rem of 15px root should be 30px (got {box.ContentRect.Height})");
        }

        // [CSS-VALUES §6.1] rem in margin
        [Fact]
        public void Rem_InMargin()
        {
            var root = LayoutTestHelper.Layout(
                @"<html style='font-size:10px'><body style='margin:0;overflow:hidden'><div id='t' style='margin-left:2rem;width:50px;height:10px'></div></body></html>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"marginLeft={box.MarginLeft}");
            Assert.True(System.Math.Abs(box.MarginLeft - 20) < 2,
                $"2rem margin-left with 10px root should be 20px (got {box.MarginLeft})");
        }

        // [CSS-VALUES §6.3] vw in padding
        [Fact]
        public void Vw_InPadding()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div id='t' style='padding-left:10vw;width:50px;height:10px'></div></body>", 400, 300);
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"paddingLeft={box.PaddingLeft}");
            Assert.True(System.Math.Abs(box.PaddingLeft - 40) < 2,
                $"10vw padding-left with 400px viewport should be 40px (got {box.PaddingLeft})");
        }

        // [CSS-VALUES §8.1] calc() mixing rem and px
        [Fact]
        public void Calc_RemMinusPx()
        {
            var root = LayoutTestHelper.Layout(
                @"<html style='font-size:20px'><body style='margin:0'><div id='t' style='width:calc(5rem - 10px);height:10px'></div></body></html>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"width={box.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 90) < 2,
                $"calc(5rem - 10px) with 20px root = 100-10 = 90px (got {box.ContentRect.Width})");
        }

        // [CSS-VALUES §8.1] calc() mixing vw and px
        [Fact]
        public void Calc_VwPlusPx()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div id='t' style='width:calc(50vw + 50px);height:10px'></div></body>", 400, 300);
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"width={box.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 250) < 2,
                $"calc(50vw + 50px) with 400px viewport = 200+50 = 250px (got {box.ContentRect.Width})");
        }

        // [CSS-VALUES §6.3] vmin when height is larger than width
        [Fact]
        public void Vmin_WhenHeightIsLarger()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div id='t' style='width:50vmin;height:10px'></div></body>", 300, 500);
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"width={box.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 150) < 2,
                $"50vmin of 300x500 should use 300 (width is smaller), giving 150px (got {box.ContentRect.Width})");
        }

        // [CSS-VALUES §6.3] vmax when height is larger than width
        [Fact]
        public void Vmax_WhenHeightIsLarger()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div id='t' style='width:50vmax;height:10px'></div></body>", 300, 500);
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"width={box.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 250) < 2,
                $"50vmax of 300x500 should use 500 (height is larger), giving 250px (got {box.ContentRect.Width})");
        }
    }
}
