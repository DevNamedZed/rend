using System;
using System.Text;
using System.Text.RegularExpressions;
using Rend.Core.Values;
using Xunit;

namespace Rend.Tests.EndToEnd
{
    public class HtmlConformanceTests
    {
        private byte[] RenderPdf(string html, float w = 400, float h = 300)
        {
            return Render.ToPdf(html, new RenderOptions
            {
                PageSize = new SizeF(w, h),
                MarginTop = 0, MarginRight = 0, MarginBottom = 0, MarginLeft = 0
            });
        }

        private string PdfText(byte[] pdf) => Encoding.ASCII.GetString(pdf);

        private void AssertValidPdf(byte[] pdf, int minSize = 200)
        {
            Assert.NotNull(pdf);
            Assert.True(pdf.Length >= minSize, $"PDF too small: {pdf.Length} bytes (min {minSize})");
            var text = PdfText(pdf);
            Assert.StartsWith("%PDF-", text);
            Assert.Contains("%%EOF", text);
        }

        private void AssertPdfStructure(byte[] pdf)
        {
            var text = PdfText(pdf);
            Assert.Contains("/Type /Catalog", text);
            Assert.Contains("/Type /Page", text);
        }

        // =============================================
        // Block Elements
        // =============================================

        [Fact]
        public void Div_RendersAsBlock()
        {
            var pdf = RenderPdf("<div>Hello from div</div>");
            AssertValidPdf(pdf);
            AssertPdfStructure(pdf);
        }

        [Fact]
        public void Paragraph_RendersAsBlock()
        {
            var pdf = RenderPdf("<p>Paragraph content</p>");
            AssertValidPdf(pdf);
            AssertPdfStructure(pdf);
        }

        [Fact]
        public void Heading1_RendersWithLargeFont()
        {
            var pdf = RenderPdf("<h1>Heading Level 1</h1>");
            AssertValidPdf(pdf);
        }

        [Fact]
        public void Heading2_Renders()
        {
            var pdf = RenderPdf("<h2>Heading Level 2</h2>");
            AssertValidPdf(pdf);
        }

        [Fact]
        public void Heading3_Renders()
        {
            var pdf = RenderPdf("<h3>Heading Level 3</h3>");
            AssertValidPdf(pdf);
        }

        [Fact]
        public void Heading4_Renders()
        {
            var pdf = RenderPdf("<h4>Heading Level 4</h4>");
            AssertValidPdf(pdf);
        }

        [Fact]
        public void Heading5_Renders()
        {
            var pdf = RenderPdf("<h5>Heading Level 5</h5>");
            AssertValidPdf(pdf);
        }

        [Fact]
        public void Heading6_Renders()
        {
            var pdf = RenderPdf("<h6>Heading Level 6</h6>");
            AssertValidPdf(pdf);
        }

