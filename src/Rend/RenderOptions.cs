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

        /// <summary>Top page margin in points. Default: 72pt (1 inch).</summary>
        private float _marginTop = 72f;
        /// <summary>Top page margin in points. Default: 72pt (1 inch).</summary>
        public float MarginTop
        {
            get => _marginTop;
            set
            {
                if (value < 0) throw new ArgumentOutOfRangeException(nameof(value), "MarginTop must be non-negative.");
                _marginTop = value;
            }
        }

        private float _marginRight = 72f;
        /// <summary>Right page margin in points. Default: 72pt (1 inch).</summary>
        public float MarginRight
        {
            get => _marginRight;
            set
            {
                if (value < 0) throw new ArgumentOutOfRangeException(nameof(value), "MarginRight must be non-negative.");
                _marginRight = value;
            }
        }

        private float _marginBottom = 72f;
        /// <summary>Bottom page margin in points. Default: 72pt (1 inch).</summary>
        public float MarginBottom
        {
            get => _marginBottom;
            set
            {
                if (value < 0) throw new ArgumentOutOfRangeException(nameof(value), "MarginBottom must be non-negative.");
                _marginBottom = value;
            }
        }

        private float _marginLeft = 72f;
        /// <summary>Left page margin in points. Default: 72pt (1 inch).</summary>
        public float MarginLeft
        {
            get => _marginLeft;
            set
            {
                if (value < 0) throw new ArgumentOutOfRangeException(nameof(value), "MarginLeft must be non-negative.");
                _marginLeft = value;
            }
        }

        /// <summary>DPI for image output. Default: 96.</summary>
        private float _dpi = 96f;
        /// <summary>DPI for image output. Default: 96.</summary>
        public float Dpi
        {
            get => _dpi;
            set
            {
                if (value <= 0) throw new ArgumentOutOfRangeException(nameof(value), "DPI must be positive.");
                _dpi = value;
            }
        }

        /// <summary>Base URL for resolving relative resource URLs.</summary>
        public Uri? BaseUrl { get; set; }

        /// <summary>Resource loader for external resources (CSS, images, fonts).</summary>
        public IResourceLoader? ResourceLoader { get; set; }

        /// <summary>
        /// Custom image resolver. Called for every image URL encountered in HTML
        /// (<c>&lt;img src&gt;</c>) or CSS (<c>background-image</c>, <c>border-image</c>,
        /// <c>list-style-image</c>). Return a stream of image bytes, or null to skip.
        /// When set, takes priority over <see cref="ResourceLoader"/> for image loading.
        /// </summary>
        public IImageResolver? ImageResolver { get; set; }

        /// <summary>Font provider. If null, a default system font provider is created.</summary>
        public IFontProvider? FontProvider { get; set; }

        /// <summary>Whether to generate PDF bookmarks from h1-h6 headings. Default: true.</summary>
        public bool GenerateBookmarks { get; set; } = true;

        /// <summary>Whether to generate PDF link annotations from &lt;a&gt; elements. Default: true.</summary>
        public bool GenerateLinks { get; set; } = true;

        /// <summary>Image output format. Default: <see cref="ImageOutputFormat.Png"/>.</summary>
        public ImageOutputFormat ImageFormat { get; set; } = ImageOutputFormat.Png;

        /// <summary>JPEG/WebP quality (1-100). Default: 90.</summary>
        private int _imageQuality = 90;
        /// <summary>JPEG/WebP quality (1-100). Default: 90.</summary>
        public int ImageQuality
        {
            get => _imageQuality;
            set
            {
                if (value < 1 || value > 100) throw new ArgumentOutOfRangeException(nameof(value), "ImageQuality must be between 1 and 100.");
                _imageQuality = value;
            }
        }

        /// <summary>PDF document title metadata.</summary>
        public string? Title { get; set; }

        /// <summary>PDF document author metadata.</summary>
        public string? Author { get; set; }

        /// <summary>Default font size in CSS pixels. Default: 16.</summary>
        private float _defaultFontSize = 16f;
        /// <summary>Default font size in CSS pixels. Default: 16.</summary>
        public float DefaultFontSize
        {
            get => _defaultFontSize;
            set
            {
                if (value <= 0) throw new ArgumentOutOfRangeException(nameof(value), "DefaultFontSize must be positive.");
                _defaultFontSize = value;
            }
        }

        /// <summary>
        /// HTML content for page headers. Rendered in the top margin area of each page.
        /// Supports template variables:
        /// <list type="bullet">
        /// <item><c>{pageNumber}</c> — current page number (1-based).</item>
        /// <item><c>{totalPages}</c> — total number of pages in the document.</item>
        /// <item><c>{date}</c> — current date in yyyy-MM-dd format.</item>
        /// </list>
        /// </summary>
        public string? HeaderHtml { get; set; }

        /// <summary>
        /// HTML content for page footers. Rendered in the bottom margin area of each page.
        /// Supports the same template variables as <see cref="HeaderHtml"/>.
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

        /// <summary>
        /// Optional shared text shaper. When set, the pipeline reuses this shaper
        /// instead of creating a new one per render call. This avoids repeated
        /// font data pinning and native memory allocation. The caller owns disposal.
        /// </summary>
        public Text.ITextShaper? TextShaper { get; set; }

        /// <summary>
        /// Optional shared Skia font mapper for image output. When set, SKTypeface
        /// instances are cached and reused across renders, avoiding repeated native
        /// memory copies of font data. The caller owns disposal.
        /// </summary>
        public Output.Image.SkiaFontMapper? FontMapper { get; set; }

        /// <summary>
        /// When true, the render result includes a layout tree snapshot for diagnostic
        /// comparison with browser layout. Default: false.
        /// </summary>
        public bool CaptureLayoutTree { get; set; }

        /// <summary>Default options.</summary>
        public static RenderOptions Default => new RenderOptions();
    }
}
