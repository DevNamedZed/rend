using System;
using System.IO;
using System.Threading.Tasks;
using PuppeteerSharp;
using SkiaSharp;

namespace Rend.VisualRegression;

/// <summary>
/// Diagnostic: render the exact same text with multiple Skia settings and compare
/// against Chrome's output to find the best-matching configuration.
/// </summary>
static class FontRenderDiag
{
    public static async Task Run(IBrowser browser)
    {
        Console.WriteLine("=== FONT RENDER SETTINGS DIAGNOSTIC ===\n");

        // Step 1: Get Chrome reference rendering
        string html = @"<html><head><style>
            * { margin: 0; padding: 0; }
            body { background: #fff; }
            .test { font-family: Arial, sans-serif; font-size: 14px; padding: 4px 8px; }
        </style></head><body>
            <div class='test'>The quick brown fox jumps over the lazy dog.</div>
        </body></html>";

        // Note: browser should be launched with --disable-lcd-text for grayscale AA
        await using var page = await browser.NewPageAsync();
        await page.SetViewportAsync(new ViewPortOptions { Width = 400, Height = 30 });
        await page.SetContentAsync(html, new NavigationOptions { WaitUntil = new[] { WaitUntilNavigation.Load } });
        byte[] chromePng = await page.ScreenshotDataAsync(new ScreenshotOptions
        {
            Clip = new PuppeteerSharp.Media.Clip { X = 0, Y = 0, Width = 400, Height = 30 }
        });

        File.WriteAllBytes("C:/tmp/font_diag_chrome.png", chromePng);

        // Decode Chrome pixels
        using var chromeImg = SKBitmap.Decode(chromePng);
        Console.WriteLine($"Chrome image: {chromeImg.Width}x{chromeImg.Height}");

        // Check if Chrome uses subpixel rendering (colored text fringes)
        AnalyzeTextPixels(chromeImg, "Chrome");

        // Step 2: Render the same text with Rend using different Skia settings
        string text = "The quick brown fox jumps over the lazy dog.";
        float fontSize = 14f;

        // Resolve Arial
        var fontProvider = CreateFontProvider();
        var fontDesc = new Rend.Fonts.FontDescriptor("Arial, sans-serif", 400, Rend.Css.CssFontStyle.Normal);
        var entry = fontProvider.ResolveFont(fontDesc);
        if (entry?.FontData == null)
        {
            Console.WriteLine("ERROR: Cannot resolve Arial font!");
            return;
        }

        using var skData = SKData.CreateCopy(entry.FontData);
        var typeface = SKTypeface.FromData(skData);
        if (typeface == null)
        {
            Console.WriteLine("ERROR: Cannot create SKTypeface!");
            return;
        }

        Console.WriteLine($"\nTypeface: {typeface.FamilyName}, Bold={typeface.IsBold}, Italic={typeface.IsItalic}");

        // Shape text with HarfBuzz to get glyph positions
        using var shaper = new Rend.Text.HarfBuzzTextShaper();
        var shaped = shaper.Shape(text, entry.FontData, fontSize);

        // Compare HarfBuzz vs Skia text width measurement
        Console.WriteLine("\n--- Width Comparison: HarfBuzz vs Skia vs Chrome ---");
        using var measureFont = new SKFont(typeface, fontSize);
        measureFont.Subpixel = true;
        measureFont.Hinting = SKFontHinting.None;

        float skiaWidth = measureFont.MeasureText(text);
        float hbWidth = shaped?.TotalWidth ?? 0;
        Console.WriteLine($"  HarfBuzz width: {hbWidth:F4}px");
        Console.WriteLine($"  Skia width:     {skiaWidth:F4}px");
        Console.WriteLine($"  Delta:          {Math.Abs(skiaWidth - hbWidth):F4}px ({Math.Abs(skiaWidth - hbWidth) / skiaWidth * 100:F2}%)");

        var chromeWidth = await page.EvaluateFunctionAsync<double>(@"() => {
            const canvas = document.createElement('canvas');
            const ctx = canvas.getContext('2d');
            ctx.font = '14px Arial, sans-serif';
            return ctx.measureText('The quick brown fox jumps over the lazy dog.').width;
        }");
        Console.WriteLine($"  Chrome width:   {chromeWidth:F4}px");
        Console.WriteLine($"  HB-Chrome:      {Math.Abs(hbWidth - chromeWidth):F4}px");
        Console.WriteLine($"  Skia-Chrome:    {Math.Abs(skiaWidth - chromeWidth):F4}px");
        string closer = Math.Abs(skiaWidth - chromeWidth) < Math.Abs(hbWidth - (float)chromeWidth) ? "CLOSER" : "NOT closer";
        Console.WriteLine($"  => Skia is {closer} to Chrome than HarfBuzz");

        // Per-glyph advance comparison
        Console.WriteLine("\n--- Per-Glyph Advance Comparison ---");
        if (shaped?.Glyphs != null)
        {
            var skiaGlyphIds = new ushort[shaped.Glyphs.Length];
            for (int i = 0; i < shaped.Glyphs.Length; i++)
                skiaGlyphIds[i] = (ushort)shaped.Glyphs[i].GlyphId;

            var skiaWidths = new float[shaped.Glyphs.Length];
            // Get widths per glyph ID
            var skGlyphWidthsBuf = new float[skiaGlyphIds.Length];
            var skGlyphBoundsBuf = new SKRect[skiaGlyphIds.Length];
            measureFont.GetGlyphWidths(skiaGlyphIds, skGlyphWidthsBuf, skGlyphBoundsBuf);
            Array.Copy(skGlyphWidthsBuf, skiaWidths, skiaGlyphIds.Length);
            float hbTotal = 0, skTotal = 0;
            int diffGlyphs = 0;
            for (int i = 0; i < Math.Min(shaped.Glyphs.Length, skiaWidths.Length); i++)
            {
                float hbAdv = shaped.Glyphs[i].XAdvance;
                float skAdv = skiaWidths[i];
                hbTotal += hbAdv;
                skTotal += skAdv;
                if (Math.Abs(hbAdv - skAdv) > 0.001f)
                {
                    diffGlyphs++;
                    if (diffGlyphs <= 10)
                    {
                        char ch = i < text.Length ? text[(int)shaped.Glyphs[i].Cluster] : '?';
                        Console.WriteLine($"  Glyph[{i}] '{ch}' (id={skiaGlyphIds[i]}): HB={hbAdv:F4} Skia={skAdv:F4} delta={hbAdv - skAdv:F4}");
                    }
                }
            }
            Console.WriteLine($"  Total: {diffGlyphs}/{shaped.Glyphs.Length} glyphs differ");
            Console.WriteLine($"  HB total: {hbTotal:F4}, Skia total: {skTotal:F4}, delta: {hbTotal - skTotal:F4}");
        }

        // Test matrix of settings
        var hintings = new[] {
            ("None", SKFontHinting.None),
            ("Slight", SKFontHinting.Slight),
            ("Normal", SKFontHinting.Normal),
            ("Full", SKFontHinting.Full),
        };
        var edgings = new[] {
            ("Alias", SKFontEdging.Alias),
            ("AntiAlias", SKFontEdging.Antialias),
            ("SubpixelAA", SKFontEdging.SubpixelAntialias),
        };

        float bestDiff = float.MaxValue;
        string bestConfig = "";

        foreach (var (hintName, hintVal) in hintings)
        {
            foreach (var (edgeName, edgeVal) in edgings)
            {
                foreach (bool subpixel in new[] { false, true })
                {
                    string configName = $"h={hintName}_e={edgeName}_sp={subpixel}";

                    using var surface = SKSurface.Create(new SKImageInfo(400, 30, SKColorType.Bgra8888, SKAlphaType.Premul));
                    var canvas = surface.Canvas;
                    canvas.Clear(SKColors.White);

                    using var font = new SKFont(typeface, fontSize);
                    font.Hinting = hintVal;
                    font.Edging = edgeVal;
                    font.Subpixel = subpixel;

                    using var paint = new SKPaint();
                    paint.IsAntialias = true;
                    paint.Color = SKColors.Black;
                    paint.Style = SKPaintStyle.Fill;

                    // Render using glyph positions (matching Rend's DrawGlyphs)
                    if (shaped?.Glyphs != null && shaped.Glyphs.Length > 0)
                    {
                        var glyphIds = new ushort[shaped.Glyphs.Length];
                        var positions = new SKPoint[shaped.Glyphs.Length];
                        float cx = 0;
                        for (int i = 0; i < shaped.Glyphs.Length; i++)
                        {
                            glyphIds[i] = (ushort)shaped.Glyphs[i].GlyphId;
                            positions[i] = new SKPoint(cx + shaped.Glyphs[i].XOffset, -shaped.Glyphs[i].YOffset);
                            cx += shaped.Glyphs[i].XAdvance;
                        }

                        using var builder = new SKTextBlobBuilder();
                        var buffer = builder.AllocatePositionedRun(font, shaped.Glyphs.Length);
                        buffer.SetGlyphs(glyphIds);
                        buffer.SetPositions(positions);
                        using var blob = builder.Build();

                        // Y position: match Chrome's baseline (font ascent from top + padding)
                        var metrics = font.Metrics;
                        float baselineY = 4f - metrics.Ascent; // padding + ascent
                        canvas.DrawText(blob!, 8f, baselineY, paint);
                    }

                    using var img = surface.Snapshot();
                    using var rendBmp = SKBitmap.FromImage(img);

                    // Compare against Chrome
                    float diff = CompareImages(chromeImg, rendBmp);

                    if (diff < bestDiff)
                    {
                        bestDiff = diff;
                        bestConfig = configName;
                    }

                    // Save all candidates for comparison
                    {
                        using var encoded = img.Encode(SKEncodedImageFormat.Png, 100);
                        File.WriteAllBytes($"C:/tmp/font_diag_{configName}.png", encoded.ToArray());
                    }

                    Console.WriteLine($"  {configName}: diff={diff:F4}%");
                }
            }
        }

        Console.WriteLine($"\n*** BEST: {bestConfig} with diff={bestDiff:F4}%");

        // Save the best configuration image
        {
            // Re-render best config for easy comparison
            File.WriteAllBytes("C:/tmp/font_diag_chrome_ref.png", chromePng);
            Console.WriteLine("  Chrome ref saved to C:/tmp/font_diag_chrome_ref.png");
        }

        Console.WriteLine("\n=== END FONT RENDER DIAGNOSTIC ===");
    }

    private static void AnalyzeTextPixels(SKBitmap img, string label)
    {
        // Check for subpixel rendering by looking for colored fringes on text edges
        int coloredPixels = 0;
        int grayPixels = 0;
        int totalTextPixels = 0;

        for (int y = 0; y < img.Height; y++)
        {
            for (int x = 0; x < img.Width; x++)
            {
                var p = img.GetPixel(x, y);
                if (p.Red == 255 && p.Green == 255 && p.Blue == 255) continue; // white bg
                if (p.Red == 0 && p.Green == 0 && p.Blue == 0) continue; // fully black

                totalTextPixels++;
                // Check if pixel has color deviation (subpixel rendering creates RGB fringes)
                int maxDiff = Math.Max(Math.Abs(p.Red - p.Green),
                    Math.Max(Math.Abs(p.Green - p.Blue), Math.Abs(p.Red - p.Blue)));
                if (maxDiff > 10)
                    coloredPixels++;
                else
                    grayPixels++;
            }
        }

        Console.WriteLine($"  {label} text analysis: {totalTextPixels} AA pixels, " +
            $"{coloredPixels} colored (subpixel), {grayPixels} gray");
        if (coloredPixels > grayPixels)
            Console.WriteLine($"  => {label} uses SUBPIXEL antialiasing (LCD/ClearType)");
        else
            Console.WriteLine($"  => {label} uses GRAYSCALE antialiasing");
    }

    private static float CompareImages(SKBitmap a, SKBitmap b)
    {
        int w = Math.Min(a.Width, b.Width);
        int h = Math.Min(a.Height, b.Height);
        int diffPixels = 0;
        int total = w * h;

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                var pa = a.GetPixel(x, y);
                var pb = b.GetPixel(x, y);
                if (Math.Abs(pa.Red - pb.Red) > 2 ||
                    Math.Abs(pa.Green - pb.Green) > 2 ||
                    Math.Abs(pa.Blue - pb.Blue) > 2)
                {
                    diffPixels++;
                }
            }
        }

        return total > 0 ? (float)diffPixels / total * 100f : 0f;
    }

    private static Rend.Fonts.IFontProvider CreateFontProvider()
    {
        var collection = new Rend.Fonts.FontCollection();
        string winFontsPath = "/mnt/c/Windows/Fonts";
        if (System.IO.Directory.Exists(winFontsPath))
        {
            try { collection.RegisterFontDirectory(winFontsPath); }
            catch { }
        }
        try { collection.RegisterFromResolver(new Rend.Fonts.SystemFontResolver()); }
        catch { }
        return collection;
    }
}
