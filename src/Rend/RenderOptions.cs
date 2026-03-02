using System;
using Rend.Core;
using Rend.Core.Values;
using Rend.Fonts;


namespace Rend
{
    /// <summary>
    /// Configuration options for HTML-to-PDF/image rendering.
    /// </summary>
    public sealed class RenderOptions
    {
        /// <summary>Page size in points. Defaults to A4 (595.28 × 841.89 pt).</summary>
        public SizeF PageSize { get; set; } = Rend.Core.Values.PageSize.A4;

        /// <summary>Page margins in points. Default: 72pt (1 inch) on all sides.</summary>
        public float MarginTop { get; set; } = 72f;
        public float MarginRight { get; set; } = 72f;
        public float MarginBottom { get; set; } = 72f;
        public float MarginLeft { get; set; } = 72f;

        /// <summary>DPI for image output. Default: 96.</summary>
        public float Dpi { get; set; } = 96f;

        /// <summary>Base URL for resolving relative resource URLs.</summary>
        public Uri? BaseUrl { get; set; }

        /// <summary>Resource loader for external resources (CSS, images, fonts).</summary>
        public IResourceLoader? ResourceLoader { get; set; }

        /// <summary>Font provider. If null, a default system font provider is created.</summary>
        public IFontProvider? FontProvider { get; set; }

        /// <summary>Whether to generate PDF bookmarks from h1-h6 headings. Default: true.</summary>
        public bool GenerateBookmarks { get; set; } = true;

        /// <summary>Whether to generate PDF link annotations from &lt;a&gt; elements. Default: true.</summary>
        public bool GenerateLinks { get; set; } = true;

        /// <summary>Image output format ("png", "jpeg", "webp"). Default: "png".</summary>
        public string ImageFormat { get; set; } = "png";

        /// <summary>JPEG/WebP quality (1-100). Default: 90.</summary>
        public int ImageQuality { get; set; } = 90;

        /// <summary>PDF document title metadata.</summary>
        public string? Title { get; set; }

        /// <summary>PDF document author metadata.</summary>
        public string? Author { get; set; }

        /// <summary>Default font size in CSS pixels. Default: 16.</summary>
        public float DefaultFontSize { get; set; } = 16f;

        /// <summary>
        /// HTML content for page headers. Rendered in the top margin area of each page.
        /// Supports template variables: {pageNumber}, {totalPages}, {date}.
        /// </summary>
        public string? HeaderHtml { get; set; }

        /// <summary>
        /// HTML content for page footers. Rendered in the bottom margin area of each page.
        /// Supports template variables: {pageNumber}, {totalPages}, {date}.
        /// </summary>
        public string? FooterHtml { get; set; }

        /// <summary>
        /// CSS media type for style resolution ("screen" or "print"). Default: null.
        /// When null, ToImage uses "screen" and ToPdf uses "print".
        /// </summary>
        public string? MediaType { get; set; }

        /// <summary>Whether the user prefers a dark color scheme. Affects prefers-color-scheme media query.</summary>
        public bool PrefersColorSchemeDark { get; set; }

        /// <summary>Progress reporter. If set, receives progress updates during rendering.</summary>
        public IProgress<RenderProgress>? Progress { get; set; }

        /// <summary>Default options.</summary>
        public static readonly RenderOptions Default = new RenderOptions();
    }
}
