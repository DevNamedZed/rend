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
    static async Task<int> Main()
    {
        Console.WriteLine("Visual Regression: Chrome vs Rend");
        Console.WriteLine();

        // Resolve output directory relative to project root (not bin/)
        var outputDir = FindOutputDir();
        if (!Directory.Exists(outputDir))
            Directory.CreateDirectory(outputDir);

        // Download Chromium if needed
        Console.Write("Downloading Chromium... ");
        var browserFetcher = new BrowserFetcher();
        await browserFetcher.DownloadAsync();
        Console.WriteLine("done.");
        Console.WriteLine();

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
                Args = new[] { "--no-sandbox", "--disable-gpu", "--disable-dev-shm-usage" },
            });
            return browser;
        }

        // Create a shared font provider so we don't reload system fonts for every test
        var fontProvider = CreateSharedFontProvider();

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
                await page.SetContentAsync(testCase.Html, new NavigationOptions
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
                };

                byte[] rendPng;
                try
                {
                    rendPng = Render.ToImage(testCase.Html, renderOptions);
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

                // --- Compare ---
                // Use per-channel threshold of 2 to tolerate minor rounding differences
                // in gradient interpolation, opacity compositing, and font antialiasing
                // between Chrome's Blink engine and Skia.
                var (diffFraction, diffPixels, totalPixels) = ImageComparer.Compare(chromePng, rendPng, perChannelThreshold: 2);
                double diffPercent = diffFraction * 100.0;

                result.DiffPercentage = diffPercent;
                result.DiffPixels = diffPixels;
                result.TotalPixels = totalPixels;

                if (diffPixels > 0)
                {
                    var diffPng = ImageDiffer.GenerateDiff(chromePng, rendPng, perChannelThreshold: 2);
                    var diffPath = Path.Combine(outputDir, $"{testCase.Id}-diff.png");
                    File.WriteAllBytes(diffPath, diffPng);
                    result.DiffImagePath = diffPath;
                }

                result.Outcome = diffPercent <= testCase.Tolerance
                    ? ComparisonOutcome.Pass
                    : ComparisonOutcome.Fail;

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

        // Summary
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
