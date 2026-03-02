using System;
using System.Collections.Generic;
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
        private readonly Func<string, byte[]?>? _resourceLoader;
        private readonly Uri? _baseUrl;

        public InlineImageResolver(Uri? baseUrl = null, Func<string, byte[]?>? resourceLoader = null)
        {
            _baseUrl = baseUrl;
            _resourceLoader = resourceLoader;
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
        private static string? ResolveImageSource(Element img)
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

        private ImageData? LoadImage(string src)
        {
            if (_cache.TryGetValue(src, out var cached))
                return cached;

            ImageData? result = null;

            if (src.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
            {
                result = DecodeDataUri(src);
            }
            else if (_resourceLoader != null)
            {
                var data = _resourceLoader(src);
                if (data != null && data.Length > 0)
                {
                    string format = DetectFormat(src, data);
                    result = new ImageData(data, 0, 0, format);
                }
            }

            _cache[src] = result;
            return result;
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

                return new ImageData(bytes, 0, 0, format);
            }
            catch
            {
                return null;
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
