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

        [Fact]
        public void Debug_Flex_AspectRatio()
        {
            // Flex item with width + aspect-ratio, align-items:flex-start
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;align-items:flex-start;width:300px'><div id='t' style='width:100px;aspect-ratio:2/1'></div></div></body>");
            var t = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(t);
            _output.WriteLine($"FLEX: width={t!.ContentRect.Width} height={t.ContentRect.Height}");
            // aspect-ratio:2/1 with width:100 should give height=50
            Assert.True(System.Math.Abs(t.ContentRect.Height - 50) < 2,
                $"Expected height 50 from aspect-ratio 2/1 with width 100 (got {t.ContentRect.Height})");
        }

        [Fact]
        public void Debug_Block_AspectRatio()
        {
            // Block with width + aspect-ratio
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='width:100px;aspect-ratio:2/1'></div></body>");
            var t = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(t);
            _output.WriteLine($"BLOCK: width={t!.ContentRect.Width} height={t.ContentRect.Height}");
            // Block: width:100, aspect-ratio:2/1 -> height should be 50
            // But height:auto=0, so ResolveHeight might not enter aspect-ratio path
            Assert.True(System.Math.Abs(t.ContentRect.Height - 50) < 2,
                $"Expected height 50 (got {t.ContentRect.Height})");
        }
        [Fact]
        public void Debug_CalcCalcPercent()
        {
            // Abspos with positioned ancestor → containing block = positioned div (300px)
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='cb' style='position:relative;width:400px;height:300px'>
                    <div id='t' style='position:absolute;top:0;left:0;width:100%;height:calc(calc(100%))'></div>
                </div></body>");
            var cb = LayoutTestHelper.FindById(root, "cb");
            var t = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(cb);
            Assert.NotNull(t);
            _output.WriteLine($"CB height={cb!.ContentRect.Height}, t height={t!.ContentRect.Height}");
            Assert.True(System.Math.Abs(t.ContentRect.Height - 300) < 2,
                $"Expected 300 (got {t.ContentRect.Height})");
        }

        [Fact]
        public void Debug_MaxCalcPercent()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='position:relative;width:400px;height:300px'>
                    <div id='t' style='position:absolute;top:0;left:0;width:100%;height:max(calc(100%))'></div>
                </div></body>");
            var t = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(t);
            _output.WriteLine($"max(calc(100%)) height={t!.ContentRect.Height}");
            Assert.True(System.Math.Abs(t.ContentRect.Height - 300) < 2,
                $"Expected 300 (got {t.ContentRect.Height})");
        }

        [Fact]
        public void Debug_Max20Args()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='position:relative;width:400px;height:300px'>
                    <div id='t' style='position:absolute;top:0;left:0;width:100%;height:max(5%,10%,15%,20%,25%,30%,35%,40%,45%,50%,55%,60%,65%,70%,75%,80%,85%,90%,95%,100%)'></div>
                </div></body>");
            var t = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(t);
            _output.WriteLine($"max(20 args) height={t!.ContentRect.Height}");
            Assert.True(System.Math.Abs(t.ContentRect.Height - 300) < 2,
                $"Expected 300 (got {t.ContentRect.Height})");
        }
        [Fact]
        public void GridItem_AspectRatio_Stretch()
        {
            // Grid: 100px row, 200px col. Child: aspect-ratio:1/1, align-self:stretch
            // Stretch gives height=100px, aspect-ratio gives width=100px (not 200px)
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-rows:100px;grid-template-columns:200px'>
                    <div id='t' style='aspect-ratio:1/1;align-self:stretch'></div>
                </div></body>");
            var t = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(t);
            _output.WriteLine($"grid stretch AR: {t!.ContentRect.Width}x{t.ContentRect.Height}");
            Assert.True(System.Math.Abs(t.ContentRect.Height - 100) < 2,
                $"Expected height 100 (got {t.ContentRect.Height})");
            Assert.True(System.Math.Abs(t.ContentRect.Width - 100) < 2,
                $"Expected width 100 from AR (got {t.ContentRect.Width})");
        }

        [Fact]
        public void FlexColumn_AspectRatio_MinWidth()
        {
            // Column flex, align-items:start, child: AR 1/1, min-width:100px
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;align-items:flex-start'>
                    <div id='t' style='aspect-ratio:1/1;flex:0 0 auto;min-height:0;min-width:100px'></div>
                </div></body>");
            var t = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(t);
            _output.WriteLine($"flex col AR min-w: {t!.ContentRect.Width}x{t.ContentRect.Height}");
            Assert.True(t.ContentRect.Width >= 99,
                $"Expected width >= 100 from min-width (got {t.ContentRect.Width})");
            Assert.True(System.Math.Abs(t.ContentRect.Height - t.ContentRect.Width) < 2,
                $"Expected square from AR (got {t.ContentRect.Width}x{t.ContentRect.Height})");
        }
        [Fact]
        public void ColumnFlex_MaxWidth_ConstrainsWidth()
        {
            // WPT: column-flex-child-with-max-width
            // Column flex item with align-self:start and max-width should cap width
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;width:200px;height:100px'>
                    <div id='item' style='align-self:flex-start;max-width:100px'>
                        <div style='width:150px;height:50px'></div>
                    </div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item");
            Assert.NotNull(item);
            _output.WriteLine($"item: {item!.ContentRect.Width}x{item.ContentRect.Height}");
            Assert.True(item.ContentRect.Width <= 101,
                $"max-width should cap at 100 (got {item.ContentRect.Width})");
        }

        // [CSS-FLEXBOX-1 §4.5] Automatic minimum size: when a flex item's main size is
        // auto and overflow is visible, min-size = min-content contribution. The inner
        // flex item's content is an img with width:500px,max-width:100%. During intrinsic
        // sizing the percentage max-width resolves to auto (CSS-SIZING-3 §5.2), so the
        // img's min-content contribution is 500px. Auto-min-size forces the flex item
        // to 500px, overflowing the 100px outer container. Chrome 116 confirms 500x500.
        [Fact]
        public void Flex_AutoMinSize_PercentMaxWidth_Img()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:100px'>
                    <div style='display:flex;flex:1 1 auto'>
                        <div id='img' style='max-width:100%;width:500px;height:500px'></div>
                    </div>
                </div></body>");
            var img = LayoutTestHelper.FindById(root, "img");
            Assert.NotNull(img);
            _output.WriteLine($"img: {img!.ContentRect.Width}x{img.ContentRect.Height}");
            Assert.True(System.Math.Abs(img.ContentRect.Width - 500) < 2,
                $"auto-min-size overflow: img should be 500px (got {img.ContentRect.Width})");
        }
    }
}
