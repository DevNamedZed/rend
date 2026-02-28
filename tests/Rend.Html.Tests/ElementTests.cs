using Xunit;
using Rend.Html;
using Rend.Html.Parser;

namespace Rend.Html.Tests
{
    public class ElementTests
    {
        private static Document ParseDoc(string html) => HtmlParser.Parse(html);

        private static Element CreateElement(string tagName, string? id = null, string? className = null)
        {
            var doc = new Document();
            var el = doc.CreateElement(tagName);
            if (id != null) el.SetAttribute("id", id);
            if (className != null) el.SetAttribute("class", className);
            return el;
        }

        [Fact]
        public void TagName_IsLowercase()
        {
            var doc = ParseDoc("<html><head></head><body><DIV></DIV></body></html>");
            var div = doc.Body!.FirstChild as Element;
            Assert.NotNull(div);
            Assert.Equal("div", div!.TagName);
        }

        [Fact]
        public void GetAttribute_ReturnsValue_WhenExists()
        {
            var el = CreateElement("div");
            el.SetAttribute("data-value", "42");

            Assert.Equal("42", el.GetAttribute("data-value"));
        }

        [Fact]
        public void GetAttribute_ReturnsNull_WhenNotExists()
        {
            var el = CreateElement("div");

            Assert.Null(el.GetAttribute("nonexistent"));
        }

        [Fact]
        public void SetAttribute_OverwritesExistingValue()
        {
            var el = CreateElement("div");
            el.SetAttribute("title", "old");
            el.SetAttribute("title", "new");

            Assert.Equal("new", el.GetAttribute("title"));
        }

        [Fact]
        public void RemoveAttribute_RemovesExisting()
        {
            var el = CreateElement("div");
            el.SetAttribute("data-x", "1");

            el.RemoveAttribute("data-x");

            Assert.Null(el.GetAttribute("data-x"));
            Assert.False(el.HasAttribute("data-x"));
        }

        [Fact]
        public void RemoveAttribute_NoOp_WhenNotExists()
        {
            var el = CreateElement("div");
            // Should not throw
            el.RemoveAttribute("nonexistent");
        }

        [Fact]
        public void HasAttribute_ReturnsTrue_WhenExists()
        {
            var el = CreateElement("div");
            el.SetAttribute("role", "button");

            Assert.True(el.HasAttribute("role"));
        }

        [Fact]
        public void HasAttribute_ReturnsFalse_WhenNotExists()
        {
            var el = CreateElement("div");

            Assert.False(el.HasAttribute("role"));
        }

        [Fact]
        public void Id_GetSet_Works()
        {
            var el = CreateElement("div");
            el.Id = "myId";

            Assert.Equal("myId", el.Id);
            Assert.Equal("myId", el.GetAttribute("id"));
        }

        [Fact]
        public void Id_SetNull_RemovesAttribute()
        {
            var el = CreateElement("div", id: "myId");
            el.Id = null;

            Assert.Null(el.Id);
            Assert.False(el.HasAttribute("id"));
        }

        [Fact]
        public void ClassList_ReturnsTokenList()
        {
            var el = CreateElement("div", className: "foo bar");

            var classList = el.ClassList;
            Assert.True(classList.Contains("foo"));
            Assert.True(classList.Contains("bar"));
            Assert.False(classList.Contains("baz"));
        }

        [Fact]
        public void Attributes_Count_IsCorrect()
        {
            var el = CreateElement("div");
            el.SetAttribute("id", "test");
            el.SetAttribute("class", "cls");
            el.SetAttribute("data-x", "1");

            Assert.Equal(3, el.Attributes.Count);
        }

        [Fact]
        public void Attributes_CanEnumerate()
        {
            var el = CreateElement("div");
            el.SetAttribute("id", "test");
            el.SetAttribute("class", "cls");

            var names = new System.Collections.Generic.List<string>();
            foreach (var attr in el.Attributes)
                names.Add(attr.Name);

            Assert.Contains("id", names);
            Assert.Contains("class", names);
        }

        [Fact]
        public void NodeType_IsElement()
        {
            var el = CreateElement("span");
            Assert.Equal(NodeType.Element, el.NodeType);
        }

        [Fact]
        public void TextContent_ConcatenatesDescendantText()
        {
            var doc = ParseDoc("<html><head></head><body><div>Hello <span>World</span></div></body></html>");
            var div = doc.Body!.FirstChild as Element;
            Assert.NotNull(div);
            Assert.Equal("Hello World", div!.TextContent);
        }

