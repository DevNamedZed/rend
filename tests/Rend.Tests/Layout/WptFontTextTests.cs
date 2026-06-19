using Rend.Css;
using Rend.Style;
using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    /// <summary>
    /// Tests for CSS font and text properties parsing and inheritance.
    /// </summary>
    public class WptFontTextTests
    {
        private readonly ITestOutputHelper _output;
        public WptFontTextTests(ITestOutputHelper output) { _output = output; }

        // ======= FONT PROPERTIES =======

        // [CSS-FONTS §3.1] font-family
        [Fact] public void FontFamily_Parsed() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='t' style='font-family:Arial,sans-serif;width:100px'>x</div></body>");
            var s = (LayoutTestHelper.FindById(r,"t")!.StyledNode as StyledElement)!;
            _output.WriteLine($"font-family={s.Style.FontFamily}");
            Assert.NotNull(s.Style.FontFamily);
        }

        // [CSS-FONTS §3.2] font-weight values
        [Fact] public void FontWeight_Normal() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='t' style='font-weight:normal;width:100px'>x</div></body>");
            Assert.Equal(400, ((LayoutTestHelper.FindById(r,"t")!.StyledNode as StyledElement)!).Style.FontWeight);
        }

        [Fact] public void FontWeight_Bold() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='t' style='font-weight:bold;width:100px'>x</div></body>");
            Assert.Equal(700, ((LayoutTestHelper.FindById(r,"t")!.StyledNode as StyledElement)!).Style.FontWeight);
        }

        [Fact] public void FontWeight_900() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='t' style='font-weight:900;width:100px'>x</div></body>");
            Assert.Equal(900, ((LayoutTestHelper.FindById(r,"t")!.StyledNode as StyledElement)!).Style.FontWeight);
        }

        [Fact] public void FontWeight_100() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='t' style='font-weight:100;width:100px'>x</div></body>");
            Assert.Equal(100, ((LayoutTestHelper.FindById(r,"t")!.StyledNode as StyledElement)!).Style.FontWeight);
        }

        // [CSS-FONTS §3.3] font-style
        [Fact] public void FontStyle_Italic() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='t' style='font-style:italic;width:100px'>x</div></body>");
            Assert.Equal(CssFontStyle.Italic, ((LayoutTestHelper.FindById(r,"t")!.StyledNode as StyledElement)!).Style.FontStyle);
        }

        [Fact] public void FontStyle_Oblique() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='t' style='font-style:oblique;width:100px'>x</div></body>");
            Assert.Equal(CssFontStyle.Oblique, ((LayoutTestHelper.FindById(r,"t")!.StyledNode as StyledElement)!).Style.FontStyle);
        }

        // [CSS-FONTS §3.5] font-size keywords
        [Fact] public void FontSize_Px() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='t' style='font-size:20px;width:100px'>x</div></body>");
            Assert.True(System.Math.Abs(((LayoutTestHelper.FindById(r,"t")!.StyledNode as StyledElement)!).Style.FontSize - 20) < 1);
        }

        [Fact] public void FontSize_Em() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='font-size:16px'><div id='t' style='font-size:1.5em;width:100px'>x</div></div></body>");
            Assert.True(System.Math.Abs(((LayoutTestHelper.FindById(r,"t")!.StyledNode as StyledElement)!).Style.FontSize - 24) < 1);
        }

        [Fact] public void FontSize_Percent() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='font-size:20px'><div id='t' style='font-size:150%;width:100px'>x</div></div></body>");
            Assert.True(System.Math.Abs(((LayoutTestHelper.FindById(r,"t")!.StyledNode as StyledElement)!).Style.FontSize - 30) < 1);
        }

        // [CSS2 §15.6] font shorthand
        [Fact] public void Font_Shorthand() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='t' style='font:bold 20px Arial;width:100px'>x</div></body>");
            var s = (LayoutTestHelper.FindById(r,"t")!.StyledNode as StyledElement)!;
            Assert.Equal(700, s.Style.FontWeight);
            Assert.True(System.Math.Abs(s.Style.FontSize - 20) < 1);
        }

        // ======= TEXT PROPERTIES =======

        // [CSS-TEXT §6.1] text-align values
        [Fact] public void TextAlign_Left() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='t' style='text-align:left;width:200px'>x</div></body>");
            Assert.Equal(CssTextAlign.Left, ((LayoutTestHelper.FindById(r,"t")!.StyledNode as StyledElement)!).Style.TextAlign);
        }

        [Fact] public void TextAlign_Right() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='t' style='text-align:right;width:200px'>x</div></body>");
            Assert.Equal(CssTextAlign.Right, ((LayoutTestHelper.FindById(r,"t")!.StyledNode as StyledElement)!).Style.TextAlign);
        }

        [Fact] public void TextAlign_Center() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='t' style='text-align:center;width:200px'>x</div></body>");
            Assert.Equal(CssTextAlign.Center, ((LayoutTestHelper.FindById(r,"t")!.StyledNode as StyledElement)!).Style.TextAlign);
        }

        [Fact] public void TextAlign_Justify() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='t' style='text-align:justify;width:200px'>x</div></body>");
            Assert.Equal(CssTextAlign.Justify, ((LayoutTestHelper.FindById(r,"t")!.StyledNode as StyledElement)!).Style.TextAlign);
        }

        [Fact] public void TextAlign_JustifyAll() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='t' style='text-align:justify-all;width:200px'>x</div></body>");
            // [COMPAT] Chrome drops text-align: justify-all → falls back to initial value (start).
            Assert.Equal(CssTextAlign.Start, ((LayoutTestHelper.FindById(r,"t")!.StyledNode as StyledElement)!).Style.TextAlign);
        }

        [Fact] public void TextAlign_Start() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='t' style='text-align:start;width:200px'>x</div></body>");
            Assert.Equal(CssTextAlign.Start, ((LayoutTestHelper.FindById(r,"t")!.StyledNode as StyledElement)!).Style.TextAlign);
        }

        [Fact] public void TextAlign_End() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='t' style='text-align:end;width:200px'>x</div></body>");
            Assert.Equal(CssTextAlign.End, ((LayoutTestHelper.FindById(r,"t")!.StyledNode as StyledElement)!).Style.TextAlign);
        }

        // [CSS-TEXT §6.2] text-align-last
        [Fact] public void TextAlignLast_Justify() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='t' style='text-align-last:justify;width:200px'>x</div></body>");
            Assert.Equal(CssTextAlign.Justify, ((LayoutTestHelper.FindById(r,"t")!.StyledNode as StyledElement)!).Style.TextAlignLast);
        }

        [Fact] public void TextAlignLast_Center() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='t' style='text-align-last:center;width:200px'>x</div></body>");
            Assert.Equal(CssTextAlign.Center, ((LayoutTestHelper.FindById(r,"t")!.StyledNode as StyledElement)!).Style.TextAlignLast);
        }

        // [CSS-TEXT §6.3] text-justify
        [Fact] public void TextJustify_None() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='t' style='text-justify:none;width:200px'>x</div></body>");
            Assert.Equal(CssTextJustify.None, ((LayoutTestHelper.FindById(r,"t")!.StyledNode as StyledElement)!).Style.TextJustify);
        }

        [Fact] public void TextJustify_InterWord() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='t' style='text-justify:inter-word;width:200px'>x</div></body>");
            Assert.Equal(CssTextJustify.InterWord, ((LayoutTestHelper.FindById(r,"t")!.StyledNode as StyledElement)!).Style.TextJustify);
        }

        [Fact] public void TextJustify_InterCharacter() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='t' style='text-justify:inter-character;width:200px'>x</div></body>");
            Assert.Equal(CssTextJustify.InterCharacter, ((LayoutTestHelper.FindById(r,"t")!.StyledNode as StyledElement)!).Style.TextJustify);
        }

        // [CSS-TEXT §7.1] text-indent
        [Fact] public void TextIndent_Parsed() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='t' style='text-indent:30px;width:200px'>x</div></body>");
            Assert.True(System.Math.Abs(((LayoutTestHelper.FindById(r,"t")!.StyledNode as StyledElement)!).Style.TextIndent - 30) < 1);
        }

        // [CSS-TEXT §5.1] text-transform
        [Fact] public void TextTransform_None() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='t' style='text-transform:none;width:200px'>x</div></body>");
            Assert.Equal(CssTextTransform.None, ((LayoutTestHelper.FindById(r,"t")!.StyledNode as StyledElement)!).Style.TextTransform);
        }

        // [CSS-TEXT §8] word-spacing and letter-spacing
        [Fact] public void WordSpacing_Px() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='t' style='word-spacing:5px;width:200px'>x</div></body>");
            Assert.True(System.Math.Abs(((LayoutTestHelper.FindById(r,"t")!.StyledNode as StyledElement)!).Style.WordSpacing - 5) < 0.5f);
        }

        [Fact] public void LetterSpacing_Px() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='t' style='letter-spacing:3px;width:200px'>x</div></body>");
            Assert.True(System.Math.Abs(((LayoutTestHelper.FindById(r,"t")!.StyledNode as StyledElement)!).Style.LetterSpacing - 3) < 0.5f);
        }

        // [CSS-TEXT §9] word-break
        [Fact] public void WordBreak_BreakAll() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='t' style='word-break:break-all;width:200px'>x</div></body>");
            Assert.Equal(CssWordBreak.BreakAll, ((LayoutTestHelper.FindById(r,"t")!.StyledNode as StyledElement)!).Style.WordBreak);
        }

        [Fact] public void WordBreak_KeepAll() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='t' style='word-break:keep-all;width:200px'>x</div></body>");
            Assert.Equal(CssWordBreak.KeepAll, ((LayoutTestHelper.FindById(r,"t")!.StyledNode as StyledElement)!).Style.WordBreak);
        }

        // [CSS-TEXT §10] overflow-wrap
        [Fact] public void OverflowWrap_BreakWord() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='t' style='overflow-wrap:break-word;width:200px'>x</div></body>");
            Assert.Equal(CssOverflowWrap.BreakWord, ((LayoutTestHelper.FindById(r,"t")!.StyledNode as StyledElement)!).Style.OverflowWrap);
        }

        [Fact] public void OverflowWrap_Anywhere() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='t' style='overflow-wrap:anywhere;width:200px'>x</div></body>");
            Assert.Equal(CssOverflowWrap.Anywhere, ((LayoutTestHelper.FindById(r,"t")!.StyledNode as StyledElement)!).Style.OverflowWrap);
        }

        // [CSS-TEXT §11] text-overflow
        [Fact] public void TextOverflow_Ellipsis() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='t' style='text-overflow:ellipsis;overflow:hidden;white-space:nowrap;width:100px'>long text</div></body>");
            Assert.Equal(CssTextOverflow.Ellipsis, ((LayoutTestHelper.FindById(r,"t")!.StyledNode as StyledElement)!).Style.TextOverflow);
        }

        [Fact] public void TextOverflow_Clip() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='t' style='text-overflow:clip;width:100px'>x</div></body>");
            Assert.Equal(CssTextOverflow.Clip, ((LayoutTestHelper.FindById(r,"t")!.StyledNode as StyledElement)!).Style.TextOverflow);
        }

        // [CSS2 §16.2] line-height
        [Fact] public void LineHeight_Unitless() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='t' style='line-height:1.5;font-size:16px;width:200px'>x</div></body>");
            var lh = ((LayoutTestHelper.FindById(r,"t")!.StyledNode as StyledElement)!).Style.LineHeight;
            // unitless stored as negative: -1.5
            Assert.True(lh < 0, $"Unitless line-height stored negative (got {lh})");
        }

        [Fact] public void LineHeight_Px() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='t' style='line-height:24px;width:200px'>x</div></body>");
            var lh = ((LayoutTestHelper.FindById(r,"t")!.StyledNode as StyledElement)!).Style.LineHeight;
            Assert.True(System.Math.Abs(lh - 24) < 1);
        }

        // [CSS2 §16.4] direction
        [Fact] public void Direction_Rtl() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='t' style='direction:rtl;width:200px'>x</div></body>");
            Assert.Equal(CssDirection.Rtl, ((LayoutTestHelper.FindById(r,"t")!.StyledNode as StyledElement)!).Style.Direction);
        }

        [Fact] public void Direction_Ltr() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='t' style='direction:ltr;width:200px'>x</div></body>");
            Assert.Equal(CssDirection.Ltr, ((LayoutTestHelper.FindById(r,"t")!.StyledNode as StyledElement)!).Style.Direction);
        }

        // [CSS-TEXT-DECOR §3.1] text-decoration-line
        [Fact] public void TextDecorationLine_Underline() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='t' style='text-decoration:underline;width:200px'>x</div></body>");
            var s = (LayoutTestHelper.FindById(r,"t")!.StyledNode as StyledElement)!;
            Assert.True((s.Style.TextDecorationLine & CssTextDecorationLine.Underline) != 0);
        }

        [Fact] public void TextDecorationLine_LineThrough() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='t' style='text-decoration:line-through;width:200px'>x</div></body>");
            var s = (LayoutTestHelper.FindById(r,"t")!.StyledNode as StyledElement)!;
            Assert.True((s.Style.TextDecorationLine & CssTextDecorationLine.LineThrough) != 0);
        }

        [Fact] public void TextDecorationLine_Overline() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='t' style='text-decoration:overline;width:200px'>x</div></body>");
            var s = (LayoutTestHelper.FindById(r,"t")!.StyledNode as StyledElement)!;
            Assert.True((s.Style.TextDecorationLine & CssTextDecorationLine.Overline) != 0);
        }

        // [CSS-TEXT-DECOR §3.3] text-decoration-style
        [Fact] public void TextDecorationStyle_Solid() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='t' style='text-decoration-line:underline;text-decoration-style:solid;width:200px'>x</div></body>");
            Assert.Equal(CssTextDecorationStyle.Solid, ((LayoutTestHelper.FindById(r,"t")!.StyledNode as StyledElement)!).Style.TextDecorationStyle);
        }

        [Fact] public void TextDecorationStyle_Dashed() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='t' style='text-decoration-line:underline;text-decoration-style:dashed;width:200px'>x</div></body>");
            Assert.Equal(CssTextDecorationStyle.Dashed, ((LayoutTestHelper.FindById(r,"t")!.StyledNode as StyledElement)!).Style.TextDecorationStyle);
        }

        [Fact] public void TextDecorationStyle_Dotted() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='t' style='text-decoration-line:underline;text-decoration-style:dotted;width:200px'>x</div></body>");
            Assert.Equal(CssTextDecorationStyle.Dotted, ((LayoutTestHelper.FindById(r,"t")!.StyledNode as StyledElement)!).Style.TextDecorationStyle);
        }

        [Fact] public void TextDecorationStyle_Double() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='t' style='text-decoration-line:underline;text-decoration-style:double;width:200px'>x</div></body>");
            Assert.Equal(CssTextDecorationStyle.Double, ((LayoutTestHelper.FindById(r,"t")!.StyledNode as StyledElement)!).Style.TextDecorationStyle);
        }

        [Fact] public void TextDecorationStyle_Wavy() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='t' style='text-decoration-line:underline;text-decoration-style:wavy;width:200px'>x</div></body>");
            Assert.Equal(CssTextDecorationStyle.Wavy, ((LayoutTestHelper.FindById(r,"t")!.StyledNode as StyledElement)!).Style.TextDecorationStyle);
        }
    }
}
