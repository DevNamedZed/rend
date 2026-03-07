using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using PuppeteerSharp;
using Rend;
using Rend.Core.Values;
using Rend.VisualRegression.Infrastructure;

namespace Rend.VisualRegression;

class Program
{
    static async Task<int> Main(string[] args)
    {
        Console.WriteLine("Visual Regression: Chrome vs Rend");
        Console.WriteLine();

        // Resolve output directory relative to project root (not bin/)
        var outputDir = FindOutputDir();
        if (!Directory.Exists(outputDir))
            Directory.CreateDirectory(outputDir);

        // Download Chrome 116 to match SkiaSharp 2.88.9's bundled Skia m116.
        // This ensures glyph rasterization uses the same Skia version on both sides.
        // Chrome 116 predates chrome-headless-shell, so we download the regular chrome binary.
        const string chromeBuildId = "116.0.5845.96";
        var chromeDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Chrome", $"Win64-{chromeBuildId}");
        var chromeExePath = Path.Combine(chromeDir, "chrome-win64", "chrome.exe");
        if (!File.Exists(chromeExePath))
        {
            Console.Write($"Downloading Chrome {chromeBuildId}... ");
            Directory.CreateDirectory(chromeDir);
            var zipUrl = $"https://storage.googleapis.com/chrome-for-testing-public/{chromeBuildId}/win64/chrome-win64.zip";
            var zipPath = Path.Combine(chromeDir, "chrome-win64.zip");
            using (var httpClient = new System.Net.Http.HttpClient())
            {
                var response = await httpClient.GetAsync(zipUrl);
                response.EnsureSuccessStatusCode();
                using var fs = File.Create(zipPath);
                await response.Content.CopyToAsync(fs);
            }
            System.IO.Compression.ZipFile.ExtractToDirectory(zipPath, chromeDir);
            File.Delete(zipPath);
            Console.WriteLine("done.");
        }
        else
        {
            Console.WriteLine($"Using Chrome {chromeBuildId}");
        }
        Console.WriteLine();

        // Run color sampler if requested
        if (args.Length > 0 && args[0] == "--color-sample")
        {
            ColorSampler.Run();
            return 0;
        }


        // Run text diagnostic if requested
        if (args.Length > 0 && args[0] == "--text-diag")
        {
            var diagBrowser = await Puppeteer.LaunchAsync(new LaunchOptions
            {
                Headless = true,
                ExecutablePath = chromeExePath,
                Args = new[] { "--no-sandbox", "--disable-gpu", "--disable-dev-shm-usage" },
            });
            await TextDiagnostic.Run(diagBrowser);
            await diagBrowser.DisposeAsync();
            return 0;
        }

        // Run table border-collapse diagnostic
        if (args.Length > 0 && args[0] == "--table-diag")
        {
            var diagBrowser = await Puppeteer.LaunchAsync(new LaunchOptions
            {
                Headless = true,
                ExecutablePath = chromeExePath,
                Args = new[] { "--no-sandbox", "--disable-gpu", "--disable-dev-shm-usage" },
            });
            await TableDiagnostic.Run(diagBrowser);
            await diagBrowser.DisposeAsync();
            return 0;
        }

        // Run font render settings diagnostic
        if (args.Length > 0 && args[0] == "--font-diag")
        {
            var diagBrowser = await Puppeteer.LaunchAsync(new LaunchOptions
            {
                Headless = true,
                ExecutablePath = chromeExePath,
                Args = new[] { "--no-sandbox", "--disable-gpu", "--disable-dev-shm-usage",
                    "--disable-lcd-text", "--font-render-hinting=none" },
            });
            await FontRenderDiag.Run(diagBrowser);
            await diagBrowser.DisposeAsync();
            return 0;
        }

        // Launch headless Chrome — will be restarted if it crashes
        IBrowser? browser = null;

        async Task<IBrowser> EnsureBrowser()
        {
            if (browser != null && !browser.IsClosed)
                return browser;
            if (browser != null)
            {
                try { await browser.DisposeAsync(); } catch { }
            }
            browser = await Puppeteer.LaunchAsync(new LaunchOptions
            {
                Headless = true,
                ExecutablePath = chromeExePath,
                Args = new[] {
                    "--no-sandbox", "--disable-gpu", "--disable-dev-shm-usage",
                    // Disable ClearType/LCD subpixel AA so Chrome uses grayscale AA.
                    // This matches Skia's off-screen rendering which doesn't know
                    // the display's physical pixel geometry.
                    "--disable-lcd-text",
                },
            });
            return browser;
        }

        // Create shared font provider, text shaper, and font mapper so we don't
        // reload system fonts, re-pin font data, or re-copy font files into native
        // memory for every test. Without sharing, 237 renders × ~5 fonts × ~1MB each
        // = massive native memory churn.
        var fontProvider = CreateSharedFontProvider();
        using var fontMapper = new Rend.Output.Image.Internal.SkiaFontMapper();
        // Use SkiaTextShaper for image rendering: Skia's text measurement matches
        // Chrome's exactly (via DirectWrite), while HarfBuzz's OpenType backend
        // produces slightly different advances (~0.005px/glyph, ~0.1px/line).
        using var textShaper = new Rend.Output.Image.SkiaTextShaper(fontMapper);

        var testCases = VisualTestCatalog.AllCases;
        var results = new List<ComparisonResult>();

        foreach (var testCase in testCases)
        {
            var sw = Stopwatch.StartNew();
            var result = new ComparisonResult
            {
                TestId = testCase.Id,
                TestName = testCase.Name,
                Category = testCase.Category,
            };

            try
            {
                // --- Chrome render (fresh page per test for stability) ---
                var b = await EnsureBrowser();
                await using var page = await b.NewPageAsync();
                await page.SetViewportAsync(new ViewPortOptions
                {
                    Width = testCase.ViewportWidth,
                    Height = testCase.ViewportHeight,
                });
                // Force light mode via media emulation to avoid system dark mode affecting screenshots
                await page.EmulateMediaFeaturesAsync(new MediaFeatureValue[]
                {
                    new MediaFeatureValue { MediaFeature = MediaFeature.PrefersColorScheme, Value = "light" },
                });
                // Ensure standards mode (DOCTYPE) so Chrome matches our renderer's behavior.
                var html = testCase.Html;
                if (!html.TrimStart().StartsWith("<!DOCTYPE", StringComparison.OrdinalIgnoreCase))
                    html = "<!DOCTYPE html>" + html;

                await page.SetContentAsync(html, new NavigationOptions
                {
                    WaitUntil = new[] { WaitUntilNavigation.Load },
                });
                // Clip to viewport size to ensure Chrome and Rend images have the same dimensions.
                // FullPage can produce larger images when content overflows the viewport.
                var chromePng = await page.ScreenshotDataAsync(new ScreenshotOptions
                {
                    Clip = new PuppeteerSharp.Media.Clip
                    {
                        X = 0,
                        Y = 0,
                        Width = testCase.ViewportWidth,
                        Height = testCase.ViewportHeight,
                    }
                });

                var chromePath = Path.Combine(outputDir, $"{testCase.Id}-chrome.png");
                File.WriteAllBytes(chromePath, chromePng);
                result.ChromeImagePath = chromePath;

                // --- Rend render ---
                var renderOptions = new RenderOptions
                {
                    PageSize = new SizeF(testCase.ViewportWidth, testCase.ViewportHeight),
                    MarginTop = 0,
                    MarginRight = 0,
                    MarginBottom = 0,
                    MarginLeft = 0,
                    Dpi = 96,
                    ImageFormat = "png",
                    FontProvider = fontProvider,
                    TextShaper = textShaper,
                    FontMapper = fontMapper,
                };

                byte[] rendPng;
                try
                {
                    rendPng = Render.ToImage(html, renderOptions);
                }
                catch (Exception ex) when (IsNativeLibraryFailure(ex))
                {
                    sw.Stop();
                    result.Outcome = ComparisonOutcome.Error;
                    result.ErrorMessage = $"Native library not available: {ex.Message}";
                    result.Duration = sw.Elapsed;
                    results.Add(result);
                    Console.WriteLine($"  ERROR  {testCase.Id} -- {testCase} ({ex.GetType().Name})");
                    continue;
                }

                var rendPath = Path.Combine(outputDir, $"{testCase.Id}-rend.png");
                File.WriteAllBytes(rendPath, rendPng);
                result.RendImagePath = rendPath;

                // --- Compare & Diff in single pass (decode images once, not twice) ---
                // Use per-channel threshold of 2 to tolerate minor rounding differences
                // in gradient interpolation, opacity compositing, and font antialiasing
                // between Chrome's Blink engine and Skia.
                var cmpResult = ImageDiffer.CompareAndDiff(chromePng, rendPng, perChannelThreshold: 2);
                double diffPercent = cmpResult.StrictDiffFraction * 100.0;
                double shiftDiffPercent = cmpResult.ShiftTolerantDiffFraction * 100.0;

                result.DiffPercentage = diffPercent;
                result.DiffPixels = cmpResult.StrictDiffPixels;
                result.ShiftTolerantDiffPercentage = shiftDiffPercent;
                result.ShiftTolerantDiffPixels = cmpResult.ShiftTolerantDiffPixels;
                result.TotalPixels = cmpResult.TotalPixels;

                if (cmpResult.DiffPng != null)
                {
                    var diffPath = Path.Combine(outputDir, $"{testCase.Id}-diff.png");
                    File.WriteAllBytes(diffPath, cmpResult.DiffPng);
                    result.DiffImagePath = diffPath;
                }


                // Determine outcome: strict pixel match only
                if (diffPercent <= testCase.Tolerance)
                {
                    result.Outcome = ComparisonOutcome.Pass;
                }
                else
                {
                    result.Outcome = ComparisonOutcome.Fail;
                }

                sw.Stop();
                result.Duration = sw.Elapsed;
                results.Add(result);

                Console.WriteLine($"  {diffPercent,6:F2}%  {testCase.Id} -- {testCase}");
            }
            catch (Exception ex)
            {
                sw.Stop();
                result.Outcome = ComparisonOutcome.Error;
                result.ErrorMessage = ex.Message;
                result.Duration = sw.Elapsed;
                results.Add(result);
                Console.WriteLine($"  ERROR  {testCase.Id} -- {testCase} ({ex.Message})");

                // If it was a browser crash, force restart on next iteration
                if (ex.Message.Contains("Session closed") || ex.Message.Contains("Protocol error"))
                {
                    try { if (browser != null) await browser.DisposeAsync(); } catch { }
                    browser = null;
                }
            }
        }

        // Clean up browser
        if (browser != null)
        {
            try { await browser.DisposeAsync(); } catch { }
        }

        // Generate report
        Console.WriteLine();
        var reportPath = Path.Combine(outputDir, "report.html");
        ReportGenerator.Generate(results, reportPath);

        double avgDiff = results.Where(r => r.Outcome != ComparisonOutcome.Error)
            .Select(r => r.DiffPercentage)
            .DefaultIfEmpty(0)
            .Average();

        int passCount = results.Count(r => r.Outcome == ComparisonOutcome.Pass);
        int failCount = results.Count(r => r.Outcome == ComparisonOutcome.Fail);
        int errorCount = results.Count(r => r.Outcome == ComparisonOutcome.Error);

        Console.WriteLine($"Results: {results.Count} tests, {passCount} passed, {failCount} failed, {errorCount} errors, avg diff {avgDiff:F2}%");
        Console.WriteLine($"Report: {reportPath}");

        return failCount > 0 || errorCount > 0 ? 1 : 0;
    }

