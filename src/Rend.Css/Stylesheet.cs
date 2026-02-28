using System.Collections.Generic;

namespace Rend.Css
{
    /// <summary>
    /// A parsed CSS stylesheet containing a list of rules.
    /// </summary>
    public sealed class Stylesheet
    {
        public IReadOnlyList<CssRule> Rules { get; }

        internal Stylesheet(List<CssRule> rules)
        {
            Rules = rules;
        }
    }

    /// <summary>
    /// Base class for CSS rules.
    /// </summary>
    public abstract class CssRule
    {
        public abstract CssRuleType Type { get; }
    }

    /// <summary>Kind discriminator for CSS rules.</summary>
    public enum CssRuleType : byte
    {
        Style, Media, FontFace, Import, Page
    }

    /// <summary>
    /// A CSS style rule: selector(s) + declarations.
    /// e.g. "div.main { color: red; margin: 10px; }"
    /// </summary>
    public sealed class StyleRule : CssRule
    {
        public override CssRuleType Type => CssRuleType.Style;

        /// <summary>The raw selector text.</summary>
        public string SelectorText { get; }

        /// <summary>The declarations in this rule (after shorthand expansion).</summary>
        public IReadOnlyList<CssDeclaration> Declarations { get; }

        internal StyleRule(string selectorText, List<CssDeclaration> declarations)
        {
            SelectorText = selectorText;
            Declarations = declarations;
        }
    }

    /// <summary>
    /// A @media rule containing a media query and nested rules.
    /// </summary>
    public sealed class MediaRule : CssRule
    {
        public override CssRuleType Type => CssRuleType.Media;

        /// <summary>The raw media query text.</summary>
        public string MediaText { get; }

        /// <summary>Nested rules inside this @media block.</summary>
        public IReadOnlyList<CssRule> Rules { get; }

        internal MediaRule(string mediaText, List<CssRule> rules)
        {
            MediaText = mediaText;
            Rules = rules;
        }
    }

    /// <summary>
    /// A @font-face rule with declarations for font properties.
    /// </summary>
    public sealed class FontFaceRule : CssRule
    {
        public override CssRuleType Type => CssRuleType.FontFace;

        public IReadOnlyList<CssDeclaration> Declarations { get; }

        internal FontFaceRule(List<CssDeclaration> declarations)
        {
            Declarations = declarations;
        }
    }

    /// <summary>
    /// A @import rule referencing an external stylesheet.
    /// </summary>
    public sealed class ImportRule : CssRule
    {
        public override CssRuleType Type => CssRuleType.Import;

        /// <summary>The URL of the imported stylesheet.</summary>
        public string Url { get; }

        /// <summary>Optional media query text.</summary>
        public string? MediaText { get; }

        internal ImportRule(string url, string? mediaText)
        {
            Url = url;
            MediaText = mediaText;
        }
    }

    /// <summary>
    /// A @page rule with page-specific declarations.
    /// </summary>
    public sealed class PageRule : CssRule
    {
        public override CssRuleType Type => CssRuleType.Page;

        /// <summary>The page selector (e.g. ":first", ":left"), or null for default.</summary>
        public string? PageSelector { get; }

        public IReadOnlyList<CssDeclaration> Declarations { get; }

        internal PageRule(string? pageSelector, List<CssDeclaration> declarations)
        {
            PageSelector = pageSelector;
            Declarations = declarations;
        }
    }
}
