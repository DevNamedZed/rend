using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Rend.VisualRegression.Infrastructure
{
    /// <summary>
    /// Loads visual regression test cases from the Web Platform Tests (WPT) CSS suite.
    /// Scans conformance/Rend.VisualRegression/wpt/css/ for HTML test files, inlines
    /// external stylesheet references, and skips tests requiring JavaScript execution.
    /// </summary>
    public static class WptTestLoader
    {
        private static readonly Regex LinkStylesheetPattern = new Regex(
            @"<link\s[^>]*rel\s*=\s*[""']stylesheet[""'][^>]*>",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex HrefPattern = new Regex(
            @"href\s*=\s*[""']([^""']+)[""']",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex ScriptPattern = new Regex(
            @"<script[\s>]",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex ReftestWaitPattern = new Regex(
            @"class\s*=\s*[""'][^""']*reftest-wait",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <summary>
        /// Loads all WPT CSS test cases from the wpt/ directory.
        /// Returns an empty list if the directory doesn't exist (WPT not downloaded).
        /// </summary>
        public static List<VisualTestCase> LoadAll()
        {
            var wptCssDir = FindWptCssDir();
            if (wptCssDir == null || !Directory.Exists(wptCssDir))
            {
                return new List<VisualTestCase>();
            }

            var allCases = new List<VisualTestCase>();
            var moduleDirs = Directory.GetDirectories(wptCssDir)
                .OrderBy(d => Path.GetFileName(d))
                .ToList();

            foreach (var moduleDir in moduleDirs)
            {
                var moduleName = Path.GetFileName(moduleDir);
                var testFiles = Directory.GetFiles(moduleDir, "*.html", SearchOption.AllDirectories)
                    .Where(f => !IsReferenceFile(f))
                    .OrderBy(f => f)
                    .ToList();

                foreach (var testFile in testFiles)
                {
                    var testCase = TryLoadTest(testFile, moduleName, moduleDir);
                    if (testCase != null)
                    {
                        allCases.Add(testCase);
                    }
                }

                if (testFiles.Count > 0)
                {
                    Console.WriteLine($"  WPT {moduleName}: {testFiles.Count} registered");
                }
            }

            return allCases;
        }

        private static VisualTestCase? TryLoadTest(string filePath, string moduleName, string moduleDir)
        {
            // Build test ID from relative path — no file I/O needed.
            var relativePath = Path.GetRelativePath(moduleDir, filePath);
            var testId = "wpt-" + moduleName + "-" +
                Path.GetFileNameWithoutExtension(relativePath)
                    .Replace(Path.DirectorySeparatorChar, '-')
                    .Replace(Path.AltDirectorySeparatorChar, '-');

            var testName = Path.GetFileNameWithoutExtension(filePath);
            var category = "WPT/" + moduleName;

            var testCase = new VisualTestCase
            {
                Id = testId,
                Name = testName,
                Category = category,
                Tags = new List<string> { "WPT", moduleName },
                ViewportWidth = 800,
                ViewportHeight = 600,
                Tolerance = 0.01,
            };

            // Fully deferred: HTML is loaded, script-checked, and stylesheet-inlined
            // only when the test actually runs. This avoids reading 17K files at startup.
            testCase.SetDeferredHtml(filePath, path =>
            {
                var html = File.ReadAllText(path);

                // Skip tests that require JavaScript or async rendering
                if (ScriptPattern.IsMatch(html) || ReftestWaitPattern.IsMatch(html))
                {
                    return "";
                }

                html = InlineStylesheets(html, Path.GetDirectoryName(path)!);
                return html;
            });

            return testCase;
        }

        /// <summary>
        /// Resolves and inlines <![CDATA[<link rel="stylesheet" href="...">]]> references.
        /// </summary>
        private static string InlineStylesheets(string html, string baseDir)
        {
            var wptRoot = FindWptRoot();

            return LinkStylesheetPattern.Replace(html, match =>
            {
                var hrefMatch = HrefPattern.Match(match.Value);
                if (!hrefMatch.Success)
                {
                    return match.Value;
                }

                var href = hrefMatch.Groups[1].Value;

                // Skip external URLs
                if (href.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                    href.StartsWith("https://", StringComparison.OrdinalIgnoreCase) ||
                    href.StartsWith("//"))
                {
                    return match.Value;
                }

                // Resolve path: absolute paths (e.g., /fonts/ahem.css) are
                // relative to the WPT repo root, not the test file directory.
                string cssPath;
                if (href.StartsWith("/") && wptRoot != null)
                {
                    cssPath = Path.GetFullPath(Path.Combine(wptRoot, href.TrimStart('/')));
                }
                else
                {
                    cssPath = Path.GetFullPath(Path.Combine(baseDir, href));
                }

                if (!File.Exists(cssPath))
                {
                    return match.Value;
                }

                try
                {
                    var cssContent = File.ReadAllText(cssPath);
                    var cssDir = Path.GetDirectoryName(cssPath)!;
                    cssContent = InlineFontUrls(cssContent, cssDir);
                    return $"<style>/* inlined: {href} */\n{cssContent}\n</style>";
                }
                catch
                {
                    return match.Value;
                }
            });
        }

        /// <summary>
        /// Returns true if the file is a reference/comparison file (not a test itself).
        /// </summary>
        private static readonly Regex FontUrlPattern = new Regex(
            @"url\(([^)]+)\)",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <summary>
        /// Converts font url() references in CSS to data: URIs so they work
        /// with SetContentAsync (no filesystem access for relative paths).
        /// </summary>
        private static string InlineFontUrls(string cssContent, string cssDir)
        {
            return FontUrlPattern.Replace(cssContent, match =>
            {
                var urlValue = match.Groups[1].Value.Trim().Trim('"', '\'');
                if (urlValue.StartsWith("data:", StringComparison.OrdinalIgnoreCase) ||
                    urlValue.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                {
                    return match.Value;
                }

                // Resolve font path. Absolute URLs (starting with /) resolve
                // against the WPT repo root, not the CSS file directory.
                string fontPath;
                if (urlValue.StartsWith("/"))
                {
                    var wptRoot = FindWptRoot();
                    if (wptRoot != null)
                    {
                        fontPath = Path.GetFullPath(Path.Combine(wptRoot, urlValue.TrimStart('/')));
                    }
                    else
                    {
                        return match.Value;
                    }
                }
                else
                {
                    fontPath = Path.GetFullPath(Path.Combine(cssDir, urlValue));
                }
                if (!File.Exists(fontPath))
                {
                    return match.Value;
                }

                try
                {
                    var fontBytes = File.ReadAllBytes(fontPath);
                    var base64 = Convert.ToBase64String(fontBytes);
                    var extension = Path.GetExtension(fontPath).ToLower();
                    var mimeType = extension switch
                    {
                        ".ttf" => "font/ttf",
                        ".otf" => "font/otf",
                        ".woff" => "font/woff",
                        ".woff2" => "font/woff2",
                        _ => "application/octet-stream"
                    };
                    return $"url(data:{mimeType};base64,{base64})";
                }
                catch
                {
                    return match.Value;
                }
            });
        }

        private static bool IsReferenceFile(string filePath)
        {
            var normalized = filePath.Replace('\\', '/');
            return normalized.Contains("/reference/") ||
                   normalized.Contains("/ref/") ||
                   Path.GetFileName(filePath).EndsWith("-ref.html", StringComparison.OrdinalIgnoreCase) ||
                   Path.GetFileName(filePath).EndsWith("-notref.html", StringComparison.OrdinalIgnoreCase);
        }

        private static string? FindWptRoot()
        {
            var dir = AppContext.BaseDirectory;
            while (dir != null)
            {
                if (File.Exists(Path.Combine(dir, "Rend.VisualRegression.csproj")))
                {
                    var wptDir = Path.Combine(dir, "wpt");
                    if (Directory.Exists(wptDir))
                    {
                        return wptDir;
                    }
                    return null;
                }
                dir = Path.GetDirectoryName(dir);
            }
            return null;
        }

        private static string? FindWptCssDir()
        {
            // Look relative to the project directory
            var dir = AppContext.BaseDirectory;
            while (dir != null)
            {
                if (File.Exists(Path.Combine(dir, "Rend.VisualRegression.csproj")))
                {
                    var wptCss = Path.Combine(dir, "wpt", "css");
                    if (Directory.Exists(wptCss))
                    {
                        return wptCss;
                    }
                    return null;
                }
                dir = Path.GetDirectoryName(dir);
            }

            return null;
        }
    }
}
