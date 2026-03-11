# Custom Image Resolver

The `IImageResolver` interface gives you full control over how images are loaded during rendering. It handles all image sources: `<img>` tags, CSS `background-image`, `border-image`, and `list-style-image`.

## Interface

```csharp
public interface IImageResolver
{
    Task<Stream?> ResolveAsync(string url, CancellationToken cancellationToken = default);
}
```

- **`url`**: The image URL as it appears in the HTML/CSS (may be relative or absolute)
- **Returns**: A readable `Stream` containing the image bytes, or `null` to skip the image
- The caller disposes the returned stream

## Basic Usage

```csharp
public class DatabaseImageResolver : IImageResolver
{
    private readonly IDbConnection _db;

    public DatabaseImageResolver(IDbConnection db) => _db = db;

    public async Task<Stream?> ResolveAsync(string url, CancellationToken cancellationToken = default)
    {
        // Extract image ID from URL
        if (!url.StartsWith("/images/")) return null;
        string id = url.Substring(8);

        byte[]? data = await _db.QuerySingleOrDefaultAsync<byte[]>(
            "SELECT data FROM images WHERE id = @id", new { id });

        return data != null ? new MemoryStream(data) : null;
    }
}

var options = new RenderOptions
{
    ImageResolver = new DatabaseImageResolver(connection),
};

await Render.ToPdfAsync(html, output, options);
```

## Priority

When both `ImageResolver` and `ResourceLoader` are set:

1. `IImageResolver.ResolveAsync()` is called first
2. If it returns `null`, `IResourceLoader.LoadAsync()` is used as fallback
3. Data URIs (`data:image/...;base64,...`) are always decoded directly, bypassing both

## Supported Formats

The resolver can return any of these image formats. Rend detects the format automatically from magic bytes:

| Format | Magic Bytes |
|--------|------------|
| PNG | `89 50 4E 47` |
| JPEG | `FF D8 FF` |
| GIF | `47 49 46` |
| WebP | `52 49 46 46 ... 57 45 42 50` |

If magic bytes don't match, Rend falls back to detecting from the URL file extension.

## Examples

### S3 / Cloud Storage

```csharp
public class S3ImageResolver : IImageResolver
{
    private readonly IAmazonS3 _s3;
    private readonly string _bucket;

    public async Task<Stream?> ResolveAsync(string url, CancellationToken cancellationToken = default)
    {
        string key = new Uri(url).AbsolutePath.TrimStart('/');
        var response = await _s3.GetObjectAsync(_bucket, key, cancellationToken);
        return response.ResponseStream;
    }
}
```

### In-Memory Cache

```csharp
public class CachedImageResolver : IImageResolver
{
    private readonly IImageResolver _inner;
    private readonly ConcurrentDictionary<string, byte[]?> _cache = new();

    public CachedImageResolver(IImageResolver inner) => _inner = inner;

    public async Task<Stream?> ResolveAsync(string url, CancellationToken cancellationToken = default)
    {
        if (_cache.TryGetValue(url, out var cached))
            return cached != null ? new MemoryStream(cached) : null;

        using var stream = await _inner.ResolveAsync(url, cancellationToken);
        if (stream == null)
        {
            _cache[url] = null;
            return null;
        }

        using var ms = new MemoryStream();
        await stream.CopyToAsync(ms, 81920, cancellationToken);
        var data = ms.ToArray();
        _cache[url] = data;
        return new MemoryStream(data);
    }
}
```

### Embedded Resources

```csharp
public class EmbeddedImageResolver : IImageResolver
{
    private readonly Assembly _assembly;
    private readonly string _prefix;

    public EmbeddedImageResolver(Assembly assembly, string resourcePrefix)
    {
        _assembly = assembly;
        _prefix = resourcePrefix;
    }

    public Task<Stream?> ResolveAsync(string url, CancellationToken cancellationToken = default)
    {
        string resourceName = _prefix + "." + Path.GetFileName(url);
        Stream? stream = _assembly.GetManifestResourceStream(resourceName);
        return Task.FromResult(stream);
    }
}
```
