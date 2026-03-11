# PDF Overlays

Draw text and images onto specific positions on existing PDF pages. Useful for filling form templates, adding signatures, placing watermarks, or injecting dynamic content into static layouts.

## Basic Usage

```csharp
using Rend.Pdf;

using var input = File.OpenRead("template.pdf");
using var output = File.Create("filled.pdf");

await PdfOverlays.ApplyAsync(input, output, new PdfOverlayElement[]
{
    new TextOverlay
    {
        Page = 1,
        X = 72,
        Y = 200,
        Text = "John Doe",
        FontSize = 14,
    },
    new TextOverlay
    {
        Page = 1,
        X = 72,
        Y = 230,
        Text = "2026-03-09",
        FontSize = 11,
        Color = CssColor.FromRgba(100, 100, 100),
    },
    new ImageOverlay
    {
        Page = 2,
        X = 100,
        Y = 500,
        Width = 200,
        Height = 80,
        Data = File.ReadAllBytes("signature.png"),
    },
});
```

## Coordinates

All coordinates use **top-left origin** in points (1/72 inch), matching how you'd visually position content on a page. The library converts to PDF's bottom-left coordinate system internally.

## Text Overlays

Text is drawn using the standard PDF fonts, which are available in every PDF viewer without embedding.

Supported font families:

| `FontFamily` value | Also accepts |
|---------------------|--------------|
| `"Helvetica"` | (default) |
| `"Times"` | `"Times New Roman"`, `"serif"` |
| `"Courier"` | `"Courier New"`, `"monospace"` |

Each family supports `Bold` and `Italic` variants:

```csharp
new TextOverlay
{
    Text = "Important",
    FontFamily = "Helvetica",
    Bold = true,
    Italic = false,
    FontSize = 16,
    Color = CssColor.FromRgba(200, 0, 0),
}
```

## Image Overlays

JPEG and PNG images are supported. JPEG is embedded directly (most efficient). PNG is decoded and re-encoded for PDF compatibility.

```csharp
new ImageOverlay
{
    Page = 1,
    X = 350,
    Y = 700,
    Width = 150,
    Height = 60,
    Data = signatureBytes,
}
```

`Width` and `Height` control the display size on the page in points. The image is scaled to fit.

## Multi-Page

Elements can target any page in the document:

```csharp
await PdfOverlays.ApplyAsync(input, output, new PdfOverlayElement[]
{
    new TextOverlay { Page = 1, X = 72, Y = 100, Text = "Page 1 header" },
    new TextOverlay { Page = 3, X = 72, Y = 100, Text = "Page 3 header" },
    new TextOverlay { Page = 5, X = 72, Y = 750, Text = "Signature line" },
});
```

Elements targeting pages outside the document range are silently ignored.

## Dependency Injection

For DI and testing, use `IPdfOverlay` and `PdfOverlay`:

```csharp
services.AddSingleton<IPdfOverlay, PdfOverlay>();

public class FormFiller
{
    private readonly IPdfOverlay _overlay;

    public FormFiller(IPdfOverlay overlay) => _overlay = overlay;

    public async Task FillAsync(Stream template, Stream output, FormData data)
    {
        var elements = data.Fields.Select(f => new TextOverlay
        {
            Page = f.Page,
            X = f.X,
            Y = f.Y,
            Text = f.Value,
            FontSize = f.FontSize,
        });

        await _overlay.ApplyAsync(template, output, elements);
    }
}
```
