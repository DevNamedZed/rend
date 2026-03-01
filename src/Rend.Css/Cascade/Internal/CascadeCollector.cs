using System;
using System.Collections.Generic;

namespace Rend.Css.Cascade.Internal
{
    /// <summary>
    /// Collects all CSS declarations that apply to an element, from all stylesheets.
    /// Each declaration is paired with its cascade priority.
    /// </summary>
    internal sealed class CascadeCollector
    {
        private readonly ISelectorMatcher _matcher;
        private int _sourceOrder;

        public CascadeCollector(ISelectorMatcher matcher)
        {
            _matcher = matcher;
        }

        /// <summary>
        /// Collect all declarations matching the element from the given stylesheet.
        /// Only collects rules that have no pseudo-element (normal element styles).
        /// </summary>
        public void Collect(IStylableElement element, Stylesheet stylesheet, CascadeOrigin origin,
            List<CascadedDeclaration> output)
        {
            CollectRules(element, stylesheet.Rules, origin, output, null);
        }

        /// <summary>
        /// Collect declarations matching the element for a specific pseudo-element (e.g. "before", "after").
        /// Only collects rules whose selector targets the given pseudo-element.
        /// </summary>
        public void CollectPseudoElement(IStylableElement element, Stylesheet stylesheet, CascadeOrigin origin,
            string pseudoElement, List<CascadedDeclaration> output)
        {
            CollectRules(element, stylesheet.Rules, origin, output, pseudoElement);
        }

        private void CollectRules(IStylableElement element, IReadOnlyList<CssRule> rules, CascadeOrigin origin,
            List<CascadedDeclaration> output, string? targetPseudo)
        {
            for (int i = 0; i < rules.Count; i++)
            {
                var rule = rules[i];

                if (rule is StyleRule sr)
                {
                    var selectorPseudo = ExtractPseudoElement(sr.SelectorText);

                    // Filter: only collect if pseudo-element matches what we're looking for
                    if (targetPseudo == null && selectorPseudo != null) continue;
                    if (targetPseudo != null && selectorPseudo != targetPseudo) continue;

                    if (_matcher.Matches(element, sr.SelectorText))
                    {
                        var specificity = _matcher.GetSpecificity(sr.SelectorText);
                        for (int j = 0; j < sr.Declarations.Count; j++)
                        {
                            var decl = sr.Declarations[j];
                            var priority = new CascadePriority(origin, decl.Important, specificity, _sourceOrder++);
                            output.Add(new CascadedDeclaration(decl, priority));
                        }
                    }
                }
                else if (rule is MediaRule mr)
                {
                    CollectRules(element, mr.Rules, origin, output, targetPseudo);
                }
                else if (rule is SupportsRule sup)
                {
                    CollectRules(element, sup.Rules, origin, output, targetPseudo);
                }
                else if (rule is LayerRule lr && lr.IsBlock)
                {
                    CollectRules(element, lr.Rules, origin, output, targetPseudo);
                }
            }
        }

        /// <summary>
        /// Extracts the pseudo-element name from a selector text, or null if none.
        /// e.g. "p::before" → "before", "div::after" → "after", "p" → null
        /// </summary>
        internal static string? ExtractPseudoElement(string selectorText)
        {
            int idx = selectorText.LastIndexOf("::", StringComparison.Ordinal);
            if (idx < 0) return null;

            // Extract the pseudo-element name after ::
            string pseudo = selectorText.Substring(idx + 2).Trim().ToLowerInvariant();

            if (pseudo == "before" || pseudo == "after" || pseudo == "first-letter" || pseudo == "first-line")
                return pseudo;

            return null;
        }

        /// <summary>
        /// Collect inline style declarations for an element.
        /// </summary>
        public void CollectInlineStyle(IStylableElement element, List<CascadedDeclaration> output)
        {
            var inlineStyle = element.InlineStyle;
            if (string.IsNullOrEmpty(inlineStyle)) return;

            // Parse the inline style as a declaration block
            var css = "x{" + inlineStyle + "}";
            var parser = new Parser.Internal.CssParserCore(css);
            var sheet = parser.ParseStylesheet();

            if (sheet.Rules.Count > 0 && sheet.Rules[0] is StyleRule sr)
            {
                var specificity = CssSpecificity.InlineStyle;
                for (int j = 0; j < sr.Declarations.Count; j++)
                {
                    var decl = sr.Declarations[j];
                    var priority = new CascadePriority(CascadeOrigin.Author, decl.Important, specificity, _sourceOrder++);
                    output.Add(new CascadedDeclaration(decl, priority));
                }
            }
        }

        /// <summary>Reset source order counter (call between elements).</summary>
        public void ResetSourceOrder()
        {
            _sourceOrder = 0;
        }
    }

    /// <summary>
    /// A declaration paired with its cascade priority.
    /// </summary>
    internal readonly struct CascadedDeclaration
    {
        public CssDeclaration Declaration { get; }
        public CascadePriority Priority { get; }

        public CascadedDeclaration(CssDeclaration declaration, CascadePriority priority)
        {
            Declaration = declaration;
            Priority = priority;
        }
    }
}
