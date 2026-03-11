using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Rend.Css.Media.Internal;
using Rend.Html;
using Rend.Rendering;

namespace Rend.Internal
{
    /// <summary>
    /// Resolves &lt;img src&gt; attributes to <see cref="ImageData"/> instances.
    /// </summary>
    internal sealed class InlineImageResolver
    {
        private readonly Dictionary<string, ImageData?> _cache = new Dictionary<string, ImageData?>();
        private readonly IImageResolver? _imageResolver;
        private readonly Func<string, byte[]?>? _resourceLoader;
        private readonly Uri? _baseUrl;
        private readonly MediaContext? _mediaContext;

        public InlineImageResolver(Uri? baseUrl = null, IImageResolver? imageResolver = null,
            Func<string, byte[]?>? resourceLoader = null, MediaContext? mediaContext = null)
        {
            _baseUrl = baseUrl;
            _imageResolver = imageResolver;
            _resourceLoader = resourceLoader;
            _mediaContext = mediaContext;
        }

        /// <summary>
        /// Load an image on-demand by URL (for CSS url() images not pre-resolved from the DOM).
        /// </summary>
        public ImageData? LoadOnDemand(string src)
        {
            return LoadImage(src);
        }

        /// <summary>
        /// Walk the DOM and resolve all img src attributes.
        /// </summary>
        public Dictionary<string, ImageData> Resolve(Document document)
        {
            var images = new Dictionary<string, ImageData>();
            CollectImages(document, images);
            return images;
        }

        private void CollectImages(Node node, Dictionary<string, ImageData> images)
        {
            if (node is Element el && el.TagName == "img")
            {
                string? src = ResolveImageSource(el);
                if (!string.IsNullOrEmpty(src) && !images.ContainsKey(src!))
                {
                    var imageData = LoadImage(src!);
                    if (imageData != null)
                        images[src!] = imageData;
                }
            }

            var child = node.FirstChild;
            while (child != null)
            {
                CollectImages(child, images);
                child = child.NextSibling;
            }
        }

        /// <summary>
        /// Resolves the best image source for an &lt;img&gt; element, considering
        /// parent &lt;picture&gt; element's &lt;source&gt; children and srcset attribute.
        /// </summary>
        private string? ResolveImageSource(Element img)
        {
            // Check if inside a <picture> element
            if (img.Parent is Element parent && parent.TagName == "picture")
            {
                // Walk <source> siblings before the <img> — first match wins
                var sibling = parent.FirstChild;
                while (sibling != null)
                {
                    if (sibling == img) break; // Stop at the <img> itself

                    if (sibling is Element source && source.TagName == "source")
                    {
                        // Check media attribute — skip if media query doesn't match
                        string? media = source.GetAttribute("media");
                        if (media != null && _mediaContext != null &&
                            !MediaQueryEvaluator.Evaluate(media, _mediaContext))
                        {
                            sibling = sibling.NextSibling;
                            continue;
                        }

                        // Check type attribute — skip unsupported formats
                        string? type = source.GetAttribute("type");
                        if (type != null && !IsSupportedImageType(type))
                        {
                            sibling = sibling.NextSibling;
                            continue;
                        }

                        // Check srcset on <source> (use first entry)
                        string? srcset = source.GetAttribute("srcset");
                        if (srcset != null)
                        {
                            string? resolved = ParseFirstSrcsetEntry(srcset);
                            if (resolved != null) return resolved;
                        }

                        // Check src on <source>
                        string? src = source.GetAttribute("src");
                        if (!string.IsNullOrEmpty(src))
                            return src;
                    }

                    sibling = sibling.NextSibling;
                }
            }

            // Check srcset on the <img> itself (use first entry or 1x descriptor)
            string? imgSrcset = img.GetAttribute("srcset");
            if (imgSrcset != null)
            {
                string? resolved = ParseFirstSrcsetEntry(imgSrcset);
                if (resolved != null) return resolved;
            }

            // Fallback to src attribute
            return img.GetAttribute("src");
        }

