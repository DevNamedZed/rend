using System.Collections.Generic;
using Rend.Adapters;
using Rend.Css;
using Rend.Fonts;
using Rend.Html;
using Rend.Style.Internal;

namespace Rend.Style
{
    /// <summary>
    /// Walks the DOM tree, resolves ComputedStyle for each element via the CSS cascade,
    /// and produces a StyledTree for layout.
    /// </summary>
    public sealed class StyleTreeBuilder
    {
        private readonly StyleResolver _resolver;
        private readonly IFontProvider? _fontProvider;
        private readonly CounterTracker _counterTracker = new CounterTracker();

        public StyleTreeBuilder(StyleResolver resolver, IFontProvider? fontProvider = null)
        {
            _resolver = resolver;
            _fontProvider = fontProvider;
        }

        /// <summary>
        /// Build a styled tree from a parsed HTML document and its stylesheets.
        /// </summary>
        public StyledTree Build(Document document, IReadOnlyList<Stylesheet> stylesheets)
        {
            // Add all stylesheets to the resolver
            for (int i = 0; i < stylesheets.Count; i++)
            {
                _resolver.AddStylesheet(stylesheets[i]);

                // Process @font-face rules
                if (_fontProvider != null)
                    FontFaceProcessor.Process(stylesheets[i].Rules, _fontProvider);
            }

            // Collect page style info from all stylesheets
            var pageStyle = new PageStyleInfo();
            for (int i = 0; i < stylesheets.Count; i++)
            {
                var ps = PageStyleProcessor.Process(stylesheets[i].Rules);
                // Last @page rule wins for each property
                pageStyle = ps;
            }

            // Find the root element
            var root = document.DocumentElement;
            if (root == null)
            {
                // Create a minimal styled element if no root
                var emptyStyle = _resolver.Resolve(
                    new StylableElementAdapter(document.Body ?? CreateFallbackElement(document)),
                    null);
                return new StyledTree(
                    new StyledElement(document.Body ?? CreateFallbackElement(document), emptyStyle, new List<StyledNode>()),
                    pageStyle);
            }

            var styledRoot = BuildElement(root, null);
            return new StyledTree(styledRoot, pageStyle);
        }

        private StyledElement BuildElement(Element element, ComputedStyle? parentStyle)
        {
            var adapter = new StylableElementAdapter(element);
            var computedStyle = _resolver.Resolve(adapter, parentStyle);

            // contain: style/content/strict scopes counters to the subtree
            var contain = computedStyle.Contain;
            bool scopeCounters = contain == CssContain.Style ||
                                 contain == CssContain.Content ||
                                 contain == CssContain.Strict;
            if (scopeCounters)
                _counterTracker.PushScope();

            // Process CSS counters
            _counterTracker.ProcessCounterReset(computedStyle);
            _counterTracker.ProcessCounterIncrement(computedStyle);
            _counterTracker.ProcessCounterSet(computedStyle);

            var children = new List<StyledNode>();

            // ::before pseudo-element (inserted as first child)
            var beforeStyle = _resolver.ResolvePseudoElement(adapter, "before", computedStyle);
            if (beforeStyle != null)
            {
                var content = GetContentText(beforeStyle, element, _counterTracker);
                if (content != null)
                    children.Add(new StyledPseudoElement("before", content, beforeStyle));
            }

            var child = element.FirstChild;
            while (child != null)
            {
                if (child is Element childEl)
                {
                    children.Add(BuildElement(childEl, computedStyle));
                }
                else if (child is TextNode textNode)
                {
                    var text = textNode.Data;
                    if (!string.IsNullOrEmpty(text))
                    {
                        children.Add(new StyledText(text, computedStyle));
                    }
                }

                child = child.NextSibling;
            }

            // ::after pseudo-element (inserted as last child)
            var afterStyle = _resolver.ResolvePseudoElement(adapter, "after", computedStyle);
            if (afterStyle != null)
            {
                var content = GetContentText(afterStyle, element, _counterTracker);
                if (content != null)
                    children.Add(new StyledPseudoElement("after", content, afterStyle));
            }

            // Pop counter scope for style containment
            if (scopeCounters)
                _counterTracker.PopScope();

            var styledElement = new StyledElement(element, computedStyle, children);

            // ::first-letter pseudo-element (style override for first letter of block text)
            var firstLetterStyle = _resolver.ResolvePseudoElement(adapter, "first-letter", computedStyle);
            if (firstLetterStyle != null)
                styledElement.FirstLetterStyle = firstLetterStyle;

            // ::first-line pseudo-element (style override for first formatted line)
            var firstLineStyle = _resolver.ResolvePseudoElement(adapter, "first-line", computedStyle);
            if (firstLineStyle != null)
                styledElement.FirstLineStyle = firstLineStyle;

            return styledElement;
        }

