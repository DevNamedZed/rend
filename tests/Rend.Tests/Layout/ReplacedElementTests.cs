using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class ReplacedElementTests
    {
        private readonly ITestOutputHelper _output;
        public ReplacedElementTests(ITestOutputHelper output) { _output = output; }

        [Fact]
        public void Img_ExplicitDimensions()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <img id='img' width='100' height='80'></body>");
            var img = LayoutTestHelper.FindById(root, "img");
            Assert.NotNull(img);
            _output.WriteLine($"img: {img!.ContentRect.Width}x{img.ContentRect.Height}");
            Assert.True(System.Math.Abs(img.ContentRect.Width - 100) < 2,
                $"img width from attribute (got {img.ContentRect.Width})");
            Assert.True(System.Math.Abs(img.ContentRect.Height - 80) < 2,
                $"img height from attribute (got {img.ContentRect.Height})");
        }

        [Fact]
        public void Img_CssOverridesAttributes()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <img id='img' width='100' height='80' style='width: 200px; height: 150px;'></body>");
            var img = LayoutTestHelper.FindById(root, "img");
            Assert.NotNull(img);
            _output.WriteLine($"img: {img!.ContentRect.Width}x{img.ContentRect.Height}");
            Assert.True(System.Math.Abs(img.ContentRect.Width - 200) < 2,
                $"CSS width overrides attribute (got {img.ContentRect.Width})");
        }

        [Fact]
        public void Hr_FullWidth()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='width: 300px;'>
                    <hr id='hr'>
                </div></body>");
            var hr = LayoutTestHelper.FindById(root, "hr");
            Assert.NotNull(hr);
            _output.WriteLine($"hr: w={hr!.ContentRect.Width} h={hr.ContentRect.Height}");
            // HR should fill container width
            Assert.True(hr.ContentRect.Width > 200, $"HR should fill width (got {hr.ContentRect.Width})");
        }

        [Fact]
        public void Br_ProducesLineBreak()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='container' style='width: 200px;'>text<br>more text</div></body>");
            var container = LayoutTestHelper.FindById(root, "container");
            Assert.NotNull(container);
            // br should produce a line break → container height > single line
            Assert.True(container!.ContentRect.Height > 20,
                $"br should create line break (container.h={container.ContentRect.Height})");
        }
    }
}