        /// <summary>
        /// Parses a srcset attribute and returns the URL of the first entry (or the 1x entry if present).
        /// srcset format: "url1 1x, url2 2x, url3 300w"
        /// </summary>
        private static string? ParseFirstSrcsetEntry(string srcset)
        {
            string? firstUrl = null;
            string? oneXUrl = null;

            var entries = srcset.Split(',');
            for (int i = 0; i < entries.Length; i++)
            {
                string entry = entries[i].Trim();
                if (entry.Length == 0) continue;

                // Split into URL and descriptor
                int spaceIdx = entry.LastIndexOf(' ');
                string url;
                string descriptor;
                if (spaceIdx > 0)
                {
                    url = entry.Substring(0, spaceIdx).Trim();
                    descriptor = entry.Substring(spaceIdx + 1).Trim();
                }
                else
                {
                    url = entry;
                    descriptor = "1x";
                }

                if (firstUrl == null) firstUrl = url;
                if (descriptor == "1x") oneXUrl = url;
            }

            return oneXUrl ?? firstUrl;
        }

        /// <summary>
        /// Returns true if the given MIME type represents a supported image format.
        /// </summary>
        private static bool IsSupportedImageType(string type)
        {
            return type == "image/png" || type == "image/jpeg" || type == "image/jpg" ||
                   type == "image/gif" || type == "image/webp" || type == "image/svg+xml";
        }

        /// <summary>
        /// Asynchronously walk the DOM and resolve all img src attributes.
        /// </summary>
        public async Task<Dictionary<string, ImageData>> ResolveAsync(Document document, CancellationToken cancellationToken = default)
        {
            var images = new Dictionary<string, ImageData>();
            await CollectImagesAsync(document, images, cancellationToken).ConfigureAwait(false);
            return images;
        }

        private async Task CollectImagesAsync(Node node, Dictionary<string, ImageData> images, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (node is Element el && el.TagName == "img")
            {
                string? src = ResolveImageSource(el);
                if (!string.IsNullOrEmpty(src) && !images.ContainsKey(src!))
                {
                    var imageData = await LoadImageAsync(src!, cancellationToken).ConfigureAwait(false);
                    if (imageData != null)
                        images[src!] = imageData;
                }
            }

            var child = node.FirstChild;
            while (child != null)
            {
                await CollectImagesAsync(child, images, cancellationToken).ConfigureAwait(false);
                child = child.NextSibling;
            }
        }

