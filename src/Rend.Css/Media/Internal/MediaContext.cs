namespace Rend.Css.Media.Internal
{
    /// <summary>
    /// The current media context for evaluating @media queries.
    /// </summary>
    internal sealed class MediaContext
    {
        /// <summary>Viewport width in px.</summary>
        public float Width { get; set; }

        /// <summary>Viewport height in px.</summary>
        public float Height { get; set; }

        /// <summary>Media type: "screen", "print", "all".</summary>
        public string MediaType { get; set; } = "screen";

        /// <summary>Orientation: "portrait" or "landscape".</summary>
        public string Orientation => Width >= Height ? "landscape" : "portrait";

        /// <summary>Whether the user prefers a dark color scheme.</summary>
        public bool PrefersColorSchemeDark { get; set; }

        /// <summary>Whether the user prefers reduced motion (always true for static output).</summary>
        public bool PrefersReducedMotion { get; set; } = true;

        /// <summary>Whether the user prefers high contrast.</summary>
        public bool PrefersContrast { get; set; }

        /// <summary>Device resolution in DPI. Default: 96.</summary>
        public float Resolution { get; set; } = 96f;

        public MediaContext(float width, float height, string mediaType = "screen")
        {
            Width = width;
            Height = height;
            MediaType = mediaType;
        }
    }
}
