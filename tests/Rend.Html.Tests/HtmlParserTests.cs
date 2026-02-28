using Xunit;
using Rend.Html;
using Rend.Html.Parser;

namespace Rend.Html.Tests
{
    public class HtmlParserTests
    {
        [Fact]
        public void Parse_SimpleHtml_CreatesDocumentWithHtmlHeadBody()
        {
            var doc = HtmlParser.Parse("<html><head></head><body></body></html>");

            Assert.NotNull(doc);
            Assert.Equal(NodeType.Document, doc.NodeType);
            Assert.NotNull(doc.DocumentElement);
            Assert.Equal("html", doc.DocumentElement!.TagName);
            Assert.NotNull(doc.Head);
            Assert.Equal("head", doc.Head!.TagName);
            Assert.NotNull(doc.Body);
            Assert.Equal("body", doc.Body!.TagName);
        }

        [Fact]
        public void Parse_EmptyString_CreatesDocumentWithImpliedStructure()
        {
            var doc = HtmlParser.Parse("");

            Assert.NotNull(doc);
            // HTML5 parser should create implied html/head/body
            Assert.NotNull(doc.DocumentElement);
            Assert.NotNull(doc.Head);
            Assert.NotNull(doc.Body);
        }

        [Fact]
        public void Parse_Doctype_CreatesDocumentTypeNode()
        {
            var doc = HtmlParser.Parse("<!DOCTYPE html><html><head></head><body></body></html>");

            var doctype = doc.Doctype;
            Assert.NotNull(doctype);
            Assert.Equal(NodeType.DocumentType, doctype!.NodeType);
            Assert.Equal("html", doctype.Name);
        }

        [Fact]
        public void Parse_Title_SetsDocumentTitle()
        {
            var doc = HtmlParser.Parse("<html><head><title>Test Page</title></head><body></body></html>");

            Assert.Equal("Test Page", doc.Title);
        }

        [Fact]
        public void Parse_TextContent_CreatesTextNodes()
        {
            var doc = HtmlParser.Parse("<html><head></head><body>Hello World</body></html>");

            var body = doc.Body;
            Assert.NotNull(body);
            Assert.True(body!.HasChildNodes);

            var textNode = body.FirstChild as TextNode;
            Assert.NotNull(textNode);
            Assert.Equal("Hello World", textNode!.Data);
            Assert.Equal(NodeType.Text, textNode.NodeType);
        }

        [Fact]
        public void Parse_NestedElements_BuildsCorrectTree()
        {
            var doc = HtmlParser.Parse("<html><head></head><body><div><p>Text</p></div></body></html>");

            var body = doc.Body!;
            var div = body.FirstChild as Element;
            Assert.NotNull(div);
            Assert.Equal("div", div!.TagName);

            var p = div.FirstChild as Element;
            Assert.NotNull(p);
            Assert.Equal("p", p!.TagName);
            Assert.Equal("Text", p.TextContent);
        }

        [Fact]
        public void Parse_Attributes_ParsedCorrectly()
        {
            var doc = HtmlParser.Parse("<html><head></head><body><div id=\"main\" class=\"container active\"></div></body></html>");

            var div = doc.Body!.FirstChild as Element;
            Assert.NotNull(div);
            Assert.Equal("main", div!.GetAttribute("id"));
            Assert.Equal("container active", div.GetAttribute("class"));
        }

        [Fact]
        public void Parse_SelfClosingTags_HandleCorrectly()
        {
            var doc = HtmlParser.Parse("<html><head></head><body><br><img src=\"test.png\"><input type=\"text\"></body></html>");

            var body = doc.Body!;
            var children = new System.Collections.Generic.List<Element>();
            var child = body.FirstChild;
            while (child != null)
            {
                if (child is Element el)
                    children.Add(el);
                child = child.NextSibling;
            }

            Assert.True(children.Count >= 3);
            Assert.Equal("br", children[0].TagName);
            // br should be void: no children
            Assert.False(children[0].HasChildNodes);
            Assert.Equal("img", children[1].TagName);
            Assert.Equal("test.png", children[1].GetAttribute("src"));
            Assert.Equal("input", children[2].TagName);
            Assert.Equal("text", children[2].GetAttribute("type"));
        }

