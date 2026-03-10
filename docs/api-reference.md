# API Reference

## Rendering

### `Render`

Static entry point for converting HTML to PDF or images.

```csharp
Render.ToPdf(html, output);
Render.ToPdf(html, output, options);
Render.ToImage(html, output, options);
await Render.ToPdfAsync(html, output, ct);
```

All methods are also available on `HtmlRenderer`, which implements `IRenderer` for dependency injection and testing.

### `RenderOptions`

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `PageSize` | `SizeF` | A4 (595.28 x 841.89 pt) | Page dimensions in points |
| `MarginTop` | `float` | 72 | Top margin in points |
| `MarginRight` | `float` | 72 | Right margin in points |
| `MarginBottom` | `float` | 72 | Bottom margin in points |
| `MarginLeft` | `float` | 72 | Left margin in points |
| `Dpi` | `float` | 96 | DPI for image output |
| `BaseUrl` | `Uri?` | null | Base URL for resolving relative resource URLs |
| `ResourceLoader` | `IResourceLoader?` | null | Loader for external resources (CSS, images, fonts) |
| `ImageResolver` | `IImageResolver?` | null | Custom image resolver (takes priority over ResourceLoader for images) |
| `FontProvider` | `IFontProvider?` | null | Font provider. If null, system fonts are used |
| `GenerateBookmarks` | `bool` | true | Generate PDF bookmarks from h1-h6 headings |
| `GenerateLinks` | `bool` | true | Generate PDF link annotations from `<a>` elements |
| `ImageFormat` | `string` | "png" | Image output format: "png", "jpeg", "webp" |
| `ImageQuality` | `int` | 90 | JPEG/WebP quality (1-100) |
| `Title` | `string?` | null | PDF document title metadata |
| `Author` | `string?` | null | PDF document author metadata |
| `DefaultFontSize` | `float` | 16 | Default font size in CSS pixels |
| `HeaderHtml` | `string?` | null | HTML for page headers. Variables: `{pageNumber}`, `{totalPages}`, `{date}` |
| `FooterHtml` | `string?` | null | HTML for page footers. Same variables as HeaderHtml |
| `MediaType` | `string?` | null | CSS media type: "screen" or "print". null = auto (screen for images, print for PDF) |
| `PrefersColorSchemeDark` | `bool` | false | Enables `prefers-color-scheme: dark` media query |
| `Progress` | `IProgress<RenderProgress>?` | null | Progress reporter |
| `TextShaper` | `ITextShaper?` | null | Shared text shaper for reuse across renders. Caller owns disposal |
| `FontMapper` | `SkiaFontMapper?` | null | Shared Skia font mapper for image output. Caller owns disposal |

### `IImageResolver`

```csharp
public interface IImageResolver
{
    Stream? Resolve(string url);
}
```

Called for every image URL in HTML (`<img>`) or CSS (`background-image`, `border-image`, `list-style-image`). Return null to fall back to the default resource loader.

### `IResourceLoader`

```csharp
public interface IResourceLoader
{
    Task<Stream> LoadAsync(Uri uri, CancellationToken ct = default);
}
```

General-purpose resource loader for CSS, images, and fonts. Built-in implementation: `FileSystemResourceLoader`.

### `RenderProgress`

| Property | Type | Description |
|----------|------|-------------|
| `Percentage` | `int` | 0-100 |
| `Stage` | `RenderStage` | Current pipeline stage |
| `Description` | `string` | Human-readable description |

`RenderStage` values: `Parsing`, `Styling`, `Layout`, `Rendering`, `Finishing`.

### `RenderResult`

| Property | Type | Description |
|----------|------|-------------|
| `Data` | `byte[]` | Rendered output bytes |
| `PageCount` | `int` | Number of pages rendered |
| `Format` | `string` | Output format ("pdf", "png", "jpeg", "webp") |

---

## Fonts

### `FontCollection`

Implements `IFontProvider`. Aggregates fonts from multiple sources.

```csharp
var fonts = new FontCollection();
fonts.RegisterFromResolver(new SystemFontResolver());
fonts.RegisterFromResolver(new DirectoryFontResolver("/path/to/fonts"));
```

### `SystemFontResolver`

Discovers fonts installed on the system. Works on Windows, macOS, and Linux.

### `DirectoryFontResolver`

Loads all font files (.ttf, .otf, .woff, .woff2) from a directory.

---

## PDF Signing (Rend.Pdf)

### `PdfSigning`

Static entry point for signing any PDF document.

