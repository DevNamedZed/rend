using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class AbsolutePercentHeightTests
    {
        private readonly ITestOutputHelper _output;
        public AbsolutePercentHeightTests(ITestOutputHelper output) { _output = output; }

        [Fact]
        public void AbsoluteDiv_PercentHeight_ResolvesAgainstCB()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='position:relative; width:100px;'>
                    <div id='abs' style='position:absolute; width:100px; height:50%;'></div>
                    <div style='height:200px;'></div>
                </div></body>");
            var abs = LayoutTestHelper.FindById(root, "abs");
            Assert.NotNull(abs);
            _output.WriteLine($"abs: {abs!.ContentRect.Width}x{abs.ContentRect.Height}");
            Assert.True(System.Math.Abs(abs.ContentRect.Height - 100) < 2,
                $"50% of 200px CB should be 100 (got {abs.ContentRect.Height})");
        }

        [Fact]
        public void AbsoluteDiv_FullSize_CoversParent()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='position:relative; width:200px;'>
                    <div id='abs' style='position:absolute; top:0; left:0; width:100%; height:100%;'></div>
                    <div style='height:150px;'></div>
                </div></body>");
            var abs = LayoutTestHelper.FindById(root, "abs");
            Assert.NotNull(abs);
            _output.WriteLine($"abs: {abs!.ContentRect.Width}x{abs.ContentRect.Height}");
            Assert.True(abs.ContentRect.Width >= 199, $"100% width (got {abs.ContentRect.Width})");
            Assert.True(abs.ContentRect.Height >= 149, $"100% height (got {abs.ContentRect.Height})");
        }

        [Fact]
        public void FixedDiv_PercentHeight_ResolvesAgainstViewport()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='fixed' style='position:fixed; top:0; left:0; width:50%; height:50%;'></div>
                </body>", 400, 300);
            var box = LayoutTestHelper.FindById(root, "fixed");
            Assert.NotNull(box);
            _output.WriteLine($"fixed: {box!.ContentRect.Width}x{box.ContentRect.Height}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 200) < 2, $"50% of 400 (got {box.ContentRect.Width})");
            Assert.True(System.Math.Abs(box.ContentRect.Height - 150) < 2, $"50% of 300 (got {box.ContentRect.Height})");
        }
    }
}
