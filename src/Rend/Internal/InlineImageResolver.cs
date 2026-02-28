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
                var src = el.GetAttribute("src");
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
