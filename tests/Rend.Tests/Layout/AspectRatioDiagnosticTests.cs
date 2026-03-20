using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class AspectRatioDiagnosticTests
    {
        private readonly ITestOutputHelper _output;
        public AspectRatioDiagnosticTests(ITestOutputHelper output) { _output = output; }

        [Fact]
        public void AspectRatio_BorderBox_WithPadding()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='d1' style='height: 50px; aspect-ratio: 2/1; box-sizing: border-box; padding-top: 25px;'></div>
                <div id='d2' style='height: 50px; aspect-ratio: 4/1; box-sizing: border-box; padding-top: 25px;'></div>
                </body>");

            var d1 = LayoutTestHelper.FindById(root, "d1");
            var d2 = LayoutTestHelper.FindById(root, "d2");
            Assert.NotNull(d1);
            Assert.NotNull(d2);

            float d1BorderWidth = d1!.ContentRect.Width + d1.PaddingLeft + d1.PaddingRight;
            float d1BorderHeight = d1.ContentRect.Height + d1.PaddingTop + d1.PaddingBottom;
            float d2BorderWidth = d2!.ContentRect.Width + d2.PaddingLeft + d2.PaddingRight;
            float d2BorderHeight = d2.ContentRect.Height + d2.PaddingTop + d2.PaddingBottom;

            _output.WriteLine($"d1 content: {d1.ContentRect.Width}x{d1.ContentRect.Height}");
            _output.WriteLine($"d1 padding: T={d1.PaddingTop} R={d1.PaddingRight} B={d1.PaddingBottom} L={d1.PaddingLeft}");
            _output.WriteLine($"d1 border-box: {d1BorderWidth}x{d1BorderHeight}");
            _output.WriteLine($"d2 content: {d2.ContentRect.Width}x{d2.ContentRect.Height}");
            _output.WriteLine($"d2 padding: T={d2.PaddingTop} R={d2.PaddingRight} B={d2.PaddingBottom} L={d2.PaddingLeft}");
            _output.WriteLine($"d2 border-box: {d2BorderWidth}x{d2BorderHeight}");

            // CSS Sizing 4: aspect-ratio with border-box applies ratio to border box
            // d1: border-box height = 50, ratio 2/1 → border-box width = 100
            Assert.True(System.Math.Abs(d1BorderWidth - 100) < 2,
                $"d1 border-box width should be 100 (got {d1BorderWidth})");

            // d2: border-box height = 50, ratio 4/1 → border-box width = 200
            // (but reference is 100px square... let me check)
        }

        [Fact]
        public void AspectRatio_ContentBox_WithPadding()
        {
            // Without border-box, ratio applies to content box
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='d1' style='height: 50px; aspect-ratio: 2/1; padding-top: 25px;'></div>
                </body>");

            var d1 = LayoutTestHelper.FindById(root, "d1");
            Assert.NotNull(d1);

            _output.WriteLine($"d1 content: {d1!.ContentRect.Width}x{d1.ContentRect.Height}");
            _output.WriteLine($"d1 padding-top: {d1.PaddingTop}");

            // content-box: height 50px (content only), padding-top separate
            // ratio 2/1 → content width = 50 * 2 = 100
            Assert.True(System.Math.Abs(d1.ContentRect.Width - 100) < 2,
                $"d1 content width should be 100 (got {d1.ContentRect.Width})");
            Assert.True(System.Math.Abs(d1.ContentRect.Height - 50) < 2,
                $"d1 content height should be 50 (got {d1.ContentRect.Height})");
        }

        [Fact]
        public void AspectRatio_AutoWidth_FromHeight()
        {
            // Basic: height given, auto width, aspect-ratio derives width
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='d1' style='height: 50px; aspect-ratio: 2/1;'></div>
                </body>");

            var d1 = LayoutTestHelper.FindById(root, "d1");
            Assert.NotNull(d1);

            _output.WriteLine($"d1: {d1!.ContentRect.Width}x{d1.ContentRect.Height}");

            // height 50, ratio 2/1 → width = 100
            Assert.True(System.Math.Abs(d1.ContentRect.Width - 100) < 2,
                $"Width should be 100 from ratio (got {d1.ContentRect.Width})");
        }

        [Fact]
        public void AspectRatio_MinWidth_Clamped()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='d1' style='height: 50px; aspect-ratio: 1/1; min-width: 100px;'></div>
                </body>");

            var d1 = LayoutTestHelper.FindById(root, "d1");
            Assert.NotNull(d1);

            _output.WriteLine($"d1: {d1!.ContentRect.Width}x{d1.ContentRect.Height}");

            // ratio 1/1 → width = 50, but min-width: 100 → width = 100
            Assert.True(d1.ContentRect.Width >= 99,
                $"Width should be >= 100 due to min-width (got {d1.ContentRect.Width})");
        }

        [Fact]
        public void AspectRatio_MaxWidth_Clamped()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='d1' style='height: 50px; aspect-ratio: 4/1; max-width: 100px;'></div>
                </body>");

            var d1 = LayoutTestHelper.FindById(root, "d1");
            Assert.NotNull(d1);

            _output.WriteLine($"d1: {d1!.ContentRect.Width}x{d1.ContentRect.Height}");

            // ratio 4/1 → width = 200, but max-width: 100 → width = 100
            Assert.True(d1.ContentRect.Width <= 101,
                $"Width should be <= 100 due to max-width (got {d1.ContentRect.Width})");
        }
    }
}
