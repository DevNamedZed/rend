using System;

namespace Rend.Css
{
    /// <summary>
    /// Options for style resolution (cascade + inheritance + computed values).
    /// </summary>
    public sealed class StyleResolverOptions
    {
        /// <summary>The media type ("screen" or "print").</summary>
        public string MediaType { get; set; } = "screen";

        /// <summary>Viewport width in px.</summary>
        public float ViewportWidth { get; set; } = 1920;

        /// <summary>Viewport height in px.</summary>
        public float ViewportHeight { get; set; } = 1080;

        /// <summary>Default root font size in px.</summary>
        public float DefaultFontSize { get; set; } = 16;

        /// <summary>Whether to apply the user-agent default stylesheet.</summary>
        public bool ApplyUserAgentStyles { get; set; } = true;

        /// <summary>Whether the user prefers a dark color scheme.</summary>
        public bool PrefersColorSchemeDark { get; set; }

        /// <summary>Whether the user prefers reduced motion. Default: true (static output).</summary>
        public bool PrefersReducedMotion { get; set; } = true;

        /// <summary>
        /// [CSS-VALUES-4 §6.1] Callback to measure the advance width of a character
        /// for font-relative units (ch). Arguments: (fontFamilies[], fontSize, codePoint).
        /// Returns the advance width in px. When null, ch falls back to 0.5em approximation.
        /// </summary>
        public Func<string[], float, int, float>? MeasureCharWidth { get; set; }

        /// <summary>Default options.</summary>
        public static readonly StyleResolverOptions Default = new StyleResolverOptions();
    }
}