        [Fact]
        public void Parse_MultipleSiblings_LinkedCorrectly()
        {
            var doc = HtmlParser.Parse("<html><head></head><body><p>A</p><p>B</p><p>C</p></body></html>");

            var body = doc.Body!;
            var first = body.FirstChild as Element;
            Assert.NotNull(first);
            Assert.Equal("p", first!.TagName);
            Assert.Equal("A", first.TextContent);

            var second = first.NextSibling as Element;
            Assert.NotNull(second);
            Assert.Equal("B", second!.TextContent);

            var third = second.NextSibling as Element;
            Assert.NotNull(third);
            Assert.Equal("C", third!.TextContent);

            Assert.Null(third.NextSibling);
        }

        [Fact]
        public void Parse_Comment_CreatesCommentNode()
        {
            var doc = HtmlParser.Parse("<html><head></head><body><!-- hello --></body></html>");

            var body = doc.Body!;
            Comment? comment = null;
            var child = body.FirstChild;
            while (child != null)
            {
                if (child is Comment c)
                {
                    comment = c;
                    break;
                }
                child = child.NextSibling;
            }

            Assert.NotNull(comment);
            Assert.Equal(NodeType.Comment, comment!.NodeType);
            Assert.Equal(" hello ", comment.Data);
        }

        [Fact]
        public void Parse_MalformedHtml_UnclosedTags_Recovers()
        {
            // The parser should handle missing closing tags gracefully
            var doc = HtmlParser.Parse("<html><head></head><body><div><p>Text</div></body></html>");

            Assert.NotNull(doc.Body);
            // The document should still parse without throwing
            var body = doc.Body!;
            Assert.True(body.HasChildNodes);
        }

        [Fact]
        public void Parse_MalformedHtml_NoHtmlTag_ImpliesStructure()
        {
            var doc = HtmlParser.Parse("<p>Just a paragraph</p>");

            Assert.NotNull(doc.DocumentElement);
            Assert.NotNull(doc.Body);
            // The paragraph should end up in the body
            var p = doc.Body!.FirstChild as Element;
            Assert.NotNull(p);
            Assert.Equal("p", p!.TagName);
        }

        [Fact]
        public void Parse_MixedContent_TextAndElements()
        {
            var doc = HtmlParser.Parse("<html><head></head><body>Before<span>Inside</span>After</body></html>");

            var body = doc.Body!;
            var first = body.FirstChild;
            Assert.NotNull(first);
            Assert.IsType<TextNode>(first);
            Assert.Equal("Before", ((TextNode)first!).Data);

            var span = first.NextSibling as Element;
            Assert.NotNull(span);
            Assert.Equal("span", span!.TagName);
            Assert.Equal("Inside", span.TextContent);

            var last = span.NextSibling;
            Assert.NotNull(last);
            Assert.IsType<TextNode>(last);
            Assert.Equal("After", ((TextNode)last!).Data);
        }

        [Fact]
        public void Parse_BooleanAttributes_HasEmptyValue()
        {
            var doc = HtmlParser.Parse("<html><head></head><body><input disabled></body></html>");

            var input = doc.Body!.FirstChild as Element;
            Assert.NotNull(input);
            Assert.True(input!.HasAttribute("disabled"));
        }

        [Fact]
        public void Parse_MultipleAttributes_AllParsed()
        {
            var doc = HtmlParser.Parse("<html><head></head><body><a href=\"/page\" title=\"Go\" class=\"link\">Click</a></body></html>");

            var a = doc.Body!.FirstChild as Element;
            Assert.NotNull(a);
            Assert.Equal("/page", a!.GetAttribute("href"));
            Assert.Equal("Go", a.GetAttribute("title"));
            Assert.Equal("link", a.GetAttribute("class"));
            Assert.Equal("Click", a.TextContent);
        }

