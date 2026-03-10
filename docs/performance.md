# Performance

## Shared Resources

The most expensive part of rendering is font loading and native memory allocation. For batch rendering, share these across calls:

```csharp
using var shaper = new HarfBuzzTextShaper();
using var fontMapper = new SkiaFontMapper();

var options = new RenderOptions
{
    TextShaper = shaper,     // Reuses HarfBuzz font data
    FontMapper = fontMapper, // Reuses Skia typeface instances
};

// All renders share the same native font data
foreach (var html in documents)
{
    byte[] pdf = Render.ToPdf(html, options);
    // ...
}
```

Without sharing, each render call creates and destroys its own HarfBuzz fonts and Skia typefaces. For a batch of 100 documents, this can reduce total time by 40-60%.

## Async Rendering

The async methods run the CPU-bound rendering pipeline on a thread pool thread:

```csharp
// Non-blocking — frees up the calling thread
await Render.ToPdfAsync(html, outputStream, options, cancellationToken);
```

For server applications handling concurrent requests, this prevents blocking the request thread during rendering.

## Stream Output

Use the `Stream` overloads to avoid holding the entire output in memory:

```csharp
// Writes directly to the response stream
Render.ToPdf(html, Response.Body, options);
```

## Font Loading

- `SystemFontResolver` scans font directories once per instance. Create it once and reuse.
- `DirectoryFontResolver` loads all fonts from a directory. For large font directories, only include what you need.
- `@font-face` fonts are loaded via `ResourceLoader` on demand.

## Image Loading

- Images are loaded during the rendering pipeline and cached per render call.
- `IImageResolver` is called once per unique URL — results are cached internally.
- Data URIs are decoded in-place without network calls.
- For large images, Rend decodes them to determine dimensions but passes raw bytes to the output target.
