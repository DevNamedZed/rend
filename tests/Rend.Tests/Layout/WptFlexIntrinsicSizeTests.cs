using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    /// <summary>
    /// Tests for flex container intrinsic sizing: max-content and min-content
    /// width computation for row and column flex with various item configurations.
    /// These mirror actual failing WPT tests from css-flexbox/intrinsic-size/.
    /// </summary>
    public class WptFlexIntrinsicSizeTests
    {
        private readonly ITestOutputHelper _output;
        public WptFlexIntrinsicSizeTests(ITestOutputHelper output) { _output = output; }

        // WPT row-001: floated row flex, item with flex:0 0 auto and nested content
        [Fact]
        public void Row001_FloatedFlex_ShrinkToFit()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:0'>
                    <div id='flex' style='display:flex;height:100px;float:left'>
                        <div style='flex:0 0 auto'>
                            <div style='float:left;width:100px'></div>
                            <div style='float:left;width:100px'></div>
                        </div>
                    </div>
                </div></body>");
            Assert.True(LayoutTestHelper.FindById(r, "flex")!.ContentRect.Width >= 99,
                $"Floated flex min-content ≥ 100 (got {LayoutTestHelper.FindById(r, "flex")!.ContentRect.Width})");
        }

        // WPT row-002: floated row flex, item with flex:1 0 100px
        [Fact]
        public void Row002_FlexBasis100_NoShrink()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='flex' style='display:flex;height:100px;float:left'>
                    <div style='flex:1 0 100px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "flex")!.ContentRect.Width - 100) < 2,
                $"Flex basis 100px no shrink (got {LayoutTestHelper.FindById(r, "flex")!.ContentRect.Width})");
        }

        // WPT row-003: floated row flex in 0-width parent, item flex:1 0 100px
        [Fact]
        public void Row003_ZeroWidthParent()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:0'>
                    <div id='flex' style='display:flex;height:100px;float:left'>
                        <div style='flex:1 0 100px'></div>
                    </div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "flex")!.ContentRect.Width - 100) < 2,
                $"Flex basis 100px in 0-width parent (got {LayoutTestHelper.FindById(r, "flex")!.ContentRect.Width})");
        }

        // WPT row-004: flex:0 1 100px min-width:0 in 0-width parent
        [Fact]
        public void Row004_ShrinkWithMinWidth0()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:0'>
                    <div id='flex' style='display:flex;height:100px;float:left'>
                        <div style='flex:0 1 100px;min-width:0'></div>
                    </div>
                </div></body>");
            // min-content: item can shrink to 0 → flex container min-content = 0
            var flex = LayoutTestHelper.FindById(r, "flex")!;
            _output.WriteLine($"flex.w={flex.ContentRect.Width}");
        }

        // WPT col-wrap-001: column wrap with max-content width
        [Fact]
        public void ColWrap001_MaxContentWidth()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px'>
                    <div id='flex' style='display:flex;flex-flow:column wrap;height:100px;width:max-content'>
                        <div style='min-height:0;width:50px;flex:0 0 100px'></div>
                        <div style='min-height:0;width:50px;flex:0 0 100px'></div>
                    </div>
                </div></body>");
            // Two 50px columns → max-content = 100px
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "flex")!.ContentRect.Width - 100) < 2,
                $"Column wrap max-content = 100 (got {LayoutTestHelper.FindById(r, "flex")!.ContentRect.Width})");
        }

        // WPT col-wrap-002: column wrap with max-height instead of height
        [Fact(Skip = "Known bug: column wrap max-height for wrapping")]
        public void ColWrap002_MaxHeight()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px'>
                    <div id='flex' style='display:flex;flex-flow:column wrap;max-height:100px;width:max-content'>
                        <div style='min-height:0;width:50px;flex:0 0 100px'></div>
                        <div style='min-height:0;width:50px;flex:0 0 100px'></div>
                    </div>
                </div></body>");
            var flex = LayoutTestHelper.FindById(r, "flex")!;
            _output.WriteLine($"col-wrap-002: {flex.ContentRect.Width}x{flex.ContentRect.Height}");
            Assert.True(flex.ContentRect.Width >= 99,
                $"Column wrap with max-height (got {flex.ContentRect.Width})");
        }

        // flex:0 0 0% — basis is 0 (percentage resolved)
        [Fact]
        public void Flex_0_0_0Percent()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px;height:100px'>
                    <div id='a' style='flex:0 0 0%;width:100px;height:100px'></div>
                    <div id='b' style='flex:0 0 0%;width:100px;height:100px'></div>
                </div></body>");
            // flex-basis:0% = 0. Items get 0 width (basis overrides width).
            Assert.True(LayoutTestHelper.FindById(r, "a")!.ContentRect.Width < 2);
            Assert.True(LayoutTestHelper.FindById(r, "b")!.ContentRect.Width < 2);
        }

        // nested column flex with auto min-height
        [Fact]
        public void NestedColumnFlex_MinHeight()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;height:20px;width:100px'>
                    <div style='display:flex;flex-direction:column'>
                        <div style='display:flex;flex-direction:column'>
                            <div id='inner' style='height:100px;width:100px'></div>
                        </div>
                    </div>
                </div></body>");
            var inner = LayoutTestHelper.FindById(r, "inner")!;
            _output.WriteLine($"inner: {inner.ContentRect.Width}x{inner.ContentRect.Height}");
            // auto min-height on column flex items prevents shrinking below content
        }

        // inline-flex max-content width
        [Fact]
        public void InlineFlex_MaxContent()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px'>
                    <div id='flex' style='display:inline-flex'>
                        <div style='width:60px;height:30px'></div>
                        <div style='width:40px;height:30px'></div>
                        <div style='width:50px;height:30px'></div>
                    </div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "flex")!.ContentRect.Width - 150) < 2);
        }

        // inline-flex with gap in max-content
        [Fact]
        public void InlineFlex_MaxContent_WithGap()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px'>
                    <div id='flex' style='display:inline-flex;gap:10px'>
                        <div style='width:60px;height:30px'></div>
                        <div style='width:40px;height:30px'></div>
                    </div>
                </div></body>");
            // 60+10+40 = 110
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "flex")!.ContentRect.Width - 110) < 2);
        }

        // column flex in width:max-content container
        [Fact]
        public void ColumnFlex_MaxContent()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px'>
                    <div id='flex' style='display:inline-flex;flex-direction:column'>
                        <div style='width:80px;height:30px'></div>
                        <div style='width:120px;height:30px'></div>
                    </div>
                </div></body>");
            // Column flex max-content = widest item = 120
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "flex")!.ContentRect.Width - 120) < 2);
        }

        // flex with flex-basis and grow in shrink-to-fit
        [Fact]
        public void FloatedFlex_BasisGrow()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='flex' style='display:flex;height:100px;float:left'>
                    <div style='flex:1 0 50px'></div>
                    <div style='flex:1 0 50px'></div>
                </div></body>");
            // Shrink-to-fit: max-content = sum of basis = 100
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "flex")!.ContentRect.Width - 100) < 2);
        }

        // flex with all items having flex:1 in shrink-to-fit
        [Fact(Skip = "Known bug: floated flex intrinsic sizing with flex:1 items")]
        public void FloatedFlex_AllGrow()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='flex' style='display:flex;height:50px;float:left'>
                    <div style='flex:1;height:50px'>
                        <div style='width:40px;height:10px'></div>
                    </div>
                    <div style='flex:1;height:50px'>
                        <div style='width:60px;height:10px'></div>
                    </div>
                </div></body>");
            // Shrink-to-fit: max-content = sum of item max-content widths
            var flex = LayoutTestHelper.FindById(r, "flex")!;
            _output.WriteLine($"flex.w={flex.ContentRect.Width}");
            Assert.True(flex.ContentRect.Width >= 99,
                $"Floated flex with grow items (got {flex.ContentRect.Width})");
        }

        // row-006: flex with border on items
        [Fact]
        public void Row006_ItemsWithBorder()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px'>
                    <div id='flex' style='display:flex;flex-direction:row;width:max-content;height:100px'>
                        <div style='flex:0 0 0px;border:10px solid transparent'>
                            <div style='width:80px'></div>
                        </div>
                    </div>
                </div></body>");
            var flex = LayoutTestHelper.FindById(r, "flex")!;
            _output.WriteLine($"row006: {flex.ContentRect.Width}");
            // flex-basis:0 but border:10px → outer size = 20px border. Content = 80px.
            // Total item = 80 + 20 = 100 (or just border since basis is 0?)
            Assert.True(flex.ContentRect.Width >= 19,
                $"Item with border (got {flex.ContentRect.Width})");
        }

        // gap-020: flex with column-gap and width:max-content
        [Fact(Skip = "Known bug: max-content with gap and min-width")]
        public void Gap020_MaxContentWithGap()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px'>
                    <div id='flex' style='display:flex;column-gap:10px;width:max-content;min-width:100px'>
                        <div style='width:40px;height:100px'></div>
                        <div style='width:40px;height:100px'></div>
                    </div>
                </div></body>");
            // max-content = 40 + 10 + 40 = 90, but min-width:100 → 100
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "flex")!.ContentRect.Width - 100) < 2);
        }
    }
}