        [Fact]
        public void TextContent_Set_ReplacesChildren()
        {
            var doc = ParseDoc("<html><head></head><body><div><p>Old</p></div></body></html>");
            var div = doc.Body!.FirstChild as Element;
            Assert.NotNull(div);

            div!.TextContent = "New text";

            Assert.Equal("New text", div.TextContent);
            Assert.IsType<TextNode>(div.FirstChild);
            Assert.Null(div.FirstChild!.NextSibling);
        }

        [Fact]
        public void QuerySelector_FindsFirstMatch()
        {
            var doc = ParseDoc("<html><head></head><body><div><p class=\"a\">1</p><p class=\"b\">2</p></div></body></html>");
            var body = doc.Body!;

            var p = body.QuerySelector("p");
            Assert.NotNull(p);
            Assert.Equal("1", p!.TextContent);
        }

        [Fact]
        public void QuerySelector_ReturnsNull_WhenNoMatch()
        {
            var doc = ParseDoc("<html><head></head><body><div>text</div></body></html>");
            var body = doc.Body!;

            Assert.Null(body.QuerySelector("span"));
        }

        [Fact]
        public void QuerySelectorAll_FindsAllMatches()
        {
            var doc = ParseDoc("<html><head></head><body><p>1</p><div><p>2</p></div><p>3</p></body></html>");
            var body = doc.Body!;

            var ps = body.QuerySelectorAll("p");
            Assert.Equal(3, ps.Count);
        }

        [Fact]
        public void Matches_ReturnsTrueForMatchingSelector()
        {
            var doc = ParseDoc("<html><head></head><body><div class=\"foo\" id=\"bar\"></div></body></html>");
            var div = doc.Body!.FirstChild as Element;
            Assert.NotNull(div);

            Assert.True(div!.Matches("div"));
            Assert.True(div.Matches(".foo"));
            Assert.True(div.Matches("#bar"));
            Assert.True(div.Matches("div.foo#bar"));
        }

        [Fact]
        public void Matches_ReturnsFalseForNonMatchingSelector()
        {
            var doc = ParseDoc("<html><head></head><body><div class=\"foo\"></div></body></html>");
            var div = doc.Body!.FirstChild as Element;
            Assert.NotNull(div);

            Assert.False(div!.Matches("span"));
            Assert.False(div.Matches(".bar"));
            Assert.False(div.Matches("#nope"));
        }

        [Fact]
        public void CloneNode_Shallow_ClonesElementAndAttributes()
        {
            var el = CreateElement("div", id: "orig");
            el.SetAttribute("data-x", "1");

            var clone = el.CloneNode(false) as Element;
            Assert.NotNull(clone);
            Assert.Equal("div", clone!.TagName);
            Assert.Equal("orig", clone.Id);
            Assert.Equal("1", clone.GetAttribute("data-x"));
            Assert.False(clone.HasChildNodes);
        }

        [Fact]
        public void CloneNode_Deep_ClonesChildren()
        {
            var doc = ParseDoc("<html><head></head><body><div id=\"parent\"><p>Child</p></div></body></html>");
            var div = doc.Body!.FirstChild as Element;

            var clone = div!.CloneNode(true) as Element;
            Assert.NotNull(clone);
            Assert.True(clone!.HasChildNodes);
            var p = clone.FirstChild as Element;
            Assert.NotNull(p);
            Assert.Equal("p", p!.TagName);
            Assert.Equal("Child", p.TextContent);
        }

        [Fact]
        public void InnerHtml_Get_SerializesChildren()
        {
            var doc = ParseDoc("<html><head></head><body><div><p>Text</p></div></body></html>");
            var div = doc.Body!.FirstChild as Element;

            var inner = div!.InnerHtml;
            Assert.Contains("<p>", inner);
            Assert.Contains("Text", inner);
        }

        [Fact]
        public void OuterHtml_IncludesElement()
        {
            var doc = ParseDoc("<html><head></head><body><div id=\"test\"><p>Text</p></div></body></html>");
            var div = doc.Body!.FirstChild as Element;

            var outer = div!.OuterHtml;
            Assert.Contains("<div", outer);
            Assert.Contains("id=", outer);
            Assert.Contains("</div>", outer);
        }

        [Fact]
        public void ToString_ReturnsTagAngleBrackets()
        {
            var el = CreateElement("section");
            Assert.Equal("<section>", el.ToString());
        }

        [Fact]
        public void SetMultipleAttributes_GrowsCorrectly()
        {
            var el = CreateElement("div");
            // Add more than the initial capacity (4) to test array growth
            for (int i = 0; i < 10; i++)
                el.SetAttribute($"data-{i}", $"val-{i}");

            Assert.Equal(10, el.Attributes.Count);
            for (int i = 0; i < 10; i++)
                Assert.Equal($"val-{i}", el.GetAttribute($"data-{i}"));
        }
    }
}
