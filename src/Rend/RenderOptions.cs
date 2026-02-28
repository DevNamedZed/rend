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

        /// <summary>Default options.</summary>
        public static readonly RenderOptions Default = new RenderOptions();
    }
}
