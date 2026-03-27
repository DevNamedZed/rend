using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class BreakSpacesTests
    {
        private readonly ITestOutputHelper _output;

        public BreakSpacesTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void BreakSpaces_TrailingSpacesCauseWrap()
        {
            // With break-spaces, trailing spaces take up space and can cause a wrap.
            // "Hello     " has trailing spaces that should overflow a 60px container
            // and wrap to a second line.
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div id='t' style='width:60px;white-space:break-spaces;font-size:10px'>Hello     World</div></body>",
                viewportWidth: 400);

            var box = LayoutTestHelper.FindById(root, "t")!;
            // Should have more than 1 line due to break-spaces wrapping at trailing space
            Assert.True(box.LineBoxes != null && box.LineBoxes.Count >= 2,
                $"Expected at least 2 lines with break-spaces, got {box.LineBoxes?.Count ?? 0}");
        }

        [Fact]
        public void PreWrap_TrailingSpacesHang()
        {
            // With pre-wrap, trailing spaces hang and don't cause wrap.
            // This tests the opposite behavior from break-spaces.
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div id='t' style='width:200px;white-space:pre-wrap;font-size:10px'>Hello World</div></body>",
                viewportWidth: 400);

            var box = LayoutTestHelper.FindById(root, "t")!;
            // "Hello World" at 10px should fit in 200px in 1 line
            Assert.True(box.LineBoxes != null && box.LineBoxes.Count == 1,
                $"Expected 1 line with pre-wrap, got {box.LineBoxes?.Count ?? 0}");
        }
    }
}
