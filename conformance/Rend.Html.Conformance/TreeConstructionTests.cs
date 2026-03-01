using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Rend.Html.Parser;
using Xunit;

namespace Rend.Html.Conformance
{
    public class TreeConstructionTests
    {
        private static readonly HashSet<string> _knownFailures;

        static TreeConstructionTests()
        {
            var path = Path.Combine(AppContext.BaseDirectory, "known-failures.txt");
            if (File.Exists(path))
            {
                _knownFailures = new HashSet<string>(
                    File.ReadAllLines(path)
                        .Select(l => l.Trim())
                        .Where(l => !string.IsNullOrEmpty(l) && !l.StartsWith("#")),
                    StringComparer.Ordinal);
            }
            else
            {
                _knownFailures = new HashSet<string>();
            }
        }

        public static IEnumerable<object[]> GetTestCases()
        {
            var testDataDir = Path.Combine(AppContext.BaseDirectory, "TestData", "tree-construction");
            if (!Directory.Exists(testDataDir))
                yield break;

            foreach (var file in Directory.GetFiles(testDataDir, "*.dat").OrderBy(f => f))
            {
                foreach (var testCase in Html5libDatParser.Parse(file))
                {
                    yield return new object[] { testCase };
                }
            }
        }

        [Theory]
        [MemberData(nameof(GetTestCases))]
        public void TreeConstruction_MatchesExpected(Html5libTestCase testCase)
        {
            var key = $"{testCase.SourceFile}[{testCase.Index}]";
            bool knownFailing = _knownFailures.Contains(key);

            string actualTree;

            try
            {
                if (testCase.FragmentContext != null)
                {
                    // Fragment parsing test
                    var contextDoc = new Document();
                    var contextElement = contextDoc.CreateElement(testCase.FragmentContext);
                    var fragment = HtmlParser.ParseFragment(testCase.Data, contextElement);
                    actualTree = Html5libTreeSerializer.SerializeFragment(fragment);
                }
                else
                {
                    // Full document parsing test
                    var options = new HtmlParserOptions { Scripting = testCase.ScriptingEnabled };
                    var doc = HtmlParser.Parse(testCase.Data, options);
                    actualTree = Html5libTreeSerializer.Serialize(doc);
                }
            }
            catch (Exception) when (knownFailing)
            {
                return; // Known failure that throws — skip
            }

            if (actualTree == testCase.ExpectedTree)
            {
                if (knownFailing)
                {
                    Assert.Fail($"PASSING: {key} is in known-failures.txt but now passes. Remove it.");
                }
                return; // Pass
            }

            if (knownFailing)
            {
                return; // Expected failure — skip
            }

            Assert.Equal(testCase.ExpectedTree, actualTree);
        }

        /// <summary>
        /// Run all test cases and write failing keys to a file.
        /// Use this to bootstrap or update known-failures.txt.
        /// </summary>
        [Fact]
        public void CollectFailures()
        {
            var failures = new List<string>();
            var testDataDir = Path.Combine(AppContext.BaseDirectory, "TestData", "tree-construction");
            if (!Directory.Exists(testDataDir))
                return;

            foreach (var file in Directory.GetFiles(testDataDir, "*.dat").OrderBy(f => f))
            {
                foreach (var testCase in Html5libDatParser.Parse(file))
                {
                    var key = $"{testCase.SourceFile}[{testCase.Index}]";
                    try
                    {
                        string actualTree;
                        if (testCase.FragmentContext != null)
                        {
                            var contextDoc = new Document();
                            var contextElement = contextDoc.CreateElement(testCase.FragmentContext);
                            var fragment = HtmlParser.ParseFragment(testCase.Data, contextElement);
                            actualTree = Html5libTreeSerializer.SerializeFragment(fragment);
                        }
                        else
                        {
                            var options = new HtmlParserOptions { Scripting = testCase.ScriptingEnabled };
                            var doc = HtmlParser.Parse(testCase.Data, options);
                            actualTree = Html5libTreeSerializer.Serialize(doc);
                        }

                        if (actualTree != testCase.ExpectedTree)
                            failures.Add(key);
                    }
                    catch
                    {
                        failures.Add(key);
                    }
                }
            }

            var outputPath = Path.Combine(AppContext.BaseDirectory, "collected-failures.txt");
            File.WriteAllLines(outputPath, failures);
        }
    }
}
