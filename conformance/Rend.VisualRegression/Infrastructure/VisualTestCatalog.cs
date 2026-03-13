using System;
using System.Collections.Generic;

namespace Rend.VisualRegression.Infrastructure
{
    public static class VisualTestCatalog
    {
        private static readonly List<VisualTestCase> _cases = new();
        private static readonly object _lock = new();
        private static bool _initialized;

        public static void Register(VisualTestCase testCase)
        {
            lock (_lock)
            {
                _cases.Add(testCase);
            }
        }

        public static IReadOnlyList<VisualTestCase> AllCases
        {
            get
            {
                EnsureInitialized();
                lock (_lock)
                {
                    return _cases.AsReadOnly();
                }
            }
        }

        private static void EnsureInitialized()
        {
            lock (_lock)
            {
                if (_initialized)
                {
                    return;
                }
                _initialized = true;
            }

            // Load all tests from YAML files in TestData/
            var yamlCases = YamlTestLoader.LoadAll();
            if (yamlCases.Count > 0)
            {
                lock (_lock)
                {
                    _cases.AddRange(yamlCases);
                }
                Console.WriteLine($"Loaded {yamlCases.Count} tests from YAML files");
            }
            else
            {
                Console.Error.WriteLine("WARNING: No YAML tests loaded. Check TestData/ directory.");
            }
        }
    }
}
