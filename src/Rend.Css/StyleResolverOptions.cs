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

        /// <summary>Default options.</summary>
        public static readonly StyleResolverOptions Default = new StyleResolverOptions();
    }
}
