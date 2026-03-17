# Performance

## Benchmarks

Measured with BenchmarkDotNet on .NET 8, warm font provider.

### HTML to PDF (default settings)

| Test | Time | Memory |
|------|------|--------|
| Simple HTML | 1.17 ms | 498 KB |
| Styled HTML | 1.27 ms | 748 KB |
| Images | 1.42 ms | 912 KB |
| 50 Lines | 1.56 ms | 1.3 MB |
| Table (50 rows) | 7.86 ms | 4.7 MB |

### Raw PDF Generation (no HTML/CSS parsing)

| Test | Time | Memory |
|------|------|--------|
| Simple | 10.6 µs | 74 KB |
| 50 Lines | 680 µs | 354 KB |
| Table | 558 µs | 331 KB |

### Pipeline Stage Breakdown (Simple HTML)

| Stage | Time | Memory |
|-------|------|--------|
| HTML parsing | 4 µs | 12 KB |
| CSS cascade + style resolution | 72 µs | 108 KB |
| Layout (block, inline, flex, grid, table) | 22 µs | 13 KB |
| Paint + PDF output | 1,069 µs | 355 KB |
| **Total** | **1,167 µs** | **498 KB** |

Parse, style, and layout together account for less than 10% of rendering time. The bottleneck is font subsetting and PDF serialization.

## PDF Options

### Font Embed Mode

Controls how fonts are embedded in the PDF output. Choose based on your speed/size tradeoff:

```csharp
using Rend.Pdf;

var options = new RenderOptions
{
    PdfOptions = new PdfDocumentOptions
    {
        FontEmbedMode = FontEmbedMode.None
    }
};

Render.ToPdf(html, output, options);
```

| Mode | Simple HTML Time | File Size | Description |
|------|-----------------|-----------|-------------|
| `Subset` (default) | 1.17 ms | Smallest | Embeds only used glyphs. Correct fonts in output. |
| `Full` | Varies | Largest | Embeds entire font files. Skips subsetting step. |
| `None` | 0.13 ms | Smallest | No embedding. Renders in Helvetica. Maximum speed. |

`FontEmbedMode.None` is useful for high-throughput scenarios like bulk report generation where exact font fidelity is not required. Text renders in Helvetica regardless of CSS `font-family`.

### Compression

```csharp
var options = new RenderOptions
{
    PdfOptions = new PdfDocumentOptions
    {
        Compression = PdfCompression.FlateFast // Default for Rend HTML-to-PDF
    }
};
```

| Mode | Description |
|------|-------------|
| `FlateFast` | Fast Deflate compression. Default for HTML-to-PDF rendering. |
| `Flate` | Optimal Deflate compression. Smaller output, slower encoding. |
| `FlateOptimal` | Explicit alias for best compression ratio. |
| `None` | No compression. Useful for debugging PDF structure. |

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

## Internal Caching

Rend caches expensive operations across render calls automatically:

- **Parsed font data**: TrueType table parsing is cached per font byte array. Cleared when the font provider is collected.
- **Subsetted fonts**: Font subsetting results are cached per font when the same glyph set is used. Cleared when the font provider is collected.
- **System fonts**: The default font provider scans system fonts once and reuses across all renders.

All caches use `ConditionalWeakTable` — entries are automatically garbage-collected when the owning font provider is no longer referenced. No manual cleanup or unbounded growth.
