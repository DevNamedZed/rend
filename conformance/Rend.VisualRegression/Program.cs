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

        // Create output directories:
        //   output/{runId}/            — per-run output (report + resources/)
        //   output/{runId}/resources/  — images, layout JSON, test HTML
        //   results/                   — latest run copy (self-contained for GH Pages)
        //   results/history/{runId}/   — archived report + results.json (lightweight)
        var projectRoot = FindProjectRoot();
        var runId = DateTime.Now.ToString("yyyy-MM-dd_HHmmss");
        var outputDir = Path.Combine(projectRoot, "output", runId);
        var resourcesDir = Path.Combine(outputDir, "resources");
        var resultsDir = Path.Combine(projectRoot, "results");
        var historyDir = Path.Combine(resultsDir, "history", runId);
        Directory.CreateDirectory(resourcesDir);
        Directory.CreateDirectory(resultsDir);
        Directory.CreateDirectory(historyDir);

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

        // Parse CLI flags.
        int workerCount = Math.Clamp(Environment.ProcessorCount / 4, 1, 2);
        string? filterPattern = null;
        string? tagFilter = null;
        for (int ai = 0; ai < args.Length; ai++)
        {
            if (args[ai] == "--parallel" && ai + 1 < args.Length && int.TryParse(args[ai + 1], out int p))
            {
                workerCount = Math.Clamp(p, 1, 32);
            }
            else if (args[ai] == "--filter" && ai + 1 < args.Length)
            {
                filterPattern = args[ai + 1];
            }
            else if (args[ai] == "--tag" && ai + 1 < args.Length)
            {
                tagFilter = args[ai + 1];
            }
        }
        Console.WriteLine($"Workers: {workerCount}");

        // Shared font provider (read-only after init).
        var fontProvider = CreateSharedFontProvider();

        // Browser pool — one Chrome process per worker slot, reused across tests.
        await using var browserPool = new BrowserPool(chromeExePath, workerCount);

        // Per-worker render resources (HtmlRenderer owns SkiaFontMapper and SkiaTextShaper, not thread-safe).
        var renderResources = new ThreadLocal<HtmlRenderer>(
            () => new HtmlRenderer(),
            trackAllValues: true);


        IReadOnlyList<VisualTestCase> testCases = VisualTestCatalog.AllCases;

        // Apply --filter (matches test ID substring)
        if (!string.IsNullOrEmpty(filterPattern))
        {
            testCases = testCases.Where(t =>
                t.Id.Contains(filterPattern, StringComparison.OrdinalIgnoreCase)).ToList();
            Console.WriteLine($"Filter: '{filterPattern}' → {testCases.Count} tests");
        }

        // Apply --tag (matches any tag, case-insensitive)
        if (!string.IsNullOrEmpty(tagFilter))
        {
            testCases = testCases.Where(t =>
                t.Tags.Any(tag => tag.Equals(tagFilter, StringComparison.OrdinalIgnoreCase))).ToList();
            Console.WriteLine($"Tag: '{tagFilter}' → {testCases.Count} tests");
        }

        // Skip layout tree capture for large suites to save memory.
        // Layout trees are only useful for debugging individual tests.
        bool captureLayoutTree = testCases.Count <= 100;

        var results = new ConcurrentBag<ComparisonResult>();
        var totalSw = Stopwatch.StartNew();
        int completedCount = 0;

        await Parallel.ForEachAsync(
            testCases,
            new ParallelOptions { MaxDegreeOfParallelism = workerCount },
            async (testCase, ct) =>
            {
                var renderer = renderResources.Value!;
                var result = await RunTest(testCase, browserPool, fontProvider, renderer, resourcesDir, captureLayoutTree: captureLayoutTree);
                if (result != null)
                {
                    results.Add(result);
                }

                Interlocked.Increment(ref completedCount);
            });

        totalSw.Stop();

        // Dispose render resources.
        foreach (var renderer in renderResources.Values)
        {
            renderer.Dispose();
        }
        renderResources.Dispose();

        // Sort results by test ID for stable output.
        var sortedResults = results.OrderBy(r => r.TestId).ToList();

        // Write JSON results file.
        var jsonPath = Path.Combine(outputDir, "results.json");
        WriteResultsJson(sortedResults, jsonPath, runId, totalSw.Elapsed);

        // Generate HTML report — all reports use "resources/" prefix.
        Console.WriteLine();
        var reportPath = Path.Combine(outputDir, "report.html");
        ReportGenerator.Generate(sortedResults, reportPath, "resources/");

        double avgDiff = sortedResults.Where(r => r.Outcome != ComparisonOutcome.Error)
            .Select(r => r.DiffPercentage)
            .DefaultIfEmpty(0)
            .Average();

        int passCount = sortedResults.Count(r => r.Outcome == ComparisonOutcome.Pass);
        int failCount = sortedResults.Count(r => r.Outcome == ComparisonOutcome.Fail);
        int errorCount = sortedResults.Count(r => r.Outcome == ComparisonOutcome.Error);

        Console.WriteLine($"Results: {sortedResults.Count} tests, {passCount} passed, {failCount} failed, {errorCount} errors, avg diff {avgDiff:F4}%");
        Console.WriteLine($"Duration: {totalSw.Elapsed.TotalSeconds:F1}s");

        // Copy to results/ (latest) and history/.
        var latestReportPath = Path.Combine(resultsDir, "report.html");
        var latestJsonPath = Path.Combine(resultsDir, "results.json");
        File.Copy(reportPath, latestReportPath, overwrite: true);
        File.Copy(jsonPath, latestJsonPath, overwrite: true);
        Console.Write($"Copying resources...");
        var copySw = Stopwatch.StartNew();
        CopyDirectoryParallel(resourcesDir, Path.Combine(resultsDir, "resources"));
        Console.Write($" results({copySw.Elapsed.TotalSeconds:F1}s)...");
        File.Copy(reportPath, Path.Combine(historyDir, "report.html"), overwrite: true);
        File.Copy(jsonPath, Path.Combine(historyDir, "results.json"), overwrite: true);
        CopyDirectoryParallel(resourcesDir, Path.Combine(historyDir, "resources"));
        Console.WriteLine($" history({copySw.Elapsed.TotalSeconds:F1}s). Done.");

        Console.WriteLine($"Output:  {outputDir}");
        Console.WriteLine($"Results: {resultsDir}");
        Console.WriteLine($"History: {historyDir}");
        Console.WriteLine($"Report:  {latestReportPath}");
        Console.WriteLine($"JSON:    {latestJsonPath}");

        return failCount > 0 || errorCount > 0 ? 1 : 0;
    }

    private static async Task<ComparisonResult> RunTest(
        VisualTestCase testCase,
        BrowserPool browserPool,
        Rend.Fonts.IFontProvider fontProvider,
        HtmlRenderer renderer,
        string resourcesDir,
        bool captureLayoutTree = true)
    {
        var sw = Stopwatch.StartNew();
        var result = new ComparisonResult
        {
            TestId = testCase.Id,
            TestName = testCase.Name,
            Category = testCase.Category,
            Tags = testCase.Tags,
            Html = testCase.Html,
        };

        try
        {
            var html = testCase.Html;
            // Skip tests that returned empty HTML (script/reftest-wait filtered at load time)
            if (string.IsNullOrWhiteSpace(html))
            {
                sw.Stop();
                // Don't count as pass or fail — just skip silently
                return null!;
            }

            if (!html.TrimStart().StartsWith("<!DOCTYPE", StringComparison.OrdinalIgnoreCase))
            {
                html = "<!DOCTYPE html>" + html;
            }

            // --- Chrome render with caching ---
            // Hash the HTML + viewport to check for cached Chrome screenshot.
            // On cache hit, skip Chrome entirely (~70% speedup per test).
            string cacheDir = Path.Combine(FindProjectRoot(), "cache", "chrome");
            Directory.CreateDirectory(cacheDir);
            string cacheKey;
            using (var sha = System.Security.Cryptography.SHA256.Create())
            {
                var hashInput = System.Text.Encoding.UTF8.GetBytes(
                    html + "|" + testCase.ViewportWidth + "x" + testCase.ViewportHeight);
                cacheKey = BitConverter.ToString(sha.ComputeHash(hashInput)).Replace("-", "").Substring(0, 16);
            }
            string cachePath = Path.Combine(cacheDir, cacheKey + ".png");

            Task<byte[]> chromeTask;
            if (File.Exists(cachePath))
            {
                // Cache hit — load from disk instead of rendering with Chrome
                chromeTask = Task.FromResult(File.ReadAllBytes(cachePath));
            }
            else
            {
                // Cache miss — render with Chrome and save
                chromeTask = RenderWithChromeAndCacheAsync(testCase, html, browserPool, resourcesDir, result, captureLayoutTree, cachePath);
            }

            // --- Rend render on current thread (CPU-bound: Skia) while Chrome does I/O ---
            byte[]? rendPng = null;
            RenderResult? rendRenderResult = null;
            Exception? rendError = null;
            try
            {
                rendRenderResult = renderer.ToImageResult(html, new RenderOptions
                {
                    PageSize = new SizeF(testCase.ViewportWidth, testCase.ViewportHeight),
                    MarginTop = 0, MarginRight = 0, MarginBottom = 0, MarginLeft = 0,
                    Dpi = 96,
                    ImageFormat = Rend.ImageOutputFormat.Png,
                    FontProvider = fontProvider,
                    CaptureLayoutTree = captureLayoutTree,
                });
                rendPng = rendRenderResult.Data;
            }
            catch (Exception ex) when (IsNativeLibraryFailure(ex))
            {
                rendError = ex;
            }

            // --- Wait for Chrome to finish ---
            byte[] chromePng = await chromeTask;

            // Write Chrome PNG to resources (needed for report, even on cache hit)
            var chromePath = Path.Combine(resourcesDir, $"{testCase.Id}-chrome.png");
            File.WriteAllBytes(chromePath, chromePng);
            result.ChromeImagePath = chromePath;

            // Handle Rend native library failure
            if (rendError != null)
            {
                sw.Stop();
                result.Outcome = ComparisonOutcome.Error;
                result.ErrorMessage = $"Native library not available: {rendError.Message}";
                result.Duration = sw.Elapsed;
                Console.WriteLine($"  ERROR  {testCase.Id} -- {testCase} ({rendError.GetType().Name})");
                return result;
            }

            // Save Rend output
            var rendPath = Path.Combine(resourcesDir, $"{testCase.Id}-rend.png");
            File.WriteAllBytes(rendPath, rendPng!);
            result.RendImagePath = rendPath;

            if (rendRenderResult?.LayoutTree != null)
            {
                result.RendLayout = rendRenderResult.LayoutTree;
                try
                {
                    var rendLayoutJson = JsonSerializer.Serialize(rendRenderResult.LayoutTree, new JsonSerializerOptions
                    {
                        WriteIndented = true,
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    });
                    var rendLayoutPath = Path.Combine(resourcesDir, $"{testCase.Id}-rend-layout.json");
                    File.WriteAllText(rendLayoutPath, rendLayoutJson);
                    result.RendLayoutPath = rendLayoutPath;
                }
                catch
                {
                    // Best-effort
                }
            }

            // Free render result — layout tree and data are already extracted/written
            rendRenderResult = null;

            // Write test HTML for report lightbox
            var htmlPath = Path.Combine(resourcesDir, $"{testCase.Id}.html");
            File.WriteAllText(htmlPath, html);

            // --- Compare ---
            var cmpResult = ImageDiffer.CompareAndDiff(chromePng, rendPng!, perChannelThreshold: 2);
            double diffPercent = cmpResult.StrictDiffFraction * 100.0;

            result.DiffPercentage = diffPercent;
            result.DiffPixels = cmpResult.StrictDiffPixels;
            result.ShiftTolerantDiffPercentage = cmpResult.ShiftTolerantDiffFraction * 100.0;
            result.ShiftTolerantDiffPixels = cmpResult.ShiftTolerantDiffPixels;
            result.TotalPixels = cmpResult.TotalPixels;

            if (cmpResult.DiffPng != null)
            {
                var diffPath = Path.Combine(resourcesDir, $"{testCase.Id}-diff.png");
                File.WriteAllBytes(diffPath, cmpResult.DiffPng);
                result.DiffImagePath = diffPath;
            }

            result.Outcome = diffPercent < testCase.Tolerance
                ? ComparisonOutcome.Pass
                : ComparisonOutcome.Fail;

            // Free large objects — already written to disk files.
            // Keeps memory bounded for large test suites (8000+ tests).
            result.ChromeLayout = null;
            result.RendLayout = null;
            result.Html = null;

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
                tags = r.Tags,
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

    private static string FindProjectRoot()
    {
        var dir = AppContext.BaseDirectory;
        while (dir != null)
        {
            if (File.Exists(Path.Combine(dir, "Rend.VisualRegression.csproj")))
            {
                return dir;
            }
            dir = Path.GetDirectoryName(dir);
        }

        var candidate = Path.Combine(Directory.GetCurrentDirectory(), "conformance", "Rend.VisualRegression");
        if (Directory.Exists(candidate))
        {
            return candidate;
        }

        return AppContext.BaseDirectory;
    }

    private static Rend.Fonts.IFontProvider CreateSharedFontProvider()
    {
        var collection = new Rend.Fonts.FontCollection();

        // On WSL2, SystemFontResolver finds Linux fonts (/usr/share/fonts).
        // Also need Windows fonts from /mnt/c/Windows/Fonts for proper rendering.
        try
        {
            var resolver = new Rend.Fonts.SystemFontResolver();
            collection.RegisterFromResolver(resolver);
        }
        catch { }

        // Load only essential Windows fonts instead of all 178 (saves ~500MB RAM).
        // Skia's system font manager handles fallback for characters not in these fonts.
        string winFontsPath = "/mnt/c/Windows/Fonts";
        if (Directory.Exists(winFontsPath))
        {
            string[] essentialFonts = {
                "arial.ttf", "ariali.ttf", "arialbd.ttf", "arialbi.ttf",
                "times.ttf", "timesi.ttf", "timesbd.ttf", "timesbi.ttf",
                "cour.ttf", "couri.ttf", "courbd.ttf", "courbi.ttf",
                "georgia.ttf", "georgiai.ttf", "georgiab.ttf", "georgiaz.ttf",
                "verdana.ttf", "verdanai.ttf", "verdanab.ttf", "verdanaz.ttf",
                "tahoma.ttf", "tahomabd.ttf",
                "trebuc.ttf", "trebucit.ttf", "trebucbd.ttf", "trebucbi.ttf",
                "impact.ttf", "comic.ttf", "comicbd.ttf",
                "consolai.ttf", "consola.ttf", "consolab.ttf", "consolaz.ttf",
                "segoeui.ttf", "segoeuii.ttf", "segoeuib.ttf", "segoeuiz.ttf",
                "calibri.ttf", "calibrii.ttf", "calibrib.ttf", "calibriz.ttf",
                "cambria.ttc", "symbol.ttf",
            };
            foreach (string fontFile in essentialFonts)
            {
                string fontPath = Path.Combine(winFontsPath, fontFile);
                if (File.Exists(fontPath))
                {
                    try
                    {
                        collection.RegisterFont(File.ReadAllBytes(fontPath));
                    }
                    catch { }
                }
            }
        }

        return collection;
    }

    private static void CopyDirectoryParallel(string sourceDir, string destDir)
    {
        Directory.CreateDirectory(destDir);

        var files = Directory.GetFiles(sourceDir);
        Parallel.ForEach(files, new ParallelOptions { MaxDegreeOfParallelism = 32 }, file =>
        {
            File.Copy(file, Path.Combine(destDir, Path.GetFileName(file)), overwrite: true);
        });

        foreach (var subDir in Directory.GetDirectories(sourceDir))
        {
            CopyDirectoryParallel(subDir, Path.Combine(destDir, Path.GetFileName(subDir)));
        }
    }

    private static void CopyDirectory(string sourceDir, string destDir)
    {
        Directory.CreateDirectory(destDir);

        foreach (var file in Directory.GetFiles(sourceDir))
        {
            File.Copy(file, Path.Combine(destDir, Path.GetFileName(file)), overwrite: true);
        }

        foreach (var subDir in Directory.GetDirectories(sourceDir))
        {
            CopyDirectory(subDir, Path.Combine(destDir, Path.GetFileName(subDir)));
        }
    }

    /// <summary>
    /// Renders test HTML in Chrome via Puppeteer and returns the screenshot PNG.
    /// Extracted so it can run concurrently with Rend rendering.
    /// </summary>
    private static async Task<byte[]> RenderWithChromeAndCacheAsync(
        VisualTestCase testCase, string html,
        BrowserPool browserPool, string resourcesDir,
        ComparisonResult result, bool captureLayoutTree, string cachePath)
    {
        var chromePng = await RenderWithChromeAsync(testCase, html, browserPool, resourcesDir, result, captureLayoutTree);
        // Save to cache for future runs
        try
        {
            File.WriteAllBytes(cachePath, chromePng);
        }
        catch
        {
            // Cache write failure is non-fatal
        }
        return chromePng;
    }

    private static async Task<byte[]> RenderWithChromeAsync(
        VisualTestCase testCase, string html,
        BrowserPool browserPool, string resourcesDir,
        ComparisonResult result, bool captureLayoutTree = true)
    {
        byte[] chromePng;
        await using (var lease = await browserPool.AcquirePageAsync())
        {
            var page = lease.Page;
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

            // Capture Chrome's layout tree via CDP (best-effort, skip for large suites)
            if (captureLayoutTree)
            {
                try
                {
                    result.ChromeLayout = await LayoutTreeDumper.DumpAsync(page);
                    if (result.ChromeLayout != null)
                    {
                        var layoutJson = JsonSerializer.Serialize(result.ChromeLayout, new JsonSerializerOptions
                        {
                            WriteIndented = true,
                            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                        });
                        var layoutPath = Path.Combine(resourcesDir, $"{testCase.Id}-chrome-layout.json");
                        File.WriteAllText(layoutPath, layoutJson);
                        result.ChromeLayoutPath = layoutPath;
                    }
                }
                catch
                {
                }
            }

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

        var chromePath = Path.Combine(resourcesDir, $"{testCase.Id}-chrome.png");
        File.WriteAllBytes(chromePath, chromePng);
        result.ChromeImagePath = chromePath;

        return chromePng;
    }

    private static bool IsNativeLibraryFailure(Exception ex)
    {
        return ex is DllNotFoundException ||
               ex is TypeInitializationException ||
               ex.InnerException is DllNotFoundException ||
               ex.Message.Contains("native", StringComparison.OrdinalIgnoreCase);
    }
}
