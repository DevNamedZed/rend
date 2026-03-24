using Rend.Adapters;
using Rend.Core.Values;
using Rend.Css;
using Rend.Html.Parser;
using Rend.Internal;
using Rend.Layout;
using Rend.Layout.Internal;
using Rend.Style;

namespace Rend.Tests.Layout
{
    /// <summary>
    /// Helper for layout-level tests. Goes HTML → parse → style → layout
    /// and returns the layout tree for inspection. No rendering or image output.
    /// </summary>
    internal static class LayoutTestHelper
    {
        /// <summary>
        /// Parse HTML, resolve styles, and compute layout. Returns the root LayoutBox.
        /// </summary>
        public static LayoutBox Layout(string html, float viewportWidth = 400, float viewportHeight = 300)
        {
            var document = HtmlParser.Parse(html);

            var selectorMatcher = new SelectorMatcherAdapter();
            var resolverOptions = new StyleResolverOptions
            {
                ViewportWidth = viewportWidth,
                ViewportHeight = viewportHeight,
                DefaultFontSize = 16,
                ApplyUserAgentStyles = true,
            };
            var styleResolver = new StyleResolver(selectorMatcher, resolverOptions);
            var treeBuilder = new StyleTreeBuilder(styleResolver, null);
            var stylesheets = HtmlStyleExtractor.Extract(document);
            var styledTree = treeBuilder.Build(document, stylesheets);

            var pageSize = new SizeF(viewportWidth, viewportHeight);
            styledTree.PageStyle.PageSize = pageSize;
            styledTree.PageStyle.MarginTop = 0;
            styledTree.PageStyle.MarginRight = 0;
            styledTree.PageStyle.MarginBottom = 0;
            styledTree.PageStyle.MarginLeft = 0;

            var layoutEngine = new LayoutEngine(null, null);
            var layoutOptions = new LayoutOptions
            {
                PageSize = pageSize,
                ViewportWidth = viewportWidth,
                ViewportHeight = viewportHeight,
                MarginTop = 0,
                MarginRight = 0,
                MarginBottom = 0,
                MarginLeft = 0,
                DefaultFontSize = 16,
                Paginate = false,
            };
            var layoutDoc = layoutEngine.Layout(styledTree, layoutOptions);
            return layoutDoc.RootBox;
        }

        /// <summary>
        /// Find a descendant LayoutBox by tag name (first match, depth-first).
        /// </summary>
        public static LayoutBox? FindByTag(LayoutBox root, string tagName)
        {
            if (root.StyledNode is StyledElement element && element.TagName == tagName)
            {
                return root;
            }
            foreach (var child in root.Children)
            {
                var found = FindByTag(child, tagName);
                if (found != null)
                {
                    return found;
                }
            }
            return null;
        }

        /// <summary>
        /// Find all descendant LayoutBoxes matching a tag name.
        /// </summary>
        public static System.Collections.Generic.List<LayoutBox> FindAllByTag(LayoutBox root, string tagName)
        {
            var results = new System.Collections.Generic.List<LayoutBox>();
            FindAllByTagRecursive(root, tagName, results);
            return results;
        }

        private static void FindAllByTagRecursive(LayoutBox box, string tagName,
            System.Collections.Generic.List<LayoutBox> results)
        {
            if (box.StyledNode is StyledElement element && element.TagName == tagName)
            {
                results.Add(box);
            }
            foreach (var child in box.Children)
            {
                FindAllByTagRecursive(child, tagName, results);
            }
        }

        /// <summary>
        /// Find a descendant LayoutBox by CSS class (first match).
        /// </summary>
        public static LayoutBox? FindByClass(LayoutBox root, string className)
        {
            if (root.StyledNode is StyledElement element)
            {
                string? cls = element.GetAttribute("class");
                if (cls != null && cls.Contains(className))
                {
                    return root;
                }
            }
            foreach (var child in root.Children)
            {
                var found = FindByClass(child, className);
                if (found != null)
                {
                    return found;
                }
            }
            return null;
        }

        /// <summary>
        /// Find a descendant LayoutBox by id attribute.
        /// </summary>
        public static LayoutBox? FindById(LayoutBox root, string id)
        {
            if (root.StyledNode is StyledElement element && element.GetAttribute("id") == id)
            {
                return root;
            }
            foreach (var child in root.Children)
            {
                var found = FindById(child, id);
                if (found != null)
                {
                    return found;
                }
            }
            return null;
        }
    }
}
