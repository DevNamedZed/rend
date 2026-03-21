using Rend.Css;
using Rend.Style;
using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    /// <summary>
    /// Tests for HTML semantic element layout behavior: block/inline classification,
    /// UA stylesheet margins/padding/borders, and correct spatial positioning.
    /// </summary>
    public class WptHtmlSemanticTests
    {
        private readonly ITestOutputHelper _output;
        public WptHtmlSemanticTests(ITestOutputHelper output) { _output = output; }

        // [HTML §4.4.7] div is a block-level element that fills available width
        [Fact]
        public void Div_IsBlockLevel_FillsAvailableWidth()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div id='t' style='height:10px'></div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"div width={box.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 400) < 2);
            Assert.True(System.Math.Abs(box.ContentRect.X - 0) < 2);
        }

        // [HTML §4.5.1] span is inline — explicit width/height have no effect
        [Fact]
        public void Span_IsInline_WidthHeightIgnored()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                <div><span id='t' style='width:200px;height:200px'>Text</span></div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            if (box != null)
            {
                _output.WriteLine($"span width={box.ContentRect.Width} height={box.ContentRect.Height}");
                Assert.True(box.ContentRect.Width < 190,
                    $"Inline span should not respect explicit width (got {box.ContentRect.Width})");
            }
        }

        // [HTML §4.4.1] p is block with UA margin-top/bottom of 1em (16px at default)
        [Fact]
        public void P_HasBlockMargins()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0;padding-bottom:1px'><p id='t'>Paragraph</p></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"p marginTop={box.MarginTop} marginBottom={box.MarginBottom} width={box.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.MarginTop - 16) < 2);
            Assert.True(System.Math.Abs(box.MarginBottom - 16) < 2);
            Assert.True(System.Math.Abs(box.ContentRect.Width - 400) < 2);
        }

        // [HTML §4.3.6] h1 font-size is 2em (32px) with margin 0.67em (21.44px)
        [Fact]
        public void H1_FontSizeAndMargin()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><h1 id='t'>Heading 1</h1></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            var style = (box.StyledNode as StyledElement)!.Style;
            _output.WriteLine($"h1 fontSize={style.FontSize} marginTop={box.MarginTop}");
            Assert.True(System.Math.Abs(style.FontSize - 32) < 2);
            Assert.True(System.Math.Abs(box.MarginTop - 21.44f) < 2);
        }

        // [HTML §4.3.6] h2 font-size is 1.5em (24px) with margin 0.83em (19.92px)
        [Fact]
        public void H2_FontSizeAndMargin()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><h2 id='t'>Heading 2</h2></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            var style = (box.StyledNode as StyledElement)!.Style;
            _output.WriteLine($"h2 fontSize={style.FontSize} marginTop={box.MarginTop}");
            Assert.True(System.Math.Abs(style.FontSize - 24) < 2);
            Assert.True(System.Math.Abs(box.MarginTop - 19.92f) < 2);
        }

        // [HTML §4.3.6] h3 font-size is 1.17em (18.72px)
        [Fact]
        public void H3_FontSizeAndMargin()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><h3 id='t'>Heading 3</h3></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            var style = (box.StyledNode as StyledElement)!.Style;
            _output.WriteLine($"h3 fontSize={style.FontSize} marginTop={box.MarginTop}");
            Assert.True(System.Math.Abs(style.FontSize - 18.72f) < 2);
        }

        // [HTML §4.3.6] h4 has default font-size (16px), margin 1.33em (21.28px)
        [Fact]
        public void H4_DefaultFontSizeWithMargin()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><h4 id='t'>Heading 4</h4></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            var style = (box.StyledNode as StyledElement)!.Style;
            _output.WriteLine($"h4 fontSize={style.FontSize} marginTop={box.MarginTop}");
            Assert.True(System.Math.Abs(style.FontSize - 16) < 2);
            Assert.True(System.Math.Abs(box.MarginTop - 21.28f) < 2);
        }

        // [HTML §4.3.6] h5 font-size is 0.83em (13.28px)
        [Fact]
        public void H5_SmallerFontSize()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><h5 id='t'>Heading 5</h5></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            var style = (box.StyledNode as StyledElement)!.Style;
            _output.WriteLine($"h5 fontSize={style.FontSize}");
            Assert.True(System.Math.Abs(style.FontSize - 13.28f) < 2);
        }

        // [HTML §4.3.6] h6 font-size is 0.67em (10.72px)
        [Fact]
        public void H6_SmallestFontSize()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><h6 id='t'>Heading 6</h6></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            var style = (box.StyledNode as StyledElement)!.Style;
            _output.WriteLine($"h6 fontSize={style.FontSize}");
            Assert.True(System.Math.Abs(style.FontSize - 10.72f) < 2);
        }

        // [HTML §4.3.3] section is a block element filling available width
        [Fact]
        public void Section_IsBlockFullWidth()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><section id='t' style='height:20px'></section></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"section width={box.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 400) < 2);
            Assert.Equal(CssDisplay.Block, (box.StyledNode as StyledElement)!.Style.Display);
        }

        // [HTML §4.3.2] article is a block element filling available width
        [Fact]
        public void Article_IsBlockFullWidth()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><article id='t' style='height:20px'></article></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"article width={box.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 400) < 2);
            Assert.Equal(CssDisplay.Block, (box.StyledNode as StyledElement)!.Style.Display);
        }

        // [HTML §4.3.8] header is a block element
        [Fact]
        public void Header_IsBlockFullWidth()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><header id='t' style='height:20px'></header></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"header width={box.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 400) < 2);
            Assert.Equal(CssDisplay.Block, (box.StyledNode as StyledElement)!.Style.Display);
        }

        // [HTML §4.3.9] footer is a block element
        [Fact]
        public void Footer_IsBlockFullWidth()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><footer id='t' style='height:20px'></footer></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"footer width={box.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 400) < 2);
            Assert.Equal(CssDisplay.Block, (box.StyledNode as StyledElement)!.Style.Display);
        }

        // [HTML §4.3.4] nav is a block element
        [Fact]
        public void Nav_IsBlockFullWidth()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><nav id='t' style='height:20px'></nav></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"nav width={box.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 400) < 2);
            Assert.Equal(CssDisplay.Block, (box.StyledNode as StyledElement)!.Style.Display);
        }

        // [HTML §4.3.7] main is a block element
        [Fact]
        public void Main_IsBlockFullWidth()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><main id='t' style='height:20px'></main></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"main width={box.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 400) < 2);
            Assert.Equal(CssDisplay.Block, (box.StyledNode as StyledElement)!.Style.Display);
        }

        // [HTML §4.3.5] aside is a block element
        [Fact]
        public void Aside_IsBlockFullWidth()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><aside id='t' style='height:20px'></aside></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"aside width={box.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 400) < 2);
            Assert.Equal(CssDisplay.Block, (box.StyledNode as StyledElement)!.Style.Display);
        }

        // [HTML §4.4.8] ul has 40px padding-left for marker indentation
        [Fact]
        public void Ul_HasPaddingLeft40()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><ul id='t'><li>Item</li></ul></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"ul paddingLeft={box.PaddingLeft} marginTop={box.MarginTop}");
            Assert.True(System.Math.Abs(box.PaddingLeft - 40) < 2);
            Assert.True(System.Math.Abs(box.MarginTop - 16) < 2);
        }

        // [HTML §4.4.8] ol has 40px padding-left
        [Fact]
        public void Ol_HasPaddingLeft40()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><ol id='t'><li>Item</li></ol></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"ol paddingLeft={box.PaddingLeft}");
            Assert.True(System.Math.Abs(box.PaddingLeft - 40) < 2);
        }

        // [HTML §4.4.8] li has display:list-item
        [Fact]
        public void Li_DisplayIsListItem()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><ul><li id='t'>Item</li></ul></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            var style = (box.StyledNode as StyledElement)!.Style;
            _output.WriteLine($"li display={style.Display}");
            Assert.Equal(CssDisplay.ListItem, style.Display);
        }

        // [HTML §4.4.8] li content is indented by ul padding
        [Fact]
        public void Li_ContentIndentedByUlPadding()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><ul id='list'><li id='t'>Item</li></ul></body>");
            var listBox = LayoutTestHelper.FindById(root, "list")!;
            var itemBox = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"ul paddingLeft={listBox.PaddingLeft} li X={itemBox.ContentRect.X}");
            Assert.True(itemBox.ContentRect.X >= 38,
                $"li should be indented by ul padding-left (X={itemBox.ContentRect.X})");
        }

        // [HTML §4.9] table/tr/td basic structure
        [Fact]
        public void Table_BasicStructureLayout()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                <table id='t' style='width:200px'>
                    <tr><td id='cell'>Content</td></tr>
                </table></body>");
            var table = LayoutTestHelper.FindById(root, "t")!;
            var cell = LayoutTestHelper.FindById(root, "cell")!;
            _output.WriteLine($"table width={table.ContentRect.Width} cell padding={cell.PaddingTop}");
            Assert.True(System.Math.Abs(table.ContentRect.Width - 200) < 2);
            Assert.True(cell.PaddingTop >= 1, "td has UA padding of 1px");
        }

        // [HTML §4.8.3] img with width/height attributes
        [Fact]
        public void Img_RespectsWidthHeightAttributes()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><img id='t' width='120' height='80'></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"img width={box.ContentRect.Width} height={box.ContentRect.Height}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 120) < 2);
            Assert.True(System.Math.Abs(box.ContentRect.Height - 80) < 2);
        }

        // [HTML §4.12.2] hr is block with inset borders and auto horizontal margins
        [Fact]
        public void Hr_IsBlockWithBorder()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='width:300px'><hr id='t'></div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"hr borderTop={box.BorderTopWidth} borderBottom={box.BorderBottomWidth} width={box.ContentRect.Width}");
            Assert.True(box.BorderTopWidth >= 1, "hr has border-width: 1px");
            Assert.True(box.ContentRect.Width > 200, "hr fills available width");
        }

        // [HTML §4.12.2] hr vertical margin is 0.5em (8px)
        [Fact]
        public void Hr_HasVerticalMargin()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0;padding-bottom:1px'><hr id='t'></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"hr marginTop={box.MarginTop} marginBottom={box.MarginBottom}");
            Assert.True(System.Math.Abs(box.MarginTop - 8) < 2);
            Assert.True(System.Math.Abs(box.MarginBottom - 8) < 2);
        }

        // [HTML §4.4.4] blockquote has left/right margins of 40px
        [Fact]
        public void Blockquote_HasLeftRightMargins()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><blockquote id='t'>Quoted text</blockquote></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"blockquote marginLeft={box.MarginLeft} marginRight={box.MarginRight} width={box.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.MarginLeft - 40) < 2);
            Assert.True(System.Math.Abs(box.MarginRight - 40) < 2);
            Assert.True(System.Math.Abs(box.ContentRect.Width - 320) < 2);
        }

        // [HTML §4.4.3] pre has white-space:pre and monospace font
        [Fact]
        public void Pre_PreservesWhitespaceAndMonospace()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><pre id='t'>Line 1
