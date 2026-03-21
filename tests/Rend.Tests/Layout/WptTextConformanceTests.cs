using Rend.Css;
using Rend.Style;
using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class WptTextConformanceTests
    {
        private readonly ITestOutputHelper _output;
        public WptTextConformanceTests(ITestOutputHelper output) { _output = output; }

        // text-align values
        [Fact] public void TextAlign_Left() { AssertTextAlign("left", CssTextAlign.Left); }
        [Fact] public void TextAlign_Right() { AssertTextAlign("right", CssTextAlign.Right); }
        [Fact] public void TextAlign_Center() { AssertTextAlign("center", CssTextAlign.Center); }
        [Fact] public void TextAlign_Justify() { AssertTextAlign("justify", CssTextAlign.Justify); }
        [Fact] public void TextAlign_JustifyAll() { AssertTextAlign("justify-all", CssTextAlign.JustifyAll); }
        [Fact] public void TextAlign_Start() { AssertTextAlign("start", CssTextAlign.Start); }
        [Fact] public void TextAlign_End() { AssertTextAlign("end", CssTextAlign.End); }

        // text-align-last values
        [Fact] public void TextAlignLast_Justify() { AssertTextAlignLast("justify", CssTextAlign.Justify); }
        [Fact] public void TextAlignLast_Center() { AssertTextAlignLast("center", CssTextAlign.Center); }
        [Fact] public void TextAlignLast_Right() { AssertTextAlignLast("right", CssTextAlign.Right); }

        // text-justify values
        [Fact] public void TextJustify_None() { AssertTextJustify("none", CssTextJustify.None); }
        [Fact] public void TextJustify_InterWord() { AssertTextJustify("inter-word", CssTextJustify.InterWord); }
        [Fact] public void TextJustify_InterCharacter() { AssertTextJustify("inter-character", CssTextJustify.InterCharacter); }

        // text-transform values
        [Fact] public void TextTransform_Uppercase() { AssertProp("text-transform:uppercase", s => Assert.Equal(CssTextTransform.Uppercase, s.TextTransform)); }
        [Fact] public void TextTransform_Lowercase() { AssertProp("text-transform:lowercase", s => Assert.Equal(CssTextTransform.Lowercase, s.TextTransform)); }
        [Fact] public void TextTransform_Capitalize() { AssertProp("text-transform:capitalize", s => Assert.Equal(CssTextTransform.Capitalize, s.TextTransform)); }
        [Fact] public void TextTransform_None() { AssertProp("text-transform:none", s => Assert.Equal(CssTextTransform.None, s.TextTransform)); }

        // white-space values
        [Fact] public void WhiteSpace_Normal() { AssertProp("white-space:normal", s => Assert.Equal(CssWhiteSpace.Normal, s.WhiteSpace)); }
        [Fact] public void WhiteSpace_Pre() { AssertProp("white-space:pre", s => Assert.Equal(CssWhiteSpace.Pre, s.WhiteSpace)); }
        [Fact] public void WhiteSpace_Nowrap() { AssertProp("white-space:nowrap", s => Assert.Equal(CssWhiteSpace.Nowrap, s.WhiteSpace)); }
        [Fact] public void WhiteSpace_PreWrap() { AssertProp("white-space:pre-wrap", s => Assert.Equal(CssWhiteSpace.PreWrap, s.WhiteSpace)); }
        [Fact] public void WhiteSpace_PreLine() { AssertProp("white-space:pre-line", s => Assert.Equal(CssWhiteSpace.PreLine, s.WhiteSpace)); }

        // word-break values
        [Fact] public void WordBreak_Normal() { AssertProp("word-break:normal", s => Assert.Equal(CssWordBreak.Normal, s.WordBreak)); }
        [Fact] public void WordBreak_BreakAll() { AssertProp("word-break:break-all", s => Assert.Equal(CssWordBreak.BreakAll, s.WordBreak)); }
        [Fact] public void WordBreak_KeepAll() { AssertProp("word-break:keep-all", s => Assert.Equal(CssWordBreak.KeepAll, s.WordBreak)); }

        // overflow-wrap values
        [Fact] public void OverflowWrap_Normal() { AssertProp("overflow-wrap:normal", s => Assert.Equal(CssOverflowWrap.Normal, s.OverflowWrap)); }
        [Fact] public void OverflowWrap_BreakWord() { AssertProp("overflow-wrap:break-word", s => Assert.Equal(CssOverflowWrap.BreakWord, s.OverflowWrap)); }
        [Fact] public void OverflowWrap_Anywhere() { AssertProp("overflow-wrap:anywhere", s => Assert.Equal(CssOverflowWrap.Anywhere, s.OverflowWrap)); }

        // text-overflow values
        [Fact] public void TextOverflow_Clip() { AssertProp("text-overflow:clip", s => Assert.Equal(CssTextOverflow.Clip, s.TextOverflow)); }
        [Fact] public void TextOverflow_Ellipsis() { AssertProp("text-overflow:ellipsis", s => Assert.Equal(CssTextOverflow.Ellipsis, s.TextOverflow)); }

        // text-indent parsed
        [Fact]
        public void TextIndent_30px()
        {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='t' style='text-indent:30px;width:200px'>x</div></body>");
            Assert.True(System.Math.Abs(((LayoutTestHelper.FindById(r, "t")!.StyledNode as StyledElement)!).Style.TextIndent - 30) < 1);
        }

        // word-spacing parsed
        [Fact]
        public void WordSpacing_5px()
        {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='t' style='word-spacing:5px;width:200px'>x</div></body>");
            Assert.True(System.Math.Abs(((LayoutTestHelper.FindById(r, "t")!.StyledNode as StyledElement)!).Style.WordSpacing - 5) < 0.5f);
        }

        // letter-spacing parsed
        [Fact]
        public void LetterSpacing_3px()
        {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='t' style='letter-spacing:3px;width:200px'>x</div></body>");
            Assert.True(System.Math.Abs(((LayoutTestHelper.FindById(r, "t")!.StyledNode as StyledElement)!).Style.LetterSpacing - 3) < 0.5f);
        }

        // line-height unitless
        [Fact]
        public void LineHeight_Unitless()
        {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='t' style='line-height:1.5;width:200px'>x</div></body>");
            Assert.True(((LayoutTestHelper.FindById(r, "t")!.StyledNode as StyledElement)!).Style.LineHeight < 0);
        }

        // line-height px
        [Fact]
        public void LineHeight_Px()
        {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='t' style='line-height:24px;width:200px'>x</div></body>");
            Assert.True(System.Math.Abs(((LayoutTestHelper.FindById(r, "t")!.StyledNode as StyledElement)!).Style.LineHeight - 24) < 1);
        }

        // direction values
        [Fact] public void Direction_Ltr() { AssertProp("direction:ltr", s => Assert.Equal(CssDirection.Ltr, s.Direction)); }
        [Fact] public void Direction_Rtl() { AssertProp("direction:rtl", s => Assert.Equal(CssDirection.Rtl, s.Direction)); }

        // HTML dir attribute
        [Fact]
        public void DirAttribute_Rtl()
        {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div dir='rtl' id='t' style='width:200px'>x</div></body>");
            Assert.Equal(CssDirection.Rtl, ((LayoutTestHelper.FindById(r, "t")!.StyledNode as StyledElement)!).Style.Direction);
        }

        // text-align inherits
        [Fact]
        public void TextAlign_Inherits()
        {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='text-align:center'><div id='t' style='width:200px'>x</div></div></body>");
            Assert.Equal(CssTextAlign.Center, ((LayoutTestHelper.FindById(r, "t")!.StyledNode as StyledElement)!).Style.TextAlign);
        }

        // white-space inherits
        [Fact]
        public void WhiteSpace_Inherits()
        {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='white-space:pre'><div id='t' style='width:200px'>x</div></div></body>");
            Assert.Equal(CssWhiteSpace.Pre, ((LayoutTestHelper.FindById(r, "t")!.StyledNode as StyledElement)!).Style.WhiteSpace);
        }

        // direction inherits
        [Fact]
        public void Direction_Inherits()
        {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='direction:rtl'><div id='t' style='width:200px'>x</div></div></body>");
            Assert.Equal(CssDirection.Rtl, ((LayoutTestHelper.FindById(r, "t")!.StyledNode as StyledElement)!).Style.Direction);
        }

        private void AssertTextAlign(string value, CssTextAlign expected)
        {
            var r = LayoutTestHelper.Layout($"<body style='margin:0'><div id='t' style='text-align:{value};width:200px'>x</div></body>");
            Assert.Equal(expected, ((LayoutTestHelper.FindById(r, "t")!.StyledNode as StyledElement)!).Style.TextAlign);
        }

        private void AssertTextAlignLast(string value, CssTextAlign expected)
        {
            var r = LayoutTestHelper.Layout($"<body style='margin:0'><div id='t' style='text-align-last:{value};width:200px'>x</div></body>");
            Assert.Equal(expected, ((LayoutTestHelper.FindById(r, "t")!.StyledNode as StyledElement)!).Style.TextAlignLast);
        }

        private void AssertTextJustify(string value, CssTextJustify expected)
        {
            var r = LayoutTestHelper.Layout($"<body style='margin:0'><div id='t' style='text-justify:{value};width:200px'>x</div></body>");
            Assert.Equal(expected, ((LayoutTestHelper.FindById(r, "t")!.StyledNode as StyledElement)!).Style.TextJustify);
        }

        private void AssertProp(string css, System.Action<ComputedStyle> assert)
        {
            var r = LayoutTestHelper.Layout($"<body style='margin:0'><div id='t' style='{css};width:200px'>x</div></body>");
            assert(((LayoutTestHelper.FindById(r, "t")!.StyledNode as StyledElement)!).Style);
        }
    }
}
