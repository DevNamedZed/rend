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
        Style, Media, FontFace, Import, Page, Supports, Namespace, Keyframes, Layer, Container
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
    /// A @container rule containing a container query condition and nested rules.
    /// </summary>
    public sealed class ContainerRule : CssRule
    {
        public override CssRuleType Type => CssRuleType.Container;

        /// <summary>The raw container query text (e.g. "sidebar (min-width: 400px)").</summary>
        public string ConditionText { get; }

        /// <summary>Nested rules inside this @container block.</summary>
        public IReadOnlyList<CssRule> Rules { get; }

        internal ContainerRule(string conditionText, List<CssRule> rules)
        {
            ConditionText = conditionText;
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

        /// <summary>
        /// Parsed unicode-range values from the unicode-range descriptor, if present.
        /// Returns null if no unicode-range descriptor is specified.
        /// </summary>
        public IReadOnlyList<UnicodeRange>? UnicodeRanges
        {
            get
            {
                for (int i = 0; i < Declarations.Count; i++)
                {
                    if (Declarations[i].Property == "unicode-range")
                    {
                        return UnicodeRange.Parse(Declarations[i].Value.ToString());
                    }
                }
                return null;
            }
        }

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
    /// A @supports rule containing a feature query and nested rules.
    /// </summary>
    public sealed class SupportsRule : CssRule
    {
        public override CssRuleType Type => CssRuleType.Supports;

        /// <summary>The raw condition text.</summary>
        public string ConditionText { get; }

        /// <summary>Nested rules inside this @supports block.</summary>
        public IReadOnlyList<CssRule> Rules { get; }

        internal SupportsRule(string conditionText, List<CssRule> rules)
        {
            ConditionText = conditionText;
            Rules = rules;
        }
    }

    /// <summary>
    /// A @namespace rule declaring an XML namespace prefix.
    /// e.g. @namespace svg "http://www.w3.org/2000/svg";
    /// </summary>
    public sealed class NamespaceRule : CssRule
    {
        public override CssRuleType Type => CssRuleType.Namespace;

        /// <summary>The namespace prefix (e.g. "svg"), or null for the default namespace.</summary>
        public string? Prefix { get; }

        /// <summary>The namespace URI.</summary>
        public string Uri { get; }

        internal NamespaceRule(string? prefix, string uri)
        {
            Prefix = prefix;
            Uri = uri;
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

    /// <summary>
    /// A @keyframes rule defining an animation's keyframes.
    /// </summary>
    public sealed class KeyframesRule : CssRule
    {
        public override CssRuleType Type => CssRuleType.Keyframes;

        /// <summary>The animation name.</summary>
        public string Name { get; }

        /// <summary>The keyframe stops.</summary>
        public IReadOnlyList<Keyframe> Keyframes { get; }

        internal KeyframesRule(string name, List<Keyframe> keyframes)
        {
            Name = name;
            Keyframes = keyframes;
        }
    }

    /// <summary>
    /// A single keyframe stop within a @keyframes rule.
    /// </summary>
    public sealed class Keyframe
    {
        /// <summary>The selector text (e.g. "from", "to", "50%").</summary>
        public string Selector { get; }

        /// <summary>The declarations at this keyframe stop.</summary>
        public IReadOnlyList<CssDeclaration> Declarations { get; }

        internal Keyframe(string selector, List<CssDeclaration> declarations)
        {
            Selector = selector;
            Declarations = declarations;
        }
    }

    /// <summary>
    /// A @layer rule declaring cascade layers.
    /// Can be a declaration form (@layer name;) or block form (@layer name { ... }).
    /// </summary>
    public sealed class LayerRule : CssRule
    {
        public override CssRuleType Type => CssRuleType.Layer;

        /// <summary>The layer name(s). Dotted names represent nested layers (e.g. "framework.base").</summary>
        public IReadOnlyList<string> Names { get; }

        /// <summary>Nested rules inside this layer block. Empty for declaration-only form.</summary>
        public IReadOnlyList<CssRule> Rules { get; }

        /// <summary>True if this is a block rule (has { } body), false if declaration-only.</summary>
        public bool IsBlock { get; }

        internal LayerRule(List<string> names, List<CssRule> rules, bool isBlock)
        {
            Names = names;
            Rules = rules;
            IsBlock = isBlock;
        }
    }
}
