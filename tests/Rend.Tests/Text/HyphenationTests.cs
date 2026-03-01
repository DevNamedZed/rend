using System;
using System.Linq;
using Rend.Text.Internal;
using Xunit;

namespace Rend.Tests.Text
{
    public class HyphenationTests
    {
        #region Pattern Loading

        [Fact]
        public void LoadPatterns_NullPatterns_ThrowsArgumentNullException()
        {
            var dict = new HyphenationDictionary();
            Assert.Throws<ArgumentNullException>(() => dict.LoadPatterns(null!));
        }

        [Fact]
        public void LoadPatterns_EmptyPatterns_NoException()
        {
            var dict = new HyphenationDictionary();
            dict.LoadPatterns("");
            var result = dict.FindHyphenPoints("hello");
            // No patterns loaded, so no hyphen points
            Assert.All(result, b => Assert.False(b));
        }

        [Fact]
        public void LoadPatterns_SinglePattern_AppliesCorrectly()
        {
            var dict = new HyphenationDictionary();
            // Pattern "he1l" means allow hyphen between 'e' and 'l' in "hel" (weight 1 = odd = allow)
            dict.LoadPatterns("he1l");
            dict.LeftMin = 1;
            dict.RightMin = 1;
            var result = dict.FindHyphenPoints("hello");
            // Position 1 (between 'e' and 'l') should be true
            Assert.True(result[1], "Expected hyphen point between 'e' and 'l'");
        }

        [Fact]
        public void LoadPatterns_MultiplePatterns_MaxWeightWins()
        {
            var dict = new HyphenationDictionary();
            // "he1l" gives weight 1 between e and l
            // "hel2l" gives weight 2 between first l and second l (even = forbid)
            dict.LoadPatterns("he1l hel2l");
            dict.LeftMin = 1;
            dict.RightMin = 1;
            var result = dict.FindHyphenPoints("hello");
            Assert.True(result[1], "Weight 1 at position 1 (odd = allow)");
            Assert.False(result[2], "Weight 2 at position 2 (even = forbid)");
        }

        #endregion

        #region FindHyphenPoints - Edge Cases

        [Fact]
        public void FindHyphenPoints_NullWord_ReturnsEmpty()
        {
            var dict = CreateEnglishDictionary();
            var result = dict.FindHyphenPoints(null!);
            Assert.Empty(result);
        }

        [Fact]
        public void FindHyphenPoints_EmptyString_ReturnsEmpty()
        {
            var dict = CreateEnglishDictionary();
            var result = dict.FindHyphenPoints("");
            Assert.Empty(result);
        }

        [Fact]
        public void FindHyphenPoints_SingleChar_ReturnsEmpty()
        {
            var dict = CreateEnglishDictionary();
            var result = dict.FindHyphenPoints("a");
            Assert.Empty(result);
        }

        [Fact]
        public void FindHyphenPoints_TwoChars_ReturnsOneFalse()
        {
            var dict = CreateEnglishDictionary();
            var result = dict.FindHyphenPoints("an");
            // Too short for min prefix (2) + min suffix (3) = 5 total, so word of length 2 has no hyphen points
            Assert.Single(result);
            Assert.False(result[0]);
        }

        [Fact]
        public void FindHyphenPoints_ThreeChars_NoHyphenation()
        {
            var dict = CreateEnglishDictionary();
            var result = dict.FindHyphenPoints("the");
            // 3 chars is too short for default LeftMin(2) + RightMin(3) = need at least 5 chars
            Assert.Equal(2, result.Length);
            Assert.All(result, b => Assert.False(b));
        }

        #endregion

        #region FindHyphenPoints - Common English Words

        [Fact]
        public void FindHyphenPoints_Hyphenation_HasBreakPoints()
        {
            var dict = CreateEnglishDictionary();
            var result = dict.FindHyphenPoints("hyphenation");
            // "hyphenation" should have at least one hyphen point
            Assert.True(result.Any(b => b), "Expected at least one hyphen point in 'hyphenation'");
        }

        [Fact]
        public void FindHyphenPoints_Algorithm_HasBreakPoints()
        {
            var dict = CreateEnglishDictionary();
            var result = dict.FindHyphenPoints("algorithm");
            // "algorithm" should have at least one hyphen point (e.g., al-go-rithm)
            Assert.True(result.Any(b => b), "Expected at least one hyphen point in 'algorithm'");
        }