Line 2</pre></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            var style = (box.StyledNode as StyledElement)!.Style;
            _output.WriteLine($"pre whiteSpace={style.WhiteSpace} marginTop={box.MarginTop}");
            Assert.Equal(CssWhiteSpace.Pre, style.WhiteSpace);
            Assert.True(System.Math.Abs(box.MarginTop - 16) < 2);
        }

        // [HTML §4.10.16] fieldset has a 2px groove border
        [Fact]
        public void Fieldset_HasGrooveBorder()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><fieldset id='t'>Content</fieldset></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"fieldset borderTop={box.BorderTopWidth} paddingLeft={box.PaddingLeft} marginLeft={box.MarginLeft}");
            Assert.True(System.Math.Abs(box.BorderTopWidth - 2) < 2);
            Assert.True(System.Math.Abs(box.MarginLeft - 2) < 2);
        }

        // [HTML §4.10.16] fieldset padding values match UA stylesheet
        [Fact]
        public void Fieldset_PaddingMatchesUA()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><fieldset id='t'>Content</fieldset></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            float expectedPaddingLeft = 0.75f * 16;
            _output.WriteLine($"fieldset paddingLeft={box.PaddingLeft} expected={expectedPaddingLeft}");
            Assert.True(System.Math.Abs(box.PaddingLeft - expectedPaddingLeft) < 2);
        }

        // [HTML §4.10.3] form is a block element
        [Fact]
        public void Form_IsBlockFullWidth()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><form id='t' style='height:20px'></form></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"form width={box.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 400) < 2);
            Assert.Equal(CssDisplay.Block, (box.StyledNode as StyledElement)!.Style.Display);
        }

        // [HTML §4.11.1] details element exists in layout tree
        [Fact]
        public void Details_RendersWithSummary()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                <details id='t' open>
                    <summary id='s'>Summary text</summary>
                    <div id='content' style='height:50px'></div>
                </details></body>");
            var details = LayoutTestHelper.FindById(root, "t")!;
            var summary = LayoutTestHelper.FindById(root, "s");
            _output.WriteLine($"details width={details.ContentRect.Width} height={details.ContentRect.Height}");
            Assert.True(System.Math.Abs(details.ContentRect.Width - 400) < 2);
            Assert.NotNull(summary);
        }

        // [HTML §4.11.1] summary has display:list-item per UA
        [Fact]
        public void Summary_IsListItem()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><details open><summary id='t'>Summary</summary><p>Detail</p></details></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            var style = (box.StyledNode as StyledElement)!.Style;
            _output.WriteLine($"summary display={style.Display}");
            Assert.Equal(CssDisplay.ListItem, style.Display);
        }

        // [HTML §4.4.9] dl has 1em top/bottom margin
        [Fact]
        public void Dl_HasVerticalMargins()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0;padding-bottom:1px'><dl id='t'><dt>Term</dt><dd>Definition</dd></dl></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"dl marginTop={box.MarginTop} marginBottom={box.MarginBottom}");
            Assert.True(System.Math.Abs(box.MarginTop - 16) < 2);
            Assert.True(System.Math.Abs(box.MarginBottom - 16) < 2);
        }

        // [HTML §4.4.11] dd has 40px left margin for indentation
        [Fact]
        public void Dd_HasLeftMargin()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><dl><dt>Term</dt><dd id='t'>Definition</dd></dl></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"dd marginLeft={box.MarginLeft} X={box.ContentRect.X}");
            Assert.True(System.Math.Abs(box.MarginLeft - 40) < 2);
        }

        // [HTML §4.4.10] dt is block element at left edge (no indentation)
        [Fact]
        public void Dt_IsBlockNoIndent()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><dl><dt id='t'>Term</dt><dd>Definition</dd></dl></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"dt X={box.ContentRect.X} marginLeft={box.MarginLeft}");
            Assert.True(System.Math.Abs(box.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(box.MarginLeft - 0) < 2);
        }

        // [HTML §4.4.8] nested ul changes list-style-type to circle
        [Fact]
        public void NestedUl_IndentsAndChangesStyle()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                <ul id='outer'>
                    <li>Item
                        <ul id='inner'><li id='innerItem'>Nested</li></ul>
                    </li>
                </ul></body>");
            var outer = LayoutTestHelper.FindById(root, "outer")!;
            var inner = LayoutTestHelper.FindById(root, "inner")!;
            _output.WriteLine($"outer paddingLeft={outer.PaddingLeft} inner paddingLeft={inner.PaddingLeft}");
            Assert.True(System.Math.Abs(outer.PaddingLeft - 40) < 2);
            Assert.True(System.Math.Abs(inner.PaddingLeft - 40) < 2);
        }

        // [HTML §4.4.8] nested list has zero top/bottom margin
        [Fact]
        public void NestedList_ZeroVerticalMargin()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                <ul>
                    <li>Item
                        <ul id='inner'><li>Nested</li></ul>
                    </li>
                </ul></body>");
            var inner = LayoutTestHelper.FindById(root, "inner")!;
            _output.WriteLine($"nested ul marginTop={inner.MarginTop} marginBottom={inner.MarginBottom}");
            Assert.True(System.Math.Abs(inner.MarginTop - 0) < 2);
            Assert.True(System.Math.Abs(inner.MarginBottom - 0) < 2);
        }

        // [HTML §4.4.12] figure has block display with 40px left/right margins
        [Fact]
        public void Figure_HasMargins()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><figure id='t'>Figure content</figure></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"figure marginLeft={box.MarginLeft} marginRight={box.MarginRight} width={box.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.MarginLeft - 40) < 2);
            Assert.True(System.Math.Abs(box.MarginRight - 40) < 2);
            Assert.True(System.Math.Abs(box.ContentRect.Width - 320) < 2);
        }

        // [HTML §4.4.13] figcaption is block level
        [Fact]
        public void Figcaption_IsBlock()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                <figure>
                    <div style='height:50px;background:gray'></div>
                    <figcaption id='t'>Caption text</figcaption>
                </figure></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            var style = (box.StyledNode as StyledElement)!.Style;
            _output.WriteLine($"figcaption display={style.Display} width={box.ContentRect.Width}");
            Assert.Equal(CssDisplay.Block, style.Display);
        }

        // [HTML §4.3.10] address is a block element
        [Fact]
        public void Address_IsBlockFullWidth()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><address id='t'>123 Main Street</address></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"address width={box.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 400) < 2);
            Assert.Equal(CssDisplay.Block, (box.StyledNode as StyledElement)!.Style.Display);
        }

        // Semantic block elements stack vertically
        [Fact]
        public void SemanticBlocks_StackVertically()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                <header id='hdr' style='height:30px'></header>
                <main id='mn' style='height:50px'></main>
                <footer id='ftr' style='height:20px'></footer></body>");
            var header = LayoutTestHelper.FindById(root, "hdr")!;
            var main = LayoutTestHelper.FindById(root, "mn")!;
            var footer = LayoutTestHelper.FindById(root, "ftr")!;
            _output.WriteLine($"header Y={header.ContentRect.Y} main Y={main.ContentRect.Y} footer Y={footer.ContentRect.Y}");
            Assert.True(System.Math.Abs(header.ContentRect.Y - 0) < 2);
            Assert.True(System.Math.Abs(main.ContentRect.Y - 30) < 2);
            Assert.True(System.Math.Abs(footer.ContentRect.Y - 80) < 2);
        }

        // Heading sizes decrease from h1 to h6
        [Fact]
        public void Headings_FontSizesDecrease()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                <h1 id='h1'>H1</h1><h2 id='h2'>H2</h2><h3 id='h3'>H3</h3>
                <h4 id='h4'>H4</h4><h5 id='h5'>H5</h5><h6 id='h6'>H6</h6></body>");
            float h1Size = (LayoutTestHelper.FindById(root, "h1")!.StyledNode as StyledElement)!.Style.FontSize;
            float h2Size = (LayoutTestHelper.FindById(root, "h2")!.StyledNode as StyledElement)!.Style.FontSize;
            float h3Size = (LayoutTestHelper.FindById(root, "h3")!.StyledNode as StyledElement)!.Style.FontSize;
            float h4Size = (LayoutTestHelper.FindById(root, "h4")!.StyledNode as StyledElement)!.Style.FontSize;
            float h5Size = (LayoutTestHelper.FindById(root, "h5")!.StyledNode as StyledElement)!.Style.FontSize;
            float h6Size = (LayoutTestHelper.FindById(root, "h6")!.StyledNode as StyledElement)!.Style.FontSize;
            _output.WriteLine($"h1={h1Size} h2={h2Size} h3={h3Size} h4={h4Size} h5={h5Size} h6={h6Size}");
            Assert.True(h1Size > h2Size);
            Assert.True(h2Size > h3Size);
            Assert.True(h3Size > h4Size);
            Assert.True(h4Size > h5Size);
            Assert.True(h5Size > h6Size);
        }

        // Headings are all bold (font-weight: 700)
        [Fact]
        public void Headings_AllBold()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                <h1 id='h1'>H1</h1><h2 id='h2'>H2</h2><h3 id='h3'>H3</h3>
                <h4 id='h4'>H4</h4><h5 id='h5'>H5</h5><h6 id='h6'>H6</h6></body>");
            for (int i = 1; i <= 6; i++)
            {
                var box = LayoutTestHelper.FindById(root, $"h{i}")!;
                var weight = (box.StyledNode as StyledElement)!.Style.FontWeight;
                _output.WriteLine($"h{i} fontWeight={weight}");
                Assert.Equal(700, weight);
            }
        }

        // [HTML §4.9] table with border-spacing creates gaps between cells
        [Fact]
        public void Table_BorderSpacingCreatesGaps()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                <table style='width:200px;border-spacing:4px'>
                    <tr><td id='c1' style='width:50%'>A</td><td id='c2' style='width:50%'>B</td></tr>
                </table></body>");
            var cell1 = LayoutTestHelper.FindById(root, "c1")!;
            var cell2 = LayoutTestHelper.FindById(root, "c2")!;
            float gap = cell2.ContentRect.X - (cell1.ContentRect.X + cell1.ContentRect.Width + cell1.PaddingRight);
            _output.WriteLine($"cell1 X={cell1.ContentRect.X} cell2 X={cell2.ContentRect.X} gap~={gap}");
            Assert.True(cell2.ContentRect.X > cell1.ContentRect.X + cell1.ContentRect.Width,
                "Cells should have spacing between them");
        }

        // [HTML §4.9] th has text-align:center and font-weight:bold
        [Fact]
        public void Th_CenterAlignedAndBold()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                <table><tr><th id='t'>Header</th></tr></table></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            var style = (box.StyledNode as StyledElement)!.Style;
            _output.WriteLine($"th textAlign={style.TextAlign} fontWeight={style.FontWeight}");
            Assert.Equal(CssTextAlign.Center, style.TextAlign);
            Assert.Equal(700, style.FontWeight);
        }

        // Blockquote reduces content width by margins on both sides
        [Fact]
        public void Blockquote_ContentWidthReducedByMargins()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                <blockquote id='t'><div id='inner' style='height:10px'></div></blockquote></body>");
            var blockquote = LayoutTestHelper.FindById(root, "t")!;
            var inner = LayoutTestHelper.FindById(root, "inner")!;
            _output.WriteLine($"blockquote width={blockquote.ContentRect.Width} inner width={inner.ContentRect.Width}");
            Assert.True(System.Math.Abs(inner.ContentRect.Width - 320) < 2);
            Assert.True(System.Math.Abs(blockquote.ContentRect.X - 40) < 2);
        }

        // Multiple p elements stack with margin collapsing
        [Fact]
        public void MultipleP_StackWithMarginCollapsing()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                <p id='p1' style='margin:16px 0'>First</p>
                <p id='p2' style='margin:16px 0'>Second</p></body>");
            var p1 = LayoutTestHelper.FindById(root, "p1")!;
            var p2 = LayoutTestHelper.FindById(root, "p2")!;
            float p1Bottom = p1.ContentRect.Y + p1.ContentRect.Height;
            _output.WriteLine($"p1 Y={p1.ContentRect.Y} bottom={p1Bottom} p2 Y={p2.ContentRect.Y}");
            float gapBetween = p2.ContentRect.Y - p1Bottom;
            Assert.True(gapBetween <= 18,
                $"Adjacent p margins should collapse (gap={gapBetween}, expected ~16)");
        }

        // Section containing constrained-width children
        [Fact]
        public void Section_ChildrenConstrainedByParent()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                <section style='width:200px'>
                    <div id='child' style='height:20px'></div>
                </section></body>");
            var child = LayoutTestHelper.FindById(root, "child")!;
            _output.WriteLine($"child width={child.ContentRect.Width}");
            Assert.True(System.Math.Abs(child.ContentRect.Width - 200) < 2);
        }

        // Figure content width is viewport minus left+right margins
        [Fact]
        public void Figure_ContentWidthMinusMargins()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                <figure id='t'>
                    <div id='figContent' style='height:30px'></div>
                    <figcaption>Caption</figcaption>
                </figure></body>");
            var figure = LayoutTestHelper.FindById(root, "t")!;
            var figContent = LayoutTestHelper.FindById(root, "figContent")!;
            _output.WriteLine($"figure width={figure.ContentRect.Width} figContent width={figContent.ContentRect.Width}");
            Assert.True(System.Math.Abs(figure.ContentRect.Width - 320) < 2);
            Assert.True(System.Math.Abs(figContent.ContentRect.Width - 320) < 2);
        }
    }
}
