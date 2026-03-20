using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class MultiColumnLayoutAssertionTests
    {
        private readonly ITestOutputHelper _output;
        public MultiColumnLayoutAssertionTests(ITestOutputHelper output) { _output = output; }

        [Fact]
        public void ColumnCount_DistributesBlocks()
        {
            // Multiple small blocks should distribute across columns
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='mc' style='column-count: 2; width: 200px;'>
                    <div style='height: 30px;'></div>
                    <div style='height: 30px;'></div>
                    <div style='height: 30px;'></div>
                    <div style='height: 30px;'></div>
                </div></body>");
            var mc = LayoutTestHelper.FindById(root, "mc");
            Assert.NotNull(mc);
            _output.WriteLine($"w={mc!.ContentRect.Width} h={mc.ContentRect.Height}");
            // 4 blocks of 30px = 120px total; in 2 columns → ~60px height
            Assert.True(mc.ContentRect.Height < 121,
                $"2 columns should be shorter than total content (got {mc.ContentRect.Height})");
        }

        [Fact]
        public void ColumnGap_AddsSeparation()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='mc' style='column-count: 2; column-gap: 20px; width: 220px;'>
                    <div style='height: 80px;'></div>
                </div></body>");
            var mc = LayoutTestHelper.FindById(root, "mc");
            Assert.NotNull(mc);
            _output.WriteLine($"w={mc!.ContentRect.Width} h={mc.ContentRect.Height}");
            // Each column = (220 - 20) / 2 = 100px wide
        }

        [Fact]
        public void ColumnWidth_DeterminesCount()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='mc' style='column-width: 100px; width: 350px;'>
                    <div style='height: 150px;'></div>
                </div></body>");
            var mc = LayoutTestHelper.FindById(root, "mc");
            Assert.NotNull(mc);
            _output.WriteLine($"w={mc!.ContentRect.Width} h={mc.ContentRect.Height}");
            // 350px / 100px columns ≈ 3 columns (with default gap)
        }

        [Fact]
        public void ColumnSpan_All_SpansAllColumns()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='mc' style='column-count: 2; width: 200px;'>
                    <div style='height: 40px;'></div>
                    <div id='spanner' style='column-span: all; height: 30px;'></div>
                    <div style='height: 40px;'></div>
                </div></body>");
            var mc = LayoutTestHelper.FindById(root, "mc");
            var spanner = LayoutTestHelper.FindById(root, "spanner");
            Assert.NotNull(mc);
            Assert.NotNull(spanner);
            _output.WriteLine($"mc h={mc!.ContentRect.Height}, spanner w={spanner!.ContentRect.Width}");
            // Spanner should be full width
            Assert.True(spanner.ContentRect.Width >= 199,
                $"column-span:all should be full width (got {spanner.ContentRect.Width})");
        }
    }
}
