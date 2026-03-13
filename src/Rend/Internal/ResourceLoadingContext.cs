using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
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

            var stack = new Stack<Node>();
            stack.Push(head);
            while (stack.Count > 0)
            {
                var current = stack.Pop();
                var child = current.FirstChild;
                while (child != null)
                {
                    if (child is Element el)
                    {
                        if (el.TagName == "link")
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
                        else
                        {
                            stack.Push(el);
                        }
                    }
                    child = child.NextSibling;
                }
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

        /// <summary>
        /// Asynchronously load external stylesheets referenced by &lt;link rel="stylesheet"&gt;.
        /// </summary>
        public async Task<List<Stylesheet>> LoadExternalStylesheetsAsync(Document document, CancellationToken cancellationToken = default)
        {
            var stylesheets = new List<Stylesheet>();
            var head = document.Head;
            if (head == null || _resourceLoader == null) return stylesheets;

            var stack = new Stack<Node>();
            stack.Push(head);
            while (stack.Count > 0)
            {
                var current = stack.Pop();
                var child = current.FirstChild;
                while (child != null)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    if (child is Element el)
                    {
                        if (el.TagName == "link")
                        {
                            var rel = el.GetAttribute("rel");
                            var href = el.GetAttribute("href");

                            if (rel != null && rel.Contains("stylesheet") && !string.IsNullOrEmpty(href))
                            {
                                var css = await LoadResourceAsync(href!, cancellationToken).ConfigureAwait(false);
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
                        else
                        {
                            stack.Push(el);
                        }
                    }
                    child = child.NextSibling;
                }
            }

            return stylesheets;
        }

        /// <summary>
        /// Asynchronously load a resource as a string.
        /// </summary>
        public async Task<string?> LoadResourceAsync(string url, CancellationToken cancellationToken = default)
        {
            if (_resourceLoader == null) return null;

            try
            {
                var uri = ResolveUri(url);
                if (uri == null) return null;

                using (var stream = await _resourceLoader.LoadAsync(uri, cancellationToken).ConfigureAwait(false))
                {
                    if (stream == null) return null;
                    using (var reader = new System.IO.StreamReader(stream))
                    {
                        return await reader.ReadToEndAsync().ConfigureAwait(false);
                    }
                }
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Asynchronously load a resource as bytes.
        /// </summary>
        public async Task<byte[]?> LoadResourceBytesAsync(string url, CancellationToken cancellationToken = default)
        {
            if (_resourceLoader == null) return null;

            try
            {
                var uri = ResolveUri(url);
                if (uri == null) return null;

                using (var stream = await _resourceLoader.LoadAsync(uri, cancellationToken).ConfigureAwait(false))
                {
                    if (stream == null) return null;
                    using (var ms = new System.IO.MemoryStream())
                    {
                        await stream.CopyToAsync(ms, 81920, cancellationToken).ConfigureAwait(false);
                        return ms.ToArray();
                    }
                }
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Resolve @import rules in a stylesheet, fetching and inlining imported stylesheets.
        /// Imported rules are inserted at the position of the @import rule (before subsequent rules per CSS spec).
        /// </summary>
        public Stylesheet ResolveImports(Stylesheet stylesheet, Uri? sheetBaseUrl = null)
        {
            var importedUrls = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var resolved = ResolveImportsCore(stylesheet, sheetBaseUrl ?? _baseUrl, importedUrls);
            return resolved;
        }

        /// <summary>
        /// Async version of ResolveImports.
        /// </summary>
        public async Task<Stylesheet> ResolveImportsAsync(Stylesheet stylesheet, Uri? sheetBaseUrl = null, CancellationToken cancellationToken = default)
        {
            var importedUrls = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var resolved = await ResolveImportsCoreAsync(stylesheet, sheetBaseUrl ?? _baseUrl, importedUrls, cancellationToken).ConfigureAwait(false);
            return resolved;
        }

        private Stylesheet ResolveImportsCore(Stylesheet stylesheet, Uri? baseUrl, HashSet<string> importedUrls)
        {
            if (_resourceLoader == null)
            {
                return stylesheet;
            }

            bool hasImports = false;
            for (int i = 0; i < stylesheet.Rules.Count; i++)
            {
                if (stylesheet.Rules[i] is ImportRule)
                {
                    hasImports = true;
                    break;
                }
            }

            if (!hasImports)
            {
                return stylesheet;
            }

            var resolvedRules = new List<CssRule>();
            for (int i = 0; i < stylesheet.Rules.Count; i++)
            {
                var rule = stylesheet.Rules[i];
                if (rule is ImportRule importRule)
                {
                    var importUrl = ResolveImportUrl(importRule.Url, baseUrl);
                    if (importUrl == null || importedUrls.Contains(importUrl))
                    {
                        continue; // skip unresolvable or circular imports
                    }

                    importedUrls.Add(importUrl);

                    var css = LoadResourceWithBase(importUrl, baseUrl);
                    if (css != null)
                    {
                        try
                        {
                            var importedSheet = CssParser.Parse(css);
                            // Determine base URL for the imported sheet (for nested @import)
                            Uri? importedBaseUrl = null;
                            if (Uri.TryCreate(importUrl, UriKind.Absolute, out var importedUri))
                            {
                                importedBaseUrl = new Uri(importedUri, ".");
                            }
                            // Recursively resolve nested imports
                            importedSheet = ResolveImportsCore(importedSheet, importedBaseUrl ?? baseUrl, importedUrls);

                            // Insert imported rules at this position
                            for (int j = 0; j < importedSheet.Rules.Count; j++)
                            {
                                resolvedRules.Add(importedSheet.Rules[j]);
                            }
                        }
                        catch
                        {
                            // Skip malformed imported stylesheets
                        }
                    }
                }
                else
                {
                    resolvedRules.Add(rule);
                }
            }

            return new Stylesheet(resolvedRules);
        }

        private async Task<Stylesheet> ResolveImportsCoreAsync(Stylesheet stylesheet, Uri? baseUrl, HashSet<string> importedUrls, CancellationToken cancellationToken)
        {
            if (_resourceLoader == null)
            {
                return stylesheet;
            }

            bool hasImports = false;
            for (int i = 0; i < stylesheet.Rules.Count; i++)
            {
                if (stylesheet.Rules[i] is ImportRule)
                {
                    hasImports = true;
                    break;
                }
            }

            if (!hasImports)
            {
                return stylesheet;
            }

            var resolvedRules = new List<CssRule>();
            for (int i = 0; i < stylesheet.Rules.Count; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var rule = stylesheet.Rules[i];
                if (rule is ImportRule importRule)
                {
                    var importUrl = ResolveImportUrl(importRule.Url, baseUrl);
                    if (importUrl == null || importedUrls.Contains(importUrl))
                    {
                        continue; // skip unresolvable or circular imports
                    }

                    importedUrls.Add(importUrl);

                    var css = await LoadResourceAsyncWithBase(importUrl, baseUrl, cancellationToken).ConfigureAwait(false);
                    if (css != null)
                    {
                        try
                        {
                            var importedSheet = CssParser.Parse(css);
                            // Determine base URL for the imported sheet (for nested @import)
                            Uri? importedBaseUrl = null;
                            if (Uri.TryCreate(importUrl, UriKind.Absolute, out var importedUri))
                            {
                                importedBaseUrl = new Uri(importedUri, ".");
                            }
                            // Recursively resolve nested imports
                            importedSheet = await ResolveImportsCoreAsync(importedSheet, importedBaseUrl ?? baseUrl, importedUrls, cancellationToken).ConfigureAwait(false);

                            // Insert imported rules at this position
                            for (int j = 0; j < importedSheet.Rules.Count; j++)
                            {
                                resolvedRules.Add(importedSheet.Rules[j]);
                            }
                        }
                        catch (OperationCanceledException)
                        {
                            throw;
                        }
                        catch
                        {
                            // Skip malformed imported stylesheets
                        }
                    }
                }
                else
                {
                    resolvedRules.Add(rule);
                }
            }

            return new Stylesheet(resolvedRules);
        }

        /// <summary>
        /// Resolve an @import URL against a base URL, returning the absolute URL string.
        /// </summary>
        private static string? ResolveImportUrl(string url, Uri? baseUrl)
        {
            if (Uri.TryCreate(url, UriKind.Absolute, out var absoluteUri))
            {
                return absoluteUri.AbsoluteUri;
            }

            if (baseUrl != null && Uri.TryCreate(baseUrl, url, out var resolvedUri))
            {
                return resolvedUri.AbsoluteUri;
            }

            return null;
        }

        /// <summary>
        /// Load a resource using a specific base URL for resolution.
        /// </summary>
        private string? LoadResourceWithBase(string absoluteUrl, Uri? baseUrl)
        {
            if (_resourceLoader == null)
            {
                return null;
            }

            try
            {
                if (!Uri.TryCreate(absoluteUrl, UriKind.Absolute, out var uri))
                {
                    return null;
                }

                using (var stream = _resourceLoader.LoadAsync(uri).GetAwaiter().GetResult())
                {
                    if (stream == null)
                    {
                        return null;
                    }
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
        /// Async version of LoadResourceWithBase.
        /// </summary>
        private async Task<string?> LoadResourceAsyncWithBase(string absoluteUrl, Uri? baseUrl, CancellationToken cancellationToken)
        {
            if (_resourceLoader == null)
            {
                return null;
            }

            try
            {
                if (!Uri.TryCreate(absoluteUrl, UriKind.Absolute, out var uri))
                {
                    return null;
                }

                using (var stream = await _resourceLoader.LoadAsync(uri, cancellationToken).ConfigureAwait(false))
                {
                    if (stream == null)
                    {
                        return null;
                    }
                    using (var reader = new System.IO.StreamReader(stream))
                    {
                        return await reader.ReadToEndAsync().ConfigureAwait(false);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                throw;
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