```csharp
PdfSigning.Sign(input, output, options);
PdfSigning.Sign(input, output, certificate);
```

All methods are also available on `PdfSigningService`, which implements `IPdfSigningService` for dependency injection and testing.

### `PdfSignatureOptions`

| Property | Type | Description |
|----------|------|-------------|
| `Signer` | `IPdfSigner` | The signer that produces the PKCS#7/CMS signature (required) |
| `SignerName` | `string?` | Signer name displayed in the signature field |
| `Reason` | `string?` | Reason for signing |
| `Location` | `string?` | Location of signing |
| `ContactInfo` | `string?` | Contact information |

### `IPdfSigner`

```csharp
public interface IPdfSigner
{
    byte[] Sign(byte[] data);
    int EstimatedSignatureSize { get; }
}
```

Implement this for external signing (HSM, cloud KMS). `Sign()` receives the concatenated PDF byte ranges and must return a DER-encoded PKCS#7/CMS detached signature. `EstimatedSignatureSize` is used to reserve space in the PDF (typical value: 8192).

### `Pkcs12Signer`

Built-in `IPdfSigner` for local PKCS#12 (.pfx/.p12) certificates.

```csharp
var signer = new Pkcs12Signer(pfxBytes, "password");
var signer = new Pkcs12Signer(certificate);
```

---

## PDF Overlays (Rend.Pdf)

### `PdfOverlays`

Static entry point for drawing text and images onto existing PDF pages.

```csharp
PdfOverlays.Apply(input, output, elements);
```

All methods are also available on `PdfOverlay`, which implements `IPdfOverlay` for dependency injection and testing.

### `PdfOverlayElement`

Base class for overlay content. Coordinates use top-left origin.

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Page` | `int` | 1 | Page number (1-based) |
| `X` | `float` | 0 | X position from left edge in points |
| `Y` | `float` | 0 | Y position from top edge in points |

### `TextOverlay`

Draws text using a standard PDF font (no embedding required).

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Text` | `string` | "" | The text to draw |
| `FontSize` | `float` | 12 | Font size in points |
| `Color` | `CssColor` | Black | Text color |
| `FontFamily` | `string` | "Helvetica" | Font family: "Helvetica", "Times", "Courier" |
| `Bold` | `bool` | false | Use bold variant |
| `Italic` | `bool` | false | Use italic variant |

### `ImageOverlay`

Draws an image (JPEG or PNG).

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Data` | `byte[]` | empty | Image bytes (JPEG or PNG) |
| `Width` | `float` | 0 | Display width in points |
| `Height` | `float` | 0 | Display height in points |

---

## Standalone PDF Writer (Rend.Pdf)

### `PdfDocument`

Full PDF document builder. See [standalone-pdf.md](standalone-pdf.md) for details.

### `PdfDocumentOptions`

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Version` | `PdfVersion` | Pdf17 | PDF version (1.4, 1.5, 1.6, 1.7) |
| `Compression` | `PdfCompression` | Flate | Stream compression (None, Flate, FlateFast, FlateOptimal) |
| `Signature` | `PdfSignatureOptions?` | null | Sign the document on save |
| `Linearize` | `bool` | false | Enable fast web view |
| `UserPassword` | `string?` | null | Set to enable encryption |
| `OwnerPassword` | `string?` | null | Defaults to UserPassword |
| `Permissions` | `PdfPermissions` | All | Document permissions |
| `EncryptionMethod` | `PdfEncryptionMethod` | Aes128 | RC4-128 or AES-128 |
| `PdfAConformance` | `PdfALevel?` | null | PDF/A level (A1b, A2b, A3b) |
| `UseObjectStreams` | `bool` | false | Cross-ref streams for smaller files (PDF 1.5+) |
| `ParallelPageGeneration` | `bool` | false | Parallel content stream generation |

---

## Dependency Injection

Every public API has an interface and instance class behind the static facade. Use these when you need DI registration or mock-based testing.

| Static Facade | Interface | Implementation |
|---------------|-----------|----------------|
| `Render` | `IRenderer` | `HtmlRenderer` |
| `PdfSigning` | `IPdfSigningService` | `PdfSigningService` |
| `PdfOverlays` | `IPdfOverlay` | `PdfOverlay` |

```csharp
services.AddSingleton<IRenderer, HtmlRenderer>();
services.AddSingleton<IPdfSigningService, PdfSigningService>();
services.AddSingleton<IPdfOverlay, PdfOverlay>();
```