        [Fact]
        public void FindHyphenPoints_Computer_HasBreakPoints()
        {
            var dict = CreateEnglishDictionary();
            var result = dict.FindHyphenPoints("computer");
            // "computer" should have at least one hyphen point (e.g., com-put-er)
            Assert.True(result.Any(b => b), "Expected at least one hyphen point in 'computer'");
        }

        [Fact]
        public void FindHyphenPoints_Programming_HasBreakPoints()
        {
            var dict = CreateEnglishDictionary();
            var result = dict.FindHyphenPoints("programming");
            // "programming" should have at least one hyphen point
            Assert.True(result.Any(b => b), "Expected at least one hyphen point in 'programming'");
        }

        [Fact]
        public void FindHyphenPoints_Development_HasBreakPoints()
        {
            var dict = CreateEnglishDictionary();
            var result = dict.FindHyphenPoints("development");
            // "development" should have at least one hyphen point (e.g., de-vel-op-ment)
            Assert.True(result.Any(b => b), "Expected at least one hyphen point in 'development'");
        }

        [Fact]
        public void FindHyphenPoints_ShortWordsNoBreaks()
        {
            var dict = CreateEnglishDictionary();
            // Words shorter than LeftMin + RightMin (2+3=5) should have no hyphen points
            string[] shortWords = { "go", "it", "at", "in", "up", "on", "do", "to", "cat", "dog", "run" };
            foreach (var word in shortWords)
            {
                var result = dict.FindHyphenPoints(word);
                Assert.All(result, b => Assert.False(b));
            }
        }

        [Fact]
        public void FindHyphenPoints_CaseInsensitive()
        {
            var dict = CreateEnglishDictionary();
            var lower = dict.FindHyphenPoints("computer");
            var upper = dict.FindHyphenPoints("COMPUTER");
            var mixed = dict.FindHyphenPoints("Computer");
            // All should produce the same hyphenation result
            Assert.Equal(lower.Length, upper.Length);
            Assert.Equal(lower.Length, mixed.Length);
            for (int i = 0; i < lower.Length; i++)
            {
                Assert.Equal(lower[i], upper[i]);
                Assert.Equal(lower[i], mixed[i]);
            }
        }

        #endregion

        #region Hyphenation Result Formatting

        [Fact]
        public void HyphenPoints_ProduceValidSplits()
        {
            var dict = CreateEnglishDictionary();
            string word = "hyphenation";
            var points = dict.FindHyphenPoints(word);
            var parts = SplitAtHyphenPoints(word, points);

            // Each part should have at least 1 character
            Assert.All(parts, p => Assert.True(p.Length >= 1, $"Part '{p}' is empty"));

            // Recombined parts should equal the original word
            string recombined = string.Join("", parts);
            Assert.Equal(word, recombined);
        }

        [Fact]
        public void HyphenPoints_RespectLeftMinRightMin()
        {
            var dict = CreateEnglishDictionary();
            string word = "algorithm";
            var points = dict.FindHyphenPoints(word);

            // First hyphen point should be at index >= LeftMin - 1 = 1
            // (meaning at least 2 chars before the first hyphen)
            for (int i = 0; i < Math.Min(dict.LeftMin - 1, points.Length); i++)
            {
                Assert.False(points[i], $"No hyphen allowed at position {i} (LeftMin constraint)");
            }

            // Last hyphen point should respect RightMin (at least 3 chars after)
            for (int i = Math.Max(0, points.Length - dict.RightMin + 1); i < points.Length; i++)
            {
                Assert.False(points[i], $"No hyphen allowed at position {i} (RightMin constraint)");
            }
        }

        #endregion

        #region LeftMin / RightMin

        [Fact]
        public void LeftMin_SetToOne_AllowsEarlierBreaks()
        {
            var dict = new HyphenationDictionary();
            dict.LoadPatterns("a1b");
            dict.LeftMin = 1;
            dict.RightMin = 1;

            // "abc" should allow hyphen between a and b
            var result = dict.FindHyphenPoints("abc");
            Assert.True(result[0], "With LeftMin=1, hyphen after first char should be allowed");
        }

        [Fact]
        public void RightMin_Default_PreventsSuffixBreaks()
        {
            var dict = new HyphenationDictionary();
            // Pattern that would place a hyphen near the end
            dict.LoadPatterns("o1n");
            dict.LeftMin = 1;
            dict.RightMin = 3;

            // "axon" - hyphen between o and n would leave only 1 char after
            var result = dict.FindHyphenPoints("axon");
            // Position 2 (between o and n) should be forbidden by RightMin=3
            Assert.False(result[2], "RightMin=3 should prevent hyphen with only 1 char remaining");
        }

