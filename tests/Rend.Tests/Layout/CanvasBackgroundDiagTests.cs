using Xunit;
using Xunit.Abstractions;
using Rend.Style;

namespace Rend.Tests.Layout
{
    public class CanvasBackgroundDiagTests
    {
        private readonly ITestOutputHelper _output;
        public CanvasBackgroundDiagTests(ITestOutputHelper output) { _output = output; }

        [Fact]
        public void RootElement_WithMarginAndHeight_HasCorrectPaddingRect()
        {
            var root = LayoutTestHelper.Layout(
                @"<!DOCTYPE html><html style='background:linear-gradient(red,blue);height:300px;margin:50px'><body></body></html>",
                viewportWidth: 800, viewportHeight: 600);

            _output.WriteLine($"Root: Content={root.ContentRect} Padding={root.PaddingRect}");
            var tag = (root.StyledNode as StyledElement)?.TagName ?? "wrapper";
            _output.WriteLine($"Root tag: {tag}");

            for (int i = 0; i < root.Children.Count; i++)
            {
                var child = root.Children[i];
                var childTag = (child.StyledNode as StyledElement)?.TagName ?? "anon";
                _output.WriteLine($"  Child[{i}] ({childTag}): Content={child.ContentRect} Padding={child.PaddingRect}");
            }

            // The html element should have margin-offset ContentRect
            // Find the html element
            var htmlBox = root;
            if (root.StyledNode == null && root.Children.Count > 0)
            {
                htmlBox = root.Children[0];
            }
            var htmlTag = (htmlBox.StyledNode as StyledElement)?.TagName ?? "?";
            _output.WriteLine($"HTML box ({htmlTag}): Content={htmlBox.ContentRect} Padding={htmlBox.PaddingRect}");

            // Root element width fills viewport (correct per CSS spec).
            // The positioning area differs from canvas via X/Y offset (margin).
            Assert.True(htmlBox.PaddingRect.X > 0 || htmlBox.PaddingRect.Y > 0,
                $"HTML PaddingRect should be offset from canvas origin, got X={htmlBox.PaddingRect.X} Y={htmlBox.PaddingRect.Y}");
        }
    }
}
