using Xunit;
using Rend.Html;
using Rend.Html.Parser;

namespace Rend.Html.Tests
{
    public class NodeTests
    {
        private static Document CreateDoc()
        {
            return new Document();
        }

        // --- Parent/child relationships ---

        [Fact]
        public void AppendChild_SetsParent()
        {
            var doc = CreateDoc();
            var div = doc.CreateElement("div");
            var p = doc.CreateElement("p");

            div.AppendChild(p);

            Assert.Same(div, p.Parent);
        }

        [Fact]
        public void AppendChild_SetsFirstAndLastChild()
        {
            var doc = CreateDoc();
            var div = doc.CreateElement("div");
            var p = doc.CreateElement("p");

            div.AppendChild(p);

            Assert.Same(p, div.FirstChild);
            Assert.Same(p, div.LastChild);
        }

        [Fact]
        public void AppendChild_MultipleChildren_SetsCorrectLinks()
        {
            var doc = CreateDoc();
            var parent = doc.CreateElement("div");
            var a = doc.CreateElement("a");
            var b = doc.CreateElement("b");
            var c = doc.CreateElement("c");

            parent.AppendChild(a);
            parent.AppendChild(b);
            parent.AppendChild(c);

            Assert.Same(a, parent.FirstChild);
            Assert.Same(c, parent.LastChild);
        }

        [Fact]
        public void AppendChild_SetsOwnerDocument()
        {
            var doc = CreateDoc();
            var div = doc.CreateElement("div");
            doc.AppendChild(div);

            Assert.Same(doc, div.OwnerDocument);
        }

        [Fact]
        public void PrependChild_InsertsAtBeginning()
        {
            var doc = CreateDoc();
            var parent = doc.CreateElement("div");
            var first = doc.CreateElement("p");
            var second = doc.CreateElement("span");

            parent.AppendChild(second);
            parent.PrependChild(first);

            Assert.Same(first, parent.FirstChild);
            Assert.Same(second, parent.LastChild);
            Assert.Same(second, first.NextSibling);
            Assert.Same(first, second.PreviousSibling);
        }

        [Fact]
        public void InsertBefore_InsertsBeforeReference()
        {
            var doc = CreateDoc();
            var parent = doc.CreateElement("div");
            var a = doc.CreateElement("a");
            var c = doc.CreateElement("c");
            var b = doc.CreateElement("b");

            parent.AppendChild(a);
            parent.AppendChild(c);
            parent.InsertBefore(b, c);

            Assert.Same(a, parent.FirstChild);
            Assert.Same(b, a.NextSibling);
            Assert.Same(c, b.NextSibling);
            Assert.Same(c, parent.LastChild);
        }

        [Fact]
        public void InsertBefore_NullReference_AppendsToEnd()
        {
            var doc = CreateDoc();
            var parent = doc.CreateElement("div");
            var a = doc.CreateElement("a");
            var b = doc.CreateElement("b");

            parent.AppendChild(a);
            parent.InsertBefore(b, null);

            Assert.Same(b, parent.LastChild);
        }

        // --- Sibling navigation ---

        [Fact]
        public void NextSibling_NavigatesForward()
        {
            var doc = CreateDoc();
            var parent = doc.CreateElement("div");
            var a = doc.CreateElement("a");
            var b = doc.CreateElement("b");
            var c = doc.CreateElement("c");

            parent.AppendChild(a);
            parent.AppendChild(b);
            parent.AppendChild(c);

            Assert.Same(b, a.NextSibling);
            Assert.Same(c, b.NextSibling);
            Assert.Null(c.NextSibling);
        }

        [Fact]
        public void PreviousSibling_NavigatesBackward()
        {
            var doc = CreateDoc();
            var parent = doc.CreateElement("div");
            var a = doc.CreateElement("a");
            var b = doc.CreateElement("b");
            var c = doc.CreateElement("c");

            parent.AppendChild(a);
            parent.AppendChild(b);
            parent.AppendChild(c);

            Assert.Null(a.PreviousSibling);
            Assert.Same(a, b.PreviousSibling);
            Assert.Same(b, c.PreviousSibling);
        }

        [Fact]
        public void FirstChild_IsNull_WhenNoChildren()
        {
            var doc = CreateDoc();
            var div = doc.CreateElement("div");

            Assert.Null(div.FirstChild);
            Assert.Null(div.LastChild);
        }

        // --- RemoveChild ---

        [Fact]
        public void RemoveChild_DetachesFromParent()
        {
            var doc = CreateDoc();
            var parent = doc.CreateElement("div");
            var child = doc.CreateElement("p");
            parent.AppendChild(child);

            parent.RemoveChild(child);

            Assert.Null(child.Parent);
            Assert.Null(parent.FirstChild);
            Assert.Null(parent.LastChild);
        }

