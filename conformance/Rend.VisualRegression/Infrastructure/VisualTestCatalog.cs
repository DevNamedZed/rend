using System;
using System.Collections.Generic;

namespace Rend.VisualRegression.Infrastructure
{
    public static class VisualTestCatalog
    {
        /// <summary>
        /// Legacy registration method used by C# test classes.
        /// These are superseded by YAML-based test definitions.
        /// </summary>
        public static void Register(VisualTestCase testCase)
        {
            // No-op: all tests come from YAML files and WPT filesystem scan.
            // C# test classes exist for historical reasons but are not used.
        }

        /// <summary>
        /// Discovers all test case definitions.
        /// YAML tests: metadata + inline HTML from TestData/*.yaml
        /// WPT tests: metadata + deferred file-backed HTML from wpt/ directory
        /// </summary>
        public static List<VisualTestCase> DiscoverTests()
        {
            var cases = new List<VisualTestCase>();

            var yamlCases = YamlTestLoader.LoadAll();
            if (yamlCases.Count > 0)
            {
                cases.AddRange(yamlCases);
                Console.WriteLine($"Loaded {yamlCases.Count} tests from YAML files");
            }

            var wptCases = WptTestLoader.LoadAll();
            if (wptCases.Count > 0)
            {
                cases.AddRange(wptCases);
                Console.WriteLine($"Loaded {wptCases.Count} WPT CSS tests");
            }

            return cases;
        }
    }
}