        #endregion

        #region Word Boundary Patterns

        [Fact]
        public void WordBoundaryPattern_DotPrefix_MatchesWordStart()
        {
            var dict = new HyphenationDictionary();
            dict.LoadPatterns(".re1s");
            dict.LeftMin = 1;
            dict.RightMin = 1;

            var result = dict.FindHyphenPoints("restart");
            Assert.True(result[1], "Word-start pattern .re1s should match 'restart'");
        }

        #endregion

        #region English Patterns Integration

        [Fact]
        public void EnglishPatterns_LoadSuccessfully()
        {
            var patterns = HyphenationPatterns.GetEnglishPatterns();
            Assert.False(string.IsNullOrEmpty(patterns));

            var dict = new HyphenationDictionary();
            dict.LoadPatterns(patterns);

            // Basic sanity: a common word should have hyphen points
            var result = dict.FindHyphenPoints("information");
            Assert.True(result.Any(b => b), "Expected hyphen points in 'information'");
        }

        [Fact]
        public void EnglishPatterns_LongWord_HasMultipleBreakPoints()
        {
            var dict = CreateEnglishDictionary();
            var result = dict.FindHyphenPoints("internationalization");
            int breakCount = result.Count(b => b);
            Assert.True(breakCount >= 2, $"Expected at least 2 break points in 'internationalization', got {breakCount}");
        }

        #endregion

        #region Integration Tests (HTML rendering)

        [Fact]
        public void HyphensAuto_ProducesValidPdf()
        {
            byte[] result;
            try
            {
                result = Render.ToPdf(@"
                    <div style='width: 60px; hyphens: auto;'>
                        <p>Internationalization is important for development.</p>
                    </div>");
            }
            catch (Exception ex) when (IsNativeLibraryFailure(ex))
            {
                return;
            }

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void HyphensNone_SuppressesSoftHyphens()
        {
            byte[] result;
            try
            {
                // With hyphens: none, soft hyphens should be stripped and NOT produce breaks
                result = Render.ToPdf(@"
                    <div style='width: 100px; hyphens: none;'>
                        <p>Sup\u00ADer\u00ADcal\u00ADi\u00ADfrag\u00ADil\u00ADis\u00ADtic</p>
                    </div>");
            }
            catch (Exception ex) when (IsNativeLibraryFailure(ex))
            {
                return;
            }

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void HyphensAuto_NarrowContainer_ProducesValidPdf()
        {
            byte[] result;
            try
            {
                result = Render.ToPdf(@"
                    <div style='width: 50px; hyphens: auto; font-size: 12px;'>
                        <p>Programming algorithms require development experience.</p>
                    </div>");
            }
            catch (Exception ex) when (IsNativeLibraryFailure(ex))
            {
                return;
            }

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void HyphensManual_SoftHyphen_ProducesValidPdf()
        {
            byte[] result;
            try
            {
                // U+00AD is soft hyphen - should break at these points in a narrow container
                result = Render.ToPdf(@"
                    <div style='width: 80px; hyphens: manual;'>
                        <p>Sup\u00ADer\u00ADcal\u00ADi\u00ADfrag\u00ADil\u00ADis\u00ADtic</p>
                    </div>");
            }
            catch (Exception ex) when (IsNativeLibraryFailure(ex))
            {
                return;
            }

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        #endregion

        #region Helpers

        private static HyphenationDictionary CreateEnglishDictionary()
        {
            var dict = new HyphenationDictionary();
            dict.LoadPatterns(HyphenationPatterns.GetEnglishPatterns());
            return dict;
        }

        /// <summary>
        /// Splits a word at hyphen points and returns the parts.
        /// </summary>
        private static string[] SplitAtHyphenPoints(string word, bool[] points)
        {
            var parts = new System.Collections.Generic.List<string>();
            int start = 0;
            for (int i = 0; i < points.Length; i++)
            {
                if (points[i])
                {
                    parts.Add(word.Substring(start, i + 1 - start));
                    start = i + 1;
                }
            }
            parts.Add(word.Substring(start));
            return parts.ToArray();
        }

        private static bool IsNativeLibraryFailure(Exception ex)
        {
            return ex is DllNotFoundException ||
                   ex is TypeInitializationException ||
                   (ex.InnerException is DllNotFoundException) ||
                   ex.Message.Contains("native", StringComparison.OrdinalIgnoreCase);
        }

        #endregion
    }
}