        [Fact]
        public void RemoveChild_MiddleChild_FixesLinks()
        {
            var doc = CreateDoc();
            var parent = doc.CreateElement("div");
            var a = doc.CreateElement("a");
            var b = doc.CreateElement("b");
            var c = doc.CreateElement("c");

            parent.AppendChild(a);
            parent.AppendChild(b);
            parent.AppendChild(c);

            parent.RemoveChild(b);

            Assert.Same(c, a.NextSibling);
            Assert.Same(a, c.PreviousSibling);
            Assert.Null(b.Parent);
            Assert.Null(b.NextSibling);
            Assert.Null(b.PreviousSibling);
        }

        [Fact]
        public void RemoveChild_FirstChild_UpdatesFirstChild()
        {
            var doc = CreateDoc();
            var parent = doc.CreateElement("div");
            var a = doc.CreateElement("a");
            var b = doc.CreateElement("b");

            parent.AppendChild(a);
            parent.AppendChild(b);

            parent.RemoveChild(a);

            Assert.Same(b, parent.FirstChild);
            Assert.Null(b.PreviousSibling);
        }

        [Fact]
        public void RemoveChild_LastChild_UpdatesLastChild()
        {
            var doc = CreateDoc();
            var parent = doc.CreateElement("div");
            var a = doc.CreateElement("a");
            var b = doc.CreateElement("b");

            parent.AppendChild(a);
            parent.AppendChild(b);

            parent.RemoveChild(b);

            Assert.Same(a, parent.LastChild);
            Assert.Null(a.NextSibling);
        }

        // --- ReplaceChild ---

        [Fact]
        public void ReplaceChild_ReplacesOldWithNew()
        {
            var doc = CreateDoc();
            var parent = doc.CreateElement("div");
            var old = doc.CreateElement("p");
            var replacement = doc.CreateElement("span");

            parent.AppendChild(old);
            parent.ReplaceChild(replacement, old);

            Assert.Same(replacement, parent.FirstChild);
            Assert.Null(old.Parent);
            Assert.Same(parent, replacement.Parent);
        }

        // --- HasChildNodes ---

        [Fact]
        public void HasChildNodes_TrueWhenHasChildren()
        {
            var doc = CreateDoc();
            var parent = doc.CreateElement("div");
            parent.AppendChild(doc.CreateElement("p"));

            Assert.True(parent.HasChildNodes);
        }

        [Fact]
        public void HasChildNodes_FalseWhenEmpty()
        {
            var doc = CreateDoc();
            var div = doc.CreateElement("div");

            Assert.False(div.HasChildNodes);
        }

        // --- ChildNodes ---

        [Fact]
        public void ChildNodes_Count_IsCorrect()
        {
            var doc = CreateDoc();
            var parent = doc.CreateElement("div");
            parent.AppendChild(doc.CreateElement("a"));
            parent.AppendChild(doc.CreateElement("b"));
            parent.AppendChild(doc.CreateElement("c"));

            Assert.Equal(3, parent.ChildNodes.Count);
        }

        [Fact]
        public void ChildNodes_Indexer_ReturnsCorrectNode()
        {
            var doc = CreateDoc();
            var parent = doc.CreateElement("div");
            var a = doc.CreateElement("a");
            var b = doc.CreateElement("b");
            parent.AppendChild(a);
            parent.AppendChild(b);

            Assert.Same(a, parent.ChildNodes[0]);
            Assert.Same(b, parent.ChildNodes[1]);
            Assert.Null(parent.ChildNodes[2]);
        }

        [Fact]
        public void ChildNodes_Enumeration_VisitsAll()
        {
            var doc = CreateDoc();
            var parent = doc.CreateElement("div");
            var a = doc.CreateElement("a");
            var b = doc.CreateElement("b");
            parent.AppendChild(a);
            parent.AppendChild(b);

            var children = new System.Collections.Generic.List<Node>();
            foreach (var child in parent.ChildNodes)
                children.Add(child);

            Assert.Equal(2, children.Count);
            Assert.Same(a, children[0]);
            Assert.Same(b, children[1]);
        }

        // --- TextNode ---

        [Fact]
        public void TextNode_Data_ReturnsText()
        {
            var doc = CreateDoc();
            var text = doc.CreateTextNode("hello");

            Assert.Equal("hello", text.Data);
            Assert.Equal(NodeType.Text, text.NodeType);
        }

        [Fact]
        public void TextNode_TextContent_SameAsData()
        {
            var doc = CreateDoc();
            var text = doc.CreateTextNode("world");

            Assert.Equal("world", text.TextContent);
        }

        [Fact]
        public void TextNode_SetTextContent_UpdatesData()
        {
            var doc = CreateDoc();
            var text = doc.CreateTextNode("old");
            text.TextContent = "new";

            Assert.Equal("new", text.Data);
        }

        // --- Comment ---

