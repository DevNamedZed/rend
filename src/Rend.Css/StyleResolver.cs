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
        private readonly CascadeCollector _collector;
        private readonly List<CascadedDeclaration> _declarations = new List<CascadedDeclaration>();
        private readonly List<Stylesheet> _filteredSheets = new List<Stylesheet>();
        private bool _sheetsFiltered;
        private float _rootFontSize;

        public StyleResolver(ISelectorMatcher matcher, StyleResolverOptions? options = null)
        {
            _matcher = matcher ?? throw new ArgumentNullException(nameof(matcher));
            _options = options ?? StyleResolverOptions.Default;
            _mediaContext = new MediaContext(_options.ViewportWidth, _options.ViewportHeight, _options.MediaType)
            {
                PrefersColorSchemeDark = _options.PrefersColorSchemeDark,
                PrefersReducedMotion = _options.PrefersReducedMotion
            };
            _collector = new CascadeCollector(_matcher);
            _rootFontSize = _options.DefaultFontSize;
        }

        /// <summary>
        /// Add an author stylesheet to the cascade.
        /// Stylesheets are applied in the order they are added.
        /// </summary>
        public void AddStylesheet(Stylesheet stylesheet)
        {
            _authorStylesheets.Add(stylesheet);
            _sheetsFiltered = false;
        }

        /// <summary>
        /// Add multiple author stylesheets.
        /// </summary>
        public void AddStylesheets(IEnumerable<Stylesheet> stylesheets)
        {
            _authorStylesheets.AddRange(stylesheets);
            _sheetsFiltered = false;
        }

        private void EnsureFilteredSheets()
        {
            if (_sheetsFiltered) return;
            _filteredSheets.Clear();
            for (int i = 0; i < _authorStylesheets.Count; i++)
                _filteredSheets.Add(FilterMediaRules(_authorStylesheets[i]));
            _sheetsFiltered = true;
        }

        /// <summary>
        /// Resolve the computed style for an element.
        /// </summary>
        /// <param name="element">The element to resolve styles for.</param>
        /// <param name="parentStyle">The parent element's computed style, or null for root.</param>
        public ComputedStyle Resolve(IStylableElement element, ComputedStyle? parentStyle = null)
        {
            _collector.ResetSourceOrder();
            _declarations.Clear();
            EnsureFilteredSheets();

            // 1. User-agent stylesheet (lowest priority)
            if (_options.ApplyUserAgentStyles)
            {
                var ua = GetFilteredUserAgentStylesheet();
                _collector.Collect(element, ua, CascadeOrigin.UserAgent, _declarations);
            }

            // 2. Author stylesheets (pre-filtered)
            for (int i = 0; i < _filteredSheets.Count; i++)
            {
                _collector.Collect(element, _filteredSheets[i], CascadeOrigin.Author, _declarations);
            }

            // 3. Inline styles (highest specificity for author origin)
            _collector.CollectInlineStyle(element, _declarations);

            // 3.5. HTML presentational hints (author origin, specificity 0)
            CollectPresentationalHints(element, _declarations);

            // 4. Resolve cascade winners
            var winners = CascadeSorter.ResolveWinners(_declarations);

            // 5. Build computed style with inheritance
            float parentFontSize = parentStyle?.FontSize ?? _options.DefaultFontSize;
            var ctx = new CssResolutionContext(
                parentFontSize,
                _rootFontSize,
                _options.ViewportWidth,
                _options.ViewportHeight,
                _options.ViewportWidth); // PercentBase: viewport width for percentage resolution

            var builder = new ComputedStyleBuilder(ctx, _options.MeasureCharWidth);
            var computed = builder.Build(winners, parentStyle);

            // [CSS-VALUES §6.1] Track root element's computed font-size for rem resolution.
            // The root element (<html>) has no parent style.
            if (parentStyle == null)
            {
                _rootFontSize = computed.FontSize;
            }

            return computed;
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
            _collector.ResetSourceOrder();
            _declarations.Clear();
            EnsureFilteredSheets();

            // 1. User-agent stylesheet
            if (_options.ApplyUserAgentStyles)
            {
                var ua = GetFilteredUserAgentStylesheet();
                _collector.CollectPseudoElement(element, ua, CascadeOrigin.UserAgent, pseudoElement, _declarations);
            }

            // 2. Author stylesheets (pre-filtered)
            for (int i = 0; i < _filteredSheets.Count; i++)
            {
                _collector.CollectPseudoElement(element, _filteredSheets[i], CascadeOrigin.Author, pseudoElement, _declarations);
            }

            // No pseudo-element rules found
            if (_declarations.Count == 0) return null;

            // 3. Resolve cascade winners
            var winners = CascadeSorter.ResolveWinners(_declarations);

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

            var builder = new ComputedStyleBuilder(ctx, _options.MeasureCharWidth);
            return builder.Build(winners, elementStyle);
        }

        /// <summary>
        /// HTML presentational hints: map HTML attributes (dir, align, etc.) to CSS declarations.
        /// These have author origin but specificity 0, so any CSS rule overrides them.
        /// </summary>
        private static void CollectPresentationalHints(IStylableElement element, List<CascadedDeclaration> declarations)
        {
            // HTML dir attribute → CSS direction property
            string? dir = element.GetAttribute("dir");
            if (dir != null)
            {
                string dirLower = dir.ToLowerInvariant();
                if (dirLower == "ltr" || dirLower == "rtl")
                {
                    var decl = new CssDeclaration("direction", new CssKeywordValue(dirLower));
                    var priority = new CascadePriority(CascadeOrigin.Author, false, CssSpecificity.Zero, -1);
                    declarations.Add(new CascadedDeclaration(decl, priority));
                }
                else if (dirLower == "auto")
                {
                    // [HTML §3.2.6.5] dir=auto: detect from first strong directional character
                    string resolved = DetectDirectionFromContent(element);
                    var decl = new CssDeclaration("direction", new CssKeywordValue(resolved));
                    var priority = new CascadePriority(CascadeOrigin.Author, false, CssSpecificity.Zero, -1);
                    declarations.Add(new CascadedDeclaration(decl, priority));
                }
            }
        }

        /// <summary>
        /// [HTML §3.2.6.5] Detect text directionality from the first strong
        /// Unicode directional character in the element's text content.
        /// Returns "ltr" or "rtl".
        /// </summary>
        private static string DetectDirectionFromContent(IStylableElement element)
        {
            string? text = element.TextContent;
            if (text != null)
            {
                for (int i = 0; i < text.Length; i++)
                {
                    char ch = text[i];
                    // Strong RTL: Arabic, Hebrew, Thaana, NKo, etc.
                    if ((ch >= 0x0590 && ch <= 0x05FF) ||  // Hebrew
                        (ch >= 0x0600 && ch <= 0x06FF) ||  // Arabic
                        (ch >= 0x0700 && ch <= 0x074F) ||  // Syriac
                        (ch >= 0x0780 && ch <= 0x07BF) ||  // Thaana
                        (ch >= 0x07C0 && ch <= 0x07FF) ||  // NKo
                        (ch >= 0xFB50 && ch <= 0xFDFF) ||  // Arabic Presentation A
                        (ch >= 0xFE70 && ch <= 0xFEFF))    // Arabic Presentation B
                    {
                        return "rtl";
                    }
                    // Strong LTR: Latin, Greek, Cyrillic, CJK, etc.
                    if ((ch >= 'A' && ch <= 'Z') ||
                        (ch >= 'a' && ch <= 'z') ||
                        (ch >= 0x00C0 && ch <= 0x024F) ||  // Latin Extended
                        (ch >= 0x0370 && ch <= 0x03FF) ||  // Greek
                        (ch >= 0x0400 && ch <= 0x04FF) ||  // Cyrillic
                        (ch >= 0x1100 && ch <= 0x11FF) ||  // Hangul Jamo
                        (ch >= 0x3040 && ch <= 0x309F) ||  // Hiragana
                        (ch >= 0x30A0 && ch <= 0x30FF) ||  // Katakana
                        (ch >= 0x4E00 && ch <= 0x9FFF))    // CJK
                    {
                        return "ltr";
                    }
                }
            }
            return "ltr"; // Default to LTR if no strong character found
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
