using Rend.Css;
using Rend.Style;
using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class WptFontsConformanceTests
    {
        private readonly ITestOutputHelper _output;
        public WptFontsConformanceTests(ITestOutputHelper output) { _output = output; }

        // font-weight values
        [Fact] public void FontWeight_Normal() { AssertFontWeight("normal", 400); }
        [Fact] public void FontWeight_Bold() { AssertFontWeight("bold", 700); }
        [Fact] public void FontWeight_100() { AssertFontWeight("100", 100); }
        [Fact] public void FontWeight_200() { AssertFontWeight("200", 200); }
        [Fact] public void FontWeight_300() { AssertFontWeight("300", 300); }
        [Fact] public void FontWeight_400() { AssertFontWeight("400", 400); }
        [Fact] public void FontWeight_500() { AssertFontWeight("500", 500); }
        [Fact] public void FontWeight_600() { AssertFontWeight("600", 600); }
        [Fact] public void FontWeight_700() { AssertFontWeight("700", 700); }
        [Fact] public void FontWeight_800() { AssertFontWeight("800", 800); }
        [Fact] public void FontWeight_900() { AssertFontWeight("900", 900); }

        // font-style values
        [Fact] public void FontStyle_Normal() { AssertProp("font-style:normal", s => Assert.Equal(CssFontStyle.Normal, s.FontStyle)); }
        [Fact] public void FontStyle_Italic() { AssertProp("font-style:italic", s => Assert.Equal(CssFontStyle.Italic, s.FontStyle)); }
        [Fact] public void FontStyle_Oblique() { AssertProp("font-style:oblique", s => Assert.Equal(CssFontStyle.Oblique, s.FontStyle)); }

        // font-size values
        [Fact] public void FontSize_16px() { AssertFontSize("16px", 16); }
        [Fact] public void FontSize_20px() { AssertFontSize("20px", 20); }
        [Fact] public void FontSize_1_5em() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='font-size:16px'><div id='t' style='font-size:1.5em;width:10px;height:10px'>x</div></div></body>");
            Assert.True(System.Math.Abs(((LayoutTestHelper.FindById(r, "t")!.StyledNode as StyledElement)!).Style.FontSize - 24) < 1);
        }
        [Fact] public void FontSize_150Percent() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='font-size:20px'><div id='t' style='font-size:150%;width:10px;height:10px'>x</div></div></body>");
            Assert.True(System.Math.Abs(((LayoutTestHelper.FindById(r, "t")!.StyledNode as StyledElement)!).Style.FontSize - 30) < 1);
        }

        // font-weight inherits
        [Fact]
        public void FontWeight_Inherits()
        {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='font-weight:700'><div id='t' style='width:10px;height:10px'>x</div></div></body>");
            Assert.Equal(700, ((LayoutTestHelper.FindById(r, "t")!.StyledNode as StyledElement)!).Style.FontWeight);
        }

        // font-size inherits
        [Fact]
        public void FontSize_Inherits()
        {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='font-size:24px'><div id='t' style='width:10px;height:10px'>x</div></div></body>");
            Assert.True(System.Math.Abs(((LayoutTestHelper.FindById(r, "t")!.StyledNode as StyledElement)!).Style.FontSize - 24) < 1);
        }

        // font-style inherits
        [Fact]
        public void FontStyle_Inherits()
        {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='font-style:italic'><div id='t' style='width:10px;height:10px'>x</div></div></body>");
            Assert.Equal(CssFontStyle.Italic, ((LayoutTestHelper.FindById(r, "t")!.StyledNode as StyledElement)!).Style.FontStyle);
        }

        // font shorthand
        [Fact]
        public void FontShorthand()
        {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='t' style='font:bold 20px Arial;width:10px;height:10px'>x</div></body>");
            var s = (LayoutTestHelper.FindById(r, "t")!.StyledNode as StyledElement)!;
            Assert.Equal(700, s.Style.FontWeight);
            Assert.True(System.Math.Abs(s.Style.FontSize - 20) < 1);
        }

        // font shorthand with italic
        [Fact]
        public void FontShorthand_Italic()
        {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='t' style='font:italic bold 18px Arial;width:10px;height:10px'>x</div></body>");
            var s = (LayoutTestHelper.FindById(r, "t")!.StyledNode as StyledElement)!;
            Assert.Equal(CssFontStyle.Italic, s.Style.FontStyle);
            Assert.Equal(700, s.Style.FontWeight);
        }

        // em unit resolves against element's own font-size
        [Fact]
        public void Em_OwnFontSize()
        {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='font-size:20px'><div id='t' style='width:5em;height:10px'>x</div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Width - 100) < 2);
        }

        // rem resolves against root font-size
        [Fact]
        public void Rem_RootFontSize()
        {
            var r = LayoutTestHelper.Layout("<html style='font-size:20px'><body style='margin:0'><div style='font-size:10px'><div id='t' style='width:5rem;height:10px'>x</div></div></body></html>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Width - 100) < 2);
        }

        // HTML elements with default font-weight
        [Fact] public void Strong_Bold() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><strong id='t' style='display:block'>B</strong></body>");
            Assert.Equal(700, ((LayoutTestHelper.FindById(r, "t")!.StyledNode as StyledElement)!).Style.FontWeight);
        }

        [Fact] public void H1_Bold() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><h1 id='t'>H</h1></body>");
            Assert.Equal(700, ((LayoutTestHelper.FindById(r, "t")!.StyledNode as StyledElement)!).Style.FontWeight);
        }

        [Fact] public void Em_Italic() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><em id='t' style='display:block'>I</em></body>");
            Assert.Equal(CssFontStyle.Italic, ((LayoutTestHelper.FindById(r, "t")!.StyledNode as StyledElement)!).Style.FontStyle);
        }

        [Fact] public void H1_LargerFont() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><h1 id='t'>H</h1></body>");
            Assert.True(((LayoutTestHelper.FindById(r, "t")!.StyledNode as StyledElement)!).Style.FontSize > 20);
        }

        private void AssertFontWeight(string value, int expected)
        {
            var r = LayoutTestHelper.Layout($"<body style='margin:0'><div id='t' style='font-weight:{value};width:10px;height:10px'>x</div></body>");
            Assert.Equal(expected, ((LayoutTestHelper.FindById(r, "t")!.StyledNode as StyledElement)!).Style.FontWeight);
        }

        private void AssertFontSize(string value, float expected)
        {
            var r = LayoutTestHelper.Layout($"<body style='margin:0'><div id='t' style='font-size:{value};width:10px;height:10px'>x</div></body>");
            Assert.True(System.Math.Abs(((LayoutTestHelper.FindById(r, "t")!.StyledNode as StyledElement)!).Style.FontSize - expected) < 1);
        }

        private void AssertProp(string css, System.Action<ComputedStyle> assert)
        {
            var r = LayoutTestHelper.Layout($"<body style='margin:0'><div id='t' style='{css};width:10px;height:10px'>x</div></body>");
            assert(((LayoutTestHelper.FindById(r, "t")!.StyledNode as StyledElement)!).Style);
        }
    }
}
