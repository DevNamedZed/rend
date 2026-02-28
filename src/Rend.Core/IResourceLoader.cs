using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Rend.Core
{
    /// <summary>
    /// Loads external resources (stylesheets, images, fonts) by URI.
    /// </summary>
    public interface IResourceLoader
    {
        Task<Stream> LoadAsync(Uri uri, CancellationToken ct = default);
    }

    /// <summary>
    /// Loads resources from the local filesystem. Supports file:// URIs and absolute paths.
    /// </summary>
    public sealed class FileSystemResourceLoader : IResourceLoader
    {
        private readonly string? _basePath;

        public FileSystemResourceLoader(string? basePath = null)
        {
            _basePath = basePath;
        }

        public Task<Stream> LoadAsync(Uri uri, CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();

            string path;
            if (uri.IsAbsoluteUri && uri.Scheme == "file")
            {
                path = uri.LocalPath;
            }
            else
            {
                string relativePath = uri.IsAbsoluteUri ? uri.AbsolutePath : uri.OriginalString;
                path = _basePath != null
                    ? Path.Combine(_basePath, relativePath)
                    : Path.GetFullPath(relativePath);
            }

            Stream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync: true);
            return Task.FromResult(stream);
        }
    }

    /// <summary>
    /// Chains multiple resource loaders. Tries each in order until one succeeds.
    /// </summary>
    public sealed class CompositeResourceLoader : IResourceLoader
    {
        private readonly IResourceLoader[] _loaders;

        public CompositeResourceLoader(params IResourceLoader[] loaders)
        {
            _loaders = loaders ?? throw new ArgumentNullException(nameof(loaders));
        }

        public async Task<Stream> LoadAsync(Uri uri, CancellationToken ct = default)
        {
            Exception? lastException = null;
            foreach (var loader in _loaders)
            {
                try
                {
                    return await loader.LoadAsync(uri, ct).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    lastException = ex;
                }
            }
            throw new FileNotFoundException(
                $"No resource loader could load '{uri}'.", lastException?.ToString());
        }
    }
}
