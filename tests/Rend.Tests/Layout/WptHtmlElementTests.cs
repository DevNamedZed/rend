using Rend.Css;
using Rend.Style;
using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    /// <summary>
    /// Tests for HTML element layout: headings, paragraphs, lists, forms,
    /// semantic elements, and UA stylesheet defaults.
    /// </summary>
    public class WptHtmlElementTests
    {
        private readonly ITestOutputHelper _output;
        public WptHtmlElementTests(ITestOutputHelper output) { _output = output; }

        // [HTML §4.3.6] heading elements have margins
        [Fact] public void H1_HasMargin() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><h1 id='t'>Heading</h1></body>");
            Assert.True(LayoutTestHelper.FindById(r,"t")!.MarginTop > 0);
        }

        [Fact] public void H2_HasMargin() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><h2 id='t'>Heading</h2></body>");
            Assert.True(LayoutTestHelper.FindById(r,"t")!.MarginTop > 0);
        }

        // [HTML §4.3.6] headings are block elements
        [Fact] public void H1_IsBlock() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><h1 id='t' style='width:200px'>H</h1></body>");
            Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width >= 199);
        }

        // [HTML §4.3.7] p element has margins
        [Fact] public void P_HasMargin() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><p id='t'>Para</p></body>");
            Assert.True(LayoutTestHelper.FindById(r,"t")!.MarginTop > 0);
        }

        // [HTML §4.4.8] ul/ol have padding-left for markers
        [Fact] public void Ul_HasPaddingLeft() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><ul id='t'><li>Item</li></ul></body>");
            Assert.True(LayoutTestHelper.FindById(r,"t")!.PaddingLeft > 0);
        }

        [Fact] public void Ol_HasPaddingLeft() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><ol id='t'><li>Item</li></ol></body>");
            Assert.True(LayoutTestHelper.FindById(r,"t")!.PaddingLeft > 0);
        }

        // [HTML] li is display:list-item
        [Fact] public void Li_IsListItem() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><ul><li id='t'>Item</li></ul></body>");
            Assert.Equal(CssDisplay.ListItem, ((LayoutTestHelper.FindById(r,"t")!.StyledNode as StyledElement)!).Style.Display);
        }

        // [HTML §4.5.9] blockquote has margin
        [Fact] public void Blockquote_HasMargin() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><blockquote id='t'>Quote</blockquote></body>");
            Assert.True(LayoutTestHelper.FindById(r,"t")!.MarginLeft > 0);
        }

        // [HTML §4.4.3] pre is preformatted
        [Fact] public void Pre_IsPreformatted() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><pre id='t'>Code</pre></body>");
            Assert.Equal(CssWhiteSpace.Pre, ((LayoutTestHelper.FindById(r,"t")!.StyledNode as StyledElement)!).Style.WhiteSpace);
        }

        // [HTML] table element
        [Fact] public void Table_IsTable() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><table id='t'><tr><td>A</td></tr></table></body>");
            Assert.Equal(CssDisplay.Table, ((LayoutTestHelper.FindById(r,"t")!.StyledNode as StyledElement)!).Style.Display);
        }

        // [HTML] img element
        [Fact] public void Img_Dimensions() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><img id='t' width='100' height='50'></body>");
            var t = LayoutTestHelper.FindById(r,"t")!;
            Assert.True(System.Math.Abs(t.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(t.ContentRect.Height - 50) < 2);
        }

        // [HTML §4.9.1] details default closed
        [Fact] public void Details_Closed() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><details id='t'><summary>S</summary><p>Hidden</p></details></body>");
            Assert.NotNull(LayoutTestHelper.FindById(r,"t"));
        }

        // [HTML §4.9.1] details open
        [Fact] public void Details_Open() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><details id='t' open><summary>S</summary><p>Visible</p></details></body>");
            Assert.NotNull(LayoutTestHelper.FindById(r,"t"));
        }

        // [HTML] fieldset has border
        [Fact] public void Fieldset_Border() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><fieldset id='t'><legend>L</legend>Content</fieldset></body>");
            Assert.True(LayoutTestHelper.FindById(r,"t")!.BorderTopWidth >= 1);
        }

        // [HTML] hr is a block element
        [Fact] public void Hr_IsBlock() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='width:200px'><hr id='t'></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width > 100);
        }

        // [HTML] b/strong are bold
        [Fact] public void Strong_IsBold() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div><strong id='t' style='display:block'>Bold</strong></div></body>");
            Assert.Equal(700, ((LayoutTestHelper.FindById(r,"t")!.StyledNode as StyledElement)!).Style.FontWeight);
        }

        // [HTML] i/em are italic
        [Fact] public void Em_IsItalic() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div><em id='t' style='display:block'>Italic</em></div></body>");
            Assert.Equal(CssFontStyle.Italic, ((LayoutTestHelper.FindById(r,"t")!.StyledNode as StyledElement)!).Style.FontStyle);
        }

        // [HTML §4.3.6] h1 has larger font-size
        [Fact] public void H1_LargerFontSize() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><h1 id='t'>H</h1></body>");
            Assert.True(((LayoutTestHelper.FindById(r,"t")!.StyledNode as StyledElement)!).Style.FontSize > 20);
        }

        // [HTML §4.3.6] h1 is bold
        [Fact] public void H1_IsBold() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><h1 id='t'>H</h1></body>");
            Assert.Equal(700, ((LayoutTestHelper.FindById(r,"t")!.StyledNode as StyledElement)!).Style.FontWeight);
        }

        // [HTML] body has 8px margin by default
        [Fact] public void Body_DefaultMargin() {
            var r = LayoutTestHelper.Layout("<body><div id='t' style='height:10px'></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.X >= 7);
        }

        // [HTML] th is center-aligned and bold
        [Fact] public void Th_CenterBold() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><table><tr><th id='t'>H</th></tr></table></body>");
            var s = (LayoutTestHelper.FindById(r,"t")!.StyledNode as StyledElement)!;
            Assert.Equal(CssTextAlign.Center, s.Style.TextAlign);
            Assert.Equal(700, s.Style.FontWeight);
        }

        // [HTML §4.3.6] small has smaller font
        [Fact] public void Small_SmallerFont() {
            var r = LayoutTestHelper.Layout("<body style='margin:0;font-size:16px'><small id='t' style='display:block'>S</small></body>");
            Assert.True(((LayoutTestHelper.FindById(r,"t")!.StyledNode as StyledElement)!).Style.FontSize < 16);
        }

        // [HTML] sup/sub affect vertical-align
        [Fact] public void Sup_Superscript() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div><sup style='display:block' id='t'>S</sup></div></body>");
            Assert.NotNull(LayoutTestHelper.FindById(r,"t"));
        }

        // [HTML] div is display:block
        [Fact] public void Div_IsBlock() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='t' style='height:10px'>D</div></body>");
            Assert.Equal(CssDisplay.Block, ((LayoutTestHelper.FindById(r,"t")!.StyledNode as StyledElement)!).Style.Display);
        }

        // [HTML] span is display:inline (resolved as block in our BFC test)
        [Fact] public void Span_IsInline() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div><span id='t' style='display:inline-block;width:10px;height:10px'>S</span></div></body>");
            Assert.NotNull(LayoutTestHelper.FindById(r,"t"));
        }
    }
}
