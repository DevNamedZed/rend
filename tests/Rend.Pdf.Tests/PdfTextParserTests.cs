using Rend.Pdf.Internal;
using Xunit;

namespace Rend.Pdf.Tests
{
    /// <summary>
    /// Unit tests for the text-surgery helpers used by the overlay writer: object lookup must be
    /// token-anchored, and inline-dictionary extraction must not be fooled by `>>` inside a string.
    /// </summary>
    public class PdfTextParserTests
    {
        [Fact]
        public void ExtractObjectContent_DoesNotMatchObjectNumberAsSubstring()
        {
            // Object 13 is defined before object 3; looking up object 3 must not match the "3 0 obj"
            // that appears inside "13 0 obj".
            string pdf = "13 0 obj\n<< /Marker (thirteen) >>\nendobj\n"
                       + "3 0 obj\n<< /Marker (three) >>\nendobj\n";

            string content = PdfTextParser.ExtractObjectContent(pdf, 3);

            Assert.Contains("three", content);
            Assert.DoesNotContain("thirteen", content);
        }

        [Fact]
        public void TryGetInlineDictEntry_SkipsStringLiteralContainingDelimiters()
        {
            // The ">>" inside the (a >> b) string literal must not be taken as the dict's close.
            string dict = "/Font << /F1 1 0 R /Note (a >> b) >> /Other 2 0 R";

            bool found = PdfTextParser.TryGetInlineDictEntry(dict, "/Font",
                out string inner, out int matchStart, out int matchEnd);

            Assert.True(found);
            Assert.Contains("/F1 1 0 R", inner);
            Assert.Contains("(a >> b)", inner);
            Assert.True(matchEnd > matchStart);
        }

        [Fact]
        public void TryGetInlineDictEntry_DoesNotMatchKeyPrefix()
        {
            // /Font must not match inside /FontMatrix.
            string dict = "/FontMatrix [0.001 0 0 0.001 0 0] /Font << /F1 1 0 R >>";

            bool found = PdfTextParser.TryGetInlineDictEntry(dict, "/Font",
                out string inner, out _, out _);

            Assert.True(found);
            Assert.Contains("/F1 1 0 R", inner);
            Assert.DoesNotContain("0.001", inner);
        }
    }
}
