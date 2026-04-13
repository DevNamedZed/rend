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
    internal sealed class StyleTreeBuilder
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

            // contain: style/content/strict scopes counters to the subtree.
            var contain = computedStyle.Contain;
            bool scopeCounters = contain == CssContain.Style ||
                                 contain == CssContain.Content ||
                                 contain == CssContain.Strict;
            bool isListContainer = IsListContainerElement(element);
            bool isListItem = IsListItemElement(element);

            // [CSS-LISTS-3 §2] A counter created by counter-reset has a scope that
            // includes the element, its descendants, and its following siblings
            // (with their descendants). To achieve sibling visibility we write the
            // new counter into the parent's current scope rather than pushing a new
            // scope for this element. However, if a counter with the same name is
            // already in scope (inherited from an ancestor or preceding sibling),
            // this is a nested instance and must shadow the outer one only for
            // this element and its descendants — so we push a new scope in that
            // case so the outer counter is restored when we pop. If the reset
            // declaration uses an unsupported function (e.g. reversed()), we
            // preserve the old always-push semantics because our parser cannot
            // extract the real counter name and would otherwise leak state.
            var counterResetEntries = CounterTracker.GetCounterResetEntries(computedStyle);
            bool hasNestedCounterReset = false;
            if (counterResetEntries != null)
            {
                for (int i = 0; i < counterResetEntries.Count; i++)
                {
                    if (_counterTracker.IsCounterInScope(counterResetEntries[i].Name))
                    {
                        hasNestedCounterReset = true;
                        break;
                    }
                }
            }
            bool counterResetUsesFunction = CounterTracker.CounterResetHasFunctionValue(computedStyle);

            bool pushedCounterScope = scopeCounters
                || isListContainer
                || hasNestedCounterReset
                || counterResetUsesFunction;
            if (pushedCounterScope)
            {
                _counterTracker.PushScope();
            }

            if (counterResetEntries != null)
            {
                _counterTracker.ApplyCounterResetEntries(counterResetEntries);
            }
            if (isListContainer)
            {
                ApplyImplicitListContainerReset(element, computedStyle);
            }
            _counterTracker.ProcessCounterIncrement(computedStyle);
            if (isListItem)
            {
                ApplyImplicitListItemIncrement(element, computedStyle);
            }
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

            if (pushedCounterScope)
            {
                _counterTracker.PopScope();
            }

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

        private const string ListItemCounterName = "list-item";

        private static bool IsListContainerElement(Element element)
        {
            string tag = element.TagName;
            return string.Equals(tag, "ol", System.StringComparison.OrdinalIgnoreCase)
                || string.Equals(tag, "ul", System.StringComparison.OrdinalIgnoreCase)
                || string.Equals(tag, "menu", System.StringComparison.OrdinalIgnoreCase)
                || string.Equals(tag, "dir", System.StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsListItemElement(Element element)
        {
            return string.Equals(element.TagName, "li", System.StringComparison.OrdinalIgnoreCase);
        }

        // [CSS-LISTS-3 §3] UA rule: ol, ul, menu, dir { counter-reset: list-item }
        // Chrome's reversed-list initial value is count+1 (not the spec's §4.4.2 algorithm),
        // and the <ol start> attribute maps to counter-reset: list-item (start - 1).
        private void ApplyImplicitListContainerReset(Element element, ComputedStyle style)
        {
            if (CounterTracker.StyleHasCounterResetEntry(style, ListItemCounterName))
            {
                return;
            }

            int initialValue = 0;
            bool isOl = string.Equals(element.TagName, "ol", System.StringComparison.OrdinalIgnoreCase);
            if (isOl && element.GetAttribute("reversed") != null)
            {
                int itemCount = CountListItemChildren(element);
                initialValue = itemCount + 1;
            }
            else if (isOl)
            {
                string? startAttribute = element.GetAttribute("start");
                if (startAttribute != null && int.TryParse(startAttribute, out int startValue))
                {
                    initialValue = startValue - 1;
                }
            }

            _counterTracker.ResetCounterInCurrentScope(ListItemCounterName, initialValue);
        }

        // [CSS-LISTS-3 §3] UA rule: li { counter-increment: list-item }
        // Author counter-increment / counter-set / <li value=...> override the implicit increment.
        // A reversed parent list flips the implicit direction to -1.
        private void ApplyImplicitListItemIncrement(Element element, ComputedStyle style)
        {
            if (CounterTracker.StyleHasCounterIncrementEntry(style, ListItemCounterName))
            {
                return;
            }
            if (CounterTracker.StyleHasCounterSetEntry(style, ListItemCounterName))
            {
                return;
            }

            string? valueAttribute = element.GetAttribute("value");
            if (valueAttribute != null && int.TryParse(valueAttribute, out int attributeValue))
            {
                _counterTracker.ResetCounterInCurrentScope(ListItemCounterName, attributeValue);
                return;
            }

            int increment = IsInsideReversedListContainer(element) ? -1 : 1;
            _counterTracker.IncrementCounterInScope(ListItemCounterName, increment);
        }

        private static int CountListItemChildren(Element element)
        {
            int count = 0;
            var child = element.FirstChild;
            while (child != null)
            {
                if (child is Element childElement && IsListItemElement(childElement))
                {
                    count++;
                }
                child = child.NextSibling;
            }
            return count;
        }

        private static bool IsInsideReversedListContainer(Element element)
        {
            var parent = element.Parent;
            while (parent != null)
            {
                if (parent is Element parentElement)
                {
                    if (IsListContainerElement(parentElement))
                    {
                        return string.Equals(parentElement.TagName, "ol", System.StringComparison.OrdinalIgnoreCase)
                            && parentElement.GetAttribute("reversed") != null;
                    }
                }
                parent = parent.Parent;
            }
            return false;
        }
    }
}