        /// <summary>
        /// Asynchronously load an image on-demand by URL.
        /// </summary>
        public async Task<ImageData?> LoadImageAsync(string src, CancellationToken cancellationToken = default)
        {
            if (_cache.TryGetValue(src, out var cached))
                return cached;

            ImageData? result = null;

            if (src.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
            {
                result = DecodeDataUri(src);
            }
            else
            {
                byte[]? data = await LoadFromResolverAsync(src, cancellationToken).ConfigureAwait(false)
                    ?? LoadFromResourceLoader(src);
                if (data != null && data.Length > 0)
                {
                    string format = DetectFormat(src, data);
                    result = CreateImageData(data, format);
                }
            }

            _cache[src] = result;
            return result;
        }

        private async Task<byte[]?> LoadFromResolverAsync(string src, CancellationToken cancellationToken)
        {
            if (_imageResolver == null) return null;

            using var stream = await _imageResolver.ResolveAsync(src, cancellationToken).ConfigureAwait(false);
            if (stream == null) return null;

            using var ms = new MemoryStream();
            await stream.CopyToAsync(ms, 81920, cancellationToken).ConfigureAwait(false);
            var bytes = ms.ToArray();
            return bytes.Length > 0 ? bytes : null;
        }

        private ImageData? LoadImage(string src)
        {
            if (_cache.TryGetValue(src, out var cached))
                return cached;

            ImageData? result = null;

            if (src.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
            {
                result = DecodeDataUri(src);
            }
            else
            {
                byte[]? data = LoadFromResolver(src) ?? LoadFromResourceLoader(src);
                if (data != null && data.Length > 0)
                {
                    string format = DetectFormat(src, data);
                    result = CreateImageData(data, format);
                }
            }

            _cache[src] = result;
            return result;
        }

        private byte[]? LoadFromResolver(string src)
        {
            if (_imageResolver == null) return null;

            using var stream = _imageResolver.ResolveAsync(src).GetAwaiter().GetResult();
            if (stream == null) return null;

            using var ms = new MemoryStream();
            stream.CopyTo(ms);
            var bytes = ms.ToArray();
            return bytes.Length > 0 ? bytes : null;
        }

        private byte[]? LoadFromResourceLoader(string src)
        {
            if (_resourceLoader == null) return null;
            return _resourceLoader(src);
        }

        private static ImageData? DecodeDataUri(string dataUri)
        {
            int commaIndex = dataUri.IndexOf(',');
            if (commaIndex < 0) return null;

            string header = dataUri.Substring(0, commaIndex);
            string data = dataUri.Substring(commaIndex + 1);

            if (!header.Contains(";base64")) return null;

            try
            {
                var bytes = Convert.FromBase64String(data);
                string format = "png";
                if (header.Contains("image/jpeg") || header.Contains("image/jpg"))
                    format = "jpeg";
                else if (header.Contains("image/gif"))
                    format = "gif";
                else if (header.Contains("image/webp"))
                    format = "webp";

                return CreateImageData(bytes, format);
            }
            catch
            {
                return null;
            }
        }

        private static ImageData CreateImageData(byte[] data, string format)
        {
            DetectDimensions(data, out int w, out int h);
            return new ImageData(data, w, h, format);
        }

        private static void DetectDimensions(byte[] data, out int width, out int height)
        {
            width = 0;
            height = 0;

            if (data.Length < 8) return;

            // PNG: IHDR chunk starts at offset 8, width at 16, height at 20 (big-endian)
            if (data[0] == 0x89 && data[1] == 0x50 && data[2] == 0x4E && data[3] == 0x47)
            {
                if (data.Length >= 24)
                {
                    width = (data[16] << 24) | (data[17] << 16) | (data[18] << 8) | data[19];
                    height = (data[20] << 24) | (data[21] << 16) | (data[22] << 8) | data[23];
                }
                return;
            }

            // JPEG: scan for SOF0/SOF2 marker (0xFF 0xC0 or 0xFF 0xC2)
            if (data[0] == 0xFF && data[1] == 0xD8)
            {
                int offset = 2;
                while (offset + 4 < data.Length)
                {
                    if (data[offset] != 0xFF) break;
                    byte marker = data[offset + 1];
                    if (marker == 0xC0 || marker == 0xC2)
                    {
                        if (offset + 9 < data.Length)
                        {
                            height = (data[offset + 5] << 8) | data[offset + 6];
                            width = (data[offset + 7] << 8) | data[offset + 8];
                        }
                        return;
                    }
                    // Skip to next marker
                    if (offset + 3 < data.Length)
                    {
                        int segLen = (data[offset + 2] << 8) | data[offset + 3];
                        offset += 2 + segLen;
                    }
                    else break;
                }
                return;
            }

            // GIF: width at offset 6, height at offset 8 (little-endian)
            if (data[0] == 0x47 && data[1] == 0x49 && data[2] == 0x46 && data.Length >= 10)
            {
                width = data[6] | (data[7] << 8);
                height = data[8] | (data[9] << 8);
                return;
            }
        }

        private static string DetectFormat(string src, byte[] data)
        {
            // Detect from magic bytes
            if (data.Length >= 8)
            {
                if (data[0] == 0x89 && data[1] == 0x50 && data[2] == 0x4E && data[3] == 0x47)
                    return "png";
                if (data[0] == 0xFF && data[1] == 0xD8 && data[2] == 0xFF)
                    return "jpeg";
                if (data[0] == 0x47 && data[1] == 0x49 && data[2] == 0x46)
                    return "gif";
                if (data[0] == 0x52 && data[1] == 0x49 && data[2] == 0x46 && data[3] == 0x46 &&
                    data[8] == 0x57 && data[9] == 0x45 && data[10] == 0x42 && data[11] == 0x50)
                    return "webp";
            }

            // Fallback: detect from extension
            if (src.EndsWith(".png", StringComparison.OrdinalIgnoreCase)) return "png";
            if (src.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
                src.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase)) return "jpeg";
            if (src.EndsWith(".gif", StringComparison.OrdinalIgnoreCase)) return "gif";
            if (src.EndsWith(".webp", StringComparison.OrdinalIgnoreCase)) return "webp";

            return "png";
        }
    }
}
