using System;
using System.Collections.Generic;
using Rend.Core;
using Rend.Css;
using Rend.Html;

namespace Rend.Internal
{
    /// <summary>
    /// Coordinates resource loading during rendering: external stylesheets, images, fonts.
    /// </summary>
    internal sealed class ResourceLoadingContext
    {
        private readonly Uri? _baseUrl;
        private readonly IResourceLoader? _resourceLoader;

        public ResourceLoadingContext(Uri? baseUrl, IResourceLoader? resourceLoader)
        {
            _baseUrl = baseUrl;
            _resourceLoader = resourceLoader;
        }

        /// <summary>
        /// Load external stylesheets referenced by &lt;link rel="stylesheet"&gt;.
        /// </summary>
        public List<Stylesheet> LoadExternalStylesheets(Document document)
        {
            var stylesheets = new List<Stylesheet>();
            var head = document.Head;
            if (head == null || _resourceLoader == null) return stylesheets;

            var child = head.FirstChild;
            while (child != null)
            {
                if (child is Element el && el.TagName == "link")
                {
                    var rel = el.GetAttribute("rel");
                    var href = el.GetAttribute("href");

                    if (rel != null && rel.Contains("stylesheet") && !string.IsNullOrEmpty(href))
                    {
                        var css = LoadResource(href!);
                        if (css != null)
                        {
                            try
                            {
                                var sheet = CssParser.Parse(css);
                                stylesheets.Add(sheet);
                            }
                            catch
                            {
                                // Skip malformed external stylesheets
                            }
                        }
                    }
                }
                child = child.NextSibling;
            }

            return stylesheets;
        }

        /// <summary>
        /// Load a resource as a string.
        /// </summary>
        public string? LoadResource(string url)
        {
            if (_resourceLoader == null) return null;

            try
            {
                var uri = ResolveUri(url);
                if (uri == null) return null;

                using (var stream = _resourceLoader.LoadAsync(uri).GetAwaiter().GetResult())
                {
                    if (stream == null) return null;
                    using (var reader = new System.IO.StreamReader(stream))
                    {
                        return reader.ReadToEnd();
                    }
                }
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Load a resource as bytes.
        /// </summary>
        public byte[]? LoadResourceBytes(string url)
        {
            if (_resourceLoader == null) return null;

            try
            {
                var uri = ResolveUri(url);
                if (uri == null) return null;

                using (var stream = _resourceLoader.LoadAsync(uri).GetAwaiter().GetResult())
                {
                    if (stream == null) return null;
                    using (var ms = new System.IO.MemoryStream())
                    {
                        stream.CopyTo(ms);
                        return ms.ToArray();
                    }
                }
            }
            catch
            {
                return null;
            }
        }

        private Uri? ResolveUri(string url)
        {
            if (Uri.TryCreate(url, UriKind.Absolute, out var absoluteUri))
                return absoluteUri;

            if (_baseUrl != null && Uri.TryCreate(_baseUrl, url, out var resolvedUri))
                return resolvedUri;

            // Try as file path
            if (Uri.TryCreate("file:///" + url, UriKind.Absolute, out var fileUri))
                return fileUri;

            return null;
        }
    }
}
