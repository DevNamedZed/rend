# Custom Image Resolver

The `IImageResolver` interface gives you full control over how images are loaded during rendering. It handles all image sources: `<img>` tags, CSS `background-image`, `border-image`, and `list-style-image`.

## Interface

```csharp
public interface IImageResolver
{
    Stream? Resolve(string url);
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

    public Stream? Resolve(string url)
    {
        // Extract image ID from URL
        if (!url.StartsWith("/images/")) return null;
        string id = url.Substring(8);

        byte[]? data = _db.QuerySingleOrDefault<byte[]>(
            "SELECT data FROM images WHERE id = @id", new { id });

        return data != null ? new MemoryStream(data) : null;
    }
}

var options = new RenderOptions
{
    ImageResolver = new DatabaseImageResolver(connection),
};

Render.ToPdf(html, output, options);
```

## Priority

When both `ImageResolver` and `ResourceLoader` are set:

1. `IImageResolver.Resolve()` is called first
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

    public Stream? Resolve(string url)
    {
        string key = new Uri(url).AbsolutePath.TrimStart('/');
        var response = _s3.GetObject(_bucket, key);
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

    public Stream? Resolve(string url)
    {
        var data = _cache.GetOrAdd(url, u =>
        {
            using var stream = _inner.Resolve(u);
            if (stream == null) return null;
            using var ms = new MemoryStream();
            stream.CopyTo(ms);
            return ms.ToArray();
        });

        return data != null ? new MemoryStream(data) : null;
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

    public Stream? Resolve(string url)
    {
        string resourceName = _prefix + "." + Path.GetFileName(url);
        return _assembly.GetManifestResourceStream(resourceName);
    }
}
```
