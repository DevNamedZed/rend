using Rend.Core.Values;

namespace Rend.Layout
{
    /// <summary>
    /// Configuration options for the layout engine.
    /// </summary>
    public sealed class LayoutOptions
    {
        /// <summary>Page size in points. Defaults to A4.</summary>
        public SizeF PageSize { get; set; } = Rend.Core.Values.PageSize.A4;

        /// <summary>Page margins in points.</summary>
        public float MarginTop { get; set; } = 72f;
        public float MarginRight { get; set; } = 72f;
        public float MarginBottom { get; set; } = 72f;
        public float MarginLeft { get; set; } = 72f;

        /// <summary>Viewport width in CSS pixels (for percentage resolution).</summary>
        public float ViewportWidth { get; set; } = 816f; // A4 at 96 DPI minus margins

        /// <summary>Viewport height in CSS pixels.</summary>
        public float ViewportHeight { get; set; } = 1056f;

        /// <summary>Default font size in CSS pixels.</summary>
        public float DefaultFontSize { get; set; } = 16f;

        /// <summary>Whether to paginate the output.</summary>
        public bool Paginate { get; set; } = true;

        /// <summary>Default options.</summary>
        public static readonly LayoutOptions Default = new LayoutOptions();
    }
}
