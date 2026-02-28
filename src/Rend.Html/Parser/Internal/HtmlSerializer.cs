using System.Text;

namespace Rend.Html.Parser.Internal
{
    /// <summary>
    /// Serializes DOM nodes back to HTML strings.
    /// Used by Element.InnerHtml, Element.OuterHtml.
    /// </summary>
    internal static class HtmlSerializer
    {
        internal static string Serialize(Element element)
        {
            var sb = new StringBuilder();
            SerializeElement(element, sb);
            return sb.ToString();
        }

        internal static string SerializeChildren(Node parent)
        {
            var sb = new StringBuilder();
            var child = parent.FirstChild;
            while (child != null)
            {
                SerializeNode(child, sb);
                child = child.NextSibling;
            }
            return sb.ToString();
        }

        internal static void ParseAndSetInnerHtml(Element element, string html)
        {
            // Clear existing children
            element.RemoveAllChildren();

            // Full implementation will use HtmlParser.ParseFragment in Phase 5.
            // For now, just set raw text content.
            if (!string.IsNullOrEmpty(html))
            {
                var doc = element.OwnerDocument;
                if (doc != null)
                    element.AppendChild(doc.CreateTextNode(html));
            }
        }

        private static void SerializeNode(Node node, StringBuilder sb)
        {
            switch (node)
            {
                case Element el:
                    SerializeElement(el, sb);
                    break;
                case TextNode text:
                    EscapeText(text.Data, sb);
                    break;
                case Comment comment:
                    sb.Append("<!--");
                    sb.Append(comment.Data);
                    sb.Append("-->");
                    break;
                case DocumentType dt:
                    sb.Append("<!DOCTYPE ");
                    sb.Append(dt.Name);
                    sb.Append('>');
                    break;
            }
        }

        private static void SerializeElement(Element el, StringBuilder sb)
        {
            sb.Append('<');
            sb.Append(el.TagName);

            var attrs = el.Attributes;
            for (int i = 0; i < attrs.Count; i++)
            {
                var attr = attrs[i];
                sb.Append(' ');
                sb.Append(attr.Name);
                if (attr.Value.Length > 0)
                {
                    sb.Append("=\"");
                    EscapeAttributeValue(attr.Value, sb);
                    sb.Append('"');
                }
            }

            sb.Append('>');

            // Void elements don't have closing tags
            if (IsVoidElement(el.TagName))
                return;

            var child = el.FirstChild;
            while (child != null)
            {
                SerializeNode(child, sb);
                child = child.NextSibling;
            }

            sb.Append("</");
            sb.Append(el.TagName);
            sb.Append('>');
        }

        private static void EscapeText(string text, StringBuilder sb)
        {
            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                switch (c)
                {
                    case '&': sb.Append("&amp;"); break;
                    case '<': sb.Append("&lt;"); break;
                    case '>': sb.Append("&gt;"); break;
                    default: sb.Append(c); break;
                }
            }
        }

        private static void EscapeAttributeValue(string value, StringBuilder sb)
        {
            for (int i = 0; i < value.Length; i++)
            {
                char c = value[i];
                switch (c)
                {
                    case '&': sb.Append("&amp;"); break;
                    case '"': sb.Append("&quot;"); break;
                    default: sb.Append(c); break;
                }
            }
        }

        private static bool IsVoidElement(string tagName)
        {
            // Reference equality on interned strings
            switch (tagName)
            {
                case "area":
                case "base":
                case "br":
                case "col":
                case "embed":
                case "hr":
                case "img":
                case "input":
                case "link":
                case "meta":
                case "param":
                case "source":
                case "track":
                case "wbr":
                    return true;
                default:
                    return false;
            }
        }
    }
}