        /// <summary>
        /// Extracts the text content from a pseudo-element's computed style.
        /// Returns null if content is "none", "normal", or empty.
        /// Resolves attr() and counter() functions against the owning element.
        /// </summary>
        private static string? GetContentText(ComputedStyle style, Element element,
            CounterTracker? counters = null)
        {
            var rawContent = style.ContentRaw;
            if (rawContent == null) return null;

            // Handle CssValue types (function values like attr(), counter(), string values, lists)
            if (rawContent is CssFunctionValue fn)
            {
                return ResolveContentFunction(fn, element, counters);
            }

            if (rawContent is CssKeywordValue contentKw)
            {
                if (contentKw.Keyword == "open-quote" && counters != null)
                    return counters.GetOpenQuote(style);
                if (contentKw.Keyword == "close-quote" && counters != null)
                    return counters.GetCloseQuote(style);
                if (contentKw.Keyword == "none" || contentKw.Keyword == "normal")
                    return null;
            }

            if (rawContent is CssListValue list)
            {
                return ResolveContentList(list, element, counters, style);
            }

            // Fall back to string representation
            var content = style.Content;
            if (string.IsNullOrEmpty(content)) return null;
            if (content == "none" || content == "normal") return null;
            return content;
        }

        private static string? ResolveContentFunction(CssFunctionValue fn, Element element,
            CounterTracker? counters)
        {
            if (fn.Name == "attr")
                return ResolveAttrFunction(fn, element);
            if (fn.Name == "counter" && counters != null)
                return ResolveCounterFunction(fn, counters);
            if (fn.Name == "counters" && counters != null)
                return ResolveCountersFunction(fn, counters);
            return null;
        }

        private static string? ResolveAttrFunction(CssFunctionValue fn, Element element)
        {
            if (fn.Arguments.Count == 0) return null;
            string? attrName = null;
            if (fn.Arguments[0] is CssKeywordValue kw)
                attrName = kw.Keyword;
            else
                attrName = fn.Arguments[0].ToString();

            if (string.IsNullOrEmpty(attrName)) return null;
            return element.GetAttribute(attrName);
        }

        private static string? ResolveCounterFunction(CssFunctionValue fn, CounterTracker counters)
        {
            if (fn.Arguments.Count == 0) return null;
            string? counterName = null;
            if (fn.Arguments[0] is CssKeywordValue kw)
                counterName = kw.Keyword;
            else
                counterName = fn.Arguments[0].ToString();

            if (string.IsNullOrEmpty(counterName)) return null;

            // Optional second argument: list-style-type
            string? style = null;
            if (fn.Arguments.Count >= 2 && fn.Arguments[1] is CssKeywordValue styleKw)
                style = styleKw.Keyword;

            return counters.FormatCounter(counterName, style);
        }

        private static string? ResolveCountersFunction(CssFunctionValue fn, CounterTracker counters)
        {
            if (fn.Arguments.Count < 2) return null;
            string? counterName = null;
            if (fn.Arguments[0] is CssKeywordValue kw)
                counterName = kw.Keyword;
            else
                counterName = fn.Arguments[0].ToString();

            if (string.IsNullOrEmpty(counterName)) return null;

            // Second argument: separator string
            string separator = ".";
            if (fn.Arguments[1] is CssStringValue sv)
                separator = sv.Value;
            else
                separator = fn.Arguments[1].ToString() ?? ".";

            // Optional third argument: list-style-type
            string? style = null;
            if (fn.Arguments.Count >= 3 && fn.Arguments[2] is CssKeywordValue styleKw)
                style = styleKw.Keyword;

            return counters.FormatCounters(counterName, separator, style);
        }

        private static string? ResolveContentList(CssListValue list, Element element,
            CounterTracker? counters, ComputedStyle? style = null)
        {
            var sb = new System.Text.StringBuilder();
            for (int i = 0; i < list.Values.Count; i++)
            {
                var val = list.Values[i];
                if (val is CssStringValue sv)
                    sb.Append(sv.Value);
                else if (val is CssFunctionValue fn)
                {
                    var resolved = ResolveContentFunction(fn, element, counters);
                    if (resolved != null) sb.Append(resolved);
                }
                else if (val is CssKeywordValue kw)
                {
                    if (kw.Keyword == "open-quote" && counters != null && style != null)
                        sb.Append(counters.GetOpenQuote(style));
                    else if (kw.Keyword == "close-quote" && counters != null && style != null)
                        sb.Append(counters.GetCloseQuote(style));
                    else if (kw.Keyword != "none" && kw.Keyword != "normal")
                        sb.Append(kw.Keyword);
                }
            }
            return sb.Length > 0 ? sb.ToString() : null;
        }

        private static Element CreateFallbackElement(Document document)
        {
            return document.CreateElement("div");
        }
    }
}
