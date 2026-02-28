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

        public MediaContext(float width, float height, string mediaType = "screen")
        {
            Width = width;
            Height = height;
            MediaType = mediaType;
        }
    }
}
