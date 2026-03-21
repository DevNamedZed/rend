using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    /// <summary>
    /// Tests mirroring WPT css-multicol patterns.
    /// </summary>
    public class WptMulticolConformanceTests
    {
        private readonly ITestOutputHelper _output;
        public WptMulticolConformanceTests(ITestOutputHelper output) { _output = output; }

        // column-count:2 balances blocks evenly
        [Fact]
        public void ColumnCount2_Balanced()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='mc' style='column-count:2;column-gap:0;width:200px'>
                    <div style='height:40px'></div>
                    <div style='height:40px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "mc")!.ContentRect.Height - 40) < 2);
        }

        // column-count:3 balances 6 blocks
        [Fact]
        public void ColumnCount3_SixBlocks()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='mc' style='column-count:3;column-gap:0;width:300px'>
                    <div style='height:30px'></div><div style='height:30px'></div>
                    <div style='height:30px'></div><div style='height:30px'></div>
                    <div style='height:30px'></div><div style='height:30px'></div>
                </div></body>");
            // 6*30=180 in 3 cols → 60px each
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "mc")!.ContentRect.Height - 60) < 2);
        }

        // column-gap adds space between columns
        [Fact]
        public void ColumnGap_AddsSpace()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='mc' style='column-count:2;column-gap:20px;width:220px'>
                    <div style='height:60px'></div>
                    <div style='height:60px'></div>
                </div></body>");
            // Each column = (220-20)/2 = 100px wide, height = 60px
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "mc")!.ContentRect.Height - 60) < 2);
        }

        // column-span:all spans full width
        [Fact]
        public void ColumnSpan_All()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='column-count:2;width:200px'>
                    <div style='height:30px'></div>
                    <div id='span' style='column-span:all;height:20px'></div>
                    <div style='height:30px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "span")!.ContentRect.Width - 200) < 2);
        }

        // column-rule doesn't affect layout
        [Fact]
        public void ColumnRule_NoLayoutEffect()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='mc' style='column-count:2;column-rule:5px solid red;column-gap:20px;width:220px'>
                    <div style='height:40px'></div>
                    <div style='height:40px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "mc")!.ContentRect.Width - 220) < 2);
        }

        // column-width determines column count
        [Fact]
        public void ColumnWidth_DeterminesCount()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='mc' style='column-width:100px;width:350px'>
                    <div style='height:100px'></div>
                </div></body>");
            // 350/100 ≈ 3 columns
            var mc = LayoutTestHelper.FindById(r, "mc")!;
            _output.WriteLine($"mc: {mc.ContentRect.Width}x{mc.ContentRect.Height}");
        }

        // column-count with uneven content
        [Fact]
        public void ColumnCount2_UnevenContent()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='mc' style='column-count:2;column-gap:0;width:200px'>
                    <div style='height:30px'></div>
                    <div style='height:30px'></div>
                    <div style='height:30px'></div>
                </div></body>");
            // 3*30=90 in 2 cols → balanced ~45-60px
            Assert.True(LayoutTestHelper.FindById(r, "mc")!.ContentRect.Height <= 61);
        }

        // column-span:all between content segments
        [Fact]
        public void ColumnSpan_BetweenContent()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='mc' style='column-count:2;column-gap:0;width:200px'>
                    <div style='height:20px'></div>
                    <div style='height:20px'></div>
                    <div id='s' style='column-span:all;height:30px'></div>
                    <div style='height:20px'></div>
                    <div style='height:20px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "s")!.ContentRect.Width - 200) < 2);
        }

        // multicol auto height = balanced column height + spanners
        [Fact]
        public void AutoHeight_WithSpanner()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='mc' style='column-count:2;column-gap:0;width:200px'>
                    <div style='height:40px'></div>
                    <div style='height:40px'></div>
                    <div style='column-span:all;height:20px'></div>
                    <div style='height:40px'></div>
                    <div style='height:40px'></div>
                </div></body>");
            // pre-spanner: 80 in 2 cols = 40. Spanner = 20. Post-spanner: 80 in 2 cols = 40.
            // Total = 40+20+40 = 100.
            var mc = LayoutTestHelper.FindById(r, "mc")!;
            _output.WriteLine($"mc.h={mc.ContentRect.Height}");
            Assert.True(System.Math.Abs(mc.ContentRect.Height - 100) < 2);
        }

        // multicol with explicit height limits column height
        [Fact]
        public void ExplicitHeight_LimitsColumns()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='mc' style='column-count:2;column-gap:0;width:200px;height:50px'>
                    <div style='height:40px'></div>
                    <div style='height:40px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "mc")!.ContentRect.Height - 50) < 2);
        }

        // multicol inside flex container
        [Fact]
        public void Multicol_InsideFlex()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div id='mc' style='column-count:2;column-gap:0;flex:1'>
                        <div style='height:40px'></div>
                        <div style='height:40px'></div>
                    </div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "mc")!.ContentRect.Width - 300) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "mc")!.ContentRect.Height - 40) < 2);
        }

        // multicol inside grid item
        [Fact]
        public void Multicol_InsideGrid()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:300px;width:300px'>
                    <div id='mc' style='column-count:3;column-gap:0'>
                        <div style='height:30px'></div>
                        <div style='height:30px'></div>
                        <div style='height:30px'></div>
                    </div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "mc")!.ContentRect.Height - 30) < 2);
        }

        // column-count:1 = no column splitting
        [Fact]
        public void ColumnCount1_NoSplit()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='mc' style='column-count:1;width:200px'>
                    <div style='height:50px'></div>
                    <div style='height:50px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "mc")!.ContentRect.Height - 100) < 2);
        }
    }
}