        [Fact]
        public void Parse_DeeplyNested_BuildsCorrectDepth()
        {
            var doc = HtmlParser.Parse("<html><head></head><body><div><ul><li><a><span>deep</span></a></li></ul></div></body></html>");

            var div = doc.Body!.FirstChild as Element;
            Assert.NotNull(div);
            Assert.Equal("div", div!.TagName);

            var ul = div.FirstChild as Element;
            Assert.NotNull(ul);
            Assert.Equal("ul", ul!.TagName);

            var li = ul.FirstChild as Element;
            Assert.NotNull(li);
            Assert.Equal("li", li!.TagName);

            var a = li.FirstChild as Element;
            Assert.NotNull(a);
            Assert.Equal("a", a!.TagName);

            var span = a.FirstChild as Element;
            Assert.NotNull(span);
            Assert.Equal("span", span!.TagName);
            Assert.Equal("deep", span.TextContent);
        }

        [Fact]
        public void Parse_Stream_ProducesSameResult()
        {
            var html = "<html><head><title>Stream</title></head><body><p>Test</p></body></html>";
            var stream = new System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes(html));

            var doc = HtmlParser.Parse(stream);

            Assert.Equal("Stream", doc.Title);
            var p = doc.Body!.FirstChild as Element;
            Assert.NotNull(p);
            Assert.Equal("Test", p!.TextContent);
        }

        [Fact]
        public void ParseFragment_ReturnsDocumentFragment()
        {
            var fragment = HtmlParser.ParseFragment("<p>One</p><p>Two</p>");

            Assert.NotNull(fragment);
            Assert.Equal(NodeType.DocumentFragment, fragment.NodeType);
            Assert.True(fragment.HasChildNodes);

            var first = fragment.FirstChild as Element;
            Assert.NotNull(first);
            Assert.Equal("p", first!.TagName);
            Assert.Equal("One", first.TextContent);

            var second = first.NextSibling as Element;
            Assert.NotNull(second);
            Assert.Equal("p", second!.TagName);
            Assert.Equal("Two", second.TextContent);
        }

        [Fact]
        public void Parse_NullHtml_ThrowsArgumentNullException()
        {
            Assert.Throws<System.ArgumentNullException>(() => HtmlParser.Parse((string)null!));
        }

        [Fact]
        public void Parse_EntityCharacters_DecodedCorrectly()
        {
            var doc = HtmlParser.Parse("<html><head></head><body><p>&amp; &lt; &gt;</p></body></html>");

            var p = doc.Body!.FirstChild as Element;
            Assert.NotNull(p);
            Assert.Equal("& < >", p!.TextContent);
        }

        [Fact]
        public void Parse_Table_StructureIsCorrect()
        {
            var doc = HtmlParser.Parse("<html><head></head><body><table><tr><td>Cell</td></tr></table></body></html>");

            var table = doc.Body!.FirstChild as Element;
            Assert.NotNull(table);
            Assert.Equal("table", table!.TagName);
        }

        [Fact]
        public void Parse_GetElementById_FindsElement()
        {
            var doc = HtmlParser.Parse("<html><head></head><body><div id=\"target\">Found</div></body></html>");

            var el = doc.GetElementById("target");
            Assert.NotNull(el);
            Assert.Equal("div", el!.TagName);
            Assert.Equal("Found", el.TextContent);
        }

        [Fact]
        public void Parse_GetElementsByTagName_FindsAll()
        {
            var doc = HtmlParser.Parse("<html><head></head><body><p>A</p><div><p>B</p></div><p>C</p></body></html>");

            var ps = doc.GetElementsByTagName("p");
            Assert.Equal(3, ps.Count);
        }

        [Fact]
        public void Parse_GetElementsByClassName_FindsAll()
        {
            var doc = HtmlParser.Parse("<html><head></head><body><div class=\"x\">1</div><div class=\"y\">2</div><span class=\"x\">3</span></body></html>");

            var xs = doc.GetElementsByClassName("x");
            Assert.Equal(2, xs.Count);
        }
    }
}