        [Fact]
        public void Comment_Data_ReturnsText()
        {
            var doc = CreateDoc();
            var comment = doc.CreateComment("a comment");

            Assert.Equal("a comment", comment.Data);
            Assert.Equal(NodeType.Comment, comment.NodeType);
        }

        // --- DocumentType ---

        [Fact]
        public void DocumentType_Properties_AreSet()
        {
            var doc = CreateDoc();
            var doctype = doc.CreateDocumentType("html", "", "");

            Assert.Equal("html", doctype.Name);
            Assert.Equal("", doctype.PublicId);
            Assert.Equal("", doctype.SystemId);
            Assert.Equal(NodeType.DocumentType, doctype.NodeType);
        }

        // --- DocumentFragment ---

        [Fact]
        public void DocumentFragment_AppendToNode_TransfersChildren()
        {
            var doc = CreateDoc();
            var fragment = doc.CreateDocumentFragment();
            var a = doc.CreateElement("a");
            var b = doc.CreateElement("b");
            fragment.AppendChild(a);
            fragment.AppendChild(b);

            var target = doc.CreateElement("div");
            target.AppendChild(fragment);

            Assert.Same(a, target.FirstChild);
            Assert.Same(b, target.LastChild);
            // Fragment should be empty after transfer
            Assert.Null(fragment.FirstChild);
        }

        // --- AppendChild re-parents ---

        [Fact]
        public void AppendChild_MovesNodeFromOldParent()
        {
            var doc = CreateDoc();
            var parent1 = doc.CreateElement("div");
            var parent2 = doc.CreateElement("section");
            var child = doc.CreateElement("p");

            parent1.AppendChild(child);
            Assert.Same(parent1, child.Parent);

            parent2.AppendChild(child);
            Assert.Same(parent2, child.Parent);
            Assert.Null(parent1.FirstChild);
        }

        // --- CloneNode ---

        [Fact]
        public void CloneNode_TextNode_Clones()
        {
            var doc = CreateDoc();
            var text = doc.CreateTextNode("hello");
            var clone = text.CloneNode() as TextNode;

            Assert.NotNull(clone);
            Assert.Equal("hello", clone!.Data);
            Assert.NotSame(text, clone);
        }

        [Fact]
        public void CloneNode_Comment_Clones()
        {
            var doc = CreateDoc();
            var comment = doc.CreateComment("test");
            var clone = comment.CloneNode() as Comment;

            Assert.NotNull(clone);
            Assert.Equal("test", clone!.Data);
            Assert.NotSame(comment, clone);
        }

        // --- Parsed document tree navigation ---

        [Fact]
        public void ParsedDocument_ParentNavigation()
        {
            var doc = HtmlParser.Parse("<html><head></head><body><div><p>text</p></div></body></html>");

            var p = doc.QuerySelector("p")!;
            var div = p.Parent as Element;
            Assert.NotNull(div);
            Assert.Equal("div", div!.TagName);

            var body = div.Parent as Element;
            Assert.NotNull(body);
            Assert.Equal("body", body!.TagName);

            var html = body.Parent as Element;
            Assert.NotNull(html);
            Assert.Equal("html", html!.TagName);

            Assert.Same(doc, html.Parent);
        }

        [Fact]
        public void ParsedDocument_SiblingNavigation()
        {
            var doc = HtmlParser.Parse("<html><head></head><body><p>1</p><p>2</p><p>3</p></body></html>");

            var first = doc.Body!.FirstChild as Element;
            Assert.NotNull(first);
            Assert.Equal("1", first!.TextContent);

            var second = first.NextSibling as Element;
            Assert.NotNull(second);
            Assert.Equal("2", second!.TextContent);
            Assert.Same(first, second.PreviousSibling);

            var third = second.NextSibling as Element;
            Assert.NotNull(third);
            Assert.Equal("3", third!.TextContent);
            Assert.Same(second, third.PreviousSibling);

            Assert.Null(third.NextSibling);
            Assert.Null(first.PreviousSibling);
        }

        [Fact]
        public void Document_NodeType_IsDocument()
        {
            var doc = CreateDoc();
            Assert.Equal(NodeType.Document, doc.NodeType);
        }

        [Fact]
        public void Document_TextContent_ReturnsEmpty()
        {
            var doc = HtmlParser.Parse("<html><head></head><body>Text</body></html>");
            Assert.Equal(string.Empty, doc.TextContent);
        }

        [Fact]
        public void AppendChild_Self_Throws()
        {
            var doc = CreateDoc();
            var div = doc.CreateElement("div");

            Assert.Throws<System.InvalidOperationException>(() => div.AppendChild(div));
        }

        [Fact]
        public void RemoveChild_NotAChild_Throws()
        {
            var doc = CreateDoc();
            var parent = doc.CreateElement("div");
            var notChild = doc.CreateElement("span");

            Assert.Throws<System.InvalidOperationException>(() => parent.RemoveChild(notChild));
        }
    }
}
