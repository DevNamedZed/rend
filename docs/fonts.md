# Font Configuration

Rend uses HarfBuzz for text shaping with system font discovery by default. You can customize font loading for full control over which fonts are available.

## Default Behavior

When `RenderOptions.FontProvider` is null, Rend creates a `FontCollection` with `SystemFontResolver` and discovers fonts from standard OS directories:

- **Windows**: `C:\Windows\Fonts`
- **macOS**: `/Library/Fonts`, `/System/Library/Fonts`, `~/Library/Fonts`
- **Linux**: `/usr/share/fonts`, `/usr/local/share/fonts`, `~/.fonts`

## Custom Font Sources

### System Fonts + Custom Directory

```csharp
var fonts = new FontCollection();
fonts.RegisterFromResolver(new SystemFontResolver());
fonts.RegisterFromResolver(new DirectoryFontResolver("/app/fonts"));

var options = new RenderOptions { FontProvider = fonts };
```

### Custom Directory Only (No System Fonts)

```csharp
var fonts = new FontCollection();
fonts.RegisterFromResolver(new DirectoryFontResolver("/app/fonts"));

var options = new RenderOptions { FontProvider = fonts };
```

### Supported Font Formats

| Format | Extensions |
|--------|-----------|
| TrueType | `.ttf` |
| OpenType | `.otf` |
| WOFF | `.woff` |
| WOFF2 | `.woff2` |
| TrueType Collection | `.ttc` |

## CSS @font-face

`@font-face` rules in your HTML/CSS are loaded via the `ResourceLoader`:

```html
<style>
@font-face {
    font-family: 'CustomFont';
    src: url('/fonts/custom.woff2') format('woff2');
    font-weight: 400;
    font-style: normal;
}

body { font-family: 'CustomFont', sans-serif; }
</style>
```

```csharp
var options = new RenderOptions
{
    ResourceLoader = new FileSystemResourceLoader(),
    BaseUrl = new Uri("file:///app/assets/"),
};
```

## Font Fallback

Rend follows the CSS font-family fallback chain. If a glyph isn't found in the first font, it tries the next one. Generic families map to platform defaults:

| Generic | Windows | macOS | Linux |
|---------|---------|-------|-------|
| `serif` | Times New Roman | Times | DejaVu Serif |
| `sans-serif` | Arial | Helvetica | DejaVu Sans |
| `monospace` | Consolas | Menlo | DejaVu Sans Mono |

## Performance: Shared TextShaper and FontMapper

For batch rendering, reuse the text shaper and font mapper to avoid repeated native memory allocation:

```csharp
using var shaper = new HarfBuzzTextShaper();
using var fontMapper = new SkiaFontMapper();

var options = new RenderOptions
{
    TextShaper = shaper,
    FontMapper = fontMapper,
};

foreach (var html in documents)
{
    Render.ToPdf(html, outputStream, options);
}
```

Both are thread-safe for read operations. The caller owns disposal.
