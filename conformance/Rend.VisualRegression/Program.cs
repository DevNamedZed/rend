using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
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

        // Create a timestamped run folder under the output root.
        var outputRoot = FindOutputRoot();
        var runId = DateTime.Now.ToString("yyyy-MM-dd_HHmmss");
        var outputDir = Path.Combine(outputRoot, runId);
        Directory.CreateDirectory(outputDir);

        // Download Chrome 116 to match SkiaSharp's bundled Skia m116.
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

        // Run form control diagnostic
        if (args.Length > 0 && args[0] == "--form-diag")
        {
            var diagBrowser = await Puppeteer.LaunchAsync(new LaunchOptions
            {
                Headless = true,
                ExecutablePath = chromeExePath,
                Args = new[] { "--no-sandbox", "--disable-gpu", "--disable-dev-shm-usage",
                    "--disable-lcd-text" },
            });
            await FormDiagnostic.Run(diagBrowser);
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

        // Determine parallelism level.
        int workerCount = Math.Clamp(Environment.ProcessorCount / 2, 1, 8);
        for (int ai = 0; ai < args.Length; ai++)
        {
            if (args[ai] == "--parallel" && ai + 1 < args.Length && int.TryParse(args[ai + 1], out int p))
                workerCount = Math.Clamp(p, 1, 32);
        }
        Console.WriteLine($"Workers: {workerCount}");

        // Shared font provider (read-only after init).
        var fontProvider = CreateSharedFontProvider();

        // Browser pool — reuses Chrome instances across workers.
        await using var browserPool = new BrowserPool(chromeExePath, workerCount);

        // Per-worker render resources (SkiaTextShaper is not thread-safe).
        var renderResources = new ThreadLocal<(Rend.Output.Image.Internal.SkiaFontMapper mapper, Rend.Output.Image.SkiaTextShaper shaper)>(
            () =>
            {
                var mapper = new Rend.Output.Image.Internal.SkiaFontMapper();
                var shaper = new Rend.Output.Image.SkiaTextShaper(mapper);
                return (mapper, shaper);
            },
            trackAllValues: true);


        var testCases = VisualTestCatalog.AllCases;
        var results = new ConcurrentBag<ComparisonResult>();
        var totalSw = Stopwatch.StartNew();

        await Parallel.ForEachAsync(
            testCases,
            new ParallelOptions { MaxDegreeOfParallelism = workerCount },
            async (testCase, ct) =>
            {
                var res = renderResources.Value;
                var result = await RunTest(testCase, browserPool, fontProvider, res.shaper, res.mapper, outputDir);
                results.Add(result);
            });

        totalSw.Stop();

        // Dispose render resources.
        foreach (var res in renderResources.Values)
        {
            res.shaper.Dispose();
            res.mapper.Dispose();
        }
        renderResources.Dispose();

        // Sort results by test ID for stable output.
        var sortedResults = results.OrderBy(r => r.TestId).ToList();

        // Write JSON results file.
        var jsonPath = Path.Combine(outputDir, "results.json");
        WriteResultsJson(sortedResults, jsonPath, runId, totalSw.Elapsed);

        // Generate HTML report.
        Console.WriteLine();
        var reportPath = Path.Combine(outputDir, "report.html");
        ReportGenerator.Generate(sortedResults, reportPath);

        double avgDiff = sortedResults.Where(r => r.Outcome != ComparisonOutcome.Error)
            .Select(r => r.DiffPercentage)
            .DefaultIfEmpty(0)
            .Average();

        int passCount = sortedResults.Count(r => r.Outcome == ComparisonOutcome.Pass);
        int failCount = sortedResults.Count(r => r.Outcome == ComparisonOutcome.Fail);
        int errorCount = sortedResults.Count(r => r.Outcome == ComparisonOutcome.Error);

        Console.WriteLine($"Results: {sortedResults.Count} tests, {passCount} passed, {failCount} failed, {errorCount} errors, avg diff {avgDiff:F4}%");
        Console.WriteLine($"Duration: {totalSw.Elapsed.TotalSeconds:F1}s");
        Console.WriteLine($"Output:  {outputDir}");
        Console.WriteLine($"Report:  {reportPath}");
        Console.WriteLine($"JSON:    {jsonPath}");

        return failCount > 0 || errorCount > 0 ? 1 : 0;
    }

    private static async Task<ComparisonResult> RunTest(
        VisualTestCase testCase,
        BrowserPool browserPool,
        Rend.Fonts.IFontProvider fontProvider,
        Rend.Output.Image.SkiaTextShaper textShaper,
        Rend.Output.Image.Internal.SkiaFontMapper fontMapper,
        string outputDir)
    {
        var sw = Stopwatch.StartNew();
        var result = new ComparisonResult
        {
            TestId = testCase.Id,
            TestName = testCase.Name,
            Category = testCase.Category,
            Html = testCase.Html,
        };

        try
        {
            var html = testCase.Html;
            if (!html.TrimStart().StartsWith("<!DOCTYPE", StringComparison.OrdinalIgnoreCase))
                html = "<!DOCTYPE html>" + html;

            // --- Chrome render ---
            byte[] chromePng;
            await using (var lease = await browserPool.AcquireAsync())
            {
                await using var page = await lease.Browser.NewPageAsync();
                await page.SetViewportAsync(new ViewPortOptions
                {
                    Width = testCase.ViewportWidth,
                    Height = testCase.ViewportHeight,
                });
                await page.EmulateMediaFeaturesAsync(new MediaFeatureValue[]
                {
                    new MediaFeatureValue { MediaFeature = MediaFeature.PrefersColorScheme, Value = "light" },
                });

                await page.SetContentAsync(html, new NavigationOptions
                {
                    WaitUntil = new[] { WaitUntilNavigation.Load },
                });

                chromePng = await page.ScreenshotDataAsync(new ScreenshotOptions
                {
                    Clip = new PuppeteerSharp.Media.Clip
                    {
                        X = 0, Y = 0,
                        Width = testCase.ViewportWidth,
                        Height = testCase.ViewportHeight,
                    }
                });
            }

            var chromePath = Path.Combine(outputDir, $"{testCase.Id}-chrome.png");
            File.WriteAllBytes(chromePath, chromePng);
            result.ChromeImagePath = chromePath;

            // --- Rend render ---
            byte[] rendPng;
            try
            {
                rendPng = Render.ToImage(html, new RenderOptions
                {
                    PageSize = new SizeF(testCase.ViewportWidth, testCase.ViewportHeight),
                    MarginTop = 0, MarginRight = 0, MarginBottom = 0, MarginLeft = 0,
                    Dpi = 96,
                    ImageFormat = "png",
                    FontProvider = fontProvider,
                    TextShaper = textShaper,
                    FontMapper = fontMapper,
                });
            }
            catch (Exception ex) when (IsNativeLibraryFailure(ex))
            {
                sw.Stop();
                result.Outcome = ComparisonOutcome.Error;
                result.ErrorMessage = $"Native library not available: {ex.Message}";
                result.Duration = sw.Elapsed;
                Console.WriteLine($"  ERROR  {testCase.Id} -- {testCase} ({ex.GetType().Name})");
                return result;
            }

            var rendPath = Path.Combine(outputDir, $"{testCase.Id}-rend.png");
            File.WriteAllBytes(rendPath, rendPng);
            result.RendImagePath = rendPath;

            // --- Compare ---
            var cmpResult = ImageDiffer.CompareAndDiff(chromePng, rendPng, perChannelThreshold: 2);
            double diffPercent = cmpResult.StrictDiffFraction * 100.0;

            result.DiffPercentage = diffPercent;
            result.DiffPixels = cmpResult.StrictDiffPixels;
            result.ShiftTolerantDiffPercentage = cmpResult.ShiftTolerantDiffFraction * 100.0;
            result.ShiftTolerantDiffPixels = cmpResult.ShiftTolerantDiffPixels;
            result.TotalPixels = cmpResult.TotalPixels;

            if (cmpResult.DiffPng != null)
            {
                var diffPath = Path.Combine(outputDir, $"{testCase.Id}-diff.png");
                File.WriteAllBytes(diffPath, cmpResult.DiffPng);
                result.DiffImagePath = diffPath;
            }

            result.Outcome = diffPercent < testCase.Tolerance
                ? ComparisonOutcome.Pass
                : ComparisonOutcome.Fail;

            sw.Stop();
            result.Duration = sw.Elapsed;
            Console.WriteLine($"  {diffPercent,8:F4}%  {testCase.Id} -- {testCase}");
        }
        catch (Exception ex)
        {
            sw.Stop();
            result.Outcome = ComparisonOutcome.Error;
            result.ErrorMessage = ex.Message;
            result.Duration = sw.Elapsed;
            Console.WriteLine($"  ERROR  {testCase.Id} -- {testCase} ({ex.Message})");
        }

        return result;
    }

    private static void WriteResultsJson(List<ComparisonResult> results, string path, string runId, TimeSpan totalDuration)
    {
        int passCount = results.Count(r => r.Outcome == ComparisonOutcome.Pass);
        int failCount = results.Count(r => r.Outcome == ComparisonOutcome.Fail);
        int errorCount = results.Count(r => r.Outcome == ComparisonOutcome.Error);
        double avgDiff = results.Where(r => r.Outcome != ComparisonOutcome.Error)
            .Select(r => r.DiffPercentage).DefaultIfEmpty(0).Average();

        var payload = new
        {
            runId,
            timestamp = DateTime.Now.ToString("o"),
            totalDurationMs = (int)totalDuration.TotalMilliseconds,
            summary = new
            {
                total = results.Count,
                passed = passCount,
                failed = failCount,
                errors = errorCount,
                avgDiffPercentage = Math.Round(avgDiff, 4),
            },
            tests = results.Select(r => new
            {
                testId = r.TestId,
                testName = r.TestName,
                category = r.Category,
                outcome = r.Outcome.ToString(),
                diffPercentage = Math.Round(r.DiffPercentage, 4),
                shiftTolerantDiffPercentage = Math.Round(r.ShiftTolerantDiffPercentage, 4),
                diffPixels = r.DiffPixels,
                shiftTolerantDiffPixels = r.ShiftTolerantDiffPixels,
                totalPixels = r.TotalPixels,
                durationMs = (int)r.Duration.TotalMilliseconds,
                errorMessage = r.ErrorMessage,
            }).ToList(),
        };

        File.WriteAllText(path, JsonSerializer.Serialize(payload, new JsonSerializerOptions { WriteIndented = true }));
    }

    private static string FindOutputRoot()
    {
        var dir = AppContext.BaseDirectory;
        while (dir != null)
        {
            if (File.Exists(Path.Combine(dir, "Rend.VisualRegression.csproj")))
                return Path.Combine(dir, "output");
            dir = Path.GetDirectoryName(dir);
        }

        var candidate = Path.Combine(Directory.GetCurrentDirectory(), "conformance", "Rend.VisualRegression", "output");
        if (Directory.Exists(Path.GetDirectoryName(candidate)!))
            return candidate;

        return Path.Combine(AppContext.BaseDirectory, "output");
    }

    private static Rend.Fonts.IFontProvider CreateSharedFontProvider()
    {
        var collection = new Rend.Fonts.FontCollection();

        string winFontsPath = "/mnt/c/Windows/Fonts";
        if (Directory.Exists(winFontsPath))
        {
            try { collection.RegisterFontDirectory(winFontsPath); }
            catch { }
        }

        try
        {
            var resolver = new Rend.Fonts.SystemFontResolver();
            collection.RegisterFromResolver(resolver);
        }
        catch { }

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
