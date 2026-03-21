using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    /// <summary>
    /// Tests for flex item sizing: transferred sizes, aspect-ratio interactions,
    /// percentage sizing, and min/max constraints on flex items.
    /// </summary>
    public class WptFlexItemSizingTests
    {
        private readonly ITestOutputHelper _output;
        public WptFlexItemSizingTests(ITestOutputHelper output) { _output = output; }

        // flex item percentage width resolves against container
        [Fact]
        public void PercentWidth()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:400px'><div id='t' style='width:25%;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Width - 100) < 2);
        }

        // flex item percentage height in definite container
        [Fact]
        public void PercentHeight_Definite()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:200px;height:200px'><div id='t' style='width:50px;height:50%'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Height - 100) < 2);
        }

        // flex item with max-width clamps grow
        [Fact]
        public void MaxWidth_ClampsGrow()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:300px'><div id='a' style='flex:1;max-width:80px;height:30px'></div><div id='b' style='flex:1;height:30px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r, "a")!.ContentRect.Width <= 81);
            Assert.True(LayoutTestHelper.FindById(r, "b")!.ContentRect.Width >= 219);
        }

        // flex item with min-width prevents shrink
        [Fact]
        public void MinWidth_PreventsShrink()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:200px'><div id='t' style='flex:0 1 150px;min-width:120px;height:30px'></div><div style='flex:0 1 150px;height:30px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r, "t")!.ContentRect.Width >= 119);
        }

        // flex item with max-height in column flex
        [Fact]
        public void Column_MaxHeight()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-direction:column;height:300px;width:100px'><div id='t' style='flex:1;max-height:80px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r, "t")!.ContentRect.Height <= 81);
        }

        // flex item with min-height in column flex
        [Fact]
        public void Column_MinHeight()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-direction:column;height:300px;width:100px'><div id='t' style='flex:1;min-height:200px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r, "t")!.ContentRect.Height >= 199);
        }

        // flex item border-box with flex-basis
        [Fact]
        public void BorderBox_FlexBasis()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:300px'><div id='t' style='box-sizing:border-box;flex:0 0 150px;padding:20px;border:5px solid;height:50px'></div></div></body>");
            var t = LayoutTestHelper.FindById(r, "t")!;
            float total = t.ContentRect.Width + t.PaddingLeft + t.PaddingRight + t.BorderLeftWidth + t.BorderRightWidth;
            Assert.True(System.Math.Abs(total - 150) < 2);
        }

        // flex item with padding affects main size
        [Fact]
        public void Padding_AffectsMainSize()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:200px'><div id='a' style='flex:1;padding:10px;height:30px'></div><div id='b' style='flex:1;padding:10px;height:30px'></div></div></body>");
            // Total padding = 40. Remaining = 160. Each content = 80.
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "a")!.ContentRect.Width - 80) < 2);
        }

        // flex item with margin affects position
        [Fact]
        public void Margin_AffectsPosition()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:200px'><div id='a' style='width:50px;height:30px;margin-right:20px'></div><div id='b' style='width:50px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "b")!.ContentRect.X - 70) < 2);
        }

        // flex item flex-shrink:0 prevents shrinking
        [Fact]
        public void Shrink0_NoShrink()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:100px'><div id='t' style='flex-shrink:0;width:200px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Width - 200) < 2);
        }

        // flex item with calc() flex-basis
        [Fact]
        public void CalcFlexBasis()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:400px'><div id='t' style='flex:0 0 calc(50% - 20px);height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Width - 180) < 2);
        }

        // flex item with flex-basis percentage in column
        [Fact]
        public void Column_BasisPercent()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-direction:column;height:200px;width:100px'><div id='t' style='flex:0 0 50%'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Height - 100) < 2);
        }

        // flex items with different grow ratios and basis
        [Fact]
        public void GrowRatios_WithBasis()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:400px'><div id='a' style='flex:1 0 100px;height:30px'></div><div id='b' style='flex:3 0 100px;height:30px'></div></div></body>");
            // Free = 200. a gets +50, b gets +150.
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "a")!.ContentRect.Width - 150) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "b")!.ContentRect.Width - 250) < 2);
        }

        // flex items with different shrink ratios
        [Fact]
        public void ShrinkRatios_Weighted()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:100px'><div id='a' style='flex:0 1 100px;height:30px'></div><div id='b' style='flex:0 2 100px;height:30px'></div></div></body>");
            // Overflow=100. Scaled: a=1*100=100, b=2*100=200. Total=300.
            // a shrinks 100*(100/300)≈33→67. b shrinks 100*(200/300)≈67→33.
            var a = LayoutTestHelper.FindById(r, "a")!;
            var b = LayoutTestHelper.FindById(r, "b")!;
            Assert.True(a.ContentRect.Width > b.ContentRect.Width, $"a > b (a={a.ContentRect.Width}, b={b.ContentRect.Width})");
        }

        // flex: 0 0 auto uses width
        [Fact]
        public void Flex_0_0_Auto_UsesWidth()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:300px'><div id='t' style='flex:0 0 auto;width:120px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Width - 120) < 2);
        }

        // flex: 0 0 ignores width (basis=0)
        [Fact]
        public void Flex_0_0_BasisZero()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:300px'><div id='t' style='flex:0 0;width:120px;height:30px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r, "t")!.ContentRect.Width < 2);
        }

        // flex: none = 0 0 auto
        [Fact]
        public void Flex_None()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:300px'><div id='t' style='flex:none;width:80px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Width - 80) < 2);
        }

        // flex: auto = 1 1 auto
        [Fact]
        public void Flex_Auto()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:300px'><div id='t' style='flex:auto;width:80px;height:30px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r, "t")!.ContentRect.Width >= 299);
        }

        // flex: initial = 0 1 auto
        [Fact]
        public void Flex_Initial()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:300px'><div id='t' style='flex:initial;width:80px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Width - 80) < 2);
        }

        // negative flex-grow rejected
        [Fact]
        public void NegativeGrow_Rejected()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:300px'><div id='t' style='flex-grow:-1;width:80px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Width - 80) < 2);
        }

        // negative flex-shrink rejected
        [Fact]
        public void NegativeShrink_Rejected()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:100px'><div id='a' style='flex-shrink:-1;width:100px;height:30px'></div><div style='width:100px;height:30px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r, "a")!.ContentRect.Width < 100);
        }
    }
}
