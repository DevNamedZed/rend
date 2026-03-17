using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Rend.VisualRegression.Infrastructure
{
    /// <summary>
    /// Loads visual regression test cases from YAML files in the TestData/ directory.
    /// Each YAML file represents a category of tests. The category name comes from
    /// the first comment line in the file (e.g., "# Typography" → "Typography").
    /// </summary>
    public static class YamlTestLoader
    {
        /// <summary>
        /// Loads all test cases from YAML files in the TestData/ directory.
        /// </summary>
        public static List<VisualTestCase> LoadAll()
        {
            var testDataDir = FindTestDataDir();
            if (testDataDir == null || !Directory.Exists(testDataDir))
            {
                Console.Error.WriteLine($"WARNING: TestData directory not found. Falling back to compiled tests.");
                return new List<VisualTestCase>();
            }

            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(UnderscoredNamingConvention.Instance)
                .IgnoreUnmatchedProperties()
                .Build();

            var allCases = new List<VisualTestCase>();
            var yamlFiles = Directory.GetFiles(testDataDir, "*.yaml").OrderBy(f => f).ToList();

            foreach (var yamlFile in yamlFiles)
            {
                try
                {
                    var content = File.ReadAllText(yamlFile);
                    var category = ExtractCategory(content, yamlFile);
                    var testFile = deserializer.Deserialize<YamlTestFile>(content);

                    if (testFile?.Tests == null)
                    {
                        continue;
                    }

                    foreach (var test in testFile.Tests)
                    {
                        var html = test.Html;

                        // Resolve file reference if present
                        if (!string.IsNullOrWhiteSpace(test.File))
                        {
                            var repoRoot = FindRepoRoot();
                            if (repoRoot != null)
                            {
                                var filePath = Path.Combine(repoRoot, test.File.Replace('/', Path.DirectorySeparatorChar));
                                if (System.IO.File.Exists(filePath))
                                {
                                    html = System.IO.File.ReadAllText(filePath);
                                }
                                else
                                {
                                    Console.Error.WriteLine($"WARNING: File not found: {filePath}");
                                    continue;
                                }
                            }
                            else
                            {
                                Console.Error.WriteLine($"WARNING: Could not find repo root to resolve file: {test.File}");
                                continue;
                            }
                        }

                        if (string.IsNullOrWhiteSpace(test.Id) || string.IsNullOrWhiteSpace(html))
                        {
                            continue;
                        }

                        var tags = new List<string>();
                        // Auto-tag by suite based on YAML filename
                        var fileName = Path.GetFileNameWithoutExtension(yamlFile);
                        if (fileName.Equals("playground", StringComparison.OrdinalIgnoreCase))
                        {
                            tags.Add("Playground");
                        }
                        else
                        {
                            tags.Add("Regression");
                        }
                        // Add category as a tag (if not already present)
                        if (!string.IsNullOrWhiteSpace(category)
                            && !tags.Contains(category, StringComparer.OrdinalIgnoreCase))
                        {
                            tags.Add(category);
                        }
                        // Add explicit tags from YAML
                        if (test.Tags != null)
                        {
                            foreach (var tag in test.Tags)
                            {
                                if (!tags.Contains(tag, StringComparer.OrdinalIgnoreCase))
                                {
                                    tags.Add(tag);
                                }
                            }
                        }

                        allCases.Add(new VisualTestCase
                        {
                            Id = test.Id.Trim(),
                            Name = test.Name?.Trim() ?? test.Id.Trim(),
                            Category = category,
                            Tags = tags,
                            Html = html.TrimEnd(),
                            ViewportWidth = test.ViewportWidth > 0 ? test.ViewportWidth : 400,
                            ViewportHeight = test.ViewportHeight > 0 ? test.ViewportHeight : 300,
                            Tolerance = test.Tolerance > 0 ? test.Tolerance : 0.01,
                        });
                    }
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"WARNING: Failed to load {Path.GetFileName(yamlFile)}: {ex.Message}");
                }
            }

            return allCases;
        }

        /// <summary>
        /// Extracts the category name from the first comment line (e.g., "# Typography").
        /// Falls back to formatting the filename.
        /// </summary>
        private static string ExtractCategory(string content, string filePath)
        {
            using var reader = new StringReader(content);
            var firstLine = reader.ReadLine();
            if (firstLine != null && firstLine.StartsWith("# "))
            {
                return firstLine.Substring(2).Trim();
            }

            // Fallback: derive from filename (e.g., "basic-elements.yaml" → "Basic Elements")
            var name = Path.GetFileNameWithoutExtension(filePath);
            return string.Join(" ", name.Split('-').Select(w =>
                w.Length > 0 ? char.ToUpper(w[0]) + w.Substring(1) : w));
        }

        private static string? FindRepoRoot()
        {
            var dir = AppContext.BaseDirectory;
            while (dir != null)
            {
                if (Directory.Exists(Path.Combine(dir, ".git")) ||
                    File.Exists(Path.Combine(dir, "Directory.Build.props")))
                {
                    return dir;
                }
                dir = Path.GetDirectoryName(dir);
            }
            return null;
        }

        private static string? FindTestDataDir()
        {
            // Walk up from base directory to find the project root
            var dir = AppContext.BaseDirectory;
            while (dir != null)
            {
                if (File.Exists(Path.Combine(dir, "Rend.VisualRegression.csproj")))
                {
                    return Path.Combine(dir, "TestData");
                }
                dir = Path.GetDirectoryName(dir);
            }

            // Fallback: check relative to working directory
            var candidate = Path.Combine(Directory.GetCurrentDirectory(),
                "conformance", "Rend.VisualRegression", "TestData");
            if (Directory.Exists(candidate))
            {
                return candidate;
            }

            return null;
        }
    }

    /// <summary>YAML file structure: a list of tests.</summary>
    internal sealed class YamlTestFile
    {
        public List<YamlTestEntry>? Tests { get; set; }
    }

    /// <summary>A single test entry in a YAML file.</summary>
    internal sealed class YamlTestEntry
    {
        public string Id { get; set; } = "";
        public string? Name { get; set; }
        public string Html { get; set; } = "";
        /// <summary>Path to an HTML file, relative to the repository root.</summary>
        public string? File { get; set; }
        public List<string>? Tags { get; set; }
        public int ViewportWidth { get; set; }
        public int ViewportHeight { get; set; }
        public double Tolerance { get; set; }
    }
}
