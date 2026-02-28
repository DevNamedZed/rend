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
        /// </summary>
        public void Collect(IStylableElement element, Stylesheet stylesheet, CascadeOrigin origin,
            List<CascadedDeclaration> output)
        {
            CollectRules(element, stylesheet.Rules, origin, output);
        }

        private void CollectRules(IStylableElement element, IReadOnlyList<CssRule> rules, CascadeOrigin origin,
            List<CascadedDeclaration> output)
        {
            for (int i = 0; i < rules.Count; i++)
            {
                var rule = rules[i];

                if (rule is StyleRule sr)
                {
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
                    // Media rules: collected at resolution time (after media evaluation)
                    // For now, recurse into all media rules (evaluator handles filtering upstream)
                    CollectRules(element, mr.Rules, origin, output);
                }
            }
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
