using System.Linq;
using Rend.Adapters;
using Rend.Html.Parser;
using Xunit;

namespace Rend.Tests.Adapters
{
    public class StylableElementAdapterTests
    {
        [Fact]
        public void TagName_ReturnsLowercaseTagName()
        {
            var doc = HtmlParser.Parse("<div></div>");
            var div = FindFirstElement(doc, "div");
            var adapter = new StylableElementAdapter(div!);

            Assert.Equal("div", adapter.TagName);
        }

        [Fact]
        public void Id_ReturnsElementId()
        {
            var doc = HtmlParser.Parse("<div id=\"main\"></div>");
            var div = FindFirstElement(doc, "div");
            var adapter = new StylableElementAdapter(div!);

            Assert.Equal("main", adapter.Id);
        }

        [Fact]
        public void Id_ReturnsNull_WhenNoIdAttribute()
        {
            var doc = HtmlParser.Parse("<div></div>");
            var div = FindFirstElement(doc, "div");
            var adapter = new StylableElementAdapter(div!);

            Assert.Null(adapter.Id);
        }

        [Fact]
        public void ClassList_ReturnsParsedClasses()
        {
            var doc = HtmlParser.Parse("<div class=\"foo bar baz\"></div>");
            var div = FindFirstElement(doc, "div");
            var adapter = new StylableElementAdapter(div!);

            var classes = adapter.ClassList;
            Assert.Contains("foo", classes);
            Assert.Contains("bar", classes);
            Assert.Contains("baz", classes);
            Assert.Equal(3, classes.Count);
        }

        [Fact]
        public void ClassList_ReturnsEmpty_WhenNoClassAttribute()
        {
            var doc = HtmlParser.Parse("<div></div>");
            var div = FindFirstElement(doc, "div");
            var adapter = new StylableElementAdapter(div!);

            Assert.Empty(adapter.ClassList);
        }

        [Fact]
        public void GetAttribute_ReturnsAttributeValue()
        {
            var doc = HtmlParser.Parse("<a href=\"https://example.com\">Link</a>");
            var a = FindFirstElement(doc, "a");
            var adapter = new StylableElementAdapter(a!);

            Assert.Equal("https://example.com", adapter.GetAttribute("href"));
        }

        [Fact]
        public void GetAttribute_ReturnsNull_WhenNotPresent()
        {
            var doc = HtmlParser.Parse("<div></div>");
            var div = FindFirstElement(doc, "div");
            var adapter = new StylableElementAdapter(div!);

            Assert.Null(adapter.GetAttribute("data-missing"));
        }

        [Fact]
        public void InlineStyle_ReturnsStyleAttribute()
        {
            var doc = HtmlParser.Parse("<div style=\"color: red\"></div>");
            var div = FindFirstElement(doc, "div");
            var adapter = new StylableElementAdapter(div!);

            Assert.Equal("color: red", adapter.InlineStyle);
        }

        [Fact]
        public void Parent_ReturnsParentElement()
        {
            var doc = HtmlParser.Parse("<div><span></span></div>");
            var span = FindFirstElement(doc, "span");
            var adapter = new StylableElementAdapter(span!);

            var parent = adapter.Parent;
            Assert.NotNull(parent);
            Assert.Equal("div", parent!.TagName);
        }

        [Fact]
        public void Children_ReturnsChildElements()
        {
            var doc = HtmlParser.Parse("<ul><li>One</li><li>Two</li><li>Three</li></ul>");
            var ul = FindFirstElement(doc, "ul");
            var adapter = new StylableElementAdapter(ul!);

            var children = adapter.Children.ToList();
            Assert.Equal(3, children.Count);
            Assert.All(children, c => Assert.Equal("li", c.TagName));
        }

        [Fact]
        public void FirstChild_ReturnsFirstElementChild()
        {
            var doc = HtmlParser.Parse("<div><span>First</span><p>Second</p></div>");
            var div = FindFirstElement(doc, "div");
            var adapter = new StylableElementAdapter(div!);

            var first = adapter.FirstChild;
            Assert.NotNull(first);
            Assert.Equal("span", first!.TagName);
        }

        [Fact]
        public void LastChild_ReturnsLastElementChild()
        {
            var doc = HtmlParser.Parse("<div><span>First</span><p>Second</p></div>");
            var div = FindFirstElement(doc, "div");
            var adapter = new StylableElementAdapter(div!);

            var last = adapter.LastChild;
            Assert.NotNull(last);
            Assert.Equal("p", last!.TagName);
        }

        [Fact]
        public void NextSibling_ReturnsNextElementSibling()
        {
            var doc = HtmlParser.Parse("<div><span>A</span><p>B</p></div>");
            var span = FindFirstElement(doc, "span");
            var adapter = new StylableElementAdapter(span!);

            var next = adapter.NextSibling;
            Assert.NotNull(next);
            Assert.Equal("p", next!.TagName);
        }

        [Fact]
        public void PreviousSibling_ReturnsPreviousElementSibling()
        {
            var doc = HtmlParser.Parse("<div><span>A</span><p>B</p></div>");
            var p = FindFirstElement(doc, "p");
            var adapter = new StylableElementAdapter(p!);

            var prev = adapter.PreviousSibling;
            Assert.NotNull(prev);
            Assert.Equal("span", prev!.TagName);
        }

        [Fact]
        public void PreviousSibling_ReturnsNull_WhenFirst()
        {
            var doc = HtmlParser.Parse("<div><span>A</span></div>");
            var span = FindFirstElement(doc, "span");
            var adapter = new StylableElementAdapter(span!);

            Assert.Null(adapter.PreviousSibling);
        }

        [Fact]
        public void NextSibling_ReturnsNull_WhenLast()
        {
            var doc = HtmlParser.Parse("<div><span>A</span></div>");
            var span = FindFirstElement(doc, "span");
            var adapter = new StylableElementAdapter(span!);

            Assert.Null(adapter.NextSibling);
        }

        [Fact]
        public void Element_ReturnsUnderlyingElement()
        {
            var doc = HtmlParser.Parse("<div></div>");
            var div = FindFirstElement(doc, "div");
            var adapter = new StylableElementAdapter(div!);

            Assert.Same(div, adapter.Element);
        }

        private static Rend.Html.Element? FindFirstElement(Rend.Html.Node node, string tagName)
        {
            if (node is Rend.Html.Element el && el.TagName == tagName)
                return el;

            var child = node.FirstChild;
            while (child != null)
            {
                var result = FindFirstElement(child, tagName);
                if (result != null) return result;
                child = child.NextSibling;
            }
            return null;
        }
    }
}
