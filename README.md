# Rend

A high-fidelity HTML/CSS rendering engine for .NET. Converts HTML to PDF or images (PNG, JPEG, WebP) with pixel-perfect accuracy.

> **Status**: Active development. Focused on Chrome-level rendering accuracy. The public API will evolve — expect breaking changes.

## Quick Start

```csharp
// HTML to PDF
byte[] pdf = Render.ToPdf("<h1>Hello, World!</h1>");
File.WriteAllBytes("output.pdf", pdf);

// HTML to image
byte[] png = Render.ToImage("<h1>Hello, World!</h1>");
File.WriteAllBytes("output.png", png);
```

### With Options

```csharp
var options = new RenderOptions
{
    PageSize = Rend.Core.Values.PageSize.Letter,
    MarginTop = 48f,
    MarginRight = 48f,
    MarginBottom = 48f,
    MarginLeft = 48f,
    Title = "Invoice #1234",
    Author = "Acme Corp",
    ImageFormat = "png",
    Dpi = 150f,
};

byte[] pdf = Render.ToPdf(html, options);
```

### Async

```csharp
byte[] pdf = await Render.ToPdfAsync(html, options, cancellationToken);

// Or write directly to a stream
await Render.ToPdfAsync(html, outputStream, options, cancellationToken);
```

### Headers and Footers

```csharp
var options = new RenderOptions
{
    HeaderHtml = "<div style='font-size:10px;text-align:center'>Page {pageNumber} of {totalPages}</div>",
    FooterHtml = "<div style='font-size:9px;text-align:right'>{date}</div>",
};
```

### Progress Reporting

```csharp
var progress = new Progress<RenderProgress>(p =>
    Console.WriteLine($"[{p.Percentage}%] {p.Stage}: {p.Description}"));

var options = new RenderOptions { Progress = progress };
```

### Custom Fonts

```csharp
var fonts = new FontCollection();
fonts.RegisterFromResolver(new SystemFontResolver());
fonts.RegisterFromResolver(new DirectoryFontResolver("/path/to/fonts"));

var options = new RenderOptions { FontProvider = fonts };
```

## Features

### Layout

- Block, inline, and inline-block formatting contexts
- Flexbox (all axes, wrapping, alignment, gap, order, flex-grow/shrink)
- CSS Grid (auto-placement, fr/minmax/repeat, auto-fill/fit, named lines/areas, subgrid)
- Table layout (fixed and auto, rowspan/colspan, border-collapse)
- Positioned elements (relative, absolute, fixed, sticky)
- Floats and clear
- Multi-column layout
- Pagination with page breaks, orphans, and widows

### CSS

- Full cascade, specificity, and inheritance
- `calc()`, `var()`, custom properties
- `@media`, `@font-face`, `@import`, `@page`, `@supports`
- CSS Color Level 4
- Shorthand expansion

### Rendering

- Backgrounds (solid, gradients, images, multiple)
- Borders (all styles, border-radius, border-image)
- Box shadows, text shadows
- CSS transforms, opacity, filters, clip-path
- Linear, radial, and conic gradients
- SVG rendering
- `text-overflow: ellipsis`

### Text

- HarfBuzz text shaping
- Unicode line/word breaking (UAX #14, #29)
- Bidirectional text (UAX #9)
- White-space handling, text-transform, letter/word-spacing
- Font fallback chains
- WOFF/WOFF2 support
- System font discovery

### PDF Output

- PDF 1.7 with font subsetting (TrueType, CFF/OpenType)
- Bookmarks, clickable links, document metadata
- AcroForms (text fields, checkboxes, dropdowns)
- Encryption (RC4-128, AES-128)
- ICC color profiles, XMP metadata

### Image Output

- PNG, JPEG, WebP via SkiaSharp
- Configurable DPI

## Architecture

```
HTML string
     |
     v
[ HTML Parser ]        WHATWG HTML5 spec
     |
     v
[ CSS Parser ]         CSS Syntax Level 3
     |
     v
[ Style Resolution ]   Cascade, specificity, inheritance
     |
     v
[ Layout Engine ]      Block, inline, flex, grid, table, floats, positioning
     |
     v
[ Painter ]            Abstract drawing commands
     |
     +---------+---------+
     |                   |
     v                   v
[ PDF Target ]    [ Skia Target ]
     |                   |
     v                   v
  PDF bytes        Image bytes
```

### Projects

| Project | Description |
|---------|-------------|
| `Rend.Core` | Shared types — geometry, color, units |
| `Rend.Html` | HTML5 parser, DOM, selector engine |
| `Rend.Css` | CSS3 parser, cascade, style resolution |
| `Rend.Pdf` | Standalone PDF 1.7 writer (no HTML/CSS dependency) |
| `Rend` | Layout, rendering, text shaping, fonts, output bridges, orchestrator |

All projects target **netstandard2.0** (.NET Framework 4.6.1+, .NET Core 2.0+, .NET 5–9+).

### Standalone PDF Writer

`Rend.Pdf` is independently usable — no HTML/CSS dependency:

```csharp
using Rend.Pdf;

var doc = new PdfDocument();
var page = doc.AddPage(595.28f, 841.89f);
var font = doc.AddFont(fontData);

var cs = page.ContentStream;
cs.BeginText();
cs.SetFont(font, 12f);
cs.SetTextPosition(72f, 750f);
cs.ShowText(font.Encode("Hello from Rend.Pdf!"));
cs.EndText();

using var stream = File.Create("output.pdf");
doc.Save(stream);
```

## Building

Requires .NET 8 SDK.

```bash
dotnet build Rend.sln
dotnet test Rend.sln
```

## License

MIT
