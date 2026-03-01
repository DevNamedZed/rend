using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rend.Html.Conformance
{
    public static class Html5libTreeSerializer
    {
        private const string SvgNamespace = "http://www.w3.org/2000/svg";
        private const string MathMlNamespace = "http://www.w3.org/1998/Math/MathML";
        private const string XlinkNamespace = "http://www.w3.org/1999/xlink";
        private const string XmlNamespace = "http://www.w3.org/XML/1998/namespace";
        private const string XmlnsNamespace = "http://www.w3.org/2000/xmlns/";

        public static string Serialize(Document document)
        {
            var sb = new StringBuilder();
            SerializeChildren(document, sb, 0);
            return sb.ToString().TrimEnd('\n');
        }

        public static string SerializeFragment(DocumentFragment fragment)
        {
            var sb = new StringBuilder();
            SerializeChildren(fragment, sb, 0);
            return sb.ToString().TrimEnd('\n');
        }

        private static void SerializeChildren(Node parent, StringBuilder sb, int depth)
        {
            var child = parent.FirstChild;
            while (child != null)
            {
                SerializeNode(child, sb, depth);
                child = child.NextSibling;
            }
        }

        private static void SerializeNode(Node node, StringBuilder sb, int depth)
        {
            var indent = new string(' ', depth * 2);

            switch (node.NodeType)
            {
                case NodeType.DocumentType:
                {
                    var dt = (DocumentType)node;
                    sb.Append("| ");
                    sb.Append(indent);
                    sb.Append("<!DOCTYPE ");
                    sb.Append(dt.Name);
                    if (!string.IsNullOrEmpty(dt.PublicId) || !string.IsNullOrEmpty(dt.SystemId))
                    {
                        sb.Append(" \"");
                        sb.Append(dt.PublicId ?? "");
                        sb.Append("\" \"");
                        sb.Append(dt.SystemId ?? "");
                        sb.Append("\"");
                    }
                    sb.Append(">\n");
                    break;
                }

                case NodeType.Comment:
                {
                    var comment = (Comment)node;
                    sb.Append("| ");
                    sb.Append(indent);
                    sb.Append("<!-- ");
                    sb.Append(comment.Data);
                    sb.Append(" -->\n");
                    break;
                }

                case NodeType.Text:
                {
                    var text = (TextNode)node;
                    sb.Append("| ");
                    sb.Append(indent);
                    sb.Append("\"");
                    sb.Append(text.Data);
                    sb.Append("\"\n");
                    break;
                }

                case NodeType.Element:
                {
                    var element = (Element)node;
                    sb.Append("| ");
                    sb.Append(indent);
                    sb.Append("<");

                    // Namespace prefix for SVG and MathML elements
                    var ns = element.NamespaceUri;
                    if (ns == SvgNamespace)
                        sb.Append("svg ");
                    else if (ns == MathMlNamespace)
                        sb.Append("math ");

                    sb.Append(element.TagName);
                    sb.Append(">\n");

                    // Attributes sorted alphabetically
                    // Namespace-prefixed attributes use "prefix attrname" format
                    var attrs = new List<(string sortKey, string display, string value)>();
                    foreach (var attr in element.Attributes)
                    {
                        attrs.Add((attr.Name, attr.Name, attr.Value));
                    }
                    attrs.Sort((a, b) => string.Compare(a.sortKey, b.sortKey, StringComparison.Ordinal));

                    foreach (var (_, display, value) in attrs)
                    {
                        sb.Append("| ");
                        sb.Append(indent);
                        sb.Append("  ");
                        sb.Append(display);
                        sb.Append("=\"");
                        sb.Append(value);
                        sb.Append("\"\n");
                    }

                    // Template content pseudo-node
                    if (element.TagName == "template")
                    {
                        sb.Append("| ");
                        sb.Append(indent);
                        sb.Append("  content\n");
                        SerializeChildren(element, sb, depth + 2);
                    }
                    else
                    {
                        SerializeChildren(element, sb, depth + 1);
                    }
                    break;
                }
            }
        }
    }
}
