using System;
using System.Collections.Generic;
using Rend.Core.Values;
using Rend.Css.Cascade.Internal;
using Rend.Css.Media.Internal;
using Rend.Css.Resolution.Internal;
using Rend.Css.Supports.Internal;
using Rend.Css.UserAgent.Internal;

namespace Rend.Css
{
    /// <summary>
    /// Public API for resolving CSS styles for elements.
    /// Performs the full cascade: user-agent defaults → author stylesheets → inline styles → inheritance.
    /// </summary>
    public sealed class StyleResolver
    {
        private readonly ISelectorMatcher _matcher;
        private readonly StyleResolverOptions _options;
        private readonly MediaContext _mediaContext;
        private readonly List<Stylesheet> _authorStylesheets = new List<Stylesheet>();
        private Stylesheet? _filteredUserAgent;

        public StyleResolver(ISelectorMatcher matcher, StyleResolverOptions? options = null)
        {
            _matcher = matcher ?? throw new ArgumentNullException(nameof(matcher));
            _options = options ?? StyleResolverOptions.Default;
            _mediaContext = new MediaContext(_options.ViewportWidth, _options.ViewportHeight, _options.MediaType)
            {
                PrefersColorSchemeDark = _options.PrefersColorSchemeDark,
                PrefersReducedMotion = _options.PrefersReducedMotion
            };
        }

        /// <summary>
        /// Add an author stylesheet to the cascade.
        /// Stylesheets are applied in the order they are added.
        /// </summary>
        public void AddStylesheet(Stylesheet stylesheet)
        {
            _authorStylesheets.Add(stylesheet);
        }

        /// <summary>
        /// Add multiple author stylesheets.
        /// </summary>
        public void AddStylesheets(IEnumerable<Stylesheet> stylesheets)
        {
            _authorStylesheets.AddRange(stylesheets);
        }

        /// <summary>
        /// Resolve the computed style for an element.
        /// </summary>
        /// <param name="element">The element to resolve styles for.</param>
        /// <param name="parentStyle">The parent element's computed style, or null for root.</param>
        public ComputedStyle Resolve(IStylableElement element, ComputedStyle? parentStyle = null)
        {
            var collector = new CascadeCollector(_matcher);
            var declarations = new List<CascadedDeclaration>();

            // 1. User-agent stylesheet (lowest priority)
            if (_options.ApplyUserAgentStyles)
            {
                var ua = GetFilteredUserAgentStylesheet();
                collector.Collect(element, ua, CascadeOrigin.UserAgent, declarations);
            }

            // 2. Author stylesheets
            foreach (var sheet in _authorStylesheets)
            {
                var filtered = FilterMediaRules(sheet);
                collector.Collect(element, filtered, CascadeOrigin.Author, declarations);
            }

            // 3. Inline styles (highest specificity for author origin)
            collector.CollectInlineStyle(element, declarations);

            // 4. Resolve cascade winners
            var winners = CascadeSorter.ResolveWinners(declarations);

            // 5. Build computed style with inheritance
            float parentFontSize = parentStyle?.FontSize ?? _options.DefaultFontSize;
            var ctx = new CssResolutionContext(
                parentFontSize,
                _options.DefaultFontSize,
                _options.ViewportWidth,
                _options.ViewportHeight,
                _options.ViewportWidth); // PercentBase: viewport width for percentage resolution

            var builder = new ComputedStyleBuilder(ctx);
            return builder.Build(winners, parentStyle);
        }

        /// <summary>
        /// Resolve the computed style for a pseudo-element (::before, ::after, ::first-letter, ::first-line).
        /// Returns null if no rules target the pseudo-element.
        /// For ::before/::after, also returns null if content is "none"/empty.
        /// </summary>
        /// <param name="element">The element the pseudo-element belongs to.</param>
        /// <param name="pseudoElement">The pseudo-element name.</param>
        /// <param name="elementStyle">The parent element's computed style.</param>
        public ComputedStyle? ResolvePseudoElement(IStylableElement element, string pseudoElement, ComputedStyle elementStyle)
        {
            var collector = new CascadeCollector(_matcher);
            var declarations = new List<CascadedDeclaration>();

            // 1. User-agent stylesheet
            if (_options.ApplyUserAgentStyles)
            {
                var ua = GetFilteredUserAgentStylesheet();
                collector.CollectPseudoElement(element, ua, CascadeOrigin.UserAgent, pseudoElement, declarations);
            }

            // 2. Author stylesheets
            foreach (var sheet in _authorStylesheets)
            {
                var filtered = FilterMediaRules(sheet);
                collector.CollectPseudoElement(element, filtered, CascadeOrigin.Author, pseudoElement, declarations);
            }

            // No pseudo-element rules found
            if (declarations.Count == 0) return null;

            // 3. Resolve cascade winners
            var winners = CascadeSorter.ResolveWinners(declarations);

            // 4. Check if content property is set (required for ::before/::after to generate)
            //    ::first-letter and ::first-line don't need content — they style existing text
            bool needsContent = pseudoElement == "before" || pseudoElement == "after";
            if (needsContent && !winners.ContainsKey("content")) return null;

            // 5. Build computed style (inherits from the element, not the element's parent)
            float parentFontSize = elementStyle.FontSize;
            var ctx = new CssResolutionContext(
                parentFontSize,
                _options.DefaultFontSize,
                _options.ViewportWidth,
                _options.ViewportHeight,
                _options.ViewportWidth); // PercentBase: viewport width for percentage resolution

            var builder = new ComputedStyleBuilder(ctx);
            return builder.Build(winners, elementStyle);
        }

        private Stylesheet GetFilteredUserAgentStylesheet()
        {
            if (_filteredUserAgent != null) return _filteredUserAgent;
            var ua = UserAgentStylesheet.Get();
            _filteredUserAgent = FilterMediaRules(ua);
            return _filteredUserAgent;
        }

        /// <summary>
        /// Filter a stylesheet by evaluating @media rules against the current context.
        /// Returns a new stylesheet with only the rules that match.
        /// </summary>
        private Stylesheet FilterMediaRules(Stylesheet sheet)
        {
            var filtered = new List<CssRule>();
            FilterRules(sheet.Rules, filtered);
            return new Stylesheet(filtered);
        }

        private void FilterRules(IReadOnlyList<CssRule> rules, List<CssRule> output)
        {
            for (int i = 0; i < rules.Count; i++)
            {
                var rule = rules[i];

                if (rule is MediaRule mr)
                {
                    if (MediaQueryEvaluator.Evaluate(mr.MediaText, _mediaContext))
                    {
                        // Flatten: add the nested rules directly
                        FilterRules(mr.Rules, output);
                    }
                    // else: skip this @media block entirely
                }
                else if (rule is SupportsRule sr)
                {
                    if (SupportsEvaluator.Evaluate(sr.ConditionText))
                    {
                        // Flatten: add the nested rules directly
                        FilterRules(sr.Rules, output);
                    }
                    // else: skip this @supports block entirely
                }
                else if (rule is ContainerRule cr)
                {
                    // Evaluate container query against viewport dimensions as initial containing block.
                    // In a single-pass static renderer, this is the best approximation.
                    if (ContainerQueryEvaluator.Evaluate(cr.ConditionText, _mediaContext))
                    {
                        FilterRules(cr.Rules, output);
                    }
                }
                else
                {
                    output.Add(rule);
                }
            }
        }
    }
}
