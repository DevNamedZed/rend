using System.Collections.Generic;
using Rend.Css;
using Rend.Html;

namespace Rend.Internal
{
    /// <summary>
    /// Extracts CSS stylesheets from a parsed HTML document:
    /// &lt;style&gt; inline blocks and &lt;link rel="stylesheet"&gt; references.
    /// </summary>
    internal static class HtmlStyleExtractor
    {
        public static List<Stylesheet> Extract(Document document)
        {
            var stylesheets = new List<Stylesheet>();
            var head = document.Head;
            if (head == null) return stylesheets;

            var child = head.FirstChild;
            while (child != null)
            {
                if (child is Element el)
                {
                    if (el.TagName == "style")
                    {
                        var cssText = el.TextContent;
                        if (!string.IsNullOrWhiteSpace(cssText))
                        {
                            var sheet = CssParser.Parse(cssText);
                            stylesheets.Add(sheet);
                        }
                    }
                    // Note: <link rel="stylesheet"> requires resource loading,
                    // handled separately by ResourceLoadingContext
                }

                child = child.NextSibling;
            }

            // Also check for <style> elements in <body>
            var body = document.Body;
            if (body != null)
                ExtractStylesFromSubtree(body, stylesheets);

            return stylesheets;
        }

        private static void ExtractStylesFromSubtree(Node node, List<Stylesheet> stylesheets)
        {
            var child = node.FirstChild;
            while (child != null)
            {
                if (child is Element el && el.TagName == "style")
                {
                    var cssText = el.TextContent;
                    if (!string.IsNullOrWhiteSpace(cssText))
                    {
                        var sheet = CssParser.Parse(cssText);
                        stylesheets.Add(sheet);
                    }
                }
                else if (child is Element container)
                {
                    ExtractStylesFromSubtree(container, stylesheets);
                }

                child = child.NextSibling;
            }
        }
    }
}