    private static string FindOutputDir()
    {
        // Walk up from the current directory to find the project directory
        var dir = AppContext.BaseDirectory;
        while (dir != null)
        {
            var csproj = Path.Combine(dir, "Rend.VisualRegression.csproj");
            if (File.Exists(csproj))
                return Path.Combine(dir, "output");
            dir = Path.GetDirectoryName(dir);
        }

        // Fallback: try relative from working directory
        var candidate = Path.Combine(Directory.GetCurrentDirectory(), "conformance", "Rend.VisualRegression", "output");
        if (Directory.Exists(Path.GetDirectoryName(candidate)!))
            return candidate;

        // Last resort
        return Path.Combine(AppContext.BaseDirectory, "output");
    }

    private static Rend.Fonts.IFontProvider CreateSharedFontProvider()
    {
        var collection = new Rend.Fonts.FontCollection();

        // On WSL2, prefer Windows fonts first so Rend uses the same fonts as Chrome
        // (which runs on Windows via PuppeteerSharp). This ensures matching metrics.
        string winFontsPath = "/mnt/c/Windows/Fonts";
        if (System.IO.Directory.Exists(winFontsPath))
        {
            try
            {
                collection.RegisterFontDirectory(winFontsPath);
            }
            catch
            {
                // Windows fonts unavailable
            }
        }

        try
        {
            var resolver = new Rend.Fonts.SystemFontResolver();
            collection.RegisterFromResolver(resolver);
        }
        catch
        {
            // System fonts unavailable — fall back to defaults
        }

        return collection;
    }

    private static bool IsNativeLibraryFailure(Exception ex)
    {
        return ex is DllNotFoundException ||
               ex is TypeInitializationException ||
               ex.InnerException is DllNotFoundException ||
               ex.Message.Contains("native", StringComparison.OrdinalIgnoreCase);
    }
}