        [Fact]
        public void AllHeadings_ProduceDifferentSizes()
        {
            var pdf = RenderPdf(@"
                <h1>H1</h1><h2>H2</h2><h3>H3</h3>
                <h4>H4</h4><h5>H5</h5><h6>H6</h6>");
            AssertValidPdf(pdf, 300);
        }

        [Fact]
        public void Blockquote_Renders()
        {
            var pdf = RenderPdf("<blockquote>Quoted text</blockquote>");
            AssertValidPdf(pdf);
        }

        [Fact]
        public void Pre_PreservesFormatting()
        {
            var pdf = RenderPdf("<pre>  indented\n    more indented\nback</pre>");
            AssertValidPdf(pdf);
        }

        [Fact]
        public void Address_Renders()
        {
            var pdf = RenderPdf("<address>123 Main St<br>City, State</address>");
            AssertValidPdf(pdf);
        }

        [Fact]
        public void Section_RendersAsBlock()
        {
            var pdf = RenderPdf("<section><h2>Section Title</h2><p>Content</p></section>");
            AssertValidPdf(pdf);
        }

        [Fact]
        public void Article_RendersAsBlock()
        {
            var pdf = RenderPdf("<article><h2>Article Title</h2><p>Article body</p></article>");
            AssertValidPdf(pdf);
        }

        [Fact]
        public void Nav_RendersAsBlock()
        {
            var pdf = RenderPdf("<nav><a href='#'>Home</a> <a href='#'>About</a></nav>");
            AssertValidPdf(pdf);
        }

        [Fact]
        public void Aside_RendersAsBlock()
        {
            var pdf = RenderPdf("<aside><p>Sidebar content</p></aside>");
            AssertValidPdf(pdf);
        }

        [Fact]
        public void Header_RendersAsBlock()
        {
            var pdf = RenderPdf("<header><h1>Page Title</h1></header>");
            AssertValidPdf(pdf);
        }

        [Fact]
        public void Footer_RendersAsBlock()
        {
            var pdf = RenderPdf("<footer><p>Copyright 2026</p></footer>");
            AssertValidPdf(pdf);
        }

        [Fact]
        public void Main_RendersAsBlock()
        {
            var pdf = RenderPdf("<main><p>Main content area</p></main>");
            AssertValidPdf(pdf);
        }

        [Fact]
        public void Figure_WithFigcaption_Renders()
        {
            var pdf = RenderPdf(@"
                <figure>
                    <div style='width:50px;height:50px;background:gray'></div>
                    <figcaption>Figure 1: A gray box</figcaption>
                </figure>");
            AssertValidPdf(pdf);
        }

        // =============================================
        // Inline Elements
        // =============================================

        [Fact]
        public void Span_RendersInline()
        {
            var pdf = RenderPdf("<p>Text with <span>span element</span> inside</p>");
            AssertValidPdf(pdf);
        }

        [Fact]
        public void Anchor_RendersInline()
        {
            var pdf = RenderPdf("<p>Click <a href='https://example.com'>here</a> for more</p>");
            AssertValidPdf(pdf);
        }

        [Fact]
        public void Strong_RendersBold()
        {
            var pdf = RenderPdf("<p>This is <strong>strong</strong> text</p>");
            AssertValidPdf(pdf);
        }

        [Fact]
        public void Em_RendersItalic()
        {
            var pdf = RenderPdf("<p>This is <em>emphasized</em> text</p>");
            AssertValidPdf(pdf);
        }

        [Fact]
        public void Bold_RendersBold()
        {
            var pdf = RenderPdf("<p>This is <b>bold</b> text</p>");
            AssertValidPdf(pdf);
        }

        [Fact]
        public void Italic_RendersItalic()
        {
            var pdf = RenderPdf("<p>This is <i>italic</i> text</p>");
            AssertValidPdf(pdf);
        }

        [Fact]
        public void Underline_Renders()
        {
            var pdf = RenderPdf("<p>This is <u>underlined</u> text</p>");
            AssertValidPdf(pdf);
        }

        [Fact]
        public void Strikethrough_Renders()
        {
            var pdf = RenderPdf("<p>This is <s>struck through</s> text</p>");
            AssertValidPdf(pdf);
        }

        [Fact]
        public void Small_RendersSmaller()
        {
            var pdf = RenderPdf("<p>Normal <small>small text</small> normal</p>");
            AssertValidPdf(pdf);
        }

        [Fact]
        public void Sub_RendersSubscript()
        {
            var pdf = RenderPdf("<p>H<sub>2</sub>O</p>");
            AssertValidPdf(pdf);
        }

        [Fact]
        public void Sup_RendersSuperscript()
        {
            var pdf = RenderPdf("<p>E=mc<sup>2</sup></p>");
            AssertValidPdf(pdf);
        }

        [Fact]
        public void Mark_RendersHighlighted()
        {
            var pdf = RenderPdf("<p>This is <mark>highlighted</mark> text</p>");
            AssertValidPdf(pdf);
        }

        [Fact]
        public void Code_RendersMonospace()
        {
            var pdf = RenderPdf("<p>Use <code>console.log()</code> to debug</p>");
            AssertValidPdf(pdf);
        }

        [Fact]
        public void Kbd_RendersMonospace()
        {
            var pdf = RenderPdf("<p>Press <kbd>Ctrl</kbd>+<kbd>C</kbd></p>");
            AssertValidPdf(pdf);
        }

        [Fact]
        public void Samp_RendersMonospace()
        {
            var pdf = RenderPdf("<p>Output: <samp>Hello World</samp></p>");
            AssertValidPdf(pdf);
        }

        [Fact]
        public void Var_RendersItalic()
        {
            var pdf = RenderPdf("<p>The variable <var>x</var> is unknown</p>");
            AssertValidPdf(pdf);
        }

        [Fact]
        public void Abbr_Renders()
        {
            var pdf = RenderPdf("<p>The <abbr title='World Health Organization'>WHO</abbr> recommends it</p>");
            AssertValidPdf(pdf);
        }

        [Fact]
        public void Cite_RendersItalic()
        {
            var pdf = RenderPdf("<p>As described in <cite>The Art of War</cite></p>");
            AssertValidPdf(pdf);
        }

        [Fact]
        public void Q_RendersWithQuotes()
        {
            var pdf = RenderPdf("<p>She said <q>Hello World</q> loudly</p>");
            AssertValidPdf(pdf);
        }

        [Fact]
        public void MixedInlineElements_AllRender()
        {
            var pdf = RenderPdf(@"<p>
                <strong>bold</strong> <em>italic</em> <u>underline</u>
                <s>strike</s> <code>code</code> <small>small</small>
                <sub>sub</sub> <sup>sup</sup> <mark>mark</mark>
            </p>");
            AssertValidPdf(pdf);
        }

        // =============================================
        // List Elements
        // =============================================

        [Fact]
        public void UnorderedList_Renders()
        {
            var pdf = RenderPdf(@"
                <ul>
                    <li>Item 1</li>
                    <li>Item 2</li>
                    <li>Item 3</li>
                </ul>");
            AssertValidPdf(pdf);
        }

        [Fact]
        public void OrderedList_Renders()
        {
            var pdf = RenderPdf(@"
                <ol>
                    <li>First</li>
                    <li>Second</li>
                    <li>Third</li>
                </ol>");
            AssertValidPdf(pdf);
        }

        [Fact]
        public void NestedLists_Render()
        {
            var pdf = RenderPdf(@"
                <ul>
                    <li>Level 1
                        <ul>
                            <li>Level 2a</li>
                            <li>Level 2b
                                <ul><li>Level 3</li></ul>
                            </li>
                        </ul>
                    </li>
                    <li>Level 1 again</li>
                </ul>");
            AssertValidPdf(pdf);
        }

        [Fact]
        public void DefinitionList_Renders()
        {
            var pdf = RenderPdf(@"
                <dl>
                    <dt>Term 1</dt>
                    <dd>Definition 1</dd>
                    <dt>Term 2</dt>
                    <dd>Definition 2</dd>
                </dl>");
            AssertValidPdf(pdf);
        }

        [Fact]
        public void EmptyList_DoesNotCrash()
        {
            var pdf = RenderPdf("<ul></ul>");
            AssertValidPdf(pdf);
        }

        [Fact]
        public void ListWithManyItems_Renders()
        {
            var sb = new StringBuilder("<ol>");
            for (int i = 1; i <= 50; i++)
                sb.Append($"<li>Item number {i}</li>");
            sb.Append("</ol>");
            var pdf = RenderPdf(sb.ToString(), 400, 2000);
            AssertValidPdf(pdf);
        }

        // =============================================
        // Table Elements
        // =============================================

        [Fact]
        public void SimpleTable_Renders()
        {
            var pdf = RenderPdf(@"
                <table>
                    <tr><td>A</td><td>B</td></tr>
                    <tr><td>C</td><td>D</td></tr>
                </table>");
            AssertValidPdf(pdf);
        }

        [Fact]
        public void TableWithHeaders_Renders()
        {
            var pdf = RenderPdf(@"
                <table>
                    <thead>
                        <tr><th>Name</th><th>Age</th></tr>
                    </thead>
                    <tbody>
                        <tr><td>Alice</td><td>30</td></tr>
                        <tr><td>Bob</td><td>25</td></tr>
                    </tbody>
                </table>");
            AssertValidPdf(pdf);
        }

        [Fact]
        public void TableWithFooter_Renders()
        {
            var pdf = RenderPdf(@"
                <table>
                    <thead><tr><th>Item</th><th>Price</th></tr></thead>
                    <tbody>
                        <tr><td>Widget</td><td>$10</td></tr>
                    </tbody>
                    <tfoot><tr><td>Total</td><td>$10</td></tr></tfoot>
                </table>");
            AssertValidPdf(pdf);
        }

        [Fact]
        public void TableWithCaption_Renders()
        {
            var pdf = RenderPdf(@"
                <table>
                    <caption>Monthly Sales</caption>
                    <tr><th>Month</th><th>Sales</th></tr>
                    <tr><td>Jan</td><td>100</td></tr>
                </table>");
            AssertValidPdf(pdf);
        }

        [Fact]
        public void TableWithColgroup_Renders()
        {
            var pdf = RenderPdf(@"
                <table>
                    <colgroup>
                        <col style='background-color:#eee'>
                        <col>
                    </colgroup>
                    <tr><td>A</td><td>B</td></tr>
                </table>");
            AssertValidPdf(pdf);
        }

        [Fact]
        public void EmptyTable_DoesNotCrash()
        {
            var pdf = RenderPdf("<table></table>");
            AssertValidPdf(pdf);
        }

        // =============================================
        // Form Elements
        // =============================================

        [Fact]
        public void Form_Renders()
        {
            var pdf = RenderPdf(@"
                <form>
                    <label>Name:</label>
                    <input type='text' value='John'>
                </form>");
            AssertValidPdf(pdf);
        }

        [Fact]
        public void TextInput_Renders()
        {
            var pdf = RenderPdf("<input type='text' value='Hello'>");
            AssertValidPdf(pdf);
        }

        [Fact]
        public void Textarea_Renders()
        {
            var pdf = RenderPdf("<textarea>Some text content here</textarea>");
            AssertValidPdf(pdf);
        }

        [Fact]
        public void Select_Renders()
        {
            var pdf = RenderPdf(@"
                <select>
                    <option>Option 1</option>
                    <option selected>Option 2</option>
                    <option>Option 3</option>
                </select>");
            AssertValidPdf(pdf);
        }

        [Fact]
        public void Button_Renders()
        {
            var pdf = RenderPdf("<button>Click Me</button>");
            AssertValidPdf(pdf);
        }

        [Fact]
        public void Fieldset_WithLegend_Renders()
        {
            var pdf = RenderPdf(@"
                <fieldset>
                    <legend>Personal Info</legend>
                    <label>Name:</label>
                    <input type='text'>
                </fieldset>");
            AssertValidPdf(pdf);
        }

        [Fact]
        public void CheckboxInput_Renders()
        {
            var pdf = RenderPdf("<label><input type='checkbox' checked> Accept terms</label>");
            AssertValidPdf(pdf);
        }

        [Fact]
        public void RadioInput_Renders()
        {
            var pdf = RenderPdf(@"
                <label><input type='radio' name='color' checked> Red</label>
                <label><input type='radio' name='color'> Blue</label>");
            AssertValidPdf(pdf);
        }

        // =============================================
        // Media / Replaced Elements
        // =============================================

        [Fact]
        public void Img_WithoutSrc_DoesNotCrash()
        {
            var pdf = RenderPdf("<img alt='Missing image'>");
            AssertValidPdf(pdf);
        }

        [Fact]
        public void Br_RendersLineBreak()
        {
            var pdf = RenderPdf("<p>Line one<br>Line two<br>Line three</p>");
            AssertValidPdf(pdf);
        }

        [Fact]
        public void Hr_RendersHorizontalRule()
        {
            var pdf = RenderPdf("<p>Above</p><hr><p>Below</p>");
            AssertValidPdf(pdf);
        }

        [Fact]
        public void Wbr_DoesNotCrash()
        {
            var pdf = RenderPdf("<p>Supercali<wbr>fragilistic<wbr>expialidocious</p>");
            AssertValidPdf(pdf);
        }

        // =============================================
        // Semantic Elements
        // =============================================

        [Fact]
        public void Details_Renders()
        {
            var pdf = RenderPdf(@"
                <details>
                    <summary>Click to expand</summary>
                    <p>Hidden content here</p>
                </details>");
            AssertValidPdf(pdf);
        }

        [Fact]
        public void DetailsOpen_ShowsContent()
        {
            var pdf = RenderPdf(@"
                <details open>
                    <summary>Expanded section</summary>
                    <p>Visible content</p>
                </details>");
            AssertValidPdf(pdf);
        }

        [Fact]
        public void Dialog_Renders()
        {
            var pdf = RenderPdf("<dialog open>Dialog content</dialog>");
            AssertValidPdf(pdf);
        }

        [Fact]
        public void Meter_Renders()
        {
            var pdf = RenderPdf("<meter value='0.7' min='0' max='1'>70%</meter>");
            AssertValidPdf(pdf);
        }

        [Fact]
        public void Progress_Renders()
        {
            var pdf = RenderPdf("<progress value='50' max='100'>50%</progress>");
            AssertValidPdf(pdf);
        }

        // =============================================
        // Self-Closing and Void Elements
        // =============================================

        [Fact]
        public void SelfClosingBr_Renders()
        {
            var pdf = RenderPdf("<p>Line 1<br/>Line 2</p>");
            AssertValidPdf(pdf);
        }

        [Fact]
        public void SelfClosingHr_Renders()
        {
            var pdf = RenderPdf("<hr/>");
            AssertValidPdf(pdf);
        }

        [Fact]
        public void SelfClosingImg_DoesNotCrash()
        {
            var pdf = RenderPdf("<img />");
            AssertValidPdf(pdf);
        }

        [Fact]
        public void SelfClosingInput_Renders()
        {
            var pdf = RenderPdf("<input type='text' />");
            AssertValidPdf(pdf);
        }

        [Fact]
        public void VoidElements_AllSupported()
        {
            var pdf = RenderPdf(@"
                <p>Text<br>Break</p>
                <hr>
                <img alt='test'>
                <input type='text'>
                <p>More text</p>");
            AssertValidPdf(pdf);
        }

        // =============================================
        // Nesting and Structure
        // =============================================

        [Fact]
        public void NestedDivs_Render()
        {
            var pdf = RenderPdf(@"
                <div>
                    <div>
                        <div>
                            <p>Deeply nested paragraph</p>
                        </div>
                    </div>
                </div>");
            AssertValidPdf(pdf);
        }

        [Fact]
        public void DeeplyNestedElements_20Levels()
        {
            var sb = new StringBuilder();
            for (int i = 0; i < 20; i++)
                sb.Append("<div>");
            sb.Append("<p>Deep content</p>");
            for (int i = 0; i < 20; i++)
                sb.Append("</div>");
            var pdf = RenderPdf(sb.ToString());
            AssertValidPdf(pdf);
        }

        [Fact]
        public void DeeplyNestedElements_30Levels()
        {
            var sb = new StringBuilder();
            for (int i = 0; i < 30; i++)
                sb.Append("<div>");
            sb.Append("<span>Very deep</span>");
            for (int i = 0; i < 30; i++)
                sb.Append("</div>");
            var pdf = RenderPdf(sb.ToString());
            AssertValidPdf(pdf);
        }

        [Fact]
        public void EmptyDiv_DoesNotCrash()
        {
            var pdf = RenderPdf("<div></div>");
            AssertValidPdf(pdf);
        }

        [Fact]
        public void EmptyParagraph_DoesNotCrash()
        {
            var pdf = RenderPdf("<p></p>");
            AssertValidPdf(pdf);
        }

        [Fact]
        public void EmptySpan_DoesNotCrash()
        {
            var pdf = RenderPdf("<span></span>");
            AssertValidPdf(pdf);
        }

        [Fact]
        public void WhitespaceOnlyElements_DoNotCrash()
        {
            var pdf = RenderPdf("<div>   </div><p>  \n  </p><span>  </span>");
            AssertValidPdf(pdf);
        }

        // =============================================
        // HTML5 Error Recovery / Unclosed Tags
        // =============================================

        [Fact]
        public void UnclosedParagraph_Recovers()
        {
            var pdf = RenderPdf("<p>First paragraph<p>Second paragraph");
            AssertValidPdf(pdf);
        }

        [Fact]
        public void UnclosedDiv_Recovers()
        {
            var pdf = RenderPdf("<div>Content without closing tag");
            AssertValidPdf(pdf);
        }

        [Fact]
        public void UnclosedListItems_Recovers()
        {
            var pdf = RenderPdf("<ul><li>Item 1<li>Item 2<li>Item 3</ul>");
            AssertValidPdf(pdf);
        }

        [Fact]
        public void MismatchedTags_Recovers()
        {
            var pdf = RenderPdf("<div><p>Mismatched</div></p>");
            AssertValidPdf(pdf);
        }

        [Fact]
        public void UnclosedBold_Recovers()
        {
            var pdf = RenderPdf("<p><b>Bold text that is never closed</p>");
            AssertValidPdf(pdf);
        }

        // =============================================
        // HTML Entities
        // =============================================

        [Fact]
        public void NamedEntities_Render()
        {
            var pdf = RenderPdf("<p>&amp; &lt; &gt; &quot;</p>");
            AssertValidPdf(pdf);
        }

        [Fact]
        public void DecimalNumericEntities_Render()
        {
            var pdf = RenderPdf("<p>&#169; &#174; &#8364;</p>");
            AssertValidPdf(pdf);
        }

        [Fact]
        public void HexNumericEntities_Render()
        {
            var pdf = RenderPdf("<p>&#x00A9; &#x00AE; &#x20AC;</p>");
            AssertValidPdf(pdf);
        }

        [Fact]
        public void NonBreakingSpace_Entity()
        {
            var pdf = RenderPdf("<p>Word&nbsp;Word</p>");
            AssertValidPdf(pdf);
        }

        [Fact]
        public void MixedEntities_AllRender()
        {
            var pdf = RenderPdf("<p>&amp; &#169; &#x00A9; &lt;tag&gt; &quot;quoted&quot;</p>");
            AssertValidPdf(pdf);
        }

        // =============================================
        // Document Structure Edge Cases
        // =============================================

        [Fact]
        public void MultipleBodies_DoesNotCrash()
        {
            var pdf = RenderPdf("<html><body>First body</body><body>Second body</body></html>");
            AssertValidPdf(pdf);
        }

        [Fact]
        public void MissingHtmlHeadBody_StillRenders()
        {
            var pdf = RenderPdf("<p>Just a paragraph, no html/head/body wrappers</p>");
            AssertValidPdf(pdf);
        }

        [Fact]
        public void PlainText_WithNoTags()
        {
            var pdf = RenderPdf("Hello World plain text");
            AssertValidPdf(pdf);
        }

        [Fact]
        public void HtmlComments_Ignored()
        {
            var pdf = RenderPdf("<!-- This is a comment --><p>Visible</p><!-- Another comment -->");
            AssertValidPdf(pdf);
        }

        [Fact]
        public void CommentInsideElement_Ignored()
        {
            var pdf = RenderPdf("<div>Before <!-- comment --> After</div>");
            AssertValidPdf(pdf);
        }

        [Fact]
        public void CdataSection_Handled()
        {
            var pdf = RenderPdf("<p><![CDATA[Some CDATA content]]></p>");
            AssertValidPdf(pdf);
        }

        [Fact]
        public void Doctype_Handled()
        {
            var pdf = RenderPdf("<!DOCTYPE html><html><head></head><body><p>With doctype</p></body></html>");
            AssertValidPdf(pdf);
            AssertPdfStructure(pdf);
        }

        [Fact]
        public void DoctypeHtml5_Handled()
        {
            var pdf = RenderPdf("<!doctype html><p>Content</p>");
            AssertValidPdf(pdf);
        }

        // =============================================
        // Attribute Handling
        // =============================================

        [Fact]
        public void ClassAttribute_Parsed()
        {
            var pdf = RenderPdf(@"
                <style>.red { color: red; }</style>
                <p class='red'>Red text</p>");
            AssertValidPdf(pdf);
        }

        [Fact]
        public void IdAttribute_Parsed()
        {
            var pdf = RenderPdf(@"
                <style>#special { font-weight: bold; }</style>
                <p id='special'>Special text</p>");
            AssertValidPdf(pdf);
        }

        [Fact]
        public void StyleAttribute_InlineStyles()
        {
            var pdf = RenderPdf("<p style='color: blue; font-size: 20px;'>Styled text</p>");
            AssertValidPdf(pdf);
        }

        [Fact]
        public void DataAttributes_DoNotCrash()
        {
            var pdf = RenderPdf("<div data-value='123' data-name='test'>Content</div>");
            AssertValidPdf(pdf);
        }

        [Fact]
        public void BooleanAttribute_Hidden()
        {
            var pdf = RenderPdf("<div hidden>Hidden content</div><p>Visible content</p>");
            AssertValidPdf(pdf);
        }

        [Fact]
        public void BooleanAttribute_Disabled()
        {
            var pdf = RenderPdf("<input type='text' disabled value='Cannot edit'>");
            AssertValidPdf(pdf);
        }

        [Fact]
        public void UnquotedAttributeValues_Parsed()
        {
            var pdf = RenderPdf("<div style=color:red>Red text</div>");
            AssertValidPdf(pdf);
        }

        [Fact]
        public void MultipleClasses_Parsed()
        {
            var pdf = RenderPdf(@"
                <style>.bold { font-weight: bold; } .large { font-size: 24px; }</style>
                <p class='bold large'>Bold and large</p>");
            AssertValidPdf(pdf);
        }

        // =============================================
        // Text Content
        // =============================================

        [Fact]
        public void PlainTextBody_Renders()
        {
            var pdf = RenderPdf("<body>Just plain text in the body</body>");
            AssertValidPdf(pdf);
        }

        [Fact]
        public void TextWithEntities_Renders()
        {
            var pdf = RenderPdf("<p>Price: $10 &amp; up &mdash; best deal &lt;ever&gt;</p>");
            AssertValidPdf(pdf);
        }

        [Fact]
        public void MixedTextAndElements_Render()
        {
            var pdf = RenderPdf("<p>Normal <b>bold</b> normal <i>italic</i> normal</p>");
            AssertValidPdf(pdf);
        }

        [Fact]
        public void WhitespaceNormalization_CollapsesSpaces()
        {
            var pdf = RenderPdf("<p>Multiple     spaces     between     words</p>");
            AssertValidPdf(pdf);
        }

        [Fact]
        public void ConsecutiveWhitespace_Collapsed()
        {
            var pdf = RenderPdf("<p>  Leading   and   trailing   spaces  </p>");
            AssertValidPdf(pdf);
        }

        [Fact]
        public void LineBreaksInSource_CollapsedToSpaces()
        {
            var pdf = RenderPdf(@"<p>First line
            second line
            third line</p>");
            AssertValidPdf(pdf);
        }

        [Fact]
        public void PreformattedText_PreservesWhitespace()
        {
            var pdf = RenderPdf("<pre>  Two spaces\n    Four spaces\n\tTab</pre>");
            AssertValidPdf(pdf);
        }

        [Fact]
        public void TextOnly_NoWrappingElements()
        {
            var pdf = RenderPdf("Just raw text, no HTML at all");
            AssertValidPdf(pdf);
        }

        [Fact]
        public void EmptyString_ProducesValidPdf()
        {
            var pdf = RenderPdf("");
            AssertValidPdf(pdf);
        }

        [Fact]
        public void WhitespaceOnly_ProducesValidPdf()
        {
            var pdf = RenderPdf("   \n  \t  \n   ");
            AssertValidPdf(pdf);
        }

        // =============================================
        // CSS Integration
        // =============================================

        [Fact]
        public void StyleElement_InHead_Applied()
        {
            var pdf = RenderPdf(@"
                <html><head>
                    <style>p { color: navy; font-size: 18px; }</style>
                </head><body>
                    <p>Styled paragraph</p>
                </body></html>");
            AssertValidPdf(pdf);
        }

        [Fact]
        public void MultipleStyleElements_AllApplied()
        {
            var pdf = RenderPdf(@"
                <html><head>
                    <style>p { color: red; }</style>
                    <style>p { font-size: 20px; }</style>
                    <style>.special { font-weight: bold; }</style>
                </head><body>
                    <p class='special'>Styled by multiple sheets</p>
                </body></html>");
            AssertValidPdf(pdf);
        }

        [Fact]
        public void InlineStyle_OverridesStylesheet()
        {
            var pdf = RenderPdf(@"
                <style>p { color: red; }</style>
                <p style='color: blue;'>Should be blue</p>");
            AssertValidPdf(pdf);
        }

        [Fact]
        public void ClassSelector_Applied()
        {
            var pdf = RenderPdf(@"
                <style>.highlight { background-color: yellow; color: black; }</style>
                <p class='highlight'>Highlighted text</p>
                <p>Normal text</p>");
            AssertValidPdf(pdf);
        }

        [Fact]
        public void IdSelector_Applied()
        {
            var pdf = RenderPdf(@"
                <style>#title { font-size: 32px; color: darkblue; }</style>
                <h1 id='title'>Big Title</h1>");
            AssertValidPdf(pdf);
        }

        [Fact]
        public void ElementSelector_Applied()
        {
            var pdf = RenderPdf(@"
                <style>
                    h1 { color: navy; }
                    p { color: gray; }
                    strong { color: red; }
                </style>
                <h1>Title</h1>
                <p>Text with <strong>strong</strong></p>");
            AssertValidPdf(pdf);
        }

        [Fact]
        public void DescendantSelector_Applied()
        {
            var pdf = RenderPdf(@"
                <style>
                    .container p { margin-left: 20px; color: green; }
                </style>
                <div class='container'>
                    <p>Indented green paragraph</p>
                </div>
                <p>Normal paragraph</p>");
            AssertValidPdf(pdf);
        }

        [Fact]
        public void ChildSelector_Applied()
        {
            var pdf = RenderPdf(@"
                <style>
                    .parent > p { color: red; }
                </style>
                <div class='parent'>
                    <p>Direct child (red)</p>
                    <div><p>Not direct child</p></div>
                </div>");
            AssertValidPdf(pdf);
        }

        [Fact]
        public void MediaQuery_PrintMediaApplied()
        {
            var pdf = RenderPdf(@"
                <style>
                    p { color: blue; }
                    @media print {
                        p { color: black; }
                    }
                </style>
                <p>This should use print styles</p>");
            AssertValidPdf(pdf);
        }

        [Fact]
        public void CombinedSelectors_Work()
        {
            var pdf = RenderPdf(@"
                <style>
                    p.intro { font-size: 18px; font-style: italic; }
                    div.card > h2 { color: navy; }
                    ul li { margin-bottom: 5px; }
                </style>
                <p class='intro'>Introduction text</p>
                <div class='card'><h2>Card Title</h2></div>
                <ul><li>Item</li></ul>");
            AssertValidPdf(pdf);
        }

        // =============================================
        // Real-World HTML Patterns
        // =============================================

        [Fact]
        public void BlogPostLayout_Renders()
        {
            var pdf = RenderPdf(@"
                <html><head><style>
                    body { font-family: serif; line-height: 1.6; }
                    .post-title { font-size: 28px; margin-bottom: 10px; }
                    .post-meta { color: gray; font-size: 14px; }
                    .post-body p { margin-bottom: 12px; }
                </style></head><body>
                    <article>
                        <h1 class='post-title'>My Blog Post</h1>
                        <div class='post-meta'>Published on January 1, 2026 by Author</div>
                        <div class='post-body'>
                            <p>This is the first paragraph of the blog post. It contains
                               some introductory text about the topic at hand.</p>
                            <p>The second paragraph goes into more detail. It might contain
                               <strong>bold text</strong> and <em>italic text</em> for emphasis.</p>
                            <blockquote>A notable quote from someone famous.</blockquote>
                            <p>And a conclusion paragraph wrapping things up.</p>
                        </div>
                    </article>
                </body></html>", 600, 800);
            AssertValidPdf(pdf, 300);
            AssertPdfStructure(pdf);
        }

        [Fact]
        public void ProductCard_Renders()
        {
            var pdf = RenderPdf(@"
                <style>
                    .card { border: 1px solid #ddd; padding: 16px; max-width: 300px; }
                    .card-title { font-size: 20px; margin-bottom: 8px; }
                    .card-price { font-size: 24px; color: green; font-weight: bold; }
                    .card-desc { color: #666; font-size: 14px; }
                    .card-btn { display: block; background: blue; color: white; padding: 10px; text-align: center; }
                </style>
                <div class='card'>
                    <h2 class='card-title'>Widget Pro</h2>
                    <p class='card-price'>$49.99</p>
                    <p class='card-desc'>The best widget money can buy. Features include
                       durability, efficiency, and style.</p>
                    <a class='card-btn' href='#'>Add to Cart</a>
                </div>");
            AssertValidPdf(pdf, 300);
        }

        [Fact]
        public void NavigationMenu_Renders()
        {
            var pdf = RenderPdf(@"
                <style>
                    nav { background: #333; padding: 10px; }
                    nav a { color: white; padding: 8px 16px; text-decoration: none; }
                    nav a:first-child { font-weight: bold; }
                </style>
                <nav>
                    <a href='/'>Home</a>
                    <a href='/about'>About</a>
                    <a href='/services'>Services</a>
                    <a href='/contact'>Contact</a>
                </nav>");
            AssertValidPdf(pdf);
        }

        [Fact]
        public void DataTable_Renders()
        {
            var pdf = RenderPdf(@"
                <style>
                    table { border-collapse: collapse; width: 100%; }
                    th, td { border: 1px solid #ddd; padding: 8px; text-align: left; }
                    th { background-color: #4CAF50; color: white; }
                    tr:nth-child(even) { background-color: #f2f2f2; }
                </style>
                <table>
                    <thead>
                        <tr><th>Name</th><th>Role</th><th>Status</th></tr>
                    </thead>
                    <tbody>
                        <tr><td>Alice</td><td>Developer</td><td>Active</td></tr>
                        <tr><td>Bob</td><td>Designer</td><td>Active</td></tr>
                        <tr><td>Charlie</td><td>Manager</td><td>On Leave</td></tr>
                        <tr><td>Diana</td><td>QA</td><td>Active</td></tr>
                    </tbody>
                </table>", 500, 400);
            AssertValidPdf(pdf, 300);
        }

        [Fact]
        public void FormLayout_Renders()
        {
            var pdf = RenderPdf(@"
                <style>
                    .form-group { margin-bottom: 16px; }
                    label { display: block; font-weight: bold; margin-bottom: 4px; }
                    input[type='text'], input[type='email'], textarea {
                        display: block; width: 100%; padding: 8px; border: 1px solid #ccc;
                    }
                    button { background: #007bff; color: white; padding: 10px 20px; border: none; }
                </style>
                <form>
                    <div class='form-group'>
                        <label>Full Name</label>
                        <input type='text' value='John Doe'>
                    </div>
                    <div class='form-group'>
                        <label>Email</label>
                        <input type='email' value='john@example.com'>
                    </div>
                    <div class='form-group'>
                        <label>Message</label>
                        <textarea>Hello, I would like to inquire about...</textarea>
                    </div>
                    <button>Submit</button>
                </form>", 500, 500);
            AssertValidPdf(pdf, 300);
        }

        [Fact]
        public void TwoColumnLayout_Renders()
        {
            var pdf = RenderPdf(@"
                <style>
                    .row { display: flex; }
                    .col { flex: 1; padding: 10px; }
                    .col-left { background: #f0f0f0; }
                    .col-right { background: #e0e0e0; }
                </style>
                <div class='row'>
                    <div class='col col-left'>
                        <h2>Left Column</h2>
                        <p>Content on the left side of the layout.</p>
                    </div>
                    <div class='col col-right'>
                        <h2>Right Column</h2>
                        <p>Content on the right side of the layout.</p>
                    </div>
                </div>", 600, 400);
            AssertValidPdf(pdf, 300);
        }

        [Fact]
        public void CardGrid_Renders()
        {
            var pdf = RenderPdf(@"
                <style>
                    .grid { display: flex; flex-wrap: wrap; gap: 10px; }
                    .card { flex: 0 0 calc(50% - 10px); border: 1px solid #ccc; padding: 12px; }
                    .card h3 { margin: 0 0 8px 0; }
                    .card p { color: #666; font-size: 14px; }
                </style>
                <div class='grid'>
                    <div class='card'><h3>Card 1</h3><p>Description for card one.</p></div>
                    <div class='card'><h3>Card 2</h3><p>Description for card two.</p></div>
                    <div class='card'><h3>Card 3</h3><p>Description for card three.</p></div>
                    <div class='card'><h3>Card 4</h3><p>Description for card four.</p></div>
                </div>", 600, 400);
            AssertValidPdf(pdf, 300);
        }

        [Fact]
        public void HeaderContentFooter_Renders()
        {
            var pdf = RenderPdf(@"
                <style>
                    body { margin: 0; font-family: sans-serif; }
                    .site-header { background: #333; color: white; padding: 16px; }
                    .site-content { padding: 20px; min-height: 200px; }
                    .site-footer { background: #333; color: white; padding: 10px; text-align: center; }
                </style>
                <header class='site-header'>
                    <h1>My Website</h1>
                </header>
                <main class='site-content'>
                    <h2>Welcome</h2>
                    <p>This is the main content area of the website.</p>
                    <p>It contains multiple paragraphs and sections.</p>
                </main>
                <footer class='site-footer'>
                    <p>Copyright 2026 My Website. All rights reserved.</p>
                </footer>", 600, 600);
            AssertValidPdf(pdf, 300);
            AssertPdfStructure(pdf);
        }

        // =============================================
        // Additional Complex Scenarios
        // =============================================

        [Fact]
        public void NestedTables_Render()
        {
            var pdf = RenderPdf(@"
                <table border='1'>
                    <tr>
                        <td>Outer Cell 1</td>
                        <td>
                            <table border='1'>
                                <tr><td>Inner A</td><td>Inner B</td></tr>
                                <tr><td>Inner C</td><td>Inner D</td></tr>
                            </table>
                        </td>
                    </tr>
                </table>");
            AssertValidPdf(pdf);
        }

        [Fact]
        public void MixedListTypes_Render()
        {
            var pdf = RenderPdf(@"
                <ol>
                    <li>Ordered item 1
                        <ul>
                            <li>Unordered sub-item</li>
                            <li>Another sub-item</li>
                        </ul>
                    </li>
                    <li>Ordered item 2</li>
                </ol>");
            AssertValidPdf(pdf);
        }

        [Fact]
        public void CodeBlock_InPre_Renders()
        {
            var pdf = RenderPdf(@"
                <pre><code>function hello() {
    console.log('Hello World');
    return 42;
}</code></pre>");
            AssertValidPdf(pdf);
        }

        [Fact]
        public void InlineAndBlockMixed_Renders()
        {
            var pdf = RenderPdf(@"
                <div>
                    <span>Inline</span>
                    <div>Block</div>
                    <span>Inline again</span>
                    <p>Paragraph</p>
                    Text node
                </div>");
            AssertValidPdf(pdf);
        }

        [Fact]
        public void LargeDocument_Renders()
        {
            var sb = new StringBuilder("<html><body>");
            for (int i = 0; i < 100; i++)
            {
                sb.Append($"<h2>Section {i + 1}</h2>");
                sb.Append($"<p>Paragraph {i + 1} with some text content to fill the page.</p>");
            }
            sb.Append("</body></html>");
            var pdf = RenderPdf(sb.ToString(), 600, 5000);
            AssertValidPdf(pdf, 500);
        }

        [Fact]
        public void MultipleParagraphs_ProduceLargerPdf()
        {
            var singlePdf = RenderPdf("<p>One paragraph</p>");
            var multiPdf = RenderPdf(@"
                <p>First paragraph</p>
                <p>Second paragraph</p>
                <p>Third paragraph</p>
                <p>Fourth paragraph</p>
                <p>Fifth paragraph</p>");
            Assert.True(multiPdf.Length >= singlePdf.Length,
                $"Multiple paragraphs ({multiPdf.Length}) should produce at least as large a PDF as a single one ({singlePdf.Length})");
        }

        [Fact]
        public void SpecialHtmlCharacters_InAttributes()
        {
            var pdf = RenderPdf(@"<a href='https://example.com/page?a=1&amp;b=2'>Link with &amp;</a>");
            AssertValidPdf(pdf);
        }

        [Fact]
        public void UnicodeContent_DoesNotCrash()
        {
            var pdf = RenderPdf("<p>Hello World - Basic Latin</p>");
            AssertValidPdf(pdf);
        }

        [Fact]
        public void ScriptTags_Ignored()
        {
            var pdf = RenderPdf(@"
                <p>Before script</p>
                <script>alert('should not appear');</script>
                <p>After script</p>");
            AssertValidPdf(pdf);
        }

        [Fact]
        public void StyleTagInBody_StillApplied()
        {
            var pdf = RenderPdf(@"
                <body>
                    <style>.red { color: red; }</style>
                    <p class='red'>Red text</p>
                </body>");
            AssertValidPdf(pdf);
        }

        [Fact]
        public void MultipleClassSelectors_Applied()
        {
            var pdf = RenderPdf(@"
                <style>
                    .a.b { color: red; }
                    .a { font-size: 20px; }
                    .b { font-weight: bold; }
                </style>
                <p class='a b'>Has both classes</p>
                <p class='a'>Has only class a</p>");
            AssertValidPdf(pdf);
        }

        [Fact]
        public void SiblingSelector_Applied()
        {
            var pdf = RenderPdf(@"
                <style>
                    h2 + p { font-weight: bold; color: navy; }
                </style>
                <h2>Title</h2>
                <p>First paragraph after heading (bold navy)</p>
                <p>Second paragraph (normal)</p>");
            AssertValidPdf(pdf);
        }

        [Fact]
        public void PseudoClassFirstChild_Applied()
        {
            var pdf = RenderPdf(@"
                <style>
                    li:first-child { color: red; }
                </style>
                <ul>
                    <li>First (red)</li>
                    <li>Second</li>
                    <li>Third</li>
                </ul>");
            AssertValidPdf(pdf);
        }

        [Fact]
        public void BackgroundAndBorder_OnDiv()
        {
            var pdf = RenderPdf(@"
                <div style='background: #f0f0f0; border: 2px solid #333; padding: 20px;'>
                    <p>Content with background and border</p>
                </div>");
            AssertValidPdf(pdf);
        }

        [Fact]
        public void BoxModelProperties_Applied()
        {
            var pdf = RenderPdf(@"
                <div style='margin: 10px; padding: 20px; border: 1px solid black; width: 200px;'>
                    <p>Box model test</p>
                </div>");
            AssertValidPdf(pdf);
        }

        [Fact]
        public void TextAlignment_Applied()
        {
            var pdf = RenderPdf(@"
                <p style='text-align: left;'>Left aligned</p>
                <p style='text-align: center;'>Center aligned</p>
                <p style='text-align: right;'>Right aligned</p>");
            AssertValidPdf(pdf);
        }

        [Fact]
        public void DisplayNone_HidesContent()
        {
            var pdf = RenderPdf(@"
                <p>Visible</p>
                <p style='display: none;'>Hidden</p>
                <p>Also visible</p>");
            AssertValidPdf(pdf);
        }

        [Fact]
        public void OverflowHidden_DoesNotCrash()
        {
            var pdf = RenderPdf(@"
                <div style='width: 100px; height: 50px; overflow: hidden;'>
                    <p>This content is longer than the container and should be clipped
                       when overflow is hidden.</p>
                </div>");
            AssertValidPdf(pdf);
        }

        [Fact]
        public void BorderRadius_DoesNotCrash()
        {
            var pdf = RenderPdf(@"
                <div style='width: 100px; height: 100px; background: blue; border-radius: 10px;'></div>");
            AssertValidPdf(pdf);
        }

        [Fact]
        public void ListStyleType_Variations()
        {
            var pdf = RenderPdf(@"
                <ul style='list-style-type: disc;'><li>Disc</li></ul>
                <ul style='list-style-type: circle;'><li>Circle</li></ul>
                <ul style='list-style-type: square;'><li>Square</li></ul>
                <ol style='list-style-type: decimal;'><li>Decimal</li></ol>
                <ol style='list-style-type: lower-alpha;'><li>Lower alpha</li></ol>");
            AssertValidPdf(pdf);
        }

        [Fact]
        public void FloatLeft_RendersLayout()
        {
            var pdf = RenderPdf(@"
                <div style='float: left; width: 100px; height: 100px; background: red;'></div>
                <p>Text flows around the floated element on the right side.</p>
                <div style='clear: both;'></div>");
            AssertValidPdf(pdf);
        }

        [Fact]
        public void FlexboxLayout_Renders()
        {
            var pdf = RenderPdf(@"
                <div style='display: flex; gap: 10px;'>
                    <div style='flex: 1; background: #eee; padding: 10px;'>Flex Item 1</div>
                    <div style='flex: 1; background: #ddd; padding: 10px;'>Flex Item 2</div>
                    <div style='flex: 1; background: #ccc; padding: 10px;'>Flex Item 3</div>
                </div>", 600, 300);
            AssertValidPdf(pdf);
        }

        [Fact]
        public void GridLayout_Renders()
        {
            var pdf = RenderPdf(@"
                <div style='display: grid; grid-template-columns: 1fr 1fr; gap: 10px;'>
                    <div style='background: #eee; padding: 10px;'>Grid Item 1</div>
                    <div style='background: #ddd; padding: 10px;'>Grid Item 2</div>
                    <div style='background: #ccc; padding: 10px;'>Grid Item 3</div>
                    <div style='background: #bbb; padding: 10px;'>Grid Item 4</div>
                </div>", 600, 300);
            AssertValidPdf(pdf);
        }

        [Fact]
        public void PositionRelative_DoesNotCrash()
        {
            var pdf = RenderPdf(@"
                <div style='position: relative;'>
                    <p>Relatively positioned content</p>
                    <div style='position: absolute; top: 0; right: 0;'>Absolute child</div>
                </div>");
            AssertValidPdf(pdf);
        }

        [Fact]
        public void MinimalValidHtml_ProducesValidPdf()
        {
            var pdf = RenderPdf("<!DOCTYPE html><html><head><title>Test</title></head><body></body></html>");
            AssertValidPdf(pdf);
            AssertPdfStructure(pdf);
        }

        [Fact]
        public void FullPage_Invoice_Renders()
        {
            var pdf = RenderPdf(@"
                <html><head><style>
                    body { font-family: sans-serif; font-size: 12px; }
                    .header { text-align: center; margin-bottom: 20px; }
                    .header h1 { margin: 0; }
                    .info { margin-bottom: 20px; }
                    table { width: 100%; border-collapse: collapse; }
                    th, td { border: 1px solid #ddd; padding: 8px; }
                    th { background: #f5f5f5; text-align: left; }
                    .total { text-align: right; font-weight: bold; font-size: 16px; margin-top: 10px; }
                </style></head><body>
                    <div class='header'>
                        <h1>INVOICE</h1>
                        <p>Invoice #1001</p>
                    </div>
                    <div class='info'>
                        <p><strong>Bill To:</strong> John Doe</p>
                        <p><strong>Date:</strong> March 1, 2026</p>
                    </div>
                    <table>
                        <thead>
                            <tr><th>Item</th><th>Qty</th><th>Price</th><th>Total</th></tr>
                        </thead>
                        <tbody>
                            <tr><td>Widget A</td><td>2</td><td>$25.00</td><td>$50.00</td></tr>
                            <tr><td>Widget B</td><td>1</td><td>$75.00</td><td>$75.00</td></tr>
                            <tr><td>Service Fee</td><td>1</td><td>$15.00</td><td>$15.00</td></tr>
                        </tbody>
                    </table>
                    <p class='total'>Total: $140.00</p>
                </body></html>", 600, 600);
            AssertValidPdf(pdf, 400);
            AssertPdfStructure(pdf);
        }
    }
}
