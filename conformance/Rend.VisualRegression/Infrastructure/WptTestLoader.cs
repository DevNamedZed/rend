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

                int loaded = 0;
                int skipped = 0;

                foreach (var testFile in testFiles)
                {
                    var testCase = TryLoadTest(testFile, moduleName, moduleDir);
                    if (testCase != null)
                    {
                        allCases.Add(testCase);
                        loaded++;
                    }
                    else
                    {
                        skipped++;
                    }
                }

                if (loaded > 0 || skipped > 0)
                {
                    Console.WriteLine($"  WPT {moduleName}: {loaded} loaded, {skipped} skipped");
                }
            }

            return allCases;
        }

        private static VisualTestCase? TryLoadTest(string filePath, string moduleName, string moduleDir)
        {
            string html;
            try
            {
                html = File.ReadAllText(filePath);
            }
            catch
            {
                return null;
            }

            // Skip tests that require JavaScript execution
            if (ScriptPattern.IsMatch(html))
            {
                return null;
            }

            // Skip tests that need async rendering (reftest-wait)
            if (ReftestWaitPattern.IsMatch(html))
            {
                return null;
            }

            // Inline external stylesheet references
            html = InlineStylesheets(html, Path.GetDirectoryName(filePath)!);

            // Build test ID from relative path
            var relativePath = Path.GetRelativePath(moduleDir, filePath);
            var testId = "wpt-" + moduleName + "-" +
                Path.GetFileNameWithoutExtension(relativePath)
                    .Replace(Path.DirectorySeparatorChar, '-')
                    .Replace(Path.AltDirectorySeparatorChar, '-');

            // Extract test name from <title> if present
            var titleMatch = Regex.Match(html, @"<title>([^<]+)</title>", RegexOptions.IgnoreCase);
            var testName = titleMatch.Success
                ? titleMatch.Groups[1].Value.Trim()
                : Path.GetFileNameWithoutExtension(filePath);

            var category = "WPT/" + moduleName;

            return new VisualTestCase
            {
                Id = testId,
                Name = testName,
                Category = category,
                Tags = new List<string> { "WPT", moduleName },
                Html = html,
                ViewportWidth = 800,
                ViewportHeight = 600,
                Tolerance = 0.01,
            };
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
