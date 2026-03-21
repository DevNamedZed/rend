using Rend.Css;
using Rend.Style;
using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    /// <spec>CSS-VALUES §6.1 https://drafts.csswg.org/css-values/#font-relative-lengths</spec>
    public class WptCssEmFontSizeTests
    {
        private readonly ITestOutputHelper _output;
        public WptCssEmFontSizeTests(ITestOutputHelper output) { _output = output; }

        // [CSS-VALUES §6.1] 1em equals inherited font-size (default 16px)
        [Fact]
        public void Em_DefaultFontSize_1em_Equals_16px()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div id='t' style='width:1em;height:10px'></div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"width={box.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 16) < 1);
        }

        // [CSS-VALUES §6.1] 2em equals 32px at default font-size
        [Fact]
        public void Em_DefaultFontSize_2em_Equals_32px()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div id='t' style='width:2em;height:10px'></div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"width={box.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 32) < 1);
        }

        // [CSS-VALUES §6.1] 0.5em equals 8px at default font-size
        [Fact]
        public void Em_DefaultFontSize_Half_Equals_8px()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div id='t' style='width:0.5em;height:10px'></div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"width={box.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 8) < 1);
        }

        // [CSS-VALUES §6.1] em resolves relative to parent font-size 20px
        [Fact]
        public void Em_ParentFontSize20_1em_Equals_20px()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='font-size:20px'><div id='t' style='width:1em;height:10px'></div></div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"width={box.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 20) < 1);
        }

        // [CSS-VALUES §6.1] em resolves relative to parent font-size 24px
        [Fact]
        public void Em_ParentFontSize24_2em_Equals_48px()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='font-size:24px'><div id='t' style='width:2em;height:10px'></div></div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"width={box.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 48) < 1);
        }

        // [CSS-VALUES §6.1] nested em: outer 2em (32px), inner 1em resolves to 32px
        [Fact]
        public void Em_Nested_EmOfEm_Compounds()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='font-size:2em'><div id='t' style='width:1em;height:10px'></div></div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"width={box.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 32) < 1);
        }

        // [CSS-VALUES §6.1] em on width property
        [Fact]
        public void Em_OnWidth_ResolvesCorrectly()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='font-size:20px'><div id='t' style='width:5em;height:10px'></div></div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"width={box.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 100) < 1);
        }

        // [CSS-VALUES §6.1] em on height property
        [Fact]
        public void Em_OnHeight_ResolvesCorrectly()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='font-size:20px'><div id='t' style='width:10px;height:3em'></div></div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"height={box.ContentRect.Height}");
            Assert.True(System.Math.Abs(box.ContentRect.Height - 60) < 1);
        }

        // [CSS-VALUES §6.1] em on padding property
        [Fact]
        public void Em_OnPadding_ResolvesCorrectly()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='font-size:20px'><div id='t' style='padding:1em;width:10px;height:10px'></div></div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"paddingTop={box.PaddingTop} paddingLeft={box.PaddingLeft}");
            Assert.True(System.Math.Abs(box.PaddingTop - 20) < 1);
            Assert.True(System.Math.Abs(box.PaddingRight - 20) < 1);
            Assert.True(System.Math.Abs(box.PaddingBottom - 20) < 1);
            Assert.True(System.Math.Abs(box.PaddingLeft - 20) < 1);
        }

        // [CSS-VALUES §6.1] em on margin property
        [Fact]
        public void Em_OnMargin_ResolvesCorrectly()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0;overflow:hidden'><div style='font-size:20px'><div id='t' style='margin:1.5em;width:10px;height:10px'></div></div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"marginTop={box.MarginTop} marginLeft={box.MarginLeft}");
            Assert.True(System.Math.Abs(box.MarginTop - 30) < 1);
            Assert.True(System.Math.Abs(box.MarginLeft - 30) < 1);
        }

        // [CSS-VALUES §6.1] em on border-width property
        [Fact]
        public void Em_OnBorderWidth_ResolvesCorrectly()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='font-size:20px'><div id='t' style='border:0.5em solid black;width:10px;height:10px'></div></div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"borderTop={box.BorderTopWidth}");
            Assert.True(System.Math.Abs(box.BorderTopWidth - 10) < 1);
            Assert.True(System.Math.Abs(box.BorderRightWidth - 10) < 1);
            Assert.True(System.Math.Abs(box.BorderBottomWidth - 10) < 1);
            Assert.True(System.Math.Abs(box.BorderLeftWidth - 10) < 1);
        }

        // [CSS-FLEXBOX §9.2] em on flex-basis
        [Fact]
        public void Em_OnFlexBasis_ResolvesCorrectly()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display:flex;font-size:20px;width:300px'>
                    <div id='t' style='flex:0 0 5em;height:10px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"width={box.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 100) < 2);
        }

        // [CSS-GRID §7.2] em on grid item width inside grid
        [Fact]
        public void Em_OnGridItemWidth_ResolvesCorrectly()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display:grid;font-size:20px;grid-template-columns:200px 200px;width:400px'>
                    <div id='a' style='width:5em;height:10px'></div>
                    <div id='b' style='width:3em;height:10px'></div>
                </div></body>");
            var boxA = LayoutTestHelper.FindById(root, "a")!;
            var boxB = LayoutTestHelper.FindById(root, "b")!;
            _output.WriteLine($"a.width={boxA.ContentRect.Width} b.width={boxB.ContentRect.Width}");
            Assert.True(System.Math.Abs(boxA.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(boxB.ContentRect.Width - 60) < 2);
        }

        // [CSS-CASCADE §6.4] font-size:inherit preserves parent em base
        [Fact]
        public void Em_WithFontSizeInherit_UsesParentSize()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='font-size:24px'><div style='font-size:inherit'><div id='t' style='width:1em;height:10px'></div></div></div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"width={box.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 24) < 1);
        }

        // [CSS-VALUES §6.1] font-size:2em compounds: parent 16px, child 2em=32px, 1em width=32px
        [Fact]
        public void Em_FontSize2em_CompoundsOnParent()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='font-size:16px'><div style='font-size:2em'><div id='t' style='width:1em;height:10px'></div></div></div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"width={box.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 32) < 1);
        }

        // [CSS-VALUES §6.1] triple nesting: 16px -> 2em(32px) -> 2em(64px), 1em width=64px
        [Fact]
        public void Em_TripleNesting_CompoundsCorrectly()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='font-size:16px'><div style='font-size:2em'><div style='font-size:2em'><div id='t' style='width:1em;height:10px'></div></div></div></div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"width={box.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 64) < 1);
        }

        // [CSS-VALUES §6.1] rem ignores parent font-size, uses root
        [Fact]
        public void Rem_IgnoresParentFontSize()
        {
            var root = LayoutTestHelper.Layout(
                "<html style='font-size:16px'><body style='margin:0'><div style='font-size:40px'><div id='t' style='width:2rem;height:10px'></div></div></body></html>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"width={box.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 32) < 1);
        }

        // [CSS-VALUES §6.1] rem uses root element font-size
        [Fact]
        public void Rem_UsesRootFontSize()
        {
            var root = LayoutTestHelper.Layout(
                "<html style='font-size:20px'><body style='margin:0'><div id='t' style='width:3rem;height:10px'></div></body></html>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"width={box.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 60) < 1);
        }

        // [CSS-VALUES §6.1] rem on width property
        [Fact]
        public void Rem_OnWidth_ResolvesFromRoot()
        {
            var root = LayoutTestHelper.Layout(
                "<html style='font-size:10px'><body style='margin:0'><div style='font-size:50px'><div id='t' style='width:10rem;height:10px'></div></div></body></html>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"width={box.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 100) < 1);
        }

        // [CSS-VALUES §6.1] rem on padding
        [Fact]
        public void Rem_OnPadding_ResolvesFromRoot()
        {
            var root = LayoutTestHelper.Layout(
                "<html style='font-size:12px'><body style='margin:0'><div style='font-size:40px'><div id='t' style='padding:2rem;width:10px;height:10px'></div></div></body></html>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"paddingTop={box.PaddingTop}");
            Assert.True(System.Math.Abs(box.PaddingTop - 24) < 1);
            Assert.True(System.Math.Abs(box.PaddingLeft - 24) < 1);
        }

        // [CSS-VALUES §6.1] rem on margin
        [Fact]
        public void Rem_OnMargin_ResolvesFromRoot()
        {
            var root = LayoutTestHelper.Layout(
                "<html style='font-size:10px'><body style='margin:0;overflow:hidden'><div style='font-size:50px'><div id='t' style='margin:3rem;width:10px;height:10px'></div></div></body></html>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"marginTop={box.MarginTop} marginLeft={box.MarginLeft}");
            Assert.True(System.Math.Abs(box.MarginTop - 30) < 1);
            Assert.True(System.Math.Abs(box.MarginLeft - 30) < 1);
        }

        // [CSS-VALUES §8.1] calc() mixing em and px
        [Fact]
        public void Calc_EmPlusPx_ResolvesCorrectly()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='font-size:20px'><div id='t' style='width:calc(2em + 10px);height:10px'></div></div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"width={box.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 50) < 1);
        }

        // [CSS-VALUES §6.1] em with different viewport width (em is font-relative, not viewport-relative)
        [Fact]
        public void Em_UnaffectedByViewportWidth()
        {
            var rootNarrow = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='font-size:20px'><div id='t' style='width:2em;height:10px'></div></div></body>",
                200, 200);
            var rootWide = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='font-size:20px'><div id='t' style='width:2em;height:10px'></div></div></body>",
                800, 600);
            var narrowBox = LayoutTestHelper.FindById(rootNarrow, "t")!;
            var wideBox = LayoutTestHelper.FindById(rootWide, "t")!;
            _output.WriteLine($"narrow={narrowBox.ContentRect.Width} wide={wideBox.ContentRect.Width}");
            Assert.True(System.Math.Abs(narrowBox.ContentRect.Width - 40) < 1);
            Assert.True(System.Math.Abs(wideBox.ContentRect.Width - 40) < 1);
        }

        // [CSS-VALUES §6.1] 0em always resolves to 0px
        [Fact]
        public void Em_Zero_Equals_ZeroPx()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='font-size:20px'><div id='t' style='width:0em;height:10px'></div></div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"width={box.ContentRect.Width}");
            Assert.True(box.ContentRect.Width < 1);
        }

        // [CSS-VALUES §6.1] em on font-size resolves against PARENT font-size per CSS spec
        [Fact]
        public void Em_OnFontSize_ResolvesAgainstParent()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='font-size:10px'><div style='font-size:3em'><div id='t' style='width:1em;height:10px'></div></div></div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"width={box.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 30) < 1);
        }

        // [CSS-VALUES §6.1] em on multiple properties simultaneously
        [Fact]
        public void Em_OnMultipleProperties_AllResolveCorrectly()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0;overflow:hidden'><div style='font-size:10px'><div id='t' style='width:10em;height:5em;padding:1em;margin:2em'></div></div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"w={box.ContentRect.Width} h={box.ContentRect.Height} pad={box.PaddingTop} mar={box.MarginTop}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 100) < 1);
            Assert.True(System.Math.Abs(box.ContentRect.Height - 50) < 1);
            Assert.True(System.Math.Abs(box.PaddingTop - 10) < 1);
            Assert.True(System.Math.Abs(box.MarginTop - 20) < 1);
        }

        // [CSS-VALUES §6.1] rem with default root font-size (16px)
        [Fact]
        public void Rem_DefaultRootFontSize_1rem_Equals_16px()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='font-size:50px'><div id='t' style='width:1rem;height:10px'></div></div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"width={box.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 16) < 1);
        }

        // [CSS-VALUES §6.1] deeply nested em does not lose precision
        [Fact]
        public void Em_DeeplyNested_MaintainsPrecision()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='font-size:10px'>
                    <div style='font-size:2em'>
                        <div style='font-size:1.5em'>
                            <div id='t' style='width:1em;height:10px'></div>
                        </div>
                    </div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"width={box.ContentRect.Width}");
            // 10px * 2 * 1.5 = 30px
            Assert.True(System.Math.Abs(box.ContentRect.Width - 30) < 1);
        }
    }
}
