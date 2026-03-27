using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class FitContentFunctionTests
    {
        private readonly ITestOutputHelper _output;
        public FitContentFunctionTests(ITestOutputHelper output) { _output = output; }

        [Fact]
        public void FitContent_100px_ClampsBetweenMinAndMax()
        {
            // fit-content(100px) = min(max-content, max(min-content, 100px))
            // Content: two 60px inline-blocks → min-content=60, max-content=120
            // Result: min(120, max(60, 100)) = min(120, 100) = 100
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div id='t' style='width:fit-content(100px);height:100px;background:green'>
                    <div style='display:inline-block;width:60px;height:10px'></div>
                    <div style='display:inline-block;width:60px;height:10px'></div>
                </div></body>", viewportWidth: 800);

            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"fit-content div: W={box.ContentRect.Width} H={box.ContentRect.Height}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 100) < 2,
                $"Expected width≈100 from fit-content(100px), got {box.ContentRect.Width}");
        }
    }
}
